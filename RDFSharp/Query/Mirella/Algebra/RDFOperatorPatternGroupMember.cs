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

using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOperatorPatternGroupMember represents a binary SPARQL algebra node (Union or Minus)
    /// that combines two pattern group members into a tree structure.
    /// </summary>
    public sealed class RDFOperatorPatternGroupMember : RDFPatternGroupMember
    {
        #region Properties
        /// <summary>
        /// Type of the binary operator (Union or Minus)
        /// </summary>
        public RDFQueryEnums.RDFQueryOperatorType OperatorType { get; }

        /// <summary>
        /// Left operand of the binary operator
        /// </summary>
        public RDFPatternGroupMember LeftOperand { get; }

        /// <summary>
        /// Right operand of the binary operator
        /// </summary>
        public RDFPatternGroupMember RightOperand { get; }

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
        internal RDFOperatorPatternGroupMember(RDFQueryEnums.RDFQueryOperatorType operatorType, RDFPatternGroupMember left, RDFPatternGroupMember right)
        {
            #region Guards
            if (left == null)
                throw new RDFQueryException("Cannot create RDFOperatorPatternGroupMember because given \"left\" parameter is null.");
            if (right == null)
                throw new RDFQueryException("Cannot create RDFOperatorPatternGroupMember because given \"right\" parameter is null.");
            if (!(left is RDFPattern || left is RDFPropertyPath || left is RDFOperatorPatternGroupMember))
                throw new RDFQueryException("Cannot create RDFOperatorPatternGroupMember because given \"left\" parameter is not a pattern, property path, or operator node.");
            if (!(right is RDFPattern || right is RDFPropertyPath || right is RDFOperatorPatternGroupMember))
                throw new RDFQueryException("Cannot create RDFOperatorPatternGroupMember because given \"right\" parameter is not a pattern, property path, or operator node.");
            if (ReferenceEquals(left, right))
                throw new RDFQueryException("Cannot create RDFOperatorPatternGroupMember because \"left\" and \"right\" are the same instance (self-reference would cause redundant evaluation).");
            #endregion

            OperatorType = operatorType;
            LeftOperand = left;
            RightOperand = right;
            IsEvaluable = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates a Union operator combining this node with the given pattern
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFOperatorPatternGroupMember Union(RDFPattern other)
            => new RDFOperatorPatternGroupMember(RDFQueryEnums.RDFQueryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this node with the given property path
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFOperatorPatternGroupMember Union(RDFPropertyPath other)
            => new RDFOperatorPatternGroupMember(RDFQueryEnums.RDFQueryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this node with the given operator tree
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFOperatorPatternGroupMember Union(RDFOperatorPatternGroupMember other)
            => new RDFOperatorPatternGroupMember(RDFQueryEnums.RDFQueryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Minus operator combining this node with the given pattern
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFOperatorPatternGroupMember Minus(RDFPattern other)
            => new RDFOperatorPatternGroupMember(RDFQueryEnums.RDFQueryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this node with the given property path
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFOperatorPatternGroupMember Minus(RDFPropertyPath other)
            => new RDFOperatorPatternGroupMember(RDFQueryEnums.RDFQueryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this node with the given operator tree
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFOperatorPatternGroupMember Minus(RDFOperatorPatternGroupMember other)
            => new RDFOperatorPatternGroupMember(RDFQueryEnums.RDFQueryOperatorType.Minus, this, other);

        /// <summary>
        /// Sets the operator result to be joined as Optional with the previous member
        /// </summary>
        public RDFOperatorPatternGroupMember Optional()
        {
            IsOptional = true;
            return this;
        }

        /// <summary>
        /// Collects all variables from this operator tree's leaf patterns and property paths
        /// </summary>
        internal IEnumerable<RDFVariable> GetVariables()
        {
            foreach (RDFVariable variable in GetVariablesFromOperand(LeftOperand))
                yield return variable;
            foreach (RDFVariable variable in GetVariablesFromOperand(RightOperand))
                yield return variable;
        }
        #endregion

        #region Internals
        /// <summary>
        /// Collects variables from a single operand (recursing into operator subtrees)
        /// </summary>
        private static IEnumerable<RDFVariable> GetVariablesFromOperand(RDFPatternGroupMember operand)
        {
            switch (operand)
            {
                case RDFPattern pattern:
                    return pattern.Variables;
                case RDFPropertyPath propertyPath:
                    List<RDFVariable> ppVars = new List<RDFVariable>(2);
                    if (propertyPath.Start is RDFVariable startVar)
                        ppVars.Add(startVar);
                    if (propertyPath.End is RDFVariable endVar)
                        ppVars.Add(endVar);
                    return ppVars;
                case RDFOperatorPatternGroupMember operatorNode:
                    return operatorNode.GetVariables();
                default:
                    return Enumerable.Empty<RDFVariable>();
            }
        }
        #endregion
    }
}
