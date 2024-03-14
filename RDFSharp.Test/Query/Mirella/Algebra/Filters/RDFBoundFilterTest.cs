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
    public class RDFBoundFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateBoundFilter()
        {
            RDFBoundFilter filter = new RDFBoundFilter(new RDFVariable("?VAR"));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.VariableName);
            Assert.IsTrue(filter.VariableName.Equals("?VAR"));
            Assert.IsTrue(filter.ToString().Equals("FILTER ( BOUND(?VAR) )"));
            Assert.IsTrue(filter.ToString([]).Equals("FILTER ( BOUND(?VAR) )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBoundFilterBecauseNullVariable()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBoundFilter(null));

        [TestMethod]
        public void ShouldCreateBoundFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBoundFilter filter = new RDFBoundFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }
        
        [TestMethod]
        public void ShouldCreateBoundFilterAndKeepRowWithEmptyVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = string.Empty;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBoundFilter filter = new RDFBoundFilter(new RDFVariable("?Q"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBoundFilterAndKeepRowWithUnknownVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBoundFilter filter = new RDFBoundFilter(new RDFVariable("?Q"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBoundFilterAndKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBoundFilter filter = new RDFBoundFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBoundFilterAndNotKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBoundFilter filter = new RDFBoundFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBoundFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBoundFilter filter = new RDFBoundFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }
        #endregion
    }
}