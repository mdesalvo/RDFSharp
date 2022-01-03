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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyDataMetadata represents a collector for relations connecting ontology facts.
    /// </summary>
    public class RDFOntologyDataMetadata
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
        /// "skos:member" relations [SKOS]
        /// </summary>
        internal RDFOntologyTaxonomy Member { get; set; }

        /// <summary>
        /// "skos:memberList" relations [SKOS]
        /// </summary>
        internal RDFOntologyTaxonomy MemberList { get; set; }
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
            this.NegativeAssertions = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false); //OWL2
            this.Member = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false); //SKOS
            this.MemberList = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false); //SKOS
        }
        #endregion
    }

}