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
    /// RDFDeleteWhereOperation is the SPARQL "DELETE WHERE" operation implementation
    /// </summary>
    public class RDFDeleteWhereOperation : RDFOperation
    {
        #region Interfaces
        /// <summary>
        /// Gives the string representation of the DELETE WHERE operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintDeleteWhereOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the templates of the operation
        /// </summary>
        public RDFDeleteWhereOperation AddDeleteTemplate(RDFPattern template)
            => (RDFDeleteWhereOperation)AddDeleteNonGroundTemplate(template);

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public new RDFDeleteWhereOperation AddPrefix(RDFNamespace prefix)
            => (RDFDeleteWhereOperation)base.AddPrefix(prefix);

        /// <summary>
        /// Adds the given pattern group to the body of the operation
        /// </summary>
        public new RDFDeleteWhereOperation AddPatternGroup(RDFPatternGroup patternGroup)
            => (RDFDeleteWhereOperation)base.AddPatternGroup(patternGroup);

        /// <summary>
        /// Adds the given modifier to the operation
        /// </summary>
        public new RDFDeleteWhereOperation AddModifier(RDFDistinctModifier modifier)
            => (RDFDeleteWhereOperation)base.AddModifier(modifier);

        /// <summary>
        /// Adds the given subquery to the operation
        /// </summary>
        public new RDFDeleteWhereOperation AddSubQuery(RDFSelectQuery subQuery)
            => (RDFDeleteWhereOperation)base.AddSubQuery(subQuery);
        #endregion
    }
}