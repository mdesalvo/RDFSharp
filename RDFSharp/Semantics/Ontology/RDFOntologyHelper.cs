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

using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFOntologyHelper contains utility methods supporting RDFS/OWL-DL modeling, validation and reasoning
    /// </summary>
    public static class RDFOntologyHelper {

		#region Model
	
		#region ClassModel
	
        #region SubClassOf
        /// <summary>
        /// Checks if the given aClass is subClass of the given bClass within the given class model
        /// </summary>
        public static Boolean CheckIsSubClassOf(this RDFOntologyClassModel classModel, RDFOntologyClass aClass, RDFOntologyClass bClass) {
            return (aClass      != null && bClass != null && classModel != null ? classModel.GetSuperClassesOf(aClass).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the subClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel GetSubClassesOf(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result           = new RDFOntologyClassModel();
            if (ontClass        != null && classModel != null) {

                //Step 1: Reason on the given class
                result           = classModel.GetSubClassesOfInternal(ontClass);

                //Step 2: Reason on the equivalent classes
                foreach (var ec in classModel.GetEquivalentClassesOf(ontClass)) {
                    result       = result.UnionWith(classModel.GetSubClassesOfInternal(ec));
                }

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "rdfs:subClassOf" taxonomy to discover direct and indirect subClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel GetSubClassesOfInternalVisitor(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result           = new RDFOntologyClassModel();

            // Transitivity of "rdfs:subClassOf" taxonomy: ((A SUBCLASSOF B)  &&  (B SUBCLASSOF C))  =>  (A SUBCLASSOF C)
            foreach (var sc     in classModel.Relations.SubClassOf.SelectEntriesByObject(ontClass)) {
                result.AddClass((RDFOntologyClass)sc.TaxonomySubject);
                result           = result.UnionWith(classModel.GetSubClassesOfInternalVisitor((RDFOntologyClass)sc.TaxonomySubject));
            }

            return result;
        }
        internal static RDFOntologyClassModel GetSubClassesOfInternal(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result1          = new RDFOntologyClassModel();
            var result2          = new RDFOntologyClassModel();

            // Step 1: Direct subsumption of "rdfs:subClassOf" taxonomy
            result1              = classModel.GetSubClassesOfInternalVisitor(ontClass);

            // Step 2: Enlist equivalent classes of subclasses
            result2              = result2.UnionWith(result1);
            foreach (var sc     in result1) {
                result2          = result2.UnionWith(classModel.GetEquivalentClassesOf(sc)
                                                               .UnionWith(classModel.GetSubClassesOf(sc)));
            }

            return result2;
        }
        #endregion

        #region SuperClassOf
        /// <summary>
        /// Checks if the given aClass is superClass of the given bClass within the given class model
        /// </summary>
        public static Boolean CheckIsSuperClassOf(this RDFOntologyClassModel classModel, RDFOntologyClass aClass, RDFOntologyClass bClass) {
            return (aClass      != null && bClass != null && classModel != null ? classModel.GetSubClassesOf(aClass).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the superClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel GetSuperClassesOf(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result           = new RDFOntologyClassModel();
            if (ontClass        != null && classModel != null) {

                //Step 1: Reason on the given class
                result           = classModel.GetSuperClassesOfInternal(ontClass);

                //Step 2: Reason on the equivalent classes
                foreach (var ec in classModel.GetEquivalentClassesOf(ontClass)) {
                    result       = result.UnionWith(classModel.GetSuperClassesOfInternal(ec));
                }

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "rdfs:subClassOf" taxonomy to discover direct and indirect superClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel GetSuperClassesOfInternalVisitor(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result           = new RDFOntologyClassModel();

            // Transitivity of "rdfs:subClassOf" taxonomy: ((A SUPERCLASSOF B)  &&  (B SUPERCLASSOF C))  =>  (A SUPERCLASSOF C)
            foreach (var sc     in classModel.Relations.SubClassOf.SelectEntriesBySubject(ontClass)) {
                result.AddClass((RDFOntologyClass)sc.TaxonomyObject);
                result           = result.UnionWith(classModel.GetSuperClassesOfInternalVisitor((RDFOntologyClass)sc.TaxonomyObject));
            }

            return result;
        }
        internal static RDFOntologyClassModel GetSuperClassesOfInternal(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result1          = new RDFOntologyClassModel();
            var result2          = new RDFOntologyClassModel();

            // Step 1: Direct subsumption of "rdfs:subClassOf" taxonomy
            result1              = classModel.GetSuperClassesOfInternalVisitor(ontClass);

            // Step 2: Enlist equivalent classes of superclasses
            result2              = result2.UnionWith(result1);
            foreach (var sc     in result1) {
                result2          = result2.UnionWith(classModel.GetEquivalentClassesOf(sc)
                                                               .UnionWith(classModel.GetSuperClassesOf(sc)));
            }

            return result2;
        }
        #endregion

        #region EquivalentClass
        /// <summary>
        /// Checks if the given aClass is equivalentClass of the given bClass within the given class model
        /// </summary>
        public static Boolean CheckIsEquivalentClassOf(this RDFOntologyClassModel classModel, RDFOntologyClass aClass, RDFOntologyClass bClass) {
            return (aClass != null && bClass != null && classModel != null ? classModel.GetEquivalentClassesOf(aClass).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the equivalentClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel GetEquivalentClassesOf(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result      = new RDFOntologyClassModel();
            if (ontClass   != null && classModel != null) {
                result      = classModel.GetEquivalentClassesOfInternal(ontClass, null)
                                        .RemoveClass(ontClass); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:equivalentClass" taxonomy to discover direct and indirect equivalentClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel GetEquivalentClassesOfInternal(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass, Dictionary<Int64, RDFOntologyClass> visitContext) {
            var result        = new RDFOntologyClassModel();

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyClass>() { { ontClass.PatternMemberID, ontClass } };
            }
            else {
                if (!visitContext.ContainsKey(ontClass.PatternMemberID)) {
                     visitContext.Add(ontClass.PatternMemberID, ontClass);
                }
                else {
                     return result;
                }
            }
            #endregion

            // Transitivity of "owl:equivalentClass" taxonomy: ((A EQUIVALENTCLASSOF B)  &&  (B EQUIVALENTCLASS C))  =>  (A EQUIVALENTCLASS C)
            foreach (var  ec in classModel.Relations.EquivalentClass.SelectEntriesBySubject(ontClass)) {
                result.AddClass((RDFOntologyClass)ec.TaxonomyObject);
                result        = result.UnionWith(classModel.GetEquivalentClassesOfInternal((RDFOntologyClass)ec.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #region DisjointWith
        /// <summary>
        /// Checks if the given aClass is disjointClass with the given bClass within the given class model
        /// </summary>
        public static Boolean CheckIsDisjointClassWith(this RDFOntologyClassModel classModel, RDFOntologyClass aClass, RDFOntologyClass bClass) {
            return (aClass != null && bClass != null && classModel != null ? classModel.GetDisjointClassesWith(aClass).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the disjointClasses with the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel GetDisjointClassesWith(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result        = new RDFOntologyClassModel();
            if (ontClass     != null && classModel != null) {
                result        = classModel.GetDisjointClassesWithInternal(ontClass, null)
                                          .RemoveClass(ontClass); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:disjointWith" taxonomy to discover direct and indirect disjointClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel GetDisjointClassesWithInternal(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass, Dictionary<Int64, RDFOntologyClass> visitContext) {
            var result1       = new RDFOntologyClassModel();
            var result2       = new RDFOntologyClassModel();

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyClass>() { { ontClass.PatternMemberID, ontClass } };
            }
            else {
                if (!visitContext.ContainsKey(ontClass.PatternMemberID)) {
                     visitContext.Add(ontClass.PatternMemberID, ontClass);
                }
                else {
                     return result1;
                }
            }
            #endregion

            // Inference: ((A DISJOINTWITH B)   &&  (B EQUIVALENTCLASS C))  =>  (A DISJOINTWITH C)
            foreach (var  dw in classModel.Relations.DisjointWith.SelectEntriesBySubject(ontClass)) {
                result1.AddClass((RDFOntologyClass)dw.TaxonomyObject);
                result1       = result1.UnionWith(classModel.GetEquivalentClassesOfInternal((RDFOntologyClass)dw.TaxonomyObject, visitContext));
            }

            // Inference: ((A DISJOINTWITH B)   &&  (B SUPERCLASS C))  =>  (A DISJOINTWITH C)
            result2           = result2.UnionWith(result1);
            foreach (var   c in result1) {
                result2       = result2.UnionWith(classModel.GetSubClassesOfInternal(c));
            }
            result1           = result1.UnionWith(result2);

            // Inference: ((A EQUIVALENTCLASS B || A SUBCLASSOF B)  &&  (B DISJOINTWITH C))     =>  (A DISJOINTWITH C)
            var compatibleCls = classModel.GetSuperClassesOf(ontClass)
                                          .UnionWith(classModel.GetEquivalentClassesOf(ontClass));
            foreach (var  ec in compatibleCls) {
                result1       = result1.UnionWith(classModel.GetDisjointClassesWithInternal(ec, visitContext));
            }

            return result1;
        }
        #endregion

        #region Domain
        /// <summary>
        /// Checks if the given ontology class is domain of the given ontology property within the given ontology class model
        /// </summary>
        public static Boolean CheckIsDomainOf(this RDFOntologyClassModel classModel, RDFOntologyClass domainClass, RDFOntologyProperty ontProperty) {
            return (domainClass        != null && ontProperty != null && classModel != null ? classModel.GetDomainOf(ontProperty).Classes.ContainsKey(domainClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the domain classes of the given property within the given ontology class model
        /// </summary>
        public static RDFOntologyClassModel GetDomainOf(this RDFOntologyClassModel classModel, RDFOntologyProperty ontProperty) {
            var result                  = new RDFOntologyClassModel();
            if (ontProperty            != null && classModel != null) {
                if (ontProperty.Domain != null) {
                    result              = classModel.GetSubClassesOf(ontProperty.Domain)
                                                    .UnionWith(classModel.GetEquivalentClassesOf(ontProperty.Domain))
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
        public static Boolean CheckIsRangeOf(this RDFOntologyClassModel classModel, RDFOntologyClass rangeClass, RDFOntologyProperty ontProperty) {
            return (rangeClass        != null && ontProperty != null && classModel != null ? classModel.GetRangeOf(ontProperty).Classes.ContainsKey(rangeClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the range classes of the given property within the given ontology class model
        /// </summary>
        public static RDFOntologyClassModel GetRangeOf(this RDFOntologyClassModel classModel, RDFOntologyProperty ontProperty) {
            var result                 = new RDFOntologyClassModel();
            if (ontProperty           != null && classModel != null) {
                if (ontProperty.Range != null) {
                    result             = classModel.GetSubClassesOf(ontProperty.Range)
                                                   .UnionWith(classModel.GetEquivalentClassesOf(ontProperty.Range))
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
        public static Boolean CheckIsLiteralCompatible(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass) {
            var result    = false;
            if (ontClass != null && classModel != null) {
                result    = (ontClass.IsDataRangeClass()
                                || ontClass.Equals(RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass())
                                    || classModel.CheckIsSubClassOf(ontClass, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
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
        public static Boolean CheckIsSubPropertyOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty aProperty, RDFOntologyProperty bProperty) {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetSuperPropertiesOf(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the sub properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetSubPropertiesOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty) {
            var result         = new RDFOntologyPropertyModel();
            if (ontProperty   != null && propertyModel != null) {

                //Step 1: Reason on the given property
                result         = propertyModel.GetSubPropertiesOfInternal(ontProperty);

                //Step 2: Reason on the equivalent properties
                foreach (var  ep in propertyModel.GetEquivalentPropertiesOf(ontProperty)) {
                    result     = result.UnionWith(propertyModel.GetSubPropertiesOfInternal(ep));
                }

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "rdfs:subPropertyOf" taxonomy to discover direct and indirect subProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel GetSubPropertiesOfInternalVisitor(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty) {
            var result         = new RDFOntologyPropertyModel();

            // Transitivity of "rdfs:subPropertyOf" taxonomy: ((A SUBPROPERTYOF B)  &&  (B SUBPROPERTYOF C))  =>  (A SUBPROPERTYOF C)
            foreach (var   sp in propertyModel.Relations.SubPropertyOf.SelectEntriesByObject(ontProperty)) {
                result.AddProperty((RDFOntologyProperty)sp.TaxonomySubject);
                result         = result.UnionWith(propertyModel.GetSubPropertiesOfInternalVisitor((RDFOntologyProperty)sp.TaxonomySubject));
            }

            return result;
        }
        internal static RDFOntologyPropertyModel GetSubPropertiesOfInternal(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty) {
            var result1        = new RDFOntologyPropertyModel();
            var result2        = new RDFOntologyPropertyModel();

            // Step 1: Direct subsumption of "rdfs:subPropertyOf" taxonomy
            result1            = propertyModel.GetSubPropertiesOfInternalVisitor(ontProperty);

            // Step 2: Enlist equivalent properties of subproperties
            result2            = result2.UnionWith(result1);
            foreach (var   sp in result1) {
                result2        = result2.UnionWith(propertyModel.GetEquivalentPropertiesOf(sp)
                                                                .UnionWith(propertyModel.GetSubPropertiesOf(sp)));
            }

            return result2;
        }
        #endregion

        #region SuperPropertyOf
        /// <summary>
        /// Checks if the given aProperty is superProperty of the given bProperty within the given property model
        /// </summary>
        public static Boolean CheckIsSuperPropertyOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty aProperty, RDFOntologyProperty bProperty) {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetSubPropertiesOf(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the super properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetSuperPropertiesOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty) {
            var result         = new RDFOntologyPropertyModel();
            if (ontProperty   != null && propertyModel != null) {

                //Step 1: Reason on the given property
                result         = propertyModel.GetSuperPropertiesOfInternal(ontProperty);

                //Step 2: Reason on the equivalent properties
                foreach (var  ep in propertyModel.GetEquivalentPropertiesOf(ontProperty)) {
                    result     = result.UnionWith(propertyModel.GetSuperPropertiesOfInternal(ep));
                }

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "rdfs:subPropertyOf" taxonomy to discover direct and indirect superProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel GetSuperPropertiesOfInternalVisitor(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty) {
            var result         = new RDFOntologyPropertyModel();

            // Transitivity of "rdfs:subPropertyOf" taxonomy: ((A SUPERPROPERTYOF B)  &&  (B SUPERPROPERTYOF C))  =>  (A SUPERPROPERTYOF C)
            foreach (var   sp in propertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(ontProperty)) {
                result.AddProperty((RDFOntologyProperty)sp.TaxonomyObject);
                result         = result.UnionWith(propertyModel.GetSuperPropertiesOfInternalVisitor((RDFOntologyProperty)sp.TaxonomyObject));
            }

            return result;
        }
        internal static RDFOntologyPropertyModel GetSuperPropertiesOfInternal(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty) {
            var result1        = new RDFOntologyPropertyModel();
            var result2        = new RDFOntologyPropertyModel();

            // Step 1: Direct subsumption of "rdfs:subPropertyOf" taxonomy
            result1            = propertyModel.GetSuperPropertiesOfInternalVisitor(ontProperty);

            // Step 2: Enlist equivalent properties of subproperties
            result2            = result2.UnionWith(result1);
            foreach (var sp in result1) {
                result2        = result2.UnionWith(propertyModel.GetEquivalentPropertiesOf(sp)
                                                                .UnionWith(propertyModel.GetSuperPropertiesOf(sp)));
            }

            return result2;
        }
        #endregion

        #region EquivalentProperty
        /// <summary>
        /// Checks if the given aProperty is equivalentProperty of the given bProperty within the given property model
        /// </summary>
        public static Boolean CheckIsEquivalentPropertyOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty aProperty, RDFOntologyProperty bProperty) {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetEquivalentPropertiesOf(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the equivalentProperties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetEquivalentPropertiesOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty) {
            var result         = new RDFOntologyPropertyModel();
            if (ontProperty   != null && propertyModel != null) {
                result         = propertyModel.GetEquivalentPropertiesOfInternal(ontProperty, null)
                                              .RemoveProperty(ontProperty); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:equivalentProperty" taxonomy to discover direct and indirect equivalentProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel GetEquivalentPropertiesOfInternal(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty, Dictionary<Int64, RDFOntologyProperty> visitContext) {
            var result         = new RDFOntologyPropertyModel();

            #region visitContext
            if (visitContext  == null) {
                visitContext   = new Dictionary<Int64, RDFOntologyProperty>() { { ontProperty.PatternMemberID, ontProperty } };
            }
            else {
                if (!visitContext.ContainsKey(ontProperty.PatternMemberID)) {
                     visitContext.Add(ontProperty.PatternMemberID, ontProperty);
                }
                else {
                     return result;
                }
            }
            #endregion

            // Transitivity of "owl:equivalentProperty" taxonomy: ((A EQUIVALENTPROPERTY B)  &&  (B EQUIVALENTPROPERTY C))  =>  (A EQUIVALENTPROPERTY C)
            foreach (var  ep  in propertyModel.Relations.EquivalentProperty.SelectEntriesBySubject(ontProperty)) {
                result.AddProperty((RDFOntologyProperty)ep.TaxonomyObject);
                result         = result.UnionWith(propertyModel.GetEquivalentPropertiesOfInternal((RDFOntologyProperty)ep.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #region InverseOf
        /// <summary>
        /// Checks if the given aProperty is inverse property of the given bProperty within the given property model
        /// </summary>
        public static Boolean CheckIsInversePropertyOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyObjectProperty aProperty, RDFOntologyObjectProperty bProperty) {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetInversePropertiesOf(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the inverse properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetInversePropertiesOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyObjectProperty ontProperty) {
            var result         = new RDFOntologyPropertyModel();
            if (ontProperty   != null && propertyModel != null) {

                //Step 1: Reason on the given property
                //Subject-side inverseOf relation
                foreach (var invOf in propertyModel.Relations.InverseOf.SelectEntriesBySubject(ontProperty)) {
                    result.AddProperty((RDFOntologyObjectProperty)invOf.TaxonomyObject);
                }
                //Object-side inverseOf relation
                foreach (var invOf in propertyModel.Relations.InverseOf.SelectEntriesByObject(ontProperty))  {
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
        public static Boolean CheckIsSameFactAs(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyFact bFact) {
            return (aFact     != null && bFact != null && data != null ? data.GetSameFactsAs(aFact).Facts.ContainsKey(bFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the sameFacts of the given fact within the given data
        /// </summary>
        public static RDFOntologyData GetSameFactsAs(this RDFOntologyData data, RDFOntologyFact ontFact) {
            var result         = new RDFOntologyData();
            if (ontFact       != null && data != null) {
                result         = data.GetSameFactsAsInternal(ontFact, null)
                                     .RemoveFact(ontFact); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:sameAs" taxonomy to discover direct and indirect samefacts of the given facts
        /// </summary>
        internal static RDFOntologyData GetSameFactsAsInternal(this RDFOntologyData data, RDFOntologyFact ontFact, Dictionary<Int64, RDFOntologyFact> visitContext) {
            var result         = new RDFOntologyData();

            #region visitContext
            if (visitContext  == null) {
                visitContext   = new Dictionary<Int64, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else {
                if (!visitContext.ContainsKey(ontFact.PatternMemberID)) {
                     visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else {
                     return result;
                }
            }
            #endregion

            // Transitivity of "owl:sameAs" taxonomy: ((A SAMEAS B)  &&  (B SAMEAS C))  =>  (A SAMEAS C)
            foreach (var   sf in data.Relations.SameAs.SelectEntriesBySubject(ontFact)) {
                result.AddFact((RDFOntologyFact)sf.TaxonomyObject);
                result         = result.UnionWith(data.GetSameFactsAsInternal((RDFOntologyFact)sf.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #region DifferentFrom
        /// <summary>
        /// Checks if the given aFact is differentFrom the given bFact within the given data
        /// </summary>
        public static Boolean CheckIsDifferentFactFrom(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyFact bFact) {
            return (aFact      != null && bFact != null && data != null ? data.GetDifferentFactsFrom(aFact).Facts.ContainsKey(bFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the different facts of the given fact within the given data
        /// </summary>
        public static RDFOntologyData GetDifferentFactsFrom(this RDFOntologyData data, RDFOntologyFact ontFact) {
            var result          = new RDFOntologyData();
            if (ontFact        != null && data != null) {
                result          = data.GetDifferentFactsFromInternal(ontFact, null)
                                      .RemoveFact(ontFact); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:differentFrom" taxonomy to discover direct and indirect differentFacts of the given facts
        /// </summary>
        internal static RDFOntologyData GetDifferentFactsFromInternal(this RDFOntologyData data, RDFOntologyFact ontFact, Dictionary<Int64, RDFOntologyFact> visitContext) {
            var result         = new RDFOntologyData();

            #region visitContext
            if (visitContext  == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else {
                if (!visitContext.ContainsKey(ontFact.PatternMemberID)) {
                     visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else {
                     return result;
                }
            }
            #endregion

            // Inference: (A DIFFERENTFROM B  &&  B SAMEAS C         =>  A DIFFERENTFROM C)
            foreach (var   df in data.Relations.DifferentFrom.SelectEntriesBySubject(ontFact)) {
                result.AddFact((RDFOntologyFact)df.TaxonomyObject);
                result         = result.UnionWith(data.GetSameFactsAsInternal((RDFOntologyFact)df.TaxonomyObject, visitContext));
            }

            // Inference: (A SAMEAS B         &&  B DIFFERENTFROM C  =>  A DIFFERENTFROM C)
            foreach (var   sa in data.GetSameFactsAs(ontFact)) {
                result         = result.UnionWith(data.GetDifferentFactsFromInternal(sa, visitContext));
            }

            return result;
        }
        #endregion

        #region TransitiveProperty
        /// <summary>
        /// Checks if the given "aFact -> transProp" assertion links to the given bFact within the given data
        /// </summary>
        public static Boolean CheckIsTransitiveAssertionOf(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyObjectProperty transProp, RDFOntologyFact bFact) {
            return (aFact  != null && transProp != null && transProp.IsTransitiveProperty() && bFact != null && data != null ? data.GetTransitiveAssertionsOf(aFact, transProp).Facts.ContainsKey(bFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the given "aFact -> transOntProp" assertions within the given data
        /// </summary>
        public static RDFOntologyData GetTransitiveAssertionsOf(this RDFOntologyData data, RDFOntologyFact ontFact, RDFOntologyObjectProperty transOntProp) {
            var result     = new RDFOntologyData();
            if (ontFact   != null && transOntProp != null && transOntProp.IsTransitiveProperty() && data != null) {
                result       = data.GetTransitiveAssertionsOfInternal(ontFact, transOntProp, null);
            }
            return result;
        }

        /// <summary>
        /// Enlists the transitive assertions of the given fact and the given property within the given data
        /// </summary>
        internal static RDFOntologyData GetTransitiveAssertionsOfInternal(this RDFOntologyData data, RDFOntologyFact ontFact, RDFOntologyObjectProperty ontProp, Dictionary<Int64, RDFOntologyFact> visitContext) {
            var result        = new RDFOntologyData();

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else {
                if (!visitContext.ContainsKey(ontFact.PatternMemberID)) {
                     visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else {
                     return result;
                }
            }
            #endregion

            // ((F1 P F2)    &&  (F2 P F3))  =>  (F1 P F3)
            foreach (var  ta in data.Relations.Assertions.SelectEntriesBySubject(ontFact)
                                                         .SelectEntriesByPredicate(ontProp)) {
                result.AddFact((RDFOntologyFact)ta.TaxonomyObject);
                result        = result.UnionWith(data.GetTransitiveAssertionsOfInternal((RDFOntologyFact)ta.TaxonomyObject, ontProp, visitContext));
            }

            return result;
        }
        #endregion

        #region MemberOf
        /// <summary>
        /// Checks if the given fact is member of the given class within the given ontology
        /// </summary>
        public static Boolean CheckIsMemberOf(this RDFOntology ontology, RDFOntologyFact ontFact, RDFOntologyClass ontClass) {
            return (ontFact != null && ontClass != null && ontology != null ? ontology.GetMembersOf(ontClass).Facts.ContainsKey(ontFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the facts which are members of the given class within the given ontology
        /// </summary>
        public static RDFOntologyData GetMembersOf(this RDFOntology ontology, RDFOntologyClass ontClass) {
            var result       = new RDFOntologyData();
            if (ontClass    != null && ontology != null) {

                //Expand ontology
                var expOnt   = ontology.UnionWith(RDFBASEOntology.Instance);

                //DataRange/Literal-Compatible
                if (expOnt.Model.ClassModel.CheckIsLiteralCompatible(ontClass)) {
                    result   = expOnt.GetMembersOfLiteralCompatibleClass(ontClass);
                }

                //Restriction/Composite/Enumerate/Class
                else {
                    result   = expOnt.GetMembersOfNonLiteralCompatibleClass(ontClass);
                }

            }
            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given class within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfClass(this RDFOntology ontology, RDFOntologyClass ontClass) {
            var result           = new RDFOntologyData();

            //Get the compatible classes
            var compCls          = ontology.Model.ClassModel.GetSubClassesOf(ontClass)
                                                            .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf(ontClass))
                                                            .AddClass(ontClass);

            //Filter "classType" relations made with compatible classes
            var fTaxonomy        = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            foreach (var c      in compCls) {
                fTaxonomy        = fTaxonomy.UnionWith(ontology.Data.Relations.ClassType.SelectEntriesByObject(c));
            }
            foreach (var tEntry in fTaxonomy) {

                //Add the fact and its synonyms
                if (tEntry.TaxonomySubject.IsFact()) {
                    result       = result.UnionWith(ontology.Data.GetSameFactsAs((RDFOntologyFact)tEntry.TaxonomySubject))
                                         .AddFact((RDFOntologyFact)tEntry.TaxonomySubject);
                }

            }

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given composition within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfComposite(this RDFOntology ontology, RDFOntologyClass ontCompClass) {
            var result               = new RDFOntologyData();

            #region Intersection
            if (ontCompClass        is RDFOntologyIntersectionClass) {

                //Filter "intersectionOf" relations made with the given intersection class
                var firstIter        = true;
                var iTaxonomy        = ontology.Model.ClassModel.Relations.IntersectionOf.SelectEntriesBySubject(ontCompClass);
                foreach (var tEntry in iTaxonomy) {
                    if (firstIter)   {
                        result       = ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject);
                        firstIter    = false;
                    }
                    else {
                        result       = result.IntersectWith(ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject));
                    }
                }

            }
            #endregion

            #region Union
            else if (ontCompClass   is RDFOntologyUnionClass) {

                //Filter "unionOf" relations made with the given union class
                var uTaxonomy        = ontology.Model.ClassModel.Relations.UnionOf.SelectEntriesBySubject(ontCompClass);
                foreach (var tEntry in uTaxonomy) {
                    result           = result.UnionWith(ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject));
                }

            }
            #endregion

            #region Complement
            else if (ontCompClass   is RDFOntologyComplementClass) {
                result               = ontology.Data.DifferenceWith(ontology.GetMembersOf(((RDFOntologyComplementClass)ontCompClass).ComplementOf));
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given enumeration within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfEnumerate(this RDFOntology ontology, RDFOntologyEnumerateClass ontEnumClass) {
            var result           = new RDFOntologyData();

            //Filter "oneOf" relations made with the given enumerate class
            var enTaxonomy       = ontology.Model.ClassModel.Relations.OneOf.SelectEntriesBySubject(ontEnumClass);
            foreach (var tEntry in enTaxonomy) {

                //Add the fact and its synonyms
                if (tEntry.TaxonomySubject.IsEnumerateClass() && tEntry.TaxonomyObject.IsFact()) {
                    result       = result.UnionWith(ontology.Data.GetSameFactsAs((RDFOntologyFact)tEntry.TaxonomyObject))
                                         .AddFact((RDFOntologyFact)tEntry.TaxonomyObject);
                }

            }

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given restriction within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfRestriction(this RDFOntology ontology, RDFOntologyRestriction ontRestriction) {
            var result          = new RDFOntologyData();

            //Enlist the properties which are compatible with the restriction's "OnProperty"
            var compProps       = ontology.Model.PropertyModel.GetSubPropertiesOf(ontRestriction.OnProperty)
                                                              .UnionWith(ontology.Model.PropertyModel.GetEquivalentPropertiesOf(ontRestriction.OnProperty))
                                                              .AddProperty(ontRestriction.OnProperty);

            //Filter assertions made with enlisted compatible properties
            var fTaxonomy       = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            foreach (var p     in compProps) {
                fTaxonomy       = fTaxonomy.UnionWith(ontology.Data.Relations.Assertions.SelectEntriesByPredicate(p));
            }

            #region Cardinality
            if (ontRestriction is RDFOntologyCardinalityRestriction) {

                //Item2 is a counter for occurrences of the restricted property within the subject fact
                var fCount      = new Dictionary<Int64, Tuple<RDFOntologyFact, Int64>>();

                //Iterate the compatible assertions
                foreach (var tEntry in fTaxonomy) {
                    if (!fCount.ContainsKey(tEntry.TaxonomySubject.PatternMemberID)) {
                         fCount.Add(tEntry.TaxonomySubject.PatternMemberID, new Tuple<RDFOntologyFact, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, 1));
                    }
                    else {
                         var occurrencyCounter = fCount[tEntry.TaxonomySubject.PatternMemberID].Item2;
                         fCount[tEntry.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, occurrencyCounter + 1);
                    }
                }

                //Apply the cardinality restriction on the tracked facts
                var fCountEnum  = fCount.Values.GetEnumerator();
                while (fCountEnum.MoveNext())    {
                    var passesMinCardinality     = true;
                    var passesMaxCardinality     = true;

                    //MinCardinality: signal tracked facts having "#occurrences < MinCardinality"
                    if (((RDFOntologyCardinalityRestriction)ontRestriction).MinCardinality > 0) {
                        if (fCountEnum.Current.Item2 < ((RDFOntologyCardinalityRestriction)ontRestriction).MinCardinality) {
                            passesMinCardinality = false;
                        }
                    }

                    //MaxCardinality: signal tracked facts having "#occurrences > MaxCardinality"
                    if (((RDFOntologyCardinalityRestriction)ontRestriction).MaxCardinality > 0) {
                        if (fCountEnum.Current.Item2 > ((RDFOntologyCardinalityRestriction)ontRestriction).MaxCardinality) {
                            passesMaxCardinality = false;
                        }
                    }

                    //Save the candidate fact if it passes cardinality restrictions
                    if (passesMinCardinality    && passesMaxCardinality) {
                        result.AddFact(fCountEnum.Current.Item1);
                    }
                }

            }
            #endregion

            #region AllValuesFrom/SomeValuesFrom
            else if (ontRestriction is RDFOntologyAllValuesFromRestriction || ontRestriction is RDFOntologySomeValuesFromRestriction) {

                //Item2 is a counter for occurrences of the restricted property with a range member of the restricted "FromClass"
                //Item3 is a counter for occurrences of the restricted property with a range member not of the restricted "FromClass"
                var fCount      = new Dictionary<Int64, Tuple<RDFOntologyFact, Int64, Int64>>();

                //Enlist the classes which are compatible with the restricted "FromClass"
                var compClasses = ontRestriction is RDFOntologyAllValuesFromRestriction 
                                    ? ontology.Model.ClassModel.GetSubClassesOf(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass)
                                                               .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass))
                                                               .AddClass(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass)
                                    : ontology.Model.ClassModel.GetSubClassesOf(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass)
                                                               .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass))
                                                               .AddClass(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass);

                //Iterate the compatible assertions
                foreach (var tEntry in fTaxonomy) {

                    //Initialize the occurrence counters of the subject fact
                    if (!fCount.ContainsKey(tEntry.TaxonomySubject.PatternMemberID)) {
                         fCount.Add(tEntry.TaxonomySubject.PatternMemberID, new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, 0, 0));
                    }

                    //Iterate the class types of the object fact, checking presence of the restricted "FromClass"
                    var fromClassFound             = false;
                    var objFactClassTypes          = ontology.Data.Relations.ClassType.SelectEntriesBySubject(tEntry.TaxonomyObject);
                    foreach (var objFactClassType in objFactClassTypes) {
                        var compObjFactClassTypes  = ontology.Model.ClassModel.GetSuperClassesOf((RDFOntologyClass)objFactClassType.TaxonomyObject)
                                                                              .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf((RDFOntologyClass)objFactClassType.TaxonomyObject))
                                                                              .AddClass((RDFOntologyClass)objFactClassType.TaxonomyObject);
                        if (compObjFactClassTypes.IntersectWith(compClasses).ClassesCount > 0) {
                            fromClassFound         = true;
                            break;
                        }
                    }

                    //Update the occurrence counters of the subject fact
                    var equalityCounter            = fCount[tEntry.TaxonomySubject.PatternMemberID].Item2;
                    var differenceCounter          = fCount[tEntry.TaxonomySubject.PatternMemberID].Item3;
                    if (fromClassFound) {
                        fCount[tEntry.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, equalityCounter + 1, differenceCounter);
                    }
                    else {
                        fCount[tEntry.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, Int64, Int64>((RDFOntologyFact)tEntry.TaxonomySubject, equalityCounter, differenceCounter + 1);
                    }

                }

                //Apply the restriction on the subject facts
                var fCountEnum                     = fCount.Values.GetEnumerator();
                while  (fCountEnum.MoveNext())     {
                    //AllValuesFrom
                    if (ontRestriction is RDFOntologyAllValuesFromRestriction) {
                        if (fCountEnum.Current.Item2 >= 1 && fCountEnum.Current.Item3 == 0) {
                            result.AddFact(fCountEnum.Current.Item1);
                        }
                    }
                    //SomeValuesFrom
                    else {
                        if (fCountEnum.Current.Item2 >= 1) {
                            result.AddFact(fCountEnum.Current.Item1);
                        }
                    }
                }

            }
            #endregion

            #region HasValue
            else if (ontRestriction     is RDFOntologyHasValueRestriction) {
                if (((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue.IsFact()) {

                    //Enlist the same facts of the restriction's "RequiredValue"
                    var compFacts        = ontology.Data.GetSameFactsAs((RDFOntologyFact)((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue)
                                                        .AddFact((RDFOntologyFact)((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue);

                    //Iterate the compatible assertions
                    foreach (var tEntry in fTaxonomy) {
                        if (tEntry.TaxonomyObject.IsFact()) {
                            if (compFacts.SelectFact(tEntry.TaxonomyObject.ToString()) != null) {
                                result.AddFact((RDFOntologyFact)tEntry.TaxonomySubject);
                            }
                        }
                    }

                }
                else if (((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue.IsLiteral()) {

                    //Iterate the compatible assertions and track the occurrence informations
                    foreach (var tEntry in fTaxonomy) {
                        if (tEntry.TaxonomyObject.IsLiteral()) {
                            try {
                                var semanticLiteralsCompare  = RDFQueryUtilities.CompareRDFPatternMembers(((RDFOntologyHasValueRestriction)ontRestriction).RequiredValue.Value, tEntry.TaxonomyObject.Value);
                                if (semanticLiteralsCompare == 0) {
                                    result.AddFact((RDFOntologyFact)tEntry.TaxonomySubject);
                                }
                            }
                            finally { }
                        }
                    }

                }
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Enlists the literals which are members of the given literal-compatible class within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfLiteralCompatibleClass(this RDFOntology ontology, RDFOntologyClass ontClass) {
            var result         = new RDFOntologyData();

            #region DataRange
            if (ontClass.IsDataRangeClass()) {

                //Filter "oneOf" relations made with the given datarange class
                var drTaxonomy = ontology.Model.ClassModel.Relations.OneOf.SelectEntriesBySubject(ontClass);
                foreach (var tEntry in drTaxonomy) {

                    //Add the literal
                    if (tEntry.TaxonomySubject.IsDataRangeClass() && tEntry.TaxonomyObject.IsLiteral()) {
                        result.AddLiteral((RDFOntologyLiteral)tEntry.TaxonomyObject);
                    }

                }

            }
            #endregion

            #region Literal
            //Asking for "rdfs:Literal" is the only way to get enlistment of plain literals, since they have really no semantic
            else if (ontClass.Equals(RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass())) {
                foreach (var ontLit in ontology.Data.Literals.Values) {
                    result.AddLiteral(ontLit);
                }
            }
            #endregion

            #region SubLiteral
            else {
                foreach (var ontLit in ontology.Data.Literals.Values.Where(l => l.Value is RDFTypedLiteral)) {
                    var dTypeClass   = ontology.Model.ClassModel.SelectClass(RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)ontLit.Value).Datatype));
                    if (dTypeClass  != null) {
                        if (dTypeClass.Equals(ontClass)
                                || ontology.Model.ClassModel.CheckIsSubClassOf(dTypeClass, ontClass)
                                    || ontology.Model.ClassModel.CheckIsEquivalentClassOf(dTypeClass, ontClass)) {
                            result.AddLiteral(ontLit);
                        }
                    }
                }
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given non literal-compatible class within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfNonLiteralCompatibleClass(this RDFOntology ontology, RDFOntologyClass ontClass) {
            var result     = new RDFOntologyData();
            if (ontClass  != null && ontology != null) {

                //Restriction
                if (ontClass.IsRestrictionClass()) {
                    result = ontology.GetMembersOfRestriction((RDFOntologyRestriction)ontClass);
                }

                //Composite
                else if (ontClass.IsCompositeClass()) {
                    result = ontology.GetMembersOfComposite(ontClass);
                }

                //Enumerate
                else if (ontClass.IsEnumerateClass()) {
                    result = ontology.GetMembersOfEnumerate((RDFOntologyEnumerateClass)ontClass);
                }

                //Class
                else {
                    result = ontology.GetMembersOfClass(ontClass);
                }

            }
            return result;
        }
        #endregion
		
		#endregion

		#region Inferences
		
		#region GetInferences
		/// <summary>
		/// Gets an ontology made by semantic inferences found in the given one
		/// </summary>
		public static RDFOntology GetInferences(this RDFOntology ontology) {
			var result       = new RDFOntology((RDFResource)ontology.Value);
			if (ontology    != null) {
				result.Model = ontology.Model.GetInferences();
				result.Data  = ontology.Data.GetInferences();
			}
			return result;
		}

		/// <summary>
		/// Gets an ontology model made by semantic inferences found in the given one
		/// </summary>
		public static RDFOntologyModel GetInferences(this RDFOntologyModel ontologyModel) {
			var result               = new RDFOntologyModel();
			if (ontologyModel       != null) {
				result.ClassModel    = ontologyModel.ClassModel.GetInferences();
				result.PropertyModel = ontologyModel.PropertyModel.GetInferences();
			}
			return result;
		}

		/// <summary>
		/// Gets an ontology class model made by semantic inferences found in the given one
		/// </summary>
		public static RDFOntologyClassModel GetInferences(this RDFOntologyClassModel ontologyClassModel) {
			var result              = new RDFOntologyClassModel();
			if (ontologyClassModel != null) {

				//SubClassOf
				foreach (var entry in ontologyClassModel.Relations.SubClassOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.SubClassOf.AddEntry(entry);

				//EquivalentClass
				foreach (var entry in ontologyClassModel.Relations.EquivalentClass.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.SubClassOf.AddEntry(entry);

				//DisjointWith
				foreach (var entry in ontologyClassModel.Relations.DisjointWith.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.SubClassOf.AddEntry(entry);

				//UnionOf
				foreach (var entry in ontologyClassModel.Relations.UnionOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.SubClassOf.AddEntry(entry);

				//IntersectionOf
				foreach (var entry in ontologyClassModel.Relations.IntersectionOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.SubClassOf.AddEntry(entry);

				//OneOf
				foreach (var entry in ontologyClassModel.Relations.OneOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.SubClassOf.AddEntry(entry);

			}
			return result;
		}

		/// <summary>
		/// Gets an ontology property model made by semantic inferences found in the given one
		/// </summary>
		public static RDFOntologyPropertyModel GetInferences(this RDFOntologyPropertyModel ontologyPropertyModel) {
			var result                 = new RDFOntologyPropertyModel();
			if (ontologyPropertyModel != null) {

				//SubPropertyOf
				foreach (var entry in ontologyPropertyModel.Relations.SubPropertyOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.SubPropertyOf.AddEntry(entry);

				//EquivalentProperty
				foreach (var entry in ontologyPropertyModel.Relations.EquivalentProperty.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.EquivalentProperty.AddEntry(entry);

				//InverseOf
				foreach (var entry in ontologyPropertyModel.Relations.InverseOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.InverseOf.AddEntry(entry);

			}
			return result;
		}

		/// <summary>
		/// Gets an ontology data made by semantic inferences found in the given one
		/// </summary>
		public static RDFOntologyData GetInferences(this RDFOntologyData ontologyData) {
			var result              = new RDFOntologyData();
			if (ontologyData       != null) {

				//ClassType
				foreach (var entry in ontologyData.Relations.ClassType.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.ClassType.AddEntry(entry);

				//SameAs
				foreach (var entry in ontologyData.Relations.SameAs.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.SameAs.AddEntry(entry);

				//DifferentFrom
				foreach (var entry in ontologyData.Relations.DifferentFrom.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.DifferentFrom.AddEntry(entry);

				//Assertions
				foreach (var entry in ontologyData.Relations.Assertions.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
					result.Relations.Assertions.AddEntry(entry);

			}
			return result;
		}
		#endregion

		#region ClearInferences
		/// <summary>
		/// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
		/// </summary>
		public static void ClearInferences(this RDFOntology ontology) {
			if (ontology != null) {
				ontology.Model.ClearInferences();
				ontology.Data.ClearInferences();
			}           
		}

		/// <summary>
		/// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
		/// </summary>
		public static void ClearInferences(this RDFOntologyModel ontologyModel) {
			if (ontologyModel != null) {
				ontologyModel.ClassModel.ClearInferences();
				ontologyModel.PropertyModel.ClearInferences();
			}           
		}

		/// <summary>
		/// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
		/// </summary>
		public static void ClearInferences(this RDFOntologyClassModel ontologyClassModel) {
			if (ontologyClassModel != null) {
				var cacheRemove     = new Dictionary<Int64, Object>();

				//SubClassOf
				foreach (var t     in ontologyClassModel.Relations.SubClassOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c     in cacheRemove.Keys) { ontologyClassModel.Relations.SubClassOf.Entries.Remove(c); }
				cacheRemove.Clear();

				//EquivalentClass
				foreach (var t     in ontologyClassModel.Relations.EquivalentClass.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c     in cacheRemove.Keys) { ontologyClassModel.Relations.EquivalentClass.Entries.Remove(c); }
				cacheRemove.Clear();

				//DisjointWith
				foreach (var t     in ontologyClassModel.Relations.DisjointWith.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c     in cacheRemove.Keys) { ontologyClassModel.Relations.DisjointWith.Entries.Remove(c); }
				cacheRemove.Clear();

				//UnionOf
				foreach (var t     in ontologyClassModel.Relations.UnionOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c     in cacheRemove.Keys) { ontologyClassModel.Relations.UnionOf.Entries.Remove(c); }
				cacheRemove.Clear();

				//IntersectionOf
				foreach (var t     in ontologyClassModel.Relations.IntersectionOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c     in cacheRemove.Keys) { ontologyClassModel.Relations.IntersectionOf.Entries.Remove(c); }
				cacheRemove.Clear();

				//OneOf
				foreach (var t     in ontologyClassModel.Relations.OneOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c     in cacheRemove.Keys) { ontologyClassModel.Relations.OneOf.Entries.Remove(c); }
				cacheRemove.Clear();
			}
		}

		/// <summary>
		/// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
		/// </summary>
		public static void ClearInferences(this RDFOntologyPropertyModel ontologyPropertyModel) {
			if (ontologyPropertyModel != null) {
				var cacheRemove        = new Dictionary<Int64, Object>();

				//SubPropertyOf
				foreach (var t        in ontologyPropertyModel.Relations.SubPropertyOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c        in cacheRemove.Keys) { ontologyPropertyModel.Relations.SubPropertyOf.Entries.Remove(c); }
				cacheRemove.Clear();

				//EquivalentProperty
				foreach (var t        in ontologyPropertyModel.Relations.EquivalentProperty.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c        in cacheRemove.Keys) { ontologyPropertyModel.Relations.EquivalentProperty.Entries.Remove(c); }
				cacheRemove.Clear();

				//InverseOf
				foreach (var t        in ontologyPropertyModel.Relations.InverseOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c        in cacheRemove.Keys) { ontologyPropertyModel.Relations.InverseOf.Entries.Remove(c); }
				cacheRemove.Clear();
			}
		}

		/// <summary>
		/// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
		/// </summary>
		public static void ClearInferences(this RDFOntologyData ontologyData) {
			if (ontologyData   != null) {
				var cacheRemove = new Dictionary<Int64, Object>();

				//ClassType
				foreach (var t in ontologyData.Relations.ClassType.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c in cacheRemove.Keys) { ontologyData.Relations.ClassType.Entries.Remove(c); }
				cacheRemove.Clear();

				//SameAs
				foreach (var t in ontologyData.Relations.SameAs.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c in cacheRemove.Keys) { ontologyData.Relations.SameAs.Entries.Remove(c); }
				cacheRemove.Clear();

				//DifferentFrom
				foreach (var t in ontologyData.Relations.DifferentFrom.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c in cacheRemove.Keys) { ontologyData.Relations.DifferentFrom.Entries.Remove(c); }
				cacheRemove.Clear();

				//Assertions
				foreach (var t in ontologyData.Relations.Assertions.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner)) {
					cacheRemove.Add(t.TaxonomyEntryID, null);
				}
				foreach (var c in cacheRemove.Keys) { ontologyData.Relations.Assertions.Entries.Remove(c); }
				cacheRemove.Clear();
			}
		}
		#endregion
		
		#endregion

    }

}