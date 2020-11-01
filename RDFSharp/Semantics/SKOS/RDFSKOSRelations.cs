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

using RDFSharp.Semantics.OWL;

namespace RDFSharp.Semantics.SKOS
{

    /// <summary>
    /// RDFSKOSRelations represents a collector for relations connecting skos concepts.
    /// </summary>
    public class RDFSKOSRelations
    {

        #region Properties

        #region SKOS
        /// <summary>
        /// "skos:hasTopConcept" relations
        /// </summary>
        public RDFOntologyTaxonomy TopConcept { get; internal set; }

        /// <summary>
        /// "skos:broader" relations
        /// </summary>
        public RDFOntologyTaxonomy Broader { get; internal set; }

        /// <summary>
        /// "skos:broaderTransitive" relations
        /// </summary>
        public RDFOntologyTaxonomy BroaderTransitive { get; internal set; }

        /// <summary>
        /// "skos:broadMatch" relations
        /// </summary>
        public RDFOntologyTaxonomy BroadMatch { get; internal set; }

        /// <summary>
        /// "skos:narrower" relations
        /// </summary>
        public RDFOntologyTaxonomy Narrower { get; internal set; }

        /// <summary>
        /// "skos:narrowerTransitive" relations
        /// </summary>
        public RDFOntologyTaxonomy NarrowerTransitive { get; internal set; }

        /// <summary>
        /// "skos:narrowMatch" relations
        /// </summary>
        public RDFOntologyTaxonomy NarrowMatch { get; internal set; }

        /// <summary>
        /// "skos:related" relations
        /// </summary>
        public RDFOntologyTaxonomy Related { get; internal set; }

        /// <summary>
        /// "skos:relatedMatch" relations
        /// </summary>
        public RDFOntologyTaxonomy RelatedMatch { get; internal set; }

        /// <summary>
        /// "skos:semanticRelation" relations
        /// </summary>
        public RDFOntologyTaxonomy SemanticRelation { get; internal set; }

        /// <summary>
        /// "skos:mappingRelation" relations
        /// </summary>
        public RDFOntologyTaxonomy MappingRelation { get; internal set; }

        /// <summary>
        /// "skos:closeMatch" relations
        /// </summary>
        public RDFOntologyTaxonomy CloseMatch { get; internal set; }

        /// <summary>
        /// "skos:exactMatch" relations
        /// </summary>
        public RDFOntologyTaxonomy ExactMatch { get; internal set; }

        /// <summary>
        /// "skos:Notation" relations
        /// </summary>
        public RDFOntologyTaxonomy Notation { get; internal set; }
        #endregion

        #region SKOS-XL
        /// <summary>
        /// "skosxl:prefLabel" relations
        /// </summary>
        public RDFOntologyTaxonomy PrefLabel { get; internal set; }

        /// <summary>
        /// "skosxl:altLabel" relations
        /// </summary>
        public RDFOntologyTaxonomy AltLabel { get; internal set; }

        /// <summary>
        /// "skosxl:hiddenLabel" relations
        /// </summary>
        public RDFOntologyTaxonomy HiddenLabel { get; internal set; }

        /// <summary>
        /// "skosxl:LiteralForm" relations
        /// </summary>
        public RDFOntologyTaxonomy LiteralForm { get; internal set; }

        /// <summary>
        /// "skosxl:LabelRelation" relations
        /// </summary>
        public RDFOntologyTaxonomy LabelRelation { get; internal set; }
        #endregion

        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty relations metadata
        /// </summary>
        internal RDFSKOSRelations()
        {

            //SKOS
            this.TopConcept = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.Broader = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.BroaderTransitive = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.BroadMatch = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.Narrower = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.NarrowerTransitive = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.NarrowMatch = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.Related = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.RelatedMatch = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.SemanticRelation = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.MappingRelation = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.CloseMatch = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.ExactMatch = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.Notation = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);

            //SKOS-XL
            this.PrefLabel = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.AltLabel = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.HiddenLabel = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.LiteralForm = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);
            this.LabelRelation = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data);

        }
        #endregion

    }

}