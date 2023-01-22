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
            DeleteTemplates = new List<RDFPattern>();
            InsertTemplates = new List<RDFPattern>();
            Variables = new List<RDFVariable>();
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
        internal T AddDeleteGroundTemplate<T>(RDFPattern template) where T : RDFOperation
        {
            if (template == null)
                throw new RDFQueryException($"Cannot add DELETE template to operation because it is null.");
            if (template.Variables.Count > 0)
                throw new RDFQueryException($"Cannot add DELETE template '{template}' to operation because it is not ground: please ensure it does not contain variables.");

            if (!DeleteTemplates.Any(tp => tp.Equals(template)))
                DeleteTemplates.Add(template);

            return (T)this;
        }

        /// <summary>
        /// Adds the given pattern to the DELETE templates of the operation
        /// </summary>
        internal T AddDeleteNonGroundTemplate<T>(RDFPattern template) where T : RDFOperation
        {
            if (template == null)
                throw new RDFQueryException($"Cannot add DELETE template to operation because it is null.");

            if (!DeleteTemplates.Any(tp => tp.Equals(template)))
            {
                DeleteTemplates.Add(template);
                CollectVariables(template);
            }

            return (T)this;
        }

        /// <summary>
        /// Adds the given ground pattern to the INSERT templates of the operation
        /// </summary>
        internal T AddInsertGroundTemplate<T>(RDFPattern template) where T : RDFOperation
        {
            if (template == null)
                throw new RDFQueryException($"Cannot add INSERT template to operation because it is null.");
            if (template.Variables.Count > 0)
                throw new RDFQueryException($"Cannot add INSERT template '{template}' to operation because it is not ground: please ensure it does not contain variables.");

            if (!InsertTemplates.Any(tp => tp.Equals(template)))
                InsertTemplates.Add(template);

            return (T)this;
        }

        /// <summary>
        /// Adds the given pattern to the INSERT templates of the operation
        /// </summary>
        internal T AddInsertNonGroundTemplate<T>(RDFPattern template) where T : RDFOperation
        {
            if (template == null)
                throw new RDFQueryException($"Cannot add INSERT template to operation because it is null.");

            if (!InsertTemplates.Any(tp => tp.Equals(template)))
            {
                InsertTemplates.Add(template);
                CollectVariables(template);
            }

            return (T)this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        internal new T AddPrefix<T>(RDFNamespace prefix) where T : RDFOperation
        {
            if (prefix == null)
                throw new RDFQueryException($"Cannot add prefix to operation because it is null.");

            if (!Prefixes.Any(p => p.Equals(prefix)))
                Prefixes.Add(prefix);

            return (T)this;
        }

        /// <summary>
        /// Adds the given pattern group to the body of the operation
        /// </summary>
        internal new T AddPatternGroup<T>(RDFPatternGroup patternGroup) where T : RDFOperation
        {
            if (patternGroup == null)
                throw new RDFQueryException($"Cannot add pattern group to operation because it is null.");

            if (!GetPatternGroups().Any(q => q.Equals(patternGroup)))
                QueryMembers.Add(patternGroup);

            return (T)this;
        }

        /// <summary>
        /// Adds the given modifier to the operation
        /// </summary>
        internal new T AddModifier<T>(RDFDistinctModifier modifier) where T : RDFOperation
        {
            if (modifier == null)
                throw new RDFQueryException($"Cannot add modifier to operation because it is null.");

            if (!GetModifiers().Any(m => m is RDFDistinctModifier))
                QueryMembers.Add(modifier);

            return (T)this;
        }

        /// <summary>
        /// Adds the given subquery to the operation
        /// </summary>
        internal new T AddSubQuery<T>(RDFSelectQuery subQuery) where T : RDFOperation
        {
            if (subQuery == null)
                throw new RDFQueryException($"Cannot add sub query to operation because it is null.");

            if (!GetSubQueries().Any(q => q.Equals(subQuery)))
                QueryMembers.Add(subQuery.SubQuery());

            return (T)this;
        }

        /// <summary>
        /// Collects the variables contained in the given non-ground template
        /// </summary>
        internal void CollectVariables(RDFPattern template)
        {
            //Context
            if (template.Context != null && template.Context is RDFVariable)
            {
                if (!Variables.Any(v => v.Equals(template.Context)))
                    Variables.Add((RDFVariable)template.Context);
            }

            //Subject
            if (template.Subject is RDFVariable)
            {
                if (!Variables.Any(v => v.Equals(template.Subject)))
                    Variables.Add((RDFVariable)template.Subject);
            }

            //Predicate
            if (template.Predicate is RDFVariable)
            {
                if (!Variables.Any(v => v.Equals(template.Predicate)))
                    Variables.Add((RDFVariable)template.Predicate);
            }

            //Object
            if (template.Object is RDFVariable)
            {
                if (!Variables.Any(v => v.Equals(template.Object)))
                    Variables.Add((RDFVariable)template.Object);
            }
        }
        #endregion
    }
}