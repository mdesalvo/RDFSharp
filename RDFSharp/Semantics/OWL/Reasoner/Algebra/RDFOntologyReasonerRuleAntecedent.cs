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

using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFSWRLRuleAntecedent represents the antecedent of a rule
    /// </summary>
    public class RDFOntologyReasonerRuleAntecedent
    {
        #region Properties
        /// <summary>
        /// Atoms composing the antecedent
        /// </summary>
        internal List<RDFOntologyReasonerRuleAtom> Atoms { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty antecedent
        /// </summary>
        public RDFOntologyReasonerRuleAntecedent()
            => this.Atoms = new List<RDFOntologyReasonerRuleAtom>();
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the antecedent
        /// </summary>
        public override string ToString()
            => string.Join(" ^ ", this.Atoms);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given atom to the antecedent
        /// </summary>
        public RDFOntologyReasonerRuleAntecedent AddAtom(RDFOntologyReasonerRuleAtom atom)
        {
            if (atom != null && !this.Atoms.Any(x => x.ToString().Equals(atom.ToString(), StringComparison.OrdinalIgnoreCase)))
                this.Atoms.Add(atom);
            return this;
        }

        /// <summary>
        /// Adds the given built-in to the antecedent
        /// </summary>
        public RDFOntologyReasonerRuleAntecedent AddBuiltIn(RDFOntologyReasonerRuleBuiltIn builtIn)
            => AddAtom(builtIn);

        /// <summary>
        /// Evaluates the antecedent in the context of the given ontology
        /// </summary>
        internal DataTable Evaluate(RDFOntology ontology, RDFOntologyReasonerOptions options)
        {
            //Execute the antecedent atoms
            List<DataTable> atomResults = new List<DataTable>();
            this.Atoms.Where(atom => !atom.IsBuiltIn).ToList().ForEach(atom =>
            {
                atomResults.Add(atom.EvaluateOnAntecedent(ontology, options));
            });

            //Join results of antecedent atoms
            DataTable antecedentResult = new RDFQueryEngine().CombineTables(atomResults, false);

            //Execute the antecedent built-ins
            this.Atoms.Where(atom => atom.IsBuiltIn).OfType<RDFOntologyReasonerRuleBuiltIn>().ToList().ForEach(builtin =>
            {
                antecedentResult = builtin.Evaluate(antecedentResult, ontology, options);
            });

            //Return the antecedent result
            antecedentResult.TableName = this.ToString();
            return antecedentResult;
        }
        #endregion
    }

}