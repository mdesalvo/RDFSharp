/*
   Copyright 2012-2026 Marco De Salvo

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific LangMatchesuage governing permissions and
   limitations under the License.
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFLangMatchesExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateLangMatchesExpressionWithExpressionExpression()
    {
        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFConstantExpression(new RDFPlainLiteral("en-US--ltr")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(LANGMATCHES(LANG((?V1 + ?V2)),\"en-US--ltr\"))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(LANGMATCHES(LANG((?V1 + ?V2)),\"en-US--ltr\"))", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionWithExpressionVariable()
    {
        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?L"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(LANGMATCHES(LANG((?V1 + ?V2)),?L))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(LANGMATCHES(LANG((?V1 + ?V2)),?L))", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionWithVariableExpression()
    {
        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(
            new RDFVariable("?L"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(LANGMATCHES(LANG(?L),(?V1 + ?V2)))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(LANGMATCHES(LANG(?L),(?V1 + ?V2)))", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionWithVariableVariable()
    {
        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(
            new RDFVariable("?L"),
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(LANGMATCHES(LANG(?L),?V1))", StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(LANGMATCHES(LANG(?L),?V1))", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLangMatchesExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLangMatchesExpression(null as RDFExpression, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLangMatchesExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLangMatchesExpression(null as RDFVariable, new RDFVariable("?V")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLangMatchesExpressionWithExpressionBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?V")), null as RDFExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingLangMatchesExpressionWithVariableBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFLangMatchesExpression(new RDFVariable("?V"), null as RDFVariable));

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultTrue()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("en-us")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionWithLeftExpressionAndCalculateResultTrue()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?B")), new RDFConstantExpression(new RDFPlainLiteral("en-us")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultTrueOnSuperLangTag()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US--rtl").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("EN")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultTrueOnSuperLangTagRegional()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US--rtl").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("EN-us")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionWithLeftExpressionAndCalculateResultTrueOnSuperLangTag()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US--rtl").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?B")), new RDFConstantExpression(new RDFPlainLiteral("EN")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultTrueOnRightVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("en-US").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariable("?A"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionWithLeftExpressionAndCalculateResultTrueOnRightVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("en-US").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?B")), new RDFVariable("?A"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalseOnSuperLangTag()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("EN-US")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("en-UK")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalseOnNullLeftColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Empty));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalseOnUnlanguagedLiteral()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("en-UK")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndCalculateResultFalseOnResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?A"), new RDFConstantExpression(new RDFPlainLiteral("EN-US")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndNotCalculateResultBecauseUnkownLeftColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?Q"), new RDFConstantExpression(new RDFPlainLiteral("en-us")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldCreateExactLangMatchesExpressionAndNotCalculateResultBecauseUnkownRightColumn()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("en-US").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariable("?Q"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldCreateStarLangMatchesExpressionAndCalculateResultTrue()
    {
       RDFTable table = new RDFTable();
       table.AddColumn("?A");
       table.AddColumn("?B");
       table.AddRow(new Dictionary<string, string>
       {
           { "?A", new RDFResource("http://example.org/").ToString() },
           { "?B", new RDFPlainLiteral("hello", "en-US--ltr").ToString() }
       });

       RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("*")));
       RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

       Assert.IsNotNull(result);
       Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateStarLangMatchesExpressionAndCalculateResultTrueOnRightVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("*").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US--ltr").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariable("?A"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateStarLangMatchesExpressionAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(new RDFPlainLiteral("*")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateNoLangMatchesExpressionAndCalculateResultTrue()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Empty));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateNoLangMatchesExpressionAndCalculateResultTrueOnRightVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", null! },
            { "?B", new RDFPlainLiteral("hello").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.True));
    }

    [TestMethod]
    public void ShouldCreateNoLangMatchesExpressionAndCalculateResultFalse()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Empty));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Equals(RDFTypedLiteral.False));
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionAndNotCalculateResultBecauseLeftVariableResolvingToResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", RDFPlainLiteral.Empty.ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?A"), new RDFVariable("?B"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void ShouldCreateLangMatchesExpressionAndNotCalculateResultBecauseRightVariableResolvingToResource()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello").ToString() }
        });

        RDFLangMatchesExpression expression = new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFVariable("?A"));
        RDFPatternMember result = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(result);
    }
    #endregion
}