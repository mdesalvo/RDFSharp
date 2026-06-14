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
/// Unit tests for the CLEAR/LOAD half of RDFOperationParser: the CLEAR and LOAD operations
/// (the optional SILENT flag, the source/target IRIs, and the DEFAULT/NAMED/ALL/GRAPH flavors of CLEAR).
/// </summary>
public partial class RDFOperationParserTest
{
    #region ClearLoad

    /// <summary>
    /// Asserts that printing the given operation, parsing the print back, and printing the result yields the very
    /// same text: the round-trip oracle for SPARQL UPDATE operations.
    /// </summary>
    private static void AssertOperationRoundTrips(RDFOperation originalOperation)
    {
        string printedOperation = originalOperation.ToString();
        RDFOperation reparsedOperation = RDFOperationParserFactory.ParseOperation(printedOperation);
        Assert.AreEqual(RDFTestUtilities.NormalizeEOL(printedOperation), RDFTestUtilities.NormalizeEOL(reparsedOperation.ToString()));
    }

    #region LOAD

    [TestMethod]
    public void ShouldParseLoadOperation()
    {
        RDFLoadOperation operation = RDFLoadOperation.FromString("LOAD <http://example.org/data>");

        Assert.AreEqual("http://example.org/data", operation.FromContext.ToString());
        Assert.IsNull(operation.ToContext);
        Assert.IsFalse(operation.IsSilent);
    }

    [TestMethod]
    public void ShouldParseLoadSilentIntoGraph()
    {
        RDFLoadOperation operation = RDFLoadOperation.FromString(
            "LOAD SILENT <http://example.org/data> INTO GRAPH <http://example.org/g>");

        Assert.IsTrue(operation.IsSilent);
        Assert.AreEqual("http://example.org/data", operation.FromContext.ToString());
        Assert.AreEqual("http://example.org/g", operation.ToContext.ToString());
    }

    [TestMethod]
    public void ShouldRoundTripLoad()
        => AssertOperationRoundTrips(new RDFLoadOperation(new Uri("http://example.org/data")));

    [TestMethod]
    public void ShouldRoundTripLoadSilentIntoGraph()
        => AssertOperationRoundTrips(new RDFLoadOperation(new Uri("http://example.org/data"))
            .Silent()
            .SetContext(new Uri("http://example.org/g")));

    [TestMethod]
    public void ShouldThrowOnLoadWithLiteralSource()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFLoadOperation.FromString("LOAD \"data\""));

    [TestMethod]
    public void ShouldThrowOnLoadIntoWithoutGraphKeyword()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFLoadOperation.FromString("LOAD <http://example.org/data> INTO <http://example.org/g>"));

    [TestMethod]
    public void ShouldThrowWhenLoadFromStringFedAClearOperation()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFLoadOperation.FromString("CLEAR ALL"));

    #endregion

    #region CLEAR

    [TestMethod]
    public void ShouldParseClearAll()
    {
        RDFClearOperation operation = RDFClearOperation.FromString("CLEAR ALL");

        Assert.AreEqual(RDFQueryEnums.RDFClearOperationFlavor.ALL, operation.OperationFlavor);
        Assert.IsNull(operation.FromContext);
        Assert.IsFalse(operation.IsSilent);
    }

    [TestMethod]
    public void ShouldParseClearDefault()
        => Assert.AreEqual(RDFQueryEnums.RDFClearOperationFlavor.DEFAULT,
            RDFClearOperation.FromString("CLEAR DEFAULT").OperationFlavor);

    [TestMethod]
    public void ShouldParseClearNamed()
        => Assert.AreEqual(RDFQueryEnums.RDFClearOperationFlavor.NAMED,
            RDFClearOperation.FromString("CLEAR NAMED").OperationFlavor);

    [TestMethod]
    public void ShouldParseClearSilentGraph()
    {
        RDFClearOperation operation = RDFClearOperation.FromString("CLEAR SILENT GRAPH <http://example.org/g>");

        Assert.IsTrue(operation.IsSilent);
        Assert.AreEqual("http://example.org/g", operation.FromContext.ToString());
    }

    [TestMethod]
    public void ShouldParseClearGraphWithPrefixedName()
    {
        RDFClearOperation operation = RDFClearOperation.FromString(
            "PREFIX ex: <http://example.org/> CLEAR GRAPH ex:g");

        Assert.AreEqual("http://example.org/g", operation.FromContext.ToString());
    }

    [TestMethod]
    public void ShouldRoundTripClearAll()
        => AssertOperationRoundTrips(new RDFClearOperation(RDFQueryEnums.RDFClearOperationFlavor.ALL));

    [TestMethod]
    public void ShouldRoundTripClearSilentGraph()
        => AssertOperationRoundTrips(new RDFClearOperation(new Uri("http://example.org/g")).Silent());

    [TestMethod]
    public void ShouldThrowOnClearWithUnknownGraphRef()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFClearOperation.FromString("CLEAR FOO"));

    #endregion

    #endregion
}
