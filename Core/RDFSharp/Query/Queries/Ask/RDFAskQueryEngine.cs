/*
   Copyright 2012-2016 Marco De Salvo

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
    /// RDFAskQueryEngine is the MIRELLA SPARQL query subengine for construction and execution of "ASK" queries
    /// </summary>
    internal static class RDFAskQueryEngine {

        #region Methods
        /// <summary>
        /// Get the intermediate result tables of the given pattern group
        /// </summary>
        internal static void EvaluatePatterns(RDFAskQuery query, RDFPatternGroup patternGroup, Object graphOrStore) {
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

                //Save the result datatable
                query.PatternResultTables[patternGroup].Add(patternResultsTable);

            }
        }

        /// <summary>
        /// Apply the filters of the given pattern group to its result table
        /// </summary>
        internal static void ApplyFilters(RDFAskQuery query, RDFPatternGroup patternGroup) {
            if (patternGroup.Patterns.Any() && patternGroup.Filters.Any()) {
                DataTable filteredTable  = query.PatternGroupResultTables[patternGroup].Clone();
                IEnumerator rowsEnum     = query.PatternGroupResultTables[patternGroup].Rows.GetEnumerator();

                //Iterate the rows of the pattern group's result table
                Boolean keepRow          = false;
                while (rowsEnum.MoveNext()) {

                    //Apply the pattern group's filters on the row
                    keepRow              = true;
                    IEnumerator<RDFFilter> filtersEnum = patternGroup.Filters.GetEnumerator();
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

                //Save the result datatable
                query.PatternGroupResultTables[patternGroup] = filteredTable;
            }
        }

        /// <summary>
        /// Get the result table of the given pattern group
        /// </summary>
        internal static void CombinePatterns(RDFAskQuery query, RDFPatternGroup patternGroup) {
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