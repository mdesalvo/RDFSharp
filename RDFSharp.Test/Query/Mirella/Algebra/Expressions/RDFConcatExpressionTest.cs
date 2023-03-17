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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFConcatExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateEEConcatExpression1()
        {
            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariableExpression(new RDFVariable("?V3")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONCAT((?V1 + ?V2), ?V3))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(CONCAT((?V1 + ?V2), ?V3))"));
        }

        [TestMethod]
        public void ShouldCreateEEConcatExpression2()
        {
            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFConstantExpression(new RDFTypedLiteral("25.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONCAT((?V1 + ?V2), 25))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(CONCAT((?V1 + ?V2), 25))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals("(CONCAT((?V1 + ?V2), 25))"));
        }

        [TestMethod]
        public void ShouldCreateEEConcatExpressionNested()
        {
            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?V3")), new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US"))));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONCAT((?V1 + ?V2), (CONCAT(?V3, \"hello\"@EN-US))))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(CONCAT((?V1 + ?V2), (CONCAT(?V3, \"hello\"@EN-US))))"));
        }

        [TestMethod]
        public void ShouldCreateEVConcatExpression()
        {
            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariable("?V3"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONCAT((?V1 + ?V2), ?V3))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(CONCAT((?V1 + ?V2), ?V3))"));
        }

        [TestMethod]
        public void ShouldCreateVEConcatExpression()
        {
            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?V3"),
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONCAT(?V3, (?V1 + ?V2)))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(CONCAT(?V3, (?V1 + ?V2)))"));
        }

        [TestMethod]
        public void ShouldCreateVVConcatExpression()
        {
            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?V3"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONCAT(?V3, ?V1))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(CONCAT(?V3, ?V1))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEEConcatExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFConcatExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEVConcatExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFConcatExpression(null as RDFExpression, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVEConcatExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFConcatExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVVConcatExpressionNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFConcatExpression(null as RDFVariable, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("A");
            row["?B"] = new RDFPlainLiteral("B");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariableExpression(new RDFVariable("?A")),
                new RDFVariableExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("AB")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEVAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("A");
            row["?B"] = new RDFPlainLiteral("B");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariableExpression(new RDFVariable("?A")),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("AB")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("A");
            row["?B"] = new RDFPlainLiteral("B");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?A"),
                new RDFVariableExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("AB")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("A");
            row["?B"] = new RDFPlainLiteral("B");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("AB")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVV2AndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("---13", RDFModelEnums.RDFDatatypes.XSD_GDAY);
            row["?B"] = new RDFPlainLiteral("B");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("---13ZB")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVV3AndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            row["?B"] = new RDFPlainLiteral("B");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/B")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVV4AndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("B");
            row["?B"] = new RDFResource("http://example.org/");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("Bhttp://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateComplexResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?V1", typeof(string));
            table.Columns.Add("?V2", typeof(string));
            table.Columns.Add("?V3", typeof(string));
            DataRow row = table.NewRow();
            row["?V1"] = new RDFTypedLiteral("56", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
            row["?V2"] = new RDFTypedLiteral("4.2", RDFModelEnums.RDFDatatypes.XSD_FLOAT);
            row["?V3"] = new RDFResource("ex:org");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFConcatExpression(
                    new RDFVariableExpression(new RDFVariable("?V3")),
                    new RDFConstantExpression(new RDFPlainLiteral("hello","en-US"))));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("60.2ex:orghello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultAllEmpties()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?V1", typeof(string));
            table.Columns.Add("?V2", typeof(string));
            DataRow row = table.NewRow();
            row["?V1"] = null;
            row["?V2"] = new RDFTypedLiteral("4.2", RDFModelEnums.RDFDatatypes.XSD_FLOAT);
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty)));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultOnUnboundLeft()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT);
            row["?B"] = new RDFPlainLiteral("B");
            row["?C"] = new RDFPlainLiteral("C");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                new RDFConcatExpression(new RDFConcatExpression(new RDFVariable("?C"), new RDFVariable("?C")), new RDFVariable("?C")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("CCC")));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultOnUnboundRight()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT);
            row["?B"] = new RDFPlainLiteral("B");
            row["?C"] = new RDFPlainLiteral("C");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFConcatExpression(new RDFConcatExpression(new RDFVariable("?C"), new RDFVariable("?C")), new RDFVariable("?C")),
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("CCC")));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableLeft()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT);
            row["?B"] = new RDFPlainLiteral("B");
            row["?C"] = new RDFPlainLiteral("C");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?Q"),
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnknownVariableRight()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_FLOAT);
            row["?B"] = new RDFPlainLiteral("B");
            row["?C"] = new RDFPlainLiteral("C");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFConcatExpression expression = new RDFConcatExpression(
                new RDFVariable("?A"),
                new RDFVariable("?Q"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}