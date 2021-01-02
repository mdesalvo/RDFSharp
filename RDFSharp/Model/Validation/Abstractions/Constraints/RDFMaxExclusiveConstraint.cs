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
using System;
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFMaxExclusiveConstraint represents a SHACL constraint on an exclusive upper-bound value for a given RDF term
    /// </summary>
    public class RDFMaxExclusiveConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Exclusive upper-bound value required on the given RDF term
        /// </summary>
        public RDFPatternMember Value { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a maxExclusive constraint with the given resource value
        /// </summary>
        public RDFMaxExclusiveConstraint(RDFResource value) : base()
        {
            if (value != null)
            {
                this.Value = value;
            }
            else
            {
                throw new RDFModelException("Cannot create RDFMaxExclusiveConstraint because given \"value\" parameter is null.");
            }
        }

        /// <summary>
        /// Default-ctor to build a maxExclusive constraint with the given literal value
        /// </summary>
        public RDFMaxExclusiveConstraint(RDFLiteral value) : base()
        {
            if (value != null)
            {
                this.Value = value;
            }
            else
            {
                throw new RDFModelException("Cannot create RDFMaxExclusiveConstraint because given \"value\" parameter is null.");
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

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                int comparison = RDFQueryUtilities.CompareRDFPatternMembers(this.Value, valueNode);
                if (comparison == -99 || comparison <= 0)
                {
                    report.AddResult(new RDFValidationResult(shape,
                                                             RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT,
                                                             focusNode,
                                                             shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                             valueNode,
                                                             shape.Messages,
                                                             shape.Severity));
                }
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

                //sh:maxExclusive
                if (this.Value is RDFResource)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_EXCLUSIVE, (RDFResource)this.Value));
                else
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.MAX_EXCLUSIVE, (RDFLiteral)this.Value));

            }
            return result;
        }
        #endregion

    }
}