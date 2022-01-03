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
            => AddDeleteNonGroundTemplate<RDFDeleteWhereOperation>(template);

        /// <summary>
        /// Adds the given prefix declaration to the operation
        /// </summary>
        public RDFDeleteWhereOperation AddPrefix(RDFNamespace prefix)
            => AddPrefix<RDFDeleteWhereOperation>(prefix);

        /// <summary>
        /// Adds the given pattern group to the body of the operation
        /// </summary>
        public RDFDeleteWhereOperation AddPatternGroup(RDFPatternGroup patternGroup)
            => AddPatternGroup<RDFDeleteWhereOperation>(patternGroup);

        /// <summary>
        /// Adds the given modifier to the operation
        /// </summary>
        public RDFDeleteWhereOperation AddModifier(RDFDistinctModifier modifier)
            => AddModifier<RDFDeleteWhereOperation>(modifier);

        /// <summary>
        /// Adds the given subquery to the operation
        /// </summary>
        public RDFDeleteWhereOperation AddSubQuery(RDFSelectQuery subQuery)
            => AddSubQuery<RDFDeleteWhereOperation>(subQuery);
        #endregion
    }
}