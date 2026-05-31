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
using System.Web;
using System.Xml;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTriX is responsible for managing serialization to and from TriX data format.
    /// </summary>
    internal static class RDFTriX
    {
        #region Statics
        /// <summary>
        /// Namespace of the TriX data format
        /// </summary>
        internal const string TriXNamespace = "http://www.w3.org/2004/03/trix/trix-1/";

        /// <summary>
        /// Builds the XmlWriterSettings shared by the graph and store TriX serializers.
        /// </summary>
        internal static XmlWriterSettings GetWriterSettings()
            => new XmlWriterSettings
            {
                Encoding = RDFModelUtilities.UTF8_NoBOM,
                Indent = true,
                NewLineChars = Environment.NewLine,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                CloseOutput = true
            };

        /// <summary>
        /// Builds the XmlReaderSettings shared by the graph and store TriX deserializers.
        /// </summary>
        internal static XmlReaderSettings GetReaderSettings()
            => new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Parse,
                XmlResolver = null,
                CloseInput = true,
                IgnoreWhitespace = true,
                IgnoreComments = true
            };

        #endregion

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

                using (XmlWriter trixWriter = XmlWriter.Create(outputStream, GetWriterSettings()))
                {
                    XmlDocument trixDoc = new XmlDocument();

                    trixWriter.WriteStartDocument();
                    trixWriter.WriteStartElement(string.Empty, "TriX", TriXNamespace);
                    WriteTriXGraph(trixWriter, trixDoc, graph);
                    trixWriter.WriteEndElement();
                    trixWriter.WriteEndDocument();
                }

                #endregion serialize
            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot serialize TriX because: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Streams the TriX structure corresponding to the given graph, writing it to the given XML writer.
        /// </summary>
        internal static void WriteTriXGraph(XmlWriter trixWriter, XmlDocument trixDoc, RDFGraph graph)
        {
            trixWriter.WriteStartElement(string.Empty, "graph", TriXNamespace);

            #region uri
            trixWriter.WriteStartElement(string.Empty, "uri", TriXNamespace);
            trixWriter.WriteString(graph.ToString());
            trixWriter.WriteEndElement();
            #endregion

            #region triple
            foreach (RDFTriple triple in graph)
                BuildTriXTriple(trixDoc, triple).WriteTo(trixWriter);
            #endregion

            trixWriter.WriteEndElement();
        }

        /// <summary>
        /// Builds the detached "&lt;triple&gt;" DOM subtree corresponding to the given triple.
        /// </summary>
        private static XmlNode BuildTriXTriple(XmlDocument trixDoc, RDFTriple t)
        {
            XmlNode tripleElement = trixDoc.CreateNode(XmlNodeType.Element, "triple", TriXNamespace);

            #region subject

            XmlNode subjElement = ((RDFResource)t.Subject).IsBlank
                ? trixDoc.CreateNode(XmlNodeType.Element, "id", TriXNamespace)
                : trixDoc.CreateNode(XmlNodeType.Element, "uri", TriXNamespace);
            XmlText subjElementText = trixDoc.CreateTextNode(t.Subject.ToString());
            subjElement.AppendChild(subjElementText);
            tripleElement.AppendChild(subjElement);

            #endregion subject

            #region predicate

            XmlNode uriElementP = trixDoc.CreateNode(XmlNodeType.Element, "uri", TriXNamespace);
            XmlText uriTextP = trixDoc.CreateTextNode(t.Predicate.ToString());
            uriElementP.AppendChild(uriTextP);
            tripleElement.AppendChild(uriElementP);

            #endregion predicate

            #region object

            if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
            {
                XmlNode objElement = ((RDFResource)t.Object).IsBlank ?
                    trixDoc.CreateNode(XmlNodeType.Element, "id", TriXNamespace) :
                    trixDoc.CreateNode(XmlNodeType.Element, "uri", TriXNamespace);
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
                    XmlNode plainLiteralElement = trixDoc.CreateNode(XmlNodeType.Element, "plainLiteral", TriXNamespace);
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
                    XmlNode typedLiteralElement = trixDoc.CreateNode(XmlNodeType.Element, "typedLiteral", TriXNamespace);
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

            return tripleElement;
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
                using (XmlReader trixReader = XmlReader.Create(inputStream, GetReaderSettings()))
                {
                    XmlDocument nodeFactory = new XmlDocument { XmlResolver = null };
                    Dictionary<string, long> hashContext = new Dictionary<string, long>();

                    #region <TriX>
                    if (MoveToTriXRoot(trixReader, " given file does not encode a TriX graph."))
                    {
                        int encodedGraphs = 0;
                        while (trixReader.Read())
                        {
                            if (trixReader.NodeType == XmlNodeType.EndElement)
                                break;
                            if (trixReader.NodeType != XmlNodeType.Element)
                                continue;
                            if (!trixReader.LocalName.Equals("graph", StringComparison.Ordinal))
                                throw new Exception(" a \"<graph>\" element was expected, instead of unrecognized \"<" + trixReader.LocalName + ">\".");

                            encodedGraphs++;
                            if (encodedGraphs > 1)
                                throw new Exception(" given TriX file seems to encode more than one graph.");

                            #region <graph>
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

                                        result.SetContext(RDFModelUtilities.GetUriFromString(graphChild.ChildNodes[0]?.InnerText)
                                                            ?? RDFNamespaceRegister.DefaultNamespace.NamespaceUri);
                                    }

                                    //<triple> gives a triple of the graph
                                    else if (graphChild.Name.Equals("triple", StringComparison.Ordinal) && graphChild.ChildNodes.Count == 3)
                                        ParseTriXTriple(result, graphChild, hashContext);

                                    //Neither <uri> or a well-formed <triple>: exception must be raised
                                    else
                                        throw new Exception("found a TriX element (" + graphChild.Name + ") which is neither \"<uri>\" or \"<triple>\", or is a \"<triple>\" without the required 3 childs.");
                                }
                            }
                            #endregion <graph>
                        }
                    }
                    #endregion <TriX>
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
        /// Advances the reader onto the "&lt;TriX&gt;" root, validating its name and namespace.
        /// Returns false when the document carries no root element.
        /// </summary>
        internal static bool MoveToTriXRoot(XmlReader trixReader, string mismatchMessage)
        {
            if (trixReader.MoveToContent() != XmlNodeType.Element)
                return false;

            if (!trixReader.LocalName.Equals("TriX", StringComparison.Ordinal)
                 || !trixReader.NamespaceURI.Equals(TriXNamespace, StringComparison.Ordinal))
            {
                throw new Exception(mismatchMessage);
            }

            return true;
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