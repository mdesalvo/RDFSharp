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
    /// RDFDatatypeConstraint represents a SHACL constraint on the specified datatype for a given RDF term
    /// </summary>
    public class RDFDatatypeConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Allowed datatype for the given RDF term
        /// </summary>
        public RDFModelEnums.RDFDatatypes Datatype { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a datatype constraint with the given datatype
        /// </summary>
        public RDFDatatypeConstraint(RDFModelEnums.RDFDatatypes datatype)
			=> this.Datatype = datatype;
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
                shapeMessages.Add(new RDFPlainLiteral($"Value must be a valid literal of type <{RDFModelUtilities.GetDatatypeFromEnum(this.Datatype)}>"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
            {
                switch (valueNode)
                {
                    //Resource
                    case RDFResource _:
                        report.AddResult(new RDFValidationResult(shape,
                                                                 RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT,
                                                                 focusNode,
                                                                 shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                 valueNode,
                                                                 shapeMessages,
                                                                 shape.Severity));
                        break;

                    //PlainLiteral
                    case RDFPlainLiteral valueNodePlainLiteral:
                        if (this.Datatype != RDFModelEnums.RDFDatatypes.XSD_STRING || valueNodePlainLiteral.HasLanguage())
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        break;

                    //TypedLiteral
                    case RDFTypedLiteral valueNodeTypedLiteral:
                        if (this.Datatype != valueNodeTypedLiteral.Datatype)
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        break;
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
                //sh:datatype
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.DATATYPE, new RDFResource(RDFModelUtilities.GetDatatypeFromEnum(this.Datatype))));
            }
            return result;
        }
        #endregion
    }
}