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
/// Unit tests for the built-in-call dispatch of RDFQueryParser (the SPARQL 1.1 built-in functions).
/// </summary>
public partial class RDFQueryParserTest
{
    #region BuiltInCall

    [TestMethod]
    public void ShouldParseBareBuiltInConstraintWithoutBrackets()
    {
        //A BuiltInCall is a valid Constraint on its own (no surrounding parentheses)
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER REGEX(?o, \"^a\", \"i\") }");

        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        Assert.IsInstanceOfType(expressionFilter.Expression, typeof(RDFRegexExpression));
    }

    [TestMethod]
    public void ShouldParseConcatFoldedLeftAssociative()
    {
        //CONCAT(a, b, c) folds into CONCAT(CONCAT(a, b), c)
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(CONCAT(?o, \"-\", \"x\") = \"y\") }");

        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        RDFComparisonExpression comparison = (RDFComparisonExpression)expressionFilter.Expression;
        RDFConcatExpression outerConcat = (RDFConcatExpression)comparison.LeftArgument;
        Assert.IsInstanceOfType(outerConcat.LeftArgument, typeof(RDFConcatExpression));
    }

    [TestMethod]
    public void ShouldParseSameTermBuiltIn()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(SAMETERM(?s, ?o)) }");

        RDFExpressionFilter expressionFilter = (RDFExpressionFilter)SingleFilterOf(query);
        Assert.IsInstanceOfType(expressionFilter.Expression, typeof(RDFSameTermExpression));
    }

    [TestMethod]
    public void ShouldThrowOnUnknownBuiltIn()
    {
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(FOOBAR(?o)) }"));
    }

    /// <summary>
    /// Exercises EVERY representable SPARQL 1.1 built-in dispatched by ParseBuiltInCall, each inside a computed
    /// projection — the one context that accepts every built-in regardless of its return type — so the whole
    /// built-in switch (nullary/unary/binary/variadic/ternary and the literal-argument ones) is covered and
    /// pinned. The assertion is "parses into a query": the built-in's switch case is reached and wired. This also
    /// guards the lexer's function-name tokenization for the digit/underscore names (MD5, SHA256, ENCODE_FOR_URI).
    /// </summary>
    [TestMethod]
    // Nullary
    [DataRow("NOW()")]
    [DataRow("RAND()")]
    [DataRow("UUID()")]
    [DataRow("STRUUID()")]
    [DataRow("BNODE()")]
    // Unary
    [DataRow("STR(?o)")]
    [DataRow("LANG(?o)")]
    [DataRow("DATATYPE(?o)")]
    [DataRow("ABS(?o)")]
    [DataRow("CEIL(?o)")]
    [DataRow("FLOOR(?o)")]
    [DataRow("ROUND(?o)")]
    [DataRow("STRLEN(?o)")]
    [DataRow("UCASE(?o)")]
    [DataRow("LCASE(?o)")]
    [DataRow("ENCODE_FOR_URI(?o)")]
    [DataRow("MD5(?o)")]
    [DataRow("SHA1(?o)")]
    [DataRow("SHA256(?o)")]
    [DataRow("SHA384(?o)")]
    [DataRow("SHA512(?o)")]
    [DataRow("YEAR(?o)")]
    [DataRow("MONTH(?o)")]
    [DataRow("DAY(?o)")]
    [DataRow("HOURS(?o)")]
    [DataRow("MINUTES(?o)")]
    [DataRow("SECONDS(?o)")]
    [DataRow("LANGDIR(?o)")]
    [DataRow("BOUND(?o)")]
    [DataRow("ISIRI(?o)")]
    [DataRow("ISURI(?o)")]
    [DataRow("ISBLANK(?o)")]
    [DataRow("ISLITERAL(?o)")]
    [DataRow("ISNUMERIC(?o)")]
    [DataRow("HASLANG(?o)")]
    [DataRow("HASLANGDIR(?o)")]
    [DataRow("IRI(?o)")]
    [DataRow("URI(?o)")]
    [DataRow("TZ(?o)")]
    [DataRow("TIMEZONE(?o)")]
    [DataRow("BNODE(?o)")]
    // Binary
    [DataRow("CONTAINS(?o, \"x\")")]
    [DataRow("STRSTARTS(?o, \"x\")")]
    [DataRow("STRENDS(?o, \"x\")")]
    [DataRow("STRBEFORE(?o, \"x\")")]
    [DataRow("STRAFTER(?o, \"x\")")]
    [DataRow("STRDT(\"123\", <http://www.w3.org/2001/XMLSchema#integer>)")]
    [DataRow("STRLANG(\"hello\", \"en\")")]
    [DataRow("SAMETERM(?s, ?o)")]
    [DataRow("LANGMATCHES(LANG(?o), \"en\")")]
    // Variadic
    [DataRow("CONCAT(?o, \"-x\")")]
    [DataRow("COALESCE(?o, ?s)")]
    // Ternary / literal-argument
    [DataRow("IF(ISLITERAL(?o), ?o, ?s)")]
    [DataRow("REGEX(?o, \"^a\", \"i\")")]
    [DataRow("REPLACE(?o, \"a\", \"b\")")]
    [DataRow("SUBSTR(?o, 1, 2)")]
    [DataRow("STRLANGDIR(\"hi\", \"en\", \"ltr\")")]
    public void ShouldParseBuiltIn(string builtInCall)
    {
        //Every built-in is accepted in a computed projection regardless of its return type: the switch case is
        //reached and wired, so a successful parse proves the built-in is recognized and dispatched.
        RDFSelectQuery parsedQuery = RDFSelectQuery.FromString($"SELECT (({builtInCall}) AS ?v) WHERE {{ ?s ?p ?o }}");
        Assert.IsNotNull(parsedQuery);

        //The built-in landed as the computed projection's value-expression
        Assert.AreEqual(1, parsedQuery.ProjectionVars.Count);
        Assert.IsNotNull(parsedQuery.ProjectionVars.Single().Value.Item2);
    }

    /// <summary>
    /// "SPARQL 100%" iso-functionality gate: each newly representable library expression (STRBEFORE, STRAFTER,
    /// TZ, IRI, BNODE(arg)) is built VIA API in a computed projection and must be interchangeable with the query
    /// obtained by parsing its printed SPARQL — both on printing round-trip and on evaluation against a sample graph
    /// (see <see cref="RDFTestUtilities.AssertIso(RDFSelectQuery, RDFGraph)"/>). BNODE(arg) is deterministic
    /// (hash-derived) so the two sides yield the very same blank nodes.
    /// </summary>
    [TestMethod]
    public void ShouldRoundTripAndEvaluateNewExpressionsIso()
    {
        RDFResource predicate = new RDFResource("http://example.org/p");
        RDFGraph sampleGraph = new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s1"), predicate, new RDFPlainLiteral("hello")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s2"), predicate, new RDFPlainLiteral("http://example.org/abs")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s3"), predicate, new RDFTypedLiteral("2020-01-01T00:00:00Z", RDFModelEnums.RDFDatatypes.XSD_DATETIME)));

        RDFVariable objectVariable = new RDFVariable("?o");
        RDFExpression[] newExpressions =
        {
            new RDFStrBeforeExpression(objectVariable, new RDFConstantExpression(new RDFPlainLiteral("l"))),
            new RDFStrAfterExpression(objectVariable, new RDFConstantExpression(new RDFPlainLiteral("l"))),
            new RDFTzExpression(objectVariable),
            new RDFTimezoneExpression(objectVariable),
            new RDFIriExpression(objectVariable),
            new RDFBNodeExpression(objectVariable)
        };

        foreach (RDFExpression newExpression in newExpressions)
        {
            RDFSelectQuery apiQuery = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?s"), predicate, objectVariable)))
                .AddProjectionVariable(new RDFVariable("?v"), newExpression);

            RDFTestUtilities.AssertIso(apiQuery, sampleGraph);
        }
    }

    /// <summary>
    /// A bare FILTER constraint must evaluate to a boolean. A built-in whose value-expression is NON-boolean
    /// (e.g. the integer-returning STRLEN, the string-returning UCASE/SUBSTR, a bare arithmetic expression) has
    /// no place as a standalone constraint and cannot be wrapped in an <c>RDFExpressionFilter</c>: the parser
    /// rejects it via the <c>WrapExpressionInFilter</c> default branch. This pins that boundary.
    /// </summary>
    [TestMethod]
    [DataRow("STRLEN(?o)")]
    [DataRow("UCASE(?o)")]
    [DataRow("SUBSTR(?o, 1, 2)")]
    [DataRow("?o + 1")]
    public void ShouldThrowOnNonRepresentableBareFilterBuiltIn(string builtInCall)
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s ?p ?o FILTER({builtInCall}) }}"));

    /// <summary>
    /// The boolean built-ins that DO have an <c>RDFExpressionFilter</c> constructor are accepted as bare FILTER
    /// constraints (covering the WrapExpressionInFilter dispatch for each representable type).
    /// </summary>
    [TestMethod]
    [DataRow("BOUND(?o)")]
    [DataRow("ISIRI(?o)")]
    [DataRow("ISURI(?o)")]
    [DataRow("ISBLANK(?o)")]
    [DataRow("ISLITERAL(?o)")]
    [DataRow("ISNUMERIC(?o)")]
    [DataRow("SAMETERM(?s, ?o)")]
    [DataRow("LANGMATCHES(LANG(?o), \"en\")")]
    [DataRow("REGEX(?o, \"^a\")")]
    [DataRow("CONTAINS(?o, \"x\")")]
    [DataRow("STRSTARTS(?o, \"x\")")]
    [DataRow("STRENDS(?o, \"x\")")]
    [DataRow("HASLANG(?o)")]
    [DataRow("HASLANGDIR(?o)")]
    public void ShouldParseRepresentableBareFilterBuiltIn(string builtInCall)
    {
        RDFSelectQuery parsedQuery = RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s ?p ?o FILTER({builtInCall}) }}");
        Assert.IsInstanceOfType<RDFExpressionFilter>(SingleFilterOf(parsedQuery));
    }

    /// <summary>
    /// "SPARQL 100%" iso-functionality gate for Parser finding #1: each boolean string/lang built-in newly
    /// representable as a bare FILTER constraint is exercised BOTH ways — built through the fluent API (the new
    /// <c>RDFExpressionFilter</c> constructors) and obtained by parsing the printed SPARQL — and the two must be
    /// interchangeable on printing round-trip AND on evaluation against a sample graph (see
    /// <see cref="RDFTestUtilities.AssertIso(RDFSelectQuery, RDFGraph)"/>).
    /// </summary>
    [TestMethod]
    public void ShouldRoundTripAndEvaluateBareFilterBuiltInsIso()
    {
        //Sample graph mixing plain, language-tagged and language+direction literals so each built-in has both
        //matching and non-matching rows to select on
        RDFResource predicate = new RDFResource("http://example.org/p");
        RDFGraph sampleGraph = new RDFGraph()
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s1"), predicate, new RDFPlainLiteral("hello")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s2"), predicate, new RDFPlainLiteral("world")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s3"), predicate, new RDFPlainLiteral("ciao", "it")))
            .AddTriple(new RDFTriple(new RDFResource("http://example.org/s4"), predicate, new RDFPlainLiteral("salut", "fr--ltr")));

        RDFVariable objectVariable = new RDFVariable("?o");
        RDFFilter[] bareFilters =
        {
            new RDFExpressionFilter(new RDFContainsExpression(objectVariable, new RDFConstantExpression(new RDFPlainLiteral("ell")))),
            new RDFExpressionFilter(new RDFStrStartsExpression(objectVariable, new RDFConstantExpression(new RDFPlainLiteral("wor")))),
            new RDFExpressionFilter(new RDFStrEndsExpression(objectVariable, new RDFConstantExpression(new RDFPlainLiteral("lo")))),
            new RDFExpressionFilter(new RDFHasLangExpression(objectVariable)),
            new RDFExpressionFilter(new RDFHasLangDirExpression(objectVariable))
        };

        foreach (RDFFilter bareFilter in bareFilters)
        {
            RDFSelectQuery apiQuery = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?s"), predicate, objectVariable))
                    .AddFilter(bareFilter));

            RDFTestUtilities.AssertIso(apiQuery, sampleGraph);
        }
    }

    [TestMethod]
    [DataRow("ltr")]
    [DataRow("rtl")]
    public void ShouldRoundTripStrLangDir(string direction)
    {
        //STRLANGDIR prints its direction as a quoted string literal, so the printed form parses back IDENTICALLY
        //(this pins the printer fix: a bare 'ltr'/'rtl' would be read as a prefixed name and break the round-trip)
        string printed = RDFSelectQuery.FromString(
            $"SELECT (STRLANGDIR(?o, \"en\", \"{direction}\") AS ?v) WHERE {{ ?s ?p ?o }}").ToString();

        Assert.IsTrue(printed.Contains($"\"{direction}\""));
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(printed),
            RDFTestUtilities.NormalizeEOL(RDFSelectQuery.FromString(printed).ToString()));
    }
    #endregion
}
