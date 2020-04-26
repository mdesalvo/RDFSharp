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
                foreach (RDFShape shape in shapesGraph)
                    report.MergeResults(shape.EvaluateShape(new RDFValidationContext(shapesGraph, dataGraph)));
            }
            return report;
        }

        /// <summary>
        /// Validates the given data graph against the given SHACL shape
        /// </summary>
        internal static RDFValidationReport EvaluateShape(this RDFShape shape, RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            if (!shape.Deactivated) {

                //Set current shape
                validationContext.Shape = shape;
             
                //Get focus nodes of current shape
                validationContext.FocusNodes = validationContext.DataGraph.GetFocusNodesOf(validationContext.Shape);

                //Get value nodes of each focus node
                validationContext.ValueNodes.Clear();
                validationContext.FocusNodes.ForEach(focusNode => {
                    if (!validationContext.ValueNodes.ContainsKey(focusNode.PatternMemberID))
                        validationContext.ValueNodes.Add(focusNode.PatternMemberID, validationContext.DataGraph.GetValueNodesOf(shape, focusNode));
                });

                //Evaluate constraints
                foreach (RDFConstraint currentConstraint in shape)
                    report.MergeResults(currentConstraint.Evaluate(validationContext));

            }
            return report;
        }

        /// <summary>
        /// Validates the given data graph against the given SHACL shape (preserves both focus nodes and values nodes)
        /// </summary>
        internal static RDFValidationReport EvaluateShapeWithFocusAndValuesPreservation(this RDFShape shape, RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            if (!shape.Deactivated) {

               //Evaluate constraints
                foreach (RDFConstraint currentConstraint in shape)
                    report.MergeResults(currentConstraint.Evaluate(validationContext));

            }
            return report;
        }

        /// <summary>
        /// Validates the given data graph against the given SHACL shape (preserves only focus nodes)
        /// </summary>
        internal static RDFValidationReport EvaluateShapeWithFocusPreservation(this RDFShape shape, RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            if (!shape.Deactivated) {

                //Set current shape
                validationContext.Shape = shape;

                //Get value nodes of each focus node
                validationContext.ValueNodes.Clear();
                validationContext.FocusNodes.ForEach(focusNode => {
                    if (!validationContext.ValueNodes.ContainsKey(focusNode.PatternMemberID))
                        validationContext.ValueNodes.Add(focusNode.PatternMemberID, validationContext.DataGraph.GetValueNodesOf(shape, focusNode));
                });

                //Evaluate constraints
                foreach (RDFConstraint currentConstraint in shape)
                    report.MergeResults(currentConstraint.Evaluate(validationContext));

            }
            return report;
        }
        #endregion

    }
}