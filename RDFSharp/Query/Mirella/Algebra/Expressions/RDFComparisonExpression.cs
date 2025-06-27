/*
   Copyright 2012-2025 Marco De Salvo

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
using System.Data;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFComparisonExpression represents an expression applying a comparison between the given arguments.
    /// </summary>
    public sealed class RDFComparisonExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// Comparison to be applied between the expression arguments
        /// </summary>
        public RDFQueryEnums.RDFComparisonFlavors ComparisonFlavor { get; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a comparison expression of the given type on the given arguments
        /// </summary>
        public RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor, RDFExpression leftArgument, RDFExpression rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion

            ComparisonFlavor = comparisonFlavor;
        }

        /// <summary>
        /// Default-ctor to build a comparison expression of the given type on the given arguments
        /// </summary>
        public RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor, RDFExpression leftArgument, RDFVariable rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion

            ComparisonFlavor = comparisonFlavor;
        }

        /// <summary>
        /// Default-ctor to build a comparison expression of the given type on the given arguments
        /// </summary>
        public RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor, RDFVariable leftArgument, RDFExpression rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion

            ComparisonFlavor = comparisonFlavor;
        }

        /// <summary>
        /// Default-ctor to build a comparison expression of the given type on the given arguments
        /// </summary>
        public RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor, RDFVariable leftArgument, RDFVariable rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion

            ComparisonFlavor = comparisonFlavor;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the unary expression
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(L OPERATOR R)
            sb.Append('(');
            string leftValue = LeftArgument is RDFExpression leftArgumentExpression ? leftArgumentExpression.ToString(prefixes) : RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes);
            string rightValue = RightArgument is RDFExpression rightArgumentExpression ? rightArgumentExpression.ToString(prefixes) : RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes);
            switch (ComparisonFlavor)
            {
                case RDFQueryEnums.RDFComparisonFlavors.LessThan:
                    sb.Append($"{leftValue} < {rightValue}");
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan:
                    sb.Append($"{leftValue} <= {rightValue}");
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.EqualTo:
                    sb.Append($"{leftValue} = {rightValue}");
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.NotEqualTo:
                    sb.Append($"{leftValue} != {rightValue}");
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan:
                    sb.Append($"{leftValue} >= {rightValue}");
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.GreaterThan:
                    sb.Append($"{leftValue} > {rightValue}");
                    break;
            }
            sb.Append(')');

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the comparison expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
        {
            RDFPatternMember expressionResult = null;

            #region Guards
            if (LeftArgument is RDFVariable && !row.Table.Columns.Contains(LeftArgument.ToString()))
                return null;
            if (RightArgument is RDFVariable && !row.Table.Columns.Contains(RightArgument.ToString()))
                return null;
            #endregion

            try
            {
                #region Evaluate Arguments
                //Evaluate left argument (Expression VS Variable)
                RDFPatternMember leftArgumentPMember;
                if (LeftArgument is RDFExpression leftArgumentExpression)
                    leftArgumentPMember = leftArgumentExpression.ApplyExpression(row);
                else
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[LeftArgument.ToString()].ToString());

                //Evaluate right argument (Expression VS Variable)
                RDFPatternMember rightArgumentPMember;
                if (RightArgument is RDFExpression rightArgumentExpression)
                    rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                else
                    rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember(row[RightArgument.ToString()].ToString());

                //Binding error from arguments
                if (leftArgumentPMember == null || rightArgumentPMember == null)
                    return null;
                #endregion

                #region Calculate Result
                int comparison = RDFQueryUtilities.CompareRDFPatternMembers(leftArgumentPMember, rightArgumentPMember);

                //Type Error
                if (comparison == -99)
                    return null;

                //Type Correct
                switch (ComparisonFlavor)
                {
                    case RDFQueryEnums.RDFComparisonFlavors.LessThan:
                        expressionResult = comparison  < 0 ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan:
                        expressionResult = comparison <= 0 ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.EqualTo:
                        expressionResult = comparison == 0 ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.NotEqualTo:
                        expressionResult = comparison != 0 ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan:
                        expressionResult = comparison >= 0 ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                        break;
                    case RDFQueryEnums.RDFComparisonFlavors.GreaterThan:
                        expressionResult = comparison  > 0 ? RDFTypedLiteral.True : RDFTypedLiteral.False;
                        break;
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }
        #endregion
    }
}