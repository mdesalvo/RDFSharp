/*
   Copyright 2012-2017 Marco De Salvo

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

        #region Properties
        /// <summary>
        /// Static instance of the comparer used by the engine to compare data columns
        /// </summary>
        internal static readonly DataColumnComparer dtComparer = new DataColumnComparer();
        #endregion

        #region Methods
        /// <summary>
        /// Get the intermediate result tables of the given pattern group
        /// </summary>
        internal static void EvaluatePatterns(RDFQuery query, RDFPatternGroup patternGroup, RDFDataSource graphOrStore) {
            query.PatternResultTables[patternGroup] = new List<DataTable>();

            //Iterate over the patterns of the pattern group
            foreach (var pattern in patternGroup.Patterns) {

                //Apply the pattern to the graph/store
                var patternResultsTable             = graphOrStore.IsGraph() ? ApplyPattern(pattern, (RDFGraph)graphOrStore) : 
                                                                               ApplyPattern(pattern, (RDFStore)graphOrStore);

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
        internal static void ApplyFilters(RDFQuery query, RDFPatternGroup patternGroup) {
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
        /// Apply the query modifiers to the query result table
        /// </summary>
        internal static DataTable ApplyModifiers(RDFQuery query, DataTable table) {
            String tablenameBak   = table.TableName;

            //SELECT query has ORDERBY modifiers and PROJECTION operator
            if (query is RDFSelectQuery) {

                //Apply the ORDERBY modifiers
                var ordModifiers  = query.Modifiers.Where(m => m is RDFOrderByModifier);
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

            }

            //Apply the DISTINCT modifier
            var distinctModifier  = query.Modifiers.SingleOrDefault(m => m is RDFDistinctModifier);
            if (distinctModifier != null) {
                table             = distinctModifier.ApplyModifier(table);
            }

            //Apply the OFFSET modifier
            var offsetModifier    = query.Modifiers.SingleOrDefault(m => m is RDFOffsetModifier);
            if (offsetModifier   != null) {
                table             = offsetModifier.ApplyModifier(table);
            }            

            //Apply the LIMIT modifier
            var limitModifier     = query.Modifiers.SingleOrDefault(m => m is RDFLimitModifier);
            if (limitModifier    != null) {
                table             = limitModifier.ApplyModifier(table);
            }

            table.TableName       = tablenameBak;
            return table;
        }

        /// <summary>
        /// Get the result table of the given pattern group
        /// </summary>
        internal static void CombinePatterns(RDFQuery query, RDFPatternGroup patternGroup) {
            if (patternGroup.Patterns.Any()) {

                //Populate pattern group result table
                var patternGroupResultTable = CombineTables(query.PatternResultTables[patternGroup], false);

                //Add it to the list of pattern group result tables
                query.PatternGroupResultTables.Add(patternGroup, patternGroupResultTable);

                //Populate its metadata
                query.PatternGroupResultTables[patternGroup].TableName = patternGroup.ToString();
                if (!query.PatternGroupResultTables[patternGroup].ExtendedProperties.ContainsKey("IsOptional")) {
                     query.PatternGroupResultTables[patternGroup].ExtendedProperties.Add("IsOptional", patternGroup.IsOptional);
                }
                else {
                     query.PatternGroupResultTables[patternGroup].ExtendedProperties["IsOptional"] = patternGroup.IsOptional;
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
            if (!describeQuery.PatternGroups.Any()) {

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
                      describeQuery.PatternGroups.ForEach(pg => pg.Variables.ForEach(v => describeQuery.AddDescribeTerm(v)));
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
        /// Joins two datatables WITHOUT support for OPTIONAL data
        /// </summary>
        internal static DataTable InnerJoinTables(DataTable dt1, DataTable dt2) {
            DataTable result                   = new DataTable();
            IEnumerable<DataColumn> dt1Cols    = dt1.Columns.OfType<DataColumn>();
            IEnumerable<DataColumn> dt2Cols    = dt2.Columns.OfType<DataColumn>();
            //To avoid possibility of multiple enumerations of IEnumerable
            IEnumerable<DataColumn> dt1Columns = (dt1Cols as IList<DataColumn> ?? dt1Cols.ToList<DataColumn>());
            IEnumerable<DataColumn> dt2Columns = (dt2Cols as IList<DataColumn> ?? dt2Cols.ToList<DataColumn>());

            //Determine common columns
            DataColumn[] commonColumns         = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                           .Select(c => new DataColumn(c.Caption, c.DataType))
                                                           .ToArray();

            //PRODUCT-JOIN
            if (commonColumns.Length  == 0) {

                //Create the structure of the product table
                result.Columns.AddRange(dt1Columns.Union(dt2Columns, dtComparer)
                              .Select(c => new DataColumn(c.Caption, c.DataType))
                              .ToArray());

                //Loop through dt1 table
                result.AcceptChanges();
                result.BeginLoadData();
                foreach (DataRow parentRow in dt1.Rows) {
                    Object[] firstArray       = parentRow.ItemArray;

                    //Loop through dt2 table
                    foreach (DataRow childRow in dt2.Rows) {
                        Object[] secondArray  = childRow.ItemArray;
                        Object[] productArray = new Object[firstArray.Length + secondArray.Length];
                        Array.Copy(firstArray,  0, productArray, 0, firstArray.Length);
                        Array.Copy(secondArray, 0, productArray, firstArray.Length, secondArray.Length);
                        result.LoadDataRow(productArray, true);
                    }

                }
                result.EndLoadData();

            }

            //INNER-JOIN
            else {

                //Use a DataSet to leverage a relation linking the common columns
                using (DataSet ds = new DataSet()) {

                    //Add copy of the tables to the dataset
                    ds.Tables.AddRange(new DataTable[] { dt1, dt2 });

                    //Identify join columns from dt1
                    DataColumn[] parentColumns  = new DataColumn[commonColumns.Length];
                    for (Int32 i = 0; i < parentColumns.Length; i++) {
                        parentColumns[i]        = ds.Tables[0].Columns[commonColumns[i].ColumnName];
                    }
                    //Identify join columns from dt2
                    DataColumn[] childColumns   = new DataColumn[commonColumns.Length];
                    for (Int32 i = 0; i < childColumns.Length; i++) {
                        childColumns[i]         = ds.Tables[1].Columns[commonColumns[i].ColumnName];
                    }

                    //Create the relation linking the common columns
                    DataRelation r              = new DataRelation("JoinRelation", parentColumns, childColumns, false);
                    ds.Relations.Add(r);

                    //Create the structure of the join table
                    List<String> duplicatedCols = new List<String>();
                    for (Int32 i = 0; i < ds.Tables[0].Columns.Count; i++) {
                        result.Columns.Add(ds.Tables[0].Columns[i].ColumnName, ds.Tables[0].Columns[i].DataType);
                    }
                    for (Int32 i = 0; i < ds.Tables[1].Columns.Count; i++) {
                        if (!result.Columns.Contains(ds.Tables[1].Columns[i].ColumnName)) {
                            result.Columns.Add(ds.Tables[1].Columns[i].ColumnName, ds.Tables[1].Columns[i].DataType);
                        }
                        else {
                            //Manage duplicated columns by appending a known identificator to their name
                            result.Columns.Add(ds.Tables[1].Columns[i].ColumnName + "_DUPLICATE_", ds.Tables[1].Columns[i].DataType);
                            duplicatedCols.Add(ds.Tables[1].Columns[i].ColumnName + "_DUPLICATE_");
                        }
                    }

                    //Loop through dt1 table
                    result.AcceptChanges();
                    result.BeginLoadData();
                    foreach (DataRow firstRow in ds.Tables[0].Rows) {

                        //Get "joined" dt2 rows by exploiting the leveraged relation
                        DataRow[] childRows          = firstRow.GetChildRows(r);
                        if (childRows.Length > 0) {
                            Object[] parentArray     = firstRow.ItemArray;
                            foreach (DataRow secondRow in childRows) {
                                Object[] secondArray = secondRow.ItemArray;
                                Object[] joinArray   = new Object[parentArray.Length + secondArray.Length];
                                Array.Copy(parentArray, 0, joinArray, 0, parentArray.Length);
                                Array.Copy(secondArray, 0, joinArray, parentArray.Length, secondArray.Length);
                                result.LoadDataRow(joinArray, true);
                            }
                        }

                    }
                    //Eliminate the duplicated columns from the result table
                    duplicatedCols.ForEach(c => result.Columns.Remove(c));
                    result.EndLoadData();

                }

            }

            return result;
        }

        /// <summary>
        /// Joins two datatables WITH support for OPTIONAL data
        /// </summary>
        internal static DataTable OuterJoinTables(DataTable dt1, DataTable dt2) {
            DataTable finalResult              = new DataTable();
            IEnumerable<DataColumn> dt1Cols    = dt1.Columns.OfType<DataColumn>();
            IEnumerable<DataColumn> dt2Cols    = dt2.Columns.OfType<DataColumn>();
            //To avoid possibility of multiple enumerations of IEnumerable
            IEnumerable<DataColumn> dt1Columns = (dt1Cols as IList<DataColumn> ?? dt1Cols.ToList<DataColumn>());
            IEnumerable<DataColumn> dt2Columns = (dt2Cols as IList<DataColumn> ?? dt2Cols.ToList<DataColumn>());

            Boolean dt2IsOptionalTable         = (dt2.ExtendedProperties.ContainsKey("IsOptional") && dt2.ExtendedProperties["IsOptional"].Equals(true));
            Boolean joinInvalidationFlag       = false;
            Boolean foundAnyResult             = false;
            String strResCol                   = String.Empty;
            

            //Step 1: Determine common columns
            DataColumn[] commonColumns      = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                        .Select(c => new DataColumn(c.Caption, c.DataType))
                                                        .ToArray();

            //Step 2: Create structure of finalResult table
            finalResult.Columns.AddRange(dt1Columns.Union(dt2Columns.Except(commonColumns), dtComparer)
                               .Select(c => new DataColumn(c.Caption, c.DataType))
                               .ToArray());

            //Step 3: Loop through dt1 table
            finalResult.AcceptChanges();
            finalResult.BeginLoadData();
            foreach (DataRow leftRow in dt1.Rows) {
                foundAnyResult              = false;

                //Step 4: Loop through dt2 table
                foreach (DataRow rightRow in dt2.Rows) {
                    joinInvalidationFlag    = false;

                    //Step 5: Create a temporary join row
                    DataRow joinRow         = finalResult.NewRow();
                    foreach (DataColumn resCol in finalResult.Columns) {
                        if (!joinInvalidationFlag) {
                            strResCol       = resCol.ToString();

                            //Step 6a: NON-COMMON column
                            if (!commonColumns.Any(col => col.ToString().Equals(strResCol, StringComparison.Ordinal))) {

                                //Take value from left
                                if (dt1Columns.Any(col => col.ToString().Equals(strResCol, StringComparison.Ordinal))) {
                                    joinRow[strResCol] = leftRow[strResCol];
                                }

                                //Take value from right
                                else {
                                    joinRow[strResCol] = rightRow[strResCol];
                                }

                            }

                            //Step 6b: COMMON column
                            else {

                                //Left value is NULL
                                if (leftRow.IsNull(strResCol)) {

                                    //Right value is NULL
                                    if (rightRow.IsNull(strResCol)) {
                                        //Take NULL value
                                        joinRow[strResCol]       = DBNull.Value;
                                    }

                                    //Right value is NOT NULL
                                    else {
                                        //Take value from right
                                        joinRow[strResCol]       = rightRow[strResCol];
                                    }

                                }

                                //Left value is NOT NULL
                                else {

                                    //Right value is NULL
                                    if (rightRow.IsNull(strResCol)) {
                                        //Take value from left
                                        joinRow[strResCol]       = leftRow[strResCol];
                                    }

                                    //Right value is NOT NULL
                                    else {

                                        //Left value is EQUAL TO right value
                                        if (leftRow[strResCol].ToString().Equals(rightRow[strResCol].ToString(), StringComparison.Ordinal)) {
                                            //Take value from left (it's the same)
                                            joinRow[strResCol]   = leftRow[strResCol];
                                        }

                                        //Left value is NOT EQUAL TO right value
                                        else {
                                            //Raise the join invalidation flag
                                            joinInvalidationFlag = true;
                                            //Reject changes on the join row
                                            joinRow.RejectChanges();
                                        }

                                    }

                                }

                            }
                        }
                    }

                    //Step 7: Add join row to finalResults table
                    if (!joinInvalidationFlag) {
                         joinRow.AcceptChanges();
                         finalResult.Rows.Add(joinRow);
                         foundAnyResult       = true;
                    }

                }

                //Step 8: Manage presence of "OPTIONAL" pattern to the right
                if (!foundAnyResult && dt2IsOptionalTable) {
                     //In this case, the left row must be kept anyway and other columns from right are NULL
                     DataRow optionalRow      = finalResult.NewRow();
                     optionalRow.ItemArray    = leftRow.ItemArray;
                     optionalRow.AcceptChanges();
                     finalResult.Rows.Add(optionalRow);
                }

            }
            finalResult.EndLoadData();

            return finalResult;
        }

        /// <summary>
        /// Merges / Joins / Products the given list of data tables, based on presence of common columns and dynamic detection of Optional / Union operators
        /// </summary>
        internal static DataTable CombineTables(List<DataTable> dataTables, Boolean isMerge) {
            DataTable finalTable          = new DataTable();
            Boolean switchToOuterJoin     = false;

            if (dataTables.Count > 0) {

                //Process Unions
                for (Int32 i = 1; i < dataTables.Count; i++) {
                    if (isMerge || (dataTables[i - 1].ExtendedProperties.ContainsKey("JoinAsUnion") && dataTables[i - 1].ExtendedProperties["JoinAsUnion"].Equals(true))) {

                        //Merge the previous table into the current one
                        dataTables[i].Merge(dataTables[i - 1], true, MissingSchemaAction.Add);

                        //Clear the previous table and flag it as logically deleted
                        dataTables[i - 1].Rows.Clear();
                        dataTables[i - 1].ExtendedProperties.Add("LogicallyDeleted", true);

                        //Set automatic switch to OuterJoin (because we have done Unions, so null values must be preserved)
                        switchToOuterJoin = true;

                    }
                }
                dataTables.RemoveAll(dt   => dt.ExtendedProperties.ContainsKey("LogicallyDeleted") && dt.ExtendedProperties["LogicallyDeleted"].Equals(true));

                //Process Joins
                finalTable                = dataTables[0];
                for (Int32 i = 1; i < dataTables.Count; i++) {

                    //Set automatic switch to OuterJoin in case of relevant "Optional" detected
                    switchToOuterJoin     = (switchToOuterJoin || (dataTables[i].ExtendedProperties.ContainsKey("IsOptional") && dataTables[i].ExtendedProperties["IsOptional"].Equals(true)));

                    //Support OPTIONAL data
                    if (switchToOuterJoin) {
                        finalTable        = RDFQueryEngine.OuterJoinTables(finalTable, dataTables[i]);
                    }

                    //Do not support OPTIONAL data
                    else {
                        finalTable        = RDFQueryEngine.InnerJoinTables(finalTable, dataTables[i]);
                    }

                }

            }
            return finalTable;
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
                    RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.O, resultTable);
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
                        RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.P, resultTable);
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
                        RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
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
                        RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.S, resultTable);
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
                        RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
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
                        RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
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
                        RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
                    }
                }
            }
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
                            RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.O, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.P, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.S, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.C, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CO, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CP, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CPO, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CS, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSO, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSP, resultTable);
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
                                RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSPO, resultTable);
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
                        RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.O, resultTable);
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
                            RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.P, resultTable);
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
                            RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
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
                            RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.S, resultTable);
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
                            RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
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
                            RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
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
                            RDFQueryEngine.PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
                        }
                    }
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given graph
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, List<RDFTriple> triples, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable) {
            var bindings    = new Dictionary<String, String>();

            //Iterate result graph's triples
            foreach (var t in triples)  {
                switch (patternHole) {
                    //->P->O
                    case RDFQueryEnums.RDFPatternHoles.S:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        break;
                    //S->->O
                    case RDFQueryEnums.RDFPatternHoles.P:
                        bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        break;
                    //S->P->
                    case RDFQueryEnums.RDFPatternHoles.O:
                        bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        break;
                    //->->O
                    case RDFQueryEnums.RDFPatternHoles.SP:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        }
                        break;
                    //->P->
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
                        break;
                    //S->->
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
                        break;
                    //->->
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
                        break;
                }
                RDFQueryUtilities.AddRow(resultTable, bindings);
                bindings.Clear();
            }
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given store
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, RDFMemoryStore store, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable) {
            var bindings    = new Dictionary<String, String>();

            //Iterate result store's quadruples
            foreach (var q in store) {
                switch (patternHole) {
                    //->S->P->O
                    case RDFQueryEnums.RDFPatternHoles.C:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        break;
                    //->->P->O
                    case RDFQueryEnums.RDFPatternHoles.CS:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString())) {
                             bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        break;
                    //C->->P->O
                    case RDFQueryEnums.RDFPatternHoles.S:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        break;
                    //->S->->O
                    case RDFQueryEnums.RDFPatternHoles.CP:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        break;
                    //C->S->->O
                    case RDFQueryEnums.RDFPatternHoles.P:
                        bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        break;
                    //->S->P->
                    case RDFQueryEnums.RDFPatternHoles.CO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->S->P->
                    case RDFQueryEnums.RDFPatternHoles.O:
                        bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //->->->O
                    case RDFQueryEnums.RDFPatternHoles.CSP:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString())) {
                             bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        break;
                    //C->->->O
                    case RDFQueryEnums.RDFPatternHoles.SP:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        break;
                    //->->P->
                    case RDFQueryEnums.RDFPatternHoles.CSO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString())) {
                             bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->->P->
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //->S->->
                    case RDFQueryEnums.RDFPatternHoles.CPO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->S->->
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //->->->
                    case RDFQueryEnums.RDFPatternHoles.CSPO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString())) {
                             bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->->->
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString())) {
                             bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString())) {
                             bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                }
                RDFQueryUtilities.AddRow(resultTable, bindings);
                bindings.Clear();
            }
        }
        #endregion

    }

    /// <summary>
    /// Utility class for comparison between datacolumns
    /// </summary>
    internal class DataColumnComparer: IEqualityComparer<DataColumn> {

        #region Methods
        public Boolean Equals(DataColumn column1, DataColumn column2) {
            if (column1        != null) {
                return column2 != null && column1.Caption.Equals(column2.Caption, StringComparison.Ordinal);
            }
            return column2     == null;
        }

        public Int32 GetHashCode(DataColumn column) {
            return column.Caption.GetHashCode();
        }
        #endregion

    }

}