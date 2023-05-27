/*
   Copyright 2012-2023 Marco De Salvo

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
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFNamespaceRegister is a singleton in-memory container for registered RDF namespaces.
    /// </summary>
    public sealed class RDFNamespaceRegister : IEnumerable<RDFNamespace>
    {
        #region Properties
        /// <summary>
        /// Default namespace of the library (rdfsharp)
        /// </summary>
        private static readonly RDFNamespace RDFSharpNS = new RDFNamespace(RDFVocabulary.RDFSHARP.PREFIX, RDFVocabulary.RDFSHARP.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.RDFSHARP.DEREFERENCE_URI));

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
        public static int NamespacesCount
            => Instance.Register.Count;

        /// <summary>
        /// Gets the enumerator on the register's namespaces for iteration
        /// </summary>
        public static IEnumerator<RDFNamespace> NamespacesEnumerator
            => Instance.Register.GetEnumerator();
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to initialize the singleton instance of the register
        /// </summary>
        static RDFNamespaceRegister()
        {
            Instance = new RDFNamespaceRegister()
            {
                Register = new List<RDFNamespace>()
                {
                    RDFSharpNS,

                    //Basic
                    new RDFNamespace(RDFVocabulary.RDF.PREFIX, RDFVocabulary.RDF.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.RDF.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.RDFS.PREFIX, RDFVocabulary.RDFS.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.RDFS.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.XSD.PREFIX, RDFVocabulary.XSD.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.XSD.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.OWL.PREFIX, RDFVocabulary.OWL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.OWL.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.SHACL.PREFIX, RDFVocabulary.SHACL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SHACL.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.SWRL.PREFIX, RDFVocabulary.SWRL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SWRL.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.SWRL.SWRLB.PREFIX, RDFVocabulary.SWRL.SWRLB.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SWRL.SWRLB.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.XML.PREFIX, RDFVocabulary.XML.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.XML.DEREFERENCE_URI)),

                    //Extended
                    new RDFNamespace(RDFVocabulary.DC.PREFIX, RDFVocabulary.DC.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.DC.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.DC.DCAM.PREFIX, RDFVocabulary.DC.DCAM.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.DC.DCAM.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.DC.DCTERMS.PREFIX, RDFVocabulary.DC.DCTERMS.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.DC.DCTERMS.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.DC.DCTYPE.PREFIX, RDFVocabulary.DC.DCTYPE.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.DC.DCTYPE.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.FOAF.PREFIX, RDFVocabulary.FOAF.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.FOAF.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.GEO.PREFIX, RDFVocabulary.GEO.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.GEO.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.GEOSPARQL.PREFIX, RDFVocabulary.GEOSPARQL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.GEOSPARQL.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.GEOSPARQL.SF.PREFIX, RDFVocabulary.GEOSPARQL.SF.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.GEOSPARQL.SF.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.GEOSPARQL.GEOF.PREFIX, RDFVocabulary.GEOSPARQL.GEOF.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.GEOSPARQL.GEOF.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.SKOS.PREFIX, RDFVocabulary.SKOS.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SKOS.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.SKOS.SKOSXL.PREFIX, RDFVocabulary.SKOS.SKOSXL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SKOS.SKOSXL.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.TIME.PREFIX, RDFVocabulary.TIME.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.TIME.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.TIME.GREG.PREFIX, RDFVocabulary.TIME.GREG.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.TIME.GREG.DEREFERENCE_URI)),
                    new RDFNamespace(RDFVocabulary.TIME.THORS.PREFIX, RDFVocabulary.TIME.THORS.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.TIME.THORS.DEREFERENCE_URI))
                }
            };

            DefaultNamespace = RDFSharpNS;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the register's namespaces
        /// </summary>
        IEnumerator<RDFNamespace> IEnumerable<RDFNamespace>.GetEnumerator()
            => NamespacesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the register's namespaces
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => NamespacesEnumerator;
        #endregion

        #region Methods
        /// <summary>
        /// Sets the given namespace as default namespace of the library.
        /// </summary>
        public static void SetDefaultNamespace(RDFNamespace nSpace)
        {
            if (nSpace != null)
            {
                DefaultNamespace = nSpace;

                //Also add it to the register
                AddNamespace(nSpace);
            }
        }

        /// <summary>
        /// Resets the default namespace of the library.
        /// </summary>
        public static void ResetDefaultNamespace()
            => DefaultNamespace = RDFSharpNS;

        /// <summary>
        /// Adds the given namespace to the register, if it has unique prefix and uri.
        /// </summary>
        public static void AddNamespace(RDFNamespace nSpace)
        {
            if (nSpace != null)
            {
                if (GetByPrefix(nSpace.NamespacePrefix) == null && GetByUri(nSpace.NamespaceUri.ToString()) == null)
                    Instance.Register.Add(nSpace);
            }
        }

        /// <summary>
        /// Removes the namespace having the given Uri.
        /// </summary>
        public static void RemoveByUri(string uri)
        {
            if (uri != null)
                Instance.Register.RemoveAll(ns => ns.NamespaceUri.ToString().Equals(uri.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Removes the namespace having the given prefix.
        /// </summary>
        public static void RemoveByPrefix(string prefix)
        {
            if (prefix != null)
                Instance.Register.RemoveAll(ns => ns.NamespacePrefix.Equals(prefix.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retrieves a namespace by seeking presence of its Uri.
        /// </summary>
        public static RDFNamespace GetByUri(string uri, bool enablePrefixCCService = false)
        {
            if (uri != null)
            {
                RDFNamespace result = Instance.Register.Find(ns => ns.NamespaceUri.ToString().Equals(uri.Trim(), StringComparison.OrdinalIgnoreCase));
                if (result == null && enablePrefixCCService)
                    result = LookupPrefixCC(uri.Trim().TrimEnd(new char[] { '#' }), 2);
                return result;
            }
            return null;
        }

        /// <summary>
        /// Retrieves a namespace by seeking presence of its prefix.
        /// </summary>
        public static RDFNamespace GetByPrefix(string prefix, bool enablePrefixCCService = false)
        {
            if (prefix != null)
            {
                RDFNamespace result = Instance.Register.Find(ns => ns.NamespacePrefix.Equals(prefix.Trim(), StringComparison.OrdinalIgnoreCase));
                if (result == null && enablePrefixCCService)
                    result = LookupPrefixCC(prefix.Trim(), 1);
                return result;
            }
            return null;
        }

        /// <summary>
        /// Looksup the given prefix or namespace into the prefix.cc service
        /// </summary>
        internal static RDFNamespace LookupPrefixCC(string data, int lookupMode)
        {
            string lookupString = lookupMode == 1 ? string.Concat("http://prefix.cc/", data, ".file.txt")
                                                  : string.Concat("http://prefix.cc/reverse?uri=", data, "&format=txt");

            using (RDFWebClient webclient = new RDFWebClient(1000))
            {
                try
                {
                    string response = webclient.DownloadString(lookupString);
                    string prefix = response.Split('\t')[0];
                    string nspace = response.Split('\t')[1].TrimEnd(new char[] { '\n' });
                    RDFNamespace result = new RDFNamespace(prefix, nspace);

                    //Also add the namespace to the register, to avoid future lookups
                    AddNamespace(result);

                    //Return the found result
                    return result;
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// Removes namespaces marked as temporary
        /// </summary>
        internal static void RemoveTemporaryNamespaces()
            => Instance.Register.RemoveAll(x => x.IsTemporary);
        #endregion
    }
}