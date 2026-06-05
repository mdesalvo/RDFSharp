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
        internal Dictionary<long, List<RDFTable>> PatternGroupMemberResultTables { get; set; }

        /// <summary>
        /// Dictionary of result tables produced by evaluation of query members
        /// </summary>
        internal Dictionary<long, RDFTable> QueryMemberResultTables { get; set; }

        /// <summary>
        /// Attribute denoting an optional pattern/patternGroup/query
        /// </summary>
        internal const string IsOptional = nameof(IsOptional);

        /// <summary>
        /// Attribute denoting a pattern/patternGroup/query to be joined as union
        /// </summary>
        internal const string JoinAsUnion = nameof(JoinAsUnion);

        /// <summary>
        /// Attribute denoting a pattern/patternGroup/query to be joined as minus
        /// </summary>
        internal const string JoinAsMinus = nameof(JoinAsMinus);
        #endregion

        #region Ctors
        /// <summary>
        /// Initializes a query engine instance
        /// </summary>
        internal RDFQueryEngine()
        {
            PatternGroupMemberResultTables = new Dictionary<long, List<RDFTable>>();
            QueryMemberResultTables = new Dictionary<long, RDFTable>();
        }
        #endregion

        #region Methods

        #region MIRELLA SPARQL
        /// <summary>
        /// Evaluates the given SPARQL query on the given RDF datasource
        /// </summary>
        private RDFTable EvaluateQuery(RDFQuery query, RDFDataSource datasource)
        {
            RDFTable queryResultTable = new RDFTable();

            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Count > 0)
            {
                //Evaluate the active members of the query
                EvaluateQueryMembers(evaluableQueryMembers, datasource);

                //Combine intermediate results into final table
                queryResultTable = CombineTables(QueryMemberResultTables.Values.ToList());
            }

            return queryResultTable;
        }

        /// <summary>
        /// Evaluates the given SPARQL SELECT query on the given RDF datasource
        /// </summary>
        internal RDFSelectQueryResult EvaluateSelectQuery(RDFSelectQuery selectQuery, RDFDataSource datasource)
        {
            //Evaluate the body of the query
            RDFTable queryResultTable = EvaluateQuery(selectQuery, datasource);

            //Evaluate the modifiers of the query
            RDFTable finalTable = ApplyModifiers(selectQuery, queryResultTable);

            //Expose the result of the query
            DataTable selectResults = finalTable.ToDataTable();
            selectResults.ExtendedProperties[IsOptional] = finalTable.IsOptional;
            selectResults.ExtendedProperties[JoinAsUnion] = finalTable.JoinAsUnion;
            selectResults.ExtendedProperties[JoinAsMinus] = finalTable.JoinAsMinus;
            return new RDFSelectQueryResult
            {
                SelectResults = selectResults
            };
        }

        /// <summary>
        /// Evaluates the given SPARQL DESCRIBE query on the given RDF datasource
        /// </summary>
        internal RDFDescribeQueryResult EvaluateDescribeQuery(RDFDescribeQuery describeQuery, RDFDataSource datasource)
        {
            #region Utilities
            RDFTable FillDescribeTerms(RDFTable qResultTable)
            {
                RDFTable resultTable = new RDFTable();
                if (datasource.IsFederation())
                {
                    foreach (RDFDataSource fedDataSource in ((RDFFederation)datasource).Where(fedDataSource => !fedDataSource.IsFederation() || ((RDFFederation)fedDataSource).DataSourcesCount != 0))
                        MergeTable(resultTable, DescribeTerms(describeQuery, fedDataSource, qResultTable));
                }
                else
                {
                    resultTable = DescribeTerms(describeQuery, datasource, qResultTable);
                }
                return resultTable;
            }
            #endregion

            //Evaluate the body of the query
            RDFTable queryResultTable = EvaluateQuery(describeQuery, datasource);

            //Expose the result of the query
            return new RDFDescribeQueryResult
            {
                DescribeResults = ApplyModifiers(describeQuery, FillDescribeTerms(queryResultTable)).ToDataTable()
            };
        }

        /// <summary>
        /// Evaluates the given SPARQL CONSTRUCT query on the given RDF datasource
        /// </summary>
        internal RDFConstructQueryResult EvaluateConstructQuery(RDFConstructQuery constructQuery, RDFDataSource datasource)
        {
            //Evaluate the body of the query
            RDFTable queryResultTable = EvaluateQuery(constructQuery, datasource);

            //Expose the result of the query
            return new RDFConstructQueryResult
            {
                ConstructResults = ApplyModifiers(constructQuery, FillTemplates(constructQuery.Templates, queryResultTable, false)).ToDataTable()
            };
        }

        /// <summary>
        /// Evaluates the given SPARQL ASK query on the given RDF datasource
        /// </summary>
        internal RDFAskQueryResult EvaluateAskQuery(RDFAskQuery askQuery, RDFDataSource datasource)
        {
            //Evaluate the body of the query
            RDFTable queryResultTable = EvaluateQuery(askQuery, datasource);

            //Expose the result of the query
            return new RDFAskQueryResult
            {
                 AskResult = queryResultTable.RowsCount > 0
            };
        }

        /// <summary>
        /// Evaluates the given list of query members against the given datasource
        /// </summary>
        internal void EvaluateQueryMembers(List<RDFQueryMember> evaluableQueryMembers, RDFDataSource datasource)
        {
            foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
                switch (evaluableQueryMember)
                {
                    case RDFPatternGroup patternGroup:
                        //Get the intermediate result tables of the patternGroup
                        EvaluatePatternGroup(patternGroup, datasource);

                        //Get the result table of the patternGroup
                        FinalizePatternGroup(patternGroup);

                        //Apply the filters of the patternGroup to its result table
                        ApplyFilters(patternGroup);
                        break;

                    case RDFSelectQuery subQuery:
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = subQuery.ApplyToDataSource(datasource);

                        //Expose it in internal format
                        RDFTable subQueryTable = RDFTable.FromDataTable(subQueryResult.SelectResults);
                        subQueryTable.IsOptional = subQuery.IsOptional || subQueryResult.SelectResults.ExtendedProperties[IsOptional] is true;
                        subQueryTable.JoinAsUnion = subQuery.JoinAsUnion;
                        subQueryTable.JoinAsMinus = subQuery.JoinAsMinus;

                        //Save updates
                        QueryMemberResultTables[subQuery.QueryMemberID] = subQueryTable;
                        break;
                }
        }

        /// <summary>
        /// Gets the intermediate result tables of the given patternGroup
        /// </summary>
        internal void EvaluatePatternGroup(RDFPatternGroup patternGroup, RDFDataSource dataSource)
        {
            //Grab the set of evaluable patternGroup members
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers()
                                                                         .Distinct()
                                                                         .ToList();

            //Determine if query optimizations are eligible on the given set of patternGroup members
            //(tries to optimize the execution order of patterns within reorderable inner-join blocks)
            if (dataSource is RDFGraph || dataSource is RDFMemoryStore)
                evaluablePGMembers = RDFQueryOptimizer.OptimizePatternGroup(evaluablePGMembers, dataSource);

            //Before starting effective evaluation, initialize the list of result tables for this patternGroup
            PatternGroupMemberResultTables[patternGroup.QueryMemberID] = new List<RDFTable>(evaluablePGMembers.Count);

            //**Service** evaluation => send it querified to SPARQL endpoint
            if (patternGroup.EvaluateAsService.HasValue)
            {
                //Cleanup patternGroup in order to stringify into vanilla "SELECT *"
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

                //Restore patternGroup to its official state
                patternGroup.IsOptional = isOptional;
                patternGroup.JoinAsUnion = joinAsUnion;
                patternGroup.JoinAsMinus = joinAsMinus;
                patternGroup.EvaluateAsService = asService;

                //Set metadata of the result table
                serviceResultsTable.IsOptional = patternGroup.IsOptional;
                serviceResultsTable.JoinAsUnion = patternGroup.JoinAsUnion;
                serviceResultsTable.JoinAsMinus = patternGroup.JoinAsMinus;

                //Save the result table
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
                            //Set metadata of the result table
                            patternResultsTable.IsOptional = pattern.IsOptional;
                            patternResultsTable.JoinAsUnion = pattern.JoinAsUnion;
                            patternResultsTable.JoinAsMinus = pattern.JoinAsMinus;
                            //Save the result table
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(patternResultsTable);
                            break;

                        case RDFPropertyPath propertyPath:
                            //Evaluate property path on the given data source
                            RDFTable pPathResultsTable = ApplyPropertyPath(propertyPath, dataSource);
                            //Set metadata of the result table
                            pPathResultsTable.IsOptional = propertyPath.IsOptional;
                            pPathResultsTable.JoinAsUnion = propertyPath.JoinAsUnion;
                            pPathResultsTable.JoinAsMinus = propertyPath.JoinAsMinus;
                            //Save the result table
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(pPathResultsTable);
                            break;

                        case RDFValues values:
                            //Transform SPARQL values into an equivalent filter
                            RDFValuesFilter valuesFilter = values.GetValuesFilter();
                            //Save the result table
                            PatternGroupMemberResultTables[patternGroup.QueryMemberID].Add(valuesFilter.ValuesTable);
                            //Inject SPARQL values filter
                            patternGroup.AddFilter(valuesFilter);
                            break;

                        case RDFBind bind:
                            //Bind operator is evaluated like an "artificial ending" of the patternGroup:
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
            //Grab the set of evaluable patternGroup members
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers()
                                                                         .Distinct()
                                                                         .ToList();
            if (evaluablePGMembers.Count > 0)
            {
                //Populate patternGroup result table
                RDFTable patternGroupResultTable = CombineTables(PatternGroupMemberResultTables[patternGroup.QueryMemberID]);

                //Populate its metadata (IsOptional)
                patternGroupResultTable.IsOptional = patternGroup.IsOptional || patternGroupResultTable.IsOptional;

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
            //Grab the set of evaluable patternGroup members
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers()
                                                                         .Distinct()
                                                                         .ToList();
            List<RDFFilter> filters = patternGroup.GetFilters().ToList();
            if (evaluablePGMembers.Count > 0 && filters.Count > 0)
            {
                RDFTable patternGroupMemberTable = QueryMemberResultTables[patternGroup.QueryMemberID];
                RDFTable filteredPatternGroupMemberTable = patternGroupMemberTable.Clone();
                int columnsCount = patternGroupMemberTable.ColumnsCount;

                //Iterate the rows of the pattern group's result table
                foreach (RDFTableRow currentRow in patternGroupMemberTable.Rows)
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
                        string[] cells = new string[columnsCount];
                        for (int c = 0; c < columnsCount; c++)
                            cells[c] = currentRow[c];
                        filteredPatternGroupMemberTable.AddRow(cells);
                    }
                }

                //Save the result table
                QueryMemberResultTables[patternGroup.QueryMemberID] = filteredPatternGroupMemberTable;
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

            //Carry the incoming join flags through the modifiers onto the query result table
            table.IsOptional = inOptional;
            table.JoinAsUnion = inUnion;
            table.JoinAsMinus = inMinus;
            return table;
        }

        /// <summary>
        /// Fills the given templates with data from the given result table<br/>
        /// (needsContext flag is true only when the caller is a store operation)
        /// </summary>
        internal RDFTable FillTemplates(List<RDFPattern> templates, RDFTable resultTable, bool needsContext)
        {
            //Create the structure of the result table
            RDFTable result = new RDFTable();
            if (needsContext)
                result.AddColumn("?CONTEXT");
            result.AddColumn("?SUBJECT");
            result.AddColumn("?PREDICATE");
            result.AddColumn("?OBJECT");

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
                                                                   || tp.Variables.TrueForAll(v => resultTable.HasColumn(v.ToString()))))
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
                    result.AddRow(bindings);
                    continue;
                }
                #endregion

                #region NON-GROUND TEMPLATE
                foreach (RDFTableRow resultRow in resultTable.Rows)
                {
                    #region CONTEXT
                    if (needsContext)
                    {
                        //Context of the template is a variable
                        if (template.Context is RDFVariable)
                        {
                            //Check if the template must be skipped, in order to not produce illegal triples
                            //Row contains an unbound value in position of the variable corresponding to the template context
                            if (resultRow.IsUnbound(templateCtx))
                                continue;

                            RDFPatternMember ctx = ParseRDFPatternMember(resultRow[templateCtx]);
                            //Row contains a literal in position of the variable corresponding to the template context
                            if (ctx is RDFLiteral)
                                continue;
                            //Row contains a resource in position of the variable corresponding to the template context
                            bindings["?CONTEXT"] = ctx.ToString();
                        }
                        //Context of the template is a resource
                        else
                        {
                            bindings["?CONTEXT"] = templateCtx ?? defaultContext;
                        }
                    }
                    #endregion

                    #region SUBJECT
                    //Subject of the template is a variable
                    if (template.Subject is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template subject
                        if (resultRow.IsUnbound(templateSubj))
                            continue;

                        RDFPatternMember subj = ParseRDFPatternMember(resultRow[templateSubj]);
                        //Row contains a literal in position of the variable corresponding to the template subject
                        if (subj is RDFLiteral)
                            continue;
                        //Row contains a resource in position of the variable corresponding to the template subject
                        bindings["?SUBJECT"] = subj.ToString();
                    }
                    //Subject of the template is a resource
                    else
                    {
                        bindings["?SUBJECT"] = templateSubj;
                    }
                    #endregion

                    #region PREDICATE
                    //Predicate of the template is a variable
                    if (template.Predicate is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template predicate
                        if (resultRow.IsUnbound(templatePred))
                            continue;

                        RDFPatternMember pred = ParseRDFPatternMember(resultRow[templatePred]);
                        //Row contains a blank resource or a literal in position of the variable corresponding to the template predicate
                        if ((pred is RDFResource predRes && predRes.IsBlank) || pred is RDFLiteral)
                            continue;
                        //Row contains a non-blank resource in position of the variable corresponding to the template predicate
                        bindings["?PREDICATE"] = pred.ToString();
                    }
                    //Predicate of the template is a resource
                    else
                    {
                        bindings["?PREDICATE"] = templatePred;
                    }
                    #endregion

                    #region OBJECT
                    //Object of the template is a variable
                    if (template.Object is RDFVariable)
                    {
                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template object
                        if (resultRow.IsUnbound(templateObj))
                            continue;

                        RDFPatternMember obj = ParseRDFPatternMember(resultRow[templateObj]);
                        //Row contains a resource or a literal in position of the variable corresponding to the template object
                        bindings["?OBJECT"] = obj.ToString();
                    }
                    //Object of the template is a resource or a literal
                    else
                    {
                        bindings["?OBJECT"] = templateObj;
                    }
                    #endregion

                    //Insert the triple into the final table
                    result.AddRow(bindings);
                }
                #endregion
            }

            return result;
        }

        /// <summary>
        /// Describes the terms of the given DESCRIBE query with data from the given result table
        /// </summary>
        internal RDFTable DescribeTerms(RDFDescribeQuery describeQuery, RDFDataSource dataSource, RDFTable resultTable)
        {
            //Create the structure of the result table
            RDFTable result = new RDFTable();
            if (dataSource.IsStore())
                result.AddColumn("?CONTEXT");
            result.AddColumn("?SUBJECT");
            result.AddColumn("?PREDICATE");
            result.AddColumn("?OBJECT");

            //In case of "DESCRIBE *" query, all the variables must be considered describe terms
            if (describeQuery.DescribeTerms.Count == 0)
                FetchDescribeVariablesFromQueryMembers(describeQuery, describeQuery.GetEvaluableQueryMembers());

            //Iterate the describe terms of the query
            foreach (RDFPatternMember describeTerm in describeQuery.DescribeTerms)
                switch (describeTerm)
                {
                    case RDFResource describeResource:
                        MergeTable(result, DescribeResourceTerm(describeResource, dataSource, result));
                        break;

                    case RDFVariable describeVariable:
                        MergeTable(result, DescribeVariableTerm(describeVariable, dataSource, result, resultTable));
                        break;
                }

            return result;
        }

        /// <summary>
        /// Describes the given resource term with data from the given result table
        /// </summary>
        internal RDFTable DescribeResourceTerm(RDFResource describeResource, RDFDataSource dataSource, RDFTable describeTemplate)
        {
            #region Utilities
            RDFGraph QueryGraph(RDFGraph dsGraph)
                => describeResource.IsBlank
                   ? dsGraph[s: describeResource]
                      .UnionWith(dsGraph[o: describeResource])
                   : dsGraph[s: describeResource]
                      .UnionWith(dsGraph[p: describeResource])
                      .UnionWith(dsGraph[o: describeResource]);

            RDFMemoryStore QueryStore(RDFStore dsStore)
                => describeResource.IsBlank
                   ? dsStore[s: describeResource]
                      .UnionWith(dsStore[o: describeResource])
                   : dsStore[c: new RDFContext(describeResource.URI)]
                      .UnionWith(dsStore[s: describeResource])
                      .UnionWith(dsStore[p: describeResource])
                      .UnionWith(dsStore[o: describeResource]);

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

            RDFTable result = describeTemplate.Clone();

            switch (dataSource)
            {
                //GRAPH
                case RDFGraph dataSourceGraph:
                    RDFGraph graph = QueryGraph(dataSourceGraph);
                    MergeTable(result, RDFTable.FromDataTable(graph.ToDataTable()));
                    break;

                //STORE
                case RDFStore dataSourceStore:
                    RDFMemoryStore store = QueryStore(dataSourceStore);
                    MergeTable(result, RDFTable.FromDataTable(store.ToDataTable()));
                    break;

                //FEDERATION / SPARQL ENDPOINT
                default:
                    RDFSelectQuery query = BuildFederationOrSPARQLEndpointQuery();
                    RDFSelectQueryResult queryResults = dataSource.IsSPARQLEndpoint() ? query.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                                                      : query.ApplyToFederation((RDFFederation)dataSource);
                    MergeTable(result, RDFTable.FromDataTable(queryResults.SelectResults));
                    break;
            }

            return result;
        }

        /// <summary>
        /// Describes the given literal term with data from the given result table
        /// </summary>
        internal RDFTable DescribeLiteralTerm(RDFLiteral describeLiteral, RDFDataSource dataSource, RDFTable describeTemplate)
        {
            #region Utilities
            RDFGraph QueryGraph(RDFGraph dsGraph)
                => dsGraph[l: describeLiteral];

            RDFMemoryStore QueryStore(RDFStore dsStore)
                => dsStore[l: describeLiteral];

            RDFSelectQuery BuildFederationOrSPARQLEndpointQuery()
                => new RDFSelectQuery()
                        .AddPatternGroup(new RDFPatternGroup()
                          .AddPattern(new RDFPattern(new RDFVariable("?SUBJECT"), new RDFVariable("?PREDICATE"), describeLiteral)));
            #endregion

            RDFTable result = describeTemplate.Clone();

            switch (dataSource)
            {
                //GRAPH
                case RDFGraph dataSourceGraph:
                    RDFGraph graph = QueryGraph(dataSourceGraph);
                    MergeTable(result, RDFTable.FromDataTable(graph.ToDataTable()));
                    break;

                //STORE
                case RDFStore dataSourceStore:
                    RDFMemoryStore store = QueryStore(dataSourceStore);
                    MergeTable(result, RDFTable.FromDataTable(store.ToDataTable()));
                    break;

                //FEDERATION / SPARQL ENDPOINT
                default:
                    RDFSelectQuery query = BuildFederationOrSPARQLEndpointQuery();
                    RDFSelectQueryResult queryResults =
                        dataSource.IsSPARQLEndpoint() ? query.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                      : query.ApplyToFederation((RDFFederation)dataSource);
                    MergeTable(result, RDFTable.FromDataTable(queryResults.SelectResults));
                    break;
            }

            return result;
        }

        /// <summary>
        /// Describes the given variable term with data from the given result table
        /// </summary>
        internal RDFTable DescribeVariableTerm(RDFVariable describeVariable, RDFDataSource dataSource, RDFTable describeTemplate, RDFTable resultTable)
        {
            RDFTable result = describeTemplate.Clone();

            //In order to be processed this variable must be a column of the results table!
            string describeVariableName = describeVariable.ToString();
            if (!resultTable.HasColumn(describeVariableName))
                return result;

            //Iterate the results table's rows to retrieve terms to be described
            foreach (RDFPatternMember describeVariableValue in
                      (from RDFTableRow resultRow
                       in resultTable.Rows
                       where !resultRow.IsUnbound(describeVariableName)
                       select ParseRDFPatternMember(resultRow[describeVariableName])))
            {
                //Execute most appropriate strategy, depending on the type of the variable value
                switch (describeVariableValue)
                {
                    //RESOURCE
                    case RDFResource describeResource:
                        RDFTable describeResourceTable = DescribeResourceTerm(describeResource, dataSource, describeTemplate);
                        MergeTable(result, describeResourceTable);
                        break;

                    //LITERAL
                    case RDFLiteral describeLiteral:
                        RDFTable describeLiteralTable = DescribeLiteralTerm(describeLiteral, dataSource, describeTemplate);
                        MergeTable(result, describeLiteralTable);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches the describe variables from the given collection of query members and propagates them to the given describe query
        /// </summary>
        internal void FetchDescribeVariablesFromQueryMembers(RDFDescribeQuery describeQuery, IEnumerable<RDFQueryMember> evaluableQueryMembers)
        {
            foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
            {
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
        }

        /// <summary>
        /// Applies the given pattern to the given data source
        /// </summary>
        internal RDFTable ApplyPattern(RDFPattern pattern, RDFDataSource dataSource)
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
            return new RDFTable();
        }

        /// <summary>
        /// Applies the given pattern to the given graph
        /// </summary>
        internal RDFTable ApplyPatternToGraph(RDFPattern pattern, RDFGraph graph)
        {
            RDFTable patternResultTable = new RDFTable();
            StringBuilder templateHoleDetector = new StringBuilder();

            //Analyze subject of the pattern
            if (pattern.Subject is RDFVariable)
            {
                templateHoleDetector.Append('S');
                patternResultTable.AddColumn(pattern.Subject.ToString());
            }

            //Analyze predicate of the pattern
            if (pattern.Predicate is RDFVariable)
            {
                templateHoleDetector.Append('P');
                patternResultTable.AddColumn(pattern.Predicate.ToString());
            }

            //Analyze object of the pattern
            bool pObjRes = pattern.Object is RDFResource;
            bool pObjLit = pattern.Object is RDFLiteral;
            if (pattern.Object is RDFVariable)
            {
                templateHoleDetector.Append('O');
                patternResultTable.AddColumn(pattern.Object.ToString());
            }

            //Analyze templateHoleDetector to refine the set of matching triples
            List<RDFTriple> matchingTriples = null;
            switch (templateHoleDetector.ToString())
            {
                case "S":
                    matchingTriples = graph.SelectTriples(p: (RDFResource)pattern.Predicate, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "P":
                    matchingTriples = graph.SelectTriples(s: (RDFResource)pattern.Subject, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "O":
                    matchingTriples = graph.SelectTriples(s: (RDFResource)pattern.Subject, p: (RDFResource)pattern.Predicate);
                    break;

                case "SP":
                    matchingTriples = graph.SelectTriples(o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same S and P variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                    break;

                case "SO":
                    matchingTriples = graph.SelectTriples(p: (RDFResource)pattern.Predicate);
                    //In case of same S and O variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                    break;

                case "PO":
                    matchingTriples = graph.SelectTriples(s: (RDFResource)pattern.Subject);
                    //In case of same P and O variable, must refine matching triples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                    break;

                case "SPO":
                    matchingTriples = graph.SelectTriples();
                    //In case of same S and P variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Predicate));
                    //In case of same S and O variable, must refine matching triples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Subject.Equals(mt.Object));
                    //In case of same P and O variable, must refine matching triples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingTriples = matchingTriples.FindAll(mt => mt.Predicate.Equals(mt.Object));
                    break;
            }

            //Fully-bound patterns (no holes) match no switch case and leave the table empty
            if (matchingTriples != null)
                PopulateTable(pattern, matchingTriples, patternResultTable);

            return patternResultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given store
        /// </summary>
        internal RDFTable ApplyPatternToStore(RDFPattern pattern, RDFStore store)
        {
            RDFTable patternResultTable = new RDFTable();
            StringBuilder templateHoleDetector = new StringBuilder();

            //Analyze context of the pattern
            bool hasContext = pattern.Context != null;
            if (hasContext && pattern.Context is RDFVariable)
            {
                templateHoleDetector.Append('C');
                patternResultTable.AddColumn(pattern.Context.ToString());
            }

            //Analyze subject of the pattern
            if (pattern.Subject is RDFVariable)
            {
                templateHoleDetector.Append('S');
                patternResultTable.AddColumn(pattern.Subject.ToString());
            }

            //Analyze predicate of the pattern
            if (pattern.Predicate is RDFVariable)
            {
                templateHoleDetector.Append('P');
                patternResultTable.AddColumn(pattern.Predicate.ToString());
            }

            //Analyze object of the pattern
            bool pObjRes = pattern.Object is RDFResource;
            bool pObjLit = pattern.Object is RDFLiteral;
            if (pattern.Object is RDFVariable)
            {
                templateHoleDetector.Append('O');
                patternResultTable.AddColumn(pattern.Object.ToString());
            }

            //Analyze templateHoleDetector to refine the set of matching quadruples
            List<RDFQuadruple> matchingQuadruples = null;
            switch (templateHoleDetector.ToString())
            {
                case "C":
                    matchingQuadruples = store.SelectQuadruples(s: (RDFResource)pattern.Subject, p: (RDFResource)pattern.Predicate, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "S":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, p: (RDFResource)pattern.Predicate, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "P":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, s: (RDFResource)pattern.Subject, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    break;

                case "O":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, s: (RDFResource)pattern.Subject, p: (RDFResource)pattern.Predicate);
                    break;

                case "CS":
                    matchingQuadruples = store.SelectQuadruples(p: (RDFResource)pattern.Predicate, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Subject));
                    break;

                case "CP":
                    matchingQuadruples = store.SelectQuadruples(s: (RDFResource)pattern.Subject, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Predicate));
                    break;

                case "CO":
                    matchingQuadruples = store.SelectQuadruples(s: (RDFResource)pattern.Subject, p: (RDFResource)pattern.Predicate);
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Object));
                    break;

                case "SP":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Predicate));
                    break;

                case "SO":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, p: (RDFResource)pattern.Predicate);
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Object));
                    break;

                case "PO":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null, s: (RDFResource)pattern.Subject);
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Predicate.Equals(mq.Object));
                    break;

                case "CSP":
                    matchingQuadruples = store.SelectQuadruples(o: pObjRes ? (RDFResource)pattern.Object : null, l: pObjLit ? (RDFLiteral)pattern.Object : null);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Subject));
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Predicate));
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Predicate));
                    break;

                case "CSO":
                    matchingQuadruples = store.SelectQuadruples(p: (RDFResource)pattern.Predicate);
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Subject));
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Object));
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Object));
                    break;

                case "CPO":
                    matchingQuadruples = store.SelectQuadruples(s: (RDFResource)pattern.Subject);
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Predicate));
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Object));
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Predicate.Equals(mq.Object));
                    break;

                case "SPO":
                    matchingQuadruples = store.SelectQuadruples(c: hasContext ? (RDFContext)pattern.Context : null);
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Predicate));
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Object));
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Predicate.Equals(mq.Object));
                    break;

                case "CSPO":
                    matchingQuadruples = store.SelectQuadruples();
                    //In case of same C and S variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Subject))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Subject));
                    //In case of same C and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Predicate));
                    //In case of same C and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Context.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Context.Equals(mq.Object));
                    //In case of same S and P variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Predicate))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Predicate));
                    //In case of same S and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Subject.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Subject.Equals(mq.Object));
                    //In case of same P and O variable, must refine matching quadruples with a further value comparison
                    if (pattern.Predicate.Equals(pattern.Object))
                        matchingQuadruples = matchingQuadruples.FindAll(mq => mq.Predicate.Equals(mq.Object));
                    break;
            }

            //Fully-bound patterns (no holes) match no switch case and leave the table empty
            if (matchingQuadruples != null)
                PopulateTable(pattern, matchingQuadruples, patternResultTable);

            return patternResultTable;
        }

        /// <summary>
        /// Applies the given pattern to the given federation
        /// </summary>
        internal RDFTable ApplyPatternToFederation(RDFPattern pattern, RDFFederation federation)
        {
            RDFTable resultTable = new RDFTable();

            //Iterate data sources of the federation
            foreach (RDFDataSource dataSource in federation)
            {
                switch (dataSource)
                {
                    case RDFGraph dataSourceGraph:
                        RDFTable graphTable = ApplyPatternToGraph(pattern, dataSourceGraph);
                        MergeTable(resultTable, graphTable);
                        break;

                    case RDFStore dataSourceStore:
                        RDFTable storeTable = ApplyPatternToStore(pattern, dataSourceStore);
                        MergeTable(resultTable, storeTable);
                        break;

                    case RDFFederation dataSourceFederation:
                        RDFTable federationTable = ApplyPatternToFederation(pattern, dataSourceFederation);
                        MergeTable(resultTable, federationTable);
                        break;

                    case RDFSPARQLEndpoint dataSourceSparqlEndpoint:
                        //Pattern is transformed into an equivalent "SELECT *" query which is sent to the SPARQL endpoint.
                        //SPARQL endpoint options are eventually retrieved directly from the federation.
                        federation.EndpointDataSourcesQueryOptions.TryGetValue(dataSourceSparqlEndpoint.ToString(), out RDFSPARQLEndpointQueryOptions dataSourceSparqlEndpointOptions);
                        RDFSelectQuery sparqlEndpointQuery =  new RDFSelectQuery()
                                                                .AddPatternGroup(new RDFPatternGroup().AddPattern(pattern));
                        RDFSelectQueryResult sparqlEndpointTable = sparqlEndpointQuery.ApplyToSPARQLEndpoint(dataSourceSparqlEndpoint, dataSourceSparqlEndpointOptions);
                        MergeTable(resultTable, RDFTable.FromDataTable(sparqlEndpointTable.SelectResults));
                        break;
                }
            }

            return resultTable;
        }

        /// <summary>
        /// Applies the given property path to the given graph
        /// </summary>
        internal RDFTable ApplyPropertyPath(RDFPropertyPath propertyPath, RDFDataSource dataSource)
        {
            //Dispatch to transitive evaluation when any step carries a cardinality constraint
            if (propertyPath.HasTransitiveSteps)
                return ApplyTransitivePropertyPath(propertyPath, dataSource);

            //Otherwise evaluate a standard "finite-set" property path
            //Translate property path into equivalent list of patterns
            List<RDFPattern> patternList = propertyPath.GetPatternList();
            List<RDFTable> patternTables = new List<RDFTable>(patternList.Count);

            //Evaluate produced list of patterns
            foreach (RDFPattern pattern in patternList)
            {
                //Apply pattern to graph
                RDFTable patternTable = ApplyPattern(pattern, dataSource);

                //Set join flags
                patternTable.IsOptional = pattern.IsOptional;
                patternTable.JoinAsUnion = pattern.JoinAsUnion;
                patternTable.JoinAsMinus = pattern.JoinAsMinus;

                //Add produced table
                patternTables.Add(patternTable);
            }

            //Merge produced list of tables
            RDFTable resultTable = CombineTables(patternTables);

            //Remove property path variables
            foreach (string ppColumn in (from RDFTableColumn ppCol
                                         in resultTable.Columns
                                         where ppCol.Name.StartsWith("?__PP", StringComparison.Ordinal)
                                         select ppCol.Name).ToArray())
            {
                resultTable.RemoveColumn(ppColumn);
            }

            return resultTable;
        }

        #region PropertyPath Engine (Transitive Closure)
        /// <summary>
        /// Applies a property path containing at least one transitive cardinality step (?, *, +, {min,max}).<br/>
        /// The datasource adjacency for every step property is materialized once into in-memory maps and the
        /// unbounded transitive closure (+, *) is memoized via strongly-connected-component condensation, so
        /// the closure is computed a single time and shared across all seeds and frontier nodes (instead of
        /// recomputing an independent BFS from each node). Seeds are pruned to the actual domain when the path
        /// cannot match a zero-length path, and a concrete end node is resolved by traversing the inverse
        /// closure from the end instead of seeding from every node in the datasource.
        /// </summary>
        internal RDFTable ApplyTransitivePropertyPath(RDFPropertyPath propertyPath, RDFDataSource dataSource)
        {
            RDFTable resultTable = new RDFTable();

            //Collect information about boundary terms of the property path
            bool startIsVar = propertyPath.Start is RDFVariable;
            bool endIsVar   = propertyPath.End   is RDFVariable;
            RDFResource startResource = propertyPath.Start as RDFResource;
            RDFResource endResource   = propertyPath.End   as RDFResource;

            //Add output columns only for the terms that are variables
            if (startIsVar)
                resultTable.AddColumn(propertyPath.Start.ToString());
            if (endIsVar)
                resultTable.AddColumn(propertyPath.End.ToString());

            //Track already-added (start, end) pairs to avoid duplicate rows
            HashSet<string> addedRows = new HashSet<string>();

            #region Utilities
            //Emits one result row for the (s, e) pair, applying concrete-term filters and deduplication
            void AddBindingRow(RDFResource s, RDFResource e)
            {
                //Skip if concrete start does not match the seed
                if (!startIsVar && !s.Equals(startResource))
                    return;
                //Skip if concrete end does not match the reached node
                if (!endIsVar && !e.Equals(endResource))
                    return;

                //Deduplicate by the variable portion of the key
                string key = (startIsVar ? s.ToString() : string.Empty) + "|" + (endIsVar ? e.ToString() : string.Empty);
                if (!addedRows.Add(key))
                    return;

                if (startIsVar || endIsVar)
                {
                    //At least one variable: populate the corresponding columns
                    Dictionary<string, string> row = new Dictionary<string, string>();
                    if (startIsVar)
                        row[propertyPath.Start.ToString()] = s.ToString();
                    if (endIsVar)
                        row[propertyPath.End.ToString()]   = e.ToString();
                    resultTable.AddRow(row);
                }
                else
                {
                    //Both terms are concrete: emit a blank existence row (pattern matched)
                    resultTable.AddRow(new string[resultTable.ColumnsCount]);
                }
            }
            #endregion

            //Materialize the datasource adjacency once and reuse it (plus memoized closures) across all seeds
            RDFTransitivePathCache transitivePathCache = new RDFTransitivePathCache(dataSource);

            //Fast path: concrete end + variable start on a single OneOrMore step, e.g. "?s prop+ <end>".
            //The naive plan seeds from every node and keeps only those reaching <end>. But "x reaches end over
            //forward edges" is the same statement as "end reaches x over REVERSED edges": the set of valid
            //starts is precisely the forward closure of the relation read backwards, evaluated once from <end>.
            //So we flip the traversal direction (!IsInverseStep) and emit one row per node the end can reach,
            //turning an O(V) seed sweep into a single closure lookup.
            if (startIsVar && !endIsVar
                 && propertyPath.Steps.Count == 1
                 && propertyPath.Steps[0].StepCardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore)
            {
                RDFPropertyPathStep onlyStep = propertyPath.Steps[0];
                foreach (RDFResource startNode in transitivePathCache.GetTransitiveClosureindex(onlyStep.StepProperty, !onlyStep.IsInverseStep)
                                                                     .EnumerateReachableNodes(endResource))
                {
                    AddBindingRow(startNode, endResource);
                }
                return resultTable;
            }

            //Determine the seed set, pruning it to the actual domain whenever the path cannot produce a
            //zero-length (identity) match; otherwise every resource node in the datasource is a candidate
            foreach (RDFResource seed in GetTransitiveSeeds(propertyPath, startIsVar, startResource, transitivePathCache))
            {
                foreach (RDFResource reached in EvaluateStepsFromNode(seed, propertyPath.Steps, transitivePathCache))
                    AddBindingRow(seed, reached);
            }

            return resultTable;
        }

        /// <summary>
        /// Computes the seed set for a transitive property path evaluation.<br/>
        /// A concrete start yields a single seed. A variable start over a single step that requires at least
        /// one hop (+ or {min,max} with min &gt;= 1) is pruned to the nodes actually having an outgoing edge on
        /// the step property (its domain). In every other case (?, *, {0,n} or multi-step paths) a zero-length
        /// match is possible, so all resource nodes in the datasource remain candidate seeds.
        /// </summary>
        private IEnumerable<RDFResource> GetTransitiveSeeds(RDFPropertyPath propertyPath, bool startIsVar, RDFResource startResource, RDFTransitivePathCache transitivePathCache)
        {
            if (!startIsVar)
                return new List<RDFResource> { startResource };

            if (propertyPath.Steps.Count == 1)
            {
                RDFPropertyPathStep step = propertyPath.Steps[0];
                bool requiresHop = step.StepCardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore
                                    || (step.StepCardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.BoundedRange && step.MinCardinality >= 1);
                if (requiresHop)
                    return transitivePathCache.GetSources(step.StepProperty, step.IsInverseStep);
            }

            return GetAllResourceNodes(transitivePathCache.DataSource);
        }

        /// <summary>
        /// Evaluates all path steps from the given start node, returning the set of reachable end nodes.<br/>
        /// This is the relational composition of the steps: <c>current</c> is the frontier (the set of nodes
        /// reached so far), and each step transforms it into the next frontier. Sequence-flavored steps compose
        /// (the output of one feeds the input of the next); a run of consecutive Alternative-flavored steps forms
        /// a single group whose branches are taken in parallel and unioned, modelling "a | b | c" within the path.
        /// </summary>
        private List<RDFResource> EvaluateStepsFromNode(RDFResource startNode, List<RDFPropertyPathStep> steps, RDFTransitivePathCache transitivePathCache)
        {
            //The frontier starts as the single seed node and is rewritten group by group
            List<RDFResource> current = new List<RDFResource> { startNode };

            int i = 0;
            while (i < steps.Count)
            {
                //Start a new group with the current step; if it is Alternative, keep collecting
                //consecutive Alternative steps (they form a single union-branch of the path)
                List<RDFPropertyPathStep> group = new List<RDFPropertyPathStep> { steps[i] };
                if (steps[i].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                {
                    while (i + 1 < steps.Count && steps[i + 1].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                        group.Add(steps[++i]);
                }
                i++;

                //Expand every current frontier node through the group, deduplicating by pattern member hash
                Dictionary<long, RDFResource> next = new Dictionary<long, RDFResource>();
                foreach (RDFResource node in current)
                {
                    IEnumerable<RDFResource> reached = group.Count == 1 ? EvaluateSingleStepFromNode(node, group[0], transitivePathCache)
                                                                        : group.SelectMany(step => EvaluateSingleStepFromNode(node, step, transitivePathCache));
                    foreach (RDFResource r in reached)
                        next[r.PatternMemberID] = r;
                }
                current = next.Values.ToList();
            }

            return current;
        }

        /// <summary>
        /// Evaluates a single path step from the given node according to its cardinality. The optional/star
        /// variants are expressed in terms of the others: adding the node itself models the zero-hop (reflexive)
        /// match, so <c>?</c> = self ∪ one-hop and <c>*</c> = self ∪ <c>+</c>.<br/>
        /// - ExactlyOne   → one direct hop via the step property<br/>
        /// - ZeroOrOne    → node itself plus at most one direct hop (? operator)<br/>
        /// - OneOrMore    → the memoized transitive closure, one hop or more (+ operator)<br/>
        /// - ZeroOrMore   → node itself plus the memoized transitive closure (* operator)<br/>
        /// - BoundedRange → BFS keeping only nodes at depth [min, max], including node itself when min is 0
        /// </summary>
        private IEnumerable<RDFResource> EvaluateSingleStepFromNode(RDFResource node, RDFPropertyPathStep step, RDFTransitivePathCache transitivePathCache)
        {
            switch (step.StepCardinality)
            {
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne:
                {
                    //Include the node itself (zero hops) and any direct successor (one hop)
                    Dictionary<long, RDFResource> result = new Dictionary<long, RDFResource> { [node.PatternMemberID] = node };
                    foreach (RDFResource r in GetDirectSuccessors(node, step.StepProperty, step.IsInverseStep, transitivePathCache))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                case RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore:
                    //At least one hop: reuse the memoized transitive closure of the step property
                    return transitivePathCache.GetTransitiveClosureindex(step.StepProperty, step.IsInverseStep).EnumerateReachableNodes(node);

                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore:
                {
                    //Include the node itself (zero hops) and all closure-reachable nodes
                    Dictionary<long, RDFResource> result = new Dictionary<long, RDFResource> { [node.PatternMemberID] = node };
                    foreach (RDFResource r in transitivePathCache.GetTransitiveClosureindex(step.StepProperty, step.IsInverseStep).EnumerateReachableNodes(node))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                case RDFQueryEnums.RDFPropertyPathStepCardinalities.BoundedRange:
                {
                    Dictionary<long, RDFResource> result = new Dictionary<long, RDFResource>();
                    //When min is 0 the start node is a valid result (zero hops)
                    if (step.MinCardinality == 0)
                        result[node.PatternMemberID] = node;
                    foreach (RDFResource r in BFSReachable(node, step.StepProperty, step.IsInverseStep, transitivePathCache, step.MinCardinality, step.MaxCardinality))
                        result[r.PatternMemberID] = r;
                    return result.Values;
                }

                default: // ExactlyOne
                    return GetDirectSuccessors(node, step.StepProperty, step.IsInverseStep, transitivePathCache);
            }
        }

        /// <summary>
        /// Returns the resources reachable from the given node in exactly one hop via the given property,
        /// reading from the pre-materialized adjacency successorsMap held by <paramref name="transitivePathCache"/>.<br/>
        /// When <paramref name="inverse"/> is true, traversal goes in the opposite direction (object → subject).
        /// </summary>
        private List<RDFResource> GetDirectSuccessors(RDFResource node, RDFResource property, bool inverse, RDFTransitivePathCache transitivePathCache)
        {
            Dictionary<long, List<RDFResource>> successorsMap = transitivePathCache.GetSuccessorsMap(property, inverse);
            return successorsMap.TryGetValue(node.PatternMemberID, out List<RDFResource> successors) ? successors : EmptyResourceList;
        }

        /// <summary>
        /// Returns all distinct resource nodes (subjects and resource-typed objects) present in the given datasource.<br/>
        /// Used to seed evaluation when the path start is a variable and zero-length matches are possible. Literal
        /// objects are excluded because they cannot appear as subjects in further hops.
        /// </summary>
        private static List<RDFResource> GetAllResourceNodes(RDFDataSource dataSource)
        {
            HashSet<long> seen  = new HashSet<long>();
            List<RDFResource> nodes = new List<RDFResource>();

            #region Utilities
            //Add r to the list only the first time its hash is encountered
            void CollectNode(RDFResource r)
            {
                if (r != null && seen.Add(r.PatternMemberID))
                    nodes.Add(r);
            }
            #endregion

            switch (dataSource)
            {
                case RDFGraph graph:
                    foreach (RDFTriple triple in graph.SelectTriples())
                    {
                        if (triple.Subject is RDFResource tSubj)
                            CollectNode(tSubj);
                        if (triple.Object is RDFResource tObj)
                            CollectNode(tObj);
                    }
                    break;

                case RDFStore store:
                    foreach (RDFQuadruple quadruple in store.SelectQuadruples())
                    {
                        if (quadruple.Subject is RDFResource qSubj)
                            CollectNode(qSubj);
                        if (quadruple.Object is RDFResource qObj)
                            CollectNode(qObj);
                    }
                    break;

                case RDFFederation federation:
                    foreach (RDFDataSource federationMember in federation)
                    {
                        foreach (RDFResource r in GetAllResourceNodes(federationMember))
                            CollectNode(r);
                    }
                    break;
            }
            return nodes;
        }

        /// <summary>
        /// Returns all resource nodes reachable from <paramref name="startNode"/> by traversing
        /// <paramref name="property"/> repeatedly via BFS over the pre-materialized adjacency map, collecting
        /// only nodes whose depth falls within [<paramref name="minHops"/>, <paramref name="maxHops"/>].
        /// Pass <paramref name="maxHops"/> = -1 for an unbounded search.
        /// Cycles are handled by the <c>enqueued</c> set, which prevents re-enqueuing already-seen nodes.
        /// </summary>
        private List<RDFResource> BFSReachable(RDFResource startNode, RDFResource property, bool inverse, RDFTransitivePathCache cache, int minHops, int maxHops)
        {
            List<RDFResource> result = new List<RDFResource>();

            //Initialize data structures for beginning of BFS visit
            HashSet<long> collected = new HashSet<long>();
            HashSet<long> enqueued = new HashSet<long> { startNode.PatternMemberID };
            Queue<(RDFResource node, int depth)> queue = new Queue<(RDFResource, int)>();
            queue.Enqueue((startNode, 0));

            while (queue.Count > 0)
            {
                //Dequeue the node to be visited
                (RDFResource current, int depth) = queue.Dequeue();

                //Do not expand beyond the maximum depth (avoids unnecessary work when bounded)
                if (maxHops >= 0 && depth >= maxHops)
                    continue;

                foreach (RDFResource neighbor in GetDirectSuccessors(current, property, inverse, cache))
                {
                    int newDepth = depth + 1;

                    //Collect this neighbor if it satisfies the minimum hop constraint and it has not
                    //been collected yet. Note: startNode itself can be a valid result when a cycle
                    //closes back to it (e.g. alice->bob->carol->alice with OneOrMore), so we do NOT
                    //pre-seed collected with startNode — only enqueued is pre-seeded to stop re-expansion.
                    if (newDepth >= minHops && collected.Add(neighbor.PatternMemberID))
                        result.Add(neighbor);

                    //Enqueue neighbors for further expansion only if not yet enqueued
                    //(prevents infinite loops and redundant work in cyclic graphs)
                    if (enqueued.Add(neighbor.PatternMemberID) && (maxHops < 0 || newDepth < maxHops))
                        queue.Enqueue((neighbor, newDepth));
                }
            }

            return result;
        }

        /// <summary>
        /// Shared empty resource list returned for nodes with no outgoing edge on a step property.
        /// </summary>
        private static readonly List<RDFResource> EmptyResourceList = new List<RDFResource>(0);

        /// <summary>
        /// Holds, for the lifetime of a single transitive property path evaluation, the in-memory adjacency
        /// maps of every step property (materialized once from the datasource) and the lazily-built, memoized
        /// transitive closures derived from them. Reused across all seeds and frontier nodes so that the
        /// datasource is scanned and each closure computed only once.
        /// </summary>
        private sealed class RDFTransitivePathCache
        {
            internal RDFDataSource DataSource { get; }
            private readonly Dictionary<long, RDFPropertyAdjacency> byProperty = new Dictionary<long, RDFPropertyAdjacency>();

            internal RDFTransitivePathCache(RDFDataSource dataSource)
                => DataSource = dataSource;

            private RDFPropertyAdjacency GetPropertyAdjacency(RDFResource property)
            {
                if (!byProperty.TryGetValue(property.PatternMemberID, out RDFPropertyAdjacency adjacency))
                {
                    adjacency = RDFPropertyAdjacency.BuildPropertyAdjaceny(property, DataSource);
                    byProperty[property.PatternMemberID] = adjacency;
                }
                return adjacency;
            }

            /// <summary>
            /// Returns the (node hash → successors) adjacency successors adjacencyMap for the step property in the requested direction.
            /// </summary>
            internal Dictionary<long, List<RDFResource>> GetSuccessorsMap(RDFResource property, bool inverse)
            {
                RDFPropertyAdjacency adjacency = GetPropertyAdjacency(property);
                return inverse ? adjacency.Reverse : adjacency.Forward;
            }

            /// <summary>
            /// Returns the domain of the step property in the requested direction: the resources that actually
            /// have at least one outgoing edge, used to prune seeds when zero-length matches are impossible.
            /// </summary>
            internal IEnumerable<RDFResource> GetSources(RDFResource property, bool inverse)
            {
                RDFPropertyAdjacency adjacency = GetPropertyAdjacency(property);
                Dictionary<long, List<RDFResource>> adjacencyMap = inverse ? adjacency.Reverse : adjacency.Forward;
                List<RDFResource> sources = new List<RDFResource>(adjacencyMap.Count);
                foreach (long key in adjacencyMap.Keys)
                    sources.Add(adjacency.Nodes[key]);
                return sources;
            }

            /// <summary>
            /// Returns the memoized transitive closure of the step property in the requested direction,
            /// building it once on first use.
            /// </summary>
            internal RDFTransitiveClosureIndex GetTransitiveClosureindex(RDFResource property, bool inverse)
            {
                RDFPropertyAdjacency adjacency = GetPropertyAdjacency(property);
                if (inverse)
                    return adjacency.ReverseClosure ?? (adjacency.ReverseClosure = RDFTransitiveClosureIndex.BuildTransitiveClosureIndex(adjacency.Reverse, adjacency.Nodes));
                return adjacency.ForwardClosure ?? (adjacency.ForwardClosure = RDFTransitiveClosureIndex.BuildTransitiveClosureIndex(adjacency.Forward, adjacency.Nodes));
            }
        }

        /// <summary>
        /// In-memory adjacency of a single property: forward (subject → resource objects) and reverse
        /// (object → subjects) maps keyed by pattern member hash, the node registry mapping each hash back to
        /// its resource, and the lazily-built transitive closures over either direction.
        /// </summary>
        private sealed class RDFPropertyAdjacency
        {
            internal Dictionary<long, List<RDFResource>> Forward;
            internal Dictionary<long, List<RDFResource>> Reverse;
            internal Dictionary<long, RDFResource> Nodes;
            internal RDFTransitiveClosureIndex ForwardClosure;
            internal RDFTransitiveClosureIndex ReverseClosure;

            /// <summary>
            /// Scans the datasource once for the triples/quadruples carrying the given property and builds the
            /// forward and reverse adjacency maps (with duplicate edges collapsed) plus the node registry.
            /// </summary>
            internal static RDFPropertyAdjacency BuildPropertyAdjaceny(RDFResource property, RDFDataSource dataSource)
            {
                Dictionary<long, Dictionary<long, RDFResource>> forward = new Dictionary<long, Dictionary<long, RDFResource>>();
                Dictionary<long, Dictionary<long, RDFResource>> reverse = new Dictionary<long, Dictionary<long, RDFResource>>();
                Dictionary<long, RDFResource> nodes = new Dictionary<long, RDFResource>();

                #region Utilities
                void AddEdge(RDFResource subj, RDFResource obj)
                {
                    long subjHash = subj.PatternMemberID, objHash = obj.PatternMemberID;
                    nodes[subjHash] = subj;
                    nodes[objHash] = obj;

                    if (!forward.TryGetValue(subjHash, out Dictionary<long, RDFResource> fOut))
                        forward[subjHash] = fOut = new Dictionary<long, RDFResource>();
                    fOut[objHash] = obj;

                    if (!reverse.TryGetValue(objHash, out Dictionary<long, RDFResource> rOut))
                        reverse[objHash] = rOut = new Dictionary<long, RDFResource>();
                    rOut[subjHash] = subj;
                }
                #endregion

                CollectPropertyEdges(property, dataSource, AddEdge);

                return new RDFPropertyAdjacency
                {
                    Forward = FlattenSuccessorsList(forward),
                    Reverse = FlattenSuccessorsList(reverse),
                    Nodes = nodes
                };
            }

            //Walks the datasource (recursing over federation members) emitting every (subject, resource-object)
            //edge carrying the given property; literal objects are skipped as they cannot continue a path
            private static void CollectPropertyEdges(RDFResource property, RDFDataSource dataSource, Action<RDFResource, RDFResource> addEdge)
            {
                switch (dataSource)
                {
                    case RDFGraph graph:
                        foreach (RDFTriple triple in graph.SelectTriples(p: property))
                        {
                            if (triple.Object is RDFResource o)
                                addEdge((RDFResource)triple.Subject, o);
                        }
                        break;

                    case RDFStore store:
                        foreach (RDFQuadruple quadruple in store.SelectQuadruples(p: property))
                        {
                            if (quadruple.Object is RDFResource o)
                                addEdge((RDFResource)quadruple.Subject, o);
                        }
                        break;

                    case RDFFederation federation:
                        foreach (RDFDataSource member in federation)
                            CollectPropertyEdges(property, member, addEdge);
                        break;
                }
            }

            //Collapses the dedup dictionaries into plain successor lists
            private static Dictionary<long, List<RDFResource>> FlattenSuccessorsList(Dictionary<long, Dictionary<long, RDFResource>> source)
            {
                Dictionary<long, List<RDFResource>> result = new Dictionary<long, List<RDFResource>>(source.Count);
                foreach (KeyValuePair<long, Dictionary<long, RDFResource>> kv in source)
                    result[kv.Key] = new List<RDFResource>(kv.Value.Values);
                return result;
            }
        }

        /// <summary>
        /// Precomputed all-pairs transitive reachability over a single property direction, used to answer "+"
        /// (and, with the start node added by the caller, "*") in amortized output time.
        /// <para>
        /// THE PROBLEM. A property path such as "?x knows+ ?y" asks, for every node x, the set of nodes reachable
        /// from x by following one or more "knows" edges — i.e. the transitive closure of the relation. Computing
        /// this with an independent breadth-first search per node costs O(V·(V+E)) and recomputes the same
        /// sub-closures over and over; on cyclic data it also has to carefully avoid looping forever.
        /// </para>
        /// <para>
        /// THE IDEA. Two nodes that sit on a common cycle reach exactly the same set of nodes (each can reach the
        /// other and therefore everything the other reaches). Such mutually-reachable groups are the graph's
        /// STRONGLY-CONNECTED COMPONENTS (SCCs): maximal sets where every node reaches every other node. If we
        /// collapse each SCC to a single super-node we obtain the CONDENSATION graph, which is always acyclic
        /// (a DAG) — any cycle would, by definition, have been swallowed into one SCC. On a DAG reachability is
        /// trivial to accumulate with one pass in topological order, and every node of an SCC inherits the very
        /// same reachable set, so the closure is computed once per component instead of once per node.
        /// </para>
        /// <para>
        /// THE ALGORITHM. <see cref="BuildTransitiveClosureIndex"/> runs Tarjan's SCC algorithm (a single DFS, here made iterative to
        /// survive very deep chains without blowing the call stack) to label every node with its component, then
        /// builds the condensation edges and propagates reachability across them. Querying a node (<see cref="EnumerateReachableNodes"/>)
        /// is then just "emit my own component's members if it is cyclic, plus the members of every downstream
        /// component".
        /// </para>
        /// <para>
        /// THE "+" SEMANTICS. "+" requires at least one hop, so a node reaches ITSELF only if a non-trivial cycle
        /// brings it back: that happens exactly when its component is cyclic — either it has more than one member,
        /// or it is a single node carrying a self-loop edge (x knows x). A singleton component without a self-loop
        /// never reaches itself. The caller turns "+" into "*" simply by
        /// adding the start node itself (the zero-hop reflexive match).
        /// </para>
        /// </summary>
        private sealed class RDFTransitiveClosureIndex
        {
            //Component id assigned to each node hash (nodes in the same SCC share the same id)
            private readonly Dictionary<long, int> sccOf;
            //Resources belonging to each component, indexed by component id
            private readonly List<List<RDFResource>> members;
            //For each component, the set of OTHER component ids reachable from it across the condensation DAG
            //(transitively closed; never contains the component itself)
            private readonly List<HashSet<int>> reachableComponents;
            //For each component, whether it reaches itself in >= 1 hop (i.e. it is cyclic: size > 1 or self-loop)
            private readonly bool[] selfReaching;

            private RDFTransitiveClosureIndex(Dictionary<long, int> sccOf, List<List<RDFResource>> members, List<HashSet<int>> reachableComponents, bool[] selfReaching)
            {
                this.sccOf = sccOf;
                this.members = members;
                this.reachableComponents = reachableComponents;
                this.selfReaching = selfReaching;
            }

            /// <summary>
            /// Enumerates every resource reachable from the given node in one or more hops, without duplicates.
            /// </summary>
            /// <remarks>
            /// The result is the union of two disjoint families of nodes, so no de-duplication is needed:
            /// (1) the node's own component, emitted only when that component is cyclic (the "reaches itself"
            /// case of "+"); and (2) the members of every component reachable downstream in the condensation.
            /// These are disjoint because a node belongs to exactly one component, and a component never appears
            /// among its own downstream reachable components (the condensation is acyclic).
            /// </remarks>
            internal IEnumerable<RDFResource> EnumerateReachableNodes(RDFResource node)
            {
                //A node outside the relation (no incident edge on this property/direction) reaches nothing
                if (node == null || !sccOf.TryGetValue(node.PatternMemberID, out int component))
                    yield break;

                //(1) A cyclic component reaches all of its own members, including the node itself
                if (selfReaching[component])
                {
                    foreach (RDFResource member in members[component])
                        yield return member;
                }

                //(2) Plus every member of every downstream component (disjoint from the above, so no duplicates)
                foreach (int reachedComponent in reachableComponents[component])
                {
                    foreach (RDFResource member in members[reachedComponent])
                        yield return member;
                }
            }

            /// <summary>
            /// Builds the closure index from the given adjacency map and node registry by (1) discovering the
            /// strongly-connected components with Tarjan's algorithm, then (2) propagating reachability across
            /// the resulting acyclic condensation.
            /// </summary>
            internal static RDFTransitiveClosureIndex BuildTransitiveClosureIndex(Dictionary<long, List<RDFResource>> map, Dictionary<long, RDFResource> nodes)
            {
                // ───────────────────────────────────────────────────────────────────────────────────────────
                // PHASE 1 — Tarjan's strongly-connected-components algorithm.
                //
                // Tarjan runs a single depth-first search and assigns to each node two numbers:
                //   index[v]   = the order in which v was first visited (its DFS discovery time);
                //   lowLink[v] = the smallest index reachable from v's DFS subtree while staying on the search
                //                stack — intuitively, the oldest node v can "climb back" to via a back/cross edge.
                // Nodes are pushed onto a separate `tarjanStack` as they are discovered and stay there until
                // their whole component is finalized. The key invariant is: v is the ROOT of an SCC exactly when
                // lowLink[v] == index[v] — meaning nothing in v's subtree can reach an older node, so v together
                // with everything still above it on the stack forms one maximal mutually-reachable set.
                //
                // We need this as an EXPLICIT-STACK iteration rather than recursion: chains in real data can be
                // tens of thousands of edges deep, which would overflow the runtime call stack. Each work-stack
                // frame is (node v, pos) where `pos` is the index of the next neighbor of v still to examine, so
                // popping a frame resumes v's neighbor loop exactly where a recursive call would have continued.
                // ───────────────────────────────────────────────────────────────────────────────────────────
                Dictionary<long, int> index = new Dictionary<long, int>();
                Dictionary<long, int> lowLink = new Dictionary<long, int>();
                HashSet<long> onStack = new HashSet<long>();
                Stack<long> tarjanStack = new Stack<long>();
                Dictionary<long, int> sccOf = new Dictionary<long, int>();
                List<List<long>> componentHashes = new List<List<long>>();
                int nextIndex = 0;
                Stack<(long node, int pos)> work = new Stack<(long, int)>();
 
                foreach (long start in nodes.Keys)
                {
                    //Every node must be a DFS root once; skip the ones already reached by a previous DFS tree
                    if (index.ContainsKey(start))
                        continue;
 
                    work.Push((start, 0));
                    while (work.Count > 0)
                    {
                        (long v, int pos) = work.Pop();
                        List<RDFResource> neighbors = map.TryGetValue(v, out List<RDFResource> nl) ? nl : null;

                        if (pos == 0)
                        {
                            //First time we enter v: assign its discovery index, seed its low-link to itself,
                            //and place it on the Tarjan stack as a candidate SCC member
                            index[v] = nextIndex;
                            lowLink[v] = nextIndex;
                            nextIndex++;
                            tarjanStack.Push(v);
                            onStack.Add(v);
                        }
                        else
                        {
                            //We are resuming v right after fully exploring its (pos-1)-th neighbor (the child the
                            //recursive version would have just returned from). Propagate that child's low-link up:
                            //if the child could climb to an older node, so can v.
                            long childHash = neighbors[pos - 1].PatternMemberID;
                            if (lowLink[childHash] < lowLink[v])
                                lowLink[v] = lowLink[childHash];
                        }

                        //Scan v's remaining neighbors looking for the next one to descend into
                        bool recursed = false;
                        if (neighbors != null)
                        {
                            for (int n = pos; n < neighbors.Count; n++)
                            {
                                long w = neighbors[n].PatternMemberID;
                                if (!index.ContainsKey(w))
                                {
                                    //Unvisited: emulate the recursive call. Re-push v to resume AFTER w (n+1),
                                    //then push w to be explored next; break so w is processed before we continue v.
                                    work.Push((v, n + 1));
                                    work.Push((w, 0));
                                    recursed = true;
                                    break;
                                }
                                //Already visited and still on the stack ⇒ this is a back/cross edge inside the
                                //current component: v can climb back to w's (older) discovery index
                                if (onStack.Contains(w) && index[w] < lowLink[v])
                                    lowLink[v] = index[w];
                            }
                        }
                        if (recursed)
                            continue;

                        //All of v's neighbors are explored. If v never found a way back to an older node, it is
                        //the root of an SCC: everything pushed onto the Tarjan stack at or above v forms it.
                        if (lowLink[v] == index[v])
                        {
                            List<long> component = new List<long>();
                            long popped;
                            do
                            {
                                popped = tarjanStack.Pop();
                                onStack.Remove(popped);
                                //Component ids are handed out as components are finalized; see the reverse
                                //topological-order note exploited in PHASE 2 below
                                sccOf[popped] = componentHashes.Count;
                                component.Add(popped);
                            }
                            while (popped != v);
                            componentHashes.Add(component);
                        }
                    }
                }

                // ───────────────────────────────────────────────────────────────────────────────────────────
                // PHASE 2 — Condensation and reachability propagation.
                //
                // With every node labelled by component, we now treat each component as a single super-node and
                // accumulate, per component, the set of OTHER components reachable from it. Because the
                // condensation is a DAG this is a one-pass dynamic program, and the crucial enabler is the order
                // in which Tarjan emits components: it finalizes a component only AFTER finalizing everything
                // reachable from it, so component ids come out in REVERSE TOPOLOGICAL ORDER. Hence for any
                // condensation edge from→to we are guaranteed `to < from`, and processing components by ascending
                // id means every successor's reachable set is already complete when we read it.
                // ───────────────────────────────────────────────────────────────────────────────────────────

                int componentCount = componentHashes.Count;

                //Map each component's node hashes back to their resources (kept for emission by EnumerateReachableNodes)
                List<List<RDFResource>> members = new List<List<RDFResource>>(componentCount);
                for (int c = 0; c < componentCount; c++)
                {
                    List<RDFResource> memberResources = new List<RDFResource>(componentHashes[c].Count);
                    foreach (long h in componentHashes[c])
                        memberResources.Add(nodes[h]);
                    members.Add(memberResources);
                }

                //Classify each component as cyclic-or-not and collect the condensation's edges.
                //A component is cyclic (reaches itself in >= 1 hop) when it has more than one member; the
                //remaining cyclic case — a singleton with a self-loop edge — is detected while scanning edges.
                bool[] selfReaching = new bool[componentCount];
                List<HashSet<int>> condensationSucc = new List<HashSet<int>>(componentCount);
                for (int c = 0; c < componentCount; c++)
                {
                    selfReaching[c] = members[c].Count > 1;
                    condensationSucc.Add(new HashSet<int>());
                }
                foreach (KeyValuePair<long, List<RDFResource>> edges in map)
                {
                    int from = sccOf[edges.Key];
                    foreach (RDFResource neighbor in edges.Value)
                    {
                        int to = sccOf[neighbor.PatternMemberID];
                        if (from == to)
                        {
                            //An edge internal to a component. It only carries new information when it is a
                            //genuine self-loop (x→x) on a singleton: that makes the lone node reach itself.
                            if (edges.Key == neighbor.PatternMemberID)
                                selfReaching[from] = true;
                        }
                        else
                            //An edge between distinct components becomes an edge of the condensation DAG
                            condensationSucc[from].Add(to);
                    }
                }

                //Reachability DP over the DAG: reachable(c) = ⋃ over each successor s of { s } ∪ reachable(s).
                //Ascending id order is a valid evaluation order thanks to the reverse-topological numbering above.
                List<HashSet<int>> reachableComponents = new List<HashSet<int>>(componentCount);
                for (int c = 0; c < componentCount; c++)
                    reachableComponents.Add(null);
                for (int c = 0; c < componentCount; c++)
                {
                    HashSet<int> reached = new HashSet<int>();
                    foreach (int succ in condensationSucc[c])
                    {
                        reached.Add(succ);
                        reached.UnionWith(reachableComponents[succ]); //already finalized since succ < c
                    }
                    reachableComponents[c] = reached;
                }

                return new RDFTransitiveClosureIndex(sccOf, members, reachableComponents, selfReaching);
            }
        }
        #endregion

        /// <summary>
        /// Applies the given raw string query to the given SPARQL endpoint
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        internal static RDFQueryResult ApplyRawToSPARQLEndpoint(string queryType, string query, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
        {
            #region Utilities
            void AdjustVariableColumnNames(DataTable qrTable)
            {
                //Eventually adjust column names (should start with "?")
                int columnsCount = qrTable.Columns.Count;
                for (int i = 0; i < columnsCount; i++)
                {
                    if (!qrTable.Columns[i].ColumnName.StartsWith("?", StringComparison.Ordinal))
                        qrTable.Columns[i].ColumnName = $"?{qrTable.Columns[i].ColumnName}";
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
                                string queryString = $"query={HttpUtility.UrlEncode(query)}";
                                if (!string.IsNullOrEmpty(defaultGraphUri))
                                    queryString = $"using-graph-uri={HttpUtility.UrlEncode(defaultGraphUri)}&{queryString}";
                                if (!string.IsNullOrEmpty(namedGraphUri))
                                    queryString = $"using-named-graph-uri={HttpUtility.UrlEncode(namedGraphUri)}&{queryString}";
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
                    return queryResult ?? new RDFAskQueryResult();

                case "SELECT":
                    return queryResult ?? new RDFSelectQueryResult();

                case "CONSTRUCT":
                    return queryResult ?? new RDFConstructQueryResult();

                case "DESCRIBE":
                    return queryResult ?? new RDFDescribeQueryResult();
            }

            return queryResult;
        }
        #endregion

        #region MIRELLA TABLES
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
            //Plain iteration over the bindings avoids the per-row LINQ Where() iterator/closure
            //allocation, and the KeyValuePair access avoids re-looking-up the value by key
            foreach (KeyValuePair<string, string> binding in bindings)
            {
                if (table.Columns.Contains(binding.Key))
                {
                    resultRow[binding.Key] = binding.Value;
                    rowAdded = true;
                }
            }

            if (rowAdded)
                table.Rows.Add(resultRow);
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given graph
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, List<RDFTriple> triples, RDFTable resultTable)
        {
            //Resolve the target ordinal of each variable position once (-1 for non-variable positions, whose
            //ToString() is not a column name). Positions binding the same variable resolve to the same ordinal,
            //so the inequality checks apply "first key wins" dedup (in S,P,O order).
            int colS = resultTable.OrdinalOf(pattern.Subject.ToString());
            int colP = resultTable.OrdinalOf(pattern.Predicate.ToString());
            int colO = resultTable.OrdinalOf(pattern.Object.ToString());
            bool writeS = colS >= 0;
            bool writeP = colP >= 0 && colP != colS;
            bool writeO = colO >= 0 && colO != colS && colO != colP;
            int width = resultTable.ColumnsCount;

            //Iterate result graph's triples
            foreach (RDFTriple triple in triples)
            {
                string[] cells = new string[width];
                if (writeS)
                    cells[colS] = triple.Subject.ToString();
                if (writeP)
                    cells[colP] = triple.Predicate.ToString();
                if (writeO)
                    cells[colO] = triple.Object.ToString();
                resultTable.AddRow(cells);
            }
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given store
        /// </summary>
        internal static void PopulateTable(RDFPattern pattern, List<RDFQuadruple> quadruples, RDFTable resultTable)
        {
            //Resolve the target ordinal of each variable position once (-1 for non-variable positions).
            //Positions binding the same variable resolve to the same ordinal, so the inequality checks
            //apply "first key wins" dedup (in C,S,P,O order).
            string patternContext = pattern.Context?.ToString();
            int colC = patternContext != null ? resultTable.OrdinalOf(patternContext) : -1;
            int colS = resultTable.OrdinalOf(pattern.Subject.ToString());
            int colP = resultTable.OrdinalOf(pattern.Predicate.ToString());
            int colO = resultTable.OrdinalOf(pattern.Object.ToString());
            bool writeC = colC >= 0;
            bool writeS = colS >= 0 && colS != colC;
            bool writeP = colP >= 0 && colP != colC && colP != colS;
            bool writeO = colO >= 0 && colO != colC && colO != colS && colO != colP;
            int width = resultTable.ColumnsCount;

            //Iterate result store's quadruples
            foreach (RDFQuadruple quadruple in quadruples)
            {
                string[] cells = new string[width];
                if (writeC)
                    cells[colC] = quadruple.Context.ToString();
                if (writeS)
                    cells[colS] = quadruple.Subject.ToString();
                if (writeP)
                    cells[colP] = quadruple.Predicate.ToString();
                if (writeO)
                    cells[colO] = quadruple.Object.ToString();
                resultTable.AddRow(cells);
            }
        }

        /// <summary>
        /// Builds a collision-free Ordinal key over the row's common columns, or null if any of them is UNBOUND
        /// (an UNBOUND column can never take part in an equality match)
        /// </summary>
        private static string BuildJoinKey(RDFTableRow row, int[] commonOrdinals)
        {
            StringBuilder keyBuilder = new StringBuilder();
            for (int i = 0; i < commonOrdinals.Length; i++)
            {
                string cell = row[commonOrdinals[i]];
                if (cell == null)
                    return null;
                keyBuilder.Append(cell.Length).Append(':').Append(cell);
            }
            return keyBuilder.ToString();
        }

        /// <summary>
        /// Tells whether two rows are join-compatible on their common columns: for each of them either side
        /// is UNBOUND, or both are bound to the same value (Ordinal).
        /// </summary>
        private static bool AreJoinCompatible(RDFTableRow leftRow, int[] leftCommonOrdinals, RDFTableRow rightRow, int[] rightCommonOrdinals)
        {
            for (int i = 0; i < leftCommonOrdinals.Length; i++)
            {
                string leftCell = leftRow[leftCommonOrdinals[i]];
                string rightCell = rightRow[rightCommonOrdinals[i]];
                if (leftCell != null && rightCell != null && !string.Equals(leftCell, rightCell, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Resolves the names of the columns shared by both tables (already normalized, Ordinal comparison)
        /// </summary>
        private static List<string> CommonColumnNames(RDFTable leftTable, RDFTable rightTable)
        {
            List<string> commonNames = new List<string>();
            foreach (RDFTableColumn leftColumn in leftTable.Columns)
                if (rightTable.HasColumn(leftColumn.Name))
                    commonNames.Add(leftColumn.Name);
            return commonNames;
        }

        /// <summary>
        /// Joins two tables WITHOUT support for OPTIONAL/UNION (strict inner-join, or product when no common column)
        /// </summary>
        internal static RDFTable InnerJoinTables(RDFTable leftTable, RDFTable rightTable)
        {
            RDFTable joinTable = new RDFTable();
            List<string> commonNames = CommonColumnNames(leftTable, rightTable);

            //PRODUCT-JOIN (no common columns): full cartesian product, left-major
            if (commonNames.Count == 0)
            {
                foreach (RDFTableColumn leftColumn in leftTable.Columns)
                    joinTable.AddColumn(leftColumn.Name);
                foreach (RDFTableColumn rightColumn in rightTable.Columns)
                    joinTable.AddColumn(rightColumn.Name);

                int leftWidth = leftTable.ColumnsCount;
                int rightWidth = rightTable.ColumnsCount;
                foreach (RDFTableRow leftRow in leftTable.Rows)
                    foreach (RDFTableRow rightRow in rightTable.Rows)
                    {
                        string[] cells = new string[leftWidth + rightWidth];
                        for (int i = 0; i < leftWidth; i++)
                            cells[i] = leftRow[i];
                        for (int i = 0; i < rightWidth; i++)
                            cells[leftWidth + i] = rightRow[i];
                        joinTable.AddRow(cells);
                    }
                return joinTable;
            }

            //INNER-JOIN: result columns = left columns + right non-common columns (common columns kept from left)
            foreach (RDFTableColumn leftColumn in leftTable.Columns)
                joinTable.AddColumn(leftColumn.Name);
            List<int> rightNonCommonOrdinals = new List<int>();
            foreach (RDFTableColumn rightColumn in rightTable.Columns)
                if (!leftTable.HasColumn(rightColumn.Name))
                {
                    joinTable.AddColumn(rightColumn.Name);
                    rightNonCommonOrdinals.Add(rightColumn.Ordinal);
                }

            int leftColumnsWidth = leftTable.ColumnsCount;
            int joinColumnsWidth = joinTable.ColumnsCount;
            int[] leftCommonOrdinals = commonNames.Select(leftTable.OrdinalOf).ToArray();
            int[] rightCommonOrdinals = commonNames.Select(rightTable.OrdinalOf).ToArray();

            //Hash the right table by its common-column key (rows with any UNBOUND common cell can never match)
            RDFTableRowCollection rightRows = rightTable.Rows;
            Dictionary<string, List<int>> rightIndex = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (int ri = 0; ri < rightRows.Count; ri++)
            {
                string key = BuildJoinKey(rightRows[ri], rightCommonOrdinals);
                if (key != null)
                {
                    if (!rightIndex.TryGetValue(key, out List<int> bucket))
                        rightIndex[key] = bucket = new List<int>();
                    bucket.Add(ri);
                }
            }

            //Probe with the left table, preserving left-major then right order
            foreach (RDFTableRow leftRow in leftTable.Rows)
            {
                string key = BuildJoinKey(leftRow, leftCommonOrdinals);
                if (key == null || !rightIndex.TryGetValue(key, out List<int> matches))
                    continue;
                foreach (int ri in matches)
                {
                    RDFTableRow rightRow = rightRows[ri];
                    string[] cells = new string[joinColumnsWidth];
                    for (int i = 0; i < leftColumnsWidth; i++)
                        cells[i] = leftRow[i];
                    for (int k = 0; k < rightNonCommonOrdinals.Count; k++)
                        cells[leftColumnsWidth + k] = rightRow[rightNonCommonOrdinals[k]];
                    joinTable.AddRow(cells);
                }
            }
            return joinTable;
        }

        /// <summary>
        /// Partitions the right table's rows for a compatible-mappings join (OPTIONAL/UNION/MINUS): rows whose
        /// common cells are ALL bound are hash-indexed by their join key (so a fully-bound left row finds its
        /// exact matches in O(1)); rows carrying at least one UNBOUND common cell are kept apart as "wildcard"
        /// rows, because an UNBOUND cell is compatible with any value and so would escape a plain hash lookup.
        /// </summary>
        private static void PartitionRightRowsForCompatibleJoin(
            RDFTableRowCollection rightRows,
            int[] rightCommonOrdinals,
            out Dictionary<string, List<int>> boundRightRowsByKey,
            out List<int> wildcardRightRows)
        {
            boundRightRowsByKey = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            wildcardRightRows = new List<int>();

            for (int rightRowIndex = 0; rightRowIndex < rightRows.Count; rightRowIndex++)
            {
                //A null key means at least one common cell is UNBOUND => the row joins as a wildcard
                string joinKey = BuildJoinKey(rightRows[rightRowIndex], rightCommonOrdinals);
                if (joinKey == null)
                {
                    wildcardRightRows.Add(rightRowIndex);
                }
                else
                {
                    if (!boundRightRowsByKey.TryGetValue(joinKey, out List<int> sameKeyRows))
                        boundRightRowsByKey[joinKey] = sameKeyRows = new List<int>();
                    sameKeyRows.Add(rightRowIndex);
                }
            }
        }

        /// <summary>
        /// Returns, in ascending original order, the indexes of the right rows join-compatible with the given
        /// left row, using the partitioned index. A fully-bound left row resolves its exact matches via the
        /// hash and only scans the (typically empty) wildcard rows; a left row with an UNBOUND common cell is
        /// itself a wildcard, so it falls back to scanning every right row. The ascending order reproduces the
        /// left-major/right-minor emission order of the former nested-loop join.
        /// </summary>
        private static List<int> FindCompatibleRightRows(
            RDFTableRow leftRow,
            int[] leftCommonOrdinals,
            RDFTableRowCollection rightRows,
            int[] rightCommonOrdinals,
            Dictionary<string, List<int>> boundRightRowsByKey,
            List<int> wildcardRightRows)
        {
            string leftKey = BuildJoinKey(leftRow, leftCommonOrdinals);

            //Left row is itself a wildcard (some common cell UNBOUND): it can match broadly => scan all right
            if (leftKey == null)
            {
                List<int> scannedMatches = new List<int>();
                for (int rightRowIndex = 0; rightRowIndex < rightRows.Count; rightRowIndex++)
                {
                    if (AreJoinCompatible(leftRow, leftCommonOrdinals, rightRows[rightRowIndex], rightCommonOrdinals))
                        scannedMatches.Add(rightRowIndex);
                }
                return scannedMatches;
            }

            //Left row is fully bound: the exact-key right rows are guaranteed compatible (all common cells equal)
            boundRightRowsByKey.TryGetValue(leftKey, out List<int> exactMatches);

            //Fast path: no wildcard right rows => the exact bucket is already the (ascending) answer
            if (wildcardRightRows.Count == 0)
                return exactMatches ?? new List<int>();

            //Otherwise merge the exact matches with the compatible wildcard rows, restoring ascending order
            List<int> matches = exactMatches != null ? new List<int>(exactMatches) : new List<int>();
            foreach (int wildcardRowIndex in wildcardRightRows)
            {
                if (AreJoinCompatible(leftRow, leftCommonOrdinals, rightRows[wildcardRowIndex], rightCommonOrdinals))
                    matches.Add(wildcardRowIndex);
            }
            matches.Sort();
            return matches;
        }

        /// <summary>
        /// Tells whether the given left row has at least one join-compatible right row, using the partitioned
        /// index. Used by MINUS, which keeps only the left rows that have NO compatible right row.
        /// </summary>
        private static bool HasCompatibleRightRow(
            RDFTableRow leftRow,
            int[] leftCommonOrdinals,
            RDFTableRowCollection rightRows,
            int[] rightCommonOrdinals,
            Dictionary<string, List<int>> boundRightRowsByKey,
            List<int> wildcardRightRows)
        {
            string leftKey = BuildJoinKey(leftRow, leftCommonOrdinals);

            //Left wildcard row => scan all right rows, stopping at the first compatible one
            if (leftKey == null)
            {
                for (int rightRowIndex = 0; rightRowIndex < rightRows.Count; rightRowIndex++)
                {
                    if (AreJoinCompatible(leftRow, leftCommonOrdinals, rightRows[rightRowIndex], rightCommonOrdinals))
                        return true;
                }
                return false;
            }

            //Fully-bound left: any exact-key right row is an immediate match...
            if (boundRightRowsByKey.TryGetValue(leftKey, out List<int> exactMatches) && exactMatches.Count > 0)
                return true;

            //...otherwise a wildcard right row may still be compatible
            foreach (int wildcardRowIndex in wildcardRightRows)
            {
                if (AreJoinCompatible(leftRow, leftCommonOrdinals, rightRows[wildcardRowIndex], rightCommonOrdinals))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Joins two tables WITH support for OPTIONAL/UNION (compatible-mappings outer-join with coalescing)
        /// </summary>
        internal static RDFTable OuterJoinTables(RDFTable leftTable, RDFTable rightTable)
        {
            RDFTable joinTable = new RDFTable();
            bool rightIsOptional = rightTable.IsOptional;
            List<string> commonNames = CommonColumnNames(leftTable, rightTable);

            //Result columns = left columns + right non-common columns
            foreach (RDFTableColumn leftColumn in leftTable.Columns)
                joinTable.AddColumn(leftColumn.Name);
            List<int> rightNonCommonOrdinals = new List<int>();
            foreach (RDFTableColumn rightColumn in rightTable.Columns)
                if (!leftTable.HasColumn(rightColumn.Name))
                {
                    joinTable.AddColumn(rightColumn.Name);
                    rightNonCommonOrdinals.Add(rightColumn.Ordinal);
                }

            int leftColumnsWidth = leftTable.ColumnsCount;
            int joinColumnsWidth = joinTable.ColumnsCount;
            int[] leftCommonOrdinals = commonNames.Select(leftTable.OrdinalOf).ToArray();
            int[] rightCommonOrdinals = commonNames.Select(rightTable.OrdinalOf).ToArray();

            //Hash the right rows once (bound rows by key + wildcard rows apart) to avoid the O(n*m) nested scan
            RDFTableRowCollection rightRows = rightTable.Rows;
            PartitionRightRowsForCompatibleJoin(
                rightRows,
                rightCommonOrdinals,
                out Dictionary<string, List<int>> boundRightRowsByKey,
                out List<int> wildcardRightRows);

            foreach (RDFTableRow leftRow in leftTable.Rows)
            {
                List<int> compatibleRightRows = FindCompatibleRightRows(
                    leftRow, leftCommonOrdinals, rightRows, rightCommonOrdinals, boundRightRowsByKey, wildcardRightRows);

                foreach (int rightRowIndex in compatibleRightRows)
                {
                    RDFTableRow rightRow = rightRows[rightRowIndex];
                    string[] cells = new string[joinColumnsWidth];
                    //Left part (includes common columns, taken from left)
                    for (int i = 0; i < leftColumnsWidth; i++)
                        cells[i] = leftRow[i];
                    //Coalesce common columns: when left is UNBOUND but right is bound, take right
                    for (int c = 0; c < leftCommonOrdinals.Length; c++)
                    {
                        int leftOrdinal = leftCommonOrdinals[c];
                        if (cells[leftOrdinal] == null)
                            cells[leftOrdinal] = rightRow[rightCommonOrdinals[c]];
                    }
                    //Right non-common columns
                    for (int k = 0; k < rightNonCommonOrdinals.Count; k++)
                        cells[leftColumnsWidth + k] = rightRow[rightNonCommonOrdinals[k]];
                    joinTable.AddRow(cells);
                }

                //No related rows but right table is OPTIONAL => keep left row, right non-common stay UNBOUND
                if (compatibleRightRows.Count == 0 && rightIsOptional)
                {
                    string[] cells = new string[joinColumnsWidth];
                    for (int i = 0; i < leftColumnsWidth; i++)
                        cells[i] = leftRow[i];
                    joinTable.AddRow(cells);
                }
            }
            return joinTable;
        }

        /// <summary>
        /// Computes the difference between left table and right table (MINUS): keeps each left row that has
        /// no join-compatible right row; when there are no common columns, every left row is kept.
        /// </summary>
        internal static RDFTable DiffJoinTables(RDFTable leftTable, RDFTable rightTable)
        {
            RDFTable diffTable = new RDFTable();
            foreach (RDFTableColumn leftColumn in leftTable.Columns)
                diffTable.AddColumn(leftColumn.Name);

            List<string> commonNames = CommonColumnNames(leftTable, rightTable);
            int width = leftTable.ColumnsCount;

            //No common columns => keep all left rows
            if (commonNames.Count == 0)
            {
                foreach (RDFTableRow leftRow in leftTable.Rows)
                {
                    string[] cells = new string[width];
                    for (int i = 0; i < width; i++)
                        cells[i] = leftRow[i];
                    diffTable.AddRow(cells);
                }
                return diffTable;
            }

            int[] leftCommonOrdinals = commonNames.Select(leftTable.OrdinalOf).ToArray();
            int[] rightCommonOrdinals = commonNames.Select(rightTable.OrdinalOf).ToArray();

            //Hash the right rows once to answer "has a compatible right row?" without the O(n*m) nested scan
            RDFTableRowCollection rightRows = rightTable.Rows;
            PartitionRightRowsForCompatibleJoin(
                rightRows,
                rightCommonOrdinals,
                out Dictionary<string, List<int>> boundRightRowsByKey,
                out List<int> wildcardRightRows);

            foreach (RDFTableRow leftRow in leftTable.Rows)
                //Keep the left row only when no compatible right row exists (set-difference semantics)
                if (!HasCompatibleRightRow(leftRow, leftCommonOrdinals, rightRows, rightCommonOrdinals, boundRightRowsByKey, wildcardRightRows))
                {
                    string[] cells = new string[width];
                    for (int i = 0; i < width; i++)
                        cells[i] = leftRow[i];
                    diffTable.AddRow(cells);
                }
            return diffTable;
        }

        /// <summary>
        /// Merges the source table into the target table (UNION, MissingSchemaAction.Add): columns of source
        /// missing in target are added (existing target rows get UNBOUND there), then source rows are appended.
        /// </summary>
        private static void MergeTable(RDFTable targetTable, RDFTable sourceTable)
        {
            foreach (RDFTableColumn sourceColumn in sourceTable.Columns)
                targetTable.AddColumn(sourceColumn.Name);

            int targetWidth = targetTable.ColumnsCount;
            int[] sourceToTarget = new int[sourceTable.ColumnsCount];
            for (int i = 0; i < sourceTable.ColumnsCount; i++)
                sourceToTarget[i] = targetTable.OrdinalOf(sourceTable.Columns[i].Name);

            foreach (RDFTableRow sourceRow in sourceTable.Rows)
            {
                string[] cells = new string[targetWidth];
                for (int i = 0; i < sourceToTarget.Length; i++)
                    cells[sourceToTarget[i]] = sourceRow[i];
                targetTable.AddRow(cells);
            }
        }

        /// <summary>
        /// Combines the given tables, applying dynamically detected Union / Minus / Optional operators
        /// </summary>
        internal static RDFTable CombineTables(List<RDFTable> dataTables)
        {
            switch (dataTables.Count)
            {
                case 0: return new RDFTable();
                case 1: return dataTables[0];
            }

            //Step 1: process Union operators (merge previous into current, then logically delete previous)
            bool hasDoneUnions = false;
            for (int i = 1; i < dataTables.Count; i++)
                if (dataTables[i - 1].JoinAsUnion)
                {
                    MergeTable(dataTables[i], dataTables[i - 1]);
                    dataTables[i - 1] = null;
                    hasDoneUnions = true;
                }
            if (hasDoneUnions)
                dataTables.RemoveAll(dt => dt == null);

            //Step 2: process Minus operators (diff previous against current, preserving current's flags)
            bool hasDoneMinus = false;
            for (int i = 1; i < dataTables.Count; i++)
                if (dataTables[i - 1].JoinAsMinus)
                {
                    RDFTable diffTable = DiffJoinTables(dataTables[i - 1], dataTables[i]);
                    diffTable.IsOptional = dataTables[i].IsOptional;
                    diffTable.JoinAsUnion = dataTables[i].JoinAsUnion;
                    diffTable.JoinAsMinus = dataTables[i].JoinAsMinus;
                    dataTables[i] = diffTable;
                    dataTables[i - 1] = null;
                    hasDoneMinus = true;
                }
            if (hasDoneMinus)
                dataTables.RemoveAll(dt => dt == null);

            //Step 3: compute joins (switch to outer-join on Optional, or always when coming from Union)
            RDFTable finalTable = dataTables[0];
            bool needsOuterJoin = hasDoneUnions;
            for (int i = 1; i < dataTables.Count; i++)
            {
                needsOuterJoin |= dataTables[i].IsOptional;
                finalTable = needsOuterJoin ? OuterJoinTables(finalTable, dataTables[i])
                                            : InnerJoinTables(finalTable, dataTables[i]);
            }
            return finalTable;
        }

        /// <summary>
        /// Compares two cells for sorting: an UNBOUND (null) cell is the smallest value; bound values are
        /// compared by Unicode code point (Ordinal), as agreed for the migration (SPARQL-style ordering).
        /// </summary>
        private static int CompareCells(string leftCell, string rightCell)
        {
            if (leftCell == null)
                return rightCell == null ? 0 : -1;
            if (rightCell == null)
                return 1;
            return string.CompareOrdinal(leftCell, rightCell);
        }

        /// <summary>
        /// Returns a new table with the rows ordered by the given keys (column name + descending flag).
        /// The sort is STABLE (rows equal on all keys keep their original order), UNBOUND sorts first when
        /// ascending and last when descending, and keys whose column is absent are ignored.
        /// </summary>
        internal static RDFTable SortTable(RDFTable table, IList<(string column, bool descending)> sortKeys)
        {
            //Resolve sort-key ordinals once, dropping keys whose column is not in the table
            List<(int ordinal, bool descending)> keys = new List<(int, bool)>();
            foreach ((string column, bool descending) sortKey in sortKeys)
            {
                int ordinal = table.OrdinalOf(sortKey.column);
                if (ordinal >= 0)
                    keys.Add((ordinal, sortKey.descending));
            }

            RDFTable sortedTable = new RDFTable();
            foreach (RDFTableColumn column in table.Columns)
                sortedTable.AddColumn(column.Name);

            //Snapshot rows as cell arrays so they can be reordered
            int width = table.ColumnsCount;
            int rowCount = table.RowsCount;
            RDFTableRowCollection sourceRows = table.Rows;
            string[][] rows = new string[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                string[] cells = new string[width];
                RDFTableRow sourceRow = sourceRows[i];
                for (int c = 0; c < width; c++)
                    cells[c] = sourceRow[c];
                rows[i] = cells;
            }

            if (keys.Count > 0)
            {
                //Sort an index array; the original index is the final tie-break, making the sort stable
                int[] order = Enumerable.Range(0, rowCount).ToArray();
                Array.Sort(order, (x, y) =>
                {
                    foreach ((int ordinal, bool descending) key in keys)
                    {
                        int comparison = CompareCells(rows[x][key.ordinal], rows[y][key.ordinal]);
                        if (comparison != 0)
                            return key.descending ? -comparison : comparison;
                    }
                    return x.CompareTo(y);
                });

                string[][] reordered = new string[rowCount][];
                for (int i = 0; i < rowCount; i++)
                    reordered[i] = rows[order[i]];
                rows = reordered;
            }

            foreach (string[] cells in rows)
                sortedTable.AddRow(cells);
            return sortedTable;
        }

        /// <summary>
        /// Returns a new table with duplicate rows removed (preserving first-occurrence order). Two rows are
        /// equal when every cell matches with Ordinal comparison and UNBOUND equals UNBOUND.
        /// </summary>
        internal static RDFTable DistinctTable(RDFTable table)
        {
            RDFTable distinctTable = new RDFTable();
            foreach (RDFTableColumn column in table.Columns)
                distinctTable.AddColumn(column.Name);

            int width = table.ColumnsCount;
            HashSet<string> seenKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (RDFTableRow row in table.Rows)
            {
                string[] cells = new string[width];
                StringBuilder keyBuilder = new StringBuilder();
                for (int c = 0; c < width; c++)
                {
                    string cell = row[c];
                    cells[c] = cell;
                    //UNBOUND gets a marker distinct from any bound value; bound values are length-prefixed
                    if (cell == null)
                        keyBuilder.Append(" ;");
                    else
                        keyBuilder.Append(cell.Length).Append(':').Append(cell).Append(';');
                }

                if (seenKeys.Add(keyBuilder.ToString()))
                    distinctTable.AddRow(cells);
            }
            return distinctTable;
        }

        /// <summary>
        /// Applies the projection operator on the given table, based on the given query's projection variables
        /// </summary>
        internal static RDFTable ProjectTable(RDFSelectQuery query, RDFTable table)
        {
            //Projection expression variables
            ProjectExpressions(query, table);

            //Execute configured sort modifiers (stable Ordinal sort via RDFTable, UNBOUND sorts smallest)
            RDFOrderByModifier[] orderByModifiers = query.GetModifiers().OfType<RDFOrderByModifier>().ToArray();
            if (orderByModifiers.Length > 0)
            {
                List<(string, bool)> sortKeys = orderByModifiers
                    .Select(m => (m.Variable.ToString(), m.OrderByFlavor == RDFQueryEnums.RDFOrderByFlavors.DESC))
                    .ToList();
                table = SortTable(table, sortKeys);
            }

            //Execute projection algorithm
            if (query.ProjectionVars.Count > 0)
            {
                //BuildTransitiveClosureIndex the projected table with the projection variables, ordered by their target ordinal:
                //values are taken from the matching source column when present, otherwise the column stays UNBOUND
                List<KeyValuePair<RDFVariable, (int, RDFExpression)>> orderedProjections = query.ProjectionVars
                    .OrderBy(pv => pv.Value.Item1)
                    .ToList();

                RDFTable projectedTable = new RDFTable();
                int projCount = orderedProjections.Count;
                int[] sourceOrdinals = new int[projCount];
                for (int i = 0; i < projCount; i++)
                {
                    string projVarString = orderedProjections[i].Key.ToString();
                    projectedTable.AddColumn(projVarString);
                    sourceOrdinals[i] = table.OrdinalOf(projVarString);
                }

                foreach (RDFTableRow sourceRow in table.Rows)
                {
                    string[] cells = new string[projCount];
                    for (int i = 0; i < projCount; i++)
                        cells[i] = sourceOrdinals[i] >= 0 ? sourceRow[sourceOrdinals[i]] : null;
                    projectedTable.AddRow(cells);
                }
                table = projectedTable;
            }
            return table;
        }

        /// <summary>
        /// Fills the given table with new columns from the given query's projection expressions
        /// </summary>
        internal static void ProjectExpressions(RDFSelectQuery query, RDFTable table)
        {
            foreach (KeyValuePair<RDFVariable, (int, RDFExpression)> projectionExpression in query.ProjectionVars.Where(pv => pv.Value.Item2 != null)
                                                                                                                 .OrderBy(pv => pv.Value.Item1))
                EvaluateExpression(projectionExpression.Value.Item2, projectionExpression.Key, table);
        }

        /// <summary>
        /// Fills the given table with new column from the given bind's variable
        /// </summary>
        internal static void ProjectBind(RDFBind bind, RDFTable table)
            => EvaluateExpression(bind.Expression, bind.Variable, table);

        /// <summary>
        /// Evaluates the given expression on the given table and projects the given variable
        /// </summary>
        internal static void EvaluateExpression(RDFExpression expression, RDFVariable variable, RDFTable table)
        {
            string bindVariable = variable.ToString();
            if (!table.HasColumn(bindVariable))
            {
                //Project bind column
                table.AddColumn(bindVariable);
                int bindOrdinal = table.OrdinalOf(bindVariable);

                //Valorize bind column
                if (table.RowsCount == 0)
                {
                    //Ensure to add the row only in case the expression has evaluated without binding errors,
                    //(otherwise in this scenario we would always answer true for ASK queries due to this row)
                    RDFPatternMember bindResult = expression.ApplyExpression(table.NewRow());
                    if (bindResult != null)
                        table.AddRow(new Dictionary<string, string> { { bindVariable, bindResult.ToString() } });
                }
                else
                {
                    int rowCount = table.RowsCount;
                    RDFTableRowCollection rows = table.Rows;
                    for (int i = 0; i < rowCount; i++)
                    {
                        string[] cells = table.GetRowArray(i);
                        cells[bindOrdinal] = expression.ApplyExpression(rows[i])?.ToString();
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}