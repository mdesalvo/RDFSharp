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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFArithmeticEngineTest
{
    #region Tests
    [TestMethod]
    //L=integer
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '+', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "10", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '-', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '*', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "24", RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '/', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "1.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '+', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "10.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '-', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "2.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '*', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "24.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '/', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "1.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '+', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "10", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '-', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '*', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "24", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '/', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "1.5", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '+', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '-', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '*', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '/', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "1.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    //L=decimal
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '+', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "10.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '-', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "2.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '*', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "24.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '/', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "1.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '+', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "10.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '-', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "2.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '*', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "24.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '/', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "1.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '+', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "10", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '-', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '*', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "24", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '/', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "1.5", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '+', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '-', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '*', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '/', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "1.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    //L=float
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '+', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "10", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '-', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '*', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "24", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '/', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "1.5", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '+', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "10", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '-', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '*', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "24", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '/', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "1.5", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '+', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "10", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '-', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "2", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '*', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "24", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '/', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "1.5", RDFModelEnums.RDFDatatypes.XSD_FLOAT)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '+', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '-', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '*', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '/', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "1.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    //L=double
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '+', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '-', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '*', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '/', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "1.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '+', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '-', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '*', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '/', "4", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "1.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '+', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '-', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '*', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '/', "4", RDFModelEnums.RDFDatatypes.XSD_FLOAT, "1.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '+', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '-', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "2", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '*', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "24", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '/', "4", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "1.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)]
    public void ShouldComputeNumericArithmeticMatrix(string leftValue, RDFModelEnums.RDFDatatypes leftType, char op, string rightValue, RDFModelEnums.RDFDatatypes rightType, string expectedValue, RDFModelEnums.RDFDatatypes expectedType)
    {
        RDFTypedLiteral result = RDFArithmeticEngine.ComputeNumericOperation(new RDFTypedLiteral(leftValue, leftType), new RDFTypedLiteral(rightValue, rightType), op);
        Assert.IsNotNull(result);
        Assert.AreEqual(new RDFTypedLiteral(expectedValue, expectedType).ToString(), result.ToString());
    }

    //Exactness (no floating-point noise) and canonical xsd:decimal/integer lexical forms
    [TestMethod]
    [DataRow("3.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '+', "0.1", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "3.1", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)] //NOT 3.1000000000000001
    [DataRow("3.10", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '*', "1", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "3.1", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)] //trailing zero stripped
    [DataRow("13", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '+', "13", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, "26.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)] //integral decimal keeps ".0"
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '/', "3", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "2.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)] //int/int -> decimal "2.0"
    [DataRow("1000", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '+', "0", RDFModelEnums.RDFDatatypes.XSD_INTEGER, "1000", RDFModelEnums.RDFDatatypes.XSD_INTEGER)] //integer never scientific
    [DataRow("1/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL, '+', "1/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL, "1.0", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)] //owl:rational -> exact decimal
    [DataRow("1/2", RDFModelEnums.RDFDatatypes.OWL_RATIONAL, '+', "0.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, "1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)] //owl:rational mixed with double
    [DataRow("6", RDFModelEnums.RDFDatatypes.OWL_REAL, '+', "4", RDFModelEnums.RDFDatatypes.OWL_REAL, "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)] //owl:real -> double
    public void ShouldComputeNumericArithmeticCanonical(string leftValue, RDFModelEnums.RDFDatatypes leftType, char op, string rightValue, RDFModelEnums.RDFDatatypes rightType, string expectedValue, RDFModelEnums.RDFDatatypes expectedType)
    {
        RDFTypedLiteral result = RDFArithmeticEngine.ComputeNumericOperation(new RDFTypedLiteral(leftValue, leftType), new RDFTypedLiteral(rightValue, rightType), op);
        Assert.IsNotNull(result);
        Assert.AreEqual(new RDFTypedLiteral(expectedValue, expectedType).ToString(), result.ToString());
    }

    //Integer/integer division promotes to decimal for EVERY integer-family member (covers GetNumericRank default)
    [TestMethod]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_INTEGER)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_LONG)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_INT)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_SHORT)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_BYTE)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)]
    public void ShouldPromoteIntegerFamilyDivisionToDecimal(RDFModelEnums.RDFDatatypes integerType)
    {
        RDFTypedLiteral result = RDFArithmeticEngine.ComputeNumericOperation(new RDFTypedLiteral("7", integerType), new RDFTypedLiteral("2", integerType), '/');
        Assert.IsNotNull(result);
        Assert.AreEqual(new RDFTypedLiteral("3.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString(), result.ToString());
    }

    //Negative integer-family members (value 7 would be illegal) divide to decimal too
    [TestMethod]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)]
    [DataRow(RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)]
    public void ShouldPromoteNegativeIntegerFamilyDivisionToDecimal(RDFModelEnums.RDFDatatypes integerType)
    {
        RDFTypedLiteral result = RDFArithmeticEngine.ComputeNumericOperation(new RDFTypedLiteral("-7", integerType), new RDFTypedLiteral("-2", integerType), '/');
        Assert.IsNotNull(result);
        Assert.AreEqual(new RDFTypedLiteral("3.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString(), result.ToString());
    }

    //Guards: every error condition yields a null result (the caller treats it as "no binding")
    [TestMethod]
    [DataRow("hello", RDFModelEnums.RDFDatatypes.XSD_STRING, '+', "4", RDFModelEnums.RDFDatatypes.XSD_INTEGER)] //non-numeric LEFT
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '+', "true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)] //non-numeric RIGHT
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER, '/', "0", RDFModelEnums.RDFDatatypes.XSD_INTEGER)] //decimal division by zero
    [DataRow("6", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '/', "0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)] //double division by zero
    [DataRow("79228162514264337593543950335", RDFModelEnums.RDFDatatypes.XSD_DECIMAL, '*', "2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)] //decimal overflow
    [DataRow("1E308", RDFModelEnums.RDFDatatypes.XSD_DOUBLE, '*', "10", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)] //double overflow -> Infinity
    [DataRow("1E38", RDFModelEnums.RDFDatatypes.XSD_FLOAT, '*', "100", RDFModelEnums.RDFDatatypes.XSD_FLOAT)] //float downcast -> Infinity
    public void ShouldComputeNumericArithmeticReturningNull(string leftValue, RDFModelEnums.RDFDatatypes leftType, char op, string rightValue, RDFModelEnums.RDFDatatypes rightType)
        => Assert.IsNull(RDFArithmeticEngine.ComputeNumericOperation(new RDFTypedLiteral(leftValue, leftType), new RDFTypedLiteral(rightValue, rightType), op));

    //A null operand is a type error => null result
    [TestMethod]
    public void ShouldComputeNumericArithmeticReturningNullOnNullOperands()
    {
        RDFTypedLiteral six = new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
        Assert.IsNull(RDFArithmeticEngine.ComputeNumericOperation(null, six, '+'));
        Assert.IsNull(RDFArithmeticEngine.ComputeNumericOperation(six, null, '+'));
        Assert.IsNull(RDFArithmeticEngine.ComputeNumericOperation(null, null, '+'));
    }

    //owl:rational "p/q" evaluates to the exact decimal p/q
    [TestMethod]
    [DataRow("10/2", 5.0)]
    [DataRow("7/2", 3.5)]
    [DataRow("1/4", 0.25)]
    [DataRow("-9/4", -2.25)]
    public void ShouldComputeOWLRationalValue(string lexical, double expected)
        => Assert.AreEqual((decimal)expected, RDFArithmeticEngine.ComputeOWLRationalValue(new RDFTypedLiteral(lexical, RDFModelEnums.RDFDatatypes.OWL_RATIONAL)));
    #endregion
}
