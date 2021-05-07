/*
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

using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFQueryUtilities is a collector of reusable utility methods for RDF query management
    /// </summary>
    public static class RDFQueryUtilities
    {

        #region MIRELLA RDF
        /// <summary>
        /// Parses the given string to return an instance of pattern member
        /// </summary>
        internal static RDFPatternMember ParseRDFPatternMember(string pMember)
        {
            if (pMember == null)
                throw new RDFQueryException("Cannot parse pattern member because given \"pMember\" parameter is null.");
            
            #region Uri
            if (Uri.TryCreate(pMember, UriKind.Absolute, out _))
                return new RDFResource(pMember);
            #endregion

            #region Plain Literal
            if (!pMember.Contains("^^")
                    || pMember.EndsWith("^^")
                        || RDFModelUtilities.GetUriFromString(pMember.Substring(pMember.LastIndexOf("^^", StringComparison.Ordinal) + 2)) == null)
            {
                RDFPlainLiteral pLit = null;
                if (RDFNTriples.regexLPL.Match(pMember).Success)
                {
                    string pLitVal = pMember.Substring(0, pMember.LastIndexOf("@", StringComparison.Ordinal));
                    string pLitLng = pMember.Substring(pMember.LastIndexOf("@", StringComparison.Ordinal) + 1);
                    pLit = new RDFPlainLiteral(pLitVal, pLitLng);
                }
                else
                {
                    pLit = new RDFPlainLiteral(pMember);
                }
                return pLit;
            }
            #endregion

            #region Typed Literal
            string tLitValue = pMember.Substring(0, pMember.LastIndexOf("^^", StringComparison.Ordinal));
            string tLitDatatype = pMember.Substring(pMember.LastIndexOf("^^", StringComparison.Ordinal) + 2);
            RDFModelEnums.RDFDatatypes dt = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
            RDFTypedLiteral tLit = new RDFTypedLiteral(tLitValue, dt);
            return tLit;
            #endregion
        }

        /// <summary>
        /// Compares the given pattern members, throwing a "Type Error" whenever the comparison operator detects sematically incompatible members;
        /// </summary>
        internal static int CompareRDFPatternMembers(RDFPatternMember left, RDFPatternMember right)
        {

            #region NULLS
            if (left == null)
            {
                if (right == null)
                {
                    return 0;
                }
                return -1;
            }
            if (right == null)
            {
                return 1;
            }
            #endregion

            #region RESOURCE/CONTEXT
            if (left is RDFResource || left is RDFContext)
            {

                //RESOURCE/CONTEXT VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                {
                    return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }

                //RESOURCE/CONTEXT VS TYPEDLITERAL
                else
                {
                    if (((RDFTypedLiteral)right).HasStringDatatype())
                    {
                        return string.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

            }
            #endregion

            #region PLAINLITERAL
            else if (left is RDFPlainLiteral)
            {

                //PLAINLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                {
                    return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }

                //PLAINLITERAL VS TYPEDLITERAL
                else
                {
                    if (((RDFTypedLiteral)right).HasStringDatatype())
                    {
                        return string.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

            }
            #endregion

            #region TYPEDLITERAL
            else
            {

                //TYPEDLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                {
                    if (((RDFTypedLiteral)left).HasStringDatatype())
                    {
                        return string.Compare(((RDFTypedLiteral)left).Value, right.ToString(), StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

                //TYPEDLITERAL VS TYPEDLITERAL
                else
                {
                    if (((RDFTypedLiteral)left).HasBooleanDatatype() && ((RDFTypedLiteral)right).HasBooleanDatatype())
                    {
                        bool leftValueBoolean = bool.Parse(((RDFTypedLiteral)left).Value);
                        bool rightValueBoolean = bool.Parse(((RDFTypedLiteral)right).Value);
                        return leftValueBoolean.CompareTo(rightValueBoolean);
                    }
                    else if (((RDFTypedLiteral)left).HasDatetimeDatatype() && ((RDFTypedLiteral)right).HasDatetimeDatatype())
                    {
                        DateTime leftValueDateTime = DateTime.Parse(((RDFTypedLiteral)left).Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        DateTime rightValueDateTime = DateTime.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        return leftValueDateTime.CompareTo(rightValueDateTime);
                    }
                    else if (((RDFTypedLiteral)left).HasDecimalDatatype() && ((RDFTypedLiteral)right).HasDecimalDatatype())
                    {
                        decimal leftValueDecimal = decimal.Parse(((RDFTypedLiteral)left).Value, CultureInfo.InvariantCulture);
                        decimal rightValueDecimal = decimal.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture);
                        return leftValueDecimal.CompareTo(rightValueDecimal);
                    }
                    else if (((RDFTypedLiteral)left).HasStringDatatype() && ((RDFTypedLiteral)right).HasStringDatatype())
                    {
                        string leftValueString = ((RDFTypedLiteral)left).Value;
                        string rightValueString = ((RDFTypedLiteral)right).Value;
                        return leftValueString.CompareTo(rightValueString);
                    }
                    else if (((RDFTypedLiteral)left).HasTimespanDatatype() && ((RDFTypedLiteral)right).HasTimespanDatatype())
                    {
                        TimeSpan leftValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)left).Value);
                        TimeSpan rightValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)right).Value);
                        return leftValueDuration.CompareTo(rightValueDuration);
                    }
                    else
                    {
                        return -99; //Type Error
                    }
                }

            }
            #endregion

        }

        /// <summary>
        /// Tries to abbreviate the string representation of the given pattern member by searching for it in the given list of namespaces
        /// </summary>
        internal static (bool, string) AbbreviateRDFPatternMember(RDFPatternMember patternMember, List<RDFNamespace> prefixes)
        {
            #region Prefix Search
            //Check if the pattern member starts with a known prefix, if so just return it
            if (prefixes == null)
                prefixes = new List<RDFNamespace>();
            var prefixToSearch = patternMember.ToString().Split(':')[0];
            var searchedPrefix = prefixes.Find(pf => pf.NamespacePrefix.Equals(prefixToSearch, StringComparison.OrdinalIgnoreCase));
            if (searchedPrefix != null)
            {
                return (true, patternMember.ToString());
            }
            #endregion

            #region Namespace Search
            //Check if the pattern member starts with a known namespace, if so replace it with its prefix
            string pmString = patternMember.ToString();
            bool abbrev = false;
            prefixes.ForEach(ns =>
            {
                if (!abbrev)
                {
                    string nS = ns.ToString();
                    if (!pmString.Equals(nS, StringComparison.OrdinalIgnoreCase))
                    {
                        if (pmString.StartsWith(nS))
                        {
                            pmString = pmString.Replace(nS, string.Concat(ns.NamespacePrefix, ":")).TrimEnd(new char[] { '/' });

                            //Accept the abbreviation only if it has generated a valid XSD QName
                            try
                            {
                                var qn = new RDFTypedLiteral(pmString, RDFModelEnums.RDFDatatypes.XSD_QNAME);
                                abbrev = true;
                            }
                            catch
                            {
                                pmString = patternMember.ToString();
                                abbrev = false;
                            }
                        }
                    }
                }
            });
            return (abbrev, pmString);
            #endregion
        }

        /// <summary>
        /// Removes the duplicates from the given list of T elements
        /// </summary>
        internal static List<T> RemoveDuplicates<T>(List<T> elements) where T : RDFPatternMember
        {
            List<T> results = new List<T>();
            if (elements?.Count > 0)
            {
                HashSet<long> lookup = new HashSet<long>();
                elements.ForEach(element =>
                {
                    if (!lookup.Contains(element.PatternMemberID))
                    {
                        lookup.Add(element.PatternMemberID);
                        results.Add(element);
                    }
                });
            }
            return results;
        }
        #endregion

        #region MIRELLA ASYNC
        /// <summary>
        /// Asynchronously applies the query to the given data source
        /// </summary>
        internal static async Task<RDFSelectQueryResult> ApplyToDataSourceAsync(this RDFSelectQuery selectQuery, RDFDataSource dataSource)
        {
            if (selectQuery != null && dataSource != null)
            {
                switch (dataSource)
                {
                    case RDFGraph graph:
                        return await selectQuery.ApplyToGraphAsync(graph);
                    case RDFStore store:
                        return await selectQuery.ApplyToStoreAsync(store);
                    case RDFFederation federation:
                        return await selectQuery.ApplyToFederationAsync(federation);
                    case RDFSPARQLEndpoint sparqlEndpoint:
                        return await selectQuery.ApplyToSPARQLEndpointAsync(sparqlEndpoint);
                }
            }
            return new RDFSelectQueryResult();
        }

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public static async Task<RDFSelectQueryResult> ApplyToGraphAsync(this RDFSelectQuery selectQuery, RDFGraph graph)
        => graph != null ? await new RDFQueryAsyncEngine().EvaluateSelectQueryAsync(selectQuery, graph)
                         : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public static async Task<RDFAskQueryResult> ApplyToGraphAsync(this RDFAskQuery askQuery, RDFGraph graph)
        => graph != null ? await new RDFQueryAsyncEngine().EvaluateAskQueryAsync(askQuery, graph)
                         : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public static async Task<RDFConstructQueryResult> ApplyToGraphAsync(this RDFConstructQuery constructQuery, RDFGraph graph)
            => graph != null ? await new RDFQueryAsyncEngine().EvaluateConstructQueryAsync(constructQuery, graph)
                             : new RDFConstructQueryResult(constructQuery?.ToString());

        /// <summary>
        /// Asynchronously applies the query to the given graph
        /// </summary>
        public static async Task<RDFDescribeQueryResult> ApplyToGraphAsync(this RDFDescribeQuery describeQuery, RDFGraph graph)
            => graph != null ? await new RDFQueryAsyncEngine().EvaluateDescribeQueryAsync(describeQuery, graph)
                             : new RDFDescribeQueryResult(describeQuery?.ToString());

        /// <summary>
        /// Asynchronously applies the query to the given store
        /// </summary>
        public static async Task<RDFSelectQueryResult> ApplyToStoreAsync(this RDFSelectQuery selectQuery, RDFStore store)
            => store != null ? await new RDFQueryAsyncEngine().EvaluateSelectQueryAsync(selectQuery, store)
                             : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given store
        /// </summary>
        public static async Task<RDFAskQueryResult> ApplyToStoreAsync(this RDFAskQuery askQuery, RDFStore store)
            => store != null ? await new RDFQueryAsyncEngine().EvaluateAskQueryAsync(askQuery, store)
                             : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given store
        /// </summary>
        public static async Task<RDFConstructQueryResult> ApplyToStoreAsync(this RDFConstructQuery constructQuery, RDFStore store)
            => store != null ? await new RDFQueryAsyncEngine().EvaluateConstructQueryAsync(constructQuery, store)
                             : new RDFConstructQueryResult(constructQuery?.ToString());

        /// <summary>
        /// Asynchronously applies the query to the given store
        /// </summary>
        public static async Task<RDFDescribeQueryResult> ApplyToStoreAsync(this RDFDescribeQuery describeQuery, RDFStore store)
            => store != null ? await new RDFQueryAsyncEngine().EvaluateDescribeQueryAsync(describeQuery, store)
                             : new RDFDescribeQueryResult(describeQuery?.ToString());

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public static async Task<RDFSelectQueryResult> ApplyToFederationAsync(this RDFSelectQuery selectQuery, RDFFederation federation)
            => federation != null ? await new RDFQueryAsyncEngine().EvaluateSelectQueryAsync(selectQuery, federation)
                                  : new RDFSelectQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public static async Task<RDFAskQueryResult> ApplyToFederationAsync(this RDFAskQuery askQuery, RDFFederation federation)
            => federation != null ? await new RDFQueryAsyncEngine().EvaluateAskQueryAsync(askQuery, federation)
                                  : new RDFAskQueryResult();

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public static async Task<RDFConstructQueryResult> ApplyToFederationAsync(this RDFConstructQuery constructQuery, RDFFederation federation)
            => federation != null ? await new RDFQueryAsyncEngine().EvaluateConstructQueryAsync(constructQuery, federation)
                                  : new RDFConstructQueryResult(constructQuery?.ToString());

        /// <summary>
        /// Asynchronously applies the query to the given federation
        /// </summary>
        public static async Task<RDFDescribeQueryResult> ApplyToFederationAsync(this RDFDescribeQuery describeQuery, RDFFederation federation)
            => federation != null ? await new RDFQueryAsyncEngine().EvaluateDescribeQueryAsync(describeQuery, federation)
                                  : new RDFDescribeQueryResult(describeQuery?.ToString());

        /// <summary>
        /// Asynchronously asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public static async Task<RDFSelectQueryResult> ApplyToSPARQLEndpointAsync(this RDFSelectQuery selectQuery, RDFSPARQLEndpoint sparqlEndpoint)
        {
            RDFSelectQueryResult selResult = new RDFSelectQueryResult();
            if (selectQuery != null && sparqlEndpoint != null)
            {
                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient())
                {
                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(selectQuery.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/sparql-results+xml");

                    //Send querystring to SPARQL endpoint
                    byte[] sparqlResponse = await webClient.DownloadDataTaskAsync(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                            selResult = await Task.Run(() => RDFSelectQueryResult.FromSparqlXmlResult(sStream));
                        selResult.SelectResults.TableName = selectQuery.ToString();
                    }
                }

                //Eventually adjust column names (should start with "?")
                int columnsCount = selResult.SelectResults.Columns.Count;
                for (int i = 0; i < columnsCount; i++)
                {
                    if (!selResult.SelectResults.Columns[i].ColumnName.StartsWith("?"))
                        selResult.SelectResults.Columns[i].ColumnName = string.Concat("?", selResult.SelectResults.Columns[i].ColumnName);
                }
            }
            return selResult;
        }

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public static async Task<RDFAskQueryResult> ApplyToSPARQLEndpointAsync(this RDFAskQuery askQuery, RDFSPARQLEndpoint sparqlEndpoint)
        {
            RDFAskQueryResult askResult = new RDFAskQueryResult();
            if (askQuery != null && sparqlEndpoint != null)
            {
                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient())
                {
                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(askQuery.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/sparql-results+xml");

                    //Send querystring to SPARQL endpoint
                    byte[] sparqlResponse = await webClient.DownloadDataTaskAsync(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                            askResult = await Task.Run(() => RDFAskQueryResult.FromSparqlXmlResult(sStream));
                    }
                }
            }
            return askResult;
        }

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public static async Task<RDFConstructQueryResult> ApplyToSPARQLEndpointAsync(this RDFConstructQuery constructQuery, RDFSPARQLEndpoint sparqlEndpoint)
        {
            RDFConstructQueryResult constructResult = new RDFConstructQueryResult(constructQuery?.ToString());
            if (constructQuery != null && sparqlEndpoint != null)
            {
                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient())
                {
                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(constructQuery.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                    webClient.Headers.Add(HttpRequestHeader.Accept, "text/turtle");

                    //Send querystring to SPARQL endpoint
                    byte[] sparqlResponse = await webClient.DownloadDataTaskAsync(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                            constructResult = await Task.Run(() => RDFConstructQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream)));
                        constructResult.ConstructResults.TableName = constructQuery.ToString();
                    }
                }

                //Eventually adjust column names (should start with "?")
                int columnsCount = constructResult.ConstructResults.Columns.Count;
                for (int i = 0; i < columnsCount; i++)
                {
                    if (!constructResult.ConstructResults.Columns[i].ColumnName.StartsWith("?"))
                        constructResult.ConstructResults.Columns[i].ColumnName = string.Concat("?", constructResult.ConstructResults.Columns[i].ColumnName);
                }
            }
            return constructResult;
        }

        /// <summary>
        /// Asynchronously applies the query to the given SPARQL endpoint
        /// </summary>
        public static async Task<RDFDescribeQueryResult> ApplyToSPARQLEndpointAsync(this RDFDescribeQuery describeQuery, RDFSPARQLEndpoint sparqlEndpoint)
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult(describeQuery?.ToString());
            if (describeQuery != null && sparqlEndpoint != null)
            {
                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient())
                {
                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(describeQuery.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                    webClient.Headers.Add(HttpRequestHeader.Accept, "text/turtle");

                    //Send querystring to SPARQL endpoint
                    byte[] sparqlResponse = await webClient.DownloadDataTaskAsync(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (MemoryStream sStream = new MemoryStream(sparqlResponse))
                            describeResult = await Task.Run(() => RDFDescribeQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream)));
                        describeResult.DescribeResults.TableName = describeQuery.ToString();
                    }
                }

                //Eventually adjust column names (should start with "?")
                int columnsCount = describeResult.DescribeResults.Columns.Count;
                for (int i = 0; i < columnsCount; i++)
                {
                    if (!describeResult.DescribeResults.Columns[i].ColumnName.StartsWith("?"))
                        describeResult.DescribeResults.Columns[i].ColumnName = string.Concat("?", describeResult.DescribeResults.Columns[i].ColumnName);
                }
            }
            return describeResult;
        }

        #endregion

    }

}