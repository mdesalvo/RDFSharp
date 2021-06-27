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
using RDFSharp.Semantics.OWL;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLIndividualPropertyAtom represents an atom describing assertions relating ontology individuals 
    /// </summary>
    public class RDFSWRLIndividualPropertyAtom : RDFSWRLAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an individual atom with the given property and arguments
        /// </summary>
        public RDFSWRLIndividualPropertyAtom(RDFOntologyObjectProperty ontologyObjectProperty, RDFVariable leftArgument, RDFVariable rightArgument)
            : base(ontologyObjectProperty, leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an individual atom with the given property and arguments
        /// </summary>
        public RDFSWRLIndividualPropertyAtom(RDFOntologyObjectProperty ontologyObjectProperty, RDFVariable leftArgument, RDFOntologyFact rightArgument)
            : base(ontologyObjectProperty, leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an individual atom with the given property and arguments
        /// </summary>
        public RDFSWRLIndividualPropertyAtom(RDFOntologyObjectProperty ontologyObjectProperty, RDFOntologyFact leftArgument, RDFVariable rightArgument)
            : base(ontologyObjectProperty, leftArgument, rightArgument) { }
        #endregion
    }
}