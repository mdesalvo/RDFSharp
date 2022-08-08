/*
   Copyright 2012-2022 Marco De Salvo

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
using RDFSharp.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFTriG is responsible for managing serialization to and from TriG data format.
    /// </summary>
    internal static class RDFTriG
    {
        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given store to the given filepath using TriG data format.
        /// </summary>
        internal static void Serialize(RDFStore store, string filepath)
            => Serialize(store, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given store to the given stream using TriG data format.
        /// </summary>
        internal static void Serialize(RDFStore store, Stream outputStream)
        {
            try
            {
                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    //Extract graphs
                    List<RDFGraph> graphs = store.ExtractGraphs();

                    //Collect namespaces
                    List<RDFNamespace> prefixes = new List<RDFNamespace>();
                    foreach (RDFGraph graph in graphs)
                        prefixes.AddRange(RDFModelUtilities.GetGraphNamespaces(graph));

                    //Write namespaces (avoid duplicates)
                    HashSet<string> printedNamespaces = new HashSet<string>();
                    foreach (RDFNamespace ns in prefixes.OrderBy(n => n.NamespacePrefix))
                    {
                        if (!printedNamespaces.Contains(ns.NamespacePrefix))
                        {
                            printedNamespaces.Add(ns.NamespacePrefix);
                            sw.WriteLine(string.Concat("@prefix ", ns.NamespacePrefix, ": <", ns.NamespaceUri, ">."));
                        }
                    }
                    if (printedNamespaces.Count > 0)
                        sw.WriteLine();

                    //Write graphs
                    foreach (RDFGraph graph in graphs)
                        RDFTurtle.WriteTurtleGraph(sw, graph, prefixes, true);
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot serialize TriG because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given TriG filepath to a memory store.
        /// </summary>
        internal static RDFMemoryStore Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Deserializes the given TriG stream to a memory store.
        /// </summary>
        internal static RDFMemoryStore Deserialize(Stream inputStream)
        {
            try
            {
                #region deserialize

                RDFMemoryStore result = new RDFMemoryStore();
                
                //TODO


                return result;

                #endregion deserialize
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot deserialize TriG because: " + ex.Message, ex);
            }
        }
        #endregion

        #endregion
    }
}