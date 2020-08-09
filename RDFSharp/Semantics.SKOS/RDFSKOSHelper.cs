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
using System;
using System.Collections.Generic;

namespace RDFSharp.Semantics
{

    /// <summary>
    ///  RDFSKOSHelper contains utility methods supporting SKOS modeling and reasoning
    /// </summary>
    public static class RDFSKOSHelper {

        #region Modeling

        #region Relations

        #region TopConcept
        /// <summary>
        /// Adds the given concept to the conceptScheme as top concept of the hierarchy
        /// </summary>
        public static RDFSKOSConceptScheme AddTopConceptRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                                 RDFSKOSConcept concept) {
            if (conceptScheme != null && concept != null && !conceptScheme.Equals(concept)) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add skos:hasTopConcept relation to the scheme
                conceptScheme.Relations.TopConcept.AddEntry(new RDFOntologyTaxonomyEntry(conceptScheme, RDFVocabulary.SKOS.HAS_TOP_CONCEPT.ToRDFOntologyObjectProperty(), concept));

            }
            return conceptScheme;
        }
        #endregion

        #region Semantic
        /// <summary>
        /// Adds a 'skos:semanticRelation' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddSemanticRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                               RDFSKOSConcept aConcept, 
                                                               RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {

                //Add concepts to the scheme
                conceptScheme.AddConcept(aConcept);
                conceptScheme.AddConcept(bConcept);

                //Add skos:semanticRelation relation to the scheme
                conceptScheme.Relations.SemanticRelation.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.SEMANTIC_RELATION.ToRDFOntologyObjectProperty(), bConcept));

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:related' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddRelatedRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                              RDFSKOSConcept aConcept,
                                                              RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckRelatedRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:related relations to the scheme
                    conceptScheme.Relations.Related.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.RELATED.ToRDFOntologyObjectProperty(), bConcept));
                    conceptScheme.Relations.Related.AddEntry(new RDFOntologyTaxonomyEntry(bConcept, RDFVocabulary.SKOS.RELATED.ToRDFOntologyObjectProperty(), aConcept)
                                                   .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));

                }
            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:broader' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddBroaderRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                              RDFSKOSConcept aConcept, 
                                                              RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckBroaderRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:broader relation to the scheme
                    conceptScheme.Relations.Broader.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.BROADER.ToRDFOntologyObjectProperty(), bConcept));

                }
            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:broaderTransitive' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddBroaderTransitiveRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                                        RDFSKOSConcept aConcept,
                                                                        RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckBroaderRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:broaderTransitive relation to the scheme
                    conceptScheme.Relations.BroaderTransitive.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.BROADER_TRANSITIVE.ToRDFOntologyObjectProperty(), bConcept));

                }
            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:narrower' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddNarrowerRelation(this RDFSKOSConceptScheme conceptScheme,
                                                               RDFSKOSConcept aConcept,
                                                               RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckNarrowerRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:narrower relation to the scheme
                    conceptScheme.Relations.Narrower.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.NARROWER.ToRDFOntologyObjectProperty(), bConcept));

                }
            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:narrowerTransitive' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddNarrowerTransitiveRelation(this RDFSKOSConceptScheme conceptScheme,
                                                                         RDFSKOSConcept aConcept,
                                                                         RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckNarrowerRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:narrowerTransitive relation to the scheme
                    conceptScheme.Relations.NarrowerTransitive.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.NARROWER_TRANSITIVE.ToRDFOntologyObjectProperty(), bConcept));

                }
            }
            return conceptScheme;
        }
        #endregion

        #region Mapping
        /// <summary>
        /// Adds a 'skos:mappingRelation' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddMappingRelation(this RDFSKOSConceptScheme conceptScheme,
                                                              RDFSKOSConcept aConcept,
                                                              RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {

                //Add concepts to the scheme
                conceptScheme.AddConcept(aConcept);
                conceptScheme.AddConcept(bConcept);

                //Add skos:mappingRelation relation to the scheme
                conceptScheme.Relations.MappingRelation.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.MAPPING_RELATION.ToRDFOntologyObjectProperty(), bConcept));

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:closeMatch' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddCloseMatchRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                                 RDFSKOSConcept aConcept, 
                                                                 RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckCloseOrExactMatchRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:closeMatch relation to the scheme
                    conceptScheme.Relations.CloseMatch.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.CLOSE_MATCH.ToRDFOntologyObjectProperty(), bConcept));
                    conceptScheme.Relations.CloseMatch.AddEntry(new RDFOntologyTaxonomyEntry(bConcept, RDFVocabulary.SKOS.CLOSE_MATCH.ToRDFOntologyObjectProperty(), aConcept)
                                                      .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));

                }
            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:exactMatch' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddExactMatchRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                                 RDFSKOSConcept aConcept, 
                                                                 RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckCloseOrExactMatchRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:exactMatch relation to the scheme
                    conceptScheme.Relations.ExactMatch.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.EXACT_MATCH.ToRDFOntologyObjectProperty(), bConcept));
                    conceptScheme.Relations.ExactMatch.AddEntry(new RDFOntologyTaxonomyEntry(bConcept, RDFVocabulary.SKOS.EXACT_MATCH.ToRDFOntologyObjectProperty(), aConcept)
                                                      .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));

                }
            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:broadMatch' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddBroadMatchRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                                 RDFSKOSConcept aConcept, 
                                                                 RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckBroaderRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:broadMatch relation to the scheme
                    conceptScheme.Relations.BroadMatch.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.BROAD_MATCH.ToRDFOntologyObjectProperty(), bConcept));

                }
            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:narrowMatch' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddNarrowMatchRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                                  RDFSKOSConcept aConcept, 
                                                                  RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckNarrowerRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:narrowMatch relation to the scheme
                    conceptScheme.Relations.NarrowMatch.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.NARROW_MATCH.ToRDFOntologyObjectProperty(), bConcept));

                }
            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skos:relatedMatch' relation between the given concepts within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddRelatedMatchAssertion(this RDFSKOSConceptScheme conceptScheme, 
                                                                    RDFSKOSConcept aConcept, 
                                                                    RDFSKOSConcept bConcept) {
            if (conceptScheme != null && aConcept != null && bConcept != null && !aConcept.Equals(bConcept)) {
                if (RDFSKOSChecker.CheckRelatedRelation(conceptScheme, aConcept, bConcept)) {

                    //Add concepts to the scheme
                    conceptScheme.AddConcept(aConcept);
                    conceptScheme.AddConcept(bConcept);

                    //Add skos:relatedMatch relation to the scheme
                    conceptScheme.Relations.RelatedMatch.AddEntry(new RDFOntologyTaxonomyEntry(aConcept, RDFVocabulary.SKOS.RELATED_MATCH.ToRDFOntologyObjectProperty(), bConcept));
                    conceptScheme.Relations.RelatedMatch.AddEntry(new RDFOntologyTaxonomyEntry(bConcept, RDFVocabulary.SKOS.RELATED_MATCH.ToRDFOntologyObjectProperty(), aConcept)
                                                        .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));

                }
            }
            return conceptScheme;
        }
        #endregion

        #region Notation
        /// <summary>
        /// Adds the given notation to the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddNotationRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                               RDFSKOSConcept concept, 
                                                               RDFOntologyLiteral notationLiteral) {
            if (conceptScheme != null && concept != null && notationLiteral != null) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add skos:Notation relation to the scheme
                conceptScheme.Relations.Notation.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.NOTATION.ToRDFOntologyDatatypeProperty(), notationLiteral));

            }
            return conceptScheme;
        }
        #endregion

        #region Label
        /// <summary>
        /// Adds the given label as preferred label of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddPrefLabelRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                                RDFSKOSConcept concept, 
                                                                RDFSKOSLabel label, 
                                                                RDFOntologyLiteral prefLabelLiteral) {
            if (conceptScheme != null && concept != null && label != null && prefLabelLiteral != null) {

                //Only plain literals are allowed as skosxl:prefLabel assertions
                if (prefLabelLiteral.Value  is RDFPlainLiteral) {
                    if (RDFSKOSChecker.CheckPrefLabel(conceptScheme, concept, prefLabelLiteral)) {

                        //Add concept to the scheme
                        conceptScheme.AddConcept(concept);

                        //Add label to the scheme
                        conceptScheme.AddLabel(label);

                        //Add prefLabel relation
                        conceptScheme.Relations.PrefLabel.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.SKOSXL.PREF_LABEL.ToRDFOntologyObjectProperty(), label));

                        //Add literalForm relation
                        conceptScheme.Relations.LiteralForm.AddEntry(new RDFOntologyTaxonomyEntry(label, RDFVocabulary.SKOS.SKOSXL.LITERAL_FORM.ToRDFOntologyDatatypeProperty(), prefLabelLiteral));

                    }
                }

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given label as alternative label of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddAltLabelRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                               RDFSKOSConcept concept, 
                                                               RDFSKOSLabel label, 
                                                               RDFOntologyLiteral altLabelLiteral) {
            if (conceptScheme != null && concept != null && label != null && altLabelLiteral != null) {

                //Only plain literals are allowed as skosxl:altLabel assertions
                if (altLabelLiteral.Value  is RDFPlainLiteral) {
                    if (RDFSKOSChecker.CheckAltLabel(conceptScheme, concept, altLabelLiteral)) {

                        //Add concept to the scheme
                        conceptScheme.AddConcept(concept);

                        //Add label to the scheme
                        conceptScheme.AddLabel(label);

                        //Add altLabel relation
                        conceptScheme.Relations.AltLabel.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.SKOSXL.ALT_LABEL.ToRDFOntologyObjectProperty(), label));

                        //Add literalForm relation
                        conceptScheme.Relations.LiteralForm.AddEntry(new RDFOntologyTaxonomyEntry(label, RDFVocabulary.SKOS.SKOSXL.LITERAL_FORM.ToRDFOntologyDatatypeProperty(), altLabelLiteral));

                    }
                }

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given label as hidden label of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddHiddenLabelRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                                  RDFSKOSConcept concept, 
                                                                  RDFSKOSLabel label, 
                                                                  RDFOntologyLiteral hiddenLabelLiteral) {
            if (conceptScheme != null && concept != null && label != null && hiddenLabelLiteral != null) {

                //Only plain literals are allowed as skosxl:hiddenLabel assertions
                if (hiddenLabelLiteral.Value  is RDFPlainLiteral) {
                    if (RDFSKOSChecker.CheckHiddenLabel(conceptScheme, concept, hiddenLabelLiteral)) {

                        //Add concept to the scheme
                        conceptScheme.AddConcept(concept);

                        //Add label to the scheme
                        conceptScheme.AddLabel(label);

                        //Add hiddenLabel relation
                        conceptScheme.Relations.HiddenLabel.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.SKOSXL.HIDDEN_LABEL.ToRDFOntologyObjectProperty(), label));

                        //Add literalForm relation
                        conceptScheme.Relations.LiteralForm.AddEntry(new RDFOntologyTaxonomyEntry(label, RDFVocabulary.SKOS.SKOSXL.LITERAL_FORM.ToRDFOntologyDatatypeProperty(), hiddenLabelLiteral));


                    }
                }

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds a 'skosxl:labelRelation' relation between the given labels within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddLabelRelation(this RDFSKOSConceptScheme conceptScheme, 
                                                            RDFSKOSLabel aLabel, 
                                                            RDFSKOSLabel bLabel) {
            if (conceptScheme != null && aLabel != null && bLabel != null && !aLabel.Equals(bLabel)) {

                //Add labels to the scheme
                conceptScheme.AddLabel(aLabel);
                conceptScheme.AddLabel(bLabel);

                //Add skosxl:labelRelation relation to the scheme
                conceptScheme.Relations.LabelRelation.AddEntry(new RDFOntologyTaxonomyEntry(aLabel, RDFVocabulary.SKOS.SKOSXL.LABEL_RELATION.ToRDFOntologyObjectProperty(), bLabel));
                conceptScheme.Relations.LabelRelation.AddEntry(new RDFOntologyTaxonomyEntry(bLabel, RDFVocabulary.SKOS.SKOSXL.LABEL_RELATION.ToRDFOntologyObjectProperty(), aLabel)
                                                     .SetInference(RDFSemanticsEnums.RDFOntologyInferenceType.API));

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given literal form of the given label within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddLiteralFormAssertion(this RDFSKOSConceptScheme conceptScheme, 
                                                                   RDFSKOSLabel label, 
                                                                   RDFOntologyLiteral literal) {
            if (conceptScheme != null && label != null && literal != null) {

                //Add label to the scheme
                conceptScheme.AddLabel(label);

                //Add literalForm relation
                conceptScheme.Relations.LiteralForm.AddEntry(new RDFOntologyTaxonomyEntry(label, RDFVocabulary.SKOS.SKOSXL.LITERAL_FORM.ToRDFOntologyDatatypeProperty(), literal));

            }
            return conceptScheme;
        }
        #endregion

        #endregion

        #region Annotations

        #region Lexical Labeling
        /// <summary>
        /// Adds the given literal as preferred label annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddPrefLabelAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                  RDFSKOSConcept concept, 
                                                                  RDFOntologyLiteral prefLabelLiteral) {
            if (conceptScheme != null && concept != null && prefLabelLiteral != null) {

                //Only plain literals are allowed as skos:prefLabel annotations
                if (prefLabelLiteral.Value  is RDFPlainLiteral) {
                    if (RDFSKOSChecker.CheckPrefLabel(conceptScheme, concept, prefLabelLiteral)) {

                        //Add concept to the scheme
                        conceptScheme.AddConcept(concept);

                        //Add prefLabel annotation
                        conceptScheme.Annotations.PrefLabel.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.PREF_LABEL.ToRDFOntologyAnnotationProperty(), prefLabelLiteral));

                    }
                }

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given literal as alternative label annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddAltLabelAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                 RDFSKOSConcept concept, 
                                                                 RDFOntologyLiteral altLabelLiteral) {
            if (conceptScheme != null && concept != null && altLabelLiteral != null) {

                //Only plain literals are allowed as skos:altLabel annotations
                if (altLabelLiteral.Value is RDFPlainLiteral) {
                    if (RDFSKOSChecker.CheckAltLabel(conceptScheme, concept, altLabelLiteral)) {

                        //Add concept to the scheme
                        conceptScheme.AddConcept(concept);

                        //Add altLabel annotation
                        conceptScheme.Annotations.AltLabel.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.ALT_LABEL.ToRDFOntologyAnnotationProperty(), altLabelLiteral));

                    }
                }

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the "concept -> skos:hiddenLabel -> hiddenLabelLiteral" annotation to the concept scheme
        /// </summary>
        public static RDFSKOSConceptScheme AddHiddenLabelAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                    RDFSKOSConcept concept, 
                                                                    RDFOntologyLiteral hiddenLabelLiteral) {
            if (conceptScheme != null && concept != null && hiddenLabelLiteral != null) {

                //Only plain literals are allowed as skos:hiddenLabel annotations
                if (hiddenLabelLiteral.Value is RDFPlainLiteral) {
                    if (RDFSKOSChecker.CheckHiddenLabel(conceptScheme, concept, hiddenLabelLiteral)) {

                        //Add concept to the scheme
                        conceptScheme.AddConcept(concept);

                        //Add hiddenLabel annotation
                        conceptScheme.Annotations.HiddenLabel.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.HIDDEN_LABEL.ToRDFOntologyAnnotationProperty(), hiddenLabelLiteral));

                    }
                }

            }
            return conceptScheme;
        }
        #endregion

        #region Documentation
        /// <summary>
        /// Adds the given literal as 'skos:note' annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddNoteAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                             RDFSKOSConcept concept, 
                                                             RDFOntologyLiteral literal) {
            if (conceptScheme != null && concept != null && literal != null) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add note annotation to the scheme
                conceptScheme.Annotations.Note.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.NOTE.ToRDFOntologyAnnotationProperty(), literal));

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given literal as 'skos:changeNote' annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddChangeNoteAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                   RDFSKOSConcept concept, 
                                                                   RDFOntologyLiteral literal) {
            if (conceptScheme != null && concept != null && literal != null) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add changeNote annotation to the scheme
                conceptScheme.Annotations.ChangeNote.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.CHANGE_NOTE.ToRDFOntologyAnnotationProperty(), literal));

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given literal as 'skos:editorialNote' annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddEditorialNoteAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                      RDFSKOSConcept concept, 
                                                                      RDFOntologyLiteral literal) {
            if (conceptScheme != null && concept != null && literal != null) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add editorialNote annotation to the scheme
                conceptScheme.Annotations.EditorialNote.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.EDITORIAL_NOTE.ToRDFOntologyAnnotationProperty(), literal));

            }
            return conceptScheme;
        }

        /// <summary>
        ///Adds the given literal as 'skos:historyNote' annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddHistoryNoteAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                    RDFSKOSConcept concept, 
                                                                    RDFOntologyLiteral literal) {
            if (conceptScheme != null && concept != null && literal != null) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add historyNote annotation to the scheme
                conceptScheme.Annotations.HistoryNote.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.HISTORY_NOTE.ToRDFOntologyAnnotationProperty(), literal));

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given literal as 'skos:scopeNote' annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddScopeNoteAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                  RDFSKOSConcept concept, 
                                                                  RDFOntologyLiteral literal) {
            if (conceptScheme != null && concept != null && literal != null) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add scopeNote annotation to the scheme
                conceptScheme.Annotations.ScopeNote.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.SCOPE_NOTE.ToRDFOntologyAnnotationProperty(), literal));

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given literal as 'skos:definition' annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddDefinitionAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                   RDFSKOSConcept concept, 
                                                                   RDFOntologyLiteral literal) {
            if (conceptScheme != null && concept != null && literal != null) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add definition annotation to the scheme
                conceptScheme.Annotations.Definition.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.DEFINITION.ToRDFOntologyAnnotationProperty(), literal));

            }
            return conceptScheme;
        }

        /// <summary>
        /// Adds the given literal as 'skos:example' annotation of the given concept within the conceptScheme
        /// </summary>
        public static RDFSKOSConceptScheme AddExampleAnnotation(this RDFSKOSConceptScheme conceptScheme, 
                                                                RDFSKOSConcept concept, 
                                                                RDFOntologyLiteral literal) {
            if (conceptScheme != null && concept != null && literal != null) {

                //Add concept to the scheme
                conceptScheme.AddConcept(concept);

                //Add example annotation to the scheme
                conceptScheme.Annotations.Example.AddEntry(new RDFOntologyTaxonomyEntry(concept, RDFVocabulary.SKOS.EXAMPLE.ToRDFOntologyAnnotationProperty(), literal));

            }
            return conceptScheme;
        }
        #endregion

        #endregion

        #endregion

        #region Reasoning

        #region Semantic

        #region Broader/BroaderTransitive
        /// <summary>
        /// Checks if the given aConcept has broader/broaderTransitive concept the given bConcept within the given scheme
        /// </summary>
        public static Boolean CheckHasBroaderConcept(this RDFSKOSConceptScheme conceptScheme, 
                                                     RDFSKOSConcept aConcept, 
                                                     RDFSKOSConcept bConcept) {
            return (aConcept != null && bConcept != null && conceptScheme != null ? conceptScheme.GetBroaderConceptsOf(aConcept).Concepts.ContainsKey(bConcept.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the broader/broaderTransitive concepts of the given concept within the given scheme
        /// </summary>
        public static RDFSKOSConceptScheme GetBroaderConceptsOf(this RDFSKOSConceptScheme conceptScheme, 
                                                                RDFSKOSConcept concept) {
            var result         = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);
            if (concept       != null && conceptScheme != null) {

                //Get skos:broader concepts
                foreach (var broaderConcept in conceptScheme.Relations.Broader.SelectEntriesBySubject(concept)) {
                    result.AddConcept((RDFSKOSConcept)broaderConcept.TaxonomyObject);
                }

                //Get skos:broaderTransitive concepts
                result         = result.UnionWith(conceptScheme.GetBroaderConceptsOfInternal(concept, null))
                                       .RemoveConcept(concept); //Safety deletion

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "skos:broaderTransitive" taxonomy to discover direct and indirect broader concepts of the given scheme
        /// </summary>
        internal static RDFSKOSConceptScheme GetBroaderConceptsOfInternal(this RDFSKOSConceptScheme conceptScheme, 
                                                                          RDFSKOSConcept concept, 
                                                                          Dictionary<Int64, RDFSKOSConcept> visitContext) {
            var result        = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);

            #region visitContext
            if (visitContext == null) {
                visitContext  = new Dictionary<Int64, RDFSKOSConcept>() { { concept.PatternMemberID, concept } };
            }
            else {
                if (!visitContext.ContainsKey(concept.PatternMemberID)) {
                     visitContext.Add(concept.PatternMemberID, concept);
                }
                else {
                     return result;
                }
            }
            #endregion

            //Transitivity of "skos:broaderTransitive" taxonomy: 
            //((A SKOS:BROADERTRANSITIVE B)  &&  (B SKOS:BROADERTRANSITIVE C))  =>  (A SKOS:BROADERTRANSITIVE C)
            foreach (var bt  in conceptScheme.Relations.BroaderTransitive.SelectEntriesBySubject(concept)) {
                result.AddConcept((RDFSKOSConcept)bt.TaxonomyObject);

                //Exploit skos:broaderTransitive taxonomy
                result        = result.UnionWith(conceptScheme.GetBroaderConceptsOfInternal((RDFSKOSConcept)bt.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #region Narrower/NarrowerTransitive
        /// <summary>
        /// Checks if the given aConcept has narrower/narrowerTransitive concept the given bConcept within the given scheme
        /// </summary>
        public static Boolean CheckHasNarrowerConcept(this RDFSKOSConceptScheme conceptScheme, 
                                                      RDFSKOSConcept aConcept, 
                                                      RDFSKOSConcept bConcept) {
            return (aConcept != null && bConcept != null && conceptScheme != null ? conceptScheme.GetNarrowerConceptsOf(aConcept).Concepts.ContainsKey(bConcept.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the narrower/narrowerTransitive concepts of the given concept within the given scheme
        /// </summary>
        public static RDFSKOSConceptScheme GetNarrowerConceptsOf(this RDFSKOSConceptScheme conceptScheme, 
                                                                 RDFSKOSConcept concept) {
            var result   = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);
            if (concept != null && conceptScheme != null) {

                //Get skos:narrower concepts
                foreach (var narrowerConcept in conceptScheme.Relations.Narrower.SelectEntriesBySubject(concept)) {
                    result.AddConcept((RDFSKOSConcept)narrowerConcept.TaxonomyObject);
                }

                //Get skos:narrowerTransitive concepts
                result   = result.UnionWith(conceptScheme.GetNarrowerConceptsOfInternal(concept, null))
                                 .RemoveConcept(concept); //Safety deletion

            }
            return result;
        }

        /// <summary>
        /// Subsumes the "skos:narrowerTransitive" taxonomy to discover direct and indirect narrower concepts of the given scheme
        /// </summary>
        internal static RDFSKOSConceptScheme GetNarrowerConceptsOfInternal(this RDFSKOSConceptScheme conceptScheme, 
                                                                           RDFSKOSConcept concept, 
                                                                           Dictionary<Int64, RDFSKOSConcept> visitContext) {
            var result         = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);

            #region visitContext
            if (visitContext  == null) {
                visitContext   = new Dictionary<Int64, RDFSKOSConcept>() { { concept.PatternMemberID, concept } };
            }
            else {
                if (!visitContext.ContainsKey(concept.PatternMemberID)) {
                     visitContext.Add(concept.PatternMemberID, concept);
                }
                else {
                     return result;
                }
            }
            #endregion

            //Transitivity of "skos:narrowerTransitive" taxonomy:
            //((A SKOS:NARROWERTRANSITIVE B)  &&  (B SKOS:NARROWERTRANSITIVE C))  =>  (A SKOS:NARROWERTRANSITIVE C)
            foreach (var nt   in conceptScheme.Relations.NarrowerTransitive.SelectEntriesBySubject(concept)) {
                result.AddConcept((RDFSKOSConcept)nt.TaxonomyObject);

                //Exploit skos:narrowerTransitive taxonomy
                result         = result.UnionWith(conceptScheme.GetNarrowerConceptsOfInternal((RDFSKOSConcept)nt.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #region Related
        /// <summary>
        /// Checks if the given aConcept has related concept the given bConcept within the given scheme
        /// </summary>
        public static Boolean CheckHasRelatedConcept(this RDFSKOSConceptScheme data, 
                                                     RDFSKOSConcept aConcept, 
                                                     RDFSKOSConcept bConcept) {
            return (aConcept != null && bConcept != null && data != null ? data.GetRelatedConceptsOf(aConcept).Concepts.ContainsKey(bConcept.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the related concepts of the given concept within the given scheme
        /// </summary>
        public static RDFSKOSConceptScheme GetRelatedConceptsOf(this RDFSKOSConceptScheme conceptScheme, 
                                                                RDFSKOSConcept concept) {
            var result   = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);
            if (concept != null && conceptScheme != null) {
                foreach (var relatedConcept in conceptScheme.Relations.Related.SelectEntriesBySubject(concept)) {
                    result.AddConcept((RDFSKOSConcept)relatedConcept.TaxonomyObject);
                }
            }
            return result;
        }
        #endregion

        #endregion

        #region Mapping

        #region BroadMatch
        /// <summary>
        /// Checks if the given aConcept has broadMatch concept the given bConcept within the given scheme
        /// </summary>
        public static Boolean CheckHasBroadMatchConcept(this RDFSKOSConceptScheme conceptScheme, 
                                                        RDFSKOSConcept aConcept, 
                                                        RDFSKOSConcept bConcept) {
            return (aConcept != null && bConcept != null && conceptScheme != null ? conceptScheme.GetBroadMatchConceptsOf(aConcept).Concepts.ContainsKey(bConcept.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the broadMatch concepts of the given concept within the given scheme
        /// </summary>
        public static RDFSKOSConceptScheme GetBroadMatchConceptsOf(this RDFSKOSConceptScheme conceptScheme, 
                                                                   RDFSKOSConcept concept) {
            var result   = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);
            if (concept != null && conceptScheme != null) {
                foreach (var broadMatchConcept in conceptScheme.Relations.BroadMatch.SelectEntriesBySubject(concept)) {
                    result.AddConcept((RDFSKOSConcept)broadMatchConcept.TaxonomyObject);
                }
            }
            return result;
        }
        #endregion

        #region NarrowMatch
        /// <summary>
        /// Checks if the given aConcept has narrowMatch concept the given bConcept within the given scheme
        /// </summary>
        public static Boolean CheckHasNarrowMatchConcept(this RDFSKOSConceptScheme conceptScheme, 
                                                         RDFSKOSConcept aConcept, 
                                                         RDFSKOSConcept bConcept) {
            return (aConcept != null && bConcept != null && conceptScheme != null ? conceptScheme.GetNarrowMatchConceptsOf(aConcept).Concepts.ContainsKey(bConcept.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the narrowMatch concepts of the given concept within the given scheme
        /// </summary>
        public static RDFSKOSConceptScheme GetNarrowMatchConceptsOf(this RDFSKOSConceptScheme conceptScheme, 
                                                                    RDFSKOSConcept concept) {
            var result   = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);
            if (concept != null && conceptScheme != null) {
                foreach (var narrowMatchConcept in conceptScheme.Relations.NarrowMatch.SelectEntriesBySubject(concept)) {
                    result.AddConcept((RDFSKOSConcept)narrowMatchConcept.TaxonomyObject);
                }
            }
            return result;
        }
        #endregion

        #region RelatedMatch
        /// <summary>
        /// Checks if the given aConcept has relatedMatch concept the given bConcept within the given scheme
        /// </summary>
        public static Boolean CheckHasRelatedMatchConcept(this RDFSKOSConceptScheme conceptScheme, 
                                                          RDFSKOSConcept aConcept, 
                                                          RDFSKOSConcept bConcept) {
            return (aConcept != null && bConcept != null && conceptScheme != null ? conceptScheme.GetRelatedMatchConceptsOf(aConcept).Concepts.ContainsKey(bConcept.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the relatedMatch concepts of the given concept within the given scheme
        /// </summary>
        public static RDFSKOSConceptScheme GetRelatedMatchConceptsOf(this RDFSKOSConceptScheme conceptScheme, 
                                                                     RDFSKOSConcept concept) {
            var result   = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);
            if (concept != null && conceptScheme != null) {
                foreach (var relatedMatchConcept in conceptScheme.Relations.RelatedMatch.SelectEntriesBySubject(concept)) {
                    result.AddConcept((RDFSKOSConcept)relatedMatchConcept.TaxonomyObject);
                }
            }
            return result;
        }
        #endregion

        #region CloseMatch
        /// <summary>
        /// Checks if the given aConcept skos:closeMatch the given bConcept within the given scheme
        /// </summary>
        public static Boolean CheckHasCloseMatchConcept(this RDFSKOSConceptScheme conceptScheme, 
                                                        RDFSKOSConcept aConcept, 
                                                        RDFSKOSConcept bConcept) {
            return (aConcept != null && bConcept != null && conceptScheme != null ? conceptScheme.GetCloseMatchConceptsOf(aConcept).Concepts.ContainsKey(bConcept.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the skos:closeMatch concepts of the given concept within the given scheme
        /// </summary>
        public static RDFSKOSConceptScheme GetCloseMatchConceptsOf(this RDFSKOSConceptScheme conceptScheme, 
                                                                   RDFSKOSConcept concept) {
            var result   = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);
            if (concept != null && conceptScheme != null) {
                foreach (var closeMatchConcept in conceptScheme.Relations.CloseMatch.SelectEntriesBySubject(concept)) {
                    result.AddConcept((RDFSKOSConcept)closeMatchConcept.TaxonomyObject);
                }
            }
            return result;
        }
        #endregion

        #region ExactMatch
        /// <summary>
        /// Checks if the given aConcept skos:exactMatch the given bConcept within the given scheme
        /// </summary>
        public static Boolean CheckHasExactMatchConcept(this RDFSKOSConceptScheme conceptScheme, 
                                                        RDFSKOSConcept aConcept, 
                                                        RDFSKOSConcept bConcept) {
            return (aConcept != null && bConcept != null && conceptScheme != null ? conceptScheme.GetExactMatchConceptsOf(aConcept).Concepts.ContainsKey(bConcept.PatternMemberID) : false);
        }

        /// <summary>
        /// Enlists the skos:exactMatch concepts of the given concept within the given scheme
        /// </summary>
        public static RDFSKOSConceptScheme GetExactMatchConceptsOf(this RDFSKOSConceptScheme conceptScheme, 
                                                                   RDFSKOSConcept concept) {
            var result   = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);
            if (concept != null && conceptScheme != null) {
                result   = conceptScheme.GetExactMatchConceptsOfInternal(concept, null)
                                        .RemoveConcept(concept); //Safety deletion
            }
            return result;
        }

        /// <summary>
        /// Subsumes the "skos:exactMatch" taxonomy to discover direct and indirect exactmatches of the given concept
        /// </summary>
        internal static RDFSKOSConceptScheme GetExactMatchConceptsOfInternal(this RDFSKOSConceptScheme conceptScheme, 
                                                                             RDFSKOSConcept concept, 
                                                                             Dictionary<Int64, RDFSKOSConcept> visitContext) {
            var result         = new RDFSKOSConceptScheme((RDFResource)conceptScheme.Value);

            #region visitContext
            if (visitContext  == null) {
                visitContext   = new Dictionary<Int64, RDFSKOSConcept>() { { concept.PatternMemberID, concept } };
            }
            else {
                if (!visitContext.ContainsKey(concept.PatternMemberID)) {
                     visitContext.Add(concept.PatternMemberID, concept);
                }
                else {
                     return result;
                }
            }
            #endregion

            // Transitivity of "skos:exactMatch" taxonomy: 
            //((A SKOS:EXACTMATCH B)  &&  (B SKOS:EXACTMATCH C))  =>  (A SKOS:EXACTMATCH C)
            foreach (var em   in conceptScheme.Relations.ExactMatch.SelectEntriesBySubject(concept)) {
                result.AddConcept((RDFSKOSConcept)em.TaxonomyObject);
                result         = result.UnionWith(conceptScheme.GetExactMatchConceptsOfInternal((RDFSKOSConcept)em.TaxonomyObject, visitContext));
            }

            return result;
        }
        #endregion

        #endregion

        #endregion

    }

}