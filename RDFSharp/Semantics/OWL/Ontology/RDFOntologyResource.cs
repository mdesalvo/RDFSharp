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
        public override string ToString() => this.Value.ToString();
        #endregion

        #region Methods
        /// <summary>
        /// Checks if this ontology resource represents an ontology class
        /// </summary>
        internal bool IsClass() => this is RDFOntologyClass;

        /// <summary>
        /// Checks if this ontology resource represents an ontology deprecated class
        /// </summary>
        internal bool IsDeprecatedClass() => this is RDFOntologyClass ontClass && ontClass.Deprecated;

        /// <summary>
        /// Checks if this ontology resource represents an ontology restriction class
        /// </summary>
        internal bool IsRestrictionClass() => this is RDFOntologyRestriction;

        /// <summary>
        /// Checks if this ontology resource represents an ontology composite class (union/intersection/complement)
        /// </summary>
        internal bool IsCompositeClass() => this is RDFOntologyUnionClass ||
                                                this is RDFOntologyIntersectionClass ||
                                                    this is RDFOntologyComplementClass;

        /// <summary>
        /// Checks if this ontology resource represents an ontology enumerate class
        /// </summary>
        internal bool IsEnumerateClass() => this is RDFOntologyEnumerateClass;

        /// <summary>
        /// Checks if this ontology resource represents an ontology datarange class
        /// </summary>
        internal bool IsDataRangeClass() => this is RDFOntologyDataRangeClass;

        /// <summary>
        /// CHecks if this ontology resource represents a simple ontology class
        /// </summary>
        internal bool IsSimpleClass() => this.IsClass() &&
                                            !this.IsRestrictionClass() &&
                                                !this.IsCompositeClass() &&
                                                    !this.IsEnumerateClass() &&
                                                        !this.IsDataRangeClass();

        /// <summary>
        /// Checks if this ontology resource represents an ontology property
        /// </summary>
        internal bool IsProperty() => this is RDFOntologyProperty;

        /// <summary>
        /// Checks if this ontology resource represents an ontology deprecated property
        /// </summary>
        internal bool IsDeprecatedProperty() => this is RDFOntologyProperty ontProp && ontProp.Deprecated;

        /// <summary>
        /// Checks if this ontology resource represents an ontology functional property
        /// </summary>
        internal bool IsFunctionalProperty() => this is RDFOntologyProperty ontProp && ontProp.Functional;

        /// <summary>
        /// Checks if this ontology resource represents an ontology symmetric property
        /// </summary>
        internal bool IsSymmetricProperty() => this is RDFOntologyObjectProperty ontObjProp && ontObjProp.Symmetric;

        /// <summary>
        /// Checks if this ontology resource represents an ontology asymmetric property [OWL2]
        /// </summary>
        internal bool IsAsymmetricProperty() => this is RDFOntologyObjectProperty ontObjProp && ontObjProp.Asymmetric;

        /// <summary>
        /// Checks if this ontology resource represents an ontology reflexive property [OWL2]
        /// </summary>
        internal bool IsReflexiveProperty() => this is RDFOntologyObjectProperty ontObjProp && ontObjProp.Reflexive;

        /// <summary>
        /// Checks if this ontology resource represents an ontology irreflexive property [OWL2]
        /// </summary>
        internal bool IsIrreflexiveProperty() => this is RDFOntologyObjectProperty ontObjProp && ontObjProp.Irreflexive;

        /// <summary>
        /// Checks if this ontology resource represents an ontology transitive property
        /// </summary>
        internal bool IsTransitiveProperty() => this is RDFOntologyObjectProperty ontObjProp && ontObjProp.Transitive;

        /// <summary>
        /// Checks if this ontology resource represents an ontology inverse functional property
        /// </summary>
        internal bool IsInverseFunctionalProperty() => this is RDFOntologyObjectProperty ontObjProp && ontObjProp.InverseFunctional;

        /// <summary>
        /// Checks if this ontology resource represents an ontology annotation property
        /// </summary>
        internal bool IsAnnotationProperty() => this is RDFOntologyAnnotationProperty;

        /// <summary>
        /// Checks if this ontology resource represents an ontology datatype property
        /// </summary>
        internal bool IsDatatypeProperty() => this is RDFOntologyDatatypeProperty;

        /// <summary>
        /// Checks if this ontology resource represents an ontology object property
        /// </summary>
        internal bool IsObjectProperty() => this is RDFOntologyObjectProperty;

        /// <summary>
        /// Checks if this ontology resource represents an ontology fact
        /// </summary>
        internal bool IsFact() => this is RDFOntologyFact;

        /// <summary>
        /// Checks if this ontology resource represents an ontology literal
        /// </summary>
        internal bool IsLiteral() => this is RDFOntologyLiteral;

        /// <summary>
        /// Checks if this ontology resource represents an ontology
        /// </summary>
        internal bool IsOntology() => this is RDFOntology;
        #endregion

    }

}