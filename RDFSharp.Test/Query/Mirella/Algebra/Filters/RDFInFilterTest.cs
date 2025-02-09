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

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFInFilterTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateInFilter()
    {
        RDFInFilter filter = new RDFInFilter(new RDFVariable("?VAR"), [ RDFVocabulary.RDF.ALT, RDFVocabulary.RDF.BAG,
            null, new RDFPlainLiteral("hello", "en-US"), new RDFTypedLiteral("---25", RDFModelEnums.RDFDatatypes.XSD_GDAY) ]);

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.TermToSearch);
        Assert.IsTrue(filter.TermToSearch.Equals(new RDFVariable("?VAR")));
        Assert.IsNotNull(filter.InTerms);
        Assert.IsTrue(filter.InTerms.Count == 4);
        Assert.IsTrue(filter.ToString().Equals($"FILTER ( ?VAR IN (<{RDFVocabulary.RDF.ALT}>, <{RDFVocabulary.RDF.BAG}>, \"hello\"@EN-US, \"---25Z\"^^<{RDFVocabulary.XSD.G_DAY}>) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals($"FILTER ( ?VAR IN (rdf:Alt, rdf:Bag, \"hello\"@EN-US, \"---25Z\"^^<{RDFVocabulary.XSD.G_DAY}>) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingInFilterBecauseNullSearchTerm()
        => Assert.ThrowsException<RDFQueryException>(() => new RDFInFilter(null, []));

    [TestMethod]
    public void ShouldCreateInFilterAndKeepRow1()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:novo").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInFilter filter = new RDFInFilter(new RDFVariable("?A"), [new RDFResource("ex:novo")]);
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateInFilterAndKeepRow2()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:novo").ToString();
        row["?B"] = new RDFTypedLiteral("27.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInFilter filter = new RDFInFilter(new RDFVariable("?B"), [new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_INT)]);
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateInFilterAndKeepRow3()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null;
        row["?B"] = new RDFTypedLiteral("27.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInFilter filter = new RDFInFilter(new RDFVariable("?A"), [new RDFPlainLiteral(null)]);
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateInFilterAndNotKeepRow()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:novo").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInFilter filter = new RDFInFilter(new RDFVariable("?A"), [new RDFPlainLiteral("hello", "en")]);
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateInFilterAndNotKeepRowBecauseNegation()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("ex:novo").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFInFilter filter = new RDFInFilter(new RDFVariable("?A"), [new RDFResource("ex:novo")]);
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsFalse(keepRow);
    }
    #endregion
}