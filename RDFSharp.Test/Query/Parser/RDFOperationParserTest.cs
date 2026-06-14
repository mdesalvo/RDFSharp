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
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for RDFOperationParser, the SPARQL UPDATE entry point/dispatcher: it dispatches a command string to
/// the matching RDFOperation, rejecting not-yet-implemented forms, non-representable graph-management operations
/// (CREATE/DROP/…), multiple ';'-separated operations, and empty input. The grammar of each operation form is
/// exercised in the sibling partials (e.g. <c>RDFOperationParser.ClearLoadTest.cs</c>).
/// </summary>
[TestClass]
public partial class RDFOperationParserTest
{
    #region OperationDispatcher

    [TestMethod]
    public void ShouldDispatchLoadOperation()
        => Assert.IsInstanceOfType<RDFLoadOperation>(
            RDFOperationParserFactory.ParseOperation("LOAD <http://example.org/data>"));

    [TestMethod]
    public void ShouldDispatchClearOperation()
        => Assert.IsInstanceOfType<RDFClearOperation>(
            RDFOperationParserFactory.ParseOperation("CLEAR ALL"));

    [TestMethod]
    public void ShouldDispatchOperationAfterPrologue()
        => Assert.IsInstanceOfType<RDFClearOperation>(
            RDFOperationParserFactory.ParseOperation("PREFIX ex: <http://example.org/> CLEAR GRAPH ex:g"));

    [TestMethod]
    public void ShouldTolerateSingleTrailingSemicolon()
        => Assert.IsInstanceOfType<RDFLoadOperation>(
            RDFOperationParserFactory.ParseOperation("LOAD <http://example.org/data> ;"));

    [TestMethod]
    public void ShouldThrowOnNotYetSupportedInsertForm()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFOperationParserFactory.ParseOperation("INSERT DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> }"));

    [TestMethod]
    public void ShouldThrowOnNotYetSupportedDeleteForm()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFOperationParserFactory.ParseOperation("DELETE WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnNonRepresentableCreateOperation()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFOperationParserFactory.ParseOperation("CREATE GRAPH <http://example.org/g>"));

    [TestMethod]
    public void ShouldThrowOnNonRepresentableDropOperation()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFOperationParserFactory.ParseOperation("DROP GRAPH <http://example.org/g>"));

    [TestMethod]
    public void ShouldThrowOnMultipleSemicolonSeparatedOperations()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFOperationParserFactory.ParseOperation("CLEAR ALL ; CLEAR DEFAULT"));

    [TestMethod]
    public void ShouldThrowOnEmptyOperation()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFOperationParserFactory.ParseOperation("   "));

    [TestMethod]
    public void ShouldThrowOnUnknownOperationKeyword()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFOperationParserFactory.ParseOperation("FOOBAR <http://example.org/g>"));

    #endregion
}
