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
using System;
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;
using System.Globalization;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFAddExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateAddExpressionWithVariables()
        {
            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?L"), new RDFVariable("?R"));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.IsUnary);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsTrue(expression.LeftArgument.Equals(new RDFVariable("?L")));
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.RightArgument.Equals(new RDFVariable("?R")));
            Assert.IsTrue(expression.ToString().Equals($"(?L + ?R)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals($"(?L + ?R)"));
        }

        [TestMethod]
        public void ShouldCreateAddExpressionWithVariableAndTypedLiteral()
        {
            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?L"), new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));

            Assert.IsNotNull(expression);
            Assert.IsFalse(expression.IsUnary);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsTrue(expression.LeftArgument.Equals(new RDFVariable("?L")));
            Assert.IsNotNull(expression.RightArgument);
            Assert.IsTrue(expression.RightArgument.Equals(new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
            Assert.IsTrue(expression.ToString().Equals($"(?L + {25.1d.ToString(CultureInfo.InvariantCulture)})"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals($"(?L + {25.1d.ToString(CultureInfo.InvariantCulture)})"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAddExpressionWithVariablesBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFAddExpression(null, new RDFVariable("?R")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAddExpressionWithVariableAndTypedLiteralBecauseNullLeftArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFAddExpression(null, new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAddExpressionWithVariablesBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFAddExpression(new RDFVariable("?L"), null as RDFVariable));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAddExpressionWithVariableandTypedLiteralBecauseNullRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFAddExpression(new RDFVariable("?L"), null as RDFTypedLiteral));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAddExpressionWithVariableandTypedLiteralBecauseNonNumericRightArgument()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFAddExpression(new RDFVariable("?L"), new RDFTypedLiteral("23", RDFModelEnums.RDFDatatypes.XSD_GDAY)));

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndCalculateResult()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNotNull(expressionResult);
            Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("30.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)));
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecauseUnknownLeftColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?Z"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecauseUnknownRightColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?Z"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecauseResourceLeftColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:subj").ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecausePlainLiteralLeftColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("55").ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecauseTypedLiteralNotNumericLeftColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("05", RDFModelEnums.RDFDatatypes.XSD_GDAY).ToString();
            row["?B"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecauseResourceRightColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(); 
            row["?B"] = new RDFResource("ex:subj").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecausePlainLiteralRightColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("55").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecauseTypedLiteralNotNumericRightColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            row["?B"] = new RDFTypedLiteral("05", RDFModelEnums.RDFDatatypes.XSD_GDAY).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecauseNullLeftColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFTypedLiteral("25.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }

        [TestMethod]
        public void ShouldApplyExpressionWithVariablesAndNotCalculateResultBecauseNullRightColumn()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString();
            row["?B"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFAddExpression expression = new RDFAddExpression(new RDFVariable("?A"), new RDFVariable("?B"));
            RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

            Assert.IsNull(expressionResult);
        }
        #endregion
    }
}