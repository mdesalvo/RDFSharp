/*
   Copyright 2012-2023 Marco De Salvo

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
            Assert.IsTrue(modifier.ToString().Equals("ASC(?VAR)"));
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
            Assert.IsTrue(modifier.ToString().Equals("DESC(?VAR)"));
            Assert.IsNotNull(modifier.QueryMemberStringID);
            Assert.IsTrue(modifier.QueryMemberID.Equals(RDFModelUtilities.CreateHash(modifier.QueryMemberStringID)));
            Assert.IsTrue(modifier.Equals(modifier));
            Assert.IsFalse(modifier.Equals(new RDFDistinctModifier()));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingOrderByModifierBecauseNullVariable()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOrderByModifier(null, RDFQueryEnums.RDFOrderByFlavors.ASC));

        [TestMethod]
        public void ShouldApplyOrderByModifierAscending()
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

            DataTable orderedTable = new RDFOrderByModifier(new RDFVariable("?A"), RDFQueryEnums.RDFOrderByFlavors.ASC)
                                            .ApplyModifier(table);

            Assert.IsTrue(orderedTable.DefaultView.Sort.Equals("?A ASC"));

            //Finalize table (simulate Mirella engine)
            orderedTable = orderedTable.DefaultView.ToTable();

            Assert.IsNotNull(orderedTable);
            Assert.IsTrue(orderedTable.Columns.Count == 3);
            Assert.IsTrue(orderedTable.Columns.Contains("?A"));
            Assert.IsTrue(orderedTable.Columns.Contains("?B"));
            Assert.IsTrue(orderedTable.Columns.Contains("?C"));
            Assert.IsTrue(orderedTable.Rows.Count == 3);
            Assert.IsTrue(orderedTable.Rows[0]["?A"].ToString().Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[0]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(orderedTable.Rows[0]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?A"].ToString().Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en").ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?C"].ToString().Equals(string.Empty));
            Assert.IsTrue(orderedTable.Rows[2]["?A"].ToString().Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[2]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(orderedTable.Rows[2]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
        }

        [TestMethod]
        public void ShouldApplyOrderByModifierDescending()
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

            DataTable orderedTable = new RDFOrderByModifier(new RDFVariable("?A"), RDFQueryEnums.RDFOrderByFlavors.DESC)
                                            .ApplyModifier(table);

            Assert.IsTrue(orderedTable.DefaultView.Sort.Equals("?A DESC"));

            //Finalize table (simulate Mirella engine)
            orderedTable = orderedTable.DefaultView.ToTable();

            Assert.IsNotNull(orderedTable);
            Assert.IsTrue(orderedTable.Columns.Count == 3);
            Assert.IsTrue(orderedTable.Columns.Contains("?A"));
            Assert.IsTrue(orderedTable.Columns.Contains("?B"));
            Assert.IsTrue(orderedTable.Columns.Contains("?C"));
            Assert.IsTrue(orderedTable.Rows.Count == 3);
            Assert.IsTrue(orderedTable.Rows[0]["?A"].ToString().Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[0]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(orderedTable.Rows[0]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?A"].ToString().Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en").ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?C"].ToString().Equals(string.Empty));
            Assert.IsTrue(orderedTable.Rows[2]["?A"].ToString().Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[2]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(orderedTable.Rows[2]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
        }

        [TestMethod]
        public void ShouldApplyTwoOrderByModifiers()
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
            row2["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
            row2["?C"] = DBNull.Value;
            table.Rows.Add(row2);
            table.AcceptChanges();

            DataTable orderedTableA = new RDFOrderByModifier(new RDFVariable("?A"), RDFQueryEnums.RDFOrderByFlavors.ASC)
                                            .ApplyModifier(table);

            Assert.IsTrue(orderedTableA.DefaultView.Sort.Equals("?A ASC"));

            DataTable orderedTableB = new RDFOrderByModifier(new RDFVariable("?B"), RDFQueryEnums.RDFOrderByFlavors.DESC)
                                            .ApplyModifier(orderedTableA);

            Assert.IsTrue(orderedTableA.DefaultView.Sort.Equals("?A ASC, ?B DESC"));
            
            //Finalize table (simulate Mirella engine)
            orderedTableB = orderedTableB.DefaultView.ToTable();

            Assert.IsNotNull(orderedTableB);
            Assert.IsTrue(orderedTableB.Columns.Count == 3);
            Assert.IsTrue(orderedTableB.Columns.Contains("?A"));
            Assert.IsTrue(orderedTableB.Columns.Contains("?B"));
            Assert.IsTrue(orderedTableB.Columns.Contains("?C"));
            Assert.IsTrue(orderedTableB.Rows.Count == 3);
            Assert.IsTrue(orderedTableB.Rows[0]["?A"].ToString().Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTableB.Rows[0]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(orderedTableB.Rows[0]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
            Assert.IsTrue(orderedTableB.Rows[1]["?A"].ToString().Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTableB.Rows[1]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en").ToString()));
            Assert.IsTrue(orderedTableB.Rows[1]["?C"].ToString().Equals(string.Empty));
            Assert.IsTrue(orderedTableB.Rows[2]["?A"].ToString().Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTableB.Rows[2]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(orderedTableB.Rows[2]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
        }

        [TestMethod]
        public void ShouldApplyOrderByModifierWithUnexistingVariable()
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

            DataTable orderedTable = new RDFOrderByModifier(new RDFVariable("?D"), RDFQueryEnums.RDFOrderByFlavors.DESC)
                                            .ApplyModifier(table);

            Assert.IsTrue(string.IsNullOrEmpty(orderedTable.DefaultView.Sort));

            //Finalize table (simulate Mirella engine)
            orderedTable = orderedTable.DefaultView.ToTable();

            Assert.IsNotNull(orderedTable);
            Assert.IsTrue(orderedTable.Columns.Count == 3);
            Assert.IsTrue(orderedTable.Columns.Contains("?A"));
            Assert.IsTrue(orderedTable.Columns.Contains("?B"));
            Assert.IsTrue(orderedTable.Columns.Contains("?C"));
            Assert.IsTrue(orderedTable.Rows.Count == 3);
            Assert.IsTrue(orderedTable.Rows[0]["?A"].ToString().Equals(new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[0]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(orderedTable.Rows[0]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?A"].ToString().Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en-US").ToString()));
            Assert.IsTrue(orderedTable.Rows[1]["?C"].ToString().Equals(new RDFResource("ex:value0").ToString()));
            Assert.IsTrue(orderedTable.Rows[2]["?A"].ToString().Equals(new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString()));
            Assert.IsTrue(orderedTable.Rows[2]["?B"].ToString().Equals(new RDFPlainLiteral("hello", "en").ToString()));
            Assert.IsTrue(orderedTable.Rows[2]["?C"].ToString().Equals(string.Empty));
        }
        #endregion
    }
}