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
    public class RDFConditionalExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateConditionalExpression()
        {
            RDFConditionalExpression expression = new RDFConditionalExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("?V1")),
                    new RDFConstantExpression(new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE))),
                new RDFVariableExpression(new RDFVariable("?V1")),
                new RDFVariableExpression(new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(IF((?V1 <= 45), ?V1, ?V2))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(IF((?V1 <= 45), ?V1, ?V2))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingConditionalExpressionWithExpressionBecauseNullConditionArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFConditionalExpression(
                null,
                new RDFVariableExpression(new RDFVariable("?V1")),
                new RDFVariableExpression(new RDFVariable("?V2"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingConditionalExpressionWithExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFConditionalExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("?V1")),
                    new RDFConstantExpression(new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE))),
                null,
                new RDFVariableExpression(new RDFVariable("?V2"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingConditionalExpressionWithVariableBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFConditionalExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("?V1")),
                    new RDFConstantExpression(new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE))),
                new RDFVariableExpression(new RDFVariable("?V1")),
                null));

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultLeft()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConditionalExpression expression = new RDFConditionalExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE))),
                new RDFConstantExpression(new RDFPlainLiteral("yes")),
                new RDFConstantExpression(new RDFPlainLiteral("no")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("yes")));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultRight()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConditionalExpression expression = new RDFConditionalExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE))),
                new RDFConstantExpression(new RDFPlainLiteral("yes")),
                new RDFConstantExpression(new RDFPlainLiteral("no")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("no")));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseNotBooleanCondition()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConditionalExpression expression = new RDFConditionalExpression(
                new RDFAddExpression(
                    new RDFVariable("?A"),
                    new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE)),
                new RDFConstantExpression(new RDFPlainLiteral("yes")),
                new RDFConstantExpression(new RDFPlainLiteral("no")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundCondition()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConditionalExpression expression = new RDFConditionalExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("?Q")),
                    new RDFConstantExpression(new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE))),
                new RDFConstantExpression(new RDFPlainLiteral("yes")),
                new RDFConstantExpression(new RDFPlainLiteral("no")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundLeft()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConditionalExpression expression = new RDFConditionalExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE))),
                new RDFVariableExpression(new RDFVariable("?Q")),
                new RDFConstantExpression(new RDFPlainLiteral("no")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundRight()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConditionalExpression expression = new RDFConditionalExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                    new RDFVariableExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_BYTE))),
                new RDFConstantExpression(new RDFPlainLiteral("no")),
                new RDFVariableExpression(new RDFVariable("?Q")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}