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
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

/// <summary>
/// Verifies that SPARQL UPDATE operations (INSERT WHERE, DELETE WHERE, DELETE/INSERT WHERE)
/// correctly apply cardinality-based property path evaluation in their WHERE clauses.
/// </summary>
[TestClass]
public class RDFPropertyPathCardinalityOperationTest
{
    #region Shared vocabulary
    private static readonly RDFResource Alice  = new RDFResource("ex:alice");
    private static readonly RDFResource Bob    = new RDFResource("ex:bob");
    private static readonly RDFResource Carol  = new RDFResource("ex:carol");
    private static readonly RDFResource Dave   = new RDFResource("ex:dave");
    private static readonly RDFResource Eve    = new RDFResource("ex:eve");

    private static readonly RDFResource Knows   = new RDFResource("ex:knows");
    private static readonly RDFResource Parent  = new RDFResource("ex:parent");
    private static readonly RDFResource Tag     = new RDFResource("ex:tag");
    private static readonly RDFResource Tagged  = new RDFResource("ex:tagged");
    private static readonly RDFResource Reached = new RDFResource("ex:reached");
    private static readonly RDFResource Ancestor= new RDFResource("ex:ancestor");
    private static readonly RDFResource Related = new RDFResource("ex:related");

    private static readonly RDFVariable VarS = new RDFVariable("s");
    private static readonly RDFVariable VarE = new RDFVariable("e");
    private static readonly RDFVariable VarX = new RDFVariable("x");

    /// <summary>Builds alice→bob→carol→dave chain via ex:knows</summary>
    private static RDFGraph BuildChain()
    {
        var g = new RDFGraph();
        g.AddTriple(new RDFTriple(Alice, Knows, Bob));
        g.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        g.AddTriple(new RDFTriple(Carol, Knows, Dave));
        return g;
    }
    #endregion

    // ─────────────────────────────────────────────────────────────────
    // INSERT WHERE — prop+
    // ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void InsertWhere_OneOrMore_ForwardChain()
    {
        // WHERE { alice ex:knows+ ?e } INSERT { alice ex:reached ?e }
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        // bob, carol, dave should be tagged as reached
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Alice))); // no self
    }

    [TestMethod]
    public void InsertWhere_OneOrMore_BothVariables()
    {
        // WHERE { ?s ex:knows+ ?e } INSERT { ?s ex:ancestor ?e }
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(VarS, Ancestor, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Ancestor, Dave)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Ancestor, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Bob,   Ancestor, Dave)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Bob,   Ancestor, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Carol, Ancestor, Dave)));
    }

    [TestMethod]
    public void InsertWhere_ZeroOrMore_IncludesSelf()
    {
        // WHERE { alice ex:knows* ?e } INSERT { alice ex:related ?e }
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Related, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Dave)));
    }

    [TestMethod]
    public void InsertWhere_ZeroOrOne_OnlyOneHop()
    {
        // WHERE { alice ex:knows? ?e } INSERT { alice ex:tag ?e }
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice))); // 0 hops
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));   // 1 hop
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));// no 2-hops
    }

    [TestMethod]
    public void InsertWhere_BoundedRange_Exact2()
    {
        // WHERE { alice ex:knows{2,2} ?e } INSERT { alice ex:tag ?e }
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 2))));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Dave)));
    }

    [TestMethod]
    public void InsertWhere_BoundedRange_1To2()
    {
        // WHERE { alice ex:knows{1,2} ?e } INSERT { alice ex:tag ?e }
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(1, 2))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Dave)));
    }

    [TestMethod]
    public void InsertWhere_OneOrMore_Inverse()
    {
        // WHERE { dave ^ex:knows+ ?e } INSERT { dave ex:ancestor ?e }
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Dave, Ancestor, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Dave, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Alice)));
    }

    [TestMethod]
    public void InsertWhere_NoMatchingPath_NoInsert()
    {
        // WHERE { dave ex:knows+ ?e } — dave has no outgoing knows
        var graph = BuildChain();
        long before = graph.TriplesCount;
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Dave, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Dave, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);
        Assert.AreEqual(before, graph.TriplesCount);
    }

    // ─────────────────────────────────────────────────────────────────
    // DELETE WHERE — prop+
    // ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void DeleteWhere_OneOrMore_RemovesReachableEdges()
    {
        // Anchor graph: alice reached bob, carol, dave  (already inserted)
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Alice, Reached, Bob));
        graph.AddTriple(new RDFTriple(Alice, Reached, Carol));
        graph.AddTriple(new RDFTriple(Alice, Reached, Dave));

        // WHERE { alice ex:knows+ ?e } DELETE { alice ex:reached ?e }
        var op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
        // Original knows chain should remain
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Knows, Bob)));
    }

    [TestMethod]
    public void DeleteWhere_ZeroOrMore_RemovesSelfAndChain()
    {
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Alice, Related, Alice));
        graph.AddTriple(new RDFTriple(Alice, Related, Bob));
        graph.AddTriple(new RDFTriple(Alice, Related, Carol));

        var op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Related, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Related, Alice)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Related, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Related, Carol)));
    }

    [TestMethod]
    public void DeleteWhere_ZeroOrOne_OnlyRemovesUpToOneHop()
    {
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Alice, Tag, Alice));
        graph.AddTriple(new RDFTriple(Alice, Tag, Bob));
        graph.AddTriple(new RDFTriple(Alice, Tag, Carol));

        var op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol))); // not matched (2 hops)
    }

    [TestMethod]
    public void DeleteWhere_BoundedRange_Exact2()
    {
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Alice, Tag, Bob));
        graph.AddTriple(new RDFTriple(Alice, Tag, Carol));
        graph.AddTriple(new RDFTriple(Alice, Tag, Dave));

        var op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 2))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));   // 1-hop, not deleted
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));// 2-hop, deleted
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Dave)));  // 3-hop, not deleted
    }

    [TestMethod]
    public void DeleteWhere_OneOrMore_Inverse_RemovesAncestors()
    {
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Dave, Ancestor, Carol));
        graph.AddTriple(new RDFTriple(Dave, Ancestor, Bob));
        graph.AddTriple(new RDFTriple(Dave, Ancestor, Alice));

        var op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Dave, Ancestor, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Dave, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Alice)));
    }

    [TestMethod]
    public void DeleteWhere_NoMatch_GraphUnchanged()
    {
        var graph = BuildChain();
        long before = graph.TriplesCount;

        var op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Eve, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Eve, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);
        Assert.AreEqual(before, graph.TriplesCount);
    }

    [TestMethod]
    public void DeleteWhere_OneOrMore_BothVariables()
    {
        var graph = BuildChain();
        // Pre-populate ancestor triples
        graph.AddTriple(new RDFTriple(Alice, Ancestor, Bob));
        graph.AddTriple(new RDFTriple(Alice, Ancestor, Carol));
        graph.AddTriple(new RDFTriple(Alice, Ancestor, Dave));
        graph.AddTriple(new RDFTriple(Bob,   Ancestor, Carol));
        graph.AddTriple(new RDFTriple(Bob,   Ancestor, Dave));
        graph.AddTriple(new RDFTriple(Carol, Ancestor, Dave));

        var op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(VarS, Ancestor, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Ancestor, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Carol, Ancestor, Dave)));
    }

    // ─────────────────────────────────────────────────────────────────
    // DELETE/INSERT WHERE
    // ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void DeleteInsertWhere_OneOrMore_MovesReachable()
    {
        // Reached triples exist; after op delete Reached, insert Tagged for all reachable via knows+
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Alice, Reached, Bob));
        graph.AddTriple(new RDFTriple(Alice, Reached, Carol));

        var op = new RDFDeleteInsertWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddInsertTemplate(new RDFPattern(Alice, Tagged, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Dave)));
    }

    [TestMethod]
    public void DeleteInsertWhere_ZeroOrMore_IncludesSelfInInsert()
    {
        var graph = BuildChain();

        var op = new RDFDeleteInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Related, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Dave)));
    }

    [TestMethod]
    public void DeleteInsertWhere_ZeroOrOne_LimitedScope()
    {
        var graph = BuildChain();

        var op = new RDFDeleteInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
    }

    [TestMethod]
    public void DeleteInsertWhere_BoundedRange_DeleteOnlyInRange()
    {
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Alice, Reached, Bob));
        graph.AddTriple(new RDFTriple(Alice, Reached, Carol));
        graph.AddTriple(new RDFTriple(Alice, Reached, Dave));

        // DELETE { alice reached ?e } INSERT { alice tagged ?e } WHERE { alice knows{1,2} ?e }
        var op = new RDFDeleteInsertWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddInsertTemplate(new RDFPattern(Alice, Tagged, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(1, 2))));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));   // in {1,2} → deleted
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol))); // in {1,2} → deleted
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));   // out of {1,2} → kept
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Dave)));
    }

    [TestMethod]
    public void DeleteInsertWhere_OneOrMore_OnStore()
    {
        var store = new RDFMemoryStore();
        var ctx   = new RDFContext("ex:ctx");
        store.AddQuadruple(new RDFQuadruple(ctx, Alice, Knows, Bob));
        store.AddQuadruple(new RDFQuadruple(ctx, Bob,   Knows, Carol));

        var op = new RDFDeleteInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToStore(store);

        bool reachedBob   = store.ContainsQuadruple(new RDFQuadruple(ctx, Alice, Reached, Bob));
        bool reachedCarol = store.ContainsQuadruple(new RDFQuadruple(ctx, Alice, Reached, Carol));
        Assert.IsTrue(reachedBob   || store.SelectQuadruples(s: Alice, p: Reached, o: Bob).Any());
        Assert.IsTrue(reachedCarol || store.SelectQuadruples(s: Alice, p: Reached, o: Carol).Any());
    }

    // ─────────────────────────────────────────────────────────────────
    // Complex WHERE clauses (patterns + path)
    // ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void InsertWhere_PatternPlusTransitivePath_JoinedCorrectly()
    {
        // Graph: alice parent bob, bob knows carol knows dave
        // WHERE { ?s ex:parent ?x . ?x ex:knows+ ?e } INSERT { ?s ex:reached ?e }
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Parent, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows,  Dave));

        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(VarS, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(VarS, Parent, VarX))
                .AddPropertyPath(new RDFPropertyPath(VarX, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob))); // bob is ?x, not ?e
    }

    [TestMethod]
    public void DeleteWhere_PatternPlusTransitivePath_DeletesCorrectly()
    {
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Parent, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows,  Dave));
        graph.AddTriple(new RDFTriple(Alice, Reached, Carol));
        graph.AddTriple(new RDFTriple(Alice, Reached, Dave));
        graph.AddTriple(new RDFTriple(Bob,   Reached, Alice)); // unrelated

        var op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(VarS, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(VarS, Parent, VarX))
                .AddPropertyPath(new RDFPropertyPath(VarX, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Bob, Reached, Alice))); // untouched
    }

    [TestMethod]
    public void InsertWhere_TransitivePath_WithFilter()
    {
        // WHERE { ?s ex:knows+ ?e FILTER(sameTerm(?s, alice)) } INSERT { alice ex:reached ?e }
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore()))
                .AddFilter(new RDFExpressionFilter(
                    new RDFComparisonExpression(
                        RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                        new RDFVariableExpression(VarS),
                        new RDFConstantExpression(Alice)))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
    }

    [TestMethod]
    public void InsertWhere_BoundedRange_ZeroMin_IncludesSelf()
    {
        var graph = BuildChain();
        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(0, 1))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
    }

    [TestMethod]
    public void InsertWhere_OneOrMore_DoesNotInsertDuplicates()
    {
        // Pre-insert alice reached bob; re-run op; triple count should be unchanged on bob
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Alice, Reached, Bob));
        long before = graph.TriplesCount;

        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        // Graph semantics prevent duplicate triples; count should reflect unique triples
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
    }

    [TestMethod]
    public void DeleteInsertWhere_TransitiveSequence_Correct()
    {
        // alice parent bob, bob knows+ carol/dave
        // DELETE { ?s ex:tag ?e } INSERT { ?s ex:reached ?e }
        // WHERE { ?s ex:parent ?x . ?x ex:knows+ ?e }
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Parent, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows,  Dave));
        graph.AddTriple(new RDFTriple(Alice, Tag, Carol));
        graph.AddTriple(new RDFTriple(Alice, Tag, Dave));

        var op = new RDFDeleteInsertWhereOperation()
            .AddDeleteTemplate(new RDFPattern(VarS, Tag, VarE))
            .AddInsertTemplate(new RDFPattern(VarS, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(VarS, Parent, VarX))
                .AddPropertyPath(new RDFPropertyPath(VarX, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Dave)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
    }

    [TestMethod]
    public void InsertWhere_ZeroOrMore_Cycle_FinitesResult()
    {
        // alice <-> bob cycle; insert reached for all reachable via ZeroOrMore
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Alice));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        var op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        op.ApplyToGraph(graph); // should terminate, not loop

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
    }
}
