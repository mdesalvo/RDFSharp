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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the SubSelect half of RDFQueryParser: a nested SELECT query used as the content of a group
/// graph pattern (GroupGraphPattern ::= '{' ( SubSelect | GroupGraphPatternSub ) '}'). Covers top-level and
/// joined subqueries, subqueries carrying their own projection expression and solution modifiers, and round-trips.
/// </summary>
public partial class RDFQueryParserTest
{
    #region SubQuery
    [TestMethod]
    public void ShouldRoundTripSubQuery()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
                .AddProjectionVariable(new RDFVariable("s")));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSubQueryWithModifiers()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
                .AddProjectionVariable(new RDFVariable("s"))
                .AddModifier(new RDFDistinctModifier())
                .AddModifier(new RDFLimitModifier(5)));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSubQueryJoinedWithTriples()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("x"), new RDFVariable("y"))))
                .AddProjectionVariable(new RDFVariable("s")));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldParseTopLevelSubQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SELECT ?s WHERE { ?s ?p ?o } }");

        RDFSelectQuery subQuery = (RDFSelectQuery)query.GetSubQueries().Single();
        Assert.IsTrue(subQuery.IsSubQuery);
        Assert.AreEqual(1, subQuery.ProjectionVars.Count);
        Assert.IsTrue(subQuery.ProjectionVars.Keys.Any(v => v.VariableName == "?S"));
        Assert.AreEqual(1, subQuery.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseSubQueryJoinedWithTriples()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o { SELECT ?s WHERE { ?s a ?t } } }");

        //The body holds a plain pattern group AND a subquery side by side
        Assert.AreEqual(1, query.GetPatternGroups().Count());
        Assert.AreEqual(1, query.GetSubQueries().Count());
    }

    [TestMethod]
    public void ShouldParseSubQueryWithProjectionExpression()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SELECT ?s (?o AS ?x) WHERE { ?s ?p ?o } }");

        RDFSelectQuery subQuery = (RDFSelectQuery)query.GetSubQueries().Single();
        Assert.AreEqual(2, subQuery.ProjectionVars.Count);
        //The computed projection variable carries its expression
        Assert.IsTrue(subQuery.ProjectionVars.Any(pv => pv.Key.VariableName == "?X" && pv.Value.Item2 != null));
    }

    [TestMethod]
    public void ShouldParseSubQueryWithDistinctAndLimit()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { SELECT DISTINCT ?s WHERE { ?s ?p ?o } LIMIT 5 }");

        RDFSelectQuery subQuery = (RDFSelectQuery)query.GetSubQueries().Single();
        Assert.IsTrue(subQuery.GetModifiers().Any(m => m is RDFDistinctModifier));
        Assert.IsTrue(subQuery.GetModifiers().Any(m => m is RDFLimitModifier));
    }

    [TestMethod]
    public void ShouldParseNestedSubQueryInsideUnion()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { { SELECT ?s WHERE { ?s a ?t } } UNION { ?s ?p ?o } }");

        //A UNION whose left operand is a subquery yields a single binary operator member
        Assert.AreEqual(1, query.GetEvaluableQueryMembers().Count());
    }
    #endregion
}
