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

using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFSPARQLEndpointTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateSPARQLEndpoint()
    {
        RDFSPARQLEndpoint sparqlEndpoint = new RDFSPARQLEndpoint(new Uri("http://sparql/query"));

        Assert.IsNotNull(sparqlEndpoint);
        Assert.IsTrue(sparqlEndpoint.BaseAddress.Equals(new Uri("http://sparql/query")));
        Assert.IsNotNull(sparqlEndpoint.QueryParams);
        Assert.AreEqual(0, sparqlEndpoint.QueryParams.Count);
        Assert.IsTrue(sparqlEndpoint.ToString().Equals("http://sparql/query"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingSPARQLEndpointBecauseNullUri()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFSPARQLEndpoint(null));

    [TestMethod]
    public void ShouldAddQueryOptions()
    {
        RDFSPARQLEndpoint sparqlEndpoint = new RDFSPARQLEndpoint(new Uri("http://sparql/query"));
        sparqlEndpoint.AddDefaultGraphUri("http://ex1.org/");
        sparqlEndpoint.AddNamedGraphUri("http://ex2.org/");

        Assert.IsNotNull(sparqlEndpoint);
        Assert.IsTrue(sparqlEndpoint.BaseAddress.Equals(new Uri("http://sparql/query")));
        Assert.IsNotNull(sparqlEndpoint.QueryParams);
        Assert.AreEqual(2, sparqlEndpoint.QueryParams.Count);
        Assert.IsTrue(sparqlEndpoint.QueryParams["default-graph-uri"].Equals("http://ex1.org/"));
        Assert.IsTrue(sparqlEndpoint.QueryParams["named-graph-uri"].Equals("http://ex2.org/"));
        Assert.IsTrue(sparqlEndpoint.ToString().Equals("http://sparql/query"));
    }

    [TestMethod]
    public void ShouldSetBasicAuthorizationHeader()
    {
        string authHeaderValue = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("user:pwd"));
        RDFSPARQLEndpoint sparqlEndpoint = new RDFSPARQLEndpoint(new Uri("http://sparql/query"));
        sparqlEndpoint.SetBasicAuthorizationHeader(authHeaderValue);

        Assert.IsNotNull(sparqlEndpoint);
        Assert.IsTrue(sparqlEndpoint.BaseAddress.Equals(new Uri("http://sparql/query")));
        Assert.IsNotNull(sparqlEndpoint.QueryParams);
        Assert.AreEqual(0, sparqlEndpoint.QueryParams.Count);
        Assert.IsTrue(string.Equals(sparqlEndpoint.ToString(), "http://sparql/query"));
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointAuthorizationTypes.Basic, sparqlEndpoint.AuthorizationType);
        Assert.IsTrue(string.Equals(sparqlEndpoint.AuthorizationValue, authHeaderValue));
    }

    [TestMethod]
    public void ShouldSetBearerAuthorizationHeader()
    {
        RDFSPARQLEndpoint sparqlEndpoint = new RDFSPARQLEndpoint(new Uri("http://sparql/query"));
        sparqlEndpoint.SetBearerAuthorizationHeader("vF9dft4qmT");

        Assert.IsNotNull(sparqlEndpoint);
        Assert.IsTrue(sparqlEndpoint.BaseAddress.Equals(new Uri("http://sparql/query")));
        Assert.IsNotNull(sparqlEndpoint.QueryParams);
        Assert.AreEqual(0, sparqlEndpoint.QueryParams.Count);
        Assert.IsTrue(string.Equals(sparqlEndpoint.ToString(), "http://sparql/query"));
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointAuthorizationTypes.Bearer, sparqlEndpoint.AuthorizationType);
        Assert.IsTrue(string.Equals(sparqlEndpoint.AuthorizationValue, "vF9dft4qmT"));
    }

    [TestMethod]
    public void ShouldCreateSPARQLEndpointQueryOptions()
    {
        RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions = new RDFSPARQLEndpointQueryOptions();

        Assert.IsNotNull(sparqlEndpointQueryOptions);
        Assert.AreEqual(-1, sparqlEndpointQueryOptions.TimeoutMilliseconds);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, sparqlEndpointQueryOptions.ErrorBehavior);
    }

    [TestMethod]
    public void ShouldCreateSPARQLEndpointQueryOptionsWithTimeoutMilliseconds()
    {
        RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions = new RDFSPARQLEndpointQueryOptions(2000);

        Assert.IsNotNull(sparqlEndpointQueryOptions);
        Assert.AreEqual(2000, sparqlEndpointQueryOptions.TimeoutMilliseconds);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, sparqlEndpointQueryOptions.ErrorBehavior);
    }

    [TestMethod]
    public void ShouldCreateSPARQLEndpointQueryOptionsWithNegativeTimeoutMilliseconds()
    {
        RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions = new RDFSPARQLEndpointQueryOptions(-2000);

        Assert.IsNotNull(sparqlEndpointQueryOptions);
        Assert.AreEqual(-1, sparqlEndpointQueryOptions.TimeoutMilliseconds);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, sparqlEndpointQueryOptions.ErrorBehavior);
    }

    [TestMethod]
    public void ShouldCreateSPARQLEndpointQueryOptionsWithTimeoutMillisecondsAndErrorBehavior()
    {
        RDFSPARQLEndpointQueryOptions sparqlEndpointQueryOptions = new RDFSPARQLEndpointQueryOptions(2000, RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult);

        Assert.IsNotNull(sparqlEndpointQueryOptions);
        Assert.AreEqual(2000, sparqlEndpointQueryOptions.TimeoutMilliseconds);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult, sparqlEndpointQueryOptions.ErrorBehavior);
    }

    //SPARQL UPDATE

    [TestMethod]
    public void ShouldCreateSPARQLEndpointOperationOptions()
    {
        RDFSPARQLEndpointOperationOptions sparqlEndpointOperationOptions = new RDFSPARQLEndpointOperationOptions();

        Assert.IsNotNull(sparqlEndpointOperationOptions);
        Assert.AreEqual(-1, sparqlEndpointOperationOptions.TimeoutMilliseconds);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.Sparql_Update, sparqlEndpointOperationOptions.RequestContentType);
    }

    [TestMethod]
    public void ShouldCreateSPARQLEndpointOperationOptionsWithTimeoutMilliseconds()
    {
        RDFSPARQLEndpointOperationOptions sparqlEndpointOperationOptions = new RDFSPARQLEndpointOperationOptions(20000);

        Assert.IsNotNull(sparqlEndpointOperationOptions);
        Assert.AreEqual(20000, sparqlEndpointOperationOptions.TimeoutMilliseconds);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.Sparql_Update, sparqlEndpointOperationOptions.RequestContentType);
    }

    [TestMethod]
    public void ShouldCreateSPARQLEndpointOperationOptionsWithNegativeTimeoutMilliseconds()
    {
        RDFSPARQLEndpointOperationOptions sparqlEndpointOperationOptions = new RDFSPARQLEndpointOperationOptions(-2000);

        Assert.IsNotNull(sparqlEndpointOperationOptions);
        Assert.AreEqual(-1, sparqlEndpointOperationOptions.TimeoutMilliseconds);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.Sparql_Update, sparqlEndpointOperationOptions.RequestContentType);
    }

    [TestMethod]
    public void ShouldCreateSPARQLEndpointOperationOptionsWithTimeouteMillisecondsAndRequestContentType()
    {
        RDFSPARQLEndpointOperationOptions sparqlEndpointOperationOptions = new RDFSPARQLEndpointOperationOptions(20000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded);

        Assert.IsNotNull(sparqlEndpointOperationOptions);
        Assert.AreEqual(20000, sparqlEndpointOperationOptions.TimeoutMilliseconds);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded, sparqlEndpointOperationOptions.RequestContentType);
    }
    #endregion
}