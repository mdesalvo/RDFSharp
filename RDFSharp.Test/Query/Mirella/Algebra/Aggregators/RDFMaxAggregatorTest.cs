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
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFMaxAggregatorTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)]
        [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)]
        public void ShouldCreateMaxAggregator(RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor)
        {
            RDFMaxAggregator aggregator = new RDFMaxAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"), aggregatorFlavor);

            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
            Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
            Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
            Assert.IsFalse(aggregator.IsDistinct);
            Assert.IsTrue(aggregator.AggregatorFlavor == aggregatorFlavor);
            Assert.IsTrue(aggregator.ToString().Equals("(MAX(?AGGVAR) AS ?PROJVAR)"));
            Assert.IsNotNull(aggregator.AggregatorContext);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
        }

        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)]
        [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)]
        public void ShouldThrowExceptionOnCreatingStringMaxAggregatorBecauseNullAggregatorVariable(RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor)
            =>  Assert.ThrowsException<RDFQueryException>(() => new RDFMaxAggregator(null as RDFVariable, new RDFVariable("?PROJVAR"), aggregatorFlavor));

        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)]
        [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)]
        public void ShouldThrowExceptionOnCreatingStringMaxAggregatorBecauseNullPartitionVariable(RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor)
            =>  Assert.ThrowsException<RDFQueryException>(() => new RDFMaxAggregator(new RDFVariable("?AGGVAR"), null as RDFVariable, aggregatorFlavor));

        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)]
        [DataRow(RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)]
        public void ShouldCreateDistinctMaxAggregator(RDFQueryEnums.RDFMinMaxAggregatorFlavors aggregatorFlavor)
        {
            RDFMaxAggregator aggregator = new RDFMaxAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"), aggregatorFlavor)
                                                .Distinct() as RDFMaxAggregator;

            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
            Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
            Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
            Assert.IsTrue(aggregator.IsDistinct);
            Assert.IsTrue(aggregator.AggregatorFlavor == aggregatorFlavor);
            Assert.IsTrue(aggregator.ToString().Equals("(MAX(DISTINCT ?AGGVAR) AS ?PROJVAR)"));
            Assert.IsNotNull(aggregator.AggregatorContext);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
        }

        [TestMethod]
        public void ShouldApplyModifierWithMaxAggregatorString()
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
            DataRow row3 = table.NewRow();
            row3["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row3["?B"] = new RDFPlainLiteral("hello", "en-UK").ToString();
            row3["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row3);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            modifier.AddAggregator(new RDFMaxAggregator(new RDFVariable("?B"), new RDFVariable("?MAXPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.String));
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?MAXPROJ");
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?MAXPROJ"].ToString().Equals("hello@EN-US"));
            Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
            Assert.IsTrue(result.Rows[1]["?MAXPROJ"].ToString().Equals("hello@EN-US"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithDistinctMaxAggregatorString()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFResource("http://example.org/test/test1").ToString();
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
            DataRow row3 = table.NewRow();
            row3["?A"] = new RDFTypedLiteral("29", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row3["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row3["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row3);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            modifier.AddAggregator(new RDFMaxAggregator(new RDFVariable("?B"), new RDFVariable("?MAXPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.String).Distinct());
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?MAXPROJ");
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?MAXPROJ"].ToString().Equals("http://example.org/test/test1"));
            Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
            Assert.IsTrue(result.Rows[1]["?MAXPROJ"].ToString().Equals("hello@EN-US"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithMaxAggregatorStringAndHavingClause()
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
            row1["?B"] = new RDFPlainLiteral("hello", "en").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            RDFMaxAggregator aggregator = new RDFMaxAggregator(new RDFVariable("?B"), new RDFVariable("?MAXPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.String)
                                                    .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFPlainLiteral("hello", "en-US")) as RDFMaxAggregator;
            modifier.AddAggregator(aggregator);
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?MAXPROJ");
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?MAXPROJ"].ToString().Equals("hello@EN-US"));
            Assert.IsTrue(aggregator.PrintHavingClause(null).Equals($"(MAX(?B) = \"hello\"@EN-US)"));
            Assert.IsTrue(aggregator.PrintHavingClause([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals($"(MAX(?B) = \"hello\"@EN-US)"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithMaxAggregatorNumeric()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("25.114", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?A"] = new RDFTypedLiteral("22.47", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString();
            row3["?B"] = new RDFPlainLiteral("hello", "en-UK").ToString();
            row3["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row3);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            modifier.AddAggregator(new RDFMaxAggregator(new RDFVariable("?A"), new RDFVariable("?MAXPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric));
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?MAXPROJ");
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?MAXPROJ"].ToString().Equals($"27.5^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
            Assert.IsTrue(result.Rows[1]["?MAXPROJ"].ToString().Equals($"25.114^^{RDFVocabulary.XSD.DOUBLE}"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithDistinctMaxAggregatorNumeric()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27.0", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFResource("http://example.org/test/test1").ToString();
            row0["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("27.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?A"] = new RDFTypedLiteral("29", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row3["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row3["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row3);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            modifier.AddAggregator(new RDFMaxAggregator(new RDFVariable("?A"), new RDFVariable("?MAXPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric).Distinct());
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?MAXPROJ");
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?MAXPROJ"].ToString().Equals($"27^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
            Assert.IsTrue(result.Rows[1]["?MAXPROJ"].ToString().Equals($"29^^{RDFVocabulary.XSD.DOUBLE}"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithMaxAggregatorNumericAndHavingClause()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("28.24", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFTypedLiteral("28", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER).ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            RDFMaxAggregator aggregator = new RDFMaxAggregator(new RDFVariable("?A"), new RDFVariable("?MAXPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric)
                                                    .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFTypedLiteral("28", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)) as RDFMaxAggregator;
            modifier.AddAggregator(aggregator);
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?MAXPROJ");
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?MAXPROJ"].ToString().Equals($"28.24^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(aggregator.PrintHavingClause(null).Equals($"(MAX(?A) > \"28\"^^<{RDFVocabulary.XSD.POSITIVE_INTEGER}>)"));
            Assert.IsTrue(aggregator.PrintHavingClause([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals($"(MAX(?A) > \"28\"^^xsd:positiveInteger)"));
        }

        [TestMethod]
        public void ShouldApplyModifierWithMaxAggregatorNumericOnNonNumericValues()
        {
            DataTable table = new DataTable();
            table.Columns.Add("?A", typeof(string));
            table.Columns.Add("?B", typeof(string));
            table.Columns.Add("?C", typeof(string));
            DataRow row0 = table.NewRow();
            row0["?A"] = new RDFTypedLiteral("27.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString();
            row0["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row0["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?A"] = new RDFResource("ex:value0").ToString();
            row1["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row1["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?A"] = new RDFTypedLiteral("26", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            row2["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
            row2["?C"] = new RDFResource("ex:value1").ToString();
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?A"] = new RDFTypedLiteral("22.47", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString();
            row3["?B"] = new RDFPlainLiteral("hello", "en-UK").ToString();
            row3["?C"] = new RDFResource("ex:value0").ToString();
            table.Rows.Add(row3);
            table.AcceptChanges();

            RDFGroupByModifier modifier = new RDFGroupByModifier([new RDFVariable("?C")]);
            modifier.AddAggregator(new RDFMaxAggregator(new RDFVariable("?A"), new RDFVariable("?MAXPROJ"), RDFQueryEnums.RDFMinMaxAggregatorFlavors.Numeric));
            DataTable result = modifier.ApplyModifier(table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Columns[0].ColumnName == "?C");
            Assert.IsTrue(result.Columns[1].ColumnName == "?MAXPROJ");
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(result.Rows[0]["?C"].ToString().Equals("ex:value1"));
            Assert.IsTrue(result.Rows[0]["?MAXPROJ"].ToString().Equals($"27.5^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(result.Rows[1]["?C"].ToString().Equals("ex:value0"));
            Assert.IsTrue(result.Rows[1]["?MAXPROJ"].ToString().Equals(string.Empty));
        }
        #endregion
    }
}