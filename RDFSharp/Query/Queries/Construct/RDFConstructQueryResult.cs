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
using System.Collections;
using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Query {

    /// <summary>
    /// RDFConstructQueryResult is a container for SPARQL "CONSTRUCT" query results.
    /// </summary>
    public class RDFConstructQueryResult {

        #region Properties
        /// <summary>
        /// Tabular response of the query
        /// </summary>
        public DataTable ConstructResults { get; internal set; }

        /// <summary>
        /// Gets the number of results produced by the query
        /// </summary>
        public Int64 ConstructResultsCount { 
            get {
                return this.ConstructResults.Rows.Count;
            }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty CONSTRUCT result
        /// </summary>
        internal RDFConstructQueryResult(String tableName) {
            this.ConstructResults = new DataTable(tableName);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Builds a graph corresponding to the query result
        /// </summary>
        public RDFGraph ToRDFGraph() {
            RDFGraph result        = new RDFGraph();
            RDFPatternMember subj  = null;
            RDFPatternMember pred  = null;
            RDFPatternMember obj   = null;
            
            //Iterate the datatable rows and generate the corresponding triples to be added to the result graph
            IEnumerator resultRows = this.ConstructResults.Rows.GetEnumerator();
            while (resultRows.MoveNext()) {
                subj               = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["SUBJECT"].ToString());
                pred               = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["PREDICATE"].ToString());
                obj                = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["OBJECT"].ToString());
                if (obj is RDFResource) {
                    result.AddTriple(new RDFTriple((RDFResource)subj, (RDFResource)pred, (RDFResource)obj));
                }
                else {
                    result.AddTriple(new RDFTriple((RDFResource)subj, (RDFResource)pred, (RDFLiteral)obj));
                }                
            }

            return result;
        }

        /// <summary>
        /// Builds a query result corresponding to the given graph
        /// </summary>
        public static RDFConstructQueryResult FromRDFGraph(RDFGraph graph) {
            RDFConstructQueryResult result = new RDFConstructQueryResult(String.Empty);
            if (graph != null) {

                //Transform the graph into a datatable and assign it to the query result
                result.ConstructResults    = graph.ToDataTable();

            }
            return result;
        }
        #endregion

    }

}