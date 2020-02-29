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
        public static RDFValidationReport Validate(this RDFShapesGraph shapesGraph,
                                                   RDFGraph dataGraph) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            if (dataGraph != null) {
                foreach (RDFShape shape in shapesGraph.Where(s => !s.Deactivated)) {
                    foreach (RDFResource focusNode in dataGraph.GetFocusNodesOf(shape)) {
                        foreach (RDFPatternMember valueNode in dataGraph.GetValueNodesOf(shape, focusNode)) {
                            report.MergeResults(shape.EvaluateShape(shapesGraph, dataGraph, focusNode, valueNode));
                        }
                    }
                }
            }
            return report;
        }

        /// <summary>
        /// Validates the given data graph against the given SHACL shape
        /// </summary>
        internal static RDFValidationReport EvaluateShape(this RDFShape shape,
                                                          RDFShapesGraph shapesGraph,
                                                          RDFGraph dataGraph,
                                                          RDFResource focusNode,
                                                          RDFPatternMember valueNode) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            if (dataGraph != null) {
                foreach (RDFConstraint constraint in shape) {
                    report.MergeResults(constraint.EvaluateConstraint(shapesGraph, shape, dataGraph, focusNode, valueNode));
                }
            }
            return report;
        }
        #endregion

    }
}