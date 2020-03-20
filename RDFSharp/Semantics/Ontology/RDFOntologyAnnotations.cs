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

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFOntologyAnnotations represents a collector for annotations describing ontology resources.
    /// </summary>
    public class RDFOntologyAnnotations {

        #region Properties
        /// <summary>
        /// "owl:versionInfo" annotations
        /// </summary>
        public RDFOntologyTaxonomy VersionInfo { get; internal set; }

        /// <summary>
        /// "owl:versionIRI" annotations
        /// </summary>
        public RDFOntologyTaxonomy VersionIRI { get; internal set; }

        /// <summary>
        /// "rdfs:comment" annotations
        /// </summary>
        public RDFOntologyTaxonomy Comment { get; internal set; }

        /// <summary>
        /// "rdfs:label" annotations
        /// </summary>
        public RDFOntologyTaxonomy Label { get; internal set; }

        /// <summary>
        /// "rdfs:seeAlso" annotations
        /// </summary>
        public RDFOntologyTaxonomy SeeAlso { get; internal set; }

        /// <summary>
        /// "rdfs:isDefinedBy" annotations
        /// </summary>
        public RDFOntologyTaxonomy IsDefinedBy { get; internal set; }

        /// <summary>
        /// "owl:priorVersion" annotations (specific for ontologies)
        /// </summary>
        public RDFOntologyTaxonomy PriorVersion { get; internal set; }

        /// <summary>
        /// "owl:BackwardCompatibleWith" annotations (specific for ontologies)
        /// </summary>
        public RDFOntologyTaxonomy BackwardCompatibleWith { get; internal set; }

        /// <summary>
        /// "owl:IncompatibleWith" annotations (specific for ontologies)
        /// </summary>
        public RDFOntologyTaxonomy IncompatibleWith { get; internal set; }

        /// <summary>
        /// "owl:imports" annotations (specific for ontologies)
        /// </summary>
        public RDFOntologyTaxonomy Imports { get; internal set; }

        /// <summary>
        /// Custom-property annotations
        /// </summary>
        public RDFOntologyTaxonomy CustomAnnotations { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology annotations metadata
        /// </summary>
        internal RDFOntologyAnnotations() {
            this.VersionInfo            = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.VersionIRI             = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.Comment                = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.Label                  = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.SeeAlso                = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.IsDefinedBy            = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.PriorVersion           = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.BackwardCompatibleWith = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.IncompatibleWith       = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.Imports                = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
            this.CustomAnnotations      = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation);
        }
        #endregion

    }

}