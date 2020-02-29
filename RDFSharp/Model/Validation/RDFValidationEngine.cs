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
        public static RDFValidationReport Validate(this RDFShapesGraph shapesGraph,
                                                   RDFGraph dataGraph) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            if (dataGraph != null) {
                foreach (RDFShape currentShape in shapesGraph.Where(s => !s.Deactivated)) {

                    //Get focus nodes of current shape
                    List<RDFResource> focusNodes = dataGraph.GetFocusNodesOf(currentShape);
                    foreach (RDFResource currentFocusNode in focusNodes) {

                        //Get value nodes of current focus node
                        List<RDFPatternMember> valueNodes = dataGraph.GetValueNodesOf(currentShape, currentFocusNode);
                        foreach (RDFPatternMember currentValueNode in valueNodes) {

                            //Evaluate constraints on current value node
                            foreach (RDFConstraint constraint in currentShape) {
                                report.MergeResults(constraint.Evaluate(shapesGraph, currentShape, dataGraph, currentFocusNode, currentValueNode, valueNodes));
                            }

                        }

                    }

                }
            }
            return report;
        }
        #endregion

    }
}