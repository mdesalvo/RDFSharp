/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Data;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasonerRuleConsequent represents the consequent of a rule
    /// </summary>
    public class RDFOntologyReasonerRuleConsequent
    {
        #region Properties
        /// <summary>
        /// Atoms composing the consequent
        /// </summary>
        internal List<RDFOntologyReasonerRuleAtom> Atoms { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty consequent
        /// </summary>
        public RDFOntologyReasonerRuleConsequent()
            => this.Atoms = new List<RDFOntologyReasonerRuleAtom>();
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the consequent
        /// </summary>
        public override string ToString()
            => string.Join(" ^ ", this.Atoms);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given atom to the consequent
        /// </summary>
        public RDFOntologyReasonerRuleConsequent AddAtom(RDFOntologyReasonerRuleAtom atom)
        {
            if (atom != null && !this.Atoms.Any(x => x.ToString().Equals(atom.ToString(), StringComparison.OrdinalIgnoreCase)))
                this.Atoms.Add(atom);
            return this;
        }

        /// <summary>
        /// Evaluates the consequent in the context of the given antecedent results
        /// </summary>
        internal RDFOntologyReasonerReport Evaluate(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            //Execute the consequent atoms
            this.Atoms.ForEach(atom => report.Merge(atom.EvaluateOnConsequent(antecedentResults, ontology, options)));

            //Return the consequent result
            return report;
        }
        #endregion
    }

}