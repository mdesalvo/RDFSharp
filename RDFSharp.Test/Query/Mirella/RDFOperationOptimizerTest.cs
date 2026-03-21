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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

/// <summary>
/// Regression suite verifying that RDFQueryOptimizer does not alter the functional
/// correctness of SPARQL UPDATE operations (INSERT WHERE, DELETE WHERE,
/// DELETE/INSERT WHERE) when the optimizer reorganizes the WHERE-clause patterns.
///
/// Every test follows the same structure:
///   1. Build a known RDF graph/store.
///   2. Execute a SPARQL UPDATE operation whose WHERE clause contains a combination
///      of plain patterns, OPTIONAL, UNION, MINUS, BIND and/or VALUES.
///   3. Assert that the triples/quadruples that were expected to change actually
///      changed, and that triples/quadruples that should be untouched are intact.
///
/// The optimizer is invoked transparently by RDFOperationEngine; these tests ensure
/// that reordering does not alter join semantics or filter evaluation.
/// </summary>
[TestClass]
public class RDFOperationOptimizerTest
{
    // ── shared resources ─────────────────────────────────────────────────────
    private static readonly RDFResource Person    = new RDFResource("ex:Person");
    private static readonly RDFResource Dog       = new RDFResource("ex:Dog");
    private static readonly RDFResource rdfType   = RDFVocabulary.RDF.TYPE;
    private static readonly RDFResource hasName   = new RDFResource("ex:hasName");
    private static readonly RDFResource dogOf     = new RDFResource("ex:dogOf");
    private static readonly RDFResource knows     = new RDFResource("ex:knows");
    private static readonly RDFResource age       = new RDFResource("ex:age");
    private static readonly RDFResource city      = new RDFResource("ex:city");
    private static readonly RDFResource tagged    = new RDFResource("ex:tagged");
    private static readonly RDFResource retired   = new RDFResource("ex:retired");
    private static readonly RDFResource processed = new RDFResource("ex:processed");

    private static readonly RDFResource alice   = new RDFResource("ex:alice");
    private static readonly RDFResource bob     = new RDFResource("ex:bob");
    private static readonly RDFResource carol   = new RDFResource("ex:carol");
    private static readonly RDFResource dave    = new RDFResource("ex:dave");
    private static readonly RDFResource pluto   = new RDFResource("ex:pluto");
    private static readonly RDFResource fido    = new RDFResource("ex:fido");

    #region Tests

    // ════════════════════════════════════════════════════════════════════════
    // INSERT WHERE
    // ════════════════════════════════════════════════════════════════════════

    // ── 1. INSERT WHERE with two plain patterns (reorderable) ────────────────
    [TestMethod]
    public void InsertWhere_TwoPlainPatterns_ShouldInsertMatchingTriple()
    {
        // WHERE: ?person rdf:type ex:Person . ?person ex:dogOf ?dog .
        // INSERT: ?person ex:tagged ?dog .
        // alice is a Person and has pluto as her dog → should be tagged
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(pluto, dogOf,   alice),   // pluto dogOf alice
            new RDFTriple(bob,   rdfType, Person)   // bob has no dog
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), tagged, new RDFVariable("?dog")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?dog"), dogOf, new RDFVariable("?person"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.InsertResultsCount, "Should tag alice with pluto");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, tagged, pluto)));
        Assert.AreEqual(4, graph.TriplesCount);
    }

    // ── 2. INSERT WHERE with OPTIONAL: absent optional must not block inserts ─
    [TestMethod]
    public void InsertWhere_WithOptional_ShouldInsertEvenWhenOptionalAbsent()
    {
        // WHERE: ?person rdf:type ex:Person . OPTIONAL { ?person ex:age ?a }
        // INSERT: ?person ex:tagged "yes"
        // alice has age, bob does not — both must be tagged
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, age,     new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), tagged, new RDFPlainLiteral("yes")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?person"), age, new RDFVariable("?a")).Optional()));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.InsertResultsCount, "Both alice and bob must be tagged");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, tagged, new RDFPlainLiteral("yes"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   tagged, new RDFPlainLiteral("yes"))));
    }

    // ── 3. INSERT WHERE with VALUES: only matching values must be inserted ────
    [TestMethod]
    public void InsertWhere_WithValues_ShouldInsertOnlyValuesMembers()
    {
        // VALUES { ?person { ex:alice ex:bob } }
        // WHERE: ?person rdf:type ex:Person .
        // carol is also a Person but not in VALUES
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person),
            new RDFTriple(carol, rdfType, Person)
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?person"), [alice, bob]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), tagged, new RDFPlainLiteral("selected")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person)));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.InsertResultsCount, "Only alice and bob are in VALUES");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, tagged, new RDFPlainLiteral("selected"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   tagged, new RDFPlainLiteral("selected"))));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(carol, tagged, new RDFPlainLiteral("selected"))));
    }

    // ── 4. INSERT WHERE with BIND: inserted triple must use bound variable ────
    [TestMethod]
    public void InsertWhere_WithBind_ShouldUseBindValue()
    {
        // WHERE: ?person rdf:type ex:Person . BIND(?person AS ?p2)
        // INSERT: ?p2 ex:processed "true"
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?p2"), processed, new RDFPlainLiteral("true")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?person")), new RDFVariable("?p2"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.InsertResultsCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, processed, new RDFPlainLiteral("true"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   processed, new RDFPlainLiteral("true"))));
    }

    // ── 5. INSERT WHERE with UNION: both branches contribute ─────────────────
    [TestMethod]
    public void InsertWhere_WithUnion_ShouldInsertFromBothBranches()
    {
        // WHERE: { ?x rdf:type ex:Person } UNION { ?x rdf:type ex:Dog }
        // INSERT: ?x ex:tagged "entity"
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(pluto, rdfType, Dog)
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?x"), tagged, new RDFPlainLiteral("entity")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?x"), rdfType, Person).UnionWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?x"), rdfType, Dog)));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.InsertResultsCount, "alice (Person) and pluto (Dog) must be tagged");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, tagged, new RDFPlainLiteral("entity"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(pluto, tagged, new RDFPlainLiteral("entity"))));
    }

    // ── 6. INSERT WHERE with MINUS: excluded subjects must not be inserted ────
    [TestMethod]
    public void InsertWhere_WithMinus_ShouldNotInsertExcludedSubjects()
    {
        // WHERE: ?person rdf:type ex:Person . MINUS { ?person ex:retired "true" }
        // alice is retired → excluded; bob is not → inserted
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType,  Person),
            new RDFTriple(alice, retired,  new RDFPlainLiteral("true")),
            new RDFTriple(bob,   rdfType,  Person)
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), processed, new RDFPlainLiteral("yes")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person).MinusWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?person"), retired, new RDFPlainLiteral("true"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.InsertResultsCount, "Only bob (not retired) must be processed");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob, processed, new RDFPlainLiteral("yes"))));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, processed, new RDFPlainLiteral("yes"))));
    }

    // ── 7. INSERT WHERE: VALUES + OPTIONAL combination ───────────────────────
    [TestMethod]
    public void InsertWhere_ValuesAndOptional_ShouldRespectBothConstraints()
    {
        // VALUES { ?person { ex:alice ex:bob } }
        // WHERE: ?person rdf:type ex:Person . OPTIONAL { ?person ex:city ?c }
        // INSERT: ?person ex:tagged "filtered"
        // carol is not in VALUES even though she is a Person with a city
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, city,    new RDFPlainLiteral("Rome")),
            new RDFTriple(bob,   rdfType, Person),
            new RDFTriple(carol, rdfType, Person),
            new RDFTriple(carol, city,    new RDFPlainLiteral("Milan"))
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?person"), [alice, bob]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), tagged, new RDFPlainLiteral("filtered")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?person"), city, new RDFVariable("?c")).Optional()));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.InsertResultsCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, tagged, new RDFPlainLiteral("filtered"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   tagged, new RDFPlainLiteral("filtered"))));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(carol, tagged, new RDFPlainLiteral("filtered"))));
    }

    // ── 8. INSERT WHERE: BIND + VALUES combination ───────────────────────────
    [TestMethod]
    public void InsertWhere_BindAndValues_ShouldRespectBothConstraints()
    {
        // VALUES { ?person { ex:alice } }
        // WHERE: ?person rdf:type ex:Person . BIND(?person AS ?p2)
        // INSERT: ?p2 ex:processed "done"
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?person"), [alice]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?p2"), processed, new RDFPlainLiteral("done")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?person")), new RDFVariable("?p2"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.InsertResultsCount, "Only alice is in VALUES");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, processed, new RDFPlainLiteral("done"))));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,  processed, new RDFPlainLiteral("done"))));
    }

    // ════════════════════════════════════════════════════════════════════════
    // DELETE WHERE
    // ════════════════════════════════════════════════════════════════════════

    // ── 9. DELETE WHERE with two plain patterns (reorderable) ─────────────
    [TestMethod]
    public void DeleteWhere_TwoPlainPatterns_ShouldDeleteMatchingTriples()
    {
        // WHERE: ?person rdf:type ex:Person . ?person ex:knows ?other .
        // DELETE: ?person ex:knows ?other
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, knows,   bob),
            new RDFTriple(bob,   rdfType, Person),
            new RDFTriple(bob,   knows,   carol),
            new RDFTriple(carol, knows,   dave)     // carol is not a Person
        ]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?other")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?other"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount, "alice→bob and bob→carol must be deleted");
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, knows, bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,   knows, carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(carol,  knows, dave)), "carol's triple must survive");
    }

    // ── 10. DELETE WHERE with OPTIONAL: optional absent → still delete ───────
    [TestMethod]
    public void DeleteWhere_WithOptional_ShouldDeleteRegardlessOfOptional()
    {
        // WHERE: ?person rdf:type ex:Person . OPTIONAL { ?person ex:city ?c }
        // DELETE: ?person rdf:type ex:Person
        // Both alice (has city) and bob (no city) must lose their rdf:type triple
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, city,    new RDFPlainLiteral("Rome")),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?person"), city, new RDFVariable("?c")).Optional()));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,   rdfType, Person)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, city, new RDFPlainLiteral("Rome"))));
    }

    // ── 11. DELETE WHERE with VALUES: only matching values deleted ────────────
    [TestMethod]
    public void DeleteWhere_WithValues_ShouldDeleteOnlyValueMembers()
    {
        // VALUES { ?person { ex:alice } }
        // WHERE: ?person rdf:type ex:Person .
        // DELETE: ?person rdf:type ex:Person
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?person"), [alice]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person)));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.DeleteResultsCount);
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   rdfType, Person)), "bob must survive");
    }

    // ── 12. DELETE WHERE with BIND ────────────────────────────────────────────
    [TestMethod]
    public void DeleteWhere_WithBind_ShouldDeleteUsingBoundVariable()
    {
        // WHERE: ?person rdf:type ex:Person . BIND(?person AS ?p2)
        // DELETE: ?p2 rdf:type ex:Person
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?p2"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?person")), new RDFVariable("?p2"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,   rdfType, Person)));
    }

    // ── 13. DELETE WHERE with UNION ───────────────────────────────────────────
    [TestMethod]
    public void DeleteWhere_WithUnion_ShouldDeleteFromBothBranches()
    {
        // WHERE: { ?x rdf:type ex:Person } UNION { ?x rdf:type ex:Dog }
        // DELETE: ?x rdf:type ?t  (but only the matched t)
        // Simpler: DELETE ?x ex:tagged "entity" after prior setup
        // Let's tag them first, then delete via UNION
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(pluto, rdfType, Dog)
        ]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        // DELETE: remove all rdf:type triples for Persons OR Dogs
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?x"), rdfType, new RDFVariable("?t")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?x"), rdfType, Person).UnionWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?x"), rdfType, Dog)));
        // We need ?t bound for the template — add a second pattern group that captures it
        // Simpler approach: use a typed template
        RDFDeleteWhereOperation op2 = new RDFDeleteWhereOperation();
        op2.AddDeleteTemplate(new RDFPattern(new RDFVariable("?x"), rdfType, Person));
        op2.AddDeleteTemplate(new RDFPattern(new RDFVariable("?x"), rdfType, Dog));
        op2.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?x"), rdfType, Person).UnionWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?x"), rdfType, Dog)));
        RDFOperationResult result = op2.ApplyToGraph(graph);

        Assert.IsTrue(result.DeleteResultsCount >= 2, "Both alice (Person) and pluto (Dog) type triples must be deleted");
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(pluto, rdfType, Dog)));
    }

    // ── 14. DELETE WHERE with MINUS ───────────────────────────────────────────
    [TestMethod]
    public void DeleteWhere_WithMinus_ShouldNotDeleteExcludedSubjects()
    {
        // WHERE: ?person rdf:type ex:Person . MINUS { ?person ex:retired "true" }
        // DELETE: ?person rdf:type ex:Person
        // alice is retired → must NOT be deleted; bob is not → must be deleted
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, retired, new RDFPlainLiteral("true")),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person).MinusWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?person"), retired, new RDFPlainLiteral("true"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.DeleteResultsCount, "Only bob must be deleted");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice,  rdfType, Person)), "alice must survive");
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,   rdfType, Person)));
    }

    // ── 15. DELETE WHERE: OPTIONAL + VALUES combination ───────────────────────
    [TestMethod]
    public void DeleteWhere_OptionalAndValues_ShouldRespectBothConstraints()
    {
        // VALUES { ?p { ex:bob } } WHERE: ?p rdf:type ex:Person . OPTIONAL { ?p ex:city ?c }
        // DELETE: ?p rdf:type ex:Person
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, city,    new RDFPlainLiteral("Rome")),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?p"), [bob]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?p"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(new RDFVariable("?p"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?p"), city, new RDFVariable("?c")).Optional()));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.DeleteResultsCount, "Only bob is in VALUES");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)), "alice must survive");
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,  rdfType, Person)));
    }

    // ── 16. DELETE WHERE: BIND + MINUS combination ────────────────────────────
    [TestMethod]
    public void DeleteWhere_BindAndMinus_ShouldRespectBothConstraints()
    {
        // WHERE: ?person rdf:type ex:Person .
        //        BIND(?person AS ?p2) .
        //        MINUS { ?person ex:retired "true" }
        // DELETE: ?p2 rdf:type ex:Person
        // alice is retired → excluded; bob is not → deleted via ?p2
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, retired, new RDFPlainLiteral("true")),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?p2"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person).MinusWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?person"), retired, new RDFPlainLiteral("true")))
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?person")), new RDFVariable("?p2"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.DeleteResultsCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)), "alice must survive");
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,  rdfType, Person)));
    }

    // ════════════════════════════════════════════════════════════════════════
    // DELETE/INSERT WHERE
    // ════════════════════════════════════════════════════════════════════════

    // ── 17. DELETE/INSERT WHERE: plain swap ───────────────────────────────────
    [TestMethod]
    public void DeleteInsertWhere_PlainPatterns_ShouldSwapTriples()
    {
        // WHERE: ?dog ex:dogOf ?owner . ?owner rdf:type ex:Person .
        // DELETE: ?dog ex:dogOf ?owner
        // INSERT: ?owner ex:hasDog ?dog
        RDFGraph graph = new RDFGraph([
            new RDFTriple(pluto, dogOf,   alice),
            new RDFTriple(fido,  dogOf,   bob),
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?dog"), dogOf, new RDFVariable("?owner")));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?owner"), new RDFResource("ex:hasDog"), new RDFVariable("?dog")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?dog"), dogOf, new RDFVariable("?owner")))
            .AddPattern(new RDFPattern(new RDFVariable("?owner"), rdfType, Person)));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.AreEqual(2, result.InsertResultsCount);
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(pluto, dogOf,   alice)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(fido,  dogOf,   bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, new RDFResource("ex:hasDog"), pluto)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   new RDFResource("ex:hasDog"), fido)));
    }

    // ── 18. DELETE/INSERT WHERE with OPTIONAL ─────────────────────────────────
    [TestMethod]
    public void DeleteInsertWhere_WithOptional_ShouldHandleMissingOptionalCorrectly()
    {
        // WHERE: ?person rdf:type ex:Person . OPTIONAL { ?person ex:city ?c }
        // DELETE: ?person rdf:type ex:Person
        // INSERT: ?person ex:processed "yes"
        // alice has city, bob does not — both must be processed
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, city,    new RDFPlainLiteral("Rome")),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), rdfType, Person));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), processed, new RDFPlainLiteral("yes")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?person"), city, new RDFVariable("?c")).Optional()));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.AreEqual(2, result.InsertResultsCount);
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,   rdfType, Person)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, processed, new RDFPlainLiteral("yes"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   processed, new RDFPlainLiteral("yes"))));
    }

    // ── 19. DELETE/INSERT WHERE with VALUES ───────────────────────────────────
    [TestMethod]
    public void DeleteInsertWhere_WithValues_ShouldOperateOnlyOnValueMembers()
    {
        // VALUES { ?person { ex:alice } }
        // WHERE: ?person rdf:type ex:Person .
        // DELETE: ?person rdf:type ex:Person
        // INSERT: ?person ex:processed "yes"
        // bob is a Person but NOT in VALUES → must be untouched
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?person"), [alice]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), rdfType, Person));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), processed, new RDFPlainLiteral("yes")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person)));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.DeleteResultsCount);
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, processed, new RDFPlainLiteral("yes"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,  rdfType, Person)), "bob must be untouched");
    }

    // ── 20. DELETE/INSERT WHERE with BIND ─────────────────────────────────────
    [TestMethod]
    public void DeleteInsertWhere_WithBind_ShouldUseBindInTemplates()
    {
        // WHERE: ?person rdf:type ex:Person . BIND(?person AS ?p2)
        // DELETE: ?person rdf:type ex:Person
        // INSERT: ?p2 ex:processed "bound"
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), rdfType, Person));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?p2"), processed, new RDFPlainLiteral("bound")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?person")), new RDFVariable("?p2"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.AreEqual(2, result.InsertResultsCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, processed, new RDFPlainLiteral("bound"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   processed, new RDFPlainLiteral("bound"))));
    }

    // ── 21. DELETE/INSERT WHERE with MINUS ────────────────────────────────────
    [TestMethod]
    public void DeleteInsertWhere_WithMinus_ShouldExcludeRetiredPersons()
    {
        // WHERE: ?person rdf:type ex:Person . MINUS { ?person ex:retired "true" }
        // DELETE: ?person rdf:type ex:Person
        // INSERT: ?person ex:processed "active"
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, retired, new RDFPlainLiteral("true")),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), rdfType, Person));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), processed, new RDFPlainLiteral("active")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person).MinusWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?person"), retired, new RDFPlainLiteral("true"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(1, result.DeleteResultsCount);
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)), "alice (retired) must not be deleted");
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,  rdfType, Person)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   processed, new RDFPlainLiteral("active"))));
    }

    // ════════════════════════════════════════════════════════════════════════
    // STORE (quadruple) scenarios
    // ════════════════════════════════════════════════════════════════════════

    // ── 22. INSERT WHERE on store with two plain patterns ─────────────────────
    [TestMethod]
    public void InsertWhere_OnStore_TwoPlainPatterns_ShouldInsertQuadruple()
    {
        RDFContext ctx = new RDFContext("ex:graph1");
        RDFMemoryStore store = new RDFMemoryStore([
            new RDFQuadruple(ctx, alice, rdfType, Person),
            new RDFQuadruple(ctx, alice, knows,   bob),
            new RDFQuadruple(ctx, bob,   rdfType, Person)
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(ctx, new RDFVariable("?person"), tagged, new RDFPlainLiteral("connected")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(ctx, new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(ctx, new RDFVariable("?person"), knows, new RDFVariable("?other"))));
        RDFOperationResult result = op.ApplyToStore(store);

        Assert.AreEqual(1, result.InsertResultsCount, "Only alice knows someone");
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, alice, tagged, new RDFPlainLiteral("connected"))));
    }

    // ── 23. INSERT WHERE on store with OPTIONAL ───────────────────────────────
    [TestMethod]
    public void InsertWhere_OnStore_WithOptional_ShouldInsertEvenWhenOptionalMissing()
    {
        RDFContext ctx = new RDFContext("ex:graph1");
        RDFMemoryStore store = new RDFMemoryStore([
            new RDFQuadruple(ctx, alice, rdfType, Person),
            new RDFQuadruple(ctx, alice, city,    new RDFPlainLiteral("Rome")),
            new RDFQuadruple(ctx, bob,   rdfType, Person)
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(ctx, new RDFVariable("?person"), processed, new RDFPlainLiteral("ok")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(ctx, new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(ctx, new RDFVariable("?person"), city, new RDFVariable("?c")).Optional()));
        RDFOperationResult result = op.ApplyToStore(store);

        Assert.AreEqual(2, result.InsertResultsCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, alice, processed, new RDFPlainLiteral("ok"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, bob,   processed, new RDFPlainLiteral("ok"))));
    }

    // ── 24. DELETE WHERE on store with VALUES ─────────────────────────────────
    [TestMethod]
    public void DeleteWhere_OnStore_WithValues_ShouldDeleteOnlyValueMembers()
    {
        RDFContext ctx = new RDFContext("ex:graph1");
        RDFMemoryStore store = new RDFMemoryStore([
            new RDFQuadruple(ctx, alice, rdfType, Person),
            new RDFQuadruple(ctx, bob,   rdfType, Person),
            new RDFQuadruple(ctx, carol, rdfType, Person)
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?person"), [alice, bob]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(ctx, new RDFVariable("?person"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(ctx, new RDFVariable("?person"), rdfType, Person)));
        RDFOperationResult result = op.ApplyToStore(store);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.IsFalse(store.ContainsQuadruple(new RDFQuadruple(ctx, alice, rdfType, Person)));
        Assert.IsFalse(store.ContainsQuadruple(new RDFQuadruple(ctx, bob,   rdfType, Person)));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(ctx, carol,  rdfType, Person)));
    }

    // ── 25. DELETE WHERE on store with BIND ───────────────────────────────────
    [TestMethod]
    public void DeleteWhere_OnStore_WithBind_ShouldDeleteUsingBoundVariable()
    {
        RDFContext ctx = new RDFContext("ex:graph1");
        RDFMemoryStore store = new RDFMemoryStore([
            new RDFQuadruple(ctx, alice, rdfType, Person),
            new RDFQuadruple(ctx, bob,   rdfType, Person)
        ]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(ctx, new RDFVariable("?p2"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(ctx, new RDFVariable("?person"), rdfType, Person))
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?person")), new RDFVariable("?p2"))));
        RDFOperationResult result = op.ApplyToStore(store);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.IsFalse(store.ContainsQuadruple(new RDFQuadruple(ctx, alice, rdfType, Person)));
        Assert.IsFalse(store.ContainsQuadruple(new RDFQuadruple(ctx, bob,   rdfType, Person)));
    }

    // ════════════════════════════════════════════════════════════════════════
    // Complex multi-constraint scenarios
    // ════════════════════════════════════════════════════════════════════════

    // ── 26. INSERT WHERE: four plain patterns (reordering maximally exercised) ─
    [TestMethod]
    public void InsertWhere_FourPlainPatterns_ShouldProduceCorrectJoin()
    {
        // ?person rdf:type Person . ?person ex:knows ?friend .
        // ?friend rdf:type Person . ?friend ex:city ?c .
        // INSERT: ?person ex:tagged ?c
        // alice knows bob. bob lives in Rome → alice tagged Rome
        // bob knows carol. carol lives in Milan → bob tagged Milan
        // dave knows alice but alice has no city → no insert for dave
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, knows,   bob),
            new RDFTriple(bob,   rdfType, Person),
            new RDFTriple(bob,   knows,   carol),
            new RDFTriple(bob,   city,    new RDFPlainLiteral("Rome")),
            new RDFTriple(carol, rdfType, Person),
            new RDFTriple(carol, city,    new RDFPlainLiteral("Milan")),
            new RDFTriple(dave,  rdfType, Person),
            new RDFTriple(dave,  knows,   alice)
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), tagged, new RDFVariable("?c")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?person"), knows,   new RDFVariable("?friend")))
            .AddPattern(new RDFPattern(new RDFVariable("?friend"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?friend"), city,    new RDFVariable("?c"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.InsertResultsCount);
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, tagged, new RDFPlainLiteral("Rome"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   tagged, new RDFPlainLiteral("Milan"))));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(dave,  tagged, new RDFPlainLiteral("Rome"))),
            "dave's friend alice has no city");
    }

    // ── 27. DELETE WHERE: OPTIONAL + BIND together ────────────────────────────
    [TestMethod]
    public void DeleteWhere_OptionalThenBind_ShouldDeleteFromBoundVar()
    {
        // WHERE: ?person rdf:type ex:Person .
        //        OPTIONAL { ?person ex:city ?c }
        //        BIND(?person AS ?p2)
        // DELETE: ?p2 rdf:type ex:Person
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, city,    new RDFPlainLiteral("Rome")),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?p2"), rdfType, Person));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?person"), city, new RDFVariable("?c")).Optional())
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?person")), new RDFVariable("?p2"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount);
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(bob,   rdfType, Person)));
    }

    // ── 28. DELETE/INSERT WHERE: UNION + BIND combo ───────────────────────────
    [TestMethod]
    public void DeleteInsertWhere_UnionAndBind_ShouldProcessBothBranches()
    {
        // WHERE: { ?x rdf:type ex:Person } UNION { ?x rdf:type ex:Dog }
        //        BIND(?x AS ?entity)
        // DELETE: ?entity rdf:type ex:Person
        // INSERT: ?entity ex:processed "entity"
        // pluto is a Dog (not Person) → not deleted but tagged
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(pluto, rdfType, Dog)
        ]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?entity"), rdfType, Person));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?entity"), processed, new RDFPlainLiteral("entity")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?x"), rdfType, Person).UnionWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?x"), rdfType, Dog))
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?x")), new RDFVariable("?entity"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.IsTrue(result.DeleteResultsCount >= 1, "alice (Person) must be deleted");
        Assert.IsTrue(result.InsertResultsCount >= 2, "both alice and pluto must be processed");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, processed, new RDFPlainLiteral("entity"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(pluto, processed, new RDFPlainLiteral("entity"))));
    }

    // ── 29. DELETE/INSERT WHERE: MINUS + VALUES combo ─────────────────────────
    [TestMethod]
    public void DeleteInsertWhere_MinusAndValues_ShouldApplyBothConstraints()
    {
        // VALUES { ?person { ex:alice ex:bob ex:carol } }
        // WHERE: ?person rdf:type ex:Person . MINUS { ?person ex:retired "true" }
        // DELETE: ?person rdf:type ex:Person
        // INSERT: ?person ex:processed "active"
        // alice is retired → excluded. bob and carol → processed. dave not in VALUES → untouched.
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, retired, new RDFPlainLiteral("true")),
            new RDFTriple(bob,   rdfType, Person),
            new RDFTriple(carol, rdfType, Person),
            new RDFTriple(dave,  rdfType, Person)
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?person"), [alice, bob, carol]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), rdfType, Person));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), processed, new RDFPlainLiteral("active")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person).MinusWithNext())
            .AddPattern(new RDFPattern(new RDFVariable("?person"), retired, new RDFPlainLiteral("true"))));
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount, "bob and carol must be deleted");
        Assert.AreEqual(2, result.InsertResultsCount, "bob and carol must be processed");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)), "alice (retired) must survive");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(dave,  rdfType, Person)), "dave (not in VALUES) must survive");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   processed, new RDFPlainLiteral("active"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(carol, processed, new RDFPlainLiteral("active"))));
    }

    // ── 30. DELETE/INSERT WHERE: all barrier types combined ───────────────────
    [TestMethod]
    public void DeleteInsertWhere_AllBarrierTypes_ComplexScenario()
    {
        // WHERE clause member sequence (showing optimizer barrier positions):
        //   VALUES { ?person { alice bob carol } }       ← barrier
        //   ?person rdf:type Person . MinusWithNext()   ← barrier (MINUS leader)
        //   ?person ex:retired "true"                    ← barrier (MINUS follower)
        //   ?person ex:knows ?friend                     ← plain (REORDERABLE)
        //   ?friend rdf:type Person                      ← plain (REORDERABLE, may be swapped)
        //   OPTIONAL { ?friend ex:city ?c }              ← barrier
        //   BIND(?person AS ?p2)                         ← barrier
        //
        // The optimizer may swap the two plain patterns; all barriers stay fixed.
        //
        // Evaluation trace:
        //   VALUES   → ?person ∈ {alice, bob, carol}
        //   MINUS    → (Person ∩ VALUES) MINUS retired = {bob, carol}   (alice IS retired)
        //   knows    → (bob, carol) and (carol, dave)
        //   friend IS Person → both carol and dave qualify
        //   OPTIONAL city → no city for carol or dave → null
        //   BIND     → ?p2 = bob or carol
        //
        // DELETE: ?p2 rdf:type Person  →  bob and carol deleted
        // INSERT: ?p2 ex:processed "complex"  →  bob and carol inserted
        // alice (retired) and dave (not in VALUES) must be untouched.
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, knows,   bob),
            new RDFTriple(alice, retired, new RDFPlainLiteral("true")),
            new RDFTriple(bob,   rdfType, Person),
            new RDFTriple(bob,   knows,   carol),
            new RDFTriple(carol, rdfType, Person),
            new RDFTriple(carol, knows,   dave),
            new RDFTriple(dave,  rdfType, Person)
        ]);

        RDFValues values = new RDFValues();
        values.AddColumn(new RDFVariable("?person"), [alice, bob, carol]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?p2"), rdfType, Person));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?p2"), processed, new RDFPlainLiteral("complex")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddValues(values)
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person).MinusWithNext())   // MINUS leader
            .AddPattern(new RDFPattern(new RDFVariable("?person"), retired, new RDFPlainLiteral("true")))   // MINUS follower
            .AddPattern(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?friend")))      // plain (reorderable)
            .AddPattern(new RDFPattern(new RDFVariable("?friend"), rdfType, Person))                        // plain (reorderable)
            .AddPattern(new RDFPattern(new RDFVariable("?friend"), city, new RDFVariable("?c")).Optional()) // barrier
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?person")), new RDFVariable("?p2")))  // barrier
        );
        RDFOperationResult result = op.ApplyToGraph(graph);

        Assert.AreEqual(2, result.DeleteResultsCount, "bob and carol must be deleted");
        Assert.AreEqual(2, result.InsertResultsCount, "bob and carol must be processed");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(bob,   processed, new RDFPlainLiteral("complex"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(carol, processed, new RDFPlainLiteral("complex"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, rdfType, Person)), "alice (retired) must survive");
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(dave,  rdfType, Person)), "dave (not in VALUES) must survive");
    }

    // ── 31. INSERT WHERE: idempotency — re-running does not duplicate triples ─
    [TestMethod]
    public void InsertWhere_Idempotency_RerunShouldNotDuplicateTriples()
    {
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(bob,   rdfType, Person)
        ]);

        RDFInsertWhereOperation op = new RDFInsertWhereOperation();
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), processed, new RDFPlainLiteral("ok")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person)));

        RDFOperationResult first  = op.ApplyToGraph(graph);
        RDFOperationResult second = op.ApplyToGraph(graph);   // same triples already present

        Assert.AreEqual(2, first.InsertResultsCount,  "First run should insert 2");
        Assert.AreEqual(0, second.InsertResultsCount, "Second run should insert 0 (already present)");
        Assert.AreEqual(4, graph.TriplesCount,        "Graph must not grow on second run");
    }

    // ── 32. DELETE/INSERT WHERE: multiple pattern groups (subquery-like) ──────
    [TestMethod]
    public void DeleteInsertWhere_MultiplePatternGroups_ShouldCombineCorrectly()
    {
        // Two separate pattern groups (combined as INNER JOIN by the engine):
        //   PG1: ?person rdf:type ex:Person . ?person ex:knows ?friend .
        //   PG2: ?friend rdf:type ex:Person .
        // DELETE: ?person ex:knows ?friend
        // INSERT: ?person ex:processed "knows-person"
        // alice knows bob (Person) and carol (Person). dave knows alice (Person).
        RDFGraph graph = new RDFGraph([
            new RDFTriple(alice, rdfType, Person),
            new RDFTriple(alice, knows,   bob),
            new RDFTriple(alice, knows,   carol),
            new RDFTriple(bob,   rdfType, Person),
            new RDFTriple(carol, rdfType, Person),
            new RDFTriple(dave,  rdfType, Person),
            new RDFTriple(dave,  knows,   alice)
        ]);

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation();
        op.AddDeleteTemplate(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?friend")));
        op.AddInsertTemplate(new RDFPattern(new RDFVariable("?person"), processed, new RDFPlainLiteral("knows-person")));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?person"), rdfType, Person))
            .AddPattern(new RDFPattern(new RDFVariable("?person"), knows, new RDFVariable("?friend"))));
        op.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?friend"), rdfType, Person)));
        RDFOperationResult result = op.ApplyToGraph(graph);

        // alice→bob, alice→carol, dave→alice all qualify
        Assert.AreEqual(3, result.DeleteResultsCount);
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, knows, bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(alice, knows, carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(dave,  knows, alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(alice, processed, new RDFPlainLiteral("knows-person"))));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(dave,  processed, new RDFPlainLiteral("knows-person"))));
    }

    #endregion
}
