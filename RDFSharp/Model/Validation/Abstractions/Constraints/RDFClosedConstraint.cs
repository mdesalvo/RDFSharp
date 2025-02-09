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
using System.Linq;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFClosedConstraint represents a SHACL constraint on the predicates allowed for a given RDF term
    /// </summary>
    public class RDFClosedConstraint : RDFConstraint
    {
        #region Properties
        /// <summary>
        /// Flag indicating that closure of predicates is required or not
        /// </summary>
        public bool Closed { get; internal set; }

        /// <summary>
        /// Properties allowed on the given RDF term
        /// </summary>
        internal Dictionary<long, RDFResource> IgnoredProperties { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a closed constraint with the given behavior
        /// </summary>
        public RDFClosedConstraint(bool closed)
        {
            Closed = closed;
            IgnoredProperties = new Dictionary<long, RDFResource>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given property to the allowed properties of this constraint
        /// </summary>
        public RDFClosedConstraint AddIgnoredProperty(RDFResource ignoredProperty)
        {
            if (ignoredProperty != null && !IgnoredProperties.ContainsKey(ignoredProperty.PatternMemberID))
                IgnoredProperties.Add(ignoredProperty.PatternMemberID, ignoredProperty);
            return this;
        }

        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal override RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph, RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode, List<RDFPatternMember> valueNodes)
        {
            RDFValidationReport report = new RDFValidationReport();
            
            #region Evaluation
            if (Closed)
            {
                //Extend ignored properties with paths of property constraints
                List<RDFResource> allowedProperties = new List<RDFResource>(IgnoredProperties.Values);
                IEnumerable<RDFPropertyConstraint> propertyConstraints = shape.Constraints.OfType<RDFPropertyConstraint>();
                foreach (RDFPropertyConstraint propertyConstraint in propertyConstraints)
                {
                    if (shapesGraph.SelectShape(propertyConstraint.PropertyShapeUri.ToString()) is RDFPropertyShape propertyShape)
                        allowedProperties.Add(propertyShape.Path);
                }

                //In case no shape messages have been provided, this constraint emits a default one (for usability)
                List<RDFLiteral> shapeMessages = new List<RDFLiteral>(shape.Messages);
                if (shapeMessages.Count == 0)
                    shapeMessages.Add(new RDFPlainLiteral("Predicate is not allowed (closed shape)"));

                //Detect unallowed predicates
                foreach (RDFPatternMember valueNode in valueNodes)
                    if (valueNode is RDFResource valueNodeResource)
                    {
                        RDFGraph valuenodeResourceGraph = dataGraph.SelectTriplesBySubject(valueNodeResource);
                        IEnumerable<RDFTriple> unallowedTriples = valuenodeResourceGraph.Where(t => !allowedProperties.Any(p => p.Equals(t.Predicate)));
                        foreach (RDFTriple unallowedTriple in unallowedTriples)
                            report.AddResult(new RDFValidationResult(shape,
                                                                     RDFVocabulary.SHACL.CLOSED_CONSTRAINT_COMPONENT,
                                                                     valueNodeResource,
                                                                     unallowedTriple.Predicate as RDFResource,
                                                                     unallowedTriple.Object,
                                                                     shapeMessages,
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
                //sh:closed
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.CLOSED, Closed ? RDFTypedLiteral.True : RDFTypedLiteral.False));

                //Get collection from ignored properties
                RDFCollection ignoredProperties = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource) { InternalReificationSubject = this };
                foreach (RDFResource ignoredProperty in IgnoredProperties.Values)
                    ignoredProperties.AddItem(ignoredProperty);
                result.AddCollection(ignoredProperties);

                //sh:ignoredProperties
                result.AddTriple(new RDFTriple(shape, RDFVocabulary.SHACL.IGNORED_PROPERTIES, ignoredProperties.ReificationSubject));
            }
            return result;
        }
        #endregion
    }
}