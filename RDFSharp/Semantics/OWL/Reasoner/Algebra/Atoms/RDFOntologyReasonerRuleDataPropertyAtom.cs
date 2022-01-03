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
    /// RDFOntologyReasonerRuleDataPropertyAtom represents an atom inferring assertions relating ontology facts to literals
    /// </summary>
    public class RDFOntologyReasonerRuleDataPropertyAtom : RDFOntologyReasonerRuleAtom
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a data property atom with the given property and arguments
        /// </summary>
        public RDFOntologyReasonerRuleDataPropertyAtom(RDFOntologyDatatypeProperty ontologyDatatypeProperty, RDFVariable leftArgument, RDFVariable rightArgument)
            : base(ontologyDatatypeProperty, leftArgument, rightArgument) { }

        /// <summary>
        /// Default-ctor to build a data property atom with the given property and arguments
        /// </summary>
        public RDFOntologyReasonerRuleDataPropertyAtom(RDFOntologyDatatypeProperty ontologyDatatypeProperty, RDFVariable leftArgument, RDFOntologyLiteral rightArgument)
            : base(ontologyDatatypeProperty, leftArgument, rightArgument) { }
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

            //Materialize data property assertions of the atom predicate
            RDFOntologyTaxonomy assertions = ontology.Data.Relations.Assertions.SelectEntriesByPredicate(this.Predicate);
            if (this.RightArgument is RDFOntologyLiteral rightArgumentLiteral)
                assertions = assertions.SelectEntriesByObject(rightArgumentLiteral);
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
                                                      : ((RDFOntologyLiteral)this.RightArgument).Value;

                if (leftArgumentValue is RDFResource leftArgumentValueResource
                        && rightArgumentValue is RDFLiteral rightArgumentValueLiteral)
                {
                    //Search the fact in the ontology
                    RDFOntologyFact fact = ontology.Data.SelectFact(leftArgumentValueResource.ToString());
                    if (fact == null)
                        fact = new RDFOntologyFact(leftArgumentValueResource);

                    //Search the literal in the ontology
                    RDFOntologyLiteral literal = ontology.Data.SelectLiteral(rightArgumentValueLiteral.ToString());
                    if (literal == null)
                        literal = new RDFOntologyLiteral(rightArgumentValueLiteral);

                    //Protect atom's inferences with implicit taxonomy checks (only if taxonomy protection has been requested)
                    if (!options.EnforceTaxonomyProtection || RDFOntologyChecker.CheckAssertionCompatibility(ontology.Data, fact, (RDFOntologyDatatypeProperty)this.Predicate, literal))
                    {
                        //Create the inference as a taxonomy entry
                        RDFOntologyTaxonomyEntry sem_inf = new RDFOntologyTaxonomyEntry(fact, (RDFOntologyDatatypeProperty)this.Predicate, literal)
                                                                .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.Reasoner);

                        //Add the inference to the report
                        if (!ontology.Data.Relations.Assertions.ContainsEntry(sem_inf))
                            report.AddEvidence(new RDFOntologyReasonerEvidence(RDFSemanticsEnums.RDFOntologyReasonerEvidenceCategory.Data, this.ToString(), nameof(RDFOntologyData.Relations.Assertions), sem_inf));
                    }
                }
            }

            return report;
        }
        #endregion
    }

}