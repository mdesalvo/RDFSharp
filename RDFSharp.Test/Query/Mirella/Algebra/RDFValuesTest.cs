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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Query;
using RDFSharp.Model;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFValuesTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateValues()
    {
        RDFValues values = new RDFValues();

        Assert.IsNotNull(values);
        Assert.IsNotNull(values.Bindings);
        Assert.IsEmpty(values.Bindings);
        Assert.AreEqual(0, values.MaxBindingsLength());
        Assert.IsFalse(values.IsEvaluable);
        Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES () {", Environment.NewLine, "    }"), StringComparison.Ordinal));
        Assert.IsTrue(values.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(values.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddColumns()
    {
        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?V1"), [RDFVocabulary.RDF.TYPE]);
        values.AddColumn(new RDFVariable("?V2"), [RDFVocabulary.FOAF.KNOWS]);
        values.AddColumn(new RDFVariable("?V3"), null);

        Assert.IsNotNull(values);
        Assert.IsNotNull(values.Bindings);
        Assert.HasCount(3, values.Bindings);
        Assert.AreEqual(1, values.MaxBindingsLength());
        Assert.IsTrue(values.IsEvaluable);
        Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "    }"), StringComparison.Ordinal));
        Assert.IsTrue(values.ToString([RDFNamespaceRegister.GetByPrefix("rdf")], string.Empty).Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( rdf:type <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "    }"), StringComparison.Ordinal));
        Assert.IsTrue(values.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(values.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddColumnsAtVariableBindingsLength()
    {
        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?V1"), [RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT]);
        values.AddColumn(new RDFVariable("?V2"), [RDFVocabulary.FOAF.KNOWS]);
        values.AddColumn(new RDFVariable("?V3"), null);

        Assert.IsNotNull(values);
        Assert.IsNotNull(values.Bindings);
        Assert.HasCount(3, values.Bindings);
        Assert.AreEqual(2, values.MaxBindingsLength());
        Assert.IsTrue(values.IsEvaluable);
        Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt> UNDEF UNDEF )", Environment.NewLine, "    }"), StringComparison.Ordinal));
        Assert.IsTrue(values.ToString([RDFNamespaceRegister.GetByPrefix("rdf")], string.Empty).Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( rdf:type <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "      ( rdf:Alt UNDEF UNDEF )", Environment.NewLine, "    }"), StringComparison.Ordinal));
        Assert.IsTrue(values.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(values.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldGetRDFTable()
    {
        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?V1"), [RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT]);
        values.AddColumn(new RDFVariable("?V2"), [RDFVocabulary.FOAF.KNOWS]);
        values.AddColumn(new RDFVariable("?V3"), null);
        RDFTable valuesTable = values.GetRDFTable();

        Assert.IsNotNull(valuesTable);
        //Contains an UNDEF binding (?V3) => the values block behaves as optional
        Assert.IsTrue(valuesTable.IsOptional);
        Assert.AreEqual(3, valuesTable.Columns.Count);
        Assert.IsTrue(valuesTable.HasColumn("?V1"));
        Assert.IsTrue(valuesTable.HasColumn("?V2"));
        Assert.IsTrue(valuesTable.HasColumn("?V3"));
        Assert.AreEqual(2, valuesTable.Rows.Count);
        Assert.AreEqual(RDFVocabulary.RDF.TYPE.ToString(), valuesTable.Rows[0]["?V1"]);
        Assert.AreEqual(RDFVocabulary.FOAF.KNOWS.ToString(), valuesTable.Rows[0]["?V2"]);
        Assert.IsTrue(valuesTable.Rows[0].IsUnbound("?V3"));
        Assert.AreEqual(RDFVocabulary.RDF.ALT.ToString(), valuesTable.Rows[1]["?V1"]);
        Assert.IsTrue(valuesTable.Rows[1].IsUnbound("?V2"));
        Assert.IsTrue(valuesTable.Rows[1].IsUnbound("?V3"));
    }

    [TestMethod]
    public void ShouldAddEmptyColumn()
    {
        //'VALUES ?v { }' => a declared-but-empty column (zero rows), NOT a single all-UNDEF row
        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?V1"), []);

        Assert.HasCount(1, values.Bindings);
        Assert.IsEmpty(values.Bindings["?V1"]);
        Assert.AreEqual(0, values.MaxBindingsLength());
        Assert.IsTrue(values.IsEvaluable);

        RDFTable valuesTable = values.GetRDFTable();
        Assert.AreEqual(1, valuesTable.Columns.Count);
        Assert.IsTrue(valuesTable.HasColumn("?V1"));
        Assert.AreEqual(0, valuesTable.Rows.Count);
        //No row => no UNDEF => not optional
        Assert.IsFalse(valuesTable.IsOptional);
    }

    [TestMethod]
    public void ShouldAddSingleUndefRow()
    {
        //'VALUES ?v { UNDEF }' => one row whose single column is UNBOUND. UNDEF is a null ELEMENT inside the
        //bindings list (a row), which is DISTINCT from an empty/null list (zero rows = empty data block).
        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?V1"), [null]);

        Assert.HasCount(1, values.Bindings);
        Assert.HasCount(1, values.Bindings["?V1"]);
        Assert.IsNull(values.Bindings["?V1"][0]);
        Assert.AreEqual(1, values.MaxBindingsLength());
        Assert.IsTrue(values.IsEvaluable);
        Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES ?V1 { UNDEF }"), StringComparison.Ordinal));

        RDFTable valuesTable = values.GetRDFTable();
        Assert.AreEqual(1, valuesTable.Rows.Count);
        Assert.IsTrue(valuesTable.Rows[0].IsUnbound("?V1"));
        //An UNDEF binding makes the block behave as optional
        Assert.IsTrue(valuesTable.IsOptional);
    }

    [TestMethod]
    public void ShouldAddColumnWithUndefBetweenBoundValues()
    {
        //'VALUES ?v { 5 UNDEF 25 }' => an UNDEF row SANDWICHED between two bound rows: the null element must
        //keep its ordinal position (not truncate/shift the column), so the three rows stay 5 / UNBOUND / 25.
        RDFTypedLiteral five = new RDFTypedLiteral("5", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
        RDFTypedLiteral twentyfive = new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?V1"), [five, null, twentyfive]);

        Assert.HasCount(3, values.Bindings["?V1"]);
        Assert.AreEqual(3, values.MaxBindingsLength());
        Assert.AreEqual(five, values.Bindings["?V1"][0]);
        Assert.IsNull(values.Bindings["?V1"][1]);
        Assert.AreEqual(twentyfive, values.Bindings["?V1"][2]);

        RDFTable valuesTable = values.GetRDFTable();
        Assert.AreEqual(3, valuesTable.Rows.Count);
        Assert.AreEqual(five.ToString(), valuesTable.Rows[0]["?V1"]);
        Assert.IsTrue(valuesTable.Rows[1].IsUnbound("?V1"));
        Assert.AreEqual(twentyfive.ToString(), valuesTable.Rows[2]["?V1"]);
        //The interleaved UNDEF makes the block behave as optional
        Assert.IsTrue(valuesTable.IsOptional);
    }

    [TestMethod]
    public void ShouldGetRDFTableForNilBlock()
    {
        //'VALUES () { () () }' => zero columns, two empty-domain identity rows (multiset doubling)
        RDFValues values = new RDFValues { NilRowsCount = 2, IsEvaluable = true };

        Assert.IsEmpty(values.Bindings);
        Assert.AreEqual(2, values.MaxBindingsLength());

        RDFTable valuesTable = values.GetRDFTable();
        Assert.AreEqual(0, valuesTable.Columns.Count);
        Assert.AreEqual(2, valuesTable.Rows.Count);
        //No column => no UNDEF => not optional
        Assert.IsFalse(valuesTable.IsOptional);
    }
    #endregion
}