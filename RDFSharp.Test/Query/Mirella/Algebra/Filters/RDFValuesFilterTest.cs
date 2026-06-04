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
using System;
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFValuesFilterTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateValuesFilterWithSingleBinding()
    {
        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [RDFVocabulary.RDF.ALT]));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Values);
        Assert.HasCount(1, filter.Values.Bindings);
        Assert.IsNotNull(filter.ValuesTable);
        Assert.IsTrue(filter.ToString().Equals("VALUES ?A { <" + RDFVocabulary.RDF.ALT + "> }", StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("VALUES ?A { rdf:Alt }", StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateValuesFilterWithUndefSingleBinding()
    {
        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [null]));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Values);
        Assert.HasCount(1, filter.Values.Bindings);
        Assert.IsNotNull(filter.ValuesTable);
        Assert.IsTrue(filter.ToString().Equals("VALUES ?A { UNDEF }", StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("VALUES ?A { UNDEF }", StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithSingleBinding()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithUndefSingleBinding()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [null]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithSingleBindingAndUnknownVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?Q"), [new RDFResource("http://example.org/")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithSingleBindingBecauseNegation()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/test/")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndNotKeepRowWithSingleBinding()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/test/")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndNotKeepRowWithSingleBindingBecauseNullValue()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", null },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/test/")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndNotKeepRowWithSingleBindingBecauseNegation()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterWithMultipleBindings()
    {
        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [RDFVocabulary.RDF.ALT])
                .AddColumn(new RDFVariable("?B"), [RDFVocabulary.RDF.BAG]));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Values);
        Assert.HasCount(2, filter.Values.Bindings);
        Assert.IsNotNull(filter.ValuesTable);
        Assert.IsTrue(filter.ToString().Equals("VALUES (?A ?B) {" + Environment.NewLine + "      ( <" + RDFVocabulary.RDF.ALT + "> <" + RDFVocabulary.RDF.BAG + "> )" + Environment.NewLine + "    }", StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("VALUES (?A ?B) {" + Environment.NewLine + "      ( rdf:Alt rdf:Bag )" + Environment.NewLine + "    }", StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateValuesFilterWithUndefMultipleBindings()
    {
        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [null])
                .AddColumn(new RDFVariable("?B"), [RDFVocabulary.RDF.BAG]));

        Assert.IsNotNull(filter);
        Assert.IsNotNull(filter.Values);
        Assert.HasCount(2, filter.Values.Bindings);
        Assert.IsNotNull(filter.ValuesTable);
        Assert.IsTrue(filter.ToString().Equals("VALUES (?A ?B) {" + Environment.NewLine + "      ( UNDEF <" + RDFVocabulary.RDF.BAG + "> )" + Environment.NewLine + "    }", StringComparison.Ordinal));
        Assert.IsTrue(filter.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("VALUES (?A ?B) {" + Environment.NewLine + "      ( UNDEF rdf:Bag )" + Environment.NewLine + "    }", StringComparison.Ordinal));
        Assert.IsTrue(filter.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(filter.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithMultipleBindings()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/")])
                .AddColumn(new RDFVariable("?B"), [new RDFPlainLiteral("hello", "en-US")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithUndefMultipleBindings()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [null])
                .AddColumn(new RDFVariable("?B"), [new RDFPlainLiteral("hello", "en-US")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithMultipleBindingsAndUnknownVariable()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/")])
                .AddColumn(new RDFVariable("?Q"), [new RDFPlainLiteral("hello", "en-US")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithMultipleBindingsBecauseNegation()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/")])
                .AddColumn(new RDFVariable("?B"), [new RDFPlainLiteral("hello")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowWithMultipleBindingsHavingNull()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", null },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/")])
                .AddColumn(new RDFVariable("?B"), [null]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsTrue(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndKeepRowsWithMultipleBindings()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example2.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/"), new RDFResource("http://example2.org/")])
                .AddColumn(new RDFVariable("?B"), [null, new RDFPlainLiteral("hello", "en-US")]));
        bool keepRow1 = filter.ApplyFilter(table.Rows[0], false);
        bool keepRow2 = filter.ApplyFilter(table.Rows[1], false);

        Assert.IsTrue(keepRow1);
        Assert.IsTrue(keepRow2);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndNotKeepRowWithMultipleBindings()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/test/")])
                .AddColumn(new RDFVariable("?B"), [new RDFPlainLiteral("hello", "en-US")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndNotKeepRowWithUndefMultipleBindings()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [null])
                .AddColumn(new RDFVariable("?B"), [new RDFPlainLiteral("hello")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], false);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndNotKeepRowWithMultipleBindingsBecauseNegation()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/")])
                .AddColumn(new RDFVariable("?B"), [new RDFPlainLiteral("hello", "en-US")]));
        bool keepRow = filter.ApplyFilter(table.Rows[0], true);

        Assert.IsFalse(keepRow);
    }

    [TestMethod]
    public void ShouldCreateValuesFilterAndNotKeepRowsWithMultipleBindings()
    {
        RDFTable table = new RDFTable();
        table.AddColumn("?A");
        table.AddColumn("?B");
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });
        table.AddRow(new Dictionary<string, string>()
        {
            { "?A", new RDFResource("http://example2.org/").ToString() },
            { "?B", new RDFPlainLiteral("hello", "en-US").ToString() },
        });

        RDFValuesFilter filter = new RDFValuesFilter(
            new RDFValues().AddColumn(new RDFVariable("?A"), [new RDFResource("http://example.org/"), new RDFResource("http://example3.org/")])
                .AddColumn(new RDFVariable("?B"), [null, new RDFPlainLiteral("hello", "en-US")]));
        bool keepRow1 = filter.ApplyFilter(table.Rows[0], false);
        bool keepRow2 = filter.ApplyFilter(table.Rows[1], false);

        Assert.IsTrue(keepRow1);
        Assert.IsFalse(keepRow2);
    }
    #endregion
}
