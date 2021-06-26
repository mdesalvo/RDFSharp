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
using System;
using System.Collections.Generic;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLAtom represents an atom contained in a SWRL rule's antecedent/consequent
    /// </summary>
    public abstract class RDFSWRLAtom
    {
        #region Properties
        /// <summary>
        /// Represents the predicate checked on the ontology (e.g: "http://xmlns.com/foaf/0.1/Person")
        /// </summary>
        public RDFOntologyResource Predicate { get; internal set; }

        /// <summary>
        /// Represents the friendly name used for printing the atom's predicate (e.g: "Person")
        /// </summary>
        public string PredicateName { get; internal set; }

        /// <summary>
        /// Represents the arguments given to the atom's predicate
        /// </summary>
        public List<RDFPatternMember> Arguments { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an atom with given predicate and arguments (e.g. "hasBrother(?x,?y)")
        /// </summary>
        internal RDFSWRLAtom(RDFOntologyResource predicate, string predicateName, List<RDFPatternMember> arguments)
        {
            if (predicate == null)
                throw new RDFSemanticsException("Cannot create SWRL atom because given \"predicate\" parameter is null");

            if (string.IsNullOrWhiteSpace(predicateName))
                throw new RDFSemanticsException("Cannot create SWRL atom because given \"predicateName\" parameter is null or empty");

            if (arguments?.Count == 0)
                throw new RDFSemanticsException("Cannot create SWRL atom because given \"arguments\" parameter is null or does not contain elements");

            this.Predicate = predicate;
            this.PredicateName = predicateName;            
            this.Arguments = arguments;
        }
        #endregion

        #region Interfaces

        #endregion

        #region Methods

        #endregion
    }
}