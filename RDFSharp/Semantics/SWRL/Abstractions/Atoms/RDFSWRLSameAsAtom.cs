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
using System.Collections;
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
        internal override RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults, RDFOntology ontology)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            string leftArgumentString = this.LeftArgument.ToString();
            string rightArgumentString = this.RightArgument.ToString();

            //Guard: the antecedent results table MUST have a column corresponding to the atom's left argument
            if (!antecedentResults.Columns.Contains(leftArgumentString))
                return report;

            //Guard: the antecedent results table MUST have a column corresponding to the atom's right argument (if variable)
            if (this.RightArgument is RDFVariable && !antecedentResults.Columns.Contains(rightArgumentString))
                return report;

            //Iterate the antecedent results table to materialize the atom's reasoner evidences
            IEnumerator rowsEnum = antecedentResults.Rows.GetEnumerator();
            while (rowsEnum.MoveNext())
            {
                DataRow currentRow = (DataRow)rowsEnum.Current;

                //Guard: the current row MUST have a BOUND value in the column corresponding to the atom's left argument
                if (currentRow.IsNull(leftArgumentString))
                    continue;

                //Guard: the current row MUST have a BOUND value in the column corresponding to the atom's right argument
                if (currentRow.IsNull(rightArgumentString))
                    continue;

                //Parse the value of the column corresponding to the atom's left argument
                RDFPatternMember leftArgumentValue = RDFQueryUtilities.ParseRDFPatternMember(currentRow[leftArgumentString].ToString());

                //Parse the value of the column corresponding to the atom's right argument
                RDFPatternMember rightArgumentValue = this.RightArgument;
                if (this.RightArgument is RDFVariable)
                    rightArgumentValue = RDFQueryUtilities.ParseRDFPatternMember(currentRow[rightArgumentString].ToString());

                if (leftArgumentValue is RDFResource leftArgumentValueResource
                        && rightArgumentValue is RDFResource rightArgumentValueResource)
                {
                    //Search the left fact in the ontology
                    RDFOntologyFact leftFact = ontology.Data.SelectFact(leftArgumentValueResource.ToString());
                    if (leftFact == null)
                        leftFact = new RDFOntologyFact(leftArgumentValueResource);

                    //Search the right fact in the ontology
                    RDFOntologyFact rightFact = ontology.Data.SelectFact(rightArgumentValueResource.ToString());
                    if (rightFact == null)
                        rightFact = new RDFOntologyFact(rightArgumentValueResource);

                    //Protect atom's inferences with implicit taxonomy checks
                    if (!RDFOntologyHelper.CheckIsDifferentFactFrom(ontology.Data, leftFact, rightFact))
                    {
                        //Create the inference as a taxonomy entry
                        RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(leftFact, (RDFOntologyObjectProperty)this.Predicate, rightFact)
                                                                 .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                        RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(rightFact, (RDFOntologyObjectProperty)this.Predicate, leftFact)
                                                                 .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.SameAs.ContainsEntry(sem_infA))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, this.ToString(), nameof(RDFOntologyData.Relations.SameAs), sem_infA));
                        if (!ontology.Data.Relations.SameAs.ContainsEntry(sem_infB))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, this.ToString(), nameof(RDFOntologyData.Relations.SameAs), sem_infB));
                    }
                }
            }

            return report;
        }
        #endregion
    }
}