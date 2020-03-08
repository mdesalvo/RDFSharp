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
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFValidationEngine analyzes a given data graph by applying the given SHACL shapes graph,
    /// in order to find error and inconsistency evidences affecting its structure.
    /// </summary>
    public static class RDFValidationEngine {

        #region Methods
        /// <summary>
        /// Validates the given data graph against the given SHACL shapes graph
        /// </summary>
        public static RDFValidationReport Validate(this RDFShapesGraph shapesGraph, RDFGraph dataGraph) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            if (dataGraph != null) {
                RDFValidationContext validationContext = new RDFValidationContext(shapesGraph, dataGraph);

                //Evaluate active shapes of shapes graph
                foreach (RDFShape shape in shapesGraph.Where(s => !s.Deactivated))
                    report.MergeResults(shape.EvaluateShape(validationContext));
            }
            return report;
        }

        /// <summary>
        /// Validates the given data graph against the given SHACL shape
        /// </summary>
        internal static RDFValidationReport EvaluateShape(this RDFShape shape, RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());

            //Set current shape
            validationContext.Shape = shape;

            //Get focus nodes of current shape
            validationContext.FocusNodes = validationContext.DataGraph.GetFocusNodesOf(shape);
            foreach (RDFResource focusNode in validationContext.FocusNodes) {

                //Set current focus node
                validationContext.FocusNode = focusNode;

                //Get value nodes of current focus node
                validationContext.ValueNodes = validationContext.DataGraph.GetValueNodesOf(shape, focusNode);
                
                //Evaluate constraints
                foreach (RDFConstraint currentConstraint in shape)
                    report.MergeResults(currentConstraint.Evaluate(validationContext));

            }

            return report;
        }
        #endregion

    }
}