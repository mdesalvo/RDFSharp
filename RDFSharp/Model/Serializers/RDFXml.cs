/*
   Copyright 2012-2015 Marco De Salvo

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
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFXml is responsible for managing serialization to and from Xml data format.
    /// </summary>
    internal static class RDFXml {

        #region Methods
        /// <summary>
        /// Serializes the given graph to the given filepath using Xml data format. 
        /// </summary>
        internal static void Serialize(RDFGraph graph, String filepath) {
            try {

                #region serialize
                using (XmlTextWriter rdfxmlWriter = new XmlTextWriter(filepath, Encoding.UTF8))  {
                    XmlDocument rdfDoc            = new XmlDocument();
                    rdfxmlWriter.Formatting       = Formatting.Indented;

                    #region xmlDecl
                    XmlDeclaration xmlDecl        = rdfDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    rdfDoc.AppendChild(xmlDecl);
                    #endregion

                    #region rdfRoot
                    XmlNode rdfRoot               = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":RDF", RDFVocabulary.RDF.BASE_URI);
                    XmlAttribute rdfRootNS        = rdfDoc.CreateAttribute("xmlns:" + RDFVocabulary.RDF.PREFIX);
                    XmlText rdfRootNSText         = rdfDoc.CreateTextNode(RDFVocabulary.RDF.BASE_URI);
                    rdfRootNS.AppendChild(rdfRootNSText);
                    rdfRoot.Attributes.Append(rdfRootNS);

                    #region prefixes
                    //Write the graph's prefixes (except for "rdf", which has already been written)
                    graph.GraphMetadata.Namespaces.ForEach(p => {
                        if (!p.Prefix.Equals(RDFVocabulary.RDF.PREFIX, StringComparison.Ordinal) && !p.Prefix.Equals("base", StringComparison.Ordinal)) {
                            XmlAttribute pfRootNS     = rdfDoc.CreateAttribute("xmlns:" + p.Prefix);
                            XmlText pfRootNSText      = rdfDoc.CreateTextNode(p.ToString());
                            pfRootNS.AppendChild(pfRootNSText);
                            rdfRoot.Attributes.Append(pfRootNS);
                        }
                    });
                    //Write the graph's base uri to resolve eventual relative #IDs
                    XmlAttribute pfBaseNS             = rdfDoc.CreateAttribute(RDFVocabulary.XML.PREFIX + ":base");
                    XmlText pfBaseNSText              = rdfDoc.CreateTextNode(graph.Context.ToString());
                    pfBaseNS.AppendChild(pfBaseNSText);
                    rdfRoot.Attributes.Append(pfBaseNS);
                    #endregion

                    #region linq
                    //Group the graph's triples by subj
                    var groupedList =  (from    triple in graph
                                        orderby triple.Subject.ToString()
                                        group   triple by new {
                                            subj = triple.Subject.ToString()
                                        });
                    #endregion

                    #region graph
                    //Iterate over the calculated groups
                    Dictionary<RDFResource, XmlNode> containers = new Dictionary<RDFResource, XmlNode>();
                    
                    //Floating containers have reification subject which is never object of any graph's triple
                    Boolean floatingContainers                  = graph.GraphMetadata.Containers.Keys.Any(k =>
                                                                        graph.Triples.Values.Count(v => v.Object.Equals(k)) == 0);
                    //Floating collections have reification subject which is never object of any graph's triple
                    Boolean floatingCollections                 = graph.GraphMetadata.Collections.Keys.Any(k => 
                                                                        graph.Triples.Values.Count(v => v.Object.Equals(k)) == 0);

                    foreach (var group in groupedList) {

                        #region subj
                        //Check if the current subj is a container or a collection subj: if so it must be
                        //serialized in the canonical RDF/XML way instead of the "rdf:Description" way
                        XmlNode subjNode              = null;
                        String subj                   = group.Key.subj;

                        //It is a container subj, so add it to the containers pool
                        if (graph.GraphMetadata.Containers.Keys.Any(k => k.ToString().Equals(subj, StringComparison.Ordinal)) && !floatingContainers) {
                            switch (graph.GraphMetadata.Containers.Single(c => c.Key.ToString().Equals(subj, StringComparison.Ordinal)).Value) {
                                case RDFModelEnums.RDFContainerTypes.Bag:
                                    subjNode  = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Bag", RDFVocabulary.RDF.BASE_URI);
                                    containers.Add(new RDFResource(subj), subjNode);
                                    break;
                                case RDFModelEnums.RDFContainerTypes.Seq:
                                    subjNode  = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Seq", RDFVocabulary.RDF.BASE_URI);
                                    containers.Add(new RDFResource(subj), subjNode);
                                    break;
                                case RDFModelEnums.RDFContainerTypes.Alt:
                                    subjNode  = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Alt", RDFVocabulary.RDF.BASE_URI);
                                    containers.Add(new RDFResource(subj), subjNode);
                                    break;
                            }
                        }

                        //It is a subj of a collection of resources, so do not append triples having it as a subject
                        //because we will reconstruct the collection and append it as a whole
                        else if (graph.GraphMetadata.Collections.Keys.Any(k => k.ToString().Equals(subj, StringComparison.Ordinal))                                                         &&
                                 graph.GraphMetadata.Collections.Single(c => c.Key.ToString().Equals(subj, StringComparison.Ordinal)).Value.ItemType == RDFModelEnums.RDFItemTypes.Resource &&
                                 !floatingCollections) {
                            continue;
                        }

                        //It is neither a container or a collection subj
                        else {
                            subjNode                       = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Description", RDFVocabulary.RDF.BASE_URI);
                            //<rdf:Description rdf:nodeID="blankID">
                            XmlAttribute subjNodeDesc      = null;
                            XmlText subjNodeDescText       = rdfDoc.CreateTextNode(group.Key.subj);
                            if (group.Key.subj.StartsWith("bnode:")) {
                                subjNodeDescText.InnerText = subjNodeDescText.InnerText.Replace("bnode:", String.Empty);
                                subjNodeDesc               = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":nodeID", RDFVocabulary.RDF.BASE_URI);
                            }
                            //<rdf:Description rdf:about="subjURI">
                            else {
                                subjNodeDesc               = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":about", RDFVocabulary.RDF.BASE_URI);
                            }
                            subjNodeDesc.AppendChild(subjNodeDescText);
                            subjNode.Attributes.Append(subjNodeDesc);
                        }
                        #endregion

                        #region predObjList
                        //Iterate over the triples of the current group
                        foreach (var triple in group) {

                            //Do not append the triple if it is "SUBJECT rdf:type rdf:[Bag|Seq|Alt]" 
                            if (!(triple.Predicate.Equals(RDFVocabulary.RDF.TYPE) &&
                                  (subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.Ordinal) ||
                                   subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.Ordinal) ||
                                   subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.Ordinal)))) {

                                #region pred
                                String predString     = triple.Predicate.ToString();
                                //"<predPREF:predURI"
                                RDFNamespace predNS   = 
								    (RDFNamespaceRegister.GetByNamespace(predString) ?? 
									     RDFModelUtilities.GenerateNamespace(predString, false));
                                //Refine the pred with eventually necessary sanitizations
                                String predUri        = predString.Replace(predNS.ToString(), predNS.Prefix + ":")
                                                                  .Replace(":#", ":")
                                                                  .TrimEnd(new Char[] { ':', '/' });
                                //Sanitize eventually detected automatic namespace
                                if (predUri.StartsWith("autoNS:")) {
                                    predUri           = predUri.Replace("autoNS:", string.Empty);
                                }
                                //Do not write "xmlns" attribute if the predUri is the context of the graph
                                XmlNode predNode      = null;
                                if (predNS.ToString().Equals(graph.Context.ToString(), StringComparison.Ordinal)) {
                                    predNode          = rdfDoc.CreateNode(XmlNodeType.Element, predUri, null);
                                }
                                else {
                                    predNode          = rdfDoc.CreateNode(XmlNodeType.Element, predUri, predNS.ToString());
                                }
                                #endregion

                                #region object
                                if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {

                                    //If the object is a container subj, we must append its entire node saved in the containers dictionary
                                    if (containers.Keys.Any(k => k.Equals(triple.Object)) && !floatingContainers) {
                                        predNode.AppendChild(containers.Single(c => c.Key.Equals(triple.Object)).Value);
                                    }

                                    //Else, if the object is a subject of a collection of resources, we must append the "rdf:parseType=Collection" attribute to the predicate node
                                    else if (graph.GraphMetadata.Collections.Keys.Any(k => k.Equals(triple.Object))                                                                                     &&
                                             graph.GraphMetadata.Collections.Single(c => c.Key.Equals(triple.Object)).Value.ItemType == RDFModelEnums.RDFItemTypes.Resource &&
                                             !floatingCollections) {
                                        XmlAttribute rdfParseType = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":parseType", RDFVocabulary.RDF.BASE_URI);
                                        XmlText rdfParseTypeText  = rdfDoc.CreateTextNode("Collection");
                                        rdfParseType.AppendChild(rdfParseTypeText);
                                        predNode.Attributes.Append(rdfParseType);
                                        //Then we append sequentially the collection elements 
                                        List<XmlNode> collElements = RDFModelUtilities.ReconstructCollection(graph.GraphMetadata, (RDFResource)triple.Object, rdfDoc);
                                        collElements.ForEach(c => predNode.AppendChild(c)); 
                                    }

                                    //Else, threat it as a traditional object node
                                    else {
                                        String objString               = triple.Object.ToString();
                                        XmlAttribute predNodeDesc      = null;
                                        XmlText predNodeDescText       = rdfDoc.CreateTextNode(objString);
                                        //  rdf:nodeID="blankID">
                                        if (objString.StartsWith("bnode:")) {
                                            predNodeDescText.InnerText = predNodeDescText.InnerText.Replace("bnode:", String.Empty);  
                                            predNodeDesc               = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":nodeID", RDFVocabulary.RDF.BASE_URI);
                                        }
                                        //  rdf:resource="objURI">
                                        else {
                                            predNodeDesc               = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":resource", RDFVocabulary.RDF.BASE_URI);
                                        }
                                        predNodeDesc.AppendChild(predNodeDescText);
                                        predNode.Attributes.Append(predNodeDesc);
                                    }
                                }
                                #endregion

                                #region literal
                                else {

                                    #region plain literal
                                    if (triple.Object is RDFPlainLiteral) {
                                        RDFPlainLiteral pLit      = (RDFPlainLiteral)triple.Object;
                                        //  xml:lang="plitLANG">
                                        if (pLit.Language        != String.Empty) {
                                            XmlAttribute plainLiteralLangNodeDesc = rdfDoc.CreateAttribute(RDFVocabulary.XML.PREFIX + ":lang", RDFVocabulary.XML.BASE_URI);
                                            XmlText plainLiteralLangNodeDescText  = rdfDoc.CreateTextNode(pLit.Language);
                                            plainLiteralLangNodeDesc.AppendChild(plainLiteralLangNodeDescText);
                                            predNode.Attributes.Append(plainLiteralLangNodeDesc);
                                        }
                                    }
                                    #endregion

                                    #region typed literal
                                    //  rdf:datatype="tlitURI">
                                    else {
                                        RDFTypedLiteral tLit      = (RDFTypedLiteral)triple.Object;
                                        XmlAttribute typedLiteralNodeDesc = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":datatype", RDFVocabulary.RDF.BASE_URI);
                                        XmlText typedLiteralNodeDescText  = rdfDoc.CreateTextNode(tLit.Datatype.ToString());
                                        typedLiteralNodeDesc.AppendChild(typedLiteralNodeDescText);
                                        predNode.Attributes.Append(typedLiteralNodeDesc);
                                    }
                                    #endregion

                                    //litVALUE</predPREF:predURI>"
                                    XmlText litNodeDescText       = rdfDoc.CreateTextNode(((RDFLiteral)triple.Object).Value);
                                    predNode.AppendChild(litNodeDescText);
                                }
                                #endregion

                                subjNode.AppendChild(predNode);
                            }

                        }

                        //Raw containers must not be written as-is, instead they have to be saved
                        //and attached when their subj is found later as object of a triple
                        if (!subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.Ordinal) &&
                            !subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.Ordinal) &&
                            !subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.Ordinal)) {
                            rdfRoot.AppendChild(subjNode);
                        }
                        #endregion

                    }
                    #endregion

                    rdfDoc.AppendChild(rdfRoot);
                    #endregion

                    rdfDoc.Save(rdfxmlWriter);
                }
                #endregion

            }
            catch (Exception ex) {
                throw new RDFModelException("Cannot serialize Xml because: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Deserializes the given Xml filepath to a graph. 
        /// </summary>
        internal static RDFGraph Deserialize(String filepath) {
            try {

                #region deserialize
                XmlReaderSettings xrs       = new XmlReaderSettings(); 
                xrs.IgnoreComments          = true;
                xrs.DtdProcessing           = DtdProcessing.Ignore;

                RDFGraph result             = new RDFGraph();
                using(XmlReader xr          = XmlReader.Create(new StreamReader(filepath, Encoding.UTF8), xrs)) {

                    #region load
                    XmlDocument xmlDoc      = new XmlDocument();
                    xmlDoc.Load(xr);
                    #endregion

                    #region root
                    //Prepare the namespace table for the Xml selections
                    XmlNamespaceManager nsMgr         = new XmlNamespaceManager(new NameTable());
                    nsMgr.AddNamespace(RDFVocabulary.RDF.PREFIX, RDFVocabulary.RDF.BASE_URI);

                    //Select "rdf:RDF" root node
                    XmlNode rdfRDF                    = RDFModelUtilities.GetRdfRootNode(xmlDoc, nsMgr);
                    #endregion

                    #region prefixes
                    //Select "xmlns" attributes and try to add them to the namespace register
                    XmlAttributeCollection xmlnsAttrs = RDFModelUtilities.GetXmlnsNamespaces(rdfRDF, nsMgr);
                        
                    //Try to get the "xml:base" attribute, which is needed to resolve eventual relative #IDs in "rdf:about" nodes
                    //If it is not found, set it to the graph Uri
                    Uri xmlBase                       = null;
                    if (xmlnsAttrs                   != null && xmlnsAttrs.Count > 0) {
                        XmlAttribute xmlBaseAttr      = (rdfRDF.Attributes["xml:base"] ?? rdfRDF.Attributes["xmlns"]);
                        if (xmlBaseAttr              != null) {
                            xmlBase                   = RDFModelUtilities.GetUriFromString(xmlBaseAttr.Value);
                        }                        
                    }
                    //Always keep in synch the Context and the xmlBase
                    if (xmlBase                      != null) {
                        result.Context                = xmlBase;
                    }
                    else {
                        xmlBase                       = result.Context;
                    }
                    #endregion

                    #region elements
                    //Parse resource elements, which are the childs of root node and represent the subjects
                    if (rdfRDF.HasChildNodes) {
                        IEnumerator subjNodesEnum     = rdfRDF.ChildNodes.GetEnumerator();
                        while (subjNodesEnum != null && subjNodesEnum.MoveNext()) {
                                
                            #region subj
                            //Get the current resource node
                            XmlNode subjNode          = (XmlNode)subjNodesEnum.Current;
                            RDFResource subj          = RDFModelUtilities.GetSubjectNode(subjNode, xmlBase, result);
                            if (subj == null) {
                                continue;
                            }
                            #endregion

                            #region predObjList
                            //Parse pred elements, which are the childs of subj element
                            if (subjNode.HasChildNodes) {
                                IEnumerator predNodesEnum     = subjNode.ChildNodes.GetEnumerator();
                                while (predNodesEnum != null && predNodesEnum.MoveNext()) {
                                        
                                    //Get the current pred node
                                    RDFResource pred          = null;
                                    XmlNode predNode          = (XmlNode)predNodesEnum.Current;
                                    if (predNode.NamespaceURI == String.Empty) {
                                        pred                  = new RDFResource(xmlBase + predNode.LocalName);
                                    }
                                    else { 
                                        pred                  = (predNode.LocalName.StartsWith("autoNS")   ? 
                                                                    new RDFResource(predNode.NamespaceURI) : 
                                                                    new RDFResource(predNode.NamespaceURI + predNode.LocalName));
                                    }

                                    #region object
                                    //Check if there is a "rdf:about" or a "rdf:resource" attribute
                                    XmlAttribute rdfObject    = 
                                        (RDFModelUtilities.GetRdfAboutAttribute(predNode) ?? 
                                            RDFModelUtilities.GetRdfResourceAttribute(predNode));
                                    if (rdfObject != null) {
                                        //Attribute found, but we must check if it is "rdf:ID", "rdf:nodeID" or a relative Uri
                                        String rdfObjectValue = RDFModelUtilities.ResolveRelativeNode(rdfObject, xmlBase);
                                        RDFResource  obj      = new RDFResource(rdfObjectValue);
                                        result.AddTriple(new RDFTriple(subj, pred, obj));
                                        continue;
                                    }
                                    #endregion

                                    #region typed literal
                                    //Check if there is a "rdf:datatype" attribute
                                    XmlAttribute rdfDatatype  = RDFModelUtilities.GetRdfDatatypeAttribute(predNode);
                                    if (rdfDatatype != null) {
                                        RDFDatatype dt        = RDFModelUtilities.GetDatatypeFromString(rdfDatatype.Value);
                                        RDFTypedLiteral tLit  = new RDFTypedLiteral(HttpUtility.HtmlDecode(predNode.InnerText), dt);
                                        result.AddTriple(new RDFTriple(subj, pred, tLit));
                                        continue;
                                    }
									//Check if there is a "rdf:parseType=Literal" attribute
                                    XmlAttribute parseLiteral = RDFModelUtilities.GetParseTypeLiteralAttribute(predNode);
                                    if (parseLiteral != null) {
                                        RDFTypedLiteral tLit  = new RDFTypedLiteral(HttpUtility.HtmlDecode(predNode.InnerXml), RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.RDFS.PREFIX, "Literal"));
                                        result.AddTriple(new RDFTriple(subj, pred, tLit));
                                        continue;
                                    }
                                    #endregion

                                    #region plain literal
                                    //Check if there is a "xml:lang" attribute, or if a unique textual child
                                    XmlAttribute xmlLang      = RDFModelUtilities.GetXmlLangAttribute(predNode);
                                    if (xmlLang != null ||  (predNode.HasChildNodes && predNode.ChildNodes.Count == 1 && predNode.ChildNodes[0].NodeType == XmlNodeType.Text)) {
                                        RDFPlainLiteral pLit  = new RDFPlainLiteral(HttpUtility.HtmlDecode(predNode.InnerText), (xmlLang != null ? xmlLang.Value : String.Empty));
                                        result.AddTriple(new RDFTriple(subj, pred, pLit));
                                        continue;
                                    }
                                    #endregion

                                    #region collection
                                    //Check if there is a "rdf:parseType=Collection" attribute
                                    XmlAttribute rdfCollect   = RDFModelUtilities.GetParseTypeCollectionAttribute(predNode);
                                    if (rdfCollect           != null) {
                                        RDFModelUtilities.ParseCollectionElements(xmlBase, predNode, subj, pred, result);
                                        continue;
                                    }
                                    #endregion

                                    #region container
                                    //Check if there is a "rdf:[Bag|Seq|Alt]" child node
                                    XmlNode container        = RDFModelUtilities.GetContainerNode(predNode);
                                    if (container != null) {
                                        //Distinguish the right type of RDF container to build
                                        if (container.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.Ordinal)      || container.LocalName.Equals("Bag", StringComparison.Ordinal)) {
                                                RDFModelUtilities.ParseContainerElements(RDFModelEnums.RDFContainerTypes.Bag, container, subj, pred, result);
                                        }
                                        else if (container.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.Ordinal) || container.LocalName.Equals("Seq", StringComparison.Ordinal)) {
                                            RDFModelUtilities.ParseContainerElements(RDFModelEnums.RDFContainerTypes.Seq, container, subj, pred, result);
                                        }
                                        else if (container.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.Ordinal) || container.LocalName.Equals("Alt", StringComparison.Ordinal)) {
                                            RDFModelUtilities.ParseContainerElements(RDFModelEnums.RDFContainerTypes.Alt, container, subj, pred, result);
                                        }                                        
                                    }
                                    #endregion

                                }
                            }
                            #endregion

                        }
                    }
                    #endregion

                }
                return result;
                #endregion

            }
            catch (Exception ex) {
                throw new RDFModelException("Cannot deserialize Xml because: " + ex.Message, ex);
            }
        }
        #endregion

    }

}