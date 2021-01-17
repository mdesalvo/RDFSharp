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
        /// Initializes the lens ontology with support for SKOS ontology
        /// </summary>
        public RDFOntologyPropertyModelLens InitializeSKOS()
        {
            this.Ontology = this.Ontology.UnionWith(RDFSKOSOntology.Instance);
            return this;
        }

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
        #endregion
    }
}