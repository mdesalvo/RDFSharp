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
    /// RDFDescribeQuery is the SPARQL "DESCRIBE" query implementation.
    /// </summary>
    public class RDFDescribeQuery: RDFQuery {

        #region Properties
        /// <summary>
        /// List of RDF terms to be described by the query
        /// </summary>
        internal List<RDFPatternMember> DescribeTerms { get; set; }

        /// <summary>
        /// List of variables carried by the template patterns of the query
        /// </summary>
        internal List<RDFVariable> Variables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty DESCRIBE query
        /// </summary>
        public RDFDescribeQuery() {
            this.DescribeTerms = new List<RDFPatternMember>();
            this.Variables     = new List<RDFVariable>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the DESCRIBE query
        /// </summary>
        public override String ToString() {
            StringBuilder query = new StringBuilder();

            // DESCRIBE
            query.Append("DESCRIBE");

            // TERMS
            if (this.DescribeTerms.Any()) {
                this.DescribeTerms.ForEach(t => query.Append(" " + RDFQueryUtilities.PrintRDFPatternMember(t)));                   
            }
            else {
                query.Append(" *");
            }

            #region PATTERN GROUPS
            query.Append("\nWHERE {\n");
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
                        printingUnion = false;
                        query.Append(pg.ToString(2));
                        query.Append("  }\n");
                    }
                    else {
                        query.Append(pg.ToString());
                    }
                }

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
        /// Adds the given resource to the describe terms of the query
        /// </summary>
        public RDFDescribeQuery AddDescribeTerm(RDFResource describeTerm) {
            if (describeTerm != null) {
                if (!this.DescribeTerms.Any(dt => dt.Equals(describeTerm))) {
                     this.DescribeTerms.Add(describeTerm);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given variable to the describe terms of the query
        /// </summary>
        public RDFDescribeQuery AddDescribeTerm(RDFVariable describeVar) {
            if (describeVar != null) {
                if (!this.DescribeTerms.Any(dt => dt.Equals(describeVar))) {
                     this.DescribeTerms.Add(describeVar);
                     //Variable
                     if (!this.Variables.Any(v => v.Equals(describeVar))) {
                          this.Variables.Add(describeVar);
                     }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFDescribeQuery AddPatternGroup(RDFPatternGroup patternGroup) {
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
        public RDFDescribeQuery AddModifier(RDFLimitModifier modifier) {
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
        public RDFDescribeQuery AddModifier(RDFOffsetModifier modifier) {
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
        public RDFDescribeQueryResult ApplyToGraph(RDFGraph graph) {
            if (graph != null) {
                return this.ApplyToDataSource(graph);
            }
            return new RDFDescribeQueryResult(this.ToString());
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFDescribeQueryResult ApplyToStore(RDFStore store) {
            if (store != null) {
                return this.ApplyToDataSource(store);
            }
            return new RDFDescribeQueryResult(this.ToString());
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFDescribeQueryResult ApplyToFederation(RDFFederation federation) {
            if (federation != null) {
                return this.ApplyToDataSource(federation);
            }
            return new RDFDescribeQueryResult(this.ToString());
        }

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFDescribeQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint) {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult(this.ToString());
            if (sparqlEndpoint                   != null) {
                RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluating DESCRIBE query on SPARQL endpoint '{0}'...", sparqlEndpoint));
				
				//Establish a connection to the given SPARQL endpoint
                using (WebClient webClient        = new WebClient()) {

                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(this.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                    webClient.Headers.Add(HttpRequestHeader.Accept, "text/turtle");

                    //Send querystring to SPARQL endpoint
                    var sparqlResponse            = webClient.DownloadData(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse           != null) {
                        using (var sStream        = new MemoryStream(sparqlResponse)) {
                            describeResult        = RDFDescribeQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                        }
                        describeResult.DescribeResults.TableName = this.ToString();
                    }

                }

                RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluated DESCRIBE query on SPARQL endpoint '{0}': Found '{1}' results.", sparqlEndpoint, describeResult.DescribeResultsCount));
            }
            return describeResult;
        }

        /// <summary>
        /// Applies the query to the given datasource
        /// </summary>
        internal RDFDescribeQueryResult ApplyToDataSource(RDFDataSource datasource) {
            this.QueryMemberResultTables.Clear();
            this.PatternResultTables.Clear();
            RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluating SPARQL DESCRIBE query on DataSource '{0}'...", datasource));

            RDFDescribeQueryResult describeResult      = new RDFDescribeQueryResult(this.ToString());
            if (this.GetEvaluableMembers().Any()) {

                //Iterate the evaluable members of the query
                var fedPatternResultTables             = new Dictionary<Int64, List<DataTable>>();
                foreach (var evaluableMember          in this.GetEvaluableMembers()) {

                    #region PATTERN GROUP
                    if (evaluableMember               is RDFPatternGroup) {
                        RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluating PatternGroup '{0}' on DataSource '{1}'...", (RDFPatternGroup)evaluableMember, datasource));

                        //Step 1: Get the intermediate result tables of the current pattern group
                        if (datasource.IsFederation()) {

                            #region TrueFederations
                            foreach (var store        in (RDFFederation)datasource) {

                                //Step FED.1: Evaluate the patterns of the current pattern group on the current store
                                RDFQueryEngine.EvaluatePatternGroup(this, (RDFPatternGroup)evaluableMember, store);

                                //Step FED.2: Federate the patterns of the current pattern group on the current store
                                if (!fedPatternResultTables.ContainsKey(evaluableMember.QueryMemberID)) {
                                     fedPatternResultTables.Add(evaluableMember.QueryMemberID, this.PatternResultTables[evaluableMember.QueryMemberID]);
                                }
                                else {
                                     fedPatternResultTables[evaluableMember.QueryMemberID].ForEach(fprt =>
                                        fprt.Merge(this.PatternResultTables[evaluableMember.QueryMemberID].Single(prt => prt.TableName.Equals(fprt.TableName, StringComparison.Ordinal)), true, MissingSchemaAction.Add));
                                }

                            }
                            this.PatternResultTables[evaluableMember.QueryMemberID] = fedPatternResultTables[evaluableMember.QueryMemberID];
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

                //Step 5: Describe the terms from the result table
                DataTable describeResultTable          = new DataTable(this.ToString());
                if (datasource.IsFederation()) {

                    #region TrueFederations
                    foreach (var store                in (RDFFederation)datasource) {
                        describeResultTable.Merge(RDFQueryEngine.DescribeTerms(this, store, queryResultTable), true, MissingSchemaAction.Add);
                    }
                    #endregion

                }
                else {
                    describeResultTable                = RDFQueryEngine.DescribeTerms(this, datasource, queryResultTable);
                }

                //Step 6: Apply the modifiers of the query to the result table
                describeResult.DescribeResults         = RDFQueryEngine.ApplyModifiers(this, describeResultTable);

            }
            else {

                //In this case the only chance to proceed is to have resources in the describe terms,
                //which will be used to search for S-P-O data. Variables are ignored in this scenario.
                if (this.DescribeTerms.Any(dt          =>  dt is RDFResource)) {

                    //Step 1: Describe the terms from the result table
                    DataTable describeResultTable      = new DataTable(this.ToString());
                    if (datasource.IsFederation()) {

                        #region TrueFederations
                        foreach (var store            in (RDFFederation)datasource) {
                            describeResultTable.Merge(RDFQueryEngine.DescribeTerms(this, store, new DataTable()), true, MissingSchemaAction.Add);
                        }
                        #endregion

                    }
                    else {
                        describeResultTable            = RDFQueryEngine.DescribeTerms(this, datasource, new DataTable());
                    }

                    //Step 2: Apply the modifiers of the query to the result table
                    describeResult.DescribeResults     = RDFQueryEngine.ApplyModifiers(this, describeResultTable);

                }

            }
            RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluated SPARQL DESCRIBE query on DataSource '{0}': Found '{1}' results.", datasource, describeResult.DescribeResultsCount));

            describeResult.DescribeResults.TableName   = this.ToString();
            return describeResult;
        }
        #endregion

    }

}