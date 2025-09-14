/*
   Copyright 2012-2025 Marco De Salvo

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

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFPatternConstraint represents a SHACL constraint on the specified regular expression for a given RDF term
    /// </summary>
    public sealed class RDFPatternConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Regular Expression to be applied on the given RDF term
        /// </summary>
        public Regex RegEx { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a pattern constraint with the given regex
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        public RDFPatternConstraint(Regex regex)
            => RegEx = regex ?? throw new RDFModelException("Cannot create RDFPatternConstraint because given \"regex\" parameter is null.");
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();
            RDFPropertyShape pShape = shape as RDFPropertyShape;

            //In case no shape messages have been provided, this constraint emits a default one (for usability)
            List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
            if (shapeMessages.Count == 0)
                shapeMessages.Add(new RDFPlainLiteral($"Must match expression {RegEx} and can't be a blank node"));

            #region Evaluation
            foreach (RDFPatternMember valueNode in valueNodes)
                switch (valueNode)
                {
                    //Resource
                    case RDFResource valueNodeResource:
                        if (valueNodeResource.IsBlank || !RegEx.IsMatch(valueNodeResource.ToString()))
                        {
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     pShape?.Path,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        }
                        break;

                    //Literal
                    case RDFLiteral valueNodeLiteral:
                        if (!RegEx.IsMatch(valueNodeLiteral.Value))
                        {
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT,
                                                                     focusNode,
                                                                     pShape?.Path,
                                                                     valueNode,
                                                                     shapeMessages,
                                                                     shape.Severity));
                        }
                        break;
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
                //sh:pattern
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.PATTERN, new RDFTypedLiteral(RegEx.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING)));

                //sh:flags
                StringBuilder regexFlags = new StringBuilder(4);
                if (RegEx.Options.HasFlag(RegexOptions.IgnoreCase))
                    regexFlags.Append('i');
                if (RegEx.Options.HasFlag(RegexOptions.Singleline))
                    regexFlags.Append('s');
                if (RegEx.Options.HasFlag(RegexOptions.Multiline))
                    regexFlags.Append('m');
                if (RegEx.Options.HasFlag(RegexOptions.IgnorePatternWhitespace))
                    regexFlags.Append('x');
                if (regexFlags.Length > 0)
                    result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.FLAGS, new RDFTypedLiteral(regexFlags.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING)));
            }
            return result;
        }
        #endregion
    }
}