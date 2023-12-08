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
    public class RDFSHA256ExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateSHA256ExpressionWithExpression()
        {
            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(SHA256((?V1 + ?V2)))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(SHA256((?V1 + ?V2)))"));
        }

        [TestMethod]
        public void ShouldCreateSHA256ExpressionWithVariable()
        {
            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariable("?V1"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(SHA256(?V1))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(SHA256(?V1))"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingSHA256ExpressionWithExpressionBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFSHA256Expression(null as RDFExpression));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingSHA256ExpressionWithVariableBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFSHA256Expression(null as RDFVariable));

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNull()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")));
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

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiteralWithLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("hello","en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:subj").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariableExpression(new RDFVariable("?A")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("f1b58b3117fa075d0207a55da73b3c5f5aed7ce995b9a3a34961907555349563")));
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

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariableExpression(new RDFVariable("?Q")));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithExpressionAndNotCalculateResultBecauseBindingError()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("45", RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFAddExpression(new RDFVariable("?A"), new RDFConstantExpression(new RDFTypedLiteral("10", RDFModelEnums.RDFDatatypes.XSD_INT))));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
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

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")));
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

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")));
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

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndCalculateResultOnTypedLiteral()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.RDFS_LITERAL).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariableAndCalculateResultOnResource()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:subj").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariable("?A"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("f1b58b3117fa075d0207a55da73b3c5f5aed7ce995b9a3a34961907555349563")));
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

            RDFSHA256Expression expression = new RDFSHA256Expression(
                new RDFVariable("?Q"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}