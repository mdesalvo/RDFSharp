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
        #region DOAP
        /// <summary>
        /// DOAP represents the Description-of-a-Project vocabulary.
        /// </summary>
        public static class DOAP
        {

            #region Properties
            /// <summary>
            /// doap
            /// </summary>
            public static readonly string PREFIX = "doap";

            /// <summary>
            /// http://usefulinc.com/ns/doap#
            /// </summary>
            public static readonly string BASE_URI = "http://usefulinc.com/ns/doap#";

            /// <summary>
            /// http://usefulinc.com/ns/doap#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://usefulinc.com/ns/doap#";

            /// <summary>
            /// doap:Project
            /// </summary>
            public static readonly RDFResource PROJECT = new RDFResource(string.Concat(DOAP.BASE_URI, "Project"));

            /// <summary>
            /// doap:Version
            /// </summary>
            public static readonly RDFResource VERSION = new RDFResource(string.Concat(DOAP.BASE_URI, "Version"));

            /// <summary>
            /// doap:Specification
            /// </summary>
            public static readonly RDFResource SPECIFICATION = new RDFResource(string.Concat(DOAP.BASE_URI, "Specification"));

            /// <summary>
            /// doap:Repository
            /// </summary>
            public static readonly RDFResource REPOSITORY_CLASS = new RDFResource(string.Concat(DOAP.BASE_URI, "Repository"));

            /// <summary>
            /// doap:SVNRepository
            /// </summary>
            public static readonly RDFResource SVN_REPOSITORY = new RDFResource(string.Concat(DOAP.BASE_URI, "SVNRepository"));

            /// <summary>
            /// doap:GitRepository
            /// </summary>
            public static readonly RDFResource GIT_REPOSITORY = new RDFResource(string.Concat(DOAP.BASE_URI, "GitRepository"));

            /// <summary>
            /// doap:BKRepository
            /// </summary>
            public static readonly RDFResource BK_REPOSITORY = new RDFResource(string.Concat(DOAP.BASE_URI, "BKRepository"));

            /// <summary>
            /// doap:CVSRepository
            /// </summary>
            public static readonly RDFResource CVS_REPOSITORY = new RDFResource(string.Concat(DOAP.BASE_URI, "CVSRepository"));

            /// <summary>
            /// doap:ArchRepository
            /// </summary>
            public static readonly RDFResource ARCH_REPOSITORY = new RDFResource(string.Concat(DOAP.BASE_URI, "ArchRepository"));

            /// <summary>
            /// doap:HgRepository
            /// </summary>
            public static readonly RDFResource HG_REPOSITORY = new RDFResource(string.Concat(DOAP.BASE_URI, "HgRepository"));

            /// <summary>
            /// doap:DarcsRepository
            /// </summary>
            public static readonly RDFResource DARCS_REPOSITORY = new RDFResource(string.Concat(DOAP.BASE_URI, "DarcsRepository"));

            /// <summary>
            /// doap:BazaarBranch
            /// </summary>
            public static readonly RDFResource BAZAAR_BRANCH = new RDFResource(string.Concat(DOAP.BASE_URI, "BazaarBranch"));

            /// <summary>
            /// doap:GitBranch
            /// </summary>
            public static readonly RDFResource GIT_BRANCH = new RDFResource(string.Concat(DOAP.BASE_URI, "GitBranch"));

            /// <summary>
            /// doap:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(string.Concat(DOAP.BASE_URI, "name"));

            /// <summary>
            /// doap:homepage
            /// </summary>
            public static readonly RDFResource HOMEPAGE = new RDFResource(string.Concat(DOAP.BASE_URI, "homepage"));

            /// <summary>
            /// doap:old-homepage
            /// </summary>
            public static readonly RDFResource OLD_HOMEPAGE = new RDFResource(string.Concat(DOAP.BASE_URI, "old-homepage"));

            /// <summary>
            /// doap:created
            /// </summary>
            public static readonly RDFResource CREATED = new RDFResource(string.Concat(DOAP.BASE_URI, "created"));

            /// <summary>
            /// doap:shortdesc
            /// </summary>
            public static readonly RDFResource SHORTDESC = new RDFResource(string.Concat(DOAP.BASE_URI, "shortdesc"));

            /// <summary>
            /// doap:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(string.Concat(DOAP.BASE_URI, "description"));

            /// <summary>
            /// doap:release
            /// </summary>
            public static readonly RDFResource RELEASE = new RDFResource(string.Concat(DOAP.BASE_URI, "release"));

            /// <summary>
            /// doap:mailing-list
            /// </summary>
            public static readonly RDFResource MAILING_LIST = new RDFResource(string.Concat(DOAP.BASE_URI, "mailing-list"));

            /// <summary>
            /// doap:support-forum
            /// </summary>
            public static readonly RDFResource SUPPORT_FORUM = new RDFResource(string.Concat(DOAP.BASE_URI, "support-forum"));

            /// <summary>
            /// doap:developer-forum
            /// </summary>
            public static readonly RDFResource DEVELOPER_FORUM = new RDFResource(string.Concat(DOAP.BASE_URI, "developer-forum"));

            /// <summary>
            /// doap:category
            /// </summary>
            public static readonly RDFResource CATEGORY = new RDFResource(string.Concat(DOAP.BASE_URI, "category"));

            /// <summary>
            /// doap:license
            /// </summary>
            public static readonly RDFResource LICENSE = new RDFResource(string.Concat(DOAP.BASE_URI, "license"));

            /// <summary>
            /// doap:repository
            /// </summary>
            public static readonly RDFResource REPOSITORY_PROPERTY = new RDFResource(string.Concat(DOAP.BASE_URI, "repository"));

            /// <summary>
            /// doap:repositoryOf
            /// </summary>
            public static readonly RDFResource REPOSITORYOF = new RDFResource(string.Concat(DOAP.BASE_URI, "repositoryOf"));

            /// <summary>
            /// doap:anon-root
            /// </summary>
            public static readonly RDFResource ANON_ROOT = new RDFResource(string.Concat(DOAP.BASE_URI, "anon-root"));

            /// <summary>
            /// doap:browse
            /// </summary>
            public static readonly RDFResource BROWSE = new RDFResource(string.Concat(DOAP.BASE_URI, "browse"));

            /// <summary>
            /// doap:module
            /// </summary>
            public static readonly RDFResource MODULE = new RDFResource(string.Concat(DOAP.BASE_URI, "module"));

            /// <summary>
            /// doap:location
            /// </summary>
            public static readonly RDFResource LOCATION = new RDFResource(string.Concat(DOAP.BASE_URI, "location"));

            /// <summary>
            /// doap:download-page
            /// </summary>
            public static readonly RDFResource DOWNLOAD_PAGE = new RDFResource(string.Concat(DOAP.BASE_URI, "download-page"));

            /// <summary>
            /// doap:download-mirror
            /// </summary>
            public static readonly RDFResource DOWNLOAD_MIRROR = new RDFResource(string.Concat(DOAP.BASE_URI, "download-mirror"));

            /// <summary>
            /// doap:revision
            /// </summary>
            public static readonly RDFResource REVISION = new RDFResource(string.Concat(DOAP.BASE_URI, "revision"));

            /// <summary>
            /// doap:file-release
            /// </summary>
            public static readonly RDFResource FILE_RELEASE = new RDFResource(string.Concat(DOAP.BASE_URI, "file-release"));

            /// <summary>
            /// doap:wiki
            /// </summary>
            public static readonly RDFResource WIKI = new RDFResource(string.Concat(DOAP.BASE_URI, "wiki"));

            /// <summary>
            /// doap:bug-database
            /// </summary>
            public static readonly RDFResource BUG_DATABASE = new RDFResource(string.Concat(DOAP.BASE_URI, "bug-database"));

            /// <summary>
            /// doap:screenshots
            /// </summary>
            public static readonly RDFResource SCREENSHOTS = new RDFResource(string.Concat(DOAP.BASE_URI, "screenshots"));

            /// <summary>
            /// doap:maintainer
            /// </summary>
            public static readonly RDFResource MAINTAINER = new RDFResource(string.Concat(DOAP.BASE_URI, "maintainer"));

            /// <summary>
            /// doap:developer
            /// </summary>
            public static readonly RDFResource DEVELOPER = new RDFResource(string.Concat(DOAP.BASE_URI, "developer"));

            /// <summary>
            /// doap:documenter
            /// </summary>
            public static readonly RDFResource DOCUMENTER = new RDFResource(string.Concat(DOAP.BASE_URI, "documenter"));

            /// <summary>
            /// doap:translator
            /// </summary>
            public static readonly RDFResource TRANSLATOR = new RDFResource(string.Concat(DOAP.BASE_URI, "translator"));

            /// <summary>
            /// doap:tester
            /// </summary>
            public static readonly RDFResource TESTER = new RDFResource(string.Concat(DOAP.BASE_URI, "tester"));

            /// <summary>
            /// doap:helper
            /// </summary>
            public static readonly RDFResource HELPER = new RDFResource(string.Concat(DOAP.BASE_URI, "helper"));

            /// <summary>
            /// doap:programming-language
            /// </summary>
            public static readonly RDFResource PROGRAMMING_LANGUAGE = new RDFResource(string.Concat(DOAP.BASE_URI, "programming-language"));

            /// <summary>
            /// doap:os
            /// </summary>
            public static readonly RDFResource OS = new RDFResource(string.Concat(DOAP.BASE_URI, "os"));

            /// <summary>
            /// doap:implements
            /// </summary>
            public static readonly RDFResource IMPLEMENTS = new RDFResource(string.Concat(DOAP.BASE_URI, "implements"));

            /// <summary>
            /// doap:service-endpoint
            /// </summary>
            public static readonly RDFResource SERVICE_ENDPOINT = new RDFResource(string.Concat(DOAP.BASE_URI, "service-endpoint"));

            /// <summary>
            /// doap:language
            /// </summary>
            public static readonly RDFResource LANGUAGE = new RDFResource(string.Concat(DOAP.BASE_URI, "language"));

            /// <summary>
            /// doap:vendor
            /// </summary>
            public static readonly RDFResource VENDOR = new RDFResource(string.Concat(DOAP.BASE_URI, "vendor"));

            /// <summary>
            /// doap:platform
            /// </summary>
            public static readonly RDFResource PLATFORM = new RDFResource(string.Concat(DOAP.BASE_URI, "platform"));

            /// <summary>
            /// doap:audience
            /// </summary>
            public static readonly RDFResource AUDIENCE = new RDFResource(string.Concat(DOAP.BASE_URI, "audience"));

            /// <summary>
            /// doap:blog
            /// </summary>
            public static readonly RDFResource BLOG = new RDFResource(string.Concat(DOAP.BASE_URI, "blog"));
            #endregion

        }
        #endregion
    }
}