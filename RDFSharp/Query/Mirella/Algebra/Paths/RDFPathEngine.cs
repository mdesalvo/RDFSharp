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
    /// <summary>
    /// <para>
    /// RDFPathEngine evaluates SPARQL 1.1 property paths over a datasource. A property path is a recursive
    /// expression tree (<see cref="RDFPropertyPathExpression"/>) and every node denotes a BINARY RELATION over
    /// the datasource terms (the set of (start, end) pairs the sub-path connects). The engine computes that
    /// relation compositionally:
    /// <list type="bullet">
    /// <item><c>Link</c> — the one-hop adjacency of a predicate (with the memoized transitive closure for + / *);</item>
    /// <item><c>Sequence</c> — relational composition of its children (lazy frontier expansion);</item>
    /// <item><c>Alternative</c> — union of its children;</item>
    /// <item><c>NegatedPropertySet</c> — one hop over any predicate not in the set;</item>
    /// <item>cardinality (<c>? * +</c>) and inverse (<c>^</c>) decorate any node.</item>
    /// </list>
    /// A path NODE is an <see cref="RDFPatternMember"/>: a resource everywhere a hop can continue, but possibly a
    /// literal when it is the terminal object of the last hop (or the source of an inverse hop). Only resources
    /// carry outgoing edges, so literals naturally behave as sinks.
    /// </para>
    /// <para>
    /// LOOP SAFETY (the chief hazard of recursive path evaluation): the recursion is on the FINITE, ACYCLIC
    /// expression tree, so it always terminates regardless of the data. The only cycles are in the DATA, under
    /// <c>+</c>/<c>*</c>: there the sub-relation is first materialized into an EXPLICIT, FINITE adjacency map and
    /// the closure is always delegated to <see cref="RDFTransitiveClosureIndex"/> (Tarjan SCC → acyclic
    /// condensation), which is provably cycle-proof. The engine NEVER walks the data graph recursively for a
    /// closure.
    /// </para>
    /// </summary>
    internal static class RDFPathEngine
    {
        #region Methods
        /// <summary>
        /// Evaluates the given property path against the given datasource, returning the table of (Start, End)
        /// bindings (a column per variable endpoint; a blank existence row when both endpoints are concrete).
        /// </summary>
        internal static RDFTable ApplyPropertyPath(RDFPropertyPath propertyPath, RDFDataSource dataSource)
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

            #region Utilities
            //Emits one result row for the (s, e) pair, applying concrete-term filters and deduplication
            void AddBindingRow(RDFPatternMember s, RDFPatternMember e)
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

                //At least one variable: populate the corresponding columns
                if (startIsVar || endIsVar)
                {
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    if (startIsVar)
                        row[propertyPath.Start.ToString()] = s.ToString();
                    if (endIsVar)
                        row[propertyPath.End.ToString()]   = e.ToString();
                    resultTable.AddRow(row);
                } 
                //Both terms are concrete: emit a blank existence row (pattern matched)
                else
                {
                    resultTable.AddRow(new string[resultTable.ColumnsCount]);
                }
            }
            #endregion

            //Materialize the datasource adjacency once and reuse it (plus memoized closures) across all seeds
            RDFPathEvaluationCache evaluationCache = new RDFPathEvaluationCache(dataSource);
            RDFPropertyPathExpression rootExpression = propertyPath.Expression;

            //Fast path: concrete end + variable start on a single OneOrMore Link, e.g. "?s prop+ <end>".
            //The naive plan seeds from every node and keeps only those reaching <end>. But "x reaches end over
            //forward edges" is the same statement as "end reaches x over REVERSED edges": the set of valid
            //starts is precisely the forward closure of the relation read backwards, evaluated once from <end>.
            if (startIsVar && !endIsVar
                 && rootExpression.Kind == RDFQueryEnums.RDFPropertyPathExpressionKinds.Link
                 && rootExpression.Cardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore)
            {
                foreach (RDFPatternMember startNode in evaluationCache.GetClosureIndex(rootExpression.Property, !rootExpression.IsInverse)
                                                                      .EnumerateReachableNodes(endResource))
                {
                    AddBindingRow(startNode, endResource);
                }
                return resultTable;
            }

            //Determine the seed set: a concrete start yields a single seed; a variable start is pruned to the
            //domain of a single non-zero-length Link, otherwise every node is a candidate seed
            foreach (RDFPatternMember seed in GetSeeds(rootExpression, startIsVar, startResource, evaluationCache))
            {
                foreach (RDFPatternMember reached in EvaluateExpressionFromNode(seed, rootExpression, evaluationCache))
                    AddBindingRow(seed, reached);
            }

            return resultTable;
        }

        /// <summary>
        /// Computes the seed set for a property path evaluation. A concrete start yields a single seed. A variable
        /// start over a single Link that requires at least one hop (no zero-length match) is pruned to the nodes
        /// actually having an outgoing edge on the step property (its domain). In every other case a zero-length
        /// match may be possible, so all nodes in the datasource remain candidate seeds.
        /// </summary>
        private static IEnumerable<RDFPatternMember> GetSeeds(RDFPropertyPathExpression rootExpression, bool startIsVar, RDFResource startResource, RDFPathEvaluationCache evaluationCache)
        {
            //Concrete start: the single resource is the only seed
            if (!startIsVar)
                return new List<RDFPatternMember> { startResource };

            //Variable start over a single Link that cannot match zero-length (a plain or '+' hop): only the terms
            //with an outgoing edge on that property can begin a match, so seed from its domain instead of all nodes
            if (rootExpression.Kind == RDFQueryEnums.RDFPropertyPathExpressionKinds.Link
                 && (rootExpression.Cardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne
                      || rootExpression.Cardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore))
            {
                return evaluationCache.GetSources(rootExpression.Property, rootExpression.IsInverse);
            }

            //Anything else (a zero-length-capable '?'/'*', or a composite/negated root) may bind the start to any
            //term via a zero-length or backward match, so every node is a candidate seed
            return GetAllNodes(evaluationCache.DataSource);
        }

        /// <summary>
        /// Evaluates the sub-path <paramref name="expression"/> starting from <paramref name="node"/>, returning
        /// the set of reachable end nodes. This is the recursive heart of the engine: the recursion is on the
        /// finite expression tree (always terminating), while data cycles are handled only inside the memoized
        /// transitive closures.
        /// </summary>
        private static IEnumerable<RDFPatternMember> EvaluateExpressionFromNode(RDFPatternMember node, RDFPropertyPathExpression expression, RDFPathEvaluationCache evaluationCache)
        {
            switch (expression.Kind)
            {
                //A single predicate: its cardinality reuses the memoized per-property adjacency/closure directly
                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Link:
                    return EvaluateLinkFromNode(node, expression, evaluationCache);

                //Composite/negated nodes: lazy when undecorated, materialized when they carry cardinality/inverse
                default:
                    if (expression.Cardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne && !expression.IsInverse)
                        return EvaluateUndecoratedCompositeFromNode(node, expression, evaluationCache);
                    return EvaluateDecoratedCompositeFromNode(node, expression, evaluationCache);
            }
        }

        /// <summary>
        /// Evaluates a Link node (single predicate) from the given node, honoring its inverse direction and
        /// cardinality by reusing the memoized per-property adjacency and transitive closure.
        /// </summary>
        private static IEnumerable<RDFPatternMember> EvaluateLinkFromNode(RDFPatternMember node, RDFPropertyPathExpression linkExpression, RDFPathEvaluationCache evaluationCache)
        {
            //Direction of the hop: '^' reads the reverse adjacency (object→subject)
            bool inverse = linkExpression.IsInverse;
            switch (linkExpression.Cardinality)
            {
                //'?': the node itself (zero hops) plus its direct successors (at most one hop), deduplicated by hash
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne:
                {
                    Dictionary<long, RDFPatternMember> result = new Dictionary<long, RDFPatternMember> { [node.PatternMemberID] = node };
                    foreach (RDFPatternMember r in evaluationCache.GetDirectSuccessors(node, linkExpression.Property, inverse))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                //'+': one-or-more hops = the memoized transitive closure from the node (no zero-length match)
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore:
                    return evaluationCache.GetClosureIndex(linkExpression.Property, inverse).EnumerateReachableNodes(node);

                //'*': zero-or-more hops = the node itself (zero-length) unioned with its '+' closure
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore:
                {
                    Dictionary<long, RDFPatternMember> result = new Dictionary<long, RDFPatternMember> { [node.PatternMemberID] = node };
                    foreach (RDFPatternMember r in evaluationCache.GetClosureIndex(linkExpression.Property, inverse).EnumerateReachableNodes(node))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                //ExactlyOne: a single direct hop
                default:
                    return evaluationCache.GetDirectSuccessors(node, linkExpression.Property, inverse);
            }
        }

        /// <summary>
        /// Evaluates the BASE relation of a Sequence/Alternative/NegatedPropertySet node (its kind and children,
        /// ignoring the node's own cardinality and inverse decorations) lazily from the given node. Sequence
        /// composes children left-to-right (frontier expansion); Alternative unions them; NegatedPropertySet
        /// takes one hop over any non-excluded predicate.
        /// </summary>
        private static IEnumerable<RDFPatternMember> EvaluateUndecoratedCompositeFromNode(RDFPatternMember node, RDFPropertyPathExpression expression, RDFPathEvaluationCache evaluationCache)
        {
            switch (expression.Kind)
            {
                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence:
                {
                    //Compose children: each child rewrites the current frontier into the next one
                    List<RDFPatternMember> frontier = new List<RDFPatternMember> { node };
                    foreach (RDFPropertyPathExpression child in expression.Children)
                    {
                        Dictionary<long, RDFPatternMember> next = new Dictionary<long, RDFPatternMember>();
                        foreach (RDFPatternMember frontierNode in frontier)
                            foreach (RDFPatternMember reached in EvaluateExpressionFromNode(frontierNode, child, evaluationCache))
                                next[reached.PatternMemberID] = reached;
                        frontier = next.Values.ToList();
                    }
                    return frontier;
                }

                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative:
                {
                    //Union of the children evaluated in parallel from the same node
                    Dictionary<long, RDFPatternMember> union = new Dictionary<long, RDFPatternMember>();
                    foreach (RDFPropertyPathExpression child in expression.Children)
                        foreach (RDFPatternMember reached in EvaluateExpressionFromNode(node, child, evaluationCache))
                            union[reached.PatternMemberID] = reached;
                    return union.Values;
                }

                default: // NegatedPropertySet
                    return evaluationCache.GetNegatedSuccessors(node, expression.NegatedMembers);
            }
        }

        /// <summary>
        /// Evaluates a Sequence/Alternative/NegatedPropertySet node that carries a cardinality and/or an inverse
        /// decoration, from the given node. The base relation of the composite is materialized once into an
        /// explicit finite adjacency map (memoized), then the inverse is applied by reversing the map and the
        /// cardinality by delegating to the SCC transitive closure — never by recursive data traversal.
        /// </summary>
        private static IEnumerable<RDFPatternMember> EvaluateDecoratedCompositeFromNode(RDFPatternMember node, RDFPropertyPathExpression expression, RDFPathEvaluationCache evaluationCache)
        {
            //Materialize the (possibly reversed) base relation of the composite, memoized per node identity. The
            //node's cardinality is applied below ON TOP of this finite relation (never on the live data graph).
            RDFPathRelation relation = evaluationCache.GetCompositeRelation(expression);

            switch (expression.Cardinality)
            {
                //'?': the node itself plus its direct successors in the composite relation
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne:
                {
                    Dictionary<long, RDFPatternMember> result = new Dictionary<long, RDFPatternMember> { [node.PatternMemberID] = node };
                    foreach (RDFPatternMember r in relation.GetSuccessors(node))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                //'+': the SCC transitive closure of the composite relation from the node
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore:
                    return evaluationCache.GetCompositeClosure(expression).EnumerateReachableNodes(node);

                //'*': the node itself (zero-length) unioned with the '+' closure of the composite relation
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore:
                {
                    Dictionary<long, RDFPatternMember> result = new Dictionary<long, RDFPatternMember> { [node.PatternMemberID] = node };
                    foreach (RDFPatternMember r in evaluationCache.GetCompositeClosure(expression).EnumerateReachableNodes(node))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                //ExactlyOne: the composite is decorated only by inverse, so just one step over the (reversed) relation
                default:
                    return relation.GetSuccessors(node);
            }
        }

        /// <summary>
        /// Returns all distinct nodes (subjects, and resource-or-literal objects) present in the given datasource.<br/>
        /// Used to seed evaluation when the path start is a variable and zero-length matches are possible.
        /// </summary>
        private static List<RDFPatternMember> GetAllNodes(RDFDataSource dataSource)
        {
            HashSet<long> seen  = new HashSet<long>();
            List<RDFPatternMember> nodes = new List<RDFPatternMember>();

            #region Utilities
            //Add n to the list only the first time its hash is encountered (distinct, insertion-ordered)
            void CollectNode(RDFPatternMember n)
            {
                if (n != null && seen.Add(n.PatternMemberID))
                    nodes.Add(n);
            }
            #endregion

            //Every subject and every object of the datasource is a candidate term (objects include literals); a
            //federation is the union of its members' nodes
            switch (dataSource)
            {
                case RDFGraph graph:
                    foreach (RDFTriple triple in graph.SelectTriples())
                    {
                        CollectNode(triple.Subject);
                        CollectNode(triple.Object);
                    }
                    break;

                case RDFStore store:
                    foreach (RDFQuadruple quadruple in store.SelectQuadruples())
                    {
                        CollectNode(quadruple.Subject);
                        CollectNode(quadruple.Object);
                    }
                    break;

                case RDFFederation federation:
                    foreach (RDFDataSource federationMember in federation)
                    {
                        foreach (RDFPatternMember n in GetAllNodes(federationMember))
                            CollectNode(n);
                    }
                    break;
            }
            return nodes;
        }

        /// <summary>
        /// Shared empty node list returned for nodes with no outgoing edge on a step property.
        /// </summary>
        private static readonly List<RDFPatternMember> EmptyNodeList = new List<RDFPatternMember>(0);

        /// <summary>
        /// An explicit, finite binary relation over datasource terms: the (node hash → successors) adjacency map
        /// plus the registry mapping each hash back to its term. Produced by materializing a composite sub-path;
        /// consumed by the closure builder and by inverse reversal.
        /// </summary>
        private sealed class RDFPathRelation
        {
            /// <summary>
            /// The adjacency itself: each node hash mapped to the list of terms it relates to (its successors).
            /// </summary>
            internal Dictionary<long, List<RDFPatternMember>> Map { get; }

            /// <summary>
            /// The node registry resolving every hash appearing in the relation (source or successor) back to its
            /// term — needed because the maps are keyed by hash for speed.
            /// </summary>
            internal Dictionary<long, RDFPatternMember> Nodes { get; }

            internal RDFPathRelation(Dictionary<long, List<RDFPatternMember>> map, Dictionary<long, RDFPatternMember> nodes)
            {
                Map = map;
                Nodes = nodes;
            }

            //Successors of the node, or the shared empty list when it has no outgoing edge in this relation
            internal List<RDFPatternMember> GetSuccessors(RDFPatternMember node)
                => Map.TryGetValue(node.PatternMemberID, out List<RDFPatternMember> successors) ? successors : EmptyNodeList;

            /// <summary>
            /// Returns the reverse of this relation (edges flipped), preserving the node registry. Used to apply an
            /// inverse decoration ('^') to a composite sub-path.
            /// </summary>
            internal RDFPathRelation Reverse()
            {
                //Accumulate, for each former successor, the set of its former sources (deduplicated by hash)
                Dictionary<long, Dictionary<long, RDFPatternMember>> reversed = new Dictionary<long, Dictionary<long, RDFPatternMember>>();
                foreach (KeyValuePair<long, List<RDFPatternMember>> edges in Map)
                {
                    RDFPatternMember from = Nodes[edges.Key];
                    foreach (RDFPatternMember to in edges.Value)
                    {
                        if (!reversed.TryGetValue(to.PatternMemberID, out Dictionary<long, RDFPatternMember> preds))
                            reversed[to.PatternMemberID] = preds = new Dictionary<long, RDFPatternMember>();
                        preds[from.PatternMemberID] = from;
                    }
                }
                //Flatten the dedup dictionaries into plain successor lists, reusing the same node registry
                Dictionary<long, List<RDFPatternMember>> reversedMap = new Dictionary<long, List<RDFPatternMember>>(reversed.Count);
                foreach (KeyValuePair<long, Dictionary<long, RDFPatternMember>> kv in reversed)
                    reversedMap[kv.Key] = new List<RDFPatternMember>(kv.Value.Values);
                return new RDFPathRelation(reversedMap, Nodes);
            }
        }

        /// <summary>
        /// Holds, for the lifetime of a single property path evaluation, the in-memory adjacency maps of every
        /// step property (materialized once from the datasource), the lazily-built memoized transitive closures,
        /// the per-member-set negated adjacency, and the materialized relations/closures of composite sub-paths.
        /// </summary>
        private sealed class RDFPathEvaluationCache
        {
            /// <summary>
            /// The datasource being evaluated; the single source from which every adjacency is materialized.
            /// </summary>
            internal RDFDataSource DataSource { get; }

            /// <summary>
            /// Per-property one-hop adjacency, keyed by the predicate hash: built once on first use of each step
            /// property and reused (with its lazily-built closures) across every seed and frontier node.
            /// </summary>
            private readonly Dictionary<long, RDFPropertyAdjacency> byProperty = new Dictionary<long, RDFPropertyAdjacency>();

            /// <summary>
            /// Per-negated-set one-hop adjacency, keyed by a canonical signature of the member set (so two
            /// occurrences of the same <c>!(…)</c> share one scan of the datasource).
            /// </summary>
            private readonly Dictionary<string, RDFNegatedAdjacency> byNegatedSet = new Dictionary<string, RDFNegatedAdjacency>();

            /// <summary>
            /// The materialized finite relation of each composite sub-path (Sequence/Alternative/NegatedPropertySet
            /// carrying an inverse and/or cardinality), keyed by the expression node identity to memoize the sweep.
            /// </summary>
            private readonly Dictionary<RDFPropertyPathExpression, RDFPathRelation> compositeRelations = new Dictionary<RDFPropertyPathExpression, RDFPathRelation>();

            /// <summary>
            /// The transitive closure over each composite sub-path's relation (for <c>+</c>/<c>*</c> on a group),
            /// keyed by the expression node identity so the cycle-proof SCC closure is built only once.
            /// </summary>
            private readonly Dictionary<RDFPropertyPathExpression, RDFTransitiveClosureIndex> compositeClosures = new Dictionary<RDFPropertyPathExpression, RDFTransitiveClosureIndex>();

            internal RDFPathEvaluationCache(RDFDataSource dataSource)
                => DataSource = dataSource;

            #region Per-property adjacency / closure
            private RDFPropertyAdjacency GetPropertyAdjacency(RDFResource property)
            {
                //Build the property's adjacency on first request, then cache it so every later hop on the same
                //predicate (and every direction/closure derived from it) reuses a single datasource scan
                if (!byProperty.TryGetValue(property.PatternMemberID, out RDFPropertyAdjacency adjacency))
                {
                    adjacency = RDFPropertyAdjacency.BuildPropertyAdjaceny(property, DataSource);
                    byProperty[property.PatternMemberID] = adjacency;
                }
                return adjacency;
            }

            /// <summary>
            /// Returns the terms reachable from the given node in exactly one hop via the given property in the
            /// requested direction.
            /// </summary>
            internal List<RDFPatternMember> GetDirectSuccessors(RDFPatternMember node, RDFResource property, bool inverse)
            {
                //Pick the forward (subject→object) or reverse (object→subject) map for the requested direction
                Dictionary<long, List<RDFPatternMember>> successorsMap = inverse ? GetPropertyAdjacency(property).Reverse : GetPropertyAdjacency(property).Forward;
                //Look up the node's successors, returning the shared empty list when it has no outgoing edge
                return successorsMap.TryGetValue(node.PatternMemberID, out List<RDFPatternMember> successors) ? successors : EmptyNodeList;
            }

            /// <summary>
            /// Returns the domain of the step property in the requested direction: the terms that actually have at
            /// least one outgoing edge, used to prune seeds when zero-length matches are impossible.
            /// </summary>
            internal IEnumerable<RDFPatternMember> GetSources(RDFResource property, bool inverse)
            {
                RDFPropertyAdjacency adjacency = GetPropertyAdjacency(property);
                //The keys of the chosen direction's map are exactly the terms with at least one outgoing edge
                Dictionary<long, List<RDFPatternMember>> adjacencyMap = inverse ? adjacency.Reverse : adjacency.Forward;
                //Resolve each source hash back to its term via the node registry
                List<RDFPatternMember> sources = new List<RDFPatternMember>(adjacencyMap.Count);
                foreach (long key in adjacencyMap.Keys)
                    sources.Add(adjacency.Nodes[key]);
                return sources;
            }

            /// <summary>
            /// Returns the memoized transitive closure of the step property in the requested direction.
            /// </summary>
            internal RDFTransitiveClosureIndex GetClosureIndex(RDFResource property, bool inverse)
            {
                RDFPropertyAdjacency adjacency = GetPropertyAdjacency(property);
                //Build the closure for the requested direction lazily on first use and cache it on the adjacency,
                //so the (potentially expensive) SCC condensation is computed at most once per property+direction
                if (inverse)
                    return adjacency.ReverseClosure ?? (adjacency.ReverseClosure = RDFTransitiveClosureIndex.BuildTransitiveClosureIndex(adjacency.Reverse, adjacency.Nodes));
                return adjacency.ForwardClosure ?? (adjacency.ForwardClosure = RDFTransitiveClosureIndex.BuildTransitiveClosureIndex(adjacency.Forward, adjacency.Nodes));
            }
            #endregion

            #region Negated property set adjacency
            /// <summary>
            /// Returns the terms reachable from the given node in one hop over any predicate NOT among the negated
            /// members (a forward edge whose predicate is not in the forward-excluded set, or a reverse edge whose
            /// predicate is not in the inverse-excluded set), built once per member-set and memoized.
            /// </summary>
            internal List<RDFPatternMember> GetNegatedSuccessors(RDFPatternMember node, List<(RDFResource Property, bool IsInverse)> members)
            {
                //Canonical, order-independent signature of the member set (each member tagged '^' if inverse),
                //so two textually-different but equivalent negated sets share one built adjacency
                string key = string.Join("|", members.Select(m => (m.IsInverse ? "^" : "") + m.Property.PatternMemberID).OrderBy(s => s, StringComparer.Ordinal));
                //Build the negated adjacency on first request and cache it under that signature
                if (!byNegatedSet.TryGetValue(key, out RDFNegatedAdjacency adjacency))
                {
                    adjacency = RDFNegatedAdjacency.BuildNegatedAdjacency(members, DataSource);
                    byNegatedSet[key] = adjacency;
                }
                //Return the node's successors, or the shared empty list when it has none
                return adjacency.Forward.TryGetValue(node.PatternMemberID, out List<RDFPatternMember> successors) ? successors : EmptyNodeList;
            }
            #endregion

            #region Composite relation / closure
            /// <summary>
            /// Returns the materialized, finite relation of a composite sub-path (its kind and children, with the
            /// node's own inverse applied but NOT its cardinality), memoized per node identity. Materialization
            /// sweeps every node as a seed and evaluates the base relation from it.
            /// </summary>
            internal RDFPathRelation GetCompositeRelation(RDFPropertyPathExpression expression)
            {
                //Materialize the composite's relation once per expression node and cache it
                if (!compositeRelations.TryGetValue(expression, out RDFPathRelation relation))
                {
                    //First the kind+children base relation, then the node's own inverse (if any) by flipping it;
                    //the cardinality is NOT applied here (it is handled by GetCompositeClosure)
                    relation = BuildCompositeBaseRelation(expression);
                    if (expression.IsInverse)
                        relation = relation.Reverse();
                    compositeRelations[expression] = relation;
                }
                return relation;
            }

            /// <summary>
            /// Returns the memoized transitive closure over a composite sub-path's relation, delegating to the
            /// cycle-proof SCC closure builder.
            /// </summary>
            internal RDFTransitiveClosureIndex GetCompositeClosure(RDFPropertyPathExpression expression)
            {
                //Build the closure once per expression node and cache it
                if (!compositeClosures.TryGetValue(expression, out RDFTransitiveClosureIndex closure))
                {
                    //Feed the composite's already-materialized finite relation to the same cycle-proof SCC builder
                    //used for single properties — this is what makes (a/b)+ safe and fast on cyclic data
                    RDFPathRelation relation = GetCompositeRelation(expression);
                    closure = RDFTransitiveClosureIndex.BuildTransitiveClosureIndex(relation.Map, relation.Nodes);
                    compositeClosures[expression] = closure;
                }
                return closure;
            }

            //Materializes the base relation (kind + children, ignoring outer cardinality/inverse) of a composite
            //node by sweeping all nodes and evaluating the lazy base from each
            private RDFPathRelation BuildCompositeBaseRelation(RDFPropertyPathExpression expression)
            {
                //The relation being built: node-hash → successors, plus the registry resolving each hash to its term
                Dictionary<long, List<RDFPatternMember>> map = new Dictionary<long, List<RDFPatternMember>>();
                Dictionary<long, RDFPatternMember> nodes = new Dictionary<long, RDFPatternMember>();

                //Evaluate the composite's undecorated base from every node, recording the edges it produces. This
                //single sweep turns the recursive sub-path into an explicit finite map (the anti-loop invariant:
                //the closure later runs on THIS map, never on the live data graph).
                foreach (RDFPatternMember seed in GetAllNodes(DataSource))
                {
                    List<RDFPatternMember> successors = null;
                    foreach (RDFPatternMember reached in EvaluateUndecoratedCompositeFromNode(seed, expression, this))
                    {
                        //Lazily create the successor list only for seeds that actually reach something
                        (successors ?? (successors = new List<RDFPatternMember>())).Add(reached);
                        nodes[reached.PatternMemberID] = reached;
                    }
                    //Register the seed and its edges only when non-empty (sinks stay absent from the map)
                    if (successors != null)
                    {
                        nodes[seed.PatternMemberID] = seed;
                        map[seed.PatternMemberID] = successors;
                    }
                }
                return new RDFPathRelation(map, nodes);
            }
            #endregion
        }

        /// <summary>
        /// In-memory one-hop adjacency for a negated property set: a forward map (node → reachable terms) collecting
        /// every edge whose predicate is not excluded in the corresponding direction, keyed by node hash.
        /// </summary>
        private sealed class RDFNegatedAdjacency
        {
            internal Dictionary<long, List<RDFPatternMember>> Forward;

            /// <summary>
            /// Scans the datasource once and builds the one-hop adjacency over any predicate not among the negated
            /// members: a forward triple (s p o) contributes (s → o) when p is not a forward-excluded predicate,
            /// and contributes (o → s) when p is not an inverse-excluded predicate (the SPARQL negated set unions
            /// the forward and inverse directions).
            /// </summary>
            internal static RDFNegatedAdjacency BuildNegatedAdjacency(List<(RDFResource Property, bool IsInverse)> members, RDFDataSource dataSource)
            {
                //The predicates the negated set forbids in each direction (kept as their pattern-member hashes):
                //the plain members forbid the forward direction, the '^' members forbid the inverse direction.
                HashSet<long> forwardForbiddenPredicates = new HashSet<long>(members.Where(m => !m.IsInverse).Select(m => m.Property.PatternMemberID));
                HashSet<long> inverseForbiddenPredicates = new HashSet<long>(members.Where(m => m.IsInverse).Select(m => m.Property.PatternMemberID));

                //Per SPARQL 1.1 §18.4, the negated set is the UNION of a forward branch (present when the set has
                //forward members, or is empty) and an inverse branch (present only when the set has inverse '^'
                //members). A purely-inverse set (e.g. !^p) therefore does NOT traverse the forward direction.
                bool traversesForward = inverseForbiddenPredicates.Count == 0 || forwardForbiddenPredicates.Count > 0;
                bool traversesInverse = inverseForbiddenPredicates.Count > 0;

                Dictionary<long, Dictionary<long, RDFPatternMember>> forward = new Dictionary<long, Dictionary<long, RDFPatternMember>>();

                #region Utilities
                //Records a directed edge from→to in the negated-set forward map, deduplicating successors by hash
                void AddEdge(RDFPatternMember from, RDFPatternMember to)
                {
                    if (!forward.TryGetValue(from.PatternMemberID, out Dictionary<long, RDFPatternMember> outgoing))
                        forward[from.PatternMemberID] = outgoing = new Dictionary<long, RDFPatternMember>();
                    outgoing[to.PatternMemberID] = to;
                }

                //Decides which negated-set edges a single (subject, predicate, object) triple contributes: a
                //forward edge subject→object unless the predicate is a forward-excluded member, and/or a reverse
                //edge object→subject unless the predicate is an inverse-excluded ('^') member.
                void AddNegatedEdgesForTriple(RDFResource subject, RDFResource predicate, RDFPatternMember objectTerm)
                {
                    //Forward direction: predicate not among the forward-forbidden (non-inverse) members
                    if (traversesForward && !forwardForbiddenPredicates.Contains(predicate.PatternMemberID))
                        AddEdge(subject, objectTerm);
                    //Inverse direction: predicate not among the inverse-forbidden ('^') members
                    if (traversesInverse && !inverseForbiddenPredicates.Contains(predicate.PatternMemberID))
                        AddEdge(objectTerm, subject);
                }
                #endregion

                CollectAllEdges(dataSource, AddNegatedEdgesForTriple);

                Dictionary<long, List<RDFPatternMember>> flattened = new Dictionary<long, List<RDFPatternMember>>(forward.Count);
                foreach (KeyValuePair<long, Dictionary<long, RDFPatternMember>> kv in forward)
                    flattened[kv.Key] = new List<RDFPatternMember>(kv.Value.Values);
                return new RDFNegatedAdjacency { Forward = flattened };
            }

            //Walks the datasource (recursing over federation members) handing every (subject, predicate, object)
            //triple to the given edge-builder
            private static void CollectAllEdges(RDFDataSource dataSource, Action<RDFResource, RDFResource, RDFPatternMember> addEdgesForTriple)
            {
                switch (dataSource)
                {
                    case RDFGraph graph:
                        foreach (RDFTriple triple in graph.SelectTriples())
                            addEdgesForTriple((RDFResource)triple.Subject, (RDFResource)triple.Predicate, triple.Object);
                        break;

                    case RDFStore store:
                        foreach (RDFQuadruple quadruple in store.SelectQuadruples())
                            addEdgesForTriple((RDFResource)quadruple.Subject, (RDFResource)quadruple.Predicate, quadruple.Object);
                        break;

                    case RDFFederation federation:
                        foreach (RDFDataSource member in federation)
                            CollectAllEdges(member, addEdgesForTriple);
                        break;
                }
            }
        }

        /// <summary>
        /// In-memory adjacency of a single property: forward (subject → objects) and reverse (object → subjects)
        /// maps keyed by pattern member hash, the node registry mapping each hash back to its term, and the
        /// lazily-built transitive closures over either direction. Objects may be literals (terminal sinks).
        /// </summary>
        private sealed class RDFPropertyAdjacency
        {
            internal Dictionary<long, List<RDFPatternMember>> Forward;
            internal Dictionary<long, List<RDFPatternMember>> Reverse;
            internal Dictionary<long, RDFPatternMember> Nodes;
            internal RDFTransitiveClosureIndex ForwardClosure;
            internal RDFTransitiveClosureIndex ReverseClosure;

            /// <summary>
            /// Scans the datasource once for the triples/quadruples carrying the given property and builds the
            /// forward and reverse adjacency maps (with duplicate edges collapsed) plus the node registry.
            /// </summary>
            internal static RDFPropertyAdjacency BuildPropertyAdjaceny(RDFResource property, RDFDataSource dataSource)
            {
                Dictionary<long, Dictionary<long, RDFPatternMember>> forward = new Dictionary<long, Dictionary<long, RDFPatternMember>>();
                Dictionary<long, Dictionary<long, RDFPatternMember>> reverse = new Dictionary<long, Dictionary<long, RDFPatternMember>>();
                Dictionary<long, RDFPatternMember> nodes = new Dictionary<long, RDFPatternMember>();

                #region Utilities
                //Records the property edge subject→object in both the forward and reverse maps (deduplicating
                //successors by hash) and registers both terms in the node registry
                void AddEdge(RDFResource subj, RDFPatternMember obj)
                {
                    long subjHash = subj.PatternMemberID, objHash = obj.PatternMemberID;
                    nodes[subjHash] = subj;
                    nodes[objHash] = obj;

                    if (!forward.TryGetValue(subjHash, out Dictionary<long, RDFPatternMember> fOut))
                        forward[subjHash] = fOut = new Dictionary<long, RDFPatternMember>();
                    fOut[objHash] = obj;

                    if (!reverse.TryGetValue(objHash, out Dictionary<long, RDFPatternMember> rOut))
                        reverse[objHash] = rOut = new Dictionary<long, RDFPatternMember>();
                    rOut[subjHash] = subj;
                }
                #endregion

                CollectPropertyEdges(property, dataSource, AddEdge);

                return new RDFPropertyAdjacency
                {
                    Forward = FlattenSuccessorsList(forward),
                    Reverse = FlattenSuccessorsList(reverse),
                    Nodes = nodes
                };
            }

            //Walks the datasource (recursing over federation members) emitting every (subject, object) edge
            //carrying the given property; objects may be literals (terminal nodes of a path)
            private static void CollectPropertyEdges(RDFResource property, RDFDataSource dataSource, Action<RDFResource, RDFPatternMember> addEdge)
            {
                switch (dataSource)
                {
                    case RDFGraph graph:
                        foreach (RDFTriple triple in graph.SelectTriples(p: property))
                            addEdge((RDFResource)triple.Subject, triple.Object);
                        break;

                    case RDFStore store:
                        foreach (RDFQuadruple quadruple in store.SelectQuadruples(p: property))
                            addEdge((RDFResource)quadruple.Subject, quadruple.Object);
                        break;

                    case RDFFederation federation:
                        foreach (RDFDataSource member in federation)
                            CollectPropertyEdges(property, member, addEdge);
                        break;
                }
            }

            //Collapses the dedup dictionaries into plain successor lists
            private static Dictionary<long, List<RDFPatternMember>> FlattenSuccessorsList(Dictionary<long, Dictionary<long, RDFPatternMember>> source)
            {
                Dictionary<long, List<RDFPatternMember>> result = new Dictionary<long, List<RDFPatternMember>>(source.Count);
                foreach (KeyValuePair<long, Dictionary<long, RDFPatternMember>> kv in source)
                    result[kv.Key] = new List<RDFPatternMember>(kv.Value.Values);
                return result;
            }
        }

        /// <summary>
        /// Precomputed all-pairs transitive reachability over a single binary relation, used to answer "+"
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
        /// THE ALGORITHM. <see cref="BuildTransitiveClosureIndex"/> runs Tarjan's SCC algorithm (a single DFS, here made iterative to
        /// survive very deep chains without blowing the call stack) to label every node with its component, then
        /// builds the condensation edges and propagates reachability across them. Querying a node (<see cref="EnumerateReachableNodes"/>)
        /// is then just "emit my own component's members if it is cyclic, plus the members of every downstream
        /// component".
        /// </para>
        /// <para>
        /// THE "+" SEMANTICS. "+" requires at least one hop, so a node reaches ITSELF only if a non-trivial cycle
        /// brings it back: that happens exactly when its component is cyclic — either it has more than one member,
        /// or it is a single node carrying a self-loop edge (x knows x). A singleton component without a self-loop
        /// never reaches itself. The caller turns "+" into "*" simply by
        /// adding the start node itself (the zero-hop reflexive match).
        /// </para>
        /// </summary>
        private sealed class RDFTransitiveClosureIndex
        {
            //Component id assigned to each node hash (nodes in the same SCC share the same id)
            private readonly Dictionary<long, int> sccOf;
            //Terms belonging to each component, indexed by component id
            private readonly List<List<RDFPatternMember>> members;
            //For each component, the set of OTHER component ids reachable from it across the condensation DAG
            //(transitively closed; never contains the component itself)
            private readonly List<HashSet<int>> reachableComponents;
            //For each component, whether it reaches itself in >= 1 hop (i.e. it is cyclic: size > 1 or self-loop)
            private readonly bool[] selfReaching;

            private RDFTransitiveClosureIndex(Dictionary<long, int> sccOf, List<List<RDFPatternMember>> members, List<HashSet<int>> reachableComponents, bool[] selfReaching)
            {
                this.sccOf = sccOf;
                this.members = members;
                this.reachableComponents = reachableComponents;
                this.selfReaching = selfReaching;
            }

            /// <summary>
            /// Enumerates every term reachable from the given node in one or more hops, without duplicates.
            /// </summary>
            /// <remarks>
            /// The result is the union of two disjoint families of nodes, so no de-duplication is needed:
            /// (1) the node's own component, emitted only when that component is cyclic (the "reaches itself"
            /// case of "+"); and (2) the members of every component reachable downstream in the condensation.
            /// These are disjoint because a node belongs to exactly one component, and a component never appears
            /// among its own downstream reachable components (the condensation is acyclic).
            /// </remarks>
            internal IEnumerable<RDFPatternMember> EnumerateReachableNodes(RDFPatternMember node)
            {
                //A node outside the relation (no incident edge on this property/direction) reaches nothing
                if (node == null || !sccOf.TryGetValue(node.PatternMemberID, out int component))
                    yield break;

                //(1) A cyclic component reaches all of its own members, including the node itself
                if (selfReaching[component])
                {
                    foreach (RDFPatternMember member in members[component])
                        yield return member;
                }

                //(2) Plus every member of every downstream component (disjoint from the above, so no duplicates)
                foreach (int reachedComponent in reachableComponents[component])
                {
                    foreach (RDFPatternMember member in members[reachedComponent])
                        yield return member;
                }
            }

            /// <summary>
            /// Builds the closure index from the given adjacency map and node registry by (1) discovering the
            /// strongly-connected components with Tarjan's algorithm, then (2) propagating reachability across
            /// the resulting acyclic condensation.
            /// </summary>
            internal static RDFTransitiveClosureIndex BuildTransitiveClosureIndex(Dictionary<long, List<RDFPatternMember>> map, Dictionary<long, RDFPatternMember> nodes)
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
                        List<RDFPatternMember> neighbors = map.TryGetValue(v, out List<RDFPatternMember> nl) ? nl : null;

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

                //Map each component's node hashes back to their terms (kept for emission by EnumerateReachableNodes)
                List<List<RDFPatternMember>> members = new List<List<RDFPatternMember>>(componentCount);
                for (int c = 0; c < componentCount; c++)
                {
                    List<RDFPatternMember> memberTerms = new List<RDFPatternMember>(componentHashes[c].Count);
                    foreach (long h in componentHashes[c])
                        memberTerms.Add(nodes[h]);
                    members.Add(memberTerms);
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
                foreach (KeyValuePair<long, List<RDFPatternMember>> edges in map)
                {
                    int from = sccOf[edges.Key];
                    foreach (RDFPatternMember neighbor in edges.Value)
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
