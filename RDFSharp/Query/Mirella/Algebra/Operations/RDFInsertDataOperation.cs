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
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static RDFSharp.Query.RDFQueryUtilities;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFInsertDataOperation is the SPARQL "INSERT DATA" operation implementation
    /// </summary>
    public class RDFInsertDataOperation : RDFOperation
    {
        #region Interfaces
        /// <summary>
        /// Gives the string representation of the INSERT DATA operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintInsertDataOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given ground pattern to the templates of the operation
        /// </summary>
        public RDFInsertDataOperation AddInsertTemplate(RDFPattern template)
        {
            //This operation accepts only ground patterns
            if (template?.Variables.Count == 0)
            {
                if (!this.InsertTemplates.Any(tp => tp.Equals(template)))
                    this.InsertTemplates.Add(template);
            }
            return this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public RDFInsertDataOperation AddPrefix(RDFNamespace prefix)
        {
            if (prefix != null)
            {
                if (!this.Prefixes.Any(p => p.Equals(prefix)))
                    this.Prefixes.Add(prefix);
            }
            return this;
        }
        #endregion
    }
}