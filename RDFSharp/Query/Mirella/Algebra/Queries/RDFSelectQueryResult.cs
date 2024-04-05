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

using RDFSharp.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFSelectQueryResult is a container for SPARQL "SELECT" query results.
    /// </summary>
    public class RDFSelectQueryResult : RDFQueryResult
    {
        #region Properties
        /// <summary>
        /// Tabular response of the query
        /// </summary>
        public DataTable SelectResults { get; internal set; }

        /// <summary>
        /// Gets the number of results produced by the query
        /// </summary>
        public long SelectResultsCount
            => SelectResults.Rows.Count;
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SELECT result
        /// </summary>
        internal RDFSelectQueryResult()
            => SelectResults = new DataTable();
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
                using (XmlTextWriter sparqlWriter = new XmlTextWriter(outputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    sparqlWriter.Formatting = Formatting.Indented;

                    #region xmlDecl
                    XmlDocument sparqlDoc = new XmlDocument();
                    sparqlDoc.AppendChild(sparqlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                    #endregion

                    #region sparqlRoot
                    XmlNode sparqlRoot = sparqlDoc.CreateNode(XmlNodeType.Element, "sparql", null);
                    XmlAttribute sparqlRootNS = sparqlDoc.CreateAttribute("xmlns");
                    XmlText sparqlRootNSText = sparqlDoc.CreateTextNode("http://www.w3.org/2005/sparql-results#");
                    sparqlRootNS.AppendChild(sparqlRootNSText);
                    sparqlRoot.Attributes.Append(sparqlRootNS);

                    #region sparqlHead
                    XmlNode sparqlHeadElement = sparqlDoc.CreateNode(XmlNodeType.Element, "head", null);
                    IEnumerator resultColumns = SelectResults.Columns.GetEnumerator();
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
                    IEnumerator resultRows = SelectResults.Rows.GetEnumerator();
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
                                else if (rdfTerm is RDFLiteral rdfTermLit)
                                {
                                    XmlNode litElement = sparqlDoc.CreateNode(XmlNodeType.Element, "literal", null);
                                    if (rdfTermLit is RDFPlainLiteral rdfTermPLit)
                                    {
                                        if (rdfTermPLit.HasLanguage())
                                        {
                                            XmlAttribute xmlLang = sparqlDoc.CreateAttribute(string.Concat(RDFVocabulary.XML.PREFIX, ":lang"), RDFVocabulary.XML.BASE_URI);
                                            XmlText xmlLangText = sparqlDoc.CreateTextNode(rdfTermPLit.Language);
                                            xmlLang.AppendChild(xmlLangText);
                                            litElement.Attributes.Append(xmlLang);
                                        }
                                        XmlText plainLiteralText = sparqlDoc.CreateTextNode(RDFModelUtilities.EscapeControlCharsForXML(HttpUtility.HtmlDecode(rdfTermLit.Value)));
                                        litElement.AppendChild(plainLiteralText);
                                    }
                                    else
                                    {
                                        XmlAttribute datatype = sparqlDoc.CreateAttribute("datatype");
                                        XmlText datatypeText = sparqlDoc.CreateTextNode(RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)rdfTermLit).Datatype));
                                        datatype.AppendChild(datatypeText);
                                        litElement.Attributes.Append(datatype);
                                        XmlText typedLiteralText = sparqlDoc.CreateTextNode(RDFModelUtilities.EscapeControlCharsForXML(HttpUtility.HtmlDecode(rdfTermLit.Value)));
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
            catch (Exception ex) { throw new RDFQueryException("Cannot serialize SPARQL XML RESULT because: " + ex.Message, ex); }
        }

        /// <summary>
        /// Asynchronously writes the "SPARQL Query Results XML Format" stream corresponding to the SELECT query result
        /// </summary>
        public Task ToSparqlXmlResultAsync(Stream outputStream)
            => Task.Run(() => ToSparqlXmlResult(outputStream));

        /// <summary>
        /// Writes the "SPARQL Query Results XML Format" file corresponding to the SELECT query result
        /// </summary>
        public void ToSparqlXmlResult(string filepath)
            => ToSparqlXmlResult(new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Asynchronously writes the "SPARQL Query Results XML Format" file corresponding to the SELECT query result
        /// </summary>
        public Task ToSparqlXmlResultAsync(string filepath)
            => Task.Run(() => ToSparqlXmlResult(filepath));
        #endregion

        #region Read
        /// <summary>
        /// Reads the given "SPARQL Query Results XML Format" file into a SELECT query result
        /// </summary>
        public static RDFSelectQueryResult FromSparqlXmlResult(string filepath)
            => FromSparqlXmlResult(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Asynchronously reads the given "SPARQL Query Results XML Format" file into a SELECT query result
        /// </summary>
        public static Task<RDFSelectQueryResult> FromSparqlXmlResultAsync(string filepath)
            => Task.Run(() => FromSparqlXmlResult(filepath));

        /// <summary>
        /// Reads the given "SPARQL Query Results XML Format" stream into a SELECT query result
        /// </summary>
        public static RDFSelectQueryResult FromSparqlXmlResult(Stream inputStream)
        {
            RDFSelectQueryResult result = new RDFSelectQueryResult();

            try
            {
                #region deserialize                
                using (StreamReader streamReader = new StreamReader(inputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(streamReader)
                            { DtdProcessing = DtdProcessing.Parse, XmlResolver = null, Normalization = false })
                    {
                        #region document
                        XmlDocument srxDoc = new XmlDocument() { XmlResolver = null };
                        srxDoc.Load(xmlReader);
                        #endregion

                        #region results
                        bool foundHead = false;
                        bool foundResults = false;
                        IEnumerator nodesEnum = srxDoc.DocumentElement.ChildNodes.GetEnumerator();
                        while (nodesEnum?.MoveNext() ?? false)
                        {
                            XmlNode node = (XmlNode)nodesEnum.Current;

                            #region HEAD
                            if (string.Equals(node.Name, "HEAD", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!node.HasChildNodes)
                                    throw new Exception("\"head\" node was found without children.");

                                foundHead = true;
                                IEnumerator variablesEnum = node.ChildNodes.GetEnumerator();
                                while (variablesEnum?.MoveNext() ?? false)
                                {
                                    #region VARIABLE
                                    XmlNode varNode = (XmlNode)variablesEnum.Current;
                                    if (string.Equals(varNode.Name, "VARIABLE", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (varNode.Attributes == null || varNode.Attributes.Count == 0)
                                            throw new Exception("one \"variable\" node was found without attributes.");
                                        if (string.IsNullOrEmpty(varNode.Attributes["name"]?.Value))
                                            throw new Exception("one \"variable\" node was found without, or with empty, \"name\" attribute.");

                                        RDFQueryEngine.AddColumn(result.SelectResults, varNode.Attributes["name"].Value);
                                    }
                                    #endregion
                                }
                            }
                            #endregion

                            #region RESULTS
                            else if (node.Name.ToUpperInvariant().Equals("RESULTS", StringComparison.Ordinal))
                            {
                                if (!foundHead)
                                    throw new Exception("\"head\" node was not found, or was after \"results\" node.");
                                
                                foundResults = true;
                                IEnumerator resultsEnum = node.ChildNodes.GetEnumerator();
                                while (resultsEnum?.MoveNext() ?? false)
                                {
                                    XmlNode resNode = (XmlNode)resultsEnum.Current;

                                    #region RESULT
                                    if (string.Equals(resNode.Name, "RESULT", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (resNode.HasChildNodes)
                                        {
                                            Dictionary<string, string> results = new Dictionary<string, string>();
                                            IEnumerator bindingsEnum = resNode.ChildNodes.GetEnumerator();
                                            while (bindingsEnum?.MoveNext() ?? false)
                                            {
                                                XmlNode bindingNode = (XmlNode)bindingsEnum.Current;
                                                bool foundUri = false;
                                                bool foundLit = false;

                                                #region BINDING
                                                if (string.Equals(bindingNode.Name, "BINDING", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (bindingNode.Attributes == null || bindingNode.Attributes.Count == 0)
                                                        throw new Exception("one \"binding\" node was found without attributes.");
                                                    if (string.IsNullOrEmpty(bindingNode.Attributes["name"]?.Value))
                                                        throw new Exception("one \"binding\" node was found without, or with empty, \"name\" attribute.");
                                                    if (!bindingNode.HasChildNodes)
                                                        throw new Exception("one \"binding\" node was found without children.");

                                                    #region URI / BNODE
                                                    if (string.Equals(bindingNode.FirstChild.Name, "URI", StringComparison.OrdinalIgnoreCase) 
                                                         || string.Equals(bindingNode.FirstChild.Name, "BNODE", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        if (RDFModelUtilities.GetUriFromString(bindingNode.InnerText) == null)
                                                            throw new Exception("one \"uri\" node contained data not corresponding to a valid Uri.");

                                                        foundUri = true;
                                                        results.Add(bindingNode.Attributes["name"].Value, bindingNode.InnerText);
                                                    }
                                                    #endregion

                                                    #region LITERAL
                                                    else if (string.Equals(bindingNode.FirstChild.Name, "LITERAL", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        foundLit = true;
                                                        if (bindingNode.FirstChild.Attributes == null || bindingNode.FirstChild.Attributes.Count == 0)
                                                            results.Add(bindingNode.Attributes["name"].Value, bindingNode.InnerText);
                                                        else if (!string.IsNullOrEmpty(bindingNode.FirstChild.Attributes["datatype"]?.Value))
                                                            results.Add(bindingNode.Attributes["name"].Value, string.Concat(bindingNode.FirstChild.InnerText, "^^", bindingNode.FirstChild.Attributes["datatype"].Value));
                                                        else if (!string.IsNullOrEmpty(bindingNode.FirstChild.Attributes["xml:lang"]?.Value))
                                                            results.Add(bindingNode.Attributes["name"].Value, string.Concat(bindingNode.FirstChild.InnerText, "@", bindingNode.FirstChild.Attributes["xml:lang"].Value));
                                                        else
                                                            throw new Exception("one \"literal\" node was found with attribute different from \"datatype\" or \"xml:lang\".");
                                                    }
                                                    #endregion
                                                }
                                                #endregion

                                                if (!foundUri && !foundLit)
                                                    throw new Exception("one \"binding\" node was found without mandatory child \"uri\" or \"literal\".");
                                            }
                                            RDFQueryEngine.AddRow(result.SelectResults, results);
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                        }

                        if (!foundHead)
                            throw new Exception("mandatory \"head\" node was not found");
                        if (!foundResults)
                            throw new Exception("mandatory \"results\" node was not found");
                        #endregion
                    }
                }
                #endregion
            }
            catch (Exception ex) { throw new RDFQueryException("Cannot read given \"SPARQL Query Results XML Format\" source because: " + ex.Message, ex); }

            return result;
        }

        /// <summary>
        /// Asynchronously reads the given "SPARQL Query Results XML Format" stream into a SELECT query result
        /// </summary>
        public static Task<RDFSelectQueryResult> FromSparqlXmlResultAsync(Stream inputStream)
            => Task.Run(() => FromSparqlXmlResult(inputStream));
        #endregion

        #endregion
    }
}