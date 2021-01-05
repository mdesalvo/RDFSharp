﻿/*
   Copyright 2015-2020 Marco De Salvo

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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyChecker is responsible for implicit RDFS/OWL-DL validation of ontologies during modeling
    /// </summary>
    internal static class RDFOntologyChecker
    {

        #region ClassModel
        /// <summary>
        /// Checks if the given class is a reserved BASE ontology class
        /// </summary>
        internal static bool CheckReservedClass(RDFOntologyClass ontClass)
        {
            return (RDFBASEOntology.Instance.Model.ClassModel.Classes.ContainsKey(ontClass.PatternMemberID));
        }

        /// <summary>
        /// Checks if the given childclass can be set subclassof the given motherclass
        /// </summary>
        internal static bool CheckSubClassOfCompatibility(RDFOntologyClassModel classModel,
                                                          RDFOntologyClass childClass,
                                                          RDFOntologyClass motherClass)
        {
            return (!classModel.CheckIsSubClassOf(motherClass, childClass)
                        && !classModel.CheckIsEquivalentClassOf(motherClass, childClass)
                            && !classModel.CheckIsDisjointClassWith(motherClass, childClass));
        }

        /// <summary>
        /// Checks if the given aclass can be set equivalentclassof the given bclass
        /// </summary>
        internal static bool CheckEquivalentClassCompatibility(RDFOntologyClassModel classModel,
                                                               RDFOntologyClass aClass,
                                                               RDFOntologyClass bClass)
        {
            return (!classModel.CheckIsSubClassOf(aClass, bClass)
                        && !classModel.CheckIsSuperClassOf(aClass, bClass)
                            && !classModel.CheckIsDisjointClassWith(aClass, bClass));
        }

        /// <summary>
        /// Checks if the given aclass can be set disjointwith the given bclass
        /// </summary>
        internal static bool CheckDisjointWithCompatibility(RDFOntologyClassModel classModel,
                                                            RDFOntologyClass aClass,
                                                            RDFOntologyClass bClass)
        {
            return (!classModel.CheckIsSubClassOf(aClass, bClass)
                        && !classModel.CheckIsSuperClassOf(aClass, bClass)
                            && !classModel.CheckIsEquivalentClassOf(aClass, bClass));
        }
        #endregion

        #region PropertyModel
        /// <summary>
        /// Checks if the given property is a reserved BASE ontology property
        /// </summary>
        internal static bool CheckReservedProperty(RDFOntologyProperty ontProperty)
        {
            return (RDFBASEOntology.Instance.Model.PropertyModel.Properties.ContainsKey(ontProperty.PatternMemberID));
        }

        /// <summary>
        /// Checks if the given childproperty can be set subpropertyof the given motherproperty
        /// </summary>
        internal static bool CheckSubPropertyOfCompatibility(RDFOntologyPropertyModel propertyModel,
                                                             RDFOntologyObjectProperty childProperty,
                                                             RDFOntologyObjectProperty motherProperty)
        {
            return (!propertyModel.CheckIsSubPropertyOf(motherProperty, childProperty)
                        && !propertyModel.CheckIsEquivalentPropertyOf(motherProperty, childProperty)
                            && !propertyModel.CheckIsPropertyDisjointWith(motherProperty, childProperty)
                                //OWL2-DL decidability
                                && !propertyModel.CheckIsPropertyChain(childProperty)
                                    && !propertyModel.CheckIsPropertyChain(motherProperty));
        }

        /// <summary>
        /// Checks if the given childproperty can be set subpropertyof the given motherproperty
        /// </summary>
        internal static bool CheckSubPropertyOfCompatibility(RDFOntologyPropertyModel propertyModel,
                                                             RDFOntologyDatatypeProperty childProperty,
                                                             RDFOntologyDatatypeProperty motherProperty)
        {
            return (!propertyModel.CheckIsSubPropertyOf(motherProperty, childProperty)
                        && !propertyModel.CheckIsEquivalentPropertyOf(motherProperty, childProperty)
                            && !propertyModel.CheckIsPropertyDisjointWith(motherProperty, childProperty));
        }

        /// <summary>
        /// Checks if the given aProperty can be set equivalentpropertyof the given bProperty
        /// </summary>
        internal static bool CheckEquivalentPropertyCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                  RDFOntologyObjectProperty aProperty,
                                                                  RDFOntologyObjectProperty bProperty)
        {
            return (!propertyModel.CheckIsSubPropertyOf(aProperty, bProperty)
                        && !propertyModel.CheckIsSuperPropertyOf(aProperty, bProperty)
                            && !propertyModel.CheckIsPropertyDisjointWith(aProperty, bProperty)
                                //OWL2-DL decidability
                                && !propertyModel.CheckIsPropertyChain(aProperty)
                                    && !propertyModel.CheckIsPropertyChain(bProperty));
        }

        /// <summary>
        /// Checks if the given aProperty can be set equivalentpropertyof the given bProperty
        /// </summary>
        internal static bool CheckEquivalentPropertyCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                  RDFOntologyDatatypeProperty aProperty,
                                                                  RDFOntologyDatatypeProperty bProperty)
        {
            return (!propertyModel.CheckIsSubPropertyOf(aProperty, bProperty)
                        && !propertyModel.CheckIsSuperPropertyOf(aProperty, bProperty)
                            && !propertyModel.CheckIsPropertyDisjointWith(aProperty, bProperty));
        }

        /// <summary>
        /// Checks if the given aProperty can be set propertyDisjointwith the given bProperty [OWL2]
        /// </summary>
        internal static bool CheckPropertyDisjointWithCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                    RDFOntologyObjectProperty aProperty,
                                                                    RDFOntologyObjectProperty bProperty)
        {
            return (!propertyModel.CheckIsSubPropertyOf(aProperty, bProperty)
                        && !propertyModel.CheckIsSuperPropertyOf(aProperty, bProperty)
                            && !propertyModel.CheckIsEquivalentPropertyOf(aProperty, bProperty)
                                //OWL2-DL decidability
                                && !propertyModel.CheckIsPropertyChain(aProperty)
                                    && !propertyModel.CheckIsPropertyChain(bProperty));
        }

        /// <summary>
        /// Checks if the given aProperty can be set propertyDisjointwith the given bProperty [OWL2]
        /// </summary>
        internal static bool CheckPropertyDisjointWithCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                    RDFOntologyDatatypeProperty aProperty,
                                                                    RDFOntologyDatatypeProperty bProperty)
        {
            return (!propertyModel.CheckIsSubPropertyOf(aProperty, bProperty)
                        && !propertyModel.CheckIsSuperPropertyOf(aProperty, bProperty)
                            && !propertyModel.CheckIsEquivalentPropertyOf(aProperty, bProperty));
        }

        /// <summary>
        /// Checks if the given aProperty can be set inverseof the given bProperty
        /// </summary>
        internal static bool CheckInverseOfPropertyCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                 RDFOntologyObjectProperty aProperty,
                                                                 RDFOntologyObjectProperty bProperty)
        {
            return (!propertyModel.CheckIsSubPropertyOf(aProperty, bProperty)
                        && !propertyModel.CheckIsSuperPropertyOf(aProperty, bProperty)
                            && !propertyModel.CheckIsEquivalentPropertyOf(aProperty, bProperty)
                                //OWL2-DL decidability
                                && !propertyModel.CheckIsPropertyChain(aProperty)
                                    && !propertyModel.CheckIsPropertyChain(bProperty));
        }
        #endregion

        #region Data
        /// <summary>
        /// Checks if the given class can be assigned as classtype of facts
        /// </summary>
        internal static bool CheckClassTypeCompatibility(RDFOntologyClass ontologyClass)
        {
            return (!ontologyClass.IsRestrictionClass()
                        && !ontologyClass.IsCompositeClass()
                             && !ontologyClass.IsEnumerateClass()
                                 && !ontologyClass.IsDataRangeClass());
        }

        /// <summary>
        /// Checks if the given afact can be set sameas the given bfact
        /// </summary>
        internal static bool CheckSameAsCompatibility(RDFOntologyData ontologyData,
                                                      RDFOntologyFact aFact,
                                                      RDFOntologyFact bFact)
        {
            return (!ontologyData.CheckIsDifferentFactFrom(aFact, bFact));
        }

        /// <summary>
        /// Checks if the given afact can be set differentfrom the given bfact
        /// </summary>
        internal static bool CheckDifferentFromCompatibility(RDFOntologyData ontologyData,
                                                             RDFOntologyFact aFact,
                                                             RDFOntologyFact bFact)
        {
            return (!ontologyData.CheckIsSameFactAs(aFact, bFact));
        }

        /// <summary>
        /// Checks if the given "aFact -> objectProperty -> bFact" has transitive assertions
        /// which would cause transitive cycles (unallowed concept in OWL-DL)
        /// </summary>
        internal static bool CheckTransitiveAssertionCompatibility(RDFOntologyData ontologyData,
                                                                   RDFOntologyFact aFact,
                                                                   RDFOntologyObjectProperty objectProperty,
                                                                   RDFOntologyFact bFact)
        {
            return !ontologyData.CheckIsTransitiveAssertionOf(bFact, objectProperty, aFact);
        }

        /// <summary>
        /// Checks if the given "aFact -> objectProperty -> bFact" can be an assertion
        /// </summary>
        internal static bool CheckAssertionCompatibility(RDFOntologyData ontologyData,
                                                         RDFOntologyFact aFact,
                                                         RDFOntologyObjectProperty objectProperty,
                                                         RDFOntologyFact bFact)
        {
            return !ontologyData.CheckIsNegativeAssertion(aFact, objectProperty, bFact);
        }

        /// <summary>
        /// Checks if the given "aFact -> datatypeProperty -> ontologyLiteral" can be an assertion
        /// </summary>
        internal static bool CheckAssertionCompatibility(RDFOntologyData ontologyData,
                                                         RDFOntologyFact aFact,
                                                         RDFOntologyDatatypeProperty datatypeProperty,
                                                         RDFOntologyLiteral ontologyLiteral)
        {
            return !ontologyData.CheckIsNegativeAssertion(aFact, datatypeProperty, ontologyLiteral);
        }

        /// <summary>
        /// Checks if the given "aFact -> objectProperty -> bFact" can be a negative assertion
        /// </summary>
        internal static bool CheckNegativeAssertionCompatibility(RDFOntologyData ontologyData,
                                                                 RDFOntologyFact aFact,
                                                                 RDFOntologyObjectProperty objectProperty,
                                                                 RDFOntologyFact bFact)
        {
            return !ontologyData.CheckIsAssertion(aFact, objectProperty, bFact);
        }

        /// <summary>
        /// Checks if the given "aFact -> datatypeProperty -> ontologyLiteral" can be a negative assertion
        /// </summary>
        internal static bool CheckNegativeAssertionCompatibility(RDFOntologyData ontologyData,
                                                                 RDFOntologyFact aFact,
                                                                 RDFOntologyDatatypeProperty datatypeProperty,
                                                                 RDFOntologyLiteral ontologyLiteral)
        {
            return !ontologyData.CheckIsAssertion(aFact, datatypeProperty, ontologyLiteral);
        }
        #endregion

    }

}