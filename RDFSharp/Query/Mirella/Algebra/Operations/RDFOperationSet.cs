/*
   Copyright 2012-2026 Marco De Salvo

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

using System.Collections.Generic;
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    /// <summary>
    /// <para>
    /// RDFOperationSet models a SPARQL 1.1 UPDATE request made of MORE than one operation, i.e. the chain of
    /// ';'-separated operations of the grammar <c>Update ::= Prologue ( Update1 ( ';' Update )? )?</c>. It is a
    /// thin, composition-based wrapper over an ORDERED list of <see cref="RDFOperation"/> instances.
    /// </para>
    /// <para>
    /// Applying a set to a graph/store runs its operations IN ORDER against the SAME mutable data source, so the
    /// effects of an earlier operation are visible to the later ones (e.g. an INSERT DATA followed by a DELETE
    /// WHERE that matches the freshly inserted triples). The returned per-operation results mirror
    /// <see cref="Operations"/> positionally (ordinal semantics: the i-th result is the outcome of the i-th
    /// operation).
    /// </para>
    /// <para>
    /// Atomicity is BEST-EFFORT and explicitly NOT transactional: there is no rollback. If an operation in the
    /// middle of the chain throws, the mutations already applied by the operations preceding it remain in the data
    /// source. Callers that need all-or-nothing semantics must snapshot/restore the data source themselves.
    /// </para>
    /// </summary>
    public sealed class RDFOperationSet
    {
        #region Properties
        /// <summary>
        /// The ordered operations composing the set; they are applied (and serialized) in this exact order.
        /// </summary>
        public List<RDFOperation> Operations { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an empty operation set
        /// </summary>
        public RDFOperationSet()
            => Operations = new List<RDFOperation>();
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the operation set (its operations joined by the ';' separator)
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintOperationSet(this);
        #endregion

        #region Methods
        /// <summary>
        /// Parses the given SPARQL UPDATE string, possibly carrying a chain of ';'-separated operations, into an
        /// RDFOperationSet preserving the source order of its operations.
        /// </summary>
        /// <exception cref="RDFQueryException">When the string is null/empty or not a syntactically valid SPARQL UPDATE chain.</exception>
        public static RDFOperationSet FromString(string operationSet)
            => RDFOperationParserFactory.ParseOperationSet(operationSet);

        /// <summary>
        /// Adds the given operation to the end of the set, preserving its ordinal position in the chain
        /// </summary>
        /// <exception cref="RDFQueryException">When the given operation is null.</exception>
        public RDFOperationSet AddOperation(RDFOperation operation)
        {
            #region Guards
            if (operation == null)
                throw new RDFQueryException("Cannot add operation to operation set because it is null.");
            #endregion

            Operations.Add(operation);
            return this;
        }

        /// <summary>
        /// Applies the operations of the set, IN ORDER, to the given graph (best-effort, no rollback). Returns the
        /// per-operation results positionally aligned with <see cref="Operations"/>.
        /// </summary>
        public IReadOnlyList<RDFOperationResult> ApplyToGraph(RDFGraph graph)
        {
            List<RDFOperationResult> operationSetResults = new List<RDFOperationResult>(Operations.Count);
            if (graph != null)
                Operations.ForEach(operation => operationSetResults.Add(operation.ApplyToGraph(graph)));
            return operationSetResults;
        }

        /// <summary>
        /// Asynchronously applies the operations of the set, IN ORDER, to the given graph (best-effort, no rollback)
        /// </summary>
        public Task<IReadOnlyList<RDFOperationResult>> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => ApplyToGraph(graph));

        /// <summary>
        /// Applies the operations of the set, IN ORDER, to the given store (best-effort, no rollback). Returns the
        /// per-operation results positionally aligned with <see cref="Operations"/>.
        /// </summary>
        public IReadOnlyList<RDFOperationResult> ApplyToStore(RDFStore store)
        {
            List<RDFOperationResult> operationSetResults = new List<RDFOperationResult>(Operations.Count);
            if (store != null)
                Operations.ForEach(operation => operationSetResults.Add(operation.ApplyToStore(store)));
            return operationSetResults;
        }

        /// <summary>
        /// Asynchronously applies the operations of the set, IN ORDER, to the given store (best-effort, no rollback)
        /// </summary>
        public Task<IReadOnlyList<RDFOperationResult>> ApplyToStoreAsync(RDFStore store)
            => Task.Run(() => ApplyToStore(store));

        /// <summary>
        /// Applies the operation set to the given SPARQL UPDATE endpoint, sending its ';'-joined operations as a
        /// SINGLE request so the endpoint executes the whole chain as one command.
        /// </summary>
        public bool ApplyToSPARQLUpdateEndpoint(RDFSPARQLEndpoint sparqlUpdateEndpoint)
            => ApplyToSPARQLUpdateEndpoint(sparqlUpdateEndpoint, new RDFSPARQLEndpointOperationOptions());

        /// <summary>
        /// Applies the operation set to the given SPARQL UPDATE endpoint, sending its ';'-joined operations as a
        /// SINGLE request so the endpoint executes the whole chain as one command.
        /// </summary>
        public bool ApplyToSPARQLUpdateEndpoint(RDFSPARQLEndpoint sparqlUpdateEndpoint, RDFSPARQLEndpointOperationOptions sparqlUpdateEndpointOperationOptions)
            => sparqlUpdateEndpoint != null && new RDFOperationEngine().EvaluateOperationSetOnSPARQLUpdateEndpoint(this, sparqlUpdateEndpoint, sparqlUpdateEndpointOperationOptions);

        /// <summary>
        /// Asynchronously applies the operation set to the given SPARQL UPDATE endpoint (single request)
        /// </summary>
        public Task<bool> ApplyToSPARQLUpdateEndpointAsync(RDFSPARQLEndpoint sparqlUpdateEndpoint)
            => ApplyToSPARQLUpdateEndpointAsync(sparqlUpdateEndpoint, new RDFSPARQLEndpointOperationOptions());

        /// <summary>
        /// Asynchronously applies the operation set to the given SPARQL UPDATE endpoint (single request)
        /// </summary>
        public Task<bool> ApplyToSPARQLUpdateEndpointAsync(RDFSPARQLEndpoint sparqlUpdateEndpoint, RDFSPARQLEndpointOperationOptions sparqlUpdateEndpointOperationOptions)
            => Task.Run(() => ApplyToSPARQLUpdateEndpoint(sparqlUpdateEndpoint, sparqlUpdateEndpointOperationOptions));
        #endregion
    }
}