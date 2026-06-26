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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFServiceTest
{
    #region Helpers
    private static RDFPatternGroup SamplePatternGroup()
        => new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?s"), new RDFVariable("?p"), new RDFVariable("?o")));

    private static RDFSPARQLEndpoint SampleEndpoint()
        => new RDFSPARQLEndpoint(new Uri("http://example.org/sparql"));
    #endregion

    #region Ctors
    [TestMethod]
    public void ShouldCreateServiceWithConcreteEndpoint()
    {
        RDFPatternGroup inner = SamplePatternGroup();
        RDFService service = new RDFService(SampleEndpoint(), inner);

        Assert.IsNotNull(service);
        Assert.IsTrue(service.IsEvaluable);
        Assert.IsFalse(service.IsOptional);
        Assert.IsNull(service.EndpointVariable);
        Assert.IsNotNull(service.Endpoint);
        Assert.AreEqual("http://example.org/sparql", service.Endpoint.BaseAddress.ToString());
        Assert.AreSame(inner, service.InnerPattern);
        Assert.IsNotNull(service.QueryOptions);
        Assert.AreEqual(RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, service.QueryOptions.ErrorBehavior);
        Assert.IsFalse(service.IsSilent);
    }

    [TestMethod]
    public void ShouldCreateServiceWithVariableEndpoint()
    {
        RDFService service = new RDFService(new RDFVariable("?ep"), SamplePatternGroup());

        Assert.IsNotNull(service);
        Assert.IsNotNull(service.EndpointVariable);
        Assert.AreEqual("?EP", service.EndpointVariable.ToString());
        Assert.IsNotNull(service.Endpoint); //a reusable placeholder endpoint is instantiated for the variable case
    }

    [TestMethod]
    public void ShouldCreateSilentService()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup(),
            new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult });

        Assert.IsTrue(service.IsSilent);
    }

    [TestMethod]
    public void ShouldThrowOnCreatingServiceBecauseNullEndpoint()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFService((RDFSPARQLEndpoint)null, SamplePatternGroup()));

    [TestMethod]
    public void ShouldThrowOnCreatingServiceBecauseNullVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFService((RDFVariable)null, SamplePatternGroup()));

    [TestMethod]
    public void ShouldThrowOnCreatingConcreteServiceBecauseNullInner()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFService(SampleEndpoint(), null));

    [TestMethod]
    public void ShouldThrowOnCreatingVariableServiceBecauseNullInner()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFService(new RDFVariable("?ep"), null));
    #endregion

    #region Interfaces
    [TestMethod]
    public void ShouldPrintConcreteService()
        => Assert.Contains("SERVICE <http://example.org/sparql>", new RDFService(SampleEndpoint(), SamplePatternGroup()).ToString());

    [TestMethod]
    public void ShouldPrintVariableService()
        => Assert.Contains("SERVICE ?EP", new RDFService(new RDFVariable("?ep"), SamplePatternGroup()).ToString());

    [TestMethod]
    public void ShouldPrintSilentService()
        => Assert.Contains("SERVICE SILENT <http://example.org/sparql>", new RDFService(SampleEndpoint(), SamplePatternGroup(),
            new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult }).ToString());

    [TestMethod]
    public void ShouldPrintOptionalService()
        => Assert.Contains("OPTIONAL {", new RDFService(SampleEndpoint(), SamplePatternGroup()).Optional().ToString());
    #endregion

    #region Optional
    [TestMethod]
    public void ShouldSetServiceOptional()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());

        Assert.IsFalse(service.IsOptional);
        Assert.AreSame(service, service.Optional());
        Assert.IsTrue(service.IsOptional);
    }
    #endregion

    #region Union/Minus fluent
    [TestMethod]
    public void ShouldCombineServiceUnionWithPatternGroup()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember union = service.Union(new RDFPatternGroup());

        Assert.AreSame(service, union.LeftOperand);
        Assert.IsInstanceOfType(union.RightOperand, typeof(RDFPatternGroup));
    }

    [TestMethod]
    public void ShouldCombineServiceUnionWithSubQuery()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember union = service.Union(new RDFSelectQuery());

        Assert.IsInstanceOfType(union.RightOperand, typeof(RDFSelectQuery));
    }

    [TestMethod]
    public void ShouldCombineServiceUnionWithBinaryQueryMember()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember union = service.Union(new RDFPatternGroup().Union(new RDFPatternGroup()));

        Assert.IsInstanceOfType(union.RightOperand, typeof(RDFBinaryQueryMember));
    }

    [TestMethod]
    public void ShouldCombineServiceUnionWithService()
    {
        RDFService left = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFService right = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember union = left.Union(right);

        Assert.AreSame(left, union.LeftOperand);
        Assert.AreSame(right, union.RightOperand);
    }

    [TestMethod]
    public void ShouldCombineServiceMinusWithPatternGroup()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember minus = service.Minus(new RDFPatternGroup());

        Assert.AreSame(service, minus.LeftOperand);
        Assert.IsInstanceOfType(minus.RightOperand, typeof(RDFPatternGroup));
    }

    [TestMethod]
    public void ShouldCombineServiceMinusWithSubQuery()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember minus = service.Minus(new RDFSelectQuery());

        Assert.IsInstanceOfType(minus.RightOperand, typeof(RDFSelectQuery));
    }

    [TestMethod]
    public void ShouldCombineServiceMinusWithBinaryQueryMember()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember minus = service.Minus(new RDFPatternGroup().Union(new RDFPatternGroup()));

        Assert.IsInstanceOfType(minus.RightOperand, typeof(RDFBinaryQueryMember));
    }

    [TestMethod]
    public void ShouldCombineServiceMinusWithService()
    {
        RDFService left = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFService right = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember minus = left.Minus(right);

        Assert.AreSame(right, minus.RightOperand);
    }

    [TestMethod]
    public void ShouldCombinePatternGroupUnionAndMinusWithService()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());

        Assert.IsInstanceOfType(new RDFPatternGroup().Union(service).RightOperand, typeof(RDFService));
        Assert.IsInstanceOfType(new RDFPatternGroup().Minus(service).RightOperand, typeof(RDFService));
    }

    [TestMethod]
    public void ShouldCombineSubQueryUnionAndMinusWithService()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());

        Assert.IsInstanceOfType(new RDFSelectQuery().Union(service).RightOperand, typeof(RDFService));
        Assert.IsInstanceOfType(new RDFSelectQuery().Minus(service).RightOperand, typeof(RDFService));
    }

    [TestMethod]
    public void ShouldCombineBinaryQueryMemberUnionAndMinusWithService()
    {
        RDFService service = new RDFService(SampleEndpoint(), SamplePatternGroup());
        RDFBinaryQueryMember tree = new RDFPatternGroup().Union(new RDFPatternGroup());

        Assert.IsInstanceOfType(tree.Union(service).RightOperand, typeof(RDFService));
        Assert.IsInstanceOfType(tree.Minus(service).RightOperand, typeof(RDFService));
    }
    #endregion
}
