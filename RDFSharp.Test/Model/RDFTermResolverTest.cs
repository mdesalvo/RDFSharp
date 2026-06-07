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
using System;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFTermResolverTest
{
    #region Tests
    [TestMethod]
    public void ShouldResolveBaseUriFromGraphContext()
    {
        //The graph-backed resolver must expose, as base IRI, exactly the graph context
        //(this is the historical behavior: relative IRIs were resolved against "graph.ToString()")
        RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/base/"));
        RDFGraphTermResolver resolver = new RDFGraphTermResolver(graph);

        Assert.AreEqual("http://example.org/base/", resolver.BaseUri);
        Assert.AreEqual(graph.ToString(), resolver.BaseUri);
    }

    [TestMethod]
    public void ShouldReflectGraphContextChangesLive()
    {
        //The resolver reads the wrapped graph LIVE: this is required because Turtle "@base" directives
        //mutate the graph context mid-document, and subsequent relative IRIs must see the new base
        RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/first/"));
        RDFGraphTermResolver resolver = new RDFGraphTermResolver(graph);

        Assert.AreEqual("http://example.org/first/", resolver.BaseUri);

        graph.SetContext(new Uri("http://example.org/second/"));

        Assert.AreEqual("http://example.org/second/", resolver.BaseUri);
    }

    [TestMethod]
    public void ShouldResolveDefaultNamespaceForEmptyPrefix()
    {
        //An empty prefix label denotes the default namespace, which for the graph-backed resolver
        //is the graph context (matching how the ":" prefix used "graph.Context.ToString()")
        RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/default#"));
        RDFGraphTermResolver resolver = new RDFGraphTermResolver(graph);

        Assert.AreEqual(graph.Context.ToString(), resolver.ResolveNamespace(string.Empty));
    }

    [TestMethod]
    public void ShouldResolveDefaultNamespaceForNullPrefix()
    {
        //A null prefix label is treated the same as an empty one (the default namespace)
        RDFGraph graph = new RDFGraph().SetContext(new Uri("http://example.org/default#"));
        RDFGraphTermResolver resolver = new RDFGraphTermResolver(graph);

        Assert.AreEqual(graph.Context.ToString(), resolver.ResolveNamespace(null));
    }

    [TestMethod]
    public void ShouldResolveRegisteredPrefix()
    {
        //A non-empty prefix is resolved through the global namespace register: the built-in "rdf"
        //prefix must map to the RDF vocabulary base URI
        RDFGraph graph = new RDFGraph();
        RDFGraphTermResolver resolver = new RDFGraphTermResolver(graph);

        Assert.AreEqual(RDFVocabulary.RDF.BASE_URI, resolver.ResolveNamespace(RDFVocabulary.RDF.PREFIX));
    }

    [TestMethod]
    public void ShouldReturnNullForUnregisteredPrefix()
    {
        //An unregistered prefix cannot be resolved, so the resolver must return null
        //(this is what historically let ParseQNameOrBoolean fall back to the default namespace)
        RDFGraph graph = new RDFGraph();
        RDFGraphTermResolver resolver = new RDFGraphTermResolver(graph);

        Assert.IsNull(resolver.ResolveNamespace("thisPrefixIsDefinitelyNotRegistered"));
    }
    #endregion
}