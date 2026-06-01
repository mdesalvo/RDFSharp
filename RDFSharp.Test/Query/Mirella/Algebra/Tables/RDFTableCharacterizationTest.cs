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
using System.Data;
using System.Linq;
using System.Text;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Characterization oracle for the v4 DataTable -> RDFTable migration.
///
/// These tests pin, bit-for-bit, the OBSERVABLE behavior of the current (DataView/DataSet-based)
/// Mirella evaluation pipeline on the null/ordering/join edge cases that the port must preserve:
///   - ORDER BY null placement (DataView sorts an UNBOUND value as the smallest: first ASC, last DESC)
///   - ORDER BY tie stability (equal keys keep insertion order, in BOTH directions => stable sort)
///   - DISTINCT null equality (UNBOUND == UNBOUND, collapses to a single group)
///   - product join (no common variable)
///   - MINUS with a shared variable
///
/// They run through the PUBLIC query API (RDFQueryEngine.EvaluateSelectQuery), so they stay valid
/// across the migration: when SelectResults becomes an RDFTable the only thing to update is the body
/// of Dump(...) - the frozen expected snapshots below must NOT change.
/// </summary>
[TestClass]
public class RDFTableCharacterizationTest
{
    #region Fixture
    //Deterministic graph: ex:y3 has no name (=> UNBOUND under OPTIONAL); ex:y1 and ex:y5 share name "N1"
    private static RDFGraph BuildGraph() => new RDFGraph(
    [
        new RDFTriple(new RDFResource("ex:b"), new RDFResource("ex:dogOf"), new RDFResource("ex:y2")),
        new RDFTriple(new RDFResource("ex:a"), new RDFResource("ex:dogOf"), new RDFResource("ex:y1")),
        new RDFTriple(new RDFResource("ex:c"), new RDFResource("ex:dogOf"), new RDFResource("ex:y3")),
        new RDFTriple(new RDFResource("ex:d"), new RDFResource("ex:dogOf"), new RDFResource("ex:y5")),
        new RDFTriple(new RDFResource("ex:y1"), new RDFResource("ex:hasName"), new RDFPlainLiteral("N1")),
        new RDFTriple(new RDFResource("ex:y2"), new RDFResource("ex:hasName"), new RDFPlainLiteral("N2")),
        new RDFTriple(new RDFResource("ex:y5"), new RDFResource("ex:hasName"), new RDFPlainLiteral("N1"))
    ]);

    //Canonical, deterministic dump of a SELECT result: header line of column names, then one line per
    //row with cells joined by '|', UNBOUND variables rendered as "<UNBOUND>". This is the single place
    //to touch when SelectResults switches from DataTable to RDFTable (the frozen strings stay the same).
    private static string Dump(RDFSelectQueryResult result)
    {
        DataTable table = result.SelectResults;
        string[] columns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToArray();
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Join("|", columns));
        foreach (DataRow row in table.Rows)
            sb.Append('\n').Append(string.Join("|", columns.Select(c => row.IsNull(c) ? "<UNBOUND>" : (string)row[c])));
        return sb.ToString();
    }

    private static RDFSelectQueryResult Evaluate(RDFSelectQuery query)
        => new RDFQueryEngine().EvaluateSelectQuery(query, BuildGraph());
    #endregion

    #region Tests
    [TestMethod]
    public void ShouldPlaceUnboundFirstWhenOrderByAscending()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?O"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?N"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        Assert.AreEqual(
            "?O|?Y|?N\n" +
            "ex:c|ex:y3|<UNBOUND>\n" +
            "ex:a|ex:y1|N1\n" +
            "ex:d|ex:y5|N1\n" +
            "ex:b|ex:y2|N2",
            Dump(Evaluate(query)));
    }

    [TestMethod]
    public void ShouldPlaceUnboundLastWhenOrderByDescending()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?O"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?N"), RDFQueryEnums.RDFOrderByFlavors.DESC));

        Assert.AreEqual(
            "?O|?Y|?N\n" +
            "ex:b|ex:y2|N2\n" +
            "ex:a|ex:y1|N1\n" +
            "ex:d|ex:y5|N1\n" +
            "ex:c|ex:y3|<UNBOUND>",
            Dump(Evaluate(query)));
    }

    [TestMethod]
    public void ShouldCollapseUnboundAndDuplicatesUnderDistinct()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?O"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddProjectionVariable(new RDFVariable("?N"))
            .AddModifier(new RDFDistinctModifier())
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?N"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        Assert.AreEqual(
            "?N\n" +
            "<UNBOUND>\n" +
            "N1\n" +
            "N2",
            Dump(Evaluate(query)));
    }

    [TestMethod]
    public void ShouldComputeProductJoinWhenNoCommonVariable()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?A"), new RDFResource("ex:dogOf"), new RDFResource("ex:y1")))
                .AddPattern(new RDFPattern(new RDFVariable("?B"), new RDFResource("ex:dogOf"), new RDFResource("ex:y2"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?A"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        Assert.AreEqual(
            "?A|?B\n" +
            "ex:a|ex:b",
            Dump(Evaluate(query)));
    }

    [TestMethod]
    public void ShouldSubtractMatchingRowsUnderMinus()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?O"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                .MinusWithNext())
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?O"), new RDFResource("ex:dogOf"), new RDFResource("ex:y1"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?O"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        Assert.AreEqual(
            "?O|?Y\n" +
            "ex:b|ex:y2\n" +
            "ex:c|ex:y3\n" +
            "ex:d|ex:y5",
            Dump(Evaluate(query)));
    }
    #endregion
}
