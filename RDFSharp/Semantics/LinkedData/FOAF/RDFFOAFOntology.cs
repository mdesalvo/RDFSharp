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
using System.Collections.Generic;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFFOAFOntology represents an OWL-DL ontology implementation of FOAF vocabulary
    /// </summary>
    public static class RDFFOAFOntology
    {

        #region Properties
        /// <summary>
        /// Singleton instance of the FOAF ontology
        /// </summary>
        public static RDFOntology Instance { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the FOAF ontology
        /// </summary>
        static RDFFOAFOntology()
        {

            #region Declarations

            #region Ontology
            Instance = new RDFOntology(new RDFResource(RDFVocabulary.FOAF.BASE_URI));
            #endregion

            #region Classes
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.AGENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.DOCUMENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.GROUP.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.IMAGE.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.ONLINE_ACCOUNT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.ONLINE_CHAT_ACCOUNT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.ONLINE_ECOMMERCE_ACCOUNT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.ONLINE_GAMING_ACCOUNT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.ORGANIZATION.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.PERSON.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.PERSONAL_PROFILE_DOCUMENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.FOAF.PROJECT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(new RDFOntologyDataRangeClass(new RDFResource("bnode:Genders")));

            //OWL-DL Completeness
            Instance.Model.ClassModel.AddClass(RDFVocabulary.DC.DCTERMS.AGENT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.GEO.SPATIAL_THING.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.SKOS.CONCEPT.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(new RDFOntologyClass(new RDFResource("http://schema.org/CreativeWork")));
            Instance.Model.ClassModel.AddClass(new RDFOntologyClass(new RDFResource("http://schema.org/ImageObject")));
            Instance.Model.ClassModel.AddClass(new RDFOntologyClass(new RDFResource("http://schema.org/Person")));
            Instance.Model.ClassModel.AddClass(new RDFOntologyClass(new RDFResource("http://www.w3.org/2000/10/swap/pim/contact#Person")));
            #endregion

            #region Properties
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.ACCOUNT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.ACCOUNT_NAME.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.ACCOUNT_SERVICE_HOMEPAGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.AGE.ToRDFOntologyDatatypeProperty().SetFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.AIM_CHAT_ID.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.BASED_NEAR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.BIRTHDAY.ToRDFOntologyDatatypeProperty().SetFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.CURRENT_PROJECT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.DEPICTION.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.DEPICTS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.DNA_CHECKSUM.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.FAMILY_NAME.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.FIRSTNAME.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.FOCUS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.FUNDED_BY.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.GEEK_CODE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.GENDER.ToRDFOntologyDatatypeProperty().SetFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.GIVEN_NAME.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.HOLDS_ACCOUNT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.HOMEPAGE.ToRDFOntologyObjectProperty().SetInverseFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.ICQ_CHAT_ID.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.IMG.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.INTEREST.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.IS_PRIMARY_TOPIC_OF.ToRDFOntologyObjectProperty().SetInverseFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.JABBER_ID.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.KNOWS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.LOGO.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.MADE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.MAKER.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.MBOX.ToRDFOntologyObjectProperty().SetInverseFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.MBOX_SHA1SUM.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.MEMBER.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.MEMBERSHIP_CLASS.ToRDFOntologyAnnotationProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.MSN_CHAT_ID.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.MYERS_BRIGGS.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.NAME.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.NICK.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.OPEN_ID.ToRDFOntologyObjectProperty().SetInverseFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.PAGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.PAST_PROJECT.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.PHONE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.PLAN.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.PRIMARY_TOPIC.ToRDFOntologyObjectProperty().SetFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.PUBLICATIONS.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.SCHOOL_HOMEPAGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.SHA1.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.SKYPE_ID.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.STATUS.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.SURNAME.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.THEME.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.THUMBNAIL.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.TIPJAR.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.TITLE.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.TOPIC.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.TOPIC_INTEREST.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.WEBLOG.ToRDFOntologyObjectProperty().SetInverseFunctional(true));
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.WORKINFO_HOMEPAGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.WORKPLACE_HOMEPAGE.ToRDFOntologyObjectProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.YAHOO_CHAT_ID.ToRDFOntologyDatatypeProperty());
            #endregion

            #endregion

            #region Taxonomies

            #region ClassModel

            //SubClassOf
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.IMAGE.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.GROUP.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_CHAT_ACCOUNT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_ACCOUNT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_ECOMMERCE_ACCOUNT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_ACCOUNT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_GAMING_ACCOUNT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_ACCOUNT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ORGANIZATION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSONAL_PROFILE_DOCUMENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));

            //EquivalentClass
            Instance.Model.ClassModel.AddEquivalentClassRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.DC.DCTERMS.AGENT.ToString()));
            Instance.Model.ClassModel.AddEquivalentClassRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()), Instance.Model.ClassModel.SelectClass("http://schema.org/CreativeWork"));
            Instance.Model.ClassModel.AddEquivalentClassRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.IMAGE.ToString()), Instance.Model.ClassModel.SelectClass("http://schema.org/ImageObject"));
            Instance.Model.ClassModel.AddEquivalentClassRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()), Instance.Model.ClassModel.SelectClass("http://schema.org/Person"));
            Instance.Model.ClassModel.AddEquivalentClassRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()), Instance.Model.ClassModel.SelectClass("http://www.w3.org/2000/10/swap/pim/contact#Person"));

            //DisjointWith
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ORGANIZATION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ORGANIZATION.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PROJECT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.ClassModel.AddDisjointWithRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PROJECT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));

            //OneOf
            Instance.Model.ClassModel.AddOneOfRelation(
                (RDFOntologyDataRangeClass)Instance.Model.ClassModel.SelectClass("bnode:Genders"), 
                new List<RDFOntologyLiteral>() {
                    new RDFOntologyLiteral(new RDFTypedLiteral("female", RDFModelEnums.RDFDatatypes.XSD_STRING)),
                    new RDFOntologyLiteral(new RDFTypedLiteral("male", RDFModelEnums.RDFDatatypes.XSD_STRING))
                }
            );

            #endregion

            #region PropertyModel

            //SubPropertyOf
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.AIM_CHAT_ID.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.NICK.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.HOMEPAGE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PAGE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.HOMEPAGE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.IS_PRIMARY_TOPIC_OF.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.ICQ_CHAT_ID.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.NICK.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.IMG.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.DEPICTION.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.IS_PRIMARY_TOPIC_OF.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PAGE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MSN_CHAT_ID.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.NICK.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.OPEN_ID.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.IS_PRIMARY_TOPIC_OF.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.TIPJAR.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PAGE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.WEBLOG.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PAGE.ToString()));
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.YAHOO_CHAT_ID.ToString()), (RDFOntologyDatatypeProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.NICK.ToString()));

            //InverseOf
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.DEPICTION.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.DEPICTS.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PRIMARY_TOPIC.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.IS_PRIMARY_TOPIC_OF.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MADE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MAKER.ToString()));
            Instance.Model.PropertyModel.AddInverseOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PAGE.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.TOPIC.ToString()));

            //Domain/Range
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.ACCOUNT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.ACCOUNT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_ACCOUNT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.ACCOUNT_NAME.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_ACCOUNT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.AGE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.AGE.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.ACCOUNT_SERVICE_HOMEPAGE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_ACCOUNT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.ACCOUNT_SERVICE_HOMEPAGE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.AIM_CHAT_ID.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.BASED_NEAR.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.BASED_NEAR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.BIRTHDAY.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.BIRTHDAY.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.DATE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.CURRENT_PROJECT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.DEPICTION.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.IMAGE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.DEPICTS.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.IMAGE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.FAMILY_NAME.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.FIRSTNAME.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.FOCUS.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.SKOS.CONCEPT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.GEEK_CODE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.GENDER.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.GENDER.ToString()).SetRange(Instance.Model.ClassModel.SelectClass("bnode:Genders"));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.HOLDS_ACCOUNT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.HOLDS_ACCOUNT.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.ONLINE_ACCOUNT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.HOMEPAGE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.ICQ_CHAT_ID.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.IMG.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.IMG.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.IMAGE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.INTEREST.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.INTEREST.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.IS_PRIMARY_TOPIC_OF.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.JABBER_ID.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.KNOWS.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.KNOWS.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MBOX.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MBOX_SHA1SUM.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MADE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MAKER.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MEMBER.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.GROUP.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MEMBER.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MSN_CHAT_ID.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.MYERS_BRIGGS.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.OPEN_ID.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.OPEN_ID.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PAGE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PAST_PROJECT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PLAN.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PRIMARY_TOPIC.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PUBLICATIONS.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.PUBLICATIONS.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.SCHOOL_HOMEPAGE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.SCHOOL_HOMEPAGE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.SKYPE_ID.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.SHA1.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.STATUS.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.SURNAME.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.THUMBNAIL.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.IMAGE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.THUMBNAIL.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.IMAGE.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.TIPJAR.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.TIPJAR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.TOPIC.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.TOPIC_INTEREST.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.WEBLOG.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.WEBLOG.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.WORKINFO_HOMEPAGE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.WORKINFO_HOMEPAGE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.WORKPLACE_HOMEPAGE.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.PERSON.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.WORKPLACE_HOMEPAGE.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.DOCUMENT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.YAHOO_CHAT_ID.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.FOAF.AGENT.ToString()));

            #endregion

            #endregion

        }
        #endregion

    }

}