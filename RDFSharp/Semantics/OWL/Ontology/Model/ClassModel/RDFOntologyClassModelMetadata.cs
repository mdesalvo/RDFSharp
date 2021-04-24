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
    /// RDFOntologyClassModelMetadata represents a collector for relations describing ontology classes.
    /// </summary>
    public class RDFOntologyClassModelMetadata
    {

        #region Properties
        /// <summary>
        /// "rdfs:subClassOf" relations
        /// </summary>
        public RDFOntologyTaxonomy SubClassOf { get; internal set; }

        /// <summary>
        /// "owl:equivalentClass" relations
        /// </summary>
        public RDFOntologyTaxonomy EquivalentClass { get; internal set; }

        /// <summary>
        /// "owl:disjointWith" relations
        /// </summary>
        public RDFOntologyTaxonomy DisjointWith { get; internal set; }

        /// <summary>
        /// "owl:oneOf" relations (specific for enumerate and datarange classes)
        /// </summary>
        public RDFOntologyTaxonomy OneOf { get; internal set; }

        /// <summary>
        /// "owl:intersectionOf" relations (specific for intersection classes)
        /// </summary>
        public RDFOntologyTaxonomy IntersectionOf { get; internal set; }

        /// <summary>
        /// "owl:unionOf" relations (specific for union classes)
        /// </summary>
        public RDFOntologyTaxonomy UnionOf { get; internal set; }

        /// <summary>
        /// "owl:hasKey" relations [OWL2]
        /// </summary>
        public RDFOntologyTaxonomy HasKey { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology class model metadata
        /// </summary>
        internal RDFOntologyClassModelMetadata()
        {
            this.SubClassOf = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.EquivalentClass = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.DisjointWith = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.OneOf = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.IntersectionOf = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.UnionOf = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            this.HasKey = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
        }
        #endregion
    }

}