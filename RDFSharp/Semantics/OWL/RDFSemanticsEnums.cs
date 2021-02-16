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
            /// Does not export any semantic inference
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

    }

}