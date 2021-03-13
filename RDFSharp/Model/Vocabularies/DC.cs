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
        #region DC
        /// <summary>
        /// DC represents the Dublin Core vocabulary (with DCAM, DCTERMS and DCTYPE extensions).
        /// </summary>
        public static class DC
        {

            #region Properties
            /// <summary>
            /// dc
            /// </summary>
            public static readonly string PREFIX = "dc";

            /// <summary>
            /// http://purl.org/dc/elements/1.1/
            /// </summary>
            public static readonly string BASE_URI = "http://purl.org/dc/elements/1.1/";

            /// <summary>
            /// https://www.dublincore.org/specifications/dublin-core/dcmi-terms/dublin_core_elements.rdf
            /// </summary>
            public static readonly string DEREFERENCE_URI = "https://www.dublincore.org/specifications/dublin-core/dcmi-terms/dublin_core_elements.rdf";

            /// <summary>
            /// dc:contributor
            /// </summary>
            public static readonly RDFResource CONTRIBUTOR = new RDFResource(string.Concat(DC.BASE_URI, "contributor"));

            /// <summary>
            /// dc:coverage
            /// </summary>
            public static readonly RDFResource COVERAGE = new RDFResource(string.Concat(DC.BASE_URI, "coverage"));

            /// <summary>
            /// dc:creator
            /// </summary>
            public static readonly RDFResource CREATOR = new RDFResource(string.Concat(DC.BASE_URI, "creator"));

            /// <summary>
            /// dc:date
            /// </summary>
            public static readonly RDFResource DATE = new RDFResource(string.Concat(DC.BASE_URI, "date"));

            /// <summary>
            /// dc:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(string.Concat(DC.BASE_URI, "description"));

            /// <summary>
            /// dc:format
            /// </summary>
            public static readonly RDFResource FORMAT = new RDFResource(string.Concat(DC.BASE_URI, "format"));

            /// <summary>
            /// dc:identifier
            /// </summary>
            public static readonly RDFResource IDENTIFIER = new RDFResource(string.Concat(DC.BASE_URI, "identifier"));

            /// <summary>
            /// dc:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource(string.Concat(DC.BASE_URI, "language"));

            /// <summary>
            /// dc:publisher
            /// </summary>
            public static readonly RDFResource PUBLISHER = new RDFResource(string.Concat(DC.BASE_URI, "publisher"));

            /// <summary>
            /// dc:relation
            /// </summary>
            public static readonly RDFResource RELATION = new RDFResource(string.Concat(DC.BASE_URI, "relation"));

            /// <summary>
            /// dc:rights
            /// </summary>
            public static readonly RDFResource RIGHTS = new RDFResource(string.Concat(DC.BASE_URI, "rights"));

            /// <summary>
            /// dc:source
            /// </summary>
            public static readonly RDFResource SOURCE = new RDFResource(string.Concat(DC.BASE_URI, "source"));

            /// <summary>
            /// dc:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(string.Concat(DC.BASE_URI, "subject"));

            /// <summary>
            /// dc:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource(string.Concat(DC.BASE_URI, "title"));

            /// <summary>
            /// dc:type
            /// </summary>
            public static readonly RDFResource TYPE = new RDFResource(string.Concat(DC.BASE_URI, "type"));
            #endregion

            #region Extended Properties

            #region DCAM
            /// <summary>
            /// DCAM extensions
            /// </summary>
            public static class DCAM
            {

                #region Properties
                /// <summary>
                /// dcam
                /// </summary>
                public static readonly string PREFIX = "dcam";

                /// <summary>
                /// http://purl.org/dc/dcam/
                /// </summary>
                public static readonly string BASE_URI = "http://purl.org/dc/dcam/";

                /// <summary>
                /// https://www.dublincore.org/specifications/dublin-core/dcmi-terms/dublin_core_abstract_model.rdf
                /// </summary>
                public static readonly string DEREFERENCE_URI = "https://www.dublincore.org/specifications/dublin-core/dcmi-terms/dublin_core_abstract_model.rdf";

                /// <summary>
                /// dcam:memberOf
                /// </summary>
                public static readonly RDFResource MEMBER_OF = new RDFResource(string.Concat(DCAM.BASE_URI, "memberOf"));

                /// <summary>
                /// dcam:VocabularyEncodingScheme
                /// </summary>
                public static readonly RDFResource VOCABULARY_ENCODING_SCHEME = new RDFResource(string.Concat(DCAM.BASE_URI, "VocabularyEncodingScheme"));
                #endregion

            }
            #endregion

            #region DCTERMS
            /// <summary>
            /// DCTERMS extensions
            /// </summary>
            public static class DCTERMS
            {

                #region Properties
                /// <summary>
                /// dcterms
                /// </summary>
                public static readonly string PREFIX = "dcterms";

                /// <summary>
                /// http://purl.org/dc/terms/
                /// </summary>
                public static readonly string BASE_URI = "http://purl.org/dc/terms/";

                /// <summary>
                /// https://www.dublincore.org/specifications/dublin-core/dcmi-terms/dublin_core_terms.rdf
                /// </summary>
                public static readonly string DEREFERENCE_URI = "https://www.dublincore.org/specifications/dublin-core/dcmi-terms/dublin_core_terms.rdf";

                /// <summary>
                /// dcterms:abstract
                /// </summary>
                public static readonly RDFResource ABSTRACT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "abstract"));

                /// <summary>
                /// dcterms:accessRights
                /// </summary>
                public static readonly RDFResource ACCESS_RIGHTS = new RDFResource(string.Concat(DCTERMS.BASE_URI, "accessRights"));

                /// <summary>
                /// dcterms:accrualMethod
                /// </summary>
                public static readonly RDFResource ACCRUAL_METHOD = new RDFResource(string.Concat(DCTERMS.BASE_URI, "accrualMethod"));

                /// <summary>
                /// dcterms:accrualPeriodicity
                /// </summary>
                public static readonly RDFResource ACCRUAL_PERIODICITY = new RDFResource(string.Concat(DCTERMS.BASE_URI, "accrualPeriodicity"));

                /// <summary>
                /// dcterms:accrualPolicy
                /// </summary>
                public static readonly RDFResource ACCRUAL_POLICY = new RDFResource(string.Concat(DCTERMS.BASE_URI, "accrualPolicy"));

                /// <summary>
                /// dcterms:Agent
                /// </summary>
                public static readonly RDFResource AGENT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Agent"));

                /// <summary>
                /// dcterms:AgentClass
                /// </summary>
                public static readonly RDFResource AGENT_CLASS = new RDFResource(string.Concat(DCTERMS.BASE_URI, "AgentClass"));

                /// <summary>
                /// dcterms:alternative
                /// </summary>
                public static readonly RDFResource ALTERNATIVE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "alternative"));

                /// <summary>
                /// dcterms:audience
                /// </summary>
                public static readonly RDFResource AUDIENCE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "audience"));

                /// <summary>
                /// dcterms:available
                /// </summary>
                public static readonly RDFResource AVAILABLE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "available"));

                /// <summary>
                /// dcterms:bibliographicCitation
                /// </summary>
                public static readonly RDFResource BIBLIOGRAPHIC_CITATION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "bibliographicCitation"));

                /// <summary>
                /// dcterms:BibliographicResource
                /// </summary>
                public static readonly RDFResource BIBLIOGRAPHIC_RESOURCE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "BibliographicResource"));

                /// <summary>
                /// dcterms:conformsTo
                /// </summary>
                public static readonly RDFResource CONFORMS_TO = new RDFResource(string.Concat(DCTERMS.BASE_URI, "conformsTo"));

                /// <summary>
                /// dcterms:contributor
                /// </summary>
                public static readonly RDFResource CONTRIBUTOR = new RDFResource(string.Concat(DCTERMS.BASE_URI, "contributor"));

                /// <summary>
                /// dcterms:coverage
                /// </summary>
                public static readonly RDFResource COVERAGE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "coverage"));

                /// <summary>
                /// dcterms:created
                /// </summary>
                public static readonly RDFResource CREATED = new RDFResource(string.Concat(DCTERMS.BASE_URI, "created"));

                /// <summary>
                /// dcterms:creator
                /// </summary>
                public static readonly RDFResource CREATOR = new RDFResource(string.Concat(DCTERMS.BASE_URI, "creator"));

                /// <summary>
                /// dcterms:date
                /// </summary>
                public static readonly RDFResource DATE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "date"));

                /// <summary>
                /// dcterms:dateAccepted
                /// </summary>
                public static readonly RDFResource DATE_ACCEPTED = new RDFResource(string.Concat(DCTERMS.BASE_URI, "dateAccepted"));

                /// <summary>
                /// dcterms:dateCopyrighted
                /// </summary>
                public static readonly RDFResource DATE_COPYRIGHTED = new RDFResource(string.Concat(DCTERMS.BASE_URI, "dateCopyrighted"));

                /// <summary>
                /// dcterms:dateSubmitted
                /// </summary>
                public static readonly RDFResource DATE_SUBMITTED = new RDFResource(string.Concat(DCTERMS.BASE_URI, "dateSubmitted"));

                /// <summary>
                /// dcterms:description
                /// </summary>
                public static readonly RDFResource DESCRIPTION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "description"));

                /// <summary>
                /// dcterms:educationLevel
                /// </summary>
                public static readonly RDFResource EDUCATION_LEVEL = new RDFResource(string.Concat(DCTERMS.BASE_URI, "educationLevel"));

                /// <summary>
                /// dcterms:extent
                /// </summary>
                public static readonly RDFResource EXTENT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "extent"));

                /// <summary>
                /// dcterms:FileFormat
                /// </summary>
                public static readonly RDFResource FILE_FORMAT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "FileFormat"));

                /// <summary>
                /// dcterms:format
                /// </summary>
                public static readonly RDFResource FORMAT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "format"));

                /// <summary>
                /// dcterms:Frequency
                /// </summary>
                public static readonly RDFResource FREQUENCY = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Frequency"));

                /// <summary>
                /// dcterms:hasFormat
                /// </summary>
                public static readonly RDFResource HAS_FORMAT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "hasFormat"));

                /// <summary>
                /// dcterms:hasPart
                /// </summary>
                public static readonly RDFResource HAS_PART = new RDFResource(string.Concat(DCTERMS.BASE_URI, "hasPart"));

                /// <summary>
                /// dcterms:hasVersion
                /// </summary>
                public static readonly RDFResource HAS_VERSION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "hasVersion"));

                /// <summary>
                /// dcterms:identifier
                /// </summary>
                public static readonly RDFResource IDENTIFIER = new RDFResource(string.Concat(DCTERMS.BASE_URI, "identifier"));

                /// <summary>
                /// dcterms:instructionalMethod
                /// </summary>
                public static readonly RDFResource INSTRUCTIONAL_METHOD = new RDFResource(string.Concat(DCTERMS.BASE_URI, "instructionalMethod"));

                /// <summary>
                /// dcterms:isFormatOf
                /// </summary>
                public static readonly RDFResource IS_FORMAT_OF = new RDFResource(string.Concat(DCTERMS.BASE_URI, "isFormatOf"));

                /// <summary>
                /// dcterms:isPartOf
                /// </summary>
                public static readonly RDFResource IS_PART_OF = new RDFResource(string.Concat(DCTERMS.BASE_URI, "isPartOf"));

                /// <summary>
                /// dcterms:isReferencedBy
                /// </summary>
                public static readonly RDFResource IS_REFERENCED_BY = new RDFResource(string.Concat(DCTERMS.BASE_URI, "isReferencedBy"));

                /// <summary>
                /// dcterms:isReplacedBy
                /// </summary>
                public static readonly RDFResource IS_REPLACED_BY = new RDFResource(string.Concat(DCTERMS.BASE_URI, "isReplacedBy"));

                /// <summary>
                /// dcterms:isRequiredBy
                /// </summary>
                public static readonly RDFResource IS_REQUIRED_BY = new RDFResource(string.Concat(DCTERMS.BASE_URI, "isRequiredBy"));

                /// <summary>
                /// dcterms:issued
                /// </summary>
                public static readonly RDFResource ISSUED = new RDFResource(string.Concat(DCTERMS.BASE_URI, "issued"));

                /// <summary>
                /// dcterms:isVersionOf
                /// </summary>
                public static readonly RDFResource IS_VERSION_OF = new RDFResource(string.Concat(DCTERMS.BASE_URI, "isVersionOf"));

                /// <summary>
                /// dcterms:Jurisdiction
                /// </summary>
                public static readonly RDFResource JURISDICTION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Jurisdiction"));

                /// <summary>
                /// dcterms:language
                /// </summary>
                public static readonly RDFResource LANGUAGE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "language"));

                /// <summary>
                /// dcterms:license
                /// </summary>
                public static readonly RDFResource LICENSE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "license"));

                /// <summary>
                /// dcterms:LicenseDocument
                /// </summary>
                public static readonly RDFResource LICENSE_DOCUMENT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "LicenseDocument"));

                /// <summary>
                /// dcterms:LinguisticSystem
                /// </summary>
                public static readonly RDFResource LINGUISTIC_SYSTEM = new RDFResource(string.Concat(DCTERMS.BASE_URI, "LinguisticSystem"));

                /// <summary>
                /// dcterms:Location
                /// </summary>
                public static readonly RDFResource LOCATION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Location"));

                /// <summary>
                /// dcterms:LocationPeriodOrJurisdiction
                /// </summary>
                public static readonly RDFResource LOCATION_PERIOD_OR_JURISDICTION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "LocationPeriodOrJurisdiction"));

                /// <summary>
                /// dcterms:mediator
                /// </summary>
                public static readonly RDFResource MEDIATOR = new RDFResource(string.Concat(DCTERMS.BASE_URI, "mediator"));

                /// <summary>
                /// dcterms:MediaType
                /// </summary>
                public static readonly RDFResource MEDIA_TYPE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "MediaType"));

                /// <summary>
                /// dcterms:MediaTypeOrExtent
                /// </summary>
                public static readonly RDFResource MEDIA_TYPE_OR_EXTENT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "MediaTypeOrExtent"));

                /// <summary>
                /// dcterms:medium
                /// </summary>
                public static readonly RDFResource MEDIUM = new RDFResource(string.Concat(DCTERMS.BASE_URI, "medium"));

                /// <summary>
                /// dcterms:MethodOfAccrual
                /// </summary>
                public static readonly RDFResource METHOD_OF_ACCRUAL = new RDFResource(string.Concat(DCTERMS.BASE_URI, "MethodOfAccrual"));

                /// <summary>
                /// dcterms:MethodOfInstruction
                /// </summary>
                public static readonly RDFResource METHOD_OF_INSTRUCTION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "MethodOfInstruction"));

                /// <summary>
                /// dcterms:modified
                /// </summary>
                public static readonly RDFResource MODIFIED = new RDFResource(string.Concat(DCTERMS.BASE_URI, "modified"));

                /// <summary>
                /// dcterms:PeriodOfTime
                /// </summary>
                public static readonly RDFResource PERIOD_OF_TIME = new RDFResource(string.Concat(DCTERMS.BASE_URI, "PeriodOfTime"));

                /// <summary>
                /// dcterms:PhysicalMedium
                /// </summary>
                public static readonly RDFResource PHYSICAL_MEDIUM = new RDFResource(string.Concat(DCTERMS.BASE_URI, "PhysicalMedium"));

                /// <summary>
                /// dcterms:PhysicalResource
                /// </summary>
                public static readonly RDFResource PHYSICAL_RESOURCE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "PhysicalResource"));

                /// <summary>
                /// dcterms:Policy
                /// </summary>
                public static readonly RDFResource POLICY = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Policy"));

                /// <summary>
                /// dcterms:provenance
                /// </summary>
                public static readonly RDFResource PROVENANCE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "provenance"));

                /// <summary>
                /// dcterms:ProvenanceStatement
                /// </summary>
                public static readonly RDFResource PROVENANCE_STATEMENT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "ProvenanceStatement"));

                /// <summary>
                /// dcterms:publisher
                /// </summary>
                public static readonly RDFResource PUBLISHER = new RDFResource(string.Concat(DCTERMS.BASE_URI, "publisher"));

                /// <summary>
                /// dcterms:references
                /// </summary>
                public static readonly RDFResource REFERENCES = new RDFResource(string.Concat(DCTERMS.BASE_URI, "references"));

                /// <summary>
                /// dcterms:relation
                /// </summary>
                public static readonly RDFResource RELATION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "relation"));

                /// <summary>
                /// dcterms:replaces
                /// </summary>
                public static readonly RDFResource REPLACES = new RDFResource(string.Concat(DCTERMS.BASE_URI, "replaces"));

                /// <summary>
                /// dcterms:requires
                /// </summary>
                public static readonly RDFResource REQUIRES = new RDFResource(string.Concat(DCTERMS.BASE_URI, "requires"));

                /// <summary>
                /// dcterms:rights
                /// </summary>
                public static readonly RDFResource RIGHTS = new RDFResource(string.Concat(DCTERMS.BASE_URI, "rights"));

                /// <summary>
                /// dcterms:RightsStatement
                /// </summary>
                public static readonly RDFResource RIGHTS_STATEMENT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "RightsStatement"));

                /// <summary>
                /// dcterms:rightsHolder
                /// </summary>
                public static readonly RDFResource RIGHTS_HOLDER = new RDFResource(string.Concat(DCTERMS.BASE_URI, "rightsHolder"));

                /// <summary>
                /// dcterms:SizeOrDuration
                /// </summary>
                public static readonly RDFResource SIZE_OR_DURATION = new RDFResource(string.Concat(DCTERMS.BASE_URI, "SizeOrDuration"));

                /// <summary>
                /// dcterms:source
                /// </summary>
                public static readonly RDFResource SOURCE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "source"));

                /// <summary>
                /// dcterms:spatial
                /// </summary>
                public static readonly RDFResource SPATIAL = new RDFResource(string.Concat(DCTERMS.BASE_URI, "spatial"));

                /// <summary>
                /// dcterms:Standard
                /// </summary>
                public static readonly RDFResource STANDARD = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Standard"));

                /// <summary>
                /// dcterms:subject
                /// </summary>
                public static readonly RDFResource SUBJECT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "subject"));

                /// <summary>
                /// dcterms:tableOfContents
                /// </summary>
                public static readonly RDFResource TABLE_OF_CONTENTS = new RDFResource(string.Concat(DCTERMS.BASE_URI, "tableOfContents"));

                /// <summary>
                /// dcterms:temporal
                /// </summary>
                public static readonly RDFResource TEMPORAL = new RDFResource(string.Concat(DCTERMS.BASE_URI, "temporal"));

                /// <summary>
                /// dcterms:title
                /// </summary>
                public static readonly RDFResource TITLE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "title"));

                /// <summary>
                /// dcterms:type
                /// </summary>
                public static readonly RDFResource TYPE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "type"));

                /// <summary>
                /// dcterms:valid
                /// </summary>
                public static readonly RDFResource VALID = new RDFResource(string.Concat(DCTERMS.BASE_URI, "valid"));

                #region Vocabulary Encoding Schemes
                /// <summary>
                /// dcterms:DCMIType
                /// </summary>
                public static readonly RDFResource DCMITYPE = new RDFResource(string.Concat(DCTERMS.BASE_URI, "DCMIType"));

                /// <summary>
                /// dcterms:DDC
                /// </summary>
                public static readonly RDFResource DDC = new RDFResource(string.Concat(DCTERMS.BASE_URI, "DDC"));

                /// <summary>
                /// dcterms:IMT
                /// </summary>
                public static readonly RDFResource IMT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "IMT"));

                /// <summary>
                /// dcterms:LCC
                /// </summary>
                public static readonly RDFResource LCC = new RDFResource(string.Concat(DCTERMS.BASE_URI, "LCC"));

                /// <summary>
                /// dcterms:LCSH
                /// </summary>
                public static readonly RDFResource LCSH = new RDFResource(string.Concat(DCTERMS.BASE_URI, "LCSH"));

                /// <summary>
                /// dcterms:MESH
                /// </summary>
                public static readonly RDFResource MESH = new RDFResource(string.Concat(DCTERMS.BASE_URI, "MESH"));

                /// <summary>
                /// dcterms:NLM
                /// </summary>
                public static readonly RDFResource NLM = new RDFResource(string.Concat(DCTERMS.BASE_URI, "NLM"));

                /// <summary>
                /// dcterms:TGN
                /// </summary>
                public static readonly RDFResource TGN = new RDFResource(string.Concat(DCTERMS.BASE_URI, "TGN"));

                /// <summary>
                /// dcterms:UDC
                /// </summary>
                public static readonly RDFResource UDC = new RDFResource(string.Concat(DCTERMS.BASE_URI, "UDC"));
                #endregion

                #region Syntax Encoding Schemes
                /// <summary>
                /// dcterms:Box
                /// </summary>
                public static readonly RDFResource BOX = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Box"));

                /// <summary>
                /// dcterms:ISO3166
                /// </summary>
                public static readonly RDFResource ISO3166 = new RDFResource(string.Concat(DCTERMS.BASE_URI, "ISO3166"));

                /// <summary>
                /// dcterms:ISO639-2
                /// </summary>
                public static readonly RDFResource ISO639_2 = new RDFResource(string.Concat(DCTERMS.BASE_URI, "ISO639-2"));

                /// <summary>
                /// dcterms:ISO639-3
                /// </summary>
                public static readonly RDFResource ISO639_3 = new RDFResource(string.Concat(DCTERMS.BASE_URI, "ISO639-3"));

                /// <summary>
                /// dcterms:Period
                /// </summary>
                public static readonly RDFResource PERIOD = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Period"));

                /// <summary>
                /// dcterms:Point
                /// </summary>
                public static readonly RDFResource POINT = new RDFResource(string.Concat(DCTERMS.BASE_URI, "Point"));

                /// <summary>
                /// dcterms:RFC1766
                /// </summary>
                public static readonly RDFResource RFC1766 = new RDFResource(string.Concat(DCTERMS.BASE_URI, "RFC1766"));

                /// <summary>
                /// dcterms:RFC3066
                /// </summary>
                public static readonly RDFResource RFC3066 = new RDFResource(string.Concat(DCTERMS.BASE_URI, "RFC3066"));

                /// <summary>
                /// dcterms:RFC4646
                /// </summary>
                public static readonly RDFResource RFC4646 = new RDFResource(string.Concat(DCTERMS.BASE_URI, "RFC4646"));

                /// <summary>
                /// dcterms:RFC5646
                /// </summary>
                public static readonly RDFResource RFC5646 = new RDFResource(string.Concat(DCTERMS.BASE_URI, "RFC5646"));

                /// <summary>
                /// dcterms:URI
                /// </summary>
                public static readonly RDFResource URI = new RDFResource(string.Concat(DCTERMS.BASE_URI, "URI"));

                /// <summary>
                /// dcterms:W3CDTF
                /// </summary>
                public static readonly RDFResource W3CDTF = new RDFResource(string.Concat(DCTERMS.BASE_URI, "W3CDTF"));
                #endregion

                #endregion

            }
            #endregion

            #region DCTYPE
            /// <summary>
            /// DCTYPE extensions
            /// </summary>
            public static class DCTYPE
            {

                #region Properties
                /// <summary>
                /// dctype
                /// </summary>
                public static readonly string PREFIX = "dctype";

                /// <summary>
                /// http://purl.org/dc/dcmitype/
                /// </summary>
                public static readonly string BASE_URI = "http://purl.org/dc/dcmitype/";

                /// <summary>
                /// https://www.dublincore.org/specifications/dublin-core/dcmi-terms/dublin_core_type.rdf
                /// </summary>
                public static readonly string DEREFERENCE_URI = "https://www.dublincore.org/specifications/dublin-core/dcmi-terms/dublin_core_type.rdf";

                /// <summary>
                /// dctype:Collection
                /// </summary>
                public static readonly RDFResource COLLECTION = new RDFResource(string.Concat(DCTYPE.BASE_URI, "Collection"));

                /// <summary>
                /// dctype:Dataset
                /// </summary>
                public static readonly RDFResource DATASET = new RDFResource(string.Concat(DCTYPE.BASE_URI, "Dataset"));

                /// <summary>
                /// dctype:Event
                /// </summary>
                public static readonly RDFResource EVENT = new RDFResource(string.Concat(DCTYPE.BASE_URI, "Event"));

                /// <summary>
                /// dctype:Image
                /// </summary>
                public static readonly RDFResource IMAGE = new RDFResource(string.Concat(DCTYPE.BASE_URI, "Image"));

                /// <summary>
                /// dctype:InteractiveResource
                /// </summary>
                public static readonly RDFResource INTERACTIVE_RESOURCE = new RDFResource(string.Concat(DCTYPE.BASE_URI, "InteractiveResource"));

                /// <summary>
                /// dctype:MovingImage
                /// </summary>
                public static readonly RDFResource MOVING_IMAGE = new RDFResource(string.Concat(DCTYPE.BASE_URI, "MovingImage"));

                /// <summary>
                /// dctype:PhysicalObject
                /// </summary>
                public static readonly RDFResource PHYSICAL_OBJECT = new RDFResource(string.Concat(DCTYPE.BASE_URI, "PhysicalObject"));

                /// <summary>
                /// dctype:Service
                /// </summary>
                public static readonly RDFResource SERVICE = new RDFResource(string.Concat(DCTYPE.BASE_URI, "Service"));

                /// <summary>
                /// dctype:Software
                /// </summary>
                public static readonly RDFResource SOFTWARE = new RDFResource(string.Concat(DCTYPE.BASE_URI, "Software"));

                /// <summary>
                /// dctype:Sound
                /// </summary>
                public static readonly RDFResource SOUND = new RDFResource(string.Concat(DCTYPE.BASE_URI, "Sound"));

                /// <summary>
                /// dctype:StillImage
                /// </summary>
                public static readonly RDFResource STILL_IMAGE = new RDFResource(string.Concat(DCTYPE.BASE_URI, "StillImage"));

                /// <summary>
                /// dctype:Text
                /// </summary>
                public static readonly RDFResource TEXT = new RDFResource(string.Concat(DCTYPE.BASE_URI, "Text"));
                #endregion

            }
            #endregion

            #endregion

        }
        #endregion
    }
}