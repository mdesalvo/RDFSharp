﻿/*
   Copyright 2012-2020 Marco De Salvo

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
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFXml is responsible for managing serialization to and from XML data format.
    /// </summary>
    internal static class RDFXml
    {

        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given graph to the given filepath using XML data format.
        /// </summary>
        internal static void Serialize(RDFGraph graph, string filepath)
        {
            Serialize(graph, new FileStream(filepath, FileMode.Create));
        }

        /// <summary>
        /// Serializes the given graph to the given stream using XML data format.
        /// </summary>
        internal static void Serialize(RDFGraph graph, Stream outputStream)
        {
            try
            {

                #region serialize
                using (XmlTextWriter rdfxmlWriter = new XmlTextWriter(outputStream, Encoding.UTF8))
                {
                    XmlDocument rdfDoc = new XmlDocument();
                    rdfxmlWriter.Formatting = Formatting.Indented;

                    #region xmlDecl
                    XmlDeclaration xmlDecl = rdfDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    rdfDoc.AppendChild(xmlDecl);
                    #endregion

                    #region rdfRoot
                    XmlNode rdfRoot = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":RDF", RDFVocabulary.RDF.BASE_URI);
                    XmlAttribute rdfRootNS = rdfDoc.CreateAttribute("xmlns:" + RDFVocabulary.RDF.PREFIX);
                    XmlText rdfRootNSText = rdfDoc.CreateTextNode(RDFVocabulary.RDF.BASE_URI);
                    rdfRootNS.AppendChild(rdfRootNSText);
                    rdfRoot.Attributes.Append(rdfRootNS);

                    #region prefixes
                    //Write the prefixes (except for "rdf" and "base")
                    var graphNamespaces = RDFModelUtilities.GetGraphNamespaces(graph);
                    var autoNamespaces = GetAutomaticNamespaces(graph);
                    graphNamespaces.Union(autoNamespaces).ToList().ForEach(p =>
                    {
                        if (!p.NamespacePrefix.Equals(RDFVocabulary.RDF.PREFIX, StringComparison.OrdinalIgnoreCase)
                                && !p.NamespacePrefix.Equals("base", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlAttribute pfRootNS = rdfDoc.CreateAttribute("xmlns:" + p.NamespacePrefix);
                            XmlText pfRootNSText = rdfDoc.CreateTextNode(p.ToString());
                            pfRootNS.AppendChild(pfRootNSText);
                            rdfRoot.Attributes.Append(pfRootNS);
                        }
                    });
                    //Write the graph's base uri to resolve eventual relative #IDs
                    XmlAttribute pfBaseNS = rdfDoc.CreateAttribute(RDFVocabulary.XML.PREFIX + ":base");
                    XmlText pfBaseNSText = rdfDoc.CreateTextNode(graph.Context.ToString());
                    pfBaseNS.AppendChild(pfBaseNSText);
                    rdfRoot.Attributes.Append(pfBaseNS);
                    #endregion

                    #region cont/coll
                    //Fetch data describing containers of the graph
                    var containers = graph.SelectTriplesByObject(RDFVocabulary.RDF.ALT)
                                                         .UnionWith(graph.SelectTriplesByObject(RDFVocabulary.RDF.BAG))
                                                         .UnionWith(graph.SelectTriplesByObject(RDFVocabulary.RDF.SEQ))
                                                         .SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)
                                                         .Select(x => new
                                                         {
                                                             ContainerUri = (RDFResource)x.Subject,
                                                             ContainerType = (x.Object.Equals(RDFVocabulary.RDF.ALT) ? RDFModelEnums.RDFContainerTypes.Alt :
                                                                                x.Object.Equals(RDFVocabulary.RDF.BAG) ? RDFModelEnums.RDFContainerTypes.Bag :
                                                                                                                         RDFModelEnums.RDFContainerTypes.Seq)
                                                         }).ToList();
                    //Fetch data describing collections of the graph
                    var collections = graph.SelectTriplesByObject(RDFVocabulary.RDF.LIST)
                                                         .SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE)
                                                         .Select(x => new
                                                         {
                                                             CollectionUri = (RDFResource)x.Subject,
                                                             CollectionValue = graph.SelectTriplesBySubject((RDFResource)x.Subject)
                                                                                    .SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST)
                                                                                    .FirstOrDefault()?.Object,
                                                             CollectionNext = graph.SelectTriplesBySubject((RDFResource)x.Subject)
                                                                                    .SelectTriplesByPredicate(RDFVocabulary.RDF.REST)
                                                                                    .FirstOrDefault()?.Object,
                                                         }).ToList();
                    #endregion

                    #region linq
                    //Group the graph's triples by subject (containers must be handled first)
                    var groupedList = graph.GroupBy(x => x.Subject.ToString())
                                           .OrderByDescending(x => containers.Any(c => c.ContainerUri.ToString().Equals(x.Key)));
                    #endregion

                    #region graph
                    Dictionary<long, XmlNode> containersXML = new Dictionary<long, XmlNode>();

                    //Floating containers have reification subject which is never object of any graph's triple
                    bool floatingContainers = containers.Any(k => !graph.Triples.Any(v => v.Value.Object.Equals(k.ContainerUri)));
                    //Floating collections have reification subject which is never object of any graph's triple
                    bool floatingCollections = collections.Any(k => !graph.Triples.Any(v => v.Value.Object.Equals(k.CollectionUri)));

                    //Iterate over the calculated groups
                    foreach (var group in groupedList)
                    {

                        #region subj
                        //Check if the current subj is a container or a collection subj: if so, it must be
                        //serialized in abbreviation RDF/XML syntax instead of canonical "rdf:Description"
                        XmlNode subjNode = null;
                        string subj = group.Key;
                        long subjHash = RDFModelUtilities.CreateHash(subj);
                        var subjContainer = containers.Find(x => x.ContainerUri.PatternMemberID == subjHash);
                        var subjCollection = collections.Find(x => x.CollectionUri.PatternMemberID == subjHash);

                        //It is a container subj, so add it to the containersXML pool
                        if (subjContainer != null && !floatingContainers)
                        {
                            switch (subjContainer.ContainerType)
                            {
                                case RDFModelEnums.RDFContainerTypes.Bag:
                                    subjNode = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Bag", RDFVocabulary.RDF.BASE_URI);
                                    break;
                                case RDFModelEnums.RDFContainerTypes.Seq:
                                    subjNode = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Seq", RDFVocabulary.RDF.BASE_URI);
                                    break;
                                case RDFModelEnums.RDFContainerTypes.Alt:
                                    subjNode = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Alt", RDFVocabulary.RDF.BASE_URI);
                                    break;
                            }
                            containersXML.Add(subjHash, subjNode);
                        }

                        //It is a collection subj (of resources), so do not append its triples because
                        //we will reconstruct the collection and append it as "rdf:parseType=Collections"
                        else if (subjCollection != null && subjCollection.CollectionValue is RDFResource && !floatingCollections)
                        {
                            continue;
                        }

                        //It is a traditional subject
                        else
                        {
                            subjNode = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Description", RDFVocabulary.RDF.BASE_URI);
                            //<rdf:Description rdf:nodeID="blankID">
                            XmlAttribute subjNodeDesc = null;
                            XmlText subjNodeDescText = rdfDoc.CreateTextNode(group.Key);
                            if (group.Key.StartsWith("bnode:"))
                            {
                                subjNodeDesc = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":nodeID", RDFVocabulary.RDF.BASE_URI);
                            }
                            //<rdf:Description rdf:about="subjURI">
                            else
                            {
                                subjNodeDesc = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":about", RDFVocabulary.RDF.BASE_URI);
                            }
                            subjNodeDesc.AppendChild(subjNodeDescText);
                            subjNode.Attributes.Append(subjNodeDesc);
                        }
                        #endregion

                        #region predObjList
                        //Iterate over the triples of the current group
                        foreach (var triple in group)
                        {

                            //Do not append the triple if it is "SUBJECT rdf:type rdf:[Bag|Seq|Alt]"
                            if (!(triple.Predicate.Equals(RDFVocabulary.RDF.TYPE) &&
                                  (subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.OrdinalIgnoreCase)
                                    || subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.OrdinalIgnoreCase)
                                       || subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.OrdinalIgnoreCase))))
                            {

                                #region pred
                                string predString = triple.Predicate.ToString();
                                //"<predPREF:predURI"
                                RDFNamespace predNS = RDFNamespaceRegister.GetByUri(predString) ?? GenerateNamespace(predString, false);
                                string predUri = (predNS.NamespacePrefix.Equals("autoNS", StringComparison.OrdinalIgnoreCase) ?
                                                            predString.Replace(predNS.ToString(), autoNamespaces.Find(ns => ns.NamespaceUri.Equals(predNS.NamespaceUri)).NamespacePrefix + ":") :
                                                            predString.Replace(predNS.ToString(), predNS.NamespacePrefix + ":"))
                                                        .Replace(":#", ":")
                                                        .TrimEnd(new char[] { ':', '/' });
                                try
                                {
                                    var predUriQName = new RDFTypedLiteral(predUri, RDFModelEnums.RDFDatatypes.XSD_QNAME);
                                }
                                catch
                                {
                                    throw new RDFModelException(string.Format("found '{0}' predicate which cannot be abbreviated to a valid QName", predUri));
                                }
                                XmlNode predNode = rdfDoc.CreateNode(XmlNodeType.Element, predUri, predNS.ToString());
                                #endregion

                                #region object
                                if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                {
                                    var containerObj = containers.Find(x => x.ContainerUri.Equals(triple.Object));
                                    var collectionObj = collections.Find(x => x.CollectionUri.Equals(triple.Object));

                                    //Object is a container subj: we must append its node saved in the containersXML dictionary
                                    if (containerObj != null && !floatingContainers)
                                    {
                                        predNode.AppendChild(containersXML[containerObj.ContainerUri.PatternMemberID]);
                                    }

                                    //Object is a collection subj (of resources): we must append its "rdf:parseType=Collection" representation
                                    else if (collectionObj != null && collectionObj.CollectionValue is RDFResource && !floatingCollections)
                                    {

                                        //Append "rdf:parseType=Collection" attribute
                                        XmlAttribute rdfParseType = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":parseType", RDFVocabulary.RDF.BASE_URI);
                                        XmlText rdfParseTypeText = rdfDoc.CreateTextNode("Collection");
                                        rdfParseType.AppendChild(rdfParseTypeText);
                                        predNode.Attributes.Append(rdfParseType);

                                        //Append "rdf:parseType=Collection" elements
                                        bool nilFound = false;
                                        RDFResource currentCollItem = (RDFResource)triple.Object;
                                        List<XmlNode> collElements = new List<XmlNode>();
                                        XmlNode collElementToAppend = null;
                                        XmlAttribute collElementAttr = null;
                                        XmlText collElementAttrText = null;
                                        while (!nilFound)
                                        {
                                            var collElement = collections.Find(x => x.CollectionUri.Equals(currentCollItem));
                                            if (collElement == null || collElement.CollectionValue == null || collElement.CollectionNext == null)
                                            {
                                                throw new RDFModelException(string.Format("Collection having '{0}' as subject is not well-formed. Please check presence of its 'rdf:type/rdf:first/rdf:rest' triples.", currentCollItem));
                                            }
                                            collElementToAppend = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Description", RDFVocabulary.RDF.BASE_URI);
                                            collElementAttr = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":about", RDFVocabulary.RDF.BASE_URI);
                                            collElementAttrText = rdfDoc.CreateTextNode(collElement.CollectionValue.ToString());
                                            if (collElementAttrText.InnerText.StartsWith("bnode:"))
                                            {
                                                collElementAttrText.InnerText = collElementAttrText.InnerText.Replace("bnode:", string.Empty);
                                            }
                                            collElementAttr.AppendChild(collElementAttrText);
                                            collElementToAppend.Attributes.Append(collElementAttr);
                                            collElements.Add(collElementToAppend);

                                            //Verify if this is the last element of the collection (pointing to next="rdf:nil")
                                            if (collElement.CollectionNext.Equals(RDFVocabulary.RDF.NIL))
                                            {
                                                nilFound = true;
                                            }
                                            else
                                            {
                                                currentCollItem = (RDFResource)collElement.CollectionNext;
                                            }
                                        }
                                        collElements.ForEach(c => predNode.AppendChild(c));

                                    }

                                    //Object is traditional
                                    else
                                    {
                                        string objString = triple.Object.ToString();
                                        XmlAttribute predNodeDesc = null;
                                        XmlText predNodeDescText = rdfDoc.CreateTextNode(objString);
                                        //  rdf:nodeID="blankID">
                                        if (objString.StartsWith("bnode:"))
                                        {
                                            predNodeDesc = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":nodeID", RDFVocabulary.RDF.BASE_URI);
                                        }
                                        //  rdf:resource="objURI">
                                        else
                                        {
                                            predNodeDesc = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":resource", RDFVocabulary.RDF.BASE_URI);
                                        }
                                        predNodeDesc.AppendChild(predNodeDescText);
                                        predNode.Attributes.Append(predNodeDesc);
                                    }
                                }
                                #endregion

                                #region literal
                                else
                                {

                                    #region plain literal
                                    if (triple.Object is RDFPlainLiteral)
                                    {
                                        RDFPlainLiteral pLit = (RDFPlainLiteral)triple.Object;
                                        //  xml:lang="plitLANG">
                                        if (pLit.Language != string.Empty)
                                        {
                                            XmlAttribute plainLiteralLangNodeDesc = rdfDoc.CreateAttribute(RDFVocabulary.XML.PREFIX + ":lang", RDFVocabulary.XML.BASE_URI);
                                            XmlText plainLiteralLangNodeDescText = rdfDoc.CreateTextNode(pLit.Language);
                                            plainLiteralLangNodeDesc.AppendChild(plainLiteralLangNodeDescText);
                                            predNode.Attributes.Append(plainLiteralLangNodeDesc);
                                        }
                                    }
                                    #endregion

                                    #region typed literal
                                    //  rdf:datatype="tlitURI">
                                    else
                                    {
                                        RDFTypedLiteral tLit = (RDFTypedLiteral)triple.Object;
                                        XmlAttribute typedLiteralNodeDesc = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":datatype", RDFVocabulary.RDF.BASE_URI);
                                        XmlText typedLiteralNodeDescText = rdfDoc.CreateTextNode(RDFModelUtilities.GetDatatypeFromEnum(tLit.Datatype));
                                        typedLiteralNodeDesc.AppendChild(typedLiteralNodeDescText);
                                        predNode.Attributes.Append(typedLiteralNodeDesc);
                                    }
                                    #endregion

                                    //litVALUE</predPREF:predURI>"
                                    XmlText litNodeDescText = rdfDoc.CreateTextNode(RDFModelUtilities.EscapeControlCharsForXML(HttpUtility.HtmlDecode(((RDFLiteral)triple.Object).Value)));
                                    predNode.AppendChild(litNodeDescText);
                                }
                                #endregion

                                subjNode.AppendChild(predNode);
                            }

                        }

                        //Raw containers must not be written as-is, instead they have to be saved
                        //and attached when their subj is found later as object of a triple
                        if (!subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.OrdinalIgnoreCase)
                                && !subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.OrdinalIgnoreCase)
                                    && !subjNode.Name.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.OrdinalIgnoreCase))
                        {
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
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot serialize RDF/Xml because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given Xml filepath to a graph.
        /// </summary>
        internal static RDFGraph Deserialize(string filepath)
        {
            return Deserialize(new FileStream(filepath, FileMode.Open));
        }

        /// <summary>
        /// Deserializes the given Xml stream to a graph.
        /// </summary>
        internal static RDFGraph Deserialize(Stream inputStream)
        {
            try
            {

                #region deserialize
                RDFGraph result = new RDFGraph();
                using (StreamReader streamReader = new StreamReader(inputStream, Encoding.UTF8))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(streamReader))
                    {
                        xmlReader.DtdProcessing = DtdProcessing.Parse;
                        xmlReader.Normalization = false;

                        #region document
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(xmlReader);
                        #endregion

                        #region root
                        //Prepare the namespace table for the Xml selections
                        var nsMgr = new XmlNamespaceManager(new NameTable());
                        nsMgr.AddNamespace(RDFVocabulary.RDF.PREFIX, RDFVocabulary.RDF.BASE_URI);

                        //Select "rdf:RDF" root node
                        var rdfRDF = GetRdfRootNode(xmlDoc, nsMgr);
                        #endregion

                        #region prefixes
                        //Select "xmlns" attributes and try to add them to the namespace register
                        var xmlnsAttrs = GetXmlnsNamespaces(rdfRDF, nsMgr);

                        //Try to get the "xml:base" attribute, which is needed to resolve eventual relative #IDs in "rdf:about" nodes
                        //If it is not found, set it to the graph Uri
                        Uri xmlBase = null;
                        if (xmlnsAttrs != null && xmlnsAttrs.Count > 0)
                        {
                            var xmlBaseAttr = (rdfRDF.Attributes?[RDFVocabulary.XML.PREFIX + ":base"]
                                                    ?? rdfRDF.Attributes?["xmlns"]);
                            if (xmlBaseAttr != null)
                            {
                                xmlBase = RDFModelUtilities.GetUriFromString(xmlBaseAttr.Value);
                            }
                        }
                        //Always keep in synch the Context and the xmlBase
                        if (xmlBase != null)
                        {
                            result.SetContext(xmlBase);
                        }
                        else
                        {
                            xmlBase = result.Context;
                        }
                        #endregion

                        #region elements
                        //Parse children of root node
                        if (rdfRDF.HasChildNodes)
                            ParseNodeList(rdfRDF.ChildNodes, result, xmlBase, GetXmlLangAttribute(rdfRDF));
                        #endregion

                    }
                }

                return result;
                #endregion

            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot deserialize RDF/Xml because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Parses the given list of nodes
        /// </summary>
        private static List<RDFResource> ParseNodeList(XmlNodeList nodeList, RDFGraph result, Uri xmlBase, XmlAttribute xmlLangParent, RDFResource subjectParent = null)
        {
            var subjects = new List<RDFResource>();
            var subjNodesEnum = nodeList.GetEnumerator();
            while (subjNodesEnum != null && subjNodesEnum.MoveNext())
            {

                #region subject
                //Get the current resource node
                XmlNode subjNode = (XmlNode)subjNodesEnum.Current;

                //Skip subject if it is not an element
                if (subjNode.NodeType != XmlNodeType.Element)
                    continue;

                //Assert resource as subject
                XmlAttribute xmlLangSubj = GetXmlLangAttribute(subjNode) ?? xmlLangParent;
                RDFResource subj = subjectParent ?? GetSubjectNode(subjNode, xmlBase, result);
                if (subj == null)
                    subj = new RDFResource();
                subjects.Add(subj);
                #endregion

                #region predObjList
                //Parse subject attributes
                if (subjNode.Attributes?.Count > 0)
                {
                    List<XmlAttribute> subjAttributes = subjNode.Attributes.OfType<XmlAttribute>().ToList();
                    subjAttributes.ForEach(subjAttr =>
                    {
                        switch (subjAttr.Name.ToLower())
                        {
                            //Skip reserved attributes
                            case "rdf:about":
                            case "rdf:resource":
                            case "rdf:parsetype":
                            case "rdf:id":
                            case "rdf:nodeid":
                            case "xml:lang":
                                break;

                            //Threat rdf:type attribute as SPO
                            case "rdf:type":
                                Uri rdfTypeValue = RDFModelUtilities.GetUriFromString(subjAttr.Value);
                                if (rdfTypeValue != null)
                                {
                                    RDFResource obj = new RDFResource(rdfTypeValue.ToString());
                                    result.AddTriple(new RDFTriple(subj, RDFVocabulary.RDF.TYPE, obj));
                                }
                                break;

                            //Threat other attributes as SPL
                            default:
                                RDFResource subjAttrPred = subjAttr.NamespaceURI == string.Empty ? new RDFResource(xmlBase + subjAttr.LocalName)
                                                                                                 : new RDFResource(subjAttr.NamespaceURI + subjAttr.LocalName);
                                RDFPlainLiteral plit = new RDFPlainLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(subjAttr.Value)), xmlLangSubj?.Value);
                                result.AddTriple(new RDFTriple(subj, subjAttrPred, plit));
                                break;
                        }
                    });
                }

                //Parse subject children (predicates)
                if (subjNode.HasChildNodes)
                {
                    IEnumerator predNodesEnum = subjNode.ChildNodes.GetEnumerator();
                    while (predNodesEnum != null && predNodesEnum.MoveNext())
                    {

                        #region predicate
                        //Get the current pred node
                        RDFResource pred = null;
                        XmlNode predNode = (XmlNode)predNodesEnum.Current;

                        //Skip predicate if it is not an element
                        if (predNode.NodeType != XmlNodeType.Element)
                            continue;

                        //Get the predicate
                        XmlAttribute xmlLangPred = GetXmlLangAttribute(predNode) ?? xmlLangSubj;
                        if (predNode.NamespaceURI == string.Empty)
                            pred = new RDFResource(xmlBase + predNode.LocalName);
                        else
                            pred = (predNode.LocalName.StartsWith("autoNS") ? new RDFResource(predNode.NamespaceURI)
                                                                            : new RDFResource(predNode.NamespaceURI + predNode.LocalName));
                        #endregion

                        #region objList

                        #region collection
                        //Check if predicate has "rdf:parseType=Collection" attribute
                        XmlAttribute rdfCollect = GetParseTypeCollectionAttribute(predNode);
                        if (rdfCollect != null)
                        {
                            ParseCollectionElements(xmlBase, predNode, subj, pred, result, xmlLangPred);
                            continue;
                        }
                        #endregion

                        #region container
                        //Check if predicate has "rdf:[Bag|Seq|Alt]" child node
                        XmlNode container = GetContainerNode(predNode);
                        if (container != null)
                        {
                            //Distinguish the right type of RDF container to build
                            if (container.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.OrdinalIgnoreCase)
                                    || container.LocalName.Equals("Bag", StringComparison.OrdinalIgnoreCase))
                                ParseContainerElements(RDFModelEnums.RDFContainerTypes.Bag, container, subj, pred, result);
                            else if (container.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.OrdinalIgnoreCase)
                                    || container.LocalName.Equals("Seq", StringComparison.OrdinalIgnoreCase))
                                ParseContainerElements(RDFModelEnums.RDFContainerTypes.Seq, container, subj, pred, result);
                            else if (container.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.OrdinalIgnoreCase)
                                    || container.LocalName.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                                ParseContainerElements(RDFModelEnums.RDFContainerTypes.Alt, container, subj, pred, result);
                            continue;
                        }
                        #endregion

                        #region object
                        //Check if predicate has one of these specific attributes:
                        //"rdf:about", "rdf:resource", "rdf:nodeID", "rdf:ID", "rdf:parseType=Resource"
                        XmlAttribute rdfObject = (GetRdfAboutAttribute(predNode)
                                                    ?? GetRdfResourceAttribute(predNode)
                                                        ?? GetParseTypeResourceAttribute(predNode));
                        if (rdfObject != null)
                        {
                            #region rdf:parseType=Resource
                            if (rdfObject.Name.ToLower().Equals("rdf:parsetype"))
                            {
                                //"rdf:parseType=Resource" appends a triple with blank object,
                                //which represents the subject of nested resource descriptions
                                RDFResource parsetypeResource = new RDFResource();
                                RDFTriple parsetypeResourceTriple = new RDFTriple(subj, pred, parsetypeResource);
                                result.AddTriple(parsetypeResourceTriple);

                                //Check if predicate has children (nested resource descriptions)
                                if (predNode.HasChildNodes)
                                    ParseNodeList(predNode.SelectNodes("."), result, xmlBase, xmlLangPred, parsetypeResource);
                            }
                            #endregion

                            #region rdf:about,rdf:resource,rdf:ID,rdf:nodeID
                            else
                            {
                                string objValue = ResolveRelativeNode(rdfObject, xmlBase);
                                RDFResource obj = new RDFResource(objValue);

                                #region rdf:ID
                                if (rdfObject.Name.ToLower().Equals("rdf:id"))
                                {
                                    //"rdf:ID" at predicate level appends reified triple represented by object
                                    RDFTriple objTriple = new RDFTriple(obj, pred, new RDFPlainLiteral(string.Empty));
                                    foreach (RDFTriple reifObjTriple in ReifyRdfIdPredicateTriple(obj, subj, objTriple))
                                        result.AddTriple(reifObjTriple);
                                }
                                #endregion

                                #region rdf:about,rdf:resource,rdf:nodeID
                                else
                                {
                                    RDFTriple objTriple = new RDFTriple(subj, pred, obj);
                                    result.AddTriple(objTriple);
                                }
                                #endregion
                            }
                            #endregion

                            continue;
                        }
                        #endregion

                        #region typed literal
                        //Check if predicate has "rdf:datatype" attribute
                        XmlAttribute rdfDatatype = GetRdfDatatypeAttribute(predNode);
                        if (rdfDatatype != null)
                        {
                            var dtype = RDFModelUtilities.GetDatatypeFromString(rdfDatatype.Value);
                            RDFTypedLiteral tLit = new RDFTypedLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(predNode.InnerText)), dtype);
                            result.AddTriple(new RDFTriple(subj, pred, tLit));
                            continue;
                        }
                        //Check if predicate has "rdf:parseType=Literal" attribute
                        XmlAttribute parseLiteral = GetParseTypeLiteralAttribute(predNode);
                        if (parseLiteral != null)
                        {
                            RDFTypedLiteral tLit = new RDFTypedLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(predNode.InnerXml)), RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL);
                            result.AddTriple(new RDFTriple(subj, pred, tLit));
                            continue;
                        }
                        #endregion

                        #region plain literal
                        //Check if predicate has a unique textual child
                        if (predNode.HasChildNodes && predNode.ChildNodes.Count == 1 && predNode.ChildNodes[0].NodeType == XmlNodeType.Text)
                        {
                            RDFPlainLiteral pLit = new RDFPlainLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(predNode.InnerText)), xmlLangPred?.Value);
                            result.AddTriple(new RDFTriple(subj, pred, pLit));
                            continue;
                        }
                        #endregion

                        #region nested description
                        //At last, check if predicate has children (nested resource descriptions)
                        if (predNode.HasChildNodes)
                        {
                            var nestedResources = ParseNodeList(predNode.ChildNodes, result, xmlBase, xmlLangPred);
                            foreach (var nestedResource in nestedResources)
                                result.AddTriple(new RDFTriple(subj, pred, nestedResource));
                        }
                        #endregion

                        #endregion

                    }
                }
                #endregion

            }
            return subjects;
        }

        /// <summary>
        /// Gives the "rdf:RDF" root node of the document
        /// </summary>
        private static XmlNode GetRdfRootNode(XmlDocument xmlDoc, XmlNamespaceManager nsMgr)
        {
            XmlNode rdf =
                (xmlDoc.SelectSingleNode(RDFVocabulary.RDF.PREFIX + ":RDF", nsMgr) ??
                    xmlDoc.SelectSingleNode("RDF", nsMgr));

            //Invalid RDF/XML file: root node is neither "rdf:RDF" or "RDF"
            if (rdf == null)
            {
                throw new Exception("Given file has not a valid \"rdf:RDF\" or \"RDF\" root node");
            }

            return rdf;
        }

        /// <summary>
        /// Gives the collection of "xmlns" attributes of the "rdf:RDF" root node
        /// </summary>
        private static XmlAttributeCollection GetXmlnsNamespaces(XmlNode rdfRDF, XmlNamespaceManager nsMgr)
        {
            XmlAttributeCollection xmlns = rdfRDF.Attributes;
            if (xmlns != null && xmlns.Count > 0)
            {

                IEnumerator iEnum = xmlns.GetEnumerator();
                while (iEnum != null && iEnum.MoveNext())
                {
                    XmlAttribute attr = (XmlAttribute)iEnum.Current;
                    if (attr.LocalName.ToUpperInvariant() != "XMLNS"
                            && attr.Name.ToUpperInvariant() != "XML:LANG")
                    {

                        //Try to resolve the current namespace against the namespace register;
                        //if not resolved, create new namespace with scope limited to actual node
                        RDFNamespace ns =
                        (RDFNamespaceRegister.GetByPrefix(attr.LocalName) ??
                                RDFNamespaceRegister.GetByUri(attr.Value) ??
                                    new RDFNamespace(attr.LocalName, attr.Value));

                        nsMgr.AddNamespace(ns.NamespacePrefix, ns.NamespaceUri.ToString());

                    }
                }

            }
            return xmlns;
        }

        /// <summary>
        /// Generates an automatic prefix for a namespace
        /// </summary>
        private static RDFNamespace GenerateNamespace(string namespaceString, bool isDatatypeNamespace)
        {
            if (namespaceString != null && namespaceString.Trim() != string.Empty)
            {

                //Extract the prefixable part from the Uri
                Uri uriNS = RDFModelUtilities.GetUriFromString(namespaceString);
                if (uriNS == null)
                {
                    throw new RDFModelException("Cannot create RDFNamespace because given \"namespaceString\" (" + namespaceString + ") parameter cannot be converted to a valid Uri");
                }
                string fragment = null;
                string nspace = uriNS.AbsoluteUri;

                // e.g.:  "http://www.w3.org/2001/XMLSchema#integer"
                if (uriNS.Fragment != string.Empty)
                {
                    fragment = uriNS.Fragment.Replace("#", string.Empty);           //"integer"
                    if (fragment != string.Empty)
                    {
                        nspace = Regex.Replace(nspace, fragment + "$", string.Empty); //"http://www.w3.org/2001/XMLSchema#"
                    }
                }
                else
                {
                    // e.g.:  "http://example.org/integer"
                    if (uriNS.LocalPath != "/")
                    {
                        if (!isDatatypeNamespace)
                        {
                            nspace = Regex.Replace(nspace, uriNS.Segments[uriNS.Segments.Length - 1] + "$", string.Empty);
                        }
                    }
                }

                //Check if a namespace with the extracted Uri is in the register, or generate an automatic one
                return (RDFNamespaceRegister.GetByUri(nspace) ?? new RDFNamespace("autoNS", nspace));

            }
            throw new RDFModelException("Cannot create RDFNamespace because given \"namespaceString\" parameter is null or empty");
        }

        /// <summary>
        /// Gets the list of automatic namespaces used within the predicates of the triples of the given graph
        /// </summary>
        private static List<RDFNamespace> GetAutomaticNamespaces(RDFGraph graph)
        {
            var result = new List<RDFNamespace>();
            foreach (var p in graph.Triples.Select(x => x.Value.Predicate.ToString()).Distinct())
            {
                var nspace = GenerateNamespace(p, false);

                //Check if the predicate can be abbreviated to a valid QName
                if (!nspace.NamespaceUri.ToString().Equals(p))
                {
                    if (nspace.NamespacePrefix.StartsWith("autoNS"))
                    {
                        if (!result.Contains(nspace))
                        {
                            result.Add(new RDFNamespace("autoNS" + (result.Count + 1), nspace.NamespaceUri.ToString()));
                        }
                    }
                }
                else
                {
                    throw new RDFModelException(string.Format("found '{0}' predicate which cannot be abbreviated to a valid QName.", p));
                }

            }
            return result.ToList();
        }

        /// <summary>
        /// Gives the subj node extracted from the attribute list of the current element
        /// </summary>
        private static RDFResource GetSubjectNode(XmlNode subjNode, Uri xmlBase, RDFGraph result)
        {
            RDFResource subj = null;

            //If there are attributes, search for the one describing the subject
            if (subjNode.Attributes != null && subjNode.Attributes.Count > 0)
            {

                //We are interested in finding the "rdf:about" or "rdf:resource"
                XmlAttribute rdfAbout =
                    (GetRdfAboutAttribute(subjNode)
                        ?? GetRdfResourceAttribute(subjNode));
                if (rdfAbout != null)
                {
                    //Attribute found, but we must check if it is "rdf:ID", "rdf:nodeID" or a relative Uri:
                    //in this case it must be resolved against the xmlBase namespace, or else it remains the same
                    string rdfAboutValue = ResolveRelativeNode(rdfAbout, xmlBase);
                    subj = new RDFResource(rdfAboutValue);
                }

            }
            if (subj == null)
                subj = new RDFResource();

            //We must check if the node is not a standard "rdf:Description": this is
            //the case we can directly build a triple with "rdf:type" pred
            if (!CheckIfRdfDescriptionNode(subjNode))
            {
                RDFResource obj = null;
                if (subjNode.NamespaceURI == string.Empty)
                {
                    obj = new RDFResource(xmlBase + subjNode.LocalName);
                }
                else
                {
                    obj = new RDFResource(subjNode.NamespaceURI + subjNode.LocalName);
                }
                result.AddTriple(new RDFTriple(subj, RDFVocabulary.RDF.TYPE, obj));
            }

            return subj;
        }

        /// <summary>
        /// Checks if the given attribute is absolute Uri, relative Uri, "rdf:ID" relative Uri, "rdf:nodeID" blank node Uri
        /// </summary>
        private static string ResolveRelativeNode(XmlAttribute attr, Uri xmlBase)
        {
            if (attr != null && xmlBase != null)
            {
                string attrValue = attr.Value;

                //"rdf:ID" relative Uri: must be resolved against the xmlBase namespace
                if (attr.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":ID", StringComparison.OrdinalIgnoreCase)
                        || attr.LocalName.Equals("ID", StringComparison.OrdinalIgnoreCase))
                {
                    attrValue = RDFModelUtilities.GetUriFromString(xmlBase + attrValue).ToString();
                }

                //"rdf:nodeID" relative Uri: must be resolved against the "bnode:" prefix
                else if (attr.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":nodeID", StringComparison.OrdinalIgnoreCase)
                            || attr.LocalName.Equals("nodeID", StringComparison.OrdinalIgnoreCase))
                {
                    if (!attrValue.StartsWith("bnode:"))
                    {
                        attrValue = "bnode:" + attrValue;
                    }
                }

                //"rdf:about" or "rdf:resource" relative Uri: must be resolved against the xmlBase namespace
                else if (RDFModelUtilities.GetUriFromString(attrValue) == null)
                {
                    attrValue = RDFModelUtilities.GetUriFromString(xmlBase + attrValue).ToString();
                }

                return attrValue;
            }
            throw new RDFModelException("Cannot resolve relative node because given \"attr\" or \"xmlBase\" parameters are null");
        }

        /// <summary>
        /// Verify if we are on a standard rdf:Description element
        /// </summary>
        private static bool CheckIfRdfDescriptionNode(XmlNode subjNode)
        {
            bool result = subjNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Description", StringComparison.OrdinalIgnoreCase)
                                || subjNode.LocalName.Equals("Description", StringComparison.OrdinalIgnoreCase);

            return result;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF subj
        /// </summary>
        private static XmlAttribute GetRdfAboutAttribute(XmlNode subjNode)
        {
            //rdf:about
            XmlAttribute rdfAbout =
                (subjNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":about"] ??
                    (subjNode.Attributes?["about"] ??
                        (subjNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":nodeID"] ??
                            (subjNode.Attributes?["nodeID"] ??
                                (subjNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":ID"] ??
                                    subjNode.Attributes?["ID"])))));

            return rdfAbout;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF object
        /// </summary>
        private static XmlAttribute GetRdfResourceAttribute(XmlNode predNode)
        {
            //rdf:Resource
            XmlAttribute rdfResource =
                (predNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":resource"] ??
                    (predNode.Attributes?["resource"] ??
                        (predNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":nodeID"] ??
                            predNode.Attributes?["nodeID"])));

            return rdfResource;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF typed literal datatype
        /// </summary>
        private static XmlAttribute GetRdfDatatypeAttribute(XmlNode predNode)
        {
            //rdf:datatype
            XmlAttribute rdfDatatype =
                (predNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":datatype"] ??
                    predNode.Attributes?["datatype"]);

            return rdfDatatype;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF plain literal language
        /// </summary>
        private static XmlAttribute GetXmlLangAttribute(XmlNode predNode)
        {
            //xml:lang
            XmlAttribute xmlLang =
                (predNode.Attributes?[RDFVocabulary.XML.PREFIX + ":lang"] ??
                    predNode.Attributes?["lang"]);

            return xmlLang;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Collection"
        /// </summary>
        private static XmlAttribute GetParseTypeCollectionAttribute(XmlNode predNode)
        {
            XmlAttribute rdfCollection =
                predNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":parseType"] ??
                    predNode.Attributes?["parseType"];

            return (rdfCollection != null && rdfCollection.Value.Equals("Collection", StringComparison.OrdinalIgnoreCase)) ? rdfCollection : null;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Literal"
        /// </summary>
        private static XmlAttribute GetParseTypeLiteralAttribute(XmlNode predNode)
        {
            XmlAttribute rdfLiteral =
                predNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":parseType"] ??
                    predNode.Attributes?["parseType"];

            return (rdfLiteral != null && rdfLiteral.Value.Equals("Literal", StringComparison.OrdinalIgnoreCase)) ? rdfLiteral : null;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Resource"
        /// </summary>
        private static XmlAttribute GetParseTypeResourceAttribute(XmlNode predNode)
        {
            XmlAttribute rdfResource =
                predNode.Attributes?[RDFVocabulary.RDF.PREFIX + ":parseType"] ??
                    predNode.Attributes?["parseType"];

            return ((rdfResource != null && rdfResource.Value.Equals("Resource", StringComparison.OrdinalIgnoreCase)) ? rdfResource : null);
        }

        /// <summary>
        /// Builds the reification graph of the special triple obtained from rdf:ID found at predicate level
        /// </summary>
        private static RDFGraph ReifyRdfIdPredicateTriple(RDFResource reificationSubject, RDFResource subject, RDFTriple triple)
        {
            var reifGraph = new RDFGraph();
            var reifSubj = reificationSubject;

            reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.SUBJECT, subject));
            reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate));
            reifGraph.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.OBJECT, (RDFLiteral)triple.Object));

            return reifGraph;
        }

        /// <summary>
        /// Given an attribute representing a RDF collection, iterates on its constituent elements
        /// to build its standard reification triples.
        /// </summary>
        private static void ParseCollectionElements(Uri xmlBase, XmlNode predNode, RDFResource subj,
                                                    RDFResource pred, RDFGraph result, XmlAttribute xmlLangSubj)
        {
            //Attach the collection as the blank object of the current predicate
            RDFResource obj = new RDFResource();
            result.AddTriple(new RDFTriple(subj, pred, obj));

            //Parse the collection items: since parseType="Collection" is a
            //RDF/XML syntactic sugar available *only* for resource collections,
            //we can iterate detected collection items as resources
            if (predNode.HasChildNodes)
            {
                XmlAttribute xmlLangPred = GetXmlLangAttribute(predNode) ?? xmlLangSubj;
                List<RDFResource> elems = ParseNodeList(predNode.ChildNodes, result, xmlBase, xmlLangPred);
                foreach (RDFResource elem in elems)
                {
                    // obj -> rdf:type -> rdf:list
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));

                    // obj -> rdf:first -> res
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.FIRST, elem));

                    //Last element of a collection must give a triple to a "rdf:nil" object
                    RDFResource newObj;
                    if (elem != elems.Last())
                    {
                        // obj -> rdf:rest -> newObj
                        newObj = new RDFResource();
                    }
                    else
                    {
                        // obj -> rdf:rest -> rdf:nil
                        newObj = RDFVocabulary.RDF.NIL;
                    }
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.REST, newObj));
                    obj = newObj;
                }
            }
        }

        /// <summary>
        /// Verify if we are on a standard rdf:[Bag|Seq|Alt] element
        /// </summary>
        private static bool CheckIfRdfContainerNode(XmlNode containerNode)
        {
            bool result = containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.OrdinalIgnoreCase)
                                || containerNode.LocalName.Equals("Bag", StringComparison.OrdinalIgnoreCase)
                                    || containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.OrdinalIgnoreCase)
                                        || containerNode.LocalName.Equals("Seq", StringComparison.OrdinalIgnoreCase)
                                            || containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.OrdinalIgnoreCase)
                                                || containerNode.LocalName.Equals("Alt", StringComparison.OrdinalIgnoreCase);

            return result;
        }

        /// <summary>
        /// Given an element, return the child element which can correspond to the RDF container
        /// </summary>
        private static XmlNode GetContainerNode(XmlNode predNode)
        {
            //A container is the first child of the given node and it has no attributes.
            //Its localname must be the canonical "rdf:[Bag|Seq|Alt]", so we check for this.
            if (predNode.HasChildNodes)
            {
                XmlNode containerNode = predNode.FirstChild;
                bool isRdfContainer = CheckIfRdfContainerNode(containerNode);
                if (isRdfContainer)
                    return containerNode;
            }

            return null;
        }

        /// <summary>
        /// Given an element representing a RDF container, iterates on its constituent elements
        /// to build its standard reification triples.
        /// </summary>
        private static void ParseContainerElements(RDFModelEnums.RDFContainerTypes contType, XmlNode container,
                                                    RDFResource subj, RDFResource pred, RDFGraph result)
        {

            //Attach the container as the blank object of the current pred
            RDFResource obj = new RDFResource();
            result.AddTriple(new RDFTriple(subj, pred, obj));

            //obj -> rdf:type -> rdf:[Bag|Seq|Alt]
            switch (contType)
            {
                case RDFModelEnums.RDFContainerTypes.Bag:
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.BAG));
                    break;
                case RDFModelEnums.RDFContainerTypes.Seq:
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.SEQ));
                    break;
                default:
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT));
                    break;
            }

            //Iterate on the container items
            if (container.HasChildNodes)
            {
                IEnumerator elems = container.ChildNodes.GetEnumerator();
                List<string> elemVals = new List<string>();
                while (elems != null && elems.MoveNext())
                {
                    XmlNode elem = (XmlNode)elems.Current;

                    //Skip container item if it is not an element
                    if (elem.NodeType != XmlNodeType.Element)
                        continue;

                    XmlAttribute elemUri = GetRdfResourceAttribute(elem);

                    #region Container Resource Item
                    //This is a container of resources
                    if (elemUri != null)
                    {

                        //Sanitize eventual blank node value detected by presence of "nodeID" attribute
                        if (elemUri.LocalName.Equals("nodeID", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!elemUri.Value.StartsWith("bnode:"))
                            {
                                elemUri.Value = "bnode:" + elemUri.Value;
                            }
                        }

                        //obj -> rdf:_N -> VALUE
                        if (contType == RDFModelEnums.RDFContainerTypes.Alt)
                        {
                            if (!elemVals.Contains(elemUri.Value))
                            {
                                elemVals.Add(elemUri.Value);
                                result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), new RDFResource(elemUri.Value)));
                            }
                        }
                        else
                        {
                            result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), new RDFResource(elemUri.Value)));
                        }

                    }
                    #endregion

                    #region Container Literal Item
                    //This is a container of literals
                    else
                    {

                        //Parse the literal contained in the item
                        RDFLiteral literal = null;
                        XmlAttribute attr = GetRdfDatatypeAttribute(elem);
                        if (attr != null)
                        {
                            literal = new RDFTypedLiteral(elem.InnerText, RDFModelUtilities.GetDatatypeFromString(attr.InnerText));
                        }
                        else
                        {
                            attr = GetXmlLangAttribute(elem);
                            literal = new RDFPlainLiteral(elem.InnerText, (attr != null ? attr.InnerText : string.Empty));
                        }

                        //obj -> rdf:_N -> VALUE
                        if (contType == RDFModelEnums.RDFContainerTypes.Alt)
                        {
                            if (!elemVals.Contains(literal.ToString()))
                            {
                                elemVals.Add(literal.ToString());
                                result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), literal));
                            }
                        }
                        else
                        {
                            result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), literal));
                        }

                    }
                    #endregion

                }
            }

        }
        #endregion

        #endregion

    }

}