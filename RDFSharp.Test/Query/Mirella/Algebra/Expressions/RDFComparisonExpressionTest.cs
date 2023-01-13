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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFComparisonExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateEEComparisonExpressionLess()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFConstantExpression(new RDFTypedLiteral("24.08", RDFModelEnums.RDFDatatypes.XSD_FLOAT)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) < (24.08))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2) < (24.08))"));
        }

        [TestMethod]
        public void ShouldCreateEEComparisonExpressionLessOrEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFDynamicExpression(new RDFVariable("?V3")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) <= (?V3))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2) <= (?V3))"));
        }

        [TestMethod]
        public void ShouldCreateEEComparisonExpressionEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                new RDFDynamicExpression(new RDFVariable("?V1")),
                new RDFDynamicExpression(new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1) = (?V2))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1) = (?V2))"));
        }

        [TestMethod]
        public void ShouldCreateEEComparisonExpressionNotEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
                new RDFDynamicExpression(new RDFVariable("?V1")),
                new RDFDynamicExpression(new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1) != (?V2))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1) != (?V2))"));
        }

        [TestMethod]
        public void ShouldCreateEEComparisonExpressionGreaterOrEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFConstantExpression(new RDFTypedLiteral("24.08", RDFModelEnums.RDFDatatypes.XSD_FLOAT)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) >= (24.08))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2) >= (24.08))"));
        }

        [TestMethod]
        public void ShouldCreateEEComparisonExpressionGreater()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFDynamicExpression(new RDFVariable("?V3")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) > (?V3))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2) > (?V3))"));
        }

        [TestMethod]
        public void ShouldCreateEVComparisonExpressionLess()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariable("?V3"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) < ?V3)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2) < ?V3)"));
        }

        [TestMethod]
        public void ShouldCreateEVComparisonExpressionLessOrEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariable("?V3"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) <= ?V3)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2) <= ?V3)"));
        }

        [TestMethod]
        public void ShouldCreateEVComparisonExpressionEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                new RDFDynamicExpression(new RDFVariable("?V1")),
                new RDFVariable("?V2"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1) = ?V2)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1) = ?V2)"));
        }

        [TestMethod]
        public void ShouldCreateEVComparisonExpressionNotEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
                new RDFDynamicExpression(new RDFVariable("?V1")),
                new RDFVariable("?V2"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1) != ?V2)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1) != ?V2)"));
        }

        [TestMethod]
        public void ShouldCreateEVComparisonExpressionGreaterOrEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariable("?V3"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) >= ?V3)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2) >= ?V3)"));
        }

        [TestMethod]
        public void ShouldCreateEVComparisonExpressionGreater()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariable("?V3"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2) > ?V3)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2) > ?V3)"));
        }

        [TestMethod]
        public void ShouldCreateVEComparisonExpressionLess()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
                new RDFVariable("?V3"),
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V3 < (?V1 + ?V2))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V3 < (?V1 + ?V2))"));
        }

        [TestMethod]
        public void ShouldCreateVEComparisonExpressionLessOrEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                new RDFVariable("?V3"),
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V3 <= (?V1 + ?V2))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V3 <= (?V1 + ?V2))"));
        }

        [TestMethod]
        public void ShouldCreateVEComparisonExpressionEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                new RDFVariable("?V2"),
                new RDFDynamicExpression(new RDFVariable("?V1")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V2 = (?V1))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V2 = (?V1))"));
        }

        [TestMethod]
        public void ShouldCreateVEComparisonExpressionNotEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
                new RDFVariable("?V2"),
                new RDFDynamicExpression(new RDFVariable("?V1")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V2 != (?V1))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V2 != (?V1))"));
        }

        [TestMethod]
        public void ShouldCreateVEComparisonExpressionGreaterOrEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariable("?V3"),
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V3 >= (?V1 + ?V2))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V3 >= (?V1 + ?V2))"));
        }

        [TestMethod]
        public void ShouldCreateVEComparisonExpressionGreater()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                new RDFVariable("?V3"),
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V3 > (?V1 + ?V2))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V3 > (?V1 + ?V2))"));
        }

        [TestMethod]
        public void ShouldCreateVVComparisonExpressionLess()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
                new RDFVariable("?V3"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V3 < ?V1)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V3 < ?V1)"));
        }

        [TestMethod]
        public void ShouldCreateVVComparisonExpressionLessOrEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                new RDFVariable("?V3"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V3 <= ?V1)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V3 <= ?V1)"));
        }

        [TestMethod]
        public void ShouldCreateVVComparisonExpressionEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                new RDFVariable("?V2"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V2 = ?V1)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V2 = ?V1)"));
        }

        [TestMethod]
        public void ShouldCreateVVComparisonExpressionNotEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
                new RDFVariable("?V2"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V2 != ?V1)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V2 != ?V1)"));
        }

        [TestMethod]
        public void ShouldCreateVVComparisonExpressionGreaterOrEqual()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariable("?V3"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V3 >= ?V1)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V3 >= ?V1)"));
        }

        [TestMethod]
        public void ShouldCreateVVComparisonExpressionGreater()
        {
            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                new RDFVariable("?V3"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V3 > ?V1)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V3 > ?V1)"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEEComparisonExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null as RDFMathExpression, new RDFDynamicExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEVComparisonExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null as RDFMathExpression, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVEComparisonExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null as RDFVariable, new RDFDynamicExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVVComparisonExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null as RDFVariable, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEEComparisonExpressionBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFDynamicExpression(new RDFVariable("?V")), null as RDFMathExpression));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEVComparisonExpressionBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariable("?V"), null as RDFMathExpression));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVEComparisonExpressionBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFDynamicExpression(new RDFVariable("?V")), null as RDFVariable));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVVComparisonExpressionBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariable("?V"), null as RDFVariable));

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResultTrue()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                new RDFDynamicExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResultFalse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessThan,
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                new RDFDynamicExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEVAndCalculateResultTrue()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEVAndCalculateResultFalse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVEAndCalculateResultTrue()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan,
                new RDFVariable("?B"),
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVEAndCalculateResultFalse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariable("?B"),
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseTypeError()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            row["?C"] = new RDFResource("ex:subj").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                new RDFVariable("?C"),
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseBindingErrorOnExpression()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:subj").ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariable("?B"),
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseBindingErrorOnVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            row["?C"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariable("?C"),
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableInExpression()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariable("?B"),
                new RDFAddExpression(new RDFVariable("?Q"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableInLeftVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan,
                new RDFVariable("?Q"),
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableInRightVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonExpression expression = new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                new RDFVariable("?Q"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}