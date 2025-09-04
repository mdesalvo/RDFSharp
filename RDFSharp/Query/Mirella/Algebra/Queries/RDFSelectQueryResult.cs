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
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using RDFSharp.Model;

namespace RDFSharp.Query;

/// <summary>
/// RDFSelectQueryResult is a container for SPARQL "SELECT" query results.
/// </summary>
public sealed class RDFSelectQueryResult : RDFQueryResult
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
    /// Builds an empty SELECT result
    /// </summary>
    internal RDFSelectQueryResult()
        => SelectResults = new DataTable();
    #endregion

    #region Methods

    #region Write
    /// <summary>
    /// Writes the "SPARQL Query Results XML Format" stream corresponding to the SELECT query result
    /// </summary>
    /// <exception cref="RDFQueryException"></exception>
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
                foreach (DataColumn resultColumn in SelectResults.Columns)
                {
                    XmlNode variableElement = sparqlDoc.CreateNode(XmlNodeType.Element, "variable", null);
                    XmlAttribute varElName = sparqlDoc.CreateAttribute("name");
                    XmlText varElNameText = sparqlDoc.CreateTextNode(resultColumn.ToString());
                    varElName.AppendChild(varElNameText);
                    variableElement.Attributes.Append(varElName);
                    sparqlHeadElement.AppendChild(variableElement);
                }
                sparqlRoot.AppendChild(sparqlHeadElement);
                #endregion

                #region sparqlResults
                XmlNode sparqlResultsElement = sparqlDoc.CreateNode(XmlNodeType.Element, "results", null);
                foreach (DataRow resultRow in SelectResults.Rows)
                {
                    XmlNode resultElement = sparqlDoc.CreateNode(XmlNodeType.Element, "result", null);
                    foreach (DataColumn resultColumn in SelectResults.Columns)
                    {
                        string resultColumnString = resultColumn.ToString();
                        if (!resultRow.IsNull(resultColumnString))
                        {
                            XmlNode bindingElement = sparqlDoc.CreateNode(XmlNodeType.Element, "binding", null);
                            XmlAttribute bindElName = sparqlDoc.CreateAttribute("name");
                            XmlText bindElNameText = sparqlDoc.CreateTextNode(resultColumnString);
                            bindElName.AppendChild(bindElNameText);
                            bindingElement.Attributes.Append(bindElName);

                            #region RDFTerm
                            RDFPatternMember rdfTerm = RDFQueryUtilities.ParseRDFPatternMember(resultRow[resultColumnString].ToString());
                            switch (rdfTerm)
                            {
                                case RDFResource _ when rdfTerm.ToString().StartsWith("bnode:", StringComparison.OrdinalIgnoreCase):
                                {
                                    XmlNode bnodeElement = sparqlDoc.CreateNode(XmlNodeType.Element, "bnode", null);
                                    XmlText bnodeElText = sparqlDoc.CreateTextNode(rdfTerm.ToString());
                                    bnodeElement.AppendChild(bnodeElText);
                                    bindingElement.AppendChild(bnodeElement);
                                    break;
                                }
                                case RDFResource _:
                                {
                                    XmlNode uriElement = sparqlDoc.CreateNode(XmlNodeType.Element, "uri", null);
                                    XmlText uriElText = sparqlDoc.CreateTextNode(rdfTerm.ToString());
                                    uriElement.AppendChild(uriElText);
                                    bindingElement.AppendChild(uriElement);
                                    break;
                                }
                                case RDFLiteral rdfTermLit:
                                {
                                    XmlNode litElement = sparqlDoc.CreateNode(XmlNodeType.Element, "literal", null);
                                    if (rdfTermLit is RDFPlainLiteral rdfTermPLit)
                                    {
                                        if (rdfTermPLit.HasLanguage())
                                        {
                                            XmlAttribute xmlLang = sparqlDoc.CreateAttribute($"{RDFVocabulary.XML.PREFIX}:lang", RDFVocabulary.XML.BASE_URI);
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
                                        XmlText datatypeText = sparqlDoc.CreateTextNode(((RDFTypedLiteral)rdfTermLit).Datatype.URI.ToString());
                                        datatype.AppendChild(datatypeText);
                                        litElement.Attributes.Append(datatype);
                                        XmlText typedLiteralText = sparqlDoc.CreateTextNode(RDFModelUtilities.EscapeControlCharsForXML(HttpUtility.HtmlDecode(rdfTermLit.Value)));
                                        litElement.AppendChild(typedLiteralText);
                                    }
                                    bindingElement.AppendChild(litElement);
                                    break;
                                }
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
    /// Asynchronously writes the "SPARQL Query Results XML Format" stream corresponding to the SELECT query result
    /// </summary>
    /// <exception cref="RDFQueryException"></exception>
    public Task ToSparqlXmlResultAsync(Stream outputStream)
        => Task.Run(() => ToSparqlXmlResult(outputStream));

    /// <summary>
    /// Writes the "SPARQL Query Results XML Format" file corresponding to the SELECT query result
    /// </summary>
    /// <exception cref="RDFQueryException"></exception>
    public void ToSparqlXmlResult(string filepath)
        => ToSparqlXmlResult(new FileStream(filepath, FileMode.Create));

    /// <summary>
    /// Asynchronously writes the "SPARQL Query Results XML Format" file corresponding to the SELECT query result
    /// </summary>
    /// <exception cref="RDFQueryException"></exception>
    public Task ToSparqlXmlResultAsync(string filepath)
        => Task.Run(() => ToSparqlXmlResult(filepath));
    #endregion

    #region Read
    /// <summary>
    /// Reads the given "SPARQL Query Results XML Format" file into a SELECT query result
    /// </summary>
    /// <exception cref="RDFQueryException"></exception>
    public static RDFSelectQueryResult FromSparqlXmlResult(string filepath)
        => FromSparqlXmlResult(new FileStream(filepath, FileMode.Open));

    /// <summary>
    /// Asynchronously reads the given "SPARQL Query Results XML Format" file into a SELECT query result
    /// </summary>
    /// <exception cref="RDFQueryException"></exception>
    public static Task<RDFSelectQueryResult> FromSparqlXmlResultAsync(string filepath)
        => Task.Run(() => FromSparqlXmlResult(filepath));

    /// <summary>
    /// Reads the given "SPARQL Query Results XML Format" stream into a SELECT query result
    /// </summary>
    /// <exception cref="RDFQueryException"></exception>
    public static RDFSelectQueryResult FromSparqlXmlResult(Stream inputStream)
    {
        RDFSelectQueryResult result = new RDFSelectQueryResult();

        try
        {
            #region deserialize
            using (StreamReader streamReader = new StreamReader(inputStream, RDFModelUtilities.UTF8_NoBOM))
            {
                using (XmlTextReader xmlReader = new XmlTextReader(streamReader))
                {
                    xmlReader.DtdProcessing = DtdProcessing.Parse;
                    xmlReader.XmlResolver = null;
                    xmlReader.Normalization = false;

                    #region document
                    XmlDocument srxDoc = new XmlDocument { XmlResolver = null };
                    srxDoc.Load(xmlReader);
                    #endregion

                    #region results
                    bool foundHead = false;
                    bool foundResults = false;
                    foreach (XmlNode node in srxDoc.DocumentElement.ChildNodes)
                    {
                        #region HEAD
                        if (string.Equals(node.Name, "HEAD", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!node.HasChildNodes)
                                throw new Exception("\"head\" node was found without children.");

                            foundHead = true;
                            foreach (XmlNode varNode in node.ChildNodes)
                            {
                                #region VARIABLE
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
                        else if (string.Equals(node.Name, "RESULTS", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!foundHead)
                                throw new Exception("\"head\" node was not found, or was after \"results\" node.");

                            foundResults = true;
                            foreach (XmlNode resNode in node.ChildNodes)
                            {
                                #region RESULT
                                if (string.Equals(resNode.Name, "RESULT", StringComparison.OrdinalIgnoreCase) && resNode.HasChildNodes)
                                {
                                    Dictionary<string, string> results = [];
                                    foreach (XmlNode bindingNode in resNode.ChildNodes)
                                    {
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
                                                    results.Add(bindingNode.Attributes["name"].Value, $"{bindingNode.FirstChild.InnerText}^^{bindingNode.FirstChild.Attributes["datatype"].Value}");
                                                else if (!string.IsNullOrEmpty(bindingNode.FirstChild.Attributes["xml:lang"]?.Value))
                                                    results.Add(bindingNode.Attributes["name"].Value, $"{bindingNode.FirstChild.InnerText}@{bindingNode.FirstChild.Attributes["xml:lang"].Value}");
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
        catch (Exception ex)
        {
            throw new RDFQueryException("Cannot read given \"SPARQL Query Results XML Format\" source because: " + ex.Message, ex);
        }

        return result;
    }

    /// <summary>
    /// Asynchronously reads the given "SPARQL Query Results XML Format" stream into a SELECT query result
    /// </summary>
    /// <exception cref="RDFQueryException"></exception>
    public static Task<RDFSelectQueryResult> FromSparqlXmlResultAsync(Stream inputStream)
        => Task.Run(() => FromSparqlXmlResult(inputStream));
    #endregion

    #endregion
}