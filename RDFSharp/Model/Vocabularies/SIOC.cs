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
        #region SIOC
        /// <summary>
        /// SIOC represents the Semantically-Interlinked Online Communities vocabulary.
        /// </summary>
        public static class SIOC
        {

            #region Properties
            /// <summary>
            /// sioc
            /// </summary>
            public static readonly string PREFIX = "sioc";

            /// <summary>
            /// http://rdfs.org/sioc/ns#
            /// </summary>
            public static readonly string BASE_URI = "http://rdfs.org/sioc/ns#";

            /// <summary>
            /// http://rdfs.org/sioc/ns#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://rdfs.org/sioc/ns#";

            /// <summary>
            /// sioc:Community
            /// </summary>
            public static readonly RDFResource COMMUNITY = new RDFResource(string.Concat(SIOC.BASE_URI,"Community"));

            /// <summary>
            /// sioc:Container
            /// </summary>
            public static readonly RDFResource CONTAINER = new RDFResource(string.Concat(SIOC.BASE_URI,"Container"));

            /// <summary>
            /// sioc:Forum
            /// </summary>
            public static readonly RDFResource FORUM = new RDFResource(string.Concat(SIOC.BASE_URI,"Forum"));

            /// <summary>
            /// sioc:Item
            /// </summary>
            public static readonly RDFResource ITEM = new RDFResource(string.Concat(SIOC.BASE_URI,"Item"));

            /// <summary>
            /// sioc:Post
            /// </summary>
            public static readonly RDFResource POST = new RDFResource(string.Concat(SIOC.BASE_URI,"Post"));

            /// <summary>
            /// sioc:Role
            /// </summary>
            public static readonly RDFResource ROLE = new RDFResource(string.Concat(SIOC.BASE_URI,"Role"));

            /// <summary>
            /// sioc:Space
            /// </summary>
            public static readonly RDFResource SPACE = new RDFResource(string.Concat(SIOC.BASE_URI,"Space"));

            /// <summary>
            /// sioc:Site
            /// </summary>
            public static readonly RDFResource SITE = new RDFResource(string.Concat(SIOC.BASE_URI,"Site"));

            /// <summary>
            /// sioc:Thread
            /// </summary>
            public static readonly RDFResource THREAD = new RDFResource(string.Concat(SIOC.BASE_URI,"Thread"));

            /// <summary>
            /// sioc:UserAccount
            /// </summary>
            public static readonly RDFResource USER_ACCOUNT = new RDFResource(string.Concat(SIOC.BASE_URI,"UserAccount"));

            /// <summary>
            /// sioc:Usergroup
            /// </summary>
            public static readonly RDFResource USER_GROUP = new RDFResource(string.Concat(SIOC.BASE_URI,"Usergroup"));

            /// <summary>
            /// sioc:about
            /// </summary>
            public static readonly RDFResource ABOUT = new RDFResource(string.Concat(SIOC.BASE_URI,"about"));

            /// <summary>
            /// sioc:account_of
            /// </summary>
            public static readonly RDFResource ACCOUNT_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"account_of"));

            /// <summary>
            /// sioc:addressed_to
            /// </summary>
            public static readonly RDFResource ADDRESSED_TO = new RDFResource(string.Concat(SIOC.BASE_URI,"addressed_to"));

            /// <summary>
            /// sioc:administrator_of
            /// </summary>
            public static readonly RDFResource ADMINISTRATOR_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"administrator_of"));

            /// <summary>
            /// sioc:attachment
            /// </summary>
            public static readonly RDFResource ATTACHMENT = new RDFResource(string.Concat(SIOC.BASE_URI,"attachment"));

            /// <summary>
            /// sioc:avatar
            /// </summary>
            public static readonly RDFResource AVATAR = new RDFResource(string.Concat(SIOC.BASE_URI,"avatar"));

            /// <summary>
            /// sioc:container_of
            /// </summary>
            public static readonly RDFResource CONTAINER_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"container_of"));

            /// <summary>
            /// sioc:content
            /// </summary>
            public static readonly RDFResource CONTENT = new RDFResource(string.Concat(SIOC.BASE_URI,"content"));

            /// <summary>
            /// sioc:creator_of
            /// </summary>
            public static readonly RDFResource CREATOR_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"creator_of"));

            /// <summary>
            /// sioc:earlier_version
            /// </summary>
            public static readonly RDFResource EARLIER_VERSION = new RDFResource(string.Concat(SIOC.BASE_URI,"earlier_version"));

            /// <summary>
            /// sioc:email
            /// </summary>
            public static readonly RDFResource EMAIL = new RDFResource(string.Concat(SIOC.BASE_URI,"email"));

            /// <summary>
            /// sioc:email_sha1
            /// </summary>
            public static readonly RDFResource EMAIL_SHA1 = new RDFResource(string.Concat(SIOC.BASE_URI,"email_sha1"));

            /// <summary>
            /// sioc:embeds_knowledge
            /// </summary>
            public static readonly RDFResource EMBEDS_KNOWLEDGE = new RDFResource(string.Concat(SIOC.BASE_URI,"embeds_knowledge"));

            /// <summary>
            /// sioc:feed
            /// </summary>
            public static readonly RDFResource FEED = new RDFResource(string.Concat(SIOC.BASE_URI,"feed"));

            /// <summary>
            /// sioc:follows
            /// </summary>
            public static readonly RDFResource FOLLOWS = new RDFResource(string.Concat(SIOC.BASE_URI,"follows"));

            /// <summary>
            /// sioc:function_of
            /// </summary>
            public static readonly RDFResource FUNCTION_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"function_of"));

            /// <summary>
            /// sioc:has_administrator
            /// </summary>
            public static readonly RDFResource HAS_ADMINISTRATOR = new RDFResource(string.Concat(SIOC.BASE_URI,"has_administrator"));

            /// <summary>
            /// sioc:has_container
            /// </summary>
            public static readonly RDFResource HAS_CONTAINER = new RDFResource(string.Concat(SIOC.BASE_URI,"has_container"));

            /// <summary>
            /// sioc:has_creator
            /// </summary>
            public static readonly RDFResource HAS_CREATOR = new RDFResource(string.Concat(SIOC.BASE_URI,"has_creator"));

            /// <summary>
            /// sioc:has_discussion
            /// </summary>
            public static readonly RDFResource HAS_DISCUSSION = new RDFResource(string.Concat(SIOC.BASE_URI,"has_discussion"));

            /// <summary>
            /// sioc:has_function
            /// </summary>
            public static readonly RDFResource HAS_FUNCTION = new RDFResource(string.Concat(SIOC.BASE_URI,"has_function"));

            /// <summary>
            /// sioc:has_host
            /// </summary>
            public static readonly RDFResource HAS_HOST = new RDFResource(string.Concat(SIOC.BASE_URI,"has_host"));

            /// <summary>
            /// sioc:has_member
            /// </summary>
            public static readonly RDFResource HAS_MEMBER = new RDFResource(string.Concat(SIOC.BASE_URI,"has_member"));

            /// <summary>
            /// sioc:has_moderator
            /// </summary>
            public static readonly RDFResource HAS_MODERATOR = new RDFResource(string.Concat(SIOC.BASE_URI,"has_moderator"));

            /// <summary>
            /// sioc:has_modifier
            /// </summary>
            public static readonly RDFResource HAS_MODIFIER = new RDFResource(string.Concat(SIOC.BASE_URI,"has_modifier"));

            /// <summary>
            /// sioc:has_owner
            /// </summary>
            public static readonly RDFResource HAS_OWNER = new RDFResource(string.Concat(SIOC.BASE_URI,"has_owner"));

            /// <summary>
            /// sioc:has_parent
            /// </summary>
            public static readonly RDFResource HAS_PARENT = new RDFResource(string.Concat(SIOC.BASE_URI,"has_parent"));

            /// <summary>
            /// sioc:has_reply
            /// </summary>
            public static readonly RDFResource HAS_REPLY = new RDFResource(string.Concat(SIOC.BASE_URI,"has_reply"));

            /// <summary>
            /// sioc:has_scope
            /// </summary>
            public static readonly RDFResource HAS_SCOPE = new RDFResource(string.Concat(SIOC.BASE_URI,"has_scope"));

            /// <summary>
            /// sioc:has_space
            /// </summary>
            public static readonly RDFResource HAS_SPACE = new RDFResource(string.Concat(SIOC.BASE_URI,"has_space"));

            /// <summary>
            /// sioc:has_subscriber
            /// </summary>
            public static readonly RDFResource HAS_SUBSCRIBER = new RDFResource(string.Concat(SIOC.BASE_URI,"has_subscriber"));

            /// <summary>
            /// sioc:has_usergroup
            /// </summary>
            public static readonly RDFResource HAS_USERGROUP = new RDFResource(string.Concat(SIOC.BASE_URI,"has_usergroup"));

            /// <summary>
            /// sioc:host_of
            /// </summary>
            public static readonly RDFResource HOST_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"host_of"));

            /// <summary>
            /// sioc:id
            /// </summary>
            public static readonly RDFResource ID = new RDFResource(string.Concat(SIOC.BASE_URI,"id"));

            /// <summary>
            /// sioc:ip_address
            /// </summary>
            public static readonly RDFResource IP_ADDRESS = new RDFResource(string.Concat(SIOC.BASE_URI,"ip_address"));

            /// <summary>
            /// sioc:last_activity_date
            /// </summary>
            public static readonly RDFResource LAST_ACTIVITY_DATE = new RDFResource(string.Concat(SIOC.BASE_URI,"last_activity_date"));

            /// <summary>
            /// sioc:last_item_date
            /// </summary>
            public static readonly RDFResource LAST_ITEM_DATE = new RDFResource(string.Concat(SIOC.BASE_URI,"last_item_date"));

            /// <summary>
            /// sioc:last_reply_date
            /// </summary>
            public static readonly RDFResource LAST_REPLY_DATE = new RDFResource(string.Concat(SIOC.BASE_URI,"last_reply_date"));

            /// <summary>
            /// sioc:later_version
            /// </summary>
            public static readonly RDFResource LATER_VERSION = new RDFResource(string.Concat(SIOC.BASE_URI,"later_version"));

            /// <summary>
            /// sioc:latest_version
            /// </summary>
            public static readonly RDFResource LATEST_VERSION = new RDFResource(string.Concat(SIOC.BASE_URI,"latest_version"));

            /// <summary>
            /// sioc:link
            /// </summary>
            public static readonly RDFResource LINK = new RDFResource(string.Concat(SIOC.BASE_URI,"link"));

            /// <summary>
            /// sioc:links_to
            /// </summary>
            public static readonly RDFResource LINKS_TO = new RDFResource(string.Concat(SIOC.BASE_URI,"links_to"));

            /// <summary>
            /// sioc:member_of
            /// </summary>
            public static readonly RDFResource MEMBER_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"member_of"));

            /// <summary>
            /// sioc:moderator_of
            /// </summary>
            public static readonly RDFResource MODERATOR_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"moderator_of"));

            /// <summary>
            /// sioc:modifier_of
            /// </summary>
            public static readonly RDFResource MODIFIER_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"modifier_of"));

            /// <summary>
            /// sioc:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(string.Concat(SIOC.BASE_URI,"name"));

            /// <summary>
            /// sioc:next_by_date
            /// </summary>
            public static readonly RDFResource NEXT_BY_DATE = new RDFResource(string.Concat(SIOC.BASE_URI,"next_by_date"));

            /// <summary>
            /// sioc:next_version
            /// </summary>
            public static readonly RDFResource NEXT_VERSION = new RDFResource(string.Concat(SIOC.BASE_URI,"next_version"));

            /// <summary>
            /// sioc:note
            /// </summary>
            public static readonly RDFResource NOTE = new RDFResource(string.Concat(SIOC.BASE_URI,"note"));

            /// <summary>
            /// sioc:num_authors
            /// </summary>
            public static readonly RDFResource NUM_AUTHORS = new RDFResource(string.Concat(SIOC.BASE_URI,"num_authors"));

            /// <summary>
            /// sioc:num_items
            /// </summary>
            public static readonly RDFResource NUM_ITEMS = new RDFResource(string.Concat(SIOC.BASE_URI,"num_items"));

            /// <summary>
            /// sioc:num_replies
            /// </summary>
            public static readonly RDFResource NUM_REPLIES = new RDFResource(string.Concat(SIOC.BASE_URI,"num_replies"));

            /// <summary>
            /// sioc:num_threads
            /// </summary>
            public static readonly RDFResource NUM_THREADS = new RDFResource(string.Concat(SIOC.BASE_URI,"num_threads"));

            /// <summary>
            /// sioc:num_views
            /// </summary>
            public static readonly RDFResource NUM_VIEWS = new RDFResource(string.Concat(SIOC.BASE_URI,"num_views"));

            /// <summary>
            /// sioc:owner_of
            /// </summary>
            public static readonly RDFResource OWNER_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"owner_of"));

            /// <summary>
            /// sioc:parent_of
            /// </summary>
            public static readonly RDFResource PARENT_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"parent_of"));

            /// <summary>
            /// sioc:previous_by_date
            /// </summary>
            public static readonly RDFResource PREVIOUS_BY_DATE = new RDFResource(string.Concat(SIOC.BASE_URI,"previous_by_date"));

            /// <summary>
            /// sioc:previous_version
            /// </summary>
            public static readonly RDFResource PREVIOUS_VERSION = new RDFResource(string.Concat(SIOC.BASE_URI,"previous_version"));

            /// <summary>
            /// sioc:related_to
            /// </summary>
            public static readonly RDFResource RELATED_TO = new RDFResource(string.Concat(SIOC.BASE_URI,"related_to"));

            /// <summary>
            /// sioc:reply_of
            /// </summary>
            public static readonly RDFResource REPLY_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"reply_of"));

            /// <summary>
            /// sioc:scope_of
            /// </summary>
            public static readonly RDFResource SCOPE_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"scope_of"));

            /// <summary>
            /// sioc:sibling
            /// </summary>
            public static readonly RDFResource SIBLING = new RDFResource(string.Concat(SIOC.BASE_URI,"sibling"));

            /// <summary>
            /// sioc:space_of
            /// </summary>
            public static readonly RDFResource SPACE_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"space_of"));

            /// <summary>
            /// sioc:subscriber_of
            /// </summary>
            public static readonly RDFResource SUBSCRIBER_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"subscriber_of"));

            /// <summary>
            /// sioc:topic
            /// </summary>
            public static readonly RDFResource TOPIC = new RDFResource(string.Concat(SIOC.BASE_URI,"topic"));

            /// <summary>
            /// sioc:usergroup_of
            /// </summary>
            public static readonly RDFResource USERGROUP_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"usergroup_of"));

            /// <summary>
            /// sioc:User
            /// </summary>
            public static readonly RDFResource USER = new RDFResource(string.Concat(SIOC.BASE_URI,"User"));

            /// <summary>
            /// sioc:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource(string.Concat(SIOC.BASE_URI,"title"));

            /// <summary>
            /// sioc:content_ecoded
            /// </summary>
            public static readonly RDFResource CONTENT_ENCODED = new RDFResource(string.Concat(SIOC.BASE_URI,"content_ecoded"));

            /// <summary>
            /// sioc:created_at
            /// </summary>
            public static readonly RDFResource CREATED_AT = new RDFResource(string.Concat(SIOC.BASE_URI,"created_at"));

            /// <summary>
            /// sioc:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(string.Concat(SIOC.BASE_URI,"description"));

            /// <summary>
            /// sioc:first_name
            /// </summary>
            public static readonly RDFResource FIRST_NAME = new RDFResource(string.Concat(SIOC.BASE_URI,"first_name"));

            /// <summary>
            /// sioc:last_name
            /// </summary>
            public static readonly RDFResource LAST_NAME = new RDFResource(string.Concat(SIOC.BASE_URI,"last_name"));

            /// <summary>
            /// sioc:group_of
            /// </summary>
            public static readonly RDFResource GROUP_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"group_of"));

            /// <summary>
            /// sioc:has_group
            /// </summary>
            public static readonly RDFResource HAS_GROUP = new RDFResource(string.Concat(SIOC.BASE_URI,"has_group"));

            /// <summary>
            /// sioc:has_part
            /// </summary>
            public static readonly RDFResource HAS_PART = new RDFResource(string.Concat(SIOC.BASE_URI,"has_part"));

            /// <summary>
            /// sioc:modified_at
            /// </summary>
            public static readonly RDFResource MODIFIED_AT = new RDFResource(string.Concat(SIOC.BASE_URI,"modified_at"));

            /// <summary>
            /// sioc:part_of
            /// </summary>
            public static readonly RDFResource PART_OF = new RDFResource(string.Concat(SIOC.BASE_URI,"part_of"));

            /// <summary>
            /// sioc:reference
            /// </summary>
            public static readonly RDFResource REFERENCE = new RDFResource(string.Concat(SIOC.BASE_URI,"reference"));

            /// <summary>
            /// sioc:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(string.Concat(SIOC.BASE_URI,"subject"));
            #endregion

        }
        #endregion
    }
}