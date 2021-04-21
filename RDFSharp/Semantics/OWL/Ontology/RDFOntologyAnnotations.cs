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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyAnnotations represents a collector for annotations describing ontology resources.
    /// </summary>
    public class RDFOntologyAnnotations : IDisposable
    {

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

        /// <summary>
        /// Flag indicating that the ontology annotations has already been disposed
        /// </summary>
        private bool Disposed { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology annotations metadata
        /// </summary>
        internal RDFOntologyAnnotations()
        {
            this.VersionInfo = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.VersionIRI = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.Comment = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.Label = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.SeeAlso = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.IsDefinedBy = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.PriorVersion = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.BackwardCompatibleWith = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.IncompatibleWith = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.Imports = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.CustomAnnotations = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.Disposed = false;
        }

        /// <summary>
        /// Destroys the ontology annotations instance
        /// </summary>
        ~RDFOntologyAnnotations() => this.Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Disposes the ontology annotations
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the ontology annotations (business logic of resources disposal)
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed)
                return;

            if (disposing)
            {
                this.VersionInfo.Dispose();
                this.VersionInfo = null;

                this.VersionIRI.Dispose();
                this.VersionIRI = null;

                this.Comment.Dispose();
                this.Comment = null;

                this.Label.Dispose();
                this.Label = null;

                this.SeeAlso.Dispose();
                this.SeeAlso = null;

                this.IsDefinedBy.Dispose();
                this.IsDefinedBy = null;

                this.PriorVersion.Dispose();
                this.PriorVersion = null;

                this.BackwardCompatibleWith.Dispose();
                this.BackwardCompatibleWith = null;

                this.IncompatibleWith.Dispose();
                this.IncompatibleWith = null;

                this.Imports.Dispose();
                this.Imports = null;

                this.CustomAnnotations.Dispose();
                this.CustomAnnotations = null;
            }

            this.Disposed = true;
        }
        #endregion
    }

}