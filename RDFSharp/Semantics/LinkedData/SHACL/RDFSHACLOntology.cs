/*
   Copyright 2015-2020 Marco De Salvo

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

using RDFSharp.Model;
using RDFSharp.Model.Vocabularies;
using RDFSharp.Semantics.Ontology;

namespace RDFSharp.Semantics.LinkedData.SHACL
{
    /// <summary>
    /// RDFSHACLOntology represents an OWL-DL ontology implementation of W3C SHACL Core vocabulary
    /// </summary>
    public static class RDFSHACLOntology
    {

        #region Properties
        /// <summary>
        /// Singleton instance of the SHACL ontology
        /// </summary>
        public static RDFOntology Instance { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the SHACL ontology
        /// </summary>
        static RDFSHACLOntology()
        {

            #region Declarations

            #region Ontology
            Instance = new RDFOntology(new RDFResource("https://rdfsharp.codeplex.com/semantics/shacl#"));
            #endregion

            #region Classes
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SHAPE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.NODE_SHAPE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.NODE_KIND_CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.BLANK_NODE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.BLANK_NODE_OR_IRI.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.BLANK_NODE_OR_LITERAL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.IRI.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.IRI_OR_LITERAL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.LITERAL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.VALIDATION_RESULT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SEVERITY_CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.VALIDATOR_CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_EXECUTABLE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_CONSTRAINT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_CONSTRAINT_COMPONENT_SPARQL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_ASK_VALIDATOR.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_ASK_EXECUTABLE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_SELECT_VALIDATOR.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_SELECT_EXECUTABLE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_CONSTRUCT_EXECUTABLE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.SPARQL_UPDATE_EXECUTABLE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.VALIDATION_REPORT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PARAMETERIZABLE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.AND_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.AND_CONSTRAINT_COMPONENT_AND.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.CLASS_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.CLASS_CONSTRAINT_COMPONENT_CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.CLOSED_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.CLOSED_CONSTRAINT_COMPONENT_CLOSED.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.CLOSED_CONSTRAINT_COMPONENT_IGNORED_PROPERTIES.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT_DATATYPE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT_DISJOINT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.EQUALS_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.EQUALS_CONSTRAINT_COMPONENT_EQUALS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.HAS_VALUE_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.HAS_VALUE_CONSTRAINT_COMPONENT_HAS_VALUE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.IN_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.IN_CONSTRAINT_COMPONENT_IN.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT_LANGUAGE_IN.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT_LESS_THAN.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT_LESS_THAN_OR_EQUALS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MAX_COUNT_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MAX_COUNT_CONSTRAINT_COMPONENT_MAX_COUNT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT_MIN_COUNT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT_MAX_EXCLUSIVE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MIN_EXCLUSIVE_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MIN_EXCLUSIVE_CONSTRAINT_COMPONENT_MIN_EXCLUSIVE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MAX_LENGTH_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MAX_LENGTH_CONSTRAINT_COMPONENT_MAX_LENGTH.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MIN_INCLUSIVE_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MIN_INCLUSIVE_CONSTRAINT_COMPONENT_MIN_INCLUSIVE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT_MIN_LENGTH.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.NODE_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.NODE_CONSTRAINT_COMPONENT_NODE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT_NODE_KIND.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.NOT_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.NOT_CONSTRAINT_COMPONENT_NOT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.OR_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.OR_CONSTRAINT_COMPONENT_OR.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT_FLAGS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT_PATTERN.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PROPERTY_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PROPERTY_CONSTRAINT_COMPONENT_PROPERTY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_MAX_COUNT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPES_DISJOINT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_MIN_COUNT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPES_DISJOINT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT_UNIQUE_LANG.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.XONE_CONSTRAINT_COMPONENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.XONE_CONSTRAINT_COMPONENT_XONE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PREFIX_DECLARATION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SHACL.PROPERTY_GROUP.ToRDFOntologyClass());
            #endregion

            #region Properties
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.CONFORMS.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SHAPES_GRAPH_WELL_FORMED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.DEACTIVATED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MESSAGE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.RESULT_MESSAGE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.OPTIONAL.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.DESCRIPTION.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.LABEL_TEMPLATE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.PREFIX_PROPERTY.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.NAMESPACE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SELECT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.CONSTRUCT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.UPDATE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.ASK.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.UNIQUE_LANG.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPES_DISJOINT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.PATTERN.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.FLAGS.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MIN_COUNT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MIN_LENGTH.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MAX_COUNT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MAX_LENGTH.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.CLOSED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.NAME.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.ORDER.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MAX_INCLUSIVE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MAX_EXCLUSIVE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MIN_INCLUSIVE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.MIN_EXCLUSIVE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.DECLARE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.PREFIXES.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.TARGET_CLASS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.TARGET_NODE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SEVERITY_PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.DETAIL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.GROUP.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.NOT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.NODE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.NODE_KIND.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.PARAMETER_PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SPARQL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.RESULT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.RESULT_SEVERITY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SOURCE_SHAPE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SHAPES_GRAPH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SUGGESTED_SHAPES_GRAPH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.VALIDATOR_PROPERTY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.NODE_VALIDATOR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.PROPERTY_VALIDATOR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.XONE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.OR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.AND.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.IN.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.ALTERNATIVE_PATH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.IGNORED_PROPERTIES.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.LANGUAGE_IN.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.TARGET_SUBJECTS_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.TARGET_OBJECTS_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.LESS_THAN.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.EQUALS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.DISJOINT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.CLASS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.DATATYPE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.PATH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.ENTAILMENT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.FOCUS_NODE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.SOURCE_CONSTRAINT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.RESULT_PATH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.ZERO_OR_MORE_PATH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.ZERO_OR_ONE_PATH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.ONE_OR_MORE_PATH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.INVERSE_PATH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.DEFAULT_VALUE.ToRDFOntologyProperty()); //plain property
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.HAS_VALUE.ToRDFOntologyProperty()); //plain property
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SHACL.VALUE.ToRDFOntologyProperty()); //plain property
            #endregion

            #region Facts
            Instance.Data.AddFact(RDFVocabulary.SHACL.INFO.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.SHACL.WARNING.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.SHACL.VIOLATION.ToRDFOntologyFact());
            #endregion

            #endregion

            #region Taxonomies

            #region ClassModel

            //SubClassOf
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_SHAPE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.BLANK_NODE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.BLANK_NODE_OR_IRI.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.BLANK_NODE_OR_LITERAL.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.IRI.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.IRI_OR_LITERAL.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.LITERAL.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETERIZABLE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.AND_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.AND_CONSTRAINT_COMPONENT_AND.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CLASS_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CLASS_CONSTRAINT_COMPONENT_CLASS.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CLOSED_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CLOSED_CONSTRAINT_COMPONENT_CLOSED.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CLOSED_CONSTRAINT_COMPONENT_IGNORED_PROPERTIES.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.DATATYPE_CONSTRAINT_COMPONENT_DATATYPE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.DISJOINT_CONSTRAINT_COMPONENT_DISJOINT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.EQUALS_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.EQUALS_CONSTRAINT_COMPONENT_EQUALS.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.HAS_VALUE_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.HAS_VALUE_CONSTRAINT_COMPONENT_HAS_VALUE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.IN_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.IN_CONSTRAINT_COMPONENT_IN.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.LANGUAGE_IN_CONSTRAINT_COMPONENT_LANGUAGE_IN.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.LESS_THAN_CONSTRAINT_COMPONENT_LESS_THAN.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.LESS_THAN_OR_EQUALS_CONSTRAINT_COMPONENT_LESS_THAN_OR_EQUALS.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MAX_COUNT_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MAX_COUNT_CONSTRAINT_COMPONENT_MAX_COUNT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MIN_COUNT_CONSTRAINT_COMPONENT_MIN_COUNT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MAX_EXCLUSIVE_CONSTRAINT_COMPONENT_MAX_EXCLUSIVE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MIN_EXCLUSIVE_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MIN_EXCLUSIVE_CONSTRAINT_COMPONENT_MIN_EXCLUSIVE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MAX_LENGTH_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MAX_LENGTH_CONSTRAINT_COMPONENT_MAX_LENGTH.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MIN_INCLUSIVE_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MIN_INCLUSIVE_CONSTRAINT_COMPONENT_MIN_INCLUSIVE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.MIN_LENGTH_CONSTRAINT_COMPONENT_MIN_LENGTH.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_CONSTRAINT_COMPONENT_NODE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CONSTRAINT_COMPONENT_NODE_KIND.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NOT_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NOT_CONSTRAINT_COMPONENT_NOT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.OR_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.OR_CONSTRAINT_COMPONENT_OR.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT_FLAGS.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PATTERN_CONSTRAINT_COMPONENT_PATTERN.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_CONSTRAINT_COMPONENT_PROPERTY.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_MAX_COUNT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPES_DISJOINT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_MIN_COUNT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT_CONSTRAINT_COMPONENT_QUALIFIED_VALUE_SHAPES_DISJOINT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.UNIQUE_LANG_CONSTRAINT_COMPONENT_UNIQUE_LANG.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.XONE_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.XONE_CONSTRAINT_COMPONENT_XONE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_CONSTRAINT_COMPONENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_CONSTRAINT_COMPONENT_SPARQL.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_ASK_VALIDATOR.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATOR_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_ASK_VALIDATOR.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_ASK_EXECUTABLE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_ASK_EXECUTABLE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_EXECUTABLE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_SELECT_VALIDATOR.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATOR_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_SELECT_VALIDATOR.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_SELECT_EXECUTABLE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_SELECT_EXECUTABLE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_EXECUTABLE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_CONSTRUCT_EXECUTABLE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_EXECUTABLE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_UPDATE_EXECUTABLE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_EXECUTABLE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_CONSTRAINT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_SELECT_EXECUTABLE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATION_RESULT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));

            #endregion

            #region PropertyModel

            //Domain/Range
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SELECT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_SELECT_EXECUTABLE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SELECT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.CONSTRUCT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_CONSTRUCT_EXECUTABLE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.CONSTRUCT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.ASK.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_ASK_EXECUTABLE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.ASK.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.UPDATE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_UPDATE_EXECUTABLE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.UPDATE.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.CONFORMS.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATION_REPORT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.CONFORMS.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BOOLEAN.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PREFIX_PROPERTY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PREFIX_DECLARATION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PREFIX_PROPERTY.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NAMESPACE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PREFIX_DECLARATION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NAMESPACE.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.ANY_URI.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SHAPES_GRAPH_WELL_FORMED.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATION_REPORT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SHAPES_GRAPH_WELL_FORMED.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BOOLEAN.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.DEACTIVATED.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BOOLEAN.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.RESULT_MESSAGE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.DESCRIPTION.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.DESCRIPTION.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.OPTIONAL.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.OPTIONAL.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BOOLEAN.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PATTERN.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.FLAGS.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.CLOSED.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BOOLEAN.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NAME.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NAME.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.ORDER.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INTEGER.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.LABEL_TEMPLATE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETERIZABLE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.UNIQUE_LANG.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BOOLEAN.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.QUALIFIED_MIN_COUNT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INTEGER.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.QUALIFIED_MAX_COUNT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INTEGER.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.QUALIFIED_VALUE_SHAPES_DISJOINT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.BOOLEAN.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PROPERTY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PROPERTY.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.DECLARE.ToString()).SetDomain(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.OWL.ONTOLOGY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.DECLARE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PREFIX_DECLARATION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PREFIXES.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_EXECUTABLE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PREFIXES.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.OWL.ONTOLOGY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.TARGET_CLASS.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.TARGET_NODE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SEVERITY_PROPERTY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SEVERITY_PROPERTY.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SEVERITY_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.DETAIL.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.DETAIL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.GROUP.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.GROUP.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_GROUP.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NOT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NODE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NODE_KIND.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.NODE_KIND_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.MIN_COUNT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INTEGER.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.MIN_LENGTH.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INTEGER.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.MAX_COUNT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INTEGER.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.MAX_LENGTH.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.INTEGER.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PARAMETER_PROPERTY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETERIZABLE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PARAMETER_PROPERTY.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PARAMETER_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SPARQL.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SPARQL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SPARQL_CONSTRAINT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.RESULT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATION_REPORT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.RESULT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATION_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.RESULT_SEVERITY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.RESULT_SEVERITY.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SEVERITY_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SOURCE_SHAPE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SOURCE_SHAPE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SOURCE_CONSTRAINT_COMPONENT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SHAPES_GRAPH.ToString()).SetDomain(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.OWL.ONTOLOGY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SHAPES_GRAPH.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.OWL.ONTOLOGY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SUGGESTED_SHAPES_GRAPH.ToString()).SetDomain(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.OWL.ONTOLOGY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SUGGESTED_SHAPES_GRAPH.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.OWL.ONTOLOGY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.ENTAILMENT.ToString()).SetDomain(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.OWL.ONTOLOGY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.VALIDATOR_PROPERTY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.VALIDATOR_PROPERTY.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATOR_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NODE_VALIDATOR.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.NODE_VALIDATOR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATOR_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PROPERTY_VALIDATOR.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.CONSTRAINT_COMPONENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PROPERTY_VALIDATOR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.VALIDATOR_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.TARGET_SUBJECTS_OF.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.TARGET_OBJECTS_OF.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.PATH.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.RESULT_PATH.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.DEFAULT_VALUE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.PROPERTY_SHAPE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.VALUE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.FOCUS_NODE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.SOURCE_CONSTRAINT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.ABSTRACT_RESULT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.XONE.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.RDF.LIST.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.OR.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.RDF.LIST.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.AND.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.RDF.LIST.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.IN.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.RDF.LIST.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.IGNORED_PROPERTIES.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.RDF.LIST.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.LANGUAGE_IN.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.RDF.LIST.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SHACL.ALTERNATIVE_PATH.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.RDF.LIST.ToString()));

            #endregion

            #region Data

            //ClassType
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.SHACL.INFO.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SEVERITY_CLASS.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.SHACL.WARNING.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SEVERITY_CLASS.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.SHACL.VIOLATION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SHACL.SEVERITY_CLASS.ToString()));

            #endregion

            #endregion

        }
        #endregion

    }
}