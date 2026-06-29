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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the BIND and VALUES (inline data) half of RDFQueryParser: both are pattern-group members
/// absorbed into the surrounding group. Covers BIND assignments, the compact one-variable VALUES form, the
/// extended multi-variable form with the UNDEF placeholder, and the malformed/degenerate error contracts.
/// </summary>
public partial class RDFQueryParserTest
{
    #region Bind
    [TestMethod]
    public void ShouldRoundTripBind()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
                .AddBind(new RDFBind(
                    new RDFAddExpression(new RDFVariable("o"), new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
                    new RDFVariable("n"))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldParseBind()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o BIND(?o AS ?x) }");

        RDFPatternGroup patternGroup = query.GetPatternGroups().Single();
        Assert.AreEqual(1, patternGroup.GetPatterns().Count());
        RDFBind bind = patternGroup.GetBinds().Single();
        Assert.AreEqual("?X", bind.Variable.VariableName);
        Assert.IsNotNull(bind.Expression);
    }

    [TestMethod]
    public void ShouldParseBindInterleavedWithFilterAndTriples()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(?o > 1) BIND(?o AS ?x) ?a ?b ?c }");

        //Triples, filter and bind all collapse into ONE pattern group (inline members are absorbed)
        RDFPatternGroup patternGroup = query.GetPatternGroups().Single();
        Assert.AreEqual(2, patternGroup.GetPatterns().Count());
        Assert.AreEqual(1, patternGroup.GetFilters().Count());
        Assert.AreEqual(1, patternGroup.GetBinds().Count());
    }

    [TestMethod]
    public void ShouldParseBindOpeningTheGroupBody()
    {
        //A BIND may open the group body (before any triple): it must still land in a pattern group
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { BIND(1 AS ?x) }");

        Assert.AreEqual(1, query.GetPatternGroups().Single().GetBinds().Count());
    }

    [TestMethod]
    public void ShouldThrowOnBindWithoutAs()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o BIND(?o ?x) }"));

    [TestMethod]
    public void ShouldThrowOnBindWithoutResultVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o BIND(?o AS x) }"));
    #endregion

    #region Values
    [TestMethod]
    public void ShouldRoundTripValuesSingleVariable()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
                .AddValues(new RDFValues()
                    .AddColumn(new RDFVariable("o"), [new RDFPlainLiteral("a"), new RDFPlainLiteral("b")])));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripValuesMultipleVariablesWithUndef()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
                .AddValues(new RDFValues()
                    .AddColumn(new RDFVariable("x"), [RDFVocabulary.RDF.TYPE, null])
                    .AddColumn(new RDFVariable("y"), [RDFVocabulary.FOAF.KNOWS, RDFVocabulary.FOAF.NAME])));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldParseValuesSingleVariable()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES ?o { 1 2 } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.AreEqual(1, values.Bindings.Count);
        Assert.AreEqual(2, values.Bindings["?O"].Count);
    }

    [TestMethod]
    public void ShouldParseValuesSingleVariableWithUndef()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES ?o { 1 UNDEF } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.AreEqual(2, values.Bindings["?O"].Count);
        Assert.IsNotNull(values.Bindings["?O"][0]);
        Assert.IsNull(values.Bindings["?O"][1]);
    }

    [TestMethod]
    public void ShouldParseValuesSingleVariableWithUndefBetweenBoundValues()
    {
        //UNDEF sandwiched between two bound values must keep its ordinal position (5 / UNBOUND / 25)
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES ?o { 5 UNDEF 25 } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.AreEqual(3, values.Bindings["?O"].Count);
        Assert.IsNotNull(values.Bindings["?O"][0]);
        Assert.IsNull(values.Bindings["?O"][1]);
        Assert.IsNotNull(values.Bindings["?O"][2]);
    }

    [TestMethod]
    public void ShouldParseValuesMultipleVariablesWithUndef()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES (?x ?y) { (1 2) (UNDEF 3) } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.AreEqual(2, values.Bindings.Count);
        Assert.AreEqual(2, values.Bindings["?X"].Count);
        Assert.AreEqual(2, values.Bindings["?Y"].Count);
        //The UNDEF in the second row of the first column maps to a null (unbound) binding
        Assert.IsNull(values.Bindings["?X"][1]);
        Assert.IsNotNull(values.Bindings["?Y"][1]);
    }

    [TestMethod]
    public void ShouldParseValuesUsingPrefixedNames()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("PREFIX foaf: <http://xmlns.com/foaf/0.1/> SELECT * WHERE { ?s ?p ?o VALUES ?o { foaf:Person foaf:Agent } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.AreEqual("http://xmlns.com/foaf/0.1/Person", values.Bindings["?O"][0].ToString());
        Assert.AreEqual("http://xmlns.com/foaf/0.1/Agent", values.Bindings["?O"][1].ToString());
    }

    [TestMethod]
    public void ShouldParseEmptyValuesDataBlockOneVar()
    {
        //'VALUES ?o { }' is a legal SPARQL block (zero solutions): a declared-but-empty column
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES ?o { } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.AreEqual(1, values.Bindings.Count);
        Assert.IsEmpty(values.Bindings["?O"]);
        Assert.AreEqual(0, values.MaxBindingsLength());
        Assert.IsTrue(values.IsEvaluable);
    }

    [TestMethod]
    public void ShouldParseEmptyValuesDataBlockMultiVar()
    {
        //'VALUES (?x) { }' multi-var extended form with no rows: a declared-but-empty column
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES (?x) { } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.AreEqual(1, values.Bindings.Count);
        Assert.IsEmpty(values.Bindings["?X"]);
        Assert.AreEqual(0, values.MaxBindingsLength());
        Assert.IsTrue(values.IsEvaluable);
    }

    [TestMethod]
    public void ShouldParseNilValuesDataBlock()
    {
        //'VALUES () { () () }' NIL block: zero columns, two empty-domain identity rows (multiset doubling)
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES () { () () } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.IsEmpty(values.Bindings);
        Assert.AreEqual(2, values.NilRowsCount);
        Assert.AreEqual(2, values.MaxBindingsLength());
        Assert.IsTrue(values.IsEvaluable);
    }

    [TestMethod]
    public void ShouldParseEmptyNilValuesDataBlock()
    {
        //'VALUES () { }' NIL block with no rows: zero solutions (annihilates the join), still evaluable
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES () { } }");

        RDFValues values = query.GetPatternGroups().Single().GetValues().Single();
        Assert.IsEmpty(values.Bindings);
        Assert.AreEqual(0, values.NilRowsCount);
        Assert.IsTrue(values.IsEvaluable);
    }

    [TestMethod]
    public void ShouldParseTrailingValuesClause()
    {
        //'SELECT ... WHERE { ... } VALUES ...' query-level inline data (SELECT-only trailing ValuesClause)
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } VALUES ?o { 1 2 }");

        Assert.IsNotNull(query.QueryValues);
        Assert.AreEqual(2, query.QueryValues.Bindings["?O"].Count);
        //The trailing block must NOT leak into the pattern group
        Assert.IsEmpty(query.GetPatternGroups().Single().GetValues());
    }

    [TestMethod]
    public void ShouldParseTrailingValuesClauseInSubQuery()
    {
        //A subselect reuses ParseSelectQuery, so it gets the trailing ValuesClause too
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } VALUES ?o { 1 } } }");

        RDFSelectQuery subQuery = query.GetSubQueries().OfType<RDFSelectQuery>().Single();
        Assert.IsNotNull(subQuery.QueryValues);
        Assert.AreEqual(1, subQuery.QueryValues.Bindings["?O"].Count);
    }

    [TestMethod]
    public void ShouldThrowOnValuesRowArityMismatch()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES (?x ?y) { (1) } }"));

    [TestMethod]
    public void ShouldThrowOnNilValuesRowArityMismatch()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o VALUES () { (1) } }"));

    [TestMethod]
    public void ShouldRoundTripAndEvaluateEmptyValuesBlockIso()
    {
        RDFGraph sampleGraph = new RDFGraph();
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s1"), new RDFResource("ex:p"), new RDFPlainLiteral("a")));

        //'VALUES ?o { }' is a "zero solutions" block: the whole query must yield no row
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFResource("ex:p"), new RDFVariable("o")))
                .AddValues(new RDFValues().AddColumn(new RDFVariable("o"), [])));

        RDFTestUtilities.AssertIso(query, sampleGraph);
        Assert.AreEqual(0, query.ApplyToGraph(sampleGraph).SelectResults.Rows.Count);
    }

    [TestMethod]
    public void ShouldRoundTripAndEvaluateTrailingValuesIso()
    {
        RDFGraph sampleGraph = new RDFGraph();
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s1"), new RDFResource("ex:p"), new RDFPlainLiteral("a")));
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s2"), new RDFResource("ex:p"), new RDFPlainLiteral("b")));

        //Trailing query-level VALUES restricts ?o to "a": the join with the whole WHERE keeps only that row
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFResource("ex:p"), new RDFVariable("o"))))
            .SetValues(new RDFValues().AddColumn(new RDFVariable("o"), [new RDFPlainLiteral("a")]));

        RDFTestUtilities.AssertIso(query, sampleGraph);
        Assert.AreEqual(1, query.ApplyToGraph(sampleGraph).SelectResults.Rows.Count);
    }

    [TestMethod]
    public void ShouldEvaluateNilValuesBlocks()
    {
        RDFGraph sampleGraph = new RDFGraph();
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s1"), new RDFResource("ex:p"), new RDFPlainLiteral("a")));
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s2"), new RDFResource("ex:p"), new RDFPlainLiteral("b")));

        //'VALUES () { () }' is the identity (one empty-domain row): result unchanged (2 rows)
        Assert.AreEqual(2, RDFSelectQuery.FromString("SELECT * WHERE { ?s <ex:p> ?o VALUES () { () } }")
            .ApplyToGraph(sampleGraph).SelectResults.Rows.Count);

        //'VALUES () { }' has no row: it annihilates the join (zero solutions)
        Assert.AreEqual(0, RDFSelectQuery.FromString("SELECT * WHERE { ?s <ex:p> ?o VALUES () { } }")
            .ApplyToGraph(sampleGraph).SelectResults.Rows.Count);

        //'VALUES () { () () }' has two empty-domain rows: it doubles every solution (2 -> 4)
        Assert.AreEqual(4, RDFSelectQuery.FromString("SELECT * WHERE { ?s <ex:p> ?o VALUES () { () () } }")
            .ApplyToGraph(sampleGraph).SelectResults.Rows.Count);
    }

    [TestMethod]
    public void ShouldPropagateSubQueryTrailingValuesToOuter()
    {
        RDFGraph sampleGraph = new RDFGraph();
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s1"), new RDFResource("ex:p"), new RDFPlainLiteral("a")));
        sampleGraph.AddTriple(new RDFTriple(new RDFResource("ex:s2"), new RDFResource("ex:p"), new RDFPlainLiteral("b")));

        //A subselect's trailing VALUES is scoped to the subselect, but its EFFECT (the restricted rows) reaches
        //the outer query as the subquery's result: the outer result must contain only the "a" row.
        RDFSelectQueryResult result = RDFSelectQuery
            .FromString("SELECT * WHERE { { SELECT * WHERE { ?s <ex:p> ?o } VALUES ?o { \"a\" } } }")
            .ApplyToGraph(sampleGraph);

        Assert.AreEqual(1, result.SelectResults.Rows.Count);
        Assert.AreEqual("a", result.SelectResults.Rows[0]["?o"].ToString());
    }
    #endregion
}
