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

            // PATTERN GROUPS
            Boolean printingUnion  = false;
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

            // MODIFIERS
            if (this.Modifiers.Any(mod => mod is RDFLimitModifier || mod is RDFOffsetModifier)) {
                this.Modifiers.FindAll(mod => mod is RDFLimitModifier).ForEach(lim  => query.Append("\n" + lim));
                this.Modifiers.FindAll(mod => mod is RDFOffsetModifier).ForEach(off => query.Append("\n" + off));
            }

            return query.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the templates of the query
        /// </summary>
        public RDFConstructQuery AddTemplate(RDFPattern template) {
            if (template != null) {
                if (!this.Templates.Exists(tp => tp.Equals(template))) {
                     this.Templates.Add(template);
                     
                     //Context
                     if (template.Context != null && template.Context is RDFVariable) {
                         if (!this.Variables.Exists(v => v.Equals(template.Context))) {
                              this.Variables.Add((RDFVariable)template.Context);
                         }
                     }
                     
                     //Subject
                     if (template.Subject is RDFVariable) {
                         if (!this.Variables.Exists(v => v.Equals(template.Subject))) {
                              this.Variables.Add((RDFVariable)template.Subject);
                         }
                     }
                     
                     //Predicate
                     if (template.Predicate is RDFVariable) {
                         if (!this.Variables.Exists(v => v.Equals(template.Predicate))) {
                              this.Variables.Add((RDFVariable)template.Predicate);
                         }
                     }
                     
                     //Object
                     if (template.Object is RDFVariable) {
                         if (!this.Variables.Exists(v => v.Equals(template.Object))) {
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
                if (!this.PatternGroups.Exists(pg => pg.PatternGroupName.Equals(patternGroup.PatternGroupName, StringComparison.Ordinal))) {
                     this.PatternGroups.Add(patternGroup);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFConstructQuery AddModifier(RDFLimitModifier modifier) {
            if (modifier != null) {
                if (!this.Modifiers.Any(m => m is RDFLimitModifier)) {
                     this.Modifiers.Add(modifier);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFConstructQuery AddModifier(RDFOffsetModifier modifier) {
            if (modifier != null) {
                if (!this.Modifiers.Any(m => m is RDFOffsetModifier)) {
                     this.Modifiers.Add(modifier);
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
            throw new RDFQueryException("Cannot execute CONSTRUCT query because given \"graph\" parameter is null.");
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFConstructQueryResult ApplyToStore(RDFStore store) {
            if (store != null) {
                return this.ApplyToDataSource(store);
            }
            throw new RDFQueryException("Cannot execute CONSTRUCT query because given \"store\" parameter is null.");
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFConstructQueryResult ApplyToFederation(RDFFederation federation) {
            if (federation != null) {
                return this.ApplyToDataSource(federation);
            }
            throw new RDFQueryException("Cannot execute CONSTRUCT query because given \"federation\" parameter is null.");
        }
        
        /// <summary>
        /// Applies the query to the given datasource
        /// </summary>
        internal RDFConstructQueryResult ApplyToDataSource(RDFDataSource datasource) {
            this.PatternGroupResultTables.Clear();
            this.PatternResultTables.Clear();

            RDFConstructQueryResult constructResult = new RDFConstructQueryResult(this.ToString());
            if (this.PatternGroups.Any()) {

                //Iterate the pattern groups of the query
                var fedPatternResultTables          = new Dictionary<RDFPatternGroup, List<DataTable>>();
                foreach (var patternGroup          in this.PatternGroups) {

                    //Step 1: Get the intermediate result tables of the current pattern group
                    if (datasource.IsFederation()) {

                        #region TrueFederations
                        foreach (var store         in (RDFFederation)datasource) {

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
                DataTable queryResultTable          = RDFQueryEngine.CombineTables(this.PatternGroupResultTables.Values.ToList(), false);

                //Step 5: Fill the templates from the result table
                DataTable filledResultTable         = RDFQueryEngine.FillTemplates(this, queryResultTable);

                //Step 6: Apply the modifiers of the query to the result table
                constructResult.ConstructResults    = RDFQueryEngine.ApplyModifiers(this, filledResultTable);

            }

            return constructResult;
        }
        #endregion

    }

}