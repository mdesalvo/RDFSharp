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
/// Unit tests for the DESCRIBE-form half of RDFQueryParser: the describe-terms ('*' or a list of variables/IRIs),
/// the OPTIONAL WHERE clause, and the model-imposed failures (dataset clauses and ORDER BY/GROUP BY/HAVING
/// modifiers, all non-representable on a DESCRIBE query).
/// </summary>
public partial class RDFQueryParserTest
{
    #region DescribeQuery

    /// <summary>
    /// Asserts that printing the given DESCRIBE query, parsing the print back, and printing the result yields the
    /// very same text: the round-trip oracle for the DESCRIBE form (sibling of <see cref="AssertAskQueryRoundTrips"/>).
    /// </summary>
    private static void AssertDescribeQueryRoundTrips(RDFDescribeQuery originalQuery)
    {
        string printedQuery = originalQuery.ToString();
        RDFDescribeQuery reparsedQuery = RDFDescribeQuery.FromString(printedQuery);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(printedQuery), RDFTestUtilities.NormalizeEOL(reparsedQuery.ToString()));
    }

    [TestMethod]
    public void ShouldRoundTripDescribeWildcard()
        => AssertDescribeQueryRoundTrips(new RDFDescribeQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))));

    [TestMethod]
    public void ShouldRoundTripDescribeWithVariableTerm()
        => AssertDescribeQueryRoundTrips(new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("s"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))));

    [TestMethod]
    public void ShouldRoundTripDescribeWithPrefixedIriTerms()
        => AssertDescribeQueryRoundTrips(new RDFDescribeQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
            .AddDescribeTerm(RDFVocabulary.FOAF.PERSON)
            .AddDescribeTerm(new RDFVariable("p"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("p"), RDFVocabulary.RDF.TYPE, RDFVocabulary.FOAF.PERSON))));

    [TestMethod]
    public void ShouldRoundTripDescribeWithLimitAndOffset()
        => AssertDescribeQueryRoundTrips(new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("s"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddModifier(new RDFLimitModifier(10))
            .AddModifier(new RDFOffsetModifier(5)));

    [TestMethod]
    public void ShouldRoundTripDescribeWithOrderByAndGroupBy()
        => AssertDescribeQueryRoundTrips(new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("s"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddModifier(new RDFGroupByModifier([new RDFVariable("s")]))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("s"), RDFQueryEnums.RDFOrderByFlavors.ASC)));

    [TestMethod]
    public void ShouldRoundTripDescribeWithUnionInWhere()
        => AssertDescribeQueryRoundTrips(RDFDescribeQuery.FromString(
            "DESCRIBE ?s WHERE { { ?s ?p ?o } UNION { ?a ?b ?c } }"));

    [TestMethod]
    public void ShouldParseDescribeWildcardWithWhere()
    {
        RDFDescribeQuery query = RDFDescribeQuery.FromString("DESCRIBE * WHERE { ?s ?p ?o }");

        //'*' is modeled by an empty describe-term list
        Assert.AreEqual(0, query.DescribeTerms.Count);
        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseDescribeWithVariableAndIriTerms()
    {
        RDFDescribeQuery query = RDFDescribeQuery.FromString(
            "PREFIX foaf: <http://xmlns.com/foaf/0.1/> DESCRIBE ?p foaf:Person WHERE { ?p a foaf:Person }");

        Assert.AreEqual(2, query.DescribeTerms.Count);
        Assert.IsTrue(query.DescribeTerms[0] is RDFVariable);
        Assert.IsTrue(query.DescribeTerms[1] is RDFResource);
        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseDescribeWithIriTermAndNoWhere()
    {
        RDFDescribeQuery query = RDFDescribeQuery.FromString("DESCRIBE <http://example.org/s>");

        //WHERE is optional in DESCRIBE: no pattern groups, a single IRI describe-term
        Assert.AreEqual(1, query.DescribeTerms.Count);
        Assert.IsTrue(query.DescribeTerms[0] is RDFResource);
        Assert.AreEqual(0, query.GetPatternGroups().Count());
    }

    [TestMethod]
    public void ShouldRoundTripDescribeWithIriTermAndNoWhere()
        => AssertDescribeQueryRoundTrips(RDFDescribeQuery.FromString("DESCRIBE <http://example.org/s>"));

    [TestMethod]
    public void ShouldParseDescribeWithLimitAndNoWhere()
    {
        RDFDescribeQuery query = RDFDescribeQuery.FromString("DESCRIBE <http://example.org/s> LIMIT 5");

        Assert.AreEqual(1, query.DescribeTerms.Count);
        Assert.AreEqual(0, query.GetPatternGroups().Count());
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFLimitModifier));
    }

    [TestMethod]
    public void ShouldThrowWhenDescribeFromStringFedASelectQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFDescribeQuery.FromString("SELECT * WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnDescribeWithDatasetClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFDescribeQuery.FromString("DESCRIBE ?s FROM <http://example.org/g> WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldParseDescribeWithOrderByModifier()
    {
        RDFDescribeQuery query = RDFDescribeQuery.FromString("DESCRIBE ?s WHERE { ?s ?p ?o } ORDER BY ?s");

        Assert.AreEqual(1, query.DescribeTerms.Count);
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFOrderByModifier));
    }

    [TestMethod]
    public void ShouldParseDescribeWithGroupByModifier()
    {
        RDFDescribeQuery query = RDFDescribeQuery.FromString("DESCRIBE ?s WHERE { ?s ?p ?o } GROUP BY ?s");

        Assert.AreEqual(1, query.DescribeTerms.Count);
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFGroupByModifier));
    }

    [TestMethod]
    public void ShouldThrowOnDescribeWithLiteralTerm()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFDescribeQuery.FromString("DESCRIBE \"hello\" WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnDescribeWithNoTermNorWildcard()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFDescribeQuery.FromString("DESCRIBE WHERE { }"));

    #endregion
}
