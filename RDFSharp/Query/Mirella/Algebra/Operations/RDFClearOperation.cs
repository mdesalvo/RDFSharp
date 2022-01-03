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
using System;
using static RDFSharp.Query.RDFQueryEnums;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFClearOperation is the SPARQL "CLEAR" operation implementation
    /// </summary>
    public class RDFClearOperation : RDFOperation
    {
        #region Properties
        /// <summary>
        /// Flag indicating that the operation will hide errors from the SPARQL UPDATE endpoint
        /// </summary>
        public bool IsSilent { get; internal set; }

        /// <summary>
        /// Represents the Uri of the remote graph from which RDF data will be removed
        /// </summary>
        public Uri FromContext { get; internal set; }

        /// <summary>
        /// Represents the flavor adopted in case of an implicit SPARQL CLEAR operation
        /// </summary>
        public RDFClearOperationFlavor OperationFlavor { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an explicit CLEAR operation for the given remote graph Uri
        /// </summary>
        public RDFClearOperation(Uri fromContext)
        {
            if (fromContext == null)
                throw new RDFQueryException("Cannot create RDFClearOperation because given \"fromContext\" parameter is null.");

            this.FromContext = fromContext;
        }

        /// <summary>
        /// Default-ctor to build an implicit CLEAR operation with the given flavor
        /// </summary>
        public RDFClearOperation(RDFClearOperationFlavor operationFlavor)
            => this.OperationFlavor = operationFlavor;
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the CLEAR operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintClearOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Sets the operation as silent, so that errors will not be delivered to the application
        /// </summary>
        public RDFClearOperation Silent()
        {
            this.IsSilent = true;
            return this;
        }
        #endregion
    }
}