/*
   Copyright 2012-2026 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    // RDFQueryEngine (MIRELLA): property path evaluation, including transitive (*, +, ?, {n,m})
    // acceleration via materialized adjacency and SCC-condensed transitive closure.
    internal partial class RDFQueryEngine
    {
        /// <summary>
        /// Applies the given property path to the given graph
        /// </summary>
        internal RDFTable ApplyPropertyPath(RDFPropertyPath propertyPath, RDFDataSource dataSource)
        {
            //Dispatch to transitive evaluation when any step carries a cardinality constraint
            if (propertyPath.HasTransitiveSteps)
                return ApplyTransitivePropertyPath(propertyPath, dataSource);

            //Translate property path into equivalent list of patterns
            List<RDFPattern> patternList = propertyPath.GetPatternList();
            List<RDFTable> patternTables = new List<RDFTable>(patternList.Count);

            //Evaluate produced list of patterns
            foreach (RDFPattern pattern in patternList)
            {
                //Apply pattern to graph
                RDFTable patternTable = ApplyPattern(pattern, dataSource);

                //Set join flags
                patternTable.IsOptional = pattern.IsOptional;
                patternTable.JoinAsUnion = pattern.JoinAsUnion;
                patternTable.JoinAsMinus = pattern.JoinAsMinus;

                //Add produced table
                patternTables.Add(patternTable);
            }

            //Merge produced list of tables
            RDFTable resultTable = CombineTables(patternTables);

            //Remove property path variables
            foreach (string ppColumn in (from RDFTableColumn ppCol
                                         in resultTable.Columns
                                         where ppCol.Name.StartsWith("?__PP", StringComparison.Ordinal)
                                         select ppCol.Name).ToArray())
            {
                resultTable.RemoveColumn(ppColumn);
            }

            return resultTable;
        }

        /// <summary>
        /// Applies a property path containing at least one transitive cardinality step (?, *, +, {min,max}).<br/>
        /// The datasource adjacency for every step property is materialized once into in-memory maps and the
        /// unbounded transitive closure (+, *) is memoized via strongly-connected-component condensation, so
        /// the closure is computed a single time and shared across all seeds and frontier nodes (instead of
        /// recomputing an independent BFS from each node). Seeds are pruned to the actual domain when the path
        /// cannot match a zero-length path, and a concrete end node is resolved by traversing the inverse
        /// closure from the end instead of seeding from every node in the datasource.
        /// </summary>
        internal RDFTable ApplyTransitivePropertyPath(RDFPropertyPath propertyPath, RDFDataSource dataSource)
        {
            RDFTable resultTable = new RDFTable();

            //Collect information about boundary terms of the property path
            bool startIsVar = propertyPath.Start is RDFVariable;
            bool endIsVar   = propertyPath.End   is RDFVariable;
            RDFResource startResource = propertyPath.Start as RDFResource;
            RDFResource endResource   = propertyPath.End   as RDFResource;

            //Add output columns only for the terms that are variables
            if (startIsVar)
                resultTable.AddColumn(propertyPath.Start.ToString());
            if (endIsVar)
                resultTable.AddColumn(propertyPath.End.ToString());

            //Track already-added (start, end) pairs to avoid duplicate rows
            HashSet<string> addedRows = new HashSet<string>();

            //Emits one result row for the (s, e) pair, applying concrete-term filters and deduplication
            void AddBindingRow(RDFResource s, RDFResource e)
            {
                //Skip if concrete start does not match the seed
                if (!startIsVar && !s.Equals(startResource))
                    return;
                //Skip if concrete end does not match the reached node
                if (!endIsVar && !e.Equals(endResource))
                    return;

                //Deduplicate by the variable portion of the key
                string key = (startIsVar ? s.ToString() : string.Empty) + "|" + (endIsVar ? e.ToString() : string.Empty);
                if (!addedRows.Add(key))
                    return;

                if (startIsVar || endIsVar)
                {
                    //At least one variable: populate the corresponding columns
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    if (startIsVar)
                        row[propertyPath.Start.ToString()] = s.ToString();
                    if (endIsVar)
                        row[propertyPath.End.ToString()]   = e.ToString();
                    resultTable.AddRow(row);
                }
                else
                {
                    //Both terms are concrete: emit a blank existence row (pattern matched)
                    resultTable.AddRow(new string[resultTable.ColumnsCount]);
                }
            }

            //Materialize the datasource adjacency once and reuse it (plus memoized closures) across all seeds
            RDFTransitivePathCache cache = new RDFTransitivePathCache(dataSource);

            //Fast path: concrete end + variable start on a single OneOrMore step, e.g. "?s prop+ <end>".
            //The naive plan seeds from every node and keeps only those reaching <end>. But "x reaches end over
            //forward edges" is the same statement as "end reaches x over REVERSED edges": the set of valid
            //starts is precisely the forward closure of the relation read backwards, evaluated once from <end>.
            //So we flip the traversal direction (!IsInverseStep) and emit one row per node the end can reach,
            //turning an O(V) seed sweep into a single closure lookup.
            if (startIsVar && !endIsVar
                 && propertyPath.Steps.Count == 1
                 && propertyPath.Steps[0].StepCardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore)
            {
                RDFPropertyPathStep onlyStep = propertyPath.Steps[0];
                foreach (RDFResource startNode in cache.GetClosure(onlyStep.StepProperty, !onlyStep.IsInverseStep).Reachable(endResource))
                    AddBindingRow(startNode, endResource);
                return resultTable;
            }

            //Determine the seed set, pruning it to the actual domain whenever the path cannot produce a
            //zero-length (identity) match; otherwise every resource node in the datasource is a candidate
            IEnumerable<RDFResource> seeds = GetTransitiveSeeds(propertyPath, startIsVar, startResource, cache);
            foreach (RDFResource seed in seeds)
            {
                foreach (RDFResource reached in EvaluateStepsFromNode(seed, propertyPath.Steps, cache))
                    AddBindingRow(seed, reached);
            }

            return resultTable;
        }

        /// <summary>
        /// Computes the seed set for a transitive property path evaluation.<br/>
        /// A concrete start yields a single seed. A variable start over a single step that requires at least
        /// one hop (+ or {min,max} with min &gt;= 1) is pruned to the nodes actually having an outgoing edge on
        /// the step property (its domain). In every other case (?, *, {0,n} or multi-step paths) a zero-length
        /// match is possible, so all resource nodes in the datasource remain candidate seeds.
        /// </summary>
        private IEnumerable<RDFResource> GetTransitiveSeeds(RDFPropertyPath propertyPath, bool startIsVar, RDFResource startResource, RDFTransitivePathCache cache)
        {
            if (!startIsVar)
                return new List<RDFResource> { startResource };

            if (propertyPath.Steps.Count == 1)
            {
                RDFPropertyPathStep step = propertyPath.Steps[0];
                bool requiresHop = step.StepCardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore
                                    || (step.StepCardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.BoundedRange && step.MinCardinality >= 1);
                if (requiresHop)
                    return cache.GetSources(step.StepProperty, step.IsInverseStep);
            }

            return GetAllResourceNodes(cache.DataSource);
        }

        /// <summary>
        /// Evaluates all path steps from the given start node, returning the set of reachable end nodes.<br/>
        /// This is the relational composition of the steps: <c>current</c> is the frontier (the set of nodes
        /// reached so far), and each step transforms it into the next frontier. Sequence-flavored steps compose
        /// (the output of one feeds the input of the next); a run of consecutive Alternative-flavored steps forms
        /// a single group whose branches are taken in parallel and unioned, modelling "a | b | c" within the path.
        /// </summary>
        private List<RDFResource> EvaluateStepsFromNode(RDFResource startNode, List<RDFPropertyPathStep> steps, RDFTransitivePathCache cache)
        {
            //The frontier starts as the single seed node and is rewritten group by group
            List<RDFResource> current = new List<RDFResource> { startNode };

            int i = 0;
            while (i < steps.Count)
            {
                //Start a new group with the current step; if it is Alternative, keep collecting
                //consecutive Alternative steps (they form a single union-branch of the path)
                List<RDFPropertyPathStep> group = new List<RDFPropertyPathStep> { steps[i] };
                if (steps[i].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                {
                    while (i + 1 < steps.Count && steps[i + 1].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                        group.Add(steps[++i]);
                }
                i++;

                //Expand every current frontier node through the group, deduplicating by pattern member hash
                Dictionary<long, RDFResource> next = new Dictionary<long, RDFResource>();
                foreach (RDFResource node in current)
                {
                    IEnumerable<RDFResource> reached = group.Count == 1 ? EvaluateSingleStepFromNode(node, group[0], cache)
                                                                        : group.SelectMany(step => EvaluateSingleStepFromNode(node, step, cache));
                    foreach (RDFResource r in reached)
                        next[r.PatternMemberID] = r;
                }
                current = next.Values.ToList();
            }

            return current;
        }

        /// <summary>
        /// Evaluates a single path step from the given node according to its cardinality. The optional/star
        /// variants are expressed in terms of the others: adding the node itself models the zero-hop (reflexive)
        /// match, so <c>?</c> = self ∪ one-hop and <c>*</c> = self ∪ <c>+</c>.<br/>
        /// - ExactlyOne   → one direct hop via the step property<br/>
        /// - ZeroOrOne    → node itself plus at most one direct hop (? operator)<br/>
        /// - OneOrMore    → the memoized transitive closure, one hop or more (+ operator)<br/>
        /// - ZeroOrMore   → node itself plus the memoized transitive closure (* operator)<br/>
        /// - BoundedRange → BFS keeping only nodes at depth [min, max], including node itself when min is 0
        /// </summary>
        private IEnumerable<RDFResource> EvaluateSingleStepFromNode(RDFResource node, RDFPropertyPathStep step, RDFTransitivePathCache cache)
        {
            switch (step.StepCardinality)
            {
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne:
                {
                    //Include the node itself (zero hops) and any direct successor (one hop)
                    Dictionary<long, RDFResource> result = new Dictionary<long, RDFResource> { [node.PatternMemberID] = node };
                    foreach (RDFResource r in GetDirectSuccessors(node, step.StepProperty, step.IsInverseStep, cache))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                case RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore:
                    //At least one hop: reuse the memoized transitive closure of the step property
                    return cache.GetClosure(step.StepProperty, step.IsInverseStep).Reachable(node);

                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore:
                {
                    //Include the node itself (zero hops) and all closure-reachable nodes
                    Dictionary<long, RDFResource> result = new Dictionary<long, RDFResource> { [node.PatternMemberID] = node };
                    foreach (RDFResource r in cache.GetClosure(step.StepProperty, step.IsInverseStep).Reachable(node))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                case RDFQueryEnums.RDFPropertyPathStepCardinalities.BoundedRange:
                {
                    Dictionary<long, RDFResource> result = new Dictionary<long, RDFResource>();
                    //When min is 0 the start node is a valid result (zero hops)
                    if (step.MinCardinality == 0)
                        result[node.PatternMemberID] = node;
                    foreach (RDFResource r in BFSReachable(node, step.StepProperty, step.IsInverseStep, cache, step.MinCardinality, step.MaxCardinality))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                default: // ExactlyOne
                    return GetDirectSuccessors(node, step.StepProperty, step.IsInverseStep, cache);
            }
        }

        /// <summary>
        /// Returns the resources reachable from the given node in exactly one hop via the given property,
        /// reading from the pre-materialized adjacency map held by <paramref name="cache"/>.<br/>
        /// When <paramref name="inverse"/> is true, traversal goes in the opposite direction (object → subject).
        /// </summary>
        private List<RDFResource> GetDirectSuccessors(RDFResource node, RDFResource property, bool inverse, RDFTransitivePathCache cache)
        {
            Dictionary<long, List<RDFResource>> map = cache.GetMap(property, inverse);
            return map.TryGetValue(node.PatternMemberID, out List<RDFResource> successors) ? successors : EmptyResourceList;
        }

        /// <summary>
        /// Returns all distinct resource nodes (subjects and resource-typed objects) present in the given datasource.<br/>
        /// Used to seed evaluation when the path start is a variable and zero-length matches are possible. Literal
        /// objects are excluded because they cannot appear as subjects in further hops.
        /// </summary>
        private static List<RDFResource> GetAllResourceNodes(RDFDataSource dataSource)
        {
            HashSet<long> seen  = new HashSet<long>();
            List<RDFResource> nodes = new List<RDFResource>();

            #region Utilities
            //Add r to the list only the first time its hash is encountered
            void CollectNode(RDFResource r)
            {
                if (r != null && seen.Add(r.PatternMemberID))
                    nodes.Add(r);
            }
            #endregion

            switch (dataSource)
            {
                case RDFGraph graph:
                    foreach (RDFTriple t in graph.SelectTriples())
                    {
                        if (t.Subject is RDFResource tSubj)
                            CollectNode(tSubj);
                        if (t.Object is RDFResource tObj)
                            CollectNode(tObj);
                    }
                    break;

                case RDFStore store:
                    foreach (RDFQuadruple q in store.SelectQuadruples())
                    {
                        if (q.Subject is RDFResource qSubj)
                            CollectNode(qSubj);
                        if (q.Object is RDFResource qObj)
                            CollectNode(qObj);
                    }
                    break;

                case RDFFederation federation:
                    foreach (RDFDataSource member in federation)
                    {
                        foreach (RDFResource r in GetAllResourceNodes(member))
                            CollectNode(r);
                    }
                    break;
            }
            return nodes;
        }

        /// <summary>
        /// Returns all resource nodes reachable from <paramref name="startNode"/> by traversing
        /// <paramref name="property"/> repeatedly via BFS over the pre-materialized adjacency map, collecting
        /// only nodes whose depth falls within [<paramref name="minHops"/>, <paramref name="maxHops"/>].
        /// Pass <paramref name="maxHops"/> = -1 for an unbounded search.
        /// Cycles are handled by the <c>enqueued</c> set, which prevents re-enqueuing already-seen nodes.
        /// </summary>
        private List<RDFResource> BFSReachable(RDFResource startNode, RDFResource property, bool inverse, RDFTransitivePathCache cache, int minHops, int maxHops)
        {
            List<RDFResource> result = new List<RDFResource>();

            //Initialize data structures for beginning of BFS visit
            HashSet<long> collected = new HashSet<long>();
            HashSet<long> enqueued = new HashSet<long> { startNode.PatternMemberID };
            Queue<(RDFResource node, int depth)> queue = new Queue<(RDFResource, int)>();
            queue.Enqueue((startNode, 0));

            while (queue.Count > 0)
            {
                (RDFResource current, int depth) = queue.Dequeue();

                //Do not expand beyond the maximum depth (avoids unnecessary work when bounded)
                if (maxHops >= 0 && depth >= maxHops)
                    continue;

                foreach (RDFResource neighbor in GetDirectSuccessors(current, property, inverse, cache))
                {
                    int newDepth = depth + 1;

                    //Collect this neighbor if it satisfies the minimum hop constraint and it has not
                    //been collected yet. Note: startNode itself can be a valid result when a cycle
                    //closes back to it (e.g. alice->bob->carol->alice with OneOrMore), so we do NOT
                    //pre-seed collected with startNode — only enqueued is pre-seeded to stop re-expansion.
                    if (newDepth >= minHops && collected.Add(neighbor.PatternMemberID))
                        result.Add(neighbor);

                    //Enqueue neighbors for further expansion only if not yet enqueued
                    //(prevents infinite loops and redundant work in cyclic graphs)
                    if (enqueued.Add(neighbor.PatternMemberID) && (maxHops < 0 || newDepth < maxHops))
                        queue.Enqueue((neighbor, newDepth));
                }
            }

            return result;
        }

        /// <summary>
        /// Shared empty resource list returned for nodes with no outgoing edge on a step property.
        /// </summary>
        private static readonly List<RDFResource> EmptyResourceList = new List<RDFResource>(0);

        #region Transitive property path acceleration
        /// <summary>
        /// Holds, for the lifetime of a single transitive property path evaluation, the in-memory adjacency
        /// maps of every step property (materialized once from the datasource) and the lazily-built, memoized
        /// transitive closures derived from them. Reused across all seeds and frontier nodes so that the
        /// datasource is scanned and each closure computed only once.
        /// </summary>
        private sealed class RDFTransitivePathCache
        {
            internal RDFDataSource DataSource { get; }
            private readonly Dictionary<long, RDFPropertyAdjacency> byProperty = new Dictionary<long, RDFPropertyAdjacency>();

            internal RDFTransitivePathCache(RDFDataSource dataSource)
                => DataSource = dataSource;

            private RDFPropertyAdjacency GetAdjacency(RDFResource property)
            {
                if (!byProperty.TryGetValue(property.PatternMemberID, out RDFPropertyAdjacency adjacency))
                {
                    adjacency = RDFPropertyAdjacency.Build(property, DataSource);
                    byProperty[property.PatternMemberID] = adjacency;
                }
                return adjacency;
            }

            /// <summary>
            /// Returns the (node hash → successors) adjacency map for the step property in the requested direction.
            /// </summary>
            internal Dictionary<long, List<RDFResource>> GetMap(RDFResource property, bool inverse)
            {
                RDFPropertyAdjacency adjacency = GetAdjacency(property);
                return inverse ? adjacency.Reverse : adjacency.Forward;
            }

            /// <summary>
            /// Returns the domain of the step property in the requested direction: the resources that actually
            /// have at least one outgoing edge, used to prune seeds when zero-length matches are impossible.
            /// </summary>
            internal IEnumerable<RDFResource> GetSources(RDFResource property, bool inverse)
            {
                RDFPropertyAdjacency adjacency = GetAdjacency(property);
                Dictionary<long, List<RDFResource>> map = inverse ? adjacency.Reverse : adjacency.Forward;
                List<RDFResource> sources = new List<RDFResource>(map.Count);
                foreach (long key in map.Keys)
                    sources.Add(adjacency.Nodes[key]);
                return sources;
            }

            /// <summary>
            /// Returns the memoized transitive closure of the step property in the requested direction,
            /// building it once on first use.
            /// </summary>
            internal RDFTransitiveClosureIndex GetClosure(RDFResource property, bool inverse)
            {
                RDFPropertyAdjacency adjacency = GetAdjacency(property);
                if (inverse)
                    return adjacency.ReverseClosure ?? (adjacency.ReverseClosure = RDFTransitiveClosureIndex.Build(adjacency.Reverse, adjacency.Nodes));
                return adjacency.ForwardClosure ?? (adjacency.ForwardClosure = RDFTransitiveClosureIndex.Build(adjacency.Forward, adjacency.Nodes));
            }
        }

        /// <summary>
        /// In-memory adjacency of a single property: forward (subject → resource objects) and reverse
        /// (object → subjects) maps keyed by pattern member hash, the node registry mapping each hash back to
        /// its resource, and the lazily-built transitive closures over either direction.
        /// </summary>
        private sealed class RDFPropertyAdjacency
        {
            internal Dictionary<long, List<RDFResource>> Forward;
            internal Dictionary<long, List<RDFResource>> Reverse;
            internal Dictionary<long, RDFResource> Nodes;
            internal RDFTransitiveClosureIndex ForwardClosure;
            internal RDFTransitiveClosureIndex ReverseClosure;

            /// <summary>
            /// Scans the datasource once for the triples/quadruples carrying the given property and builds the
            /// forward and reverse adjacency maps (with duplicate edges collapsed) plus the node registry.
            /// </summary>
            internal static RDFPropertyAdjacency Build(RDFResource property, RDFDataSource dataSource)
            {
                Dictionary<long, Dictionary<long, RDFResource>> forward = new Dictionary<long, Dictionary<long, RDFResource>>();
                Dictionary<long, Dictionary<long, RDFResource>> reverse = new Dictionary<long, Dictionary<long, RDFResource>>();
                Dictionary<long, RDFResource> nodes = new Dictionary<long, RDFResource>();

                void AddEdge(RDFResource subj, RDFResource obj)
                {
                    long subjHash = subj.PatternMemberID, objHash = obj.PatternMemberID;
                    nodes[subjHash] = subj;
                    nodes[objHash] = obj;

                    if (!forward.TryGetValue(subjHash, out Dictionary<long, RDFResource> fOut))
                        forward[subjHash] = fOut = new Dictionary<long, RDFResource>();
                    fOut[objHash] = obj;

                    if (!reverse.TryGetValue(objHash, out Dictionary<long, RDFResource> rOut))
                        reverse[objHash] = rOut = new Dictionary<long, RDFResource>();
                    rOut[subjHash] = subj;
                }
                CollectEdges(property, dataSource, AddEdge);

                return new RDFPropertyAdjacency
                {
                    Forward = Flatten(forward),
                    Reverse = Flatten(reverse),
                    Nodes = nodes
                };
            }

            //Walks the datasource (recursing over federation members) emitting every (subject, resource-object)
            //edge carrying the given property; literal objects are skipped as they cannot continue a path
            private static void CollectEdges(RDFResource property, RDFDataSource dataSource, Action<RDFResource, RDFResource> addEdge)
            {
                switch (dataSource)
                {
                    case RDFGraph graph:
                        foreach (RDFTriple t in graph.SelectTriples(p: property))
                            if (t.Object is RDFResource o)
                                addEdge((RDFResource)t.Subject, o);
                        break;

                    case RDFStore store:
                        foreach (RDFQuadruple q in store.SelectQuadruples(p: property))
                            if (q.Object is RDFResource o)
                                addEdge((RDFResource)q.Subject, o);
                        break;

                    case RDFFederation federation:
                        foreach (RDFDataSource member in federation)
                            CollectEdges(property, member, addEdge);
                        break;
                }
            }

            //Collapses the dedup dictionaries into plain successor lists
            private static Dictionary<long, List<RDFResource>> Flatten(Dictionary<long, Dictionary<long, RDFResource>> source)
            {
                Dictionary<long, List<RDFResource>> result = new Dictionary<long, List<RDFResource>>(source.Count);
                foreach (KeyValuePair<long, Dictionary<long, RDFResource>> kv in source)
                    result[kv.Key] = new List<RDFResource>(kv.Value.Values);
                return result;
            }
        }

        /// <summary>
        /// Precomputed all-pairs transitive reachability over a single property direction, used to answer "+"
        /// (and, with the start node added by the caller, "*") in amortized output time.
        /// <para>
        /// THE PROBLEM. A property path such as "?x knows+ ?y" asks, for every node x, the set of nodes reachable
        /// from x by following one or more "knows" edges — i.e. the transitive closure of the relation. Computing
        /// this with an independent breadth-first search per node costs O(V·(V+E)) and recomputes the same
        /// sub-closures over and over; on cyclic data it also has to carefully avoid looping forever.
        /// </para>
        /// <para>
        /// THE IDEA. Two nodes that sit on a common cycle reach exactly the same set of nodes (each can reach the
        /// other and therefore everything the other reaches). Such mutually-reachable groups are the graph's
        /// STRONGLY-CONNECTED COMPONENTS (SCCs): maximal sets where every node reaches every other node. If we
        /// collapse each SCC to a single super-node we obtain the CONDENSATION graph, which is always acyclic
        /// (a DAG) — any cycle would, by definition, have been swallowed into one SCC. On a DAG reachability is
        /// trivial to accumulate with one pass in topological order, and every node of an SCC inherits the very
        /// same reachable set, so the closure is computed once per component instead of once per node.
        /// </para>
        /// <para>
        /// THE ALGORITHM. <see cref="Build"/> runs Tarjan's SCC algorithm (a single DFS, here made iterative to
        /// survive very deep chains without blowing the call stack) to label every node with its component, then
        /// builds the condensation edges and propagates reachability across them. Querying a node (<see cref="Reachable"/>)
        /// is then just "emit my own component's members if it is cyclic, plus the members of every downstream
        /// component".
        /// </para>
        /// <para>
        /// THE "+" SEMANTICS. "+" requires at least one hop, so a node reaches ITSELF only if a non-trivial cycle
        /// brings it back: that happens exactly when its component is cyclic — either it has more than one member,
        /// or it is a single node carrying a self-loop edge (x knows x). A singleton component without a self-loop
        /// never reaches itself. This reproduces, set-for-set, the result of the previous per-seed BFS, including
        /// the tricky case of a cycle closing back onto the start node. The caller turns "+" into "*" simply by
        /// adding the start node itself (the zero-hop reflexive match).
        /// </para>
        /// </summary>
        private sealed class RDFTransitiveClosureIndex
        {
            //Component id assigned to each node hash (nodes in the same SCC share the same id)
            private readonly Dictionary<long, int> sccOf;
            //Resources belonging to each component, indexed by component id
            private readonly List<List<RDFResource>> members;
            //For each component, the set of OTHER component ids reachable from it across the condensation DAG
            //(transitively closed; never contains the component itself)
            private readonly List<HashSet<int>> reachableComponents;
            //For each component, whether it reaches itself in >= 1 hop (i.e. it is cyclic: size > 1 or self-loop)
            private readonly bool[] selfReaching;

            private RDFTransitiveClosureIndex(Dictionary<long, int> sccOf, List<List<RDFResource>> members, List<HashSet<int>> reachableComponents, bool[] selfReaching)
            {
                this.sccOf = sccOf;
                this.members = members;
                this.reachableComponents = reachableComponents;
                this.selfReaching = selfReaching;
            }

            /// <summary>
            /// Enumerates every resource reachable from the given node in one or more hops, without duplicates.
            /// </summary>
            /// <remarks>
            /// The result is the union of two disjoint families of nodes, so no de-duplication is needed:
            /// (1) the node's own component, emitted only when that component is cyclic (the "reaches itself"
            /// case of "+"); and (2) the members of every component reachable downstream in the condensation.
            /// These are disjoint because a node belongs to exactly one component, and a component never appears
            /// among its own downstream reachable components (the condensation is acyclic).
            /// </remarks>
            internal IEnumerable<RDFResource> Reachable(RDFResource node)
            {
                //A node outside the relation (no incident edge on this property/direction) reaches nothing
                if (node == null || !sccOf.TryGetValue(node.PatternMemberID, out int component))
                    yield break;

                //(1) A cyclic component reaches all of its own members, including the node itself
                if (selfReaching[component])
                    foreach (RDFResource member in members[component])
                        yield return member;

                //(2) Plus every member of every downstream component (disjoint from the above, so no duplicates)
                foreach (int reachedComponent in reachableComponents[component])
                    foreach (RDFResource member in members[reachedComponent])
                        yield return member;
            }

            /// <summary>
            /// Builds the closure index from the given adjacency map and node registry by (1) discovering the
            /// strongly-connected components with Tarjan's algorithm, then (2) propagating reachability across
            /// the resulting acyclic condensation.
            /// </summary>
            internal static RDFTransitiveClosureIndex Build(Dictionary<long, List<RDFResource>> map, Dictionary<long, RDFResource> nodes)
            {
                // ───────────────────────────────────────────────────────────────────────────────────────────
                // PHASE 1 — Tarjan's strongly-connected-components algorithm.
                //
                // Tarjan runs a single depth-first search and assigns to each node two numbers:
                //   index[v]   = the order in which v was first visited (its DFS discovery time);
                //   lowLink[v] = the smallest index reachable from v's DFS subtree while staying on the search
                //                stack — intuitively, the oldest node v can "climb back" to via a back/cross edge.
                // Nodes are pushed onto a separate `tarjanStack` as they are discovered and stay there until
                // their whole component is finalized. The key invariant is: v is the ROOT of an SCC exactly when
                // lowLink[v] == index[v] — meaning nothing in v's subtree can reach an older node, so v together
                // with everything still above it on the stack forms one maximal mutually-reachable set.
                //
                // We need this as an EXPLICIT-STACK iteration rather than recursion: chains in real data can be
                // tens of thousands of edges deep, which would overflow the runtime call stack. Each work-stack
                // frame is (node v, pos) where `pos` is the index of the next neighbor of v still to examine, so
                // popping a frame resumes v's neighbor loop exactly where a recursive call would have continued.
                // ───────────────────────────────────────────────────────────────────────────────────────────
                Dictionary<long, int> index = new Dictionary<long, int>();
                Dictionary<long, int> lowLink = new Dictionary<long, int>();
                HashSet<long> onStack = new HashSet<long>();
                Stack<long> tarjanStack = new Stack<long>();
                Dictionary<long, int> sccOf = new Dictionary<long, int>();
                List<List<long>> componentHashes = new List<List<long>>();
                int nextIndex = 0;

                Stack<(long node, int pos)> work = new Stack<(long, int)>();
                foreach (long start in nodes.Keys)
                {
                    //Every node must be a DFS root once; skip the ones already reached by a previous DFS tree
                    if (index.ContainsKey(start))
                        continue;
                    work.Push((start, 0));
                    while (work.Count > 0)
                    {
                        (long v, int pos) = work.Pop();
                        List<RDFResource> neighbors = map.TryGetValue(v, out List<RDFResource> nl) ? nl : null;

                        if (pos == 0)
                        {
                            //First time we enter v: assign its discovery index, seed its low-link to itself,
                            //and place it on the Tarjan stack as a candidate SCC member
                            index[v] = nextIndex;
                            lowLink[v] = nextIndex;
                            nextIndex++;
                            tarjanStack.Push(v);
                            onStack.Add(v);
                        }
                        else
                        {
                            //We are resuming v right after fully exploring its (pos-1)-th neighbor (the child the
                            //recursive version would have just returned from). Propagate that child's low-link up:
                            //if the child could climb to an older node, so can v.
                            long childHash = neighbors[pos - 1].PatternMemberID;
                            if (lowLink[childHash] < lowLink[v])
                                lowLink[v] = lowLink[childHash];
                        }

                        //Scan v's remaining neighbors looking for the next one to descend into
                        bool recursed = false;
                        if (neighbors != null)
                        {
                            for (int n = pos; n < neighbors.Count; n++)
                            {
                                long w = neighbors[n].PatternMemberID;
                                if (!index.ContainsKey(w))
                                {
                                    //Unvisited: emulate the recursive call. Re-push v to resume AFTER w (n+1),
                                    //then push w to be explored next; break so w is processed before we continue v.
                                    work.Push((v, n + 1));
                                    work.Push((w, 0));
                                    recursed = true;
                                    break;
                                }
                                //Already visited and still on the stack ⇒ this is a back/cross edge inside the
                                //current component: v can climb back to w's (older) discovery index
                                if (onStack.Contains(w) && index[w] < lowLink[v])
                                    lowLink[v] = index[w];
                            }
                        }
                        if (recursed)
                            continue;

                        //All of v's neighbors are explored. If v never found a way back to an older node, it is
                        //the root of an SCC: everything pushed onto the Tarjan stack at or above v forms it.
                        if (lowLink[v] == index[v])
                        {
                            List<long> component = new List<long>();
                            long popped;
                            do
                            {
                                popped = tarjanStack.Pop();
                                onStack.Remove(popped);
                                //Component ids are handed out as components are finalized; see the reverse
                                //topological-order note exploited in PHASE 2 below
                                sccOf[popped] = componentHashes.Count;
                                component.Add(popped);
                            }
                            while (popped != v);
                            componentHashes.Add(component);
                        }
                    }
                }

                // ───────────────────────────────────────────────────────────────────────────────────────────
                // PHASE 2 — Condensation and reachability propagation.
                //
                // With every node labelled by component, we now treat each component as a single super-node and
                // accumulate, per component, the set of OTHER components reachable from it. Because the
                // condensation is a DAG this is a one-pass dynamic program, and the crucial enabler is the order
                // in which Tarjan emits components: it finalizes a component only AFTER finalizing everything
                // reachable from it, so component ids come out in REVERSE TOPOLOGICAL ORDER. Hence for any
                // condensation edge from→to we are guaranteed `to < from`, and processing components by ascending
                // id means every successor's reachable set is already complete when we read it.
                // ───────────────────────────────────────────────────────────────────────────────────────────

                int componentCount = componentHashes.Count;

                //Map each component's node hashes back to their resources (kept for emission by Reachable)
                List<List<RDFResource>> members = new List<List<RDFResource>>(componentCount);
                for (int c = 0; c < componentCount; c++)
                {
                    List<RDFResource> memberResources = new List<RDFResource>(componentHashes[c].Count);
                    foreach (long h in componentHashes[c])
                        memberResources.Add(nodes[h]);
                    members.Add(memberResources);
                }

                //Classify each component as cyclic-or-not and collect the condensation's edges.
                //A component is cyclic (reaches itself in >= 1 hop) when it has more than one member; the
                //remaining cyclic case — a singleton with a self-loop edge — is detected while scanning edges.
                bool[] selfReaching = new bool[componentCount];
                List<HashSet<int>> condensationSucc = new List<HashSet<int>>(componentCount);
                for (int c = 0; c < componentCount; c++)
                {
                    selfReaching[c] = members[c].Count > 1;
                    condensationSucc.Add(new HashSet<int>());
                }
                foreach (KeyValuePair<long, List<RDFResource>> edges in map)
                {
                    int from = sccOf[edges.Key];
                    foreach (RDFResource neighbor in edges.Value)
                    {
                        int to = sccOf[neighbor.PatternMemberID];
                        if (from == to)
                        {
                            //An edge internal to a component. It only carries new information when it is a
                            //genuine self-loop (x→x) on a singleton: that makes the lone node reach itself.
                            if (edges.Key == neighbor.PatternMemberID)
                                selfReaching[from] = true;
                        }
                        else
                            //An edge between distinct components becomes an edge of the condensation DAG
                            condensationSucc[from].Add(to);
                    }
                }

                //Reachability DP over the DAG: reachable(c) = ⋃ over each successor s of { s } ∪ reachable(s).
                //Ascending id order is a valid evaluation order thanks to the reverse-topological numbering above.
                List<HashSet<int>> reachableComponents = new List<HashSet<int>>(componentCount);
                for (int c = 0; c < componentCount; c++)
                    reachableComponents.Add(null);
                for (int c = 0; c < componentCount; c++)
                {
                    HashSet<int> reached = new HashSet<int>();
                    foreach (int succ in condensationSucc[c])
                    {
                        reached.Add(succ);
                        reached.UnionWith(reachableComponents[succ]); //already finalized since succ < c
                    }
                    reachableComponents[c] = reached;
                }

                return new RDFTransitiveClosureIndex(sccOf, members, reachableComponents, selfReaching);
            }
        }
        #endregion
    }
}
