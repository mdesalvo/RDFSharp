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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOperationEngine is the engine for execution of SPARQL UPDATE operations
    /// </summary>
    internal class RDFOperationEngine : RDFQueryEngine
    {
        #region Methods

        #region MIRELLA SPARQL UPDATE
        /// <summary>
        /// Evaluates the given SPARQL INSERT DATA operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateInsertDataOperation(RDFInsertDataOperation insertDataOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult() {
                InsertResults = PopulateInsertOperationResults(insertDataOperation.InsertTemplates, datasource) };
            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL INSERT WHERE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateInsertWhereOperation(RDFInsertWhereOperation insertWhereOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();

            #region CONSTRUCT
            //Inject SPARQL values within every evaluable member
            insertWhereOperation.InjectValues(insertWhereOperation.GetValues());

            DataTable insertWhereResultTable = new DataTable();
            RDFConstructQueryResult constructResult = new RDFConstructQueryResult(insertWhereOperation.ToString());
            List<RDFQueryMember> evaluableQueryMembers = insertWhereOperation.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {
                //Iterate the evaluable members of the query
                Dictionary<long, List<DataTable>> fedQueryMemberTemporaryResultTables = new Dictionary<long, List<DataTable>>();
                foreach (RDFQueryMember evaluableQueryMember in evaluableQueryMembers)
                {
                    #region PATTERN GROUP
                    if (evaluableQueryMember is RDFPatternGroup)
                    {
                        //Cleanup eventual data from stateful pattern group members
                        ((RDFPatternGroup)evaluableQueryMember).GroupMembers.ForEach(gm =>
                        {
                            if (gm is RDFExistsFilter)
                                ((RDFExistsFilter)gm).PatternResults?.Clear();
                        });

                        //Get the intermediate result tables of the pattern group
                        EvaluatePatternGroup(insertWhereOperation, (RDFPatternGroup)evaluableQueryMember, datasource, false);

                        //Get the result table of the pattern group
                        FinalizePatternGroup(insertWhereOperation, (RDFPatternGroup)evaluableQueryMember);

                        //Apply the filters of the pattern group to its result table
                        ApplyFilters(insertWhereOperation, (RDFPatternGroup)evaluableQueryMember);
                    }
                    #endregion

                    #region SUBQUERY
                    else if (evaluableQueryMember is RDFQuery)
                    {
                        //Get the result table of the subquery
                        RDFSelectQueryResult subQueryResult = ((RDFSelectQuery)evaluableQueryMember).ApplyToDataSource(datasource);
                        if (!QueryMemberFinalResultTables.ContainsKey(evaluableQueryMember.QueryMemberID))
                        {
                            //Populate its name
                            QueryMemberFinalResultTables.Add(evaluableQueryMember.QueryMemberID, subQueryResult.SelectResults);

                            //Populate its metadata (IsOptional)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("IsOptional"))
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("IsOptional", ((RDFSelectQuery)evaluableQueryMember).IsOptional);
                            else
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"] = ((RDFSelectQuery)evaluableQueryMember).IsOptional
                                                                                                                                        || (bool)QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["IsOptional"];

                            //Populate its metadata (JoinAsUnion)
                            if (!QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.ContainsKey("JoinAsUnion"))
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties.Add("JoinAsUnion", ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion);
                            else
                                QueryMemberFinalResultTables[evaluableQueryMember.QueryMemberID].ExtendedProperties["JoinAsUnion"] = ((RDFSelectQuery)evaluableQueryMember).JoinAsUnion;
                        }
                    }
                    #endregion
                }

                //Get the result table of the query
                insertWhereResultTable = CombineTables(QueryMemberFinalResultTables.Values.ToList(), false);
            }

            //Fill the templates from the result table
            DataTable filledResultTable = FillTemplates(insertWhereOperation.InsertTemplates, insertWhereResultTable, datasource.IsStore());

            //Apply the modifiers of the query to the result table
            constructResult.ConstructResults = ApplyModifiers(insertWhereOperation, filledResultTable);
            #endregion

            #region INSERT
            List<RDFPattern> insertWhereTemplates = new List<RDFPattern>();
            if (datasource.IsGraph())
            {
                RDFContext graphContext = new RDFContext(((RDFGraph)datasource).Context);
                RDFGraph insertWhereGraph = RDFGraph.FromDataTable(constructResult.ConstructResults);
                foreach (RDFTriple insertWhereTriple in insertWhereGraph)
                    insertWhereTemplates.Add(new RDFPattern(graphContext, insertWhereTriple.Subject, insertWhereTriple.Predicate, insertWhereTriple.Object));
            }
            else if (datasource.IsStore())
            {
                RDFMemoryStore insertWhereStore = RDFMemoryStore.FromDataTable(constructResult.ConstructResults);
                foreach (RDFQuadruple insertWhereQuadruple in insertWhereStore)
                    insertWhereTemplates.Add(new RDFPattern(insertWhereQuadruple.Context, insertWhereQuadruple.Subject, insertWhereQuadruple.Predicate, insertWhereQuadruple.Object));
            }
            operationResult.InsertResults = PopulateInsertOperationResults(insertWhereTemplates, datasource);
            #endregion

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL DELETE DATA operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteDataOperation(RDFDeleteDataOperation deleteDataOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult() {
                DeleteResults = PopulateDeleteOperationResults(deleteDataOperation.DeleteTemplates, datasource) };
            return operationResult;
        }
        
        /// <summary>
        /// Populates a datatble with data from the given INSERT templates
        /// </summary>
        internal DataTable PopulateInsertOperationResults(List<RDFPattern> insertDataTemplates, RDFDataSource datasource)
        {
            DataTable resultTable = new DataTable("INSERT_RESULTS");
            if (datasource.IsStore())
                resultTable.Columns.Add("?CONTEXT", SystemString);
            resultTable.Columns.Add("?SUBJECT", SystemString);
            resultTable.Columns.Add("?PREDICATE", SystemString);
            resultTable.Columns.Add("?OBJECT", SystemString);

            Dictionary<string, string> bindings = new Dictionary<string, string>();
            insertDataTemplates.ForEach(insertTemplate =>
            {
                //GRAPH
                if (datasource.IsGraph())
                {
                    RDFTriple insertTriple = insertTemplate.Object is RDFLiteral
                                                ? new RDFTriple((RDFResource)insertTemplate.Subject, (RDFResource)insertTemplate.Predicate, (RDFLiteral)insertTemplate.Object)
                                                : new RDFTriple((RDFResource)insertTemplate.Subject, (RDFResource)insertTemplate.Predicate, (RDFResource)insertTemplate.Object);
                    if (!((RDFGraph)datasource).ContainsTriple(insertTriple))
                    {
                        //Add the bindings to the operation result
                        bindings.Add("?SUBJECT", insertTriple.Subject.ToString());
                        bindings.Add("?PREDICATE", insertTriple.Predicate.ToString());
                        bindings.Add("?OBJECT", insertTriple.Object.ToString());
                        AddRow(resultTable, bindings);
                        bindings.Clear();

                        //Add the triple to the graph
                        ((RDFGraph)datasource).AddTriple(insertTriple);
                    }
                }

                //STORE
                else if (datasource.IsStore())
                {
                    RDFContext insertContext = insertTemplate.Context as RDFContext ?? new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri);
                    RDFQuadruple insertQuadruple = insertTemplate.Object is RDFLiteral
                                                      ? new RDFQuadruple(insertContext, (RDFResource)insertTemplate.Subject, (RDFResource)insertTemplate.Predicate, (RDFLiteral)insertTemplate.Object)
                                                      : new RDFQuadruple(insertContext, (RDFResource)insertTemplate.Subject, (RDFResource)insertTemplate.Predicate, (RDFResource)insertTemplate.Object);
                    if (!((RDFStore)datasource).ContainsQuadruple(insertQuadruple))
                    {
                        //Add the bindings to the operation result
                        bindings.Add("?CONTEXT", insertContext.ToString());
                        bindings.Add("?SUBJECT", insertQuadruple.Subject.ToString());
                        bindings.Add("?PREDICATE", insertQuadruple.Predicate.ToString());
                        bindings.Add("?OBJECT", insertQuadruple.Object.ToString());
                        AddRow(resultTable, bindings);
                        bindings.Clear();

                        //Add the quadruple to the store
                        ((RDFStore)datasource).AddQuadruple(insertQuadruple);
                    }
                }
            });

            return resultTable;
        }

        /// <summary>
        /// Populates a datatble with data from the given DELETE templates
        /// </summary>
        internal DataTable PopulateDeleteOperationResults(List<RDFPattern> deleteDataTemplates, RDFDataSource datasource)
        {
            DataTable resultTable = new DataTable("DELETE_RESULTS");
            if (datasource.IsStore())
                resultTable.Columns.Add("?CONTEXT", SystemString);
            resultTable.Columns.Add("?SUBJECT", SystemString);
            resultTable.Columns.Add("?PREDICATE", SystemString);
            resultTable.Columns.Add("?OBJECT", SystemString);

            Dictionary<string, string> bindings = new Dictionary<string, string>();
            deleteDataTemplates.ForEach(deleteTemplate =>
            {
                //GRAPH
                if (datasource.IsGraph())
                {
                    RDFTriple deleteTriple = deleteTemplate.Object is RDFLiteral
                                                ? new RDFTriple((RDFResource)deleteTemplate.Subject, (RDFResource)deleteTemplate.Predicate, (RDFLiteral)deleteTemplate.Object)
                                                : new RDFTriple((RDFResource)deleteTemplate.Subject, (RDFResource)deleteTemplate.Predicate, (RDFResource)deleteTemplate.Object);
                    if (((RDFGraph)datasource).ContainsTriple(deleteTriple))
                    {
                        //Add the bindings to the operation result
                        bindings.Add("?SUBJECT", deleteTriple.Subject.ToString());
                        bindings.Add("?PREDICATE", deleteTriple.Predicate.ToString());
                        bindings.Add("?OBJECT", deleteTriple.Object.ToString());
                        AddRow(resultTable, bindings);
                        bindings.Clear();

                        //Remove the triple from the graph
                        ((RDFGraph)datasource).RemoveTriple(deleteTriple);
                    }
                }

                //STORE
                else if (datasource.IsStore())
                {
                    RDFContext deleteContext = deleteTemplate.Context as RDFContext ?? new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri);
                    RDFQuadruple deleteQuadruple = deleteTemplate.Object is RDFLiteral
                                                      ? new RDFQuadruple(deleteContext, (RDFResource)deleteTemplate.Subject, (RDFResource)deleteTemplate.Predicate, (RDFLiteral)deleteTemplate.Object)
                                                      : new RDFQuadruple(deleteContext, (RDFResource)deleteTemplate.Subject, (RDFResource)deleteTemplate.Predicate, (RDFResource)deleteTemplate.Object);
                    if (((RDFStore)datasource).ContainsQuadruple(deleteQuadruple))
                    {
                        //Add the bindings to the operation result
                        bindings.Add("?CONTEXT", deleteContext.ToString());
                        bindings.Add("?SUBJECT", deleteQuadruple.Subject.ToString());
                        bindings.Add("?PREDICATE", deleteQuadruple.Predicate.ToString());
                        bindings.Add("?OBJECT", deleteQuadruple.Object.ToString());
                        AddRow(resultTable, bindings);
                        bindings.Clear();

                        //Remove the quadruple from the store
                        ((RDFStore)datasource).RemoveQuadruple(deleteQuadruple);
                    }
                }
            });

            return resultTable;
        }
        #endregion

        #endregion
    }
}