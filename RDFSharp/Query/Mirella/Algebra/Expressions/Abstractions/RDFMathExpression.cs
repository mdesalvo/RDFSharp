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
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFMathExpression represents an arithmetical expression to be applied on a query results table.
    /// </summary>
    public abstract class RDFMathExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds an arithmetical expression with given arguments
        /// </summary>
        protected RDFMathExpression(RDFExpression leftArgument, RDFExpression rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Builds an arithmetical expression with given arguments
        /// </summary>
        protected RDFMathExpression(RDFExpression leftArgument, RDFVariable rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Builds an arithmetical expression with given arguments
        /// </summary>
        protected RDFMathExpression(RDFExpression leftArgument, RDFTypedLiteral rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.HasDecimalDatatype())
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a numeric typed literal");
            #endregion
        }

        /// <summary>
        /// Builds an arithmetical expression with given arguments
        /// </summary>
        protected RDFMathExpression(RDFVariable leftArgument, RDFExpression rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Builds an arithmetical expression with given arguments
        /// </summary>
        protected RDFMathExpression(RDFVariable leftArgument, RDFVariable rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            #endregion
        }

        /// <summary>
        /// Builds an arithmetical expression with given arguments
        /// </summary>
        protected RDFMathExpression(RDFVariable leftArgument, RDFTypedLiteral rightArgument)
            : base(leftArgument, rightArgument)
        {
            #region Guards
            if (rightArgument == null)
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is null");
            if (!rightArgument.HasDecimalDatatype())
                throw new RDFQueryException("Cannot create expression because given \"rightArgument\" parameter is not a numeric typed literal");
            #endregion
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the arithmetical addition expression
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //(L MATHOP R)
            sb.Append('(');
            if (LeftArgument is RDFExpression expLeftArgument)
                sb.Append(expLeftArgument.ToString(prefixes));
            else
                sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));
            sb.Append(' ').Append(ArithmeticOperator).Append(' ');
            switch (RightArgument)
            {
                case RDFExpression expRightArgument:
                    sb.Append(expRightArgument.ToString(prefixes));
                    break;
                case RDFTypedLiteral tlRightArgument:
                    sb.Append(tlRightArgument.Value);
                    break;
                default:
                    sb.Append(RDFQueryPrinter.PrintPatternMember((RDFPatternMember)RightArgument, prefixes));
                    break;
            }
            sb.Append(')');

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the arithmetical expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(RDFTableRow row)
        {
            RDFTypedLiteral expressionResult = null;

            #region Guards
            string leftColumnName = LeftArgument is RDFVariable ? LeftArgument.ToString() : null;
            if (leftColumnName != null && !row.HasColumn(leftColumnName))
                return null;
            string rightColumnName = RightArgument is RDFVariable ? RightArgument.ToString() : null;
            if (rightColumnName != null && !row.HasColumn(rightColumnName))
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
                    leftArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember((row[leftColumnName] ?? string.Empty));

                //Evaluate right argument (Expression VS Variable VS TypedLiteral)
                RDFPatternMember rightArgumentPMember;
                switch (RightArgument)
                {
                    case RDFExpression rightArgumentExpression:
                        rightArgumentPMember = rightArgumentExpression.ApplyExpression(row);
                        break;
                    case RDFVariable _:
                        rightArgumentPMember = RDFQueryUtilities.ParseRDFPatternMember((row[rightColumnName] ?? string.Empty));
                        break;
                    default:
                        rightArgumentPMember = (RDFTypedLiteral)RightArgument;
                        break;
                }
                #endregion

                #region Calculate Result
                //Delegate to the shared SPARQL numeric-arithmetic primitive, which applies the type-promotion lattice
                //(integer<decimal<float<double), computes integer/decimal exactly and returns null on type errors,
                //division by zero, overflow or non-finite results (treated by the caller as "no binding")
                if (leftArgumentPMember is RDFTypedLiteral leftArgumentTypedLiteral
                     && rightArgumentPMember is RDFTypedLiteral rightArgumentTypedLiteral)
                {
                    expressionResult = RDFArithmeticEngine.ComputeNumericOperation(leftArgumentTypedLiteral, rightArgumentTypedLiteral, ArithmeticOperator);
                }
                #endregion
            }
            catch { /* Just a no-op, since type errors are normal when trying to face variable's bindings */ }

            return expressionResult;
        }

        /// <summary>
        /// The arithmetic operator carried by this expression ('+', '-', '*' or '/'): supplied by each concrete
        /// subclass and consumed by both the printer (ToString) and the shared numeric-arithmetic engine
        /// </summary>
        protected abstract char ArithmeticOperator { get; }
        #endregion
    }
}