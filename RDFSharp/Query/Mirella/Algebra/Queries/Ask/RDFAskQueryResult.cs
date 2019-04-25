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
using System.IO;
using System.Text;
using System.Xml;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFAskResult is a container for SPARQL "ASK" query results.
    /// </summary>
    public class RDFAskQueryResult
    {

        #region Properties
        /// <summary>
        /// Boolean response of the ASK query
        /// </summary>
        public Boolean AskResult { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ASK result
        /// </summary>
        internal RDFAskQueryResult()
        {
            this.AskResult = false;
        }
        #endregion

        #region Methods

        #region Write
        /// <summary>
        /// Writes the "SPARQL Query Results XML Format" stream corresponding to the ASK query result
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
                    sparqlRoot.AppendChild(sparqlHeadElement);
                    #endregion

                    #region sparqlResults
                    XmlNode sparqlResultsElement = sparqlDoc.CreateNode(XmlNodeType.Element, "boolean", null);
                    XmlText askResultText = sparqlDoc.CreateTextNode(this.AskResult.ToString().ToUpperInvariant());
                    sparqlResultsElement.AppendChild(askResultText);
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
        /// Writes the "SPARQL Query Results XML Format" file corresponding to the ASK query result
        /// </summary>
        public void ToSparqlXmlResult(String filepath)
        {
            ToSparqlXmlResult(new FileStream(filepath, FileMode.Create));
        }
        #endregion

        #region Read
        /// <summary>
        /// Reads the given "SPARQL Query Results XML Format" stream into an ASK query result
        /// </summary>
        public static RDFAskQueryResult FromSparqlXmlResult(Stream inputStream)
        {
            try
            {

                #region deserialize
                RDFAskQueryResult result = new RDFAskQueryResult();
                using (StreamReader streamReader = new StreamReader(inputStream, Encoding.UTF8))
                {
                    using (XmlTextReader xmlReader = new XmlTextReader(streamReader))
                    {
                        xmlReader.DtdProcessing = DtdProcessing.Parse;
                        xmlReader.Normalization = false;

                        #region load
                        XmlDocument srxDoc = new XmlDocument();
                        srxDoc.Load(xmlReader);
                        #endregion

                        #region parse
                        Boolean foundHead = false;
                        Boolean foundBoolean = false;
                        var nodesEnum = srxDoc.DocumentElement.ChildNodes.GetEnumerator();
                        while (nodesEnum != null && nodesEnum.MoveNext())
                        {
                            XmlNode node = (XmlNode)nodesEnum.Current;

                            #region HEAD
                            if (node.Name.ToUpperInvariant().Equals("HEAD", StringComparison.Ordinal))
                            {
                                foundHead = true;
                            }
                            #endregion

                            #region BOOLEAN
                            else if (node.Name.ToUpperInvariant().Equals("BOOLEAN", StringComparison.Ordinal))
                            {
                                foundBoolean = true;
                                if (foundHead)
                                {
                                    Boolean bRes = false;
                                    if (Boolean.TryParse(node.InnerText, out bRes))
                                    {
                                        result.AskResult = bRes;
                                    }
                                    else
                                    {
                                        throw new Exception("\"boolean\" node contained data not corresponding to a valid Boolean.");
                                    }
                                }
                                else
                                {
                                    throw new Exception("\"head\" node was not found, or was after \"boolean\" node.");
                                }
                            }
                            #endregion

                        }

                        if (!foundHead)
                        {
                            throw new Exception("mandatory \"head\" node was not found");
                        }
                        if (!foundBoolean)
                        {
                            throw new Exception("mandatory \"boolean\" node was not found");
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

        /// <summary>
        /// Reads the given "SPARQL Query Results XML Format" file into an ASK query result
        /// </summary>
        public static RDFAskQueryResult FromSparqlXmlResult(String filepath)
        {
            return FromSparqlXmlResult(new FileStream(filepath, FileMode.Open));
        }
        #endregion

        #endregion

    }

}