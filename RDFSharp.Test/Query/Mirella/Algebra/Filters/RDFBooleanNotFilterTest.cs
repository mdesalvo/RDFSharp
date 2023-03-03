/*
   Copyright 2012-2023 Marco De Salvo

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
    public class RDFBooleanNotFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateBooleanNotFilter()
        {
            RDFBooleanNotFilter filter = new RDFBooleanNotFilter(new RDFIsUriFilter(new RDFVariable("?VARU")));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.Filter);
            Assert.IsTrue(filter.ToString().Equals($"FILTER ( !( ISURI(?VARU) ) )"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals("FILTER ( !( ISURI(?VARU) ) )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldCreateNestedBooleanNotFilter()
        {
            RDFBooleanNotFilter filterA = new RDFBooleanNotFilter(new RDFIsUriFilter(new RDFVariable("?VARU")));
            RDFBooleanNotFilter filterB = new RDFBooleanNotFilter(filterA);

            Assert.IsNotNull(filterB);
            Assert.IsNotNull(filterB.Filter);
            Assert.IsTrue(filterB.ToString().Equals($"FILTER ( !( !( ISURI(?VARU) ) ) )"));
            Assert.IsTrue(filterB.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals($"FILTER ( !( !( ISURI(?VARU) ) ) )"));
            Assert.IsTrue(filterB.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filterB.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBooleanNotFilterBecauseNullFilter()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanNotFilter(null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingBooleanNotFilterBecauseExistsFilter()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanNotFilter(new RDFExistsFilter(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?OBJ")))));

        [TestMethod]
        public void ShouldCreateBooleanNotFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanNotFilter filter = new RDFBooleanNotFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBooleanNotFilterAndKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanNotFilter filter = new RDFBooleanNotFilter(new RDFLangMatchesFilter(new RDFVariable("?B"), "en-US"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBooleanNotFilterAndNotKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanNotFilter filter = new RDFBooleanNotFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_FLOAT));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateBooleanNotFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFBooleanNotFilter filter = new RDFBooleanNotFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }
        #endregion
    }
}
