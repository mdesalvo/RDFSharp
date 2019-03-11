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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFAskQuery is the SPARQL "ASK" query implementation.
    /// </summary>
    public class RDFAskQuery: RDFQuery {

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ASK query
        /// </summary>
        public RDFAskQuery() { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the ASK query
        /// </summary>
        public override String ToString() {
            StringBuilder query = new StringBuilder();

            #region PREFIXES
            if (this.Prefixes.Any()) {
                this.Prefixes.ForEach(pf => {
                    query.Append("PREFIX " + pf.NamespacePrefix + ": <" + pf.NamespaceUri + ">\n");
                });
                query.Append("\n");
            }
            #endregion

            #region ASK
            query.Append("ASK");
            #endregion

            #region WHERE
            query.Append("\nWHERE {\n");

            #region EVALUABLEMEMBERS
            Boolean printingUnion        = false;
            RDFQueryMember lastQueryMbr  = this.GetEvaluableQueryMembers().LastOrDefault();
            foreach(var queryMember     in this.GetEvaluableQueryMembers()) {

                #region PATTERNGROUPS
                if (queryMember         is RDFPatternGroup) {

                    //Current pattern group is set as UNION with the next one
                    if (((RDFPatternGroup)queryMember).JoinAsUnion) {

                        //Current pattern group IS NOT the last of the query (so UNION keyword must be appended at last)
                        if (!queryMember.Equals(lastQueryMbr)) {
                             //Begin a new Union block
                             if (!printingUnion) {
                                  printingUnion = true;
                                  query.Append("\n  {");
                             }
                             query.Append(((RDFPatternGroup)queryMember).ToString(2, this.Prefixes) + "    UNION");
                        }

                        //Current pattern group IS the last of the query (so UNION keyword must not be appended at last)
                        else {
                            //End the Union block
                             if (printingUnion) {
                                 printingUnion  = false;
                                 query.Append(((RDFPatternGroup)queryMember).ToString(2, this.Prefixes));
                                 query.Append("  }\n");
                             }
                             else {
                                 query.Append(((RDFPatternGroup)queryMember).ToString(0, this.Prefixes));
                             }
                        }

                    }

                    //Current pattern group is set as INTERSECT with the next one
                    else {
                        //End the Union block
                        if (printingUnion) {
                            printingUnion       = false;
                            query.Append(((RDFPatternGroup)queryMember).ToString(2, this.Prefixes));
                            query.Append("  }\n");
                        }
                        else {
                            query.Append(((RDFPatternGroup)queryMember).ToString(0, this.Prefixes));
                        }
                    }

                }                
                #endregion

            }
            #endregion

            query.Append("\n}");
            #endregion

            return query.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFAskQuery AddPatternGroup(RDFPatternGroup patternGroup) {
            if (patternGroup != null) {
                if (!this.GetPatternGroups().Any(q => q.Equals(patternGroup))) {
                     this.QueryMembers.Add(patternGroup);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the query
        /// </summary>
        public RDFAskQuery AddPrefix(RDFNamespace prefix) {
            if (prefix != null) {
                if (!this.Prefixes.Any(p => p.Equals(prefix))) {
                     this.Prefixes.Add(prefix);
                }
            }
            return this;
        }

        /// <summary>
        /// Applies the query to the given graph 
        /// </summary>
        public RDFAskQueryResult ApplyToGraph(RDFGraph graph) {
            if (graph != null) {
                return RDFQueryEngine.CreateNew().EvaluateAskQuery(this, graph);
            }
            else {
                return new RDFAskQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFAskQueryResult ApplyToStore(RDFStore store) {
            if (store != null) {
                return RDFQueryEngine.CreateNew().EvaluateAskQuery(this, store);
            }
            else {
                return new RDFAskQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFAskQueryResult ApplyToFederation(RDFFederation federation) {
            if (federation != null) {
                return RDFQueryEngine.CreateNew().EvaluateAskQuery(this, federation);
            }
            else {
                return new RDFAskQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFAskQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint) {
            RDFAskQueryResult askResult    = new RDFAskQueryResult();
            if (sparqlEndpoint            != null) {
                RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluating ASK query on SPARQL endpoint '{0}'...", sparqlEndpoint));

                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient()) {

                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(this.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/sparql-results+xml");

                    //Send querystring to SPARQL endpoint
                    var sparqlResponse     = webClient.DownloadData(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse    != null) {
                        using (var sStream = new MemoryStream(sparqlResponse)) {
                            askResult      = RDFAskQueryResult.FromSparqlXmlResult(sStream);
                        }
                    }

                }

                RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluated ASK query on SPARQL endpoint '{0}': Result is '{1}'.", sparqlEndpoint, askResult.AskResult));
            }
            return askResult;
        }
        #endregion

    }

}