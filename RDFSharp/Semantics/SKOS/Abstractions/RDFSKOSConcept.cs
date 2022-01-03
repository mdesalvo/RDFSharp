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
using RDFSharp.Semantics.OWL;

namespace RDFSharp.Semantics.SKOS
{

    /// <summary>
    /// RDFSKOSConcept represents an instance of skos:Concept within an instance of skos:ConceptScheme
    /// </summary>
    public class RDFSKOSConcept : RDFOntologyFact
    {

        #region Ctors
        /// <summary>
        /// Default-ctor to build a skos:Concept with the given name
        /// </summary>
        public RDFSKOSConcept(RDFResource conceptName) : base(conceptName) { }
        #endregion

    }

}