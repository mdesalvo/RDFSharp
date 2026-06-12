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
    #endregion
}
