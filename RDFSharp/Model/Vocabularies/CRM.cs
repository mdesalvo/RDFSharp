/*
   Copyright 2012-2020 Marco De Salvo

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
            public static readonly String PREFIX = "crm";

            /// <summary>
            /// http://www.cidoc-crm.org/cidoc-crm/
            /// </summary>
            public static readonly String BASE_URI = "http://www.cidoc-crm.org/cidoc-crm/";

            /// <summary>
            /// crm:E1_CRM_Entity
            /// </summary>
            public static readonly RDFResource E1_CRM_ENTITY = new RDFResource(CRM.BASE_URI + "E1_CRM_Entity");

            /// <summary>
            /// crm:E2_Temporal_Entity
            /// </summary>
            public static readonly RDFResource E2_TEMPORAL_ENTITY = new RDFResource(CRM.BASE_URI + "E2_Temporal_Entity");

            /// <summary>
            /// crm:E3_Condition_State
            /// </summary>
            public static readonly RDFResource E3_CONDITION_STATE = new RDFResource(CRM.BASE_URI + "E3_Condition_State");

            /// <summary>
            /// crm:E4_Period
            /// </summary>
            public static readonly RDFResource E4_PERIOD = new RDFResource(CRM.BASE_URI + "E4_Period");

            /// <summary>
            /// crm:E5_Event
            /// </summary>
            public static readonly RDFResource E5_EVENT = new RDFResource(CRM.BASE_URI + "E5_Event");

            /// <summary>
            /// crm:E6_Destruction
            /// </summary>
            public static readonly RDFResource E6_DESTRUCTION = new RDFResource(CRM.BASE_URI + "E6_Destruction");

            /// <summary>
            /// crm:E7_Activity
            /// </summary>
            public static readonly RDFResource E7_ACTIVITY = new RDFResource(CRM.BASE_URI + "E7_Activity");

            /// <summary>
            /// crm:E8_Acquisition
            /// </summary>
            public static readonly RDFResource E8_ACQUISITION = new RDFResource(CRM.BASE_URI + "E8_Acquisition");

            /// <summary>
            /// crm:E9_Move
            /// </summary>
            public static readonly RDFResource E9_MOVE = new RDFResource(CRM.BASE_URI + "E9_Move");

            /// <summary>
            /// crm:E10_Transfer_of_Custody
            /// </summary>
            public static readonly RDFResource E10_TRANSFER_OF_CUSTODY = new RDFResource(CRM.BASE_URI + "E10_Transfer_of_Custody");

            /// <summary>
            /// crm:E11_Modification
            /// </summary>
            public static readonly RDFResource E11_MODIFICATION = new RDFResource(CRM.BASE_URI + "E11_Modification");

            /// <summary>
            /// crm:E12_Production
            /// </summary>
            public static readonly RDFResource E12_PRODUCTION = new RDFResource(CRM.BASE_URI + "E12_Production");

            /// <summary>
            /// crm:E13_Attribute_Assignment
            /// </summary>
            public static readonly RDFResource E13_ATTRIBUTE_ASSIGNMENT = new RDFResource(CRM.BASE_URI + "E13_Attribute_Assignment");

            /// <summary>
            /// crm:E14_Condition_Assessment
            /// </summary>
            public static readonly RDFResource E14_CONDITION_ASSESSMENT = new RDFResource(CRM.BASE_URI + "E14_Condition_Assessment");

            /// <summary>
            /// crm:E15_Identifier_Assignment
            /// </summary>
            public static readonly RDFResource E15_IDENTIFIER_ASSIGNMENT = new RDFResource(CRM.BASE_URI + "E15_Identifier_Assignment");

            /// <summary>
            /// crm:E16_Measurement
            /// </summary>
            public static readonly RDFResource E16_MEASUREMENT = new RDFResource(CRM.BASE_URI + "E16_Measurement");

            /// <summary>
            /// crm:E17_Type_Assignment
            /// </summary>
            public static readonly RDFResource E17_TYPE_ASSIGNMENT = new RDFResource(CRM.BASE_URI + "E17_Type_Assignment");

            /// <summary>
            /// crm:E18_Physical_Thing
            /// </summary>
            public static readonly RDFResource E18_PHYSICAL_THING = new RDFResource(CRM.BASE_URI + "E18_Physical_Thing");

            /// <summary>
            /// crm:E19_Physical_Object
            /// </summary>
            public static readonly RDFResource E19_PHYSICAL_OBJECT = new RDFResource(CRM.BASE_URI + "E19_Physical_Object");

            /// <summary>
            /// crm:E20_Biological_Object
            /// </summary>
            public static readonly RDFResource E20_BIOLOGICAL_OBJECT = new RDFResource(CRM.BASE_URI + "E20_Biological_Object");

            /// <summary>
            /// crm:E21_Person
            /// </summary>
            public static readonly RDFResource E21_PERSON = new RDFResource(CRM.BASE_URI + "E21_Person");

            /// <summary>
            /// crm:E22_Man-Made_Object
            /// </summary>
            public static readonly RDFResource E22_MAN_MADE_OBJECT = new RDFResource(CRM.BASE_URI + "E22_Man-Made_Object");

            /// <summary>
            /// crm:E24_Physical_Man-Made_Thing
            /// </summary>
            public static readonly RDFResource E24_PHYSICAL_MAN_MADE_THING = new RDFResource(CRM.BASE_URI + "E24_Physical_Man-Made_Thing");

            /// <summary>
            /// crm:E25_Man-Made_Feature
            /// </summary>
            public static readonly RDFResource E25_MAN_MADE_FEATURE = new RDFResource(CRM.BASE_URI + "E25_Man-Made_Feature");

            /// <summary>
            /// crm:E26_Physical_Feature
            /// </summary>
            public static readonly RDFResource E26_PHYSICAL_FEATURE = new RDFResource(CRM.BASE_URI + "E26_Physical_Feature");

            /// <summary>
            /// crm:E27_Site
            /// </summary>
            public static readonly RDFResource E27_SITE = new RDFResource(CRM.BASE_URI + "E27_Site");

            /// <summary>
            /// crm:E28_Conceptual_Object
            /// </summary>
            public static readonly RDFResource E28_CONCEPTUAL_OBJECT = new RDFResource(CRM.BASE_URI + "E28_Conceptual_Object");

            /// <summary>
            /// crm:E29_Design_or_Procedure
            /// </summary>
            public static readonly RDFResource E29_DESIGN_OR_PROCEDURE = new RDFResource(CRM.BASE_URI + "E29_Design_or_Procedure");

            /// <summary>
            /// crm:E30_Right
            /// </summary>
            public static readonly RDFResource E30_RIGHT = new RDFResource(CRM.BASE_URI + "E30_Right");

            /// <summary>
            /// crm:E31_Document
            /// </summary>
            public static readonly RDFResource E31_DOCUMENT = new RDFResource(CRM.BASE_URI + "E31_Document");

            /// <summary>
            /// crm:E32_Authority_Document
            /// </summary>
            public static readonly RDFResource E32_AUTHORITY_DOCUMENT = new RDFResource(CRM.BASE_URI + "E32_Authority_Document");

            /// <summary>
            /// crm:E33_Linguistic_Object
            /// </summary>
            public static readonly RDFResource E33_LINGUISTIC_OBJECT = new RDFResource(CRM.BASE_URI + "E33_Linguistic_Object");

            /// <summary>
            /// crm:E34_Inscription
            /// </summary>
            public static readonly RDFResource E34_INSCRIPTION = new RDFResource(CRM.BASE_URI + "E34_Inscription");

            /// <summary>
            /// crm:E35_Title
            /// </summary>
            public static readonly RDFResource E35_TITLE = new RDFResource(CRM.BASE_URI + "E35_Title");

            /// <summary>
            /// crm:E36_Visual_Item
            /// </summary>
            public static readonly RDFResource E36_VISUAL_ITEM = new RDFResource(CRM.BASE_URI + "E36_Visual_Item");

            /// <summary>
            /// crm:E37_Mark
            /// </summary>
            public static readonly RDFResource E37_MARK = new RDFResource(CRM.BASE_URI + "E37_Mark");

            /// <summary>
            /// crm:E38_Image
            /// </summary>
            public static readonly RDFResource E38_IMAGE = new RDFResource(CRM.BASE_URI + "E38_Image");

            /// <summary>
            /// crm:E39_Actor
            /// </summary>
            public static readonly RDFResource E39_ACTOR = new RDFResource(CRM.BASE_URI + "E39_Actor");

            /// <summary>
            /// crm:E40_Legal_Body
            /// </summary>
            public static readonly RDFResource E40_LEGAL_BODY = new RDFResource(CRM.BASE_URI + "E40_Legal_Body");

            /// <summary>
            /// crm:E41_Appellation
            /// </summary>
            public static readonly RDFResource E41_APPELLATION = new RDFResource(CRM.BASE_URI + "E41_Appellation");

            /// <summary>
            /// crm:E42_Identifier
            /// </summary>
            public static readonly RDFResource E42_IDENTIFIER = new RDFResource(CRM.BASE_URI + "E42_Identifier");

            /// <summary>
            /// crm:E44_Place_Appellation
            /// </summary>
            public static readonly RDFResource E44_PLACE_APPELLATION = new RDFResource(CRM.BASE_URI + "E44_Place_Appellation");

            /// <summary>
            /// crm:E45_Address
            /// </summary>
            public static readonly RDFResource E45_ADDRESS = new RDFResource(CRM.BASE_URI + "E45_Address");

            /// <summary>
            /// crm:E46_Section_Definition
            /// </summary>
            public static readonly RDFResource E46_SECTION_DEFINITION = new RDFResource(CRM.BASE_URI + "E46_Section_Definition");

            /// <summary>
            /// crm:E47_Spatial_Coordinates
            /// </summary>
            public static readonly RDFResource E47_SPATIAL_COORDINATES = new RDFResource(CRM.BASE_URI + "E47_Spatial_Coordinates");

            /// <summary>
            /// crm:E48_Place_Name
            /// </summary>
            public static readonly RDFResource E48_PLACE_NAME = new RDFResource(CRM.BASE_URI + "E48_Place_Name");

            /// <summary>
            /// crm:E49_Time_Appellation
            /// </summary>
            public static readonly RDFResource E49_TIME_APPELLATION = new RDFResource(CRM.BASE_URI + "E49_Time_Appellation");

            /// <summary>
            /// crm:E50_Date
            /// </summary>
            public static readonly RDFResource E50_DATE = new RDFResource(CRM.BASE_URI + "E50_Date");

            /// <summary>
            /// crm:E51_Contact_Point
            /// </summary>
            public static readonly RDFResource E51_CONTACT_POINT = new RDFResource(CRM.BASE_URI + "E51_Contact_Point");

            /// <summary>
            /// crm:E52_Time-Span
            /// </summary>
            public static readonly RDFResource E52_TIME_SPAN = new RDFResource(CRM.BASE_URI + "E52_Time-Span");

            /// <summary>
            /// crm:E53_Place
            /// </summary>
            public static readonly RDFResource E53_PLACE = new RDFResource(CRM.BASE_URI + "E53_Place");

            /// <summary>
            /// crm:E54_Dimension
            /// </summary>
            public static readonly RDFResource E54_DIMENSION = new RDFResource(CRM.BASE_URI + "E54_Dimension");

            /// <summary>
            /// crm:E55_Type
            /// </summary>
            public static readonly RDFResource E55_TYPE = new RDFResource(CRM.BASE_URI + "E55_Type");

            /// <summary>
            /// crm:E56_Language
            /// </summary>
            public static readonly RDFResource E56_LANGUAGE = new RDFResource(CRM.BASE_URI + "E56_Language");

            /// <summary>
            /// crm:E57_Material
            /// </summary>
            public static readonly RDFResource E57_MATERIAL = new RDFResource(CRM.BASE_URI + "E57_Material");

            /// <summary>
            /// crm:E58_Measurement_Unit
            /// </summary>
            public static readonly RDFResource E58_MEASUREMENT_UNIT = new RDFResource(CRM.BASE_URI + "E58_Measurement_Unit");

            /// <summary>
            /// crm:E63_Beginning_of_Existence
            /// </summary>
            public static readonly RDFResource E63_BEGINNING_OF_EXISTENCE = new RDFResource(CRM.BASE_URI + "E63_Beginning_of_Existence");

            /// <summary>
            /// crm:E64_End_of_Existence
            /// </summary>
            public static readonly RDFResource E64_END_OF_EXISTENCE = new RDFResource(CRM.BASE_URI + "E64_End_of_Existence");

            /// <summary>
            /// crm:E65_Creation
            /// </summary>
            public static readonly RDFResource E65_CREATION = new RDFResource(CRM.BASE_URI + "E65_Creation");

            /// <summary>
            /// crm:E66_Formation
            /// </summary>
            public static readonly RDFResource E66_FORMATION = new RDFResource(CRM.BASE_URI + "E66_Formation");

            /// <summary>
            /// crm:E67_Birth
            /// </summary>
            public static readonly RDFResource E67_BIRTH = new RDFResource(CRM.BASE_URI + "E67_Birth");

            /// <summary>
            /// crm:E68_Dissolution
            /// </summary>
            public static readonly RDFResource E68_DISSOLUTION = new RDFResource(CRM.BASE_URI + "E68_Dissolution");

            /// <summary>
            /// crm:E69_Death
            /// </summary>
            public static readonly RDFResource E69_DEATH = new RDFResource(CRM.BASE_URI + "E69_Death");

            /// <summary>
            /// crm:E70_Thing
            /// </summary>
            public static readonly RDFResource E70_THING = new RDFResource(CRM.BASE_URI + "E70_Thing");

            /// <summary>
            /// crm:E71_Man-Made_Thing
            /// </summary>
            public static readonly RDFResource E71_MAN_MADE_THING = new RDFResource(CRM.BASE_URI + "E71_Man-Made_Thing");

            /// <summary>
            /// crm:E72_Legal_Object
            /// </summary>
            public static readonly RDFResource E72_LEGAL_OBJECT = new RDFResource(CRM.BASE_URI + "E72_Legal_Object");

            /// <summary>
            /// crm:E73_Information_Object
            /// </summary>
            public static readonly RDFResource E73_INFORMATION_OBJECT = new RDFResource(CRM.BASE_URI + "E73_Information_Object");

            /// <summary>
            /// crm:E74_Group
            /// </summary>
            public static readonly RDFResource E74_GROUP = new RDFResource(CRM.BASE_URI + "E74_Group");

            /// <summary>
            /// crm:E75_Conceptual_Object_Appellation
            /// </summary>
            public static readonly RDFResource E75_CONCEPTUAL_OBJECT_APPELLATION = new RDFResource(CRM.BASE_URI + "E75_Conceptual_Object_Appellation");

            /// <summary>
            /// crm:E77_Persistent_Item
            /// </summary>
            public static readonly RDFResource E77_PERSISTENT_ITEM = new RDFResource(CRM.BASE_URI + "E77_Persistent_Item");

            /// <summary>
            /// crm:E78_Collection
            /// </summary>
            public static readonly RDFResource E78_COLLECTION = new RDFResource(CRM.BASE_URI + "E78_Collection");

            /// <summary>
            /// crm:E79_Part_Addition
            /// </summary>
            public static readonly RDFResource E79_PART_ADDITION = new RDFResource(CRM.BASE_URI + "E79_Part_Addition");

            /// <summary>
            /// crm:E80_Part_Removal
            /// </summary>
            public static readonly RDFResource E80_PART_REMOVAL = new RDFResource(CRM.BASE_URI + "E80_Part_Removal");

            /// <summary>
            /// crm:E81_Transformation
            /// </summary>
            public static readonly RDFResource E81_TRANSFORMATION = new RDFResource(CRM.BASE_URI + "E81_Transformation");

            /// <summary>
            /// crm:E82_Actor_Appellation
            /// </summary>
            public static readonly RDFResource E82_ACTOR_APPELLATION = new RDFResource(CRM.BASE_URI + "E82_Actor_Appellation");

            /// <summary>
            /// crm:E83_Type_Creation
            /// </summary>
            public static readonly RDFResource E83_TYPE_CREATION = new RDFResource(CRM.BASE_URI + "E83_Type_Creation");

            /// <summary>
            /// crm:E84_Information_Carrier
            /// </summary>
            public static readonly RDFResource E84_INFORMATION_CARRIER = new RDFResource(CRM.BASE_URI + "E84_Information_Carrier");

            /// <summary>
            /// crm:E85_Joining
            /// </summary>
            public static readonly RDFResource E85_JOINING = new RDFResource(CRM.BASE_URI + "E85_Joining");

            /// <summary>
            /// crm:E86_Leaving
            /// </summary>
            public static readonly RDFResource E86_LEAVING = new RDFResource(CRM.BASE_URI + "E86_Leaving");

            /// <summary>
            /// crm:E87_Curation_Activity
            /// </summary>
            public static readonly RDFResource E87_CURATION_ACTIVITY = new RDFResource(CRM.BASE_URI + "E87_Curation_Activity");

            /// <summary>
            /// crm:E89_Propositional_Object
            /// </summary>
            public static readonly RDFResource E89_PROPOSITIONAL_OBJECT = new RDFResource(CRM.BASE_URI + "E89_Propositional_Object");

            /// <summary>
            /// crm:E90_Symbolic_Object
            /// </summary>
            public static readonly RDFResource E90_SYMBOLIC_OBJECT = new RDFResource(CRM.BASE_URI + "E90_Symbolic_Object");

            /// <summary>
            /// crm:P1_is_identified_by
            /// </summary>
            public static readonly RDFResource P1_IS_IDENTIFIED_BY = new RDFResource(CRM.BASE_URI + "P1_is_identified_by");

            /// <summary>
            /// crm:P1i_identifies
            /// </summary>
            public static readonly RDFResource P1I_IDENTIFIES = new RDFResource(CRM.BASE_URI + "P1i_identifies");

            /// <summary>
            /// crm:P2_has_type
            /// </summary>
            public static readonly RDFResource P2_HAS_TYPE = new RDFResource(CRM.BASE_URI + "P2_has_type");

            /// <summary>
            /// crm:P2i_is_type_of
            /// </summary>
            public static readonly RDFResource P2I_IS_TYPE_OF = new RDFResource(CRM.BASE_URI + "P2i_is_type_of");

            /// <summary>
            /// crm:P3_has_note
            /// </summary>
            public static readonly RDFResource P3_HAS_NOTE = new RDFResource(CRM.BASE_URI + "P3_has_note");

            /// <summary>
            /// crm:P4_has_time-span
            /// </summary>
            public static readonly RDFResource P4_HAS_TIME_SPAN = new RDFResource(CRM.BASE_URI + "P4_has_time-span");

            /// <summary>
            /// crm:P4i_is_time-span_of
            /// </summary>
            public static readonly RDFResource P4I_IS_TIME_SPAN_OF = new RDFResource(CRM.BASE_URI + "P4i_is_time-span_of");

            /// <summary>
            /// crm:P5_consists_of
            /// </summary>
            public static readonly RDFResource P5_CONSISTS_OF = new RDFResource(CRM.BASE_URI + "P5_consists_of");

            /// <summary>
            /// crm:P5i_forms_part_of
            /// </summary>
            public static readonly RDFResource P5I_FORMS_PART_OF = new RDFResource(CRM.BASE_URI + "P5i_forms_part_of");

            /// <summary>
            /// crm:P7_took_place_at
            /// </summary>
            public static readonly RDFResource P7_TOOK_PLACE_AT = new RDFResource(CRM.BASE_URI + "P7_took_place_at");

            /// <summary>
            /// crm:P7i_witnessed
            /// </summary>
            public static readonly RDFResource P7I_WITNESSED = new RDFResource(CRM.BASE_URI + "P7i_witnessed");

            /// <summary>
            /// crm:P8_took_place_on_or_within
            /// </summary>
            public static readonly RDFResource P8_TOOK_PLACE_ON_OR_WITHIN = new RDFResource(CRM.BASE_URI + "P8_took_place_on_or_within");

            /// <summary>
            /// crm:P8i_witnessed
            /// </summary>
            public static readonly RDFResource P8I_WITNESSED = new RDFResource(CRM.BASE_URI + "P8i_witnessed");

            /// <summary>
            /// crm:P9_consists_of
            /// </summary>
            public static readonly RDFResource P9_CONSISTS_OF = new RDFResource(CRM.BASE_URI + "P9_consists_of");

            /// <summary>
            /// crm:P9i_forms_part_of
            /// </summary>
            public static readonly RDFResource P9I_FORMS_PART_OF = new RDFResource(CRM.BASE_URI + "P9i_forms_part_of");

            /// <summary>
            /// crm:P10_falls_within
            /// </summary>
            public static readonly RDFResource P10_FALLS_WITHIN = new RDFResource(CRM.BASE_URI + "P10_falls_within");

            /// <summary>
            /// crm:P10i_contains
            /// </summary>
            public static readonly RDFResource P10I_CONTAINS = new RDFResource(CRM.BASE_URI + "P10i_contains");

            /// <summary>
            /// crm:P11_had_participant
            /// </summary>
            public static readonly RDFResource P11_HAD_PARTICIPANT = new RDFResource(CRM.BASE_URI + "P11_had_participant");

            /// <summary>
            /// crm:P11i_participated_in
            /// </summary>
            public static readonly RDFResource P11I_PARTICIPATED_IN = new RDFResource(CRM.BASE_URI + "P11i_participated_in");

            /// <summary>
            /// crm:P12_occurred_in_the_presence_of
            /// </summary>
            public static readonly RDFResource P12_OCCURRED_IN_THE_PRESENCE_OF = new RDFResource(CRM.BASE_URI + "P12_occurred_in_the_presence_of");

            /// <summary>
            /// crm:P12i_was_present_at
            /// </summary>
            public static readonly RDFResource P12I_WAS_PRESENT_AT = new RDFResource(CRM.BASE_URI + "P12i_was_present_at");

            /// <summary>
            /// crm:P13_destroyed
            /// </summary>
            public static readonly RDFResource P13_DESTROYED = new RDFResource(CRM.BASE_URI + "P13_destroyed");

            /// <summary>
            /// crm:P13i_was_destroyed_by
            /// </summary>
            public static readonly RDFResource P13I_WAS_DESTROYED_BY = new RDFResource(CRM.BASE_URI + "P13i_was_destroyed_by");

            /// <summary>
            /// crm:P14_carried_out_by
            /// </summary>
            public static readonly RDFResource P14_CARRIED_OUT_BY = new RDFResource(CRM.BASE_URI + "P14_carried_out_by");

            /// <summary>
            /// crm:P14i_performed
            /// </summary>
            public static readonly RDFResource P14I_PERFORMED = new RDFResource(CRM.BASE_URI + "P14i_performed");

            /// <summary>
            /// crm:XXX
            /// </summary>
            public static readonly RDFResource XXX = new RDFResource(CRM.BASE_URI + "XXX");

            /// <summary>
            /// crm:P15_was_influenced_by
            /// </summary>
            public static readonly RDFResource P15_WAS_INFLUENCED_BY = new RDFResource(CRM.BASE_URI + "P15_was_influenced_by");

            /// <summary>
            /// crm:P15i_influenced
            /// </summary>
            public static readonly RDFResource P15I_INFLUENCED = new RDFResource(CRM.BASE_URI + "P15i_influenced");

            /// <summary>
            /// crm:P16_used_specific_object
            /// </summary>
            public static readonly RDFResource P16_USED_SPECIFIC_OBJECT = new RDFResource(CRM.BASE_URI + "P16_used_specific_object");

            /// <summary>
            /// crm:P16i_was_used_for
            /// </summary>
            public static readonly RDFResource P16I_WAS_USED_FOR = new RDFResource(CRM.BASE_URI + "P16i_was_used_for");

            /// <summary>
            /// crm:P17_was_motivated_by
            /// </summary>
            public static readonly RDFResource P17_WAS_MOTIVATED_BY = new RDFResource(CRM.BASE_URI + "P17_was_motivated_by");

            /// <summary>
            /// crm:P17i_motivated
            /// </summary>
            public static readonly RDFResource P17I_MOTIVATED = new RDFResource(CRM.BASE_URI + "P17i_motivated");
            #endregion

        }
        #endregion
    }
}