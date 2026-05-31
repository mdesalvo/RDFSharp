/*
   Copyright 2012-2026 Marco De Salvo

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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using RDFSharp.Model;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFTriX is responsible for managing serialization to and from TriX data format.
    /// </summary>
    internal static class RDFTriX
    {
        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given store to the given filepath using TriX data format.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static void Serialize(RDFStore store, string filepath)
            => Serialize(store, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given store to the given stream using TriX data format.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static void Serialize(RDFStore store, Stream outputStream)
        {
            try
            {
                #region serialize
                using (XmlWriter trixWriter = XmlWriter.Create(outputStream, Model.RDFTriX.GetWriterSettings()))
                {
                    XmlDocument trixDoc = new XmlDocument();

                    trixWriter.WriteStartDocument();
                    trixWriter.WriteStartElement(string.Empty, "TriX", Model.RDFTriX.TriXNamespace);

                    #region graphs
                    foreach (RDFGraph graph in store.ExtractGraphs())
                        Model.RDFTriX.WriteTriXGraph(trixWriter, trixDoc, graph);
                    #endregion

                    trixWriter.WriteEndElement();
                    trixWriter.WriteEndDocument();
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot serialize TriX because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given TriX filepath to a memory store.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static RDFMemoryStore Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Deserializes the given TriX stream to a memory store.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static RDFMemoryStore Deserialize(Stream inputStream)
        {
            RDFMemoryStore result = new RDFMemoryStore();
            Dictionary<long, RDFGraph> graphs = new Dictionary<long, RDFGraph>();

            try
            {
                #region deserialize
                using (XmlReader trixReader = XmlReader.Create(inputStream, Model.RDFTriX.GetReaderSettings()))
                {
                    XmlDocument nodeFactory = new XmlDocument { XmlResolver = null };
                    Dictionary<string, long> hashContext = new Dictionary<string, long>();

                    #region <TriX>
                    if (Model.RDFTriX.MoveToTriXRoot(trixReader, " given file does not encode a TriX dataset."))
                    {
                        while (trixReader.Read())
                        {
                            if (trixReader.NodeType == XmlNodeType.EndElement)
                                break;
                            if (trixReader.NodeType != XmlNodeType.Element)
                                continue;
                            if (!trixReader.LocalName.Equals("graph", StringComparison.Ordinal))
                                throw new Exception(" a \"<graph>\" element was expected, instead of unrecognized \"<" + trixReader.LocalName + ">\".");

                            #region <graph>

                            //Setup defaults for the current iterating graph
                            Uri graphUri = RDFNamespaceRegister.DefaultNamespace.NamespaceUri;
                            long graphID = RDFNamespaceRegister.DefaultNamespace.NamespaceID;
                            if (!graphs.ContainsKey(graphID))
                                graphs.Add(graphID, new RDFGraph().SetContext(graphUri));

                            long encodedUris = 0;
                            using (XmlReader graphReader = trixReader.ReadSubtree())
                            {
                                graphReader.Read();  //position on <graph>
                                graphReader.Read();  //position on its first child (or its end tag)
                                while (graphReader.ReadState == ReadState.Interactive && graphReader.NodeType != XmlNodeType.EndElement)
                                {
                                    if (graphReader.NodeType != XmlNodeType.Element)
                                    {
                                        graphReader.Read();
                                        continue;
                                    }

                                    //ReadNode materialises only this child's subtree and advances the reader past it
                                    XmlNode graphChild = nodeFactory.ReadNode(graphReader);

                                    //<uri> gives the context of the graph
                                    if (graphChild.Name.Equals("uri", StringComparison.Ordinal))
                                    {
                                        encodedUris++;
                                        if (encodedUris > 1)
                                            throw new Exception(" given file encodes a graph with more than one \"<uri>\" element.");

                                        graphUri = RDFModelUtilities.GetUriFromString(graphChild.ChildNodes[0]?.InnerText)
                                                    ?? RDFNamespaceRegister.DefaultNamespace.NamespaceUri;
                                        graphID = RDFModelUtilities.CreateHash(graphUri.ToString());
                                        if (!graphs.ContainsKey(graphID))
                                            graphs.Add(graphID, new RDFGraph().SetContext(graphUri));
                                    }

                                    //<triple> gives a triple of the graph
                                    else if (graphChild.Name.Equals("triple", StringComparison.Ordinal) && graphChild.ChildNodes.Count == 3)
                                        Model.RDFTriX.ParseTriXTriple(graphs[graphID], graphChild, hashContext);

                                    //Neither <uri> or a well-formed <triple>: exception must be raised
                                    else
                                        throw new Exception("found a TriX element (" + graphChild.Name + ") which is neither \"<uri>\" or \"<triple>\", or is a \"<triple>\" without the required 3 childs.");
                                }
                            }

                            #endregion <graph>
                        }

                        //At last merge parsed graphs into the final store
                        foreach (RDFGraph graph in graphs.Values)
                            result.MergeGraph(graph);
                    }
                    #endregion <TriX>
                }
                #endregion deserialize
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot deserialize TriX because: " + ex.Message, ex);
            }

            return result;
        }
        #endregion

        #endregion
    }
}