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
            RDFOperationResult operationResult = new RDFOperationResult();

            Dictionary<string, string> bindings = new Dictionary<string, string>();
            insertDataOperation.InsertTemplates.ForEach(insertTemplate =>
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
                        bindings.Add("?CONTEXT", ((RDFGraph)datasource).ToString());
                        bindings.Add("?SUBJECT", insertTriple.Subject.ToString());
                        bindings.Add("?PREDICATE", insertTriple.Predicate.ToString());
                        bindings.Add("?OBJECT", insertTriple.Object.ToString());
                        AddRow(operationResult.InsertResults, bindings);
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
                        AddRow(operationResult.InsertResults, bindings);
                        bindings.Clear();

                        //Add the quadruple to the store
                        ((RDFStore)datasource).AddQuadruple(insertQuadruple);
                    }
                }
            });

            return operationResult;
        }

        /// <summary>
        /// Evaluates the given SPARQL DELETE DATA operation on the given RDF datasource
        /// </summary>
        internal RDFOperationResult EvaluateDeleteDataOperation(RDFDeleteDataOperation deleteDataOperation, RDFDataSource datasource)
        {
            RDFOperationResult operationResult = new RDFOperationResult();

            Dictionary<string, string> bindings = new Dictionary<string, string>();
            deleteDataOperation.DeleteTemplates.ForEach(deleteTemplate =>
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
                        bindings.Add("?CONTEXT", ((RDFGraph)datasource).ToString());
                        bindings.Add("?SUBJECT", deleteTriple.Subject.ToString());
                        bindings.Add("?PREDICATE", deleteTriple.Predicate.ToString());
                        bindings.Add("?OBJECT", deleteTriple.Object.ToString());
                        AddRow(operationResult.DeleteResults, bindings);
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
                        AddRow(operationResult.DeleteResults, bindings);
                        bindings.Clear();

                        //Remove the quadruple from the store
                        ((RDFStore)datasource).RemoveQuadruple(deleteQuadruple);
                    }
                }
            });

            return operationResult;
        }

        #endregion

        #endregion
    }
}