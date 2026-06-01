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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFTableTest
{
    #region Schema
    [TestMethod]
    public void ShouldCreateEmptyTable()
    {
        RDFTable table = new RDFTable();

        Assert.IsNotNull(table);
        Assert.AreEqual(0, table.ColumnsCount);
        Assert.AreEqual(0, table.RowsCount);
        Assert.AreEqual(0, table.Columns.Count);
        Assert.AreEqual(0, table.Rows.Count);
        Assert.IsFalse(table.IsOptional);
        Assert.IsFalse(table.JoinAsUnion);
        Assert.IsFalse(table.JoinAsMinus);
    }

    [TestMethod]
    public void ShouldAddColumnNormalizingName()
    {
        RDFTable table = new RDFTable();
        RDFTableColumn column = table.AddColumn("  ?subject ");

        Assert.AreEqual(1, table.ColumnsCount);
        Assert.AreEqual("?SUBJECT", column.Name);
        Assert.AreEqual(0, column.Ordinal);
        Assert.AreSame(column, table.Columns[0]);
    }

    [TestMethod]
    public void ShouldAddColumnsAssigningProgressiveOrdinals()
    {
        RDFTable table = new RDFTable();
        RDFTableColumn colS = table.AddColumn("?S");
        RDFTableColumn colP = table.AddColumn("?P");
        RDFTableColumn colO = table.AddColumn("?O");

        Assert.AreEqual(3, table.ColumnsCount);
        Assert.AreEqual(0, colS.Ordinal);
        Assert.AreEqual(1, colP.Ordinal);
        Assert.AreEqual(2, colO.Ordinal);
    }

    [TestMethod]
    public void ShouldNotAddDuplicateColumnEvenWithDifferentCasing()
    {
        RDFTable table = new RDFTable();
        RDFTableColumn first = table.AddColumn("?S");
        RDFTableColumn second = table.AddColumn("?s");

        Assert.AreEqual(1, table.ColumnsCount);
        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void ShouldLookupColumnCaseInsensitively()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");

        Assert.IsTrue(table.HasColumn("?S"));
        Assert.IsTrue(table.HasColumn(" ?s "));
        Assert.IsFalse(table.HasColumn("?X"));
        Assert.IsFalse(table.HasColumn(null));
        Assert.AreEqual(0, table.OrdinalOf("?s"));
        Assert.AreEqual(-1, table.OrdinalOf("?X"));
        Assert.AreEqual(-1, table.OrdinalOf(null));
    }
    #endregion

    #region Data
    [TestMethod]
    public void ShouldAddRowFromBindings()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        table.AddRow(new Dictionary<string, string> { { "?S", "ex:s" }, { "?P", "ex:p" } });

        Assert.AreEqual(1, table.RowsCount);
        RDFTableRow row = table.Rows[0];
        Assert.AreEqual("ex:s", row["?S"]);
        Assert.AreEqual("ex:p", row["?P"]);
    }

    [TestMethod]
    public void ShouldAddRowFromBindingsLeavingMissingColumnsUnbound()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        table.AddRow(new Dictionary<string, string> { { "?S", "ex:s" } });

        Assert.AreEqual(1, table.RowsCount);
        RDFTableRow row = table.Rows[0];
        Assert.AreEqual("ex:s", row["?S"]);
        Assert.IsNull(row["?P"]);
    }

    [TestMethod]
    public void ShouldAddRowFromBindingsIgnoringUnknownColumns()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddRow(new Dictionary<string, string> { { "?S", "ex:s" }, { "?UNKNOWN", "ex:x" } });

        Assert.AreEqual(1, table.RowsCount);
        Assert.AreEqual("ex:s", table.Rows[0]["?S"]);
    }

    [TestMethod]
    public void ShouldNotAddRowWhenNoBindingMatchesAColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddRow(new Dictionary<string, string> { { "?UNKNOWN", "ex:x" } });

        Assert.AreEqual(0, table.RowsCount);
    }

    [TestMethod]
    public void ShouldAddRowFromCells()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        table.AddRow(new[] { "ex:s", (string)null });

        Assert.AreEqual(1, table.RowsCount);
        Assert.AreEqual("ex:s", table.Rows[0]["?S"]);
        Assert.IsTrue(table.Rows[0].IsUnbound("?P"));
    }

    [TestMethod]
    public void ShouldThrowAddingNullCells()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFTable().AddRow((string[])null));

    [TestMethod]
    public void ShouldThrowAddingCellsOfWrongLength()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");

        Assert.ThrowsExactly<RDFQueryException>(() => table.AddRow(new[] { "ex:s" }));
    }

    [TestMethod]
    public void ShouldWidenExistingRowsWhenAddingColumnLater()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddRow(new[] { "ex:s" });
        table.AddColumn("?P");

        Assert.AreEqual(2, table.ColumnsCount);
        Assert.AreEqual(1, table.RowsCount);
        Assert.AreEqual("ex:s", table.Rows[0]["?S"]);
        Assert.IsTrue(table.Rows[0].IsUnbound("?P"));
    }
    #endregion

    #region Rows
    [TestMethod]
    public void ShouldEnumerateRows()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddRow(new[] { "ex:s1" });
        table.AddRow(new[] { "ex:s2" });

        List<string> subjects = new List<string>();
        foreach (RDFTableRow row in table.Rows)
            subjects.Add(row["?S"]);

        CollectionAssert.AreEqual(new[] { "ex:s1", "ex:s2" }, subjects);
    }

    [TestMethod]
    public void ShouldEnumerateRowsViaLinq()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddRow(new[] { "ex:s1" });
        table.AddRow(new[] { "ex:s2" });

        Assert.AreEqual(2, table.Rows.Count());
        Assert.AreEqual("ex:s2", table.Rows.Last()["?S"]);
    }

    [TestMethod]
    public void ShouldResetRowsEnumerator()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddRow(new[] { "ex:s1" });
        table.AddRow(new[] { "ex:s2" });

        RDFTableRowCollection.Enumerator enumerator = table.Rows.GetEnumerator();
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual("ex:s1", enumerator.Current["?S"]);
        enumerator.Reset();
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual("ex:s1", enumerator.Current["?S"]);
        Assert.IsTrue(enumerator.MoveNext());
        Assert.AreEqual("ex:s2", enumerator.Current["?S"]);
        Assert.IsFalse(enumerator.MoveNext());
        enumerator.Dispose();
    }
    #endregion

    #region Interop
    [TestMethod]
    public void ShouldExportToDataTable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        table.AddRow(new[] { "ex:s", (string)null });

        DataTable dataTable = table.ToDataTable();

        Assert.AreEqual(2, dataTable.Columns.Count);
        Assert.AreEqual("?S", dataTable.Columns[0].ColumnName);
        Assert.AreEqual("?P", dataTable.Columns[1].ColumnName);
        Assert.AreEqual(typeof(string), dataTable.Columns[0].DataType);
        Assert.AreEqual(1, dataTable.Rows.Count);
        Assert.AreEqual("ex:s", dataTable.Rows[0]["?S"]);
        Assert.IsTrue(dataTable.Rows[0].IsNull("?P"));
    }

    [TestMethod]
    public void ShouldImportFromDataTable()
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("?s", typeof(string));
        dataTable.Columns.Add("?p", typeof(string));
        dataTable.Rows.Add("ex:s", DBNull.Value);

        RDFTable table = RDFTable.FromDataTable(dataTable);

        Assert.AreEqual(2, table.ColumnsCount);
        Assert.AreEqual("?S", table.Columns[0].Name);
        Assert.AreEqual("?P", table.Columns[1].Name);
        Assert.AreEqual(1, table.RowsCount);
        Assert.AreEqual("ex:s", table.Rows[0]["?S"]);
        Assert.IsTrue(table.Rows[0].IsUnbound("?P"));
    }

    [TestMethod]
    public void ShouldImportFromDataTableProjectingNonStringValues()
    {
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("?N", typeof(int));
        dataTable.Rows.Add(42);

        RDFTable table = RDFTable.FromDataTable(dataTable);

        Assert.AreEqual("42", table.Rows[0]["?N"]);
    }

    [TestMethod]
    public void ShouldThrowImportingNullDataTable()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFTable.FromDataTable(null));

    [TestMethod]
    public void ShouldRoundTripThroughDataTable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?S");
        table.AddColumn("?P");
        table.AddRow(new[] { "ex:s", "ex:p" });
        table.AddRow(new[] { "ex:s2", (string)null });

        RDFTable roundTripped = RDFTable.FromDataTable(table.ToDataTable());

        Assert.AreEqual(table.ColumnsCount, roundTripped.ColumnsCount);
        Assert.AreEqual(table.RowsCount, roundTripped.RowsCount);
        Assert.AreEqual("ex:s", roundTripped.Rows[0]["?S"]);
        Assert.AreEqual("ex:p", roundTripped.Rows[0]["?P"]);
        Assert.AreEqual("ex:s2", roundTripped.Rows[1]["?S"]);
        Assert.IsTrue(roundTripped.Rows[1].IsUnbound("?P"));
    }
    #endregion
}
