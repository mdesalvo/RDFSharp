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
    /// RDFConstructQuery is the SPARQL "CONSTRUCT" query implementation.
    /// </summary>
    public class RDFConstructQuery : RDFQuery
    {

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
        public RDFConstructQuery()
        {
            this.Templates = new List<RDFPattern>();
            this.Variables = new List<RDFVariable>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the CONSTRUCT query
        /// </summary>
        public override String ToString()
        {
            return RDFQueryPrinter.PrintConstructQuery(this);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the templates of the query
        /// </summary>
        public RDFConstructQuery AddTemplate(RDFPattern template)
        {
            if (template != null)
            {
                if (!this.Templates.Any(tp => tp.Equals(template)))
                {
                    this.Templates.Add(template);

                    //Context
                    if (template.Context != null && template.Context is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(template.Context)))
                        {
                            this.Variables.Add((RDFVariable)template.Context);
                        }
                    }

                    //Subject
                    if (template.Subject is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(template.Subject)))
                        {
                            this.Variables.Add((RDFVariable)template.Subject);
                        }
                    }

                    //Predicate
                    if (template.Predicate is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(template.Predicate)))
                        {
                            this.Variables.Add((RDFVariable)template.Predicate);
                        }
                    }

                    //Object
                    if (template.Object is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(template.Object)))
                        {
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
        public RDFConstructQuery AddPatternGroup(RDFPatternGroup patternGroup)
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
        /// Adds the given modifier to the query
        /// </summary>
        public RDFConstructQuery AddModifier(RDFLimitModifier modifier)
        {
            if (modifier != null)
            {
                if (!this.GetModifiers().Any(m => m is RDFLimitModifier))
                {
                    this.QueryMembers.Add(modifier);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        public RDFConstructQuery AddModifier(RDFOffsetModifier modifier)
        {
            if (modifier != null)
            {
                if (!this.GetModifiers().Any(m => m is RDFOffsetModifier))
                {
                    this.QueryMembers.Add(modifier);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the query
        /// </summary>
        public RDFConstructQuery AddPrefix(RDFNamespace prefix)
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
        public RDFConstructQuery AddSubQuery(RDFSelectQuery subQuery)
        {
            if (subQuery != null)
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
        public RDFConstructQueryResult ApplyToGraph(RDFGraph graph)
        {
            if (graph != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateConstructQuery(this, graph);
            }
            else
            {
                return new RDFConstructQueryResult(this.ToString());
            }
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFConstructQueryResult ApplyToStore(RDFStore store)
        {
            if (store != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateConstructQuery(this, store);
            }
            else
            {
                return new RDFConstructQueryResult(this.ToString());
            }
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFConstructQueryResult ApplyToFederation(RDFFederation federation)
        {
            if (federation != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateConstructQuery(this, federation);
            }
            else
            {
                return new RDFConstructQueryResult(this.ToString());
            }
        }

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFConstructQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
        {
            RDFConstructQueryResult constructResult = new RDFConstructQueryResult(this.ToString());
            if (sparqlEndpoint != null)
            {
                RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluating CONSTRUCT query on SPARQL endpoint '{0}'...", sparqlEndpoint));

                //Establish a connection to the given SPARQL endpoint
                using (WebClient webClient = new WebClient())
                {

                    //Insert reserved "query" parameter
                    webClient.QueryString.Add("query", HttpUtility.UrlEncode(this.ToString()));

                    //Insert user-provided parameters
                    webClient.QueryString.Add(sparqlEndpoint.QueryParams);

                    //Insert request headers
                    webClient.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
                    webClient.Headers.Add(HttpRequestHeader.Accept, "text/turtle");

                    //Send querystring to SPARQL endpoint
                    var sparqlResponse = webClient.DownloadData(sparqlEndpoint.BaseAddress);

                    //Parse response from SPARQL endpoint
                    if (sparqlResponse != null)
                    {
                        using (var sStream = new MemoryStream(sparqlResponse))
                        {
                            constructResult = RDFConstructQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                        }
                        constructResult.ConstructResults.TableName = this.ToString();
                    }

                }

                RDFQueryEvents.RaiseCONSTRUCTQueryEvaluation(String.Format("Evaluated CONSTRUCT query on SPARQL endpoint '{0}': Found '{1}' results.", sparqlEndpoint, constructResult.ConstructResultsCount));
            }
            return constructResult;
        }
        #endregion

    }

}