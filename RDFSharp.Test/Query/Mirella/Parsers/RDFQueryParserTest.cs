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

namespace RDFSharp.Test.Query.Mirella.Parsers;

/// <summary>
/// Unit tests for the phase F0 surface of RDFQueryParser: the lexing primitives (keyword runs, variables),
/// the prologue (BASE/PREFIX) and term resolution flowing through the autonomous SPARQL resolver.
/// </summary>
[TestClass]
public class RDFQueryParserTest
{
    #region Prologue
    [TestMethod]
    public void ShouldParseEmptyPrologueAndLeaveQueryFormUntouched()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("SELECT * WHERE { ?s ?p ?o }");
        RDFQueryParser.ParsePrologue(parserContext);

        //No BASE/PREFIX were present: the resolver stays empty and the query form keyword is left to be read
        Assert.AreEqual(string.Empty, parserContext.Resolver.BaseUri);
        Assert.AreEqual("SELECT", RDFQueryParser.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParseBaseDeclaration()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("BASE <http://example.org/base/> SELECT");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://example.org/base/", parserContext.Resolver.BaseUri);
        Assert.AreEqual("SELECT", RDFQueryParser.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParsePrefixDeclaration()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("PREFIX foaf: <http://xmlns.com/foaf/0.1/> SELECT");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://xmlns.com/foaf/0.1/", parserContext.Resolver.ResolveNamespace("foaf"));
        Assert.AreEqual("SELECT", RDFQueryParser.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParseDefaultNamespaceDeclaration()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("PREFIX : <http://example.org/> SELECT");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://example.org/", parserContext.Resolver.ResolveNamespace(string.Empty));
        Assert.AreEqual("SELECT", RDFQueryParser.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParseMultiplePrologueDeclarations()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext(
            "BASE <http://example.org/base/> PREFIX foaf: <http://xmlns.com/foaf/0.1/> PREFIX ex: <http://example.org/ns#> ASK");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://example.org/base/", parserContext.Resolver.BaseUri);
        Assert.AreEqual("http://xmlns.com/foaf/0.1/", parserContext.Resolver.ResolveNamespace("foaf"));
        Assert.AreEqual("http://example.org/ns#", parserContext.Resolver.ResolveNamespace("ex"));
        Assert.AreEqual("ASK", RDFQueryParser.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParsePrologueKeywordsCaseInsensitively()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext(
            "base <http://example.org/base/> prefix ex: <http://example.org/ns#> SELECT");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://example.org/base/", parserContext.Resolver.BaseUri);
        Assert.AreEqual("http://example.org/ns#", parserContext.Resolver.ResolveNamespace("ex"));
        Assert.AreEqual("SELECT", RDFQueryParser.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldThrowOnPrefixDeclarationWithoutColon()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("PREFIX foaf <http://xmlns.com/foaf/0.1/> SELECT");

        Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParser.ParsePrologue(parserContext));
    }
    #endregion

    #region Variables
    [TestMethod]
    public void ShouldParseVariableWithQuestionMarkSigil()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("?name");
        RDFVariable variable = RDFQueryParser.ParseVariable(parserContext);

        //RDFVariable normalizes the bare name to "?NAME"
        Assert.AreEqual("?NAME", variable.VariableName);
    }

    [TestMethod]
    public void ShouldParseVariableWithDollarSigil()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("$age");
        RDFVariable variable = RDFQueryParser.ParseVariable(parserContext);

        Assert.AreEqual("?AGE", variable.VariableName);
    }

    [TestMethod]
    public void ShouldStopVariableAtNonNameCharacter()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("?s ?p");
        RDFVariable firstVariable = RDFQueryParser.ParseVariable(parserContext);

        //Parsing must stop at the space, leaving the rest of the input available for the next variable
        Assert.AreEqual("?S", firstVariable.VariableName);
        RDFQueryParser.SkipWhitespace(parserContext);
        RDFVariable secondVariable = RDFQueryParser.ParseVariable(parserContext);
        Assert.AreEqual("?P", secondVariable.VariableName);
    }

    [TestMethod]
    public void ShouldThrowOnVariableWithoutSigil()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("name");

        Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParser.ParseVariable(parserContext));
    }

    [TestMethod]
    public void ShouldThrowOnVariableWithEmptyName()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("? ");

        Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParser.ParseVariable(parserContext));
    }
    #endregion

    #region Terms
    [TestMethod]
    public void ShouldParseTermAsAbsoluteIri()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("<http://example.org/subject>");
        RDFPatternMember term = RDFQueryParser.ParseTerm(parserContext);

        Assert.IsInstanceOfType<RDFResource>(term);
        Assert.AreEqual("http://example.org/subject", term.ToString());
    }

    [TestMethod]
    public void ShouldParseTermAsPrefixedNameUsingDeclaredPrefix()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("PREFIX foaf: <http://xmlns.com/foaf/0.1/> foaf:Person");
        RDFQueryParser.ParsePrologue(parserContext);
        RDFPatternMember term = RDFQueryParser.ParseTerm(parserContext);

        Assert.IsInstanceOfType<RDFResource>(term);
        Assert.AreEqual("http://xmlns.com/foaf/0.1/Person", term.ToString());
    }

    [TestMethod]
    public void ShouldParseTermAsPrefixedNameUsingRegisterLeniency()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("rdf:type");
        RDFPatternMember term = RDFQueryParser.ParseTerm(parserContext);

        //The well-known rdf prefix is auto-resolved even without a PREFIX directive
        Assert.IsInstanceOfType<RDFResource>(term);
        Assert.AreEqual(RDFVocabulary.RDF.TYPE.ToString(), term.ToString());
    }

    [TestMethod]
    public void ShouldParseTermAsPlainLiteral()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("\"hello world\"");
        RDFPatternMember term = RDFQueryParser.ParseTerm(parserContext);

        Assert.IsInstanceOfType<RDFPlainLiteral>(term);
        Assert.AreEqual("hello world", term.ToString());
    }
    #endregion

    #region SelectQuery (round-trip)
    /// <summary>
    /// Asserts that printing the given query, parsing the print back, and printing the result yields the very
    /// same text: the round-trip oracle that proves the parser reconstructs the object-model faithfully.
    /// </summary>
    private static void AssertSelectQueryRoundTrips(RDFSelectQuery originalQuery)
    {
        string printedQuery = originalQuery.ToString();
        RDFSelectQuery reparsedQuery = RDFSelectQuery.FromString(printedQuery);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(printedQuery), RDFTestUtilities.NormalizeEOL(reparsedQuery.ToString()));
    }

    [TestMethod]
    public void ShouldRoundTripSelectStarWithSingleAllVariablePattern()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectWithProjectionVariablesAndMultiplePatterns()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("person"), RDFVocabulary.RDF.TYPE, RDFVocabulary.FOAF.PERSON))
                .AddPattern(new RDFPattern(new RDFVariable("person"), RDFVocabulary.FOAF.NAME, new RDFVariable("name"))))
            .AddProjectionVariable(new RDFVariable("person"))
            .AddProjectionVariable(new RDFVariable("name"));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectDistinct()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddProjectionVariable(new RDFVariable("s"))
            .AddModifier(new RDFDistinctModifier());

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectWithLimitAndOffset()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddModifier(new RDFLimitModifier(10))
            .AddModifier(new RDFOffsetModifier(5));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectWithOrderByAscendingAndDescending()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("s"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("o"), RDFQueryEnums.RDFOrderByFlavors.DESC));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectWithLiteralObjects()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("hello")))
                .AddPattern(new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("ciao", "it")))
                .AddPattern(new RDFPattern(new RDFVariable("s"), RDFVocabulary.FOAF.AGE, new RDFTypedLiteral("42", RDFModelEnums.RDFDatatypes.XSD_INTEGER))));

        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripSelectWithMultiplePatternGroups()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o"))))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("o"), new RDFVariable("q"), new RDFVariable("z"))));

        AssertSelectQueryRoundTrips(query);
    }
    #endregion

    #region SelectQuery (structure)
    [TestMethod]
    public void ShouldParseSelectStarWithoutWhereKeyword()
    {
        //The WHERE keyword is optional in SPARQL: the group graph pattern braces are enough
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * { ?s ?p ?o }");

        Assert.AreEqual(0, query.ProjectionVars.Count);
        List<RDFPatternGroup> patternGroups = query.GetPatternGroups().ToList();
        Assert.AreEqual(1, patternGroups.Count);
        Assert.AreEqual(1, patternGroups[0].GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldParseSelectWithProjectionVariables()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?s ?o WHERE { ?s ?p ?o }");

        Assert.AreEqual(2, query.ProjectionVars.Count);
        Assert.IsTrue(query.ProjectionVars.Keys.Any(v => v.VariableName == "?S"));
        Assert.IsTrue(query.ProjectionVars.Keys.Any(v => v.VariableName == "?O"));
    }

    [TestMethod]
    public void ShouldParseVariablesWithDollarSigilInBody()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT $s WHERE { $s $p $o }");

        Assert.AreEqual(1, query.ProjectionVars.Count);
        RDFPattern pattern = query.GetPatternGroups().Single().GetPatterns().Single();
        Assert.AreEqual("?S", ((RDFVariable)pattern.Subject).VariableName);
        Assert.AreEqual("?P", ((RDFVariable)pattern.Predicate).VariableName);
        Assert.AreEqual("?O", ((RDFVariable)pattern.Object).VariableName);
    }

    [TestMethod]
    public void ShouldParseTheVerbAAsRdfType()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s a ?type }");

        RDFPattern pattern = query.GetPatternGroups().Single().GetPatterns().Single();
        Assert.IsInstanceOfType<RDFResource>(pattern.Predicate);
        Assert.AreEqual(RDFVocabulary.RDF.TYPE.ToString(), pattern.Predicate.ToString());
    }

    [TestMethod]
    public void ShouldParsePredicateObjectListWithSemicolon()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p1> ?o1 ; <http://example.org/p2> ?o2 }");

        List<RDFPattern> patterns = query.GetPatternGroups().Single().GetPatterns().ToList();
        Assert.AreEqual(2, patterns.Count);
        Assert.AreEqual("http://example.org/p1", patterns[0].Predicate.ToString());
        Assert.AreEqual("http://example.org/p2", patterns[1].Predicate.ToString());
        //Both predicate-object groups share the same subject
        Assert.AreEqual("?S", ((RDFVariable)patterns[0].Subject).VariableName);
        Assert.AreEqual("?S", ((RDFVariable)patterns[1].Subject).VariableName);
    }

    [TestMethod]
    public void ShouldParseObjectListWithComma()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s <http://example.org/p> ?o1 , ?o2 }");

        List<RDFPattern> patterns = query.GetPatternGroups().Single().GetPatterns().ToList();
        Assert.AreEqual(2, patterns.Count);
        //Both objects share the same subject and predicate
        Assert.AreEqual("http://example.org/p", patterns[0].Predicate.ToString());
        Assert.AreEqual("http://example.org/p", patterns[1].Predicate.ToString());
        Assert.AreEqual("?O1", ((RDFVariable)patterns[0].Object).VariableName);
        Assert.AreEqual("?O2", ((RDFVariable)patterns[1].Object).VariableName);
    }

    [TestMethod]
    public void ShouldParseKeywordsCaseInsensitively()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("select distinct ?s where { ?s ?p ?o } limit 5");

        Assert.AreEqual(1, query.ProjectionVars.Count);
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFDistinctModifier));
        Assert.IsTrue(query.GetModifiers().Any(m => m is RDFLimitModifier));
    }

    [TestMethod]
    public void ShouldTreatReducedAsNoOp()
    {
        //REDUCED is ratified leniency: it is recognized but produces no modifier
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT REDUCED ?s WHERE { ?s ?p ?o }");

        Assert.AreEqual(1, query.ProjectionVars.Count);
        Assert.IsFalse(query.GetModifiers().Any(m => m is RDFDistinctModifier));
    }

    [TestMethod]
    public void ShouldDefaultBareOrderByVariableToAscending()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } ORDER BY ?s");

        RDFOrderByModifier orderBy = query.GetModifiers().OfType<RDFOrderByModifier>().Single();
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.ASC, orderBy.OrderByFlavor);
        Assert.AreEqual("?S", orderBy.Variable.VariableName);
    }

    [TestMethod]
    public void ShouldParseOrderByWithAscAndDescDirections()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } ORDER BY ASC(?s) DESC(?o)");

        List<RDFOrderByModifier> orderBys = query.GetModifiers().OfType<RDFOrderByModifier>().ToList();
        Assert.AreEqual(2, orderBys.Count);
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.ASC, orderBys[0].OrderByFlavor);
        Assert.AreEqual(RDFQueryEnums.RDFOrderByFlavors.DESC, orderBys[1].OrderByFlavor);
    }

    [TestMethod]
    public void ShouldIgnoreCommentsWhileParsing()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } # trailing comment");

        Assert.AreEqual(1, query.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldReattachDeclaredPrefixesToParsedQuery()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("PREFIX ex: <http://example.org/> SELECT * WHERE { ?s ex:knows ?o }");

        //The PREFIX from the prologue must be carried onto the query so it re-serializes its prologue
        Assert.IsTrue(query.GetPrefixes().Any(p => p.NamespacePrefix == "ex"));
        RDFPattern pattern = query.GetPatternGroups().Single().GetPatterns().Single();
        Assert.AreEqual("http://example.org/knows", pattern.Predicate.ToString());
    }
    #endregion

    #region SelectQuery (errors)
    [TestMethod]
    public void ShouldThrowOnNullOrEmptyQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("   "));

    [TestMethod]
    public void ShouldThrowOnEmptyProjection()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnProjectionExpressionUntilSupported()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT (?s AS ?x) WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnUnclosedWhereClause()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o "));

    [TestMethod]
    public void ShouldThrowOnMissingByAfterOrder()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o } ORDER ?s"));

    [TestMethod]
    public void ShouldThrowOnLiteralInSubjectPosition()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFSelectQuery.FromString("SELECT * WHERE { \"lit\" ?p ?o }"));
    #endregion

    #region GraphPatternAlgebra (F2a round-trips)
    //Helper: PatternGroup with a single triple ?varS <predUri> ?varO
    private static RDFPatternGroup MakePG(string varS, string predUri, string varO)
        => new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable(varS), new RDFResource(predUri), new RDFVariable(varO)));

    [TestMethod]
    public void ShouldRoundTripUnion()
    {
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgB));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripUnionChain()
    {
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFPatternGroup pgC = MakePG("s", "http://example.org/p3", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgB).Union(pgC));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripMinus()
    {
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Minus(pgB));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripOptional()
    {
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "name");
        pgB.Optional();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(pgA)
            .AddPatternGroup(pgB);
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripUnionOfMinus()
    {
        //A ∪ (B ∖ C)
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFPatternGroup pgC = MakePG("s", "http://example.org/p3", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgB.Minus(pgC)));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripMinusOfUnion()
    {
        //(A ∪ B) ∖ C
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFPatternGroup pgC = MakePG("s", "http://example.org/p3", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(pgA.Union(pgB).Minus(pgC));
        AssertSelectQueryRoundTrips(query);
    }

    [TestMethod]
    public void ShouldRoundTripOptionalOperator()
    {
        //OPTIONAL (A ∪ B)
        RDFPatternGroup pgA = MakePG("s", "http://example.org/p1", "o");
        RDFPatternGroup pgB = MakePG("s", "http://example.org/p2", "o");
        RDFOperatorQueryMember op = pgA.Union(pgB);
        op.Optional();
        RDFPatternGroup pgC = MakePG("s", "http://example.org/p3", "o");
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(pgC)
            .AddOperator(op);
        AssertSelectQueryRoundTrips(query);
    }
    #endregion

    #region GraphPatternAlgebra (F2a spec syntax)
    [TestMethod]
    public void ShouldParseUnionFromSpecSyntax()
    {
        //SPARQL spec: GroupOrUnionGraphPattern ::= GroupGraphPattern ('UNION' GroupGraphPattern)*
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } UNION { ?s <http://example.org/p2> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFOperatorQueryMember op = (RDFOperatorQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.LeftOperand);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldParseUnionChainFromSpecSyntax()
    {
        //Three-way UNION: left-associative Union(Union(A,B),C)
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } UNION { ?s <http://example.org/p2> ?o } UNION { ?s <http://example.org/p3> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFOperatorQueryMember op = (RDFOperatorQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
        //Left operand is itself a Union(A,B)
        Assert.IsInstanceOfType<RDFOperatorQueryMember>(op.LeftOperand);
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union,
            ((RDFOperatorQueryMember)op.LeftOperand).OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldParseMinusFromSpecSyntax()
    {
        //SPARQL spec: MinusGraphPattern ::= 'MINUS' GroupGraphPattern
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } MINUS { ?s <http://example.org/p2> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFOperatorQueryMember op = (RDFOperatorQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, op.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.LeftOperand);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldParseOptionalFromSpecSyntax()
    {
        //SPARQL spec: OptionalGraphPattern ::= 'OPTIONAL' GroupGraphPattern
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } OPTIONAL { ?s <http://example.org/name> ?name } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(2, evaluable.Count);
        RDFPatternGroup main = (RDFPatternGroup)evaluable[0];
        RDFPatternGroup optional = (RDFPatternGroup)evaluable[1];
        Assert.IsFalse(main.IsOptional);
        Assert.IsTrue(optional.IsOptional);
    }

    [TestMethod]
    public void ShouldParseInlineTriplesWithOptional()
    {
        //Inline BGP followed by OPTIONAL — TriplesBlock must stop at the keyword
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s <http://example.org/p1> ?o OPTIONAL { ?s <http://example.org/name> ?name } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(2, evaluable.Count);
        Assert.IsInstanceOfType<RDFPatternGroup>(evaluable[0]);
        RDFPatternGroup optional = (RDFPatternGroup)evaluable[1];
        Assert.IsTrue(optional.IsOptional);
    }

    [TestMethod]
    public void ShouldParseInlineTriplesWithMinus()
    {
        //Inline BGP followed by MINUS — the BGP becomes the left operand of the tree node
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { ?s <http://example.org/p1> ?o . MINUS { ?s <http://example.org/p2> ?o2 } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFOperatorQueryMember op = (RDFOperatorQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, op.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.LeftOperand);
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldParseMinusWithUnionRightOperand()
    {
        //A ∖ (B ∪ C) — right operand of MINUS is a union group per the spec
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } MINUS { { ?s <http://example.org/p2> ?o } UNION { ?s <http://example.org/p3> ?o } } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFOperatorQueryMember minus = (RDFOperatorQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, minus.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(minus.LeftOperand);
        //Right operand is Union(B,C)
        RDFOperatorQueryMember rightUnion = (RDFOperatorQueryMember)minus.RightOperand;
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, rightUnion.OperatorType);
    }

    [TestMethod]
    public void ShouldParseUnionOfMinus()
    {
        //A ∪ (B ∖ C) — SPARQL-compliant: Union left-operand A, right-operand is a Minus group
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } UNION { { ?s <http://example.org/p2> ?o } MINUS { ?s <http://example.org/p3> ?o } } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFOperatorQueryMember union = (RDFOperatorQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, union.OperatorType);
        Assert.IsInstanceOfType<RDFPatternGroup>(union.LeftOperand);
        //Right operand is Minus(B,C)
        RDFOperatorQueryMember rightMinus = (RDFOperatorQueryMember)union.RightOperand;
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, rightMinus.OperatorType);
    }

    [TestMethod]
    public void ShouldParseMultiGroupMinusWrapsAccumulator()
    {
        //{ {A} {B} MINUS {C} } — MINUS binds the whole accumulated left side: Minus(subquery(A,B), C)
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { { ?s <http://example.org/p1> ?o } { ?s <http://example.org/p2> ?x } MINUS { ?s <http://example.org/p3> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        RDFOperatorQueryMember op = (RDFOperatorQueryMember)evaluable[0];
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, op.OperatorType);
        //Left operand must be a subquery wrapping A and B
        Assert.IsInstanceOfType<RDFSelectQuery>(op.LeftOperand);
        RDFSelectQuery leftSubQuery = (RDFSelectQuery)op.LeftOperand;
        Assert.AreEqual(2, leftSubQuery.GetPatternGroups().Count());
        Assert.IsInstanceOfType<RDFPatternGroup>(op.RightOperand);
    }

    [TestMethod]
    public void ShouldDropLeadingMinusResiliently()
    {
        //A leading MINUS with no left side: the right operand is kept as a plain element
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { MINUS { ?s <http://example.org/p> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        Assert.IsInstanceOfType<RDFPatternGroup>(evaluable[0]);
    }

    [TestMethod]
    public void ShouldDropStrayUnionResiliently()
    {
        //A stray UNION at the top level (no left GroupGraphPattern): it is dropped and parsing continues
        RDFSelectQuery query = RDFSelectQuery.FromString(
            "SELECT * WHERE { UNION { ?s <http://example.org/p> ?o } }");

        List<RDFQueryMember> evaluable = query.GetEvaluableQueryMembers().ToList();
        Assert.AreEqual(1, evaluable.Count);
        Assert.IsInstanceOfType<RDFPatternGroup>(evaluable[0]);
    }

    [TestMethod]
    public void ShouldThrowOnUnsupportedKeywordFilter()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { FILTER(?x > 0) }"));

    [TestMethod]
    public void ShouldThrowOnUnsupportedKeywordBind()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { BIND(1 AS ?x) }"));
    #endregion

    #region GraphPatternAlgebra (F2a end-to-end execution)
    //These tests close the loop that the round-trip and structural tests leave open: they parse a SPARQL
    //string, EXECUTE it against a real in-memory graph, and assert on the actual RESULT SET. This is what
    //actually proves that the algebra TREE the parser builds is evaluated by the engine with SPARQL-compliant
    //semantics — in particular that mixed nested operators bind as the spec requires (e.g. A ∪ (B ∖ C)) and
    //not with the fixed-precedence flattening of the old model (which computed (A ∪ B) ∖ C).

    //Membership dataset shared by the execution tests below.
    //Three predicates mark membership in the synthetic sets A, B, C:
    //  A = {x1,x2,x5}   (via <http://ex/inA>)
    //  B = {x2,x3}      (via <http://ex/inB>)
    //  C = {x3,x4,x5}   (via <http://ex/inC>)
    //The subjects deliberately overlap so that union/minus interplay is observable in the result set.
    private static RDFGraph BuildMembershipDataset()
    {
        RDFResource predicateInA = new RDFResource("http://ex/inA");
        RDFResource predicateInB = new RDFResource("http://ex/inB");
        RDFResource predicateInC = new RDFResource("http://ex/inC");
        RDFResource membershipFlag = new RDFResource("http://ex/yes");

        RDFGraph dataset = new RDFGraph();
        foreach (string subjectLocalName in new[] { "x1", "x2", "x5" })
            dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/" + subjectLocalName), predicateInA, membershipFlag));
        foreach (string subjectLocalName in new[] { "x2", "x3" })
            dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/" + subjectLocalName), predicateInB, membershipFlag));
        foreach (string subjectLocalName in new[] { "x3", "x4", "x5" })
            dataset.AddTriple(new RDFTriple(new RDFResource("http://ex/" + subjectLocalName), predicateInC, membershipFlag));
        return dataset;
    }

    //Helper: run the query against the dataset and return the bound values of ?s, sorted, including duplicates
    //(SPARQL bag semantics — we deliberately do NOT de-duplicate, so multiset cardinality is observable).
    private static string[] ExecuteAndCollectSubjects(string sparqlQuery, RDFGraph dataset)
    {
        DataTable results = RDFSelectQuery.FromString(sparqlQuery).ApplyToGraph(dataset).SelectResults;
        return results.AsEnumerable()
                      .Select(row => row.Field<string>("?s"))
                      .OrderBy(subjectValue => subjectValue, System.StringComparer.Ordinal)
                      .ToArray();
    }

    [TestMethod]
    public void ShouldExecuteUnionOfMinusWithSparqlCompliantBindingAndBagSemantics()
    {
        //Spec grouping: A ∪ (B ∖ C). With B ∖ C = {x2,x3} ∖ {x3,x4,x5} = {x2}, the bag union with
        //A = {x1,x2,x5} yields {x1,x2,x2,x5} (x2 appears once from A and once from B∖C — bag union keeps both).
        //Crucially x5 SURVIVES, which would be impossible under the old fixed-precedence (A ∪ B) ∖ C = {x1,x2}.
        RDFGraph dataset = BuildMembershipDataset();
        string[] subjects = ExecuteAndCollectSubjects(
            "SELECT ?s WHERE { { ?s <http://ex/inA> ?f } UNION { { ?s <http://ex/inB> ?f } MINUS { ?s <http://ex/inC> ?f } } }",
            dataset);

        CollectionAssert.AreEqual(
            new[] { "http://ex/x1", "http://ex/x2", "http://ex/x2", "http://ex/x5" }, subjects,
            "Expected SPARQL-compliant A ∪ (B ∖ C) in bag semantics; actual: " + string.Join(",", subjects));
    }

    [TestMethod]
    public void ShouldExecuteMinusOfUnionWithSparqlCompliantBinding()
    {
        //Spec grouping: (A ∪ B) ∖ C. With A ∪ B = {x1,x2,x2,x3,x5} and C = {x3,x4,x5},
        //removing every left row whose ?s is in C drops x3 and x5, leaving {x1,x2,x2}.
        //This is the DUAL of the previous test and must differ from it, proving the tree honors the braces.
        RDFGraph dataset = BuildMembershipDataset();
        string[] subjects = ExecuteAndCollectSubjects(
            "SELECT ?s WHERE { { { ?s <http://ex/inA> ?f } UNION { ?s <http://ex/inB> ?f } } MINUS { ?s <http://ex/inC> ?f } }",
            dataset);

        CollectionAssert.AreEqual(
            new[] { "http://ex/x1", "http://ex/x2", "http://ex/x2" }, subjects,
            "Expected SPARQL-compliant (A ∪ B) ∖ C; actual: " + string.Join(",", subjects));
    }

    [TestMethod]
    public void ShouldExecuteOptionalKeepingLeftRowsWhenRightIsAbsent()
    {
        //OPTIONAL must preserve every left (A) row even when the right (C) pattern does not match, leaving ?c
        //unbound for those rows. A = {x1,x2,x5}; only x5 is also in C, so x1 and x2 keep ?c UNBOUND.
        RDFGraph dataset = BuildMembershipDataset();
        DataTable results = RDFSelectQuery.FromString(
            "SELECT ?s ?c WHERE { ?s <http://ex/inA> ?f OPTIONAL { ?s <http://ex/inC> ?c } }")
            .ApplyToGraph(dataset).SelectResults;

        //All three A-subjects survive the left join
        string[] subjects = results.AsEnumerable()
            .Select(row => row.Field<string>("?s"))
            .OrderBy(subjectValue => subjectValue, System.StringComparer.Ordinal).ToArray();
        CollectionAssert.AreEqual(new[] { "http://ex/x1", "http://ex/x2", "http://ex/x5" }, subjects);

        //x5 has ?c bound (it is in C); x1 and x2 have ?c UNBOUND (left-join padding)
        int boundOptionalCount = results.AsEnumerable().Count(row => row.Field<string>("?c") != null);
        Assert.AreEqual(1, boundOptionalCount, "Only x5 should carry a bound ?c value");
    }
    #endregion
}

/// <summary>
/// Unit tests for RDFSPARQLTermResolver, the autonomous base-IRI/prefix resolver that the SPARQL
/// parser feeds to the reused Turtle term-parsers. These tests exercise the resolver in isolation,
/// without driving the parser, so its accumulation and resolution rules are pinned down independently.
/// </summary>
[TestClass]
public class RDFSPARQLTermResolverTest
{
    #region BaseUri
    [TestMethod]
    public void ShouldReturnEmptyBaseUriWhenNoBaseDeclared()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();

        //A query without a BASE directive must expose an empty base IRI (not null)
        Assert.AreEqual(string.Empty, resolver.BaseUri);
    }

    [TestMethod]
    public void ShouldRecordDeclaredBaseIri()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();
        resolver.SetBaseIri("http://example.org/base/");

        Assert.AreEqual("http://example.org/base/", resolver.BaseUri);
    }

    [TestMethod]
    public void ShouldOverrideBaseIriWithLaterDeclaration()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();
        resolver.SetBaseIri("http://example.org/first/");
        resolver.SetBaseIri("http://example.org/second/");

        //SPARQL semantics: the last BASE declaration wins
        Assert.AreEqual("http://example.org/second/", resolver.BaseUri);
    }

    [TestMethod]
    public void ShouldNormalizeNullBaseIriToEmptyString()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();
        resolver.SetBaseIri(null);

        Assert.AreEqual(string.Empty, resolver.BaseUri);
    }
    #endregion

    #region ResolveNamespace
    [TestMethod]
    public void ShouldResolveDeclaredPrefix()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();
        resolver.RegisterPrefix("ex", "http://example.org/");

        Assert.AreEqual("http://example.org/", resolver.ResolveNamespace("ex"));
    }

    [TestMethod]
    public void ShouldResolveDeclaredDefaultNamespaceForEmptyPrefix()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();
        resolver.RegisterPrefix(string.Empty, "http://example.org/default/");

        Assert.AreEqual("http://example.org/default/", resolver.ResolveNamespace(string.Empty));
    }

    [TestMethod]
    public void ShouldTreatNullPrefixAsDefaultNamespace()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();
        resolver.RegisterPrefix(null, "http://example.org/default/");

        //A null prefix label is normalized to the empty-string key used for the default namespace
        Assert.AreEqual("http://example.org/default/", resolver.ResolveNamespace(null));
    }

    [TestMethod]
    public void ShouldOverrideDeclaredPrefixWithLaterDeclaration()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();
        resolver.RegisterPrefix("ex", "http://example.org/first/");
        resolver.RegisterPrefix("ex", "http://example.org/second/");

        //SPARQL semantics: the last PREFIX declaration of the same label wins
        Assert.AreEqual("http://example.org/second/", resolver.ResolveNamespace("ex"));
    }

    [TestMethod]
    public void ShouldFallBackToRegisterForWellKnownPrefix()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();

        //Leniency: a well-known prefix (rdf) is auto-resolved through the register even without a PREFIX directive
        Assert.AreEqual(RDFVocabulary.RDF.BASE_URI, resolver.ResolveNamespace("rdf"));
    }

    [TestMethod]
    public void ShouldPreferDeclaredPrefixOverRegister()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();
        //Re-bind the well-known "rdf" prefix to a custom namespace within this query's prologue
        resolver.RegisterPrefix("rdf", "http://example.org/custom-rdf/");

        //The query's own declaration must take precedence over the register leniency
        Assert.AreEqual("http://example.org/custom-rdf/", resolver.ResolveNamespace("rdf"));
    }

    [TestMethod]
    public void ShouldReturnNullForUnknownPrefix()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();

        Assert.IsNull(resolver.ResolveNamespace("thisPrefixWasNeverDeclared"));
    }

    [TestMethod]
    public void ShouldReturnNullForUndeclaredDefaultNamespace()
    {
        RDFSPARQLTermResolver resolver = new RDFSPARQLTermResolver();

        //The default namespace is never auto-resolved through the register: it must be explicitly declared
        Assert.IsNull(resolver.ResolveNamespace(string.Empty));
    }
    #endregion
}