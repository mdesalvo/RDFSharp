/*
   Copyright 2012-2020 Marco De Salvo
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

using RDFSharp.Model;
using RDFSharp.Query;
using System.Data;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyReasonerRuleSubtractBuiltIn represents a built-in of type swrlb:subtract
    /// </summary>
    public class RDFOntologyReasonerRuleSubtractBuiltIn : RDFOntologyReasonerRuleBuiltIn
    {
        #region Properties
        /// <summary>
        /// Represents the Uri of the built-in (swrlb:subtract)
        /// </summary>
        private static RDFResource BuiltInUri = new RDFResource($"swrlb:subtract");

        /// <summary>
        /// Represents the numeric value to be subtracted to the RightArgument for checking equality of the LeftArgument
        /// </summary>
        private double SubtractValue { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a swrlb:subtract built-in with given arguments
        /// </summary>
        public RDFOntologyReasonerRuleSubtractBuiltIn(RDFVariable leftArgument, RDFVariable rightArgument, double subtractValue)
            : base(new RDFOntologyResource() { Value = BuiltInUri }, leftArgument, rightArgument)
                => this.SubtractValue = subtractValue;
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the built-in
        /// </summary>
        public override string ToString()
            => PrintMathBuiltIn(this.SubtractValue);
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the built-in in the context of the given antecedent results
        /// </summary>
        internal override DataTable Evaluate(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options)
            => EvaluateMathBuiltIn("-", this.SubtractValue, antecedentResults);

        internal override DataTable EvaluateOnAntecedent(RDFOntology ontology, RDFOntologyReasonerOptions options) => null;
        internal override RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options) => null;
        #endregion
    }
}