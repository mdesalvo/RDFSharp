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
    /// RDFConstructQuery is the SPARQL "CONSTRUCT" query implementation.
    /// </summary>
    public class RDFConstructQuery: RDFQuery {

        #region Properties
        /// <summary>
        /// List of template patterns carried by the query
        /// </summary>
        internal List<RDFPattern> Templates { get; set; }

        /// <summary>
        /// List of variables carried by the template patterns of the query
        /// </summary>
        internal List<RDFVariable> Variables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty CONSTRUCT query
        /// </summary>
        public RDFConstructQuery() {
            this.Templates = new List<RDFPattern>();
            this.Variables = new List<RDFVariable>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the CONSTRUCT query
        /// </summary>
        public override String ToString() {
            StringBuilder query    = new StringBuilder();

            // CONSTRUCT
            query.Append("CONSTRUCT {\n");

            // TEMPLATES
            this.Templates.ForEach(tp => {
                String tpString    = tp.ToString();

                //Remove the Context from the template print (since it is not supported by CONSTRUCT query)
                if (tp.Context    != null) {
                    tpString       = tpString.Replace("GRAPH " + tp.Context + " { ", String.Empty).TrimEnd(new Char[] { ' ', '}' });
                }

                //Remove the Optional indicator from the template print (since it is not supported by CONSTRUCT query)
                if (tp.IsOptional) {
                    tpString       = tpString.Replace("OPTIONAL { ", String.Empty).TrimEnd(new Char[] { ' ', '}' });
                }

                query.Append("  "  + tpString + " .\n");
            });
            query.Append("}\nWHERE {\n");

            #region QUERY MEMBERS
            Boolean printingUnion       = false;
            RDFQueryMember lastQueryMbr = this.GetEvaluableMembers().LastOrDefault();
            foreach(var queryMember    in this.GetEvaluableMembers()) {

                #region PATTERN GROUP
                if (queryMember        is RDFPatternGroup) {

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
            query.Append("\n}");
            #endregion

            #region MODIFIERS
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
        /// Adds the given pattern to the templates of the query
        /// </summary>
        public RDFConstructQuery AddTemplate(RDFPattern template) {
            if (template != null) {
                if (!this.Templates.Any(tp => tp.Equals(template))) {
                     this.Templates.Add(template);
                     
                     //Context
                     if (template.Context != null && template.Context is RDFVariable) {
                         if (!this.Variables.Any(v => v.Equals(template.Context))) {
                              this.Variables.Add((RDFVariable)template.Context);
                         }
                     }
                     
                     //Subject
                     if (template.Subject is RDFVariable) {
                         if (!this.Variables.Any(v => v.Equals(template.Subject))) {
                              this.Variables.Add((RDFVariable)template.Subject);
                         }
                     }
                     
                     //Predicate
                     if (template.Predicate is RDFVariable) {
                         if (!this.Variables.Any(v => v.Equals(template.Predicate))) {
                              this.Variables.Add((RDFVariable)template.Predicate);
                         }
                     }
                     
                     //Object
                     if (template.Object is RDFVariable) {
                         if (!this.Variables.Any(v => v.Equals(template.Object))) {
                              this.Variables.Add((RDFVariable)template.Object);
                         }
                     }

                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given pattern group to the body of the query
        /// </summary>
        public RDFConstructQuery AddPatternGroup(RDFPatternGroup patternGroup) {
            if (patternGroup != null) {
                if (!this.GetPatternGroups().Any(q => q.PatternGroupName.Equals(patternGroup.PatternGroupName, StringComparison.OrdinalIgnoreCase))) {
                     this.QueryMembers.Add(patternGroup);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFConstructQuery AddModifier(RDFLimitModifier modifier) {
            if (modifier != null) {
                if (!this.GetModifiers().Any(m => m is RDFLimitModifier)) {
                     this.QueryMembers.Add(modifier);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFConstructQuery AddModifier(RDFOffsetModifier modifier) {
            if (modifier != null) {
                if (!this.GetModifiers().Any(m => m is RDFOffsetModifier)) {
                     this.QueryMembers.Add(modifier);
                }
            }
            return this;
        }

        /// <summary>
        /// Applies the query to the given graph 
        /// </summary>
        public RDFConstructQueryResult ApplyToGraph(RDFGraph graph) {
            if (graph != null) {
                return this.ApplyToDataSource(graph);
            }
            return new RDFConstructQueryResult(this.ToString());
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFConstructQueryResult ApplyToStore(RDFStore store) {
            if (store != null) {
                return this.ApplyToDataSource(store);
            }
            return new RDFConstructQueryResult(this.ToString());
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFConstructQueryResult ApplyToFederation(RDFFederation federation) {
            if (federation != null) {
                return this.ApplyToDataSource(federation);
            }
            return new RDFConstructQueryResult(this.ToString());
        }

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFConstructQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint) {
            RDFConstructQueryResult constructResult = new RDFConstructQueryResult(this.ToString());
            if (sparqlEndpoint                     != null) {
                RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluating CONSTRUCT query on SPARQL endpoint '{0}'...", sparqlEndpoint));

                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient          = new WebClient()) {

                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(this.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                    webClient.Headers.Add(HttpRequestHeader.Accept, "text/turtle");

                    //Send querystring to SPARQL endpoint
                    var sparqlResponse              = webClient.DownloadData(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse             != null) {
                        using (var sStream          = new MemoryStream(sparqlResponse)) {
                            constructResult         = RDFConstructQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                        }
                        constructResult.ConstructResults.TableName = this.ToString();
                    }

                }

                RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluated CONSTRUCTQuery on SPARQL endpoint '{0}': Found '{1}' results.", sparqlEndpoint, constructResult.ConstructResultsCount));
            }
            return constructResult;
        }

        /// <summary>
        /// Applies the query to the given datasource
        /// </summary>
        internal RDFConstructQueryResult ApplyToDataSource(RDFDataSource datasource) {
            this.QueryMemberResultTables.Clear();
            this.PatternGroupMemberResultTables.Clear();
            RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluating CONSTRUCT query on DataSource '{0}'...", datasource));

            RDFConstructQueryResult constructResult    = new RDFConstructQueryResult(this.ToString());
            if (this.GetEvaluableMembers().Any()) {

                //Iterate the evaluable members of the query
                var fedPatternGroupMemberResultTables  = new Dictionary<Int64, List<DataTable>>();
                foreach (var evaluableMember          in this.GetEvaluableMembers()) {

                    #region PATTERN GROUP
                    if (evaluableMember               is RDFPatternGroup) {
                        RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluating PatternGroup '{0}' on DataSource '{1}'...", (RDFPatternGroup)evaluableMember, datasource));

                        //Step 1: Get the intermediate result tables of the current pattern group
                        if (datasource.IsFederation()) {

                            #region TrueFederations
                            foreach (var store        in (RDFFederation)datasource) {

                                //Step FED.1: Evaluate the patterns of the current pattern group on the current store
                                RDFQueryEngine.EvaluatePatternGroup(this, (RDFPatternGroup)evaluableMember, store);

                                //Step FED.2: Federate the patterns of the current pattern group on the current store
                                if (!fedPatternGroupMemberResultTables.ContainsKey(evaluableMember.QueryMemberID)) {
                                     fedPatternGroupMemberResultTables.Add(evaluableMember.QueryMemberID, this.PatternGroupMemberResultTables[evaluableMember.QueryMemberID]);
                                }
                                else {
                                     fedPatternGroupMemberResultTables[evaluableMember.QueryMemberID].ForEach(fprt =>
                                       fprt.Merge(this.PatternGroupMemberResultTables[evaluableMember.QueryMemberID].Single(prt => prt.TableName.Equals(fprt.TableName, StringComparison.Ordinal)), true, MissingSchemaAction.Add));
                                }

                            }
                            this.PatternGroupMemberResultTables[evaluableMember.QueryMemberID] = fedPatternGroupMemberResultTables[evaluableMember.QueryMemberID];
                            #endregion

                        }
                        else {
                            RDFQueryEngine.EvaluatePatternGroup(this, (RDFPatternGroup)evaluableMember, datasource);
                        }

                        //Step 2: Get the result table of the current pattern group
                        RDFQueryEngine.FinalizePatternGroup(this, (RDFPatternGroup)evaluableMember);

                        //Step 3: Apply the filters of the current pattern group to its result table
                        RDFQueryEngine.ApplyFilters(this, (RDFPatternGroup)evaluableMember);
                    }
                    #endregion

                }

                //Step 4: Get the result table of the query
                DataTable queryResultTable             = RDFQueryUtilities.CombineTables(this.QueryMemberResultTables.Values.ToList(), false);

                //Step 5: Fill the templates from the result table
                DataTable filledResultTable            = RDFQueryEngine.FillTemplates(this, queryResultTable);

                //Step 6: Apply the modifiers of the query to the result table
                constructResult.ConstructResults       = RDFQueryEngine.ApplyModifiers(this, filledResultTable);

            }
            RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluated SPARQL CONSTRUCT query on DataSource '{0}': Found '{1}' results.", datasource, constructResult.ConstructResultsCount));

            constructResult.ConstructResults.TableName = this.ToString();
            return constructResult;
        }
        #endregion

    }

}