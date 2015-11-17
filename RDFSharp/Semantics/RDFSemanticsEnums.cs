/*
   Copyright 2012-2015 Marco De Salvo

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

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFSemanticsEnums represents a collector for all the enumerations used by the "RDFSharp.Semantics" namespace
    /// </summary>
    public static class RDFSemanticsEnums {

        /// <summary>
        /// RDFOntologyValidationEvidenceCategory represents an enumeration for possible categories of ontology validation evidence
        /// </summary>
        public enum RDFOntologyValidationEvidenceCategory { Warning, Error };

        /// <summary>
        /// RDFOntologyReasoningEvidenceCategory represents an enumeration for possible categories of ontology reasoning evidence
        /// </summary>
        public enum RDFOntologyReasoningEvidenceCategory { ClassModel, PropertyModel, Data };

    }

}