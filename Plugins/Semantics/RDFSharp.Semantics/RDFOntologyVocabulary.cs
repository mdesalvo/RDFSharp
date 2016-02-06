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
using RDFSharp.Model;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyVocabulary is an helper for handy usage of supported RDF/RDFS/OWL classes and properties.
    /// </summary>
    public static class RDFOntologyVocabulary {

        #region Classes
        /// <summary>
        /// Standard RDF/RDFS/OWL classes
        /// </summary>
        public static class Classes {

            /// <summary>
            /// owl:Thing
            /// </summary>
            public static readonly RDFOntologyClass THING = new RDFOntologyClass(RDFVocabulary.OWL.THING);

            /// <summary>
            /// owl:Nothing
            /// </summary>
            public static readonly RDFOntologyClass NOTHING = new RDFOntologyClass(RDFVocabulary.OWL.NOTHING);

            /// <summary>
            /// rdfs:Literal
            /// </summary>
            public static readonly RDFOntologyClass LITERAL = new RDFOntologyClass(RDFVocabulary.RDFS.LITERAL);

            /// <summary>
            /// rdf:XMLLiteral
            /// </summary>
            public static readonly RDFOntologyClass XML_LITERAL = new RDFOntologyClass(RDFVocabulary.RDF.XML_LITERAL);

        }
        #endregion

        #region AnnotationProperties
        /// <summary>
        /// Standard RDFS/OWL annotation properties
        /// </summary>
        public static class AnnotationProperties {

            /// <summary>
            /// owl:versionInfo
            /// </summary>
            public static readonly RDFOntologyAnnotationProperty VERSION_INFO  = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.VERSION_INFO);

            /// <summary>
            /// rdfs:comment
            /// </summary>
            public static readonly RDFOntologyAnnotationProperty COMMENT = new RDFOntologyAnnotationProperty(RDFVocabulary.RDFS.COMMENT);

            /// <summary>
            /// rdfs:label
            /// </summary>
            public static readonly RDFOntologyAnnotationProperty LABEL = new RDFOntologyAnnotationProperty(RDFVocabulary.RDFS.LABEL);

            /// <summary>
            /// rdfs:seeAlso
            /// </summary>
            public static readonly RDFOntologyAnnotationProperty SEE_ALSO = new RDFOntologyAnnotationProperty(RDFVocabulary.RDFS.SEE_ALSO);

            /// <summary>
            /// rdfs:isDefinedBy
            /// </summary>
            public static readonly RDFOntologyAnnotationProperty IS_DEFINED_BY = new RDFOntologyAnnotationProperty(RDFVocabulary.RDFS.IS_DEFINED_BY);

            /// <summary>
            /// owl:imports
            /// </summary>
            public static readonly RDFOntologyAnnotationProperty IMPORTS = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.IMPORTS);

            /// <summary>
            /// owl:BackwardCompatibleWith
            /// </summary>
            public static readonly  RDFOntologyAnnotationProperty BACKWARD_COMPATIBLE_WITH = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH);

            /// <summary>
            /// owl:incompatibleWith
            /// </summary>
            public static readonly RDFOntologyAnnotationProperty INCOMPATIBLE_WITH = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.INCOMPATIBLE_WITH);

            /// <summary>
            /// owl:priorVersion
            /// </summary>
            public static readonly RDFOntologyAnnotationProperty PRIOR_VERSION = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.PRIOR_VERSION);

        }
        #endregion

        #region ObjectProperties
        /// <summary>
        /// Standard RDFS/OWL object properties
        /// </summary>
        public static class ObjectProperties {

            /// <summary>
            /// rdf:type
            /// </summary>
            public static readonly RDFOntologyObjectProperty TYPE = new RDFOntologyObjectProperty(RDFVocabulary.RDF.TYPE);

            /// <summary>
            /// rdfs:subClassOf
            /// </summary>
            public static readonly RDFOntologyObjectProperty SUB_CLASS_OF = new RDFOntologyObjectProperty(RDFVocabulary.RDFS.SUB_CLASS_OF);

            /// <summary>
            /// rdfs:subPropertyOf
            /// </summary>
            public static readonly RDFOntologyObjectProperty SUB_PROPERTY_OF = new RDFOntologyObjectProperty(RDFVocabulary.RDFS.SUB_PROPERTY_OF);

            /// <summary>
            /// owl:equivalentClass
            /// </summary>
            public static readonly RDFOntologyObjectProperty EQUIVALENT_CLASS = new RDFOntologyObjectProperty(RDFVocabulary.OWL.EQUIVALENT_CLASS);

            /// <summary>
            /// owl:equivalentProperty
            /// </summary>
            public static readonly RDFOntologyObjectProperty EQUIVALENT_PROPERTY = new RDFOntologyObjectProperty(RDFVocabulary.OWL.EQUIVALENT_PROPERTY);

            /// <summary>
            /// owl:disjointWith
            /// </summary>
            public static readonly RDFOntologyObjectProperty DISJOINT_WITH = new RDFOntologyObjectProperty(RDFVocabulary.OWL.DISJOINT_WITH);

            /// <summary>
            /// owl:inverseOf
            /// </summary>
            public static readonly RDFOntologyObjectProperty INVERSE_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.INVERSE_OF);

            /// <summary>
            /// owl:oneOf
            /// </summary>
            public static readonly RDFOntologyObjectProperty ONE_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.ONE_OF);

            /// <summary>
            /// owl:intersectionOf
            /// </summary>
            public static readonly RDFOntologyObjectProperty INTERSECTION_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.INTERSECTION_OF);

            /// <summary>
            /// owl:unionOf
            /// </summary>
            public static readonly RDFOntologyObjectProperty UNION_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.UNION_OF);

            /// <summary>
            /// owl:complementOf
            /// </summary>
            public static readonly RDFOntologyObjectProperty COMPLEMENT_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.COMPLEMENT_OF);

            /// <summary>
            /// owl:sameAs
            /// </summary>
            public static readonly  RDFOntologyObjectProperty SAME_AS = new RDFOntologyObjectProperty(RDFVocabulary.OWL.SAME_AS);

            /// <summary>
            /// owl:differentFrom
            /// </summary>
            public static readonly RDFOntologyObjectProperty DIFFERENT_FROM = new RDFOntologyObjectProperty(RDFVocabulary.OWL.DIFFERENT_FROM);

        }
        #endregion

        #region DatatypeProperties
        /// <summary>
        /// Standard RDFS/OWL datatype properties
        /// </summary>
        public static class DatatypeProperties {

            /// <summary>
            /// owl:oneOf
            /// </summary>
            public static readonly RDFOntologyDatatypeProperty ONE_OF = new RDFOntologyDatatypeProperty(RDFVocabulary.OWL.ONE_OF);

        }
        #endregion

    }

}