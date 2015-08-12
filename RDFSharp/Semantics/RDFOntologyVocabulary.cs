/*
   Copyright 2012-2015 Marco De Salvo

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics
{   
    
    /// <summary>
    /// RDFOntologyVocabulary is an helper for handy usage of supported RDF/RDFS/OWL classes and properties.
    /// </summary>
    internal static class RDFOntologyVocabulary {

        #region Classes
        /// <summary>
        /// Standard RDF/RDFS/OWL classes
        /// </summary>
        internal static class Classes {
            
            /// <summary>
            /// owl:Thing
            /// </summary>
            internal static readonly RDFOntologyClass THING = new RDFOntologyClass(RDFVocabulary.OWL.THING);

            /// <summary>
            /// owl:Nothing
            /// </summary>
            internal static readonly RDFOntologyClass NOTHING = new RDFOntologyClass(RDFVocabulary.OWL.NOTHING);

            /// <summary>
            /// rdfs:Literal
            /// </summary>
            internal static readonly RDFOntologyClass LITERAL = new RDFOntologyClass(RDFVocabulary.RDFS.LITERAL);

            /// <summary>
            /// rdf:XMLLiteral
            /// </summary>
            internal static readonly RDFOntologyClass XML_LITERAL = new RDFOntologyClass(RDFVocabulary.RDF.XML_LITERAL);

        }
        #endregion

        #region AnnotationProperties
        /// <summary>
        /// Standard RDFS/OWL annotation properties
        /// </summary>
        internal static class AnnotationProperties {
            
            /// <summary>
            /// owl:versionInfo
            /// </summary>
            internal static readonly RDFOntologyAnnotationProperty VERSION_INFO  = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.VERSION_INFO);

            /// <summary>
            /// rdfs:comment
            /// </summary>
            internal static readonly RDFOntologyAnnotationProperty COMMENT = new RDFOntologyAnnotationProperty(RDFVocabulary.RDFS.COMMENT);

            /// <summary>
            /// rdfs:label
            /// </summary>
            internal static readonly RDFOntologyAnnotationProperty LABEL = new RDFOntologyAnnotationProperty(RDFVocabulary.RDFS.LABEL);

            /// <summary>
            /// rdfs:seeAlso
            /// </summary>
            internal static readonly RDFOntologyAnnotationProperty SEE_ALSO = new RDFOntologyAnnotationProperty(RDFVocabulary.RDFS.SEE_ALSO);

            /// <summary>
            /// rdfs:isDefinedBy
            /// </summary>
            internal static readonly RDFOntologyAnnotationProperty IS_DEFINED_BY = new RDFOntologyAnnotationProperty(RDFVocabulary.RDFS.IS_DEFINED_BY);

            /// <summary>
            /// owl:imports
            /// </summary>
            internal static readonly RDFOntologyAnnotationProperty IMPORTS = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.IMPORTS);

            /// <summary>
            /// owl:BackwardCompatibleWith
            /// </summary>
            internal static readonly  RDFOntologyAnnotationProperty BACKWARD_COMPATIBLE_WITH = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.BACKWARD_COMPATIBLE_WITH);

            /// <summary>
            /// owl:incompatibleWith
            /// </summary>
            internal static readonly RDFOntologyAnnotationProperty INCOMPATIBLE_WITH = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.INCOMPATIBLE_WITH);

            /// <summary>
            /// owl:priorVersion
            /// </summary>
            internal static readonly RDFOntologyAnnotationProperty PRIOR_VERSION = new RDFOntologyAnnotationProperty(RDFVocabulary.OWL.PRIOR_VERSION);

        }
        #endregion

        #region ObjectProperties
        /// <summary>
        /// Standard RDFS/OWL object properties
        /// </summary>
        internal static class ObjectProperties {

            /// <summary>
            /// rdf:type
            /// </summary>
            internal static readonly RDFOntologyObjectProperty TYPE = new RDFOntologyObjectProperty(RDFVocabulary.RDF.TYPE);

            /// <summary>
            /// rdfs:subClassOf
            /// </summary>
            internal static readonly RDFOntologyObjectProperty SUB_CLASS_OF = new RDFOntologyObjectProperty(RDFVocabulary.RDFS.SUB_CLASS_OF);

            /// <summary>
            /// rdfs:subPropertyOf
            /// </summary>
            internal static readonly RDFOntologyObjectProperty SUB_PROPERTY_OF = new RDFOntologyObjectProperty(RDFVocabulary.RDFS.SUB_PROPERTY_OF);

            /// <summary>
            /// owl:equivalentClass
            /// </summary>
            internal static readonly RDFOntologyObjectProperty EQUIVALENT_CLASS = new RDFOntologyObjectProperty(RDFVocabulary.OWL.EQUIVALENT_CLASS);

            /// <summary>
            /// owl:equivalentProperty
            /// </summary>
            internal static readonly RDFOntologyObjectProperty EQUIVALENT_PROPERTY = new RDFOntologyObjectProperty(RDFVocabulary.OWL.EQUIVALENT_PROPERTY);

            /// <summary>
            /// owl:disjointWith
            /// </summary>
            internal static readonly RDFOntologyObjectProperty DISJOINT_WITH = new RDFOntologyObjectProperty(RDFVocabulary.OWL.DISJOINT_WITH);

            /// <summary>
            /// owl:inverseOf
            /// </summary>
            internal static readonly RDFOntologyObjectProperty INVERSE_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.INVERSE_OF);

            /// <summary>
            /// owl:oneOf
            /// </summary>
            internal static readonly RDFOntologyObjectProperty ONE_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.ONE_OF);

            /// <summary>
            /// owl:intersectionOf
            /// </summary>
            internal static readonly RDFOntologyObjectProperty INTERSECTION_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.INTERSECTION_OF);

            /// <summary>
            /// owl:unionOf
            /// </summary>
            internal static readonly RDFOntologyObjectProperty UNION_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.UNION_OF);

            /// <summary>
            /// owl:complementOf
            /// </summary>
            internal static readonly RDFOntologyObjectProperty COMPLEMENT_OF = new RDFOntologyObjectProperty(RDFVocabulary.OWL.COMPLEMENT_OF);

            /// <summary>
            /// owl:sameAs
            /// </summary>
            internal static readonly  RDFOntologyObjectProperty SAME_AS = new RDFOntologyObjectProperty(RDFVocabulary.OWL.SAME_AS);

            /// <summary>
            /// owl:differentFrom
            /// </summary>
            internal static readonly RDFOntologyObjectProperty DIFFERENT_FROM = new RDFOntologyObjectProperty(RDFVocabulary.OWL.DIFFERENT_FROM);

        }
        #endregion

        #region DatatypeProperties
        /// <summary>
        /// Standard RDFS/OWL datatype properties
        /// </summary>
        internal static class DatatypeProperties {

            /// <summary>
            /// owl:oneOf
            /// </summary>
            internal static readonly RDFOntologyDatatypeProperty ONE_OF = new RDFOntologyDatatypeProperty(RDFVocabulary.OWL.ONE_OF);

        }
        #endregion

    }

}