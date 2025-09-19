/*
   Copyright 2012-2025 Marco De Salvo

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
    /// RDFNamespaceRegister is a singleton in-memory container for registered RDF namespaces
    /// </summary>
    public sealed class RDFNamespaceRegister : IEnumerable<RDFNamespace>
    {
        #region Properties
        /// <summary>
        /// Default namespace of the library
        /// </summary>
        public static RDFNamespace DefaultNamespace { get; internal set; }

        /// <summary>
        /// Singleton instance of the RDFNamespaceRegister class
        /// </summary>
        public static RDFNamespaceRegister Instance { get; }

        /// <summary>
        /// List of registered namespaces
        /// </summary>
        internal List<RDFNamespace> Register { get; set; }

        /// <summary>
        /// Client used for namespace lookup to prefix.cc services
        /// </summary>
        internal static RDFWebClient WebClient { get; set; }

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
        /// Initializes the singleton instance of the register
        /// </summary>
        static RDFNamespaceRegister()
        {
            Instance = new RDFNamespaceRegister
            {
                Register = new List<RDFNamespace>(32)
                {
                    //Basic
                    new RDFNamespace(RDFVocabulary.RDF.PREFIX, RDFVocabulary.RDF.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.RDF.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.RDFS.PREFIX, RDFVocabulary.RDFS.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.RDFS.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.XSD.PREFIX, RDFVocabulary.XSD.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.XSD.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.OWL.PREFIX, RDFVocabulary.OWL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.OWL.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.SHACL.PREFIX, RDFVocabulary.SHACL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SHACL.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.SWRL.PREFIX, RDFVocabulary.SWRL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SWRL.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.SWRL.SWRLB.PREFIX, RDFVocabulary.SWRL.SWRLB.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SWRL.SWRLB.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.XML.PREFIX, RDFVocabulary.XML.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.XML.DEREFERENCE_URI)).SetReserved(true),

                    //Extended
                    new RDFNamespace(RDFVocabulary.DC.PREFIX, RDFVocabulary.DC.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.DC.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.DC.DCAM.PREFIX, RDFVocabulary.DC.DCAM.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.DC.DCAM.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.DC.DCTERMS.PREFIX, RDFVocabulary.DC.DCTERMS.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.DC.DCTERMS.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.DC.DCTYPE.PREFIX, RDFVocabulary.DC.DCTYPE.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.DC.DCTYPE.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.FOAF.PREFIX, RDFVocabulary.FOAF.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.FOAF.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.GEO.PREFIX, RDFVocabulary.GEO.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.GEO.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.GEOSPARQL.PREFIX, RDFVocabulary.GEOSPARQL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.GEOSPARQL.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.GEOSPARQL.SF.PREFIX, RDFVocabulary.GEOSPARQL.SF.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.GEOSPARQL.SF.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.GEOSPARQL.GEOF.PREFIX, RDFVocabulary.GEOSPARQL.GEOF.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.GEOSPARQL.GEOF.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.SKOS.PREFIX, RDFVocabulary.SKOS.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SKOS.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.SKOS.SKOSXL.PREFIX, RDFVocabulary.SKOS.SKOSXL.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.SKOS.SKOSXL.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.TIME.PREFIX, RDFVocabulary.TIME.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.TIME.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.TIME.GREG.PREFIX, RDFVocabulary.TIME.GREG.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.TIME.GREG.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.TIME.THORS.PREFIX, RDFVocabulary.TIME.THORS.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.TIME.THORS.DEREFERENCE_URI)).SetReserved(true),
                    new RDFNamespace(RDFVocabulary.RDFSHARP.PREFIX, RDFVocabulary.RDFSHARP.BASE_URI).SetDereferenceUri(new Uri(RDFVocabulary.RDFSHARP.DEREFERENCE_URI)).SetReserved(true)
                }
            };

            DefaultNamespace = new RDFNamespace(RDFVocabulary.RDFSHARP.PREFIX, RDFVocabulary.RDFSHARP.BASE_URI)
                                    .SetDereferenceUri(new Uri(RDFVocabulary.RDFSHARP.DEREFERENCE_URI))
                                    .SetReserved(true);
            WebClient = new RDFWebClient(2000);
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
            => DefaultNamespace = new RDFNamespace(RDFVocabulary.RDFSHARP.PREFIX, RDFVocabulary.RDFSHARP.BASE_URI)
                                    .SetDereferenceUri(new Uri(RDFVocabulary.RDFSHARP.DEREFERENCE_URI))
                                    .SetReserved(true);

        /// <summary>
        /// Adds the given namespace to the register, if it has unique prefix and uri.
        /// </summary>
        public static void AddNamespace(RDFNamespace nSpace)
        {
            if (nSpace != null
                && GetByPrefix(nSpace.NamespacePrefix) == null
                && GetByUri(nSpace.NamespaceUri.ToString()) == null)
            {
                Instance.Register.Add(nSpace);
            }
        }

        /// <summary>
        /// Removes the namespace having the given Uri.
        /// </summary>
        public static void RemoveByUri(string uri)
        {
            if (uri != null)
            {
                string uriToDelete = uri.Trim();
                Instance.Register.RemoveAll(ns => !ns.IsReserved && string.Equals(ns.NamespaceUri.ToString(), uriToDelete, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Removes the namespace having the given prefix.
        /// </summary>
        public static void RemoveByPrefix(string prefix)
        {
            if (prefix != null)
            {
                string prefixToDelete = prefix.Trim();
                Instance.Register.RemoveAll(ns => !ns.IsReserved && string.Equals(ns.NamespacePrefix, prefixToDelete, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Retrieves a namespace by seeking presence of its Uri (cascading a lookup to prefix.cc service, if specified)
        /// </summary>
        public static RDFNamespace GetByUri(string uri, bool enablePrefixCCService=false)
        {
            RDFNamespace result = null;
            if (uri != null)
            {
                string uriToSearch = uri.Trim();
                result = Instance.Register.Find(ns => string.Equals(ns.NamespaceUri.ToString(), uriToSearch, StringComparison.OrdinalIgnoreCase));
                if (result == null && enablePrefixCCService)
                {
                    result = LookupPrefixCC(uriToSearch.TrimEnd('#'), 2);
                    //prefix.cc service correctly resolved the given namespace, but we must ensure we haven't a colliding prefix
                    if (result != null && GetByPrefix(result.NamespacePrefix) == null)
                        Instance.Register.Add(result);
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a namespace by seeking presence of its prefix (cascading a lookup to prefix.cc service, if specified)
        /// </summary>
        public static RDFNamespace GetByPrefix(string prefix, bool enablePrefixCCService=false)
        {
            RDFNamespace result = null;
            if (prefix != null)
            {
                string prefixToSearch = prefix.Trim();
                result = Instance.Register.Find(ns => string.Equals(ns.NamespacePrefix, prefixToSearch, StringComparison.OrdinalIgnoreCase));
                if (result == null && enablePrefixCCService)
                {
                    result = LookupPrefixCC(prefixToSearch, 1);
                    //prefix.cc service correctly resolved the given namespace, but we must ensure we haven't a colliding namespace
                    if (result != null && GetByUri(result.NamespaceUri.ToString()) == null)
                        Instance.Register.Add(result);
                }
            }
            return result;
        }

        /// <summary>
        /// Lookups the given prefix or namespace into the prefix.cc service
        /// </summary>
        internal static RDFNamespace LookupPrefixCC(string data, int lookupMode)
        {
            try
            {
                string serviceResponse = WebClient.DownloadString(
                    lookupMode == 1 ? $"http://prefix.cc/{data}.file.txt"
                                    : $"http://prefix.cc/reverse?uri={data}&format=txt");
                string[] namespaceParts = serviceResponse.Split('\t');
                return new RDFNamespace(namespaceParts[0], namespaceParts[1].TrimEnd(Environment.NewLine));
            }
            catch { return null; }
        }

        /// <summary>
        /// Removes namespaces marked as temporary
        /// </summary>
        internal static void RemoveTemporaryNamespaces()
            => Instance.Register.RemoveAll(x => x.IsTemporary);
        #endregion
    }
}