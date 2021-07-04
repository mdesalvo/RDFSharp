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

using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Semantics.OWL;
using System.Data;
using System.Linq;
using System.Text;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLAtom represents an atom contained in a SWRL rule's antecedent/consequent
    /// </summary>
    public abstract class RDFSWRLAtom
    {
        #region Properties
        /// <summary>
        /// Represents the atom's predicate
        /// </summary>
        public RDFOntologyResource Predicate { get; internal set; }

        /// <summary>
        /// Represents the left argument given to the atom's predicate
        /// </summary>
        public RDFPatternMember LeftArgument { get; internal set; }

        /// <summary>
        /// Represents the right argument given to the atom's predicate
        /// </summary>
        public RDFPatternMember RightArgument { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an atom with given predicate and arguments
        /// </summary>
        internal RDFSWRLAtom(RDFOntologyResource predicate, RDFPatternMember leftArgument, RDFPatternMember rightArgument)
        {
            if (predicate == null)
                throw new RDFSemanticsException("Cannot create SWRL atom because given \"predicate\" parameter is null");

            if (leftArgument == null)
                throw new RDFSemanticsException("Cannot create SWRL atom because given \"leftArgument\" parameter is null");

            this.Predicate = predicate;
            this.LeftArgument = leftArgument;
            this.RightArgument = rightArgument;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the atom
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            //Predicate
            sb.Append(RDFModelUtilities.GetShortUri(((RDFResource)this.Predicate.Value).URI));

            //Arguments
            sb.Append($"({this.LeftArgument}");
            if (this.RightArgument != null)
                sb.Append($",{this.RightArgument}");
            sb.Append(")");

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the atom in the context of an antecedent
        /// </summary>
        internal abstract DataTable EvaluateOnAntecedent(RDFOntology ontology);

        /// <summary>
        /// Evaluates the atom in the context of a consequent
        /// </summary>
        internal abstract RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults, RDFOntology ontology);
        #endregion
    }
}