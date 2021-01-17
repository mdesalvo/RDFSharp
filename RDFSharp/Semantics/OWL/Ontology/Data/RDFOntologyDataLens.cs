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

using RDFSharp.Model;
using RDFSharp.Semantics.SKOS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyDataLens represents a magnifying glass on the knowledge available for a fact within an ontology
    /// </summary>
    public class RDFOntologyDataLens
    {
        #region Properties
        /// <summary>
        /// Fact being observed by the data lens
        /// </summary>
        public RDFOntologyFact OntologyFact { get; internal set; }

        /// <summary>
        /// Ontology being observed by the data lens
        /// </summary>
        public RDFOntology Ontology { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a data lens for the given fact on the given ontology
        /// </summary>
        public RDFOntologyDataLens(RDFOntologyFact ontologyFact, RDFOntology ontology)
        {
            if (ontologyFact != null)
            {
                if (ontology != null)
                {
                    this.OntologyFact = ontologyFact;
                    this.Ontology = ontology.UnionWith(RDFBASEOntology.Instance);
                }
                else
                {
                    throw new RDFSemanticsException("Cannot create data lens because given \"ontology\" parameter is null");
                }
            }
            else
            {
                throw new RDFSemanticsException("Cannot create data lens because given \"ontologyFact\" parameter is null");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the lens ontology with support for SKOS ontology
        /// </summary>
        public RDFOntologyDataLens InitializeSKOS()
        {
            this.Ontology = this.Ontology.UnionWith(RDFSKOSOntology.Instance);
            return this;
        }

        /// <summary>
        /// Enlists the facts which are directly (or indirectly, if inference is requested) equivalent to the lens fact
        /// </summary>
        public List<(bool, RDFOntologyFact)> SameFacts(bool enableInference)
        {
            List<(bool, RDFOntologyFact)> result = new List<(bool, RDFOntologyFact)>();

            //First-level enlisting of same facts
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.SameAs.SelectEntriesBySubject(this.OntologyFact).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyFact)sf.TaxonomyObject));

            //Inference-enabled discovery of same facts
            if (enableInference)
            {
                List<RDFOntologyFact> sameFacts = RDFOntologyHelper.GetSameFactsAs(this.Ontology.Data, this.OntologyFact).ToList();
                foreach (RDFOntologyFact sameFact in sameFacts)
                {
                    if (!result.Any(f => f.Item2.Equals(sameFact)))
                        result.Add((true, sameFact));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the facts which are directly (or indirectly, if inference is requested) different from the lens fact
        /// </summary>
        public List<(bool, RDFOntologyFact)> DifferentFacts(bool enableInference)
        {
            List<(bool, RDFOntologyFact)> result = new List<(bool, RDFOntologyFact)>();

            //First-level enlisting of different facts
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.DifferentFrom.SelectEntriesBySubject(this.OntologyFact).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyFact)sf.TaxonomyObject));

            //Inference-enabled discovery of different facts
            if (enableInference)
            {
                List<RDFOntologyFact> differentFacts = RDFOntologyHelper.GetDifferentFactsFrom(this.Ontology.Data, this.OntologyFact).ToList();
                foreach (RDFOntologyFact differentFact in differentFacts)
                {
                    if (!result.Any(f => f.Item2.Equals(differentFact)))
                        result.Add((true, differentFact));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the object assertions which are directly (or indirectly, if inference is requested) assigned to the lens fact
        /// </summary>
        public List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)> ObjectAssertions(bool enableInference)
        {
            List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)> result = new List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)>();

            RDFOntologyTaxonomy sftaxonomy = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(this.OntologyFact);
            if (enableInference)
            {
                //Inference-enabled discovery of assigned object relations
                foreach (RDFOntologyTaxonomyEntry sf in sftaxonomy.Where(te => te.TaxonomyPredicate.IsObjectProperty()))
                    result.Add((sf.IsInference(), (RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
            }
            else
            {
                //First-level enlisting of assigned object relations
                foreach (RDFOntologyTaxonomyEntry sf in sftaxonomy.Where(te => te.TaxonomyPredicate.IsObjectProperty() && !te.IsInference()))
                    result.Add((false, (RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
            }

            return result;
        }

        /// <summary>
        /// Enlists the negative object assertions which are directly (or indirectly, if inference is requested) assigned to the lens fact
        /// </summary>
        public List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)> NegativeObjectAssertions(bool enableInference)
        {
            List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)> result = new List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)>();

            RDFOntologyTaxonomy sftaxonomy = this.Ontology.Data.Relations.NegativeAssertions.SelectEntriesBySubject(this.OntologyFact);
            if (enableInference)
            {
                //Inference-enabled discovery of assigned object negative relations
                foreach (RDFOntologyTaxonomyEntry sf in sftaxonomy.Where(te => te.TaxonomyPredicate.IsObjectProperty()))
                    result.Add((sf.IsInference(), (RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
            }
            else
            {
                //First-level enlisting of assigned object negative relations
                foreach (RDFOntologyTaxonomyEntry sf in sftaxonomy.Where(te => te.TaxonomyPredicate.IsObjectProperty() && !te.IsInference()))
                    result.Add((false, (RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
            }

            return result;
        }

        /// <summary>
        /// Enlists the data assertions which are directly (or indirectly, if inference is requested) assigned to the lens fact
        /// </summary>
        public List<(bool,RDFOntologyDatatypeProperty, RDFOntologyLiteral)> DataAssertions(bool enableInference)
        {
            List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)> result = new List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)>();

            RDFOntologyTaxonomy sftaxonomy = this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(this.OntologyFact);
            if (enableInference)
            {
                //Inference-enabled discovery of assigned literal relations
                foreach (RDFOntologyTaxonomyEntry sf in sftaxonomy.Where(te => te.TaxonomyPredicate.IsDatatypeProperty()))
                    result.Add((sf.IsInference(), (RDFOntologyDatatypeProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));
            }
            else
            {
                //First-level enlisting of assigned literal relations
                foreach (RDFOntologyTaxonomyEntry sf in sftaxonomy.Where(te => te.TaxonomyPredicate.IsDatatypeProperty() && !te.IsInference()))
                    result.Add((false, (RDFOntologyDatatypeProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));
            }

            return result;
        }

        /// <summary>
        /// Enlists the negative data assertions which are directly (or indirectly, if inference is requested) assigned to the lens fact
        /// </summary>
        public List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)> NegativeDataAssertions(bool enableInference)
        {
            List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)> result = new List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)>();

            RDFOntologyTaxonomy sftaxonomy = this.Ontology.Data.Relations.NegativeAssertions.SelectEntriesBySubject(this.OntologyFact);
            if (enableInference)
            {
                //Inference-enabled discovery of assigned literal negative relations
                foreach (RDFOntologyTaxonomyEntry sf in sftaxonomy.Where(te => te.TaxonomyPredicate.IsDatatypeProperty()))
                    result.Add((sf.IsInference(), (RDFOntologyDatatypeProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));
            }
            else
            {
                //First-level enlisting of assigned literal negative relations
                foreach (RDFOntologyTaxonomyEntry sf in sftaxonomy.Where(te => te.TaxonomyPredicate.IsDatatypeProperty() && !te.IsInference()))
                    result.Add((false, (RDFOntologyDatatypeProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));
            }

            return result;
        }

        /// <summary>
        /// Enlists the object annotations which are assigned to the lens fact
        /// </summary>
        public List<(RDFOntologyAnnotationProperty, RDFOntologyFact)> ObjectAnnotations()
        {
            List<(RDFOntologyAnnotationProperty, RDFOntologyFact)> result = new List<(RDFOntologyAnnotationProperty, RDFOntologyFact)>();

            //SeeAlso
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.SeeAlso.SelectEntriesBySubject(this.OntologyFact)
                                                                                          .Where(te => te.TaxonomyObject.IsFact()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));

            //IsDefinedBy
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.IsDefinedBy.SelectEntriesBySubject(this.OntologyFact)
                                                                                              .Where(te => te.TaxonomyObject.IsFact()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));

            //Custom Annotations
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(this.OntologyFact)
                                                                                                    .Where(te => te.TaxonomyObject.IsFact()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Enlists the literal annotations which are assigned to the lens fact
        /// </summary>
        public List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> DataAnnotations()
        {
            List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> result = new List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)>();

            //VersionInfo
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.VersionInfo.SelectEntriesBySubject(this.OntologyFact)
                                                                                              .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Comment
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.Comment.SelectEntriesBySubject(this.OntologyFact)
                                                                                          .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Label
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.Label.SelectEntriesBySubject(this.OntologyFact)
                                                                                        .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //SeeAlso
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.SeeAlso.SelectEntriesBySubject(this.OntologyFact)
                                                                                          .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //IsDefinedBy
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.IsDefinedBy.SelectEntriesBySubject(this.OntologyFact)
                                                                                              .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Custom Annotations
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(this.OntologyFact)
                                                                                                    .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            return result;
        }
        #endregion
    }
}