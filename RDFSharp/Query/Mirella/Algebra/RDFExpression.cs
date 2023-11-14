/*
   Copyright 2012-2023 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFExpression represents an expression to be applied on a query results table.
    /// </summary>
    public abstract class RDFExpression : RDFExpressionArgument
    {
        #region Properties
        /// <summary>
        /// Represents the left argument given to the expression
        /// </summary>
        public RDFExpressionArgument LeftArgument { get; internal set; }

        /// <summary>
        /// Represents the right argument given to the expression
        /// </summary>
        public RDFExpressionArgument RightArgument { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an expression with given expression arguments
        /// </summary>
        public RDFExpression(RDFExpression leftArgument, RDFExpression rightArgument)
            : this(leftArgument as RDFExpressionArgument, rightArgument as RDFExpressionArgument) { }

        /// <summary>
        /// Default-ctor to build an expression with given mixed arguments
        /// </summary>
        public RDFExpression(RDFExpression leftArgument, RDFPatternMember rightArgument)
            : this(leftArgument as RDFExpressionArgument, rightArgument as RDFExpressionArgument) { }

        /// <summary>
        /// Default-ctor to build an expression with given mixed arguments
        /// </summary>
        public RDFExpression(RDFPatternMember leftArgument, RDFExpression rightArgument)
            : this(leftArgument as RDFExpressionArgument, rightArgument as RDFExpressionArgument) { }

        /// <summary>
        /// Default-ctor to build an expression with given pattern member arguments
        /// </summary>
        public RDFExpression(RDFPatternMember leftArgument, RDFPatternMember rightArgument)
            : this(leftArgument as RDFExpressionArgument, rightArgument as RDFExpressionArgument) { }

        /// <summary>
        /// Internal-ctor to build an expression with given expression arguments
        /// </summary>
        internal RDFExpression(RDFExpressionArgument leftArgument, RDFExpressionArgument rightArgument)
        {
            #region Guards
            bool isRandExpression = this is RDFRandExpression;
            bool isBNodeExpression = this is RDFBNodeExpression; 
            if (!isRandExpression && !isBNodeExpression && leftArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"leftArgument\" parameter is null");
            #endregion

            LeftArgument = leftArgument;
            RightArgument = rightArgument;
        }

        /// <summary>
        /// Internal-ctor to build an expression without arguments (e.g: RAND)
        /// </summary>
        internal RDFExpression() { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the expression
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal abstract string ToString(List<RDFNamespace> prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Applies the expression on the given datarow
        /// </summary>
        internal abstract RDFPatternMember ApplyExpression(DataRow row);
        #endregion
    }

    /// <summary>
    /// RDFExpressionArgument represents an argument given to an expression
    /// </summary>
    public class RDFExpressionArgument { }
}