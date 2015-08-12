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
    /// RDFDescribeQueryEngine is the MIRELLA SPARQL query subengine for construction and execution of "DESCRIBE" queries
    /// </summary>
    internal static class RDFDescribeQueryEngine {

        #region Methods
        /// <summary>
        /// Get the intermediate result tables of the given pattern group
        /// </summary>
        internal static void EvaluatePatterns(RDFDescribeQuery query, RDFPatternGroup patternGroup, Object graphOrStore) {
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
        internal static void ApplyFilters(RDFDescribeQuery query, RDFPatternGroup patternGroup) {
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
        internal static DataTable ApplyModifiers(RDFDescribeQuery query, DataTable table) {
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
        internal static void CombinePatterns(RDFDescribeQuery query, RDFPatternGroup patternGroup) {
            if (patternGroup.Patterns.Any()) {

                //Populate pattern group result table
                DataTable patternGroupResultTable                      = RDFQueryEngine.CombineTables(query.PatternResultTables[patternGroup], false);

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
        /// Describes the terms of the given query with data from the given result table
        /// </summary>
        internal static DataTable DescribeTerms(RDFDescribeQuery describeQuery, Object graphOrStore, DataTable resultTable) {

            //Create the structure of the result datatable
            DataTable result                 = new DataTable("DESCRIBE_RESULTS");
            result.Columns.Add("SUBJECT",   Type.GetType("System.String"));
            result.Columns.Add("PREDICATE", Type.GetType("System.String"));
            result.Columns.Add("OBJECT",    Type.GetType("System.String"));
            result.AcceptChanges();

            //Query IS empty, so does not have pattern groups to fetch data from 
            //(we can only proceed by searching for resources in the describe terms)
            if (describeQuery.IsEmpty) {                

                //Iterate the describe terms of the query which are resources (variables are omitted, since useless)
                foreach (RDFPatternMember dt in describeQuery.DescribeTerms.Where(dterm => dterm is RDFResource)) {

                    //Search on GRAPH
                    if (graphOrStore is RDFGraph) {
                        //Search as RESOURCE (S-P-O)
                        RDFGraph desc        = ((RDFGraph)graphOrStore).SelectTriplesBySubject((RDFResource)dt)
                                                    .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByPredicate((RDFResource)dt))
                                                        .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByObject((RDFResource)dt));
                        result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                    }

                    //Search on STORE
                    else {
                        //Search as RESOURCE (S-P-O)
                        RDFMemoryStore desc  = ((RDFMemoryStore)((RDFMemoryStore)((RDFMemoryStore)((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt))
                                                    .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt)))
                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt)));
                        result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                    }

                }

            }

            //Query IS NOT empty, so does have pattern groups to fetch data from
            else {

                //In case of a "Star" query, all the variables must be considered describe terms
                if (describeQuery.IsStar) {
                    describeQuery.PatternGroups.ForEach(pg => 
                        pg.Variables.ForEach(v => describeQuery.AddDescribeTerm(v))
                    );
                }

                //Iterate the describe terms of the query
                foreach (RDFPatternMember dt in describeQuery.DescribeTerms) {

                    //The describe term is a variable
                    if (dt is RDFVariable) {

                        //Process the variable
                        if(resultTable.Columns.Contains(dt.ToString())) {

                            //Iterate the results datatable's rows to retrieve terms to be described
                            IEnumerator rowsEnum           = resultTable.Rows.GetEnumerator();
                            while (rowsEnum.MoveNext()) {

                                //Row contains a value in position of the variable corresponding to the describe term
                                if (!((DataRow)rowsEnum.Current).IsNull(dt.ToString())) {
                                    
                                    //Retrieve the term to be described
                                    RDFPatternMember term  = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[dt.ToString()].ToString());

                                    //Search on GRAPH
                                    if (graphOrStore is RDFGraph) {
                                        //Search as RESOURCE (S-P-O)
                                        if (term is RDFResource) {
                                            RDFGraph desc  = ((RDFGraph)graphOrStore).SelectTriplesBySubject((RDFResource)term)
                                                                    .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByPredicate((RDFResource)term))
                                                                        .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByObject((RDFResource)term));
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }
                                        //Search as LITERAL (L)
                                        else {
                                            RDFGraph desc  = ((RDFGraph)graphOrStore).SelectTriplesByLiteral((RDFLiteral)term);
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }
                                    }

                                    //Search on STORE
                                    else {
                                        //Search as RESOURCE (S-P-O)
                                        if (term is RDFResource) {
                                            RDFMemoryStore desc  = ((RDFMemoryStore)((RDFMemoryStore)((RDFMemoryStore)((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)term))
                                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)term)))
                                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)term)));
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }
                                        //Search as LITERAL (L)
                                        else {
                                            RDFMemoryStore desc  = ((RDFMemoryStore)((RDFStore)graphOrStore).SelectQuadruplesByLiteral((RDFLiteral)term));
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }
                                    }

                                }

                            }

                        }

                    }

                    //The describe term is a resource
                    else {

                        //Search on GRAPH
                        if (graphOrStore is RDFGraph) {
                            //Search as RESOURCE (S-P-O)
                            RDFGraph desc        = ((RDFGraph)graphOrStore).SelectTriplesBySubject((RDFResource)dt)
                                                        .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByPredicate((RDFResource)dt))
                                                            .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByObject((RDFResource)dt));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }

                        //Search on STORE
                        else {
                            //Search as RESOURCE (S-P-O)
                            RDFMemoryStore desc  = ((RDFMemoryStore)((RDFMemoryStore)((RDFMemoryStore)((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt))
                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt)))
                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt)));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }

                    }

                }

            }

            return result;
        }
        #endregion

    }

}