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
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Xml;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTriX is responsible for managing serialization to and from TriX data format.
    /// </summary>
    internal static class RDFTriX
    {
        #region Methods

        #region Write

        /// <summary>
        /// Serializes the given graph to the given filepath using TriX data format.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static void Serialize(RDFGraph graph, string filepath)
            => Serialize(graph, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given graph to the given stream using TriX data format.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static void Serialize(RDFGraph graph, Stream outputStream)
        {
            try
            {
                #region serialize

                using (XmlTextWriter trixWriter = new XmlTextWriter(outputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    trixWriter.Formatting = Formatting.Indented;

                    #region xmlDecl
                    XmlDocument trixDoc = new XmlDocument();
                    trixDoc.AppendChild(trixDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                    #endregion

                    #region trixRoot
                    XmlNode trixRoot = trixDoc.CreateNode(XmlNodeType.Element, "TriX", null);
                    XmlAttribute trixRootNS = trixDoc.CreateAttribute("xmlns");
                    XmlText trixRootNSText = trixDoc.CreateTextNode("http://www.w3.org/2004/03/trix/trix-1/");
                    trixRootNS.AppendChild(trixRootNSText);
                    trixRoot.Attributes.Append(trixRootNS);

                    #region graph
                    AppendTriXGraph(trixDoc, trixRoot, graph);
                    #endregion

                    trixDoc.AppendChild(trixRoot);
                    #endregion

                    trixDoc.Save(trixWriter);
                }

                #endregion serialize
            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot serialize TriX because: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Serializes the TriX structure corresponding to the given graph, appending it to the given XML root node.
        /// </summary>
        internal static void AppendTriXGraph(XmlDocument trixDoc, XmlNode trixRoot, RDFGraph graph)
        {
            XmlNode graphElement = trixDoc.CreateNode(XmlNodeType.Element, "graph", null);
            XmlNode graphUriElement = trixDoc.CreateNode(XmlNodeType.Element, "uri", null);
            XmlText graphUriElementT = trixDoc.CreateTextNode(graph.ToString());
            graphUriElement.AppendChild(graphUriElementT);
            graphElement.AppendChild(graphUriElement);

            #region triple

            foreach (RDFTriple t in graph)
            {
                XmlNode tripleElement = trixDoc.CreateNode(XmlNodeType.Element, "triple", null);

                #region subject

                XmlNode subjElement = ((RDFResource)t.Subject).IsBlank
                    ? trixDoc.CreateNode(XmlNodeType.Element, "id", null)
                    : trixDoc.CreateNode(XmlNodeType.Element, "uri", null);
                XmlText subjElementText = trixDoc.CreateTextNode(t.Subject.ToString());
                subjElement.AppendChild(subjElementText);
                tripleElement.AppendChild(subjElement);

                #endregion subject

                #region predicate

                XmlNode uriElementP = trixDoc.CreateNode(XmlNodeType.Element, "uri", null);
                XmlText uriTextP = trixDoc.CreateTextNode(t.Predicate.ToString());
                uriElementP.AppendChild(uriTextP);
                tripleElement.AppendChild(uriElementP);

                #endregion predicate

                #region object

                if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                {
                    XmlNode objElement = ((RDFResource)t.Object).IsBlank ?
                        trixDoc.CreateNode(XmlNodeType.Element, "id", null) :
                        trixDoc.CreateNode(XmlNodeType.Element, "uri", null);
                    XmlText objElementText = trixDoc.CreateTextNode(t.Object.ToString());
                    objElement.AppendChild(objElementText);
                    tripleElement.AppendChild(objElement);
                }

                #endregion object

                #region literal

                else
                {
                    #region plain literal

                    if (t.Object is RDFPlainLiteral objLit)
                    {
                        XmlNode plainLiteralElement = trixDoc.CreateNode(XmlNodeType.Element, "plainLiteral", null);
                        if (objLit.HasLanguage())
                        {
                            XmlAttribute xmlLang = trixDoc.CreateAttribute("xml:lang", RDFVocabulary.XML.BASE_URI);
                            XmlText xmlLangText = trixDoc.CreateTextNode(objLit.Language);
                            xmlLang.AppendChild(xmlLangText);
                            plainLiteralElement.Attributes.Append(xmlLang);
                        }
                        XmlText plainLiteralText = trixDoc.CreateTextNode(RDFModelUtilities.EscapeControlCharsForXML(HttpUtility.HtmlDecode(objLit.Value)));
                        plainLiteralElement.AppendChild(plainLiteralText);
                        tripleElement.AppendChild(plainLiteralElement);
                    }

                    #endregion plain literal

                    #region typed literal

                    else
                    {
                        XmlNode typedLiteralElement = trixDoc.CreateNode(XmlNodeType.Element, "typedLiteral", null);
                        XmlAttribute datatype = trixDoc.CreateAttribute("datatype");
                        XmlText datatypeText = trixDoc.CreateTextNode(((RDFTypedLiteral)t.Object).Datatype.URI.ToString());
                        datatype.AppendChild(datatypeText);
                        typedLiteralElement.Attributes.Append(datatype);
                        XmlText typedLiteralText = trixDoc.CreateTextNode(RDFModelUtilities.EscapeControlCharsForXML(HttpUtility.HtmlDecode(((RDFLiteral)t.Object).Value)));
                        typedLiteralElement.AppendChild(typedLiteralText);
                        tripleElement.AppendChild(typedLiteralElement);
                    }

                    #endregion typed literal
                }

                #endregion literal

                graphElement.AppendChild(tripleElement);
            }

            #endregion triple

            trixRoot.AppendChild(graphElement);
        }

        #endregion Write

        #region Read

        /// <summary>
        /// Deserializes the given TriX filepath to a graph.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static RDFGraph Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open), null);
        /// <summary>
        /// Deserializes the given TriX stream to a graph.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static RDFGraph Deserialize(Stream inputStream, Uri graphContext)
        {
            RDFGraph result = new RDFGraph().SetContext(graphContext);

            try
            {
                #region deserialize
                using (StreamReader streamReader = new StreamReader(inputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    using (XmlTextReader trixReader = new XmlTextReader(streamReader))
                    {
                        trixReader.DtdProcessing = DtdProcessing.Parse;
                        trixReader.XmlResolver = null;
                        trixReader.Normalization = false;
                        XmlDocument trixDoc = new XmlDocument { XmlResolver = null };
                        trixDoc.Load(trixReader);

                        #region <TriX>

                        if (trixDoc.DocumentElement != null)
                        {
                            #region Guards
                            if (!trixDoc.DocumentElement.Name.Equals("TriX")
                                 || !trixDoc.DocumentElement.NamespaceURI.Equals("http://www.w3.org/2004/03/trix/trix-1/"))
                                throw new Exception(" given file does not encode a TriX graph.");
                            if (trixDoc.DocumentElement.ChildNodes.Count > 1)
                                throw new Exception(" given TriX file seems to encode more than one graph.");
                            #endregion Guards

                            Dictionary<string, long> hashContext = new Dictionary<string, long>(128);
                            foreach (XmlNode graph in trixDoc.DocumentElement.ChildNodes)
                            {
                                if (!graph.Name.Equals("graph", StringComparison.Ordinal))
                                    throw new Exception(" a \"<graph>\" element was expected, instead of unrecognized \"<" + graph.Name + ">\".");

                                #region <graph>
                                long encodedUris = 0;
                                foreach (XmlNode graphChild in graph.ChildNodes)
                                {
                                    #region <uri>

                                    //<uri> gives the context of the graph
                                    if (graphChild.Name.Equals("uri", StringComparison.Ordinal))
                                    {
                                        encodedUris++;
                                        if (encodedUris > 1)
                                            throw new Exception(" given file encodes a graph with more than one \"<uri>\" element.");

                                        result.SetContext(RDFModelUtilities.GetUriFromString(graphChild.ChildNodes[0]?.InnerText)
                                                            ?? RDFNamespaceRegister.DefaultNamespace.NamespaceUri);
                                    }

                                    #endregion <uri>

                                    #region <triple>

                                    //<triple> gives a triple of the graph
                                    else if (graphChild.Name.Equals("triple", StringComparison.Ordinal) && graphChild.ChildNodes.Count == 3)
                                        ParseTriXTriple(result, graphChild, hashContext);

                                    //Neither <uri> or a well-formed <triple>: exception must be raised
                                    else
                                        throw new Exception("found a TriX element (" + graphChild.Name + ") which is neither \"<uri>\" or \"<triple>\", or is a \"<triple>\" without the required 3 childs.");

                                    #endregion <triple>
                                }
                                #endregion <graph>
                            }
                        }

                        #endregion <TriX>
                    }
                }
                #endregion deserialize
            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot deserialize TriX because: " + ex.Message, ex);
            }

            return result;
        }

        /// <summary>
        /// Deserializes the TriX structure corresponding to the given triple node, appending the RDFTriple into the given graph.
        /// </summary>
        internal static void ParseTriXTriple(RDFGraph result, XmlNode graphChild, Dictionary<string, long> hashContext)
        {
            //Subject is a resource ("<uri>") or a blank node ("<id>")
            if (string.Equals(graphChild.ChildNodes[0].Name, "uri", StringComparison.Ordinal)
                 || string.Equals(graphChild.ChildNodes[0].Name, "id", StringComparison.Ordinal))
            {
                //Subject without value: exception must be raised
                if (string.IsNullOrEmpty(graphChild.ChildNodes[0].InnerText))
                    throw new Exception("subject (" + graphChild.ChildNodes[0].Name + ") of \"<triple>\" element has \"<uri>\" or \"<id>\" element without value.");

                //Sanitize eventual blank node value
                if (string.Equals(graphChild.ChildNodes[0].Name, "id", StringComparison.Ordinal)
                     && !graphChild.ChildNodes[0].InnerText.StartsWith("bnode:", StringComparison.OrdinalIgnoreCase))
                {
                    graphChild.ChildNodes[0].InnerText = $"bnode:{graphChild.ChildNodes[0].InnerText.Replace("_:", string.Empty)}";
                }
            }
            //Subject unrecognized: exception must be raised
            else
            {
                throw new Exception("subject (" + graphChild.ChildNodes[0].Name + ") of \"<triple>\" element is neither \"<uri>\" or \"<id>\".");
            }

            //Predicate is a resource ("<uri>")
            if (graphChild.ChildNodes[1].Name.Equals("uri", StringComparison.Ordinal))
            {
                //Predicate without value: exception must be raised
                if (string.IsNullOrEmpty(graphChild.ChildNodes[1].InnerText))
                    throw new Exception("predicate (" + graphChild.ChildNodes[1].Name + ") of \"<triple>\" element has \"<uri>\" element without value.");
            }
            //Predicate unrecognized: exception must be raised
            else
            {
                throw new Exception("predicate (" + graphChild.ChildNodes[1].Name + ") of \"<triple>\" element must be \"<uri>\".");
            }

            //Object is a resource ("<uri>") or a blank node ("<id>")
            if (string.Equals(graphChild.ChildNodes[2].Name, "uri", StringComparison.Ordinal)
                 || string.Equals(graphChild.ChildNodes[2].Name, "id", StringComparison.Ordinal))
            {
                //Object without value: exception must be raised
                if (string.IsNullOrEmpty(graphChild.ChildNodes[2].InnerText))
                    throw new Exception("object (" + graphChild.ChildNodes[2].Name + ") of \"<triple>\" element has \"<uri>\" or \"<id>\" element without value.");

                //Sanitize eventual blank node value
                if (string.Equals(graphChild.ChildNodes[2].Name, "id", StringComparison.Ordinal)
                     && !graphChild.ChildNodes[2].InnerText.StartsWith("bnode:", StringComparison.OrdinalIgnoreCase))
                {
                    graphChild.ChildNodes[2].InnerText = $"bnode:{graphChild.ChildNodes[2].InnerText.Replace("_:", string.Empty)}";
                }

                result.AddTriple(new RDFTriple(new RDFResource(graphChild.ChildNodes[0].InnerText, hashContext),
                                               new RDFResource(graphChild.ChildNodes[1].InnerText, hashContext),
                                               new RDFResource(graphChild.ChildNodes[2].InnerText, hashContext)));
            }

            //Object is a plain literal ("<plainLiteral>")
            else if (string.Equals(graphChild.ChildNodes[2].Name, "plainLiteral", StringComparison.Ordinal))
            {
                XmlAttribute xmlLang = graphChild.ChildNodes[2].Attributes["xml:lang"];

                //Plain literal has language
                if (xmlLang != null)
                {
                    result.AddTriple(new RDFTriple(new RDFResource(graphChild.ChildNodes[0].InnerText, hashContext),
                                                   new RDFResource(graphChild.ChildNodes[1].InnerText, hashContext),
                                                   new RDFPlainLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(graphChild.ChildNodes[2].InnerText)), xmlLang.Value)));
                }

                //Plain literal has not language
                else
                {
                    result.AddTriple(new RDFTriple(new RDFResource(graphChild.ChildNodes[0].InnerText, hashContext),
                                                   new RDFResource(graphChild.ChildNodes[1].InnerText, hashContext),
                                                   new RDFPlainLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(graphChild.ChildNodes[2].InnerText)))));
                }
            }

            //Object is a typed literal ("<typedLiteral>")
            else if (string.Equals(graphChild.ChildNodes[2].Name, "typedLiteral", StringComparison.Ordinal))
            {
                XmlAttribute datatype = graphChild.ChildNodes[2].Attributes["datatype"];

                //Typed literal has datatype
                if (datatype != null)
                {
                    result.AddTriple(new RDFTriple(new RDFResource(graphChild.ChildNodes[0].InnerText, hashContext),
                                                   new RDFResource(graphChild.ChildNodes[1].InnerText, hashContext),
                                                   new RDFTypedLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(graphChild.ChildNodes[2].InnerText)), RDFDatatypeRegister.GetDatatype(datatype.Value))));
                }

                //Typed literal has not datatype: exception must be raised
                else
                {
                    throw new Exception(" found typed literal without required \"datatype\" attribute.");
                }
            }

            //Object unrecognized: exception must be raised
            else
            {
                throw new Exception("object (" + graphChild.ChildNodes[2].Name + ") of \"<triple>\" element is neither \"<uri>\" or \"<id>\" or \"<plainLiteral>\" or \"<typedLiteral>\".");
            }
        }

        #endregion Read

        #endregion Methods
    }
}