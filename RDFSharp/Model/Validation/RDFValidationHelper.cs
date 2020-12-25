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
        internal static List<RDFPatternMember> GetInstancesOfClass(this RDFGraph dataGraph, RDFResource className, HashSet<Int64> visitContext = null)
        {
            var result = new List<RDFPatternMember>();
            if (className != null && dataGraph != null)
            {

                #region visitContext
                if (visitContext == null)
                {
                    visitContext = new HashSet<Int64>() { { className.PatternMemberID } };
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

                //Step 1: Detect declared node shapes
                DetectTypedNodeShapes(graph, result);

                //Step 2: Detect declared property shapes
                DetectTypedPropertyShapes(graph, result);

                //Step 3: Detect inline property shapes
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
                //Definition
                RDFNodeShape nodeShape = new RDFNodeShape((RDFResource)declaredNodeShape.Subject);

                //Targets
                DetectShapeTargets(graph, shapesGraph, nodeShape);

                //Attributes
                DetectShapeAttributes(graph, shapesGraph, nodeShape);

                //Constraints
                DetectShapeConstraints(graph, shapesGraph, nodeShape);
            }
        }

        /// <summary>
        /// Detects the typed instances of shacl:PropertyShape and populates the shapes graph with their definition
        /// </summary>
        private static void DetectTypedPropertyShapes(RDFGraph graph, RDFShapesGraph shapesGraph)
        {

        }

        /// <summary>
        /// Detects the inlined instances of shacl:PropertyShape and populates the shapes graph with their definition
        /// </summary>
        private static void DetectInlinePropertyShapes(RDFGraph graph, RDFShapesGraph shapesGraph)
        {

        }

        /// <summary>
        /// Detects the targets of the given shape
        /// </summary>
        private static void DetectShapeTargets(RDFGraph graph, RDFShapesGraph shapesGraph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph.SelectTriplesBySubject(shape);

            //shacl:TargetClass
            RDFGraph targetClasses = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_CLASS);
            foreach (RDFTriple targetClass in targetClasses.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetClass((RDFResource)targetClass.Object));

            //shacl:TargetNode
            RDFGraph targetNodes = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_NODE);
            foreach (RDFTriple targetNode in targetNodes.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetNode((RDFResource)targetNode.Object));

            //shacl:TargetSubjectsOf
            RDFGraph targetSubjectsOf = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_SUBJECTS_OF);
            foreach (RDFTriple targetSubjectOf in targetSubjectsOf.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetSubjectsOf((RDFResource)targetSubjectOf.Object));

            //shacl:TargetObjectsOf
            RDFGraph targetObjectsOf = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_OBJECTS_OF);
            foreach (RDFTriple targetObjectOf in targetObjectsOf.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO))
                shape.AddTarget(new RDFTargetObjectsOf((RDFResource)targetObjectOf.Object));
        }

        /// <summary>
        /// Detects the attributes of the given shape
        /// </summary>
        private static void DetectShapeAttributes(RDFGraph graph, RDFShapesGraph shapesGraph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph.SelectTriplesBySubject(shape);

            //shacl:severity
            shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Violation);
            RDFTriple shapeSeverity = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.SEVERITY_PROPERTY).FirstOrDefault();
            if (shapeSeverity != null)
            {
                if (shapeSeverity.Object.Equals(RDFVocabulary.SHACL.INFO))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Info);
                else if (shapeSeverity.Object.Equals(RDFVocabulary.SHACL.WARNING))
                    shape.SetSeverity(RDFValidationEnums.RDFShapeSeverity.Warning);
            }

            //shacl:deactivated
            shape.Activate();
            RDFTriple shapeDeactivated = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.DEACTIVATED).FirstOrDefault();
            if (shapeDeactivated != null)
            {
                if (shapeDeactivated.Object is RDFTypedLiteral shapeDeactivatedLiteral
                        && shapeDeactivatedLiteral.HasBooleanDatatype() && bool.Parse(shapeDeactivatedLiteral.Value))
                    shape.Deactivate();
            }

            //shacl:message
            RDFGraph shapeMessages = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.MESSAGE);
            foreach (RDFTriple shapeMessage in shapeMessages.Where(t => t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL))
                shape.AddMessage((RDFLiteral)shapeMessage.Object);
        }

        /// <summary>
        /// Detects the constraints of the given shape
        /// </summary>
        private static void DetectShapeConstraints(RDFGraph graph, RDFShapesGraph shapesGraph, RDFShape shape)
        {
            RDFGraph shapeDefinition = graph.SelectTriplesBySubject(shape);

            //shacl:and
            RDFTriple shapeAndConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.AND).FirstOrDefault();
            if (shapeAndConstraint != null)
            {
                if (shapeAndConstraint.Object is RDFResource shapeAndConstraintResource)
                {
                    RDFAndConstraint andConstraint = new RDFAndConstraint();
                    RDFCollection andConstraintCollection = RDFModelUtilities.DeserializeCollectionFromGraph(graph, shapeAndConstraintResource, RDFModelEnums.RDFTripleFlavors.SPO);
                    andConstraintCollection.Items.ForEach(item => andConstraint.AddShape((RDFResource)item));
                    shape.AddConstraint(andConstraint);
                }
            }

            //shacl:class
            RDFTriple shapeClassConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.CLASS).FirstOrDefault();
            if (shapeClassConstraint != null)
            {
                if (shapeClassConstraint.Object is RDFResource shapeClassConstraintResource)
                    shape.AddConstraint(new RDFClassConstraint(shapeClassConstraintResource);
            }

            //shacl:closed
            RDFTriple shapeClosedConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.CLOSED).FirstOrDefault();
            if (shapeClosedConstraint != null)
            {
                if (shapeClosedConstraint.Object is RDFTypedLiteral shapeClosedConstraintLiteral
                        && shapeClosedConstraintLiteral.HasBooleanDatatype() && bool.Parse(shapeClosedConstraintLiteral.Value))
                {
                    RDFClosedConstraint closedConstraint = new RDFClosedConstraint(true);

                    //shacl:ignoredProperties
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

            //shacl:datatype
            RDFTriple shapeDatatypeConstraint = shapeDefinition.SelectTriplesByPredicate(RDFVocabulary.SHACL.DATATYPE).FirstOrDefault();
            if (shapeDatatypeConstraint != null)
            {
                if (shapeDatatypeConstraint.Object is RDFResource shapeDatatypeConstraintResource)
                    shape.AddConstraint(new RDFDatatypeConstraint(RDFModelUtilities.GetDatatypeFromString(shapeDatatypeConstraintResource.ToString())));
            }
        }
        #endregion

    }
}