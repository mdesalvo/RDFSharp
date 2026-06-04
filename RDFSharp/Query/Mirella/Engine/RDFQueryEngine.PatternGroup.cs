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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    // RDFQueryEngine (MIRELLA): pattern group evaluation, finalization, filters and modifiers.
    internal partial class RDFQueryEngine
    {
        /// <summary>
        /// Gets the intermediate result tables of the given pattern group
        /// </summary>
        internal void EvaluatePatternGroup(RDFPatternGroup patternGroup, RDFDataSource dataSource)
        {
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers()
                                                                         .Distinct()
                                                                         .ToList();

            //Optimize execution order of patterns within reorderable inner-join blocks
            if (dataSource is RDFGraph || dataSource is RDFMemoryStore)
                evaluablePGMembers = RDFQueryOptimizer.OptimizePatternOrder(evaluablePGMembers, dataSource);

            PatternGroupMemberResultTables[patternGroup.QueryMemberID] = new List<RDFTable>(evaluablePGMembers.Count);

            //**Service** evaluation => send it querified to SPARQL endpoint
            if (patternGroup.EvaluateAsService.HasValue)
            {
                //Cleanup pattern group in order to stringify into vanilla "SELECT *"
                bool isOptional = patternGroup.IsOptional;
                bool joinAsUnion = patternGroup.JoinAsUnion;
                bool joinAsMinus = patternGroup.JoinAsMinus;
                (RDFSPARQLEndpoint, RDFSPARQLEndpointQueryOptions)? asService = patternGroup.EvaluateAsService;
                patternGroup.IsOptional = false;
                patternGroup.JoinAsUnion = false;
                patternGroup.JoinAsMinus = false;
                patternGroup.EvaluateAsService = null;

                //Send query to SPARQL endpoint
                RDFSelectQueryResult serviceResults = new RDFSelectQuery()
                                                        .AddPatternGroup(patternGroup)
                                                        .ApplyToSPARQLEndpoint(asService.Value.Item1, asService.Value.Item2);
                RDFTable serviceResultsTable = RDFTable.FromDataTable(serviceResults.SelectResults);

                //Restore pattern group to its official state
                patternGroup.IsOptional = isOptional;
                patternGroup.JoinAsUnion = joinAsUnion;
                patternGroup.JoinAsMinus = joinAsMinus;
                patternGroup.EvaluateAsService = asService;

                //Set metadata of result table
                serviceResultsTable.IsOptional = patternGroup.IsOptional;
                serviceResultsTable.JoinAsUnion = patternGroup.JoinAsUnion;
                serviceResultsTable.JoinAsMinus = patternGroup.JoinAsMinus;

                //Save result table
                PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(serviceResultsTable);
            }

            //**Standard** evaluation => iterate its active members
            else
            {
                foreach (RDFPatternGroupMember evaluablePGMember in evaluablePGMembers)
                    switch (evaluablePGMember)
                    {
                        case RDFPattern pattern:
                            //Evaluate pattern on the given data source
                            RDFTable patternResultsTable = ApplyPattern(pattern, dataSource);
                            //Set metadata of result table
                            patternResultsTable.IsOptional = pattern.IsOptional;
                            patternResultsTable.JoinAsUnion = pattern.JoinAsUnion;
                            patternResultsTable.JoinAsMinus = pattern.JoinAsMinus;
                            //Save result table
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(patternResultsTable);
                            break;

                        case RDFPropertyPath propertyPath:
                            //Evaluate property path on the given data source
                            RDFTable pPathResultsTable = ApplyPropertyPath(propertyPath, dataSource);
                            //Set metadata of result table
                            pPathResultsTable.IsOptional = propertyPath.IsOptional;
                            pPathResultsTable.JoinAsUnion = propertyPath.JoinAsUnion;
                            pPathResultsTable.JoinAsMinus = propertyPath.JoinAsMinus;
                            //Save result table
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(pPathResultsTable);
                            break;

                        case RDFValues values:
                            //Transform SPARQL values into an equivalent filter
                            RDFValuesFilter valuesFilter = values.GetValuesFilter();
                            //Save result table
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(valuesFilter.ValuesTable);
                            //Inject SPARQL values filter
                            patternGroup.AddFilter(valuesFilter);
                            break;

                        case RDFBind bind:
                            //Bind operator is evaluated like an artificial ending of the pattern group:
                            // first we combine the tables collected until this moment
                            // then we evaluate the bind expression and project the bind variable, producing the comprehensive bind table
                            // finally we drop all tables collected until this moment, except the comprehensive bind table
                            //Populate current patternGroup result table
                            RDFTable currentPatternGroupResultTable = CombineTables(PatternGroupMemberResultTables[patternGroup.QueryMemberID]);
                            //Evaluate bind operator on the current patternGroup result table
                            ProjectBind(bind, currentPatternGroupResultTable);
                            //Delete previous patternGroup result tables and replace them with bind operator's one
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Clear();
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(currentPatternGroupResultTable);
                            break;

                        case RDFExistsFilter existsFilter:
                            //Evaluate exists filter's pattern on the given data source and save its result directly into the filter
                            existsFilter.PatternResults = ApplyPattern(existsFilter.Pattern, dataSource);
                            break;
                    }
            }
        }

        /// <summary>
        /// Gets the final result table of the given pattern group
        /// </summary>
        internal void FinalizePatternGroup(RDFPatternGroup patternGroup)
        {
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            if (evaluablePGMembers.Count > 0)
            {
                //Populate patternGroup result table
                RDFTable patternGroupResultTable = CombineTables(PatternGroupMemberResultTables[patternGroup.QueryMemberID]);

                //Populate its metadata (IsOptional)
                patternGroupResultTable.IsOptional = patternGroup.IsOptional
                  || patternGroupResultTable.IsOptional;

                //Populate its metadata (JoinAsUnion)
                patternGroupResultTable.JoinAsUnion = patternGroup.JoinAsUnion;

                //Populate its metadata (JoinAsMinus)
                patternGroupResultTable.JoinAsMinus = patternGroup.JoinAsMinus;

                //Add it to the list of query member result tables
                QueryMemberResultTables.Add(patternGroup.QueryMemberID, patternGroupResultTable);
            }
        }

        /// <summary>
        /// Applies the filters of the given pattern group to its result table
        /// </summary>
        internal void ApplyFilters(RDFPatternGroup patternGroup)
        {
            List<RDFPatternGroupMember> evaluablePatternGroupMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            List<RDFFilter> filters = patternGroup.GetFilters().ToList();
            if (evaluablePatternGroupMembers.Count > 0 && filters.Count > 0)
            {
                RDFTable sourceTable = QueryMemberResultTables[patternGroup.QueryMemberID];
                RDFTable filteredTable = sourceTable.Clone();
                int width = sourceTable.ColumnsCount;

                //Iterate the rows of the pattern group's result table
                foreach (RDFTableRow currentRow in sourceTable.Rows)
                {
                    //Apply the pattern group's filters on the row
                    bool keepRow = true;
                    foreach (RDFFilter filter in filters)
                    {
                        keepRow = filter.ApplyFilter(currentRow, false);
                        //Quick-Exit at the first failure
                        if (!keepRow)
                            break;
                    }

                    //If the row has passed all the filters, keep it in the filtered result table
                    if (keepRow)
                    {
                        string[] cells = new string[width];
                        for (int c = 0; c < width; c++)
                            cells[c] = currentRow[c];
                        filteredTable.AddRow(cells);
                    }
                }

                //Save the result table
                QueryMemberResultTables[patternGroup.QueryMemberID] = filteredTable;
            }
        }

        /// <summary>
        /// Applies the query modifiers to the query result table
        /// </summary>
        internal RDFTable ApplyModifiers(RDFQuery query, RDFTable table)
        {
            //Save the incoming join flags so they can be carried onto the modified result
            bool inOptional = table.IsOptional;
            bool inUnion = table.JoinAsUnion;
            bool inMinus = table.JoinAsMinus;

            #region GROUPBY/PROJECTION
            List<RDFModifier> modifiers = query.GetModifiers().ToList();
            if (query is RDFSelectQuery selectQuery)
            {
                #region GROUPBY
                RDFGroupByModifier groupByModifier = modifiers.OfType<RDFGroupByModifier>().FirstOrDefault();
                if (groupByModifier != null)
                {
                    table = groupByModifier.ApplyModifier(table);

                    //Adjust projection to work only with partition variables and aggregator variables
                    selectQuery.ProjectionVars.Clear();
                    groupByModifier.PartitionVariables.ForEach(pv => selectQuery.AddProjectionVariable(pv));
                    groupByModifier.Aggregators.ForEach(ag => selectQuery.AddProjectionVariable(ag.ProjectionVariable));
                }
                #endregion

                #region PROJECTION
                table = ProjectTable(selectQuery, table);
                #endregion
            }
            #endregion

            #region DISTINCT
            RDFDistinctModifier distinctModifier = modifiers.OfType<RDFDistinctModifier>().FirstOrDefault();
            if (distinctModifier != null)
                table = distinctModifier.ApplyModifier(table);
            #endregion

            #region OFFSET
            RDFOffsetModifier offsetModifier = modifiers.OfType<RDFOffsetModifier>().FirstOrDefault();
            if (offsetModifier != null)
                table = offsetModifier.ApplyModifier(table);
            #endregion

            #region  LIMIT
            RDFLimitModifier limitModifier = modifiers.OfType<RDFLimitModifier>().FirstOrDefault();
            if (limitModifier != null)
                table = limitModifier.ApplyModifier(table);
            #endregion

            //Carry the incoming join flags (IsOptional/Union/Minus) through the modifiers
            //onto the query result table
            table.IsOptional = inOptional;
            table.JoinAsUnion = inUnion;
            table.JoinAsMinus = inMinus;
            return table;
        }
    }
}