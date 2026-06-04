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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFLimitModifierTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateLimitModifier()
    {
        RDFLimitModifier modifier = new RDFLimitModifier(25);

        Assert.IsNotNull(modifier);
        Assert.AreEqual(25, modifier.Limit);
        Assert.IsFalse(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("LIMIT 25", StringComparison.Ordinal));
        Assert.IsNotNull(modifier.QueryMemberStringID);
        Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
        Assert.IsTrue(modifier.Equals(modifier));
        Assert.IsFalse(modifier.Equals(new RDFLimitModifier(25)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLimitModifierBecauseNegativeValue()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLimitModifier(-1));

    [TestMethod]
    public void ShouldApplyLimitModifier()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en").ToString()
        });

        RDFTable limitedTable = new RDFLimitModifier(1).ApplyModifier(table);

        Assert.IsNotNull(limitedTable);
        Assert.AreEqual(3, limitedTable.ColumnsCount);
        Assert.IsTrue(limitedTable.HasColumn("?A"));
        Assert.IsTrue(limitedTable.HasColumn("?B"));
        Assert.IsTrue(limitedTable.HasColumn("?C"));
        Assert.AreEqual(1, limitedTable.RowsCount);
        Assert.IsTrue(limitedTable.Rows[0]["?A"].Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(limitedTable.Rows[0]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(limitedTable.Rows[0]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyLimitZeroModifier()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en-US").ToString(),
            ["?C"] = new RDFResource("ex:value0").ToString()
        });
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en").ToString()
        });

        RDFTable limitedTable = new RDFLimitModifier(0).ApplyModifier(table);

        Assert.IsNotNull(limitedTable);
        Assert.AreEqual(3, limitedTable.ColumnsCount);
        Assert.IsTrue(limitedTable.HasColumn("?A"));
        Assert.IsTrue(limitedTable.HasColumn("?B"));
        Assert.IsTrue(limitedTable.HasColumn("?C"));
        Assert.AreEqual(0, limitedTable.RowsCount);
    }

    [TestMethod]
    public void ShouldApplyLimitModifierToEmptyTable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");

        RDFTable limitedTable = new RDFLimitModifier(35).ApplyModifier(table);

        Assert.IsNotNull(limitedTable);
        Assert.AreEqual(3, limitedTable.ColumnsCount);
        Assert.IsTrue(limitedTable.HasColumn("?A"));
        Assert.IsTrue(limitedTable.HasColumn("?B"));
        Assert.IsTrue(limitedTable.HasColumn("?C"));
        Assert.AreEqual(0, limitedTable.RowsCount);
    }
    #endregion
}
