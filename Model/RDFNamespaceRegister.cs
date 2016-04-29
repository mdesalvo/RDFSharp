/*
   Copyright 2012-2016 Marco De Salvo

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
using System.Collections;
using System.Collections.Generic;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFNamespaceRegister is a singleton container for registered RDF namespaces.
    /// </summary>
    public sealed class RDFNamespaceRegister: IEnumerable<RDFNamespace> {

        #region Properties
        /// <summary>
        /// Default namespace of the library
        /// </summary>
        public static RDFNamespace DefaultNamespace { get; internal set; }
        
        /// <summary>
        /// Singleton instance of the RDFNamespaceRegister class
        /// </summary>
        internal static RDFNamespaceRegister Instance { get; set; }

        /// <summary>
        /// List of registered namespaces
        /// </summary>
        internal List<RDFNamespace> Register { get; set; }

        /// <summary>
        /// Count of the register's namespaces
        /// </summary>
        public static Int32 NamespacesCount {
            get { return Instance.Register.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the register's namespaces for iteration
        /// </summary>
        public static IEnumerator<RDFNamespace> NamespacesEnumerator {
            get { return Instance.Register.GetEnumerator(); }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the singleton instance of the register
        /// </summary>
        static RDFNamespaceRegister() {
            Instance          = new RDFNamespaceRegister();
            Instance.Register = new List<RDFNamespace>();
            SetDefaultNamespace(new RDFNamespace("rdfsharp", "http://rdfsharp.codeplex.com/default_graph#"));

            #region Basic Namespaces
            //xsd
            AddNamespace(new RDFNamespace(RDFVocabulary.XSD.PREFIX,           RDFVocabulary.XSD.BASE_URI));
            //xml
            AddNamespace(new RDFNamespace(RDFVocabulary.XML.PREFIX,           RDFVocabulary.XML.BASE_URI));
            //rdf
            AddNamespace(new RDFNamespace(RDFVocabulary.RDF.PREFIX,           RDFVocabulary.RDF.BASE_URI));
            //rdfs
            AddNamespace(new RDFNamespace(RDFVocabulary.RDFS.PREFIX,          RDFVocabulary.RDFS.BASE_URI));
            //owl
            AddNamespace(new RDFNamespace(RDFVocabulary.OWL.PREFIX,           RDFVocabulary.OWL.BASE_URI));
            #endregion

            #region Extended Namespaces
            //foaf
            AddNamespace(new RDFNamespace(RDFVocabulary.FOAF.PREFIX,          RDFVocabulary.FOAF.BASE_URI));
            //skos
            AddNamespace(new RDFNamespace(RDFVocabulary.SKOS.PREFIX,          RDFVocabulary.SKOS.BASE_URI));
            //dc (and extensions)
            AddNamespace(new RDFNamespace(RDFVocabulary.DC.PREFIX,            RDFVocabulary.DC.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.DC.DCAM.PREFIX,       RDFVocabulary.DC.DCAM.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.DC.DCTERMS.PREFIX,    RDFVocabulary.DC.DCTERMS.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.DC.DCTYPE.PREFIX,     RDFVocabulary.DC.DCTYPE.BASE_URI));
            //geo
            AddNamespace(new RDFNamespace(RDFVocabulary.GEO.PREFIX,           RDFVocabulary.GEO.BASE_URI));
			//rss
            AddNamespace(new RDFNamespace(RDFVocabulary.RSS.PREFIX,           RDFVocabulary.RSS.BASE_URI));
            //dbpedia
            AddNamespace(new RDFNamespace(RDFVocabulary.DBPEDIA.PREFIX,       RDFVocabulary.DBPEDIA.BASE_URI));
            //og (and extensions)
            AddNamespace(new RDFNamespace(RDFVocabulary.OG.PREFIX,            RDFVocabulary.OG.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_MUSIC.PREFIX,   RDFVocabulary.OG.OG_MUSIC.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_VIDEO.PREFIX,   RDFVocabulary.OG.OG_VIDEO.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_ARTICLE.PREFIX, RDFVocabulary.OG.OG_ARTICLE.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_BOOK.PREFIX,    RDFVocabulary.OG.OG_BOOK.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_PROFILE.PREFIX, RDFVocabulary.OG.OG_PROFILE.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_WEBSITE.PREFIX, RDFVocabulary.OG.OG_WEBSITE.BASE_URI));
            //sioc
            AddNamespace(new RDFNamespace(RDFVocabulary.SIOC.PREFIX,          RDFVocabulary.SIOC.BASE_URI));
            //vs
            AddNamespace(new RDFNamespace(RDFVocabulary.VOCAB_STATUS.PREFIX,  RDFVocabulary.VOCAB_STATUS.BASE_URI));
            #endregion
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the register's namespaces
        /// </summary>
        IEnumerator<RDFNamespace> IEnumerable<RDFNamespace>.GetEnumerator() {
            return Instance.Register.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the register's namespaces
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return Instance.Register.GetEnumerator();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the given namespace as default namespace of the library 
        /// </summary>
        public static void SetDefaultNamespace(RDFNamespace defaultNamespace) {
            if (defaultNamespace != null) {
                DefaultNamespace = defaultNamespace;
                AddNamespace(defaultNamespace);
            }
        }

        /// <summary>
        /// Adds the given namespace to the register, avoiding duplicate insertions
        /// </summary>
        public static void AddNamespace(RDFNamespace nSpace) {
            if (nSpace != null) {
                if (!ContainsNamespace(nSpace)) {
                     Instance.Register.Add(nSpace);
                }
            }
        }

        /// <summary>
        /// Removes the given namespace from the register
        /// </summary>
        public static void RemoveNamespace(RDFNamespace nSpace) {
            //DefaultNamespace can't be removed
            if (nSpace != null && !nSpace.Equals(DefaultNamespace)) {
                Instance.Register.RemoveAll(ns => ns.Prefix.Equals(nSpace.Prefix, StringComparison.Ordinal) 
                                               || ns.Namespace.Equals(nSpace.Namespace));
            }
        }

        /// <summary>
        /// Checks for existence of the given namespace in the register by seeking presence of its prefix or its uri
        /// </summary>
        public static Boolean ContainsNamespace(RDFNamespace nSpace) {
            if (nSpace != null) {
                return Instance.Register.Exists(ns => ns.Prefix.Equals(nSpace.Prefix, StringComparison.Ordinal)
                                                   || ns.Namespace.Equals(nSpace.Namespace));
            }
            return false;
        }

        /// <summary>
        /// Retrieves a namespace from the register by seeking presence of its uri
        /// </summary>
        public static RDFNamespace GetByNamespace(String nSpace) {
            Uri tempNS = RDFModelUtilities.GetUriFromString(nSpace);
            if(tempNS != null){
                return Instance.Register.Find(ns => ns.Namespace.Equals(tempNS));
            }
            return null;
        }

        /// <summary>
        /// Retrieves a namespace from the register by seeking presence of its prefix
        /// </summary>
        public static RDFNamespace GetByPrefix(String prefix) {
            if (prefix != null && prefix.Trim() != String.Empty) {
                return Instance.Register.Find(ns => ns.Prefix.Equals(prefix, StringComparison.Ordinal));
            }
            return null;
        }
        #endregion

    }

}