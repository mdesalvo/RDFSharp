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
    /// RDFConstructQueryEngine is the subengine for construction and execution of SPARQL CONSTRUCT queries (MIRELLA)
    /// </summary>
    internal static class RDFConstructQueryEngine {

        #region Methods
        /// <summary>
        /// Get the intermediate result tables of the given pattern group
        /// </summary>
        internal static void EvaluatePatterns(RDFConstructQuery query, RDFPatternGroup patternGroup, Object graphOrStore) {
            query.PatternResultTables[patternGroup] = new List<DataTable>();

            //Iterate over the patterns of the pattern group
            foreach (RDFPattern pattern in patternGroup.Patterns) {

                //Apply the pattern to the graph/store
                DataTable patternResultsTable       = graphOrStore is RDFGraph ? RDFQueryEngine.ApplyPattern(pattern, (RDFGraph)graphOrStore)
                                                                               : RDFQueryEngine.ApplyPattern(pattern, (RDFStore)graphOrStore);

                //Set the name and the optionality metadata of the result datatable
                patternResultsTable.TableName       = pattern.ToString();
                patternResultsTable.ExtendedProperties.Add("IsOptional",  pattern.IsOptional);
                patternResultsTable.ExtendedProperties.Add("JoinAsUnion", pattern.JoinAsUnion);

                //Save the result datatable
                query.PatternResultTables[patternGroup].Add(patternResultsTable);

            }
        }

        /// <summary>
        /// Apply the filters of the given pattern group to its result table
        /// </summary>
        internal static void ApplyFilters(RDFConstructQuery query, RDFPatternGroup patternGroup) {
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

                //Save the result datatable
                query.PatternGroupResultTables[patternGroup] = filteredTable;
            }
        }

        /// <summary>
        /// Apply the query modifiers to the query result table
        /// </summary>
        internal static DataTable ApplyModifiers(RDFConstructQuery query, DataTable table) {
            String tablenameBak = table.TableName;

            //Apply the DISTINCT modifier
            table               = new RDFDistinctModifier().ApplyModifier(table);

            //Apply the OFFSET modifier
            table               = query.Modifiers.Where(m => m is RDFOffsetModifier)
                                                 .Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));

            //Apply the LIMIT modifier
            table               = query.Modifiers.Where(m => m is RDFLimitModifier)
                                                 .Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));

            table.TableName     = tablenameBak;
            return table;
        }

        /// <summary>
        /// Get the result table of the given pattern group
        /// </summary>
        internal static void CombinePatterns(RDFConstructQuery query, RDFPatternGroup patternGroup) {
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

        /// <summary>
        /// Fills the templates of the given query with data from the given result table
        /// </summary>
        internal static DataTable FillTemplates(RDFConstructQuery constructQuery, DataTable resultTable) {

            //Create the structure of the result datatable
            DataTable result = new DataTable("CONSTRUCT_RESULTS");
            result.Columns.Add("SUBJECT",   Type.GetType("System.String"));
            result.Columns.Add("PREDICATE", Type.GetType("System.String"));
            result.Columns.Add("OBJECT",    Type.GetType("System.String"));
            result.AcceptChanges();

            //Initialize working variables
            Dictionary<String, String> constructRow = new Dictionary<String, String>();
            constructRow.Add("SUBJECT",   null);
            constructRow.Add("PREDICATE", null);
            constructRow.Add("OBJECT",    null);

            //Iterate on the templates
            foreach (RDFPattern tp in constructQuery.Templates.Where(tp => tp.Variables.Count == 0 || 
                                                                     tp.Variables.TrueForAll(v => resultTable.Columns.Contains(v.ToString())))) {

                #region GROUND TEMPLATE
                //Check if the template is ground, so represents an explicit triple to be added as-is
                if (tp.Variables.Count       == 0) {
                    constructRow["SUBJECT"]   = tp.Subject.ToString();
                    constructRow["PREDICATE"] = tp.Predicate.ToString();
                    constructRow["OBJECT"]    = tp.Object.ToString();
                    RDFQueryUtilities.AddRow(result, constructRow);
                    continue;
                }
                #endregion

                #region NON-GROUND TEMPLATE
                //Iterate the result datatable's rows to construct the triples of the current template
                IEnumerator rowsEnum              = resultTable.Rows.GetEnumerator();
                while (rowsEnum.MoveNext()) {

                    #region SUBJECT
                    //Subject of the template is a variable
                    if (tp.Subject is RDFVariable) {

                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template subject
                        if (((DataRow)rowsEnum.Current).IsNull(tp.Subject.ToString())) {
                            continue;
                        }

                        RDFPatternMember subj     = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[tp.Subject.ToString()].ToString());
                        //Row contains a literal in position of the variable corresponding to the template subject
                        if (subj is RDFLiteral) {
                            continue;
                        }
                        //Row contains a resource in position of the variable corresponding to the template subject
                        constructRow["SUBJECT"]   = subj.ToString();

                    }
                    //Subject of the template is a resource
                    else {
                        constructRow["SUBJECT"]   = tp.Subject.ToString();
                    }
                    #endregion

                    #region PREDICATE
                    //Predicate of the template is a variable
                    if (tp.Predicate is RDFVariable) {

                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template predicate
                        if (((DataRow)rowsEnum.Current).IsNull(tp.Predicate.ToString())) {
                            continue;
                        }
                        RDFPatternMember pred     = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[tp.Predicate.ToString()].ToString());
                        //Row contains a blank resource or a literal in position of the variable corresponding to the template predicate
                        if ((pred is RDFResource && ((RDFResource)pred).IsBlank) || pred is RDFLiteral) {
                            continue;
                        }
                        //Row contains a non-blank resource in position of the variable corresponding to the template predicate
                        constructRow["PREDICATE"] = pred.ToString();

                    }
                    //Predicate of the template is a resource
                    else {
                        constructRow["PREDICATE"] = tp.Predicate.ToString();
                    }
                    #endregion

                    #region OBJECT
                    //Object of the template is a variable
                    if (tp.Object is RDFVariable) {

                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template object
                        if (((DataRow)rowsEnum.Current).IsNull(tp.Object.ToString())) {
                            continue;
                        }
                        RDFPatternMember obj      = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[tp.Object.ToString()].ToString());
                        //Row contains a resource or a literal in position of the variable corresponding to the template object
                        constructRow["OBJECT"]    = obj.ToString();

                    }
                    //Object of the template is a resource or a literal
                    else {
                        constructRow["OBJECT"]    = tp.Object.ToString();
                    }
                    #endregion

                    //Insert the triple into the final table
                    RDFQueryUtilities.AddRow(result, constructRow);

                }
                #endregion

            }

            return result;
        }
        #endregion

    }

}