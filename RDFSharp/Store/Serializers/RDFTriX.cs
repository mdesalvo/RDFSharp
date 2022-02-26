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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

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
        internal static void Serialize(RDFStore store, string filepath)
            => Serialize(store, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given store to the given stream using TriX data format.
        /// </summary>
        internal static void Serialize(RDFStore store, Stream outputStream)
        {
            try
            {
                #region serialize
                using (XmlTextWriter trixWriter = new XmlTextWriter(outputStream, RDFModelUtilities.UTF8_NoBom))
                {
                    XmlDocument trixDoc = new XmlDocument();
                    trixWriter.Formatting = Formatting.Indented;

                    #region xmlDecl
                    XmlDeclaration trixDecl = trixDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    trixDoc.AppendChild(trixDecl);
                    #endregion

                    #region trixRoot
                    XmlNode trixRoot = trixDoc.CreateNode(XmlNodeType.Element, "TriX", null);
                    XmlAttribute trixRootNS = trixDoc.CreateAttribute("xmlns");
                    XmlText trixRootNSText = trixDoc.CreateTextNode("http://www.w3.org/2004/03/trix/trix-1/");
                    trixRootNS.AppendChild(trixRootNSText);
                    trixRoot.Attributes.Append(trixRootNS);

                    #region graphs
                    foreach (RDFGraph graph in store.ExtractGraphs())
                        RDFSharp.Model.RDFTriX.AppendTriXGraph(trixDoc, trixRoot, graph);
                    #endregion

                    trixDoc.AppendChild(trixRoot);
                    #endregion

                    trixDoc.Save(trixWriter);
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
        internal static RDFMemoryStore Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Deserializes the given TriX stream to a memory store.
        /// </summary>
        internal static RDFMemoryStore Deserialize(Stream inputStream)
        {
            try
            {
                #region deserialize

                RDFMemoryStore result = new RDFMemoryStore();
                Dictionary<long, RDFGraph> graphs = new Dictionary<long, RDFGraph>();
                using (StreamReader streamReader = new StreamReader(inputStream, RDFModelUtilities.UTF8_NoBom))
                {
                    using (XmlTextReader trixReader = new XmlTextReader(streamReader))
                    {
                        trixReader.DtdProcessing = DtdProcessing.Parse;
                        trixReader.Normalization = false;
                        XmlDocument trixDoc = new XmlDocument();
                        trixDoc.Load(trixReader);

                        #region <TriX>

                        if (trixDoc.DocumentElement != null)
                        {
                            #region Guards

                            if (!trixDoc.DocumentElement.Name.Equals("TriX")
                                    || !trixDoc.DocumentElement.NamespaceURI.Equals("http://www.w3.org/2004/03/trix/trix-1/"))
                                throw new Exception(" given file does not encode a TriX dataset.");

                            #endregion Guards

                            IEnumerator graphEnum = trixDoc.DocumentElement.ChildNodes.GetEnumerator();
                            while (graphEnum != null && graphEnum.MoveNext())
                            {
                                XmlNode graph = (XmlNode)graphEnum.Current;
                                if (!graph.Name.Equals("graph", StringComparison.Ordinal))
                                    throw new Exception(" a \"<graph>\" element was expected, instead of unrecognized \"<" + graph.Name + ">\".");
                                
                                #region <graph>

                                //Setup defaults for the current iterating graph
                                Uri graphUri = RDFNamespaceRegister.DefaultNamespace.NamespaceUri;
                                long graphID = RDFNamespaceRegister.DefaultNamespace.NamespaceID;
                                if (!graphs.ContainsKey(graphID))
                                    graphs.Add(graphID, new RDFGraph().SetContext(graphUri));

                                long encodedUris = 0;
                                IEnumerator graphChildren = graph.ChildNodes.GetEnumerator();
                                while (graphChildren != null && graphChildren.MoveNext())
                                {
                                    XmlNode graphChild = (XmlNode)graphChildren.Current;

                                    #region <uri>

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

                                    #endregion <uri>

                                    #region <triple>

                                    //<triple> gives a triple of the graph
                                    else if (graphChild.Name.Equals("triple", StringComparison.Ordinal) && graphChild.ChildNodes.Count == 3)
                                    {
                                        //Subject is a resource ("<uri>") or a blank node ("<id>")
                                        if (graphChild.ChildNodes[0].Name.Equals("uri", StringComparison.Ordinal)
                                                || graphChild.ChildNodes[0].Name.Equals("id", StringComparison.Ordinal))
                                        {
                                            //Subject without value: exception must be raised
                                            if (string.IsNullOrEmpty(graphChild.ChildNodes[0].InnerText))
                                                throw new Exception("subject (" + graphChild.ChildNodes[0].Name + ") of \"<triple>\" element has \"<uri>\" or \"<id>\" element without value.");

                                            //Sanitize eventual blank node value
                                            if (graphChild.ChildNodes[0].Name.Equals("id", StringComparison.Ordinal))
                                            {
                                                if (!graphChild.ChildNodes[0].InnerText.StartsWith("bnode:"))
                                                    graphChild.ChildNodes[0].InnerText = string.Concat("bnode:", graphChild.ChildNodes[0].InnerText.Replace("_:", string.Empty));
                                            }
                                        }
                                        //Subject unrecognized: exception must be raised
                                        else
                                            throw new Exception("subject (" + graphChild.ChildNodes[0].Name + ") of \"<triple>\" element is neither \"<uri>\" or \"<id>\".");
                                        
                                        //Predicate is a resource ("<uri>") 
                                        if (graphChild.ChildNodes[1].Name.Equals("uri", StringComparison.Ordinal))
                                        {
                                            //Predicate without value: exception must be raised
                                            if (string.IsNullOrEmpty(graphChild.ChildNodes[1].InnerText))
                                                throw new Exception("predicate (" + graphChild.ChildNodes[1].Name + ") of \"<triple>\" element has \"<uri>\" element without value.");
                                        }
                                        //Predicate unrecognized: exception must be raised
                                        else
                                            throw new Exception("predicate (" + graphChild.ChildNodes[1].Name + ") of \"<triple>\" element must be \"<uri>\".");

                                        //Object is a resource ("<uri>") or a blank node ("<id>")
                                        if (graphChild.ChildNodes[2].Name.Equals("uri", StringComparison.Ordinal)
                                                || graphChild.ChildNodes[2].Name.Equals("id", StringComparison.Ordinal))
                                        {
                                            //Object without value: exception must be raised
                                            if (string.IsNullOrEmpty(graphChild.ChildNodes[2].InnerText))
                                                throw new Exception("object (" + graphChild.ChildNodes[2].Name + ") of \"<triple>\" element has \"<uri>\" or \"<id>\" element without value.");

                                            //Sanitize eventual blank node value
                                            if (graphChild.ChildNodes[2].Name.Equals("id", StringComparison.Ordinal))
                                            {
                                                if (!graphChild.ChildNodes[2].InnerText.StartsWith("bnode:"))
                                                    graphChild.ChildNodes[2].InnerText = string.Concat("bnode:", graphChild.ChildNodes[2].InnerText.Replace("_:", string.Empty));
                                            }

                                            graphs[graphID].AddTriple(new RDFTriple(new RDFResource(graphChild.ChildNodes[0].InnerText),
                                                                                    new RDFResource(graphChild.ChildNodes[1].InnerText),
                                                                                    new RDFResource(graphChild.ChildNodes[2].InnerText)));
                                        }
                                        
                                        //Object is a plain literal ("<plainLiteral>")
                                        else if (graphChild.ChildNodes[2].Name.Equals("plainLiteral"))
                                        {
                                            XmlAttribute xmlLang = graphChild.ChildNodes[2].Attributes["xml:lang"];

                                            //Plain literal has language
                                            if (xmlLang != null)
                                                graphs[graphID].AddTriple(new RDFTriple(new RDFResource(graphChild.ChildNodes[0].InnerText),
                                                                                        new RDFResource(graphChild.ChildNodes[1].InnerText),
                                                                                        new RDFPlainLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(graphChild.ChildNodes[2].InnerText)), xmlLang.Value)));
                                            
                                            //Plain literal has not language
                                            else
                                                graphs[graphID].AddTriple(new RDFTriple(new RDFResource(graphChild.ChildNodes[0].InnerText),
                                                                                        new RDFResource(graphChild.ChildNodes[1].InnerText),
                                                                                        new RDFPlainLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(graphChild.ChildNodes[2].InnerText)))));
                                        }
                                        
                                        //Object is a typed literal ("<typedLiteral>")
                                        else if (graphChild.ChildNodes[2].Name.Equals("typedLiteral", StringComparison.Ordinal))
                                        {
                                            XmlAttribute datatype = graphChild.ChildNodes[2].Attributes["datatype"];

                                            //Typed literal has datatype
                                            if (datatype != null)
                                                graphs[graphID].AddTriple(new RDFTriple(new RDFResource(graphChild.ChildNodes[0].InnerText),
                                                                                        new RDFResource(graphChild.ChildNodes[1].InnerText),
                                                                                        new RDFTypedLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(graphChild.ChildNodes[2].InnerText)), RDFModelUtilities.GetDatatypeFromString(datatype.Value))));

                                            //Typed literal has not datatype: exception must be raised
                                            else
                                                throw new Exception(" found typed literal without required \"datatype\" attribute.");
                                        }
                                        
                                        //Object unrecognized: exception must be raised
                                        else
                                            throw new Exception("object (" + graphChild.ChildNodes[2].Name + ") of \"<triple>\" element is neither \"<uri>\" or \"<id>\" or \"<plainLiteral>\" or \"<typedLiteral>\".");
                                    }
                                    
                                    //Neither <uri> or a well-formed <triple>: exception must be raised
                                    else
                                        throw new Exception("found a TriX element (" + graphChild.Name + ") which is neither \"<uri>\" or \"<triple>\", or is a \"<triple>\" without the required 3 childs.");

                                    #endregion <triple>
                                }

                                #endregion <graph>
                            }

                            //At last merge parsed graphs into the final store
                            foreach (RDFGraph graph in graphs.Values)
                                result.MergeGraph(graph);
                        }

                        #endregion <TriX>
                    }
                }
                return result;

                #endregion deserialize
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot deserialize TriX because: " + ex.Message, ex);
            }
        }
        #endregion

        #endregion
    }

}