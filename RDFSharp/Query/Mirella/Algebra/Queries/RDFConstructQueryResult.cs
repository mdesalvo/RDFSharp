/*
   Copyright 2012-2025 Marco De Salvo

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

using System.Data;
using System.Threading.Tasks;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFConstructQueryResult is a container for SPARQL "CONSTRUCT" query results.
    /// </summary>
    public sealed class RDFConstructQueryResult : RDFQueryResult
    {
        #region Properties
        /// <summary>
        /// Tabular response of the query
        /// </summary>
        public DataTable ConstructResults { get; internal set; }

        /// <summary>
        /// Gets the number of results produced by the query
        /// </summary>
        public long ConstructResultsCount
            => ConstructResults.Rows.Count;
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an empty CONSTRUCT result
        /// </summary>
        internal RDFConstructQueryResult()
            => ConstructResults = new DataTable();
        #endregion

        #region Methods
        /// <summary>
        /// Gets a graph corresponding to the query result
        /// </summary>
        public RDFGraph ToRDFGraph()
        {
            RDFGraph result = new RDFGraph();

            //Iterate the datatable rows and generate the corresponding triples to be added to the result graph
            foreach (DataRow resultRow in ConstructResults.Rows)
            {
                RDFPatternMember subj = RDFQueryUtilities.ParseRDFPatternMember(resultRow["?SUBJECT"].ToString());
                RDFPatternMember pred = RDFQueryUtilities.ParseRDFPatternMember(resultRow["?PREDICATE"].ToString());
                RDFPatternMember obj = RDFQueryUtilities.ParseRDFPatternMember(resultRow["?OBJECT"].ToString());
                if (obj is RDFResource objRes)
                    result.AddTriple(new RDFTriple((RDFResource)subj, (RDFResource)pred, objRes));
                else
                    result.AddTriple(new RDFTriple((RDFResource)subj, (RDFResource)pred, (RDFLiteral)obj));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously gets a graph corresponding to the query result
        /// </summary>
        public Task<RDFGraph> ToRDFGraphAsync()
            => Task.Run(ToRDFGraph);

        /// <summary>
        /// Gets a query result corresponding to the given graph
        /// </summary>
        public static RDFConstructQueryResult FromRDFGraph(RDFGraph graph)
        {
            RDFConstructQueryResult result = new RDFConstructQueryResult();
            if (graph != null)
            {
                //Transform the graph into a datatable and assign it to the query result
                result.ConstructResults = graph.ToDataTable();
            }
            return result;
        }

        /// <summary>
        /// Asynchronously gets a query result corresponding to the given graph
        /// </summary>
        public static Task<RDFConstructQueryResult> FromRDFGraphAsync(RDFGraph graph)
            => Task.Run(() => FromRDFGraph(graph));
        #endregion
    }
}