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
using System.Collections.Generic;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLClassAtom represents an atom describing instances of a given ontology class 
    /// </summary>
    public class RDFSWRLClassAtom : RDFSWRLAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a class atom with the given ontology class, predicate name and arguments
        /// </summary>
        public RDFSWRLClassAtom(RDFOntologyClass ontologyClass, string predicateName, List<RDFPatternMember> arguments)
            : base(ontologyClass, predicateName, arguments) { }
        #endregion

        #region Interfaces

        #endregion

        #region Methods

        #endregion
    }
}