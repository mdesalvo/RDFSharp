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
using System.Linq;
using System.Threading.Tasks;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFDeleteDataOperation is the SPARQL "DELETE DATA" operation implementation
    /// </summary>
    public class RDFDeleteDataOperation : RDFOperation
    {
        #region Interfaces
        /// <summary>
        /// Gives the string representation of the DELETE DATA operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintDeleteDataOperation(this);
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty DELETE DATA operation
        /// </summary>
        public RDFDeleteDataOperation() : base()
            => this.IsDeleteData = true;
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given ground pattern to the templates of the operation
        /// </summary>
        public RDFDeleteDataOperation AddDeleteTemplate(RDFPattern template)
        {
            //This operation accepts only ground patterns
            if (template?.Variables.Count == 0)
            {
                if (!this.DeleteTemplates.Any(tp => tp.Equals(template)))
                    this.DeleteTemplates.Add(template);
            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public RDFDeleteDataOperation AddPrefix(RDFNamespace prefix)
        {
            if (prefix != null)
            {
                if (!this.Prefixes.Any(p => p.Equals(prefix)))
                    this.Prefixes.Add(prefix);
            }
            return this;
        }

        /// <summary>
        /// Applies the operation to the given graph
        /// </summary>
        public override RDFOperationResult ApplyToGraph(RDFGraph graph)
            => graph != null ? new RDFOperationEngine().EvaluateDeleteDataOperation(this, graph)
                             : new RDFOperationResult();

        /// <summary>
        /// Asynchronously applies the operation to the given graph
        /// </summary>
        public override Task<RDFOperationResult> ApplyToGraphAsync(RDFGraph graph)
            => Task.Run(() => ApplyToGraph(graph));

        /// <summary>
        /// Applies the operation to the given store
        /// </summary>
        public override RDFOperationResult ApplyToStore(RDFStore store)
            => store != null ? new RDFOperationEngine().EvaluateDeleteDataOperation(this, store)
                             : new RDFOperationResult();

        /// <summary>
        /// Asynchronously applies the operation to the given store
        /// </summary>
        public override Task<RDFOperationResult> ApplyToStoreAsync(RDFStore store)
            => Task.Run(() => ApplyToStore(store));
        #endregion
    }
}