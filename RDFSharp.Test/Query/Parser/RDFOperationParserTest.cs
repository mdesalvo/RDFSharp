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
/// the matching RDFOperation, dispatching the graph-management operations (CREATE/DROP/COPY/MOVE/ADD), rejecting
/// the unsupported WITH/USING dataset clauses, multiple ';'-separated operations, and empty input. The grammar of
/// each operation form is exercised in the sibling partials (e.g. <c>RDFOperationParser.ClearLoadTest.cs</c>).
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
    public void ShouldDispatchInsertDataOperation()
        => Assert.IsInstanceOfType<RDFInsertDataOperation>(
            RDFOperationParserFactory.ParseOperation("INSERT DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> }"));

    [TestMethod]
    public void ShouldDispatchDeleteDataOperation()
        => Assert.IsInstanceOfType<RDFDeleteDataOperation>(
            RDFOperationParserFactory.ParseOperation("DELETE DATA { <http://example.org/s> <http://example.org/p> <http://example.org/o> }"));

    [TestMethod]
    public void ShouldDispatchInsertWhereOperation()
        => Assert.IsInstanceOfType<RDFInsertWhereOperation>(
            RDFOperationParserFactory.ParseOperation("INSERT { ?s ?p ?o } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldDispatchDeleteWhereShortFormOperation()
        => Assert.IsInstanceOfType<RDFDeleteWhereOperation>(
            RDFOperationParserFactory.ParseOperation("DELETE WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldDispatchDeleteWhereLongFormOperation()
        => Assert.IsInstanceOfType<RDFDeleteWhereOperation>(
            RDFOperationParserFactory.ParseOperation("DELETE { ?s ?p ?o } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldDispatchDeleteInsertWhereOperation()
        => Assert.IsInstanceOfType<RDFDeleteInsertWhereOperation>(
            RDFOperationParserFactory.ParseOperation("DELETE { ?s ?p ?o } INSERT { ?s ?p ?o2 } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnNonRepresentableWithClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFOperationParserFactory.ParseOperation("WITH <http://example.org/g> DELETE { ?s ?p ?o } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnNonRepresentableUsingClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFOperationParserFactory.ParseOperation("DELETE { ?s ?p ?o } USING <http://example.org/g> WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldDispatchCreateOperation()
        => Assert.IsInstanceOfType<RDFCreateOperation>(
            RDFOperationParserFactory.ParseOperation("CREATE GRAPH <http://example.org/g>"));

    [TestMethod]
    public void ShouldDispatchDropOperation()
        => Assert.IsInstanceOfType<RDFDropOperation>(
            RDFOperationParserFactory.ParseOperation("DROP GRAPH <http://example.org/g>"));

    [TestMethod]
    public void ShouldDispatchCopyOperation()
        => Assert.IsInstanceOfType<RDFCopyOperation>(
            RDFOperationParserFactory.ParseOperation("COPY <http://example.org/g1> TO <http://example.org/g2>"));

    [TestMethod]
    public void ShouldDispatchMoveOperation()
        => Assert.IsInstanceOfType<RDFMoveOperation>(
            RDFOperationParserFactory.ParseOperation("MOVE <http://example.org/g1> TO <http://example.org/g2>"));

    [TestMethod]
    public void ShouldDispatchAddOperation()
        => Assert.IsInstanceOfType<RDFAddOperation>(
            RDFOperationParserFactory.ParseOperation("ADD <http://example.org/g1> TO <http://example.org/g2>"));

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

    [TestMethod]
    public void ShouldParseOperationSetWithMultipleOperations()
    {
        RDFOperationSet operationSet = RDFOperationParserFactory.ParseOperationSet("CLEAR ALL ; CLEAR DEFAULT ; LOAD <http://example.org/data>");

        Assert.IsNotNull(operationSet);
        Assert.HasCount(3, operationSet.Operations);
        Assert.IsInstanceOfType<RDFClearOperation>(operationSet.Operations[0]);
        Assert.IsInstanceOfType<RDFClearOperation>(operationSet.Operations[1]);
        Assert.IsInstanceOfType<RDFLoadOperation>(operationSet.Operations[2]);
    }

    [TestMethod]
    public void ShouldParseOperationSetWithSingleOperation()
    {
        RDFOperationSet operationSet = RDFOperationParserFactory.ParseOperationSet("CLEAR ALL");

        Assert.IsNotNull(operationSet);
        Assert.HasCount(1, operationSet.Operations);
    }

    [TestMethod]
    public void ShouldParseOperationSetWithTrailingSeparator()
    {
        RDFOperationSet operationSet = RDFOperationParserFactory.ParseOperationSet("CLEAR ALL ; CLEAR DEFAULT ;");

        Assert.IsNotNull(operationSet);
        Assert.HasCount(2, operationSet.Operations);
    }

    [TestMethod]
    public void ShouldThrowOnParsingOperationSetWithMissingSeparator()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFOperationParserFactory.ParseOperationSet("CLEAR ALL CLEAR DEFAULT"));

    [TestMethod]
    public void ShouldThrowOnParsingOperationSetWithEmptyOperationInChain()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFOperationParserFactory.ParseOperationSet("CLEAR ALL ; ; CLEAR DEFAULT"));

    [TestMethod]
    public void ShouldParseOperationSetWithGraphManagementOperationInChain()
    {
        RDFOperationSet operationSet = RDFOperationParserFactory.ParseOperationSet("CLEAR ALL ; CREATE GRAPH <http://example.org/g> ; MOVE <http://example.org/g1> TO <http://example.org/g2>");

        Assert.IsNotNull(operationSet);
        Assert.HasCount(3, operationSet.Operations);
        Assert.IsInstanceOfType<RDFClearOperation>(operationSet.Operations[0]);
        Assert.IsInstanceOfType<RDFCreateOperation>(operationSet.Operations[1]);
        Assert.IsInstanceOfType<RDFMoveOperation>(operationSet.Operations[2]);
    }

    [TestMethod]
    public void ShouldThrowOnParsingOperationSetWithNonRepresentableOperationInChain()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFOperationParserFactory.ParseOperationSet("CLEAR ALL ; DELETE { ?s ?p ?o } USING <http://example.org/g> WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnParsingOperationSetWithOnlyPrologue()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFOperationParserFactory.ParseOperationSet("PREFIX ex: <http://example.org/>"));

    [TestMethod]
    public void ShouldThrowOnEmptyOperationSet()
        => Assert.ThrowsExactly<RDFQueryException>(() => RDFOperationParserFactory.ParseOperationSet("   "));

    #endregion
}
