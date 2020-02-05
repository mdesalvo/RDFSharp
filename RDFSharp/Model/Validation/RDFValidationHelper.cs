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
using System.Linq;

namespace RDFSharp.Model
{
    /// <summary>
    ///  RDFValidationHelper contains utility methods supporting SHACL modeling and validation
    /// </summary>
    internal static class RDFValidationHelper {

        #region Methods

        #region Modeling
        /// <summary>
        /// Gets the focus nodes of the given shape within the given data graph
        /// </summary>
        internal static List<RDFResource> GetFocusNodesOf(this RDFGraph dataGraph,
                                                               RDFShape shape) {
            var result = new List<RDFResource>();
            if (shape != null && dataGraph != null) {
                foreach (var target in shape.Targets) {
                    switch (target) {

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
        /// Gets the value nodes of the given shape within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetValueNodesOf(this RDFGraph dataGraph,
                                                               RDFShape shape,
                                                               RDFResource focusNode) {
            var result = new List<RDFPatternMember>();
            if (shape != null && dataGraph != null) {
                switch (shape) {

                    //sh:NodeShape
                    case RDFNodeShape nodeShape:
                        result.Add(focusNode);
                        break;

                    //sh:PropertyShape
                    case RDFPropertyShape propertyShape:
                        foreach (var triple in dataGraph.SelectTriplesBySubject(focusNode)
                                                        .SelectTriplesByPredicate(((RDFPropertyShape)shape).Path))
                            result.Add(triple.Object);
                        break;

                }
            }
            return RDFQueryUtilities.RemoveDuplicates(result);
        }

        /// <summary>
        /// Gets the direct (rdf:type) and indirect (rdfs:subClassOf) instances of the given class within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetInstancesOfClass(this RDFGraph dataGraph,
                                                                   RDFResource className, 
                                                                   HashSet<Int64> visitContext = null) {
            var result = new List<RDFPatternMember>();
            if (className != null && dataGraph != null) {

                #region visitContext
                if (visitContext == null) {
                    visitContext = new HashSet<Int64>() { { className.PatternMemberID } };
                }
                else {
                    if (!visitContext.Contains(className.PatternMemberID)) {
                        visitContext.Add(className.PatternMemberID);
                    }
                    else {
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

        /// <summary>
        /// Checks if the given language tag is used within the given collection of literals
        /// </summary>
        internal static Boolean CheckLanguageTagInUse(List<RDFLiteral> literals, 
                                                      String languageTag) {
            return literals.Any(lit => lit is RDFPlainLiteral
                                            && ((RDFPlainLiteral)lit).Language.Equals(languageTag, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Gets a shapes graph representation of the given graph
        /// </summary>
        internal static RDFShapesGraph FromRDFGraph(RDFGraph graph) {
            if (graph != null) {
                RDFShapesGraph result = new RDFShapesGraph(new RDFResource(graph.Context.ToString()));
                RDFGraph typesGraph = graph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE);
                RDFGraph pathGraph = graph.SelectTriplesByPredicate(RDFVocabulary.SHACL.PATH);

                #region Typed Shape

                //NodeShape
                foreach (RDFTriple shapeTriple in typesGraph.SelectTriplesByObject(RDFVocabulary.SHACL.NODE_SHAPE))
                    result.AddShape(new RDFNodeShape((RDFResource)shapeTriple.Subject));

                //PropertyShape
                foreach (RDFTriple shapeTriple in typesGraph.SelectTriplesByObject(RDFVocabulary.SHACL.PROPERTY_SHAPE))
                    DetectShapeTypeByPath(result, (RDFResource)shapeTriple.Subject, pathGraph, false);

                #endregion

                #region Target Shape

                //TargetClass
                foreach (RDFTriple shapeTriple in graph.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_CLASS))
                    DetectShapeTypeByPath(result, (RDFResource)shapeTriple.Subject, pathGraph, true);

                //TargetNode
                foreach (RDFTriple shapeTriple in graph.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_NODE))
                    DetectShapeTypeByPath(result, (RDFResource)shapeTriple.Subject, pathGraph, true);

                //TargetSubjectsOf
                foreach (RDFTriple shapeTriple in graph.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_SUBJECTS_OF))
                    DetectShapeTypeByPath(result, (RDFResource)shapeTriple.Subject, pathGraph, true);

                //TargetObjectsOf
                foreach (RDFTriple shapeTriple in graph.SelectTriplesByPredicate(RDFVocabulary.SHACL.TARGET_OBJECTS_OF))
                    DetectShapeTypeByPath(result, (RDFResource)shapeTriple.Subject, pathGraph, true);

                #endregion

                return result;
            }
            return null;
        }
        private static void DetectShapeTypeByPath(RDFShapesGraph result, 
                                                          RDFResource shape, 
                                                          RDFGraph pathGraph,
                                                          bool allowNodeShapeFallback) {

            //Search for sh:path property
            RDFGraph propertyShapeGraph = pathGraph.SelectTriplesBySubject(shape);

            //sh:path property defined
            if (propertyShapeGraph.TriplesCount > 0)
                if (propertyShapeGraph.First().Object is RDFResource)
                    result.AddShape(new RDFPropertyShape(shape, (RDFResource)propertyShapeGraph.First().Object));
                else
                    throw new RDFModelException(String.Format("Cannot create RDFPropertyShape with identifier '{0}' because 'sh:path' property links to a literal", shape));

            //sh:path property not defined
            else
                if (allowNodeShapeFallback)
                    result.AddShape(new RDFNodeShape(shape));
                else
                    throw new RDFModelException(String.Format("Cannot create RDFPropertyShape with identifier '{0}' because 'sh:path' property is not defined", shape));

        }
        #endregion

        #endregion

    }
}