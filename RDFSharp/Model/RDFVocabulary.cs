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

using System.Diagnostics.CodeAnalysis;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFVocabulary is an helper for handy usage of supported LinkedData vocabularies
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class RDFVocabulary
    {
        #region DC
        /// <summary>
        /// DC represents the Dublin Core vocabulary (with DCAM, DCTERMS and DCTYPE extensions)
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
            public static readonly RDFResource CONTRIBUTOR = new RDFResource($"{BASE_URI}contributor");

            /// <summary>
            /// dc:coverage
            /// </summary>
            public static readonly RDFResource COVERAGE = new RDFResource($"{BASE_URI}coverage");

            /// <summary>
            /// dc:creator
            /// </summary>
            public static readonly RDFResource CREATOR = new RDFResource($"{BASE_URI}creator");

            /// <summary>
            /// dc:date
            /// </summary>
            public static readonly RDFResource DATE = new RDFResource($"{BASE_URI}date");

            /// <summary>
            /// dc:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource($"{BASE_URI}description");

            /// <summary>
            /// dc:format
            /// </summary>
            public static readonly RDFResource FORMAT = new RDFResource($"{BASE_URI}format");

            /// <summary>
            /// dc:identifier
            /// </summary>
            public static readonly RDFResource IDENTIFIER = new RDFResource($"{BASE_URI}identifier");

            /// <summary>
            /// dc:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource($"{BASE_URI}language");

            /// <summary>
            /// dc:publisher
            /// </summary>
            public static readonly RDFResource PUBLISHER = new RDFResource($"{BASE_URI}publisher");

            /// <summary>
            /// dc:relation
            /// </summary>
            public static readonly RDFResource RELATION = new RDFResource($"{BASE_URI}relation");

            /// <summary>
            /// dc:rights
            /// </summary>
            public static readonly RDFResource RIGHTS = new RDFResource($"{BASE_URI}rights");

            /// <summary>
            /// dc:source
            /// </summary>
            public static readonly RDFResource SOURCE = new RDFResource($"{BASE_URI}source");

            /// <summary>
            /// dc:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource($"{BASE_URI}subject");

            /// <summary>
            /// dc:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource($"{BASE_URI}title");

            /// <summary>
            /// dc:type
            /// </summary>
            public static readonly RDFResource TYPE = new RDFResource($"{BASE_URI}type");
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
                public static readonly RDFResource MEMBER_OF = new RDFResource($"{BASE_URI}memberOf");

                /// <summary>
                /// dcam:VocabularyEncodingScheme
                /// </summary>
                public static readonly RDFResource VOCABULARY_ENCODING_SCHEME = new RDFResource($"{BASE_URI}VocabularyEncodingScheme");
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
                public static readonly RDFResource ABSTRACT = new RDFResource($"{BASE_URI}abstract");

                /// <summary>
                /// dcterms:accessRights
                /// </summary>
                public static readonly RDFResource ACCESS_RIGHTS = new RDFResource($"{BASE_URI}accessRights");

                /// <summary>
                /// dcterms:accrualMethod
                /// </summary>
                public static readonly RDFResource ACCRUAL_METHOD = new RDFResource($"{BASE_URI}accrualMethod");

                /// <summary>
                /// dcterms:accrualPeriodicity
                /// </summary>
                public static readonly RDFResource ACCRUAL_PERIODICITY = new RDFResource($"{BASE_URI}accrualPeriodicity");

                /// <summary>
                /// dcterms:accrualPolicy
                /// </summary>
                public static readonly RDFResource ACCRUAL_POLICY = new RDFResource($"{BASE_URI}accrualPolicy");

                /// <summary>
                /// dcterms:Agent
                /// </summary>
                public static readonly RDFResource AGENT = new RDFResource($"{BASE_URI}Agent");

                /// <summary>
                /// dcterms:AgentClass
                /// </summary>
                public static readonly RDFResource AGENT_CLASS = new RDFResource($"{BASE_URI}AgentClass");

                /// <summary>
                /// dcterms:alternative
                /// </summary>
                public static readonly RDFResource ALTERNATIVE = new RDFResource($"{BASE_URI}alternative");

                /// <summary>
                /// dcterms:audience
                /// </summary>
                public static readonly RDFResource AUDIENCE = new RDFResource($"{BASE_URI}audience");

                /// <summary>
                /// dcterms:available
                /// </summary>
                public static readonly RDFResource AVAILABLE = new RDFResource($"{BASE_URI}available");

                /// <summary>
                /// dcterms:bibliographicCitation
                /// </summary>
                public static readonly RDFResource BIBLIOGRAPHIC_CITATION = new RDFResource($"{BASE_URI}bibliographicCitation");

                /// <summary>
                /// dcterms:BibliographicResource
                /// </summary>
                public static readonly RDFResource BIBLIOGRAPHIC_RESOURCE = new RDFResource($"{BASE_URI}BibliographicResource");

                /// <summary>
                /// dcterms:conformsTo
                /// </summary>
                public static readonly RDFResource CONFORMS_TO = new RDFResource($"{BASE_URI}conformsTo");

                /// <summary>
                /// dcterms:contributor
                /// </summary>
                public static readonly RDFResource CONTRIBUTOR = new RDFResource($"{BASE_URI}contributor");

                /// <summary>
                /// dcterms:coverage
                /// </summary>
                public static readonly RDFResource COVERAGE = new RDFResource($"{BASE_URI}coverage");

                /// <summary>
                /// dcterms:created
                /// </summary>
                public static readonly RDFResource CREATED = new RDFResource($"{BASE_URI}created");

                /// <summary>
                /// dcterms:creator
                /// </summary>
                public static readonly RDFResource CREATOR = new RDFResource($"{BASE_URI}creator");

                /// <summary>
                /// dcterms:date
                /// </summary>
                public static readonly RDFResource DATE = new RDFResource($"{BASE_URI}date");

                /// <summary>
                /// dcterms:dateAccepted
                /// </summary>
                public static readonly RDFResource DATE_ACCEPTED = new RDFResource($"{BASE_URI}dateAccepted");

                /// <summary>
                /// dcterms:dateCopyrighted
                /// </summary>
                public static readonly RDFResource DATE_COPYRIGHTED = new RDFResource($"{BASE_URI}dateCopyrighted");

                /// <summary>
                /// dcterms:dateSubmitted
                /// </summary>
                public static readonly RDFResource DATE_SUBMITTED = new RDFResource($"{BASE_URI}dateSubmitted");

                /// <summary>
                /// dcterms:description
                /// </summary>
                public static readonly RDFResource DESCRIPTION = new RDFResource($"{BASE_URI}description");

                /// <summary>
                /// dcterms:educationLevel
                /// </summary>
                public static readonly RDFResource EDUCATION_LEVEL = new RDFResource($"{BASE_URI}educationLevel");

                /// <summary>
                /// dcterms:extent
                /// </summary>
                public static readonly RDFResource EXTENT = new RDFResource($"{BASE_URI}extent");

                /// <summary>
                /// dcterms:FileFormat
                /// </summary>
                public static readonly RDFResource FILE_FORMAT = new RDFResource($"{BASE_URI}FileFormat");

                /// <summary>
                /// dcterms:format
                /// </summary>
                public static readonly RDFResource FORMAT = new RDFResource($"{BASE_URI}format");

                /// <summary>
                /// dcterms:Frequency
                /// </summary>
                public static readonly RDFResource FREQUENCY = new RDFResource($"{BASE_URI}Frequency");

                /// <summary>
                /// dcterms:hasFormat
                /// </summary>
                public static readonly RDFResource HAS_FORMAT = new RDFResource($"{BASE_URI}hasFormat");

                /// <summary>
                /// dcterms:hasPart
                /// </summary>
                public static readonly RDFResource HAS_PART = new RDFResource($"{BASE_URI}hasPart");

                /// <summary>
                /// dcterms:hasVersion
                /// </summary>
                public static readonly RDFResource HAS_VERSION = new RDFResource($"{BASE_URI}hasVersion");

                /// <summary>
                /// dcterms:identifier
                /// </summary>
                public static readonly RDFResource IDENTIFIER = new RDFResource($"{BASE_URI}identifier");

                /// <summary>
                /// dcterms:instructionalMethod
                /// </summary>
                public static readonly RDFResource INSTRUCTIONAL_METHOD = new RDFResource($"{BASE_URI}instructionalMethod");

                /// <summary>
                /// dcterms:isFormatOf
                /// </summary>
                public static readonly RDFResource IS_FORMAT_OF = new RDFResource($"{BASE_URI}isFormatOf");

                /// <summary>
                /// dcterms:isPartOf
                /// </summary>
                public static readonly RDFResource IS_PART_OF = new RDFResource($"{BASE_URI}isPartOf");

                /// <summary>
                /// dcterms:isReferencedBy
                /// </summary>
                public static readonly RDFResource IS_REFERENCED_BY = new RDFResource($"{BASE_URI}isReferencedBy");

                /// <summary>
                /// dcterms:isReplacedBy
                /// </summary>
                public static readonly RDFResource IS_REPLACED_BY = new RDFResource($"{BASE_URI}isReplacedBy");

                /// <summary>
                /// dcterms:isRequiredBy
                /// </summary>
                public static readonly RDFResource IS_REQUIRED_BY = new RDFResource($"{BASE_URI}isRequiredBy");

                /// <summary>
                /// dcterms:issued
                /// </summary>
                public static readonly RDFResource ISSUED = new RDFResource($"{BASE_URI}issued");

                /// <summary>
                /// dcterms:isVersionOf
                /// </summary>
                public static readonly RDFResource IS_VERSION_OF = new RDFResource($"{BASE_URI}isVersionOf");

                /// <summary>
                /// dcterms:Jurisdiction
                /// </summary>
                public static readonly RDFResource JURISDICTION = new RDFResource($"{BASE_URI}Jurisdiction");

                /// <summary>
                /// dcterms:language
                /// </summary>
                public static readonly RDFResource LANGUAGE = new RDFResource($"{BASE_URI}language");

                /// <summary>
                /// dcterms:license
                /// </summary>
                public static readonly RDFResource LICENSE = new RDFResource($"{BASE_URI}license");

                /// <summary>
                /// dcterms:LicenseDocument
                /// </summary>
                public static readonly RDFResource LICENSE_DOCUMENT = new RDFResource($"{BASE_URI}LicenseDocument");

                /// <summary>
                /// dcterms:LinguisticSystem
                /// </summary>
                public static readonly RDFResource LINGUISTIC_SYSTEM = new RDFResource($"{BASE_URI}LinguisticSystem");

                /// <summary>
                /// dcterms:Location
                /// </summary>
                public static readonly RDFResource LOCATION = new RDFResource($"{BASE_URI}Location");

                /// <summary>
                /// dcterms:LocationPeriodOrJurisdiction
                /// </summary>
                public static readonly RDFResource LOCATION_PERIOD_OR_JURISDICTION = new RDFResource($"{BASE_URI}LocationPeriodOrJurisdiction");

                /// <summary>
                /// dcterms:mediator
                /// </summary>
                public static readonly RDFResource MEDIATOR = new RDFResource($"{BASE_URI}mediator");

                /// <summary>
                /// dcterms:MediaType
                /// </summary>
                public static readonly RDFResource MEDIA_TYPE = new RDFResource($"{BASE_URI}MediaType");

                /// <summary>
                /// dcterms:MediaTypeOrExtent
                /// </summary>
                public static readonly RDFResource MEDIA_TYPE_OR_EXTENT = new RDFResource($"{BASE_URI}MediaTypeOrExtent");

                /// <summary>
                /// dcterms:medium
                /// </summary>
                public static readonly RDFResource MEDIUM = new RDFResource($"{BASE_URI}medium");

                /// <summary>
                /// dcterms:MethodOfAccrual
                /// </summary>
                public static readonly RDFResource METHOD_OF_ACCRUAL = new RDFResource($"{BASE_URI}MethodOfAccrual");

                /// <summary>
                /// dcterms:MethodOfInstruction
                /// </summary>
                public static readonly RDFResource METHOD_OF_INSTRUCTION = new RDFResource($"{BASE_URI}MethodOfInstruction");

                /// <summary>
                /// dcterms:modified
                /// </summary>
                public static readonly RDFResource MODIFIED = new RDFResource($"{BASE_URI}modified");

                /// <summary>
                /// dcterms:PeriodOfTime
                /// </summary>
                public static readonly RDFResource PERIOD_OF_TIME = new RDFResource($"{BASE_URI}PeriodOfTime");

                /// <summary>
                /// dcterms:PhysicalMedium
                /// </summary>
                public static readonly RDFResource PHYSICAL_MEDIUM = new RDFResource($"{BASE_URI}PhysicalMedium");

                /// <summary>
                /// dcterms:PhysicalResource
                /// </summary>
                public static readonly RDFResource PHYSICAL_RESOURCE = new RDFResource($"{BASE_URI}PhysicalResource");

                /// <summary>
                /// dcterms:Policy
                /// </summary>
                public static readonly RDFResource POLICY = new RDFResource($"{BASE_URI}Policy");

                /// <summary>
                /// dcterms:provenance
                /// </summary>
                public static readonly RDFResource PROVENANCE = new RDFResource($"{BASE_URI}provenance");

                /// <summary>
                /// dcterms:ProvenanceStatement
                /// </summary>
                public static readonly RDFResource PROVENANCE_STATEMENT = new RDFResource($"{BASE_URI}ProvenanceStatement");

                /// <summary>
                /// dcterms:publisher
                /// </summary>
                public static readonly RDFResource PUBLISHER = new RDFResource($"{BASE_URI}publisher");

                /// <summary>
                /// dcterms:references
                /// </summary>
                public static readonly RDFResource REFERENCES = new RDFResource($"{BASE_URI}references");

                /// <summary>
                /// dcterms:relation
                /// </summary>
                public static readonly RDFResource RELATION = new RDFResource($"{BASE_URI}relation");

                /// <summary>
                /// dcterms:replaces
                /// </summary>
                public static readonly RDFResource REPLACES = new RDFResource($"{BASE_URI}replaces");

                /// <summary>
                /// dcterms:requires
                /// </summary>
                public static readonly RDFResource REQUIRES = new RDFResource($"{BASE_URI}requires");

                /// <summary>
                /// dcterms:rights
                /// </summary>
                public static readonly RDFResource RIGHTS = new RDFResource($"{BASE_URI}rights");

                /// <summary>
                /// dcterms:RightsStatement
                /// </summary>
                public static readonly RDFResource RIGHTS_STATEMENT = new RDFResource($"{BASE_URI}RightsStatement");

                /// <summary>
                /// dcterms:rightsHolder
                /// </summary>
                public static readonly RDFResource RIGHTS_HOLDER = new RDFResource($"{BASE_URI}rightsHolder");

                /// <summary>
                /// dcterms:SizeOrDuration
                /// </summary>
                public static readonly RDFResource SIZE_OR_DURATION = new RDFResource($"{BASE_URI}SizeOrDuration");

                /// <summary>
                /// dcterms:source
                /// </summary>
                public static readonly RDFResource SOURCE = new RDFResource($"{BASE_URI}source");

                /// <summary>
                /// dcterms:spatial
                /// </summary>
                public static readonly RDFResource SPATIAL = new RDFResource($"{BASE_URI}spatial");

                /// <summary>
                /// dcterms:Standard
                /// </summary>
                public static readonly RDFResource STANDARD = new RDFResource($"{BASE_URI}Standard");

                /// <summary>
                /// dcterms:subject
                /// </summary>
                public static readonly RDFResource SUBJECT = new RDFResource($"{BASE_URI}subject");

                /// <summary>
                /// dcterms:tableOfContents
                /// </summary>
                public static readonly RDFResource TABLE_OF_CONTENTS = new RDFResource($"{BASE_URI}tableOfContents");

                /// <summary>
                /// dcterms:temporal
                /// </summary>
                public static readonly RDFResource TEMPORAL = new RDFResource($"{BASE_URI}temporal");

                /// <summary>
                /// dcterms:title
                /// </summary>
                public static readonly RDFResource TITLE = new RDFResource($"{BASE_URI}title");

                /// <summary>
                /// dcterms:type
                /// </summary>
                public static readonly RDFResource TYPE = new RDFResource($"{BASE_URI}type");

                /// <summary>
                /// dcterms:valid
                /// </summary>
                public static readonly RDFResource VALID = new RDFResource($"{BASE_URI}valid");

                #region Vocabulary Encoding Schemes
                /// <summary>
                /// dcterms:DCMIType
                /// </summary>
                public static readonly RDFResource DCMITYPE = new RDFResource($"{BASE_URI}DCMIType");

                /// <summary>
                /// dcterms:DDC
                /// </summary>
                public static readonly RDFResource DDC = new RDFResource($"{BASE_URI}DDC");

                /// <summary>
                /// dcterms:IMT
                /// </summary>
                public static readonly RDFResource IMT = new RDFResource($"{BASE_URI}IMT");

                /// <summary>
                /// dcterms:LCC
                /// </summary>
                public static readonly RDFResource LCC = new RDFResource($"{BASE_URI}LCC");

                /// <summary>
                /// dcterms:LCSH
                /// </summary>
                public static readonly RDFResource LCSH = new RDFResource($"{BASE_URI}LCSH");

                /// <summary>
                /// dcterms:MESH
                /// </summary>
                public static readonly RDFResource MESH = new RDFResource($"{BASE_URI}MESH");

                /// <summary>
                /// dcterms:NLM
                /// </summary>
                public static readonly RDFResource NLM = new RDFResource($"{BASE_URI}NLM");

                /// <summary>
                /// dcterms:TGN
                /// </summary>
                public static readonly RDFResource TGN = new RDFResource($"{BASE_URI}TGN");

                /// <summary>
                /// dcterms:UDC
                /// </summary>
                public static readonly RDFResource UDC = new RDFResource($"{BASE_URI}UDC");
                #endregion

                #region Syntax Encoding Schemes
                /// <summary>
                /// dcterms:Box
                /// </summary>
                public static readonly RDFResource BOX = new RDFResource($"{BASE_URI}Box");

                /// <summary>
                /// dcterms:ISO3166
                /// </summary>
                public static readonly RDFResource ISO3166 = new RDFResource($"{BASE_URI}ISO3166");

                /// <summary>
                /// dcterms:ISO639-2
                /// </summary>
                public static readonly RDFResource ISO639_2 = new RDFResource($"{BASE_URI}ISO639-2");

                /// <summary>
                /// dcterms:ISO639-3
                /// </summary>
                public static readonly RDFResource ISO639_3 = new RDFResource($"{BASE_URI}ISO639-3");

                /// <summary>
                /// dcterms:Period
                /// </summary>
                public static readonly RDFResource PERIOD = new RDFResource($"{BASE_URI}Period");

                /// <summary>
                /// dcterms:Point
                /// </summary>
                public static readonly RDFResource POINT = new RDFResource($"{BASE_URI}Point");

                /// <summary>
                /// dcterms:RFC1766
                /// </summary>
                public static readonly RDFResource RFC1766 = new RDFResource($"{BASE_URI}RFC1766");

                /// <summary>
                /// dcterms:RFC3066
                /// </summary>
                public static readonly RDFResource RFC3066 = new RDFResource($"{BASE_URI}RFC3066");

                /// <summary>
                /// dcterms:RFC4646
                /// </summary>
                public static readonly RDFResource RFC4646 = new RDFResource($"{BASE_URI}RFC4646");

                /// <summary>
                /// dcterms:RFC5646
                /// </summary>
                public static readonly RDFResource RFC5646 = new RDFResource($"{BASE_URI}RFC5646");

                /// <summary>
                /// dcterms:URI
                /// </summary>
                public static readonly RDFResource URI = new RDFResource($"{BASE_URI}URI");

                /// <summary>
                /// dcterms:W3CDTF
                /// </summary>
                public static readonly RDFResource W3CDTF = new RDFResource($"{BASE_URI}W3CDTF");
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
                public static readonly RDFResource COLLECTION = new RDFResource($"{BASE_URI}Collection");

                /// <summary>
                /// dctype:Dataset
                /// </summary>
                public static readonly RDFResource DATASET = new RDFResource($"{BASE_URI}Dataset");

                /// <summary>
                /// dctype:Event
                /// </summary>
                public static readonly RDFResource EVENT = new RDFResource($"{BASE_URI}Event");

                /// <summary>
                /// dctype:Image
                /// </summary>
                public static readonly RDFResource IMAGE = new RDFResource($"{BASE_URI}Image");

                /// <summary>
                /// dctype:InteractiveResource
                /// </summary>
                public static readonly RDFResource INTERACTIVE_RESOURCE = new RDFResource($"{BASE_URI}InteractiveResource");

                /// <summary>
                /// dctype:MovingImage
                /// </summary>
                public static readonly RDFResource MOVING_IMAGE = new RDFResource($"{BASE_URI}MovingImage");

                /// <summary>
                /// dctype:PhysicalObject
                /// </summary>
                public static readonly RDFResource PHYSICAL_OBJECT = new RDFResource($"{BASE_URI}PhysicalObject");

                /// <summary>
                /// dctype:Service
                /// </summary>
                public static readonly RDFResource SERVICE = new RDFResource($"{BASE_URI}Service");

                /// <summary>
                /// dctype:Software
                /// </summary>
                public static readonly RDFResource SOFTWARE = new RDFResource($"{BASE_URI}Software");

                /// <summary>
                /// dctype:Sound
                /// </summary>
                public static readonly RDFResource SOUND = new RDFResource($"{BASE_URI}Sound");

                /// <summary>
                /// dctype:StillImage
                /// </summary>
                public static readonly RDFResource STILL_IMAGE = new RDFResource($"{BASE_URI}StillImage");

                /// <summary>
                /// dctype:Text
                /// </summary>
                public static readonly RDFResource TEXT = new RDFResource($"{BASE_URI}Text");
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
        public static class FOAF
        {
            #region Properties
            /// <summary>
            /// foaf
            /// </summary>
            public static readonly string PREFIX = "foaf";

            /// <summary>
            /// http://xmlns.com/foaf/0.1/
            /// </summary>
            public static readonly string BASE_URI = "http://xmlns.com/foaf/0.1/";

            /// <summary>
            /// http://xmlns.com/foaf/0.1/
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://xmlns.com/foaf/0.1/";

            /// <summary>
            /// foaf:Agent
            /// </summary>
            public static readonly RDFResource AGENT = new RDFResource($"{BASE_URI}Agent");

            /// <summary>
            /// foaf:Person
            /// </summary>
            public static readonly RDFResource PERSON = new RDFResource($"{BASE_URI}Person");

            /// <summary>
            /// foaf:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource($"{BASE_URI}name");

            /// <summary>
            /// foaf:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource($"{BASE_URI}title");

            /// <summary>
            /// foaf:img
            /// </summary>
            public static readonly RDFResource IMG = new RDFResource($"{BASE_URI}img");

            /// <summary>
            /// foaf:depiction
            /// </summary>
            public static readonly RDFResource DEPICTION = new RDFResource($"{BASE_URI}depiction");

            /// <summary>
            /// foaf:depicts
            /// </summary>
            public static readonly RDFResource DEPICTS = new RDFResource($"{BASE_URI}depicts");

            /// <summary>
            /// foaf:familyName
            /// </summary>
            public static readonly RDFResource FAMILY_NAME = new RDFResource($"{BASE_URI}familyName");

            /// <summary>
            /// foaf:givenName
            /// </summary>
            public static readonly RDFResource GIVEN_NAME = new RDFResource($"{BASE_URI}givenName");

            /// <summary>
            /// foaf:knows
            /// </summary>
            public static readonly RDFResource KNOWS = new RDFResource($"{BASE_URI}knows");

            /// <summary>
            /// foaf:skypeID
            /// </summary>
            public static readonly RDFResource SKYPE_ID = new RDFResource($"{BASE_URI}skypeID");

            /// <summary>
            /// foaf:based_near
            /// </summary>
            public static readonly RDFResource BASED_NEAR = new RDFResource($"{BASE_URI}based_near");

            /// <summary>
            /// foaf:age
            /// </summary>
            public static readonly RDFResource AGE = new RDFResource($"{BASE_URI}age");

            /// <summary>
            /// foaf:made
            /// </summary>
            public static readonly RDFResource MADE = new RDFResource($"{BASE_URI}made");

            /// <summary>
            /// foaf:maker
            /// </summary>
            public static readonly RDFResource MAKER = new RDFResource($"{BASE_URI}maker");

            /// <summary>
            /// foaf:primaryTopic
            /// </summary>
            public static readonly RDFResource PRIMARY_TOPIC = new RDFResource($"{BASE_URI}primaryTopic");

            /// <summary>
            /// foaf:isPrimaryTopicOf
            /// </summary>
            public static readonly RDFResource IS_PRIMARY_TOPIC_OF = new RDFResource($"{BASE_URI}isPrimaryTopicOf");

            /// <summary>
            /// foaf:Project
            /// </summary>
            public static readonly RDFResource PROJECT = new RDFResource($"{BASE_URI}Project");

            /// <summary>
            /// foaf:Organization
            /// </summary>
            public static readonly RDFResource ORGANIZATION = new RDFResource($"{BASE_URI}Organization");

            /// <summary>
            /// foaf:Group
            /// </summary>
            public static readonly RDFResource GROUP = new RDFResource($"{BASE_URI}Group");

            /// <summary>
            /// foaf:Document
            /// </summary>
            public static readonly RDFResource DOCUMENT = new RDFResource($"{BASE_URI}Document");

            /// <summary>
            /// foaf:Image
            /// </summary>
            public static readonly RDFResource IMAGE = new RDFResource($"{BASE_URI}Image");

            /// <summary>
            /// foaf:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource($"{BASE_URI}member");

            /// <summary>
            /// foaf:focus
            /// </summary>
            public static readonly RDFResource FOCUS = new RDFResource($"{BASE_URI}focus");

            /// <summary>
            /// foaf:fundedBy
            /// </summary>
            public static readonly RDFResource FUNDED_BY = new RDFResource($"{BASE_URI}fundedBy");

            /// <summary>
            /// foaf:geekcode
            /// </summary>
            public static readonly RDFResource GEEK_CODE = new RDFResource($"{BASE_URI}geekcode");

            /// <summary>
            /// foaf:theme
            /// </summary>
            public static readonly RDFResource THEME = new RDFResource($"{BASE_URI}theme");

            /// <summary>
            /// foaf:nick
            /// </summary>
            public static readonly RDFResource NICK = new RDFResource($"{BASE_URI}nick");

            /// <summary>
            /// foaf:mbox
            /// </summary>
            public static readonly RDFResource MBOX = new RDFResource($"{BASE_URI}mbox");

            /// <summary>
            /// foaf:homepage
            /// </summary>
            public static readonly RDFResource HOMEPAGE = new RDFResource($"{BASE_URI}homepage");

            /// <summary>
            /// foaf:weblog
            /// </summary>
            public static readonly RDFResource WEBLOG = new RDFResource($"{BASE_URI}weblog");

            /// <summary>
            /// foaf:openid
            /// </summary>
            public static readonly RDFResource OPEN_ID = new RDFResource($"{BASE_URI}openid");

            /// <summary>
            /// foaf:jabberID
            /// </summary>
            public static readonly RDFResource JABBER_ID = new RDFResource($"{BASE_URI}jabberID");

            /// <summary>
            /// foaf:aimChatID
            /// </summary>
            public static readonly RDFResource AIM_CHAT_ID = new RDFResource($"{BASE_URI}aimChatID");

            /// <summary>
            /// foaf:icqChatID
            /// </summary>
            public static readonly RDFResource ICQ_CHAT_ID = new RDFResource($"{BASE_URI}icqChatID");

            /// <summary>
            /// foaf:msnChatID
            /// </summary>
            public static readonly RDFResource MSN_CHAT_ID = new RDFResource($"{BASE_URI}msnChatID");

            /// <summary>
            /// foaf:yahooChatID
            /// </summary>
            public static readonly RDFResource YAHOO_CHAT_ID = new RDFResource($"{BASE_URI}yahooChatID");

            /// <summary>
            /// foaf:myersBriggs
            /// </summary>
            public static readonly RDFResource MYERS_BRIGGS = new RDFResource($"{BASE_URI}myersBriggs");

            /// <summary>
            /// foaf:dnaChecksum
            /// </summary>
            public static readonly RDFResource DNA_CHECKSUM = new RDFResource($"{BASE_URI}dnaChecksum");

            /// <summary>
            /// foaf:membershipClass
            /// </summary>
            public static readonly RDFResource MEMBERSHIP_CLASS = new RDFResource($"{BASE_URI}membershipClass");

            /// <summary>
            /// foaf:holdsAccount
            /// </summary>
            public static readonly RDFResource HOLDS_ACCOUNT = new RDFResource($"{BASE_URI}holdsAccount");

            /// <summary>
            /// foaf:firstName
            /// </summary>
            public static readonly RDFResource FIRSTNAME = new RDFResource($"{BASE_URI}firstName");

            /// <summary>
            /// foaf:surname
            /// </summary>
            public static readonly RDFResource SURNAME = new RDFResource($"{BASE_URI}surname");

            /// <summary>
            /// foaf:plan
            /// </summary>
            public static readonly RDFResource PLAN = new RDFResource($"{BASE_URI}plan");

            /// <summary>
            /// foaf:mbox_sha1sum
            /// </summary>
            public static readonly RDFResource MBOX_SHA1SUM = new RDFResource($"{BASE_URI}mbox_sha1sum");

            /// <summary>
            /// foaf:interest
            /// </summary>
            public static readonly RDFResource INTEREST = new RDFResource($"{BASE_URI}interest");

            /// <summary>
            /// foaf:topic_interest
            /// </summary>
            public static readonly RDFResource TOPIC_INTEREST = new RDFResource($"{BASE_URI}topic_interest");

            /// <summary>
            /// foaf:topic
            /// </summary>
            public static readonly RDFResource TOPIC = new RDFResource($"{BASE_URI}topic");

            /// <summary>
            /// foaf:page
            /// </summary>
            public static readonly RDFResource PAGE = new RDFResource($"{BASE_URI}page");

            /// <summary>
            /// foaf:workplaceHomepage
            /// </summary>
            public static readonly RDFResource WORKPLACE_HOMEPAGE = new RDFResource($"{BASE_URI}workplaceHomepage");

            /// <summary>
            /// foaf:workinfoHomepage
            /// </summary>
            public static readonly RDFResource WORKINFO_HOMEPAGE = new RDFResource($"{BASE_URI}workinfoHomepage");

            /// <summary>
            /// foaf:schoolHomepage
            /// </summary>
            public static readonly RDFResource SCHOOL_HOMEPAGE = new RDFResource($"{BASE_URI}schoolHomepage");

            /// <summary>
            /// foaf:publications
            /// </summary>
            public static readonly RDFResource PUBLICATIONS = new RDFResource($"{BASE_URI}publications");

            /// <summary>
            /// foaf:currentProject
            /// </summary>
            public static readonly RDFResource CURRENT_PROJECT = new RDFResource($"{BASE_URI}currentProject");

            /// <summary>
            /// foaf:pastProject
            /// </summary>
            public static readonly RDFResource PAST_PROJECT = new RDFResource($"{BASE_URI}pastProject");

            /// <summary>
            /// foaf:account
            /// </summary>
            public static readonly RDFResource ACCOUNT = new RDFResource($"{BASE_URI}account");

            /// <summary>
            /// foaf:OnlineAccount
            /// </summary>
            public static readonly RDFResource ONLINE_ACCOUNT = new RDFResource($"{BASE_URI}OnlineAccount");

            /// <summary>
            /// foaf:OnlineChatAccount
            /// </summary>
            public static readonly RDFResource ONLINE_CHAT_ACCOUNT = new RDFResource($"{BASE_URI}OnlineChatAccount");

            /// <summary>
            /// foaf:OnlineEcommerceAccount
            /// </summary>
            public static readonly RDFResource ONLINE_ECOMMERCE_ACCOUNT = new RDFResource($"{BASE_URI}OnlineEcommerceAccount");

            /// <summary>
            /// foaf:OnlineGamingAccount
            /// </summary>
            public static readonly RDFResource ONLINE_GAMING_ACCOUNT = new RDFResource($"{BASE_URI}OnlineGamingAccount");

            /// <summary>
            /// foaf:accountName
            /// </summary>
            public static readonly RDFResource ACCOUNT_NAME = new RDFResource($"{BASE_URI}accountName");

            /// <summary>
            /// foaf:accountServiceHomepage
            /// </summary>
            public static readonly RDFResource ACCOUNT_SERVICE_HOMEPAGE = new RDFResource($"{BASE_URI}accountServiceHomepage");

            /// <summary>
            /// foaf:PersonalProfileDocument
            /// </summary>
            public static readonly RDFResource PERSONAL_PROFILE_DOCUMENT = new RDFResource($"{BASE_URI}PersonalProfileDocument");

            /// <summary>
            /// foaf:tipjar
            /// </summary>
            public static readonly RDFResource TIPJAR = new RDFResource($"{BASE_URI}tipjar");

            /// <summary>
            /// foaf:sha1
            /// </summary>
            public static readonly RDFResource SHA1 = new RDFResource($"{BASE_URI}sha1");

            /// <summary>
            /// foaf:thumbnail
            /// </summary>
            public static readonly RDFResource THUMBNAIL = new RDFResource($"{BASE_URI}thumbnail");

            /// <summary>
            /// foaf:logo
            /// </summary>
            public static readonly RDFResource LOGO = new RDFResource($"{BASE_URI}logo");

            /// <summary>
            /// foaf:phone
            /// </summary>
            public static readonly RDFResource PHONE = new RDFResource($"{BASE_URI}phone");

            /// <summary>
            /// foaf:status
            /// </summary>
            public static readonly RDFResource STATUS = new RDFResource($"{BASE_URI}status");

            /// <summary>
            /// foaf:gender
            /// </summary>
            public static readonly RDFResource GENDER = new RDFResource($"{BASE_URI}gender");

            /// <summary>
            /// foaf:birthday
            /// </summary>
            public static readonly RDFResource BIRTHDAY = new RDFResource($"{BASE_URI}birthday");
            #endregion
        }
        #endregion

        #region GEO
        /// <summary>
        /// GEO represents the W3C GEO vocabulary.
        /// </summary>
        public static class GEO
        {
            #region Properties
            /// <summary>
            /// geo
            /// </summary>
            public static readonly string PREFIX = "geo";

            /// <summary>
            /// http://www.w3.org/2003/01/geo/wgs84_pos#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2003/01/geo/wgs84_pos#";

            /// <summary>
            /// http://www.w3.org/2003/01/geo/wgs84_pos#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2003/01/geo/wgs84_pos#";

            /// <summary>
            /// geo:lat
            /// </summary>
            public static readonly RDFResource LAT = new RDFResource($"{BASE_URI}lat");

            /// <summary>
            /// geo:long
            /// </summary>
            public static readonly RDFResource LONG = new RDFResource($"{BASE_URI}long");

            /// <summary>
            /// geo:lat_long
            /// </summary>
            public static readonly RDFResource LAT_LONG = new RDFResource($"{BASE_URI}lat_long");

            /// <summary>
            /// geo:alt
            /// </summary>
            public static readonly RDFResource ALT = new RDFResource($"{BASE_URI}alt");

            /// <summary>
            /// geo:Point
            /// </summary>
            public static readonly RDFResource POINT = new RDFResource($"{BASE_URI}Point");

            /// <summary>
            /// geo:SpatialThing
            /// </summary>
            public static readonly RDFResource SPATIAL_THING = new RDFResource($"{BASE_URI}SpatialThing");

            /// <summary>
            /// geo:location
            /// </summary>
            public static readonly RDFResource LOCATION = new RDFResource($"{BASE_URI}location");
            #endregion
        }
        #endregion

        #region GEOSPARQL
        /// <summary>
        /// GEOSPARQL represents the OGC GeoSPARQL 1.0 vocabulary (with SF and GEOF extensions)
        /// </summary>
        public static class GEOSPARQL
        {
            #region Properties
            /// <summary>
            /// geosparql
            /// </summary>
            public static readonly string PREFIX = "geosparql";

            /// <summary>
            /// http://www.opengis.net/ont/geosparql#
            /// </summary>
            public static readonly string BASE_URI = "http://www.opengis.net/ont/geosparql#";

            /// <summary>
            /// http://www.opengis.net/ont/geosparql#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.opengis.net/ont/geosparql#";

            /// <summary>
            /// geosparql:wktLiteral
            /// </summary>
            public static readonly RDFResource WKT_LITERAL = new RDFResource($"{BASE_URI}wktLiteral");

            /// <summary>
            /// geosparql:gmlLiteral
            /// </summary>
            public static readonly RDFResource GML_LITERAL = new RDFResource($"{BASE_URI}gmlLiteral");

            /// <summary>
            /// geosparql:SpatialObject
            /// </summary>
            public static readonly RDFResource SPATIAL_OBJECT = new RDFResource($"{BASE_URI}SpatialObject");

            /// <summary>
            /// geosparql:Geometry
            /// </summary>
            public static readonly RDFResource GEOMETRY = new RDFResource($"{BASE_URI}Geometry");

            /// <summary>
            /// geosparql:Feature
            /// </summary>
            public static readonly RDFResource FEATURE = new RDFResource($"{BASE_URI}Feature");

            /// <summary>
            /// geosparql:defaultGeometry
            /// </summary>
            public static readonly RDFResource DEFAULT_GEOMETRY = new RDFResource($"{BASE_URI}defaultGeometry");

            /// <summary>
            /// geosparql:ehDisjoint
            /// </summary>
            public static readonly RDFResource EH_DISJOINT = new RDFResource($"{BASE_URI}ehDisjoint");

            /// <summary>
            /// geosparql:rcc8ntpp
            /// </summary>
            public static readonly RDFResource RCC8NTPP = new RDFResource($"{BASE_URI}rcc8ntpp");

            /// <summary>
            /// geosparql:ehContains
            /// </summary>
            public static readonly RDFResource EH_CONTAINS = new RDFResource($"{BASE_URI}ehContains");

            /// <summary>
            /// geosparql:rcc8tppi
            /// </summary>
            public static readonly RDFResource RCC8TPPI = new RDFResource($"{BASE_URI}rcc8tppi");

            /// <summary>
            /// geosparql:rcc8ec
            /// </summary>
            public static readonly RDFResource RCC8EC = new RDFResource($"{BASE_URI}rcc8ec");

            /// <summary>
            /// geosparql:sfEquals
            /// </summary>
            public static readonly RDFResource SF_EQUALS = new RDFResource($"{BASE_URI}sfEquals");

            /// <summary>
            /// geosparql:ehOverlap
            /// </summary>
            public static readonly RDFResource EH_OVERLAP = new RDFResource($"{BASE_URI}ehOverlap");

            /// <summary>
            /// geosparql:hasGeometry
            /// </summary>
            public static readonly RDFResource HAS_GEOMETRY = new RDFResource($"{BASE_URI}hasGeometry");

            /// <summary>
            /// geosparql:rcc8dc
            /// </summary>
            public static readonly RDFResource RCC8DC = new RDFResource($"{BASE_URI}rcc8dc");

            /// <summary>
            /// geosparql:ehCovers
            /// </summary>
            public static readonly RDFResource EH_COVERS = new RDFResource($"{BASE_URI}ehCovers");

            /// <summary>
            /// geosparql:ehCoveredBy
            /// </summary>
            public static readonly RDFResource EH_COVERED_BY = new RDFResource($"{BASE_URI}ehCoveredBy");

            /// <summary>
            /// geosparql:sfContains
            /// </summary>
            public static readonly RDFResource SF_CONTAINS = new RDFResource($"{BASE_URI}sfContains");

            /// <summary>
            /// geosparql:rcc8tpp
            /// </summary>
            public static readonly RDFResource RCC8TPP = new RDFResource($"{BASE_URI}rcc8tpp");

            /// <summary>
            /// geosparql:ehEquals
            /// </summary>
            public static readonly RDFResource EH_EQUALS = new RDFResource($"{BASE_URI}ehEquals");

            /// <summary>
            /// geosparql:rcc8po
            /// </summary>
            public static readonly RDFResource RCC8PO = new RDFResource($"{BASE_URI}rcc8po");

            /// <summary>
            /// geosparql:sfOverlaps
            /// </summary>
            public static readonly RDFResource SF_OVERLAPS = new RDFResource($"{BASE_URI}sfOverlaps");

            /// <summary>
            /// geosparql:sfWithin
            /// </summary>
            public static readonly RDFResource SF_WITHIN = new RDFResource($"{BASE_URI}sfWithin");

            /// <summary>
            /// geosparql:sfTouches
            /// </summary>
            public static readonly RDFResource SF_TOUCHES = new RDFResource($"{BASE_URI}sfTouches");

            /// <summary>
            /// geosparql:sfIntersects
            /// </summary>
            public static readonly RDFResource SF_INTERSECTS = new RDFResource($"{BASE_URI}sfIntersects");

            /// <summary>
            /// geosparql:sfCrosses
            /// </summary>
            public static readonly RDFResource SF_CROSSES = new RDFResource($"{BASE_URI}sfCrosses");

            /// <summary>
            /// geosparql:rcc8ntppi
            /// </summary>
            public static readonly RDFResource RCC8NTPPI = new RDFResource($"{BASE_URI}rcc8ntppi");

            /// <summary>
            /// geosparql:rcc8eq
            /// </summary>
            public static readonly RDFResource RCC8EQ = new RDFResource($"{BASE_URI}rcc8eq");

            /// <summary>
            /// geosparql:ehMeet
            /// </summary>
            public static readonly RDFResource EH_MEET = new RDFResource($"{BASE_URI}ehMeet");

            /// <summary>
            /// geosparql:sfDisjoint
            /// </summary>
            public static readonly RDFResource SF_DISJOINT = new RDFResource($"{BASE_URI}sfDisjoint");

            /// <summary>
            /// geosparql:ehInside
            /// </summary>
            public static readonly RDFResource EH_INSIDE = new RDFResource($"{BASE_URI}ehInside");

            /// <summary>
            /// geosparql:spatialDimension
            /// </summary>
            public static readonly RDFResource SPATIAL_DIMENSION = new RDFResource($"{BASE_URI}spatialDimension");

            /// <summary>
            /// geosparql:isEmpty
            /// </summary>
            public static readonly RDFResource IS_EMPTY = new RDFResource($"{BASE_URI}isEmpty");

            /// <summary>
            /// geosparql:coordinateDimension
            /// </summary>
            public static readonly RDFResource COORDINATE_DIMENSION = new RDFResource($"{BASE_URI}coordinateDimension");

            /// <summary>
            /// geosparql:asWKT
            /// </summary>
            public static readonly RDFResource AS_WKT = new RDFResource($"{BASE_URI}asWKT");

            /// <summary>
            /// geosparql:asGML
            /// </summary>
            public static readonly RDFResource AS_GML = new RDFResource($"{BASE_URI}asGML");

            /// <summary>
            /// geosparql:isSimple
            /// </summary>
            public static readonly RDFResource IS_SIMPLE = new RDFResource($"{BASE_URI}isSimple");

            /// <summary>
            /// geosparql:dimension
            /// </summary>
            public static readonly RDFResource DIMENSION = new RDFResource($"{BASE_URI}dimension");

            /// <summary>
            /// geosparql:hasSerialization
            /// </summary>
            public static readonly RDFResource HAS_SERIALIZATION = new RDFResource($"{BASE_URI}hasSerialization");
            #endregion

            #region Extended Properties
            /// <summary>
            /// Simple Features extensions
            /// </summary>
            public static class SF
            {
                /// <summary>
                /// sf
                /// </summary>
                public static readonly string PREFIX = "sf";

                /// <summary>
                /// http://www.opengis.net/ont/sf#
                /// </summary>
                public static readonly string BASE_URI = "http://www.opengis.net/ont/sf#";

                /// <summary>
                /// http://www.opengis.net/ont/sf#
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://www.opengis.net/ont/sf#";

                /// <summary>
                /// sf:Point
                /// </summary>
                public static readonly RDFResource POINT = new RDFResource($"{BASE_URI}Point");

                /// <summary>
                /// sf:Curve
                /// </summary>
                public static readonly RDFResource CURVE = new RDFResource($"{BASE_URI}Curve");

                /// <summary>
                /// sf:Surface
                /// </summary>
                public static readonly RDFResource SURFACE = new RDFResource($"{BASE_URI}Surface");

                /// <summary>
                /// sf:Polygon
                /// </summary>
                public static readonly RDFResource POLYGON = new RDFResource($"{BASE_URI}Polygon");

                /// <summary>
                /// sf:Triangle
                /// </summary>
                public static readonly RDFResource TRIANGLE = new RDFResource($"{BASE_URI}Triangle");

                /// <summary>
                /// sf:LineString
                /// </summary>
                public static readonly RDFResource LINESTRING = new RDFResource($"{BASE_URI}LineString");

                /// <summary>
                /// sf:LinearRing
                /// </summary>
                public static readonly RDFResource LINEAR_RING = new RDFResource($"{BASE_URI}LinearRing");

                /// <summary>
                /// sf:Line
                /// </summary>
                public static readonly RDFResource LINE = new RDFResource($"{BASE_URI}Line");

                /// <summary>
                /// sf:GeometryCollection
                /// </summary>
                public static readonly RDFResource GEOMETRY_COLLECTION = new RDFResource($"{BASE_URI}GeometryCollection");

                /// <summary>
                /// sf:MultiPoint
                /// </summary>
                public static readonly RDFResource MULTI_POINT = new RDFResource($"{BASE_URI}MultiPoint");

                /// <summary>
                /// sf:MultiCurve
                /// </summary>
                public static readonly RDFResource MULTI_CURVE = new RDFResource($"{BASE_URI}MultiCurve");

                /// <summary>
                /// sf:MultiSurface
                /// </summary>
                public static readonly RDFResource MULTI_SURFACE = new RDFResource($"{BASE_URI}MultiSurface");

                /// <summary>
                /// sf:MultiPolygon
                /// </summary>
                public static readonly RDFResource MULTI_POLYGON = new RDFResource($"{BASE_URI}MultiPolygon");

                /// <summary>
                /// sf:MultiLineString
                /// </summary>
                public static readonly RDFResource MULTI_LINESTRING = new RDFResource($"{BASE_URI}MultiLineString");

                /// <summary>
                /// sf:PolyhedralSurface
                /// </summary>
                public static readonly RDFResource POLYHEDRAL_SURFACE = new RDFResource($"{BASE_URI}PolyhedralSurface");

                /// <summary>
                /// sf:TIN
                /// </summary>
                public static readonly RDFResource TIN = new RDFResource($"{BASE_URI}TIN");
            }

            /// <summary>
            /// Functions extensions
            /// </summary>
            public static class GEOF
            {
                /// <summary>
                /// geof
                /// </summary>
                public static readonly string PREFIX = "geof";

                /// <summary>
                /// http://www.opengis.net/def/function/geosparql/
                /// </summary>
                public static readonly string BASE_URI = "http://www.opengis.net/def/function/geosparql/";

                /// <summary>
                /// http://www.opengis.net/def/function/geosparql/
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://www.opengis.net/def/function/geosparql/";

                /// <summary>
                /// geof:boundary
                /// </summary>
                public static readonly RDFResource BOUNDARY = new RDFResource($"{BASE_URI}boundary");

                /// <summary>
                /// geof:buffer
                /// </summary>
                public static readonly RDFResource BUFFER = new RDFResource($"{BASE_URI}buffer");

                /// <summary>
                /// geof:ehContains
                /// </summary>
                public static readonly RDFResource EH_CONTAINS = new RDFResource($"{BASE_URI}ehContains");

                /// <summary>
                /// geof:sfContains
                /// </summary>
                public static readonly RDFResource SF_CONTAINS = new RDFResource($"{BASE_URI}sfContains");

                /// <summary>
                /// geof:convexHull
                /// </summary>
                public static readonly RDFResource CONVEX_HULL = new RDFResource($"{BASE_URI}convexHull");

                /// <summary>
                /// geof:ehCoveredBy
                /// </summary>
                public static readonly RDFResource EH_COVERED_BY = new RDFResource($"{BASE_URI}ehCoveredBy");

                /// <summary>
                /// geof:ehCovers
                /// </summary>
                public static readonly RDFResource EH_COVERS = new RDFResource($"{BASE_URI}ehCovers");

                /// <summary>
                /// geof:sfCrosses
                /// </summary>
                public static readonly RDFResource SF_CROSSES = new RDFResource($"{BASE_URI}sfCrosses");

                /// <summary>
                /// geof:difference
                /// </summary>
                public static readonly RDFResource DIFFERENCE = new RDFResource($"{BASE_URI}difference");

                /// <summary>
                /// geof:rcc8dc
                /// </summary>
                public static readonly RDFResource RCC8DC = new RDFResource($"{BASE_URI}rcc8dc");

                /// <summary>
                /// geof:ehDisjoint
                /// </summary>
                public static readonly RDFResource EH_DISJOINT = new RDFResource($"{BASE_URI}ehDisjoint");

                /// <summary>
                /// geof:sfDisjoint
                /// </summary>
                public static readonly RDFResource SF_DISJOINT = new RDFResource($"{BASE_URI}sfDisjoint");

                /// <summary>
                /// geof:distance
                /// </summary>
                public static readonly RDFResource DISTANCE = new RDFResource($"{BASE_URI}distance");

                /// <summary>
                /// geof:envelope
                /// </summary>
                public static readonly RDFResource ENVELOPE = new RDFResource($"{BASE_URI}envelope");

                /// <summary>
                /// geof:ehEquals
                /// </summary>
                public static readonly RDFResource EH_EQUALS = new RDFResource($"{BASE_URI}ehEquals");

                /// <summary>
                /// geof:sfEquals
                /// </summary>
                public static readonly RDFResource SF_EQUALS = new RDFResource($"{BASE_URI}sfEquals");

                /// <summary>
                /// geof:rcc8eq
                /// </summary>
                public static readonly RDFResource RCC8EQ = new RDFResource($"{BASE_URI}rcc8eq");

                /// <summary>
                /// geof:rcc8ec
                /// </summary>
                public static readonly RDFResource RCC8EC = new RDFResource($"{BASE_URI}rcc8ec");

                /// <summary>
                /// geof:getSRID
                /// </summary>
                public static readonly RDFResource GET_SRID = new RDFResource($"{BASE_URI}getSRID");

                /// <summary>
                /// geof:ehInside
                /// </summary>
                public static readonly RDFResource EH_INSIDE = new RDFResource($"{BASE_URI}ehInside");

                /// <summary>
                /// geof:intersection
                /// </summary>
                public static readonly RDFResource INTERSECTION = new RDFResource($"{BASE_URI}intersection");

                /// <summary>
                /// geof:sfIntersects
                /// </summary>
                public static readonly RDFResource SF_INTERSECTS = new RDFResource($"{BASE_URI}sfIntersects");

                /// <summary>
                /// geof:ehMeet
                /// </summary>
                public static readonly RDFResource EH_MEET = new RDFResource($"{BASE_URI}ehMeet");

                /// <summary>
                /// geof:rcc8ntpp
                /// </summary>
                public static readonly RDFResource RCC8NTPP = new RDFResource($"{BASE_URI}rcc8ntpp");

                /// <summary>
                /// geof:rcc8ntppi
                /// </summary>
                public static readonly RDFResource RCC8NTPPI = new RDFResource($"{BASE_URI}rcc8ntppi");

                /// <summary>
                /// geof:ehOverlap
                /// </summary>
                public static readonly RDFResource EH_OVERLAP = new RDFResource($"{BASE_URI}ehOverlap");

                /// <summary>
                /// geof:sfOverlaps
                /// </summary>
                public static readonly RDFResource SF_OVERLAPS = new RDFResource($"{BASE_URI}sfOverlaps");

                /// <summary>
                /// geof:rcc8po
                /// </summary>
                public static readonly RDFResource RCC8PO = new RDFResource($"{BASE_URI}rcc8po");

                /// <summary>
                /// geof:relate
                /// </summary>
                public static readonly RDFResource RELATE = new RDFResource($"{BASE_URI}relate");

                /// <summary>
                /// geof:symDifference
                /// </summary>
                public static readonly RDFResource SYM_DIFFERENCE = new RDFResource($"{BASE_URI}symDifference");

                /// <summary>
                /// geof:rcc8tpp
                /// </summary>
                public static readonly RDFResource RCC8TPP = new RDFResource($"{BASE_URI}rcc8tpp");

                /// <summary>
                /// geof:rcc8tppi
                /// </summary>
                public static readonly RDFResource RCC8TPPI = new RDFResource($"{BASE_URI}rcc8tppi");

                /// <summary>
                /// geof:sfTouches
                /// </summary>
                public static readonly RDFResource SF_TOUCHES = new RDFResource($"{BASE_URI}sfTouches");

                /// <summary>
                /// geof:union
                /// </summary>
                public static readonly RDFResource SF_UNION = new RDFResource($"{BASE_URI}union");

                /// <summary>
                /// geof:sfWithin
                /// </summary>
                public static readonly RDFResource SF_WITHIN = new RDFResource($"{BASE_URI}sfWithin");
            }
            #endregion
        }
        #endregion

        #region OWL
        /// <summary>
        /// OWL represents the OWL vocabulary.
        /// </summary>
        public static class OWL
        {
            #region Properties
            /// <summary>
            /// owl
            /// </summary>
            public static readonly string PREFIX = "owl";

            /// <summary>
            /// http://www.w3.org/2002/07/owl#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2002/07/owl#";

            /// <summary>
            /// http://www.w3.org/2002/07/owl#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2002/07/owl#";

            /// <summary>
            /// owl:Ontology
            /// </summary>
            public static readonly RDFResource ONTOLOGY = new RDFResource($"{BASE_URI}Ontology");

            /// <summary>
            /// owl:imports
            /// </summary>
            public static readonly RDFResource IMPORTS = new RDFResource($"{BASE_URI}imports");

            /// <summary>
            /// owl:Class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource($"{BASE_URI}Class");

            /// <summary>
            /// owl:Individual
            /// </summary>
            public static readonly RDFResource INDIVIDUAL = new RDFResource($"{BASE_URI}Individual");

            /// <summary>
            /// owl:Thing
            /// </summary>
            public static readonly RDFResource THING = new RDFResource($"{BASE_URI}Thing");

            /// <summary>
            /// owl:Nothing
            /// </summary>
            public static readonly RDFResource NOTHING = new RDFResource($"{BASE_URI}Nothing");

            /// <summary>
            /// owl:NamedIndividual
            /// </summary>
            public static readonly RDFResource NAMED_INDIVIDUAL = new RDFResource($"{BASE_URI}NamedIndividual");

            /// <summary>
            /// owl:Restriction
            /// </summary>
            public static readonly RDFResource RESTRICTION = new RDFResource($"{BASE_URI}Restriction");

            /// <summary>
            /// owl:onProperty
            /// </summary>
            public static readonly RDFResource ON_PROPERTY = new RDFResource($"{BASE_URI}onProperty");

            /// <summary>
            /// owl:equivalentClass
            /// </summary>
            public static readonly RDFResource EQUIVALENT_CLASS = new RDFResource($"{BASE_URI}equivalentClass");

            /// <summary>
            /// owl:DeprecatedClass
            /// </summary>
            public static readonly RDFResource DEPRECATED_CLASS = new RDFResource($"{BASE_URI}DeprecatedClass");

            /// <summary>
            /// owl:equivalentProperty
            /// </summary>
            public static readonly RDFResource EQUIVALENT_PROPERTY = new RDFResource($"{BASE_URI}equivalentProperty");

            /// <summary>
            /// owl:DeprecatedProperty
            /// </summary>
            public static readonly RDFResource DEPRECATED_PROPERTY = new RDFResource($"{BASE_URI}DeprecatedProperty");

            /// <summary>
            /// owl:inverseOf
            /// </summary>
            public static readonly RDFResource INVERSE_OF = new RDFResource($"{BASE_URI}inverseOf");

            /// <summary>
            /// owl:DatatypeProperty
            /// </summary>
            public static readonly RDFResource DATATYPE_PROPERTY = new RDFResource($"{BASE_URI}DatatypeProperty");

            /// <summary>
            /// owl:ObjectProperty
            /// </summary>
            public static readonly RDFResource OBJECT_PROPERTY = new RDFResource($"{BASE_URI}ObjectProperty");

            /// <summary>
            /// owl:TransitiveProperty
            /// </summary>
            public static readonly RDFResource TRANSITIVE_PROPERTY = new RDFResource($"{BASE_URI}TransitiveProperty");

            /// <summary>
            /// owl:SymmetricProperty
            /// </summary>
            public static readonly RDFResource SYMMETRIC_PROPERTY = new RDFResource($"{BASE_URI}SymmetricProperty");

            /// <summary>
            /// owl:FunctionalProperty
            /// </summary>
            public static readonly RDFResource FUNCTIONAL_PROPERTY = new RDFResource($"{BASE_URI}FunctionalProperty");

            /// <summary>
            /// owl:InverseFunctionalProperty
            /// </summary>
            public static readonly RDFResource INVERSE_FUNCTIONAL_PROPERTY = new RDFResource($"{BASE_URI}InverseFunctionalProperty");

            /// <summary>
            /// owl:AnnotationProperty
            /// </summary>
            public static readonly RDFResource ANNOTATION_PROPERTY = new RDFResource($"{BASE_URI}AnnotationProperty");

            /// <summary>
            /// owl:OntologyProperty
            /// </summary>
            public static readonly RDFResource ONTOLOGY_PROPERTY = new RDFResource($"{BASE_URI}OntologyProperty");

            /// <summary>
            /// owl:allValuesFrom
            /// </summary>
            public static readonly RDFResource ALL_VALUES_FROM = new RDFResource($"{BASE_URI}allValuesFrom");

            /// <summary>
            /// owl:someValuesFrom
            /// </summary>
            public static readonly RDFResource SOME_VALUES_FROM = new RDFResource($"{BASE_URI}someValuesFrom");

            /// <summary>
            /// owl:hasValue
            /// </summary>
            public static readonly RDFResource HAS_VALUE = new RDFResource($"{BASE_URI}hasValue");

            /// <summary>
            /// owl:minCardinality
            /// </summary>
            public static readonly RDFResource MIN_CARDINALITY = new RDFResource($"{BASE_URI}minCardinality");

            /// <summary>
            /// owl:maxCardinality
            /// </summary>
            public static readonly RDFResource MAX_CARDINALITY = new RDFResource($"{BASE_URI}maxCardinality");

            /// <summary>
            /// owl:cardinality
            /// </summary>
            public static readonly RDFResource CARDINALITY = new RDFResource($"{BASE_URI}cardinality");

            /// <summary>
            /// owl:sameAs
            /// </summary>
            public static readonly RDFResource SAME_AS = new RDFResource($"{BASE_URI}sameAs");

            /// <summary>
            /// owl:differentFrom
            /// </summary>
            public static readonly RDFResource DIFFERENT_FROM = new RDFResource($"{BASE_URI}differentFrom");

            /// <summary>
            /// owl:members
            /// </summary>
            public static readonly RDFResource MEMBERS = new RDFResource($"{BASE_URI}members");

            /// <summary>
            /// owl:distinctMembers
            /// </summary>
            public static readonly RDFResource DISTINCT_MEMBERS = new RDFResource($"{BASE_URI}distinctMembers");

            /// <summary>
            /// owl:intersectionOf
            /// </summary>
            public static readonly RDFResource INTERSECTION_OF = new RDFResource($"{BASE_URI}intersectionOf");

            /// <summary>
            /// owl:unionOf
            /// </summary>
            public static readonly RDFResource UNION_OF = new RDFResource($"{BASE_URI}unionOf");

            /// <summary>
            /// owl:complementOf
            /// </summary>
            public static readonly RDFResource COMPLEMENT_OF = new RDFResource($"{BASE_URI}complementOf");

            /// <summary>
            /// owl:oneOf
            /// </summary>
            public static readonly RDFResource ONE_OF = new RDFResource($"{BASE_URI}oneOf");

            /// <summary>
            /// owl:DataRange
            /// </summary>
            public static readonly RDFResource DATA_RANGE = new RDFResource($"{BASE_URI}DataRange");

            /// <summary>
            /// owl:backwardCompatibleWith
            /// </summary>
            public static readonly RDFResource BACKWARD_COMPATIBLE_WITH = new RDFResource($"{BASE_URI}backwardCompatibleWith");

            /// <summary>
            /// owl:incompatibleWith
            /// </summary>
            public static readonly RDFResource INCOMPATIBLE_WITH = new RDFResource($"{BASE_URI}incompatibleWith");

            /// <summary>
            /// owl:disjointWith
            /// </summary>
            public static readonly RDFResource DISJOINT_WITH = new RDFResource($"{BASE_URI}disjointWith");

            /// <summary>
            /// owl:priorVersion
            /// </summary>
            public static readonly RDFResource PRIOR_VERSION = new RDFResource($"{BASE_URI}priorVersion");

            /// <summary>
            /// owl:versionInfo
            /// </summary>
            public static readonly RDFResource VERSION_INFO = new RDFResource($"{BASE_URI}versionInfo");

            /// <summary>
            /// owl:versionIRI
            /// </summary>
            public static readonly RDFResource VERSION_IRI = new RDFResource($"{BASE_URI}versionIRI");

            /// <summary>
            /// owl:disjointUnionOf [OWL2]
            /// </summary>
            public static readonly RDFResource DISJOINT_UNION_OF = new RDFResource($"{BASE_URI}disjointUnionOf");

            /// <summary>
            /// owl:AllDisjointClasses [OWL2]
            /// </summary>
            public static readonly RDFResource ALL_DISJOINT_CLASSES = new RDFResource($"{BASE_URI}AllDisjointClasses");

            /// <summary>
            /// owl:AllDifferent [OWL2]
            /// </summary>
            public static readonly RDFResource ALL_DIFFERENT = new RDFResource($"{BASE_URI}AllDifferent");

            /// <summary>
            /// owl:AllDisjointProperties [OWL2]
            /// </summary>
            public static readonly RDFResource ALL_DISJOINT_PROPERTIES = new RDFResource($"{BASE_URI}AllDisjointProperties");

            /// <summary>
            /// owl:AsymmetricProperty [OWL2]
            /// </summary>
            public static readonly RDFResource ASYMMETRIC_PROPERTY = new RDFResource($"{BASE_URI}AsymmetricProperty");

            /// <summary>
            /// owl:ReflexiveProperty [OWL2]
            /// </summary>
            public static readonly RDFResource REFLEXIVE_PROPERTY = new RDFResource($"{BASE_URI}ReflexiveProperty");

            /// <summary>
            /// owl:IrreflexiveProperty [OWL2]
            /// </summary>
            public static readonly RDFResource IRREFLEXIVE_PROPERTY = new RDFResource($"{BASE_URI}IrreflexiveProperty");

            /// <summary>
            /// owl:qualifiedCardinality [OWL2]
            /// </summary>
            public static readonly RDFResource QUALIFIED_CARDINALITY = new RDFResource($"{BASE_URI}qualifiedCardinality");

            /// <summary>
            /// owl:minQualifiedCardinality [OWL2]
            /// </summary>
            public static readonly RDFResource MIN_QUALIFIED_CARDINALITY = new RDFResource($"{BASE_URI}minQualifiedCardinality");

            /// <summary>
            /// owl:maxQualifiedCardinality [OWL2]
            /// </summary>
            public static readonly RDFResource MAX_QUALIFIED_CARDINALITY = new RDFResource($"{BASE_URI}maxQualifiedCardinality");

            /// <summary>
            /// owl:onClass [OWL2]
            /// </summary>
            public static readonly RDFResource ON_CLASS = new RDFResource($"{BASE_URI}onClass");

            /// <summary>
            /// owl:onDataRange [OWL2]
            /// </summary>
            public static readonly RDFResource ON_DATARANGE = new RDFResource($"{BASE_URI}onDataRange");

            /// <summary>
            /// owl:onDatatype [OWL2]
            /// </summary>
            public static readonly RDFResource ON_DATATYPE = new RDFResource($"{BASE_URI}onDatatype");

            /// <summary>
            /// owl:withRestrictions [OWL2]
            /// </summary>
            public static readonly RDFResource WITH_RESTRICTIONS = new RDFResource($"{BASE_URI}withRestrictions");

            /// <summary>
            /// owl:propertyDisjointWith [OWL2]
            /// </summary>
            public static readonly RDFResource PROPERTY_DISJOINT_WITH = new RDFResource($"{BASE_URI}propertyDisjointWith");

            /// <summary>
            /// owl:hasSelf [OWL2]
            /// </summary>
            public static readonly RDFResource HAS_SELF = new RDFResource($"{BASE_URI}hasSelf");

            /// <summary>
            /// owl:NegativePropertyAssertion [OWL2]
            /// </summary>
            public static readonly RDFResource NEGATIVE_PROPERTY_ASSERTION = new RDFResource($"{BASE_URI}NegativePropertyAssertion");

            /// <summary>
            /// owl:sourceIndividual [OWL2]
            /// </summary>
            public static readonly RDFResource SOURCE_INDIVIDUAL = new RDFResource($"{BASE_URI}sourceIndividual");

            /// <summary>
            /// owl:assertionProperty [OWL2]
            /// </summary>
            public static readonly RDFResource ASSERTION_PROPERTY = new RDFResource($"{BASE_URI}assertionProperty");

            /// <summary>
            /// owl:targetIndividual [OWL2]
            /// </summary>
            public static readonly RDFResource TARGET_INDIVIDUAL = new RDFResource($"{BASE_URI}targetIndividual");

            /// <summary>
            /// owl:targetValue [OWL2]
            /// </summary>
            public static readonly RDFResource TARGET_VALUE = new RDFResource($"{BASE_URI}targetValue");

            /// <summary>
            /// owl:hasKey [OWL2]
            /// </summary>
            public static readonly RDFResource HAS_KEY = new RDFResource($"{BASE_URI}hasKey");

            /// <summary>
            /// owl:propertyChainAxiom [OWL2]
            /// </summary>
            public static readonly RDFResource PROPERTY_CHAIN_AXIOM = new RDFResource($"{BASE_URI}propertyChainAxiom");

            /// <summary>
            /// owl:topProperty [OWL2]
            /// </summary>
            public static readonly RDFResource TOP_PROPERTY = new RDFResource($"{BASE_URI}topProperty");

            /// <summary>
            /// owl:topObjectProperty [OWL2]
            /// </summary>
            public static readonly RDFResource TOP_OBJECT_PROPERTY = new RDFResource($"{BASE_URI}topObjectProperty");

            /// <summary>
            /// owl:topDataProperty [OWL2]
            /// </summary>
            public static readonly RDFResource TOP_DATA_PROPERTY = new RDFResource($"{BASE_URI}topDataProperty");

            /// <summary>
            /// owl:bottomProperty [OWL2]
            /// </summary>
            public static readonly RDFResource BOTTOM_PROPERTY = new RDFResource($"{BASE_URI}bottomProperty");

            /// <summary>
            /// owl:bottomObjectProperty [OWL2]
            /// </summary>
            public static readonly RDFResource BOTTOM_OBJECT_PROPERTY = new RDFResource($"{BASE_URI}bottomObjectProperty");

            /// <summary>
            /// owl:bottomDataProperty [OWL2]
            /// </summary>
            public static readonly RDFResource BOTTOM_DATA_PROPERTY = new RDFResource($"{BASE_URI}bottomDataProperty");

            /// <summary>
            /// owl:Axiom [OWL2]
            /// </summary>
            public static readonly RDFResource AXIOM = new RDFResource($"{BASE_URI}Axiom");

            /// <summary>
            /// owl:annotatedSource [OWL2]
            /// </summary>
            public static readonly RDFResource ANNOTATED_SOURCE = new RDFResource($"{BASE_URI}annotatedSource");

            /// <summary>
            /// owl:annotatedProperty [OWL2]
            /// </summary>
            public static readonly RDFResource ANNOTATED_PROPERTY = new RDFResource($"{BASE_URI}annotatedProperty");

            /// <summary>
            /// owl:annotatedTarget [OWL2]
            /// </summary>
            public static readonly RDFResource ANNOTATED_TARGET = new RDFResource($"{BASE_URI}annotatedTarget");

            /// <summary>
            /// owl:deprecated
            /// </summary>
            public static readonly RDFResource DEPRECATED = new RDFResource($"{BASE_URI}deprecated");

            /// <summary>
            /// owl:real [OWL2]
            /// </summary>
            public static readonly RDFResource REAL = new RDFResource($"{BASE_URI}real");
            #endregion
        }
        #endregion

        #region RDFSHARP
        /// <summary>
        /// RDFSHARP represents the vocabulary of this library.
        /// </summary>
        public static class RDFSHARP
        {
            #region Properties
            /// <summary>
            /// rdfsharp
            /// </summary>
            public static readonly string PREFIX = "rdfsharp";

            /// <summary>
            /// https://rdfsharp.codeplex.com/
            /// </summary>
            public static readonly string BASE_URI = "https://rdfsharp.codeplex.com/";

            /// <summary>
            /// https://rdfsharp.codeplex.com/
            /// </summary>
            public static readonly string DEREFERENCE_URI = "https://rdfsharp.codeplex.com/";
            #endregion
        }
        #endregion

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
            public static readonly RDFResource BAG = new RDFResource($"{BASE_URI}Bag");

            /// <summary>
            /// rdf:Seq
            /// </summary>
            public static readonly RDFResource SEQ = new RDFResource($"{BASE_URI}Seq");

            /// <summary>
            /// rdf:Alt
            /// </summary>
            public static readonly RDFResource ALT = new RDFResource($"{BASE_URI}Alt");

            /// <summary>
            /// rdf:Statement
            /// </summary>
            public static readonly RDFResource STATEMENT = new RDFResource($"{BASE_URI}Statement");

            /// <summary>
            /// rdf:Property
            /// </summary>
            public static readonly RDFResource PROPERTY = new RDFResource($"{BASE_URI}Property");

            /// <summary>
            /// rdf:XMLLiteral
            /// </summary>
            public static readonly RDFResource XML_LITERAL = new RDFResource($"{BASE_URI}XMLLiteral");

            /// <summary>
            /// rdf:PlainLiteral
            /// </summary>
            public static readonly RDFResource PLAIN_LITERAL = new RDFResource($"{BASE_URI}PlainLiteral");

            /// <summary>
            /// rdf:HTML
            /// </summary>
            public static readonly RDFResource HTML = new RDFResource($"{BASE_URI}HTML");

            /// <summary>
            /// rdf:JSON
            /// </summary>
            public static readonly RDFResource JSON = new RDFResource($"{BASE_URI}JSON");

            /// <summary>
            /// rdf:List
            /// </summary>
            public static readonly RDFResource LIST = new RDFResource($"{BASE_URI}List");

            /// <summary>
            /// rdf:nil
            /// </summary>
            public static readonly RDFResource NIL = new RDFResource($"{BASE_URI}nil");

            /// <summary>
            /// rdf:first
            /// </summary>
            public static readonly RDFResource FIRST = new RDFResource($"{BASE_URI}first");

            /// <summary>
            /// rdf:rest
            /// </summary>
            public static readonly RDFResource REST = new RDFResource($"{BASE_URI}rest");

            /// <summary>
            /// rdf:li
            /// </summary>
            public static readonly RDFResource LI = new RDFResource($"{BASE_URI}li");

            /// <summary>
            /// rdf:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource($"{BASE_URI}subject");

            /// <summary>
            /// rdf:predicate
            /// </summary>
            public static readonly RDFResource PREDICATE = new RDFResource($"{BASE_URI}predicate");

            /// <summary>
            /// rdf:object
            /// </summary>
            public static readonly RDFResource OBJECT = new RDFResource($"{BASE_URI}object");

            /// <summary>
            /// rdf:type
            /// </summary>
            public static readonly RDFResource TYPE = new RDFResource($"{BASE_URI}type");

            /// <summary>
            /// rdf:value
            /// </summary>
            public static readonly RDFResource VALUE = new RDFResource($"{BASE_URI}value");

            /// <summary>
            /// rdf:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource($"{BASE_URI}language");

            // RDF 1.2

            /// <summary>
            /// rdf:langRange
            /// </summary>
            public static readonly RDFResource LANG_RANGE = new RDFResource($"{BASE_URI}langRange");

            /// <summary>
            /// rdf:langString
            /// </summary>
            public static readonly RDFResource LANG_STRING = new RDFResource($"{BASE_URI}langString");

            /// <summary>
            /// rdf:dirLangString
            /// </summary>
            public static readonly RDFResource DIR_LANG_STRING = new RDFResource($"{BASE_URI}dirLangString");

            /// <summary>
            /// rdf:direction
            /// </summary>
            public static readonly RDFResource DIRECTION = new RDFResource($"{BASE_URI}direction");

            /// <summary>
            /// rdf:reifies
            /// </summary>
            public static readonly RDFResource REIFIES = new RDFResource($"{BASE_URI}reifies");

            /// <summary>
            /// rdf:TripleTerm
            /// </summary>
            public static readonly RDFResource TRIPLE_TERM = new RDFResource($"{BASE_URI}TripleTerm");

            /// <summary>
            /// rdf:ttSubject
            /// </summary>
            public static readonly RDFResource TT_SUBJECT = new RDFResource($"{BASE_URI}ttSubject");

            /// <summary>
            /// rdf:ttPredicate
            /// </summary>
            public static readonly RDFResource TT_PREDICATE = new RDFResource($"{BASE_URI}ttPredicate");

            /// <summary>
            /// rdf:ttObject
            /// </summary>
            public static readonly RDFResource TT_OBJECT = new RDFResource($"{BASE_URI}ttObject");
            #endregion
        }
        #endregion

        #region RDFS
        /// <summary>
        /// RDFS represents the W3C RDF Schema vocabulary.
        /// </summary>
        public static class RDFS
        {
            #region Properties
            /// <summary>
            /// rdfs
            /// </summary>
            public static readonly string PREFIX = "rdfs";

            /// <summary>
            /// http://www.w3.org/2000/01/rdf-schema#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2000/01/rdf-schema#";

            /// <summary>
            /// http://www.w3.org/2000/01/rdf-schema#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2000/01/rdf-schema#";

            /// <summary>
            /// rdfs:Resource
            /// </summary>
            public static readonly RDFResource RESOURCE = new RDFResource($"{BASE_URI}Resource");

            /// <summary>
            /// rdfs:Class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource($"{BASE_URI}Class");

            /// <summary>
            /// rdfs:Literal
            /// </summary>
            public static readonly RDFResource LITERAL = new RDFResource($"{BASE_URI}Literal");

            /// <summary>
            /// rdfs:Container
            /// </summary>
            public static readonly RDFResource CONTAINER = new RDFResource($"{BASE_URI}Container");

            /// <summary>
            /// rdfs:Datatype
            /// </summary>
            public static readonly RDFResource DATATYPE = new RDFResource($"{BASE_URI}Datatype");

            /// <summary>
            /// rdfs:ContainerMembershipProperty
            /// </summary>
            public static readonly RDFResource CONTAINER_MEMBERSHIP_PROPERTY = new RDFResource($"{BASE_URI}ContainerMembershipProperty");

            /// <summary>
            /// rdfs:range
            /// </summary>
            public static readonly RDFResource RANGE = new RDFResource($"{BASE_URI}range");

            /// <summary>
            /// rdfs:domain
            /// </summary>
            public static readonly RDFResource DOMAIN = new RDFResource($"{BASE_URI}domain");

            /// <summary>
            /// rdfs:subClassOf
            /// </summary>
            public static readonly RDFResource SUB_CLASS_OF = new RDFResource($"{BASE_URI}subClassOf");

            /// <summary>
            /// rdfs:subPropertyOf
            /// </summary>
            public static readonly RDFResource SUB_PROPERTY_OF = new RDFResource($"{BASE_URI}subPropertyOf");

            /// <summary>
            /// rdfs:label
            /// </summary>
            public static readonly RDFResource LABEL = new RDFResource($"{BASE_URI}label");

            /// <summary>
            /// rdfs:comment
            /// </summary>
            public static readonly RDFResource COMMENT = new RDFResource($"{BASE_URI}comment");

            /// <summary>
            /// rdfs:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource($"{BASE_URI}member");

            /// <summary>
            /// rdfs:seeAlso
            /// </summary>
            public static readonly RDFResource SEE_ALSO = new RDFResource($"{BASE_URI}seeAlso");

            /// <summary>
            /// rdfs:isDefinedBy
            /// </summary>
            public static readonly RDFResource IS_DEFINED_BY = new RDFResource($"{BASE_URI}isDefinedBy");
            #endregion
        }
        #endregion

        #region SHACL
        /// <summary>
        /// SHACL represents the W3C SHACL Core vocabulary.
        /// </summary>
        public static class SHACL
        {
            #region Properties
            /// <summary>
            /// sh
            /// </summary>
            public static readonly string PREFIX = "sh";

            /// <summary>
            /// http://www.w3.org/ns/shacl#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/ns/shacl#";

            /// <summary>
            /// http://www.w3.org/ns/shacl#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/ns/shacl#";

            /// <summary>
            /// sh:Shape
            /// </summary>
            public static readonly RDFResource SHAPE = new RDFResource($"{BASE_URI}Shape");

            /// <summary>
            /// sh:NodeShape
            /// </summary>
            public static readonly RDFResource NODE_SHAPE = new RDFResource($"{BASE_URI}NodeShape");

            /// <summary>
            /// sh:PropertyShape
            /// </summary>
            public static readonly RDFResource PROPERTY_SHAPE = new RDFResource($"{BASE_URI}PropertyShape");

            /// <summary>
            /// sh:deactivated
            /// </summary>
            public static readonly RDFResource DEACTIVATED = new RDFResource($"{BASE_URI}deactivated");

            /// <summary>
            /// sh:message
            /// </summary>
            public static readonly RDFResource MESSAGE = new RDFResource($"{BASE_URI}message");

            /// <summary>
            /// sh:severity
            /// </summary>
            public static readonly RDFResource SEVERITY_PROPERTY = new RDFResource($"{BASE_URI}severity");

            /// <summary>
            /// sh:targetClass
            /// </summary>
            public static readonly RDFResource TARGET_CLASS = new RDFResource($"{BASE_URI}targetClass");

            /// <summary>
            /// sh:targetNode
            /// </summary>
            public static readonly RDFResource TARGET_NODE = new RDFResource($"{BASE_URI}targetNode");

            /// <summary>
            /// sh:targetObjectsOf
            /// </summary>
            public static readonly RDFResource TARGET_OBJECTS_OF = new RDFResource($"{BASE_URI}targetObjectsOf");

            /// <summary>
            /// sh:targetSubjectsOf
            /// </summary>
            public static readonly RDFResource TARGET_SUBJECTS_OF = new RDFResource($"{BASE_URI}targetSubjectsOf");

            /// <summary>
            /// sh:BlankNode
            /// </summary>
            public static readonly RDFResource BLANK_NODE = new RDFResource($"{BASE_URI}BlankNode");

            /// <summary>
            /// sh:BlankNodeOrIRI
            /// </summary>
            public static readonly RDFResource BLANK_NODE_OR_IRI = new RDFResource($"{BASE_URI}BlankNodeOrIRI");

            /// <summary>
            /// sh:BlankNodeOrLiteral
            /// </summary>
            public static readonly RDFResource BLANK_NODE_OR_LITERAL = new RDFResource($"{BASE_URI}BlankNodeOrLiteral");

            /// <summary>
            /// sh:IRI
            /// </summary>
            public static readonly RDFResource IRI = new RDFResource($"{BASE_URI}IRI");

            /// <summary>
            /// sh:IRIOrLiteral
            /// </summary>
            public static readonly RDFResource IRI_OR_LITERAL = new RDFResource($"{BASE_URI}IRIOrLiteral");

            /// <summary>
            /// sh:Literal
            /// </summary>
            public static readonly RDFResource LITERAL = new RDFResource($"{BASE_URI}Literal");

            /// <summary>
            /// sh:ValidationReport
            /// </summary>
            public static readonly RDFResource VALIDATION_REPORT = new RDFResource($"{BASE_URI}ValidationReport");

            /// <summary>
            /// sh:conforms
            /// </summary>
            public static readonly RDFResource CONFORMS = new RDFResource($"{BASE_URI}conforms");

            /// <summary>
            /// sh:result
            /// </summary>
            public static readonly RDFResource RESULT = new RDFResource($"{BASE_URI}result");

            /// <summary>
            /// sh:shapesGraphWellFormed
            /// </summary>
            public static readonly RDFResource SHAPES_GRAPH_WELL_FORMED = new RDFResource($"{BASE_URI}shapesGraphWellFormed");

            /// <summary>
            /// sh:AbstractResult
            /// </summary>
            public static readonly RDFResource ABSTRACT_RESULT = new RDFResource($"{BASE_URI}AbstractResult");

            /// <summary>
            /// sh:ValidationResult
            /// </summary>
            public static readonly RDFResource VALIDATION_RESULT = new RDFResource($"{BASE_URI}ValidationResult");

            /// <summary>
            /// sh:Severity
            /// </summary>
            public static readonly RDFResource SEVERITY_CLASS = new RDFResource($"{BASE_URI}Severity");

            /// <summary>
            /// sh:Info
            /// </summary>
            public static readonly RDFResource INFO = new RDFResource($"{BASE_URI}Info");

            /// <summary>
            /// sh:Violation
            /// </summary>
            public static readonly RDFResource VIOLATION = new RDFResource($"{BASE_URI}Violation");

            /// <summary>
            /// sh:Warning
            /// </summary>
            public static readonly RDFResource WARNING = new RDFResource($"{BASE_URI}Warning");

            /// <summary>
            /// sh:detail
            /// </summary>
            public static readonly RDFResource DETAIL = new RDFResource($"{BASE_URI}detail");

            /// <summary>
            /// sh:focusNode
            /// </summary>
            public static readonly RDFResource FOCUS_NODE = new RDFResource($"{BASE_URI}focusNode");

            /// <summary>
            /// sh:resultMessage
            /// </summary>
            public static readonly RDFResource RESULT_MESSAGE = new RDFResource($"{BASE_URI}resultMessage");

            /// <summary>
            /// sh:resultPath
            /// </summary>
            public static readonly RDFResource RESULT_PATH = new RDFResource($"{BASE_URI}resultPath");

            /// <summary>
            /// sh:resultSeverity
            /// </summary>
            public static readonly RDFResource RESULT_SEVERITY = new RDFResource($"{BASE_URI}resultSeverity");

            /// <summary>
            /// sh:sourceConstraint
            /// </summary>
            public static readonly RDFResource SOURCE_CONSTRAINT = new RDFResource($"{BASE_URI}sourceConstraint");

            /// <summary>
            /// sh:sourceShape
            /// </summary>
            public static readonly RDFResource SOURCE_SHAPE = new RDFResource($"{BASE_URI}sourceShape");

            /// <summary>
            /// sh:sourceConstraintComponent
            /// </summary>
            public static readonly RDFResource SOURCE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}sourceConstraintComponent");

            /// <summary>
            /// sh:value
            /// </summary>
            public static readonly RDFResource VALUE = new RDFResource($"{BASE_URI}value");

            /// <summary>
            /// sh:shapesGraph
            /// </summary>
            public static readonly RDFResource SHAPES_GRAPH = new RDFResource($"{BASE_URI}shapesGraph");

            /// <summary>
            /// sh:suggestedShapesGraph
            /// </summary>
            public static readonly RDFResource SUGGESTED_SHAPES_GRAPH = new RDFResource($"{BASE_URI}suggestedShapesGraph");

            /// <summary>
            /// sh:entailment
            /// </summary>
            public static readonly RDFResource ENTAILMENT = new RDFResource($"{BASE_URI}entailment");

            /// <summary>
            /// sh:path
            /// </summary>
            public static readonly RDFResource PATH = new RDFResource($"{BASE_URI}path");

            /// <summary>
            /// sh:inversePath
            /// </summary>
            public static readonly RDFResource INVERSE_PATH = new RDFResource($"{BASE_URI}inversePath");

            /// <summary>
            /// sh:alternativePath
            /// </summary>
            public static readonly RDFResource ALTERNATIVE_PATH = new RDFResource($"{BASE_URI}alternativePath");

            /// <summary>
            /// sh:zeroOrMorePath
            /// </summary>
            public static readonly RDFResource ZERO_OR_MORE_PATH = new RDFResource($"{BASE_URI}zeroOrMorePath");

            /// <summary>
            /// sh:oneOrMorePath
            /// </summary>
            public static readonly RDFResource ONE_OR_MORE_PATH = new RDFResource($"{BASE_URI}oneOrMorePath");

            /// <summary>
            /// sh:zeroOrOnePath
            /// </summary>
            public static readonly RDFResource ZERO_OR_ONE_PATH = new RDFResource($"{BASE_URI}zeroOrOnePath");

            /// <summary>
            /// sh:defaultValue
            /// </summary>
            public static readonly RDFResource DEFAULT_VALUE = new RDFResource($"{BASE_URI}defaultValue");

            /// <summary>
            /// sh:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource($"{BASE_URI}description");

            /// <summary>
            /// sh:group
            /// </summary>
            public static readonly RDFResource GROUP = new RDFResource($"{BASE_URI}group");

            /// <summary>
            /// sh:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource($"{BASE_URI}name");

            /// <summary>
            /// sh:order
            /// </summary>
            public static readonly RDFResource ORDER = new RDFResource($"{BASE_URI}order");

            /// <summary>
            /// sh:PropertyGroup
            /// </summary>
            public static readonly RDFResource PROPERTY_GROUP = new RDFResource($"{BASE_URI}PropertyGroup");

            /// <summary>
            /// sh:Parameterizable
            /// </summary>
            public static readonly RDFResource PARAMETERIZABLE = new RDFResource($"{BASE_URI}Parameterizable");

            /// <summary>
            /// sh:Parameter
            /// </summary>
            public static readonly RDFResource PARAMETER_CLASS = new RDFResource($"{BASE_URI}Parameter");

            /// <summary>
            /// sh:parameter
            /// </summary>
            public static readonly RDFResource PARAMETER_PROPERTY = new RDFResource($"{BASE_URI}parameter");

            /// <summary>
            /// sh:labelTemplate
            /// </summary>
            public static readonly RDFResource LABEL_TEMPLATE = new RDFResource($"{BASE_URI}labelTemplate");

            /// <summary>
            /// sh:optional
            /// </summary>
            public static readonly RDFResource OPTIONAL = new RDFResource($"{BASE_URI}optional");

            /// <summary>
            /// sh:ConstraintComponent
            /// </summary>
            public static readonly RDFResource CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}ConstraintComponent");

            /// <summary>
            /// sh:validator
            /// </summary>
            public static readonly RDFResource VALIDATOR_PROPERTY = new RDFResource($"{BASE_URI}validator");

            /// <summary>
            /// sh:nodeValidator
            /// </summary>
            public static readonly RDFResource NODE_VALIDATOR = new RDFResource($"{BASE_URI}nodeValidator");

            /// <summary>
            /// sh:propertyValidator
            /// </summary>
            public static readonly RDFResource PROPERTY_VALIDATOR = new RDFResource($"{BASE_URI}propertyValidator");

            /// <summary>
            /// sh:Validator
            /// </summary>
            public static readonly RDFResource VALIDATOR_CLASS = new RDFResource($"{BASE_URI}Validator");

            /// <summary>
            /// sh:SPARQLAskValidator
            /// </summary>
            public static readonly RDFResource SPARQL_ASK_VALIDATOR = new RDFResource($"{BASE_URI}SPARQLAskValidator");

            /// <summary>
            /// sh:SPARQLSelectValidator
            /// </summary>
            public static readonly RDFResource SPARQL_SELECT_VALIDATOR = new RDFResource($"{BASE_URI}SPARQLSelectValidator");

            /// <summary>
            /// sh:AndConstraintComponent
            /// </summary>
            public static readonly RDFResource AND_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}AndConstraintComponent");

            /// <summary>
            /// sh:AndConstraintComponent-and
            /// </summary>
            public static readonly RDFResource AND_CONSTRAINT_COMPONENT_AND = new RDFResource($"{BASE_URI}AndConstraintComponent-and");

            /// <summary>
            /// sh:and
            /// </summary>
            public static readonly RDFResource AND = new RDFResource($"{BASE_URI}and");

            /// <summary>
            /// sh:ClassConstraintComponent
            /// </summary>
            public static readonly RDFResource CLASS_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}ClassConstraintComponent");

            /// <summary>
            /// sh:ClassConstraintComponent-class
            /// </summary>
            public static readonly RDFResource CLASS_CONSTRAINT_COMPONENT_CLASS = new RDFResource($"{BASE_URI}ClassConstraintComponent-class");

            /// <summary>
            /// sh:class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource($"{BASE_URI}class");

            /// <summary>
            /// sh:ClosedConstraintComponent
            /// </summary>
            public static readonly RDFResource CLOSED_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}ClosedConstraintComponent");

            /// <summary>
            /// sh:ClosedConstraintComponent-closed
            /// </summary>
            public static readonly RDFResource CLOSED_CONSTRAINT_COMPONENT_CLOSED = new RDFResource($"{BASE_URI}ClosedConstraintComponent-closed");

            /// <summary>
            /// sh:ClosedConstraintComponent-ignoredProperties
            /// </summary>
            public static readonly RDFResource CLOSED_CONSTRAINT_COMPONENT_IGNORED_PROPERTIES = new RDFResource($"{BASE_URI}ClosedConstraintComponent-ignoredProperties");

            /// <summary>
            /// sh:closed
            /// </summary>
            public static readonly RDFResource CLOSED = new RDFResource($"{BASE_URI}closed");

            /// <summary>
            /// sh:ignoredProperties
            /// </summary>
            public static readonly RDFResource IGNORED_PROPERTIES = new RDFResource($"{BASE_URI}ignoredProperties");

            /// <summary>
            /// sh:DatatypeConstraintComponent
            /// </summary>
            public static readonly RDFResource DATATYPE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}DatatypeConstraintComponent");

            /// <summary>
            /// sh:DatatypeConstraintComponent-datatype
            /// </summary>
            public static readonly RDFResource DATATYPE_CONSTRAINT_COMPONENT_DATATYPE = new RDFResource($"{BASE_URI}DatatypeConstraintComponent-datatype");

            /// <summary>
            /// sh:datatype
            /// </summary>
            public static readonly RDFResource DATATYPE = new RDFResource($"{BASE_URI}datatype");

            /// <summary>
            /// sh:DisjointConstraintComponent
            /// </summary>
            public static readonly RDFResource DISJOINT_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}DisjointConstraintComponent");

            /// <summary>
            /// sh:DisjointConstraintComponent-disjoint
            /// </summary>
            public static readonly RDFResource DISJOINT_CONSTRAINT_COMPONENT_DISJOINT = new RDFResource($"{BASE_URI}DisjointConstraintComponent-disjoint");

            /// <summary>
            /// sh:disjoint
            /// </summary>
            public static readonly RDFResource DISJOINT = new RDFResource($"{BASE_URI}disjoint");

            /// <summary>
            /// sh:EqualsConstraintComponent
            /// </summary>
            public static readonly RDFResource EQUALS_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}EqualsConstraintComponent");

            /// <summary>
            /// sh:EqualsConstraintComponent-equals
            /// </summary>
            public static readonly RDFResource EQUALS_CONSTRAINT_COMPONENT_EQUALS = new RDFResource($"{BASE_URI}EqualsConstraintComponent-equals");

            /// <summary>
            /// sh:equals
            /// </summary>
            public static readonly RDFResource EQUALS = new RDFResource($"{BASE_URI}equals");

            /// <summary>
            /// sh:HasValueConstraintComponent
            /// </summary>
            public static readonly RDFResource HAS_VALUE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}HasValueConstraintComponent");

            /// <summary>
            /// sh:HasValueConstraintComponent-hasValue
            /// </summary>
            public static readonly RDFResource HAS_VALUE_CONSTRAINT_COMPONENT_HAS_VALUE = new RDFResource($"{BASE_URI}HasValueConstraintComponent-hasValue");

            /// <summary>
            /// sh:hasValue
            /// </summary>
            public static readonly RDFResource HAS_VALUE = new RDFResource($"{BASE_URI}hasValue");

            /// <summary>
            /// sh:InConstraintComponent
            /// </summary>
            public static readonly RDFResource IN_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}InConstraintComponent");

            /// <summary>
            /// sh:InConstraintComponent-in
            /// </summary>
            public static readonly RDFResource IN_CONSTRAINT_COMPONENT_IN = new RDFResource($"{BASE_URI}InConstraintComponent-in");

            /// <summary>
            /// sh:in
            /// </summary>
            public static readonly RDFResource IN = new RDFResource($"{BASE_URI}in");

            /// <summary>
            /// sh:LanguageInConstraintComponent
            /// </summary>
            public static readonly RDFResource LANGUAGE_IN_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}LanguageInConstraintComponent");

            /// <summary>
            /// sh:LanguageInConstraintComponent-languageIn
            /// </summary>
            public static readonly RDFResource LANGUAGE_IN_CONSTRAINT_COMPONENT_LANGUAGE_IN = new RDFResource($"{BASE_URI}LanguageInConstraintComponent-languageIn");

            /// <summary>
            /// sh:languageIn
            /// </summary>
            public static readonly RDFResource LANGUAGE_IN = new RDFResource($"{BASE_URI}languageIn");

            /// <summary>
            /// sh:LessThanConstraintComponent
            /// </summary>
            public static readonly RDFResource LESS_THAN_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}LessThanConstraintComponent");

            /// <summary>
            /// sh:LessThanConstraintComponent-lessThan
            /// </summary>
            public static readonly RDFResource LESS_THAN_CONSTRAINT_COMPONENT_LESS_THAN = new RDFResource($"{BASE_URI}LessThanConstraintComponent-lessThan");

            /// <summary>
            /// sh:lessThan
            /// </summary>
            public static readonly RDFResource LESS_THAN = new RDFResource($"{BASE_URI}lessThan");

            /// <summary>
            /// sh:LessThanOrEqualsConstraintComponent
            /// </summary>
            public static readonly RDFResource LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}LessThanOrEqualsConstraintComponent");

            /// <summary>
            /// sh:LessThanOrEqualsConstraintComponent-lessThanOrEquals
            /// </summary>
            public static readonly RDFResource LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT_LESS_THAN_OR_EQUALS = new RDFResource($"{BASE_URI}LessThanOrEqualsConstraintComponent-lessThanOrEquals");

            /// <summary>
            /// sh:lessThanOrEquals
            /// </summary>
            public static readonly RDFResource LESS_THAN_OR_EQUALS = new RDFResource($"{BASE_URI}lessThanOrEquals");

            /// <summary>
            /// sh:MaxCountConstraintComponent
            /// </summary>
            public static readonly RDFResource MAX_COUNT_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}MaxCountConstraintComponent");

            /// <summary>
            /// sh:MaxCountConstraintComponent-maxCount
            /// </summary>
            public static readonly RDFResource MAX_COUNT_CONSTRAINT_COMPONENT_MAX_COUNT = new RDFResource($"{BASE_URI}MaxCountConstraintComponent-maxCount");

            /// <summary>
            /// sh:maxCount
            /// </summary>
            public static readonly RDFResource MAX_COUNT = new RDFResource($"{BASE_URI}maxCount");

            /// <summary>
            /// sh:MaxExclusiveConstraintComponent
            /// </summary>
            public static readonly RDFResource MAX_EXCLUSIVE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}MaxExclusiveConstraintComponent");

            /// <summary>
            /// sh:MaxExclusiveConstraintComponent-maxExclusive
            /// </summary>
            public static readonly RDFResource MAX_EXCLUSIVE_CONSTRAINT_COMPONENT_MAX_EXCLUSIVE = new RDFResource($"{BASE_URI}MaxExclusiveConstraintComponent-maxExclusive");

            /// <summary>
            /// sh:maxExclusive
            /// </summary>
            public static readonly RDFResource MAX_EXCLUSIVE = new RDFResource($"{BASE_URI}maxExclusive");

            /// <summary>
            /// sh:MaxInclusiveConstraintComponent
            /// </summary>
            public static readonly RDFResource MAX_INCLUSIVE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}MaxInclusiveConstraintComponent");

            /// <summary>
            /// sh:MaxInclusiveConstraintComponent-maxInclusive
            /// </summary>
            public static readonly RDFResource MAX_INCLUSIVE_CONSTRAINT_COMPONENT_MAX_INCLUSIVE = new RDFResource($"{BASE_URI}MaxInclusiveConstraintComponent-maxInclusive");

            /// <summary>
            /// sh:maxInclusive
            /// </summary>
            public static readonly RDFResource MAX_INCLUSIVE = new RDFResource($"{BASE_URI}maxInclusive");

            /// <summary>
            /// sh:MaxLengthConstraintComponent
            /// </summary>
            public static readonly RDFResource MAX_LENGTH_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}MaxLengthConstraintComponent");

            /// <summary>
            /// sh:MaxLengthConstraintComponent-maxLength
            /// </summary>
            public static readonly RDFResource MAX_LENGTH_CONSTRAINT_COMPONENT_MAX_LENGTH = new RDFResource($"{BASE_URI}MaxLengthConstraintComponent-maxLength");

            /// <summary>
            /// sh:maxLength
            /// </summary>
            public static readonly RDFResource MAX_LENGTH = new RDFResource($"{BASE_URI}maxLength");

            /// <summary>
            /// sh:MinCountConstraintComponent
            /// </summary>
            public static readonly RDFResource MIN_COUNT_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}MinCountConstraintComponent");

            /// <summary>
            /// sh:MinCountConstraintComponent-minCount
            /// </summary>
            public static readonly RDFResource MIN_COUNT_CONSTRAINT_COMPONENT_MIN_COUNT = new RDFResource($"{BASE_URI}MinCountConstraintComponent-minCount");

            /// <summary>
            /// sh:minCount
            /// </summary>
            public static readonly RDFResource MIN_COUNT = new RDFResource($"{BASE_URI}minCount");

            /// <summary>
            /// sh:MinExclusiveConstraintComponent
            /// </summary>
            public static readonly RDFResource MIN_EXCLUSIVE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}MinExclusiveConstraintComponent");

            /// <summary>
            /// sh:MinExclusiveConstraintComponent-minExclusive
            /// </summary>
            public static readonly RDFResource MIN_EXCLUSIVE_CONSTRAINT_COMPONENT_MIN_EXCLUSIVE = new RDFResource($"{BASE_URI}MinExclusiveConstraintComponent-minExclusive");

            /// <summary>
            /// sh:minExclusive
            /// </summary>
            public static readonly RDFResource MIN_EXCLUSIVE = new RDFResource($"{BASE_URI}minExclusive");

            /// <summary>
            /// sh:MinInclusiveConstraintComponent
            /// </summary>
            public static readonly RDFResource MIN_INCLUSIVE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}MinInclusiveConstraintComponent");

            /// <summary>
            /// sh:MinInclusiveConstraintComponent-minInclusive
            /// </summary>
            public static readonly RDFResource MIN_INCLUSIVE_CONSTRAINT_COMPONENT_MIN_INCLUSIVE = new RDFResource($"{BASE_URI}MinInclusiveConstraintComponent-minInclusive");

            /// <summary>
            /// sh:minInclusive
            /// </summary>
            public static readonly RDFResource MIN_INCLUSIVE = new RDFResource($"{BASE_URI}minInclusive");

            /// <summary>
            /// sh:MinLengthConstraintComponent
            /// </summary>
            public static readonly RDFResource MIN_LENGTH_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}MinLengthConstraintComponent");

            /// <summary>
            /// sh:MinLengthConstraintComponent-minLength
            /// </summary>
            public static readonly RDFResource MIN_LENGTH_CONSTRAINT_COMPONENT_MIN_LENGTH = new RDFResource($"{BASE_URI}MinLengthConstraintComponent-minLength");

            /// <summary>
            /// sh:minLength
            /// </summary>
            public static readonly RDFResource MIN_LENGTH = new RDFResource($"{BASE_URI}minLength");

            /// <summary>
            /// sh:NodeConstraintComponent
            /// </summary>
            public static readonly RDFResource NODE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}NodeConstraintComponent");

            /// <summary>
            /// sh:NodeConstraintComponent-node
            /// </summary>
            public static readonly RDFResource NODE_CONSTRAINT_COMPONENT_NODE = new RDFResource($"{BASE_URI}NodeConstraintComponent-node");

            /// <summary>
            /// sh:node
            /// </summary>
            public static readonly RDFResource NODE = new RDFResource($"{BASE_URI}node");

            /// <summary>
            /// sh:NodeKindConstraintComponent
            /// </summary>
            public static readonly RDFResource NODE_KIND_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}NodeKindConstraintComponent");

            /// <summary>
            /// sh:NodeKindConstraintComponent-nodeKind
            /// </summary>
            public static readonly RDFResource NODE_KIND_CONSTRAINT_COMPONENT_NODE_KIND = new RDFResource($"{BASE_URI}NodeKindConstraintComponent-nodeKind");

            /// <summary>
            /// sh:nodeKind
            /// </summary>
            public static readonly RDFResource NODE_KIND = new RDFResource($"{BASE_URI}nodeKind");

            /// <summary>
            /// sh:NodeKind
            /// </summary>
            public static readonly RDFResource NODE_KIND_CLASS = new RDFResource($"{BASE_URI}NodeKind");

            /// <summary>
            /// sh:NotConstraintComponent
            /// </summary>
            public static readonly RDFResource NOT_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}NotConstraintComponent");

            /// <summary>
            /// sh:NotConstraintComponent-not
            /// </summary>
            public static readonly RDFResource NOT_CONSTRAINT_COMPONENT_NOT = new RDFResource($"{BASE_URI}NotConstraintComponent-not");

            /// <summary>
            /// sh:not
            /// </summary>
            public static readonly RDFResource NOT = new RDFResource($"{BASE_URI}not");

            /// <summary>
            /// sh:OrConstraintComponent
            /// </summary>
            public static readonly RDFResource OR_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}OrConstraintComponent");

            /// <summary>
            /// sh:OrConstraintComponent-or
            /// </summary>
            public static readonly RDFResource OR_CONSTRAINT_COMPONENT_OR = new RDFResource($"{BASE_URI}OrConstraintComponent-or");

            /// <summary>
            /// sh:or
            /// </summary>
            public static readonly RDFResource OR = new RDFResource($"{BASE_URI}or");

            /// <summary>
            /// sh:PatternConstraintComponent
            /// </summary>
            public static readonly RDFResource PATTERN_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}PatternConstraintComponent");

            /// <summary>
            /// sh:PatternConstraintComponent-pattern
            /// </summary>
            public static readonly RDFResource PATTERN_CONSTRAINT_COMPONENT_PATTERN = new RDFResource($"{BASE_URI}PatternConstraintComponent-pattern");

            /// <summary>
            /// sh:PatternConstraintComponent-flags
            /// </summary>
            public static readonly RDFResource PATTERN_CONSTRAINT_COMPONENT_FLAGS = new RDFResource($"{BASE_URI}PatternConstraintComponent-flags");

            /// <summary>
            /// sh:flags
            /// </summary>
            public static readonly RDFResource FLAGS = new RDFResource($"{BASE_URI}flags");

            /// <summary>
            /// sh:pattern
            /// </summary>
            public static readonly RDFResource PATTERN = new RDFResource($"{BASE_URI}pattern");

            /// <summary>
            /// sh:PropertyConstraintComponent
            /// </summary>
            public static readonly RDFResource PROPERTY_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}PropertyConstraintComponent");

            /// <summary>
            /// sh:PropertyConstraintComponent-property
            /// </summary>
            public static readonly RDFResource PROPERTY_CONSTRAINT_COMPONENT_PROPERTY = new RDFResource($"{BASE_URI}PropertyConstraintComponent-property");

            /// <summary>
            /// sh:property
            /// </summary>
            public static readonly RDFResource PROPERTY = new RDFResource($"{BASE_URI}property");

            /// <summary>
            /// sh:QualifiedMaxCountConstraintComponent
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}QualifiedMaxCountConstraintComponent");

            /// <summary>
            /// sh:QualifiedMaxCountConstraintComponent-qualifiedMaxCount
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_MAX_COUNT = new RDFResource($"{BASE_URI}QualifiedMaxCountConstraintComponent-qualifiedMaxCount");

            /// <summary>
            /// sh:QualifiedMaxCountConstraintComponent-qualifiedValueShape
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPE = new RDFResource($"{BASE_URI}QualifiedMaxCountConstraintComponent-qualifiedValueShape");

            /// <summary>
            /// sh:QualifiedMaxCountConstraintComponent-qualifiedValueShapesDisjoint
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPES_DISJOINT = new RDFResource($"{BASE_URI}QualifiedMaxCountConstraintComponent-qualifiedValueShapesDisjoint");

            /// <summary>
            /// sh:QualifiedMinCountConstraintComponent
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}QualifiedMinCountConstraintComponent");

            /// <summary>
            /// sh:QualifiedMinCountConstraintComponent-qualifiedMinCount
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_MIN_COUNT = new RDFResource($"{BASE_URI}QualifiedMinCountConstraintComponent-qualifiedMinCount");

            /// <summary>
            /// sh:QualifiedMinCountConstraintComponent-qualifiedValueShape
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPE = new RDFResource($"{BASE_URI}QualifiedMinCountConstraintComponent-qualifiedValueShape");

            /// <summary>
            /// sh:QualifiedMinCountConstraintComponent-qualifiedValueShapesDisjoint
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPES_DISJOINT = new RDFResource($"{BASE_URI}QualifiedMinCountConstraintComponent-qualifiedValueShapesDisjoint");

            /// <summary>
            /// sh:qualifiedMaxCount
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT = new RDFResource($"{BASE_URI}qualifiedMaxCount");

            /// <summary>
            /// sh:qualifiedMinCount
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT = new RDFResource($"{BASE_URI}qualifiedMinCount");

            /// <summary>
            /// sh:qualifiedValueShape
            /// </summary>
            public static readonly RDFResource QUALIFIED_VALUE_SHAPE = new RDFResource($"{BASE_URI}qualifiedValueShape");

            /// <summary>
            /// sh:UniqueLangConstraintComponent
            /// </summary>
            public static readonly RDFResource UNIQUE_LANG_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}UniqueLangConstraintComponent");

            /// <summary>
            /// sh:UniqueLangConstraintComponent-uniqueLang
            /// </summary>
            public static readonly RDFResource UNIQUE_LANG_CONSTRAINT_COMPONENT_UNIQUE_LANG = new RDFResource($"{BASE_URI}UniqueLangConstraintComponent-uniqueLang");

            /// <summary>
            /// sh:uniqueLang
            /// </summary>
            public static readonly RDFResource UNIQUE_LANG = new RDFResource($"{BASE_URI}uniqueLang");

            /// <summary>
            /// sh:XoneConstraintComponent
            /// </summary>
            public static readonly RDFResource XONE_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}XoneConstraintComponent");

            /// <summary>
            /// sh:XoneConstraintComponent-xone
            /// </summary>
            public static readonly RDFResource XONE_CONSTRAINT_COMPONENT_XONE = new RDFResource($"{BASE_URI}XoneConstraintComponent-xone");

            /// <summary>
            /// sh:xone
            /// </summary>
            public static readonly RDFResource XONE = new RDFResource($"{BASE_URI}xone");

            /// <summary>
            /// sh:SPARQLExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_EXECUTABLE = new RDFResource($"{BASE_URI}SPARQLExecutable");

            /// <summary>
            /// sh:SPARQLAskExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_ASK_EXECUTABLE = new RDFResource($"{BASE_URI}SPARQLAskExecutable");

            /// <summary>
            /// sh:ask
            /// </summary>
            public static readonly RDFResource ASK = new RDFResource($"{BASE_URI}ask");

            /// <summary>
            /// sh:SPARQLConstructExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_CONSTRUCT_EXECUTABLE = new RDFResource($"{BASE_URI}SPARQLConstructExecutable");

            /// <summary>
            /// sh:construct
            /// </summary>
            public static readonly RDFResource CONSTRUCT = new RDFResource($"{BASE_URI}construct");

            /// <summary>
            /// sh:SPARQLSelectExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_SELECT_EXECUTABLE = new RDFResource($"{BASE_URI}SPARQLSelectExecutable");

            /// <summary>
            /// sh:select
            /// </summary>
            public static readonly RDFResource SELECT = new RDFResource($"{BASE_URI}select");

            /// <summary>
            /// sh:SPARQLUpdateExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_UPDATE_EXECUTABLE = new RDFResource($"{BASE_URI}SPARQLUpdateExecutable");

            /// <summary>
            /// sh:update
            /// </summary>
            public static readonly RDFResource UPDATE = new RDFResource($"{BASE_URI}update");

            /// <summary>
            /// sh:prefixes
            /// </summary>
            public static readonly RDFResource PREFIXES = new RDFResource($"{BASE_URI}prefixes");

            /// <summary>
            /// sh:PrefixDeclaration
            /// </summary>
            public static readonly RDFResource PREFIX_DECLARATION = new RDFResource($"{BASE_URI}PrefixDeclaration");

            /// <summary>
            /// sh:declare
            /// </summary>
            public static readonly RDFResource DECLARE = new RDFResource($"{BASE_URI}declare");

            /// <summary>
            /// sh:prefix
            /// </summary>
            public static readonly RDFResource PREFIX_PROPERTY = new RDFResource($"{BASE_URI}prefix");

            /// <summary>
            /// sh:namespace
            /// </summary>
            public static readonly RDFResource NAMESPACE = new RDFResource($"{BASE_URI}namespace");

            /// <summary>
            /// sh:SPARQLConstraintComponent
            /// </summary>
            public static readonly RDFResource SPARQL_CONSTRAINT_COMPONENT = new RDFResource($"{BASE_URI}SPARQLConstraintComponent");

            /// <summary>
            /// sh:SPARQLConstraintComponent-sparql
            /// </summary>
            public static readonly RDFResource SPARQL_CONSTRAINT_COMPONENT_SPARQL = new RDFResource($"{BASE_URI}SPARQLConstraintComponent-sparql");

            /// <summary>
            /// sh:sparql
            /// </summary>
            public static readonly RDFResource SPARQL = new RDFResource($"{BASE_URI}sparql");

            /// <summary>
            /// sh:SPARQLConstraint
            /// </summary>
            public static readonly RDFResource SPARQL_CONSTRAINT = new RDFResource($"{BASE_URI}SPARQLConstraint");
            #endregion
        }
        #endregion

        #region SKOS
        /// <summary>
        /// SKOS represents the W3C SKOS vocabulary (with SKOS-XL extensions)
        /// </summary>
        public static class SKOS
        {
            #region Properties
            /// <summary>
            /// skos
            /// </summary>
            public static readonly string PREFIX = "skos";

            /// <summary>
            /// http://www.w3.org/2004/02/skos/core#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2004/02/skos/core#";

            /// <summary>
            /// http://www.w3.org/2004/02/skos/core#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2004/02/skos/core#";

            /// <summary>
            /// skos:Concept
            /// </summary>
            public static readonly RDFResource CONCEPT = new RDFResource($"{BASE_URI}Concept");

            /// <summary>
            /// skos:ConceptScheme
            /// </summary>
            public static readonly RDFResource CONCEPT_SCHEME = new RDFResource($"{BASE_URI}ConceptScheme");

            /// <summary>
            /// skos:inScheme
            /// </summary>
            public static readonly RDFResource IN_SCHEME = new RDFResource($"{BASE_URI}inScheme");

            /// <summary>
            /// skos:hasTopConcept
            /// </summary>
            public static readonly RDFResource HAS_TOP_CONCEPT = new RDFResource($"{BASE_URI}hasTopConcept");

            /// <summary>
            /// skos:topConceptOf
            /// </summary>
            public static readonly RDFResource TOP_CONCEPT_OF = new RDFResource($"{BASE_URI}topConceptOf");

            /// <summary>
            /// skos:altLabel
            /// </summary>
            public static readonly RDFResource ALT_LABEL = new RDFResource($"{BASE_URI}altLabel");

            /// <summary>
            /// skos:hiddenLabel
            /// </summary>
            public static readonly RDFResource HIDDEN_LABEL = new RDFResource($"{BASE_URI}hiddenLabel");

            /// <summary>
            /// skos:prefLabel
            /// </summary>
            public static readonly RDFResource PREF_LABEL = new RDFResource($"{BASE_URI}prefLabel");

            /// <summary>
            /// skos:notation
            /// </summary>
            public static readonly RDFResource NOTATION = new RDFResource($"{BASE_URI}notation");

            /// <summary>
            /// skos:changeNote
            /// </summary>
            public static readonly RDFResource CHANGE_NOTE = new RDFResource($"{BASE_URI}changeNote");

            /// <summary>
            /// skos:definition
            /// </summary>
            public static readonly RDFResource DEFINITION = new RDFResource($"{BASE_URI}definition");

            /// <summary>
            /// skos:example
            /// </summary>
            public static readonly RDFResource EXAMPLE = new RDFResource($"{BASE_URI}example");

            /// <summary>
            /// skos:editorialNote
            /// </summary>
            public static readonly RDFResource EDITORIAL_NOTE = new RDFResource($"{BASE_URI}editorialNote");

            /// <summary>
            /// skos:historyNote
            /// </summary>
            public static readonly RDFResource HISTORY_NOTE = new RDFResource($"{BASE_URI}historyNote");

            /// <summary>
            /// skos:note
            /// </summary>
            public static readonly RDFResource NOTE = new RDFResource($"{BASE_URI}note");

            /// <summary>
            /// skos:scopeNote
            /// </summary>
            public static readonly RDFResource SCOPE_NOTE = new RDFResource($"{BASE_URI}scopeNote");

            /// <summary>
            /// skos:broader
            /// </summary>
            public static readonly RDFResource BROADER = new RDFResource($"{BASE_URI}broader");

            /// <summary>
            /// skos:broaderTransitive
            /// </summary>
            public static readonly RDFResource BROADER_TRANSITIVE = new RDFResource($"{BASE_URI}broaderTransitive");

            /// <summary>
            /// skos:narrower
            /// </summary>
            public static readonly RDFResource NARROWER = new RDFResource($"{BASE_URI}narrower");

            /// <summary>
            /// skos:narrowerTransitive
            /// </summary>
            public static readonly RDFResource NARROWER_TRANSITIVE = new RDFResource($"{BASE_URI}narrowerTransitive");

            /// <summary>
            /// skos:related
            /// </summary>
            public static readonly RDFResource RELATED = new RDFResource($"{BASE_URI}related");

            /// <summary>
            /// skos:semanticRelation
            /// </summary>
            public static readonly RDFResource SEMANTIC_RELATION = new RDFResource($"{BASE_URI}semanticRelation");

            /// <summary>
            /// skos:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource($"{BASE_URI}subject");

            /// <summary>
            /// skos:Collection
            /// </summary>
            public static readonly RDFResource COLLECTION = new RDFResource($"{BASE_URI}Collection");

            /// <summary>
            /// skos:OrderedCollection
            /// </summary>
            public static readonly RDFResource ORDERED_COLLECTION = new RDFResource($"{BASE_URI}OrderedCollection");

            /// <summary>
            /// skos:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource($"{BASE_URI}member");

            /// <summary>
            /// skos:memberList
            /// </summary>
            public static readonly RDFResource MEMBER_LIST = new RDFResource($"{BASE_URI}memberList");

            /// <summary>
            /// skos:broadMatch
            /// </summary>
            public static readonly RDFResource BROAD_MATCH = new RDFResource($"{BASE_URI}broadMatch");

            /// <summary>
            /// skos:closeMatch
            /// </summary>
            public static readonly RDFResource CLOSE_MATCH = new RDFResource($"{BASE_URI}closeMatch");

            /// <summary>
            /// skos:narrowMatch
            /// </summary>
            public static readonly RDFResource NARROW_MATCH = new RDFResource($"{BASE_URI}narrowMatch");

            /// <summary>
            /// skos:relatedMatch
            /// </summary>
            public static readonly RDFResource RELATED_MATCH = new RDFResource($"{BASE_URI}relatedMatch");

            /// <summary>
            /// skos:exactMatch
            /// </summary>
            public static readonly RDFResource EXACT_MATCH = new RDFResource($"{BASE_URI}exactMatch");

            /// <summary>
            /// skos:mappingRelation
            /// </summary>
            public static readonly RDFResource MAPPING_RELATION = new RDFResource($"{BASE_URI}mappingRelation");
            #endregion

            #region Extended Properties
            /// <summary>
            /// SKOS-XL extensions
            /// </summary>
            public static class SKOSXL
            {
                #region Properties
                /// <summary>
                /// skosxl
                /// </summary>
                public static readonly string PREFIX = "skosxl";

                /// <summary>
                /// http://www.w3.org/2008/05/skos-xl#
                /// </summary>
                public static readonly string BASE_URI = "http://www.w3.org/2008/05/skos-xl#";

                /// <summary>
                /// http://www.w3.org/2008/05/skos-xl#
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://www.w3.org/2008/05/skos-xl#";

                /// <summary>
                /// skosxl:Label
                /// </summary>
                public static readonly RDFResource LABEL = new RDFResource($"{BASE_URI}Label");

                /// <summary>
                /// skosxl:altLabel
                /// </summary>
                public static readonly RDFResource ALT_LABEL = new RDFResource($"{BASE_URI}altLabel");

                /// <summary>
                /// skosxl:hiddenLabel
                /// </summary>
                public static readonly RDFResource HIDDEN_LABEL = new RDFResource($"{BASE_URI}hiddenLabel");

                /// <summary>
                /// skosxl:labelRelation
                /// </summary>
                public static readonly RDFResource LABEL_RELATION = new RDFResource($"{BASE_URI}labelRelation");

                /// <summary>
                /// skosxl:literalForm
                /// </summary>
                public static readonly RDFResource LITERAL_FORM = new RDFResource($"{BASE_URI}literalForm");

                /// <summary>
                /// skosxl:prefLabel
                /// </summary>
                public static readonly RDFResource PREF_LABEL = new RDFResource($"{BASE_URI}prefLabel");
                #endregion
            }
            #endregion
        }
        #endregion

        #region SWRL
        /// <summary>
        /// SWRL represents the W3C Semantic Web Rule Language vocabulary.
        /// </summary>
        public static class SWRL
        {
            #region Properties
            /// <summary>
            /// swrl
            /// </summary>
            public static readonly string PREFIX = "swrl";

            /// <summary>
            /// http://www.w3.org/2003/11/swrl#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2003/11/swrl#";

            /// <summary>
            /// http://www.w3.org/2003/11/swrl#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2003/11/swrl#";

            /// <summary>
            /// swrl:Imp
            /// </summary>
            public static readonly RDFResource IMP = new RDFResource($"{BASE_URI}Imp");

            /// <summary>
            /// swrl:head
            /// </summary>
            public static readonly RDFResource HEAD = new RDFResource($"{BASE_URI}head");

            /// <summary>
            /// swrl:body
            /// </summary>
            public static readonly RDFResource BODY = new RDFResource($"{BASE_URI}body");

            /// <summary>
            /// swrl:Variable
            /// </summary>
            public static readonly RDFResource VARIABLE = new RDFResource($"{BASE_URI}Variable");

            /// <summary>
            /// swrl:Atom
            /// </summary>
            public static readonly RDFResource ATOM = new RDFResource($"{BASE_URI}Atom");

            /// <summary>
            /// swrl:AtomList
            /// </summary>
            public static readonly RDFResource ATOMLIST = new RDFResource($"{BASE_URI}AtomList");

            /// <summary>
            /// swrl:Builtin
            /// </summary>
            public static readonly RDFResource BUILTIN_CLS = new RDFResource($"{BASE_URI}Builtin");

            /// <summary>
            /// swrl:argument1
            /// </summary>
            public static readonly RDFResource ARGUMENT1 = new RDFResource($"{BASE_URI}argument1");

            /// <summary>
            /// swrl:argument2
            /// </summary>
            public static readonly RDFResource ARGUMENT2 = new RDFResource($"{BASE_URI}argument2");

            /// <summary>
            /// swrl:arguments
            /// </summary>
            public static readonly RDFResource ARGUMENTS = new RDFResource($"{BASE_URI}arguments");

            /// <summary>
            /// swrl:classPredicate
            /// </summary>
            public static readonly RDFResource CLASS_PREDICATE = new RDFResource($"{BASE_URI}classPredicate");

            /// <summary>
            /// swrl:propertyPredicate
            /// </summary>
            public static readonly RDFResource PROPERTY_PREDICATE = new RDFResource($"{BASE_URI}propertyPredicate");

            /// <summary>
            /// swrl:dataRange
            /// </summary>
            public static readonly RDFResource DATARANGE = new RDFResource($"{BASE_URI}dataRange");

            /// <summary>
            /// swrl:builtin
            /// </summary>
            public static readonly RDFResource BUILTIN_PROP = new RDFResource($"{BASE_URI}builtin");

            /// <summary>
            /// swrl:ClassAtom
            /// </summary>
            public static readonly RDFResource CLASS_ATOM = new RDFResource($"{BASE_URI}ClassAtom");

            /// <summary>
            /// swrl:IndividualPropertyAtom
            /// </summary>
            public static readonly RDFResource INDIVIDUAL_PROPERTY_ATOM = new RDFResource($"{BASE_URI}IndividualPropertyAtom");

            /// <summary>
            /// swrl:DatavaluedPropertyAtom
            /// </summary>
            public static readonly RDFResource DATAVALUED_PROPERTY_ATOM = new RDFResource($"{BASE_URI}DatavaluedPropertyAtom");

            /// <summary>
            /// swrl:SameIndividualAtom
            /// </summary>
            public static readonly RDFResource SAME_INDIVIDUAL_ATOM = new RDFResource($"{BASE_URI}SameIndividualAtom");

            /// <summary>
            /// swrl:DifferentIndividualsAtom
            /// </summary>
            public static readonly RDFResource DIFFERENT_INDIVIDUALS_ATOM = new RDFResource($"{BASE_URI}DifferentIndividualsAtom");

            /// <summary>
            /// swrl:DataRangeAtom
            /// </summary>
            public static readonly RDFResource DATARANGE_ATOM = new RDFResource($"{BASE_URI}DataRangeAtom");

            /// <summary>
            /// swrl:BuiltinAtom
            /// </summary>
            public static readonly RDFResource BUILTIN_ATOM = new RDFResource($"{BASE_URI}BuiltinAtom");
            #endregion

            #region Extended Properties
            /// <summary>
            /// SWRLB represents the W3C Semantic Web Rule Language - BuiltIns vocabulary.
            /// </summary>
            public static class SWRLB
            {
                #region Properties
                /// <summary>
                /// swrlb
                /// </summary>
                public static readonly string PREFIX = "swrlb";

                /// <summary>
                /// http://www.w3.org/2003/11/swrlb#
                /// </summary>
                public static readonly string BASE_URI = "http://www.w3.org/2003/11/swrlb#";

                /// <summary>
                /// http://www.w3.org/2003/11/swrlb#
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://www.w3.org/2003/11/swrlb#";
                #endregion
            }
            #endregion
        }
        #endregion

        #region TIME
        /// <summary>
        /// TIME represents the OWL-Time vocabulary
        /// </summary>
        public static class TIME
        {
            #region Properties
            /// <summary>
            /// time
            /// </summary>
            public static readonly string PREFIX = "time";

            /// <summary>
            /// http://www.w3.org/2006/time#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/2006/time#";

            /// <summary>
            /// http://www.w3.org/2006/time#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/2006/time#";

            /// <summary>
            /// time:DateTimeDescription
            /// </summary>
            public static readonly RDFResource DATETIME_DESCRIPTION = new RDFResource($"{BASE_URI}DateTimeDescription");

            /// <summary>
            /// time:DateTimeInterval
            /// </summary>
            public static readonly RDFResource DATETIME_INTERVAL = new RDFResource($"{BASE_URI}DateTimeInterval");

            /// <summary>
            /// time:DayOfWeek
            /// </summary>
            public static readonly RDFResource DAY_OF_WEEK_CLASS = new RDFResource($"{BASE_URI}DayOfWeek");

            /// <summary>
            /// time:Duration
            /// </summary>
            public static readonly RDFResource DURATION = new RDFResource($"{BASE_URI}Duration");

            /// <summary>
            /// time:DurationDescription
            /// </summary>
            public static readonly RDFResource DURATION_DESCRIPTION = new RDFResource($"{BASE_URI}DurationDescription");

            /// <summary>
            /// time:GeneralDateTimeDescription
            /// </summary>
            public static readonly RDFResource GENERAL_DATETIME_DESCRIPTION = new RDFResource($"{BASE_URI}GeneralDateTimeDescription");

            /// <summary>
            /// time:GeneralDurationDescription
            /// </summary>
            public static readonly RDFResource GENERAL_DURATION_DESCRIPTION = new RDFResource($"{BASE_URI}GeneralDurationDescription");

            /// <summary>
            /// time:Instant
            /// </summary>
            public static readonly RDFResource INSTANT = new RDFResource($"{BASE_URI}Instant");

            /// <summary>
            /// time:Interval
            /// </summary>
            public static readonly RDFResource INTERVAL = new RDFResource($"{BASE_URI}Interval");

            /// <summary>
            /// time:MonthOfYear
            /// </summary>
            public static readonly RDFResource MONTH_OF_YEAR_CLASS = new RDFResource($"{BASE_URI}MonthOfYear");

            /// <summary>
            /// time:ProperInterval
            /// </summary>
            public static readonly RDFResource PROPER_INTERVAL = new RDFResource($"{BASE_URI}ProperInterval");

            /// <summary>
            /// time:TemporalDuration
            /// </summary>
            public static readonly RDFResource TEMPORAL_DURATION = new RDFResource($"{BASE_URI}TemporalDuration");

            /// <summary>
            /// time:TemporalEntity
            /// </summary>
            public static readonly RDFResource TEMPORAL_ENTITY = new RDFResource($"{BASE_URI}TemporalEntity");

            /// <summary>
            /// time:TemporalPosition
            /// </summary>
            public static readonly RDFResource TEMPORAL_POSITION = new RDFResource($"{BASE_URI}TemporalPosition");

            /// <summary>
            /// time:TemporalUnit
            /// </summary>
            public static readonly RDFResource TEMPORAL_UNIT = new RDFResource($"{BASE_URI}TemporalUnit");

            /// <summary>
            /// time:TimePosition
            /// </summary>
            public static readonly RDFResource TIME_POSITION = new RDFResource($"{BASE_URI}TimePosition");

            /// <summary>
            /// time:TimeZone
            /// </summary>
            public static readonly RDFResource TIMEZONE_CLASS = new RDFResource($"{BASE_URI}TimeZone");

            /// <summary>
            /// time:TRS
            /// </summary>
            public static readonly RDFResource TRS = new RDFResource($"{BASE_URI}TRS");

            /// <summary>
            /// time:after
            /// </summary>
            public static readonly RDFResource AFTER = new RDFResource($"{BASE_URI}after");

            /// <summary>
            /// time:before
            /// </summary>
            public static readonly RDFResource BEFORE = new RDFResource($"{BASE_URI}before");

            /// <summary>
            /// time:day
            /// </summary>
            public static readonly RDFResource DAY = new RDFResource($"{BASE_URI}day");

            /// <summary>
            /// time:dayOfWeek
            /// </summary>
            public static readonly RDFResource DAY_OF_WEEK = new RDFResource($"{BASE_URI}dayOfWeek");

            /// <summary>
            /// time:dayOfYear
            /// </summary>
            public static readonly RDFResource DAY_OF_YEAR = new RDFResource($"{BASE_URI}dayOfYear");

            /// <summary>
            /// time:days
            /// </summary>
            public static readonly RDFResource DAYS = new RDFResource($"{BASE_URI}days");

            /// <summary>
            /// time:disjoint
            /// </summary>
            public static readonly RDFResource DISJOINT = new RDFResource($"{BASE_URI}disjoint");

            /// <summary>
            /// time:equals
            /// </summary>
            public static readonly RDFResource EQUALS = new RDFResource($"{BASE_URI}equals");

            /// <summary>
            /// time:hasBeginning
            /// </summary>
            public static readonly RDFResource HAS_BEGINNING = new RDFResource($"{BASE_URI}hasBeginning");

            /// <summary>
            /// time:hasDateTimeDescription
            /// </summary>
            public static readonly RDFResource HAS_DATETIME_DESCRIPTION = new RDFResource($"{BASE_URI}hasDateTimeDescription");

            /// <summary>
            /// time:hasDuration
            /// </summary>
            public static readonly RDFResource HAS_DURATION = new RDFResource($"{BASE_URI}hasDuration");

            /// <summary>
            /// time:hasDurationDescription
            /// </summary>
            public static readonly RDFResource HAS_DURATION_DESCRIPTION = new RDFResource($"{BASE_URI}hasDurationDescription");

            /// <summary>
            /// time:hasEnd
            /// </summary>
            public static readonly RDFResource HAS_END = new RDFResource($"{BASE_URI}hasEnd");

            /// <summary>
            /// time:hasInside
            /// </summary>
            public static readonly RDFResource HAS_INSIDE = new RDFResource($"{BASE_URI}hasInside");

            /// <summary>
            /// time:hasTemporalDuration
            /// </summary>
            public static readonly RDFResource HAS_TEMPORAL_DURATION = new RDFResource($"{BASE_URI}hasTemporalDuration");

            /// <summary>
            /// time:hasTime
            /// </summary>
            public static readonly RDFResource HAS_TIME = new RDFResource($"{BASE_URI}hasTime");

            /// <summary>
            /// time:hasTRS
            /// </summary>
            public static readonly RDFResource HAS_TRS = new RDFResource($"{BASE_URI}hasTRS");

            /// <summary>
            /// time:hasXSDDuration
            /// </summary>
            public static readonly RDFResource HAS_XSD_DURATION = new RDFResource($"{BASE_URI}hasXSDDuration");

            /// <summary>
            /// time:hour
            /// </summary>
            public static readonly RDFResource HOUR = new RDFResource($"{BASE_URI}hour");

            /// <summary>
            /// time:hours
            /// </summary>
            public static readonly RDFResource HOURS = new RDFResource($"{BASE_URI}hours");

            /// <summary>
            /// time:inDateTime
            /// </summary>
            public static readonly RDFResource IN_DATETIME = new RDFResource($"{BASE_URI}inDateTime");

            /// <summary>
            /// time:inside
            /// </summary>
            public static readonly RDFResource INSIDE = new RDFResource($"{BASE_URI}inside");

            /// <summary>
            /// time:inTemporalPosition
            /// </summary>
            public static readonly RDFResource IN_TEMPORAL_POSITION = new RDFResource($"{BASE_URI}inTemporalPosition");

            /// <summary>
            /// time:intervalAfter
            /// </summary>
            public static readonly RDFResource INTERVAL_AFTER = new RDFResource($"{BASE_URI}intervalAfter");

            /// <summary>
            /// time:intervalBefore
            /// </summary>
            public static readonly RDFResource INTERVAL_BEFORE = new RDFResource($"{BASE_URI}intervalBefore");

            /// <summary>
            /// time:intervalContains
            /// </summary>
            public static readonly RDFResource INTERVAL_CONTAINS = new RDFResource($"{BASE_URI}intervalContains");

            /// <summary>
            /// time:intervalDisjoint
            /// </summary>
            public static readonly RDFResource INTERVAL_DISJOINT = new RDFResource($"{BASE_URI}intervalDisjoint");

            /// <summary>
            /// time:intervalDuring
            /// </summary>
            public static readonly RDFResource INTERVAL_DURING = new RDFResource($"{BASE_URI}intervalDuring");

            /// <summary>
            /// time:intervalEquals
            /// </summary>
            public static readonly RDFResource INTERVAL_EQUALS = new RDFResource($"{BASE_URI}intervalEquals");

            /// <summary>
            /// time:intervalFinishedBy
            /// </summary>
            public static readonly RDFResource INTERVAL_FINISHED_BY = new RDFResource($"{BASE_URI}intervalFinishedBy");

            /// <summary>
            /// time:intervalFinishes
            /// </summary>
            public static readonly RDFResource INTERVAL_FINISHES = new RDFResource($"{BASE_URI}intervalFinishes");

            /// <summary>
            /// time:intervalIn
            /// </summary>
            public static readonly RDFResource INTERVAL_IN = new RDFResource($"{BASE_URI}intervalIn");

            /// <summary>
            /// time:intervalMeets
            /// </summary>
            public static readonly RDFResource INTERVAL_MEETS = new RDFResource($"{BASE_URI}intervalMeets");

            /// <summary>
            /// time:intervalMetBy
            /// </summary>
            public static readonly RDFResource INTERVAL_MET_BY = new RDFResource($"{BASE_URI}intervalMetBy");

            /// <summary>
            /// time:intervalOverlappedBy
            /// </summary>
            public static readonly RDFResource INTERVAL_OVERLAPPED_BY = new RDFResource($"{BASE_URI}intervalOverlappedBy");

            /// <summary>
            /// time:intervalOverlaps
            /// </summary>
            public static readonly RDFResource INTERVAL_OVERLAPS = new RDFResource($"{BASE_URI}intervalOverlaps");

            /// <summary>
            /// time:intervalStartedBy
            /// </summary>
            public static readonly RDFResource INTERVAL_STARTED_BY = new RDFResource($"{BASE_URI}intervalStartedBy");

            /// <summary>
            /// time:intervalStarts
            /// </summary>
            public static readonly RDFResource INTERVAL_STARTS = new RDFResource($"{BASE_URI}intervalStarts");

            /// <summary>
            /// time:inTimePosition
            /// </summary>
            public static readonly RDFResource IN_TIME_POSITION = new RDFResource($"{BASE_URI}inTimePosition");

            /// <summary>
            /// time:inXSDDate
            /// </summary>
            public static readonly RDFResource IN_XSD_DATE = new RDFResource($"{BASE_URI}inXSDDate");

            /// <summary>
            /// time:inXSDDateTime
            /// </summary>
            public static readonly RDFResource IN_XSD_DATETIME = new RDFResource($"{BASE_URI}inXSDDateTime");

            /// <summary>
            /// time:inXSDDateTimeStamp
            /// </summary>
            public static readonly RDFResource IN_XSD_DATETIMESTAMP = new RDFResource($"{BASE_URI}inXSDDateTimeStamp");

            /// <summary>
            /// time:inXSDgYear
            /// </summary>
            public static readonly RDFResource IN_XSD_GYEAR = new RDFResource($"{BASE_URI}inXSDgYear");

            /// <summary>
            /// time:inXSDgYearMonth
            /// </summary>
            public static readonly RDFResource IN_XSD_GYEARMONTH = new RDFResource($"{BASE_URI}inXSDgYearMonth");

            /// <summary>
            /// time:minute
            /// </summary>
            public static readonly RDFResource MINUTE = new RDFResource($"{BASE_URI}minute");

            /// <summary>
            /// time:minutes
            /// </summary>
            public static readonly RDFResource MINUTES = new RDFResource($"{BASE_URI}minutes");

            /// <summary>
            /// time:month
            /// </summary>
            public static readonly RDFResource MONTH = new RDFResource($"{BASE_URI}month");

            /// <summary>
            /// time:monthOfYear
            /// </summary>
            public static readonly RDFResource MONTH_OF_YEAR = new RDFResource($"{BASE_URI}monthOfYear");

            /// <summary>
            /// time:months
            /// </summary>
            public static readonly RDFResource MONTHS = new RDFResource($"{BASE_URI}months");

            /// <summary>
            /// time:nominalPosition
            /// </summary>
            public static readonly RDFResource NOMINAL_POSITION = new RDFResource($"{BASE_URI}nominalPosition");

            /// <summary>
            /// time:notDisjoint
            /// </summary>
            public static readonly RDFResource NOT_DISJOINT = new RDFResource($"{BASE_URI}notDisjoint");

            /// <summary>
            /// time:numericDuration
            /// </summary>
            public static readonly RDFResource NUMERIC_DURATION = new RDFResource($"{BASE_URI}numericDuration");

            /// <summary>
            /// time:numericPosition
            /// </summary>
            public static readonly RDFResource NUMERIC_POSITION = new RDFResource($"{BASE_URI}numericPosition");

            /// <summary>
            /// time:second
            /// </summary>
            public static readonly RDFResource SECOND = new RDFResource($"{BASE_URI}second");

            /// <summary>
            /// time:seconds
            /// </summary>
            public static readonly RDFResource SECONDS = new RDFResource($"{BASE_URI}seconds");

            /// <summary>
            /// time:timeZone
            /// </summary>
            public static readonly RDFResource TIMEZONE = new RDFResource($"{BASE_URI}timeZone");

            /// <summary>
            /// time:unitType
            /// </summary>
            public static readonly RDFResource UNIT_TYPE = new RDFResource($"{BASE_URI}unitType");

            /// <summary>
            /// time:week
            /// </summary>
            public static readonly RDFResource WEEK = new RDFResource($"{BASE_URI}week");

            /// <summary>
            /// time:weeks
            /// </summary>
            public static readonly RDFResource WEEKS = new RDFResource($"{BASE_URI}weeks");

            /// <summary>
            /// time:xsdDateTime
            /// </summary>
            public static readonly RDFResource XSD_DATETIME = new RDFResource($"{BASE_URI}xsdDateTime");

            /// <summary>
            /// time:year
            /// </summary>
            public static readonly RDFResource YEAR = new RDFResource($"{BASE_URI}year");

            /// <summary>
            /// time:years
            /// </summary>
            public static readonly RDFResource YEARS = new RDFResource($"{BASE_URI}years");

            /// <summary>
            /// time:generalDay
            /// </summary>
            public static readonly RDFResource GENERAL_DAY = new RDFResource($"{BASE_URI}generalDay");

            /// <summary>
            /// time:generalMonth
            /// </summary>
            public static readonly RDFResource GENERAL_MONTH = new RDFResource($"{BASE_URI}generalMonth");

            /// <summary>
            /// time:generalYear
            /// </summary>
            public static readonly RDFResource GENERAL_YEAR = new RDFResource($"{BASE_URI}generalYear");

            /// <summary>
            /// time:Friday
            /// </summary>
            public static readonly RDFResource FRIDAY = new RDFResource($"{BASE_URI}Friday");

            /// <summary>
            /// time:Monday
            /// </summary>
            public static readonly RDFResource MONDAY = new RDFResource($"{BASE_URI}Monday");

            /// <summary>
            /// time:Saturday
            /// </summary>
            public static readonly RDFResource SATURDAY = new RDFResource($"{BASE_URI}Saturday");

            /// <summary>
            /// time:Sunday
            /// </summary>
            public static readonly RDFResource SUNDAY = new RDFResource($"{BASE_URI}Sunday");

            /// <summary>
            /// time:Thursday
            /// </summary>
            public static readonly RDFResource THURSDAY = new RDFResource($"{BASE_URI}Thursday");

            /// <summary>
            /// time:Tuesday
            /// </summary>
            public static readonly RDFResource TUESDAY = new RDFResource($"{BASE_URI}Tuesday");

            /// <summary>
            /// time:Wednesday
            /// </summary>
            public static readonly RDFResource WEDNESDAY = new RDFResource($"{BASE_URI}Wednesday");

            /// <summary>
            /// time:unitCentury
            /// </summary>
            public static readonly RDFResource UNIT_CENTURY = new RDFResource($"{BASE_URI}unitCentury");

            /// <summary>
            /// time:unitDay
            /// </summary>
            public static readonly RDFResource UNIT_DAY = new RDFResource($"{BASE_URI}unitDay");

            /// <summary>
            /// time:unitDecade
            /// </summary>
            public static readonly RDFResource UNIT_DECADE = new RDFResource($"{BASE_URI}unitDecade");

            /// <summary>
            /// time:unitHour
            /// </summary>
            public static readonly RDFResource UNIT_HOUR = new RDFResource($"{BASE_URI}unitHour");

            /// <summary>
            /// time:unitMillenium
            /// </summary>
            public static readonly RDFResource UNIT_MILLENIUM = new RDFResource($"{BASE_URI}unitMillenium");

            /// <summary>
            /// time:unitMinute
            /// </summary>
            public static readonly RDFResource UNIT_MINUTE = new RDFResource($"{BASE_URI}unitMinute");

            /// <summary>
            /// time:unitMonth
            /// </summary>
            public static readonly RDFResource UNIT_MONTH = new RDFResource($"{BASE_URI}unitMonth");

            /// <summary>
            /// time:unitSecond
            /// </summary>
            public static readonly RDFResource UNIT_SECOND = new RDFResource($"{BASE_URI}unitSecond");

            /// <summary>
            /// time:unitWeek
            /// </summary>
            public static readonly RDFResource UNIT_WEEK = new RDFResource($"{BASE_URI}unitWeek");

            /// <summary>
            /// time:unitYear
            /// </summary>
            public static readonly RDFResource UNIT_YEAR = new RDFResource($"{BASE_URI}unitYear");
            #endregion

            #region Extended Properties
            /// <summary>
            /// GREG represents the TIME extension for Gregorian calendar
            /// </summary>
            public static class GREG
            {
                /// <summary>
                /// greg
                /// </summary>
                public static readonly string PREFIX = "greg";

                /// <summary>
                /// http://www.w3.org/ns/time/gregorian#
                /// </summary>
                public static readonly string BASE_URI = "http://www.w3.org/ns/time/gregorian#";

                /// <summary>
                /// http://www.w3.org/2006/time#
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://www.w3.org/ns/time/gregorian#";

                /// <summary>
                /// greg:January
                /// </summary>
                public static readonly RDFResource JANUARY = new RDFResource($"{BASE_URI}January");

                /// <summary>
                /// greg:February
                /// </summary>
                public static readonly RDFResource FEBRUARY = new RDFResource($"{BASE_URI}February");

                /// <summary>
                /// greg:March
                /// </summary>
                public static readonly RDFResource MARCH = new RDFResource($"{BASE_URI}March");

                /// <summary>
                /// greg:April
                /// </summary>
                public static readonly RDFResource APRIL = new RDFResource($"{BASE_URI}April");

                /// <summary>
                /// greg:May
                /// </summary>
                public static readonly RDFResource MAY = new RDFResource($"{BASE_URI}May");

                /// <summary>
                /// greg:June
                /// </summary>
                public static readonly RDFResource JUNE = new RDFResource($"{BASE_URI}June");

                /// <summary>
                /// greg:July
                /// </summary>
                public static readonly RDFResource JULY = new RDFResource($"{BASE_URI}July");

                /// <summary>
                /// greg:August
                /// </summary>
                public static readonly RDFResource AUGUST = new RDFResource($"{BASE_URI}August");

                /// <summary>
                /// greg:September
                /// </summary>
                public static readonly RDFResource SEPTEMBER = new RDFResource($"{BASE_URI}September");

                /// <summary>
                /// greg:October
                /// </summary>
                public static readonly RDFResource OCTOBER = new RDFResource($"{BASE_URI}October");

                /// <summary>
                /// greg:November
                /// </summary>
                public static readonly RDFResource NOVEMBER = new RDFResource($"{BASE_URI}November");

                /// <summary>
                /// greg:December
                /// </summary>
                public static readonly RDFResource DECEMBER = new RDFResource($"{BASE_URI}December");
            }

            /// <summary>
            /// THORS represents the TIME extension for modeling temporal hierarchical ordinal reference systems
            /// </summary>
            public static class THORS
            {
                /// <summary>
                /// thors
                /// </summary>
                public static readonly string PREFIX = "thors";

                /// <summary>
                /// http://resource.geosciml.org/ontology/timescale/thors#
                /// </summary>
                public static readonly string BASE_URI = "http://resource.geosciml.org/ontology/timescale/thors#";

                /// <summary>
                /// http://resource.geosciml.org/ontology/timescale/thors
                /// </summary>
                public static readonly string DEREFERENCE_URI = "http://resource.geosciml.org/ontology/timescale/thors#";

                /// <summary>
                /// thors:Era
                /// </summary>
                public static readonly RDFResource ERA = new RDFResource($"{BASE_URI}Era");

                /// <summary>
                /// thors:EraBoundary
                /// </summary>
                public static readonly RDFResource ERA_BOUNDARY = new RDFResource($"{BASE_URI}EraBoundary");

                /// <summary>
                /// thors:ReferenceSystem
                /// </summary>
                public static readonly RDFResource REFERENCE_SYSTEM = new RDFResource($"{BASE_URI}ReferenceSystem");

                /// <summary>
                /// thors:begin
                /// </summary>
                public static readonly RDFResource BEGIN = new RDFResource($"{BASE_URI}begin");

                /// <summary>
                /// thors:component
                /// </summary>
                public static readonly RDFResource COMPONENT = new RDFResource($"{BASE_URI}component");

                /// <summary>
                /// thors:end
                /// </summary>
                public static readonly RDFResource END = new RDFResource($"{BASE_URI}end");

                /// <summary>
                /// thors:member
                /// </summary>
                public static readonly RDFResource MEMBER = new RDFResource($"{BASE_URI}member");

                /// <summary>
                /// thors:nextEra
                /// </summary>
                public static readonly RDFResource NEXT_ERA = new RDFResource($"{BASE_URI}nextEra");

                /// <summary>
                /// thors:previousEra
                /// </summary>
                public static readonly RDFResource PREVIOUS_ERA = new RDFResource($"{BASE_URI}previousEra");

                /// <summary>
                /// thors:referencePoint
                /// </summary>
                public static readonly RDFResource REFERENCE_POINT = new RDFResource($"{BASE_URI}referencePoint");

                /// <summary>
                /// thors:system
                /// </summary>
                public static readonly RDFResource SYSTEM = new RDFResource($"{BASE_URI}system");

                /// <summary>
                /// thors:positionalUncertainty
                /// </summary>
                public static readonly RDFResource POSITIONAL_UNCERTAINTY = new RDFResource($"{BASE_URI}positionalUncertainty");
            }
            #endregion
        }
        #endregion

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
            public static readonly RDFResource LANG = new RDFResource($"{BASE_URI}#lang");

            /// <summary>
            /// xml:base
            /// </summary>
            public static readonly RDFResource BASE = new RDFResource($"{BASE_URI}#base");
            #endregion
        }
        #endregion

        #region XSD
        /// <summary>
        /// XSD represents the W3C XML Schema vocabulary.
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
            public static readonly RDFResource STRING = new RDFResource($"{BASE_URI}string");

            /// <summary>
            /// xsd:boolean
            /// </summary>
            public static readonly RDFResource BOOLEAN = new RDFResource($"{BASE_URI}boolean");

            /// <summary>
            /// xsd:decimal
            /// </summary>
            public static readonly RDFResource DECIMAL = new RDFResource($"{BASE_URI}decimal");

            /// <summary>
            /// xsd:float
            /// </summary>
            public static readonly RDFResource FLOAT = new RDFResource($"{BASE_URI}float");

            /// <summary>
            /// xsd:double
            /// </summary>
            public static readonly RDFResource DOUBLE = new RDFResource($"{BASE_URI}double");

            /// <summary>
            /// xsd:positiveInteger
            /// </summary>
            public static readonly RDFResource POSITIVE_INTEGER = new RDFResource($"{BASE_URI}positiveInteger");

            /// <summary>
            /// xsd:negativeInteger
            /// </summary>
            public static readonly RDFResource NEGATIVE_INTEGER = new RDFResource($"{BASE_URI}negativeInteger");

            /// <summary>
            /// xsd:nonPositiveInteger
            /// </summary>
            public static readonly RDFResource NON_POSITIVE_INTEGER = new RDFResource($"{BASE_URI}nonPositiveInteger");

            /// <summary>
            /// xsd:nonNegativeInteger
            /// </summary>
            public static readonly RDFResource NON_NEGATIVE_INTEGER = new RDFResource($"{BASE_URI}nonNegativeInteger");

            /// <summary>
            /// xsd:integer
            /// </summary>
            public static readonly RDFResource INTEGER = new RDFResource($"{BASE_URI}integer");

            /// <summary>
            /// xsd:long
            /// </summary>
            public static readonly RDFResource LONG = new RDFResource($"{BASE_URI}long");

            /// <summary>
            /// xsd:unsignedLong
            /// </summary>
            public static readonly RDFResource UNSIGNED_LONG = new RDFResource($"{BASE_URI}unsignedLong");

            /// <summary>
            /// xsd:int
            /// </summary>
            public static readonly RDFResource INT = new RDFResource($"{BASE_URI}int");

            /// <summary>
            /// xsd:unsignedInt
            /// </summary>
            public static readonly RDFResource UNSIGNED_INT = new RDFResource($"{BASE_URI}unsignedInt");

            /// <summary>
            /// xsd:short
            /// </summary>
            public static readonly RDFResource SHORT = new RDFResource($"{BASE_URI}short");

            /// <summary>
            /// xsd:unsignedShort
            /// </summary>
            public static readonly RDFResource UNSIGNED_SHORT = new RDFResource($"{BASE_URI}unsignedShort");

            /// <summary>
            /// xsd:byte
            /// </summary>
            public static readonly RDFResource BYTE = new RDFResource($"{BASE_URI}byte");

            /// <summary>
            /// xsd:unsignedByte
            /// </summary>
            public static readonly RDFResource UNSIGNED_BYTE = new RDFResource($"{BASE_URI}unsignedByte");

            /// <summary>
            /// xsd:duration
            /// </summary>
            public static readonly RDFResource DURATION = new RDFResource($"{BASE_URI}duration");

            /// <summary>
            /// xsd:dateTime
            /// </summary>
            public static readonly RDFResource DATETIME = new RDFResource($"{BASE_URI}dateTime");

            /// <summary>
            /// xsd:dateTimeStamp
            /// </summary>
            public static readonly RDFResource DATETIMESTAMP = new RDFResource($"{BASE_URI}dateTimeStamp");

            /// <summary>
            /// xsd:time
            /// </summary>
            public static readonly RDFResource TIME = new RDFResource($"{BASE_URI}time");

            /// <summary>
            /// xsd:date
            /// </summary>
            public static readonly RDFResource DATE = new RDFResource($"{BASE_URI}date");

            /// <summary>
            /// xsd:gYearMonth
            /// </summary>
            public static readonly RDFResource G_YEAR_MONTH = new RDFResource($"{BASE_URI}gYearMonth");

            /// <summary>
            /// xsd:gYear
            /// </summary>
            public static readonly RDFResource G_YEAR = new RDFResource($"{BASE_URI}gYear");

            /// <summary>
            /// xsd:gMonth
            /// </summary>
            public static readonly RDFResource G_MONTH = new RDFResource($"{BASE_URI}gMonth");

            /// <summary>
            /// xsd:gMonthDay
            /// </summary>
            public static readonly RDFResource G_MONTH_DAY = new RDFResource($"{BASE_URI}gMonthDay");

            /// <summary>
            /// xsd:gDay
            /// </summary>
            public static readonly RDFResource G_DAY = new RDFResource($"{BASE_URI}gDay");

            /// <summary>
            /// xsd:hexBinary
            /// </summary>
            public static readonly RDFResource HEX_BINARY = new RDFResource($"{BASE_URI}hexBinary");

            /// <summary>
            /// xsd:base64Binary
            /// </summary>
            public static readonly RDFResource BASE64_BINARY = new RDFResource($"{BASE_URI}base64Binary");

            /// <summary>
            /// xsd:anyURI
            /// </summary>
            public static readonly RDFResource ANY_URI = new RDFResource($"{BASE_URI}anyURI");

            /// <summary>
            /// xsd:QName
            /// </summary>
            public static readonly RDFResource QNAME = new RDFResource($"{BASE_URI}QName");

            /// <summary>
            /// xsd:NOTATION
            /// </summary>
            public static readonly RDFResource NOTATION = new RDFResource($"{BASE_URI}NOTATION");

            /// <summary>
            /// xsd:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource($"{BASE_URI}language");

            /// <summary>
            /// xsd:normalizedString
            /// </summary>
            public static readonly RDFResource NORMALIZED_STRING = new RDFResource($"{BASE_URI}normalizedString");

            /// <summary>
            /// xsd:token
            /// </summary>
            public static readonly RDFResource TOKEN = new RDFResource($"{BASE_URI}token");

            /// <summary>
            /// xsd:NMToken
            /// </summary>
            public static readonly RDFResource NMTOKEN = new RDFResource($"{BASE_URI}NMToken");

            /// <summary>
            /// xsd:Name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource($"{BASE_URI}Name");

            /// <summary>
            /// xsd:NCName
            /// </summary>
            public static readonly RDFResource NCNAME = new RDFResource($"{BASE_URI}NCName");

            /// <summary>
            /// xsd:ID
            /// </summary>
            public static readonly RDFResource ID = new RDFResource($"{BASE_URI}ID");

            //FACETS

            /// <summary>
            /// xsd:length
            /// </summary>
            public static readonly RDFResource LENGTH = new RDFResource($"{BASE_URI}length");

            /// <summary>
            /// xsd:minLength
            /// </summary>
            public static readonly RDFResource MIN_LENGTH = new RDFResource($"{BASE_URI}minLength");

            /// <summary>
            /// xsd:maxLength
            /// </summary>
            public static readonly RDFResource MAX_LENGTH = new RDFResource($"{BASE_URI}maxLength");

            /// <summary>
            /// xsd:pattern
            /// </summary>
            public static readonly RDFResource PATTERN = new RDFResource($"{BASE_URI}pattern");

            /// <summary>
            /// xsd:maxInclusive
            /// </summary>
            public static readonly RDFResource MAX_INCLUSIVE = new RDFResource($"{BASE_URI}maxInclusive");

            /// <summary>
            /// xsd:maxExclusive
            /// </summary>
            public static readonly RDFResource MAX_EXCLUSIVE = new RDFResource($"{BASE_URI}maxExclusive");

            /// <summary>
            /// xsd:minExclusive
            /// </summary>
            public static readonly RDFResource MIN_EXCLUSIVE = new RDFResource($"{BASE_URI}minExclusive");

            /// <summary>
            /// xsd:minInclusive
            /// </summary>
            public static readonly RDFResource MIN_INCLUSIVE = new RDFResource($"{BASE_URI}minInclusive");
            #endregion
        }
        #endregion
    }
}