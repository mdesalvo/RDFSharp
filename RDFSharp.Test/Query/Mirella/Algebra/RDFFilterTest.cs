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
using System.Text.RegularExpressions;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFFilterTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateExpressionFilterWithBooleanExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFBooleanAndExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                    new RDFConstantExpression(new RDFTypedLiteral("24.08", RDFModelEnums.RDFDatatypes.XSD_FLOAT))),
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFVariableExpression(new RDFVariable("?V1")),
                    new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (((?V1 + ?V2) = 24.08) && (?V1 = \"hello\"^^<http://www.w3.org/2001/XMLSchema#string>)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (((?V1 + ?V2) = 24.08) && (?V1 = \"hello\"^^xsd:string)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullBooleanExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFBooleanExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithBoundExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFBoundExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (BOUND(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (BOUND(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullBoundExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFBoundExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithComparisonExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariableExpression(new RDFVariable("?V1")), new RDFVariable("?V2")));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (?V1 = ?V2) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (?V1 = ?V2) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullComparisonExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFComparisonExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithInExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFInExpression(new RDFVariableExpression(new RDFVariable("?V1")), [new RDFPlainLiteral("hello","en-US")]));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (?V1 IN (\"hello\"@EN-US)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (?V1 IN (\"hello\"@EN-US)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullInExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFInExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsBlankExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFIsBlankExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (ISBLANK(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (ISBLANK(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsBlankExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFIsBlankExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsLiteralExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFIsLiteralExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (ISLITERAL(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (ISLITERAL(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsLiteralExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFIsLiteralExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsNumericExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFIsNumericExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (ISNUMERIC(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (ISNUMERIC(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsNumericExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFIsNumericExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsUriExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFIsUriExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (ISURI(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (ISURI(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsUriExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFIsUriExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithLangMatchesExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?V1")), new RDFConstantExpression(new RDFPlainLiteral("en-US"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (LANGMATCHES(LANG(?V1),\"en-US\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (LANGMATCHES(LANG(?V1),\"en-US\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsLangMatchesExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFLangMatchesExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithRegexExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFRegexExpression(new RDFVariableExpression(new RDFVariable("?V1")), new Regex("^hello$")));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (REGEX(?V1, \"^hello$\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (REGEX(?V1, \"^hello$\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullRegexExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFRegexExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithSameTermExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFSameTermExpression(new RDFVariableExpression(new RDFVariable("?V1")), new RDFVariableExpression(new RDFVariable("?V2"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (SAMETERM(?V1, ?V2)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (SAMETERM(?V1, ?V2)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullSameTermExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFSameTermExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithContainsExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFContainsExpression(new RDFVariable("?V1"), new RDFConstantExpression(new RDFPlainLiteral("hello"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (CONTAINS(?V1, \"hello\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (CONTAINS(?V1, \"hello\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullContainsExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFContainsExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithStrStartsExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFStrStartsExpression(new RDFVariable("?V1"), new RDFConstantExpression(new RDFPlainLiteral("he"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (STRSTARTS(?V1, \"he\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (STRSTARTS(?V1, \"he\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullStrStartsExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFStrStartsExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithStrEndsExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFStrEndsExpression(new RDFVariable("?V1"), new RDFConstantExpression(new RDFPlainLiteral("lo"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (STRENDS(?V1, \"lo\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (STRENDS(?V1, \"lo\")) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullStrEndsExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFStrEndsExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithHasLangExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFHasLangExpression(new RDFVariable("?V1")));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (HASLANG(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (HASLANG(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullHasLangExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFHasLangExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithHasLangDirExpression()
    {
        RDFFilter filter = new RDFFilter(
            new RDFHasLangDirExpression(new RDFVariable("?V1")));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (HASLANGDIR(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (HASLANGDIR(?V1)) )", System.StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullHasLangDirExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFFilter(null as RDFHasLangDirExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterAndKeepRow()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFFilter filter = new RDFFilter(
            new RDFBooleanOrExpression(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                    new RDFStrLenExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(new RDFTypedLiteral("8", RDFModelEnums.RDFDatatypes.XSD_INT))),
                new RDFStrStartsExpression(
                    new RDFVariable("?B"),
                    new RDFConstantExpression(new RDFTypedLiteral("he", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 8) || (STRSTARTS(?B, \"he\"^^<http://www.w3.org/2001/XMLSchema#string>))) )", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterAndKeepRowBecauseNegation()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFFilter filter = new RDFFilter(
            new RDFBooleanOrExpression(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                    new RDFStrLenExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_INT))),
                new RDFStrStartsExpression(
                    new RDFVariable("?B"),
                    new RDFConstantExpression(new RDFTypedLiteral("pol", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsTrue(keepRow);
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 35) || (STRSTARTS(?B, \"pol\"^^<http://www.w3.org/2001/XMLSchema#string>))) )", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsUriExpressionAndKeepRow()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFFilter filter = new RDFFilter(
            new RDFBooleanAndExpression(
                new RDFIsUriExpression(new RDFVariable("?A")),
                new RDFStrStartsExpression(
                    new RDFVariable("?B"),
                    new RDFConstantExpression(new RDFTypedLiteral("he", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( ((ISURI(?A)) && (STRSTARTS(?B, \"he\"^^<http://www.w3.org/2001/XMLSchema#string>))) )", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterAndNotKeepRow()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFFilter filter = new RDFFilter(
            new RDFBooleanOrExpression(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                    new RDFStrLenExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(new RDFTypedLiteral("35", RDFModelEnums.RDFDatatypes.XSD_INT))),
                new RDFStrStartsExpression(
                    new RDFVariable("?B"),
                    new RDFConstantExpression(new RDFTypedLiteral("pol", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 35) || (STRSTARTS(?B, \"pol\"^^<http://www.w3.org/2001/XMLSchema#string>))) )", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterAndNotKeepRowBecauseNegation()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() }
        });

        RDFFilter filter = new RDFFilter(
            new RDFBooleanOrExpression(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.GreaterThan,
                    new RDFStrLenExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(new RDFTypedLiteral("8", RDFModelEnums.RDFDatatypes.XSD_INT))),
                new RDFStrStartsExpression(
                    new RDFVariable("?B"),
                    new RDFConstantExpression(new RDFTypedLiteral("he", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsFalse(keepRow);
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 8) || (STRSTARTS(?B, \"he\"^^<http://www.w3.org/2001/XMLSchema#string>))) )", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterFromExistsExpression()
    {
        RDFFilter filter = new RDFFilter(new RDFExistsExpression(
            new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDF.ALT))));

        Assert.IsNotNull(filter);
        Assert.IsInstanceOfType(filter.Expression, typeof(RDFExistsExpression));
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( EXISTS { ?S ?P <" + RDFVocabulary.RDF.ALT + "> . } )", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyExpressionFilterFromExistsExpression()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddRow(new Dictionary<string, string> { { "?A", new RDFResource("ex:org").ToString() } });

        //Disjoint EXISTS with at least one solution holds => filter keeps the row (and its negation drops it)
        RDFExistsExpression existsExpression = new RDFExistsExpression(
            new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O")))) { PatternResults = new RDFTable() };
        existsExpression.PatternResults.AddColumn("?Z");
        existsExpression.PatternResults.AddRow(new Dictionary<string, string> { { "?Z", new RDFResource("ex:thing").ToString() } });
        RDFFilter filter = new RDFFilter(existsExpression);

        Assert.IsTrue(filter.ApplyFilter(table.Rows[0], false));
        Assert.IsFalse(filter.ApplyFilter(table.Rows[0], true));
    }
    #endregion
}
