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
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFAskResult is a container for SPARQL "ASK" query results.
    /// </summary>
    public class RDFAskQueryResult : RDFQueryResult
    {
        #region Properties
        /// <summary>
        /// Boolean response of the ASK query
        /// </summary>
        public bool AskResult { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ASK result
        /// </summary>
        internal RDFAskQueryResult()
            => AskResult = false;
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
                    sparqlRoot.AppendChild(sparqlHeadElement);
                    #endregion

                    #region sparqlResults
                    XmlNode sparqlResultsElement = sparqlDoc.CreateNode(XmlNodeType.Element, "boolean", null);
                    XmlText askResultText = sparqlDoc.CreateTextNode(AskResult.ToString().ToUpperInvariant());
                    sparqlResultsElement.AppendChild(askResultText);
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
        /// Asynchronously writes the "SPARQL Query Results XML Format" stream corresponding to the ASK query result
        /// </summary>
        public Task ToSparqlXmlResultAsync(Stream outputStream)
            => Task.Run(() => ToSparqlXmlResult(outputStream));

        /// <summary>
        /// Writes the "SPARQL Query Results XML Format" file corresponding to the ASK query result
        /// </summary>
        public void ToSparqlXmlResult(string filepath)
            => ToSparqlXmlResult(new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Asynchronously writes the "SPARQL Query Results XML Format" file corresponding to the ASK query result
        /// </summary>
        public Task ToSparqlXmlResultAsync(string filepath)
            => Task.Run(() => ToSparqlXmlResult(filepath));
        #endregion

        #region Read
        /// <summary>
        /// Reads the given "SPARQL Query Results XML Format" stream into an ASK query result
        /// </summary>
        public static RDFAskQueryResult FromSparqlXmlResult(Stream inputStream)
        {
            RDFAskQueryResult result = new RDFAskQueryResult();

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

                        #region load
                        XmlDocument srxDoc = new XmlDocument { XmlResolver = null };
                        srxDoc.Load(xmlReader);
                        #endregion

                        #region parse
                        bool foundHead = false;
                        bool foundBoolean = false;
                        foreach (XmlNode node in srxDoc.DocumentElement.ChildNodes)
                        {
                            #region HEAD
                            if (string.Equals(node.Name, "HEAD", StringComparison.OrdinalIgnoreCase))
                                foundHead = true;
                            #endregion

                            #region BOOLEAN
                            else if (string.Equals(node.Name, "BOOLEAN", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!foundHead)
                                    throw new Exception("\"head\" node was not found, or was after \"boolean\" node.");
                                if (!bool.TryParse(node.InnerText, out bool bRes))
                                    throw new Exception("\"boolean\" node contained data not corresponding to a valid Boolean.");

                                foundBoolean = true;
                                result.AskResult = bRes;
                            }
                            #endregion
                        }

                        if (!foundHead)
                            throw new Exception("mandatory \"head\" node was not found");
                        if (!foundBoolean)
                            throw new Exception("mandatory \"boolean\" node was not found");
                        #endregion
                    }
                }                
                #endregion
            }
            catch (Exception ex) { throw new RDFQueryException("Cannot read given \"SPARQL Query Results XML Format\" source because: " + ex.Message, ex); }

            return result;
        }

        /// <summary>
        /// Asynchronously reads the given "SPARQL Query Results XML Format" stream into an ASK query result
        /// </summary>
        public static Task<RDFAskQueryResult> FromSparqlXmlResultAsync(Stream inputStream)
            => Task.Run(() => FromSparqlXmlResult(inputStream));

        /// <summary>
        /// Reads the given "SPARQL Query Results XML Format" file into an ASK query result
        /// </summary>
        public static RDFAskQueryResult FromSparqlXmlResult(string filepath)
            => FromSparqlXmlResult(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Asynchronously reads the given "SPARQL Query Results XML Format" file into an ASK query result
        /// </summary>
        public static Task<RDFAskQueryResult> FromSparqlXmlResultAsync(string filepath)
            => Task.Run(() => FromSparqlXmlResult(filepath));
        #endregion

        #endregion
    }
}