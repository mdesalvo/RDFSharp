/*
   Copyright 2012-2017 Marco De Salvo

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
using System.Net;

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
        public static RDFNamespaceRegister Instance { get; internal set; }

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
            SetDefaultNamespace(new RDFNamespace("rdfsharp", "https://rdfsharp.codeplex.com/"));

            #region Basic Namespaces
            //xsd
            AddNamespace(new RDFNamespace(RDFVocabulary.XSD.PREFIX,         RDFVocabulary.XSD.BASE_URI));
            //xml
            AddNamespace(new RDFNamespace(RDFVocabulary.XML.PREFIX,         RDFVocabulary.XML.BASE_URI));
            //rdf
            AddNamespace(new RDFNamespace(RDFVocabulary.RDF.PREFIX,         RDFVocabulary.RDF.BASE_URI));
            //rdfs
            AddNamespace(new RDFNamespace(RDFVocabulary.RDFS.PREFIX,        RDFVocabulary.RDFS.BASE_URI));
            //owl
            AddNamespace(new RDFNamespace(RDFVocabulary.OWL.PREFIX,         RDFVocabulary.OWL.BASE_URI));
            #endregion

            #region Extended Namespaces
            //dc
            AddNamespace(new RDFNamespace(RDFVocabulary.DC.PREFIX,          RDFVocabulary.DC.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.DC.DCAM.PREFIX,     RDFVocabulary.DC.DCAM.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.DC.DCTERMS.PREFIX,  RDFVocabulary.DC.DCTERMS.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.DC.DCTYPE.PREFIX,   RDFVocabulary.DC.DCTYPE.BASE_URI));
            //foaf
            AddNamespace(new RDFNamespace(RDFVocabulary.FOAF.PREFIX,        RDFVocabulary.FOAF.BASE_URI));
            //geo
            AddNamespace(new RDFNamespace(RDFVocabulary.GEO.PREFIX,         RDFVocabulary.GEO.BASE_URI));
            //skos
            AddNamespace(new RDFNamespace(RDFVocabulary.SKOS.PREFIX,        RDFVocabulary.SKOS.BASE_URI));
            AddNamespace(new RDFNamespace(RDFVocabulary.SKOS.SKOSXL.PREFIX, RDFVocabulary.SKOS.SKOSXL.BASE_URI));
            //sioc
            AddNamespace(new RDFNamespace(RDFVocabulary.SIOC.PREFIX,        RDFVocabulary.SIOC.BASE_URI));
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
        /// Sets the given namespace as default namespace of the library.
        /// </summary>
        public static void SetDefaultNamespace(RDFNamespace nSpace) {
            if (nSpace          != null) {
                DefaultNamespace = nSpace;

                //Also add it to the register
                AddNamespace(nSpace);
            }
        }

        /// <summary>
        /// Adds the given namespace to the register, if it has unique prefix and uri.
        /// </summary>
        public static void AddNamespace(RDFNamespace nSpace) {
            if (nSpace != null) {
                if (GetByPrefix(nSpace.NamespacePrefix) == null && GetByUri(nSpace.NamespaceUri.ToString()) == null) {
                    Instance.Register.Add(nSpace);
                }
            }
        }

        /// <summary>
        /// Removes the namespace having the given Uri.
        /// </summary>
        public static void RemoveByUri(String uri) {
            if (uri != null) {
                Instance.Register.RemoveAll(ns => ns.NamespaceUri.ToString().Equals(uri.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Removes the namespace having the given prefix.
        /// </summary>
        public static void RemoveByPrefix(String prefix) {
            if (prefix != null) {
                Instance.Register.RemoveAll(ns => ns.NamespacePrefix.Equals(prefix.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Retrieves a namespace by seeking presence of its Uri.
        /// </summary>
        public static RDFNamespace GetByUri(String uri, Boolean enablePrefixCCService=false) {
            if (uri        != null) {
                var result  = Instance.Register.Find(ns => ns.NamespaceUri.ToString().Equals(uri.Trim(), StringComparison.OrdinalIgnoreCase));
                if (result == null && enablePrefixCCService) {
                    result  = LookupPrefixCC(uri.Trim().TrimEnd(new Char[] { '#' }), 2);
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Retrieves a namespace by seeking presence of its prefix.
        /// </summary>
        public static RDFNamespace GetByPrefix(String prefix, Boolean enablePrefixCCService=false) {
            if (prefix     != null) {
                var result  = Instance.Register.Find(ns => ns.NamespacePrefix.Equals(prefix.Trim(), StringComparison.OrdinalIgnoreCase));
                if (result == null && enablePrefixCCService) {
                    result  = LookupPrefixCC(prefix.Trim(), 1);
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Looksup the given prefix or namespace into the prefix.cc service
        /// </summary>
        internal static RDFNamespace LookupPrefixCC(String data, Int32 lookupMode) {
            var lookupString     = (lookupMode == 1 ? "http://prefix.cc/" + data + ".file.txt" :
                                                      "http://prefix.cc/reverse?uri=" + data + "&format=txt");

            using (var webclient = new WebClient()) {
                try {
                    var response = webclient.DownloadString(lookupString);
                    var prefix   = response.Split('\t')[0];
                    var nspace   = response.Split('\t')[1].TrimEnd(new Char[] { '\n' });
                    var result   = new RDFNamespace(prefix, nspace);

                    //Also add the namespace to the register, to avoid future lookups
                    AddNamespace(result);

                    //Return the found result
                    return result;
                }
                catch  (WebException wex) {
                    if (wex.Message.Contains("404")) {
                        return null;
                    }
                    else {
                        throw new RDFModelException("Cannot retrieve data from prefix.cc service because: " + wex.Message, wex);
                    }
                }
                catch(Exception ex) {
                    throw new RDFModelException("Cannot retrieve data from prefix.cc service because: " + ex.Message, ex);
                }
            }
        }
        #endregion

    }

}