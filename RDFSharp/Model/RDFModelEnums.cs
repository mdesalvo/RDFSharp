/*
   Copyright 2012-2025 Marco De Salvo

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

using System.ComponentModel;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFModelEnums represents a collector for all the enumerations used by the "RDFSharp.Model" namespace
    /// </summary>
    public static class RDFModelEnums
    {
        /// <summary>
        /// RDFFormats represents an enumeration for supported RDF graph serialization data formats.
        /// </summary>
        public enum RDFFormats
        {
            /// <summary>
            /// N-Triples serialization (https://www.w3.org/TR/n-triples/)
            /// </summary>
            NTriples = 0,
            /// <summary>
            /// Turtle serialization (https://www.w3.org/TR/turtle/)
            /// </summary>
            Turtle = 1,
            /// <summary>
            /// TriX serialization (http://www.w3.org/2004/03/trix/trix-1/)
            /// </summary>
            TriX = 2,
            /// <summary>
            /// XML serialization (https://www.w3.org/TR/rdf11-xml/)
            /// </summary>
            RdfXml = 3
        };

        /// <summary>
        /// RDFTripleFlavors represents an enumeration for possible triple flavors.
        /// </summary>
        public enum RDFTripleFlavors
        {
            /// <summary>
            /// Indicates that the object of the triple is a resource
            /// </summary>
            SPO = 1,
            /// <summary>
            /// Indicates that the object of the triple is a literal
            /// </summary>
            SPL = 2
        };

        /// <summary>
        /// RDFDatatypes represents an enumeration for supported datatypes (RDF/RDFS/XSD).
        /// </summary>
        public enum RDFDatatypes
        {
            /// <summary>
            /// http://www.w3.org/2000/01/rdf-schema#Literal
            /// </summary>
			[Description("http://www.w3.org/2000/01/rdf-schema#Literal")]
            RDFS_LITERAL = 0,
            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral
            /// </summary>
			[Description("http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral")]
            RDF_XMLLITERAL = 1,
            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#HTML
            /// </summary>
			[Description("http://www.w3.org/1999/02/22-rdf-syntax-ns#HTML")]
            RDF_HTML = 2,
            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#JSON
            /// </summary>
			[Description("http://www.w3.org/1999/02/22-rdf-syntax-ns#JSON")]
            RDF_JSON = 3,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#string
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#string")]
            XSD_STRING = 4,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#boolean
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#boolean")]
            XSD_BOOLEAN = 5,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#decimal
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#decimal")]
            XSD_DECIMAL = 6,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#float
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#float")]
            XSD_FLOAT = 7,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#double
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#double")]
            XSD_DOUBLE = 8,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#positiveInteger
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#positiveInteger")]
            XSD_POSITIVEINTEGER = 9,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#negativeInteger
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#negativeInteger")]
            XSD_NEGATIVEINTEGER = 10,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#nonPositiveInteger
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#nonPositiveInteger")]
            XSD_NONPOSITIVEINTEGER = 11,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#nonNegativeInteger
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#nonNegativeInteger")]
            XSD_NONNEGATIVEINTEGER = 12,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#integer
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#integer")]
            XSD_INTEGER = 13,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#long
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#long")]
            XSD_LONG = 14,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#int
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#int")]
            XSD_INT = 15,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#short
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#short")]
            XSD_SHORT = 16,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#byte
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#byte")]
            XSD_BYTE = 17,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#unsignedLong
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#unsignedLong")]
            XSD_UNSIGNEDLONG = 18,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#unsignedInt
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#unsignedInt")]
            XSD_UNSIGNEDINT = 19,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#unsignedShort
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#unsignedShort")]
            XSD_UNSIGNEDSHORT = 20,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#unsignedByte
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#unsignedByte")]
            XSD_UNSIGNEDBYTE = 21,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#duration
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#duration")]
            XSD_DURATION = 22,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#dateTime
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#dateTime")]
            XSD_DATETIME = 23,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#dateTimeStamp
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#dateTimeStamp")]
            XSD_DATETIMESTAMP = 24,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#date
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#date")]
            XSD_DATE = 25,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#time
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#time")]
            XSD_TIME = 26,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gYear
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#gYear")]
            XSD_GYEAR = 27,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gMonth
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#gMonth")]
            XSD_GMONTH = 28,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gDay
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#gDay")]
            XSD_GDAY = 29,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gYearMonth
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#gYearMonth")]
            XSD_GYEARMONTH = 30,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gMonthDay
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#gMonthDay")]
            XSD_GMONTHDAY = 31,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#hexBinary
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#hexBinary")]
            XSD_HEXBINARY = 32,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#base64Binary
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#base64Binary")]
            XSD_BASE64BINARY = 33,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#anyURI
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#anyURI")]
            XSD_ANYURI = 34,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#QName
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#QName")]
            XSD_QNAME = 35,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#NOTATION
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#NOTATION")]
            XSD_NOTATION = 36,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#language
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#language")]
            XSD_LANGUAGE = 37,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#normalizedString
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#normalizedString")]
            XSD_NORMALIZEDSTRING = 38,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#token
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#token")]
            XSD_TOKEN = 39,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#NMToken
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#NMToken")]
            XSD_NMTOKEN = 40,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#Name
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#Name")]
            XSD_NAME = 41,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#NCName
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#NCName")]
            XSD_NCNAME = 42,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#ID
            /// </summary>
			[Description("http://www.w3.org/2001/XMLSchema#ID")]
            XSD_ID = 43,
            /// <summary>
            /// http://www.opengis.net/ont/geosparql#wktLiteral
            /// </summary>
			[Description("http://www.opengis.net/ont/geosparql#wktLiteral")]
            GEOSPARQL_WKT = 44,
            /// <summary>
            /// http://www.opengis.net/ont/geosparql#gmlLiteral
            /// </summary>
			[Description("http://www.opengis.net/ont/geosparql#gmlLiteral")]
            GEOSPARQL_GML = 45,
            /// <summary>
            /// http://www.w3.org/2006/time#generalDay
            /// </summary>
			[Description("http://www.w3.org/2006/time#generalDay")]
            TIME_GENERALDAY = 46,
            /// <summary>
            /// http://www.w3.org/2006/time#generalMonth
            /// </summary>
			[Description("http://www.w3.org/2006/time#generalMonth")]
            TIME_GENERALMONTH = 47,
            /// <summary>
            /// http://www.w3.org/2006/time#generalYear
            /// </summary>
			[Description("http://www.w3.org/2006/time#generalYear")]
            TIME_GENERALYEAR = 48,
            /// <summary>
            /// http://www.w3.org/2002/07/owl#real
            /// </summary>
			[Description("http://www.w3.org/2002/07/owl#real")]
            OWL_REAL = 49
        };

        /// <summary>
        /// RDFContainerTypes represents an enumeration for supported container types.
        /// </summary>
        public enum RDFContainerTypes
        {
            /// <summary>
            /// Represents an unordered list which allows duplicates
            /// </summary>
            Bag = 0,
            /// <summary>
            /// Represents an ordered list which allows duplicates
            /// </summary>
            Seq = 1,
            /// <summary>
            /// Represents an unordered list which does not allow duplicates
            /// </summary>
            Alt = 2
        };

        /// <summary>
        /// RDFItemTypes represents an enumeration for acceptable RDFContainer and RDFCollection item types.
        /// </summary>
        public enum RDFItemTypes
        {
            /// <summary>
            /// Indicates that a container/collection accepts only resources
            /// </summary>
            Resource = 1,
            /// <summary>
            /// Indicates that a container/collection accepts only literals
            /// </summary>
            Literal = 2
        };

        /// <summary>
        /// RDFPlainLiteralDirections represents an enumeration for supported directions of plain literal's value
        /// </summary>
        public enum RDFPlainLiteralDirections
        {
            /// <summary>
            /// Indicates that the value of the plain literal should be read in "left to right" direction
            /// </summary>
            LTR = 1,
            /// <summary>
            /// Indicates that the value of the plain literal should be read in "right to left" direction
            /// </summary>
            RTL = 2
        };
    }
}