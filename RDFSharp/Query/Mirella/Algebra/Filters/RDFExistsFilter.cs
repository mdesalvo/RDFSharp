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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFExistsFilter represents a filter for checking existence of a given group graph pattern.
    /// </summary>
    public class RDFExistsFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Group graph pattern to be evaluated on the RDF data source. As per SPARQL grammar
        /// (<c>GroupGraphPattern ::= '{' ( SubSelect | GroupGraphPatternSub ) '}'</c>) it is either a
        /// <see cref="RDFSelectQuery"/> (SubSelect) or a <see cref="RDFPatternGroup"/> (GroupGraphPatternSub).
        /// </summary>
        public RDFQueryMember GroupGraphPattern { get; internal set; }

        /// <summary>
        /// Results of the group graph pattern evaluation on the RDF data source
        /// </summary>
        internal RDFTable PatternResults { get; set; }

        /// <summary>
        /// The resultset for which <see cref="PatternResultsIndex"/> was last built: lets the index be reused
        /// across all the rows of one filter application and rebuilt when a fresh evaluation assigns a new table.
        /// </summary>
        private RDFTable IndexedTable { get; set; }

        /// <summary>
        /// Per variable column: maps each value's PatternMemberID to the set of PatternResults row indexes
        /// holding that value (columnName -> (valueMemberID -> resultRowIndexes)).
        /// </summary>
        private Dictionary<string, Dictionary<long, HashSet<int>>> PatternResultsIndex { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an Exists filter on the given SubSelect group graph pattern
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExistsFilter(RDFSelectQuery subSelect)
        {
            #region Guards
            if (subSelect == null)
                throw new RDFQueryException("Cannot create RDFExistsFilter because given \"subSelect\" parameter is null.");
            #endregion

            //Embedding a SELECT as an EXISTS body makes it a nested pattern (no prologue of its own), exactly like
            //attaching it through AddSubQuery: flag it so the printer renders it brace-wrapped without prefixes
            subSelect.IsSubQuery = true;
            GroupGraphPattern = subSelect;
            IsEvaluable = true;
        }

        /// <summary>
        /// Builds an Exists filter on the given pattern group graph pattern
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFExistsFilter(RDFPatternGroup patternGroup)
        {
            #region Guards
            if (patternGroup == null)
                throw new RDFQueryException("Cannot create RDFExistsFilter because given \"patternGroup\" parameter is null.");
            #endregion

            GroupGraphPattern = patternGroup;
            IsEvaluable = true;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);

        //Prefix-aware overload: renders the inner group graph pattern using the given namespaces (NOT EXISTS overrides this)
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat("FILTER ( EXISTS ", RDFQueryPrinter.PrintGroupGraphPattern(GroupGraphPattern, prefixes), " )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow.
        /// EXISTS keeps a row when the group graph pattern produced at least one result compatible with that row;
        /// "compatible" means agreeing on every variable shared between the pattern results and the row.
        /// Instead of rescanning (and reparsing) the whole pattern resultset for each candidate row,
        /// we consult a hash index built once per resultset, turning each check into O(1) lookups.
        /// </summary>
        internal override bool ApplyFilter(RDFTableRow row, bool applyNegation)
        {
            //EXISTS defaults to "fail": the row survives only if we can positively prove a compatible result.
            //A group graph pattern that produced no solution at all can never satisfy EXISTS for any row.
            bool keepRow = false;

            #region Evaluation
            if (PatternResults?.Rows.Count > 0)
            {
                //Variable columns of the pattern results that also occur in the candidate row: these are the only
                //dimensions on which the row and the pattern results must agree (the shared columns)
                List<string> sharedVariables = GetSharedVariables(row);

                #region Disjoint Evaluation
                //No shared variable => pattern and row are disjoint, so (since the pattern has at least one result)
                //every pattern result is compatible with the row
                if (sharedVariables.Count == 0)
                    keepRow = true;
                #endregion

                #region Non-Disjoint Evaluation
                //Row is kept iff at least one pattern result agrees with the bound shared cells of the row
                else
                {
                    //Ensure the hash index is ready (built once, then reused for every row of this application)
                    BuildPatternResultsIndex();

                    //We start from "every pattern result is a candidate" and progressively narrow the set by
                    //intersecting, for each BOUND shared cell, the pattern-result rows carrying that value.
                    //An UNBOUND shared cell behaves as a wildcard, so it adds no constraint and is skipped.
                    HashSet<int> compatibleResultRowIndexes = null;
                    bool hasBoundSharedCell = false;
                    foreach (string sharedVariable in sharedVariables)
                    {
                        //UNBOUND shared cell => wildcard: it constrains nothing, so leave the candidate set as-is
                        if (row.IsUnbound(sharedVariable))
                            continue;
                        hasBoundSharedCell = true;

                        //Resolve the row's value for this column to its canonical member identity, then fetch the
                        //pattern-result rows holding the same identity. No bucket => no compatible result at all.
                        long rowValueID = RDFQueryUtilities.ParseRDFPatternMember(row[sharedVariable] ?? string.Empty).PatternMemberID;
                        if (!PatternResultsIndex[sharedVariable].TryGetValue(rowValueID, out HashSet<int> resultRowsWithSameValue))
                            break;

                        //First bound column => seed the candidate set (copy, so we never mutate the index buckets);
                        //subsequent columns => keep only the results that also match this value (logical AND)
                        if (compatibleResultRowIndexes == null)
                            compatibleResultRowIndexes = new HashSet<int>(resultRowsWithSameValue);
                        else
                            compatibleResultRowIndexes.IntersectWith(resultRowsWithSameValue);

                        //Once the running intersection becomes empty no result can ever satisfy all columns
                        if (compatibleResultRowIndexes.Count == 0)
                            break;
                    }

                    //When every shared cell was UNBOUND there is no constraint, so all results match; otherwise the
                    //surviving intersection must contain at least one result (null/empty => incompatibility => drop)
                    if (!hasBoundSharedCell || compatibleResultRowIndexes?.Count > 0)
                        keepRow = true;
                }
                #endregion
            }
            #endregion

            //NOT EXISTS reuses this method passing applyNegation=true to invert the EXISTS outcome
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }

        /// <summary>
        /// Returns the variable columns present both in the pattern results and in the given row: these are the
        /// columns on which EXISTS correlates the two sides (independent of the pattern's concrete shape).
        /// </summary>
        private List<string> GetSharedVariables(RDFTableRow row)
        {
            List<string> sharedVariables = new List<string>(PatternResults.ColumnsCount);

            //Every column of the pattern results is a query variable; keep the ones also present in the row
            foreach (RDFTableColumn resultColumn in PatternResults.Columns)
            {
                if (row.HasColumn(resultColumn.Name))
                    sharedVariables.Add(resultColumn.Name);
            }
            return sharedVariables;
        }

        /// <summary>
        /// Lazily builds (once per pattern resultset) the hash index used by ApplyFilter. For every variable
        /// column of the pattern results, it maps each distinct value (by canonical member identity) to the set
        /// of pattern-result row indexes carrying that value. The index is keyed by the resultset's reference
        /// identity, so a fresh evaluation (which assigns a new PatternResults table) triggers a rebuild.
        /// </summary>
        private void BuildPatternResultsIndex()
        {
            //The index is still valid as long as it was built for the very same resultset instance: bail out
            //to reuse it across all the rows of one filter application (it is the whole point of the speed-up)
            if (ReferenceEquals(IndexedTable, PatternResults))
                return;

            PatternResultsIndex = new Dictionary<string, Dictionary<long, HashSet<int>>>();

            //Index each variable column of the pattern results
            foreach (RDFTableColumn resultColumn in PatternResults.Columns)
            {
                string columnName = resultColumn.Name;

                //PatternMemberID (canonical value identity) -> indexes of the result rows holding that value
                Dictionary<long, HashSet<int>> valueToResultRowIndexes = new Dictionary<long, HashSet<int>>();
                RDFTableRowCollection resultRows = PatternResults.Rows;
                for (int resultRowIndex = 0; resultRowIndex < resultRows.Count; resultRowIndex++)
                {
                    string cellValue = resultRows[resultRowIndex][columnName];
                    //An UNBOUND result cell can never be the target of an equality match, so we skip it
                    if (cellValue == null)
                        continue;
                    long valueID = RDFQueryUtilities.ParseRDFPatternMember(cellValue).PatternMemberID;
                    if (!valueToResultRowIndexes.TryGetValue(valueID, out HashSet<int> resultRowIndexes))
                        valueToResultRowIndexes[valueID] = resultRowIndexes = new HashSet<int>();
                    resultRowIndexes.Add(resultRowIndex);
                }
                PatternResultsIndex[columnName] = valueToResultRowIndexes;
            }

            //Remember which resultset this index belongs to, so the next calls can safely reuse it
            IndexedTable = PatternResults;
        }
        #endregion
    }
}