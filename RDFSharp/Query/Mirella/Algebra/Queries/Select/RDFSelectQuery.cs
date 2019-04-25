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

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFSelectQuery is the SPARQL "SELECT" query implementation.
    /// </summary>
    public class RDFSelectQuery : RDFQuery
    {

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
        public RDFSelectQuery()
        {
            this.ProjectionVars = new Dictionary<RDFVariable, Int32>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the SELECT query
        /// </summary>
        public override String ToString()
        {
            return RDFQueryPrinter.PrintSelectQuery(this, 0, false);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFSelectQuery AddPatternGroup(RDFPatternGroup patternGroup)
        {
            if (patternGroup != null)
            {
                if (!this.GetPatternGroups().Any(q => q.Equals(patternGroup)))
                {
                    this.QueryMembers.Add(patternGroup);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given variable to the results of the query
        /// </summary>
        public RDFSelectQuery AddProjectionVariable(RDFVariable projectionVariable)
        {
            if (projectionVariable != null)
            {
                if (!this.ProjectionVars.Any(pv => pv.Key.ToString().Equals(projectionVariable.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    this.ProjectionVars.Add(projectionVariable, this.ProjectionVars.Count);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFSelectQuery AddModifier(RDFModifier modifier)
        {
            if (modifier != null)
            {
                //Ensure to have only one groupby modifier in the query
                if (modifier is RDFGroupByModifier && this.GetModifiers().Any(m => m is RDFGroupByModifier))
                {
                    return this;
                }
                //Ensure to have only one distinct modifier in the query
                if (modifier is RDFDistinctModifier && this.GetModifiers().Any(m => m is RDFDistinctModifier))
                {
                    return this;
                }
                //Ensure to have only one limit modifier in the query
                if (modifier is RDFLimitModifier && this.GetModifiers().Any(m => m is RDFLimitModifier))
                {
                    return this;
                }
                //Ensure to have only one offset modifier in the query
                if (modifier is RDFOffsetModifier && this.GetModifiers().Any(m => m is RDFOffsetModifier))
                {
                    return this;
                }
                //Ensure to have only one orderby modifier per variable in the query
                if (modifier is RDFOrderByModifier && this.GetModifiers().Any(m => m is RDFOrderByModifier && ((RDFOrderByModifier)m).Variable.Equals(((RDFOrderByModifier)modifier).Variable)))
                {
                    return this;
                }

                //Add the modifier, avoiding duplicates
                if (!this.GetModifiers().Any(m => m.Equals(modifier)))
                {
                    this.QueryMembers.Add(modifier);
                }

            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the query
        /// </summary>
        public RDFSelectQuery AddPrefix(RDFNamespace prefix)
        {
            if (prefix != null)
            {
                if (!this.Prefixes.Any(p => p.Equals(prefix)))
                {
                    this.Prefixes.Add(prefix);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given subquery to the query
        /// </summary>
        public RDFSelectQuery AddSubQuery(RDFSelectQuery subQuery)
        {
            if (subQuery != null && !this.Equals(subQuery))
            {
                if (!this.GetSubQueries().Any(q => q.Equals(subQuery)))
                {
                    this.QueryMembers.Add(subQuery.SubQuery());
                }
            }
            return this;
        }

        /// <summary>
        /// Applies the query to the given graph 
        /// </summary>
        public RDFSelectQueryResult ApplyToGraph(RDFGraph graph)
        {
            if (graph != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateSelectQuery(this, graph);
            }
            else
            {
                return new RDFSelectQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFSelectQueryResult ApplyToStore(RDFStore store)
        {
            if (store != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateSelectQuery(this, store);
            }
            else
            {
                return new RDFSelectQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFSelectQueryResult ApplyToFederation(RDFFederation federation)
        {
            if (federation != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateSelectQuery(this, federation);
            }
            else
            {
                return new RDFSelectQueryResult();
            }
        }

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFSelectQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
        {
            RDFSelectQueryResult selResult = new RDFSelectQueryResult();
            if (sparqlEndpoint != null)
            {
                RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Evaluating SELECT query on SPARQL endpoint '{0}'...", sparqlEndpoint));

                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient())
                {

                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(this.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/sparql-results+xml");

                    //Send querystring to SPARQL endpoint
                    var sparqlResponse = webClient.DownloadData(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (var sStream = new MemoryStream(sparqlResponse))
                        {
                            selResult = RDFSelectQueryResult.FromSparqlXmlResult(sStream);
                        }
                        selResult.SelectResults.TableName = this.ToString();
                    }

                }

                RDFQueryEvents.RaiseSELECTQueryEvaluation(String.Format("Evaluated SELECT query on SPARQL endpoint '{0}': Found '{1}' results.", sparqlEndpoint, selResult.SelectResultsCount));
            }
            return selResult;
        }

        /// <summary>
        /// Sets the query as optional
        /// </summary>
        public RDFSelectQuery Optional()
        {
            this.IsOptional = true;
            return this;
        }

        /// <summary>
        /// Sets the query to be joined as union with the next query member
        /// </summary>
        public RDFSelectQuery UnionWithNext()
        {
            this.JoinAsUnion = true;
            return this;
        }
        #endregion

    }

}