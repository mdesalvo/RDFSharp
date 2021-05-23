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
    public abstract class RDFOperation : RDFQuery
    {
        #region Properties
        /// <summary>
        /// Templates for SPARQL DELETE operations
        /// </summary>
        internal List<RDFPattern> DeleteTemplates { get; set; }

        /// <summary>
        /// Templates for SPARQL INSERT operations
        /// </summary>
        internal List<RDFPattern> InsertTemplates { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty operation
        /// </summary>
        internal RDFOperation() : base()
        {
            this.DeleteTemplates = new List<RDFPattern>();
            this.InsertTemplates = new List<RDFPattern>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the operation to the given graph
        /// </summary>
        public abstract RDFOperationResult ApplyToGraph(RDFGraph graph);

        /// <summary>
        /// Asynchronously applies the operation to the given graph
        /// </summary>
        public abstract Task<RDFOperationResult> ApplyToGraphAsync(RDFGraph graph);

        /// <summary>
        /// Applies the operation to the given store
        /// </summary>
        public abstract RDFOperationResult ApplyToStore(RDFStore store);

        /// <summary>
        /// Asynchronously applies the operation to the given store
        /// </summary>
        public abstract Task<RDFOperationResult> ApplyToStoreAsync(RDFStore store);
        #endregion
    }
}