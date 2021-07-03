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

using RDFSharp.Query;
using RDFSharp.Semantics.OWL;
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLObjectPropertyAtom represents an atom describing assertions relating ontology facts 
    /// </summary>
    public class RDFSWRLObjectPropertyAtom : RDFSWRLAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an object property atom with the given property and arguments
        /// </summary>
        public RDFSWRLObjectPropertyAtom(RDFOntologyObjectProperty ontologyObjectProperty, RDFVariable leftArgument, RDFVariable rightArgument)
            : base(ontologyObjectProperty, leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an object property atom with the given property and arguments
        /// </summary>
        public RDFSWRLObjectPropertyAtom(RDFOntologyObjectProperty ontologyObjectProperty, RDFVariable leftArgument, RDFOntologyFact rightArgument)
            : base(ontologyObjectProperty, leftArgument, rightArgument) { }
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
            atomResult.ExtendedProperties.Add("ATOM_TYPE", nameof(RDFSWRLObjectPropertyAtom));

            //Materialize object property assertions of the atom predicate
            RDFOntologyTaxonomy assertions = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(this.Predicate);
            if (this.RightArgument is RDFOntologyFact rightArgumentFact)
                assertions = assertions.SelectEntriesByObject(rightArgumentFact);
            foreach (RDFOntologyTaxonomyEntry assertion in assertions)
            {
                Dictionary<string, string> bindings = new Dictionary<string, string>();
                bindings.Add(this.LeftArgument.ToString(), assertion.TaxonomySubject.ToString());
                if (this.RightArgument is RDFVariable)
                    bindings.Add(this.RightArgument.ToString(), assertion.TaxonomyObject.ToString());

                RDFQueryEngine.AddRow(atomResult, bindings);
            }

            //Return the atom result
            return atomResult;
        }

        /// <summary>
        /// Evaluates the atom in the context of an consequent
        /// </summary>
        internal override RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults, RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();

            //TODO

            return report;
        }
        #endregion
    }
}