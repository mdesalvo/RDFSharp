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
    public class RDFCoalesceExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateEECoalesceExpression1()
        {
            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariableExpression(new RDFVariable("?V3")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(COALESCE((?V1 + ?V2), ?V3))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(COALESCE((?V1 + ?V2), ?V3))"));
        }

        [TestMethod]
        public void ShouldCreateEECoalesceExpression2()
        {
            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(COALESCE((?V1 + ?V2), \"hello\"^^<{RDFVocabulary.XSD.STRING}>))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals($"(COALESCE((?V1 + ?V2), \"hello\"^^<{RDFVocabulary.XSD.STRING}>))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals("(COALESCE((?V1 + ?V2), \"hello\"^^xsd:string))"));
        }

        [TestMethod]
        public void ShouldCreateEECoalesceExpressionNested()
        {
            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFCoalesceExpression(new RDFVariableExpression(new RDFVariable("?V3")), new RDFConstantExpression(new RDFPlainLiteral("hello","EN-US"))));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(COALESCE((?V1 + ?V2), (COALESCE(?V3, \"hello\"@EN-US))))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(COALESCE((?V1 + ?V2), (COALESCE(?V3, \"hello\"@EN-US))))"));
        }

        [TestMethod]
        public void ShouldCreateEVCoalesceExpression()
        {
            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                new RDFVariable("?V3"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(COALESCE((?V1 + ?V2), ?V3))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(COALESCE((?V1 + ?V2), ?V3))"));
        }

        [TestMethod]
        public void ShouldCreateVECoalesceExpression()
        {
            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFVariable("?V3"),
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(COALESCE(?V3, (?V1 + ?V2)))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(COALESCE(?V3, (?V1 + ?V2)))"));
        }

        [TestMethod]
        public void ShouldCreateVVCoalesceExpression()
        {
            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFVariable("?V3"),
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(COALESCE(?V3, ?V1))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(COALESCE(?V3, ?V1))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEECoalesceExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFCoalesceExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingEVCoalesceExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFCoalesceExpression(null as RDFExpression, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVECoalesceExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFCoalesceExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V"))));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVVCoalesceExpressionNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFCoalesceExpression(null as RDFVariable, new RDFVariable("?V")));

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResultCoalesceLeft()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            row["?B"] = new RDFPlainLiteral("hello");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFVariableExpression(new RDFVariable("?A")),
                new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResultCoalesceRight()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            row["?B"] = new RDFPlainLiteral("hello");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?B")),
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithEEAndCalculateResultCoalesceNull()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            row["?B"] = new RDFPlainLiteral("hello");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?A")),
                new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?B")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndCalculateResultCoalesceLeft()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFVariable("?A"),
                new RDFVariable("?Q"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndCalculateResultCoalesceRight()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFVariable("?Q"),
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVVAndCalculateResultCoalesceNull()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFVariable("?Q"),
                new RDFVariable("?W"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithNestedCoalesce()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            row["?B"] = new RDFPlainLiteral("hello");
            row["?C"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFCoalesceExpression(
                    new RDFSubstringExpression(new RDFVariable("?C"), 1),
                    new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?B"))),
                new RDFCoalesceExpression(
                    new RDFVariableExpression(new RDFVariable("?A")),
                    new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?B"))));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithNestedCoalesceNull()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            row["?B"] = new RDFPlainLiteral("hello");
            row["?C"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFCoalesceExpression(
                    new RDFSubstringExpression(new RDFVariable("?C"), 1),
                    new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?B"))),
                new RDFCoalesceExpression(
                    new RDFVariableExpression(new RDFVariable("?Q")),
                    new RDFVariable("?Y")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithDeepNestedCoalesce()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            row["?B"] = new RDFPlainLiteral("hello");
            row["?C"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFCoalesceExpression(
                    new RDFCoalesceExpression(
                        new RDFSubstringExpression(new RDFVariable("?C"), 1),
                        new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?B"))),
                    new RDFCoalesceExpression(
                        new RDFVariableExpression(new RDFVariable("?T")),
                        new RDFAddExpression(new RDFVariable("?C"), new RDFVariable("?B")))),
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithDeepNestedCoalesceNull()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/");
            row["?B"] = new RDFPlainLiteral("hello");
            row["?C"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFCoalesceExpression expression = new RDFCoalesceExpression(
                new RDFCoalesceExpression(
                    new RDFCoalesceExpression(
                        new RDFSubstringExpression(new RDFVariable("?C"), 1),
                        new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)), new RDFVariable("?B"))),
                    new RDFCoalesceExpression(
                        new RDFVariableExpression(new RDFVariable("?T")),
                        new RDFAddExpression(new RDFVariable("?C"), new RDFVariable("?B")))),
                new RDFVariable("?Z"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}