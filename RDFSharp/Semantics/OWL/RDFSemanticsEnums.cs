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

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFSemanticsEnums represents a collector for all the enumerations used by the "RDFSharp.Semantics" namespace
    /// </summary>
    public static class RDFSemanticsEnums
    {

        /// <summary>
        /// RDFOntologyStandardAnnotation represents an enumeration for predefined ontology annotation properties
        /// </summary>
        public enum RDFOntologyStandardAnnotation
        {
            /// <summary>
            /// owl:versionInfo
            /// </summary>
            VersionInfo = 1,
            /// <summary>
            /// owl:versionIRI
            /// </summary>
            VersionIRI = 2,
            /// <summary>
            /// rdfs:comment
            /// </summary>
            Comment = 3,
            /// <summary>
            /// rdfs:label
            /// </summary>
            Label = 4,
            /// <summary>
            /// rdfs:seeAlso
            /// </summary>
            SeeAlso = 5,
            /// <summary>
            /// rdfs:isDefinedBy
            /// </summary>
            IsDefinedBy = 6,
            /// <summary>
            /// owl:imports
            /// </summary>
            Imports = 7,
            /// <summary>
            /// owl:BackwardCompatibleWith
            /// </summary>
            BackwardCompatibleWith = 8,
            /// <summary>
            /// owl:IncompatibleWith
            /// </summary>
            IncompatibleWith = 9,
            /// <summary>
            /// owl:PriorVersion
            /// </summary>
            PriorVersion = 10
        }

        /// <summary>
        /// RDFOntologyValidatorEvidenceCategory represents an enumeration for possible categories of ontology validation evidence
        /// </summary>
        public enum RDFOntologyValidatorEvidenceCategory
        {
            /// <summary>
            /// Specifications have not been violated: ontology may contain semantic inconsistencies
            /// </summary>
            Warning = 1,
            /// <summary>
            /// Specifications have been violated: ontology will contain semantic inconsistencies
            /// </summary>
            Error = 2
        };

        /// <summary>
        /// RDFOntologyReasonerEvidenceCategory represents an enumeration for possible categories of ontology reasoner evidence
        /// </summary>
        public enum RDFOntologyReasonerEvidenceCategory
        {
            /// <summary>
            /// Semantic inference has been generated within the ontology class model
            /// </summary>
            ClassModel = 1,
            /// <summary>
            /// Semantic inference has been generated within the ontology property model
            /// </summary>
            PropertyModel = 2,
            /// <summary>
            /// Semantic inference has been generated within the ontology data
            /// </summary>
            Data = 3
        };

        /// <summary>
        /// RDFOntologyInferenceType represents an enumeration for possible types of a semantic inference
        /// </summary>
        public enum RDFOntologyInferenceType
        {
            /// <summary>
            /// Not a semantic inference (reserved to RDFSharp)
            /// </summary>
            None = 0,
            /// <summary>
            /// Semantic inference generated during ontology modeling (reserved to RDFSharp)
            /// </summary>
            API = 1,
            /// <summary>
            /// Semantic inference generated during ontology reasoning
            /// </summary>
            Reasoner = 2
        };

        /// <summary>
        /// RDFOntologyInferenceExportBehavior represents an enumeration for supported inference export behaviors
        /// </summary>
        public enum RDFOntologyInferenceExportBehavior
        {
            /// <summary>
            /// Does not export any semantic inferences
            /// </summary>
            None = 0,
            /// <summary>
            /// Exports only semantic inferences of ontology model
            /// </summary>
            OnlyModel = 1,
            /// <summary>
            /// Exports only semantic inferences of ontology data
            /// </summary>
            OnlyData = 2,
            /// <summary>
            /// Exports both semantic inferences of ontology model and ontology data
            /// </summary>
            ModelAndData = 3
        };

        /// <summary>
        /// RDFOntologyTaxonomyCategory represents an enumeration for supported types of taxonomy
        /// </summary>
        public enum RDFOntologyTaxonomyCategory
        {
            /// <summary>
            /// Annotation taxonomy
            /// </summary>
            Annotation = 0,
            /// <summary>
            /// Model taxonomy
            /// </summary>
            Model = 1,
            /// <summary>
            /// Data taxonomy
            /// </summary>
            Data = 2
        };

        /// <summary>
        /// RDFOntologyClassNature represents an enumeration for possible nature of an ontology class (RDFS/OWL)
        /// </summary>
        public enum RDFOntologyClassNature
        {
            /// <summary>
            /// rdfs:Class
            /// </summary>
            RDFS = 0,
            /// <summary>
            /// owl:Class
            /// </summary>
            OWL = 1
        }

        /// <summary>
        /// RDFOntologyStandardReasonerRule represents an enumeration for available standard RDFS/OWL-DL/OWL2 reasoner rules
        /// </summary>
        public enum RDFOntologyStandardReasonerRule
        {
            /// <summary>
            /// SUBCLASS(C1,C2) ^ SUBCLASS(C2,C3) -> SUBCLASS(C1,C3)<br/>
            /// SUBCLASS(C1,C2) ^ EQUIVALENTCLASS(C2,C3) -> SUBCLASS(C1,C3)<br/>
            /// EQUIVALENTCLASS(C1,C2) ^ SUBCLASSOF(C2,C3) -> SUBCLASS(C1,C3)
            /// </summary>
            SubClassTransitivity = 1,
            /// <summary>
            /// EQUIVALENTCLASS(C1,C2) ^ EQUIVALENTCLASS(C2,C3) -> EQUIVALENTCLASS(C1,C3)
            /// </summary>
            EquivalentClassTransitivity = 2,
            /// <summary>
            /// EQUIVALENTCLASS(C1,C2) ^ DISJOINTWITH(C2,C3) -> DISJOINTWITH(C1,C3)<br/>
            /// SUBCLASS(C1,C2) ^ DISJOINTWITH(C2,C3) -> DISJOINTWITH(C1,C3)<br/>
            /// DISJOINTWITH(C1,C2) ^ EQUIVALENTCLASS(C2,C3) -> DISJOINTWITH(C1,C3)
            /// </summary>
            DisjointWithEntailment = 3,
            /// <summary>
            /// SUBPROPERTY(P1,P2) ^ SUBPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)<br/>
            /// SUBPROPERTY(P1,P2) ^ EQUIVALENTPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)<br/>
            /// EQUIVALENTPROPERTY(P1,P2) ^ SUBPROPERTY(P2,P3) -> SUBPROPERTY(P1,P3)
            /// </summary>
            SubPropertyTransitivity = 4,
            /// <summary>
            /// EQUIVALENTPROPERTY(P1,P2) ^ EQUIVALENTPROPERTY(P2,P3) -> EQUIVALENTPROPERTY(P1,P3)
            /// </summary>
            EquivalentPropertyTransitivity = 5,
            /// <summary>
            /// P(F1,F2) ^ DOMAIN(P,C) -> C(F1)
            /// </summary>
            DomainEntailment = 6,
            /// <summary>
            /// P(F1,F2) ^ RANGE(P,C) -> C(F2)
            /// </summary>
            RangeEntailment = 7,
            /// <summary>
            /// SAMEAS(F1,F2) ^ SAMEAS(F2,F3) -> SAMEAS(F1,F3)
            /// </summary>
            SameAsTransitivity = 8,
            /// <summary>
            /// SAMEAS(F1,F2) ^ DIFFERENTFROM(F2,F3) -> DIFFERENTFROM(F1,F3)<br/>
            /// DIFFERENTFROM(F1,F2) ^ SAMEAS(F2,F3) -> DIFFERENTFROM(F1,F3)
            /// </summary>
            DifferentFromEntailment = 9,
            /// <summary>
            /// C1(F) ^ SUBCLASSOF(C1,C2) -> C2(F)<br/>
            /// C1(F) ^ EQUIVALENTCLASS(C1,C2) -> C2(F)
            /// </summary>
            ClassTypeEntailment = 10,
            /// <summary>
            /// C(F) -> NAMEDINDIVIDUAL(F)
            /// </summary>
            NamedIndividualEntailment = 11,
            /// <summary>
            /// P(F1,F2) ^ SYMMETRICPROPERTY(P) -> P(F2,F1)
            /// </summary>
            SymmetricPropertyEntailment = 12,
            /// <summary>
            /// P(F1,F2) ^ P(F2,F3) ^ TRANSITIVEPROPERTY(P) -> P(F1,F3)
            /// </summary>
            TransitivePropertyEntailment = 13,
            /// <summary>
            /// P(F1,F2) ^ REFLEXIVEPROPERTY(P) -> P(F1,F1)
            /// </summary>
            ReflexivePropertyEntailment = 14,
            /// <summary>
            /// P1(F1,F2) ^ INVERSEOF(P1,P2) -> P2(F2,F1)
            /// </summary>
            InverseOfEntailment = 15,
            /// <summary>
            /// P1(F1,F2) ^ SUBPROPERTY(P1,P2) -> P2(F1,F2)<br/>
            /// P1(F1,F2) ^ EQUIVALENTPROPERTY(P1,P2) -> P2(F1,F2)
            /// </summary>
            PropertyEntailment = 16,
            /// <summary>
            /// P(F1,F2) ^ SAMEAS(F1,F3) -> P(F3,F2)<br/>
            /// P(F1,F2) ^ SAMEAS(F2,F3) -> P(F1,F3)
            /// </summary>
            SameAsEntailment = 17,
            /// <summary>
            /// C(F1) ^ SUBCLASS(C,R) ^ RESTRICTION(R) ^ ONPROPERTY(R,P) ^ HASVALUE(R,F2) -> P(F1,F2)
            /// </summary>
            HasValueEntailment = 18,
            /// <summary>
            /// C(F) ^ SUBCLASS(C,R) ^ RESTRICTION(R) ^ ONPROPERTY(R,P) ^ HASSELF(R,"TRUE") -> P(F,F)
            /// </summary>
            HasSelfEntailment = 19,
            /// <summary>
            /// HASKEY(C,P) ^ C(F1) ^ C(F2) ^ P(F1,"K") ^ P(F2,"K") -> SAMEAS(F1,F2)
            /// </summary>
            HasKeyEntailment = 20,
            /// <summary>
            /// PROPERTYCHAINAXIOM(PCA) ^ MEMBER(PCA,P1) ^ MEMBER(PCA,P2) ^ P1(F1,X) ^ P2(X,F2) => PCA(F1,F2)
            /// </summary>
            PropertyChainEntailment = 21
        }

    }

}