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
        public RDFOntologyFact Fact { get; internal set; }

        /// <summary>
        /// Ontology being observed by the data lens
        /// </summary>
        public RDFOntology Ontology { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a data lens for the given fact on the given ontology
        /// </summary>
        public RDFOntologyDataLens(RDFOntologyFact fact, RDFOntology ontology)
        {
            if (fact != null)
            {
                if (ontology != null)
                {
                    this.Fact = fact;
                    this.Ontology = ontology;
                }
                else
                {
                    throw new RDFSemanticsException("Cannot create data lens because given \"ontology\" parameter is null");
                }
            }
            else
            {
                throw new RDFSemanticsException("Cannot create data lens because given \"fact\" parameter is null");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Enlists the facts which are directly (or indirectly, if inference is requested) equivalent to the lens fact
        /// </summary>
        public List<(bool, RDFOntologyFact)> SameAs(bool enableInference)
        {
            List<(bool, RDFOntologyFact)> result = new List<(bool, RDFOntologyFact)>();

            //First-level enlisting of same facts
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.SameAs.SelectEntriesBySubject(this.Fact)
                                                                                       .Where(te => te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
            {
                result.Add((false, (RDFOntologyFact)sf.TaxonomyObject));
            }

            //Inference-enabled discovery of same facts
            if (enableInference)
            {
                List<RDFOntologyFact> sameFacts = RDFOntologyHelper.GetSameFactsAs(this.Ontology.Data, this.Fact).ToList();
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
        public List<(bool, RDFOntologyFact)> DifferentFrom(bool enableInference)
        {
            List<(bool, RDFOntologyFact)> result = new List<(bool, RDFOntologyFact)>();

            //First-level enlisting of different facts
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.DifferentFrom.SelectEntriesBySubject(this.Fact)
                                                                                              .Where(te => te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
            {
                result.Add((false, (RDFOntologyFact)sf.TaxonomyObject));
            }

            //Inference-enabled discovery of different facts
            if (enableInference)
            {
                List<RDFOntologyFact> differentFacts = RDFOntologyHelper.GetDifferentFactsFrom(this.Ontology.Data, this.Fact).ToList();
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
        public List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)> ObjectProperties(bool enableInference)
        {
            List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)> result = new List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)>();

            if (enableInference)
            {
                //Inference-enabled discovery of assigned object relations
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(this.Fact)
                                                                          .Where(te => te.TaxonomyPredicate is RDFOntologyObjectProperty))
                {
                    result.Add((sf.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.None, (RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
                }
            }
            else
            {
                //First-level enlisting of assigned object relations
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(this.Fact)
                                                                          .Where(te => te.TaxonomyPredicate is RDFOntologyObjectProperty
                                                                                            && te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
                {
                    result.Add((false, (RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the negative object assertions which are directly (or indirectly, if inference is requested) assigned to the lens fact
        /// </summary>
        public List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)> NegativeObjectProperties(bool enableInference)
        {
            List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)> result = new List<(bool, RDFOntologyObjectProperty, RDFOntologyFact)>();

            if (enableInference)
            {
                //Inference-enabled discovery of assigned object negative relations
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.NegativeAssertions.SelectEntriesBySubject(this.Fact)
                                                                                  .Where(te => te.TaxonomyPredicate is RDFOntologyObjectProperty))
                {
                    result.Add((sf.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.None, (RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
                }
            }
            else
            {
                //First-level enlisting of assigned object negative relations
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.NegativeAssertions.SelectEntriesBySubject(this.Fact)
                                                                                  .Where(te => te.TaxonomyPredicate is RDFOntologyObjectProperty
                                                                                                    && te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
                {
                    result.Add((false, (RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the data assertions which are directly (or indirectly, if inference is requested) assigned to the lens fact
        /// </summary>
        public List<(bool,RDFOntologyDatatypeProperty, RDFOntologyLiteral)> DataProperties(bool enableInference)
        {
            List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)> result = new List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)>();

            if (enableInference)
            {
                //Inference-enabled discovery of assigned literal relations
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(this.Fact)
                                                                          .Where(te => te.TaxonomyPredicate is RDFOntologyDatatypeProperty))
                {
                    result.Add((sf.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.None, (RDFOntologyDatatypeProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));
                }
            }
            else
            {
                //First-level enlisting of assigned literal relations
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(this.Fact)
                                                                          .Where(te => te.TaxonomyPredicate is RDFOntologyDatatypeProperty
                                                                                            && te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
                {
                    result.Add((false, (RDFOntologyDatatypeProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the negative data assertions which are directly (or indirectly, if inference is requested) assigned to the lens fact
        /// </summary>
        public List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)> NegativeDataProperties(bool enableInference)
        {
            List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)> result = new List<(bool, RDFOntologyDatatypeProperty, RDFOntologyLiteral)>();

            if (enableInference)
            {
                //Inference-enabled discovery of assigned literal negative relations
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.NegativeAssertions.SelectEntriesBySubject(this.Fact)
                                                                                  .Where(te => te.TaxonomyPredicate is RDFOntologyDatatypeProperty))
                {
                    result.Add((sf.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.None, (RDFOntologyDatatypeProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));
                }
            }
            else
            {
                //First-level enlisting of assigned literal negative relations
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Relations.NegativeAssertions.SelectEntriesBySubject(this.Fact)
                                                                                  .Where(te => te.TaxonomyPredicate is RDFOntologyDatatypeProperty
                                                                                                    && te.InferenceType == RDFSemanticsEnums.RDFOntologyInferenceType.None))
                {
                    result.Add((false, (RDFOntologyDatatypeProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));
                }
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
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.SeeAlso.SelectEntriesBySubject(this.Fact)
                                                                     .Where(te => te.TaxonomyObject is RDFOntologyFact))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));

            //IsDefinedBy
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.IsDefinedBy.SelectEntriesBySubject(this.Fact)
                                                                         .Where(te => te.TaxonomyObject is RDFOntologyFact))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));

            //Custom Annotations
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(this.Fact)
                                                                               .Where(te => te.TaxonomyObject is RDFOntologyFact))
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
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.VersionInfo.SelectEntriesBySubject(this.Fact)
                                                                         .Where(te => te.TaxonomyObject is RDFOntologyLiteral))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Comment
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.Comment.SelectEntriesBySubject(this.Fact)
                                                                     .Where(te => te.TaxonomyObject is RDFOntologyLiteral))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Label
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.Label.SelectEntriesBySubject(this.Fact)
                                                                   .Where(te => te.TaxonomyObject is RDFOntologyLiteral))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //SeeAlso
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.SeeAlso.SelectEntriesBySubject(this.Fact)
                                                                     .Where(te => te.TaxonomyObject is RDFOntologyLiteral))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //IsDefinedBy
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.IsDefinedBy.SelectEntriesBySubject(this.Fact)
                                                                         .Where(te => te.TaxonomyObject is RDFOntologyLiteral))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Custom Annotations
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Data.Annotations.CustomAnnotations.SelectEntriesBySubject(this.Fact)
                                                                               .Where(te => te.TaxonomyObject is RDFOntologyLiteral))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            return result;
        }
        #endregion
    }
}