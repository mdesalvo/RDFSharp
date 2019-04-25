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
    /// RDFQueryEngine is the engine for execution of SPARQL queries (MIRELLA)
    /// </summary>
    internal class RDFQueryEngine
    {

        #region Properties
        /// <summary>
        /// Dictionary of temporary result tables produced by evaluation of query members
        /// </summary>
        internal Dictionary<Int64, List<DataTable>> QueryMemberTemporaryResultTables { get; set; }

        /// <summary>
        /// Dictionary of final result tables produced by evaluation of query members
        /// </summary>
        internal Dictionary<Int64, DataTable> QueryMemberFinalResultTables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize a query engine instance
        /// </summary>
        private RDFQueryEngine()
        {
            this.QueryMemberTemporaryResultTables = new Dictionary<Int64, List<DataTable>>();
            this.QueryMemberFinalResultTables = new Dictionary<Int64, DataTable>();
        }
        internal static RDFQueryEngine CreateNew()
        {
            return new RDFQueryEngine();
        }
        #endregion

        #region Methods

        #region MIRELLA SPARQL
        /// <summary>
        /// Evaluates the given SPARQL SELECT query on the given RDF datasource
        /// </summary>
        internal RDFSelectQueryResult EvaluateSelectQuery(RDFSelectQuery selectQuery, RDFDataSource datasource)
        {
            RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Evaluating SPARQL SELECT query on DataSource '{0}'...", datasource));

            RDFSelectQueryResult queryResult = new RDFSelectQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = selectQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {

                //Iterate the evaluable members of the query
                var fedQueryMemberTemporaryResultTables = new Dictionary<Int64, List<DataTable>>();
                foreach (var evaluableQueryMember in evaluableQueryMembers)
                {

                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Evaluating PatternGroup '{0}' on DataSource '{1}'...", (RDFPatternGroup)evaluableQueryMember, datasource));

                        //Step 1: Get the intermediate result tables of the current pattern group
                        if (datasource.IsFederation())
                        {

                            #region TrueFederations
                            foreach (var store in (RDFFederation)datasource)
                            {

                                //Step FED.1: Evaluate the current pattern group on the current store
                                EvaluatePatternGroup(selectQuery, (RDFPatternGroup)evaluableQueryMember, store);

                                //Step FED.2: Federate the results of the current pattern group on the current store
                                if (!fedQueryMemberTemporaryResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                                {
                                    fedQueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID]);
                                }
                                else
                                {
                                    fedQueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID].ForEach(fqmtrt =>
                                      fqmtrt.Merge(QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID].Single(qmtrt => qmtrt.TableName.Equals(fqmtrt.TableName, StringComparison.Ordinal)), true, MissingSchemaAction.Add));
                                }

                            }
                            QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID] = fedQueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID];
                            #endregion

                        }
                        else
                        {
                            EvaluatePatternGroup(selectQuery, (RDFPatternGroup)evaluableQueryMember, datasource);
                        }

                        //Step 2: Get the result table of the current pattern group
                        FinalizePatternGroup(selectQuery, (RDFPatternGroup)evaluableQueryMember);

                        //Step 3: Apply the filters of the current pattern group to its result table
                        ApplyFilters(selectQuery, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = (datasource is RDFGraph ? ((RDFSelectQuery)evaluableQueryMember).ApplyToGraph((RDFGraph)datasource)
                                                                : datasource is RDFStore ? ((RDFSelectQuery)evaluableQueryMember).ApplyToStore((RDFStore)datasource)
                                                                  : ((RDFSelectQuery)evaluableQueryMember).ApplyToFederation((RDFFederation)datasource));
                        if (!QueryMemberFinalResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                        {
                            //Populate its name
                            QueryMemberFinalResultTables.Add(evaluableQueryMember.QueryMemberID, subQueryResult.SelectResults);
                            //Populate its metadata (IsOptional)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional"))
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("IsOptional", ((RDFSelectQuery)evaluableQueryMember).IsOptional);
                            }
                            else
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional;
                            }
                            //Populate its metadata (JoinAsUnion)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion"))
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion);
                            }
                            else
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["JoinAsUnion"] = ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion;
                            }
                        }
                    }
                    #endregion

                }

                //Step 4: Get the result table of the query
                var queryResultTable = CombineTables(QueryMemberFinalResultTables.Values.ToList(), false);

                //Step 5: Apply the modifiers of the query to the result table
                queryResult.SelectResults = ApplyModifiers(selectQuery, queryResultTable);

            }
            RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Evaluated SPARQL SELECT query on DataSource '{0}': Found '{1}' results.", datasource, queryResult.SelectResultsCount));

            queryResult.SelectResults.TableName = selectQuery.ToString();
            return queryResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL DESCRIBE query on the given RDF datasource
        /// </summary>
        internal RDFDescribeQueryResult EvaluateDescribeQuery(RDFDescribeQuery describeQuery, RDFDataSource datasource)
        {
            RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluating SPARQL DESCRIBE query on DataSource '{0}'...", datasource));

            RDFDescribeQueryResult queryResult = new RDFDescribeQueryResult(this.ToString());
            List<RDFQueryMember> evaluableQueryMembers = describeQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {

                //Iterate the evaluable members of the query
                var fedQueryMemberTemporaryResultTables = new Dictionary<Int64, List<DataTable>>();
                foreach (var evaluableQueryMember in evaluableQueryMembers)
                {

                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluating PatternGroup '{0}' on DataSource '{1}'...", (RDFPatternGroup)evaluableQueryMember, datasource));

                        //Step 1: Get the intermediate result tables of the current pattern group
                        if (datasource.IsFederation())
                        {

                            #region TrueFederations
                            foreach (var store in (RDFFederation)datasource)
                            {

                                //Step FED.1: Evaluate the current pattern group on the current store
                                EvaluatePatternGroup(describeQuery, (RDFPatternGroup)evaluableQueryMember, store);

                                //Step FED.2: Federate the results of the current pattern group on the current store
                                if (!fedQueryMemberTemporaryResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                                {
                                    fedQueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID]);
                                }
                                else
                                {
                                    fedQueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID].ForEach(fqmtrt =>
                                       fqmtrt.Merge(QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID].Single(qmtrt => qmtrt.TableName.Equals(fqmtrt.TableName, StringComparison.Ordinal)), true, MissingSchemaAction.Add));
                                }

                            }
                            QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID] = fedQueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID];
                            #endregion

                        }
                        else
                        {
                            EvaluatePatternGroup(describeQuery, (RDFPatternGroup)evaluableQueryMember, datasource);
                        }

                        //Step 2: Get the result table of the current pattern group
                        FinalizePatternGroup(describeQuery, (RDFPatternGroup)evaluableQueryMember);

                        //Step 3: Apply the filters of the current pattern group to its result table
                        ApplyFilters(describeQuery, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = (datasource is RDFGraph ? ((RDFSelectQuery)evaluableQueryMember).ApplyToGraph((RDFGraph)datasource)
                                                                : datasource is RDFStore ? ((RDFSelectQuery)evaluableQueryMember).ApplyToStore((RDFStore)datasource)
                                                                  : ((RDFSelectQuery)evaluableQueryMember).ApplyToFederation((RDFFederation)datasource));
                        if (!QueryMemberFinalResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                        {
                            //Populate its name
                            QueryMemberFinalResultTables.Add(evaluableQueryMember.QueryMemberID, subQueryResult.SelectResults);
                            //Populate its metadata (IsOptional)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional"))
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("IsOptional", ((RDFSelectQuery)evaluableQueryMember).IsOptional);
                            }
                            else
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional;
                            }
                            //Populate its metadata (JoinAsUnion)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion"))
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion);
                            }
                            else
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["JoinAsUnion"] = ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion;
                            }
                        }
                    }
                    #endregion

                }

                //Step 4: Get the result table of the query
                DataTable queryResultTable = CombineTables(this.QueryMemberFinalResultTables.Values.ToList(), false);

                //Step 5: Describe the terms from the result table
                DataTable describeResultTable = new DataTable(this.ToString());
                if (datasource.IsFederation())
                {

                    #region TrueFederations
                    foreach (var store in (RDFFederation)datasource)
                    {
                        describeResultTable.Merge(DescribeTerms(describeQuery, store, queryResultTable), true, MissingSchemaAction.Add);
                    }
                    #endregion

                }
                else
                {
                    describeResultTable = DescribeTerms(describeQuery, datasource, queryResultTable);
                }

                //Step 6: Apply the modifiers of the query to the result table
                queryResult.DescribeResults = ApplyModifiers(describeQuery, describeResultTable);

            }
            else
            {

                //In this case the only chance to proceed is to have resources in the describe terms,
                //which will be used to search for S-P-O data. Variables are ignored in this scenario.
                if (describeQuery.DescribeTerms.Any(dt => dt is RDFResource))
                {

                    //Step 1: Describe the terms from the result table
                    DataTable describeResultTable = new DataTable(this.ToString());
                    if (datasource.IsFederation())
                    {

                        #region TrueFederations
                        foreach (var store in (RDFFederation)datasource)
                        {
                            describeResultTable.Merge(DescribeTerms(describeQuery, store, new DataTable()), true, MissingSchemaAction.Add);
                        }
                        #endregion

                    }
                    else
                    {
                        describeResultTable = DescribeTerms(describeQuery, datasource, new DataTable());
                    }

                    //Step 2: Apply the modifiers of the query to the result table
                    queryResult.DescribeResults = ApplyModifiers(describeQuery, describeResultTable);

                }

            }
            RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluated SPARQL DESCRIBE query on DataSource '{0}': Found '{1}' results.", datasource, queryResult.DescribeResultsCount));

            queryResult.DescribeResults.TableName = describeQuery.ToString();
            return queryResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL CONSTRUCT query on the given RDF datasource
        /// </summary>
        internal RDFConstructQueryResult EvaluateConstructQuery(RDFConstructQuery constructQuery, RDFDataSource datasource)
        {
            RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluating CONSTRUCT query on DataSource '{0}'...", datasource));

            RDFConstructQueryResult constructResult = new RDFConstructQueryResult(this.ToString());
            List<RDFQueryMember> evaluableQueryMembers = constructQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {

                //Iterate the evaluable members of the query
                var fedQueryMemberTemporaryResultTables = new Dictionary<Int64, List<DataTable>>();
                foreach (var evaluableQueryMember in evaluableQueryMembers)
                {

                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluating PatternGroup '{0}' on DataSource '{1}'...", (RDFPatternGroup)evaluableQueryMember, datasource));

                        //Step 1: Get the intermediate result tables of the current pattern group
                        if (datasource.IsFederation())
                        {

                            #region TrueFederations
                            foreach (var store in (RDFFederation)datasource)
                            {

                                //Step FED.1: Evaluate the current pattern group on the current store
                                EvaluatePatternGroup(constructQuery, (RDFPatternGroup)evaluableQueryMember, store);

                                //Step FED.2: Federate the results of the current pattern group on the current store
                                if (!fedQueryMemberTemporaryResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                                {
                                    fedQueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID]);
                                }
                                else
                                {
                                    fedQueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID].ForEach(fqmtrt =>
                                      fqmtrt.Merge(QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID].Single(qmtrt => qmtrt.TableName.Equals(fqmtrt.TableName, StringComparison.Ordinal)), true, MissingSchemaAction.Add));
                                }

                            }
                            QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID] = fedQueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID];
                            #endregion

                        }
                        else
                        {
                            EvaluatePatternGroup(constructQuery, (RDFPatternGroup)evaluableQueryMember, datasource);
                        }

                        //Step 2: Get the result table of the current pattern group
                        FinalizePatternGroup(constructQuery, (RDFPatternGroup)evaluableQueryMember);

                        //Step 3: Apply the filters of the current pattern group to its result table
                        ApplyFilters(constructQuery, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = (datasource is RDFGraph ? ((RDFSelectQuery)evaluableQueryMember).ApplyToGraph((RDFGraph)datasource)
                                                                : datasource is RDFStore ? ((RDFSelectQuery)evaluableQueryMember).ApplyToStore((RDFStore)datasource)
                                                                  : ((RDFSelectQuery)evaluableQueryMember).ApplyToFederation((RDFFederation)datasource));
                        if (!QueryMemberFinalResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                        {
                            //Populate its name
                            QueryMemberFinalResultTables.Add(evaluableQueryMember.QueryMemberID, subQueryResult.SelectResults);
                            //Populate its metadata (IsOptional)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional"))
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("IsOptional", ((RDFSelectQuery)evaluableQueryMember).IsOptional);
                            }
                            else
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional;
                            }
                            //Populate its metadata (JoinAsUnion)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion"))
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion);
                            }
                            else
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["JoinAsUnion"] = ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion;
                            }
                        }
                    }
                    #endregion

                }

                //Step 4: Get the result table of the query
                DataTable queryResultTable = CombineTables(QueryMemberFinalResultTables.Values.ToList(), false);

                //Step 5: Fill the templates from the result table
                DataTable filledResultTable = FillTemplates(constructQuery, queryResultTable);

                //Step 6: Apply the modifiers of the query to the result table
                constructResult.ConstructResults = ApplyModifiers(constructQuery, filledResultTable);

            }
            RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluated SPARQL CONSTRUCT query on DataSource '{0}': Found '{1}' results.", datasource, constructResult.ConstructResultsCount));

            constructResult.ConstructResults.TableName = constructQuery.ToString();
            return constructResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL ASK query on the given RDF datasource
        /// </summary>
        internal RDFAskQueryResult EvaluateAskQuery(RDFAskQuery askQuery, RDFDataSource datasource)
        {
            RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluating SPARQL ASK query on DataSource '{0}'...", datasource));

            RDFAskQueryResult askResult = new RDFAskQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = askQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {

                //Iterate the evaluable members of the query
                var fedQueryMemberTemporaryResultTables = new Dictionary<Int64, List<DataTable>>();
                foreach (var evaluableQueryMember in evaluableQueryMembers)
                {

                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluating PatternGroup '{0}' on DataSource '{1}'...", (RDFPatternGroup)evaluableQueryMember, datasource));

                        //Step 1: Get the intermediate result tables of the current pattern group
                        if (datasource.IsFederation())
                        {

                            #region TrueFederations
                            foreach (var store in (RDFFederation)datasource)
                            {

                                //Step FED.1: Evaluate the current pattern group on the current store
                                EvaluatePatternGroup(askQuery, (RDFPatternGroup)evaluableQueryMember, store);

                                //Step FED.2: Federate the results of the current pattern group on the current store
                                if (!fedQueryMemberTemporaryResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                                {
                                    fedQueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID]);
                                }
                                else
                                {
                                    fedQueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID].ForEach(fqmtrt =>
                                       fqmtrt.Merge(QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID].Single(qmtrt => qmtrt.TableName.Equals(fqmtrt.TableName, StringComparison.Ordinal)), true, MissingSchemaAction.Add));
                                }

                            }
                            QueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID] = fedQueryMemberTemporaryResultTables[evaluableQueryMember.QueryMemberID];
                            #endregion

                        }
                        else
                        {
                            EvaluatePatternGroup(askQuery, (RDFPatternGroup)evaluableQueryMember, datasource);
                        }

                        //Step 2: Get the result table of the current pattern group
                        FinalizePatternGroup(askQuery, (RDFPatternGroup)evaluableQueryMember);

                        //Step 3: Apply the filters of the current pattern group to its result table
                        ApplyFilters(askQuery, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = (datasource is RDFGraph ? ((RDFSelectQuery)evaluableQueryMember).ApplyToGraph((RDFGraph)datasource)
                                                                : datasource is RDFStore ? ((RDFSelectQuery)evaluableQueryMember).ApplyToStore((RDFStore)datasource)
                                                                  : ((RDFSelectQuery)evaluableQueryMember).ApplyToFederation((RDFFederation)datasource));
                        if (!QueryMemberFinalResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                        {
                            //Populate its name
                            QueryMemberFinalResultTables.Add(evaluableQueryMember.QueryMemberID, subQueryResult.SelectResults);
                            //Populate its metadata (IsOptional)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional"))
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("IsOptional", ((RDFSelectQuery)evaluableQueryMember).IsOptional);
                            }
                            else
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional;
                            }
                            //Populate its metadata (JoinAsUnion)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion"))
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion);
                            }
                            else
                            {
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["JoinAsUnion"] = ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion;
                            }
                        }
                    }
                    #endregion

                }

                //Step 4: Get the result table of the query
                var queryResultTable = CombineTables(QueryMemberFinalResultTables.Values.ToList(), false);

                //Step 5: Transform the result into a boolean response 
                askResult.AskResult = (queryResultTable.Rows.Count > 0);

            }
            RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluated SPARQL ASK query on DataSource '{0}': Result is '{1}'.", datasource, askResult.AskResult));

            return askResult;
        }

        /// <summary>
        /// Get the intermediate result tables of the given pattern group
        /// </summary>
        internal void EvaluatePatternGroup(RDFQuery query, RDFPatternGroup patternGroup, RDFDataSource graphOrStore)
        {
            QueryMemberTemporaryResultTables[patternGroup.QueryMemberID] = new List<DataTable>();

            //Iterate the evaluable members of the pattern group
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            foreach (var evaluablePGMember in evaluablePGMembers)
            {

                #region Pattern
                if (evaluablePGMember is RDFPattern)
                {
                    var patternResultsTable = graphOrStore.IsGraph() ? ApplyPattern((RDFPattern)evaluablePGMember, (RDFGraph)graphOrStore) :
                                                                       ApplyPattern((RDFPattern)evaluablePGMember, (RDFStore)graphOrStore);

                    #region Events
                    //Raise query event messages
                    var eventMsg = String.Format("Pattern '{0}' has been evaluated on DataSource '{1}': Found '{2}' results.", (RDFPattern)evaluablePGMember, graphOrStore, patternResultsTable.Rows.Count);
                    if (query is RDFAskQuery)
                        RDFQueryEvents.RaiseASKQueryEvaluation(eventMsg);
                    else if (query is RDFConstructQuery)
                        RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(eventMsg);
                    else if (query is RDFDescribeQuery)
                        RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(eventMsg);
                    else if (query is RDFSelectQuery)
                        RDFQueryEvents.RaiseSELECTQueryEvaluation(eventMsg);
                    #endregion

                    //Set name and metadata of result datatable
                    patternResultsTable.TableName = ((RDFPattern)evaluablePGMember).ToString();
                    patternResultsTable.ExtendedProperties.Add("IsOptional", ((RDFPattern)evaluablePGMember).IsOptional);
                    patternResultsTable.ExtendedProperties.Add("JoinAsUnion", ((RDFPattern)evaluablePGMember).JoinAsUnion);

                    //Save result datatable
                    QueryMemberTemporaryResultTables[patternGroup.QueryMemberID].Add(patternResultsTable);
                }
                #endregion

                #region PropertyPath
                else if (evaluablePGMember is RDFPropertyPath)
                {
                    var pPathResultsTable = ApplyPropertyPath((RDFPropertyPath)evaluablePGMember, graphOrStore);

                    #region Events
                    //Raise query event messages
                    var eventMsg = String.Format(String.Format("PropertyPath '{0}' has been evaluated on DataSource '{1}': Found '{2}' results.", (RDFPropertyPath)evaluablePGMember, graphOrStore, pPathResultsTable.Rows.Count));
                    if (query is RDFAskQuery)
                        RDFQueryEvents.RaiseASKQueryEvaluation(eventMsg);
                    else if (query is RDFConstructQuery)
                        RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(eventMsg);
                    else if (query is RDFDescribeQuery)
                        RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(eventMsg);
                    else if (query is RDFSelectQuery)
                        RDFQueryEvents.RaiseSELECTQueryEvaluation(eventMsg);
                    #endregion

                    //Set name of result datatable
                    pPathResultsTable.TableName = ((RDFPropertyPath)evaluablePGMember).ToString();

                    //Save result datatable
                    QueryMemberTemporaryResultTables[patternGroup.QueryMemberID].Add(pPathResultsTable);
                }
                #endregion

            }
        }

        /// <summary>
        /// Get the final result table of the given pattern group
        /// </summary>
        internal void FinalizePatternGroup(RDFQuery query, RDFPatternGroup patternGroup)
        {
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            if (evaluablePGMembers.Any())
            {

                //Populate query member result table
                var queryMemberFinalResultTable = CombineTables(QueryMemberTemporaryResultTables[patternGroup.QueryMemberID], false);

                //Add it to the list of query member result tables
                QueryMemberFinalResultTables.Add(patternGroup.QueryMemberID, queryMemberFinalResultTable);

                //Populate its name
                QueryMemberFinalResultTables[patternGroup.QueryMemberID].TableName = patternGroup.ToString();
                //Populate its metadata (IsOptional)
                if (!QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional"))
                {
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties.Add("IsOptional", patternGroup.IsOptional);
                }
                else
                {
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties["IsOptional"] = patternGroup.IsOptional;
                }
                //Populate its metadata (JoinAsUnion)
                if (!QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion"))
                {
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", patternGroup.JoinAsUnion);
                }
                else
                {
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties["JoinAsUnion"] = patternGroup.JoinAsUnion;
                }

            }
        }

        /// <summary>
        /// Apply the filters of the given pattern group to its result table
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
                Boolean keepRow = false;
                while (rowsEnum.MoveNext())
                {

                    //Apply the pattern group's filters on the row
                    keepRow = true;
                    var filtersEnum = filters.GetEnumerator();
                    while (keepRow && filtersEnum.MoveNext())
                    {
                        keepRow = filtersEnum.Current.ApplyFilter((DataRow)rowsEnum.Current, false);
                    }

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
        /// Apply the query modifiers to the query result table
        /// </summary>
        internal DataTable ApplyModifiers(RDFQuery query, DataTable table)
        {

            #region GROUPBY/ORDERBY/PROJECTION
            List<RDFModifier> modifiers = query.GetModifiers().ToList();
            if (query is RDFSelectQuery)
            {

                #region GROUPBY
                var grbModifier = modifiers.SingleOrDefault(m => m is RDFGroupByModifier);
                if (grbModifier != null)
                {
                    table = grbModifier.ApplyModifier(table);

                    #region PROJECTION
                    ((RDFSelectQuery)query).ProjectionVars.Clear();
                    ((RDFGroupByModifier)grbModifier).PartitionVariables.ForEach(pv => {
                        ((RDFSelectQuery)query).AddProjectionVariable(pv);
                    });
                    ((RDFGroupByModifier)grbModifier).Aggregators.ForEach(ag => {
                        ((RDFSelectQuery)query).AddProjectionVariable(ag.ProjectionVariable);
                    });
                    #endregion
                }
                #endregion

                #region ORDERBY
                var ordModifiers = modifiers.Where(m => m is RDFOrderByModifier);
                if (ordModifiers.Any())
                {
                    table = ordModifiers.Aggregate(table, (current, modifier) => modifier.ApplyModifier(current));
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
            {
                table = distinctModifier.ApplyModifier(table);
            }
            #endregion

            #region OFFSET
            var offsetModifier = modifiers.SingleOrDefault(m => m is RDFOffsetModifier);
            if (offsetModifier != null)
            {
                table = offsetModifier.ApplyModifier(table);
            }
            #endregion

            #region  LIMIT
            var limitModifier = modifiers.SingleOrDefault(m => m is RDFLimitModifier);
            if (limitModifier != null)
            {
                table = limitModifier.ApplyModifier(table);
            }
            #endregion

            return table;
        }

        /// <summary>
        /// Fills the templates of the given CONSTRUCT query with data from the given result table
        /// </summary>
        internal DataTable FillTemplates(RDFConstructQuery constructQuery, DataTable resultTable)
        {

            //Create the structure of the result datatable
            DataTable result = new DataTable("CONSTRUCT_RESULTS");
            result.Columns.Add("SUBJECT", Type.GetType("System.String"));
            result.Columns.Add("PREDICATE", Type.GetType("System.String"));
            result.Columns.Add("OBJECT", Type.GetType("System.String"));
            result.AcceptChanges();

            //Initialize working variables
            var constructRow = new Dictionary<String, String>();
            constructRow.Add("SUBJECT", null);
            constructRow.Add("PREDICATE", null);
            constructRow.Add("OBJECT", null);

            //Iterate on the templates
            foreach (RDFPattern tp in constructQuery.Templates.Where(tp => tp.Variables.Count == 0 ||
                                                                           tp.Variables.TrueForAll(v => resultTable.Columns.Contains(v.ToString()))))
            {

                #region GROUND TEMPLATE
                //Check if the template is ground, so represents an explicit triple to be added as-is
                if (tp.Variables.Count == 0)
                {
                    constructRow["SUBJECT"] = tp.Subject.ToString();
                    constructRow["PREDICATE"] = tp.Predicate.ToString();
                    constructRow["OBJECT"] = tp.Object.ToString();
                    AddRow(result, constructRow);
                    continue;
                }
                #endregion

                #region NON-GROUND TEMPLATE
                //Iterate the result datatable's rows to construct the triples of the current template
                IEnumerator rowsEnum = resultTable.Rows.GetEnumerator();
                while (rowsEnum.MoveNext())
                {

                    #region SUBJECT
                    //Subject of the template is a variable
                    if (tp.Subject is RDFVariable)
                    {

                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template subject
                        if (((DataRow)rowsEnum.Current).IsNull(tp.Subject.ToString()))
                        {
                            continue;
                        }

                        RDFPatternMember subj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[tp.Subject.ToString()].ToString());
                        //Row contains a literal in position of the variable corresponding to the template subject
                        if (subj is RDFLiteral)
                        {
                            continue;
                        }
                        //Row contains a resource in position of the variable corresponding to the template subject
                        constructRow["SUBJECT"] = subj.ToString();

                    }
                    //Subject of the template is a resource
                    else
                    {
                        constructRow["SUBJECT"] = tp.Subject.ToString();
                    }
                    #endregion

                    #region PREDICATE
                    //Predicate of the template is a variable
                    if (tp.Predicate is RDFVariable)
                    {

                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template predicate
                        if (((DataRow)rowsEnum.Current).IsNull(tp.Predicate.ToString()))
                        {
                            continue;
                        }
                        RDFPatternMember pred = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[tp.Predicate.ToString()].ToString());
                        //Row contains a blank resource or a literal in position of the variable corresponding to the template predicate
                        if ((pred is RDFResource && ((RDFResource)pred).IsBlank) || pred is RDFLiteral)
                        {
                            continue;
                        }
                        //Row contains a non-blank resource in position of the variable corresponding to the template predicate
                        constructRow["PREDICATE"] = pred.ToString();

                    }
                    //Predicate of the template is a resource
                    else
                    {
                        constructRow["PREDICATE"] = tp.Predicate.ToString();
                    }
                    #endregion

                    #region OBJECT
                    //Object of the template is a variable
                    if (tp.Object is RDFVariable)
                    {

                        //Check if the template must be skipped, in order to not produce illegal triples
                        //Row contains an unbound value in position of the variable corresponding to the template object
                        if (((DataRow)rowsEnum.Current).IsNull(tp.Object.ToString()))
                        {
                            continue;
                        }
                        RDFPatternMember obj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)rowsEnum.Current)[tp.Object.ToString()].ToString());
                        //Row contains a resource or a literal in position of the variable corresponding to the template object
                        constructRow["OBJECT"] = obj.ToString();

                    }
                    //Object of the template is a resource or a literal
                    else
                    {
                        constructRow["OBJECT"] = tp.Object.ToString();
                    }
                    #endregion

                    //Insert the triple into the final table
                    AddRow(result, constructRow);

                }
                #endregion

            }

            return result;
        }

        /// <summary>
        /// Describes the terms of the given DESCRIBE query with data from the given result table
        /// </summary>
        internal DataTable DescribeTerms(RDFDescribeQuery describeQuery, RDFDataSource graphOrStore, DataTable resultTable)
        {

            //Create the structure of the result datatable
            DataTable result = new DataTable("DESCRIBE_RESULTS");
            if (graphOrStore.IsStore())
            {
                result.Columns.Add("CONTEXT", Type.GetType("System.String"));
            }
            result.Columns.Add("SUBJECT", Type.GetType("System.String"));
            result.Columns.Add("PREDICATE", Type.GetType("System.String"));
            result.Columns.Add("OBJECT", Type.GetType("System.String"));
            result.AcceptChanges();

            //Query IS empty, so does not have evaluable members to fetch data from. 
            //We can only proceed by searching for resources in the describe terms.
            IEnumerable<RDFQueryMember> evaluableQueryMembers = describeQuery.GetEvaluableQueryMembers();
            if (!evaluableQueryMembers.Any())
            {

                //Iterate the describe terms of the query which are resources (variables are omitted, since useless)
                foreach (RDFPatternMember dt in describeQuery.DescribeTerms.Where(dterm => dterm is RDFResource))
                {

                    //Search on GRAPH
                    if (graphOrStore.IsGraph())
                    {

                        //Search as RESOURCE (S-P-O)
                        RDFGraph desc = ((RDFGraph)graphOrStore).SelectTriplesBySubject((RDFResource)dt)
                                                    .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByPredicate((RDFResource)dt))
                                                        .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByObject((RDFResource)dt));
                        result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);

                    }

                    //Search on STORE
                    else
                    {

                        //Search as BLANK RESOURCE (S-P-O)
                        if (((RDFResource)dt).IsBlank)
                        {
                            RDFMemoryStore desc = ((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt)
                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt))
                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }

                        //Search as NON-BLANK RESOURCE (C-S-P-O)
                        else
                        {
                            RDFMemoryStore desc = ((RDFStore)graphOrStore).SelectQuadruplesByContext(new RDFContext(((RDFResource)dt).URI))
                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt))
                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt))
                                                                .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                        }

                    }

                }

            }

            //Query IS NOT empty, so does have query members to fetch data from.
            else
            {

                //In case of a "Star" query, all the variables must be considered describe terms
                if (!describeQuery.DescribeTerms.Any())
                {
                    foreach(var evaluableQueryMember in evaluableQueryMembers)
                    {
                        #region PATTERN GROUP
                        if (evaluableQueryMember is RDFPatternGroup)
                        {
                            ((RDFPatternGroup)evaluableQueryMember).Variables.ForEach(v => describeQuery.AddDescribeTerm(v));
                        }
                        #endregion
                    }
                }

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
                                    if (graphOrStore.IsGraph())
                                    {

                                        //Search as RESOURCE (S-P-O)
                                        if (term is RDFResource)
                                        {
                                            RDFGraph desc = ((RDFGraph)graphOrStore).SelectTriplesBySubject((RDFResource)term)
                                                                    .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByPredicate((RDFResource)term))
                                                                        .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByObject((RDFResource)term));
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }

                                        //Search as LITERAL (L)
                                        else
                                        {
                                            RDFGraph desc = ((RDFGraph)graphOrStore).SelectTriplesByLiteral((RDFLiteral)term);
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                        }

                                    }

                                    //Search on STORE
                                    else
                                    {

                                        //Search as RESOURCE
                                        if (term is RDFResource)
                                        {

                                            //Search as BLANK RESOURCE (S-P-O)
                                            if (((RDFResource)term).IsBlank)
                                            {
                                                RDFMemoryStore desc = ((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)term)
                                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)term))
                                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)term));
                                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                            }

                                            //Search as NON-BLANK RESOURCE (C-S-P-O)
                                            else
                                            {
                                                RDFMemoryStore desc = ((RDFStore)graphOrStore).SelectQuadruplesByContext(new RDFContext(((RDFResource)term).URI))
                                                                        .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)term))
                                                                            .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)term))
                                                                                .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)term));
                                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                                            }

                                        }

                                        //Search as LITERAL (L)
                                        else
                                        {

                                            RDFMemoryStore desc = ((RDFStore)graphOrStore).SelectQuadruplesByLiteral((RDFLiteral)term);
                                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);

                                        }

                                    }

                                }

                            }

                        }

                    }

                    //The describe term is a resource
                    else
                    {

                        //Search on GRAPH
                        if (graphOrStore.IsGraph())
                        {

                            //Search as RESOURCE (S-P-O)
                            RDFGraph desc = ((RDFGraph)graphOrStore).SelectTriplesBySubject((RDFResource)dt)
                                                        .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByPredicate((RDFResource)dt))
                                                            .UnionWith(((RDFGraph)graphOrStore).SelectTriplesByObject((RDFResource)dt));
                            result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);

                        }

                        //Search on STORE
                        else
                        {

                            //Search as BLANK RESOURCE (S-P-O)
                            if (((RDFResource)dt).IsBlank)
                            {
                                RDFMemoryStore desc = ((RDFStore)graphOrStore).SelectQuadruplesBySubject((RDFResource)dt)
                                                           .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByPredicate((RDFResource)dt))
                                                               .UnionWith(((RDFStore)graphOrStore).SelectQuadruplesByObject((RDFResource)dt));
                                result.Merge(desc.ToDataTable(), true, MissingSchemaAction.Add);
                            }

                            //Search as NON-BLANK RESOURCE (C-S-P-O)
                            else
                            {
                                RDFMemoryStore desc = ((RDFStore)graphOrStore).SelectQuadruplesByContext(new RDFContext(((RDFResource)dt).URI))
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
        internal DataTable ApplyPattern(RDFPattern pattern, RDFGraph graph)
        {
            var matchingTriples = new List<RDFTriple>();
            var resultTable = new DataTable();

            //Apply the right pattern to the graph
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
        /// Applies the given property path to the given graph
        /// </summary>
        internal DataTable ApplyPropertyPath(RDFPropertyPath propertyPath, RDFDataSource graphOrStore)
        {
            var resultTable = new DataTable();

            //Translate property path into equivalent list of patterns
            var patternList = propertyPath.GetPatternList();

            //Evaluate produced list of patterns
            var patternTables = new List<DataTable>();
            patternList.ForEach(pattern =>
            {

                //Apply pattern to graph
                var patternTable = graphOrStore is RDFGraph ? ApplyPattern(pattern, (RDFGraph)graphOrStore) :
                                                              ApplyPattern(pattern, (RDFStore)graphOrStore);

                //Set extended properties
                patternTable.ExtendedProperties.Add("IsOptional", pattern.IsOptional);
                patternTable.ExtendedProperties.Add("JoinAsUnion", pattern.JoinAsUnion);

                //Add produced table
                patternTables.Add(patternTable);

            });

            //Merge produced list of tables
            resultTable = CombineTables(patternTables, false);

            //Remove property path variables
            var propPathCols = new List<DataColumn>();
            foreach (DataColumn dtCol in resultTable.Columns)
            {
                if (dtCol.ColumnName.StartsWith("?__PP"))
                {
                    propPathCols.Add(dtCol);
                }
            }
            propPathCols.ForEach(ppc =>
            {
                resultTable.Columns.Remove(ppc.ColumnName);
            });

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
            public Boolean Equals(DataColumn column1, DataColumn column2)
            {
                if (column1 != null)
                {
                    return column2 != null && column1.Caption.Equals(column2.Caption, StringComparison.Ordinal);
                }
                return column2 == null;
            }

            public Int32 GetHashCode(DataColumn column)
            {
                return column.Caption.GetHashCode();
            }
            #endregion

        }
        /// <summary>
        /// Static instance of the comparer used by the engine to compare data columns
        /// </summary>
        internal static readonly DataColumnComparer dtComparer = new DataColumnComparer();

        /// <summary>
        /// Adds a new column to the given table, avoiding duplicates 
        /// </summary>
        internal static void AddColumn(DataTable table, String columnName)
        {
            if (!table.Columns.Contains(columnName.Trim().ToUpperInvariant()))
            {
                table.Columns.Add(columnName.Trim().ToUpperInvariant(), Type.GetType("System.String"));
            }
        }

        /// <summary>
        /// Adds a new row to the given table 
        /// </summary>
        internal static void AddRow(DataTable table, Dictionary<String, String> bindings)
        {
            Boolean rowAdded = false;
            DataRow resultRow = table.NewRow();
            bindings.Keys.ToList().ForEach(k =>
            {
                if (table.Columns.Contains(k))
                {
                    resultRow[k] = bindings[k];
                    rowAdded = true;
                }
            });
            if (rowAdded)
            {
                table.Rows.Add(resultRow);
            }
        }

        /// <summary>
        /// Builds the table results of the pattern with values from the given graph
        /// </summary>
        internal void PopulateTable(RDFPattern pattern, List<RDFTriple> triples, RDFQueryEnums.RDFPatternHoles patternHole, DataTable resultTable)
        {
            var bindings = new Dictionary<String, String>();

            //Iterate result graph's triples
            foreach (var t in triples)
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
                        {
                            bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        }
                        break;
                    //->P->
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
                        break;
                    //S->->
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
                        break;
                    //->->
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(pattern.Subject.ToString(), t.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                        {
                            bindings.Add(pattern.Predicate.ToString(), t.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), t.Object.ToString());
                        }
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
            var bindings = new Dictionary<String, String>();

            //Iterate result store's quadruples
            foreach (var q in store)
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
                        {
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
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                        {
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
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
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
                        if (!bindings.ContainsKey(pattern.Subject.ToString()))
                        {
                            bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                        {
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        break;
                    //C->->->O
                    case RDFQueryEnums.RDFPatternHoles.SP:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                        {
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        break;
                    //->->P->
                    case RDFQueryEnums.RDFPatternHoles.CSO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString()))
                        {
                            bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->->P->
                    case RDFQueryEnums.RDFPatternHoles.SO:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //->S->->
                    case RDFQueryEnums.RDFPatternHoles.CPO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                        {
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->S->->
                    case RDFQueryEnums.RDFPatternHoles.PO:
                        bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //->->->
                    case RDFQueryEnums.RDFPatternHoles.CSPO:
                        bindings.Add(pattern.Context.ToString(), q.Context.ToString());
                        if (!bindings.ContainsKey(pattern.Subject.ToString()))
                        {
                            bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                        {
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
                        break;
                    //C->->->
                    case RDFQueryEnums.RDFPatternHoles.SPO:
                        bindings.Add(pattern.Subject.ToString(), q.Subject.ToString());
                        if (!bindings.ContainsKey(pattern.Predicate.ToString()))
                        {
                            bindings.Add(pattern.Predicate.ToString(), q.Predicate.ToString());
                        }
                        if (!bindings.ContainsKey(pattern.Object.ToString()))
                        {
                            bindings.Add(pattern.Object.ToString(), q.Object.ToString());
                        }
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
            IEnumerable<DataColumn> dt1Cols = dt1.Columns.OfType<DataColumn>();
            IEnumerable<DataColumn> dt2Cols = dt2.Columns.OfType<DataColumn>();
            IEnumerable<DataColumn> dt1Columns = (dt1Cols as IList<DataColumn> ?? dt1Cols.ToList<DataColumn>());
            IEnumerable<DataColumn> dt2Columns = (dt2Cols as IList<DataColumn> ?? dt2Cols.ToList<DataColumn>());

            //Determine common columns
            DataColumn[] commonColumns = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                            .Select(c => new DataColumn(c.Caption, c.DataType))
                                                            .ToArray();

            //PRODUCT-JOIN
            if (commonColumns.Length == 0)
            {

                //Create the structure of the product table
                result.Columns.AddRange(dt1Columns.Union(dt2Columns, dtComparer)
                              .Select(c => new DataColumn(c.Caption, c.DataType))
                              .ToArray());

                //Loop through dt1 table
                result.AcceptChanges();
                result.BeginLoadData();
                foreach (DataRow parentRow in dt1.Rows)
                {
                    Object[] firstArray = parentRow.ItemArray;

                    //Loop through dt2 table
                    foreach (DataRow childRow in dt2.Rows)
                    {
                        Object[] secondArray = childRow.ItemArray;
                        Object[] productArray = new Object[firstArray.Length + secondArray.Length];
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
                    for (Int32 i = 0; i < parentColumns.Length; i++)
                    {
                        parentColumns[i] = ds.Tables[0].Columns[commonColumns[i].ColumnName];
                    }
                    //Identify join columns from dt2
                    DataColumn[] childColumns = new DataColumn[commonColumns.Length];
                    for (Int32 i = 0; i < childColumns.Length; i++)
                    {
                        childColumns[i] = ds.Tables[1].Columns[commonColumns[i].ColumnName];
                    }

                    //Create the relation linking the common columns
                    DataRelation r = new DataRelation("JoinRelation", parentColumns, childColumns, false);
                    ds.Relations.Add(r);

                    //Create the structure of the join table
                    List<String> duplicateCols = new List<String>();
                    for (Int32 i = 0; i < ds.Tables[0].Columns.Count; i++)
                    {
                        result.Columns.Add(ds.Tables[0].Columns[i].ColumnName, ds.Tables[0].Columns[i].DataType);
                    }
                    for (Int32 i = 0; i < ds.Tables[1].Columns.Count; i++)
                    {
                        if (!result.Columns.Contains(ds.Tables[1].Columns[i].ColumnName))
                        {
                            result.Columns.Add(ds.Tables[1].Columns[i].ColumnName, ds.Tables[1].Columns[i].DataType);
                        }
                        else
                        {
                            //Manage duplicate columns by appending a known identificator to their name
                            result.Columns.Add(ds.Tables[1].Columns[i].ColumnName + "_DUPLICATE_", ds.Tables[1].Columns[i].DataType);
                            duplicateCols.Add(ds.Tables[1].Columns[i].ColumnName + "_DUPLICATE_");
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
                            Object[] parentArray = firstRow.ItemArray;
                            foreach (DataRow secondRow in childRows)
                            {
                                Object[] secondArray = secondRow.ItemArray;
                                Object[] joinArray = new Object[parentArray.Length + secondArray.Length];
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
            IEnumerable<DataColumn> dt1Cols = dt1.Columns.OfType<DataColumn>();
            IEnumerable<DataColumn> dt2Cols = dt2.Columns.OfType<DataColumn>();
            IEnumerable<DataColumn> dt1Columns = (dt1Cols as IList<DataColumn> ?? dt1Cols.ToList<DataColumn>());
            IEnumerable<DataColumn> dt2Columns = (dt2Cols as IList<DataColumn> ?? dt2Cols.ToList<DataColumn>());

            Boolean dt2IsOptionalTable = (dt2.ExtendedProperties.ContainsKey("IsOptional") && dt2.ExtendedProperties["IsOptional"].Equals(true));
            Boolean joinInvalidationFlag = false;
            Boolean foundAnyResult = false;
            String strResCol = String.Empty;


            //Step 1: Determine common columns
            DataColumn[] commonColumns = dt1Columns.Intersect(dt2Columns, dtComparer)
                                                            .Select(c => new DataColumn(c.Caption, c.DataType))
                                                            .ToArray();

            //Step 2: Create structure of finalResult table
            finalResult.Columns.AddRange(dt1Columns.Union(dt2Columns.Except(commonColumns), dtComparer)
                               .Select(c => new DataColumn(c.Caption, c.DataType))
                               .ToArray());

            //Step 3: Loop through dt1 table
            finalResult.AcceptChanges();
            finalResult.BeginLoadData();
            foreach (DataRow leftRow in dt1.Rows)
            {
                foundAnyResult = false;

                //Step 4: Loop through dt2 table
                foreach (DataRow rightRow in dt2.Rows)
                {
                    joinInvalidationFlag = false;

                    //Step 5: Create a temporary join row
                    DataRow joinRow = finalResult.NewRow();
                    foreach (DataColumn resCol in finalResult.Columns)
                    {
                        if (!joinInvalidationFlag)
                        {
                            strResCol = resCol.ToString();

                            //Step 6a: NON-COMMON column
                            if (!commonColumns.Any(col => col.ToString().Equals(strResCol, StringComparison.Ordinal)))
                            {

                                //Take value from left
                                if (dt1Columns.Any(col => col.ToString().Equals(strResCol, StringComparison.Ordinal)))
                                {
                                    joinRow[strResCol] = leftRow[strResCol];
                                }

                                //Take value from right
                                else
                                {
                                    joinRow[strResCol] = rightRow[strResCol];
                                }

                            }

                            //Step 6b: COMMON column
                            else
                            {

                                //Left value is NULL
                                if (leftRow.IsNull(strResCol))
                                {

                                    //Right value is NULL
                                    if (rightRow.IsNull(strResCol))
                                    {
                                        //Take NULL value
                                        joinRow[strResCol] = DBNull.Value;
                                    }

                                    //Right value is NOT NULL
                                    else
                                    {
                                        //Take value from right
                                        joinRow[strResCol] = rightRow[strResCol];
                                    }

                                }

                                //Left value is NOT NULL
                                else
                                {

                                    //Right value is NULL
                                    if (rightRow.IsNull(strResCol))
                                    {
                                        //Take value from left
                                        joinRow[strResCol] = leftRow[strResCol];
                                    }

                                    //Right value is NOT NULL
                                    else
                                    {

                                        //Left value is EQUAL TO right value
                                        if (leftRow[strResCol].ToString().Equals(rightRow[strResCol].ToString(), StringComparison.Ordinal))
                                        {
                                            //Take value from left (it's the same)
                                            joinRow[strResCol] = leftRow[strResCol];
                                        }

                                        //Left value is NOT EQUAL TO right value
                                        else
                                        {
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
                    if (!joinInvalidationFlag)
                    {
                        joinRow.AcceptChanges();
                        finalResult.Rows.Add(joinRow);
                        foundAnyResult = true;
                    }

                }

                //Step 8: Manage presence of "OPTIONAL" pattern to the right
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
        /// Merges / Joins / Products the given list of data tables, based on presence of common columns and dynamic detection of Optional / Union operators
        /// </summary>
        internal DataTable CombineTables(List<DataTable> dataTables, Boolean isMerge)
        {
            DataTable finalTable = new DataTable();
            Boolean switchToOuterJoin = false;

            if (dataTables.Count > 0)
            {

                //Process Unions
                for (Int32 i = 1; i < dataTables.Count; i++)
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
                for (Int32 i = 1; i < dataTables.Count; i++)
                {

                    //Set automatic switch to OuterJoin in case of relevant "Optional" detected
                    switchToOuterJoin = (switchToOuterJoin || (dataTables[i].ExtendedProperties.ContainsKey("IsOptional") && dataTables[i].ExtendedProperties["IsOptional"].Equals(true)));

                    //Support OPTIONAL data
                    if (switchToOuterJoin)
                    {
                        finalTable = OuterJoinTables(finalTable, dataTables[i]);
                    }

                    //Do not support OPTIONAL data
                    else
                    {
                        finalTable = InnerJoinTables(finalTable, dataTables[i]);
                    }

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
                var nonProjCols = new List<DataColumn>();
                foreach (DataColumn dtCol in table.Columns)
                {
                    if (!query.ProjectionVars.Any(pv => pv.Key.ToString().Equals(dtCol.ColumnName, StringComparison.OrdinalIgnoreCase)))
                    {
                        nonProjCols.Add(dtCol);
                    }
                }
                nonProjCols.ForEach(npc =>
                {
                    table.Columns.Remove(npc.ColumnName);
                });

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