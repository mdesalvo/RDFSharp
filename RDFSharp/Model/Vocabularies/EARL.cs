/*
   Copyright 2012-2022 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License"));
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies.
    /// </summary>
    public static partial class RDFVocabulary
    {
        #region EARL
        /// <summary>
        /// EARL represents the Evaluation-And-Report-Language vocabulary.
        /// </summary>
        public static class EARL
        {

            #region Properties
            /// <summary>
            /// earl
            /// </summary>
            public static readonly string PREFIX = "earl";

            /// <summary>
            /// http://www.w3.org/ns/earl#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/ns/earl#";

            /// <summary>
            /// http://www.w3.org/ns/earl#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/ns/earl#";

            /// <summary>
            /// earl:assertedBy
            /// </summary>
            public static readonly RDFResource ASSERTED_BY = new RDFResource(string.Concat(EARL.BASE_URI, "assertedBy"));

            /// <summary>
            /// earl:Assertion
            /// </summary>
            public static readonly RDFResource ASSERTION = new RDFResource(string.Concat(EARL.BASE_URI, "Assertion"));

            /// <summary>
            /// earl:Assertor
            /// </summary>
            public static readonly RDFResource ASSERTOR = new RDFResource(string.Concat(EARL.BASE_URI, "Assertor"));

            /// <summary>
            /// earl:automatic
            /// </summary>
            public static readonly RDFResource AUTOMATIC = new RDFResource(string.Concat(EARL.BASE_URI, "automatic"));

            /// <summary>
            /// earl:cantTell
            /// </summary>
            public static readonly RDFResource CANT_TELL = new RDFResource(string.Concat(EARL.BASE_URI, "cantTell"));

            /// <summary>
            /// earl:CannotTell
            /// </summary>
            public static readonly RDFResource CANNOT_TELL = new RDFResource(string.Concat(EARL.BASE_URI, "CannotTell"));

            /// <summary>
            /// earl:Fail
            /// </summary>
            public static readonly RDFResource FAIL = new RDFResource(string.Concat(EARL.BASE_URI, "Fail"));

            /// <summary>
            /// earl:failed
            /// </summary>
            public static readonly RDFResource FAILED = new RDFResource(string.Concat(EARL.BASE_URI, "failed"));

            /// <summary>
            /// earl:info
            /// </summary>
            public static readonly RDFResource INFO = new RDFResource(string.Concat(EARL.BASE_URI, "info"));

            /// <summary>
            /// earl:inapplicable
            /// </summary>
            public static readonly RDFResource INAPPLICABLE = new RDFResource(string.Concat(EARL.BASE_URI, "inapplicable"));

            /// <summary>
            /// earl:mainAssertor
            /// </summary>
            public static readonly RDFResource MAIN_ASSERTOR = new RDFResource(string.Concat(EARL.BASE_URI, "mainAssertor"));

            /// <summary>
            /// earl:manual
            /// </summary>
            public static readonly RDFResource MANUAL = new RDFResource(string.Concat(EARL.BASE_URI, "manual"));

            /// <summary>
            /// earl:mode
            /// </summary>
            public static readonly RDFResource MODE = new RDFResource(string.Concat(EARL.BASE_URI, "mode"));

            /// <summary>
            /// earl:NotApplicable
            /// </summary>
            public static readonly RDFResource NOT_APPLICABLE = new RDFResource(string.Concat(EARL.BASE_URI, "NotApplicable"));

            /// <summary>
            /// earl:NotTested
            /// </summary>
            public static readonly RDFResource NOT_TESTED = new RDFResource(string.Concat(EARL.BASE_URI, "NotTested"));

            /// <summary>
            /// earl:outcome
            /// </summary>
            public static readonly RDFResource OUTCOME = new RDFResource(string.Concat(EARL.BASE_URI, "outcome"));

            /// <summary>
            /// earl:OutcomeValue
            /// </summary>
            public static readonly RDFResource OUTCOME_VALUE = new RDFResource(string.Concat(EARL.BASE_URI, "OutcomeValue"));

            /// <summary>
            /// earl:Pass
            /// </summary>
            public static readonly RDFResource PASS = new RDFResource(string.Concat(EARL.BASE_URI, "Pass"));

            /// <summary>
            /// earl:passed
            /// </summary>
            public static readonly RDFResource PASSED = new RDFResource(string.Concat(EARL.BASE_URI, "passed"));

            /// <summary>
            /// earl:pointer
            /// </summary>
            public static readonly RDFResource POINTER = new RDFResource(string.Concat(EARL.BASE_URI, "pointer"));

            /// <summary>
            /// earl:result
            /// </summary>
            public static readonly RDFResource RESULT = new RDFResource(string.Concat(EARL.BASE_URI, "result"));

            /// <summary>
            /// earl:semiAuto
            /// </summary>
            public static readonly RDFResource SEMIAUTO = new RDFResource(string.Concat(EARL.BASE_URI, "semiAuto"));

            /// <summary>
            /// earl:Software
            /// </summary>
            public static readonly RDFResource SOFTWARE = new RDFResource(string.Concat(EARL.BASE_URI, "Software"));

            /// <summary>
            /// earl:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(string.Concat(EARL.BASE_URI, "subject"));

            /// <summary>
            /// earl:test
            /// </summary>
            public static readonly RDFResource TEST = new RDFResource(string.Concat(EARL.BASE_URI, "test"));

            /// <summary>
            /// earl:TestCase
            /// </summary>
            public static readonly RDFResource TEST_CASE = new RDFResource(string.Concat(EARL.BASE_URI, "TestCase"));

            /// <summary>
            /// earl:TestCriterion
            /// </summary>
            public static readonly RDFResource TEST_CRITERION = new RDFResource(string.Concat(EARL.BASE_URI, "TestCriterion"));

            /// <summary>
            /// earl:TestMode
            /// </summary>
            public static readonly RDFResource TEST_MODE = new RDFResource(string.Concat(EARL.BASE_URI, "TestMode"));

            /// <summary>
            /// earl:TestRequirement
            /// </summary>
            public static readonly RDFResource TEST_REQUIREMENT = new RDFResource(string.Concat(EARL.BASE_URI, "TestRequirement"));

            /// <summary>
            /// earl:TestResult
            /// </summary>
            public static readonly RDFResource TEST_RESULT = new RDFResource(string.Concat(EARL.BASE_URI, "TestResult"));

            /// <summary>
            /// earl:TestSubject
            /// </summary>
            public static readonly RDFResource TEST_SUBJECT = new RDFResource(string.Concat(EARL.BASE_URI, "TestSubject"));

            /// <summary>
            /// earl:undisclosed
            /// </summary>
            public static readonly RDFResource UNDISCLOSED = new RDFResource(string.Concat(EARL.BASE_URI, "undisclosed"));

            /// <summary>
            /// earl:unknownMode
            /// </summary>
            public static readonly RDFResource UNKNOWN_MODE = new RDFResource(string.Concat(EARL.BASE_URI, "unknownMode"));

            /// <summary>
            /// earl:untested
            /// </summary>
            public static readonly RDFResource UNTESTED = new RDFResource(string.Concat(EARL.BASE_URI, "untested"));

            #endregion

        }
        #endregion
    }
}