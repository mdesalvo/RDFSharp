/*
   Copyright 2012-2016 Marco De Salvo

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
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyReasoningHelper represents an helper for lite RDFS/OWL-DL reasoning tasks.
    /// </summary>
    public static class RDFOntologyReasoningHelper {

        #region Model

        #region ClassModel

        #region SubClassOf
        /// <summary>
        /// Checks if the given aClass is subClass of the given bClass within the given class model
        /// </summary>
        public static Boolean IsSubClassOf(RDFOntologyClass aClass, RDFOntologyClass bClass, RDFOntologyClassModel classModel) {
            return (aClass  != null && bClass != null && classModel != null ? RDFOntologyReasoningHelper.EnlistSuperClassesOf(aClass, classModel).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the subClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel EnlistSubClassesOf(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result       = new RDFOntologyClassModel();
            if (ontClass    != null && classModel != null) {

                //Step 1: Reason on the given class
                result       = RDFSemanticsUtilities.EnlistSubClassesOf_Core(ontClass, classModel);

                //Step 2: Reason on the equivalent classes of the given class
                foreach(var ec in RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(ontClass, classModel)) {
                    result   = result.UnionWith(RDFSemanticsUtilities.EnlistSubClassesOf_Core(ec, classModel));
                }

            }
            return result;
        }
        #endregion

        #region SuperClassOf
        /// <summary>
        /// Checks if the given aClass is superClass of the given bClass within the given class model
        /// </summary>
        public static Boolean IsSuperClassOf(RDFOntologyClass aClass, RDFOntologyClass bClass, RDFOntologyClassModel classModel) {
            return (aClass  != null && bClass != null && classModel != null ? RDFOntologyReasoningHelper.EnlistSubClassesOf(aClass, classModel).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the superClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel EnlistSuperClassesOf(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result       = new RDFOntologyClassModel();
            if (ontClass    != null && classModel != null) {

                //Step 1: Reason on the given class
                result       = RDFSemanticsUtilities.EnlistSuperClassesOf_Core(ontClass, classModel);

                //Step 2: Reason on the equivalent classes of the given class
                foreach(var ec in RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(ontClass, classModel)) {
                    result   = result.UnionWith(RDFSemanticsUtilities.EnlistSuperClassesOf_Core(ec, classModel));
                }

            }
            return result;
        }
        #endregion

        #region EquivalentClass
        /// <summary>
        /// Checks if the given aClass is equivalentClass of the given bClass within the given class model
        /// </summary>
        public static Boolean IsEquivalentClassOf(RDFOntologyClass aClass, RDFOntologyClass bClass, RDFOntologyClassModel classModel) {
            return(aClass != null && bClass != null && classModel != null ? RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(aClass, classModel).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the equivalentClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel EnlistEquivalentClassesOf(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result     = new RDFOntologyClassModel();
            if (ontClass  != null && classModel != null) {
                result     = RDFSemanticsUtilities.EnlistEquivalentClassesOf_Core(ontClass, classModel, null).RemoveClass(ontClass);
            }
            return result;
        }
        #endregion

        #region DisjointWith
        /// <summary>
        /// Checks if the given aClass is disjointClass with the given bClass within the given class model
        /// </summary>
        public static Boolean IsDisjointClassWith(RDFOntologyClass aClass, RDFOntologyClass bClass, RDFOntologyClassModel classModel) {
            return(aClass != null && bClass != null && classModel != null ? RDFOntologyReasoningHelper.EnlistDisjointClassesWith(aClass, classModel).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the disjointClasses with the given class within the given class model
        /// </summary>
        public   static RDFOntologyClassModel EnlistDisjointClassesWith(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result     = new RDFOntologyClassModel();
            if (ontClass  != null && classModel != null) {
                result     = RDFSemanticsUtilities.EnlistDisjointClassesWith_Core(ontClass, classModel, null).RemoveClass(ontClass);
            }
            return result;
        }
        #endregion

        #region Domain
        /// <summary>
        /// Checks if the given ontology class is domain of the given ontology property within the given ontology class model
        /// </summary>
        public static Boolean IsDomainClassOf(RDFOntologyClass domainClass, RDFOntologyProperty ontProperty, RDFOntologyClassModel classModel) {
            return (domainClass != null && ontProperty != null && classModel != null ? RDFOntologyReasoningHelper.EnlistDomainClassesOf(ontProperty, classModel).Classes.ContainsKey(domainClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the domain classes of the given property within the given ontology class model
        /// </summary>
        public static RDFOntologyClassModel EnlistDomainClassesOf(RDFOntologyProperty ontProperty, RDFOntologyClassModel classModel) { 
            var result           = new RDFOntologyClassModel();
            if (ontProperty     != null  && classModel != null) {
                if (ontProperty.Domain   != null) {
                    result       = RDFOntologyReasoningHelper.EnlistSubClassesOf(ontProperty.Domain, classModel)
                                       .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(ontProperty.Domain, classModel))
                                       .AddClass(ontProperty.Domain);
                }
            }
            return result;
        }
        #endregion

        #region Range
        /// <summary>
        /// Checks if the given ontology class is range of the given ontology property within the given ontology class model
        /// </summary>
        public static Boolean IsRangeClassOf(RDFOntologyClass rangeClass, RDFOntologyProperty ontProperty, RDFOntologyClassModel classModel) {
            return (rangeClass != null && ontProperty != null && classModel != null ? RDFOntologyReasoningHelper.EnlistRangeClassesOf(ontProperty, classModel).Classes.ContainsKey(rangeClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the range classes of the given property within the given ontology class model
        /// </summary>
        public static RDFOntologyClassModel EnlistRangeClassesOf(RDFOntologyProperty ontProperty, RDFOntologyClassModel classModel) {
            var result          = new RDFOntologyClassModel();
            if (ontProperty    != null && classModel != null) {
                if (ontProperty.Range  != null) {
                    result      = RDFOntologyReasoningHelper.EnlistSubClassesOf(ontProperty.Range, classModel)
                                     .UnionWith(RDFOntologyReasoningHelper.EnlistEquivalentClassesOf(ontProperty.Range, classModel))
                                     .AddClass(ontProperty.Range);
                }
            }
            return result;
        }
        #endregion

        #region Literal
        /// <summary>
        /// Checks if the given ontology class is compatible with 'rdfs:Literal' within the given class model
        /// </summary>
        public static Boolean IsLiteralCompatibleClass(RDFOntologyClass ontClass, RDFOntologyClassModel classModel) {
            var result    = false;
            if (ontClass != null && classModel != null) {
                result    = (ontClass.IsDataRangeClass()                                                                 ||
                             ontClass.Equals(RDFOntologyVocabulary.Classes.LITERAL)                                      ||
                             RDFOntologyReasoningHelper.IsSubClassOf(ontClass, RDFOntologyVocabulary.Classes.LITERAL, classModel) ||
                             RDFOntologyReasoningHelper.IsEquivalentClassOf(ontClass, RDFOntologyVocabulary.Classes.LITERAL, classModel));
            }
            return result;
        }
        #endregion

        #endregion

        #region PropertyModel

        #region SubPropertyOf
        /// <summary>
        /// Checks if the given aProperty is subProperty of the given bProperty within the given property model
        /// </summary>
        public static Boolean IsSubPropertyOf(RDFOntologyProperty aProperty, RDFOntologyProperty bProperty, RDFOntologyPropertyModel propertyModel) {
            return (aProperty != null && bProperty != null && propertyModel != null ? RDFOntologyReasoningHelper.EnlistSuperPropertiesOf(aProperty, propertyModel).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the sub properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel EnlistSubPropertiesOf(RDFOntologyProperty ontProperty, RDFOntologyPropertyModel propertyModel) {
            var result         = new RDFOntologyPropertyModel();
            if(ontProperty    != null && propertyModel != null) {

                //Step 1: Reason on the given property
                result         = RDFSemanticsUtilities.EnlistSubPropertiesOf_Core(ontProperty, propertyModel);

                //Step 2: Reason on the equivalent properties of the given property
                foreach(var   ep in RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(ontProperty, propertyModel)) {
                    result     = result.UnionWith(RDFSemanticsUtilities.EnlistSubPropertiesOf_Core(ep, propertyModel));
                }

            }
            return result;
        }
        #endregion

        #region SuperPropertyOf
        /// <summary>
        /// Checks if the given aProperty is superProperty of the given bProperty within the given property model
        /// </summary>
        public static Boolean IsSuperPropertyOf(RDFOntologyProperty aProperty, RDFOntologyProperty bProperty, RDFOntologyPropertyModel propertyModel) {
            return (aProperty != null && bProperty != null && propertyModel != null ? RDFOntologyReasoningHelper.EnlistSubPropertiesOf(aProperty, propertyModel).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the super properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel EnlistSuperPropertiesOf(RDFOntologyProperty ontProperty, RDFOntologyPropertyModel propertyModel) {
            var result         = new RDFOntologyPropertyModel();
            if(ontProperty    != null && propertyModel != null) {

                //Step 1: Reason on the given property
                result         = RDFSemanticsUtilities.EnlistSuperPropertiesOf_Core(ontProperty, propertyModel);

                //Step 2: Reason on the equivalent properties of the given property
                foreach(var   ep in RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(ontProperty, propertyModel)) {
                    result     = result.UnionWith(RDFSemanticsUtilities.EnlistSuperPropertiesOf_Core(ep, propertyModel));
                }

            }
            return result;
        }
        #endregion

        #region EquivalentProperty
        /// <summary>
        /// Checks if the given aProperty is equivalentProperty of the given bProperty within the given property model
        /// </summary>
        public static Boolean IsEquivalentPropertyOf(RDFOntologyProperty aProperty, RDFOntologyProperty bProperty, RDFOntologyPropertyModel propertyModel) {
            return (aProperty != null && bProperty != null && propertyModel != null ? RDFOntologyReasoningHelper.EnlistEquivalentPropertiesOf(aProperty, propertyModel).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the equivalentProperties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel EnlistEquivalentPropertiesOf(RDFOntologyProperty ontProperty, RDFOntologyPropertyModel propertyModel) {
            var result         = new RDFOntologyPropertyModel();
            if (ontProperty   != null && propertyModel != null) {
                result         = RDFSemanticsUtilities.EnlistEquivalentPropertiesOf_Core(ontProperty, propertyModel, null).RemoveProperty(ontProperty);
            }
            return result;
        }
        #endregion

        #region InverseOf
        /// <summary>
        /// Checks if the given aProperty is inverse property of the given bProperty within the given property model
        /// </summary>
        public static Boolean IsInversePropertyOf(RDFOntologyObjectProperty aProperty, RDFOntologyObjectProperty bProperty, RDFOntologyPropertyModel propertyModel) {
            return (aProperty != null && bProperty != null && propertyModel != null ? RDFOntologyReasoningHelper.EnlistInversePropertiesOf(aProperty, propertyModel).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the inverse properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel EnlistInversePropertiesOf(RDFOntologyObjectProperty ontProperty, RDFOntologyPropertyModel propertyModel) {
            var result              = new RDFOntologyPropertyModel();
            if (ontProperty        != null && propertyModel != null) {
                
                //Subject-side inverseOf relation
                foreach (var invOf in propertyModel.Relations.InverseOf.SelectEntriesBySubject(ontProperty)) {
                    result.AddProperty((RDFOntologyObjectProperty)invOf.TaxonomyObject);
                }

                //Object-side inverseOf relation
                foreach (var invOf in propertyModel.Relations.InverseOf.SelectEntriesByObject(ontProperty)) {
                    result.AddProperty((RDFOntologyObjectProperty)invOf.TaxonomySubject);
                }

                result.RemoveProperty(ontProperty); //Safety deletion
            }
            return result;
        }
        #endregion

        #endregion

        #endregion

        #region Data

        #region SameAs
        /// <summary>
        /// Checks if the given aFact is sameAs the given bFact within the given data
        /// </summary>
        public static Boolean IsSameFactAs(RDFOntologyFact aFact, RDFOntologyFact bFact, RDFOntologyData data) {
            return (aFact != null && bFact != null && data != null ? RDFOntologyReasoningHelper.EnlistSameFactsAs(aFact, data).Facts.ContainsKey(bFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the sameFacts of the given fact within the given data
        /// </summary>
        public static RDFOntologyData EnlistSameFactsAs(RDFOntologyFact ontFact, RDFOntologyData data) {
            var result     = new RDFOntologyData();
            if (ontFact   != null && data != null) {
                result     = RDFSemanticsUtilities.EnlistSameFactsAs_Core(ontFact, data, null).RemoveFact(ontFact);
            }
            return result;
        }
        #endregion

        #region DifferentFrom
        /// <summary>
        /// Checks if the given aFact is differentFrom the given bFact within the given data
        /// </summary>
        public static Boolean IsDifferentFactFrom(RDFOntologyFact aFact, RDFOntologyFact bFact, RDFOntologyData data) {
            return (aFact != null && bFact != null && data != null ? RDFOntologyReasoningHelper.EnlistDifferentFactsFrom(aFact, data).Facts.ContainsKey(bFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the different facts of the given fact within the given data
        /// </summary>
        public static RDFOntologyData EnlistDifferentFactsFrom(RDFOntologyFact ontFact, RDFOntologyData data) {
            var result     = new RDFOntologyData();
            if (ontFact   != null && data != null) {
                result     = RDFSemanticsUtilities.EnlistDifferentFactsFrom_Core(ontFact, data, null).RemoveFact(ontFact);
            }
            return result;
        }
        #endregion

        #region EnlistMembersOf
        /// <summary>
        /// Checks if the given fact is member of the given class within the given ontology
        /// </summary>
        public static Boolean IsMemberOf(RDFOntologyFact ontFact, RDFOntologyClass ontClass, RDFOntology ontology) { 
            return(ontFact != null && ontClass != null && ontology != null ? RDFOntologyReasoningHelper.EnlistMembersOf(ontClass, ontology).Facts.ContainsKey(ontFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the facts which are members of the given class within the given ontology
        /// </summary>
        public static RDFOntologyData EnlistMembersOf(RDFOntologyClass ontClass, RDFOntology ontology) {
            var result      = new RDFOntologyData();
            if (ontClass   != null && ontology != null) {
                
                //Restriction
                if (ontClass.IsRestrictionClass()) {
                    result  = RDFSemanticsUtilities.EnlistMembersOfRestriction((RDFOntologyRestriction)ontClass,  ontology);
                }

                //Enumeration
                else if (ontClass.IsEnumerateClass()) {
                    result  = RDFSemanticsUtilities.EnlistMembersOfEnumerate((RDFOntologyEnumerateClass)ontClass, ontology);
                }

                //DataRange
                else if (ontClass.IsDataRangeClass()) {
                    result  = RDFSemanticsUtilities.EnlistMembersOfDataRange((RDFOntologyDataRangeClass)ontClass, ontology);
                }

                //Composite
                else if (ontClass.IsCompositeClass()) {
                    result  = RDFSemanticsUtilities.EnlistMembersOfComposite(ontClass, ontology);
                }

                //SimpleClass
                else {
                    result  = RDFSemanticsUtilities.EnlistMembersOfClass(ontClass, ontology);
                }

            }
            return result;
        }
        #endregion

        #endregion

    }

}