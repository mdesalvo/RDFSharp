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

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTermResolver is the neutral abstraction that supplies, to the term-level parsers of RDFTurtle,
    /// the only two pieces of contextual information they actually need in order to materialize an RDF term:
    /// a base IRI (to resolve relative IRI references) and a prefix-to-namespace resolution function.
    ///
    /// Decoupling these two responsibilities away from RDFGraph lets the very same term-level parsers
    /// (ParseURI, ParseQNameOrBoolean, ParseValue, ParseQuotedLiteral, ...) be reused verbatim by the
    /// SPARQL parser, which resolves base/prefixes from a query-local context instead of a graph.
    /// </summary>
    internal abstract class RDFTermResolver
    {
        /// <summary>
        /// The base IRI used to resolve relative IRI references and bare-fragment IRIs (e.g. "#").
        /// </summary>
        internal abstract string BaseUri { get; }

        /// <summary>
        /// Resolves a prefix label to its namespace IRI. An empty (or null) prefix label denotes the
        /// default namespace (the ":" prefix). Returns null when the prefix cannot be resolved.
        /// </summary>
        internal abstract string ResolveNamespace(string prefixLabel);
    }

    /// <summary>
    /// RDFGraphTermResolver is the graph-backed implementation of RDFTermResolver used while deserializing
    /// Turtle/TriG data into an RDFGraph. It reads from the wrapped graph LIVE (at call time): this is
    /// required because Turtle "@base"/"@prefix" directives mutate the graph context and the global
    /// namespace register WHILE the document is being parsed, and subsequent relative IRIs and prefixed
    /// names must resolve against the most recent state.
    /// </summary>
    internal sealed class RDFGraphTermResolver : RDFTermResolver
    {
        #region Properties
        /// <summary>
        /// The graph being populated during deserialization, queried live for base/default-namespace.
        /// </summary>
        private readonly RDFGraph graph;
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a graph-backed term resolver wrapping the given graph.
        /// </summary>
        internal RDFGraphTermResolver(RDFGraph graph)
            => this.graph = graph;
        #endregion

        #region Methods
        /// <summary>
        /// The base IRI is the graph's current context.
        /// </summary>
        internal override string BaseUri
            => graph.ToString();

        /// <summary>
        /// Resolves a prefix label: an empty prefix maps to the graph's default context, while a non-empty prefix
        /// is looked up in the global namespace register (returning null when not registered).
        /// </summary>
        internal override string ResolveNamespace(string prefixLabel)
            => string.IsNullOrEmpty(prefixLabel)
                ? graph.Context.ToString()
                : RDFNamespaceRegister.GetByPrefix(prefixLabel)?.ToString();
        #endregion
    }
}