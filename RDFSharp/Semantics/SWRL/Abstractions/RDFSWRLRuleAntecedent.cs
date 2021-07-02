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
using System.Data;
using System.Linq;

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
        /// <summary>
        /// Gives the string representation of the antecedent
        /// </summary>
        public override string ToString()
            => string.Join(" ∧ ", this.Atoms);
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

        /// <summary>
        /// Applies the antecedent to the given ontology
        /// </summary>
        internal DataTable ApplyToOntology(RDFOntology ontology)
        {
            //Execute the antecedent atoms
            List<DataTable> atomResults = new List<DataTable>();
            this.Atoms.ForEach(atom => atomResults.Add(atom.ApplyToOntology(ontology)));

            //Exploit the query engine to join results of antecedent atoms
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable antecedentResult = queryEngine.CombineTables(atomResults, false);

            //Return the antecedent result
            return antecedentResult;
        }
        #endregion
    }
}