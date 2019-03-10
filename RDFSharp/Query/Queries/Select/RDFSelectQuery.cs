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

namespace RDFSharp.Query {

    /// <summary>
    /// RDFSelectQuery is the SPARQL "SELECT" query implementation.
    /// </summary>
    public class RDFSelectQuery: RDFQuery {

        #region Properties
        /// <summary>
        /// Dictionary of projection variables and associated ordinals
        /// </summary>
        internal Dictionary<RDFVariable, Int32> ProjectionVars { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SELECT query
        /// </summary>
        public RDFSelectQuery() {
            this.ProjectionVars = new Dictionary<RDFVariable, Int32>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SELECT query
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

            #region SELECT
            query.Append("SELECT");
            #endregion

            #region DISTINCT
            this.GetModifiers().Where(mod => mod is RDFDistinctModifier)
                               .ToList()
                               .ForEach(dm => query.Append(" " + dm));
            #endregion

            #region VARIABLES
            if (this.ProjectionVars.Any()) {
                this.ProjectionVars.OrderBy(x => x.Value).ToList().ForEach(variable => query.Append(" " + variable.Key));
            }
            else {
                query.Append(" *");
            }
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
                            query.Append(((RDFPatternGroup)queryMember).ToString(2) + "    UNION");
                        }

                        //Current pattern group IS the last of the query (so UNION keyword must not be appended at last)
                        else {
                            //End the Union block
                            if (printingUnion) {
                                printingUnion  = false;
                                query.Append(((RDFPatternGroup)queryMember).ToString(2));
                                query.Append("  }\n");
                            }
                            else {
                                query.Append(((RDFPatternGroup)queryMember).ToString());
                            }
                        }

                    }

                    //Current pattern group is set as INTERSECT with the next one
                    else {
                        //End the Union block
                        if (printingUnion) {
                            printingUnion      = false;
                            query.Append(((RDFPatternGroup)queryMember).ToString(2));
                            query.Append("  }\n");
                        }
                        else {
                            query.Append(((RDFPatternGroup)queryMember).ToString());
                        }
                    }

                }
                #endregion

            }
            #endregion

            query.Append("\n}");
            #endregion

            #region MODIFIERS
            // ORDER BY
            if (this.GetModifiers().Any(mod     => mod is RDFOrderByModifier)) {
                query.Append("\nORDER BY");
                this.GetModifiers().Where(mod   => mod is RDFOrderByModifier)
                                   .ToList()
                                   .ForEach(om  => query.Append(" " + om));
            }

            // LIMIT/OFFSET
            if (this.GetModifiers().Any(mod     => mod is RDFLimitModifier || mod is RDFOffsetModifier)) {
                this.GetModifiers().Where(mod   => mod is RDFLimitModifier)
                                   .ToList()
                                   .ForEach(lim => query.Append("\n" + lim));
                this.GetModifiers().Where(mod   => mod is RDFOffsetModifier)
                                   .ToList()
                                   .ForEach(off => query.Append("\n" + off));
            }
            #endregion

            return query.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFSelectQuery AddPatternGroup(RDFPatternGroup patternGroup) {
            if (patternGroup != null) {
                if (!this.GetPatternGroups().Any(q => q.Equals(patternGroup))) {
                     this.QueryMembers.Add(patternGroup);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given variable to the results of the query
        /// </summary>
        public RDFSelectQuery AddProjectionVariable(RDFVariable projectionVariable) {
            if (projectionVariable != null) {
                if (!this.ProjectionVars.Any(pv => pv.Key.ToString().Equals(projectionVariable.ToString(), StringComparison.OrdinalIgnoreCase))) {
                     this.ProjectionVars.Add(projectionVariable, this.ProjectionVars.Count);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the SELECT query
        /// </summary>
        public RDFSelectQuery AddModifier(RDFModifier modifier) {
            if (modifier != null) {
            
                //Ensure to have only one distinct modifier in the query
                if (modifier is RDFDistinctModifier && this.GetModifiers().Any(m => m is RDFDistinctModifier)) {
                    return this;
                }
                //Ensure to have only one limit modifier in the query
                if (modifier is RDFLimitModifier    && this.GetModifiers().Any(m => m is RDFLimitModifier)) {
                    return this;
                }
                //Ensure to have only one offset modifier in the query
                if (modifier is RDFOffsetModifier   && this.GetModifiers().Any(m => m is RDFOffsetModifier)) {
                    return this;
                }
                //Ensure to have only one orderby modifier per variable in the query
                if (modifier is RDFOrderByModifier  && this.GetModifiers().Any(m => m is RDFOrderByModifier && ((RDFOrderByModifier)m).Variable.Equals(((RDFOrderByModifier)modifier).Variable))) {
                    return this;
                }

                //Add the modifier, avoiding duplicates
                if (!this.GetModifiers().Any(m => m.Equals(modifier))) {
                     this.QueryMembers.Add(modifier);
                }

            }
            return this;
        }

        /// <summary>
        /// Applies the query to the given graph 
        /// </summary>
        public RDFSelectQueryResult ApplyToGraph(RDFGraph graph) {
            if (graph != null) {
                return RDFQueryEngine.CreateNew().EvaluateSelectQuery(this, graph);
            }
            else {
                return new RDFSelectQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFSelectQueryResult ApplyToStore(RDFStore store) {
            if (store != null) {
                return RDFQueryEngine.CreateNew().EvaluateSelectQuery(this, store);
            }
            else {
                return new RDFSelectQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFSelectQueryResult ApplyToFederation(RDFFederation federation) {
            if (federation != null) {
                return RDFQueryEngine.CreateNew().EvaluateSelectQuery(this, federation);
            }
            else {
                return new RDFSelectQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFSelectQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint) {
            RDFSelectQueryResult selResult = new RDFSelectQueryResult();
            if (sparqlEndpoint            != null) {
                RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Evaluating SELECT query on SPARQL endpoint '{0}'...", sparqlEndpoint));
				
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
                            selResult      = RDFSelectQueryResult.FromSparqlXmlResult(sStream);
                        }
                        selResult.SelectResults.TableName = this.ToString();
                    }

                }

                RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Evaluated SELECT query on SPARQL endpoint '{0}': Found '{1}' results.", sparqlEndpoint, selResult.SelectResultsCount));
            }
            return selResult;
        }
        #endregion

    }

}