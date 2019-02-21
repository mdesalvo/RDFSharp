/*
   Copyright 2012-2019 Marco De Salvo

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

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFQueryEngine is the engine for construction and execution of SPARQL queries (MIRELLA)
    /// </summary>
    internal static class RDFQueryEngine {

        #region MIRELLA SPARQL
        /// <summary>
        /// Get the intermediate result tables of the given pattern group
        /// </summary>
        internal static void EvaluatePatternGroup(RDFQuery query, RDFPatternGroup patternGroup, RDFDataSource graphOrStore) {
            query.PatternResultTables[patternGroup.QueryMemberID] = new List<DataTable>();

            //Iterate the evaluable members of the pattern group
            foreach (var groupMember in patternGroup.GetEvaluableMembers()) {

                #region Pattern
                if (groupMember      is RDFPattern) {
                    var patternResultsTable         = graphOrStore.IsGraph() ? ApplyPattern((RDFPattern)groupMember, (RDFGraph)graphOrStore) :
                                                                               ApplyPattern((RDFPattern)groupMember, (RDFStore)graphOrStore);

                    #region Events
                    //Raise query event messages
                    var eventMsg    = String.Format("Pattern '{0}' has been evaluated on DataSource '{1}': Found '{2}' results.", (RDFPattern)groupMember, graphOrStore, patternResultsTable.Rows.Count);
                    if (query      is RDFAskQuery)
                        RDFQueryEvents.RaiseASKQueryEvaluation(eventMsg);
                    else if (query is RDFConstructQuery)
                        RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(eventMsg);
                    else if (query is RDFDescribeQuery)
                        RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(eventMsg);
                    else if (query is RDFSelectQuery)
                        RDFQueryEvents.RaiseSELECTQueryEvaluation(eventMsg);
                    #endregion

                    //Set name and metadata of result datatable
                    patternResultsTable.TableName   = ((RDFPattern)groupMember).ToString();
                    patternResultsTable.ExtendedProperties.Add("IsOptional",  ((RDFPattern)groupMember).IsOptional);
                    patternResultsTable.ExtendedProperties.Add("JoinAsUnion", ((RDFPattern)groupMember).JoinAsUnion);

                    //Save result datatable
                    query.PatternResultTables[patternGroup.QueryMemberID].Add(patternResultsTable);
                }
                #endregion

                #region PropertyPath
                else if (groupMember is RDFPropertyPath) {
                    var propPathResultsTable        = graphOrStore.IsGraph() ? ApplyPropertyPath((RDFPropertyPath)groupMember, (RDFGraph)graphOrStore) :
                                                                               ApplyPropertyPath((RDFPropertyPath)groupMember, (RDFStore)graphOrStore);

                    #region Events
                    //Raise query event messages
                    var eventMsg    = String.Format(String.Format("PropertyPath '{0}' has been evaluated on DataSource '{1}': Found '{2}' results.", (RDFPropertyPath)groupMember, graphOrStore, propPathResultsTable.Rows.Count));
                    if (query      is RDFAskQuery)
                        RDFQueryEvents.RaiseASKQueryEvaluation(eventMsg);
                    else if (query is RDFConstructQuery)
                        RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(eventMsg);
                    else if (query is RDFDescribeQuery)
                        RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(eventMsg);
                    else if (query is RDFSelectQuery)
                        RDFQueryEvents.RaiseSELECTQueryEvaluation(eventMsg);
                    #endregion

                    //Set name of result datatable
                    propPathResultsTable.TableName  = ((RDFPropertyPath)groupMember).ToString();

                    //Save result datatable
                    query.PatternResultTables[patternGroup.QueryMemberID].Add(propPathResultsTable);
                }
                #endregion

            }
        }

        /// <summary>
        /// Get the comprehensive result table of the given pattern group
        /// </summary>
        internal static void FinalizePatternGroup(RDFQuery query, RDFPatternGroup patternGroup) {
            if (patternGroup.GetEvaluableMembers().Any()) {

                //Populate pattern group result table
                var patternGroupResultTable = RDFQueryUtilities.CombineTables(query.PatternResultTables[patternGroup.QueryMemberID], false);

                //Add it to the list of query member result tables
                query.QueryMemberResultTables.Add(patternGroup.QueryMemberID, patternGroupResultTable);

                //Populate its name and metadata
                query.QueryMemberResultTables[patternGroup.QueryMemberID].TableName = patternGroup.ToString();
                if (!query.QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional")) {
                     query.QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties.Add("IsOptional", patternGroup.IsOptional);
                }
                else {
                     query.QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties["IsOptional"] = patternGroup.IsOptional;
                }
                if (!query.QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion")) {
                     query.QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", patternGroup.JoinAsUnion);
                }
                else {
                    query.QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties["JoinAsUnion"] = patternGroup.JoinAsUnion;
                }

            }
        }

        /// <summary>
        /// Apply the filters of the given pattern group to its result table
        /// </summary>
        internal static void ApplyFilters(RDFQuery query, RDFPatternGroup patternGroup) {
            if (patternGroup.GetEvaluableMembers().Any() && patternGroup.GetFilters().Any()) {
                DataTable filteredTable  = query.QueryMemberResultTables[patternGroup.QueryMemberID].Clone();
                IEnumerator rowsEnum     = query.QueryMemberResultTables[patternGroup.QueryMemberID].Rows.GetEnumerator();

                //Iterate the rows of the pattern group's result table
                Boolean keepRow          = false;
                while (rowsEnum.MoveNext()) {

                    //Apply the pattern group's filters on the row
                    keepRow              = true;
                    var filtersEnum      = patternGroup.GetFilters().GetEnumerator();
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
                query.QueryMemberResultTables[patternGroup.QueryMemberID] = filteredTable;
            }
        }

        /// <summary>
        /// Apply the query modifiers to the query result table
        /// </summary>
        internal static DataTable ApplyModifiers(RDFQuery query, DataTable table) {

            //SELECT query has ORDERBY modifiers and PROJECTION operator
            if (query is RDFSelectQuery) {

                //Apply the ORDERBY modifiers
                var ordModifiers  = query.GetModifiers().Where(m => m is RDFOrderByModifier);
                if (ordModifiers.Any()) {
                    table         = ordModifiers.Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));
                    table         = table.DefaultView.ToTable();
                }

                //Apply the PROJECTION operator
                if (((RDFSelectQuery)query).ProjectionVars.Any()) {

                    //Remove non-projection variables
                    var nonProjCols    = new List<DataColumn>();
                    foreach(DataColumn dtCol in table.Columns) {
                        if (!((RDFSelectQuery)query).ProjectionVars.Any(pv => pv.Key.ToString().Equals(dtCol.ColumnName, StringComparison.OrdinalIgnoreCase))) {
                             nonProjCols.Add(dtCol);
                        }
                    }
                    nonProjCols.ForEach(npc => {
                        table.Columns.Remove(npc.ColumnName);
                    });

                    //Adjust ordinals
                    foreach (var pVar in ((RDFSelectQuery)query).ProjectionVars) {
                        RDFQueryUtilities.AddColumn(table, pVar.Key.ToString());
                        table.Columns[pVar.Key.ToString()].SetOrdinal(pVar.Value);
                    }

                }
                //Remove property path variables
                var propPathCols       = new List<DataColumn>();
                foreach (DataColumn dtCol in table.Columns) {
                    if (dtCol.ColumnName.StartsWith("?__PP")) {
                        propPathCols.Add(dtCol);
                    }
                }
                propPathCols.ForEach(ppc => {
                    table.Columns.Remove(ppc.ColumnName);
                });

            }

            //Apply the DISTINCT modifier
            var distinctModifier  = query.GetModifiers().SingleOrDefault(m => m is RDFDistinctModifier);
            if (distinctModifier != null) {
                table             = distinctModifier.ApplyModifier(table);
            }

            //Apply the OFFSET modifier
            var offsetModifier    = query.GetModifiers().SingleOrDefault(m => m is RDFOffsetModifier);
            if (offsetModifier   != null) {
                table             = offsetModifier.ApplyModifier(table);
            }            

            //Apply the LIMIT modifier
            var limitModifier     = query.GetModifiers().SingleOrDefault(m => m is RDFLimitModifier);
            if (limitModifier    != null) {
                table             = limitModifier.ApplyModifier(table);
            }

            return table;
        }

        /// <summary>
        /// Fills the templates of the given CONSTRUCT query with data from the given result table
        /// </summary>
        internal static DataTable FillTemplates(RDFConstructQuery constructQuery, DataTable resultTable) {

            //Create the structure of the result datatable
            DataTable result = new DataTable("CONSTRUCT_RESULTS");
            result.Columns.Add("SUBJECT",   Type.GetType("System.String"));
            result.Columns.Add("PREDICATE", Type.GetType("System.String"));
            result.Columns.Add("OBJECT",    Type.GetType("System.String"));
            result.AcceptChanges();

            //Initialize working variables
            var constructRow = new Dictionary<String, String>();
            constructRow.Add("SUBJECT",   null);
            constructRow.Add("PREDICATE", null);
            constructRow.Add("OBJECT",    null);

            //Iterate on the templates
            foreach(RDFPattern tp in constructQuery.Templates.Where(tp => tp.Variables.Count == 0 ||
                                                                          tp.Variables.TrueForAll(v => resultTable.Columns.Contains(v.ToString())))) {

                #region GROUND TEMPLATE
                //Check if the template is ground, so represents an explicit triple to be added as-is
                if (tp.Variables.Count           == 0) {
                    constructRow["SUBJECT"]       = tp.Subject.ToString();
                    constructRow["PREDICATE"]     = tp.Predicate.ToString();
                    constructRow["OBJECT"]        = tp.Object.ToString();
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
                        if(((DataRow)rowsEnum.Current).IsNull(tp.Object.ToString())) {
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

        /// <summary>
        /// Describes the terms of the given DESCRIBE query with data from the given result table
        /// </summary>
        internal static DataTable DescribeTerms(RDFDescribeQuery describeQuery, RDFDataSource graphOrStore, DataTable resultTable) {

            //Create the structure of the result datatable
            DataTable result                  = new DataTable("DESCRIBE_RESULTS");
            if (graphOrStore.IsStore()) {
                result.Columns.Add("CONTEXT", Type.GetType("System.String"));
            }
            result.Columns.Add("SUBJECT",     Type.GetType("System.String"));
            result.Columns.Add("PREDICATE",   Type.GetType("System.String"));
            result.Columns.Add("OBJECT",      Type.GetType("System.String"));
            result.AcceptChanges();

            //Query IS empty, so does not have pattern groups to fetch data from 
            //(we can only proceed by searching for resources in the describe terms)
            if (!describeQuery.GetPatternGroups().Any()) {

                 //Iterate the describe terms of the query which are resources (variables are omitted, since useless)
                 foreach(RDFPatternMember dt in describeQuery.DescribeTerms.Where(dterm => dterm is RDFResource)) {
                 
                     //Search on GRAPH
                     if (graphOrStore.IsGraph()) {

                         //Search as RESOURCE (S-P-O)
                         RDFGraph desc        = ((RDFGraph)graphOrStore).SelectTriplesBySubject((RDFResource)dt)
                                                     .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByPredicate((RDFResource)dt))
                                                         .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByObject((RDFResource)dt));
                         result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);

                     }
                 
                     //Search on STORE
                     else {

                         //Search as BLANK RESOURCE (S-P-O)
                         if (((RDFResource)dt).IsBlank) {
                             RDFMemoryStore desc  = ((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt)
                                                         .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt))
                                                             .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt));
                             result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                         }

                         //Search as NON-BLANK RESOURCE (C-S-P-O)
                         else {
                             RDFMemoryStore desc  = ((RDFStore)graphOrStore).SelectQuadruplesByContext(new RDFContext(((RDFResource)dt).URI))
                                                         .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt))
                                                             .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt))
                                                                 .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt));
                             result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                         }

                     }

                 }

            }

            //Query IS NOT empty, so does have pattern groups to fetch data from
            else {

                 //In case of a "Star" query, all the variables must be considered describe terms
                 if (!describeQuery.DescribeTerms.Any()) {
                      describeQuery.GetPatternGroups()
                                   .ToList()
                                   .ForEach(q => q.Variables.ForEach(v => describeQuery.AddDescribeTerm(v)));
                 }

                 //Iterate the describe terms of the query
                 foreach(RDFPatternMember dt in describeQuery.DescribeTerms) {

                     //The describe term is a variable
                     if (dt is RDFVariable) {

                         //Process the variable
                         if (resultTable.Columns.Contains(dt.ToString())) {

                             //Iterate the results datatable's rows to retrieve terms to be described
                             IEnumerator rowsEnum           = resultTable.Rows.GetEnumerator();
                             while (rowsEnum.MoveNext()) {

                                 //Row contains a value in position of the variable corresponding to the describe term
                                 if(!((DataRow)rowsEnum.Current).IsNull(dt.ToString())) {

                                     //Retrieve the term to be described
                                     RDFPatternMember term  = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[dt.ToString()].ToString());

                                     //Search on GRAPH
                                     if (graphOrStore.IsGraph()) {

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

                                         //Search as RESOURCE
                                         if (term is RDFResource) {

                                            //Search as BLANK RESOURCE (S-P-O)
                                            if(((RDFResource)term).IsBlank) {
                                                RDFMemoryStore desc  = ((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)term)
                                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)term))
                                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)term));
                                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                            }

                                            //Search as NON-BLANK RESOURCE (C-S-P-O)
                                            else {
                                                RDFMemoryStore desc  = ((RDFStore)graphOrStore).SelectQuadruplesByContext(new RDFContext(((RDFResource)term).URI))
                                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)term))
                                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)term))
                                                                                .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)term));
                                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                            }

                                        }

                                        //Search as LITERAL (L)
                                        else {

                                            RDFMemoryStore desc  = ((RDFStore)graphOrStore).SelectQuadruplesByLiteral((RDFLiteral)term);
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
                         if (graphOrStore.IsGraph()) {

                             //Search as RESOURCE (S-P-O)
                             RDFGraph desc        = ((RDFGraph)graphOrStore).SelectTriplesBySubject((RDFResource)dt)
                                                         .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByPredicate((RDFResource)dt))
                                                             .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByObject((RDFResource)dt));
                             result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);

                         }
                 
                         //Search on STORE
                         else {

                             //Search as BLANK RESOURCE (S-P-O)
                             if (((RDFResource)dt).IsBlank) {
                                 RDFMemoryStore desc  = ((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt)
                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt))
                                                                .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt));
                                 result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                             }

                             //Search as NON-BLANK RESOURCE (C-S-P-O)
                             else {
                                 RDFMemoryStore desc  = ((RDFStore)graphOrStore).SelectQuadruplesByContext(new RDFContext(((RDFResource)dt).URI))
                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt))
                                                                .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt))
                                                                    .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt));
                                 result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                             }

                        }

                     }
                 
                 }

            }

            return result;
        }

        /// <summary>
        /// Applies the given pattern to the given graph
        /// </summary>
        internal static DataTable ApplyPattern(RDFPattern pattern, RDFGraph graph) {
            var matchingTriples  = new List<RDFTriple>();
            var resultTable      = new DataTable();

            //Apply the right pattern to the graph
            if (pattern.Subject            is RDFResource) {
                if (pattern.Predicate      is RDFResource) {
                    //S->P->
                    matchingTriples         = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject,
                                                                                     (RDFResource)pattern.Predicate,
                                                                                     null,
                                                                                     null);
                    RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                    RDFQueryUtilities.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.O, resultTable);
                }
                else {
                    if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                        if (pattern.Object is RDFResource) {
                            //S->->O
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject,
                                                                                     null,
                                                                                     (RDFResource)pattern.Object,
                                                                                     null);
                        }
                        else {
                            //S->->L
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject,
                                                                                     null,
                                                                                     null,
                                                                                     (RDFLiteral)pattern.Object);
                        }
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                        RDFQueryUtilities.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.P, resultTable);
                    }
                    else {
                        //S->->
                        matchingTriples     = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject,
                                                                                     null,
                                                                                     null,
                                                                                     null);
                        //In case of same P and O variable, must refine matching triples with a further value comparison
                        if (pattern.Predicate.Equals(pattern.Object)) {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                        }
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                        RDFQueryUtilities.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
                    }
                }
            }
            else {
                if (pattern.Predicate      is RDFResource) {
                    if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                        if (pattern.Object is RDFResource) {
                            //->P->O
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, null,
                                                                                     (RDFResource)pattern.Predicate,
                                                                                     (RDFResource)pattern.Object,
                                                                                     null);
                        }
                        else {
                            //->P->L
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, null,
                                                                                     (RDFResource)pattern.Predicate,
                                                                                     null,
                                                                                     (RDFLiteral)pattern.Object);
                        }
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                        RDFQueryUtilities.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.S, resultTable);
                    }
                    else {
                        //->P->
                        matchingTriples     = RDFModelUtilities.SelectTriples(graph, null,
                                                                                     (RDFResource)pattern.Predicate,
                                                                                     null,
                                                                                     null);
                        //In case of same S and O variable, must refine matching triples with a further value comparison
                        if (pattern.Subject.Equals(pattern.Object)) {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                        }
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                        RDFQueryUtilities.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
                    }
                }
                else {
                    if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                        if (pattern.Object is RDFResource) {
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, null,
                                                                                     null,
                                                                                     (RDFResource)pattern.Object,
                                                                                     null);
                        }
                        else {
                            //->->L
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, null,
                                                                                     null,
                                                                                     null,
                                                                                     (RDFLiteral)pattern.Object);
                        }
                        //In case of same S and P variable, must refine matching triples with a further value comparison
                        if (pattern.Subject.Equals(pattern.Predicate)) {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                        }
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                        RDFQueryUtilities.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
                    }
                    else {
                        //->->
                        matchingTriples     = RDFModelUtilities.SelectTriples(graph, null,
                                                                                     null,
                                                                                     null,
                                                                                     null);
                        //In case of same S and P variable, must refine matching triples with a further value comparison
                        if (pattern.Subject.Equals(pattern.Predicate)) {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                        }
                        //In case of same S and O variable, must refine matching triples with a further value comparison
                        if (pattern.Subject.Equals(pattern.Object)) {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                        }
                        //In case of same P and O variable, must refine matching triples with a further value comparison
                        if (pattern.Predicate.Equals(pattern.Object)) {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                        }
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                        RDFQueryUtilities.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
                    }
                }
            }
            return resultTable;
        }

        /// <summary>
        /// Applies the given property path to the given graph
        /// </summary>
        internal static DataTable ApplyPropertyPath(RDFPropertyPath propertyPath, RDFGraph graph) {
            var resultTable       = new DataTable();

            //Translate property path into equivalent list of patterns
            var patternList       = propertyPath.GetPatternList();

            //Evaluate produced list of patterns
            var patternTables     = new List<DataTable>();
            patternList.ForEach(p => {

                //Apply pattern to graph
                var patternTable  = ApplyPattern(p, graph);

                //Set extended properties
                patternTable.ExtendedProperties.Add("IsOptional",  p.IsOptional);
                patternTable.ExtendedProperties.Add("JoinAsUnion", p.JoinAsUnion);

                //Add produced table
                patternTables.Add(patternTable);

            });

            //Merge produced list of tables
            resultTable           = RDFQueryUtilities.CombineTables(patternTables, false);
            resultTable.TableName = propertyPath.ToString();

            return resultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given store
        /// </summary>
        internal static DataTable ApplyPattern(RDFPattern pattern, RDFStore store) {
            RDFMemoryStore result = new RDFMemoryStore();
            DataTable resultTable = new DataTable();

            //CSPO pattern
            if (pattern.Context                    != null) {
                if (pattern.Context                is RDFContext) {
                    if (pattern.Subject            is RDFResource) {
                        if (pattern.Predicate      is RDFResource) {
                            //C->S->P->
                            result                  = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             (RDFResource)pattern.Subject,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             null);
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                            RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.O, resultTable);
                        }
                        else {
                            if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                                //C->S->->O
                                if (pattern.Object is RDFResource) {
                                    result          = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                                }
                                //C->S->->L
                                else {
                                    result          = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.P, resultTable);
                            }
                            else {
                                //C->S->->
                                result              = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             null,
                                                                             null);
                                //In case of same P and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Predicate.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
                            }
                        }
                    }
                    else {
                        if (pattern.Predicate      is RDFResource) {
                            if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                                //C->->P->O
                                if (pattern.Object is RDFResource) {
                                    result          = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                                }
                                //C->->P->L
                                else {
                                    result          = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.S, resultTable);
                            }
                            else {
                                //C->->P->
                                result              = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             null);
                                //In case of same S and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
                            }
                        }
                        else {
                            if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                                //C->->->O
                                if (pattern.Object is RDFResource) {
                                    result          = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             null,
                                                                             null,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                                }
                                //C->->->L
                                else {
                                    result          = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             null,
                                                                             null,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                                }
                                //In case of same S and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Predicate)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
                            }
                            else {
                                //C->->->
                                result              = store.SelectQuadruples((RDFContext)pattern.Context,
                                                                             null,
                                                                             null,
                                                                             null,
                                                                             null);
                                //In case of same S and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Predicate)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same S and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                                }
                                //In case of same P and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Predicate.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
                            }
                        }
                    }
                }
                else {
                    if (pattern.Subject            is RDFResource) {
                        if (pattern.Predicate      is RDFResource) {
                            if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                                //->S->P->O
                                if (pattern.Object is RDFResource) {
                                    result          = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             (RDFResource)pattern.Predicate,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                                }
                                //->S->P->L
                                else {
                                    result          = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Context.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.C, resultTable);
                            }
                            else {
                                //->S->P->
                                result              = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             null);
                                //In case of same C and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Context.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CO, resultTable);
                            }
                        }
                        else {
                            if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                                //->S->->O
                                if (pattern.Object is RDFResource) {
                                    result          = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                                }
                                //->S->->L
                                else {
                                    result          = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                                }
                                //In case of same C and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Predicate)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Context.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CP, resultTable);
                            }
                            else {
                                //->S->->
                                result              = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             null,
                                                                             null);
                                //In case of same C and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Predicate)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same C and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                                }
                                //In case of same P and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Predicate.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Context.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CPO, resultTable);
                            }
                        }
                    }
                    else {
                        if (pattern.Predicate      is RDFResource) {
                            if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                                //->->P->O
                                if (pattern.Object is RDFResource) {
                                    result          = store.SelectQuadruples(null,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                                }
                                //->->P->L
                                else {
                                    result          = store.SelectQuadruples(null,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                                }
                                //In case of same C and S variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Subject)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Context.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CS, resultTable);
                            }
                            else {
                                //->->P->
                                result              = store.SelectQuadruples(null,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             null);
                                //In case of same C and S variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Subject)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                                }
                                //In case of same C and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                                }
                                //In case of same S and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Context.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSO, resultTable);
                            }
                        }
                        else {
                            if (pattern.Object     is RDFResource || pattern.Object is RDFLiteral) {
                                //->->->O
                                if (pattern.Object is RDFResource) {
                                    result          = store.SelectQuadruples(null,
                                                                             null,
                                                                             null,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                                }
                                //->->->L
                                else {
                                    result          = store.SelectQuadruples(null,
                                                                             null,
                                                                             null,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                                }
                                //In case of same C and S variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Subject)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                                }
                                //In case of same C and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Predicate)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same S and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Predicate)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Context.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSP, resultTable);
                            }
                            else {
                                //->->->
                                result              = store.SelectQuadruples(null,
                                                                             null,
                                                                             null,
                                                                             null,
                                                                             null);
                                //In case of same C and S variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Subject)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                                }
                                //In case of same C and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Predicate)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same C and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                                }
                                //In case of same S and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Predicate)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same S and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                                }
                                //In case of same P and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Predicate.Equals(pattern.Object)) {
                                    result          = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                                }
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Context.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                                RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                                RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSPO, resultTable);
                            }
                        }
                    }
                }
            }

            //SPO pattern
            else {
                if (pattern.Subject                is RDFResource) {
                    if (pattern.Predicate          is RDFResource) {
                        //S->P->
                        result                      = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             null);
                        RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                        RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.O, resultTable);
                    }
                    else {
                        if (pattern.Object         is RDFResource || pattern.Object is RDFLiteral) {
                            //S->->O
                            if (pattern.Object     is RDFResource) {
                                result              = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                            }
                            //S->->L
                            else {
                                result              = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                            }
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                            RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.P, resultTable);
                        }
                        else {
                            //S->->
                            result                  = store.SelectQuadruples(null,
                                                                             (RDFResource)pattern.Subject,
                                                                             null,
                                                                             null,
                                                                             null);
                            //In case of same P and O variable, must refine matching quadruples with a further value comparison
                            if (pattern.Predicate.Equals(pattern.Object)) {
                                result              = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                            }
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                            RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
                        }
                    }
                }
                else {
                    if (pattern.Predicate          is RDFResource) {
                        if (pattern.Object         is RDFResource || pattern.Object is RDFLiteral) {
                            //->P->O
                            if (pattern.Object     is RDFResource) {
                                result              = store.SelectQuadruples(null,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                            }
                            //->P->L
                            else {
                                result              = store.SelectQuadruples(null,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                            }
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                            RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.S, resultTable);
                        }
                        else {
                            //->P->
                            result                  = store.SelectQuadruples(null,
                                                                             null,
                                                                             (RDFResource)pattern.Predicate,
                                                                             null,
                                                                             null);
                            //In case of same S and O variable, must refine matching quadruples with a further value comparison
                            if (pattern.Subject.Equals(pattern.Object)) {
                                result              = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                            }
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                            RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
                        }
                    }
                    else {
                        if (pattern.Object         is RDFResource || pattern.Object is RDFLiteral) {
                            //->->O
                            if (pattern.Object     is RDFResource) {
                                result              = store.SelectQuadruples(null,
                                                                             null,
                                                                             null,
                                                                             (RDFResource)pattern.Object,
                                                                             null);
                            }
                            //->->L
                            else {
                                result              = store.SelectQuadruples(null,
                                                                             null,
                                                                             null,
                                                                             null,
                                                                             (RDFLiteral)pattern.Object);
                            }
                            //In case of same S and P variable, must refine matching quadruples with a further value comparison
                            if (pattern.Subject.Equals(pattern.Predicate)) {
                                result              = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                            }
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                            RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
                        }
                        else {
                            //->->
                            result                  = store.SelectQuadruples(null,
                                                                             null,
                                                                             null,
                                                                             null,
                                                                             null);
                            //In case of same S and P variable, must refine matching quadruples with a further value comparison
                            if (pattern.Subject.Equals(pattern.Predicate)) {
                                result              = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                            }
                            //In case of same S and O variable, must refine matching quadruples with a further value comparison
                            if (pattern.Subject.Equals(pattern.Object)) {
                                result              = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                            }
                            //In case of same P and O variable, must refine matching quadruples with a further value comparison
                            if (pattern.Predicate.Equals(pattern.Object)) {
                                result              = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                            }
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Subject.ToString());
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Predicate.ToString());
                            RDFQueryUtilities.AddColumn(resultTable, pattern.Object.ToString());
                            RDFQueryUtilities.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
                        }
                    }
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Applies the given property path to the given store
        /// </summary>
        internal static DataTable ApplyPropertyPath(RDFPropertyPath propertyPath, RDFStore store) {
            var resultTable       = new DataTable();

            //Translate property path into equivalent list of patterns
            var patternList       = propertyPath.GetPatternList();

            //Evaluate produced list of patterns
            var patternTables     = new List<DataTable>();
            patternList.ForEach(p => {

                //Apply pattern to store
                var patternTable  = ApplyPattern(p, store);

                //Set extended properties
                patternTable.ExtendedProperties.Add("IsOptional",  p.IsOptional);
                patternTable.ExtendedProperties.Add("JoinAsUnion", p.JoinAsUnion);

                //Add produced table
                patternTables.Add(patternTable);

            });

            //Merge produced list of tables
            resultTable           = RDFQueryUtilities.CombineTables(patternTables, false);
            resultTable.TableName = propertyPath.ToString();

            return resultTable;
        }
        #endregion

    }

}