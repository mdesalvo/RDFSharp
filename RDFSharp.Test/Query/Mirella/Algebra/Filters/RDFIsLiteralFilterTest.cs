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
    public class RDFIsLiteralFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateIsLiteralFilter()
        {
            RDFIsLiteralFilter filter = new RDFIsLiteralFilter(new RDFVariable("?VAR"));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.VariableName);
            Assert.IsTrue(filter.VariableName.Equals("?VAR"));
            Assert.IsTrue(filter.ToString().Equals("FILTER ( ISLITERAL(?VAR) )"));
            Assert.IsTrue(filter.ToString([]).Equals("FILTER ( ISLITERAL(?VAR) )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingIsLiteralFilterBecauseNullVariable()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFIsLiteralFilter(null));

        [TestMethod]
        public void ShouldCreateIsLiteralFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFPlainLiteral("Hi!").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsLiteralFilter filter = new RDFIsLiteralFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsLiteralFilterAndKeepRowWithNullVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsLiteralFilter filter = new RDFIsLiteralFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsLiteralFilterAndKeepRowWithUnknownVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsLiteralFilter filter = new RDFIsLiteralFilter(new RDFVariable("?Q"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsLiteralFilterAndKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource().ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsLiteralFilter filter = new RDFIsLiteralFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsLiteralFilterAndNotKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("ex:novo").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsLiteralFilter filter = new RDFIsLiteralFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsLiteralFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsLiteralFilter filter = new RDFIsLiteralFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsLiteralFilterAndNotKeepRowWithNullVariableBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsLiteralFilter filter = new RDFIsLiteralFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }
        #endregion
    }
}
