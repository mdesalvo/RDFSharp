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

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFEARLOntology represents an OWL-DL ontology implementation of EARL vocabulary
    /// </summary>
    public static class RDFEARLOntology
    {

        #region Properties
        /// <summary>
        /// Singleton instance of the EARL ontology
        /// </summary>
        public static RDFOntology Instance { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the EARL ontology
        /// </summary>
        static RDFEARLOntology()
        {

            #region Declarations

            #region Ontology
            Instance = new RDFOntology(new RDFResource(RDFVocabulary.EARL.BASE_URI));
            #endregion

            #region Classes
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.ASSERTION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.ASSERTOR.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.CANNOT_TELL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.FAIL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.NOT_APPLICABLE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.NOT_TESTED.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.OUTCOME_VALUE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.PASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.SOFTWARE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.TEST_CASE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.TEST_CRITERION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.TEST_MODE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.TEST_REQUIREMENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.TEST_RESULT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.EARL.TEST_SUBJECT.ToRDFOntologyClass());

            //OWL-DL Completeness
            Instance.Model.ClassModel.AddClass(new RDFOntologyClass(new RDFResource("http://usefulinc.com/ns/doap#Project")));
            Instance.Model.ClassModel.AddClass(new RDFOntologyClass(new RDFResource("http://www.w3.org/2009/pointers#Pointer")));
            #endregion

            #region Properties
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.ASSERTED_BY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.INFO.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.MAIN_ASSERTOR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.MODE.ToRDFOntologyObjectProperty());            
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.OUTCOME.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.POINTER.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.RESULT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.SUBJECT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.EARL.TEST.ToRDFOntologyObjectProperty());
            #endregion

            #region Facts
            Instance.Data.AddFact(RDFVocabulary.EARL.AUTOMATIC.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.CANT_TELL.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.INAPPLICABLE.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.FAILED.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.MANUAL.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.PASSED.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.SEMIAUTO.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.UNDISCLOSED.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.UNKNOWN_MODE.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.EARL.UNTESTED.ToRDFOntologyFact());
            #endregion

            #endregion

            #region Taxonomies

            #region ClassModel

            //SubClassOf
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_REQUIREMENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_CRITERION.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_CASE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_CRITERION.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.PASS.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.OUTCOME_VALUE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.FAIL.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.OUTCOME_VALUE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.CANNOT_TELL.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.OUTCOME_VALUE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.NOT_APPLICABLE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.OUTCOME_VALUE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.NOT_TESTED.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.OUTCOME_VALUE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.SOFTWARE.ToString()), Instance.Model.ClassModel.SelectClass("http://usefulinc.com/ns/doap#Project"));

            #endregion

            #region PropertyModel

            //Domain/Range
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.ASSERTED_BY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.ASSERTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.ASSERTED_BY.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.ASSERTOR.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.SUBJECT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.ASSERTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.SUBJECT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_SUBJECT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.TEST.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.ASSERTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.TEST.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_CRITERION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.RESULT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.ASSERTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.RESULT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.MODE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.ASSERTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.MODE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_MODE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.MAIN_ASSERTOR.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.ASSERTOR.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.MAIN_ASSERTOR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.ASSERTOR.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.OUTCOME.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.OUTCOME.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.OUTCOME_VALUE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.POINTER.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.POINTER.ToString()).SetRange(Instance.Model.ClassModel.SelectClass("http://www.w3.org/2009/pointers#Pointer"));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.EARL.INFO.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_RESULT.ToString()));

            #endregion

            #region Data

            //ClassType
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.AUTOMATIC.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_MODE.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.MANUAL.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_MODE.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.SEMIAUTO.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_MODE.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.UNDISCLOSED.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_MODE.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.UNKNOWN_MODE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.TEST_MODE.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.PASSED.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.PASS.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.FAILED.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.FAIL.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.CANT_TELL.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.CANNOT_TELL.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.INAPPLICABLE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.NOT_APPLICABLE.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.EARL.UNTESTED.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.EARL.NOT_TESTED.ToString()));

            #endregion

            #endregion

        }
        #endregion

    }

}