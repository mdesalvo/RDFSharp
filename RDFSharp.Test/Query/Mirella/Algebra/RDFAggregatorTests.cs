/*
   Copyright 2012-2022 Marco De Salvo

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
    public class RDFAggregatorTests
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateAggregator()
        {
            RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
            Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
            Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
            Assert.IsFalse(aggregator.IsDistinct);
            Assert.IsTrue(aggregator.ToString().Equals(string.Empty));
            Assert.IsNotNull(aggregator.AggregatorContext);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAggregatorBecauseNullAggregatorVariable()
            =>  Assert.ThrowsException<RDFQueryException>(() => new RDFAggregator(null as RDFVariable, new RDFVariable("?PROJVAR")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingAggregatorBecauseNullPartitionVariable()
            =>  Assert.ThrowsException<RDFQueryException>(() => new RDFAggregator(new RDFVariable("?AGGVAR"), null as RDFVariable));
        
        [TestMethod]
        public void ShouldSetDistinct()
        {
            RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
            aggregator.Distinct();

            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
            Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
            Assert.IsTrue(aggregator.HavingClause.Equals((false, RDFQueryEnums.RDFComparisonFlavors.EqualTo, null)));
            Assert.IsTrue(aggregator.IsDistinct);
            Assert.IsTrue(aggregator.ToString().Equals(string.Empty));
            Assert.IsNotNull(aggregator.AggregatorContext);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
        }

        [DataTestMethod]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.LessThan)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.EqualTo)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan)]
        [DataRow(RDFQueryEnums.RDFComparisonFlavors.GreaterThan)]
        public void ShouldSetHavingClause(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor)
        {
            RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
            aggregator.SetHavingClause(comparisonFlavor, new RDFVariable("?X"));
            aggregator.SetHavingClause(comparisonFlavor, null); //Will not set the having clause, since null values are not allowed

            Assert.IsNotNull(aggregator);
            Assert.IsTrue(aggregator.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
            Assert.IsTrue(aggregator.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
            Assert.IsTrue(aggregator.HavingClause.Equals((true, comparisonFlavor, new RDFVariable("?X"))));
            Assert.IsFalse(aggregator.IsDistinct);
            Assert.IsTrue(aggregator.ToString().Equals(string.Empty));
            Assert.IsNotNull(aggregator.AggregatorContext);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
            Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
        }

        [TestMethod]
        public void ShouldGetRowValueAsNumber()
        {
            RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
            DataTable table = new DataTable();
            DataColumn column = new DataColumn("?AGGVAR", typeof(string));
            table.Columns.Add(column);

            DataRow row0 = table.NewRow();
            row0["?AGGVAR"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            table.Rows.Add(row0);
            table.AcceptChanges();
            double value0 = aggregator.GetRowValueAsNumber(table.Rows[0]);
            Assert.IsTrue(value0.Equals(25.0d));

            DataRow row1 = table.NewRow();
            row1["?AGGVAR"] = DBNull.Value;
            table.Rows.Add(row1);
            table.AcceptChanges();
            double value1 = aggregator.GetRowValueAsNumber(table.Rows[1]);
            Assert.IsTrue(value1.Equals(double.NaN));

            DataRow row2 = table.NewRow();
            row2["?AGGVAR"] = null;
            table.Rows.Add(row2);
            table.AcceptChanges();
            double value2 = aggregator.GetRowValueAsNumber(table.Rows[2]);
            Assert.IsTrue(value2.Equals(double.NaN));

            DataRow row3 = table.NewRow();
            row3["?AGGVAR"] = new RDFResource("ex:res").ToString();
            table.Rows.Add(row3);
            table.AcceptChanges();
            double value3 = aggregator.GetRowValueAsNumber(table.Rows[3]);
            Assert.IsTrue(value3.Equals(double.NaN));

            DataRow row4 = table.NewRow();
            row4["?AGGVAR"] = new RDFTypedLiteral("2012", RDFModelEnums.RDFDatatypes.XSD_GYEAR).ToString();
            table.Rows.Add(row4);
            table.AcceptChanges();
            double value4 = aggregator.GetRowValueAsNumber(table.Rows[4]);
            Assert.IsTrue(value4.Equals(double.NaN));

            DataRow row5 = table.NewRow();
            row5["?AGGVAR"] = "73523534763524347325732573573257673257382568732587638756328756387563875638756587537567356735";
            table.Rows.Add(row5);
            table.AcceptChanges();
            double value5 = aggregator.GetRowValueAsNumber(table.Rows[5]);
            Assert.IsTrue(value5.Equals(double.NaN));
        }

        [TestMethod]
        public void ShouldGetRowValueAsString()
        {
            RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
            DataTable table = new DataTable();
            DataColumn column = new DataColumn("?AGGVAR", typeof(string));
            table.Columns.Add(column);

            DataRow row0 = table.NewRow();
            row0["?AGGVAR"] = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
            table.Rows.Add(row0);
            table.AcceptChanges();
            string value0 = aggregator.GetRowValueAsString(table.Rows[0]);
            Assert.IsTrue(value0.Equals($"25^^{RDFVocabulary.XSD.FLOAT}"));

            DataRow row1 = table.NewRow();
            row1["?AGGVAR"] = DBNull.Value;
            table.Rows.Add(row1);
            table.AcceptChanges();
            string value1 = aggregator.GetRowValueAsString(table.Rows[1]);
            Assert.IsTrue(value1.Equals(string.Empty));

            DataRow row2 = table.NewRow();
            row2["?AGGVAR"] = null;
            table.Rows.Add(row2);
            table.AcceptChanges();
            string value2 = aggregator.GetRowValueAsString(table.Rows[2]);
            Assert.IsTrue(value2.Equals(string.Empty));

            DataRow row3 = table.NewRow();
            row3["?AGGVAR"] = new RDFResource("ex:res").ToString();
            table.Rows.Add(row3);
            table.AcceptChanges();
            string value3 = aggregator.GetRowValueAsString(table.Rows[3]);
            Assert.IsTrue(value3.Equals("ex:res"));

            DataRow row4 = table.NewRow();
            row4["?AGGVAR"] = new RDFPlainLiteral("hello", "en-US").ToString();
            table.Rows.Add(row4);
            table.AcceptChanges();
            string value4 = aggregator.GetRowValueAsString(table.Rows[4]);
            Assert.IsTrue(value4.Equals("hello@EN-US"));
        }

        [TestMethod]
        public void ShouldAddPartitionKeyToAggregatorContext()
        {
            RDFAggregatorContext aggCtx = new RDFAggregatorContext();
            aggCtx.AddPartitionKey<string>("testPKey", "value");

            Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionResult"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionResult"].Equals("value"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionCounter"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionCounter"].Equals(0d));
        }

        [TestMethod]
        public void ShouldGetPartitionKeyExecutionResultFromAggregatorContext()
        {
            RDFAggregatorContext aggCtx = new RDFAggregatorContext();
            string value = aggCtx.GetPartitionKeyExecutionResult<string>("testPKey", "value");

            Assert.IsTrue(value.Equals("value"));
            Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionResult"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionResult"].Equals(value));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionCounter"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionCounter"].Equals(0d));
        }

        [TestMethod]
        public void ShouldGetPartitionKeyExecutionCounterFromAggregatorContext()
        {
            RDFAggregatorContext aggCtx = new RDFAggregatorContext();
            aggCtx.AddPartitionKey<string>("testPKey", "value");
            double execCounter = aggCtx.GetPartitionKeyExecutionCounter("testPKey");

            Assert.IsTrue(execCounter.Equals(0d));
            Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionResult"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionResult"].Equals("value"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionCounter"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionCounter"].Equals(execCounter));
        }

        [TestMethod]
        public void ShouldUpdatePartitionKeyExecutionResultToAggregatorContext()
        {
            RDFAggregatorContext aggCtx = new RDFAggregatorContext();
            aggCtx.AddPartitionKey<string>("testPKey", "value");
            aggCtx.UpdatePartitionKeyExecutionResult<string>("testPKey", "value2");

            Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionResult"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionResult"].Equals("value2"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionCounter"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionCounter"].Equals(0d));
        }

        [TestMethod]
        public void ShouldUpdatePartitionKeyExecutionCounterToAggregatorContext()
        {
            RDFAggregatorContext aggCtx = new RDFAggregatorContext();
            aggCtx.AddPartitionKey<string>("testPKey", "value");
            aggCtx.UpdatePartitionKeyExecutionCounter("testPKey");

            Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionResult"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionResult"].Equals("value"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionCounter"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionCounter"].Equals(1d));
        }

        [TestMethod]
        public void ShouldCheckPartitionKeyRowValueCacheFromAggregatorContext()
        {
            RDFAggregatorContext aggCtx = new RDFAggregatorContext();
            aggCtx.AddPartitionKey<string>("testPKey", "value");

            Assert.IsFalse(aggCtx.CheckPartitionKeyRowValueCache<string>("testPKey", "value"));

            Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionResult"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionResult"].Equals("value"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionCounter"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionCounter"].Equals(0d));
        }

        [TestMethod]
        public void ShouldUpdatePartitionKeyRowValueCacheToAggregatorContext()
        {
            RDFAggregatorContext aggCtx = new RDFAggregatorContext();
            aggCtx.AddPartitionKey<string>("testPKey", "value");

            Assert.IsFalse(aggCtx.CheckPartitionKeyRowValueCache<string>("testPKey", "value"));
            aggCtx.UpdatePartitionKeyRowValueCache<string>("testPKey", "value");
            Assert.IsTrue(aggCtx.CheckPartitionKeyRowValueCache<string>("testPKey", "value"));

            Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionResult"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionResult"].Equals("value"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ContainsKey("ExecutionCounter"));
            Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"]["ExecutionCounter"].Equals(0d));
        }
        #endregion
    }
}