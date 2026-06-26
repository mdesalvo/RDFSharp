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
    /// RDFMoveOperation is the SPARQL "MOVE" graph-management operation implementation
    /// (<c>MOVE 'SILENT'? GraphOrDefault TO GraphOrDefault</c>): it first clears the destination graph,
    /// inserts into it all the triples of the source graph, then drops the source graph (effectively a
    /// rename source→destination). A null context (on either side) denotes the DEFAULT graph; a non-null
    /// context denotes a named graph. Being a two-graph operation, it is meaningful only on a
    /// context-aware store.
    /// </summary>
    public sealed class RDFMoveOperation : RDFOperation
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
        /// Gives the string representation of the MOVE operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintMoveOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Parses the given SPARQL UPDATE string into an RDFMoveOperation.
        /// </summary>
        /// <exception cref="RDFQueryException">When the string is not a syntactically valid MOVE operation.</exception>
        public static RDFMoveOperation FromString(string moveOperation)
        {
            RDFOperation parsedOperation = RDFOperationParserFactory.ParseOperation(moveOperation);

            //The factory dispatches on the operation form: enforce that the parsed operation is indeed a MOVE
            if (parsedOperation is RDFMoveOperation parsedMoveOperation)
                return parsedMoveOperation;

            throw new RDFQueryException("Cannot parse MOVE operation because the given command represents a different SPARQL UPDATE operation (" + parsedOperation.GetType().Name + ")");
        }

        /// <summary>
        /// Sets the Uri of the source graph (null denotes the DEFAULT graph)
        /// </summary>
        public RDFMoveOperation SetFromContext(Uri fromContext)
        {
            FromContext = fromContext;
            return this;
        }

        /// <summary>
        /// Sets the Uri of the destination graph (null denotes the DEFAULT graph)
        /// </summary>
        public RDFMoveOperation SetToContext(Uri toContext)
        {
            ToContext = toContext;
            return this;
        }

        /// <summary>
        /// Sets the operation as silent, so that errors will not be delivered to the application
        /// </summary>
        public RDFMoveOperation Silent()
        {
            IsSilent = true;
            return this;
        }
        #endregion
    }
}