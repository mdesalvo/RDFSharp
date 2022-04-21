/*
   Copyright 2012-2022 Marco De Salvo

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

using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryEngine is the engine for execution of SPARQL queries (MIRELLA)
    /// </summary>
    internal class RDFQueryEngine
    {
        #region Properties
        /// <summary>
        /// Dictionary of temporary result tables produced by evaluation of query members
        /// </summary>
        internal Dictionary<long, List<DataTable>> QueryMemberTemporaryResultTables { get; set; }

        /// <summary>
        /// Dictionary of final result tables produced by evaluation of query members
        /// </summary>
        internal Dictionary<long, DataTable> QueryMemberFinalResultTables { get; set; }

        /// <summary>
        /// Default column type used for Mirella tables
        /// </summary>
        internal static Type SystemString = typeof(string); 
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize a query engine instance
        /// </summary>
        internal RDFQueryEngine()
        {
            this.QueryMemberTemporaryResultTables = new Dictionary<long, List<DataTable>>();
            this.QueryMemberFinalResultTables = new Dictionary<long, DataTable>();
        }
        #endregion

        #region Methods

        #region MIRELLA SPARQL
        /// <summary>
        /// Evaluates the given SPARQL SELECT query on the given RDF datasource
        /// </summary>
        internal RDFSelectQueryResult EvaluateSelectQuery(RDFSelectQuery selectQuery, RDFDataSource datasource)
        {
            //Inject SPARQL values within every evaluable member
            selectQuery.InjectValues(selectQuery.GetValues());

            RDFSelectQueryResult queryResult = new RDFSelectQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = selectQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {
                //Iterate the evaluable members of the query
                EvaluateQueryMembers(selectQuery, evaluableQueryMembers, datasource);

                //Get the result table of the query
                DataTable queryResultTable = CombineTables(QueryMemberFinalResultTables.Values.ToList(), false);

                //Apply the modifiers of the query to the result table
                queryResult.SelectResults = ApplyModifiers(selectQuery, queryResultTable);
            }
            
            queryResult.SelectResults.TableName = selectQuery.ToString();
            return queryResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL DESCRIBE query on the given RDF datasource
        /// </summary>
        internal RDFDescribeQueryResult EvaluateDescribeQuery(RDFDescribeQuery describeQuery, RDFDataSource datasource)
        {
            DataTable FillDescribeTerms(DataTable qResultTable)
            {
                DataTable resultTable = new DataTable();

                if (datasource.IsFederation())
                {
                    foreach (RDFDataSource fedDataSource in (RDFFederation)datasource)
                    {
                        //Ensure to skip tricky empty federations
                        if (fedDataSource.IsFederation() && ((RDFFederation)fedDataSource).DataSourcesCount == 0)
                            continue;
                        resultTable.Merge(DescribeTerms(describeQuery, fedDataSource, qResultTable), true, MissingSchemaAction.Add);
                    }
                }
                else
                {
                    resultTable = DescribeTerms(describeQuery, datasource, qResultTable);
                }

                return resultTable;
            }

            //Inject SPARQL values within every evaluable member
            describeQuery.InjectValues(describeQuery.GetValues());

            DataTable queryResultTable = new DataTable();
            RDFDescribeQueryResult queryResult = new RDFDescribeQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = describeQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {
                //Iterate the evaluable members of the query
                EvaluateQueryMembers(describeQuery, evaluableQueryMembers, datasource);

                //Get the result table of the query
                queryResultTable = CombineTables(this.QueryMemberFinalResultTables.Values.ToList(), false);
            }

            //Describe the terms from the result table
            DataTable describeResultTable = FillDescribeTerms(queryResultTable);

            //Apply the modifiers of the query to the result table
            queryResult.DescribeResults = ApplyModifiers(describeQuery, describeResultTable);
            
            queryResult.DescribeResults.TableName = describeQuery.ToString();
            return queryResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL CONSTRUCT query on the given RDF datasource
        /// </summary>
        internal RDFConstructQueryResult EvaluateConstructQuery(RDFConstructQuery constructQuery, RDFDataSource datasource)
        {
            //Inject SPARQL values within every evaluable member
            constructQuery.InjectValues(constructQuery.GetValues());

            DataTable queryResultTable = new DataTable();
            RDFConstructQueryResult constructResult = new RDFConstructQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = constructQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {
                //Iterate the evaluable members of the query
                EvaluateQueryMembers(constructQuery, evaluableQueryMembers, datasource);

                //Get the result table of the query
                queryResultTable = CombineTables(QueryMemberFinalResultTables.Values.ToList(), false);
            }

            //Fill the templates from the result table
            DataTable filledResultTable = FillTemplates(constructQuery.Templates, queryResultTable, false);

            //Apply the modifiers of the query to the result table
            constructResult.ConstructResults = ApplyModifiers(constructQuery, filledResultTable);

            constructResult.ConstructResults.TableName = constructQuery.ToString();
            return constructResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL ASK query on the given RDF datasource
        /// </summary>
        internal RDFAskQueryResult EvaluateAskQuery(RDFAskQuery askQuery, RDFDataSource datasource)
        {
            //Inject SPARQL values within every evaluable member
            askQuery.InjectValues(askQuery.GetValues());

            RDFAskQueryResult askResult = new RDFAskQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = askQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {
                //Iterate the evaluable members of the query
                EvaluateQueryMembers(askQuery, evaluableQueryMembers, datasource);

                //Get the result table of the query
                DataTable queryResultTable = CombineTables(QueryMemberFinalResultTables.Values.ToList(), false);

                //Transform the result into a boolean response
                askResult.AskResult = (queryResultTable.Rows.Count > 0);
            }
            
            return askResult;
        }

        /// <summary>
        /// Evaluates the given list of query members against the given datasource
        /// </summary>
        internal void EvaluateQueryMembers(RDFQuery query, List<RDFQueryMember> evaluableQueryMembers, RDFDataSource datasource)
        {
            foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
            {
                #region PATTERN GROUP
                if (evaluableQueryMember is RDFPatternGroup patternGroup)
                {
                    //Cleanup eventual data from stateful pattern group members
                    patternGroup.GroupMembers.ForEach(gm =>
                    {
                        if (gm is RDFExistsFilter existsFilter)
                            existsFilter.PatternResults?.Clear();
                    });

                    //Get the intermediate result tables of the pattern group
                    EvaluatePatternGroup(query, patternGroup, datasource);

                    //Get the result table of the pattern group
                    FinalizePatternGroup(query, patternGroup);

                    //Apply the filters of the pattern group to its result table
                    ApplyFilters(query, patternGroup);
                }
                #endregion

                #region SUBQUERY
                else if (evaluableQueryMember is RDFQuery subQuery)
                {
                    //Get the result table of the subquery
                    RDFSelectQueryResult subQueryResult = ((RDFSelectQuery)subQuery).ApplyToDataSource(datasource);
                    if (!QueryMemberFinalResultTables.ContainsKey(subQuery.QueryMemberID))
                    {
                        //Populate its name
                        QueryMemberFinalResultTables.Add(subQuery.QueryMemberID, subQueryResult.SelectResults);

                        //Populate its metadata (IsOptional)
                        if (!QueryMemberFinalResultTables[subQuery.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional"))
                            QueryMemberFinalResultTables[subQuery.QueryMemberID].ExtendedProperties.Add("IsOptional", subQuery.IsOptional);
                        else
                            QueryMemberFinalResultTables[subQuery.QueryMemberID].ExtendedProperties["IsOptional"] = subQuery.IsOptional
                                                                                                                      || (bool)QueryMemberFinalResultTables[subQuery.QueryMemberID].ExtendedProperties["IsOptional"];

                        //Populate its metadata (JoinAsUnion)
                        if (!QueryMemberFinalResultTables[subQuery.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion"))
                            QueryMemberFinalResultTables[subQuery.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", subQuery.JoinAsUnion);
                        else
                            QueryMemberFinalResultTables[subQuery.QueryMemberID].ExtendedProperties["JoinAsUnion"] = subQuery.JoinAsUnion;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Gets the intermediate result tables of the given pattern group
        /// </summary>
        internal void EvaluatePatternGroup(RDFQuery query, RDFPatternGroup patternGroup, RDFDataSource dataSource)
        {
            QueryMemberTemporaryResultTables[patternGroup.QueryMemberID] = new List<DataTable>();

            //Iterate the evaluable members of the pattern group
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().Distinct().ToList();
            foreach (RDFPatternGroupMember evaluablePGMember in evaluablePGMembers)
            {
                #region Pattern
                if (evaluablePGMember is RDFPattern pattern)
                {
                    DataTable patternResultsTable = ApplyPattern(pattern, dataSource);

                    //Set name and metadata of result datatable
                    patternResultsTable.TableName = pattern.ToString();
                    patternResultsTable.ExtendedProperties.Add("IsOptional", pattern.IsOptional);
                    patternResultsTable.ExtendedProperties.Add("JoinAsUnion", pattern.JoinAsUnion);

                    //Save result datatable
                    QueryMemberTemporaryResultTables[patternGroup.QueryMemberID].Add(patternResultsTable);
                }
                #endregion

                #region PropertyPath
                else if (evaluablePGMember is RDFPropertyPath propertyPath)
                {
                    DataTable pPathResultsTable = ApplyPropertyPath(propertyPath, dataSource);

                    //Set name of result datatable
                    pPathResultsTable.TableName = propertyPath.ToString();

                    //Save result datatable
                    QueryMemberTemporaryResultTables[patternGroup.QueryMemberID].Add(pPathResultsTable);
                }
                #endregion

                #region Values
                else if (evaluablePGMember is RDFValues values)
                {
                    //Transform SPARQL values into an equivalent filter
                    RDFValuesFilter valuesFilter = values.GetValuesFilter();
                    
                    //Save result datatable
                    QueryMemberTemporaryResultTables[patternGroup.QueryMemberID].Add(valuesFilter.ValuesTable);

                    //Inject SPARQL values filter
                    patternGroup.AddFilter(valuesFilter);
                }
                #endregion

                #region Filter (Exists)
                else if (evaluablePGMember is RDFExistsFilter existsFilter)
                {
                    DataTable existsFilterResultsTable = ApplyPattern(existsFilter.Pattern, dataSource);

                    //Set name and metadata of result datatable
                    existsFilterResultsTable.TableName = existsFilter.Pattern.ToString();
                    existsFilterResultsTable.ExtendedProperties.Add("IsOptional", false);
                    existsFilterResultsTable.ExtendedProperties.Add("JoinAsUnion", false);

                    //Save result datatable (directly into the filter)
                    existsFilter.PatternResults = existsFilterResultsTable;
                }
                #endregion
            }
        }

        /// <summary>
        /// Gets the final result table of the given pattern group
        /// </summary>
        internal void FinalizePatternGroup(RDFQuery query, RDFPatternGroup patternGroup)
        {
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            if (evaluablePGMembers.Any())
            {
                //Populate query member result table
                DataTable queryMemberFinalResultTable = CombineTables(QueryMemberTemporaryResultTables[patternGroup.QueryMemberID], false);

                //Add it to the list of query member result tables
                QueryMemberFinalResultTables.Add(patternGroup.QueryMemberID, queryMemberFinalResultTable);

                //Populate its name
                QueryMemberFinalResultTables[patternGroup.QueryMemberID].TableName = patternGroup.ToString();

                //Populate its metadata (IsOptional)
                if (!QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional"))
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties.Add("IsOptional", patternGroup.IsOptional);
                else
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties["IsOptional"] = patternGroup.IsOptional
                                                                                                                    || (bool)QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties["IsOptional"];

                //Populate its metadata (JoinAsUnion)
                if (!QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion"))
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", patternGroup.JoinAsUnion);
                else
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties["JoinAsUnion"] = patternGroup.JoinAsUnion;
            }
        }

        /// <summary>
        /// Applies the filters of the given pattern group to its result table
        /// </summary>
        internal void ApplyFilters(RDFQuery query, RDFPatternGroup patternGroup)
        {
            List<RDFPatternGroupMember> evaluablePatternGroupMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            List<RDFFilter> filters = patternGroup.GetFilters().ToList();
            if (evaluablePatternGroupMembers.Any() && filters.Any())
            {
                DataTable filteredTable = QueryMemberFinalResultTables[patternGroup.QueryMemberID].Clone();
                IEnumerator rowsEnum = QueryMemberFinalResultTables[patternGroup.QueryMemberID].Rows.GetEnumerator();

                //Iterate the rows of the pattern group's result table
                bool keepRow = false;
                while (rowsEnum.MoveNext())
                {
                    //Apply the pattern group's filters on the row
                    keepRow = true;
                    var filtersEnum = filters.GetEnumerator();
                    while (keepRow && filtersEnum.MoveNext())
                        keepRow = filtersEnum.Current.ApplyFilter((DataRow)rowsEnum.Current, false);

                    //If the row has passed all the filters, keep it in the filtered result table
                    if (keepRow)
                    {
                        DataRow newRow = filteredTable.NewRow();
                        newRow.ItemArray = ((DataRow)rowsEnum.Current).ItemArray;
                        filteredTable.Rows.Add(newRow);
                    }
                }

                //Save the result datatable
                QueryMemberFinalResultTables[patternGroup.QueryMemberID] = filteredTable;
            }
        }

        /// <summary>
        /// Applies the query modifiers to the query result table
        /// </summary>
        internal DataTable ApplyModifiers(RDFQuery query, DataTable table)
        {
            #region GROUPBY/ORDERBY/PROJECTION
            List<RDFModifier> modifiers = query.GetModifiers().ToList();
            if (query is RDFSelectQuery)
            {
                #region GROUPBY
                var groupbyModifier = modifiers.SingleOrDefault(m => m is RDFGroupByModifier);
                if (groupbyModifier != null)
                {
                    table = groupbyModifier.ApplyModifier(table);

                    #region PROJECTION
                    ((RDFSelectQuery)query).ProjectionVars.Clear();
                    ((RDFGroupByModifier)groupbyModifier).PartitionVariables.ForEach(pv => ((RDFSelectQuery)query).AddProjectionVariable(pv));
                    ((RDFGroupByModifier)groupbyModifier).Aggregators.ForEach(ag => ((RDFSelectQuery)query).AddProjectionVariable(ag.ProjectionVariable));
                    #endregion
                }
                #endregion

                #region ORDERBY
                var orderbyModifiers = modifiers.Where(m => m is RDFOrderByModifier);
                if (orderbyModifiers.Any())
                {
                    table = orderbyModifiers.Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));
                    table = table.DefaultView.ToTable();
                }
                #endregion

                #region PROJECTION
                table = ProjectTable((RDFSelectQuery)query, table);
                #endregion
            }
            #endregion

            #region DISTINCT
            var distinctModifier = modifiers.SingleOrDefault(m => m is RDFDistinctModifier);
            if (distinctModifier != null)
                table = distinctModifier.ApplyModifier(table);
            #endregion

            #region OFFSET
            var offsetModifier = modifiers.SingleOrDefault(m => m is RDFOffsetModifier);
            if (offsetModifier != null)
                table = offsetModifier.ApplyModifier(table);
            #endregion

            #region  LIMIT
            var limitModifier = modifiers.SingleOrDefault(m => m is RDFLimitModifier);
            if (limitModifier != null)
                table = limitModifier.ApplyModifier(table);
            #endregion

            return table;
        }

        /// <summary>
        /// Fills the given templates with data from the given result table<br/>
        /// (needsContext flag is true only when the caller is a store operation)
        /// </summary>
        internal DataTable FillTemplates(List<RDFPattern> templates, DataTable resultTable, bool needsContext)
        {
            //Create the structure of the result datatable
            DataTable result = new DataTable();
            if (needsContext)
                AddColumn(result, "?CONTEXT");
            AddColumn(result, "?SUBJECT");
            AddColumn(result, "?PREDICATE");
            AddColumn(result, "?OBJECT");

            //Initialize working variables
            Dictionary<string, string> bindings = new Dictionary<string, string>();
            if (needsContext)
                bindings.Add("?CONTEXT", null);
            bindings.Add("?SUBJECT", null);
            bindings.Add("?PREDICATE", null);
            bindings.Add("?OBJECT", null);

            //Iterate on the templates
            foreach (RDFPattern template in templates.Where(tp => tp.Variables.Count == 0
                                                                    || tp.Variables.TrueForAll(v => resultTable.Columns.Contains(v.ToString()))))
            {
                #region GROUND TEMPLATE
                if (template.Variables.Count == 0)
                {
                    if (needsContext)
                        bindings["?CONTEXT"] = template.Context?.ToString() ?? RDFNamespaceRegister.DefaultNamespace.ToString();
                    bindings["?SUBJECT"] = template.Subject.ToString();
                    bindings["?PREDICATE"] = template.Predicate.ToString();
                    bindings["?OBJECT"] = template.Object.ToString();
                    AddRow(result, bindings);
                    continue;
                }
                #endregion

                #region NON-GROUND TEMPLATE
                IEnumerator rowsEnum = resultTable.Rows.GetEnumerator();
                while (rowsEnum.MoveNext())
                {
                    #region CONTEXT
                    if (needsContext)
                    {
                        //Context of the template is a variable
                        if (template.Context is RDFVariable)
                        {
                            //Check if the template must be skipped, in order to not produce illegal triples
                            //Row contains an unbound value in position of the variable corresponding to the template context
                            if (((DataRow)rowsEnum.Current).IsNull(template.Context.ToString()))
                                continue;

                            RDFPatternMember ctx = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[template.Context.ToString()].ToString());
                            //Row contains a literal in position of the variable corresponding to the template context
                            if (ctx is RDFLiteral)
                                continue;
                            //Row contains a resource in position of the variable corresponding to the template context
                            bindings["?CONTEXT"] = ctx.ToString();
                        }
                        //Context of the template is a resource
                        else
                        {
                            bindings["?CONTEXT"] = template.Context?.ToString() ?? RDFNamespaceRegister.DefaultNamespace.ToString();
                        }
                    }
                    #endregion

                    #region SUBJECT
                    //Subject of the template is a variable
                    if (template.Subject is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template subject
                        if (((DataRow)rowsEnum.Current).IsNull(template.Subject.ToString()))
                            continue;

                        RDFPatternMember subj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[template.Subject.ToString()].ToString());
                        //Row contains a literal in position of the variable corresponding to the template subject
                        if (subj is RDFLiteral)
                            continue;
                        //Row contains a resource in position of the variable corresponding to the template subject
                        bindings["?SUBJECT"] = subj.ToString();
                    }
                    //Subject of the template is a resource
                    else
                    {
                        bindings["?SUBJECT"] = template.Subject.ToString();
                    }
                    #endregion

                    #region PREDICATE
                    //Predicate of the template is a variable
                    if (template.Predicate is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template predicate
                        if (((DataRow)rowsEnum.Current).IsNull(template.Predicate.ToString()))
                            continue;

                        RDFPatternMember pred = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[template.Predicate.ToString()].ToString());
                        //Row contains a blank resource or a literal in position of the variable corresponding to the template predicate
                        if ((pred is RDFResource && ((RDFResource)pred).IsBlank) || pred is RDFLiteral)
                            continue;
                        //Row contains a non-blank resource in position of the variable corresponding to the template predicate
                        bindings["?PREDICATE"] = pred.ToString();
                    }
                    //Predicate of the template is a resource
                    else
                    {
                        bindings["?PREDICATE"] = template.Predicate.ToString();
                    }
                    #endregion

                    #region OBJECT
                    //Object of the template is a variable
                    if (template.Object is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template object
                        if (((DataRow)rowsEnum.Current).IsNull(template.Object.ToString()))
                            continue;

                        RDFPatternMember obj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[template.Object.ToString()].ToString());
                        //Row contains a resource or a literal in position of the variable corresponding to the template object
                        bindings["?OBJECT"] = obj.ToString();
                    }
                    //Object of the template is a resource or a literal
                    else
                    {
                        bindings["?OBJECT"] = template.Object.ToString();
                    }
                    #endregion

                    //Insert the triple into the final table
                    AddRow(result, bindings);
                }
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Describes the terms of the given DESCRIBE query with data from the given result table
        /// </summary>
        internal DataTable DescribeTerms(RDFDescribeQuery describeQuery, RDFDataSource dataSource, DataTable resultTable)
        {
            //Create the structure of the result datatable
            DataTable result = new DataTable();
            if (dataSource.IsStore())
                AddColumn(result, "?CONTEXT");
            AddColumn(result, "?SUBJECT");
            AddColumn(result, "?PREDICATE");
            AddColumn(result, "?OBJECT");

            //Query IS empty, so does not have evaluable members to fetch data from.
            //We can only proceed by searching for resources in the describe terms.
            IEnumerable<RDFQueryMember> evaluableQueryMembers = describeQuery.GetEvaluableQueryMembers();
            if (!evaluableQueryMembers.Any())
            {
                //Iterate the describe terms of the query which are resources (variables are omitted, since useless)
                foreach (RDFResource dt in describeQuery.DescribeTerms.OfType<RDFResource>())
                {
                    //Search on GRAPH
                    if (dataSource is RDFGraph dataSourceGraph)
                    {
                        //Search as RESOURCE (S-P-O)
                        RDFGraph desc = dataSourceGraph.SelectTriplesBySubject(dt)
                                                       .UnionWith(dataSourceGraph.SelectTriplesByPredicate(dt))
                                                       .UnionWith(dataSourceGraph.SelectTriplesByObject(dt));
                        result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                    }
                    //Search on STORE
                    else if (dataSource is RDFStore dataSourceStore)
                    {
                        //Search as BLANK RESOURCE (S-P-O)
                        if (dt.IsBlank)
                        {
                            RDFMemoryStore desc = dataSourceStore.SelectQuadruplesBySubject(dt)
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesByPredicate(dt))
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesByObject(dt));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }
                        //Search as NON-BLANK RESOURCE (C-S-P-O)
                        else
                        {
                            RDFMemoryStore desc = dataSourceStore.SelectQuadruplesByContext(new RDFContext(dt.URI))
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesBySubject(dt))
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesByPredicate(dt))
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesByObject(dt));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }
                    }
                    //Search on SPARQL endpoint / FEDERATION
                    else
                    {
                        //Create CONSTRUCT query to build term description
                        RDFConstructQuery constructQuery =
                            new RDFConstructQuery()
                                .AddPatternGroup(new RDFPatternGroup("PG1")
                                    .AddPattern(new RDFPattern(dt, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                                    .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), dt, new RDFVariable("?OBJECT")).UnionWithNext())
                                    .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), dt))
                                )
                                .AddTemplate(new RDFPattern(dt, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")))
                                .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), dt, new RDFVariable("?OBJECT")))
                                .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), dt));

                        //Apply the query to the SPARQL endpoint / FEDERATION
                        RDFConstructQueryResult constructQueryResults =
                            dataSource.IsSPARQLEndpoint() ? constructQuery.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                          : constructQuery.ApplyToFederation((RDFFederation)dataSource);
                        result.Merge(constructQueryResults.ConstructResults, true, MissingSchemaAction.Add);
                    }
                }
            }

            //Query IS NOT empty, so does have query members to fetch data from.
            //We proceed by searching for resources and variables in the describe terms.
            else
            {
                //In case of a "Star" query, all the variables must be extracted as describe terms
                if (!describeQuery.DescribeTerms.Any())
                    GetDescribeTermsFromQueryMembers(describeQuery, evaluableQueryMembers);

                //Iterate the describe terms of the query
                foreach (RDFPatternMember dt in describeQuery.DescribeTerms)
                {
                    //The describe term is a variable
                    if (dt is RDFVariable)
                    {
                        //Process the variable
                        if (resultTable.Columns.Contains(dt.ToString()))
                        {
                            //Iterate the results datatable's rows to retrieve terms to be described
                            IEnumerator rowsEnum = resultTable.Rows.GetEnumerator();
                            while (rowsEnum.MoveNext())
                            {
                                //Row contains a value in position of the variable corresponding to the describe term
                                if (!((DataRow)rowsEnum.Current).IsNull(dt.ToString()))
                                {
                                    //Retrieve the term to be described
                                    RDFPatternMember term = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[dt.ToString()].ToString());

                                    //Search on GRAPH
                                    if (dataSource is RDFGraph dataSourceGraph)
                                    {
                                        //Search as RESOURCE (S-P-O)
                                        if (term is RDFResource termResource)
                                        {
                                            RDFGraph desc = dataSourceGraph.SelectTriplesBySubject(termResource)
                                                                           .UnionWith(dataSourceGraph.SelectTriplesByPredicate(termResource))
                                                                           .UnionWith(dataSourceGraph.SelectTriplesByObject(termResource));
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }
                                        //Search as LITERAL (L)
                                        else
                                        {
                                            RDFGraph desc = dataSourceGraph.SelectTriplesByLiteral((RDFLiteral)term);
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }
                                    }
                                    //Search on STORE
                                    else if (dataSource is RDFStore dataSourceStore)
                                    {
                                        //Search as RESOURCE
                                        if (term is RDFResource termResource)
                                        {
                                            //Search as BLANK RESOURCE (S-P-O)
                                            if (termResource.IsBlank)
                                            {
                                                RDFMemoryStore desc = dataSourceStore.SelectQuadruplesBySubject(termResource)
                                                                                     .UnionWith(dataSourceStore.SelectQuadruplesByPredicate(termResource))
                                                                                     .UnionWith(dataSourceStore.SelectQuadruplesByObject(termResource));
                                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                            }
                                            //Search as NON-BLANK RESOURCE (C-S-P-O)
                                            else
                                            {
                                                RDFMemoryStore desc = dataSourceStore.SelectQuadruplesByContext(new RDFContext(termResource.URI))
                                                                                     .UnionWith(dataSourceStore.SelectQuadruplesBySubject(termResource))
                                                                                     .UnionWith(dataSourceStore.SelectQuadruplesByPredicate(termResource))
                                                                                     .UnionWith(dataSourceStore.SelectQuadruplesByObject(termResource));
                                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                            }
                                        }
                                        //Search as LITERAL (L)
                                        else
                                        {
                                            RDFMemoryStore desc = dataSourceStore.SelectQuadruplesByLiteral((RDFLiteral)term);
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }
                                    }
                                    //Search on SPARQL endpoint / FEDERATION
                                    else
                                    {
                                        //Create CONSTRUCT query to build term description
                                        RDFConstructQuery constructQuery =
                                            term is RDFResource termResource
                                                ? new RDFConstructQuery()
                                                    .AddPatternGroup(new RDFPatternGroup("PG1")
                                                        .AddPattern(new RDFPattern(termResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                                                        .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), termResource, new RDFVariable("?OBJECT")).UnionWithNext())
                                                        .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), termResource))
                                                    )
                                                    .AddTemplate(new RDFPattern(termResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")))
                                                    .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), termResource, new RDFVariable("?OBJECT")))
                                                    .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), termResource))
                                                : new RDFConstructQuery()
                                                    .AddPatternGroup(new RDFPatternGroup("PG1")
                                                        .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), (RDFLiteral)term))
                                                    )
                                                    .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), (RDFLiteral)term));

                                        //Apply the query to the SPARQL endpoint / FEDERATION
                                        RDFConstructQueryResult constructQueryResults =
                                            dataSource.IsSPARQLEndpoint() ? constructQuery.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                                          : constructQuery.ApplyToFederation((RDFFederation)dataSource);
                                        result.Merge(constructQueryResults.ConstructResults, true, MissingSchemaAction.Add);
                                    }
                                }
                            }
                        }
                    }

                    //The describe term is a resource
                    else
                    {

                        //Search on GRAPH
                        if (dataSource is RDFGraph dataSourceGraph)
                        {
                            //Search as RESOURCE (S-P-O)
                            RDFGraph desc = dataSourceGraph.SelectTriplesBySubject((RDFResource)dt)
                                                           .UnionWith(dataSourceGraph.SelectTriplesByPredicate((RDFResource)dt))
                                                           .UnionWith(dataSourceGraph.SelectTriplesByObject((RDFResource)dt));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }
                        //Search on STORE
                        else if (dataSource is RDFStore dataSourceStore)
                        {
                            //Search as BLANK RESOURCE (S-P-O)
                            if (((RDFResource)dt).IsBlank)
                            {
                                RDFMemoryStore desc = dataSourceStore.SelectQuadruplesBySubject((RDFResource)dt)
                                                                     .UnionWith(dataSourceStore.SelectQuadruplesByPredicate((RDFResource)dt))
                                                                     .UnionWith(dataSourceStore.SelectQuadruplesByObject((RDFResource)dt));
                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                            }
                            //Search as NON-BLANK RESOURCE (C-S-P-O)
                            else
                            {
                                RDFMemoryStore desc = dataSourceStore.SelectQuadruplesByContext(new RDFContext(((RDFResource)dt).URI))
                                                                     .UnionWith(dataSourceStore.SelectQuadruplesBySubject((RDFResource)dt))
                                                                     .UnionWith(dataSourceStore.SelectQuadruplesByPredicate((RDFResource)dt))
                                                                     .UnionWith(dataSourceStore.SelectQuadruplesByObject((RDFResource)dt));
                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                            }
                        }
                        //Search on SPARQL endpoint / FEDERATION
                        else
                        {
                            //Create CONSTRUCT query to build term description
                            RDFConstructQuery constructQuery =
                                new RDFConstructQuery()
                                    .AddPatternGroup(new RDFPatternGroup("PG1")
                                        .AddPattern(new RDFPattern(dt, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                                        .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), dt, new RDFVariable("?OBJECT")).UnionWithNext())
                                        .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), dt))
                                    )
                                    .AddTemplate(new RDFPattern(dt, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")))
                                    .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), dt, new RDFVariable("?OBJECT")))
                                    .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), dt));

                            //Apply the query to the federation
                            RDFConstructQueryResult constructQueryResults =
                                dataSource.IsSPARQLEndpoint() ? constructQuery.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                              : constructQuery.ApplyToFederation((RDFFederation)dataSource);
                            result.Merge(constructQueryResults.ConstructResults, true, MissingSchemaAction.Add);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Exracts the describe terms from the given collection of query members
        /// </summary>
        internal void GetDescribeTermsFromQueryMembers(RDFDescribeQuery describeQuery, IEnumerable<RDFQueryMember> evaluableQueryMembers)
        {
            foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
            {
                #region PATTERN GROUP
                if (evaluableQueryMember is RDFPatternGroup)
                    ((RDFPatternGroup)evaluableQueryMember).Variables.ForEach(v => describeQuery.AddDescribeTerm(v));
                #endregion

                #region SUBQUERY
                else if (evaluableQueryMember is RDFQuery)
                    GetDescribeTermsFromQueryMembers(describeQuery, ((RDFSelectQuery)evaluableQueryMember).GetEvaluableQueryMembers());
                #endregion
            }
        }

        /// <summary>
        /// Applies the given pattern to the given data source
        /// </summary>
        internal DataTable ApplyPattern(RDFPattern pattern, RDFDataSource dataSource)
        {
            switch (dataSource)
            {
                case RDFGraph dataSourceGraph:
                    return ApplyPattern(pattern, dataSourceGraph);

                case RDFStore dataSourceStore:
                    return ApplyPattern(pattern, dataSourceStore);

                case RDFFederation dataSourceFederation:
                    return ApplyPattern(pattern, dataSourceFederation);

                case RDFSPARQLEndpoint dataSourceSparqlEndpoint:
                    return ApplyPattern(pattern, dataSourceSparqlEndpoint);
            }
            return new DataTable();
        }

        /// <summary>
        /// Applies the given pattern to the given graph
        /// </summary>
        internal DataTable ApplyPattern(RDFPattern pattern, RDFGraph graph)
        {
            List<RDFTriple> matchingTriples = new List<RDFTriple>();
            DataTable resultTable = new DataTable();

            //SPO pattern
            if (pattern.Subject is RDFResource)
            {
                if (pattern.Predicate is RDFResource)
                {
                    //S->P->
                    matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, null);
                    AddColumn(resultTable, pattern.Object.ToString());
                    PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.O, resultTable);
                }
                else
                {
                    if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                    {
                        if (pattern.Object is RDFResource)
                        {
                            //S->->O
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject, null, (RDFResource)pattern.Object, null);
                        }
                        else
                        {
                            //S->->L
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject, null, null, (RDFLiteral)pattern.Object);
                        }
                        AddColumn(resultTable, pattern.Predicate.ToString());
                        PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.P, resultTable);
                    }
                    else
                    {
                        //S->->
                        matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject, null, null, null);
                        //In case of same P and O variable, must refine matching triples with a further value comparison
                        if (pattern.Predicate.Equals(pattern.Object))
                        {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                        }
                        AddColumn(resultTable, pattern.Predicate.ToString());
                        AddColumn(resultTable, pattern.Object.ToString());
                        PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
                    }
                }
            }
            else
            {
                if (pattern.Predicate is RDFResource)
                {
                    if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                    {
                        if (pattern.Object is RDFResource)
                        {
                            //->P->O
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, null, (RDFResource)pattern.Predicate, (RDFResource)pattern.Object, null);
                        }
                        else
                        {
                            //->P->L
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, null, (RDFResource)pattern.Predicate, null, (RDFLiteral)pattern.Object);
                        }
                        AddColumn(resultTable, pattern.Subject.ToString());
                        PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.S, resultTable);
                    }
                    else
                    {
                        //->P->
                        matchingTriples = RDFModelUtilities.SelectTriples(graph, null, (RDFResource)pattern.Predicate, null, null);
                        //In case of same S and O variable, must refine matching triples with a further value comparison
                        if (pattern.Subject.Equals(pattern.Object))
                        {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                        }
                        AddColumn(resultTable, pattern.Subject.ToString());
                        AddColumn(resultTable, pattern.Object.ToString());
                        PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
                    }
                }
                else
                {
                    if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                    {
                        if (pattern.Object is RDFResource)
                        {
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, null, null, (RDFResource)pattern.Object, null);
                        }
                        else
                        {
                            //->->L
                            matchingTriples = RDFModelUtilities.SelectTriples(graph, null, null, null, (RDFLiteral)pattern.Object);
                        }
                        //In case of same S and P variable, must refine matching triples with a further value comparison
                        if (pattern.Subject.Equals(pattern.Predicate))
                        {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                        }
                        AddColumn(resultTable, pattern.Subject.ToString());
                        AddColumn(resultTable, pattern.Predicate.ToString());
                        PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
                    }
                    else
                    {
                        //->->
                        matchingTriples = RDFModelUtilities.SelectTriples(graph, null, null, null, null);
                        //In case of same S and P variable, must refine matching triples with a further value comparison
                        if (pattern.Subject.Equals(pattern.Predicate))
                        {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                        }
                        //In case of same S and O variable, must refine matching triples with a further value comparison
                        if (pattern.Subject.Equals(pattern.Object))
                        {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                        }
                        //In case of same P and O variable, must refine matching triples with a further value comparison
                        if (pattern.Predicate.Equals(pattern.Object))
                        {
                            matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                        }
                        AddColumn(resultTable, pattern.Subject.ToString());
                        AddColumn(resultTable, pattern.Predicate.ToString());
                        AddColumn(resultTable, pattern.Object.ToString());
                        PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
                    }
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given store
        /// </summary>
        internal DataTable ApplyPattern(RDFPattern pattern, RDFStore store)
        {
            RDFMemoryStore result = new RDFMemoryStore();
            DataTable resultTable = new DataTable();

            //CSPO pattern
            if (pattern.Context != null)
            {
                if (pattern.Context is RDFContext)
                {
                    if (pattern.Subject is RDFResource)
                    {
                        if (pattern.Predicate is RDFResource)
                        {
                            //C->S->P->
                            result = store.SelectQuadruples((RDFContext)pattern.Context, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, null);
                            AddColumn(resultTable, pattern.Object.ToString());
                            PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.O, resultTable);
                        }
                        else
                        {
                            if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                            {
                                //C->S->->O
                                if (pattern.Object is RDFResource)
                                {
                                    result = store.SelectQuadruples((RDFContext)pattern.Context, (RDFResource)pattern.Subject, null, (RDFResource)pattern.Object, null);
                                }
                                //C->S->->L
                                else
                                {
                                    result = store.SelectQuadruples((RDFContext)pattern.Context, (RDFResource)pattern.Subject, null, null, (RDFLiteral)pattern.Object);
                                }
                                AddColumn(resultTable, pattern.Predicate.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.P, resultTable);
                            }
                            else
                            {
                                //C->S->->
                                result = store.SelectQuadruples((RDFContext)pattern.Context, (RDFResource)pattern.Subject, null, null, null);
                                //In case of same P and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Predicate.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                                }
                                AddColumn(resultTable, pattern.Predicate.ToString());
                                AddColumn(resultTable, pattern.Object.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
                            }
                        }
                    }
                    else
                    {
                        if (pattern.Predicate is RDFResource)
                        {
                            if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                            {
                                //C->->P->O
                                if (pattern.Object is RDFResource)
                                {
                                    result = store.SelectQuadruples((RDFContext)pattern.Context, null, (RDFResource)pattern.Predicate, (RDFResource)pattern.Object, null);
                                }
                                //C->->P->L
                                else
                                {
                                    result = store.SelectQuadruples((RDFContext)pattern.Context, null, (RDFResource)pattern.Predicate, null, (RDFLiteral)pattern.Object);
                                }
                                AddColumn(resultTable, pattern.Subject.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.S, resultTable);
                            }
                            else
                            {
                                //C->->P->
                                result = store.SelectQuadruples((RDFContext)pattern.Context, null, (RDFResource)pattern.Predicate, null, null);
                                //In case of same S and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                                }
                                AddColumn(resultTable, pattern.Subject.ToString());
                                AddColumn(resultTable, pattern.Object.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
                            }
                        }
                        else
                        {
                            if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                            {
                                //C->->->O
                                if (pattern.Object is RDFResource)
                                {
                                    result = store.SelectQuadruples((RDFContext)pattern.Context, null, null, (RDFResource)pattern.Object, null);
                                }
                                //C->->->L
                                else
                                {
                                    result = store.SelectQuadruples((RDFContext)pattern.Context, null, null, null, (RDFLiteral)pattern.Object);
                                }
                                //In case of same S and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Predicate))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                                }
                                AddColumn(resultTable, pattern.Subject.ToString());
                                AddColumn(resultTable, pattern.Predicate.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
                            }
                            else
                            {
                                //C->->->
                                result = store.SelectQuadruples((RDFContext)pattern.Context, null, null, null, null);
                                //In case of same S and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Predicate))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same S and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                                }
                                //In case of same P and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Predicate.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                                }
                                AddColumn(resultTable, pattern.Subject.ToString());
                                AddColumn(resultTable, pattern.Predicate.ToString());
                                AddColumn(resultTable, pattern.Object.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
                            }
                        }
                    }
                }
                else
                {
                    if (pattern.Subject is RDFResource)
                    {
                        if (pattern.Predicate is RDFResource)
                        {
                            if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                            {
                                //->S->P->O
                                if (pattern.Object is RDFResource)
                                {
                                    result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, (RDFResource)pattern.Object, null);
                                }
                                //->S->P->L
                                else
                                {
                                    result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, (RDFLiteral)pattern.Object);
                                }
                                AddColumn(resultTable, pattern.Context.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.C, resultTable);
                            }
                            else
                            {
                                //->S->P->
                                result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, null);
                                //In case of same C and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                                }
                                AddColumn(resultTable, pattern.Context.ToString());
                                AddColumn(resultTable, pattern.Object.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CO, resultTable);
                            }
                        }
                        else
                        {
                            if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                            {
                                //->S->->O
                                if (pattern.Object is RDFResource)
                                {
                                    result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, (RDFResource)pattern.Object, null);
                                }
                                //->S->->L
                                else
                                {
                                    result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, null, (RDFLiteral)pattern.Object);
                                }
                                //In case of same C and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Predicate))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                                }
                                AddColumn(resultTable, pattern.Context.ToString());
                                AddColumn(resultTable, pattern.Predicate.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CP, resultTable);
                            }
                            else
                            {
                                //->S->->
                                result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, null, null);
                                //In case of same C and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Predicate))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same C and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                                }
                                //In case of same P and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Predicate.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                                }
                                AddColumn(resultTable, pattern.Context.ToString());
                                AddColumn(resultTable, pattern.Predicate.ToString());
                                AddColumn(resultTable, pattern.Object.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CPO, resultTable);
                            }
                        }
                    }
                    else
                    {
                        if (pattern.Predicate is RDFResource)
                        {
                            if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                            {
                                //->->P->O
                                if (pattern.Object is RDFResource)
                                {
                                    result = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, (RDFResource)pattern.Object, null);
                                }
                                //->->P->L
                                else
                                {
                                    result = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, null, (RDFLiteral)pattern.Object);
                                }
                                //In case of same C and S variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Subject))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                                }
                                AddColumn(resultTable, pattern.Context.ToString());
                                AddColumn(resultTable, pattern.Subject.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CS, resultTable);
                            }
                            else
                            {
                                //->->P->
                                result = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, null, null);
                                //In case of same C and S variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Subject))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                                }
                                //In case of same C and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                                }
                                //In case of same S and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                                }
                                AddColumn(resultTable, pattern.Context.ToString());
                                AddColumn(resultTable, pattern.Subject.ToString());
                                AddColumn(resultTable, pattern.Object.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSO, resultTable);
                            }
                        }
                        else
                        {
                            if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                            {
                                //->->->O
                                if (pattern.Object is RDFResource)
                                {
                                    result = store.SelectQuadruples(null, null, null, (RDFResource)pattern.Object, null);
                                }
                                //->->->L
                                else
                                {
                                    result = store.SelectQuadruples(null, null, null, null, (RDFLiteral)pattern.Object);
                                }
                                //In case of same C and S variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Subject))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                                }
                                //In case of same C and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Predicate))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same S and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Predicate))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                                }
                                AddColumn(resultTable, pattern.Context.ToString());
                                AddColumn(resultTable, pattern.Subject.ToString());
                                AddColumn(resultTable, pattern.Predicate.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSP, resultTable);
                            }
                            else
                            {
                                //->->->
                                result = store.SelectQuadruples(null, null, null, null, null);
                                //In case of same C and S variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Subject))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                                }
                                //In case of same C and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Predicate))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same C and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Context.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                                }
                                //In case of same S and P variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Predicate))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                                }
                                //In case of same S and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Subject.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                                }
                                //In case of same P and O variable, must refine matching quadruples with a further value comparison
                                if (pattern.Predicate.Equals(pattern.Object))
                                {
                                    result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                                }
                                AddColumn(resultTable, pattern.Context.ToString());
                                AddColumn(resultTable, pattern.Subject.ToString());
                                AddColumn(resultTable, pattern.Predicate.ToString());
                                AddColumn(resultTable, pattern.Object.ToString());
                                PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.CSPO, resultTable);
                            }
                        }
                    }
                }
            }

            //SPO pattern
            else
            {
                if (pattern.Subject is RDFResource)
                {
                    if (pattern.Predicate is RDFResource)
                    {
                        //S->P->
                        result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, null);
                        AddColumn(resultTable, pattern.Object.ToString());
                        PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.O, resultTable);
                    }
                    else
                    {
                        if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                        {
                            //S->->O
                            if (pattern.Object is RDFResource)
                            {
                                result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, (RDFResource)pattern.Object, null);
                            }
                            //S->->L
                            else
                            {
                                result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, null, (RDFLiteral)pattern.Object);
                            }
                            AddColumn(resultTable, pattern.Predicate.ToString());
                            PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.P, resultTable);
                        }
                        else
                        {
                            //S->->
                            result = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, null, null);
                            //In case of same P and O variable, must refine matching quadruples with a further value comparison
                            if (pattern.Predicate.Equals(pattern.Object))
                            {
                                result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                            }
                            AddColumn(resultTable, pattern.Predicate.ToString());
                            AddColumn(resultTable, pattern.Object.ToString());
                            PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.PO, resultTable);
                        }
                    }
                }
                else
                {
                    if (pattern.Predicate is RDFResource)
                    {
                        if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                        {
                            //->P->O
                            if (pattern.Object is RDFResource)
                            {
                                result = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, (RDFResource)pattern.Object, null);
                            }
                            //->P->L
                            else
                            {
                                result = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, null, (RDFLiteral)pattern.Object);
                            }
                            AddColumn(resultTable, pattern.Subject.ToString());
                            PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.S, resultTable);
                        }
                        else
                        {
                            //->P->
                            result = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, null, null);
                            //In case of same S and O variable, must refine matching quadruples with a further value comparison
                            if (pattern.Subject.Equals(pattern.Object))
                            {
                                result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                            }
                            AddColumn(resultTable, pattern.Subject.ToString());
                            AddColumn(resultTable, pattern.Object.ToString());
                            PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SO, resultTable);
                        }
                    }
                    else
                    {
                        if (pattern.Object is RDFResource || pattern.Object is RDFLiteral)
                        {
                            //->->O
                            if (pattern.Object is RDFResource)
                            {
                                result = store.SelectQuadruples(null, null, null, (RDFResource)pattern.Object, null);
                            }
                            //->->L
                            else
                            {
                                result = store.SelectQuadruples(null, null, null, null, (RDFLiteral)pattern.Object);
                            }
                            //In case of same S and P variable, must refine matching quadruples with a further value comparison
                            if (pattern.Subject.Equals(pattern.Predicate))
                            {
                                result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                            }
                            AddColumn(resultTable, pattern.Subject.ToString());
                            AddColumn(resultTable, pattern.Predicate.ToString());
                            PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SP, resultTable);
                        }
                        else
                        {
                            //->->
                            result = store.SelectQuadruples(null, null, null, null, null);
                            //In case of same S and P variable, must refine matching quadruples with a further value comparison
                            if (pattern.Subject.Equals(pattern.Predicate))
                            {
                                result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                            }
                            //In case of same S and O variable, must refine matching quadruples with a further value comparison
                            if (pattern.Subject.Equals(pattern.Object))
                            {
                                result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                            }
                            //In case of same P and O variable, must refine matching quadruples with a further value comparison
                            if (pattern.Predicate.Equals(pattern.Object))
                            {
                                result = new RDFMemoryStore(result.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                            }
                            AddColumn(resultTable, pattern.Subject.ToString());
                            AddColumn(resultTable, pattern.Predicate.ToString());
                            AddColumn(resultTable, pattern.Object.ToString());
                            PopulateTable(pattern, result, RDFQueryEnums.RDFPatternHoles.SPO, resultTable);
                        }
                    }
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given federation
        /// </summary>
        internal DataTable ApplyPattern(RDFPattern pattern, RDFFederation federation)
        {
            DataTable resultTable = new DataTable();

            //Iterate data sources of the federation
            foreach (RDFDataSource dataSource in federation)
            {
                switch (dataSource)
                {
                    case RDFGraph dataSourceGraph:
                        DataTable graphTable = ApplyPattern(pattern, dataSourceGraph);
                        resultTable.Merge(graphTable, true, MissingSchemaAction.Add);
                        break;

                    case RDFStore dataSourceStore:
                        DataTable storeTable = ApplyPattern(pattern, dataSourceStore);
                        resultTable.Merge(storeTable, true, MissingSchemaAction.Add);
                        break;

                    case RDFFederation dataSourceFederation:
                        DataTable federationTable = ApplyPattern(pattern, dataSourceFederation);
                        resultTable.Merge(federationTable, true, MissingSchemaAction.Add);
                        break;

                    case RDFSPARQLEndpoint dataSourceSparqlEndpoint:
                        DataTable sparqlEndpointTable = ApplyPattern(pattern, dataSourceSparqlEndpoint);
                        resultTable.Merge(sparqlEndpointTable, true, MissingSchemaAction.Add);
                        break;
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given federation
        /// </summary>
        internal DataTable ApplyPattern(RDFPattern pattern, RDFSPARQLEndpoint sparqlEndpoint)
        {
            //Transform the pattern into an equivalent "SELECT *" query
            RDFSelectQuery selectQuery =
                new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup("FEDQUERY")
                        .AddPattern(pattern));

            //Apply the query to the SPARQL endpoint
            RDFSelectQueryResult selectQueryResult = selectQuery.ApplyToSPARQLEndpoint(sparqlEndpoint);

            //Eventually adjust variable names (should start with "?")
            int columnsCount = selectQueryResult.SelectResults.Columns.Count;
            for (int i = 0; i < columnsCount; i++)
            {
                if (!selectQueryResult.SelectResults.Columns[i].ColumnName.StartsWith("?"))
                    selectQueryResult.SelectResults.Columns[i].ColumnName = string.Concat("?", selectQueryResult.SelectResults.Columns[i].ColumnName);
            }

            return selectQueryResult.SelectResults;
        }

        /// <summary>
        /// Applies the given property path to the given graph
        /// </summary>
        internal DataTable ApplyPropertyPath(RDFPropertyPath propertyPath, RDFDataSource dataSource)
        {
            DataTable resultTable = new DataTable();

            //Translate property path into equivalent list of patterns
            List<RDFPattern> patternList = propertyPath.GetPatternList();

            //Evaluate produced list of patterns
            List<DataTable> patternTables = new List<DataTable>();
            foreach (RDFPattern pattern in patternList)
            {
                //Apply pattern to graph
                DataTable patternTable = ApplyPattern(pattern, dataSource);

                //Set extended properties
                patternTable.ExtendedProperties.Add("IsOptional", pattern.IsOptional);
                patternTable.ExtendedProperties.Add("JoinAsUnion", pattern.JoinAsUnion);

                //Add produced table
                patternTables.Add(patternTable);
            }

            //Merge produced list of tables
            resultTable = CombineTables(patternTables, false);

            //Remove property path variables
            List<string> propPathCols = new List<string>();
            foreach (DataColumn dtCol in resultTable.Columns)
            {
                if (dtCol.ColumnName.StartsWith("?__PP"))
                    propPathCols.Add(dtCol.ColumnName);
            }
            propPathCols.ForEach(ppc => resultTable.Columns.Remove(ppc));

            resultTable.TableName = propertyPath.ToString();
            return resultTable;
        }
        #endregion

        #region MIRELLA TABLES
        /// <summary>
        /// Utility class for comparison between data columns
        /// </summary>
        internal class DataColumnComparer : IEqualityComparer<DataColumn>
        {
            #region Methods
            public bool Equals(DataColumn column1, DataColumn column2)
            {
                if (column1 != null)
                    return column2 != null && column1.ColumnName.Equals(column2.ColumnName, StringComparison.Ordinal);

                return column2 == null;
            }

            public int GetHashCode(DataColumn column)
                => column.ColumnName.GetHashCode();
            #endregion
        }
        /// <summary>
        /// Static instance of the comparer used by the engine to compare data columns
        /// </summary>
        internal static readonly DataColumnComparer dtComparer = new DataColumnComparer();

        /// <summary>
        /// Adds a new column to the given table, avoiding duplicates
        /// </summary>
        internal static void AddColumn(DataTable table, string columnName)
        {
            string colName = columnName.Trim().ToUpperInvariant();
            if (!table.Columns.Contains(colName))
                table.Columns.Add(colName, SystemString);
        }

        /// <summary>
        /// Adds a new row to the given table
        /// </summary>
        internal static void AddRow(DataTable table, Dictionary<string, string> bindings)
        {
            bool rowAdded = false;

            DataRow resultRow = table.NewRow();
            foreach (string bindingKey in bindings.Keys)
            {
                if (table.Columns.Contains(bindingKey))
                {
                    resultRow[bindingKey] = bindings[bindingKey];
                    rowAdded = true;
                }
            }

            if (rowAdded)
                table.Rows.Add(resultRow);
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given graph
        /// </summary>
        internal void PopulateTable(RDFPattern pattern, List<RDFTriple> triples, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable)
        {
            Dictionary<string, string> bindings = new Dictionary<string, string>();

            //Iterate result graph's triples
            foreach (RDFTriple t in triples)
            {
                switch (patternHole)
                {
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
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                            bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        break;
                    //->P->
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        break;
                    //S->->
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        break;
                    //->->
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                            bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        break;
                }
                AddRow(resultTable, bindings);
                bindings.Clear();
            }
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given store
        /// </summary>
        internal void PopulateTable(RDFPattern pattern, RDFMemoryStore store, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable)
        {
            Dictionary<string, string> bindings = new Dictionary<string, string>();

            //Iterate result store's quadruples
            foreach (RDFQuadruple q in store)
            {
                switch (patternHole)
                {
                    //->S->P->O
                    case RDFQueryEnums.RDFPatternHoles.C:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        break;
                    //->->P->O
                    case RDFQueryEnums.RDFPatternHoles.CS:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString()))
                            bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        break;
                    //C->->P->O
                    case RDFQueryEnums.RDFPatternHoles.S:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        break;
                    //->S->->O
                    case RDFQueryEnums.RDFPatternHoles.CP:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        break;
                    //C->S->->O
                    case RDFQueryEnums.RDFPatternHoles.P:
                        bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        break;
                    //->S->P->
                    case RDFQueryEnums.RDFPatternHoles.CO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //C->S->P->
                    case RDFQueryEnums.RDFPatternHoles.O:
                        bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //->->->O
                    case RDFQueryEnums.RDFPatternHoles.CSP:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString()))
                            bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        break;
                    //C->->->O
                    case RDFQueryEnums.RDFPatternHoles.SP:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        break;
                    //->->P->
                    case RDFQueryEnums.RDFPatternHoles.CSO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString()))
                            bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //C->->P->
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //->S->->
                    case RDFQueryEnums.RDFPatternHoles.CPO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //C->S->->
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //->->->
                    case RDFQueryEnums.RDFPatternHoles.CSPO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString()))
                            bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                    //C->->->
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        break;
                }
                AddRow(resultTable, bindings);
                bindings.Clear();
            }
        }

        /// <summary>
        /// Joins two datatables WITHOUT support for OPTIONAL data
        /// </summary>
        internal DataTable InnerJoinTables(DataTable dt1, DataTable dt2)
        {
            DataTable result = new DataTable();
            DataColumn[] dt1Columns = dt1.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] dt2Columns = dt2.Columns.OfType<DataColumn>().ToArray();

            //Determine common columns
            DataColumn[] commonColumns = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                   .Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                   .ToArray();

            //PRODUCT-JOIN
            if (commonColumns.Length == 0)
            {
                //Create the structure of the product table
                result.Columns.AddRange(dt1Columns.Union(dt2Columns, dtComparer)
                              .Select(c => new DataColumn(c.ColumnName, c.DataType))
                              .ToArray());

                //Loop through dt1 table
                result.AcceptChanges();
                result.BeginLoadData();
                foreach (DataRow parentRow in dt1.Rows)
                {
                    object[] firstArray = parentRow.ItemArray;

                    //Loop through dt2 table
                    foreach (DataRow childRow in dt2.Rows)
                    {
                        object[] secondArray = childRow.ItemArray;
                        object[] productArray = new object[firstArray.Length + secondArray.Length];
                        Array.Copy(firstArray, 0, productArray, 0, firstArray.Length);
                        Array.Copy(secondArray, 0, productArray, firstArray.Length, secondArray.Length);
                        result.LoadDataRow(productArray, true);
                    }
                }
                result.EndLoadData();
            }

            //INNER-JOIN
            else
            {
                //Use a DataSet to leverage a relation linking the common columns
                using (DataSet ds = new DataSet())
                {
                    //Add copy of the tables to the dataset
                    ds.Tables.AddRange(new DataTable[] { dt1, dt2 });

                    //Identify join columns from dt1
                    DataColumn[] parentColumns = new DataColumn[commonColumns.Length];
                    for (int i = 0; i < parentColumns.Length; i++)
                        parentColumns[i] = ds.Tables[0].Columns[commonColumns[i].ColumnName];

                    //Identify join columns from dt2
                    DataColumn[] childColumns = new DataColumn[commonColumns.Length];
                    for (int i = 0; i < childColumns.Length; i++)
                        childColumns[i] = ds.Tables[1].Columns[commonColumns[i].ColumnName];

                    //Create the relation linking the common columns
                    DataRelation r = new DataRelation("JoinRelation", parentColumns, childColumns, false);
                    ds.Relations.Add(r);

                    //Create the structure of the join table
                    List<string> duplicateCols = new List<string>();
                    for (int i = 0; i < ds.Tables[0].Columns.Count; i++)
                        result.Columns.Add(ds.Tables[0].Columns[i].ColumnName, ds.Tables[0].Columns[i].DataType);
                    for (int i = 0; i < ds.Tables[1].Columns.Count; i++)
                    {
                        if (!result.Columns.Contains(ds.Tables[1].Columns[i].ColumnName))
                            result.Columns.Add(ds.Tables[1].Columns[i].ColumnName, ds.Tables[1].Columns[i].DataType);
                        else
                        {
                            //Manage duplicate columns by appending a known identificator to their name
                            string duplicateColKey = string.Concat(ds.Tables[1].Columns[i].ColumnName, "_DUPLICATE_");
                            result.Columns.Add(duplicateColKey, ds.Tables[1].Columns[i].DataType);
                            duplicateCols.Add(duplicateColKey);
                        }
                    }

                    //Loop through dt1 table
                    result.AcceptChanges();
                    result.BeginLoadData();
                    foreach (DataRow firstRow in ds.Tables[0].Rows)
                    {

                        //Get "joined" dt2 rows by exploiting the leveraged relation
                        DataRow[] childRows = firstRow.GetChildRows(r);
                        if (childRows.Length > 0)
                        {
                            object[] parentArray = firstRow.ItemArray;
                            foreach (DataRow secondRow in childRows)
                            {
                                object[] secondArray = secondRow.ItemArray;
                                object[] joinArray = new object[parentArray.Length + secondArray.Length];
                                Array.Copy(parentArray, 0, joinArray, 0, parentArray.Length);
                                Array.Copy(secondArray, 0, joinArray, parentArray.Length, secondArray.Length);
                                result.LoadDataRow(joinArray, true);
                            }
                        }

                    }
                    //Eliminate the duplicated columns from the result table
                    duplicateCols.ForEach(c => result.Columns.Remove(c));
                    result.EndLoadData();
                }
            }

            return result;
        }

        /// <summary>
        /// Joins two datatables WITH support for OPTIONAL data
        /// </summary>
        internal DataTable OuterJoinTables(DataTable dt1, DataTable dt2)
        {
            DataTable finalResult = new DataTable();
            DataColumn[] dt1Columns = dt1.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] dt2Columns = dt2.Columns.OfType<DataColumn>().ToArray();

            bool dt2IsOptionalTable = (dt2.ExtendedProperties.ContainsKey("IsOptional") && dt2.ExtendedProperties["IsOptional"].Equals(true));
            bool joinInvalidationFlag = false;
            bool foundAnyResult = false;

            //Determine common columns
            DataColumn[] commonColumns = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                   .Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                   .ToArray();

            //Create structure of finalResult table
            finalResult.Columns.AddRange(dt1Columns.Union(dt2Columns.Except(commonColumns), dtComparer)
                               .Select(c => new DataColumn(c.ColumnName, c.DataType))
                               .ToArray());

            //Calculate finalResult columns attribution
            Dictionary<string, (bool,string)> finalResColumnsAttribution = new Dictionary<string, (bool,string)>();
            foreach (DataColumn finalResCol in finalResult.Columns)
            {
                //COMMON attribution
                bool commonAttribution = false;
                if (commonColumns.Any(col => col.ColumnName.Equals(finalResCol.ColumnName, StringComparison.Ordinal)))
                    commonAttribution = true;

                //DT attribution
                string dtAttribution = "DT2";
                if (dt1Columns.Any(col => col.ColumnName.Equals(finalResCol.ColumnName, StringComparison.Ordinal)))
                    dtAttribution = "DT1";

                finalResColumnsAttribution.Add(finalResCol.ColumnName, (commonAttribution, dtAttribution));
            }

            //Loop through dt1 table
            finalResult.AcceptChanges();
            finalResult.BeginLoadData();
            foreach (DataRow leftRow in dt1.Rows)
            {
                foundAnyResult = false;

                //Loop through dt2 table
                foreach (DataRow rightRow in dt2.Rows)
                {
                    joinInvalidationFlag = false;

                    //Create a temporary join row
                    DataRow joinRow = finalResult.NewRow();
                    foreach (DataColumn finalResCol in finalResult.Columns)
                    {
                        //NON-COMMON column
                        if (!finalResColumnsAttribution[finalResCol.ColumnName].Item1)
                        {
                            //Take value from left
                            if (finalResColumnsAttribution[finalResCol.ColumnName].Item2 == "DT1")
                                joinRow[finalResCol.ColumnName] = leftRow[finalResCol.ColumnName];

                            //Take value from right
                            else
                                joinRow[finalResCol.ColumnName] = rightRow[finalResCol.ColumnName];
                        }

                        //COMMON column
                        else
                        {
                            //Left value is NULL
                            if (leftRow.IsNull(finalResCol.ColumnName))
                            {
                                //Right value is NULL
                                if (rightRow.IsNull(finalResCol.ColumnName))
                                {
                                    //Take NULL value
                                    joinRow[finalResCol.ColumnName] = DBNull.Value;
                                }

                                //Right value is NOT NULL
                                else
                                {
                                    //Take value from right
                                    joinRow[finalResCol.ColumnName] = rightRow[finalResCol.ColumnName];
                                }
                            }

                            //Left value is NOT NULL
                            else
                            {
                                //Right value is NULL
                                if (rightRow.IsNull(finalResCol.ColumnName))
                                {
                                    //Take value from left
                                    joinRow[finalResCol.ColumnName] = leftRow[finalResCol.ColumnName];
                                }

                                //Right value is NOT NULL
                                else
                                {
                                    //Left value is EQUAL TO right value
                                    if (leftRow[finalResCol.ColumnName].ToString().Equals(rightRow[finalResCol.ColumnName].ToString(), StringComparison.Ordinal))
                                    {
                                        //Take value from left (it's the same)
                                        joinRow[finalResCol.ColumnName] = leftRow[finalResCol.ColumnName];
                                    }

                                    //Left value is NOT EQUAL TO right value
                                    else
                                    {
                                        //Raise the join invalidation flag
                                        joinInvalidationFlag = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //Add join row to finalResults table
                    if (!joinInvalidationFlag)
                    {
                        joinRow.AcceptChanges();
                        finalResult.Rows.Add(joinRow);
                        foundAnyResult = true;
                    }
                }

                //Manage presence of "OPTIONAL" pattern to the right
                if (!foundAnyResult && dt2IsOptionalTable)
                {
                    //In this case, the left row must be kept anyway and other columns from right are NULL
                    DataRow optionalRow = finalResult.NewRow();
                    optionalRow.ItemArray = leftRow.ItemArray;
                    optionalRow.AcceptChanges();
                    finalResult.Rows.Add(optionalRow);
                }
            }
            finalResult.EndLoadData();

            return finalResult;
        }

        /// <summary>
        /// Combines the given list of data tables, depending on presence of common columns and dynamic detection of Optional / Union operators
        /// </summary>
        internal DataTable CombineTables(List<DataTable> dataTables, bool isMerge)
        {
            DataTable finalTable = new DataTable();

            bool switchToOuterJoin = false;
            if (dataTables.Count > 0)
            {
                //Process Unions
                for (int i = 1; i < dataTables.Count; i++)
                {
                    if (isMerge || (dataTables[i - 1].ExtendedProperties.ContainsKey("JoinAsUnion") && dataTables[i - 1].ExtendedProperties["JoinAsUnion"].Equals(true)))
                    {
                        //Merge the previous table into the current one
                        dataTables[i].Merge(dataTables[i - 1], true, MissingSchemaAction.Add);

                        //Clear the previous table and flag it as logically deleted
                        dataTables[i - 1].Rows.Clear();
                        dataTables[i - 1].ExtendedProperties.Add("LogicallyDeleted", true);

                        //Set automatic switch to OuterJoin (because we have done Unions, so null values must be preserved)
                        switchToOuterJoin = true;
                    }
                }
                dataTables.RemoveAll(dt => dt.ExtendedProperties.ContainsKey("LogicallyDeleted") && dt.ExtendedProperties["LogicallyDeleted"].Equals(true));

                //Process Joins
                finalTable = dataTables[0];
                for (int i = 1; i < dataTables.Count; i++)
                {
                    //Set automatic switch to OuterJoin in case of relevant "Optional" detected
                    switchToOuterJoin = (switchToOuterJoin || (dataTables[i].ExtendedProperties.ContainsKey("IsOptional") && dataTables[i].ExtendedProperties["IsOptional"].Equals(true)));

                    //Support OPTIONAL data
                    if (switchToOuterJoin)
                        finalTable = OuterJoinTables(finalTable, dataTables[i]);

                    //Do not support OPTIONAL data
                    else
                        finalTable = InnerJoinTables(finalTable, dataTables[i]);
                }
            }

            return finalTable;
        }

        /// <summary>
        /// Applies the projection operator on the given table, based on the given query's projection variables
        /// </summary>
        internal DataTable ProjectTable(RDFSelectQuery query, DataTable table)
        {
            if (query.ProjectionVars.Any())
            {
                //Remove non-projection variables
                List<DataColumn> nonProjCols = new List<DataColumn>();
                foreach (DataColumn dtCol in table.Columns)
                {
                    if (!query.ProjectionVars.Any(pv => pv.Key.ToString().Equals(dtCol.ColumnName, StringComparison.OrdinalIgnoreCase)))
                        nonProjCols.Add(dtCol);
                }
                nonProjCols.ForEach(npc => table.Columns.Remove(npc.ColumnName));

                //Adjust ordinals
                foreach (var pVar in query.ProjectionVars)
                {
                    AddColumn(table, pVar.Key.ToString());
                    table.Columns[pVar.Key.ToString()].SetOrdinal(pVar.Value);
                }
            }
            return table;
        }
        #endregion

        #endregion
    }
}