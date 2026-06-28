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
public class RDFExistsExpressionTest
{
    #region Helpers
    //A simple pattern group body reused across the correlation tests (the pattern itself is irrelevant to the
    //correlation, which works purely on the columns produced by the group graph pattern evaluation)
    private static RDFPatternGroup SamplePatternGroup()
        => new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O")));

    private static RDFTable RowTable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddColumn("?C");
        return table;
    }

    //True when ApplyExpression returned the boolean typed literal 'true'
    private static bool ExistsHolds(RDFExistsExpression expression, RDFTableRow row)
        => expression.ApplyExpression(row)?.Equals(RDFTypedLiteral.True) ?? false;
    #endregion

    #region Tests (creation)
    [TestMethod]
    public void ShouldCreateExistsExpressionFromPatternGroup()
    {
        RDFExistsExpression expression = new RDFExistsExpression(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDF.ALT)));

        Assert.IsNotNull(expression);
        Assert.IsNotNull(expression.GroupGraphPattern);
        Assert.IsInstanceOfType(expression.GroupGraphPattern, typeof(RDFPatternGroup));
        Assert.IsTrue(expression.ToString().Equals("EXISTS { ?S ?P <" + RDFVocabulary.RDF.ALT + "> . }", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("EXISTS { ?S ?P rdf:Alt . }", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateExistsExpressionFromSubSelect()
    {
        RDFSelectQuery subSelect = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"))));
        RDFExistsExpression expression = new RDFExistsExpression(subSelect);

        Assert.IsNotNull(expression);
        Assert.IsInstanceOfType(expression.GroupGraphPattern, typeof(RDFSelectQuery));
        Assert.IsTrue(((RDFSelectQuery)expression.GroupGraphPattern).IsSubQuery);
        Assert.IsTrue(expression.ToString().StartsWith("EXISTS {", System.StringComparison.Ordinal));
        Assert.IsTrue(expression.ToString().Contains("SELECT"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExistsExpressionBecauseNullPatternGroup()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExistsExpression((RDFPatternGroup)null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExistsExpressionBecauseNullSubSelect()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExistsExpression((RDFSelectQuery)null));
    #endregion

    #region Tests (correlation)
    [TestMethod]
    public void ShouldReturnFalseWhenPatternResultsIsNull()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string> { { "?A", new RDFResource("ex:org").ToString() }, { "?B", null }, { "?C", null } });

        //PatternResults left null (never pre-evaluated) => EXISTS is false
        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup());

        Assert.IsFalse(ExistsHolds(expression, table.Rows[0]));
    }

    [TestMethod]
    public void ShouldReturnTrueDisjointCaseWithNonEmptyResults()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        //No column shared with the row, but the group graph pattern produced at least one solution => true
        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup()) { PatternResults = new RDFTable() };
        expression.PatternResults.AddColumn("?Z");
        expression.PatternResults.AddRow(new Dictionary<string, string> { { "?Z", new RDFResource("ex:thing").ToString() } });

        Assert.IsTrue(ExistsHolds(expression, table.Rows[0]));
    }

    [TestMethod]
    public void ShouldReturnFalseDisjointCaseWithEmptyResults()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        //No column shared with the row AND the group graph pattern produced no solution => false
        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup()) { PatternResults = new RDFTable() };
        expression.PatternResults.AddColumn("?Z");

        Assert.IsFalse(ExistsHolds(expression, table.Rows[0]));
    }

    [TestMethod]
    public void ShouldReturnTrueMatchingSingleColumn()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup()) { PatternResults = new RDFTable() };
        expression.PatternResults.AddColumn("?A");
        expression.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() }
        });

        Assert.IsTrue(ExistsHolds(expression, table.Rows[0]));
    }

    [TestMethod]
    public void ShouldReturnTrueMatchingTwoColumns()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup()) { PatternResults = new RDFTable() };
        expression.PatternResults.AddColumn("?A");
        expression.PatternResults.AddColumn("?B");
        expression.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        Assert.IsTrue(ExistsHolds(expression, table.Rows[0]));
    }

    [TestMethod]
    public void ShouldReturnTrueBecauseUnboundSharedCellIsWildcard()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", null },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup()) { PatternResults = new RDFTable() };
        expression.PatternResults.AddColumn("?A");
        expression.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString() }
        });

        Assert.IsTrue(ExistsHolds(expression, table.Rows[0]));
    }

    [TestMethod]
    public void ShouldReturnFalseBecauseUnmatchingColumn()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup()) { PatternResults = new RDFTable() };
        expression.PatternResults.AddColumn("?A");
        expression.PatternResults.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFPlainLiteral("hello").ToString() }
        });

        Assert.IsFalse(ExistsHolds(expression, table.Rows[0]));
    }

    [TestMethod]
    public void ShouldReturnFalseBecauseTwoColumnsNeverMatchTogether()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:v1").ToString() },
            { "?B", new RDFResource("ex:v2").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        //Each value appears alone in a result row, but no single result row carries BOTH => intersection empties => false
        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup()) { PatternResults = new RDFTable() };
        expression.PatternResults.AddColumn("?A");
        expression.PatternResults.AddColumn("?B");
        expression.PatternResults.AddRow(new Dictionary<string, string> { { "?A", new RDFResource("ex:v1").ToString() }, { "?B", new RDFResource("ex:other").ToString() } });
        expression.PatternResults.AddRow(new Dictionary<string, string> { { "?A", new RDFResource("ex:other").ToString() }, { "?B", new RDFResource("ex:v2").ToString() } });

        Assert.IsFalse(ExistsHolds(expression, table.Rows[0]));
    }

    [TestMethod]
    public void ShouldReturnFalseBecauseEmptyResponseTable()
    {
        RDFTable table = RowTable();
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("ex:org").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
            { "?C", new RDFResource("ex:org").ToString() }
        });

        RDFExistsExpression expression = new RDFExistsExpression(SamplePatternGroup()) { PatternResults = new RDFTable() };
        expression.PatternResults.AddColumn("?A");

        Assert.IsFalse(ExistsHolds(expression, table.Rows[0]));
    }
    #endregion

    #region Tests (FindNestedExistsExpressions)
    [TestMethod]
    public void ShouldFindStandaloneExistsExpression()
    {
        RDFExistsExpression existsExpression = new RDFExistsExpression(SamplePatternGroup());

        List<RDFExistsExpression> found = RDFExistsExpression.FindNestedExistsExpressions(existsExpression);

        Assert.AreEqual(1, found.Count);
        Assert.AreSame(existsExpression, found[0]);
    }

    [TestMethod]
    public void ShouldFindNoExistsExpressionInPlainTree()
    {
        RDFExpression tree = new RDFBooleanAndExpression(
            new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFVariableExpression(new RDFVariable("?x")), new RDFConstantExpression(new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER))),
            new RDFBoundExpression(new RDFVariable("?y")));

        List<RDFExistsExpression> found = RDFExistsExpression.FindNestedExistsExpressions(tree);

        Assert.AreEqual(0, found.Count);
    }

    [TestMethod]
    public void ShouldFindNothingInNullTree()
        => Assert.AreEqual(0, RDFExistsExpression.FindNestedExistsExpressions(null).Count);

    [TestMethod]
    public void ShouldFindExistsNestedInBooleanAnd()
    {
        RDFExistsExpression existsExpression = new RDFExistsExpression(SamplePatternGroup());
        RDFExpression tree = new RDFBooleanAndExpression(existsExpression, new RDFBoundExpression(new RDFVariable("?y")));

        List<RDFExistsExpression> found = RDFExistsExpression.FindNestedExistsExpressions(tree);

        Assert.AreEqual(1, found.Count);
        Assert.AreSame(existsExpression, found[0]);
    }

    [TestMethod]
    public void ShouldFindExistsNestedInBooleanOrAndNot()
    {
        RDFExistsExpression leftExists = new RDFExistsExpression(SamplePatternGroup());
        RDFExistsExpression rightExists = new RDFExistsExpression(SamplePatternGroup());
        RDFExpression tree = new RDFBooleanOrExpression(leftExists, new RDFNotExpression(rightExists));

        List<RDFExistsExpression> found = RDFExistsExpression.FindNestedExistsExpressions(tree);

        Assert.AreEqual(2, found.Count);
        CollectionAssert.Contains(found, leftExists);
        CollectionAssert.Contains(found, rightExists);
    }

    [TestMethod]
    public void ShouldFindExistsNestedInIfCondition()
    {
        RDFExistsExpression existsExpression = new RDFExistsExpression(SamplePatternGroup());
        RDFExpression tree = new RDFIfExpression(existsExpression,
            new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFConstantExpression(new RDFTypedLiteral("0", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));

        List<RDFExistsExpression> found = RDFExistsExpression.FindNestedExistsExpressions(tree);

        Assert.AreEqual(1, found.Count);
        Assert.AreSame(existsExpression, found[0]);
    }

    [TestMethod]
    public void ShouldFindExistsNestedInInTerms()
    {
        RDFExistsExpression existsExpression = new RDFExistsExpression(SamplePatternGroup());
        RDFExpression tree = new RDFInExpression(new RDFVariableExpression(new RDFVariable("?x")),
            new List<RDFExpression> { existsExpression });

        List<RDFExistsExpression> found = RDFExistsExpression.FindNestedExistsExpressions(tree);

        Assert.AreEqual(1, found.Count);
        Assert.AreSame(existsExpression, found[0]);
    }

    [TestMethod]
    public void ShouldFindExistsNestedDeeplyAndPreserveCount()
    {
        RDFExistsExpression deepExists = new RDFExistsExpression(SamplePatternGroup());
        RDFExpression tree = new RDFBooleanAndExpression(
            new RDFBooleanOrExpression(new RDFBoundExpression(new RDFVariable("?a")), new RDFNotExpression(deepExists)),
            new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFVariableExpression(new RDFVariable("?x")), new RDFConstantExpression(new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));

        List<RDFExistsExpression> found = RDFExistsExpression.FindNestedExistsExpressions(tree);

        Assert.AreEqual(1, found.Count);
        Assert.AreSame(deepExists, found[0]);
    }
    #endregion
}
