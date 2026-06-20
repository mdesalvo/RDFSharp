/*
   Copyright 2012-2026 Marco De Salvo

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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFBinaryQueryMember represents a binary SPARQL algebra node (Union or Minus)
    /// that combines two query members into a tree structure.
    /// </summary>
    public sealed class RDFBinaryQueryMember : RDFQueryMember
    {
        #region Properties
        /// <summary>
        /// Type of the binary operator (Union or Minus)
        /// </summary>
        public RDFQueryEnums.RDFBinaryOperatorType OperatorType { get; }

        /// <summary>
        /// Left operand of the binary operator
        /// </summary>
        public RDFQueryMember LeftOperand { get; }

        /// <summary>
        /// Right operand of the binary operator
        /// </summary>
        public RDFQueryMember RightOperand { get; }

        /// <summary>
        /// Flag indicating the operator result to be joined as Optional
        /// </summary>
        internal bool IsOptional { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a binary operator node with the given operands
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        internal RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType operatorType, RDFQueryMember left, RDFQueryMember right)
        {
            #region Guards
            if (left == null)
                throw new RDFQueryException("Cannot create RDFBinaryQueryMember because given \"left\" parameter is null.");
            if (right == null)
                throw new RDFQueryException("Cannot create RDFBinaryQueryMember because given \"right\" parameter is null.");
            if (!(left is RDFPatternGroup || left is RDFSelectQuery || left is RDFBinaryQueryMember))
                throw new RDFQueryException("Cannot create RDFBinaryQueryMember because given \"left\" parameter is not a pattern group, select query, or operator node.");
            if (!(right is RDFPatternGroup || right is RDFSelectQuery || right is RDFBinaryQueryMember))
                throw new RDFQueryException("Cannot create RDFBinaryQueryMember because given \"right\" parameter is not a pattern group, select query, or operator node.");
            if (ReferenceEquals(left, right))
                throw new RDFQueryException("Cannot create RDFBinaryQueryMember because \"left\" and \"right\" are the same instance (self-reference would cause redundant evaluation).");
            #endregion

            OperatorType = operatorType;
            LeftOperand = left;
            RightOperand = right;
            IsEvaluable = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a Union operator combining this node with the given query member
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Union(RDFPatternGroup other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this node with the given subquery
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Union(RDFSelectQuery other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this node with the given operator tree
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Union(RDFBinaryQueryMember other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Minus operator combining this node with the given query member
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Minus(RDFPatternGroup other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this node with the given subquery
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Minus(RDFSelectQuery other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this node with the given operator tree
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryQueryMember Minus(RDFBinaryQueryMember other)
            => new RDFBinaryQueryMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Sets the operator result to be joined as Optional with the previous query member
        /// </summary>
        public RDFBinaryQueryMember Optional()
        {
            IsOptional = true;
            return this;
        }
        #endregion
    }
}