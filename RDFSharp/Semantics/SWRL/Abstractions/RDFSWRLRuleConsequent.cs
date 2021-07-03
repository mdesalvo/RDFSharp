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

using RDFSharp.Semantics.OWL;
using System.Data;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLRuleConsequent represents the consequent of a SWRL rule
    /// </summary>
    public class RDFSWRLRuleConsequent : RDFSWRLAtomCollection
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty consequent
        /// </summary>
        public RDFSWRLRuleConsequent() : base() { }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given atom to the consequent
        /// </summary>
        public RDFSWRLRuleConsequent AddAtom(RDFSWRLAtom atom)
            => AddAtom<RDFSWRLRuleConsequent>(atom);

        /// <summary>
        /// Evaluates the consequent in the context of the given antecedent results
        /// </summary>
        internal RDFOntologyReasonerReport Evaluate(DataTable antecedentResults, RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            //Execute the consequent atoms
            this.Atoms.ForEach(atom => report.Merge(atom.EvaluateOnConsequent(antecedentResults, ontology)));

            //Return the consequent result
            return report;
        }
        #endregion
    }
}