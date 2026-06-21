/*
   Copyright 2012-2026 Marco De Salvo

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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFExistsFilterTest
{
    #region Helpers
    //A simple pattern group body reused across the correlation tests (the pattern itself is irrelevant to the
    //correlation, which now works purely on the columns produced by the group graph pattern evaluation)
    private static RDFPatternGroup SamplePatternGroup()
        => new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O")));

    private static RDFTable RowTable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        return table;
    }
    #endregion

    #region Tests (creation)
    [TestMethod]
    public void ShouldCreateExistsFilterFromPatternGroup()
    {
        RDFExistsFilter filter = new RDFExistsFilter(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDF.ALT)));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.GroupGraphPattern);
        Assert.IsInstanceOfType(filter.GroupGraphPattern, typeof(RDFPatternGroup));
        Assert.IsNull(filter.PatternResults);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( EXISTS { ?S ?P <" + RDFVocabulary.RDF.ALT + "> . } )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("FILTER ( EXISTS { ?S ?P rdf:Alt . } )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateExistsFilterFromSubSelect()
    {
        RDFSelectQuery subSelect = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"))));
        RDFExistsFilter filter = new RDFExistsFilter(subSelect);

        Assert.IsNotNull(filter);
        Assert.IsInstanceOfType(filter.GroupGraphPattern, typeof(RDFSelectQuery));
        Assert.IsTrue(((RDFSelectQuery)filter.GroupGraphPattern).IsSubQuery);
        Assert.IsTrue(filter.ToString().StartsWith("FILTER ( EXISTS {", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString().Contains("SELECT"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExistsFilterBecauseNullPatternGroup()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExistsFilter((RDFPatternGroup)null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExistsFilterBecauseNullSubSelect()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExistsFilter((RDFSelectQuery)null));
    #endregion

    #region Tests (correlation)
    [TestMethod]
    public void ShouldKeepRowDisjointCaseWithNonEmptyResults()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        //No column shared with the row, but the group graph pattern produced at least one solution => keep
        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?Z");
        filter.PatternResults.AddRow(new Dictionary<string, string> { { "?Z", new RDFResource("ex:thing").ToString() } });

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldNotKeepRowDisjointCaseWithEmptyResults()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        //No column shared with the row AND the group graph pattern produced no solution => drop (EXISTS is false)
        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?Z");

        Assert.IsFalse(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldKeepRowMatchingSingleColumn()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() }
        });

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldKeepRowMatchingTwoColumns()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddColumn("?B");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldKeepRowBecauseUnboundSharedCellIsWildcard()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", null },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() }
        });

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldKeepRowBecauseNegationOfUnmatchingValue()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() }
        });

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], true));
    }

    [TestMethod]
    public void ShouldNotKeepRowBecauseUnmatchingColumn()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello").ToString() }
        });

        Assert.IsFalse(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldNotKeepRowBecauseNegationOfMatchingValue()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() }
        });

        Assert.IsFalse(filter.ApplyFilter(table.Rows[0], true));
    }

    [TestMethod]
    public void ShouldNotKeepRowBecauseEmptyResponseTable()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsFilter filter = new RDFExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");

        Assert.IsFalse(filter.ApplyFilter(table.Rows[0], false));
    }
    #endregion
}
