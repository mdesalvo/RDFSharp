/*
   Copyright 2012-2022 Marco De Salvo

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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyProperty represents a property definition within an ontology model.
    /// </summary>
    public class RDFOntologyProperty : RDFOntologyResource
    {

        #region Properties
        /// <summary>
        /// Flag indicating that this ontology property is "owl:DeprecatedProperty"
        /// </summary>
        public bool Deprecated { get; internal set; }

        /// <summary>
        /// Flag indicating that this ontology property is "owl:FunctionalProperty"
        /// </summary>
        public bool Functional { get; internal set; }

        /// <summary>
        /// Domain class of the ontology property
        /// </summary>
        public RDFOntologyClass Domain { get; internal set; }

        /// <summary>
        /// Range class of the ontology property
        /// </summary>
        public RDFOntologyClass Range { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an ontology RDF property with the given non-blank name
        /// </summary>
        internal RDFOntologyProperty(RDFResource propertyName)
        {
            if (propertyName != null)
            {
                if (!propertyName.IsBlank)
                {
                    this.Value = propertyName;
                }
                else
                {
                    throw new RDFSemanticsException("Cannot create RDFOntologyProperty because given \"propertyName\" parameter is a blank resource.");
                }
            }
            else
            {
                throw new RDFSemanticsException("Cannot create RDFOntologyProperty because given \"propertyName\" parameter is null.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the domain of this ontology property to the given ontology class
        /// </summary>
        public RDFOntologyProperty SetDomain(RDFOntologyClass domainClass)
        {
            if (!this.IsAnnotationProperty())
                this.Domain = domainClass;
            return this;
        }

        /// <summary>
        /// Sets the range of this ontology property to the given ontology class
        /// </summary>
        public RDFOntologyProperty SetRange(RDFOntologyClass rangeClass)
        {
            if (!this.IsAnnotationProperty())
                this.Range = rangeClass;
            return this;
        }

        /// <summary>
        /// Sets or unsets this ontology property as "owl:FunctionalProperty"
        /// </summary>
        public RDFOntologyProperty SetFunctional(bool functional)
        {
            if (!this.IsAnnotationProperty())
                this.Functional = functional;
            return this;
        }

        /// <summary>
        /// Sets or unsets this ontology property as "owl:DeprecatedProperty"
        /// </summary>
        public RDFOntologyProperty SetDeprecated(bool deprecated)
        {
            if (!this.IsAnnotationProperty())
                this.Deprecated = deprecated;
            return this;
        }
        #endregion

    }

}