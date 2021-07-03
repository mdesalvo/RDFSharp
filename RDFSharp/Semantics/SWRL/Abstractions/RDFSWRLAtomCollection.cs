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
using System.Data;
using System.Linq;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLAtomCollection represents a collection of SWRL atoms
    /// </summary>
    public class RDFSWRLAtomCollection
    {
        #region Properties
        /// <summary>
        /// Atoms composing the collection
        /// </summary>
        internal List<RDFSWRLAtom> Atoms { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty atom collection
        /// </summary>
        internal RDFSWRLAtomCollection()
            => this.Atoms = new List<RDFSWRLAtom>();
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the atom collection
        /// </summary>
        public override string ToString()
            => string.Join(",", this.Atoms);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given atom to the collection
        /// </summary>
        internal T AddAtom<T>(RDFSWRLAtom atom) where T : RDFSWRLAtomCollection
        {
            if (atom != null && !this.Atoms.Any(x => x.ToString().Equals(atom.ToString(), StringComparison.OrdinalIgnoreCase)))
                this.Atoms.Add(atom);
            return (T)this;
        }

        /// <summary>
        /// Applies the atom collection to the given ontology
        /// </summary>
        public DataTable ApplyToOntology(RDFOntology ontology)
        {
            //Execute the antecedent atoms
            List<DataTable> atomResults = new List<DataTable>();
            this.Atoms.ForEach(atom => atomResults.Add(atom.ApplyToOntology(ontology)));

            //Exploit the query engine to join results of antecedent atoms
            DataTable antecedentResult = new RDFQueryEngine().CombineTables(atomResults, false);
            antecedentResult.TableName = this.ToString();

            //Return the antecedent result
            return antecedentResult;
        }
        #endregion
    }
}