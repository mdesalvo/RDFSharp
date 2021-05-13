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
using System.Threading.Tasks;
using RDFSharp.Model;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntology represents an ontology definition.
    /// </summary>
    public class RDFOntology : RDFOntologyResource
    {

        #region Properties
        /// <summary>
        /// Model (T-BOX) of the ontology
        /// </summary>
        public RDFOntologyModel Model { get; internal set; }

        /// <summary>
        /// Data (A-BOX) of the ontology
        /// </summary>
        public RDFOntologyData Data { get; internal set; }

        /// <summary>
        /// Annotations describing the ontology
        /// </summary>
        public RDFOntologyAnnotations Annotations { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology with the given name
        /// </summary>
        public RDFOntology(RDFResource ontologyName)
        {
            if (ontologyName != null)
            {
                this.Value = ontologyName;
                this.Model = new RDFOntologyModel();
                this.Data = new RDFOntologyData();
                this.Annotations = new RDFOntologyAnnotations();
                this.SetLazyPatternMemberID();
            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntology because given \"ontologyName\" parameter is null.");
            }
        }
        
        /// <summary>
        /// Default-ctor to build an ontology with the given name and the given components
        /// </summary>
        public RDFOntology(RDFResource ontologyName, RDFOntologyModel ontologyModel, RDFOntologyData ontologyData, RDFOntologyAnnotations ontologyAnnotations): this(ontologyName)
        {            
            this.Model = ontologyModel ?? new RDFOntologyModel();
            this.Data = ontologyData ?? new RDFOntologyData();
            this.Annotations = ontologyAnnotations ?? new RDFOntologyAnnotations();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given standard annotation to the ontology resource
        /// </summary>
        public RDFOntology AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation, RDFOntologyResource annotationValue)
        {
            if (annotationValue != null)
            {
                switch (standardAnnotation)
                {

                    //owl:versionInfo accepts a literal as range
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.VersionInfo.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology with owl:versionInfo value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //owl:versionIRI accepts a non-literal as range (in literature it must be an ontology)
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                        if (!annotationValue.IsLiteral())
                        {
                            this.Annotations.VersionIRI.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology with owl:versionIRI value '{0}' because it is an ontology literal", annotationValue));
                        }
                        break;

                    //rdfs:comment accepts a literal as range
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.Comment.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology with rdfs:comment value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //rdfs:label accepts a literal as range
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                        if (annotationValue.IsLiteral())
                        {
                            this.Annotations.Label.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology with rdfs:label value '{0}' because it is not an ontology literal", annotationValue));
                        }
                        break;

                    //rdfs:seeAlso accepts everything as range
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso:
                        this.Annotations.SeeAlso.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:isDefinedBy accepts everything as range
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy:
                        this.Annotations.IsDefinedBy.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:priorVersion accepts a non-literal as range (in literature it must be an ontology)
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion:
                        if (!annotationValue.IsLiteral())
                        {
                            this.Annotations.PriorVersion.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology with owl:priorVersion value '{0}' because it is an ontology literal", annotationValue));
                        }
                        break;

                    //owl:imports accepts a non-literal as range (in literature it must be an ontology)
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports:
                        if (!annotationValue.IsLiteral())
                        {
                            this.Annotations.Imports.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology with owl:imports value '{0}' because it is an ontology literal", annotationValue));
                        }
                        break;

                    //owl:backwardCompatibleWith accepts a non-literal as range (in literature it must be an ontology)
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith:
                        if (!annotationValue.IsLiteral())
                        {
                            this.Annotations.BackwardCompatibleWith.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology with owl:backwardCompatibleWith value '{0}' because it is an ontology literal", annotationValue));
                        }
                        break;

                    //owl:incompatibleWith accepts a non-literal as range (in literature it must be an ontology)
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith:
                        if (!annotationValue.IsLiteral())
                        {
                            this.Annotations.IncompatibleWith.AddEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                        }
                        else
                        {
                            RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Cannot annotate ontology with owl:incompatibleWith value '{0}' because it is an ontology literal", annotationValue));
                        }
                        break;

                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given custom annotation to the ontology resource
        /// </summary>
        public RDFOntology AddCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty, RDFOntologyResource ontologyResource)
        {
            if (ontologyAnnotationProperty != null && ontologyResource != null)
            {

                //owl:versionInfo
                if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyResource);
                }

                //owl:versionIRI
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyResource);
                }

                //rdfs:comment
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyResource);
                }

                //rdfs:label
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyResource);
                }

                //rdfs:seeAlso
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyResource);
                }

                //rdfs:isDefinedBy
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyResource);
                }

                //owl:imports
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyResource);
                }

                //owl:backwardCompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyResource);
                }

                //owl:incompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyResource);
                }

                //owl:priorVersion
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty()))
                {
                    this.AddStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyResource);
                }

                //custom annotation
                else
                {
                    this.Annotations.CustomAnnotations.AddEntry(new RDFOntologyTaxonomyEntry(this, ontologyAnnotationProperty, ontologyResource));
                }

            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given standard annotation from the ontology resource
        /// </summary>
        public RDFOntology RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation standardAnnotation, RDFOntologyResource annotationValue)
        {
            if (annotationValue != null)
            {
                switch (standardAnnotation)
                {

                    //owl:versionInfo
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo:
                        this.Annotations.VersionInfo.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:versionIRI
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI:
                        this.Annotations.VersionIRI.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:comment
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment:
                        this.Annotations.Comment.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:label
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label:
                        this.Annotations.Label.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:seeAlso
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso:
                        this.Annotations.SeeAlso.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //rdfs:isDefinedBy
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy:
                        this.Annotations.IsDefinedBy.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:priorVersion
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion:
                        this.Annotations.PriorVersion.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:imports
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports:
                        this.Annotations.Imports.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:backwardCompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith:
                        this.Annotations.BackwardCompatibleWith.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                    //owl:incompatibleWith
                    case RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith:
                        this.Annotations.IncompatibleWith.RemoveEntry(new RDFOntologyTaxonomyEntry(this, RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty(), annotationValue));
                        break;

                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given custom annotation from the ontology resource
        /// </summary>
        public RDFOntology RemoveCustomAnnotation(RDFOntologyAnnotationProperty ontologyAnnotationProperty, RDFOntologyResource ontologyResource)
        {
            if (ontologyAnnotationProperty != null && ontologyResource != null)
            {

                //owl:versionInfo
                if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionInfo, ontologyResource);
                }

                //owl:versionIRI
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.VersionIRI, ontologyResource);
                }

                //rdfs:comment
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Comment, ontologyResource);
                }

                //rdfs:label
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Label, ontologyResource);
                }

                //rdfs:seeAlso
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.SeeAlso, ontologyResource);
                }

                //rdfs:isDefinedBy
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IsDefinedBy, ontologyResource);
                }

                //owl:imports
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.Imports, ontologyResource);
                }

                //owl:backwardCompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.BackwardCompatibleWith, ontologyResource);
                }

                //owl:incompatibleWith
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.IncompatibleWith, ontologyResource);
                }

                //owl:priorVersion
                else if (ontologyAnnotationProperty.Equals(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty()))
                {
                    this.RemoveStandardAnnotation(RDFSemanticsEnums.RDFOntologyStandardAnnotation.PriorVersion, ontologyResource);
                }

                //custom annotation
                else
                {
                    this.Annotations.CustomAnnotations.RemoveEntry(new RDFOntologyTaxonomyEntry(this, ontologyAnnotationProperty, ontologyResource));
                }

            }
            return this;
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection ontology from this ontology and the given one
        /// </summary>
        public RDFOntology IntersectWith(RDFOntology ontology)
        {
            var result = new RDFOntology(new RDFResource(RDFNamespaceRegister.DefaultNamespace.ToString()));
            if (ontology != null)
            {

                //Intersect the models
                result.Model = this.Model.IntersectWith(ontology.Model);

                //Intersect the data
                result.Data = this.Data.IntersectWith(ontology.Data);

                //Intersect the annotations
                result.Annotations.VersionInfo = this.Annotations.VersionInfo.IntersectWith(ontology.Annotations.VersionInfo);
                result.Annotations.VersionIRI = this.Annotations.VersionIRI.IntersectWith(ontology.Annotations.VersionIRI);
                result.Annotations.Comment = this.Annotations.Comment.IntersectWith(ontology.Annotations.Comment);
                result.Annotations.Label = this.Annotations.Label.IntersectWith(ontology.Annotations.Label);
                result.Annotations.SeeAlso = this.Annotations.SeeAlso.IntersectWith(ontology.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = this.Annotations.IsDefinedBy.IntersectWith(ontology.Annotations.IsDefinedBy);
                result.Annotations.PriorVersion = this.Annotations.PriorVersion.IntersectWith(ontology.Annotations.PriorVersion);
                result.Annotations.BackwardCompatibleWith = this.Annotations.BackwardCompatibleWith.IntersectWith(ontology.Annotations.BackwardCompatibleWith);
                result.Annotations.IncompatibleWith = this.Annotations.IncompatibleWith.IntersectWith(ontology.Annotations.IncompatibleWith);
                result.Annotations.Imports = this.Annotations.Imports.IntersectWith(ontology.Annotations.Imports);
                result.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.IntersectWith(ontology.Annotations.CustomAnnotations);

            }

            return result;
        }

        /// <summary>
        /// Builds a new union ontology from this ontology and the given one
        /// </summary>
        public RDFOntology UnionWith(RDFOntology ontology)
        {
            var result = new RDFOntology(new RDFResource(RDFNamespaceRegister.DefaultNamespace.ToString()));

            //Use this model
            result.Model = result.Model.UnionWith(this.Model);

            //Use this data
            result.Data = result.Data.UnionWith(this.Data);

            //Use this annotations
            result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
            result.Annotations.VersionIRI = result.Annotations.VersionIRI.UnionWith(this.Annotations.VersionIRI);
            result.Annotations.Comment = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
            result.Annotations.Label = result.Annotations.Label.UnionWith(this.Annotations.Label);
            result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
            result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
            result.Annotations.PriorVersion = result.Annotations.PriorVersion.UnionWith(this.Annotations.PriorVersion);
            result.Annotations.BackwardCompatibleWith = result.Annotations.BackwardCompatibleWith.UnionWith(this.Annotations.BackwardCompatibleWith);
            result.Annotations.IncompatibleWith = result.Annotations.IncompatibleWith.UnionWith(this.Annotations.IncompatibleWith);
            result.Annotations.Imports = result.Annotations.Imports.UnionWith(this.Annotations.Imports);
            result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);

            //Manage the given ontology
            if (ontology != null)
            {

                //Union the model
                result.Model = result.Model.UnionWith(ontology.Model);

                //Union the data
                result.Data = result.Data.UnionWith(ontology.Data);

                //Union the annotations
                result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(ontology.Annotations.VersionInfo);
                result.Annotations.VersionIRI = result.Annotations.VersionIRI.UnionWith(ontology.Annotations.VersionIRI);
                result.Annotations.Comment = result.Annotations.Comment.UnionWith(ontology.Annotations.Comment);
                result.Annotations.Label = result.Annotations.Label.UnionWith(ontology.Annotations.Label);
                result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(ontology.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(ontology.Annotations.IsDefinedBy);
                result.Annotations.PriorVersion = result.Annotations.PriorVersion.UnionWith(ontology.Annotations.PriorVersion);
                result.Annotations.BackwardCompatibleWith = result.Annotations.BackwardCompatibleWith.UnionWith(ontology.Annotations.BackwardCompatibleWith);
                result.Annotations.IncompatibleWith = result.Annotations.IncompatibleWith.UnionWith(ontology.Annotations.IncompatibleWith);
                result.Annotations.Imports = result.Annotations.Imports.UnionWith(ontology.Annotations.Imports);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(ontology.Annotations.CustomAnnotations);

            }
            return result;
        }

        /// <summary>
        /// Builds a new difference ontology from this ontology and the given one
        /// </summary>
        public RDFOntology DifferenceWith(RDFOntology ontology)
        {
            var result = new RDFOntology(new RDFResource(RDFNamespaceRegister.DefaultNamespace.ToString()));

            //Use this model
            result.Model = result.Model.UnionWith(this.Model);

            //Use this data
            result.Data = result.Data.UnionWith(this.Data);

            //Use this annotations
            result.Annotations.VersionInfo = result.Annotations.VersionInfo.UnionWith(this.Annotations.VersionInfo);
            result.Annotations.VersionIRI = result.Annotations.VersionIRI.UnionWith(this.Annotations.VersionIRI);
            result.Annotations.Comment = result.Annotations.Comment.UnionWith(this.Annotations.Comment);
            result.Annotations.Label = result.Annotations.Label.UnionWith(this.Annotations.Label);
            result.Annotations.SeeAlso = result.Annotations.SeeAlso.UnionWith(this.Annotations.SeeAlso);
            result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.UnionWith(this.Annotations.IsDefinedBy);
            result.Annotations.PriorVersion = result.Annotations.PriorVersion.UnionWith(this.Annotations.PriorVersion);
            result.Annotations.BackwardCompatibleWith = result.Annotations.BackwardCompatibleWith.UnionWith(this.Annotations.BackwardCompatibleWith);
            result.Annotations.IncompatibleWith = result.Annotations.IncompatibleWith.UnionWith(this.Annotations.IncompatibleWith);
            result.Annotations.Imports = result.Annotations.Imports.UnionWith(this.Annotations.Imports);
            result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.CustomAnnotations);

            //Manage the given ontology
            if (ontology != null)
            {

                //Difference the model
                result.Model = result.Model.DifferenceWith(ontology.Model);

                //Difference the data
                result.Data = result.Data.DifferenceWith(ontology.Data);

                //Difference the annotations
                result.Annotations.VersionInfo = result.Annotations.VersionInfo.DifferenceWith(ontology.Annotations.VersionInfo);
                result.Annotations.VersionIRI = result.Annotations.VersionIRI.DifferenceWith(ontology.Annotations.VersionIRI);
                result.Annotations.Comment = result.Annotations.Comment.DifferenceWith(ontology.Annotations.Comment);
                result.Annotations.Label = result.Annotations.Label.DifferenceWith(ontology.Annotations.Label);
                result.Annotations.SeeAlso = result.Annotations.SeeAlso.DifferenceWith(ontology.Annotations.SeeAlso);
                result.Annotations.IsDefinedBy = result.Annotations.IsDefinedBy.DifferenceWith(ontology.Annotations.IsDefinedBy);
                result.Annotations.PriorVersion = result.Annotations.PriorVersion.DifferenceWith(ontology.Annotations.PriorVersion);
                result.Annotations.BackwardCompatibleWith = result.Annotations.BackwardCompatibleWith.DifferenceWith(ontology.Annotations.BackwardCompatibleWith);
                result.Annotations.IncompatibleWith = result.Annotations.IncompatibleWith.DifferenceWith(ontology.Annotations.IncompatibleWith);
                result.Annotations.Imports = result.Annotations.Imports.DifferenceWith(ontology.Annotations.Imports);
                result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.DifferenceWith(ontology.Annotations.CustomAnnotations);

            }
            return result;
        }

        /// <summary>
        /// Merges the given ontology into this one, returning this one enhanced
        /// </summary>
        public RDFOntology Merge(RDFOntology ontology)
        {
            if (ontology != null)
            {
                //Merge the models
                this.Model = this.Model.UnionWith(ontology.Model);

                //Merge the data
                this.Data = this.Data.UnionWith(ontology.Data);

                //Merge the annotations
                this.Annotations.VersionInfo = this.Annotations.VersionInfo.UnionWith(ontology.Annotations.VersionInfo);
                this.Annotations.VersionIRI = this.Annotations.VersionIRI.UnionWith(ontology.Annotations.VersionIRI);
                this.Annotations.Comment = this.Annotations.Comment.UnionWith(ontology.Annotations.Comment);
                this.Annotations.Label = this.Annotations.Label.UnionWith(ontology.Annotations.Label);
                this.Annotations.SeeAlso = this.Annotations.SeeAlso.UnionWith(ontology.Annotations.SeeAlso);
                this.Annotations.IsDefinedBy = this.Annotations.IsDefinedBy.UnionWith(ontology.Annotations.IsDefinedBy);
                this.Annotations.PriorVersion = this.Annotations.PriorVersion.UnionWith(ontology.Annotations.PriorVersion);
                this.Annotations.BackwardCompatibleWith = this.Annotations.BackwardCompatibleWith.UnionWith(ontology.Annotations.BackwardCompatibleWith);
                this.Annotations.IncompatibleWith = this.Annotations.IncompatibleWith.UnionWith(ontology.Annotations.IncompatibleWith);
                this.Annotations.Imports = this.Annotations.Imports.UnionWith(ontology.Annotations.Imports);
                this.Annotations.CustomAnnotations = this.Annotations.CustomAnnotations.UnionWith(ontology.Annotations.CustomAnnotations);
            }
            return this;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets an ontology representation of the given graph.
        /// </summary>
        public static RDFOntology FromRDFGraph(RDFGraph ontGraph)
            => RDFSemanticsUtilities.FromRDFGraph(ontGraph);

        /// <summary>
        /// Asynchronously gets an ontology representation of the given graph.
        /// </summary>
        public static Task<RDFOntology> FromRDFGraphAsync(RDFGraph ontGraph)
            => Task.Run(() => FromRDFGraph(ontGraph));

        /// <summary>
        /// Gets a graph representation of this ontology, exporting inferences according to the selected behavior
        /// </summary>
        public RDFGraph ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
            => RDFSemanticsUtilities.ToRDFGraph(this, infexpBehavior);

        /// <summary>
        /// Asynchronously gets a graph representation of this ontology, exporting inferences according to the selected behavior
        /// </summary>
        public Task<RDFGraph> ToRDFGraphAsync(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
            => Task.Run(() => ToRDFGraph(infexpBehavior));
        #endregion

        #endregion

    }

    /// <summary>
    /// RDFBASEOntology represents a partial OWL-DL ontology implementation of structural vocabularies (RDF/RDFS/OWL/XSD).
    /// </summary>
    public static class RDFBASEOntology
    {

        #region Properties
        /// <summary>
        /// Singleton instance of the BASE ontology
        /// </summary>
        public static RDFOntology Instance { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the BASE ontology
        /// </summary>
        static RDFBASEOntology()
        {

            #region Declarations

            #region Ontology
            Instance = new RDFOntology(new RDFResource("https://rdfsharp.codeplex.com/semantics/base#"));
            #endregion

            #region Classes

            //RDF+RDFS
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDFS.RESOURCE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDFS.CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDFS.CONTAINER.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDFS.DATATYPE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.XML_LITERAL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.HTML.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.JSON.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.STATEMENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.ALT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.BAG.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.SEQ.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.RDF.LIST.ToRDFOntologyClass());

            //XSD
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.ANY_URI.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.BASE64_BINARY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.BOOLEAN.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.BYTE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.DATE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.DOUBLE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.DURATION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.FLOAT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.G_DAY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.G_MONTH.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.G_MONTH_DAY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.G_YEAR.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.G_YEAR_MONTH.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.HEX_BINARY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.INT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.LANGUAGE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.LONG.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.NAME.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.NCNAME.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.NMTOKEN.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.NORMALIZED_STRING.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.NOTATION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.POSITIVE_INTEGER.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.QNAME.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.SHORT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.STRING.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.TIME.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.TOKEN.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.UNSIGNED_BYTE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.UNSIGNED_INT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.UNSIGNED_LONG.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.XSD.UNSIGNED_SHORT.ToRDFOntologyClass());

            //OWL
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.DEPRECATED_CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.RESTRICTION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.DATA_RANGE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.INDIVIDUAL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.ONTOLOGY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.ANNOTATION_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.DATATYPE_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.DEPRECATED_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.OBJECT_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.ONTOLOGY_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.FUNCTIONAL_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.INVERSE_FUNCTIONAL_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.SYMMETRIC_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.TRANSITIVE_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.THING.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.NOTHING.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.NAMED_INDIVIDUAL.ToRDFOntologyClass());

            //OWL2
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.ALL_DISJOINT_CLASSES.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.ALL_DISJOINT_PROPERTIES.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.ALL_DIFFERENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.ASYMMETRIC_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.REFLEXIVE_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.IRREFLEXIVE_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.OWL.NEGATIVE_PROPERTY_ASSERTION.ToRDFOntologyClass());

            #endregion

            #region Properties

            //RDF+RDFS
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDF.FIRST.ToRDFOntologyProperty()); //plain property
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDF.REST.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDF.LI.ToRDFOntologyProperty()); //plain property
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDF.SUBJECT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDF.PREDICATE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDF.OBJECT.ToRDFOntologyObjectProperty()); //plain property
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDF.VALUE.ToRDFOntologyProperty()); //plain property
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.SUB_CLASS_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.SUB_PROPERTY_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.DOMAIN.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.RANGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.COMMENT.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.LABEL.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.SEE_ALSO.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.IS_DEFINED_BY.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.RDFS.MEMBER.ToRDFOntologyProperty()); //plain property

            //OWL
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.EQUIVALENT_CLASS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.DISJOINT_WITH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.EQUIVALENT_PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.INVERSE_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.ON_PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.ONE_OF.ToRDFOntologyProperty()); //plain property
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.UNION_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.INTERSECTION_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.COMPLEMENT_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.ALL_VALUES_FROM.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.SOME_VALUES_FROM.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.HAS_VALUE.ToRDFOntologyProperty()); //plain property
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.MEMBERS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.DISTINCT_MEMBERS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.CARDINALITY.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.MIN_CARDINALITY.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.MAX_CARDINALITY.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.VERSION_INFO.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.VERSION_IRI.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.IMPORTS.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.INCOMPATIBLE_WITH.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.PRIOR_VERSION.ToRDFOntologyAnnotationProperty());

            //OWL2
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.PROPERTY_DISJOINT_WITH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.DISJOINT_UNION_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.HAS_SELF.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.QUALIFIED_CARDINALITY.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.MIN_QUALIFIED_CARDINALITY.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.MAX_QUALIFIED_CARDINALITY.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.ON_CLASS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.ON_DATARANGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.SOURCE_INDIVIDUAL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.ASSERTION_PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.TARGET_INDIVIDUAL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.TARGET_VALUE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.HAS_KEY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.PROPERTY_CHAIN_AXIOM.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.TOP_OBJECT_PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.TOP_DATA_PROPERTY.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.BOTTOM_OBJECT_PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.OWL.BOTTOM_DATA_PROPERTY.ToRDFOntologyDatatypeProperty());

            #endregion

            #region Facts
            //RDF+RDFS
            Instance.Data.AddFact(RDFVocabulary.RDF.NIL.ToRDFOntologyFact());
            #endregion

            #endregion

            #region Taxonomies

            //SubClassOf
            var subClassOf = RDFVocabulary.RDFS.SUB_CLASS_OF.ToRDFOntologyObjectProperty();
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.RDF.XML_LITERAL.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.RDF.HTML.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.RDF.JSON.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.STRING.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.BOOLEAN.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.BASE64_BINARY.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.HEX_BINARY.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.FLOAT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.DOUBLE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.ANY_URI.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.QNAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NOTATION.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.DURATION.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.TIME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.DATE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_YEAR_MONTH.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATE.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_YEAR.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATE.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_MONTH_DAY.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATE.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_DAY.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATE.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_MONTH.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATE.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NORMALIZED_STRING.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.STRING.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.TOKEN.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NORMALIZED_STRING.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.LANGUAGE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.TOKEN.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.TOKEN.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NMTOKEN.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.TOKEN.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NCNAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NAME.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.LONG.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.LONG.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INT.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.SHORT.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.POSITIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_LONG.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.UNSIGNED_LONG.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.UNSIGNED_INT.ToRDFOntologyClass()));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.UNSIGNED_SHORT.ToRDFOntologyClass()));

            //ClassType
            Instance.Data.Relations.ClassType.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.RDF.NIL.ToRDFOntologyFact(), RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), RDFVocabulary.RDF.LIST.ToRDFOntologyClass()));

            #endregion

            #region Materializations
            //These materialized inferences are needed in order to properly cleanup working ontologies after reasoning activities
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INT.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.LONG.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.LONG.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.LONG.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.LONG.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.UNSIGNED_INT.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.UNSIGNED_LONG.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_BYTE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_INT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_LONG.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_LONG.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_LONG.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.UNSIGNED_LONG.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.UNSIGNED_SHORT.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.POSITIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.INTEGER.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.POSITIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.POSITIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DECIMAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.DATE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.TIME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_DAY.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_DAY.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_MONTH.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_MONTH.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_MONTH_DAY.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_MONTH_DAY.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_YEAR.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_YEAR.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_YEAR_MONTH.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.DATETIME.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.G_YEAR_MONTH.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NORMALIZED_STRING.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.LANGUAGE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NORMALIZED_STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.LANGUAGE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.LANGUAGE.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NORMALIZED_STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NCNAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.TOKEN.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NCNAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NORMALIZED_STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NCNAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NCNAME.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NMTOKEN.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.NORMALIZED_STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NMTOKEN.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.NMTOKEN.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.TOKEN.ToRDFOntologyClass(), subClassOf, RDFVocabulary.XSD.STRING.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            Instance.Model.ClassModel.Relations.SubClassOf.AddEntry(new RDFOntologyTaxonomyEntry(RDFVocabulary.XSD.TOKEN.ToRDFOntologyClass(), subClassOf, RDFVocabulary.RDFS.LITERAL.ToRDFOntologyClass()).SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));
            #endregion

        }
        #endregion

    }

}