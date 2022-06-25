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
using System.Threading.Tasks;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFInsertDataOperationTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateInsertDataOperation()
        {
            RDFInsertDataOperation operation = new RDFInsertDataOperation();

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.DeleteTemplates);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsNotNull(operation.InsertTemplates);
            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsNotNull(operation.Variables);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 0);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"INSERT DATA {
}"));
        }

        [TestMethod]
        public void ShouldAddInsertTemplate()
        {
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"));
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"));
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.DeleteTemplates);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsNotNull(operation.InsertTemplates);
            Assert.IsTrue(operation.InsertTemplates.Count == 2);
            Assert.IsNotNull(operation.Variables);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 3);
            Assert.IsTrue(operation.QueryMembers.Count == 0);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX owl: <http://www.w3.org/2002/07/owl#>

INSERT DATA {
  <ex:subj> <ex:pred> <ex:obj> .
  rdfs:Class rdf:type owl:Class .
}"));
        }

        [TestMethod]
        public void ShouldApplyToNullGraph()
        {
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFOperationResult result = operation.ApplyToGraph(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsNotNull(result.InsertResultsCount == 0);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToGraph()
        {
            RDFGraph graph = new RDFGraph();
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS)); //Duplicate triple...
            RDFOperationResult result = operation.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 3);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsNotNull(result.InsertResultsCount == 2);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToNullGraphAsync()
        {
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFOperationResult result = await operation.ApplyToGraphAsync(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsNotNull(result.InsertResultsCount == 0);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToGraphAsync()
        {
            RDFGraph graph = new RDFGraph();
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFOperationResult result = await operation.ApplyToGraphAsync(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 3);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsNotNull(result.InsertResultsCount == 2);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToNullStore()
        {
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFOperationResult result = operation.ApplyToStore(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsNotNull(result.InsertResultsCount == 0);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToStore()
        {
            RDFMemoryStore graph = new RDFMemoryStore();
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS)); //Duplicate triple...
            RDFOperationResult result = operation.ApplyToStore(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 4);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsNotNull(result.InsertResultsCount == 2);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?CONTEXT"].ToString(), $"{RDFNamespaceRegister.DefaultNamespace}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToNullStoreAsync()
        {
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            RDFOperationResult result = await operation.ApplyToStoreAsync(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsNotNull(result.InsertResultsCount == 0);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToStoreAsync()
        {
            RDFMemoryStore graph = new RDFMemoryStore();
            RDFInsertDataOperation operation = new RDFInsertDataOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS));
            operation.AddInsertTemplate(new RDFPattern(RDFVocabulary.RDFS.CLASS,RDFVocabulary.RDF.TYPE,RDFVocabulary.OWL.CLASS)); //Duplicate triple...
            RDFOperationResult result = await operation.ApplyToStoreAsync(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 4);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsNotNull(result.InsertResultsCount == 2);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:subj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), "ex:pred"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:obj"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?CONTEXT"].ToString(), $"{RDFNamespaceRegister.DefaultNamespace}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), $"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), $"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }
        #endregion
    }
}