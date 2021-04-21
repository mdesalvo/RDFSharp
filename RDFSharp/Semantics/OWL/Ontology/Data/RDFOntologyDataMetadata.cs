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
    /// RDFOntologyDataMetadata represents a collector for relations connecting ontology facts.
    /// </summary>
    public class RDFOntologyDataMetadata : IDisposable
    {

        #region Properties
        /// <summary>
        /// "rdf:type" relations
        /// </summary>
        public RDFOntologyTaxonomy ClassType { get; internal set; }

        /// <summary>
        /// "owl:sameAs" relations
        /// </summary>
        public RDFOntologyTaxonomy SameAs { get; internal set; }

        /// <summary>
        /// "owl:differentFrom" relations
        /// </summary>
        public RDFOntologyTaxonomy DifferentFrom { get; internal set; }

        /// <summary>
        /// "ontology property -> ontology resource" custom relations
        /// </summary>
        public RDFOntologyTaxonomy Assertions { get; internal set; }

        /// <summary>
        /// "ontology property -> ontology resource" custom negative relations [OWL2]
        /// </summary>
        public RDFOntologyTaxonomy NegativeAssertions { get; internal set; }

        /// <summary>
        /// Flag indicating that the ontology data metadata has already been disposed
        /// </summary>
        private bool Disposed { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology data metadata
        /// </summary>
        internal RDFOntologyDataMetadata()
        {
            this.ClassType = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false);
            this.SameAs = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false);
            this.DifferentFrom = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false);
            this.Assertions = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false);
            this.NegativeAssertions = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false);
            this.Disposed = false;
        }

        /// <summary>
        /// Destroys the ontology data metadata instance
        /// </summary>
        ~RDFOntologyDataMetadata() => this.Dispose(false);
        #endregion

        #region Interfaces
        /// <summary>
        /// Disposes the ontology data metadata
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the ontology data metadata (business logic of resources disposal)
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed)
                return;

            if (disposing)
            {
                this.ClassType.Dispose();
                this.ClassType = null;

                this.SameAs.Dispose();
                this.SameAs = null;

                this.DifferentFrom.Dispose();
                this.DifferentFrom = null;

                this.Assertions.Dispose();
                this.Assertions = null;

                this.NegativeAssertions.Dispose();
                this.NegativeAssertions = null;
            }

            this.Disposed = true;
        }
        #endregion
    }

}