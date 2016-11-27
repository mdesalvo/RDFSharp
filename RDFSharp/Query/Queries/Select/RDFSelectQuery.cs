/*
   Copyright 2012-2016 Marco De Salvo

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

namespace RDFSharp.Query {

    /// <summary>
    /// RDFSelectQuery is the SPARQL "SELECT" query implementation.
    /// </summary>
    public class RDFSelectQuery: RDFQuery {

        #region Properties
        /// <summary>
        /// Checks if the query is a "SELECT *" query, so contains all/none projection variables
        /// </summary>
        internal Boolean IsStar {
            get { return (this.PatternGroups.TrueForAll(pg => pg.Variables.TrueForAll(v => !v.IsResult) || pg.Variables.TrueForAll(v =>  v.IsResult))); }
        }

        /// <summary>
        /// Checks if the query is empty, so contains no pattern groups
        /// </summary>
        internal override Boolean IsEmpty {
            get { return this.PatternGroups.Count == 0; }
        }

        /// <summary>
        /// Dictionary of projection variables and associated ordinals
        /// </summary>
        internal Dictionary<RDFVariable, Int32> ProjVars { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SELECT query
        /// </summary>
        public RDFSelectQuery() {
            this.ProjVars = new Dictionary<RDFVariable, Int32>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SELECT query
        /// </summary>
        public override String ToString() {
            StringBuilder query = new StringBuilder();

            // SELECT
            query.Append("SELECT");

            // DISTINCT
            this.Modifiers.FindAll(mod => mod is RDFDistinctModifier).ForEach(dm => query.Append(" " + dm));

            // VARIABLES
            if (this.IsStar) {
                query.Append(" *");
            }
            else {
                this.ProjVars.OrderBy(x => x.Value).ToList().ForEach(variable => query.Append(" " + variable.Key));
            }

            // PATTERN GROUPS
            query.Append("\nWHERE {\n");
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
                        printingUnion = false;
                        query.Append(pg.ToString(2));
                        query.Append("  }\n");
                    }
                    else {
                        query.Append(pg.ToString());
                    }
                }

            });
            query.Append("\n}");

            // ORDER BY
            if (this.Modifiers.Any(mod => mod is RDFOrderByModifier)) {
                query.Append("\nORDER BY");
                this.Modifiers.FindAll(mod => mod is RDFOrderByModifier).ForEach(om => query.Append(" " + om));
            }

            // LIMIT/OFFSET
            if (this.Modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier)) {
                this.Modifiers.FindAll(mod => mod is RDFLimitModifier).ForEach(lim  => query.Append("\n" + lim));
                this.Modifiers.FindAll(mod => mod is RDFOffsetModifier).ForEach(off => query.Append("\n" + off));
            }

            return query.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFSelectQuery AddPatternGroup(RDFPatternGroup patternGroup) {
            if (patternGroup != null) {
                if (!this.PatternGroups.Exists(pg => pg.PatternGroupName.Equals(patternGroup.PatternGroupName, StringComparison.Ordinal))) {
                     this.PatternGroups.Add(patternGroup);

                     //Also collect the projection variables of the pattern group
                     patternGroup.Variables.Where(v => v.IsResult).ToList().ForEach(v => {
                         if (!this.ProjVars.ContainsKey(v)) {
                              this.ProjVars.Add(v, this.ProjVars.Count);
                         }
                     });
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
                if (modifier is RDFDistinctModifier && this.Modifiers.Any(m => m is RDFDistinctModifier)) {
                    return this;
                }
                //Ensure to have only one limit modifier in the query
                if (modifier is RDFLimitModifier    && this.Modifiers.Any(m => m is RDFLimitModifier)) {
                    return this;
                }
                //Ensure to have only one offset modifier in the query
                if (modifier is RDFOffsetModifier   && this.Modifiers.Any(m => m is RDFOffsetModifier)) {
                    return this;
                }
                //Ensure to have only one orderby modifier per variable in the query
                if (modifier is RDFOrderByModifier  && this.Modifiers.Any(m => m is RDFOrderByModifier && ((RDFOrderByModifier)m).Variable.Equals(((RDFOrderByModifier)modifier).Variable))) {
                    return this;
                }

                //Add the modifier, avoiding duplicates
                if (!this.Modifiers.Exists(m => m.Equals(modifier))) {
                     this.Modifiers.Add(modifier);
                }

            }
            return this;
        }

        /// <summary>
        /// Applies the query to the given graph 
        /// </summary>
        public RDFSelectQueryResult ApplyToGraph(RDFGraph graph) {
            if (graph != null) {
                this.PatternGroupResultTables.Clear();
                this.PatternResultTables.Clear();

                RDFSelectQueryResult selectResult    = new RDFSelectQueryResult(this.ToString());
                if (!this.IsEmpty) {

                    //Iterate the pattern groups of the query
                    foreach (RDFPatternGroup patternGroup in this.PatternGroups) {

                        //Step 1: Get the intermediate result tables of the current pattern group
                        RDFQueryEngine.EvaluatePatterns(this, patternGroup, graph);

                        //Step 2: Get the result table of the current pattern group
                        RDFQueryEngine.CombinePatterns(this, patternGroup);

                        //Step 3: Apply the filters of the current pattern group to its result table
                        RDFQueryEngine.ApplyFilters(this, patternGroup);

                    }

                    //Step 4: Get the result table of the query
                    DataTable queryResultTable       = RDFQueryEngine.CombineTables(this.PatternGroupResultTables.Values.ToList(), false);

                    //Step 5: Apply the modifiers of the query to the result table
                    selectResult.SelectResults       = RDFQueryEngine.ApplyModifiers(this, queryResultTable);

                }

                return selectResult;
            }
            throw new RDFQueryException("Cannot execute SELECT query because given \"graph\" parameter is null.");
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFSelectQueryResult ApplyToStore(RDFStore store) {
            if (store != null) {
                this.PatternGroupResultTables.Clear();
                this.PatternResultTables.Clear();

                RDFSelectQueryResult selectResult    = new RDFSelectQueryResult(this.ToString());
                if (!this.IsEmpty) {

                    //Iterate the pattern groups of the query
                    foreach (RDFPatternGroup patternGroup in this.PatternGroups) {

                        //Step 1: Get the intermediate result tables of the current pattern group
                        RDFQueryEngine.EvaluatePatterns(this, patternGroup, store);

                        //Step 2: Get the result table of the current pattern group
                        RDFQueryEngine.CombinePatterns(this, patternGroup);

                        //Step 3: Apply the filters of the current pattern group to its result table
                        RDFQueryEngine.ApplyFilters(this, patternGroup);

                    }

                    //Step 4: Get the result table of the query
                    DataTable queryResultTable       = RDFQueryEngine.CombineTables(this.PatternGroupResultTables.Values.ToList(), false);

                    //Step 5: Apply the modifiers of the query to the result table
                    selectResult.SelectResults       = RDFQueryEngine.ApplyModifiers(this, queryResultTable);

                }

                return selectResult;
            }
            throw new RDFQueryException("Cannot execute SELECT query because given \"store\" parameter is null.");
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFSelectQueryResult ApplyToFederation(RDFFederation federation) {
            if (federation != null) {
                this.PatternGroupResultTables.Clear();
                this.PatternResultTables.Clear();

                RDFSelectQueryResult selectResult = new RDFSelectQueryResult(this.ToString());
                if (!this.IsEmpty) {

                    //Iterate the pattern groups of the query
                    var fedPatternResultTables    = new Dictionary<RDFPatternGroup, List<DataTable>>();
                    foreach (RDFPatternGroup patternGroup in this.PatternGroups) {

                        #region TrueFederations
                        foreach (RDFStore store in federation) {

                            //Step 1: Evaluate the patterns of the current pattern group on the current store
                            RDFQueryEngine.EvaluatePatterns(this, patternGroup, store);

                            //Step 2: Federate the patterns of the current pattern group on the current store
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

                        //Step 3: Get the result table of the current pattern group
                        RDFQueryEngine.CombinePatterns(this, patternGroup);

                        //Step 4: Apply the filters of the current pattern group to its result table
                        RDFQueryEngine.ApplyFilters(this, patternGroup);

                    }

                    //Step 5: Get the result table of the query
                    DataTable queryResultTable    = RDFQueryEngine.CombineTables(this.PatternGroupResultTables.Values.ToList(), false);

                    //Step 6: Apply the modifiers of the query to the result table
                    selectResult.SelectResults    = RDFQueryEngine.ApplyModifiers(this, queryResultTable);

                }

                return selectResult;
            }
            throw new RDFQueryException("Cannot execute SELECT query because given \"federation\" parameter is null.");
        }
        #endregion

    }

}