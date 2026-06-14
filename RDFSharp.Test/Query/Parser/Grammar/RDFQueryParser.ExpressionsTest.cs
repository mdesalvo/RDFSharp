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

    [TestMethod]
    public void ShouldThrowOnUnsupportedStandardBuiltIn()
    {
        //IRI() is a standard SPARQL built-in but has no expression class in the engine
        Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFSelectQuery.FromString("SELECT * WHERE { ?s ?p ?o FILTER(IRI(?o) = ?s) }"));
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
    // Binary
    [DataRow("CONTAINS(?o, \"x\")")]
    [DataRow("STRSTARTS(?o, \"x\")")]
    [DataRow("STRENDS(?o, \"x\")")]
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
    /// The boolean-returning string built-ins CONTAINS / STRSTARTS / STRENDS / HASLANG / HASLANGDIR parse as value
    /// expressions, but the flat model's <c>RDFExpressionFilter</c> exposes no constructor for them, so they are
    /// NOT representable as a bare FILTER constraint (a known model limit; the idiomatic workaround is to compare
    /// them, e.g. <c>FILTER(CONTAINS(?o,"x") = true)</c>, which the parser accepts). This pins that boundary.
    /// </summary>
    [TestMethod]
    [DataRow("CONTAINS(?o, \"x\")")]
    [DataRow("STRSTARTS(?o, \"x\")")]
    [DataRow("STRENDS(?o, \"x\")")]
    [DataRow("HASLANG(?o)")]
    [DataRow("HASLANGDIR(?o)")]
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
    public void ShouldParseRepresentableBareFilterBuiltIn(string builtInCall)
    {
        RDFSelectQuery parsedQuery = RDFSelectQuery.FromString($"SELECT * WHERE {{ ?s ?p ?o FILTER({builtInCall}) }}");
        Assert.IsInstanceOfType<RDFExpressionFilter>(SingleFilterOf(parsedQuery));
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
