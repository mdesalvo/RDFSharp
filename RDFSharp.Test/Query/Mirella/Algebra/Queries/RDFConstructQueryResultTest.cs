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
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFConstructQueryResultTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateConstructQueryResult()
    {
        RDFConstructQueryResult result = new RDFConstructQueryResult();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ConstructResults);
        Assert.AreEqual(0, result.ConstructResults.Columns.Count);
        Assert.AreEqual(0, result.ConstructResultsCount);
    }

    [TestMethod]
    public void ShouldSerializeConstructQueryResultToGraph()
    {
        RDFConstructQueryResult constructResult = new RDFConstructQueryResult();
        constructResult.ConstructResults.Columns.Add(new DataColumn("?SUBJECT", typeof(string)));
        constructResult.ConstructResults.Columns.Add(new DataColumn("?PREDICATE", typeof(string)));
        constructResult.ConstructResults.Columns.Add(new DataColumn("?OBJECT", typeof(string)));
        DataRow row0 = constructResult.ConstructResults.NewRow();
        row0["?SUBJECT"] = "ex:subj";
        row0["?PREDICATE"] = "ex:pred";
        row0["?OBJECT"] = "lit@EN-US";
        constructResult.ConstructResults.Rows.Add(row0);
        DataRow row1 = constructResult.ConstructResults.NewRow();
        row1["?SUBJECT"] = "bnode:12345";
        row1["?PREDICATE"] = $"{RDFVocabulary.RDF.TYPE}";
        row1["?OBJECT"] = "ex:obj";
        constructResult.ConstructResults.Rows.Add(row1);
        DataRow row2 = constructResult.ConstructResults.NewRow();
        row2["?SUBJECT"] = "ex:subj";
        row2["?PREDICATE"] = "ex:pred";
        row2["?OBJECT"] = "lit";
        constructResult.ConstructResults.Rows.Add(row2);
        DataRow row3 = constructResult.ConstructResults.NewRow();
        row3["?SUBJECT"] = "ex:subj";
        row3["?PREDICATE"] = "ex:pred";
        row3["?OBJECT"] = $"lit^^{RDFVocabulary.XSD.STRING}";
        constructResult.ConstructResults.Rows.Add(row3);
        constructResult.ConstructResults.AcceptChanges();

        Assert.AreEqual(3, constructResult.ConstructResults.Columns.Count);
        Assert.AreEqual(4, constructResult.ConstructResultsCount);

        RDFGraph constructGraph = constructResult.ToRDFGraph();

        Assert.IsNotNull(constructGraph);
        Assert.AreEqual(4, constructGraph.TriplesCount);
        Assert.IsTrue(constructGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US"))));
        Assert.IsTrue(constructGraph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj"))));
        Assert.IsTrue(constructGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))));
        Assert.IsTrue(constructGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
    }

    [TestMethod]
    public async Task ShouldSerializeConstructQueryResultToGraphAsync()
    {
        RDFConstructQueryResult constructResult = new RDFConstructQueryResult();
        constructResult.ConstructResults.Columns.Add(new DataColumn("?SUBJECT", typeof(string)));
        constructResult.ConstructResults.Columns.Add(new DataColumn("?PREDICATE", typeof(string)));
        constructResult.ConstructResults.Columns.Add(new DataColumn("?OBJECT", typeof(string)));
        DataRow row0 = constructResult.ConstructResults.NewRow();
        row0["?SUBJECT"] = "ex:subj";
        row0["?PREDICATE"] = "ex:pred";
        row0["?OBJECT"] = "lit@EN-US";
        constructResult.ConstructResults.Rows.Add(row0);
        DataRow row1 = constructResult.ConstructResults.NewRow();
        row1["?SUBJECT"] = "bnode:12345";
        row1["?PREDICATE"] = $"{RDFVocabulary.RDF.TYPE}";
        row1["?OBJECT"] = "ex:obj";
        constructResult.ConstructResults.Rows.Add(row1);
        DataRow row2 = constructResult.ConstructResults.NewRow();
        row2["?SUBJECT"] = "ex:subj";
        row2["?PREDICATE"] = "ex:pred";
        row2["?OBJECT"] = "lit";
        constructResult.ConstructResults.Rows.Add(row2);
        DataRow row3 = constructResult.ConstructResults.NewRow();
        row3["?SUBJECT"] = "ex:subj";
        row3["?PREDICATE"] = "ex:pred";
        row3["?OBJECT"] = $"lit^^{RDFVocabulary.XSD.STRING}";
        constructResult.ConstructResults.Rows.Add(row3);
        constructResult.ConstructResults.AcceptChanges();

        Assert.AreEqual(3, constructResult.ConstructResults.Columns.Count);
        Assert.AreEqual(4, constructResult.ConstructResultsCount);

        RDFGraph constructGraph = await constructResult.ToRDFGraphAsync();

        Assert.IsNotNull(constructGraph);
        Assert.AreEqual(4, constructGraph.TriplesCount);
        Assert.IsTrue(await constructGraph.ContainsTripleAsync(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US"))));
        Assert.IsTrue(await constructGraph.ContainsTripleAsync(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj"))));
        Assert.IsTrue(await constructGraph.ContainsTripleAsync(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))));
        Assert.IsTrue(await constructGraph.ContainsTripleAsync(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
    }

    [TestMethod]
    public void ShouldSerializeEmptyConstructQueryResultToGraph()
    {
        RDFConstructQueryResult constructResult = new RDFConstructQueryResult();
        RDFGraph constructGraph = constructResult.ToRDFGraph();

        Assert.IsNotNull(constructGraph);
        Assert.AreEqual(0, constructGraph.TriplesCount);
    }

    [TestMethod]
    public void ShouldDeserializeConstructQueryResultFromGraph()
    {
        RDFGraph constructGraph = new RDFGraph();
        constructGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US")));
        constructGraph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj")));
        constructGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit")));
        constructGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING)));
        RDFConstructQueryResult constructResult = RDFConstructQueryResult.FromRDFGraph(constructGraph);

        Assert.IsNotNull(constructResult);
        Assert.IsNotNull(constructResult.ConstructResults);
        Assert.AreEqual(3, constructResult.ConstructResults.Columns.Count);
        Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(4, constructResult.ConstructResultsCount);
        Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?SUBJECT"].ToString().Equals("ex:subj", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?PREDICATE"].ToString().Equals("ex:pred", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?SUBJECT"].ToString().Equals("bnode:12345", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?PREDICATE"].ToString().Equals($"{RDFVocabulary.RDF.TYPE}", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?OBJECT"].ToString().Equals("ex:obj", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?SUBJECT"].ToString().Equals("ex:subj", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?PREDICATE"].ToString().Equals("ex:pred", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?OBJECT"].ToString().Equals("lit", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?SUBJECT"].ToString().Equals("ex:subj", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?PREDICATE"].ToString().Equals("ex:pred", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?OBJECT"].ToString().Equals($"lit^^{RDFVocabulary.XSD.STRING}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ShouldDeserializeConstructQueryResultFromGraphAsync()
    {
        RDFGraph constructGraph = new RDFGraph();
        await constructGraph.AddTripleAsync(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US")));
        await constructGraph.AddTripleAsync(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj")));
        await constructGraph.AddTripleAsync(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit")));
        await constructGraph.AddTripleAsync(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING)));
        RDFConstructQueryResult constructResult = await RDFConstructQueryResult.FromRDFGraphAsync(constructGraph);

        Assert.IsNotNull(constructResult);
        Assert.IsNotNull(constructResult.ConstructResults);
        Assert.AreEqual(3, constructResult.ConstructResults.Columns.Count);
        Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(4, constructResult.ConstructResultsCount);
        Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?SUBJECT"].ToString().Equals("ex:subj", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?PREDICATE"].ToString().Equals("ex:pred", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?SUBJECT"].ToString().Equals("bnode:12345", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?PREDICATE"].ToString().Equals($"{RDFVocabulary.RDF.TYPE}", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?OBJECT"].ToString().Equals("ex:obj", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?SUBJECT"].ToString().Equals("ex:subj", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?PREDICATE"].ToString().Equals("ex:pred", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?OBJECT"].ToString().Equals("lit", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?SUBJECT"].ToString().Equals("ex:subj", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?PREDICATE"].ToString().Equals("ex:pred", System.StringComparison.Ordinal));
        Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?OBJECT"].ToString().Equals($"lit^^{RDFVocabulary.XSD.STRING}", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldDeserializeConstructQueryResultFromNullGraph()
    {
        RDFConstructQueryResult constructResult = RDFConstructQueryResult.FromRDFGraph(null);

        Assert.IsNotNull(constructResult);
        Assert.IsNotNull(constructResult.ConstructResults);
        Assert.AreEqual(0, constructResult.ConstructResults.Columns.Count);
        Assert.AreEqual(0, constructResult.ConstructResultsCount);
    }
    #endregion
}