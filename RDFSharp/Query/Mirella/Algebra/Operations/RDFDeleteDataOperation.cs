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

        #region Methods
        /// <summary>
        /// Adds the given ground pattern to the templates of the operation
        /// </summary>
        public RDFDeleteDataOperation AddDeleteTemplate(RDFPattern template)
            => AddDeleteGroundTemplate<RDFDeleteDataOperation>(template);

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public RDFDeleteDataOperation AddPrefix(RDFNamespace prefix)
            => AddPrefix<RDFDeleteDataOperation>(prefix);
        #endregion
    }
}