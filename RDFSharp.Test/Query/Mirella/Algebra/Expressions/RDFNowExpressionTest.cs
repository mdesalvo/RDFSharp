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
public class RDFNowExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateNowExpression()
    {
        RDFNowExpression expression = new RDFNowExpression();

        Assert.IsNotNull(expression);
        Assert.IsNull(expression.LeftArgument);
        Assert.IsNull(expression.RightArgument);
        Assert.IsTrue(expression.ToString().Equals("(NOW())", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([]).Equals("(NOW())", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyExpressionAndCalculateResult()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFTypedLiteral("5.1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() },
            { "?B", new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INT).ToString() },
        });

        RDFNowExpression expression = new RDFNowExpression();
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult is RDFTypedLiteral tlitExprRes && tlitExprRes.HasDatetimeDatatype());

        System.Threading.Thread.Sleep(5);

        RDFPatternMember expressionResult2 = expression.ApplyExpression(table.Rows[0]);
        Assert.IsTrue(expressionResult2 is RDFTypedLiteral tlitExprRes2 && tlitExprRes2.HasDatetimeDatatype());
        Assert.IsFalse(expressionResult.Equals(expressionResult2));
    }
    #endregion
}