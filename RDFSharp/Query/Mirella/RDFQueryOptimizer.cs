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
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryOptimizer is responsible for reordering pattern group members before execution
    /// in order to minimize intermediate result sizes without altering query semantics.
    /// </summary>
    internal static class RDFQueryOptimizer
    {
        #region Methods
        /// <summary>
        /// Optimizes the execution order of pattern group members by reordering each block of inner-join
        /// with a greedy join-graph heuristic that minimizes the size of the intermediate
        /// results, without altering the declared query representation. Since inner-join is commutative and
        /// associative, this changes only the execution order and never the produced result set.
        /// OPTIONAL, UNION, MINUS pairs, BIND, VALUES and PropertyPath patternGroupMembers act as ordering barriers.
        /// </summary>
        internal static List<RDFPatternGroupMember> OptimizePatternGroup(
            List<RDFPatternGroupMember> patternGroupMembers, RDFDataSource dataSource)
        {
            //Nothing to reorder with fewer than two members: return the input as-is
            if (patternGroupMembers.Count < 2)
                return patternGroupMembers;

            //Work on a defensive copy so the caller's list (the query's declared representation) is never mutated
            List<RDFPatternGroupMember> result = new List<RDFPatternGroupMember>(patternGroupMembers);

            //Single linear scan that carves the member list into maximal runs ("blocks") of consecutive
            //reorderable inner-join patterns, reordering each block in isolation. Everything that is NOT
            //reorderable (OPTIONAL/UNION/MINUS/BIND/VALUES/PropertyPath/EXISTS) is a BARRIER: it keeps its
            //declared position, which is what preserves the query semantics across blocks.
            //
            //'blockStart' is the index where the current run began, or -1 when we are outside a run. We loop up
            //to result.Count INCLUSIVE so that the final iteration (i == Count) acts as a virtual barrier that
            //flushes a run reaching the end of the list.
            int blockStart = -1;
            for (int i = 0; i <= result.Count; i++)
            {
                bool isReorderable = i < result.Count && IsPatternGroupReorderableAt(result, i);
                switch (isReorderable)
                {
                    //Entering a new run: remember where it starts
                    case true when blockStart == -1:
                        blockStart = i;
                        break;
                    //Leaving a run at a barrier (or the virtual end-of-list): flush it
                    case false when blockStart != -1:
                    {
                        //The run spans [blockStart, i)
                        int blockLength = i - blockStart;
                        //A run of a single pattern has nothing to reorder; only reorder runs of 2+ patterns
                        if (blockLength > 1)
                            ReorderPatternGroupByJoinGraph(result, blockStart, blockLength, dataSource);
                        //Close the run and look for the next one
                        blockStart = -1;
                        break;
                    }
                }
            }

            return result;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Checks whether the pattern group member at the given position is eligible for reordering
        /// </summary>
        private static bool IsPatternGroupReorderableAt(List<RDFPatternGroupMember> patternGroupMembers, int i)
        {
            //Only plain inner-join RDFPattern instances are reorderable
            if (!(patternGroupMembers[i] is RDFPattern pattern))
                return false;
            if (pattern.IsOptional)
                return false;

            return true;
        }

        /// <summary>
        /// Reorders, in place, a block of inner-join patterns using a greedy join-graph heuristic that keeps
        /// the intermediate results small. It grows a "join frontier": it starts from the most selective
        /// pattern, then repeatedly appends the pattern that is CONNECTED to the variables bound so far (to
        /// avoid cartesian products) and that should grow the intermediate the least - preferring patterns
        /// that introduce fewer NEW variables (they behave as filters) and, on a tie, the more selective one.
        /// Disconnected patterns are deferred; when one is unavoidable, the most selective starts a new branch.
        /// </summary>
        private static void ReorderPatternGroupByJoinGraph(
            List<RDFPatternGroupMember> patternGroupMembers, int start, int length, RDFDataSource dataSource)
        {
            //Snapshot the block's patterns and pre-compute every pattern's static cardinality just once
            //(these are cheap index-count lookups based only on the pattern's bound, non-variable terms)
            List<RDFPattern> remainingPatterns = new List<RDFPattern>(length);
            Dictionary<RDFPattern, long> staticCardinalityByPattern = new Dictionary<RDFPattern, long>(length);
            for (int offset = 0; offset < length; offset++)
            {
                RDFPattern pattern = (RDFPattern)patternGroupMembers[start + offset];
                remainingPatterns.Add(pattern);
                staticCardinalityByPattern[pattern] = EstimatePatternCardinality(pattern, dataSource);
            }

            //Names of the variables already produced by the patterns chosen so far (the current join frontier)
            HashSet<string> boundVariableNames = new HashSet<string>(StringComparer.Ordinal);

            //Place the patterns back into the member list one at a time, in the chosen execution order
            for (int placedCount = 0; placedCount < length; placedCount++)
            {
                int nextPatternIndex = SelectNextPattern(remainingPatterns, staticCardinalityByPattern, boundVariableNames);
                RDFPattern chosenPattern = remainingPatterns[nextPatternIndex];
                remainingPatterns.RemoveAt(nextPatternIndex);

                patternGroupMembers[start + placedCount] = chosenPattern;

                //Extend the frontier with the variables this pattern binds
                foreach (RDFVariable patternVariable in chosenPattern.Variables)
                    boundVariableNames.Add(patternVariable.ToString());
            }
        }

        /// <summary>
        /// Selects, among the still-unplaced patterns, the index of the next pattern to execute. A pattern is
        /// "connected" when it shares at least one variable with the already-bound frontier; connected patterns
        /// are always preferred over disconnected ones (which would force a cartesian product). Within the same
        /// connectivity, the ranking is: fewer NEW variables first (a pattern that binds nothing new is a pure
        /// filter that shrinks the intermediate), then lower static cardinality, then earliest declared
        /// position (so the result is deterministic and stable). The very first pick (empty frontier) has no
        /// connectivity to exploit yet, so it is ranked purely by cardinality (most selective seed).
        /// </summary>
        private static int SelectNextPattern(
            List<RDFPattern> remainingPatterns, Dictionary<RDFPattern, long> staticCardinalityByPattern, HashSet<string> boundVariableNames)
        {
            //The first pick of a block has an empty frontier: there is no connectivity to exploit yet, so the
            //seed must be chosen by raw selectivity alone (see the per-candidate neutralization below)
            bool isSeedPick = boundVariableNames.Count == 0;

            //Running "best so far" descriptor. We keep the three ranking keys of the current best candidate
            //(connectivity, number of new variables, static cardinality) plus its index, and replace it only on
            //a STRICT improvement. 'bestInitialized' distinguishes "no candidate seen yet" from a real best,
            //and the sentinel maxima guarantee the very first candidate always wins the initial comparison.
            int bestIndex = 0;
            bool bestIsConnected = false;
            int bestNewVariableCount = int.MaxValue;
            long bestCardinality = long.MaxValue;
            bool bestInitialized = false;

            for (int i = 0; i < remainingPatterns.Count; i++)
            {
                RDFPattern candidatePattern = remainingPatterns[i];

                //Classify the candidate against the current frontier by walking its variables once:
                // - "connected" becomes true as soon as ANY variable is already bound (so this pattern joins
                //   onto the partial result instead of multiplying it as a cartesian product);
                // - "newVariableCount" counts the variables it would ADD to the frontier (the fewer, the less
                //   the intermediate can grow; zero means the pattern is a pure filter over already-bound vars).
                bool isConnected = false;
                int newVariableCount = 0;
                foreach (RDFVariable candidateVariable in candidatePattern.Variables)
                {
                    if (boundVariableNames.Contains(candidateVariable.ToString()))
                        isConnected = true;
                    else
                        newVariableCount++;
                }

                //A fully-ground pattern (no variables) is a pure existence check that never grows the intermediate,
                //so treat it as connected so it runs as early as possible
                if (candidatePattern.Variables.Count == 0)
                    isConnected = true;

                //At seed time nothing is bound yet, so connectivity (all false) and new-variable counts (all equal
                //to the arity) carry no useful signal and would merely bias toward low-arity patterns. Neutralize
                //both keys so the comparison below collapses to "lowest cardinality wins" => most selective seed.
                if (isSeedPick)
                {
                    isConnected = true;
                    newVariableCount = 0;
                }

                long candidateCardinality = staticCardinalityByPattern[candidatePattern];

                //Decide whether this candidate beats the current best using a LEXICOGRAPHIC comparison of the
                //three keys, in priority order: (1) connected before disconnected, (2) fewer new variables,
                //(3) lower cardinality. We compare key-by-key and stop at the first key that differs.
                //Crucially the test is STRICT ('<' / a true flag): when all keys are equal the candidate does
                //NOT win, so the earliest-encountered (i.e. earliest declared) pattern is kept => stable order.
                bool isBetter;
                if (!bestInitialized)
                    isBetter = true;                                    //first candidate seen: it is the best by default
                else if (isConnected != bestIsConnected)
                    isBetter = isConnected;                             //(1) prefer the connected one
                else if (newVariableCount != bestNewVariableCount)
                    isBetter = newVariableCount < bestNewVariableCount; //(2) prefer fewer new variables
                else
                    isBetter = candidateCardinality < bestCardinality;  //(3) prefer the more selective one

                if (isBetter)
                {
                    //Promote this candidate to "best so far", caching its keys for the next comparisons
                    bestInitialized = true;
                    bestIndex = i;
                    bestIsConnected = isConnected;
                    bestNewVariableCount = newVariableCount;
                    bestCardinality = candidateCardinality;
                }
            }

            return bestIndex;
        }

        /// <summary>
        /// Estimates the cardinality of the given pattern against the given data source
        /// by computing the minimum count among its bound index lookups
        /// </summary>
        private static long EstimatePatternCardinality(RDFPattern pattern, RDFDataSource dataSource)
        {
            switch (dataSource)
            {
                case RDFGraph graph:
                    return EstimatePatternCardinalityOnGraph(pattern, graph);
                case RDFMemoryStore store:
                    return EstimatePatternCardinalityOnStore(pattern, store);
                default:
                    return pattern.Variables.Count;
            }
        }

        /// <summary>
        /// Estimates the cardinality of the given pattern on the given graph
        /// by taking the minimum count among its bound index lookups
        /// </summary>
        private static long EstimatePatternCardinalityOnGraph(RDFPattern pattern, RDFGraph graph)
        {
            long estimate = graph.TriplesCount;

            if (pattern.Subject is RDFResource subj)
            {
                long c = graph.Index.LookupIndexBySubject(subj).Count;
                if (c < estimate) estimate = c;
            }

            if (pattern.Predicate is RDFResource pred)
            {
                long c = graph.Index.LookupIndexByPredicate(pred).Count;
                if (c < estimate) estimate = c;
            }

            if (pattern.Object is RDFResource obj)
            {
                long c = graph.Index.LookupIndexByObject(obj).Count;
                if (c < estimate) estimate = c;
            }
            else if (pattern.Object is RDFLiteral lit)
            {
                long c = graph.Index.LookupIndexByLiteral(lit).Count;
                if (c < estimate) estimate = c;
            }

            return estimate;
        }

        /// <summary>
        /// Estimates the cardinality of the given pattern on the given store
        /// by taking the minimum count among its bound index lookups
        /// </summary>
        private static long EstimatePatternCardinalityOnStore(RDFPattern pattern, RDFMemoryStore store)
        {
            long estimate = store.QuadruplesCount;

            if (pattern.Context is RDFContext ctx)
            {
                long c = store.Index.LookupIndexByContext(ctx).Count;
                if (c < estimate) estimate = c;
            }

            if (pattern.Subject is RDFResource subj)
            {
                long c = store.Index.LookupIndexBySubject(subj).Count;
                if (c < estimate) estimate = c;
            }

            if (pattern.Predicate is RDFResource pred)
            {
                long c = store.Index.LookupIndexByPredicate(pred).Count;
                if (c < estimate) estimate = c;
            }

            if (pattern.Object is RDFResource obj)
            {
                long c = store.Index.LookupIndexByObject(obj).Count;
                if (c < estimate) estimate = c;
            }
            else if (pattern.Object is RDFLiteral lit)
            {
                long c = store.Index.LookupIndexByLiteral(lit).Count;
                if (c < estimate) estimate = c;
            }

            return estimate;
        }
        #endregion
    }
}