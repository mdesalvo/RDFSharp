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
    /// RDFInsertWhereOperation is the SPARQL "INSERT WHERE" operation implementation
    /// </summary>
    public class RDFInsertWhereOperation : RDFOperation
    {
        #region Interfaces
        /// <summary>
        /// Gives the string representation of the INSERT WHERE operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintInsertWhereOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the templates of the operation
        /// </summary>
        public RDFInsertWhereOperation AddInsertTemplate(RDFPattern template)
            => (RDFInsertWhereOperation)AddInsertNonGroundTemplate(template);

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public new RDFInsertWhereOperation AddPrefix(RDFNamespace prefix)
            => (RDFInsertWhereOperation)base.AddPrefix(prefix);

        /// <summary>
        /// Adds the given pattern group to the body of the operation
        /// </summary>
        public new RDFInsertWhereOperation AddPatternGroup(RDFPatternGroup patternGroup)
            => (RDFInsertWhereOperation)base.AddPatternGroup(patternGroup);

        /// <summary>
        /// Adds the given modifier to the operation
        /// </summary>
        public new RDFInsertWhereOperation AddModifier(RDFDistinctModifier modifier)
            => (RDFInsertWhereOperation)base.AddModifier(modifier);

        /// <summary>
        /// Adds the given subquery to the operation
        /// </summary>
        public new RDFInsertWhereOperation AddSubQuery(RDFSelectQuery subQuery)
            => (RDFInsertWhereOperation)base.AddSubQuery(subQuery);
        #endregion
    }
}