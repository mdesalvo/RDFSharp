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
public class RDFExpressionFilterTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateExpressionFilterWithBooleanExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFBooleanAndExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")),
                    new RDFConstantExpression(new RDFTypedLiteral("24.08", RDFModelEnums.RDFDatatypes.XSD_FLOAT))),
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFVariableExpression(new RDFVariable("?V1")),
                    new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (((?V1 + ?V2) = 24.08) && (?V1 = \"hello\"^^<http://www.w3.org/2001/XMLSchema#string>)) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (((?V1 + ?V2) = 24.08) && (?V1 = \"hello\"^^xsd:string)) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullBooleanExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFBooleanExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithBoundExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFBoundExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (BOUND(?V1)) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (BOUND(?V1)) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullBoundExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFBoundExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithComparisonExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo, new RDFVariableExpression(new RDFVariable("?V1")), new RDFVariable("?V2")));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (?V1 = ?V2) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (?V1 = ?V2) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullComparisonExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFComparisonExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithInExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFInExpression(new RDFVariableExpression(new RDFVariable("?V1")), [new RDFPlainLiteral("hello","en-US")]));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (?V1 IN (\"hello\"@EN-US)) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (?V1 IN (\"hello\"@EN-US)) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullInExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFInExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsBlankExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFIsBlankExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (ISBLANK(?V1)) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (ISBLANK(?V1)) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsBlankExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFIsBlankExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsLiteralExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFIsLiteralExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (ISLITERAL(?V1)) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (ISLITERAL(?V1)) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsLiteralExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFIsLiteralExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsNumericExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFIsNumericExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (ISNUMERIC(?V1)) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (ISNUMERIC(?V1)) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsNumericExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFIsNumericExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsUriExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFIsUriExpression(new RDFVariableExpression(new RDFVariable("?V1"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (ISURI(?V1)) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (ISURI(?V1)) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsUriExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFIsUriExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithLangMatchesExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFLangMatchesExpression(new RDFVariableExpression(new RDFVariable("?V1")), new RDFConstantExpression(new RDFPlainLiteral("en-US"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (LANGMATCHES(LANG(?V1),\"en-US\")) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (LANGMATCHES(LANG(?V1),\"en-US\")) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullIsLangMatchesExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFLangMatchesExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithRegexExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFRegexExpression(new RDFVariableExpression(new RDFVariable("?V1")), new Regex("^hello$")));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (REGEX(STR(?V1), \"^hello$\")) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (REGEX(STR(?V1), \"^hello$\")) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullRegexExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFRegexExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterWithSameTermExpression()
    {
        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFSameTermExpression(new RDFVariableExpression(new RDFVariable("?V1")), new RDFVariableExpression(new RDFVariable("?V2"))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Expression);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( (SAMETERM(?V1, ?V2)) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( (SAMETERM(?V1, ?V2)) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingExpressionFilterBecauseNullSameTermExpression()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFExpressionFilter(null as RDFSameTermExpression));

    [TestMethod]
    public void ShouldCreateExpressionFilterAndKeepRow()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFExpressionFilter filter = new RDFExpressionFilter(
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
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 8) || (STRSTARTS(?B, \"he\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterAndKeepRowBecauseNegation()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFExpressionFilter filter = new RDFExpressionFilter(
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
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 35) || (STRSTARTS(?B, \"pol\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterWithIsUriExpressionAndKeepRow()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFExpressionFilter filter = new RDFExpressionFilter(
            new RDFBooleanAndExpression(
                new RDFIsUriExpression(new RDFVariable("?A")),
                new RDFStrStartsExpression(
                    new RDFVariable("?B"),
                    new RDFConstantExpression(new RDFTypedLiteral("he", RDFModelEnums.RDFDatatypes.XSD_STRING)))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( ((ISURI(?A)) && (STRSTARTS(?B, \"he\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterAndNotKeepRow()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFExpressionFilter filter = new RDFExpressionFilter(
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
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 35) || (STRSTARTS(?B, \"pol\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
    }

    [TestMethod]
    public void ShouldCreateExpressionFilterAndNotKeepRowBecauseNegation()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFResource("http://example.org/").ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFExpressionFilter filter = new RDFExpressionFilter(
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
        Assert.IsTrue(string.Equals(filter.ToString(), "FILTER ( (((STRLEN(?A)) > 8) || (STRSTARTS(?B, \"he\"^^<http://www.w3.org/2001/XMLSchema#string>))) )"));
    }
    #endregion
}