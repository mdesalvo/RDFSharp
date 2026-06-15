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
public class RDFIriExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateIriExpressionWithExpression()
    {
        RDFIriExpression expression = new RDFIriExpression(
            new RDFVariableExpression(new RDFVariable("?V1")));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(IRI(?V1))", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(IRI(?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateIriExpressionWithVariable()
    {
        RDFIriExpression expression = new RDFIriExpression(
            new RDFVariable("?V1"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(IRI(?V1))", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingIriExpressionBecauseNullLeftArgumentExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFIriExpression(null as RDFExpression));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingIriExpressionBecauseNullLeftArgumentVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFIriExpression(null as RDFVariable));

    [TestMethod]
    public void ShouldApplyExpressionAndReturnResourceUnchanged()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/subject").ToString() }
        });

        RDFIriExpression expression = new RDFIriExpression(new RDFVariable("?A"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/subject")));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndBuildResourceFromAbsoluteIriString()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("http://example.org/built").ToString() }
        });

        RDFIriExpression expression = new RDFIriExpression(new RDFVariableExpression(new RDFVariable("?A")));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult is RDFResource);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.org/built")));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateNullResultOnRelativeIriString()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("relative/path").ToString() }
        });

        //No query BASE exists in the model, so a relative IRI cannot be resolved => unbound
        RDFIriExpression expression = new RDFIriExpression(new RDFVariable("?A"));
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
            { "?A", new RDFResource("http://example.org/subject").ToString() }
        });

        RDFIriExpression expression = new RDFIriExpression(new RDFVariable("?NOTBOUND"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}
