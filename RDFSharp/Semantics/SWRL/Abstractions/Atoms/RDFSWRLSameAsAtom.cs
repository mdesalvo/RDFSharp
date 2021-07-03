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
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLSameAsAtom represents an atom describing owl:sameAs relations between ontology facts 
    /// </summary>
    public class RDFSWRLSameAsAtom : RDFSWRLObjectPropertyAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an owl:sameAs atom with the given arguments
        /// </summary>
        public RDFSWRLSameAsAtom(RDFVariable leftArgument, RDFVariable rightArgument)
            : base(RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an owl:sameAs atom with the given arguments
        /// </summary>
        public RDFSWRLSameAsAtom(RDFVariable leftArgument, RDFOntologyFact rightArgument)
            : base(RDFVocabulary.OWL.SAME_AS.ToRDFOntologyObjectProperty(), leftArgument, rightArgument) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the atom in the context of an antecedent
        /// </summary>
        internal override DataTable EvaluateOnAntecedent(RDFOntology ontology)
        {
            //Initialize the structure of the atom result
            DataTable atomResult = new DataTable(this.ToString());
            RDFQueryEngine.AddColumn(atomResult, this.LeftArgument.ToString());
            if (this.RightArgument is RDFVariable)
                RDFQueryEngine.AddColumn(atomResult, this.RightArgument.ToString());
            atomResult.ExtendedProperties.Add("ATOM_TYPE", nameof(RDFSWRLSameAsAtom));

            //Materialize owl:sameAs inferences of the atom
            if (this.RightArgument is RDFOntologyFact rightArgumentFact)
            { 
                RDFOntologyData sameFacts = RDFOntologyHelper.GetSameFactsAs(ontology.Data, rightArgumentFact);
                foreach (RDFOntologyFact sameFact in sameFacts)
                {
                    Dictionary<string, string> bindings = new Dictionary<string, string>();
                    bindings.Add(this.LeftArgument.ToString(), sameFact.ToString());

                    RDFQueryEngine.AddRow(atomResult, bindings);
                }
            }
            else
            {
                foreach (RDFOntologyFact ontologyFact in ontology.Data)
                {
                    RDFOntologyData sameFacts = RDFOntologyHelper.GetSameFactsAs(ontology.Data, ontologyFact);
                    foreach (RDFOntologyFact sameFact in sameFacts)
                    {
                        Dictionary<string, string> bindings = new Dictionary<string, string>();
                        bindings.Add(this.LeftArgument.ToString(), ontologyFact.ToString());
                        bindings.Add(this.RightArgument.ToString(), sameFact.ToString());

                        RDFQueryEngine.AddRow(atomResult, bindings);
                    }
                }
            }

            //Return the atom result
            return atomResult;
        }

        /// <summary>
        /// Evaluates the atom in the context of an consequent
        /// </summary>
        internal override RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            //TODO

            return report;
        }
        #endregion
    }
}