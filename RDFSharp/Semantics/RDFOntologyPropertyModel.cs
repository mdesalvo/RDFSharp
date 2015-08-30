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
    /// RDFOntologyPropertyModel represents the property-oriented model component (T-BOX) of an ontology.
    /// </summary>
    public class RDFOntologyPropertyModel: IEnumerable<RDFOntologyProperty> {

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
            get { return this.Properties.Count(p => p.Value.Functional); }
        }

        /// <summary>
        /// Count of the symmetric properties composing the property model
        /// </summary>
        public Int64 SymmetricPropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsObjectProperty() && ((RDFOntologyObjectProperty)p.Value).Symmetric); }
        }

        /// <summary>
        /// Count of the transitive properties composing the property model
        /// </summary>
        public Int64 TransitivePropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsObjectProperty() && ((RDFOntologyObjectProperty)p.Value).Transitive); }
        }

        /// <summary>
        /// Count of the inverse functional properties composing the property model
        /// </summary>
        public Int64 InverseFunctionalPropertiesCount {
            get { return this.Properties.Count(p => p.Value.IsObjectProperty() && ((RDFOntologyObjectProperty)p.Value).InverseFunctional); }
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
                return this.Properties.Values.Where(p => p.Functional)
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's symmetric properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyObjectProperty> SymmetricPropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsObjectProperty() && ((RDFOntologyObjectProperty)p).Symmetric)
                                             .OfType<RDFOntologyObjectProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's transitive properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyObjectProperty> TransitivePropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsObjectProperty() && ((RDFOntologyObjectProperty)p).Transitive)
                                             .OfType<RDFOntologyObjectProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator on the property model's inverse functional properties for iteration
        /// </summary>
        public IEnumerator<RDFOntologyObjectProperty> InverseFunctionalPropertiesEnumerator {
            get {
                return this.Properties.Values.Where(p => p.IsObjectProperty() && ((RDFOntologyObjectProperty)p).InverseFunctional)
                                             .OfType<RDFOntologyObjectProperty>()
                                             .GetEnumerator();
            }
        }

        /// <summary>
        /// Annotations describing properties of the ontology property model
        /// </summary>
        public RDFOntologyAnnotationsMetadata Annotations { get; internal set; }

        /// <summary>
        /// Relations connecting properties of the ontology property model
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
            this.Annotations = new RDFOntologyAnnotationsMetadata();
            this.Relations   = new RDFOntologyPropertyModelMetadata();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the property model's properties
        /// </summary>
        IEnumerator<RDFOntologyProperty> IEnumerable<RDFOntologyProperty>.GetEnumerator() {
            return this.Properties.Values.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the property model's properties
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Properties.Values.GetEnumerator();
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

                     //Avoid usage of reserved annotation and taxonomy properties
                     if(ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.COMMENT)                  ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.LABEL)                    ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.SEE_ALSO)                 ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.IS_DEFINED_BY)            ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.VERSION_INFO)             ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.PRIOR_VERSION)            ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.INCOMPATIBLE_WITH)        ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.BACKWARD_COMPATIBLE_WITH) ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.AnnotationProperties.IMPORTS)                  ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.DatatypeProperties.ONE_OF)                     ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.TYPE)                         ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.SUB_PROPERTY_OF)              ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.SUB_CLASS_OF)                 ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.SAME_AS)                      ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.DIFFERENT_FROM)               ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.UNION_OF)                     ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.INTERSECTION_OF)              ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.COMPLEMENT_OF)                ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.DISJOINT_WITH)                ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_CLASS)             ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY)          ||
                        ontologyProperty.Equals(RDFOntologyVocabulary.ObjectProperties.INVERSE_OF)) {
                        return this;
                     }

                     this.Properties.Add(ontologyProperty.PatternMemberID, ontologyProperty);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyProperty -> owl:VersionInfo -> ontologyLiteral" annotation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddVersionInfoAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyLiteral ontologyLiteral) {
            if (ontologyProperty != null && ontologyLiteral != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.VersionInfo.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.VERSION_INFO, ontologyLiteral));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyProperty -> rdfs:comment -> ontologyLiteral" annotation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddCommentAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyLiteral ontologyLiteral) {
            if (ontologyProperty != null && ontologyLiteral != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.Comment.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.COMMENT, ontologyLiteral));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyProperty -> rdfs:label -> ontologyLiteral" annotation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddLabelAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyLiteral ontologyLiteral) {
            if (ontologyProperty != null && ontologyLiteral != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.Label.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.LABEL, ontologyLiteral));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyProperty -> rdfs:seeAlso -> ontologyResource" annotation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddSeeAlsoAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyResource ontologyResource) {
            if (ontologyProperty != null && ontologyResource != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.SeeAlso.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.SEE_ALSO, ontologyResource));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyProperty -> rdfs:isDefinedBy -> ontologyResource" annotation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddIsDefinedByAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyResource ontologyResource) {
            if (ontologyProperty != null && ontologyResource != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.IsDefinedBy.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.IS_DEFINED_BY, ontologyResource));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "ontologyProperty -> ontologyAnnotationProperty -> ontologyResource" annotation to the class model 
        /// </summary>
        public RDFOntologyPropertyModel AddCustomAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyAnnotationProperty ontologyAnnotationProperty, RDFOntologyResource ontologyResource) {
            if (ontologyProperty != null && ontologyAnnotationProperty != null && ontologyResource != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {

                     //Standard RDFS/OWL annotation properties cannot be used in custom annotations
                     if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO)             ||
                         ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT)                 ||
                         ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL)                   ||
                         ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO)                ||
                         ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY)           ||
                         ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS)                  ||
                         ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH) ||
                         ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH)        ||
                         ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION)) {
                         return this;
                     }

                     this.Annotations.CustomAnnotations.AddEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, ontologyAnnotationProperty, ontologyResource));
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the "childProperty -> rdfs:subPropertyOf -> motherProperty" relation to the property model.
        /// </summary>
        public RDFOntologyPropertyModel AddSubPropertyOfRelation(RDFOntologyObjectProperty childProperty, RDFOntologyObjectProperty motherProperty) {
            if (childProperty != null && motherProperty != null && !childProperty.Equals(motherProperty)) {

                //Enforce taxonomy checks before adding the subPropertyOf relation, in order to not model inconsistencies
                if (!RDFOntologyHelper.IsSubPropertyOf(motherProperty, childProperty, this)        &&
                    !RDFOntologyHelper.IsEquivalentPropertyOf(motherProperty, childProperty, this)) {
                        this.Relations.SubPropertyOf.AddEntry(new RDFOntologyTaxonomyEntry(childProperty, RDFOntologyVocabulary.ObjectProperties.SUB_PROPERTY_OF, motherProperty));
                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "childProperty -> rdfs:subPropertyOf -> motherProperty" relation to the property model.
        /// </summary>
        public RDFOntologyPropertyModel AddSubPropertyOfRelation(RDFOntologyDatatypeProperty childProperty, RDFOntologyDatatypeProperty motherProperty) {
            if (childProperty != null && motherProperty != null && !childProperty.Equals(motherProperty)) {

                //Enforce taxonomy checks before adding the subPropertyOf relation, in order to not model inconsistencies
                if (!RDFOntologyHelper.IsSubPropertyOf(motherProperty, childProperty, this) &&
                    !RDFOntologyHelper.IsEquivalentPropertyOf(motherProperty, childProperty, this)) {
                        this.Relations.SubPropertyOf.AddEntry(new RDFOntologyTaxonomyEntry(childProperty, RDFOntologyVocabulary.ObjectProperties.SUB_PROPERTY_OF, motherProperty));
                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "aProperty -> owl:equivalentProperty -> bProperty" relation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddEquivalentPropertyRelation(RDFOntologyObjectProperty aProperty, RDFOntologyObjectProperty bProperty) {
            if (aProperty  != null && bProperty != null && !aProperty.Equals(bProperty)) {

                //Enforce taxonomy checks before adding the equivalentProperty relation, in order to not model inconsistencies
                if (!RDFOntologyHelper.IsSubPropertyOf(aProperty, bProperty, this)   &&
                    !RDFOntologyHelper.IsSuperPropertyOf(aProperty, bProperty, this)) {
                        this.Relations.EquivalentProperty.AddEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, bProperty));
                        this.Relations.EquivalentProperty.AddEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, aProperty).SetInference(true));
                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "aProperty -> owl:equivalentProperty -> bProperty" relation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddEquivalentPropertyRelation(RDFOntologyDatatypeProperty aProperty, RDFOntologyDatatypeProperty bProperty) {
            if (aProperty  != null && bProperty != null && !aProperty.Equals(bProperty)) {

                //Enforce taxonomy checks before adding the equivalentProperty relation, in order to not model inconsistencies
                if (!RDFOntologyHelper.IsSubPropertyOf(aProperty, bProperty, this)   &&
                    !RDFOntologyHelper.IsSuperPropertyOf(aProperty, bProperty, this)) {
                        this.Relations.EquivalentProperty.AddEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, bProperty));
                        this.Relations.EquivalentProperty.AddEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, aProperty).SetInference(true));
                }

            }
            return this;
        }

        /// <summary>
        /// Adds the "aProperty -> owl:inverseOf -> bProperty" relation to the property model 
        /// </summary>
        public RDFOntologyPropertyModel AddInverseOfRelation(RDFOntologyObjectProperty aProperty, RDFOntologyObjectProperty bProperty) {
            if (aProperty != null && bProperty != null && !aProperty.Equals(bProperty)) {
                if (!aProperty.IsAnnotationProperty()) {
                     this.Relations.InverseOf.AddEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFOntologyVocabulary.ObjectProperties.INVERSE_OF, bProperty));
                     this.Relations.InverseOf.AddEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFOntologyVocabulary.ObjectProperties.INVERSE_OF, aProperty).SetInference(true));
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
        /// Removes the "ontologyProperty -> owl:VersionInfo -> ontologyLiteral" annotation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveVersionInfoAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyLiteral ontologyLiteral) {
            if (ontologyProperty != null && ontologyLiteral != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.VersionInfo.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.VERSION_INFO, ontologyLiteral));
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyProperty -> rdfs:comment -> ontologyLiteral" annotation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveCommentAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyLiteral ontologyLiteral) {
            if (ontologyProperty != null && ontologyLiteral != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.Comment.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.COMMENT, ontologyLiteral));
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyProperty -> rdfs:label -> ontologyLiteral" annotation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveLabelAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyLiteral ontologyLiteral) {
            if (ontologyProperty != null && ontologyLiteral != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.Label.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.LABEL, ontologyLiteral));
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyProperty -> rdfs:seeAlso -> ontologyResource" annotation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveSeeAlsoAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyResource ontologyResource) {
            if (ontologyProperty != null && ontologyResource != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.SeeAlso.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.SEE_ALSO, ontologyResource));
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyProperty -> rdfs:isDefinedBy -> ontologyResource" annotation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveIsDefinedByAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyResource ontologyResource) {
            if (ontologyProperty != null && ontologyResource != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.IsDefinedBy.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, RDFOntologyVocabulary.AnnotationProperties.IS_DEFINED_BY, ontologyResource));
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the "ontologyProperty -> ontologyAnnotationProperty -> ontologyResource" annotation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveCustomAnnotation(RDFOntologyProperty ontologyProperty, RDFOntologyAnnotationProperty ontologyAnnotationProperty, RDFOntologyResource ontologyResource) {
            if (ontologyProperty != null && ontologyAnnotationProperty != null && ontologyResource != null) {
                if (!ontologyProperty.IsAnnotationProperty()) {
                     this.Annotations.CustomAnnotations.RemoveEntry(new RDFOntologyTaxonomyEntry(ontologyProperty, ontologyAnnotationProperty, ontologyResource));
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the "childProperty -> rdfs:subPropertyOf -> motherProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveSubPropertyOfRelation(RDFOntologyObjectProperty childProperty, RDFOntologyObjectProperty motherProperty) {
            if (childProperty != null && motherProperty != null) {
                this.Relations.SubPropertyOf.RemoveEntry(new RDFOntologyTaxonomyEntry(childProperty, RDFOntologyVocabulary.ObjectProperties.SUB_PROPERTY_OF, motherProperty));
            }
            return this;
        }

        /// <summary>
        /// Removes the "childProperty -> rdfs:subPropertyOf -> motherProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveSubPropertyOfRelation(RDFOntologyDatatypeProperty childProperty, RDFOntologyDatatypeProperty motherProperty) {
            if (childProperty != null && motherProperty != null) {
                this.Relations.SubPropertyOf.RemoveEntry(new RDFOntologyTaxonomyEntry(childProperty, RDFOntologyVocabulary.ObjectProperties.SUB_PROPERTY_OF, motherProperty));
            }
            return this;
        }

        /// <summary>
        /// Removes the "aProperty -> owl:equivalentProperty -> bProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveEquivalentPropertyRelation(RDFOntologyObjectProperty aProperty, RDFOntologyObjectProperty bProperty) {
            if (aProperty != null && bProperty != null) {
                this.Relations.EquivalentProperty.RemoveEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, bProperty));
                this.Relations.EquivalentProperty.RemoveEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, aProperty));
            }
            return this;
        }

        /// <summary>
        /// Removes the "aProperty -> owl:equivalentProperty -> bProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveEquivalentPropertyRelation(RDFOntologyDatatypeProperty aProperty, RDFOntologyDatatypeProperty bProperty) {
            if (aProperty != null && bProperty != null) {
                this.Relations.EquivalentProperty.RemoveEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, bProperty));
                this.Relations.EquivalentProperty.RemoveEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFOntologyVocabulary.ObjectProperties.EQUIVALENT_PROPERTY, aProperty));
            }
            return this;
        }

        /// <summary>
        /// Removes the "aProperty -> owl:inverseOf -> bProperty" relation from the property model 
        /// </summary>
        public RDFOntologyPropertyModel RemoveInverseOfRelation(RDFOntologyObjectProperty aProperty, RDFOntologyObjectProperty bProperty) {
            if (aProperty != null && bProperty != null) {
                this.Relations.InverseOf.RemoveEntry(new RDFOntologyTaxonomyEntry(aProperty, RDFOntologyVocabulary.ObjectProperties.INVERSE_OF, bProperty));
                this.Relations.InverseOf.RemoveEntry(new RDFOntologyTaxonomyEntry(bProperty, RDFOntologyVocabulary.ObjectProperties.INVERSE_OF, aProperty).SetInference(true));
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
        /// Gets a graph representation of this ontology property model
        /// </summary>
        public RDFGraph ToRDFGraph() {
            var result   = new RDFGraph();
            foreach (var p in this) {
                if  (p.IsAnnotationProperty()) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.ANNOTATION_PROPERTY));
                }
                else if (p.IsDatatypeProperty()) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DATATYPE_PROPERTY));
                }
                else if (p.IsObjectProperty()) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.OBJECT_PROPERTY));
                     if (((RDFOntologyObjectProperty)p).Symmetric) {
                         result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.SYMMETRIC_PROPERTY));
                     }
                     if (((RDFOntologyObjectProperty)p).Transitive) {
                         result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.TRANSITIVE_PROPERTY));
                     }
                     if (((RDFOntologyObjectProperty)p).InverseFunctional) {
                         result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY));
                     }
                }
                if  (p.Functional) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.FUNCTIONAL_PROPERTY));
                }
                if  (p.Deprecated) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.DEPRECATED_PROPERTY));
                }
                if  (p.Domain != null) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDFS.DOMAIN, (RDFResource)p.Domain.Value));
                }
                if  (p.Range != null) {
                     result.AddTriple(new RDFTriple((RDFResource)p.Value, RDFVocabulary.RDFS.RANGE, (RDFResource)p.Range.Value));
                }

                //Relations
                foreach (var subpropertyOf in this.Relations.SubPropertyOf) {
                     result.AddTriple(new RDFTriple((RDFResource)subpropertyOf.TaxonomySubject.Value, RDFVocabulary.RDFS.SUB_PROPERTY_OF, (RDFResource)subpropertyOf.TaxonomyObject.Value));
                }
                foreach (var equivpropertyOf in this.Relations.EquivalentProperty) {
                     result.AddTriple(new RDFTriple((RDFResource)equivpropertyOf.TaxonomySubject.Value, RDFVocabulary.OWL.EQUIVALENT_PROPERTY, (RDFResource)equivpropertyOf.TaxonomyObject.Value));
                }
                foreach (var inverseOf in this.Relations.InverseOf) {
                     result.AddTriple(new RDFTriple((RDFResource)inverseOf.TaxonomySubject.Value, RDFVocabulary.OWL.INVERSE_OF, (RDFResource)inverseOf.TaxonomyObject.Value));
                }

                //Annotations
                foreach (var versInfo in this.Annotations.VersionInfo) {
                     result.AddTriple(new RDFTriple((RDFResource)versInfo.TaxonomySubject.Value, RDFVocabulary.OWL.VERSION_INFO, (RDFLiteral)versInfo.TaxonomyObject.Value));
                }
                foreach (var comment in this.Annotations.Comment) {
                     result.AddTriple(new RDFTriple((RDFResource)comment.TaxonomySubject.Value, RDFVocabulary.RDFS.COMMENT, (RDFLiteral)comment.TaxonomyObject.Value));
                }
                foreach (var label in this.Annotations.Label) {
                     result.AddTriple(new RDFTriple((RDFResource)label.TaxonomySubject.Value, RDFVocabulary.RDFS.LABEL, (RDFLiteral)label.TaxonomyObject.Value));
                }
                foreach (var seeAlso in this.Annotations.SeeAlso) {
                     if (seeAlso.TaxonomyObject.IsLiteral()) {
                         result.AddTriple(new RDFTriple((RDFResource)seeAlso.TaxonomySubject.Value, RDFVocabulary.RDFS.SEE_ALSO, (RDFLiteral)seeAlso.TaxonomyObject.Value));
                     }
                     else {
                         result.AddTriple(new RDFTriple((RDFResource)seeAlso.TaxonomySubject.Value, RDFVocabulary.RDFS.SEE_ALSO, (RDFResource)seeAlso.TaxonomyObject.Value));
                     }
                }
                foreach (var isDefBy in this.Annotations.IsDefinedBy) {
                     if (isDefBy.TaxonomyObject.IsLiteral()) {
                         result.AddTriple(new RDFTriple((RDFResource)isDefBy.TaxonomySubject.Value, RDFVocabulary.RDFS.IS_DEFINED_BY, (RDFLiteral)isDefBy.TaxonomyObject.Value));
                     }
                     else {
                         result.AddTriple(new RDFTriple((RDFResource)isDefBy.TaxonomySubject.Value, RDFVocabulary.RDFS.IS_DEFINED_BY, (RDFResource)isDefBy.TaxonomyObject.Value));
                     }
                }
                foreach (var custAnn in this.Annotations.CustomAnnotations) {
                     if (custAnn.TaxonomyObject.IsLiteral()) {
                         result.AddTriple(new RDFTriple((RDFResource)custAnn.TaxonomySubject.Value, (RDFResource)custAnn.TaxonomyPredicate.Value, (RDFLiteral)custAnn.TaxonomyObject.Value));
                     }
                     else {
                         result.AddTriple(new RDFTriple((RDFResource)custAnn.TaxonomySubject.Value, (RDFResource)custAnn.TaxonomyPredicate.Value, (RDFResource)custAnn.TaxonomyObject.Value));
                     }
                }

            }
            return result;
        }
        #endregion

        #endregion

    }

}