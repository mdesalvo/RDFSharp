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

using RDFSharp.Query;
using System;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyResource represents a generic resource definition within an ontology.
    /// </summary>
    public class RDFOntologyResource : RDFPatternMember
    {

        #region Properties
        /// <summary>
        /// Value of the ontology resource
        /// </summary>
        public RDFPatternMember Value { get; internal set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the ontology resource
        /// </summary>
        public override string ToString()
        {
            return this.Value.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if this ontology resource represents an ontology class
        /// </summary>
        internal bool IsClass()
        {
            return (this is RDFOntologyClass);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology deprecated class
        /// </summary>
        internal bool IsDeprecatedClass()
        {
            return (this is RDFOntologyClass && ((RDFOntologyClass)this).Deprecated);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology restriction class
        /// </summary>
        internal bool IsRestrictionClass()
        {
            return (this is RDFOntologyRestriction);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology composite class (union/intersection/complement)
        /// </summary>
        internal bool IsCompositeClass()
        {
            return (this is RDFOntologyUnionClass ||
                    this is RDFOntologyIntersectionClass ||
                    this is RDFOntologyComplementClass);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology enumerate class
        /// </summary>
        internal bool IsEnumerateClass()
        {
            return (this is RDFOntologyEnumerateClass);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology datarange class
        /// </summary>
        internal bool IsDataRangeClass()
        {
            return (this is RDFOntologyDataRangeClass);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology property
        /// </summary>
        internal bool IsProperty()
        {
            return (this is RDFOntologyProperty);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology deprecated property
        /// </summary>
        internal bool IsDeprecatedProperty()
        {
            return (this is RDFOntologyProperty && ((RDFOntologyProperty)this).Deprecated);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology functional property
        /// </summary>
        internal bool IsFunctionalProperty()
        {
            return (this is RDFOntologyProperty && ((RDFOntologyProperty)this).Functional);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology symmetric property
        /// </summary>
        internal bool IsSymmetricProperty()
        {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).Symmetric);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology asymmetric property [OWL2]
        /// </summary>
        internal bool IsAsymmetricProperty()
        {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).Asymmetric);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology reflexive property [OWL2]
        /// </summary>
        internal bool IsReflexiveProperty()
        {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).Reflexive);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology irreflexive property [OWL2]
        /// </summary>
        internal bool IsIrreflexiveProperty()
        {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).Irreflexive);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology transitive property
        /// </summary>
        internal bool IsTransitiveProperty()
        {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).Transitive);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology inverse functional property
        /// </summary>
        internal bool IsInverseFunctionalProperty()
        {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).InverseFunctional);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology annotation property
        /// </summary>
        internal bool IsAnnotationProperty()
        {
            return (this is RDFOntologyAnnotationProperty);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology datatype property
        /// </summary>
        internal bool IsDatatypeProperty()
        {
            return (this is RDFOntologyDatatypeProperty);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology object property
        /// </summary>
        internal bool IsObjectProperty()
        {
            return (this is RDFOntologyObjectProperty);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology fact
        /// </summary>
        internal bool IsFact()
        {
            return (this is RDFOntologyFact);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology literal
        /// </summary>
        internal bool IsLiteral()
        {
            return (this is RDFOntologyLiteral);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology
        /// </summary>
        internal bool IsOntology()
        {
            return (this is RDFOntology);
        }
        #endregion

    }

}