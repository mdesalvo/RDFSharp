/*
   Copyright 2012-2025 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using RDFSharp.Model;
using RDFSharp.Store;
using static RDFSharp.Query.RDFQueryUtilities;

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
        /// Attribute denoting an optional pattern/patternGroup/query
        /// </summary>
        internal const string IsOptional = "IsOptional";

        /// <summary>
        /// Attribute denoting a pattern/patternGroup/query to be joined as union
        /// </summary>
        internal const string JoinAsUnion = "JoinAsUnion";

        /// <summary>
        /// Attribute denoting a pattern/patternGroup/query to be joined as minus
        /// </summary>
        internal const string JoinAsMinus = "JoinAsMinus";

        /// <summary>
        /// Attribute denoting a logically deleted intermediate results table
        /// </summary>
        internal const string LogicallyDeleted = "LogicallyDeleted";
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize a query engine instance
        /// </summary>
        internal RDFQueryEngine()
        {
            PatternGroupMemberResultTables = new Dictionary<long, List<DataTable>>();
            QueryMemberResultTables = new Dictionary<long, DataTable>();
        }
        #endregion

        #region Methods

        #region MIRELLA SPARQL
        /// <summary>
        /// Evaluates the given SPARQL SELECT query on the given RDF datasource
        /// </summary>
        internal RDFSelectQueryResult EvaluateSelectQuery(RDFSelectQuery selectQuery, RDFDataSource datasource)
        {
            DataTable queryResultTable = new DataTable();
            List<RDFQueryMember> evaluableQueryMembers = selectQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Count > 0)
            {
                //Iterate the evaluable members of the query
                EvaluateQueryMembers(evaluableQueryMembers, datasource);

                //Get the result table of the query
                queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList());
            }

            //Apply the modifiers of the query to the result table
            return new RDFSelectQueryResult
            {
                SelectResults = ApplyModifiers(selectQuery, queryResultTable)
            };
        }

        /// <summary>
        /// Evaluates the given SPARQL DESCRIBE query on the given RDF datasource
        /// </summary>
        internal RDFDescribeQueryResult EvaluateDescribeQuery(RDFDescribeQuery describeQuery, RDFDataSource datasource)
        {
            DataTable queryResultTable = new DataTable();
            List<RDFQueryMember> evaluableQueryMembers = describeQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Count > 0)
            {
                //Iterate the evaluable members of the query
                EvaluateQueryMembers(evaluableQueryMembers, datasource);

                //Get the result table of the query
                queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList());
            }

            //Apply the modifiers of the query to the result table
            return new RDFDescribeQueryResult
            {
                DescribeResults = ApplyModifiers(describeQuery, FillDescribeTerms(queryResultTable))
            };

            #region Utilities
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
            #endregion
        }

        /// <summary>
        /// Evaluates the given SPARQL CONSTRUCT query on the given RDF datasource
        /// </summary>
        internal RDFConstructQueryResult EvaluateConstructQuery(RDFConstructQuery constructQuery, RDFDataSource datasource)
        {
            DataTable queryResultTable = new DataTable();
            List<RDFQueryMember> evaluableQueryMembers = constructQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Count > 0)
            {
                //Iterate the evaluable members of the query
                EvaluateQueryMembers(evaluableQueryMembers, datasource);

                //Get the result table of the query
                queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList());
            }

            //Apply the modifiers of the query to the result table
            return new RDFConstructQueryResult
            {
                ConstructResults = ApplyModifiers(constructQuery, FillTemplates(constructQuery.Templates, queryResultTable, false))
            };
        }

        /// <summary>
        /// Evaluates the given SPARQL ASK query on the given RDF datasource
        /// </summary>
        internal RDFAskQueryResult EvaluateAskQuery(RDFAskQuery askQuery, RDFDataSource datasource)
        {
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = askQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Count > 0)
            {
                //Iterate the evaluable members of the query
                EvaluateQueryMembers(evaluableQueryMembers, datasource);

                //Get the result table of the query
                DataTable queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList());

                //Transform the result into a boolean response
                askResult.AskResult = (queryResultTable.Rows.Count > 0);
            }

            return askResult;
        }

        /// <summary>
        /// Evaluates the given list of query members against the given datasource
        /// </summary>
        internal void EvaluateQueryMembers(List<RDFQueryMember> evaluableQueryMembers, RDFDataSource datasource)
        {
            foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
            {
                switch (evaluableQueryMember)
                {
                    case RDFPatternGroup patternGroup:
                        //Get the intermediate result tables of the pattern group
                        EvaluatePatternGroup(patternGroup, datasource);

                        //Get the result table of the pattern group
                        FinalizePatternGroup(patternGroup);

                        //Apply the filters of the pattern group to its result table
                        ApplyFilters(patternGroup);
                        break;

                    case RDFSelectQuery subQuery:
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = subQuery.ApplyToDataSource(datasource);

                        //Populate its metadata (IsOptional)
                        subQueryResult.SelectResults.ExtendedProperties[IsOptional] = subQuery.IsOptional
                          || subQueryResult.SelectResults.ExtendedProperties[IsOptional] is true;

                        //Populate its metadata (JoinAsUnion)
                        subQueryResult.SelectResults.ExtendedProperties[JoinAsUnion] = subQuery.JoinAsUnion;

                        //Populate its metadata (JoinAsMinus)
                        subQueryResult.SelectResults.ExtendedProperties[JoinAsMinus] = subQuery.JoinAsMinus;

                        //Save updates
                        QueryMemberResultTables[subQuery.QueryMemberID] = subQueryResult.SelectResults;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the intermediate result tables of the given pattern group
        /// </summary>
        internal void EvaluatePatternGroup(RDFPatternGroup patternGroup, RDFDataSource dataSource)
        {
            PatternGroupMemberResultTables[patternGroup.QueryMemberID] = new List<DataTable>();

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
                DataTable serviceResultsTable = serviceResults.SelectResults;

                //Restore pattern group to its official state
                patternGroup.IsOptional = isOptional;
                patternGroup.JoinAsUnion = joinAsUnion;
                patternGroup.JoinAsMinus = joinAsMinus;
                patternGroup.EvaluateAsService = asService;

                //Set name and metadata of result datatable
                serviceResultsTable.ExtendedProperties.Add(IsOptional, patternGroup.IsOptional);
                serviceResultsTable.ExtendedProperties.Add(JoinAsUnion, patternGroup.JoinAsUnion);
                serviceResultsTable.ExtendedProperties.Add(JoinAsMinus, patternGroup.JoinAsMinus);

                //Save result datatable
                PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(serviceResultsTable);
            }

            //**Standard** evaluation => iterate its active members
            else
            {
                foreach (RDFPatternGroupMember evaluablePGMember in patternGroup.GetEvaluablePatternGroupMembers().Distinct().ToList())
                {
                    switch (evaluablePGMember)
                    {
                        case RDFPattern pattern:
                            //Evaluate pattern on the given data source
                            DataTable patternResultsTable = ApplyPattern(pattern, dataSource);

                            //Set name and metadata of result datatable
                            patternResultsTable.ExtendedProperties.Add(IsOptional, pattern.IsOptional);
                            patternResultsTable.ExtendedProperties.Add(JoinAsUnion, pattern.JoinAsUnion);
                            patternResultsTable.ExtendedProperties.Add(JoinAsMinus, pattern.JoinAsMinus);

                            //Save result datatable
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(patternResultsTable);
                            break;

                        case RDFPropertyPath propertyPath:
                            //Evaluate property path on the given data source
                            DataTable pPathResultsTable = ApplyPropertyPath(propertyPath, dataSource);

                            //Save result datatable
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(pPathResultsTable);
                            break;

                        case RDFValues values:
                            //Transform SPARQL values into an equivalent filter
                            RDFValuesFilter valuesFilter = values.GetValuesFilter();

                            //Save result datatable
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
                            DataTable currentPatternGroupResultTable = CombineTables(PatternGroupMemberResultTables[patternGroup.QueryMemberID]);

                            //Evaluate bind operator on the current patternGroup result table
                            ProjectBind(bind, currentPatternGroupResultTable);

                            //Delete previous patternGroup result tables and replace them with bind operator's one
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Clear();
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(currentPatternGroupResultTable);
                            break;

                        case RDFExistsFilter existsFilter:
                            //Evaluate exists filter's pattern on the given data source
                            DataTable existsFilterResultsTable = ApplyPattern(existsFilter.Pattern, dataSource);

                            //Set name and metadata of result datatable
                            existsFilterResultsTable.ExtendedProperties.Add(IsOptional, false);
                            existsFilterResultsTable.ExtendedProperties.Add(JoinAsUnion, false);
                            existsFilterResultsTable.ExtendedProperties.Add(JoinAsMinus, false);

                            //Save result datatable (directly into the filter)
                            existsFilter.PatternResults = existsFilterResultsTable;
                            break;
                    }
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
                DataTable patternGroupResultTable = CombineTables(PatternGroupMemberResultTables[patternGroup.QueryMemberID]);

                //Populate its metadata (IsOptional)
                patternGroupResultTable.ExtendedProperties[IsOptional] = patternGroup.IsOptional
                  || patternGroupResultTable.ExtendedProperties[IsOptional] is true;

                //Populate its metadata (JoinAsUnion)
                patternGroupResultTable.ExtendedProperties[JoinAsUnion] = patternGroup.JoinAsUnion;

                //Populate its metadata (JoinAsMinus)
                patternGroupResultTable.ExtendedProperties[JoinAsMinus] = patternGroup.JoinAsMinus;

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
                DataTable filteredTable = QueryMemberResultTables[patternGroup.QueryMemberID].Clone();

                //Iterate the rows of the pattern group's result table
                foreach (DataRow currentRow in QueryMemberResultTables[patternGroup.QueryMemberID].Rows)
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
                        DataRow newRow = filteredTable.NewRow();
                        newRow.ItemArray = currentRow.ItemArray;
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
            #region GROUPBY/PROJECTION
            List<RDFModifier> modifiers = query.GetModifiers().ToList();
            if (query is RDFSelectQuery selectQuery)
            {
                #region GROUPBY
                RDFGroupByModifier groupByModifier = modifiers.OfType<RDFGroupByModifier>().SingleOrDefault();
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
            string defaultContext = RDFNamespaceRegister.DefaultNamespace.ToString();
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
                        bindings["?CONTEXT"] = templateCtx ?? defaultContext;
                    bindings["?SUBJECT"] = templateSubj;
                    bindings["?PREDICATE"] = templatePred;
                    bindings["?OBJECT"] = templateObj;
                    AddRow(result, bindings);
                    continue;
                }
                #endregion

                #region NON-GROUND TEMPLATE
                foreach (DataRow resultRow in resultTable.Rows)
                {
                    #region CONTEXT
                    if (needsContext)
                    {
                        //Context of the template is a variable
                        if (template.Context is RDFVariable)
                        {
                            //Check if the template must be skipped, in order to not produce illegal triples
                            //Row contains an unbound value in position of the variable corresponding to the template context
                            if (resultRow.IsNull(templateCtx))
                                continue;

                            RDFPatternMember ctx = ParseRDFPatternMember(resultRow[templateCtx].ToString());
                            //Row contains a literal in position of the variable corresponding to the template context
                            if (ctx is RDFLiteral)
                                continue;
                            //Row contains a resource in position of the variable corresponding to the template context
                            bindings["?CONTEXT"] = ctx.ToString();
                        }
                        //Context of the template is a resource
                        else
                            bindings["?CONTEXT"] = templateCtx ?? defaultContext;
                    }
                    #endregion

                    #region SUBJECT
                    //Subject of the template is a variable
                    if (template.Subject is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template subject
                        if (resultRow.IsNull(templateSubj))
                            continue;

                        RDFPatternMember subj = ParseRDFPatternMember(resultRow[templateSubj].ToString());
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
                        if (resultRow.IsNull(templatePred))
                            continue;

                        RDFPatternMember pred = ParseRDFPatternMember(resultRow[templatePred].ToString());
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
                        if (resultRow.IsNull(templateObj))
                            continue;

                        RDFPatternMember obj = ParseRDFPatternMember(resultRow[templateObj].ToString());
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
            if (describeQuery.DescribeTerms.Count == 0)
                FetchDescribeVariablesFromQueryMembers(describeQuery, describeQuery.GetEvaluableQueryMembers());

            //Iterate the describe terms of the query
            foreach (RDFPatternMember describeTerm in describeQuery.DescribeTerms)
            {
                switch (describeTerm)
                {
                    case RDFResource describeResource:
                        result.Merge(DescribeResourceTerm(describeResource, dataSource, result), true, MissingSchemaAction.Add);
                        break;

                    case RDFVariable describeVariable:
                        result.Merge(DescribeVariableTerm(describeVariable, dataSource, result, resultTable), true, MissingSchemaAction.Add);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Describes the given resource term with data from the given result table
        /// </summary>
        internal DataTable DescribeResourceTerm(RDFResource describeResource, RDFDataSource dataSource, DataTable describeTemplate)
        {
            #region Utilities
            RDFGraph QueryGraph(RDFGraph dsGraph)
                => dsGraph[describeResource, null, null, null]
                    .UnionWith(dsGraph[null, describeResource, null, null])
                    .UnionWith(dsGraph[null, null, describeResource, null]);

            RDFMemoryStore QueryStore(RDFStore dsStore)
                => describeResource.IsBlank 
                   ? dsStore.SelectQuadruplesBySubject(describeResource)
                            .UnionWith(dsStore.SelectQuadruplesByObject(describeResource))
                   : dsStore.SelectQuadruplesByContext(new RDFContext(describeResource.URI))
                            .UnionWith(dsStore.SelectQuadruplesBySubject(describeResource))
                            .UnionWith(dsStore.SelectQuadruplesByPredicate(describeResource))
                            .UnionWith(dsStore.SelectQuadruplesByObject(describeResource));

            RDFSelectQuery BuildFederationOrSPARQLEndpointQuery()
                => describeResource.IsBlank 
                   ? new RDFSelectQuery()
                        .AddPatternGroup(new RDFPatternGroup()
                          .AddPattern(new RDFPattern(describeResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeResource)))
                   : new RDFSelectQuery()
                        .AddPatternGroup(new RDFPatternGroup()
                          .AddPattern(new RDFPattern(describeResource, new RDFVariable("?PREDICATE"), new RDFVariable("?OBJECT")).UnionWithNext())
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), describeResource, new RDFVariable("?OBJECT")).UnionWithNext())
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeResource)));
            #endregion

            DataTable result = describeTemplate.Clone();

            switch (dataSource)
            {
                //GRAPH
                case RDFGraph dataSourceGraph:
                    RDFGraph graph = QueryGraph(dataSourceGraph);
                    result.Merge(graph.ToDataTable(), true, MissingSchemaAction.Add);
                    break;

                //STORE
                case RDFStore dataSourceStore:
                    RDFMemoryStore store = QueryStore(dataSourceStore);
                    result.Merge(store.ToDataTable(), true, MissingSchemaAction.Add);
                    break;

                //FEDERATION / SPARQL ENDPOINT
                default:
                    RDFSelectQuery query = BuildFederationOrSPARQLEndpointQuery();
                    RDFSelectQueryResult queryResults = dataSource.IsSPARQLEndpoint() ? query.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                                                      : query.ApplyToFederation((RDFFederation)dataSource);
                    result.Merge(queryResults.SelectResults, true, MissingSchemaAction.Add);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Describes the given literal term with data from the given result table
        /// </summary>
        internal DataTable DescribeLiteralTerm(RDFLiteral describeLiteral, RDFDataSource dataSource, DataTable describeTemplate)
        {
            #region Utilities
            RDFGraph QueryGraph(RDFGraph dsGraph)
                => dsGraph[null, null, null, describeLiteral];

            RDFMemoryStore QueryStore(RDFStore dsStore)
                => dsStore.SelectQuadruplesByLiteral(describeLiteral);

            RDFSelectQuery BuildFederationOrSPARQLEndpointQuery()
                => new RDFSelectQuery()
                        .AddPatternGroup(new RDFPatternGroup()
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeLiteral)));
            #endregion

            DataTable result = describeTemplate.Clone();

            switch (dataSource)
            {
                //GRAPH
                case RDFGraph dataSourceGraph:
                    RDFGraph graph = QueryGraph(dataSourceGraph);
                    result.Merge(graph.ToDataTable(), true, MissingSchemaAction.Add);
                    break;

                //STORE
                case RDFStore dataSourceStore:
                    RDFMemoryStore store = QueryStore(dataSourceStore);
                    result.Merge(store.ToDataTable(), true, MissingSchemaAction.Add);
                    break;

                //FEDERATION / SPARQL ENDPOINT
                default:
                    RDFSelectQuery query = BuildFederationOrSPARQLEndpointQuery();
                    RDFSelectQueryResult queryResults = 
                        dataSource.IsSPARQLEndpoint() ? query.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                      : query.ApplyToFederation((RDFFederation)dataSource);
                    result.Merge(queryResults.SelectResults, true, MissingSchemaAction.Add);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Describes the given variable term with data from the given result table
        /// </summary>
        internal DataTable DescribeVariableTerm(RDFVariable describeVariable, RDFDataSource dataSource, DataTable describeTemplate, DataTable resultTable)
        {
            DataTable result = describeTemplate.Clone();

            //In order to be processed this variable must be a column of the results table!
            string describeVariableName = describeVariable.ToString();
            if (!resultTable.Columns.Contains(describeVariableName))
                return result;

            //Iterate the results table's rows to retrieve terms to be described
            foreach (RDFPatternMember describeVariableValue in
                     from DataRow resultRow in resultTable.Rows
                     where !resultRow.IsNull(describeVariableName)
                     select ParseRDFPatternMember(resultRow[describeVariableName].ToString()))
            {
                //Execute most appropriate strategy, depending on the type of the variable value
                switch (describeVariableValue)
                {
                    //RESOURCE
                    case RDFResource describeResource:
                        DataTable describeResourceTable = DescribeResourceTerm(describeResource, dataSource, describeTemplate);
                        result.Merge(describeResourceTable, true, MissingSchemaAction.Add);
                        break;

                    //LITERAL
                    case RDFLiteral describeLiteral:
                        DataTable describeLiteralTable = DescribeLiteralTerm(describeLiteral, dataSource, describeTemplate);
                        result.Merge(describeLiteralTable, true, MissingSchemaAction.Add);
                        break;
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
                switch (evaluableQueryMember)
                {
                    //PATTERN GROUP
                    case RDFPatternGroup pgEvaluableQueryMember:
                        pgEvaluableQueryMember.Variables.ForEach(v => describeQuery.AddDescribeTerm(v));
                        break;

                    //SUBQUERY
                    case RDFSelectQuery sqEvaluableQueryMember:
                        FetchDescribeVariablesFromQueryMembers(describeQuery, sqEvaluableQueryMember.GetEvaluableQueryMembers());
                        break;
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
            List<RDFTriple> matchingTriples;
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
            RDFMemoryStore matchingQuadruples;
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
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Subject)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CS, patternResultTable);
                    break;

                case "CP":
                    matchingQuadruples = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Predicate)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CP, patternResultTable);
                    break;

                case "CO":
                    matchingQuadruples = store.SelectQuadruples(null, (RDFResource)pattern.Subject, (RDFResource)pattern.Predicate, null, null);
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CO, patternResultTable);
                    break;

                case "SP":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, null, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Subject.Equals(mq.Predicate)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.SP, patternResultTable);
                    break;

                case "SO":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, null, (RDFResource)pattern.Predicate, null, null);
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Subject.Equals(mq.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.SO, patternResultTable);
                    break;

                case "PO":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, (RDFResource)pattern.Subject, null, null, null);
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Predicate.Equals(mq.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.PO, patternResultTable);
                    break;

                case "CSP":
                    matchingQuadruples = store.SelectQuadruples(null, null, null, patternObjectIsResource ? (RDFResource)pattern.Object : null, patternObjectIsLiteral ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Subject)).ToList());
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Predicate)).ToList());
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Subject.Equals(mq.Predicate)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CSP, patternResultTable);
                    break;

                case "CSO":
                    matchingQuadruples = store.SelectQuadruples(null, null, (RDFResource)pattern.Predicate, null, null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Subject)).ToList());
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Object)).ToList());
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Subject.Equals(mq.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CSO, patternResultTable);
                    break;

                case "CPO":
                    matchingQuadruples = store.SelectQuadruples(null, (RDFResource)pattern.Subject, null, null, null);
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Predicate)).ToList());
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Object)).ToList());
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Predicate.Equals(mq.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.CPO, patternResultTable);
                    break;

                case "SPO":
                    matchingQuadruples = store.SelectQuadruples(hasContext ? (RDFContext)pattern.Context : null, null, null, null, null);
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Subject.Equals(mq.Predicate)).ToList());
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Subject.Equals(mq.Object)).ToList());
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Predicate.Equals(mq.Object)).ToList());
                    PopulateTable(pattern, matchingQuadruples, RDFQueryEnums.RDFPatternHoles.SPO, patternResultTable);
                    break;

                case "CSPO":
                    matchingQuadruples = store.SelectQuadruples(null, null, null, null, null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Subject)).ToList());
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Predicate)).ToList());
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Context.Equals(mq.Object)).ToList());
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Subject.Equals(mq.Predicate)).ToList());
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Subject.Equals(mq.Object)).ToList());
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = new RDFMemoryStore(matchingQuadruples.Where(mq => mq.Predicate.Equals(mq.Object)).ToList());
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
                        //Pattern is transformed into an equivalent "SELECT *" query which is sent to the SPARQL endpoint.
                        //SPARQL endpoint options are eventually retrieved directly from the federation.
                        federation.EndpointDataSourcesQueryOptions.TryGetValue(dataSourceSparqlEndpoint.ToString(), out RDFSPARQLEndpointQueryOptions dataSourceSparqlEndpointOptions);
                        RDFSelectQuery sparqlEndpointQuery =  new RDFSelectQuery().AddPatternGroup(new RDFPatternGroup().AddPattern(pattern));
                        RDFSelectQueryResult sparqlEndpointTable = sparqlEndpointQuery.ApplyToSPARQLEndpoint(dataSourceSparqlEndpoint, dataSourceSparqlEndpointOptions);
                        resultTable.Merge(sparqlEndpointTable.SelectResults, true, MissingSchemaAction.Add);
                        break;
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Applies the given property path to the given graph
        /// </summary>
        internal DataTable ApplyPropertyPath(RDFPropertyPath propertyPath, RDFDataSource dataSource)
        {
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
                patternTable.ExtendedProperties.Add(JoinAsMinus, pattern.JoinAsMinus);

                //Add produced table
                patternTables.Add(patternTable);
            }

            //Merge produced list of tables
            DataTable resultTable = CombineTables(patternTables);

            //Remove property path variables
            List<string> propPathCols = (from DataColumn dtCol 
                                         in resultTable.Columns 
                                         where dtCol.ColumnName.StartsWith("?__PP", StringComparison.Ordinal) 
                                         select dtCol.ColumnName).ToList();
            propPathCols.ForEach(ppc => resultTable.Columns.Remove(ppc));

            return resultTable;
        }

        /// <summary>
        /// Applies the given raw string query to the given SPARQL endpoint
        /// </summary>
        internal RDFQueryResult ApplyRawToSPARQLEndpoint(string queryType, string query, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
        {
            #region Utilities
            void AdjustVariableColumnNames(DataTable qrTable)
            {
                //Eventually adjust column names (should start with "?")
                int columnsCount = qrTable.Columns.Count;
                for (int i = 0; i < columnsCount; i++)
                {
                    if (!qrTable.Columns[i].ColumnName.StartsWith("?", StringComparison.Ordinal))
                        qrTable.Columns[i].ColumnName = string.Concat("?", qrTable.Columns[i].ColumnName);
                }
            }
            #endregion

            RDFQueryResult queryResult = null;
            if (!string.IsNullOrWhiteSpace(query) && sparqlEndpoint != null)
            {
                if (sparqlEndpointQueryOptions == null)
                    sparqlEndpointQueryOptions = new RDFSPARQLEndpointQueryOptions();

                //Establish a connection to the given SPARQL endpoint
                using (RDFWebClient webClient = new RDFWebClient(sparqlEndpointQueryOptions.TimeoutMilliseconds))
                {
                    //Parse user-provided parameters
                    string defaultGraphUri = sparqlEndpoint.QueryParams.Get("default-graph-uri");
                    string namedGraphUri = sparqlEndpoint.QueryParams.Get("named-graph-uri");

                    //Insert request headers
                    sparqlEndpoint.FillWebClientAuthorization(webClient);
                    switch (queryType)
                    {
                        case "ASK":
                        case "SELECT":
                            webClient.Headers.Add(HttpRequestHeader.Accept, "application/sparql-results+xml");
                            break;

                        case "CONSTRUCT":
                        case "DESCRIBE":
                            webClient.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                            webClient.Headers.Add(HttpRequestHeader.Accept, "text/turtle");
                            break;
                    }

                    //Prepare request (GET vs POST)
                    byte[] sparqlResponse = null;
                    try
                    {
                        switch (sparqlEndpointQueryOptions.QueryMethod)
                        {
                            //query via GET with URL-encoded querystring
                            default:
                            case RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Get:
                                //Handle user-provided parameters
                                webClient.QueryString.Add("query", HttpUtility.UrlEncode(query));
                                webClient.QueryString.Add(sparqlEndpoint.QueryParams);
                                //Send query to SPARQL endpoint
                                sparqlResponse = webClient.DownloadData(sparqlEndpoint.BaseAddress);
                                break;

                            //query via POST with URL-encoded body
                            case RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post:
                                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                                //Handle user-provided parameters
                                string queryString = string.Concat("query=", HttpUtility.UrlEncode(query));
                                if (!string.IsNullOrEmpty(defaultGraphUri))
                                    queryString = string.Concat("using-graph-uri=", HttpUtility.UrlEncode(defaultGraphUri), "&", queryString);
                                if (!string.IsNullOrEmpty(namedGraphUri))
                                    queryString = string.Concat("using-named-graph-uri=", HttpUtility.UrlEncode(namedGraphUri), "&", queryString);
                                //Send query to SPARQL endpoint
                                sparqlResponse = Encoding.UTF8.GetBytes(webClient.UploadString(sparqlEndpoint.BaseAddress, queryString));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (sparqlEndpointQueryOptions.ErrorBehavior == RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)
                            throw new RDFQueryException($"{queryType} query on SPARQL endpoint failed because: {ex.Message}", ex);
                    }

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                        {
                            switch (queryType)
                            {
                                case "ASK":
                                    queryResult = RDFAskQueryResult.FromSparqlXmlResult(sStream);
                                    break;

                                case "SELECT":
                                    queryResult = RDFSelectQueryResult.FromSparqlXmlResult(sStream);
                                    AdjustVariableColumnNames(((RDFSelectQueryResult)queryResult).SelectResults);
                                    break;

                                case "CONSTRUCT":
                                    queryResult = RDFConstructQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                                    AdjustVariableColumnNames(((RDFConstructQueryResult)queryResult).ConstructResults);
                                    break;

                                case "DESCRIBE":
                                    queryResult = RDFDescribeQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                                    AdjustVariableColumnNames(((RDFDescribeQueryResult)queryResult).DescribeResults);
                                    break;
                            }
                        }
                }
            }

            //Adjust to give eventual empty result
            switch (queryType)
            {
                case "ASK":
                    queryResult = queryResult ?? new RDFAskQueryResult();
                    break;

                case "SELECT":
                    queryResult = queryResult ?? new RDFSelectQueryResult();
                    break;

                case "CONSTRUCT":
                    queryResult = queryResult ?? new RDFConstructQueryResult();
                    break;

                case "DESCRIBE":
                    queryResult = queryResult ?? new RDFDescribeQueryResult();
                    break;
            }

            return queryResult;
        }
        #endregion

        #region MIRELLA TABLES
        /// <summary>
        /// Utility class for comparison between data columns
        /// </summary>
        internal sealed class DataColumnComparer : IEqualityComparer<DataColumn>
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
                table.Columns.Add(colName, typeof(string));
        }

        /// <summary>
        /// Adds a new row to the given table
        /// </summary>
        internal static void AddRow(DataTable table, Dictionary<string, string> bindings)
        {
            bool rowAdded = false;

            DataRow resultRow = table.NewRow();
            foreach (string bindingKey in bindings.Keys.Where(bk => table.Columns.Contains(bk)))
            {
                resultRow[bindingKey] = bindings[bindingKey];
                rowAdded = true;
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
        /// Joins two datatables WITHOUT support for OPTIONAL/UNION
        /// </summary>
        internal static DataTable InnerJoinTables(DataTable leftTable, DataTable rightTable)
        {
            DataTable joinTable = new DataTable();

            //Determine common columns
            DataColumn[] leftColumns = leftTable.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] rightColumns = rightTable.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] commonColumns = leftColumns.Intersect(rightColumns, dtComparer)
                                                    .Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                    .ToArray();

            //PRODUCT-JOIN
            if (commonColumns.Length == 0)
            {
                //Create the structure of the product table
                joinTable.Columns.AddRange(leftColumns.Union(rightColumns, dtComparer)
                                                      .Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                      .ToArray());

                //Loop left table
                joinTable.BeginLoadData();
                foreach (DataRow leftRow in leftTable.Rows)
                {
                    //Loop right table
                    foreach (DataRow rightRow in rightTable.Rows)
                    {
                        //Join left array with right array
                        object[] joinArray = new object[leftRow.ItemArray.Length + rightRow.ItemArray.Length];
                        Array.Copy(leftRow.ItemArray, 0, joinArray, 0, leftRow.ItemArray.Length);
                        Array.Copy(rightRow.ItemArray, 0, joinArray, leftRow.ItemArray.Length, rightRow.ItemArray.Length);
                        joinTable.LoadDataRow(joinArray, true);
                    }
                }
                joinTable.EndLoadData();
            }

            //INNER-JOIN
            else
            {
                using (DataSet dataSet = new DataSet())
                {
                    //Add tables to the dataset
                    dataSet.Tables.Add(leftTable);
                    dataSet.Tables.Add(rightTable);

                    //Create the relation linking the common columns
                    DataColumn[] leftRelationColumns = new DataColumn[commonColumns.Length];
                    DataColumn[] rightRelationColumns = new DataColumn[commonColumns.Length];
                    for (int i = 0; i < commonColumns.Length; i++)
                    {
                        leftRelationColumns[i] = dataSet.Tables[0].Columns[commonColumns[i].ColumnName];
                        rightRelationColumns[i] = dataSet.Tables[1].Columns[commonColumns[i].ColumnName];
                    }
                    DataRelation dataRelation = new DataRelation("JoinRelation", leftRelationColumns, rightRelationColumns, false);
                    dataSet.Relations.Add(dataRelation);

                    //Create the structure of the join table
                    List<string> duplicateColumns = new List<string>();
                    for (int i = 0; i < dataSet.Tables[0].Columns.Count; i++)
                        joinTable.Columns.Add(dataSet.Tables[0].Columns[i].ColumnName, dataSet.Tables[0].Columns[i].DataType);
                    for (int i = 0; i < dataSet.Tables[1].Columns.Count; i++)
                    {
                        if (!joinTable.Columns.Contains(dataSet.Tables[1].Columns[i].ColumnName))
                            joinTable.Columns.Add(dataSet.Tables[1].Columns[i].ColumnName, dataSet.Tables[1].Columns[i].DataType);
                        else
                        {
                            //Keep track of duplicate columns by appending a known identifier to their name
                            string duplicateColKey = string.Concat(dataSet.Tables[1].Columns[i].ColumnName, "_DUPLICATE_");
                            joinTable.Columns.Add(duplicateColKey, dataSet.Tables[1].Columns[i].DataType);
                            duplicateColumns.Add(duplicateColKey);
                        }
                    }

                    //Loop left table
                    joinTable.BeginLoadData();
                    foreach (DataRow leftRow in dataSet.Tables[0].Rows)
                    {
                        DataRow[] relatedRows = leftRow.GetChildRows(dataRelation);
                        if (relatedRows.Length > 0)
                        {
                            //Loop right table (only rows from relation)
                            foreach (DataRow relatedRow in relatedRows)
                            {
                                //Join left array with related array
                                object[] joinArray = new object[leftRow.ItemArray.Length + relatedRow.ItemArray.Length];
                                Array.Copy(leftRow.ItemArray, 0, joinArray, 0, leftRow.ItemArray.Length);
                                Array.Copy(relatedRow.ItemArray, 0, joinArray, leftRow.ItemArray.Length, relatedRow.ItemArray.Length);
                                joinTable.LoadDataRow(joinArray, true);
                            }
                        }
                    }
                    joinTable.EndLoadData();

                    //Eliminate the duplicated columns from the inner-join table
                    duplicateColumns.ForEach(dc => joinTable.Columns.Remove(dc));
                }
            }

            return joinTable;
        }

        /// <summary>
        /// Joins two datatables WITH support for OPTIONAL/UNION
        /// </summary>
        internal static DataTable OuterJoinTables(DataTable leftTable, DataTable rightTable)
        {
            #region Utilities
            bool CheckJoin(DataRow leftRow, DataRow rightRow, string commonColumn)
                => leftRow.IsNull(commonColumn)
                    || rightRow.IsNull(commonColumn)
                    || string.Equals(leftRow[commonColumn]?.ToString(), rightRow[commonColumn]?.ToString(), StringComparison.Ordinal);
            #endregion

            DataTable joinTable = new DataTable();

            //Determine common columns
            bool rightIsOptionalTable = rightTable.ExtendedProperties.ContainsKey(IsOptional) 
                                          && rightTable.ExtendedProperties[IsOptional].Equals(true);
            DataColumn[] leftColumns = leftTable.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] rightColumns = rightTable.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] commonColumns = leftColumns.Intersect(rightColumns, dtComparer)
                                                    .Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                    .ToArray();

            //Create structure of outer-join table
            joinTable.Columns.AddRange(leftColumns.Union(rightColumns.Except(commonColumns), dtComparer)
                                                  .Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                  .ToArray());

            //Calculate outer-join column's attribution
            Dictionary<string, (bool,string)> joinColumnsAttribution = new Dictionary<string, (bool,string)>();
            foreach (DataColumn joinColumn in joinTable.Columns)
            {
                //COMMON attribution
                bool commonAttribution = commonColumns.Contains(joinColumn, dtComparer);

                //DT1/DT2 attribution
                string dtAttribution = leftColumns.Contains(joinColumn, dtComparer) ? "DT1" : "DT2";

                joinColumnsAttribution.Add(joinColumn.ColumnName, (commonAttribution, dtAttribution));
            }

            //Loop left table
            joinTable.BeginLoadData();
            foreach (DataRow leftRow in leftTable.Rows)
            {
                //Leverage a relation between left row and right table based on common columns
                //(this helps at slightly reducing O(N^2) complexity to O(N*K) where K << N)
                EnumerableRowCollection<DataRow> relatedRows = commonColumns.Aggregate(rightTable.AsEnumerable(), 
                                                                    (current, commonColumn) => current.Where(relatedRow => CheckJoin(leftRow, relatedRow, commonColumn.ColumnName)));
                List<DataRow> relatedRowsList = relatedRows.ToList();

                //Relation HAS found data => proceed with outer-join
                if (relatedRowsList.Count > 0)
                {
                    foreach (DataRow relatedRow in relatedRowsList)
                    {
                        //Create the candidate outer-join row
                        DataRow joinRow = joinTable.NewRow();
                        foreach (DataColumn joinColumn in joinTable.Columns)
                        {
                            //NON-COMMON column
                            if (!joinColumnsAttribution[joinColumn.ColumnName].Item1)
                            {
                                //Take value from left
                                if (string.Equals(joinColumnsAttribution[joinColumn.ColumnName].Item2, "DT1", StringComparison.Ordinal))
                                    joinRow[joinColumn.ColumnName] = leftRow[joinColumn.ColumnName];

                                //Take value from related
                                else
                                    joinRow[joinColumn.ColumnName] = relatedRow[joinColumn.ColumnName];
                            }

                            //COMMON column
                            else
                            {
                                //Left value is NULL
                                if (leftRow.IsNull(joinColumn.ColumnName))
                                {
                                    //Right value is NULL
                                    if (relatedRow.IsNull(joinColumn.ColumnName))
                                    {
                                        //Take NULL value
                                        joinRow[joinColumn.ColumnName] = DBNull.Value;
                                    }

                                    //Right value is NOT NULL
                                    else
                                    {
                                        //Take value from related
                                        joinRow[joinColumn.ColumnName] = relatedRow[joinColumn.ColumnName];
                                    }
                                }

                                //Left value is NOT NULL
                                else
                                {
                                    //Take value from left
                                    joinRow[joinColumn.ColumnName] = leftRow[joinColumn.ColumnName];
                                }
                            }
                        }

                        //Add the join row to the outer-join table
                        joinTable.Rows.Add(joinRow);
                    }
                }

                //Relation HASN'T found data => check for OPTIONAL presence
                else
                {
                    //If OPTIONAL is required by right table, left row must be kept anyway and other columns from right are NULL
                    if (rightIsOptionalTable)
                    {
                        DataRow joinRow = joinTable.NewRow();
                        joinRow.ItemArray = leftRow.ItemArray;
                        joinTable.Rows.Add(joinRow);
                    }
                }
            }
            joinTable.EndLoadData();

            return joinTable;
        }

        /// <summary>
        /// Computes the difference between left datatable and right datatable
        /// </summary>
        internal static DataTable DiffJoinTables(DataTable leftTable, DataTable rightTable)
        {
            #region Utilities
            bool CheckJoin(DataRow leftRow, DataRow rightRow, string commonColumn)
                => leftRow.IsNull(commonColumn)
                    || rightRow.IsNull(commonColumn)
                    || string.Equals(leftRow[commonColumn]?.ToString(), rightRow[commonColumn]?.ToString(), StringComparison.Ordinal);
            #endregion

            DataTable diffTable = new DataTable();

            //Determine common columns
            DataColumn[] leftColumns = leftTable.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] rightColumns = rightTable.Columns.OfType<DataColumn>().ToArray();
            DataColumn[] commonColumns = leftColumns.Intersect(rightColumns, dtComparer)
                                                    .Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                    .ToArray();
            int commonColumnsCount = commonColumns.Length;

            //Create structure of diff table
            diffTable.Columns.AddRange(leftColumns.Select(c => new DataColumn(c.ColumnName, c.DataType))
                                                  .ToArray());

            //Loop left table
            diffTable.BeginLoadData();
            foreach (DataRow leftRow in leftTable.Rows)
            {
                //In case of no common columns, just take left row
                if (commonColumnsCount == 0)
                {
                    DataRow diffRow = diffTable.NewRow();
                    diffRow.ItemArray = leftRow.ItemArray;
                    diffTable.Rows.Add(diffRow);
                    continue;
                }

                //Leverage a relation between left row and right table based on common columns
                //(this helps at slightly reducing O(N^2) complexity to O(N*K) where K << N)
                EnumerableRowCollection<DataRow> relatedRows = commonColumns.Aggregate(rightTable.AsEnumerable(), 
                                                                    (current, commonColumn) => current.Where(relatedRow => CheckJoin(leftRow, relatedRow, commonColumn.ColumnName)));
                List<DataRow> relatedRowsList = relatedRows.ToList();

                //Take left row only if it HASN'T any right matches
                if (relatedRowsList.Count == 0)
                {
                    DataRow diffRow = diffTable.NewRow();
                    diffRow.ItemArray = leftRow.ItemArray;
                    diffTable.Rows.Add(diffRow);
                }
            }
            diffTable.EndLoadData();

            return diffTable;
        }

        /// <summary>
        /// Combines the given list of data tables, depending on presence of common columns and dynamic detection of Optional / Union / Minus operators
        /// </summary>
        internal static DataTable CombineTables(List<DataTable> dataTables)
        {
            DataTable finalTable = new DataTable();
            switch (dataTables.Count)
            {
                case 0: return finalTable;
                case 1: return dataTables[0];
            }

            #region Utilities
            bool ProcessUnions()
            {
                bool hasProcessedUnion = false;
                for (int i = 1; i < dataTables.Count; i++)
                {
                    bool previousTableRequiresUnion = dataTables[i-1].ExtendedProperties.ContainsKey(JoinAsUnion) 
                                                        && dataTables[i-1].ExtendedProperties[JoinAsUnion].Equals(true);
                    if (previousTableRequiresUnion)
                    {
                        //Backup extended attributes of the current table
                        bool currentTableRequiresOptional = dataTables[i].ExtendedProperties.ContainsKey(IsOptional) 
                                                              && dataTables[i].ExtendedProperties[IsOptional].Equals(true);
                        bool currentTableRequiresUnion = dataTables[i].ExtendedProperties.ContainsKey(JoinAsUnion) 
                                                           && dataTables[i].ExtendedProperties[JoinAsUnion].Equals(true);
                        bool currentTableRequiresMinus = dataTables[i].ExtendedProperties.ContainsKey(JoinAsMinus) 
                                                           && dataTables[i].ExtendedProperties[JoinAsMinus].Equals(true);

                        //Merge the previous table into the current one
                        dataTables[i].Merge(dataTables[i-1], true, MissingSchemaAction.Add);

                        //Restore extended attributes of the current table
                        dataTables[i].ExtendedProperties.Clear();
                        dataTables[i].ExtendedProperties.Add(IsOptional, currentTableRequiresOptional);
                        dataTables[i].ExtendedProperties.Add(JoinAsUnion, currentTableRequiresUnion);
                        dataTables[i].ExtendedProperties.Add(JoinAsMinus, currentTableRequiresMinus);

                        //Clear the previous table and flag it as logically deleted
                        dataTables[i-1].Rows.Clear();
                        dataTables[i-1].ExtendedProperties.Clear();
                        dataTables[i-1].ExtendedProperties.Add(LogicallyDeleted, true);

                        //Signal that at least one UnionWithNext has been performed
                        hasProcessedUnion = true;
                    }
                }
                return hasProcessedUnion;
            }
            bool ProcessMinus()
            {
                bool hasProcessedMinus = false;
                for (int i = 1; i < dataTables.Count; i++)
                {
                    bool previousTableRequiresMinus = dataTables[i-1].ExtendedProperties.ContainsKey(JoinAsMinus) 
                                                        && dataTables[i-1].ExtendedProperties[JoinAsMinus].Equals(true);
                    if (previousTableRequiresMinus)
                    {
                        //Backup extended attributes of the current table
                        bool currentTableRequiresOptional = dataTables[i].ExtendedProperties.ContainsKey(IsOptional) 
                                                              && dataTables[i].ExtendedProperties[IsOptional].Equals(true);
                        bool currentTableRequiresUnion = dataTables[i].ExtendedProperties.ContainsKey(JoinAsUnion) 
                                                           && dataTables[i].ExtendedProperties[JoinAsUnion].Equals(true);
                        bool currentTableRequiresMinus = dataTables[i].ExtendedProperties.ContainsKey(JoinAsMinus) 
                                                           && dataTables[i].ExtendedProperties[JoinAsMinus].Equals(true);

                        //Diff the previous table against the current one (which is replaced)
                        dataTables[i] = DiffJoinTables(dataTables[i-1], dataTables[i]);

                        //Restore extended attributes of the current table
                        dataTables[i].ExtendedProperties.Clear();
                        dataTables[i].ExtendedProperties.Add(IsOptional, currentTableRequiresOptional);
                        dataTables[i].ExtendedProperties.Add(JoinAsUnion, currentTableRequiresUnion);
                        dataTables[i].ExtendedProperties.Add(JoinAsMinus, currentTableRequiresMinus);

                        //Clear the previous table and flag it as logically deleted
                        dataTables[i-1].Rows.Clear();
                        dataTables[i-1].ExtendedProperties.Clear();
                        dataTables[i-1].ExtendedProperties.Add(LogicallyDeleted, true);

                        //Signal that at least one MinusWithNext has been performed
                        hasProcessedMinus = true;
                    }
                }
                return hasProcessedMinus;
            }
            void ComputeJoins(bool needsOuterJoin)
            {
                finalTable = dataTables[0];
                for (int i = 1; i < dataTables.Count; i++)
                {
                    //Switch to OuterJoin when encountering Optional behavior (or always, in case we come from Union behavior)
                    needsOuterJoin |= dataTables[i].ExtendedProperties.ContainsKey(IsOptional)
                                       && dataTables[i].ExtendedProperties[IsOptional].Equals(true);

                    //Proceed with most proper join strategy for current table
                    finalTable = needsOuterJoin ? OuterJoinTables(finalTable, dataTables[i]) 
                                                : InnerJoinTables(finalTable, dataTables[i]);
                }
            }
            #endregion

            //Step 1: process Union operators
            bool hasDoneUnions = ProcessUnions();
            if (hasDoneUnions)
                dataTables.RemoveAll(dt => dt.ExtendedProperties.ContainsKey(LogicallyDeleted) 
                                            && dt.ExtendedProperties[LogicallyDeleted].Equals(true));

            //Step 2: process Minus operators
            bool hasDoneMinus = ProcessMinus();
            if (hasDoneMinus)
                dataTables.RemoveAll(dt => dt.ExtendedProperties.ContainsKey(LogicallyDeleted) 
                                            && dt.ExtendedProperties[LogicallyDeleted].Equals(true));

            //Step 3: compute joins
            ComputeJoins(hasDoneUnions);

            return finalTable;
        }

        /// <summary>
        /// Applies the projection operator on the given table, based on the given query's projection variables
        /// </summary>
        internal static DataTable ProjectTable(RDFSelectQuery query, DataTable table)
        {
            //Projection expression variables
            ProjectExpressions(query, table);

            //Execute configured sort modifiers
            RDFOrderByModifier[] orderByModifiers = query.GetModifiers().OfType<RDFOrderByModifier>().ToArray();
            if (orderByModifiers.Length > 0)
            {
                table = orderByModifiers.Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));
                table = table.DefaultView.ToTable();
            }

            //Execute projection algorithm
            if (query.ProjectionVars.Count > 0)
            {
                //Remove non-projection variables
                DataColumn[] tableColumns = table.Columns.OfType<DataColumn>().ToArray();
                foreach (DataColumn tableColumn in tableColumns.Where(tableColumn => !query.ProjectionVars.Any(projVar => string.Equals(projVar.Key.ToString(), tableColumn.ColumnName, StringComparison.OrdinalIgnoreCase))))
                    table.Columns.Remove(tableColumn);

                //Adjust projection ordinals
                foreach (KeyValuePair<RDFVariable, (int, RDFExpression)> projectionVar in query.ProjectionVars)
                {
                    string projVarString = projectionVar.Key.ToString();
                    AddColumn(table, projVarString);
                    table.Columns[projVarString].SetOrdinal(projectionVar.Value.Item1);
                }
                table.AcceptChanges();
            }
            return table;
        }

        /// <summary>
        /// Fills the given table with new columns from the given query's projection expressions
        /// </summary>
        internal static void ProjectExpressions(RDFSelectQuery query, DataTable table)
        {
            foreach (KeyValuePair<RDFVariable, (int, RDFExpression)> projectionExpression in query.ProjectionVars.OrderBy(pv => pv.Value.Item1)
                                                                                                                 .Where(pv => pv.Value.Item2 != null))
                EvaluateExpression(projectionExpression.Value.Item2, projectionExpression.Key, table);
        }

        /// <summary>
        /// Fills the given table with new column from the given bind's variable
        /// </summary>
        internal static void ProjectBind(RDFBind bind, DataTable table)
            => EvaluateExpression(bind.Expression, bind.Variable, table);

        /// <summary>
        /// Evaluates the given expression on the given table and projects the given variable
        /// </summary>
        internal static void EvaluateExpression(RDFExpression expression, RDFVariable variable, DataTable table)
        {
            table.BeginLoadData();

            string bindVariable = variable.ToString();
            if (!table.Columns.Contains(bindVariable))
            {
                //Project bind column
                AddColumn(table, bindVariable);

                //Valorize bind column
                if (table.Rows.Count == 0)
                {
                    //Ensure to add the row only in case the expression has evaluated without binding errors,
                    //(otherwise in this scenario we would always answer true for ASK queries due to this row)
                    RDFPatternMember bindResult = expression.ApplyExpression(table.NewRow());
                    if (bindResult != null)
                        AddRow(table, new Dictionary<string, string> { { bindVariable, bindResult.ToString() } });
                }
                else
                {
                    foreach (DataRow row in table.AsEnumerable())
                        row[bindVariable] = expression.ApplyExpression(row)?.ToString();
                }
            }

            table.EndLoadData();
            table.AcceptChanges();
        }
        #endregion

        #endregion
    }
}