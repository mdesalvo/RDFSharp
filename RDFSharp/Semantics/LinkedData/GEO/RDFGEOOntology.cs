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

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFGEOOntology represents an OWL-DL ontology implementation of W3C GEO vocabulary
    /// </summary>
    public static class RDFGEOOntology
    {

        #region Properties
        /// <summary>
        /// Singleton instance of the GEO ontology
        /// </summary>
        public static RDFOntology Instance { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the GEO ontology
        /// </summary>
        static RDFGEOOntology()
        {

            #region Declarations

            #region Ontology
            Instance = new RDFOntology(new RDFResource(RDFVocabulary.GEO.BASE_URI));
            #endregion

            #region Classes
            Instance.Model.ClassModel.AddClass(RDFVocabulary.GEO.SPATIAL_THING.ToRDFOntologyClass());
            Instance.Model.ClassModel.AddClass(RDFVocabulary.GEO.POINT.ToRDFOntologyClass());
            #endregion

            #region Properties
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.GEO.ALT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.GEO.LAT.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.GEO.LONG.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.GEO.LAT_LONG.ToRDFOntologyDatatypeProperty());
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.GEO.LOCATION.ToRDFOntologyObjectProperty());

            //OWL-DL Completeness
            Instance.Model.PropertyModel.AddProperty(RDFVocabulary.FOAF.BASED_NEAR.ToRDFOntologyObjectProperty());
            #endregion

            #endregion

            #region Taxonomies

            #region ClassModel

            //SubClassOf
            Instance.Model.ClassModel.AddSubClassOfRelation(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.POINT.ToString()), Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));

            #endregion

            #region PropertyModel

            //SubPropertyOf
            Instance.Model.PropertyModel.AddSubPropertyOfRelation((RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.LOCATION.ToString()), (RDFOntologyObjectProperty)Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.BASED_NEAR.ToString()));

            //Domain/Range
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.ALT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.ALT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.FLOAT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.LAT.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.LAT.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.FLOAT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.LONG.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.LONG.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.FLOAT.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.LAT_LONG.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.LAT_LONG.ToString()).SetRange(RDFBASEOntology.Instance.Model.ClassModel.SelectClass(RDFVocabulary.XSD.STRING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.GEO.LOCATION.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));

            //Domain/Range
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.BASED_NEAR.ToString()).SetDomain(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            Instance.Model.PropertyModel.SelectProperty(RDFVocabulary.FOAF.BASED_NEAR.ToString()).SetRange(Instance.Model.ClassModel.SelectClass(RDFVocabulary.GEO.SPATIAL_THING.ToString()));
            #endregion

            #endregion

        }
        #endregion

    }

}