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
        /// Optimizes the execution order of pattern group members by reordering inner-join RDFPattern blocks
        /// in ascending estimated cardinality, without altering the declared query representation.
        /// OPTIONAL, UNION, MINUS pairs, BIND, VALUES and PropertyPath members act as ordering barriers.
        /// </summary>
        internal static List<RDFPatternGroupMember> OptimizePatternOrder(
            List<RDFPatternGroupMember> members, RDFDataSource dataSource)
        {
            if (members.Count < 2)
                return members;

            List<RDFPatternGroupMember> result = new List<RDFPatternGroupMember>(members);

            //Walk the list to find and sort reorderable blocks
            int blockStart = -1;
            for (int i = 0; i <= result.Count; i++)
            {
                bool isReorderable = i < result.Count && IsPatternReorderableAt(result, i);
                if (isReorderable && blockStart == -1)
                    blockStart = i;
                else if (!isReorderable && blockStart != -1)
                {
                    int blockLength = i - blockStart;
                    if (blockLength > 1)
                        SortPatternBlock(result, blockStart, blockLength, dataSource);
                    blockStart = -1;
                }
            }

            return result;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Checks whether the pattern group member at the given position is eligible for reordering
        /// </summary>
        private static bool IsPatternReorderableAt(List<RDFPatternGroupMember> members, int i)
        {
            //Only plain inner-join RDFPattern instances are reorderable
            if (!(members[i] is RDFPattern pattern))
                return false;
            if (pattern.IsOptional || pattern.JoinAsUnion || pattern.JoinAsMinus)
                return false;

            //The element immediately after a UNION/MINUS leader is the second half of a tied pair: it cannot be moved
            if (i > 0 && members[i - 1] is RDFPattern prevPattern
                      && (prevPattern.JoinAsUnion || prevPattern.JoinAsMinus))
                return false;

            return true;
        }

        /// <summary>
        /// Sorts a reorderable block of patterns in-place by ascending estimated cardinality
        /// </summary>
        private static void SortPatternBlock(
            List<RDFPatternGroupMember> members, int start, int length, RDFDataSource dataSource)
        {
            List<RDFPatternGroupMember> block = members.GetRange(start, length);
            block.Sort((a, b) =>
                EstimatePatternCardinality((RDFPattern)a, dataSource)
                    .CompareTo(EstimatePatternCardinality((RDFPattern)b, dataSource)));
            for (int i = 0; i < length; i++)
                members[start + i] = block[i];
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