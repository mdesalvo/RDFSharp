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

using System.Collections.Generic;
using System.Data;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFSPARQLConstraint represents a SHACL-SPARQL constraint (sh:SPARQLConstraint): for each focus node
    /// it runs a self-contained SELECT query (carrying its own PREFIX prologue) over the data graph, having
    /// pre-bound "?this" to the focus node. Every returned solution is reported as a validation result, where
    /// the optional "?value"/"?path"/"?message" projections feed the value, path and message of the result.
    /// </summary>
    public sealed class RDFSPARQLConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// The SELECT query whose solutions (one per row) describe the violations of the focus node (sh:select).
        /// It must project "?this"; it may optionally project "?value", "?path" and "?message".
        /// </summary>
        public RDFSelectQuery SelectQuery { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a SPARQL constraint driven by the given SELECT query (each returned solution is a violation)
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        public RDFSPARQLConstraint(RDFSelectQuery selectQuery)
            => SelectQuery = selectQuery ?? throw new RDFModelException("Cannot create RDFSPARQLConstraint because given \"selectQuery\" parameter is null.");
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            //SHACL-SPARQL constraints validate per focus node (the value nodes are irrelevant here)
            if (focusNode is RDFResource focusNodeResource)
            {
                //Pre-bind "?this" to the current focus node by injecting a trailing "VALUES ?this { <focus> }":
                //this scopes the SELECT solutions to the focus node being validated (reset for each focus node)
                SelectQuery.SetValues(new RDFValues().AddColumn(new RDFVariable("?this"), new List<RDFPatternMember> { focusNodeResource }));

                //In case no shape messages have been provided, this constraint emits a default one (for usability)
                List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
                if (shapeMessages.Count == 0)
                    shapeMessages.Add(new RDFPlainLiteral("Focus node did not satisfy the given SHACL-SPARQL constraint"));

                #region Evaluation
                //Each returned solution describes a violation of the focus node
                DataTable constraintResults = SelectQuery.ApplyToGraph(dataGraph).SelectResults;
                foreach (DataRow constraintResult in constraintResults.Rows)
                {
                    //sh:value (optional): the value which caused the violation
                    RDFPatternMember resultValue = null;
                    if (constraintResults.Columns.Contains("?VALUE") && !constraintResult.IsNull("?VALUE"))
                        resultValue = RDFQueryUtilities.ParseRDFPatternMember(constraintResult["?VALUE"].ToString());

                    //sh:resultPath (optional): the path which caused the violation (only resources are legal)
                    RDFResource resultPath = null;
                    if (constraintResults.Columns.Contains("?PATH") && !constraintResult.IsNull("?PATH")
                         && RDFQueryUtilities.ParseRDFPatternMember(constraintResult["?PATH"].ToString()) is RDFResource resultPathResource)
                        resultPath = resultPathResource;

                    //sh:resultMessage (optional): the per-solution message overrides the shape messages
                    List<RDFLiteral> resultMessages = shapeMessages;
                    if (constraintResults.Columns.Contains("?MESSAGE") && !constraintResult.IsNull("?MESSAGE")
                         && RDFQueryUtilities.ParseRDFPatternMember(constraintResult["?MESSAGE"].ToString()) is RDFLiteral resultMessageLiteral)
                        resultMessages = new List<RDFLiteral> { resultMessageLiteral };

                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.SPARQL_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             resultPath,
                                                             resultValue,
                                                             resultMessages,
                                                             shape.Severity));
                }
                #endregion

                //Release the per-focus-node pre-binding, so the carried query is left pristine for reuse/serialization
                SelectQuery.QueryValues = null;
            }

            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape)
        {
            RDFGraph result = new RDFGraph();

            if (shape != null)
            {
                //shape sh:sparql _:c
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.SPARQL, this));

                //_:c rdf:type sh:SPARQLConstraint
                result.AddTriple(new RDFTriple(this, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.SPARQL_CONSTRAINT));

                //_:c sh:select "<SelectQuery>"^^xsd:string
                result.AddTriple(new RDFTriple(this, RDFVocabulary.SHACL.SELECT, new RDFTypedLiteral(SelectQuery.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING)));
            }

            return result;
        }
        #endregion
    }
}