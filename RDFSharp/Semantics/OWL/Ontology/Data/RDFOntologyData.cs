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
using RDFSharp.Semantics.SKOS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyData represents the data component (A-BOX) of an ontology.
    /// </summary>
    public class RDFOntologyData : IEnumerable<RDFOntologyFact>
    {
        #region Properties
        /// <summary>
        /// Count of the facts composing the data
        /// </summary>
        public long FactsCount
            => this.Facts.Count;

        /// <summary>
        /// Count of the literals composing the data
        /// </summary>
        public long LiteralsCount
            => this.Literals.Count;

        /// <summary>
        /// Gets the enumerator on the facts of the data for iteration
        /// </summary>
        public IEnumerator<RDFOntologyFact> FactsEnumerator
            => this.Facts.Values.GetEnumerator();

        /// <summary>
        /// Gets the enumerator on the literals of the data for iteration
        /// </summary>
        public IEnumerator<RDFOntologyLiteral> LiteralsEnumerator
            => this.Literals.Values.GetEnumerator();

        /// <summary>
        /// Annotations describing facts of the ontology data
        /// </summary>
        public RDFOntologyAnnotations Annotations { get; internal set; }

        /// <summary>
        /// Relations describing facts of the ontology data
        /// </summary>
        public RDFOntologyDataMetadata Relations { get; internal set; }

        /// <summary>
        /// Dictionary of facts composing the ontology data
        /// </summary>
        internal Dictionary<long, RDFOntologyFact> Facts { get; set; }

        /// <summary>
        /// Dictionary of literals composing the ontology data
        /// </summary>
        internal Dictionary<long, RDFOntologyLiteral> Literals { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology data
        /// </summary>
        public RDFOntologyData()
        {
            this.Facts = new Dictionary<long, RDFOntologyFact>();
            this.Literals = new Dictionary<long, RDFOntologyLiteral>();
            this.Annotations = new RDFOntologyAnnotations();
            this.Relations = new RDFOntologyDataMetadata();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the data's facts
        /// </summary>
        IEnumerator<RDFOntologyFact> IEnumerable<RDFOntologyFact>.GetEnumerator() => this.FactsEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the data's facts
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => this.FactsEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given fact to the data
        /// </summary>
        public RDFOntologyData AddFact(RDFOntologyFact ontologyFact)
        {
            if (ontologyFact != null)
            {
                if (!this.Facts.ContainsKey(ontologyFact.PatternMemberID))
                    this.Facts.Add(ontologyFact.PatternMemberID, ontologyFact);
            }
            return this;
        }

        /// <summary>
        /// Adds the given literal to the data
        /// </summary>
        public RDFOntologyData AddLiteral(RDFOntologyLiteral ontologyLiteral)
        {
            if (ontologyLiteral != null)
            {
                if (!this.Literals.ContainsKey(ontologyLiteral.PatternMemberID))
                    this.Literals.Add(ontologyLiteral.PatternMemberID, ontologyLiteral);
            }
            return this;
        }

        /// <summary>
        /// Adds the given standard annotation to the given ontology fact
        /// </summary>
        public RDFOntologyData AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation, RDFOntologyFact ontologyFact, RDFOntologyResource annotationValue)
        {
            if (ontologyFact != null && annotationValue != null)
            {
                switch (standardAnnotation)
                {
                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.VersionInfo.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology fact with owl:versionInfo value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //rdfs:comment
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.Comment.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology fact with rdfs:comment value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //rdfs:label
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.Label.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology fact with rdfs:label value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //rdfs:seeAlso
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso:
                        this.Annotations.SeeAlso.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:isDefinedBy
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy:
                        this.Annotations.IsDefinedBy.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    #region Unsupported
                    //owl:versionIRI
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology fact with owl:versionIRI because it is reserved for ontologies");
                        break;

                    //owl:priorVersion
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology fact with owl:priorVersion because it is reserved for ontologies");
                        break;

                    //owl:imports
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology fact with owl:imports because it is reserved for ontologies");
                        break;

                    //owl:backwardCompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology fact with owl:backwardCompatibleWith because it is reserved for ontologies");
                        break;

                    //owl:incompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith:
                        RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology fact with owl:incompatibleWith because it is reserved for ontologies");
                        break;
                    #endregion
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given custom annotation to the given ontology fact
        /// </summary>
        public RDFOntologyData AddCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty, RDFOntologyFact ontologyFact, RDFOntologyResource annotationValue)
        {
            if (ontologyAnnotationProperty != null && ontologyFact != null && annotationValue != null)
            {
                //standard annotation
                if (RDFSemanticsUtilities.StandardAnnotationProperties.Contains(ontologyAnnotationProperty.PatternMemberID))
                {
                    //owl:versionInfo
                    if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyFact, annotationValue);

                    //rdfs:comment
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyFact, annotationValue);

                    //rdfs:label
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyFact, annotationValue);

                    //rdfs:seeAlso
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyFact, annotationValue);

                    //rdfs:isDefinedBy
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyFact, annotationValue);

                    #region Unsupported
                    //owl:versionIRI
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyFact, annotationValue);

                    //owl:imports
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyFact, annotationValue);

                    //owl:backwardCompatibleWith
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyFact, annotationValue);

                    //owl:incompatibleWith
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyFact, annotationValue);

                    //owl:priorVersion
                    else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION))
                        this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyFact, annotationValue);
                    #endregion
                }

                //custom annotation
                else
                    this.Annotations.CustomAnnotations.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, ontologyAnnotationProperty, annotationValue));
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyFact -> rdf:type -> ontologyClass" relation to the data (and links the given axiom annotation if provided)
        /// </summary>
        public RDFOntologyData AddClassTypeRelation(RDFOntologyFact ontologyFact, RDFOntologyClass ontologyClass, RDFOntologyAxiomAnnotation axiomAnnotation=null)
        {
            if (ontologyFact != null && ontologyClass != null)
            {
                //Enforce preliminary check on usage of BASE classes
                if (!RDFOntologyChecker.CheckReservedClass(ontologyClass))
                {
                    //Enforce taxonomy checks before adding the ClassType relation
                    if (RDFOntologyChecker.CheckClassTypeCompatibility(ontologyClass))
                    {
                        RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), ontologyClass);
                        this.Relations.ClassType.AddEntry(taxonomyEntry);
                        this.AddFact(ontologyFact);

                        //Link owl:Axiom annotation
                        this.AddAxiomAnnotation(taxonomyEntry, axiomAnnotation, nameof(RDFOntologyDataMetadata.ClassType));
                    }
                    else
                    {
                        //Raise warning event to inform the user: ClassType relation cannot be added to the data because only plain classes can be explicitly assigned as class types of facts
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("ClassType relation between fact '{0}' and class '{1}' cannot be added to the data because only plain classes can be explicitly assigned as class types of facts.", ontologyFact, ontologyClass));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: ClassType relation cannot be added to the data because usage of BASE reserved classes compromises the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("ClassType relation between fact '{0}' and class '{1}' cannot be added to the data because because usage of BASE reserved classes compromises the taxonomy consistency.", ontologyFact, ontologyClass));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "aFact -> owl:sameAs -> bFact" relation to the data (and links the given axiom annotation if provided)
        /// </summary>
        public RDFOntologyData AddSameAsRelation(RDFOntologyFact aFact, RDFOntologyFact bFact, RDFOntologyAxiomAnnotation axiomAnnotation=null)
        {
            if (aFact != null && bFact != null && !aFact.Equals(bFact))
            {
                //Enforce taxonomy checks before adding the SameAs relation
                if (RDFOntologyChecker.CheckSameAsCompatibility(this, aFact, bFact))
                {
                    RDFOntologyTaxonomyEntry sameAsLeft = new RDFOntologyTaxonomyEntry(aFact, RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), bFact);
                    this.Relations.SameAs.AddEntry(sameAsLeft);
                    RDFOntologyTaxonomyEntry sameAsRight = new RDFOntologyTaxonomyEntry(bFact, RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), aFact).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API);
                    this.Relations.SameAs.AddEntry(sameAsRight);

                    //Link owl:Axiom annotation
                    this.AddAxiomAnnotation(sameAsLeft, axiomAnnotation, nameof(RDFOntologyDataMetadata.SameAs));
                }
                else
                {
                    //Raise warning event to inform the user: SameAs relation cannot be added to the data because it violates the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("SameAs relation between fact '{0}' and fact '{1}' cannot be added to the data because it violates the taxonomy consistency.", aFact, bFact));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "aFact -> owl:differentFrom -> bFact" relation to the data (and links the given axiom annotation if provided)
        /// </summary>
        public RDFOntologyData AddDifferentFromRelation(RDFOntologyFact aFact, RDFOntologyFact bFact, RDFOntologyAxiomAnnotation axiomAnnotation=null)
        {
            if (aFact != null && bFact != null && !aFact.Equals(bFact))
            {
                //Enforce taxonomy checks before adding the DifferentFrom relation
                if (RDFOntologyChecker.CheckDifferentFromCompatibility(this, aFact, bFact))
                {
                    RDFOntologyTaxonomyEntry differentFromLeft = new RDFOntologyTaxonomyEntry(aFact, RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), bFact);
                    this.Relations.DifferentFrom.AddEntry(differentFromLeft);
                    RDFOntologyTaxonomyEntry differentFromRight = new RDFOntologyTaxonomyEntry(bFact, RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), aFact).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API);
                    this.Relations.DifferentFrom.AddEntry(differentFromRight);

                    //Link owl:Axiom annotation
                    this.AddAxiomAnnotation(differentFromLeft, axiomAnnotation, nameof(RDFOntologyDataMetadata.DifferentFrom));
                }
                else
                {
                    //Raise warning event to inform the user: DifferentFrom relation cannot be added to the data because it violates the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("DifferentFrom relation between fact '{0}' and fact '{1}' cannot be added to the data because it violates the taxonomy consistency.", aFact, bFact));
                }
            }
            return this;
        }

        /// <summary>
        /// Foreach of the given list of facts, adds the "aFact -> owl:differentFrom -> bFact" relation to the data [OWL2]
        /// </summary>
        public RDFOntologyData AddAllDifferentRelation(List<RDFOntologyFact> ontologyFacts)
        {
            ontologyFacts?.ForEach(outerFact =>
                ontologyFacts?.ForEach(innerFact => this.AddDifferentFromRelation(innerFact, outerFact)));
            return this;
        }

        /// <summary>
        /// Adds the "aFact -> objectProperty -> bFact" relation to the data (and links the given axiom annotation if provided)
        /// </summary>
        public RDFOntologyData AddAssertionRelation(RDFOntologyFact aFact, RDFOntologyObjectProperty objectProperty, RDFOntologyFact bFact, RDFOntologyAxiomAnnotation axiomAnnotation=null)
        {
            if (aFact != null && objectProperty != null && bFact != null)
            {
                //Enforce preliminary check on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(objectProperty))
                {
                    //Enforce taxonomy checks before adding the assertion
                    //Creation of transitive cycles is not allowed [OWL-DL]
                    if (RDFOntologyChecker.CheckTransitiveAssertionCompatibility(this, aFact, objectProperty, bFact))
                    {
                        //Collision with negative assertions must be avoided [OWL2]
                        if (RDFOntologyChecker.CheckAssertionCompatibility(this, aFact, objectProperty, bFact))
                        {
                            if (objectProperty.Equals(RDFVocabulary.SKOS.MEMBER))
                                this.AddMemberRelation(aFact, bFact, axiomAnnotation);
                            else if (objectProperty.Equals(RDFVocabulary.SKOS.MEMBER_LIST))
                                this.AddMemberListRelation(aFact, bFact, axiomAnnotation);
                            else
                            {
                                RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(aFact, objectProperty, bFact);
                                this.Relations.Assertions.AddEntry(taxonomyEntry);

                                //Link owl:Axiom annotation
                                this.AddAxiomAnnotation(taxonomyEntry, axiomAnnotation, nameof(RDFOntologyDataMetadata.Assertions));
                            }
                        }
                        else
                        {
                            //Raise warning event to inform the user: Assertion relation cannot be added to the data because it violates the taxonomy consistency
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation between fact '{0}' and fact '{1}' with property '{2}' cannot be added to the data because it would violate the taxonomy consistency (negative assertion detected).", aFact, bFact, objectProperty));
                        }
                    }
                    else
                    {
                        //Raise warning event to inform the user: Assertion relation cannot be added to the data because it violates the taxonomy transitive consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation between fact '{0}' and fact '{1}' with transitive property '{2}' cannot be added to the data because it would violate the taxonomy consistency (transitive cycle detected).", aFact, bFact, objectProperty));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: Assertion relation cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation between fact '{0}' and fact '{1}' cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency.", aFact, bFact));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyFact -> datatypeProperty -> ontologyLiteral" relation to the data (and links the given axiom annotation if provided)
        /// </summary>
        public RDFOntologyData AddAssertionRelation(RDFOntologyFact ontologyFact, RDFOntologyDatatypeProperty datatypeProperty, RDFOntologyLiteral ontologyLiteral, RDFOntologyAxiomAnnotation axiomAnnotation = null)
        {
            if (ontologyFact != null && datatypeProperty != null && ontologyLiteral != null)
            {
                //Enforce preliminary check on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(datatypeProperty))
                {
                    //Collision with negative assertions must be avoided [OWL2]
                    if (RDFOntologyChecker.CheckAssertionCompatibility(this, ontologyFact, datatypeProperty, ontologyLiteral))
                    {
                        //Cannot accept assertion in case of SKOS collection predicates
                        if (!datatypeProperty.Equals(RDFVocabulary.SKOS.MEMBER)
                                && !datatypeProperty.Equals(RDFVocabulary.SKOS.MEMBER_LIST))
                        {
                            RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(ontologyFact, datatypeProperty, ontologyLiteral);
                            this.Relations.Assertions.AddEntry(taxonomyEntry);
                            this.AddLiteral(ontologyLiteral);

                            //Link owl:Axiom annotation
                            this.AddAxiomAnnotation(taxonomyEntry, axiomAnnotation, nameof(RDFOntologyDataMetadata.Assertions));
                        }
                        else
                        {
                            //Raise warning event to inform the user: Assertion relation cannot be added to the data because it violates the taxonomy consistency
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation between fact '{0}' and literal '{1}' with property '{2}' cannot be added to the data because it would violate the taxonomy consistency (SKOS collection detected).", ontologyFact, ontologyLiteral, datatypeProperty));
                        }
                    }
                    else
                    {
                        //Raise warning event to inform the user: Assertion relation cannot be added to the data because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation between fact '{0}' and literal '{1}' with property '{2}' cannot be added to the data because it would violate the taxonomy consistency (negative assertion detected).", ontologyFact, ontologyLiteral, datatypeProperty));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: Assertion relation cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("Assertion relation between fact '{0}' and literal '{1}' cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency.", ontologyFact, ontologyLiteral));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "aFact -> objectProperty -> bFact" negative relation to the data [OWL2]
        /// </summary>
        public RDFOntologyData AddNegativeAssertionRelation(RDFOntologyFact aFact, RDFOntologyObjectProperty objectProperty, RDFOntologyFact bFact)
        {
            if (aFact != null && objectProperty != null && bFact != null)
            {
                //Enforce preliminary check on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(objectProperty))
                {
                    //Collision with assertions must be avoided [OWL2]
                    if (RDFOntologyChecker.CheckNegativeAssertionCompatibility(this, aFact, objectProperty, bFact))
                    {
                        //Cannot accept negative assertion in case of SKOS collection predicates
                        if (!objectProperty.Equals(RDFVocabulary.SKOS.MEMBER)
                                && !objectProperty.Equals(RDFVocabulary.SKOS.MEMBER_LIST))
                            this.Relations.NegativeAssertions.AddEntry(new RDFOntologyTaxonomyEntry(aFact, objectProperty, bFact));
                        else
                        {
                            //Raise warning event to inform the user: NegativeAssertion relation cannot be added to the data because it violates the taxonomy consistency
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation between fact '{0}' and fact '{1}' with property '{2}' cannot be added to the data because it would violate the taxonomy consistency (SKOS collection detected).", aFact, bFact, objectProperty));
                        }
                    }
                    else
                    {
                        //Raise warning event to inform the user: NegativeAssertion relation cannot be added to the data because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation between fact '{0}' and fact '{1}' with property '{2}' cannot be added to the data because it would violate the taxonomy consistency (assertion detected).", aFact, bFact, objectProperty));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: NegativeAssertion relation cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation between fact '{0}' and fact '{1}' cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency.", aFact, bFact));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyFact -> datatypeProperty -> ontologyLiteral" negative relation to the data [OWL2]
        /// </summary>
        public RDFOntologyData AddNegativeAssertionRelation(RDFOntologyFact ontologyFact, RDFOntologyDatatypeProperty datatypeProperty, RDFOntologyLiteral ontologyLiteral)
        {
            if (ontologyFact != null && datatypeProperty != null && ontologyLiteral != null)
            {
                //Enforce preliminary check on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(datatypeProperty))
                {
                    //Collision with assertions must be avoided [OWL2]
                    if (RDFOntologyChecker.CheckNegativeAssertionCompatibility(this, ontologyFact, datatypeProperty, ontologyLiteral))
                    {
                        //Cannot accept negative assertion in case of SKOS collection predicates
                        if (!datatypeProperty.Equals(RDFVocabulary.SKOS.MEMBER)
                                && !datatypeProperty.Equals(RDFVocabulary.SKOS.MEMBER_LIST))
                        {
                            this.Relations.NegativeAssertions.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, datatypeProperty, ontologyLiteral));
                            this.AddLiteral(ontologyLiteral);
                        }
                        else
                        {
                            //Raise warning event to inform the user: NegativeAssertion relation cannot be added to the data because it violates the taxonomy consistency
                            RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation between fact '{0}' and literal '{1}' with property '{2}' cannot be added to the data because it would violate the taxonomy consistency (SKOS collection detected).", ontologyFact, ontologyLiteral, datatypeProperty));
                        }
                    }
                    else
                    {
                        //Raise warning event to inform the user: NegativeAssertion relation cannot be added to the data because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation between fact '{0}' and literal '{1}' with property '{2}' cannot be added to the data because it would violate the taxonomy consistency (assertion detected).", ontologyFact, ontologyLiteral, datatypeProperty));
                    }
                }
                else
                {
                    //Raise warning event to inform the user: NegativeAssertion relation cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(string.Format("NegativeAssertion relation between fact '{0}' and literal '{1}' cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency.", ontologyFact, ontologyLiteral));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given owl:Axiom annotation to the given taxonomy entry
        /// </summary>
        internal RDFOntologyData AddAxiomAnnotation(RDFOntologyTaxonomyEntry taxonomyEntry, RDFOntologyAxiomAnnotation axiomAnnotation, string targetTaxonomyName)
        {
            #region DetectTargetTaxonomy
            RDFOntologyTaxonomy DetectTargetTaxonomy()
            {
                RDFOntologyTaxonomy targetTaxonomy = default;
                switch (targetTaxonomyName)
                {
                    case nameof(RDFOntologyDataMetadata.ClassType):
                        targetTaxonomy = this.Relations.ClassType;
                        break;
                    case nameof(RDFOntologyDataMetadata.SameAs):
                        targetTaxonomy = this.Relations.SameAs;
                        break;
                    case nameof(RDFOntologyDataMetadata.DifferentFrom):
                        targetTaxonomy = this.Relations.DifferentFrom;
                        break;
                    case nameof(RDFOntologyDataMetadata.Assertions):
                        targetTaxonomy = this.Relations.Assertions;
                        break;
                    case nameof(RDFOntologyDataMetadata.Member):
                        targetTaxonomy = this.Relations.Member;
                        break;
                    case nameof(RDFOntologyDataMetadata.MemberList):
                        targetTaxonomy = this.Relations.MemberList;
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
        /// Removes the given fact from the data
        /// </summary>
        public RDFOntologyData RemoveFact(RDFOntologyFact ontologyFact)
        {
            if (ontologyFact != null)
            {
                //Declaration
                if (this.Facts.ContainsKey(ontologyFact.PatternMemberID))
                    this.Facts.Remove(ontologyFact.PatternMemberID);

                //ClassType
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.ClassType.SelectEntriesBySubject(ontologyFact))
                {
                    this.Relations.ClassType.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }   

                //SameAs
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.SameAs.SelectEntriesBySubject(ontologyFact))
                {
                    this.Relations.SameAs.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.SameAs.SelectEntriesByObject(ontologyFact))
                { 
                    this.Relations.SameAs.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }

                //DifferentFrom
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.DifferentFrom.SelectEntriesBySubject(ontologyFact))
                {
                    this.Relations.DifferentFrom.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }                    
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.DifferentFrom.SelectEntriesByObject(ontologyFact))
                {
                    this.Relations.DifferentFrom.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }   

                //Assertions
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Assertions.SelectEntriesBySubject(ontologyFact))
                { 
                    this.Relations.Assertions.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Assertions.SelectEntriesByObject(ontologyFact))
                { 
                    this.Relations.Assertions.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }

                //Member [SKOS]
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Member.SelectEntriesBySubject(ontologyFact))
                {
                    this.Relations.Member.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }                    
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Member.SelectEntriesByObject(ontologyFact))
                {
                    this.Relations.Member.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }                    

                //MemberList [SKOS]
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.MemberList.SelectEntriesBySubject(ontologyFact))
                {
                    this.Relations.MemberList.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }   
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.MemberList.SelectEntriesByObject(ontologyFact))
                {
                    this.Relations.MemberList.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }   

                //NegativeAssertions [OWL2]
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.NegativeAssertions.SelectEntriesBySubject(ontologyFact))
                    this.Relations.NegativeAssertions.RemoveEntry(taxonomyEntry);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.NegativeAssertions.SelectEntriesByObject(ontologyFact))
                    this.Relations.NegativeAssertions.RemoveEntry(taxonomyEntry);

                //Annotations
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.VersionInfo.SelectEntriesBySubject(ontologyFact))
                    this.Annotations.VersionInfo.RemoveEntry(taxonomyEntry);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.Comment.SelectEntriesBySubject(ontologyFact))
                    this.Annotations.Comment.RemoveEntry(taxonomyEntry);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.Label.SelectEntriesBySubject(ontologyFact))
                    this.Annotations.Label.RemoveEntry(taxonomyEntry);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.SeeAlso.SelectEntriesBySubject(ontologyFact))
                    this.Annotations.SeeAlso.RemoveEntry(taxonomyEntry);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.IsDefinedBy.SelectEntriesBySubject(ontologyFact))
                    this.Annotations.IsDefinedBy.RemoveEntry(taxonomyEntry);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.CustomAnnotations.SelectEntriesBySubject(ontologyFact))
                    this.Annotations.CustomAnnotations.RemoveEntry(taxonomyEntry);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.CustomAnnotations.SelectEntriesByObject(ontologyFact))
                    this.Annotations.CustomAnnotations.RemoveEntry(taxonomyEntry);
            }
            return this;
        }

        /// <summary>
        /// Replaces all the occurrences of the given fact with the given new fact in the data
        /// </summary>
        public RDFOntologyData ReplaceFact(RDFOntologyFact ontologyFact, RDFOntologyFact newOntologyFact)
        {
            if (ontologyFact != null && newOntologyFact != null && !ontologyFact.Equals(newOntologyFact))
            {
                //Declarations
                this.AddFact(newOntologyFact);

                //ClassType
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.ClassType.SelectEntriesBySubject(ontologyFact))
                    this.AddClassTypeRelation(newOntologyFact, (RDFOntologyClass)taxonomyEntry.TaxonomyObject);

                //SameAs
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.SameAs.SelectEntriesBySubject(ontologyFact))
                    this.AddSameAsRelation(newOntologyFact, (RDFOntologyFact)taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.SameAs.SelectEntriesByObject(ontologyFact))
                    this.AddSameAsRelation((RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyFact);

                //DifferentFrom
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.DifferentFrom.SelectEntriesBySubject(ontologyFact))
                    this.AddDifferentFromRelation(newOntologyFact, (RDFOntologyFact)taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.DifferentFrom.SelectEntriesByObject(ontologyFact))
                    this.AddDifferentFromRelation((RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyFact);

                //Assertions
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Assertions.SelectEntriesBySubject(ontologyFact))
                {
                    if (taxonomyEntry.TaxonomyObject.Value is RDFLiteral)
                        this.AddAssertionRelation(newOntologyFact, (RDFOntologyDatatypeProperty)taxonomyEntry.TaxonomyPredicate, (RDFOntologyLiteral)taxonomyEntry.TaxonomyObject);
                    else
                        this.AddAssertionRelation(newOntologyFact, (RDFOntologyObjectProperty)taxonomyEntry.TaxonomyPredicate, (RDFOntologyFact)taxonomyEntry.TaxonomyObject);
                }
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Assertions.SelectEntriesByObject(ontologyFact))
                    this.AddAssertionRelation((RDFOntologyFact)taxonomyEntry.TaxonomySubject, (RDFOntologyObjectProperty)taxonomyEntry.TaxonomyPredicate, newOntologyFact);

                //Member [SKOS]
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Member.SelectEntriesBySubject(ontologyFact))
                    this.AddMemberRelation(newOntologyFact, (RDFOntologyFact)taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Member.SelectEntriesByObject(ontologyFact))
                    this.AddMemberRelation((RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyFact);

                //MemberList [SKOS]
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.MemberList.SelectEntriesBySubject(ontologyFact))
                    this.AddMemberListRelation(newOntologyFact, (RDFOntologyFact)taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.MemberList.SelectEntriesByObject(ontologyFact))
                    this.AddMemberListRelation((RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyFact);

                //NegativeAssertions [OWL2]
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.NegativeAssertions.SelectEntriesBySubject(ontologyFact))
                {
                    if (taxonomyEntry.TaxonomyObject.Value is RDFLiteral)
                        this.AddNegativeAssertionRelation(newOntologyFact, (RDFOntologyDatatypeProperty)taxonomyEntry.TaxonomyPredicate, (RDFOntologyLiteral)taxonomyEntry.TaxonomyObject);
                    else
                        this.AddNegativeAssertionRelation(newOntologyFact, (RDFOntologyObjectProperty)taxonomyEntry.TaxonomyPredicate, (RDFOntologyFact)taxonomyEntry.TaxonomyObject);
                }
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.NegativeAssertions.SelectEntriesByObject(ontologyFact))
                    this.AddNegativeAssertionRelation((RDFOntologyFact)taxonomyEntry.TaxonomySubject, (RDFOntologyObjectProperty)taxonomyEntry.TaxonomyPredicate, newOntologyFact);

                //Annotations
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.VersionInfo.SelectEntriesBySubject(ontologyFact))
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, newOntologyFact, taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.Comment.SelectEntriesBySubject(ontologyFact))
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, newOntologyFact, taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.Label.SelectEntriesBySubject(ontologyFact))
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, newOntologyFact, taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.SeeAlso.SelectEntriesBySubject(ontologyFact))
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, newOntologyFact, taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.IsDefinedBy.SelectEntriesBySubject(ontologyFact))
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, newOntologyFact, taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.CustomAnnotations.SelectEntriesBySubject(ontologyFact))
                    this.AddCustomAnnotation((RDFOntologyAnnotationProperty)taxonomyEntry.TaxonomyPredicate, newOntologyFact, taxonomyEntry.TaxonomyObject);
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.CustomAnnotations.SelectEntriesByObject(ontologyFact))
                    this.AddCustomAnnotation((RDFOntologyAnnotationProperty)taxonomyEntry.TaxonomyPredicate, (RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyFact);

                //Drop replaced fact
                this.RemoveFact(ontologyFact);                
            }
            return this;
        }

        /// <summary>
        /// Removes the given literal from the data
        /// </summary>
        public RDFOntologyData RemoveLiteral(RDFOntologyLiteral ontologyLiteral)
        {
            if (ontologyLiteral != null)
            {
                if (this.Literals.ContainsKey(ontologyLiteral.PatternMemberID))
                    this.Literals.Remove(ontologyLiteral.PatternMemberID);
            }
            return this;
        }

        /// <summary>
        /// Replaces all the occurrences of the given literal with the given new literal in the data
        /// </summary>
        public RDFOntologyData ReplaceLiteral(RDFOntologyLiteral ontologyLiteral, RDFOntologyLiteral newOntologyLiteral)
        {
            if (ontologyLiteral != null && newOntologyLiteral != null && !ontologyLiteral.Equals(newOntologyLiteral))
            {
                //Declarations
                this.RemoveLiteral(ontologyLiteral);
                this.AddLiteral(newOntologyLiteral);

                //Assertions
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.Assertions.SelectEntriesByObject(ontologyLiteral))
                {
                    this.Relations.Assertions.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);

                    this.AddAssertionRelation((RDFOntologyFact)taxonomyEntry.TaxonomySubject, (RDFOntologyDatatypeProperty)taxonomyEntry.TaxonomyPredicate, newOntologyLiteral);
                }

                //NegativeAssertions [OWL2]
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Relations.NegativeAssertions.SelectEntriesByObject(ontologyLiteral))
                {
                    this.Relations.NegativeAssertions.RemoveEntry(taxonomyEntry);
                    this.AddNegativeAssertionRelation((RDFOntologyFact)taxonomyEntry.TaxonomySubject, (RDFOntologyDatatypeProperty)taxonomyEntry.TaxonomyPredicate, newOntologyLiteral);
                }

                //Annotations
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.VersionInfo.SelectEntriesByObject(ontologyLiteral))
                {
                    this.Annotations.VersionInfo.RemoveEntry(taxonomyEntry);
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, (RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyLiteral);
                }
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.Comment.SelectEntriesByObject(ontologyLiteral))
                {
                    this.Annotations.Comment.RemoveEntry(taxonomyEntry);
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, (RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyLiteral);
                }
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.Label.SelectEntriesByObject(ontologyLiteral))
                {
                    this.Annotations.Label.RemoveEntry(taxonomyEntry);
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, (RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyLiteral);
                }   
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.SeeAlso.SelectEntriesByObject(ontologyLiteral))
                {
                    this.Annotations.SeeAlso.RemoveEntry(taxonomyEntry);
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, (RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyLiteral);
                }
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.IsDefinedBy.SelectEntriesByObject(ontologyLiteral))
                {
                    this.Annotations.IsDefinedBy.RemoveEntry(taxonomyEntry);
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, (RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyLiteral);
                }
                foreach (RDFOntologyTaxonomyEntry taxonomyEntry in this.Annotations.CustomAnnotations.SelectEntriesByObject(ontologyLiteral))
                {
                    this.Annotations.CustomAnnotations.RemoveEntry(taxonomyEntry);
                    this.AddCustomAnnotation((RDFOntologyAnnotationProperty)taxonomyEntry.TaxonomyPredicate, (RDFOntologyFact)taxonomyEntry.TaxonomySubject, newOntologyLiteral);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given standard annotation from the given ontology fact
        /// </summary>
        public RDFOntologyData RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation, RDFOntologyFact ontologyFact, RDFOntologyResource annotationValue)
        {
            if (ontologyFact != null && annotationValue != null)
            {
                switch (standardAnnotation)
                {
                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                        this.Annotations.VersionInfo.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:comment
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                        this.Annotations.Comment.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:label
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                        this.Annotations.Label.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:seeAlso
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso:
                        this.Annotations.SeeAlso.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:isDefinedBy
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy:
                        this.Annotations.IsDefinedBy.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given custom annotation from the given ontology fact
        /// </summary>
        public RDFOntologyData RemoveCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty, RDFOntologyFact ontologyFact, RDFOntologyResource annotationValue)
        {
            if (ontologyAnnotationProperty != null && ontologyFact != null && annotationValue != null)
            {
                //owl:versionInfo
                if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyFact, annotationValue);
                
                //rdfs:comment
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyFact, annotationValue);
                
                //rdfs:label
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyFact, annotationValue);
                
                //rdfs:seeAlso
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyFact, annotationValue);
                
                //rdfs:isDefinedBy
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyFact, annotationValue);

                #region Unsupported
                //owl:versionIRI
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyFact, annotationValue);

                //owl:imports
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyFact, annotationValue);
                
                //owl:backwardCompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyFact, annotationValue);
                
                //owl:incompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyFact, annotationValue);
                
                //owl:priorVersion
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty()))
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyFact, annotationValue);
                #endregion

                //custom annotation
                else
                    this.Annotations.CustomAnnotations.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, ontologyAnnotationProperty, annotationValue));
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyFact -> rdf:type -> ontologyClass" relation from the data
        /// </summary>
        public RDFOntologyData RemoveClassTypeRelation(RDFOntologyFact ontologyFact, RDFOntologyClass ontologyClass)
        {
            if (ontologyFact != null && ontologyClass != null)
            {
                RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), ontologyClass);
                this.Relations.ClassType.RemoveEntry(taxonomyEntry);

                //Unlink owl:Axiom annotation
                this.RemoveAxiomAnnotation(taxonomyEntry);
            }                
            return this;
        }

        /// <summary>
        /// Removes the "aFact -> owl:sameAs -> bFact" relation from the data
        /// </summary>
        public RDFOntologyData RemoveSameAsRelation(RDFOntologyFact aFact, RDFOntologyFact bFact)
        {
            if (aFact != null && bFact != null)
            {
                RDFOntologyTaxonomyEntry sameAsLeft = new RDFOntologyTaxonomyEntry(aFact, RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), bFact);
                this.Relations.SameAs.RemoveEntry(sameAsLeft);
                RDFOntologyTaxonomyEntry sameAsRight = new RDFOntologyTaxonomyEntry(bFact, RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), aFact);
                this.Relations.SameAs.RemoveEntry(sameAsRight);

                //Unlink owl:Axiom annotations
                this.RemoveAxiomAnnotation(sameAsLeft);
                this.RemoveAxiomAnnotation(sameAsRight);
            }
            return this;
        }

        /// <summary>
        /// Removes the "aFact -> owl:differentFrom -> bFact" relation from the data
        /// </summary>
        public RDFOntologyData RemoveDifferentFromRelation(RDFOntologyFact aFact, RDFOntologyFact bFact)
        {
            if (aFact != null && bFact != null)
            {
                RDFOntologyTaxonomyEntry differentFromLeft = new RDFOntologyTaxonomyEntry(aFact, RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), bFact);
                this.Relations.DifferentFrom.RemoveEntry(differentFromLeft);
                RDFOntologyTaxonomyEntry differentFromRight = new RDFOntologyTaxonomyEntry(bFact, RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), aFact);
                this.Relations.DifferentFrom.RemoveEntry(differentFromRight);

                //Unlink owl:Axiom annotations
                this.RemoveAxiomAnnotation(differentFromLeft);
                this.RemoveAxiomAnnotation(differentFromRight);
            }
            return this;
        }

        /// <summary>
        /// Foreach of the given list of facts, removes the "aFact -> owl:differentFrom -> bFact" relation from the data [OWL2]
        /// </summary>
        public RDFOntologyData RemoveAllDifferentRelation(List<RDFOntologyFact> ontologyFacts)
        {
            ontologyFacts?.ForEach(outerFact =>
            {
                ontologyFacts?.ForEach(innerFact =>
                {
                    this.RemoveDifferentFromRelation(innerFact, outerFact);
                });
            });
            return this;
        }

        /// <summary>
        /// Removes the "aFact -> objectProperty -> bFact" relation from the data
        /// </summary>
        public RDFOntologyData RemoveAssertionRelation(RDFOntologyFact aFact, RDFOntologyObjectProperty objectProperty, RDFOntologyFact bFact)
        {
            if (aFact != null && objectProperty != null && bFact != null)
            {
                if (objectProperty.Equals(RDFVocabulary.SKOS.MEMBER))
                    this.RemoveMemberRelation(aFact, bFact);
                else if (objectProperty.Equals(RDFVocabulary.SKOS.MEMBER_LIST))
                    this.RemoveMemberListRelation(aFact, bFact);
                else
                {
                    RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(aFact, objectProperty, bFact);
                    this.Relations.Assertions.RemoveEntry(taxonomyEntry);

                    //Unlink owl:Axiom annotation
                    this.RemoveAxiomAnnotation(taxonomyEntry);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyFact -> datatypeProperty -> ontologyLiteral" relation from the data
        /// </summary>
        public RDFOntologyData RemoveAssertionRelation(RDFOntologyFact ontologyFact, RDFOntologyDatatypeProperty datatypeProperty, RDFOntologyLiteral ontologyLiteral)
        {
            if (ontologyFact != null && datatypeProperty != null && ontologyLiteral != null)
            {
                RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(ontologyFact, datatypeProperty, ontologyLiteral);
                this.Relations.Assertions.RemoveEntry(taxonomyEntry);

                //Unlink owl:Axiom annotation
                this.RemoveAxiomAnnotation(taxonomyEntry);
            }
            return this;
        }

        /// <summary>
        /// Removes the "aFact -> objectProperty -> bFact" negative relation from the data [OWL2]
        /// </summary>
        public RDFOntologyData RemoveNegativeAssertionRelation(RDFOntologyFact aFact, RDFOntologyObjectProperty objectProperty, RDFOntologyFact bFact)
        {
            if (aFact != null && objectProperty != null && bFact != null)
                this.Relations.NegativeAssertions.RemoveEntry(new RDFOntologyTaxonomyEntry(aFact, objectProperty, bFact));
            return this;
        }

        /// <summary>
        /// Removes the "ontologyFact -> datatypeProperty -> ontologyLiteral" negative relation from the data [OWL2]
        /// </summary>
        public RDFOntologyData RemoveNegativeAssertionRelation(RDFOntologyFact ontologyFact, RDFOntologyDatatypeProperty datatypeProperty, RDFOntologyLiteral ontologyLiteral)
        {
            if (ontologyFact != null && datatypeProperty != null && ontologyLiteral != null)
                this.Relations.NegativeAssertions.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, datatypeProperty, ontologyLiteral));
            return this;
        }

        /// <summary>
        /// Removes the given owl:Axiom annotation [OWL2]
        /// </summary>
        internal RDFOntologyData RemoveAxiomAnnotation(RDFOntologyTaxonomyEntry taxonomyEntry)
        {
            foreach (RDFOntologyTaxonomyEntry axnTaxonomyEntry in this.Annotations.AxiomAnnotations.SelectEntriesBySubject(this.GetTaxonomyEntryRepresentative(taxonomyEntry)))
                this.Annotations.AxiomAnnotations.RemoveEntry(axnTaxonomyEntry);
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the fact represented by the given identifier
        /// </summary>
        public RDFOntologyFact SelectFact(long factID)
            => this.Facts.ContainsKey(factID) ? this.Facts[factID] : null;

        /// <summary>
        /// Selects the fact represented by the given string
        /// </summary>
        public RDFOntologyFact SelectFact(string fact)
            => fact != null ? SelectFact(RDFModelUtilities.CreateHash(fact)) : null;

        /// <summary>
        /// Selects the literal represented by the given identifier
        /// </summary>
        public RDFOntologyLiteral SelectLiteral(long literalID)
            => this.Literals.ContainsKey(literalID) ? this.Literals[literalID] : null;

        /// <summary>
        /// Selects the literal represented by the given string
        /// </summary>
        public RDFOntologyLiteral SelectLiteral(string literal)
            => literal != null ? SelectLiteral(RDFModelUtilities.CreateHash(literal)) : null;

        /// <summary>
        /// Gets the representative of the given taxonomy entry
        /// </summary>
        internal RDFOntologyFact GetTaxonomyEntryRepresentative(RDFOntologyTaxonomyEntry taxonomyEntry)
            => new RDFOntologyFact(new RDFResource($"bnode:semref{taxonomyEntry.TaxonomyEntryID}"));
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection data from this data and a given one
        /// </summary>
        public RDFOntologyData IntersectWith(RDFOntologyData ontologyData)
        {
            RDFOntologyData result = new RDFOntologyData();

            if (ontologyData != null)
            {
                //Add intersection facts
                foreach (RDFOntologyFact f in this)
                    if (ontologyData.Facts.ContainsKey(f.PatternMemberID))
                        result.AddFact(f);

                //Add intersection literals
                foreach (RDFOntologyLiteral l in this.Literals.Values)
                    if (ontologyData.Literals.ContainsKey(l.PatternMemberID))
                        result.AddLiteral(l);

                //Add intersection relations
                result.Relations.ClassType = this.Relations.ClassType.IntersectWith(ontologyData.Relations.ClassType);
                result.Relations.SameAs = this.Relations.SameAs.IntersectWith(ontologyData.Relations.SameAs);
                result.Relations.DifferentFrom = this.Relations.DifferentFrom.IntersectWith(ontologyData.Relations.DifferentFrom);
                result.Relations.Assertions = this.Relations.Assertions.IntersectWith(ontologyData.Relations.Assertions);
                result.Relations.NegativeAssertions = this.Relations.NegativeAssertions.IntersectWith(ontologyData.Relations.NegativeAssertions); //OWL2
                result.Relations.Member = this.Relations.Member.IntersectWith(ontologyData.Relations.Member); //SKOS
                result.Relations.MemberList = this.Relations.MemberList.IntersectWith(ontologyData.Relations.MemberList); //SKOS

                //Add intersection annotations
                result.Annotations.VersionInfo = this.Annotations.VersionInfo.IntersectWith(ontologyData.Annotations.VersionInfo);
                result.Annotations.Comment = this.Annotations.Comment.IntersectWith(ontologyData.Annotations.Comment);
                result.Annotations.Label = this.Annotations.Label.IntersectWith(ontologyData.Annotations.Label);
                result.Annotations.SeeAlso = this.Annotations.SeeAlso.IntersectWith(ontologyData.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = this.Annotations.IsDefinedBy.IntersectWith(ontologyData.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.IntersectWith(ontologyData.Annotations.CustomAnnotations);
                result.Annotations.AxiomAnnotations = this.Annotations.AxiomAnnotations.IntersectWith(ontologyData.Annotations.AxiomAnnotations); //OWL2
            }

            return result;
        }

        /// <summary>
        /// Builds a new union data from this data and a given one
        /// </summary>
        public RDFOntologyData UnionWith(RDFOntologyData ontologyData)
        {
            RDFOntologyData result = new RDFOntologyData();

            //Add facts from this data
            foreach (RDFOntologyFact f in this)
                result.AddFact(f);

            //Add literals from this data
            foreach (RDFOntologyLiteral l in this.Literals.Values)
                result.AddLiteral(l);

            //Add relations from this data
            result.Relations.ClassType = result.Relations.ClassType.UnionWith(this.Relations.ClassType);
            result.Relations.SameAs = result.Relations.SameAs.UnionWith(this.Relations.SameAs);
            result.Relations.DifferentFrom = result.Relations.DifferentFrom.UnionWith(this.Relations.DifferentFrom);
            result.Relations.Assertions = result.Relations.Assertions.UnionWith(this.Relations.Assertions);
            result.Relations.NegativeAssertions = result.Relations.NegativeAssertions.UnionWith(this.Relations.NegativeAssertions); //OWL2
            result.Relations.Member = result.Relations.Member.UnionWith(this.Relations.Member); //SKOS
            result.Relations.MemberList = result.Relations.MemberList.UnionWith(this.Relations.MemberList); //SKOS

            //Add annotations from this data
            result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
            result.Annotations.Comment = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
            result.Annotations.Label = result.Annotations.Label.UnionWith(this.Annotations.Label);
            result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
            result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
            result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);
            result.Annotations.AxiomAnnotations = result.Annotations.AxiomAnnotations.UnionWith(this.Annotations.AxiomAnnotations); //OWL2

            //Manage the given data
            if (ontologyData != null)
            {
                //Add facts from the given data
                foreach (RDFOntologyFact f in ontologyData)
                    result.AddFact(f);

                //Add literals from the given data
                foreach (RDFOntologyLiteral l in ontologyData.Literals.Values)
                    result.AddLiteral(l);

                //Add relations from the given data
                result.Relations.ClassType = result.Relations.ClassType.UnionWith(ontologyData.Relations.ClassType);
                result.Relations.SameAs = result.Relations.SameAs.UnionWith(ontologyData.Relations.SameAs);
                result.Relations.DifferentFrom = result.Relations.DifferentFrom.UnionWith(ontologyData.Relations.DifferentFrom);
                result.Relations.Assertions = result.Relations.Assertions.UnionWith(ontologyData.Relations.Assertions);
                result.Relations.NegativeAssertions = result.Relations.NegativeAssertions.UnionWith(ontologyData.Relations.NegativeAssertions); //OWL2
                result.Relations.Member = result.Relations.Member.UnionWith(ontologyData.Relations.Member); //SKOS
                result.Relations.MemberList = result.Relations.MemberList.UnionWith(ontologyData.Relations.MemberList); //SKOS

                //Add annotations from the given data
                result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(ontologyData.Annotations.VersionInfo);
                result.Annotations.Comment = result.Annotations.Comment.UnionWith(ontologyData.Annotations.Comment);
                result.Annotations.Label = result.Annotations.Label.UnionWith(ontologyData.Annotations.Label);
                result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(ontologyData.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(ontologyData.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(ontologyData.Annotations.CustomAnnotations);
                result.Annotations.AxiomAnnotations = result.Annotations.AxiomAnnotations.UnionWith(ontologyData.Annotations.AxiomAnnotations); //OWL2
            }

            return result;
        }

        /// <summary>
        /// Builds a new difference data from this data and a given one
        /// </summary>
        public RDFOntologyData DifferenceWith(RDFOntologyData ontologyData)
        {
            RDFOntologyData result = new RDFOntologyData();

            if (ontologyData != null)
            {
                //Add difference facts
                foreach (RDFOntologyFact f in this)
                    if (!ontologyData.Facts.ContainsKey(f.PatternMemberID))
                        result.AddFact(f);

                //Add difference literals
                foreach (RDFOntologyLiteral l in this.Literals.Values)
                    if (!ontologyData.Literals.ContainsKey(l.PatternMemberID))
                        result.AddLiteral(l);

                //Add difference relations
                result.Relations.ClassType = this.Relations.ClassType.DifferenceWith(ontologyData.Relations.ClassType);
                result.Relations.SameAs = this.Relations.SameAs.DifferenceWith(ontologyData.Relations.SameAs);
                result.Relations.DifferentFrom = this.Relations.DifferentFrom.DifferenceWith(ontologyData.Relations.DifferentFrom);
                result.Relations.Assertions = this.Relations.Assertions.DifferenceWith(ontologyData.Relations.Assertions);
                result.Relations.NegativeAssertions = this.Relations.NegativeAssertions.DifferenceWith(ontologyData.Relations.NegativeAssertions); //OWL2
                result.Relations.Member = this.Relations.Member.DifferenceWith(ontologyData.Relations.Member); //SKOS
                result.Relations.MemberList = this.Relations.MemberList.DifferenceWith(ontologyData.Relations.MemberList); //SKOS

                //Add difference annotations
                result.Annotations.VersionInfo = this.Annotations.VersionInfo.DifferenceWith(ontologyData.Annotations.VersionInfo);
                result.Annotations.Comment = this.Annotations.Comment.DifferenceWith(ontologyData.Annotations.Comment);
                result.Annotations.Label = this.Annotations.Label.DifferenceWith(ontologyData.Annotations.Label);
                result.Annotations.SeeAlso = this.Annotations.SeeAlso.DifferenceWith(ontologyData.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = this.Annotations.IsDefinedBy.DifferenceWith(ontologyData.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.DifferenceWith(ontologyData.Annotations.CustomAnnotations);
                result.Annotations.AxiomAnnotations = this.Annotations.AxiomAnnotations.DifferenceWith(ontologyData.Annotations.AxiomAnnotations); //OWL2
            }
            else
            {
                //Add facts from this data
                foreach (RDFOntologyFact f in this)
                    result.AddFact(f);

                //Add literals from this data
                foreach (RDFOntologyLiteral l in this.Literals.Values)
                    result.AddLiteral(l);

                //Add relations from this data
                result.Relations.ClassType = result.Relations.ClassType.UnionWith(this.Relations.ClassType);
                result.Relations.SameAs = result.Relations.SameAs.UnionWith(this.Relations.SameAs);
                result.Relations.DifferentFrom = result.Relations.DifferentFrom.UnionWith(this.Relations.DifferentFrom);
                result.Relations.Assertions = result.Relations.Assertions.UnionWith(this.Relations.Assertions);
                result.Relations.NegativeAssertions = result.Relations.NegativeAssertions.UnionWith(this.Relations.NegativeAssertions); //OWL2
                result.Relations.Member = result.Relations.Member.UnionWith(this.Relations.Member); //SKOS
                result.Relations.MemberList = result.Relations.MemberList.UnionWith(this.Relations.MemberList); //SKOS

                //Add annotations from this data
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
        /// Gets a graph representation of this ontology data, exporting inferences according to the selected behavior
        /// </summary>
        public RDFGraph ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            RDFGraph result = new RDFGraph();

            //Relations
            result = result.UnionWith(this.Relations.SameAs.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.SameAs)))
                           .UnionWith(this.Relations.DifferentFrom.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.DifferentFrom)))
                           .UnionWith(this.Relations.ClassType.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.ClassType)))
                           .UnionWith(this.Relations.Assertions.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.Assertions)))
                           .UnionWith(this.Relations.NegativeAssertions.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.NegativeAssertions))) //OWL2
                           .UnionWith(this.Relations.Member.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.Member))) //SKOS
                           .UnionWith(this.Relations.MemberList.ReifyToRDFGraph(infexpBehavior, nameof(this.Relations.MemberList))); //SKOS

            //Annotations
            result = result.UnionWith(this.Annotations.VersionInfo.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.VersionInfo)))
                           .UnionWith(this.Annotations.Comment.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.Comment)))
                           .UnionWith(this.Annotations.Label.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.Label)))
                           .UnionWith(this.Annotations.SeeAlso.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.SeeAlso)))
                           .UnionWith(this.Annotations.IsDefinedBy.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.IsDefinedBy)))
                           .UnionWith(this.Annotations.CustomAnnotations.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.CustomAnnotations)))
                           .UnionWith(this.Annotations.AxiomAnnotations.ReifyToRDFGraph(infexpBehavior, nameof(this.Annotations.AxiomAnnotations), null, null, this)); //OWL2

            return result;
        }

        /// <summary>
        /// Asynchronously gets a graph representation of this ontology data, exporting inferences according to the selected behavior
        /// </summary>
        public Task<RDFGraph> ToRDFGraphAsync(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
            => Task.Run(() => ToRDFGraph(infexpBehavior));
        #endregion

        #endregion
    }
}