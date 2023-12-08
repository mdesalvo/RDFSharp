/*
   Copyright 2012-2024 Marco De Salvo

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
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFLimitModifierTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateLimitModifier()
        {
            RDFLimitModifier modifier = new RDFLimitModifier(25);

            Assert.IsNotNull(modifier);
            Assert.IsTrue(modifier.Limit == 25);
            Assert.IsFalse(modifier.IsEvaluable);
            Assert.IsTrue(modifier.ToString().Equals("LIMIT 25"));
            Assert.IsNotNull(modifier.QueryMemberStringID);
            Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
            Assert.IsTrue(modifier.Equals(modifier));
            Assert.IsFalse(modifier.Equals(new RDFLimitModifier(25)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingLimitModifierBecauseNegativeValue()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFLimitModifier(-1));

        [TestMethod]
        public void ShouldApplyLimitModifier()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));            
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
            row2["?C"] = DBNull.Value;
            table.Rows.Add(row2);
            table.AcceptChanges();
            table.DefaultView.Sort = "?A DESC";

            DataTable limitedTable = new RDFLimitModifier(1).ApplyModifier(table);

            Assert.IsNotNull(limitedTable);
            Assert.IsTrue(limitedTable.Columns.Count == 3);
            Assert.IsTrue(limitedTable.Columns.Contains("?A"));
            Assert.IsTrue(limitedTable.Columns.Contains("?B"));
            Assert.IsTrue(limitedTable.Columns.Contains("?C"));
            Assert.IsTrue(limitedTable.Rows.Count == 1);
            Assert.IsTrue(limitedTable.Rows[0]["?A"].ToString().Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(limitedTable.Rows[0]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(limitedTable.Rows[0]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
            Assert.IsTrue(limitedTable.DefaultView.Sort.Equals("?A DESC"));
        }

        [TestMethod]
        public void ShouldApplyLimitZeroModifier()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));            
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
            row2["?C"] = DBNull.Value;
            table.Rows.Add(row2);
            table.AcceptChanges();
            table.DefaultView.Sort = "?A DESC";

            DataTable limitedTable = new RDFLimitModifier(0).ApplyModifier(table);

            Assert.IsNotNull(limitedTable);
            Assert.IsTrue(limitedTable.Columns.Count == 3);
            Assert.IsTrue(limitedTable.Columns.Contains("?A"));
            Assert.IsTrue(limitedTable.Columns.Contains("?B"));
            Assert.IsTrue(limitedTable.Columns.Contains("?C"));
            Assert.IsTrue(limitedTable.Rows.Count == 0);
            Assert.IsTrue(limitedTable.DefaultView.Sort.Equals("?A DESC"));
        }

        [TestMethod]
        public void ShouldApplyLimitModifierToEmptyTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));            
            table.AcceptChanges();
            table.DefaultView.Sort = "?A DESC";

            DataTable limitedTable = new RDFLimitModifier(35).ApplyModifier(table);

            Assert.IsNotNull(limitedTable);
            Assert.IsTrue(limitedTable.Columns.Count == 3);
            Assert.IsTrue(limitedTable.Columns.Contains("?A"));
            Assert.IsTrue(limitedTable.Columns.Contains("?B"));
            Assert.IsTrue(limitedTable.Columns.Contains("?C"));
            Assert.IsTrue(limitedTable.Rows.Count == 0);
            Assert.IsTrue(limitedTable.DefaultView.Sort.Equals("?A DESC"));
        }
        #endregion
    }
}