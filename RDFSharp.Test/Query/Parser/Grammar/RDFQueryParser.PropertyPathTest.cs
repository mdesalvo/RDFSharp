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
/// Unit tests for the property-path half of RDFQueryParser: the verb of a triples block may be a full SPARQL 1.1
/// Path expression (sequence '/', alternative '|', inverse '^', cardinality '? * +', negated property set '!')
/// rather than a single predicate. Coverage spans the W3C path grammar accepted by the parser, the construction
/// of the recursive RDFPropertyPathExpression tree, the now-supported recursive shapes, and round-trips.
/// </summary>
public partial class RDFQueryParserTest
{
    #region PropertyPath

    //Convenience: the single property path of a parsed single-group query, and its expression tree root
    private static RDFPropertyPath SingleParsedPath(string sparql)
        => RDFSelectQuery.FromString(sparql).GetPatternGroups().Single().GetPropertyPaths().Single();

    private static RDFPropertyPathExpression SingleParsedExpression(string sparql)
        => SingleParsedPath(sparql).Expression;

    //Asserts that the given expression is a Link over the given predicate with the given inverse/cardinality decorations
    private static void AssertLink(RDFPropertyPathExpression expression, string iri, bool inverse = false,
        RDFQueryEnums.RDFPropertyPathStepCardinalities cardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne)
    {
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Link, expression.Kind);
        Assert.IsTrue(expression.Property.Equals(new RDFResource(iri)));
        Assert.AreEqual(inverse, expression.IsInverse);
        Assert.AreEqual(cardinality, expression.Cardinality);
    }

    #region Parsing (W3C grammar)
    [TestMethod]
    public void ShouldKeepPlainPredicateAsTriple()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p> ?o }");

        //A bare predicate must stay a plain triple, NOT a degenerate single-step property path
        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
        Assert.IsFalse(query.GetPatternGroups().Single().GetPropertyPaths().Any());
    }

    [TestMethod]
    public void ShouldKeepAShorthandAsTriple()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s a ?o }");

        RDFPattern pattern = query.GetPatternGroups().Single().GetPatterns().Single();
        Assert.IsFalse(query.GetPatternGroups().Single().GetPropertyPaths().Any());
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource(RDFVocabulary.RDF.TYPE.ToString())));
    }

    [TestMethod]
    public void ShouldParseSequencePath()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s <http://example.org/p1>/<http://example.org/p2> ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, expression.Kind);
        Assert.AreEqual(2, expression.Children.Count);
        AssertLink(expression.Children[0], "http://example.org/p1");
        AssertLink(expression.Children[1], "http://example.org/p2");
    }

    [TestMethod]
    public void ShouldParseAlternativePath()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s <http://example.org/p1>|<http://example.org/p2> ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative, expression.Kind);
        Assert.AreEqual(2, expression.Children.Count);
        AssertLink(expression.Children[0], "http://example.org/p1");
        AssertLink(expression.Children[1], "http://example.org/p2");
    }

    [TestMethod]
    public void ShouldParseParenthesizedAlternativePath()
    {
        //Parenthesized pure alternative is equivalent to the bare alternative
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s (<http://example.org/p1>|<http://example.org/p2>) ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative, expression.Kind);
        Assert.AreEqual(2, expression.Children.Count);
    }

    [TestMethod]
    public void ShouldParseInverseStep()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s ^<http://example.org/p> ?o }");
        AssertLink(expression, "http://example.org/p", inverse: true);
    }

    [TestMethod]
    public void ShouldParseOneOrMoreStep()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s <http://example.org/p>+ ?o }");
        AssertLink(expression, "http://example.org/p", cardinality: RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore);
    }

    [TestMethod]
    public void ShouldParseZeroOrMoreStep()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s <http://example.org/p>* ?o }");
        AssertLink(expression, "http://example.org/p", cardinality: RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore);
    }

    [TestMethod]
    public void ShouldParseZeroOrOneStep()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s <http://example.org/p>? ?o }");
        AssertLink(expression, "http://example.org/p", cardinality: RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne);
    }

    [TestMethod]
    public void ShouldParseInverseOneOrMoreStep()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s ^<http://example.org/p>+ ?o }");
        AssertLink(expression, "http://example.org/p", inverse: true, cardinality: RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore);
    }

    [TestMethod]
    public void ShouldParseAlternativeThenSequencePath()
    {
        //(p1|p2)/p3 -> sequence of [alternative {p1,p2}, link p3]
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s (<http://example.org/p1>|<http://example.org/p2>)/<http://example.org/p3> ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, expression.Kind);
        Assert.AreEqual(2, expression.Children.Count);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative, expression.Children[0].Kind);
        AssertLink(expression.Children[1], "http://example.org/p3");
    }

    [TestMethod]
    public void ShouldParseSequenceThenAlternativePath()
    {
        //p1/(p2|p3) -> sequence of [link p1, alternative {p2,p3}]
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s <http://example.org/p1>/(<http://example.org/p2>|<http://example.org/p3>) ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, expression.Kind);
        Assert.AreEqual(2, expression.Children.Count);
        AssertLink(expression.Children[0], "http://example.org/p1");
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative, expression.Children[1].Kind);
    }

    [TestMethod]
    public void ShouldParseAShorthandInsidePath()
    {
        //'a' (rdf:type) as the first step of a sequence path
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s a/<http://example.org/p> ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, expression.Kind);
        AssertLink(expression.Children[0], RDFVocabulary.RDF.TYPE.ToString());
    }

    [TestMethod]
    public void ShouldFlattenRedundantSequenceGroup()
    {
        //(p1/p2) used as a sequence member flattens losslessly into a two-link sequence
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s (<http://example.org/p1>/<http://example.org/p2>) ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, expression.Kind);
        Assert.AreEqual(2, expression.Children.Count);
    }

    [TestMethod]
    public void ShouldParsePathWithMultipleObjects()
    {
        //A comma-separated object list shares the same start and path: one RDFPropertyPath per object
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p>+ ?o1, ?o2 }");

        Assert.AreEqual(2, query.GetPatternGroups().Single().GetPropertyPaths().Count());
    }

    [TestMethod]
    public void ShouldParsePathInPredicateObjectList()
    {
        //';' chains a path verb and a plain predicate on the same subject
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p1>/<http://example.org/p2> ?o ; <http://example.org/p3> ?z }");

        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPropertyPaths().Count());
        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }
    #endregion

    #region Recursive shapes (IP5.3 — now representable by the RDFPropertyPathExpression tree)
    [TestMethod]
    public void ShouldParseNegatedPropertySetSingle()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s !<http://example.org/p> ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.NegatedPropertySet, expression.Kind);
        Assert.AreEqual(1, expression.NegatedMembers.Count);
        Assert.IsTrue(expression.NegatedMembers[0].Property.Equals(new RDFResource("http://example.org/p")));
        Assert.IsFalse(expression.NegatedMembers[0].IsInverse);
    }

    [TestMethod]
    public void ShouldParseNegatedPropertySetWithInverseMembers()
    {
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s !(<http://example.org/p1>|^<http://example.org/p2>) ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.NegatedPropertySet, expression.Kind);
        Assert.AreEqual(2, expression.NegatedMembers.Count);
        Assert.IsFalse(expression.NegatedMembers[0].IsInverse);
        Assert.IsTrue(expression.NegatedMembers[1].IsInverse);
    }

    [TestMethod]
    public void ShouldParseGroupedCardinality()
    {
        //(p1/p2)+ -> sequence carrying a OneOrMore cardinality
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s (<http://example.org/p1>/<http://example.org/p2>)+ ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, expression.Kind);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, expression.Cardinality);
        Assert.AreEqual(2, expression.Children.Count);
    }

    [TestMethod]
    public void ShouldParseInverseGroup()
    {
        //^(p1|p2) -> alternative carrying the inverse flag
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s ^(<http://example.org/p1>|<http://example.org/p2>) ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative, expression.Kind);
        Assert.IsTrue(expression.IsInverse);
        Assert.AreEqual(2, expression.Children.Count);
    }

    [TestMethod]
    public void ShouldParseSequenceAsAlternativeBranch()
    {
        //p1/p2|p3 -> alternative of [sequence {p1,p2}, link p3]
        RDFPropertyPathExpression expression = SingleParsedExpression("SELECT * WHERE { ?s <http://example.org/p1>/<http://example.org/p2>|<http://example.org/p3> ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative, expression.Kind);
        Assert.AreEqual(2, expression.Children.Count);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, expression.Children[0].Kind);
        AssertLink(expression.Children[1], "http://example.org/p3");
    }
    #endregion

    #region Round-trips
    [TestMethod]
    public void ShouldRoundTripSequencePath()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("o"))
                    .AddSequenceStep(RDFPropertyPathExpression.Link(new RDFResource("http://example.org/p1")))
                    .AddSequenceStep(RDFPropertyPathExpression.Link(new RDFResource("http://example.org/p2")))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripAlternativePath()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("o"))
                    .AddAlternativeSteps([
                        RDFPropertyPathExpression.Link(new RDFResource("http://example.org/p1")),
                        RDFPropertyPathExpression.Link(new RDFResource("http://example.org/p2"))])));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripInverseOneOrMorePath()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("o"))
                    .AddSequenceStep(RDFPropertyPathExpression.Link(new RDFResource("http://example.org/p")).Inverse().OneOrMore())));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripAlternativeThenSequencePath()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("o"))
                    .AddAlternativeSteps([
                        RDFPropertyPathExpression.Link(new RDFResource("http://example.org/p1")),
                        RDFPropertyPathExpression.Link(new RDFResource("http://example.org/p2"))])
                    .AddSequenceStep(RDFPropertyPathExpression.Link(new RDFResource("http://example.org/p3")))));

        AssertSelectQueryRoundTrips(query);
    }

    /// <summary>
    /// Print→parse→print idempotence over composed paths, including the recursive shapes opened by IP5.3
    /// (negated property set, cardinality on a group, inverse of a group, sequence as an alternative branch).
    /// </summary>
    [TestMethod]
    [DataRow("foaf:knows/(foaf:name|foaf:account)")]   // alternative as a sequence member
    [DataRow("foaf:knows/(foaf:name/foaf:account)")]   // group-wrapped sequence spliced into the sequence
    [DataRow("foaf:knows|(foaf:name|foaf:account)")]   // nested alternative flattened into the parent
    [DataRow("!foaf:knows")]                            // negated property set, single member
    [DataRow("!(foaf:knows|^foaf:name)")]              // negated property set, inverse member
    [DataRow("(foaf:knows/foaf:name)+")]               // cardinality on a group
    [DataRow("^(foaf:knows|foaf:name)")]               // inverse of a group
    [DataRow("foaf:knows/foaf:name|foaf:account")]     // sequence as an alternative branch
    [DataRow("(^foaf:knows)+")]                         // cardinality of an inverse (explicit group)
    public void ShouldParseAndReserializeComposedPropertyPath(string path)
    {
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s {path} ?o }}");
        Assert.IsNotNull(query);

        string firstPrint = query.ToString();
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(firstPrint),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(firstPrint).ToString()));
    }
    #endregion

    #endregion
}
