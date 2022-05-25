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
    public class RDFDescribeQueryTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        [TestCleanup]
        public void Cleanup()  { server.Stop(); server.Dispose(); }
        
        #region Tests
        [TestMethod]
        public void ShouldCreateDescribeQuery()
        {
            RDFDescribeQuery query = new RDFDescribeQuery();

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.QueryMembers);
            Assert.IsTrue(query.QueryMembers.Count == 0);
            Assert.IsNotNull(query.DescribeTerms);
            Assert.IsTrue(query.DescribeTerms.Count == 0);
            Assert.IsNotNull(query.Variables);
            Assert.IsTrue(query.Variables.Count == 0);
            Assert.IsNotNull(query.Prefixes);
            Assert.IsTrue(query.Prefixes.Count == 0);
            Assert.IsTrue(query.IsEvaluable);
            Assert.IsFalse(query.IsOptional);
            Assert.IsFalse(query.JoinAsUnion);
            Assert.IsFalse(query.IsSubQuery);
            Assert.IsTrue(query.ToString().Equals("DESCRIBE *" + Environment.NewLine + "WHERE {" + Environment.NewLine + "}"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 0);
            Assert.IsTrue(query.GetPatternGroups().Count() == 0);
            Assert.IsTrue(query.GetSubQueries().Count() == 0);
            Assert.IsTrue(query.GetValues().Count() == 0);
            Assert.IsTrue(query.GetModifiers().Count() == 0);
            Assert.IsTrue(query.GetPrefixes().Count() == 0);
        }

        [TestMethod]
        public void ShouldCreateDescribeQueryWithQueryMembers()
        {
            RDFDescribeQuery query = new RDFDescribeQuery();
            query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            query.AddDescribeTerm(new RDFVariable("?S"));
            query.AddDescribeTerm(new RDFVariable("?S")); //Will be discarded, since duplicate terms are not allowed
            query.AddDescribeTerm(null as RDFVariable); //Will be discarded, since null templates are not allowed
            query.AddDescribeTerm(new RDFResource("ex:flower"));
            query.AddDescribeTerm(new RDFResource("ex:flower")); //Will be discarded, since duplicate terms are not allowed
            query.AddDescribeTerm(null as RDFResource); //Will be discarded, since null templates are not allowed
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

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+Environment.NewLine+"DESCRIBE ?S <ex:flower>"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  {"+Environment.NewLine+"    SELECT ?S ?P"+Environment.NewLine+"    WHERE {"+Environment.NewLine+"      {"+Environment.NewLine+"        ?S ?P owl:Class ."+Environment.NewLine+"        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.DescribeTerms.Count == 2);
            Assert.IsTrue(query.Variables.Count == 1);
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 2);
            Assert.IsTrue(query.GetPatternGroups().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Count() == 1);
            Assert.IsTrue(query.GetValues().Count() == 1);
            Assert.IsTrue(query.GetModifiers().Count() == 3);
            Assert.IsTrue(query.GetPrefixes().Count() == 2);
        }

        [TestMethod]
        public void ShouldCreateDescribeQueryWithOptionalQueryMembers()
        {
            RDFDescribeQuery query = new RDFDescribeQuery();
            query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            query.AddDescribeTerm(new RDFVariable("?S"));
            query.AddDescribeTerm(new RDFResource("ex:flower"));
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

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+""+Environment.NewLine+"DESCRIBE ?S <ex:flower>"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"    FILTER ( ISURI(?S) ) "+Environment.NewLine+"  }"+Environment.NewLine+"  OPTIONAL {"+Environment.NewLine+"    SELECT ?S ?P"+Environment.NewLine+"    WHERE {"+Environment.NewLine+"      {"+Environment.NewLine+"        ?S ?P owl:Class ."+Environment.NewLine+"        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.DescribeTerms.Count == 2);
            Assert.IsTrue(query.Variables.Count == 1);
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 2);
            Assert.IsTrue(query.GetPatternGroups().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Single().IsOptional);
            Assert.IsTrue(query.GetValues().Count() == 1);
            Assert.IsTrue(query.GetModifiers().Count() == 3);
            Assert.IsTrue(query.GetPrefixes().Count() == 2);
        }

        [TestMethod]
        public void ShouldCreateDescribeQueryWithUnionQueryMembers()
        {
            RDFDescribeQuery query = new RDFDescribeQuery();
            query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            query.AddDescribeTerm(new RDFVariable("?S"));
            query.AddDescribeTerm(new RDFResource("ex:flower"));
            query.AddPatternGroup(
                new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                    .AddFilter(new RDFIsUriFilter(new RDFVariable("?S")))
                    .UnionWithNext());
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

            Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>"+Environment.NewLine+"PREFIX owl: <http://www.w3.org/2002/07/owl#>"+Environment.NewLine+""+Environment.NewLine+"DESCRIBE ?S <ex:flower>"+Environment.NewLine+"WHERE {"+Environment.NewLine+"  {"+Environment.NewLine+"    {"+Environment.NewLine+"      ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ."+Environment.NewLine+"      FILTER ( ISURI(?S) ) "+Environment.NewLine+"    }"+Environment.NewLine+"    UNION"+Environment.NewLine+"    {"+Environment.NewLine+"      SELECT ?S ?P"+Environment.NewLine+"      WHERE {"+Environment.NewLine+"        {"+Environment.NewLine+"          ?S ?P owl:Class ."+Environment.NewLine+"          VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ."+Environment.NewLine+"        }"+Environment.NewLine+"      }"+Environment.NewLine+"    }"+Environment.NewLine+"  }"+Environment.NewLine+"}"+Environment.NewLine+"LIMIT 100"+Environment.NewLine+"OFFSET 20"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.DescribeTerms.Count == 2);
            Assert.IsTrue(query.Variables.Count == 1);
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 2);
            Assert.IsTrue(query.GetPatternGroups().Count() == 1);
            Assert.IsTrue(query.GetPatternGroups().Single().JoinAsUnion);
            Assert.IsTrue(query.GetSubQueries().Count() == 1);
            Assert.IsTrue(query.GetValues().Count() == 1);
            Assert.IsTrue(query.GetModifiers().Count() == 3);
            Assert.IsTrue(query.GetPrefixes().Count() == 2);
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithStarTerms()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddModifier(new RDFDistinctModifier());
            RDFDescribeQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 3);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithOneResourceTerm()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFResource("ex:flower"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithOneVariableTerm()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?S"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithMoreVariableTerms()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?S"))
                .AddDescribeTerm(new RDFVariable("?P"))
                .AddDescribeTerm(new RDFVariable("?L"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))))
                .AddModifier(new RDFDistinctModifier());
            RDFDescribeQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 3);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithNoBody()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFResource("ex:flower"))
                .AddDescribeTerm(new RDFVariable("?Q")); //This variable will not be evaluated, since it is not part of results table
            RDFDescribeQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToGraphAndNotHaveResults()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?S"))
                .AddDescribeTerm(new RDFVariable("?P"))
                .AddDescribeTerm(new RDFVariable("?Q")) //This variable will not be evaluated, since it is not part of results table
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 0);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToGraphAndNotHaveResultsBecauseUnboundVariable()
        {
            RDFGraph graph = new RDFGraph();
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?L"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))).Optional()
                    .AddFilter(new RDFBooleanNotFilter(new RDFBoundFilter(new RDFVariable("?L")))));
            RDFDescribeQueryResult result = query.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 0);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToNullGraphAndNotHaveResults()
        {
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?S"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddModifier(new RDFDistinctModifier());
            RDFDescribeQueryResult result = query.ApplyToGraph(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 0);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 0);
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndHaveResultsWithStarTerms()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddModifier(new RDFDistinctModifier());
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 3);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndHaveResultsWithOneResourceTerm()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFResource("ex:flower"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndHaveResultsWithOneContextResourceTerm()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFResource("ex:ctx2"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 1);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx2"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:tree"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndHaveResultsWithOneBlankResourceTerm()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFResource("bnode:12345"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 1);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx2"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("bnode:12345"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndHaveResultsWithOneVariableTerm()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?S"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndHaveResultsWithOneContextVariableTerm()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?C"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 3);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx1"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx1"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx2"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndHaveResultsWithMoreVariableTerms()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?S"))
                .AddDescribeTerm(new RDFVariable("?P"))
                .AddDescribeTerm(new RDFVariable("?L"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))))
                .AddModifier(new RDFDistinctModifier());
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 3);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx1"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx1"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx2"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndHaveResultsWithNoBody()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFResource("ex:flower"))
                .AddDescribeTerm(new RDFVariable("?Q")); //This variable will not be evaluated, since it is not part of results table
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"ex:color"));
            Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals($"white@EN"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndNotHaveResults()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?S"))
                .AddDescribeTerm(new RDFVariable("?P"))
                .AddDescribeTerm(new RDFVariable("?Q")) //This variable will not be evaluated, since it is not part of results table
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 0);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToStoreAndNotHaveResultsBecauseUnboundVariable()
        {
            RDFMemoryStore store = new RDFMemoryStore();
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
            store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?L"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS))
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))).Optional()
                    .AddFilter(new RDFBooleanNotFilter(new RDFBoundFilter(new RDFVariable("?L")))));
            RDFDescribeQueryResult result = query.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 0);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT"));
            Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT"));
            Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE"));
            Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT"));
        }

        [TestMethod]
        public void ShouldApplyDescribeQueryToNullStoreAndNotHaveResults()
        {
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
                .AddDescribeTerm(new RDFVariable("?S"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
                .AddModifier(new RDFDistinctModifier());
            RDFDescribeQueryResult result = query.ApplyToStore(null);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResultsCount == 0);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 0);
        }
        #endregion
    }
}