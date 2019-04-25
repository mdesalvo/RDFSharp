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
    /// RDFDescribeQuery is the SPARQL "DESCRIBE" query implementation.
    /// </summary>
    public class RDFDescribeQuery : RDFQuery
    {

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
        public RDFDescribeQuery()
        {
            this.DescribeTerms = new List<RDFPatternMember>();
            this.Variables = new List<RDFVariable>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the DESCRIBE query
        /// </summary>
        public override String ToString()
        {
            return RDFQueryPrinter.PrintDescribeQuery(this);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given resource to the describe terms of the query
        /// </summary>
        public RDFDescribeQuery AddDescribeTerm(RDFResource describeTerm)
        {
            if (describeTerm != null)
            {
                if (!this.DescribeTerms.Any(dt => dt.Equals(describeTerm)))
                {
                    this.DescribeTerms.Add(describeTerm);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given variable to the describe terms of the query
        /// </summary>
        public RDFDescribeQuery AddDescribeTerm(RDFVariable describeVar)
        {
            if (describeVar != null)
            {
                if (!this.DescribeTerms.Any(dt => dt.Equals(describeVar)))
                {
                    this.DescribeTerms.Add(describeVar);
                    //Variable
                    if (!this.Variables.Any(v => v.Equals(describeVar)))
                    {
                        this.Variables.Add(describeVar);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given pattern group to the query
        /// </summary>
        public RDFDescribeQuery AddPatternGroup(RDFPatternGroup patternGroup)
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
        public RDFDescribeQuery AddModifier(RDFLimitModifier modifier)
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
        public RDFDescribeQuery AddModifier(RDFOffsetModifier modifier)
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
        public RDFDescribeQuery AddPrefix(RDFNamespace prefix)
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
        public RDFDescribeQuery AddSubQuery(RDFSelectQuery subQuery)
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
        public RDFDescribeQueryResult ApplyToGraph(RDFGraph graph)
        {
            if (graph != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateDescribeQuery(this, graph);
            }
            else
            {
                return new RDFDescribeQueryResult(this.ToString());
            }
        }

        /// <summary>
        /// Applies the query to the given store 
        /// </summary>
        public RDFDescribeQueryResult ApplyToStore(RDFStore store)
        {
            if (store != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateDescribeQuery(this, store);
            }
            else
            {
                return new RDFDescribeQueryResult(this.ToString());
            }
        }

        /// <summary>
        /// Applies the query to the given federation
        /// </summary>
        public RDFDescribeQueryResult ApplyToFederation(RDFFederation federation)
        {
            if (federation != null)
            {
                return RDFQueryEngine.CreateNew().EvaluateDescribeQuery(this, federation);
            }
            else
            {
                return new RDFDescribeQueryResult(this.ToString());
            }
        }

        /// <summary>
        /// Applies the query to the given SPARQL endpoint
        /// </summary>
        public RDFDescribeQueryResult ApplyToSPARQLEndpoint(RDFSPARQLEndpoint sparqlEndpoint)
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult(this.ToString());
            if (sparqlEndpoint != null)
            {
                RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluating DESCRIBE query on SPARQL endpoint '{0}'...", sparqlEndpoint));

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
                            describeResult = RDFDescribeQueryResult.FromRDFGraph(RDFGraph.FromStream(RDFModelEnums.RDFFormats.Turtle, sStream));
                        }
                        describeResult.DescribeResults.TableName = this.ToString();
                    }

                }

                RDFQueryEvents.RaiseDESCRIBEQueryEvaluation(String.Format("Evaluated DESCRIBE query on SPARQL endpoint '{0}': Found '{1}' results.", sparqlEndpoint, describeResult.DescribeResultsCount));
            }
            return describeResult;
        }
        #endregion

    }

}