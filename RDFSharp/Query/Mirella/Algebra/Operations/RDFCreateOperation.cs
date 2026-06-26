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
    /// RDFCreateOperation is the SPARQL "CREATE" graph-management operation implementation:
    /// it declares the existence of a named graph (<c>CREATE 'SILENT'? GRAPH iri</c>). Since RDFSharp does
    /// not record empty graphs, applying it to a graph/store is a spec-legal no-op; it is fully meaningful
    /// only when forwarded to a SPARQL UPDATE endpoint that does record empty graphs.
    /// </summary>
    public sealed class RDFCreateOperation : RDFOperation
    {
        #region Properties
        /// <summary>
        /// Flag indicating that the operation will hide errors from the SPARQL UPDATE endpoint
        /// </summary>
        public bool IsSilent { get; internal set; }

        /// <summary>
        /// Represents the Uri of the named graph to be created
        /// </summary>
        public Uri FromContext { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a CREATE operation for the given named graph Uri
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFCreateOperation(Uri fromContext)
            => FromContext = fromContext ?? throw new RDFQueryException("Cannot create RDFCreateOperation because given \"fromContext\" parameter is null.");
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the CREATE operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintCreateOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Parses the given SPARQL UPDATE string into an RDFCreateOperation.
        /// </summary>
        /// <exception cref="RDFQueryException">When the string is not a syntactically valid CREATE operation.</exception>
        public static RDFCreateOperation FromString(string createOperation)
        {
            RDFOperation parsedOperation = RDFOperationParserFactory.ParseOperation(createOperation);

            //The factory dispatches on the operation form: enforce that the parsed operation is indeed a CREATE
            if (parsedOperation is RDFCreateOperation parsedCreateOperation)
                return parsedCreateOperation;

            throw new RDFQueryException("Cannot parse CREATE operation because the given command represents a different SPARQL UPDATE operation (" + parsedOperation.GetType().Name + ")");
        }

        /// <summary>
        /// Sets the operation as silent, so that errors will not be delivered to the application
        /// </summary>
        public RDFCreateOperation Silent()
        {
            IsSilent = true;
            return this;
        }
        #endregion
    }
}