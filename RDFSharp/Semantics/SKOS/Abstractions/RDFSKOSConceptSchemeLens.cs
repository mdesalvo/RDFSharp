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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Semantics.SKOS
{
    /// <summary>
    /// RDFSKOSConceptSchemeLens represents a magnifying glass on the knowledge available for a concept within a scheme
    /// </summary>
    public class RDFSKOSConceptSchemeLens
    {
        #region Properties
        /// <summary>
        /// Concept being observed by the scheme lens
        /// </summary>
        public RDFSKOSConcept Concept { get; internal set; }

        /// <summary>
        /// Scheme being observed by the scheme lens
        /// </summary>
        public RDFSKOSConceptScheme Scheme { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a scheme lens for the given concept on the given scheme
        /// </summary>
        public RDFSKOSConceptSchemeLens(RDFSKOSConcept concept, RDFSKOSConceptScheme scheme)
        {
            if (concept != null)
            {
                if (scheme != null)
                {
                    this.Concept = concept;
                    this.Scheme = scheme;
                }
                else
                {
                    throw new RDFSemanticsException("Cannot create scheme lens because given \"scheme\" parameter is null");
                }
            }
            else
            {
                throw new RDFSemanticsException("Cannot create scheme lens because given \"concept\" parameter is null");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Enlists the concepts which are directly (or indirectly, if inference is requested) broader to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> BroaderConcepts(bool enableInference)
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of broader concepts
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.Broader.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            //Inference-enabled discovery of broader concepts
            if (enableInference)
            {
                List<RDFSKOSConcept> broaderConcepts = RDFSKOSHelper.GetBroaderConceptsOf(this.Scheme, this.Concept).ToList();
                foreach (RDFSKOSConcept broaderConcept in broaderConcepts)
                {
                    if (!result.Any(f => f.Item2.Equals(broaderConcept)))
                        result.Add((true, broaderConcept));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are directly (or indirectly, if inference is requested) broader to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> BroaderConceptsAsync(bool enableInference)
            => Task.Run(() => BroaderConcepts(enableInference));

        /// <summary>
        /// Enlists the concepts which are directly (or indirectly, if inference is requested) broad match to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> BroadMatchConcepts(bool enableInference)
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of broad match concepts
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.BroadMatch.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            //Inference-enabled discovery of broad match concepts
            if (enableInference)
            {
                List<RDFSKOSConcept> broadMatchConcepts = RDFSKOSHelper.GetBroadMatchConceptsOf(this.Scheme, this.Concept).ToList();
                foreach (RDFSKOSConcept broadMatchConcept in broadMatchConcepts)
                {
                    if (!result.Any(f => f.Item2.Equals(broadMatchConcept)))
                        result.Add((true, broadMatchConcept));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are directly (or indirectly, if inference is requested) broad match to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> BroadMatchConceptsAsync(bool enableInference)
            => Task.Run(() => BroadMatchConcepts(enableInference));

        /// <summary>
        /// Enlists the concepts which are directly (or indirectly, if inference is requested) narrower to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> NarrowerConcepts(bool enableInference)
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of narrower concepts
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.Narrower.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            //Inference-enabled discovery of narrower concepts
            if (enableInference)
            {
                List<RDFSKOSConcept> narrowerConcepts = RDFSKOSHelper.GetNarrowerConceptsOf(this.Scheme, this.Concept).ToList();
                foreach (RDFSKOSConcept narrowerConcept in narrowerConcepts)
                {
                    if (!result.Any(f => f.Item2.Equals(narrowerConcept)))
                        result.Add((true, narrowerConcept));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are directly (or indirectly, if inference is requested) narrower to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> NarrowerConceptsAsync(bool enableInference)
            => Task.Run(() => NarrowerConcepts(enableInference));

        /// <summary>
        /// Enlists the concepts which are directly (or indirectly, if inference is requested) narrow match to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> NarrowMatchConcepts(bool enableInference)
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of narrow match concepts
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.NarrowMatch.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            //Inference-enabled discovery of narrow match concepts
            if (enableInference)
            {
                List<RDFSKOSConcept> narrowMatchConcepts = RDFSKOSHelper.GetNarrowMatchConceptsOf(this.Scheme, this.Concept).ToList();
                foreach (RDFSKOSConcept narrowMatchConcept in narrowMatchConcepts)
                {
                    if (!result.Any(f => f.Item2.Equals(narrowMatchConcept)))
                        result.Add((true, narrowMatchConcept));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are directly (or indirectly, if inference is requested) narrow match to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> NarrowMatchConceptsAsync(bool enableInference)
            => Task.Run(() => NarrowMatchConcepts(enableInference));

        /// <summary>
        /// Enlists the concepts which are directly (or indirectly, if inference is requested) close match to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> CloseMatchConcepts(bool enableInference)
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of close match concepts
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.CloseMatch.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            //Inference-enabled discovery of close match concepts
            if (enableInference)
            {
                List<RDFSKOSConcept> closeMatchConcepts = RDFSKOSHelper.GetCloseMatchConceptsOf(this.Scheme, this.Concept).ToList();
                foreach (RDFSKOSConcept closeMatchConcept in closeMatchConcepts)
                {
                    if (!result.Any(f => f.Item2.Equals(closeMatchConcept)))
                        result.Add((true, closeMatchConcept));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are directly (or indirectly, if inference is requested) close match to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> CloseMatchConceptsAsync(bool enableInference)
            => Task.Run(() => CloseMatchConcepts(enableInference));

        /// <summary>
        /// Enlists the concepts which are directly (or indirectly, if inference is requested) exact match to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> ExactMatchConcepts(bool enableInference)
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of exact match concepts
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.ExactMatch.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            //Inference-enabled discovery of exact match concepts
            if (enableInference)
            {
                List<RDFSKOSConcept> exactMatchConcepts = RDFSKOSHelper.GetExactMatchConceptsOf(this.Scheme, this.Concept).ToList();
                foreach (RDFSKOSConcept exactMatchConcept in exactMatchConcepts)
                {
                    if (!result.Any(f => f.Item2.Equals(exactMatchConcept)))
                        result.Add((true, exactMatchConcept));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are directly (or indirectly, if inference is requested) exact match to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> ExactMatchConceptsAsync(bool enableInference)
            => Task.Run(() => ExactMatchConcepts(enableInference));

        /// <summary>
        /// Enlists the concepts which are directly (or indirectly, if inference is requested) related to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> RelatedConcepts(bool enableInference)
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of related concepts
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.Related.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            //Inference-enabled discovery of related concepts
            if (enableInference)
            {
                List<RDFSKOSConcept> relatedConcepts = RDFSKOSHelper.GetRelatedConceptsOf(this.Scheme, this.Concept).ToList();
                foreach (RDFSKOSConcept relatedConcept in relatedConcepts)
                {
                    if (!result.Any(f => f.Item2.Equals(relatedConcept)))
                        result.Add((true, relatedConcept));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are directly (or indirectly, if inference is requested) related to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> RelatedConceptsAsync(bool enableInference)
            => Task.Run(() => RelatedConcepts(enableInference));

        /// <summary>
        /// Enlists the concepts which are directly (or indirectly, if inference is requested) related match to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> RelatedMatchConcepts(bool enableInference)
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of exact related concepts
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.RelatedMatch.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            //Inference-enabled discovery of related match concepts
            if (enableInference)
            {
                List<RDFSKOSConcept> relatedMatchConcepts = RDFSKOSHelper.GetRelatedMatchConceptsOf(this.Scheme, this.Concept).ToList();
                foreach (RDFSKOSConcept relatedMatchConcept in relatedMatchConcepts)
                {
                    if (!result.Any(f => f.Item2.Equals(relatedMatchConcept)))
                        result.Add((true, relatedMatchConcept));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are directly (or indirectly, if inference is requested) related match to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> RelatedMatchConceptsAsync(bool enableInference)
            => Task.Run(() => RelatedMatchConcepts(enableInference));

        /// <summary>
        /// Enlists the concepts which are mapping relation to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> MappingRelationConcepts()
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of mapping relation concepts (reasoning is not available on this taxonomy)
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.MappingRelation.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are mapping relation to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> MappingRelationConceptsAsync()
            => Task.Run(() => MappingRelationConcepts());

        /// <summary>
        /// Enlists the concepts which are semantic relation to the lens concept
        /// </summary>
        public List<(bool, RDFSKOSConcept)> SemanticRelationConcepts()
        {
            List<(bool, RDFSKOSConcept)> result = new List<(bool, RDFSKOSConcept)>();

            //First-level enlisting of semantic relation concepts (reasoning is not available on this taxonomy)
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.SemanticRelation.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((false, (RDFSKOSConcept)sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the concepts which are semantic relation to the lens concept
        /// </summary>
        public Task<List<(bool, RDFSKOSConcept)>> SemanticRelationConceptsAsync()
            => Task.Run(() => SemanticRelationConcepts());

        /// <summary>
        /// Enlists the literal notations of the lens concept
        /// </summary>
        public List<RDFOntologyLiteral> Notations()
        {
            List<RDFOntologyLiteral> result = new List<RDFOntologyLiteral>();

            //First-level enlisting of literal notations (reasoning is not available on this taxonomy)
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.Notation.SelectEntriesBySubject(this.Concept).Where(te => !te.IsInference()))
                result.Add((RDFOntologyLiteral)sf.TaxonomyObject);

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the literal notations of the lens concept
        /// </summary>
        public Task<List<RDFOntologyLiteral>> NotationsAsync()
            => Task.Run(() => Notations());

        /// <summary>
        /// Checks if the lens concept is the top concept of the lens scheme
        /// </summary>
        public bool IsTopConcept()
            => this.Scheme.Relations.TopConcept.Any(r => r.TaxonomyObject.Equals(this.Concept));

        /// <summary>
        /// Asynchronously checks if the lens concept is the top concept of the lens scheme
        /// </summary>
        public Task<bool> IsTopConceptAsync()
            => Task.Run(() => IsTopConcept());

        /// <summary>
        /// Enlists the label relations which are assigned to the lens concept (SKOS-XL)
        /// </summary>
        public List<(RDFOntologyObjectProperty, RDFSKOSLabel)> LabelRelations()
        {
            List<(RDFOntologyObjectProperty, RDFSKOSLabel)> result = new List<(RDFOntologyObjectProperty, RDFSKOSLabel)>();

            //PrefLabel
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.PrefLabel.SelectEntriesBySubject(this.Concept))
                result.Add(((RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFSKOSLabel)sf.TaxonomyObject));

            //AltLabel
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.AltLabel.SelectEntriesBySubject(this.Concept))
                result.Add(((RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFSKOSLabel)sf.TaxonomyObject));

            //HiddenLabel
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Relations.HiddenLabel.SelectEntriesBySubject(this.Concept))
                result.Add(((RDFOntologyObjectProperty)sf.TaxonomyPredicate, (RDFSKOSLabel)sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the label relations which are assigned to the lens concept (SKOS-XL)
        /// </summary>
        public Task<List<(RDFOntologyObjectProperty, RDFSKOSLabel)>> LabelRelationsAsync()
            => Task.Run(() => LabelRelations());

        /// <summary>
        /// Enlists the label annotations which are assigned to the lens concept
        /// </summary>
        public List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> LabelAnnotations()
        {
            List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> result = new List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)>();

            //PrefLabel
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.PrefLabel.SelectEntriesBySubject(this.Concept)
                                                                                     .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //AltLabel
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.AltLabel.SelectEntriesBySubject(this.Concept)
                                                                                    .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //HiddenLabel
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.HiddenLabel.SelectEntriesBySubject(this.Concept)
                                                                                       .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the label annotations which are assigned to the lens concept
        /// </summary>
        public Task<List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)>> LabelAnnotationsAsync()
            => Task.Run(() => LabelAnnotations());

        /// <summary>
        /// Enlists the documentation annotations which are assigned to the lens concept
        /// </summary>
        public List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> DocumentationAnnotations()
        {
            List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> result = new List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)>();

            //Note
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.Note.SelectEntriesBySubject(this.Concept)
                                                                                .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //ChangeNote
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.ChangeNote.SelectEntriesBySubject(this.Concept)
                                                                                      .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //EditorialNote
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.EditorialNote.SelectEntriesBySubject(this.Concept)
                                                                                         .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //HistoryNote
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.HistoryNote.SelectEntriesBySubject(this.Concept)
                                                                                       .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //ScopeNote
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.ScopeNote.SelectEntriesBySubject(this.Concept)
                                                                                     .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Definition
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.Definition.SelectEntriesBySubject(this.Concept)
                                                                                      .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Example
            foreach (RDFOntologyTaxonomyEntry sf in this.Scheme.Annotations.Example.SelectEntriesBySubject(this.Concept)
                                                                                   .Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the documentation annotations which are assigned to the lens concept
        /// </summary>
        public Task<List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)>> DocumentationAnnotationsAsync()
            => Task.Run(() => DocumentationAnnotations());
        #endregion
    }
}