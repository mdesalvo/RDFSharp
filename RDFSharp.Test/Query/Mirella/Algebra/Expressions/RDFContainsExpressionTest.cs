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
    public class RDFContainsExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateEEContainsExpression1()
        {
            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariableExpression(new RDFVariable("?V3")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONTAINS((?V1 + ?V2), ?V3))"));
            Assert.IsTrue(expression.ToString([]).Equals("(CONTAINS((?V1 + ?V2), ?V3))"));
        }

        [TestMethod]
        public void ShouldCreateEEContainsExpression2()
        {
            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(CONTAINS((?V1 + ?V2), \"hello\"^^<{RDFVocabulary.XSD.STRING}>))"));
            Assert.IsTrue(expression.ToString([]).Equals($"(CONTAINS((?V1 + ?V2), \"hello\"^^<{RDFVocabulary.XSD.STRING}>))"));
            Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("(CONTAINS((?V1 + ?V2), \"hello\"^^xsd:string))"));
        }

        [TestMethod]
        public void ShouldCreateEEContainsExpressionNested()
        {
            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFContainsExpression(new RDFVariableExpression(new RDFVariable("?V3")), new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US"))));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONTAINS((?V1 + ?V2), (CONTAINS(?V3, \"hello\"@EN-US))))"));
            Assert.IsTrue(expression.ToString([]).Equals("(CONTAINS((?V1 + ?V2), (CONTAINS(?V3, \"hello\"@EN-US))))"));
        }

        [TestMethod]
        public void ShouldCreateEVContainsExpression()
        {
            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariable("?V3"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONTAINS((?V1 + ?V2), ?V3))"));
            Assert.IsTrue(expression.ToString([]).Equals("(CONTAINS((?V1 + ?V2), ?V3))"));
        }

        [TestMethod]
        public void ShouldCreateVEContainsExpression()
        {
            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?V3"),
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONTAINS(?V3, (?V1 + ?V2)))"));
            Assert.IsTrue(expression.ToString([]).Equals("(CONTAINS(?V3, (?V1 + ?V2)))"));
        }

        [TestMethod]
        public void ShouldCreateVVContainsExpression()
        {
            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?V3"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(CONTAINS(?V3, ?V1))"));
            Assert.IsTrue(expression.ToString([]).Equals("(CONTAINS(?V3, ?V1))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEEContainsExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFContainsExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEVContainsExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFContainsExpression(null as RDFExpression, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVEContainsExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFContainsExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVVContainsExpressionNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFContainsExpression(null as RDFVariable, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("HELLO","EN-US");
            row["?B"] = new RDFPlainLiteral("LL");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariableExpression(new RDFVariable("?A")),
                new RDFVariableExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEVAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/test1");
            row["?B"] = new RDFResource("http://example.org/");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariableExpression(new RDFVariable("?A")),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVEAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("I am ex:org literal");
            row["?B"] = new RDFResource("ex:org");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariableExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL);
            row["?B"] = new RDFTypedLiteral("ll", RDFModelEnums.RDFDatatypes.XSD_STRING);
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
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
            row["?A"] = new RDFPlainLiteral("HELO", "EN-US");
            row["?B"] = new RDFPlainLiteral("LL");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariableExpression(new RDFVariable("?A")),
                new RDFVariableExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEVAndCalculateResultFalse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://e.org/test1");
            row["?B"] = new RDFResource("http://example.org/");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariableExpression(new RDFVariable("?A")),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVEAndCalculateResultFalse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("I am ex:Org literal");
            row["?B"] = new RDFResource("ex:org");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariableExpression(new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndCalculateResultFalse()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("helo", RDFModelEnums.RDFDatatypes.RDFS_LITERAL);
            row["?B"] = new RDFTypedLiteral("ll", RDFModelEnums.RDFDatatypes.XSD_STRING);
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultEvenOnNullLeft()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.False));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultEvenOnNullRight()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello").ToString();
            row["?B"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultEvenOnNullLeftAndRight1()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndCalculateResultEvenOnNullLeftAndRight2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = RDFPlainLiteral.Empty;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFTypedLiteral.True));
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundLeft()
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

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")),
                new RDFContainsExpression(new RDFContainsExpression(new RDFVariable("?C"), new RDFVariable("?C")), new RDFVariable("?C")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseUnboundRight()
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

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFContainsExpression(new RDFContainsExpression(new RDFVariable("?C"), new RDFVariable("?C")), new RDFVariable("?C")),
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
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

            RDFContainsExpression expression = new RDFContainsExpression(
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

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariable("?Q"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseNotStringTypedLiteralLeft()
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

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?A"),
                new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionAndNotCalculateResultBecauseNotStringTypedLiteralRight()
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

            RDFContainsExpression expression = new RDFContainsExpression(
                new RDFVariable("?B"),
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}