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
    /// RDFSWRLClassAtom represents an atom describing instances of a given ontology class 
    /// </summary>
    public class RDFSWRLClassAtom : RDFSWRLAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a class atom with the given class and arguments
        /// </summary>
        public RDFSWRLClassAtom(RDFOntologyClass ontologyClass, RDFVariable leftArgument)
            : base(ontologyClass, leftArgument, null) { }
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
            atomResult.ExtendedProperties.Add("ATOM_TYPE", nameof(RDFSWRLClassAtom));

            //Materialize members of the atom class
            RDFOntologyData ontologyData = RDFOntologyHelper.GetMembersOf(ontology, (RDFOntologyClass)this.LeftArgument);
            foreach (RDFOntologyFact ontologyFact in ontologyData)
            {
                Dictionary<string, string> bindings = new Dictionary<string, string>();
                bindings.Add(this.LeftArgument.ToString(), ontologyFact.ToString());

                RDFQueryEngine.AddRow(atomResult, bindings);
            }

            //Return the atom result
            return atomResult;
        }
        #endregion
    }
}