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
using RDFSharp.Semantics.OWL;
using System.Collections.Generic;

namespace RDFSharp.Semantics.SKOS
{

    /// <summary>
    /// RDFSKOSOntology represents an OWL-DL ontology implementation of W3C SKOS vocabulary
    /// </summary>
    public static class RDFSKOSOntology
    {

        #region Properties
        /// <summary>
        /// Singleton instance of the SKOS ontology
        /// </summary>
        public static RDFOntology Instance { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the SKOS ontology
        /// </summary>
        static RDFSKOSOntology()
        {

            #region Declarations

            #region Ontology
            Instance = new RDFOntology(new RDFResource(RDFVocabulary.SKOS.BASE_URI));
            #endregion

            #region Classes
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SKOS.COLLECTION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SKOS.CONCEPT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SKOS.CONCEPT_SCHEME.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SKOS.ORDERED_COLLECTION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(new RDFOntologyUnionClass(new RDFResource("bnode:ConceptCollection")));

            //SKOS.SKOSXL
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToRDFOntologyClass());
            #endregion

            #region Properties
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.ALT_LABEL.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.BROAD_MATCH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.BROADER.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.BROADER_TRANSITIVE.ToRDFOntologyObjectProperty().SetTransitive(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.CHANGE_NOTE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.CLOSE_MATCH.ToRDFOntologyObjectProperty().SetSymmetric(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.DEFINITION.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.EDITORIAL_NOTE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.EXACT_MATCH.ToRDFOntologyObjectProperty().SetSymmetric(true).SetTransitive(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.EXAMPLE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.HAS_TOP_CONCEPT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.HIDDEN_LABEL.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.HISTORY_NOTE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.NARROW_MATCH.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.NARROWER.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.NARROWER_TRANSITIVE.ToRDFOntologyObjectProperty().SetTransitive(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.IN_SCHEME.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.MAPPING_RELATION.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.MEMBER.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.MEMBER_LIST.ToRDFOntologyObjectProperty().SetFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.NOTATION.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.NOTE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.PREF_LABEL.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.RELATED_MATCH.ToRDFOntologyObjectProperty().SetSymmetric(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.RELATED.ToRDFOntologyObjectProperty().SetSymmetric(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.SCOPE_NOTE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.SEMANTIC_RELATION.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.TOP_CONCEPT_OF.ToRDFOntologyObjectProperty());

            //SKOS.SKOSXL
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.SKOSXL.LITERAL_FORM.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.SKOSXL.PREF_LABEL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.SKOSXL.ALT_LABEL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.SKOSXL.HIDDEN_LABEL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.SKOS.SKOSXL.LABEL_RELATION.ToRDFOntologyObjectProperty().SetSymmetric(true));
            #endregion

            #endregion

            #region Taxonomies

            #region ClassModel

            //Restrictions
            Instance.Model.ClassModel.AddClass(new RDFOntologyCardinalityRestriction(new RDFResource("bnode:ExactlyOneLiteralForm"), Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SKOSXL.LITERAL_FORM.ToString()), 1, 1));

            //SubClassOf
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.ORDERED_COLLECTION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.COLLECTION.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()), Instance.Model.ClassModel.SelectClass("bnode:ExactlyOneLiteralForm"));

            //DisjointWith
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.COLLECTION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()));
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.COLLECTION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT_SCHEME.ToString()));
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.COLLECTION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT_SCHEME.ToString()));
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT_SCHEME.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));

            //UnionOf
            Instance.Model.ClassModel.AddUnionOfRelation(
                (RDFOntologyUnionClass)Instance.Model.ClassModel.SelectClass("bnode:ConceptCollection"),
                new List<RDFOntologyClass>() {
                    Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()),
                    Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.COLLECTION.ToString())
                }
            );

            #endregion

            #region PropertyModel

            //SubPropertyOf
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROAD_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROADER.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROAD_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.MAPPING_RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROADER.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROADER_TRANSITIVE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROADER_TRANSITIVE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SEMANTIC_RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.CLOSE_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.MAPPING_RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.EXACT_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.CLOSE_MATCH.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.MAPPING_RELATION.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SEMANTIC_RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROW_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROWER.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROW_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.MAPPING_RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROWER.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROWER_TRANSITIVE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROWER_TRANSITIVE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SEMANTIC_RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.TOP_CONCEPT_OF.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.IN_SCHEME.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.RELATED_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.RELATED.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.RELATED_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.MAPPING_RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.RELATED.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SEMANTIC_RELATION.ToString()));

            //InverseOf
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROAD_MATCH.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROW_MATCH.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROADER.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROWER.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.BROADER_TRANSITIVE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.NARROWER_TRANSITIVE.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.HAS_TOP_CONCEPT.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.TOP_CONCEPT_OF.ToString()));

            //Domain/Range
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.HAS_TOP_CONCEPT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT_SCHEME.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.HAS_TOP_CONCEPT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.IN_SCHEME.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT_SCHEME.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.MEMBER.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.COLLECTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.MEMBER.ToString()).SetRange(Instance.Model.ClassModel.SelectClass("bnode:ConceptCollection"));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.MEMBER_LIST.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.ORDERED_COLLECTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SEMANTIC_RELATION.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SEMANTIC_RELATION.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.TOP_CONCEPT_OF.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.TOP_CONCEPT_OF.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT_SCHEME.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SKOSXL.LITERAL_FORM.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SKOSXL.PREF_LABEL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SKOSXL.ALT_LABEL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SKOSXL.HIDDEN_LABEL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SKOSXL.LABEL_RELATION.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.SKOS.SKOSXL.LABEL_RELATION.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.SKOSXL.LABEL.ToString()));

            #endregion

            #endregion

        }
        #endregion

    }

}