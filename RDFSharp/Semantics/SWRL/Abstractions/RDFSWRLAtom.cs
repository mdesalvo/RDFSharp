﻿/*
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
    /// RDFSWRLAtom represents an atom contained in a SWRL rule's antecedent/consequent
    /// </summary>
    public abstract class RDFSWRLAtom
    {
        #region Properties
        /// <summary>
        /// Represents the predicate given to the atom
        /// </summary>
        public string Predicate { get; internal set; }

        /// <summary>
        /// Represents the arguments given to the atom
        /// </summary>
        public List<RDFPatternMember> Arguments { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an atom with given predicate and arguments (e.g.: predicate(arg1,arg2,...))
        /// </summary>
        internal RDFSWRLAtom(string predicate, List<RDFPatternMember> arguments)
        {
            if (string.IsNullOrWhiteSpace(predicate))
                throw new RDFSemanticsException("Cannot create SWRL atom because given \"predicate\" parameter is null or empty");

            if (arguments?.Count == 0)
                throw new RDFSemanticsException("Cannot create SWRL atom because given \"arguments\" parameter is null or does not contain elements");

            this.Predicate = predicate.ToUpperInvariant().Trim();
            this.Arguments = arguments;
        }
        #endregion

        #region Interfaces

        #endregion

        #region Methods

        #endregion
    }
}