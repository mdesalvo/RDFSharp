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
    }
}