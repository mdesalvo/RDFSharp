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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFSerializerUtilities is a collector of reusable utility methods for RDF serialization management
    /// </summary>
    internal static class RDFSerializerUtilities {

        #region Methods

        #region RDFNTriples
        /// <summary>
        /// Regex to catch 8-byte unicodes in NTriples
        /// </summary>
        private static readonly Lazy<Regex> regexU8 = new Lazy<Regex>(() => new Regex(@"\\U([0-9A-Fa-f]{8})", RegexOptions.Compiled));
        /// <summary>
        /// Regex to catch 4-byte unicodes in NTriples
        /// </summary>
        private static readonly Lazy<Regex> regexU4 = new Lazy<Regex>(() => new Regex(@"\\u([0-9A-Fa-f]{4})", RegexOptions.Compiled));
        /// <summary>
        /// Regex to parse NTriples focusing on predicate position 
        /// </summary>
        private static readonly Lazy<Regex> regexNT = new Lazy<Regex>(() => new Regex(@"(?<pred>\s+<[^>]+>\s+)", RegexOptions.ExplicitCapture | RegexOptions.Compiled)); 

        /// <summary>
        /// Tries to parse the given NTriple, throwing an error in case of a basic syntactical error is found 
        /// </summary>
        internal static String[] ParseNTriple(String ntriple) {
            String[] tokens   = new String[3];
            
            //A legal NTriple starts with "_:" of blanks or "<" of non-blanks
            if (ntriple.StartsWith("_:") || ntriple.StartsWith("<")) {
                
                //Parse NTriple by exploiting surrounding spaces and angle brackets of predicate
                tokens        = regexNT.Value.Split(ntriple, 2);
                
                //An illegal NTriple cannot be splitted into three parts with this regex, because predicate not being isolated
                if (tokens.Length != 3) {
                    throw new Exception("found illegal NTriple, predicate must be surrounded by \" <\" and \"> \"");
                }

                //Check subject for well-formedness
                tokens[0]     = tokens[0].Trim();
                if (tokens[0].Contains(" ")) {
                    throw new Exception("found illegal NTriple, subject Uri cannot contain spaces");
                }
                if ((tokens[0].StartsWith("<")   && !tokens[0].EndsWith(">")) ||
                    (tokens[0].StartsWith("_:")  && tokens[0].EndsWith(">"))  ||
                    (tokens[0].Count(c => c.Equals('<')) > 1)                 ||
                    (tokens[0].Count(c => c.Equals('>')) > 1))  {
                    throw new Exception("found illegal NTriple, subject Uri is not well-formed");
                }

                //Check predicate for well-formedness
                tokens[1]     = tokens[1].Trim();
                if (tokens[1].Contains(" ")) {
                    throw new Exception("found illegal NTriple, predicate Uri cannot contain spaces");
                }
                if ((tokens[1].Count(c => c.Equals('<')) > 1)                 ||
                    (tokens[1].Count(c => c.Equals('>')) > 1)) {
                    throw new Exception("found illegal NTriple, predicate Uri is not well-formed");
                }

                //Check object for well-formedness
                tokens[2]     = tokens[2].Trim();
                if (tokens[2].StartsWith("<")) {
                    if (tokens[2].Contains(" ")) {
                        throw new Exception("found illegal NTriple, object Uri cannot contain spaces");
                    }
                    if ((!tokens[2].EndsWith(">")                             ||
                         (tokens[2].Count(c => c.Equals('<')) > 1)            ||
                         (tokens[2].Count(c => c.Equals('>')) > 1))) {
                        throw new Exception("found illegal NTriple, object Uri is not well-formed");
                    }
                }
                else if (tokens[2].StartsWith("_:")) {
                    if (tokens[2].EndsWith(">")) {
                        throw new Exception("found illegal NTriple, object Uri is not well-formed");
                    }
                }
            }
            else {
                throw new Exception("found illegal NTriple, must start with \"_:\" or with \"<\"");
            }

            return tokens;
        }

        /// <summary>
        /// Turns back ASCII-encoded Unicodes into Unicodes. 
        /// </summary>
        internal static String ASCII_To_Unicode(String asciiString) {
            if (asciiString  != null) {
                asciiString   = regexU8.Value.Replace(asciiString, match => ((Char)Int32.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));
                asciiString   = regexU4.Value.Replace(asciiString, match => ((Char)Int32.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));                
            }
            return asciiString;
        }

        /// <summary>
        /// Turns Unicodes into ASCII-encoded Unicodes. 
        /// </summary>
        internal static String Unicode_To_ASCII(String unicodeString) {
            if (unicodeString  != null) {
                StringBuilder b = new StringBuilder();
                Char[] chars    = unicodeString.ToCharArray();
                foreach (Char c in chars) {
                    if (c <= 127) {
                        b.Append(c);
                    }
                    else {
                        if (c  <= 65535) {
                            b.Append("\\u" + ((Int32)c).ToString("X4"));
                        }
                        else {
                            b.Append("\\U" + ((Int32)c).ToString("X8"));
                        }
                    }
                }
                unicodeString  = b.ToString();
            }
            return unicodeString;
        }
        #endregion

        #region RDFXml
        /// <summary>
        /// Gives the "rdf:RDF" root node of the document
        /// </summary>
        internal static XmlNode GetRdfRootNode(XmlDocument xmlDoc, XmlNamespaceManager nsMgr) {
            XmlNode rdf = 
                (xmlDoc.SelectSingleNode(RDFVocabulary.RDF.PREFIX + ":RDF", nsMgr) ??
                    xmlDoc.SelectSingleNode("RDF", nsMgr));

            //Invalid RDF/XML file: root node is neither "rdf:RDF" or "RDF"
            if (rdf    == null) {
                throw new Exception("Given file has not a valid \"rdf:RDF\" or \"RDF\" root node");
            }

            return rdf;
        }

        /// <summary>
        /// Gives the collection of "xmlns" attributes of the "rdf:RDF" root node
        /// </summary>
        internal static XmlAttributeCollection GetXmlnsNamespaces(XmlNode rdfRDF, XmlNamespaceManager nsMgr) {
            XmlAttributeCollection xmlns = rdfRDF.Attributes;
            if (xmlns != null           && xmlns.Count > 0) {

                IEnumerator iEnum        = xmlns.GetEnumerator();
                while (iEnum != null    && iEnum.MoveNext()) {
                    XmlAttribute attr    = (XmlAttribute)iEnum.Current;
                    if (attr.LocalName.ToUpperInvariant() != "XMLNS") {

                        //Try to resolve the current namespace against the namespace register; 
                        //if not resolved, create new namespace with scope limited to actual node
                        RDFNamespace ns  =  
                        (RDFNamespaceRegister.GetByPrefix(attr.LocalName) ??
                                RDFNamespaceRegister.GetByNamespace(attr.Value) ??
                                    new RDFNamespace(attr.LocalName, attr.Value));

                        nsMgr.AddNamespace(ns.Prefix, ns.Namespace.ToString());

                    }
                }

            }
            return xmlns;
        }

        /// <summary>
        /// Gives the subj node extracted from the attribute list of the current element 
        /// </summary>
        internal static RDFResource GetSubjectNode(XmlNode subjNode, Uri xmlBase, RDFGraph result) {
            RDFResource subj              = null;

            //If there are attributes, search them for the one representing the subj
            if (subjNode.Attributes      != null && subjNode.Attributes.Count > 0) {

                //We are interested in finding the "rdf:about" node for the subj
                XmlAttribute rdfAbout     = GetRdfAboutAttribute(subjNode);
                if(rdfAbout != null) {
                    //Attribute found, but we must check if it is "rdf:ID", "rdf:nodeID" or a relative Uri: 
                    //in this case it must be resolved against the xmlBase namespace, or else it remains the same
                    String rdfAboutValue  = RDFSerializerUtilities.ResolveRelativeNode(rdfAbout, xmlBase);
                    subj                  = new RDFResource(rdfAboutValue);
                }

                //If "rdf:about" attribute has been found for the subj, we must
                //check if the node is not a standard "rdf:Description": this is
                //the case we can directly build a triple with "rdf:type" pred
                if (subj != null && !CheckIfRdfDescriptionNode(subjNode)) {
                    RDFResource obj       = null;
                    if (subjNode.NamespaceURI == String.Empty) {
                        obj               = new RDFResource(xmlBase + subjNode.LocalName);
                    }
                    else {
                        obj               = new RDFResource(subjNode.NamespaceURI + subjNode.LocalName);
                    }
                    result.AddTriple(new RDFTriple(subj, RDFVocabulary.RDF.TYPE, obj));
                }
                
            }

            //There are no attributes, so there's only one way we can handle this element:
            //if it is a standard rdf:Description, it is a blank Subject
            else {
                if (CheckIfRdfDescriptionNode(subjNode)) {
                    subj                  = new RDFResource();
                }                
            }

            return subj;
        }

        /// <summary>
        /// Checks if the given attribute is absolute Uri, relative Uri, "rdf:ID" relative Uri, "rdf:nodeID" blank node Uri
        /// </summary>
        internal static String ResolveRelativeNode(XmlAttribute attr, Uri xmlBase) {
            if (attr != null && xmlBase != null) {
                String attrValue      = attr.Value;

                //"rdf:ID" relative Uri: must be resolved against the xmlBase namespace
                if (attr.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":ID", StringComparison.Ordinal)         ||
                    attr.LocalName.Equals("ID", StringComparison.Ordinal)) {
                        attrValue     = RDFModelUtilities.GetUriFromString(xmlBase + attrValue).ToString();
                }

                //"rdf:nodeID" relative Uri: must be resolved against the "bnode:" prefix
                else if(attr.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":nodeID", StringComparison.Ordinal) ||
                        attr.LocalName.Equals("nodeID", StringComparison.Ordinal)) {
                        if (!attrValue.StartsWith("bnode:")) {
                            attrValue = "bnode:" + attrValue; 
                        }
                }

                //"rdf:about" or "rdf:resource" relative Uri: must be resolved against the xmlBase namespace
                else if (RDFModelUtilities.GetUriFromString(attrValue) == null) {
                    attrValue         = RDFModelUtilities.GetUriFromString(xmlBase + attrValue).ToString();
                }

                return attrValue;
            }
            throw new RDFModelException("Cannot resolve relative node because given \"attr\" or \"xmlBase\" parameters are null");
        }

        /// <summary>
        /// Verify if we are on a standard rdf:Description element
        /// </summary>
        internal static Boolean CheckIfRdfDescriptionNode(XmlNode subjNode) {
            Boolean result = (subjNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Description", StringComparison.Ordinal) ||
                              subjNode.LocalName.Equals("Description", StringComparison.Ordinal));
            return result;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF subj
        /// </summary>
        internal static XmlAttribute GetRdfAboutAttribute(XmlNode subjNode) {
            //rdf:about
            XmlAttribute rdfAbout = 
                (subjNode.Attributes[RDFVocabulary.RDF.PREFIX + ":about"] ?? 
                    (subjNode.Attributes["about"] ?? 
                        (subjNode.Attributes[RDFVocabulary.RDF.PREFIX + ":nodeID"] ?? 
                            (subjNode.Attributes["nodeID"] ?? 
                                (subjNode.Attributes[RDFVocabulary.RDF.PREFIX + ":ID"] ?? 
                                    subjNode.Attributes["ID"])))));
            return rdfAbout;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF object
        /// </summary>
        internal static XmlAttribute GetRdfResourceAttribute(XmlNode predNode) {
            //rdf:Resource
            XmlAttribute rdfResource = 
                (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":resource"] ?? 
                    (predNode.Attributes["resource"] ?? 
                        (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":nodeID"] ?? 
                            predNode.Attributes["nodeID"])));
            return rdfResource;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF typed literal datatype
        /// </summary>
        internal static XmlAttribute GetRdfDatatypeAttribute(XmlNode predNode) {
            //rdf:datatype
            XmlAttribute rdfDatatype = 
                (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":datatype"] ?? 
                    predNode.Attributes["datatype"]);
            return rdfDatatype;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF plain literal language
        /// </summary>
        internal static XmlAttribute GetXmlLangAttribute(XmlNode predNode) {
            //xml:lang
            XmlAttribute xmlLang = 
                (predNode.Attributes[RDFVocabulary.XML.PREFIX + ":lang"] ?? 
                    predNode.Attributes["lang"]);
            return xmlLang;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Collection"
        /// </summary>
        internal static XmlAttribute GetParseTypeCollectionAttribute(XmlNode predNode) {
            XmlAttribute rdfCollection = 
                (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":parseType"] ??
                    predNode.Attributes["parseType"]);

            return ((rdfCollection != null && rdfCollection.Value.Equals("Collection", StringComparison.Ordinal)) ? rdfCollection : null);
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Literal"
        /// </summary>
        internal static XmlAttribute GetParseTypeLiteralAttribute(XmlNode predNode) {
            XmlAttribute rdfLiteral =
                (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":parseType"] ??
                    predNode.Attributes["parseType"]);

            return ((rdfLiteral != null && rdfLiteral.Value.Equals("Literal", StringComparison.Ordinal)) ? rdfLiteral : null);
        }

        /// <summary>
        /// Given an attribute representing a RDF collection, iterates on its constituent elements
        /// to build its standard reification triples. 
        /// </summary>
        internal static void ParseCollectionElements(Uri xmlBase, XmlNode predNode, RDFResource subj,
                                                     RDFResource pred, RDFGraph result) {

            //Attach the collection as the blank object of the current pred
            RDFResource  obj              = new RDFResource();
            result.AddTriple(new RDFTriple(subj, pred, obj));

            //Iterate on the collection items to reify it
            if (predNode.HasChildNodes) {
                IEnumerator elems         = predNode.ChildNodes.GetEnumerator();
                while (elems != null     && elems.MoveNext()) {
                    XmlNode elem          = (XmlNode)elems.Current;
                    
                    //Try to get items as "rdf:about" attributes, or as "rdf:resource"
                    XmlAttribute elemUri  = 
					    (GetRdfAboutAttribute(elem) ?? 
						     GetRdfResourceAttribute(elem));
                    if (elemUri          != null) {

                        //Sanitize eventual blank node or relative value, depending on attribute found
                        elemUri.Value     = RDFSerializerUtilities.ResolveRelativeNode(elemUri, xmlBase);

                        // obj -> rdf:type -> rdf:list
                        result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));

                        // obj -> rdf:first -> res
                        result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.FIRST, new RDFResource(elemUri.Value)));

                        //Last element of a collection must give a triple to a "rdf:nil" object
                        RDFResource newObj;
                        if (elem         != predNode.ChildNodes.Item(predNode.ChildNodes.Count - 1)) {
                            // obj -> rdf:rest -> newObj
                            newObj        = new RDFResource();                            
                        }
                        else {
                            // obj -> rdf:rest -> rdf:nil
                            newObj        = RDFVocabulary.RDF.NIL;
                        }
                        result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.REST, newObj));
                        obj               = newObj;

                    }
                }
            }
        }

        /// <summary>
        /// Given the metadata of a graph and a collection resource, it reconstructs the RDF collection and returns it as a list of nodes
        /// This is needed for building the " rdf:parseType=Collection>" RDF/XML abbreviation goody for collections of resources
        /// </summary>
        internal static List<XmlNode> ReconstructCollection(RDFGraphMetadata rdfGraphMetadata, RDFResource tripleObject, XmlDocument rdfDoc) {
            List<XmlNode> result          = new List<XmlNode>();
            Boolean nilFound              = false;
            XmlNode collElementToAppend   = null;
            XmlAttribute collElementAttr  = null;
            XmlText collElementAttrText   = null;

            //Iterate the elements of the collection until the last one (pointing to next="rdf:nil")
            while (!nilFound) {
                var collElement           = rdfGraphMetadata.Collections[tripleObject];
                collElementToAppend       = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Description", RDFVocabulary.RDF.BASE_URI);
                collElementAttr           = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":about", RDFVocabulary.RDF.BASE_URI);
                collElementAttrText       = rdfDoc.CreateTextNode(collElement.ItemValue.ToString());
                if (collElementAttrText.InnerText.StartsWith("bnode:")) {
                    collElementAttrText.InnerText = collElementAttrText.InnerText.Replace("bnode:", String.Empty);
                }
                collElementAttr.AppendChild(collElementAttrText);
                collElementToAppend.Attributes.Append(collElementAttr);
                result.Add(collElementToAppend);

                //Verify if this is the last element of the collection (pointing to next="rdf:nil")
                if (collElement.ItemNext.ToString().Equals(RDFVocabulary.RDF.NIL.ToString(), StringComparison.Ordinal)) {
                    nilFound              = true;
                }
                else {
                    tripleObject          = collElement.ItemNext as RDFResource;
                }

            }

            return result;
        }

        /// <summary>
        /// Verify if we are on a standard rdf:[Bag|Seq|Alt] element
        /// </summary>
        internal static Boolean CheckIfRdfContainerNode(XmlNode containerNode) {
            Boolean result = (containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.Ordinal) || containerNode.LocalName.Equals("Bag", StringComparison.Ordinal) ||
                              containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.Ordinal) || containerNode.LocalName.Equals("Seq", StringComparison.Ordinal) ||
                              containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.Ordinal) || containerNode.LocalName.Equals("Alt", StringComparison.Ordinal));
            return result;
        }

        /// <summary>
        /// Given an element, return the child element which can correspond to the RDF container
        /// </summary>
        internal static XmlNode GetContainerNode(XmlNode predNode) {
            //A container is the first child of the given node and it has no attributes.
            //Its localname must be the canonical "rdf:[Bag|Seq|Alt]", so we check for this.
            if (predNode.HasChildNodes) {
                XmlNode containerNode  = predNode.FirstChild;
                Boolean isRdfContainer = CheckIfRdfContainerNode(containerNode);
                if (isRdfContainer) {
                    return containerNode;
                }
            }
            return null;
        }

        /// <summary>
        /// Given an element representing a RDF container, iterates on its constituent elements
        /// to build its standard reification triples. 
        /// </summary>
        internal static void ParseContainerElements(RDFModelEnums.RDFContainerTypes contType, XmlNode container, 
                                                    RDFResource subj, RDFResource pred, RDFGraph result) {

            //Attach the container as the blank object of the current pred
            RDFResource  obj                 = new RDFResource();
            result.AddTriple(new RDFTriple(subj, pred, obj));
            
            //obj -> rdf:type -> rdf:[Bag|Seq|Alt]
            switch (contType) {
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
            if (container.HasChildNodes) {
                IEnumerator elems            = container.ChildNodes.GetEnumerator();
                List<String> elemVals        = new List<String>();
                while (elems != null        && elems.MoveNext()) {
                    XmlNode elem             = (XmlNode)elems.Current;
                    XmlAttribute elemUri     = GetRdfResourceAttribute(elem);

                    #region Container Resource Item
                    //This is a container of resources
                    if (elemUri             != null) {

                        //Sanitize eventual blank node value detected by presence of "nodeID" attribute
                        if (elemUri.LocalName.Equals("nodeID", StringComparison.Ordinal)) {
                            if (!elemUri.Value.StartsWith("bnode:")) {
                                elemUri.Value= "bnode:" + elemUri.Value;
                            }
                        }

                        //obj -> rdf:_N -> VALUE 
                        if (contType        == RDFModelEnums.RDFContainerTypes.Alt) {
                            if (!elemVals.Contains(elemUri.Value)) {
                                elemVals.Add(elemUri.Value);
                                result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), new RDFResource(elemUri.Value)));
                            }
                        }
                        else {
                            result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), new RDFResource(elemUri.Value)));
                        }

                    }
                    #endregion

                    #region Container Literal Item
                    //This is a container of literals
                    else {

                        //Parse the literal contained in the item
                        RDFLiteral literal   = null;
                        XmlAttribute attr    = GetRdfDatatypeAttribute(elem);
                        if (attr            != null) {
                            literal          = new RDFTypedLiteral(elem.InnerText, RDFModelUtilities.GetDatatypeFromString(attr.InnerText));
                        }
                        else {
                            attr             = GetXmlLangAttribute(elem);
                            literal          = new RDFPlainLiteral(elem.InnerText, (attr != null ? attr.InnerText : String.Empty));
                        }

                        //obj -> rdf:_N -> VALUE 
                        if (contType        == RDFModelEnums.RDFContainerTypes.Alt) {
                            if (!elemVals.Contains(literal.ToString())) {
                                elemVals.Add(literal.ToString());
                                result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), literal));
                            }
                        }
                        else {
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