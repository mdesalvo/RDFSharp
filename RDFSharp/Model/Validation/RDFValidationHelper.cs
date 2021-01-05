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
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{
    /// <summary>
    ///  RDFValidationHelper contains utility methods supporting SHACL modeling and validation
    /// </summary>
    internal static class RDFValidationHelper
    {

        #region Modeling
        /// <summary>
        /// Gets the focus nodes of the given shape
        /// </summary>
        internal static List<RDFPatternMember> GetFocusNodesOf(this RDFGraph dataGraph, RDFShape shape)
        {
            List<RDFPatternMember> result = new List<RDFPatternMember>();
            if (shape != null && dataGraph != null)
            {
                foreach (RDFTarget target in shape.Targets)
                {
                    switch (target)
                    {

                        //sh:targetClass
                        case RDFTargetClass targetClass:
                            result.AddRange(dataGraph.GetInstancesOfClass(target.TargetValue)
                                                     .OfType<RDFResource>());
                            break;

                        //sh:targetNode
                        case RDFTargetNode targetNode:
                            result.Add(target.TargetValue);
                            break;

                        //sh:targetSubjectsOf
                        case RDFTargetSubjectsOf targetSubjectsOf:
                            result.AddRange(dataGraph.SelectTriplesByPredicate(target.TargetValue)
                                                     .Select(x => x.Subject)
                                                     .OfType<RDFResource>());
                            break;

                        //sh:targetObjectsOf
                        case RDFTargetObjectsOf targetObjectsOf:
                            result.AddRange(dataGraph.SelectTriplesByPredicate(target.TargetValue)
                                                     .Select(x => x.Object)
                                                     .OfType<RDFResource>());
                            break;

                    }
                }
            }
            return RDFQueryUtilities.RemoveDuplicates(result);
        }

        /// <summary>
        /// Gets the value nodes of the given focus node
        /// </summary>
        internal static List<RDFPatternMember> GetValueNodesOf(this RDFGraph dataGraph, RDFShape shape, RDFPatternMember focusNode)
        {
            List<RDFPatternMember> result = new List<RDFPatternMember>();
            if (shape != null && dataGraph != null)
            {
                switch (shape)
                {

                    //sh:NodeShape
                    case RDFNodeShape nodeShape:
                        result.Add(focusNode);
                        break;

                    //sh:PropertyShape
                    case RDFPropertyShape propertyShape:
                        if (focusNode is RDFResource)
                        {
                            foreach (var triple in dataGraph.SelectTriplesBySubject((RDFResource)focusNode)
                                                            .SelectTriplesByPredicate(((RDFPropertyShape)shape).Path))
                            {
                                result.Add(triple.Object);
                            }
                        }
                        break;

                }
            }
            return RDFQueryUtilities.RemoveDuplicates(result);
        }

        /// <summary>
        /// Gets the direct (rdf:type) and indirect (rdfs:subClassOf) instances of the given class within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetInstancesOfClass(this RDFGraph dataGraph, RDFResource className, HashSet<long> visitContext = null)
        {
            var result = new List<RDFPatternMember>();
            if (className != null && dataGraph != null)
            {

                #region visitContext
                if (visitContext == null)
                {
                    visitContext = new HashSet<long>() { { className.PatternMemberID } };
                }
                else
                {
                    if (!visitContext.Contains(className.PatternMemberID))
                    {
                        visitContext.Add(className.PatternMemberID);
                    }
                    else
                    {
                        return result;
                    }
                }
                #endregion

                //rdf:type
                foreach (var triple in dataGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)
                                                .SelectTriplesByObject(className))
                    result.Add(triple.Subject);

                //rdfs:subClassOf
                foreach (var triple in dataGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_CLASS_OF)
                                                .SelectTriplesByObject(className))
                    result.AddRange(dataGraph.GetInstancesOfClass((RDFResource)triple.Subject, visitContext));

            }
            return result;
        }
        #endregion

        #region Conversion
        internal static RDFShapesGraph FromRDFGraph(RDFGraph graph)
        {
            if (graph != null)
            {
                RDFShapesGraph result = new RDFShapesGraph(new RDFResource(graph.Context.ToString()));

                DetectTypedNodeShapes(graph, result);
                DetectTypedPropertyShapes(graph, result);
                DetectInlinePropertyShapes(graph, result);

                return result;
            }
            return null;
        }

        /// <summary>
        /// Detects the typed instances of shacl:NodeShape and populates the shapes graph with their definition
        /// </summary>
        private static void DetectTypedNodeShapes(RDFGraph graph, RDFShapesGraph shapesGraph)
        {
            RDFGraph declaredNodeShapes = graph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)
                                               .SelectTriplesByObject(RDFVocabulary.SHACL.NODE_SHAPE);

            foreach (RDFTriple declaredNodeShape in declaredNodeShapes)
            {
                RDFNodeShape nodeShape = new RDFNodeShape((RDFResource)declaredNodeShape.Subject);

                DetectShapeTargets(graph, nodeShape);
                DetectShapeAttributes(graph, nodeShape);
                DetectShapeConstraints(graph, nodeShape);

                shapesGraph.AddShape(nodeShape);
            }
        }

        /// <summary>
        /// Detects the typed instances of shacl:PropertyShape and populates the shapes graph with their definition
        /// </summary>
        private static void DetectTypedPropertyShapes(RDFGraph graph, RDFShapesGraph shapesGraph)
        {
            RDFGraph declaredPropertyShapes = graph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)
                                                   .SelectTriplesByObject(RDFVocabulary.SHACL.PROPERTY_SHAPE);

            foreach (RDFTriple declaredPropertyShape in declaredPropertyShapes)
            {
                RDFTriple declaredPropertyShapePath = graph.SelectTriplesBySubject((RDFResource)declaredPropertyShape.Subject)
                                                           .SelectTriplesByPredicate(RDFVocabulary.SHACL.PATH)
                                                           .FirstOrDefault();

                if (declaredPropertyShapePath != null
                        && declaredPropertyShapePath.Object is RDFResource)
                {
                    RDFPropertyShape propertyShape = new RDFPropertyShape((RDFResource)declaredPropertyShape.Subject, (RDFResource)declaredPropertyShapePath.Object);

                    DetectShapeTargets(graph, propertyShape);
                    DetectShapeAttributes(graph, propertyShape);
                    DetectShapeNonValidatingAttributes(graph, propertyShape);
                    DetectShapeConstraints(graph, propertyShape);

                    shapesGraph.AddShape(propertyShape);
                }
            }
        }

        /// <summary>
        /// Detects the inline instances of shacl:PropertyShape and populates the shapes graph with their definition
        /// </summary>
        private static void DetectInlinePropertyShapes(RDFGraph graph, RDFShapesGraph shapesGraph)
        {
            RDFGraph inlinePropertyShapes = graph.SelectTriplesByPredicate(RDFVocabulary.SHACL.PROPERTY);

            foreach (RDFTriple inlinePropertyShape in inlinePropertyShapes)
            {
                //Inline property shapes are blank objects of "sh:property" constraints:
                //we wont find their explicit shape definition within the shapes graph.
                if (inlinePropertyShape.Object is RDFResource inlinePropertyShapeResource
                        && inlinePropertyShapeResource.IsBlank
                            && shapesGraph.SelectShape(inlinePropertyShapeResource.ToString()) == null)
                {
                    RDFTriple inlinePropertyShapePath = graph.SelectTriplesBySubject(inlinePropertyShapeResource)
                                                             .SelectTriplesByPredicate(RDFVocabulary.SHACL.PATH)
                                                             .FirstOrDefault();

                    if (inlinePropertyShapePath != null
                            && inlinePropertyShapePath.Object is RDFResource)
                    {
                        RDFPropertyShape propertyShape = new RDFPropertyShape(inlinePropertyShapeResource, (RDFResource)inlinePropertyShapePath.Object);

                        DetectShapeTargets(graph, propertyShape);
                        DetectShapeAttributes(graph, propertyShape);
                        DetectShapeNonValidatingAttributes(graph, propertyShape);
                        DetectShapeConstraints(graph, propertyShape);

                        shapesGraph.AddShape(propertyShape);
                    }
                }
            }
        }

        /// <summary>
        /// Detects the targets of the given shape
        /// </summary>
        private static void DetectShapeTargets(RDFGraph graph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph.SelectTriplesBySubject(shape);

            //sh:TargetClass (accepted occurrences: N)
            RDFGraph targetClasses = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_CLASS);
            foreach (RDFTriple targetClass in targetClasses.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetClass((RDFResource)targetClass.Object));

            //sh:TargetNode (accepted occurrences: N)
            RDFGraph targetNodes = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_NODE);
            foreach (RDFTriple targetNode in targetNodes.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetNode((RDFResource)targetNode.Object));

            //sh:TargetSubjectsOf (accepted occurrences: N)
            RDFGraph targetSubjectsOf = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_SUBJECTS_OF);
            foreach (RDFTriple targetSubjectOf in targetSubjectsOf.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetSubjectsOf((RDFResource)targetSubjectOf.Object));

            //sh:TargetObjectsOf (accepted occurrences: N)
            RDFGraph targetObjectsOf = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_OBJECTS_OF);
            foreach (RDFTriple targetObjectOf in targetObjectsOf.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetObjectsOf((RDFResource)targetObjectOf.Object));
        }

        /// <summary>
        /// Detects the attributes of the given shape
        /// </summary>
        private static void DetectShapeAttributes(RDFGraph graph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph.SelectTriplesBySubject(shape);

            //sh:severity (accepted occurrences: 1)
            RDFTriple shapeSeverity = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.SEVERITY_PROPERTY).FirstOrDefault();
            if (shapeSeverity != null)
            {
                if (shapeSeverity.Object.Equals(RDFVocabulary.SHACL.INFO))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Info);
                else if (shapeSeverity.Object.Equals(RDFVocabulary.SHACL.WARNING))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Warning);
            }

            //sh:deactivated (accepted occurrences: 1)
            RDFTriple shapeDeactivated = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.DEACTIVATED).FirstOrDefault();
            if (shapeDeactivated != null)
            {
                if (shapeDeactivated.Object is RDFTypedLiteral shapeDeactivatedLiteral
                        && shapeDeactivatedLiteral.HasBooleanDatatype() && bool.Parse(shapeDeactivatedLiteral.Value))
                    shape.Deactivate();
            }

            //sh:message (accepted occurrences: N)
            RDFGraph shapeMessages = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MESSAGE);
            foreach (RDFTriple shapeMessage in shapeMessages.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                shape.AddMessage((RDFLiteral)shapeMessage.Object);
        }

        /// <summary>
        /// Detects the non validating attributes of the given property shape
        /// </summary>
        private static void DetectShapeNonValidatingAttributes(RDFGraph graph, RDFPropertyShape propertyShape)
        {
            RDFGraph shapeDefinition = graph.SelectTriplesBySubject(propertyShape);

            //sh:description (accepted occurrences: N)
            RDFGraph shapeDescriptions = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.DESCRIPTION);
            foreach (RDFTriple shapeDescription in shapeDescriptions.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                propertyShape.AddDescription((RDFLiteral)shapeDescription.Object);

            //sh:name (accepted occurrences: N)
            RDFGraph shapeNames = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.NAME);
            foreach (RDFTriple shapeName in shapeNames.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                propertyShape.AddName((RDFLiteral)shapeName.Object);

            //sh:group (accepted occurrences: 1)
            RDFTriple shapeGroup = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.GROUP).FirstOrDefault();
            if (shapeGroup != null)
            {
                if (shapeGroup.Object is RDFResource)
                    propertyShape.SetGroup((RDFResource)shapeGroup.Object);
            }

            //sh:order (accepted occurrences: 1)
            RDFTriple shapeOrder = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.ORDER).FirstOrDefault();
            if (shapeOrder != null)
            {
                if (shapeOrder.Object is RDFTypedLiteral shapeOrderLiteral
                        && shapeOrderLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    propertyShape.SetOrder(int.Parse(shapeOrderLiteral.Value));
            }
        }

        /// <summary>
        /// Detects the constraints of the given shape
        /// </summary>
        private static void DetectShapeConstraints(RDFGraph graph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph.SelectTriplesBySubject(shape);

            //sh:and (accepted occurrences: N)
            RDFGraph shapeAndConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.AND);
            foreach (RDFTriple shapeAndConstraint in shapeAndConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFAndConstraint andConstraint = new RDFAndConstraint();
                RDFCollection andConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeAndConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPO);
                andConstraintCollection.Items.ForEach(item => andConstraint.AddShape((RDFResource)item));
                shape.AddConstraint(andConstraint);
            }

            //sh:class (accepted occurrences: N)
            RDFGraph shapeClassConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.CLASS);
            foreach (RDFTriple shapeClassConstraint in shapeClassConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFClassConstraint((RDFResource)shapeClassConstraint.Object));

            //sh:closed (accepted occurrences: 1)
            RDFTriple shapeClosedConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.CLOSED).FirstOrDefault();
            if (shapeClosedConstraint != null)
            {
                if (shapeClosedConstraint.Object is RDFTypedLiteral shapeClosedConstraintLiteral
                        && shapeClosedConstraintLiteral.HasBooleanDatatype())
                {
                    RDFClosedConstraint closedConstraint = new RDFClosedConstraint(bool.Parse(shapeClosedConstraintLiteral.Value));

                    //sh:ignoredProperties (accepted occurrences: 1)
                    RDFTriple shapeIgnoredPropertiesConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.IGNORED_PROPERTIES).FirstOrDefault();
                    if (shapeIgnoredPropertiesConstraint != null)
                    {
                        if (shapeIgnoredPropertiesConstraint.Object is RDFResource shapeIgnoredPropertiesConstraintResource)
                        {
                            RDFCollection shapeIgnoredPropertiesConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, shapeIgnoredPropertiesConstraintResource, RDFModelEnums.RDFTripleFlavors.SPO);
                            shapeIgnoredPropertiesConstraintCollection.Items.ForEach(item => closedConstraint.AddIgnoredProperty((RDFResource)item));
                        }
                    }

                    shape.AddConstraint(closedConstraint);
                }
            }

            //sh:datatype (accepted occurrences: N)
            RDFGraph shapeDatatypeConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.DATATYPE);
            foreach (RDFTriple shapeDatatypeConstraint in shapeDatatypeConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFDatatypeConstraint(RDFModelUtilities.GetDatatypeFromString(shapeDatatypeConstraint.Object.ToString())));

            //sh:disjoint (accepted occurrences: N)
            RDFGraph shapeDisjointConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.DISJOINT);
            foreach (RDFTriple shapeDisjointConstraint in shapeDisjointConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFDisjointConstraint((RDFResource)shapeDisjointConstraint.Object));

            //sh:equals (accepted occurrences: N)
            RDFGraph shapeEqualsConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.EQUALS);
            foreach (RDFTriple shapeEqualsConstraint in shapeEqualsConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFEqualsConstraint((RDFResource)shapeEqualsConstraint.Object));

            //sh:hasValue (accepted occurrences: N)
            RDFGraph shapeHasValueConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.HAS_VALUE);
            foreach (RDFTriple shapeHasValueConstraint in shapeHasValueConstraints)
            {
                if (shapeHasValueConstraint.Object is RDFResource)
                    shape.AddConstraint(new RDFHasValueConstraint((RDFResource)shapeHasValueConstraint.Object));
                else if (shapeHasValueConstraint.Object is RDFLiteral)
                    shape.AddConstraint(new RDFHasValueConstraint((RDFLiteral)shapeHasValueConstraint.Object));
            }

            //sh:in (accepted occurrences: N)
            RDFGraph shapeInConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.IN);
            foreach (RDFTriple shapeInConstraint in shapeInConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFModelEnums.RDFTripleFlavors shapeInConstraintCollectionFlavor = RDFModelUtilities.DetectCollectionFlavorFromGraph(graph, (RDFResource)shapeInConstraint.Object);
                RDFCollection shapeInConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeInConstraint.Object, shapeInConstraintCollectionFlavor);
                RDFInConstraint inConstraint = new RDFInConstraint(shapeInConstraintCollection.ItemType);
                shapeInConstraintCollection.Items.ForEach(item =>
                {
                    if (item is RDFResource)
                        inConstraint.AddValue((RDFResource)item);
                    else if (item is RDFLiteral)
                        inConstraint.AddValue((RDFLiteral)item);
                });
                shape.AddConstraint(inConstraint);
            }

            //sh:languageIn (accepted occurrences: N)
            RDFGraph shapeLanguageInConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.LANGUAGE_IN);
            foreach (RDFTriple shapeLanguageInConstraint in shapeLanguageInConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFCollection shapeLanguageInConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeLanguageInConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPL);
                shape.AddConstraint(new RDFLanguageInConstraint(shapeLanguageInConstraintCollection.Select(x => x.ToString()).ToList()));
            }

            //sh:lessThan (accepted occurrences: N)
            RDFGraph shapeLessThanConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.LESS_THAN);
            foreach (RDFTriple shapeLessThanConstraint in shapeLessThanConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFLessThanConstraint((RDFResource)shapeLessThanConstraint.Object));

            //sh:lessThanOrEquals (accepted occurrences: N)
            RDFGraph shapeLessThanOrEqualsConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS);
            foreach (RDFTriple shapeLessThanOrEqualsConstraint in shapeLessThanOrEqualsConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFLessThanOrEqualsConstraint((RDFResource)shapeLessThanOrEqualsConstraint.Object));

            //sh:maxCount (accepted occurrences: 1)
            RDFTriple shapeMaxCountConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MAX_COUNT).FirstOrDefault();
            if (shapeMaxCountConstraint != null)
            {
                if (shapeMaxCountConstraint.Object is RDFTypedLiteral shaclMaxCountConstraintLiteral
                        && shaclMaxCountConstraintLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    shape.AddConstraint(new RDFMaxCountConstraint(int.Parse(shaclMaxCountConstraintLiteral.Value)));
            }

            //sh:maxExclusive (accepted occurrences: 1)
            RDFTriple shapeMaxExclusiveConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MAX_EXCLUSIVE).FirstOrDefault();
            if (shapeMaxExclusiveConstraint != null)
            {
                if (shapeMaxExclusiveConstraint.Object is RDFResource)
                    shape.AddConstraint(new RDFMaxExclusiveConstraint((RDFResource)shapeMaxExclusiveConstraint.Object));
                else if (shapeMaxExclusiveConstraint.Object is RDFLiteral)
                    shape.AddConstraint(new RDFMaxExclusiveConstraint((RDFLiteral)shapeMaxExclusiveConstraint.Object));
            }

            //sh:maxInclusive (accepted occurrences: 1)
            RDFTriple shapeMaxInclusiveConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MAX_INCLUSIVE).FirstOrDefault();
            if (shapeMaxInclusiveConstraint != null)
            {
                if (shapeMaxInclusiveConstraint.Object is RDFResource)
                    shape.AddConstraint(new RDFMaxInclusiveConstraint((RDFResource)shapeMaxInclusiveConstraint.Object));
                else if (shapeMaxInclusiveConstraint.Object is RDFLiteral)
                    shape.AddConstraint(new RDFMaxInclusiveConstraint((RDFLiteral)shapeMaxInclusiveConstraint.Object));
            }

            //sh:maxLength (accepted occurrences: 1)
            RDFTriple shapeMaxLengthConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MAX_LENGTH).FirstOrDefault();
            if (shapeMaxLengthConstraint != null)
            {
                if (shapeMaxLengthConstraint.Object is RDFTypedLiteral shaclMaxLengthConstraintLiteral
                        && shaclMaxLengthConstraintLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    shape.AddConstraint(new RDFMaxLengthConstraint(int.Parse(shaclMaxLengthConstraintLiteral.Value)));
            }

            //sh:minCount (accepted occurrences: 1)
            RDFTriple shapeMinCountConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MIN_COUNT).FirstOrDefault();
            if (shapeMinCountConstraint != null)
            {
                if (shapeMinCountConstraint.Object is RDFTypedLiteral shaclMinCountConstraintLiteral
                        && shaclMinCountConstraintLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    shape.AddConstraint(new RDFMinCountConstraint(int.Parse(shaclMinCountConstraintLiteral.Value)));
            }

            //sh:minExclusive (accepted occurrences: 1)
            RDFTriple shapeMinExclusiveConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MIN_EXCLUSIVE).FirstOrDefault();
            if (shapeMinExclusiveConstraint != null)
            {
                if (shapeMinExclusiveConstraint.Object is RDFResource)
                    shape.AddConstraint(new RDFMinExclusiveConstraint((RDFResource)shapeMinExclusiveConstraint.Object));
                else if (shapeMinExclusiveConstraint.Object is RDFLiteral)
                    shape.AddConstraint(new RDFMinExclusiveConstraint((RDFLiteral)shapeMinExclusiveConstraint.Object));
            }

            //sh:minInclusive (accepted occurrences: 1)
            RDFTriple shapeMinInclusiveConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MIN_INCLUSIVE).FirstOrDefault();
            if (shapeMinInclusiveConstraint != null)
            {
                if (shapeMinInclusiveConstraint.Object is RDFResource)
                    shape.AddConstraint(new RDFMinInclusiveConstraint((RDFResource)shapeMinInclusiveConstraint.Object));
                else if (shapeMinInclusiveConstraint.Object is RDFLiteral)
                    shape.AddConstraint(new RDFMinInclusiveConstraint((RDFLiteral)shapeMinInclusiveConstraint.Object));
            }

            //sh:minLength (accepted occurrences: 1)
            RDFTriple shapeMinLengthConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MIN_LENGTH).FirstOrDefault();
            if (shapeMinLengthConstraint != null)
            {
                if (shapeMinLengthConstraint.Object is RDFTypedLiteral shaclMinLengthConstraintLiteral
                        && shaclMinLengthConstraintLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                    shape.AddConstraint(new RDFMinLengthConstraint(int.Parse(shaclMinLengthConstraintLiteral.Value)));
            }

            //sh:node (accepted occurrences: N)
            RDFGraph shapeNodeConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.NODE);
            foreach (RDFTriple shapeNodeConstraint in shapeNodeConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFNodeConstraint((RDFResource)shapeNodeConstraint.Object));

            //sh:nodeKind (accepted occurrences: 1)
            RDFTriple shapeNodeKindConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.NODE_KIND).FirstOrDefault();
            if (shapeNodeKindConstraint != null)
            {
                if (shapeNodeKindConstraint.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNode));
                else if (shapeNodeKindConstraint.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_IRI))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI));
                else if (shapeNodeKindConstraint.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_LITERAL))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral));
                else if (shapeNodeKindConstraint.Object.Equals(RDFVocabulary.SHACL.IRI))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.IRI));
                else if (shapeNodeKindConstraint.Object.Equals(RDFVocabulary.SHACL.IRI_OR_LITERAL))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.IRIOrLiteral));
                else if (shapeNodeKindConstraint.Object.Equals(RDFVocabulary.SHACL.LITERAL))
                    shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.Literal));
            }

            //sh:not (accepted occurrences: N)
            RDFGraph shapeNotConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.NOT);
            foreach (RDFTriple shapeNotConstraint in shapeNotConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFNotConstraint((RDFResource)shapeNotConstraint.Object));

            //sh:or (accepted occurrences: N)
            RDFGraph shapeOrConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.OR);
            foreach (RDFTriple shapeOrConstraint in shapeOrConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFOrConstraint orConstraint = new RDFOrConstraint();
                RDFCollection orConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeOrConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPO);
                orConstraintCollection.Items.ForEach(item => orConstraint.AddShape((RDFResource)item));
                shape.AddConstraint(orConstraint);
            }

            //sh:pattern (accepted occurrences: 1)
            RDFTriple shapePatternConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.PATTERN).FirstOrDefault();
            if (shapePatternConstraint != null)
            {
                if (shapePatternConstraint.Object is RDFTypedLiteral shapePatternConstraintLiteral
                        && shapePatternConstraintLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))
                {
                    //sh:flags (accepted occurrences: 1)
                    RegexOptions regexOptions = RegexOptions.None;
                    RDFTriple shapeFlagsConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.FLAGS).FirstOrDefault();
                    if (shapeFlagsConstraint != null)
                    {
                        if (shapeFlagsConstraint.Object is RDFTypedLiteral shapeFlagsConstraintLiteral
                                && shapeFlagsConstraintLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))
                        {
                            if (shapeFlagsConstraintLiteral.Value.Contains("i"))
                                regexOptions |= RegexOptions.IgnoreCase;
                            if (shapeFlagsConstraintLiteral.Value.Contains("s"))
                                regexOptions |= RegexOptions.Singleline;
                            if (shapeFlagsConstraintLiteral.Value.Contains("m"))
                                regexOptions |= RegexOptions.Multiline;
                            if (shapeFlagsConstraintLiteral.Value.Contains("x"))
                                regexOptions |= RegexOptions.IgnorePatternWhitespace;
                        }
                    }
                    shape.AddConstraint(new RDFPatternConstraint(new Regex(shapePatternConstraintLiteral.Value, regexOptions)));
                }
            }

            //sh:property (accepted occurrences: N)
            RDFGraph shapePropertyConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.PROPERTY);
            foreach (RDFTriple shapePropertyConstraint in shapePropertyConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFPropertyConstraint((RDFResource)shapePropertyConstraint.Object));

            //sh:qualifiedValueShape (accepted occurrences: 1)
            RDFTriple shapeQualifiedValueConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE).FirstOrDefault();
            if (shapeQualifiedValueConstraint != null)
            {
                if (shapeQualifiedValueConstraint.Object is RDFResource)
                {
                    //sh:qualifiedMinCount (accepted occurrences: 1)
                    int? qualifiedMinCountValue = null;
                    RDFTriple shapeQualifiedMinCountConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT).FirstOrDefault();
                    if (shapeQualifiedMinCountConstraint != null)
                    {
                        if (shapeQualifiedMinCountConstraint.Object is RDFTypedLiteral shapeQualifiedMinCountConstraintLiteral
                                && shapeQualifiedMinCountConstraintLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                            qualifiedMinCountValue = int.Parse(shapeQualifiedMinCountConstraintLiteral.Value);
                    }

                    //sh:qualifiedMaxCount (accepted occurrences: 1)
                    int? qualifiedMaxCountValue = null;
                    RDFTriple shapeQualifiedMaxCountConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT).FirstOrDefault();
                    if (shapeQualifiedMaxCountConstraint != null)
                    {
                        if (shapeQualifiedMaxCountConstraint.Object is RDFTypedLiteral shapeQualifiedMaxCountConstraintLiteral
                                && shapeQualifiedMaxCountConstraintLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER))
                            qualifiedMaxCountValue = int.Parse(shapeQualifiedMaxCountConstraintLiteral.Value);
                    }

                    shape.AddConstraint(new RDFQualifiedValueShapeConstraint((RDFResource)shapeQualifiedValueConstraint.Object, qualifiedMinCountValue, qualifiedMaxCountValue));
                }
            }

            //sh:uniqueLang (accepted occurrences: 1)
            RDFTriple shapeUniqueLangConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.UNIQUE_LANG).FirstOrDefault();
            if (shapeUniqueLangConstraint != null)
            {
                if (shapeUniqueLangConstraint.Object is RDFTypedLiteral shapeUniqueLangConstraintLiteral
                        && shapeUniqueLangConstraintLiteral.HasBooleanDatatype())
                    shape.AddConstraint(new RDFUniqueLangConstraint(bool.Parse(shapeUniqueLangConstraintLiteral.Value)));
            }

            //sh:xone (accepted occurrences: N)
            RDFGraph shapeXoneConstraints = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.XONE);
            foreach (RDFTriple shapeXoneConstraint in shapeXoneConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFXoneConstraint xoneConstraint = new RDFXoneConstraint();
                RDFCollection xoneConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeXoneConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPO);
                xoneConstraintCollection.Items.ForEach(item => xoneConstraint.AddShape((RDFResource)item));
                shape.AddConstraint(xoneConstraint);
            }
        }
        #endregion

    }
}