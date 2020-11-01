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

using RDFSharp.Query;
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFPropertyConstraint represents a SHACL constraint requiring the specified property shape for a given RDF term
    /// </summary>
    public class RDFPropertyConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Identifier of the property shape against which the given RDF term must be validated
        /// </summary>
        public RDFResource PropertyShapeUri { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a property constraint with the given property shape identifier
        /// </summary>
        public RDFPropertyConstraint(RDFResource propertyShapeUri) : base()
        {
            if (propertyShapeUri != null)
            {
                this.PropertyShapeUri = propertyShapeUri;
            }
            else
            {
                throw new RDFModelException("Cannot create RDFPropertyConstraint because given \"propertyShapeUri\" parameter is null.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            //Search for given property shape
            RDFPropertyShape propertyShape = shapesGraph.SelectShape(this.PropertyShapeUri.ToString()) as RDFPropertyShape;
            if (propertyShape == null)
                return report;

            #region Evaluation
            RDFValidationReport propertyShapeReport = RDFValidationEngine.ValidateShape(shapesGraph, dataGraph, propertyShape, valueNodes);
            if (!propertyShapeReport.Conforms)
                report.MergeResults(propertyShapeReport);
            #endregion

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

                //sh:property
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.PROPERTY, this.PropertyShapeUri));

            }
            return result;
        }
        #endregion

    }
}