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
    /// RDFDeleteWhereOperation is the SPARQL "DELETE/INSERT WHERE" operation implementation
    /// </summary>
    public class RDFDeleteInsertWhereOperation : RDFOperation
    {
        #region Interfaces
        /// <summary>
        /// Gives the string representation of the DELETE/INSERT WHERE operation
        /// </summary>
        public override string ToString()
            => RDFOperationPrinter.PrintDeleteInsertWhereOperation(this);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the DELETE templates of the operation
        /// </summary>
        public RDFDeleteInsertWhereOperation AddDeleteTemplate(RDFPattern template)
            => AddDeleteNonGroundTemplate<RDFDeleteInsertWhereOperation>(template);

        /// <summary>
        /// Adds the given pattern to the INSERT templates of the operation
        /// </summary>
        public RDFDeleteInsertWhereOperation AddInsertTemplate(RDFPattern template)
            => AddInsertNonGroundTemplate<RDFDeleteInsertWhereOperation>(template);

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public RDFDeleteInsertWhereOperation AddPrefix(RDFNamespace prefix)
            => AddPrefix<RDFDeleteInsertWhereOperation>(prefix);

        /// <summary>
        /// Adds the given pattern group to the body of the operation
        /// </summary>
        public RDFDeleteInsertWhereOperation AddPatternGroup(RDFPatternGroup patternGroup)
            => AddPatternGroup<RDFDeleteInsertWhereOperation>(patternGroup);

        /// <summary>
        /// Adds the given modifier to the operation
        /// </summary>
        public RDFDeleteInsertWhereOperation AddModifier(RDFDistinctModifier modifier)
            => AddModifier<RDFDeleteInsertWhereOperation>(modifier);

        /// <summary>
        /// Adds the given subquery to the operation
        /// </summary>
        public RDFDeleteInsertWhereOperation AddSubQuery(RDFSelectQuery subQuery)
            => AddSubQuery<RDFDeleteInsertWhereOperation>(subQuery);
        #endregion
    }
}