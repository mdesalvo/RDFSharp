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

using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyValidatorRuleset implements a subset of RDFS/OWL-DL/OWL2 validation rules
    /// </summary>
    internal static class RDFOntologyValidatorRuleset
    {

        #region Rule:Vocabulary_Disjointness
        /// <summary>
        /// Validation rule checking for disjointness of vocabulary of classes, properties and facts
        /// </summary>
        internal static RDFOntologyValidatorReport Vocabulary_Disjointness(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'Vocabulary_Disjointness'...");

            #region ClassModel
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var c in ontology.Model.ClassModel)
            {
                if (ontology.Model.PropertyModel.Properties.ContainsKey(c.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                        "Vocabulary_Disjointness",
                        String.Format("Disjointess of class model and property model is violated because the name '{0}' refers both to a class and a property.", c),
                        String.Format("Remove, or rename, one of the two entities: at the moment, the given ontology is OWL Full!")
                    ));
                }
                if (ontology.Data.Facts.ContainsKey(c.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                        "Vocabulary_Disjointness",
                        String.Format("Disjointess of class model and data is violated because the name '{0}' refers both to a class and a fact.", c),
                        String.Format("Remove, or rename, one of the two entities: at the moment, the given ontology is OWL Full!")
                    ));
                }
            }
            #endregion

            #region PropertyModel
            foreach (var p in ontology.Model.PropertyModel)
            {
                if (ontology.Data.Facts.ContainsKey(p.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                        "Vocabulary_Disjointness",
                        String.Format("Disjointess of property model and data is violated because the name '{0}' refers both to a property and a fact.", p),
                        String.Format("Remove, or rename, one of the two entities: at the moment, the given ontology is OWL Full!")
                    ));
                }
            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'Vocabulary_Disjointness': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:Vocabulary_Declaration
        /// <summary>
        /// Validation rule checking for declaration of classes, properties and facts
        /// </summary>
        internal static RDFOntologyValidatorReport Vocabulary_Declaration(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'Vocabulary_Declaration'...");

            #region Classes

            //SubClassOf
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var c in ontology.Model.ClassModel.Relations.SubClassOf)
            {
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by a 'rdfs:subclassOf' relation.", c.TaxonomySubject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                    ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by a 'rdfs:subclassOf' relation.", c.TaxonomyObject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                    ));
                }
            }

            //EquivalentClass
            foreach (var c in ontology.Model.ClassModel.Relations.EquivalentClass)
            {
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:EquivalentClass' relation.", c.TaxonomySubject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                    ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:EquivalentClass' relation.", c.TaxonomyObject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                    ));
                }
            }

            //DisjointWith
            foreach (var c in ontology.Model.ClassModel.Relations.DisjointWith)
            {
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:DisjointWith' relation.", c.TaxonomySubject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                   ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:DisjointWith' relation.", c.TaxonomyObject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                    ));
                }
            }

            //OneOf
            foreach (var c in ontology.Model.ClassModel.Relations.OneOf)
            {
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:oneOf' relation.", c.TaxonomySubject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                   ));
                }
                if (c.TaxonomySubject.IsEnumerateClass())
                {
                    if (!ontology.Data.Facts.ContainsKey(c.TaxonomyObject.PatternMemberID))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                            "Vocabulary_Declaration",
                            String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by an 'owl:oneOf' relation.", c.TaxonomyObject),
                            String.Format("Add declaration of ontology fact '{0}' to the data.", c.TaxonomyObject)
                       ));
                    }
                }
            }

            //IntersectionOf
            foreach (var c in ontology.Model.ClassModel.Relations.IntersectionOf)
            {
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:intersectionOf' relation.", c.TaxonomySubject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                  ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:intersectionOf' relation.", c.TaxonomyObject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                   ));
                }
            }

            //UnionOf
            foreach (var c in ontology.Model.ClassModel.Relations.UnionOf)
            {
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:unionOf' relation.", c.TaxonomySubject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomySubject)
                  ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(c.TaxonomyObject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by an 'owl:unionOf' relation.", c.TaxonomyObject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", c.TaxonomyObject)
                   ));
                }
            }

            //ComplementOf
            foreach (var complCls in ontology.Model.ClassModel.Where(c => c is RDFOntologyComplementClass).OfType<RDFOntologyComplementClass>())
            {
                if (!ontology.Model.ClassModel.Classes.ContainsKey(complCls.ComplementOf.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: complement class '{1}' requires it.", complCls.ComplementOf, complCls),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", complCls.ComplementOf)
                   ));
                }
            }

            //Domain / Range
            foreach (var p in ontology.Model.PropertyModel.Where(prop => prop.Domain != null || prop.Range != null))
            {
                if (p.Domain != null)
                {
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(p.Domain.PatternMemberID))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                            "Vocabulary_Declaration",
                            String.Format("Declaration of ontology class '{0}' is not found in the class model: ontology property '{1}' requires it as 'rdfs:domain'.", p.Domain, p),
                            String.Format("Add declaration of ontology class '{0}' to the class model.", p.Domain)
                        ));
                    }
                }
                if (p.Range != null)
                {
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(p.Range.PatternMemberID))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                            "Vocabulary_Declaration",
                            String.Format("Declaration of ontology class '{0}' is not found in the class model: ontology property '{1}' requires it as 'rdfs:range'.", p.Range, p),
                            String.Format("Add declaration of ontology class '{0}' to the class model.", p.Range)
                        ));
                    }
                }
            }

            #endregion

            #region Restrictions
            foreach (var restrEnum in ontology.Model.ClassModel.Where(c => c.IsRestrictionClass()).OfType<RDFOntologyRestriction>())
            {

                //OnProperty
                var onProp = restrEnum.OnProperty;
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(onProp.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: restriction class '{1}' requires it as 'owl:OnProperty'.", onProp, restrEnum),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", onProp)
                    ));
                }

                // AllValuesFrom
                if (restrEnum is RDFOntologyAllValuesFromRestriction)
                {
                    var fromClass = ((RDFOntologyAllValuesFromRestriction)restrEnum).FromClass;
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(fromClass.Value.PatternMemberID))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                            "Vocabulary_Declaration",
                            String.Format("Declaration of ontology class '{0}' is not found in the class model: restriction class '{1}' requires it as 'owl:FromClass'.", fromClass, restrEnum),
                            String.Format("Add declaration of ontology class '{0}' to the class model.", fromClass)
                        ));
                    }
                }

                // SomeValuesFrom
                else if (restrEnum is RDFOntologySomeValuesFromRestriction)
                {
                    var fromClass = ((RDFOntologySomeValuesFromRestriction)restrEnum).FromClass;
                    if (!ontology.Model.ClassModel.Classes.ContainsKey(fromClass.Value.PatternMemberID))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                            "Vocabulary_Declaration",
                            String.Format("Declaration of ontology class '{0}' is not found in the class model: restriction class '{1}' requires it as 'owl:FromClass'.", fromClass, restrEnum),
                            String.Format("Add declaration of ontology class '{0}' to the class model.", fromClass)
                        ));
                    }
                }

                // HasValue
                else if (restrEnum is RDFOntologyHasValueRestriction)
                {
                    var reqValue = ((RDFOntologyHasValueRestriction)restrEnum).RequiredValue;
                    if (reqValue.IsFact() && !ontology.Data.Facts.ContainsKey(reqValue.Value.PatternMemberID))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                            "Vocabulary_Declaration",
                            String.Format("Declaration of ontology fact '{0}' is not found in the data: restriction class '{1}' requires it as 'owl:RequiredValue'.", reqValue, restrEnum),
                            String.Format("Add declaration of ontology fact '{0}' to the data.", reqValue)
                        ));
                    }
                }

            }
            #endregion

            #region Properties

            //SubPropertyOf
            foreach (var p in ontology.Model.PropertyModel.Relations.SubPropertyOf)
            {
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomySubject.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by a 'rdfs:subpropertyOf' relation.", p.TaxonomySubject),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomySubject)
                   ));
                }
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomyObject.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by a 'rdfs:subpropertyOf' relation.", p.TaxonomyObject),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomyObject)
                    ));
                }
            }

            //EquivalentProperty
            foreach (var p in ontology.Model.PropertyModel.Relations.EquivalentProperty)
            {
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomySubject.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:EquivalentProperty' relation.", p.TaxonomySubject),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomySubject)
                   ));
                }
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomyObject.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:EquivalentProperty' relation.", p.TaxonomyObject),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomyObject)
                    ));
                }
            }

            //PropertyDisjointWith
            foreach (var p in ontology.Model.PropertyModel.Relations.PropertyDisjointWith)
            {
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomySubject.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:PropertyDisjointWith' relation.", p.TaxonomySubject),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomySubject)
                   ));
                }
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomyObject.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:PropertyDisjointWith' relation.", p.TaxonomyObject),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomyObject)
                    ));
                }
            }

            //InverseOf
            foreach (var p in ontology.Model.PropertyModel.Relations.InverseOf)
            {
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomySubject.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:InverseOf' relation.", p.TaxonomySubject),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomySubject)
                    ));
                }
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(p.TaxonomyObject.Value.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an 'owl:InverseOf' relation.", p.TaxonomyObject),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", p.TaxonomyObject)
                    ));
                }
            }

            #endregion

            #region Facts

            //ClassType
            foreach (var f in ontology.Data.Relations.ClassType)
            {
                if (!ontology.Data.Facts.ContainsKey(f.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'rdf:type' relation.", f.TaxonomySubject),
                        String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomySubject)
                    ));
                }
                if (!ontology.Model.ClassModel.Classes.ContainsKey(f.TaxonomyObject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology class '{0}' is not found in the class model: it is required by a 'rdf:type' relation.", f.TaxonomyObject),
                        String.Format("Add declaration of ontology class '{0}' to the class model.", f.TaxonomyObject)
                    ));
                }
            }
            foreach (var f in ontology.Data.Where(fact => ontology.Data.Relations.ClassType.SelectEntriesBySubject(fact).EntriesCount == 0))
            {
                report.AddEvidence(new RDFOntologyValidatorEvidence(
                    RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                    "Vocabulary_Declaration",
                    String.Format("Ontology fact '{0}' is found in the data, but it does not have classtype definitions.", f),
                    String.Format("Add a classtype definition for ontology fact '{0}' to the data.", f)
                ));
            }

            //SameAs
            foreach (var f in ontology.Data.Relations.SameAs)
            {
                if (!ontology.Data.Facts.ContainsKey(f.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'owl:sameAs' relation.", f.TaxonomySubject),
                        String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomySubject)
                    ));
                }
                if (!ontology.Data.Facts.ContainsKey(f.TaxonomyObject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'owl:sameAs' relation.", f.TaxonomyObject),
                        String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomyObject)
                    ));
                }
            }

            //DifferentFrom
            foreach (var f in ontology.Data.Relations.DifferentFrom)
            {
                if (!ontology.Data.Facts.ContainsKey(f.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'owl:differentFrom' relation.", f.TaxonomySubject),
                        String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomySubject)
                    ));
                }
                if (!ontology.Data.Facts.ContainsKey(f.TaxonomyObject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by a 'owl:differentFrom' relation.", f.TaxonomyObject),
                        String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomyObject)
                    ));
                }
            }

            //Assertions
            foreach (var f in ontology.Data.Relations.Assertions)
            {
                if (!ontology.Data.Facts.ContainsKey(f.TaxonomySubject.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by an assertion relation.", f.TaxonomySubject),
                        String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomySubject)
                    ));
                }
                if (!ontology.Model.PropertyModel.Properties.ContainsKey(f.TaxonomyPredicate.PatternMemberID))
                {
                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                        "Vocabulary_Declaration",
                        String.Format("Declaration of ontology property '{0}' is not found in the property model: it is required by an assertion relation.", f.TaxonomyPredicate),
                        String.Format("Add declaration of ontology property '{0}' to the property model.", f.TaxonomyPredicate)
                    ));
                }
                if (f.TaxonomyPredicate.IsObjectProperty())
                {
                    if (!ontology.Data.Facts.ContainsKey(f.TaxonomyObject.PatternMemberID))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                            "Vocabulary_Declaration",
                            String.Format("Declaration of ontology fact '{0}' is not found in the data: it is required by an assertion relation.", f.TaxonomyObject),
                            String.Format("Add declaration of ontology fact '{0}' to the data.", f.TaxonomyObject)
                        ));
                    }
                }
            }

            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'Vocabulary_Declaration': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:Domain_Range
        /// <summary>
        /// Validation rule checking for consistency of rdfs:domain and rdfs:range axioms
        /// </summary>
        internal static RDFOntologyValidatorReport Domain_Range(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'Domain_Range'...");

            #region Domain_Range
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            var classCache = new Dictionary<Int64, RDFOntologyData>();
            var litCheckCache = new Dictionary<Int64, Boolean>();

            foreach (var assertion in ontology.Data.Relations.Assertions.Where(asn =>
                                                       ((RDFOntologyProperty)asn.TaxonomyPredicate).Domain != null ||
                                                       ((RDFOntologyProperty)asn.TaxonomyPredicate).Range != null))
            {

                #region Domain
                var domain = ((RDFOntologyProperty)assertion.TaxonomyPredicate).Domain;
                if (domain != null)
                {

                    //Domain class cannot be a datarange or literal-compatible class
                    if (!litCheckCache.ContainsKey(domain.PatternMemberID))
                    {
                        litCheckCache.Add(domain.PatternMemberID, ontology.Model.ClassModel.CheckIsLiteralCompatibleClass(domain));
                    }
                    if (litCheckCache[domain.PatternMemberID])
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "Domain_Range",
                            String.Format("Violation of 'rdfs:domain' constraint on property '{0}': ontology class '{1}' cannot be compatible with 'rdfs:Literal'.", assertion.TaxonomyPredicate, domain),
                            String.Format("Review relations of ontology class '{0}'.", domain)
                        ));
                    }
                    else
                    {

                        //Cache-Miss
                        if (!classCache.ContainsKey(domain.PatternMemberID))
                        {
                            //It's a non literal-compatible class, so we can speed up things by calling the internal method
                            classCache.Add(domain.PatternMemberID, ontology.GetMembersOfNonLiteralCompatibleClass(domain));
                        }

                        //Cache-Check
                        if (classCache[domain.PatternMemberID].SelectFact(assertion.TaxonomySubject.ToString()) == null)
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "Domain_Range",
                                String.Format("Violation of 'rdfs:domain' constraint on property '{0}': ontology fact '{1}' is incompatible with domain class '{2}'.", assertion.TaxonomyPredicate, assertion.TaxonomySubject, domain),
                                String.Format("Review classtypes of ontology fact '{0}'.", assertion.TaxonomySubject)
                            ));
                        }

                    }

                }
                #endregion

                #region Range
                var range = ((RDFOntologyProperty)assertion.TaxonomyPredicate).Range;
                if (range != null)
                {
                    if (((RDFOntologyProperty)assertion.TaxonomyPredicate).IsObjectProperty())
                    {

                        //Cache-Miss
                        if (!classCache.ContainsKey(range.PatternMemberID))
                        {
                            classCache.Add(range.PatternMemberID, ontology.GetMembersOf(range));
                        }

                        //Cache-Check
                        if (classCache[range.PatternMemberID].SelectFact(assertion.TaxonomyObject.ToString()) == null)
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "Domain_Range",
                                String.Format("Violation of 'rdfs:range' constraint on property '{0}': ontology fact '{1}' is incompatible with range class '{2}'.", assertion.TaxonomyPredicate, assertion.TaxonomyObject, range),
                                String.Format("Review classtypes of ontology fact '{0}'.", assertion.TaxonomyObject)
                            ));
                        }

                    }
                    else
                    {

                        //Cache-Miss
                        if (!classCache.ContainsKey(range.PatternMemberID))
                        {
                            classCache.Add(range.PatternMemberID, ontology.GetMembersOf(range));
                        }

                        //Cache-Check
                        if (classCache[range.PatternMemberID].SelectLiteral(assertion.TaxonomyObject.ToString()) == null)
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "Domain_Range",
                                String.Format("Violation of 'rdfs:range' constraint on property '{0}': ontology literal '{1}' is incompatible with range class '{2}'.", assertion.TaxonomyPredicate, assertion.TaxonomyObject, range),
                                String.Format("Review datatype of ontology literal '{0}'.", assertion.TaxonomyObject)
                            ));
                        }

                    }
                }
                #endregion

            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'Domain_Range': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:InverseOf
        /// <summary>
        /// Validation rule checking for consistency of owl:inverseOf axioms
        /// </summary>
        internal static RDFOntologyValidatorReport InverseOf(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'InverseOf'...");

            #region InverseOf
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var invOf in ontology.Model.PropertyModel.Relations.InverseOf)
            {

                #region Domain VS Range
                if (((RDFOntologyObjectProperty)invOf.TaxonomySubject).Domain != null &&
                    ((RDFOntologyObjectProperty)invOf.TaxonomyObject).Range != null)
                {
                    if (!ontology.Model.ClassModel.CheckIsRangeOf(((RDFOntologyObjectProperty)invOf.TaxonomySubject).Domain, (RDFOntologyObjectProperty)invOf.TaxonomyObject))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "InverseOf",
                            String.Format("Violation of 'rdfs:domain' constraint on 'owl:inverseOf' taxonomy between property '{0}' and property '{1}'.", invOf.TaxonomySubject, invOf.TaxonomyObject),
                            String.Format("Review 'rdfs:domain' constraint on ontology property '{0}' in order to be compatible with 'rdfs:range' constraint on ontology property '{1}'.", invOf.TaxonomySubject, invOf.TaxonomyObject)
                        ));
                    }
                }
                #endregion

                #region Range VS Domain
                if (((RDFOntologyObjectProperty)invOf.TaxonomySubject).Range != null &&
                    ((RDFOntologyObjectProperty)invOf.TaxonomyObject).Domain != null)
                {
                    if (!ontology.Model.ClassModel.CheckIsDomainOf(((RDFOntologyObjectProperty)invOf.TaxonomySubject).Range, (RDFOntologyObjectProperty)invOf.TaxonomyObject))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "InverseOf",
                            String.Format("Violation of 'rdfs:range' constraint on 'owl:inverseOf' taxonomy between property '{0}' and property '{1}'.", invOf.TaxonomySubject, invOf.TaxonomyObject),
                            String.Format("Review 'rdfs:range' constraint on ontology property '{0}' in order to be compatible with 'rdfs:domain' constraint on ontology property '{1}'.", invOf.TaxonomySubject, invOf.TaxonomyObject)
                        ));
                    }
                }
                #endregion

            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'InverseOf': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:SymmetricProperty
        /// <summary>
        /// Validation rule checking for consistency of owl:SymmetricProperty axioms
        /// </summary>
        internal static RDFOntologyValidatorReport SymmetricProperty(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'SymmetricProperty'...");

            #region SymmetricProperty
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var symProp in ontology.Model.PropertyModel.Where(prop => prop.IsSymmetricProperty() && (prop.Domain != null || prop.Range != null)))
            {
                foreach (var asn in ontology.Data.Relations.Assertions.Where(asn => asn.TaxonomyPredicate.Equals(symProp)))
                {

                    #region Domain
                    if (symProp.Domain != null)
                    {
                        if (!ontology.CheckIsMemberOf((RDFOntologyFact)asn.TaxonomyObject, symProp.Domain))
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "SymmetricProperty",
                                String.Format("Violation of 'owl:SymmetricProperty' constraint on property '{0}': range fact '{1}' is not compatible with domain class '{2}' of the property.", symProp, asn.TaxonomyObject, symProp.Domain),
                                String.Format("Review classtypes of ontology fact '{0}' in order to be compatible with 'rdfs:domain' constraint on ontology property '{1}'.", asn.TaxonomyObject, symProp)
                            ));
                        }
                    }
                    #endregion

                    #region Range
                    if (symProp.Range != null)
                    {
                        if (!ontology.CheckIsMemberOf((RDFOntologyFact)asn.TaxonomySubject, symProp.Range))
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "SymmetricProperty",
                                String.Format("Violation of 'owl:SymmetricProperty' constraint on property '{0}': domain fact '{1}' is not compatible with range class '{2}' of the property.", symProp, asn.TaxonomySubject, symProp.Range),
                                String.Format("Review classtypes of ontology fact '{0}' in order to be compatible with 'rdfs:range' constraint on ontology property '{1}'.", asn.TaxonomySubject, symProp)
                            ));
                        }
                    }
                    #endregion

                }
            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'SymmetricProperty': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:AsymmetricProperty [OWL2]
        /// <summary>
        /// Validation rule checking for consistency of owl:AsymmetricProperty axioms [OWL2]
        /// </summary>
        internal static RDFOntologyValidatorReport AsymmetricProperty(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'AsymmetricProperty'...");

            #region AsymmetricProperty
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var asymProp in ontology.Model.PropertyModel.Where(prop => prop.IsAsymmetricProperty()))
            {
                foreach (var asn in ontology.Data.Relations.Assertions.Where(asn => asn.TaxonomyPredicate.Equals(asymProp)))
                {

                    if (ontology.Data.Relations.Assertions.Any(x => x.TaxonomyPredicate.Equals(asn.TaxonomyPredicate)
                                                                        && x.TaxonomySubject.Equals(asn.TaxonomyObject)
                                                                            && x.TaxonomyObject.Equals(asn.TaxonomySubject)))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "AsymmetricProperty",
                            String.Format("Violation of 'owl:AsymmetricProperty' constraint on property '{0}'.", asymProp),
                            String.Format("Remove the assertion '{0}->{1}->{2}' from the ontology data.", asn.TaxonomyObject, asn.TaxonomyPredicate, asn.TaxonomySubject)
                        ));
                    }

                }
            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'AsymmetricProperty': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:IrreflexiveProperty [OWL2]
        /// <summary>
        /// Validation rule checking for consistency of owl:IrreflexiveProperty axioms [OWL2]
        /// </summary>
        internal static RDFOntologyValidatorReport IrreflexiveProperty(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'IrreflexiveProperty'...");

            #region IrreflexiveProperty
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var irrefProp in ontology.Model.PropertyModel.Where(prop => prop.IsIrreflexiveProperty()))
            {
                foreach (var asn in ontology.Data.Relations.Assertions.Where(asn => asn.TaxonomyPredicate.Equals(irrefProp)))
                {

                    if (asn.TaxonomySubject.Equals(asn.TaxonomyObject))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "IrreflexiveProperty",
                            String.Format("Violation of 'owl:IrreflexiveProperty' constraint on property '{0}'.", irrefProp),
                            String.Format("Remove the assertion '{0}->{1}->{2}' from the ontology data.", asn.TaxonomySubject, asn.TaxonomyPredicate, asn.TaxonomyObject)
                        ));
                    }

                }
            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'IrreflexiveProperty': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:PropertyDisjoint [OWL2]
        /// <summary>
        /// Validation rule checking for consistency of owl:propertyDisjointWith axioms [OWL2]
        /// </summary>
        internal static RDFOntologyValidatorReport PropertyDisjoint(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'PropertyDisjoint'...");

            #region PropertyDisjoint
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var propertyDisjointWithRelation in ontology.Model.PropertyModel.Relations.PropertyDisjointWith)
            {

                //Calculate properties compatible with left-side of disjointness relation (equivalent properties / subproperties)
                var leftSideProps = ontology.Model.PropertyModel.GetPropertiesDisjointWith((RDFOntologyProperty)propertyDisjointWithRelation.TaxonomyObject);

                //Calculate properties compatible with right-side of disjointness relation (equivalent properties / subproperties)
                var rightSideProps = ontology.Model.PropertyModel.GetPropertiesDisjointWith((RDFOntologyProperty)propertyDisjointWithRelation.TaxonomySubject);

                //Validate disjointness relation
                foreach (var asn in ontology.Data.Relations.Assertions.Where(asn => leftSideProps.SelectProperty(asn.TaxonomyPredicate.ToString()) != null))
                {

                    //Calculate facts compatible with subject of assertion
                    var subjects = ontology.Data.GetSameFactsAs((RDFOntologyFact)asn.TaxonomySubject)
                                                .AddFact((RDFOntologyFact)asn.TaxonomySubject);

                    //Calculate facts/literals compatible with object of assertion
                    var objectIsFact = asn.TaxonomyObject.IsFact();
                    var objects = objectIsFact ? ontology.Data.GetSameFactsAs((RDFOntologyFact)asn.TaxonomyObject)
                                                              .AddFact((RDFOntologyFact)asn.TaxonomyObject)
                                               : new RDFOntologyData().AddLiteral((RDFOntologyLiteral)asn.TaxonomyObject);

                    //Cannot connect same individuals with disjoint property
                    foreach (var disjAsn in ontology.Data.Relations.Assertions.Where(a => rightSideProps.SelectProperty(a.TaxonomyPredicate.ToString()) != null
                                                                                            && subjects.SelectFact(a.TaxonomySubject.ToString()) != null
                                                                                                && ((objectIsFact && objects.SelectFact(a.TaxonomyObject.ToString()) != null)
                                                                                                       || objects.SelectLiteral(a.TaxonomyObject.ToString()) != null)))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "PropertyDisjoint",
                            String.Format("Violation of disjointness between ontology properties '{0}' and '{1}'.", propertyDisjointWithRelation.TaxonomySubject, propertyDisjointWithRelation.TaxonomyObject),
                            String.Format("Remove assertion '{0}' from ontology data, or review disjointness relation between properties '{1}' and '{2}'.", disjAsn, propertyDisjointWithRelation.TaxonomySubject, propertyDisjointWithRelation.TaxonomyObject)
                        ));
                    }

                }

            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'PropertyDisjoint': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:NegativeAssertions [OWL2]
        /// <summary>
        /// Validation rule checking for consistency of owl:NegativePropertyAssertion axioms [OWL2]
        /// </summary>
        internal static RDFOntologyValidatorReport NegativeAssertions(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'NegativeAssertions'...");

            #region NegativeAssertions
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var negativeAssertion in ontology.Data.Relations.NegativeAssertions)
            {
                //Enlist the facts which are compatible with negative assertion subject
                RDFOntologyData compatibleSubjects = ontology.Data.GetSameFactsAs((RDFOntologyFact)negativeAssertion.TaxonomySubject)
                                                                  .AddFact((RDFOntologyFact)negativeAssertion.TaxonomySubject);

                //Enlist the properties which are compatible with negative assertion predicate
                RDFOntologyPropertyModel compatibleProperties = ontology.Model.PropertyModel.GetSubPropertiesOf((RDFOntologyProperty)negativeAssertion.TaxonomyPredicate)
                                                                                            .UnionWith(ontology.Model.PropertyModel.GetEquivalentPropertiesOf((RDFOntologyProperty)negativeAssertion.TaxonomyPredicate))
                                                                                            .AddProperty((RDFOntologyProperty)negativeAssertion.TaxonomyPredicate);

                if (negativeAssertion.TaxonomyObject is RDFOntologyFact)
                {
                    //Enlist the facts which are compatible with negative assertion object
                    RDFOntologyData compatibleObjects = ontology.Data.GetSameFactsAs((RDFOntologyFact)negativeAssertion.TaxonomyObject)
                                                                      .AddFact((RDFOntologyFact)negativeAssertion.TaxonomyObject);

                    //Check if negative assertion is violated by any existing assertions
                    if (ontology.Data.Relations.Assertions.Any(asn => compatibleSubjects.Any(subj => subj.Equals(asn.TaxonomySubject))
                                                                        && compatibleProperties.Any(pred => pred.Equals(asn.TaxonomyPredicate))
                                                                            && compatibleObjects.Any(obj => obj.Equals(asn.TaxonomyObject))))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "NegativeAssertions",
                            String.Format("Violation of negative assertion '{0}'.", negativeAssertion),
                            String.Format("Review negative assertion '{0}' because ontology data contains at least one assertion (may be an inference) violating it.", negativeAssertion)
                        ));
                    }
                }
                else
                {
                    //Check if negative assertion is violated by any existing assertions
                    if (ontology.Data.Relations.Assertions.Any(asn => compatibleSubjects.Any(subj => subj.Equals(asn.TaxonomySubject))
                                                                        && compatibleProperties.Any(pred => pred.Equals(asn.TaxonomyPredicate))
                                                                            && negativeAssertion.TaxonomyObject.Equals(asn.TaxonomyObject)))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "NegativeAssertions",
                            String.Format("Violation of negative assertion '{0}'.", negativeAssertion),
                            String.Format("Review negative assertion '{0}' because ontology data contains at least one assertion (maybe an A-BOX inference) violating it.", negativeAssertion)
                        ));
                    }
                }
            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'NegativeAssertions': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:ClassType
        /// <summary>
        /// Validation rule checking for consistency of rdf:type axioms
        /// </summary>
        internal static RDFOntologyValidatorReport ClassType(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'ClassType'...");

            #region Facts
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            var disjWithCache = new Dictionary<Int64, RDFOntologyClassModel>();
            var litCheckCache = new Dictionary<Int64, Boolean>();

            foreach (var fact in ontology.Data)
            {
                var classTypes = new RDFOntologyClassModel();
                foreach (var cType in ontology.Data.Relations.ClassType.SelectEntriesBySubject(fact))
                {

                    //ClassTypes of a fact cannot be compatible with "rdfs:Literal"
                    var cTypeClass = ontology.Model.ClassModel.SelectClass(cType.TaxonomyObject.ToString());
                    if (cTypeClass != null)
                    {
                        if (!litCheckCache.ContainsKey(cTypeClass.PatternMemberID))
                        {
                            litCheckCache.Add(cTypeClass.PatternMemberID, ontology.Model.ClassModel.CheckIsLiteralCompatibleClass(cTypeClass));
                        }
                        if (litCheckCache[cTypeClass.PatternMemberID])
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "ClassType",
                                String.Format("Ontology fact '{0}' has a classtype '{1}' which is compatible with 'rdfs:Literal' and this cannot be possible.", fact, cTypeClass),
                                String.Format("Review classtypes of ontology fact '{0}'.", fact)
                            ));
                        }
                        else
                        {
                            classTypes.AddClass(cTypeClass);
                        }
                    }

                }

                //ClassTypes of a fact cannot be disjoint
                foreach (var cType in classTypes)
                {
                    if (!disjWithCache.ContainsKey(cType.PatternMemberID))
                    {
                        disjWithCache.Add(cType.PatternMemberID, ontology.Model.ClassModel.GetDisjointClassesWith(cType));
                    }
                    foreach (var disjWithCType in disjWithCache[cType.PatternMemberID].IntersectWith(classTypes))
                    {
                        report.AddEvidence(new RDFOntologyValidatorEvidence(
                            RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                            "ClassType",
                            String.Format("Ontology fact '{0}' has both classtypes '{1}' and '{2}', which cannot be compatible because of an 'owl:disjointWith' constraint.", fact, cType, disjWithCType),
                            String.Format("Review classtypes of ontology fact '{0}'.", fact)
                        ));
                    }
                }

            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'ClassType': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:GlobalCardinalityConstraint
        /// <summary>
        /// Validation rule checking consistency of global cardinality constraints
        /// </summary>
        internal static RDFOntologyValidatorReport GlobalCardinalityConstraint(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'GlobalCardinalityConstraint'...");

            #region GlobalCardinalityConstraint
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var prop in ontology.Model.PropertyModel.Where(p => p.Functional || (p.IsObjectProperty() && ((RDFOntologyObjectProperty)p).InverseFunctional)))
            {
                var assertions = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(prop);
                var subjCache = new Dictionary<Int64, RDFOntologyResource>();
                var objCache = new Dictionary<Int64, RDFOntologyResource>();
                foreach (var asn in assertions)
                {

                    #region Functional
                    if (prop.IsFunctionalProperty())
                    {

                        if (!subjCache.ContainsKey(asn.TaxonomySubject.PatternMemberID))
                        {
                            subjCache.Add(asn.TaxonomySubject.PatternMemberID, asn.TaxonomySubject);
                        }
                        else
                        {

                            //FunctionalProperty can only occur once per subject fact within assertions
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "GlobalCardinalityConstraint",
                                String.Format("Ontology property '{0}' has an 'owl:FunctionalProperty' global cardinality constraint, which is violated by subject ontology fact '{1}'.", asn.TaxonomyPredicate, asn.TaxonomySubject),
                                String.Format("Remove the '{0} {1} {2}' assertion from the data.", asn.TaxonomySubject, asn.TaxonomyPredicate, asn.TaxonomyObject)
                            ));

                        }

                        //FunctionalProperty cannot be TransitiveProperty (even indirectly)
                        if (prop.IsTransitiveProperty() || ontology.Model.PropertyModel.GetSuperPropertiesOf(prop).Any(p => p.IsTransitiveProperty()))
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "GlobalCardinalityConstraint",
                                String.Format("Ontology property '{0}' has an 'owl:FunctionalProperty' global cardinality constraint, but it is also declared as 'owl:TransitiveProperty'. This is not allowed in OWL-DL.", prop),
                                String.Format("Remove the 'owl:FunctionalProperty' global cardinality constraint from the ontology property '{0}', or unset this property as 'owl:TransitiveProperty'.", prop)
                            ));
                        }

                    }
                    #endregion

                    #region InverseFunctional
                    if (prop.IsInverseFunctionalProperty())
                    {
                        if (!objCache.ContainsKey(asn.TaxonomyObject.PatternMemberID))
                        {
                            objCache.Add(asn.TaxonomyObject.PatternMemberID, asn.TaxonomyObject);
                        }
                        else
                        {

                            //InverseFunctionalProperty can only occur once per object fact within assertions
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "GlobalCardinalityConstraint",
                                String.Format("Ontology property '{0}' has an 'owl:InverseFunctionalProperty' global cardinality constraint, which is violated by object ontology fact '{1}'.", asn.TaxonomyPredicate, asn.TaxonomyObject),
                                String.Format("Remove the '{0} {1} {2}' assertion from the data.", asn.TaxonomySubject, asn.TaxonomyPredicate, asn.TaxonomyObject)
                            ));

                        }

                        //InverseFunctionalProperty cannot be TransitiveProperty (even indirectly)
                        if (prop.IsTransitiveProperty() || ontology.Model.PropertyModel.GetSuperPropertiesOf(prop).Any(p => p.IsTransitiveProperty()))
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "GlobalCardinalityConstraint",
                                String.Format("Ontology property '{0}' has an 'owl:InverseFunctionalProperty' global cardinality constraint, but it is also declared as 'owl:TransitiveProperty'. This is not allowed in OWL-DL.", prop),
                                String.Format("Remove the 'owl:InverseFunctionalProperty' global cardinality constraint from the ontology property '{0}', or unset this property as 'owl:TransitiveProperty'.", prop)
                            ));
                        }

                    }
                    #endregion

                }
            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'GlobalCardinalityConstraint': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:LocalCardinalityConstraint
        /// <summary>
        /// Validation rule checking consistency of local cardinality constraints
        /// </summary>
        internal static RDFOntologyValidatorReport LocalCardinalityConstraint(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'LocalCardinalityConstraint'...");

            #region LocalCardinalityConstraint
            //OWL-DL requires that for a transitive property no local cardinality constraints should
            //be declared on the property itself, or its super properties, or its inverse properties.
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            var restrictions = ontology.Model.ClassModel.Where(c => c is RDFOntologyCardinalityRestriction || c is RDFOntologyQualifiedCardinalityRestriction)
                                                        .OfType<RDFOntologyRestriction>();
            foreach (var cardRestr in restrictions)
            {
                var restrProp = ontology.Model.PropertyModel.SelectProperty(cardRestr.OnProperty.ToString());
                if (restrProp != null)
                {
                    if (restrProp.IsObjectProperty())
                    {
                        if (((RDFOntologyObjectProperty)restrProp).Transitive)
                        {
                            report.AddEvidence(new RDFOntologyValidatorEvidence(
                                RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                "LocalCardinalityConstraint",
                                String.Format("Ontology property '{0}' is an 'owl:TransitiveProperty', but it has a local cardinality constraint '{1}'. This is not permitted in OWL-DL.", restrProp, cardRestr),
                                String.Format("Unset the ontology property '{0}' as 'owl:TransitiveProperty', or remove the local cardinality constraint '{1}' applied on it.", restrProp, cardRestr)
                            ));
                        }
                        foreach (var subProps in ontology.Model.PropertyModel.GetSubPropertiesOf(restrProp))
                        {
                            if (subProps.IsObjectProperty())
                            {
                                if (((RDFOntologyObjectProperty)subProps).Transitive)
                                {
                                    report.AddEvidence(new RDFOntologyValidatorEvidence(
                                        RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                        "LocalCardinalityConstraint",
                                        String.Format("Ontology property '{0}' has a local cardinality constraint '{1}', but it is also super property of ontology property '{2}', which is an 'owl:TransitiveProperty'. This is not permitted in OWL-DL.", restrProp, cardRestr, subProps),
                                        String.Format("Unset the ontology property '{0}' as 'owl:TransitiveProperty', or remove the local cardinality constraint '{1}' applied on its super property '{2}'.", subProps, cardRestr, restrProp)
                                    ));
                                }
                            }
                        }
                        foreach (var inverseProps in ontology.Model.PropertyModel.Relations.InverseOf.SelectEntriesBySubject(restrProp))
                        {
                            if (((RDFOntologyObjectProperty)inverseProps.TaxonomyObject).Transitive)
                            {
                                report.AddEvidence(new RDFOntologyValidatorEvidence(
                                    RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Error,
                                    "LocalCardinalityConstraint",
                                    String.Format("Ontology property '{0}' has a local cardinality constraint '{1}', but it also has an 'owl:inverseOf' relation with ontology property '{2}', which is an 'owl:TransitiveProperty'. This is not permitted in OWL-DL.", restrProp, cardRestr, inverseProps.TaxonomyObject),
                                    String.Format("Unset the ontology property '{0}' as 'owl:TransitiveProperty', or remove the local cardinality constraint '{1}' applied on its inverse ontology property '{2}'.", inverseProps.TaxonomyObject, cardRestr, restrProp)
                                ));
                            }
                        }
                    }
                }
            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'LocalCardinalityConstraint': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

        #region Rule:Deprecation
        /// <summary>
        /// Validation rule checking for usage of deprecated classes and properties
        /// </summary>
        internal static RDFOntologyValidatorReport Deprecation(RDFOntology ontology)
        {
            RDFSemanticsEvents.RaiseSemanticsInfo("Launching execution of validation rule 'Deprecation'...");

            #region Class
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            foreach (var deprCls in ontology.Data.Relations.ClassType.Where(c => c.TaxonomyObject.IsDeprecatedClass()))
            {
                report.AddEvidence(new RDFOntologyValidatorEvidence(
                    RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                    "Deprecation",
                    String.Format("Ontology fact '{0}' has a classtype '{1}', which is deprecated (may be removed in a future ontology version!).", deprCls.TaxonomySubject, deprCls.TaxonomyObject),
                    String.Format("Update the classtype of ontology fact '{0}' to a non-deprecated class definition.", deprCls.TaxonomySubject)
                ));
            }
            #endregion

            #region Property
            foreach (var deprProp in ontology.Data.Relations.Assertions.Where(p => p.TaxonomyPredicate.IsDeprecatedProperty()))
            {
                report.AddEvidence(new RDFOntologyValidatorEvidence(
                    RDFSemanticsEnums.RDFOntologyValidatorEvidenceCategory.Warning,
                    "Deprecation",
                    String.Format("Ontology fact '{0}' has an assertion using ontology property '{1}', which is deprecated (may be removed in a future ontology version!).", deprProp.TaxonomySubject, deprProp.TaxonomyPredicate),
                    String.Format("Update the assertion of ontology fact '{0}' to a non-deprecated property definition.", deprProp.TaxonomySubject)
                ));
            }
            #endregion

            RDFSemanticsEvents.RaiseSemanticsInfo("Completed execution of validation rule 'Deprecation': found " + report.EvidencesCount + " evidences.");
            return report;
        }
        #endregion

    }

}