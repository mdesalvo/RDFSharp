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
using static RDFSharp.Query.RDFQueryEnums;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFDropOperation is the SPARQL "DROP" graph-management operation implementation
    /// (<c>DROP 'SILENT'? (GRAPH iri | DEFAULT | NAMED | ALL)</c>). Since RDFSharp does not record empty
    /// graphs, DROP is equivalent to CLEAR: it removes the triples/quadruples of the referenced graph(s).
    /// </summary>
    public sealed class RDFDropOperation : RDFOperation
    {
        #region Properties
        /// <summary>
        /// Flag indicating that the operation will hide errors from the SPARQL UPDATE endpoint
        /// </summary>
        public bool IsSilent { get; internal set; }

        /// <summary>
        /// Represents the Uri of the remote graph to be dropped
        /// </summary>
        public Uri FromContext { get; internal set; }

        /// <summary>
        /// Represents the flavor adopted in case of an implicit SPARQL DROP operation
        /// </summary>
        public RDFClearOperationFlavor OperationFlavor { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an explicit DROP operation for the given remote graph Uri
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFDropOperation(Uri fromContext)
            => FromContext = fromContext ?? throw new RDFQueryException("Cannot create RDFDropOperation because given \"fromContext\" parameter is null.");

        /// <summary>
        /// Builds an implicit DROP operation with the given flavor
        /// </summary>
        public RDFDropOperation(RDFClearOperationFlavor operationFlavor)
            => OperationFlavor = operationFlavor;
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the DROP operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintDropOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Parses the given SPARQL UPDATE string into an RDFDropOperation.
        /// </summary>
        /// <exception cref="RDFQueryException">When the string is not a syntactically valid DROP operation.</exception>
        public static RDFDropOperation FromString(string dropOperation)
        {
            RDFOperation parsedOperation = RDFOperationParserFactory.ParseOperation(dropOperation);

            //The factory dispatches on the operation form: enforce that the parsed operation is indeed a DROP
            if (parsedOperation is RDFDropOperation parsedDropOperation)
                return parsedDropOperation;

            throw new RDFQueryException("Cannot parse DROP operation because the given command represents a different SPARQL UPDATE operation (" + parsedOperation.GetType().Name + ")");
        }

        /// <summary>
        /// Sets the operation as silent, so that errors will not be delivered to the application
        /// </summary>
        public RDFDropOperation Silent()
        {
            IsSilent = true;
            return this;
        }
        #endregion
    }
}