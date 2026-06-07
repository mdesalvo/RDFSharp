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