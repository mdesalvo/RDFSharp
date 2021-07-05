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

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLRuleAntecedent represents the antecedent of a SWRL rule
    /// </summary>
    public class RDFSWRLRuleAntecedent : RDFSWRLAtomCollection
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty antecedent
        /// </summary>
        public RDFSWRLRuleAntecedent() : base() { }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given atom to the antecedent
        /// </summary>
        public RDFSWRLRuleAntecedent AddAtom(RDFSWRLAtom atom)
            => AddAtom<RDFSWRLRuleAntecedent>(atom);

        /// <summary>
        /// Evaluates the antecedent in the context of the given ontology
        /// </summary>
        internal DataTable Evaluate(RDFOntology ontology, RDFSWRLRuleOptions ruleOptions)
        {
            //Execute the antecedent atoms
            List<DataTable> atomResults = new List<DataTable>();
            this.Atoms.ForEach(atom => atomResults.Add(atom.EvaluateOnAntecedent(ontology, ruleOptions)));

            //Join results of antecedent atoms
            DataTable antecedentResult = new RDFQueryEngine().CombineTables(atomResults, false);
            antecedentResult.TableName = this.ToString();

            //Return the antecedent result
            return antecedentResult;
        }
        #endregion
    }
}