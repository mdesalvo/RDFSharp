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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFLoadOperation is the SPARQL "LOAD" operation implementation
    /// </summary>
    public class RDFLoadOperation : RDFOperation
    {
        #region Properties
        /// <summary>
        /// Flag indicating that the operation will hide errors from the SPARQL UPDATE endpoint
        /// </summary>
        public bool IsSilent { get; internal set; }

        /// <summary>
        /// Represents the Uri of the remote graph from which RDF data will be fetched
        /// </summary>
        public Uri FromContext { get; internal set; }

        /// <summary>
        /// Represents the Uri of the graph into which RDF data will be inserted
        /// </summary>
        public Uri ToContext { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a LOAD operation for the given remote graph Uri
        /// </summary>
        public RDFLoadOperation(Uri fromContext)
        {
            if (fromContext == null)
                throw new RDFQueryException("Cannot create RDFLoadOperation because given \"fromContext\" parameter is null.");

            this.FromContext = fromContext;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the LOAD operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintLoadOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Sets the context of the graph into which RDF data will be inserted
        /// </summary>
        public RDFLoadOperation SetContext(Uri toContext)
        {
            this.ToContext = toContext;
            return this;
        }

        /// <summary>
        /// Sets the operation as silent, so that errors will not be delivered to the application
        /// </summary>
        public RDFLoadOperation Silent()
        {
            this.IsSilent = true;
            return this;
        }
        #endregion
    }
}