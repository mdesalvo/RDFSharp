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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFAggregatorTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateAggregator()
    {
        RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.Metadata.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.Metadata.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsFalse(aggregator.Metadata.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals(string.Empty, StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAggregatorBecauseNullAggregatorVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFAggregator(null, new RDFVariable("?PROJVAR")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingAggregatorBecauseNullPartitionVariable()
        =>  Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFAggregator(new RDFVariable("?AGGVAR"), null));

    [TestMethod]
    public void ShouldSetDistinct()
    {
        RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
        aggregator.Distinct();

        Assert.IsNotNull(aggregator);
        Assert.IsTrue(aggregator.Metadata.AggregatorVariable.Equals(new RDFVariable("?AGGVAR")));
        Assert.IsTrue(aggregator.Metadata.ProjectionVariable.Equals(new RDFVariable("?PROJVAR")));
        Assert.IsTrue(aggregator.Metadata.IsDistinct);
        Assert.IsTrue(aggregator.ToString().Equals(string.Empty, StringComparison.Ordinal));
        Assert.IsNotNull(aggregator.AggregatorContext);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionCache);
        Assert.IsNotNull(aggregator.AggregatorContext.ExecutionRegistry);
    }

    [TestMethod]
    public void ShouldSetHiddenFlag()
    {
        RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
        Assert.IsFalse(aggregator.Metadata.IsHidden);

        aggregator.Metadata.IsHidden = true;
        Assert.IsTrue(aggregator.Metadata.IsHidden);
    }

    [TestMethod]
    public void ShouldGetAggregateCallStringForPlainAggregate()
    {
        //'(COUNT(?E) AS ?CNT)' → 'COUNT(?E)' (strip the leading '(' and the ' AS ?proj)' suffix)
        RDFCountAggregator aggregator = new RDFCountAggregator(new RDFVariable("?E"), new RDFVariable("?CNT"));
        Assert.AreEqual("COUNT(?E)", aggregator.GetAggregateCallString());
    }

    [TestMethod]
    public void ShouldGetAggregateCallStringForDistinctAggregate()
    {
        RDFCountAggregator aggregator = new RDFCountAggregator(new RDFVariable("?E"), new RDFVariable("?CNT"));
        aggregator.Distinct();
        Assert.AreEqual("COUNT(DISTINCT ?E)", aggregator.GetAggregateCallString());
    }

    [TestMethod]
    public void ShouldGetAggregateCallStringForCountAll()
    {
        //COUNT(*) has no aggregated variable but still re-prints faithfully
        RDFCountAggregator aggregator = new RDFCountAggregator(new RDFVariable("?CNT"));
        Assert.AreEqual("COUNT(*)", aggregator.GetAggregateCallString());
    }

    [TestMethod]
    public void ShouldGetRowValueAsNumber()
    {
        RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
        RDFTable table = new RDFTable();
        table.AddColumn("?AGGVAR");

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() }
        });
        double value0 = aggregator.GetRowValueAsNumber(table.Rows[0]);
        Assert.IsTrue(value0.Equals(25.0d));

        table.AddRow([null]);
        double value1 = aggregator.GetRowValueAsNumber(table.Rows[1]);
        Assert.IsTrue(value1.Equals(double.NaN));

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", null }
        });
        double value2 = aggregator.GetRowValueAsNumber(table.Rows[2]);
        Assert.IsTrue(value2.Equals(double.NaN));

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", new RDFResource("ex:res").ToString() }
        });
        double value3 = aggregator.GetRowValueAsNumber(table.Rows[3]);
        Assert.IsTrue(value3.Equals(double.NaN));

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", new RDFTypedLiteral("2012", RDFModelEnums.RDFDatatypes.XSD_GYEAR).ToString() }
        });
        double value4 = aggregator.GetRowValueAsNumber(table.Rows[4]);
        Assert.IsTrue(value4.Equals(double.NaN));

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", "73523534763524347325732573573257673257382568732587638756328756387563875638756587537567356735" }
        });
        double value5 = aggregator.GetRowValueAsNumber(table.Rows[5]);
        Assert.IsTrue(value5.Equals(double.NaN));
    }

    [TestMethod]
    public void ShouldGetRowValueAsString()
    {
        RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
        RDFTable table = new RDFTable();
        table.AddColumn("?AGGVAR");

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() }
        });
        string value0 = aggregator.GetRowValueAsString(table.Rows[0]);
        Assert.IsTrue(value0.Equals($"25^^{RDFVocabulary.XSD.FLOAT}", StringComparison.Ordinal));

        table.AddRow([null]);
        string value1 = aggregator.GetRowValueAsString(table.Rows[1]);
        Assert.IsTrue(value1.Equals(string.Empty, StringComparison.Ordinal));

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", null }
        });
        string value2 = aggregator.GetRowValueAsString(table.Rows[2]);
        Assert.IsTrue(value2.Equals(string.Empty, StringComparison.Ordinal));

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", new RDFResource("ex:res").ToString() }
        });
        string value3 = aggregator.GetRowValueAsString(table.Rows[3]);
        Assert.IsTrue(value3.Equals("ex:res", StringComparison.Ordinal));

        table.AddRow(new Dictionary<string, string>
        {
            { "?AGGVAR", new RDFPlainLiteral("hello", "en-US").ToString() }
        });
        string value4 = aggregator.GetRowValueAsString(table.Rows[4]);
        Assert.IsTrue(value4.Equals("hello@EN-US", StringComparison.Ordinal));
    }

    //Virtuals (for test completeness, since they're just no-op at this level)

    [TestMethod]
    public void ShouldExecutePartition()
    {
        RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
        aggregator.ExecutePartition(null, default(RDFTableRow)); //Just no-op
    }

    [TestMethod]
    public void ShouldExecuteProjection()
    {
        RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
        RDFTable projectionTable = aggregator.ExecuteProjectionTable(null); //Just no-op

        Assert.IsNull(projectionTable);
    }

    [TestMethod]
    public void ShouldUpdateProjectionTable()
    {
        RDFAggregator aggregator = new RDFAggregator(new RDFVariable("?AGGVAR"), new RDFVariable("?PROJVAR"));
        aggregator.UpdateProjectionTable(null, null); //Just no-op
    }

    //AggregatorContext

    [TestMethod]
    public void ShouldAddPartitionKeyToAggregatorContext()
    {
        RDFAggregatorContext aggCtx = new RDFAggregatorContext();
        aggCtx.AddPartitionKey("testPKey", "value");

        Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionResult.Equals("value"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionCounter.Equals(0d));
    }

    [TestMethod]
    public void ShouldGetPartitionKeyExecutionResultFromAggregatorContext()
    {
        RDFAggregatorContext aggCtx = new RDFAggregatorContext();
        string value = aggCtx.GetPartitionKeyExecutionResult("testPKey", "value");

        Assert.IsTrue(value.Equals("value", StringComparison.Ordinal));
        Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionResult.Equals(value));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionCounter.Equals(0d));
    }

    [TestMethod]
    public void ShouldGetPartitionKeyExecutionCounterFromAggregatorContext()
    {
        RDFAggregatorContext aggCtx = new RDFAggregatorContext();
        aggCtx.AddPartitionKey("testPKey", "value");
        double execCounter = aggCtx.GetPartitionKeyExecutionCounter("testPKey");

        Assert.IsTrue(execCounter.Equals(0d));
        Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionResult.Equals("value"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionCounter.Equals(execCounter));
    }

    [TestMethod]
    public void ShouldUpdatePartitionKeyExecutionResultToAggregatorContext()
    {
        RDFAggregatorContext aggCtx = new RDFAggregatorContext();
        aggCtx.AddPartitionKey("testPKey", "value");
        aggCtx.UpdatePartitionKeyExecutionResult("testPKey", "value2");

        Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionResult.Equals("value2"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionCounter.Equals(0d));
    }

    [TestMethod]
    public void ShouldUpdatePartitionKeyExecutionCounterToAggregatorContext()
    {
        RDFAggregatorContext aggCtx = new RDFAggregatorContext();
        aggCtx.AddPartitionKey("testPKey", "value");
        aggCtx.UpdatePartitionKeyExecutionCounter("testPKey");

        Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionResult.Equals("value"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionCounter.Equals(1d));
    }

    [TestMethod]
    public void ShouldCheckPartitionKeyRowValueCacheFromAggregatorContext()
    {
        RDFAggregatorContext aggCtx = new RDFAggregatorContext();
        aggCtx.AddPartitionKey("testPKey", "value");

        Assert.IsFalse(aggCtx.CheckPartitionKeyRowValueCache("testPKey", "value"));

        Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionResult.Equals("value"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionCounter.Equals(0d));
    }

    [TestMethod]
    public void ShouldUpdatePartitionKeyRowValueCacheToAggregatorContext()
    {
        RDFAggregatorContext aggCtx = new RDFAggregatorContext();
        aggCtx.AddPartitionKey("testPKey", "value");

        Assert.IsFalse(aggCtx.CheckPartitionKeyRowValueCache("testPKey", "value"));
        aggCtx.UpdatePartitionKeyRowValueCache("testPKey", "value");
        Assert.IsTrue(aggCtx.CheckPartitionKeyRowValueCache("testPKey", "value"));

        Assert.IsTrue(aggCtx.ExecutionRegistry.ContainsKey("testPKey"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionResult.Equals("value"));
        Assert.IsTrue(aggCtx.ExecutionRegistry["testPKey"].ExecutionCounter.Equals(0d));
    }
    #endregion
}
