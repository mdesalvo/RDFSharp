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

using System;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasonerRuleMolecule represents a collection of atoms
    /// </summary>
    public class RDFOntologyReasonerRuleMolecule
    {
        #region Properties
        /// <summary>
        /// Atoms composing the molecule
        /// </summary>
        internal List<RDFOntologyReasonerRuleAtom> Atoms { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty molecule
        /// </summary>
        internal RDFOntologyReasonerRuleMolecule()
            => this.Atoms = new List<RDFOntologyReasonerRuleAtom>();
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the molecule
        /// </summary>
        public override string ToString()
            => string.Join(", ", this.Atoms);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given atom to the molecule
        /// </summary>
        internal T AddAtom<T>(RDFOntologyReasonerRuleAtom atom) where T : RDFOntologyReasonerRuleMolecule
        {
            if (atom != null && !this.Atoms.Any(x => x.ToString().Equals(atom.ToString(), StringComparison.OrdinalIgnoreCase)))
                this.Atoms.Add(atom);
            return (T)this;
        }
        #endregion
    }

}