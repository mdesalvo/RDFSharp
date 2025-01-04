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
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFAvgAggregatorTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateAvgAggregator()
        {
            RDFAvgAggregator aggregator = new RDFAvgAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
            Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
            Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
            Assert.IsFalse(aggregator.IsDistinct);
            Assert.IsTrue(aggregator.ToString().Equals("(AVG(?AGGVAR) AS ?PROJVAR)"));
            Assert.IsNotNull(aggregator.AggregatorContext);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAvgAggregatorBecauseNullAggregatorVariable()
            =>  Assert.ThrowsException<RDFQueryException>(() => new RDFAvgAggregator(null as RDFVariable, new RDFVariable("?PROJVAR")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAvgAggregatorBecauseNullPartitionVariable()
            =>  Assert.ThrowsException<RDFQueryException>(() => new RDFAvgAggregator(new RDFVariable("?AGGVAR"), null as RDFVariable));

        [TestMethod]
        public void ShouldCreateDistinctAvgAggregator()
        {
            RDFAvgAggregator aggregator = new RDFAvgAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"))
                                                .Distinct() as RDFAvgAggregator;

            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
            Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
            Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
            Assert.IsTrue(aggregator.IsDistinct);
            Assert.IsTrue(aggregator.ToString().Equals("(AVG(DISTINCT ?AGGVAR) AS ?PROJVAR)"));
            Assert.IsNotNull(aggregator.AggregatorContext);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
        }

        [TestMethod]
        public void ShouldApplyModifierWithAvgAggregator()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            modifier.AddAggregator(new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVGPROJ")));
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?AVGPROJ");
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?AVGPROJ"].ToString().Equals($"26.5^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
            Assert.IsTrue(result.Rows[1]["?AVGPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithDistinctAvgAggregator()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            modifier.AddAggregator(new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVGPROJ")).Distinct());
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?AVGPROJ");
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?AVGPROJ"].ToString().Equals($"27^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
            Assert.IsTrue(result.Rows[1]["?AVGPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithAvgAggregatorAndHavingClause()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            RDFAvgAggregator aggregator = new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVGPROJ"))
                                                    .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("25.99", RDFModelEnums.RDFDatatypes.XSD_FLOAT)) as RDFAvgAggregator;
            modifier.AddAggregator(aggregator);
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?AVGPROJ");
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?AVGPROJ"].ToString().Equals($"26.5^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(aggregator.PrintHavingClause(null).Equals($"(AVG(?A) >= \"25.99\"^^<{RDFVocabulary.XSD.FLOAT}>)"));
            Assert.IsTrue(aggregator.PrintHavingClause([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals($"(AVG(?A) >= \"25.99\"^^xsd:float)"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithAvgAggregatorOperatingOnNonNumericValues()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?A"] = new RDFTypedLiteral("2022-09-04Z", RDFModelEnums.RDFDatatypes.XSD_DATE).ToString();
            row3["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row3["?C"] = new RDFResource("ex:value2").ToString();
            table.Rows.Add(row3);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            modifier.AddAggregator(new RDFAvgAggregator(new RDFVariable("?A"), new RDFVariable("?AVGPROJ")));
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?AVGPROJ");
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?AVGPROJ"].ToString().Equals($"26.85^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
            Assert.IsTrue(result.Rows[1]["?AVGPROJ"].ToString().Equals($"25^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(result.Rows[2]["?C"].ToString().Equals("ex:value2"));
            Assert.IsTrue(result.Rows[2]["?AVGPROJ"].ToString().Equals(string.Empty)); //Projection for NaN
        }
        #endregion
    }
}