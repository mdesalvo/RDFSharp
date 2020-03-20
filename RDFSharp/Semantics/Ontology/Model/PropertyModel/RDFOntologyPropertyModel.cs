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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFOntologyPropertyModel represents the property-oriented model component (T-BOX) of an ontology.
    /// </summary>
    public sealed class RDFOntologyPropertyModel: IEnumerable<RDFOntologyProperty> {

        #region Properties
        /// <summary>
        /// Count of the properties composing the property model
        /// </summary>
        public Int64 PropertiesCount {
            get { return this.Properties.Count; }
        }

        /// <summary>
        /// Count of the annotation properties composing the property model
        /// </summary>
        public Int64 AnnotationPropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsAnnotationProperty()); }
        }

        /// <summary>
        /// Count of the datatype properties composing the property model
        /// </summary>
        public Int64 DatatypePropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsDatatypeProperty()); }
        }

        /// <summary>
        /// Count of the object properties composing the property model
        /// </summary>
        public Int64 ObjectPropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsObjectProperty()); }
        }

        /// <summary>
        /// Count of the functional properties composing the property model
        /// </summary>
        public Int64 FunctionalPropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsFunctionalProperty()); }
        }

        /// <summary>
        /// Count of the symmetric properties composing the property model
        /// </summary>
        public Int64 SymmetricPropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsSymmetricProperty()); }
        }

        /// <summary>
        /// Count of the transitive properties composing the property model
        /// </summary>
        public Int64 TransitivePropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsTransitiveProperty()); }
        }

        /// <summary>
        /// Count of the inverse functional properties composing the property model
        /// </summary>
        public Int64 InverseFunctionalPropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsInverseFunctionalProperty()); }
        }

        /// <summary>
        /// Gets the enumerator on the property model's properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyProperty> PropertiesEnumerator {
            get { return this.Properties.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Gets the enumerator on the property model's annotation properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyAnnotationProperty> AnnotationPropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsAnnotationProperty())
                                             .OfType<RDFOntologyAnnotationProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's datatype properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyDatatypeProperty> DatatypePropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsDatatypeProperty())
                                             .OfType<RDFOntologyDatatypeProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's object properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyObjectProperty> ObjectPropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsObjectProperty())
                                             .OfType<RDFOntologyObjectProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's functional properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyProperty> FunctionalPropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsFunctionalProperty())
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's symmetric properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyObjectProperty> SymmetricPropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsSymmetricProperty())
                                             .OfType<RDFOntologyObjectProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's transitive properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyObjectProperty> TransitivePropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsTransitiveProperty())
                                             .OfType<RDFOntologyObjectProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's inverse functional properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyObjectProperty> InverseFunctionalPropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsInverseFunctionalProperty())
                                             .OfType<RDFOntologyObjectProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Annotations describing properties of the ontology property model
        /// </summary>
        public RDFOntologyAnnotations Annotations { get; internal set; }

        /// <summary>
        /// Relations describing properties of the ontology property model
        /// </summary>
        public RDFOntologyPropertyModelMetadata Relations { get; internal set; }

        /// <summary>
        /// Dictionary of properties composing the property model
        /// </summary>
        internal Dictionary<Int64, RDFOntologyProperty> Properties { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty property model
        /// </summary>
        public RDFOntologyPropertyModel() {
            this.Properties  = new Dictionary<Int64, RDFOntologyProperty>();
            this.Annotations = new RDFOntologyAnnotations();
            this.Relations   = new RDFOntologyPropertyModelMetadata();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the property model's properties
        /// </summary>
        IEnumerator<RDFOntologyProperty> IEnumerable<RDFOntologyProperty>.GetEnumerator() {
            return this.PropertiesEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the property model's properties
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.PropertiesEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given property to the ontology property model
        /// </summary>
        public RDFOntologyPropertyModel AddProperty(RDFOntologyProperty ontologyProperty) {
            if (ontologyProperty != null) {
                if (!this.Properties.ContainsKey(ontologyProperty.PatternMemberID)) {
                     this.Properties.Add(ontologyProperty.PatternMemberID, ontologyProperty);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given standard annotation to the given ontology property
        /// </summary>
        public RDFOntologyPropertyModel AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation,
                                                              RDFOntologyProperty ontologyProperty,
                                                              RDFOntologyResource annotationValue) {
            if (ontologyProperty != null && annotationValue != null) {
                switch (standardAnnotation) {

                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                         if (annotationValue.IsLiteral()) {
                             this.Annotations.VersionInfo.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                         }
                         else {
                             RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Cannot annotate ontology property with owl:versionInfo value '{0}' because it is not an ontology literal", annotationValue));
                         }
                         break;

                    //owl:versionIRI
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                         RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology property with owl:versionIRI because it is reserved for ontologies");
                         break;

                    //rdfs:comment
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                         if (annotationValue.IsLiteral()) {
                             this.Annotations.Comment.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                         }
                         else {
                             RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Cannot annotate ontology property with rdfs:comment value '{0}' because it is not an ontology literal", annotationValue));
                         }
                         break;

                    //rdfs:label
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                         if (annotationValue.IsLiteral()) {
                             this.Annotations.Label.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                         }
                         else {
                             RDFSemanticsEvents.RaiseSemanticsInfo(String.Format("Cannot annotate ontology property with rdfs:label value '{0}' because it is not an ontology literal", annotationValue));
                         }
                         break;

                    //rdfs:seeAlso
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso:
                         this.Annotations.SeeAlso.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //rdfs:isDefinedBy
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy:
                         this.Annotations.IsDefinedBy.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:priorVersion
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion:
                         RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology property with owl:priorVersion because it is reserved for ontologies");
                         break;

                    //owl:imports
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports:
                         RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology property with owl:imports because it is reserved for ontologies");
                         break;

                    //owl:backwardCompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith:
                         RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology property with owl:backwardCompatibleWith because it is reserved for ontologies");
                         break;

                    //owl:incompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith:
                         RDFSemanticsEvents.RaiseSemanticsInfo("Cannot annotate ontology property with owl:incompatibleWith because it is reserved for ontologies");
                         break;

                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given custom annotation to the given ontology property
        /// </summary>
        public RDFOntologyPropertyModel AddCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty,
                                                            RDFOntologyProperty ontologyProperty,
                                                            RDFOntologyResource annotationValue) {
            if (ontologyAnnotationProperty != null && ontologyProperty != null && annotationValue != null) {

                //owl:versionInfo
                if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty())) {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyProperty, annotationValue);
                }

                //owl:versionIRI
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyProperty, annotationValue);
                }

                //rdfs:comment
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyProperty, annotationValue);
                }

                //rdfs:label
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyProperty, annotationValue);
                }

                //rdfs:seeAlso
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyProperty, annotationValue);
                }

                //rdfs:isDefinedBy
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyProperty, annotationValue);
                }

                //owl:imports
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyProperty, annotationValue);
                }

                //owl:backwardCompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyProperty, annotationValue);
                }

                //owl:incompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyProperty, annotationValue);
                }

                //owl:priorVersion
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty())) {
                     this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyProperty, annotationValue);
                }

                //custom annotation
                else {
                     this.Annotations.CustomAnnotations.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, ontologyAnnotationProperty, annotationValue));
                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "childProperty -> rdfs:subPropertyOf -> motherProperty" relation to the property model.
        /// </summary>
        public RDFOntologyPropertyModel AddSubPropertyOfRelation(RDFOntologyObjectProperty childProperty, 
                                                                 RDFOntologyObjectProperty motherProperty) {
            if (childProperty != null && motherProperty != null && !childProperty.Equals(motherProperty)) {

                //Enforce preliminary checks on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(childProperty) && !RDFOntologyChecker.CheckReservedProperty(motherProperty)) {

                     //Enforce taxonomy checks before adding the subPropertyOf relation
                     if (RDFOntologyChecker.CheckSubPropertyOfCompatibility(this, childProperty, motherProperty)) {
                         this.Relations.SubPropertyOf.AddEntry(new RDFOntologyTaxonomyEntry(childProperty, RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty(), motherProperty));
                     }
                     else {

                         //Raise warning event to inform the user: SubPropertyOf relation cannot be added to the property model because it violates the taxonomy consistency
                         RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SubPropertyOf relation between child property '{0}' and mother property '{1}' cannot be added to the property model because it violates the taxonomy consistency.", childProperty, motherProperty));

                     }

                }
                else {

                     //Raise warning event to inform the user: SubPropertyOf relation cannot be added to the property model because it violates the taxonomy consistency
                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SubPropertyOf relation between child property '{0}' and mother property '{1}' cannot be added to the property model because usage of BASE reserved properties compromises the taxonomy consistency.", childProperty, motherProperty));

                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "childProperty -> rdfs:subPropertyOf -> motherProperty" relation to the property model.
        /// </summary>
        public RDFOntologyPropertyModel AddSubPropertyOfRelation(RDFOntologyDatatypeProperty childProperty, 
                                                                 RDFOntologyDatatypeProperty motherProperty) {
            if (childProperty != null && motherProperty != null && !childProperty.Equals(motherProperty)) {

                //Enforce preliminary checks on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(childProperty) && !RDFOntologyChecker.CheckReservedProperty(motherProperty)) {

                    //Enforce taxonomy checks before adding the subPropertyOf relation
                    if (RDFOntologyChecker.CheckSubPropertyOfCompatibility(this, childProperty, motherProperty)) {
                        this.Relations.SubPropertyOf.AddEntry(new RDFOntologyTaxonomyEntry(childProperty, RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty(), motherProperty));
                     }
                     else {

                        //Raise warning event to inform the user: SubPropertyOf relation cannot be added to the property model because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SubPropertyOf relation between child property '{0}' and mother property '{1}' cannot be added to the property model because it violates the taxonomy consistency.", childProperty, motherProperty));

                     }

                }
                else {

                     //Raise warning event to inform the user: SubPropertyOf relation cannot be added to the property model because it violates the taxonomy consistency
                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("SubPropertyOf relation between child property '{0}' and mother property '{1}' cannot be added to the property model because usage of BASE reserved properties compromises the taxonomy consistency.", childProperty, motherProperty));

                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "aProperty -> owl:equivalentProperty -> bProperty" relation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddEquivalentPropertyRelation(RDFOntologyObjectProperty aProperty, 
                                                                      RDFOntologyObjectProperty bProperty) {
            if (aProperty  != null && bProperty != null && !aProperty.Equals(bProperty)) {

                //Enforce preliminary checks on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(aProperty) && !RDFOntologyChecker.CheckReservedProperty(bProperty)) {

                    //Enforce taxonomy checks before adding the equivalentProperty relation
                    if (RDFOntologyChecker.CheckEquivalentPropertyCompatibility(this, aProperty, bProperty)) {
                        this.Relations.EquivalentProperty.AddEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty(), bProperty));
                        this.Relations.EquivalentProperty.AddEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty(), aProperty).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
                     }
                     else {

                        //Raise warning event to inform the user: EquivalentProperty relation cannot be added to the property model because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("EquivalentProperty relation between property '{0}' and property '{1}' cannot be added to the property model because it violates the taxonomy consistency.", aProperty, bProperty));

                     }

                }
                else {

                     //Raise warning event to inform the user: EquivalentProperty relation cannot be added to the property model because it violates the taxonomy consistency
                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("EquivalentProperty relation between property '{0}' and property '{1}' cannot be added to the property model because usage of BASE reserved properties compromises the taxonomy consistency.", aProperty, bProperty));

                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "aProperty -> owl:equivalentProperty -> bProperty" relation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddEquivalentPropertyRelation(RDFOntologyDatatypeProperty aProperty, 
                                                                      RDFOntologyDatatypeProperty bProperty) {
            if (aProperty  != null && bProperty != null && !aProperty.Equals(bProperty)) {

                //Enforce preliminary checks on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(aProperty) && !RDFOntologyChecker.CheckReservedProperty(bProperty)) {

                    //Enforce taxonomy checks before adding the equivalentProperty relation
                    if (RDFOntologyChecker.CheckEquivalentPropertyCompatibility(this, aProperty, bProperty)) {
                        this.Relations.EquivalentProperty.AddEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty(), bProperty));
                        this.Relations.EquivalentProperty.AddEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty(), aProperty).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
                     }
                     else {

                        //Raise warning event to inform the user: EquivalentProperty relation cannot be added to the property model because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("EquivalentProperty relation between property '{0}' and property '{1}' cannot be added to the property model because it violates the taxonomy consistency.", aProperty, bProperty));

                     }

                }
                else {

                     //Raise warning event to inform the user: EquivalentProperty relation cannot be added to the property model because it violates the taxonomy consistency
                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("EquivalentProperty relation between property '{0}' and property '{1}' cannot be added to the property model because usage of BASE reserved properties compromises the taxonomy consistency.", aProperty, bProperty));

                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "aProperty -> owl:inverseOf -> bProperty" relation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddInverseOfRelation(RDFOntologyObjectProperty aProperty, 
                                                             RDFOntologyObjectProperty bProperty) {
            if (aProperty != null && bProperty != null && !aProperty.Equals(bProperty)) {

                //Enforce preliminary checks on usage of BASE properties
                if (!RDFOntologyChecker.CheckReservedProperty(aProperty) && !RDFOntologyChecker.CheckReservedProperty(bProperty)) {

                    //Enforce taxonomy checks before adding the inverseOf relation
                    if (RDFOntologyChecker.CheckInverseOfPropertyCompatibility(this, aProperty, bProperty)) {
                        this.Relations.InverseOf.AddEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFVocabulary.OWL.INVERSE_OF.ToRDFOntologyObjectProperty(), bProperty));
                        this.Relations.InverseOf.AddEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFVocabulary.OWL.INVERSE_OF.ToRDFOntologyObjectProperty(), aProperty).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
                     }
                     else {

                        //Raise warning event to inform the user: InverseOf relation cannot be added to the property model because it violates the taxonomy consistency
                        RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("InverseOf relation between property '{0}' and property '{1}' cannot be added to the property model because it violates the taxonomy consistency.", aProperty, bProperty));

                     }

                }
                else {

                     //Raise warning event to inform the user: InverseOf relation cannot be added to the property model because it violates the taxonomy consistency
                     RDFSemanticsEvents.RaiseSemanticsWarning(String.Format("InverseOf relation between property '{0}' and property '{1}' cannot be added to the property model because usage of BASE reserved properties compromises the taxonomy consistency.", aProperty, bProperty));

                }

            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given property from the ontology property model
        /// </summary>
        public RDFOntologyPropertyModel RemoveProperty(RDFOntologyProperty ontologyProperty) {
            if (ontologyProperty != null) {
                if (this.Properties.ContainsKey(ontologyProperty.PatternMemberID)) {
                    this.Properties.Remove(ontologyProperty.PatternMemberID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given standard annotation from the given ontology property
        /// </summary>
        public RDFOntologyPropertyModel RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation,
                                                                 RDFOntologyProperty ontologyProperty,
                                                                 RDFOntologyResource annotationValue) {
            if (ontologyProperty != null && annotationValue != null) {
                switch (standardAnnotation) {

                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                         this.Annotations.VersionInfo.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:versionIRI
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                         this.Annotations.VersionIRI.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //rdfs:comment
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                         this.Annotations.Comment.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //rdfs:label
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                         this.Annotations.Label.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //rdfs:seeAlso
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso:
                         this.Annotations.SeeAlso.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //rdfs:isDefinedBy
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy:
                         this.Annotations.IsDefinedBy.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:priorVersion
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion:
                         this.Annotations.PriorVersion.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:imports
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports:
                         this.Annotations.Imports.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:backwardCompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith:
                         this.Annotations.BackwardCompatibleWith.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                    //owl:incompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith:
                         this.Annotations.IncompatibleWith.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                         break;

                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given custom annotation from the given ontology property
        /// </summary>
        public RDFOntologyPropertyModel RemoveCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty,
                                                               RDFOntologyProperty ontologyProperty,
                                                               RDFOntologyResource annotationValue) {
            if (ontologyAnnotationProperty != null && ontologyProperty != null && annotationValue != null) {

                //owl:versionInfo
                if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty())) {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyProperty, annotationValue);
                }

                //owl:versionIRI
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyProperty, annotationValue);
                }

                //rdfs:comment
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyProperty, annotationValue);
                }

                //rdfs:label
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyProperty, annotationValue);
                }

                //rdfs:seeAlso
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyProperty, annotationValue);
                }

                //rdfs:isDefinedBy
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyProperty, annotationValue);
                }

                //owl:imports
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyProperty, annotationValue);
                }

                //owl:backwardCompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyProperty, annotationValue);
                }

                //owl:incompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyProperty, annotationValue);
                }

                //owl:priorVersion
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty())) {
                     this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyProperty, annotationValue);
                }

                //custom annotation
                else {
                     this.Annotations.CustomAnnotations.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, ontologyAnnotationProperty, annotationValue));
                }

            }
            return this;
        }

        /// <summary>
        /// Removes the "childProperty -> rdfs:subPropertyOf -> motherProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveSubPropertyOfRelation(RDFOntologyObjectProperty childProperty, 
                                                                    RDFOntologyObjectProperty motherProperty) {
            if (childProperty != null && motherProperty != null) {
                this.Relations.SubPropertyOf.RemoveEntry(new RDFOntologyTaxonomyEntry(childProperty, RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty(), motherProperty));
            }
            return this;
        }

        /// <summary>
        /// Removes the "childProperty -> rdfs:subPropertyOf -> motherProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveSubPropertyOfRelation(RDFOntologyDatatypeProperty childProperty, 
                                                                    RDFOntologyDatatypeProperty motherProperty) {
            if (childProperty != null && motherProperty != null) {
                this.Relations.SubPropertyOf.RemoveEntry(new RDFOntologyTaxonomyEntry(childProperty, RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty(), motherProperty));
            }
            return this;
        }

        /// <summary>
        /// Removes the "aProperty -> owl:equivalentProperty -> bProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveEquivalentPropertyRelation(RDFOntologyObjectProperty aProperty, 
                                                                         RDFOntologyObjectProperty bProperty) {
            if (aProperty != null && bProperty != null) {
                this.Relations.EquivalentProperty.RemoveEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty(), bProperty));
                this.Relations.EquivalentProperty.RemoveEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty(), aProperty));
            }
            return this;
        }

        /// <summary>
        /// Removes the "aProperty -> owl:equivalentProperty -> bProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveEquivalentPropertyRelation(RDFOntologyDatatypeProperty aProperty, 
                                                                         RDFOntologyDatatypeProperty bProperty) {
            if (aProperty != null && bProperty != null) {
                this.Relations.EquivalentProperty.RemoveEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty(), bProperty));
                this.Relations.EquivalentProperty.RemoveEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty(), aProperty));
            }
            return this;
        }

        /// <summary>
        /// Removes the "aProperty -> owl:inverseOf -> bProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveInverseOfRelation(RDFOntologyObjectProperty aProperty, 
                                                                RDFOntologyObjectProperty bProperty) {
            if (aProperty != null && bProperty != null) {
                this.Relations.InverseOf.RemoveEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFVocabulary.OWL.INVERSE_OF.ToRDFOntologyObjectProperty(), bProperty));
                this.Relations.InverseOf.RemoveEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFVocabulary.OWL.INVERSE_OF.ToRDFOntologyObjectProperty(), aProperty));
            }
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the ontology property represented by the given string from the ontology property model
        /// </summary>
        public RDFOntologyProperty SelectProperty(String ontProperty) {
            if (ontProperty       != null) {
                Int64 propertyID   = RDFModelUtilities.CreateHash(ontProperty);
                if (this.Properties.ContainsKey(propertyID)) {
                    return this.Properties[propertyID];
                }
            }
            return null;
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection property model from this property model and a given one
        /// </summary>
        public RDFOntologyPropertyModel IntersectWith(RDFOntologyPropertyModel propertyModel) {
            var result          = new RDFOntologyPropertyModel();
            if (propertyModel  != null) {

                //Add intersection properties
                foreach (var p in this) {
                    if  (propertyModel.Properties.ContainsKey(p.PatternMemberID)) {
                         result.AddProperty(p);
                    }
                }

                //Add intersection relations
                result.Relations.SubPropertyOf       = this.Relations.SubPropertyOf.IntersectWith(propertyModel.Relations.SubPropertyOf);
                result.Relations.EquivalentProperty  = this.Relations.EquivalentProperty.IntersectWith(propertyModel.Relations.EquivalentProperty);
                result.Relations.InverseOf           = this.Relations.InverseOf.IntersectWith(propertyModel.Relations.InverseOf);

                //Add intersection annotations
                result.Annotations.VersionInfo       = this.Annotations.VersionInfo.IntersectWith(propertyModel.Annotations.VersionInfo);
                result.Annotations.Comment           = this.Annotations.Comment.IntersectWith(propertyModel.Annotations.Comment);
                result.Annotations.Label             = this.Annotations.Label.IntersectWith(propertyModel.Annotations.Label);
                result.Annotations.SeeAlso           = this.Annotations.SeeAlso.IntersectWith(propertyModel.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy       = this.Annotations.IsDefinedBy.IntersectWith(propertyModel.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.IntersectWith(propertyModel.Annotations.CustomAnnotations);

            }
            return result;
        }

        /// <summary>
        /// Builds a new union property model from this property model and a given one
        /// </summary>
        public RDFOntologyPropertyModel UnionWith(RDFOntologyPropertyModel propertyModel) {
            var result   = new RDFOntologyPropertyModel();

            //Add properties from this property model
            foreach (var p in this) {
                result.AddProperty(p);
            }

            //Add relations from this property model
            result.Relations.SubPropertyOf       = result.Relations.SubPropertyOf.UnionWith(this.Relations.SubPropertyOf);
            result.Relations.EquivalentProperty  = result.Relations.EquivalentProperty.UnionWith(this.Relations.EquivalentProperty);
            result.Relations.InverseOf           = result.Relations.InverseOf.UnionWith(this.Relations.InverseOf);

            //Add annotations from this property model
            result.Annotations.VersionInfo       = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
            result.Annotations.Comment           = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
            result.Annotations.Label             = result.Annotations.Label.UnionWith(this.Annotations.Label);
            result.Annotations.SeeAlso           = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
            result.Annotations.IsDefinedBy       = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
            result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);

            //Manage the given property model
            if (propertyModel  != null) {

                //Add properties from the given property model
                foreach (var p in propertyModel) {
                    result.AddProperty(p);
                }

                //Add relations from the given property model
                result.Relations.SubPropertyOf       = result.Relations.SubPropertyOf.UnionWith(propertyModel.Relations.SubPropertyOf);
                result.Relations.EquivalentProperty  = result.Relations.EquivalentProperty.UnionWith(propertyModel.Relations.EquivalentProperty);
                result.Relations.InverseOf           = result.Relations.InverseOf.UnionWith(propertyModel.Relations.InverseOf);

                //Add annotations from the given property model
                result.Annotations.VersionInfo       = result.Annotations.VersionInfo.UnionWith(propertyModel.Annotations.VersionInfo);
                result.Annotations.Comment           = result.Annotations.Comment.UnionWith(propertyModel.Annotations.Comment);
                result.Annotations.Label             = result.Annotations.Label.UnionWith(propertyModel.Annotations.Label);
                result.Annotations.SeeAlso           = result.Annotations.SeeAlso.UnionWith(propertyModel.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy       = result.Annotations.IsDefinedBy.UnionWith(propertyModel.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(propertyModel.Annotations.CustomAnnotations);

            }
            return result;
        }

        /// <summary>
        /// Builds a new difference property model from this property model and a given one
        /// </summary>
        public RDFOntologyPropertyModel DifferenceWith(RDFOntologyPropertyModel propertyModel) {
            var result          = new RDFOntologyPropertyModel();
            if (propertyModel  != null) {

                //Add difference properties
                foreach (var p in this) {
                    if  (!propertyModel.Properties.ContainsKey(p.PatternMemberID)) {
                          result.AddProperty(p);
                    }
                }

                //Add difference relations
                result.Relations.SubPropertyOf       = this.Relations.SubPropertyOf.DifferenceWith(propertyModel.Relations.SubPropertyOf);
                result.Relations.EquivalentProperty  = this.Relations.EquivalentProperty.DifferenceWith(propertyModel.Relations.EquivalentProperty);
                result.Relations.InverseOf           = this.Relations.InverseOf.DifferenceWith(propertyModel.Relations.InverseOf);

                //Add difference annotations
                result.Annotations.VersionInfo       = this.Annotations.VersionInfo.DifferenceWith(propertyModel.Annotations.VersionInfo);
                result.Annotations.Comment           = this.Annotations.Comment.DifferenceWith(propertyModel.Annotations.Comment);
                result.Annotations.Label             = this.Annotations.Label.DifferenceWith(propertyModel.Annotations.Label);
                result.Annotations.SeeAlso           = this.Annotations.SeeAlso.DifferenceWith(propertyModel.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy       = this.Annotations.IsDefinedBy.DifferenceWith(propertyModel.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.DifferenceWith(propertyModel.Annotations.CustomAnnotations);

            }
            else {

                //Add properties from this property model
                foreach (var p in this) {
                    result.AddProperty(p);
                }

                //Add relations from this property model
                result.Relations.SubPropertyOf       = result.Relations.SubPropertyOf.UnionWith(this.Relations.SubPropertyOf);
                result.Relations.EquivalentProperty  = result.Relations.EquivalentProperty.UnionWith(this.Relations.EquivalentProperty);
                result.Relations.InverseOf           = result.Relations.InverseOf.UnionWith(this.Relations.InverseOf);

                //Add annotations from this property model
                result.Annotations.VersionInfo       = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
                result.Annotations.Comment           = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
                result.Annotations.Label             = result.Annotations.Label.UnionWith(this.Annotations.Label);
                result.Annotations.SeeAlso           = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy       = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);

            }
            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this ontology property model, exporting inferences according to the selected behavior
        /// </summary>
        public RDFGraph ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior) {
            var result      = new RDFGraph();

            //Definitions
            foreach (var p in this.Where(prop => !RDFOntologyChecker.CheckReservedProperty(prop))) {
                if  (p.IsAnnotationProperty()) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ANNOTATION_PROPERTY));
                }
                else if (p.IsDatatypeProperty()) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DATATYPE_PROPERTY));
                }
                else if (p.IsObjectProperty()) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.OBJECT_PROPERTY));
                     if (p.IsSymmetricProperty()) {
                         result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.SYMMETRIC_PROPERTY));
                     }
                     if (p.IsTransitiveProperty()) {
                         result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.TRANSITIVE_PROPERTY));
                     }
                     if (p.IsInverseFunctionalProperty()) {
                         result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY));
                     }
                }
                else {
                    result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.PROPERTY));
                }

                if  (p.IsFunctionalProperty()) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY));
                }
                if  (p.IsDeprecatedProperty()) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY));
                }
                if  (p.Domain != null) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDFS.DOMAIN, (RDFResource)p.Domain.Value));
                }
                if  (p.Range  != null) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDFS.RANGE,  (RDFResource)p.Range.Value));
                }
            }

            //Relations
            result       = result.UnionWith(this.Relations.SubPropertyOf.ToRDFGraph(infexpBehavior))
                                 .UnionWith(this.Relations.EquivalentProperty.ToRDFGraph(infexpBehavior))
                                 .UnionWith(this.Relations.InverseOf.ToRDFGraph(infexpBehavior));

            //Annotations
            result       = result.UnionWith(this.Annotations.VersionInfo.ToRDFGraph(infexpBehavior))
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