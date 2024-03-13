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
    public class RDFLangMatchesFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateNoneLangMatchesFilter()
        {
            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?VAR"), null);

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.VariableName);
            Assert.IsTrue(filter.VariableName.Equals("?VAR"));
            Assert.IsNotNull(filter.Language);
            Assert.IsTrue(filter.Language.Equals(string.Empty));
            Assert.IsNull(filter.ExactLanguageRegex);
            Assert.IsTrue(filter.ToString().Equals("FILTER ( LANGMATCHES(LANG(?VAR), \"\") )"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { }).Equals("FILTER ( LANGMATCHES(LANG(?VAR), \"\") )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldCreateStarLangMatchesFilter()
        {
            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?VAR"), "*");

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.VariableName);
            Assert.IsTrue(filter.VariableName.Equals("?VAR"));
            Assert.IsNotNull(filter.Language);
            Assert.IsTrue(filter.Language.Equals("*"));
            Assert.IsNull(filter.ExactLanguageRegex);
            Assert.IsTrue(filter.ToString().Equals("FILTER ( LANGMATCHES(LANG(?VAR), \"*\") )"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { }).Equals("FILTER ( LANGMATCHES(LANG(?VAR), \"*\") )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldCreateExactLangMatchesFilter()
        {
            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?VAR"), "en-US");

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.VariableName);
            Assert.IsTrue(filter.VariableName.Equals("?VAR"));
            Assert.IsNotNull(filter.Language);
            Assert.IsTrue(filter.Language.Equals("EN-US"));
            Assert.IsNotNull(filter.ExactLanguageRegex);
            Assert.IsTrue(filter.ToString().Equals("FILTER ( LANGMATCHES(LANG(?VAR), \"EN-US\") )"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { }).Equals("FILTER ( LANGMATCHES(LANG(?VAR), \"EN-US\") )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingLangMatchesFilterBecauseNullVariable()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFLangMatchesFilter(null, "*"));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingLangMatchesFilterBecauseInvalidLanguage()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFLangMatchesFilter(new RDFVariable("?VAR"), "756"));

        [TestMethod]
        public void ShouldCreateNoneLangMatchesFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?A"), string.Empty);
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateStarLangMatchesFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?B"), "*");
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateExactLangMatchesFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?B"), "en-US");
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNoneLangMatchesFilterAndKeepRowWithNullValue()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?A"), null);
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateExactLangMatchesFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?B"), "en-US");
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateNoneLangMatchesFilterAndNotKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?B"), null);
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateStarLangMatchesFilterAndNotKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?B"), "*");
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateExactLangMatchesFilterAndKeepRowHavingSubLanguage()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?B"), "en");
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateExactLangMatchesFilterAndNotKeepRowHavingSubLanguageBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFLangMatchesFilter filter = new RDFLangMatchesFilter(new RDFVariable("?B"), "en");
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }
        #endregion
    }
}