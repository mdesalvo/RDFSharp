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
}