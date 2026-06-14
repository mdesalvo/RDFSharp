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

namespace RDFSharp.Test.Query;

/// <summary>
/// Unit tests for the Modify/WHERE half of RDFOperationParser: DELETE WHERE (short and long forms),
/// INSERT … WHERE, and DELETE … INSERT … WHERE — template-plus-WHERE operations whose QuadPattern templates may
/// carry variables. WITH and USING dataset clauses are rejected as non-representable.
/// </summary>
public partial class RDFOperationParserTest
{
    #region Modify

    #region DELETE WHERE (short)

    [TestMethod]
    public void ShouldParseDeleteWhereShortForm()
    {
        RDFDeleteWhereOperation operation = RDFDeleteWhereOperation.FromString("DELETE WHERE { ?s ?p ?o }");

        //The QuadPattern doubles as both the delete templates and the WHERE pattern
        Assert.AreEqual(1, operation.DeleteTemplates.Count);
        Assert.AreEqual(1, operation.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldRoundTripDeleteWhereShortForm()
        => AssertOperationRoundTrips(RDFDeleteWhereOperation.FromString("DELETE WHERE { ?s ?p ?o . ?o ?q ?z }"));

    #endregion

    #region DELETE … WHERE (long)

    [TestMethod]
    public void ShouldParseDeleteLongForm()
    {
        RDFDeleteWhereOperation operation = RDFDeleteWhereOperation.FromString(
            "DELETE { ?s ?p ?o } WHERE { ?s ?p ?o . ?s ?p2 ?o2 }");

        Assert.AreEqual(1, operation.DeleteTemplates.Count);
        Assert.AreEqual(2, operation.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldRoundTripDeleteLongForm()
        => AssertOperationRoundTrips(new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))));

    #endregion

    #region INSERT … WHERE

    [TestMethod]
    public void ShouldParseInsertWhere()
    {
        RDFInsertWhereOperation operation = RDFInsertWhereOperation.FromString(
            "PREFIX ex: <http://example.org/> INSERT { ?s ex:copy ?o } WHERE { ?s ex:p ?o }");

        Assert.AreEqual(1, operation.InsertTemplates.Count);
        Assert.AreEqual(1, operation.GetPatternGroups().Single().GetPatterns().Count());
        Assert.AreEqual(1, operation.GetPrefixes().Count);
    }

    [TestMethod]
    public void ShouldRoundTripInsertWhere()
        => AssertOperationRoundTrips(new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(new RDFVariable("s"), new RDFResource("http://example.org/copy"), new RDFVariable("o")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFResource("http://example.org/p"), new RDFVariable("o")))));

    [TestMethod]
    public void ShouldParseInsertWhereWithUnionInWhere()
    {
        RDFInsertWhereOperation operation = RDFInsertWhereOperation.FromString(
            "INSERT { ?s ?p ?o } WHERE { { ?s ?p ?o } UNION { ?a ?b ?c } }");

        Assert.AreEqual(1, operation.InsertTemplates.Count);
    }

    #endregion

    #region DELETE … INSERT … WHERE

    [TestMethod]
    public void ShouldParseDeleteInsertWhere()
    {
        RDFDeleteInsertWhereOperation operation = RDFDeleteInsertWhereOperation.FromString(
            "DELETE { ?s ?p ?o } INSERT { ?s ?p ?o2 } WHERE { ?s ?p ?o }");

        Assert.AreEqual(1, operation.DeleteTemplates.Count);
        Assert.AreEqual(1, operation.InsertTemplates.Count);
        Assert.AreEqual(1, operation.GetPatternGroups().Single().GetPatterns().Count());
    }

    [TestMethod]
    public void ShouldRoundTripDeleteInsertWhere()
        => AssertOperationRoundTrips(new RDFDeleteInsertWhereOperation()
            .AddDeleteTemplate(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))
            .AddInsertTemplate(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o2")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")))));

    #endregion

    #region Failures

    [TestMethod]
    public void ShouldThrowOnModifyWithWithClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFDeleteWhereOperation.FromString("WITH <http://example.org/g> DELETE { ?s ?p ?o } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnModifyWithUsingClause()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFDeleteWhereOperation.FromString("DELETE { ?s ?p ?o } USING <http://example.org/g> WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowOnInsertWhereWithPropertyPathInTemplate()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFInsertWhereOperation.FromString(
                "PREFIX ex: <http://example.org/> INSERT { ?s ex:p/ex:q ?o } WHERE { ?s ?p ?o }"));

    [TestMethod]
    public void ShouldThrowWhenDeleteWhereFromStringFedAnInsertWhere()
        => Assert.ThrowsExactly<RDFQueryException>(() =>
            RDFDeleteWhereOperation.FromString("INSERT { ?s ?p ?o } WHERE { ?s ?p ?o }"));

    #endregion

    #endregion
}
