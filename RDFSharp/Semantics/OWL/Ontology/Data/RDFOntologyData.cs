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
using System;
using System.Collections;
using System.Collections.Generic;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyData represents the data component (A-BOX) of an ontology.
    /// </summary>
    public sealed class RDFOntologyData: IEnumerable<RDFOntologyFact> {

        #region Properties
        /// <summary>
        /// Count of the facts composing the data
        /// </summary>
        public Int64 FactsCount {
            get { return this.Facts.Count; }
        }

        /// <summary>
        /// Count of the literals composing the data
        /// </summary>
        public Int64 LiteralsCount {
            get { return this.Literals.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the facts of the data for iteration
        /// </summary>
        public IEnumerator<RDFOntologyFact> FactsEnumerator {
            get { return this.Facts.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Gets the enumerator on the literals of the data for iteration
        /// </summary>
        public IEnumerator<RDFOntologyLiteral> LiteralsEnumerator {
            get { return this.Literals.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Annotations describing facts of the ontology data
        /// </summary>
        public RDFOntologyAnnotations Annotations { get; internal set; }

        /// <summary>
        /// Relations describing facts of the ontology data
        /// </summary>
        public RDFOntologyDataMetadata Relations { get; internal set; }

        /// <summary>
        /// Dictionary of facts composing the data
        /// </summary>
        internal Dictionary<Int64, RDFOntologyFact> Facts { get; set; }

        /// <summary>
        /// Dictionary of literals composing the data
        /// </summary>
        internal Dictionary<Int64, RDFOntologyLiteral> Literals { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology data
        /// </summary>
        public RDFOntologyData() {
            this.Facts       = new Dictionary<Int64, RDFOntologyFact>();
            this.Literals    = new Dictionary<Int64, RDFOntologyLiteral>();
            this.Annotations = new RDFOntologyAnnotations();
            this.Relations   = new RDFOntologyDataMetadata();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the data's facts
        /// </summary>
        IEnumerator<RDFOntologyFact> IEnumerable<RDFOntologyFact>.GetEnumerator() {
            return this.FactsEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the data's facts
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.FactsEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given fact to the data
        /// </summary>
        public RDFOntologyData AddFact(RDFOntologyFact ontologyFact) {
            if (ontologyFact  != null) {
                if (!this.Facts.ContainsKey(ontologyFact.PatternMemberID)) {
                     this.Facts.Add(ontologyFact.PatternMemberID, ontologyFact);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given literal to the data
        /// </summary>
        public RDFOntologyData AddLiteral(RDFOntologyLiteral ontologyLiteral) {
            if (ontologyLiteral  != null) {
                if (!this.Literals.ContainsKey(ontologyLiteral.PatternMemberID)) {
                     this.Literals.Add(ontologyLiteral.PatternMemberID, ontologyLiteral);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given standard annotation to the given ontology fact
        /// </summary>
        public RDFOntologyData AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation,
                                                     RDFOntologyFact ontologyFact,
                                                     RDFOntologyResource annotationValue) {
            if (ontologyFact != null && annotationValue != null) {
                switch (standardAnnotation) {

                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                         if (annotationValue.IsLiteral()) {
                             this.Annotations.VersionInfo.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                         }
                         else {
                             RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Cannot annotate ontology fact with owl:versionInfo value '{0}' because it is not an ontology literal", annotationValue));
                         }
                         break;

                    //owl:versionIRI
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                         RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology fact with owl:versionIRI because it is reserved for ontologies");
                         break;

                    //rdfs:comment
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                         if (annotationValue.IsLiteral()) {
                             this.Annotations.Comment.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                         }
                         else {
                             RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Cannot annotate ontology fact with rdfs:comment value '{0}' because it is not an ontology literal", annotationValue));
                         }
                         break;

                    //rdfs:label
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                         if (annotationValue.IsLiteral()) {
                             this.Annotations.Label.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                         }
                         else {
                             RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Cannot annotate ontology fact with rdfs:label value '{0}' because it is not an ontology literal", annotationValue));
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

                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given custom annotation to the given ontology fact
        /// </summary>
        public RDFOntologyData AddCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty,
                                                   RDFOntologyFact ontologyFact,
                                                   RDFOntologyResource annotationValue) {
            if (ontologyAnnotationProperty != null && ontologyFact != null && annotationValue != null) {

                //owl:versionInfo
                if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty())) {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyFact, annotationValue);
                }

                //owl:versionIRI
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyFact, annotationValue);
                }

                //rdfs:comment
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyFact, annotationValue);
                }

                //rdfs:label
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyFact, annotationValue);
                }

                //rdfs:seeAlso
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyFact, annotationValue);
                }

                //rdfs:isDefinedBy
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyFact, annotationValue);
                }

                //owl:imports
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyFact, annotationValue);
                }

                //owl:backwardCompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyFact, annotationValue);
                }

                //owl:incompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyFact, annotationValue);
                }

                //owl:priorVersion
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyFact, annotationValue);
                }

                //custom annotation
                else {
                     this.Annotations.CustomAnnotations.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, ontologyAnnotationProperty, annotationValue));
                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyFact -> rdf:type -> ontologyClass" relation to the data.
        /// </summary>
        public RDFOntologyData AddClassTypeRelation(RDFOntologyFact ontologyFact, RDFOntologyClass ontologyClass) {
            if (ontologyFact != null && ontologyClass != null) {

                //Enforce preliminary check on usage of BASE classes
                if (!RDFOntologyChecker.CheckReservedClass(ontologyClass)) {

                     //Enforce taxonomy checks before adding the ClassType relation
                     if (RDFOntologyChecker.CheckClassTypeCompatibility(ontologyClass)) {
                         this.Relations.ClassType.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), ontologyClass));
                     }
                     else {

                         //Raise warning event to inform the user: ClassType relation cannot be added to the data because only plain classes can be explicitly assigned as class types of facts
                         RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("ClassType relation between fact '{0}' and class '{1}' cannot be added to the data because only plain classes can be explicitly assigned as class types of facts.", ontologyFact, ontologyClass));

                     }

                }
                else {

                     //Raise warning event to inform the user: ClassType relation cannot be added to the data because usage of BASE reserved classes compromises the taxonomy consistency
                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("ClassType relation between fact '{0}' and class '{1}' cannot be added to the data because because usage of BASE reserved classes compromises the taxonomy consistency.", ontologyFact, ontologyClass));

                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "aFact -> owl:sameAs -> bFact" relation to the data
        /// </summary>
        public RDFOntologyData AddSameAsRelation(RDFOntologyFact aFact, RDFOntologyFact bFact) {
            if (aFact != null && bFact != null && !aFact.Equals(bFact)) {

                //Enforce taxonomy checks before adding the SameAs relation
                if (RDFOntologyChecker.CheckSameAsCompatibility(this, aFact, bFact)) {
                    this.Relations.SameAs.AddEntry(new RDFOntologyTaxonomyEntry(aFact, RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), bFact));
                    this.Relations.SameAs.AddEntry(new RDFOntologyTaxonomyEntry(bFact, RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), aFact).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
                }
                else {

                    //Raise warning event to inform the user: SameAs relation cannot be added to the data because it violates the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SameAs relation between fact '{0}' and fact '{1}' cannot be added to the data because it violates the taxonomy consistency.", aFact, bFact));

                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "aFact -> owl:differentFrom -> bFact" relation to the data
        /// </summary>
        public RDFOntologyData AddDifferentFromRelation(RDFOntologyFact aFact, RDFOntologyFact bFact) {
            if (aFact != null && bFact != null && !aFact.Equals(bFact)) {

               //Enforce taxonomy checks before adding the DifferentFrom relation
                if (RDFOntologyChecker.CheckDifferentFromCompatibility(this, aFact, bFact)) {
                    this.Relations.DifferentFrom.AddEntry(new RDFOntologyTaxonomyEntry(aFact, RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), bFact));
                    this.Relations.DifferentFrom.AddEntry(new RDFOntologyTaxonomyEntry(bFact, RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), aFact).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
                }
                else {

                    //Raise warning event to inform the user: DifferentFrom relation cannot be added to the data because it violates the taxonomy consistency
                    RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("DifferentFrom relation between fact '{0}' and fact '{1}' cannot be added to the data because it violates the taxonomy consistency.", aFact, bFact));

                }

            }
            return this;
        }

        /// <summary>
        /// Foreach of the given list of facts, adds the "aFact -> owl:differentFrom -> bFact" relation to the data [OWL2]
        /// </summary>
        public RDFOntologyData AddAllDifferentRelation(List<RDFOntologyFact> ontologyFacts) {
            ontologyFacts?.ForEach(outerFact =>
                ontologyFacts?.ForEach(innerFact => this.AddDifferentFromRelation(innerFact, outerFact)));
            return this;
        }

        /// <summary>
        /// Adds the "aFact -> objectProperty -> bFact" relation to the data
        /// </summary>
        public RDFOntologyData AddAssertionRelation(RDFOntologyFact aFact, 
                                                    RDFOntologyObjectProperty objectProperty, 
                                                    RDFOntologyFact bFact) {
            if (aFact              != null && objectProperty != null && bFact != null) {

                //Enforce preliminary check on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(objectProperty)) {

                     //Enforce taxonomy checks before adding the assertion
                     //Creation of transitive cycles is not allowed (OWL-DL)
                     if (RDFOntologyChecker.CheckTransitiveAssertionCompatibility(this, aFact, objectProperty, bFact)) {
                         this.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(aFact, objectProperty, bFact));
                     }
                     else {

                         //Raise warning event to inform the user: Assertion relation cannot be added to the data because it violates the taxonomy transitive consistency
                         RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Assertion relation between fact '{0}' and fact '{1}' with transitive property '{2}' cannot be added to the data because it would violate the taxonomy consistency (transitive cycle detected).", aFact, bFact, objectProperty));

                     }

                }
                else {

                     //Raise warning event to inform the user: Assertion relation cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency
                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Assertion relation between fact '{0}' and fact '{1}' cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency.", aFact, bFact));

                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyFact -> datatypeProperty -> ontologyLiteral" relation to the data
        /// </summary>
        public RDFOntologyData AddAssertionRelation(RDFOntologyFact ontologyFact, 
                                                    RDFOntologyDatatypeProperty datatypeProperty, 
                                                    RDFOntologyLiteral ontologyLiteral) {
            if (ontologyFact       != null && datatypeProperty != null && ontologyLiteral != null) {

                //Enforce preliminary check on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(datatypeProperty)) {
                     this.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(ontologyFact, datatypeProperty, ontologyLiteral));
                     this.AddLiteral(ontologyLiteral);
                }
                else {

                     //Raise warning event to inform the user: Assertion relation cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency
                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("Assertion relation between fact '{0}' and literal '{1}' cannot be added to the data because usage of BASE reserved properties compromises the taxonomy consistency.", ontologyFact, ontologyLiteral));

                }

            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given fact from the data.
        /// </summary>
        public RDFOntologyData RemoveFact(RDFOntologyFact ontologyFact) {
            if (ontologyFact != null) {
                if (this.Facts.ContainsKey(ontologyFact.PatternMemberID)) {
                    this.Facts.Remove(ontologyFact.PatternMemberID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given literal from the data
        /// </summary>
        public RDFOntologyData RemoveLiteral(RDFOntologyLiteral ontologyLiteral) {
            if (ontologyLiteral != null) {
                if (this.Literals.ContainsKey(ontologyLiteral.PatternMemberID)) {
                    this.Literals.Remove(ontologyLiteral.PatternMemberID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given standard annotation from the given ontology fact
        /// </summary>
        public RDFOntologyData RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation,
                                                        RDFOntologyFact ontologyFact,
                                                        RDFOntologyResource annotationValue) {
            if (ontologyFact != null && annotationValue != null) {
                switch (standardAnnotation) {

                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                         this.Annotations.VersionInfo.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:versionIRI
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                         this.Annotations.VersionIRI.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty(), annotationValue));
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

                    //owl:priorVersion
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion:
                         this.Annotations.PriorVersion.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:imports
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports:
                         this.Annotations.Imports.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:backwardCompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith:
                         this.Annotations.BackwardCompatibleWith.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:incompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith:
                         this.Annotations.IncompatibleWith.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given custom annotation from the given ontology fact
        /// </summary>
        public RDFOntologyData RemoveCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty,
                                                      RDFOntologyFact ontologyFact,
                                                      RDFOntologyResource annotationValue) {
            if (ontologyAnnotationProperty != null && ontologyFact != null && annotationValue != null) {

                //owl:versionInfo
                if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty())) {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyFact, annotationValue);
                }

                //owl:versionIRI
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyFact, annotationValue);
                }

                //rdfs:comment
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyFact, annotationValue);
                }

                //rdfs:label
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyFact, annotationValue);
                }

                //rdfs:seeAlso
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyFact, annotationValue);
                }

                //rdfs:isDefinedBy
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyFact, annotationValue);
                }

                //owl:imports
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyFact, annotationValue);
                }

                //owl:backwardCompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyFact, annotationValue);
                }

                //owl:incompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyFact, annotationValue);
                }

                //owl:priorVersion
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyFact, annotationValue);
                }

                //custom annotation
                else {
                     this.Annotations.CustomAnnotations.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, ontologyAnnotationProperty, annotationValue));
                }

            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyFact -> rdf:type -> ontologyClass" relation from the data
        /// </summary>
        public RDFOntologyData RemoveClassTypeRelation(RDFOntologyFact ontologyFact, RDFOntologyClass ontologyClass) {
            if (ontologyFact != null && ontologyClass != null) {
                this.Relations.ClassType.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), ontologyClass));
            }
            return this;
        }

        /// <summary>
        /// Removes the "aFact -> owl:sameAs -> bFact" relation from the data
        /// </summary>
        public RDFOntologyData RemoveSameAsRelation(RDFOntologyFact aFact, RDFOntologyFact bFact) {
            if (aFact != null && bFact != null) {
                this.Relations.SameAs.RemoveEntry(new RDFOntologyTaxonomyEntry(aFact, RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), bFact));
                this.Relations.SameAs.RemoveEntry(new RDFOntologyTaxonomyEntry(bFact, RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), aFact));
            }
            return this;
        }

        /// <summary>
        /// Removes the "aFact -> owl:differentFrom -> bFact" relation from the data
        /// </summary>
        public RDFOntologyData RemoveDifferentFromRelation(RDFOntologyFact aFact, RDFOntologyFact bFact) {
            if (aFact != null && bFact != null) {
                this.Relations.DifferentFrom.RemoveEntry(new RDFOntologyTaxonomyEntry(aFact, RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), bFact));
                this.Relations.DifferentFrom.RemoveEntry(new RDFOntologyTaxonomyEntry(bFact, RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), aFact));
            }
            return this;
        }

        /// <summary>
        /// Foreach of the given list of facts, removes the "aFact -> owl:differentFrom -> bFact" relation from the data [OWL2]
        /// </summary>
        public RDFOntologyData RemoveAllDifferentRelation(List<RDFOntologyFact> ontologyFacts) {
            ontologyFacts?.ForEach(outerFact => {
                ontologyFacts?.ForEach(innerFact => {
                    this.RemoveDifferentFromRelation(innerFact, outerFact);
                });
            });
            return this;
        }

        /// <summary>
        /// Removes the "aFact -> objectProperty -> bFact" relation from the data
        /// </summary>
        public RDFOntologyData RemoveAssertionRelation(RDFOntologyFact aFact, 
                                                       RDFOntologyObjectProperty objectProperty, 
                                                       RDFOntologyFact bFact) {
            if (aFact != null && objectProperty != null && bFact != null) {
                this.Relations.Assertions.RemoveEntry(new RDFOntologyTaxonomyEntry(aFact, objectProperty, bFact));
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyFact -> datatypeProperty -> ontologyLiteral" relation from the data
        /// </summary>
        public RDFOntologyData RemoveAssertionRelation(RDFOntologyFact ontologyFact, 
                                                       RDFOntologyDatatypeProperty datatypeProperty, 
                                                       RDFOntologyLiteral ontologyLiteral) {
            if (ontologyFact != null && datatypeProperty != null && ontologyLiteral != null) {
                this.Relations.Assertions.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyFact, datatypeProperty, ontologyLiteral));
            }
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the fact represented by the given string from the data
        /// </summary>
        public RDFOntologyFact SelectFact(String fact) {
            if (fact         != null) {
                Int64 factID  = RDFModelUtilities.CreateHash(fact);
                if (this.Facts.ContainsKey(factID)) {
                    return this.Facts[factID];
                }
            }
            return null;
        }

        /// <summary>
        /// Selects the literal represented by the given string from the data
        /// </summary>
        public RDFOntologyLiteral SelectLiteral(String literal) {
            if (literal         != null) {
                Int64 literalID  = RDFModelUtilities.CreateHash(literal);
                if (this.Literals.ContainsKey(literalID)) {
                    return this.Literals[literalID];
                }
            }
            return null;
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection data from this data and a given one
        /// </summary>
        public RDFOntologyData IntersectWith(RDFOntologyData ontologyData) {
            var result        = new RDFOntologyData();
            if (ontologyData != null) {

                //Add intersection facts
                foreach (var  f in this) {
                    if  (ontologyData.Facts.ContainsKey(f.PatternMemberID)) {
                         result.AddFact(f);
                    }
                }

                //Add intersection literals
                foreach (var  l in this.Literals.Values) {
                    if  (ontologyData.Literals.ContainsKey(l.PatternMemberID)) {
                         result.AddLiteral(l);
                    }
                }

                //Add intersection relations
                result.Relations.ClassType = this.Relations.ClassType.IntersectWith(ontologyData.Relations.ClassType);
                result.Relations.SameAs = this.Relations.SameAs.IntersectWith(ontologyData.Relations.SameAs);
                result.Relations.DifferentFrom = this.Relations.DifferentFrom.IntersectWith(ontologyData.Relations.DifferentFrom);
                result.Relations.Assertions = this.Relations.Assertions.IntersectWith(ontologyData.Relations.Assertions);
                result.Relations.NegativeAssertions = this.Relations.NegativeAssertions.IntersectWith(ontologyData.Relations.NegativeAssertions);

                //Add intersection annotations
                result.Annotations.VersionInfo = this.Annotations.VersionInfo.IntersectWith(ontologyData.Annotations.VersionInfo);
                result.Annotations.Comment = this.Annotations.Comment.IntersectWith(ontologyData.Annotations.Comment);
                result.Annotations.Label = this.Annotations.Label.IntersectWith(ontologyData.Annotations.Label);
                result.Annotations.SeeAlso = this.Annotations.SeeAlso.IntersectWith(ontologyData.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = this.Annotations.IsDefinedBy.IntersectWith(ontologyData.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.IntersectWith(ontologyData.Annotations.CustomAnnotations);

            }
            return result;
        }

        /// <summary>
        /// Builds a new union data from this data and a given one
        /// </summary>
        public RDFOntologyData UnionWith(RDFOntologyData ontologyData) {
            var result   = new RDFOntologyData();

            //Add facts from this data
            foreach (var f in this) {
                result.AddFact(f);
            }

            //Add literals from this data
            foreach (var l in this.Literals.Values) {
                result.AddLiteral(l);
            }

            //Add relations from this data
            result.Relations.ClassType = result.Relations.ClassType.UnionWith(this.Relations.ClassType);
            result.Relations.SameAs = result.Relations.SameAs.UnionWith(this.Relations.SameAs);
            result.Relations.DifferentFrom = result.Relations.DifferentFrom.UnionWith(this.Relations.DifferentFrom);
            result.Relations.Assertions = result.Relations.Assertions.UnionWith(this.Relations.Assertions);
            result.Relations.NegativeAssertions = result.Relations.NegativeAssertions.UnionWith(this.Relations.NegativeAssertions);

            //Add annotations from this data
            result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
            result.Annotations.Comment = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
            result.Annotations.Label = result.Annotations.Label.UnionWith(this.Annotations.Label);
            result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
            result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
            result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);

            //Manage the given data
            if (ontologyData != null) {

                //Add facts from the given data
                foreach (var  f in ontologyData) {
                    result.AddFact(f);
                }

                //Add literals from the given data
                foreach (var  l in ontologyData.Literals.Values) {
                    result.AddLiteral(l);
                }

                //Add relations from the given data
                result.Relations.ClassType = result.Relations.ClassType.UnionWith(ontologyData.Relations.ClassType);
                result.Relations.SameAs = result.Relations.SameAs.UnionWith(ontologyData.Relations.SameAs);
                result.Relations.DifferentFrom = result.Relations.DifferentFrom.UnionWith(ontologyData.Relations.DifferentFrom);
                result.Relations.Assertions = result.Relations.Assertions.UnionWith(ontologyData.Relations.Assertions);
                result.Relations.NegativeAssertions = result.Relations.NegativeAssertions.UnionWith(ontologyData.Relations.NegativeAssertions);

                //Add annotations from the given data
                result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(ontologyData.Annotations.VersionInfo);
                result.Annotations.Comment = result.Annotations.Comment.UnionWith(ontologyData.Annotations.Comment);
                result.Annotations.Label = result.Annotations.Label.UnionWith(ontologyData.Annotations.Label);
                result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(ontologyData.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(ontologyData.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(ontologyData.Annotations.CustomAnnotations);

            }
            return result;
        }

        /// <summary>
        /// Builds a new difference data from this data and a given one
        /// </summary>
        public RDFOntologyData DifferenceWith(RDFOntologyData ontologyData) {
            var result        = new RDFOntologyData();
            if (ontologyData != null) {

                //Add difference facts
                foreach (var  f in this) {
                    if  (!ontologyData.Facts.ContainsKey(f.PatternMemberID)) {
                          result.AddFact(f);
                    }
                }

                //Add difference literals
                foreach (var  l in this.Literals.Values) {
                    if  (!ontologyData.Literals.ContainsKey(l.PatternMemberID)) {
                          result.AddLiteral(l);
                    }
                }

                //Add difference relations
                result.Relations.ClassType = this.Relations.ClassType.DifferenceWith(ontologyData.Relations.ClassType);
                result.Relations.SameAs = this.Relations.SameAs.DifferenceWith(ontologyData.Relations.SameAs);
                result.Relations.DifferentFrom = this.Relations.DifferentFrom.DifferenceWith(ontologyData.Relations.DifferentFrom);
                result.Relations.Assertions = this.Relations.Assertions.DifferenceWith(ontologyData.Relations.Assertions);
                result.Relations.NegativeAssertions = this.Relations.NegativeAssertions.DifferenceWith(ontologyData.Relations.NegativeAssertions);

                //Add difference annotations
                result.Annotations.VersionInfo = this.Annotations.VersionInfo.DifferenceWith(ontologyData.Annotations.VersionInfo);
                result.Annotations.Comment = this.Annotations.Comment.DifferenceWith(ontologyData.Annotations.Comment);
                result.Annotations.Label = this.Annotations.Label.DifferenceWith(ontologyData.Annotations.Label);
                result.Annotations.SeeAlso = this.Annotations.SeeAlso.DifferenceWith(ontologyData.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = this.Annotations.IsDefinedBy.DifferenceWith(ontologyData.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.DifferenceWith(ontologyData.Annotations.CustomAnnotations);

            }
            else {

                //Add facts from this data
                foreach (var f in this) {
                    result.AddFact(f);
                }

                //Add literals from this data
                foreach (var l in this.Literals.Values) {
                    result.AddLiteral(l);
                }

                //Add relations from this data
                result.Relations.ClassType = result.Relations.ClassType.UnionWith(this.Relations.ClassType);
                result.Relations.SameAs = result.Relations.SameAs.UnionWith(this.Relations.SameAs);
                result.Relations.DifferentFrom = result.Relations.DifferentFrom.UnionWith(this.Relations.DifferentFrom);
                result.Relations.Assertions = result.Relations.Assertions.UnionWith(this.Relations.Assertions);
                result.Relations.NegativeAssertions = result.Relations.NegativeAssertions.UnionWith(this.Relations.NegativeAssertions);

                //Add annotations from this data
                result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
                result.Annotations.Comment = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
                result.Annotations.Label = result.Annotations.Label.UnionWith(this.Annotations.Label);
                result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);

            }
            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this ontology data, exporting inferences according to the selected behavior
        /// </summary>
        public RDFGraph ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior) {
            var result = new RDFGraph();

            //Relations
            result = result.UnionWith(this.Relations.SameAs.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Relations.DifferentFrom.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Relations.ClassType.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Relations.Assertions.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Relations.NegativeAssertions.ReifyToRDFGraph(infexpBehavior));

            //Annotations
            result = result.UnionWith(this.Annotations.VersionInfo.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Annotations.Comment.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Annotations.Label.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Annotations.SeeAlso.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Annotations.IsDefinedBy.ToRDFGraph(infexpBehavior))
                           .UnionWith(this.Annotations.CustomAnnotations.ToRDFGraph(infexpBehavior));

            return result;        
        }
        #endregion

        #endregion

    }

}