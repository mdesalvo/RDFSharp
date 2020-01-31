/*
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

using RDFSharp.Semantics.Ontology.Data;
using RDFSharp.Semantics.Ontology.Model.ClassModel;
using RDFSharp.Semantics.Ontology.Model.PropertyModel;
using RDFSharp.Semantics.Ontology.Model.PropertyModel.Properties;
using System;

namespace RDFSharp.Semantics.Ontology
{

    /// <summary>
    /// RDFOntologyChecker is responsible for implicit RDFS/OWL-DL validation of ontologies during modeling
    /// </summary>
    internal static class RDFOntologyChecker {

        #region ClassModel
        /// <summary>
        /// Checks if the given class is a reserved BASE ontology class
        /// </summary>
        internal static Boolean CheckReservedClass(RDFOntologyClass ontClass) {
            return (RDFBASEOntology.Instance.Model.ClassModel.Classes.ContainsKey(ontClass.PatternMemberID));
        }

        /// <summary>
        /// Checks if the given childclass can be set subclassof the given motherclass
        /// </summary>
        internal static Boolean CheckSubClassOfCompatibility(RDFOntologyClassModel classModel, 
                                                             RDFOntologyClass childClass,
                                                             RDFOntologyClass motherClass) {
            return (!classModel.CheckIsSubClassOf(motherClass, childClass)
                        && !classModel.CheckIsEquivalentClassOf(motherClass, childClass) 
                            && !classModel.CheckIsDisjointClassWith(motherClass, childClass));
        }

        /// <summary>
        /// Checks if the given aclass can be set equivalentclassof the given bclass
        /// </summary>
        internal static Boolean CheckEquivalentClassCompatibility(RDFOntologyClassModel classModel,
                                                                  RDFOntologyClass aClass,
                                                                  RDFOntologyClass bClass) {
            return (!classModel.CheckIsSubClassOf(aClass, bClass)
                        && !classModel.CheckIsSuperClassOf(aClass, bClass)
                            && !classModel.CheckIsDisjointClassWith(aClass, bClass));
        }

        /// <summary>
        /// Checks if the given aclass can be set disjointwith the given bclass
        /// </summary>
        internal static Boolean CheckDisjointWithCompatibility(RDFOntologyClassModel classModel,
                                                               RDFOntologyClass aClass,
                                                               RDFOntologyClass bClass) {
            return (!classModel.CheckIsSubClassOf(aClass, bClass)
                        && !classModel.CheckIsSuperClassOf(aClass, bClass)
                            && !classModel.CheckIsEquivalentClassOf(aClass, bClass));
        }
        #endregion

        #region PropertyModel
        /// <summary>
        /// Checks if the given property is a reserved BASE ontology property
        /// </summary>
        internal static Boolean CheckReservedProperty(RDFOntologyProperty ontProperty) {
            return (RDFBASEOntology.Instance.Model.PropertyModel.Properties.ContainsKey(ontProperty.PatternMemberID));
        }

        /// <summary>
        /// Checks if the given childproperty can be set subpropertyof the given motherproperty
        /// </summary>
        internal static Boolean CheckSubPropertyOfCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                RDFOntologyObjectProperty childProperty,
                                                                RDFOntologyObjectProperty motherProperty) {
            return (!propertyModel.CheckIsSubPropertyOf(motherProperty, childProperty)
                        && !propertyModel.CheckIsEquivalentPropertyOf(motherProperty, childProperty));
        }

        /// <summary>
        /// Checks if the given childproperty can be set subpropertyof the given motherproperty
        /// </summary>
        internal static Boolean CheckSubPropertyOfCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                RDFOntologyDatatypeProperty childProperty,
                                                                RDFOntologyDatatypeProperty motherProperty) {
            return (!propertyModel.CheckIsSubPropertyOf(motherProperty, childProperty)
                        && !propertyModel.CheckIsEquivalentPropertyOf(motherProperty, childProperty));
        }

        /// <summary>
        /// Checks if the given aproperty can be set equivalentpropertyof the given bproperty
        /// </summary>
        internal static Boolean CheckEquivalentPropertyCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                     RDFOntologyObjectProperty aProperty,
                                                                     RDFOntologyObjectProperty bProperty) {
            return (!propertyModel.CheckIsSubPropertyOf(aProperty, bProperty)
                        && !propertyModel.CheckIsSuperPropertyOf(aProperty, bProperty));
        }

        /// <summary>
        /// Checks if the given aproperty can be set equivalentpropertyof the given bproperty
        /// </summary>
        internal static Boolean CheckEquivalentPropertyCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                     RDFOntologyDatatypeProperty aProperty,
                                                                     RDFOntologyDatatypeProperty bProperty) {
            return (!propertyModel.CheckIsSubPropertyOf(aProperty, bProperty)
                        && !propertyModel.CheckIsSuperPropertyOf(aProperty, bProperty));
        }

        /// <summary>
        /// Checks if the given aproperty can be set inverseof the given bproperty
        /// </summary>
        internal static Boolean CheckInverseOfPropertyCompatibility(RDFOntologyPropertyModel propertyModel,
                                                                    RDFOntologyObjectProperty aProperty,
                                                                    RDFOntologyObjectProperty bProperty) {
            return (!propertyModel.CheckIsSubPropertyOf(aProperty, bProperty)
                        && !propertyModel.CheckIsSuperPropertyOf(aProperty, bProperty)
                            && !propertyModel.CheckIsEquivalentPropertyOf(aProperty, bProperty));
        }
        #endregion

        #region Data
        /// <summary>
        /// Checks if the given class can be assigned as classtype of facts
        /// </summary>
        internal static Boolean CheckClassTypeCompatibility(RDFOntologyClass ontologyClass) {
            return (!ontologyClass.IsRestrictionClass()
                        && !ontologyClass.IsCompositeClass()
                             && !ontologyClass.IsEnumerateClass()
                                 && !ontologyClass.IsDataRangeClass());
        }

        /// <summary>
        /// Checks if the given afact can be set sameas the given bfact
        /// </summary>
        internal static Boolean CheckSameAsCompatibility(RDFOntologyData ontologyData, 
                                                         RDFOntologyFact aFact, 
                                                         RDFOntologyFact bFact) {
            return (!ontologyData.CheckIsDifferentFactFrom(aFact, bFact));
        }

        /// <summary>
        /// Checks if the given afact can be set differentfrom the given bfact
        /// </summary>
        internal static Boolean CheckDifferentFromCompatibility(RDFOntologyData ontologyData, 
                                                                RDFOntologyFact aFact, 
                                                                RDFOntologyFact bFact) {
            return (!ontologyData.CheckIsSameFactAs(aFact, bFact));
        }

        /// <summary>
        /// Checks if the given "aFact -> objectProperty -> bFact" has transitive assertions
        /// which would cause transitive cycles (unallowed concept in OWL-DL)
        /// </summary>
        internal static Boolean CheckTransitiveAssertionCompatibility(RDFOntologyData ontologyData,
                                                                      RDFOntologyFact aFact,
                                                                      RDFOntologyObjectProperty objectProperty,
                                                                      RDFOntologyFact bFact) {
            return !ontologyData.CheckIsTransitiveAssertionOf(bFact, objectProperty, aFact);
        }
        #endregion

    }

}