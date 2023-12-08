/*
   Copyright 2012-2024 Marco De Salvo

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
using System.Data;
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFDescribeQueryResultTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateDescribeQueryResult()
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult();

            Assert.IsNotNull(describeResult);
            Assert.IsNotNull(describeResult.DescribeResults);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 0);
            Assert.IsTrue(describeResult.DescribeResultsCount == 0);
        }

        [TestMethod]
        public void ShouldSerializeDescribeQueryResultToGraph()
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult();
            describeResult.DescribeResults.Columns.Add(new DataColumn("?SUBJECT", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?PREDICATE", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?OBJECT", typeof(string)));
            DataRow row0 = describeResult.DescribeResults.NewRow();
            row0["?SUBJECT"] = "ex:subj";
            row0["?PREDICATE"] = "ex:pred";
            row0["?OBJECT"] = "lit@EN-US";
            describeResult.DescribeResults.Rows.Add(row0);
            DataRow row1 = describeResult.DescribeResults.NewRow();
            row1["?SUBJECT"] = "bnode:12345";
            row1["?PREDICATE"] = $"{RDFVocabulary.RDF.TYPE}";
            row1["?OBJECT"] = $"ex:obj";
            describeResult.DescribeResults.Rows.Add(row1);
            DataRow row2 = describeResult.DescribeResults.NewRow();
            row2["?SUBJECT"] = "ex:subj";
            row2["?PREDICATE"] = "ex:pred";
            row2["?OBJECT"] = "lit";
            describeResult.DescribeResults.Rows.Add(row2);
            DataRow row3 = describeResult.DescribeResults.NewRow();
            row3["?SUBJECT"] = "ex:subj";
            row3["?PREDICATE"] = "ex:pred";
            row3["?OBJECT"] = $"lit^^{RDFVocabulary.XSD.STRING}";
            describeResult.DescribeResults.Rows.Add(row3);
            describeResult.DescribeResults.AcceptChanges();
            
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(describeResult.DescribeResultsCount == 4);

            RDFGraph describeGraph = describeResult.ToRDFGraph();
            
            Assert.IsNotNull(describeGraph);
            Assert.IsTrue(describeGraph.TriplesCount == 4);
            Assert.IsTrue(describeGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US"))));
            Assert.IsTrue(describeGraph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj"))));
            Assert.IsTrue(describeGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))));
            Assert.IsTrue(describeGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public async Task ShouldSerializeDescribeQueryResultToGraphAsync()
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult();
            describeResult.DescribeResults.Columns.Add(new DataColumn("?SUBJECT", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?PREDICATE", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?OBJECT", typeof(string)));
            DataRow row0 = describeResult.DescribeResults.NewRow();
            row0["?SUBJECT"] = "ex:subj";
            row0["?PREDICATE"] = "ex:pred";
            row0["?OBJECT"] = "lit@EN-US";
            describeResult.DescribeResults.Rows.Add(row0);
            DataRow row1 = describeResult.DescribeResults.NewRow();
            row1["?SUBJECT"] = "bnode:12345";
            row1["?PREDICATE"] = $"{RDFVocabulary.RDF.TYPE}";
            row1["?OBJECT"] = $"ex:obj";
            describeResult.DescribeResults.Rows.Add(row1);
            DataRow row2 = describeResult.DescribeResults.NewRow();
            row2["?SUBJECT"] = "ex:subj";
            row2["?PREDICATE"] = "ex:pred";
            row2["?OBJECT"] = "lit";
            describeResult.DescribeResults.Rows.Add(row2);
            DataRow row3 = describeResult.DescribeResults.NewRow();
            row3["?SUBJECT"] = "ex:subj";
            row3["?PREDICATE"] = "ex:pred";
            row3["?OBJECT"] = $"lit^^{RDFVocabulary.XSD.STRING}";
            describeResult.DescribeResults.Rows.Add(row3);
            describeResult.DescribeResults.AcceptChanges();
            
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(describeResult.DescribeResultsCount == 4);

            RDFGraph describeGraph = await describeResult.ToRDFGraphAsync();
            
            Assert.IsNotNull(describeGraph);
            Assert.IsTrue(describeGraph.TriplesCount == 4);
            Assert.IsTrue(describeGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US"))));
            Assert.IsTrue(describeGraph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj"))));
            Assert.IsTrue(describeGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))));
            Assert.IsTrue(describeGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldSerializeEmptyDescribeQueryResultToGraph()
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult();
            RDFGraph describeGraph = describeResult.ToRDFGraph();
            
            Assert.IsNotNull(describeGraph);
            Assert.IsTrue(describeGraph.TriplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeDescribeQueryResultFromGraph()
        {
            RDFGraph describeGraph = new RDFGraph();
            describeGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US")));
            describeGraph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj")));
            describeGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit")));
            describeGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFDescribeQueryResult describeResult = RDFDescribeQueryResult.FromRDFGraph(describeGraph);

            Assert.IsNotNull(describeResult);
            Assert.IsNotNull(describeResult.DescribeResults);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(describeResult.DescribeResultsCount == 4);
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?SUBJECT"].ToString().Equals("bnode:12345"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?PREDICATE"].ToString().Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?OBJECT"].ToString().Equals($"ex:obj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?OBJECT"].ToString().Equals("lit"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?OBJECT"].ToString().Equals($"lit^^{RDFVocabulary.XSD.STRING}"));
        }

        [TestMethod]
        public async Task ShouldDeserializeDescribeQueryResultFromGraphAsync()
        {
            RDFGraph describeGraph = new RDFGraph();
            describeGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US")));
            describeGraph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj")));
            describeGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit")));
            describeGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFDescribeQueryResult describeResult = await RDFDescribeQueryResult.FromRDFGraphAsync(describeGraph);

            Assert.IsNotNull(describeResult);
            Assert.IsNotNull(describeResult.DescribeResults);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(describeResult.DescribeResultsCount == 4);
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?SUBJECT"].ToString().Equals("bnode:12345"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?PREDICATE"].ToString().Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?OBJECT"].ToString().Equals($"ex:obj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?OBJECT"].ToString().Equals("lit"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?OBJECT"].ToString().Equals($"lit^^{RDFVocabulary.XSD.STRING}"));
        }

        [TestMethod]
        public void ShouldDeserializeDescribeQueryResultFromNullGraph()
        {
            RDFDescribeQueryResult describeResult = RDFDescribeQueryResult.FromRDFGraph(null);

            Assert.IsNotNull(describeResult);
            Assert.IsNotNull(describeResult.DescribeResults);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 0);
            Assert.IsTrue(describeResult.DescribeResultsCount == 0);
        }

        //STORE

        [TestMethod]
        public void ShouldSerializeDescribeQueryResultToStore()
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult();
            describeResult.DescribeResults.Columns.Add(new DataColumn("?CONTEXT", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?SUBJECT", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?PREDICATE", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?OBJECT", typeof(string)));
            DataRow row0 = describeResult.DescribeResults.NewRow();
            row0["?CONTEXT"] = "ex:ctx";
            row0["?SUBJECT"] = "ex:subj";
            row0["?PREDICATE"] = "ex:pred";
            row0["?OBJECT"] = "lit@EN-US";
            describeResult.DescribeResults.Rows.Add(row0);
            DataRow row1 = describeResult.DescribeResults.NewRow();
            row1["?CONTEXT"] = DBNull.Value.ToString();
            row1["?SUBJECT"] = "bnode:12345";
            row1["?PREDICATE"] = $"{RDFVocabulary.RDF.TYPE}";
            row1["?OBJECT"] = $"ex:obj";
            describeResult.DescribeResults.Rows.Add(row1);
            DataRow row2 = describeResult.DescribeResults.NewRow();
            row2["?CONTEXT"] = DBNull.Value.ToString();
            row2["?SUBJECT"] = "ex:subj";
            row2["?PREDICATE"] = "ex:pred";
            row2["?OBJECT"] = "lit";
            describeResult.DescribeResults.Rows.Add(row2);
            DataRow row3 = describeResult.DescribeResults.NewRow();
            row3["?CONTEXT"] = DBNull.Value.ToString();
            row3["?SUBJECT"] = "ex:subj";
            row3["?PREDICATE"] = "ex:pred";
            row3["?OBJECT"] = $"lit^^{RDFVocabulary.XSD.STRING}";
            describeResult.DescribeResults.Rows.Add(row3);
            describeResult.DescribeResults.AcceptChanges();
            
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(describeResult.DescribeResultsCount == 4);

            RDFMemoryStore describeStore = describeResult.ToRDFMemoryStore();
            
            Assert.IsNotNull(describeStore);
            Assert.IsTrue(describeStore.QuadruplesCount == 4);
            Assert.IsTrue(describeStore.ContainsQuadruple(new RDFQuadruple(new RDFContext(new Uri("ex:ctx")), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US"))));
            Assert.IsTrue(describeStore.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj"))));
            Assert.IsTrue(describeStore.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))));
            Assert.IsTrue(describeStore.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public async Task ShouldSerializeDescribeQueryResultToStoreAsync()
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult();
            describeResult.DescribeResults.Columns.Add(new DataColumn("?CONTEXT", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?SUBJECT", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?PREDICATE", typeof(string)));
            describeResult.DescribeResults.Columns.Add(new DataColumn("?OBJECT", typeof(string)));
            DataRow row0 = describeResult.DescribeResults.NewRow();
            row0["?CONTEXT"] = "ex:ctx";
            row0["?SUBJECT"] = "ex:subj";
            row0["?PREDICATE"] = "ex:pred";
            row0["?OBJECT"] = "lit@EN-US";
            describeResult.DescribeResults.Rows.Add(row0);
            DataRow row1 = describeResult.DescribeResults.NewRow();
            row1["?CONTEXT"] = DBNull.Value.ToString();
            row1["?SUBJECT"] = "bnode:12345";
            row1["?PREDICATE"] = $"{RDFVocabulary.RDF.TYPE}";
            row1["?OBJECT"] = $"ex:obj";
            describeResult.DescribeResults.Rows.Add(row1);
            DataRow row2 = describeResult.DescribeResults.NewRow();
            row2["?CONTEXT"] = DBNull.Value.ToString();
            row2["?SUBJECT"] = "ex:subj";
            row2["?PREDICATE"] = "ex:pred";
            row2["?OBJECT"] = "lit";
            describeResult.DescribeResults.Rows.Add(row2);
            DataRow row3 = describeResult.DescribeResults.NewRow();
            row3["?CONTEXT"] = DBNull.Value.ToString();
            row3["?SUBJECT"] = "ex:subj";
            row3["?PREDICATE"] = "ex:pred";
            row3["?OBJECT"] = $"lit^^{RDFVocabulary.XSD.STRING}";
            describeResult.DescribeResults.Rows.Add(row3);
            describeResult.DescribeResults.AcceptChanges();
            
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(describeResult.DescribeResultsCount == 4);

            RDFMemoryStore describeStore = await describeResult.ToRDFMemoryStoreAsync();
            
            Assert.IsNotNull(describeStore);
            Assert.IsTrue(describeStore.QuadruplesCount == 4);
            Assert.IsTrue(describeStore.ContainsQuadruple(new RDFQuadruple(new RDFContext(new Uri("ex:ctx")), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US"))));
            Assert.IsTrue(describeStore.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj"))));
            Assert.IsTrue(describeStore.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))));
            Assert.IsTrue(describeStore.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldSerializeEmptyDescribeQueryResultToStore()
        {
            RDFDescribeQueryResult describeResult = new RDFDescribeQueryResult();
            RDFMemoryStore describeStore = describeResult.ToRDFMemoryStore();
            
            Assert.IsNotNull(describeStore);
            Assert.IsTrue(describeStore.QuadruplesCount == 0);
        }

        [TestMethod]
        public void ShouldDeserializeDescribeQueryResultFromStore()
        {
            RDFMemoryStore describeStore = new RDFMemoryStore();
            describeStore.AddQuadruple(new RDFQuadruple(new RDFContext(new Uri("ex:ctx")), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US")));
            describeStore.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj")));
            describeStore.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit")));
            describeStore.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFDescribeQueryResult describeResult = RDFDescribeQueryResult.FromRDFMemoryStore(describeStore);

            Assert.IsNotNull(describeResult);
            Assert.IsNotNull(describeResult.DescribeResults);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(describeResult.DescribeResultsCount == 4);
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?CONTEXT"].ToString().Equals("ex:ctx"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?CONTEXT"].ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?SUBJECT"].ToString().Equals("bnode:12345"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?PREDICATE"].ToString().Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?OBJECT"].ToString().Equals($"ex:obj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?CONTEXT"].ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?OBJECT"].ToString().Equals("lit"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?CONTEXT"].ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?OBJECT"].ToString().Equals($"lit^^{RDFVocabulary.XSD.STRING}"));
        }

        [TestMethod]
        public async Task ShouldDeserializeDescribeQueryResultFromStoreAsync()
        {
            RDFMemoryStore describeStore = new RDFMemoryStore();
            describeStore.AddQuadruple(new RDFQuadruple(new RDFContext(new Uri("ex:ctx")), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US")));
            describeStore.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj")));
            describeStore.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit")));
            describeStore.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFDescribeQueryResult describeResult = await RDFDescribeQueryResult.FromRDFMemoryStoreAsync(describeStore);

            Assert.IsNotNull(describeResult);
            Assert.IsNotNull(describeResult.DescribeResults);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(describeResult.DescribeResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(describeResult.DescribeResultsCount == 4);
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?CONTEXT"].ToString().Equals("ex:ctx"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?CONTEXT"].ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?SUBJECT"].ToString().Equals("bnode:12345"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?PREDICATE"].ToString().Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[1]["?OBJECT"].ToString().Equals($"ex:obj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?CONTEXT"].ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[2]["?OBJECT"].ToString().Equals("lit"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?CONTEXT"].ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(describeResult.DescribeResults.Rows[3]["?OBJECT"].ToString().Equals($"lit^^{RDFVocabulary.XSD.STRING}"));
        }

        [TestMethod]
        public void ShouldDeserializeDescribeQueryResultFromNullStore()
        {
            RDFDescribeQueryResult describeResult = RDFDescribeQueryResult.FromRDFMemoryStore(null);

            Assert.IsNotNull(describeResult);
            Assert.IsNotNull(describeResult.DescribeResults);
            Assert.IsTrue(describeResult.DescribeResults.Columns.Count == 0);
            Assert.IsTrue(describeResult.DescribeResultsCount == 0);
        }
        #endregion
    }
}