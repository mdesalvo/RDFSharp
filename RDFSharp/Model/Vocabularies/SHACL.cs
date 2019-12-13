/*
   Copyright 2012-2019 Marco De Salvo

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
        #region SHACL
        /// <summary>
        /// SHACL represents the W3C SHACL vocabulary.
        /// </summary>
        public static class SHACL
        {

            #region Properties
            /// <summary>
            /// sh
            /// </summary>
            public static readonly String PREFIX = "sh";

            /// <summary>
            /// http://www.w3.org/ns/shacl#
            /// </summary>
            public static readonly String BASE_URI = "http://www.w3.org/ns/shacl#";

            /// <summary>
            /// sh:Shape
            /// </summary>
            public static readonly RDFResource SHAPE = new RDFResource(SHACL.BASE_URI + "Shape");

            /// <summary>
            /// sh:NodeShape
            /// </summary>
            public static readonly RDFResource NODE_SHAPE = new RDFResource(SHACL.BASE_URI + "NodeShape");

            /// <summary>
            /// sh:PropertyShape
            /// </summary>
            public static readonly RDFResource PROPERTY_SHAPE = new RDFResource(SHACL.BASE_URI + "PropertyShape");

            /// <summary>
            /// sh:deactivated
            /// </summary>
            public static readonly RDFResource DEACTIVATED = new RDFResource(SHACL.BASE_URI + "deactivated");

            /// <summary>
            /// sh:message
            /// </summary>
            public static readonly RDFResource MESSAGE = new RDFResource(SHACL.BASE_URI + "message");

            /// <summary>
            /// sh:severity
            /// </summary>
            public static readonly RDFResource SEVERITY_PROPERTY = new RDFResource(SHACL.BASE_URI + "severity");

            /// <summary>
            /// sh:targetClass
            /// </summary>
            public static readonly RDFResource TARGET_CLASS = new RDFResource(SHACL.BASE_URI + "targetClass");

            /// <summary>
            /// sh:targetNode
            /// </summary>
            public static readonly RDFResource TARGET_NODE = new RDFResource(SHACL.BASE_URI + "targetNode");

            /// <summary>
            /// sh:targetObjectsOf
            /// </summary>
            public static readonly RDFResource TARGET_OBJECTS_OF = new RDFResource(SHACL.BASE_URI + "targetObjectsOf");

            /// <summary>
            /// sh:targetSubjectsOf
            /// </summary>
            public static readonly RDFResource TARGET_SUBJECTS_OF = new RDFResource(SHACL.BASE_URI + "targetSubjectsOf");

            /// <summary>
            /// sh:NodeKind
            /// </summary>
            public static readonly RDFResource NODE_KIND = new RDFResource(SHACL.BASE_URI + "NodeKind");

            /// <summary>
            /// sh:BlankNode
            /// </summary>
            public static readonly RDFResource BLANK_NODE = new RDFResource(SHACL.BASE_URI + "BlankNode");

            /// <summary>
            /// sh:BlankNodeOrIRI
            /// </summary>
            public static readonly RDFResource BLANK_NODE_OR_IRI = new RDFResource(SHACL.BASE_URI + "BlankNodeOrIRI");

            /// <summary>
            /// sh:BlankNodeOrLiteral
            /// </summary>
            public static readonly RDFResource BLANK_NODE_OR_LITERAL = new RDFResource(SHACL.BASE_URI + "BlankNodeOrLiteral");

            /// <summary>
            /// sh:IRI
            /// </summary>
            public static readonly RDFResource IRI = new RDFResource(SHACL.BASE_URI + "IRI");

            /// <summary>
            /// sh:IRIOrLiteral
            /// </summary>
            public static readonly RDFResource IRI_OR_LITERAL = new RDFResource(SHACL.BASE_URI + "IRIOrLiteral");

            /// <summary>
            /// sh:Literal
            /// </summary>
            public static readonly RDFResource LITERAL = new RDFResource(SHACL.BASE_URI + "Literal");

            /// <summary>
            /// sh:ValidationReport
            /// </summary>
            public static readonly RDFResource VALIDATION_REPORT = new RDFResource(SHACL.BASE_URI + "ValidationReport");

            /// <summary>
            /// sh:conforms
            /// </summary>
            public static readonly RDFResource CONFORMS = new RDFResource(SHACL.BASE_URI + "conforms");

            /// <summary>
            /// sh:result
            /// </summary>
            public static readonly RDFResource RESULT = new RDFResource(SHACL.BASE_URI + "result");

            /// <summary>
            /// sh:shapesGraphWellFormed
            /// </summary>
            public static readonly RDFResource SHAPES_GRAPH_WELL_FORMED = new RDFResource(SHACL.BASE_URI + "shapesGraphWellFormed");

            /// <summary>
            /// sh:AbstractResult
            /// </summary>
            public static readonly RDFResource ABSTRACT_RESULT = new RDFResource(SHACL.BASE_URI + "AbstractResult");

            /// <summary>
            /// sh:ValidationResult
            /// </summary>
            public static readonly RDFResource VALIDATION_RESULT = new RDFResource(SHACL.BASE_URI + "ValidationResult");

            /// <summary>
            /// sh:Severity
            /// </summary>
            public static readonly RDFResource SEVERITY_CLASS = new RDFResource(SHACL.BASE_URI + "Severity");

            /// <summary>
            /// sh:Info
            /// </summary>
            public static readonly RDFResource INFO = new RDFResource(SHACL.BASE_URI + "Info");

            /// <summary>
            /// sh:Violation
            /// </summary>
            public static readonly RDFResource VIOLATION = new RDFResource(SHACL.BASE_URI + "Violation");

            /// <summary>
            /// sh:Warning
            /// </summary>
            public static readonly RDFResource WARNING = new RDFResource(SHACL.BASE_URI + "Warning");

            /// <summary>
            /// sh:detail
            /// </summary>
            public static readonly RDFResource DETAIL = new RDFResource(SHACL.BASE_URI + "detail");

            /// <summary>
            /// sh:focusNode
            /// </summary>
            public static readonly RDFResource FOCUS_NODE = new RDFResource(SHACL.BASE_URI + "focusNode");

            /// <summary>
            /// sh:resultMessage
            /// </summary>
            public static readonly RDFResource RESULT_MESSAGE = new RDFResource(SHACL.BASE_URI + "resultMessage");

            /// <summary>
            /// sh:resultPath
            /// </summary>
            public static readonly RDFResource RESULT_PATH = new RDFResource(SHACL.BASE_URI + "resultPath");

            /// <summary>
            /// sh:resultSeverity
            /// </summary>
            public static readonly RDFResource RESULT_SEVERITY = new RDFResource(SHACL.BASE_URI + "resultSeverity");

            /// <summary>
            /// sh:sourceConstraint
            /// </summary>
            public static readonly RDFResource SOURCE_CONSTRAINT = new RDFResource(SHACL.BASE_URI + "sourceConstraint");

            /// <summary>
            /// sh:sourceShape
            /// </summary>
            public static readonly RDFResource SOURCE_SHAPE = new RDFResource(SHACL.BASE_URI + "sourceShape");

            /// <summary>
            /// sh:sourceConstraintComponent
            /// </summary>
            public static readonly RDFResource SOURCE_CONSTRAINT_COMPONENT = new RDFResource(SHACL.BASE_URI + "sourceConstraintComponent");

            /// <summary>
            /// sh:value
            /// </summary>
            public static readonly RDFResource VALUE = new RDFResource(SHACL.BASE_URI + "value");

            /// <summary>
            /// sh:shapesGraph
            /// </summary>
            public static readonly RDFResource SHAPES_GRAPH = new RDFResource(SHACL.BASE_URI + "shapesGraph");

            /// <summary>
            /// sh:suggestedShapesGraph
            /// </summary>
            public static readonly RDFResource SUGGESTED_SHAPES_GRAPH = new RDFResource(SHACL.BASE_URI + "suggestedShapesGraph");

            /// <summary>
            /// sh:entailment
            /// </summary>
            public static readonly RDFResource ENTAILMENT = new RDFResource(SHACL.BASE_URI + "entailment");

            /// <summary>
            /// sh:path
            /// </summary>
            public static readonly RDFResource PATH = new RDFResource(SHACL.BASE_URI + "path");

            /// <summary>
            /// sh:inversePath
            /// </summary>
            public static readonly RDFResource INVERSE_PATH = new RDFResource(SHACL.BASE_URI + "inversePath");

            /// <summary>
            /// sh:alternativePath
            /// </summary>
            public static readonly RDFResource ALTERNATIVE_PATH = new RDFResource(SHACL.BASE_URI + "alternativePath");

            /// <summary>
            /// sh:zeroOrMorePath
            /// </summary>
            public static readonly RDFResource ZERO_OR_MORE_PATH = new RDFResource(SHACL.BASE_URI + "zeroOrMorePath");

            /// <summary>
            /// sh:oneOrMorePath
            /// </summary>
            public static readonly RDFResource ONE_OR_MORE_PATH = new RDFResource(SHACL.BASE_URI + "oneOrMorePath");

            /// <summary>
            /// sh:zeroOrOnePath
            /// </summary>
            public static readonly RDFResource ZERO_OR_ONE_PATH = new RDFResource(SHACL.BASE_URI + "zeroOrOnePath");

            /// <summary>
            /// sh:defaultValue
            /// </summary>
            public static readonly RDFResource DEFAULT_VALUE = new RDFResource(SHACL.BASE_URI + "defaultValue");

            /// <summary>
            /// sh:description
            /// </summary>
            public static readonly RDFResource DESCRIPTION = new RDFResource(SHACL.BASE_URI + "description");

            /// <summary>
            /// sh:group
            /// </summary>
            public static readonly RDFResource GROUP = new RDFResource(SHACL.BASE_URI + "group");

            /// <summary>
            /// sh:name
            /// </summary>
            public static readonly RDFResource NAME = new RDFResource(SHACL.BASE_URI + "name");

            /// <summary>
            /// sh:order
            /// </summary>
            public static readonly RDFResource ORDER = new RDFResource(SHACL.BASE_URI + "order");

            /// <summary>
            /// sh:PropertyGroup
            /// </summary>
            public static readonly RDFResource PROPERTY_GROUP = new RDFResource(SHACL.BASE_URI + "PropertyGroup");
            #endregion

        }
        #endregion
    }
}