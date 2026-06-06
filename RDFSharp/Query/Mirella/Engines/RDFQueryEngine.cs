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
                        RDFTableEngine.MergeTable(resultTable, DescribeTerms(describeQuery, fedDataSource, qResultTable));
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
                queryResultTable = RDFTableEngine.CombineTables(QueryMemberResultTables.Values.ToList());
            }

            return queryResultTable;
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
                            RDFTable currentPatternGroupResultTable = RDFTableEngine.CombineTables(PatternGroupMemberResultTables[patternGroup.QueryMemberID]);
                            //Evaluate bind operator on the current patternGroup result table
                            RDFTableEngine.ProjectBind(bind, currentPatternGroupResultTable);
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
                RDFTable patternGroupResultTable = RDFTableEngine.CombineTables(PatternGroupMemberResultTables[patternGroup.QueryMemberID]);

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
                table = RDFTableEngine.ProjectTable(selectQuery, table);
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
                RDFTableEngine.PopulateTable(pattern, matchingTriples, patternResultTable);

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
                RDFTableEngine.PopulateTable(pattern, matchingQuadruples, patternResultTable);

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
                        RDFTableEngine.MergeTable(resultTable, graphTable);
                        break;

                    case RDFStore dataSourceStore:
                        RDFTable storeTable = ApplyPatternToStore(pattern, dataSourceStore);
                        RDFTableEngine.MergeTable(resultTable, storeTable);
                        break;

                    case RDFFederation dataSourceFederation:
                        RDFTable federationTable = ApplyPatternToFederation(pattern, dataSourceFederation);
                        RDFTableEngine.MergeTable(resultTable, federationTable);
                        break;

                    case RDFSPARQLEndpoint dataSourceSparqlEndpoint:
                        //Pattern is transformed into an equivalent "SELECT *" query which is sent to the SPARQL endpoint.
                        //SPARQL endpoint options are eventually retrieved directly from the federation.
                        federation.EndpointDataSourcesQueryOptions.TryGetValue(dataSourceSparqlEndpoint.ToString(), out RDFSPARQLEndpointQueryOptions dataSourceSparqlEndpointOptions);
                        RDFSelectQuery sparqlEndpointQuery =  new RDFSelectQuery()
                                                                .AddPatternGroup(new RDFPatternGroup().AddPattern(pattern));
                        RDFSelectQueryResult sparqlEndpointTable = sparqlEndpointQuery.ApplyToSPARQLEndpoint(dataSourceSparqlEndpoint, dataSourceSparqlEndpointOptions);
                        RDFTableEngine.MergeTable(resultTable, RDFTable.FromDataTable(sparqlEndpointTable.SelectResults));
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
                return RDFPathEngine.ApplyTransitivePropertyPath(propertyPath, dataSource);

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
            RDFTable resultTable = RDFTableEngine.CombineTables(patternTables);

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

        // CONSTRUCT / DESCRIBE

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
                        RDFTableEngine.MergeTable(result, DescribeResourceTerm(describeResource, dataSource, result));
                        break;

                    case RDFVariable describeVariable:
                        RDFTableEngine.MergeTable(result, DescribeVariableTerm(describeVariable, dataSource, result, resultTable));
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
                    RDFTableEngine.MergeTable(result, RDFTable.FromDataTable(graph.ToDataTable()));
                    break;

                //STORE
                case RDFStore dataSourceStore:
                    RDFMemoryStore store = QueryStore(dataSourceStore);
                    RDFTableEngine.MergeTable(result, RDFTable.FromDataTable(store.ToDataTable()));
                    break;

                //FEDERATION / SPARQL ENDPOINT
                default:
                    RDFSelectQuery query = BuildFederationOrSPARQLEndpointQuery();
                    RDFSelectQueryResult queryResults = dataSource.IsSPARQLEndpoint() ? query.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                                                      : query.ApplyToFederation((RDFFederation)dataSource);
                    RDFTableEngine.MergeTable(result, RDFTable.FromDataTable(queryResults.SelectResults));
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
                    RDFTableEngine.MergeTable(result, RDFTable.FromDataTable(graph.ToDataTable()));
                    break;

                //STORE
                case RDFStore dataSourceStore:
                    RDFMemoryStore store = QueryStore(dataSourceStore);
                    RDFTableEngine.MergeTable(result, RDFTable.FromDataTable(store.ToDataTable()));
                    break;

                //FEDERATION / SPARQL ENDPOINT
                default:
                    RDFSelectQuery query = BuildFederationOrSPARQLEndpointQuery();
                    RDFSelectQueryResult queryResults =
                        dataSource.IsSPARQLEndpoint() ? query.ApplyToSPARQLEndpoint((RDFSPARQLEndpoint)dataSource)
                                                      : query.ApplyToFederation((RDFFederation)dataSource);
                    RDFTableEngine.MergeTable(result, RDFTable.FromDataTable(queryResults.SelectResults));
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
                        RDFTableEngine.MergeTable(result, describeResourceTable);
                        break;

                    //LITERAL
                    case RDFLiteral describeLiteral:
                        RDFTable describeLiteralTable = DescribeLiteralTerm(describeLiteral, dataSource, describeTemplate);
                        RDFTableEngine.MergeTable(result, describeLiteralTable);
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
        #endregion
    }
}