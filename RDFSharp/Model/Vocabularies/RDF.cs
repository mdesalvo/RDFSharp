﻿/*
   Copyright 2012-2025 Marco De Salvo

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
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies
    /// </summary>
    public static partial class RDFVocabulary
    {
        #region RDF
        /// <summary>
        /// RDF represents the W3C RDF vocabulary.
        /// </summary>
        public static class RDF
        {
            #region Properties
            /// <summary>
            /// rdf
            /// </summary>
            public const string PREFIX = "rdf";

            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#
            /// </summary>
            public const string BASE_URI = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#
            /// </summary>
            public const string DEREFERENCE_URI = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            /// <summary>
            /// rdf:Bag
            /// </summary>
            public static readonly RDFResource BAG = new RDFResource(string.Concat(BASE_URI,"Bag"));

            /// <summary>
            /// rdf:Seq
            /// </summary>
            public static readonly RDFResource SEQ = new RDFResource(string.Concat(BASE_URI,"Seq"));

            /// <summary>
            /// rdf:Alt
            /// </summary>
            public static readonly RDFResource ALT = new RDFResource(string.Concat(BASE_URI,"Alt"));

            /// <summary>
            /// rdf:Statement
            /// </summary>
            public static readonly RDFResource STATEMENT = new RDFResource(string.Concat(BASE_URI,"Statement"));

            /// <summary>
            /// rdf:Property
            /// </summary>
            public static readonly RDFResource PROPERTY = new RDFResource(string.Concat(BASE_URI,"Property"));

            /// <summary>
            /// rdf:XMLLiteral
            /// </summary>
            public static readonly RDFResource XML_LITERAL = new RDFResource(string.Concat(BASE_URI,"XMLLiteral"));

            /// <summary>
            /// rdf:PlainLiteral
            /// </summary>
            public static readonly RDFResource PLAIN_LITERAL = new RDFResource(string.Concat(BASE_URI, "PlainLiteral"));

            /// <summary>
            /// rdf:HTML
            /// </summary>
            public static readonly RDFResource HTML = new RDFResource(string.Concat(BASE_URI, "HTML"));

            /// <summary>
            /// rdf:JSON
            /// </summary>
            public static readonly RDFResource JSON = new RDFResource(string.Concat(BASE_URI, "JSON"));

            /// <summary>
            /// rdf:List
            /// </summary>
            public static readonly RDFResource LIST = new RDFResource(string.Concat(BASE_URI,"List"));

            /// <summary>
            /// rdf:nil
            /// </summary>
            public static readonly RDFResource NIL = new RDFResource(string.Concat(BASE_URI,"nil"));

            /// <summary>
            /// rdf:first
            /// </summary>
            public static readonly RDFResource FIRST = new RDFResource(string.Concat(BASE_URI, "first"));

            /// <summary>
            /// rdf:rest
            /// </summary>
            public static readonly RDFResource REST = new RDFResource(string.Concat(BASE_URI, "rest"));

            /// <summary>
            /// rdf:li
            /// </summary>
            public static readonly RDFResource LI = new RDFResource(string.Concat(BASE_URI,"li"));

            /// <summary>
            /// rdf:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(string.Concat(BASE_URI,"subject"));

            /// <summary>
            /// rdf:predicate
            /// </summary>
            public static readonly RDFResource PREDICATE = new RDFResource(string.Concat(BASE_URI,"predicate"));

            /// <summary>
            /// rdf:object
            /// </summary>
            public static readonly RDFResource OBJECT = new RDFResource(string.Concat(BASE_URI,"object"));

            /// <summary>
            /// rdf:type
            /// </summary>
            public static readonly RDFResource TYPE = new RDFResource(string.Concat(BASE_URI,"type"));

            /// <summary>
            /// rdf:value
            /// </summary>
            public static readonly RDFResource VALUE = new RDFResource(string.Concat(BASE_URI,"value"));

            /// <summary>
            /// rdf:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource(string.Concat(BASE_URI, "language"));

            /// <summary>
            /// rdf:langRange
            /// </summary>
            public static readonly RDFResource LANG_RANGE = new RDFResource(string.Concat(BASE_URI, "langRange"));

            /// <summary>
            /// rdf:langString
            /// </summary>
            public static readonly RDFResource LANG_STRING = new RDFResource(string.Concat(BASE_URI, "langString"));

            /// <summary>
            /// rdf:dirLangString
            /// </summary>
            public static readonly RDFResource DIR_LANG_STRING = new RDFResource(string.Concat(BASE_URI, "dirLangString"));

            /// <summary>
            /// rdf:direction
            /// </summary>
            public static readonly RDFResource DIRECTION = new RDFResource(string.Concat(BASE_URI, "direction"));

            /// <summary>
            /// rdf:reifies
            /// </summary>
            public static readonly RDFResource REIFIES = new RDFResource(string.Concat(BASE_URI, "reifies"));
            #endregion
        }
        #endregion
    }
}