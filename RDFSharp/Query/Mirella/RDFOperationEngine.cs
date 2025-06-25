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
using System.Linq;
using System.Net;
using System.Web;
using RDFSharp.Model;
using RDFSharp.Store;
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOperationEngine is the engine for execution of SPARQL UPDATE operations
    /// </summary>
    internal sealed class RDFOperationEngine : RDFQueryEngine
    {
        #region Methods

        #region MIRELLA SPARQL UPDATE
        /// <summary>
        /// Evaluates the given SPARQL UPDATE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateOperationOnGraphOrStore(RDFOperation operation, RDFDataSource datasource)
        {
            switch (operation)
            {
                case RDFDeleteDataOperation deleteDataOperation:
                    return EvaluateDeleteDataOperation(deleteDataOperation, datasource);
                case RDFDeleteWhereOperation deleteWhereOperation:
                    return EvaluateDeleteWhereOperation(deleteWhereOperation, datasource);
                case RDFInsertDataOperation insertDataOperation:
                    return EvaluateInsertDataOperation(insertDataOperation, datasource);
                case RDFInsertWhereOperation insertWhereOperation:
                    return EvaluateInsertWhereOperation(insertWhereOperation, datasource);
                case RDFDeleteInsertWhereOperation deleteInsertWhereOperation:
                    return EvaluateDeleteInsertWhereOperation(deleteInsertWhereOperation, datasource);
                case RDFLoadOperation loadOperation:
                    return EvaluateLoadOperation(loadOperation, datasource);
                case RDFClearOperation clearOperation:
                    return EvaluateClearOperation(clearOperation, datasource);
            }
            return new RDFOperationResult();
        }

        /// <summary>
        /// Evaluates the given SPARQL INSERT DATA operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateInsertDataOperation(RDFInsertDataOperation insertDataOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult
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
            bool isGraph = datasource.IsGraph();
            bool isStore = datasource.IsStore();

            //Execute the CONSTRUCT query for materialization of the operation templates
            RDFConstructQueryResult constructResult = ExecuteConstructQueryFromOperation(insertWhereOperation, datasource);

            //Use materialized templates for execution of the operation
            List<RDFPattern> insertTemplates = new List<RDFPattern>();
            if (isGraph)
            {
                RDFGraph insertGraph = RDFGraph.FromDataTable(constructResult.ConstructResults);
                insertTemplates.AddRange(insertGraph.Select(insertTriple => new RDFPattern(insertTriple.Subject, insertTriple.Predicate, insertTriple.Object)));
            }
            else if (isStore)
            {
                RDFMemoryStore insertStore = RDFMemoryStore.FromDataTable(constructResult.ConstructResults);
                insertTemplates.AddRange(insertStore.Select(insertQuadruple => new RDFPattern(insertQuadruple.Context, insertQuadruple.Subject, insertQuadruple.Predicate, insertQuadruple.Object)));
            }
            operationResult.InsertResults = PopulateInsertOperationResults(insertTemplates, datasource);

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL DELETE DATA operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteDataOperation(RDFDeleteDataOperation deleteDataOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult
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
            bool isGraph = datasource.IsGraph();
            bool isStore = datasource.IsStore();

            //Execute the CONSTRUCT query for materialization of the operation templates
            RDFConstructQueryResult constructResult = ExecuteConstructQueryFromOperation(deleteWhereOperation, datasource);

            //Use materialized templates for execution of the operation
            List<RDFPattern> deleteTemplates = new List<RDFPattern>();
            if (isGraph)
            {
                RDFGraph deleteGraph = RDFGraph.FromDataTable(constructResult.ConstructResults);
                deleteTemplates.AddRange(deleteGraph.Select(deleteTriple => new RDFPattern(deleteTriple.Subject, deleteTriple.Predicate, deleteTriple.Object)));
            }
            else if (isStore)
            {
                RDFMemoryStore deleteStore = RDFMemoryStore.FromDataTable(constructResult.ConstructResults);
                deleteTemplates.AddRange(deleteStore.Select(deleteQuadruple => new RDFPattern(deleteQuadruple.Context, deleteQuadruple.Subject, deleteQuadruple.Predicate, deleteQuadruple.Object)));
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
            bool isGraph = datasource.IsGraph();
            bool isStore = datasource.IsStore();

            //Execute the CONSTRUCT query for materialization of the operation templates
            RDFConstructQueryResult constructDeleteResult = new RDFOperationEngine().ExecuteConstructQueryFromOperation(deleteInsertWhereOperation, datasource, "DELETE");
            RDFConstructQueryResult constructInsertResult = new RDFOperationEngine().ExecuteConstructQueryFromOperation(deleteInsertWhereOperation, datasource, "INSERT");

            //Use materialized templates for execution of the operation
            List<RDFPattern> deleteTemplates = new List<RDFPattern>();
            List<RDFPattern> insertTemplates = new List<RDFPattern>();
            if (isGraph)
            {
                RDFGraph deleteGraph = RDFGraph.FromDataTable(constructDeleteResult.ConstructResults);
                deleteTemplates.AddRange(deleteGraph.Select(deleteTriple => new RDFPattern(deleteTriple.Subject, deleteTriple.Predicate, deleteTriple.Object)));

                RDFGraph insertGraph = RDFGraph.FromDataTable(constructInsertResult.ConstructResults);
                insertTemplates.AddRange(insertGraph.Select(insertTriple => new RDFPattern(insertTriple.Subject, insertTriple.Predicate, insertTriple.Object)));
            }
            else if (isStore)
            {
                RDFMemoryStore deleteStore = RDFMemoryStore.FromDataTable(constructDeleteResult.ConstructResults);
                deleteTemplates.AddRange(deleteStore.Select(deleteQuadruple => new RDFPattern(deleteQuadruple.Context, deleteQuadruple.Subject, deleteQuadruple.Predicate, deleteQuadruple.Object)));

                RDFMemoryStore insertStore = RDFMemoryStore.FromDataTable(constructInsertResult.ConstructResults);
                insertTemplates.AddRange(insertStore.Select(insertQuadruple => new RDFPattern(insertQuadruple.Context, insertQuadruple.Subject, insertQuadruple.Predicate, insertQuadruple.Object)));
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
            bool isGraph = datasource.IsGraph();
            bool isStore = datasource.IsStore();

            try
            {
                List<RDFPattern> insertTemplates = new List<RDFPattern>();

                //GRAPH => Dereference triples
                if (isGraph)
                {
                    insertTemplates.AddRange(RDFGraph.FromUri(loadOperation.FromContext).Select(loadTriple => new RDFPattern(loadTriple.Subject, loadTriple.Predicate, loadTriple.Object)));
                }

                //STORE => Dereference quadruples (respect the target context, if provided by the operation)
                else if (isStore)
                {
                    RDFContext targetContext = loadOperation.ToContext != null ? new RDFContext(loadOperation.ToContext) : null;
                    insertTemplates.AddRange(RDFMemoryStore.FromUri(loadOperation.FromContext).Select(loadQuadruple => new RDFPattern(targetContext ?? loadQuadruple.Context, loadQuadruple.Subject, loadQuadruple.Predicate, loadQuadruple.Object)));
                }

                operationResult.InsertResults = PopulateInsertOperationResults(insertTemplates, datasource);
            }
            catch when (loadOperation.IsSilent)
            {
                //In case the operation is silent, the exception must be suppressed
            }

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL CLEAR operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateClearOperation(RDFClearOperation clearOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();
            bool isGraph = datasource.IsGraph();
            bool isStore = datasource.IsStore();

            //Graphs => automatically execute as "CLEAR ALL" (since they are contextless by design)
            if (isGraph)
            {
                operationResult.DeleteResults = ((RDFGraph)datasource).ToDataTable();
                ((RDFGraph)datasource).ClearTriples();
            }

            //Stores => transform into a targeted "DELETE WHERE"
            else if (isStore)
            {
                try
                {
                    RDFDeleteWhereOperation deleteWhereOperation = new RDFDeleteWhereOperation();

                    //Explicit => delete quadruples having the given context
                    if (clearOperation.FromContext != null)
                    {
                        deleteWhereOperation
                                                .AddPatternGroup(new RDFPatternGroup()
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
                                    .AddPatternGroup(new RDFPatternGroup()
                                        .AddPattern(new RDFPattern(new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O"))))
                                    .AddDeleteNonGroundTemplate<RDFDeleteWhereOperation>(new RDFPattern(new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")));
                                break;

                            //Default => delete quadruples NOT having the default namespace as context
                            case RDFQueryEnums.RDFClearOperationFlavor.NAMED:
                                deleteWhereOperation
                                    .AddPatternGroup(new RDFPatternGroup()
                                        .AddPattern(new RDFPattern(new RDFVariable("C"), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")))
                                        .AddFilter(new RDFBooleanNotFilter(new RDFExpressionFilter(new RDFSameTermExpression(new RDFVariable("C"), new RDFConstantExpression(new RDFResource(RDFNamespaceRegister.DefaultNamespace.NamespaceUri.ToString())))))))
                                    .AddDeleteNonGroundTemplate<RDFDeleteWhereOperation>(new RDFPattern(new RDFVariable("C"), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")));
                                break;

                            //Default => delete all quadruples
                            case RDFQueryEnums.RDFClearOperationFlavor.ALL:
                                deleteWhereOperation
                                    .AddPatternGroup(new RDFPatternGroup()
                                        .AddPattern(new RDFPattern(new RDFVariable("C"), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O"))))
                                    .AddDeleteNonGroundTemplate<RDFDeleteWhereOperation>(new RDFPattern(new RDFVariable("C"), new RDFVariable("S"), new RDFVariable("P"), new RDFVariable("O")));
                                break;
                        }
                    }

                    operationResult = EvaluateOperationOnGraphOrStore(deleteWhereOperation, datasource);
                }
                catch when (clearOperation.IsSilent)
                {
                    //In case the operation is silent, the exception must be suppressed
                }
            }

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given operation on the given SPARQL UPDATE endpoint
        /// </summary>
        internal bool EvaluateOperationOnSPARQLUpdateEndpoint(RDFOperation operation, RDFSPARQLEndpoint sparqlUpdateEndpoint, RDFSPARQLEndpointOperationOptions sparqlUpdateEndpointOperationOptions)
        {
            //Initialize operation options if not provided
            if (sparqlUpdateEndpointOperationOptions == null)
                sparqlUpdateEndpointOperationOptions = new RDFSPARQLEndpointOperationOptions();

            //Establish a connection to the given SPARQL UPDATE endpoint
            using (RDFWebClient webClient = new RDFWebClient(sparqlUpdateEndpointOperationOptions.TimeoutMilliseconds))
            {
                //Parse user-provided parameters
                string defaultGraphUri = sparqlUpdateEndpoint.QueryParams.Get("default-graph-uri");
                string namedGraphUri = sparqlUpdateEndpoint.QueryParams.Get("named-graph-uri");

                //Insert request headers
                string operationString = operation.ToString();
                switch (sparqlUpdateEndpointOperationOptions.RequestContentType)
                {
                    //update via POST with URL-encoded body
                    case RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded:
                        webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                        operationString = $"update={HttpUtility.UrlEncode(operationString)}";
                        //Handle user-provided parameters
                        if (!string.IsNullOrEmpty(defaultGraphUri))
                            operationString = $"using-graph-uri={HttpUtility.UrlEncode(defaultGraphUri)}&{operationString}";
                        if (!string.IsNullOrEmpty(namedGraphUri))
                            operationString = $"using-named-graph-uri={HttpUtility.UrlEncode(namedGraphUri)}&{operationString}";
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

                //Insert eventual authorization headers
                sparqlUpdateEndpoint.FillWebClientAuthorization(webClient);

                //Send operation to SPARQL UPDATE endpoint
                string sparqlUpdateResponse = null;
                try
                {
                    sparqlUpdateResponse = webClient.UploadString(sparqlUpdateEndpoint.BaseAddress, operationString);

                    //We assume that (by design) the SPARQL UPDATE endpoint should raise an exception in case of operation failure
                    return true;
                }
                catch (Exception ex)
                {
                    //Silent operations can hide errors to the application
                    bool isLoadSilent = operation is RDFLoadOperation loadOperation && loadOperation.IsSilent;
                    bool isClearSilent = operation is RDFClearOperation clearOperation && clearOperation.IsSilent;
                    if (isLoadSilent || isClearSilent)
                        return false;

                    throw new RDFQueryException($"Operation on SPARQL UPDATE endpoint {sparqlUpdateEndpoint.BaseAddress} failed because: {ex.Message}; Endpoint's response was: {sparqlUpdateResponse}", ex);
                }
            }
        }

        /// <summary>
        /// Executes the CONSTRUCT query for materialization of the operation templates
        /// </summary>
        private RDFConstructQueryResult ExecuteConstructQueryFromOperation(RDFOperation operation, RDFDataSource datasource, string deleteInsertCommand = null)
        {
            DataTable resultTable = new DataTable();

            RDFConstructQueryResult constructResult = new RDFConstructQueryResult();
            List<RDFQueryMember> evaluableQueryMembers = operation.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Count > 0)
            {
                //Iterate the evaluable members of the operation
                EvaluateQueryMembers(evaluableQueryMembers, datasource);

                //Get the result table of the operation
                resultTable = CombineTables(QueryMemberResultTables.Values.ToList());
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

            return constructResult;
        }

        /// <summary>
        /// Populates a datatble with data from the given INSERT templates
        /// </summary>
        private DataTable PopulateInsertOperationResults(List<RDFPattern> insertDataTemplates, RDFDataSource datasource)
        {
            DataTable resultTable = new DataTable("INSERT_RESULTS");
            bool isGraph = datasource.IsGraph();
            bool isStore = datasource.IsStore();
            if (isStore)
                resultTable.Columns.Add("?CONTEXT", typeof(string));
            resultTable.Columns.Add("?SUBJECT", typeof(string));
            resultTable.Columns.Add("?PREDICATE", typeof(string));
            resultTable.Columns.Add("?OBJECT", typeof(string));

            Dictionary<string, string> bindings = new Dictionary<string, string>();
            insertDataTemplates.ForEach(insertTemplate =>
            {
                //GRAPH
                if (isGraph)
                {
                    RDFTriple insertTriple = insertTemplate.Object is RDFLiteral litObj
                                                ? new RDFTriple((RDFResource)insertTemplate.Subject, (RDFResource)insertTemplate.Predicate, litObj)
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
                else if (isStore)
                {
                    RDFContext insertContext = insertTemplate.Context as RDFContext ?? new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri);
                    RDFQuadruple insertQuadruple = insertTemplate.Object is RDFLiteral litObj
                                                      ? new RDFQuadruple(insertContext, (RDFResource)insertTemplate.Subject, (RDFResource)insertTemplate.Predicate, litObj)
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
            bool isGraph = datasource.IsGraph();
            bool isStore = datasource.IsStore();
            if (isStore)
                resultTable.Columns.Add("?CONTEXT", typeof(string));
            resultTable.Columns.Add("?SUBJECT", typeof(string));
            resultTable.Columns.Add("?PREDICATE", typeof(string));
            resultTable.Columns.Add("?OBJECT", typeof(string));

            Dictionary<string, string> bindings = new Dictionary<string, string>();
            deleteDataTemplates.ForEach(deleteTemplate =>
            {
                //GRAPH
                if (isGraph)
                {
                    RDFTriple deleteTriple = deleteTemplate.Object is RDFLiteral litObj
                                                ? new RDFTriple((RDFResource)deleteTemplate.Subject, (RDFResource)deleteTemplate.Predicate, litObj)
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
                else if (isStore)
                {
                    RDFContext deleteContext = deleteTemplate.Context as RDFContext ?? new RDFContext(RDFNamespaceRegister.DefaultNamespace.NamespaceUri);
                    RDFQuadruple deleteQuadruple = deleteTemplate.Object is RDFLiteral litObj
                                                      ? new RDFQuadruple(deleteContext, (RDFResource)deleteTemplate.Subject, (RDFResource)deleteTemplate.Predicate, litObj)
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