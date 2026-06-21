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
public class RDFNotExistsFilterTest
{
    #region Helpers
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
    public void ShouldCreateNotExistsFilterFromPatternGroup()
    {
        RDFNotExistsFilter filter = new RDFNotExistsFilter(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDF.ALT)));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.GroupGraphPattern);
        Assert.IsInstanceOfType(filter.GroupGraphPattern, typeof(RDFPatternGroup));
        Assert.IsNull(filter.PatternResults);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( NOT EXISTS { ?S ?P <" + RDFVocabulary.RDF.ALT + "> . } )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("FILTER ( NOT EXISTS { ?S ?P rdf:Alt . } )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateNotExistsFilterFromSubSelect()
    {
        RDFSelectQuery subSelect = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"))));
        RDFNotExistsFilter filter = new RDFNotExistsFilter(subSelect);

        Assert.IsNotNull(filter);
        Assert.IsInstanceOfType(filter.GroupGraphPattern, typeof(RDFSelectQuery));
        Assert.IsTrue(filter.ToString().StartsWith("FILTER ( NOT EXISTS {", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString().Contains("SELECT"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingNotExistsFilterBecauseNullPatternGroup()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFNotExistsFilter((RDFPatternGroup)null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingNotExistsFilterBecauseNullSubSelect()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFNotExistsFilter((RDFSelectQuery)null));
    #endregion

    #region Tests (correlation)
    [TestMethod]
    public void ShouldKeepRowDisjointCaseWithEmptyResults()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        //No column shared with the row AND no solution => EXISTS is false => NOT EXISTS keeps the row
        RDFNotExistsFilter filter = new RDFNotExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?Z");

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldNotKeepRowDisjointCaseWithNonEmptyResults()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        //No column shared but a solution exists => EXISTS is true => NOT EXISTS drops the row
        RDFNotExistsFilter filter = new RDFNotExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?Z");
        filter.PatternResults.AddRow(new Dictionary<string, string> { { "?Z", new RDFResource("ex:thing").ToString() } });

        Assert.IsFalse(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldNotKeepRowMatchingColumn()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFNotExistsFilter filter = new RDFNotExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() }
        });

        Assert.IsFalse(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldNotKeepRowBecauseUnboundSharedCellIsWildcard()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", null },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFNotExistsFilter filter = new RDFNotExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() }
        });

        Assert.IsFalse(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldKeepRowUnmatchingColumn()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFNotExistsFilter filter = new RDFNotExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello").ToString() }
        });

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], false));
    }

    [TestMethod]
    public void ShouldKeepRowBecauseNegation()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        //NOT EXISTS over a matching value drops the row; the extra negation flips it back to kept
        RDFNotExistsFilter filter = new RDFNotExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");
        filter.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() }
        });

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], true));
    }

    [TestMethod]
    public void ShouldKeepRowBecauseEmptyResponseTable()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFNotExistsFilter filter = new RDFNotExistsFilter(SamplePatternGroup()) { PatternResults = new RDFTable() };
        filter.PatternResults.AddColumn("?A");

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], false));
    }
    #endregion
}
