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

                //Engage Mirella engine for shapes detection
                RDFSelectQuery shapesDetectionQuery =
                    new RDFSelectQuery()
                        .AddPatternGroup(new RDFPatternGroup("NodeShapes")
                            .AddPattern(new RDFPattern(new RDFVariable("nodeshape"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.NODE_SHAPE))
                            .AddPattern(new RDFPattern(new RDFVariable("nodeshape"), RDFVocabulary.SHACL.TARGET_CLASS, new RDFVariable("targetclass")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("nodeshape"), RDFVocabulary.SHACL.TARGET_NODE, new RDFVariable("targetnode")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("nodeshape"), RDFVocabulary.SHACL.TARGET_SUBJECTS_OF, new RDFVariable("targetsubjectsof")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("nodeshape"), RDFVocabulary.SHACL.TARGET_OBJECTS_OF, new RDFVariable("targetobjectsof")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("nodeshape"), RDFVocabulary.SHACL.SEVERITY_PROPERTY, new RDFVariable("severity")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("nodeshape"), RDFVocabulary.SHACL.DEACTIVATED, new RDFVariable("deactivated")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("nodeshape"), RDFVocabulary.SHACL.MESSAGE, new RDFVariable("message")).Optional())
                            .UnionWithNext())
                        .AddPatternGroup(new RDFPatternGroup("PropertyShapes")
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.PROPERTY_SHAPE))
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.PATH, new RDFVariable("path")))
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.TARGET_CLASS, new RDFVariable("targetclass")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.TARGET_NODE, new RDFVariable("targetnode")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.TARGET_SUBJECTS_OF, new RDFVariable("targetsubjectsof")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.TARGET_OBJECTS_OF, new RDFVariable("targetobjectsof")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.SEVERITY_PROPERTY, new RDFVariable("severity")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.DEACTIVATED, new RDFVariable("deactivated")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.MESSAGE, new RDFVariable("message")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.DESCRIPTION, new RDFVariable("description")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.NAME, new RDFVariable("name")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.GROUP, new RDFVariable("group")).Optional())
                            .AddPattern(new RDFPattern(new RDFVariable("propertyshape"), RDFVocabulary.SHACL.ORDER, new RDFVariable("order")).Optional()));
                RDFSelectQueryResult shapesDetectionResult = shapesDetectionQuery.ApplyToGraph(graph);

                //TODO


                return result;
            }
            return null;
        }        
        #endregion

        #endregion

    }
}