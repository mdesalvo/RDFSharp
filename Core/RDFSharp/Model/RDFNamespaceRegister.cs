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
    public class RDFNamespaceRegister: IEnumerable<RDFNamespace> {

        #region Properties
        /// <summary>
        /// Default namespace of the library
        /// </summary>
        public static RDFNamespace DefaultNamespace { get; internal set; }
        
        /// <summary>
        /// Singleton instance of the RDFNamespaceRegister class
        /// </summary>
        internal static Lazy<RDFNamespaceRegister> Instance { get; set; }

        /// <summary>
        /// List of registered namespaces
        /// </summary>
        internal List<RDFNamespace> Register { get; set; }

        /// <summary>
        /// Count of the register's namespaces
        /// </summary>
        public static Int32 NamespacesCount {
            get { return RDFNamespaceRegister.Instance.Value.Register.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the register's namespaces for iteration
        /// </summary>
        public static IEnumerator<RDFNamespace> NamespacesEnumerator {
            get { return RDFNamespaceRegister.Instance.Value.Register.GetEnumerator(); }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the singleton instance of the register
        /// </summary>
        static RDFNamespaceRegister() {
            RDFNamespaceRegister.Instance                = new Lazy<RDFNamespaceRegister>();
            RDFNamespaceRegister.Instance.Value.Register = new List<RDFNamespace>();
            RDFNamespaceRegister.SetDefaultNamespace(new RDFNamespace("rdfsharp", "http://rdfsharp.codeplex.com/default_graph#"));

            #region Basic Namespaces
            //xsd
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.XSD.PREFIX,           RDFVocabulary.XSD.BASE_URI));
            //xml
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.XML.PREFIX,           RDFVocabulary.XML.BASE_URI));
            //rdf
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.RDF.PREFIX,           RDFVocabulary.RDF.BASE_URI));
            //rdfs
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.RDFS.PREFIX,          RDFVocabulary.RDFS.BASE_URI));
            //owl
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.OWL.PREFIX,           RDFVocabulary.OWL.BASE_URI));
            #endregion

            #region Extended Namespaces
            //foaf
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.FOAF.PREFIX,          RDFVocabulary.FOAF.BASE_URI));
            //skos
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.SKOS.PREFIX,          RDFVocabulary.SKOS.BASE_URI));
            //dc
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.DC.PREFIX,            RDFVocabulary.DC.BASE_URI));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.DC.DC_TERMS.PREFIX,   RDFVocabulary.DC.DC_TERMS.BASE_URI));
            //geo
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.GEO.PREFIX,           RDFVocabulary.GEO.BASE_URI));
			//rss
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.RSS.PREFIX,           RDFVocabulary.RSS.BASE_URI));
            //dbpedia
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.DBPEDIA.PREFIX,       RDFVocabulary.DBPEDIA.BASE_URI));
            //og
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.OG.PREFIX,            RDFVocabulary.OG.BASE_URI));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_MUSIC.PREFIX,   RDFVocabulary.OG.OG_MUSIC.BASE_URI));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_VIDEO.PREFIX,   RDFVocabulary.OG.OG_VIDEO.BASE_URI));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_ARTICLE.PREFIX, RDFVocabulary.OG.OG_ARTICLE.BASE_URI));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_BOOK.PREFIX,    RDFVocabulary.OG.OG_BOOK.BASE_URI));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_PROFILE.PREFIX, RDFVocabulary.OG.OG_PROFILE.BASE_URI));
            RDFNamespaceRegister.AddNamespace(new RDFNamespace(RDFVocabulary.OG.OG_WEBSITE.PREFIX, RDFVocabulary.OG.OG_WEBSITE.BASE_URI));
            #endregion
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the register's namespaces
        /// </summary>
        IEnumerator<RDFNamespace> IEnumerable<RDFNamespace>.GetEnumerator() {
            return RDFNamespaceRegister.Instance.Value.Register.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the register's namespaces
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return RDFNamespaceRegister.Instance.Value.Register.GetEnumerator();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the given namespace as default namespace of the library 
        /// </summary>
        public static void SetDefaultNamespace(RDFNamespace defaultNamespace) {
            if (defaultNamespace != null) {
                RDFNamespaceRegister.DefaultNamespace = defaultNamespace;
                RDFNamespaceRegister.AddNamespace(defaultNamespace);
            }
        }

        /// <summary>
        /// Adds the given namespace to the register, avoiding duplicate insertions
        /// </summary>
        public static void AddNamespace(RDFNamespace nSpace) {
            if (nSpace != null) {
                if (!RDFNamespaceRegister.ContainsNamespace(nSpace)) {
                    RDFNamespaceRegister.Instance.Value.Register.Add(nSpace);
                }
            }
        }

        /// <summary>
        /// Removes the given namespace from the register
        /// </summary>
        public static void RemoveNamespace(RDFNamespace nSpace) {
            //DefaultNamespace can't be removed
            if (nSpace != null && !nSpace.Equals(RDFNamespaceRegister.DefaultNamespace)) {
                RDFNamespaceRegister.Instance.Value.Register.RemoveAll(ns => 
				    (ns.Prefix.Equals(nSpace.Prefix, StringComparison.Ordinal)) || ns.Namespace.Equals(nSpace.Namespace));
            }
        }

        /// <summary>
        /// Checks for existence of the given namespace in the register by seeking presence of its prefix or its uri
        /// </summary>
        public static Boolean ContainsNamespace(RDFNamespace nSpace) {
            if (nSpace != null) {
                return RDFNamespaceRegister.Instance.Value.Register.Exists(ns => 
				    (ns.Prefix.Equals(nSpace.Prefix, StringComparison.Ordinal)) || ns.Namespace.Equals(nSpace.Namespace));
            }
            return false;
        }

        /// <summary>
        /// Retrieves a namespace from the register by seeking presence of its uri
        /// </summary>
        public static RDFNamespace GetByNamespace(String nSpace) {
            Uri tempNS = RDFModelUtilities.GetUriFromString(nSpace);
            if(tempNS != null){
                return RDFNamespaceRegister.Instance.Value.Register.Find(ns => ns.Namespace.Equals(tempNS));
            }
            return null;
        }

        /// <summary>
        /// Retrieves a namespace from the register by seeking presence of its prefix
        /// </summary>
        public static RDFNamespace GetByPrefix(String prefix) {
            if (prefix != null && prefix.Trim() != String.Empty) {
                return RDFNamespaceRegister.Instance.Value.Register.Find(ns => ns.Prefix.Equals(prefix, StringComparison.Ordinal));
            }
            return null;
        }
        #endregion

    }

}