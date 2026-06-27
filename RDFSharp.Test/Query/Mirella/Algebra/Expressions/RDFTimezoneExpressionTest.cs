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
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFTimezoneExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateTimezoneExpressionWithExpression()
    {
        RDFTimezoneExpression expression = new RDFTimezoneExpression(
            new RDFVariableExpression(new RDFVariable("?V1")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(TIMEZONE(?V1))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(TIMEZONE(?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateTimezoneExpressionWithVariable()
    {
        RDFTimezoneExpression expression = new RDFTimezoneExpression(
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(TIMEZONE(?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingTimezoneExpressionBecauseNullLeftArgumentExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFTimezoneExpression(null as RDFExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingTimezoneExpressionBecauseNullLeftArgumentVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFTimezoneExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateZeroDayTimeDurationOnDateTime()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("2020-01-01T12:30:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME).ToString() }
        });

        RDFTimezoneExpression expression = new RDFTimezoneExpression(new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        //RDFSharp stores temporal literals in UTC, so the timezone is the zero day-time duration
        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFTypedLiteral("PT0S", RDFModelEnums.RDFDatatypes.XSD_DAYTIMEDURATION)));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateNullResultOnNonTemporalArgument()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello").ToString() }
        });

        RDFTimezoneExpression expression = new RDFTimezoneExpression(new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateNullResultOnUnboundVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("2020-01-01T12:30:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME).ToString() }
        });

        RDFTimezoneExpression expression = new RDFTimezoneExpression(new RDFVariable("?NOTBOUND"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}
