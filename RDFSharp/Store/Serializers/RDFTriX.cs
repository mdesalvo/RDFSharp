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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using RDFSharp.Model;

namespace RDFSharp.Store {

    /// <summary>
    /// RDFTriX is responsible for managing serialization to and from TriX data format.
    /// </summary>
    internal static class RDFTriX {

        #region Methods
        /// <summary>
        /// Serializes the given store to the given filepath using TriX data format. 
        /// </summary>
        internal static void Serialize(RDFStore store, String filepath) {
            Serialize(store, new FileStream(filepath, FileMode.Create));
        }

        /// <summary>
        /// Serializes the given store to the given stream using TriX data format. 
        /// </summary>
        internal static void Serialize(RDFStore store, Stream outputStream) {
            try {

                #region serialize
                using (XmlTextWriter trixWriter = new XmlTextWriter(outputStream, Encoding.UTF8)) {
                    XmlDocument trixDoc         = new XmlDocument();
                    trixWriter.Formatting       = Formatting.Indented;

                    #region xmlDecl
                    XmlDeclaration trixDecl     = trixDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    trixDoc.AppendChild(trixDecl);
                    #endregion

                    #region trixRoot
                    XmlNode trixRoot            = trixDoc.CreateNode(XmlNodeType.Element, "TriX", null);
                    XmlAttribute trixRootNS     = trixDoc.CreateAttribute("xmlns");
                    XmlText trixRootNSText      = trixDoc.CreateTextNode("http://www.w3.org/2004/03/trix/trix-1/");
                    trixRootNS.AppendChild(trixRootNSText);
                    trixRoot.Attributes.Append(trixRootNS);

                    #region graphs
                    foreach (var graph             in store.ExtractGraphs()) {
                        XmlNode graphElement        = trixDoc.CreateNode(XmlNodeType.Element, "graph", null);
                        XmlNode graphUriElement     = trixDoc.CreateNode(XmlNodeType.Element, "uri", null);
                        XmlText graphUriElementT    = trixDoc.CreateTextNode(graph.ToString());
                        graphUriElement.AppendChild(graphUriElementT);
                        graphElement.AppendChild(graphUriElement);

                        #region triple
                        foreach(var t in graph) {
                            XmlNode tripleElement   = trixDoc.CreateNode(XmlNodeType.Element, "triple", null);

                            #region subj
                            XmlNode subjElement     = null;
                            XmlText subjElementText = null;
                            if(((RDFResource)t.Subject).IsBlank) {
                                subjElement         = trixDoc.CreateNode(XmlNodeType.Element, "id", null);
                                subjElementText     = trixDoc.CreateTextNode(t.Subject.ToString().Replace("bnode:", String.Empty));
                            }
                            else {
                                subjElement         = trixDoc.CreateNode(XmlNodeType.Element, "uri", null);
                                subjElementText     = trixDoc.CreateTextNode(t.Subject.ToString());
                            }
                            subjElement.AppendChild(subjElementText);
                            tripleElement.AppendChild(subjElement);
                            #endregion

                            #region pred
                            XmlNode uriElementP     = trixDoc.CreateNode(XmlNodeType.Element, "uri", null);
                            XmlText uriTextP        = trixDoc.CreateTextNode(t.Predicate.ToString());
                            uriElementP.AppendChild(uriTextP);
                            tripleElement.AppendChild(uriElementP);
                            #endregion

                            #region object
                            if(t.TripleFlavor         == RDFModelEnums.RDFTripleFlavors.SPO) {
                                XmlNode objElement     = null;
                                XmlText objElementText = null;
                                if(((RDFResource)t.Object).IsBlank) {
                                    objElement         = trixDoc.CreateNode(XmlNodeType.Element, "id", null);
                                    objElementText     = trixDoc.CreateTextNode(t.Object.ToString().Replace("bnode:", String.Empty));
                                }
                                else {
                                    objElement         = trixDoc.CreateNode(XmlNodeType.Element, "uri", null);
                                    objElementText     = trixDoc.CreateTextNode(t.Object.ToString());
                                }
                                objElement.AppendChild(objElementText);
                                tripleElement.AppendChild(objElement);
                            }
                            #endregion

                            #region literal
                            else {

                                #region plain literal
                                if (t.Object is RDFPlainLiteral) {
                                    XmlNode plainLiteralElement = trixDoc.CreateNode(XmlNodeType.Element, "plainLiteral", null);
                                    if(((RDFPlainLiteral)t.Object).Language != String.Empty) {
                                        XmlAttribute xmlLang    = trixDoc.CreateAttribute(RDFVocabulary.XML.PREFIX + ":lang", RDFVocabulary.XML.BASE_URI);
                                        XmlText xmlLangText     = trixDoc.CreateTextNode(((RDFPlainLiteral)t.Object).Language);
                                        xmlLang.AppendChild(xmlLangText);
                                        plainLiteralElement.Attributes.Append(xmlLang);
                                    }
                                    XmlText plainLiteralText    = trixDoc.CreateTextNode(HttpUtility.HtmlDecode(((RDFLiteral)t.Object).Value));
                                    plainLiteralElement.AppendChild(plainLiteralText);
                                    tripleElement.AppendChild(plainLiteralElement);
                                }
                                #endregion

                                #region typed literal
                                else {
                                    XmlNode typedLiteralElement = trixDoc.CreateNode(XmlNodeType.Element, "typedLiteral", null);
                                    XmlAttribute datatype       = trixDoc.CreateAttribute("datatype");
                                    XmlText datatypeText        = trixDoc.CreateTextNode(((RDFTypedLiteral)t.Object).Datatype.ToString());
                                    datatype.AppendChild(datatypeText);
                                    typedLiteralElement.Attributes.Append(datatype);
                                    XmlText typedLiteralText    = trixDoc.CreateTextNode(HttpUtility.HtmlDecode(((RDFLiteral)t.Object).Value));
                                    typedLiteralElement.AppendChild(typedLiteralText);
                                    tripleElement.AppendChild(typedLiteralElement);
                                }
                                #endregion

                            }
                            #endregion

                            graphElement.AppendChild(tripleElement);
                        }
                        #endregion

                        trixRoot.AppendChild(graphElement);
                    }                    
                    #endregion

                    trixDoc.AppendChild(trixRoot);
                    #endregion

                    trixDoc.Save(trixWriter);
                }
                #endregion

            }
            catch(Exception ex) {
                throw new RDFStoreException("Cannot serialize TriX because: " + ex.Message, ex);
            }
        }
        #endregion

    }

}