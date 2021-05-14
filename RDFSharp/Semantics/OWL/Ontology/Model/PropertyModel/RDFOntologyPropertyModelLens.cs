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
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyPropertyModelLens represents a magnifying glass on the knowledge available for a property within an ontology
    /// </summary>
    public class RDFOntologyPropertyModelLens
    {
        #region Properties
        /// <summary>
        /// Property being observed by the property model lens
        /// </summary>
        public RDFOntologyProperty OntologyProperty { get; internal set; }

        /// <summary>
        /// Ontology being observed by the property model lens
        /// </summary>
        public RDFOntology Ontology { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a property model lens for the given property on the given ontology
        /// </summary>
        public RDFOntologyPropertyModelLens(RDFOntologyProperty ontologyProperty, RDFOntology ontology)
        {
            if (ontologyProperty != null)
            {
                if (ontology != null)
                {
                    this.OntologyProperty = ontologyProperty;
                    this.Ontology = ontology.UnionWith(RDFBASEOntology.Instance);
                }
                else
                {
                    throw new RDFSemanticsException("Cannot create property model lens because given \"ontology\" parameter is null");
                }
            }
            else
            {
                throw new RDFSemanticsException("Cannot create property model lens because given \"ontologyProperty\" parameter is null");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Enlists the properties which are directly (or indirectly, if inference is requested) children of the lens property
        /// </summary>
        public List<(bool, RDFOntologyProperty)> SubProperties(bool enableInference)
        {
            List<(bool, RDFOntologyProperty)> result = new List<(bool, RDFOntologyProperty)>();

            //First-level enlisting of subproperties
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Relations.SubPropertyOf.SelectEntriesByObject(this.OntologyProperty).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyProperty)sf.TaxonomySubject));

            //Inference-enabled discovery of subproperties
            if (enableInference)
            {
                List<RDFOntologyProperty> subProperties = RDFOntologyHelper.GetSubPropertiesOf(this.Ontology.Model.PropertyModel, this.OntologyProperty).ToList();
                foreach (RDFOntologyProperty subProperty in subProperties)
                {
                    if (!result.Any(f => f.Item2.Equals(subProperty)))
                        result.Add((true, subProperty));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the properties which are directly (or indirectly, if inference is requested) children of the lens property
        /// </summary>
        public Task<List<(bool, RDFOntologyProperty)>> SubPropertiesAsync(bool enableInference)
            => Task.Run(() => SubProperties(enableInference));

        /// <summary>
        /// Enlists the properties which are directly (or indirectly, if inference is requested) parent of the lens property
        /// </summary>
        public List<(bool, RDFOntologyProperty)> SuperProperties(bool enableInference)
        {
            List<(bool, RDFOntologyProperty)> result = new List<(bool, RDFOntologyProperty)>();

            //First-level enlisting of superproperties
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Relations.SubPropertyOf.SelectEntriesBySubject(this.OntologyProperty).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyProperty)sf.TaxonomyObject));

            //Inference-enabled discovery of superproperties
            if (enableInference)
            {
                List<RDFOntologyProperty> superProperties = RDFOntologyHelper.GetSuperPropertiesOf(this.Ontology.Model.PropertyModel, this.OntologyProperty).ToList();
                foreach (RDFOntologyProperty superProperty in superProperties)
                {
                    if (!result.Any(f => f.Item2.Equals(superProperty)))
                        result.Add((true, superProperty));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the properties which are directly (or indirectly, if inference is requested) parent of the lens property
        /// </summary>
        public Task<List<(bool, RDFOntologyProperty)>> SuperPropertiesAsync(bool enableInference)
            => Task.Run(() => SuperProperties(enableInference));

        /// <summary>
        /// Enlists the properties which are directly (or indirectly, if inference is requested) equivalent to the lens property
        /// </summary>
        public List<(bool, RDFOntologyProperty)> EquivalentProperties(bool enableInference)
        {
            List<(bool, RDFOntologyProperty)> result = new List<(bool, RDFOntologyProperty)>();

            //First-level enlisting of equivalent properties
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Relations.EquivalentProperty.SelectEntriesBySubject(this.OntologyProperty).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyProperty)sf.TaxonomyObject));

            //Inference-enabled discovery of equivalent properties
            if (enableInference)
            {
                List<RDFOntologyProperty> equivalentProperties = RDFOntologyHelper.GetEquivalentPropertiesOf(this.Ontology.Model.PropertyModel, this.OntologyProperty).ToList();
                foreach (RDFOntologyProperty equivalentProperty in equivalentProperties)
                {
                    if (!result.Any(f => f.Item2.Equals(equivalentProperty)))
                        result.Add((true, equivalentProperty));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the properties which are directly (or indirectly, if inference is requested) equivalent to the lens property
        /// </summary>
        public Task<List<(bool, RDFOntologyProperty)>> EquivalentPropertiesAsync(bool enableInference)
            => Task.Run(() => EquivalentProperties(enableInference));

        /// <summary>
        /// Enlists the properties which are directly (or indirectly, if inference is requested) inverse of the lens property
        /// </summary>
        public List<(bool, RDFOntologyObjectProperty)> InverseProperties(bool enableInference)
        {
            List<(bool, RDFOntologyObjectProperty)> result = new List<(bool, RDFOntologyObjectProperty)>();

            if (this.OntologyProperty.IsObjectProperty())
            {
                //First-level enlisting of inverse properties
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Relations.InverseOf.SelectEntriesBySubject(this.OntologyProperty).Where(te => !te.IsInference()))
                    result.Add((false, (RDFOntologyObjectProperty)sf.TaxonomyObject));

                //Inference-enabled discovery of inverse properties
                if (enableInference)
                {
                    List<RDFOntologyObjectProperty> inverseProperties = RDFOntologyHelper.GetInversePropertiesOf(this.Ontology.Model.PropertyModel, (RDFOntologyObjectProperty)this.OntologyProperty)
                                                                                         .OfType<RDFOntologyObjectProperty>()
                                                                                         .ToList();
                    foreach (RDFOntologyObjectProperty inverseProperty in inverseProperties)
                    {
                        if (!result.Any(f => f.Item2.Equals(inverseProperty)))
                            result.Add((true, inverseProperty));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the properties which are directly (or indirectly, if inference is requested) inverse of the lens property
        /// </summary>
        public Task<List<(bool, RDFOntologyObjectProperty)>> InversePropertiesAsync(bool enableInference)
            => Task.Run(() => InverseProperties(enableInference));

        /// <summary>
        /// Enlists the properties which are directly (or indirectly, if inference is requested) disjoint with the lens property
        /// </summary>
        public List<(bool, RDFOntologyProperty)> DisjointProperties(bool enableInference)
        {
            List<(bool, RDFOntologyProperty)> result = new List<(bool, RDFOntologyProperty)>();

            //First-level enlisting of disjoint properties
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Relations.PropertyDisjointWith.SelectEntriesBySubject(this.OntologyProperty).Where(te => !te.IsInference()))
                result.Add((false, (RDFOntologyProperty)sf.TaxonomyObject));

            //Inference-enabled discovery of disjoint properties
            if (enableInference)
            {
                List<RDFOntologyProperty> disjointProperties = RDFOntologyHelper.GetPropertiesDisjointWith(this.Ontology.Model.PropertyModel, this.OntologyProperty).ToList();
                foreach (RDFOntologyProperty disjointProperty in disjointProperties)
                {
                    if (!result.Any(f => f.Item2.Equals(disjointProperty)))
                        result.Add((true, disjointProperty));
                }
            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the properties which are directly (or indirectly, if inference is requested) disjoint with the lens property
        /// </summary>
        public Task<List<(bool, RDFOntologyProperty)>> DisjointPropertiesAsync(bool enableInference)
            => Task.Run(() => DisjointProperties(enableInference));

        /// <summary>
        /// Enlists the properties which are chain axioms of the lens property
        /// </summary>
        public List<(bool, RDFOntologyObjectProperty)> ChainAxioms()
        {
            List<(bool, RDFOntologyObjectProperty)> result = new List<(bool, RDFOntologyObjectProperty)>();

            if (this.OntologyProperty.IsObjectProperty())
            {
                //First-level enlisting of chain axioms (reasoning is not available on this concept)
                foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Relations.PropertyChainAxiom.SelectEntriesBySubject(this.OntologyProperty).Where(te => !te.IsInference()))
                    result.Add((false, (RDFOntologyObjectProperty)sf.TaxonomyObject));

            }

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the properties which are chain axioms of the lens property
        /// </summary>
        public Task<List<(bool, RDFOntologyObjectProperty)>> ChainAxiomsAsync()
            => Task.Run(() => ChainAxioms());

        /// <summary>
        /// Enlists the object annotations which are assigned to the lens property
        /// </summary>
        public List<(RDFOntologyAnnotationProperty, RDFOntologyResource)> ObjectAnnotations()
        {
            List<(RDFOntologyAnnotationProperty, RDFOntologyResource)> result = new List<(RDFOntologyAnnotationProperty, RDFOntologyResource)>();

            //SeeAlso
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.SeeAlso.SelectEntriesBySubject(this.OntologyProperty).Where(te => !te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, sf.TaxonomyObject));

            //IsDefinedBy
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(this.OntologyProperty).Where(te => !te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, sf.TaxonomyObject));

            //Custom Annotations
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(this.OntologyProperty).Where(te => !te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the object annotations which are assigned to the lens property
        /// </summary>
        public Task<List<(RDFOntologyAnnotationProperty, RDFOntologyResource)>> ObjectAnnotationsAsync()
            => Task.Run(() => ObjectAnnotations());

        /// <summary>
        /// Enlists the literal annotations which are assigned to the lens property
        /// </summary>
        public List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> DataAnnotations()
        {
            List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)> result = new List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)>();

            //VersionInfo
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.VersionInfo.SelectEntriesBySubject(this.OntologyProperty).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Comment
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.Comment.SelectEntriesBySubject(this.OntologyProperty).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Label
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.Label.SelectEntriesBySubject(this.OntologyProperty).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //SeeAlso
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.SeeAlso.SelectEntriesBySubject(this.OntologyProperty).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //IsDefinedBy
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.IsDefinedBy.SelectEntriesBySubject(this.OntologyProperty).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            //Custom Annotations
            foreach (RDFOntologyTaxonomyEntry sf in this.Ontology.Model.PropertyModel.Annotations.CustomAnnotations.SelectEntriesBySubject(this.OntologyProperty).Where(te => te.TaxonomyObject.IsLiteral()))
                result.Add(((RDFOntologyAnnotationProperty)sf.TaxonomyPredicate, (RDFOntologyLiteral)sf.TaxonomyObject));

            return result;
        }

        /// <summary>
        /// Asynchronously enlists the literal annotations which are assigned to the lens property
        /// </summary>
        public Task<List<(RDFOntologyAnnotationProperty, RDFOntologyLiteral)>> DataAnnotationsAsync()
            => Task.Run(() => DataAnnotations());
        #endregion
    }
}