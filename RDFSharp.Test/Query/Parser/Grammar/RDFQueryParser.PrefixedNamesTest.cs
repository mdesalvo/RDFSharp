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
/// Cross-cutting unit tests for ABBREVIATED IRIs (prefixed names) — the prologue's PREFIX/BASE resolution applied
/// throughout the grammar: triple positions, the 'a' verb, collections and blank-node lists, property paths,
/// FILTER operands, GRAPH/VALUES/BIND, the four query forms and the SPARQL UPDATE operations. They exercise the
/// resolver (explicit prefixes, the default namespace, well-known-prefix leniency, BASE-relative resolution,
/// last-declaration-wins) and check that declared prefixes survive serialization.
/// </summary>
public partial class RDFQueryParserTest
{
    #region PrefixedNames

    //Re-serializing the parsed query and parsing that again must yield a stable print (idempotent round-trip).
    private static void AssertReserializesStably(RDFQuery parsedQuery)
    {
        string firstPrint = parsedQuery.ToString();
        RDFQuery reparsed = RDFQueryParserFactory.ParseQuery(firstPrint);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(firstPrint), RDFTestUtilities.NormalizeEOL(reparsed.ToString()));
    }

    #region Prologue resolution

    [TestMethod]
    public void ShouldResolvePrefixedNamesInEveryTriplePosition()
    {
        //Each pattern keeps a variable so it is retained (a pattern group drops fully-ground triples), while the
        //prefixed names in subject/predicate/object positions are all resolved against the declared prefix
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ?subj ex:p ex:o . ex:s ex:q ?obj }");

        RDFPattern firstPattern = PatternsOf(query).First(p => p.Predicate.ToString() == "http://example.org/p");
        RDFPattern secondPattern = PatternsOf(query).First(p => p.Predicate.ToString() == "http://example.org/q");
        Assert.AreEqual("http://example.org/o", firstPattern.Object.ToString());
        Assert.AreEqual("http://example.org/s", secondPattern.Subject.ToString());
    }

    [TestMethod]
    public void ShouldResolveDefaultNamespacePrefixedNames()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX : <http://example.org/> SELECT * WHERE { :s :p ?o }");

        Assert.AreEqual("http://example.org/p", PatternsOf(query).Single().Predicate.ToString());
    }

    [TestMethod]
    public void ShouldResolveEmptyLocalPartPrefixedName()
    {
        //'ex:' (empty local part) resolves to the bare namespace IRI
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ?s ex: ?o }");

        Assert.AreEqual("http://example.org/", PatternsOf(query).Single().Predicate.ToString());
    }

    [TestMethod]
    public void ShouldResolveMultipleDeclaredPrefixes()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX a: <http://a.org/> PREFIX b: <http://b.org/> SELECT * WHERE { a:s b:p ?o }");

        RDFPattern pattern = PatternsOf(query).Single();
        Assert.AreEqual("http://a.org/s", pattern.Subject.ToString());
        Assert.AreEqual("http://b.org/p", pattern.Predicate.ToString());
    }

    [TestMethod]
    public void ShouldHonourLastDeclarationWinsForRepeatedPrefix()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://first.org/> PREFIX ex: <http://second.org/> SELECT * WHERE { ?s ex:p ?o }");

        Assert.AreEqual("http://second.org/p", PatternsOf(query).Single().Predicate.ToString());
    }

    [TestMethod]
    public void ShouldResolveBaseRelativeIriInPrefixDeclaration()
    {
        //BASE makes the relative IRI of a PREFIX declaration absolute
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "BASE <http://example.org/> PREFIX ex: <vocab/> SELECT * WHERE { ?s ex:p ?o }");

        Assert.AreEqual("http://example.org/vocab/p", PatternsOf(query).Single().Predicate.ToString());
    }

    [TestMethod]
    public void ShouldResolveRelativeIriReferenceAgainstBase()
    {
        //A BASE directive makes a bare relative IRI reference '<test>' absolute
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "BASE <http://example.org/> SELECT * WHERE { ?s <test> ?o }");

        Assert.AreEqual("http://example.org/test", PatternsOf(query).Single().Predicate.ToString());
    }

    [TestMethod]
    public void ShouldResolveDefaultPrefixedNameAgainstBase()
    {
        //A relative default-namespace declaration is itself resolved against BASE, so ':test' becomes absolute
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "BASE <http://example.org/> PREFIX : <ns/> SELECT * WHERE { ?s :test ?o }");

        Assert.AreEqual("http://example.org/ns/test", PatternsOf(query).Single().Predicate.ToString());
    }

    [TestMethod]
    public void ShouldResolveWellKnownPrefixWithoutDeclaration()
    {
        //'foaf' is a well-known registered prefix: the leniency resolves it with no PREFIX declaration
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s a foaf:Person }");

        Assert.AreEqual(RDFVocabulary.FOAF.PERSON.ToString(), PatternsOf(query).Single().Object.ToString());
    }

    [TestMethod]
    public void ShouldResolveTheAVerbToRdfType()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s a ?type }");

        Assert.AreEqual(RDFVocabulary.RDF.TYPE.ToString(), PatternsOf(query).Single().Predicate.ToString());
    }

    [TestMethod]
    public void ShouldThrowOnUndeclaredPrefix()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s nope:p ?o }"));

    [TestMethod]
    public void ShouldThrowOnNonRegisteredUndeclaredPrefix()
    {
        //'ex' is NOT a well-known registered prefix, so without a PREFIX declaration it cannot be resolved:
        //the query must fail rather than silently inventing a namespace
        Assert.IsNull(RDFNamespaceRegister.GetByPrefix("ex"));
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ex:resource }"));
    }

    [TestMethod]
    public void ShouldAcceptBnodeConventionPrefixWithoutDeclaration()
    {
        //'bnode:' is RDFSharp's internal blank-node scheme: it is the ONLY undeclared, non-registered prefix that
        //resolves, so a query may reference a blank node by its 'bnode:label' IRI (it re-serializes as '_:label')
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p bnode:resource }");

        RDFPattern pattern = PatternsOf(query).Single();
        Assert.IsInstanceOfType(pattern.Object, typeof(RDFResource));
        Assert.IsTrue(((RDFResource)pattern.Object).IsBlank);
    }

    [TestMethod]
    public void ShouldThrowOnUndeclaredDefaultNamespace()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s :p ?o }"));

    [TestMethod]
    public void ShouldPreserveDeclaredPrefixAcrossSerialization()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ?s ex:p ex:o }");

        Assert.AreEqual(1, query.GetPrefixes().Count);
        AssertReserializesStably(query);
    }

    #endregion

    #region Triples / lists with prefixed names

    [TestMethod]
    public void ShouldResolvePrefixedNamesInPredicateObjectAndObjectLists()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ?s a ex:Type ; ex:p ex:o , ex:o2 }");

        //'?s a ex:Type' + '?s ex:p ex:o' + '?s ex:p ex:o2' = three patterns (subject variable keeps them)
        Assert.AreEqual(3, PatternsOf(query).Count);
    }

    [TestMethod]
    public void ShouldResolvePrefixedNamesInsideCollection()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ex:s ex:list ( ex:a ex:b ) }");

        //The two-item collection desugars to rdf:first/rdf:rest patterns referencing the prefixed items
        Assert.IsTrue(PatternsOf(query).Any(p => p.Object.ToString() == "http://example.org/a"));
        Assert.IsTrue(PatternsOf(query).Any(p => p.Object.ToString() == "http://example.org/b"));
    }

    [TestMethod]
    public void ShouldResolvePrefixedNamesInsideBlankNodePropertyList()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ?s ex:p [ ex:q ex:o ] }");

        Assert.IsTrue(PatternsOf(query).Any(p => p.Predicate.ToString() == "http://example.org/q"
                                              && p.Object.ToString() == "http://example.org/o"));
    }

    [TestMethod]
    public void ShouldRoundTripGraphWithPrefixedContextAndTriples()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { GRAPH ex:g { ?s ex:p ex:o } }");

        AssertReserializesStably(query);
    }

    #endregion

    #region Property paths with prefixed names

    [TestMethod]
    [DataRow("foaf:knows/foaf:name")]
    [DataRow("foaf:knows|foaf:account")]
    [DataRow("^foaf:knows")]
    [DataRow("foaf:knows+")]
    [DataRow("foaf:knows*")]
    [DataRow("foaf:knows?")]
    public void ShouldParsePropertyPathWithPrefixedPredicates(string path)
    {
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s {path} ?o }}");
        Assert.IsNotNull(query);
        AssertReserializesStably(query);
    }

    #endregion

    #region FILTER / VALUES / BIND with prefixed names

    [TestMethod]
    public void ShouldParseSameTermFilterWithPrefixedIri()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p ?o FILTER(SAMETERM(?o, foaf:Person)) }");

        Assert.IsInstanceOfType(((RDFExpressionFilter)SingleFilterOf(query)).Expression, typeof(RDFSameTermExpression));
    }

    [TestMethod]
    public void ShouldParseValuesWithPrefixedIris()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ?s ?p ?o VALUES ?o { ex:a ex:b } }");

        AssertReserializesStably(query);
    }

    [TestMethod]
    public void ShouldParseBindWithPrefixedIri()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT * WHERE { ?s ?p ?o BIND(ex:x AS ?v) }");

        AssertReserializesStably(query);
    }

    [TestMethod]
    public void ShouldParsePrefixedTypedLiteralDatatype()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s ?p \"42\"^^xsd:integer }");

        Assert.AreEqual($"42^^{RDFVocabulary.XSD.INTEGER}", PatternsOf(query).Single().Object.ToString());
    }

    #endregion

    #region Query forms with prefixed names

    [TestMethod]
    public void ShouldParseAskWithPrefixedNames()
    {
        RDFAskQuery query = RDFAskQuery.FromString(
            "PREFIX ex: <http://example.org/> ASK { ?s ex:p ex:o }");

        Assert.AreEqual(1, query.GetPrefixes().Count);
        AssertReserializesStably(query);
    }

    [TestMethod]
    public void ShouldParseConstructWithPrefixedTemplateAndWhere()
    {
        RDFConstructQuery query = RDFConstructQuery.FromString(
            "PREFIX ex: <http://example.org/> CONSTRUCT { ?s ex:knows ?o } WHERE { ?s ex:friend ?o }");

        Assert.AreEqual(1, query.Templates.Count);
        AssertReserializesStably(query);
    }

    [TestMethod]
    public void ShouldParseDescribeWithPrefixedTerm()
    {
        RDFDescribeQuery query = RDFDescribeQuery.FromString(
            "PREFIX ex: <http://example.org/> DESCRIBE ex:resource WHERE { ex:resource ?p ?o }");

        Assert.AreEqual(1, query.DescribeTerms.Count);
        Assert.AreEqual("http://example.org/resource", query.DescribeTerms.Single().ToString());
    }

    #endregion

    #region SPARQL UPDATE with prefixed names

    [TestMethod]
    public void ShouldParseInsertDataWithPrefixedQuads()
    {
        RDFInsertDataOperation operation = RDFInsertDataOperation.FromString(
            "PREFIX ex: <http://example.org/> INSERT DATA { ex:s ex:p ex:o }");

        Assert.AreEqual(1, operation.InsertTemplates.Count);
        Assert.AreEqual("http://example.org/p", operation.InsertTemplates.Single().Predicate.ToString());
    }

    [TestMethod]
    public void ShouldParseDeleteWhereWithPrefixedNames()
    {
        RDFDeleteWhereOperation operation = RDFDeleteWhereOperation.FromString(
            "PREFIX ex: <http://example.org/> DELETE WHERE { ?s ex:p ?o }");

        Assert.AreEqual(1, operation.DeleteTemplates.Count);
    }

    [TestMethod]
    public void ShouldParseClearWithPrefixedGraphIri()
    {
        RDFClearOperation operation = RDFClearOperation.FromString(
            "PREFIX ex: <http://example.org/> CLEAR GRAPH ex:g");

        Assert.AreEqual("http://example.org/g", operation.FromContext.ToString());
    }

    [TestMethod]
    public void ShouldParseLoadWithPrefixedSourceAndTarget()
    {
        RDFLoadOperation operation = RDFLoadOperation.FromString(
            "PREFIX ex: <http://example.org/> LOAD ex:doc INTO GRAPH ex:g");

        Assert.AreEqual("http://example.org/doc", operation.FromContext.ToString());
        Assert.AreEqual("http://example.org/g", operation.ToContext.ToString());
    }

    #endregion

    #endregion
}
