/*
   Copyright 2012-2015 Marco De Salvo

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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query {

    /// <summary>
    /// RDFSelectQueryEngine is the MIRELLA SPARQL query subengine for construction and execution of "SELECT" queries
    /// </summary>
    internal static class RDFSelectQueryEngine {

        #region Methods
        /// <summary>
        /// Get the intermediate result tables of the given pattern group
        /// </summary>
        internal static void EvaluatePatterns(RDFSelectQuery query, RDFPatternGroup patternGroup, Object graphOrStore) {
            query.PatternResultTables[patternGroup] = new List<DataTable>();

            //Iterate over the patterns of the pattern group
            foreach (RDFPattern pattern in patternGroup.Patterns) {

                //Apply the pattern to the graph/store
                DataTable patternResultsTable       = graphOrStore is RDFGraph ? RDFQueryEngine.ApplyPattern(pattern, (RDFGraph)graphOrStore)
                                                                               : RDFQueryEngine.ApplyPattern(pattern, (RDFStore)graphOrStore);

                //Set the name and the optionality metadata of the result datatable
                patternResultsTable.TableName       = pattern.ToString();
                patternResultsTable.ExtendedProperties.Add("IsOptional", pattern.IsOptional);
                patternResultsTable.ExtendedProperties.Add("JoinAsUnion", pattern.JoinAsUnion);

                //Save results datatable
                query.PatternResultTables[patternGroup].Add(patternResultsTable);

            }
        }

        /// <summary>
        /// Apply the filters of the given pattern group to its result table
        /// </summary>
        internal static void ApplyFilters(RDFSelectQuery query, RDFPatternGroup patternGroup) {
            if (patternGroup.Patterns.Any() && patternGroup.Filters.Any()) {
                DataTable filteredTable  = query.PatternGroupResultTables[patternGroup].Clone();
                IEnumerator rowsEnum     = query.PatternGroupResultTables[patternGroup].Rows.GetEnumerator();
                
                //Iterate the rows of the pattern group's result table
                Boolean keepRow          = false;
                while (rowsEnum.MoveNext()) {

                    //Apply the pattern group's filters on the row
                    keepRow              = true;
                    IEnumerator<RDFFilter> filtersEnum       = patternGroup.Filters.GetEnumerator();
                    while (keepRow      && filtersEnum.MoveNext()) {
                        keepRow          = filtersEnum.Current.ApplyFilter((DataRow)rowsEnum.Current, false);
                    }

                    //If the row has passed all the filters, keep it in the filtered result table
                    if (keepRow) {
                        DataRow newRow   = filteredTable.NewRow();
                        newRow.ItemArray = ((DataRow)rowsEnum.Current).ItemArray;
                        filteredTable.Rows.Add(newRow);
                    }

                }

                //Save results datatable
                query.PatternGroupResultTables[patternGroup] = filteredTable;
            }
        }

        /// <summary>
        /// Apply the query modifiers to the query result table
        /// </summary>
        internal static DataTable ApplyModifiers(RDFSelectQuery query, DataTable table) {
            String tblnmBak = table.TableName;

            //Apply the ORDERBY modifiers
            table           = query.Modifiers.Where(m => m is RDFOrderByModifier)
                                             .Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));
            table           = table.DefaultView.ToTable();

            //Apply the PROJECTION operator
            List<String> nonProjCols   = new List<String>();
            query.PatternGroups.ForEach(pg => 
                pg.Variables.ForEach(v => {
                    if (!query.IsStar && !v.IsResult) {
                        if (!nonProjCols.Exists(npc => npc.Equals(v.VariableName, StringComparison.Ordinal))) {
                            nonProjCols.Add(v.VariableName);
                        }
                    }
                })
            );
            nonProjCols.ForEach(c => {
                if (table.Columns.Contains(c)) {
                    table.Columns.Remove(c);
                }
            });

            //Apply the DISTINCT modifier
            table           = query.Modifiers.Where(m => m is RDFDistinctModifier)
                                             .Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));

            //Apply the OFFSET modifier
            table           = query.Modifiers.Where(m => m is RDFOffsetModifier)
                                             .Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));

            //Apply the LIMIT modifier
            table           = query.Modifiers.Where(m => m is RDFLimitModifier)
                                             .Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));

            table.TableName = tblnmBak;
            return table;
        }

        /// <summary>
        /// Get the result table of the given pattern group
        /// </summary>
        internal static void CombinePatterns(RDFSelectQuery query, RDFPatternGroup patternGroup) {
            if (patternGroup.Patterns.Any()) {
                                
                //Populate pattern group result table
                DataTable patternGroupResultTable = RDFQueryEngine.CombineTables(query.PatternResultTables[patternGroup], false);

                //Add it to the list of pattern group result tables
                query.PatternGroupResultTables.Add(patternGroup, patternGroupResultTable);

                //Populate its metadata
                query.PatternGroupResultTables[patternGroup].TableName = patternGroup.ToString();
                if (!query.PatternGroupResultTables[patternGroup].ExtendedProperties.ContainsKey("IsOptional")) {
                    query.PatternGroupResultTables[patternGroup].ExtendedProperties.Add("IsOptional", patternGroup.IsOptional);
                }
                else {
                    query.PatternGroupResultTables[patternGroup].ExtendedProperties["IsOptional"]  = patternGroup.IsOptional;
                }
                if (!query.PatternGroupResultTables[patternGroup].ExtendedProperties.ContainsKey("JoinAsUnion")) {
                    query.PatternGroupResultTables[patternGroup].ExtendedProperties.Add("JoinAsUnion", patternGroup.JoinAsUnion);
                }
                else {
                    query.PatternGroupResultTables[patternGroup].ExtendedProperties["JoinAsUnion"] = patternGroup.JoinAsUnion;
                }                

            }
        }
        #endregion

    }

}