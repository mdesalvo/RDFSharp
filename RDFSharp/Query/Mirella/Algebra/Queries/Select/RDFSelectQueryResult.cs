/*
   Copyright 2012-2019 Marco De Salvo

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
using System.Data;
using System.IO;
using System.Text;
using System.Xml;
using System.Web;
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFSelectQueryResult is a container for SPARQL "SELECT" query results.
    /// </summary>
    public class RDFSelectQueryResult
    {

        #region Properties
        /// <summary>
        /// Tabular response of the query
        /// </summary>
        public DataTable SelectResults { get; internal set; }

        /// <summary>
        /// Gets the number of results produced by the query
        /// </summary>
        public Int64 SelectResultsCount
        {
            get { return this.SelectResults.Rows.Count; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SELECT result
        /// </summary>
        internal RDFSelectQueryResult()
        {
            this.SelectResults = new DataTable();
        }
        #endregion

        #region Methods

        #region Write
        /// <summary>
        /// Writes the "SPARQL Query Results XML Format" stream corresponding to the SELECT query result
        /// </summary>
        public void ToSparqlXmlResult(Stream outputStream)
        {
            try
            {

                #region serialize
                using (XmlTextWriter sparqlWriter = new XmlTextWriter(outputStream, Encoding.UTF8))
                {
                    XmlDocument sparqlDoc = new XmlDocument();
                    sparqlWriter.Formatting = Formatting.Indented;

                    #region xmlDecl
                    XmlDeclaration sparqlDecl = sparqlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    sparqlDoc.AppendChild(sparqlDecl);
                    #endregion

                    #region sparqlRoot
                    XmlNode sparqlRoot = sparqlDoc.CreateNode(XmlNodeType.Element, "sparql", null);
                    XmlAttribute sparqlRootNS = sparqlDoc.CreateAttribute("xmlns");
                    XmlText sparqlRootNSText = sparqlDoc.CreateTextNode("http://www.w3.org/2005/sparql-results#");
                    sparqlRootNS.AppendChild(sparqlRootNSText);
                    sparqlRoot.Attributes.Append(sparqlRootNS);

                    #region sparqlHead
                    XmlNode sparqlHeadElement = sparqlDoc.CreateNode(XmlNodeType.Element, "head", null);
                    IEnumerator resultColumns = this.SelectResults.Columns.GetEnumerator();
                    while (resultColumns.MoveNext())
                    {
                        XmlNode variableElement = sparqlDoc.CreateNode(XmlNodeType.Element, "variable", null);
                        XmlAttribute varElName = sparqlDoc.CreateAttribute("name");
                        XmlText varElNameText = sparqlDoc.CreateTextNode(resultColumns.Current.ToString());
                        varElName.AppendChild(varElNameText);
                        variableElement.Attributes.Append(varElName);
                        sparqlHeadElement.AppendChild(variableElement);
                    }
                    sparqlRoot.AppendChild(sparqlHeadElement);
                    #endregion

                    #region sparqlResults
                    XmlNode sparqlResultsElement = sparqlDoc.CreateNode(XmlNodeType.Element, "results", null);
                    IEnumerator resultRows = this.SelectResults.Rows.GetEnumerator();
                    while (resultRows.MoveNext())
                    {
                        resultColumns.Reset();
                        XmlNode resultElement = sparqlDoc.CreateNode(XmlNodeType.Element, "result", null);
                        while (resultColumns.MoveNext())
                        {
                            if (!((DataRow)resultRows.Current).IsNull(resultColumns.Current.ToString()))
                            {
                                XmlNode bindingElement = sparqlDoc.CreateNode(XmlNodeType.Element, "binding", null);
                                XmlAttribute bindElName = sparqlDoc.CreateAttribute("name");
                                XmlText bindElNameText = sparqlDoc.CreateTextNode(resultColumns.Current.ToString());
                                bindElName.AppendChild(bindElNameText);
                                bindingElement.Attributes.Append(bindElName);

                                #region RDFTerm
                                RDFPatternMember rdfTerm = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)[resultColumns.Current.ToString()].ToString());
                                if (rdfTerm is RDFResource)
                                {
                                    if (rdfTerm.ToString().StartsWith("bnode:"))
                                    {
                                        XmlNode bnodeElement = sparqlDoc.CreateNode(XmlNodeType.Element, "bnode", null);
                                        XmlText bnodeElText = sparqlDoc.CreateTextNode(rdfTerm.ToString());
                                        bnodeElement.AppendChild(bnodeElText);
                                        bindingElement.AppendChild(bnodeElement);
                                    }
                                    else
                                    {
                                        XmlNode uriElement = sparqlDoc.CreateNode(XmlNodeType.Element, "uri", null);
                                        XmlText uriElText = sparqlDoc.CreateTextNode(rdfTerm.ToString());
                                        uriElement.AppendChild(uriElText);
                                        bindingElement.AppendChild(uriElement);
                                    }
                                }
                                else if (rdfTerm is RDFLiteral)
                                {
                                    XmlNode litElement = sparqlDoc.CreateNode(XmlNodeType.Element, "literal", null);
                                    if (rdfTerm is RDFPlainLiteral)
                                    {
                                        if (((RDFPlainLiteral)rdfTerm).Language != String.Empty)
                                        {
                                            XmlAttribute xmlLang = sparqlDoc.CreateAttribute(RDFVocabulary.XML.PREFIX + ":lang", RDFVocabulary.XML.BASE_URI);
                                            XmlText xmlLangText = sparqlDoc.CreateTextNode(((RDFPlainLiteral)rdfTerm).Language);
                                            xmlLang.AppendChild(xmlLangText);
                                            litElement.Attributes.Append(xmlLang);
                                        }
                                        XmlText plainLiteralText = sparqlDoc.CreateTextNode(RDFModelUtilities.EscapeControlCharsForXML(HttpUtility.HtmlDecode(((RDFLiteral)rdfTerm).Value)));
                                        litElement.AppendChild(plainLiteralText);
                                    }
                                    else
                                    {
                                        XmlAttribute datatype = sparqlDoc.CreateAttribute("datatype");
                                        XmlText datatypeText = sparqlDoc.CreateTextNode(RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)rdfTerm).Datatype));
                                        datatype.AppendChild(datatypeText);
                                        litElement.Attributes.Append(datatype);
                                        XmlText typedLiteralText = sparqlDoc.CreateTextNode(RDFModelUtilities.EscapeControlCharsForXML(HttpUtility.HtmlDecode(((RDFLiteral)rdfTerm).Value)));
                                        litElement.AppendChild(typedLiteralText);
                                    }
                                    bindingElement.AppendChild(litElement);
                                }
                                #endregion

                                resultElement.AppendChild(bindingElement);
                            }
                        }
                        sparqlResultsElement.AppendChild(resultElement);
                    }
                    sparqlRoot.AppendChild(sparqlResultsElement);
                    #endregion

                    sparqlDoc.AppendChild(sparqlRoot);
                    #endregion

                    sparqlDoc.Save(sparqlWriter);
                }
                #endregion

            }
            catch (Exception ex)
            {
                throw new RDFQueryException("Cannot serialize SPARQL XML RESULT because: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Writes the "SPARQL Query Results XML Format" file corresponding to the SELECT query result
        /// </summary>
        public void ToSparqlXmlResult(String filepath)
        {
            ToSparqlXmlResult(new FileStream(filepath, FileMode.Create));
        }
        #endregion

        #region Read
        /// <summary>
        /// Reads the given "SPARQL Query Results XML Format" file into a SELECT query result
        /// </summary>
        public static RDFSelectQueryResult FromSparqlXmlResult(String filepath)
        {
            return FromSparqlXmlResult(new FileStream(filepath, FileMode.Open));
        }

        /// <summary>
        /// Reads the given "SPARQL Query Results XML Format" stream into a SELECT query result
        /// </summary>
        public static RDFSelectQueryResult FromSparqlXmlResult(Stream inputStream)
        {
            try
            {

                #region deserialize
                RDFSelectQueryResult result = new RDFSelectQueryResult();
                using (StreamReader streamReader = new StreamReader(inputStream, Encoding.UTF8))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(streamReader))
                    {
                        xmlReader.DtdProcessing = DtdProcessing.Parse;
                        xmlReader.Normalization = false;

                        #region document
                        XmlDocument srxDoc = new XmlDocument();
                        srxDoc.Load(xmlReader);
                        #endregion

                        #region results
                        Boolean foundHead = false;
                        Boolean foundResults = false;
                        var nodesEnum = srxDoc.DocumentElement.ChildNodes.GetEnumerator();
                        while (nodesEnum != null && nodesEnum.MoveNext())
                        {
                            XmlNode node = (XmlNode)nodesEnum.Current;

                            #region HEAD
                            if (node.Name.ToUpperInvariant().Equals("HEAD", StringComparison.Ordinal))
                            {
                                foundHead = true;
                                if (node.HasChildNodes)
                                {
                                    var variablesEnum = node.ChildNodes.GetEnumerator();
                                    while (variablesEnum != null && variablesEnum.MoveNext())
                                    {

                                        #region VARIABLE
                                        XmlNode varNode = (XmlNode)variablesEnum.Current;
                                        if (varNode.Name.ToUpperInvariant().Equals("VARIABLE", StringComparison.Ordinal))
                                        {
                                            if (varNode.Attributes.Count > 0)
                                            {
                                                XmlAttribute varAttr = varNode.Attributes["name"];
                                                if (varAttr != null && varAttr.Value != String.Empty)
                                                {
                                                    RDFQueryEngine.AddColumn(result.SelectResults, varAttr.Value);
                                                }
                                                else
                                                {
                                                    throw new Exception("one \"variable\" node was found without, or with empty, \"name\" attribute.");
                                                }
                                            }
                                            else
                                            {
                                                throw new Exception("one \"variable\" node was found without attributes.");
                                            }
                                        }
                                        #endregion

                                    }
                                }
                                else
                                {
                                    throw new Exception("\"head\" node was found without children.");
                                }
                            }
                            #endregion

                            #region RESULTS
                            else if (node.Name.ToUpperInvariant().Equals("RESULTS", StringComparison.Ordinal))
                            {
                                foundResults = true;
                                if (foundHead)
                                {
                                    var resultsEnum = node.ChildNodes.GetEnumerator();
                                    while (resultsEnum != null && resultsEnum.MoveNext())
                                    {
                                        XmlNode resNode = (XmlNode)resultsEnum.Current;

                                        #region RESULT
                                        if (resNode.Name.ToUpperInvariant().Equals("RESULT", StringComparison.Ordinal))
                                        {
                                            if (resNode.HasChildNodes)
                                            {
                                                var results = new Dictionary<String, String>();
                                                var bdgEnum = resNode.ChildNodes.GetEnumerator();
                                                while (bdgEnum != null && bdgEnum.MoveNext())
                                                {
                                                    XmlNode bdgNode = (XmlNode)bdgEnum.Current;
                                                    Boolean foundUri = false;
                                                    Boolean foundLit = false;

                                                    #region BINDING
                                                    if (bdgNode.Name.ToUpperInvariant().Equals("BINDING", StringComparison.Ordinal))
                                                    {
                                                        if (bdgNode.Attributes != null && bdgNode.Attributes.Count > 0)
                                                        {
                                                            XmlAttribute varAttr = bdgNode.Attributes["name"];
                                                            if (varAttr != null && varAttr.Value != String.Empty)
                                                            {
                                                                if (bdgNode.HasChildNodes)
                                                                {

                                                                    #region URI / BNODE
                                                                    if (bdgNode.FirstChild.Name.ToUpperInvariant().Equals("URI", StringComparison.Ordinal) ||
                                                                        bdgNode.FirstChild.Name.ToUpperInvariant().Equals("BNODE", StringComparison.Ordinal))
                                                                    {
                                                                        foundUri = true;
                                                                        if (RDFModelUtilities.GetUriFromString(bdgNode.InnerText) != null)
                                                                        {
                                                                            results.Add(varAttr.Value, bdgNode.InnerText);
                                                                        }
                                                                        else
                                                                        {
                                                                            throw new Exception("one \"uri\" node contained data not corresponding to a valid Uri.");
                                                                        }
                                                                    }
                                                                    #endregion

                                                                    #region LITERAL
                                                                    else if (bdgNode.FirstChild.Name.ToUpperInvariant().Equals("LITERAL", StringComparison.Ordinal))
                                                                    {
                                                                        foundLit = true;
                                                                        if (bdgNode.FirstChild.Attributes != null && bdgNode.FirstChild.Attributes.Count > 0)
                                                                        {
                                                                            XmlAttribute litAttr = bdgNode.FirstChild.Attributes["datatype"];
                                                                            if (litAttr != null && litAttr.Value != String.Empty)
                                                                            {
                                                                                results.Add(varAttr.Value, bdgNode.FirstChild.InnerText + "^^" + litAttr.Value);
                                                                            }
                                                                            else
                                                                            {
                                                                                litAttr = bdgNode.FirstChild.Attributes[RDFVocabulary.XML.PREFIX + ":lang"];
                                                                                if (litAttr != null && litAttr.Value != String.Empty)
                                                                                {
                                                                                    results.Add(varAttr.Value, bdgNode.FirstChild.InnerText + "@" + litAttr.Value);
                                                                                }
                                                                                else
                                                                                {
                                                                                    throw new Exception("one \"literal\" node was found with attribute different from \"datatype\" or \"xml:lang\".");
                                                                                }
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            results.Add(varAttr.Value, bdgNode.InnerText);
                                                                        }
                                                                    }
                                                                    #endregion

                                                                }
                                                                else
                                                                {
                                                                    throw new Exception("one \"binding\" node was found without children.");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("one \"binding\" node was found without, or with empty, \"name\" attribute.");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            throw new Exception("one \"binding\" node was found without attributes.");
                                                        }
                                                    }
                                                    #endregion

                                                    if (!foundUri && !foundLit)
                                                    {
                                                        throw new Exception("one \"binding\" node was found without mandatory child \"uri\" or \"literal\".");
                                                    }

                                                }
                                                RDFQueryEngine.AddRow(result.SelectResults, results);
                                            }
                                        }
                                        #endregion

                                    }
                                }
                                else
                                {
                                    throw new Exception("\"head\" node was not found, or was after \"results\" node.");
                                }
                            }
                            #endregion

                        }

                        if (!foundHead)
                        {
                            throw new Exception("mandatory \"head\" node was not found");
                        }
                        if (!foundResults)
                        {
                            throw new Exception("mandatory \"results\" node was not found");
                        }
                        #endregion

                    }
                }
                return result;
                #endregion

            }
            catch (Exception ex)
            {
                throw new RDFQueryException("Cannot read given \"SPARQL Query Results XML Format\" source because: " + ex.Message, ex);
            }
        }
        #endregion

        #endregion

    }

}