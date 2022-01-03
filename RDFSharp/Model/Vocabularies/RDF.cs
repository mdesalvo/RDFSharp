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
        #region RDF
        /// <summary>
        /// RDF represents the RDF vocabulary.
        /// </summary>
        public static class RDF
        {

            #region Properties
            /// <summary>
            /// rdf
            /// </summary>
            public static readonly string PREFIX = "rdf";

            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            /// <summary>
            /// rdf:Bag
            /// </summary>
            public static readonly RDFResource BAG = new RDFResource(string.Concat(RDF.BASE_URI,"Bag"));

            /// <summary>
            /// rdf:Seq
            /// </summary>
            public static readonly RDFResource SEQ = new RDFResource(string.Concat(RDF.BASE_URI,"Seq"));

            /// <summary>
            /// rdf:Alt
            /// </summary>
            public static readonly RDFResource ALT = new RDFResource(string.Concat(RDF.BASE_URI,"Alt"));

            /// <summary>
            /// rdf:Statement
            /// </summary>
            public static readonly RDFResource STATEMENT = new RDFResource(string.Concat(RDF.BASE_URI,"Statement"));

            /// <summary>
            /// rdf:Property
            /// </summary>
            public static readonly RDFResource PROPERTY = new RDFResource(string.Concat(RDF.BASE_URI,"Property"));

            /// <summary>
            /// rdf:XMLLiteral
            /// </summary>
            public static readonly RDFResource XML_LITERAL = new RDFResource(string.Concat(RDF.BASE_URI,"XMLLiteral"));

            /// <summary>
            /// rdf:HTML
            /// </summary>
            public static readonly RDFResource HTML = new RDFResource(string.Concat(RDF.BASE_URI, "HTML"));

            /// <summary>
            /// rdf:JSON
            /// </summary>
            public static readonly RDFResource JSON = new RDFResource(string.Concat(RDF.BASE_URI, "JSON"));

            /// <summary>
            /// rdf:List
            /// </summary>
            public static readonly RDFResource LIST = new RDFResource(string.Concat(RDF.BASE_URI,"List"));

            /// <summary>
            /// rdf:nil
            /// </summary>
            public static readonly RDFResource NIL = new RDFResource(string.Concat(RDF.BASE_URI,"nil"));

            /// <summary>
            /// rdf:li
            /// </summary>
            public static readonly RDFResource LI = new RDFResource(string.Concat(RDF.BASE_URI,"li"));

            /// <summary>
            /// rdf:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(string.Concat(RDF.BASE_URI,"subject"));

            /// <summary>
            /// rdf:predicate
            /// </summary>
            public static readonly RDFResource PREDICATE = new RDFResource(string.Concat(RDF.BASE_URI,"predicate"));

            /// <summary>
            /// rdf:object
            /// </summary>
            public static readonly RDFResource OBJECT = new RDFResource(string.Concat(RDF.BASE_URI,"object"));

            /// <summary>
            /// rdf:type
            /// </summary>
            public static readonly RDFResource TYPE = new RDFResource(string.Concat(RDF.BASE_URI,"type"));

            /// <summary>
            /// rdf:value
            /// </summary>
            public static readonly RDFResource VALUE = new RDFResource(string.Concat(RDF.BASE_URI,"value"));

            /// <summary>
            /// rdf:first
            /// </summary>
            public static readonly RDFResource FIRST = new RDFResource(string.Concat(RDF.BASE_URI,"first"));

            /// <summary>
            /// rdf:rest
            /// </summary>
            public static readonly RDFResource REST = new RDFResource(string.Concat(RDF.BASE_URI,"rest"));
            #endregion

        }
        #endregion
    }
}