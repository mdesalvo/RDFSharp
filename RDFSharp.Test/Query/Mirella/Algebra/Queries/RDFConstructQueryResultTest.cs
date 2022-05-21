/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Data;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
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
            Assert.IsTrue(result.ConstructResults.Columns.Count == 0);
            Assert.IsTrue(result.ConstructResultsCount == 0);
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
            row1["?OBJECT"] = $"ex:obj";
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
            
            Assert.IsTrue(constructResult.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(constructResult.ConstructResultsCount == 4);

            RDFGraph constructGraph = constructResult.ToRDFGraph();
            
            Assert.IsNotNull(constructGraph);
            Assert.IsTrue(constructGraph.TriplesCount == 4);
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
            row1["?OBJECT"] = $"ex:obj";
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
            
            Assert.IsTrue(constructResult.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(constructResult.ConstructResultsCount == 4);

            RDFGraph constructGraph = await constructResult.ToRDFGraphAsync();
            
            Assert.IsNotNull(constructGraph);
            Assert.IsTrue(constructGraph.TriplesCount == 4);
            Assert.IsTrue(constructGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US"))));
            Assert.IsTrue(constructGraph.ContainsTriple(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj"))));
            Assert.IsTrue(constructGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit"))));
            Assert.IsTrue(constructGraph.ContainsTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        [TestMethod]
        public void ShouldSerializeEmptyConstructQueryResultToGraph()
        {
            RDFConstructQueryResult constructResult = new RDFConstructQueryResult();
            RDFGraph constructGraph = constructResult.ToRDFGraph();
            
            Assert.IsNotNull(constructGraph);
            Assert.IsTrue(constructGraph.TriplesCount == 0);
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
            Assert.IsTrue(constructResult.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(constructResult.ConstructResultsCount == 4);
            Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?SUBJECT"].ToString().Equals("bnode:12345"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?PREDICATE"].ToString().Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?OBJECT"].ToString().Equals($"ex:obj"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?OBJECT"].ToString().Equals("lit"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?OBJECT"].ToString().Equals($"lit^^{RDFVocabulary.XSD.STRING}"));
        }

        [TestMethod]
        public async Task ShouldDeserializeConstructQueryResultFromGraphAsync()
        {
            RDFGraph constructGraph = new RDFGraph();
            constructGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit","en-US")));
            constructGraph.AddTriple(new RDFTriple(new RDFResource("bnode:12345"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:obj")));
            constructGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFPlainLiteral("lit")));
            constructGraph.AddTriple(new RDFTriple(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFTypedLiteral("lit", RDFModelEnums.RDFDatatypes.XSD_STRING)));
            RDFConstructQueryResult constructResult = await RDFConstructQueryResult.FromRDFGraphAsync(constructGraph);

            Assert.IsNotNull(constructResult);
            Assert.IsNotNull(constructResult.ConstructResults);
            Assert.IsTrue(constructResult.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(constructResult.ConstructResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(constructResult.ConstructResultsCount == 4);
            Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[0]["?OBJECT"].ToString().Equals("lit@EN-US"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?SUBJECT"].ToString().Equals("bnode:12345"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?PREDICATE"].ToString().Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[1]["?OBJECT"].ToString().Equals($"ex:obj"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[2]["?OBJECT"].ToString().Equals("lit"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?SUBJECT"].ToString().Equals("ex:subj"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?PREDICATE"].ToString().Equals("ex:pred"));
            Assert.IsTrue(constructResult.ConstructResults.Rows[3]["?OBJECT"].ToString().Equals($"lit^^{RDFVocabulary.XSD.STRING}"));
        }

        [TestMethod]
        public void ShouldDeserializeConstructQueryResultFromNullGraph()
        {
            RDFConstructQueryResult constructResult = RDFConstructQueryResult.FromRDFGraph(null);

            Assert.IsNotNull(constructResult);
            Assert.IsNotNull(constructResult.ConstructResults);
            Assert.IsTrue(constructResult.ConstructResults.Columns.Count == 0);
            Assert.IsTrue(constructResult.ConstructResultsCount == 0);
        }
        #endregion
    }
}