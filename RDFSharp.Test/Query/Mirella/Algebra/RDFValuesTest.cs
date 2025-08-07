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

using System;
using System.Data;
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
        Assert.IsFalse(values.IsInjected);
        Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES () {", Environment.NewLine, "    }")));
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
        Assert.IsFalse(values.IsInjected);
        Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "    }")));
        Assert.IsTrue(values.ToString([RDFNamespaceRegister.GetByPrefix("rdf")], string.Empty).Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( rdf:type <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "    }")));
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
        Assert.IsFalse(values.IsInjected);
        Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt> UNDEF UNDEF )", Environment.NewLine, "    }")));
        Assert.IsTrue(values.ToString([RDFNamespaceRegister.GetByPrefix("rdf")], string.Empty).Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( rdf:type <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "      ( rdf:Alt UNDEF UNDEF )", Environment.NewLine, "    }")));
        Assert.IsTrue(values.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(values.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldGetDataTable()
    {
        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?V1"), [RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT]);
        values.AddColumn(new RDFVariable("?V2"), [RDFVocabulary.FOAF.KNOWS]);
        values.AddColumn(new RDFVariable("?V3"), null);
        DataTable valuesTable = values.GetDataTable();

        Assert.IsNotNull(valuesTable);
        Assert.IsTrue(valuesTable.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
        Assert.IsTrue((bool)valuesTable.ExtendedProperties[RDFQueryEngine.IsOptional]);
        Assert.IsTrue(valuesTable.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
        Assert.IsFalse((bool)valuesTable.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
        Assert.AreEqual(3, valuesTable.Columns.Count);
        Assert.IsTrue(valuesTable.Columns.Contains("?V1"));
        Assert.IsTrue(valuesTable.Columns.Contains("?V2"));
        Assert.IsTrue(valuesTable.Columns.Contains("?V3"));
        Assert.AreEqual(2, valuesTable.Rows.Count);
        Assert.IsTrue(valuesTable.Rows[0]["?V1"].Equals(RDFVocabulary.RDF.TYPE.ToString()));
        Assert.IsTrue(valuesTable.Rows[0]["?V2"].Equals(RDFVocabulary.FOAF.KNOWS.ToString()));
        Assert.IsTrue(valuesTable.Rows[0]["?V3"].Equals(DBNull.Value));
        Assert.IsTrue(valuesTable.Rows[1]["?V1"].Equals(RDFVocabulary.RDF.ALT.ToString()));
        Assert.IsTrue(valuesTable.Rows[1]["?V2"].Equals(DBNull.Value));
        Assert.IsTrue(valuesTable.Rows[1]["?V3"].Equals(DBNull.Value));
    }

    [TestMethod]
    public void ShouldGetValuesFilter()
    {
        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?V1"), [new RDFResource("ex:res1")]);
        RDFValuesFilter valuesFilter = values.GetValuesFilter();

        Assert.IsNotNull(valuesFilter);
        Assert.IsTrue(valuesFilter.Values.Equals(values));
    }
    #endregion
}