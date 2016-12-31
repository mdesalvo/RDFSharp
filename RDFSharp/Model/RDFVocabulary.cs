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

using System;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported RDF vocabularies.
    /// </summary>
    public class RDFVocabulary {

        #region Basic

        #region RDF
        /// <summary>
        /// RDF represents the RDF vocabulary.
        /// </summary>
        public static class RDF {

            #region Properties
            /// <summary>
            /// rdf
            /// </summary>
            public static readonly String PREFIX = "rdf";

            /// <summary>
            /// http://www.w3.org/1999/02/22-rdf-syntax-ns#
            /// </summary>
            public static readonly String  BASE_URI = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

            /// <summary>
            /// rdf:Bag
            /// </summary>
            public static readonly RDFResource BAG = new RDFResource(RDF.BASE_URI + "Bag");

            /// <summary>
            /// rdf:Seq
            /// </summary>
            public static readonly RDFResource SEQ  = new RDFResource(RDF.BASE_URI + "Seq");

            /// <summary>
            /// rdf:Alt
            /// </summary>
            public static readonly RDFResource ALT = new RDFResource(RDF.BASE_URI + "Alt");

            /// <summary>
            /// rdf:Statement
            /// </summary>
            public static readonly RDFResource STATEMENT = new RDFResource(RDF.BASE_URI + "Statement");

            /// <summary>
            /// rdf:Property
            /// </summary>
            public static readonly RDFResource PROPERTY = new RDFResource(RDF.BASE_URI + "Property");

            /// <summary>
            /// rdf:XMLLiteral
            /// </summary>
            public static readonly RDFResource XML_LITERAL = new RDFResource(RDF.BASE_URI + "XMLLiteral");

            /// <summary>
            /// rdf:List
            /// </summary>
            public static readonly RDFResource LIST = new RDFResource(RDF.BASE_URI + "List");

            /// <summary>
            /// rdf:nil
            /// </summary>
            public static readonly RDFResource NIL = new RDFResource(RDF.BASE_URI + "nil");

            /// <summary>
            /// rdf:li
            /// </summary>
            public static readonly RDFResource LI = new RDFResource(RDF.BASE_URI + "li");

            /// <summary>
            /// rdf:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(RDF.BASE_URI + "subject");

            /// <summary>
            /// rdf:predicate
            /// </summary>
            public static readonly RDFResource PREDICATE = new RDFResource(RDF.BASE_URI + "predicate");

            /// <summary>
            /// rdf:object
            /// </summary>
            public static readonly RDFResource OBJECT= new RDFResource(RDF.BASE_URI + "object");

            /// <summary>
            /// rdf:type
            /// </summary>
            public static readonly RDFResource TYPE = new RDFResource(RDF.BASE_URI + "type");

            /// <summary>
            /// rdf:value
            /// </summary>
            public static readonly RDFResource VALUE = new RDFResource(RDF.BASE_URI + "value");

            /// <summary>
            /// rdf:first
            /// </summary>
            public static readonly RDFResource FIRST = new RDFResource(RDF.BASE_URI + "first");

            /// <summary>
            /// rdf:rest
            /// </summary>
            public static readonly RDFResource REST = new RDFResource(RDF.BASE_URI + "rest");
            #endregion

        }
        #endregion

        #region RDFS
        /// <summary>
        /// RDFS represents the RDFS vocabulary.
        /// </summary>
        public static class RDFS  {

            #region Properties
            /// <summary>
            /// rdfs
            /// </summary>
            public static readonly String  PREFIX = "rdfs";

            /// <summary>
            /// http://www.w3.org/2000/01/rdf-schema#
            /// </summary>
            public static readonly String  BASE_URI = "http://www.w3.org/2000/01/rdf-schema#";

            /// <summary>
            /// rdfs:Resource
            /// </summary>
            public static readonly RDFResource RESOURCE = new RDFResource(RDFS.BASE_URI + "Resource");

            /// <summary>
            /// rdfs:Class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource(RDFS.BASE_URI + "Class");

            /// <summary>
            /// rdfs:Literal
            /// </summary>
            public static readonly RDFResource LITERAL = new RDFResource(RDFS.BASE_URI + "Literal");

            /// <summary>
            /// rdfs:Container
            /// </summary>
            public static readonly RDFResource CONTAINER = new RDFResource(RDFS.BASE_URI + "Container");

            /// <summary>
            /// rdfs:Datatype
            /// </summary>
            public static readonly RDFResource DATATYPE = new RDFResource(RDFS.BASE_URI + "Datatype");

            /// <summary>
            /// rdfs:ContainerMembershipProperty
            /// </summary>
            public static readonly RDFResource CONTAINER_MEMBERSHIP_PROPERTY = new RDFResource(RDFS.BASE_URI + "ContainerMembershipProperty");

            /// <summary>
            /// rdfs:range
            /// </summary>
            public static readonly RDFResource RANGE = new RDFResource(RDFS.BASE_URI + "range");

            /// <summary>
            /// rdfs:domain
            /// </summary>
            public static readonly RDFResource DOMAIN = new RDFResource(RDFS.BASE_URI + "domain");

            /// <summary>
            /// rdfs:subClassOf
            /// </summary>
            public static readonly RDFResource SUB_CLASS_OF = new RDFResource(RDFS.BASE_URI + "subClassOf");

            /// <summary>
            /// rdfs:subPropertyOf
            /// </summary>
            public static readonly RDFResource SUB_PROPERTY_OF = new RDFResource(RDFS.BASE_URI + "subPropertyOf");

            /// <summary>
            /// rdfs:label
            /// </summary>
            public static readonly RDFResource LABEL = new RDFResource(RDFS.BASE_URI + "label");

            /// <summary>
            /// rdfs:comment
            /// </summary>
            public static readonly RDFResource COMMENT = new RDFResource(RDFS.BASE_URI + "comment");

            /// <summary>
            /// rdfs:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource(RDFS.BASE_URI + "member");

            /// <summary>
            /// rdfs:seeAlso
            /// </summary>
            public static readonly RDFResource SEE_ALSO = new RDFResource(RDFS.BASE_URI + "seeAlso");

            /// <summary>
            /// rdfs:isDefinedBy
            /// </summary>
            public static readonly RDFResource IS_DEFINED_BY = new RDFResource(RDFS.BASE_URI + "isDefinedBy");
            #endregion

        }
        #endregion

        #region XSD
        /// <summary>
        /// XSD represents the XSD vocabulary.
        /// </summary>
        public static class XSD {

            #region Properties
            /// <summary>
            /// xsd
            /// </summary>
            public static readonly String PREFIX = "xsd";

            /// <summary>
            /// http://www.w3.org/2001/XMLSchema#
            /// </summary>
            public static readonly String  BASE_URI = "http://www.w3.org/2001/XMLSchema#";

            /// <summary>
            /// xsd:string
            /// </summary>
            public static readonly RDFResource STRING = new RDFResource(XSD.BASE_URI + "string");

            /// <summary>
            /// xsd:boolean
            /// </summary>
            public static readonly RDFResource BOOLEAN = new RDFResource(XSD.BASE_URI + "boolean");

            /// <summary>
            /// xsd:decimal
            /// </summary>
            public static readonly RDFResource DECIMAL = new RDFResource(XSD.BASE_URI + "decimal");

            /// <summary>
            /// xsd:float
            /// </summary>
            public static readonly RDFResource FLOAT = new RDFResource(XSD.BASE_URI + "float");

            /// <summary>
            /// xsd:double
            /// </summary>
            public static readonly RDFResource DOUBLE = new RDFResource(XSD.BASE_URI + "double");

            /// <summary>
            /// xsd:positiveInteger
            /// </summary>
            public static readonly RDFResource POSITIVE_INTEGER = new RDFResource(XSD.BASE_URI + "positiveInteger");

            /// <summary>
            /// xsd:negativeInteger
            /// </summary>
            public static readonly RDFResource NEGATIVE_INTEGER = new RDFResource(XSD.BASE_URI + "negativeInteger");

            /// <summary>
            /// xsd:nonPositiveInteger
            /// </summary>
            public static readonly RDFResource NON_POSITIVE_INTEGER = new RDFResource(XSD.BASE_URI + "nonPositiveInteger");

            /// <summary>
            /// xsd:nonNegativeInteger
            /// </summary>
            public static readonly RDFResource NON_NEGATIVE_INTEGER = new RDFResource(XSD.BASE_URI + "nonNegativeInteger");

            /// <summary>
            /// xsd:integer
            /// </summary>
            public static readonly RDFResource INTEGER = new RDFResource(XSD.BASE_URI + "integer");

            /// <summary>
            /// xsd:long
            /// </summary>
            public static readonly RDFResource LONG = new RDFResource(XSD.BASE_URI + "long");

            /// <summary>
            /// xsd:unsignedLong
            /// </summary>
            public static readonly RDFResource UNSIGNED_LONG = new RDFResource(XSD.BASE_URI + "unsignedLong");

            /// <summary>
            /// xsd:int
            /// </summary>
            public static readonly RDFResource INT = new RDFResource(XSD.BASE_URI + "int");

            /// <summary>
            /// xsd:unsignedInt
            /// </summary>
            public static readonly RDFResource UNSIGNED_INT = new RDFResource(XSD.BASE_URI + "unsignedInt");

            /// <summary>
            /// xsd:short
            /// </summary>
            public static readonly RDFResource SHORT = new RDFResource(XSD.BASE_URI + "short");

            /// <summary>
            /// xsd:unsignedShort
            /// </summary>
            public static readonly RDFResource UNSIGNED_SHORT = new RDFResource(XSD.BASE_URI + "unsignedShort");

            /// <summary>
            /// xsd:byte
            /// </summary>
            public static readonly RDFResource BYTE = new RDFResource(XSD.BASE_URI + "byte");

            /// <summary>
            /// xsd:unsignedByte
            /// </summary>
            public static readonly RDFResource UNSIGNED_BYTE = new RDFResource(XSD.BASE_URI + "unsignedByte");

            /// <summary>
            /// xsd:duration
            /// </summary>
            public static readonly RDFResource DURATION = new RDFResource(XSD.BASE_URI + "duration");

            /// <summary>
            /// xsd:dateTime
            /// </summary>
            public static readonly RDFResource DATETIME = new RDFResource(XSD.BASE_URI + "dateTime");

            /// <summary>
            /// xsd:time
            /// </summary>
            public static readonly RDFResource TIME = new RDFResource(XSD.BASE_URI + "time");

            /// <summary>
            /// xsd:date
            /// </summary>
            public static readonly RDFResource DATE = new RDFResource(XSD.BASE_URI + "date");

            /// <summary>
            /// xsd:gYearMonth
            /// </summary>
            public static readonly RDFResource G_YEAR_MONTH = new RDFResource(XSD.BASE_URI + "gYearMonth");

            /// <summary>
            /// xsd:gYear
            /// </summary>
            public static readonly RDFResource G_YEAR = new RDFResource(XSD.BASE_URI + "gYear");

            /// <summary>
            /// xsd:gMonth
            /// </summary>
            public static readonly RDFResource G_MONTH = new RDFResource(XSD.BASE_URI + "gMonth");

            /// <summary>
            /// xsd:gMonthDay
            /// </summary>
            public static readonly RDFResource G_MONTH_DAY = new RDFResource(XSD.BASE_URI + "gMonthDay");

            /// <summary>
            /// xsd:gDay
            /// </summary>
            public static readonly RDFResource G_DAY = new RDFResource(XSD.BASE_URI + "gDay");

            /// <summary>
            /// xsd:hexBinary
            /// </summary>
            public static readonly RDFResource HEX_BINARY = new RDFResource(XSD.BASE_URI + "hexBinary");

            /// <summary>
            /// xsd:base64Binary
            /// </summary>
            public static readonly RDFResource BASE64_BINARY = new RDFResource(XSD.BASE_URI + "base64Binary");

            /// <summary>
            /// xsd:anyURI
            /// </summary>
            public static readonly RDFResource ANY_URI = new RDFResource(XSD.BASE_URI + "anyURI");

            /// <summary>
            /// xsd:QName
            /// </summary>
            public static readonly RDFResource QNAME = new RDFResource(XSD.BASE_URI + "QName");

            /// <summary>
            /// xsd:NOTATION
            /// </summary>
            public static readonly RDFResource NOTATION = new RDFResource(XSD.BASE_URI + "NOTATION");

            /// <summary>
            /// xsd:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource(XSD.BASE_URI + "language");

            /// <summary>
            /// xsd:normalizedString
            /// </summary>
            public static readonly RDFResource NORMALIZED_STRING = new RDFResource(XSD.BASE_URI + "normalizedString");

            /// <summary>
            /// xsd:token
            /// </summary>
            public static readonly RDFResource TOKEN = new RDFResource(XSD.BASE_URI + "token");

            /// <summary>
            /// xsd:NMToken
            /// </summary>
            public static readonly RDFResource NMTOKEN = new RDFResource(XSD.BASE_URI + "NMToken");

            /// <summary>
            /// xsd:Name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(XSD.BASE_URI + "Name");

            /// <summary>
            /// xsd:NCName
            /// </summary>
            public static readonly RDFResource NCNAME = new RDFResource(XSD.BASE_URI + "NCName");

            /// <summary>
            /// xsd:ID
            /// </summary>
            public static readonly RDFResource ID = new RDFResource(XSD.BASE_URI + "ID");
            #endregion

        }
        #endregion

        #region OWL
        /// <summary>
        /// OWL represents the OWL vocabulary.
        /// </summary>
        public static class OWL {

            #region Properties
            /// <summary>
            /// owl
            /// </summary>
            public static readonly String PREFIX = "owl";

            /// <summary>
            /// http://www.w3.org/2002/07/owl#
            /// </summary>
            public static readonly String  BASE_URI = "http://www.w3.org/2002/07/owl#";

            /// <summary>
            /// owl:Ontology
            /// </summary>
            public static readonly RDFResource ONTOLOGY = new RDFResource(OWL.BASE_URI + "Ontology");

            /// <summary>
            /// owl:imports
            /// </summary>
            public static readonly RDFResource IMPORTS = new RDFResource(OWL.BASE_URI + "imports");

            /// <summary>
            /// owl:Class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource(OWL.BASE_URI + "Class");

            /// <summary>
            /// owl:Individual
            /// </summary>
            public static readonly RDFResource INDIVIDUAL = new RDFResource(OWL.BASE_URI + "Individual");

            /// <summary>
            /// owl:Thing
            /// </summary>
            public static readonly RDFResource THING = new RDFResource(OWL.BASE_URI + "Thing");

            /// <summary>
            /// owl:Nothing
            /// </summary>
            public static readonly RDFResource NOTHING = new RDFResource(OWL.BASE_URI + "Nothing");

            /// <summary>
            /// owl:Restriction
            /// </summary>
            public static readonly RDFResource RESTRICTION = new RDFResource(OWL.BASE_URI + "Restriction");

            /// <summary>
            /// owl:onProperty
            /// </summary>
            public static readonly RDFResource ON_PROPERTY = new RDFResource(OWL.BASE_URI + "onProperty");

            /// <summary>
            /// owl:equivalentClass
            /// </summary>
            public static readonly RDFResource EQUIVALENT_CLASS = new RDFResource(OWL.BASE_URI + "equivalentClass");

            /// <summary>
            /// owl:DeprecatedClass
            /// </summary>
            public static readonly RDFResource DEPRECATED_CLASS = new RDFResource(OWL.BASE_URI + "DeprecatedClass");

            /// <summary>
            /// owl:equivalentProperty
            /// </summary>
            public static readonly RDFResource EQUIVALENT_PROPERTY = new RDFResource(OWL.BASE_URI + "equivalentProperty");

            /// <summary>
            /// owl:DeprecatedProperty
            /// </summary>
            public static readonly RDFResource DEPRECATED_PROPERTY = new RDFResource(OWL.BASE_URI + "DeprecatedProperty");

            /// <summary>
            /// owl:inverseOf
            /// </summary>
            public static readonly RDFResource INVERSE_OF = new RDFResource(OWL.BASE_URI + "inverseOf");

            /// <summary>
            /// owl:DatatypeProperty
            /// </summary>
            public static readonly RDFResource DATATYPE_PROPERTY = new RDFResource(OWL.BASE_URI + "DatatypeProperty");

            /// <summary>
            /// owl:ObjectProperty
            /// </summary>
            public static readonly RDFResource OBJECT_PROPERTY = new RDFResource(OWL.BASE_URI + "ObjectProperty");

            /// <summary>
            /// owl:TransitiveProperty
            /// </summary>
            public static readonly RDFResource TRANSITIVE_PROPERTY = new RDFResource(OWL.BASE_URI + "TransitiveProperty");

            /// <summary>
            /// owl:SymmetricProperty
            /// </summary>
            public static readonly RDFResource SYMMETRIC_PROPERTY = new RDFResource(OWL.BASE_URI + "SymmetricProperty");

            /// <summary>
            /// owl:FunctionalProperty
            /// </summary>
            public static readonly RDFResource FUNCTIONAL_PROPERTY = new RDFResource(OWL.BASE_URI + "FunctionalProperty");

            /// <summary>
            /// owl:InverseFunctionalProperty
            /// </summary>
            public static readonly RDFResource INVERSE_FUNCTIONAL_PROPERTY = new RDFResource(OWL.BASE_URI + "InverseFunctionalProperty");

            /// <summary>
            /// owl:AnnotationProperty
            /// </summary>
            public static readonly RDFResource ANNOTATION_PROPERTY = new RDFResource(OWL.BASE_URI + "AnnotationProperty");

            /// <summary>
            /// owl:OntologyProperty
            /// </summary>
            public static readonly RDFResource ONTOLOGY_PROPERTY = new RDFResource(OWL.BASE_URI + "OntologyProperty");

            /// <summary>
            /// owl:allValuesFrom
            /// </summary>
            public static readonly RDFResource ALL_VALUES_FROM = new RDFResource(OWL.BASE_URI + "allValuesFrom");

            /// <summary>
            /// owl:someValuesFrom
            /// </summary>
            public static readonly RDFResource SOME_VALUES_FROM = new RDFResource(OWL.BASE_URI + "someValuesFrom");

            /// <summary>
            /// owl:hasValue
            /// </summary>
            public static readonly RDFResource HAS_VALUE = new RDFResource(OWL.BASE_URI + "hasValue");

            /// <summary>
            /// owl:minCardinality
            /// </summary>
            public static readonly RDFResource MIN_CARDINALITY = new RDFResource(OWL.BASE_URI + "minCardinality");

            /// <summary>
            /// owl:maxCardinality
            /// </summary>
            public static readonly RDFResource MAX_CARDINALITY = new RDFResource(OWL.BASE_URI + "maxCardinality");

            /// <summary>
            /// owl:cardinality
            /// </summary>
            public static readonly RDFResource CARDINALITY = new RDFResource(OWL.BASE_URI + "cardinality");

            /// <summary>
            /// owl:sameAs
            /// </summary>
            public static readonly RDFResource SAME_AS = new RDFResource(OWL.BASE_URI + "sameAs");

            /// <summary>
            /// owl:differentFrom
            /// </summary>
            public static readonly RDFResource DIFFERENT_FROM = new RDFResource(OWL.BASE_URI + "differentFrom");

            /// <summary>
            /// owl:intersectionOf
            /// </summary>
            public static readonly RDFResource INTERSECTION_OF = new RDFResource(OWL.BASE_URI + "intersectionOf");

            /// <summary>
            /// owl:unionOf
            /// </summary>
            public static readonly RDFResource UNION_OF = new RDFResource(OWL.BASE_URI + "unionOf");
			
			/// <summary>
            /// owl:complementOf
            /// </summary>
            public static readonly RDFResource COMPLEMENT_OF = new RDFResource(OWL.BASE_URI + "complementOf");

            /// <summary>
            /// owl:oneOf
            /// </summary>
            public static readonly RDFResource ONE_OF = new RDFResource(OWL.BASE_URI + "oneOf");

            /// <summary>
            /// owl:DataRange
            /// </summary>
            public static readonly RDFResource DATA_RANGE = new RDFResource(OWL.BASE_URI + "DataRange");

            /// <summary>
            /// owl:backwardCompatibleWith
            /// </summary>
            public static readonly RDFResource BACKWARD_COMPATIBLE_WITH = new RDFResource(OWL.BASE_URI + "backwardCompatibleWith");

            /// <summary>
            /// owl:incompatibleWith
            /// </summary>
            public static readonly RDFResource INCOMPATIBLE_WITH = new RDFResource(OWL.BASE_URI + "incompatibleWith");

            /// <summary>
            /// owl:disjointWith
            /// </summary>
            public static readonly RDFResource DISJOINT_WITH = new RDFResource(OWL.BASE_URI + "disjointWith");

            /// <summary>
            /// owl:priorVersion
            /// </summary>
            public static readonly RDFResource PRIOR_VERSION = new RDFResource(OWL.BASE_URI + "priorVersion");

            /// <summary>
            /// owl:versionInfo
            /// </summary>
            public static readonly RDFResource VERSION_INFO = new RDFResource(OWL.BASE_URI + "versionInfo");
			
			/// <summary>
            /// owl:versionIRI
            /// </summary>
            public static readonly RDFResource VERSION_IRI = new RDFResource(OWL.BASE_URI + "versionIRI");
            #endregion

        }
        #endregion

        #region XML
        /// <summary>
        /// XML represents the XML vocabulary.
        /// </summary>
        public static class XML {

            #region Properties
            /// <summary>
            /// xml
            /// </summary>
            public static readonly String PREFIX = "xml";

            /// <summary>
            /// http://www.w3.org/XML/1998/namespace#
            /// </summary>
            public static readonly String BASE_URI = "http://www.w3.org/XML/1998/namespace#";

            /// <summary>
            /// xml:lang
            /// </summary>
            public static readonly RDFResource LANG = new RDFResource(XML.BASE_URI + "lang");

            /// <summary>
            /// xml:base
            /// </summary>
            public static readonly RDFResource BASE = new RDFResource(XML.BASE_URI + "base");
            #endregion

        }
        #endregion

        #endregion

        #region Extended

        #region DC
        /// <summary>
        /// DC represents the Dublin Core vocabulary (with DCAM, DCTERMS and DCTYPE extensions).
        /// </summary>
        public static class DC {

            #region Properties
            /// <summary>
            /// dc
            /// </summary>
            public static readonly String PREFIX = "dc";

            /// <summary>
            /// http://purl.org/dc/elements/1.1/
            /// </summary>
            public static readonly String BASE_URI = "http://purl.org/dc/elements/1.1/";

            /// <summary>
            /// dc:contributor
            /// </summary>
            public static readonly RDFResource CONTRIBUTOR = new RDFResource(DC.BASE_URI + "contributor");

            /// <summary>
            /// dc:coverage
            /// </summary>
            public static readonly RDFResource COVERAGE = new RDFResource(DC.BASE_URI + "coverage");

            /// <summary>
            /// dc:creator
            /// </summary>
            public static readonly RDFResource CREATOR = new RDFResource(DC.BASE_URI + "creator");

            /// <summary>
            /// dc:date
            /// </summary>
            public static readonly RDFResource DATE = new RDFResource(DC.BASE_URI + "date");

            /// <summary>
            /// dc:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(DC.BASE_URI + "description");

            /// <summary>
            /// dc:format
            /// </summary>
            public static readonly RDFResource FORMAT = new RDFResource(DC.BASE_URI + "format");

            /// <summary>
            /// dc:identifier
            /// </summary>
            public static readonly RDFResource IDENTIFIER = new RDFResource(DC.BASE_URI + "identifier");

            /// <summary>
            /// dc:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource(DC.BASE_URI + "language");

            /// <summary>
            /// dc:publisher
            /// </summary>
            public static readonly RDFResource PUBLISHER = new RDFResource(DC.BASE_URI + "publisher");

            /// <summary>
            /// dc:relation
            /// </summary>
            public static readonly RDFResource RELATION = new RDFResource(DC.BASE_URI + "relation");

            /// <summary>
            /// dc:rights
            /// </summary>
            public static readonly RDFResource RIGHTS = new RDFResource(DC.BASE_URI + "rights");

            /// <summary>
            /// dc:source
            /// </summary>
            public static readonly RDFResource SOURCE = new RDFResource(DC.BASE_URI + "source");

            /// <summary>
            /// dc:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(DC.BASE_URI + "subject");

            /// <summary>
            /// dc:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource(DC.BASE_URI + "title");

            /// <summary>
            /// dc:type
            /// </summary>
            public static readonly RDFResource TYPE = new RDFResource(DC.BASE_URI + "type");
            #endregion

            #region Extended Properties

            #region DCAM
            public static class DCAM {

                #region Properties
                /// <summary>
                /// dcam
                /// </summary>
                public static readonly String PREFIX = "dcam";

                /// <summary>
                /// http://purl.org/dc/dcam/
                /// </summary>
                public static readonly String BASE_URI = "http://purl.org/dc/dcam/";

                /// <summary>
                /// dcam:memberOf
                /// </summary>
                public static readonly RDFResource MEMBER_OF = new RDFResource(DCAM.BASE_URI + "memberOf");

                /// <summary>
                /// dcam:VocabularyEncodingScheme
                /// </summary>
                public static readonly RDFResource VOCABULARY_ENCODING_SCHEME = new RDFResource(DCAM.BASE_URI + "VocabularyEncodingScheme");
                #endregion

            }
            #endregion

            #region DCTERMS
            public static class DCTERMS {

                #region Properties
                /// <summary>
                /// dcterms
                /// </summary>
                public static readonly String PREFIX = "dcterms";

                /// <summary>
                /// http://purl.org/dc/terms/
                /// </summary>
                public static readonly String BASE_URI = "http://purl.org/dc/terms/";

                /// <summary>
                /// dcterms:abstract
                /// </summary>
                public static readonly RDFResource ABSTRACT = new RDFResource(DCTERMS.BASE_URI + "abstract");

                /// <summary>
                /// dcterms:accessRights
                /// </summary>
                public static readonly RDFResource ACCESS_RIGHTS = new RDFResource(DCTERMS.BASE_URI + "accessRights");

                /// <summary>
                /// dcterms:accrualMethod
                /// </summary>
                public static readonly RDFResource ACCRUAL_METHOD = new RDFResource(DCTERMS.BASE_URI + "accrualMethod");

                /// <summary>
                /// dcterms:accrualPeriodicity
                /// </summary>
                public static readonly RDFResource ACCRUAL_PERIODICITY = new RDFResource(DCTERMS.BASE_URI + "accrualPeriodicity");

                /// <summary>
                /// dcterms:accrualPolicy
                /// </summary>
                public static readonly RDFResource ACCRUAL_POLICY = new RDFResource(DCTERMS.BASE_URI + "accrualPolicy");

                /// <summary>
                /// dcterms:Agent
                /// </summary>
                public static readonly RDFResource AGENT = new RDFResource(DCTERMS.BASE_URI + "Agent");

                /// <summary>
                /// dcterms:AgentClass
                /// </summary>
                public static readonly RDFResource AGENT_CLASS = new RDFResource(DCTERMS.BASE_URI + "AgentClass");

                /// <summary>
                /// dcterms:alternative
                /// </summary>
                public static readonly RDFResource ALTERNATIVE = new RDFResource(DCTERMS.BASE_URI + "alternative");

                /// <summary>
                /// dcterms:audience
                /// </summary>
                public static readonly RDFResource AUDIENCE = new RDFResource(DCTERMS.BASE_URI + "audience");

                /// <summary>
                /// dcterms:available
                /// </summary>
                public static readonly RDFResource AVAILABLE = new RDFResource(DCTERMS.BASE_URI + "available");

                /// <summary>
                /// dcterms:bibliographicCitation
                /// </summary>
                public static readonly RDFResource BIBLIOGRAPHIC_CITATION = new RDFResource(DCTERMS.BASE_URI + "bibliographicCitation");

                /// <summary>
                /// dcterms:BibliographicResource
                /// </summary>
                public static readonly RDFResource BIBLIOGRAPHIC_RESOURCE = new RDFResource(DCTERMS.BASE_URI + "BibliographicResource");

                /// <summary>
                /// dcterms:conformsTo
                /// </summary>
                public static readonly RDFResource CONFORMS_TO = new RDFResource(DCTERMS.BASE_URI + "conformsTo");

                /// <summary>
                /// dcterms:contributor
                /// </summary>
                public static readonly RDFResource CONTRIBUTOR = new RDFResource(DCTERMS.BASE_URI + "contributor");

                /// <summary>
                /// dcterms:coverage
                /// </summary>
                public static readonly RDFResource COVERAGE = new RDFResource(DCTERMS.BASE_URI + "coverage");

                /// <summary>
                /// dcterms:created
                /// </summary>
                public static readonly RDFResource CREATED = new RDFResource(DCTERMS.BASE_URI + "created");

                /// <summary>
                /// dcterms:creator
                /// </summary>
                public static readonly RDFResource CREATOR = new RDFResource(DCTERMS.BASE_URI + "creator");

                /// <summary>
                /// dcterms:date
                /// </summary>
                public static readonly RDFResource DATE = new RDFResource(DCTERMS.BASE_URI + "date");

                /// <summary>
                /// dcterms:dateAccepted
                /// </summary>
                public static readonly RDFResource DATE_ACCEPTED = new RDFResource(DCTERMS.BASE_URI + "dateAccepted");

                /// <summary>
                /// dcterms:dateCopyrighted
                /// </summary>
                public static readonly RDFResource DATE_COPYRIGHTED = new RDFResource(DCTERMS.BASE_URI + "dateCopyrighted");

                /// <summary>
                /// dcterms:dateSubmitted
                /// </summary>
                public static readonly RDFResource DATE_SUBMITTED = new RDFResource(DCTERMS.BASE_URI + "dateSubmitted");

                /// <summary>
                /// dcterms:description
                /// </summary>
                public static readonly RDFResource DESCRIPTION = new RDFResource(DCTERMS.BASE_URI + "description");

                /// <summary>
                /// dcterms:educationLevel
                /// </summary>
                public static readonly RDFResource EDUCATION_LEVEL = new RDFResource(DCTERMS.BASE_URI + "educationLevel");

                /// <summary>
                /// dcterms:extent
                /// </summary>
                public static readonly RDFResource EXTENT = new RDFResource(DCTERMS.BASE_URI + "extent");

                /// <summary>
                /// dcterms:FileFormat
                /// </summary>
                public static readonly RDFResource FILE_FORMAT = new RDFResource(DCTERMS.BASE_URI + "FileFormat");

                /// <summary>
                /// dcterms:format
                /// </summary>
                public static readonly RDFResource FORMAT = new RDFResource(DCTERMS.BASE_URI + "format");

                /// <summary>
                /// dcterms:Frequency
                /// </summary>
                public static readonly RDFResource FREQUENCY = new RDFResource(DCTERMS.BASE_URI + "Frequency");

                /// <summary>
                /// dcterms:hasFormat
                /// </summary>
                public static readonly RDFResource HAS_FORMAT = new RDFResource(DCTERMS.BASE_URI + "hasFormat");

                /// <summary>
                /// dcterms:hasPart
                /// </summary>
                public static readonly RDFResource HAS_PART = new RDFResource(DCTERMS.BASE_URI + "hasPart");

                /// <summary>
                /// dcterms:hasVersion
                /// </summary>
                public static readonly RDFResource HAS_VERSION = new RDFResource(DCTERMS.BASE_URI + "hasVersion");

                /// <summary>
                /// dcterms:identifier
                /// </summary>
                public static readonly RDFResource IDENTIFIER = new RDFResource(DCTERMS.BASE_URI + "identifier");

                /// <summary>
                /// dcterms:instructionalMethod
                /// </summary>
                public static readonly RDFResource INSTRUCTIONAL_METHOD = new RDFResource(DCTERMS.BASE_URI + "instructionalMethod");

                /// <summary>
                /// dcterms:isFormatOf
                /// </summary>
                public static readonly RDFResource IS_FORMAT_OF = new RDFResource(DCTERMS.BASE_URI + "isFormatOf");

                /// <summary>
                /// dcterms:isPartOf
                /// </summary>
                public static readonly RDFResource IS_PART_OF = new RDFResource(DCTERMS.BASE_URI + "isPartOf");

                /// <summary>
                /// dcterms:isReferencedBy
                /// </summary>
                public static readonly RDFResource IS_REFERENCED_BY = new RDFResource(DCTERMS.BASE_URI + "isReferencedBy");

                /// <summary>
                /// dcterms:isReplacedBy
                /// </summary>
                public static readonly RDFResource IS_REPLACED_BY = new RDFResource(DCTERMS.BASE_URI + "isReplacedBy");

                /// <summary>
                /// dcterms:isRequiredBy
                /// </summary>
                public static readonly RDFResource IS_REQUIRED_BY = new RDFResource(DCTERMS.BASE_URI + "isRequiredBy");

                /// <summary>
                /// dcterms:issued
                /// </summary>
                public static readonly RDFResource ISSUED = new RDFResource(DCTERMS.BASE_URI + "issued");

                /// <summary>
                /// dcterms:isVersionOf
                /// </summary>
                public static readonly RDFResource IS_VERSION_OF = new RDFResource(DCTERMS.BASE_URI + "isVersionOf");

                /// <summary>
                /// dcterms:Jurisdiction
                /// </summary>
                public static readonly RDFResource JURISDICTION = new RDFResource(DCTERMS.BASE_URI + "Jurisdiction");

                /// <summary>
                /// dcterms:language
                /// </summary>
                public static readonly RDFResource LANGUAGE = new RDFResource(DCTERMS.BASE_URI + "language");

                /// <summary>
                /// dcterms:license
                /// </summary>
                public static readonly RDFResource LICENSE = new RDFResource(DCTERMS.BASE_URI + "license");

                /// <summary>
                /// dcterms:LicenseDocument
                /// </summary>
                public static readonly RDFResource LICENSE_DOCUMENT = new RDFResource(DCTERMS.BASE_URI + "LicenseDocument");

                /// <summary>
                /// dcterms:LinguisticSystem
                /// </summary>
                public static readonly RDFResource LINGUISTIC_SYSTEM = new RDFResource(DCTERMS.BASE_URI + "LinguisticSystem");

                /// <summary>
                /// dcterms:Location
                /// </summary>
                public static readonly RDFResource LOCATION = new RDFResource(DCTERMS.BASE_URI + "Location");

                /// <summary>
                /// dcterms:LocationPeriodOrJurisdiction
                /// </summary>
                public static readonly RDFResource LOCATION_PERIOD_OR_JURISDICTION = new RDFResource(DCTERMS.BASE_URI + "LocationPeriodOrJurisdiction");

                /// <summary>
                /// dcterms:mediator
                /// </summary>
                public static readonly RDFResource MEDIATOR = new RDFResource(DCTERMS.BASE_URI + "mediator");

                /// <summary>
                /// dcterms:MediaType
                /// </summary>
                public static readonly RDFResource MEDIA_TYPE = new RDFResource(DCTERMS.BASE_URI + "MediaType");

                /// <summary>
                /// dcterms:MediaTypeOrExtent
                /// </summary>
                public static readonly RDFResource MEDIA_TYPE_OR_EXTENT = new RDFResource(DCTERMS.BASE_URI + "MediaTypeOrExtent");

                /// <summary>
                /// dcterms:medium
                /// </summary>
                public static readonly RDFResource MEDIUM = new RDFResource(DCTERMS.BASE_URI + "medium");

                /// <summary>
                /// dcterms:MethodOfAccrual
                /// </summary>
                public static readonly RDFResource METHOD_OF_ACCRUAL = new RDFResource(DCTERMS.BASE_URI + "MethodOfAccrual");

                /// <summary>
                /// dcterms:MethodOfInstruction
                /// </summary>
                public static readonly RDFResource METHOD_OF_INSTRUCTION = new RDFResource(DCTERMS.BASE_URI + "MethodOfInstruction");

                /// <summary>
                /// dcterms:modified
                /// </summary>
                public static readonly RDFResource MODIFIED = new RDFResource(DCTERMS.BASE_URI + "modified");

                /// <summary>
                /// dcterms:PeriodOfTime
                /// </summary>
                public static readonly RDFResource PERIOD_OF_TIME = new RDFResource(DCTERMS.BASE_URI + "PeriodOfTime");

                /// <summary>
                /// dcterms:PhysicalMedium
                /// </summary>
                public static readonly RDFResource PHYSICAL_MEDIUM = new RDFResource(DCTERMS.BASE_URI + "PhysicalMedium");

                /// <summary>
                /// dcterms:PhysicalResource
                /// </summary>
                public static readonly RDFResource PHYSICAL_RESOURCE = new RDFResource(DCTERMS.BASE_URI + "PhysicalResource");

                /// <summary>
                /// dcterms:Policy
                /// </summary>
                public static readonly RDFResource POLICY = new RDFResource(DCTERMS.BASE_URI + "Policy");

                /// <summary>
                /// dcterms:provenance
                /// </summary>
                public static readonly RDFResource PROVENANCE = new RDFResource(DCTERMS.BASE_URI + "provenance");

                /// <summary>
                /// dcterms:ProvenanceStatement
                /// </summary>
                public static readonly RDFResource PROVENANCE_STATEMENT = new RDFResource(DCTERMS.BASE_URI + "ProvenanceStatement");

                /// <summary>
                /// dcterms:publisher
                /// </summary>
                public static readonly RDFResource PUBLISHER = new RDFResource(DCTERMS.BASE_URI + "publisher");

                /// <summary>
                /// dcterms:references
                /// </summary>
                public static readonly RDFResource REFERENCES = new RDFResource(DCTERMS.BASE_URI + "references");

                /// <summary>
                /// dcterms:relation
                /// </summary>
                public static readonly RDFResource RELATION = new RDFResource(DCTERMS.BASE_URI + "relation");

                /// <summary>
                /// dcterms:replaces
                /// </summary>
                public static readonly RDFResource REPLACES = new RDFResource(DCTERMS.BASE_URI + "replaces");

                /// <summary>
                /// dcterms:requires
                /// </summary>
                public static readonly RDFResource REQUIRES = new RDFResource(DCTERMS.BASE_URI + "requires");

                /// <summary>
                /// dcterms:rights
                /// </summary>
                public static readonly RDFResource RIGHTS = new RDFResource(DCTERMS.BASE_URI + "rights");

                /// <summary>
                /// dcterms:RightsStatement
                /// </summary>
                public static readonly RDFResource RIGHTS_STATEMENT = new RDFResource(DCTERMS.BASE_URI + "RightsStatement");

                /// <summary>
                /// dcterms:rightsHolder
                /// </summary>
                public static readonly RDFResource RIGHTS_HOLDER = new RDFResource(DCTERMS.BASE_URI + "rightsHolder");

                /// <summary>
                /// dcterms:SizeOrDuration
                /// </summary>
                public static readonly RDFResource SIZE_OR_DURATION = new RDFResource(DCTERMS.BASE_URI + "SizeOrDuration");

                /// <summary>
                /// dcterms:source
                /// </summary>
                public static readonly RDFResource SOURCE = new RDFResource(DCTERMS.BASE_URI + "source");

                /// <summary>
                /// dcterms:spatial
                /// </summary>
                public static readonly RDFResource SPATIAL = new RDFResource(DCTERMS.BASE_URI + "spatial");

                /// <summary>
                /// dcterms:Standard
                /// </summary>
                public static readonly RDFResource STANDARD = new RDFResource(DCTERMS.BASE_URI + "Standard");

                /// <summary>
                /// dcterms:subject
                /// </summary>
                public static readonly RDFResource SUBJECT = new RDFResource(DCTERMS.BASE_URI + "subject");

                /// <summary>
                /// dcterms:tableOfContents
                /// </summary>
                public static readonly RDFResource TABLE_OF_CONTENTS = new RDFResource(DCTERMS.BASE_URI + "tableOfContents");

                /// <summary>
                /// dcterms:temporal
                /// </summary>
                public static readonly RDFResource TEMPORAL = new RDFResource(DCTERMS.BASE_URI + "temporal");

                /// <summary>
                /// dcterms:title
                /// </summary>
                public static readonly RDFResource TITLE = new RDFResource(DCTERMS.BASE_URI + "title");

                /// <summary>
                /// dcterms:type
                /// </summary>
                public static readonly RDFResource TYPE = new RDFResource(DCTERMS.BASE_URI + "type");

                /// <summary>
                /// dcterms:valid
                /// </summary>
                public static readonly RDFResource VALID = new RDFResource(DCTERMS.BASE_URI + "valid");

                #region Vocabulary Encoding Schemes
                /// <summary>
                /// dcterms:DCMIType
                /// </summary>
                public static readonly RDFResource DCMITYPE = new RDFResource(DCTERMS.BASE_URI + "DCMIType");

                /// <summary>
                /// dcterms:DDC
                /// </summary>
                public static readonly RDFResource DDC = new RDFResource(DCTERMS.BASE_URI + "DDC");

                /// <summary>
                /// dcterms:IMT
                /// </summary>
                public static readonly RDFResource IMT = new RDFResource(DCTERMS.BASE_URI + "IMT");

                /// <summary>
                /// dcterms:LCC
                /// </summary>
                public static readonly RDFResource LCC = new RDFResource(DCTERMS.BASE_URI + "LCC");

                /// <summary>
                /// dcterms:LCSH
                /// </summary>
                public static readonly RDFResource LCSH = new RDFResource(DCTERMS.BASE_URI + "LCSH");

                /// <summary>
                /// dcterms:MESH
                /// </summary>
                public static readonly RDFResource MESH = new RDFResource(DCTERMS.BASE_URI + "MESH");

                /// <summary>
                /// dcterms:NLM
                /// </summary>
                public static readonly RDFResource NLM = new RDFResource(DCTERMS.BASE_URI + "NLM");

                /// <summary>
                /// dcterms:TGN
                /// </summary>
                public static readonly RDFResource TGN = new RDFResource(DCTERMS.BASE_URI + "TGN");

                /// <summary>
                /// dcterms:UDC
                /// </summary>
                public static readonly RDFResource UDC = new RDFResource(DCTERMS.BASE_URI + "UDC");
                #endregion

                #region Syntax Encoding Schemes
                /// <summary>
                /// dcterms:Box
                /// </summary>
                public static readonly RDFResource BOX = new RDFResource(DCTERMS.BASE_URI + "Box");

                /// <summary>
                /// dcterms:ISO3166
                /// </summary>
                public static readonly RDFResource ISO3166 = new RDFResource(DCTERMS.BASE_URI + "ISO3166");

                /// <summary>
                /// dcterms:ISO639-2
                /// </summary>
                public static readonly RDFResource ISO639_2 = new RDFResource(DCTERMS.BASE_URI + "ISO639-2");

                /// <summary>
                /// dcterms:ISO639-3
                /// </summary>
                public static readonly RDFResource ISO639_3 = new RDFResource(DCTERMS.BASE_URI + "ISO639-3");

                /// <summary>
                /// dcterms:Period
                /// </summary>
                public static readonly RDFResource PERIOD = new RDFResource(DCTERMS.BASE_URI + "Period");

                /// <summary>
                /// dcterms:Point
                /// </summary>
                public static readonly RDFResource POINT = new RDFResource(DCTERMS.BASE_URI + "Point");

                /// <summary>
                /// dcterms:RFC1766
                /// </summary>
                public static readonly RDFResource RFC1766 = new RDFResource(DCTERMS.BASE_URI + "RFC1766");

                /// <summary>
                /// dcterms:RFC3066
                /// </summary>
                public static readonly RDFResource RFC3066 = new RDFResource(DCTERMS.BASE_URI + "RFC3066");

                /// <summary>
                /// dcterms:RFC4646
                /// </summary>
                public static readonly RDFResource RFC4646 = new RDFResource(DCTERMS.BASE_URI + "RFC4646");

                /// <summary>
                /// dcterms:RFC5646
                /// </summary>
                public static readonly RDFResource RFC5646 = new RDFResource(DCTERMS.BASE_URI + "RFC5646");

                /// <summary>
                /// dcterms:URI
                /// </summary>
                public static readonly RDFResource URI = new RDFResource(DCTERMS.BASE_URI + "URI");

                /// <summary>
                /// dcterms:W3CDTF
                /// </summary>
                public static readonly RDFResource W3CDTF = new RDFResource(DCTERMS.BASE_URI + "W3CDTF");
                #endregion

                #endregion

            }
            #endregion

            #region DCTYPE
            public static class DCTYPE {

                #region Properties
                /// <summary>
                /// dctype
                /// </summary>
                public static readonly String PREFIX = "dctype";

                /// <summary>
                /// http://purl.org/dc/dcmitype/
                /// </summary>
                public static readonly String BASE_URI = "http://purl.org/dc/dcmitype/";

                /// <summary>
                /// dctype:Collection
                /// </summary>
                public static readonly RDFResource COLLECTION = new RDFResource(DCTYPE.BASE_URI + "Collection");

                /// <summary>
                /// dctype:Dataset
                /// </summary>
                public static readonly RDFResource DATASET = new RDFResource(DCTYPE.BASE_URI + "Dataset");

                /// <summary>
                /// dctype:Event
                /// </summary>
                public static readonly RDFResource EVENT = new RDFResource(DCTYPE.BASE_URI + "Event");

                /// <summary>
                /// dctype:Image
                /// </summary>
                public static readonly RDFResource IMAGE = new RDFResource(DCTYPE.BASE_URI + "Image");

                /// <summary>
                /// dctype:InteractiveResource
                /// </summary>
                public static readonly RDFResource INTERACTIVE_RESOURCE = new RDFResource(DCTYPE.BASE_URI + "InteractiveResource");

                /// <summary>
                /// dctype:MovingImage
                /// </summary>
                public static readonly RDFResource MOVING_IMAGE = new RDFResource(DCTYPE.BASE_URI + "MovingImage");

                /// <summary>
                /// dctype:PhysicalObject
                /// </summary>
                public static readonly RDFResource PHYSICAL_OBJECT = new RDFResource(DCTYPE.BASE_URI + "PhysicalObject");

                /// <summary>
                /// dctype:Service
                /// </summary>
                public static readonly RDFResource SERVICE = new RDFResource(DCTYPE.BASE_URI + "Service");

                /// <summary>
                /// dctype:Software
                /// </summary>
                public static readonly RDFResource SOFTWARE = new RDFResource(DCTYPE.BASE_URI + "Software");

                /// <summary>
                /// dctype:Sound
                /// </summary>
                public static readonly RDFResource SOUND = new RDFResource(DCTYPE.BASE_URI + "Sound");

                /// <summary>
                /// dctype:StillImage
                /// </summary>
                public static readonly RDFResource STILL_IMAGE = new RDFResource(DCTYPE.BASE_URI + "StillImage");

                /// <summary>
                /// dctype:Text
                /// </summary>
                public static readonly RDFResource TEXT = new RDFResource(DCTYPE.BASE_URI + "Text");
                #endregion

            }
            #endregion

            #endregion

        }
        #endregion

        #region FOAF
        /// <summary>
        /// FOAF represents the Friend-of-a-Friend vocabulary.
        /// </summary>
        public static class FOAF {

            #region Properties
            /// <summary>
            /// foaf
            /// </summary>
            public static readonly String PREFIX = "foaf";

            /// <summary>
            /// http://xmlns.com/foaf/0.1/
            /// </summary>
            public static readonly String BASE_URI = "http://xmlns.com/foaf/0.1/";

            /// <summary>
            /// foaf:Agent
            /// </summary>
            public static readonly RDFResource AGENT = new RDFResource(FOAF.BASE_URI + "Agent");

            /// <summary>
            /// foaf:Person
            /// </summary>
            public static readonly RDFResource PERSON = new RDFResource(FOAF.BASE_URI + "Person");

            /// <summary>
            /// foaf:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(FOAF.BASE_URI + "name");

            /// <summary>
            /// foaf:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource(FOAF.BASE_URI + "title");

            /// <summary>
            /// foaf:img
            /// </summary>
            public static readonly RDFResource IMG = new RDFResource(FOAF.BASE_URI + "img");

            /// <summary>
            /// foaf:depiction
            /// </summary>
            public static readonly RDFResource DEPICTION = new RDFResource(FOAF.BASE_URI + "depiction");

            /// <summary>
            /// foaf:depicts
            /// </summary>
            public static readonly RDFResource DEPICTS = new RDFResource(FOAF.BASE_URI + "depicts");

            /// <summary>
            /// foaf:familyName
            /// </summary>
            public static readonly RDFResource FAMILY_NAME = new RDFResource(FOAF.BASE_URI + "familyName");

            /// <summary>
            /// foaf:givenName
            /// </summary>
            public static readonly RDFResource GIVEN_NAME = new RDFResource(FOAF.BASE_URI + "givenName");

            /// <summary>
            /// foaf:knows
            /// </summary>
            public static readonly RDFResource KNOWS = new RDFResource(FOAF.BASE_URI + "knows");

            /// <summary>
            /// foaf:skypeID
            /// </summary>
            public static readonly RDFResource SKYPE_ID = new RDFResource(FOAF.BASE_URI + "skypeID");

            /// <summary>
            /// foaf:based_near
            /// </summary>
            public static readonly RDFResource BASED_NEAR = new RDFResource(FOAF.BASE_URI + "based_near");

            /// <summary>
            /// foaf:age
            /// </summary>
            public static readonly RDFResource AGE = new RDFResource(FOAF.BASE_URI + "age");

            /// <summary>
            /// foaf:made
            /// </summary>
            public static readonly RDFResource MADE = new RDFResource(FOAF.BASE_URI + "made");

            /// <summary>
            /// foaf:maker
            /// </summary>
            public static readonly RDFResource MAKER = new RDFResource(FOAF.BASE_URI + "maker");

            /// <summary>
            /// foaf:primaryTopic
            /// </summary>
            public static readonly RDFResource PRIMARY_TOPIC = new RDFResource(FOAF.BASE_URI + "primaryTopic");

            /// <summary>
            /// foaf:isPrimaryTopicOf
            /// </summary>
            public static readonly RDFResource IS_PRIMARY_TOPIC_OF = new RDFResource(FOAF.BASE_URI + "isPrimaryTopicOf");

            /// <summary>
            /// foaf:Project
            /// </summary>
            public static readonly RDFResource PROJECT = new RDFResource(FOAF.BASE_URI + "Project");

            /// <summary>
            /// foaf:Organization
            /// </summary>
            public static readonly RDFResource ORGANIZATION = new RDFResource(FOAF.BASE_URI + "Organization");

            /// <summary>
            /// foaf:Group
            /// </summary>
            public static readonly RDFResource GROUP = new RDFResource(FOAF.BASE_URI + "Group");

            /// <summary>
            /// foaf:Document
            /// </summary>
            public static readonly RDFResource DOCUMENT = new RDFResource(FOAF.BASE_URI + "Document");

            /// <summary>
            /// foaf:Image
            /// </summary>
            public static readonly RDFResource IMAGE = new RDFResource(FOAF.BASE_URI + "Image");

            /// <summary>
            /// foaf:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource(FOAF.BASE_URI + "member");
			
			/// <summary>
            /// foaf:focus
            /// </summary>
            public static readonly RDFResource FOCUS = new RDFResource(FOAF.BASE_URI + "focus");

            /// <summary>
            /// foaf:fundedBy
            /// </summary>
            public static readonly RDFResource FUNDED_BY = new RDFResource(FOAF.BASE_URI + "fundedBy");

            /// <summary>
            /// foaf:geekcode
            /// </summary>
            public static readonly RDFResource GEEK_CODE = new RDFResource(FOAF.BASE_URI + "geekcode");

            /// <summary>
            /// foaf:theme
            /// </summary>
            public static readonly RDFResource THEME = new RDFResource(FOAF.BASE_URI + "theme");

            /// <summary>
            /// foaf:nick
            /// </summary>
            public static readonly RDFResource NICK = new RDFResource(FOAF.BASE_URI + "nick");
			
			/// <summary>
            /// foaf:mbox
            /// </summary>
            public static readonly RDFResource MBOX = new RDFResource(FOAF.BASE_URI + "mbox");
			
			/// <summary>
            /// foaf:homepage
            /// </summary>
            public static readonly RDFResource HOMEPAGE = new RDFResource(FOAF.BASE_URI + "homepage");
			
			/// <summary>
            /// foaf:weblog
            /// </summary>
            public static readonly RDFResource WEBLOG = new RDFResource(FOAF.BASE_URI + "weblog");
			
			/// <summary>
            /// foaf:openid
            /// </summary>
            public static readonly RDFResource OPEN_ID = new RDFResource(FOAF.BASE_URI + "openid");
			
			/// <summary>
            /// foaf:jabberID
            /// </summary>
            public static readonly RDFResource JABBER_ID = new RDFResource(FOAF.BASE_URI + "jabberID");

            /// <summary>
            /// foaf:aimChatID
            /// </summary>
            public static readonly RDFResource AIM_CHAT_ID = new RDFResource(FOAF.BASE_URI + "aimChatID");

            /// <summary>
            /// foaf:icqChatID
            /// </summary>
            public static readonly RDFResource ICQ_CHAT_ID = new RDFResource(FOAF.BASE_URI + "icqChatID");

            /// <summary>
            /// foaf:msnChatID
            /// </summary>
            public static readonly RDFResource MSN_CHAT_ID = new RDFResource(FOAF.BASE_URI + "msnChatID");

            /// <summary>
            /// foaf:yahooChatID
            /// </summary>
            public static readonly RDFResource YAHOO_CHAT_ID = new RDFResource(FOAF.BASE_URI + "yahooChatID");

            /// <summary>
            /// foaf:myersBriggs
            /// </summary>
            public static readonly RDFResource MYERS_BRIGGS = new RDFResource(FOAF.BASE_URI + "myersBriggs");

            /// <summary>
            /// foaf:dnaChecksum
            /// </summary>
            public static readonly RDFResource DNA_CHECKSUM = new RDFResource(FOAF.BASE_URI + "dnaChecksum");

            /// <summary>
            /// foaf:membershipClass
            /// </summary>
            public static readonly RDFResource MEMBERSHIP_CLASS = new RDFResource(FOAF.BASE_URI + "membershipClass");

            /// <summary>
            /// foaf:holdsAccount
            /// </summary>
            public static readonly RDFResource HOLDS_ACCOUNT = new RDFResource(FOAF.BASE_URI + "holdsAccount");

            /// <summary>
            /// foaf:firstName
            /// </summary>
            public static readonly RDFResource FIRSTNAME = new RDFResource(FOAF.BASE_URI + "firstName");

            /// <summary>
            /// foaf:surname
            /// </summary>
            public static readonly RDFResource SURNAME = new RDFResource(FOAF.BASE_URI + "surname");

            /// <summary>
            /// foaf:plan
            /// </summary>
            public static readonly RDFResource PLAN = new RDFResource(FOAF.BASE_URI + "plan");

            /// <summary>
            /// foaf:mbox_sha1sum
            /// </summary>
            public static readonly RDFResource MBOX_SHA1SUM = new RDFResource(FOAF.BASE_URI + "mbox_sha1sum");
			
			/// <summary>
            /// foaf:interest
            /// </summary>
            public static readonly RDFResource INTEREST = new RDFResource(FOAF.BASE_URI + "interest");
			
			/// <summary>
            /// foaf:topic_interest
            /// </summary>
            public static readonly RDFResource TOPIC_INTEREST = new RDFResource(FOAF.BASE_URI + "topic_interest");
			
			/// <summary>
            /// foaf:topic
            /// </summary>
            public static readonly RDFResource TOPIC = new RDFResource(FOAF.BASE_URI + "topic");
			
			/// <summary>
            /// foaf:page
            /// </summary>
            public static readonly RDFResource PAGE = new RDFResource(FOAF.BASE_URI + "page");
			
			/// <summary>
            /// foaf:workplaceHomepage
            /// </summary>
            public static readonly RDFResource WORKPLACE_HOMEPAGE = new RDFResource(FOAF.BASE_URI + "workplaceHomepage");
			
			/// <summary>
            /// foaf:workinfoHomepage
            /// </summary>
            public static readonly RDFResource WORKINFO_HOMEPAGE = new RDFResource(FOAF.BASE_URI + "workinfoHomepage");
			
			/// <summary>
            /// foaf:schoolHomepage
            /// </summary>
            public static readonly RDFResource SCHOOL_HOMEPAGE = new RDFResource(FOAF.BASE_URI + "schoolHomepage");
			
			/// <summary>
            /// foaf:publications
            /// </summary>
            public static readonly RDFResource PUBLICATIONS = new RDFResource(FOAF.BASE_URI + "publications");
			
			/// <summary>
            /// foaf:currentProject
            /// </summary>
            public static readonly RDFResource CURRENT_PROJECT = new RDFResource(FOAF.BASE_URI + "currentProject");
			
			/// <summary>
            /// foaf:pastProject
            /// </summary>
            public static readonly RDFResource PAST_PROJECT = new RDFResource(FOAF.BASE_URI + "pastProject");
			
			/// <summary>
            /// foaf:account
            /// </summary>
            public static readonly RDFResource ACCOUNT = new RDFResource(FOAF.BASE_URI + "account");
			
			/// <summary>
            /// foaf:OnlineAccount
            /// </summary>
            public static readonly RDFResource ONLINE_ACCOUNT = new RDFResource(FOAF.BASE_URI + "OnlineAccount");

            /// <summary>
            /// foaf:OnlineChatAccount
            /// </summary>
            public static readonly RDFResource ONLINE_CHAT_ACCOUNT = new RDFResource(FOAF.BASE_URI + "OnlineChatAccount");

            /// <summary>
            /// foaf:OnlineEcommerceAccount
            /// </summary>
            public static readonly RDFResource ONLINE_ECOMMERCE_ACCOUNT = new RDFResource(FOAF.BASE_URI + "OnlineEcommerceAccount");

            /// <summary>
            /// foaf:OnlineGamingAccount
            /// </summary>
            public static readonly RDFResource ONLINE_GAMING_ACCOUNT = new RDFResource(FOAF.BASE_URI + "OnlineGamingAccount");

            /// <summary>
            /// foaf:accountName
            /// </summary>
            public static readonly RDFResource ACCOUNT_NAME = new RDFResource(FOAF.BASE_URI + "accountName");
			
			/// <summary>
            /// foaf:accountServiceHomepage
            /// </summary>
            public static readonly RDFResource ACCOUNT_SERVICE_HOMEPAGE = new RDFResource(FOAF.BASE_URI + "accountServiceHomepage");
			
			/// <summary>
            /// foaf:PersonalProfileDocument
            /// </summary>
            public static readonly RDFResource PERSONAL_PROFILE_DOCUMENT = new RDFResource(FOAF.BASE_URI + "PersonalProfileDocument");
			
			/// <summary>
            /// foaf:tipjar
            /// </summary>
            public static readonly RDFResource TIPJAR = new RDFResource(FOAF.BASE_URI + "tipjar");
			
			/// <summary>
            /// foaf:sha1
            /// </summary>
            public static readonly RDFResource SHA1 = new RDFResource(FOAF.BASE_URI + "sha1");
			
			/// <summary>
            /// foaf:thumbnail
            /// </summary>
            public static readonly RDFResource THUMBNAIL = new RDFResource(FOAF.BASE_URI + "thumbnail");
			
			/// <summary>
            /// foaf:logo
            /// </summary>
            public static readonly RDFResource LOGO = new RDFResource(FOAF.BASE_URI + "logo");
			
			/// <summary>
            /// foaf:phone
            /// </summary>
            public static readonly RDFResource PHONE = new RDFResource(FOAF.BASE_URI + "phone");
			
			/// <summary>
            /// foaf:status
            /// </summary>
            public static readonly RDFResource STATUS = new RDFResource(FOAF.BASE_URI + "status");
			
			/// <summary>
            /// foaf:gender
            /// </summary>
            public static readonly RDFResource GENDER = new RDFResource(FOAF.BASE_URI + "gender");
			
			/// <summary>
            /// foaf:birthday
            /// </summary>
            public static readonly RDFResource BIRTHDAY = new RDFResource(FOAF.BASE_URI + "birthday");
            #endregion

        }
        #endregion

        #region GEO
        /// <summary>
        /// GEO represents the W3C GEO vocabulary.
        /// </summary>
        public static class GEO {

            #region Properties
            /// <summary>
            /// geo
            /// </summary>
            public static readonly String PREFIX = "geo";

            /// <summary>
            /// http://www.w3.org/2003/01/geo/wgs84_pos#
            /// </summary>
            public static readonly String BASE_URI = "http://www.w3.org/2003/01/geo/wgs84_pos#";

            /// <summary>
            /// geo:lat
            /// </summary>
            public static readonly RDFResource LAT = new RDFResource(GEO.BASE_URI + "lat");

            /// <summary>
            /// geo:long
            /// </summary>
            public static readonly RDFResource LONG = new RDFResource(GEO.BASE_URI + "long");

            /// <summary>
            /// geo:lat_long
            /// </summary>
            public static readonly RDFResource LAT_LONG = new RDFResource(GEO.BASE_URI + "lat_long");

            /// <summary>
            /// geo:alt
            /// </summary>
            public static readonly RDFResource ALT = new RDFResource(GEO.BASE_URI + "alt");

            /// <summary>
            /// geo:Point
            /// </summary>
            public static readonly RDFResource POINT = new RDFResource(GEO.BASE_URI + "Point");

            /// <summary>
            /// geo:SpatialThing
            /// </summary>
            public static readonly RDFResource SPATIAL_THING = new RDFResource(GEO.BASE_URI + "SpatialThing");

            /// <summary>
            /// geo:location
            /// </summary>
            public static readonly RDFResource LOCATION = new RDFResource(GEO.BASE_URI + "location");
            #endregion

        }
        #endregion

        #region SKOS
        /// <summary>
        /// SKOS represents the W3C SKOS vocabulary.
        /// </summary>
        public static class SKOS {

            #region Properties
            /// <summary>
            /// skos
            /// </summary>
            public static readonly String PREFIX = "skos";

            /// <summary>
            /// http://www.w3.org/2004/02/skos/core#
            /// </summary>
            public static readonly String BASE_URI = "http://www.w3.org/2004/02/skos/core#";

            /// <summary>
            /// skos:Concept
            /// </summary>
            public static readonly RDFResource CONCEPT = new RDFResource(SKOS.BASE_URI + "Concept");

            /// <summary>
            /// skos:ConceptScheme
            /// </summary>
            public static readonly RDFResource CONCEPT_SCHEME = new RDFResource(SKOS.BASE_URI + "ConceptScheme");

            /// <summary>
            /// skos:inScheme
            /// </summary>
            public static readonly RDFResource IN_SCHEME = new RDFResource(SKOS.BASE_URI + "inScheme");

            /// <summary>
            /// skos:hasTopConcept
            /// </summary>
            public static readonly RDFResource HAS_TOP_CONCEPT = new RDFResource(SKOS.BASE_URI + "hasTopConcept");

            /// <summary>
            /// skos:topConceptOf
            /// </summary>
            public static readonly RDFResource TOP_CONCEPT_OF = new RDFResource(SKOS.BASE_URI + "topConceptOf");

            /// <summary>
            /// skos:altLabel
            /// </summary>
            public static readonly RDFResource ALT_LABEL = new RDFResource(SKOS.BASE_URI + "altLabel");

            /// <summary>
            /// skos:hiddenLabel
            /// </summary>
            public static readonly RDFResource HIDDEN_LABEL = new RDFResource(SKOS.BASE_URI + "hiddenLabel");

            /// <summary>
            /// skos:prefLabel
            /// </summary>
            public static readonly RDFResource PREF_LABEL = new RDFResource(SKOS.BASE_URI + "prefLabel");

            /// <summary>
            /// skos:notation
            /// </summary>
            public static readonly RDFResource NOTATION = new RDFResource(SKOS.BASE_URI + "notation");

            /// <summary>
            /// skos:changeNote
            /// </summary>
            public static readonly RDFResource CHANGE_NOTE = new RDFResource(SKOS.BASE_URI + "changeNote");

            /// <summary>
            /// skos:definition
            /// </summary>
            public static readonly RDFResource DEFINITION = new RDFResource(SKOS.BASE_URI + "definition");

            /// <summary>
            /// skos:example
            /// </summary>
            public static readonly RDFResource EXAMPLE = new RDFResource(SKOS.BASE_URI + "example");

            /// <summary>
            /// skos:editorialNote
            /// </summary>
            public static readonly RDFResource EDITORIAL_NOTE = new RDFResource(SKOS.BASE_URI + "editorialNote");

            /// <summary>
            /// skos:historyNote
            /// </summary>
            public static readonly RDFResource HISTORY_NOTE = new RDFResource(SKOS.BASE_URI + "historyNote");

            /// <summary>
            /// skos:note
            /// </summary>
            public static readonly RDFResource NOTE = new RDFResource(SKOS.BASE_URI + "note");

            /// <summary>
            /// skos:scopeNote
            /// </summary>
            public static readonly RDFResource SCOPE_NOTE = new RDFResource(SKOS.BASE_URI + "scopeNote");

            /// <summary>
            /// skos:broader
            /// </summary>
            public static readonly RDFResource BROADER = new RDFResource(SKOS.BASE_URI + "broader");

            /// <summary>
            /// skos:broaderTransitive
            /// </summary>
            public static readonly RDFResource BROADER_TRANSITIVE = new RDFResource(SKOS.BASE_URI + "broaderTransitive");

            /// <summary>
            /// skos:narrower
            /// </summary>
            public static readonly RDFResource NARROWER = new RDFResource(SKOS.BASE_URI + "narrower");

            /// <summary>
            /// skos:narrowerTransitive
            /// </summary>
            public static readonly RDFResource NARROWER_TRANSITIVE = new RDFResource(SKOS.BASE_URI + "narrowerTransitive");

            /// <summary>
            /// skos:related
            /// </summary>
            public static readonly RDFResource RELATED = new RDFResource(SKOS.BASE_URI + "related");

            /// <summary>
            /// skos:semanticRelation
            /// </summary>
            public static readonly RDFResource SEMANTIC_RELATION = new RDFResource(SKOS.BASE_URI + "semanticRelation");

            /// <summary>
            /// skos:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(SKOS.BASE_URI + "subject");

            /// <summary>
            /// skos:Collection
            /// </summary>
            public static readonly RDFResource COLLECTION = new RDFResource(SKOS.BASE_URI + "Collection");

            /// <summary>
            /// skos:OrderedCollection
            /// </summary>
            public static readonly RDFResource ORDERED_COLLECTION = new RDFResource(SKOS.BASE_URI + "OrderedCollection");

            /// <summary>
            /// skos:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource(SKOS.BASE_URI + "member");

            /// <summary>
            /// skos:memberList
            /// </summary>
            public static readonly RDFResource MEMBER_LIST = new RDFResource(SKOS.BASE_URI + "memberList");

            /// <summary>
            /// skos:broadMatch
            /// </summary>
            public static readonly RDFResource BROAD_MATCH = new RDFResource(SKOS.BASE_URI + "broadMatch");

            /// <summary>
            /// skos:closeMatch
            /// </summary>
            public static readonly RDFResource CLOSE_MATCH = new RDFResource(SKOS.BASE_URI + "closeMatch");

            /// <summary>
            /// skos:narrowMatch
            /// </summary>
            public static readonly RDFResource NARROW_MATCH = new RDFResource(SKOS.BASE_URI + "narrowMatch");

            /// <summary>
            /// skos:relatedMatch
            /// </summary>
            public static readonly RDFResource RELATED_MATCH = new RDFResource(SKOS.BASE_URI + "relatedMatch");

            /// <summary>
            /// skos:exactMatch
            /// </summary>
            public static readonly RDFResource EXACT_MATCH = new RDFResource(SKOS.BASE_URI + "exactMatch");

            /// <summary>
            /// skos:mappingRelation
            /// </summary>
            public static readonly RDFResource MAPPING_RELATION = new RDFResource(SKOS.BASE_URI + "mappingRelation");
            #endregion

            #region Extended Properties
            public static class SKOSXL {

                #region Properties
                /// <summary>
                /// skosxl
                /// </summary>
                public static readonly String PREFIX = "skosxl";

                /// <summary>
                /// http://www.w3.org/2008/05/skos-xl#
                /// </summary>
                public static readonly String BASE_URI = "http://www.w3.org/2008/05/skos-xl#";

                /// <summary>
                /// skosxl:Label
                /// </summary>
                public static readonly RDFResource LABEL = new RDFResource(SKOSXL.BASE_URI + "Label");

                /// <summary>
                /// skosxl:altLabel
                /// </summary>
                public static readonly RDFResource ALT_LABEL = new RDFResource(SKOSXL.BASE_URI + "altLabel");

                /// <summary>
                /// skosxl:hiddenLabel
                /// </summary>
                public static readonly RDFResource HIDDEN_LABEL = new RDFResource(SKOSXL.BASE_URI + "hiddenLabel");

                /// <summary>
                /// skosxl:labelRelation
                /// </summary>
                public static readonly RDFResource LABEL_RELATION = new RDFResource(SKOSXL.BASE_URI + "labelRelation");

                /// <summary>
                /// skosxl:literalForm
                /// </summary>
                public static readonly RDFResource LITERAL_FORM = new RDFResource(SKOSXL.BASE_URI + "literalForm");

                /// <summary>
                /// skosxl:prefLabel
                /// </summary>
                public static readonly RDFResource PREF_LABEL = new RDFResource(SKOSXL.BASE_URI + "prefLabel");
                #endregion

            }
            #endregion

        }
        #endregion

        #region SIOC
        /// <summary>
        /// SIOC represents the Semantically-Interlinked Online Communities vocabulary.
        /// </summary>
        public static class SIOC {

            #region Properties
            /// <summary>
            /// sioc
            /// </summary>
            public static readonly String PREFIX = "sioc";

            /// <summary>
            /// http://rdfs.org/sioc/ns#
            /// </summary>
            public static readonly String BASE_URI = "http://rdfs.org/sioc/ns#";

            /// <summary>
            /// sioc:Community
            /// </summary>
            public static readonly RDFResource COMMUNITY = new RDFResource(SIOC.BASE_URI + "Community");

            /// <summary>
            /// sioc:Container
            /// </summary>
            public static readonly RDFResource CONTAINER = new RDFResource(SIOC.BASE_URI + "Container");

            /// <summary>
            /// sioc:Forum
            /// </summary>
            public static readonly RDFResource FORUM = new RDFResource(SIOC.BASE_URI + "Forum");

            /// <summary>
            /// sioc:Item
            /// </summary>
            public static readonly RDFResource ITEM = new RDFResource(SIOC.BASE_URI + "Item");

            /// <summary>
            /// sioc:Post
            /// </summary>
            public static readonly RDFResource POST = new RDFResource(SIOC.BASE_URI + "Post");

            /// <summary>
            /// sioc:Role
            /// </summary>
            public static readonly RDFResource ROLE = new RDFResource(SIOC.BASE_URI + "Role");

            /// <summary>
            /// sioc:Space
            /// </summary>
            public static readonly RDFResource SPACE = new RDFResource(SIOC.BASE_URI + "Space");

            /// <summary>
            /// sioc:Site
            /// </summary>
            public static readonly RDFResource SITE = new RDFResource(SIOC.BASE_URI + "Site");

            /// <summary>
            /// sioc:Thread
            /// </summary>
            public static readonly RDFResource THREAD = new RDFResource(SIOC.BASE_URI + "Thread");

            /// <summary>
            /// sioc:UserAccount
            /// </summary>
            public static readonly RDFResource USER_ACCOUNT = new RDFResource(SIOC.BASE_URI + "UserAccount");

            /// <summary>
            /// sioc:Usergroup
            /// </summary>
            public static readonly RDFResource USER_GROUP = new RDFResource(SIOC.BASE_URI + "Usergroup");

            /// <summary>
            /// sioc:about
            /// </summary>
            public static readonly RDFResource ABOUT = new RDFResource(SIOC.BASE_URI + "about");

            /// <summary>
            /// sioc:account_of
            /// </summary>
            public static readonly RDFResource ACCOUNT_OF = new RDFResource(SIOC.BASE_URI + "account_of");

            /// <summary>
            /// sioc:addressed_to
            /// </summary>
            public static readonly RDFResource ADDRESSED_TO = new RDFResource(SIOC.BASE_URI + "addressed_to");

            /// <summary>
            /// sioc:administrator_of
            /// </summary>
            public static readonly RDFResource ADMINISTRATOR_OF = new RDFResource(SIOC.BASE_URI + "administrator_of");

            /// <summary>
            /// sioc:attachment
            /// </summary>
            public static readonly RDFResource ATTACHMENT = new RDFResource(SIOC.BASE_URI + "attachment");

            /// <summary>
            /// sioc:avatar
            /// </summary>
            public static readonly RDFResource AVATAR = new RDFResource(SIOC.BASE_URI + "avatar");

            /// <summary>
            /// sioc:container_of
            /// </summary>
            public static readonly RDFResource CONTAINER_OF = new RDFResource(SIOC.BASE_URI + "container_of");

            /// <summary>
            /// sioc:content
            /// </summary>
            public static readonly RDFResource CONTENT = new RDFResource(SIOC.BASE_URI + "content");

            /// <summary>
            /// sioc:creator_of
            /// </summary>
            public static readonly RDFResource CREATOR_OF = new RDFResource(SIOC.BASE_URI + "creator_of");

            /// <summary>
            /// sioc:earlier_version
            /// </summary>
            public static readonly RDFResource EARLIER_VERSION = new RDFResource(SIOC.BASE_URI + "earlier_version");

            /// <summary>
            /// sioc:email
            /// </summary>
            public static readonly RDFResource EMAIL = new RDFResource(SIOC.BASE_URI + "email");

            /// <summary>
            /// sioc:email_sha1
            /// </summary>
            public static readonly RDFResource EMAIL_SHA1 = new RDFResource(SIOC.BASE_URI + "email_sha1");

            /// <summary>
            /// sioc:embeds_knowledge
            /// </summary>
            public static readonly RDFResource EMBEDS_KNOWLEDGE = new RDFResource(SIOC.BASE_URI + "embeds_knowledge");

            /// <summary>
            /// sioc:feed
            /// </summary>
            public static readonly RDFResource FEED = new RDFResource(SIOC.BASE_URI + "feed");

            /// <summary>
            /// sioc:follows
            /// </summary>
            public static readonly RDFResource FOLLOWS = new RDFResource(SIOC.BASE_URI + "follows");

            /// <summary>
            /// sioc:function_of
            /// </summary>
            public static readonly RDFResource FUNCTION_OF = new RDFResource(SIOC.BASE_URI + "function_of");

            /// <summary>
            /// sioc:has_administrator
            /// </summary>
            public static readonly RDFResource HAS_ADMINISTRATOR = new RDFResource(SIOC.BASE_URI + "has_administrator");

            /// <summary>
            /// sioc:has_container
            /// </summary>
            public static readonly RDFResource HAS_CONTAINER = new RDFResource(SIOC.BASE_URI + "has_container");

            /// <summary>
            /// sioc:has_creator
            /// </summary>
            public static readonly RDFResource HAS_CREATOR = new RDFResource(SIOC.BASE_URI + "has_creator");

            /// <summary>
            /// sioc:has_discussion
            /// </summary>
            public static readonly RDFResource HAS_DISCUSSION = new RDFResource(SIOC.BASE_URI + "has_discussion");

            /// <summary>
            /// sioc:has_function
            /// </summary>
            public static readonly RDFResource HAS_FUNCTION = new RDFResource(SIOC.BASE_URI + "has_function");

            /// <summary>
            /// sioc:has_host
            /// </summary>
            public static readonly RDFResource HAS_HOST = new RDFResource(SIOC.BASE_URI + "has_host");

            /// <summary>
            /// sioc:has_member
            /// </summary>
            public static readonly RDFResource HAS_MEMBER = new RDFResource(SIOC.BASE_URI + "has_member");

            /// <summary>
            /// sioc:has_moderator
            /// </summary>
            public static readonly RDFResource HAS_MODERATOR = new RDFResource(SIOC.BASE_URI + "has_moderator");

            /// <summary>
            /// sioc:has_modifier
            /// </summary>
            public static readonly RDFResource HAS_MODIFIER = new RDFResource(SIOC.BASE_URI + "has_modifier");

            /// <summary>
            /// sioc:has_owner
            /// </summary>
            public static readonly RDFResource HAS_OWNER = new RDFResource(SIOC.BASE_URI + "has_owner");

            /// <summary>
            /// sioc:has_parent
            /// </summary>
            public static readonly RDFResource HAS_PARENT = new RDFResource(SIOC.BASE_URI + "has_parent");

            /// <summary>
            /// sioc:has_reply
            /// </summary>
            public static readonly RDFResource HAS_REPLY = new RDFResource(SIOC.BASE_URI + "has_reply");

            /// <summary>
            /// sioc:has_scope
            /// </summary>
            public static readonly RDFResource HAS_SCOPE = new RDFResource(SIOC.BASE_URI + "has_scope");

            /// <summary>
            /// sioc:has_space
            /// </summary>
            public static readonly RDFResource HAS_SPACE = new RDFResource(SIOC.BASE_URI + "has_space");

            /// <summary>
            /// sioc:has_subscriber
            /// </summary>
            public static readonly RDFResource HAS_SUBSCRIBER = new RDFResource(SIOC.BASE_URI + "has_subscriber");

            /// <summary>
            /// sioc:has_usergroup
            /// </summary>
            public static readonly RDFResource HAS_USERGROUP = new RDFResource(SIOC.BASE_URI + "has_usergroup");

            /// <summary>
            /// sioc:host_of
            /// </summary>
            public static readonly RDFResource HOST_OF = new RDFResource(SIOC.BASE_URI + "host_of");

            /// <summary>
            /// sioc:id
            /// </summary>
            public static readonly RDFResource ID = new RDFResource(SIOC.BASE_URI + "id");

            /// <summary>
            /// sioc:ip_address
            /// </summary>
            public static readonly RDFResource IP_ADDRESS = new RDFResource(SIOC.BASE_URI + "ip_address");

            /// <summary>
            /// sioc:last_activity_date
            /// </summary>
            public static readonly RDFResource LAST_ACTIVITY_DATE = new RDFResource(SIOC.BASE_URI + "last_activity_date");

            /// <summary>
            /// sioc:last_item_date
            /// </summary>
            public static readonly RDFResource LAST_ITEM_DATE = new RDFResource(SIOC.BASE_URI + "last_item_date");

            /// <summary>
            /// sioc:last_reply_date
            /// </summary>
            public static readonly RDFResource LAST_REPLY_DATE = new RDFResource(SIOC.BASE_URI + "last_reply_date");

            /// <summary>
            /// sioc:later_version
            /// </summary>
            public static readonly RDFResource LATER_VERSION = new RDFResource(SIOC.BASE_URI + "later_version");

            /// <summary>
            /// sioc:latest_version
            /// </summary>
            public static readonly RDFResource LATEST_VERSION = new RDFResource(SIOC.BASE_URI + "latest_version");

            /// <summary>
            /// sioc:link
            /// </summary>
            public static readonly RDFResource LINK = new RDFResource(SIOC.BASE_URI + "link");

            /// <summary>
            /// sioc:links_to
            /// </summary>
            public static readonly RDFResource LINKS_TO = new RDFResource(SIOC.BASE_URI + "links_to");

            /// <summary>
            /// sioc:member_of
            /// </summary>
            public static readonly RDFResource MEMBER_OF = new RDFResource(SIOC.BASE_URI + "member_of");

            /// <summary>
            /// sioc:moderator_of
            /// </summary>
            public static readonly RDFResource MODERATOR_OF = new RDFResource(SIOC.BASE_URI + "moderator_of");

            /// <summary>
            /// sioc:modifier_of
            /// </summary>
            public static readonly RDFResource MODIFIER_OF = new RDFResource(SIOC.BASE_URI + "modifier_of");

            /// <summary>
            /// sioc:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(SIOC.BASE_URI + "name");

            /// <summary>
            /// sioc:next_by_date
            /// </summary>
            public static readonly RDFResource NEXT_BY_DATE = new RDFResource(SIOC.BASE_URI + "next_by_date");

            /// <summary>
            /// sioc:next_version
            /// </summary>
            public static readonly RDFResource NEXT_VERSION = new RDFResource(SIOC.BASE_URI + "next_version");

            /// <summary>
            /// sioc:note
            /// </summary>
            public static readonly RDFResource NOTE = new RDFResource(SIOC.BASE_URI + "note");

            /// <summary>
            /// sioc:num_authors
            /// </summary>
            public static readonly RDFResource NUM_AUTHORS = new RDFResource(SIOC.BASE_URI + "num_authors");

            /// <summary>
            /// sioc:num_items
            /// </summary>
            public static readonly RDFResource NUM_ITEMS = new RDFResource(SIOC.BASE_URI + "num_items");

            /// <summary>
            /// sioc:num_replies
            /// </summary>
            public static readonly RDFResource NUM_REPLIES = new RDFResource(SIOC.BASE_URI + "num_replies");

            /// <summary>
            /// sioc:num_threads
            /// </summary>
            public static readonly RDFResource NUM_THREADS = new RDFResource(SIOC.BASE_URI + "num_threads");

            /// <summary>
            /// sioc:num_views
            /// </summary>
            public static readonly RDFResource NUM_VIEWS = new RDFResource(SIOC.BASE_URI + "num_views");

            /// <summary>
            /// sioc:owner_of
            /// </summary>
            public static readonly RDFResource OWNER_OF = new RDFResource(SIOC.BASE_URI + "owner_of");

            /// <summary>
            /// sioc:parent_of
            /// </summary>
            public static readonly RDFResource PARENT_OF = new RDFResource(SIOC.BASE_URI + "parent_of");

            /// <summary>
            /// sioc:previous_by_date
            /// </summary>
            public static readonly RDFResource PREVIOUS_BY_DATE = new RDFResource(SIOC.BASE_URI + "previous_by_date");

            /// <summary>
            /// sioc:previous_version
            /// </summary>
            public static readonly RDFResource PREVIOUS_VERSION = new RDFResource(SIOC.BASE_URI + "previous_version");

            /// <summary>
            /// sioc:related_to
            /// </summary>
            public static readonly RDFResource RELATED_TO = new RDFResource(SIOC.BASE_URI + "related_to");

            /// <summary>
            /// sioc:reply_of
            /// </summary>
            public static readonly RDFResource REPLY_OF = new RDFResource(SIOC.BASE_URI + "reply_of");

            /// <summary>
            /// sioc:scope_of
            /// </summary>
            public static readonly RDFResource SCOPE_OF = new RDFResource(SIOC.BASE_URI + "scope_of");

            /// <summary>
            /// sioc:sibling
            /// </summary>
            public static readonly RDFResource SIBLING = new RDFResource(SIOC.BASE_URI + "sibling");

            /// <summary>
            /// sioc:space_of
            /// </summary>
            public static readonly RDFResource SPACE_OF = new RDFResource(SIOC.BASE_URI + "space_of");

            /// <summary>
            /// sioc:subscriber_of
            /// </summary>
            public static readonly RDFResource SUBSCRIBER_OF = new RDFResource(SIOC.BASE_URI + "subscriber_of");

            /// <summary>
            /// sioc:topic
            /// </summary>
            public static readonly RDFResource TOPIC = new RDFResource(SIOC.BASE_URI + "topic");

            /// <summary>
            /// sioc:usergroup_of
            /// </summary>
            public static readonly RDFResource USERGROUP_OF = new RDFResource(SIOC.BASE_URI + "usergroup_of");

            /// <summary>
            /// sioc:User
            /// </summary>
            public static readonly RDFResource USER = new RDFResource(SIOC.BASE_URI + "User");

            /// <summary>
            /// sioc:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource(SIOC.BASE_URI + "title");

            /// <summary>
            /// sioc:content_ecoded
            /// </summary>
            public static readonly RDFResource CONTENT_ENCODED = new RDFResource(SIOC.BASE_URI + "content_ecoded");

            /// <summary>
            /// sioc:created_at
            /// </summary>
            public static readonly RDFResource CREATED_AT = new RDFResource(SIOC.BASE_URI + "created_at");

            /// <summary>
            /// sioc:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(SIOC.BASE_URI + "description");

            /// <summary>
            /// sioc:first_name
            /// </summary>
            public static readonly RDFResource FIRST_NAME = new RDFResource(SIOC.BASE_URI + "first_name");

            /// <summary>
            /// sioc:last_name
            /// </summary>
            public static readonly RDFResource LAST_NAME = new RDFResource(SIOC.BASE_URI + "last_name");

            /// <summary>
            /// sioc:group_of
            /// </summary>
            public static readonly RDFResource GROUP_OF = new RDFResource(SIOC.BASE_URI + "group_of");

            /// <summary>
            /// sioc:has_group
            /// </summary>
            public static readonly RDFResource HAS_GROUP = new RDFResource(SIOC.BASE_URI + "has_group");

            /// <summary>
            /// sioc:has_part
            /// </summary>
            public static readonly RDFResource HAS_PART = new RDFResource(SIOC.BASE_URI + "has_part");

            /// <summary>
            /// sioc:modified_at
            /// </summary>
            public static readonly RDFResource MODIFIED_AT = new RDFResource(SIOC.BASE_URI + "modified_at");

            /// <summary>
            /// sioc:part_of
            /// </summary>
            public static readonly RDFResource PART_OF = new RDFResource(SIOC.BASE_URI + "part_of");

            /// <summary>
            /// sioc:reference
            /// </summary>
            public static readonly RDFResource REFERENCE = new RDFResource(SIOC.BASE_URI + "reference");

            /// <summary>
            /// sioc:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(SIOC.BASE_URI + "subject");
            #endregion

        }
        #endregion

        #endregion

    }

}