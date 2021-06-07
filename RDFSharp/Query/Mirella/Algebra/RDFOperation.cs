/*
   Copyright 2012-2020 Marco De Salvo

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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOperation is the foundation class for modeling SPARQL UPDATE operations
    /// </summary>
    public class RDFOperation : RDFQuery
    {
        #region Properties
        /// <summary>
        /// Templates for SPARQL DELETE operation
        /// </summary>
        internal List<RDFPattern> DeleteTemplates { get; set; }

        /// <summary>
        /// Templates for SPARQL INSERT operation
        /// </summary>
        internal List<RDFPattern> InsertTemplates { get; set; }

        /// <summary>
        /// List of variables carried by the templates of the operation
        /// </summary>
        internal List<RDFVariable> Variables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty operation
        /// </summary>
        internal RDFOperation()
        {
            this.DeleteTemplates = new List<RDFPattern>();
            this.InsertTemplates = new List<RDFPattern>();
            this.Variables = new List<RDFVariable>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the operation to the given graph
        /// </summary>
        public RDFOperationResult ApplyToGraph(RDFGraph graph)
            => graph != null ? new RDFOperationEngine().EvaluateOperationOnGraphOrStore(this, graph)
                             : new RDFOperationResult();

        /// <summary>
        /// Asynchronously applies the operation to the given graph
        /// </summary>
        public Task<RDFOperationResult> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => ApplyToGraph(graph));

        /// <summary>
        /// Applies the operation to the given store
        /// </summary>
        public RDFOperationResult ApplyToStore(RDFStore store)
            => store != null ? new RDFOperationEngine().EvaluateOperationOnGraphOrStore(this, store)
                             : new RDFOperationResult();

        /// <summary>
        /// Asynchronously applies the operation to the given store
        /// </summary>
        public Task<RDFOperationResult> ApplyToStoreAsync(RDFStore store)
            => Task.Run(() => ApplyToStore(store));

        /// <summary>
        /// Applies the operation to the given SPARQL UPDATE endpoint
        /// </summary>
        public bool ApplyToSPARQLUpdateEndpoint(RDFSPARQLEndpoint sparqlUpdateEndpoint)
            => ApplyToSPARQLUpdateEndpoint(sparqlUpdateEndpoint, new RDFSPARQLEndpointOperationOptions());

        /// <summary>
        /// Applies the operation to the given SPARQL UPDATE endpoint
        /// </summary>
        public bool ApplyToSPARQLUpdateEndpoint(RDFSPARQLEndpoint sparqlUpdateEndpoint, RDFSPARQLEndpointOperationOptions sparqlUpdateEndpointOperationOptions)
            => sparqlUpdateEndpoint != null ? new RDFOperationEngine().EvaluateOperationOnSPARQLUpdateEndpoint(this, sparqlUpdateEndpoint, sparqlUpdateEndpointOperationOptions)
                                            : false;

        /// <summary>
        /// Asynchronously applies the operation to the given SPARQL UPDATE endpoint
        /// </summary>
        public Task<bool> ApplyToSPARQLUpdateEndpointAsync(RDFSPARQLEndpoint sparqlUpdateEndpoint)
            => ApplyToSPARQLUpdateEndpointAsync(sparqlUpdateEndpoint, new RDFSPARQLEndpointOperationOptions());

        /// <summary>
        /// Asynchronously applies the operation to the given SPARQL UPDATE endpoint
        /// </summary>
        public Task<bool> ApplyToSPARQLUpdateEndpointAsync(RDFSPARQLEndpoint sparqlUpdateEndpoint, RDFSPARQLEndpointOperationOptions sparqlUpdateEndpointOperationOptions)
            => Task.Run(() => ApplyToSPARQLUpdateEndpoint(sparqlUpdateEndpoint, sparqlUpdateEndpointOperationOptions));
        #endregion

        #region Utilities
        /// <summary>
        /// Adds the given ground pattern to the DELETE templates of the operation
        /// </summary>
        protected RDFOperation AddDeleteGroundTemplate(RDFPattern template)
        {
            if (template?.Variables.Count == 0)
            {
                if (!this.DeleteTemplates.Any(tp => tp.Equals(template)))
                    this.DeleteTemplates.Add(template);
                return this;
            }
            throw new RDFQueryException("Cannot add DELETE template to DATA operation because it is not ground. Please ensure it doesn't contain any variables.");
        }

        /// <summary>
        /// Adds the given pattern to the DELETE templates of the operation
        /// </summary>
        protected RDFOperation AddDeleteNonGroundTemplate(RDFPattern template)
        {
            if (template != null)
            {
                if (!this.DeleteTemplates.Any(tp => tp.Equals(template)))
                {
                    this.DeleteTemplates.Add(template);
                    this.CollectVariables(template);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given ground pattern to the INSERT templates of the operation
        /// </summary>
        protected RDFOperation AddInsertGroundTemplate(RDFPattern template)
        {
            if (template?.Variables.Count == 0)
            {
                if (!this.InsertTemplates.Any(tp => tp.Equals(template)))
                    this.InsertTemplates.Add(template);
                return this;
            }
            throw new RDFQueryException("Cannot add INSERT template to DATA operation because it is not ground. Please ensure it doesn't contain any variables.");
        }

        /// <summary>
        /// Adds the given pattern to the INSERT templates of the operation
        /// </summary>
        protected RDFOperation AddInsertNonGroundTemplate(RDFPattern template)
        {
            if (template != null)
            {
                if (!this.InsertTemplates.Any(tp => tp.Equals(template)))
                {
                    this.InsertTemplates.Add(template);
                    this.CollectVariables(template);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        protected RDFOperation AddPrefix(RDFNamespace prefix)
        {
            if (prefix != null)
            {
                if (!this.Prefixes.Any(p => p.Equals(prefix)))
                    this.Prefixes.Add(prefix);
            }
            return this;
        }

        /// <summary>
        /// Adds the given pattern group to the body of the operation
        /// </summary>
        protected RDFOperation AddPatternGroup(RDFPatternGroup patternGroup)
        {
            if (patternGroup != null)
            {
                if (!this.GetPatternGroups().Any(q => q.Equals(patternGroup)))
                    this.QueryMembers.Add(patternGroup);
            }
            return this;
        }

        /// <summary>
        /// Adds the given modifier to the operation
        /// </summary>
        protected RDFOperation AddModifier(RDFDistinctModifier modifier)
        {
            if (modifier != null)
            {
                if (!this.GetModifiers().Any(m => m is RDFDistinctModifier))
                    this.QueryMembers.Add(modifier);
            }
            return this;
        }

        /// <summary>
        /// Adds the given subquery to the operation
        /// </summary>
        protected RDFOperation AddSubQuery(RDFSelectQuery subQuery)
        {
            if (subQuery != null)
            {
                if (!this.GetSubQueries().Any(q => q.Equals(subQuery)))
                    this.QueryMembers.Add(subQuery.SubQuery());
            }
            return this;
        }

        /// <summary>
        /// Collects the variables contained in the given non-ground template
        /// </summary>
        private void CollectVariables(RDFPattern template)
        {
            //Context
            if (template.Context != null && template.Context is RDFVariable)
            {
                if (!this.Variables.Any(v => v.Equals(template.Context)))
                    this.Variables.Add((RDFVariable)template.Context);
            }

            //Subject
            if (template.Subject is RDFVariable)
            {
                if (!this.Variables.Any(v => v.Equals(template.Subject)))
                    this.Variables.Add((RDFVariable)template.Subject);
            }

            //Predicate
            if (template.Predicate is RDFVariable)
            {
                if (!this.Variables.Any(v => v.Equals(template.Predicate)))
                    this.Variables.Add((RDFVariable)template.Predicate);
            }

            //Object
            if (template.Object is RDFVariable)
            {
                if (!this.Variables.Any(v => v.Equals(template.Object)))
                    this.Variables.Add((RDFVariable)template.Object);
            }
        }
        #endregion
    }
}