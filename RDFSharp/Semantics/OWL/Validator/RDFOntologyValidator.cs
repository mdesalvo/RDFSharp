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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace RDFSharp.Semantics.OWL
{

    /// <summary>
    /// RDFOntologyValidator analyzes a given ontology through a set of RDFS/OWL-DL rules
    /// in order to find error and inconsistency evidences affecting its model and data.
    /// </summary>
    public static class RDFOntologyValidator
    {

        #region Properties
        /// <summary>
        /// List of rules applied by the ontology validator
        /// </summary>
        internal static List<RDFOntologyValidatorRule> Rules { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Static-ctor to build an ontology validator
        /// </summary>
        static RDFOntologyValidator()
        {
            Rules = new List<RDFOntologyValidatorRule>()
            {
                #region BASE
                //Vocabulary_Disjointness
                new RDFOntologyValidatorRule(
                    "Vocabulary_Disjointness",
                    "This OWL-DL rule checks for disjointness of vocabulary of classes, properties and facts",
                    RDFOntologyValidatorRuleset.Vocabulary_Disjointness),

                //Vocabulary_Declaration
                new RDFOntologyValidatorRule(
                    "Vocabulary_Declaration",
                    "This OWL-DL rule checks for complete declaration of classes, properties and facts",
                    RDFOntologyValidatorRuleset.Vocabulary_Declaration),

                //Domain_Range
                new RDFOntologyValidatorRule(
                    "Domain_Range",
                    "This RDFS rule checks for consistency of rdfs:domain and rdfs:range axioms",
                    RDFOntologyValidatorRuleset.Domain_Range),

                //InverseOf
                new RDFOntologyValidatorRule(
                    "InverseOf",
                    "This OWL-DL rule checks for consistency of owl:inverseOf axioms",
                    RDFOntologyValidatorRuleset.InverseOf),

                //SymmetricProperty
                new RDFOntologyValidatorRule(
                    "SymmetricProperty",
                    "This OWL-DL rule checks for consistency of owl:SymmetricProperty axioms",
                    RDFOntologyValidatorRuleset.SymmetricProperty),

                //AsymmetricProperty [OWL2]
                new RDFOntologyValidatorRule(
                    "AsymmetricProperty",
                    "This OWL2 rule checks for consistency of owl:AsymmetricProperty axioms",
                    RDFOntologyValidatorRuleset.AsymmetricProperty),

                //IrreflexiveProperty [OWL2]
                new RDFOntologyValidatorRule(
                    "IrreflexiveProperty",
                    "This OWL2 rule checks for consistency of owl:IrreflexiveProperty axioms",
                    RDFOntologyValidatorRuleset.IrreflexiveProperty),

                //PropertyDisjoint [OWL2]
                new RDFOntologyValidatorRule(
                    "PropertyDisjoint",
                    "This OWL2 rule checks for consistency of owl:propertyDisjointWith axioms",
                    RDFOntologyValidatorRuleset.PropertyDisjoint),

                //NegativeAssertions [OWL2]
                new RDFOntologyValidatorRule(
                    "NegativeAssertions",
                    "This OWL2 rule checks for consistency of owl:NegativePropertyAssertion axioms",
                    RDFOntologyValidatorRuleset.NegativeAssertions),

                //HasKey [OWL2]
                new RDFOntologyValidatorRule(
                    "HasKey",
                    "This OWL2 rule checks for consistency of owl:HasKey axioms",
                    RDFOntologyValidatorRuleset.HasKey),

                //PropertyChainAxiom [OWL2]
                new RDFOntologyValidatorRule(
                    "PropertyChainAxiom",
                    "This OWL2 rule checks for consistency of owl:PropertyChainAxiom axioms",
                    RDFOntologyValidatorRuleset.PropertyChainAxiom),

                //ClassType
                new RDFOntologyValidatorRule(
                    "ClassType",
                    "This OWL-DL rule checks for consistency of rdf:type axioms",
                    RDFOntologyValidatorRuleset.ClassType),

                //GlobalCardinalityConstraint
                new RDFOntologyValidatorRule(
                    "GlobalCardinalityConstraint",
                    "This OWL-DL rule checks for consistency of global cardinality constraints",
                    RDFOntologyValidatorRuleset.GlobalCardinalityConstraint),

                //LocalCardinalityConstraint
                new RDFOntologyValidatorRule(
                    "LocalCardinalityConstraint",
                    "This OWL-DL rule checks for consistency of local cardinality constraints",
                    RDFOntologyValidatorRuleset.LocalCardinalityConstraint),

                //Deprecation
                new RDFOntologyValidatorRule(
                    "Deprecation",
                    "This OWL-DL rule checks for usage of deprecated classes and properties",
                    RDFOntologyValidatorRuleset.Deprecation),

                //UntypedRestrictions
                new RDFOntologyValidatorRule(
                    "UntypedRestrictions",
                    "This OWL-DL rule checks for restrictions which haven't been specialized in any supported forms",
                    RDFOntologyValidatorRuleset.UntypedRestrictions)
                #endregion
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Validates the given ontology against a set of RDFS/OWL-DL rules, detecting errors and inconsistencies affecting its model and data.
        /// </summary>
        public static RDFOntologyValidatorReport Validate(this RDFOntology ontology)
        {
            RDFOntologyValidatorReport report = new RDFOntologyValidatorReport();
            if (ontology != null)
            {
                RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Validator is going to be applied on Ontology '{0}': this may require intensive processing, depending on size and complexity of domain knowledge.", ontology.Value));

                //STEP 1: Expand ontology
                RDFOntology expOntology = ontology.UnionWith(RDFBASEOntology.Instance);

                //STEP 2: Execute rules
                Parallel.ForEach(Rules, r => report.MergeEvidences(r.ExecuteRule(expOntology)));

                RDFSemanticsEvents.RaiseSemanticsInfo(string.Format("Validator has been applied on Ontology '{0}': found " + report.EvidencesCount + " evidences.", ontology.Value));
            }
            return report;
        }

        /// <summary>
        /// Asynchronously validates the given ontology against a set of RDFS/OWL-DL rules, detecting errors and inconsistencies affecting its model and data.
        /// </summary>
        public static Task<RDFOntologyValidatorReport> ValidateAsync(this RDFOntology ontology)
            => Task.Run(() => Validate(ontology));
        #endregion

    }

}