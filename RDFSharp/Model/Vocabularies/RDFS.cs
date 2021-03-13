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
        #region RDFS
        /// <summary>
        /// RDFS represents the RDFS vocabulary.
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
            public static readonly RDFResource RESOURCE = new RDFResource(string.Concat(RDFS.BASE_URI,"Resource"));

            /// <summary>
            /// rdfs:Class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource(string.Concat(RDFS.BASE_URI,"Class"));

            /// <summary>
            /// rdfs:Literal
            /// </summary>
            public static readonly RDFResource LITERAL = new RDFResource(string.Concat(RDFS.BASE_URI,"Literal"));

            /// <summary>
            /// rdfs:Container
            /// </summary>
            public static readonly RDFResource CONTAINER = new RDFResource(string.Concat(RDFS.BASE_URI,"Container"));

            /// <summary>
            /// rdfs:Datatype
            /// </summary>
            public static readonly RDFResource DATATYPE = new RDFResource(string.Concat(RDFS.BASE_URI,"Datatype"));

            /// <summary>
            /// rdfs:ContainerMembershipProperty
            /// </summary>
            public static readonly RDFResource CONTAINER_MEMBERSHIP_PROPERTY = new RDFResource(string.Concat(RDFS.BASE_URI,"ContainerMembershipProperty"));

            /// <summary>
            /// rdfs:range
            /// </summary>
            public static readonly RDFResource RANGE = new RDFResource(string.Concat(RDFS.BASE_URI,"range"));

            /// <summary>
            /// rdfs:domain
            /// </summary>
            public static readonly RDFResource DOMAIN = new RDFResource(string.Concat(RDFS.BASE_URI,"domain"));

            /// <summary>
            /// rdfs:subClassOf
            /// </summary>
            public static readonly RDFResource SUB_CLASS_OF = new RDFResource(string.Concat(RDFS.BASE_URI,"subClassOf"));

            /// <summary>
            /// rdfs:subPropertyOf
            /// </summary>
            public static readonly RDFResource SUB_PROPERTY_OF = new RDFResource(string.Concat(RDFS.BASE_URI,"subPropertyOf"));

            /// <summary>
            /// rdfs:label
            /// </summary>
            public static readonly RDFResource LABEL = new RDFResource(string.Concat(RDFS.BASE_URI,"label"));

            /// <summary>
            /// rdfs:comment
            /// </summary>
            public static readonly RDFResource COMMENT = new RDFResource(string.Concat(RDFS.BASE_URI,"comment"));

            /// <summary>
            /// rdfs:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource(string.Concat(RDFS.BASE_URI,"member"));

            /// <summary>
            /// rdfs:seeAlso
            /// </summary>
            public static readonly RDFResource SEE_ALSO = new RDFResource(string.Concat(RDFS.BASE_URI,"seeAlso"));

            /// <summary>
            /// rdfs:isDefinedBy
            /// </summary>
            public static readonly RDFResource IS_DEFINED_BY = new RDFResource(string.Concat(RDFS.BASE_URI,"isDefinedBy"));
            #endregion

        }
        #endregion
    }
}