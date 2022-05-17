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
            Assert.IsTrue(query.ToString().Equals("CONSTRUCT" + Environment.NewLine + "{" + Environment.NewLine + "}" + Environment.NewLine + "WHERE {" + Environment.NewLine + "}"));
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
                new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddFilter(new RDFIsUriFilter(new RDFVariable("?S"))));
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup("PG1")
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S"))
                    .AddProjectionVariable(new RDFVariable("?P")));
            query.AddModifier(new RDFDistinctModifier());
            query.AddModifier(new RDFLimitModifier(100));
            query.AddModifier(new RDFOffsetModifier(20));

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+""+Environment.NewLine+"CONSTRUCT"+Environment.NewLine+"{"+Environment.NewLine+"  ?S ?P ?O ."+Environment.NewLine+"  ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"}"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  {"+Environment.NewLine+"    SELECT ?S ?P"+Environment.NewLine+"    WHERE {"+Environment.NewLine+"      {"+Environment.NewLine+"        ?S ?P owl:Class ."+Environment.NewLine+"        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
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
                new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddFilter(new RDFIsUriFilter(new RDFVariable("?S"))));
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup("PG1")
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S"))
                    .AddProjectionVariable(new RDFVariable("?P"))
                    .Optional());
            query.AddModifier(new RDFDistinctModifier());
            query.AddModifier(new RDFLimitModifier(100));
            query.AddModifier(new RDFOffsetModifier(20));

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+""+Environment.NewLine+"CONSTRUCT"+Environment.NewLine+"{"+Environment.NewLine+"  ?S ?P ?O ."+Environment.NewLine+"  ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"}"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  OPTIONAL {"+Environment.NewLine+"    SELECT ?S ?P"+Environment.NewLine+"    WHERE {"+Environment.NewLine+"      {"+Environment.NewLine+"        ?S ?P owl:Class ."+Environment.NewLine+"        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
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
                new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddFilter(new RDFIsUriFilter(new RDFVariable("?S"))));
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup("PG1")
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S"))
                    .AddProjectionVariable(new RDFVariable("?P"))
                    .UnionWithNext());
            query.AddSubQuery(
                new RDFSelectQuery()
                    .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                    .AddPatternGroup(
                        new RDFPatternGroup("PG2")
                            .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { RDFVocabulary.RDFS.CLASS })))
                    .AddProjectionVariable(new RDFVariable("?S"))
                    .AddProjectionVariable(new RDFVariable("?P")));
            query.AddModifier(new RDFDistinctModifier());
            query.AddModifier(new RDFLimitModifier(100));
            query.AddModifier(new RDFOffsetModifier(20));

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+""+Environment.NewLine+"CONSTRUCT"+Environment.NewLine+"{"+Environment.NewLine+"  ?S ?P ?O ."+Environment.NewLine+"  ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"}"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  {"+Environment.NewLine+"    {"+Environment.NewLine+"      SELECT ?S ?P"+Environment.NewLine+"      WHERE {"+Environment.NewLine+"        {"+Environment.NewLine+"          ?S ?P owl:Class ."+Environment.NewLine+"          VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"        }"+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"    UNION"+Environment.NewLine+"    {"+Environment.NewLine+"      SELECT ?S ?P"+Environment.NewLine+"      WHERE {"+Environment.NewLine+"        {"+Environment.NewLine+"          ?S ?P owl:Class ."+Environment.NewLine+"          VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"        }"+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.ConstructResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndHaveResultsWithTemplateHavingFixedContext()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(result.ConstructResults.Rows.Count == 1);
            Assert.IsTrue(result.ConstructResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseUnknownContextVariable()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?Q"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.DATATYPE)));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseContextVariableContainsLiteral()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("flower")));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("ex:flower"), new RDFVariable("?P"), new RDFVariable("?C"))));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("ex:flower"), new RDFVariable("?P"), new RDFVariable("?S"))));
            RDFConstructQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyConstructQueryToStoreAndNotHaveResultsBecauseContextVariableContainsNullValue()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            RDFConstructQuery query = new RDFConstructQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddTemplate(new RDFPattern(new RDFVariable("?L"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.LABEL))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?L")).Optional()));
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFConstructQueryResult result = query.ApplyToStore(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }
        #endregion
    }
}