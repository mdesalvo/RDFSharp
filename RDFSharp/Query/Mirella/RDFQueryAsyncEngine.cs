/*
   Copyright 2012-2020 Marco De Salvo

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
using System.Threading.Tasks;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryAsyncEngine is the engine for asynchronous execution of SPARQL queries (MIRELLA ASYNC)
    /// </summary>
    internal class RDFQueryAsyncEngine : RDFQueryEngine
    {
        #region Methods

        #region MIRELLA ASYNC SPARQL

        /// <summary>
        /// Asynchronously evaluates the given SPARQL SELECT query on the given RDF datasource
        /// </summary>
        internal async Task<RDFSelectQueryResult> EvaluateSelectQueryAsync(RDFSelectQuery selectQuery, RDFDataSource datasource)
        {
            //Inject SPARQL values within every evaluable member
            selectQuery.InjectValues(selectQuery.GetValues());

            RDFSelectQueryResult queryResult = new RDFSelectQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = selectQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {

                //Iterate the evaluable members of the query
                Dictionary<long, List<DataTable>> fedQueryMemberTemporaryResultTables = new Dictionary<long, List<DataTable>>();
                foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
                {

                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        //Step 0: Cleanup eventual data from stateful pattern group members
                        ((RDFPatternGroup)evaluableQueryMember).GroupMembers.ForEach(gm =>
                        {
                            if (gm is RDFExistsFilter)
                                ((RDFExistsFilter)gm).PatternResults?.Clear();
                        });

                        //Step 1: Get the intermediate result tables of the pattern group
                        if (datasource.IsFederation())
                        {
                            //Ensure to skip tricky empty federations
                            if (((RDFFederation)datasource).DataSourcesCount == 0)
                            {
                                fedQueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, new List<DataTable>());
                                QueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, new List<DataTable>());
                            }

                            #region TrueFederations
                            foreach (RDFDataSource fedDataSource in (RDFFederation)datasource)
                            {

                                //Step FED.1: Evaluate the pattern group on the current data source
                                await EvaluatePatternGroupAsync(selectQuery, (RDFPatternGroup)evaluableQueryMember, fedDataSource, true);

                                //Step FED.2: Federate the results of the pattern group on the current data source
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
                            await EvaluatePatternGroupAsync(selectQuery, (RDFPatternGroup)evaluableQueryMember, datasource, false);
                        }

                        //Step 2: Get the result table of the pattern group
                        await FinalizePatternGroupAsync(selectQuery, (RDFPatternGroup)evaluableQueryMember);

                        //Step 3: Apply the filters of the pattern group to its result table
                        await ApplyFiltersAsync(selectQuery, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = await ((RDFSelectQuery)evaluableQueryMember).ApplyToDataSourceAsync(datasource);
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
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional
                                                                                                                                        || (bool)QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"];
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
                DataTable queryResultTable = await CombineTablesAsync(QueryMemberFinalResultTables.Values.ToList(), false);

                //Step 5: Apply the modifiers of the query to the result table
                queryResult.SelectResults = await ApplyModifiersAsync(selectQuery, queryResultTable);

            }

            queryResult.SelectResults.TableName = selectQuery.ToString();
            return queryResult;
        }

        /// <summary>
        /// Asynchronously evaluates the given SPARQL ASK query on the given RDF datasource
        /// </summary>
        internal async Task<RDFAskQueryResult> EvaluateAskQueryAsync(RDFAskQuery askQuery, RDFDataSource datasource)
        {
            //Inject SPARQL values within every evaluable member
            askQuery.InjectValues(askQuery.GetValues());

            RDFAskQueryResult askResult = new RDFAskQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = askQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {

                //Iterate the evaluable members of the query
                Dictionary<long, List<DataTable>> fedQueryMemberTemporaryResultTables = new Dictionary<long, List<DataTable>>();
                foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
                {

                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        //Step 0: Cleanup eventual data from stateful pattern group members
                        ((RDFPatternGroup)evaluableQueryMember).GroupMembers.ForEach(gm =>
                        {
                            if (gm is RDFExistsFilter)
                                ((RDFExistsFilter)gm).PatternResults?.Clear();
                        });

                        //Step 1: Get the intermediate result tables of the pattern group
                        if (datasource.IsFederation())
                        {
                            //Ensure to skip tricky empty federations
                            if (((RDFFederation)datasource).DataSourcesCount == 0)
                            {
                                fedQueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, new List<DataTable>());
                                QueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, new List<DataTable>());
                            }

                            #region TrueFederations
                            foreach (RDFDataSource fedDataSource in (RDFFederation)datasource)
                            {

                                //Step FED.1: Evaluate the current pattern group on the data source
                                await EvaluatePatternGroupAsync(askQuery, (RDFPatternGroup)evaluableQueryMember, fedDataSource, true);

                                //Step FED.2: Federate the results of the current pattern group on the data source
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
                            await EvaluatePatternGroupAsync(askQuery, (RDFPatternGroup)evaluableQueryMember, datasource, false);
                        }

                        //Step 2: Get the result table of the pattern group
                        await FinalizePatternGroupAsync(askQuery, (RDFPatternGroup)evaluableQueryMember);

                        //Step 3: Apply the filters of the pattern group to its result table
                        await ApplyFiltersAsync(askQuery, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = await ((RDFSelectQuery)evaluableQueryMember).ApplyToDataSourceAsync(datasource);
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
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional
                                                                                                                                        || (bool)QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"];
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
                DataTable queryResultTable = await CombineTablesAsync(QueryMemberFinalResultTables.Values.ToList(), false);

                //Step 5: Transform the result into a boolean response
                askResult.AskResult = (queryResultTable.Rows.Count > 0);

            }

            return askResult;
        }

        /// <summary>
        /// Asynchronously evaluates the given SPARQL CONSTRUCT query on the given RDF datasource
        /// </summary>
        internal async Task<RDFConstructQueryResult> EvaluateConstructQueryAsync(RDFConstructQuery constructQuery, RDFDataSource datasource)
        {
            //Inject SPARQL values within every evaluable member
            constructQuery.InjectValues(constructQuery.GetValues());

            RDFConstructQueryResult constructResult = new RDFConstructQueryResult(this.ToString());
            List<RDFQueryMember> evaluableQueryMembers = constructQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {

                //Iterate the evaluable members of the query
                Dictionary<long, List<DataTable>> fedQueryMemberTemporaryResultTables = new Dictionary<long, List<DataTable>>();
                foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
                {

                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        //Step 0: Cleanup eventual data from stateful pattern group members
                        ((RDFPatternGroup)evaluableQueryMember).GroupMembers.ForEach(gm =>
                        {
                            if (gm is RDFExistsFilter)
                                ((RDFExistsFilter)gm).PatternResults?.Clear();
                        });

                        //Step 1: Get the intermediate result tables of the pattern group
                        if (datasource.IsFederation())
                        {
                            //Ensure to skip tricky empty federations
                            if (((RDFFederation)datasource).DataSourcesCount == 0)
                            {
                                fedQueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, new List<DataTable>());
                                QueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, new List<DataTable>());
                            }

                            #region TrueFederations
                            foreach (RDFDataSource fedDataSource in (RDFFederation)datasource)
                            {

                                //Step FED.1: Evaluate the current pattern group on the data source
                                await EvaluatePatternGroupAsync(constructQuery, (RDFPatternGroup)evaluableQueryMember, fedDataSource, true);

                                //Step FED.2: Federate the results of the pattern group on the data source
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
                            await EvaluatePatternGroupAsync(constructQuery, (RDFPatternGroup)evaluableQueryMember, datasource, false);
                        }

                        //Step 2: Get the result table of the pattern group
                        await FinalizePatternGroupAsync(constructQuery, (RDFPatternGroup)evaluableQueryMember);

                        //Step 3: Apply the filters of the pattern group to its result table
                        await ApplyFiltersAsync(constructQuery, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = await ((RDFSelectQuery)evaluableQueryMember).ApplyToDataSourceAsync(datasource);
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
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional
                                                                                                                                        || (bool)QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"];
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
                DataTable queryResultTable = await CombineTablesAsync(QueryMemberFinalResultTables.Values.ToList(), false);

                //Step 5: Fill the templates from the result table
                DataTable filledResultTable = await FillTemplatesAsync(constructQuery, queryResultTable);

                //Step 6: Apply the modifiers of the query to the result table
                constructResult.ConstructResults = await ApplyModifiersAsync(constructQuery, filledResultTable);

            }

            constructResult.ConstructResults.TableName = constructQuery.ToString();
            return constructResult;
        }

        /// <summary>
        /// Asynchronously evaluates the given SPARQL DESCRIBE query on the given RDF datasource
        /// </summary>
        internal async Task<RDFDescribeQueryResult> EvaluateDescribeQueryAsync(RDFDescribeQuery describeQuery, RDFDataSource datasource)
        {
            //Inject SPARQL values within every evaluable member
            describeQuery.InjectValues(describeQuery.GetValues());

            RDFDescribeQueryResult queryResult = new RDFDescribeQueryResult(this.ToString());
            List<RDFQueryMember> evaluableQueryMembers = describeQuery.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {

                //Iterate the evaluable members of the query
                Dictionary<long, List<DataTable>> fedQueryMemberTemporaryResultTables = new Dictionary<long, List<DataTable>>();
                foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
                {

                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        //Step 0: Cleanup eventual data from stateful pattern group members
                        ((RDFPatternGroup)evaluableQueryMember).GroupMembers.ForEach(gm =>
                        {
                            if (gm is RDFExistsFilter)
                                ((RDFExistsFilter)gm).PatternResults?.Clear();
                        });

                        //Step 1: Get the intermediate result tables of the pattern group
                        if (datasource.IsFederation())
                        {
                            //Ensure to skip tricky empty federations
                            if (((RDFFederation)datasource).DataSourcesCount == 0)
                            {
                                fedQueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, new List<DataTable>());
                                QueryMemberTemporaryResultTables.Add(evaluableQueryMember.QueryMemberID, new List<DataTable>());
                            }

                            #region TrueFederations
                            foreach (RDFDataSource fedDataSource in (RDFFederation)datasource)
                            {

                                //Step FED.1: Evaluate the pattern group on the data source
                                await EvaluatePatternGroupAsync(describeQuery, (RDFPatternGroup)evaluableQueryMember, fedDataSource, true);

                                //Step FED.2: Federate the results of the pattern group on the data source
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
                            await EvaluatePatternGroupAsync(describeQuery, (RDFPatternGroup)evaluableQueryMember, datasource, false);
                        }

                        //Step 2: Get the result table of the pattern group
                        await FinalizePatternGroupAsync(describeQuery, (RDFPatternGroup)evaluableQueryMember);

                        //Step 3: Apply the filters of the pattern group to its result table
                        await ApplyFiltersAsync(describeQuery, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = await ((RDFSelectQuery)evaluableQueryMember).ApplyToDataSourceAsync(datasource);
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
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional
                                                                                                                                        || (bool)QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"];
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
                DataTable queryResultTable = await CombineTablesAsync(this.QueryMemberFinalResultTables.Values.ToList(), false);

                //Step 5: Describe the terms from the result table
                DataTable describeResultTable = new DataTable(this.ToString());
                if (datasource.IsFederation())
                {

                    #region TrueFederations
                    foreach (RDFDataSource fedDataSource in (RDFFederation)datasource)
                    {
                        //Ensure to skip tricky empty federations
                        if (fedDataSource.IsFederation() && ((RDFFederation)fedDataSource).DataSourcesCount == 0)
                            continue;

                        await Task.Run(() => describeResultTable.Merge(DescribeTerms(describeQuery, fedDataSource, queryResultTable), true, MissingSchemaAction.Add));
                    }
                    #endregion

                }
                else
                {
                    describeResultTable = await DescribeTermsAsync(describeQuery, datasource, queryResultTable);
                }

                //Step 6: Apply the modifiers of the query to the result table
                queryResult.DescribeResults = await ApplyModifiersAsync(describeQuery, describeResultTable);

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
                        foreach (RDFDataSource fedDataSource in (RDFFederation)datasource)
                        {
                            //Ensure to skip tricky empty federations
                            if (fedDataSource.IsFederation() && ((RDFFederation)fedDataSource).DataSourcesCount == 0)
                                continue;

                            await Task.Run(() => describeResultTable.Merge(DescribeTerms(describeQuery, fedDataSource, new DataTable()), true, MissingSchemaAction.Add));
                        }
                        #endregion

                    }
                    else
                    {
                        describeResultTable = await DescribeTermsAsync(describeQuery, datasource, new DataTable());
                    }

                    //Step 2: Apply the modifiers of the query to the result table
                    queryResult.DescribeResults = await ApplyModifiersAsync(describeQuery, describeResultTable);

                }

            }

            queryResult.DescribeResults.TableName = describeQuery.ToString();
            return queryResult;
        }

        /// <summary>
        /// Asynchronously gets the intermediate result tables of the given pattern group
        /// </summary>
        internal async Task EvaluatePatternGroupAsync(RDFQuery query, RDFPatternGroup patternGroup, RDFDataSource dataSource, bool withinFederation)
        {
            QueryMemberTemporaryResultTables[patternGroup.QueryMemberID] = new List<DataTable>();

            //Iterate the evaluable members of the pattern group
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers()
                                                                         .Distinct()
                                                                         .ToList();
            foreach (RDFPatternGroupMember evaluablePGMember in evaluablePGMembers)
            {

                #region Pattern
                if (evaluablePGMember is RDFPattern)
                {
                    DataTable patternResultsTable = await ApplyPatternAsync((RDFPattern)evaluablePGMember, dataSource);

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
                    DataTable pPathResultsTable = await ApplyPropertyPathAsync((RDFPropertyPath)evaluablePGMember, dataSource);

                    //Set name of result datatable
                    pPathResultsTable.TableName = ((RDFPropertyPath)evaluablePGMember).ToString();

                    //Save result datatable
                    QueryMemberTemporaryResultTables[patternGroup.QueryMemberID].Add(pPathResultsTable);
                }
                #endregion

                #region Values
                else if (evaluablePGMember is RDFValues)
                {
                    DataTable valuesResultsTable = ((RDFValues)evaluablePGMember).GetDataTable();

                    //Save result datatable
                    QueryMemberTemporaryResultTables[patternGroup.QueryMemberID].Add(valuesResultsTable);

                    //Inject SPARQL values filter
                    patternGroup.AddFilter(((RDFValues)evaluablePGMember).GetValuesFilter());
                }
                #endregion

                #region Filter
                else if (evaluablePGMember is RDFFilter)
                {

                    #region ExistsFilter
                    if (evaluablePGMember is RDFExistsFilter)
                    {
                        DataTable existsFilterResultsTable = await ApplyPatternAsync(((RDFExistsFilter)evaluablePGMember).Pattern, dataSource);

                        //Set name and metadata of result datatable
                        existsFilterResultsTable.TableName = ((RDFExistsFilter)evaluablePGMember).Pattern.ToString();
                        existsFilterResultsTable.ExtendedProperties.Add("IsOptional", false);
                        existsFilterResultsTable.ExtendedProperties.Add("JoinAsUnion", false);

                        //Initialize result datatable if needed
                        if (((RDFExistsFilter)evaluablePGMember).PatternResults == null)
                            ((RDFExistsFilter)evaluablePGMember).PatternResults = existsFilterResultsTable.Clone();

                        //Assign result datatable (federation merges data)
                        if (withinFederation)
                            ((RDFExistsFilter)evaluablePGMember).PatternResults.Merge(existsFilterResultsTable, true, MissingSchemaAction.Add);
                        else
                            ((RDFExistsFilter)evaluablePGMember).PatternResults = existsFilterResultsTable;
                    }
                    #endregion

                }
                #endregion

            }
        }

        /// <summary>
        /// Asynchronously gets the final result table of the given pattern group
        /// </summary>
        internal async Task FinalizePatternGroupAsync(RDFQuery query, RDFPatternGroup patternGroup)
        {
            List<RDFPatternGroupMember> evaluablePGMembers = patternGroup.GetEvaluablePatternGroupMembers().ToList();
            if (evaluablePGMembers.Any())
            {

                //Populate query member result table
                DataTable queryMemberFinalResultTable = await CombineTablesAsync(QueryMemberTemporaryResultTables[patternGroup.QueryMemberID], false);

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
                    QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties["IsOptional"] = patternGroup.IsOptional
                                                                                                                    || (bool)QueryMemberFinalResultTables[patternGroup.QueryMemberID].ExtendedProperties["IsOptional"];
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
        /// Asynchronously applies the filters of the given pattern group to its result table
        /// </summary>
        internal async Task ApplyFiltersAsync(RDFQuery query, RDFPatternGroup patternGroup)
            => await Task.Run(() => ApplyFilters(query, patternGroup));

        /// <summary>
        /// Asynchronously applies the query modifiers to the query result table
        /// </summary>
        internal async Task<DataTable> ApplyModifiersAsync(RDFQuery query, DataTable table)
            => await Task.Run(() => ApplyModifiers(query, table));

        /// <summary>
        /// Asynchronously applies the given pattern to the given data source
        /// </summary>
        internal async Task<DataTable> ApplyPatternAsync(RDFPattern pattern, RDFDataSource dataSource)
        {
            switch (dataSource)
            {
                case RDFGraph dataSourceGraph:
                    return await Task.Run(() => ApplyPattern(pattern, dataSourceGraph));

                case RDFStore dataSourceStore:
                    return await Task.Run(() => ApplyPattern(pattern, dataSourceStore));

                case RDFFederation dataSourceFederation:
                    return await Task.Run(() => ApplyPattern(pattern, dataSourceFederation));

                case RDFSPARQLEndpoint dataSourceSparqlEndpoint:
                    return await Task.Run(() => ApplyPattern(pattern, dataSourceSparqlEndpoint));
            }
            return new DataTable();
        }

        /// <summary>
        /// Asynchronously applies the given property path to the given graph
        /// </summary>
        internal async Task<DataTable> ApplyPropertyPathAsync(RDFPropertyPath propertyPath, RDFDataSource dataSource)
        {
            DataTable resultTable = new DataTable();

            //Translate property path into equivalent list of patterns
            List<RDFPattern> patternList = propertyPath.GetPatternList();

            //Evaluate produced list of patterns
            List<DataTable> patternTables = new List<DataTable>();
            foreach (RDFPattern pattern in patternList)
            {

                //Apply pattern to graph
                DataTable patternTable = await ApplyPatternAsync(pattern, dataSource);

                //Set extended properties
                patternTable.ExtendedProperties.Add("IsOptional", pattern.IsOptional);
                patternTable.ExtendedProperties.Add("JoinAsUnion", pattern.JoinAsUnion);

                //Add produced table
                patternTables.Add(patternTable);

            }

            //Merge produced list of tables
            resultTable = await CombineTablesAsync(patternTables, false);

            //Remove property path variables
            List<DataColumn> propPathCols = new List<DataColumn>();
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

        /// <summary>
        /// Asynchronously fills the templates of the given CONSTRUCT query with data from the given result table
        /// </summary>
        internal async Task<DataTable> FillTemplatesAsync(RDFConstructQuery constructQuery, DataTable resultTable)
            => await Task.Run(() => FillTemplates(constructQuery, resultTable));

        /// <summary>
        /// Asynchronously describes the terms of the given DESCRIBE query with data from the given result table
        /// </summary>
        internal async Task<DataTable> DescribeTermsAsync(RDFDescribeQuery describeQuery, RDFDataSource dataSource, DataTable resultTable)
            => await Task.Run(() => DescribeTerms(describeQuery, dataSource, resultTable));

        #endregion

        #region MIRELLA ASYNC TABLE

        /// <summary>
        /// Asynchronously Merges / Joins / Products the given list of data tables, based on presence of common columns and dynamic detection of Optional / Union operators
        /// </summary>
        internal async Task<DataTable> CombineTablesAsync(List<DataTable> dataTables, bool isMerge)
            => await Task.Run(() => CombineTables(dataTables, isMerge));

        #endregion

        #endregion
    }
}