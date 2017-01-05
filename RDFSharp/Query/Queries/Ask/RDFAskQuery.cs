/*
   Copyright 2012-2017 Marco De Salvo

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
using System.Linq;
using System.Text;
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

            // PATTERN GROUPS
            Boolean printingUnion         = false;
            this.PatternGroups.ForEach(pg => {

                //Current pattern group is set as UNION with the next one
                if (pg.JoinAsUnion) {

                    //Current pattern group IS NOT the last of the query (so UNION keyword must be appended at last)
                    if (!pg.Equals(this.PatternGroups.Last())) {
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

            });

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
                if (!this.PatternGroups.Exists(pg => pg.PatternGroupName.Equals(patternGroup.PatternGroupName, StringComparison.Ordinal))) {
                     this.PatternGroups.Add(patternGroup);
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
            throw new RDFQueryException("Cannot execute ASK query because given \"graph\" parameter is null.");
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFAskQueryResult ApplyToStore(RDFStore store) {
            if (store != null) {
                return this.ApplyToDataSource(store);
            }
            throw new RDFQueryException("Cannot execute ASK query because given \"store\" parameter is null.");
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFAskQueryResult ApplyToFederation(RDFFederation federation) {
            if (federation != null) {
                return this.ApplyToDataSource(federation);
            }
            throw new RDFQueryException("Cannot execute ASK query because given \"federation\" parameter is null.");
        }
        
        /// <summary>
        /// Applies the query to the given datasource
        /// </summary>
        internal RDFAskQueryResult ApplyToDataSource(RDFDataSource datasource) {
            this.PatternGroupResultTables.Clear();
            this.PatternResultTables.Clear();

            RDFAskQueryResult askResult    = new RDFAskQueryResult();
            if (this.PatternGroups.Any())  {

                //Iterate the pattern groups of the query
                var fedPatternResultTables = new Dictionary<RDFPatternGroup, List<DataTable>>();
                foreach (var patternGroup in this.PatternGroups) {

                    //Step 1: Get the intermediate result tables of the current pattern group
                    if (datasource.IsFederation()) {

                        #region TrueFederations
                        foreach(var store in (RDFFederation)datasource) {

                            //Step FED.1: Evaluate the patterns of the current pattern group on the current store
                            RDFQueryEngine.EvaluatePatterns(this, patternGroup, store);

                            //Step FED.2: Federate the patterns of the current pattern group on the current store
                            if (!fedPatternResultTables.ContainsKey(patternGroup)) {
                                 fedPatternResultTables.Add(patternGroup, this.PatternResultTables[patternGroup]);
                            }
                            else {
                                 fedPatternResultTables[patternGroup].ForEach(fprt =>
                                    fprt.Merge(this.PatternResultTables[patternGroup].Single(prt => prt.TableName.Equals(fprt.TableName, StringComparison.Ordinal)), true, MissingSchemaAction.Add));
                            }

                        }
                        this.PatternResultTables[patternGroup] = fedPatternResultTables[patternGroup];
                        #endregion

                    }
                    else {
                        RDFQueryEngine.EvaluatePatterns(this, patternGroup, datasource);
                    }

                    //Step 2: Get the result table of the current pattern group
                    RDFQueryEngine.CombinePatterns(this, patternGroup);

                    //Step 3: Apply the filters of the current pattern group to its result table
                    RDFQueryEngine.ApplyFilters(this, patternGroup);

                }

                //Step 4: Get the result table of the query
                var queryResultTable = RDFQueryEngine.CombineTables(this.PatternGroupResultTables.Values.ToList(), false);

                //Step 5: Transform the result into a boolean response 
                askResult.AskResult  = (queryResultTable.Rows.Count > 0);

            }

            return askResult;
        }
        #endregion

    }

}