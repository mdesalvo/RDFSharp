/*
   Copyright 2012-2016 Marco De Salvo

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
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    #region Metadata
    /// <summary>
    /// RDFOntologyAnnotationsMetadata represents a collector for annotations describing resources of an ontology.
    /// </summary>
    public class RDFOntologyAnnotationsMetadata {

        #region Properties
        /// <summary>
        /// "owl:versionInfo" annotations
        /// </summary>
        public RDFOntologyTaxonomy VersionInfo { get; internal set; }

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
        internal RDFOntologyAnnotationsMetadata() {
            this.VersionInfo            = new RDFOntologyTaxonomy();
            this.Comment                = new RDFOntologyTaxonomy();
            this.Label                  = new RDFOntologyTaxonomy();
            this.SeeAlso                = new RDFOntologyTaxonomy();
            this.IsDefinedBy            = new RDFOntologyTaxonomy();
            this.PriorVersion           = new RDFOntologyTaxonomy();
            this.BackwardCompatibleWith = new RDFOntologyTaxonomy();
            this.IncompatibleWith       = new RDFOntologyTaxonomy();
            this.Imports                = new RDFOntologyTaxonomy();
            this.CustomAnnotations      = new RDFOntologyTaxonomy();
        }
        #endregion

    }

    /// <summary>
    /// RDFOntologyClassModelMetadata represents a collector for relations connecting classes of an ontology.
    /// </summary>
    public class RDFOntologyClassModelMetadata {

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
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology class model metadata
        /// </summary>
        internal RDFOntologyClassModelMetadata() {
            this.SubClassOf      = new RDFOntologyTaxonomy();
            this.EquivalentClass = new RDFOntologyTaxonomy();
            this.DisjointWith    = new RDFOntologyTaxonomy();
            this.OneOf           = new RDFOntologyTaxonomy();
            this.IntersectionOf  = new RDFOntologyTaxonomy();
            this.UnionOf         = new RDFOntologyTaxonomy();
        }
        #endregion

    }

    /// <summary>
    /// RDFOntologyPropertyModelMetadata represents a collector for relations connecting properties of an ontology.
    /// </summary>
    public class RDFOntologyPropertyModelMetadata {

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
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology property model metadata
        /// </summary>
        internal RDFOntologyPropertyModelMetadata() {
            this.SubPropertyOf      = new RDFOntologyTaxonomy();
            this.EquivalentProperty = new RDFOntologyTaxonomy();
            this.InverseOf          = new RDFOntologyTaxonomy();
        }
        #endregion

    }

    /// <summary>
    /// RDFOntologyDataMetadata represents a collector for relations connecting facts of an ontology.
    /// </summary>
    public class RDFOntologyDataMetadata {

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
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology data metadata
        /// </summary>
        internal RDFOntologyDataMetadata() {
            this.ClassType     = new RDFOntologyTaxonomy();
            this.SameAs        = new RDFOntologyTaxonomy();
            this.DifferentFrom = new RDFOntologyTaxonomy();
            this.Assertions    = new RDFOntologyTaxonomy();
        }
        #endregion

    }
    #endregion

}