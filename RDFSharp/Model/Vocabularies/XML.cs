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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies.
    /// </summary>
    public static partial class RDFVocabulary
    {
        #region XML
        /// <summary>
        /// XML represents the XML vocabulary.
        /// </summary>
        public static class XML
        {

            #region Properties
            /// <summary>
            /// xml
            /// </summary>
            public static readonly string PREFIX = "xml";

            /// <summary>
            /// http://www.w3.org/XML/1998/namespace
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/XML/1998/namespace";

            /// <summary>
            /// http://www.w3.org/XML/1998/namespace
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/XML/1998/namespace";

            /// <summary>
            /// xml:lang
            /// </summary>
            public static readonly RDFResource LANG = new RDFResource(string.Concat(XML.BASE_URI, "#lang"));

            /// <summary>
            /// xml:base
            /// </summary>
            public static readonly RDFResource BASE = new RDFResource(string.Concat(XML.BASE_URI, "#base"));
            #endregion

        }
        #endregion
    }
}