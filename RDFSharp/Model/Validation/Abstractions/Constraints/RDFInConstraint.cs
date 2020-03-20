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
    /// RDFInConstraint represents a SHACL constraint on the allowed values for a given RDF term
    /// </summary>
    public class RDFInConstraint : RDFConstraint {

        #region Properties
        /// <summary>
        /// Values allowed on the given RDF term
        /// </summary>
        internal Dictionary<Int64, RDFPatternMember> InValues { get; set; }

        /// <summary>
        /// Type of the allowed values (Resource/Literal)
        /// </summary>
        internal RDFModelEnums.RDFItemTypes ItemType { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a in constraint of the given type (Resource/Literal)
        /// </summary>
        public RDFInConstraint(RDFModelEnums.RDFItemTypes itemType) : base() {
            this.InValues = new Dictionary<Int64, RDFPatternMember>();
            this.ItemType = itemType;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given resource to the values of this constraint (if ItemType has been set to Resource)
        /// </summary>
        public RDFInConstraint AddValue(RDFResource resource) {
            if (this.ItemType == RDFModelEnums.RDFItemTypes.Resource) {
                if (resource != null && !this.InValues.ContainsKey(resource.PatternMemberID)) {
                    this.InValues.Add(resource.PatternMemberID, resource);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given literal to the values of this constraint (if ItemType has been set to Literal)
        /// </summary>
        public RDFInConstraint AddValue(RDFLiteral literal) {
            if (this.ItemType == RDFModelEnums.RDFItemTypes.Literal) {
                if (literal != null && !this.InValues.ContainsKey(literal.PatternMemberID)) {
                    this.InValues.Add(literal.PatternMemberID, literal);
                }
            }
            return this;
        }

        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport Evaluate(RDFValidationContext validationContext) {
            RDFValidationReport report = new RDFValidationReport(new RDFResource());
            validationContext.ValueNodes.ForEach(valueNode => {

                #region Evaluate

                //Set current value node
                validationContext.ValueNode = valueNode;

                //Evaluate current value node
                if (!this.InValues.Any(v => v.Value.Equals(validationContext.ValueNode)))
                    report.AddResult(new RDFValidationResult(validationContext.Shape,
                                                             RDFVocabulary.SHACL.IN_CONSTRAINT_COMPONENT,
                                                             validationContext.FocusNode,
                                                             validationContext.Shape is RDFPropertyShape ? ((RDFPropertyShape)validationContext.Shape).Path : null,
                                                             validationContext.ValueNode,
                                                             validationContext.Shape.Messages,
                                                             new RDFResource(),
                                                             validationContext.Shape.Severity));

                #endregion

            });
            return report;
        }

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal override RDFGraph ToRDFGraph(RDFShape shape) {
            RDFGraph result = new RDFGraph();
            if (shape != null) {

                //Get collection from values
                RDFCollection values = new RDFCollection(this.ItemType) { InternalReificationSubject = this };
                foreach (RDFPatternMember value in this.InValues.Values) {
                    if (this.ItemType == RDFModelEnums.RDFItemTypes.Literal)
                        values.AddItem((RDFLiteral)value);
                    else
                        values.AddItem((RDFResource)value);
                }
                result.AddCollection(values);

                //sh:in
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.IN, values.ReificationSubject));

            }
            return result;
        }
        #endregion

    }
}