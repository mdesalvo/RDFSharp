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

using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the graph-pattern half of RDFQueryParser: the OPTIONAL/UNION/MINUS algebra and the GRAPH clause (structure, round-trips and end-to-end execution).
/// </summary>
public partial class RDFQueryParserTest
{
    #region GraphPatternAlgebra (F2a round-trips)
    //Helper: PatternGroup with a single triple ?varS <predUri> ?varO
    private static RDFPatternGroup MakePG(string varS, string predUri, string varO)
        => new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable(varS), new RDFResource(predUri), new RDFVariable(varO)));

    [TestMethod]
    public void ShouldRoundTripUnion()
    {
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgB));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripUnionChain()
    {
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFPatternGroup pgC = MakePG("s", "http://example.org/p3", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgB).Union(pgC));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripMinus()
    {
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Minus(pgB));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripOptional()
    {
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "name");
        pgB.Optional();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(pgA)
            .AddPatternGroup(pgB);
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripUnionOfMinus()
    {
        //A ∪ (B ∖ C)
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFPatternGroup pgC = MakePG("s", "http://example.org/p3", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgB.Minus(pgC)));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripMinusOfUnion()
    {
        //(A ∪ B) ∖ C
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFPatternGroup pgC = MakePG("s", "http://example.org/p3", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgB).Minus(pgC));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripOptionalOperator()
    {
        //OPTIONAL (A ∪ B)
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFBinaryQueryMember op = pgA.Union(pgB);
        op.Optional();
        RDFPatternGroup pgC = MakePG("s", "http://example.org/p3", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(pgC)
            .AddBinaryQueryMember(op);
        AssertSelectQueryRoundTrips(query);
    }
    #endregion

    #region GraphPatternAlgebra (F2a spec syntax)
    [TestMethod]
    public void ShouldParseUnionFromSpecSyntax()
    {
        //SPARQL spec: GroupOrUnionGraphPattern ::= GroupGraphPattern ('UNION' GroupGraphPattern)*
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } UNION { ?s <http://example.org/p2> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFBinaryQueryMember op = (RDFBinaryQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, op.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.LeftOperand);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldParseUnionChainFromSpecSyntax()
    {
        //Three-way UNION: left-associative Union(Union(A,B),C)
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } UNION { ?s <http://example.org/p2> ?o } UNION { ?s <http://example.org/p3> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFBinaryQueryMember op = (RDFBinaryQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, op.OperatorType);
        //Left operand is itself a Union(A,B)
        Assert.IsInstanceOfType<RDFBinaryQueryMember>(op.LeftOperand);
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union,
            ((RDFBinaryQueryMember)op.LeftOperand).OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldParseMinusFromSpecSyntax()
    {
        //SPARQL spec: MinusGraphPattern ::= 'MINUS' GroupGraphPattern
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } MINUS { ?s <http://example.org/p2> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFBinaryQueryMember op = (RDFBinaryQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Minus, op.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.LeftOperand);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldParseOptionalFromSpecSyntax()
    {
        //SPARQL spec: OptionalGraphPattern ::= 'OPTIONAL' GroupGraphPattern
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } OPTIONAL { ?s <http://example.org/name> ?name } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(2, evaluable.Count);
        RDFPatternGroup main = (RDFPatternGroup)evaluable[0];
        RDFPatternGroup optional = (RDFPatternGroup)evaluable[1];
        Assert.IsFalse(main.IsOptional);
        Assert.IsTrue(optional.IsOptional);
    }

    [TestMethod]
    public void ShouldParseInlineTriplesWithOptional()
    {
        //Inline BGP followed by OPTIONAL — TriplesBlock must stop at the keyword
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s <http://example.org/p1> ?o OPTIONAL { ?s <http://example.org/name> ?name } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(2, evaluable.Count);
        Assert.IsInstanceOfType<RDFPatternGroup>(evaluable[0]);
        RDFPatternGroup optional = (RDFPatternGroup)evaluable[1];
        Assert.IsTrue(optional.IsOptional);
    }

    [TestMethod]
    public void ShouldParseInlineTriplesWithMinus()
    {
        //Inline BGP followed by MINUS — the BGP becomes the left operand of the tree node
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s <http://example.org/p1> ?o . MINUS { ?s <http://example.org/p2> ?o2 } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFBinaryQueryMember op = (RDFBinaryQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Minus, op.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.LeftOperand);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldParseMinusWithUnionRightOperand()
    {
        //A ∖ (B ∪ C) — right operand of MINUS is a union group per the spec
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } MINUS { { ?s <http://example.org/p2> ?o } UNION { ?s <http://example.org/p3> ?o } } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFBinaryQueryMember minus = (RDFBinaryQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Minus, minus.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(minus.LeftOperand);
        //Right operand is Union(B,C)
        RDFBinaryQueryMember rightUnion = (RDFBinaryQueryMember)minus.RightOperand;
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, rightUnion.OperatorType);
    }

    [TestMethod]
    public void ShouldParseUnionOfMinus()
    {
        //A ∪ (B ∖ C) — SPARQL-compliant: Union left-operand A, right-operand is a Minus group
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } UNION { { ?s <http://example.org/p2> ?o } MINUS { ?s <http://example.org/p3> ?o } } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFBinaryQueryMember union = (RDFBinaryQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, union.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(union.LeftOperand);
        //Right operand is Minus(B,C)
        RDFBinaryQueryMember rightMinus = (RDFBinaryQueryMember)union.RightOperand;
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Minus, rightMinus.OperatorType);
    }

    [TestMethod]
    public void ShouldParseMultiGroupMinusWrapsAccumulator()
    {
        //{ {A} {B} MINUS {C} } — MINUS binds the whole accumulated left side: Minus(subquery(A,B), C)
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } { ?s <http://example.org/p2> ?x } MINUS { ?s <http://example.org/p3> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFBinaryQueryMember op = (RDFBinaryQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Minus, op.OperatorType);
        //Left operand must be a subquery wrapping A and B
        Assert.IsInstanceOfType<RDFSelectQuery>(op.LeftOperand);
        RDFSelectQuery leftSubQuery = (RDFSelectQuery)op.LeftOperand;
        Assert.AreEqual(2, leftSubQuery.GetPatternGroups().Count());
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldDropLeadingMinusResiliently()
    {
        //A leading MINUS with no left side: the right operand is kept as a plain element
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { MINUS { ?s <http://example.org/p> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        Assert.IsInstanceOfType<RDFPatternGroup>(evaluable[0]);
    }

    [TestMethod]
    public void ShouldDropStrayUnionResiliently()
    {
        //A stray UNION at the top level (no left GroupGraphPattern): it is dropped and parsing continues
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { UNION { ?s <http://example.org/p> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        Assert.IsInstanceOfType<RDFPatternGroup>(evaluable[0]);
    }

    [TestMethod]
    public void ShouldParseFilterOnlyGroup()
    {
        //A group whose only member is a FILTER is legal: since a FILTER scopes the whole group graph pattern (not a
        //single basic graph pattern), it is kept at WHERE-clause scope and the (empty) pattern group is not emitted
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { FILTER(?x > 0) }");

        Assert.AreEqual(0, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(1, query.QueryFilters.Count);
    }
    #endregion

    #region GraphPatternAlgebra (F2a end-to-end execution)
    //These tests close the loop that the round-trip and structural tests leave open: they parse a SPARQL
    //string, EXECUTE it against a real in-memory graph, and assert on the actual RESULT SET. This is what
    //actually proves that the algebra TREE the parser builds is evaluated by the engine with SPARQL-compliant
    //semantics — in particular that mixed nested operators bind as the spec requires (e.g. A ∪ (B ∖ C)) and
    //not with the fixed-precedence flattening of the old model (which computed (A ∪ B) ∖ C).

    //Membership dataset shared by the execution tests below.
    //Three predicates mark membership in the synthetic sets A, B, C:
    //  A = {x1,x2,x5}   (via <http://ex/inA>)
    //  B = {x2,x3}      (via <http://ex/inB>)
    //  C = {x3,x4,x5}   (via <http://ex/inC>)
    //The subjects deliberately overlap so that union/minus interplay is observable in the result set.
    private static RDFGraph BuildMembershipDataset()
    {
        RDFResource predicateInA = new RDFResource("http://ex/inA");
        RDFResource predicateInB = new RDFResource("http://ex/inB");
        RDFResource predicateInC = new RDFResource("http://ex/inC");
        RDFResource membershipFlag = new RDFResource("http://ex/yes");

        RDFGraph dataset = new RDFGraph();
        foreach (string subjectLocalName in new[] { "x1", "x2", "x5" })
            dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/" + subjectLocalName), predicateInA, membershipFlag));
        foreach (string subjectLocalName in new[] { "x2", "x3" })
            dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/" + subjectLocalName), predicateInB, membershipFlag));
        foreach (string subjectLocalName in new[] { "x3", "x4", "x5" })
            dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/" + subjectLocalName), predicateInC, membershipFlag));
        return dataset;
    }

    //Helper: run the query against the dataset and return the bound values of ?s, sorted, including duplicates
    //(SPARQL bag semantics — we deliberately do NOT de-duplicate, so multiset cardinality is observable).
    private static string[] ExecuteAndCollectSubjects(string sparqlQuery, RDFGraph dataset)
    {
        DataTable results = RDFSelectQuery.FromString(sparqlQuery).ApplyToGraph(dataset).SelectResults;
        return results.AsEnumerable()
                      .Select(row => row.Field<string>("?s"))
                      .OrderBy(subjectValue => subjectValue, System.StringComparer.Ordinal)
                      .ToArray();
    }

    [TestMethod]
    public void ShouldExecuteUnionOfMinusWithSparqlCompliantBindingAndBagSemantics()
    {
        //Spec grouping: A ∪ (B ∖ C). With B ∖ C = {x2,x3} ∖ {x3,x4,x5} = {x2}, the bag union with
        //A = {x1,x2,x5} yields {x1,x2,x2,x5} (x2 appears once from A and once from B∖C — bag union keeps both).
        //Crucially x5 SURVIVES, which would be impossible under the old fixed-precedence (A ∪ B) ∖ C = {x1,x2}.
        RDFGraph dataset = BuildMembershipDataset();
        string[] subjects = ExecuteAndCollectSubjects(
            "SELECT ?s WHERE { { ?s <http://ex/inA> ?f } UNION { { ?s <http://ex/inB> ?f } MINUS { ?s <http://ex/inC> ?f } } }",
            dataset);

        CollectionAssert.AreEqual(
            new[] { "http://ex/x1", "http://ex/x2", "http://ex/x2", "http://ex/x5" }, subjects,
            "Expected SPARQL-compliant A ∪ (B ∖ C) in bag semantics; actual: " + string.Join(",", subjects));
    }

    [TestMethod]
    public void ShouldExecuteMinusOfUnionWithSparqlCompliantBinding()
    {
        //Spec grouping: (A ∪ B) ∖ C. With A ∪ B = {x1,x2,x2,x3,x5} and C = {x3,x4,x5},
        //removing every left row whose ?s is in C drops x3 and x5, leaving {x1,x2,x2}.
        //This is the DUAL of the previous test and must differ from it, proving the tree honors the braces.
        RDFGraph dataset = BuildMembershipDataset();
        string[] subjects = ExecuteAndCollectSubjects(
            "SELECT ?s WHERE { { { ?s <http://ex/inA> ?f } UNION { ?s <http://ex/inB> ?f } } MINUS { ?s <http://ex/inC> ?f } }",
            dataset);

        CollectionAssert.AreEqual(
            new[] { "http://ex/x1", "http://ex/x2", "http://ex/x2" }, subjects,
            "Expected SPARQL-compliant (A ∪ B) ∖ C; actual: " + string.Join(",", subjects));
    }

    [TestMethod]
    public void ShouldExecuteOptionalKeepingLeftRowsWhenRightIsAbsent()
    {
        //OPTIONAL must preserve every left (A) row even when the right (C) pattern does not match, leaving ?c
        //unbound for those rows. A = {x1,x2,x5}; only x5 is also in C, so x1 and x2 keep ?c UNBOUND.
        RDFGraph dataset = BuildMembershipDataset();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?s ?c WHERE { ?s <http://ex/inA> ?f OPTIONAL { ?s <http://ex/inC> ?c } }")
            .ApplyToGraph(dataset).SelectResults;

        //All three A-subjects survive the left join
        string[] subjects = results.AsEnumerable()
            .Select(row => row.Field<string>("?s"))
            .OrderBy(subjectValue => subjectValue, System.StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(new[] { "http://ex/x1", "http://ex/x2", "http://ex/x5" }, subjects);

        //x5 has ?c bound (it is in C); x1 and x2 have ?c UNBOUND (left-join padding)
        int boundOptionalCount = results.AsEnumerable().Count(row => row.Field<string>("?c") != null);
        Assert.AreEqual(1, boundOptionalCount, "Only x5 should carry a bound ?c value");
    }
    #endregion

    #region GraphClause (F3 structural)
    //Helper: returns the single pattern of the single pattern group of a query member that the parser
    //produced for a GRAPH scope. Used to inspect the per-pattern Context the GRAPH clause fixes.
    private static RDFPattern SinglePatternOf(RDFQueryMember member)
    {
        RDFPatternGroup patternGroup = (RDFPatternGroup)member;
        return patternGroup.GetPatterns().Single();
    }

    [TestMethod]
    public void ShouldParseGraphWithVariableContext()
    {
        //GRAPH ?g { ?s ?p ?o } — the graph specifier is a variable, so every pattern in the scope is
        //decorated with that same RDFVariable as its context (the engine binds it per-quadruple).
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { GRAPH ?g { ?s <http://example.org/p> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFPattern pattern = SinglePatternOf(evaluable[0]);
        Assert.IsInstanceOfType<RDFVariable>(pattern.Context);
        Assert.AreEqual("?G", pattern.Context.ToString());
    }

    [TestMethod]
    public void ShouldParseGraphWithFixedIriContext()
    {
        //GRAPH <iri> { ... } — the graph specifier is a fixed IRI, so the pattern context is an RDFContext
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { GRAPH <http://example.org/g1> { ?s <http://example.org/p> ?o } }");

        RDFPattern pattern = SinglePatternOf(query.GetEvaluableQueryMembers().Single());
        Assert.IsInstanceOfType<RDFContext>(pattern.Context);
        Assert.AreEqual("http://example.org/g1", pattern.Context.ToString());
    }

    [TestMethod]
    public void ShouldParseGraphWithPrefixedNameContext()
    {
        //The IRI graph specifier may be a prefixed name resolved through the prologue
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { GRAPH ex:g1 { ?s ex:p ?o } }");

        RDFPattern pattern = SinglePatternOf(query.GetEvaluableQueryMembers().Single());
        Assert.IsInstanceOfType<RDFContext>(pattern.Context);
        Assert.AreEqual("http://example.org/g1", pattern.Context.ToString());
    }

    [TestMethod]
    public void ShouldParseGraphAroundInlineTriples()
    {
        //GRAPH applies to a TriplesBlock with several patterns: ALL of them carry the same context
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { GRAPH ?g { ?s <http://example.org/p1> ?o1 . ?s <http://example.org/p2> ?o2 } }");

        RDFPatternGroup patternGroup = (RDFPatternGroup)query.GetEvaluableQueryMembers().Single();
        List<RDFPattern> patterns = patternGroup.GetPatterns().ToList();
        Assert.AreEqual(2, patterns.Count);
        Assert.IsTrue(patterns.All(p => p.Context is RDFVariable && p.Context.ToString() == "?G"));
    }

    [TestMethod]
    public void ShouldPropagateGraphContextThroughUnion()
    {
        //GRAPH ?g { {A} UNION {B} } — the context FULL-propagates through the UNION to every pattern
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { GRAPH ?g { { ?s <http://example.org/p1> ?o } UNION { ?s <http://example.org/p2> ?o } } }");

        RDFBinaryQueryMember union = (RDFBinaryQueryMember)query.GetEvaluableQueryMembers().Single();
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, union.OperatorType);
        RDFPattern leftPattern = ((RDFPatternGroup)union.LeftOperand).GetPatterns().Single();
        RDFPattern rightPattern = ((RDFPatternGroup)union.RightOperand).GetPatterns().Single();
        Assert.AreEqual("?G", leftPattern.Context.ToString());
        Assert.AreEqual("?G", rightPattern.Context.ToString());
    }

    [TestMethod]
    public void ShouldShadowOuterGraphContextWithInnerGraph()
    {
        //SPARQL §18.4 innermost shadowing: the inner GRAPH ?h overrides ?g for its own sub-scope.
        //GRAPH ?g { ?s ?p ?o GRAPH ?h { ?a ?b ?c } } → ?s?p?o contextualised by ?g, ?a?b?c by ?h.
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { GRAPH ?g { ?s <http://example.org/p> ?o GRAPH ?h { ?a <http://example.org/q> ?c } } }");

        //The ?g scope holds two members (the outer BGP and the inner GRAPH), collapsed into a subquery
        RDFSelectQuery graphScopeSubQuery = (RDFSelectQuery)query.GetEvaluableQueryMembers().Single();
        List<RDFPatternGroup> patternGroups = graphScopeSubQuery.GetPatternGroups().ToList();
        Assert.AreEqual(2, patternGroups.Count);

        RDFPattern outerPattern = patternGroups[0].GetPatterns().Single();
        RDFPattern innerPattern = patternGroups[1].GetPatterns().Single();
        Assert.AreEqual("?G", outerPattern.Context.ToString());
        Assert.AreEqual("?H", innerPattern.Context.ToString());
    }

    [TestMethod]
    public void ShouldThrowOnEmptyGraphWithVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { GRAPH ?g { } }"));

    [TestMethod]
    public void ShouldThrowOnEmptyGraphWithFixedIri()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { GRAPH <http://example.org/g1> { } }"));

    [TestMethod]
    public void ShouldThrowOnNestedGraphWithoutOwnPatterns()
        //GRAPH ?g { GRAPH ?h { ... } } — ?g contextualises NO pattern of its own (all are shadowed by ?h):
        //the per-pattern model cannot anchor ?g (it would enumerate named graphs), so the clause is rejected.
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { GRAPH ?g { GRAPH ?h { ?a <http://example.org/q> ?c } } }"));

    [TestMethod]
    public void ShouldThrowOnLiteralGraphSpecifier()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { GRAPH \"notagraph\" { ?s ?p ?o } }"));
    #endregion

    #region GraphClause (F3 end-to-end execution against a store)
    //Named-graphs dataset for the GRAPH execution tests. Predicate <http://ex/p> links subjects to objects
    //across two named graphs:
    //  graph <http://ex/g1>: s1, s2
    //  graph <http://ex/g2>: s3
    private static RDFMemoryStore BuildNamedGraphsStore()
    {
        RDFContext graph1 = new RDFContext("http://ex/g1");
        RDFContext graph2 = new RDFContext("http://ex/g2");
        RDFResource predicate = new RDFResource("http://ex/p");
        RDFResource objectFlag = new RDFResource("http://ex/o");

        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(graph1, new RDFResource("http://ex/s1"), predicate, objectFlag));
        store.AddQuadruple(new RDFQuadruple(graph1, new RDFResource("http://ex/s2"), predicate, objectFlag));
        store.AddQuadruple(new RDFQuadruple(graph2, new RDFResource("http://ex/s3"), predicate, objectFlag));
        return store;
    }

    [TestMethod]
    public void ShouldExecuteGraphWithFixedIriFilteringToThatGraph()
    {
        //GRAPH <http://ex/g1> { ... } restricts matching to graph g1: only s1 and s2 are returned
        RDFMemoryStore store = BuildNamedGraphsStore();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?s WHERE { GRAPH <http://ex/g1> { ?s <http://ex/p> ?o } }")
            .ApplyToStore(store).SelectResults;

        string[] subjects = results.AsEnumerable()
            .Select(row => row.Field<string>("?s"))
            .OrderBy(s => s, System.StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(new[] { "http://ex/s1", "http://ex/s2" }, subjects);
    }

    [TestMethod]
    public void ShouldExecuteGraphWithVariableBindingTheGraphName()
    {
        //GRAPH ?g { ... } binds ?g to the named graph each solution comes from: g1 (twice) and g2 (once)
        RDFMemoryStore store = BuildNamedGraphsStore();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?g ?s WHERE { GRAPH ?g { ?s <http://ex/p> ?o } }")
            .ApplyToStore(store).SelectResults;

        string[] graphs = results.AsEnumerable()
            .Select(row => row.Field<string>("?G"))
            .OrderBy(g => g, System.StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(
            new[] { "http://ex/g1", "http://ex/g1", "http://ex/g2" }, graphs,
            "Expected ?g bound per-quadruple to the source named graph; actual: " + string.Join(",", graphs));
    }

    [TestMethod]
    public void ShouldExecuteGraphVariableJoinForcingSameGraph()
    {
        //Two patterns sharing the same ?g context must come from the SAME named graph (the shared context
        //is the join key). s1 and s2 are both in g1, s3 alone in g2: pairing ?s and ?s2 within one graph.
        RDFMemoryStore store = BuildNamedGraphsStore();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?g ?s ?s2 WHERE { GRAPH ?g { ?s <http://ex/p> ?o . ?s2 <http://ex/p> ?o } }")
            .ApplyToStore(store).SelectResults;

        //g1 contributes the 2x2 cross of {s1,s2}, g2 the 1x1 of {s3}: 5 rows total, all within a single graph
        Assert.AreEqual(5, results.Rows.Count);
        Assert.IsTrue(results.AsEnumerable().All(row =>
            (row.Field<string>("?G") == "http://ex/g1") || (row.Field<string>("?G") == "http://ex/g2")));
    }
    #endregion
}
