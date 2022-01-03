/*
   Copyright 2012-2022 Marco De Salvo

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyClassModel represents the class-oriented model component (T-BOX) of an ontology.
    /// </summary>
    public class RDFOntologyClassModel : IEnumerable<RDFOntologyClass>
    {
        #region Properties
        /// <summary>
        /// Count of the classes composing the class model
        /// </summary>
        public long ClassesCount
            => this.Classes.Count;

        /// <summary>
        /// Count of the deprecated classes composing the class model
        /// </summary>
        public long DeprecatedClassesCount
            => this.Classes.Count(c => c.Value.IsDeprecatedClass());

        /// <summary>
        /// Count of the restrictions classes composing the class model
        /// </summary>
        public long RestrictionsCount
            => this.Classes.Count(c => c.Value.IsRestrictionClass());

        /// <summary>
        /// Count of the enumerate classes composing the class model
        /// </summary>
        public long EnumeratesCount
            => this.Classes.Count(c => c.Value.IsEnumerateClass());

        /// <summary>
        /// Count of the datarange classes composing the class model
        /// </summary>
        public long DataRangesCount
            => this.Classes.Count(c => c.Value.IsDataRangeClass());

        /// <summary>
        /// Count of the composite classes composing the class model
        /// </summary>
        public long CompositesCount
            => this.Classes.Count(c => c.Value.IsCompositeClass());

        /// <summary>
        /// Gets the enumerator on the class model's classes for iteration
        /// </summary>
        public IEnumerator<RDFOntologyClass> ClassesEnumerator
            => this.Classes.Values.GetEnumerator();

        /// <summary>
        /// Gets the enumerator on the class model's deprecated classes for iteration
        /// </summary>
        public IEnumerator<RDFOntologyClass> DeprecatedClassesEnumerator
            => this.Classes.Values.Where(c => c.IsDeprecatedClass())
                                  .GetEnumerator();

        /// <summary>
        /// Gets the enumerator on the class model's restriction classes for iteration
        /// </summary>
        public IEnumerator<RDFOntologyRestriction> RestrictionsEnumerator
            => this.Classes.Values.Where(c => c.IsRestrictionClass())
                                  .OfType<RDFOntologyRestriction>()
                                  .GetEnumerator();

        /// <summary>
        /// Gets the enumerator on the class model's enumerate classes for iteration
        /// </summary>
        public IEnumerator<RDFOntologyEnumerateClass> EnumeratesEnumerator
            => this.Classes.Values.Where(c => c.IsEnumerateClass())
                                  .OfType<RDFOntologyEnumerateClass>()
                                  .GetEnumerator();

        /// <summary>
        /// Gets the enumerator on the class model's datarange classes for iteration
        /// </summary>
        public IEnumerator<RDFOntologyDataRangeClass> DataRangesEnumerator
            => this.Classes.Values.Where(c => c.IsDataRangeClass())
                                  .OfType<RDFOntologyDataRangeClass>()
                                  .GetEnumerator();

        /// <summary>
        /// Gets the enumerator on the class model's composite classes for iteration
        /// </summary>
        public IEnumerator<RDFOntologyClass> CompositesEnumerator
            => this.Classes.Values.Where(c => c.IsCompositeClass())
                                  .GetEnumerator();

        /// <summary>
        /// Annotations describing classes of the ontology class model
        /// </summary>
        public RDFOntologyAnnotations Annotations { get; internal set; }

        /// <summary>
        /// Relations describing classes of the ontology class model
        /// </summary>
        public RDFOntologyClassModelMetadata Relations { get; internal set; }

        /// <summary>
        /// Dictionary of classes composing the ontology class model
        /// </summary>
        internal Dictionary<long, RDFOntologyClass> Classes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty class model
        /// </summary>
        public RDFOntologyClassModel()
        {
            this.Classes = new Dictionary<long, RDFOntologyClass>();
            this.Annotations = new RDFOntologyAnnotations();
            this.Relations = new RDFOntologyClassModelMetadata();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the class model's classes
        /// </summary>
        IEnumerator<RDFOntologyClass> IEnumerable<RDFOntologyClass>.GetEnumerator() => this.ClassesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the ontology class model's classes
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => this.ClassesEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given class to the ontology class model
        /// </summary>
        public RDFOntologyClassModel AddClass(RDFOntologyClass ontologyClass)
        {
            if (ontologyClass != null)
            {
                if (!this.Classes.ContainsKey(ontologyClass.PatternMemberID))
                    this.Classes.Add(ontologyClass.PatternMemberID, ontologyClass);
            }
            return this;
        }

        /// <summary>
        /// Adds the given restriction class to the ontology class model
        /// </summary>
        public RDFOntologyClassModel AddRestriction(RDFOntologyRestriction ontologyRestriction)
            => this.AddClass(ontologyRestriction);

        /// <summary>
        /// Adds the given standard annotation to the given ontology class
        /// </summary>
        public RDFOntologyClassModel AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation, RDFOntologyClass ontologyClass, RDFOntologyResource annotationValue)
        {
            if (ontologyClass != null && annotationValue != null)
            {
                switch (standardAnnotation)
                {

                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.VersionInfo.AddEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology class with owl:versionInfo value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //owl:versionIRI
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology class with owl:versionIRI because it is reserved for ontologies");
                        break;

                    //rdfs:comment
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.Comment.AddEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology class with rdfs:comment value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //rdfs:label
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.Label.AddEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology class with rdfs:label value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //rdfs:seeAlso
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso:
                        this.Annotations.SeeAlso.AddEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:isDefinedBy
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy:
                        this.Annotations.IsDefinedBy.AddEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:priorVersion
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology class with owl:priorVersion because it is reserved for ontologies");
                        break;

                    //owl:imports
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology class with owl:imports because it is reserved for ontologies");
                        break;

                    //owl:backwardCompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology class with owl:backwardCompatibleWith because it is reserved for ontologies");
                        break;

                    //owl:incompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology class with owl:incompatibleWith because it is reserved for ontologies");
                        break;

                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given custom annotation to the given ontology class
        /// </summary>
        public RDFOntologyClassModel AddCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty, RDFOntologyClass ontologyClass, RDFOntologyResource annotationValue)
        {
            if (ontologyAnnotationProperty != null && ontologyClass != null && annotationValue != null)
            {
                //standard annotation
                if (RDFSemanticsUtilities.StandardAnnotationProperties.Contains(ontologyAnnotationProperty.PatternMemberID))
                {
                    //owl:versionInfo
                    if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyClass, annotationValue);

                    //owl:versionIRI
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyClass, annotationValue);

                    //rdfs:comment
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyClass, annotationValue);

                    //rdfs:label
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyClass, annotationValue);

                    //rdfs:seeAlso
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyClass, annotationValue);

                    //rdfs:isDefinedBy
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyClass, annotationValue);

                    //owl:imports
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyClass, annotationValue);

                    //owl:backwardCompatibleWith
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyClass, annotationValue);

                    //owl:incompatibleWith
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyClass, annotationValue);

                    //owl:priorVersion
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyClass, annotationValue);
                }

                //custom annotation
                else
                    this.Annotations.CustomAnnotations.AddEntry(new RDFOntologyTaxonomyEntry(ontologyClass, ontologyAnnotationProperty, annotationValue));
            }
            return this;
        }

        /// <summary>
        /// Adds the "childClass -> rdfs:subClassOf -> motherClass" relation to the class model (and links the given axiom annotation if provided)
        /// </summary>
        public RDFOntologyClassModel AddSubClassOfRelation(RDFOntologyClass childClass, RDFOntologyClass motherClass, RDFOntologyAxiomAnnotation axiomAnnotation = null)
        {
            if (childClass != null && motherClass != null && !childClass.Equals(motherClass))
            {
                //Enforce preliminary checks on usage of BASE classes
                if (!RDFOntologyChecker.CheckReservedClass(childClass) && !RDFOntologyChecker.CheckReservedClass(motherClass))
                {
                    //Enforce taxonomy checks before adding the subClassOf relation
                    if (RDFOntologyChecker.CheckSubClassOfCompatibility(this, childClass, motherClass))
                    {
                        RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(childClass, RDFVocabulary.RDFS.SUB_CLASS_OF.ToRDFOntologyObjectProperty(), motherClass);
                        this.Relations.SubClassOf.AddEntry(taxonomyEntry);

                        //Link owl:Axiom annotation
                        this.AddAxiomAnnotation(taxonomyEntry, axiomAnnotation, nameof(RDFOntologyClassModelMetadata.SubClassOf));
                    }
                    else
                    {
                        //Raise warning event to inform the user: SubClassOf relation cannot be added to the class model because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SubClassOf relation between child class '{0}' and mother class '{1}' cannot be added to the class model because it violates the taxonomy consistency.", childClass, motherClass));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: SubClassOf relation cannot be added to the class model because usage of BASE reserved classes compromises the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SubClassOf relation between child class '{0}' and mother class '{1}' cannot be added to the class model because usage of BASE reserved classes compromises the taxonomy consistency.", childClass, motherClass));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "aClass -> owl:equivalentClass -> bClass" relation to the class model (and links the given axiom annotation if provided)
        /// </summary>
        public RDFOntologyClassModel AddEquivalentClassRelation(RDFOntologyClass aClass, RDFOntologyClass bClass, RDFOntologyAxiomAnnotation axiomAnnotation = null)
        {
            if (aClass != null && bClass != null && !aClass.Equals(bClass))
            {
                //Enforce preliminary checks on usage of BASE classes
                if (!RDFOntologyChecker.CheckReservedClass(aClass) && !RDFOntologyChecker.CheckReservedClass(bClass))
                {
                    //Enforce taxonomy checks before adding the equivalentClass relation
                    if (RDFOntologyChecker.CheckEquivalentClassCompatibility(this, aClass, bClass))
                    {
                        RDFOntologyTaxonomyEntry equivclassLeft = new RDFOntologyTaxonomyEntry(aClass, RDFVocabulary.OWL.EQUIVALENT_CLASS.ToRDFOntologyObjectProperty(), bClass);
                        this.Relations.EquivalentClass.AddEntry(equivclassLeft);
                        RDFOntologyTaxonomyEntry equivclassRight = new RDFOntologyTaxonomyEntry(bClass, RDFVocabulary.OWL.EQUIVALENT_CLASS.ToRDFOntologyObjectProperty(), aClass).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API);
                        this.Relations.EquivalentClass.AddEntry(equivclassRight);

                        //Link owl:Axiom annotation
                        this.AddAxiomAnnotation(equivclassLeft, axiomAnnotation, nameof(RDFOntologyClassModelMetadata.EquivalentClass));
                    }
                    else
                    {
                        //Raise warning event to inform the user: EquivalentClass relation cannot be added to the class model because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EquivalentClass relation between class '{0}' and class '{1}' cannot be added to the class model because it violates the taxonomy consistency.", aClass, bClass));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: EquivalentClass relation cannot be added to the class model because usage of BASE reserved classes compromises the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("EquivalentClass relation between class '{0}' and class '{1}' cannot be added to the class model because usage of BASE reserved classes compromises the taxonomy consistency.", aClass, bClass));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "aClass -> owl:disjointWith -> bClass" relation to the class model (and links the given axiom annotation if provided)
        /// </summary>
        public RDFOntologyClassModel AddDisjointWithRelation(RDFOntologyClass aClass, RDFOntologyClass bClass, RDFOntologyAxiomAnnotation axiomAnnotation = null)
        {
            if (aClass != null && bClass != null && !aClass.Equals(bClass))
            {
                //Enforce preliminary checks on usage of BASE classes
                if (!RDFOntologyChecker.CheckReservedClass(aClass) && !RDFOntologyChecker.CheckReservedClass(bClass))
                {
                    //Enforce taxonomy checks before adding the disjointWith relation
                    if (RDFOntologyChecker.CheckDisjointWithCompatibility(this, aClass, bClass))
                    {
                        RDFOntologyTaxonomyEntry disjwithLeft = new RDFOntologyTaxonomyEntry(aClass, RDFVocabulary.OWL.DISJOINT_WITH.ToRDFOntologyObjectProperty(), bClass);
                        this.Relations.DisjointWith.AddEntry(disjwithLeft);
                        RDFOntologyTaxonomyEntry disjwithRight = new RDFOntologyTaxonomyEntry(bClass, RDFVocabulary.OWL.DISJOINT_WITH.ToRDFOntologyObjectProperty(), aClass).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API);
                        this.Relations.DisjointWith.AddEntry(disjwithRight);

                        //Link owl:Axiom annotation
                        this.AddAxiomAnnotation(disjwithLeft, axiomAnnotation, nameof(RDFOntologyClassModelMetadata.DisjointWith));
                    }
                    else
                    {
                        //Raise warning event to inform the user: DisjointWith relation cannot be added to the class model because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("DisjointWith relation between class '{0}' and class '{1}' cannot be added to the class model because it violates the taxonomy consistency.", aClass, bClass));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: DisjointWith relation cannot be added to the class model because usage of BASE reserved classes compromises the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("DisjointWith relation between class '{0}' and class '{1}' cannot be added to the class model because usage of BASE reserved classes compromises the taxonomy consistency.", aClass, bClass));
                }
            }
            return this;
        }

        /// <summary>
        /// Foreach of the given facts, adds the "ontologyEnumerateClass -> owl:oneOf -> ontologyFact" relation to the class model
        /// </summary>
        public RDFOntologyClassModel AddOneOfRelation(RDFOntologyEnumerateClass ontologyEnumerateClass, List<RDFOntologyFact> ontologyFacts)
        {
            if (ontologyEnumerateClass != null && ontologyFacts != null)
                ontologyFacts.ForEach(f => this.Relations.OneOf.AddEntry(new RDFOntologyTaxonomyEntry(ontologyEnumerateClass, RDFVocabulary.OWL.ONE_OF.ToRDFOntologyObjectProperty(), f)));
            return this;
        }

        /// <summary>
        /// Foreach of the given literals, adds the "ontologyDataRangeClass -> owl:oneOf -> ontologyLiteral" relation to the class model
        /// </summary>
        public RDFOntologyClassModel AddOneOfRelation(RDFOntologyDataRangeClass ontologyDataRangeClass, List<RDFOntologyLiteral> ontologyLiterals)
        {
            if (ontologyDataRangeClass != null && ontologyLiterals != null)
                ontologyLiterals.ForEach(l => this.Relations.OneOf.AddEntry(new RDFOntologyTaxonomyEntry(ontologyDataRangeClass, RDFVocabulary.OWL.ONE_OF.ToRDFOntologyDatatypeProperty(), l)));
            return this;
        }

        /// <summary>
        /// Foreach of the given classes, adds the "ontologyIntersectionClass -> owl:intersectionOf -> ontologyClass" relation to the class model
        /// </summary>
        public RDFOntologyClassModel AddIntersectionOfRelation(RDFOntologyIntersectionClass ontologyIntersectionClass, List<RDFOntologyClass> ontologyClasses)
        {
            if (ontologyIntersectionClass != null && ontologyClasses != null && !ontologyClasses.Any(c => c.Equals(ontologyIntersectionClass)))
                ontologyClasses.ForEach(c => this.Relations.IntersectionOf.AddEntry(new RDFOntologyTaxonomyEntry(ontologyIntersectionClass, RDFVocabulary.OWL.INTERSECTION_OF.ToRDFOntologyObjectProperty(), c)));
            return this;
        }

        /// <summary>
        /// Foreach of the given classes, adds the "ontologyUnionClass -> owl:unionOf -> ontologyClass" relation to the class model
        /// </summary>
        public RDFOntologyClassModel AddUnionOfRelation(RDFOntologyUnionClass ontologyUnionClass, List<RDFOntologyClass> ontologyClasses)
        {
            if (ontologyUnionClass != null && ontologyClasses != null && !ontologyClasses.Any(c => c.Equals(ontologyUnionClass)))
                ontologyClasses.ForEach(c => this.Relations.UnionOf.AddEntry(new RDFOntologyTaxonomyEntry(ontologyUnionClass, RDFVocabulary.OWL.UNION_OF.ToRDFOntologyObjectProperty(), c)));
            return this;
        }

        /// <summary>
        /// Foreach of the given classes, adds the "ontologyUnionClass -> owl:unionOf -> ontologyClass" and the<br/>
        /// "ontologyClassA -> owl:disjointWith -> ontologyClassB" relations to the class model [OWL2]
        /// </summary>
        public RDFOntologyClassModel AddDisjointUnionRelation(RDFOntologyUnionClass ontologyUnionClass, List<RDFOntologyClass> ontologyClasses)
        {
            //Union
            this.AddUnionOfRelation(ontologyUnionClass, ontologyClasses);

            //Disjointness
            ontologyClasses?.ForEach(outerClass =>
                ontologyClasses?.ForEach(innerClass => this.AddDisjointWithRelation(outerClass, innerClass)));

            return this;
        }

        /// <summary>
        /// Foreach of the given classes, adds the "ontologyClassA -> owl:disjointWith -> ontologyClassB" relations to the class model [OWL2]
        /// </summary>
        public RDFOntologyClassModel AddAllDisjointClassesRelation(List<RDFOntologyClass> ontologyClasses)
        {
            ontologyClasses?.ForEach(outerClass =>
                ontologyClasses?.ForEach(innerClass => this.AddDisjointWithRelation(outerClass, innerClass)));
            return this;
        }

        /// <summary>
        /// For each of the given properties, adds the "ontologyClass -> owl:hasKey -> keyProperty" relation to the class model [OWL2]
        /// </summary>
        public RDFOntologyClassModel AddHasKeyRelation(RDFOntologyClass ontologyClass, List<RDFOntologyProperty> keyProperties)
        {
            if (ontologyClass != null)
                keyProperties?.ForEach(kp =>
                {
                    if (kp != null)
                        this.Relations.HasKey.AddEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.HAS_KEY.ToRDFOntologyObjectProperty(), kp));
                });
            return this;
        }

        /// <summary>
        /// Adds the given owl:Axiom annotation to the given taxonomy entry
        /// </summary>
        internal RDFOntologyClassModel AddAxiomAnnotation(RDFOntologyTaxonomyEntry taxonomyEntry, RDFOntologyAxiomAnnotation axiomAnnotation, string targetTaxonomyName)
        {
            #region DetectTargetTaxonomy
            RDFOntologyTaxonomy DetectTargetTaxonomy()
            {
                RDFOntologyTaxonomy targetTaxonomy = default;
                switch (targetTaxonomyName)
                {
                    case nameof(RDFOntologyClassModelMetadata.SubClassOf):
                        targetTaxonomy = this.Relations.SubClassOf;
                        break;
                    case nameof(RDFOntologyClassModelMetadata.EquivalentClass):
                        targetTaxonomy = this.Relations.EquivalentClass;
                        break;
                    case nameof(RDFOntologyClassModelMetadata.DisjointWith):
                        targetTaxonomy = this.Relations.DisjointWith;
                        break;
                }
                return targetTaxonomy;
            }
            #endregion

            RDFOntologyTaxonomy taxonomy = DetectTargetTaxonomy();
            if (axiomAnnotation != null && taxonomy != null && taxonomy.ContainsEntry(taxonomyEntry))
                this.Annotations.AxiomAnnotations.AddEntry(new RDFOntologyTaxonomyEntry(this.GetTaxonomyEntryRepresentative(taxonomyEntry), axiomAnnotation.Property, axiomAnnotation.Value));
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given class from the ontology class model
        /// </summary>
        public RDFOntologyClassModel RemoveClass(RDFOntologyClass ontologyClass)
        {
            if (ontologyClass != null)
            {
                if (this.Classes.ContainsKey(ontologyClass.PatternMemberID))
                    this.Classes.Remove(ontologyClass.PatternMemberID);
            }
            return this;
        }

        /// <summary>
        /// Removes the given restriction class from the ontology class model
        /// </summary>
        public RDFOntologyClassModel RemoveRestriction(RDFOntologyRestriction ontologyRestriction)
            => this.RemoveClass(ontologyRestriction);

        /// <summary>
        /// Removes the given standard annotation from the given ontology class
        /// </summary>
        public RDFOntologyClassModel RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation, RDFOntologyClass ontologyClass, RDFOntologyResource annotationValue)
        {
            if (ontologyClass != null && annotationValue != null)
            {
                switch (standardAnnotation)
                {

                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                        this.Annotations.VersionInfo.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:versionIRI
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                        this.Annotations.VersionIRI.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:comment
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                        this.Annotations.Comment.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:label
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                        this.Annotations.Label.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:seeAlso
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso:
                        this.Annotations.SeeAlso.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:isDefinedBy
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy:
                        this.Annotations.IsDefinedBy.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:priorVersion
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion:
                        this.Annotations.PriorVersion.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:imports
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports:
                        this.Annotations.Imports.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:backwardCompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith:
                        this.Annotations.BackwardCompatibleWith.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:incompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith:
                        this.Annotations.IncompatibleWith.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given custom annotation from the given ontology class
        /// </summary>
        public RDFOntologyClassModel RemoveCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty, RDFOntologyClass ontologyClass, RDFOntologyResource annotationValue)
        {
            if (ontologyAnnotationProperty != null && ontologyClass != null && annotationValue != null)
            {

                //owl:versionInfo
                if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyClass, annotationValue);
                }

                //owl:versionIRI
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyClass, annotationValue);
                }

                //rdfs:comment
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyClass, annotationValue);
                }

                //rdfs:label
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyClass, annotationValue);
                }

                //rdfs:seeAlso
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyClass, annotationValue);
                }

                //rdfs:isDefinedBy
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyClass, annotationValue);
                }

                //owl:imports
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyClass, annotationValue);
                }

                //owl:backwardCompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyClass, annotationValue);
                }

                //owl:incompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyClass, annotationValue);
                }

                //owl:priorVersion
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyClass, annotationValue);
                }

                //custom annotation
                else
                {
                    this.Annotations.CustomAnnotations.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, ontologyAnnotationProperty, annotationValue));
                }

            }
            return this;
        }

        /// <summary>
        /// Removes the "childClass -> rdfs:subClassOf -> motherClass" relation from the class model
        /// </summary>
        public RDFOntologyClassModel RemoveSubClassOfRelation(RDFOntologyClass childClass, RDFOntologyClass motherClass)
        {
            if (childClass != null && motherClass != null)
            {
                RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(childClass, RDFVocabulary.RDFS.SUB_CLASS_OF.ToRDFOntologyObjectProperty(), motherClass);
                this.Relations.SubClassOf.RemoveEntry(taxonomyEntry);

                //Unlink owl:Axiom annotation
                this.RemoveAxiomAnnotation(taxonomyEntry);
            }
            return this;
        }

        /// <summary>
        /// Removes the "aClass -> owl:equivalentClass -> bClass" relation from the class model
        /// </summary>
        public RDFOntologyClassModel RemoveEquivalentClassRelation(RDFOntologyClass aClass, RDFOntologyClass bClass)
        {
            if (aClass != null && bClass != null)
            {
                RDFOntologyTaxonomyEntry equivclassLeft = new RDFOntologyTaxonomyEntry(aClass, RDFVocabulary.OWL.EQUIVALENT_CLASS.ToRDFOntologyObjectProperty(), bClass);
                this.Relations.EquivalentClass.RemoveEntry(equivclassLeft);
                RDFOntologyTaxonomyEntry equivclassRight = new RDFOntologyTaxonomyEntry(bClass, RDFVocabulary.OWL.EQUIVALENT_CLASS.ToRDFOntologyObjectProperty(), aClass);
                this.Relations.EquivalentClass.RemoveEntry(equivclassRight);

                //Unlink owl:Axiom annotations
                this.RemoveAxiomAnnotation(equivclassLeft);
                this.RemoveAxiomAnnotation(equivclassRight);
            }
            return this;
        }

        /// <summary>
        /// Removes the "aClass -> owl:disjointWith -> bClass" relation from the class model
        /// </summary>
        public RDFOntologyClassModel RemoveDisjointWithRelation(RDFOntologyClass aClass, RDFOntologyClass bClass)
        {
            if (aClass != null && bClass != null)
            {
                RDFOntologyTaxonomyEntry disjwithLeft = new RDFOntologyTaxonomyEntry(aClass, RDFVocabulary.OWL.DISJOINT_WITH.ToRDFOntologyObjectProperty(), bClass);
                this.Relations.DisjointWith.RemoveEntry(disjwithLeft);
                RDFOntologyTaxonomyEntry disjwithRight = new RDFOntologyTaxonomyEntry(bClass, RDFVocabulary.OWL.DISJOINT_WITH.ToRDFOntologyObjectProperty(), aClass);
                this.Relations.DisjointWith.RemoveEntry(disjwithRight);

                //Unlink owl:Axiom annotations
                this.RemoveAxiomAnnotation(disjwithLeft);
                this.RemoveAxiomAnnotation(disjwithRight);
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyEnumerateClass -> owl:oneOf -> ontologyFact" relation from the class model
        /// </summary>
        public RDFOntologyClassModel RemoveOneOfRelation(RDFOntologyEnumerateClass ontologyEnumerateClass, RDFOntologyFact ontologyFact)
        {
            if (ontologyEnumerateClass != null && ontologyFact != null)
                this.Relations.OneOf.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyEnumerateClass, RDFVocabulary.OWL.ONE_OF.ToRDFOntologyObjectProperty(), ontologyFact));
            return this;
        }

        /// <summary>
        /// Removes the "ontologyDataRangeClass -> owl:oneOf -> ontologyLiteral" relation from the class model
        /// </summary>
        public RDFOntologyClassModel RemoveOneOfRelation(RDFOntologyDataRangeClass ontologyDataRangeClass, RDFOntologyLiteral ontologyLiteral)
        {
            if (ontologyDataRangeClass != null && ontologyLiteral != null)
                this.Relations.OneOf.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyDataRangeClass, RDFVocabulary.OWL.ONE_OF.ToRDFOntologyDatatypeProperty(), ontologyLiteral));
            return this;
        }

        /// <summary>
        /// Removes the "ontologyIntersectionClass -> owl:intersectionOf -> ontologyClass" relation from the class model
        /// </summary>
        public RDFOntologyClassModel RemoveIntersectionOfRelation(RDFOntologyIntersectionClass ontologyIntersectionClass, RDFOntologyClass ontologyClass)
        {
            if (ontologyIntersectionClass != null && ontologyClass != null)
                this.Relations.IntersectionOf.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyIntersectionClass, RDFVocabulary.OWL.INTERSECTION_OF.ToRDFOntologyObjectProperty(), ontologyClass));
            return this;
        }

        /// <summary>
        /// Removes the "ontologyUnionClass -> owl:unionOf -> ontologyClass" relation from the class model
        /// </summary>
        public RDFOntologyClassModel RemoveUnionOfRelation(RDFOntologyUnionClass ontologyUnionClass, RDFOntologyClass ontologyClass)
        {
            if (ontologyUnionClass != null && ontologyClass != null)
                this.Relations.UnionOf.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyUnionClass, RDFVocabulary.OWL.UNION_OF.ToRDFOntologyObjectProperty(), ontologyClass));
            return this;
        }

        /// <summary>
        /// Foreach of the given classes, removes the "ontologyUnionClass -> owl:unionOf -> ontologyClass" and the<br/>
        /// "ontologyClassA -> owl:disjointWith -> ontologyClassB" relations from the class model [OWL2]
        /// </summary>
        public RDFOntologyClassModel RemoveDisjointUnionRelation(RDFOntologyUnionClass ontologyUnionClass, List<RDFOntologyClass> ontologyClasses)
        {
            ontologyClasses?.ForEach(outerClass =>
            {
                ontologyClasses?.ForEach(innerClass =>
                {
                    this.RemoveUnionOfRelation(ontologyUnionClass, innerClass);
                    this.RemoveDisjointWithRelation(outerClass, innerClass);
                });
            });
            return this;
        }

        /// <summary>
        /// Foreach of the given classes, removes the "ontologyClassA -> owl:disjointWith -> ontologyClassB" relations from the class model [OWL2]
        /// </summary>
        public RDFOntologyClassModel RemoveAllDisjointClassesRelation(List<RDFOntologyClass> ontologyClasses)
        {
            ontologyClasses?.ForEach(outerClass =>
            {
                ontologyClasses?.ForEach(innerClass =>
                {
                    this.RemoveDisjointWithRelation(outerClass, innerClass);
                });
            });
            return this;
        }

        /// <summary>
        /// Removes the "ontologyClass -> owl:hasKey -> keyProperty" relation from the class model [OWL2]
        /// </summary>
        public RDFOntologyClassModel RemoveHasKeyRelation(RDFOntologyClass ontologyClass, RDFOntologyProperty keyProperty)
        {
            if (ontologyClass != null && keyProperty != null)
                this.Relations.HasKey.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyClass, RDFVocabulary.OWL.HAS_KEY.ToRDFOntologyObjectProperty(), keyProperty));
            return this;
        }

        /// <summary>
        /// Removes the given owl:Axiom annotation [OWL2]
        /// </summary>
        internal RDFOntologyClassModel RemoveAxiomAnnotation(RDFOntologyTaxonomyEntry taxonomyEntry)
        {
            foreach (RDFOntologyTaxonomyEntry axnTaxonomyEntry in this.Annotations.AxiomAnnotations.SelectEntriesBySubject(this.GetTaxonomyEntryRepresentative(taxonomyEntry)))
                this.Annotations.AxiomAnnotations.RemoveEntry(axnTaxonomyEntry);
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the ontology class represented by the given identifier
        /// </summary>
        public RDFOntologyClass SelectClass(long ontClassID)
            => this.Classes.ContainsKey(ontClassID) ? this.Classes[ontClassID] : null;

        /// <summary>
        /// Selects the ontology class represented by the given string
        /// </summary>
        public RDFOntologyClass SelectClass(string ontClass)
            => ontClass != null ? SelectClass(RDFModelUtilities.CreateHash(ontClass)) : null;

        /// <summary>
        /// Gets the representative of the given taxonomy entry
        /// </summary>
        internal RDFOntologyFact GetTaxonomyEntryRepresentative(RDFOntologyTaxonomyEntry taxonomyEntry)
            => new RDFOntologyFact(new RDFResource($"bnode:semref{taxonomyEntry.TaxonomyEntryID}"));
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection class model from this class model and a given one
        /// </summary>
        public RDFOntologyClassModel IntersectWith(RDFOntologyClassModel classModel)
        {
            RDFOntologyClassModel result = new RDFOntologyClassModel();
            if (classModel != null)
            {
                //Add intersection classes
                foreach (RDFOntologyClass c in this)
                    if (classModel.Classes.ContainsKey(c.PatternMemberID))
                        result.AddClass(c);

                //Add intersection relations
                result.Relations.SubClassOf = this.Relations.SubClassOf.IntersectWith(classModel.Relations.SubClassOf);
                result.Relations.EquivalentClass = this.Relations.EquivalentClass.IntersectWith(classModel.Relations.EquivalentClass);
                result.Relations.DisjointWith = this.Relations.DisjointWith.IntersectWith(classModel.Relations.DisjointWith);
                result.Relations.OneOf = this.Relations.OneOf.IntersectWith(classModel.Relations.OneOf);
                result.Relations.IntersectionOf = this.Relations.IntersectionOf.IntersectWith(classModel.Relations.IntersectionOf);
                result.Relations.UnionOf = this.Relations.UnionOf.IntersectWith(classModel.Relations.UnionOf);
                result.Relations.HasKey = this.Relations.HasKey.IntersectWith(classModel.Relations.HasKey); //OWL2

                //Add intersection annotations
                result.Annotations.VersionInfo = this.Annotations.VersionInfo.IntersectWith(classModel.Annotations.VersionInfo);
                result.Annotations.Comment = this.Annotations.Comment.IntersectWith(classModel.Annotations.Comment);
                result.Annotations.Label = this.Annotations.Label.IntersectWith(classModel.Annotations.Label);
                result.Annotations.SeeAlso = this.Annotations.SeeAlso.IntersectWith(classModel.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = this.Annotations.IsDefinedBy.IntersectWith(classModel.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.IntersectWith(classModel.Annotations.CustomAnnotations);
                result.Annotations.AxiomAnnotations = this.Annotations.AxiomAnnotations.IntersectWith(classModel.Annotations.AxiomAnnotations); //OWL2
            }
            return result;
        }

        /// <summary>
        /// Builds a new union class model from this class model and a given one
        /// </summary>
        public RDFOntologyClassModel UnionWith(RDFOntologyClassModel classModel)
        {
            RDFOntologyClassModel result = new RDFOntologyClassModel();

            //Add classes from this class model
            foreach (RDFOntologyClass c in this)
                result.AddClass(c);
            
            //Add relations from this class model
            result.Relations.SubClassOf = result.Relations.SubClassOf.UnionWith(this.Relations.SubClassOf);
            result.Relations.EquivalentClass = result.Relations.EquivalentClass.UnionWith(this.Relations.EquivalentClass);
            result.Relations.DisjointWith = result.Relations.DisjointWith.UnionWith(this.Relations.DisjointWith);
            result.Relations.OneOf = result.Relations.OneOf.UnionWith(this.Relations.OneOf);
            result.Relations.IntersectionOf = result.Relations.IntersectionOf.UnionWith(this.Relations.IntersectionOf);
            result.Relations.UnionOf = result.Relations.UnionOf.UnionWith(this.Relations.UnionOf);
            result.Relations.HasKey = result.Relations.HasKey.UnionWith(this.Relations.HasKey); //OWL2

            //Add annotations from this class model
            result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
            result.Annotations.Comment = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
            result.Annotations.Label = result.Annotations.Label.UnionWith(this.Annotations.Label);
            result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
            result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
            result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);
            result.Annotations.AxiomAnnotations = result.Annotations.AxiomAnnotations.UnionWith(this.Annotations.AxiomAnnotations); //OWL2

            //Manage the given class model
            if (classModel != null)
            {
                //Add classes from the given class model
                foreach (RDFOntologyClass c in classModel)
                    result.AddClass(c);
                
                //Add relations from the given class model
                result.Relations.SubClassOf = result.Relations.SubClassOf.UnionWith(classModel.Relations.SubClassOf);
                result.Relations.EquivalentClass = result.Relations.EquivalentClass.UnionWith(classModel.Relations.EquivalentClass);
                result.Relations.DisjointWith = result.Relations.DisjointWith.UnionWith(classModel.Relations.DisjointWith);
                result.Relations.OneOf = result.Relations.OneOf.UnionWith(classModel.Relations.OneOf);
                result.Relations.IntersectionOf = result.Relations.IntersectionOf.UnionWith(classModel.Relations.IntersectionOf);
                result.Relations.UnionOf = result.Relations.UnionOf.UnionWith(classModel.Relations.UnionOf);
                result.Relations.HasKey = result.Relations.HasKey.UnionWith(classModel.Relations.HasKey); //OWL2

                //Add annotations from the given class model
                result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(classModel.Annotations.VersionInfo);
                result.Annotations.Comment = result.Annotations.Comment.UnionWith(classModel.Annotations.Comment);
                result.Annotations.Label = result.Annotations.Label.UnionWith(classModel.Annotations.Label);
                result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(classModel.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(classModel.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(classModel.Annotations.CustomAnnotations);
                result.Annotations.AxiomAnnotations = result.Annotations.AxiomAnnotations.UnionWith(classModel.Annotations.AxiomAnnotations); //OWL2
            }
            return result;
        }

        /// <summary>
        /// Builds a new difference class model from this class model and a given one
        /// </summary>
        public RDFOntologyClassModel DifferenceWith(RDFOntologyClassModel classModel)
        {
            RDFOntologyClassModel result = new RDFOntologyClassModel();
            if (classModel != null)
            {
                //Add difference classes
                foreach (RDFOntologyClass c in this)
                    if (!classModel.Classes.ContainsKey(c.PatternMemberID))
                        result.AddClass(c);

                //Add difference relations
                result.Relations.SubClassOf = this.Relations.SubClassOf.DifferenceWith(classModel.Relations.SubClassOf);
                result.Relations.EquivalentClass = this.Relations.EquivalentClass.DifferenceWith(classModel.Relations.EquivalentClass);
                result.Relations.DisjointWith = this.Relations.DisjointWith.DifferenceWith(classModel.Relations.DisjointWith);
                result.Relations.OneOf = this.Relations.OneOf.DifferenceWith(classModel.Relations.OneOf);
                result.Relations.IntersectionOf = this.Relations.IntersectionOf.DifferenceWith(classModel.Relations.IntersectionOf);
                result.Relations.UnionOf = this.Relations.UnionOf.DifferenceWith(classModel.Relations.UnionOf);
                result.Relations.HasKey = this.Relations.HasKey.DifferenceWith(classModel.Relations.HasKey); //OWL2

                //Add difference annotations
                result.Annotations.VersionInfo = this.Annotations.VersionInfo.DifferenceWith(classModel.Annotations.VersionInfo);
                result.Annotations.Comment = this.Annotations.Comment.DifferenceWith(classModel.Annotations.Comment);
                result.Annotations.Label = this.Annotations.Label.DifferenceWith(classModel.Annotations.Label);
                result.Annotations.SeeAlso = this.Annotations.SeeAlso.DifferenceWith(classModel.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = this.Annotations.IsDefinedBy.DifferenceWith(classModel.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.DifferenceWith(classModel.Annotations.CustomAnnotations);
                result.Annotations.AxiomAnnotations = this.Annotations.AxiomAnnotations.DifferenceWith(classModel.Annotations.AxiomAnnotations); //OWL2
            }
            else
            {
                //Add classes from this class model
                foreach (RDFOntologyClass c in this)
                    result.AddClass(c);
                
                //Add relations from this class model
                result.Relations.SubClassOf = result.Relations.SubClassOf.UnionWith(this.Relations.SubClassOf);
                result.Relations.EquivalentClass = result.Relations.EquivalentClass.UnionWith(this.Relations.EquivalentClass);
                result.Relations.DisjointWith = result.Relations.DisjointWith.UnionWith(this.Relations.DisjointWith);
                result.Relations.OneOf = result.Relations.OneOf.UnionWith(this.Relations.OneOf);
                result.Relations.IntersectionOf = result.Relations.IntersectionOf.UnionWith(this.Relations.IntersectionOf);
                result.Relations.UnionOf = result.Relations.UnionOf.UnionWith(this.Relations.UnionOf);
                result.Relations.HasKey = result.Relations.HasKey.UnionWith(this.Relations.HasKey); //OWL2

                //Add annotations from this class model
                result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
                result.Annotations.Comment = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
                result.Annotations.Label = result.Annotations.Label.UnionWith(this.Annotations.Label);
                result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);
                result.Annotations.AxiomAnnotations = result.Annotations.AxiomAnnotations.UnionWith(this.Annotations.AxiomAnnotations); //OWL2
            }
            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this ontology class model, exporting inferences according to the selected behavior
        /// </summary>
        public RDFGraph ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            RDFGraph result = new RDFGraph();

            //Definitions
            foreach (RDFOntologyClass c in this.Where(c => !RDFOntologyChecker.CheckReservedClass(c)))
            {
                //Restriction
                if (c.IsRestrictionClass())
                {
                    result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.RESTRICTION));
                    result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.ON_PROPERTY, (RDFResource)((RDFOntologyRestriction)c).OnProperty.Value));
                    if (c is RDFOntologyAllValuesFromRestriction allValuesFromRestriction)
                        result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.ALL_VALUES_FROM, (RDFResource)allValuesFromRestriction.FromClass.Value));
                    else if (c is RDFOntologySomeValuesFromRestriction someValuesFromRestriction)
                        result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.SOME_VALUES_FROM, (RDFResource)someValuesFromRestriction.FromClass.Value));
                    else if (c is RDFOntologyHasSelfRestriction hasSelfRestriction)
                        result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.HAS_SELF, RDFTypedLiteral.True));
                    else if (c is RDFOntologyHasValueRestriction hasValueRestriction)
                    {
                        if (hasValueRestriction.RequiredValue.IsLiteral())
                            result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.HAS_VALUE, (RDFLiteral)hasValueRestriction.RequiredValue.Value));
                        else
                            result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.HAS_VALUE, (RDFResource)hasValueRestriction.RequiredValue.Value));
                    }
                    else if (c is RDFOntologyCardinalityRestriction cardinalityRestriction)
                    {
                        if (cardinalityRestriction.MinCardinality == cardinalityRestriction.MaxCardinality)
                            result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.CARDINALITY, new RDFTypedLiteral(cardinalityRestriction.MinCardinality.ToString(), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)));
                        else
                        {
                            if (cardinalityRestriction.MinCardinality > 0)
                                result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.MIN_CARDINALITY, new RDFTypedLiteral(cardinalityRestriction.MinCardinality.ToString(), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)));
                            if (cardinalityRestriction.MaxCardinality > 0)
                                result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.MAX_CARDINALITY, new RDFTypedLiteral(cardinalityRestriction.MaxCardinality.ToString(), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)));
                        }
                    }
                    else if (c is RDFOntologyQualifiedCardinalityRestriction qualifiedCardinalityRestriction)
                    {
                        if (qualifiedCardinalityRestriction.OnClass is RDFOntologyDataRangeClass)
                            result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.ON_DATARANGE, (RDFResource)qualifiedCardinalityRestriction.OnClass.Value));
                        else
                            result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.ON_CLASS, (RDFResource)qualifiedCardinalityRestriction.OnClass.Value));
                        if (qualifiedCardinalityRestriction.MinQualifiedCardinality == qualifiedCardinalityRestriction.MaxQualifiedCardinality)
                            result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.QUALIFIED_CARDINALITY, new RDFTypedLiteral(qualifiedCardinalityRestriction.MinQualifiedCardinality.ToString(), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)));
                        else
                        {
                            if (qualifiedCardinalityRestriction.MinQualifiedCardinality > 0)
                                result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.MIN_QUALIFIED_CARDINALITY, new RDFTypedLiteral(qualifiedCardinalityRestriction.MinQualifiedCardinality.ToString(), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)));
                            if (qualifiedCardinalityRestriction.MaxQualifiedCardinality > 0)
                                result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.MAX_QUALIFIED_CARDINALITY, new RDFTypedLiteral(qualifiedCardinalityRestriction.MaxQualifiedCardinality.ToString(), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)));
                        }
                    }
                }

                //Enumerate
                else if (c.IsEnumerateClass())
                {
                    result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
                    RDFCollection enumCollection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
                    enumCollection.ReificationSubject = new RDFResource(string.Concat("bnode:", c.PatternMemberID.ToString()));
                    if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.None
                            || infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyData)
                    {
                        foreach (RDFOntologyTaxonomyEntry enumMember in this.Relations.OneOf.SelectEntriesBySubject(c)
                                                                                            .Where(tax => tax.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
                            enumCollection.AddItem((RDFResource)enumMember.TaxonomyObject.Value);
                    }
                    else
                    {
                        foreach (RDFOntologyTaxonomyEntry enumMember in this.Relations.OneOf.SelectEntriesBySubject(c))
                            enumCollection.AddItem((RDFResource)enumMember.TaxonomyObject.Value);
                    }
                    result = result.UnionWith(enumCollection.ReifyCollection());
                    result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.ONE_OF, enumCollection.ReificationSubject));
                }

                //DataRange
                else if (c.IsDataRangeClass())
                {
                    result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DATA_RANGE));
                    RDFCollection drangeCollection = new RDFCollection(RDFModelEnums.RDFItemTypes.Literal);
                    drangeCollection.ReificationSubject = new RDFResource(string.Concat("bnode:", c.PatternMemberID.ToString()));
                    if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.None
                            || infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyData)
                    {
                        foreach (RDFOntologyTaxonomyEntry drangeMember in this.Relations.OneOf.SelectEntriesBySubject(c)
                                                                                              .Where(tax => tax.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
                            drangeCollection.AddItem((RDFLiteral)drangeMember.TaxonomyObject.Value);
                    }
                    else
                    {
                        foreach (RDFOntologyTaxonomyEntry drangeMember in this.Relations.OneOf.SelectEntriesBySubject(c))
                            drangeCollection.AddItem((RDFLiteral)drangeMember.TaxonomyObject.Value);
                    }
                    result = result.UnionWith(drangeCollection.ReifyCollection());
                    result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.ONE_OF, drangeCollection.ReificationSubject));
                }

                //Composite
                else if (c.IsCompositeClass())
                {
                    result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
                    if (c is RDFOntologyComplementClass)
                        result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.COMPLEMENT_OF, (RDFResource)((RDFOntologyComplementClass)c).ComplementOf.Value));
                    else if (c is RDFOntologyIntersectionClass)
                    {
                        RDFCollection intersectCollection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
                        intersectCollection.ReificationSubject = new RDFResource(string.Concat("bnode:", c.PatternMemberID.ToString()));
                        if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.None
                                || infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyData)
                        {
                            foreach (RDFOntologyTaxonomyEntry intersectMember in this.Relations.IntersectionOf.SelectEntriesBySubject(c)
                                                                                                              .Where(tax => tax.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
                                intersectCollection.AddItem((RDFResource)intersectMember.TaxonomyObject.Value);
                        }
                        else
                        {
                            foreach (RDFOntologyTaxonomyEntry intersectMember in this.Relations.IntersectionOf.SelectEntriesBySubject(c))
                                intersectCollection.AddItem((RDFResource)intersectMember.TaxonomyObject.Value);
                        }
                        result = result.UnionWith(intersectCollection.ReifyCollection());
                        result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.INTERSECTION_OF, intersectCollection.ReificationSubject));
                    }
                    else if (c is RDFOntologyUnionClass)
                    {
                        RDFCollection unionCollection = new RDFCollection(RDFModelEnums.RDFItemTypes.Resource);
                        unionCollection.ReificationSubject = new RDFResource(string.Concat("bnode:", c.PatternMemberID.ToString()));
                        if (infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.None
                                || infexpBehavior == RDFSemanticsEnums.RDFOntologyInferenceExportBehavior.OnlyData)
                        {
                            foreach (RDFOntologyTaxonomyEntry unionMember in this.Relations.UnionOf.SelectEntriesBySubject(c)
                                                                                                   .Where(tax => tax.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
                                unionCollection.AddItem((RDFResource)unionMember.TaxonomyObject.Value);
                        }
                        else
                        {
                            foreach (RDFOntologyTaxonomyEntry unionMember in this.Relations.UnionOf.SelectEntriesBySubject(c))
                                unionCollection.AddItem((RDFResource)unionMember.TaxonomyObject.Value);
                        }
                        result = result.UnionWith(unionCollection.ReifyCollection());
                        result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.OWL.UNION_OF, unionCollection.ReificationSubject));
                    }
                }

                //Class
                else
                {
                    result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.RDF.TYPE, (c.Nature == RDFSemanticsEnums.RDFOntologyClassNature.OWL ? RDFVocabulary.OWL.CLASS : RDFVocabulary.RDFS.CLASS)));
                    if (c.IsDeprecatedClass())
                        result.AddTriple(new RDFTriple((RDFResource)c.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_CLASS));
                }

            }

            //Relations
            result = result.UnionWith(this.Relations.SubClassOf.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.SubClassOf)))
                           .UnionWith(this.Relations.EquivalentClass.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.EquivalentClass)))
                           .UnionWith(this.Relations.DisjointWith.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.DisjointWith)))
                           .UnionWith(this.Relations.HasKey.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.HasKey))); //OWL2

            //Annotations
            result = result.UnionWith(this.Annotations.VersionInfo.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.VersionInfo)))
                           .UnionWith(this.Annotations.Comment.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.Comment)))
                           .UnionWith(this.Annotations.Label.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.Label)))
                           .UnionWith(this.Annotations.SeeAlso.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.SeeAlso)))
                           .UnionWith(this.Annotations.IsDefinedBy.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.IsDefinedBy)))
                           .UnionWith(this.Annotations.CustomAnnotations.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.CustomAnnotations)))
                           .UnionWith(this.Annotations.AxiomAnnotations.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.AxiomAnnotations), this, null, null)); //OWL2

            return result;
        }

        /// <summary>
        /// Asynchronously gets a graph representation of this ontology class model, exporting inferences according to the selected behavior
        /// </summary>
        public Task<RDFGraph> ToRDFGraphAsync(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
            => Task.Run(() => ToRDFGraph(infexpBehavior));
        #endregion

        #endregion
    }
}