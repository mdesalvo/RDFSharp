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
    /// RDFHasValueConstraint represents a SHACL constraint on a required value for a given RDF term
    /// </summary>
    public class RDFHasValueConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Value required on the given RDF term
        /// </summary>
        public RDFPatternMember Value { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a hasValue constraint with the given resource value
        /// </summary>
        public RDFHasValueConstraint(RDFResource value) : base()
        {
            if (value != null)
            {
                this.Value = value;
            }
            else
            {
                throw new RDFModelException("Cannot create RDFHasValueConstraint because given \"value\" parameter is null.");
            }
        }

        /// <summary>
        /// Default-ctor to build a hasValue constraint with the given literal value
        /// </summary>
        public RDFHasValueConstraint(RDFLiteral value) : base()
        {
            if (value != null)
            {
                this.Value = value;
            }
            else
            {
                throw new RDFModelException("Cannot create RDFHasValueConstraint because given \"value\" parameter is null.");
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
            if (!valueNodes.Any(v => v.Equals(this.Value)))
            {
                report.AddResult(new RDFValidationResult(shape,
                                                         RDFVocabulary.SHACL.HAS_VALUE_CONSTRAINT_COMPONENT,
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

                //sh:hasValue
                if (this.Value is RDFResource)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.HAS_VALUE, (RDFResource)this.Value));
                else
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.HAS_VALUE, (RDFLiteral)this.Value));

            }
            return result;
        }
        #endregion

    }
}