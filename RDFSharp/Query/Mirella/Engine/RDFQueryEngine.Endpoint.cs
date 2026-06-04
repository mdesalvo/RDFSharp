/*
   Copyright 2012-2026 Marco De Salvo

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
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Query
{
    // RDFQueryEngine (MIRELLA): raw SPARQL endpoint query dispatch.
    internal partial class RDFQueryEngine
    {
        /// <summary>
        /// Applies the given raw string query to the given SPARQL endpoint
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        internal static RDFQueryResult ApplyRawToSPARQLEndpoint(string queryType, string query, RDFSPARQLEndpoint sparqlEndpoint, RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions)
        {
            #region Utilities
            void AdjustVariableColumnNames(DataTable qrTable)
            {
                //Eventually adjust column names (should start with "?")
                int columnsCount = qrTable.Columns.Count;
                for (int i = 0; i < columnsCount; i++)
                {
                    if (!qrTable.Columns[i].ColumnName.StartsWith("?", StringComparison.Ordinal))
                        qrTable.Columns[i].ColumnName = $"?{qrTable.Columns[i].ColumnName}";
                }
            }
            #endregion

            RDFQueryResult queryResult = null;
            if (!string.IsNullOrWhiteSpace(query) && sparqlEndpoint != null)
            {
                if (sparqlEndpointQueryOptions == null)
                    sparqlEndpointQueryOptions = new RDFSPARQLEndpointQueryOptions();

                //Establish a connection to the given SPARQL endpoint
                using (RDFWebClient webClient = new RDFWebClient(sparqlEndpointQueryOptions.TimeoutMilliseconds))
                {
                    //Parse user-provided parameters
                    string defaultGraphUri = sparqlEndpoint.QueryParams.Get("default-graph-uri");
                    string namedGraphUri = sparqlEndpoint.QueryParams.Get("named-graph-uri");

                    //Insert request headers
                    sparqlEndpoint.FillWebClientAuthorization(webClient);
                    switch (queryType)
                    {
                        case "ASK":
                        case "SELECT":
                            webClient.Headers.Add(HttpRequestHeader.Accept, "application/sparql-results+xml");
                            break;

                        case "CONSTRUCT":
                        case "DESCRIBE":
                            webClient.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                            webClient.Headers.Add(HttpRequestHeader.Accept, "text/turtle");
                            break;
                    }

                    //Prepare request (GET vs POST)
                    byte[] sparqlResponse = null;
                    try
                    {
                        switch (sparqlEndpointQueryOptions.QueryMethod)
                        {
                            //query via GET with URL-encoded querystring
                            case RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Get:
                                //Handle user-provided parameters
                                webClient.QueryString.Add("query", HttpUtility.UrlEncode(query));
                                webClient.QueryString.Add(sparqlEndpoint.QueryParams);
                                //Send query to SPARQL endpoint
                                sparqlResponse = webClient.DownloadData(sparqlEndpoint.BaseAddress);
                                break;

                            //query via POST with URL-encoded body
                            case RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post:
                                webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                                //Handle user-provided parameters
                                string queryString = $"query={HttpUtility.UrlEncode(query)}";
                                if (!string.IsNullOrEmpty(defaultGraphUri))
                                    queryString = $"using-graph-uri={HttpUtility.UrlEncode(defaultGraphUri)}&{queryString}";
                                if (!string.IsNullOrEmpty(namedGraphUri))
                                    queryString = $"using-named-graph-uri={HttpUtility.UrlEncode(namedGraphUri)}&{queryString}";
                                //Send query to SPARQL endpoint
                                sparqlResponse = Encoding.UTF8.GetBytes(webClient.UploadString(sparqlEndpoint.BaseAddress, queryString));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (sparqlEndpointQueryOptions.ErrorBehavior == RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)
                            throw new RDFQueryException($"{queryType} query on SPARQL endpoint failed because: {ex.Message}", ex);
                    }

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                        {
                            switch (queryType)
                            {
                                case "ASK":
                                    queryResult = RDFAskQueryResult.FromSparqlXmlResult(sStream);
                                    break;

                                case "SELECT":
                                    queryResult = RDFSelectQueryResult.FromSparqlXmlResult(sStream);
                                    AdjustVariableColumnNames(((RDFSelectQueryResult)queryResult).SelectResults);
                                    break;

                                case "CONSTRUCT":
                                    queryResult = RDFConstructQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                                    AdjustVariableColumnNames(((RDFConstructQueryResult)queryResult).ConstructResults);
                                    break;

                                case "DESCRIBE":
                                    queryResult = RDFDescribeQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                                    AdjustVariableColumnNames(((RDFDescribeQueryResult)queryResult).DescribeResults);
                                    break;
                            }
                        }
                }
            }

            //Adjust to give eventual empty result
            switch (queryType)
            {
                case "ASK":
                    return queryResult ?? new RDFAskQueryResult();

                case "SELECT":
                    return queryResult ?? new RDFSelectQueryResult();

                case "CONSTRUCT":
                    return queryResult ?? new RDFConstructQueryResult();

                case "DESCRIBE":
                    return queryResult ?? new RDFDescribeQueryResult();
            }

            return queryResult;
        }
    }
}