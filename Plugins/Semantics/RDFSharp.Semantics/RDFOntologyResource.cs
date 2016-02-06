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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFOntologyResource represents a generic resource definition within an ontology.
    /// </summary>
    public class RDFOntologyResource: RDFPatternMember {

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
        public override String ToString() {
            return this.Value.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks if this ontology resource represents an ontology class
        /// </summary>
        public Boolean IsClass() {
            return (this is RDFOntologyClass);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology deprecated class
        /// </summary>
        public Boolean IsDeprecatedClass() {
            return (this is RDFOntologyClass && ((RDFOntologyClass)this).Deprecated);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology restriction class
        /// </summary>
        public Boolean IsRestrictionClass() {
            return (this is RDFOntologyRestriction);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology composite class (union/intersection/complement)
        /// </summary>
        public Boolean IsCompositeClass() {
            return (this is RDFOntologyUnionClass        ||
                    this is RDFOntologyIntersectionClass ||
                    this is RDFOntologyComplementClass);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology enumerate class
        /// </summary>
        public Boolean IsEnumerateClass() {
            return (this is RDFOntologyEnumerateClass);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology datarange class
        /// </summary>
        public Boolean IsDataRangeClass() {
            return (this is RDFOntologyDataRangeClass);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology property
        /// </summary>
        public Boolean IsProperty() {
            return (this is RDFOntologyProperty);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology deprecated property
        /// </summary>
        public Boolean IsDeprecatedProperty() {
            return (this is RDFOntologyProperty && ((RDFOntologyProperty)this).Deprecated);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology functional property
        /// </summary>
        public Boolean IsFunctionalProperty() {
            return (this is RDFOntologyProperty && ((RDFOntologyProperty)this).Functional);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology symmetric property
        /// </summary>
        public Boolean IsSymmetricProperty() {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).Symmetric);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology transitive property
        /// </summary>
        public Boolean IsTransitiveProperty() {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).Transitive);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology inverse functional property
        /// </summary>
        public Boolean IsInverseFunctionalProperty() {
            return (this is RDFOntologyObjectProperty && ((RDFOntologyObjectProperty)this).InverseFunctional);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology annotation property
        /// </summary>
        public Boolean IsAnnotationProperty() {
            return (this is RDFOntologyAnnotationProperty);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology datatype property
        /// </summary>
        public Boolean IsDatatypeProperty() {
            return (this is RDFOntologyDatatypeProperty);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology object property
        /// </summary>
        public Boolean IsObjectProperty() {
            return (this is RDFOntologyObjectProperty);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology fact
        /// </summary>
        public Boolean IsFact() {
            return (this is RDFOntologyFact);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology literal
        /// </summary>
        public Boolean IsLiteral() {
            return (this is RDFOntologyLiteral);
        }

        /// <summary>
        /// Checks if this ontology resource represents an ontology
        /// </summary>
        public Boolean IsOntology() {
            return (this is RDFOntology);
        }
        #endregion

    }

}