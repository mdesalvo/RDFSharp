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

namespace RDFSharp.Semantics.DC
{

    /// <summary>
    /// RDFDCOntology represents an OWL-DL ontology implementation of Dublin Core vocabulary
    /// </summary>
    public static class RDFDCOntology
    {

        #region Properties
        /// <summary>
        /// Singleton instance of the DC ontology
        /// </summary>
        public static RDFOntology Instance { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the DC ontology
        /// </summary>
        static RDFDCOntology()
        {

            #region Declarations

            #region Ontology
            Instance = new RDFOntology(new RDFResource(RDFVocabulary.DC.BASE_URI));
            #endregion

            #region Classes

            //DC.DCAM
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToRDFOntologyClass());

            //DC.DCTERMS
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.AGENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.AGENT_CLASS.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.BIBLIOGRAPHIC_RESOURCE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.BOX.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.FILE_FORMAT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.FREQUENCY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.ISO3166.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.ISO639_2.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.ISO639_3.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.JURISDICTION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.LICENSE_DOCUMENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.LINGUISTIC_SYSTEM.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.LOCATION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.LOCATION_PERIOD_OR_JURISDICTION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.MEDIA_TYPE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.MEDIA_TYPE_OR_EXTENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.METHOD_OF_INSTRUCTION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.METHOD_OF_ACCRUAL.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.PERIOD.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.PERIOD_OF_TIME.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.PHYSICAL_MEDIUM.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.PHYSICAL_RESOURCE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.POINT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.POLICY.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.PROVENANCE_STATEMENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.RFC1766.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.RFC3066.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.RFC4646.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.RFC5646.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.RIGHTS_STATEMENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.SIZE_OR_DURATION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.STANDARD.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.URI.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.W3CDTF.ToRDFOntologyClass());

            //DC.DCTYPE
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.COLLECTION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.DATASET.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.EVENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.IMAGE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.INTERACTIVE_RESOURCE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.MOVING_IMAGE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.PHYSICAL_OBJECT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.SERVICE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.SOFTWARE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.SOUND.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.STILL_IMAGE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTYPE.TEXT.ToRDFOntologyClass());

            #endregion

            #region Properties

            //DC
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.CONTRIBUTOR.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.COVERAGE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.CREATOR.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DATE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DESCRIPTION.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.FORMAT.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.IDENTIFIER.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.LANGUAGE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.PUBLISHER.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.RELATION.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.RIGHTS.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.SOURCE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.SUBJECT.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.TITLE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.TYPE.ToRDFOntologyAnnotationProperty());

            //DC.DCAM
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCAM.MEMBER_OF.ToRDFOntologyObjectProperty());

            //DC.DCTERMS
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.ABSTRACT.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.ACCESS_RIGHTS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_METHOD.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_PERIODICITY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_POLICY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.ALTERNATIVE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.AUDIENCE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.AVAILABLE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.BIBLIOGRAPHIC_CITATION.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.CONFORMS_TO.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.CONTRIBUTOR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.COVERAGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.CREATED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.CREATOR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.DATE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.DATE_ACCEPTED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.DATE_COPYRIGHTED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.DATE_SUBMITTED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.DESCRIPTION.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.EDUCATION_LEVEL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.EXTENT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.FORMAT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.HAS_FORMAT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.HAS_PART.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.HAS_VERSION.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.IDENTIFIER.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.ISSUED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.INSTRUCTIONAL_METHOD.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.IS_FORMAT_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.IS_PART_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.IS_REFERENCED_BY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.IS_REPLACED_BY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.IS_REQUIRED_BY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.IS_VERSION_OF.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.LANGUAGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.LICENSE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.MEDIATOR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.MEDIUM.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.MODIFIED.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.PROVENANCE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.PUBLISHER.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.REFERENCES.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.REPLACES.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.REQUIRES.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.RIGHTS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.RIGHTS_HOLDER.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.SOURCE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.SPATIAL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.SUBJECT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.TABLE_OF_CONTENTS.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.TEMPORAL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.TITLE.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.TYPE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.DC.DCTERMS.VALID.ToRDFOntologyDatatypeProperty());

            #endregion

            #region Facts

            //DC.DCTERMS
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.DCMITYPE.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.DDC.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.IMT.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.LCC.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.LCSH.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.MESH.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.NLM.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.TGN.ToRDFOntologyFact());
            Instance.Data.AddFact(RDFVocabulary.DC.DCTERMS.UDC.ToRDFOntologyFact());

            #endregion

            #endregion

            #region Taxonomies

            #region ClassModel

            //SubClassOf
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT_CLASS.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.FILE_FORMAT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.MEDIA_TYPE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.JURISDICTION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LOCATION_PERIOD_OR_JURISDICTION.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LICENSE_DOCUMENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.RIGHTS_STATEMENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LOCATION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LOCATION_PERIOD_OR_JURISDICTION.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.MEDIA_TYPE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.MEDIA_TYPE_OR_EXTENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.PERIOD_OF_TIME.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LOCATION_PERIOD_OR_JURISDICTION.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.PHYSICAL_MEDIUM.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.MEDIA_TYPE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.SIZE_OR_DURATION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.MEDIA_TYPE_OR_EXTENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTYPE.MOVING_IMAGE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTYPE.IMAGE.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTYPE.STILL_IMAGE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTYPE.IMAGE.ToString()));

            #endregion

            #region PropertyModel

            //SubPropertyOf
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ACCESS_RIGHTS.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RIGHTS.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.AVAILABLE.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.BIBLIOGRAPHIC_CITATION.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IDENTIFIER.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.CONFORMS_TO.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.CREATED.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.CREATOR.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.CONTRIBUTOR.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE_ACCEPTED.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE_COPYRIGHTED.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE_SUBMITTED.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.EDUCATION_LEVEL.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.AUDIENCE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.EXTENT.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.FORMAT.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.HAS_FORMAT.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.HAS_PART.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.HAS_VERSION.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ISSUED.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_FORMAT_OF.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_PART_OF.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_REFERENCED_BY.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_REPLACED_BY.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_REQUIRED_BY.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_VERSION_OF.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.LICENSE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RIGHTS.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.MEDIATOR.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.AUDIENCE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.MEDIUM.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.FORMAT.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.MODIFIED.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.REFERENCES.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.REPLACES.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.REQUIRES.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.SOURCE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RELATION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.SPATIAL.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.COVERAGE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.TEMPORAL.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.COVERAGE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.VALID.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.DATE.ToString()));

            //InverseOf
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.HAS_FORMAT.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_FORMAT_OF.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.HAS_PART.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_PART_OF.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.HAS_VERSION.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_VERSION_OF.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.REFERENCES.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_REFERENCED_BY.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.REPLACES.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_REPLACED_BY.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.REQUIRES.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.IS_REQUIRED_BY.ToString()));

            //Domain/Range
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCAM.MEMBER_OF.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ACCESS_RIGHTS.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.RIGHTS_STATEMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_METHOD.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTYPE.COLLECTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_METHOD.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.METHOD_OF_ACCRUAL.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_PERIODICITY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTYPE.COLLECTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_PERIODICITY.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.FREQUENCY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_POLICY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTYPE.COLLECTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.ACCRUAL_POLICY.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.POLICY.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.AUDIENCE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.BIBLIOGRAPHIC_CITATION.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.BIBLIOGRAPHIC_RESOURCE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.CONFORMS_TO.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.STANDARD.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.CONTRIBUTOR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.COVERAGE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LOCATION_PERIOD_OR_JURISDICTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.CREATOR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.EDUCATION_LEVEL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.EXTENT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.SIZE_OR_DURATION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.FORMAT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.MEDIA_TYPE_OR_EXTENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.INSTRUCTIONAL_METHOD.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.METHOD_OF_INSTRUCTION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.LANGUAGE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LINGUISTIC_SYSTEM.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.LICENSE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LICENSE_DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.MEDIATOR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT_CLASS.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.MEDIUM.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.PHYSICAL_RESOURCE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.MEDIUM.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.PHYSICAL_MEDIUM.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.PUBLISHER.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.PROVENANCE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.PROVENANCE_STATEMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RIGHTS.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.RIGHTS_STATEMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.RIGHTS_HOLDER.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.SPATIAL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.LOCATION.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.DC.DCTERMS.TEMPORAL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.PERIOD_OF_TIME.ToString()));

            #endregion

            #region Data

            //ClassType
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.DCMITYPE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.DDC.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.IMT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.LCC.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.LCSH.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.MESH.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.NLM.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.TGN.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));
            Instance.Data.AddClassTypeRelation(Instance.Data.SelectFact(RDFVocabulary.DC.DCTERMS.UDC.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCAM.VOCABULARY_ENCODING_SCHEME.ToString()));

            #endregion

            #endregion

        }
        #endregion

    }

}