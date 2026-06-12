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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Query;

namespace RDFSharp.Test.Query.Mirella.Parsers;

/// <summary>
/// Unit tests for the prologue half of RDFQueryParser: the BASE/PREFIX declarations parsing.
/// </summary>
public partial class RDFQueryParserTest
{
    #region Prologue
    [TestMethod]
    public void ShouldParseEmptyPrologueAndLeaveQueryFormUntouched()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("SELECT * WHERE { ?s ?p ?o }");
        RDFQueryParser.ParsePrologue(parserContext);

        //No BASE/PREFIX were present: the resolver stays empty and the query form keyword is left to be read
        Assert.AreEqual(string.Empty, parserContext.Resolver.BaseUri);
        Assert.AreEqual("SELECT", RDFQueryLexer.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParseBaseDeclaration()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("BASE <http://example.org/base/> SELECT");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://example.org/base/", parserContext.Resolver.BaseUri);
        Assert.AreEqual("SELECT", RDFQueryLexer.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParsePrefixDeclaration()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("PREFIX foaf: <http://xmlns.com/foaf/0.1/> SELECT");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://xmlns.com/foaf/0.1/", parserContext.Resolver.ResolveNamespace("foaf"));
        Assert.AreEqual("SELECT", RDFQueryLexer.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParseDefaultNamespaceDeclaration()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("PREFIX : <http://example.org/> SELECT");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://example.org/", parserContext.Resolver.ResolveNamespace(string.Empty));
        Assert.AreEqual("SELECT", RDFQueryLexer.ReadKeyword(parserContext));
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
        Assert.AreEqual("ASK", RDFQueryLexer.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldParsePrologueKeywordsCaseInsensitively()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext(
            "base <http://example.org/base/> prefix ex: <http://example.org/ns#> SELECT");
        RDFQueryParser.ParsePrologue(parserContext);

        Assert.AreEqual("http://example.org/base/", parserContext.Resolver.BaseUri);
        Assert.AreEqual("http://example.org/ns#", parserContext.Resolver.ResolveNamespace("ex"));
        Assert.AreEqual("SELECT", RDFQueryLexer.ReadKeyword(parserContext));
    }

    [TestMethod]
    public void ShouldThrowOnPrefixDeclarationWithoutColon()
    {
        RDFQueryParser.RDFQueryParserContext parserContext = RDFQueryParser.CreateContext("PREFIX foaf <http://xmlns.com/foaf/0.1/> SELECT");

        Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParser.ParsePrologue(parserContext));
    }
    #endregion
}
