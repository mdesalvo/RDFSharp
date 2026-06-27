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
/// Unit tests for the CREATE/DROP half of the RDFOperationParser graph-management grammar (the two operations
/// whose body is a single graph reference plus the optional SILENT flag).
/// </summary>
public partial class RDFOperationParserTest
{
    #region CreateDrop

    #region CREATE
    [TestMethod]
    public void ShouldParseCreateOperation()
    {
        RDFCreateOperation operation = RDFCreateOperation.FromString("CREATE GRAPH <http://example.org/g>");

        Assert.AreEqual("http://example.org/g", operation.FromContext.ToString());
        Assert.IsFalse(operation.IsSilent);
    }

    [TestMethod]
    public void ShouldParseCreateSilentOperation()
    {
        RDFCreateOperation operation = RDFCreateOperation.FromString("CREATE SILENT GRAPH <http://example.org/g>");

        Assert.IsTrue(operation.IsSilent);
        Assert.AreEqual("http://example.org/g", operation.FromContext.ToString());
    }

    [TestMethod]
    public void ShouldThrowOnCreateWithoutGraphKeyword()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFCreateOperation.FromString("CREATE <http://example.org/g>"));

    [TestMethod]
    public void ShouldRoundTripCreate()
        => AssertOperationRoundTrips(new RDFCreateOperation(new Uri("http://example.org/g")).Silent());
    #endregion

    #region DROP
    [TestMethod]
    public void ShouldParseDropGraphOperation()
    {
        RDFDropOperation operation = RDFDropOperation.FromString("DROP GRAPH <http://example.org/g>");

        Assert.AreEqual("http://example.org/g", operation.FromContext.ToString());
        Assert.IsFalse(operation.IsSilent);
    }

    [TestMethod]
    [DataRow("DEFAULT", RDFQueryEnums.RDFClearOperationFlavor.DEFAULT)]
    [DataRow("NAMED", RDFQueryEnums.RDFClearOperationFlavor.NAMED)]
    [DataRow("ALL", RDFQueryEnums.RDFClearOperationFlavor.ALL)]
    public void ShouldParseDropFlavorOperation(string flavorKeyword, RDFQueryEnums.RDFClearOperationFlavor expectedFlavor)
    {
        RDFDropOperation operation = RDFDropOperation.FromString("DROP SILENT " + flavorKeyword);

        Assert.IsNull(operation.FromContext);
        Assert.AreEqual(expectedFlavor, operation.OperationFlavor);
        Assert.IsTrue(operation.IsSilent);
    }

    [TestMethod]
    public void ShouldThrowOnDropWithMalformedGraphRef()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFDropOperation.FromString("DROP SOMETHING"));

    [TestMethod]
    public void ShouldRoundTripDropGraph()
        => AssertOperationRoundTrips(new RDFDropOperation(new Uri("http://example.org/g")));

    [TestMethod]
    public void ShouldRoundTripDropFlavor()
        => AssertOperationRoundTrips(new RDFDropOperation(RDFQueryEnums.RDFClearOperationFlavor.NAMED).Silent());
    #endregion

    #endregion
}
