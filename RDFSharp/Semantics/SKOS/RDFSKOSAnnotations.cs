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
using RDFSharp.Semantics.OWL;

namespace RDFSharp.Semantics.SKOS
{

    /// <summary>
    /// RDFSKOSAnnotations represents a collector for annotations describing SKOS concepts.
    /// </summary>
    public class RDFSKOSAnnotations : IDisposable
    {

        #region Properties
        /// <summary>
        /// "skos:prefLabel" annotations
        /// </summary>
        public RDFOntologyTaxonomy PrefLabel { get; internal set; }

        /// <summary>
        /// "skos:altLabel" annotations
        /// </summary>
        public RDFOntologyTaxonomy AltLabel { get; internal set; }

        /// <summary>
        /// "skos:hiddenLabel" annotations
        /// </summary>
        public RDFOntologyTaxonomy HiddenLabel { get; internal set; }

        /// <summary>
        /// "skos:Note" annotations
        /// </summary>
        public RDFOntologyTaxonomy Note { get; internal set; }

        /// <summary>
        /// "skos:changeNote" annotations
        /// </summary>
        public RDFOntologyTaxonomy ChangeNote { get; internal set; }

        /// <summary>
        /// "skos:editorialNote" annotations
        /// </summary>
        public RDFOntologyTaxonomy EditorialNote { get; internal set; }

        /// <summary>
        /// "skos:historyNote" annotations
        /// </summary>
        public RDFOntologyTaxonomy HistoryNote { get; internal set; }

        /// <summary>
        /// "skos:scopeNote" annotations
        /// </summary>
        public RDFOntologyTaxonomy ScopeNote { get; internal set; }

        /// <summary>
        /// "skos:definition" annotations
        /// </summary>
        public RDFOntologyTaxonomy Definition { get; internal set; }

        /// <summary>
        /// "skos:example" annotations
        /// </summary>
        public RDFOntologyTaxonomy Example { get; internal set; }

        /// <summary>
        /// Flag indicating that the SKOS annotations has already been disposed
        /// </summary>
        private bool Disposed { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology annotations metadata
        /// </summary>
        internal RDFSKOSAnnotations()
        {
            this.PrefLabel = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.AltLabel = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.HiddenLabel = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.Note = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.ChangeNote = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.EditorialNote = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.HistoryNote = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.ScopeNote = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.Definition = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.Example = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false);
            this.Disposed = false;
        }

        /// <summary>
        /// Destroys the SKOS annotations instance
        /// </summary>
        ~RDFSKOSAnnotations() => this.Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Disposes the SKOS annotations
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the SKOS annotations (business logic of resources disposal)
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed)
                return;

            if (disposing)
            {
                this.PrefLabel.Dispose();
                this.PrefLabel = null;

                this.AltLabel.Dispose();
                this.AltLabel = null;

                this.HiddenLabel.Dispose();
                this.HiddenLabel = null;

                this.Note.Dispose();
                this.Note = null;

                this.ChangeNote.Dispose();
                this.ChangeNote = null;

                this.EditorialNote.Dispose();
                this.EditorialNote = null;

                this.HistoryNote.Dispose();
                this.HistoryNote = null;

                this.ScopeNote.Dispose();
                this.ScopeNote = null;

                this.Definition.Dispose();
                this.Definition = null;

                this.Example.Dispose();
                this.Example = null;
            }

            this.Disposed = true;
        }
        #endregion
    }

}