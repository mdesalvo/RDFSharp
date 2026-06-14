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
/// Path expression (sequence '/', alternative '|', inverse '^', cardinality '? * +') rather than a single
/// predicate. Coverage spans the W3C path grammar accepted by the parser, the flattening of redundant grouping,
/// the explicit rejection of shapes the RDFPropertyPath model cannot represent, and round-trips.
/// </summary>
public partial class RDFQueryParserTest
{
    #region PropertyPath

    //Convenience: the single property path of a parsed single-group query
    private static RDFPropertyPath SingleParsedPath(string sparql)
        => RDFSelectQuery.FromString(sparql).GetPatternGroups().Single().GetPropertyPaths().Single();

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
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s <http://example.org/p1>/<http://example.org/p2> ?o }");

        Assert.AreEqual(2, path.Steps.Count);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence, path.Steps[0].StepFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence, path.Steps[1].StepFlavor);
        Assert.IsTrue(path.Steps[0].StepProperty.Equals(new RDFResource("http://example.org/p1")));
        Assert.IsTrue(path.Steps[1].StepProperty.Equals(new RDFResource("http://example.org/p2")));
    }

    [TestMethod]
    public void ShouldParseAlternativePath()
    {
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s <http://example.org/p1>|<http://example.org/p2> ?o }");

        Assert.AreEqual(2, path.Steps.Count);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative, path.Steps[0].StepFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative, path.Steps[1].StepFlavor);
    }

    [TestMethod]
    public void ShouldParseParenthesizedAlternativePath()
    {
        //Parenthesized pure alternative is equivalent to the bare alternative
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s (<http://example.org/p1>|<http://example.org/p2>) ?o }");

        Assert.AreEqual(2, path.Steps.Count);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative, path.Steps[0].StepFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative, path.Steps[1].StepFlavor);
    }

    [TestMethod]
    public void ShouldParseInverseStep()
    {
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s ^<http://example.org/p> ?o }");

        Assert.AreEqual(1, path.Steps.Count);
        Assert.IsTrue(path.Steps[0].IsInverseStep);
    }

    [TestMethod]
    public void ShouldParseOneOrMoreStep()
    {
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s <http://example.org/p>+ ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, path.Steps[0].StepCardinality);
    }

    [TestMethod]
    public void ShouldParseZeroOrMoreStep()
    {
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s <http://example.org/p>* ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore, path.Steps[0].StepCardinality);
    }

    [TestMethod]
    public void ShouldParseZeroOrOneStep()
    {
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s <http://example.org/p>? ?o }");

        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne, path.Steps[0].StepCardinality);
    }

    [TestMethod]
    public void ShouldParseInverseOneOrMoreStep()
    {
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s ^<http://example.org/p>+ ?o }");

        Assert.IsTrue(path.Steps[0].IsInverseStep);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, path.Steps[0].StepCardinality);
    }

    [TestMethod]
    public void ShouldParseAlternativeThenSequencePath()
    {
        //(p1|p2)/p3 -> alternative run {p1,p2} then sequence p3
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s (<http://example.org/p1>|<http://example.org/p2>)/<http://example.org/p3> ?o }");

        Assert.AreEqual(3, path.Steps.Count);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative, path.Steps[0].StepFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative, path.Steps[1].StepFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence, path.Steps[2].StepFlavor);
    }

    [TestMethod]
    public void ShouldParseSequenceThenAlternativePath()
    {
        //p1/(p2|p3) -> sequence p1 then alternative run {p2,p3}
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s <http://example.org/p1>/(<http://example.org/p2>|<http://example.org/p3>) ?o }");

        Assert.AreEqual(3, path.Steps.Count);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence, path.Steps[0].StepFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative, path.Steps[1].StepFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative, path.Steps[2].StepFlavor);
    }

    [TestMethod]
    public void ShouldParseAShorthandInsidePath()
    {
        //'a' (rdf:type) as the first step of a sequence path
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s a/<http://example.org/p> ?o }");

        Assert.AreEqual(2, path.Steps.Count);
        Assert.IsTrue(path.Steps[0].StepProperty.Equals(new RDFResource(RDFVocabulary.RDF.TYPE.ToString())));
    }

    [TestMethod]
    public void ShouldFlattenRedundantSequenceGroup()
    {
        //(p1/p2) used as a sequence member flattens losslessly into two sequence steps
        RDFPropertyPath path = SingleParsedPath("SELECT * WHERE { ?s (<http://example.org/p1>/<http://example.org/p2>) ?o }");

        Assert.AreEqual(2, path.Steps.Count);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence, path.Steps[0].StepFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence, path.Steps[1].StepFlavor);
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

    #region Rejections (shapes not representable by the RDFPropertyPath model)
    [TestMethod]
    public void ShouldThrowOnNegatedPropertySet()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s !<http://example.org/p> ?o }"));

    [TestMethod]
    public void ShouldThrowOnGroupedCardinality()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s (<http://example.org/p1>/<http://example.org/p2>)+ ?o }"));

    [TestMethod]
    public void ShouldThrowOnInverseGroup()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ^(<http://example.org/p1>|<http://example.org/p2>) ?o }"));

    [TestMethod]
    public void ShouldThrowOnSequenceAsAlternativeBranch()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p1>/<http://example.org/p2>|<http://example.org/p3> ?o }"));
    #endregion

    #region Round-trips
    [TestMethod]
    public void ShouldRoundTripSequencePath()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("o"))
                    .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("http://example.org/p1")))
                    .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("http://example.org/p2")))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripAlternativePath()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("o"))
                    .AddAlternativeSteps([
                        new RDFPropertyPathStep(new RDFResource("http://example.org/p1")),
                        new RDFPropertyPathStep(new RDFResource("http://example.org/p2"))])));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripInverseOneOrMorePath()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("o"))
                    .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("http://example.org/p")).Inverse().OneOrMore())));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripAlternativeThenSequencePath()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(new RDFVariable("s"), new RDFVariable("o"))
                    .AddAlternativeSteps([
                        new RDFPropertyPathStep(new RDFResource("http://example.org/p1")),
                        new RDFPropertyPathStep(new RDFResource("http://example.org/p2"))])
                    .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("http://example.org/p3")))));

        AssertSelectQueryRoundTrips(query);
    }

    /// <summary>
    /// Covers the path-LOWERING composition branches: an alternative used as a sequence member, a (group-wrapped)
    /// sequence spliced into a sequence, a nested alternative flattened into its parent alternative, and a
    /// single-step group unwrapped inside an alternative branch. All are representable and re-serialize stably.
    /// </summary>
    [TestMethod]
    [DataRow("foaf:knows/(foaf:name|foaf:account)")] // alternative as a sequence member
    [DataRow("foaf:knows/(foaf:name/foaf:account)")] // group-wrapped sequence spliced into the sequence
    [DataRow("foaf:knows|(foaf:name|foaf:account)")] // nested alternative flattened into the parent
    [DataRow("foaf:knows|(foaf:name)")]              // single-step group unwrapped inside an alternative branch
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
