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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFBooleanAndFilterTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateBooleanAndFilter()
    {
        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(
            new RDFExpressionFilter(
                new RDFIsUriExpression(new RDFVariable("?VARU"))),
            new RDFExpressionFilter(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFDatatypeExpression(new RDFVariable("?VARL")),
                    new RDFConstantExpression(RDFVocabulary.XSD.BOOLEAN))));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.LeftFilter);
        Assert.IsNotNull(filter.RightFilter);
        Assert.IsTrue(filter.ToString().Equals("FILTER ( ( (ISURI(?VARU)) ) && ( ((DATATYPE(?VARL)) = <http://www.w3.org/2001/XMLSchema#boolean>) ) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( ( (ISURI(?VARU)) ) && ( ((DATATYPE(?VARL)) = xsd:boolean) ) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateNestedBooleanAndFilter()
    {
        RDFBooleanAndFilter filterA = new RDFBooleanAndFilter(
            new RDFExpressionFilter(
                new RDFIsUriExpression(new RDFVariable("?VARU"))),
            new RDFExpressionFilter(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFDatatypeExpression(new RDFVariable("?VARL")),
                    new RDFConstantExpression(RDFVocabulary.XSD.BOOLEAN))));
        RDFBooleanAndFilter filterB = new RDFBooleanAndFilter(
            filterA,
            new RDFExpressionFilter(new RDFSameTermExpression(new RDFVariable("?VARL"), new RDFConstantExpression(RDFVocabulary.RDF.ALT))));

        Assert.IsNotNull(filterB);
        Assert.IsNotNull(filterB.LeftFilter);
        Assert.IsNotNull(filterB.RightFilter);
        Assert.IsTrue(filterB.ToString().Equals("FILTER ( ( ( (ISURI(?VARU)) ) && ( ((DATATYPE(?VARL)) = <http://www.w3.org/2001/XMLSchema#boolean>) ) ) && ( (SAMETERM(?VARL, <http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt>)) ) )"));
        Assert.IsTrue(filterB.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( ( ( (ISURI(?VARU)) ) && ( ((DATATYPE(?VARL)) = xsd:boolean) ) ) && ( (SAMETERM(?VARL, <http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt>)) ) )"));
        Assert.IsTrue(filterB.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filterB.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBooleanAndFilterBecauseNullLeft()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFBooleanAndFilter(null, new RDFExpressionFilter(new RDFIsUriExpression(new RDFVariable("?VAR")))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBooleanAndFilterBecauseExistsLeft()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFBooleanAndFilter(new RDFExistsFilter(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?OBJ"))), new RDFExpressionFilter(new RDFIsUriExpression(new RDFVariable("?VAR")))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBooleanAndFilterBecauseNullRight()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFBooleanAndFilter(new RDFExpressionFilter(new RDFIsUriExpression(new RDFVariable("?VAR"))), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBooleanAndFilterBecauseExistsRight()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFBooleanAndFilter(new RDFExpressionFilter(new RDFIsUriExpression(new RDFVariable("?VAR"))), new RDFExistsFilter(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?OBJ")))));

    [TestMethod]
    public void ShouldCreateBooleanAndFilterAndKeepRow()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(
            new RDFExpressionFilter(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFDatatypeExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(RDFVocabulary.XSD.FLOAT))),
            new RDFExpressionFilter(
                new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Star))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateBooleanAndFilterAndKeepRowBecauseNegation()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(
            new RDFExpressionFilter(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFDatatypeExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(RDFVocabulary.XSD.FLOAT))),
            new RDFExpressionFilter(new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Empty))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateBooleanAndFilterAndNotKeepRowBecauseNegation()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(
            new RDFExpressionFilter(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFDatatypeExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(RDFVocabulary.XSD.FLOAT))),
            new RDFExpressionFilter(new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Star))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateBooleanAndFilterAndNotKeepRowBecauseLeftFailure()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(
            new RDFExpressionFilter(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFDatatypeExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(RDFVocabulary.XSD.BOOLEAN))),
            new RDFExpressionFilter(new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Star))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateBooleanAndFilterAndNotKeepRowBecauseRightFailure()
    {
        DataTable table = new DataTable();
        table.Columns.Add("?A", typeof(string));
        table.Columns.Add("?B", typeof(string));
        DataRow row = table.NewRow();
        row["?A"] = new RDFTypedLiteral("27.7", RDFModelEnums.RDFDatatypes.XSD_FLOAT).ToString();
        row["?B"] = new RDFPlainLiteral("hello", "en-US").ToString();
        table.Rows.Add(row);
        table.AcceptChanges();

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(
            new RDFExpressionFilter(
                new RDFComparisonExpression(
                    RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFDatatypeExpression(new RDFVariable("?A")),
                    new RDFConstantExpression(RDFVocabulary.XSD.FLOAT))),
            new RDFExpressionFilter(new RDFLangMatchesExpression(new RDFVariable("?B"), new RDFConstantExpression(RDFPlainLiteral.Empty))));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }
    #endregion
}