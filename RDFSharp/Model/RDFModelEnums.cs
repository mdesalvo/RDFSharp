/*
   Copyright 2012-2017 Marco De Salvo

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
    /// RDFModelEnums represents a collector for all the enumerations used by the "RDFSharp.Model" namespace
    /// </summary>
    public static class RDFModelEnums {

        /// <summary>
        /// RDFFormats represents an enumeration for supported RDF graph serialization data formats.
        /// </summary>
        public enum RDFFormats {
            /// <summary>
            /// N-Triples serialization
            /// </summary>
            NTriples = 0,
            /// <summary>
            /// Turtle serialization
            /// </summary>
            Turtle = 1,
            /// <summary>
            /// TriX serialization
            /// </summary>
            TriX = 2,
            /// <summary>
            /// XML serialization
            /// </summary>
            RdfXml = 3
        };

        /// <summary>
        /// RDFTripleFlavors represents an enumeration for possible triple pattern flavors.
        /// </summary>
        public enum RDFTripleFlavors  {
            /// <summary>
            /// Indicates that the object of the triple is a resource
            /// </summary>
            SPO = 1,
            /// <summary>
            /// Indicates that the object of the triple is a literal
            /// </summary>
            SPL = 2,
            /// <summary>
            /// Indicates that the object of the triple is a variable
            /// </summary>
            SPV = 3
        };
        
        /// <summary>
        /// RDFDatatypes represents an enumeration for supported datatypes (RDF/RDFS/XSD).
        /// </summary>
        public enum RDFDatatypes {
            /// <summary>
            /// http://www.w3.org/2000/01/rdf-schema#Literal
            /// </summary>
            RDFS_LITERAL = 0,
            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#XMLLiteral
            /// </summary>
            RDF_XMLLITERAL = 1,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#string
            /// </summary>
            XSD_STRING = 2,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#boolean
            /// </summary>
            XSD_BOOLEAN = 3,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#decimal
            /// </summary>
            XSD_DECIMAL = 4,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#float
            /// </summary>
            XSD_FLOAT = 5,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#double
            /// </summary>
            XSD_DOUBLE = 6,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#positiveInteger
            /// </summary>
            XSD_POSITIVEINTEGER = 7,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#negativeInteger
            /// </summary>
            XSD_NEGATIVEINTEGER = 8,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#nonPositiveInteger
            /// </summary>
            XSD_NONPOSITIVEINTEGER = 9,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#nonNegativeInteger
            /// </summary>
            XSD_NONNEGATIVEINTEGER = 10,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#integer
            /// </summary>
            XSD_INTEGER = 11,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#long
            /// </summary>
            XSD_LONG = 12,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#int
            /// </summary>
            XSD_INT = 13,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#short
            /// </summary>
            XSD_SHORT = 14,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#byte
            /// </summary>
            XSD_BYTE = 15,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#unsignedLong
            /// </summary>
            XSD_UNSIGNEDLONG = 16,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#unsignedInt
            /// </summary>
            XSD_UNSIGNEDINT = 17,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#unsignedShort
            /// </summary>
            XSD_UNSIGNEDSHORT = 18,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#unsignedByte
            /// </summary>
            XSD_UNSIGNEDBYTE = 19,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#duration
            /// </summary>
            XSD_DURATION = 20,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#dateTime
            /// </summary>
            XSD_DATETIME = 21,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#date
            /// </summary>
            XSD_DATE = 22,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#time
            /// </summary>
            XSD_TIME = 23,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gYear
            /// </summary>
            XSD_GYEAR = 24,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gMonth
            /// </summary>
            XSD_GMONTH = 25,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gDay
            /// </summary>
            XSD_GDAY = 26,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gYearMonth
            /// </summary>
            XSD_GYEARMONTH = 27,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#gMonthDay
            /// </summary>
            XSD_GMONTHDAY = 28,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#hexBinary
            /// </summary>
            XSD_HEXBINARY = 29,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#base64Binary
            /// </summary>
            XSD_BASE64BINARY = 30,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#anyURI
            /// </summary>
            XSD_ANYURI = 31,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#QName
            /// </summary>
            XSD_QNAME = 32,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#NOTATION
            /// </summary>
            XSD_NOTATION = 33,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#language
            /// </summary>
            XSD_LANGUAGE = 34,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#normalizedString
            /// </summary>
            XSD_NORMALIZEDSTRING = 35,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#token
            /// </summary>
            XSD_TOKEN = 36,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#NMToken
            /// </summary>
            XSD_NMTOKEN = 37,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#Name
            /// </summary>
            XSD_NAME = 38,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#NCName
            /// </summary>
            XSD_NCNAME = 39,
            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#ID
            /// </summary>
            XSD_ID = 40
        };

        /// <summary>
        /// RDFContainerTypes represents an enumeration for supported container types.
        /// </summary>
        public enum RDFContainerTypes {
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
        public enum RDFItemTypes {
            /// <summary>
            /// Indicates that a container/collection accepts only resources
            /// </summary>
            Resource = 1,
            /// <summary>
            /// Indicates that a container/collection accepts only literals
            /// </summary>
            Literal = 2
        };

    }

}