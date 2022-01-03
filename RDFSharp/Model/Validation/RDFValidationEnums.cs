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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFValidationEnums represents a collector for all the enumerations used for SHACL modeling and validation
    /// </summary>
    public static class RDFValidationEnums
    {

        /// <summary>
        /// RDFShapeSeverity represents an enumeration for possible severity levels of shape validation evidence
        /// </summary>
        public enum RDFShapeSeverity
        {
            /// <summary>
            /// Shape has not been violated: data graph may contain trivial structural inconsistencies
            /// </summary>
            Info = 0,
            /// <summary>
            /// Shape has not been violated: data graph may contain structural inconsistencies
            /// </summary>
            Warning = 1,
            /// <summary>
            /// Shape has been violated: data graph contains structural inconsistencies
            /// </summary>
            Violation = 2
        };

        /// <summary>
        /// RDFNodeKinds represents an enumeration for supported values of sh:nodeKind constraint
        /// </summary>
        public enum RDFNodeKinds
        {
            /// <summary>
            /// sh:BlankNode
            /// </summary>
            BlankNode = 0,
            /// <summary>
            ///  sh:BlankNodeOrIRI
            /// </summary>
            BlankNodeOrIRI = 1,
            /// <summary>
            /// sh:BlankNodeOrLiteral
            /// </summary>
            BlankNodeOrLiteral = 2,
            /// <summary>
            ///  sh:IRI
            /// </summary>
            IRI = 3,
            /// <summary>
            /// sh:IRIOrLiteral
            /// </summary>
            IRIOrLiteral = 4,
            /// <summary>
            /// sh:Literal
            /// </summary>
            Literal = 5
        }

    }
}