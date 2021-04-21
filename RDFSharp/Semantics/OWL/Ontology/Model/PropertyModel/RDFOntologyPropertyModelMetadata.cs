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
    /// RDFOntologyPropertyModelMetadata represents a collector for relations describing ontology properties.
    /// </summary>
    public class RDFOntologyPropertyModelMetadata : IDisposable
    {

        #region Properties
        /// <summary>
        /// "rdfs:subPropertyOf" relations
        /// </summary>
        public RDFOntologyTaxonomy SubPropertyOf { get; internal set; }

        /// <summary>
        /// "owl:equivalentProperty" relations
        /// </summary>
        public RDFOntologyTaxonomy EquivalentProperty { get; internal set; }

        /// <summary>
        /// "owl:inverseOf" relations
        /// </summary>
        public RDFOntologyTaxonomy InverseOf { get; internal set; }

        /// <summary>
        /// "owl:propertyDisjointWith" relations [OWL2]
        /// </summary>
        public RDFOntologyTaxonomy PropertyDisjointWith { get; internal set; }

        /// <summary>
        /// "owl:propertyChainAxiom" relations [OWL2]
        /// </summary>
        public RDFOntologyTaxonomy PropertyChainAxiom { get; internal set; }

        /// <summary>
        /// Flag indicating that the ontology property model metadata has already been disposed
        /// </summary>
        private bool Disposed { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology property model metadata
        /// </summary>
        internal RDFOntologyPropertyModelMetadata()
        {
            this.SubPropertyOf = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.EquivalentProperty = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.InverseOf = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.PropertyDisjointWith = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.PropertyChainAxiom = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, true); //This taxonomy accepts duplicates
            this.Disposed = false;
        }

        /// <summary>
        /// Destroys the ontology property model metadata instance
        /// </summary>
        ~RDFOntologyPropertyModelMetadata() => this.Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Disposes the ontology property model metadata
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the ontology property model metadata (business logic of resources disposal)
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed)
                return;

            if (disposing)
            {
                this.SubPropertyOf.Dispose();
                this.SubPropertyOf = null;

                this.EquivalentProperty.Dispose();
                this.EquivalentProperty = null;

                this.InverseOf.Dispose();
                this.InverseOf = null;

                this.PropertyChainAxiom.Dispose();
                this.PropertyChainAxiom = null;

                this.PropertyDisjointWith.Dispose();
                this.PropertyDisjointWith = null;
            }

            this.Disposed = true;
        }
        #endregion
    }

}