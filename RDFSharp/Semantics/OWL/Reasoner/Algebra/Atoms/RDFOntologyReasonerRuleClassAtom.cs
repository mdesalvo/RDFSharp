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
    /// RDFOntologyReasonerRuleClassAtom represents an atom inferring instances of a given ontology class 
    /// </summary>
    public class RDFOntologyReasonerRuleClassAtom : RDFOntologyReasonerRuleAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a class atom with the given class and arguments
        /// </summary>
        public RDFOntologyReasonerRuleClassAtom(RDFOntologyClass ontologyClass, RDFVariable leftArgument)
            : base(ontologyClass, leftArgument, null) { }
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

            //Materialize members of the atom class
            RDFOntologyData ontologyData = RDFOntologyHelper.GetMembersOf(ontology, (RDFOntologyClass)this.Predicate);
            foreach (RDFOntologyFact ontologyFact in ontologyData)
            {
                Dictionary<string, string> bindings = new Dictionary<string, string>();
                bindings.Add(this.LeftArgument.ToString(), ontologyFact.ToString());

                RDFQueryEngine.AddRow(atomResult, bindings);
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
            RDFOntologyObjectProperty type = RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty();
            string leftArgumentString = this.LeftArgument.ToString();

            #region Guards
            //The antecedent results table MUST have a column corresponding to the atom's left argument
            if (!antecedentResults.Columns.Contains(leftArgumentString))
                return report;
            #endregion

            //Materialize members of the atom class (only if taxonomy protection has been requested)
            RDFOntologyData atomClassMembers =
                options.EnforceTaxonomyProtection ? RDFOntologyHelper.GetMembersOf(ontology, (RDFOntologyClass)this.Predicate)
                                                  : new RDFOntologyData();

            //Iterate the antecedent results table to materialize the atom's reasoner evidences
            IEnumerator rowsEnum = antecedentResults.Rows.GetEnumerator();
            while (rowsEnum.MoveNext())
            {
                DataRow currentRow = (DataRow)rowsEnum.Current;

                #region Guards
                //The current row MUST have a BOUND value in the column corresponding to the atom's left argument
                if (currentRow.IsNull(leftArgumentString))
                    continue;
                #endregion

                //Parse the value of the column corresponding to the atom's left argument
                RDFPatternMember leftArgumentValue = RDFQueryUtilities.ParseRDFPatternMember(currentRow[leftArgumentString].ToString());
                if (leftArgumentValue is RDFResource leftArgumentValueResource)
                {
                    //Search the fact in the ontology
                    RDFOntologyFact fact = ontology.Data.SelectFact(leftArgumentValueResource.ToString());
                    if (fact == null)
                        fact = new RDFOntologyFact(leftArgumentValueResource);

                    //Protect atom's inferences with implicit taxonomy checks (only if taxonomy protection has been requested)
                    if (!options.EnforceTaxonomyProtection || atomClassMembers.Facts.ContainsKey(fact.PatternMemberID))
                    {
                        //Create the inference as a taxonomy entry
                        RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(fact, type, (RDFOntologyClass)this.Predicate)
                                                                .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.ClassType.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, this.ToString(), nameof(RDFOntologyData.Relations.ClassType), sem_inf));
                    }
                }
            }

            return report;
        }
        #endregion
    }

}