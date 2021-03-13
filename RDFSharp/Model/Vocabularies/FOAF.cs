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
            public static readonly RDFResource AGENT = new RDFResource(string.Concat(FOAF.BASE_URI, "Agent"));

            /// <summary>
            /// foaf:Person
            /// </summary>
            public static readonly RDFResource PERSON = new RDFResource(string.Concat(FOAF.BASE_URI, "Person"));

            /// <summary>
            /// foaf:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(string.Concat(FOAF.BASE_URI, "name"));

            /// <summary>
            /// foaf:title
            /// </summary>
            public static readonly RDFResource TITLE = new RDFResource(string.Concat(FOAF.BASE_URI, "title"));

            /// <summary>
            /// foaf:img
            /// </summary>
            public static readonly RDFResource IMG = new RDFResource(string.Concat(FOAF.BASE_URI, "img"));

            /// <summary>
            /// foaf:depiction
            /// </summary>
            public static readonly RDFResource DEPICTION = new RDFResource(string.Concat(FOAF.BASE_URI, "depiction"));

            /// <summary>
            /// foaf:depicts
            /// </summary>
            public static readonly RDFResource DEPICTS = new RDFResource(string.Concat(FOAF.BASE_URI, "depicts"));

            /// <summary>
            /// foaf:familyName
            /// </summary>
            public static readonly RDFResource FAMILY_NAME = new RDFResource(string.Concat(FOAF.BASE_URI, "familyName"));

            /// <summary>
            /// foaf:givenName
            /// </summary>
            public static readonly RDFResource GIVEN_NAME = new RDFResource(string.Concat(FOAF.BASE_URI, "givenName"));

            /// <summary>
            /// foaf:knows
            /// </summary>
            public static readonly RDFResource KNOWS = new RDFResource(string.Concat(FOAF.BASE_URI, "knows"));

            /// <summary>
            /// foaf:skypeID
            /// </summary>
            public static readonly RDFResource SKYPE_ID = new RDFResource(string.Concat(FOAF.BASE_URI, "skypeID"));

            /// <summary>
            /// foaf:based_near
            /// </summary>
            public static readonly RDFResource BASED_NEAR = new RDFResource(string.Concat(FOAF.BASE_URI, "based_near"));

            /// <summary>
            /// foaf:age
            /// </summary>
            public static readonly RDFResource AGE = new RDFResource(string.Concat(FOAF.BASE_URI, "age"));

            /// <summary>
            /// foaf:made
            /// </summary>
            public static readonly RDFResource MADE = new RDFResource(string.Concat(FOAF.BASE_URI, "made"));

            /// <summary>
            /// foaf:maker
            /// </summary>
            public static readonly RDFResource MAKER = new RDFResource(string.Concat(FOAF.BASE_URI, "maker"));

            /// <summary>
            /// foaf:primaryTopic
            /// </summary>
            public static readonly RDFResource PRIMARY_TOPIC = new RDFResource(string.Concat(FOAF.BASE_URI, "primaryTopic"));

            /// <summary>
            /// foaf:isPrimaryTopicOf
            /// </summary>
            public static readonly RDFResource IS_PRIMARY_TOPIC_OF = new RDFResource(string.Concat(FOAF.BASE_URI, "isPrimaryTopicOf"));

            /// <summary>
            /// foaf:Project
            /// </summary>
            public static readonly RDFResource PROJECT = new RDFResource(string.Concat(FOAF.BASE_URI, "Project"));

            /// <summary>
            /// foaf:Organization
            /// </summary>
            public static readonly RDFResource ORGANIZATION = new RDFResource(string.Concat(FOAF.BASE_URI, "Organization"));

            /// <summary>
            /// foaf:Group
            /// </summary>
            public static readonly RDFResource GROUP = new RDFResource(string.Concat(FOAF.BASE_URI, "Group"));

            /// <summary>
            /// foaf:Document
            /// </summary>
            public static readonly RDFResource DOCUMENT = new RDFResource(string.Concat(FOAF.BASE_URI, "Document"));

            /// <summary>
            /// foaf:Image
            /// </summary>
            public static readonly RDFResource IMAGE = new RDFResource(string.Concat(FOAF.BASE_URI, "Image"));

            /// <summary>
            /// foaf:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource(string.Concat(FOAF.BASE_URI, "member"));

            /// <summary>
            /// foaf:focus
            /// </summary>
            public static readonly RDFResource FOCUS = new RDFResource(string.Concat(FOAF.BASE_URI, "focus"));

            /// <summary>
            /// foaf:fundedBy
            /// </summary>
            public static readonly RDFResource FUNDED_BY = new RDFResource(string.Concat(FOAF.BASE_URI, "fundedBy"));

            /// <summary>
            /// foaf:geekcode
            /// </summary>
            public static readonly RDFResource GEEK_CODE = new RDFResource(string.Concat(FOAF.BASE_URI, "geekcode"));

            /// <summary>
            /// foaf:theme
            /// </summary>
            public static readonly RDFResource THEME = new RDFResource(string.Concat(FOAF.BASE_URI, "theme"));

            /// <summary>
            /// foaf:nick
            /// </summary>
            public static readonly RDFResource NICK = new RDFResource(string.Concat(FOAF.BASE_URI, "nick"));

            /// <summary>
            /// foaf:mbox
            /// </summary>
            public static readonly RDFResource MBOX = new RDFResource(string.Concat(FOAF.BASE_URI, "mbox"));

            /// <summary>
            /// foaf:homepage
            /// </summary>
            public static readonly RDFResource HOMEPAGE = new RDFResource(string.Concat(FOAF.BASE_URI, "homepage"));

            /// <summary>
            /// foaf:weblog
            /// </summary>
            public static readonly RDFResource WEBLOG = new RDFResource(string.Concat(FOAF.BASE_URI, "weblog"));

            /// <summary>
            /// foaf:openid
            /// </summary>
            public static readonly RDFResource OPEN_ID = new RDFResource(string.Concat(FOAF.BASE_URI, "openid"));

            /// <summary>
            /// foaf:jabberID
            /// </summary>
            public static readonly RDFResource JABBER_ID = new RDFResource(string.Concat(FOAF.BASE_URI, "jabberID"));

            /// <summary>
            /// foaf:aimChatID
            /// </summary>
            public static readonly RDFResource AIM_CHAT_ID = new RDFResource(string.Concat(FOAF.BASE_URI, "aimChatID"));

            /// <summary>
            /// foaf:icqChatID
            /// </summary>
            public static readonly RDFResource ICQ_CHAT_ID = new RDFResource(string.Concat(FOAF.BASE_URI, "icqChatID"));

            /// <summary>
            /// foaf:msnChatID
            /// </summary>
            public static readonly RDFResource MSN_CHAT_ID = new RDFResource(string.Concat(FOAF.BASE_URI, "msnChatID"));

            /// <summary>
            /// foaf:yahooChatID
            /// </summary>
            public static readonly RDFResource YAHOO_CHAT_ID = new RDFResource(string.Concat(FOAF.BASE_URI, "yahooChatID"));

            /// <summary>
            /// foaf:myersBriggs
            /// </summary>
            public static readonly RDFResource MYERS_BRIGGS = new RDFResource(string.Concat(FOAF.BASE_URI, "myersBriggs"));

            /// <summary>
            /// foaf:dnaChecksum
            /// </summary>
            public static readonly RDFResource DNA_CHECKSUM = new RDFResource(string.Concat(FOAF.BASE_URI, "dnaChecksum"));

            /// <summary>
            /// foaf:membershipClass
            /// </summary>
            public static readonly RDFResource MEMBERSHIP_CLASS = new RDFResource(string.Concat(FOAF.BASE_URI, "membershipClass"));

            /// <summary>
            /// foaf:holdsAccount
            /// </summary>
            public static readonly RDFResource HOLDS_ACCOUNT = new RDFResource(string.Concat(FOAF.BASE_URI, "holdsAccount"));

            /// <summary>
            /// foaf:firstName
            /// </summary>
            public static readonly RDFResource FIRSTNAME = new RDFResource(string.Concat(FOAF.BASE_URI, "firstName"));

            /// <summary>
            /// foaf:surname
            /// </summary>
            public static readonly RDFResource SURNAME = new RDFResource(string.Concat(FOAF.BASE_URI, "surname"));

            /// <summary>
            /// foaf:plan
            /// </summary>
            public static readonly RDFResource PLAN = new RDFResource(string.Concat(FOAF.BASE_URI, "plan"));

            /// <summary>
            /// foaf:mbox_sha1sum
            /// </summary>
            public static readonly RDFResource MBOX_SHA1SUM = new RDFResource(string.Concat(FOAF.BASE_URI, "mbox_sha1sum"));

            /// <summary>
            /// foaf:interest
            /// </summary>
            public static readonly RDFResource INTEREST = new RDFResource(string.Concat(FOAF.BASE_URI, "interest"));

            /// <summary>
            /// foaf:topic_interest
            /// </summary>
            public static readonly RDFResource TOPIC_INTEREST = new RDFResource(string.Concat(FOAF.BASE_URI, "topic_interest"));

            /// <summary>
            /// foaf:topic
            /// </summary>
            public static readonly RDFResource TOPIC = new RDFResource(string.Concat(FOAF.BASE_URI, "topic"));

            /// <summary>
            /// foaf:page
            /// </summary>
            public static readonly RDFResource PAGE = new RDFResource(string.Concat(FOAF.BASE_URI, "page"));

            /// <summary>
            /// foaf:workplaceHomepage
            /// </summary>
            public static readonly RDFResource WORKPLACE_HOMEPAGE = new RDFResource(string.Concat(FOAF.BASE_URI, "workplaceHomepage"));

            /// <summary>
            /// foaf:workinfoHomepage
            /// </summary>
            public static readonly RDFResource WORKINFO_HOMEPAGE = new RDFResource(string.Concat(FOAF.BASE_URI, "workinfoHomepage"));

            /// <summary>
            /// foaf:schoolHomepage
            /// </summary>
            public static readonly RDFResource SCHOOL_HOMEPAGE = new RDFResource(string.Concat(FOAF.BASE_URI, "schoolHomepage"));

            /// <summary>
            /// foaf:publications
            /// </summary>
            public static readonly RDFResource PUBLICATIONS = new RDFResource(string.Concat(FOAF.BASE_URI, "publications"));

            /// <summary>
            /// foaf:currentProject
            /// </summary>
            public static readonly RDFResource CURRENT_PROJECT = new RDFResource(string.Concat(FOAF.BASE_URI, "currentProject"));

            /// <summary>
            /// foaf:pastProject
            /// </summary>
            public static readonly RDFResource PAST_PROJECT = new RDFResource(string.Concat(FOAF.BASE_URI, "pastProject"));

            /// <summary>
            /// foaf:account
            /// </summary>
            public static readonly RDFResource ACCOUNT = new RDFResource(string.Concat(FOAF.BASE_URI, "account"));

            /// <summary>
            /// foaf:OnlineAccount
            /// </summary>
            public static readonly RDFResource ONLINE_ACCOUNT = new RDFResource(string.Concat(FOAF.BASE_URI, "OnlineAccount"));

            /// <summary>
            /// foaf:OnlineChatAccount
            /// </summary>
            public static readonly RDFResource ONLINE_CHAT_ACCOUNT = new RDFResource(string.Concat(FOAF.BASE_URI, "OnlineChatAccount"));

            /// <summary>
            /// foaf:OnlineEcommerceAccount
            /// </summary>
            public static readonly RDFResource ONLINE_ECOMMERCE_ACCOUNT = new RDFResource(string.Concat(FOAF.BASE_URI, "OnlineEcommerceAccount"));

            /// <summary>
            /// foaf:OnlineGamingAccount
            /// </summary>
            public static readonly RDFResource ONLINE_GAMING_ACCOUNT = new RDFResource(string.Concat(FOAF.BASE_URI, "OnlineGamingAccount"));

            /// <summary>
            /// foaf:accountName
            /// </summary>
            public static readonly RDFResource ACCOUNT_NAME = new RDFResource(string.Concat(FOAF.BASE_URI, "accountName"));

            /// <summary>
            /// foaf:accountServiceHomepage
            /// </summary>
            public static readonly RDFResource ACCOUNT_SERVICE_HOMEPAGE = new RDFResource(string.Concat(FOAF.BASE_URI, "accountServiceHomepage"));

            /// <summary>
            /// foaf:PersonalProfileDocument
            /// </summary>
            public static readonly RDFResource PERSONAL_PROFILE_DOCUMENT = new RDFResource(string.Concat(FOAF.BASE_URI, "PersonalProfileDocument"));

            /// <summary>
            /// foaf:tipjar
            /// </summary>
            public static readonly RDFResource TIPJAR = new RDFResource(string.Concat(FOAF.BASE_URI, "tipjar"));

            /// <summary>
            /// foaf:sha1
            /// </summary>
            public static readonly RDFResource SHA1 = new RDFResource(string.Concat(FOAF.BASE_URI, "sha1"));

            /// <summary>
            /// foaf:thumbnail
            /// </summary>
            public static readonly RDFResource THUMBNAIL = new RDFResource(string.Concat(FOAF.BASE_URI, "thumbnail"));

            /// <summary>
            /// foaf:logo
            /// </summary>
            public static readonly RDFResource LOGO = new RDFResource(string.Concat(FOAF.BASE_URI, "logo"));

            /// <summary>
            /// foaf:phone
            /// </summary>
            public static readonly RDFResource PHONE = new RDFResource(string.Concat(FOAF.BASE_URI, "phone"));

            /// <summary>
            /// foaf:status
            /// </summary>
            public static readonly RDFResource STATUS = new RDFResource(string.Concat(FOAF.BASE_URI, "status"));

            /// <summary>
            /// foaf:gender
            /// </summary>
            public static readonly RDFResource GENDER = new RDFResource(string.Concat(FOAF.BASE_URI, "gender"));

            /// <summary>
            /// foaf:birthday
            /// </summary>
            public static readonly RDFResource BIRTHDAY = new RDFResource(string.Concat(FOAF.BASE_URI, "birthday"));
            #endregion

        }
        #endregion
    }
}