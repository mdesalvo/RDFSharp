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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFPropertyPathCardinalityTest
{
    #region Shared vocabulary
    private static readonly RDFResource Alice  = new RDFResource("ex:alice");
    private static readonly RDFResource Bob    = new RDFResource("ex:bob");
    private static readonly RDFResource Carol  = new RDFResource("ex:carol");
    private static readonly RDFResource Dave   = new RDFResource("ex:dave");
    private static readonly RDFResource Eve    = new RDFResource("ex:eve");
    private static readonly RDFResource Frank  = new RDFResource("ex:frank");
    private static readonly RDFResource Knows  = new RDFResource("ex:knows");
    private static readonly RDFResource Parent = new RDFResource("ex:parent");
    private static readonly RDFResource Type   = new RDFResource("rdf:type");
    private static readonly RDFResource Person = new RDFResource("ex:Person");

    private static readonly RDFVariable VarS = new RDFVariable("s");
    private static readonly RDFVariable VarE = new RDFVariable("e");
    private static readonly RDFVariable VarX = new RDFVariable("x");
    #endregion

    #region Algebra (step cardinality fluent API)

    [TestMethod]
    public void StepCardinality_Default_IsExactlyOne()
    {
        var step = new RDFPropertyPathStep(Knows);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne, step.StepCardinality);
        Assert.AreEqual(1, step.MinCardinality);
        Assert.AreEqual(1, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_ZeroOrOne_Fluent()
    {
        var step = new RDFPropertyPathStep(Knows).ZeroOrOne();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne, step.StepCardinality);
        Assert.AreEqual(0, step.MinCardinality);
        Assert.AreEqual(1, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_OneOrMore_Fluent()
    {
        var step = new RDFPropertyPathStep(Knows).OneOrMore();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, step.StepCardinality);
        Assert.AreEqual(1, step.MinCardinality);
        Assert.AreEqual(-1, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_ZeroOrMore_Fluent()
    {
        var step = new RDFPropertyPathStep(Knows).ZeroOrMore();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore, step.StepCardinality);
        Assert.AreEqual(0, step.MinCardinality);
        Assert.AreEqual(-1, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_BoundedRange_Fluent()
    {
        var step = new RDFPropertyPathStep(Knows).Repeat(2, 4);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.BoundedRange, step.StepCardinality);
        Assert.AreEqual(2, step.MinCardinality);
        Assert.AreEqual(4, step.MaxCardinality);
    }

    [TestMethod]
    [ExpectedException(typeof(RDFQueryException))]
    public void StepCardinality_Repeat_NegativeMin_Throws()
        => new RDFPropertyPathStep(Knows).Repeat(-1, 2);

    [TestMethod]
    [ExpectedException(typeof(RDFQueryException))]
    public void StepCardinality_Repeat_MaxLessThanMin_Throws()
        => new RDFPropertyPathStep(Knows).Repeat(3, 1);

    [TestMethod]
    public void StepCardinality_InverseCombines_WithCardinality()
    {
        var step = new RDFPropertyPathStep(Knows).Inverse().OneOrMore();
        Assert.IsTrue(step.IsInverseStep);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, step.StepCardinality);
    }

    [TestMethod]
    public void PropertyPath_HasTransitiveSteps_FalseByDefault()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows));
        Assert.IsFalse(path.HasTransitiveSteps);
    }

    [TestMethod]
    public void PropertyPath_HasTransitiveSteps_TrueForOneOrMore()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());
        Assert.IsTrue(path.HasTransitiveSteps);
    }

    [TestMethod]
    public void PropertyPath_HasTransitiveSteps_TrueForZeroOrMore()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());
        Assert.IsTrue(path.HasTransitiveSteps);
    }

    [TestMethod]
    public void PropertyPath_IsEvaluable_TrueForTransitiveSingleStepWithConcreteEnds()
    {
        // A transitive step with concrete start/end and Depth=1 should still be evaluable
        var path = new RDFPropertyPath(Alice, Bob)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());
        Assert.IsTrue(path.IsEvaluable);
    }

    #endregion

    #region SPARQL printer

    [TestMethod]
    public void Printer_SingleStep_ZeroOrOne()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>?") || printed.Contains("knows?"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_OneOrMore()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>+") || printed.Contains("knows+"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_ZeroOrMore()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>*") || printed.Contains("knows*"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_BoundedRange()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 4));
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("{2,4}"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_BoundedRange_ExactCount()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(3, 3));
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("{3}"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_InverseStep_OneOrMore()
    {
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("^") && (printed.Contains("knows>+") || printed.Contains("knows+")), $"Printed: {printed}");
    }

    #endregion

    #region Engine — OneOrMore (prop+)

    private static RDFGraph BuildChain()
    {
        // alice -> bob -> carol -> dave  (via ex:knows)
        var g = new RDFGraph();
        g.AddTriple(new RDFTriple(Alice, Knows, Bob));
        g.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        g.AddTriple(new RDFTriple(Carol, Knows, Dave));
        return g;
    }

    [TestMethod]
    public void Engine_OneOrMore_ChainForwardVariableEnd()
    {
        var graph = BuildChain();
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();

        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsTrue(ends.Contains(Dave.ToString()));
        Assert.IsFalse(ends.Contains(Alice.ToString())); // no self with OneOrMore
    }

    [TestMethod]
    public void Engine_OneOrMore_ChainBothVariable()
    {
        var graph = BuildChain();
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        // alice reachable from alice: bob, carol, dave
        // bob reachable from bob: carol, dave
        // carol reachable from carol: dave
        Assert.IsTrue(result.Rows.Count >= 6);
        var rows = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        Assert.IsTrue(rows.Any(p => p.Item1 == Alice.ToString() && p.Item2 == Dave.ToString()));
    }

    [TestMethod]
    public void Engine_OneOrMore_ConcreteStartEnd_Reachable()
    {
        var graph = BuildChain();
        var path = new RDFPropertyPath(Alice, Dave)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(1, result.Rows.Count);
    }

    [TestMethod]
    public void Engine_OneOrMore_ConcreteStartEnd_NotReachable()
    {
        var graph = BuildChain();
        var path = new RDFPropertyPath(Dave, Alice)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(0, result.Rows.Count);
    }

    [TestMethod]
    public void Engine_OneOrMore_InverseStep()
    {
        var graph = BuildChain();
        // dave ^knows+ ?e  => alice, bob, carol reachable via reverse knows
        var path = new RDFPropertyPath(Dave, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Alice.ToString()));
    }

    [TestMethod]
    public void Engine_OneOrMore_NoCycle_NoDuplicates()
    {
        // linear chain: no cycles
        var graph = BuildChain();
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToList();
        Assert.AreEqual(ends.Distinct().Count(), ends.Count, "No duplicates expected");
    }

    [TestMethod]
    public void Engine_OneOrMore_Cycle_DoesNotLoop()
    {
        // alice <-> bob  (cycle)
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Alice));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        // bob and carol reachable; alice is a back-edge but NOT included (OneOrMore, visited)
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
    }

    #endregion

    #region Engine — ZeroOrMore (prop*)

    [TestMethod]
    public void Engine_ZeroOrMore_IncludesSelf()
    {
        var graph = BuildChain();
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Alice.ToString()), "ZeroOrMore should include self");
        Assert.IsTrue(ends.Contains(Dave.ToString()));
    }

    [TestMethod]
    public void Engine_ZeroOrMore_BothVariables_IncludesIdentity()
    {
        var graph = BuildChain();
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var rows = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        // Self-pairs for alice, bob, carol, dave
        Assert.IsTrue(rows.Any(p => p.Item1 == Alice.ToString() && p.Item2 == Alice.ToString()));
        Assert.IsTrue(rows.Any(p => p.Item1 == Dave.ToString()  && p.Item2 == Dave.ToString()));
    }

    #endregion

    #region Engine — ZeroOrOne (prop?)

    [TestMethod]
    public void Engine_ZeroOrOne_IncludesSelf()
    {
        var graph = BuildChain();
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Alice.ToString()), "ZeroOrOne should include self");
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "ZeroOrOne should include 1-hop");
        Assert.IsFalse(ends.Contains(Carol.ToString()), "ZeroOrOne should NOT include 2-hops");
    }

    [TestMethod]
    public void Engine_ZeroOrOne_MaxOneHop()
    {
        var graph = BuildChain();
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(2, result.Rows.Count); // alice (self) + bob (1 hop)
    }

    #endregion

    #region Engine — BoundedRange (prop{n,m})

    [TestMethod]
    public void Engine_BoundedRange_Exact2()
    {
        var graph = BuildChain();
        // alice knows{2} ?e  => carol (exactly 2 hops)
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 2));

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsFalse(ends.Contains(Bob.ToString()),  "1-hop not expected");
        Assert.IsFalse(ends.Contains(Dave.ToString()), "3-hop not expected");
    }

    [TestMethod]
    public void Engine_BoundedRange_1To3()
    {
        var graph = BuildChain();
        // alice knows{1,3} ?e  => bob, carol, dave
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(1, 3));

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsTrue(ends.Contains(Dave.ToString()));
    }

    [TestMethod]
    public void Engine_BoundedRange_ZeroMin_IncludesSelf()
    {
        var graph = BuildChain();
        // alice knows{0,2} ?e  => alice, bob, carol
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(0, 2));

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Alice.ToString()), "0 hops => self");
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsFalse(ends.Contains(Dave.ToString()), "3-hop exceeds max=2");
    }

    [TestMethod]
    public void Engine_BoundedRange_EmptyResult_TooFewEdges()
    {
        var graph = BuildChain(); // chain length = 3 hops max
        // alice knows{5,7} ?e  => no results (chain too short)
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(5, 7));

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(0, result.Rows.Count);
    }

    #endregion

    #region Engine — SELECT query integration

    [TestMethod]
    public void SelectQuery_OneOrMore_ReturnsTransitiveClosure()
    {
        var graph = BuildChain();
        var query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        var result = query.ApplyToGraph(graph);
        Assert.IsTrue(result.SelectResultsCount >= 3);
    }

    [TestMethod]
    public void SelectQuery_ZeroOrMore_IncludesSelf()
    {
        var graph = BuildChain();
        var query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        var result = query.ApplyToGraph(graph);
        var ends = result.SelectResults.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Alice.ToString()));
    }

    [TestMethod]
    public void SelectQuery_ZeroOrOne_BothVars()
    {
        var graph = BuildChain();
        var query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne())));

        var result = query.ApplyToGraph(graph);
        // Self-pairs included + direct knows pairs
        Assert.IsTrue(result.SelectResultsCount > 3);
    }

    [TestMethod]
    public void SelectQuery_BoundedRange_Exact()
    {
        var graph = BuildChain();
        var query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 2))));

        var result = query.ApplyToGraph(graph);
        Assert.AreEqual(1, result.SelectResultsCount); // only carol
    }

    [TestMethod]
    public void AskQuery_OneOrMore_True()
    {
        var graph = BuildChain();
        var query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, Dave)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        var result = query.ApplyToGraph(graph);
        Assert.IsTrue(result.AskResult);
    }

    [TestMethod]
    public void AskQuery_OneOrMore_False()
    {
        var graph = BuildChain();
        var query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Dave, Alice)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        var result = query.ApplyToGraph(graph);
        Assert.IsFalse(result.AskResult);
    }

    [TestMethod]
    public void AskQuery_ZeroOrMore_SelfTrue()
    {
        var graph = BuildChain();
        var query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, Alice)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        var result = query.ApplyToGraph(graph);
        Assert.IsTrue(result.AskResult); // zero hops: alice -> alice
    }

    [TestMethod]
    public void SelectQuery_OneOrMore_WithAdditionalPattern()
    {
        // Graph: alice type Person, alice knows bob knows carol
        var graph = BuildChain();
        graph.AddTriple(new RDFTriple(Alice, Type, Person));
        graph.AddTriple(new RDFTriple(Bob,   Type, Person));

        var query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(VarS, Type, Person))
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        var result = query.ApplyToGraph(graph);
        // alice's reachable: bob, carol, dave; bob's reachable: carol, dave
        Assert.IsTrue(result.SelectResultsCount >= 5);
    }

    #endregion

    #region Engine — Store datasource

    [TestMethod]
    public void Engine_OneOrMore_OnMemoryStore()
    {
        var store = new RDFSharp.Store.RDFMemoryStore();
        var ctx   = new RDFSharp.Store.RDFContext("ex:ctx");
        store.AddQuadruple(new RDFSharp.Store.RDFQuadruple(ctx, Alice, Knows, Bob));
        store.AddQuadruple(new RDFSharp.Store.RDFQuadruple(ctx, Bob,   Knows, Carol));

        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, store);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
    }

    [TestMethod]
    public void Engine_ZeroOrMore_OnMemoryStore_IncludesSelf()
    {
        var store = new RDFSharp.Store.RDFMemoryStore();
        var ctx   = new RDFSharp.Store.RDFContext("ex:ctx");
        store.AddQuadruple(new RDFSharp.Store.RDFQuadruple(ctx, Alice, Knows, Bob));

        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, store);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Alice.ToString()));
        Assert.IsTrue(ends.Contains(Bob.ToString()));
    }

    #endregion

    #region Engine — Sequence path with mixed cardinality

    [TestMethod]
    public void Engine_Sequence_StaticThenTransitive()
    {
        // alice parent carol, carol knows dave knows eve
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Parent, Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows,  Dave));
        graph.AddTriple(new RDFTriple(Dave,  Knows,  Eve));

        // alice parent/knows+ ?e  => all nodes reachable from carol via knows+
        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Parent))
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Dave.ToString()));
        Assert.IsTrue(ends.Contains(Eve.ToString()));
        Assert.IsFalse(ends.Contains(Alice.ToString()));
        Assert.IsFalse(ends.Contains(Carol.ToString())); // parent step target, not knows+ result
    }

    [TestMethod]
    public void Engine_Sequence_TransitiveThenStatic()
    {
        // alice knows+ dave, dave parent eve
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows,  Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Dave));
        graph.AddTriple(new RDFTriple(Dave,  Parent, Eve));

        // ?s knows+/parent ?e
        var path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())
            .AddSequenceStep(new RDFPropertyPathStep(Parent));

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var rows = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        // alice knows+ bob/dave; dave parent eve → alice→eve, bob→eve
        Assert.IsTrue(rows.Any(p => p.Item1 == Alice.ToString() && p.Item2 == Eve.ToString()));
        Assert.IsTrue(rows.Any(p => p.Item1 == Bob.ToString()   && p.Item2 == Eve.ToString()));
    }

    #endregion

    #region Engine — Federation datasource

    [TestMethod]
    public void Engine_OneOrMore_Federation_ChainSplitAcrossTwoGraphs()
    {
        // Chain split: alice→bob in graph1, bob→carol→dave in graph2.
        // The transitive BFS must merge both sources to reach the full chain.
        var graph1 = new RDFGraph();
        graph1.AddTriple(new RDFTriple(Alice, Knows, Bob));

        var graph2 = new RDFGraph();
        graph2.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        graph2.AddTriple(new RDFTriple(Carol, Knows, Dave));

        var federation = new RDFFederation()
            .AddGraph(graph1)
            .AddGraph(graph2);

        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, federation);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "bob — 1 hop");
        Assert.IsTrue(ends.Contains(Carol.ToString()), "carol — 2 hops via graph2");
        Assert.IsTrue(ends.Contains(Dave.ToString()),  "dave — 3 hops via graph2");
        Assert.IsFalse(ends.Contains(Alice.ToString()), "alice must not appear (OneOrMore)");
    }

    [TestMethod]
    public void Engine_ZeroOrMore_Federation_HeterogeneousSources_IncludesSelf()
    {
        // Heterogeneous federation: one RDFGraph + one RDFMemoryStore.
        // ZeroOrMore must include the start node (zero-step case) even when data is spread.
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));

        var store = new RDFSharp.Store.RDFMemoryStore();
        var ctx   = new RDFSharp.Store.RDFContext("ex:ctx");
        store.AddQuadruple(new RDFSharp.Store.RDFQuadruple(ctx, Bob, Knows, Carol));

        var federation = new RDFFederation()
            .AddGraph(graph)
            .AddStore(store);

        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, federation);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Alice.ToString()), "alice — zero hops (self)");
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "bob — from graph source");
        Assert.IsTrue(ends.Contains(Carol.ToString()), "carol — from store source");
    }

    [TestMethod]
    public void Engine_BoundedRange_Federation_NestedFederation_ExactHops()
    {
        // Bounded range {2,3}: only nodes reachable in 2 or 3 hops must appear.
        // Data is nested inside a sub-federation to exercise the nested-federation code path.
        // Chain: alice→bob(1)→carol(2)→dave(3)→eve(4)
        var innerGraph = new RDFGraph();
        innerGraph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        innerGraph.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        var outerGraph = new RDFGraph();
        outerGraph.AddTriple(new RDFTriple(Carol, Knows, Dave));
        outerGraph.AddTriple(new RDFTriple(Dave,  Knows, Eve));

        var federation = new RDFFederation()
            .AddFederation(new RDFFederation().AddGraph(innerGraph))
            .AddGraph(outerGraph);

        var path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 3));

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, federation);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsFalse(ends.Contains(Bob.ToString()),  "bob — 1 hop, below range");
        Assert.IsTrue(ends.Contains(Carol.ToString()), "carol — 2 hops, in range");
        Assert.IsTrue(ends.Contains(Dave.ToString()),  "dave — 3 hops, in range");
        Assert.IsFalse(ends.Contains(Eve.ToString()),  "eve — 4 hops, above range");
    }

    [TestMethod]
    public void Engine_OneOrMore_InverseStep_Federation_MultiSource()
    {
        // Inverse path (^knows+) navigates edges in reverse across two graph sources.
        // dave ^knows+ ?e in a federation must reach carol, bob, alice.
        var graph1 = new RDFGraph();
        graph1.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph1.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        var graph2 = new RDFGraph();
        graph2.AddTriple(new RDFTriple(Carol, Knows, Dave));

        var federation = new RDFFederation()
            .AddGraph(graph1)
            .AddGraph(graph2);

        var path = new RDFPropertyPath(Dave, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore());

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, federation);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Carol.ToString()), "carol — 1 reverse hop (graph2)");
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "bob — 2 reverse hops (graph1)");
        Assert.IsTrue(ends.Contains(Alice.ToString()), "alice — 3 reverse hops (graph1)");
        Assert.IsFalse(ends.Contains(Dave.ToString()),  "dave must not appear (OneOrMore)");
    }

    #endregion

    #region Engine — Alternative path with cardinality

    [TestMethod]
    public void Engine_Alternative_OneOrMore()
    {
        // alice has both knows and parent edges directly
        var graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows,  Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Carol));   // knows chain: alice→bob→carol
        graph.AddTriple(new RDFTriple(Alice, Parent, Dave));
        graph.AddTriple(new RDFTriple(Dave,  Parent, Eve));     // parent chain: alice→dave→eve

        // alice (knows+|parent+) ?e  =  knows+ from alice UNION parent+ from alice
        var path = new RDFPropertyPath(Alice, VarE)
            .AddAlternativeSteps(new System.Collections.Generic.List<RDFPropertyPathStep>
            {
                new RDFPropertyPathStep(Knows).OneOrMore(),
                new RDFPropertyPathStep(Parent).OneOrMore()
            });

        var engine = new RDFQueryEngine();
        var result = engine.ApplyPropertyPath(path, graph);

        var ends = result.Rows.Cast<System.Data.DataRow>()
            .Select(r => r[0].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "knows 1-hop");
        Assert.IsTrue(ends.Contains(Carol.ToString()), "knows 2-hops");
        Assert.IsTrue(ends.Contains(Dave.ToString()),  "parent 1-hop");
        Assert.IsTrue(ends.Contains(Eve.ToString()),   "parent 2-hops");
    }

    #endregion
}
