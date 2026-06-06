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
public class RDFOrderByModifierTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateOrderByModifierAscending()
    {
        RDFOrderByModifier modifier = new RDFOrderByModifier(new RDFVariable("?VAR"), RDFQueryEnums.RDFOrderByFlavors.ASC);

        Assert.IsNotNull(modifier);
        Assert.IsTrue(modifier.Variable.Equals(new RDFVariable("?VAR")));
        Assert.IsTrue(modifier.OrderByFlavor.Equals(RDFQueryEnums.RDFOrderByFlavors.ASC));
        Assert.IsFalse(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("ASC(?VAR)", StringComparison.Ordinal));
        Assert.IsNotNull(modifier.QueryMemberStringID);
        Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
        Assert.IsTrue(modifier.Equals(modifier));
        Assert.IsFalse(modifier.Equals(new RDFDistinctModifier()));
    }

    [TestMethod]
    public void ShouldCreateOrderByModifierDescending()
    {
        RDFOrderByModifier modifier = new RDFOrderByModifier(new RDFVariable("?VAR"), RDFQueryEnums.RDFOrderByFlavors.DESC);

        Assert.IsNotNull(modifier);
        Assert.IsTrue(modifier.Variable.Equals(new RDFVariable("?VAR")));
        Assert.IsTrue(modifier.OrderByFlavor.Equals(RDFQueryEnums.RDFOrderByFlavors.DESC));
        Assert.IsFalse(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("DESC(?VAR)", StringComparison.Ordinal));
        Assert.IsNotNull(modifier.QueryMemberStringID);
        Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
        Assert.IsTrue(modifier.Equals(modifier));
        Assert.IsFalse(modifier.Equals(new RDFDistinctModifier()));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingOrderByModifierBecauseNullVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFOrderByModifier(null, RDFQueryEnums.RDFOrderByFlavors.ASC));

    [TestMethod]
    public void ShouldApplyOrderByModifierAscending()
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

        RDFTable orderedTable = new RDFOrderByModifier(new RDFVariable("?A"), RDFQueryEnums.RDFOrderByFlavors.ASC)
            .ApplyModifier(table);

        Assert.IsNotNull(orderedTable);
        Assert.AreEqual(3, orderedTable.ColumnsCount);
        Assert.IsTrue(orderedTable.HasColumn("?A"));
        Assert.IsTrue(orderedTable.HasColumn("?B"));
        Assert.IsTrue(orderedTable.HasColumn("?C"));
        Assert.AreEqual(3, orderedTable.RowsCount);
        Assert.IsTrue(orderedTable.Rows[0]["?A"].Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[0]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[0]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?A"].Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?B"].Equals(new RDFPlainLiteral("hello", "en").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1].IsUnbound("?C"));
        Assert.IsTrue(orderedTable.Rows[2]["?A"].Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyOrderByModifierDescending()
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

        RDFTable orderedTable = new RDFOrderByModifier(new RDFVariable("?A"), RDFQueryEnums.RDFOrderByFlavors.DESC)
            .ApplyModifier(table);

        Assert.IsNotNull(orderedTable);
        Assert.AreEqual(3, orderedTable.ColumnsCount);
        Assert.IsTrue(orderedTable.HasColumn("?A"));
        Assert.IsTrue(orderedTable.HasColumn("?B"));
        Assert.IsTrue(orderedTable.HasColumn("?C"));
        Assert.AreEqual(3, orderedTable.RowsCount);
        Assert.IsTrue(orderedTable.Rows[0]["?A"].Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[0]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[0]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?A"].Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?B"].Equals(new RDFPlainLiteral("hello", "en").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1].IsUnbound("?C"));
        Assert.IsTrue(orderedTable.Rows[2]["?A"].Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyTwoOrderByModifiers()
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
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
            ["?B"] = new RDFPlainLiteral("hello", "en").ToString()
        });

        //Compound ORDER BY ?A ASC, ?B DESC (the live engine composes all sort keys in the projection step)
        RDFTable orderedTable = RDFTableEngine.SortTable(table, [
            (new RDFVariable("?A").ToString(), false),
            (new RDFVariable("?B").ToString(), true)
        ]);

        Assert.IsNotNull(orderedTable);
        Assert.AreEqual(3, orderedTable.ColumnsCount);
        Assert.IsTrue(orderedTable.HasColumn("?A"));
        Assert.IsTrue(orderedTable.HasColumn("?B"));
        Assert.IsTrue(orderedTable.HasColumn("?C"));
        Assert.AreEqual(3, orderedTable.RowsCount);
        Assert.IsTrue(orderedTable.Rows[0]["?A"].Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[0]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[0]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?A"].Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?B"].Equals(new RDFPlainLiteral("hello", "en").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1].IsUnbound("?C"));
        Assert.IsTrue(orderedTable.Rows[2]["?A"].Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyOrderByModifierWithUnexistingVariable()
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

        //ORDER BY on a variable absent from the table is a no-op: rows keep their original order
        RDFTable orderedTable = new RDFOrderByModifier(new RDFVariable("?D"), RDFQueryEnums.RDFOrderByFlavors.DESC)
            .ApplyModifier(table);

        Assert.IsNotNull(orderedTable);
        Assert.AreEqual(3, orderedTable.ColumnsCount);
        Assert.IsTrue(orderedTable.HasColumn("?A"));
        Assert.IsTrue(orderedTable.HasColumn("?B"));
        Assert.IsTrue(orderedTable.HasColumn("?C"));
        Assert.AreEqual(3, orderedTable.RowsCount);
        Assert.IsTrue(orderedTable.Rows[0]["?A"].Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[0]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[0]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?A"].Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[1]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2]["?A"].Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2]["?B"].Equals(new RDFPlainLiteral("hello", "en").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(orderedTable.Rows[2].IsUnbound("?C"));
    }
    #endregion
}
