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
        #endregion
    }
}