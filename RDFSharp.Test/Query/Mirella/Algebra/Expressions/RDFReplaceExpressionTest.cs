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
using System.Text.RegularExpressions;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFReplaceExpressionTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateRegexExpressionWithExpression()
    {
        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsNotNull(expression.RegEx);
        Assert.IsTrue(expression.ToString().Equals("(REPLACE(STR((?V1 + ?V2)), \"^hello$\", STR((?V1 + ?V2)), \"ismx\"))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REPLACE(STR((?V1 + ?V2)), \"^hello$\", STR((?V1 + ?V2)), \"ismx\"))"));
    }

    [TestMethod]
    public void ShouldCreateRegexExpressionWithExpressionAndNoFlags()
    {
        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new Regex("^hello$"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsNotNull(expression.RegEx);
        Assert.IsTrue(expression.ToString().Equals("(REPLACE(STR((?V1 + ?V2)), \"^hello$\", STR((?V1 + ?V2))))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REPLACE(STR((?V1 + ?V2)), \"^hello$\", STR((?V1 + ?V2))))"));
    }

    [TestMethod]
    public void ShouldCreateRegexExpressionWithVariable()
    {
        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V1"),
            new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsNotNull(expression.RegEx);
        Assert.IsTrue(expression.ToString().Equals("(REPLACE(STR((?V1 + ?V2)), \"^hello$\", STR(?V1), \"ismx\"))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REPLACE(STR((?V1 + ?V2)), \"^hello$\", STR(?V1), \"ismx\"))"));
    }

    [TestMethod]
    public void ShouldCreateRegexExpressionWithVariableAndNoFlags()
    {
        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new RDFVariable("?V1"),
            new Regex("^hello$"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsNotNull(expression.RegEx);
        Assert.IsTrue(expression.ToString().Equals("(REPLACE(STR((?V1 + ?V2)), \"^hello$\", STR(?V1)))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REPLACE(STR((?V1 + ?V2)), \"^hello$\", STR(?V1)))"));
    }

    [TestMethod]
    public void ShouldCreateRegexVariableWithExpression()
    {
        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?V1"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsNotNull(expression.RegEx);
        Assert.IsTrue(expression.ToString().Equals("(REPLACE(STR(?V1), \"^hello$\", STR((?V1 + ?V2)), \"ismx\"))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REPLACE(STR(?V1), \"^hello$\", STR((?V1 + ?V2)), \"ismx\"))"));
    }

    [TestMethod]
    public void ShouldCreateRegexVariableWithExpressionAndNoFlags()
    {
        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?V1"),
            new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
            new Regex("^hello$"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsNotNull(expression.RegEx);
        Assert.IsTrue(expression.ToString().Equals("(REPLACE(STR(?V1), \"^hello$\", STR((?V1 + ?V2))))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REPLACE(STR(?V1), \"^hello$\", STR((?V1 + ?V2))))"));
    }

    [TestMethod]
    public void ShouldCreateRegexVariableWithVariable()
    {
        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"),
            new Regex("^hello$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsNotNull(expression.RegEx);
        Assert.IsTrue(expression.ToString().Equals("(REPLACE(STR(?V1), \"^hello$\", STR(?V2), \"ismx\"))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REPLACE(STR(?V1), \"^hello$\", STR(?V2), \"ismx\"))"));
    }

    [TestMethod]
    public void ShouldCreateRegexVariableWithVariableAndNoFlags()
    {
        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?V1"),
            new RDFVariable("?V2"),
            new Regex("^hello$"));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.LeftArgument);
        Assert.IsNotNull(expression.RightArgument);
        Assert.IsNotNull(expression.RegEx);
        Assert.IsTrue(expression.ToString().Equals("(REPLACE(STR(?V1), \"^hello$\", STR(?V2)))"));
        Assert.IsTrue(expression.ToString([]).Equals("(REPLACE(STR(?V1), \"^hello$\", STR(?V2)))"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(null as RDFExpression, new RDFVariableExpression(new RDFVariable("?V1")), new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithExpressionBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(new RDFVariableExpression(new RDFVariable("?V1")), null as RDFExpression, new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithExpressionBecauseNullRegex()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(new RDFVariableExpression(new RDFVariable("?V1")), new RDFVariableExpression(new RDFVariable("?V2")), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(null as RDFExpression, new RDFVariable("?V1"), new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithVariableBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(new RDFVariableExpression(new RDFVariable("?V1")), null as RDFVariable, new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexExpressionWithVariableBecauseNullRegex()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(new RDFVariableExpression(new RDFVariable("?V1")), new RDFVariable("?V2"), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexVariableWithExpressionBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(null as RDFVariable, new RDFVariableExpression(new RDFVariable("?V1")), new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexVariableWithExpressionBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(new RDFVariable("?V1"), null as RDFExpression, new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexVariableWithExpressionBecauseNullRegex()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(new RDFVariable("?V1"), new RDFVariableExpression(new RDFVariable("?V2")), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexVariableWithVariableBecauseNullLeftArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(null as RDFVariable, new RDFVariable("?V1"), new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexVariableWithVariableBecauseNullRightArgument()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(new RDFVariable("?V1"), null as RDFVariable, new Regex("^hello$")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingRegexVariableWithVariableBecauseNullRegex()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFReplaceExpression(new RDFVariable("?V1"), new RDFVariable("?V2"), null));

    //EE

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNullLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = "hello";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNullRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = "hello";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?B")),
            new RDFVariableExpression(new RDFVariable("?A")),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnNullLeftRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = null!;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnPlainLiterals()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "abab";
        row["?B"] = "Z";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("aZb")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnStringTypedLiterals()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"abab^^{RDFVocabulary.XSD.STRING}";
        row["?B"] = $"Z^^{RDFVocabulary.XSD.STRING}";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("aZb")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithExpressionAndCalculateResultOnResources()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "http://example.org/test/ex1";
        row["?B"] = "http://example.com/";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("http://example.org/"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.com/test/ex1")));
    }

    //EV

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnNullLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = "hello";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnNullRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = "hello";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnNullLeftRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = null!;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnPlainLiterals()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "abab";
        row["?B"] = "Z";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("aZb")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnStringTypedLiterals()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"abab^^{RDFVocabulary.XSD.STRING}";
        row["?B"] = $"Z^^{RDFVocabulary.XSD.STRING}";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("aZb")));
    }

    [TestMethod]
    public void ShouldApplyExpressionWithVariableAndCalculateResultOnResources()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "http://example.org/test/ex1";
        row["?B"] = "http://example.com/";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariableExpression(new RDFVariable("?A")),
            new RDFVariable("?B"),
            new Regex("http://example.org/"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.com/test/ex1")));
    }

    //VE

    [TestMethod]
    public void ShouldApplyVariableWithExpressionAndCalculateResultOnNullLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = "hello";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyVariableWithExpressionAndCalculateResultOnNullRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = "hello";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyVariableWithExpressionAndCalculateResultOnNullLeftRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = null!;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyVariableWithExpressionAndCalculateResultOnPlainLiterals()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "abab";
        row["?B"] = "Z";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("aZb")));
    }

    [TestMethod]
    public void ShouldApplyVariableWithExpressionAndCalculateResultOnStringTypedLiterals()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"abab^^{RDFVocabulary.XSD.STRING}";
        row["?B"] = $"Z^^{RDFVocabulary.XSD.STRING}";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("aZb")));
    }

    [TestMethod]
    public void ShouldApplyVariableWithExpressionAndCalculateResultOnResources()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "http://example.org/test/ex1";
        row["?B"] = "http://example.com/";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariableExpression(new RDFVariable("?B")),
            new Regex("http://example.org/"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.com/test/ex1")));
    }

    //VV

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndCalculateResultOnNullLeft()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = "hello";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndCalculateResultOnNullRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = "hello";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndCalculateResultOnNullLeftRight()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = null!;
        row["?B"] = null!;
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            new Regex("^hello$"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(RDFPlainLiteral.Empty));
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndCalculateResultOnPlainLiterals()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "abab";
        row["?B"] = "Z";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("aZb")));
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndCalculateResultOnStringTypedLiterals()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"abab^^{RDFVocabulary.XSD.STRING}";
        row["?B"] = $"Z^^{RDFVocabulary.XSD.STRING}";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFPlainLiteral("aZb")));
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndCalculateResultOnResources()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "http://example.org/test/ex1";
        row["?B"] = "http://example.com/";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            new Regex("http://example.org/"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNotNull(expressionResult);
        Assert.IsTrue(expressionResult.Equals(new RDFResource("http://example.com/test/ex1")));
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndNotCalculateResultBecauseNotStringLeftTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"25^^{RDFVocabulary.XSD.INTEGER}";
        row["?B"] = $"Z^^{RDFVocabulary.XSD.STRING}";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?B"),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndNotCalculateResultBecauseNotStringRightTypedLiteral()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = $"25^^{RDFVocabulary.XSD.INTEGER}";
        row["?B"] = $"Z^^{RDFVocabulary.XSD.STRING}";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?B"),
            new RDFVariable("?A"),
            new Regex("ba", RegexOptions.IgnoreCase));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndNotCalculateResultBecauseUnknownLeftVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "http://example.org/test/ex1";
        row["?B"] = "http://example.com/";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?Q"),
            new RDFVariable("?B"),
            new Regex("http://example.org/"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }

    [TestMethod]
    public void ShouldApplyVariableWithVariableAndNotCalculateResultBecauseUnknownRightVariable()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = "http://example.org/test/ex1";
        row["?B"] = "http://example.com/";
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFReplaceExpression expression = new RDFReplaceExpression(
            new RDFVariable("?A"),
            new RDFVariable("?Q"),
            new Regex("http://example.org/"));
        RDFPatternMember expressionResult = expression.ApplyExpression(table.Rows[0]);

        Assert.IsNull(expressionResult);
    }
    #endregion
}