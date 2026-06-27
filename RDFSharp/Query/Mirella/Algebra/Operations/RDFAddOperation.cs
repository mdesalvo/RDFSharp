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

using System;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFAddOperation is the SPARQL "ADD" graph-management operation implementation
    /// (<c>ADD 'SILENT'? GraphOrDefault TO GraphOrDefault</c>): it inserts all the triples of the source
    /// graph into the destination graph, leaving both the source and the pre-existing destination data in
    /// place. A null context (on either side) denotes the DEFAULT graph; a non-null context denotes a
    /// named graph. Being a two-graph operation, it is meaningful only on a context-aware store.
    /// </summary>
    public sealed class RDFAddOperation : RDFOperation
    {
        #region Properties
        /// <summary>
        /// Flag indicating that the operation will hide errors from the SPARQL UPDATE endpoint
        /// </summary>
        public bool IsSilent { get; internal set; }

        /// <summary>
        /// Represents the Uri of the source graph (null denotes the DEFAULT graph)
        /// </summary>
        public Uri FromContext { get; internal set; }

        /// <summary>
        /// Represents the Uri of the destination graph (null denotes the DEFAULT graph)
        /// </summary>
        public Uri ToContext { get; internal set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the ADD operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintAddOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Parses the given SPARQL UPDATE string into an RDFAddOperation.
        /// </summary>
        /// <exception cref="RDFQueryException">When the string is not a syntactically valid ADD operation.</exception>
        public static RDFAddOperation FromString(string addOperation)
        {
            RDFOperation parsedOperation = RDFOperationParserFactory.ParseOperation(addOperation);

            //The factory dispatches on the operation form: enforce that the parsed operation is indeed an ADD
            if (parsedOperation is RDFAddOperation parsedAddOperation)
                return parsedAddOperation;

            throw new RDFQueryException("Cannot parse ADD operation because the given command represents a different SPARQL UPDATE operation (" + parsedOperation.GetType().Name + ")");
        }

        /// <summary>
        /// Sets the Uri of the source graph (null denotes the DEFAULT graph)
        /// </summary>
        public RDFAddOperation SetFromContext(Uri fromContext)
        {
            FromContext = fromContext;
            return this;
        }

        /// <summary>
        /// Sets the Uri of the destination graph (null denotes the DEFAULT graph)
        /// </summary>
        public RDFAddOperation SetToContext(Uri toContext)
        {
            ToContext = toContext;
            return this;
        }

        /// <summary>
        /// Sets the operation as silent, so that errors will not be delivered to the application
        /// </summary>
        public RDFAddOperation Silent()
        {
            IsSilent = true;
            return this;
        }
        #endregion
    }
}