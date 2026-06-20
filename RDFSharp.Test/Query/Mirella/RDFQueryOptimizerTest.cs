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
/// <para>
/// Unit tests for RDFQueryOptimizer.OptimizePatternGroup.
/// The optimizer reorders contiguous blocks of plain inner-join RDFPattern instances
/// by ascending estimated cardinality. OPTIONAL, UNION/MINUS pairs, BIND, VALUES,
/// and PropertyPath members act as ordering barriers and must never be moved.
/// </para>
/// <para>
/// When the datasource is null the cardinality estimate falls back to the pattern's
/// variable count, so a pattern with fewer variables (more bound terms) is considered
/// more selective and is placed first.
/// </para>
/// </summary>
[TestClass]
public class RDFQueryOptimizerTest
{
    #region Utilities
    // Returns a plain inner-join pattern whose cardinality estimate (with null datasource)
    // equals the number of variable slots, controlled via the variableCount parameter.
    // variableCount 0 → fully bound (S rdf:type rdf:Class), cardinality 0
    // variableCount 1 → one variable,    e.g. (?s rdf:type rdf:Class)
    // variableCount 2 → two variables,   e.g. (?s ?p rdf:Class)
    // variableCount 3 → all variables,   e.g. (?s ?p ?o)
    private static RDFPattern MakePlainPattern(int variableCount, string suffix = "")
    {
        RDFResource s = new RDFResource("ex:s" + suffix);
        RDFResource p = new RDFResource("ex:p" + suffix);
        RDFResource o = new RDFResource("ex:o" + suffix);

        return variableCount switch
        {
            0 => new RDFPattern(s, p, o),
            1 => new RDFPattern(new RDFVariable("?s" + suffix), p, o),
            2 => new RDFPattern(new RDFVariable("?s" + suffix), new RDFVariable("?p" + suffix), o),
            _ => new RDFPattern(new RDFVariable("?s" + suffix), new RDFVariable("?p" + suffix), new RDFVariable("?o" + suffix))
        };
    }

    private static RDFBind MakeBind()
        => new RDFBind(new RDFVariableExpression(new RDFVariable("?x")), new RDFVariable("?bound"));

    private static RDFValues MakeValues()
    {
        RDFValues v = new RDFValues();
        v.AddColumn(new RDFVariable("?v"), [new RDFResource("ex:val")]);
        return v;
    }

    private static RDFPropertyPath MakePropertyPath()
        => new RDFPropertyPath(new RDFVariable("?start"), new RDFVariable("?end"))
            .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:prop")));

    // Builds an inner-join pattern from S/P/O tokens: a token starting with '?' becomes a variable, otherwise a
    // resource (prefixed "ex:"). This lets tests wire SHARED variables across patterns to drive the join-graph
    // heuristic (connectivity, filter-first, components), which the suffix-based MakePlainPattern cannot express.
    private static RDFPattern JoinPattern(string subject, string predicate, string @object)
        => new RDFPattern(AsPatternMember(subject), AsPatternMember(predicate), AsPatternMember(@object));

    private static RDFPatternMember AsPatternMember(string token)
        => token[0] == '?' ? new RDFVariable(token) : new RDFResource("ex:" + token);
    #endregion

    #region Tests

    // ── 1. Edge-case: empty list ────────────────────────────────────────────
    [TestMethod]
    public void ShouldReturnEmptyListUnchanged()
    {
        List<RDFPatternGroupMember> members = [];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreEqual(0, result.Count);
    }

    // ── 2. Edge-case: single member ─────────────────────────────────────────
    [TestMethod]
    public void ShouldReturnSingleMemberUnchanged()
    {
        RDFPattern p = MakePlainPattern(2);
        List<RDFPatternGroupMember> members = [p];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreEqual(1, result.Count);
        Assert.AreSame(p, result[0]);
    }

    // ── 3. Two plain patterns already in optimal order ───────────────────────
    [TestMethod]
    public void ShouldLeaveAlreadyOptimalOrderUnchanged()
    {
        // cardinality: p0(0) ≤ p1(1) — already sorted
        RDFPattern p0 = MakePlainPattern(0, "a");
        RDFPattern p1 = MakePlainPattern(1, "b");
        List<RDFPatternGroupMember> members = [p0, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p0, result[0]);
        Assert.AreSame(p1, result[1]);
    }

    // ── 4. Two plain patterns in wrong order → sorted ───────────────────────
    [TestMethod]
    public void ShouldSortTwoPlainPatternsByCardinality()
    {
        // cardinality: p3(3) > p1(1) — must be swapped
        RDFPattern p3 = MakePlainPattern(3, "a");
        RDFPattern p1 = MakePlainPattern(1, "b");
        List<RDFPatternGroupMember> members = [p3, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p1, result[0], "Lower cardinality must come first");
        Assert.AreSame(p3, result[1]);
    }

    // ── 5. Three plain patterns → sorted ascending ───────────────────────────
    [TestMethod]
    public void ShouldSortThreePlainPatternsByCardinalityAscending()
    {
        RDFPattern p3 = MakePlainPattern(3, "a");
        RDFPattern p0 = MakePlainPattern(0, "b");
        RDFPattern p1 = MakePlainPattern(1, "c");
        List<RDFPatternGroupMember> members = [p3, p0, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p0, result[0]);
        Assert.AreSame(p1, result[1]);
        Assert.AreSame(p3, result[2]);
    }

    // ── 6. OPTIONAL pattern is a barrier and must not be reordered ───────────
    [TestMethod]
    public void ShouldNotReorderOptionalPattern()
    {
        RDFPattern plain  = MakePlainPattern(3, "a");   // higher cardinality
        RDFPattern opt    = MakePlainPattern(0, "b").Optional(); // lower cardinality but OPTIONAL
        List<RDFPatternGroupMember> members = [plain, opt];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        // 'plain' is the only reorderable element (block length=1, no sort), 'opt' stays put
        Assert.AreSame(plain, result[0]);
        Assert.AreSame(opt,   result[1]);
    }

    // ── 7. UNION operator tree must not be reordered ───────────────────────
    [TestMethod]
    public void ShouldNotReorderUnionOperatorTree()
    {
        // [unionOp, plain(vars=1)]
        // unionOp is an RDFBinaryPatternGroupMember → barrier (not an RDFPattern)
        // plain is the only reorderable; block length=1 → no change
        RDFPattern unionLeft     = MakePlainPattern(3, "a");
        RDFPattern unionRight    = MakePlainPattern(0, "b");
        RDFBinaryPatternGroupMember unionOp = unionLeft.Union(unionRight);
        RDFPattern plain         = MakePlainPattern(1, "c");
        List<RDFPatternGroupMember> members = [unionOp, plain];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(unionOp, result[0]);
        Assert.AreSame(plain,   result[1]);
    }

    // ── 8. MINUS operator tree must not be reordered ────────────────────────
    [TestMethod]
    public void ShouldNotReorderMinusOperatorTree()
    {
        RDFPattern minusLeft     = MakePlainPattern(3, "a");
        RDFPattern minusRight    = MakePlainPattern(0, "b");
        RDFBinaryPatternGroupMember minusOp = minusLeft.Minus(minusRight);
        RDFPattern plain         = MakePlainPattern(1, "c");
        List<RDFPatternGroupMember> members = [minusOp, plain];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(minusOp, result[0]);
        Assert.AreSame(plain,   result[1]);
    }

    // ── 9. BIND acts as barrier ──────────────────────────────────────────────
    [TestMethod]
    public void ShouldTreatBindAsBarrier()
    {
        RDFBind bind   = MakeBind();
        RDFPattern p3  = MakePlainPattern(3, "a");
        RDFPattern p1  = MakePlainPattern(1, "b");
        // [p3, bind, p1] → p3 is alone in its block, p1 is alone in its block; bind is never moved
        List<RDFPatternGroupMember> members = [p3, bind, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p3,   result[0]);
        Assert.AreSame(bind, result[1]);
        Assert.AreSame(p1,   result[2]);
    }

    // ── 10. VALUES acts as barrier ───────────────────────────────────────────
    [TestMethod]
    public void ShouldTreatValuesAsBarrier()
    {
        RDFValues values = MakeValues();
        RDFPattern p3    = MakePlainPattern(3, "a");
        RDFPattern p1    = MakePlainPattern(1, "b");
        List<RDFPatternGroupMember> members = [p3, values, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p3,     result[0]);
        Assert.AreSame(values, result[1]);
        Assert.AreSame(p1,     result[2]);
    }

    // ── 11. PropertyPath acts as barrier ─────────────────────────────────────
    [TestMethod]
    public void ShouldTreatPropertyPathAsBarrier()
    {
        RDFPropertyPath path = MakePropertyPath();
        RDFPattern p3        = MakePlainPattern(3, "a");
        RDFPattern p1        = MakePlainPattern(1, "b");
        List<RDFPatternGroupMember> members = [p3, path, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p3,   result[0]);
        Assert.AreSame(path, result[1]);
        Assert.AreSame(p1,   result[2]);
    }

    // ── 12. Two independent sortable blocks separated by BIND ────────────────
    [TestMethod]
    public void ShouldSortTwoBlocksSeparatedByBind()
    {
        // Block 1: [p3, p1]      → after sort: [p1, p3]
        // Barrier: bind
        // Block 2: [p2, p0]      → after sort: [p0, p2]
        RDFPattern p3 = MakePlainPattern(3, "a");
        RDFPattern p1 = MakePlainPattern(1, "b");
        RDFBind bind  = MakeBind();
        RDFPattern p2 = MakePlainPattern(2, "c");
        RDFPattern p0 = MakePlainPattern(0, "d");
        List<RDFPatternGroupMember> members = [p3, p1, bind, p2, p0];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        // Block 1 sorted
        Assert.AreSame(p1,   result[0]);
        Assert.AreSame(p3,   result[1]);
        // Bind in place
        Assert.AreSame(bind, result[2]);
        // Block 2 sorted
        Assert.AreSame(p0,   result[3]);
        Assert.AreSame(p2,   result[4]);
    }

    // ── 13. Two sortable blocks separated by VALUES ──────────────────────────
    [TestMethod]
    public void ShouldSortTwoBlocksSeparatedByValues()
    {
        RDFPattern p3   = MakePlainPattern(3, "a");
        RDFPattern p0   = MakePlainPattern(0, "b");
        RDFValues values = MakeValues();
        RDFPattern p2   = MakePlainPattern(2, "c");
        RDFPattern p1   = MakePlainPattern(1, "d");
        List<RDFPatternGroupMember> members = [p3, p0, values, p2, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p0,     result[0]);
        Assert.AreSame(p3,     result[1]);
        Assert.AreSame(values, result[2]);
        Assert.AreSame(p1,     result[3]);
        Assert.AreSame(p2,     result[4]);
    }

    // ── 14. UNION operator tree preserved while surrounding plain patterns are sorted ──
    [TestMethod]
    public void ShouldPreserveUnionOperatorAndSortSurroundingPatterns()
    {
        // [p3, p1, unionOp, p2, p0]
        // Block before union: [p3, p1] → sorted [p1, p3]
        // Union operator: locked in place (not an RDFPattern)
        // Block after union: [p2, p0] → sorted [p0, p2]
        RDFPattern p3           = MakePlainPattern(3, "a");
        RDFPattern p1           = MakePlainPattern(1, "b");
        RDFPattern unionLeft    = MakePlainPattern(2, "c");
        RDFPattern unionRight   = MakePlainPattern(0, "d");
        RDFBinaryPatternGroupMember unionOp = unionLeft.Union(unionRight);
        RDFPattern p2           = MakePlainPattern(2, "e");
        RDFPattern p0           = MakePlainPattern(0, "f");
        List<RDFPatternGroupMember> members = [p3, p1, unionOp, p2, p0];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p1,      result[0]);
        Assert.AreSame(p3,      result[1]);
        Assert.AreSame(unionOp, result[2]);
        Assert.AreSame(p0,      result[3]);
        Assert.AreSame(p2,      result[4]);
    }

    // ── 15. MINUS operator tree preserved while surrounding plain patterns are sorted ─
    [TestMethod]
    public void ShouldPreserveMinusOperatorAndSortSurroundingPatterns()
    {
        RDFPattern p3           = MakePlainPattern(3, "a");
        RDFPattern p0           = MakePlainPattern(0, "b");
        RDFPattern minusLeft    = MakePlainPattern(2, "c");
        RDFPattern minusRight   = MakePlainPattern(1, "d");
        RDFBinaryPatternGroupMember minusOp = minusLeft.Minus(minusRight);
        RDFPattern p2           = MakePlainPattern(2, "e");
        RDFPattern p1           = MakePlainPattern(1, "f");
        List<RDFPatternGroupMember> members = [p3, p0, minusOp, p2, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);
        Assert.AreSame(p0,      result[0]);
        Assert.AreSame(p3,      result[1]);
        Assert.AreSame(minusOp, result[2]);
        Assert.AreSame(p1,      result[3]);
        Assert.AreSame(p2,      result[4]);
    }

    // ── 16. Complex scenario: VALUES → BIND → OPTIONAL → UNION → MINUS ──────
    [TestMethod]
    public void ShouldHandleComplexScenarioWithAllBarrierTypes()
    {
        // Layout:
        //   [p3a, p1a]                       ← sortable block 1
        //   values                           ← barrier
        //   [p2b, p0b]                       ← sortable block 2
        //   bind                             ← barrier
        //   optional                         ← barrier (not reorderable)
        //   [p3c, p1c]                       ← sortable block 3
        //   unionOp                          ← barrier (operator tree)
        //   [p2d, p0d]                       ← sortable block 4
        //   minusOp                          ← barrier (operator tree)
        //   [p3e, p1e]                       ← sortable block 5

        RDFPattern p3a = MakePlainPattern(3, "3a");
        RDFPattern p1a = MakePlainPattern(1, "1a");
        RDFValues  values = MakeValues();
        RDFPattern p2b = MakePlainPattern(2, "2b");
        RDFPattern p0b = MakePlainPattern(0, "0b");
        RDFBind    bind = MakeBind();
        RDFPattern opt  = MakePlainPattern(0, "opt").Optional();
        RDFPattern p3c = MakePlainPattern(3, "3c");
        RDFPattern p1c = MakePlainPattern(1, "1c");
        RDFBinaryPatternGroupMember unionOp = MakePlainPattern(2, "uL").Union(MakePlainPattern(0, "uF"));
        RDFPattern p2d = MakePlainPattern(2, "2d");
        RDFPattern p0d = MakePlainPattern(0, "0d");
        RDFBinaryPatternGroupMember minusOp = MakePlainPattern(2, "mL").Minus(MakePlainPattern(1, "mF"));
        RDFPattern p3e = MakePlainPattern(3, "3e");
        RDFPattern p1e = MakePlainPattern(1, "1e");

        List<RDFPatternGroupMember> members =
        [
            p3a, p1a, // block 1
            values, // barrier
            p2b, p0b, // block 2
            bind, // barrier
            opt, // barrier (optional)
            p3c, p1c, // block 3
            unionOp, // union operator tree (barrier)
            p2d, p0d, // block 4
            minusOp, // minus operator tree (barrier)
            p3e, p1e
        ];

        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);

        // Block 1 sorted
        Assert.AreSame(p1a,     result[0],  "Block1[0] must be p1a (card=1)");
        Assert.AreSame(p3a,     result[1],  "Block1[1] must be p3a (card=3)");
        // VALUES barrier
        Assert.AreSame(values,  result[2],  "values must stay in place");
        // Block 2 sorted
        Assert.AreSame(p0b,     result[3],  "Block2[0] must be p0b (card=0)");
        Assert.AreSame(p2b,     result[4],  "Block2[1] must be p2b (card=2)");
        // BIND barrier
        Assert.AreSame(bind,    result[5],  "bind must stay in place");
        // OPTIONAL barrier
        Assert.AreSame(opt,     result[6],  "opt must stay in place");
        // Block 3 sorted
        Assert.AreSame(p1c,     result[7],  "Block3[0] must be p1c (card=1)");
        Assert.AreSame(p3c,     result[8],  "Block3[1] must be p3c (card=3)");
        // UNION operator tree
        Assert.AreSame(unionOp, result[9],  "unionOp must stay in place");
        // Block 4 sorted
        Assert.AreSame(p0d,     result[10], "Block4[0] must be p0d (card=0)");
        Assert.AreSame(p2d,     result[11], "Block4[1] must be p2d (card=2)");
        // MINUS operator tree
        Assert.AreSame(minusOp, result[12], "minusOp must stay in place");
        // Block 5 sorted
        Assert.AreSame(p1e,     result[13], "Block5[0] must be p1e (card=1)");
        Assert.AreSame(p3e,     result[14], "Block5[1] must be p3e (card=3)");
    }

    // ── 17. Cardinality estimation on a real RDFGraph ────────────────────────
    [TestMethod]
    public void ShouldSortByCardinalityUsingRealGraph()
    {
        // Graph with two subjects: ex:alice (2 triples) and ex:bob (1 triple)
        RDFResource alice = new RDFResource("ex:alice");
        RDFResource bob   = new RDFResource("ex:bob");
        RDFResource name  = new RDFResource("ex:name");
        RDFResource knows = new RDFResource("ex:knows");

        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(alice, name,  new RDFPlainLiteral("Alice")));
        graph.AddTriple(new RDFTriple(alice, knows, bob));
        graph.AddTriple(new RDFTriple(bob,   name,  new RDFPlainLiteral("Bob")));

        // patternAlice matches only alice triples (2), patternBob only bob triples (1)
        RDFPattern patternAlice = new RDFPattern(alice, new RDFVariable("?p"), new RDFVariable("?o"));
        RDFPattern patternBob   = new RDFPattern(bob,   new RDFVariable("?p"), new RDFVariable("?o"));

        // Intentionally put the higher-cardinality pattern first
        List<RDFPatternGroupMember> members = [patternAlice, patternBob];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, graph);

        // patternBob has fewer index hits (1) → should come first
        Assert.AreSame(patternBob,   result[0], "patternBob (1 triple) must come first");
        Assert.AreSame(patternAlice, result[1]);
    }

    // ── 18. Cardinality estimation on a real RDFMemoryStore ─────────────────
    [TestMethod]
    public void ShouldSortByCardinalityUsingRealStore()
    {
        RDFContext ctx1 = new RDFContext("ex:graph1");
        RDFContext ctx2 = new RDFContext("ex:graph2");
        RDFResource alice = new RDFResource("ex:alice");
        RDFResource bob   = new RDFResource("ex:bob");
        RDFResource name  = new RDFResource("ex:name");

        RDFMemoryStore store = new RDFMemoryStore();
        // ctx1 has 2 quadruples
        store.AddQuadruple(new RDFQuadruple(ctx1, alice, name, new RDFPlainLiteral("Alice")));
        store.AddQuadruple(new RDFQuadruple(ctx1, bob,   name, new RDFPlainLiteral("Bob")));
        // ctx2 has 1 quadruple
        store.AddQuadruple(new RDFQuadruple(ctx2, alice, name, new RDFPlainLiteral("Alice2")));

        RDFPattern patternCtx1 = new RDFPattern(ctx1, new RDFVariable("?s"), name, new RDFVariable("?o"));
        RDFPattern patternCtx2 = new RDFPattern(ctx2, new RDFVariable("?s"), name, new RDFVariable("?o"));

        List<RDFPatternGroupMember> members = [patternCtx1, patternCtx2];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, store);

        // ctx2 has 1 quadruple → lower cardinality → comes first
        Assert.AreSame(patternCtx2, result[0], "patternCtx2 (1 quadruple) must come first");
        Assert.AreSame(patternCtx1, result[1]);
    }

    // ── 19. Original list must not be mutated ────────────────────────────────
    [TestMethod]
    public void ShouldNotMutateOriginalList()
    {
        RDFPattern p3 = MakePlainPattern(3, "a");
        RDFPattern p0 = MakePlainPattern(0, "b");
        List<RDFPatternGroupMember> original = [p3, p0];
        // Keep a snapshot of the original order
        List<RDFPatternGroupMember> snapshot = new List<RDFPatternGroupMember>(original);

        _ = RDFQueryOptimizer.OptimizePatternGroup(original, null);

        // The returned list is a copy; the original list instance itself is also a copy
        // made inside the optimizer, but what the caller passed must be unchanged in identity
        // (optimizer creates 'result = new List<>(members)' — original stays untouched)
        Assert.AreEqual(snapshot.Count, original.Count);
        for (int i = 0; i < snapshot.Count; i++)
            Assert.AreSame(snapshot[i], original[i]);
    }

    // ── 20. BIND between two sortable blocks: each block sorted independently ─
    [TestMethod]
    public void ShouldSortBlocksIndependentlyAroundBind()
    {
        // [p2, p3, p0, bind, p3b, p1b]
        // Block1 [p2, p3, p0] → sorted [p0, p2, p3]
        // Block2 [p3b, p1b]   → sorted [p1b, p3b]
        RDFPattern p2  = MakePlainPattern(2, "a");
        RDFPattern p3  = MakePlainPattern(3, "b");
        RDFPattern p0  = MakePlainPattern(0, "c");
        RDFBind bind   = MakeBind();
        RDFPattern p3b = MakePlainPattern(3, "d");
        RDFPattern p1b = MakePlainPattern(1, "e");

        List<RDFPatternGroupMember> members = [p2, p3, p0, bind, p3b, p1b];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);

        Assert.AreSame(p0,   result[0]);
        Assert.AreSame(p2,   result[1]);
        Assert.AreSame(p3,   result[2]);
        Assert.AreSame(bind, result[3]);
        Assert.AreSame(p1b,  result[4]);
        Assert.AreSame(p3b,  result[5]);
    }

    // ── 21. Join-graph: a CONNECTED pattern beats an equally/more selective DISCONNECTED one ──
    [TestMethod]
    public void ShouldPreferConnectedPatternOverDisconnected()
    {
        // With a null datasource cardinality == variable count, so 'seed' (1 var) is the most selective seed.
        // After it binds ?s, 'connected' shares ?s while 'disconnected' shares nothing: a pure cardinality sort
        // would keep declared order [seed, disconnected, connected], but the join-graph heuristic must pull the
        // connected pattern forward to avoid a cartesian product.
        RDFPattern seed         = JoinPattern("?s", "knows", "bob");  // {?s}    card 1
        RDFPattern disconnected = JoinPattern("?x", "p",     "?y");   // {?x,?y} card 2, shares nothing with ?s
        RDFPattern connected    = JoinPattern("?s", "age",   "?a");   // {?s,?a} card 2, shares ?s

        List<RDFPatternGroupMember> members = [seed, disconnected, connected];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);

        Assert.AreSame(seed,         result[0], "Most selective pattern must seed the chain");
        Assert.AreSame(connected,    result[1], "Connected pattern must be preferred over the disconnected one");
        Assert.AreSame(disconnected, result[2]);
    }

    // ── 22. Join-graph: a FILTER pattern (binds nothing new) runs before an expanding one ──
    [TestMethod]
    public void ShouldRunFilterPatternBeforeExpandingPattern()
    {
        // Once ?s and ?c are bound, 'filter' adds no new variable (it can only shrink the intermediate) while
        // 'expand' introduces ?d (it can grow it). Despite equal static cardinality, the filter must run first.
        RDFPattern seed   = JoinPattern("?s", "a", "o1");  // {?s}    card 1  -> seed
        RDFPattern bindC  = JoinPattern("?s", "b", "?c");  // {?s,?c} card 2  -> binds ?c
        RDFPattern expand = JoinPattern("?c", "d", "?d");  // {?c,?d} card 2  -> adds new ?d
        RDFPattern filter = JoinPattern("?s", "e", "?c");  // {?s,?c} card 2  -> all bound later => 0 new vars

        List<RDFPatternGroupMember> members = [seed, bindC, expand, filter];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);

        Assert.AreSame(seed,   result[0]);
        Assert.AreSame(bindC,  result[1]);
        Assert.AreSame(filter, result[2], "Filter (0 new variables) must precede the expanding pattern");
        Assert.AreSame(expand, result[3]);
    }

    // ── 23. Join-graph: two disconnected components stay contiguous (no cartesian interleaving) ──
    [TestMethod]
    public void ShouldKeepDisconnectedComponentsContiguous()
    {
        // Two independent chains A(?a..) and B(?x..). The heuristic seeds with the most selective pattern, then
        // exhausts that whole connected component before seeding the next one, so the two chains never interleave.
        RDFPattern a2 = JoinPattern("?a", "q", "?b");  // {?a,?b} card 2
        RDFPattern b1 = JoinPattern("?x", "p", "o2");  // {?x}    card 1  -> most selective => first seed
        RDFPattern a1 = JoinPattern("?a", "p", "o");   // {?a}    card 1
        RDFPattern b2 = JoinPattern("?x", "q", "?y");  // {?x,?y} card 2

        List<RDFPatternGroupMember> members = [a2, b1, a1, b2];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);

        // Component B (seeded by b1) is fully consumed before component A starts
        Assert.AreSame(b1, result[0], "Most selective pattern seeds first");
        Assert.AreSame(b2, result[1], "Rest of B's component follows contiguously");
        Assert.AreSame(a1, result[2], "Next component is seeded by its most selective pattern");
        Assert.AreSame(a2, result[3]);
    }

    // ── 24. Determinism: equally-ranked patterns keep their declared order (stable) ──
    [TestMethod]
    public void ShouldKeepDeclaredOrderAmongEquallyRankedPatterns()
    {
        // Three disconnected patterns with identical cardinality (3 vars each) and no shared variables: every
        // tie-breaker is equal, so the optimizer must return them in their original declared order.
        RDFPattern first  = MakePlainPattern(3, "a");
        RDFPattern second = MakePlainPattern(3, "b");
        RDFPattern third  = MakePlainPattern(3, "c");

        List<RDFPatternGroupMember> members = [first, second, third];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternGroup(members, null);

        Assert.AreSame(first,  result[0]);
        Assert.AreSame(second, result[1]);
        Assert.AreSame(third,  result[2]);
    }

    #endregion
}