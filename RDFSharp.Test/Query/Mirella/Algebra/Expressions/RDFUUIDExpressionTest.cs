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
public class RDFUUIDExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateUUIDExpression()
    {
        RDFUUIDExpression expression = new RDFUUIDExpression();

        Assert.IsNotNull(expression);
        Assert.IsNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(UUID())", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(UUID())", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() }
        });

        RDFUUIDExpression expression = new RDFUUIDExpression();

        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);
        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult is RDFResource res1 && res1.ToString().StartsWith("urn:uuid:"));

        RDFPatternMember expressionResult2 = expression.ApplyExpression(table.Rows[0]);
        Assert.IsNotNull(expressionResult2);
        Assert.IsTrue(expressionResult2 is RDFResource res2 && res2.ToString().StartsWith("urn:uuid:"));
        Assert.IsFalse(expressionResult.Equals(expressionResult2));
    }
    #endregion
}