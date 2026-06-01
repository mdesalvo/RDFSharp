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
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFTableRowTest
{
    #region Utilities
    private static RDFTableRow BuildRow(params string[] cells)
    {
        Dictionary<string, int> ordinals = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            { "?S", 0 },
            { "?P", 1 },
            { "?O", 2 }
        };
        return new RDFTableRow(cells, ordinals);
    }
    #endregion

    #region Tests
    [TestMethod]
    public void ShouldAccessCellByColumnName()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.AreEqual("ex:s", row["?S"]);
        Assert.AreEqual("ex:p", row["?P"]);
        Assert.AreEqual("ex:o", row["?O"]);
    }

    [TestMethod]
    public void ShouldAccessCellByColumnNameCaseInsensitively()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.AreEqual("ex:s", row[" ?s "]);
        Assert.AreEqual("ex:p", row["?p"]);
    }

    [TestMethod]
    public void ShouldAccessCellByOrdinal()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.AreEqual("ex:s", row[0]);
        Assert.AreEqual("ex:p", row[1]);
        Assert.AreEqual("ex:o", row[2]);
    }

    [TestMethod]
    public void ShouldTreatNullCellAsUnbound()
    {
        RDFTableRow row = BuildRow("ex:s", null, "ex:o");

        Assert.IsNull(row["?P"]);
        Assert.IsTrue(row.IsUnbound("?P"));
        Assert.IsFalse(row.IsBound("?P"));
    }

    [TestMethod]
    public void ShouldTreatNonNullCellAsBound()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.IsTrue(row.IsBound("?S"));
        Assert.IsFalse(row.IsUnbound("?S"));
    }

    [TestMethod]
    public void ShouldThrowAccessingUnknownColumnByName()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.ThrowsExactly<RDFQueryException>(() => { _ = row["?X"]; });
    }

    [TestMethod]
    public void ShouldThrowCheckingUnboundOnUnknownColumn()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.ThrowsExactly<RDFQueryException>(() => row.IsUnbound("?X"));
    }

    [TestMethod]
    public void ShouldThrowCheckingBoundOnUnknownColumn()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.ThrowsExactly<RDFQueryException>(() => row.IsBound("?X"));
    }

    [TestMethod]
    public void ShouldThrowAccessingNullColumnByName()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.ThrowsExactly<RDFQueryException>(() => { _ = row[null]; });
    }

    [TestMethod]
    public void ShouldThrowCheckingUnboundOnNullColumn()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.ThrowsExactly<RDFQueryException>(() => row.IsUnbound(null));
    }

    [TestMethod]
    public void ShouldThrowCheckingBoundOnNullColumn()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.ThrowsExactly<RDFQueryException>(() => row.IsBound(null));
    }

    [TestMethod]
    public void ShouldThrowAccessingWhitespaceColumnByName()
    {
        RDFTableRow row = BuildRow("ex:s", "ex:p", "ex:o");

        Assert.ThrowsExactly<RDFQueryException>(() => { _ = row["   "]; });
    }
    #endregion
}
