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
using System;
using System.Linq;

namespace RDFSharp.Semantics.SKOS
{

    /// <summary>
    /// RDFSKOSChecker is responsible for implicit SKOS validation of ontologies during modeling
    /// </summary>
    internal static class RDFSKOSChecker
    {

        #region Methods
        /// <summary>
        /// Checks if the skos:broader/skos:broaderTransitive/skos:broadMatch relation can be added to the given aConcept with the given bConcept
        /// </summary>
        internal static bool CheckBroaderRelation(RDFSKOSConceptScheme conceptScheme,
                                                     RDFSKOSConcept aConcept,
                                                     RDFSKOSConcept bConcept)
        {
            var canAddBroaderRel = false;

            //Avoid clash with hierarchical relations
            canAddBroaderRel = !conceptScheme.CheckHasNarrowerConcept(aConcept, bConcept);

            //Avoid clash with associative relations
            if (canAddBroaderRel)
            {
                canAddBroaderRel = !conceptScheme.CheckHasRelatedConcept(aConcept, bConcept);
            }

            //Avoid clash with mapping relations
            if (canAddBroaderRel)
            {
                canAddBroaderRel = (!conceptScheme.CheckHasNarrowMatchConcept(aConcept, bConcept) &&
                                    !conceptScheme.CheckHasCloseMatchConcept(aConcept, bConcept) &&
                                    !conceptScheme.CheckHasExactMatchConcept(aConcept, bConcept) &&
                                    !conceptScheme.CheckHasRelatedMatchConcept(aConcept, bConcept));
            }

            return canAddBroaderRel;
        }

        /// <summary>
        /// Checks if the skos:narrower/skos:narrowerTransitive/skos:narrowMatch relation can be added to the given aConcept with the given bConcept
        /// </summary>
        internal static bool CheckNarrowerRelation(RDFSKOSConceptScheme conceptScheme,
                                                      RDFSKOSConcept aConcept,
                                                      RDFSKOSConcept bConcept)
        {
            var canAddNarrowerRel = false;

            //Avoid clash with hierarchical relations
            canAddNarrowerRel = !conceptScheme.CheckHasBroaderConcept(aConcept, bConcept);

            //Avoid clash with associative relations
            if (canAddNarrowerRel)
            {
                canAddNarrowerRel = !conceptScheme.CheckHasRelatedConcept(aConcept, bConcept);
            }

            //Avoid clash with mapping relations
            if (canAddNarrowerRel)
            {
                canAddNarrowerRel = (!conceptScheme.CheckHasBroadMatchConcept(aConcept, bConcept) &&
                                     !conceptScheme.CheckHasCloseMatchConcept(aConcept, bConcept) &&
                                     !conceptScheme.CheckHasExactMatchConcept(aConcept, bConcept) &&
                                     !conceptScheme.CheckHasRelatedMatchConcept(aConcept, bConcept));
            }

            return canAddNarrowerRel;
        }

        /// <summary>
        /// Checks if the skos:related/skos:relatedMatch relation can be added to the given aConcept with the given bConcept
        /// </summary>
        internal static bool CheckRelatedRelation(RDFSKOSConceptScheme conceptScheme,
                                                     RDFSKOSConcept aConcept,
                                                     RDFSKOSConcept bConcept)
        {
            var canAddRelatedRel = false;

            //Avoid clash with hierarchical relations
            canAddRelatedRel = (!conceptScheme.CheckHasBroaderConcept(aConcept, bConcept) &&
                                    !conceptScheme.CheckHasNarrowerConcept(aConcept, bConcept));

            //Avoid clash with mapping relations
            if (canAddRelatedRel)
            {
                canAddRelatedRel = (!conceptScheme.CheckHasBroadMatchConcept(aConcept, bConcept) &&
                                    !conceptScheme.CheckHasNarrowMatchConcept(aConcept, bConcept) &&
                                    !conceptScheme.CheckHasCloseMatchConcept(aConcept, bConcept) &&
                                    !conceptScheme.CheckHasExactMatchConcept(aConcept, bConcept));
            }

            return canAddRelatedRel;
        }

        /// <summary>
        /// Checks if the skos:closeMatch/skos:exactMatch relation can be added to the given aConcept with the given bConcept
        /// </summary>
        internal static bool CheckCloseOrExactMatchRelation(RDFSKOSConceptScheme conceptScheme,
                                                               RDFSKOSConcept aConcept,
                                                               RDFSKOSConcept bConcept)
        {
            var canAddCloseOrExactMatchRel = false;

            //Avoid clash with hierarchical relations
            canAddCloseOrExactMatchRel = (!conceptScheme.CheckHasBroaderConcept(aConcept, bConcept) &&
                                              !conceptScheme.CheckHasNarrowerConcept(aConcept, bConcept));

            //Avoid clash with mapping relations
            if (canAddCloseOrExactMatchRel)
            {
                canAddCloseOrExactMatchRel = (!conceptScheme.CheckHasBroadMatchConcept(aConcept, bConcept) &&
                                              !conceptScheme.CheckHasNarrowMatchConcept(aConcept, bConcept) &&
                                              !conceptScheme.CheckHasRelatedMatchConcept(aConcept, bConcept));
            }

            return canAddCloseOrExactMatchRel;
        }

        /// <summary>
        /// Checks if the skos:prefLabel/skosxl:prefLabel informations can be added to the given concept
        /// </summary>
        internal static bool CheckPrefLabel(RDFSKOSConceptScheme conceptScheme,
                                               RDFOntologyFact concept,
                                               RDFOntologyLiteral literal)
        {
            var canAddPrefLabelInfo = false;
            var prefLabelLiteralLang = ((RDFPlainLiteral)literal.Value).Language;

            //Plain literal without language tag: only one occurrence of this information is allowed
            if (string.IsNullOrEmpty(prefLabelLiteralLang))
            {

                //Check skos:prefLabel annotation
                canAddPrefLabelInfo = !(conceptScheme.Annotations.PrefLabel.SelectEntriesBySubject(concept)
                                                                               .Any(x => x.TaxonomyObject.Value is RDFPlainLiteral
                                                                                           && string.IsNullOrEmpty(((RDFPlainLiteral)x.TaxonomyObject.Value).Language)));
                //Check skosxl:prefLabel relation
                if (canAddPrefLabelInfo)
                {
                    canAddPrefLabelInfo = !(conceptScheme.Relations.PrefLabel.SelectEntriesBySubject(concept)
                                                                             .Any(x => conceptScheme.Relations.LiteralForm.SelectEntriesBySubject(x.TaxonomyObject)
                                                                                                                          .Any(y => y.TaxonomyObject.Value is RDFPlainLiteral
                                                                                                                                      && string.IsNullOrEmpty(((RDFPlainLiteral)y.TaxonomyObject.Value).Language))));
                }

            }

            //Plain literal with language tag: only one occurrence of this information per language tag is allowed
            else
            {

                //Check skos:prefLabel annotation
                canAddPrefLabelInfo = !(conceptScheme.Annotations.PrefLabel.SelectEntriesBySubject(concept)
                                                                               .Any(x => x.TaxonomyObject.Value is RDFPlainLiteral
                                                                                           && !string.IsNullOrEmpty(((RDFPlainLiteral)x.TaxonomyObject.Value).Language)
                                                                                           && (((RDFPlainLiteral)x.TaxonomyObject.Value).Language).Equals(prefLabelLiteralLang, StringComparison.OrdinalIgnoreCase)));

                //Check skosxl:prefLabel relation
                if (canAddPrefLabelInfo)
                {
                    canAddPrefLabelInfo = !(conceptScheme.Relations.PrefLabel.SelectEntriesBySubject(concept)
                                                                             .Any(x => conceptScheme.Relations.LiteralForm.SelectEntriesBySubject(x.TaxonomyObject)
                                                                                                                          .Any(y => y.TaxonomyObject.Value is RDFPlainLiteral
                                                                                                                                      && !string.IsNullOrEmpty(((RDFPlainLiteral)x.TaxonomyObject.Value).Language)
                                                                                                                                      && (((RDFPlainLiteral)x.TaxonomyObject.Value).Language).Equals(prefLabelLiteralLang, StringComparison.OrdinalIgnoreCase))));
                }

            }

            //Pairwise disjointness with skos:altLabel/skosxl:altLabel must be preserved
            if (canAddPrefLabelInfo)
            {

                //Check skos:altLabel annotation
                canAddPrefLabelInfo = !(conceptScheme.Annotations.AltLabel.SelectEntriesBySubject(concept)
                                                                              .Any(x => x.TaxonomyObject.Equals(literal)));

                //Check skosxl:altLabel relation
                if (canAddPrefLabelInfo)
                {
                    canAddPrefLabelInfo = !(conceptScheme.Relations.AltLabel.SelectEntriesBySubject(concept)
                                                                            .Any(x => conceptScheme.Relations.LiteralForm.SelectEntriesBySubject(x.TaxonomyObject)
                                                                                                                         .Any(y => y.TaxonomyObject.Equals(literal))));
                }

            }

            //Pairwise disjointness with skos:hiddenLabel/skosxl:hiddenLabel must be preserved
            if (canAddPrefLabelInfo)
            {

                //Check skos:hiddenLabel annotation
                canAddPrefLabelInfo = !(conceptScheme.Annotations.HiddenLabel.SelectEntriesBySubject(concept)
                                                                                 .Any(x => x.TaxonomyObject.Equals(literal)));

                //Check skosxl:hiddenLabel assertion
                if (canAddPrefLabelInfo)
                {
                    canAddPrefLabelInfo = !(conceptScheme.Relations.HiddenLabel.SelectEntriesBySubject(concept)
                                                                               .Any(x => conceptScheme.Relations.LiteralForm.SelectEntriesBySubject(x.TaxonomyObject)
                                                                                                                            .Any(y => y.TaxonomyObject.Equals(literal))));
                }

            }

            return canAddPrefLabelInfo;
        }

        /// <summary>
        /// Checks if the skos:altLabel/skosxl:altLabel informations can be added to the given concept
        /// </summary>
        internal static bool CheckAltLabel(RDFSKOSConceptScheme conceptScheme,
                                              RDFOntologyFact concept,
                                              RDFOntologyLiteral literal)
        {
            var canAddAltLabelInfo = false;

            //Pairwise disjointness with skos:prefLabel/skosxl:prefLabel must be preserved
            canAddAltLabelInfo = !(conceptScheme.Annotations.PrefLabel.SelectEntriesBySubject(concept)
                                                                            .Any(x => x.TaxonomyObject.Equals(literal)));

            if (canAddAltLabelInfo)
            {
                canAddAltLabelInfo = !(conceptScheme.Relations.PrefLabel.SelectEntriesBySubject(concept)
                                                                          .Any(x => conceptScheme.Relations.LiteralForm.SelectEntriesBySubject(x.TaxonomyObject)
                                                                                                                       .Any(y => y.TaxonomyObject.Equals(literal))));
            }

            //Pairwise disjointness with skos:hiddenLabel/skosxl:hiddenLabel must be preserved
            if (canAddAltLabelInfo)
            {
                canAddAltLabelInfo = !(conceptScheme.Annotations.HiddenLabel.SelectEntriesBySubject(concept)
                                                                              .Any(x => x.TaxonomyObject.Equals(literal)));
            }

            if (canAddAltLabelInfo)
            {
                canAddAltLabelInfo = !(conceptScheme.Relations.HiddenLabel.SelectEntriesBySubject(concept)
                                                                            .Any(x => conceptScheme.Relations.HiddenLabel.SelectEntriesBySubject(x.TaxonomyObject)
                                                                                                                         .Any(y => y.TaxonomyObject.Equals(literal))));
            }

            return canAddAltLabelInfo;
        }

        /// <summary>
        /// Checks if the skos:hiddenLabel/skosxl:hiddenLabel informations can be added to the given concept
        /// </summary>
        internal static bool CheckHiddenLabel(RDFSKOSConceptScheme conceptScheme,
                                                 RDFOntologyFact concept,
                                                 RDFOntologyLiteral literal)
        {
            var canAddHiddenLabelInfo = false;

            //Pairwise disjointness with skos:prefLabel/skosxl:prefLabel must be preserved
            canAddHiddenLabelInfo = !(conceptScheme.Annotations.PrefLabel.SelectEntriesBySubject(concept)
                                                                             .Any(x => x.TaxonomyObject.Equals(literal)));

            if (canAddHiddenLabelInfo)
            {
                canAddHiddenLabelInfo = !(conceptScheme.Relations.PrefLabel.SelectEntriesBySubject(concept)
                                                                           .Any(x => conceptScheme.Relations.LiteralForm.SelectEntriesBySubject(x.TaxonomyObject)
                                                                                                                        .Any(y => y.TaxonomyObject.Equals(literal))));
            }

            //Pairwise disjointness with skos:altLabel/skosxl:altLabel must be preserved
            if (canAddHiddenLabelInfo)
            {
                canAddHiddenLabelInfo = !(conceptScheme.Annotations.AltLabel.SelectEntriesBySubject(concept)
                                                                            .Any(x => x.TaxonomyObject.Equals(literal)));
            }

            if (canAddHiddenLabelInfo)
            {
                canAddHiddenLabelInfo = !(conceptScheme.Relations.AltLabel.SelectEntriesBySubject(concept)
                                                                          .Any(x => conceptScheme.Relations.LiteralForm.SelectEntriesBySubject(x.TaxonomyObject)
                                                                                                                       .Any(y => y.TaxonomyObject.Equals(literal))));
            }

            return canAddHiddenLabelInfo;
        }
        #endregion

    }

}