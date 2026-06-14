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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the DATA half of RDFOperationParser: the INSERT DATA and DELETE DATA operations — ground
/// QuadData templates (plain triples and GRAPH quad blocks), with variables and property paths rejected.
/// </summary>
public partial class RDFOperationParserTest
{
    #region Data

    [TestMethod]
    public void ShouldParseInsertDataWithSingleTriple()
    {
        RDFInsertDataOperation operation = RDFInsertDataOperation.FromString(
            "INSERT DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> }");

        Assert.AreEqual(1, operation.InsertTemplates.Count);
        Assert.AreEqual(0, operation.InsertTemplates[0].Variables.Count);
    }

    [TestMethod]
    public void ShouldParseInsertDataWithPredicateObjectList()
    {
        RDFInsertDataOperation operation = RDFInsertDataOperation.FromString(
            "PREFIX ex: <http://example.org/> INSERT DATA { ex:s a ex:Type ; ex:p ex:o , ex:o2 }");

        //'a' + two predicate-object groups expand into three ground templates
        Assert.AreEqual(3, operation.InsertTemplates.Count);
        Assert.AreEqual(1, operation.GetPrefixes().Count);
    }

    [TestMethod]
    public void ShouldParseDeleteDataWithSingleTriple()
    {
        RDFDeleteDataOperation operation = RDFDeleteDataOperation.FromString(
            "DELETE DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> }");

        Assert.AreEqual(1, operation.DeleteTemplates.Count);
    }

    [TestMethod]
    public void ShouldParseInsertDataWithGraphQuad()
    {
        RDFInsertDataOperation operation = RDFInsertDataOperation.FromString(
            "INSERT DATA { GRAPH <http://example.org/g> { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }");

        //The triple inside the GRAPH block is a quad carrying the graph context
        Assert.AreEqual(1, operation.InsertTemplates.Count);
        Assert.IsNotNull(operation.InsertTemplates[0].Context);
        Assert.AreEqual("http://example.org/g", operation.InsertTemplates[0].Context.ToString());
    }

    [TestMethod]
    public void ShouldParseInsertDataWithDefaultAndGraphMixed()
    {
        RDFInsertDataOperation operation = RDFInsertDataOperation.FromString(
            "INSERT DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> . GRAPH <http://example.org/g> { <http://example.org/s2> <http://example.org/p2> <http://example.org/o2> } }");

        Assert.AreEqual(2, operation.InsertTemplates.Count);
        Assert.IsNull(operation.InsertTemplates[0].Context);
        Assert.IsNotNull(operation.InsertTemplates[1].Context);
    }

    [TestMethod]
    public void ShouldRoundTripInsertData()
        => AssertOperationRoundTrips(new RDFInsertDataOperation()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("foaf"))
            .AddInsertTemplate(new RDFPattern(new RDFResource("http://example.org/s"), RDFVocabulary.FOAF.NAME, new RDFTypedLiteral("Alice", RDFModelEnums.RDFDatatypes.XSD_STRING))));

    [TestMethod]
    public void ShouldRoundTripDeleteData()
        => AssertOperationRoundTrips(new RDFDeleteDataOperation()
            .AddDeleteTemplate(new RDFPattern(new RDFResource("http://example.org/s"), new RDFResource("http://example.org/p"), new RDFResource("http://example.org/o"))));

    [TestMethod]
    public void ShouldRoundTripInsertDataWithGraphQuad()
        => AssertOperationRoundTrips(new RDFInsertDataOperation()
            .AddInsertTemplate(new RDFPattern(new RDFContext("http://example.org/g"), new RDFResource("http://example.org/s"), new RDFResource("http://example.org/p"), new RDFResource("http://example.org/o"))));

    [TestMethod]
    public void ShouldThrowOnInsertDataWithVariable()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFInsertDataOperation.FromString("INSERT DATA { ?s <http://example.org/p> <http://example.org/o> }"));

    [TestMethod]
    public void ShouldThrowOnInsertDataWithVariableGraph()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFInsertDataOperation.FromString("INSERT DATA { GRAPH ?g { <http://example.org/s> <http://example.org/p> <http://example.org/o> } }"));

    [TestMethod]
    public void ShouldThrowOnInsertDataWithPropertyPath()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFInsertDataOperation.FromString(
                "PREFIX ex: <http://example.org/> INSERT DATA { ex:s ex:p/ex:q ex:o }"));

    [TestMethod]
    public void ShouldThrowOnInsertDataWithUnexpectedKeyword()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFInsertDataOperation.FromString("INSERT DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> FILTER(true) }"));

    [TestMethod]
    public void ShouldThrowWhenInsertDataFromStringFedADeleteData()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFInsertDataOperation.FromString("DELETE DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> }"));

    #endregion
}
