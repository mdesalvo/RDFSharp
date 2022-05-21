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
        #endregion
    }
}