/*
   Copyright 2012-2026 Marco De Salvo

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
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

/// <summary>
/// Tests for the tree-based evaluation of RDFOperatorQueryMember and
/// RDFOperatorPatternGroupMember in RDFQueryEngine (Phase 2).
/// </summary>
[TestClass]
public class RDFQueryEngineOperatorTreeTest
{
    #region Shared test data

    /// <summary>
    /// Builds a graph with dogs and owners, used across most tests:
    ///   pluto  -> dogOf -> topolino   (topolino  hasName "Mickey Mouse")
    ///   fido   -> dogOf -> paperino   (paperino  hasName "Donald Duck")
    ///   balto  -> dogOf -> whoever
    /// </summary>
    private static RDFGraph BuildDogGraph()
    {
        return new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),    new RDFResource("ex:dogOf"),  new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"), new RDFResource("ex:hasName"), new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),     new RDFResource("ex:dogOf"),  new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"), new RDFResource("ex:hasName"), new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),    new RDFResource("ex:dogOf"),  new RDFResource("ex:whoever"))
        ]);
    }

    #endregion

    #region Query-level operator tree (between pattern groups)

    [TestMethod]
    public void ShouldEvaluateUnionOfTwoPatternGroups()
    {
        // pgA matches pluto->topolino, pgB matches fido->paperino
        // Union should return both rows
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(2, result.SelectResultsCount);

        // Verify both values are present (order may vary depending on graph iteration)
        string[] xValues = Enumerable.Range(0, (int)result.SelectResultsCount)
            .Select(i => result.SelectResults.Rows[i]["?X"].ToString())
            .OrderBy(v => v, StringComparer.Ordinal)
            .ToArray();
        Assert.AreEqual("ex:paperino", xValues[0]);
        Assert.AreEqual("ex:topolino", xValues[1]);
    }

    [TestMethod]
    public void ShouldEvaluateMinusOfTwoPatternGroups()
    {
        // pgA matches all dogs: pluto/topolino, fido/paperino, balto/whoever
        // pgB matches balto specifically
        // Minus should exclude the row where ?Y = balto
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:balto")), new RDFVariable("?Y")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Minus(pgB))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual("ex:paperino", result.SelectResults.Rows[0]["?X"].ToString());
        Assert.AreEqual("ex:topolino", result.SelectResults.Rows[1]["?X"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluateNestedUnionThenMinusDifferentlyFromMinusThenUnion()
    {
        // CRITICAL TEST: verifies that tree nesting matters
        //
        // Graph: A={1,2,3}, B={2,3}, C={3}
        //   (A UNION B) MINUS C  →  {1,2,3,2,3} MINUS {3}  →  {1,2,2}  →  3 rows
        //   A UNION (B MINUS C)  →  A UNION {2}             →  {1,2,3,2} →  4 rows
        //
        // We use a graph with numeric-looking resources to keep things clear
        RDFGraph graph = new RDFGraph(
        [
            // Group A: items 1, 2, 3
            new RDFTriple(new RDFResource("ex:1"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")),
            new RDFTriple(new RDFResource("ex:2"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")),
            new RDFTriple(new RDFResource("ex:3"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")),
            // Group B: items 2, 3
            new RDFTriple(new RDFResource("ex:2"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")),
            new RDFTriple(new RDFResource("ex:3"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")),
            // Group C: item 3
            new RDFTriple(new RDFResource("ex:3"), new RDFResource("ex:inGroupC"), new RDFResource("ex:yes"))
        ]);

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")));
        RDFPatternGroup pgC = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupC"), new RDFResource("ex:yes")));

        // Query 1: (A UNION B) MINUS C
        RDFPatternGroup pgA1 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")));
        RDFPatternGroup pgB1 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")));
        RDFPatternGroup pgC1 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupC"), new RDFResource("ex:yes")));

        RDFSelectQuery queryUnionThenMinus = new RDFSelectQuery()
            .AddOperator(pgA1.Union(pgB1).Minus(pgC1));
        RDFSelectQueryResult resultUnionThenMinus = new RDFQueryEngine().EvaluateSelectQuery(queryUnionThenMinus, graph);

        // Query 2: A UNION (B MINUS C)
        RDFPatternGroup pgA2 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")));
        RDFPatternGroup pgB2 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")));
        RDFPatternGroup pgC2 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupC"), new RDFResource("ex:yes")));

        RDFSelectQuery queryUnionOfMinusRight = new RDFSelectQuery()
            .AddOperator(pgA2.Union(pgB2.Minus(pgC2)));
        RDFSelectQueryResult resultUnionOfMinusRight = new RDFQueryEngine().EvaluateSelectQuery(queryUnionOfMinusRight, graph);

        // The two queries MUST produce different row counts, proving tree evaluation matters
        // (A UNION B) MINUS C: union of {1,2,3} and {2,3} = {1,2,3,2,3}, minus {3} = {1,2,2} → 3 rows
        Assert.AreEqual(3, resultUnionThenMinus.SelectResultsCount,
            "(A UNION B) MINUS C should yield 3 rows");

        // A UNION (B MINUS C): B MINUS C = {2}, then union with A = {1,2,3,2} → 4 rows
        Assert.AreEqual(4, resultUnionOfMinusRight.SelectResultsCount,
            "A UNION (B MINUS C) should yield 4 rows");

        // Explicitly verify they are different
        Assert.AreNotEqual(resultUnionThenMinus.SelectResultsCount,
                          resultUnionOfMinusRight.SelectResultsCount,
                          "Tree nesting must produce different results: (A UNION B) MINUS C != A UNION (B MINUS C)");
    }

    [TestMethod]
    public void ShouldEvaluateUnionWithSubquery()
    {
        // pgA matches pluto->topolino via a direct pattern;
        // subquery selects fido->paperino.
        // Union of both should yield 2 rows.
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery subQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(subQuery));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateOptionalOperatorTree()
    {
        // When an operator tree is marked Optional, the outer join should preserve
        // left rows that have no match in the operator tree's result (UNBOUND right columns).
        //
        // pgMain: all dogs (?Y dogOf ?X) → 3 rows: pluto/topolino, fido/paperino, balto/whoever
        // Operator tree (Optional): union of two patterns that find names via ?X (shared variable)
        //   pgA: (?X hasName ?N) for topolino only; pgB: (?X hasName ?N) for paperino only
        //   → union yields topolino/Mickey and paperino/Donald with ?X and ?N
        //   → balto/whoever has no match on ?X in operator tree, but kept because Optional
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgMain = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        // Both leaf pattern groups share ?X with pgMain, ensuring a meaningful join
        RDFPatternGroup pgNamesA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")))
            .AddFilter(new RDFExpressionFilter(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFVariableExpression(new RDFVariable("?X")),
                    new RDFConstantExpression(new RDFResource("ex:topolino")))));
        RDFPatternGroup pgNamesB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")))
            .AddFilter(new RDFExpressionFilter(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFVariableExpression(new RDFVariable("?X")),
                    new RDFConstantExpression(new RDFResource("ex:paperino")))));

        // Union of two name lookups filtered by ?X, marked as Optional
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(pgMain)
            .AddOperator(pgNamesA.Union(pgNamesB).Optional())
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // All 3 dogs should appear: 2 with names, 1 with UNBOUND ?N (because Optional)
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.SelectResultsCount);
        // paperino has a name
        Assert.AreEqual("Donald Duck@EN-US", result.SelectResults.Rows[0]["?N"].ToString());
        // topolino has a name
        Assert.AreEqual("Mickey Mouse@EN-US", result.SelectResults.Rows[1]["?N"].ToString());
        // whoever has no name (UNBOUND → empty string in DataTable)
        Assert.AreEqual(string.Empty, result.SelectResults.Rows[2]["?N"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluateEmptyPatternGroupInOperatorTree()
    {
        // pgA has patterns, pgB is empty (no evaluable members)
        // Union with empty should just return pgA's results
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgEmpty = new RDFPatternGroup();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgEmpty));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // pgA yields 1 row (ex:topolino), pgEmpty yields 0 rows; union = 1 row
        Assert.AreEqual(1, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateMinusWithNoCommonVariables()
    {
        // When left and right have no common variables, MINUS keeps all left rows (SPARQL spec)
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Z"), new RDFResource("ex:hasName"), new RDFVariable("?N")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Minus(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // No common variables → all 3 left rows survive the MINUS
        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateDeepNestedOperatorTree()
    {
        // ((A UNION B) UNION C) — three levels of nesting
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgC = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgB).Union(pgC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // All three dogs should appear
        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateOperatorTreeWithFilter()
    {
        // Pattern group with a filter inside the operator tree leaf
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddFilter(new RDFExpressionFilter(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
                    new RDFVariableExpression(new RDFVariable("?Y")),
                    new RDFConstantExpression(new RDFResource("ex:balto")))));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // pgA with filter: pluto/topolino and fido/paperino (2 rows), pgB: balto/whoever (1 row) → union = 3 rows
        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateOperatorTreeAlongsidePlainPatternGroups()
    {
        // Mix of plain pattern group (joined normally) and operator tree
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgNames = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")));

        RDFPatternGroup pgPluto = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgFido = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        // The query has both a plain pattern group and an operator tree
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(pgNames)
            .AddOperator(pgPluto.Union(pgFido));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // pgNames returns 2 rows (topolino/Mickey, paperino/Donald)
        // Operator tree returns 2 rows (topolino, paperino)
        // They are joined on ?X → 2 rows with names
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateAskQueryWithOperatorTree()
    {
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFAskQuery askQuery = new RDFAskQuery()
            .AddOperator(pgA.Union(pgB));

        RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(askQuery, graph);

        Assert.IsTrue(result.AskResult);
    }

    [TestMethod]
    public void ShouldEvaluateConstructQueryWithOperatorTree()
    {
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFConstructQuery constructQuery = new RDFConstructQuery()
            .AddOperator(pgA.Union(pgB))
            .AddTemplate(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:isDog"), new RDFResource("ex:true")));

        RDFConstructQueryResult result = new RDFQueryEngine().EvaluateConstructQuery(constructQuery, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ConstructResults.Rows.Count);
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryWithOperatorTree()
    {
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFDescribeQuery describeQuery = new RDFDescribeQuery()
            .AddOperator(pgA.Union(pgB))
            .AddDescribeTerm(new RDFVariable("?X"));

        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(describeQuery, graph);

        Assert.IsNotNull(result);
        // topolino and paperino are described; each has a hasName triple → at least 2 rows
        Assert.IsTrue(result.DescribeResults.Rows.Count >= 2);
    }

    #endregion

    #region Pattern-group-level operator tree (between patterns within a pattern group)

    [TestMethod]
    public void ShouldEvaluateUnionOfTwoPatternsInPatternGroup()
    {
        // Two patterns inside a pattern group combined via Union operator tree
        RDFGraph graph = BuildDogGraph();

        RDFPattern patternPluto = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternFido = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddOperator(patternPluto.Union(patternFido)));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateMinusOfTwoPatternsInPatternGroup()
    {
        // Pattern A matches all dogs, Pattern B matches only fido->paperino
        // A MINUS B should remove the fido/paperino row
        RDFGraph graph = BuildDogGraph();

        RDFPattern patternAll = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternFido = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddOperator(patternAll.Minus(patternFido)))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // Only pluto/topolino and balto/whoever should remain
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual("ex:topolino", result.SelectResults.Rows[0]["?X"].ToString());
        Assert.AreEqual("ex:whoever", result.SelectResults.Rows[1]["?X"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluateNestedPatternOperatorTree()
    {
        // A UNION (B MINUS C) at pattern level
        RDFGraph graph = BuildDogGraph();

        RDFPattern patternPluto = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternAll = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternBalto = new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));

        // patternPluto UNION (patternAll MINUS patternBalto)
        // patternAll MINUS patternBalto = {pluto/topolino, fido/paperino} (2 rows)
        // Union with patternPluto = {topolino} + {topolino, paperino} = 3 rows
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddOperator(patternPluto.Union(patternAll.Minus(patternBalto))));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluatePatternOperatorTreeWithPropertyPath()
    {
        // Union between a pattern and a property path
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:a"), new RDFResource("ex:p1"), new RDFResource("ex:b")),
            new RDFTriple(new RDFResource("ex:b"), new RDFResource("ex:p2"), new RDFResource("ex:c"))
        ]);

        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:p1"), new RDFVariable("?O"));
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?S"), new RDFVariable("?O"))
            .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:p1")))
            .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:p2")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddOperator(pattern.Union(propertyPath)));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // Pattern yields: (ex:a, ex:b), PropertyPath yields: (ex:a, ex:c) → Union = 2 rows
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluatePatternOperatorTreeMixedWithRegularPatterns()
    {
        // A pattern group that has both a regular pattern and an operator tree
        RDFGraph graph = BuildDogGraph();

        RDFPattern patternPluto = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternFido = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddOperator(patternPluto.Union(patternFido))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // Operator tree yields {topolino, paperino}; regular pattern yields names.
        // CombineTables joins them on ?X → 2 rows with names
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    #endregion

    #region Edge cases

    [TestMethod]
    public void ShouldHandleMinusRemovingAllRows()
    {
        // When MINUS removes everything, the result should be empty
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Minus(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(0, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldHandleUnionWithIdenticalPatternGroups()
    {
        // A UNION A should yield double the rows (bag semantics, not set)
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgACopy = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgACopy));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // Each side yields 1 row; Union appends both → 2 rows
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateOperatorTreeOnStore()
    {
        // Verify that operator trees work on RDFMemoryStore (not just RDFGraph)
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFResource("ex:paperino"))
        ]);

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateChainedMinusThenUnion()
    {
        // (A MINUS B) UNION C — chaining in the opposite order
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgC = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        // (A MINUS B): {pluto/topolino, fido/paperino} (balto removed), UNION C: {balto/whoever}
        // Total: 3 rows
        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Minus(pgB).Union(pgC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(3, result.SelectResultsCount);
    }

    #endregion
}
