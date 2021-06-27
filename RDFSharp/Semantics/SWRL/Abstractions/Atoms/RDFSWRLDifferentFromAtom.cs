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

using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Semantics.OWL;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLDifferentFromAtom represents an atom describing owl:differentFrom assertions relating ontology facts 
    /// </summary>
    public class RDFSWRLDifferentFromAtom : RDFSWRLIndividualPropertyAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an owl:differentFrom atom with the given arguments
        /// </summary>
        public RDFSWRLDifferentFromAtom(RDFVariable leftArgument, RDFVariable rightArgument)
            : base(RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), leftArgument, rightArgument) { }
        #endregion
    }
}