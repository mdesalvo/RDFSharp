/*
   Copyright 2012-2024 Marco De Salvo

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
            => Serialize(graph, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given graph to the given stream using XML data format.
        /// </summary>
        internal static void Serialize(RDFGraph graph, Stream outputStream)
        {
            try
            {
                #region serialize
                using (XmlTextWriter rdfxmlWriter = new XmlTextWriter(outputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    rdfxmlWriter.Formatting = Formatting.Indented;

                    #region xmlDecl
                    XmlDocument rdfDoc = new XmlDocument();
                    rdfDoc.AppendChild(rdfDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                    #endregion

                    #region rdfRoot
                    XmlNode rdfRoot = rdfDoc.CreateNode(XmlNodeType.Element, "rdf:RDF", RDFVocabulary.RDF.BASE_URI);
                    XmlAttribute rdfRootNS = rdfDoc.CreateAttribute("xmlns:rdf");
                    XmlText rdfRootNSText = rdfDoc.CreateTextNode(RDFVocabulary.RDF.BASE_URI);
                    rdfRootNS.AppendChild(rdfRootNSText);
                    rdfRoot.Attributes.Append(rdfRootNS);

                    #region prefixes
                    //Write the prefixes (except for "rdf" and "base")
                    List<RDFNamespace> graphNamespaces = RDFModelUtilities.GetGraphNamespaces(graph);
                    List<RDFNamespace> autoNamespaces = GetAutomaticNamespaces(graph);
                    graphNamespaces.Union(autoNamespaces).ToList().ForEach(p =>
                    {
                        if (!p.NamespacePrefix.Equals("rdf", StringComparison.OrdinalIgnoreCase)
                                && !p.NamespacePrefix.Equals("base", StringComparison.OrdinalIgnoreCase))
                        {
                            XmlAttribute pfRootNS = rdfDoc.CreateAttribute(string.Concat("xmlns:", p.NamespacePrefix));
                            XmlText pfRootNSText = rdfDoc.CreateTextNode(p.ToString());
                            pfRootNS.AppendChild(pfRootNSText);
                            rdfRoot.Attributes.Append(pfRootNS);
                        }
                    });
                    //Write the graph's base uri to resolve eventual relative #IDs
                    XmlAttribute pfBaseNS = rdfDoc.CreateAttribute("xml:base");
                    XmlText pfBaseNSText = rdfDoc.CreateTextNode(graph.Context.ToString());
                    pfBaseNS.AppendChild(pfBaseNSText);
                    rdfRoot.Attributes.Append(pfBaseNS);
                    #endregion

                    #region containers/collections
                    RDFGraph rdfType = graph.SelectTriplesByPredicate(RDFVocabulary.RDF.TYPE);
                    RDFGraph rdfFirst = graph.SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST);
                    RDFGraph rdfRest = graph.SelectTriplesByPredicate(RDFVocabulary.RDF.REST);

                    //Fetch data describing containers of the graph
                    var containersXML = new Dictionary<long, XmlNode>();
                    var containers = rdfType.SelectTriplesByObject(RDFVocabulary.RDF.ALT)
                                            .UnionWith(rdfType.SelectTriplesByObject(RDFVocabulary.RDF.BAG))
                                            .UnionWith(rdfType.SelectTriplesByObject(RDFVocabulary.RDF.SEQ))                                          
                                            .Select(t => new
                                            {
                                                ContainerUri = (RDFResource)t.Subject,
                                                ContainerType = t.Object.Equals(RDFVocabulary.RDF.ALT) ? RDFModelEnums.RDFContainerTypes.Alt :
                                                                t.Object.Equals(RDFVocabulary.RDF.BAG) ? RDFModelEnums.RDFContainerTypes.Bag :
                                                                                                         RDFModelEnums.RDFContainerTypes.Seq,
                                                IsFloatingContainer = !graph.IndexedTriples.Any(v => v.Value.ObjectID.Equals(t.Subject.PatternMemberID))
                                            }).ToList();

                    //Fetch data describing collections of the graph
                    var collections = rdfType.SelectTriplesByObject(RDFVocabulary.RDF.LIST)
                                             .Select(t => new
                                             {
                                                 CollectionUri = (RDFResource)t.Subject,
                                                 CollectionValue = rdfFirst.SelectTriplesBySubject((RDFResource)t.Subject)
                                                                           .FirstOrDefault()?.Object,
                                                 CollectionNext = rdfRest.SelectTriplesBySubject((RDFResource)t.Subject)
                                                                         .FirstOrDefault()?.Object,
                                                 IsFloatingCollection = !graph.IndexedTriples.Any(v => v.Value.ObjectID.Equals(t.Subject.PatternMemberID)),
                                                 CollectionItems = RDFModelUtilities.DeserializeCollectionFromGraph(graph, (RDFResource)t.Subject, RDFModelEnums.RDFTripleFlavors.SPO, true).Items
                                             }).ToList();
                    #endregion

                    #region linq
                    //Group the graph's triples by subject (containers must be handled first)
                    var triplesGroupedBySubject = graph.GroupBy(x => x.Subject.ToString())
                                                       .OrderByDescending(x => containers.Any(c => c.ContainerUri.ToString().Equals(x.Key)));
                    #endregion

                    #region graph
                    //Iterate over the calculated groups
                    foreach (IGrouping<string, RDFTriple> triplesGroup in triplesGroupedBySubject)
                    {
                        #region subject
                        //Check if the current subject is a container/collection and it is not floating: if so,
                        //it must be serialized in abbreviated RDF/XML syntax instead of "rdf:Description"
                        XmlNode subjNode = null;
                        string subj = triplesGroup.Key;
                        long subjHash = RDFModelUtilities.CreateHash(subj);
                        var subjContainer = containers.Find(x => x.ContainerUri.PatternMemberID == subjHash);
                        var subjCollection = collections.Find(x => x.CollectionUri.PatternMemberID == subjHash);

                        //It is a container subject and it is not floating => add it to the containersXML pool
                        if (subjContainer != null && !subjContainer.IsFloatingContainer)
                        {
                            switch (subjContainer.ContainerType)
                            {
                                case RDFModelEnums.RDFContainerTypes.Bag:
                                    subjNode = rdfDoc.CreateNode(XmlNodeType.Element, "rdf:Bag", RDFVocabulary.RDF.BASE_URI);
                                    break;
                                case RDFModelEnums.RDFContainerTypes.Seq:
                                    subjNode = rdfDoc.CreateNode(XmlNodeType.Element, "rdf:Seq", RDFVocabulary.RDF.BASE_URI);
                                    break;
                                case RDFModelEnums.RDFContainerTypes.Alt:
                                    subjNode = rdfDoc.CreateNode(XmlNodeType.Element, "rdf:Alt", RDFVocabulary.RDF.BASE_URI);
                                    break;
                            }
                            containersXML.Add(subjHash, subjNode);
                        }

                        //It is a collection subject of resources and it is not floating => do not append its triples because
                        //we will reconstruct the collection later and append it as "rdf:parseType=Collections"
                        else if (subjCollection != null && !subjCollection.IsFloatingCollection && subjCollection.CollectionItems.TrueForAll(item => item is RDFResource))
                            continue;
                        
                        //It is a traditional subject
                        else
                        {
                            subjNode = rdfDoc.CreateNode(XmlNodeType.Element, "rdf:Description", RDFVocabulary.RDF.BASE_URI);
                            //<rdf:Description rdf:nodeID="blankID">
                            XmlAttribute subjNodeDesc = null;
                            XmlText subjNodeDescText = null;
                            if (triplesGroup.Key.StartsWith("bnode:", StringComparison.Ordinal))
                            {
                                subjNodeDescText = rdfDoc.CreateTextNode(triplesGroup.Key.Replace("bnode:", string.Empty));
                                subjNodeDesc = rdfDoc.CreateAttribute("rdf:nodeID", RDFVocabulary.RDF.BASE_URI);
                            }
                            //<rdf:Description rdf:about="subjURI">
                            else
                            {
                                subjNodeDescText = rdfDoc.CreateTextNode(triplesGroup.Key);
                                subjNodeDesc = rdfDoc.CreateAttribute("rdf:about", RDFVocabulary.RDF.BASE_URI);
                            }
                            subjNodeDesc.AppendChild(subjNodeDescText);
                            subjNode.Attributes.Append(subjNodeDesc);
                        }
                        #endregion

                        #region predObjList
                        //Iterate the triples of the current group
                        foreach (RDFTriple triple in triplesGroup)
                        {
                            //Do not append the triple if it is "SUBJECT rdf:type rdf:[Bag|Seq|Alt]"
                            if (!(triple.Predicate.Equals(RDFVocabulary.RDF.TYPE) &&
                                  (subjNode.Name.Equals("rdf:Bag", StringComparison.OrdinalIgnoreCase)
                                    || subjNode.Name.Equals("rdf:Seq", StringComparison.OrdinalIgnoreCase)
                                    || subjNode.Name.Equals("rdf:Alt", StringComparison.OrdinalIgnoreCase))))
                            {
                                #region predicate
                                string predString = triple.Predicate.ToString();
                                //"<predPREF:predURI"
                                RDFNamespace predNS = RDFNamespaceRegister.GetByUri(predString) ?? GenerateNamespace(predString, false);
                                string predUri = (predNS.NamespacePrefix.Equals("autoNS", StringComparison.OrdinalIgnoreCase)
                                                    ? predString.Replace(predNS.ToString(), string.Concat(autoNamespaces.Find(ns => ns.NamespaceUri.Equals(predNS.NamespaceUri)).NamespacePrefix, ":"))
                                                    : predString.Replace(predNS.ToString(), string.Concat(predNS.NamespacePrefix, ":")))
                                                        .Replace(":#", ":")
                                                        .TrimEnd(new char[] { ':', '/' });
                                try
                                {
                                    RDFTypedLiteral predUriQName = new RDFTypedLiteral(predUri, RDFModelEnums.RDFDatatypes.XSD_QNAME);
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

                                    //Object is a container subject and it is not floating => append its node saved in containersXML
                                    if (containerObj != null && !containerObj.IsFloatingContainer)
                                        predNode.AppendChild(containersXML[containerObj.ContainerUri.PatternMemberID]);
                                    
                                    //Object is a collection subject of resources and it is not floating => append its "rdf:parseType=Collection" representation
                                    else if (collectionObj != null && !collectionObj.IsFloatingCollection && collectionObj.CollectionItems.TrueForAll(item => item is RDFResource))
                                    {
                                        //Append "rdf:parseType=Collection" attribute
                                        XmlAttribute rdfParseType = rdfDoc.CreateAttribute("rdf:parseType", RDFVocabulary.RDF.BASE_URI);
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
                                                throw new RDFModelException(string.Format("Collection having '{0}' as subject is not well-formed. Please check presence of its 'rdf:type/rdf:first/rdf:rest' triples.", currentCollItem));

                                            collElementToAppend = rdfDoc.CreateNode(XmlNodeType.Element, "rdf:Description", RDFVocabulary.RDF.BASE_URI);
                                            if (collElement.CollectionValue.ToString().StartsWith("bnode:", StringComparison.Ordinal))
                                            {
                                                collElementAttrText = rdfDoc.CreateTextNode(collElement.CollectionValue.ToString().Replace("bnode:", string.Empty));
                                                collElementAttr = rdfDoc.CreateAttribute("rdf:nodeID", RDFVocabulary.RDF.BASE_URI);
                                            }
                                            else
                                            {
                                                collElementAttrText = rdfDoc.CreateTextNode(collElement.CollectionValue.ToString());
                                                collElementAttr = rdfDoc.CreateAttribute("rdf:about", RDFVocabulary.RDF.BASE_URI);
                                            }
                                            collElementAttr.AppendChild(collElementAttrText);
                                            collElementToAppend.Attributes.Append(collElementAttr);
                                            collElements.Add(collElementToAppend);

                                            //Verify if this is the last element of the collection (pointing to next="rdf:nil")
                                            if (collElement.CollectionNext.Equals(RDFVocabulary.RDF.NIL))
                                                nilFound = true;
                                            else
                                                currentCollItem = (RDFResource)collElement.CollectionNext;
                                        }
                                        collElements.ForEach(c => predNode.AppendChild(c));
                                    }

                                    //Object is traditional
                                    else
                                    {
                                        string objString = triple.Object.ToString();
                                        XmlAttribute predNodeDesc = null;
                                        XmlText predNodeDescText = null;
                                        //  rdf:nodeID="blankID">
                                        if (objString.StartsWith("bnode:"))
                                        {
                                            predNodeDescText = rdfDoc.CreateTextNode(objString.Replace("bnode:", string.Empty));
                                            predNodeDesc = rdfDoc.CreateAttribute("rdf:nodeID", RDFVocabulary.RDF.BASE_URI);
                                        }
                                        //  rdf:resource="objURI">
                                        else
                                        {
                                            predNodeDescText = rdfDoc.CreateTextNode(objString);
                                            predNodeDesc = rdfDoc.CreateAttribute("rdf:resource", RDFVocabulary.RDF.BASE_URI);
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
                                    if (triple.Object is RDFPlainLiteral plitObj)
                                    {
                                        //  xml:lang="plitLANG">
                                        if (plitObj.HasLanguage())
                                        {
                                            XmlAttribute plainLiteralLangNodeDesc = rdfDoc.CreateAttribute("xml:lang", RDFVocabulary.XML.BASE_URI);
                                            XmlText plainLiteralLangNodeDescText = rdfDoc.CreateTextNode(plitObj.Language);
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
                                        XmlAttribute typedLiteralNodeDesc = rdfDoc.CreateAttribute("rdf:datatype", RDFVocabulary.RDF.BASE_URI);
                                        XmlText typedLiteralNodeDescText = rdfDoc.CreateTextNode(tLit.Datatype.URI.ToString());
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
                        //and attached whenever their subject is found as object of a triple
                        if (!subjNode.Name.Equals("rdf:Bag", StringComparison.OrdinalIgnoreCase)
                              && !subjNode.Name.Equals("rdf:Seq", StringComparison.OrdinalIgnoreCase)
                              && !subjNode.Name.Equals("rdf:Alt", StringComparison.OrdinalIgnoreCase))
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
            => Deserialize(new FileStream(filepath, FileMode.Open), null);

        /// <summary>
        /// Deserializes the given Xml stream to a graph.
        /// </summary>
        internal static RDFGraph Deserialize(Stream inputStream, Uri graphContext)
        {
            try
            {
                #region deserialize
                RDFGraph result = new RDFGraph().SetContext(graphContext);
                using (StreamReader streamReader = new StreamReader(inputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(streamReader) 
                            { DtdProcessing = DtdProcessing.Parse, XmlResolver = null, Normalization = false })
                    {
                        #region document
                        XmlDocument xmlDoc = new XmlDocument() { XmlResolver = null };
                        xmlDoc.Load(xmlReader);
                        #endregion

                        #region root
                        //Prepare the namespace table for the Xml selections
                        XmlNamespaceManager nsMgr = new XmlNamespaceManager(new NameTable());
                        nsMgr.AddNamespace(RDFVocabulary.RDF.PREFIX, RDFVocabulary.RDF.BASE_URI);

                        //Select "rdf:RDF" root node
                        XmlNode rdfRDF = GetRdfRootNode(xmlDoc, nsMgr);
                        #endregion

                        #region prefixes
                        //Select "xmlns" attributes and try to add them to the namespace register
                        XmlAttributeCollection xmlnsAttrs = GetXmlnsNamespaces(rdfRDF, nsMgr);

                        //Try to get the "xml:base" attribute, which is needed to resolve eventual relative #IDs in "rdf:about" nodes
                        //If it is not found, set it to the graph Uri
                        Uri xmlBase = null;
                        if (xmlnsAttrs != null && xmlnsAttrs.Count > 0)
                        {
                            XmlAttribute xmlBaseAttr = rdfRDF.Attributes?["xml:base"] 
														?? rdfRDF.Attributes?["xmlns"];
                            if (xmlBaseAttr != null)
                                xmlBase = RDFModelUtilities.GetUriFromString(xmlBaseAttr.Value);
                        }
                        //Always keep in synch the Context and the xmlBase
                        if (xmlBase != null)
                            result.SetContext(xmlBase);
                        else
                            xmlBase = result.Context;
                        #endregion

                        #region elements
                        //Parse children of root node
                        Dictionary<string, long> hashContext = new Dictionary<string, long>();
                        if (rdfRDF.HasChildNodes)
                            ParseNodeList(rdfRDF.ChildNodes, result, xmlBase, GetXmlLangAttribute(rdfRDF), hashContext);
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
        private static List<RDFResource> ParseNodeList(XmlNodeList nodeList, RDFGraph result, Uri xmlBase, XmlAttribute xmlLangParent,
            Dictionary<string, long> hashContext, RDFResource subjectParent = null)
        {
            List<RDFResource> subjects = new List<RDFResource>();
            IEnumerator subjNodesEnum = nodeList.GetEnumerator();
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
                RDFResource subj = subjectParent 
                                    ?? GetSubjectNode(subjNode, xmlBase, result, hashContext) 
                                    ?? new RDFResource();
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
                                    RDFResource obj = new RDFResource(rdfTypeValue.ToString(), hashContext);
                                    result.AddTriple(new RDFTriple(subj, RDFVocabulary.RDF.TYPE, obj));
                                }
                                break;

                            //Threat other attributes as SPL
                            default:
                                RDFResource subjAttrPred = string.IsNullOrEmpty(subjAttr.NamespaceURI) ? new RDFResource(string.Concat(xmlBase, subjAttr.LocalName), hashContext)
                                                                                                       : new RDFResource(string.Concat(subjAttr.NamespaceURI, subjAttr.LocalName), hashContext);
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
                            pred = new RDFResource(string.Concat(xmlBase, predNode.LocalName), hashContext);
                        else
                            pred = predNode.LocalName.StartsWith("autoNS") ? new RDFResource(predNode.NamespaceURI, hashContext)
                                                                           : new RDFResource(string.Concat(predNode.NamespaceURI, predNode.LocalName), hashContext);
                        #endregion

                        #region objList

                        #region collection
                        //Check if predicate has "rdf:parseType=Collection" attribute
                        XmlAttribute rdfCollect = GetParseTypeCollectionAttribute(predNode);
                        if (rdfCollect != null)
                        {
                            ParseCollectionElements(xmlBase, predNode, subj, pred, result, xmlLangPred, hashContext);
                            continue;
                        }
                        #endregion

                        #region container
                        //Check if predicate has "rdf:[Bag|Seq|Alt]" child node
                        XmlNode containerNode = GetContainerNode(predNode);
                        if (containerNode != null)
                        {
                            XmlAttribute xmlLangCont = GetXmlLangAttribute(containerNode) ?? xmlLangPred;

                            //Distinguish the right type of RDF container to build
                            if (string.Equals(containerNode.LocalName, "rdf:Bag", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(containerNode.LocalName, "Bag", StringComparison.OrdinalIgnoreCase))
                                ParseContainerElements(RDFModelEnums.RDFContainerTypes.Bag, containerNode, subj, pred, result, xmlLangCont, hashContext);
                            else if (string.Equals(containerNode.LocalName, "rdf:Seq", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(containerNode.LocalName, "Seq", StringComparison.OrdinalIgnoreCase))
                                ParseContainerElements(RDFModelEnums.RDFContainerTypes.Seq, containerNode, subj, pred, result, xmlLangCont, hashContext);
                            else if (string.Equals(containerNode.LocalName, "rdf:Alt", StringComparison.OrdinalIgnoreCase)
                                      || string.Equals(containerNode.LocalName, "Alt", StringComparison.OrdinalIgnoreCase))
                                ParseContainerElements(RDFModelEnums.RDFContainerTypes.Alt, containerNode, subj, pred, result, xmlLangCont, hashContext);

                            continue;
                        }
                        #endregion

                        #region object
                        //Check if predicate has one of these specific attributes:
                        //"rdf:about", "rdf:resource", "rdf:nodeID", "rdf:ID", "rdf:parseType=Resource"
                        XmlAttribute rdfObject = GetRdfAboutAttribute(predNode)
                                                   ?? GetRdfResourceAttribute(predNode)
                                                   ?? GetParseTypeResourceAttribute(predNode);
                        XmlAttribute rdfId = GetRdfIdAttribute(predNode);
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
                                    ParseNodeList(predNode.SelectNodes("."), result, xmlBase, xmlLangPred, hashContext, parsetypeResource);
                            }
                            #endregion

                            #region rdf:about,rdf:resource,rdf:ID,rdf:nodeID
                            else
                            {
                                #region rdf:ID
                                if (rdfId != null)
                                {
                                    //"rdf:ID" at predicate level appends the parsed triple and also its reification statements;
                                    //The subject of reification is the value of "rdf:ID" attribute resolved against xmlBase
                                    RDFTriple rdfIdTriple = GetRdfIdTriple(predNode, subj, pred, xmlBase, xmlLangPred, hashContext);
                                    if (rdfIdTriple != null)
                                    {
                                        result.AddTriple(rdfIdTriple);
                                        RDFResource rdfIdReificationSubject = new RDFResource(string.Concat(xmlBase, rdfId.Value), hashContext);
                                        foreach (RDFTriple reifRDFIdTriple in ReifyRdfIdPredicateTriple(rdfIdReificationSubject, rdfIdTriple))
                                            result.AddTriple(reifRDFIdTriple);
                                    }
                                    else
                                        throw new RDFModelException($"Found rdf:ID attribute '{rdfId.Value}' at predicate level, but none of supported attributes (rdf:resource/rdf:nodeID/rdf:datatype/xml:lang) were found with him, so the described triple cannot be created.");
                                }
                                #endregion

                                #region rdf:about,rdf:resource,rdf:nodeID
                                else
                                {
                                    string objValue = ResolveRelativeNode(rdfObject, xmlBase);
                                    RDFResource obj = new RDFResource(objValue, hashContext);
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
                            RDFTypedLiteral tLit = new RDFTypedLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(predNode.InnerText)), RDFDatatypeRegister.GetDatatype(rdfDatatype.Value));
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
                        bool hasOneChildNode = predNode.HasChildNodes && predNode.ChildNodes.Count == 1;
                        if (hasOneChildNode && 
							  (predNode.ChildNodes[0].NodeType == XmlNodeType.Text 
							    || predNode.ChildNodes[0].NodeType == XmlNodeType.EntityReference))
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
                            List<RDFResource> nestedResources = ParseNodeList(predNode.ChildNodes, result, xmlBase, xmlLangPred, hashContext);
                            foreach (RDFResource nestedResource in nestedResources)
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
            XmlNode rdf = xmlDoc.SelectSingleNode("rdf:RDF", nsMgr) 
                           ?? xmlDoc.SelectSingleNode("RDF", nsMgr) 
                           ?? throw new Exception("Given file has not a valid \"rdf:RDF\" or \"RDF\" root node");
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
                    if (!string.Equals(attr.LocalName, "xmlns", StringComparison.OrdinalIgnoreCase)
                          && !string.Equals(attr.Name, "xml:lang", StringComparison.OrdinalIgnoreCase)
                          && !string.Equals(attr.Name, "xml:base", StringComparison.OrdinalIgnoreCase))
                    {
                        //Try to resolve the current namespace against the namespace register;
                        //if not resolved, create new namespace with scope limited to actual node
                        RDFNamespace ns = RDFNamespaceRegister.GetByPrefix(attr.LocalName) 
                                           ?? RDFNamespaceRegister.GetByUri(attr.Value) 
                                           ?? new RDFNamespace(attr.LocalName, attr.Value);

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
                Uri uriNS = RDFModelUtilities.GetUriFromString(namespaceString) 
                             ?? throw new RDFModelException("Cannot create RDFNamespace because given \"namespaceString\" (" + namespaceString + ") parameter cannot be converted to a valid Uri");
                string fragment;
                string nspace = uriNS.AbsoluteUri;

                // e.g.:  "http://www.w3.org/2001/XMLSchema#integer"
                if (uriNS.Fragment != string.Empty)
                {
                    fragment = uriNS.Fragment.Replace("#", string.Empty);                           //"integer"
                    if (fragment != string.Empty)
                        nspace = Regex.Replace(nspace, string.Concat(fragment, "$"), string.Empty); //"http://www.w3.org/2001/XMLSchema#"
                }
                else
                {
                    // e.g.:  "http://example.org/integer"
                    if (uriNS.LocalPath != "/" && !isDatatypeNamespace)
						nspace = Regex.Replace(nspace, string.Concat(uriNS.Segments[uriNS.Segments.Length - 1], "$"), string.Empty);
                }

                //Check if a namespace with the extracted Uri is in the register, or generate an automatic one
                return RDFNamespaceRegister.GetByUri(nspace) ?? new RDFNamespace("autoNS", nspace);
            }
            throw new RDFModelException("Cannot create RDFNamespace because given \"namespaceString\" parameter is null or empty");
        }

        /// <summary>
        /// Gets the list of automatic namespaces used within the predicates of the triples of the given graph
        /// </summary>
        private static List<RDFNamespace> GetAutomaticNamespaces(RDFGraph graph)
        {
            List<RDFNamespace> result = new List<RDFNamespace>();
            foreach (string pred in graph.IndexedTriples.Select(x => graph.GraphIndex.ResourcesRegister[x.Value.PredicateID].ToString()).Distinct())
            {
                RDFNamespace nspace = GenerateNamespace(pred, false);

                //Check if the predicate can be abbreviated to a valid QName
                if (!string.Equals(nspace.NamespaceUri.ToString(), pred))
                {
                    if (nspace.NamespacePrefix.StartsWith("autoNS") && !result.Contains(nspace))
						result.Add(new RDFNamespace(string.Concat("autoNS", (result.Count + 1).ToString()), nspace.NamespaceUri.ToString()));
                }
                else
                    throw new RDFModelException(string.Format("found '{0}' predicate which cannot be abbreviated to a valid QName.", pred));
            }
            return result.ToList();
        }

        /// <summary>
        /// Gives the subj node extracted from the attribute list of the current element
        /// </summary>
        private static RDFResource GetSubjectNode(XmlNode subjNode, Uri xmlBase, RDFGraph result, Dictionary<string, long> hashContext)
        {
            RDFResource subj = null;

            //If there are attributes, search for the one describing the subject
            if (subjNode.Attributes != null && subjNode.Attributes.Count > 0)
            {
                //We are interested in finding the "rdf:about" or "rdf:resource"
                XmlAttribute rdfAbout = GetRdfAboutAttribute(subjNode) ?? GetRdfResourceAttribute(subjNode);
                if (rdfAbout != null)
                {
                    //Attribute found, but we must check if it is "rdf:ID", "rdf:nodeID" or a relative Uri:
                    //in this case it must be resolved against the xmlBase namespace, or else it remains the same
                    string rdfAboutValue = ResolveRelativeNode(rdfAbout, xmlBase);
                    subj = new RDFResource(rdfAboutValue, hashContext);
                }
            }
            if (subj == null)
                subj = new RDFResource();

            //We must check if the node is not a standard "rdf:Description": this is
            //the case we can directly build a triple with "rdf:type" pred
            if (!CheckIfRdfDescriptionNode(subjNode))
            {
                RDFResource obj;
                if (subjNode.NamespaceURI == string.Empty)
                    obj = new RDFResource(string.Concat(xmlBase, subjNode.LocalName), hashContext);
                else
                    obj = new RDFResource(string.Concat(subjNode.NamespaceURI, subjNode.LocalName), hashContext);
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
                string xmlBaseString = xmlBase.ToString();

                //Adjust corner case for clashes on namespace ending characters ("#", "/")
                if (xmlBaseString.EndsWith("#") && attrValue.StartsWith("#"))
                    attrValue = attrValue.TrimStart('#');
                if (xmlBaseString.EndsWith("/") && attrValue.StartsWith("/"))
                    attrValue = attrValue.TrimStart('/');

                //"rdf:ID" relative Uri: must be resolved against the xmlBase namespace
                if (string.Equals(attr.LocalName, "rdf:ID", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(attr.LocalName, "ID", StringComparison.OrdinalIgnoreCase))
                {
                    //This kind of syntax requires the attribute value to start with "#"
                    if (!attrValue.StartsWith("#"))
                        attrValue = string.Concat("#", attrValue);
                    if (xmlBaseString.EndsWith("#"))
                        xmlBaseString = xmlBaseString.TrimEnd('#');
                    attrValue = RDFModelUtilities.GetUriFromString(string.Concat(xmlBaseString, attrValue)).ToString();
                }   

                //"rdf:nodeID" relative Uri: must be resolved against the "bnode:" prefix
                else if (string.Equals(attr.LocalName, "rdf:nodeID", StringComparison.OrdinalIgnoreCase)
                          || string.Equals(attr.LocalName, "nodeID", StringComparison.OrdinalIgnoreCase))
                {
                    if (!attrValue.StartsWith("bnode:"))
                        attrValue = string.Concat("bnode:", attrValue);
                }

                //"rdf:about" or "rdf:resource" relative Uri: must be resolved against the xmlBase namespace
                else if (RDFModelUtilities.GetUriFromString(attrValue) == null)
                    attrValue = RDFModelUtilities.GetUriFromString(string.Concat(xmlBaseString, attrValue)).ToString();

                return attrValue;
            }
            throw new RDFModelException("Cannot resolve relative node because given \"attr\" or \"xmlBase\" parameters are null");
        }

        /// <summary>
        /// Verify if we are on a standard rdf:Description element
        /// </summary>
        private static bool CheckIfRdfDescriptionNode(XmlNode subjNode)
            => string.Equals(subjNode.LocalName, "rdf:Description", StringComparison.OrdinalIgnoreCase)
                || string.Equals(subjNode.LocalName, "Description", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF subject
        /// </summary>
        private static XmlAttribute GetRdfAboutAttribute(XmlNode subjNode)
            => subjNode.Attributes?["rdf:about"] 
				?? subjNode.Attributes?["about"] 
				?? subjNode.Attributes?["rdf:nodeID"] 
				?? subjNode.Attributes?["nodeID"] 
				?? subjNode.Attributes?["rdf:ID"] 
				?? subjNode.Attributes?["ID"];

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF object
        /// </summary>
        private static XmlAttribute GetRdfResourceAttribute(XmlNode predNode)
            => predNode.Attributes?["rdf:resource"] 
				?? predNode.Attributes?["resource"] 
				?? predNode.Attributes?["rdf:nodeID"] 
				?? predNode.Attributes?["nodeID"];

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF ID
        /// </summary>
        private static XmlAttribute GetRdfIdAttribute(XmlNode subjNode)
            => subjNode.Attributes?["rdf:ID"] 
				?? subjNode.Attributes?["ID"];

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF typed literal datatype
        /// </summary>
        private static XmlAttribute GetRdfDatatypeAttribute(XmlNode predNode)
            => predNode.Attributes?["rdf:datatype"] 
				?? predNode.Attributes?["datatype"];

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF plain literal language
        /// </summary>
        private static XmlAttribute GetXmlLangAttribute(XmlNode predNode)
            => predNode.Attributes?["xml:lang"] 
				?? predNode.Attributes?["lang"];

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Collection"
        /// </summary>
        private static XmlAttribute GetParseTypeCollectionAttribute(XmlNode predNode)
        {
            XmlAttribute rdfCollection = predNode.Attributes?["rdf:parseType"] ?? predNode.Attributes?["parseType"];
            return string.Equals(rdfCollection?.Value, "Collection", StringComparison.OrdinalIgnoreCase) ? rdfCollection : null;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Literal"
        /// </summary>
        private static XmlAttribute GetParseTypeLiteralAttribute(XmlNode predNode)
        {
            XmlAttribute rdfLiteral = predNode.Attributes?["rdf:parseType"] ?? predNode.Attributes?["parseType"];
            return string.Equals(rdfLiteral?.Value, "Literal", StringComparison.OrdinalIgnoreCase) ? rdfLiteral : null;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Resource"
        /// </summary>
        private static XmlAttribute GetParseTypeResourceAttribute(XmlNode predNode)
        {
            XmlAttribute rdfResource = predNode.Attributes?["rdf:parseType"] ?? predNode.Attributes?["parseType"];
            return string.Equals(rdfResource?.Value, "Resource", StringComparison.OrdinalIgnoreCase) ? rdfResource : null;
        }

        /// <summary>
        /// Given an element, return the triple corresponding to the RDF ID
        /// </summary>
        private static RDFTriple GetRdfIdTriple(XmlNode predNode, RDFResource subject, RDFResource predicate, Uri xmlBase, 
            XmlAttribute xmlLangPred, Dictionary<string, long> hashContext)
        {
            //Try to resolve SPO triple by "rdf:Resource" or "rdf:nodeID" attributes
            XmlAttribute resOrNodeIdAttr = GetRdfResourceAttribute(predNode);
            if (resOrNodeIdAttr != null)
                return new RDFTriple(subject, predicate, new RDFResource(ResolveRelativeNode(resOrNodeIdAttr, xmlBase), hashContext));

            //Try to resolve SPLT triple by "rdf:datatype" or "rdf:parseType=Literal" attributes
            XmlAttribute rdfDatatypeAttr = GetRdfDatatypeAttribute(predNode);
            if (rdfDatatypeAttr != null)
            {
                RDFTypedLiteral tLit = new RDFTypedLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(predNode.InnerText)), RDFDatatypeRegister.GetDatatype(rdfDatatypeAttr.Value));
                return new RDFTriple(subject, predicate, tLit);
            }
            XmlAttribute parseLiteral = GetParseTypeLiteralAttribute(predNode);
            if (parseLiteral != null)
            {
                RDFTypedLiteral tLit = new RDFTypedLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(predNode.InnerXml)), RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL);
                return new RDFTriple(subject, predicate, tLit);
            }

            //Try to resolve SPL triple (SPLL by "xml:lang")
            bool hasOneChildNode = predNode.HasChildNodes && predNode.ChildNodes.Count == 1;
            if (hasOneChildNode && 
                    (predNode.ChildNodes[0].NodeType == XmlNodeType.Text || predNode.ChildNodes[0].NodeType == XmlNodeType.EntityReference))
            {
                RDFPlainLiteral pLit = new RDFPlainLiteral(RDFModelUtilities.ASCII_To_Unicode(HttpUtility.HtmlDecode(predNode.InnerText)), xmlLangPred?.Value);
                return new RDFTriple(subject, predicate, pLit);
            }

            return null;
        }

        /// <summary>
        /// Builds the reification graph of the special triple obtained from rdf:ID found at predicate level
        /// </summary>
        private static RDFGraph ReifyRdfIdPredicateTriple(RDFResource reificationSubject, RDFTriple triple)
        {
            RDFGraph reifGraph = new RDFGraph();

            reifGraph.AddTriple(new RDFTriple(reificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.STATEMENT));
            reifGraph.AddTriple(new RDFTriple(reificationSubject, RDFVocabulary.RDF.SUBJECT, (RDFResource)triple.Subject));
            reifGraph.AddTriple(new RDFTriple(reificationSubject, RDFVocabulary.RDF.PREDICATE, (RDFResource)triple.Predicate));
            if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                reifGraph.AddTriple(new RDFTriple(reificationSubject, RDFVocabulary.RDF.OBJECT, (RDFResource)triple.Object));
            else
                reifGraph.AddTriple(new RDFTriple(reificationSubject, RDFVocabulary.RDF.OBJECT, (RDFLiteral)triple.Object));

            return reifGraph;
        }

        /// <summary>
        /// Given an attribute representing a RDF collection, iterates on its constituent elements
        /// to build its standard reification triples.
        /// </summary>
        private static void ParseCollectionElements(Uri xmlBase, XmlNode predNode, RDFResource subj, RDFResource pred, 
            RDFGraph result, XmlAttribute xmlLangSubj, Dictionary<string, long> hashContext)
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
                List<RDFResource> elems = ParseNodeList(predNode.ChildNodes, result, xmlBase, xmlLangPred, hashContext);
				RDFResource lastElement = elems.Last();
                foreach (RDFResource elem in elems)
                {
                    // obj -> rdf:type -> rdf:list
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));

                    // obj -> rdf:first -> res
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.FIRST, elem));

                    //Last element of a collection must give a triple to a "rdf:nil" object
                    RDFResource newObj;
                    if (elem != lastElement)
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
            => string.Equals(containerNode.LocalName, "rdf:Bag", StringComparison.OrdinalIgnoreCase)
                || string.Equals(containerNode.LocalName, "Bag", StringComparison.OrdinalIgnoreCase)
                || string.Equals(containerNode.LocalName, "rdf:Seq", StringComparison.OrdinalIgnoreCase)
                || string.Equals(containerNode.LocalName, "Seq", StringComparison.OrdinalIgnoreCase)
                || string.Equals(containerNode.LocalName, "rdf:Alt", StringComparison.OrdinalIgnoreCase)
                || string.Equals(containerNode.LocalName, "Alt", StringComparison.OrdinalIgnoreCase);

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
        private static void ParseContainerElements(RDFModelEnums.RDFContainerTypes contType, XmlNode container, RDFResource subj, RDFResource pred, 
            RDFGraph result, XmlAttribute xmlLangPred, Dictionary<string, long> hashContext)
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
                        if (string.Equals(elemUri.LocalName, "nodeID", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!elemUri.Value.StartsWith("bnode:"))
                                elemUri.Value = string.Concat("bnode:", elemUri.Value);
                        }

                        //obj -> rdf:_N -> VALUE
                        if (contType == RDFModelEnums.RDFContainerTypes.Alt)
                        {
                            if (!elemVals.Contains(elemUri.Value))
                            {
                                elemVals.Add(elemUri.Value);
                                result.AddTriple(new RDFTriple(obj, new RDFResource(string.Concat(RDFVocabulary.RDF.BASE_URI, elem.LocalName), hashContext), new RDFResource(elemUri.Value, hashContext)));
                            }
                        }
                        else
                            result.AddTriple(new RDFTriple(obj, new RDFResource(string.Concat(RDFVocabulary.RDF.BASE_URI, elem.LocalName), hashContext), new RDFResource(elemUri.Value, hashContext)));
                    }
                    #endregion

                    #region Container Literal Item
                    //This is a container of literals
                    else
                    {
                        //Parse the literal contained in the item
                        RDFLiteral literal;
                        XmlAttribute attr = GetRdfDatatypeAttribute(elem);
                        if (attr != null)
                            literal = new RDFTypedLiteral(elem.InnerText, RDFDatatypeRegister.GetDatatype(attr.InnerText));
                        else
                        {
                            attr = GetXmlLangAttribute(elem) ?? xmlLangPred;
                            literal = new RDFPlainLiteral(elem.InnerText, attr?.InnerText ?? string.Empty);
                        }

                        //obj -> rdf:_N -> VALUE
                        if (contType == RDFModelEnums.RDFContainerTypes.Alt)
                        {
                            if (!elemVals.Contains(literal.ToString()))
                            {
                                elemVals.Add(literal.ToString());
                                result.AddTriple(new RDFTriple(obj, new RDFResource(string.Concat(RDFVocabulary.RDF.BASE_URI, elem.LocalName), hashContext), literal));
                            }
                        }
                        else
                            result.AddTriple(new RDFTriple(obj, new RDFResource(string.Concat(RDFVocabulary.RDF.BASE_URI, elem.LocalName), hashContext), literal));
                    }
                    #endregion
                }
            }
        }
        #endregion

        #endregion
    }
}