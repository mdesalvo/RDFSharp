/*
   Copyright 2012-2016 Marco De Salvo

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

            /// <summary>
            /// rdf:HTML
            /// </summary>
            public static readonly RDFResource HTML = new RDFResource(RDF.BASE_URI + "HTML");
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
                /// dcterms:bibliographicResource
                /// </summary>
                public static readonly RDFResource BIBLIOGRAPHIC_RESOURCE = new RDFResource(DCTERMS.BASE_URI + "bibliographicResource");

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
                public static readonly RDFResource DCMI_TYPE = new RDFResource(DCTERMS.BASE_URI + "DCMIType");

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

        }
        #endregion

        #region RSS
        /// <summary>
        /// RSS represents the RSS 1.0 vocabulary.
        /// </summary>
        public static class RSS {

            #region Properties
            /// <summary>
            /// rss
            /// </summary>
            public static readonly String PREFIX = "rss";

            /// <summary>
            /// http://purl.org/rss/1.0/
            /// </summary>
            public static readonly String BASE_URI = "http://purl.org/rss/1.0/";

            /// <summary>
            /// rss:channel
            /// </summary>
            public static readonly RDFResource CHANNEL = new RDFResource(RSS.BASE_URI + "channel");

            /// <summary>
            /// rss:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource(RSS.BASE_URI + "title");

            /// <summary>
            /// rss:link
            /// </summary>
            public static readonly RDFResource LINK = new RDFResource(RSS.BASE_URI + "link");

            /// <summary>
            /// rss:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(RSS.BASE_URI + "description");

            /// <summary>
            /// rss:image
            /// </summary>
            public static readonly RDFResource IMAGE = new RDFResource(RSS.BASE_URI + "image");

            /// <summary>
            /// rss:items
            /// </summary>
            public static readonly RDFResource ITEMS = new RDFResource(RSS.BASE_URI + "items");

            /// <summary>
            /// rss:textinput
            /// </summary>
            public static readonly RDFResource TEXT_INPUT = new RDFResource(RSS.BASE_URI + "textinput");

            /// <summary>
            /// rss:item
            /// </summary>
            public static readonly RDFResource ITEM = new RDFResource(RSS.BASE_URI + "item");

            /// <summary>
            /// rss:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(RSS.BASE_URI + "name");

            /// <summary>
            /// rss:url
            /// </summary>
            public static readonly RDFResource URL = new RDFResource(RSS.BASE_URI + "url");
            #endregion

        }
        #endregion

		#region DBPEDIA
        /// <summary>
        /// DBPEDIA represents the DBPEDIA vocabulary.
        /// </summary>
        public static class DBPEDIA {

            #region Properties
            /// <summary>
            /// dbpedia
            /// </summary>
            public static readonly String  PREFIX = "dbpedia";

            /// <summary>
            /// http://dbpedia.org/
            /// </summary>
            public static readonly String  BASE_URI = "http://dbpedia.org/";

            /// <summary>
            /// dbpedia:resource
            /// </summary>
            public static readonly RDFResource RESOURCE = new RDFResource(DBPEDIA.BASE_URI + "resource/");

			/// <summary>
            /// dbpedia:class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource(DBPEDIA.BASE_URI + "class/");

            /// <summary>
            /// dbpedia:class_yago
            /// </summary>
            public static readonly RDFResource CLASS_YAGO = new RDFResource(DBPEDIA.BASE_URI + "class/yago/");

            /// <summary>
            /// dbpedia:ontology
            /// </summary>
            public static readonly RDFResource ONTOLOGY = new RDFResource(DBPEDIA.BASE_URI + "ontology/");
			
			/// <summary>
            /// dbpedia:property
            /// </summary>
            public static readonly RDFResource PROPERTY = new RDFResource(DBPEDIA.BASE_URI + "property/");
            #endregion

        }
        #endregion

        #region OG
        /// <summary>
        /// OG represents the Open Graph Protocol Vocabulary
        /// </summary>
        public static class OG {

            #region Properties
            /// <summary>
            /// og
            /// </summary>
            public static readonly String PREFIX = "og";

            /// <summary>
            /// http://ogp.me/ns#
            /// </summary>
            public static readonly String BASE_URI = "http://ogp.me/ns#";

            /// <summary>
            /// og:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource(OG.BASE_URI + "title");

            /// <summary>
            /// og:type
            /// </summary>
            public static readonly RDFResource TYPE = new RDFResource(OG.BASE_URI + "type");

            /// <summary>
            /// og:url
            /// </summary>
            public static readonly RDFResource URL = new RDFResource(OG.BASE_URI + "url");

            /// <summary>
            /// og:image
            /// </summary>
            public static readonly RDFResource IMAGE = new RDFResource(OG.BASE_URI + "image");

            /// <summary>
            /// og:image:secure_url
            /// </summary>
            public static readonly RDFResource IMAGE_SECURE_URL = new RDFResource(OG.BASE_URI + "image:secure_url");

            /// <summary>
            /// og:image:type
            /// </summary>
            public static readonly RDFResource IMAGE_TYPE = new RDFResource(OG.BASE_URI + "image:type");

            /// <summary>
            /// og:image:width
            /// </summary>
            public static readonly RDFResource IMAGE_WIDTH = new RDFResource(OG.BASE_URI + "image:width");

            /// <summary>
            /// og:image:height
            /// </summary>
            public static readonly RDFResource IMAGE_HEIGHT = new RDFResource(OG.BASE_URI + "image:height");

            /// <summary>
            /// og:audio
            /// </summary>
            public static readonly RDFResource AUDIO = new RDFResource(OG.BASE_URI + "audio");

            /// <summary>
            /// og:audio:secure_url
            /// </summary>
            public static readonly RDFResource AUDIO_SECURE_URL = new RDFResource(OG.BASE_URI + "audio:secure_url");

            /// <summary>
            /// og:audio:type
            /// </summary>
            public static readonly RDFResource AUDIO_TYPE = new RDFResource(OG.BASE_URI + "audio:type");

            /// <summary>
            /// og:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(OG.BASE_URI + "description");

            /// <summary>
            /// og:determiner
            /// </summary>
            public static readonly RDFResource DETERMINER = new RDFResource(OG.BASE_URI + "determiner");

            /// <summary>
            /// og:locale
            /// </summary>
            public static readonly RDFResource LOCALE = new RDFResource(OG.BASE_URI + "locale");

            /// <summary>
            /// og:locale:alternate
            /// </summary>
            public static readonly RDFResource LOCALE_ALTERNATE = new RDFResource(OG.BASE_URI + "locale:alternate");

            /// <summary>
            /// og:site_name
            /// </summary>
            public static readonly RDFResource SITE_NAME = new RDFResource(OG.BASE_URI + "site_name");

            /// <summary>
            /// og:video
            /// </summary>
            public static readonly RDFResource VIDEO = new RDFResource(OG.BASE_URI + "video");

            /// <summary>
            /// og:video:secure_url
            /// </summary>
            public static readonly RDFResource VIDEO_SECURE_URL = new RDFResource(OG.BASE_URI + "video:secure_url");

            /// <summary>
            /// og:video:type
            /// </summary>
            public static readonly RDFResource VIDEO_TYPE = new RDFResource(OG.BASE_URI + "video:type");

            /// <summary>
            /// og:video:width
            /// </summary>
            public static readonly RDFResource VIDEO_WIDTH = new RDFResource(OG.BASE_URI + "video:width");

            /// <summary>
            /// og:video:height
            /// </summary>
            public static readonly RDFResource VIDEO_HEIGHT = new RDFResource(OG.BASE_URI + "video:height");
            #endregion

            #region Extended Properties

            #region OG_MUSIC
            /// <summary>
            /// OG_MUSIC represents music-related concepts in Open Graph Protocol Vocabulary
            /// </summary>
            public static class OG_MUSIC {

                #region Properties
                /// <summary>
                /// ogm
                /// </summary>
                public static readonly String PREFIX = "ogm";

                /// <summary>
                /// http://ogp.me/ns/music#
                /// </summary>
                public static readonly String BASE_URI = "http://ogp.me/ns/music#";

                /// <summary>
                /// ogm:song
                /// </summary>
                public static readonly RDFResource SONG = new RDFResource(OG_MUSIC.BASE_URI + "song");

                /// <summary>
                /// ogm:song:disc
                /// </summary>
                public static readonly RDFResource SONG_DISC = new RDFResource(OG_MUSIC.BASE_URI + "song:disc");

                /// <summary>
                /// ogm:song:track
                /// </summary>
                public static readonly RDFResource SONG_TRACK = new RDFResource(OG_MUSIC.BASE_URI + "song:track");

                /// <summary>
                /// ogm:duration
                /// </summary>
                public static readonly RDFResource DURATION = new RDFResource(OG_MUSIC.BASE_URI + "duration");

                /// <summary>
                /// ogm:album
                /// </summary>
                public static readonly RDFResource ALBUM = new RDFResource(OG_MUSIC.BASE_URI + "album");

                /// <summary>
                /// ogm:album:disc
                /// </summary>
                public static readonly RDFResource ALBUM_DISC = new RDFResource(OG_MUSIC.BASE_URI + "album:disc");

                /// <summary>
                /// ogm:album:track
                /// </summary>
                public static readonly RDFResource ALBUM_TRACK = new RDFResource(OG_MUSIC.BASE_URI + "album:track");

                /// <summary>
                /// ogm:musician
                /// </summary>
                public static readonly RDFResource MUSICIAN = new RDFResource(OG_MUSIC.BASE_URI + "musician");

                /// <summary>
                /// ogm:release_date
                /// </summary>
                public static readonly RDFResource RELEASE_DATE = new RDFResource(OG_MUSIC.BASE_URI + "release_date");

                /// <summary>
                /// ogm:playlist
                /// </summary>
                public static readonly RDFResource PLAYLIST = new RDFResource(OG_MUSIC.BASE_URI + "playlist");

                /// <summary>
                /// ogm:creator
                /// </summary>
                public static readonly RDFResource CREATOR = new RDFResource(OG_MUSIC.BASE_URI + "creator");

                /// <summary>
                /// ogm:radio_station
                /// </summary>
                public static readonly RDFResource RADIO_STATION = new RDFResource(OG_MUSIC.BASE_URI + "radio_station");
                #endregion

            }
            #endregion

            #region OG_VIDEO
            /// <summary>
            /// OG_VIDEO represents video-related concepts in Open Graph Protocol Vocabulary
            /// </summary>
            public static class OG_VIDEO {

                #region Properties
                /// <summary>
                /// ogv
                /// </summary>
                public static readonly String PREFIX = "ogv";

                /// <summary>
                /// http://ogp.me/ns/video#
                /// </summary>
                public static readonly String BASE_URI = "http://ogp.me/ns/video#";

                /// <summary>
                /// ogv:movie
                /// </summary>
                public static readonly RDFResource MOVIE = new RDFResource(OG_VIDEO.BASE_URI + "movie");

                /// <summary>
                /// ogv:actor
                /// </summary>
                public static readonly RDFResource ACTOR = new RDFResource(OG_VIDEO.BASE_URI + "actor");

                /// <summary>
                /// ogv:actor:role
                /// </summary>
                public static readonly RDFResource ACTOR_ROLE = new RDFResource(OG_VIDEO.BASE_URI + "actor:role");

                /// <summary>
                /// ogv:director
                /// </summary>
                public static readonly RDFResource DIRECTOR = new RDFResource(OG_VIDEO.BASE_URI + "director");

                /// <summary>
                /// ogv:writer
                /// </summary>
                public static readonly RDFResource WRITER = new RDFResource(OG_VIDEO.BASE_URI + "writer");

                /// <summary>
                /// ogv:duration
                /// </summary>
                public static readonly RDFResource DURATION = new RDFResource(OG_VIDEO.BASE_URI + "duration");

                /// <summary>
                /// ogv:release_date
                /// </summary>
                public static readonly RDFResource RELEASE_DATE = new RDFResource(OG_VIDEO.BASE_URI + "release_date");

                /// <summary>
                /// ogv:tag
                /// </summary>
                public static readonly RDFResource TAG = new RDFResource(OG_VIDEO.BASE_URI + "tag");

                /// <summary>
                /// ogv:episode
                /// </summary>
                public static readonly RDFResource EPISODE = new RDFResource(OG_VIDEO.BASE_URI + "episode");

                /// <summary>
                /// ogv:series
                /// </summary>
                public static readonly RDFResource SERIES = new RDFResource(OG_VIDEO.BASE_URI + "series");

                /// <summary>
                /// ogv:tv_show
                /// </summary>
                public static readonly RDFResource TV_SHOW = new RDFResource(OG_VIDEO.BASE_URI + "tv_show");

                /// <summary>
                /// ogv:other
                /// </summary>
                public static readonly RDFResource OTHER = new RDFResource(OG_VIDEO.BASE_URI + "other");
                #endregion

            }
            #endregion

            #region OG_ARTICLE
            /// <summary>
            /// OG_ARTICLE represents article-related concepts in Open Graph Protocol Vocabulary
            /// </summary>
            public static class OG_ARTICLE {

                #region Properties
                /// <summary>
                /// oga
                /// </summary>
                public static readonly String PREFIX = "oga";

                /// <summary>
                /// http://ogp.me/ns/article#
                /// </summary>
                public static readonly String BASE_URI = "http://ogp.me/ns/article#";

                /// <summary>
                /// oga:published_time
                /// </summary>
                public static readonly RDFResource PUBLISHED_TIME = new RDFResource(OG_ARTICLE.BASE_URI + "published_time");

                /// <summary>
                /// oga:modified_time
                /// </summary>
                public static readonly RDFResource MODIFIED_TIME = new RDFResource(OG_ARTICLE.BASE_URI + "modified_time");

                /// <summary>
                /// oga:expiration_time
                /// </summary>
                public static readonly RDFResource EXPIRATION_TIME = new RDFResource(OG_ARTICLE.BASE_URI + "expiration_time");

                /// <summary>
                /// oga:author
                /// </summary>
                public static readonly RDFResource AUTHOR = new RDFResource(OG_ARTICLE.BASE_URI + "author");

                /// <summary>
                /// oga:section
                /// </summary>
                public static readonly RDFResource SECTION = new RDFResource(OG_ARTICLE.BASE_URI + "section");

                /// <summary>
                /// oga:tag
                /// </summary>
                public static readonly RDFResource TAG = new RDFResource(OG_ARTICLE.BASE_URI + "tag");
                #endregion

            }
            #endregion

            #region OG_BOOK
            /// <summary>
            /// OG_BOOK represents book-related concepts in Open Graph Protocol Vocabulary
            /// </summary>
            public static class OG_BOOK {

                #region Properties
                /// <summary>
                /// ogb
                /// </summary>
                public static readonly String PREFIX = "ogb";

                /// <summary>
                /// http://ogp.me/ns/book#
                /// </summary>
                public static readonly String BASE_URI = "http://ogp.me/ns/book#";

                /// <summary>
                /// ogb:author
                /// </summary>
                public static readonly RDFResource AUTHOR = new RDFResource(OG_BOOK.BASE_URI + "author");

                /// <summary>
                /// ogb:isbn
                /// </summary>
                public static readonly RDFResource ISBN = new RDFResource(OG_BOOK.BASE_URI + "isbn");

                /// <summary>
                /// ogb:release_date
                /// </summary>
                public static readonly RDFResource RELEASE_DATE = new RDFResource(OG_BOOK.BASE_URI + "release_date");

                /// <summary>
                /// ogb:tag
                /// </summary>
                public static readonly RDFResource TAG = new RDFResource(OG_BOOK.BASE_URI + "tag");
                #endregion

            }
            #endregion

            #region OG_PROFILE
            /// <summary>
            /// OG_PROFILE represents profile-related concepts in Open Graph Protocol Vocabulary
            /// </summary>
            public static class OG_PROFILE {

                #region Properties
                /// <summary>
                /// ogp
                /// </summary>
                public static readonly String PREFIX = "ogp";

                /// <summary>
                /// http://ogp.me/ns/profile#
                /// </summary>
                public static readonly String BASE_URI = "http://ogp.me/ns/profile#";

                /// <summary>
                /// ogp:first_name
                /// </summary>
                public static readonly RDFResource FIRST_NAME = new RDFResource(OG_PROFILE.BASE_URI + "first_name");

                /// <summary>
                /// ogp:last_name
                /// </summary>
                public static readonly RDFResource LAST_NAME = new RDFResource(OG_PROFILE.BASE_URI + "last_name");

                /// <summary>
                /// ogp:username
                /// </summary>
                public static readonly RDFResource USERNAME = new RDFResource(OG_PROFILE.BASE_URI + "username");

                /// <summary>
                /// ogp:gender
                /// </summary>
                public static readonly RDFResource GENDER = new RDFResource(OG_PROFILE.BASE_URI + "gender");
                #endregion

            }
            #endregion

            #region OG_WEBSITE
            /// <summary>
            /// OG_WEBSITE represents website-related concepts in Open Graph Protocol Vocabulary
            /// </summary>
            public static class OG_WEBSITE {

                #region Properties
                /// <summary>
                /// ogw
                /// </summary>
                public static readonly String PREFIX = "ogw";

                /// <summary>
                /// http://ogp.me/ns/website#
                /// </summary>
                public static readonly String BASE_URI = "http://ogp.me/ns/website#";
                #endregion

            }
            #endregion

            #endregion

        }
        #endregion

        #endregion

    }

}