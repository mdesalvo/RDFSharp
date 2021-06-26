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

using RDFSharp.Semantics.OWL;
using System.Collections.Generic;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLRuleAntecedent represents the antecedent of a SWRL rule
    /// </summary>
    public class RDFSWRLRuleAntecedent
    {
        #region Properties
        /// <summary>
        /// Atoms composing the antecedent
        /// </summary>
        internal List<RDFSWRLAtom> Atoms { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty antecedent
        /// </summary>
        public RDFSWRLRuleAntecedent()
            => this.Atoms = new List<RDFSWRLAtom>();
        #endregion

        #region Interfaces

        #endregion

        #region Methods
        /// <summary>
        /// Adds the given atom to the antecedent
        /// </summary>
        public RDFSWRLRuleAntecedent AddAtom(RDFSWRLAtom atom)
        {
            if (atom != null)
                this.Atoms.Add(atom);
            return this;
        }
        #endregion
    }
}