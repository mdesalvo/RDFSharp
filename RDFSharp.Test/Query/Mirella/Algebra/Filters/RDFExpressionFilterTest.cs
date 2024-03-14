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
    public class RDFExpressionFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateExpressionFilter()
        {
            RDFExpressionFilter filter = new RDFExpressionFilter(
                new RDFBooleanAndExpression(
                    new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                        new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                        new RDFConstantExpression(new RDFTypedLiteral("24.08", RDFModelEnums.RDFDatatypes.XSD_FLOAT))),
                    new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                        new RDFVariableExpression(new RDFVariable("?V1")),
                        new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)))));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.Expression);
            Assert.IsTrue(filter.ToString().Equals("FILTER ( (((?V1 + ?V2) = 24.08) && (?V1 = \"hello\"^^<http://www.w3.org/2001/XMLSchema#string>)) )"));
            Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (((?V1 + ?V2) = 24.08) && (?V1 = \"hello\"^^xsd:string)) )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullExpression()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFExpressionFilter(null));

        [TestMethod]
        public void ShouldCreateExpressionFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFExpressionFilter filter = new RDFExpressionFilter(
                new RDFBooleanOrExpression(
                    new RDFComparisonExpression(
                        RDFQueryEnums.RDFComparisonFlavors.GreaterThan, 
                        new RDFLengthExpression(new RDFVariable("?A")),
                        new RDFConstantExpression(new RDFTypedLiteral("8", RDFModelEnums.RDFDatatypes.XSD_INT))),
                    new RDFStartsExpression(
                        new RDFVariable("?B"),
                        new RDFConstantExpression(new RDFTypedLiteral("he", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
            Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 8) || (STRSTARTS(?B, \"he\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
        }

        [TestMethod]
        public void ShouldCreateExpressionFilterAndKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFExpressionFilter filter = new RDFExpressionFilter(
                new RDFBooleanOrExpression(
                    new RDFComparisonExpression(
                        RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                        new RDFLengthExpression(new RDFVariable("?A")),
                        new RDFConstantExpression(new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_INT))),
                    new RDFStartsExpression(
                        new RDFVariable("?B"),
                        new RDFConstantExpression(new RDFTypedLiteral("pol", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
            Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 35) || (STRSTARTS(?B, \"pol\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
        }

        [TestMethod]
        public void ShouldCreateExpressionFilterAndNotKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFExpressionFilter filter = new RDFExpressionFilter(
                new RDFBooleanOrExpression(
                    new RDFComparisonExpression(
                        RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                        new RDFLengthExpression(new RDFVariable("?A")),
                        new RDFConstantExpression(new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_INT))),
                    new RDFStartsExpression(
                        new RDFVariable("?B"),
                        new RDFConstantExpression(new RDFTypedLiteral("pol", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
            Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 35) || (STRSTARTS(?B, \"pol\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
        }

        [TestMethod]
        public void ShouldCreateExpressionFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFExpressionFilter filter = new RDFExpressionFilter(
                new RDFBooleanOrExpression(
                    new RDFComparisonExpression(
                        RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                        new RDFLengthExpression(new RDFVariable("?A")),
                        new RDFConstantExpression(new RDFTypedLiteral("8", RDFModelEnums.RDFDatatypes.XSD_INT))),
                    new RDFStartsExpression(
                        new RDFVariable("?B"),
                        new RDFConstantExpression(new RDFTypedLiteral("he", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
            Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 8) || (STRSTARTS(?B, \"he\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
        }
        #endregion
    }
}