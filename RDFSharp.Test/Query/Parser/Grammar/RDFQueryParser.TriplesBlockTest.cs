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

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the triples half of RDFQueryParser: blank-node and collection desugaring (structure and end-to-end execution).
/// </summary>
public partial class RDFQueryParserTest
{
    #region BlankNodesAndCollections (F4 structural)
    //Helper: the single pattern group of a single-member WHERE, for inspecting the desugared patterns.
    private static List<RDFPattern> PatternsOf(RDFSelectQuery query)
        => ((RDFPatternGroup)query.GetEvaluableQueryMembers().Single()).GetPatterns().ToList();

    [TestMethod]
    public void ShouldDesugarEmptyAnonymousBlankNodeAsFreshVariableObject()
    {
        //?s :p [] — the empty anonymous blank node is a fresh existential variable, no extra triples
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p> [] }");

        List<RDFPattern> patterns = PatternsOf(query);
        Assert.AreEqual(1, patterns.Count);
        Assert.IsInstanceOfType<RDFVariable>(patterns[0].Object);
        Assert.AreEqual("?_BNODE0", patterns[0].Object.ToString());
    }

    [TestMethod]
    public void ShouldDesugarStandaloneBlankNodePropertyList()
    {
        //[ :p :o ] — the blank node is the subject of the bracketed list; one triple, subject is a fresh variable
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { [ <http://example.org/p> <http://example.org/o> ] }");

        List<RDFPattern> patterns = PatternsOf(query);
        Assert.AreEqual(1, patterns.Count);
        Assert.IsInstanceOfType<RDFVariable>(patterns[0].Subject);
        Assert.AreEqual("?_BNODE0", patterns[0].Subject.ToString());
        Assert.AreEqual("http://example.org/o", patterns[0].Object.ToString());
    }

    [TestMethod]
    public void ShouldDesugarBlankNodePropertyListInObjectPosition()
    {
        //?s :p [ :q :r ] — emits the nested triple first, then the outer triple sharing the blank variable
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s <http://example.org/p> [ <http://example.org/q> <http://example.org/r> ] }");

        List<RDFPattern> patterns = PatternsOf(query);
        Assert.AreEqual(2, patterns.Count);
        //First emitted: the blank node's nested triple (?_BNODE0 :q :r)
        Assert.AreEqual("?_BNODE0", patterns[0].Subject.ToString());
        Assert.AreEqual("http://example.org/q", patterns[0].Predicate.ToString());
        //Then the outer triple (?s :p ?_BNODE0): same blank variable is the object
        Assert.AreEqual("?s".ToUpperInvariant(), patterns[1].Subject.ToString());
        Assert.AreEqual("?_BNODE0", patterns[1].Object.ToString());
    }

    [TestMethod]
    public void ShouldDesugarCollectionIntoFirstRestNilChain()
    {
        //?s :p ( 1 2 3 ) — desugar to a fresh-variable rdf:first/rdf:rest/rdf:nil chain (no rdf:type rdf:List)
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s <http://example.org/p> ( 1 2 3 ) }");

        List<RDFPattern> patterns = PatternsOf(query);
        //6 chain triples (3x first + 3x rest) + 1 outer triple
        Assert.AreEqual(7, patterns.Count);

        string first = RDFVocabulary.RDF.FIRST.ToString();
        string rest = RDFVocabulary.RDF.REST.ToString();
        string nil = RDFVocabulary.RDF.NIL.ToString();

        //No rdf:type rdf:List typing triple is emitted (spec-faithful, unlike the Turtle deserialiser)
        Assert.IsFalse(patterns.Any(p => p.Predicate.ToString() == RDFVocabulary.RDF.TYPE.ToString()));
        //Exactly three rdf:first and three rdf:rest
        Assert.AreEqual(3, patterns.Count(p => p.Predicate.ToString() == first));
        Assert.AreEqual(3, patterns.Count(p => p.Predicate.ToString() == rest));
        //The chain terminates with rdf:rest rdf:nil exactly once
        Assert.AreEqual(1, patterns.Count(p => p.Predicate.ToString() == rest && p.Object.ToString() == nil));
        //The head node (?_BNODE0) is the object of the outer triple
        RDFPattern outerTriple = patterns.Single(p => p.Predicate.ToString() == "http://example.org/p");
        Assert.AreEqual("?_BNODE0", outerTriple.Object.ToString());
    }

    [TestMethod]
    public void ShouldDesugarEmptyCollectionAsRdfNil()
    {
        //?s :p () — the empty collection is exactly rdf:nil, no chain triples
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p> () }");

        List<RDFPattern> patterns = PatternsOf(query);
        Assert.AreEqual(1, patterns.Count);
        Assert.IsInstanceOfType<RDFResource>(patterns[0].Object);
        Assert.AreEqual(RDFVocabulary.RDF.NIL.ToString(), patterns[0].Object.ToString());
    }

    [TestMethod]
    public void ShouldMapRepeatedLabeledBlankNodeToSameVariable()
    {
        //_:x used twice must denote the same variable; and it is a variable, not a constant blank resource
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { _:x <http://example.org/p> ?o . _:x <http://example.org/q> ?z }");

        List<RDFPattern> patterns = PatternsOf(query);
        Assert.AreEqual(2, patterns.Count);
        Assert.IsInstanceOfType<RDFVariable>(patterns[0].Subject);
        Assert.AreEqual("?_BNODE_X", patterns[0].Subject.ToString());
        //Same label → same variable instance/name on both triples
        Assert.AreEqual(patterns[0].Subject.ToString(), patterns[1].Subject.ToString());
    }

    [TestMethod]
    public void ShouldPropagateGraphContextThroughCollectionDesugaring()
    {
        //GRAPH ?g { ?s :p ( 1 2 ) } — every desugared chain triple must carry the graph context
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { GRAPH ?g { ?s <http://example.org/p> ( 1 2 ) } }");

        RDFPatternGroup patternGroup = (RDFPatternGroup)query.GetEvaluableQueryMembers().Single();
        List<RDFPattern> patterns = patternGroup.GetPatterns().ToList();
        Assert.IsTrue(patterns.Count > 1);
        //ALL patterns (outer + chain) are contextualised by the same ?g variable
        Assert.IsTrue(patterns.All(p => p.Context is RDFVariable && p.Context.ToString() == "?G"));
    }
    #endregion

    #region BlankNodesAndCollections (F4 end-to-end execution)
    [TestMethod]
    public void ShouldExecuteBlankNodePropertyListAgainstGraph()
    {
        //Query: ?p :knows [ :name ?name ]  →  ?p :knows ?b . ?b :name ?name
        RDFResource knows = new RDFResource("http://ex/knows");
        RDFResource name = new RDFResource("http://ex/name");
        RDFGraph dataset = new RDFGraph();
        dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/alice"), knows, new RDFResource("http://ex/bob")));
        dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/bob"), name, new RDFPlainLiteral("Bob")));

        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?p ?name WHERE { ?p <http://ex/knows> [ <http://ex/name> ?name ] }")
            .ApplyToGraph(dataset).SelectResults;

        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("http://ex/alice", results.Rows[0].Field<string>("?p"));
        Assert.AreEqual("Bob", results.Rows[0].Field<string>("?name"));
    }

    [TestMethod]
    public void ShouldExecuteCollectionPatternAgainstAnActualRdfList()
    {
        //Build an actual two-element rdf:list in the data: :s :hasList (head) ; head=("a","b")
        RDFResource hasList = new RDFResource("http://ex/hasList");
        RDFResource listNode0 = new RDFResource();
        RDFResource listNode1 = new RDFResource();
        RDFGraph dataset = new RDFGraph();
        dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/s"), hasList, listNode0));
        dataset.AddTriple(new RDFTriple(listNode0, RDFVocabulary.RDF.FIRST, new RDFPlainLiteral("a")));
        dataset.AddTriple(new RDFTriple(listNode0, RDFVocabulary.RDF.REST, listNode1));
        dataset.AddTriple(new RDFTriple(listNode1, RDFVocabulary.RDF.FIRST, new RDFPlainLiteral("b")));
        dataset.AddTriple(new RDFTriple(listNode1, RDFVocabulary.RDF.REST, RDFVocabulary.RDF.NIL));

        //Query a two-element collection of variables: matches the list, binding ?x="a", ?y="b"
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?x ?y WHERE { <http://ex/s> <http://ex/hasList> ( ?x ?y ) }")
            .ApplyToGraph(dataset).SelectResults;

        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("a", results.Rows[0].Field<string>("?x"));
        Assert.AreEqual("b", results.Rows[0].Field<string>("?y"));
    }
    #endregion
}
