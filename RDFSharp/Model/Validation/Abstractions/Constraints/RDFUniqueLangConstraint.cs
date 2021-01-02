﻿/*
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
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFUniqueLangConstraint represents a SHACL constraint on the uniqueness of language tags for a given RDF term
    /// </summary>
    public class RDFUniqueLangConstraint : RDFConstraint
    {

        #region Properties
        /// <summary>
        /// Flag indicating that uniqueness of language tags is required or not
        /// </summary>
        public bool UniqueLang { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a uniqueLang constraint with the given behavior
        /// </summary>
        public RDFUniqueLangConstraint(bool uniqueLang) : base()
        {
            this.UniqueLang = uniqueLang;
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
            if (this.UniqueLang)
            {
                HashSet<string> reportedLangs = new HashSet<string>();
                List<RDFPlainLiteral> langlitValueNodes = valueNodes.OfType<RDFPlainLiteral>()
                                                                    .Where(vn => !string.IsNullOrEmpty(vn.Language))
                                                                    .ToList();

                foreach (RDFPlainLiteral innerlanglitValueNode in langlitValueNodes)
                {
                    foreach (RDFPlainLiteral outerlanglitValueNode in langlitValueNodes)
                    {
                        if (!innerlanglitValueNode.Equals(outerlanglitValueNode)
                                && innerlanglitValueNode.Language.Equals(outerlanglitValueNode.Language))
                        {

                            //Ensure to not report twice the same language tag
                            if (!reportedLangs.Contains(innerlanglitValueNode.Language))
                            {
                                reportedLangs.Add(innerlanglitValueNode.Language);
                                report.AddResult(new RDFValidationResult(shape,
                                                                         RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT,
                                                                         focusNode,
                                                                         shape is RDFPropertyShape ? ((RDFPropertyShape)shape).Path : null,
                                                                         null,
                                                                         shape.Messages,
                                                                         shape.Severity));
                            }

                        }
                    }
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

                //sh:uniqueLang
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.UNIQUE_LANG, new RDFTypedLiteral(this.UniqueLang.ToString(), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)));

            }
            return result;
        }
        #endregion

    }
}