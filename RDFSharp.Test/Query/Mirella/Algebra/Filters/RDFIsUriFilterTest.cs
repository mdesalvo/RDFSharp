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
    public class RDFIsUriFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateIsUriFilter()
        {
            RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("?VAR"));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.VariableName);
            Assert.IsTrue(filter.VariableName.Equals("?VAR"));
            Assert.IsTrue(filter.ToString().Equals("FILTER ( ISURI(?VAR) )"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { }).Equals("FILTER ( ISURI(?VAR) )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingIsUriFilterBecauseNullVariable()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFIsUriFilter(null));

        [TestMethod]
        public void ShouldCreateIsUriFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }
        
        [TestMethod]
        public void ShouldCreateIsUriFilterAndKeepRowWithUnknownVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("?Q"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsUriFilterAndKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsUriFilterAndNotKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsUriFilterAndNotKeepRowWithNullVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateIsUriFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFIsUriFilter filter = new RDFIsUriFilter(new RDFVariable("?A"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }
        #endregion
    }
}