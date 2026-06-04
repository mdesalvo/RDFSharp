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
public class RDFDistinctModifierTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateDistinctModifier()
    {
        RDFDistinctModifier modifier = new RDFDistinctModifier();

        Assert.IsNotNull(modifier);
        Assert.IsFalse(modifier.IsEvaluable);
        Assert.IsTrue(modifier.ToString().Equals("DISTINCT", StringComparison.Ordinal));
        Assert.IsNotNull(modifier.QueryMemberStringID);
        Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
        Assert.IsTrue(modifier.Equals(modifier));
        Assert.IsFalse(modifier.Equals(new RDFDistinctModifier()));
    }

    [TestMethod]
    public void ShouldApplyDistinctModifier()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string>
        {
            ["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(),
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

        RDFTable table2 = new RDFDistinctModifier().ApplyModifier(table);

        Assert.IsNotNull(table2);
        Assert.AreEqual(3, table2.ColumnsCount);
        Assert.IsTrue(table2.HasColumn("?A"));
        Assert.IsTrue(table2.HasColumn("?B"));
        Assert.IsTrue(table2.HasColumn("?C"));
        Assert.AreEqual(2, table2.RowsCount);
        Assert.IsTrue(table2.Rows[0]["?A"].Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(table2.Rows[0]["?B"].Equals(new RDFPlainLiteral("hello", "en-US").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(table2.Rows[0]["?C"].Equals(new RDFResource("ex:value0").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(table2.Rows[1]["?A"].Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString(), StringComparison.Ordinal));
        Assert.IsTrue(table2.Rows[1]["?B"].Equals(new RDFPlainLiteral("hello", "en").ToString(), StringComparison.Ordinal));
        Assert.IsTrue(table2.Rows[1].IsUnbound("?C"));
    }
    #endregion
}
