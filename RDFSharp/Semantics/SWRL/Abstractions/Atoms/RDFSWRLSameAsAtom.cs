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

using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Semantics.OWL;
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

        /// <summary>
        /// Applies the owl:sameAs atom to the given ontology
        /// </summary>
        internal override DataTable ApplyToOntology(RDFOntology ontology)
        {
            //Initialize the structure of the atom result
            DataTable atomResult = new DataTable(this.ToString());
            RDFQueryEngine.AddColumn(atomResult, this.LeftArgument.ToString());
            RDFQueryEngine.AddColumn(atomResult, this.RightArgument.ToString());
            atomResult.ExtendedProperties.Add("ATOM_TYPE", nameof(RDFSWRLSameAsAtom));

            //Materialize owl:sameAs inferences of the atom variables
            

            //Return the atom result
            return atomResult;
        }
    }
}