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
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFSameTermFilterTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateSameTermFilter()
    {
        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?VAR"), RDFVocabulary.RDF.ALT);

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.VariableName);
        Assert.IsTrue(filter.VariableName.Equals("?VAR"));
        Assert.IsNotNull(filter.RDFTerm);
        Assert.IsTrue(filter.RDFTerm.Equals(RDFVocabulary.RDF.ALT));
        Assert.IsNotNull(filter.RDFTermString);
        Assert.IsTrue(filter.RDFTermString.Equals($"{RDFVocabulary.RDF.ALT}"));
        Assert.IsTrue(filter.ToString().Equals($"FILTER ( SAMETERM(?VAR, <{RDFVocabulary.RDF.ALT}>) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("FILTER ( SAMETERM(?VAR, rdf:Alt) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSameTermFilterBecauseNullVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFSameTermFilter(null, new RDFVariable("?VAR")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSameTermFilterBecauseNullTerm()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFSameTermFilter(new RDFVariable("?VAR"), null));

    [TestMethod]
    public void ShouldCreateSameTermFilterAndKeepRow()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?A"), new RDFResource("http://example.org/"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateSameTermFilterAndKeepRowWithVariableAsTerm()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFResource("http://example.org/").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?A"), new RDFVariable("?B"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateSameTermFilterAndKeepRowWithVariableAsTermAndNullComparison()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = DBNull.Value;
        row["?B"] = RDFPlainLiteral.Empty;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?A"), new RDFVariable("?B"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }
        
    [TestMethod]
    public void ShouldCreateSameTermFilterAndKeepRowWithUnknownVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?Q"), new RDFVariable("?A"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateSameTermFilterAndKeepRowWithUnknownVariableAsTerm()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?A"), new RDFVariable("?Q"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateSameTermFilterAndKeepRowBecauseNegation()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?A"), new RDFPlainLiteral("Hello!"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateSameTermFilterAndNotKeepRow()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?A"), new RDFResource("http://example.org/"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateSameTermFilterAndNotKeepRowWithNullVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null;
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?A"), new RDFResource("http://example.org/"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateSameTermFilterAndNotKeepRowBecauseNegation()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFSameTermFilter filter = new RDFSameTermFilter(new RDFVariable("?A"), new RDFResource("http://example.org/"));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsFalse(keepRow);
    }
    #endregion
}