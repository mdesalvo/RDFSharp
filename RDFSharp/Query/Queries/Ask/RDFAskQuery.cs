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
using System.Threading.Tasks;
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

            // ASK
            query.Append("ASK\nWHERE {\n");

            #region PATTERN GROUPS
            Boolean printingUnion        = false;
            RDFPatternGroup lastQueryPG  = this.GetPatternGroups().LastOrDefault();
            foreach(var pg              in this.GetPatternGroups()) {

                //Current pattern group is set as UNION with the next one
                if (pg.JoinAsUnion) {

                    //Current pattern group IS NOT the last of the query (so UNION keyword must be appended at last)
                    if (!pg.Equals(lastQueryPG)) {
                         //Begin a new Union block
                         if (!printingUnion) {
                              printingUnion = true;
                              query.Append("\n  {");
                         }
                         query.Append(pg.ToString(2) + "    UNION");
                    }

                    //Current pattern group IS the last of the query (so UNION keyword must not be appended at last)
                    else {
                        //End the Union block
                         if (printingUnion) {
                             printingUnion = false;
                             query.Append(pg.ToString(2));
                             query.Append("  }\n");
                         }
                         else {
                             query.Append(pg.ToString());
                         }
                    }

                }

                //Current pattern group is set as INTERSECT with the next one
                else {
                    //End the Union block
                    if (printingUnion) {
                        printingUnion     = false;
                        query.Append(pg.ToString(2));
                        query.Append("  }\n");
                    }
                    else {
                        query.Append(pg.ToString());
                    }
                }

            }
            #endregion

            query.Append("\n}");
            return query.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFAskQuery AddPatternGroup(RDFPatternGroup patternGroup) {
            if (patternGroup != null) {
                if (!this.GetPatternGroups().Any(q => q.PatternGroupName.Equals(patternGroup.PatternGroupName, StringComparison.OrdinalIgnoreCase))) {
                     this.QueryMembers.Add(patternGroup);
                }
            }
            return this;
        }

        /// <summary>
        /// Applies the query to the given graph 
        /// </summary>
        public RDFAskQueryResult ApplyToGraph(RDFGraph graph) {
            if (graph != null) {
                return this.ApplyToDataSource(graph);
            }
            return new RDFAskQueryResult();
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFAskQueryResult ApplyToStore(RDFStore store) {
            if (store != null) {
                return this.ApplyToDataSource(store);
            }
            return new RDFAskQueryResult();
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFAskQueryResult ApplyToFederation(RDFFederation federation) {
            if (federation != null) {
                return this.ApplyToDataSource(federation);
            }
            return new RDFAskQueryResult();
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

                RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluated ASKQuery on SPARQL endpoint '{0}': Result is '{1}'.", sparqlEndpoint, askResult.AskResult));
            }
            return askResult;
        }

        /// <summary>
        /// Applies the query to the given datasource
        /// </summary>
        internal RDFAskQueryResult ApplyToDataSource(RDFDataSource datasource) {
            this.PatternGroupResultTables.Clear();
            this.PatternResultTables.Clear();
            RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluating ASK query on DataSource '{0}'...", datasource));

            RDFAskQueryResult askResult    = new RDFAskQueryResult();
            if (this.GetPatternGroups().Any()) {

                //Iterate the pattern groups of the query
                var fedPatternResultTables = new Dictionary<Int64, List<DataTable>>();
                foreach (var patternGroup in this.GetPatternGroups()) {
                    RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluating PatternGroup '{0}' on DataSource '{1}'...", patternGroup, datasource));

                    //Step 1: Get the intermediate result tables of the current pattern group
                    if (datasource.IsFederation()) {

                        #region TrueFederations
                        foreach(var store in (RDFFederation)datasource) {

                            //Step FED.1: Evaluate the patterns of the current pattern group on the current store
                            RDFQueryEngine.EvaluatePatternGroup(this, patternGroup, store);

                            //Step FED.2: Federate the patterns of the current pattern group on the current store
                            if (!fedPatternResultTables.ContainsKey(patternGroup.QueryMemberID)) {
                                 fedPatternResultTables.Add(patternGroup.QueryMemberID, this.PatternResultTables[patternGroup.QueryMemberID]);
                            }
                            else {
                                 fedPatternResultTables[patternGroup.QueryMemberID].ForEach(fprt =>
                                    fprt.Merge(this.PatternResultTables[patternGroup.QueryMemberID].Single(prt => prt.TableName.Equals(fprt.TableName, StringComparison.Ordinal)), true, MissingSchemaAction.Add));
                            }

                        }
                        this.PatternResultTables[patternGroup.QueryMemberID] = fedPatternResultTables[patternGroup.QueryMemberID];
                        #endregion

                    }
                    else {
                        RDFQueryEngine.EvaluatePatternGroup(this, patternGroup, datasource);
                    }

                    //Step 2: Get the result table of the current pattern group
                    RDFQueryEngine.FinalizePatternGroup(this, patternGroup);

                    //Step 3: Apply the filters of the current pattern group to its result table
                    RDFQueryEngine.ApplyFilters(this, patternGroup);

                }

                //Step 4: Get the result table of the query
                var queryResultTable = RDFQueryUtilities.CombineTables(this.PatternGroupResultTables.Values.ToList(), false);

                //Step 5: Transform the result into a boolean response 
                askResult.AskResult  = (queryResultTable.Rows.Count > 0);

            }
            RDFQueryEvents.RaiseASKQueryEvaluation(String.Format("Evaluated ASKQuery on DataSource '{0}': Result is '{1}'.", datasource, askResult.AskResult));
  
            return askResult;
        }
        #endregion

    }

}