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
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using RDFSharp.Query;

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
                    switch (target)
                    {
                        //sh:targetClass
                        case RDFTargetClass _:
                            result.AddRange(dataGraph.GetInstancesOfClass(target.TargetValue)
                                .OfType<RDFResource>());
                            break;

                        //sh:targetNode
                        case RDFTargetNode _:
                            result.Add(target.TargetValue);
                            break;

                        //sh:targetSubjectsOf
                        case RDFTargetSubjectsOf _:
                            result.AddRange(dataGraph[null, target.TargetValue, null, null]
                                .Select(x => x.Subject)
                                .OfType<RDFResource>());
                            break;

                        //sh:targetObjectsOf
                        case RDFTargetObjectsOf _:
                            result.AddRange(dataGraph[null, target.TargetValue, null, null]
                                .Select(x => x.Object)
                                .OfType<RDFResource>());
                            break;
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
            if (shape != null && dataGraph != null && focusNode != null)
            {
                switch (shape)
                {
                    //sh:NodeShape
                    case RDFNodeShape _:
                        result.Add(focusNode);
                        break;

                    //sh:PropertyShape
                    case RDFPropertyShape propertyShape:
                        if (focusNode is RDFResource focusNodeResource)
                        {
                            #region inversePath
                            if (propertyShape.IsInversePath)
                                result.AddRange(dataGraph[null, propertyShape.Path, focusNodeResource, null]
                                    .Select(t => t.Object));
                            #endregion

                            #region [alternative|sequence]Path
                            else if (propertyShape.AlternativePath != null || propertyShape.SequencePath != null)
                            {
                                bool isAlternativePath = propertyShape.AlternativePath != null;

                                //Contextualize property path to the given focus node
                                if (isAlternativePath)
                                    propertyShape.AlternativePath.Start = focusNode;
                                else
                                    propertyShape.SequencePath.Start = focusNode;

                                //Compute property path on the given focus node
                                DataTable pathResult = new RDFQueryEngine().ApplyPropertyPath(
                                    isAlternativePath ? propertyShape.AlternativePath : propertyShape.SequencePath, dataGraph);
                                result.AddRange(from DataRow pathResultRow
                                                in pathResult.Rows
                                                select pathResultRow["?END"]?.ToString() into prValue where !string.IsNullOrEmpty(prValue)
                                                select RDFQueryUtilities.ParseRDFPatternMember(prValue));

                                //Recontextualize property path to the initial configuration
                                if (isAlternativePath)
                                    propertyShape.AlternativePath.Start = new RDFVariable("?START");
                                else
                                    propertyShape.SequencePath.Start = new RDFVariable("?START");
                            }
                            #endregion

                            #region path
                            else
                                result.AddRange(dataGraph[focusNodeResource, propertyShape.Path, null, null]
                                    .Select(t => t.Object));
                            #endregion
                        }
                        break;
                }
            }
            return RDFQueryUtilities.RemoveDuplicates(result);
        }

        /// <summary>
        /// Gets the direct (rdf:type) and indirect (rdfs:subClassOf) instances of the given class within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetInstancesOfClass(this RDFGraph dataGraph, RDFResource className, HashSet<long> visitContext=null)
        {
            List<RDFPatternMember> result = new List<RDFPatternMember>();
            if (className != null && dataGraph != null)
            {
                #region visitContext
                if (visitContext == null)
                {
                    visitContext = new HashSet<long> { className.PatternMemberID };
                }
                else
                {
                    if (!visitContext.Add(className.PatternMemberID))
                        return result;
                }
                #endregion

                //rdf:type
                result.AddRange(dataGraph[null, RDFVocabulary.RDF.TYPE, className, null]
                    .Select(triple => triple.Subject));

                //rdfs:subClassOf
                foreach (RDFTriple triple in dataGraph[null, RDFVocabulary.RDFS.SUB_CLASS_OF, className, null])
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
            RDFGraph declaredPropertyShapes = graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE, null];
            foreach (RDFTriple declaredPropertyShape in declaredPropertyShapes)
            {
                RDFTriple declaredPropertyShapePath = graph[(RDFResource)declaredPropertyShape.Subject, RDFVocabulary.SHACL.PATH, null, null].FirstOrDefault();
                if (declaredPropertyShapePath?.Object is RDFResource declaredPropertyShapePathObject)
                {
                    RDFPropertyShape propertyShape;
                    if (declaredPropertyShapePathObject.IsBlank)
                    {
                        #region inverse
                        RDFTriple inversePath = graph[declaredPropertyShapePathObject, RDFVocabulary.SHACL.INVERSE_PATH, null, null].FirstOrDefault();
                        if (inversePath?.Object is RDFResource inversePathObject)
                        {
                            propertyShape = new RDFPropertyShape((RDFResource)declaredPropertyShape.Subject,
                                inversePathObject, true);

                            DetectShapeTargets(graph, propertyShape);
                            DetectShapeAttributes(graph, propertyShape);
                            DetectShapeNonValidatingAttributes(graph, propertyShape);
                            DetectShapeConstraints(graph, propertyShape);

                            shapesGraph.AddShape(propertyShape);
                            continue;
                        }
                        #endregion

                        #region alternative
                        RDFTriple alternativePath = graph[declaredPropertyShapePathObject, RDFVocabulary.SHACL.ALTERNATIVE_PATH, null, null].FirstOrDefault();
                        if (alternativePath?.Object is RDFResource alternativePathObject)
                        {
                            RDFCollection alternativePathCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph,
                                alternativePathObject, RDFModelEnums.RDFTripleFlavors.SPO);
                            propertyShape = new RDFPropertyShape((RDFResource)declaredPropertyShape.Subject,
                                alternativePathCollection.Items.OfType<RDFResource>().ToList(), RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative);

                            DetectShapeTargets(graph, propertyShape);
                            DetectShapeAttributes(graph, propertyShape);
                            DetectShapeNonValidatingAttributes(graph, propertyShape);
                            DetectShapeConstraints(graph, propertyShape);

                            shapesGraph.AddShape(propertyShape);
                            continue;
                        }
                        #endregion

                        #region sequence
                        RDFCollection sequencePathCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph,
                                declaredPropertyShapePathObject, RDFModelEnums.RDFTripleFlavors.SPO);
                        propertyShape = new RDFPropertyShape((RDFResource)declaredPropertyShape.Subject,
                            sequencePathCollection.Items.OfType<RDFResource>().ToList(), RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence);

                        DetectShapeTargets(graph, propertyShape);
                        DetectShapeAttributes(graph, propertyShape);
                        DetectShapeNonValidatingAttributes(graph, propertyShape);
                        DetectShapeConstraints(graph, propertyShape);

                        shapesGraph.AddShape(propertyShape);
                        #endregion
                    }
                    else
                    {
                        #region path
                        propertyShape = new RDFPropertyShape((RDFResource)declaredPropertyShape.Subject, declaredPropertyShapePathObject);

                        DetectShapeTargets(graph, propertyShape);
                        DetectShapeAttributes(graph, propertyShape);
                        DetectShapeNonValidatingAttributes(graph, propertyShape);
                        DetectShapeConstraints(graph, propertyShape);

                        shapesGraph.AddShape(propertyShape);
                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// Detects the inline instances of shacl:PropertyShape and populates the shapes graph with their definition
        /// </summary>
        private static void DetectInlinePropertyShapes(RDFGraph graph, RDFShapesGraph shapesGraph)
        {
            RDFGraph inlinePropertyShapes = graph[null, RDFVocabulary.SHACL.PROPERTY, null, null];
            foreach (RDFTriple inlinePropertyShape in inlinePropertyShapes)
                //Inline property shapes are blank objects of "sh:property" constraints:
                //we wont find their explicit shape definition within the shapes graph.
                if (inlinePropertyShape.Object is RDFResource inlinePropertyShapeResource
                    && inlinePropertyShapeResource.IsBlank
                    && shapesGraph.SelectShape(inlinePropertyShapeResource.ToString()) == null)
                {
                    RDFTriple inlinePropertyShapePath = graph[inlinePropertyShapeResource, RDFVocabulary.SHACL.PATH, null, null].FirstOrDefault();
                    if (inlinePropertyShapePath?.Object is RDFResource inlinePropertyShapePathObject)
                    {
                        RDFPropertyShape propertyShape = new RDFPropertyShape(inlinePropertyShapeResource, inlinePropertyShapePathObject);

                        DetectShapeTargets(graph, propertyShape);
                        DetectShapeAttributes(graph, propertyShape);
                        DetectShapeNonValidatingAttributes(graph, propertyShape);
                        DetectShapeConstraints(graph, propertyShape);

                        shapesGraph.AddShape(propertyShape);
                    }
                }
        }

        /// <summary>
        /// Detects the targets of the given shape
        /// </summary>
        private static void DetectShapeTargets(RDFGraph graph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph[shape, null, null, null];

            //sh:TargetClass (accepted occurrences: N)
            RDFGraph targetClasses = shapeDefinition[null, RDFVocabulary.SHACL.TARGET_CLASS, null, null];
            foreach (RDFTriple targetClass in targetClasses.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetClass((RDFResource)targetClass.Object));

            //sh:TargetNode (accepted occurrences: N)
            RDFGraph targetNodes = shapeDefinition[null, RDFVocabulary.SHACL.TARGET_NODE, null, null];
            foreach (RDFTriple targetNode in targetNodes.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetNode((RDFResource)targetNode.Object));

            //sh:TargetSubjectsOf (accepted occurrences: N)
            RDFGraph targetSubjectsOf = shapeDefinition[null, RDFVocabulary.SHACL.TARGET_SUBJECTS_OF, null, null];
            foreach (RDFTriple targetSubjectOf in targetSubjectsOf.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetSubjectsOf((RDFResource)targetSubjectOf.Object));

            //sh:TargetObjectsOf (accepted occurrences: N)
            RDFGraph targetObjectsOf = shapeDefinition[null, RDFVocabulary.SHACL.TARGET_OBJECTS_OF, null, null];
            foreach (RDFTriple targetObjectOf in targetObjectsOf.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetObjectsOf((RDFResource)targetObjectOf.Object));
        }

        /// <summary>
        /// Detects the attributes of the given shape
        /// </summary>
        private static void DetectShapeAttributes(RDFGraph graph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph[shape, null, null, null];

            //sh:severity (accepted occurrences: 1)
            RDFTriple shapeSeverity = shapeDefinition[null, RDFVocabulary.SHACL.SEVERITY_PROPERTY, null, null].FirstOrDefault();
            if (shapeSeverity != null)
            {
                if (shapeSeverity.Object.Equals(RDFVocabulary.SHACL.INFO))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Info);
                else if (shapeSeverity.Object.Equals(RDFVocabulary.SHACL.WARNING))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Warning);
            }

            //sh:deactivated (accepted occurrences: 1)
            RDFTriple shapeDeactivated = shapeDefinition[null, RDFVocabulary.SHACL.DEACTIVATED, null, null].FirstOrDefault();
            if (shapeDeactivated?.Object is RDFTypedLiteral shapeDeactivatedLiteral
                    && shapeDeactivatedLiteral.HasBooleanDatatype()
                    && bool.Parse(shapeDeactivatedLiteral.Value))
                shape.Deactivate();

            //sh:message (accepted occurrences: N)
            RDFGraph shapeMessages = shapeDefinition[null, RDFVocabulary.SHACL.MESSAGE, null, null];
            foreach (RDFTriple shapeMessage in shapeMessages.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                shape.AddMessage((RDFLiteral)shapeMessage.Object);
        }

        /// <summary>
        /// Detects the non validating attributes of the given property shape
        /// </summary>
        private static void DetectShapeNonValidatingAttributes(RDFGraph graph, RDFPropertyShape propertyShape)
        {
            RDFGraph shapeDefinition = graph[propertyShape, null, null, null];

            //sh:description (accepted occurrences: N)
            RDFGraph shapeDescriptions = shapeDefinition[null, RDFVocabulary.SHACL.DESCRIPTION, null, null];
            foreach (RDFTriple shapeDescription in shapeDescriptions.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                propertyShape.AddDescription((RDFLiteral)shapeDescription.Object);

            //sh:name (accepted occurrences: N)
            RDFGraph shapeNames = shapeDefinition[null, RDFVocabulary.SHACL.NAME, null, null];
            foreach (RDFTriple shapeName in shapeNames.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                propertyShape.AddName((RDFLiteral)shapeName.Object);

            //sh:group (accepted occurrences: 1)
            RDFTriple shapeGroup = shapeDefinition[null, RDFVocabulary.SHACL.GROUP, null, null].FirstOrDefault();
            if (shapeGroup?.Object is RDFResource shapeGroupObject)
                propertyShape.SetGroup(shapeGroupObject);

            //sh:order (accepted occurrences: 1)
            RDFTriple shapeOrder = shapeDefinition[null, RDFVocabulary.SHACL.ORDER, null, null].FirstOrDefault();
            if (shapeOrder?.Object is RDFTypedLiteral shapeOrderLiteral
                  && shapeOrderLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                propertyShape.SetOrder(int.Parse(shapeOrderLiteral.Value));
        }

        /// <summary>
        /// Detects the constraints of the given shape
        /// </summary>
        private static void DetectShapeConstraints(RDFGraph graph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph[shape, null, null, null];

            //sh:and (accepted occurrences: N)
            RDFGraph shapeAndConstraints = shapeDefinition[null, RDFVocabulary.SHACL.AND, null, null];
            foreach (RDFTriple shapeAndConstraint in shapeAndConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFAndConstraint andConstraint = new RDFAndConstraint();
                RDFCollection andConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeAndConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPO);
                andConstraintCollection.Items.ForEach(item => andConstraint.AddShape((RDFResource)item));
                shape.AddConstraint(andConstraint);
            }

            //sh:class (accepted occurrences: N)
            RDFGraph shapeClassConstraints = shapeDefinition[null, RDFVocabulary.SHACL.CLASS, null, null];
            foreach (RDFTriple shapeClassConstraint in shapeClassConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFClassConstraint((RDFResource)shapeClassConstraint.Object));

            //sh:closed (accepted occurrences: 1)
            RDFTriple shapeClosedConstraint = shapeDefinition[null, RDFVocabulary.SHACL.CLOSED, null, null].FirstOrDefault();
            if (shapeClosedConstraint?.Object is RDFTypedLiteral shapeClosedConstraintLiteral
                 && shapeClosedConstraintLiteral.HasBooleanDatatype())
            {
                RDFClosedConstraint closedConstraint = new RDFClosedConstraint(bool.Parse(shapeClosedConstraintLiteral.Value));

                //sh:ignoredProperties (accepted occurrences: 1)
                RDFTriple shapeIgnoredPropertiesConstraint = shapeDefinition[null, RDFVocabulary.SHACL.IGNORED_PROPERTIES, null, null].FirstOrDefault();
                if (shapeIgnoredPropertiesConstraint?.Object is RDFResource shapeIgnoredPropertiesConstraintResource)
                {
                    RDFCollection shapeIgnoredPropertiesConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, shapeIgnoredPropertiesConstraintResource, RDFModelEnums.RDFTripleFlavors.SPO);
                    shapeIgnoredPropertiesConstraintCollection.Items.ForEach(item => closedConstraint.AddIgnoredProperty((RDFResource)item));
                }

                shape.AddConstraint(closedConstraint);
            }

            //sh:datatype (accepted occurrences: N)
            RDFGraph shapeDatatypeConstraints = shapeDefinition[null, RDFVocabulary.SHACL.DATATYPE, null, null];
            foreach (RDFTriple shapeDatatypeConstraint in shapeDatatypeConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFDatatypeConstraint(RDFDatatypeRegister.GetDatatype(shapeDatatypeConstraint.Object.ToString())));

            //sh:disjoint (accepted occurrences: N)
            RDFGraph shapeDisjointConstraints = shapeDefinition[null, RDFVocabulary.SHACL.DISJOINT, null, null];
            foreach (RDFTriple shapeDisjointConstraint in shapeDisjointConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFDisjointConstraint((RDFResource)shapeDisjointConstraint.Object));

            //sh:equals (accepted occurrences: N)
            RDFGraph shapeEqualsConstraints = shapeDefinition[null, RDFVocabulary.SHACL.EQUALS, null, null];
            foreach (RDFTriple shapeEqualsConstraint in shapeEqualsConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFEqualsConstraint((RDFResource)shapeEqualsConstraint.Object));

            //sh:hasValue (accepted occurrences: N)
            RDFGraph shapeHasValueConstraints = shapeDefinition[null, RDFVocabulary.SHACL.HAS_VALUE, null, null];
            foreach (RDFTriple shapeHasValueConstraint in shapeHasValueConstraints)
                switch (shapeHasValueConstraint.Object)
                {
                    case RDFResource shapeHasValueConstraintObject:
                        shape.AddConstraint(new RDFHasValueConstraint(shapeHasValueConstraintObject));
                        break;
                    case RDFLiteral shapeHasValueConstraintLiteral:
                        shape.AddConstraint(new RDFHasValueConstraint(shapeHasValueConstraintLiteral));
                        break;
                }

            //sh:in (accepted occurrences: N)
            RDFGraph shapeInConstraints = shapeDefinition[null, RDFVocabulary.SHACL.IN, null, null];
            foreach (RDFTriple shapeInConstraint in shapeInConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFModelEnums.RDFTripleFlavors shapeInConstraintCollectionFlavor = RDFModelUtilities.DetectCollectionFlavorFromGraph(graph, (RDFResource)shapeInConstraint.Object);
                RDFCollection shapeInConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeInConstraint.Object, shapeInConstraintCollectionFlavor);
                RDFInConstraint inConstraint = new RDFInConstraint(shapeInConstraintCollection.ItemType);
                shapeInConstraintCollection.Items.ForEach(item =>
                {
                    switch (item)
                    {
                        case RDFResource itemRes:
                            inConstraint.AddValue(itemRes);
                            break;
                        case RDFLiteral itemLit:
                            inConstraint.AddValue(itemLit);
                            break;
                    }
                });
                shape.AddConstraint(inConstraint);
            }

            //sh:languageIn (accepted occurrences: N)
            RDFGraph shapeLanguageInConstraints = shapeDefinition[null, RDFVocabulary.SHACL.LANGUAGE_IN, null, null];
            foreach (RDFTriple shapeLanguageInConstraint in shapeLanguageInConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFCollection shapeLanguageInConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeLanguageInConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPL);
                shape.AddConstraint(new RDFLanguageInConstraint(shapeLanguageInConstraintCollection.Select(x => x.ToString()).ToList()));
            }

            //sh:lessThan (accepted occurrences: N)
            RDFGraph shapeLessThanConstraints = shapeDefinition[null, RDFVocabulary.SHACL.LESS_THAN, null, null];
            foreach (RDFTriple shapeLessThanConstraint in shapeLessThanConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFLessThanConstraint((RDFResource)shapeLessThanConstraint.Object));

            //sh:lessThanOrEquals (accepted occurrences: N)
            RDFGraph shapeLessThanOrEqualsConstraints = shapeDefinition[null, RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS, null, null];
            foreach (RDFTriple shapeLessThanOrEqualsConstraint in shapeLessThanOrEqualsConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFLessThanOrEqualsConstraint((RDFResource)shapeLessThanOrEqualsConstraint.Object));

            //sh:maxCount (accepted occurrences: 1)
            RDFTriple shapeMaxCountConstraint = shapeDefinition[null, RDFVocabulary.SHACL.MAX_COUNT, null, null].FirstOrDefault();
            if (shapeMaxCountConstraint?.Object is RDFTypedLiteral shaclMaxCountConstraintLiteral
                  && shaclMaxCountConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.AddConstraint(new RDFMaxCountConstraint(int.Parse(shaclMaxCountConstraintLiteral.Value)));

            //sh:maxExclusive (accepted occurrences: 1)
            RDFTriple shapeMaxExclusiveConstraint = shapeDefinition[null, RDFVocabulary.SHACL.MAX_EXCLUSIVE, null, null].FirstOrDefault();
            if (shapeMaxExclusiveConstraint?.Object is RDFLiteral shapeMaxExclusiveConstraintLiteral)
                shape.AddConstraint(new RDFMaxExclusiveConstraint(shapeMaxExclusiveConstraintLiteral));

            //sh:maxInclusive (accepted occurrences: 1)
            RDFTriple shapeMaxInclusiveConstraint = shapeDefinition[null, RDFVocabulary.SHACL.MAX_INCLUSIVE, null, null].FirstOrDefault();
            if (shapeMaxInclusiveConstraint?.Object is RDFLiteral shapeMaxInclusiveConstraintLiteral)
                shape.AddConstraint(new RDFMaxInclusiveConstraint(shapeMaxInclusiveConstraintLiteral));

            //sh:maxLength (accepted occurrences: 1)
            RDFTriple shapeMaxLengthConstraint = shapeDefinition[null, RDFVocabulary.SHACL.MAX_LENGTH, null, null].FirstOrDefault();
            if (shapeMaxLengthConstraint?.Object is RDFTypedLiteral shaclMaxLengthConstraintLiteral
                  && shaclMaxLengthConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.AddConstraint(new RDFMaxLengthConstraint(int.Parse(shaclMaxLengthConstraintLiteral.Value)));

            //sh:minCount (accepted occurrences: 1)
            RDFTriple shapeMinCountConstraint = shapeDefinition[null, RDFVocabulary.SHACL.MIN_COUNT, null, null].FirstOrDefault();
            if (shapeMinCountConstraint?.Object is RDFTypedLiteral shaclMinCountConstraintLiteral
                  && shaclMinCountConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.AddConstraint(new RDFMinCountConstraint(int.Parse(shaclMinCountConstraintLiteral.Value)));

            //sh:minExclusive (accepted occurrences: 1)
            RDFTriple shapeMinExclusiveConstraint = shapeDefinition[null, RDFVocabulary.SHACL.MIN_EXCLUSIVE, null, null].FirstOrDefault();
            if (shapeMinExclusiveConstraint?.Object is RDFLiteral shapeMinExclusiveConstraintLiteral)
                shape.AddConstraint(new RDFMinExclusiveConstraint(shapeMinExclusiveConstraintLiteral));

            //sh:minInclusive (accepted occurrences: 1)
            RDFTriple shapeMinInclusiveConstraint = shapeDefinition[null, RDFVocabulary.SHACL.MIN_INCLUSIVE, null, null].FirstOrDefault();
            if (shapeMinInclusiveConstraint?.Object is RDFLiteral shapeMinInclusiveConstraintLiteral)
                shape.AddConstraint(new RDFMinInclusiveConstraint(shapeMinInclusiveConstraintLiteral));

            //sh:minLength (accepted occurrences: 1)
            RDFTriple shapeMinLengthConstraint = shapeDefinition[null, RDFVocabulary.SHACL.MIN_LENGTH, null, null].FirstOrDefault();
            if (shapeMinLengthConstraint?.Object is RDFTypedLiteral shaclMinLengthConstraintLiteral
                  && shaclMinLengthConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.AddConstraint(new RDFMinLengthConstraint(int.Parse(shaclMinLengthConstraintLiteral.Value)));

            //sh:node (accepted occurrences: N)
            RDFGraph shapeNodeConstraints = shapeDefinition[null, RDFVocabulary.SHACL.NODE, null, null];
            foreach (RDFTriple shapeNodeConstraint in shapeNodeConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFNodeConstraint((RDFResource)shapeNodeConstraint.Object));

            //sh:nodeKind (accepted occurrences: 1)
            RDFTriple shapeNodeKindConstraint = shapeDefinition[null, RDFVocabulary.SHACL.NODE_KIND, null, null].FirstOrDefault();
            if (shapeNodeKindConstraint?.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE) ?? false)
                shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNode));
            else if (shapeNodeKindConstraint?.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_IRI) ?? false)
                shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNodeOrIRI));
            else if (shapeNodeKindConstraint?.Object.Equals(RDFVocabulary.SHACL.BLANK_NODE_OR_LITERAL) ?? false)
                shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.BlankNodeOrLiteral));
            else if (shapeNodeKindConstraint?.Object.Equals(RDFVocabulary.SHACL.IRI) ?? false)
                shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.IRI));
            else if (shapeNodeKindConstraint?.Object.Equals(RDFVocabulary.SHACL.IRI_OR_LITERAL) ?? false)
                shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.IRIOrLiteral));
            else if (shapeNodeKindConstraint?.Object.Equals(RDFVocabulary.SHACL.LITERAL) ?? false)
                shape.AddConstraint(new RDFNodeKindConstraint(RDFValidationEnums.RDFNodeKinds.Literal));

            //sh:not (accepted occurrences: N)
            RDFGraph shapeNotConstraints = shapeDefinition[null, RDFVocabulary.SHACL.NOT, null, null];
            foreach (RDFTriple shapeNotConstraint in shapeNotConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFNotConstraint((RDFResource)shapeNotConstraint.Object));

            //sh:or (accepted occurrences: N)
            RDFGraph shapeOrConstraints = shapeDefinition[null, RDFVocabulary.SHACL.OR, null, null];
            foreach (RDFTriple shapeOrConstraint in shapeOrConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFOrConstraint orConstraint = new RDFOrConstraint();
                RDFCollection orConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeOrConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPO);
                orConstraintCollection.Items.ForEach(item => orConstraint.AddShape((RDFResource)item));
                shape.AddConstraint(orConstraint);
            }

            //sh:pattern (accepted occurrences: 1)
            RDFTriple shapePatternConstraint = shapeDefinition[null, RDFVocabulary.SHACL.PATTERN, null, null].FirstOrDefault();
            if (shapePatternConstraint?.Object is RDFTypedLiteral shapePatternConstraintLiteral
                  && shapePatternConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.STRING.ToString()))
            {
                //sh:flags (accepted occurrences: 1)
                RegexOptions regexOptions = RegexOptions.None;
                RDFTriple shapeFlagsConstraint = shapeDefinition[null, RDFVocabulary.SHACL.FLAGS, null, null].FirstOrDefault();
                if (shapeFlagsConstraint?.Object is RDFTypedLiteral shapeFlagsConstraintLiteral
                      && shapeFlagsConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.STRING.ToString()))
                {
                    if (shapeFlagsConstraintLiteral.Value.Contains('i'))
                        regexOptions |= RegexOptions.IgnoreCase;
                    if (shapeFlagsConstraintLiteral.Value.Contains('s'))
                        regexOptions |= RegexOptions.Singleline;
                    if (shapeFlagsConstraintLiteral.Value.Contains('m'))
                        regexOptions |= RegexOptions.Multiline;
                    if (shapeFlagsConstraintLiteral.Value.Contains('x'))
                        regexOptions |= RegexOptions.IgnorePatternWhitespace;
                }
                shape.AddConstraint(new RDFPatternConstraint(new Regex(shapePatternConstraintLiteral.Value, regexOptions)));
            }

            //sh:property (accepted occurrences: N)
            RDFGraph shapePropertyConstraints = shapeDefinition[null, RDFVocabulary.SHACL.PROPERTY, null, null];
            foreach (RDFTriple shapePropertyConstraint in shapePropertyConstraints.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFPropertyConstraint((RDFResource)shapePropertyConstraint.Object));

            //sh:qualifiedValueShape (accepted occurrences: 1)
            RDFTriple shapeQualifiedValueConstraint = shapeDefinition[null, RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE, null, null].FirstOrDefault();
            if (shapeQualifiedValueConstraint?.Object is RDFResource qualifiedValueShapeUri)
            {
                //sh:qualifiedMinCount (accepted occurrences: 1)
                int? qualifiedMinCountValue = null;
                RDFTriple shapeQualifiedMinCountConstraint = shapeDefinition[null, RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT, null, null].FirstOrDefault();
                if (shapeQualifiedMinCountConstraint?.Object is RDFTypedLiteral shapeQualifiedMinCountConstraintLiteral
                      && shapeQualifiedMinCountConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                    qualifiedMinCountValue = int.Parse(shapeQualifiedMinCountConstraintLiteral.Value);

                //sh:qualifiedMaxCount (accepted occurrences: 1)
                int? qualifiedMaxCountValue = null;
                RDFTriple shapeQualifiedMaxCountConstraint = shapeDefinition[null, RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT, null, null].FirstOrDefault();
                if (shapeQualifiedMaxCountConstraint?.Object is RDFTypedLiteral shapeQualifiedMaxCountConstraintLiteral
                      && shapeQualifiedMaxCountConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                    qualifiedMaxCountValue = int.Parse(shapeQualifiedMaxCountConstraintLiteral.Value);

                shape.AddConstraint(new RDFQualifiedValueShapeConstraint(qualifiedValueShapeUri, qualifiedMinCountValue, qualifiedMaxCountValue));
            }

            //sh:uniqueLang (accepted occurrences: 1)
            RDFTriple shapeUniqueLangConstraint = shapeDefinition[null, RDFVocabulary.SHACL.UNIQUE_LANG, null, null].FirstOrDefault();
            if (shapeUniqueLangConstraint?.Object is RDFTypedLiteral shapeUniqueLangConstraintLiteral
                  && shapeUniqueLangConstraintLiteral.HasBooleanDatatype())
                shape.AddConstraint(new RDFUniqueLangConstraint(bool.Parse(shapeUniqueLangConstraintLiteral.Value)));

            //sh:xone (accepted occurrences: N)
            RDFGraph shapeXoneConstraints = shapeDefinition[null, RDFVocabulary.SHACL.XONE, null, null];
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