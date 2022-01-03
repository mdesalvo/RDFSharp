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
    /// RDFOntologyAxiomAnnotation represents an owl:Axiom annotation describing an ontology assertion
    /// </summary>
    public class RDFOntologyAxiomAnnotation
    {
        #region Properties
        /// <summary>
        /// Represents the property of the axiom annotation
        /// </summary>
        public RDFOntologyProperty Property { get; internal set; }

        /// <summary>
        /// Represents the value of the axiom annotation
        /// </summary>
        public RDFOntologyLiteral Value { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an axiom annotation with the given property and value
        /// </summary>
        public RDFOntologyAxiomAnnotation(RDFOntologyProperty property, RDFOntologyLiteral value)
        {
            if (property == null)
                throw new RDFSemanticsException("Cannot create axiom annotation because given \"property\" parameter is null");
            if (value == null)
                throw new RDFSemanticsException("Cannot create axiom annotation because given \"value\" parameter is null");

            this.Property = property;
            this.Value = value;
        }
        #endregion
    }
}