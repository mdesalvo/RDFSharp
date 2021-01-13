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
        /// Enlists the facts which are directly (or even indirectly, if inference is enabled) equivalent to the lens fact
        /// </summary>
        public List<RDFOntologyFact> SameAs(bool enableInference)
        {
            List<RDFOntologyFact> result = new List<RDFOntologyFact>();

            if (enableInference)
            {
                //Inference-enabled discovery of equivalent facts
                result.AddRange(RDFOntologyHelper.GetSameFactsAs(this.Ontology.Data, this.Fact));
            }
            else
            {
                //First-level enlisting of equivalent facts (light reasoning on relation simmetry)
                foreach (var sf in this.Ontology.Data.Relations.SameAs.SelectEntriesBySubject(this.Fact)
                                                                      .Where(te => te.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                {
                    result.Add((RDFOntologyFact)sf.TaxonomyObject);
                }
                foreach (var sf in this.Ontology.Data.Relations.SameAs.SelectEntriesByObject(this.Fact)
                                                                      .Where(te => te.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                {
                    result.Add((RDFOntologyFact)sf.TaxonomySubject);
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the facts which are directly (or even indirectly, if inference is enabled) different from the lens fact
        /// </summary>
        public List<RDFOntologyFact> DifferentFrom(bool enableInference)
        {
            List<RDFOntologyFact> result = new List<RDFOntologyFact>();

            if (enableInference)
            {
                //Inference-enabled discovery of different facts
                result.AddRange(RDFOntologyHelper.GetDifferentFactsFrom(this.Ontology.Data, this.Fact));
            }
            else
            {
                //First-level enlisting of different facts (light reasoning on relation simmetry)
                foreach (var sf in this.Ontology.Data.Relations.DifferentFrom.SelectEntriesBySubject(this.Fact)
                                                                             .Where(te => te.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                {
                    result.Add((RDFOntologyFact)sf.TaxonomyObject);
                }
                foreach (var sf in this.Ontology.Data.Relations.DifferentFrom.SelectEntriesByObject(this.Fact)
                                                                             .Where(te => te.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                {
                    result.Add((RDFOntologyFact)sf.TaxonomySubject);
                }
            }

            return result;
        }

        /// <summary>
        /// Enlists the object assertions which are directly (or even indirectly, if inference is enabled) assigned to the lens fact
        /// </summary>
        public List<(RDFOntologyObjectProperty,RDFOntologyFact)> ObjectAssertions(bool enableInference)
        {
            List<(RDFOntologyObjectProperty, RDFOntologyFact)> result = new List<(RDFOntologyObjectProperty, RDFOntologyFact)>();

            if (enableInference)
            {
                //Inference-enabled discovery of assigned relations
                foreach (var sf in this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(this.Fact))
                {
                    result.Add(((RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
                }
            }
            else
            {
                //First-level enlisting of assigne relations
                foreach (var sf in this.Ontology.Data.Relations.Assertions.SelectEntriesBySubject(this.Fact)
                                                                          .Where(te => te.InferenceType != RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner))
                {
                    result.Add(((RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFOntologyFact)sf.TaxonomyObject));
                }
            }

            return result;
        }
        #endregion
    }
}