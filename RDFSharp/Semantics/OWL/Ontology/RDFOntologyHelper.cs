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

using RDFSharp.Model;
using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyHelper contains utility methods supporting RDFS/OWL-DL modeling, validation and reasoning
    /// </summary>
    public static class RDFOntologyHelper
    {

        #region ClassModel

        #region SubClassOf
        /// <summary>
        /// Checks if the given aClass is subClass of the given bClass within the given class model
        /// </summary>
        public static bool CheckIsSubClassOf(this RDFOntologyClassModel classModel, RDFOntologyClass aClass, RDFOntologyClass bClass)
        {
            return (aClass != null && bClass != null && classModel != null ? classModel.GetSuperClassesOf(aClass).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the subClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel GetSubClassesOf(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyClassModel();
            if (ontClass != null && classModel != null)
            {

                //Step 1: Reason on the given class
                result = classModel.GetSubClassesOfInternal(ontClass);

                //Step 2: Reason on the equivalent classes
                foreach (var ec in classModel.GetEquivalentClassesOf(ontClass))
                {
                    result = result.UnionWith(classModel.GetSubClassesOfInternal(ec));
                }

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "rdfs:subClassOf" taxonomy to discover direct and indirect subClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel GetSubClassesOfInternalVisitor(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyClassModel();

            // Transitivity of "rdfs:subClassOf" taxonomy: ((A SUBCLASSOF B)  &&  (B SUBCLASSOF C))  =>  (A SUBCLASSOF C)
            foreach (var sc in classModel.Relations.SubClassOf.SelectEntriesByObject(ontClass))
            {
                result.AddClass((RDFOntologyClass)sc.TaxonomySubject);
                result = result.UnionWith(classModel.GetSubClassesOfInternalVisitor((RDFOntologyClass)sc.TaxonomySubject));
            }

            return result;
        }
        internal static RDFOntologyClassModel GetSubClassesOfInternal(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            // Step 1: Direct subsumption of "rdfs:subClassOf" taxonomy
            var result = classModel.GetSubClassesOfInternalVisitor(ontClass);

            // Step 2: Enlist equivalent classes of subclasses
            foreach (var sc in result.ToList())
                result = result.UnionWith(classModel.GetEquivalentClassesOf(sc)
                                                    .UnionWith(classModel.GetSubClassesOf(sc)));

            return result;
        }
        #endregion

        #region SuperClassOf
        /// <summary>
        /// Checks if the given aClass is superClass of the given bClass within the given class model
        /// </summary>
        public static bool CheckIsSuperClassOf(this RDFOntologyClassModel classModel, RDFOntologyClass aClass, RDFOntologyClass bClass)
        {
            return (aClass != null && bClass != null && classModel != null ? classModel.GetSubClassesOf(aClass).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the superClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel GetSuperClassesOf(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyClassModel();
            if (ontClass != null && classModel != null)
            {

                //Step 1: Reason on the given class
                result = classModel.GetSuperClassesOfInternal(ontClass);

                //Step 2: Reason on the equivalent classes
                foreach (var ec in classModel.GetEquivalentClassesOf(ontClass))
                {
                    result = result.UnionWith(classModel.GetSuperClassesOfInternal(ec));
                }

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "rdfs:subClassOf" taxonomy to discover direct and indirect superClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel GetSuperClassesOfInternalVisitor(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyClassModel();

            // Transitivity of "rdfs:subClassOf" taxonomy: ((A SUPERCLASSOF B)  &&  (B SUPERCLASSOF C))  =>  (A SUPERCLASSOF C)
            foreach (var sc in classModel.Relations.SubClassOf.SelectEntriesBySubject(ontClass))
            {
                result.AddClass((RDFOntologyClass)sc.TaxonomyObject);
                result = result.UnionWith(classModel.GetSuperClassesOfInternalVisitor((RDFOntologyClass)sc.TaxonomyObject));
            }

            return result;
        }
        internal static RDFOntologyClassModel GetSuperClassesOfInternal(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            // Step 1: Direct subsumption of "rdfs:subClassOf" taxonomy
            var result = classModel.GetSuperClassesOfInternalVisitor(ontClass);

            // Step 2: Enlist equivalent classes of superclasses
            foreach (var sc in result.ToList())
                result = result.UnionWith(classModel.GetEquivalentClassesOf(sc)
                                                    .UnionWith(classModel.GetSuperClassesOf(sc)));

            return result;
        }
        #endregion

        #region EquivalentClass
        /// <summary>
        /// Checks if the given aClass is equivalentClass of the given bClass within the given class model
        /// </summary>
        public static bool CheckIsEquivalentClassOf(this RDFOntologyClassModel classModel, RDFOntologyClass aClass, RDFOntologyClass bClass)
        {
            return (aClass != null && bClass != null && classModel != null ? classModel.GetEquivalentClassesOf(aClass).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the equivalentClasses of the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel GetEquivalentClassesOf(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyClassModel();
            if (ontClass != null && classModel != null)
            {
                result = classModel.GetEquivalentClassesOfInternal(ontClass, null)
                                   .RemoveClass(ontClass); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:equivalentClass" taxonomy to discover direct and indirect equivalentClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel GetEquivalentClassesOfInternal(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass, Dictionary<long, RDFOntologyClass> visitContext)
        {
            var result = new RDFOntologyClassModel();

            #region visitContext
            if (visitContext == null)
            {
                visitContext = new Dictionary<long, RDFOntologyClass>() { { ontClass.PatternMemberID, ontClass } };
            }
            else
            {
                if (!visitContext.ContainsKey(ontClass.PatternMemberID))
                {
                    visitContext.Add(ontClass.PatternMemberID, ontClass);
                }
                else
                {
                    return result;
                }
            }
            #endregion

            // Transitivity of "owl:equivalentClass" taxonomy: ((A EQUIVALENTCLASSOF B)  &&  (B EQUIVALENTCLASS C))  =>  (A EQUIVALENTCLASS C)
            foreach (var ec in classModel.Relations.EquivalentClass.SelectEntriesBySubject(ontClass))
            {
                result.AddClass((RDFOntologyClass)ec.TaxonomyObject);
                result = result.UnionWith(classModel.GetEquivalentClassesOfInternal((RDFOntologyClass)ec.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #region DisjointWith
        /// <summary>
        /// Checks if the given aClass is disjointClass with the given bClass within the given class model
        /// </summary>
        public static bool CheckIsDisjointClassWith(this RDFOntologyClassModel classModel, RDFOntologyClass aClass, RDFOntologyClass bClass)
        {
            return (aClass != null && bClass != null && classModel != null ? classModel.GetDisjointClassesWith(aClass).Classes.ContainsKey(bClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the disjointClasses with the given class within the given class model
        /// </summary>
        public static RDFOntologyClassModel GetDisjointClassesWith(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyClassModel();
            if (ontClass != null && classModel != null)
            {
                result = classModel.GetDisjointClassesWithInternal(ontClass, null)
                                   .RemoveClass(ontClass); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:disjointWith" taxonomy to discover direct and indirect disjointClasses of the given class
        /// </summary>
        internal static RDFOntologyClassModel GetDisjointClassesWithInternal(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass, Dictionary<long, RDFOntologyClass> visitContext)
        {
            var result = new RDFOntologyClassModel();

            #region visitContext
            if (visitContext == null)
            {
                visitContext = new Dictionary<long, RDFOntologyClass>() { { ontClass.PatternMemberID, ontClass } };
            }
            else
            {
                if (!visitContext.ContainsKey(ontClass.PatternMemberID))
                {
                    visitContext.Add(ontClass.PatternMemberID, ontClass);
                }
                else
                {
                    return result;
                }
            }
            #endregion

            // Inference: ((A DISJOINTWITH B)   &&  (B EQUIVALENTCLASS C))  =>  (A DISJOINTWITH C)
            foreach (var dw in classModel.Relations.DisjointWith.SelectEntriesBySubject(ontClass))
                result = result.UnionWith(classModel.GetEquivalentClassesOfInternal((RDFOntologyClass)dw.TaxonomyObject, visitContext))
                               .AddClass((RDFOntologyClass)dw.TaxonomyObject);

            // Inference: ((A DISJOINTWITH B)   &&  (B SUPERCLASS C))  =>  (A DISJOINTWITH C)
            foreach (var sc in result.ToList())
                result = result.UnionWith(classModel.GetSubClassesOfInternal(sc));

            // Inference: ((A EQUIVALENTCLASS B || A SUBCLASSOF B)  &&  (B DISJOINTWITH C))  =>  (A DISJOINTWITH C)
            var compatibleCls = classModel.GetSuperClassesOf(ontClass)
                                          .UnionWith(classModel.GetEquivalentClassesOf(ontClass));
            foreach (var ec in compatibleCls)
                result = result.UnionWith(classModel.GetDisjointClassesWithInternal(ec, visitContext));

            return result;
        }
        #endregion

        #region HasKey [OWL2]
        /// <summary>
        /// Gets the key values for each member of the given class having a complete (or partial, if allowed) key representation [OWL2]
        /// </summary>
        public static Dictionary<string, List<RDFOntologyResource>> GetKeyValuesOf(this RDFOntology ontology, RDFOntologyClass ontologyClass, bool allowPartialKeyValues)
        {
            Dictionary<string, List<RDFOntologyResource>> result = new Dictionary<string, List<RDFOntologyResource>>();

            RDFOntologyTaxonomy hasKeyClassTaxonomy = ontology.Model.ClassModel.Relations.HasKey.SelectEntriesBySubject(ontologyClass);
            if (hasKeyClassTaxonomy.Any())
            {
                //Enlist members of owl:hasKey class
                RDFOntologyData hasKeyClassMembers = GetMembersOf(ontology, ontologyClass);

                //Fetch owl:hasKey property values for each of owl:haskey class members
                foreach (RDFOntologyTaxonomyEntry hasKeyClassTaxonomyEntry in hasKeyClassTaxonomy)
                {
                    foreach (RDFOntologyFact hasKeyClassMember in hasKeyClassMembers)
                    {
                        List<RDFOntologyResource> keyPropertyValues = ontology.Data.Relations.Assertions.SelectEntriesBySubject(hasKeyClassMember)
                                                                                                        .SelectEntriesByPredicate(hasKeyClassTaxonomyEntry.TaxonomyObject)
                                                                                                        .Select(te => te.TaxonomyObject)
                                                                                                        .ToList();

                        //This is to signal partial owl:hasKey property value
                        if (keyPropertyValues.Count == 0)
                            keyPropertyValues.Add(null);

                        if (!result.ContainsKey(hasKeyClassMember.ToString()))
                            result.Add(hasKeyClassMember.ToString(), keyPropertyValues);
                        else
                            result[hasKeyClassMember.ToString()].AddRange(keyPropertyValues);
                    }
                }
            }

            //If partial key values are not allowed, remove them from result
            return allowPartialKeyValues ? result : result.Where(res => res.Value.TrueForAll(x => x != null))
                                                          .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        #endregion

        #region Domain
        /// <summary>
        /// Checks if the given ontology class is domain of the given ontology property within the given ontology class model
        /// </summary>
        public static bool CheckIsDomainOf(this RDFOntologyClassModel classModel, RDFOntologyClass domainClass, RDFOntologyProperty ontProperty)
        {
            return (domainClass != null && ontProperty != null && classModel != null ? classModel.GetDomainOf(ontProperty).Classes.ContainsKey(domainClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the domain classes of the given property within the given ontology class model
        /// </summary>
        public static RDFOntologyClassModel GetDomainOf(this RDFOntologyClassModel classModel, RDFOntologyProperty ontProperty)
        {
            var result = new RDFOntologyClassModel();
            if (ontProperty != null && classModel != null)
            {
                if (ontProperty.Domain != null)
                {
                    result = classModel.GetSubClassesOf(ontProperty.Domain)
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
        public static bool CheckIsRangeOf(this RDFOntologyClassModel classModel, RDFOntologyClass rangeClass, RDFOntologyProperty ontProperty)
        {
            return (rangeClass != null && ontProperty != null && classModel != null ? classModel.GetRangeOf(ontProperty).Classes.ContainsKey(rangeClass.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the range classes of the given property within the given ontology class model
        /// </summary>
        public static RDFOntologyClassModel GetRangeOf(this RDFOntologyClassModel classModel, RDFOntologyProperty ontProperty)
        {
            var result = new RDFOntologyClassModel();
            if (ontProperty != null && classModel != null)
            {
                if (ontProperty.Range != null)
                {
                    result = classModel.GetSubClassesOf(ontProperty.Range)
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
        public static bool CheckIsLiteralCompatibleClass(this RDFOntologyClassModel classModel, RDFOntologyClass ontClass)
        {
            var result = false;
            if (ontClass != null && classModel != null)
            {
                result = (ontClass.IsDataRangeClass()
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
        public static bool CheckIsSubPropertyOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty aProperty, RDFOntologyProperty bProperty)
        {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetSuperPropertiesOf(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the sub properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetSubPropertiesOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty)
        {
            var result = new RDFOntologyPropertyModel();
            if (ontProperty != null && propertyModel != null)
            {

                //Step 1: Reason on the given property
                result = propertyModel.GetSubPropertiesOfInternal(ontProperty);

                //Step 2: Reason on the equivalent properties
                foreach (var ep in propertyModel.GetEquivalentPropertiesOf(ontProperty))
                {
                    result = result.UnionWith(propertyModel.GetSubPropertiesOfInternal(ep));
                }

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "rdfs:subPropertyOf" taxonomy to discover direct and indirect subProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel GetSubPropertiesOfInternalVisitor(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty)
        {
            var result = new RDFOntologyPropertyModel();

            // Transitivity of "rdfs:subPropertyOf" taxonomy: ((A SUBPROPERTYOF B)  &&  (B SUBPROPERTYOF C))  =>  (A SUBPROPERTYOF C)
            foreach (var sp in propertyModel.Relations.SubPropertyOf.SelectEntriesByObject(ontProperty))
            {
                result.AddProperty((RDFOntologyProperty)sp.TaxonomySubject);
                result = result.UnionWith(propertyModel.GetSubPropertiesOfInternalVisitor((RDFOntologyProperty)sp.TaxonomySubject));
            }

            return result;
        }
        internal static RDFOntologyPropertyModel GetSubPropertiesOfInternal(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty)
        {
            var result1 = new RDFOntologyPropertyModel();
            var result2 = new RDFOntologyPropertyModel();

            // Step 1: Direct subsumption of "rdfs:subPropertyOf" taxonomy
            result1 = propertyModel.GetSubPropertiesOfInternalVisitor(ontProperty);

            // Step 2: Enlist equivalent properties of subproperties
            result2 = result2.UnionWith(result1);
            foreach (var sp in result1)
            {
                result2 = result2.UnionWith(propertyModel.GetEquivalentPropertiesOf(sp)
                                                         .UnionWith(propertyModel.GetSubPropertiesOf(sp)));
            }

            return result2;
        }
        #endregion

        #region SuperPropertyOf
        /// <summary>
        /// Checks if the given aProperty is superProperty of the given bProperty within the given property model
        /// </summary>
        public static bool CheckIsSuperPropertyOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty aProperty, RDFOntologyProperty bProperty)
        {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetSubPropertiesOf(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the super properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetSuperPropertiesOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty)
        {
            var result = new RDFOntologyPropertyModel();
            if (ontProperty != null && propertyModel != null)
            {

                //Step 1: Reason on the given property
                result = propertyModel.GetSuperPropertiesOfInternal(ontProperty);

                //Step 2: Reason on the equivalent properties
                foreach (var ep in propertyModel.GetEquivalentPropertiesOf(ontProperty))
                {
                    result = result.UnionWith(propertyModel.GetSuperPropertiesOfInternal(ep));
                }

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "rdfs:subPropertyOf" taxonomy to discover direct and indirect superProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel GetSuperPropertiesOfInternalVisitor(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty)
        {
            var result = new RDFOntologyPropertyModel();

            // Transitivity of "rdfs:subPropertyOf" taxonomy: ((A SUPERPROPERTYOF B)  &&  (B SUPERPROPERTYOF C))  =>  (A SUPERPROPERTYOF C)
            foreach (var sp in propertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(ontProperty))
            {
                result.AddProperty((RDFOntologyProperty)sp.TaxonomyObject);
                result = result.UnionWith(propertyModel.GetSuperPropertiesOfInternalVisitor((RDFOntologyProperty)sp.TaxonomyObject));
            }

            return result;
        }
        internal static RDFOntologyPropertyModel GetSuperPropertiesOfInternal(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty)
        {
            var result1 = new RDFOntologyPropertyModel();
            var result2 = new RDFOntologyPropertyModel();

            // Step 1: Direct subsumption of "rdfs:subPropertyOf" taxonomy
            result1 = propertyModel.GetSuperPropertiesOfInternalVisitor(ontProperty);

            // Step 2: Enlist equivalent properties of subproperties
            result2 = result2.UnionWith(result1);
            foreach (var sp in result1)
            {
                result2 = result2.UnionWith(propertyModel.GetEquivalentPropertiesOf(sp)
                                                         .UnionWith(propertyModel.GetSuperPropertiesOf(sp)));
            }

            return result2;
        }
        #endregion

        #region EquivalentProperty
        /// <summary>
        /// Checks if the given aProperty is equivalentProperty of the given bProperty within the given property model
        /// </summary>
        public static bool CheckIsEquivalentPropertyOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty aProperty, RDFOntologyProperty bProperty)
        {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetEquivalentPropertiesOf(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the equivalentProperties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetEquivalentPropertiesOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty)
        {
            var result = new RDFOntologyPropertyModel();
            if (ontProperty != null && propertyModel != null)
            {
                result = propertyModel.GetEquivalentPropertiesOfInternal(ontProperty, null)
                                      .RemoveProperty(ontProperty); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:equivalentProperty" taxonomy to discover direct and indirect equivalentProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel GetEquivalentPropertiesOfInternal(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty, Dictionary<long, RDFOntologyProperty> visitContext)
        {
            var result = new RDFOntologyPropertyModel();

            #region visitContext
            if (visitContext == null)
            {
                visitContext = new Dictionary<long, RDFOntologyProperty>() { { ontProperty.PatternMemberID, ontProperty } };
            }
            else
            {
                if (!visitContext.ContainsKey(ontProperty.PatternMemberID))
                {
                    visitContext.Add(ontProperty.PatternMemberID, ontProperty);
                }
                else
                {
                    return result;
                }
            }
            #endregion

            // Transitivity of "owl:equivalentProperty" taxonomy: ((A EQUIVALENTPROPERTY B)  &&  (B EQUIVALENTPROPERTY C))  =>  (A EQUIVALENTPROPERTY C)
            foreach (var ep in propertyModel.Relations.EquivalentProperty.SelectEntriesBySubject(ontProperty))
            {
                result.AddProperty((RDFOntologyProperty)ep.TaxonomyObject);
                result = result.UnionWith(propertyModel.GetEquivalentPropertiesOfInternal((RDFOntologyProperty)ep.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #region DisjointPropertyWith [OWL2]
        /// <summary>
        /// Checks if the given aProperty is disjointProperty with the given bProperty within the given property model
        /// </summary>
        public static bool CheckIsPropertyDisjointWith(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty aProperty, RDFOntologyProperty bProperty)
        {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetPropertiesDisjointWith(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the disjointProperties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetPropertiesDisjointWith(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty)
        {
            var result = new RDFOntologyPropertyModel();
            if (ontProperty != null && propertyModel != null)
            {
                result = propertyModel.GetPropertiesDisjointWithInternal(ontProperty, null)
                                      .RemoveProperty(ontProperty); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:propertyDisjointWith" taxonomy to discover direct and indirect disjointProperties of the given property
        /// </summary>
        internal static RDFOntologyPropertyModel GetPropertiesDisjointWithInternal(this RDFOntologyPropertyModel propertyModel, RDFOntologyProperty ontProperty, Dictionary<long, RDFOntologyProperty> visitContext)
        {
            var result1 = new RDFOntologyPropertyModel();
            var result2 = new RDFOntologyPropertyModel();

            #region visitContext
            if (visitContext == null)
            {
                visitContext = new Dictionary<long, RDFOntologyProperty>() { { ontProperty.PatternMemberID, ontProperty } };
            }
            else
            {
                if (!visitContext.ContainsKey(ontProperty.PatternMemberID))
                {
                    visitContext.Add(ontProperty.PatternMemberID, ontProperty);
                }
                else
                {
                    return result1;
                }
            }
            #endregion

            // Inference: ((A PROPERTYDISJOINTWITH B)   &&  (B EQUIVALENTPROPERTY C))  =>  (A PROPERTYDISJOINTWITH C)
            foreach (var dw in propertyModel.Relations.PropertyDisjointWith.SelectEntriesBySubject(ontProperty))
            {
                result1.AddProperty((RDFOntologyProperty)dw.TaxonomyObject);
                result1 = result1.UnionWith(propertyModel.GetEquivalentPropertiesOfInternal((RDFOntologyProperty)dw.TaxonomyObject, visitContext));
            }

            // Inference: ((A PROPERTYDISJOINTWITH B)   &&  (B SUPERPROPERTY C))  =>  (A PROPERTYDISJOINTWITH C)
            result2 = result2.UnionWith(result1);
            foreach (var p in result1)
            {
                result2 = result2.UnionWith(propertyModel.GetSubPropertiesOfInternal(p));
            }
            result1 = result1.UnionWith(result2);

            // Inference: ((A EQUIVALENTPROPERTY B || A SUBPROPERTYOF B)  &&  (B PROPERTYDISJOINTWITH C))  =>  (A PROPERTYDISJOINTWITH C)
            var compatiblePrp = propertyModel.GetSuperPropertiesOf(ontProperty)
                                             .UnionWith(propertyModel.GetEquivalentPropertiesOf(ontProperty));
            foreach (var ep in compatiblePrp)
            {
                result1 = result1.UnionWith(propertyModel.GetPropertiesDisjointWithInternal(ep, visitContext));
            }

            return result1;
        }
        #endregion

        #region PropertyChainAxiom [OWL2]
        /// <summary>
        /// Checks if the given ontProperty is a property chain within the given property model
        /// </summary>
        public static bool CheckIsPropertyChain(this RDFOntologyPropertyModel propertyModel, RDFOntologyObjectProperty ontProperty)
        {
            if (ontProperty != null && propertyModel != null)
                return propertyModel.Relations.PropertyChainAxiom.Any(te => te.TaxonomySubject.Equals(ontProperty));

            return false;
        }

        /// <summary>
        /// Checks if the given aProperty is a property chain step of the given bProperty within the given ontology
        /// </summary>
        public static bool CheckIsPropertyChainStepOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyObjectProperty aProperty, RDFOntologyObjectProperty bProperty)
        {
            if (aProperty != null && bProperty != null && propertyModel != null)
                return propertyModel.GetPropertyChainStepsOf(bProperty).Any(step => step.StepProperty.Equals(aProperty.Value));

            return false;
        }

        /// <summary>
        /// Gets the assertions for each property chain declared in the given ontology [OWL2]
        /// </summary>
        public static Dictionary<string, RDFOntologyData> GetPropertyChainAxiomsData(this RDFOntology ontology)
        {
            Dictionary<string, RDFOntologyData> result = new Dictionary<string, RDFOntologyData>();

            //Materialize graph representation of the given ontology
            RDFGraph ontologyGraph = ontology.ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData);

            //Iterate property chain axiom taxonomy of the given ontology
            foreach (IGrouping<RDFOntologyResource, RDFOntologyTaxonomyEntry> propertyChainAxiomTaxonomy in ontology.Model.PropertyModel.Relations.PropertyChainAxiom.GroupBy(t => t.TaxonomySubject))
            {
                result.Add(propertyChainAxiomTaxonomy.Key.ToString(), new RDFOntologyData());

                //Transform property chain axiom of current property into equivalent property path
                RDFPropertyPath propertyChainAxiomPath = new RDFPropertyPath(new RDFVariable("?PROPERTY_CHAIN_AXIOM_START"), new RDFVariable("?PROPERTY_CHAIN_AXIOM_END"));
                List<RDFPropertyPathStep> propertyChainAxiomPathSteps = ontology.Model.PropertyModel.GetPropertyChainStepsOf(propertyChainAxiomTaxonomy.Key);
                foreach (RDFPropertyPathStep propertyChainAxiomPathStep in propertyChainAxiomPathSteps)
                    propertyChainAxiomPath.AddSequenceStep(propertyChainAxiomPathStep);

                //Execute construct query for getting property chain axiom data from ontology
                RDFConstructQueryResult queryResult =
                    new RDFConstructQuery()
                        .AddPatternGroup(new RDFPatternGroup("PROPERTY_CHAIN_AXIOM")
                            .AddPropertyPath(propertyChainAxiomPath))
                        .AddTemplate(new RDFPattern(new RDFVariable("?PROPERTY_CHAIN_AXIOM_START"), (RDFResource)propertyChainAxiomTaxonomy.Key.Value, new RDFVariable("?PROPERTY_CHAIN_AXIOM_END")))
                        .ApplyToGraph(ontologyGraph);

                //Populate result with corresponding ontology assertions
                foreach (RDFTriple queryResultTriple in queryResult.ToRDFGraph())
                {
                    RDFOntologyFact assertionSubject = ontology.Data.SelectFact(queryResultTriple.Subject.ToString());
                    RDFOntologyProperty assertionPredicate = ontology.Model.PropertyModel.SelectProperty(queryResultTriple.Predicate.ToString());
                    RDFOntologyFact assertionObject = ontology.Data.SelectFact(queryResultTriple.Object.ToString());
                    if (assertionPredicate is RDFOntologyObjectProperty)
                        result[propertyChainAxiomTaxonomy.Key.ToString()].AddAssertionRelation(assertionSubject, (RDFOntologyObjectProperty)assertionPredicate, assertionObject);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the direct and indirect properties composing the path of the given property chain [OWL2]
        /// </summary>
        internal static List<RDFPropertyPathStep> GetPropertyChainStepsOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyResource propertyChain, HashSet<long> visitContext = null)
        {
            List<RDFPropertyPathStep> result = new List<RDFPropertyPathStep>();
            if (propertyChain != null && propertyModel != null)
            {

                #region visitContext
                if (visitContext == null)
                {
                    visitContext = new HashSet<long>() { { propertyChain.Value.PatternMemberID } };
                }
                else
                {
                    if (!visitContext.Contains(propertyChain.Value.PatternMemberID))
                    {
                        visitContext.Add(propertyChain.Value.PatternMemberID);
                    }
                    else
                    {
                        return result;
                    }
                }
                #endregion

                //owl:propertyChainAxiom
                foreach (RDFOntologyTaxonomyEntry propertyChainAxiomTaxonomyEntry in propertyModel.Relations.PropertyChainAxiom.SelectEntriesBySubject(propertyChain))
                {
                    bool containsPropertyChainAxiom = propertyModel.Relations.PropertyChainAxiom.SelectEntriesBySubject(propertyChainAxiomTaxonomyEntry.TaxonomyObject).EntriesCount > 0;
                    if (containsPropertyChainAxiom)
                        result.AddRange(propertyModel.GetPropertyChainStepsOf(propertyChainAxiomTaxonomyEntry.TaxonomyObject, visitContext));
                    else
                        result.Add(new RDFPropertyPathStep((RDFResource)propertyChainAxiomTaxonomyEntry.TaxonomyObject.Value));
                }

            }
            return result;
        }
        #endregion

        #region InverseOf
        /// <summary>
        /// Checks if the given aProperty is inverse property of the given bProperty within the given property model
        /// </summary>
        public static bool CheckIsInversePropertyOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyObjectProperty aProperty, RDFOntologyObjectProperty bProperty)
        {
            return (aProperty != null && bProperty != null && propertyModel != null ? propertyModel.GetInversePropertiesOf(aProperty).Properties.ContainsKey(bProperty.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the inverse properties of the given property within the given property model
        /// </summary>
        public static RDFOntologyPropertyModel GetInversePropertiesOf(this RDFOntologyPropertyModel propertyModel, RDFOntologyObjectProperty ontProperty)
        {
            var result = new RDFOntologyPropertyModel();
            if (ontProperty != null && propertyModel != null)
            {

                //Step 1: Reason on the given property
                //Subject-side inverseOf relation
                foreach (var invOf in propertyModel.Relations.InverseOf.SelectEntriesBySubject(ontProperty))
                {
                    result.AddProperty((RDFOntologyObjectProperty)invOf.TaxonomyObject);
                }
                //Object-side inverseOf relation
                foreach (var invOf in propertyModel.Relations.InverseOf.SelectEntriesByObject(ontProperty))
                {
                    result.AddProperty((RDFOntologyObjectProperty)invOf.TaxonomySubject);
                }
                result.RemoveProperty(ontProperty); //Safety deletion

            }
            return result;
        }
        #endregion

        #endregion

        #region Data

        #region SameAs
        /// <summary>
        /// Checks if the given aFact is sameAs the given bFact within the given data
        /// </summary>
        public static bool CheckIsSameFactAs(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyFact bFact)
        {
            return (aFact != null && bFact != null && data != null ? data.GetSameFactsAs(aFact).Facts.ContainsKey(bFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the sameFacts of the given fact within the given data
        /// </summary>
        public static RDFOntologyData GetSameFactsAs(this RDFOntologyData data, RDFOntologyFact ontFact)
        {
            var result = new RDFOntologyData();
            if (ontFact != null && data != null)
            {
                result = data.GetSameFactsAsInternal(ontFact, null)
                             .RemoveFact(ontFact); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:sameAs" taxonomy to discover direct and indirect samefacts of the given facts
        /// </summary>
        internal static RDFOntologyData GetSameFactsAsInternal(this RDFOntologyData data, RDFOntologyFact ontFact, Dictionary<long, RDFOntologyFact> visitContext)
        {
            var result = new RDFOntologyData();

            #region visitContext
            if (visitContext == null)
            {
                visitContext = new Dictionary<long, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else
            {
                if (!visitContext.ContainsKey(ontFact.PatternMemberID))
                {
                    visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else
                {
                    return result;
                }
            }
            #endregion

            // Transitivity of "owl:sameAs" taxonomy: ((A SAMEAS B)  &&  (B SAMEAS C))  =>  (A SAMEAS C)
            foreach (var sf in data.Relations.SameAs.SelectEntriesBySubject(ontFact))
            {
                result.AddFact((RDFOntologyFact)sf.TaxonomyObject);
                result = result.UnionWith(data.GetSameFactsAsInternal((RDFOntologyFact)sf.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #region DifferentFrom
        /// <summary>
        /// Checks if the given aFact is differentFrom the given bFact within the given data
        /// </summary>
        public static bool CheckIsDifferentFactFrom(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyFact bFact)
        {
            return (aFact != null && bFact != null && data != null ? data.GetDifferentFactsFrom(aFact).Facts.ContainsKey(bFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the different facts of the given fact within the given data
        /// </summary>
        public static RDFOntologyData GetDifferentFactsFrom(this RDFOntologyData data, RDFOntologyFact ontFact)
        {
            var result = new RDFOntologyData();
            if (ontFact != null && data != null)
            {
                result = data.GetDifferentFactsFromInternal(ontFact, null)
                             .RemoveFact(ontFact); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "owl:differentFrom" taxonomy to discover direct and indirect differentFacts of the given facts
        /// </summary>
        internal static RDFOntologyData GetDifferentFactsFromInternal(this RDFOntologyData data, RDFOntologyFact ontFact, Dictionary<long, RDFOntologyFact> visitContext)
        {
            var result = new RDFOntologyData();

            #region visitContext
            if (visitContext == null)
            {
                visitContext = new Dictionary<long, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else
            {
                if (!visitContext.ContainsKey(ontFact.PatternMemberID))
                {
                    visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else
                {
                    return result;
                }
            }
            #endregion

            // Inference: (A DIFFERENTFROM B  &&  B SAMEAS C         =>  A DIFFERENTFROM C)
            foreach (var df in data.Relations.DifferentFrom.SelectEntriesBySubject(ontFact))
            {
                result.AddFact((RDFOntologyFact)df.TaxonomyObject);
                result = result.UnionWith(data.GetSameFactsAsInternal((RDFOntologyFact)df.TaxonomyObject, visitContext));
            }

            // Inference: (A SAMEAS B         &&  B DIFFERENTFROM C  =>  A DIFFERENTFROM C)
            foreach (var sa in data.GetSameFactsAs(ontFact))
            {
                result = result.UnionWith(data.GetDifferentFactsFromInternal(sa, visitContext));
            }

            return result;
        }
        #endregion

        #region TransitiveProperty
        /// <summary>
        /// Checks if the given "aFact -> transProp" assertion links to the given bFact within the given data
        /// </summary>
        public static bool CheckIsTransitiveAssertionOf(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyObjectProperty transProp, RDFOntologyFact bFact)
        {
            return (aFact != null && transProp != null && transProp.IsTransitiveProperty() && bFact != null && data != null ? data.GetTransitiveAssertionsOf(aFact, transProp).Facts.ContainsKey(bFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the given "aFact -> transOntProp" assertions within the given data
        /// </summary>
        public static RDFOntologyData GetTransitiveAssertionsOf(this RDFOntologyData data, RDFOntologyFact ontFact, RDFOntologyObjectProperty transOntProp)
        {
            var result = new RDFOntologyData();
            if (ontFact != null && transOntProp != null && transOntProp.IsTransitiveProperty() && data != null)
            {
                result = data.GetTransitiveAssertionsOfInternal(ontFact, transOntProp, null);
            }
            return result;
        }

        /// <summary>
        /// Enlists the transitive assertions of the given fact and the given property within the given data
        /// </summary>
        internal static RDFOntologyData GetTransitiveAssertionsOfInternal(this RDFOntologyData data, RDFOntologyFact ontFact, RDFOntologyObjectProperty ontProp, Dictionary<long, RDFOntologyFact> visitContext)
        {
            var result = new RDFOntologyData();

            #region visitContext
            if (visitContext == null)
            {
                visitContext = new Dictionary<long, RDFOntologyFact>() { { ontFact.PatternMemberID, ontFact } };
            }
            else
            {
                if (!visitContext.ContainsKey(ontFact.PatternMemberID))
                {
                    visitContext.Add(ontFact.PatternMemberID, ontFact);
                }
                else
                {
                    return result;
                }
            }
            #endregion

            // ((F1 P F2)    &&  (F2 P F3))  =>  (F1 P F3)
            foreach (var ta in data.Relations.Assertions.SelectEntriesBySubject(ontFact)
                                                        .SelectEntriesByPredicate(ontProp))
            {
                result.AddFact((RDFOntologyFact)ta.TaxonomyObject);
                result = result.UnionWith(data.GetTransitiveAssertionsOfInternal((RDFOntologyFact)ta.TaxonomyObject, ontProp, visitContext));
            }

            return result;
        }
        #endregion

        #region Assertions
        /// <summary>
        /// Checks if the given "aFact -> objectProperty -> bFact" is an assertion within the given data
        /// </summary>
        public static bool CheckIsAssertion(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyObjectProperty objectProperty, RDFOntologyFact bFact)
        {
            if (aFact != null && bFact != null && objectProperty != null && data != null)
            {
                //Reason over subject/object facts to detect indirect potential taxonomy violations
                RDFOntologyData compatibleSubjects = data.GetSameFactsAs(aFact).AddFact(aFact);
                RDFOntologyData compatibleObjects = data.GetSameFactsAs(aFact).AddFact(bFact);

                return data.Relations.Assertions.Any(te => compatibleSubjects.Any(x => x.Equals(te.TaxonomySubject))
                                                               && te.TaxonomyPredicate.Equals(objectProperty)
                                                                   && compatibleObjects.Any(x => x.Equals(te.TaxonomyObject)));
            }
            return false;
        }

        /// <summary>
        /// Checks if the given "aFact -> datatypeProperty -> ontologyLiteral" is an assertion within the given data
        /// </summary>
        public static bool CheckIsAssertion(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyDatatypeProperty datatypeProperty, RDFOntologyLiteral ontologyLiteral)
        {
            if (aFact != null && ontologyLiteral != null && datatypeProperty != null && data != null)
            {
                //Reason over subject facts to detect indirect potential taxonomy violations
                RDFOntologyData compatibleSubjects = data.GetSameFactsAs(aFact).AddFact(aFact);

                return data.Relations.Assertions.Any(te => compatibleSubjects.Any(x => x.Equals(te.TaxonomySubject))
                                                               && te.TaxonomyPredicate.Equals(datatypeProperty)
                                                                   && te.TaxonomyObject.Equals(ontologyLiteral));
            }
            return false;
        }

        /// <summary>
        /// Checks if the given "aFact -> objectProperty -> bFact" is a negative assertion within the given data
        /// </summary>
        public static bool CheckIsNegativeAssertion(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyObjectProperty objectProperty, RDFOntologyFact bFact)
        {
            if (aFact != null && bFact != null && objectProperty != null && data != null)
            {
                //Reason over subject/object facts to detect indirect potential taxonomy violations
                RDFOntologyData compatibleSubjects = data.GetSameFactsAs(aFact).AddFact(aFact);
                RDFOntologyData compatibleObjects = data.GetSameFactsAs(aFact).AddFact(bFact);

                return data.Relations.NegativeAssertions.Any(te => compatibleSubjects.Any(x => x.Equals(te.TaxonomySubject))
                                                                       && te.TaxonomyPredicate.Equals(objectProperty)
                                                                           && compatibleObjects.Any(x => x.Equals(te.TaxonomyObject)));
            }
            return false;
        }

        /// <summary>
        /// Checks if the given "aFact -> datatypeProperty -> ontologyLiteral" is a negative assertion within the given data
        /// </summary>
        public static bool CheckIsNegativeAssertion(this RDFOntologyData data, RDFOntologyFact aFact, RDFOntologyDatatypeProperty datatypeProperty, RDFOntologyLiteral ontologyLiteral)
        {
            if (aFact != null && ontologyLiteral != null && datatypeProperty != null && data != null)
            {
                //Reason over subject facts to detect indirect potential taxonomy violations
                RDFOntologyData compatibleSubjects = data.GetSameFactsAs(aFact).AddFact(aFact);

                return data.Relations.NegativeAssertions.Any(te => compatibleSubjects.Any(x => x.Equals(te.TaxonomySubject))
                                                                       && te.TaxonomyPredicate.Equals(datatypeProperty)
                                                                           && te.TaxonomyObject.Equals(ontologyLiteral));
            }
            return false;
        }
        #endregion

        #region MemberOf
        /// <summary>
        /// Checks if the given fact is member of the given class within the given ontology
        /// </summary>
        public static bool CheckIsMemberOf(this RDFOntology ontology, RDFOntologyFact ontFact, RDFOntologyClass ontClass)
        {
            return (ontFact != null && ontClass != null && ontology != null ? ontology.GetMembersOf(ontClass).Facts.ContainsKey(ontFact.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the facts which are members of the given class within the given ontology
        /// </summary>
        public static RDFOntologyData GetMembersOf(this RDFOntology ontology, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyData();
            if (ontClass != null && ontology != null)
            {

                //Expand ontology
                var expOnt = ontology.UnionWith(RDFBASEOntology.Instance);

                //DataRange/Literal-Compatible
                if (expOnt.Model.ClassModel.CheckIsLiteralCompatibleClass(ontClass))
                    result = expOnt.GetMembersOfLiteralCompatibleClass(ontClass);

                //Restriction/Composite/Enumerate/Class
                else
                    result = expOnt.GetMembersOfNonLiteralCompatibleClass(ontClass);

            }
            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given class within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfClass(this RDFOntology ontology, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyData();

            //Get the compatible classes
            var compClasses = ontology.Model.ClassModel.GetSubClassesOf(ontClass)
                                                       .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf(ontClass))
                                                       .AddClass(ontClass);

            //Get the facts belonging to compatible classes
            var compFacts = ontology.Data.Relations.ClassType.Where(te => compClasses.Any(c => c.Equals(te.TaxonomyObject)))
                                                             .Select(te => te.TaxonomySubject);

            //Add the fact and its synonyms
            var sameFactsCache = new Dictionary<long, RDFOntologyData> ();
            foreach (var compFact in compFacts)
            {
                if (!sameFactsCache.ContainsKey(compFact.PatternMemberID))
                {
                    sameFactsCache.Add(compFact.PatternMemberID, ontology.Data.GetSameFactsAs((RDFOntologyFact)compFact));

                    result = result.UnionWith(sameFactsCache[compFact.PatternMemberID])
                                   .AddFact((RDFOntologyFact)compFact);
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given composite within the given ontology.
        /// </summary>
        internal static RDFOntologyData GetMembersOfComposite(this RDFOntology ontology, RDFOntologyClass ontCompClass, Dictionary<long, RDFOntologyData> membersCache = null)
        {
            var result = new RDFOntologyData();

            #region Intersection
            if (ontCompClass is RDFOntologyIntersectionClass)
            {

                //Filter "intersectionOf" relations made with the given intersection class
                var firstIter = true;
                var iTaxonomy = ontology.Model.ClassModel.Relations.IntersectionOf.SelectEntriesBySubject(ontCompClass);
                foreach (var tEntry in iTaxonomy)
                {
                    if (firstIter)
                    {
                        if (membersCache != null)
                        {
                            if (!membersCache.ContainsKey(tEntry.TaxonomyObject.PatternMemberID))
                                membersCache.Add(tEntry.TaxonomyObject.PatternMemberID, ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject));

                            result = membersCache[tEntry.TaxonomyObject.PatternMemberID];
                        }
                        else
                            result = ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject);

                        firstIter = false;
                    }
                    else
                    {
                        if (membersCache != null)
                        {
                            if (!membersCache.ContainsKey(tEntry.TaxonomyObject.PatternMemberID))
                                membersCache.Add(tEntry.TaxonomyObject.PatternMemberID, ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject));

                            result = result.IntersectWith(membersCache[tEntry.TaxonomyObject.PatternMemberID]);
                        }
                        else
                            result = result.IntersectWith(ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject));
                    }
                }

            }
            #endregion

            #region Union
            else if (ontCompClass is RDFOntologyUnionClass)
            {

                //Filter "unionOf" relations made with the given union class
                var uTaxonomy = ontology.Model.ClassModel.Relations.UnionOf.SelectEntriesBySubject(ontCompClass);
                foreach (var tEntry in uTaxonomy)
                {
                    if (membersCache != null)
                    {
                        if (!membersCache.ContainsKey(tEntry.TaxonomyObject.PatternMemberID))
                            membersCache.Add(tEntry.TaxonomyObject.PatternMemberID, ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject));

                        result = result.UnionWith(membersCache[tEntry.TaxonomyObject.PatternMemberID]);
                    }
                    else
                        result = result.UnionWith(ontology.GetMembersOf((RDFOntologyClass)tEntry.TaxonomyObject));
                }

            }
            #endregion

            #region Complement
            else if (ontCompClass is RDFOntologyComplementClass ontologyComplementClass)
            {
                if (membersCache != null)
                {
                    if (!membersCache.ContainsKey(ontologyComplementClass.ComplementOf.PatternMemberID))
                        membersCache.Add(ontologyComplementClass.ComplementOf.PatternMemberID, ontology.GetMembersOf(ontologyComplementClass.ComplementOf));

                    result = ontology.Data.DifferenceWith(membersCache[ontologyComplementClass.ComplementOf.PatternMemberID]);
                }
                else
                    result = ontology.Data.DifferenceWith(ontology.GetMembersOf(ontologyComplementClass.ComplementOf));
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given enumeration within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfEnumerate(this RDFOntology ontology, RDFOntologyEnumerateClass ontEnumClass)
        {
            var result = new RDFOntologyData();

            //Filter "oneOf" relations made with the given enumerate class
            var enTaxonomy = ontology.Model.ClassModel.Relations.OneOf.SelectEntriesBySubject(ontEnumClass);
            foreach (var tEntry in enTaxonomy)
            {

                //Add the fact and its synonyms
                if (tEntry.TaxonomySubject.IsEnumerateClass() && tEntry.TaxonomyObject.IsFact())
                {
                    result = result.UnionWith(ontology.Data.GetSameFactsAs((RDFOntologyFact)tEntry.TaxonomyObject))
                                   .AddFact((RDFOntologyFact)tEntry.TaxonomyObject);
                }

            }

            return result;
        }

        /// <summary>
        /// Enlists the facts which are members of the given restriction within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfRestriction(this RDFOntology ontology, RDFOntologyRestriction ontRestriction)
        {
            var result = new RDFOntologyData();

            //Enlist the properties which are compatible with the restriction's "OnProperty"
            var restrictionProperties = ontology.Model.PropertyModel.GetSubPropertiesOf(ontRestriction.OnProperty)
                                                                    .UnionWith(ontology.Model.PropertyModel.GetEquivalentPropertiesOf(ontRestriction.OnProperty))
                                                                    .AddProperty(ontRestriction.OnProperty);

            //Filter assertions made with enlisted compatible properties
            var restrictionAssertions = new RDFOntologyTaxonomy(ontology.Data.Relations.Assertions.Category, ontology.Data.Relations.Assertions.AcceptDuplicates);
            foreach (var property in restrictionProperties)
                restrictionAssertions = restrictionAssertions.UnionWith(ontology.Data.Relations.Assertions.SelectEntriesByPredicate(property));

            #region Cardinality
            if (ontRestriction is RDFOntologyCardinalityRestriction cardinalityRestriction)
            {

                //Item2 is a counter for occurrences of the restricted property within the subject fact
                var cardinalityRestrictionRegistry = new Dictionary<long, Tuple<RDFOntologyFact, long>>();

                //Iterate the compatible assertions
                foreach (var assertion in restrictionAssertions)
                {
                    if (!cardinalityRestrictionRegistry.ContainsKey(assertion.TaxonomySubject.PatternMemberID))
                    {
                        cardinalityRestrictionRegistry.Add(assertion.TaxonomySubject.PatternMemberID, new Tuple<RDFOntologyFact, long>((RDFOntologyFact)assertion.TaxonomySubject, 1));
                    }
                    else
                    {
                        var occurrencyCounter = cardinalityRestrictionRegistry[assertion.TaxonomySubject.PatternMemberID].Item2;
                        cardinalityRestrictionRegistry[assertion.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, long>((RDFOntologyFact)assertion.TaxonomySubject, occurrencyCounter + 1);
                    }
                }

                //Apply the cardinality restriction on the tracked facts
                var cardinalityRestrictionRegistryEnumerator = cardinalityRestrictionRegistry.Values.GetEnumerator();
                while (cardinalityRestrictionRegistryEnumerator.MoveNext())
                {
                    var passesMinCardinality = true;
                    var passesMaxCardinality = true;

                    //MinCardinality: signal tracked facts having "#occurrences < MinCardinality"
                    if (cardinalityRestriction.MinCardinality > 0)
                    {
                        if (cardinalityRestrictionRegistryEnumerator.Current.Item2 < cardinalityRestriction.MinCardinality)
                        {
                            passesMinCardinality = false;
                        }
                    }

                    //MaxCardinality: signal tracked facts having "#occurrences > MaxCardinality"
                    if (cardinalityRestriction.MaxCardinality > 0)
                    {
                        if (cardinalityRestrictionRegistryEnumerator.Current.Item2 > cardinalityRestriction.MaxCardinality)
                        {
                            passesMaxCardinality = false;
                        }
                    }

                    //Save the candidate fact if it passes cardinality restriction
                    if (passesMinCardinality && passesMaxCardinality)
                    {
                        result.AddFact(cardinalityRestrictionRegistryEnumerator.Current.Item1);
                    }
                }

            }
            #endregion

            #region QualifiedCardinality [OWL2]
            else if (ontRestriction is RDFOntologyQualifiedCardinalityRestriction qualifiedCardinalityRestriction)
            {

                //Item2 is a counter for occurrences of the restricted property within the subject fact
                var qualifiedCardinalityRestrictionRegistry = new Dictionary<long, Tuple<RDFOntologyFact, long>>();

                //Enlist the classes which are compatible with the restricted "OnClass"
                var onClasses = ontology.Model.ClassModel.GetSubClassesOf(qualifiedCardinalityRestriction.OnClass)
                                                         .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf(qualifiedCardinalityRestriction.OnClass))
                                                         .AddClass(qualifiedCardinalityRestriction.OnClass);

                //Iterate the compatible assertions
                var classTypesCache = new Dictionary<long, RDFOntologyClassModel>();
                foreach (var assertion in restrictionAssertions)
                {

                    //Iterate the class types of the object fact, checking presence of the restricted "OnClass"
                    var onClassFound = false;
                    var objectClassTypes = ontology.Data.Relations.ClassType.SelectEntriesBySubject(assertion.TaxonomyObject);
                    foreach (var objectClassType in objectClassTypes)
                    {
                        if (!classTypesCache.ContainsKey(objectClassType.TaxonomyObject.PatternMemberID))
                        {
                            classTypesCache.Add(objectClassType.TaxonomyObject.PatternMemberID,
                                                ontology.Model.ClassModel.GetSuperClassesOf((RDFOntologyClass)objectClassType.TaxonomyObject)
                                                                         .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf((RDFOntologyClass)objectClassType.TaxonomyObject))
                                                                         .AddClass((RDFOntologyClass)objectClassType.TaxonomyObject));
                        }

                        if (classTypesCache[objectClassType.TaxonomyObject.PatternMemberID].IntersectWith(onClasses).ClassesCount > 0)
                        {
                            onClassFound = true;
                            break;
                        }
                    }

                    //If classtype is compatible, proceed with qualified counters
                    if (onClassFound)
                    {
                        if (!qualifiedCardinalityRestrictionRegistry.ContainsKey(assertion.TaxonomySubject.PatternMemberID))
                        {
                            qualifiedCardinalityRestrictionRegistry.Add(assertion.TaxonomySubject.PatternMemberID, new Tuple<RDFOntologyFact, long>((RDFOntologyFact)assertion.TaxonomySubject, 1));
                        }
                        else
                        {
                            var occurrencyCounter = qualifiedCardinalityRestrictionRegistry[assertion.TaxonomySubject.PatternMemberID].Item2;
                            qualifiedCardinalityRestrictionRegistry[assertion.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, long>((RDFOntologyFact)assertion.TaxonomySubject, occurrencyCounter + 1);
                        }
                    }
                }

                //Apply the qualified cardinality restriction on the tracked facts
                var qualifiedCardinalityRestrictionRegistryEnumerator = qualifiedCardinalityRestrictionRegistry.Values.GetEnumerator();
                while (qualifiedCardinalityRestrictionRegistryEnumerator.MoveNext())
                {
                    var passesMinQualifiedCardinality = true;
                    var passesMaxQualifiedCardinality = true;

                    //MinQualifiedCardinality: signal tracked facts having "#occurrences < MinQualifiedCardinality"
                    if (qualifiedCardinalityRestriction.MinQualifiedCardinality > 0)
                    {
                        if (qualifiedCardinalityRestrictionRegistryEnumerator.Current.Item2 < qualifiedCardinalityRestriction.MinQualifiedCardinality)
                        {
                            passesMinQualifiedCardinality = false;
                        }
                    }

                    //MaxQualifiedCardinality: signal tracked facts having "#occurrences > MaxQualifiedCardinality"
                    if (qualifiedCardinalityRestriction.MaxQualifiedCardinality > 0)
                    {
                        if (qualifiedCardinalityRestrictionRegistryEnumerator.Current.Item2 > qualifiedCardinalityRestriction.MaxQualifiedCardinality)
                        {
                            passesMaxQualifiedCardinality = false;
                        }
                    }

                    //Save the candidate fact if it passes qualified cardinality restriction
                    if (passesMinQualifiedCardinality && passesMaxQualifiedCardinality)
                    {
                        result.AddFact(qualifiedCardinalityRestrictionRegistryEnumerator.Current.Item1);
                    }
                }

            }
            #endregion

            #region AllValuesFrom/SomeValuesFrom
            else if (ontRestriction is RDFOntologyAllValuesFromRestriction || ontRestriction is RDFOntologySomeValuesFromRestriction)
            {

                //Item2 is a counter for occurrences of the restricted property with a range member of the restricted "FromClass"
                //Item3 is a counter for occurrences of the restricted property with a range member not of the restricted "FromClass"
                var valuesFromRegistry = new Dictionary<long, Tuple<RDFOntologyFact, long, long>>();

                //Enlist the classes which are compatible with the restricted "FromClass"
                var classes = ontRestriction is RDFOntologyAllValuesFromRestriction
                                ? ontology.Model.ClassModel.GetSubClassesOf(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass)
                                                            .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass))
                                                            .AddClass(((RDFOntologyAllValuesFromRestriction)ontRestriction).FromClass)
                                : ontology.Model.ClassModel.GetSubClassesOf(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass)
                                                            .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass))
                                                            .AddClass(((RDFOntologySomeValuesFromRestriction)ontRestriction).FromClass);

                //Iterate the compatible assertions
                var classTypesCache = new Dictionary<long, RDFOntologyClassModel>();
                foreach (var assertion in restrictionAssertions)
                {

                    //Initialize the occurrence counters of the subject fact
                    if (!valuesFromRegistry.ContainsKey(assertion.TaxonomySubject.PatternMemberID))
                        valuesFromRegistry.Add(assertion.TaxonomySubject.PatternMemberID, new Tuple<RDFOntologyFact, long, long>((RDFOntologyFact)assertion.TaxonomySubject, 0, 0));

                    //Iterate the class types of the object fact, checking presence of the restricted "FromClass"
                    var fromClassFound = false;
                    var objectClassTypes = ontology.Data.Relations.ClassType.SelectEntriesBySubject(assertion.TaxonomyObject);
                    foreach (var objectClassType in objectClassTypes)
                    {
                        if (!classTypesCache.ContainsKey(objectClassType.TaxonomyObject.PatternMemberID))
                        {
                            classTypesCache.Add(objectClassType.TaxonomyObject.PatternMemberID,
                                                ontology.Model.ClassModel.GetSuperClassesOf((RDFOntologyClass)objectClassType.TaxonomyObject)
                                                                         .UnionWith(ontology.Model.ClassModel.GetEquivalentClassesOf((RDFOntologyClass)objectClassType.TaxonomyObject))
                                                                         .AddClass((RDFOntologyClass)objectClassType.TaxonomyObject));
                        }

                        if (classTypesCache[objectClassType.TaxonomyObject.PatternMemberID].IntersectWith(classes).ClassesCount > 0)
                        {
                            fromClassFound = true;
                            break;
                        }
                    }

                    //Update the occurrence counters of the subject fact
                    var equalityCounter = valuesFromRegistry[assertion.TaxonomySubject.PatternMemberID].Item2;
                    var differenceCounter = valuesFromRegistry[assertion.TaxonomySubject.PatternMemberID].Item3;
                    if (fromClassFound)
                    {
                        valuesFromRegistry[assertion.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, long, long>((RDFOntologyFact)assertion.TaxonomySubject, equalityCounter + 1, differenceCounter);
                    }
                    else
                    {
                        valuesFromRegistry[assertion.TaxonomySubject.PatternMemberID] = new Tuple<RDFOntologyFact, long, long>((RDFOntologyFact)assertion.TaxonomySubject, equalityCounter, differenceCounter + 1);
                    }

                }

                //Apply the restriction on the subject facts
                var valuesFromRegistryEnumerator = valuesFromRegistry.Values.GetEnumerator();
                while (valuesFromRegistryEnumerator.MoveNext())
                {
                    //AllValuesFrom
                    if (ontRestriction is RDFOntologyAllValuesFromRestriction)
                    {
                        if (valuesFromRegistryEnumerator.Current.Item2 >= 1 && valuesFromRegistryEnumerator.Current.Item3 == 0)
                        {
                            result.AddFact(valuesFromRegistryEnumerator.Current.Item1);
                        }
                    }
                    //SomeValuesFrom
                    else
                    {
                        if (valuesFromRegistryEnumerator.Current.Item2 >= 1)
                        {
                            result.AddFact(valuesFromRegistryEnumerator.Current.Item1);
                        }
                    }
                }

            }
            #endregion

            #region HasSelf [OWL2]
            else if (ontRestriction is RDFOntologyHasSelfRestriction hasSelfRestriction)
            {

                //Iterate the compatible assertions
                var sameFactsCache = new Dictionary<long, RDFOntologyData>();
                foreach (var assertion in restrictionAssertions.Where(x => x.TaxonomyObject.IsFact()))
                {

                    //Enlist the same facts of the assertion subject
                    if (!sameFactsCache.ContainsKey(assertion.TaxonomySubject.PatternMemberID))
                    {
                        sameFactsCache.Add(assertion.TaxonomySubject.PatternMemberID, ontology.Data.GetSameFactsAs((RDFOntologyFact)assertion.TaxonomySubject)
                                                                                                   .AddFact((RDFOntologyFact)assertion.TaxonomySubject));
                    }

                    if (sameFactsCache[assertion.TaxonomySubject.PatternMemberID].SelectFact(assertion.TaxonomySubject.ToString()) != null
                            && sameFactsCache[assertion.TaxonomySubject.PatternMemberID].SelectFact(assertion.TaxonomyObject.ToString()) != null)
                    {
                        result.AddFact((RDFOntologyFact)assertion.TaxonomySubject);
                    }

                }

            }
            #endregion

            #region HasValue
            else if (ontRestriction is RDFOntologyHasValueRestriction hasValueRestriction)
            {
                if (hasValueRestriction.RequiredValue.IsFact())
                {

                    //Enlist the same facts of the restriction's "RequiredValue"
                    var facts = ontology.Data.GetSameFactsAs((RDFOntologyFact)hasValueRestriction.RequiredValue)
                                             .AddFact((RDFOntologyFact)hasValueRestriction.RequiredValue);

                    //Iterate the compatible assertions and track the subject facts having the required value
                    foreach (var assertion in restrictionAssertions.Where(x => x.TaxonomyObject.IsFact()))
                    {
                        if (facts.SelectFact(assertion.TaxonomyObject.ToString()) != null)
                            result.AddFact((RDFOntologyFact)assertion.TaxonomySubject);
                    }

                }
                else if (hasValueRestriction.RequiredValue.IsLiteral())
                {

                    //Iterate the compatible assertions and track the subject facts having the required value
                    foreach (var assertion in restrictionAssertions.Where(x => x.TaxonomyObject.IsLiteral()))
                    {
                        if (RDFQueryUtilities.CompareRDFPatternMembers(hasValueRestriction.RequiredValue.Value, assertion.TaxonomyObject.Value) == 0)
                            result.AddFact((RDFOntologyFact)assertion.TaxonomySubject);
                    }

                }
            }
            #endregion

            return result;
        }

        /// <summary>
        /// Enlists the literals which are members of the given literal-compatible class within the given ontology
        /// </summary>
        internal static RDFOntologyData GetMembersOfLiteralCompatibleClass(this RDFOntology ontology, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyData();

            #region DataRange
            if (ontClass.IsDataRangeClass())
            {

                //Filter "oneOf" relations made with the given datarange class
                var drTaxonomy = ontology.Model.ClassModel.Relations.OneOf.SelectEntriesBySubject(ontClass);
                foreach (var tEntry in drTaxonomy)
                {

                    //Add the literal
                    if (tEntry.TaxonomySubject.IsDataRangeClass() && tEntry.TaxonomyObject.IsLiteral())
                    {
                        result.AddLiteral((RDFOntologyLiteral)tEntry.TaxonomyObject);
                    }

                }

            }
            #endregion

            #region Literal
            //Asking for "rdfs:Literal" is the only way to get enlistment of plain literals, since they have really no semantic
            else if (ontClass.Equals(RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()))
            {
                foreach (var ontLit in ontology.Data.Literals.Values)
                {
                    result.AddLiteral(ontLit);
                }
            }
            #endregion

            #region SubLiteral
            else
            {
                foreach (var ontLit in ontology.Data.Literals.Values.Where(l => l.Value is RDFTypedLiteral))
                {
                    var dTypeClass = ontology.Model.ClassModel.SelectClass(RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)ontLit.Value).Datatype));
                    if (dTypeClass != null)
                    {
                        if (dTypeClass.Equals(ontClass)
                                || ontology.Model.ClassModel.CheckIsSubClassOf(dTypeClass, ontClass)
                                    || ontology.Model.ClassModel.CheckIsEquivalentClassOf(dTypeClass, ontClass))
                        {
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
        internal static RDFOntologyData GetMembersOfNonLiteralCompatibleClass(this RDFOntology ontology, RDFOntologyClass ontClass)
        {
            var result = new RDFOntologyData();
            if (ontClass != null && ontology != null)
            {

                //Restriction
                if (ontClass.IsRestrictionClass())
                    result = ontology.GetMembersOfRestriction((RDFOntologyRestriction)ontClass);

                //Composite
                else if (ontClass.IsCompositeClass())
                    result = ontology.GetMembersOfComposite(ontClass);

                //Enumerate
                else if (ontClass.IsEnumerateClass())
                    result = ontology.GetMembersOfEnumerate((RDFOntologyEnumerateClass)ontClass);

                //Class
                else
                    result = ontology.GetMembersOfClass(ontClass);

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
        public static RDFOntology GetInferences(this RDFOntology ontology)
        {
            var result = new RDFOntology((RDFResource)ontology.Value);
            if (ontology != null)
            {
                result.Model = ontology.Model.GetInferences();
                result.Data = ontology.Data.GetInferences();
            }
            return result;
        }

        /// <summary>
        /// Gets an ontology model made by semantic inferences found in the given one
        /// </summary>
        public static RDFOntologyModel GetInferences(this RDFOntologyModel ontologyModel)
        {
            var result = new RDFOntologyModel();
            if (ontologyModel != null)
            {
                result.ClassModel = ontologyModel.ClassModel.GetInferences();
                result.PropertyModel = ontologyModel.PropertyModel.GetInferences();
            }
            return result;
        }

        /// <summary>
        /// Gets an ontology class model made by semantic inferences found in the given one
        /// </summary>
        public static RDFOntologyClassModel GetInferences(this RDFOntologyClassModel ontologyClassModel)
        {
            var result = new RDFOntologyClassModel();
            if (ontologyClassModel != null)
            {

                //SubClassOf
                foreach (var entry in ontologyClassModel.Relations.SubClassOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.SubClassOf.AddEntry(entry);

                //EquivalentClass
                foreach (var entry in ontologyClassModel.Relations.EquivalentClass.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.EquivalentClass.AddEntry(entry);

                //DisjointWith
                foreach (var entry in ontologyClassModel.Relations.DisjointWith.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.DisjointWith.AddEntry(entry);

                //UnionOf
                foreach (var entry in ontologyClassModel.Relations.UnionOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.UnionOf.AddEntry(entry);

                //IntersectionOf
                foreach (var entry in ontologyClassModel.Relations.IntersectionOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.IntersectionOf.AddEntry(entry);

                //OneOf
                foreach (var entry in ontologyClassModel.Relations.OneOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.OneOf.AddEntry(entry);

                //HasKey [OWL2]
                foreach (var entry in ontologyClassModel.Relations.HasKey.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.HasKey.AddEntry(entry);

            }
            return result;
        }

        /// <summary>
        /// Gets an ontology property model made by semantic inferences found in the given one
        /// </summary>
        public static RDFOntologyPropertyModel GetInferences(this RDFOntologyPropertyModel ontologyPropertyModel)
        {
            var result = new RDFOntologyPropertyModel();
            if (ontologyPropertyModel != null)
            {

                //SubPropertyOf
                foreach (var entry in ontologyPropertyModel.Relations.SubPropertyOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.SubPropertyOf.AddEntry(entry);

                //EquivalentProperty
                foreach (var entry in ontologyPropertyModel.Relations.EquivalentProperty.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.EquivalentProperty.AddEntry(entry);

                //InverseOf
                foreach (var entry in ontologyPropertyModel.Relations.InverseOf.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.InverseOf.AddEntry(entry);

                //PropertyDisjointWith [OWL2]
                foreach (var entry in ontologyPropertyModel.Relations.PropertyDisjointWith.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.PropertyDisjointWith.AddEntry(entry);

                //PropertyChainAxiom [OWL2]
                foreach (var entry in ontologyPropertyModel.Relations.PropertyChainAxiom.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.PropertyChainAxiom.AddEntry(entry);

            }
            return result;
        }

        /// <summary>
        /// Gets an ontology data made by semantic inferences found in the given one
        /// </summary>
        public static RDFOntologyData GetInferences(this RDFOntologyData ontologyData)
        {
            var result = new RDFOntologyData();
            if (ontologyData != null)
            {

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

                //NegativeAssertions [OWL2]
                foreach (var entry in ontologyData.Relations.NegativeAssertions.Where(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.API || tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                    result.Relations.NegativeAssertions.AddEntry(entry);

            }
            return result;
        }
        #endregion

        #region ClearInferences
        /// <summary>
        /// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
        /// </summary>
        public static void ClearInferences(this RDFOntology ontology)
        {
            if (ontology != null)
            {
                ontology.Model.ClearInferences();
                ontology.Data.ClearInferences();
            }
        }

        /// <summary>
        /// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
        /// </summary>
        public static void ClearInferences(this RDFOntologyModel ontologyModel)
        {
            if (ontologyModel != null)
            {
                ontologyModel.ClassModel.ClearInferences();
                ontologyModel.PropertyModel.ClearInferences();
            }
        }

        /// <summary>
        /// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
        /// </summary>
        public static void ClearInferences(this RDFOntologyClassModel ontologyClassModel)
        {
            if (ontologyClassModel != null)
            {
                //SubClassOf
                ontologyClassModel.Relations.SubClassOf.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //EquivalentClass
                ontologyClassModel.Relations.EquivalentClass.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //DisjointWith
                ontologyClassModel.Relations.DisjointWith.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //UnionOf
                ontologyClassModel.Relations.UnionOf.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //IntersectionOf
                ontologyClassModel.Relations.IntersectionOf.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //OneOf
                ontologyClassModel.Relations.OneOf.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //HasKey [OWL2]
                ontologyClassModel.Relations.HasKey.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
            }
        }

        /// <summary>
        /// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
        /// </summary>
        public static void ClearInferences(this RDFOntologyPropertyModel ontologyPropertyModel)
        {
            if (ontologyPropertyModel != null)
            {
                //SubPropertyOf
                ontologyPropertyModel.Relations.SubPropertyOf.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //EquivalentProperty
                ontologyPropertyModel.Relations.EquivalentProperty.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //InverseOf
                ontologyPropertyModel.Relations.InverseOf.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //PropertyDisjointWith [OWL2]
                ontologyPropertyModel.Relations.PropertyDisjointWith.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //PropertyChainAxiom [OWL2]
                ontologyPropertyModel.Relations.PropertyChainAxiom.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
            }
        }

        /// <summary>
        /// Clears all the taxonomy entries marked as semantic inferences generated by a reasoner
        /// </summary>
        public static void ClearInferences(this RDFOntologyData ontologyData)
        {
            if (ontologyData != null)
            {
                //ClassType
                ontologyData.Relations.ClassType.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //SameAs
                ontologyData.Relations.SameAs.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //DifferentFrom
                ontologyData.Relations.DifferentFrom.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //Assertions
                ontologyData.Relations.Assertions.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                //NegativeAssertions [OWL2]
                ontologyData.Relations.NegativeAssertions.Entries.RemoveAll(tEntry => tEntry.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
            }
        }
        #endregion

        #endregion

        #region Extensions

        #region Model Extensions
        /// <summary>
        /// Gets an ontology class of the given nature from the given RDF resource
        /// </summary>
        public static RDFOntologyClass ToRDFOntologyClass(this RDFResource ontResource,
                                                          RDFSemanticsEnums.RDFOntologyClassNature nature = RDFSemanticsEnums.RDFOntologyClassNature.OWL)
            => new RDFOntologyClass(ontResource, nature);

        /// <summary>
        /// Gets an ontology property from the given RDF resource
        /// </summary>
        public static RDFOntologyProperty ToRDFOntologyProperty(this RDFResource ontResource)
            => new RDFOntologyProperty(ontResource);

        /// <summary>
        /// Gets an ontology object property from the given RDF resource
        /// </summary>
        public static RDFOntologyObjectProperty ToRDFOntologyObjectProperty(this RDFResource ontResource)
            => new RDFOntologyObjectProperty(ontResource);

        /// <summary>
        /// Gets an ontology datatype property from the given RDF resource
        /// </summary>
        public static RDFOntologyDatatypeProperty ToRDFOntologyDatatypeProperty(this RDFResource ontResource)
            => new RDFOntologyDatatypeProperty(ontResource);

        /// <summary>
        /// Gets an ontology annotation property from the given RDF resource
        /// </summary>
        public static RDFOntologyAnnotationProperty ToRDFOntologyAnnotationProperty(this RDFResource ontResource)
            => new RDFOntologyAnnotationProperty(ontResource);

        /// <summary>
        /// Gets an ontology fact from the given RDF resource
        /// </summary>
        public static RDFOntologyFact ToRDFOntologyFact(this RDFResource ontResource)
            => new RDFOntologyFact(ontResource);

        /// <summary>
        /// Gets an ontology literal from the given RDF literal
        /// </summary>
        public static RDFOntologyLiteral ToRDFOntologyLiteral(this RDFLiteral ontLiteral)
            => new RDFOntologyLiteral(ontLiteral);
        #endregion

        #region Query Extensions
        /// <summary>
        /// Applies the given SPARQL SELECT query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static RDFSelectQueryResult ApplyToOntology(this RDFSelectQuery selectQuery,
                                                           RDFOntology ontology,
                                                           RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
        {
            if (selectQuery != null && ontology != null)
                return selectQuery.ApplyToGraph(ontology.ToRDFGraph(ontologyInferenceExportBehavior));

            return new RDFSelectQueryResult();
        }

        /// <summary>
        /// Asynchronously applies the given SPARQL SELECT query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static Task<RDFSelectQueryResult> ApplyToOntologyAsync(this RDFSelectQuery selectQuery,
                                                                      RDFOntology ontology,
                                                                      RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
            => Task.Run(() => ApplyToOntology(selectQuery, ontology, ontologyInferenceExportBehavior));

        /// <summary>
        /// Applies the given SPARQL ASK query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static RDFAskQueryResult ApplyToOntology(this RDFAskQuery askQuery,
                                                        RDFOntology ontology,
                                                        RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
        {
            if (askQuery != null && ontology != null)
                return askQuery.ApplyToGraph(ontology.ToRDFGraph(ontologyInferenceExportBehavior));

            return new RDFAskQueryResult();
        }

        /// <summary>
        /// Asynchronously applies the given SPARQL ASK query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static Task<RDFAskQueryResult> ApplyToOntologyAsync(this RDFAskQuery askQuery,
                                                                   RDFOntology ontology,
                                                                   RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
            => Task.Run(() => ApplyToOntology(askQuery, ontology, ontologyInferenceExportBehavior));

        /// <summary>
        /// Applies the given SPARQL CONSTRUCT query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static RDFConstructQueryResult ApplyToOntology(this RDFConstructQuery constructQuery,
                                                              RDFOntology ontology,
                                                              RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
        {
            if (constructQuery != null && ontology != null)
                return constructQuery.ApplyToGraph(ontology.ToRDFGraph(ontologyInferenceExportBehavior));

            return new RDFConstructQueryResult(RDFNamespaceRegister.DefaultNamespace.ToString());
        }

        /// <summary>
        /// Asynchronously applies the given SPARQL CONSTRUCT query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static Task<RDFConstructQueryResult> ApplyToOntologyAsync(this RDFConstructQuery constructQuery,
                                                                         RDFOntology ontology,
                                                                         RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
            => Task.Run(() => ApplyToOntology(constructQuery, ontology, ontologyInferenceExportBehavior));

        /// <summary>
        /// Applies the given SPARQL DESCRIBE query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static RDFDescribeQueryResult ApplyToOntology(this RDFDescribeQuery describeQuery,
                                                             RDFOntology ontology,
                                                             RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
        {
            if (describeQuery != null && ontology != null)
                return describeQuery.ApplyToGraph(ontology.ToRDFGraph(ontologyInferenceExportBehavior));

            return new RDFDescribeQueryResult(RDFNamespaceRegister.DefaultNamespace.ToString());
        }

        /// <summary>
        /// Asynchronously applies the given SPARQL DESCRIBE query to the given ontology (which is converted into
        /// a RDF graph including semantic inferences in respect of the given export behavior)
        /// </summary>
        public static Task<RDFDescribeQueryResult> ApplyToOntologyAsync(this RDFDescribeQuery describeQuery,
                                                                        RDFOntology ontology,
                                                                        RDFSemanticsEnums.RDFOntologyInferenceExportBehavior ontologyInferenceExportBehavior = RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.ModelAndData)
            => Task.Run(() => ApplyToOntology(describeQuery, ontology, ontologyInferenceExportBehavior));
        #endregion

        #region SemanticsExtensions
        /// <summary>
        /// Gets a graph representation of the given taxonomy, exporting inferences according to the selected behavior [OWL2]
        /// </summary>
        internal static RDFGraph ReifyToRDFGraph(this RDFOntologyTaxonomy taxonomy, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior, string taxonomyName)
        {
            RDFGraph result = new RDFGraph();
            switch (taxonomyName)
            {
                //Semantic-based reification
                case nameof(RDFOntologyDataMetadata.NegativeAssertions):
                    result = ReifyNegativeAssertionsTaxonomyToGraph(taxonomy, infexpBehavior);
                    break;

                //List-based reification
                case nameof(RDFOntologyClassModelMetadata.HasKey):
                case nameof(RDFOntologyPropertyModelMetadata.PropertyChainAxiom):
                case nameof(RDFOntologyDataMetadata.MemberList):
                    result = ReifyListTaxonomyToGraph(taxonomy, taxonomyName, infexpBehavior);
                    break;

                //Triple-based reification
                default:
                    result = ReifyTaxonomyToGraph(taxonomy, infexpBehavior);
                    break;
            }
            return result;
        }
        private static RDFGraph ReifyNegativeAssertionsTaxonomyToGraph(RDFOntologyTaxonomy taxonomy, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            RDFGraph result = new RDFGraph();
            foreach (RDFOntologyTaxonomyEntry te in taxonomy)
            {

                //Build reification triples of taxonomy entry
                RDFTriple teTriple = te.ToRDFTriple();

                //Do not export semantic inferences
                if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.None)
                {
                    if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                    {
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                        if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                        else
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                    }
                }

                //Export semantic inferences related only to ontology model
                else if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyModel)
                {
                    if (taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model ||
                            taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation)
                    {
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                        if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                        else
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                    }
                    else
                    {
                        if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                        {
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                            if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                                result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                            else
                                result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                        }
                    }
                }

                //Export semantic inferences related only to ontology data
                else if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyData)
                {
                    if (taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data ||
                            taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation)
                    {
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                        if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                        else
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                    }
                    else
                    {
                        if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                        {
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                            result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                            if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                                result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                            else
                                result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                        }
                    }
                }

                //Export semantic inferences related both to ontology model and data
                else
                {
                    result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION));
                    result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.SOURCE_INDIVIDUAL, (RDFResource)teTriple.Subject));
                    result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.ASSERTION_PROPERTY, (RDFResource)teTriple.Predicate));
                    if (teTriple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPL)
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_VALUE, (RDFLiteral)teTriple.Object));
                    else
                        result.AddTriple(new RDFTriple(teTriple.ReificationSubject, RDFVocabulary.OWL.TARGET_INDIVIDUAL, (RDFResource)teTriple.Object));
                }

            }
            return result;
        }
        private static RDFGraph ReifyListTaxonomyToGraph(RDFOntologyTaxonomy taxonomy, string taxonomyName, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            RDFGraph result = new RDFGraph();

            RDFResource taxonomyPredicate = taxonomyName.Equals(nameof(RDFOntologyClassModelMetadata.HasKey)) ? RDFVocabulary.OWL.HAS_KEY :
                                                taxonomyName.Equals(nameof(RDFOntologyPropertyModelMetadata.PropertyChainAxiom)) ? RDFVocabulary.OWL.PROPERTY_CHAIN_AXIOM :
                                                    taxonomyName.Equals(nameof(RDFOntologyDataMetadata.MemberList)) ? RDFVocabulary.SKOS.MEMBER_LIST :
                                                        null;
            if (taxonomyPredicate != null)
            {
                foreach (IGrouping<RDFOntologyResource, RDFOntologyTaxonomyEntry> tgroup in taxonomy.GroupBy(t => t.TaxonomySubject))
                {
                    //Build collection corresponding to the current subject of the given taxonomy
                    RDFCollection tgroupColl = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource, taxonomy.AcceptDuplicates);
                    foreach (RDFOntologyTaxonomyEntry tgroupEntry in tgroup.ToList())
                        tgroupColl.AddItem((RDFResource)tgroupEntry.TaxonomyObject.Value);
                    result.AddCollection(tgroupColl);

                    //Attach collection with taxonomy-specific predicate
                    result.AddTriple(new RDFTriple((RDFResource)tgroup.Key.Value, taxonomyPredicate, tgroupColl.ReificationSubject));
                }
            }

            return result;
        }
        private static RDFGraph ReifyTaxonomyToGraph(RDFOntologyTaxonomy taxonomy, RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            RDFGraph result = new RDFGraph();
            foreach (RDFOntologyTaxonomyEntry te in taxonomy)
            {

                //Do not export semantic inferences
                if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.None)
                {
                    if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                    {
                        result.AddTriple(te.ToRDFTriple());
                    }
                }

                //Export semantic inferences related only to ontology model
                else if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyModel)
                {
                    if (taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model
                            || taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation)
                    {
                        result.AddTriple(te.ToRDFTriple());
                    }
                    else
                    {
                        if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                        {
                            result.AddTriple(te.ToRDFTriple());
                        }
                    }
                }

                //Export semantic inferences related only to ontology data
                else if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyData)
                {
                    if (taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data
                            || taxonomy.Category == RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation)
                    {
                        result.AddTriple(te.ToRDFTriple());
                    }
                    else
                    {
                        if (te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None)
                        {
                            result.AddTriple(te.ToRDFTriple());
                        }
                    }
                }

                //Export semantic inferences related both to ontology model and data
                else
                {
                    result.AddTriple(te.ToRDFTriple());
                }

            }
            return result;
        }
        #endregion

        #endregion

    }

}
