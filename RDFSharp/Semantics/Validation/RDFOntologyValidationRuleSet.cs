/*
   Copyright 2012-2015 Marco De Salvo

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyValidationRuleSet represents a predefined set of RDFS/OWL-DL rules which are applied by a validator 
    /// on a given ontology in order to find error and inconsistency evidences affecting its model and data.
    /// </summary>
    internal static class RDFOntologyValidationRuleSet {

        #region Rule:Vocabulary_Reservation
        /// <summary>
        /// Validation rule checking usage of reserved RDF/RDFS/OWL terms in vocabulary of classes, properties and facts
        /// </summary>
        internal static List<RDFOntologyValidationEvidence> Vocabulary_Reservation(RDFOntology ontology) {
            var evidences    = new List<RDFOntologyValidationEvidence>();

            #region Classes
            foreach (var  c in ontology.Model.ClassModel.Where(cls => cls.ToString().StartsWith(RDFVocabulary.RDF.BASE_URI)  ||
                                                                      cls.ToString().StartsWith(RDFVocabulary.RDFS.BASE_URI) ||
                                                                      cls.ToString().StartsWith(RDFVocabulary.OWL.BASE_URI))) {
                if (!c.Equals(RDFVocabulary.OWL.THING)    && !c.Equals(RDFVocabulary.OWL.NOTHING)     &&
                    !c.Equals(RDFVocabulary.RDFS.LITERAL) && !c.Equals(RDFVocabulary.RDF.XML_LITERAL) && !c.Equals(RDFVocabulary.RDF.HTML)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                        RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                        "Vocabulary_Reservation",
                        String.Format("RDF/RDFS/OWL vocabularies are reserved, they cannot be used for class names."),
                        String.Format("Remove ontology class '{0}' from the class model.", c)
                     ));
                }
            }
            #endregion

            #region Properties
            foreach (var  p in ontology.Model.PropertyModel.Where(prop => prop.ToString().StartsWith(RDFVocabulary.RDF.BASE_URI)  ||
                                                                          prop.ToString().StartsWith(RDFVocabulary.RDFS.BASE_URI) ||
                                                                          prop.ToString().StartsWith(RDFVocabulary.OWL.BASE_URI))) {
                evidences.Add(new RDFOntologyValidationEvidence(
                    RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                    "Vocabulary_Reservation",
                    String.Format("RDF/RDFS/OWL vocabularies are reserved, they cannot be used for property names."),
                    String.Format("Remove ontology property '{0}' from the property model.", p)
                ));
            }
            #endregion

            #region Facts
            foreach (var  f in ontology.Data.Where(fact => fact.ToString().StartsWith(RDFVocabulary.RDF.BASE_URI)  ||
                                                           fact.ToString().StartsWith(RDFVocabulary.RDFS.BASE_URI) ||
                                                           fact.ToString().StartsWith(RDFVocabulary.OWL.BASE_URI))) {
                evidences.Add(new RDFOntologyValidationEvidence(
                    RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                    "Vocabulary_Reservation",
                    String.Format("RDF/RDFS/OWL vocabularies are reserved, they cannot be used for fact names."),
                    String.Format("Remove ontology fact '{0}' from the data.", f)
                ));
            }
            #endregion

            return evidences;
        }
        #endregion

        #region Rule:Vocabulary_Disjointness
        /// <summary>
        /// Validation rule checking for disjointness of vocabulary of classes, properties and facts
        /// </summary>
        internal static List<RDFOntologyValidationEvidence> Vocabulary_Disjointness(RDFOntology ontology) {
            var evidences   = new List<RDFOntologyValidationEvidence>();

            foreach (var    c in ontology.Model.ClassModel) {
                if (ontology.Model.PropertyModel.Properties.ContainsKey(c.PatternMemberID)) {
                    evidences.Add(new RDFOntologyValidationEvidence(
                        RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                        "Vocabulary_Disjointness",
                        String.Format("Disjointess of class model and property model is violated because the name '{0}' refers both to a class and a property.", c),
                        String.Format("Remove, or rename, one of the two entities.")
                    ));
                }
                if (ontology.Data.Facts.ContainsKey(c.PatternMemberID)) {
                    evidences.Add(new RDFOntologyValidationEvidence(
                        RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                        "Vocabulary_Disjointness",
                        String.Format("Disjointess of class model and data is violated because the name '{0}' refers both to a class and a fact.", c),
                        String.Format("Remove, or rename, one of the two entities.")
                    ));
                }
            }
            foreach (var    p in ontology.Model.PropertyModel) {
                if (ontology.Data.Facts.ContainsKey(p.PatternMemberID)) {
                    evidences.Add(new RDFOntologyValidationEvidence(
                        RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                        "Vocabulary_Disjointness",
                        String.Format("Disjointess of property model and data is violated because the name '{0}' refers both to a property and a fact.", p),
                        String.Format("Remove, or rename, one of the two entities.")
                    ));
                }
            }

            return evidences;
        }
        #endregion

        #region Rule:Vocabulary_Declaration
        /// <summary>
        /// Validation rule checking for declaration of classes, properties and facts
        /// </summary>
        internal static List<RDFOntologyValidationEvidence> Vocabulary_Declaration(RDFOntology ontology) {
            var evidences  = new List<RDFOntologyValidationEvidence>();

            #region Classes

            //SubClassOf
            foreach (var   c in ontology.Model.ClassModel.Relations.SubClassOf) {
                if  (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by a 'rdfs:subclassOf' relation.", c),
                          String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                      ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by a 'rdfs:subclassOf' relation.", c),
                         String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                     ));
                }
            }

            //EquivalentClass
            foreach (var   c in ontology.Model.ClassModel.Relations.EquivalentClass) {
                if  (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:EquivalentClass' relation.", c),
                          String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                      ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:EquivalentClass' relation.", c),
                         String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                     ));
                }
            }

            //DisjointWith
            foreach (var   c in ontology.Model.ClassModel.Relations.DisjointWith) {
                if  (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:DisjointWith' relation.", c),
                          String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                     ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:DisjointWith' relation.", c),
                         String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                     ));
                }
            }

            //OneOf
            foreach (var   c in ontology.Model.ClassModel.Relations.OneOf) {
                if  (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:oneOf' relation.", c),
                          String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                     ));
                }
            }

            //IntersectionOf
            foreach (var   c in ontology.Model.ClassModel.Relations.IntersectionOf) {
                if  (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:intersectionOf' relation.", c),
                          String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                     ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:intersectionOf' relation.", c),
                         String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                     ));
                }
            }

            //UnionOf
            foreach (var   c in ontology.Model.ClassModel.Relations.UnionOf) {
                if  (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:unionOf' relation.", c),
                          String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                     ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:unionOf' relation.", c),
                         String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                     ));
                }
            }

            //ComplementOf
            foreach (var complCls  in ontology.Model.ClassModel.Where(c => c is RDFOntologyComplementClass).OfType<RDFOntologyComplementClass>()) {
                if  (!ontology.Model.ClassModel.Classes.ContainsKey(complCls.ComplementOf.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology class '{0}' is not found in the class model: complement class '{1}' requires it.", complCls.ComplementOf, complCls),
                          String.Format("Add declaration of ontology class '{0}' to the class model.", complCls.ComplementOf)
                      ));
                }
            }

            //Domain / Range
            foreach (var p in ontology.Model.PropertyModel.Where(prop => prop.Domain != null || prop.Range != null)) {
                if  (p.Domain != null) {
                     if (!ontology.Model.ClassModel.Classes.ContainsKey(p.Domain.PatternMemberID)) {
                          evidences.Add(new RDFOntologyValidationEvidence(
                              RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                              "Vocabulary_Declaration",
                              String.Format("Declaration of ontology class '{0}' is not found in the class model: ontology property '{1}' requires it as 'rdfs:domain'.", p.Domain, p),
                              String.Format("Add declaration of ontology class '{0}' to the class model.", p.Domain)
                          ));
                     }
                }
                if  (p.Range != null) {
                     if (!ontology.Model.ClassModel.Classes.ContainsKey(p.Range.PatternMemberID)) {
                          evidences.Add(new RDFOntologyValidationEvidence(
                              RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                              "Vocabulary_Declaration",
                              String.Format("Declaration of ontology class '{0}' is not found in the class model: ontology property '{1}' requires it as 'rdfs:range'.", p.Range, p),
                              String.Format("Add declaration of ontology class '{0}' to the class model.", p.Range)
                          ));
                     }
                }
            }

            #endregion

            #region Restrictions

            var restrEnum  = ontology.Model.ClassModel.RestrictionsEnumerator;
            while(restrEnum.MoveNext()) {

                //OnProperty
                var onProp = restrEnum.Current.OnProperty;
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(onProp.Value.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology property '{0}' is not found in the property model: restriction class '{1}' requires it as 'owl:OnProperty'.", onProp, restrEnum.Current),
                         String.Format("Add declaration of ontology property '{0}' to the property model.", onProp)
                     ));
                }

                // AllValuesFrom
                if (restrEnum.Current is RDFOntologyAllValuesFromRestriction) {
                    var fromClass = ((RDFOntologyAllValuesFromRestriction)restrEnum.Current).FromClass;
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(fromClass.Value.PatternMemberID)) {
                         evidences.Add(new RDFOntologyValidationEvidence(
                             RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                             "Vocabulary_Declaration",
                             String.Format("Declaration of ontology class '{0}' is not found in the class model: restriction class '{1}' requires it as 'owl:FromClass'.", fromClass, restrEnum.Current),
                             String.Format("Add declaration of ontology class '{0}' to the class model.", fromClass)
                         ));
                    }
                }

                // SomeValuesFrom
                else if (restrEnum.Current is RDFOntologySomeValuesFromRestriction) {
                    var fromClass = ((RDFOntologySomeValuesFromRestriction)restrEnum.Current).FromClass;
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(fromClass.Value.PatternMemberID)) {
                         evidences.Add(new RDFOntologyValidationEvidence(
                             RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                             "Vocabulary_Declaration",
                             String.Format("Declaration of ontology class '{0}' is not found in the class model: restriction class '{1}' requires it as 'owl:FromClass'.", fromClass, restrEnum.Current),
                             String.Format("Add declaration of ontology class '{0}' to the class model.", fromClass)
                         ));
                    }
                }

                // HasValue
                else if (restrEnum.Current is RDFOntologyHasValueRestriction) {
                    var  requiredValue =    ((RDFOntologyHasValueRestriction)restrEnum.Current).RequiredValue;
                    if  (requiredValue.IsFact() && !ontology.Data.Facts.ContainsKey(requiredValue.Value.PatternMemberID)) {
                         evidences.Add(new RDFOntologyValidationEvidence(
                             RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                             "Vocabulary_Declaration",
                             String.Format("Declaration of ontology fact '{0}' is not found in the data: restriction class '{1}' requires it as 'owl:RequiredValue'.", requiredValue, restrEnum.Current),
                             String.Format("Add declaration of ontology fact '{0}' to the data.", requiredValue)
                         ));
                    }
                }

            }

            #endregion

            #region Properties

            //SubPropertyOf
            foreach (var   p in ontology.Model.PropertyModel.Relations.SubPropertyOf) {
                if  (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomySubject.Value.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by a 'rdfs:subpropertyOf' relation.", p),
                          String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomySubject)
                     ));
                }
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomyObject.Value.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by a 'rdfs:subpropertyOf' relation.", p),
                         String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomyObject)
                     ));
                }
            }

            //EquivalentProperty
            foreach (var   p in ontology.Model.PropertyModel.Relations.EquivalentProperty) {
                if  (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomySubject.Value.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:EquivalentProperty' relation.", p),
                          String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomySubject)
                     ));
                }
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomyObject.Value.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:EquivalentProperty' relation.", p),
                         String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomyObject)
                     ));
                }
            }

            //InverseOf
            foreach (var   p in ontology.Model.PropertyModel.Relations.InverseOf) {
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomySubject.Value.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:InverseOf' relation.", p),
                         String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomySubject)
                     ));
                }
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomyObject.Value.PatternMemberID)) {
                     evidences.Add(new RDFOntologyValidationEvidence(
                         RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                         "Vocabulary_Declaration",
                         String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:InverseOf' relation.", p),
                         String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomyObject)
                     ));
                }
            }

            #endregion

            #region Facts

            //ClassType
            foreach (var f in ontology.Data.Relations.ClassType) {
                if  (!ontology.Data.Facts.ContainsKey(f.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'rdf:type' relation.", f.TaxonomySubject),
                          String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomySubject)
                      ));
                }
                if  (!ontology.Model.ClassModel.Classes.ContainsKey(f.TaxonomyObject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by a 'rdf:type' relation.", f.TaxonomyObject),
                          String.Format("Add declaration of ontology class '{0}' to the class model.", f.TaxonomyObject)
                      ));
                }
            }
            foreach (var f in ontology.Data.Where(fact => ontology.Data.Relations.ClassType.SelectEntriesBySubject(fact).EntriesCount == 0)) {
                evidences.Add(new RDFOntologyValidationEvidence(
                    RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                    "Vocabulary_Declaration",
                    String.Format("Ontology fact '{0}' is found in the data, but it does not have classtype definitions.", f),
                    String.Format("Add at least one classtype definition for ontology fact '{0}' to the data.", f)
                ));
            }

            //SameAs
            foreach (var f in ontology.Data.Relations.SameAs) {
                if  (!ontology.Data.Facts.ContainsKey(f.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'owl:sameAs' relation.", f.TaxonomySubject),
                          String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomySubject)
                      ));
                }
                if  (!ontology.Data.Facts.ContainsKey(f.TaxonomyObject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'owl:sameAs' relation.", f.TaxonomyObject),
                          String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomyObject)
                      ));
                }
            }

            //DifferentFrom
            foreach (var f in ontology.Data.Relations.DifferentFrom) {
                if  (!ontology.Data.Facts.ContainsKey(f.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'owl:differentFrom' relation.", f.TaxonomySubject),
                          String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomySubject)
                      ));
                }
                if  (!ontology.Data.Facts.ContainsKey(f.TaxonomyObject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'owl:differentFrom' relation.", f.TaxonomyObject),
                          String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomyObject)
                      ));
                }
            }

            //Assertions
            foreach (var f in ontology.Data.Relations.Assertions) {
                if  (!ontology.Data.Facts.ContainsKey(f.TaxonomySubject.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by an assertion relation.", f.TaxonomySubject),
                          String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomySubject)
                      ));
                }
                if  (!ontology.Model.PropertyModel.Properties.ContainsKey(f.TaxonomyPredicate.PatternMemberID)) {
                      evidences.Add(new RDFOntologyValidationEvidence(
                          RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                          "Vocabulary_Declaration",
                          String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an assertion relation.", f.TaxonomyPredicate),
                          String.Format("Add declaration of ontology property '{0}' to the property model.", f.TaxonomyPredicate)
                      ));
                }
                if  (f.TaxonomyPredicate.IsObjectProperty()) {
                     if (!ontology.Data.Facts.ContainsKey(f.TaxonomyObject.PatternMemberID)) {
                          evidences.Add(new RDFOntologyValidationEvidence(
                              RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                              "Vocabulary_Declaration",
                              String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by an assertion relation.", f.TaxonomyObject),
                              String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomyObject)
                          ));
                     }
                }
            }

            #endregion

            return evidences;
        }
        #endregion

        #region Rule:Domain_Range
        /// <summary>
        /// Validation rule checking for consistency of rdfs:domain and rdfs:range axioms
        /// </summary>
        internal static List<RDFOntologyValidationEvidence> Domain_Range(RDFOntology ontology) {
            var evidences     = new List<RDFOntologyValidationEvidence>();
            var classCache    = new Dictionary<Int64, RDFOntologyData>();
            var litCheckCache = new Dictionary<Int64, Boolean>();
            foreach (var assertion  in ontology.Data.Relations.Assertions.Where(asn =>
                                                        ((RDFOntologyProperty)asn.TaxonomyPredicate).Domain != null ||
                                                        ((RDFOntologyProperty)asn.TaxonomyPredicate).Range  != null)) {

                #region Domain
                var domain    = ((RDFOntologyProperty)assertion.TaxonomyPredicate).Domain;
                if (domain   != null) {

                    //Domain class cannot be a datarange or compatible with rdfs:Literal
                    if (!litCheckCache.ContainsKey(domain.PatternMemberID)) {
                         litCheckCache.Add(domain.PatternMemberID, RDFOntologyReasonerHelper.IsLiteralCompatibleClass(domain, ontology.Model.ClassModel));
                    }
                    if (litCheckCache[domain.PatternMemberID]) {
                        evidences.Add(new RDFOntologyValidationEvidence(
                            RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                            "Domain_Range",
                            String.Format("Violation of 'rdfs:domain' constraint on property '{0}': ontology class '{1}' cannot be compatible with 'rdfs:Literal'.", assertion.TaxonomyPredicate, domain),
                            String.Format("Review relations of ontology class '{0}'.", domain)
                        ));
                    }
                    else {

                        //Cache-Miss
                        if (!classCache.ContainsKey(domain.PatternMemberID)) {
                             classCache.Add(domain.PatternMemberID, RDFOntologyReasonerHelper.EnlistMembersOf(domain, ontology));
                        }

                        //Cache-Check
                        if (classCache[domain.PatternMemberID].SelectFact(assertion.TaxonomySubject.ToString()) == null) {
                            evidences.Add(new RDFOntologyValidationEvidence(
                                RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                "Domain_Range",
                                String.Format("Violation of 'rdfs:domain' constraint on property '{0}': ontology fact '{1}' is incompatible with domain class '{2}'.", assertion.TaxonomyPredicate, assertion.TaxonomySubject, domain),
                                String.Format("Review classtypes of ontology fact '{0}'.", assertion.TaxonomySubject)
                            ));
                        }

                    }

                }
                #endregion

                #region Range
                var range     = ((RDFOntologyProperty)assertion.TaxonomyPredicate).Range;
                if (range    != null) {
                    if (((RDFOntologyProperty)assertion.TaxonomyPredicate).IsObjectProperty()) {

                        //Cache-Miss
                        if (!classCache.ContainsKey(range.PatternMemberID)) {
                             classCache.Add(range.PatternMemberID, RDFOntologyReasonerHelper.EnlistMembersOf(range, ontology));
                        }

                        //Cache-Check
                        if (classCache[range.PatternMemberID].SelectFact(assertion.TaxonomyObject.ToString()) == null) {
                            evidences.Add(new RDFOntologyValidationEvidence(
                                RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                "Domain_Range",
                                String.Format("Violation of 'rdfs:range' constraint on property '{0}': ontology fact '{1}' is incompatible with range class '{2}'.", assertion.TaxonomyPredicate, assertion.TaxonomyObject, range),
                                String.Format("Review classtypes of ontology fact '{0}'.", assertion.TaxonomyObject)
                            ));
                        }

                    }
                    else {

                        //Cache-Miss
                        if (!classCache.ContainsKey(range.PatternMemberID)) {
                             classCache.Add(range.PatternMemberID, RDFOntologyReasonerHelper.EnlistMembersOf(range, ontology));
                        }

                        //Cache-Check
                        if (classCache[range.PatternMemberID].SelectLiteral(assertion.TaxonomyObject.ToString()) == null) {
                            evidences.Add(new RDFOntologyValidationEvidence(
                                RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                "Domain_Range",
                                String.Format("Violation of 'rdfs:range' constraint on property '{0}': ontology literal '{1}' is incompatible with range class '{2}'.", assertion.TaxonomyPredicate, assertion.TaxonomyObject, range),
                                String.Format("Review datatype of ontology literal '{0}'.", assertion.TaxonomyObject)
                            ));
                        }

                    }
                }
                #endregion

            }
            return evidences;
        }
        #endregion

        #region Rule:ClassType
        /// <summary>
        /// Validation rule checking for consistency of rdf:type axioms
        /// </summary>
        internal static List<RDFOntologyValidationEvidence> ClassType(RDFOntology ontology) {
            var evidences           = new List<RDFOntologyValidationEvidence>();
            var disjWithCache       = new Dictionary<Int64, RDFOntologyClassModel>();
            var litCheckCache       = new Dictionary<Int64, Boolean>();

            #region Facts
            foreach (var  fact     in ontology.Data) {
                var classTypes      = new RDFOntologyClassModel();
                foreach (var cType in ontology.Data.Relations.ClassType.SelectEntriesBySubject(fact)) {

                    //ClassTypes of a fact cannot be compatible with "rdfs:Literal"
                    var cTypeClass  = ontology.Model.ClassModel.SelectClass(cType.TaxonomyObject.ToString());
                    if (cTypeClass != null) {
                        if (!litCheckCache.ContainsKey(cTypeClass.PatternMemberID)) {
                             litCheckCache.Add(cTypeClass.PatternMemberID, RDFOntologyReasonerHelper.IsLiteralCompatibleClass(cTypeClass, ontology.Model.ClassModel));
                        }
                        if (litCheckCache[cTypeClass.PatternMemberID]) {
                            evidences.Add(new RDFOntologyValidationEvidence(
                                RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                "ClassType",
                                String.Format("Ontology fact '{0}' has a classtype '{1}' which is compatible with 'rdfs:Literal' and this cannot be possible.", fact, cTypeClass),
                                String.Format("Review classtypes of ontology fact '{0}'.", fact)
                            ));
                        }
                        else {
                            classTypes.AddClass(cTypeClass);
                        }
                    }

                }

                //ClassTypes of a fact cannot be disjoint
                foreach (var cType in classTypes) {
                    if  (!disjWithCache.ContainsKey(cType.PatternMemberID)) {
                          disjWithCache.Add(cType.PatternMemberID, RDFOntologyReasonerHelper.EnlistDisjointClassesWith(cType, ontology.Model.ClassModel));
                    }
                    foreach (var disjWithCType in disjWithCache[cType.PatternMemberID].IntersectWith(classTypes)) {
                        evidences.Add(new RDFOntologyValidationEvidence(
                            RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                            "ClassType",
                            String.Format("Ontology fact '{0}' has both classtypes '{1}' and '{2}', which cannot be compatible because of an 'owl:disjointWith' constraint.", fact, cType, disjWithCType),
                            String.Format("Review classtypes of ontology fact '{0}'.", fact)
                        ));
                    }
                }

            }
            #endregion

            return evidences;
        }
        #endregion

        #region Rule:GlobalCardinalityConstraint
        /// <summary>
        /// Validation rule checking consistency of global cardinality constraints
        /// </summary>
        internal static List<RDFOntologyValidationEvidence> GlobalCardinalityConstraint(RDFOntology ontology) {
            var evidences         = new List<RDFOntologyValidationEvidence>();

            foreach (var prop    in ontology.Model.PropertyModel.Where(p => p.Functional || (p.IsObjectProperty() && ((RDFOntologyObjectProperty)p).InverseFunctional))) {
                var assertions    = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(prop);
                var subjCache     = new Dictionary<Int64, RDFOntologyResource>();
                var objCache      = new Dictionary<Int64, RDFOntologyResource>();

                foreach (var asn in assertions) {

                    #region Functional
                    if  (prop.IsFunctionalProperty()) {

                         if (!subjCache.ContainsKey(asn.TaxonomySubject.PatternMemberID)) {
                              subjCache.Add(asn.TaxonomySubject.PatternMemberID, asn.TaxonomySubject);
                         }
                         else {

                              //FunctionalProperty can only occur once per subject fact within assertions
                              evidences.Add(new RDFOntologyValidationEvidence(
                                  RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                  "GlobalCardinalityConstraint",
                                  String.Format("Ontology property '{0}' has an 'owl:FunctionalProperty' global cardinality constraint, which is violated by subject ontology fact '{1}'.", asn.TaxonomyPredicate, asn.TaxonomySubject),
                                  String.Format("Remove the '{0} {1} {2}' assertion from the data.", asn.TaxonomySubject, asn.TaxonomyPredicate, asn.TaxonomyObject)
                              ));

                         }

                         //FunctionalProperty cannot be TransitiveProperty (even indirectly)
                         if (prop.IsTransitiveProperty() || RDFOntologyReasonerHelper.EnlistSuperPropertiesOf(prop, ontology.Model.PropertyModel).Any(p => p.IsTransitiveProperty())) {
                             evidences.Add(new RDFOntologyValidationEvidence(
                                 RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                 "GlobalCardinalityConstraint",
                                 String.Format("Ontology property '{0}' has an 'owl:FunctionalProperty' global cardinality constraint, but it is also declared as 'owl:TransitiveProperty'. This is not allowed in OWL-DL.", prop),
                                 String.Format("Remove the 'owl:FunctionalProperty' global cardinality constraint from the ontology property '{0}', or unset this property as 'owl:TransitiveProperty'.", prop)
                             ));
                         }

                    }
                    #endregion

                    #region InverseFunctional
                    if  (prop.IsInverseFunctionalProperty()) {
                         if (!objCache.ContainsKey(asn.TaxonomyObject.PatternMemberID)) {
                              objCache.Add(asn.TaxonomyObject.PatternMemberID, asn.TaxonomyObject);
                         }
                         else {

                              //InverseFunctionalProperty can only occur once per object fact within assertions
                              evidences.Add(new RDFOntologyValidationEvidence(
                                  RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                  "GlobalCardinalityConstraint",
                                  String.Format("Ontology property '{0}' has an 'owl:InverseFunctionalProperty' global cardinality constraint, which is violated by object ontology fact '{1}'.", asn.TaxonomyPredicate, asn.TaxonomyObject),
                                  String.Format("Remove the '{0} {1} {2}' assertion from the data.", asn.TaxonomySubject, asn.TaxonomyPredicate, asn.TaxonomyObject)
                              ));

                         }

                         //InverseFunctionalProperty cannot be TransitiveProperty (even indirectly)
                         if (prop.IsTransitiveProperty() || RDFOntologyReasonerHelper.EnlistSuperPropertiesOf(prop, ontology.Model.PropertyModel).Any(p => p.IsTransitiveProperty())) {
                               evidences.Add(new RDFOntologyValidationEvidence(
                                   RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                   "GlobalCardinalityConstraint",
                                   String.Format("Ontology property '{0}' has an 'owl:InverseFunctionalProperty' global cardinality constraint, but it is also declared as 'owl:TransitiveProperty'. This is not allowed in OWL-DL.", prop),
                                   String.Format("Remove the 'owl:InverseFunctionalProperty' global cardinality constraint from the ontology property '{0}', or unset this property as 'owl:TransitiveProperty'.", prop)
                               ));
                         }

                    }
                    #endregion

                }
            }

            return evidences;
        }
        #endregion

        #region Rule:LocalCardinalityConstraint
        /// <summary>
        /// Validation rule checking consistency of local cardinality constraints
        /// </summary>
        internal static List<RDFOntologyValidationEvidence> LocalCardinalityConstraint(RDFOntology ontology) {
            var evidences           = new List<RDFOntologyValidationEvidence>();

            //OWL-DL requires that for a transitive property no local cardinality constraints should
            //be declared on the property itself, or its super properties, or its inverse properties.
            foreach (var cardRestr in ontology.Model.ClassModel.Where(c => c is RDFOntologyCardinalityRestriction).OfType<RDFOntologyCardinalityRestriction>()) {
                var  restrProp      = ontology.Model.PropertyModel.SelectProperty(cardRestr.OnProperty.ToString());
                if  (restrProp     != null) {
                    if (restrProp.IsObjectProperty()) {
                        if (((RDFOntologyObjectProperty)restrProp).Transitive) {
                              evidences.Add(new RDFOntologyValidationEvidence(
                                  RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                  "LocalCardinalityConstraint",
                                  String.Format("Ontology property '{0}' is an 'owl:TransitiveProperty', but it has a local cardinality constraint '{1}'. This is not permitted in OWL-DL.", restrProp, cardRestr),
                                  String.Format("Unset the ontology property '{0}' as 'owl:TransitiveProperty', or remove the local cardinality constraint '{1}' applied on it.", restrProp, cardRestr)
                              ));
                        }
                        foreach (var subProps in RDFOntologyReasonerHelper.EnlistSubPropertiesOf(restrProp, ontology.Model.PropertyModel)) {
                            if  (subProps.IsObjectProperty()) {
                                 if (((RDFOntologyObjectProperty)subProps).Transitive) {
                                       evidences.Add(new RDFOntologyValidationEvidence(
                                           RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                           "LocalCardinalityConstraint",
                                           String.Format("Ontology property '{0}' has a local cardinality constraint '{1}', but it is also super property of ontology property '{2}', which is an 'owl:TransitiveProperty'. This is not permitted in OWL-DL.", restrProp, cardRestr, subProps),
                                           String.Format("Unset the ontology property '{0}' as 'owl:TransitiveProperty', or remove the local cardinality constraint '{1}' applied on its super property '{2}'.", subProps, cardRestr, restrProp)
                                       ));
                                 }
                            }
                        }
                        foreach (var inverseProps in ontology.Model.PropertyModel.Relations.InverseOf.SelectEntriesBySubject(restrProp)) {
                            if  (((RDFOntologyObjectProperty)inverseProps.TaxonomyObject).Transitive) {
                                   evidences.Add(new RDFOntologyValidationEvidence(
                                       RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Error,
                                       "LocalCardinalityConstraint",
                                       String.Format("Ontology property '{0}' has a local cardinality constraint '{1}', but it also has an 'owl:inverseOf' relation with ontology property '{2}', which is an 'owl:TransitiveProperty'. This is not permitted in OWL-DL.", restrProp, cardRestr, inverseProps.TaxonomyObject),
                                       String.Format("Unset the ontology property '{0}' as 'owl:TransitiveProperty', or remove the local cardinality constraint '{1}' applied on its inverse ontology property '{2}'.", inverseProps.TaxonomyObject, cardRestr, restrProp)
                                   ));
                            }
                        }
                    }
                }
            }

            return evidences;
        }
        #endregion

        #region Rule:Deprecation
        /// <summary>
        /// Validation rule checking for usage of deprecated classes and properties
        /// </summary>
        internal static List<RDFOntologyValidationEvidence> Deprecation(RDFOntology ontology) {
            var evidences           = new List<RDFOntologyValidationEvidence>();

            #region Class
            foreach (var deprCls   in ontology.Data.Relations.ClassType.Where(c => c.TaxonomyObject.IsDeprecatedClass())) {
                evidences.Add(new RDFOntologyValidationEvidence(
                    RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                    "Deprecation",
                    String.Format("Ontology fact '{0}' has a classtype '{1}', which is deprecated (may be removed in a future ontology version!).", deprCls.TaxonomySubject, deprCls.TaxonomyObject),
                    String.Format("Update the classtype of ontology fact '{0}' to a non-deprecated class definition.", deprCls.TaxonomySubject)
                ));
            }
            #endregion

            #region Property
            foreach (var deprProp  in ontology.Data.Relations.Assertions.Where(p => p.TaxonomyPredicate.IsDeprecatedProperty())) {
                evidences.Add(new RDFOntologyValidationEvidence(
                    RDFSemanticsEnums.RDFOntologyValidationEvidenceCategory.Warning,
                    "Deprecation",
                    String.Format("Ontology fact '{0}' has an assertion using ontology property '{1}', which is deprecated (may be removed in a future ontology version!).", deprProp.TaxonomySubject, deprProp.TaxonomyPredicate),
                    String.Format("Update the assertion of ontology fact '{0}' to a non-deprecated property definition.", deprProp.TaxonomySubject)
                ));
            }
            #endregion

            return evidences;
        }
        #endregion

    }

}