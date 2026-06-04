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
public class RDFOffsetModifierTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateOffsetModifier()
    {
        RDFOffsetModifier modifier = new RDFOffsetModifier(25);

        Assert.IsNotNull(modifier);
        Assert.AreEqual(25, modifier.Offset);
        Assert.IsFalse(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("OFFSET 25", StringComparison.Ordinal));
        Assert.IsNotNull(modifier.QueryMemberStringID);
        Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
        Assert.IsTrue(modifier.Equals(modifier));
        Assert.IsFalse(modifier.Equals(new RDFOffsetModifier(25)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingOffsetModifierBecauseNegativeValue()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFOffsetModifier(-1));

    [TestMethod]
    public void ShouldApplyOffsetModifier()
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

        RDFTable offsetTable = new RDFOffsetModifier(1).ApplyModifier(table);

        Assert.IsNotNull(offsetTable);
        Assert.AreEqual(3, offsetTable.ColumnsCount);
        Assert.IsTrue(offsetTable.HasColumn("?A"));
        Assert.IsTrue(offsetTable.HasColumn("?B"));
        Assert.IsTrue(offsetTable.HasColumn("?C"));
        Assert.AreEqual(2, offsetTable.RowsCount);
        Assert.IsTrue(offsetTable.Rows[0]["?A"].Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(offsetTable.Rows[0]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(offsetTable.Rows[0]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(offsetTable.Rows[1]["?A"].Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(offsetTable.Rows[1]["?B"].Equals(new RDFPlainLiteral("hello", "en").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(offsetTable.Rows[1].IsUnbound("?C"));
    }

    [TestMethod]
    public void ShouldApplyOffsetExceedingModifier()
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

        RDFTable offsetTable = new RDFOffsetModifier(4).ApplyModifier(table);

        Assert.IsNotNull(offsetTable);
        Assert.AreEqual(3, offsetTable.ColumnsCount);
        Assert.IsTrue(offsetTable.HasColumn("?A"));
        Assert.IsTrue(offsetTable.HasColumn("?B"));
        Assert.IsTrue(offsetTable.HasColumn("?C"));
        Assert.AreEqual(0, offsetTable.RowsCount);
    }

    [TestMethod]
    public void ShouldApplyOffsetModifierToEmptyTable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");

        RDFTable offsetTable = new RDFOffsetModifier(2).ApplyModifier(table);

        Assert.IsNotNull(offsetTable);
        Assert.AreEqual(3, offsetTable.ColumnsCount);
        Assert.IsTrue(offsetTable.HasColumn("?A"));
        Assert.IsTrue(offsetTable.HasColumn("?B"));
        Assert.IsTrue(offsetTable.HasColumn("?C"));
        Assert.AreEqual(0, offsetTable.RowsCount);
    }
    #endregion
}
