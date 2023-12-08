/*
   Copyright 2012-2024 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFBooleanOrExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateBooleanOrExpression()
        {
            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                    new RDFConstantExpression(new RDFTypedLiteral("24.08", RDFModelEnums.RDFDatatypes.XSD_FLOAT))),
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFVariableExpression(new RDFVariable("?V1")),
                    new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING))));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(((?V1 + ?V2) = 24.08) || (?V1 = \"hello\"^^<http://www.w3.org/2001/XMLSchema#string>))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(((?V1 + ?V2) = 24.08) || (?V1 = \"hello\"^^<http://www.w3.org/2001/XMLSchema#string>))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals("(((?V1 + ?V2) = 24.08) || (?V1 = \"hello\"^^xsd:string))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBooleanOrExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanOrExpression(null, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBooleanOrExpressionBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanOrExpression(new RDFVariableExpression(new RDFVariable("?V")), null));

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultTrueBoth()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("30.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFConstantExpression(RDFTypedLiteral.True));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }


        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultTrueLeft()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("30.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFConstantExpression(RDFTypedLiteral.False));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultTrueRight()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("70.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFConstantExpression(RDFTypedLiteral.True));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultTrue2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            row["?C"] = RDFTypedLiteral.True.ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("80.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFVariableExpression(new RDFVariable("?C")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultFalse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("80.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFConstantExpression(RDFTypedLiteral.False));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultFalse2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            row["?C"] = RDFTypedLiteral.False.ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("70.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFVariableExpression(new RDFVariable("?C")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseNotBooleanLeftResponse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFConstantExpression(RDFTypedLiteral.True));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseNotBooleanRightResponse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("70.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFVariableExpression(new RDFVariable("?Q")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseNotBooleanRightResponse2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanOrExpression expression = new RDFBooleanOrExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                    new RDFConstantExpression(new RDFTypedLiteral("80.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))),
                new RDFVariableExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}