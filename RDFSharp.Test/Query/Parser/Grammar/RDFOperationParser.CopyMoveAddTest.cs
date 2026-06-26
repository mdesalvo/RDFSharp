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
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the COPY/MOVE/ADD third of the RDFOperationParser graph-management grammar: the
/// source→destination operations (a GraphOrDefault pair separated by 'TO', plus the optional SILENT flag).
/// </summary>
public partial class RDFOperationParserTest
{
    #region CopyMoveAdd
    [TestMethod]
    public void ShouldParseAddOperation()
    {
        RDFAddOperation operation = RDFAddOperation.FromString("ADD <http://example.org/g1> TO <http://example.org/g2>");

        Assert.AreEqual("http://example.org/g1", operation.FromContext.ToString());
        Assert.AreEqual("http://example.org/g2", operation.ToContext.ToString());
        Assert.IsFalse(operation.IsSilent);
    }

    [TestMethod]
    public void ShouldParseCopySilentOperation()
    {
        RDFCopyOperation operation = RDFCopyOperation.FromString("COPY SILENT <http://example.org/g1> TO <http://example.org/g2>");

        Assert.IsTrue(operation.IsSilent);
        Assert.AreEqual("http://example.org/g1", operation.FromContext.ToString());
        Assert.AreEqual("http://example.org/g2", operation.ToContext.ToString());
    }

    [TestMethod]
    public void ShouldParseMoveOperationWithExplicitGraphKeyword()
    {
        //The 'GRAPH' keyword is optional in GraphOrDefault: parsing it covers the optional-consume branch
        RDFMoveOperation operation = RDFMoveOperation.FromString("MOVE GRAPH <http://example.org/g1> TO GRAPH <http://example.org/g2>");

        Assert.AreEqual("http://example.org/g1", operation.FromContext.ToString());
        Assert.AreEqual("http://example.org/g2", operation.ToContext.ToString());
    }

    [TestMethod]
    public void ShouldParseAddOperationWithDefaultEndpoints()
    {
        RDFAddOperation operation = RDFAddOperation.FromString("ADD DEFAULT TO DEFAULT");

        Assert.IsNull(operation.FromContext);
        Assert.IsNull(operation.ToContext);
    }

    [TestMethod]
    public void ShouldThrowOnCopyWithoutToSeparator()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFCopyOperation.FromString("COPY <http://example.org/g1> <http://example.org/g2>"));

    [TestMethod]
    public void ShouldThrowOnAddWithVariableEndpoint()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFAddOperation.FromString("ADD ?g1 TO <http://example.org/g2>"));

    [TestMethod]
    public void ShouldRoundTripAdd()
        => AssertOperationRoundTrips(new RDFAddOperation().SetFromContext(new Uri("http://example.org/g1")).SetToContext(new Uri("http://example.org/g2")));

    [TestMethod]
    public void ShouldRoundTripCopySilentToDefault()
        => AssertOperationRoundTrips(new RDFCopyOperation().SetFromContext(new Uri("http://example.org/g1")).Silent());

    [TestMethod]
    public void ShouldRoundTripMoveFromDefault()
        => AssertOperationRoundTrips(new RDFMoveOperation().SetToContext(new Uri("http://example.org/g2")));
    #endregion
}
