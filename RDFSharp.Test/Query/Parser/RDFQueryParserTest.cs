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

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for RDFQueryParserFactory, the public entry point that turns a SPARQL query string into the
/// matching concrete RDFQuery instance. These tests pin down the factory's dispatch surface: routing a valid
/// form to its concrete type, and the error contract for unknown or not-yet-supported query forms.
/// </summary>
[TestClass]
public class RDFQueryParserFactoryTest
{
    #region Dispatch
    [TestMethod]
    public void ShouldParseSelectQueryAsRDFSelectQuery()
    {
        RDFQuery query = RDFQueryParserFactory.ParseQuery("SELECT * WHERE { ?s ?p ?o }");

        //The factory returns the concrete query type upcast to RDFQuery
        Assert.IsInstanceOfType<RDFSelectQuery>(query);
    }
    #endregion

    #region Errors
    [TestMethod]
    public void ShouldThrowOnNullOrEmptyQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("   "));

    [TestMethod]
    public void ShouldThrowOnUnknownQueryForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("FOOBAR * WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnMissingQueryForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("PREFIX ex: <http://example.org/>"));

    [TestMethod]
    public void ShouldThrowOnNotYetSupportedConstructForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnNotYetSupportedDescribeForm()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFQueryParserFactory.ParseQuery("DESCRIBE ?s WHERE { ?s ?p ?o }"));
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