/*
   Copyright 2012-2022 Marco De Salvo

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
    /// RDFMinInclusiveConstraint represents a SHACL constraint on an inclusive lower-bound value for a given RDF term
    /// </summary>
    public class RDFMinInclusiveConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Inclusive lower-bound value required on the given RDF term
        /// </summary>
        public RDFPatternMember Value { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a minInclusive constraint with the given resource value
        /// </summary>
        public RDFMinInclusiveConstraint(RDFResource value)
        {
            if (value == null)
                throw new RDFModelException("Cannot create RDFMinInclusiveConstraint because given \"value\" parameter is null.");
            
            this.Value = value;
        }

        /// <summary>
        /// Default-ctor to build a minInclusive constraint with the given literal value
        /// </summary>
        public RDFMinInclusiveConstraint(RDFLiteral value)
        {
            if (value == null)
                throw new RDFModelException("Cannot create RDFMinInclusiveConstraint because given \"value\" parameter is null.");
            
            this.Value = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Must have values greater or equal than <{this.Value}>"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                int comparison = RDFQueryUtilities.CompareRDFPatternMembers(this.Value, valueNode);
                if (comparison == -99 || comparison > 0)
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.MIN_INCLUSIVE_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                             valueNode,
                                                             shapeMessages,
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
                //sh:minInclusive
                if (this.Value is RDFResource)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MIN_INCLUSIVE, (RDFResource)this.Value));
                else
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MIN_INCLUSIVE, (RDFLiteral)this.Value));
            }
            return result;
        }
        #endregion
    }
}
