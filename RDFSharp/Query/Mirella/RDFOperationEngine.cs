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
using System.Net;
using System.Threading.Tasks;
using System.Web;
using static RDFSharp.Query.RDFQueryUtilities;

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
            RDFOperationResult operationResult = new RDFOperationResult()
            {
                InsertResults = PopulateInsertOperationResults(insertDataOperation.InsertTemplates, datasource)
            };
            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL INSERT WHERE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateInsertWhereOperation(RDFInsertWhereOperation insertWhereOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();

            //Execute the CONSTRUCT query for materialization of the operation templates
            RDFConstructQueryResult constructResult = ExecuteConstructQueryFromOperation(insertWhereOperation, datasource);

            //Use materialized templates for execution of the operation
            List<RDFPattern> insertWhereTemplates = new List<RDFPattern>();
            if (datasource.IsGraph())
            {
                RDFGraph insertWhereGraph = RDFGraph.FromDataTable(constructResult.ConstructResults);
                foreach (RDFTriple insertWhereTriple in insertWhereGraph)
                    insertWhereTemplates.Add(new RDFPattern(insertWhereTriple.Subject, insertWhereTriple.Predicate, insertWhereTriple.Object));
            }
            else if (datasource.IsStore())
            {
                RDFMemoryStore insertWhereStore = RDFMemoryStore.FromDataTable(constructResult.ConstructResults);
                foreach (RDFQuadruple insertWhereQuadruple in insertWhereStore)
                    insertWhereTemplates.Add(new RDFPattern(insertWhereQuadruple.Context, insertWhereQuadruple.Subject, insertWhereQuadruple.Predicate, insertWhereQuadruple.Object));
            }
            operationResult.InsertResults = PopulateInsertOperationResults(insertWhereTemplates, datasource);

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL DELETE DATA operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteDataOperation(RDFDeleteDataOperation deleteDataOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult()
            {
                DeleteResults = PopulateDeleteOperationResults(deleteDataOperation.DeleteTemplates, datasource)
            };
            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL DELETE WHERE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteWhereOperation(RDFDeleteWhereOperation deleteWhereOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();

            //Execute the CONSTRUCT query for materialization of the operation templates
            RDFConstructQueryResult constructResult = ExecuteConstructQueryFromOperation(deleteWhereOperation, datasource);

            //Use materialized templates for execution of the operation
            List<RDFPattern> deleteWhereTemplates = new List<RDFPattern>();
            if (datasource.IsGraph())
            {
                RDFGraph deleteWhereGraph = RDFGraph.FromDataTable(constructResult.ConstructResults);
                foreach (RDFTriple deleteWhereTriple in deleteWhereGraph)
                    deleteWhereTemplates.Add(new RDFPattern(deleteWhereTriple.Subject, deleteWhereTriple.Predicate, deleteWhereTriple.Object));
            }
            else if (datasource.IsStore())
            {
                RDFMemoryStore deleteWhereStore = RDFMemoryStore.FromDataTable(constructResult.ConstructResults);
                foreach (RDFQuadruple deleteWhereQuadruple in deleteWhereStore)
                    deleteWhereTemplates.Add(new RDFPattern(deleteWhereQuadruple.Context, deleteWhereQuadruple.Subject, deleteWhereQuadruple.Predicate, deleteWhereQuadruple.Object));
            }
            operationResult.DeleteResults = PopulateDeleteOperationResults(deleteWhereTemplates, datasource);

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given operation on the given SPARQL UPDATE endpoint
        /// </summary>
        internal bool EvaluateOperationOnSPARQLUpdateEndpoint(RDFOperation operation, RDFSPARQLEndpoint sparqlUpdateEndpoint, RDFSPARQLEndpointOperationOptions sparqlUpdateEndpointOperationOptions)
        {
            bool opResult = false;

            if (sparqlUpdateEndpointOperationOptions == null)
                sparqlUpdateEndpointOperationOptions = new RDFSPARQLEndpointOperationOptions();

            //Establish a connection to the given SPARQL UPDATE endpoint (30s timeout)
            using (RDFWebClient webClient = new RDFWebClient(30000))
            {
                //Insert user-provided parameters
                webClient.QueryString.Add(sparqlUpdateEndpoint.QueryParams);

                //Insert request headers
                string operationString = operation.ToString();
                if (sparqlUpdateEndpointOperationOptions.RequestContentType == RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded)
                {
                    operationString = string.Concat("update=", HttpUtility.UrlEncode(operation.ToString()));
                    webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                }
                else
                { 
                    webClient.Headers.Add(HttpRequestHeader.ContentType, "application/sparql-update");
                }

                //Send operation to SPARQL UPDATE endpoint
                string sparqlUpdateResponse = default;
                try
                {
                    sparqlUpdateResponse = webClient.UploadString(sparqlUpdateEndpoint.BaseAddress, operationString);
                    opResult = true;
                }
                catch (Exception ex)
                {
                    throw new RDFQueryException($"Operation on SPARQL UPDATE endpoint {sparqlUpdateEndpoint.BaseAddress} failed because: {ex.Message}; Endpoint's response was: {sparqlUpdateResponse}", ex);
                }
            }

            return opResult;
        }

        /// <summary>
        /// Executes the CONSTRUCT query for materialization of the operation templates
        /// </summary>
        private RDFConstructQueryResult ExecuteConstructQueryFromOperation(RDFOperation operation, RDFDataSource datasource)
        {
            //Inject SPARQL values within every evaluable member
            operation.InjectValues(operation.GetValues());

            DataTable resultTable = new DataTable();
            RDFConstructQueryResult constructResult = new RDFConstructQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = operation.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Any())
            {
                //Iterate the evaluable members of the operation
                EvaluateQueryMembers(operation, evaluableQueryMembers, datasource);

                //Get the result table of the operation
                resultTable = CombineTables(QueryMemberFinalResultTables.Values.ToList(), false);
            }

            //Fill the templates from the result table
            DataTable filledResultTable = FillTemplates(operation.IsDeleteData || operation.IsDeleteWhere ? operation.DeleteTemplates : operation.InsertTemplates, resultTable, datasource.IsStore());

            //Apply the modifiers of the query to the result table
            constructResult.ConstructResults = ApplyModifiers(operation, filledResultTable);

            constructResult.ConstructResults.TableName = "CONSTRUCT_RESULTS";
            return constructResult;
        }

        /// <summary>
        /// Populates a datatble with data from the given INSERT templates
        /// </summary>
        private DataTable PopulateInsertOperationResults(List<RDFPattern> insertDataTemplates, RDFDataSource datasource)
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
        private DataTable PopulateDeleteOperationResults(List<RDFPattern> deleteDataTemplates, RDFDataSource datasource)
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