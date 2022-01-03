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

using RDFSharp.Model;
using RDFSharp.Query;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyReasonerRuleDifferentFromAtom represents an atom inferring owl:differentFrom assertions between ontology facts 
    /// </summary>
    public class RDFOntologyReasonerRuleDifferentFromAtom : RDFOntologyReasonerRuleObjectPropertyAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build an owl:differentFrom atom with the given arguments
        /// </summary>
        public RDFOntologyReasonerRuleDifferentFromAtom(RDFVariable leftArgument, RDFVariable rightArgument)
            : base(RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build an owl:differentFrom atom with the given arguments
        /// </summary>
        public RDFOntologyReasonerRuleDifferentFromAtom(RDFVariable leftArgument, RDFOntologyFact rightArgument)
            : base(RDFVocabulary.OWL.DIFFERENT_FROM.ToRDFOntologyObjectProperty(), leftArgument, rightArgument) { }
        #endregion

        #region Methods
        /// <summary>
        /// Evaluates the atom in the context of an antecedent
        /// </summary>
        internal override DataTable EvaluateOnAntecedent(RDFOntology ontology, RDFOntologyReasonerOptions options)
        {
            //Initialize the structure of the atom result
            DataTable atomResult = new DataTable(this.ToString());
            RDFQueryEngine.AddColumn(atomResult, this.LeftArgument.ToString());
            if (this.RightArgument is RDFVariable)
                RDFQueryEngine.AddColumn(atomResult, this.RightArgument.ToString());

            //Materialize owl:differentFrom inferences of the atom
            if (this.RightArgument is RDFOntologyFact rightArgumentFact)
            {
                RDFOntologyData differentFacts = RDFOntologyHelper.GetDifferentFactsFrom(ontology.Data, rightArgumentFact);
                foreach (RDFOntologyFact differentFact in differentFacts)
                {
                    Dictionary<string, string> bindings = new Dictionary<string, string>();
                    bindings.Add(this.LeftArgument.ToString(), differentFact.ToString());

                    RDFQueryEngine.AddRow(atomResult, bindings);
                }
            }
            else
            {
                foreach (RDFOntologyFact ontologyFact in ontology.Data)
                {
                    RDFOntologyData differentFacts = RDFOntologyHelper.GetDifferentFactsFrom(ontology.Data, ontologyFact);
                    foreach (RDFOntologyFact differentFact in differentFacts)
                    {
                        Dictionary<string, string> bindings = new Dictionary<string, string>();
                        bindings.Add(this.LeftArgument.ToString(), ontologyFact.ToString());
                        bindings.Add(this.RightArgument.ToString(), differentFact.ToString());

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
        internal override RDFOntologyReasonerReport EvaluateOnConsequent(DataTable antecedentResults, RDFOntology ontology, RDFOntologyReasonerOptions options)
        {
            RDFOntologyReasonerReport report = new RDFOntologyReasonerReport();
            string leftArgumentString = this.LeftArgument.ToString();
            string rightArgumentString = this.RightArgument.ToString();

            #region Guards
            //The antecedent results table MUST have a column corresponding to the atom's left argument
            if (!antecedentResults.Columns.Contains(leftArgumentString))
                return report;

            //The antecedent results table MUST have a column corresponding to the atom's right argument (if variable)
            if (this.RightArgument is RDFVariable && !antecedentResults.Columns.Contains(rightArgumentString))
                return report;
            #endregion

            //Iterate the antecedent results table to materialize the atom's reasoner evidences
            IEnumerator rowsEnum = antecedentResults.Rows.GetEnumerator();
            while (rowsEnum.MoveNext())
            {
                DataRow currentRow = (DataRow)rowsEnum.Current;

                #region Guards
                //The current row MUST have a BOUND value in the column corresponding to the atom's left argument
                if (currentRow.IsNull(leftArgumentString))
                    continue;

                //The current row MUST have a BOUND value in the column corresponding to the atom's right argument (if variable)
                if (this.RightArgument is RDFVariable && currentRow.IsNull(rightArgumentString))
                    continue;
                #endregion

                //Parse the value of the column corresponding to the atom's left argument
                RDFPatternMember leftArgumentValue = RDFQueryUtilities.ParseRDFPatternMember(currentRow[leftArgumentString].ToString());

                //Parse the value of the column corresponding to the atom's right argument
                RDFPatternMember rightArgumentValue =
                    this.RightArgument is RDFVariable ? RDFQueryUtilities.ParseRDFPatternMember(currentRow[rightArgumentString].ToString())
                                                      : ((RDFOntologyFact)this.RightArgument).Value;

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

                    //Protect atom's inferences with implicit taxonomy checks (only if taxonomy protection has been requested)
                    if (!options.EnforceTaxonomyProtection || !RDFOntologyHelper.CheckIsSameFactAs(ontology.Data, leftFact, rightFact))
                    {
                        //Create the inference as a taxonomy entry
                        RDFOntologyTaxonomyEntry sem_infA = new RDFOntologyTaxonomyEntry(leftFact, (RDFOntologyObjectProperty)this.Predicate, rightFact)
                                                                 .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);
                        RDFOntologyTaxonomyEntry sem_infB = new RDFOntologyTaxonomyEntry(rightFact, (RDFOntologyObjectProperty)this.Predicate, leftFact)
                                                                 .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.DifferentFrom.ContainsEntry(sem_infA))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, this.ToString(), nameof(RDFOntologyData.Relations.DifferentFrom), sem_infA));
                        if (!ontology.Data.Relations.DifferentFrom.ContainsEntry(sem_infB))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, this.ToString(), nameof(RDFOntologyData.Relations.DifferentFrom), sem_infB));
                    }
                }
            }

            return report;
        }
        #endregion
    }

}