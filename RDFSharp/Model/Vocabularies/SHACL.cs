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
        #region SHACL
        /// <summary>
        /// SHACL represents the W3C SHACL Core vocabulary.
        /// </summary>
        public static class SHACL
        {

            #region Properties
            /// <summary>
            /// sh
            /// </summary>
            public static readonly string PREFIX = "sh";

            /// <summary>
            /// http://www.w3.org/ns/shacl#
            /// </summary>
            public static readonly string BASE_URI = "http://www.w3.org/ns/shacl#";

            /// <summary>
            /// http://www.w3.org/ns/shacl#
            /// </summary>
            public static readonly string DEREFERENCE_URI = "http://www.w3.org/ns/shacl#";

            /// <summary>
            /// sh:Shape
            /// </summary>
            public static readonly RDFResource SHAPE = new RDFResource(string.Concat(SHACL.BASE_URI,"Shape"));

            /// <summary>
            /// sh:NodeShape
            /// </summary>
            public static readonly RDFResource NODE_SHAPE = new RDFResource(string.Concat(SHACL.BASE_URI,"NodeShape"));

            /// <summary>
            /// sh:PropertyShape
            /// </summary>
            public static readonly RDFResource PROPERTY_SHAPE = new RDFResource(string.Concat(SHACL.BASE_URI,"PropertyShape"));

            /// <summary>
            /// sh:deactivated
            /// </summary>
            public static readonly RDFResource DEACTIVATED = new RDFResource(string.Concat(SHACL.BASE_URI,"deactivated"));

            /// <summary>
            /// sh:message
            /// </summary>
            public static readonly RDFResource MESSAGE = new RDFResource(string.Concat(SHACL.BASE_URI,"message"));

            /// <summary>
            /// sh:severity
            /// </summary>
            public static readonly RDFResource SEVERITY_PROPERTY = new RDFResource(string.Concat(SHACL.BASE_URI,"severity"));

            /// <summary>
            /// sh:targetClass
            /// </summary>
            public static readonly RDFResource TARGET_CLASS = new RDFResource(string.Concat(SHACL.BASE_URI,"targetClass"));

            /// <summary>
            /// sh:targetNode
            /// </summary>
            public static readonly RDFResource TARGET_NODE = new RDFResource(string.Concat(SHACL.BASE_URI,"targetNode"));

            /// <summary>
            /// sh:targetObjectsOf
            /// </summary>
            public static readonly RDFResource TARGET_OBJECTS_OF = new RDFResource(string.Concat(SHACL.BASE_URI,"targetObjectsOf"));

            /// <summary>
            /// sh:targetSubjectsOf
            /// </summary>
            public static readonly RDFResource TARGET_SUBJECTS_OF = new RDFResource(string.Concat(SHACL.BASE_URI,"targetSubjectsOf"));

            /// <summary>
            /// sh:BlankNode
            /// </summary>
            public static readonly RDFResource BLANK_NODE = new RDFResource(string.Concat(SHACL.BASE_URI,"BlankNode"));

            /// <summary>
            /// sh:BlankNodeOrIRI
            /// </summary>
            public static readonly RDFResource BLANK_NODE_OR_IRI = new RDFResource(string.Concat(SHACL.BASE_URI,"BlankNodeOrIRI"));

            /// <summary>
            /// sh:BlankNodeOrLiteral
            /// </summary>
            public static readonly RDFResource BLANK_NODE_OR_LITERAL = new RDFResource(string.Concat(SHACL.BASE_URI,"BlankNodeOrLiteral"));

            /// <summary>
            /// sh:IRI
            /// </summary>
            public static readonly RDFResource IRI = new RDFResource(string.Concat(SHACL.BASE_URI,"IRI"));

            /// <summary>
            /// sh:IRIOrLiteral
            /// </summary>
            public static readonly RDFResource IRI_OR_LITERAL = new RDFResource(string.Concat(SHACL.BASE_URI,"IRIOrLiteral"));

            /// <summary>
            /// sh:Literal
            /// </summary>
            public static readonly RDFResource LITERAL = new RDFResource(string.Concat(SHACL.BASE_URI,"Literal"));

            /// <summary>
            /// sh:ValidationReport
            /// </summary>
            public static readonly RDFResource VALIDATION_REPORT = new RDFResource(string.Concat(SHACL.BASE_URI,"ValidationReport"));

            /// <summary>
            /// sh:conforms
            /// </summary>
            public static readonly RDFResource CONFORMS = new RDFResource(string.Concat(SHACL.BASE_URI,"conforms"));

            /// <summary>
            /// sh:result
            /// </summary>
            public static readonly RDFResource RESULT = new RDFResource(string.Concat(SHACL.BASE_URI,"result"));

            /// <summary>
            /// sh:shapesGraphWellFormed
            /// </summary>
            public static readonly RDFResource SHAPES_GRAPH_WELL_FORMED = new RDFResource(string.Concat(SHACL.BASE_URI,"shapesGraphWellFormed"));

            /// <summary>
            /// sh:AbstractResult
            /// </summary>
            public static readonly RDFResource ABSTRACT_RESULT = new RDFResource(string.Concat(SHACL.BASE_URI,"AbstractResult"));

            /// <summary>
            /// sh:ValidationResult
            /// </summary>
            public static readonly RDFResource VALIDATION_RESULT = new RDFResource(string.Concat(SHACL.BASE_URI,"ValidationResult"));

            /// <summary>
            /// sh:Severity
            /// </summary>
            public static readonly RDFResource SEVERITY_CLASS = new RDFResource(string.Concat(SHACL.BASE_URI,"Severity"));

            /// <summary>
            /// sh:Info
            /// </summary>
            public static readonly RDFResource INFO = new RDFResource(string.Concat(SHACL.BASE_URI,"Info"));

            /// <summary>
            /// sh:Violation
            /// </summary>
            public static readonly RDFResource VIOLATION = new RDFResource(string.Concat(SHACL.BASE_URI,"Violation"));

            /// <summary>
            /// sh:Warning
            /// </summary>
            public static readonly RDFResource WARNING = new RDFResource(string.Concat(SHACL.BASE_URI,"Warning"));

            /// <summary>
            /// sh:detail
            /// </summary>
            public static readonly RDFResource DETAIL = new RDFResource(string.Concat(SHACL.BASE_URI,"detail"));

            /// <summary>
            /// sh:focusNode
            /// </summary>
            public static readonly RDFResource FOCUS_NODE = new RDFResource(string.Concat(SHACL.BASE_URI,"focusNode"));

            /// <summary>
            /// sh:resultMessage
            /// </summary>
            public static readonly RDFResource RESULT_MESSAGE = new RDFResource(string.Concat(SHACL.BASE_URI,"resultMessage"));

            /// <summary>
            /// sh:resultPath
            /// </summary>
            public static readonly RDFResource RESULT_PATH = new RDFResource(string.Concat(SHACL.BASE_URI,"resultPath"));

            /// <summary>
            /// sh:resultSeverity
            /// </summary>
            public static readonly RDFResource RESULT_SEVERITY = new RDFResource(string.Concat(SHACL.BASE_URI,"resultSeverity"));

            /// <summary>
            /// sh:sourceConstraint
            /// </summary>
            public static readonly RDFResource SOURCE_CONSTRAINT = new RDFResource(string.Concat(SHACL.BASE_URI,"sourceConstraint"));

            /// <summary>
            /// sh:sourceShape
            /// </summary>
            public static readonly RDFResource SOURCE_SHAPE = new RDFResource(string.Concat(SHACL.BASE_URI,"sourceShape"));

            /// <summary>
            /// sh:sourceConstraintComponent
            /// </summary>
            public static readonly RDFResource SOURCE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"sourceConstraintComponent"));

            /// <summary>
            /// sh:value
            /// </summary>
            public static readonly RDFResource VALUE = new RDFResource(string.Concat(SHACL.BASE_URI,"value"));

            /// <summary>
            /// sh:shapesGraph
            /// </summary>
            public static readonly RDFResource SHAPES_GRAPH = new RDFResource(string.Concat(SHACL.BASE_URI,"shapesGraph"));

            /// <summary>
            /// sh:suggestedShapesGraph
            /// </summary>
            public static readonly RDFResource SUGGESTED_SHAPES_GRAPH = new RDFResource(string.Concat(SHACL.BASE_URI,"suggestedShapesGraph"));

            /// <summary>
            /// sh:entailment
            /// </summary>
            public static readonly RDFResource ENTAILMENT = new RDFResource(string.Concat(SHACL.BASE_URI,"entailment"));

            /// <summary>
            /// sh:path
            /// </summary>
            public static readonly RDFResource PATH = new RDFResource(string.Concat(SHACL.BASE_URI,"path"));

            /// <summary>
            /// sh:inversePath
            /// </summary>
            public static readonly RDFResource INVERSE_PATH = new RDFResource(string.Concat(SHACL.BASE_URI,"inversePath"));

            /// <summary>
            /// sh:alternativePath
            /// </summary>
            public static readonly RDFResource ALTERNATIVE_PATH = new RDFResource(string.Concat(SHACL.BASE_URI,"alternativePath"));

            /// <summary>
            /// sh:zeroOrMorePath
            /// </summary>
            public static readonly RDFResource ZERO_OR_MORE_PATH = new RDFResource(string.Concat(SHACL.BASE_URI,"zeroOrMorePath"));

            /// <summary>
            /// sh:oneOrMorePath
            /// </summary>
            public static readonly RDFResource ONE_OR_MORE_PATH = new RDFResource(string.Concat(SHACL.BASE_URI,"oneOrMorePath"));

            /// <summary>
            /// sh:zeroOrOnePath
            /// </summary>
            public static readonly RDFResource ZERO_OR_ONE_PATH = new RDFResource(string.Concat(SHACL.BASE_URI,"zeroOrOnePath"));

            /// <summary>
            /// sh:defaultValue
            /// </summary>
            public static readonly RDFResource DEFAULT_VALUE = new RDFResource(string.Concat(SHACL.BASE_URI,"defaultValue"));

            /// <summary>
            /// sh:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(string.Concat(SHACL.BASE_URI,"description"));

            /// <summary>
            /// sh:group
            /// </summary>
            public static readonly RDFResource GROUP = new RDFResource(string.Concat(SHACL.BASE_URI,"group"));

            /// <summary>
            /// sh:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(string.Concat(SHACL.BASE_URI,"name"));

            /// <summary>
            /// sh:order
            /// </summary>
            public static readonly RDFResource ORDER = new RDFResource(string.Concat(SHACL.BASE_URI,"order"));

            /// <summary>
            /// sh:PropertyGroup
            /// </summary>
            public static readonly RDFResource PROPERTY_GROUP = new RDFResource(string.Concat(SHACL.BASE_URI,"PropertyGroup"));

            /// <summary>
            /// sh:Parameterizable
            /// </summary>
            public static readonly RDFResource PARAMETERIZABLE = new RDFResource(string.Concat(SHACL.BASE_URI,"Parameterizable"));

            /// <summary>
            /// sh:Parameter
            /// </summary>
            public static readonly RDFResource PARAMETER_CLASS = new RDFResource(string.Concat(SHACL.BASE_URI,"Parameter"));

            /// <summary>
            /// sh:parameter
            /// </summary>
            public static readonly RDFResource PARAMETER_PROPERTY = new RDFResource(string.Concat(SHACL.BASE_URI,"parameter"));

            /// <summary>
            /// sh:labelTemplate
            /// </summary>
            public static readonly RDFResource LABEL_TEMPLATE = new RDFResource(string.Concat(SHACL.BASE_URI,"labelTemplate"));

            /// <summary>
            /// sh:optional
            /// </summary>
            public static readonly RDFResource OPTIONAL = new RDFResource(string.Concat(SHACL.BASE_URI,"optional"));

            /// <summary>
            /// sh:ConstraintComponent
            /// </summary>
            public static readonly RDFResource CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"ConstraintComponent"));

            /// <summary>
            /// sh:validator
            /// </summary>
            public static readonly RDFResource VALIDATOR_PROPERTY = new RDFResource(string.Concat(SHACL.BASE_URI,"validator"));

            /// <summary>
            /// sh:nodeValidator
            /// </summary>
            public static readonly RDFResource NODE_VALIDATOR = new RDFResource(string.Concat(SHACL.BASE_URI,"nodeValidator"));

            /// <summary>
            /// sh:propertyValidator
            /// </summary>
            public static readonly RDFResource PROPERTY_VALIDATOR = new RDFResource(string.Concat(SHACL.BASE_URI,"propertyValidator"));

            /// <summary>
            /// sh:Validator
            /// </summary>
            public static readonly RDFResource VALIDATOR_CLASS = new RDFResource(string.Concat(SHACL.BASE_URI,"Validator"));

            /// <summary>
            /// sh:SPARQLAskValidator
            /// </summary>
            public static readonly RDFResource SPARQL_ASK_VALIDATOR = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLAskValidator"));

            /// <summary>
            /// sh:SPARQLSelectValidator
            /// </summary>
            public static readonly RDFResource SPARQL_SELECT_VALIDATOR = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLSelectValidator"));

            /// <summary>
            /// sh:AndConstraintComponent
            /// </summary>
            public static readonly RDFResource AND_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"AndConstraintComponent"));

            /// <summary>
            /// sh:AndConstraintComponent-and
            /// </summary>
            public static readonly RDFResource AND_CONSTRAINT_COMPONENT_AND = new RDFResource(string.Concat(SHACL.BASE_URI,"AndConstraintComponent-and"));

            /// <summary>
            /// sh:and
            /// </summary>
            public static readonly RDFResource AND = new RDFResource(string.Concat(SHACL.BASE_URI,"and"));

            /// <summary>
            /// sh:ClassConstraintComponent
            /// </summary>
            public static readonly RDFResource CLASS_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"ClassConstraintComponent"));

            /// <summary>
            /// sh:ClassConstraintComponent-class
            /// </summary>
            public static readonly RDFResource CLASS_CONSTRAINT_COMPONENT_CLASS = new RDFResource(string.Concat(SHACL.BASE_URI,"ClassConstraintComponent-class"));

            /// <summary>
            /// sh:class
            /// </summary>
            public static readonly RDFResource CLASS = new RDFResource(string.Concat(SHACL.BASE_URI,"class"));

            /// <summary>
            /// sh:ClosedConstraintComponent
            /// </summary>
            public static readonly RDFResource CLOSED_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"ClosedConstraintComponent"));

            /// <summary>
            /// sh:ClosedConstraintComponent-closed
            /// </summary>
            public static readonly RDFResource CLOSED_CONSTRAINT_COMPONENT_CLOSED = new RDFResource(string.Concat(SHACL.BASE_URI,"ClosedConstraintComponent-closed"));

            /// <summary>
            /// sh:ClosedConstraintComponent-ignoredProperties
            /// </summary>
            public static readonly RDFResource CLOSED_CONSTRAINT_COMPONENT_IGNORED_PROPERTIES = new RDFResource(string.Concat(SHACL.BASE_URI,"ClosedConstraintComponent-ignoredProperties"));

            /// <summary>
            /// sh:closed
            /// </summary>
            public static readonly RDFResource CLOSED = new RDFResource(string.Concat(SHACL.BASE_URI,"closed"));

            /// <summary>
            /// sh:ignoredProperties
            /// </summary>
            public static readonly RDFResource IGNORED_PROPERTIES = new RDFResource(string.Concat(SHACL.BASE_URI,"ignoredProperties"));

            /// <summary>
            /// sh:DatatypeConstraintComponent
            /// </summary>
            public static readonly RDFResource DATATYPE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"DatatypeConstraintComponent"));

            /// <summary>
            /// sh:DatatypeConstraintComponent-datatype
            /// </summary>
            public static readonly RDFResource DATATYPE_CONSTRAINT_COMPONENT_DATATYPE = new RDFResource(string.Concat(SHACL.BASE_URI,"DatatypeConstraintComponent-datatype"));

            /// <summary>
            /// sh:datatype
            /// </summary>
            public static readonly RDFResource DATATYPE = new RDFResource(string.Concat(SHACL.BASE_URI,"datatype"));

            /// <summary>
            /// sh:DisjointConstraintComponent
            /// </summary>
            public static readonly RDFResource DISJOINT_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"DisjointConstraintComponent"));

            /// <summary>
            /// sh:DisjointConstraintComponent-disjoint
            /// </summary>
            public static readonly RDFResource DISJOINT_CONSTRAINT_COMPONENT_DISJOINT = new RDFResource(string.Concat(SHACL.BASE_URI,"DisjointConstraintComponent-disjoint"));

            /// <summary>
            /// sh:disjoint
            /// </summary>
            public static readonly RDFResource DISJOINT = new RDFResource(string.Concat(SHACL.BASE_URI,"disjoint"));

            /// <summary>
            /// sh:EqualsConstraintComponent
            /// </summary>
            public static readonly RDFResource EQUALS_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"EqualsConstraintComponent"));

            /// <summary>
            /// sh:EqualsConstraintComponent-equals
            /// </summary>
            public static readonly RDFResource EQUALS_CONSTRAINT_COMPONENT_EQUALS = new RDFResource(string.Concat(SHACL.BASE_URI,"EqualsConstraintComponent-equals"));

            /// <summary>
            /// sh:equals
            /// </summary>
            public static readonly RDFResource EQUALS = new RDFResource(string.Concat(SHACL.BASE_URI,"equals"));

            /// <summary>
            /// sh:HasValueConstraintComponent
            /// </summary>
            public static readonly RDFResource HAS_VALUE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"HasValueConstraintComponent"));

            /// <summary>
            /// sh:HasValueConstraintComponent-hasValue
            /// </summary>
            public static readonly RDFResource HAS_VALUE_CONSTRAINT_COMPONENT_HAS_VALUE = new RDFResource(string.Concat(SHACL.BASE_URI,"HasValueConstraintComponent-hasValue"));

            /// <summary>
            /// sh:hasValue
            /// </summary>
            public static readonly RDFResource HAS_VALUE = new RDFResource(string.Concat(SHACL.BASE_URI,"hasValue"));

            /// <summary>
            /// sh:InConstraintComponent
            /// </summary>
            public static readonly RDFResource IN_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"InConstraintComponent"));

            /// <summary>
            /// sh:InConstraintComponent-in
            /// </summary>
            public static readonly RDFResource IN_CONSTRAINT_COMPONENT_IN = new RDFResource(string.Concat(SHACL.BASE_URI,"InConstraintComponent-in"));

            /// <summary>
            /// sh:in
            /// </summary>
            public static readonly RDFResource IN = new RDFResource(string.Concat(SHACL.BASE_URI,"in"));

            /// <summary>
            /// sh:LanguageInConstraintComponent
            /// </summary>
            public static readonly RDFResource LANGUAGE_IN_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"LanguageInConstraintComponent"));

            /// <summary>
            /// sh:LanguageInConstraintComponent-languageIn
            /// </summary>
            public static readonly RDFResource LANGUAGE_IN_CONSTRAINT_COMPONENT_LANGUAGE_IN = new RDFResource(string.Concat(SHACL.BASE_URI,"LanguageInConstraintComponent-languageIn"));

            /// <summary>
            /// sh:languageIn
            /// </summary>
            public static readonly RDFResource LANGUAGE_IN = new RDFResource(string.Concat(SHACL.BASE_URI,"languageIn"));

            /// <summary>
            /// sh:LessThanConstraintComponent
            /// </summary>
            public static readonly RDFResource LESS_THAN_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"LessThanConstraintComponent"));

            /// <summary>
            /// sh:LessThanConstraintComponent-lessThan
            /// </summary>
            public static readonly RDFResource LESS_THAN_CONSTRAINT_COMPONENT_LESS_THAN = new RDFResource(string.Concat(SHACL.BASE_URI,"LessThanConstraintComponent-lessThan"));

            /// <summary>
            /// sh:lessThan
            /// </summary>
            public static readonly RDFResource LESS_THAN = new RDFResource(string.Concat(SHACL.BASE_URI,"lessThan"));

            /// <summary>
            /// sh:LessThanOrEqualsConstraintComponent
            /// </summary>
            public static readonly RDFResource LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"LessThanOrEqualsConstraintComponent"));

            /// <summary>
            /// sh:LessThanOrEqualsConstraintComponent-lessThanOrEquals
            /// </summary>
            public static readonly RDFResource LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT_LESS_THAN_OR_EQUALS = new RDFResource(string.Concat(SHACL.BASE_URI,"LessThanOrEqualsConstraintComponent-lessThanOrEquals"));

            /// <summary>
            /// sh:lessThanOrEquals
            /// </summary>
            public static readonly RDFResource LESS_THAN_OR_EQUALS = new RDFResource(string.Concat(SHACL.BASE_URI,"lessThanOrEquals"));

            /// <summary>
            /// sh:MaxCountConstraintComponent
            /// </summary>
            public static readonly RDFResource MAX_COUNT_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"MaxCountConstraintComponent"));

            /// <summary>
            /// sh:MaxCountConstraintComponent-maxCount
            /// </summary>
            public static readonly RDFResource MAX_COUNT_CONSTRAINT_COMPONENT_MAX_COUNT = new RDFResource(string.Concat(SHACL.BASE_URI,"MaxCountConstraintComponent-maxCount"));

            /// <summary>
            /// sh:maxCount
            /// </summary>
            public static readonly RDFResource MAX_COUNT = new RDFResource(string.Concat(SHACL.BASE_URI,"maxCount"));

            /// <summary>
            /// sh:MaxExclusiveConstraintComponent
            /// </summary>
            public static readonly RDFResource MAX_EXCLUSIVE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"MaxExclusiveConstraintComponent"));

            /// <summary>
            /// sh:MaxExclusiveConstraintComponent-maxExclusive
            /// </summary>
            public static readonly RDFResource MAX_EXCLUSIVE_CONSTRAINT_COMPONENT_MAX_EXCLUSIVE = new RDFResource(string.Concat(SHACL.BASE_URI,"MaxExclusiveConstraintComponent-maxExclusive"));

            /// <summary>
            /// sh:maxExclusive
            /// </summary>
            public static readonly RDFResource MAX_EXCLUSIVE = new RDFResource(string.Concat(SHACL.BASE_URI,"maxExclusive"));

            /// <summary>
            /// sh:MaxInclusiveConstraintComponent
            /// </summary>
            public static readonly RDFResource MAX_INCLUSIVE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"MaxInclusiveConstraintComponent"));

            /// <summary>
            /// sh:MaxInclusiveConstraintComponent-maxInclusive
            /// </summary>
            public static readonly RDFResource MAX_INCLUSIVE_CONSTRAINT_COMPONENT_MAX_INCLUSIVE = new RDFResource(string.Concat(SHACL.BASE_URI,"MaxInclusiveConstraintComponent-maxInclusive"));

            /// <summary>
            /// sh:maxInclusive
            /// </summary>
            public static readonly RDFResource MAX_INCLUSIVE = new RDFResource(string.Concat(SHACL.BASE_URI,"maxInclusive"));

            /// <summary>
            /// sh:MaxLengthConstraintComponent
            /// </summary>
            public static readonly RDFResource MAX_LENGTH_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"MaxLengthConstraintComponent"));

            /// <summary>
            /// sh:MaxLengthConstraintComponent-maxLength
            /// </summary>
            public static readonly RDFResource MAX_LENGTH_CONSTRAINT_COMPONENT_MAX_LENGTH = new RDFResource(string.Concat(SHACL.BASE_URI,"MaxLengthConstraintComponent-maxLength"));

            /// <summary>
            /// sh:maxLength
            /// </summary>
            public static readonly RDFResource MAX_LENGTH = new RDFResource(string.Concat(SHACL.BASE_URI,"maxLength"));

            /// <summary>
            /// sh:MinCountConstraintComponent
            /// </summary>
            public static readonly RDFResource MIN_COUNT_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"MinCountConstraintComponent"));

            /// <summary>
            /// sh:MinCountConstraintComponent-minCount
            /// </summary>
            public static readonly RDFResource MIN_COUNT_CONSTRAINT_COMPONENT_MIN_COUNT = new RDFResource(string.Concat(SHACL.BASE_URI,"MinCountConstraintComponent-minCount"));

            /// <summary>
            /// sh:minCount
            /// </summary>
            public static readonly RDFResource MIN_COUNT = new RDFResource(string.Concat(SHACL.BASE_URI,"minCount"));

            /// <summary>
            /// sh:MinExclusiveConstraintComponent
            /// </summary>
            public static readonly RDFResource MIN_EXCLUSIVE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"MinExclusiveConstraintComponent"));

            /// <summary>
            /// sh:MinExclusiveConstraintComponent-minExclusive
            /// </summary>
            public static readonly RDFResource MIN_EXCLUSIVE_CONSTRAINT_COMPONENT_MIN_EXCLUSIVE = new RDFResource(string.Concat(SHACL.BASE_URI,"MinExclusiveConstraintComponent-minExclusive"));

            /// <summary>
            /// sh:minExclusive
            /// </summary>
            public static readonly RDFResource MIN_EXCLUSIVE = new RDFResource(string.Concat(SHACL.BASE_URI,"minExclusive"));

            /// <summary>
            /// sh:MinInclusiveConstraintComponent
            /// </summary>
            public static readonly RDFResource MIN_INCLUSIVE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"MinInclusiveConstraintComponent"));

            /// <summary>
            /// sh:MinInclusiveConstraintComponent-minInclusive
            /// </summary>
            public static readonly RDFResource MIN_INCLUSIVE_CONSTRAINT_COMPONENT_MIN_INCLUSIVE = new RDFResource(string.Concat(SHACL.BASE_URI,"MinInclusiveConstraintComponent-minInclusive"));

            /// <summary>
            /// sh:minInclusive
            /// </summary>
            public static readonly RDFResource MIN_INCLUSIVE = new RDFResource(string.Concat(SHACL.BASE_URI,"minInclusive"));

            /// <summary>
            /// sh:MinLengthConstraintComponent
            /// </summary>
            public static readonly RDFResource MIN_LENGTH_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"MinLengthConstraintComponent"));

            /// <summary>
            /// sh:MinLengthConstraintComponent-minLength
            /// </summary>
            public static readonly RDFResource MIN_LENGTH_CONSTRAINT_COMPONENT_MIN_LENGTH = new RDFResource(string.Concat(SHACL.BASE_URI,"MinLengthConstraintComponent-minLength"));

            /// <summary>
            /// sh:minLength
            /// </summary>
            public static readonly RDFResource MIN_LENGTH = new RDFResource(string.Concat(SHACL.BASE_URI,"minLength"));

            /// <summary>
            /// sh:NodeConstraintComponent
            /// </summary>
            public static readonly RDFResource NODE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"NodeConstraintComponent"));

            /// <summary>
            /// sh:NodeConstraintComponent-node
            /// </summary>
            public static readonly RDFResource NODE_CONSTRAINT_COMPONENT_NODE = new RDFResource(string.Concat(SHACL.BASE_URI,"NodeConstraintComponent-node"));

            /// <summary>
            /// sh:node
            /// </summary>
            public static readonly RDFResource NODE = new RDFResource(string.Concat(SHACL.BASE_URI,"node"));

            /// <summary>
            /// sh:NodeKindConstraintComponent
            /// </summary>
            public static readonly RDFResource NODE_KIND_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"NodeKindConstraintComponent"));

            /// <summary>
            /// sh:NodeKindConstraintComponent-nodeKind
            /// </summary>
            public static readonly RDFResource NODE_KIND_CONSTRAINT_COMPONENT_NODE_KIND = new RDFResource(string.Concat(SHACL.BASE_URI,"NodeKindConstraintComponent-nodeKind"));

            /// <summary>
            /// sh:nodeKind
            /// </summary>
            public static readonly RDFResource NODE_KIND = new RDFResource(string.Concat(SHACL.BASE_URI,"nodeKind"));

            /// <summary>
            /// sh:NodeKind
            /// </summary>
            public static readonly RDFResource NODE_KIND_CLASS = new RDFResource(string.Concat(SHACL.BASE_URI,"NodeKind"));

            /// <summary>
            /// sh:NotConstraintComponent
            /// </summary>
            public static readonly RDFResource NOT_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"NotConstraintComponent"));

            /// <summary>
            /// sh:NotConstraintComponent-not
            /// </summary>
            public static readonly RDFResource NOT_CONSTRAINT_COMPONENT_NOT = new RDFResource(string.Concat(SHACL.BASE_URI,"NotConstraintComponent-not"));

            /// <summary>
            /// sh:not
            /// </summary>
            public static readonly RDFResource NOT = new RDFResource(string.Concat(SHACL.BASE_URI,"not"));

            /// <summary>
            /// sh:OrConstraintComponent
            /// </summary>
            public static readonly RDFResource OR_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"OrConstraintComponent"));

            /// <summary>
            /// sh:OrConstraintComponent-or
            /// </summary>
            public static readonly RDFResource OR_CONSTRAINT_COMPONENT_OR = new RDFResource(string.Concat(SHACL.BASE_URI,"OrConstraintComponent-or"));

            /// <summary>
            /// sh:or
            /// </summary>
            public static readonly RDFResource OR = new RDFResource(string.Concat(SHACL.BASE_URI,"or"));

            /// <summary>
            /// sh:PatternConstraintComponent
            /// </summary>
            public static readonly RDFResource PATTERN_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"PatternConstraintComponent"));

            /// <summary>
            /// sh:PatternConstraintComponent-pattern
            /// </summary>
            public static readonly RDFResource PATTERN_CONSTRAINT_COMPONENT_PATTERN = new RDFResource(string.Concat(SHACL.BASE_URI,"PatternConstraintComponent-pattern"));

            /// <summary>
            /// sh:PatternConstraintComponent-flags
            /// </summary>
            public static readonly RDFResource PATTERN_CONSTRAINT_COMPONENT_FLAGS = new RDFResource(string.Concat(SHACL.BASE_URI,"PatternConstraintComponent-flags"));

            /// <summary>
            /// sh:flags
            /// </summary>
            public static readonly RDFResource FLAGS = new RDFResource(string.Concat(SHACL.BASE_URI,"flags"));

            /// <summary>
            /// sh:pattern
            /// </summary>
            public static readonly RDFResource PATTERN = new RDFResource(string.Concat(SHACL.BASE_URI,"pattern"));

            /// <summary>
            /// sh:PropertyConstraintComponent
            /// </summary>
            public static readonly RDFResource PROPERTY_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"PropertyConstraintComponent"));

            /// <summary>
            /// sh:PropertyConstraintComponent-property
            /// </summary>
            public static readonly RDFResource PROPERTY_CONSTRAINT_COMPONENT_PROPERTY = new RDFResource(string.Concat(SHACL.BASE_URI,"PropertyConstraintComponent-property"));

            /// <summary>
            /// sh:property
            /// </summary>
            public static readonly RDFResource PROPERTY = new RDFResource(string.Concat(SHACL.BASE_URI,"property"));

            /// <summary>
            /// sh:QualifiedMaxCountConstraintComponent
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"QualifiedMaxCountConstraintComponent"));

            /// <summary>
            /// sh:QualifiedMaxCountConstraintComponent-qualifiedMaxCount
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_MAX_COUNT = new RDFResource(string.Concat(SHACL.BASE_URI,"QualifiedMaxCountConstraintComponent-qualifiedMaxCount"));

            /// <summary>
            /// sh:QualifiedMaxCountConstraintComponent-qualifiedValueShape
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPE = new RDFResource(string.Concat(SHACL.BASE_URI,"QualifiedMaxCountConstraintComponent-qualifiedValueShape"));

            /// <summary>
            /// sh:QualifiedMaxCountConstraintComponent-qualifiedValueShapesDisjoint
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPES_DISJOINT = new RDFResource(string.Concat(SHACL.BASE_URI,"QualifiedMaxCountConstraintComponent-qualifiedValueShapesDisjoint"));

            /// <summary>
            /// sh:QualifiedMinCountConstraintComponent
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"QualifiedMinCountConstraintComponent"));

            /// <summary>
            /// sh:QualifiedMinCountConstraintComponent-qualifiedMinCount
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_MIN_COUNT = new RDFResource(string.Concat(SHACL.BASE_URI,"QualifiedMinCountConstraintComponent-qualifiedMinCount"));

            /// <summary>
            /// sh:QualifiedMinCountConstraintComponent-qualifiedValueShape
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPE = new RDFResource(string.Concat(SHACL.BASE_URI,"QualifiedMinCountConstraintComponent-qualifiedValueShape"));

            /// <summary>
            /// sh:QualifiedMinCountConstraintComponent-qualifiedValueShapesDisjoint
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPES_DISJOINT = new RDFResource(string.Concat(SHACL.BASE_URI,"QualifiedMinCountConstraintComponent-qualifiedValueShapesDisjoint"));

            /// <summary>
            /// sh:qualifiedMaxCount
            /// </summary>
            public static readonly RDFResource QUALIFIED_MAX_COUNT = new RDFResource(string.Concat(SHACL.BASE_URI,"qualifiedMaxCount"));

            /// <summary>
            /// sh:qualifiedMinCount
            /// </summary>
            public static readonly RDFResource QUALIFIED_MIN_COUNT = new RDFResource(string.Concat(SHACL.BASE_URI,"qualifiedMinCount"));

            /// <summary>
            /// sh:qualifiedValueShape
            /// </summary>
            public static readonly RDFResource QUALIFIED_VALUE_SHAPE = new RDFResource(string.Concat(SHACL.BASE_URI,"qualifiedValueShape"));

            /// <summary>
            /// sh:UniqueLangConstraintComponent
            /// </summary>
            public static readonly RDFResource UNIQUE_LANG_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"UniqueLangConstraintComponent"));

            /// <summary>
            /// sh:UniqueLangConstraintComponent-uniqueLang
            /// </summary>
            public static readonly RDFResource UNIQUE_LANG_CONSTRAINT_COMPONENT_UNIQUE_LANG = new RDFResource(string.Concat(SHACL.BASE_URI,"UniqueLangConstraintComponent-uniqueLang"));

            /// <summary>
            /// sh:uniqueLang
            /// </summary>
            public static readonly RDFResource UNIQUE_LANG = new RDFResource(string.Concat(SHACL.BASE_URI,"uniqueLang"));

            /// <summary>
            /// sh:XoneConstraintComponent
            /// </summary>
            public static readonly RDFResource XONE_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"XoneConstraintComponent"));

            /// <summary>
            /// sh:XoneConstraintComponent-xone
            /// </summary>
            public static readonly RDFResource XONE_CONSTRAINT_COMPONENT_XONE = new RDFResource(string.Concat(SHACL.BASE_URI,"XoneConstraintComponent-xone"));

            /// <summary>
            /// sh:xone
            /// </summary>
            public static readonly RDFResource XONE = new RDFResource(string.Concat(SHACL.BASE_URI,"xone"));

            /// <summary>
            /// sh:SPARQLExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_EXECUTABLE = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLExecutable"));

            /// <summary>
            /// sh:SPARQLAskExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_ASK_EXECUTABLE = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLAskExecutable"));

            /// <summary>
            /// sh:ask
            /// </summary>
            public static readonly RDFResource ASK = new RDFResource(string.Concat(SHACL.BASE_URI,"ask"));

            /// <summary>
            /// sh:SPARQLConstructExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_CONSTRUCT_EXECUTABLE = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLConstructExecutable"));

            /// <summary>
            /// sh:construct
            /// </summary>
            public static readonly RDFResource CONSTRUCT = new RDFResource(string.Concat(SHACL.BASE_URI,"construct"));

            /// <summary>
            /// sh:SPARQLSelectExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_SELECT_EXECUTABLE = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLSelectExecutable"));

            /// <summary>
            /// sh:select
            /// </summary>
            public static readonly RDFResource SELECT = new RDFResource(string.Concat(SHACL.BASE_URI,"select"));

            /// <summary>
            /// sh:SPARQLUpdateExecutable
            /// </summary>
            public static readonly RDFResource SPARQL_UPDATE_EXECUTABLE = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLUpdateExecutable"));

            /// <summary>
            /// sh:update
            /// </summary>
            public static readonly RDFResource UPDATE = new RDFResource(string.Concat(SHACL.BASE_URI,"update"));

            /// <summary>
            /// sh:prefixes
            /// </summary>
            public static readonly RDFResource PREFIXES = new RDFResource(string.Concat(SHACL.BASE_URI,"prefixes"));

            /// <summary>
            /// sh:PrefixDeclaration
            /// </summary>
            public static readonly RDFResource PREFIX_DECLARATION = new RDFResource(string.Concat(SHACL.BASE_URI,"PrefixDeclaration"));

            /// <summary>
            /// sh:declare
            /// </summary>
            public static readonly RDFResource DECLARE = new RDFResource(string.Concat(SHACL.BASE_URI,"declare"));

            /// <summary>
            /// sh:prefix
            /// </summary>
            public static readonly RDFResource PREFIX_PROPERTY = new RDFResource(string.Concat(SHACL.BASE_URI,"prefix"));

            /// <summary>
            /// sh:namespace
            /// </summary>
            public static readonly RDFResource NAMESPACE = new RDFResource(string.Concat(SHACL.BASE_URI,"namespace"));

            /// <summary>
            /// sh:SPARQLConstraintComponent
            /// </summary>
            public static readonly RDFResource SPARQL_CONSTRAINT_COMPONENT = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLConstraintComponent"));

            /// <summary>
            /// sh:SPARQLConstraintComponent-sparql
            /// </summary>
            public static readonly RDFResource SPARQL_CONSTRAINT_COMPONENT_SPARQL = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLConstraintComponent-sparql"));

            /// <summary>
            /// sh:sparql
            /// </summary>
            public static readonly RDFResource SPARQL = new RDFResource(string.Concat(SHACL.BASE_URI,"sparql"));

            /// <summary>
            /// sh:SPARQLConstraint
            /// </summary>
            public static readonly RDFResource SPARQL_CONSTRAINT = new RDFResource(string.Concat(SHACL.BASE_URI,"SPARQLConstraint"));
            #endregion

        }
        #endregion
    }
}