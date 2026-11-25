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

using System;
using System.Globalization;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFArithmeticEngine evaluates SPARQL numeric arithmetic, applying the XPath/SPARQL numeric type-promotion
    /// lattice. It is the single source of truth shared by the arithmetic expressions (RDFMathExpression) and the
    /// numeric aggregators.
    /// </summary>
    internal static class RDFArithmeticEngine
    {
        //Ranks in the SPARQL numeric promotion lattice (integer < decimal < float < double); -1 = not numeric
        private const int RANK_NONNUMERIC = -1;
        private const int RANK_INTEGER    =  0;
        private const int RANK_DECIMAL    =  1;
        private const int RANK_FLOAT      =  2;
        private const int RANK_DOUBLE     =  3;

        #region Methods
        /// <summary>
        /// Gets the numeric value of the given owl:rational typed literal (lexical form "p/q")
        /// (the input has already been checked to contain a well-formed owl:rational literal)
        /// </summary>
        internal static decimal ComputeOWLRationalValue(RDFTypedLiteral typedLiteral)
        {
            string[] owlRationalParts = typedLiteral.Value.Split('/');
            return decimal.Parse(owlRationalParts[0]) / decimal.Parse(owlRationalParts[1]);
        }

        /// <summary>
        /// Computes the SPARQL numeric arithmetic operation (operator ∈ {+,-,*,/}) between the two given numeric typed
        /// literals along the XPath/SPARQL numeric type-promotion lattice (xsd:integer &lt; xsd:decimal &lt; xsd:float
        /// &lt; xsd:double), giving null (no binding) on a non-numeric operand or on an integer/decimal division by zero or overflow.
        /// </summary>
        internal static RDFTypedLiteral ComputeNumericOperation(RDFTypedLiteral leftLiteral, RDFTypedLiteral rightLiteral, char arithmeticOperator)
        {
            //Both operands must be numeric, otherwise the operation is a type error => no binding
            int leftRank  = GetNumericRank(leftLiteral);
            int rightRank = GetNumericRank(rightLiteral);
            if (leftRank == RANK_NONNUMERIC || rightRank == RANK_NONNUMERIC)
                return null;

            //Target rank is the widest operand type (integer division is special-cased to decimal)
            int targetRank = Math.Max(leftRank, rightRank);
            if (arithmeticOperator == '/' && targetRank == RANK_INTEGER)
                targetRank = RANK_DECIMAL;

            try
            {
                #region Integer/Decimal
                //Exact regime (integer/decimal) — System.Decimal keeps e.g. 3.0 + 0.1 exactly 3.1, no fp noise
                if (targetRank <= RANK_DECIMAL)
                {
                    decimal leftDecimal = GetDecimalValue(leftLiteral), rightDecimal = GetDecimalValue(rightLiteral);
                    if (arithmeticOperator == '/' && rightDecimal == 0m)
                        return null;   //division by zero => no binding

                    decimal resultDecimal;
                    switch (arithmeticOperator)
                    {
                        case '+':
                            resultDecimal = leftDecimal + rightDecimal;
                            break;
                        case '-':
                            resultDecimal = leftDecimal - rightDecimal;
                            break;
                        case '*':
                            resultDecimal = leftDecimal * rightDecimal;
                            break;
                        default:
                            resultDecimal = leftDecimal / rightDecimal;
                            break;
                    }

                    //integer result prints without a fractional part; decimal result is canonicalized (e.g. 3.10 -> 3.1)
                    return targetRank == RANK_INTEGER
                        ? new RDFTypedLiteral(resultDecimal.ToString(CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_INTEGER)
                        : new RDFTypedLiteral(NormalizeDecimal(resultDecimal), RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
                }
                #endregion

                #region Float/Double
                //Approximate regime (float/double) — the spec mandates double arithmetic for these types
                double leftDouble  = GetDoubleValue(leftLiteral);
                double rightDouble = GetDoubleValue(rightLiteral);

                //IEEE 754 applies: division by zero, overflow and infinite operands give INF, -INF or NaN instead of an error
                double resultDouble;
                switch (arithmeticOperator)
                {
                    case '+':
                        resultDouble = leftDouble + rightDouble;
                        break;
                    case '-':
                        resultDouble = leftDouble - rightDouble;
                        break;
                    case '*':
                        resultDouble = leftDouble * rightDouble;
                        break;
                    default:
                        resultDouble = leftDouble / rightDouble;
                        break;
                }

                //A float result narrows the double outcome: an out-of-range magnitude becomes INF as IEEE 754 mandates.
                //The literal constructor validates the lexical form and writes Infinity and -Infinity as the XSD forms INF and -INF
                if (targetRank == RANK_FLOAT)
                    return new RDFTypedLiteral(((float)resultDouble).ToString(CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_FLOAT);
                return new RDFTypedLiteral(resultDouble.ToString(CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
                #endregion
            }
            catch (OverflowException) { return null; }   //out-of-range decimal => no binding
        }

        /// <summary>
        /// Computes a SPARQL numeric function on the given numeric typed literal, giving a result of the base numeric type of the argument or null when it is not numeric.
        /// </summary>
        internal static RDFTypedLiteral ComputeNumericFunction(RDFTypedLiteral argument, Func<decimal, decimal> exactFunction, Func<double, double> approximateFunction)
        {
            //A non-numeric argument is a type error => no binding
            int argumentRank = GetNumericRank(argument);
            if (argumentRank == RANK_NONNUMERIC)
                return null;

            try
            {
                //Exact regime: a type derived from xsd:integer or xsd:decimal answers with its base type (xsd:int gives xsd:integer)
                if (argumentRank == RANK_INTEGER)
                    return new RDFTypedLiteral(exactFunction(GetDecimalValue(argument)).ToString(CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_INTEGER);
                if (argumentRank == RANK_DECIMAL)
                    return new RDFTypedLiteral(NormalizeDecimal(exactFunction(GetDecimalValue(argument))), RDFModelEnums.RDFDatatypes.XSD_DECIMAL);
            }
            catch (OverflowException) { return null; }   //out-of-range decimal => no binding

            //Approximate regime: INF and NaN pass through the function and the literal constructor writes them in their XSD spelling
            double resultDouble = approximateFunction(GetDoubleValue(argument));
            if (argumentRank == RANK_FLOAT)
                return new RDFTypedLiteral(((float)resultDouble).ToString(CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_FLOAT);
            return new RDFTypedLiteral(resultDouble.ToString(CultureInfo.InvariantCulture), RDFModelEnums.RDFDatatypes.XSD_DOUBLE);
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Ranks a literal in the numeric lattice, RANK_NONNUMERIC when it is not a numeric typed literal
        /// </summary>
        private static int GetNumericRank(RDFTypedLiteral typedLiteral)
        {
            if (typedLiteral == null || !typedLiteral.HasDecimalDatatype())
                return RANK_NONNUMERIC;

            switch (typedLiteral.Datatype.TargetDatatype)
            {
                case RDFModelEnums.RDFDatatypes.XSD_DECIMAL:
                case RDFModelEnums.RDFDatatypes.OWL_RATIONAL:
                    return RANK_DECIMAL;
                case RDFModelEnums.RDFDatatypes.XSD_FLOAT:
                    return RANK_FLOAT;
                case RDFModelEnums.RDFDatatypes.XSD_DOUBLE:
                case RDFModelEnums.RDFDatatypes.OWL_REAL:
                    return RANK_DOUBLE;

                //Integer family
                default:
                    return RANK_INTEGER;
            }
        }

        /// <summary>
        /// Gives the exact value of an integer or decimal numeric literal, evaluating owl:rational ("p/q") first
        /// </summary>
        private static decimal GetDecimalValue(RDFTypedLiteral typedLiteral)
            => typedLiteral.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                ? ComputeOWLRationalValue(typedLiteral)
                : decimal.Parse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture);

        /// <summary>
        /// Gives the value of a numeric literal as a double for the approximate regime, evaluating owl:rational ("p/q") first
        /// </summary>
        private static double GetDoubleValue(RDFTypedLiteral typedLiteral)
        {
            if (typedLiteral.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                return Convert.ToDouble(ComputeOWLRationalValue(typedLiteral), CultureInfo.InvariantCulture);

            //The literal was validated at creation: its lexical form (INF and -INF included) always reads back as a double
            RDFModelUtilities.TryParseFloatingPoint(typedLiteral.Value, out double typedLiteralValue);
            return typedLiteralValue;
        }

        /// <summary>
        /// Gives the canonical xsd:decimal lexical form of the given value
        /// </summary>
        private static string NormalizeDecimal(decimal value)
        {
            //Canonical form has a decimal point with at least one digit on each side and no superfluous trailing zeros (3.10 -> 3.1, 26 -> 26.0, 2.0 stays 2.0)
            string text = value.ToString(CultureInfo.InvariantCulture);
            if (text.IndexOf('.') == -1)
                return text + ".0";
            text = text.TrimEnd('0');
            return text.EndsWith(".", StringComparison.Ordinal) ? text + "0" : text;
        }
        #endregion
    }
}