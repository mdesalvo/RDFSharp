/*
   Copyright 2012-2025 Marco De Salvo

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
    public class RDFDistinctModifierTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateDistinctModifier()
        {
            RDFDistinctModifier modifier = new RDFDistinctModifier();

            Assert.IsNotNull(modifier);
            Assert.IsFalse(modifier.IsEvaluable);
            Assert.IsTrue(modifier.ToString().Equals("DISTINCT"));
            Assert.IsNotNull(modifier.QueryMemberStringID);
            Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
            Assert.IsTrue(modifier.Equals(modifier));
            Assert.IsFalse(modifier.Equals(new RDFDistinctModifier()));
        }

        [TestMethod]
        public void ShouldApplyDistinctModifier()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));            
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
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

            DataTable table2 = new RDFDistinctModifier().ApplyModifier(table);

            Assert.IsNotNull(table2);
            Assert.IsTrue(table2.Columns.Count == 3);
            Assert.IsTrue(table2.Columns.Contains("?A"));
            Assert.IsTrue(table2.Columns.Contains("?B"));
            Assert.IsTrue(table2.Columns.Contains("?C"));
            Assert.IsTrue(table2.Rows.Count == 2);
            Assert.IsTrue(table2.Rows[0]["?A"].ToString().Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(table2.Rows[0]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(table2.Rows[0]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
            Assert.IsTrue(table2.Rows[1]["?A"].ToString().Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(table2.Rows[1]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en").ToString()));
            Assert.IsTrue(table2.Rows[1]["?C"].ToString().Equals(DBNull.Value.ToString()));
        }
        #endregion
    }
}