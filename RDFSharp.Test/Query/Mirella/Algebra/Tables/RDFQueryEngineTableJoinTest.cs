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
using System.Linq;
using System.Text;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit characterization of the v4 RDFTable-based join/combine/merge primitives in RDFQueryEngine.
/// The frozen outputs reproduce the captured behavior of the old DataTable operations, with the one
/// deliberate change agreed for the migration: the inner-join compares common columns with Ordinal
/// (case-sensitive), so values differing only by case no longer merge (see InnerJoin case test).
/// </summary>
[TestClass]
public class RDFQueryEngineTableJoinTest
{
    #region Utilities
    private static RDFTable Tab(string[] columns, params string[][] rows)
    {
        RDFTable table = new RDFTable();
        foreach (string column in columns)
            table.AddColumn(column);
        foreach (string[] row in rows)
            table.AddRow(row);
        return table;
    }

    private static string Dump(RDFTable table)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(string.Join("|", table.Columns.Select(c => c.Name)));
        foreach (RDFTableRow row in table.Rows)
            sb.Append('\n').Append(string.Join("|", Enumerable.Range(0, table.ColumnsCount).Select(i => row[i] ?? "<U>")));
        return sb.ToString();
    }

    //Left [?S,?X] and Right [?X,?O] sharing ?X, with nulls in the common column on both sides
    private static RDFTable Left() => Tab(["?S", "?X"], ["s1", "v1"], ["s2", null], ["s3", "v3"]);
    private static RDFTable Right() => Tab(["?X", "?O"], ["v1", "o1"], [null, "o2"], ["v3", "o3"]);
    #endregion

    #region InnerJoin
    [TestMethod]
    public void ShouldInnerJoinOnCommonColumnDroppingUnbound()
    {
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|v1|o1\n" +
            "s3|v3|o3",
            Dump(RDFQueryEngine.InnerJoinTables(Left(), Right())));
    }

    [TestMethod]
    public void ShouldInnerJoinAsProductWhenNoCommonColumn()
    {
        Assert.AreEqual(
            "?S|?O\n" +
            "a|p1\n" +
            "a|p2\n" +
            "b|p1\n" +
            "b|p2",
            Dump(RDFQueryEngine.InnerJoinTables(Tab(["?S"], ["a"], ["b"]), Tab(["?O"], ["p1"], ["p2"]))));
    }

    [TestMethod]
    public void ShouldInnerJoinCaseSensitivelyWithOrdinal()
    {
        //Deliberate v4 change: "ABC" no longer matches "abc" (old DataRelation was case-insensitive)
        Assert.AreEqual(
            "?X|?S|?O",
            Dump(RDFQueryEngine.InnerJoinTables(Tab(["?X", "?S"], ["ABC", "s1"]), Tab(["?X", "?O"], ["abc", "o1"]))));
    }
    #endregion

    #region OuterJoin
    [TestMethod]
    public void ShouldOuterJoinWithNullCoalescing()
    {
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|v1|o1\n" +
            "s1|v1|o2\n" +
            "s2|v1|o1\n" +
            "s2|<U>|o2\n" +
            "s2|v3|o3\n" +
            "s3|v3|o2\n" +
            "s3|v3|o3",
            Dump(RDFQueryEngine.OuterJoinTables(Left(), Right())));
    }

    [TestMethod]
    public void ShouldOuterJoinPadUnmatchedLeftWhenOptional()
    {
        RDFTable right = Tab(["?X", "?O"], ["vA", "o1"]);
        right.IsOptional = true;
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|vA|o1\n" +
            "s2|vB|<U>",
            Dump(RDFQueryEngine.OuterJoinTables(Tab(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]), right)));
    }

    [TestMethod]
    public void ShouldOuterJoinDropUnmatchedLeftWhenNotOptional()
    {
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|vA|o1",
            Dump(RDFQueryEngine.OuterJoinTables(Tab(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]), Tab(["?X", "?O"], ["vA", "o1"]))));
    }
    #endregion

    #region DiffJoin
    [TestMethod]
    public void ShouldDiffJoinRemovingAllCompatibleLeftRows()
    {
        //Every left row is compatible with some right row (null is compatible with anything) => empty
        Assert.AreEqual(
            "?S|?X",
            Dump(RDFQueryEngine.DiffJoinTables(Left(), Right())));
    }

    [TestMethod]
    public void ShouldDiffJoinKeepAllLeftWhenNoCommonColumn()
    {
        Assert.AreEqual(
            "?S\n" +
            "a\n" +
            "b",
            Dump(RDFQueryEngine.DiffJoinTables(Tab(["?S"], ["a"], ["b"]), Tab(["?O"], ["p1"], ["p2"]))));
    }

    [TestMethod]
    public void ShouldDiffJoinKeepOnlyUnmatchedLeftRows()
    {
        Assert.AreEqual(
            "?S|?X\n" +
            "s2|vB",
            Dump(RDFQueryEngine.DiffJoinTables(Tab(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]), Tab(["?X", "?O"], ["vA", "o1"]))));
    }
    #endregion

    #region CombineTables
    [TestMethod]
    public void ShouldCombineUnionMergingRows()
    {
        RDFTable a = Tab(["?X"], ["a"], ["b"]);
        a.JoinAsUnion = true;
        RDFTable b = Tab(["?X"], ["c"]);
        //Union merges previous (a) into current (b): current rows first, then merged rows
        Assert.AreEqual(
            "?X\n" +
            "c\n" +
            "a\n" +
            "b",
            Dump(RDFQueryEngine.CombineTables([a, b])));
    }

    [TestMethod]
    public void ShouldCombineDispatchingOuterJoinOnOptional()
    {
        RDFTable a = Tab(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]);
        RDFTable b = Tab(["?X", "?O"], ["vA", "o1"]);
        b.IsOptional = true;
        Assert.AreEqual(
            "?S|?X|?O\n" +
            "s1|vA|o1\n" +
            "s2|vB|<U>",
            Dump(RDFQueryEngine.CombineTables([a, b])));
    }

    [TestMethod]
    public void ShouldCombineDispatchingMinus()
    {
        RDFTable a = Tab(["?S", "?X"], ["s1", "vA"], ["s2", "vB"]);
        a.JoinAsMinus = true;
        RDFTable b = Tab(["?X", "?O"], ["vA", "o1"]);
        Assert.AreEqual(
            "?S|?X\n" +
            "s2|vB",
            Dump(RDFQueryEngine.CombineTables([a, b])));
    }

    [TestMethod]
    public void ShouldCombineReturnEmptyOnNoTables()
        => Assert.AreEqual(0, RDFQueryEngine.CombineTables(new List<RDFTable>()).RowsCount);

    [TestMethod]
    public void ShouldCombineReturnSingleTableUnchanged()
    {
        RDFTable only = Tab(["?X"], ["a"]);
        Assert.AreSame(only, RDFQueryEngine.CombineTables([only]));
    }
    #endregion

    #region SortTable
    [TestMethod]
    public void ShouldSortAscendingWithUnboundFirst()
    {
        RDFTable table = Tab(["?N"], ["N2"], [null], ["N1"]);
        Assert.AreEqual(
            "?N\n<U>\nN1\nN2",
            Dump(RDFQueryEngine.SortTable(table, [("?N", false)])));
    }

    [TestMethod]
    public void ShouldSortDescendingWithUnboundLast()
    {
        RDFTable table = Tab(["?N"], ["N2"], [null], ["N1"]);
        Assert.AreEqual(
            "?N\nN2\nN1\n<U>",
            Dump(RDFQueryEngine.SortTable(table, [("?N", true)])));
    }

    [TestMethod]
    public void ShouldSortStablyKeepingInsertionOrderOnTies()
    {
        //Two rows tie on ?N=T; their ?S order (s1 before s2) must be preserved in both directions
        RDFTable table = Tab(["?N", "?S"], ["T", "s1"], ["A", "s0"], ["T", "s2"]);
        Assert.AreEqual(
            "?N|?S\nA|s0\nT|s1\nT|s2",
            Dump(RDFQueryEngine.SortTable(table, [("?N", false)])));
        Assert.AreEqual(
            "?N|?S\nT|s1\nT|s2\nA|s0",
            Dump(RDFQueryEngine.SortTable(table, [("?N", true)])));
    }

    [TestMethod]
    public void ShouldSortByOrdinalCodePointNotCulture()
    {
        //Ordinal puts uppercase before lowercase (A<B<C<a<b), unlike DataView's culture collation
        RDFTable table = Tab(["?N"], ["B"], ["a"], ["C"], ["b"], ["A"]);
        Assert.AreEqual(
            "?N\nA\nB\nC\na\nb",
            Dump(RDFQueryEngine.SortTable(table, [("?N", false)])));
    }

    [TestMethod]
    public void ShouldSortByMultipleKeys()
    {
        RDFTable table = Tab(["?A", "?B"], ["x", "2"], ["x", "1"], ["y", "1"]);
        Assert.AreEqual(
            "?A|?B\nx|1\nx|2\ny|1",
            Dump(RDFQueryEngine.SortTable(table, [("?A", false), ("?B", false)])));
    }

    [TestMethod]
    public void ShouldSortIgnoringKeysWhoseColumnIsAbsent()
    {
        RDFTable table = Tab(["?N"], ["b"], ["a"]);
        Assert.AreEqual(
            "?N\na\nb",
            Dump(RDFQueryEngine.SortTable(table, [("?MISSING", false), ("?N", false)])));
    }
    #endregion

    #region DistinctTable
    [TestMethod]
    public void ShouldDistinctCollapsingDuplicatesPreservingOrder()
    {
        RDFTable table = Tab(["?N"], ["N2"], ["N1"], ["N2"], ["N1"]);
        Assert.AreEqual(
            "?N\nN2\nN1",
            Dump(RDFQueryEngine.DistinctTable(table)));
    }

    [TestMethod]
    public void ShouldDistinctTreatingUnboundAsEqual()
    {
        RDFTable table = Tab(["?N"], [null], ["N1"], [null]);
        Assert.AreEqual(
            "?N\n<U>\nN1",
            Dump(RDFQueryEngine.DistinctTable(table)));
    }

    [TestMethod]
    public void ShouldDistinctCaseSensitively()
    {
        RDFTable table = Tab(["?N"], ["ABC"], ["abc"], ["ABC"]);
        Assert.AreEqual(
            "?N\nABC\nabc",
            Dump(RDFQueryEngine.DistinctTable(table)));
    }

    [TestMethod]
    public void ShouldDistinctOverMultipleColumnsIncludingUnbound()
    {
        RDFTable table = Tab(["?S", "?N"], ["s", "N1"], ["s", null], ["s", "N1"], ["s", null]);
        Assert.AreEqual(
            "?S|?N\ns|N1\ns|<U>",
            Dump(RDFQueryEngine.DistinctTable(table)));
    }
    #endregion
}
