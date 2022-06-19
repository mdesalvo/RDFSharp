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
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryEngine is the engine for execution of SPARQL queries (MIRELLA)
    /// </summary>
    internal class RDFQueryEngine
    {
        #region Properties
        /// <summary>
        /// Dictionary of result tables produced by evaluation of patternGroup members
        /// </summary>
        internal Dictionary<long, List<DataTable>> PatternGroupMemberResultTables { get; set; }

        /// <summary>
        /// Dictionary of result tables produced by evaluation of query members
        /// </summary>
        internal Dictionary<long, DataTable> QueryMemberResultTables { get; set; }

        /// <summary>
        /// Default column type used for Mirella tables
        /// </summary>
        internal static readonly Type SystemString = typeof(string); 

        /// <summary>
        /// Attribute denoting an optional pattern/patternGroup/query
        /// </summary>
        internal static readonly string IsOptional = "IsOptional";

        /// <summary>
        /// Attribute denoting a pattern/patternGroup/query to be joined as union
        /// </summary>
        internal static readonly string JoinAsUnion = "JoinAsUnion";

        /// <summary>
        /// Attribute denoting a logically deleted intermediate results table
        /// </summary>
        internal static readonly string LogicallyDeleted = "LogicallyDeleted";
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize a query engine instance
        /// </summary>
        internal RDFQueryEngine()
        {
            this.PatternGroupMemberResultTables = new Dictionary<long, List<DataTable>>();
            this.QueryMemberResultTables = new Dictionary<long, DataTable>();
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
                DataTable queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList(), false);

                //Apply the modifiers of the query to the result table
                queryResult.SelectResults = ApplyModifiers(selectQuery, queryResultTable);
            }

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
                    resultTable = DescribeTerms(describeQuery, datasource, qResultTable);

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
                queryResultTable = CombineTables(this.QueryMemberResultTables.Values.ToList(), false);
            }

            //Describe the terms from the result table
            DataTable describeResultTable = FillDescribeTerms(queryResultTable);

            //Apply the modifiers of the query to the result table
            queryResult.DescribeResults = ApplyModifiers(describeQuery, describeResultTable);

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
                queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList(), false);
            }

            //Fill the templates from the result table
            DataTable filledResultTable = FillTemplates(constructQuery.Templates, queryResultTable, false);

            //Apply the modifiers of the query to the result table
            constructResult.ConstructResults = ApplyModifiers(constructQuery, filledResultTable);

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
                DataTable queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList(), false);

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
                    //Get the intermediate result tables of the pattern group
                    EvaluatePatternGroup(patternGroup, datasource);

                    //Get the result table of the pattern group
                    FinalizePatternGroup(patternGroup);

                    //Apply the filters of the pattern group to its result table
                    ApplyFilters(patternGroup);
                }
                #endregion

                #region SUBQUERY
                else if (evaluableQueryMember is RDFSelectQuery subQuery)
                {
                    //Get the result table of the subquery
                    RDFSelectQueryResult subQueryResult = subQuery.ApplyToDataSource(datasource);
                    if (!QueryMemberResultTables.ContainsKey(subQuery.QueryMemberID))
                    {
                        //Populate its name
                        QueryMemberResultTables.Add(subQuery.QueryMemberID, subQueryResult.SelectResults);

                        //Populate its metadata (IsOptional)
                        if (!QueryMemberResultTables[subQuery.QueryMemberID].ExtendedProperties.ContainsKey(IsOptional))
                            QueryMemberResultTables[subQuery.QueryMemberID].ExtendedProperties.Add(IsOptional, subQuery.IsOptional);
                        else
                            QueryMemberResultTables[subQuery.QueryMemberID].ExtendedProperties[IsOptional] = subQuery.IsOptional
                                                                                                                || (bool)QueryMemberResultTables[subQuery.QueryMemberID].ExtendedProperties[IsOptional];

                        //Populate its metadata (JoinAsUnion)
                        if (!QueryMemberResultTables[subQuery.QueryMemberID].ExtendedProperties.ContainsKey(JoinAsUnion))
                            QueryMemberResultTables[subQuery.QueryMemberID].ExtendedProperties.Add(JoinAsUnion, subQuery.JoinAsUnion);
                        else
                            QueryMemberResultTables[subQuery.QueryMemberID].ExtendedProperties[JoinAsUnion] = subQuery.JoinAsUnion;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Gets the intermediate result tables of the given pattern group
        /// </summary>
        internal void EvaluatePatternGroup(RDFPatternGroup patternGroup, RDFDataSource dataSource)
        {
            PatternGroupMemberResultTables[patternGroup.QueryMemberID] = new List<DataTable>();

            //Iterate the evaluable members of the pattern group
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().Distinct().ToList();
            foreach (RDFPatternGroupMember evaluablePGMember in evaluablePGMembers)
            {
                #region Pattern
                if (evaluablePGMember is RDFPattern pattern)
                {
                    DataTable patternResultsTable = ApplyPattern(pattern, dataSource);

                    //Set name and metadata of result datatable
                    patternResultsTable.ExtendedProperties.Add(IsOptional, pattern.IsOptional);
                    patternResultsTable.ExtendedProperties.Add(JoinAsUnion, pattern.JoinAsUnion);

                    //Save result datatable
                    PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(patternResultsTable);
                }
                #endregion

                #region PropertyPath
                else if (evaluablePGMember is RDFPropertyPath propertyPath)
                {
                    DataTable pPathResultsTable = ApplyPropertyPath(propertyPath, dataSource);

                    //Save result datatable
                    PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(pPathResultsTable);
                }
                #endregion

                #region Values
                else if (evaluablePGMember is RDFValues values)
                {
                    //Transform SPARQL values into an equivalent filter
                    RDFValuesFilter valuesFilter = values.GetValuesFilter();
                    
                    //Save result datatable
                    PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(valuesFilter.ValuesTable);

                    //Inject SPARQL values filter
                    patternGroup.AddFilter(valuesFilter);
                }
                #endregion

                #region Filter (Exists/Not Exists)
                else if (evaluablePGMember is RDFExistsFilter existsFilter)
                {
                    DataTable existsFilterResultsTable = ApplyPattern(existsFilter.Pattern, dataSource);

                    //Set name and metadata of result datatable
                    existsFilterResultsTable.ExtendedProperties.Add(IsOptional, false);
                    existsFilterResultsTable.ExtendedProperties.Add(JoinAsUnion, false);

                    //Save result datatable (directly into the filter)
                    existsFilter.PatternResults?.Clear();
                    existsFilter.PatternResults = existsFilterResultsTable;
                }
                #endregion
            }
        }

        /// <summary>
        /// Gets the final result table of the given pattern group
        /// </summary>
        internal void FinalizePatternGroup(RDFPatternGroup patternGroup)
        {
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            if (evaluablePGMembers.Any())
            {
                //Populate patternGroup result table
                DataTable patternGroupResultTable = CombineTables(PatternGroupMemberResultTables[patternGroup.QueryMemberID], false);

                //Add it to the list of query member result tables
                QueryMemberResultTables.Add(patternGroup.QueryMemberID, patternGroupResultTable);

                //Populate its metadata (IsOptional)
                if (!QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties.ContainsKey(IsOptional))
                    QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties.Add(IsOptional, patternGroup.IsOptional);
                else
                    QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties[IsOptional] = patternGroup.IsOptional
                                                                                                            || (bool)QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties[IsOptional];

                //Populate its metadata (JoinAsUnion)
                if (!QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties.ContainsKey(JoinAsUnion))
                    QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties.Add(JoinAsUnion, patternGroup.JoinAsUnion);
                else
                    QueryMemberResultTables[patternGroup.QueryMemberID].ExtendedProperties[JoinAsUnion] = patternGroup.JoinAsUnion;
            }
        }

        /// <summary>
        /// Applies the filters of the given pattern group to its result table
        /// </summary>
        internal void ApplyFilters(RDFPatternGroup patternGroup)
        {
            List<RDFPatternGroupMember> evaluablePatternGroupMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            List<RDFFilter> filters = patternGroup.GetFilters().ToList();
            if (evaluablePatternGroupMembers.Any() && filters.Any())
            {
                DataTable filteredTable = QueryMemberResultTables[patternGroup.QueryMemberID].Clone();
                IEnumerator rowsEnum = QueryMemberResultTables[patternGroup.QueryMemberID].Rows.GetEnumerator();

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
                QueryMemberResultTables[patternGroup.QueryMemberID] = filteredTable;
            }
        }

        /// <summary>
        /// Applies the query modifiers to the query result table
        /// </summary>
        internal DataTable ApplyModifiers(RDFQuery query, DataTable table)
        {
            #region GROUPBY/ORDERBY/PROJECTION
            List<RDFModifier> modifiers = query.GetModifiers().ToList();
            if (query is RDFSelectQuery selectQuery)
            {
                #region GROUPBY
                RDFGroupByModifier groupbyModifier = modifiers.OfType<RDFGroupByModifier>().SingleOrDefault();
                if (groupbyModifier != null)
                {
                    table = groupbyModifier.ApplyModifier(table);

                    #region PROJECTION
                    selectQuery.ProjectionVars.Clear();
                    groupbyModifier.PartitionVariables.ForEach(pv => selectQuery.AddProjectionVariable(pv));
                    groupbyModifier.Aggregators.ForEach(ag => selectQuery.AddProjectionVariable(ag.ProjectionVariable));
                    #endregion
                }
                #endregion

                #region ORDERBY
                IEnumerable<RDFOrderByModifier> orderbyModifiers = modifiers.OfType<RDFOrderByModifier>();
                if (orderbyModifiers.Any())
                {
                    table = orderbyModifiers.Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));
                    table = table.DefaultView.ToTable();
                }
                #endregion

                #region PROJECTION
                table = ProjectTable(selectQuery, table);
                #endregion
            }
            #endregion

            #region DISTINCT
            RDFDistinctModifier distinctModifier = modifiers.OfType<RDFDistinctModifier>().SingleOrDefault();
            if (distinctModifier != null)
                table = distinctModifier.ApplyModifier(table);
            #endregion

            #region OFFSET
            RDFOffsetModifier offsetModifier = modifiers.OfType<RDFOffsetModifier>().SingleOrDefault();
            if (offsetModifier != null)
                table = offsetModifier.ApplyModifier(table);
            #endregion

            #region  LIMIT
            RDFLimitModifier limitModifier = modifiers.OfType<RDFLimitModifier>().SingleOrDefault();
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
                string templateCtx = template.Context?.ToString();
                string templateSubj = template.Subject.ToString();
                string templatePred = template.Predicate.ToString();
                string templateObj = template.Object.ToString();

                #region GROUND TEMPLATE
                if (template.Variables.Count == 0)
                {
                    if (needsContext)
                        bindings["?CONTEXT"] = templateCtx ?? RDFNamespaceRegister.DefaultNamespace.ToString();
                    bindings["?SUBJECT"] = templateSubj;
                    bindings["?PREDICATE"] = templatePred;
                    bindings["?OBJECT"] = templateObj;
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
                            if (((DataRow)rowsEnum.Current).IsNull(templateCtx))
                                continue;

                            RDFPatternMember ctx = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[templateCtx].ToString());
                            //Row contains a literal in position of the variable corresponding to the template context
                            if (ctx is RDFLiteral)
                                continue;
                            //Row contains a resource in position of the variable corresponding to the template context
                            bindings["?CONTEXT"] = ctx.ToString();
                        }
                        //Context of the template is a resource
                        else
                            bindings["?CONTEXT"] = templateCtx ?? RDFNamespaceRegister.DefaultNamespace.ToString();
                    }
                    #endregion

                    #region SUBJECT
                    //Subject of the template is a variable
                    if (template.Subject is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template subject
                        if (((DataRow)rowsEnum.Current).IsNull(templateSubj))
                            continue;

                        RDFPatternMember subj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[templateSubj].ToString());
                        //Row contains a literal in position of the variable corresponding to the template subject
                        if (subj is RDFLiteral)
                            continue;
                        //Row contains a resource in position of the variable corresponding to the template subject
                        bindings["?SUBJECT"] = subj.ToString();
                    }
                    //Subject of the template is a resource
                    else
                        bindings["?SUBJECT"] = templateSubj;
                    #endregion

                    #region PREDICATE
                    //Predicate of the template is a variable
                    if (template.Predicate is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template predicate
                        if (((DataRow)rowsEnum.Current).IsNull(templatePred))
                            continue;

                        RDFPatternMember pred = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[templatePred].ToString());
                        //Row contains a blank resource or a literal in position of the variable corresponding to the template predicate
                        if ((pred is RDFResource predRes && predRes.IsBlank) || pred is RDFLiteral)
                            continue;
                        //Row contains a non-blank resource in position of the variable corresponding to the template predicate
                        bindings["?PREDICATE"] = pred.ToString();
                    }
                    //Predicate of the template is a resource
                    else
                        bindings["?PREDICATE"] = templatePred;
                    #endregion

                    #region OBJECT
                    //Object of the template is a variable
                    if (template.Object is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template object
                        if (((DataRow)rowsEnum.Current).IsNull(templateObj))
                            continue;

                        RDFPatternMember obj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[templateObj].ToString());
                        //Row contains a resource or a literal in position of the variable corresponding to the template object
                        bindings["?OBJECT"] = obj.ToString();
                    }
                    //Object of the template is a resource or a literal
                    else
                        bindings["?OBJECT"] = templateObj;
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

            //In case of "DESCRIBE *" query, all the variables must be considered describe terms
            if (!describeQuery.DescribeTerms.Any())
                FetchDescribeVariablesFromQueryMembers(describeQuery, describeQuery.GetEvaluableQueryMembers());

            //Iterate the describe terms of the query
            foreach (RDFPatternMember describeTerm in describeQuery.DescribeTerms)
            {
                if (describeTerm is RDFResource describeResource)
                    result.Merge(DescribeResourceTerm(describeResource, describeQuery, dataSource, result, resultTable), true, MissingSchemaAction.Add);
                else if (describeTerm is RDFVariable describeVariable)
                    result.Merge(DescribeVariableTerm(describeVariable, describeQuery, dataSource, result, resultTable), true, MissingSchemaAction.Add);
            }

            return result;
        }

        /// <summary>
        /// Describes the given resource term with data from the given result table
        /// </summary>
        internal DataTable DescribeResourceTerm(RDFResource describeResource, RDFDescribeQuery describeQuery, RDFDataSource dataSource, DataTable describeTemplate, DataTable resultTable)
        {
            DataTable result = describeTemplate.Clone();

            //Search on GRAPH
            if (dataSource is RDFGraph dataSourceGraph)
            {
                //Search as RESOURCE (S-P-O)
                RDFGraph desc = dataSourceGraph.SelectTriplesBySubject(describeResource)
                                               .UnionWith(dataSourceGraph.SelectTriplesByPredicate(describeResource))
                                               .UnionWith(dataSourceGraph.SelectTriplesByObject(describeResource));
                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
            }
            //Search on STORE
            else if (dataSource is RDFStore dataSourceStore)
            {
                //Search as BLANK RESOURCE (S-P-O)
                if (describeResource.IsBlank)
                {
                    RDFMemoryStore desc = dataSourceStore.SelectQuadruplesBySubject(describeResource)
                                                         .UnionWith(dataSourceStore.SelectQuadruplesByPredicate(describeResource))
                                                         .UnionWith(dataSourceStore.SelectQuadruplesByObject(describeResource));
                    result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                }
                //Search as NON-BLANK RESOURCE (C-S-P-O)
                else
                {
                    RDFMemoryStore desc = dataSourceStore.SelectQuadruplesByContext(new RDFContext(describeResource.URI))
                                                         .UnionWith(dataSourceStore.SelectQuadruplesBySubject(describeResource))
                                                         .UnionWith(dataSourceStore.SelectQuadruplesByPredicate(describeResource))
                                                         .UnionWith(dataSourceStore.SelectQuadruplesByObject(describeResource));
                    result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                }
            }
            //Search on SPARQL endpoint / FEDERATION
            else
            {
                //Create CONSTRUCT query to build term description
                RDFConstructQuery constructQuery =
                    new RDFConstructQuery()
                        .AddPatternGroup(new RDFPatternGroup()
                            .AddPattern(new RDFPattern(describeResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                            .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), describeResource, new RDFVariable("?OBJECT")).UnionWithNext())
                            .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeResource)))
                        .AddTemplate(new RDFPattern(describeResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")))
                        .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), describeResource, new RDFVariable("?OBJECT")))
                        .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeResource));

                //Apply the query to the SPARQL endpoint / FEDERATION
                RDFConstructQueryResult constructQueryResults =
                    dataSource.IsSPARQLEndpoint() ? constructQuery.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                  : constructQuery.ApplyToFederation((RDFFederation)dataSource);
                result.Merge(constructQueryResults.ConstructResults, true, MissingSchemaAction.Add);
            }

            return result;
        }

        /// <summary>
        /// Describes the given variable term with data from the given result table
        /// </summary>
        internal DataTable DescribeVariableTerm(RDFVariable describeVariable, RDFDescribeQuery describeQuery, RDFDataSource dataSource, DataTable describeTemplate, DataTable resultTable)
        {
            DataTable result = describeTemplate.Clone();

            //In order to be processed this variable must be a column of the results table!
            string describeVariableName = describeVariable.ToString();                        
            if (!resultTable.Columns.Contains(describeVariableName))
                return result;

            //Iterate the results table's rows to retrieve terms to be described
            IEnumerator rowsEnum = resultTable.Rows.GetEnumerator();
            while (rowsEnum.MoveNext())
            {
                //In order to be processed this variable must bind a value!
                if (((DataRow)rowsEnum.Current).IsNull(describeVariableName))
                    continue;

                //Retrieve the value of the variable
                RDFPatternMember describeVariableValue = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[describeVariableName].ToString());

                //Search on GRAPH
                if (dataSource is RDFGraph dataSourceGraph)
                {
                    //Search as RESOURCE (S-P-O)
                    if (describeVariableValue is RDFResource describeVariableValueResource)
                    {
                        RDFGraph desc = dataSourceGraph.SelectTriplesBySubject(describeVariableValueResource)
                                                       .UnionWith(dataSourceGraph.SelectTriplesByPredicate(describeVariableValueResource))
                                                       .UnionWith(dataSourceGraph.SelectTriplesByObject(describeVariableValueResource));
                        result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                    }
                    //Search as LITERAL (L)
                    else
                    {
                        RDFGraph desc = dataSourceGraph.SelectTriplesByLiteral((RDFLiteral)describeVariableValue);
                        result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                    }
                }
                //Search on STORE
                else if (dataSource is RDFStore dataSourceStore)
                {
                    //Search as RESOURCE
                    if (describeVariableValue is RDFResource describeVariableValueResource)
                    {
                        //Search as BLANK RESOURCE (S-P-O)
                        if (describeVariableValueResource.IsBlank)
                        {
                            RDFMemoryStore desc = dataSourceStore.SelectQuadruplesBySubject(describeVariableValueResource)
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesByPredicate(describeVariableValueResource))
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesByObject(describeVariableValueResource));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }
                        //Search as NON-BLANK RESOURCE (C-S-P-O)
                        else
                        {
                            RDFMemoryStore desc = dataSourceStore.SelectQuadruplesByContext(new RDFContext(describeVariableValueResource.URI))
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesBySubject(describeVariableValueResource))
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesByPredicate(describeVariableValueResource))
                                                                 .UnionWith(dataSourceStore.SelectQuadruplesByObject(describeVariableValueResource));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }
                    }
                    //Search as LITERAL (L)
                    else
                    {
                        RDFMemoryStore desc = dataSourceStore.SelectQuadruplesByLiteral((RDFLiteral)describeVariableValue);
                        result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                    }
                }
                //Search on SPARQL endpoint / FEDERATION
                else
                {
                    //Create CONSTRUCT query to build term description
                    RDFConstructQuery constructQuery =
                        describeVariableValue is RDFResource describeVariableValueResource
                            ? new RDFConstructQuery()
                                .AddPatternGroup(new RDFPatternGroup()
                                    .AddPattern(new RDFPattern(describeVariableValueResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                                    .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), describeVariableValueResource, new RDFVariable("?OBJECT")).UnionWithNext())
                                    .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeVariableValueResource)))
                                .AddTemplate(new RDFPattern(describeVariableValueResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")))
                                .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), describeVariableValueResource, new RDFVariable("?OBJECT")))
                                .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeVariableValueResource))
                            : new RDFConstructQuery()
                                .AddPatternGroup(new RDFPatternGroup()
                                    .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), (RDFLiteral)describeVariableValue)))
                                .AddTemplate(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), (RDFLiteral)describeVariableValue));

                    //Apply the query to the SPARQL endpoint / FEDERATION
                    RDFConstructQueryResult constructQueryResults =
                        dataSource.IsSPARQLEndpoint() ? constructQuery.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                      : constructQuery.ApplyToFederation((RDFFederation)dataSource);
                    result.Merge(constructQueryResults.ConstructResults, true, MissingSchemaAction.Add);
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches the describe variables from the given collection of query members and give them to the given describe query
        /// </summary>
        internal void FetchDescribeVariablesFromQueryMembers(RDFDescribeQuery describeQuery, IEnumerable<RDFQueryMember> evaluableQueryMembers)
        {
            foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
            {
                #region PATTERN GROUP
                if (evaluableQueryMember is RDFPatternGroup pgEvaluableQueryMember)
                    pgEvaluableQueryMember.Variables.ForEach(v => describeQuery.AddDescribeTerm(v));
                #endregion

                #region SUBQUERY
                else if (evaluableQueryMember is RDFSelectQuery sqEvaluableQueryMember)
                    FetchDescribeVariablesFromQueryMembers(describeQuery, sqEvaluableQueryMember.GetEvaluableQueryMembers());
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
                case RDFGraph graph:
                    return ApplyPatternToGraph(pattern, graph);

                case RDFStore store:
                    return ApplyPatternToStore(pattern, store);

                case RDFFederation federation:
                    return ApplyPatternToFederation(pattern, federation);

                case RDFSPARQLEndpoint sparqlEndpoint:
                    return ApplyPatternToSPARQLEndpoint(pattern, sparqlEndpoint);
            }
            return new DataTable();
        }

        /// <summary>
        /// Applies the given pattern to the given graph
        /// </summary>
        internal DataTable ApplyPatternToGraph(RDFPattern pattern, RDFGraph graph)
        {
            DataTable patternResultTable = new DataTable();
            StringBuilder templateHoleDetector = new StringBuilder();

            //Analyze subject of the pattern
            if (pattern.Subject is RDFVariable)
            {
                templateHoleDetector.Append('S');
                AddColumn(patternResultTable, pattern.Subject.ToString());
            }

            //Analyze predicate of the pattern
            if (pattern.Predicate is RDFVariable)
            {
                templateHoleDetector.Append('P');
                AddColumn(patternResultTable, pattern.Predicate.ToString());
            }

            //Analyze object of the pattern
            bool patternObjectIsResource = pattern.Object is RDFResource;
            bool patternObjectIsLiteral = pattern.Object is RDFLiteral;
            if (pattern.Object is RDFVariable)
            {
                templateHoleDetector.Append('O');
                AddColumn(patternResultTable, pattern.Object.ToString());
            }
            
            //Analyze templateHoleDetector to decide hole filling strategy
            List<RDFTriple> matchingTriples = new List<RDFTriple>();
            switch (templateHoleDetector.ToString())
            {
                case "S":
                    matchingTriples = RDFModelUtilities.SelectTriples(graph, null, (RDFResource)pattern.Predicate, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.S, patternResultTable);
                    break;
                case "P":
                    matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.P, patternResultTable);
                    break;
                case "O":
                    matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, null);
                    PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.O, patternResultTable);
                    break;
                case "SP":
                    matchingTriples = RDFModelUtilities.SelectTriples(graph, null, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    //In case of same S and P variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                    PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SP, patternResultTable);
                    break;
                case "SO":
                    matchingTriples = RDFModelUtilities.SelectTriples(graph, null, (RDFResource)pattern.Predicate, null, null);
                    //In case of same S and O variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                    PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SO, patternResultTable);
                    break;
                case "PO":
                    matchingTriples = RDFModelUtilities.SelectTriples(graph, (RDFResource)pattern.Subject, null, null, null);
                    //In case of same P and O variable, must refine matching triples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                    PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.PO, patternResultTable);
                    break;
                case "SPO":
                    matchingTriples = RDFModelUtilities.SelectTriples(graph, null, null, null, null);
                    //In case of same S and P variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                    //In case of same S and O variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                    //In case of same P and O variable, must refine matching triples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                    PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SPO, patternResultTable);
                    break;
            }

            return patternResultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given store
        /// </summary>
        internal DataTable ApplyPatternToStore(RDFPattern pattern, RDFStore store)
        {
            DataTable patternResultTable = new DataTable();
            StringBuilder templateHoleDetector = new StringBuilder();

            //Analyze context of the pattern
            bool hasContext = (pattern.Context != null);
            if (hasContext && pattern.Context is RDFVariable)
            {
                templateHoleDetector.Append('C');
                AddColumn(patternResultTable, pattern.Context.ToString());
            }

            //Analyze subject of the pattern
            if (pattern.Subject is RDFVariable)
            {
                templateHoleDetector.Append('S');
                AddColumn(patternResultTable, pattern.Subject.ToString());
            }

            //Analyze predicate of the pattern
            if (pattern.Predicate is RDFVariable)
            {
                templateHoleDetector.Append('P');
                AddColumn(patternResultTable, pattern.Predicate.ToString());
            }

            //Analyze object of the pattern
            bool patternObjectIsResource = pattern.Object is RDFResource;
            bool patternObjectIsLiteral = pattern.Object is RDFLiteral;
            if (pattern.Object is RDFVariable)
            {
                templateHoleDetector.Append('O');
                AddColumn(patternResultTable, pattern.Object.ToString());
            }
            
            //Analyze templateHoleDetector to decide hole filling strategy
            RDFMemoryStore matchingQuadruples = new RDFMemoryStore();
            switch (templateHoleDetector.ToString())
            {
                case "C":
                    matchingQuadruples = store.SelectQuadruples(null, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.C, patternResultTable);
                    break;
                case "S":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, null, (RDFResource)pattern.Predicate, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.S, patternResultTable);
                    break;
                case "P":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, (RDFResource)pattern.Subject, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.P, patternResultTable);
                    break;
                case "O":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, null);
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.O, patternResultTable);
                    break;
                case "CS":
                    matchingQuadruples = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CS, patternResultTable);
                    break;
                case "CP":
                    matchingQuadruples = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CP, patternResultTable);
                    break;
                case "CO":
                    matchingQuadruples = store.SelectQuadruples(null, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, null);
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CO, patternResultTable);
                    break;
                case "SP":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, null, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.SP, patternResultTable);
                    break;
                case "SO":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, null, (RDFResource)pattern.Predicate, null, null);
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.SO, patternResultTable);
                    break;
                case "PO":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, (RDFResource)pattern.Subject, null, null, null);
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.PO, patternResultTable);
                    break;
                case "CSP":
                    matchingQuadruples = store.SelectQuadruples(null, null, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CSP, patternResultTable);
                    break;
                case "CSO":
                    matchingQuadruples = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, null, null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CSO, patternResultTable);
                    break;
                case "CPO":
                    matchingQuadruples = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, null, null);
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CPO, patternResultTable);
                    break;
                case "SPO":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, null, null, null, null);
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.SPO, patternResultTable);
                    break;
                case "CSPO":
                    matchingQuadruples = store.SelectQuadruples(null, null, null, null, null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Subject)).ToList());
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Predicate)).ToList());
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Context.Equals(mt.Object)).ToList());
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Predicate)).ToList());
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Subject.Equals(mt.Object)).ToList());
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Quadruples.Values.Where(mt => mt.Predicate.Equals(mt.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CSPO, patternResultTable);
                    break;
            }

            return patternResultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given federation
        /// </summary>
        internal DataTable ApplyPatternToFederation(RDFPattern pattern, RDFFederation federation)
        {
            DataTable resultTable = new DataTable();

            //Iterate data sources of the federation
            foreach (RDFDataSource dataSource in federation)
            {
                switch (dataSource)
                {
                    case RDFGraph dataSourceGraph:
                        DataTable graphTable = ApplyPatternToGraph(pattern, dataSourceGraph);
                        resultTable.Merge(graphTable, true, MissingSchemaAction.Add);
                        break;

                    case RDFStore dataSourceStore:
                        DataTable storeTable = ApplyPatternToStore(pattern, dataSourceStore);
                        resultTable.Merge(storeTable, true, MissingSchemaAction.Add);
                        break;

                    case RDFFederation dataSourceFederation:
                        DataTable federationTable = ApplyPatternToFederation(pattern, dataSourceFederation);
                        resultTable.Merge(federationTable, true, MissingSchemaAction.Add);
                        break;

                    case RDFSPARQLEndpoint dataSourceSparqlEndpoint:
                        DataTable sparqlEndpointTable = ApplyPatternToSPARQLEndpoint(pattern, dataSourceSparqlEndpoint);
                        resultTable.Merge(sparqlEndpointTable, true, MissingSchemaAction.Add);
                        break;
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given federation
        /// </summary>
        internal DataTable ApplyPatternToSPARQLEndpoint(RDFPattern pattern, RDFSPARQLEndpoint sparqlEndpoint)
        {
            //Transform the pattern into an equivalent "SELECT *" query
            RDFSelectQuery selectQuery =
                new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
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
                patternTable.ExtendedProperties.Add(IsOptional, pattern.IsOptional);
                patternTable.ExtendedProperties.Add(JoinAsUnion, pattern.JoinAsUnion);

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

            return resultTable;
        }
        #endregion

        #region MIRELLA TABLES
        /// <summary>
        /// Utility class for comparison between data columns
        /// </summary>
        internal class DataColumnComparer : IEqualityComparer<DataColumn>
        {
            public bool Equals(DataColumn column1, DataColumn column2)
            {
                if (column1 != null)
                    return column2 != null && string.Equals(column1.ColumnName, column2.ColumnName, StringComparison.Ordinal);

                return column2 == null;
            }

            public int GetHashCode(DataColumn column)
                => column.ColumnName.GetHashCode();
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
        internal static void PopulateTable(RDFPattern pattern, List<RDFTriple> triples, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable)
        {
            string patternSubject = pattern.Subject.ToString();
            string patternPredicate = pattern.Predicate.ToString();
            string patternObject = pattern.Object.ToString();
            Dictionary<string, string> bindings = new Dictionary<string, string>();

            //Iterate result graph's triples
            foreach (RDFTriple triple in triples)
            {
                switch (patternHole)
                {
                    //?->P->O
                    case RDFQueryEnums.RDFPatternHoles.S:
                        bindings.Add(patternSubject, triple.Subject.ToString());
                        break;
                    //S->?->O
                    case RDFQueryEnums.RDFPatternHoles.P:
                        bindings.Add(patternPredicate, triple.Predicate.ToString());
                        break;
                    //S->P->?
                    case RDFQueryEnums.RDFPatternHoles.O:
                        bindings.Add(patternObject, triple.Object.ToString());
                        break;
                    //?->?->O
                    case RDFQueryEnums.RDFPatternHoles.SP:
                        bindings.Add(patternSubject, triple.Subject.ToString());
                        if (!bindings.ContainsKey(patternPredicate))
                            bindings.Add(patternPredicate, triple.Predicate.ToString());
                        break;
                    //?->P->?
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(patternSubject, triple.Subject.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, triple.Object.ToString());
                        break;
                    //S->?->?
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(patternPredicate, triple.Predicate.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, triple.Object.ToString());
                        break;
                    //?->?->?
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(patternSubject, triple.Subject.ToString());
                        if (!bindings.ContainsKey(patternPredicate))
                            bindings.Add(patternPredicate, triple.Predicate.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, triple.Object.ToString());
                        break;
                }
                AddRow(resultTable, bindings);
                bindings.Clear();
            }
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given store
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, RDFMemoryStore store, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable)
        {
            string patternContext = pattern.Context?.ToString();
            string patternSubject = pattern.Subject.ToString();
            string patternPredicate = pattern.Predicate.ToString();
            string patternObject = pattern.Object.ToString();
            Dictionary<string, string> bindings = new Dictionary<string, string>();

            //Iterate result store's quadruples
            foreach (RDFQuadruple quadruple in store)
            {
                switch (patternHole)
                {
                    //?->S->P->O
                    case RDFQueryEnums.RDFPatternHoles.C:
                        bindings.Add(patternContext, quadruple.Context.ToString());
                        break;
                    //C->?->P->O
                    case RDFQueryEnums.RDFPatternHoles.S:
                        bindings.Add(patternSubject, quadruple.Subject.ToString());
                        break;
                    //C->S->?->O
                    case RDFQueryEnums.RDFPatternHoles.P:
                        bindings.Add(patternPredicate, quadruple.Predicate.ToString());
                        break;
                    //C->S->P->?
                    case RDFQueryEnums.RDFPatternHoles.O:
                        bindings.Add(patternObject, quadruple.Object.ToString());
                        break;
                    //?->?->P->O
                    case RDFQueryEnums.RDFPatternHoles.CS:
                        bindings.Add(patternContext, quadruple.Context.ToString());
                        if (!bindings.ContainsKey(patternSubject))
                            bindings.Add(patternSubject, quadruple.Subject.ToString());
                        break;
                    //?->S->?->O
                    case RDFQueryEnums.RDFPatternHoles.CP:
                        bindings.Add(patternContext, quadruple.Context.ToString());
                        if (!bindings.ContainsKey(patternPredicate))
                            bindings.Add(patternPredicate, quadruple.Predicate.ToString());
                        break;
                    //?->S->P->?
                    case RDFQueryEnums.RDFPatternHoles.CO:
                        bindings.Add(patternContext, quadruple.Context.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, quadruple.Object.ToString());
                        break;
                    //C->?->?->O
                    case RDFQueryEnums.RDFPatternHoles.SP:
                        bindings.Add(patternSubject, quadruple.Subject.ToString());
                        if (!bindings.ContainsKey(patternPredicate))
                            bindings.Add(patternPredicate, quadruple.Predicate.ToString());
                        break;
                    //C->?->P->?
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(patternSubject, quadruple.Subject.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, quadruple.Object.ToString());
                        break;
                    //C->S->?->?
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(patternPredicate, quadruple.Predicate.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, quadruple.Object.ToString());
                        break;
                    //?->?->?->O
                    case RDFQueryEnums.RDFPatternHoles.CSP:
                        bindings.Add(patternContext, quadruple.Context.ToString());
                        if (!bindings.ContainsKey(patternSubject))
                            bindings.Add(patternSubject, quadruple.Subject.ToString());
                        if (!bindings.ContainsKey(patternPredicate))
                            bindings.Add(patternPredicate, quadruple.Predicate.ToString());
                        break;
                    //?->?->P->?
                    case RDFQueryEnums.RDFPatternHoles.CSO:
                        bindings.Add(patternContext, quadruple.Context.ToString());
                        if (!bindings.ContainsKey(patternSubject))
                            bindings.Add(patternSubject, quadruple.Subject.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, quadruple.Object.ToString());
                        break;
                    //?->S->?->?
                    case RDFQueryEnums.RDFPatternHoles.CPO:
                        bindings.Add(patternContext, quadruple.Context.ToString());
                        if (!bindings.ContainsKey(patternPredicate))
                            bindings.Add(patternPredicate, quadruple.Predicate.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, quadruple.Object.ToString());
                        break;
                    //C->?->?->?
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(patternSubject, quadruple.Subject.ToString());
                        if (!bindings.ContainsKey(patternPredicate))
                            bindings.Add(patternPredicate, quadruple.Predicate.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, quadruple.Object.ToString());
                        break;
                    //?->?->?->?
                    case RDFQueryEnums.RDFPatternHoles.CSPO:
                        bindings.Add(patternContext, quadruple.Context.ToString());
                        if (!bindings.ContainsKey(patternSubject))
                            bindings.Add(patternSubject, quadruple.Subject.ToString());
                        if (!bindings.ContainsKey(patternPredicate))
                            bindings.Add(patternPredicate, quadruple.Predicate.ToString());
                        if (!bindings.ContainsKey(patternObject))
                            bindings.Add(patternObject, quadruple.Object.ToString());
                        break;
                }
                AddRow(resultTable, bindings);
                bindings.Clear();
            }
        }

        /// <summary>
        /// Joins two datatables WITHOUT support for OPTIONAL data
        /// </summary>
        internal static DataTable InnerJoinTables(DataTable dt1, DataTable dt2)
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

                //Loop dt1 table
                result.AcceptChanges();
                result.BeginLoadData();
                foreach (DataRow parentRow in dt1.Rows)
                {
                    object[] parentArray = parentRow.ItemArray;

                    //Loop dt2 table
                    foreach (DataRow childRow in dt2.Rows)
                    {
                        object[] childArray = childRow.ItemArray;

                        //Product-Join parent array with child array into result array
                        object[] resultArray = new object[parentArray.Length + childArray.Length];
                        Array.Copy(parentArray, 0, resultArray, 0, parentArray.Length);
                        Array.Copy(childArray, 0, resultArray, parentArray.Length, childArray.Length);
                        result.LoadDataRow(resultArray, true);
                    }
                }
                result.EndLoadData();
            }

            //INNER-JOIN
            else
            {
                using (DataSet ds = new DataSet())
                {
                    //Add tables to the dataset
                    ds.Tables.Add(dt1);
                    ds.Tables.Add(dt2);

                    //Create the relation linking the common columns
                    DataColumn[] parentColumns = new DataColumn[commonColumns.Length];
                    DataColumn[] childColumns = new DataColumn[commonColumns.Length];
                    for (int i = 0; i < commonColumns.Length; i++)
                    {
                        parentColumns[i] = ds.Tables[0].Columns[commonColumns[i].ColumnName];
                        childColumns[i] = ds.Tables[1].Columns[commonColumns[i].ColumnName];
                    }
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
                            //Keep track of duplicate columns by appending a known identifier to their name
                            string duplicateColKey = string.Concat(ds.Tables[1].Columns[i].ColumnName, "_DUPLICATE_");
                            result.Columns.Add(duplicateColKey, ds.Tables[1].Columns[i].DataType);
                            duplicateCols.Add(duplicateColKey);
                        }
                    }

                    //Loop dt1 table
                    result.AcceptChanges();
                    result.BeginLoadData();
                    foreach (DataRow parentRow in ds.Tables[0].Rows)
                    {
                        DataRow[] childRows = parentRow.GetChildRows(r);
                        if (childRows.Length > 0)
                        {
                            object[] parentArray = parentRow.ItemArray;

                            //Loop dt2 table (only rows from relation)
                            foreach (DataRow childRow in childRows)
                            {
                                object[] childArray = childRow.ItemArray;

                                //Inner-Join parent array with child array into result array
                                object[] resultArray = new object[parentArray.Length + childArray.Length];
                                Array.Copy(parentArray, 0, resultArray, 0, parentArray.Length);
                                Array.Copy(childArray, 0, resultArray, parentArray.Length, childArray.Length);
                                result.LoadDataRow(resultArray, true);
                            }
                        }
                    }
                    result.EndLoadData();

                    //Eliminate the duplicated columns from the result table
                    duplicateCols.ForEach(c => result.Columns.Remove(c));
                }
            }

            return result;
        }

        /// <summary>
        /// Joins two datatables WITH support for OPTIONAL data
        /// </summary>
        internal static DataTable OuterJoinTables(DataTable dt1, DataTable dt2)
        {
            DataTable result = new DataTable();
            DataColumn[] dt1Columns = dt1.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] dt2Columns = dt2.Columns.OfType<DataColumn>().ToArray();

            bool dt2IsOptionalTable = (dt2.ExtendedProperties.ContainsKey(IsOptional) && dt2.ExtendedProperties[IsOptional].Equals(true));
            bool joinInvalidationFlag = false;
            bool foundResults = false;

            //Determine common columns
            DataColumn[] commonColumns = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                   .Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                   .ToArray();

            //Create structure of result table
            result.Columns.AddRange(dt1Columns.Union(dt2Columns.Except(commonColumns), dtComparer)
                          .Select(c => new DataColumn(c.ColumnName, c.DataType))
                          .ToArray());

            //Calculate result columns attribution
            Dictionary<string, (bool,string)> resultColumnsAttribution = new Dictionary<string, (bool,string)>();
            foreach (DataColumn resultColumn in result.Columns)
            {
                //COMMON attribution
                bool commonAttribution = false;
                if (commonColumns.Contains(resultColumn, dtComparer))
                    commonAttribution = true;

                //DT attribution
                string dtAttribution = "DT2";
                if (dt1Columns.Contains(resultColumn, dtComparer))
                    dtAttribution = "DT1";

                resultColumnsAttribution.Add(resultColumn.ColumnName, (commonAttribution, dtAttribution));
            }

            //Loop dt1 table
            result.AcceptChanges();
            result.BeginLoadData();
            foreach (DataRow leftRow in dt1.Rows)
            {
                foundResults = false;

                //Loop dt2 table
                foreach (DataRow rightRow in dt2.Rows)
                {
                    joinInvalidationFlag = false;

                    //Create a temporary row
                    DataRow resultRow = result.NewRow();
                    foreach (DataColumn resultColumn in result.Columns)
                    {
                        //NON-COMMON column
                        if (!resultColumnsAttribution[resultColumn.ColumnName].Item1)
                        {
                            //Take value from left
                            if (string.Equals(resultColumnsAttribution[resultColumn.ColumnName].Item2, "DT1", StringComparison.Ordinal))
                                resultRow[resultColumn.ColumnName] = leftRow[resultColumn.ColumnName];

                            //Take value from right
                            else
                                resultRow[resultColumn.ColumnName] = rightRow[resultColumn.ColumnName];
                        }

                        //COMMON column
                        else
                        {
                            //Left value is NULL
                            if (leftRow.IsNull(resultColumn.ColumnName))
                            {
                                //Right value is NULL
                                if (rightRow.IsNull(resultColumn.ColumnName))
                                {
                                    //Take NULL value
                                    resultRow[resultColumn.ColumnName] = DBNull.Value;
                                }

                                //Right value is NOT NULL
                                else
                                {
                                    //Take value from right
                                    resultRow[resultColumn.ColumnName] = rightRow[resultColumn.ColumnName];
                                }
                            }

                            //Left value is NOT NULL
                            else
                            {
                                //Right value is NULL
                                if (rightRow.IsNull(resultColumn.ColumnName))
                                {
                                    //Take value from left
                                    resultRow[resultColumn.ColumnName] = leftRow[resultColumn.ColumnName];
                                }

                                //Right value is NOT NULL
                                else
                                {
                                    //Left value is EQUAL TO right value
                                    if (string.Equals(leftRow[resultColumn.ColumnName].ToString(), rightRow[resultColumn.ColumnName].ToString(), StringComparison.Ordinal))
                                    {
                                        //Take value from left (it's the same as right)
                                        resultRow[resultColumn.ColumnName] = leftRow[resultColumn.ColumnName];
                                    }

                                    //Left value is NOT EQUAL TO right value
                                    else
                                    {
                                        //Raise the join invalidation flag
                                        joinInvalidationFlag = true;
                                        resultRow.RejectChanges();
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //Add row to result table
                    if (!joinInvalidationFlag)
                    {
                        resultRow.AcceptChanges();
                        result.Rows.Add(resultRow);
                        foundResults = true;
                    }
                }

                //Manage presence of "OPTIONAL" pattern to the right
                if (!foundResults && dt2IsOptionalTable)
                {
                    //In this case, left row must be kept anyway and other columns from right are NULL
                    DataRow optionalRow = result.NewRow();
                    optionalRow.ItemArray = leftRow.ItemArray;
                    optionalRow.AcceptChanges();
                    result.Rows.Add(optionalRow);
                }
            }
            result.EndLoadData();

            return result;
        }

        /// <summary>
        /// Combines the given list of data tables, depending on presence of common columns and dynamic detection of Optional / Union operators
        /// </summary>
        internal static DataTable CombineTables(List<DataTable> dataTables, bool isMerge)
        {
            DataTable finalTable = new DataTable();

            bool switchToOuterJoin = false;
            if (dataTables.Count > 0)
            {
                //Process Unions
                for (int i = 1; i < dataTables.Count; i++)
                {
                    bool previousTableRequiresUnion = dataTables[i-1].ExtendedProperties.ContainsKey(JoinAsUnion) && dataTables[i-1].ExtendedProperties[JoinAsUnion].Equals(true);
                    if (isMerge || previousTableRequiresUnion)
                    {
                        //Backup extended attributes of the current table
                        bool currentTableRequiresUnion = dataTables[i].ExtendedProperties.ContainsKey(JoinAsUnion) && dataTables[i].ExtendedProperties[JoinAsUnion].Equals(true);
                        bool currentTableRequiresOptional = dataTables[i].ExtendedProperties.ContainsKey(IsOptional) && dataTables[i].ExtendedProperties[IsOptional].Equals(true);

                        //Merge the previous table into the current one
                        dataTables[i].Merge(dataTables[i-1], true, MissingSchemaAction.Add);

                        //Restore extended attributes of the current table
                        dataTables[i].ExtendedProperties.Clear();
                        dataTables[i].ExtendedProperties.Add(JoinAsUnion, currentTableRequiresUnion);
                        dataTables[i].ExtendedProperties.Add(IsOptional, currentTableRequiresOptional);

                        //Clear the previous table and flag it as logically deleted
                        dataTables[i-1].Rows.Clear();
                        dataTables[i-1].ExtendedProperties.Add(LogicallyDeleted, true);

                        //Set automatic switch to OuterJoin (because we have done Unions, so null values must be preserved)
                        switchToOuterJoin = true;
                    }
                }
                dataTables.RemoveAll(dt => dt.ExtendedProperties.ContainsKey(LogicallyDeleted) && dt.ExtendedProperties[LogicallyDeleted].Equals(true));

                //Process Joins
                finalTable = dataTables[0];
                for (int i = 1; i < dataTables.Count; i++)
                {
                    //Set automatic switch to OuterJoin in case of "Optional" detected
                    bool currentTableRequiresOptional = dataTables[i].ExtendedProperties.ContainsKey(IsOptional) && dataTables[i].ExtendedProperties[IsOptional].Equals(true);
                    switchToOuterJoin = (switchToOuterJoin || currentTableRequiresOptional);

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
        internal static DataTable ProjectTable(RDFSelectQuery query, DataTable table)
        {
            if (query.ProjectionVars.Any())
            {
                //Remove non-projection variables
                DataColumn[] tableColumns = table.Columns.OfType<DataColumn>().ToArray();
                foreach (DataColumn tableColumn in tableColumns)
                {
                    if (!query.ProjectionVars.Any(projVar => string.Equals(projVar.Key.ToString(), tableColumn.ColumnName, StringComparison.OrdinalIgnoreCase)))
                        table.Columns.Remove(tableColumn);
                }

                //Adjust projection ordinals
                foreach (var projVar in query.ProjectionVars)
                {
                    string projVarString = projVar.Key.ToString();
                    AddColumn(table, projVarString);
                    table.Columns[projVarString].SetOrdinal(projVar.Value);
                }
            }
            return table;
        }
        #endregion

        #endregion
    }
}