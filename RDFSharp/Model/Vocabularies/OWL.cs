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
            public static readonly RDFResource ONTOLOGY = new RDFResource(string.Concat(OWL.BASE_URI,"Ontology"));

            /// <summary>
            /// owl:imports
            /// </summary>
            public static readonly RDFResource IMPORTS = new RDFResource(string.Concat(OWL.BASE_URI,"imports"));

            /// <summary>
            /// owl:Class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource(string.Concat(OWL.BASE_URI,"Class"));

            /// <summary>
            /// owl:Individual
            /// </summary>
            public static readonly RDFResource INDIVIDUAL = new RDFResource(string.Concat(OWL.BASE_URI,"Individual"));

            /// <summary>
            /// owl:Thing
            /// </summary>
            public static readonly RDFResource THING = new RDFResource(string.Concat(OWL.BASE_URI,"Thing"));

            /// <summary>
            /// owl:Nothing
            /// </summary>
            public static readonly RDFResource NOTHING = new RDFResource(string.Concat(OWL.BASE_URI,"Nothing"));

            /// <summary>
            /// owl:NamedIndividual
            /// </summary>
            public static readonly RDFResource NAMED_INDIVIDUAL = new RDFResource(string.Concat(OWL.BASE_URI,"NamedIndividual"));

            /// <summary>
            /// owl:Restriction
            /// </summary>
            public static readonly RDFResource RESTRICTION = new RDFResource(string.Concat(OWL.BASE_URI,"Restriction"));

            /// <summary>
            /// owl:onProperty
            /// </summary>
            public static readonly RDFResource ON_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"onProperty"));

            /// <summary>
            /// owl:equivalentClass
            /// </summary>
            public static readonly RDFResource EQUIVALENT_CLASS = new RDFResource(string.Concat(OWL.BASE_URI,"equivalentClass"));

            /// <summary>
            /// owl:DeprecatedClass
            /// </summary>
            public static readonly RDFResource DEPRECATED_CLASS = new RDFResource(string.Concat(OWL.BASE_URI,"DeprecatedClass"));

            /// <summary>
            /// owl:equivalentProperty
            /// </summary>
            public static readonly RDFResource EQUIVALENT_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"equivalentProperty"));

            /// <summary>
            /// owl:DeprecatedProperty
            /// </summary>
            public static readonly RDFResource DEPRECATED_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"DeprecatedProperty"));

            /// <summary>
            /// owl:inverseOf
            /// </summary>
            public static readonly RDFResource INVERSE_OF = new RDFResource(string.Concat(OWL.BASE_URI,"inverseOf"));

            /// <summary>
            /// owl:DatatypeProperty
            /// </summary>
            public static readonly RDFResource DATATYPE_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"DatatypeProperty"));

            /// <summary>
            /// owl:ObjectProperty
            /// </summary>
            public static readonly RDFResource OBJECT_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"ObjectProperty"));

            /// <summary>
            /// owl:TransitiveProperty
            /// </summary>
            public static readonly RDFResource TRANSITIVE_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"TransitiveProperty"));

            /// <summary>
            /// owl:SymmetricProperty
            /// </summary>
            public static readonly RDFResource SYMMETRIC_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"SymmetricProperty"));

            /// <summary>
            /// owl:FunctionalProperty
            /// </summary>
            public static readonly RDFResource FUNCTIONAL_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"FunctionalProperty"));

            /// <summary>
            /// owl:InverseFunctionalProperty
            /// </summary>
            public static readonly RDFResource INVERSE_FUNCTIONAL_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"InverseFunctionalProperty"));

            /// <summary>
            /// owl:AnnotationProperty
            /// </summary>
            public static readonly RDFResource ANNOTATION_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"AnnotationProperty"));

            /// <summary>
            /// owl:OntologyProperty
            /// </summary>
            public static readonly RDFResource ONTOLOGY_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"OntologyProperty"));

            /// <summary>
            /// owl:allValuesFrom
            /// </summary>
            public static readonly RDFResource ALL_VALUES_FROM = new RDFResource(string.Concat(OWL.BASE_URI,"allValuesFrom"));

            /// <summary>
            /// owl:someValuesFrom
            /// </summary>
            public static readonly RDFResource SOME_VALUES_FROM = new RDFResource(string.Concat(OWL.BASE_URI,"someValuesFrom"));

            /// <summary>
            /// owl:hasValue
            /// </summary>
            public static readonly RDFResource HAS_VALUE = new RDFResource(string.Concat(OWL.BASE_URI,"hasValue"));

            /// <summary>
            /// owl:minCardinality
            /// </summary>
            public static readonly RDFResource MIN_CARDINALITY = new RDFResource(string.Concat(OWL.BASE_URI,"minCardinality"));

            /// <summary>
            /// owl:maxCardinality
            /// </summary>
            public static readonly RDFResource MAX_CARDINALITY = new RDFResource(string.Concat(OWL.BASE_URI,"maxCardinality"));

            /// <summary>
            /// owl:cardinality
            /// </summary>
            public static readonly RDFResource CARDINALITY = new RDFResource(string.Concat(OWL.BASE_URI,"cardinality"));

            /// <summary>
            /// owl:sameAs
            /// </summary>
            public static readonly RDFResource SAME_AS = new RDFResource(string.Concat(OWL.BASE_URI,"sameAs"));

            /// <summary>
            /// owl:differentFrom
            /// </summary>
            public static readonly RDFResource DIFFERENT_FROM = new RDFResource(string.Concat(OWL.BASE_URI,"differentFrom"));

            /// <summary>
            /// owl:members
            /// </summary>
            public static readonly RDFResource MEMBERS = new RDFResource(string.Concat(OWL.BASE_URI,"members"));

            /// <summary>
            /// owl:distinctMembers
            /// </summary>
            public static readonly RDFResource DISTINCT_MEMBERS = new RDFResource(string.Concat(OWL.BASE_URI,"distinctMembers"));

            /// <summary>
            /// owl:intersectionOf
            /// </summary>
            public static readonly RDFResource INTERSECTION_OF = new RDFResource(string.Concat(OWL.BASE_URI,"intersectionOf"));

            /// <summary>
            /// owl:unionOf
            /// </summary>
            public static readonly RDFResource UNION_OF = new RDFResource(string.Concat(OWL.BASE_URI,"unionOf"));

            /// <summary>
            /// owl:complementOf
            /// </summary>
            public static readonly RDFResource COMPLEMENT_OF = new RDFResource(string.Concat(OWL.BASE_URI,"complementOf"));

            /// <summary>
            /// owl:oneOf
            /// </summary>
            public static readonly RDFResource ONE_OF = new RDFResource(string.Concat(OWL.BASE_URI,"oneOf"));

            /// <summary>
            /// owl:DataRange
            /// </summary>
            public static readonly RDFResource DATA_RANGE = new RDFResource(string.Concat(OWL.BASE_URI,"DataRange"));

            /// <summary>
            /// owl:backwardCompatibleWith
            /// </summary>
            public static readonly RDFResource BACKWARD_COMPATIBLE_WITH = new RDFResource(string.Concat(OWL.BASE_URI,"backwardCompatibleWith"));

            /// <summary>
            /// owl:incompatibleWith
            /// </summary>
            public static readonly RDFResource INCOMPATIBLE_WITH = new RDFResource(string.Concat(OWL.BASE_URI,"incompatibleWith"));

            /// <summary>
            /// owl:disjointWith
            /// </summary>
            public static readonly RDFResource DISJOINT_WITH = new RDFResource(string.Concat(OWL.BASE_URI,"disjointWith"));

            /// <summary>
            /// owl:priorVersion
            /// </summary>
            public static readonly RDFResource PRIOR_VERSION = new RDFResource(string.Concat(OWL.BASE_URI,"priorVersion"));

            /// <summary>
            /// owl:versionInfo
            /// </summary>
            public static readonly RDFResource VERSION_INFO = new RDFResource(string.Concat(OWL.BASE_URI,"versionInfo"));

            /// <summary>
            /// owl:versionIRI
            /// </summary>
            public static readonly RDFResource VERSION_IRI = new RDFResource(string.Concat(OWL.BASE_URI,"versionIRI"));

            /// <summary>
            /// owl:disjointUnionOf [OWL2]
            /// </summary>
            public static readonly RDFResource DISJOINT_UNION_OF = new RDFResource(string.Concat(OWL.BASE_URI,"disjointUnionOf"));

            /// <summary>
            /// owl:AllDisjointClasses [OWL2]
            /// </summary>
            public static readonly RDFResource ALL_DISJOINT_CLASSES = new RDFResource(string.Concat(OWL.BASE_URI,"AllDisjointClasses"));

            /// <summary>
            /// owl:AllDifferent [OWL2]
            /// </summary>
            public static readonly RDFResource ALL_DIFFERENT = new RDFResource(string.Concat(OWL.BASE_URI,"AllDifferent"));

            /// <summary>
            /// owl:AllDisjointProperties [OWL2]
            /// </summary>
            public static readonly RDFResource ALL_DISJOINT_PROPERTIES = new RDFResource(string.Concat(OWL.BASE_URI,"AllDisjointProperties"));

            /// <summary>
            /// owl:AsymmetricProperty [OWL2]
            /// </summary>
            public static readonly RDFResource ASYMMETRIC_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"AsymmetricProperty"));

            /// <summary>
            /// owl:ReflexiveProperty [OWL2]
            /// </summary>
            public static readonly RDFResource REFLEXIVE_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"ReflexiveProperty"));

            /// <summary>
            /// owl:IrreflexiveProperty [OWL2]
            /// </summary>
            public static readonly RDFResource IRREFLEXIVE_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"IrreflexiveProperty"));

            /// <summary>
            /// owl:qualifiedCardinality [OWL2]
            /// </summary>
            public static readonly RDFResource QUALIFIED_CARDINALITY = new RDFResource(string.Concat(OWL.BASE_URI,"qualifiedCardinality"));

            /// <summary>
            /// owl:minQualifiedCardinality [OWL2]
            /// </summary>
            public static readonly RDFResource MIN_QUALIFIED_CARDINALITY = new RDFResource(string.Concat(OWL.BASE_URI,"minQualifiedCardinality"));

            /// <summary>
            /// owl:maxQualifiedCardinality [OWL2]
            /// </summary>
            public static readonly RDFResource MAX_QUALIFIED_CARDINALITY = new RDFResource(string.Concat(OWL.BASE_URI,"maxQualifiedCardinality"));

            /// <summary>
            /// owl:onClass [OWL2]
            /// </summary>
            public static readonly RDFResource ON_CLASS = new RDFResource(string.Concat(OWL.BASE_URI,"onClass"));

            /// <summary>
            /// owl:onDataRange [OWL2]
            /// </summary>
            public static readonly RDFResource ON_DATARANGE = new RDFResource(string.Concat(OWL.BASE_URI,"onDataRange"));

            /// <summary>
            /// owl:propertyDisjointWith [OWL2]
            /// </summary>
            public static readonly RDFResource PROPERTY_DISJOINT_WITH = new RDFResource(string.Concat(OWL.BASE_URI,"propertyDisjointWith"));

            /// <summary>
            /// owl:hasSelf [OWL2]
            /// </summary>
            public static readonly RDFResource HAS_SELF = new RDFResource(string.Concat(OWL.BASE_URI,"hasSelf"));

            /// <summary>
            /// owl:NegativePropertyAssertion [OWL2]
            /// </summary>
            public static readonly RDFResource NEGATIVE_PROPERTY_ASSERTION = new RDFResource(string.Concat(OWL.BASE_URI,"NegativePropertyAssertion"));

            /// <summary>
            /// owl:sourceIndividual [OWL2]
            /// </summary>
            public static readonly RDFResource SOURCE_INDIVIDUAL = new RDFResource(string.Concat(OWL.BASE_URI,"sourceIndividual"));

            /// <summary>
            /// owl:assertionProperty [OWL2]
            /// </summary>
            public static readonly RDFResource ASSERTION_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"assertionProperty"));

            /// <summary>
            /// owl:targetIndividual [OWL2]
            /// </summary>
            public static readonly RDFResource TARGET_INDIVIDUAL = new RDFResource(string.Concat(OWL.BASE_URI,"targetIndividual"));

            /// <summary>
            /// owl:targetValue [OWL2]
            /// </summary>
            public static readonly RDFResource TARGET_VALUE = new RDFResource(string.Concat(OWL.BASE_URI,"targetValue"));

            /// <summary>
            /// owl:hasKey [OWL2]
            /// </summary>
            public static readonly RDFResource HAS_KEY = new RDFResource(string.Concat(OWL.BASE_URI,"hasKey"));

            /// <summary>
            /// owl:propertyChainAxiom [OWL2]
            /// </summary>
            public static readonly RDFResource PROPERTY_CHAIN_AXIOM = new RDFResource(string.Concat(OWL.BASE_URI,"propertyChainAxiom"));

            /// <summary>
            /// owl:topProperty [OWL2]
            /// </summary>
            public static readonly RDFResource TOP_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"topProperty"));

            /// <summary>
            /// owl:topObjectProperty [OWL2]
            /// </summary>
            public static readonly RDFResource TOP_OBJECT_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"topObjectProperty"));

            /// <summary>
            /// owl:topDataProperty [OWL2]
            /// </summary>
            public static readonly RDFResource TOP_DATA_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"topDataProperty"));

            /// <summary>
            /// owl:bottomProperty [OWL2]
            /// </summary>
            public static readonly RDFResource BOTTOM_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"bottomProperty"));

            /// <summary>
            /// owl:bottomObjectProperty [OWL2]
            /// </summary>
            public static readonly RDFResource BOTTOM_OBJECT_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"bottomObjectProperty"));

            /// <summary>
            /// owl:bottomDataProperty [OWL2]
            /// </summary>
            public static readonly RDFResource BOTTOM_DATA_PROPERTY = new RDFResource(string.Concat(OWL.BASE_URI,"bottomDataProperty"));
            #endregion

        }
        #endregion
    }
}