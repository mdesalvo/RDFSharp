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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFSubstringExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateSubstringExpressionWithExpressionAndStart()
        {
            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?V")), 5);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5))"));
            Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5))"));
        }

        [TestMethod]
        public void ShouldCreateSubstringExpressionWithExpressionAndStartAndLength()
        {
            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?V")), 5, 2);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5, 2))"));
            Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5, 2))"));
        }

        [TestMethod]
        public void ShouldCreateSubstringExpressionWithExpressionAndStartAndLengthNull()
        {
            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?V")), 5, null);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5))"));
            Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5))"));
        }

        [TestMethod]
        public void ShouldCreateSubstringExpressionWithVariableAndStart()
        {
            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?V"), 5);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5))"));
            Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5))"));
        }

        [TestMethod]
        public void ShouldCreateSubstringExpressionWithVariableAndStartAndLength()
        {
            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?V"), 5, 2);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5, 2))"));
            Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5, 2))"));
        }

        [TestMethod]
        public void ShouldCreateSubstringExpressionWithVariableAndStartAndLengthNull()
        {
            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?V"), 5, null);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(SUBSTRING(?V, 5))"));
            Assert.IsTrue(expression.ToString([]).Equals("(SUBSTRING(?V, 5))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingESubstringExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFSubstringExpression(null as RDFExpression, 5));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVSubstringExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFSubstringExpression(null as RDFVariable, 5));

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ttp://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithNestedExpressionAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFSubstringExpression(new RDFVariableExpression(new RDFVariable("?A")), 2), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("tp://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2, 7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ttp://e")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2, 7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndNegativeLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExactEndingStartAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 19);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingStartAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 20);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingStartAndLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 20, 4);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingStartAndExceedingLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 20, 44);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingStartAndNegativeLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 20, -4);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExactEndingLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1, 19);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1, 20);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNull()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseUnboundColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?A")), 1);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseUnknownColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFAddExpression(new RDFVariable("?Q"), new RDFVariable("?A")), 1);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotStringTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndNegativeLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExactEndingStartAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingStartAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExactEndingLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1, 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1, 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndNegativeLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExactEndingStartAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingStartAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExactEndingLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1, 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1, 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNegativeStartAndNegativeLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), -2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExactEndingStartAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingStartAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExactEndingLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1, 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndExceedingLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariableExpression(new RDFVariable("?A")), 1, 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ttp://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2, 7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ttp://e")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2, 7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndNegativeLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExactEndingStartAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 19);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingStartAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 20);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingStartAndLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 20, 4);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingStartAndExceedingLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 20, 44);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingStartAndNegativeLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 20, -4);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExactEndingLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1, 19);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingLengthAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1, 20);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndCalculateResultOnNull()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseUnknownColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?Q"), 1);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotStringTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndNegativeLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExactEndingStartAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingStartAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExactEndingLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1, 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingLengthAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1, 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndNegativeLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExactEndingStartAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingStartAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral(string.Empty, "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExactEndingLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1, 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingLengthAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1, 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello", "en-US")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("ell")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2, 3);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hel")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNegativeStartAndNegativeLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), -2, -7);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExactEndingStartAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("o")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingStartAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExactEndingLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1, 5);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndExceedingLengthAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSubstringExpression expression = new RDFSubstringExpression(
                new RDFVariable("?A"), 1, 6);
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("hello")));
        }
        #endregion
    }
}