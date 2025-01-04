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
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFRegexFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateRegexFilterWithoutFlags()
        {
            RDFRegexFilter filter = new RDFRegexFilter(new RDFVariable("?VAR"), new Regex("^hello$"));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.VariableName);
            Assert.IsTrue(filter.VariableName.Equals("?VAR"));
            Assert.IsNotNull(filter.RegEx);
            Assert.IsTrue(filter.RegEx.ToString().Equals("^hello$"));
            Assert.IsTrue(filter.ToString().Equals("FILTER ( REGEX(STR(?VAR), \"^hello$\") )"));
            Assert.IsTrue(filter.ToString([]).Equals("FILTER ( REGEX(STR(?VAR), \"^hello$\") )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldCreateRegexFilterWithFlags()
        {
            RDFRegexFilter filter = new RDFRegexFilter(new RDFVariable("?VAR"), new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.VariableName);
            Assert.IsTrue(filter.VariableName.Equals("?VAR"));
            Assert.IsNotNull(filter.RegEx);
            Assert.IsTrue(filter.RegEx.ToString().Equals("^hello$"));
            Assert.IsTrue(filter.ToString().Equals("FILTER ( REGEX(STR(?VAR), \"^hello$\", \"ismx\") )"));
            Assert.IsTrue(filter.ToString([]).Equals("FILTER ( REGEX(STR(?VAR), \"^hello$\", \"ismx\") )"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingRegexFilterBecauseNullVariable()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFRegexFilter(null, new Regex("^hello$")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingRegexFilterBecauseNullRegex()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFRegexFilter(new RDFVariable("?VAR"), null));

        [TestMethod]
        public void ShouldCreateRegexFilterAndKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFRegexFilter filter = new RDFRegexFilter(new RDFVariable("?A"), new Regex("^http"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateRegexFilterAndKeepRowBecauseUnknownVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFRegexFilter filter = new RDFRegexFilter(new RDFVariable("?Q"), new Regex("^http"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateRegexFilterAndKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFRegexFilter filter = new RDFRegexFilter(new RDFVariable("?A"), new Regex("^ftp"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateRegexFilterAndKeepRowOnNullValue()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = DBNull.Value;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFRegexFilter filter = new RDFRegexFilter(new RDFVariable("?A"), new Regex("^(.){0}$"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateRegexFilterAndNotKeepRow()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFRegexFilter filter = new RDFRegexFilter(new RDFVariable("?A"), new Regex("^ftp"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateRegexFilterAndNotKeepRowBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFRegexFilter filter = new RDFRegexFilter(new RDFVariable("?A"), new Regex("^http"));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }
        #endregion
    }
}