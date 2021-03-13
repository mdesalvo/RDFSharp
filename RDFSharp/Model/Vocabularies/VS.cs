/*
   Copyright 2012-2020 Marco De Salvo

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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies.
    /// </summary>
    public static partial class RDFVocabulary
    {
        #region VS
        /// <summary>
        /// VS represents the Vocabulary-Status vocabulary.
        /// </summary>
        public static class VS
        {

            #region Properties
            /// <summary>
            /// vs
            /// </summary>
            public static readonly string PREFIX = "vs";

            /// <summary>
            /// http://www.w3.org/2003/06/sw-vocab-status/ns#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2003/06/sw-vocab-status/ns#";

            /// <summary>
            /// http://www.w3.org/2003/06/sw-vocab-status/ns#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2003/06/sw-vocab-status/ns#";

            /// <summary>
            /// vs:term_status
            /// </summary>
            public static readonly RDFResource TERM_STATUS = new RDFResource(string.Concat(VS.BASE_URI, "term_status"));

            /// <summary>
            /// vs:stable
            /// </summary>
            public static readonly RDFResource STABLE = new RDFResource(string.Concat(VS.BASE_URI, "stable"));

            /// <summary>
            /// vs:testing
            /// </summary>
            public static readonly RDFResource TESTING = new RDFResource(string.Concat(VS.BASE_URI, "testing"));

            /// <summary>
            /// vs:unstable
            /// </summary>
            public static readonly RDFResource UNSTABLE = new RDFResource(string.Concat(VS.BASE_URI, "unstable"));
            #endregion

        }
        #endregion
    }
}