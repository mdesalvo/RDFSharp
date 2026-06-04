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
/// Unit tests for RDFQueryOptimizer.OptimizePatternOrder.
/// The optimizer reorders contiguous blocks of plain inner-join RDFPattern instances
/// by ascending estimated cardinality. OPTIONAL, UNION/MINUS pairs, BIND, VALUES,
/// and PropertyPath members act as ordering barriers and must never be moved.
///
/// When the datasource is null the cardinality estimate falls back to the pattern's
/// variable count, so a pattern with fewer variables (more bound terms) is considered
/// more selective and is placed first.
/// </summary>
[TestClass]
public class RDFQueryOptimizerTest
{
    #region Helpers
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
    #endregion

    #region Tests

    // ── 1. Edge-case: empty list ────────────────────────────────────────────
    [TestMethod]
    public void ShouldReturnEmptyListUnchanged()
    {
        List<RDFPatternGroupMember> members = [];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
        Assert.AreEqual(0, result.Count);
    }

    // ── 2. Edge-case: single member ─────────────────────────────────────────
    [TestMethod]
    public void ShouldReturnSingleMemberUnchanged()
    {
        RDFPattern p = MakePlainPattern(2);
        List<RDFPatternGroupMember> members = [p];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
        // 'plain' is the only reorderable element (block length=1, no sort), 'opt' stays put
        Assert.AreSame(plain, result[0]);
        Assert.AreSame(opt,   result[1]);
    }

    // ── 7. UNION leader and its follower must not be reordered ───────────────
    [TestMethod]
    public void ShouldNotReorderUnionLeaderOrItsFollower()
    {
        // [unionLeader(vars=3), unionFollower(vars=0), plain(vars=1)]
        // unionLeader has JoinAsUnion=true → barrier
        // unionFollower is immediately after the leader → barrier
        // plain is the only reorderable; block length=1 → no change
        RDFPattern unionLeader   = MakePlainPattern(3, "a").UnionWithNext();
        RDFPattern unionFollower = MakePlainPattern(0, "b");
        RDFPattern plain         = MakePlainPattern(1, "c");
        List<RDFPatternGroupMember> members = [unionLeader, unionFollower, plain];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
        Assert.AreSame(unionLeader,   result[0]);
        Assert.AreSame(unionFollower, result[1]);
        Assert.AreSame(plain,         result[2]);
    }

    // ── 8. MINUS leader and its follower must not be reordered ───────────────
    [TestMethod]
    public void ShouldNotReorderMinusLeaderOrItsFollower()
    {
        RDFPattern minusLeader   = MakePlainPattern(3, "a").MinusWithNext();
        RDFPattern minusFollower = MakePlainPattern(0, "b");
        RDFPattern plain         = MakePlainPattern(1, "c");
        List<RDFPatternGroupMember> members = [minusLeader, minusFollower, plain];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
        Assert.AreSame(minusLeader,   result[0]);
        Assert.AreSame(minusFollower, result[1]);
        Assert.AreSame(plain,         result[2]);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
        Assert.AreSame(p0,     result[0]);
        Assert.AreSame(p3,     result[1]);
        Assert.AreSame(values, result[2]);
        Assert.AreSame(p1,     result[3]);
        Assert.AreSame(p2,     result[4]);
    }

    // ── 14. UNION pair preserved while surrounding plain patterns are sorted ──
    [TestMethod]
    public void ShouldPreserveUnionPairAndSortSurroundingPatterns()
    {
        // [p3, p1, union_leader, union_follower, p2, p0]
        // Block before union: [p3, p1] → sorted [p1, p3]
        // Union pair: locked in place
        // Block after union: [p2, p0] → sorted [p0, p2]
        RDFPattern p3           = MakePlainPattern(3, "a");
        RDFPattern p1           = MakePlainPattern(1, "b");
        RDFPattern unionLeader  = MakePlainPattern(2, "c").UnionWithNext();
        RDFPattern unionFollower = MakePlainPattern(0, "d");
        RDFPattern p2           = MakePlainPattern(2, "e");
        RDFPattern p0           = MakePlainPattern(0, "f");
        List<RDFPatternGroupMember> members = [p3, p1, unionLeader, unionFollower, p2, p0];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
        Assert.AreSame(p1,           result[0]);
        Assert.AreSame(p3,           result[1]);
        Assert.AreSame(unionLeader,  result[2]);
        Assert.AreSame(unionFollower,result[3]);
        Assert.AreSame(p0,           result[4]);
        Assert.AreSame(p2,           result[5]);
    }

    // ── 15. MINUS pair preserved while surrounding plain patterns are sorted ─
    [TestMethod]
    public void ShouldPreserveMinusPairAndSortSurroundingPatterns()
    {
        RDFPattern p3           = MakePlainPattern(3, "a");
        RDFPattern p0           = MakePlainPattern(0, "b");
        RDFPattern minusLeader  = MakePlainPattern(2, "c").MinusWithNext();
        RDFPattern minusFollower = MakePlainPattern(1, "d");
        RDFPattern p2           = MakePlainPattern(2, "e");
        RDFPattern p1           = MakePlainPattern(1, "f");
        List<RDFPatternGroupMember> members = [p3, p0, minusLeader, minusFollower, p2, p1];
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);
        Assert.AreSame(p0,            result[0]);
        Assert.AreSame(p3,            result[1]);
        Assert.AreSame(minusLeader,   result[2]);
        Assert.AreSame(minusFollower, result[3]);
        Assert.AreSame(p1,            result[4]);
        Assert.AreSame(p2,            result[5]);
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
        //   union_leader, union_follower     ← barrier pair
        //   [p2d, p0d]                       ← sortable block 4
        //   minus_leader, minus_follower     ← barrier pair
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
        RDFPattern unionL = MakePlainPattern(2, "uL").UnionWithNext();
        RDFPattern unionF = MakePlainPattern(0, "uF");
        RDFPattern p2d = MakePlainPattern(2, "2d");
        RDFPattern p0d = MakePlainPattern(0, "0d");
        RDFPattern minusL = MakePlainPattern(2, "mL").MinusWithNext();
        RDFPattern minusF = MakePlainPattern(1, "mF");
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
            unionL, unionF, // union pair (barriers)
            p2d, p0d, // block 4
            minusL, minusF, // minus pair (barriers)
            p3e, p1e
        ];

        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);

        // Block 1 sorted
        Assert.AreSame(p1a,    result[0],  "Block1[0] must be p1a (card=1)");
        Assert.AreSame(p3a,    result[1],  "Block1[1] must be p3a (card=3)");
        // VALUES barrier
        Assert.AreSame(values, result[2],  "values must stay in place");
        // Block 2 sorted
        Assert.AreSame(p0b,    result[3],  "Block2[0] must be p0b (card=0)");
        Assert.AreSame(p2b,    result[4],  "Block2[1] must be p2b (card=2)");
        // BIND barrier
        Assert.AreSame(bind,   result[5],  "bind must stay in place");
        // OPTIONAL barrier
        Assert.AreSame(opt,    result[6],  "opt must stay in place");
        // Block 3 sorted
        Assert.AreSame(p1c,    result[7],  "Block3[0] must be p1c (card=1)");
        Assert.AreSame(p3c,    result[8],  "Block3[1] must be p3c (card=3)");
        // UNION pair
        Assert.AreSame(unionL, result[9],  "unionL must stay in place");
        Assert.AreSame(unionF, result[10], "unionF must stay in place");
        // Block 4 sorted
        Assert.AreSame(p0d,    result[11], "Block4[0] must be p0d (card=0)");
        Assert.AreSame(p2d,    result[12], "Block4[1] must be p2d (card=2)");
        // MINUS pair
        Assert.AreSame(minusL, result[13], "minusL must stay in place");
        Assert.AreSame(minusF, result[14], "minusF must stay in place");
        // Block 5 sorted
        Assert.AreSame(p1e,    result[15], "Block5[0] must be p1e (card=1)");
        Assert.AreSame(p3e,    result[16], "Block5[1] must be p3e (card=3)");
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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, graph);

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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, store);

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

        _ = RDFQueryOptimizer.OptimizePatternOrder(original, null);

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
        List<RDFPatternGroupMember> result = RDFQueryOptimizer.OptimizePatternOrder(members, null);

        Assert.AreSame(p0,   result[0]);
        Assert.AreSame(p2,   result[1]);
        Assert.AreSame(p3,   result[2]);
        Assert.AreSame(bind, result[3]);
        Assert.AreSame(p1b,  result[4]);
        Assert.AreSame(p3b,  result[5]);
    }

    #endregion
}
