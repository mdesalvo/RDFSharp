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
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFValuesFilterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateValuesFilterWithSingleBinding()
        {
            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?A"), new List<RDFPatternMember>() { RDFVocabulary.RDF.ALT }));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.Values);
            Assert.IsTrue(filter.Values.Bindings.Count == 1);
            Assert.IsTrue(filter.ToString().Equals("VALUES ?A { <" + RDFVocabulary.RDF.ALT + "> }"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals("VALUES ?A { rdf:Alt }"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldCreateValuesFilterWithUndefSingleBinding()
        {
            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?A"), new List<RDFPatternMember>() { null }));

            Assert.IsNotNull(filter);
            Assert.IsNotNull(filter.Values);
            Assert.IsTrue(filter.Values.Bindings.Count == 1);
            Assert.IsTrue(filter.ToString().Equals("VALUES ?A { UNDEF }"));
            Assert.IsTrue(filter.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals("VALUES ?A { UNDEF }"));
            Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldCreateValuesFilterAndKeepRowWithSingleBinding()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?A"), new List<RDFPatternMember>() { new RDFResource("http://example.org/") }));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateValuesFilterAndKeepRowWithUndefSingleBinding()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?A"), new List<RDFPatternMember>() { null }));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateValuesFilterAndKeepRowWithSingleBindingAndUnknownVariable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?Q"), new List<RDFPatternMember>() { new RDFResource("http://example.org/") }));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateValuesFilterAndKeepRowWithSingleBindingBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?A"), new List<RDFPatternMember>() { new RDFResource("http://example.org/test/") }));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsTrue(keepRow);
        }

        [TestMethod]
        public void ShouldCreateValuesFilterAndNotKeepRowWithSingleBinding()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?A"), new List<RDFPatternMember>() { new RDFResource("http://example.org/test/") }));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateValuesFilterAndNotKeepRowWithSingleBindingBecauseNullValue()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = null;
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?A"), new List<RDFPatternMember>() { new RDFResource("http://example.org/test/") }));
            bool keepRow = filter.ApplyFilter(table.Rows[0], false);

            Assert.IsFalse(keepRow);
        }

        [TestMethod]
        public void ShouldCreateValuesFilterAndNotKeepRowWithSingleBindingBecauseNegation()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            DataRow row = table.NewRow();
            row["?A"] = new RDFResource("http://example.org/").ToString();
            row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row);
            table.AcceptChanges();

            RDFValuesFilter filter = new RDFValuesFilter(
                new RDFValues().AddColumn(new RDFVariable("?A"), new List<RDFPatternMember>() { new RDFResource("http://example.org/") }));
            bool keepRow = filter.ApplyFilter(table.Rows[0], true);

            Assert.IsFalse(keepRow);
        }
        #endregion
    }
}