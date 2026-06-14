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
using System.Linq;
using System.Net;
using System.Web;
using RDFSharp.Model;
using RDFSharp.Store;
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOperationEngine is the engine for execution of SPARQL UPDATE operations (MIRELLA)
    /// </summary>
    internal sealed class RDFOperationEngine : RDFQueryEngine
    {
        #region Methods
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
            =>  new RDFOperationResult
                {
                    InsertResults = PopulateInsertOperationResults(insertDataOperation.InsertTemplates, datasource)
                };

        /// <summary>
        /// Evaluates the given SPARQL INSERT WHERE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateInsertWhereOperation(RDFInsertWhereOperation insertWhereOperation, RDFDataSource datasource)
            =>  new RDFOperationResult
                {
                    //Materialize the operation templates from the WHERE result, then apply them
                    InsertResults = PopulateInsertOperationResults(
                                      MaterializeOperationTemplates(insertWhereOperation, datasource), datasource)
                };

        /// <summary>
        /// Evaluates the given SPARQL DELETE DATA operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteDataOperation(RDFDeleteDataOperation deleteDataOperation, RDFDataSource datasource)
            =>  new RDFOperationResult
                {
                    DeleteResults = PopulateDeleteOperationResults(deleteDataOperation.DeleteTemplates, datasource)
                };

        /// <summary>
        /// Evaluates the given SPARQL DELETE WHERE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteWhereOperation(RDFDeleteWhereOperation deleteWhereOperation, RDFDataSource datasource)
            =>  new RDFOperationResult
                {
                    //Materialize the operation templates from the WHERE result, then apply them
                    DeleteResults = PopulateDeleteOperationResults(
                                      MaterializeOperationTemplates(deleteWhereOperation, datasource), datasource)
                };

        /// <summary>
        /// Evaluates the given SPARQL DELETE/INSERT WHERE operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteInsertWhereOperation(RDFDeleteInsertWhereOperation deleteInsertWhereOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();

            //Materialize the DELETE and INSERT templates from the shared WHERE result
            //(a dedicated engine per side keeps their intermediate evaluation state isolated)
            List<RDFPattern> deleteTemplates = new RDFOperationEngine().MaterializeOperationTemplates(deleteInsertWhereOperation, datasource, "DELETE");
            List<RDFPattern> insertTemplates = new RDFOperationEngine().MaterializeOperationTemplates(deleteInsertWhereOperation, datasource, "INSERT");

            //Apply DELETE before INSERT (SPARQL UPDATE semantics)
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
                    insertTemplates.AddRange(RDFGraph.FromUri(loadOperation.FromContext)
                                                     .Select(loadTriple => new RDFPattern(loadTriple.Subject, loadTriple.Predicate, loadTriple.Object)));
                }

                //STORE => Dereference quadruples (respect the target context, if provided by the operation)
                else if (isStore)
                {
                    RDFContext targetContext = loadOperation.ToContext != null ? new RDFContext(loadOperation.ToContext) : null;
                    insertTemplates.AddRange(RDFMemoryStore.FromUri(loadOperation.FromContext)
                                                           .Select(loadQuadruple => new RDFPattern(targetContext ?? loadQuadruple.Context, loadQuadruple.Subject, loadQuadruple.Predicate, loadQuadruple.Object)));
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
        /// <exception cref="RDFQueryException"></exception>
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
        #endregion

        #region Utilities
        /// <summary>
        /// Materializes the operation templates by evaluating its WHERE clause and filling the chosen templates,
        /// returning them as a ready-to-apply list of RDFPattern.
        /// </summary>
        private List<RDFPattern> MaterializeOperationTemplates(RDFOperation operation, RDFDataSource datasource, string deleteInsertCommand = null)
        {
            //Evaluate the WHERE clause of the operation down to its result table
            RDFTable resultTable = new RDFTable();
            List<RDFQueryMember> evaluableQueryMembers = operation.GetEvaluableQueryMembers().ToList();
            if (evaluableQueryMembers.Count > 0)
            {
                EvaluateQueryMembers(evaluableQueryMembers, datasource);
                resultTable = RDFTableEngine.CombineTables(QueryMemberResultTables.Values.ToList());
            }

            //Pick the side of templates to materialize
            List<RDFPattern> templates;
            switch (deleteInsertCommand)
            {
                case "DELETE":
                    templates = operation.DeleteTemplates;
                    break;
                case "INSERT":
                    templates = operation.InsertTemplates;
                    break;
                default:
                    templates = operation is RDFDeleteDataOperation || operation is RDFDeleteWhereOperation ? operation.DeleteTemplates : operation.InsertTemplates;
                    break;
            }

            //Fill the templates from the WHERE result and run the operation modifiers, then build the patterns
            //straight from the resulting table (columns ?CONTEXT?/?SUBJECT/?PREDICATE/?OBJECT)
            bool needsContext = datasource.IsStore();
            RDFTable filledResultTable = ApplyModifiers(operation, FillTemplates(templates, resultTable, needsContext));
            return BuildPatternsFromTemplateTable(filledResultTable, needsContext);
        }

        /// <summary>
        /// Builds the list of patterns from a filled template table (the columns produced by FillTemplates),
        /// parsing each cell into its RDF term. Rows that could not yield a legal triple/quadruple are skipped
        /// (they cannot arise from FillTemplates, which already discards illegal templates).
        /// </summary>
        private static List<RDFPattern> BuildPatternsFromTemplateTable(RDFTable templateTable, bool needsContext)
        {
            int ctxOrdinal = needsContext ? templateTable.OrdinalOf("?CONTEXT") : -1;
            int subjOrdinal = templateTable.OrdinalOf("?SUBJECT");
            int predOrdinal = templateTable.OrdinalOf("?PREDICATE");
            int objOrdinal = templateTable.OrdinalOf("?OBJECT");
            RDFContext defaultContext = new RDFContext();

            List<RDFPattern> patterns = new List<RDFPattern>(templateTable.RowsCount);
            foreach (RDFTableRow row in templateTable.Rows)
            {
                //SUBJECT must be a resource, PREDICATE a non-blank resource, OBJECT a bound resource or literal
                string subjCell = subjOrdinal >= 0 ? row[subjOrdinal] : null;
                string predCell = predOrdinal >= 0 ? row[predOrdinal] : null;
                string objCell = objOrdinal >= 0 ? row[objOrdinal] : null;
                if (string.IsNullOrEmpty(subjCell) || string.IsNullOrEmpty(predCell) || objCell == null)
                    continue;
                if (!(ParseRDFPatternMember(subjCell) is RDFResource subj))
                    continue;
                if (!(ParseRDFPatternMember(predCell) is RDFResource pred) || pred.IsBlank)
                    continue;
                RDFPatternMember obj = ParseRDFPatternMember(objCell);

                if (needsContext)
                {
                    //CONTEXT defaults to the default namespace when unbound; a bound value must be a resource
                    string ctxCell = ctxOrdinal >= 0 ? row[ctxOrdinal] : null;
                    RDFContext context;
                    if (string.IsNullOrEmpty(ctxCell))
                        context = defaultContext;
                    else if (ParseRDFPatternMember(ctxCell) is RDFResource ctxRes && !ctxRes.IsBlank)
                        context = new RDFContext(ctxRes.ToString());
                    else
                        continue;

                    patterns.Add(obj is RDFResource objRes ? new RDFPattern(context, subj, pred, objRes)
                                                           : new RDFPattern(context, subj, pred, (RDFLiteral)obj));
                }
                else
                {
                    patterns.Add(obj is RDFResource objRes ? new RDFPattern(subj, pred, objRes)
                                                           : new RDFPattern(subj, pred, (RDFLiteral)obj));
                }
            }
            return patterns;
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
                        RDFTableEngine.AddRow(resultTable, bindings);
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
                        RDFTableEngine.AddRow(resultTable, bindings);
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
                        RDFTableEngine.AddRow(resultTable, bindings);
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
                        RDFTableEngine.AddRow(resultTable, bindings);
                        bindings.Clear();

                        //Remove the quadruple from the store
                        ((RDFStore)datasource).RemoveQuadruple(deleteQuadruple);
                    }
                }
            });

            return resultTable;
        }
        #endregion
    }
}