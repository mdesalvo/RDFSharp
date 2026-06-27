/*
   Copyright 2012-2026 Marco De Salvo
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
                            result.AddRange(dataGraph.SelectTriples(p: target.TargetValue)
                                  .Select(x => x.Subject)
                                  .OfType<RDFResource>());
                            break;

                        //sh:targetObjectsOf
                        case RDFTargetObjectsOf _:
                            result.AddRange(dataGraph.SelectTriples(p: target.TargetValue)
                                  .Select(x => x.Object)
                                  .OfType<RDFResource>());
                            break;

                        //sh:SPARQLTarget
                        case RDFTargetSPARQL targetSPARQL:
                            DataTable targetResults = targetSPARQL.SelectQuery.ApplyToGraph(dataGraph).SelectResults;
                            if (targetResults.Columns.Contains("?THIS"))
                                foreach (DataRow targetResult in targetResults.Rows)
                                {
                                    //Skip rows where "?this" is unbound (UNDEF/null), since they cannot designate a focus node
                                    if (targetResult.IsNull("?THIS"))
                                        continue;

                                    //Only resources are legal focus nodes: literals bound to "?this" are silently discarded
                                    if (RDFQueryUtilities.ParseRDFPatternMember(targetResult["?THIS"].ToString()) is RDFResource focusNode)
                                        result.Add(focusNode);
                                }
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
                            RDFPropertyPathExpression pathExpression = propertyShape.Path.Expression;

                            #region single predicate (fast path)
                            //A plain single predicate (possibly inverse) is a ground triple lookup, not a path evaluation
                            if (pathExpression.Kind == RDFQueryEnums.RDFPropertyPathExpressionKinds.Link
                                 && pathExpression.Cardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne)
                            {
                                if (pathExpression.IsInverse)
                                    result.AddRange(dataGraph.SelectTriples(p: pathExpression.Property, o: focusNodeResource)
                                          .Select(t => t.Subject));
                                else
                                    result.AddRange(dataGraph.SelectTriples(s: focusNodeResource, p: pathExpression.Property)
                                          .Select(t => t.Object));
                            }
                            #endregion

                            #region complex path (tree evaluation)
                            //Every other SHACL path (sequence/alternative/inverse-of-group/recursive cardinality)
                            //is evaluated as a binary relation rooted at the focus node, collecting the endpoints
                            else
                            {
                                //Compute the property path on a fresh copy rooted at the given focus node (sharing the
                                //immutable path units): the shape's stored path is never touched, so concurrent
                                //validations stay isolated and the focus node is the only varying boundary term
                                RDFPropertyPath contextualizedPath = new RDFPropertyPath(focusNode, new RDFVariable("?END"));
                                propertyShape.Path.SequenceUnits.ForEach(pathUnit => contextualizedPath.AddSequenceStep(pathUnit));
                                RDFTable pathResult = new RDFQueryEngine().ApplyPropertyPath(contextualizedPath, dataGraph);
                                result.AddRange(from RDFTableRow pathResultRow
                                                in pathResult.Rows
                                                select pathResultRow["?END"]
                                                into prValue where !string.IsNullOrEmpty(prValue)
                                                select RDFQueryUtilities.ParseRDFPatternMember(prValue));
                            }
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
                    visitContext = new HashSet<long> { className.PatternMemberID };
                else if (!visitContext.Add(className.PatternMemberID))
                    return result;
                #endregion

                //rdf:type
                result.AddRange(dataGraph.SelectTriples(p: RDFVocabulary.RDF.TYPE, o: className)
                      .Select(triple => triple.Subject));

                //rdfs:subClassOf
                foreach (RDFTriple triple in dataGraph.SelectTriples(p: RDFVocabulary.RDFS.SUB_CLASS_OF, o: className))
                    result.AddRange(dataGraph.GetInstancesOfClass((RDFResource)triple.Subject, visitContext));
            }
            return result;
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Gets ths SHACL shapes graph definition encoded in the given RDF graph
        /// </summary>
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
            foreach (RDFTriple declaredNodeShape in graph.SelectTriples(p:RDFVocabulary.RDF.TYPE, o:RDFVocabulary.SHACL.NODE_SHAPE))
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
            foreach (RDFTriple declaredPropertyShape in graph.SelectTriples(p: RDFVocabulary.RDF.TYPE, o: RDFVocabulary.SHACL.PROPERTY_SHAPE))
            {
                RDFTriple declaredPropertyShapePath = graph.SelectTriples(s: (RDFResource)declaredPropertyShape.Subject, p: RDFVocabulary.SHACL.PATH).FirstOrDefault();
                if (declaredPropertyShapePath?.Object is RDFResource declaredPropertyShapePathObject)
                {
                    RDFPropertyShape propertyShape = new RDFPropertyShape((RDFResource)declaredPropertyShape.Subject, DetectShapePath(graph, declaredPropertyShapePathObject));

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
            foreach (RDFTriple inlinePropertyShape in graph.SelectTriples(p: RDFVocabulary.SHACL.PROPERTY))
                //Inline property shapes are blank objects of "sh:property" constraints:
                //we won't find their explicit shape definition within the shapes graph
                if (inlinePropertyShape.Object is RDFResource inlinePropertyShapeResource
                     && inlinePropertyShapeResource.IsBlank
                     && shapesGraph.SelectShape(inlinePropertyShapeResource.ToString()) == null)
                {
                    RDFTriple inlinePropertyShapePath = graph.SelectTriples(s: inlinePropertyShapeResource, p: RDFVocabulary.SHACL.PATH).FirstOrDefault();
                    if (inlinePropertyShapePath?.Object is RDFResource inlinePropertyShapePathObject)
                    {
                        RDFPropertyShape propertyShape = new RDFPropertyShape(inlinePropertyShapeResource, DetectShapePath(graph, inlinePropertyShapePathObject));

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
            RDFGraph shapeDefinition = graph[s: shape];

            //sh:TargetClass (accepted occurrences: N)
            foreach (RDFTriple targetClass in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.TARGET_CLASS)
                                                             .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetClass((RDFResource)targetClass.Object));

            //sh:TargetNode (accepted occurrences: N)
            foreach (RDFTriple targetNode in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.TARGET_NODE)
                                                            .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetNode((RDFResource)targetNode.Object));

            //sh:TargetSubjectsOf (accepted occurrences: N)
            foreach (RDFTriple targetSubjectOf in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.TARGET_SUBJECTS_OF)
                                                                 .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetSubjectsOf((RDFResource)targetSubjectOf.Object));

            //sh:TargetObjectsOf (accepted occurrences: N)
            foreach (RDFTriple targetObjectOf in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.TARGET_OBJECTS_OF)
                                                                .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetObjectsOf((RDFResource)targetObjectOf.Object));

            //sh:SPARQLTarget via sh:target (accepted occurrences: N)
            foreach (RDFTriple target in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.TARGET)
                                                        .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFGraph targetDefinition = graph[s: (RDFResource)target.Object];
                if (targetDefinition.ContainsTriple(new RDFTriple((RDFResource)target.Object, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.SPARQL_TARGET)))
                {
                    RDFTriple targetSelect = targetDefinition.SelectTriples(p: RDFVocabulary.SHACL.SELECT)
                                                             .FirstOrDefault(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL);
                    if (targetSelect != null)
                        shape.AddTarget(new RDFTargetSPARQL(RDFSelectQuery.FromString(((RDFLiteral)targetSelect.Object).Value)));
                }
            }
        }

        /// <summary>
        /// Detects the attributes of the given shape
        /// </summary>
        private static void DetectShapeAttributes(RDFGraph graph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph[s: shape];

            //sh:severity (accepted occurrences: 1)
            RDFTriple shapeSeverity = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.SEVERITY_PROPERTY).FirstOrDefault();
            if (shapeSeverity != null)
            {
                if (shapeSeverity.Object.Equals(RDFVocabulary.SHACL.INFO))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Info);
                else if (shapeSeverity.Object.Equals(RDFVocabulary.SHACL.WARNING))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Warning);
            }

            //sh:deactivated (accepted occurrences: 1)
            RDFTriple shapeDeactivated = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.DEACTIVATED).FirstOrDefault();
            if (shapeDeactivated?.Object is RDFTypedLiteral shapeDeactivatedLiteral
                    && shapeDeactivatedLiteral.HasBooleanDatatype()
                    && bool.Parse(shapeDeactivatedLiteral.Value))
                shape.Deactivate();

            //sh:message (accepted occurrences: N)
            foreach (RDFTriple shapeMessage in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MESSAGE)
                                                              .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                shape.AddMessage((RDFLiteral)shapeMessage.Object);
        }

        /// <summary>
        /// Detects the non validating attributes of the given property shape
        /// </summary>
        private static void DetectShapeNonValidatingAttributes(RDFGraph graph, RDFPropertyShape shape)
        {
            RDFGraph shapeDefinition = graph[s: shape];

            //sh:description (accepted occurrences: N)
            foreach (RDFTriple shapeDescription in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.DESCRIPTION)
                                                                  .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                shape.AddDescription((RDFLiteral)shapeDescription.Object);

            //sh:name (accepted occurrences: N)
            foreach (RDFTriple shapeName in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.NAME)
                                                           .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                shape.AddName((RDFLiteral)shapeName.Object);

            //sh:group (accepted occurrences: 1)
            RDFTriple shapeGroup = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.GROUP).FirstOrDefault();
            if (shapeGroup?.Object is RDFResource shapeGroupObject)
                shape.SetGroup(shapeGroupObject);

            //sh:order (accepted occurrences: 1)
            RDFTriple shapeOrder = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.ORDER).FirstOrDefault();
            if (shapeOrder?.Object is RDFTypedLiteral shapeOrderLiteral
                  && shapeOrderLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.SetOrder(int.Parse(shapeOrderLiteral.Value));
        }

        /// <summary>
        /// Detects the constraints of the given shape
        /// </summary>
        private static void DetectShapeConstraints(RDFGraph graph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph[s: shape];

            //sh:and (accepted occurrences: N)
            foreach (RDFTriple shapeAndConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.AND)
                                                                    .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFAndConstraint andConstraint = new RDFAndConstraint();
                RDFCollection andConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeAndConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPO);
                andConstraintCollection.Items.ForEach(item => andConstraint.AddShape((RDFResource)item));
                shape.AddConstraint(andConstraint);
            }

            //sh:class (accepted occurrences: N)
            foreach (RDFTriple shapeClassConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.CLASS)
                                                                      .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFClassConstraint((RDFResource)shapeClassConstraint.Object));

            //sh:closed (accepted occurrences: 1)
            RDFTriple shapeClosedConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.CLOSED).FirstOrDefault();
            if (shapeClosedConstraint?.Object is RDFTypedLiteral shapeClosedConstraintLiteral
                 && shapeClosedConstraintLiteral.HasBooleanDatatype())
            {
                RDFClosedConstraint closedConstraint = new RDFClosedConstraint(bool.Parse(shapeClosedConstraintLiteral.Value));

                //sh:ignoredProperties (accepted occurrences: 1)
                RDFTriple shapeIgnoredPropertiesConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.IGNORED_PROPERTIES).FirstOrDefault();
                if (shapeIgnoredPropertiesConstraint?.Object is RDFResource shapeIgnoredPropertiesConstraintResource)
                {
                    RDFCollection shapeIgnoredPropertiesConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, shapeIgnoredPropertiesConstraintResource, RDFModelEnums.RDFTripleFlavors.SPO);
                    shapeIgnoredPropertiesConstraintCollection.Items.ForEach(item => closedConstraint.AddIgnoredProperty((RDFResource)item));
                }

                shape.AddConstraint(closedConstraint);
            }

            //sh:datatype (accepted occurrences: N)
            foreach (RDFTriple shapeDatatypeConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.DATATYPE)
                                                                         .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFDatatypeConstraint(RDFDatatypeRegister.GetDatatype(shapeDatatypeConstraint.Object.ToString())));

            //sh:disjoint (accepted occurrences: N)
            foreach (RDFTriple shapeDisjointConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.DISJOINT)
                                                                         .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFDisjointConstraint((RDFResource)shapeDisjointConstraint.Object));

            //sh:equals (accepted occurrences: N)
            foreach (RDFTriple shapeEqualsConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.EQUALS)
                                                                       .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFEqualsConstraint((RDFResource)shapeEqualsConstraint.Object));

            //sh:hasValue (accepted occurrences: N)
            foreach (RDFTriple shapeHasValueConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.HAS_VALUE))
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
            foreach (RDFTriple shapeInConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.IN)
                                                                   .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
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
            foreach (RDFTriple shapeLanguageInConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.LANGUAGE_IN)
                                                                           .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFCollection shapeLanguageInConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeLanguageInConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPL);
                shape.AddConstraint(new RDFLanguageInConstraint(shapeLanguageInConstraintCollection.Select(x => x.ToString()).ToList()));
            }

            //sh:lessThan (accepted occurrences: N)
            foreach (RDFTriple shapeLessThanConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.LESS_THAN)
                                                                         .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFLessThanConstraint((RDFResource)shapeLessThanConstraint.Object));

            //sh:lessThanOrEquals (accepted occurrences: N)
            foreach (RDFTriple shapeLessThanOrEqualsConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS)
                                                                                 .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFLessThanOrEqualsConstraint((RDFResource)shapeLessThanOrEqualsConstraint.Object));

            //sh:maxCount (accepted occurrences: 1)
            RDFTriple shapeMaxCountConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MAX_COUNT).FirstOrDefault();
            if (shapeMaxCountConstraint?.Object is RDFTypedLiteral shaclMaxCountConstraintLiteral
                  && shaclMaxCountConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.AddConstraint(new RDFMaxCountConstraint(int.Parse(shaclMaxCountConstraintLiteral.Value)));

            //sh:maxExclusive (accepted occurrences: 1)
            RDFTriple shapeMaxExclusiveConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MAX_EXCLUSIVE).FirstOrDefault();
            if (shapeMaxExclusiveConstraint?.Object is RDFLiteral shapeMaxExclusiveConstraintLiteral)
                shape.AddConstraint(new RDFMaxExclusiveConstraint(shapeMaxExclusiveConstraintLiteral));

            //sh:maxInclusive (accepted occurrences: 1)
            RDFTriple shapeMaxInclusiveConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MAX_INCLUSIVE).FirstOrDefault();
            if (shapeMaxInclusiveConstraint?.Object is RDFLiteral shapeMaxInclusiveConstraintLiteral)
                shape.AddConstraint(new RDFMaxInclusiveConstraint(shapeMaxInclusiveConstraintLiteral));

            //sh:maxLength (accepted occurrences: 1)
            RDFTriple shapeMaxLengthConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MAX_LENGTH).FirstOrDefault();
            if (shapeMaxLengthConstraint?.Object is RDFTypedLiteral shaclMaxLengthConstraintLiteral
                  && shaclMaxLengthConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.AddConstraint(new RDFMaxLengthConstraint(int.Parse(shaclMaxLengthConstraintLiteral.Value)));

            //sh:minCount (accepted occurrences: 1)
            RDFTriple shapeMinCountConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MIN_COUNT).FirstOrDefault();
            if (shapeMinCountConstraint?.Object is RDFTypedLiteral shaclMinCountConstraintLiteral
                  && shaclMinCountConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.AddConstraint(new RDFMinCountConstraint(int.Parse(shaclMinCountConstraintLiteral.Value)));

            //sh:minExclusive (accepted occurrences: 1)
            RDFTriple shapeMinExclusiveConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MIN_EXCLUSIVE).FirstOrDefault();
            if (shapeMinExclusiveConstraint?.Object is RDFLiteral shapeMinExclusiveConstraintLiteral)
                shape.AddConstraint(new RDFMinExclusiveConstraint(shapeMinExclusiveConstraintLiteral));

            //sh:minInclusive (accepted occurrences: 1)
            RDFTriple shapeMinInclusiveConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MIN_INCLUSIVE).FirstOrDefault();
            if (shapeMinInclusiveConstraint?.Object is RDFLiteral shapeMinInclusiveConstraintLiteral)
                shape.AddConstraint(new RDFMinInclusiveConstraint(shapeMinInclusiveConstraintLiteral));

            //sh:minLength (accepted occurrences: 1)
            RDFTriple shapeMinLengthConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.MIN_LENGTH).FirstOrDefault();
            if (shapeMinLengthConstraint?.Object is RDFTypedLiteral shaclMinLengthConstraintLiteral
                  && shaclMinLengthConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                shape.AddConstraint(new RDFMinLengthConstraint(int.Parse(shaclMinLengthConstraintLiteral.Value)));

            //sh:node (accepted occurrences: N)
            foreach (RDFTriple shapeNodeConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.NODE)
                                                                     .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFNodeConstraint((RDFResource)shapeNodeConstraint.Object));

            //sh:nodeKind (accepted occurrences: 1)
            RDFTriple shapeNodeKindConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.NODE_KIND).FirstOrDefault();
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
            foreach (RDFTriple shapeNotConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.NOT)
                                                                    .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFNotConstraint((RDFResource)shapeNotConstraint.Object));

            //sh:or (accepted occurrences: N)
            foreach (RDFTriple shapeOrConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.OR)
                                                                   .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFOrConstraint orConstraint = new RDFOrConstraint();
                RDFCollection orConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeOrConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPO);
                orConstraintCollection.Items.ForEach(item => orConstraint.AddShape((RDFResource)item));
                shape.AddConstraint(orConstraint);
            }

            //sh:pattern (accepted occurrences: 1)
            RDFTriple shapePatternConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.PATTERN).FirstOrDefault();
            if (shapePatternConstraint?.Object is RDFTypedLiteral shapePatternConstraintLiteral
                  && shapePatternConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.STRING.ToString()))
            {
                //sh:flags (accepted occurrences: 1)
                RegexOptions regexOptions = RegexOptions.None;
                RDFTriple shapeFlagsConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.FLAGS).FirstOrDefault();
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
            foreach (RDFTriple shapePropertyConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.PROPERTY)
                                                                         .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddConstraint(new RDFPropertyConstraint((RDFResource)shapePropertyConstraint.Object));

            //sh:qualifiedValueShape (accepted occurrences: 1)
            RDFTriple shapeQualifiedValueConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE).FirstOrDefault();
            if (shapeQualifiedValueConstraint?.Object is RDFResource qualifiedValueShapeUri)
            {
                //sh:qualifiedMinCount (accepted occurrences: 1)
                int? qualifiedMinCountValue = null;
                RDFTriple shapeQualifiedMinCountConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT).FirstOrDefault();
                if (shapeQualifiedMinCountConstraint?.Object is RDFTypedLiteral shapeQualifiedMinCountConstraintLiteral
                      && shapeQualifiedMinCountConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                    qualifiedMinCountValue = int.Parse(shapeQualifiedMinCountConstraintLiteral.Value);

                //sh:qualifiedMaxCount (accepted occurrences: 1)
                int? qualifiedMaxCountValue = null;
                RDFTriple shapeQualifiedMaxCountConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT).FirstOrDefault();
                if (shapeQualifiedMaxCountConstraint?.Object is RDFTypedLiteral shapeQualifiedMaxCountConstraintLiteral
                      && shapeQualifiedMaxCountConstraintLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString()))
                    qualifiedMaxCountValue = int.Parse(shapeQualifiedMaxCountConstraintLiteral.Value);

                shape.AddConstraint(new RDFQualifiedValueShapeConstraint(qualifiedValueShapeUri, qualifiedMinCountValue, qualifiedMaxCountValue));
            }

            //sh:uniqueLang (accepted occurrences: 1)
            RDFTriple shapeUniqueLangConstraint = shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.UNIQUE_LANG).FirstOrDefault();
            if (shapeUniqueLangConstraint?.Object is RDFTypedLiteral shapeUniqueLangConstraintLiteral
                  && shapeUniqueLangConstraintLiteral.HasBooleanDatatype())
                shape.AddConstraint(new RDFUniqueLangConstraint(bool.Parse(shapeUniqueLangConstraintLiteral.Value)));

            //sh:xone (accepted occurrences: N)
            foreach (RDFTriple shapeXoneConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.XONE)
                                                                     .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                RDFXoneConstraint xoneConstraint = new RDFXoneConstraint();
                RDFCollection xoneConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)shapeXoneConstraint.Object, RDFModelEnums.RDFTripleFlavors.SPO);
                xoneConstraintCollection.Items.ForEach(item => xoneConstraint.AddShape((RDFResource)item));
                shape.AddConstraint(xoneConstraint);
            }

            //sh:sparql (accepted occurrences: N)
            foreach (RDFTriple shapeSparqlConstraint in shapeDefinition.SelectTriples(p: RDFVocabulary.SHACL.SPARQL)
                                                                       .Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
            {
                //The sh:sparql object must be typed as sh:SPARQLConstraint and must carry an sh:select query
                RDFGraph sparqlConstraintDefinition = graph[s: (RDFResource)shapeSparqlConstraint.Object];
                if (sparqlConstraintDefinition.ContainsTriple(new RDFTriple((RDFResource)shapeSparqlConstraint.Object, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.SPARQL_CONSTRAINT)))
                {
                    RDFTriple sparqlConstraintSelect = sparqlConstraintDefinition.SelectTriples(p: RDFVocabulary.SHACL.SELECT)
                                                                                 .FirstOrDefault(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL);
                    if (sparqlConstraintSelect != null)
                        shape.AddConstraint(new RDFSPARQLConstraint(RDFSelectQuery.FromString(((RDFLiteral)sparqlConstraintSelect.Object).Value)));
                }
            }
        }
        
        /// <summary>
        /// Detects the path rooted at the given node
        /// </summary>
        private static RDFPropertyPath DetectShapePath(RDFGraph graph, RDFResource node)
            => new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
                    .AddSequenceStep(DetectShapePathExpression(graph, node));

        /// <summary>
        /// Detects a single node of the path definition, turning it into the equivalent path expression
        /// </summary>
        private static RDFPropertyPathExpression DetectShapePathExpression(RDFGraph graph, RDFResource node)
        {
            //A bare predicate IRI is a plain Link
            if (!node.IsBlank)
                return RDFPropertyPathExpression.Link(node);

            RDFGraph pathDefinition = graph[s: node];

            //sh:inversePath O -> ^(...)
            if (pathDefinition.SelectTriples(p: RDFVocabulary.SHACL.INVERSE_PATH).FirstOrDefault()?.Object is RDFResource inversePathObject)
                return DetectShapePathExpression(graph, inversePathObject).Inverse();

            //sh:zeroOrMorePath O -> (...)*
            if (pathDefinition.SelectTriples(p: RDFVocabulary.SHACL.ZERO_OR_MORE_PATH).FirstOrDefault()?.Object is RDFResource zeroOrMorePathObject)
                return DetectShapePathExpression(graph, zeroOrMorePathObject).ZeroOrMore();

            //sh:oneOrMorePath O -> (...)+
            if (pathDefinition.SelectTriples(p: RDFVocabulary.SHACL.ONE_OR_MORE_PATH).FirstOrDefault()?.Object is RDFResource oneOrMorePathObject)
                return DetectShapePathExpression(graph, oneOrMorePathObject).OneOrMore();

            //sh:zeroOrOnePath O -> (...)?
            if (pathDefinition.SelectTriples(p: RDFVocabulary.SHACL.ZERO_OR_ONE_PATH).FirstOrDefault()?.Object is RDFResource zeroOrOnePathObject)
                return DetectShapePathExpression(graph, zeroOrOnePathObject).ZeroOrOne();

            //sh:alternativePath L -> (...|...)
            if (pathDefinition.SelectTriples(p: RDFVocabulary.SHACL.ALTERNATIVE_PATH).FirstOrDefault()?.Object is RDFResource alternativePathObject)
            {
                RDFCollection alternativeList = RDFModelUtilities.DeserializeCollectionFromGraph(graph, alternativePathObject, RDFModelEnums.RDFTripleFlavors.SPO);
                return RDFPropertyPathExpression.Alternative(alternativeList.Items.OfType<RDFResource>()
                                                                                  .Select(item => DetectShapePathExpression(graph, item))
                                                                                  .ToList());
            }

            //Otherwise the node is an rdf:List (sequence) -> P1/P2/...
            RDFCollection sequenceList = RDFModelUtilities.DeserializeCollectionFromGraph(graph, node, RDFModelEnums.RDFTripleFlavors.SPO);
            List<RDFPropertyPathExpression> sequenceSteps = sequenceList.Items.OfType<RDFResource>()
                                                                              .Select(item => DetectShapePathExpression(graph, item))
                                                                              .ToList();
            return sequenceSteps.Count == 1 ? sequenceSteps[0] : RDFPropertyPathExpression.Sequence(sequenceSteps);
        }
        #endregion

        #region Serialization
        /// <summary>
        /// Builds the single-predicate property path equivalent to the given predicate IRI: this is the
        /// bare-predicate result path reported by SHACL-SPARQL and closed-shape constraints (a single violating
        /// predicate, not a property shape's path). Returns null when the IRI is null.
        /// </summary>
        internal static RDFPropertyPath SinglePredicatePath(RDFResource predicate)
            => predicate != null
                ? new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
                        .AddSequenceStep(RDFPropertyPathExpression.Link(predicate))
                : null;

        /// <summary>
        /// Serializes the given path expression into RDF, returning its path node (the bare predicate IRI for
        /// a plain single predicate, otherwise the blank node rooting the structure) and adding the structure triples
        /// to the given graph.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static RDFResource SerializeShapePath(RDFGraph graph, RDFPropertyPathExpression expression)
        {
            #region Utilities
            //Builds the rdf:List of the SHACL path nodes of the given children, returning the list head
            RDFResource SerializeShaclPathList(List<RDFPropertyPathExpression> pathExpressions)
            {
                RDFCollection pathNodes = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
                pathExpressions.ForEach(pathExpression => pathNodes.AddItem(SerializeShapePath(graph, pathExpression)));
                graph.AddCollection(pathNodes);
                return pathNodes.ReificationSubject;
            }
            
            //Wraps the given inner path node in a fresh blank node carrying the given SHACL path predicate
            RDFResource WrapShaclPath(RDFResource shaclPathPredicate, RDFResource innerPathNode)
            {
                RDFResource wrapperNode = new RDFResource();
                graph.AddTriple(new RDFTriple(wrapperNode, shaclPathPredicate, innerPathNode));
                return wrapperNode;
            }
            #endregion

            //Undecorated core (the node kind)
            RDFResource pathNode;
            switch (expression.Kind)
            {
                //A single predicate: the path node is the bare IRI
                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Link:
                    pathNode = expression.Property;
                    break;

                //A sequence P1/P2/...: the path node is the rdf:List of the child path nodes
                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence:
                    pathNode = SerializeShaclPathList(expression.Children);
                    break;

                //An alternative P1|P2|...: bnode sh:alternativePath -> rdf:List of the child path nodes
                case RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative:
                    pathNode = new RDFResource();
                    graph.AddTriple(new RDFTriple(pathNode, RDFVocabulary.SHACL.ALTERNATIVE_PATH, SerializeShaclPathList(expression.Children)));
                    break;

                //A negated property set has no SHACL representation (SHACL paths do not include negation)
                default:
                    throw new RDFModelException("Cannot serialize SHACL property path because it contains a negated property set, which is not representable in SHACL.");
            }

            //Cardinality decoration: ?/+/* -> bnode sh:zeroOrOnePath/oneOrMorePath/zeroOrMorePath
            switch (expression.Cardinality)
            {
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne:
                    pathNode = WrapShaclPath(RDFVocabulary.SHACL.ZERO_OR_ONE_PATH, pathNode);
                    break;
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore:
                    pathNode = WrapShaclPath(RDFVocabulary.SHACL.ONE_OR_MORE_PATH, pathNode);
                    break;
                case RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore:
                    pathNode = WrapShaclPath(RDFVocabulary.SHACL.ZERO_OR_MORE_PATH, pathNode);
                    break;
            }

            //Inverse decoration: ^ -> bnode sh:inversePath
            if (expression.IsInverse)
                pathNode = WrapShaclPath(RDFVocabulary.SHACL.INVERSE_PATH, pathNode);

            return pathNode;
        }
        #endregion
    }
}