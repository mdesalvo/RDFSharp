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
        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(new RDFIsUriFilter(new RDFVariable("?VARU")), new RDFDatatypeFilter(new RDFVariable("?VARL"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.LeftFilter);
        Assert.IsNotNull(filter.RightFilter);
        Assert.IsTrue(filter.ToString().Equals($"FILTER ( ( ISURI(?VARU) ) && ( DATATYPE(?VARL) = <{RDFVocabulary.XSD.BOOLEAN}> ) )"));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals("FILTER ( ( ISURI(?VARU) ) && ( DATATYPE(?VARL) = xsd:boolean ) )"));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateNestedBooleanAndFilter()
    {
        RDFBooleanAndFilter filterA = new RDFBooleanAndFilter(new RDFIsUriFilter(new RDFVariable("?VARU")), new RDFDatatypeFilter(new RDFVariable("?VARL"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN));
        RDFBooleanAndFilter filterB = new RDFBooleanAndFilter(filterA, new RDFSameTermFilter(new RDFVariable("?VARL"), RDFVocabulary.RDF.ALT));

        Assert.IsNotNull(filterB);
        Assert.IsNotNull(filterB.LeftFilter);
        Assert.IsNotNull(filterB.RightFilter);
        Assert.IsTrue(filterB.ToString().Equals($"FILTER ( ( ( ISURI(?VARU) ) && ( DATATYPE(?VARL) = <{RDFVocabulary.XSD.BOOLEAN}> ) ) && ( SAMETERM(?VARL, <{RDFVocabulary.RDF.ALT}>) ) )"));
        Assert.IsTrue(filterB.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals($"FILTER ( ( ( ISURI(?VARU) ) && ( DATATYPE(?VARL) = xsd:boolean ) ) && ( SAMETERM(?VARL, <{RDFVocabulary.RDF.ALT}>) ) )"));
        Assert.IsTrue(filterB.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filterB.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBooleanAndFilterBecauseNullLeft()
        => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanAndFilter(null, new RDFIsUriFilter(new RDFVariable("?VAR"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBooleanAndFilterBecauseExistsLeft()
        => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanAndFilter(new RDFExistsFilter(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?OBJ"))), new RDFIsUriFilter(new RDFVariable("?VAR"))));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBooleanAndFilterBecauseNullRight()
        => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanAndFilter(new RDFIsUriFilter(new RDFVariable("?VAR")), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBooleanAndFilterBecauseExistsRight()
        => Assert.ThrowsException<RDFQueryException>(() => new RDFBooleanAndFilter(new RDFIsUriFilter(new RDFVariable("?VAR")), new RDFExistsFilter(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?OBJ")))));

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

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_FLOAT), new RDFLangMatchesFilter(new RDFVariable("?B"), "*"));
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

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_FLOAT), new RDFLangMatchesFilter(new RDFVariable("?B"), null));
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

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_FLOAT), new RDFLangMatchesFilter(new RDFVariable("?B"), "*"));
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

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN), new RDFLangMatchesFilter(new RDFVariable("?B"), "*"));
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

        RDFBooleanAndFilter filter = new RDFBooleanAndFilter(new RDFDatatypeFilter(new RDFVariable("?A"), RDFModelEnums.RDFDatatypes.XSD_FLOAT), new RDFLangMatchesFilter(new RDFVariable("?B"), null));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }
    #endregion
}