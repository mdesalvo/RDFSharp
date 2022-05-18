/*
   Copyright 2012-2022 Marco De Salvo

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

using RDFSharp.Model;
using RDFSharp.Store;
using System.Collections;
using System.Data;
using System.Threading.Tasks;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFConstructQueryResult is a container for SPARQL "CONSTRUCT" query results.
    /// </summary>
    public class RDFConstructQueryResult
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
            => this.ConstructResults.Rows.Count;
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty CONSTRUCT result
        /// </summary>
        internal RDFConstructQueryResult()
            => this.ConstructResults = new DataTable();
        #endregion

        #region Methods
        /// <summary>
        /// Gets a graph corresponding to the query result
        /// </summary>
        public RDFGraph ToRDFGraph()
        {
            RDFGraph result = new RDFGraph();
            RDFPatternMember subj = null;
            RDFPatternMember pred = null;
            RDFPatternMember obj = null;

            //Iterate the datatable rows and generate the corresponding triples to be added to the result graph
            IEnumerator resultRows = this.ConstructResults.Rows.GetEnumerator();
            while (resultRows.MoveNext())
            {
                subj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["?SUBJECT"].ToString());
                pred = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["?PREDICATE"].ToString());
                obj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["?OBJECT"].ToString());
                if (obj is RDFResource)
                    result.AddTriple(new RDFTriple((RDFResource)subj, (RDFResource)pred, (RDFResource)obj));
                else
                    result.AddTriple(new RDFTriple((RDFResource)subj, (RDFResource)pred, (RDFLiteral)obj));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously gets a graph corresponding to the query result
        /// </summary>
        public Task<RDFGraph> ToRDFGraphAsync()
            => Task.Run(() => ToRDFGraph());

        /// <summary>
        /// Gets a memory store corresponding to the query result
        /// </summary>
        public RDFMemoryStore ToRDFStore()
        {
            RDFMemoryStore result = new RDFMemoryStore();
            RDFContext defaultCtx = new RDFContext();
            RDFContext ctx = new RDFContext();
            RDFPatternMember subj = null;
            RDFPatternMember pred = null;
            RDFPatternMember obj = null;

            //Iterate the datatable rows and generate the corresponding quadruples to be added to the result store
            bool hasCtx = this.ConstructResults.Columns.Contains("?CONTEXT");
            IEnumerator resultRows = this.ConstructResults.Rows.GetEnumerator();
            while (resultRows.MoveNext())
            {
                //Context is parsed only if construct table has the "?CONTEXT" column
                //and is accepted only if it represents a non-blank resource, otherwise
                //default context is safely adopted for the candidate quadruple
                if (hasCtx)
                {
                    RDFPatternMember rowCtx = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["?CONTEXT"].ToString());
                    ctx = rowCtx is RDFResource rowCtxRes && !rowCtxRes.IsBlank ? new RDFContext(rowCtxRes.URI) : defaultCtx;
                }

                subj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["?SUBJECT"].ToString());
                pred = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["?PREDICATE"].ToString());
                obj = RDFQueryUtilities.ParseRDFPatternMember(((DataRow)resultRows.Current)["?OBJECT"].ToString());
                if (obj is RDFResource)
                    result.AddQuadruple(new RDFQuadruple(ctx, (RDFResource)subj, (RDFResource)pred, (RDFResource)obj));
                else
                    result.AddQuadruple(new RDFQuadruple(ctx, (RDFResource)subj, (RDFResource)pred, (RDFLiteral)obj));
            }

            return result;
        }

        /// <summary>
        /// Asynchronously gets a memory store corresponding to the query result
        /// </summary>
        public Task<RDFMemoryStore> ToRDFStoreAsync()
            => Task.Run(() => ToRDFStore());

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

        /// <summary>
        /// Gets a query result corresponding to the given memory store
        /// </summary>
        public static RDFConstructQueryResult FromRDFStore(RDFMemoryStore store)
        {
            RDFConstructQueryResult result = new RDFConstructQueryResult();
            if (store != null)
            {
                //Transform the store into a datatable and assign it to the query result
                result.ConstructResults = store.ToDataTable();
            }
            return result;
        }

        /// <summary>
        /// Asynchronously gets a query result corresponding to the given memory store
        /// </summary>
        public static Task<RDFConstructQueryResult> FromRDFStoreAsync(RDFMemoryStore store)
            => Task.Run(() => FromRDFStore(store));
        #endregion
    }
}