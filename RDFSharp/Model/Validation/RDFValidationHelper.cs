/*
   Copyright 2012-2019 Marco De Salvo
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

using System;
using System.Collections.Generic;
using System.Linq;
using RDFSharp.Query;

namespace RDFSharp.Model.Validation
{
    /// <summary>
    ///  RDFValidationHelper contains utility methods supporting SHACL modeling and validation
    /// </summary>
    internal static class RDFValidationHelper {

        #region Methods

        #region Modeling
        /// <summary>
        /// Gets the SHACL focus nodes of the given SHACL shape within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetFocusNodesOf(this RDFGraph dataGraph,
                                                               RDFShape shape) {
            var result = new List<RDFPatternMember>();
            if (shape != null && dataGraph != null) {
                shape.Targets.ForEach(target => {
                    switch (target) {

                        //sh:targetClass
                        case RDFTargetClass targetClass:
                            result.AddRange(GetInstancesOfClass(target.TargetValue, dataGraph));
                            break;

                        //sh:targetNode
                        case RDFTargetNode targetNode:
                            result.Add(target.TargetValue);
                            break;

                        //sh:targetSubjectsOf
                        case RDFTargetSubjectsOf targetSubjectsOf:
                            foreach(var triple in dataGraph.SelectTriplesByPredicate(target.TargetValue))
                                result.Add(triple.Subject);
                            break;

                        //sh:targetObjectsOf
                        case RDFTargetObjectsOf targetObjectsOf:
                            foreach (var triple in dataGraph.SelectTriplesByPredicate(target.TargetValue))
                                result.Add(triple.Object);
                            break;

                    }
                });
            }
            return result;
        }

        /// <summary>
        /// Gets the SHACL value nodes of the given SHACL property shape within the given data graph
        /// </summary>
        internal static List<RDFPatternMember> GetValueNodesOf(this RDFGraph dataGraph,
                                                               RDFPropertyShape propertyShape) {
            var result = new List<RDFPatternMember>();
            if (propertyShape != null && dataGraph != null) {
                foreach (var triple in dataGraph.SelectTriplesByPredicate(propertyShape.Path))
                    result.Add(triple.Object);
            }
            return result;
        }

        /// <summary>
        /// Gets the direct (rdf:type) and indirect (rdfs:subClassOf/owl:equivalentClass) instances of the given class
        /// </summary>
        internal static List<RDFPatternMember> GetInstancesOfClass(RDFResource className, 
                                                                   RDFGraph dataGraph, 
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
                var typeTriples = dataGraph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE);
                foreach (var triple in typeTriples.SelectTriplesByObject(className))
                    result.Add(triple.Subject);

                //rdfs:subClassOf / owl:equivalentClass
                var subclassOfTriples = dataGraph.SelectTriplesByPredicate(RDFVocabulary.RDFS.SUB_CLASS_OF);
                var equivalentClassTriples = dataGraph.SelectTriplesByPredicate(RDFVocabulary.OWL.EQUIVALENT_CLASS);
                foreach (var triple in subclassOfTriples.UnionWith(equivalentClassTriples)
                                                        .SelectTriplesByObject(className))
                    result.AddRange(GetInstancesOfClass((RDFResource)triple.Subject, dataGraph, visitContext));

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

        #endregion

    }
}