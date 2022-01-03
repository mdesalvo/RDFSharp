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
        #region CRM
        /// <summary>
        /// CRM represents the CIDOC CRM 5.0.4 vocabulary.
        /// </summary>
        public static class CRM
        {

            #region Properties
            /// <summary>
            /// crm
            /// </summary>
            public static readonly string PREFIX = "crm";

            /// <summary>
            /// http://www.cidoc-crm.org/cidoc-crm/
            /// </summary>
            public static readonly string BASE_URI = "http://www.cidoc-crm.org/cidoc-crm/";

            /// <summary>
            /// http://www.cidoc-crm.org/sites/default/files/cidoc_crm_v5.0.4_official_release.rdfs
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.cidoc-crm.org/sites/default/files/cidoc_crm_v5.0.4_official_release.rdfs";

            /// <summary>
            /// crm:E1_CRM_Entity
            /// </summary>
            public static readonly RDFResource E1_CRM_ENTITY = new RDFResource(string.Concat(CRM.BASE_URI, "E1_CRM_Entity"));

            /// <summary>
            /// crm:E2_Temporal_Entity
            /// </summary>
            public static readonly RDFResource E2_TEMPORAL_ENTITY = new RDFResource(string.Concat(CRM.BASE_URI, "E2_Temporal_Entity"));

            /// <summary>
            /// crm:E3_Condition_State
            /// </summary>
            public static readonly RDFResource E3_CONDITION_STATE = new RDFResource(string.Concat(CRM.BASE_URI, "E3_Condition_State"));

            /// <summary>
            /// crm:E4_Period
            /// </summary>
            public static readonly RDFResource E4_PERIOD = new RDFResource(string.Concat(CRM.BASE_URI, "E4_Period"));

            /// <summary>
            /// crm:E5_Event
            /// </summary>
            public static readonly RDFResource E5_EVENT = new RDFResource(string.Concat(CRM.BASE_URI, "E5_Event"));

            /// <summary>
            /// crm:E6_Destruction
            /// </summary>
            public static readonly RDFResource E6_DESTRUCTION = new RDFResource(string.Concat(CRM.BASE_URI, "E6_Destruction"));

            /// <summary>
            /// crm:E7_Activity
            /// </summary>
            public static readonly RDFResource E7_ACTIVITY = new RDFResource(string.Concat(CRM.BASE_URI, "E7_Activity"));

            /// <summary>
            /// crm:E8_Acquisition
            /// </summary>
            public static readonly RDFResource E8_ACQUISITION = new RDFResource(string.Concat(CRM.BASE_URI, "E8_Acquisition"));

            /// <summary>
            /// crm:E9_Move
            /// </summary>
            public static readonly RDFResource E9_MOVE = new RDFResource(string.Concat(CRM.BASE_URI, "E9_Move"));

            /// <summary>
            /// crm:E10_Transfer_of_Custody
            /// </summary>
            public static readonly RDFResource E10_TRANSFER_OF_CUSTODY = new RDFResource(string.Concat(CRM.BASE_URI, "E10_Transfer_of_Custody"));

            /// <summary>
            /// crm:E11_Modification
            /// </summary>
            public static readonly RDFResource E11_MODIFICATION = new RDFResource(string.Concat(CRM.BASE_URI, "E11_Modification"));

            /// <summary>
            /// crm:E12_Production
            /// </summary>
            public static readonly RDFResource E12_PRODUCTION = new RDFResource(string.Concat(CRM.BASE_URI, "E12_Production"));

            /// <summary>
            /// crm:E13_Attribute_Assignment
            /// </summary>
            public static readonly RDFResource E13_ATTRIBUTE_ASSIGNMENT = new RDFResource(string.Concat(CRM.BASE_URI, "E13_Attribute_Assignment"));

            /// <summary>
            /// crm:E14_Condition_Assessment
            /// </summary>
            public static readonly RDFResource E14_CONDITION_ASSESSMENT = new RDFResource(string.Concat(CRM.BASE_URI, "E14_Condition_Assessment"));

            /// <summary>
            /// crm:E15_Identifier_Assignment
            /// </summary>
            public static readonly RDFResource E15_IDENTIFIER_ASSIGNMENT = new RDFResource(string.Concat(CRM.BASE_URI, "E15_Identifier_Assignment"));

            /// <summary>
            /// crm:E16_Measurement
            /// </summary>
            public static readonly RDFResource E16_MEASUREMENT = new RDFResource(string.Concat(CRM.BASE_URI, "E16_Measurement"));

            /// <summary>
            /// crm:E17_Type_Assignment
            /// </summary>
            public static readonly RDFResource E17_TYPE_ASSIGNMENT = new RDFResource(string.Concat(CRM.BASE_URI, "E17_Type_Assignment"));

            /// <summary>
            /// crm:E18_Physical_Thing
            /// </summary>
            public static readonly RDFResource E18_PHYSICAL_THING = new RDFResource(string.Concat(CRM.BASE_URI, "E18_Physical_Thing"));

            /// <summary>
            /// crm:E19_Physical_Object
            /// </summary>
            public static readonly RDFResource E19_PHYSICAL_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E19_Physical_Object"));

            /// <summary>
            /// crm:E20_Biological_Object
            /// </summary>
            public static readonly RDFResource E20_BIOLOGICAL_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E20_Biological_Object"));

            /// <summary>
            /// crm:E21_Person
            /// </summary>
            public static readonly RDFResource E21_PERSON = new RDFResource(string.Concat(CRM.BASE_URI, "E21_Person"));

            /// <summary>
            /// crm:E22_Man-Made_Object
            /// </summary>
            public static readonly RDFResource E22_MAN_MADE_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E22_Man-Made_Object"));

            /// <summary>
            /// crm:E24_Physical_Man-Made_Thing
            /// </summary>
            public static readonly RDFResource E24_PHYSICAL_MAN_MADE_THING = new RDFResource(string.Concat(CRM.BASE_URI, "E24_Physical_Man-Made_Thing"));

            /// <summary>
            /// crm:E25_Man-Made_Feature
            /// </summary>
            public static readonly RDFResource E25_MAN_MADE_FEATURE = new RDFResource(string.Concat(CRM.BASE_URI, "E25_Man-Made_Feature"));

            /// <summary>
            /// crm:E26_Physical_Feature
            /// </summary>
            public static readonly RDFResource E26_PHYSICAL_FEATURE = new RDFResource(string.Concat(CRM.BASE_URI, "E26_Physical_Feature"));

            /// <summary>
            /// crm:E27_Site
            /// </summary>
            public static readonly RDFResource E27_SITE = new RDFResource(string.Concat(CRM.BASE_URI, "E27_Site"));

            /// <summary>
            /// crm:E28_Conceptual_Object
            /// </summary>
            public static readonly RDFResource E28_CONCEPTUAL_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E28_Conceptual_Object"));

            /// <summary>
            /// crm:E29_Design_or_Procedure
            /// </summary>
            public static readonly RDFResource E29_DESIGN_OR_PROCEDURE = new RDFResource(string.Concat(CRM.BASE_URI, "E29_Design_or_Procedure"));

            /// <summary>
            /// crm:E30_Right
            /// </summary>
            public static readonly RDFResource E30_RIGHT = new RDFResource(string.Concat(CRM.BASE_URI, "E30_Right"));

            /// <summary>
            /// crm:E31_Document
            /// </summary>
            public static readonly RDFResource E31_DOCUMENT = new RDFResource(string.Concat(CRM.BASE_URI, "E31_Document"));

            /// <summary>
            /// crm:E32_Authority_Document
            /// </summary>
            public static readonly RDFResource E32_AUTHORITY_DOCUMENT = new RDFResource(string.Concat(CRM.BASE_URI, "E32_Authority_Document"));

            /// <summary>
            /// crm:E33_Linguistic_Object
            /// </summary>
            public static readonly RDFResource E33_LINGUISTIC_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E33_Linguistic_Object"));

            /// <summary>
            /// crm:E34_Inscription
            /// </summary>
            public static readonly RDFResource E34_INSCRIPTION = new RDFResource(string.Concat(CRM.BASE_URI, "E34_Inscription"));

            /// <summary>
            /// crm:E35_Title
            /// </summary>
            public static readonly RDFResource E35_TITLE = new RDFResource(string.Concat(CRM.BASE_URI, "E35_Title"));

            /// <summary>
            /// crm:E36_Visual_Item
            /// </summary>
            public static readonly RDFResource E36_VISUAL_ITEM = new RDFResource(string.Concat(CRM.BASE_URI, "E36_Visual_Item"));

            /// <summary>
            /// crm:E37_Mark
            /// </summary>
            public static readonly RDFResource E37_MARK = new RDFResource(string.Concat(CRM.BASE_URI, "E37_Mark"));

            /// <summary>
            /// crm:E38_Image
            /// </summary>
            public static readonly RDFResource E38_IMAGE = new RDFResource(string.Concat(CRM.BASE_URI, "E38_Image"));

            /// <summary>
            /// crm:E39_Actor
            /// </summary>
            public static readonly RDFResource E39_ACTOR = new RDFResource(string.Concat(CRM.BASE_URI, "E39_Actor"));

            /// <summary>
            /// crm:E40_Legal_Body
            /// </summary>
            public static readonly RDFResource E40_LEGAL_BODY = new RDFResource(string.Concat(CRM.BASE_URI, "E40_Legal_Body"));

            /// <summary>
            /// crm:E41_Appellation
            /// </summary>
            public static readonly RDFResource E41_APPELLATION = new RDFResource(string.Concat(CRM.BASE_URI, "E41_Appellation"));

            /// <summary>
            /// crm:E42_Identifier
            /// </summary>
            public static readonly RDFResource E42_IDENTIFIER = new RDFResource(string.Concat(CRM.BASE_URI, "E42_Identifier"));

            /// <summary>
            /// crm:E44_Place_Appellation
            /// </summary>
            public static readonly RDFResource E44_PLACE_APPELLATION = new RDFResource(string.Concat(CRM.BASE_URI, "E44_Place_Appellation"));

            /// <summary>
            /// crm:E45_Address
            /// </summary>
            public static readonly RDFResource E45_ADDRESS = new RDFResource(string.Concat(CRM.BASE_URI, "E45_Address"));

            /// <summary>
            /// crm:E46_Section_Definition
            /// </summary>
            public static readonly RDFResource E46_SECTION_DEFINITION = new RDFResource(string.Concat(CRM.BASE_URI, "E46_Section_Definition"));

            /// <summary>
            /// crm:E47_Spatial_Coordinates
            /// </summary>
            public static readonly RDFResource E47_SPATIAL_COORDINATES = new RDFResource(string.Concat(CRM.BASE_URI, "E47_Spatial_Coordinates"));

            /// <summary>
            /// crm:E48_Place_Name
            /// </summary>
            public static readonly RDFResource E48_PLACE_NAME = new RDFResource(string.Concat(CRM.BASE_URI, "E48_Place_Name"));

            /// <summary>
            /// crm:E49_Time_Appellation
            /// </summary>
            public static readonly RDFResource E49_TIME_APPELLATION = new RDFResource(string.Concat(CRM.BASE_URI, "E49_Time_Appellation"));

            /// <summary>
            /// crm:E50_Date
            /// </summary>
            public static readonly RDFResource E50_DATE = new RDFResource(string.Concat(CRM.BASE_URI, "E50_Date"));

            /// <summary>
            /// crm:E51_Contact_Point
            /// </summary>
            public static readonly RDFResource E51_CONTACT_POINT = new RDFResource(string.Concat(CRM.BASE_URI, "E51_Contact_Point"));

            /// <summary>
            /// crm:E52_Time-Span
            /// </summary>
            public static readonly RDFResource E52_TIME_SPAN = new RDFResource(string.Concat(CRM.BASE_URI, "E52_Time-Span"));

            /// <summary>
            /// crm:E53_Place
            /// </summary>
            public static readonly RDFResource E53_PLACE = new RDFResource(string.Concat(CRM.BASE_URI, "E53_Place"));

            /// <summary>
            /// crm:E54_Dimension
            /// </summary>
            public static readonly RDFResource E54_DIMENSION = new RDFResource(string.Concat(CRM.BASE_URI, "E54_Dimension"));

            /// <summary>
            /// crm:E55_Type
            /// </summary>
            public static readonly RDFResource E55_TYPE = new RDFResource(string.Concat(CRM.BASE_URI, "E55_Type"));

            /// <summary>
            /// crm:E56_Language
            /// </summary>
            public static readonly RDFResource E56_LANGUAGE = new RDFResource(string.Concat(CRM.BASE_URI, "E56_Language"));

            /// <summary>
            /// crm:E57_Material
            /// </summary>
            public static readonly RDFResource E57_MATERIAL = new RDFResource(string.Concat(CRM.BASE_URI, "E57_Material"));

            /// <summary>
            /// crm:E58_Measurement_Unit
            /// </summary>
            public static readonly RDFResource E58_MEASUREMENT_UNIT = new RDFResource(string.Concat(CRM.BASE_URI, "E58_Measurement_Unit"));

            /// <summary>
            /// crm:E63_Beginning_of_Existence
            /// </summary>
            public static readonly RDFResource E63_BEGINNING_OF_EXISTENCE = new RDFResource(string.Concat(CRM.BASE_URI, "E63_Beginning_of_Existence"));

            /// <summary>
            /// crm:E64_End_of_Existence
            /// </summary>
            public static readonly RDFResource E64_END_OF_EXISTENCE = new RDFResource(string.Concat(CRM.BASE_URI, "E64_End_of_Existence"));

            /// <summary>
            /// crm:E65_Creation
            /// </summary>
            public static readonly RDFResource E65_CREATION = new RDFResource(string.Concat(CRM.BASE_URI, "E65_Creation"));

            /// <summary>
            /// crm:E66_Formation
            /// </summary>
            public static readonly RDFResource E66_FORMATION = new RDFResource(string.Concat(CRM.BASE_URI, "E66_Formation"));

            /// <summary>
            /// crm:E67_Birth
            /// </summary>
            public static readonly RDFResource E67_BIRTH = new RDFResource(string.Concat(CRM.BASE_URI, "E67_Birth"));

            /// <summary>
            /// crm:E68_Dissolution
            /// </summary>
            public static readonly RDFResource E68_DISSOLUTION = new RDFResource(string.Concat(CRM.BASE_URI, "E68_Dissolution"));

            /// <summary>
            /// crm:E69_Death
            /// </summary>
            public static readonly RDFResource E69_DEATH = new RDFResource(string.Concat(CRM.BASE_URI, "E69_Death"));

            /// <summary>
            /// crm:E70_Thing
            /// </summary>
            public static readonly RDFResource E70_THING = new RDFResource(string.Concat(CRM.BASE_URI, "E70_Thing"));

            /// <summary>
            /// crm:E71_Man-Made_Thing
            /// </summary>
            public static readonly RDFResource E71_MAN_MADE_THING = new RDFResource(string.Concat(CRM.BASE_URI, "E71_Man-Made_Thing"));

            /// <summary>
            /// crm:E72_Legal_Object
            /// </summary>
            public static readonly RDFResource E72_LEGAL_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E72_Legal_Object"));

            /// <summary>
            /// crm:E73_Information_Object
            /// </summary>
            public static readonly RDFResource E73_INFORMATION_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E73_Information_Object"));

            /// <summary>
            /// crm:E74_Group
            /// </summary>
            public static readonly RDFResource E74_GROUP = new RDFResource(string.Concat(CRM.BASE_URI, "E74_Group"));

            /// <summary>
            /// crm:E75_Conceptual_Object_Appellation
            /// </summary>
            public static readonly RDFResource E75_CONCEPTUAL_OBJECT_APPELLATION = new RDFResource(string.Concat(CRM.BASE_URI, "E75_Conceptual_Object_Appellation"));

            /// <summary>
            /// crm:E77_Persistent_Item
            /// </summary>
            public static readonly RDFResource E77_PERSISTENT_ITEM = new RDFResource(string.Concat(CRM.BASE_URI, "E77_Persistent_Item"));

            /// <summary>
            /// crm:E78_Collection
            /// </summary>
            public static readonly RDFResource E78_COLLECTION = new RDFResource(string.Concat(CRM.BASE_URI, "E78_Collection"));

            /// <summary>
            /// crm:E79_Part_Addition
            /// </summary>
            public static readonly RDFResource E79_PART_ADDITION = new RDFResource(string.Concat(CRM.BASE_URI, "E79_Part_Addition"));

            /// <summary>
            /// crm:E80_Part_Removal
            /// </summary>
            public static readonly RDFResource E80_PART_REMOVAL = new RDFResource(string.Concat(CRM.BASE_URI, "E80_Part_Removal"));

            /// <summary>
            /// crm:E81_Transformation
            /// </summary>
            public static readonly RDFResource E81_TRANSFORMATION = new RDFResource(string.Concat(CRM.BASE_URI, "E81_Transformation"));

            /// <summary>
            /// crm:E82_Actor_Appellation
            /// </summary>
            public static readonly RDFResource E82_ACTOR_APPELLATION = new RDFResource(string.Concat(CRM.BASE_URI, "E82_Actor_Appellation"));

            /// <summary>
            /// crm:E83_Type_Creation
            /// </summary>
            public static readonly RDFResource E83_TYPE_CREATION = new RDFResource(string.Concat(CRM.BASE_URI, "E83_Type_Creation"));

            /// <summary>
            /// crm:E84_Information_Carrier
            /// </summary>
            public static readonly RDFResource E84_INFORMATION_CARRIER = new RDFResource(string.Concat(CRM.BASE_URI, "E84_Information_Carrier"));

            /// <summary>
            /// crm:E85_Joining
            /// </summary>
            public static readonly RDFResource E85_JOINING = new RDFResource(string.Concat(CRM.BASE_URI, "E85_Joining"));

            /// <summary>
            /// crm:E86_Leaving
            /// </summary>
            public static readonly RDFResource E86_LEAVING = new RDFResource(string.Concat(CRM.BASE_URI, "E86_Leaving"));

            /// <summary>
            /// crm:E87_Curation_Activity
            /// </summary>
            public static readonly RDFResource E87_CURATION_ACTIVITY = new RDFResource(string.Concat(CRM.BASE_URI, "E87_Curation_Activity"));

            /// <summary>
            /// crm:E89_Propositional_Object
            /// </summary>
            public static readonly RDFResource E89_PROPOSITIONAL_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E89_Propositional_Object"));

            /// <summary>
            /// crm:E90_Symbolic_Object
            /// </summary>
            public static readonly RDFResource E90_SYMBOLIC_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "E90_Symbolic_Object"));

            /// <summary>
            /// crm:P1_is_identified_by
            /// </summary>
            public static readonly RDFResource P1_IS_IDENTIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P1_is_identified_by"));

            /// <summary>
            /// crm:P1i_identifies
            /// </summary>
            public static readonly RDFResource P1I_IDENTIFIES = new RDFResource(string.Concat(CRM.BASE_URI, "P1i_identifies"));

            /// <summary>
            /// crm:P2_has_type
            /// </summary>
            public static readonly RDFResource P2_HAS_TYPE = new RDFResource(string.Concat(CRM.BASE_URI, "P2_has_type"));

            /// <summary>
            /// crm:P2i_is_type_of
            /// </summary>
            public static readonly RDFResource P2I_IS_TYPE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P2i_is_type_of"));

            /// <summary>
            /// crm:P3_has_note
            /// </summary>
            public static readonly RDFResource P3_HAS_NOTE = new RDFResource(string.Concat(CRM.BASE_URI, "P3_has_note"));

            /// <summary>
            /// crm:P4_has_time-span
            /// </summary>
            public static readonly RDFResource P4_HAS_TIME_SPAN = new RDFResource(string.Concat(CRM.BASE_URI, "P4_has_time-span"));

            /// <summary>
            /// crm:P4i_is_time-span_of
            /// </summary>
            public static readonly RDFResource P4I_IS_TIME_SPAN_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P4i_is_time-span_of"));

            /// <summary>
            /// crm:P5_consists_of
            /// </summary>
            public static readonly RDFResource P5_CONSISTS_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P5_consists_of"));

            /// <summary>
            /// crm:P5i_forms_part_of
            /// </summary>
            public static readonly RDFResource P5I_FORMS_PART_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P5i_forms_part_of"));

            /// <summary>
            /// crm:P7_took_place_at
            /// </summary>
            public static readonly RDFResource P7_TOOK_PLACE_AT = new RDFResource(string.Concat(CRM.BASE_URI, "P7_took_place_at"));

            /// <summary>
            /// crm:P7i_witnessed
            /// </summary>
            public static readonly RDFResource P7I_WITNESSED = new RDFResource(string.Concat(CRM.BASE_URI, "P7i_witnessed"));

            /// <summary>
            /// crm:P8_took_place_on_or_within
            /// </summary>
            public static readonly RDFResource P8_TOOK_PLACE_ON_OR_WITHIN = new RDFResource(string.Concat(CRM.BASE_URI, "P8_took_place_on_or_within"));

            /// <summary>
            /// crm:P8i_witnessed
            /// </summary>
            public static readonly RDFResource P8I_WITNESSED = new RDFResource(string.Concat(CRM.BASE_URI, "P8i_witnessed"));

            /// <summary>
            /// crm:P9_consists_of
            /// </summary>
            public static readonly RDFResource P9_CONSISTS_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P9_consists_of"));

            /// <summary>
            /// crm:P9i_forms_part_of
            /// </summary>
            public static readonly RDFResource P9I_FORMS_PART_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P9i_forms_part_of"));

            /// <summary>
            /// crm:P10_falls_within
            /// </summary>
            public static readonly RDFResource P10_FALLS_WITHIN = new RDFResource(string.Concat(CRM.BASE_URI, "P10_falls_within"));

            /// <summary>
            /// crm:P10i_contains
            /// </summary>
            public static readonly RDFResource P10I_CONTAINS = new RDFResource(string.Concat(CRM.BASE_URI, "P10i_contains"));

            /// <summary>
            /// crm:P11_had_participant
            /// </summary>
            public static readonly RDFResource P11_HAD_PARTICIPANT = new RDFResource(string.Concat(CRM.BASE_URI, "P11_had_participant"));

            /// <summary>
            /// crm:P11i_participated_in
            /// </summary>
            public static readonly RDFResource P11I_PARTICIPATED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P11i_participated_in"));

            /// <summary>
            /// crm:P12_occurred_in_the_presence_of
            /// </summary>
            public static readonly RDFResource P12_OCCURRED_IN_THE_PRESENCE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P12_occurred_in_the_presence_of"));

            /// <summary>
            /// crm:P12i_was_present_at
            /// </summary>
            public static readonly RDFResource P12I_WAS_PRESENT_AT = new RDFResource(string.Concat(CRM.BASE_URI, "P12i_was_present_at"));

            /// <summary>
            /// crm:P13_destroyed
            /// </summary>
            public static readonly RDFResource P13_DESTROYED = new RDFResource(string.Concat(CRM.BASE_URI, "P13_destroyed"));

            /// <summary>
            /// crm:P13i_was_destroyed_by
            /// </summary>
            public static readonly RDFResource P13I_WAS_DESTROYED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P13i_was_destroyed_by"));

            /// <summary>
            /// crm:P14_carried_out_by
            /// </summary>
            public static readonly RDFResource P14_CARRIED_OUT_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P14_carried_out_by"));

            /// <summary>
            /// crm:P14i_performed
            /// </summary>
            public static readonly RDFResource P14I_PERFORMED = new RDFResource(string.Concat(CRM.BASE_URI, "P14i_performed"));

            /// <summary>
            /// crm:P15_was_influenced_by
            /// </summary>
            public static readonly RDFResource P15_WAS_INFLUENCED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P15_was_influenced_by"));

            /// <summary>
            /// crm:P15i_influenced
            /// </summary>
            public static readonly RDFResource P15I_INFLUENCED = new RDFResource(string.Concat(CRM.BASE_URI, "P15i_influenced"));

            /// <summary>
            /// crm:P16_used_specific_object
            /// </summary>
            public static readonly RDFResource P16_USED_SPECIFIC_OBJECT = new RDFResource(string.Concat(CRM.BASE_URI, "P16_used_specific_object"));

            /// <summary>
            /// crm:P16i_was_used_for
            /// </summary>
            public static readonly RDFResource P16I_WAS_USED_FOR = new RDFResource(string.Concat(CRM.BASE_URI, "P16i_was_used_for"));

            /// <summary>
            /// crm:P17_was_motivated_by
            /// </summary>
            public static readonly RDFResource P17_WAS_MOTIVATED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P17_was_motivated_by"));

            /// <summary>
            /// crm:P17i_motivated
            /// </summary>
            public static readonly RDFResource P17I_MOTIVATED = new RDFResource(string.Concat(CRM.BASE_URI, "P17i_motivated"));

            /// <summary>
            /// crm:P19_was_intended_use_of
            /// </summary>
            public static readonly RDFResource P19_WAS_INTENDED_USE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P19_was_intended_use_of"));

            /// <summary>
            /// crm:P19i_was_made_for
            /// </summary>
            public static readonly RDFResource P19I_WAS_MADE_FOR = new RDFResource(string.Concat(CRM.BASE_URI, "P19i_was_made_for"));

            /// <summary>
            /// crm:P20_had_specific_purpose
            /// </summary>
            public static readonly RDFResource P20_HAD_SPECIFIC_PURPOSE = new RDFResource(string.Concat(CRM.BASE_URI, "P20_had_specific_purpose"));

            /// <summary>
            /// crm:P20i_was_purpose_of
            /// </summary>
            public static readonly RDFResource P20I_WAS_PURPOSE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P20i_was_purpose_of"));

            /// <summary>
            /// crm:P21_had_general_purpose
            /// </summary>
            public static readonly RDFResource P21_HAD_GENERAL_PURPOSE = new RDFResource(string.Concat(CRM.BASE_URI, "P21_had_general_purpose"));

            /// <summary>
            /// crm:P21i_was_purpose_of
            /// </summary>
            public static readonly RDFResource P21I_WAS_PURPOSE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P21i_was_purpose_of"));

            /// <summary>
            /// crm:P22_transferred_title_to
            /// </summary>
            public static readonly RDFResource P22_TRANSFERRED_TITLE_TO = new RDFResource(string.Concat(CRM.BASE_URI, "P22_transferred_title_to"));

            /// <summary>
            /// crm:P22i_acquired_title_through
            /// </summary>
            public static readonly RDFResource P22I_ACQUIRED_TITLE_THROUGH = new RDFResource(string.Concat(CRM.BASE_URI, "P22i_acquired_title_through"));

            /// <summary>
            /// crm:P23_transferred_title_from
            /// </summary>
            public static readonly RDFResource P23_TRANSFERRED_TITLE_FROM = new RDFResource(string.Concat(CRM.BASE_URI, "P23_transferred_title_from"));

            /// <summary>
            /// crm:P23i_surrendered_title_through
            /// </summary>
            public static readonly RDFResource P23I_SURRENDERED_TITLE_THROUGH = new RDFResource(string.Concat(CRM.BASE_URI, "P23i_surrendered_title_through"));

            /// <summary>
            /// crm:P24_transferred_title_of
            /// </summary>
            public static readonly RDFResource P24_TRANSFERRED_TITLE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P24_transferred_title_of"));

            /// <summary>
            /// crm:P24i_changed_ownership_through
            /// </summary>
            public static readonly RDFResource P24I_CHANGED_OWNERSHIP_THROUGH = new RDFResource(string.Concat(CRM.BASE_URI, "P24i_changed_ownership_through"));

            /// <summary>
            /// crm:P25_moved
            /// </summary>
            public static readonly RDFResource P25_MOVED = new RDFResource(string.Concat(CRM.BASE_URI, "P25_moved"));

            /// <summary>
            /// crm:P25i_moved_by
            /// </summary>
            public static readonly RDFResource P25I_MOVED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P25i_moved_by"));

            /// <summary>
            /// crm:P26_moved_to
            /// </summary>
            public static readonly RDFResource P26_MOVED_TO = new RDFResource(string.Concat(CRM.BASE_URI, "P26_moved_to"));

            /// <summary>
            /// crm:P26i_was_destination_of
            /// </summary>
            public static readonly RDFResource P26I_WAS_DESTINATION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P26i_was_destination_of"));

            /// <summary>
            /// crm:P27_moved_from
            /// </summary>
            public static readonly RDFResource P27_MOVED_FROM = new RDFResource(string.Concat(CRM.BASE_URI, "P27_moved_from"));

            /// <summary>
            /// crm:P27i_was_origin_of
            /// </summary>
            public static readonly RDFResource P27I_WAS_ORIGIN_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P27i_was_origin_of"));

            /// <summary>
            /// crm:P28_custody_surrendered_by
            /// </summary>
            public static readonly RDFResource P28_CUSTODY_SURRENDERED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P28_custody_surrendered_by"));

            /// <summary>
            /// crm:P28i_surrendered_custody_through
            /// </summary>
            public static readonly RDFResource P28I_SURRENDERED_CUSTODY_THROUGH = new RDFResource(string.Concat(CRM.BASE_URI, "P28i_surrendered_custody_through"));

            /// <summary>
            /// crm:P29_custody_received_by
            /// </summary>
            public static readonly RDFResource P29_CUSTODY_RECEIVED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P29_custody_received_by"));

            /// <summary>
            /// crm:P29i_received_custody_through
            /// </summary>
            public static readonly RDFResource P29I_RECEIVED_CUSTODY_THROUGH = new RDFResource(string.Concat(CRM.BASE_URI, "P29i_received_custody_through"));

            /// <summary>
            /// crm:P30_transferred_custody_of
            /// </summary>
            public static readonly RDFResource P30_TRANSFERRED_CUSTODY_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P30_transferred_custody_of"));

            /// <summary>
            /// crm:P30i_custody_transferred_through
            /// </summary>
            public static readonly RDFResource P30I_CUSTODY_TRANSFERRED_THROUGH = new RDFResource(string.Concat(CRM.BASE_URI, "P30i_custody_transferred_through"));

            /// <summary>
            /// crm:P31_has_modified
            /// </summary>
            public static readonly RDFResource P31_HAS_MODIFIED = new RDFResource(string.Concat(CRM.BASE_URI, "P31_has_modified"));

            /// <summary>
            /// crm:P31i_was_modified_by
            /// </summary>
            public static readonly RDFResource P31I_WAS_MODIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P31i_was_modified_by"));

            /// <summary>
            /// crm:P32_used_general_technique
            /// </summary>
            public static readonly RDFResource P32_USED_GENERAL_TECHNIQUE = new RDFResource(string.Concat(CRM.BASE_URI, "P32_used_general_technique"));

            /// <summary>
            /// crm:P32i_was_technique_of
            /// </summary>
            public static readonly RDFResource P32I_WAS_TECHNIQUE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P32i_was_technique_of"));

            /// <summary>
            /// crm:P33_used_specific_technique
            /// </summary>
            public static readonly RDFResource P33_USED_SPECIFIC_TECHNIQUE = new RDFResource(string.Concat(CRM.BASE_URI, "P33_used_specific_technique"));

            /// <summary>
            /// crm:P33i_was_used_by
            /// </summary>
            public static readonly RDFResource P33I_WAS_USED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P33i_was_used_by"));

            /// <summary>
            /// crm:P34_concerned
            /// </summary>
            public static readonly RDFResource P34_CONCERNED = new RDFResource(string.Concat(CRM.BASE_URI, "P34_concerned"));

            /// <summary>
            /// crm:P34i_was_assessed_by
            /// </summary>
            public static readonly RDFResource P34I_WAS_ASSESSED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P34i_was_assessed_by"));

            /// <summary>
            /// crm:P35_has_identified
            /// </summary>
            public static readonly RDFResource P35_HAS_IDENTIFIED = new RDFResource(string.Concat(CRM.BASE_URI, "P35_has_identified"));

            /// <summary>
            /// crm:P35i_was_identified_by
            /// </summary>
            public static readonly RDFResource P35I_WAS_IDENTIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P35i_was_identified_by"));

            /// <summary>
            /// crm:P37_assigned
            /// </summary>
            public static readonly RDFResource P37_ASSIGNED = new RDFResource(string.Concat(CRM.BASE_URI, "P37_assigned"));

            /// <summary>
            /// crm:P37i_was_assigned_by
            /// </summary>
            public static readonly RDFResource P37I_WAS_ASSIGNED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P37i_was_assigned_by"));

            /// <summary>
            /// crm:P38_deassigned
            /// </summary>
            public static readonly RDFResource P38_DEASSIGNED = new RDFResource(string.Concat(CRM.BASE_URI, "P38_deassigned"));

            /// <summary>
            /// crm:P38i_was_deassigned_by
            /// </summary>
            public static readonly RDFResource P38I_WAS_DEASSIGNED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P38i_was_deassigned_by"));

            /// <summary>
            /// crm:P39_measured
            /// </summary>
            public static readonly RDFResource P39_MEASURED = new RDFResource(string.Concat(CRM.BASE_URI, "P39_measured"));

            /// <summary>
            /// crm:P39i_was_measured_by
            /// </summary>
            public static readonly RDFResource P39I_WAS_MEASURED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P39i_was_measured_by"));

            /// <summary>
            /// crm:P40_observed_dimension
            /// </summary>
            public static readonly RDFResource P40_OBSERVED_DIMENSION = new RDFResource(string.Concat(CRM.BASE_URI, "P40_observed_dimension"));

            /// <summary>
            /// crm:P40i_was_observed_in
            /// </summary>
            public static readonly RDFResource P40I_WAS_OBSERVED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P40i_was_observed_in"));

            /// <summary>
            /// crm:P41_classified
            /// </summary>
            public static readonly RDFResource P41_CLASSIFIED = new RDFResource(string.Concat(CRM.BASE_URI, "P41_classified"));

            /// <summary>
            /// crm:P41i_was_classified_by
            /// </summary>
            public static readonly RDFResource P41I_WAS_CLASSIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P41i_was_classified_by"));

            /// <summary>
            /// crm:P42_assigned
            /// </summary>
            public static readonly RDFResource P42_ASSIGNED = new RDFResource(string.Concat(CRM.BASE_URI, "P42_assigned"));

            /// <summary>
            /// crm:P42i_was_assigned_by
            /// </summary>
            public static readonly RDFResource P42I_WAS_ASSIGNED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P42i_was_assigned_by"));

            /// <summary>
            /// crm:P43_has_dimension
            /// </summary>
            public static readonly RDFResource P43_HAS_DIMENSION = new RDFResource(string.Concat(CRM.BASE_URI, "P43_has_dimension"));

            /// <summary>
            /// crm:P43i_is_dimension_of
            /// </summary>
            public static readonly RDFResource P43I_IS_DIMENSION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P43i_is_dimension_of"));

            /// <summary>
            /// crm:P44_has_condition
            /// </summary>
            public static readonly RDFResource P44_HAS_CONDITION = new RDFResource(string.Concat(CRM.BASE_URI, "P44_has_condition"));

            /// <summary>
            /// crm:P44i_is_condition_of
            /// </summary>
            public static readonly RDFResource P44I_IS_CONDITION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P44i_is_condition_of"));

            /// <summary>
            /// crm:P45_consists_of
            /// </summary>
            public static readonly RDFResource P45_CONSISTS_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P45_consists_of"));

            /// <summary>
            /// crm:P45i_is_incorporated_in
            /// </summary>
            public static readonly RDFResource P45I_IS_INCORPORATED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P45i_is_incorporated_in"));

            /// <summary>
            /// crm:P46_is_composed_of
            /// </summary>
            public static readonly RDFResource P46_IS_COMPOSED_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P46_is_composed_of"));

            /// <summary>
            /// crm:P46i_forms_part_of
            /// </summary>
            public static readonly RDFResource P46I_FORMS_PART_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P46i_forms_part_of"));

            /// <summary>
            /// crm:P48_has_preferred_identifier
            /// </summary>
            public static readonly RDFResource P48_HAS_PREFERRED_IDENTIFIER = new RDFResource(string.Concat(CRM.BASE_URI, "P48_has_preferred_identifier"));

            /// <summary>
            /// crm:P48i_is_preferred_identifier_of
            /// </summary>
            public static readonly RDFResource P48I_IS_PREFERRED_IDENTIFIER_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P48i_is_preferred_identifier_of"));

            /// <summary>
            /// crm:P49_has_former_or_current_keeper
            /// </summary>
            public static readonly RDFResource P49_HAS_FORMER_OR_CURRENT_KEEPER = new RDFResource(string.Concat(CRM.BASE_URI, "P49_has_former_or_current_keeper"));

            /// <summary>
            /// crm:P49i_is_former_or_current_keeper_of
            /// </summary>
            public static readonly RDFResource P49I_IS_FORMER_OR_CURRENT_KEEPER_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P49i_is_former_or_current_keeper_of"));

            /// <summary>
            /// crm:P50_has_current_keeper
            /// </summary>
            public static readonly RDFResource P50_HAS_CURRENT_KEEPER = new RDFResource(string.Concat(CRM.BASE_URI, "P50_has_current_keeper"));

            /// <summary>
            /// crm:P50i_is_current_keeper_of
            /// </summary>
            public static readonly RDFResource P50I_IS_CURRENT_KEEPER_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P50i_is_current_keeper_of"));

            /// <summary>
            /// crm:P51_has_former_or_current_owner
            /// </summary>
            public static readonly RDFResource P51_HAS_FORMER_OR_CURRENT_OWNER = new RDFResource(string.Concat(CRM.BASE_URI, "P51_has_former_or_current_owner"));

            /// <summary>
            /// crm:P51i_is_former_or_current_owner_of
            /// </summary>
            public static readonly RDFResource P51I_IS_FORMER_OR_CURRENT_OWNER_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P51i_is_former_or_current_owner_of"));

            /// <summary>
            /// crm:P52_has_current_owner
            /// </summary>
            public static readonly RDFResource P52_HAS_CURRENT_OWNER = new RDFResource(string.Concat(CRM.BASE_URI, "P52_has_current_owner"));

            /// <summary>
            /// crm:P52i_is_current_owner_of
            /// </summary>
            public static readonly RDFResource P52I_IS_CURRENT_OWNER_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P52i_is_current_owner_of"));

            /// <summary>
            /// crm:P53_has_former_or_current_location
            /// </summary>
            public static readonly RDFResource P53_HAS_FORMER_OR_CURRENT_LOCATION = new RDFResource(string.Concat(CRM.BASE_URI, "P53_has_former_or_current_location"));

            /// <summary>
            /// crm:P53i_is_former_or_current_location_of
            /// </summary>
            public static readonly RDFResource P53I_IS_FORMER_OR_CURRENT_LOCATION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P53i_is_former_or_current_location_of"));

            /// <summary>
            /// crm:P54_has_current_permanent_location
            /// </summary>
            public static readonly RDFResource P54_HAS_CURRENT_PERMANENT_LOCATION = new RDFResource(string.Concat(CRM.BASE_URI, "P54_has_current_permanent_location"));

            /// <summary>
            /// crm:P54i_is_current_permanent_location_of
            /// </summary>
            public static readonly RDFResource P54I_IS_CURRENT_PERMANENT_LOCATION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P54i_is_current_permanent_location_of"));

            /// <summary>
            /// crm:P55_has_current_location
            /// </summary>
            public static readonly RDFResource P55_HAS_CURRENT_LOCATION = new RDFResource(string.Concat(CRM.BASE_URI, "P55_has_current_location"));

            /// <summary>
            /// crm:P55i_currently_holds
            /// </summary>
            public static readonly RDFResource P55I_CURRENTLY_HOLDS = new RDFResource(string.Concat(CRM.BASE_URI, "P55i_currently_holds"));

            /// <summary>
            /// crm:P56_bears_feature
            /// </summary>
            public static readonly RDFResource P56_BEARS_FEATURE = new RDFResource(string.Concat(CRM.BASE_URI, "P56_bears_feature"));

            /// <summary>
            /// crm:P56i_is_found_on
            /// </summary>
            public static readonly RDFResource P56I_IS_FOUND_ON = new RDFResource(string.Concat(CRM.BASE_URI, "P56i_is_found_on"));

            /// <summary>
            /// crm:P57_has_number_of_parts
            /// </summary>
            public static readonly RDFResource P57_HAS_NUMBER_OF_PARTS = new RDFResource(string.Concat(CRM.BASE_URI, "P57_has_number_of_parts"));

            /// <summary>
            /// crm:P58_has_section_definition
            /// </summary>
            public static readonly RDFResource P58_HAS_SECTION_DEFINITION = new RDFResource(string.Concat(CRM.BASE_URI, "P58_has_section_definition"));

            /// <summary>
            /// crm:P58i_defines_section
            /// </summary>
            public static readonly RDFResource P58I_DEFINES_SECTION = new RDFResource(string.Concat(CRM.BASE_URI, "P58i_defines_section"));

            /// <summary>
            /// crm:P59_has_section
            /// </summary>
            public static readonly RDFResource P59_HAS_SECTION = new RDFResource(string.Concat(CRM.BASE_URI, "P59_has_section"));

            /// <summary>
            /// crm:P59i_is_located_on_or_within
            /// </summary>
            public static readonly RDFResource P59I_IS_LOCATED_ON_OR_WITHIN = new RDFResource(string.Concat(CRM.BASE_URI, "P59i_is_located_on_or_within"));

            /// <summary>
            /// crm:P62_depicts
            /// </summary>
            public static readonly RDFResource P62_DEPICTS = new RDFResource(string.Concat(CRM.BASE_URI, "P62_depicts"));

            /// <summary>
            /// crm:P62i_is_depicted_by
            /// </summary>
            public static readonly RDFResource P62I_IS_DEPICTED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P62i_is_depicted_by"));

            /// <summary>
            /// crm:P65_shows_visual_item
            /// </summary>
            public static readonly RDFResource P65_SHOWS_VISUAL_ITEM = new RDFResource(string.Concat(CRM.BASE_URI, "P65_shows_visual_item"));

            /// <summary>
            /// crm:P65i_is_shown_by
            /// </summary>
            public static readonly RDFResource P65I_IS_SHOWN_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P65i_is_shown_by"));

            /// <summary>
            /// crm:P67_refers_to
            /// </summary>
            public static readonly RDFResource P67_REFERS_TO = new RDFResource(string.Concat(CRM.BASE_URI, "P67_refers_to"));

            /// <summary>
            /// crm:P67i_is_referred_to_by
            /// </summary>
            public static readonly RDFResource P67I_IS_REFERRED_TO_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P67i_is_referred_to_by"));

            /// <summary>
            /// crm:P68_foresees_use_of
            /// </summary>
            public static readonly RDFResource P68_FORESEES_USE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P68_foresees_use_of"));

            /// <summary>
            /// crm:P68i_use_foreseen_by
            /// </summary>
            public static readonly RDFResource P68I_USE_FORESEEN_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P68i_use_foreseen_by"));

            /// <summary>
            /// crm:P69_is_associated_with
            /// </summary>
            public static readonly RDFResource P69_IS_ASSOCIATED_WITH = new RDFResource(string.Concat(CRM.BASE_URI, "P69_is_associated_with"));

            /// <summary>
            /// crm:P70_documents
            /// </summary>
            public static readonly RDFResource P70_DOCUMENTS = new RDFResource(string.Concat(CRM.BASE_URI, "P70_documents"));

            /// <summary>
            /// crm:P70i_is_documented_in
            /// </summary>
            public static readonly RDFResource P70I_IS_DOCUMENTED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P70i_is_documented_in"));

            /// <summary>
            /// crm:P71_lists
            /// </summary>
            public static readonly RDFResource P71_LISTS = new RDFResource(string.Concat(CRM.BASE_URI, "P71_lists"));

            /// <summary>
            /// crm:P71i_is_listed_in
            /// </summary>
            public static readonly RDFResource P71I_IS_LISTED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P71i_is_listed_in"));

            /// <summary>
            /// crm:P72_has_language
            /// </summary>
            public static readonly RDFResource P72_HAS_LANGUAGE = new RDFResource(string.Concat(CRM.BASE_URI, "P72_has_language"));

            /// <summary>
            /// crm:P72i_is_language_of
            /// </summary>
            public static readonly RDFResource P72I_IS_LANGUAGE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P72i_is_language_of"));

            /// <summary>
            /// crm:P73_has_translation
            /// </summary>
            public static readonly RDFResource P73_HAS_TRANSLATION = new RDFResource(string.Concat(CRM.BASE_URI, "P73_has_translation"));

            /// <summary>
            /// crm:P73i_is_translation_of
            /// </summary>
            public static readonly RDFResource P73I_IS_TRANSLATION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P73i_is_translation_of"));

            /// <summary>
            /// crm:P74_has_current_or_former_residence
            /// </summary>
            public static readonly RDFResource P74_HAS_CURRENT_OR_FORMER_RESIDENCE = new RDFResource(string.Concat(CRM.BASE_URI, "P74_has_current_or_former_residence"));

            /// <summary>
            /// crm:P74i_is_current_or_former_residence_of
            /// </summary>
            public static readonly RDFResource P74I_IS_CURRENT_OR_FORMER_RESIDENCE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P74i_is_current_or_former_residence_of"));

            /// <summary>
            /// crm:P75_possesses
            /// </summary>
            public static readonly RDFResource P75_POSSESSES = new RDFResource(string.Concat(CRM.BASE_URI, "P75_possesses"));

            /// <summary>
            /// crm:P75i_is_possessed_by
            /// </summary>
            public static readonly RDFResource P75I_IS_POSSESSED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P75i_is_possessed_by"));

            /// <summary>
            /// crm:P76_has_contact_point
            /// </summary>
            public static readonly RDFResource P76_HAS_CONTACT_POINT = new RDFResource(string.Concat(CRM.BASE_URI, "P76_has_contact_point"));

            /// <summary>
            /// crm:P76i_provides_access_to
            /// </summary>
            public static readonly RDFResource P76I_PROVIDES_ACCESS_TO = new RDFResource(string.Concat(CRM.BASE_URI, "P76i_provides_access_to"));

            /// <summary>
            /// crm:P78_is_identified_by
            /// </summary>
            public static readonly RDFResource P78_IS_IDENTIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P78_is_identified_by"));

            /// <summary>
            /// crm:P78i_identifies
            /// </summary>
            public static readonly RDFResource P78I_IDENTIFIES = new RDFResource(string.Concat(CRM.BASE_URI, "P78i_identifies"));

            /// <summary>
            /// crm:P79_beginning_is_qualified_by
            /// </summary>
            public static readonly RDFResource P79_BEGINNING_IS_QUALIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P79_beginning_is_qualified_by"));

            /// <summary>
            /// crm:P80_end_is_qualified_by
            /// </summary>
            public static readonly RDFResource P80_END_IS_QUALIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P80_end_is_qualified_by"));

            /// <summary>
            /// crm:P81_ongoing_throughout
            /// </summary>
            public static readonly RDFResource P81_ONGOING_THROUGHOUT = new RDFResource(string.Concat(CRM.BASE_URI, "P81_ongoing_throughout"));

            /// <summary>
            /// crm:P82_at_some_time_within
            /// </summary>
            public static readonly RDFResource P82_AT_SOME_TIME_WITHIN = new RDFResource(string.Concat(CRM.BASE_URI, "P82_at_some_time_within"));

            /// <summary>
            /// crm:P83_had_at_least_duration
            /// </summary>
            public static readonly RDFResource P83_HAD_AT_LEAST_DURATION = new RDFResource(string.Concat(CRM.BASE_URI, "P83_had_at_least_duration"));

            /// <summary>
            /// crm:P83i_was_minimum_duration_of
            /// </summary>
            public static readonly RDFResource P83I_WAS_MINIMUM_DURATION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P83i_was_minimum_duration_of"));

            /// <summary>
            /// crm:P84_had_at_most_duration
            /// </summary>
            public static readonly RDFResource P84_HAD_AT_MOST_DURATION = new RDFResource(string.Concat(CRM.BASE_URI, "P84_had_at_most_duration"));

            /// <summary>
            /// crm:P84i_was_maximum_duration_of
            /// </summary>
            public static readonly RDFResource P84I_WAS_MAXIMUM_DURATION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P84i_was_maximum_duration_of"));

            /// <summary>
            /// crm:P86_falls_within
            /// </summary>
            public static readonly RDFResource P86_FALLS_WITHIN = new RDFResource(string.Concat(CRM.BASE_URI, "P86_falls_within"));

            /// <summary>
            /// crm:P86i_contains
            /// </summary>
            public static readonly RDFResource P86I_CONTAINS = new RDFResource(string.Concat(CRM.BASE_URI, "P86i_contains"));

            /// <summary>
            /// crm:P87_is_identified_by
            /// </summary>
            public static readonly RDFResource P87_IS_IDENTIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P87_is_identified_by"));

            /// <summary>
            /// crm:P87i_identifies
            /// </summary>
            public static readonly RDFResource P87I_IDENTIFIES = new RDFResource(string.Concat(CRM.BASE_URI, "P87i_identifies"));

            /// <summary>
            /// crm:P88_consists_of
            /// </summary>
            public static readonly RDFResource P88_CONSISTS_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P88_consists_of"));

            /// <summary>
            /// crm:P88i_forms_part_of
            /// </summary>
            public static readonly RDFResource P88I_FORMS_PART_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P88i_forms_part_of"));

            /// <summary>
            /// crm:P89_falls_within
            /// </summary>
            public static readonly RDFResource P89_FALLS_WITHIN = new RDFResource(string.Concat(CRM.BASE_URI, "P89_falls_within"));

            /// <summary>
            /// crm:P89i_contains
            /// </summary>
            public static readonly RDFResource P89I_CONTAINS = new RDFResource(string.Concat(CRM.BASE_URI, "P89i_contains"));

            /// <summary>
            /// crm:P90_has_value
            /// </summary>
            public static readonly RDFResource P90_HAS_VALUE = new RDFResource(string.Concat(CRM.BASE_URI, "P90_has_value"));

            /// <summary>
            /// crm:P91_has_unit
            /// </summary>
            public static readonly RDFResource P91_HAS_UNIT = new RDFResource(string.Concat(CRM.BASE_URI, "P91_has_unit"));

            /// <summary>
            /// crm:P91i_is_unit_of
            /// </summary>
            public static readonly RDFResource P91I_IS_UNIT_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P91i_is_unit_of"));

            /// <summary>
            /// crm:P92_brought_into_existence
            /// </summary>
            public static readonly RDFResource P92_BROUGHT_INTO_EXISTENCE = new RDFResource(string.Concat(CRM.BASE_URI, "P92_brought_into_existence"));

            /// <summary>
            /// crm:P92i_was_brought_into_existence_by
            /// </summary>
            public static readonly RDFResource P92I_WAS_BROUGHT_INTO_EXISTENCE_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P92i_was_brought_into_existence_by"));

            /// <summary>
            /// crm:P93_took_out_of_existence
            /// </summary>
            public static readonly RDFResource P93_TOOK_OUT_OF_EXISTENCE = new RDFResource(string.Concat(CRM.BASE_URI, "P93_took_out_of_existence"));

            /// <summary>
            /// crm:P93i_was_taken_out_of_existence_by
            /// </summary>
            public static readonly RDFResource P93I_WAS_TAKEN_OUT_OF_EXISTENCE_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P93i_was_taken_out_of_existence_by"));

            /// <summary>
            /// crm:P94_has_created
            /// </summary>
            public static readonly RDFResource P94_HAS_CREATED = new RDFResource(string.Concat(CRM.BASE_URI, "P94_has_created"));

            /// <summary>
            /// crm:P94i_was_created_by
            /// </summary>
            public static readonly RDFResource P94I_WAS_CREATED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P94i_was_created_by"));

            /// <summary>
            /// crm:P95_has_formed
            /// </summary>
            public static readonly RDFResource P95_HAS_FORMED = new RDFResource(string.Concat(CRM.BASE_URI, "P95_has_formed"));

            /// <summary>
            /// crm:P95i_was_formed_by
            /// </summary>
            public static readonly RDFResource P95I_WAS_FORMED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P95i_was_formed_by"));

            /// <summary>
            /// crm:P96_by_mother
            /// </summary>
            public static readonly RDFResource P96_BY_MOTHER = new RDFResource(string.Concat(CRM.BASE_URI, "P96_by_mother"));

            /// <summary>
            /// crm:P96i_gave_birth
            /// </summary>
            public static readonly RDFResource P96I_GAVE_BIRTH = new RDFResource(string.Concat(CRM.BASE_URI, "P96i_gave_birth"));

            /// <summary>
            /// crm:P97_from_father
            /// </summary>
            public static readonly RDFResource P97_FROM_FATHER = new RDFResource(string.Concat(CRM.BASE_URI, "P97_from_father"));

            /// <summary>
            /// crm:P97i_was_father_for
            /// </summary>
            public static readonly RDFResource P97I_WAS_FATHER_FOR = new RDFResource(string.Concat(CRM.BASE_URI, "P97i_was_father_for"));

            /// <summary>
            /// crm:P98_brought_into_life
            /// </summary>
            public static readonly RDFResource P98_BROUGHT_INTO_LIFE = new RDFResource(string.Concat(CRM.BASE_URI, "P98_brought_into_life"));

            /// <summary>
            /// crm:P98i_was_born
            /// </summary>
            public static readonly RDFResource P98I_WAS_BORN = new RDFResource(string.Concat(CRM.BASE_URI, "P98i_was_born"));

            /// <summary>
            /// crm:P99_dissolved
            /// </summary>
            public static readonly RDFResource P99_DISSOLVED = new RDFResource(string.Concat(CRM.BASE_URI, "P99_dissolved"));

            /// <summary>
            /// crm:P99i_was_dissolved_by
            /// </summary>
            public static readonly RDFResource P99I_WAS_DISSOLVED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P99i_was_dissolved_by"));

            /// <summary>
            /// crm:P100_was_death_of
            /// </summary>
            public static readonly RDFResource P100_WAS_DEATH_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P100_was_death_of"));

            /// <summary>
            /// crm:P100i_died_in
            /// </summary>
            public static readonly RDFResource P100I_DIED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P100i_died_in"));

            /// <summary>
            /// crm:P101_had_as_general_use
            /// </summary>
            public static readonly RDFResource P101_HAD_AS_GENERAL_USE = new RDFResource(string.Concat(CRM.BASE_URI, "P101_had_as_general_use"));

            /// <summary>
            /// crm:P101i_was_use_of
            /// </summary>
            public static readonly RDFResource P101I_WAS_USE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P101i_was_use_of"));

            /// <summary>
            /// crm:P102_has_title
            /// </summary>
            public static readonly RDFResource P102_HAS_TITLE = new RDFResource(string.Concat(CRM.BASE_URI, "P102_has_title"));

            /// <summary>
            /// crm:P102i_is_title_of
            /// </summary>
            public static readonly RDFResource P102I_IS_TITLE_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P102i_is_title_of"));

            /// <summary>
            /// crm:P103_was_intended_for
            /// </summary>
            public static readonly RDFResource P103_WAS_INTENDED_FOR = new RDFResource(string.Concat(CRM.BASE_URI, "P103_was_intended_for"));

            /// <summary>
            /// crm:P103i_was_intention_of
            /// </summary>
            public static readonly RDFResource P103I_WAS_INTENTION_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P103i_was_intention_of"));

            /// <summary>
            /// crm:P104_is_subject_to
            /// </summary>
            public static readonly RDFResource P104_IS_SUBJECT_TO = new RDFResource(string.Concat(CRM.BASE_URI, "P104_is_subject_to"));

            /// <summary>
            /// crm:P104i_applies_to
            /// </summary>
            public static readonly RDFResource P104I_APPLIES_TO = new RDFResource(string.Concat(CRM.BASE_URI, "P104i_applies_to"));

            /// <summary>
            /// crm:P105_right_held_by
            /// </summary>
            public static readonly RDFResource P105_RIGHT_HELD_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P105_right_held_by"));

            /// <summary>
            /// crm:P105i_has_right_on
            /// </summary>
            public static readonly RDFResource P105I_HAS_RIGHT_ON = new RDFResource(string.Concat(CRM.BASE_URI, "P105i_has_right_on"));

            /// <summary>
            /// crm:P106_is_composed_of
            /// </summary>
            public static readonly RDFResource P106_IS_COMPOSED_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P106_is_composed_of"));

            /// <summary>
            /// crm:P106i_forms_part_of
            /// </summary>
            public static readonly RDFResource P106I_FORMS_PART_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P106i_forms_part_of"));

            /// <summary>
            /// crm:P107_has_current_or_former_member
            /// </summary>
            public static readonly RDFResource P107_HAS_CURRENT_OR_FORMER_MEMBER = new RDFResource(string.Concat(CRM.BASE_URI, "P107_has_current_or_former_member"));

            /// <summary>
            /// crm:P107i_is_current_or_former_member_of
            /// </summary>
            public static readonly RDFResource P107I_IS_CURRENT_OR_FORMER_MEMBER_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P107i_is_current_or_former_member_of"));

            /// <summary>
            /// crm:P108_has_produced
            /// </summary>
            public static readonly RDFResource P108_HAS_PRODUCED = new RDFResource(string.Concat(CRM.BASE_URI, "P108_has_produced"));

            /// <summary>
            /// crm:P108i_was_produced_by
            /// </summary>
            public static readonly RDFResource P108I_WAS_PRODUCED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P108i_was_produced_by"));

            /// <summary>
            /// crm:P109_has_current_or_former_curator
            /// </summary>
            public static readonly RDFResource P109_HAS_CURRENT_OR_FORMER_CURATOR = new RDFResource(string.Concat(CRM.BASE_URI, "P109_has_current_or_former_curator"));

            /// <summary>
            /// crm:P109i_is_current_or_former_curator_of
            /// </summary>
            public static readonly RDFResource P109I_IS_CURRENT_OR_FORMER_CURATOR_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P109i_is_current_or_former_curator_of"));

            /// <summary>
            /// crm:P110_augmented
            /// </summary>
            public static readonly RDFResource P110_AUGMENTED = new RDFResource(string.Concat(CRM.BASE_URI, "P110_augmented"));

            /// <summary>
            /// crm:P110i_was_augmented_by
            /// </summary>
            public static readonly RDFResource P110I_WAS_AUGMENTED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P110i_was_augmented_by"));

            /// <summary>
            /// crm:P111_added
            /// </summary>
            public static readonly RDFResource P111_ADDED = new RDFResource(string.Concat(CRM.BASE_URI, "P111_added"));

            /// <summary>
            /// crm:P111i_was_added_by
            /// </summary>
            public static readonly RDFResource P111I_WAS_ADDED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P111i_was_added_by"));

            /// <summary>
            /// crm:P112_diminished
            /// </summary>
            public static readonly RDFResource P112_DIMINISHED = new RDFResource(string.Concat(CRM.BASE_URI, "P112_diminished"));

            /// <summary>
            /// crm:P112i_was_diminished_by
            /// </summary>
            public static readonly RDFResource P112I_WAS_DIMINISHED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P112i_was_diminished_by"));

            /// <summary>
            /// crm:P113_removed
            /// </summary>
            public static readonly RDFResource P113_REMOVED = new RDFResource(string.Concat(CRM.BASE_URI, "P113_removed"));

            /// <summary>
            /// crm:P113i_was_removed_by
            /// </summary>
            public static readonly RDFResource P113I_WAS_REMOVED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P113i_was_removed_by"));

            /// <summary>
            /// crm:P114_is_equal_in_time_to
            /// </summary>
            public static readonly RDFResource P114_IS_EQUAL_IN_TIME_TO = new RDFResource(string.Concat(CRM.BASE_URI, "P114_is_equal_in_time_to"));

            /// <summary>
            /// crm:P115_finishes
            /// </summary>
            public static readonly RDFResource P115_FINISHES = new RDFResource(string.Concat(CRM.BASE_URI, "P115_finishes"));

            /// <summary>
            /// crm:P115i_is_finished_by
            /// </summary>
            public static readonly RDFResource P115I_IS_FINISHED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P115i_is_finished_by"));

            /// <summary>
            /// crm:P116_starts
            /// </summary>
            public static readonly RDFResource P116_STARTS = new RDFResource(string.Concat(CRM.BASE_URI, "P116_starts"));

            /// <summary>
            /// crm:P116i_is_started_by
            /// </summary>
            public static readonly RDFResource P116I_IS_STARTED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P116i_is_started_by"));

            /// <summary>
            /// crm:P117_occurs_during
            /// </summary>
            public static readonly RDFResource P117_OCCURS_DURING = new RDFResource(string.Concat(CRM.BASE_URI, "P117_occurs_during"));

            /// <summary>
            /// crm:P117i_includes
            /// </summary>
            public static readonly RDFResource P117I_INCLUDES = new RDFResource(string.Concat(CRM.BASE_URI, "P117i_includes"));

            /// <summary>
            /// crm:P118_overlaps_in_time_with
            /// </summary>
            public static readonly RDFResource P118_OVERLAPS_IN_TIME_WITH = new RDFResource(string.Concat(CRM.BASE_URI, "P118_overlaps_in_time_with"));

            /// <summary>
            /// crm:P118i_is_overlapped_in_time_by
            /// </summary>
            public static readonly RDFResource P118I_IS_OVERLAPPED_IN_TIME_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P118i_is_overlapped_in_time_by"));

            /// <summary>
            /// crm:P119_meets_in_time_with
            /// </summary>
            public static readonly RDFResource P119_MEETS_IN_TIME_WITH = new RDFResource(string.Concat(CRM.BASE_URI, "P119_meets_in_time_with"));

            /// <summary>
            /// crm:P119i_is_met_in_time_by
            /// </summary>
            public static readonly RDFResource P119I_IS_MET_IN_TIME_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P119i_is_met_in_time_by"));

            /// <summary>
            /// crm:P120_occurs_before
            /// </summary>
            public static readonly RDFResource P120_OCCURS_BEFORE = new RDFResource(string.Concat(CRM.BASE_URI, "P120_occurs_before"));

            /// <summary>
            /// crm:P120i_occurs_after
            /// </summary>
            public static readonly RDFResource P120I_OCCURS_AFTER = new RDFResource(string.Concat(CRM.BASE_URI, "P120i_occurs_after"));

            /// <summary>
            /// crm:P121_overlaps_with
            /// </summary>
            public static readonly RDFResource P121_OVERLAPS_WITH = new RDFResource(string.Concat(CRM.BASE_URI, "P121_overlaps_with"));

            /// <summary>
            /// crm:P122_borders_with
            /// </summary>
            public static readonly RDFResource P122_BORDERS_WITH = new RDFResource(string.Concat(CRM.BASE_URI, "P122_borders_with"));

            /// <summary>
            /// crm:P123_resulted_in
            /// </summary>
            public static readonly RDFResource P123_RESULTED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P123_resulted_in"));

            /// <summary>
            /// crm:P123i_resulted_from
            /// </summary>
            public static readonly RDFResource P123I_RESULTED_FROM = new RDFResource(string.Concat(CRM.BASE_URI, "P123i_resulted_from"));

            /// <summary>
            /// crm:P124_transformed
            /// </summary>
            public static readonly RDFResource P124_TRANSFORMED = new RDFResource(string.Concat(CRM.BASE_URI, "P124_transformed"));

            /// <summary>
            /// crm:P124i_was_transformed_by
            /// </summary>
            public static readonly RDFResource P124I_WAS_TRANSFORMED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P124i_was_transformed_by"));

            /// <summary>
            /// crm:P125_used_object_of_type
            /// </summary>
            public static readonly RDFResource P125_USED_OBJECT_OF_TYPE = new RDFResource(string.Concat(CRM.BASE_URI, "P125_used_object_of_type"));

            /// <summary>
            /// crm:P125i_was_type_of_object_used_in
            /// </summary>
            public static readonly RDFResource P125I_WAS_TYPE_OF_OBJECT_USED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P125i_was_type_of_object_used_in"));

            /// <summary>
            /// crm:P126_employed
            /// </summary>
            public static readonly RDFResource P126_EMPLOYED = new RDFResource(string.Concat(CRM.BASE_URI, "P126_employed"));

            /// <summary>
            /// crm:P126i_was_employed_in
            /// </summary>
            public static readonly RDFResource P126I_WAS_EMPLOYED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P126i_was_employed_in"));

            /// <summary>
            /// crm:P127_has_broader_term
            /// </summary>
            public static readonly RDFResource P127_HAS_BROADER_TERM = new RDFResource(string.Concat(CRM.BASE_URI, "P127_has_broader_term"));

            /// <summary>
            /// crm:P127i_has_narrower_term
            /// </summary>
            public static readonly RDFResource P127I_HAS_NARROWER_TERM = new RDFResource(string.Concat(CRM.BASE_URI, "P127i_has_narrower_term"));

            /// <summary>
            /// crm:P128_carries
            /// </summary>
            public static readonly RDFResource P128_CARRIES = new RDFResource(string.Concat(CRM.BASE_URI, "P128_carries"));

            /// <summary>
            /// crm:P128i_is_carried_by
            /// </summary>
            public static readonly RDFResource P128I_IS_CARRIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P128i_is_carried_by"));

            /// <summary>
            /// crm:P129_is_about
            /// </summary>
            public static readonly RDFResource P129_IS_ABOUT = new RDFResource(string.Concat(CRM.BASE_URI, "P129_is_about"));

            /// <summary>
            /// crm:P129i_is_subject_of
            /// </summary>
            public static readonly RDFResource P129I_IS_SUBJECT_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P129i_is_subject_of"));

            /// <summary>
            /// crm:P130_shows_features_of
            /// </summary>
            public static readonly RDFResource P130_SHOWS_FEATURES_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P130_shows_features_of"));

            /// <summary>
            /// crm:P130i_features_are_also_found_on
            /// </summary>
            public static readonly RDFResource P130I_FEATURES_ARE_ALSO_FOUND_ON = new RDFResource(string.Concat(CRM.BASE_URI, "P130i_features_are_also_found_on"));

            /// <summary>
            /// crm:P131_is_identified_by
            /// </summary>
            public static readonly RDFResource P131_IS_IDENTIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P131_is_identified_by"));

            /// <summary>
            /// crm:P131i_identifies
            /// </summary>
            public static readonly RDFResource P131I_IDENTIFIES = new RDFResource(string.Concat(CRM.BASE_URI, "P131i_identifies"));

            /// <summary>
            /// crm:P132_overlaps_with
            /// </summary>
            public static readonly RDFResource P132_OVERLAPS_WITH = new RDFResource(string.Concat(CRM.BASE_URI, "P132_overlaps_with"));

            /// <summary>
            /// crm:P133_is_separated_from
            /// </summary>
            public static readonly RDFResource P133_IS_SEPARATED_FROM = new RDFResource(string.Concat(CRM.BASE_URI, "P133_is_separated_from"));

            /// <summary>
            /// crm:P134_continued
            /// </summary>
            public static readonly RDFResource P134_CONTINUED = new RDFResource(string.Concat(CRM.BASE_URI, "P134_continued"));

            /// <summary>
            /// crm:P134i_was_continued_by
            /// </summary>
            public static readonly RDFResource P134I_WAS_CONTINUED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P134i_was_continued_by"));

            /// <summary>
            /// crm:P135_created_type
            /// </summary>
            public static readonly RDFResource P135_CREATED_TYPE = new RDFResource(string.Concat(CRM.BASE_URI, "P135_created_type"));

            /// <summary>
            /// crm:P135i_was_created_by
            /// </summary>
            public static readonly RDFResource P135I_WAS_CREATED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P135i_was_created_by"));

            /// <summary>
            /// crm:P136_was_based_on
            /// </summary>
            public static readonly RDFResource P136_WAS_BASED_ON = new RDFResource(string.Concat(CRM.BASE_URI, "P136_was_based_on"));

            /// <summary>
            /// crm:P136i_supported_type_creation
            /// </summary>
            public static readonly RDFResource P136I_SUPPORTED_TYPE_CREATION = new RDFResource(string.Concat(CRM.BASE_URI, "P136i_supported_type_creation"));

            /// <summary>
            /// crm:P137_exemplifies
            /// </summary>
            public static readonly RDFResource P137_EXEMPLIFIES = new RDFResource(string.Concat(CRM.BASE_URI, "P137_exemplifies"));

            /// <summary>
            /// crm:P137i_is_exemplified_by
            /// </summary>
            public static readonly RDFResource P137I_IS_EXEMPLIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P137i_is_exemplified_by"));

            /// <summary>
            /// crm:P138_represents
            /// </summary>
            public static readonly RDFResource P138_REPRESENTS = new RDFResource(string.Concat(CRM.BASE_URI, "P138_represents"));

            /// <summary>
            /// crm:P138i_has_representation
            /// </summary>
            public static readonly RDFResource P138I_HAS_REPRESENTATION = new RDFResource(string.Concat(CRM.BASE_URI, "P138i_has_representation"));

            /// <summary>
            /// crm:P139_has_alternative_form
            /// </summary>
            public static readonly RDFResource P139_HAS_ALTERNATIVE_FORM = new RDFResource(string.Concat(CRM.BASE_URI, "P139_has_alternative_form"));

            /// <summary>
            /// crm:P140_assigned_attribute_to
            /// </summary>
            public static readonly RDFResource P140_ASSIGNED_ATTRIBUTE_TO = new RDFResource(string.Concat(CRM.BASE_URI, "P140_assigned_attribute_to"));

            /// <summary>
            /// crm:P140i_was_attributed_by
            /// </summary>
            public static readonly RDFResource P140I_WAS_ATTRIBUTED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P140i_was_attributed_by"));

            /// <summary>
            /// crm:P141_assigned
            /// </summary>
            public static readonly RDFResource P141_ASSIGNED = new RDFResource(string.Concat(CRM.BASE_URI, "P141_assigned"));

            /// <summary>
            /// crm:P141i_was_assigned_by
            /// </summary>
            public static readonly RDFResource P141I_WAS_ASSIGNED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P141i_was_assigned_by"));

            /// <summary>
            /// crm:P142_used_constituent
            /// </summary>
            public static readonly RDFResource P142_USED_CONSTITUENT = new RDFResource(string.Concat(CRM.BASE_URI, "P142_used_constituent"));

            /// <summary>
            /// crm:P142i_was_used_in
            /// </summary>
            public static readonly RDFResource P142I_WAS_USED_IN = new RDFResource(string.Concat(CRM.BASE_URI, "P142i_was_used_in"));

            /// <summary>
            /// crm:P143_joined
            /// </summary>
            public static readonly RDFResource P143_JOINED = new RDFResource(string.Concat(CRM.BASE_URI, "P143_joined"));

            /// <summary>
            /// crm:P143i_was_joined_by
            /// </summary>
            public static readonly RDFResource P143I_WAS_JOINED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P143i_was_joined_by"));

            /// <summary>
            /// crm:P144_joined_with
            /// </summary>
            public static readonly RDFResource P144_JOINED_WITH = new RDFResource(string.Concat(CRM.BASE_URI, "P144_joined_with"));

            /// <summary>
            /// crm:P144i_gained_member_by
            /// </summary>
            public static readonly RDFResource P144I_GAINED_MEMBER_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P144i_gained_member_by"));

            /// <summary>
            /// crm:P145_separated
            /// </summary>
            public static readonly RDFResource P145_SEPARATED = new RDFResource(string.Concat(CRM.BASE_URI, "P145_separated"));

            /// <summary>
            /// crm:P145i_left_by
            /// </summary>
            public static readonly RDFResource P145I_LEFT_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P145i_left_by"));

            /// <summary>
            /// crm:P146_separated_from
            /// </summary>
            public static readonly RDFResource P146_SEPARATED_FROM = new RDFResource(string.Concat(CRM.BASE_URI, "P146_separated_from"));

            /// <summary>
            /// crm:P146i_lost_member_by
            /// </summary>
            public static readonly RDFResource P146I_LOST_MEMBER_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P146i_lost_member_by"));

            /// <summary>
            /// crm:P147_curated
            /// </summary>
            public static readonly RDFResource P147_CURATED = new RDFResource(string.Concat(CRM.BASE_URI, "P147_curated"));

            /// <summary>
            /// crm:P147i_was_curated_by
            /// </summary>
            public static readonly RDFResource P147I_WAS_CURATED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P147i_was_curated_by"));

            /// <summary>
            /// crm:P148_has_component
            /// </summary>
            public static readonly RDFResource P148_HAS_COMPONENT = new RDFResource(string.Concat(CRM.BASE_URI, "P148_has_component"));

            /// <summary>
            /// crm:P148i_is_component_of
            /// </summary>
            public static readonly RDFResource P148I_IS_COMPONENT_OF = new RDFResource(string.Concat(CRM.BASE_URI, "P148i_is_component_of"));

            /// <summary>
            /// crm:P149_is_identified_by
            /// </summary>
            public static readonly RDFResource P149_IS_IDENTIFIED_BY = new RDFResource(string.Concat(CRM.BASE_URI, "P149_is_identified_by"));

            /// <summary>
            /// crm:P149i_identifies
            /// </summary>
            public static readonly RDFResource P149I_IDENTIFIES = new RDFResource(string.Concat(CRM.BASE_URI, "P149i_identifies"));
            #endregion

        }
        #endregion
    }
}