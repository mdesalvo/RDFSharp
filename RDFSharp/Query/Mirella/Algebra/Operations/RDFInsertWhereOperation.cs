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
            => AddInsertNonGroundTemplate<RDFInsertWhereOperation>(template);

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public RDFInsertWhereOperation AddPrefix(RDFNamespace prefix)
            => AddPrefix<RDFInsertWhereOperation>(prefix);

        /// <summary>
        /// Adds the given pattern group to the body of the operation
        /// </summary>
        public RDFInsertWhereOperation AddPatternGroup(RDFPatternGroup patternGroup)
            => AddPatternGroup<RDFInsertWhereOperation>(patternGroup);

        /// <summary>
        /// Adds the given modifier to the operation
        /// </summary>
        public RDFInsertWhereOperation AddModifier(RDFDistinctModifier modifier)
            => AddModifier<RDFInsertWhereOperation>(modifier);

        /// <summary>
        /// Adds the given subquery to the operation
        /// </summary>
        public RDFInsertWhereOperation AddSubQuery(RDFSelectQuery subQuery)
            => AddSubQuery<RDFInsertWhereOperation>(subQuery);
        #endregion
    }
}