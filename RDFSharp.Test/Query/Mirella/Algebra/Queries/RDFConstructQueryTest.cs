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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFConstructQueryTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        [TestCleanup]
        public void Cleanup()  { server.Stop(); server.Dispose(); }
        
        #region Tests
        [TestMethod]
        public void ShouldCreateConstructQuery()
        {
            RDFConstructQuery query = new RDFConstructQuery();

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.QueryMembers);
            Assert.IsTrue(query.QueryMembers.Count == 0);
            Assert.IsNotNull(query.Templates);
            Assert.IsTrue(query.Templates.Count == 0);
            Assert.IsNotNull(query.Variables);
            Assert.IsTrue(query.Variables.Count == 0);
            Assert.IsNotNull(query.Prefixes);
            Assert.IsTrue(query.Prefixes.Count == 0);
            Assert.IsTrue(query.IsEvaluable);
            Assert.IsFalse(query.IsOptional);
            Assert.IsFalse(query.JoinAsUnion);
            Assert.IsFalse(query.IsSubQuery);
            Assert.IsTrue(query.ToString().Equals("CONSTRUCT {" + Environment.NewLine + "}" + Environment.NewLine + "WHERE {" + Environment.NewLine + "}"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 0);
            Assert.IsTrue(query.GetPatternGroups().Count() == 0);
            Assert.IsTrue(query.GetSubQueries().Count() == 0);
            Assert.IsTrue(query.GetValues().Count() == 0);
            Assert.IsTrue(query.GetModifiers().Count() == 0);
            Assert.IsTrue(query.GetPrefixes().Count() == 0);
        }

        [TestMethod]
        public void ShouldCreateConstructQueryWithQueryMembers()
        {
            RDFConstructQuery query = new RDFConstructQuery();
            query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            query.AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O")));
            query.AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            query.AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)); //Will be discarded, since duplicate templates are not allowed
            query.AddTemplate(null); //Will be discarded, since null templates are not allowed
            query.AddPatternGroup(
                new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddFilter(new RDFIsUriFilter(new RDFVariable("?S"))));
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup()
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S"))
                    .AddProjectionVariable(new RDFVariable("?P")));
            query.AddModifier(new RDFDistinctModifier());
            query.AddModifier(new RDFLimitModifier(100));
            query.AddModifier(new RDFOffsetModifier(20));

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+""+Environment.NewLine+"CONSTRUCT {"+Environment.NewLine+"  ?S ?P ?O ."+Environment.NewLine+"  ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"}"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  {"+Environment.NewLine+"    SELECT ?S ?P"+Environment.NewLine+"    WHERE {"+Environment.NewLine+"      {"+Environment.NewLine+"        ?S ?P owl:Class ."+Environment.NewLine+"        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.Templates.Count == 2);
            Assert.IsTrue(query.Variables.Count == 4);
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 2);
            Assert.IsTrue(query.GetPatternGroups().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Count() == 1);
            Assert.IsTrue(query.GetValues().Count() == 1);
            Assert.IsTrue(query.GetModifiers().Count() == 3);
            Assert.IsTrue(query.GetPrefixes().Count() == 2);
        }

        [TestMethod]
        public void ShouldCreateConstructQueryWithOptionalQueryMembers()
        {
            RDFConstructQuery query = new RDFConstructQuery();
            query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            query.AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O")));
            query.AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            query.AddPatternGroup(
                new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddFilter(new RDFIsUriFilter(new RDFVariable("?S"))));
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup()
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S"))
                    .AddProjectionVariable(new RDFVariable("?P"))
                    .Optional());
            query.AddModifier(new RDFDistinctModifier());
            query.AddModifier(new RDFLimitModifier(100));
            query.AddModifier(new RDFOffsetModifier(20));

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+""+Environment.NewLine+"CONSTRUCT {"+Environment.NewLine+"  ?S ?P ?O ."+Environment.NewLine+"  ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"}"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  OPTIONAL {"+Environment.NewLine+"    SELECT ?S ?P"+Environment.NewLine+"    WHERE {"+Environment.NewLine+"      {"+Environment.NewLine+"        ?S ?P owl:Class ."+Environment.NewLine+"        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.Templates.Count == 2);
            Assert.IsTrue(query.Variables.Count == 4);
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 2);
            Assert.IsTrue(query.GetPatternGroups().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Single().IsOptional);
            Assert.IsTrue(query.GetValues().Count() == 1);
            Assert.IsTrue(query.GetModifiers().Count() == 3);
            Assert.IsTrue(query.GetPrefixes().Count() == 2);
        }

        [TestMethod]
        public void ShouldCreateConstructQueryWithUnionQueryMembers()
        {
            RDFConstructQuery query = new RDFConstructQuery();
            query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            query.AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O")));
            query.AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            query.AddPatternGroup(
                new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddFilter(new RDFIsUriFilter(new RDFVariable("?S"))));
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup()
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S"))
                    .AddProjectionVariable(new RDFVariable("?P"))
                    .UnionWithNext());
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup()
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S"))
                    .AddProjectionVariable(new RDFVariable("?P")));
            query.AddModifier(new RDFDistinctModifier());
            query.AddModifier(new RDFLimitModifier(100));
            query.AddModifier(new RDFOffsetModifier(20));

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+""+Environment.NewLine+"CONSTRUCT {"+Environment.NewLine+"  ?S ?P ?O ."+Environment.NewLine+"  ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"}"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  {"+Environment.NewLine+"    {"+Environment.NewLine+"      SELECT ?S ?P"+Environment.NewLine+"      WHERE {"+Environment.NewLine+"        {"+Environment.NewLine+"          ?S ?P owl:Class ."+Environment.NewLine+"          VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"        }"+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"    UNION"+Environment.NewLine+"    {"+Environment.NewLine+"      SELECT ?S ?P"+Environment.NewLine+"      WHERE {"+Environment.NewLine+"        {"+Environment.NewLine+"          ?S ?P owl:Class ."+Environment.NewLine+"          VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"        }"+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.Templates.Count == 2);
            Assert.IsTrue(query.Variables.Count == 4);
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 3);
            Assert.IsTrue(query.GetPatternGroups().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Count() == 2);
            Assert.IsTrue(query.GetSubQueries().ElementAt(0).JoinAsUnion);
            Assert.IsFalse(query.GetSubQueries().ElementAt(1).JoinAsUnion);
            Assert.IsTrue(query.GetValues().Count() == 2);
            Assert.IsTrue(query.GetModifiers().Count() == 3);
            Assert.IsTrue(query.GetPrefixes().Count() == 2);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndHaveResultsWithTemplateHavingFixedSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFResource("ex:flower"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndHaveResultsWithTemplateHavingFixedPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndHaveResultsWithTemplateHavingFixedObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndHaveResultsWithGroundTemplate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFResource("ex:flower"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDFS.SUB_CLASS_OF}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{new RDFResource("ex:plant")}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecauseNoTemplates()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecauseUnknownSubjectVariable()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?Q"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecauseSubjectVariableContainsLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("flower")));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("ex:flower"), new RDFVariable("?P"), new RDFVariable("?S"))));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecauseSubjectVariableContainsNullValue()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?L"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.LABEL))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecauseUnknownPredicateVariable()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?Q"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecausePredicateVariableContainsLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("flower")));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("ex:flower"), new RDFVariable("?S"), new RDFVariable("?P"))));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecausePredicateVariableContainsBlankNode()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("ex:flower"), new RDFVariable("?S"), new RDFVariable("?P"))));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecausePredicateVariableContainsNullValue()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(RDFVocabulary.RDF.TYPE, new RDFVariable("?L"), RDFVocabulary.RDFS.LABEL))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecauseUnknownObjectVariable()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?Q")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToGraphAndNotHaveResultsBecauseObjectVariableContainsNullValue()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToNullGraphAndNotHaveResults()
        {
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndHaveResults()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndHaveResultsWithTemplateHavingFixedSubject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:flower"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndHaveResultsWithTemplateHavingFixedPredicate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndHaveResultsWithTemplateHavingFixedObject()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndHaveResultsWithGroundTemplate()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDFS.SUB_CLASS_OF}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{new RDFResource("ex:plant")}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResults()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseNoTemplates()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseUnknownSubjectVariable()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Q"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseSubjectVariableContainsLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("flower")));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("ex:flower"), new RDFVariable("?P"), new RDFVariable("?S"))));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseSubjectVariableContainsNullValue()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?L"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.LABEL))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseUnknownPredicateVariable()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?Q"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecausePredicateVariableContainsLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("flower")));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("ex:flower"), new RDFVariable("?S"), new RDFVariable("?P"))));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecausePredicateVariableContainsBlankNode()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("ex:flower"), new RDFVariable("?S"), new RDFVariable("?P"))));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecausePredicateVariableContainsNullValue()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), RDFVocabulary.RDF.TYPE, new RDFVariable("?L"), RDFVocabulary.RDFS.LABEL))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseUnknownObjectVariable()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?Q")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseObjectVariableContainsNullValue()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToNullStoreAndNotHaveResults()
        {
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndHaveResultsWithTemplateHavingFixedSubject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFResource("ex:flower"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndHaveResultsWithTemplateHavingFixedPredicate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndHaveResultsWithTemplateHavingFixedObject()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndHaveResultsWithGroundTemplate()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFResource("ex:flower"), RDFVocabulary.RDFS.SUB_CLASS_OF, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDFS.SUB_CLASS_OF}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{new RDFResource("ex:plant")}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecauseNoTemplates()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecauseUnknownSubjectVariable()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?Q"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecauseSubjectVariableContainsLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("flower")));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("ex:flower"), new RDFVariable("?P"), new RDFVariable("?S"))));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecauseSubjectVariableContainsNullValue()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?L"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.LABEL))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecauseUnknownPredicateVariable()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?Q"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecausePredicateVariableContainsLiteral()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("flower")));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("ex:flower"), new RDFVariable("?S"), new RDFVariable("?P"))));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecausePredicateVariableContainsBlankNode()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFResource("bnode:12345")));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("ex:flower"), new RDFVariable("?S"), new RDFVariable("?P"))));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecausePredicateVariableContainsNullValue()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(RDFVocabulary.RDF.TYPE, new RDFVariable("?L"), RDFVocabulary.RDFS.LABEL))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecauseUnknownObjectVariable()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?Q")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToFederationAndNotHaveResultsBecauseObjectVariableContainsNullValue()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
            RDFConstructQueryResult result = query.ApplyToFederation(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToNullFederationAndNotHaveResults()
        {
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToFederation(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToSPARQLEndpointAndHaveResults()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldApplyConstructQueryToSPARQLEndpointAndHaveResults/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/turtle")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldApplyConstructQueryToSPARQLEndpointAndHaveResults/sparql"));
            RDFConstructQueryResult result = query.ApplyToSPARQLEndpoint(endpoint);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToSPARQLEndpointAndNotHaveResults()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldApplyConstructQueryToSPARQLEndpointAndNotHaveResults/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody("", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/turtle")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldApplyConstructQueryToSPARQLEndpointAndNotHaveResults/sparql"));
            RDFConstructQueryResult result = query.ApplyToSPARQLEndpoint(endpoint);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
        }

        [TestMethod]
        public void ShouldApplyRawConstructQueryToSPARQLEndpoint()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldApplyRawConstructQueryToSPARQLEndpoint/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/turtle")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldApplyRawConstructQueryToSPARQLEndpoint/sparql"));
            RDFConstructQueryResult result = RDFConstructQuery.ApplyRawToSPARQLEndpoint(query.ToString(), endpoint);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToNullSPARQLEndpointAndNotHaveResults()
        {
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToSPARQLEndpoint(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenApplyingConstructQueryToSPARQLEndpointAccordingToTimingAndBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldThrowExceptionWhenApplyingConstructQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(750));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldThrowExceptionWhenApplyingConstructQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql"));

            Assert.ThrowsException<RDFQueryException>(() => query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(250, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)));
        }

        [TestMethod]
        public void ShouldGiveEmptyResultWhenApplyingConstructQueryToSPARQLEndpointAccordingToTimingAndBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldGiveEmptyResultWhenApplyingConstructQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody("", encoding: Encoding.UTF8)
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(750));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldGiveEmptyResultWhenApplyingConstructQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql"));
            RDFConstructQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(250, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult));

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionWhenApplyingConstructQueryToSPARQLEndpointAccordingToBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldThrowExceptionWhenApplyingConstructQueryToSPARQLEndpointAccordingToBehavior/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody("Internal Server Error")
                            .WithStatusCode(HttpStatusCode.InternalServerError)
                            .WithFault(FaultType.NONE));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldThrowExceptionWhenApplyingConstructQueryToSPARQLEndpointAccordingToBehavior/sparql"));

            Assert.ThrowsException<RDFQueryException>(() => query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(750, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)));
        }

        [TestMethod]
        public void ShouldGiveEmptyResultWhenApplyingConstructQueryToSPARQLEndpointAccordingToBehavior()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldGiveEmptyResultWhenApplyingConstructQueryToSPARQLEndpointAccordingToBehavior/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody("Internal Server Error")
                            .WithStatusCode(HttpStatusCode.InternalServerError)
                            .WithFault(FaultType.NONE));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldGiveEmptyResultWhenApplyingConstructQueryToSPARQLEndpointAccordingToBehavior/sparql"));
            RDFConstructQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(750, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult));

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 0);
        }

        //ASYNC

        [TestMethod]
        public async Task ShouldApplyConstructQueryToGraphAsyncAndHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = await query.ApplyToGraphAsync(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToGraphAsyncAndNotHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = await query.ApplyToGraphAsync(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToNullGraphAsyncAndNotHaveResults()
        {
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = await query.ApplyToGraphAsync(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToStoreAsyncAndHaveResults()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = await query.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToStoreAsyncAndNotHaveResults()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = await query.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToNullStoreAsyncAndNotHaveResults()
        {
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = await query.ApplyToStoreAsync(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToFederationAsyncAndHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = await query.ApplyToFederationAsync(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToFederationAsyncAndNotHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFFederation federation = new RDFFederation();
            federation.AddGraph(graph);
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = await query.ApplyToFederationAsync(federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToNullFederationAsyncAndNotHaveResults()
        {
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = await query.ApplyToFederationAsync(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToSPARQLEndpointAsyncAndHaveResults()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldApplyConstructQueryToSPARQLEndpointAsyncAndHaveResults/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/turtle")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldApplyConstructQueryToSPARQLEndpointAsyncAndHaveResults/sparql"));
            RDFConstructQueryResult result = await query.ApplyToSPARQLEndpointAsync(endpoint);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToSPARQLEndpointAsyncAndNotHaveResults()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldApplyConstructQueryToSPARQLEndpointAsyncAndNotHaveResults/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody("", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/turtle")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldApplyConstructQueryToSPARQLEndpointAsyncAndNotHaveResults/sparql"));
            RDFConstructQueryResult result = await query.ApplyToSPARQLEndpointAsync(endpoint);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
        }

        [TestMethod]
        public async Task ShouldApplyRawConstructQueryToSPARQLEndpointAsync()
        {
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFConstructQueryTest/ShouldApplyRawConstructQueryToSPARQLEndpointAsync/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                            .WithHeader("Content-Type", "application/turtle")
                            .WithStatusCode(HttpStatusCode.OK));

            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFConstructQueryTest/ShouldApplyRawConstructQueryToSPARQLEndpointAsync/sparql"));
            RDFConstructQueryResult result = await RDFConstructQuery.ApplyRawToSPARQLEndpointAsync(query.ToString(), endpoint);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
        }

        [TestMethod]
        public async Task ShouldApplyConstructQueryToNullSPARQLEndpointAsyncAndNotHaveResults()
        {
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:plant")))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = await query.ApplyToSPARQLEndpointAsync(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 0);
        }
        #endregion
    }
}