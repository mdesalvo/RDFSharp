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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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
        /// Evaluates the given SPARQL UPDATE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateOperationOnGraphOrStore(RDFOperation operation, RDFDataSource datasource)
        {
            RDFOperationResult result = new RDFOperationResult();

            if (operation is RDFDeleteDataOperation deleteDataOperation)
                result = EvaluateDeleteDataOperation(deleteDataOperation, datasource);
            else if (operation is RDFDeleteWhereOperation deleteWhereOperation)
                result = EvaluateDeleteWhereOperation(deleteWhereOperation, datasource);
            else if (operation is RDFInsertDataOperation insertDataOperation)
                result = EvaluateInsertDataOperation(insertDataOperation, datasource);
            else if (operation is RDFInsertWhereOperation insertWhereOperation)
                result = EvaluateInsertWhereOperation(insertWhereOperation, datasource);
            else if (operation is RDFDeleteInsertWhereOperation deleteInsertWhereOperation)
                result = EvaluateDeleteInsertWhereOperation(deleteInsertWhereOperation, datasource);
            else if (operation is RDFLoadOperation loadOperation)
                result = EvaluateLoadOperation(loadOperation, datasource);
            else if (operation is RDFClearOperation clearOperation)
                result = EvaluateClearOperation(clearOperation, datasource);

            return result;
        }

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
            List<RDFPattern> insertTemplates = new List<RDFPattern>();
            if (datasource.IsGraph())
            {
                RDFGraph insertGraph = RDFGraph.FromDataTable(constructResult.ConstructResults);
                foreach (RDFTriple insertTriple in insertGraph)
                    insertTemplates.Add(new RDFPattern(insertTriple.Subject, insertTriple.Predicate, insertTriple.Object));
            }
            else if (datasource.IsStore())
            {
                RDFMemoryStore insertStore = RDFMemoryStore.FromDataTable(constructResult.ConstructResults);
                foreach (RDFQuadruple insertQuadruple in insertStore)
                    insertTemplates.Add(new RDFPattern(insertQuadruple.Context, insertQuadruple.Subject, insertQuadruple.Predicate, insertQuadruple.Object));
            }
            operationResult.InsertResults = PopulateInsertOperationResults(insertTemplates, datasource);

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
            List<RDFPattern> deleteTemplates = new List<RDFPattern>();
            if (datasource.IsGraph())
            {
                RDFGraph deleteGraph = RDFGraph.FromDataTable(constructResult.ConstructResults);
                foreach (RDFTriple deleteTriple in deleteGraph)
                    deleteTemplates.Add(new RDFPattern(deleteTriple.Subject, deleteTriple.Predicate, deleteTriple.Object));
            }
            else if (datasource.IsStore())
            {
                RDFMemoryStore deleteStore = RDFMemoryStore.FromDataTable(constructResult.ConstructResults);
                foreach (RDFQuadruple deleteQuadruple in deleteStore)
                    deleteTemplates.Add(new RDFPattern(deleteQuadruple.Context, deleteQuadruple.Subject, deleteQuadruple.Predicate, deleteQuadruple.Object));
            }
            operationResult.DeleteResults = PopulateDeleteOperationResults(deleteTemplates, datasource);

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL DELETE/INSERT WHERE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteInsertWhereOperation(RDFDeleteInsertWhereOperation deleteInsertWhereOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();

            //Execute the CONSTRUCT query for materialization of the operation templates
            RDFConstructQueryResult constructDeleteResult = new RDFOperationEngine().ExecuteConstructQueryFromOperation(deleteInsertWhereOperation, datasource, "DELETE");
            RDFConstructQueryResult constructInsertResult = new RDFOperationEngine().ExecuteConstructQueryFromOperation(deleteInsertWhereOperation, datasource, "INSERT");

            //Use materialized templates for execution of the operation
            List<RDFPattern> deleteTemplates = new List<RDFPattern>();
            List<RDFPattern> insertTemplates = new List<RDFPattern>();
            if (datasource.IsGraph())
            {
                RDFGraph deleteGraph = RDFGraph.FromDataTable(constructDeleteResult.ConstructResults);
                foreach (RDFTriple deleteTriple in deleteGraph)
                    deleteTemplates.Add(new RDFPattern(deleteTriple.Subject, deleteTriple.Predicate, deleteTriple.Object));

                RDFGraph insertGraph = RDFGraph.FromDataTable(constructInsertResult.ConstructResults);
                foreach (RDFTriple insertTriple in insertGraph)
                    insertTemplates.Add(new RDFPattern(insertTriple.Subject, insertTriple.Predicate, insertTriple.Object));
            }
            else if (datasource.IsStore())
            {
                RDFMemoryStore deleteStore = RDFMemoryStore.FromDataTable(constructDeleteResult.ConstructResults);
                foreach (RDFQuadruple deleteQuadruple in deleteStore)
                    deleteTemplates.Add(new RDFPattern(deleteQuadruple.Context, deleteQuadruple.Subject, deleteQuadruple.Predicate, deleteQuadruple.Object));

                RDFMemoryStore insertStore = RDFMemoryStore.FromDataTable(constructInsertResult.ConstructResults);
                foreach (RDFQuadruple insertQuadruple in insertStore)
                    insertTemplates.Add(new RDFPattern(insertQuadruple.Context, insertQuadruple.Subject, insertQuadruple.Predicate, insertQuadruple.Object));
            }
            operationResult.DeleteResults = PopulateDeleteOperationResults(deleteTemplates, datasource);
            operationResult.InsertResults = PopulateInsertOperationResults(insertTemplates, datasource);

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL LOAD operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateLoadOperation(RDFLoadOperation loadOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();

            try
            {
                //Dereference graph Uri in order to try fetching RDF data
                RDFGraph insertGraph = RDFGraph.FromUri(loadOperation.FromContext);

                //Use fetched RDF data for execution of the operation
                List<RDFPattern> insertTemplates = new List<RDFPattern>();
                if (datasource.IsGraph())
                {
                    foreach (RDFTriple insertTriple in insertGraph)
                        insertTemplates.Add(new RDFPattern(insertTriple.Subject, insertTriple.Predicate, insertTriple.Object));
                }
                else if (datasource.IsStore())
                {
                    RDFContext context = new RDFContext(loadOperation.ToContext ?? RDFNamespaceRegister.DefaultNamespace.NamespaceUri);
                    foreach (RDFTriple insertTriple in insertGraph)
                        insertTemplates.Add(new RDFPattern(context, insertTriple.Subject, insertTriple.Predicate, insertTriple.Object)); 
                }
                operationResult.InsertResults = PopulateInsertOperationResults(insertTemplates, datasource);
            }
            catch
            {
                //In case the operation is silent, the exception must be suppressed
                if (!loadOperation.IsSilent)
                    throw;
            }

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL CLEAR operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateClearOperation(RDFClearOperation clearOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();

            //Graphs => automatically execute as "CLEAR ALL" (since they are contextless by design)
            if (datasource.IsGraph())
            {
                operationResult.DeleteResults = ((RDFGraph)datasource).ToDataTable();
                ((RDFGraph)datasource).ClearTriples();
            }

            //Stores => transform into a targeted "DELETE WHERE"
            else if (datasource.IsStore())
            {
                try
                {
                    RDFDeleteWhereOperation deleteWhereOperation = new RDFDeleteWhereOperation();

                    //Explicit => delete quadruples having the given context
                    if (clearOperation.FromContext != null)
                    {
                        deleteWhereOperation
                            .AddPatternGroup(new RDFPatternGroup("PG1")
                                .AddPattern(new RDFPattern(new RDFContext(clearOperation.FromContext), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O"))))
                            .AddDeleteNonGroundTemplate<RDFDeleteWhereOperation>(new RDFPattern(new RDFContext(clearOperation.FromContext), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")));
                    }

                    //Implicit => delete quadruples according to the given operation flavor
                    else
                    {
                        switch (clearOperation.OperationFlavor)
                        {
                            //Default => delete quadruples having the default namespace as context
                            case RDFQueryEnums.RDFClearOperationFlavor.DEFAULT:
                                deleteWhereOperation
                                    .AddPatternGroup(new RDFPatternGroup("PG1")
                                        .AddPattern(new RDFPattern(new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O"))))
                                    .AddDeleteNonGroundTemplate<RDFDeleteWhereOperation>(new RDFPattern(new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")));
                                break;

                            //Default => delete quadruples NOT having the default namespace as context
                            case RDFQueryEnums.RDFClearOperationFlavor.NAMED:
                                deleteWhereOperation
                                    .AddPatternGroup(new RDFPatternGroup("PG1")
                                        .AddPattern(new RDFPattern(new RDFVariable("C"), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")))
                                        .AddFilter(new RDFBooleanNotFilter(new RDFSameTermFilter(new RDFVariable("C"), new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri)))))
                                    .AddDeleteNonGroundTemplate<RDFDeleteWhereOperation>(new RDFPattern(new RDFVariable("C"), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")));
                                break;

                            //Default => delete all quadruples
                            case RDFQueryEnums.RDFClearOperationFlavor.ALL:
                                deleteWhereOperation
                                    .AddPatternGroup(new RDFPatternGroup("PG1")
                                        .AddPattern(new RDFPattern(new RDFVariable("C"), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O"))))
                                    .AddDeleteNonGroundTemplate<RDFDeleteWhereOperation>(new RDFPattern(new RDFVariable("C"), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")));
                                break;
                        }
                    }

                    operationResult = EvaluateOperationOnGraphOrStore(deleteWhereOperation, datasource);
                }
                catch
                {
                    //In case the operation is silent, the exception must be suppressed
                    if (!clearOperation.IsSilent)
                        throw;
                }
            }

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given operation on the given SPARQL UPDATE endpoint
        /// </summary>
        internal bool EvaluateOperationOnSPARQLUpdateEndpoint(RDFOperation operation, RDFSPARQLEndpoint sparqlUpdateEndpoint, RDFSPARQLEndpointOperationOptions sparqlUpdateEndpointOperationOptions)
        {
            bool opResult = false;

            //Initialize operation options if not provided
            if (sparqlUpdateEndpointOperationOptions == null)
                sparqlUpdateEndpointOperationOptions = new RDFSPARQLEndpointOperationOptions();

            //Establish a connection to the given SPARQL UPDATE endpoint (60s timeout)
            using (RDFWebClient webClient = new RDFWebClient(60000))
            {
                //Parse user-provided parameters
                string defaultGraphUri = sparqlUpdateEndpoint.QueryParams.Get("default-graph-uri");
                string namedGraphUri = sparqlUpdateEndpoint.QueryParams.Get("named-graph-uri");

                //Insert request headers
                string operationString = operation.ToString();
                switch (sparqlUpdateEndpointOperationOptions.RequestContentType)
                {
                    //update via POST with URL-encoded parameters
                    case RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded:
                        webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                        operationString = string.Concat("update=", HttpUtility.UrlEncode(operationString));
                        //Handle user-provided parameters
                        if (!string.IsNullOrEmpty(defaultGraphUri))
                            operationString = string.Concat("using-graph-uri=", HttpUtility.UrlEncode(defaultGraphUri), "&", operationString);
                        if (!string.IsNullOrEmpty(namedGraphUri))
                            operationString = string.Concat("using-named-graph-uri=", HttpUtility.UrlEncode(namedGraphUri), "&", operationString);                        
                        break;

                    //update via POST directly
                    case RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.Sparql_Update:
                        webClient.Headers.Add(HttpRequestHeader.ContentType, "application/sparql-update");
                        //Handle user-provided parameters
                        if (!string.IsNullOrEmpty(defaultGraphUri))
                            webClient.QueryString.Add("using-graph-uri", defaultGraphUri);
                        if (!string.IsNullOrEmpty(namedGraphUri))
                            webClient.QueryString.Add("using-named-graph-uri", namedGraphUri);
                        break;
                }

                //Send operation to SPARQL UPDATE endpoint
                string sparqlUpdateResponse = default;
                try
                {
                    sparqlUpdateResponse = webClient.UploadString(sparqlUpdateEndpoint.BaseAddress, operationString);

                    //We assume that (by design) the SPARQL UPDATE endpoint should raise an exception in case of operation failure
                    opResult = true;
                }
                catch (Exception ex)
                {
                    //Silent operations can hide errors to the application
                    bool isLoadSilent = operation is RDFLoadOperation loadOperation && loadOperation.IsSilent;
                    bool isClearSilent = operation is RDFClearOperation clearOperation && clearOperation.IsSilent;
                    if (isLoadSilent || isClearSilent)
                        return opResult;

                    throw new RDFQueryException($"Operation on SPARQL UPDATE endpoint {sparqlUpdateEndpoint.BaseAddress} failed because: {ex.Message}; Endpoint's response was: {sparqlUpdateResponse}", ex);
                }
            }

            return opResult;
        }

        /// <summary>
        /// Executes the CONSTRUCT query for materialization of the operation templates
        /// </summary>
        private RDFConstructQueryResult ExecuteConstructQueryFromOperation(RDFOperation operation, RDFDataSource datasource, string deleteInsertCommand=null)
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
            DataTable filledResultTable;
            switch (deleteInsertCommand)
            {
                case "DELETE":
                    filledResultTable = FillTemplates(operation.DeleteTemplates, resultTable, datasource.IsStore());
                    break;

                case "INSERT":
                    filledResultTable = FillTemplates(operation.InsertTemplates, resultTable, datasource.IsStore());
                    break;

                default:
                    filledResultTable = FillTemplates(operation is RDFDeleteDataOperation || operation is RDFDeleteWhereOperation ? operation.DeleteTemplates : operation.InsertTemplates, resultTable, datasource.IsStore());
                    break;
            }

            //Apply the modifiers of the query to the result table
            constructResult.ConstructResults = ApplyModifiers(operation, filledResultTable);

            constructResult.ConstructResults.TableName = operation.ToString();
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