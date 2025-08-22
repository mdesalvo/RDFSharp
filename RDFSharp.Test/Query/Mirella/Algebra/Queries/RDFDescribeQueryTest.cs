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
using System;
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

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFDescribeQueryTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    #region Tests
    [TestMethod]
    public void ShouldCreateDescribeQuery()
    {
        RDFDescribeQuery query = new RDFDescribeQuery();

        Assert.IsNotNull(query);
        Assert.IsNotNull(query.QueryMembers);
        Assert.IsEmpty(query.QueryMembers);
        Assert.IsNotNull(query.DescribeTerms);
        Assert.IsEmpty(query.DescribeTerms);
        Assert.IsNotNull(query.Variables);
        Assert.IsEmpty(query.Variables);
        Assert.IsNotNull(query.Prefixes);
        Assert.IsEmpty(query.Prefixes);
        Assert.IsTrue(query.IsEvaluable);
        Assert.IsTrue(query.ToString().Equals("DESCRIBE *" + Environment.NewLine + "WHERE {" + Environment.NewLine + "}", StringComparison.Ordinal));
        Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
        Assert.AreEqual(0, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(0, query.GetPatternGroups().Count());
        Assert.AreEqual(0, query.GetSubQueries().Count());
        Assert.IsEmpty(query.GetValues());
        Assert.AreEqual(0, query.GetModifiers().Count());
        Assert.IsEmpty(query.GetPrefixes());
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
            new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                .AddFilter(new RDFExpressionFilter(new RDFIsUriExpression(new RDFVariable("?S")))));
        query.AddSubQuery(
            new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                .AddPatternGroup(
                    new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [RDFVocabulary.RDFS.CLASS])))
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?P")));
        query.AddModifier(new RDFDistinctModifier());
        query.AddModifier(new RDFLimitModifier(100));
        query.AddModifier(new RDFOffsetModifier(20));

        Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>" + Environment.NewLine + "PREFIX owl: <http://www.w3.org/2002/07/owl#>" + Environment.NewLine + Environment.NewLine + "DESCRIBE ?S <ex:flower>" + Environment.NewLine + "WHERE {" + Environment.NewLine + "  {" + Environment.NewLine + "    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ." + Environment.NewLine + "    FILTER ( (ISURI(?S)) ) " + Environment.NewLine + "  }" + Environment.NewLine + "  {" + Environment.NewLine + "    SELECT ?S ?P" + Environment.NewLine + "    WHERE {" + Environment.NewLine + "      {" + Environment.NewLine + "        ?S ?P owl:Class ." + Environment.NewLine + "        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ." + Environment.NewLine + "      }" + Environment.NewLine + "    }" + Environment.NewLine + "  }" + Environment.NewLine + "}" + Environment.NewLine + "LIMIT 100" + Environment.NewLine + "OFFSET 20", StringComparison.Ordinal));
        Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
        Assert.HasCount(2, query.DescribeTerms);
        Assert.HasCount(1, query.Variables);
        Assert.AreEqual(2, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(1, query.GetPatternGroups().Count());
        Assert.AreEqual(1, query.GetSubQueries().Count());
        Assert.HasCount(1, query.GetValues());
        Assert.AreEqual(3, query.GetModifiers().Count());
        Assert.HasCount(2, query.GetPrefixes());
    }

    [TestMethod]
    public void ShouldCreateDescribeQueryWithOptionalQueryMembers()
    {
        RDFDescribeQuery query = new RDFDescribeQuery();
        query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
        query.AddDescribeTerm(new RDFVariable("?S"));
        query.AddDescribeTerm(new RDFResource("ex:flower"));
        query.AddPatternGroup(
            new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                .AddFilter(new RDFExpressionFilter(new RDFIsUriExpression(new RDFVariable("?S")))));
        query.AddSubQuery(
            new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                .AddPatternGroup(
                    new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [RDFVocabulary.RDFS.CLASS])))
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?P"))
                .Optional());
        query.AddModifier(new RDFDistinctModifier());
        query.AddModifier(new RDFLimitModifier(100));
        query.AddModifier(new RDFOffsetModifier(20));

        Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>" + Environment.NewLine + "PREFIX owl: <http://www.w3.org/2002/07/owl#>" + Environment.NewLine + "" + Environment.NewLine + "DESCRIBE ?S <ex:flower>" + Environment.NewLine + "WHERE {" + Environment.NewLine + "  {" + Environment.NewLine + "    ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ." + Environment.NewLine + "    FILTER ( (ISURI(?S)) ) " + Environment.NewLine + "  }" + Environment.NewLine + "  OPTIONAL {" + Environment.NewLine + "    SELECT ?S ?P" + Environment.NewLine + "    WHERE {" + Environment.NewLine + "      {" + Environment.NewLine + "        ?S ?P owl:Class ." + Environment.NewLine + "        VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ." + Environment.NewLine + "      }" + Environment.NewLine + "    }" + Environment.NewLine + "  }" + Environment.NewLine + "}" + Environment.NewLine + "LIMIT 100" + Environment.NewLine + "OFFSET 20", StringComparison.Ordinal));
        Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
        Assert.HasCount(2, query.DescribeTerms);
        Assert.HasCount(1, query.Variables);
        Assert.AreEqual(2, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(1, query.GetPatternGroups().Count());
        Assert.AreEqual(1, query.GetSubQueries().Count());
        Assert.IsTrue(query.GetSubQueries().Single() is RDFSelectQuery { IsOptional: true });
        Assert.HasCount(1, query.GetValues());
        Assert.AreEqual(3, query.GetModifiers().Count());
        Assert.HasCount(2, query.GetPrefixes());
    }

    [TestMethod]
    public void ShouldCreateDescribeQueryWithUnionQueryMembers()
    {
        RDFDescribeQuery query = new RDFDescribeQuery();
        query.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
        query.AddDescribeTerm(new RDFVariable("?S"));
        query.AddDescribeTerm(new RDFResource("ex:flower"));
        query.AddPatternGroup(
            new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS))
                .AddFilter(new RDFExpressionFilter(new RDFIsUriExpression(new RDFVariable("?S"))))
                .UnionWithNext());
        query.AddSubQuery(
            new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("owl"))
                .AddPatternGroup(
                    new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.OWL.CLASS))
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [RDFVocabulary.RDFS.CLASS])))
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?P")));
        query.AddModifier(new RDFDistinctModifier());
        query.AddModifier(new RDFLimitModifier(100));
        query.AddModifier(new RDFOffsetModifier(20));

        Assert.IsTrue(query.ToString().Equals("PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>" + Environment.NewLine + "PREFIX owl: <http://www.w3.org/2002/07/owl#>" + Environment.NewLine + "" + Environment.NewLine + "DESCRIBE ?S <ex:flower>" + Environment.NewLine + "WHERE {" + Environment.NewLine + "  {" + Environment.NewLine + "    {" + Environment.NewLine + "      ?S rdf:type <http://www.w3.org/2000/01/rdf-schema#Class> ." + Environment.NewLine + "      FILTER ( (ISURI(?S)) ) " + Environment.NewLine + "    }" + Environment.NewLine + "    UNION" + Environment.NewLine + "    {" + Environment.NewLine + "      SELECT ?S ?P" + Environment.NewLine + "      WHERE {" + Environment.NewLine + "        {" + Environment.NewLine + "          ?S ?P owl:Class ." + Environment.NewLine + "          VALUES ?S { <http://www.w3.org/2000/01/rdf-schema#Class> } ." + Environment.NewLine + "        }" + Environment.NewLine + "      }" + Environment.NewLine + "    }" + Environment.NewLine + "  }" + Environment.NewLine + "}" + Environment.NewLine + "LIMIT 100" + Environment.NewLine + "OFFSET 20", StringComparison.Ordinal));
        Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
        Assert.HasCount(2, query.DescribeTerms);
        Assert.HasCount(1, query.Variables);
        Assert.AreEqual(2, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(1, query.GetPatternGroups().Count());
        Assert.IsTrue(query.GetPatternGroups().Single().JoinAsUnion);
        Assert.AreEqual(1, query.GetSubQueries().Count());
        Assert.HasCount(1, query.GetValues());
        Assert.AreEqual(3, query.GetModifiers().Count());
        Assert.HasCount(2, query.GetPrefixes());
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithStarTermsFromValues()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white", "en")));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddPatternGroup(new RDFPatternGroup()
                .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [new RDFResource("ex:flower")])))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithStarTermsFromPropertyPath()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white", "en")));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?S"), RDFVocabulary.RDFS.CLASS)
                    .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE))))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithStarTermsFromBind()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white", "en")));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddPatternGroup(new RDFPatternGroup()
                .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:flower")), new RDFVariable("?S"))))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithOneVariableTermFromBind()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white", "en")));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:flower")), new RDFVariable("?S"))));
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToGraphAndHaveResultsWithOneVariableTermFromBindProducingErrors()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white", "en")));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddBind(new RDFBind(new RDFAbsExpression(new RDFConstantExpression(new RDFResource("ex:flower"))), new RDFVariable("?S"))));
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS))
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS))
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))).Optional()
                .AddFilter(new RDFBooleanNotFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?L"))))));
        RDFDescribeQueryResult result = query.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToNullGraphAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToGraph(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx1"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx1"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS))
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx1"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx1"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
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
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:flower"))
            .AddDescribeTerm(new RDFVariable("?Q")) //In absence of a query body, describe variables are discarded
            .AddDescribeTerm(new RDFResource("bnode:12345"));
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("bnode:12345"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
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
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS))
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))).Optional()
                .AddFilter(new RDFBooleanNotFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?L"))))));
        RDFDescribeQueryResult result = query.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToNullStoreAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToStore(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndHaveResultsWithStarTerms()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFMemoryStore store2 = new RDFMemoryStore([
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:shrub"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)]);
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(new RDFResource("ex:grass"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        RDFGraph graph2 = new RDFGraph();
        graph2.AddTripleAsync(new RDFTriple(new RDFResource("ex:plant"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)).GetAwaiter().GetResult();
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        federation.AddGraph(graph);
        federation.AddGraph(graph2);
        federation.AddStore(store2);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(6, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?CONTEXT"].Equals(DBNull.Value));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?SUBJECT"].Equals("ex:grass"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[4]["?CONTEXT"].Equals(DBNull.Value));
        Assert.IsTrue(result.DescribeResults.Rows[4]["?SUBJECT"].Equals("ex:plant"));
        Assert.IsTrue(result.DescribeResults.Rows[4]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[4]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[5]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[5]["?SUBJECT"].Equals("ex:shrub"));
        Assert.IsTrue(result.DescribeResults.Rows[5]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[5]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndHaveResultsWithOneResourceTerm()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        RDFMemoryStore store2 = new RDFMemoryStore([
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.DC.CREATOR, new RDFResource("ex:God"))]);
        RDFGraph graph2 = new RDFGraph();
        graph2.AddTripleAsync(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("flower","en"))).GetAwaiter().GetResult();
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        federation.AddStore(store2);
        federation.AddGraph(graph2);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFResource("ex:flower"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(4, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals("http://purl.org/dc/elements/1.1/creator"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals("ex:God"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?CONTEXT"].Equals(DBNull.Value));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?PREDICATE"].Equals("http://www.w3.org/2000/01/rdf-schema#label"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?OBJECT"].Equals("flower@EN"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndHaveResultsWithOneContextResourceTerm()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFMemoryStore store2 = new RDFMemoryStore([
            new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:flower"), RDFVocabulary.DC.CREATOR, new RDFResource("ex:God"))]);
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        federation.AddStore(store2);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFResource("ex:ctx2"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx2"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:tree"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx2"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("http://purl.org/dc/elements/1.1/creator"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("ex:God"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndHaveResultsWithOneBlankResourceTerm()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("bnode:12345"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFGraph graph = new RDFGraph();
        graph.AddTripleAsync(new RDFTriple(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("this is a blank node"))).GetAwaiter().GetResult();
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        federation.AddGraph(graph);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFResource("bnode:12345"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx2"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("bnode:12345"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals(DBNull.Value));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("bnode:12345"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals($"{RDFVocabulary.RDFS.LABEL}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("this is a blank node"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndHaveResultsWithOneVariableTerm()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        RDFGraph graph = new RDFGraph();
        graph.AddTripleAsync(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("flower","en"))).GetAwaiter().GetResult();
        RDFMemoryStore store2 = new RDFMemoryStore([
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.DC.CREATOR, new RDFResource("ex:God"))]);
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        federation.AddGraph(graph);
        federation.AddStore(store2);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(4, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals(DBNull.Value));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals("http://www.w3.org/2000/01/rdf-schema#label"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals("flower@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?PREDICATE"].Equals("http://purl.org/dc/elements/1.1/creator"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?OBJECT"].Equals("ex:God"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndHaveResultsWithOneContextVariableTerm()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?C"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx1"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx1"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx2"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndHaveResultsWithMoreVariableTerms()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx1"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx2"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddDescribeTerm(new RDFVariable("?P"))
            .AddDescribeTerm(new RDFVariable("?L"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS))
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx1"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx1"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx2"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndHaveResultsWithNoBody()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:flower"))
            .AddDescribeTerm(new RDFVariable("?Q")); //This variable will not be evaluated, since it is not part of results table
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndNotHaveResults()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddDescribeTerm(new RDFVariable("?P"))
            .AddDescribeTerm(new RDFVariable("?Q")) //This variable will not be evaluated, since it is not part of results table
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToFederationAndNotHaveResultsBecauseUnboundVariable()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?L"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS))
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:color"), new RDFVariable("?L"))).Optional()
                .AddFilter(new RDFBooleanNotFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?L"))))));
        RDFDescribeQueryResult result = query.ApplyToFederation(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToNullFederationAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToFederation(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToEmptyFederationAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = query.ApplyToFederation(new RDFFederation().AddFederation(new RDFFederation()));

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToSPARQLEndpointAndHaveResults()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAndHaveResults/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAndHaveResults/sparql"));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(endpoint);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToSPARQLEndpointAndHaveResultsViaPost()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAndHaveResultsViaPost/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAndHaveResultsViaPost/sparql"));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions {
            QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post });

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToSPARQLEndpointAndNotHaveResults()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAndNotHaveResults/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody("", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAndNotHaveResults/sparql"));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(endpoint);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToSPARQLEndpointAndNotHaveResultsViaPost()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAndNotHaveResultsViaPost/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithBody("", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAndNotHaveResultsViaPost/sparql"));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions {
            QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post });

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldApplyRawDescribeQueryToSPARQLEndpoint()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyRawDescribeQueryToSPARQLEndpoint/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyRawDescribeQueryToSPARQLEndpoint/sparql"));
        RDFDescribeQueryResult result = RDFDescribeQuery.ApplyRawToSPARQLEndpoint(query.ToString(), endpoint);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
    }

    [TestMethod]
    public void ShouldApplyRawDescribeQueryToSPARQLEndpointViaPost()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyRawDescribeQueryToSPARQLEndpointViaPost/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyRawDescribeQueryToSPARQLEndpointViaPost/sparql"));
        RDFDescribeQueryResult result = RDFDescribeQuery.ApplyRawToSPARQLEndpoint(query.ToString(), endpoint, new RDFSPARQLEndpointQueryOptions {
            QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post });

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
    }

    [TestMethod]
    public void ShouldApplyDescribeQueryToNullSPARQLEndpointAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehavior()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(400));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql"));

        Assert.ThrowsExactly<RDFQueryException>(() => query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(250, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehaviorViaPost()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehaviorViaPost/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(400));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehaviorViaPost/sparql"));

        Assert.ThrowsExactly<RDFQueryException>(() => query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(250,
            RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post)));
    }

    [TestMethod]
    public void ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehavior()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody("", encoding: Encoding.UTF8)
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(400));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehavior/sparql"));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(250, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult));

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehaviorViaPost()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehaviorViaPost/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithBody("", encoding: Encoding.UTF8)
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(400));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToTimingAndBehaviorViaPost/sparql"));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(250,
            RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult, RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post));

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehavior()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehavior/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody("Internal Server Error")
                    .WithStatusCode(HttpStatusCode.InternalServerError)
                    .WithFault(FaultType.NONE));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehavior/sparql"));

        Assert.ThrowsExactly<RDFQueryException>(() => query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(750, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException)));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehaviorViaPost()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehaviorViaPost/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithBody("Internal Server Error")
                    .WithStatusCode(HttpStatusCode.InternalServerError)
                    .WithFault(FaultType.NONE));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldThrowExceptionWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehaviorViaPost/sparql"));

        Assert.ThrowsExactly<RDFQueryException>(() => query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(750,
            RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post)));
    }

    [TestMethod]
    public void ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehavior()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehavior/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody("Internal Server Error")
                    .WithStatusCode(HttpStatusCode.InternalServerError)
                    .WithFault(FaultType.NONE));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehavior/sparql"));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(750, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult));

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public void ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehaviorViaPost()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehaviorViaPost/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithBody("Internal Server Error")
                    .WithStatusCode(HttpStatusCode.InternalServerError)
                    .WithFault(FaultType.NONE));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldGiveEmptyResultWhenApplyingDescribeQueryToSPARQLEndpointAccordingToBehaviorViaPost/sparql"));
        RDFDescribeQueryResult result = query.ApplyToSPARQLEndpoint(endpoint, new RDFSPARQLEndpointQueryOptions(750,
            RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult, RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post));

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    //ASYNC

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToGraphAsyncAndHaveResults()
    {
        RDFGraph graph = new RDFGraph();
        await graph.AddTripleAsync(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        await graph.AddTripleAsync(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        await graph.AddTripleAsync(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = await query.ApplyToGraphAsync(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToGraphAsyncAndNotHaveResults()
    {
        RDFGraph graph = new RDFGraph();
        await graph.AddTripleAsync(new RDFTriple(new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        await graph.AddTripleAsync(new RDFTriple(new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        await graph.AddTripleAsync(new RDFTriple(new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddDescribeTerm(new RDFVariable("?P"))
            .AddDescribeTerm(new RDFVariable("?Q")) //This variable will not be evaluated, since it is not part of results table
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = await query.ApplyToGraphAsync(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToNullGraphAsyncAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = await query.ApplyToGraphAsync(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToStoreAsyncAndHaveResults()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = await query.ApplyToStoreAsync(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToStoreAsyncAndNotHaveResults()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        await store.AddQuadrupleAsync(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddDescribeTerm(new RDFVariable("?P"))
            .AddDescribeTerm(new RDFVariable("?Q")) //This variable will not be evaluated, since it is not part of results table
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = await query.ApplyToStoreAsync(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToNullStoreAsyncAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = await query.ApplyToStoreAsync(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToFederationAsyncAndHaveResults()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFGraph graph = new RDFGraph();
        await graph.AddTripleAsync(new RDFTriple(new RDFResource("ex:grass"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS));
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        federation.AddGraph(graph);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = await query.ApplyToFederationAsync(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(4, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?PREDICATE"].Equals("ex:color"));
        Assert.IsTrue(result.DescribeResults.Rows[1]["?OBJECT"].Equals("white@EN"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?CONTEXT"].Equals("ex:ctx"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?SUBJECT"].Equals("ex:tree"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[2]["?OBJECT"].Equals($"{RDFVocabulary.OWL.CLASS}"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?CONTEXT"].Equals(DBNull.Value));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?SUBJECT"].Equals("ex:grass"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[3]["?OBJECT"].Equals($"{RDFVocabulary.RDFS.CLASS}"));
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToFederationAsyncAndNotHaveResults()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:flower"), new RDFResource("ex:color"), new RDFPlainLiteral("white","en")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:tree"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS));
        RDFFederation federation = new RDFFederation();
        federation.AddStore(store);
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddDescribeTerm(new RDFVariable("?P"))
            .AddDescribeTerm(new RDFVariable("?Q")) //This variable will not be evaluated, since it is not part of results table
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = await query.ApplyToFederationAsync(federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Columns[0].ColumnName.Equals("?CONTEXT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[1].ColumnName.Equals("?SUBJECT", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[2].ColumnName.Equals("?PREDICATE", StringComparison.Ordinal));
        Assert.IsTrue(result.DescribeResults.Columns[3].ColumnName.Equals("?OBJECT", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToNullFederationAsyncAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), RDFVocabulary.RDFS.CLASS)))
            .AddModifier(new RDFDistinctModifier());
        RDFDescribeQueryResult result = await query.ApplyToFederationAsync(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToSPARQLEndpointAsyncAndHaveResults()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAsyncAndHaveResults/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAsyncAndHaveResults/sparql"));
        RDFDescribeQueryResult result = await query.ApplyToSPARQLEndpointAsync(endpoint);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToSPARQLEndpointAsyncAndNotHaveResults()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAsyncAndNotHaveResults/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody("", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyDescribeQueryToSPARQLEndpointAsyncAndNotHaveResults/sparql"));
        RDFDescribeQueryResult result = await query.ApplyToSPARQLEndpointAsync(endpoint);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
    }

    [TestMethod]
    public async Task ShouldApplyRawDescribeQueryToSPARQLEndpointAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDescribeQueryTest/ShouldApplyRawDescribeQueryToSPARQLEndpointAsync/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.@base <{RDFNamespaceRegister.DefaultNamespace}>.<ex:flower> a <ex:plant>.", encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/turtle")
                    .WithStatusCode(HttpStatusCode.OK));

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDescribeQueryTest/ShouldApplyRawDescribeQueryToSPARQLEndpointAsync/sparql"));
        RDFDescribeQueryResult result = await RDFDescribeQuery.ApplyRawToSPARQLEndpointAsync(query.ToString(), endpoint);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.IsTrue(result.DescribeResults.Rows[0]["?SUBJECT"].Equals("ex:flower"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?PREDICATE"].Equals($"{RDFVocabulary.RDF.TYPE}"));
        Assert.IsTrue(result.DescribeResults.Rows[0]["?OBJECT"].Equals("ex:plant"));
    }

    [TestMethod]
    public async Task ShouldApplyDescribeQueryToNullSPARQLEndpointAsyncAndNotHaveResults()
    {
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX))
            .AddDescribeTerm(new RDFVariable("?S"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        RDFDescribeQueryResult result = await query.ApplyToSPARQLEndpointAsync(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(0, result.DescribeResultsCount);
        Assert.AreEqual(0, result.DescribeResults.Columns.Count);
    }
    #endregion
}