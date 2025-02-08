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
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFLowerCaseExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateLCaseExpressionWithExpression()
        {
            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(LCASE((?V1 + ?V2)))"));
            Assert.IsTrue(expression.ToString([]).Equals("(LCASE((?V1 + ?V2)))"));
        }

        [TestMethod]
        public void ShouldCreateLCaseExpressionWithVariable()
        {
            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(LCASE(?V1))"));
            Assert.IsTrue(expression.ToString([]).Equals("(LCASE(?V1))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingLCaseExpressionWithExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFLowerCaseExpression(null as RDFExpression));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingLCaseExpressionWithVariableBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFLowerCaseExpression(null as RDFVariable));

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNull()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("heLLo").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariableExpression(new RDFVariable("?A")));
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
            row["?A"] = new RDFPlainLiteral("heLLo","en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariableExpression(new RDFVariable("?A")));
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
            row["?A"] = new RDFTypedLiteral("hEllo", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:subj").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotStringTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseNotBoundVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariableExpression(new RDFVariable("?Q")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hEllo").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariable("?A"));
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
            row["?A"] = new RDFPlainLiteral("hELlo", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariable("?A"));
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
            row["?A"] = new RDFTypedLiteral("heLlo", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotStringExpression()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:subj").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotStringTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndNotCalculateResultBecauseNotBoundVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLowerCaseExpression expression = new RDFLowerCaseExpression(
                new RDFVariable("?Q"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}