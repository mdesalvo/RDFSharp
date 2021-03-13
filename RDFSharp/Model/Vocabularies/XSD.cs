/*
   Copyright 2012-2020 Marco De Salvo

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
        #region XSD
        /// <summary>
        /// XSD represents the XSD vocabulary.
        /// </summary>
        public static class XSD
        {

            #region Properties
            /// <summary>
            /// xsd
            /// </summary>
            public static readonly string PREFIX = "xsd";

            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2001/XMLSchema#";

            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2001/XMLSchema#";

            /// <summary>
            /// xsd:string
            /// </summary>
            public static readonly RDFResource STRING = new RDFResource(string.Concat(XSD.BASE_URI,"string"));

            /// <summary>
            /// xsd:boolean
            /// </summary>
            public static readonly RDFResource BOOLEAN = new RDFResource(string.Concat(XSD.BASE_URI,"boolean"));

            /// <summary>
            /// xsd:decimal
            /// </summary>
            public static readonly RDFResource DECIMAL = new RDFResource(string.Concat(XSD.BASE_URI,"decimal"));

            /// <summary>
            /// xsd:float
            /// </summary>
            public static readonly RDFResource FLOAT = new RDFResource(string.Concat(XSD.BASE_URI,"float"));

            /// <summary>
            /// xsd:double
            /// </summary>
            public static readonly RDFResource DOUBLE = new RDFResource(string.Concat(XSD.BASE_URI,"double"));

            /// <summary>
            /// xsd:positiveInteger
            /// </summary>
            public static readonly RDFResource POSITIVE_INTEGER = new RDFResource(string.Concat(XSD.BASE_URI,"positiveInteger"));

            /// <summary>
            /// xsd:negativeInteger
            /// </summary>
            public static readonly RDFResource NEGATIVE_INTEGER = new RDFResource(string.Concat(XSD.BASE_URI,"negativeInteger"));

            /// <summary>
            /// xsd:nonPositiveInteger
            /// </summary>
            public static readonly RDFResource NON_POSITIVE_INTEGER = new RDFResource(string.Concat(XSD.BASE_URI,"nonPositiveInteger"));

            /// <summary>
            /// xsd:nonNegativeInteger
            /// </summary>
            public static readonly RDFResource NON_NEGATIVE_INTEGER = new RDFResource(string.Concat(XSD.BASE_URI,"nonNegativeInteger"));

            /// <summary>
            /// xsd:integer
            /// </summary>
            public static readonly RDFResource INTEGER = new RDFResource(string.Concat(XSD.BASE_URI,"integer"));

            /// <summary>
            /// xsd:long
            /// </summary>
            public static readonly RDFResource LONG = new RDFResource(string.Concat(XSD.BASE_URI,"long"));

            /// <summary>
            /// xsd:unsignedLong
            /// </summary>
            public static readonly RDFResource UNSIGNED_LONG = new RDFResource(string.Concat(XSD.BASE_URI,"unsignedLong"));

            /// <summary>
            /// xsd:int
            /// </summary>
            public static readonly RDFResource INT = new RDFResource(string.Concat(XSD.BASE_URI,"int"));

            /// <summary>
            /// xsd:unsignedInt
            /// </summary>
            public static readonly RDFResource UNSIGNED_INT = new RDFResource(string.Concat(XSD.BASE_URI,"unsignedInt"));

            /// <summary>
            /// xsd:short
            /// </summary>
            public static readonly RDFResource SHORT = new RDFResource(string.Concat(XSD.BASE_URI,"short"));

            /// <summary>
            /// xsd:unsignedShort
            /// </summary>
            public static readonly RDFResource UNSIGNED_SHORT = new RDFResource(string.Concat(XSD.BASE_URI,"unsignedShort"));

            /// <summary>
            /// xsd:byte
            /// </summary>
            public static readonly RDFResource BYTE = new RDFResource(string.Concat(XSD.BASE_URI,"byte"));

            /// <summary>
            /// xsd:unsignedByte
            /// </summary>
            public static readonly RDFResource UNSIGNED_BYTE = new RDFResource(string.Concat(XSD.BASE_URI,"unsignedByte"));

            /// <summary>
            /// xsd:duration
            /// </summary>
            public static readonly RDFResource DURATION = new RDFResource(string.Concat(XSD.BASE_URI,"duration"));

            /// <summary>
            /// xsd:dateTime
            /// </summary>
            public static readonly RDFResource DATETIME = new RDFResource(string.Concat(XSD.BASE_URI,"dateTime"));

            /// <summary>
            /// xsd:time
            /// </summary>
            public static readonly RDFResource TIME = new RDFResource(string.Concat(XSD.BASE_URI,"time"));

            /// <summary>
            /// xsd:date
            /// </summary>
            public static readonly RDFResource DATE = new RDFResource(string.Concat(XSD.BASE_URI,"date"));

            /// <summary>
            /// xsd:gYearMonth
            /// </summary>
            public static readonly RDFResource G_YEAR_MONTH = new RDFResource(string.Concat(XSD.BASE_URI,"gYearMonth"));

            /// <summary>
            /// xsd:gYear
            /// </summary>
            public static readonly RDFResource G_YEAR = new RDFResource(string.Concat(XSD.BASE_URI,"gYear"));

            /// <summary>
            /// xsd:gMonth
            /// </summary>
            public static readonly RDFResource G_MONTH = new RDFResource(string.Concat(XSD.BASE_URI,"gMonth"));

            /// <summary>
            /// xsd:gMonthDay
            /// </summary>
            public static readonly RDFResource G_MONTH_DAY = new RDFResource(string.Concat(XSD.BASE_URI,"gMonthDay"));

            /// <summary>
            /// xsd:gDay
            /// </summary>
            public static readonly RDFResource G_DAY = new RDFResource(string.Concat(XSD.BASE_URI,"gDay"));

            /// <summary>
            /// xsd:hexBinary
            /// </summary>
            public static readonly RDFResource HEX_BINARY = new RDFResource(string.Concat(XSD.BASE_URI,"hexBinary"));

            /// <summary>
            /// xsd:base64Binary
            /// </summary>
            public static readonly RDFResource BASE64_BINARY = new RDFResource(string.Concat(XSD.BASE_URI,"base64Binary"));

            /// <summary>
            /// xsd:anyURI
            /// </summary>
            public static readonly RDFResource ANY_URI = new RDFResource(string.Concat(XSD.BASE_URI,"anyURI"));

            /// <summary>
            /// xsd:QName
            /// </summary>
            public static readonly RDFResource QNAME = new RDFResource(string.Concat(XSD.BASE_URI,"QName"));

            /// <summary>
            /// xsd:NOTATION
            /// </summary>
            public static readonly RDFResource NOTATION = new RDFResource(string.Concat(XSD.BASE_URI,"NOTATION"));

            /// <summary>
            /// xsd:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource(string.Concat(XSD.BASE_URI,"language"));

            /// <summary>
            /// xsd:normalizedString
            /// </summary>
            public static readonly RDFResource NORMALIZED_STRING = new RDFResource(string.Concat(XSD.BASE_URI,"normalizedString"));

            /// <summary>
            /// xsd:token
            /// </summary>
            public static readonly RDFResource TOKEN = new RDFResource(string.Concat(XSD.BASE_URI,"token"));

            /// <summary>
            /// xsd:NMToken
            /// </summary>
            public static readonly RDFResource NMTOKEN = new RDFResource(string.Concat(XSD.BASE_URI,"NMToken"));

            /// <summary>
            /// xsd:Name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(string.Concat(XSD.BASE_URI,"Name"));

            /// <summary>
            /// xsd:NCName
            /// </summary>
            public static readonly RDFResource NCNAME = new RDFResource(string.Concat(XSD.BASE_URI,"NCName"));

            /// <summary>
            /// xsd:ID
            /// </summary>
            public static readonly RDFResource ID = new RDFResource(string.Concat(XSD.BASE_URI,"ID"));
            #endregion

        }
        #endregion
    }
}