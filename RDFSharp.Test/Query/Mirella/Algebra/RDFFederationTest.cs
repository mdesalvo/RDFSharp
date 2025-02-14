/*
   Copyright 2012-2025 Marco De Salvo

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
using System;
using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFFederationTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateFederation()
    {
        RDFFederation federation = new RDFFederation();

        Assert.IsNotNull(federation);
        Assert.IsNotNull(federation.DataSources);
        Assert.AreEqual(0, federation.DataSourcesCount);
        Assert.IsNotNull(federation.EndpointDataSourcesQueryOptions);
        Assert.AreEqual(0, federation.EndpointDataSourcesQueryOptions.Count);
        Assert.IsTrue(federation.FederationName.StartsWith("FEDERATION|ID=", StringComparison.Ordinal));
        Assert.IsTrue(federation.ToString().Equals(federation.FederationName));

        int i = federation.Count();
        Assert.AreEqual(0, i);

        int j = 0;
        IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
        while (datasourcesEnumerator.MoveNext()) j++;
        Assert.AreEqual(0, j);
    }

    [TestMethod]
    public void ShouldAddGraphToFederation()
    {
        RDFFederation federation = new RDFFederation();
        federation.AddGraph(new RDFGraph());
        federation.AddGraph(null); //Will be discarded, since null is not allowed

        Assert.AreEqual(1, federation.DataSourcesCount);
        Assert.AreEqual(0, federation.EndpointDataSourcesQueryOptions.Count);

        int i = federation.Count();
        Assert.AreEqual(1, i);

        int j = 0;
        IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
        while (datasourcesEnumerator.MoveNext()) j++;
        Assert.AreEqual(1, j);
    }

    [TestMethod]
    public void ShouldAddStoreToFederation()
    {
        RDFFederation federation = new RDFFederation();
        federation.AddStore(new RDFMemoryStore());
        federation.AddStore(null); //Will be discarded, since null is not allowed

        Assert.AreEqual(1, federation.DataSourcesCount);
        Assert.AreEqual(0, federation.EndpointDataSourcesQueryOptions.Count);

        int i = federation.Count();
        Assert.AreEqual(1, i);

        int j = 0;
        IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
        while (datasourcesEnumerator.MoveNext()) j++;
        Assert.AreEqual(1, j);
    }

    [TestMethod]
    public void ShouldAddFederationToFederation()
    {
        RDFFederation federation = new RDFFederation();
        federation.AddFederation(new RDFFederation());
        federation.AddFederation(federation); //Will be discarded, since self-reference is not allowed
        federation.AddFederation(null); //Will be discarded, since null is not allowed

        Assert.AreEqual(1, federation.DataSourcesCount);
        Assert.AreEqual(0, federation.EndpointDataSourcesQueryOptions.Count);

        int i = federation.Count();
        Assert.AreEqual(1, i);

        int j = 0;
        IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
        while (datasourcesEnumerator.MoveNext()) j++;
        Assert.AreEqual(1, j);
    }

    [TestMethod]
    public void ShouldAddSPARQLEndpointToFederation()
    {
        RDFFederation federation = new RDFFederation();
        federation.AddSPARQLEndpoint(new RDFSPARQLEndpoint(new Uri("ex:sparqlEndpoint1")));
        federation.AddSPARQLEndpoint(new RDFSPARQLEndpoint(new Uri("ex:sparqlEndpoint1"))); //Will be discarded, since duplicate endpoints are not allowed
        federation.AddSPARQLEndpoint(null); //Will be discarded, since null is not allowed
        federation.AddSPARQLEndpoint(new RDFSPARQLEndpoint(new Uri("ex:sparqlEndpoint2")), new RDFSPARQLEndpointQueryOptions { 
            ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult, 
            QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post, 
            TimeoutMilliseconds = 25000});

        Assert.AreEqual(2, federation.DataSourcesCount);
        Assert.AreEqual(2, federation.EndpointDataSourcesQueryOptions.Count);
        Assert.IsTrue(federation.EndpointDataSourcesQueryOptions.ContainsKey("ex:sparqlEndpoint1"));
        Assert.IsTrue(federation.EndpointDataSourcesQueryOptions.ContainsKey("ex:sparqlEndpoint2"));

        int i = federation.Count();
        Assert.AreEqual(2, i);

        int j = 0;
        IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
        while (datasourcesEnumerator.MoveNext()) j++;
        Assert.AreEqual(2, j);
    }

    [TestMethod]
    public void ShouldClearFederation()
    {
        RDFFederation federation = new RDFFederation();
        federation.AddGraph(new RDFGraph());
        federation.AddSPARQLEndpoint(new RDFSPARQLEndpoint(new Uri("ex:sparqlEndpoint")), new RDFSPARQLEndpointQueryOptions { TimeoutMilliseconds = 12500 });
        federation.ClearDataSources();

        Assert.IsNotNull(federation);
        Assert.IsNotNull(federation.DataSources);
        Assert.AreEqual(0, federation.DataSourcesCount);
        Assert.IsNotNull(federation.EndpointDataSourcesQueryOptions);
        Assert.AreEqual(0, federation.EndpointDataSourcesQueryOptions.Count);

        int i = federation.Count();
        Assert.AreEqual(0, i);

        int j = 0;
        IEnumerator<RDFDataSource> datasourcesEnumerator = federation.DataSourcesEnumerator;
        while (datasourcesEnumerator.MoveNext()) j++;
        Assert.AreEqual(0, j);
    }
    #endregion
}