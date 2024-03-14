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
    public class RDFComparisonFilterTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.LessThan)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.EqualTo)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.GreaterThan)]
        public void ShouldCreateComparisonFilter(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor)
        {
            RDFComparisonFilter filter = new RDFComparisonFilter(comparisonFlavor, new RDFVariable("?LEFT"), RDFVocabulary.RDF.ALT);

            Assert.IsNotNull(filter);
            Assert.IsTrue(filter.ComparisonFlavor == comparisonFlavor);
            Assert.IsNotNull(filter.LeftMember);
            Assert.IsTrue(filter.LeftMember.Equals(new RDFVariable("?LEFT")));
            Assert.IsNotNull(filter.LeftMemberString);
            Assert.IsTrue(filter.LeftMemberString.Equals("?LEFT"));
            Assert.IsNotNull(filter.RightMember);
            Assert.IsTrue(filter.RightMember.Equals(RDFVocabulary.RDF.ALT));
            Assert.IsNotNull(filter.RightMemberString);
            Assert.IsTrue(filter.RightMemberString.Equals(RDFVocabulary.RDF.ALT.ToString()));

            List<RDFNamespace> namespaces = [RDFNamespaceRegister.GetByPrefix("rdf")];
            string leftValue = RDFQueryPrinter.PrintPatternMember(filter.LeftMember, null);
            string leftValueNS = RDFQueryPrinter.PrintPatternMember(filter.LeftMember, namespaces);
            string rightValue = RDFQueryPrinter.PrintPatternMember(filter.RightMember, null);
            string rightValueNS = RDFQueryPrinter.PrintPatternMember(filter.RightMember, namespaces);
            switch (comparisonFlavor)
            {
                case RDFQueryEnums.RDFComparisonFlavors.LessThan:
                    Assert.IsTrue(filter.ToString().Equals($"FILTER ( {leftValue} < {rightValue} )"));
                    Assert.IsTrue(filter.ToString(namespaces).Equals($"FILTER ( {leftValueNS} < {rightValueNS} )"));
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan:
                    Assert.IsTrue(filter.ToString().Equals($"FILTER ( {leftValue} <= {rightValue} )"));
                    Assert.IsTrue(filter.ToString(namespaces).Equals($"FILTER ( {leftValueNS} <= {rightValueNS} )"));
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.EqualTo:
                    Assert.IsTrue(filter.ToString().Equals($"FILTER ( {leftValue} = {rightValue} )"));
                    Assert.IsTrue(filter.ToString(namespaces).Equals($"FILTER ( {leftValueNS} = {rightValueNS} )"));
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.NotEqualTo:
                    Assert.IsTrue(filter.ToString().Equals($"FILTER ( {leftValue} != {rightValue} )"));
                    Assert.IsTrue(filter.ToString(namespaces).Equals($"FILTER ( {leftValueNS} != {rightValueNS} )"));
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan:
                    Assert.IsTrue(filter.ToString().Equals($"FILTER ( {leftValue} >= {rightValue} )"));
                    Assert.IsTrue(filter.ToString(namespaces).Equals($"FILTER ( {leftValueNS} >= {rightValueNS} )"));
                    break;
                case RDFQueryEnums.RDFComparisonFlavors.GreaterThan:
                    Assert.IsTrue(filter.ToString().Equals($"FILTER ( {leftValue} > {rightValue} )"));
                    Assert.IsTrue(filter.ToString(namespaces).Equals($"FILTER ( {leftValueNS} > {rightValueNS} )"));
                    break;
            }
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingComparisonFilterBecauseNullLeftMember()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.EqualTo, null, new RDFVariable("?RIGHT")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingComparisonFilterBecauseNullRightMember()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariable("?LEFT"), null));
        
        [TestMethod]
        public void ShouldCreateComparisonFilterWithVariableVsLiteralAndKeepRow1()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFVariable("?A"), new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_DECIMAL));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithVariableVsLiteralAndKeepRow2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariable("?A"), new RDFTypedLiteral("27.70", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithVariableVsLiteralAndKeepRow3()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo, new RDFVariable("?A"), new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_DOUBLE));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithVariableVsLiteralAndNotKeepRowBecauseUnknownVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFVariable("?Q"), new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_DECIMAL));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithLiteralVsVariableAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.LessThan,  new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_DECIMAL), new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithVariableVsVariableAndKeepRow1()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan, new RDFVariable("?A"), new RDFVariable("?B"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithVariableVsVariableAndKeepRow2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFVariable("?B"), new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithLiteralVsVariableAndNotKeepRowBecauseUnknownVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.LessThan, new RDFTypedLiteral("12", RDFModelEnums.RDFDatatypes.XSD_DECIMAL), new RDFVariable("?Q"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithVariableVsVariableAndNotKeepRowBecauseTypeError()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFTypedLiteral("---30", RDFModelEnums.RDFDatatypes.XSD_GDAY).ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.LessThan, new RDFVariable("?A"), new RDFVariable("?B"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithResourceVsResourceAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:novo").ToString();
            row["?B"] = new RDFResource("ex:novum").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo, new RDFVariable("?A"), new RDFVariable("?B"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithResourceVsResourceAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:novo").ToString();
            row["?B"] = new RDFResource("ex:novum").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo, new RDFVariable("?A"), new RDFVariable("?B"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateComparisonFilterWithEmptyPlainLiteralVsNullAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral(string.Empty).ToString();
            row["?B"] = null;
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFComparisonFilter filter = new RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariable("?A"), new RDFVariable("?B"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }
        #endregion
    }
}