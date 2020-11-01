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
    /// RDFMinCountConstraint represents a SHACL constraint on the minimum required occurrences for a given RDF term
    /// </summary>
    public class RDFMinCountConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Indicates the minimum required occurrences for a given RDF term
        /// </summary>
        public int MinCount { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a minCount constraint with the given minCount
        /// </summary>
        public RDFMinCountConstraint(int minCount) : base()
        {
            this.MinCount = minCount < 0 ? 0 : minCount;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            #region Evaluation
            if (valueNodes.Count < this.MinCount)
            {
                report.AddResult(new RDFValidationResult(shape,
                                                         RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT,
                                                         focusNode,
                                                         shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                         null,
                                                         shape.Messages,
                                                         shape.Severity));
            }
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

                //sh:minCount
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MIN_COUNT, new RDFTypedLiteral(this.MinCount.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

            }
            return result;
        }
        #endregion

    }
}