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
        #region SKOS
        /// <summary>
        /// SKOS represents the W3C SKOS vocabulary.
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
            public static readonly RDFResource CONCEPT = new RDFResource(string.Concat(SKOS.BASE_URI,"Concept"));

            /// <summary>
            /// skos:ConceptScheme
            /// </summary>
            public static readonly RDFResource CONCEPT_SCHEME = new RDFResource(string.Concat(SKOS.BASE_URI,"ConceptScheme"));

            /// <summary>
            /// skos:inScheme
            /// </summary>
            public static readonly RDFResource IN_SCHEME = new RDFResource(string.Concat(SKOS.BASE_URI,"inScheme"));

            /// <summary>
            /// skos:hasTopConcept
            /// </summary>
            public static readonly RDFResource HAS_TOP_CONCEPT = new RDFResource(string.Concat(SKOS.BASE_URI,"hasTopConcept"));

            /// <summary>
            /// skos:topConceptOf
            /// </summary>
            public static readonly RDFResource TOP_CONCEPT_OF = new RDFResource(string.Concat(SKOS.BASE_URI,"topConceptOf"));

            /// <summary>
            /// skos:altLabel
            /// </summary>
            public static readonly RDFResource ALT_LABEL = new RDFResource(string.Concat(SKOS.BASE_URI,"altLabel"));

            /// <summary>
            /// skos:hiddenLabel
            /// </summary>
            public static readonly RDFResource HIDDEN_LABEL = new RDFResource(string.Concat(SKOS.BASE_URI,"hiddenLabel"));

            /// <summary>
            /// skos:prefLabel
            /// </summary>
            public static readonly RDFResource PREF_LABEL = new RDFResource(string.Concat(SKOS.BASE_URI,"prefLabel"));

            /// <summary>
            /// skos:notation
            /// </summary>
            public static readonly RDFResource NOTATION = new RDFResource(string.Concat(SKOS.BASE_URI,"notation"));

            /// <summary>
            /// skos:changeNote
            /// </summary>
            public static readonly RDFResource CHANGE_NOTE = new RDFResource(string.Concat(SKOS.BASE_URI,"changeNote"));

            /// <summary>
            /// skos:definition
            /// </summary>
            public static readonly RDFResource DEFINITION = new RDFResource(string.Concat(SKOS.BASE_URI,"definition"));

            /// <summary>
            /// skos:example
            /// </summary>
            public static readonly RDFResource EXAMPLE = new RDFResource(string.Concat(SKOS.BASE_URI,"example"));

            /// <summary>
            /// skos:editorialNote
            /// </summary>
            public static readonly RDFResource EDITORIAL_NOTE = new RDFResource(string.Concat(SKOS.BASE_URI,"editorialNote"));

            /// <summary>
            /// skos:historyNote
            /// </summary>
            public static readonly RDFResource HISTORY_NOTE = new RDFResource(string.Concat(SKOS.BASE_URI,"historyNote"));

            /// <summary>
            /// skos:note
            /// </summary>
            public static readonly RDFResource NOTE = new RDFResource(string.Concat(SKOS.BASE_URI,"note"));

            /// <summary>
            /// skos:scopeNote
            /// </summary>
            public static readonly RDFResource SCOPE_NOTE = new RDFResource(string.Concat(SKOS.BASE_URI,"scopeNote"));

            /// <summary>
            /// skos:broader
            /// </summary>
            public static readonly RDFResource BROADER = new RDFResource(string.Concat(SKOS.BASE_URI,"broader"));

            /// <summary>
            /// skos:broaderTransitive
            /// </summary>
            public static readonly RDFResource BROADER_TRANSITIVE = new RDFResource(string.Concat(SKOS.BASE_URI,"broaderTransitive"));

            /// <summary>
            /// skos:narrower
            /// </summary>
            public static readonly RDFResource NARROWER = new RDFResource(string.Concat(SKOS.BASE_URI,"narrower"));

            /// <summary>
            /// skos:narrowerTransitive
            /// </summary>
            public static readonly RDFResource NARROWER_TRANSITIVE = new RDFResource(string.Concat(SKOS.BASE_URI,"narrowerTransitive"));

            /// <summary>
            /// skos:related
            /// </summary>
            public static readonly RDFResource RELATED = new RDFResource(string.Concat(SKOS.BASE_URI,"related"));

            /// <summary>
            /// skos:semanticRelation
            /// </summary>
            public static readonly RDFResource SEMANTIC_RELATION = new RDFResource(string.Concat(SKOS.BASE_URI,"semanticRelation"));

            /// <summary>
            /// skos:subject
            /// </summary>
            public static readonly RDFResource SUBJECT = new RDFResource(string.Concat(SKOS.BASE_URI,"subject"));

            /// <summary>
            /// skos:Collection
            /// </summary>
            public static readonly RDFResource COLLECTION = new RDFResource(string.Concat(SKOS.BASE_URI,"Collection"));

            /// <summary>
            /// skos:OrderedCollection
            /// </summary>
            public static readonly RDFResource ORDERED_COLLECTION = new RDFResource(string.Concat(SKOS.BASE_URI,"OrderedCollection"));

            /// <summary>
            /// skos:member
            /// </summary>
            public static readonly RDFResource MEMBER = new RDFResource(string.Concat(SKOS.BASE_URI,"member"));

            /// <summary>
            /// skos:memberList
            /// </summary>
            public static readonly RDFResource MEMBER_LIST = new RDFResource(string.Concat(SKOS.BASE_URI,"memberList"));

            /// <summary>
            /// skos:broadMatch
            /// </summary>
            public static readonly RDFResource BROAD_MATCH = new RDFResource(string.Concat(SKOS.BASE_URI,"broadMatch"));

            /// <summary>
            /// skos:closeMatch
            /// </summary>
            public static readonly RDFResource CLOSE_MATCH = new RDFResource(string.Concat(SKOS.BASE_URI,"closeMatch"));

            /// <summary>
            /// skos:narrowMatch
            /// </summary>
            public static readonly RDFResource NARROW_MATCH = new RDFResource(string.Concat(SKOS.BASE_URI,"narrowMatch"));

            /// <summary>
            /// skos:relatedMatch
            /// </summary>
            public static readonly RDFResource RELATED_MATCH = new RDFResource(string.Concat(SKOS.BASE_URI,"relatedMatch"));

            /// <summary>
            /// skos:exactMatch
            /// </summary>
            public static readonly RDFResource EXACT_MATCH = new RDFResource(string.Concat(SKOS.BASE_URI,"exactMatch"));

            /// <summary>
            /// skos:mappingRelation
            /// </summary>
            public static readonly RDFResource MAPPING_RELATION = new RDFResource(string.Concat(SKOS.BASE_URI,"mappingRelation"));
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
                public static readonly RDFResource LABEL = new RDFResource(string.Concat(SKOSXL.BASE_URI, "Label"));

                /// <summary>
                /// skosxl:altLabel
                /// </summary>
                public static readonly RDFResource ALT_LABEL = new RDFResource(string.Concat(SKOSXL.BASE_URI, "altLabel"));

                /// <summary>
                /// skosxl:hiddenLabel
                /// </summary>
                public static readonly RDFResource HIDDEN_LABEL = new RDFResource(string.Concat(SKOSXL.BASE_URI, "hiddenLabel"));

                /// <summary>
                /// skosxl:labelRelation
                /// </summary>
                public static readonly RDFResource LABEL_RELATION = new RDFResource(string.Concat(SKOSXL.BASE_URI, "labelRelation"));

                /// <summary>
                /// skosxl:literalForm
                /// </summary>
                public static readonly RDFResource LITERAL_FORM = new RDFResource(string.Concat(SKOSXL.BASE_URI, "literalForm"));

                /// <summary>
                /// skosxl:prefLabel
                /// </summary>
                public static readonly RDFResource PREF_LABEL = new RDFResource(string.Concat(SKOSXL.BASE_URI, "prefLabel"));
                #endregion

            }
            #endregion

        }
        #endregion
    }
}