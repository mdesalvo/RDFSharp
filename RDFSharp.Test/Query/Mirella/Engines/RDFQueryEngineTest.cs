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
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;
using WireMock.Util;
using WireMock.Types;
using System.Web;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFQueryEngineTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    //Every test starts its own mock endpoint: releasing it here keeps one host alive at a time,
    //since each leaked host holds a file watcher and exhausts the inotify instances of Linux machines
    [TestCleanup]
    public void Cleanup() { server?.Stop(); server?.Dispose(); }

    #region Tests
    [TestMethod]
    public void ShouldCreateQueryEngine()
    {
        RDFQueryEngine queryEngine = new RDFQueryEngine();

        Assert.IsNotNull(queryEngine);
        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.IsNotNull(queryEngine.QueryMemberResultTables);
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    //SELECT QUERY

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithResultsHavingCommonSPVariables()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithResultsHavingCommonSOVariables()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:pluto")),
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithResultsHavingCommonPOVariables()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:dogOf")),
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?X"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithResultsHavingCommonSPOVariables()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:pluto")),
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), new RDFVariable("?Y")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphUnionsThenOptional()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPattern patPluto = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patFido  = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patBalto = new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patPluto.Union(patFido).Union(patBalto))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphUnionsThenInner()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPattern patPluto = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patFido  = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patBalto = new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patPluto.Union(patFido).Union(patBalto))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphMinusPatternGroup()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup pg1 = new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFPatternGroup pg2 = new RDFPatternGroup()
                .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:balto")), new RDFVariable("?Y")));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pg1.Minus(pg2))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL("""
            SELECT *
            WHERE {
              {
                {
                  ?Y <ex:dogOf> ?X .
                  OPTIONAL { ?X <ex:hasName> ?N } .
                }
                MINUS
                {
                  BIND(<ex:balto> AS ?Y) .
                }
              }
            }
            ORDER BY ASC(?X)

            """), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphMinusPattern()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPattern patA = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patB = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFResource("ex:paperino"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patA.Minus(patB)));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL("""
            SELECT *
            WHERE {
              {
                { ?Y <ex:dogOf> ?X }
                MINUS
                { ?Y <ex:dogOf> <ex:paperino> }
              }
            }

            """), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphBindAndProjectionExpressions()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("BIND"))), new RDFVariable("?XBIND")))
                .AddBind(new RDFBind(new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_FLOAT))), new RDFVariable("?NEVERBOUND"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XBIND"))
            .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFStrLenExpression(new RDFVariable("?XBIND")))
            .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(5, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphBindAndProjectionExpressionsSortedByBind()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("BIND"))), new RDFVariable("?XBIND")))
                .AddBind(new RDFBind(new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_FLOAT))), new RDFVariable("?NEVERBOUND"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?XBIND"), RDFQueryEnums.RDFOrderByFlavors.DESC))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XBIND"))
            .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFStrLenExpression(new RDFVariable("?XBIND")))
            .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(5, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:whoeverBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:paperinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphBindAndProjectionExpressionsSortedByProjectionExpressions()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperinoo")),
            new RDFTriple(new RDFResource("ex:paperinoo"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("BIND"))), new RDFVariable("?XBIND")))
                .AddBind(new RDFBind(new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_FLOAT))), new RDFVariable("?NEVERBOUND"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?XBINDLENGTH"), RDFQueryEnums.RDFOrderByFlavors.DESC))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XBIND"))
            .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFStrLenExpression(new RDFVariable("?XBIND")))
            .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(5, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperinoo", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinooBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"16^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphBindAndProjectionExpressionsSortedByProjectionExpressionsThenByBind()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:doggy"),new RDFResource("ex:dogOf"),new RDFResource("ex:pippo")),
            new RDFTriple(new RDFResource("ex:pippo"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Goofy Goof", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperinoo")),
            new RDFTriple(new RDFResource("ex:paperinoo"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoev"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("BIND"))), new RDFVariable("?XBIND")))
                .AddBind(new RDFBind(new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_FLOAT))), new RDFVariable("?NEVERBOUND"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?XBINDLENGTH"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?XBIND"), RDFQueryEnums.RDFOrderByFlavors.DESC))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XBIND"))
            .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFStrLenExpression(new RDFVariable("?XBIND")))
            .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(5, result.SelectResults.Columns.Count);
        Assert.AreEqual(4, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:whoev", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:whoevBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"12^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:doggy", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:pippo", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:pippoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"12^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:topolinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?X"].ToString(), "ex:paperinoo", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XBIND"].ToString(), "ex:paperinooBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XBINDLENGTH"].ToString(), $"16^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphBindAndProjectionExpressionsSortedByBindThenByProjectionExpressions()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:doggy"),new RDFResource("ex:dogOf"),new RDFResource("ex:pippo")),
            new RDFTriple(new RDFResource("ex:pippo"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Goofy Goof", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperinoo")),
            new RDFTriple(new RDFResource("ex:paperinoo"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoev"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("BIND"))), new RDFVariable("?XBIND")))
                .AddBind(new RDFBind(new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_FLOAT))), new RDFVariable("?NEVERBOUND"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?XBIND"), RDFQueryEnums.RDFOrderByFlavors.DESC))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?XBINDLENGTH"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XBIND"))
            .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFStrLenExpression(new RDFVariable("?XBIND")))
            .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(5, result.SelectResults.Columns.Count);
        Assert.AreEqual(4, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:whoev", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:whoevBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"12^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:doggy", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:pippo", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:pippoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"12^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?X"].ToString(), "ex:paperinoo", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XBIND"].ToString(), "ex:paperinooBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XBINDLENGTH"].ToString(), $"16^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphProjectionExpressionsSortedByUnboundProjectionExpressions()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:doggy"),new RDFResource("ex:dogOf"),new RDFResource("ex:pippo")),
            new RDFTriple(new RDFResource("ex:pippo"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Goofy Goof", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperinoo")),
            new RDFTriple(new RDFResource("ex:paperinoo"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoev"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?XFLOOR"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddProjectionVariable(new RDFVariable("?XFLOOR"), new RDFFloorExpression(new RDFVariable("?X")));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(4, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XFLOOR"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XFLOOR"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XFLOOR"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XFLOOR"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphStarWithBind()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("BIND"))), new RDFVariable("?XBIND")))
                .AddBind(new RDFBind(new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_FLOAT))), new RDFVariable("?NEVERBOUND"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?XBIND"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(5, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableConstantExpressionBoundValue()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:pluto")), new RDFVariable("?Y"))));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableConstantExpressionProjectedValue()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?Y"), new RDFConstantExpression(new RDFResource("ex:pluto")));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableNowExpressionProjectedValue()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?Y"), new RDFNowExpression());
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsFalse(string.IsNullOrEmpty(result.SelectResults.Rows[0]["?Y"].ToString()));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableComplexVariableLessExpressionProjectedValue1()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?Y"), new RDFStrLenExpression(
                new RDFConcatExpression(new RDFConstantExpression(new RDFPlainLiteral("hello","en-US")),
                    new RDFStrLenExpression(new RDFMD5Expression(new RDFConstantExpression(new RDFPlainLiteral("hello","en-US")))))));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals(new RDFTypedLiteral("7", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableComplexVariableLessExpressionProjectedValue2()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?Y"), new RDFAddExpression(
                new RDFStrLenExpression(new RDFConstantExpression(new RDFPlainLiteral("hello", "en-US"))),
                new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("3", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
                    new RDFMultiplyExpression(new RDFConstantExpression(new RDFTypedLiteral("3", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)), new RDFTypedLiteral("4.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals(new RDFTypedLiteral("21.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL).ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableComplexVariableLessExpressionProjectedValue3()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?Y"), new RDFIfExpression(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFRandExpression(), new RDFConstantExpression(new RDFTypedLiteral("0.50", RDFModelEnums.RDFDatatypes.XSD_FLOAT))),
                new RDFConstantExpression(new RDFPlainLiteral(">0.50")),
                new RDFConstantExpression(new RDFPlainLiteral("<=0.50"))));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals(">0.50", StringComparison.Ordinal) || result.SelectResults.Rows[0]["?Y"].ToString().Equals("<=0.50", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableRandExpressionProjectedValue()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?Y"), new RDFRandExpression());
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsFalse(string.IsNullOrEmpty(result.SelectResults.Rows[0]["?Y"].ToString()));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableFloorExpressionProjectedValue()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddProjectionVariable(new RDFVariable("?Y"), new RDFFloorExpression(new RDFConstantExpression(new RDFTypedLiteral("3.35", RDFModelEnums.RDFDatatypes.XSD_FLOAT))));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals($"3^^{RDFVocabulary.XSD.FLOAT}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableFloorExpressionProjectedValueFromSubQuery1()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddSubQuery(new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFFloorExpression(new RDFConstantExpression(new RDFTypedLiteral("3.35", RDFModelEnums.RDFDatatypes.XSD_FLOAT)))))
            .AddProjectionVariable(new RDFVariable("?Z"), new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(result.SelectResults.Rows[0]["?Z"].ToString().Equals($"4^^{RDFVocabulary.XSD.FLOAT}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphEmptyTableFloorExpressionProjectedValueFromSubQuery2()
    {
        RDFGraph graph = new RDFGraph();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddSubQuery(new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFFloorExpression(new RDFConstantExpression(new RDFTypedLiteral("3.35", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
                .AddProjectionVariable(new RDFVariable("?Z"), new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals($"3^^{RDFVocabulary.XSD.FLOAT}", StringComparison.Ordinal));
        Assert.IsTrue(result.SelectResults.Rows[0]["?Z"].ToString().Equals($"4^^{RDFVocabulary.XSD.FLOAT}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithComplexQuery1()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:baubau"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperoga")),
            new RDFTriple(new RDFResource("ex:paperoga"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("white")),
            new RDFTriple(new RDFResource("ex:snoopie"),new RDFResource("ex:dogOf"),new RDFResource("ex:linus"))
        ]);

        RDFPattern patPluto1 = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patFido1 = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patBalto1 = new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patBaubau1 = new RDFPattern(new RDFResource("ex:baubau"), new RDFResource("ex:dogOf"), new RDFVariable("?Y"));
        RDFPattern patSnoopie1 = new RDFPattern(new RDFResource("ex:snoopie"), new RDFResource("ex:dogOf"), new RDFVariable("?Y"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patPluto1.Union(patFido1).Union(patBalto1))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddBinaryPatternGroupMember(patBaubau1.Union(patSnoopie1))
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))))
                .Optional())
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(4, result.SelectResults.Columns.Count);
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:paperoga", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "white", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:paperoga", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?C"].ToString(), "white", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithComplexQuery2()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:baubau"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperoga")),
            new RDFTriple(new RDFResource("ex:paperoga"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("white")),
            new RDFTriple(new RDFResource("ex:snoopie"),new RDFResource("ex:dogOf"),new RDFResource("ex:linus"))
        ]);

        RDFPattern patPluto2 = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patFido2 = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patBalto2 = new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patBaubau2 = new RDFPattern(new RDFResource("ex:baubau"), new RDFResource("ex:dogOf"), new RDFVariable("?Y"));
        RDFPattern patSnoopie2 = new RDFPattern(new RDFResource("ex:snoopie"), new RDFResource("ex:dogOf"), new RDFVariable("?Y"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patPluto2.Union(patFido2).Union(patBalto2))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddBinaryPatternGroupMember(patBaubau2.Union(patSnoopie2))
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasColor"), new RDFVariable("?C")).Optional()))
                .Optional())
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(4, result.SelectResults.Columns.Count);
        Assert.AreEqual(4, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:paperoga", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "white", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:linus", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?C"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:paperoga", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?C"].ToString(), "white", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?Y"].ToString(), "ex:linus", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?C"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithComplexQuery3()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:baubau"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperoga")),
            new RDFTriple(new RDFResource("ex:paperoga"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("white")),
            new RDFTriple(new RDFResource("ex:snoopie"),new RDFResource("ex:dogOf"),new RDFResource("ex:linus"))
        ]);

        RDFPattern patPluto3 = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patFido3 = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patBalto3 = new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patBaubau3 = new RDFPattern(new RDFResource("ex:baubau"), new RDFResource("ex:dogOf"), new RDFVariable("?Y"));
        RDFPattern patSnoopie3 = new RDFPattern(new RDFResource("ex:snoopie"), new RDFResource("ex:dogOf"), new RDFVariable("?Y"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patPluto3.Union(patFido3).Union(patBalto3))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddBinaryPatternGroupMember(patBaubau3.Union(patSnoopie3))
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasColor"), new RDFVariable("?C")).Optional()))
                .Optional())
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(4, result.SelectResults.Columns.Count);
        Assert.AreEqual(6, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:paperoga", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "white", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:linus", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?C"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:paperoga", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?C"].ToString(), "white", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?Y"].ToString(), "ex:linus", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?C"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[4]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[4]["?N"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[4]["?Y"].ToString(), "ex:paperoga", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[4]["?C"].ToString(), "white", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[5]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[5]["?N"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[5]["?Y"].ToString(), "ex:linus", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[5]["?C"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithComplexQuery4()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:baubau"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperoga")),
            new RDFTriple(new RDFResource("ex:paperoga"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("white")),
            new RDFTriple(new RDFResource("ex:snoopie"),new RDFResource("ex:dogOf"),new RDFResource("ex:linus"))
        ]);

        RDFPatternGroup pg4a = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sq4 = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFPatternGroup pg4b = new RDFPatternGroup()
            .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [ new RDFResource("ex:balto") ])
                .AddColumn(new RDFVariable("?X"), [ new RDFResource("ex:whoever") ]));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pg4a.Minus(sq4.Union(pg4b)))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?N"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:snoopie", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:linus", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(query.ToString()),
            RDFTestUtilities.NormalizeEOL("""
            SELECT ?Y ?X ?N
            WHERE {
              {
                {
                  ?Y <ex:dogOf> ?X .
                  OPTIONAL { ?X <ex:hasName> ?N } .
                }
                MINUS
                {
                {
                  SELECT *
                  WHERE {
                    {
                      ?Y <ex:dogOf> ?X .
                      ?X <ex:hasColor> ?C .
                    }
                  }
                }
                  UNION
                  {
                    VALUES (?Y ?X) {
                      ( <ex:balto> <ex:whoever> )
                    } .
                  }
                }
              }
            }
            ORDER BY ASC(?X)

            """), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithNoResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(0, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithSingleEmptySolutionBecauseNoEvaluableQueryMembers()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery();
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(0, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResults()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonCSVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonCPVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:ctx"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Y"), new RDFVariable("?C"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(4, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonCOVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:ctx")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?C")))
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonCSPVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFVariable("?C"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonCSOVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:dogOf"),new RDFResource("ex:ctx")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFResource("ex:dogOf"), new RDFVariable("?C")))
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?C"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonCPOVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:ctx"),new RDFResource("ex:ctx")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Y"), new RDFVariable("?C"), new RDFVariable("?C")))
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonCSPOVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:ctx")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFVariable("?C"), new RDFVariable("?C")))
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonSPVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonSOVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:pluto")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonPOVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:dogOf")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?X"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(3, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreWithResultsHavingCommonSPOVariables()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:pluto")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), new RDFVariable("?Y")))
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(2, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnStoreBindAndProjectionExpressions()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("BIND"))), new RDFVariable("?XBIND")))
                .AddBind(new RDFBind(new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_FLOAT))), new RDFVariable("?NEVERBOUND"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XBIND"))
            .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFStrLenExpression(new RDFVariable("?XBIND")))
            .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(5, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnFederationWithResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino"))
        ]);
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFFederation federation = new RDFFederation().AddGraph(graph).AddStore(store);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(4, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?C"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), DBNull.Value.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnFederationBindAndProjectionExpressions()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino"))
        ]);
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFFederation federation = new RDFFederation().AddGraph(graph).AddStore(store);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("BIND"))), new RDFVariable("?XBIND")))
                .AddBind(new RDFBind(new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_FLOAT))), new RDFVariable("?NEVERBOUND"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddProjectionVariable(new RDFVariable("?Y"))
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XBIND"))
            .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFStrLenExpression(new RDFVariable("?XBIND")))
            .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, federation);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.SelectResults);
        Assert.AreEqual(5, result.SelectResults.Columns.Count);
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnFederationWithResultsSPARQLEndpoints()
    {
        string receivedQuery1 = "";
        string receivedQuery2 = "";

        const string mockedResponseXml1 =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:pluto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:topolino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:fido</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:paperino</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;
        const string mockedResponseXml2 =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:balto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:whoever</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;

        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpoints1/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery1 = req.RawQuery;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponseXml1,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpoints2/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery2 = req.Body;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponseXml2,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint1 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpoints1/sparql"));
        RDFSPARQLEndpoint endpoint2 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpoints2/sparql"));
        RDFFederation federation = new RDFFederation()
            .AddSPARQLEndpoint(endpoint1)
            .AddSPARQLEndpoint(endpoint2, new RDFSPARQLEndpointQueryOptions { QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post });
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));
        DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, federation).SelectResults;

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual(3, result.Rows.Count);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));

        string qString = query.ToString();
        Assert.IsNotNull(receivedQuery1);
        Assert.IsTrue(string.Equals(receivedQuery1, $"?query={HttpUtility.UrlEncode(qString)}", StringComparison.Ordinal));
        Assert.IsNotNull(receivedQuery2);
        Assert.IsTrue(string.Equals(receivedQuery2, $"query={HttpUtility.UrlEncode(qString)}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQuryOnFederationWithResultsSPARQLEndpointsOneGivingEmptyResult()
    {
        string receivedQuery1 = "";
        string receivedQuery2 = "";

        const string mockedResponseXml1 =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:pluto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:topolino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:fido</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:paperino</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;
        const string mockedResponseXml2 =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:balto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:whoever</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;

        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneGivingEmptyResult1/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery1 = req.RawQuery;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponseXml1,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneGivingEmptyResult2/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery2 = req.Body;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponseXml2,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(200));

        RDFSPARQLEndpoint endpoint1 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneGivingEmptyResult1/sparql"));
        RDFSPARQLEndpoint endpoint2 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneGivingEmptyResult2/sparql"));
        RDFFederation federation = new RDFFederation()
            .AddSPARQLEndpoint(endpoint1)
            .AddSPARQLEndpoint(endpoint2, new RDFSPARQLEndpointQueryOptions {
                ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult,
                QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post,
                TimeoutMilliseconds = 100 });
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));
        DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, federation).SelectResults;

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual(2, result.Rows.Count);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));

        string qString = query.ToString();
        Assert.IsNotNull(receivedQuery1);
        Assert.IsTrue(string.Equals(receivedQuery1, $"?query={HttpUtility.UrlEncode(qString)}", StringComparison.Ordinal));
        Assert.IsNotNull(receivedQuery2);
        Assert.IsTrue(string.Equals(receivedQuery2, string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueyOnFederationWithResultsSPARQLEndpointsOneThrowingException()
    {
        const string mockedResponseXml1 =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:pluto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:topolino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:fido</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:paperino</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;
        const string mockedResponseXml2 =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:balto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:whoever</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;

        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException1/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(_ => new WireMock.ResponseMessage
                    {
                        BodyData = new BodyData
                        {
                            BodyAsString = mockedResponseXml1,
                            Encoding = Encoding.UTF8,
                            DetectedBodyType = BodyType.String
                        }
                    })
                    .WithStatusCode(HttpStatusCode.OK));
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException2/sparql")
                    .UsingPost())
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(_ => new WireMock.ResponseMessage
                    {
                        BodyData = new BodyData
                        {
                            BodyAsString = mockedResponseXml2,
                            Encoding = Encoding.UTF8,
                            DetectedBodyType = BodyType.String
                        }
                    })
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(200));

        RDFSPARQLEndpoint endpoint1 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException1/sparql"));
        RDFSPARQLEndpoint endpoint2 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException2/sparql"));
        RDFFederation federation = new RDFFederation()
            .AddSPARQLEndpoint(endpoint1)
            .AddSPARQLEndpoint(endpoint2, new RDFSPARQLEndpointQueryOptions {
                ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException,
                QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post,
                TimeoutMilliseconds = 100 });
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));

        Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFQueryEngine().EvaluateSelectQuery(query, federation));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithServicePatternGroup()
    {
        string receivedQuery = "";
        string expectedQuery = string.Concat("?query=SELECT *", Environment.NewLine, "WHERE {", Environment.NewLine, "  {", Environment.NewLine, "    ?Y <ex:dogOf> ?X .", Environment.NewLine, "  }", Environment.NewLine, "}", Environment.NewLine);
        const string mockedResponseXml =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:pluto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:topolino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:fido</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:paperino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:balto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:whoever</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithServicePatternGroup/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery = req.RawQuery;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponseXml,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithServicePatternGroup/sparql"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(endpoint, new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))));
        DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual(3, result.Rows.Count);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));

        //Proves that the pattern group has been sent as an equivalent SELECT * to the given endpoint
        Assert.IsNotNull(receivedQuery);
        Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery), expectedQuery, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithVariableEndpointService()
    {
        const string mockedResponseXml =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y"><uri>ex:pluto</uri></binding>
                  <binding name="?X"><uri>ex:topolino</uri></binding>
                </result>
              </results>
            </sparql>
            """;
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithVariableEndpointService/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(_ => new WireMock.ResponseMessage
                    {
                        BodyData = new BodyData { BodyAsString = mockedResponseXml, Encoding = Encoding.UTF8, DetectedBodyType = BodyType.String }
                    })
                    .WithStatusCode(HttpStatusCode.OK));

        RDFResource endpointResource = new RDFResource(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithVariableEndpointService/sparql");
        //A VALUES block binds the endpoint variable ?EP, which a deferred SERVICE ?EP then resolves at runtime
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddValues(new RDFValues().AddColumn(new RDFVariable("?EP"), [endpointResource])))
            .AddService(new RDFService(new RDFVariable("?EP"), new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))));
        DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        //The endpoint variable is bound, on every returned row, to the resolved endpoint IRI
        Assert.IsTrue(string.Equals(result.Rows[0]["?EP"].ToString(), endpointResource.ToString(), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowOnVariableEndpointServiceNotBoundAndNotSilent()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(new RDFVariable("?EP"), new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))));

        Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()));
    }

    [TestMethod]
    public void ShouldGiveEmptyResultOnVariableEndpointServiceNotBoundAndSilent()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(new RDFVariable("?EP"), new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))),
                new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult }));
        DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Rows.Count);
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithValuesInjectedIntoServicePatternGroup()
    {
        string receivedQuery = "";
        const string expectedQuery =
            """
            ?query=SELECT *
            WHERE {
              {
                VALUES (?Y ?X) {
                  ( <ex:pluto> <ex:topolino> )
                } .
                ?Y <ex:dogOf> ?X .
              }
            }

            """;
        const string mockedResponseXml =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:pluto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:topolino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:fido</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:paperino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:balto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:whoever</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithValuesInjectedIntoServicePatternGroup/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery = req.RawQuery;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponseXml,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithValuesInjectedIntoServicePatternGroup/sparql"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(endpoint, new RDFPatternGroup()
                .AddValues(new RDFValues()
                    .AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")])
                    .AddColumn(new RDFVariable("?X"), [new RDFResource("ex:topolino")]))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))));
        DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual(3, result.Rows.Count);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal)); //Here it is mocked in XML, but real SPARQL servers will have it thanks to injected VALUES
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));

        //Proves that the pattern group has been sent as an equivalent SELECT * to the given endpoint
        Assert.IsNotNull(receivedQuery);
        Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(HttpUtility.UrlDecode(receivedQuery)), RDFTestUtilities.NormalizeEOL(expectedQuery), StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithServicePatternGroupAndCombineResults()
    {
        string receivedQuery = "";
        string expectedQuery = string.Concat("?query=SELECT *", Environment.NewLine, "WHERE {", Environment.NewLine, "  {", Environment.NewLine, "    ?Y <ex:dogOf> ?X .", Environment.NewLine, "  }", Environment.NewLine, "}", Environment.NewLine);
        const string mockedResponseXml =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:pluto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:topolino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:fido</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:paperino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:balto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:whoever</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithServicePatternGroupAndCombineResults/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery = req.RawQuery;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponseXml,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithServicePatternGroupAndCombineResults/sparql"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:topolino")), new RDFVariable("?X"))))
            .AddService(new RDFService(endpoint, new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))));
        DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));

        //Proves that the pattern group has been sent as an equivalent SELECT * to the given endpoint
        Assert.IsNotNull(receivedQuery);
        Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery), expectedQuery, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateSelectQueryOnGraphWithDifferentServicePatternGroupsAndCombineResults()
    {
        string receivedQuery1 = "";
        string expectedQuery1 = string.Concat("?query=SELECT *", Environment.NewLine, "WHERE {", Environment.NewLine, "  {", Environment.NewLine, "    ?Y <ex:dogOf> ?X .", Environment.NewLine, "  }", Environment.NewLine, "}", Environment.NewLine);
        const string mockedResponse1Xml =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:pluto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:topolino</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;
        string receivedQuery2 = "";
        string expectedQuery2 = string.Concat("?query=SELECT *", Environment.NewLine, "WHERE {", Environment.NewLine, "  {", Environment.NewLine, "    ?Y <ex:dogOf> ?X .", Environment.NewLine, "  }", Environment.NewLine, "}", Environment.NewLine);
        const string mockedResponse2Xml =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <sparql xmlns="http://www.w3.org/2005/sparql-results#">
              <head>
                <variable name="?Y" />
                <variable name="?X" />
              </head>
              <results>
                <result>
                  <binding name="?Y">
                    <uri>ex:pluto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:topolino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:fido</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:paperino</uri>
                  </binding>
                </result>
                <result>
                  <binding name="?Y">
                    <uri>ex:balto</uri>
                  </binding>
                  <binding name="?X">
                    <uri>ex:whoever</uri>
                  </binding>
                </result>
              </results>
            </sparql>
            """;
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithDifferentServicePatternGroupsAndCombineResults1/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery1 = req.RawQuery;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponse1Xml,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));

        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithDifferentServicePatternGroupsAndCombineResults2/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery2 = req.RawQuery;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponse2Xml,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint1 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithDifferentServicePatternGroupsAndCombineResults1/sparql"));
        RDFSPARQLEndpoint endpoint2 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnGraphWithDifferentServicePatternGroupsAndCombineResults2/sparql"));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddService(new RDFService(endpoint1, new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))))
            .AddService(new RDFService(endpoint2, new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))));
        DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Columns.Count);
        Assert.AreEqual(1, result.Rows.Count);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));

        //Proves that the pattern groups have been sent as equivalent SELECT * to the given endpoints
        Assert.IsNotNull(receivedQuery1);
        Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery1), expectedQuery1, StringComparison.Ordinal));
        Assert.IsNotNull(receivedQuery2);
        Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery2), expectedQuery2, StringComparison.Ordinal));
    }

    //DESCRIBE QUERY

    [TestMethod]
    public void ShouldEvaluateDescribeQueryWithResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?Y"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFLimitModifier(2));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryOnStoreWithResultsFromvariableTerms()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?Y"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFLimitModifier(2));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx2", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryOnStoreWithResultsFromVariableTermAssumingBlankValues()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("bnode:12345"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?Y"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFLimitModifier(2));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "bnode:12345", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx2", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryOnStoreWithResultsFromResourceTerm()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:pluto"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFLimitModifier(2));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryOnStoreWithResultsFromContextTerm()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:ctx1"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.AreEqual(2, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx1", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?SUBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?PREDICATE"].ToString(), "ex:hasName", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?OBJECT"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryOnStoreWithResultsFromBlankResourceTerm()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("bnode:12345"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx1"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx2"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("bnode:12345"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFLimitModifier(2));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(4, result.DescribeResults.Columns.Count);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "bnode:12345", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryWithResultsFromResourceTerm()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:pluto"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryWithResultsFromBlankResourceTerm()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("bnode:12345"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("bnode:12345"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "bnode:12345", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryWithResultsFromResourceTermOnly()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:pluto"));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.AreEqual(1, result.DescribeResultsCount);
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryWithNoResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?Y"))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.AreEqual(0, result.DescribeResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryWithNoResultsBecauseNoEvaluableQueryMembersAndVariableDescribeTerm()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
        ]);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?Y"));
        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.DescribeResults);
        Assert.AreEqual(3, result.DescribeResults.Columns.Count);
        Assert.AreEqual(0, result.DescribeResultsCount);
    }

    //CONSTRUCT QUERY

    [TestMethod]
    public void ShouldEvaluateConstructQueryWithResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFConstructQuery query = new RDFConstructQuery()
            .AddTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFLimitModifier(2));
        RDFConstructQueryResult result = new RDFQueryEngine().EvaluateConstructQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ConstructResults);
        Assert.AreEqual(3, result.ConstructResults.Columns.Count);
        Assert.AreEqual(2, result.ConstructResultsCount);
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[1]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateConstructQueryWithResultsFromGroundTemplate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFConstructQuery query = new RDFConstructQuery()
            .AddTemplate(new RDFPattern(new RDFResource("ex:pluto"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFLimitModifier(2));
        RDFConstructQueryResult result = new RDFQueryEngine().EvaluateConstructQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ConstructResults);
        Assert.AreEqual(3, result.ConstructResults.Columns.Count);
        Assert.AreEqual(1, result.ConstructResultsCount);
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateConstructQueryWithNoResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFConstructQuery query = new RDFConstructQuery()
            .AddTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFConstructQueryResult result = new RDFQueryEngine().EvaluateConstructQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ConstructResults);
        Assert.AreEqual(3, result.ConstructResults.Columns.Count);
        Assert.AreEqual(0, result.ConstructResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateConstructQueryWithNoResultsBecauseNoEvaluableQueryMembers()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
        ]);

        RDFConstructQuery query = new RDFConstructQuery()
            .AddTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
        RDFConstructQueryResult result = new RDFQueryEngine().EvaluateConstructQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.ConstructResults);
        Assert.AreEqual(3, result.ConstructResults.Columns.Count);
        Assert.AreEqual(0, result.ConstructResultsCount);
    }

    //ASK QUERY

    [TestMethod]
    public void ShouldEvaluateAskQueryTrue()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFAskQuery query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.AskResult);
    }

    [TestMethod]
    public void ShouldEvaluateAskQueryFalse()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFAskQuery query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsFalse(result.AskResult);
    }

    [TestMethod]
    public void ShouldEvaluateAskQueryTrueBecauseNoEvaluableQueryMembers()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
        ]);

        RDFAskQuery query = new RDFAskQuery();
        RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.AskResult);
    }

    //UTILITIES

    [TestMethod]
    public void ShouldEvaluateQueryMembersWithResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddFilter(new RDFFilter(new RDFBoundExpression(new RDFVariable("?N")))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")]))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        List<RDFQueryMember> evaluableQueryMembers = [.. query.GetEvaluableQueryMembers()];

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluateQueryMembers(evaluableQueryMembers, graph);

        Assert.IsNotNull(queryEngine.QueryMemberResultTables);
        Assert.HasCount(2, queryEngine.QueryMemberResultTables);
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value != null);
        Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.IsOptional);
        Assert.AreEqual(3, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count);
        Assert.AreEqual(2, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value != null);
        Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.IsOptional);
        Assert.AreEqual(1, queryEngine.QueryMemberResultTables.ElementAt(1).Value.Columns.Count);
        Assert.AreEqual(1, queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluateQueryMembersWithResultsAndExtendedPropertiesOpUn()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup pgOpUn = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
            .AddFilter(new RDFFilter(new RDFBoundExpression(new RDFVariable("?N"))))
            .Optional();
        RDFSelectQuery sqOpUn = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")])));
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgOpUn.Union(sqOpUn))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        List<RDFQueryMember> evaluableQueryMembers = [.. query.GetEvaluableQueryMembers()];

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluateQueryMembers(evaluableQueryMembers, graph);

        Assert.IsNotNull(queryEngine.QueryMemberResultTables);
        Assert.HasCount(1, queryEngine.QueryMemberResultTables);
        var opUnTable = queryEngine.QueryMemberResultTables.ElementAt(0).Value;
        Assert.IsNotNull(opUnTable);
        Assert.IsTrue((bool)opUnTable.IsOptional);
        Assert.AreEqual(3, opUnTable.Columns.Count);
        Assert.AreEqual(3, opUnTable.Rows.Count);
        Assert.IsTrue(string.Equals(opUnTable.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(opUnTable.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(opUnTable.Rows[0]["?N"], "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(opUnTable.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(opUnTable.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(opUnTable.Rows[1]["?N"], "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(opUnTable.Rows[2]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsNull(opUnTable.Rows[2]["?X"]);
        Assert.IsNull(opUnTable.Rows[2]["?N"]);
    }

    [TestMethod]
    public void ShouldEvaluateQueryMembersWithResultsAndExtendedPropertiesUnOp()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup pgUnOp = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
            .AddFilter(new RDFFilter(new RDFBoundExpression(new RDFVariable("?N"))));
        RDFSelectQuery sqUnOp = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")])))
            .Optional();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgUnOp.Union(sqUnOp))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        List<RDFQueryMember> evaluableQueryMembers = [.. query.GetEvaluableQueryMembers()];

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluateQueryMembers(evaluableQueryMembers, graph);

        Assert.IsNotNull(queryEngine.QueryMemberResultTables);
        Assert.HasCount(1, queryEngine.QueryMemberResultTables);
        var unOpTable = queryEngine.QueryMemberResultTables.ElementAt(0).Value;
        Assert.IsNotNull(unOpTable);
        Assert.IsFalse((bool)unOpTable.IsOptional);
        Assert.AreEqual(3, unOpTable.Columns.Count);
        Assert.AreEqual(3, unOpTable.Rows.Count);
        Assert.IsTrue(string.Equals(unOpTable.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(unOpTable.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(unOpTable.Rows[0]["?N"], "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(unOpTable.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(unOpTable.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(unOpTable.Rows[1]["?N"], "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(unOpTable.Rows[2]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsNull(unOpTable.Rows[2]["?X"]);
        Assert.IsNull(unOpTable.Rows[2]["?N"]);
    }

    [TestMethod]
    public void ShouldEvaluateQueryMembersWithNoResults()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddFilter(new RDFFilter(new RDFBoundExpression(new RDFVariable("?N")))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")]))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
        List<RDFQueryMember> evaluableQueryMembers = [.. query.GetEvaluableQueryMembers()];

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluateQueryMembers(evaluableQueryMembers, graph);

        Assert.IsNotNull(queryEngine.QueryMemberResultTables);
        Assert.HasCount(2, queryEngine.QueryMemberResultTables);
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value != null);
        Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.IsOptional);
        Assert.AreEqual(3, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count);
        Assert.AreEqual(0, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count);
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value != null);
        Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.IsOptional);
        Assert.AreEqual(1, queryEngine.QueryMemberResultTables.ElementAt(1).Value.Columns.Count);
        Assert.AreEqual(1, queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluatePatternGroupWithResultsFromPattern()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph);

        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(2, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value);
        //Optimizer places ex:hasName pattern first (cardinality 2 < cardinality 3 of ex:dogOf)
        Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0] != null);
        Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].IsOptional);
        Assert.AreEqual(2, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count);
        Assert.AreEqual(2, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1] != null);
        Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].IsOptional);
        Assert.AreEqual(2, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Columns.Count);
        Assert.AreEqual(3, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluatePatternGroupWithNoResultsFromPattern()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPattern patNoRes1 = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X"));
        RDFPattern patNoRes2 = new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasName"), new RDFVariable("?N"));
        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddBinaryPatternGroupMember(patNoRes1.Union(patNoRes2.Optional()));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph);

        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value);
        var noResTable = queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0];
        Assert.IsNotNull(noResTable);
        Assert.IsFalse((bool)noResTable.IsOptional);
        Assert.AreEqual(3, noResTable.Columns.Count);
        Assert.AreEqual(0, noResTable.Rows.Count);
    }

    [TestMethod]
    public void ShouldEvaluatePatternGroupWithResultsFromPropertyPath()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?N"))
                .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:dogOf")))
                .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:hasName"))));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph);

        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value);
        Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0] != null);
        Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].IsOptional);
        Assert.AreEqual(2, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count);
        Assert.AreEqual(2, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluatePatternGroupWithNoResultsFromPropertyPath()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?N"))
                .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:dogOf")))
                .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:hasName2"))));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph);

        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value);
        Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0] != null);
        Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].IsOptional);
        Assert.AreEqual(2, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count);
        Assert.AreEqual(0, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count);
    }

    [TestMethod]
    public void ShouldEvaluatePatternGroupWithResultsFromValues()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")]));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph);

        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value);
        Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0] != null);
        Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].IsOptional);
        Assert.AreEqual(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count);
        Assert.AreEqual(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        //VALUES is now a pure join member: no dedicated filter is injected into the pattern group anymore
        Assert.IsFalse(patternGroup.GetFilters().Any());
    }

    [TestMethod]
    public void ShouldEvaluatePatternGroupWithResultsFromUndefValues()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [null])); //one UNDEF row
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph);

        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value);
        Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0] != null);
        Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].IsOptional); //UNDEF does not make the block optional
        Assert.AreEqual(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count);
        Assert.AreEqual(1, queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?Y"] ?? string.Empty, string.Empty, StringComparison.Ordinal));
        //VALUES is now a pure join member: no dedicated filter is injected into the pattern group anymore
        Assert.IsFalse(patternGroup.GetFilters().Any());
    }

    [TestMethod]
    public void ShouldEvaluateBGPThroughPureInnerJoinFastPath()
    {
        //Two fully-bound, non-optional patterns sharing ?o => CombineTables takes the strict inner-join fast-path.
        //The end-to-end result must be exactly the equi-join (Alice knows Bob, Bob is 30).
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:alice"), new RDFResource("ex:knows"), new RDFResource("ex:bob")),
            new RDFTriple(new RDFResource("ex:bob"), new RDFResource("ex:age"), new RDFTypedLiteral("30", RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
            new RDFTriple(new RDFResource("ex:alice"), new RDFResource("ex:age"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?s"), new RDFResource("ex:knows"), new RDFVariable("?o")))
                .AddPattern(new RDFPattern(new RDFVariable("?o"), new RDFResource("ex:age"), new RDFVariable("?age"))));

        DataTable results = query.ApplyToGraph(graph).SelectResults;
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("ex:alice", results.Rows[0]["?s"].ToString());
        Assert.AreEqual("ex:bob", results.Rows[0]["?o"].ToString());
        Assert.IsTrue(results.Rows[0]["?age"].ToString().StartsWith("30", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldInnerJoinFullyBoundSubQueryWithMother()
    {
        //A subselect that joins INNER with its mother: the outer pattern '?s ex:knows ?o' joins on ?s with the
        //subquery 'SELECT ?s WHERE { ?s a ex:Person }'. Both are fully bound and non-optional, so CombineTables
        //picks the strict inner-join: only Alice (a Person who knows Bob) survives; Carol (not a Person) is dropped.
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:alice"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")),
            new RDFTriple(new RDFResource("ex:bob"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person")),
            new RDFTriple(new RDFResource("ex:alice"), new RDFResource("ex:knows"), new RDFResource("ex:bob")),
            new RDFTriple(new RDFResource("ex:carol"), new RDFResource("ex:knows"), new RDFResource("ex:alice"))
        ]);

        RDFSelectQuery subQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?s"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:Person"))))
            .AddProjectionVariable(new RDFVariable("?s"));

        //The subquery result is fully bound (BGP binds ?s, projection keeps an existing column) => inner-eligible
        RDFTable subQueryTable = new RDFQueryEngine().EvaluateSelectQueryToTable(subQuery, graph);
        Assert.IsTrue(subQueryTable.IsFullyBound);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?s"), new RDFResource("ex:knows"), new RDFVariable("?o"))))
            .AddSubQuery(subQuery);

        DataTable results = query.ApplyToGraph(graph).SelectResults;
        Assert.AreEqual(1, results.Rows.Count);
        Assert.AreEqual("ex:alice", results.Rows[0]["?s"].ToString());
        Assert.AreEqual("ex:bob", results.Rows[0]["?o"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluatePatternGroupWithResultsFromExistsFilter()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddFilter(new RDFFilter(new RDFExistsExpression(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph);

        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables);
        Assert.IsEmpty(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value);
        Assert.IsTrue(patternGroup.GetFilters().First() is RDFFilter);

        RDFExistsExpression existsExpression = (RDFExistsExpression)((RDFFilter)patternGroup.GetFilters().First()).Expression;
        Assert.IsNotNull(existsExpression.PatternResults);
        Assert.IsTrue(existsExpression.PatternResults != null);
        Assert.IsFalse((bool)existsExpression.PatternResults.IsOptional);
        Assert.AreEqual(2, existsExpression.PatternResults.Columns.Count);
        Assert.AreEqual(3, existsExpression.PatternResults.Rows.Count);
        Assert.IsTrue(string.Equals(existsExpression.PatternResults.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(existsExpression.PatternResults.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(existsExpression.PatternResults.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(existsExpression.PatternResults.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(existsExpression.PatternResults.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(existsExpression.PatternResults.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldEvaluatePatternGroupWithNoResultsFromExistsFilter()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddFilter(new RDFFilter(new RDFExistsExpression(new RDFPatternGroup().AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X"))))));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph);

        Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
        Assert.HasCount(1, queryEngine.PatternGroupMemberResultTables);
        Assert.IsEmpty(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value);
        Assert.IsTrue(patternGroup.GetFilters().First() is RDFFilter);

        RDFExistsExpression existsExpression = (RDFExistsExpression)((RDFFilter)patternGroup.GetFilters().First()).Expression;
        Assert.IsNotNull(existsExpression.PatternResults);
        Assert.IsTrue(existsExpression.PatternResults != null);
        Assert.IsFalse((bool)existsExpression.PatternResults.IsOptional);
        Assert.AreEqual(2, existsExpression.PatternResults.Columns.Count);
        Assert.AreEqual(0, existsExpression.PatternResults.Rows.Count);
    }

    [TestMethod]
    public void ShouldFinalizePatternGroup()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph); //Just to obtain real pattern tables (instead of mocking them)
        queryEngine.FinalizePatternGroup(patternGroup);

        Assert.IsNotNull(queryEngine.QueryMemberResultTables);
        Assert.HasCount(1, queryEngine.QueryMemberResultTables);
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ContainsKey(patternGroup.QueryMemberID));
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value != null);
        Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.IsOptional);
        Assert.AreEqual(3, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count);
        Assert.AreEqual(3, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?N"] ?? string.Empty, string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldFinalizePatternGroupWithOptional()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
            .Optional();
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph); //Just to obtain real pattern tables (instead of mocking them)
        queryEngine.FinalizePatternGroup(patternGroup);

        Assert.IsNotNull(queryEngine.QueryMemberResultTables);
        Assert.HasCount(1, queryEngine.QueryMemberResultTables);
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ContainsKey(patternGroup.QueryMemberID));
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value != null);
        Assert.IsTrue((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.IsOptional);
        Assert.AreEqual(3, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count);
        Assert.AreEqual(3, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(), "Donald Duck@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?X"].ToString(), "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?N"] ?? string.Empty, string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyFilters()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:baobao"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperoga")),
            new RDFTriple(new RDFResource("ex:paperoga"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Paperoga", "it-IT")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
            .AddFilter(new RDFFilter(new RDFRegexExpression(new RDFVariable("?Y"), new Regex("^ex:[a-zA-Z]+o$"))))
            .AddFilter(new RDFFilter(new RDFInExpression(new RDFVariable("?Y"), [new RDFResource("ex:pluto")])));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(patternGroup, graph); //Just to obtain real pattern tables (instead of mocking them)
        queryEngine.FinalizePatternGroup(patternGroup); //Just to obtain real pattern group table  (instead of mocking it)
        queryEngine.ApplyFilters(patternGroup);

        Assert.IsNotNull(queryEngine.QueryMemberResultTables);
        Assert.HasCount(1, queryEngine.QueryMemberResultTables);
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ContainsKey(patternGroup.QueryMemberID));
        Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value != null);
        Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.IsOptional);
        Assert.AreEqual(3, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count);
        Assert.AreEqual(1, queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count);
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyModifiers()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
            .AddModifier(new RDFGroupByModifier([new RDFVariable("?Y")])
                .AddAggregator(new RDFSampleAggregator(new RDFVariable("?X"), new RDFVariable("?SAMPLE_X"))))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?SAMPLE_X"), RDFQueryEnums.RDFOrderByFlavors.ASC))
            .AddModifier(new RDFOffsetModifier(1))
            .AddModifier(new RDFLimitModifier(2))
            .AddModifier(new RDFDistinctModifier())
            .AddProjectionVariable(new RDFVariable("?N")); //Will be overridden by GroupBy operator
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        queryEngine.EvaluatePatternGroup(query.GetPatternGroups().First(), graph); //Just to obtain real pattern tables (instead of mocking them)
        queryEngine.FinalizePatternGroup(query.GetPatternGroups().First()); //Just to obtain real pattern group table  (instead of mocking it)
        queryEngine.ApplyFilters(query.GetPatternGroups().First()); //Just to obtain real filtered table (instead of mocking it)
        RDFTable resultTable = queryEngine.ApplyModifiers(query, queryEngine.QueryMemberResultTables.ElementAt(0).Value, graph);

        Assert.AreEqual(2, resultTable.Columns.Count);
        Assert.AreEqual(2, resultTable.Rows.Count);
        Assert.IsTrue(string.Equals(resultTable.Rows[0]["?Y"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(resultTable.Rows[0]["?SAMPLE_X"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(resultTable.Rows[1]["?Y"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(resultTable.Rows[1]["?SAMPLE_X"].ToString(), "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldFillTemplateTriples()
    {
        List<RDFPattern> templates =
        [
            new RDFPattern(new RDFResource("ex:bracco"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")),
            new RDFPattern(new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog"))
        ];
        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "" }, { "?X", "ex:paperino" } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "hello" }, { "?X", "ex:paperino" } });
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable filledTable = queryEngine.FillTemplates(templates, table, false);

        Assert.IsNotNull(filledTable);
        Assert.AreEqual(3, filledTable.ColumnsCount);
        Assert.AreEqual(3, filledTable.RowsCount);
        Assert.IsTrue(string.Equals(filledTable.Rows[0]["?SUBJECT"], "ex:bracco", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[0]["?PREDICATE"], $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[0]["?OBJECT"], "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[1]["?SUBJECT"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[1]["?PREDICATE"], $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[1]["?OBJECT"], "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[2]["?SUBJECT"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[2]["?PREDICATE"], $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[2]["?OBJECT"], "ex:dog", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldFillTemplateQuadruples()
    {
        List<RDFPattern> templates =
        [
            new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:bracco"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")),
            new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog"))
        ];
        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "" }, { "?X", "ex:paperino" } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "hello" }, { "?X", "ex:paperino" } });
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable filledTable = queryEngine.FillTemplates(templates, table, true);

        Assert.IsNotNull(filledTable);
        Assert.AreEqual(4, filledTable.ColumnsCount);
        Assert.AreEqual(3, filledTable.RowsCount);
        Assert.IsTrue(string.Equals(filledTable.Rows[0]["?CONTEXT"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[0]["?SUBJECT"], "ex:bracco", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[0]["?PREDICATE"], $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[0]["?OBJECT"], "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[1]["?CONTEXT"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[1]["?SUBJECT"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[1]["?PREDICATE"], $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[1]["?OBJECT"], "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[2]["?CONTEXT"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[2]["?SUBJECT"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[2]["?PREDICATE"], $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(filledTable.Rows[2]["?OBJECT"], "ex:dog", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldDescribeStarTerms()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);

        RDFPatternGroup pgDescStar = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescStar = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddBinaryQueryMember(pgDescStar.Union(sqDescStar));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" }, { "?N", "Mickey Mouse@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" }, { "?N", "Donald Duck@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:balto" }, { "?X", "ex:whoever" }, { "?N", null }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, graph, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(12, result.RowsCount);
    }

    [TestMethod]
    public void ShouldDescribeResourceTerms()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);

        RDFPatternGroup pgDescRes = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescRes = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:balto"))
            .AddBinaryQueryMember(pgDescRes.Union(sqDescRes));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" }, { "?N", "Mickey Mouse@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" }, { "?N", "Donald Duck@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:balto" }, { "?X", "ex:whoever" }, { "?N", null }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, graph, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(2, result.RowsCount);
    }

    [TestMethod]
    public void ShouldDescribeResourceTermsOnFederation()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino"))
        ]);
        RDFGraph graph2 = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US"))
        ]);
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);
        RDFMemoryStore store2 = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:snoopy"),new RDFResource("ex:dogOf"),new RDFResource("ex:linus")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:linus"),new RDFResource("ex:hasName"),new RDFTypedLiteral("Linus", RDFModelEnums.RDFDatatypes.XSD_STRING))
        ]);
        RDFFederation federation = new RDFFederation().AddGraph(graph)
            .AddGraph(graph2)
            .AddStore(store)
            .AddStore(store2);

        RDFPatternGroup pgDescFed = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescFed = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:balto"))
            .AddDescribeTerm(new RDFResource("ex:snoopy"))
            .AddDescribeTerm(new RDFResource())
            .AddBinaryQueryMember(pgDescFed.Union(sqDescFed));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" }, { "?N", "Mickey Mouse@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" }, { "?N", "Donald Duck@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:balto" }, { "?X", "ex:whoever" }, { "?N", null }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:snoopy" }, { "?X", "ex:linus" }, { "?N", $"Linus^^{RDFVocabulary.XSD.STRING}" }, { "?C", null } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, federation, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
    }

    [TestMethod]
    public void ShouldDescribeUnexistingResourceTerms()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);

        RDFPatternGroup pgDescUnex = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescUnex = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFResource("ex:balto2"))
            .AddBinaryQueryMember(pgDescUnex.Union(sqDescUnex));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" }, { "?N", "Mickey Mouse@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" }, { "?N", "Donald Duck@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:balto" }, { "?X", "ex:whoever" }, { "?N", null }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, graph, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(0, result.RowsCount);
    }

    [TestMethod]
    public void ShouldDescribeVariableTerms()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);

        RDFPatternGroup pgDescVar = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescVar = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?Y"))
            .AddBinaryQueryMember(pgDescVar.Union(sqDescVar));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" }, { "?N", "Mickey Mouse@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" }, { "?N", "Donald Duck@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:balto" }, { "?X", "ex:whoever" }, { "?N", null }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, graph, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(4, result.RowsCount);
    }

    [TestMethod]
    public void ShouldDescribeVariableTermsOnFederation()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino"))
        ]);
        RDFGraph graph2 = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);
        RDFFederation federation = new RDFFederation().AddGraph(graph).AddGraph(graph2);

        RDFPatternGroup pgDescVarFed = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescVarFed = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?Y"))
            .AddDescribeTerm(new RDFVariable("?N"))
            .AddBinaryQueryMember(pgDescVarFed.Union(sqDescVarFed));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" }, { "?N", "Mickey Mouse@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" }, { "?N", "Donald Duck@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:balto" }, { "?X", "ex:whoever" }, { "?N", null }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, federation, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(6, result.RowsCount);
    }

    [TestMethod]
    public void ShouldDescribeUnexistingVariableTerms()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);

        RDFPatternGroup pgDescUnexVar = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescUnexVar = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?Z"))
            .AddBinaryQueryMember(pgDescUnexVar.Union(sqDescUnexVar));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:pluto" }, { "?X", "ex:topolino" }, { "?N", "Mickey Mouse@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:fido" }, { "?X", "ex:paperino" }, { "?N", "Donald Duck@EN-US" }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", "ex:balto" }, { "?X", "ex:whoever" }, { "?N", null }, { "?C", null } });
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, graph, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(0, result.RowsCount);
    }

    [TestMethod]
    public void ShouldDescribeLiteralBoundVariableTermsOnGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);

        RDFPatternGroup pgDescLitGraph = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescLitGraph = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?C"))
            .AddBinaryQueryMember(pgDescLitGraph.Union(sqDescLitGraph));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, graph, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
    }

    [TestMethod]
    public void ShouldDescribeLiteralBoundVariableTermsOnStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
            new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
        ]);

        RDFPatternGroup pgDescLitStore = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
        RDFSelectQuery sqDescLitStore = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))));
        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddDescribeTerm(new RDFVariable("?C"))
            .AddBinaryQueryMember(pgDescLitStore.Union(sqDescLitStore));

        RDFTable table = new RDFTable();
        table.AddColumn("?Y");
        table.AddColumn("?X");
        table.AddColumn("?N");
        table.AddColumn("?C");
        table.AddRow(new Dictionary<string, string> { { "?Y", null }, { "?X", null }, { "?N", null }, { "?C", "green@EN" } });

        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.DescribeTerms(query, store, table);

        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
    }

    [TestMethod]
    public void ShouldApplyPatternToDataSourceGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPattern(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternToDataSourceStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPattern(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternToDataSourceFederation()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
        ]);
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext(), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFFederation federation = new RDFFederation()
            .AddGraph(graph)
            .AddStore(store);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPattern(pattern, federation);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithSubjectVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithPredicateVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithObjectVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithLiteralVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:topolino"), new RDFResource("ex:hasName"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithSubjectPredicateVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?V"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?V"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualSubjectPredicateVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithSubjectObjectVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualSubjectObjectVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:pluto")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?Y"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithPredicateObjectVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?V"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?V"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualPredicateObjectVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:dogOf")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?V"), new RDFVariable("?V"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?V"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithSubjectPredicateObjectVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:dogOf")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?V"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(5, result.RowsCount); //All the triples...
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualSubjectPredicateObjectVariableToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:pluto")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), new RDFVariable("?Y"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithSubjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithPredicateVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithLiteralVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"), new RDFResource("ex:hasName"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextSubjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualContextSubjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextPredicateVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualContextPredicateVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:ctx"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFVariable("?C"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualContextObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:ctx")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?C"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextLiteralVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:topolino"), new RDFResource("ex:hasName"), new RDFVariable("?N"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?N"], "Mickey Mouse@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextSubjectPredicateVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Y"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualContextSubjectPredicateVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFVariable("?C"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextSubjectObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualContextSubjectObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:dogOf"),new RDFResource("ex:ctx")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFResource("ex:dogOf"), new RDFVariable("?C"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextPredicateObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?P"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualContextPredicateObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:ctx"),new RDFResource("ex:ctx")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFVariable("?C"), new RDFVariable("?C"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithContextSubjectPredicateObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?Y"), new RDFVariable("?P"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.ColumnsCount);
        Assert.AreEqual(5, result.RowsCount); //All the quadruples...
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualContextSubjectPredicateObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:ctx")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?C"), new RDFVariable("?C"), new RDFVariable("?C"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?C"], "ex:ctx", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithSubjectPredicateVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFVariable("?V"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?V"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualSubjectPredicateVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFVariable("?Y"), new RDFResource("ex:topolino"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithSubjectObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualSubjectObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:pluto")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?Y"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithPredicateObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFVariable("?V"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?V"], "ex:dogOf", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualPredicateObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:dogOf")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFVariable("?V"), new RDFVariable("?V"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?V"], "ex:dogOf", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternWithSubjectPredicateObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:dogOf")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFVariable("?V"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.ColumnsCount);
        Assert.AreEqual(5, result.RowsCount); //All the quadruples...
    }

    [TestMethod]
    public void ShouldApplyPatternWithEqualSubjectPredicateObjectVariableToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:pluto")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFVariable("?Y"), new RDFVariable("?Y"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToStore(pattern, store);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternToFederationHavingGraph()
    {
        RDFGraph graph1 = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US"))
        ]);
        RDFGraph graph2 = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFFederation federation = new RDFFederation()
            .AddFederation(new RDFFederation().AddGraph(graph1))
            .AddGraph(graph2);

        RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToFederation(pattern, federation);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternToFederationHavingStore()
    {
        RDFMemoryStore store1 = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US"))
        ]);
        RDFMemoryStore store2 = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFFederation federation = new RDFFederation()
            .AddFederation(new RDFFederation().AddStore(store1))
            .AddStore(store2);

        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToFederation(pattern, federation);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternToFederationHavingFederation()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US"))
        ]);
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFFederation federation = new RDFFederation()
            .AddFederation(new RDFFederation().AddGraph(graph))
            .AddFederation(new RDFFederation().AddFederation(new RDFFederation().AddStore(store)));

        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToFederation(pattern, federation);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:whoever", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPatternToFederationHavingSPARQLEndpoint()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldApplyPatternToFederationHavingSPARQLEndpoint/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody(
                        """
                        <?xml version="1.0" encoding="utf-8"?>
                        <sparql xmlns="http://www.w3.org/2005/sparql-results#">
                          <head>
                            <variable name="?Y" />
                            <variable name="?X" />
                          </head>
                          <results>
                            <result>
                              <binding name="?Y">
                                <uri>ex:pluto</uri>
                              </binding>
                              <binding name="?X">
                                <uri>ex:topolino</uri>
                              </binding>
                            </result>
                          </results>
                        </sparql>
                        """, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithStatusCode(HttpStatusCode.OK));
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldApplyPatternToFederationHavingSPARQLEndpoint/sparql"));

        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US"))
        ]);
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFFederation federation = new RDFFederation()
            .AddFederation(new RDFFederation().AddGraph(graph))
            .AddFederation(new RDFFederation().AddFederation(new RDFFederation().AddStore(store)))
            .AddFederation(new RDFFederation().AddSPARQLEndpoint(endpoint))
            .AddFederation(new RDFFederation());

        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPatternToFederation(pattern, federation);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(3, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?X"], "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?X"], "ex:whoever", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[2]["?X"], "ex:topolino", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPropertyPath()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US"))
        ]);
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?N"))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:dogOf")))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:hasName")));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPropertyPath(propertyPath, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(2, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?N"], "Mickey Mouse@EN-US", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[1]["?N"], "Donald Duck@EN-US", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPropertyPathOverFederationOfLocalDataSourcesOnly()
    {
        //The two hops of the path live in two different graphs of the same federation: this must keep working
        //exactly as before, since local members are still merged into one unified adjacency (see FlattenFederation)
        RDFGraph dogOfGraph = new RDFGraph([ new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino")) ]);
        RDFGraph hasNameGraph = new RDFGraph([ new RDFTriple(new RDFResource("ex:topolino"), new RDFResource("ex:hasName"), new RDFResource("ex:topolinoName")) ]);
        RDFFederation federation = new RDFFederation().AddGraph(dogOfGraph).AddGraph(hasNameGraph);

        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?N"))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:dogOf")))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:hasName")));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPropertyPath(propertyPath, federation);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?N"], "ex:topolinoName", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyPropertyPathAcrossFederationWithSPARQLEndpointAndMergeResults()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldApplyPropertyPathAcrossFederationWithSPARQLEndpointAndMergeResults/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody(
                        """
                        <?xml version="1.0" encoding="utf-8"?>
                        <sparql xmlns="http://www.w3.org/2005/sparql-results#">
                          <head>
                            <variable name="?Y" />
                            <variable name="?N" />
                          </head>
                          <results>
                            <result>
                              <binding name="?Y"><uri>ex:fido</uri></binding>
                              <binding name="?N"><uri>ex:paperinoName</uri></binding>
                            </result>
                          </results>
                        </sparql>
                        """, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithStatusCode(HttpStatusCode.OK));

        //The federation has one local graph (pluto -> topolinoName) and one remote endpoint (fido -> paperinoName,
        //mocked above): both bindings are fully contained in a single source, so both must show up in the merge
        RDFGraph localGraph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"), new RDFResource("ex:hasName"), new RDFResource("ex:topolinoName"))
        ]);
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldApplyPropertyPathAcrossFederationWithSPARQLEndpointAndMergeResults/sparql"));
        RDFFederation federation = new RDFFederation().AddGraph(localGraph).AddSPARQLEndpoint(endpoint);

        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?N"))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:dogOf")))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:hasName")));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPropertyPath(propertyPath, federation);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(2, result.RowsCount);
        Assert.IsTrue(result.Rows.Any(row => string.Equals(row["?Y"], "ex:pluto", StringComparison.Ordinal) && string.Equals(row["?N"], "ex:topolinoName", StringComparison.Ordinal)));
        Assert.IsTrue(result.Rows.Any(row => string.Equals(row["?Y"], "ex:fido", StringComparison.Ordinal) && string.Equals(row["?N"], "ex:paperinoName", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void ShouldApplyPropertyPathAcrossNestedFederationReachingSPARQLEndpointAtDepth2()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldApplyPropertyPathAcrossNestedFederationReachingSPARQLEndpointAtDepth2/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody(
                        """
                        <?xml version="1.0" encoding="utf-8"?>
                        <sparql xmlns="http://www.w3.org/2005/sparql-results#">
                          <head>
                            <variable name="?Y" />
                            <variable name="?N" />
                          </head>
                          <results>
                            <result>
                              <binding name="?Y"><uri>ex:fido</uri></binding>
                              <binding name="?N"><uri>ex:paperinoName</uri></binding>
                            </result>
                          </results>
                        </sparql>
                        """, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithStatusCode(HttpStatusCode.OK));

        //The endpoint sits two levels deep (outerFederation -> innerFederation -> endpoint): FlattenFederation
        //must recurse through the nesting to reach it, exactly as GetAllNodes/CollectPropertyEdges already do for
        //Graph/Store members
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldApplyPropertyPathAcrossNestedFederationReachingSPARQLEndpointAtDepth2/sparql"));
        RDFFederation innerFederation = new RDFFederation().AddSPARQLEndpoint(endpoint);
        RDFFederation outerFederation = new RDFFederation().AddFederation(innerFederation);

        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?N"))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:dogOf")))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:hasName")));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPropertyPath(propertyPath, outerFederation);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ColumnsCount);
        Assert.AreEqual(1, result.RowsCount);
        Assert.IsTrue(string.Equals(result.Rows[0]["?Y"], "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.Rows[0]["?N"], "ex:paperinoName", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldNotReconstructPropertyPathHopThatCrossesFromSPARQLEndpointToLocalDataSource()
    {
        //The endpoint only knows the first hop (dogOf) and has no hasName data of its own, so its own SPARQL
        //engine correctly finds no match for the full 2-hop path and returns an empty result set
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFQueryEngineTest/ShouldNotReconstructPropertyPathHopThatCrossesFromSPARQLEndpointToLocalDataSource/sparql")
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithBody(
                        """
                        <?xml version="1.0" encoding="utf-8"?>
                        <sparql xmlns="http://www.w3.org/2005/sparql-results#">
                          <head>
                            <variable name="?Y" />
                            <variable name="?N" />
                          </head>
                          <results />
                        </sparql>
                        """, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithStatusCode(HttpStatusCode.OK));

        //The local graph only knows the second hop (hasName) and has no dogOf data of its own, so it also cannot
        //complete the path alone. The (pluto -> topolinoName) binding only exists by stitching the endpoint's
        //dogOf edge with the local graph's hasName edge, which this design does NOT attempt across sources.
        RDFGraph localGraph = new RDFGraph([ new RDFTriple(new RDFResource("ex:topolino"), new RDFResource("ex:hasName"), new RDFResource("ex:topolinoName")) ]);
        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldNotReconstructPropertyPathHopThatCrossesFromSPARQLEndpointToLocalDataSource/sparql"));
        RDFFederation federation = new RDFFederation().AddGraph(localGraph).AddSPARQLEndpoint(endpoint);

        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?N"))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:dogOf")))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:hasName")));
        RDFQueryEngine queryEngine = new RDFQueryEngine();
        RDFTable result = queryEngine.ApplyPropertyPath(propertyPath, federation);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.RowsCount);
    }

    #endregion

    #region Operator tree tests (Union/Minus)

    #region Shared test data

    /// <summary>
    /// Builds a graph with dogs and owners, used across most tests:
    ///   pluto  -> dogOf -> topolino   (topolino  hasName "Mickey Mouse")
    ///   fido   -> dogOf -> paperino   (paperino  hasName "Donald Duck")
    ///   balto  -> dogOf -> whoever
    /// </summary>
    private static RDFGraph BuildDogGraph()
    {
        return new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),    new RDFResource("ex:dogOf"),  new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"), new RDFResource("ex:hasName"), new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),     new RDFResource("ex:dogOf"),  new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"), new RDFResource("ex:hasName"), new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),    new RDFResource("ex:dogOf"),  new RDFResource("ex:whoever"))
        ]);
    }

    #endregion

    #region Query-level operator tree (between pattern groups)

    [TestMethod]
    public void ShouldEvaluateUnionOfTwoPatternGroups()
    {
        // pgA matches pluto->topolino, pgB matches fido->paperino
        // Union should return both rows
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual(2, result.SelectResultsCount);

        // Verify both values are present (order may vary depending on graph iteration)
        string[] xValues = Enumerable.Range(0, (int)result.SelectResultsCount)
            .Select(i => result.SelectResults.Rows[i]["?X"].ToString())
            .OrderBy(v => v, StringComparer.Ordinal)
            .ToArray();
        Assert.AreEqual("ex:paperino", xValues[0]);
        Assert.AreEqual("ex:topolino", xValues[1]);
    }

    [TestMethod]
    public void ShouldEvaluateMinusOfTwoPatternGroups()
    {
        // pgA matches all dogs: pluto/topolino, fido/paperino, balto/whoever
        // pgB matches balto specifically
        // Minus should exclude the row where ?Y = balto
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:balto")), new RDFVariable("?Y")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Minus(pgB))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual("ex:paperino", result.SelectResults.Rows[0]["?X"].ToString());
        Assert.AreEqual("ex:topolino", result.SelectResults.Rows[1]["?X"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluateNestedUnionThenMinusDifferentlyFromMinusThenUnion()
    {
        // CRITICAL TEST: verifies that tree nesting matters
        //
        // Graph: A={1,2,3}, B={2,3}, C={3}
        //   (A UNION B) MINUS C  →  {1,2,3,2,3} MINUS {3}  →  {1,2,2}  →  3 rows
        //   A UNION (B MINUS C)  →  A UNION {2}             →  {1,2,3,2} →  4 rows
        //
        // We use a graph with numeric-looking resources to keep things clear
        RDFGraph graph = new RDFGraph(
        [
            // Group A: items 1, 2, 3
            new RDFTriple(new RDFResource("ex:1"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")),
            new RDFTriple(new RDFResource("ex:2"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")),
            new RDFTriple(new RDFResource("ex:3"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")),
            // Group B: items 2, 3
            new RDFTriple(new RDFResource("ex:2"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")),
            new RDFTriple(new RDFResource("ex:3"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")),
            // Group C: item 3
            new RDFTriple(new RDFResource("ex:3"), new RDFResource("ex:inGroupC"), new RDFResource("ex:yes"))
        ]);

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")));
        RDFPatternGroup pgC = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupC"), new RDFResource("ex:yes")));

        // Query 1: (A UNION B) MINUS C
        RDFPatternGroup pgA1 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")));
        RDFPatternGroup pgB1 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")));
        RDFPatternGroup pgC1 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupC"), new RDFResource("ex:yes")));

        RDFSelectQuery queryUnionThenMinus = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA1.Union(pgB1).Minus(pgC1));
        RDFSelectQueryResult resultUnionThenMinus = new RDFQueryEngine().EvaluateSelectQuery(queryUnionThenMinus, graph);

        // Query 2: A UNION (B MINUS C)
        RDFPatternGroup pgA2 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupA"), new RDFResource("ex:yes")));
        RDFPatternGroup pgB2 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupB"), new RDFResource("ex:yes")));
        RDFPatternGroup pgC2 = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:inGroupC"), new RDFResource("ex:yes")));

        RDFSelectQuery queryUnionOfMinusRight = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA2.Union(pgB2.Minus(pgC2)));
        RDFSelectQueryResult resultUnionOfMinusRight = new RDFQueryEngine().EvaluateSelectQuery(queryUnionOfMinusRight, graph);

        // The two queries MUST produce different row counts, proving tree evaluation matters
        // (A UNION B) MINUS C: union of {1,2,3} and {2,3} = {1,2,3,2,3}, minus {3} = {1,2,2} → 3 rows
        Assert.AreEqual(3, resultUnionThenMinus.SelectResultsCount,
            "(A UNION B) MINUS C should yield 3 rows");

        // A UNION (B MINUS C): B MINUS C = {2}, then union with A = {1,2,3,2} → 4 rows
        Assert.AreEqual(4, resultUnionOfMinusRight.SelectResultsCount,
            "A UNION (B MINUS C) should yield 4 rows");

        // Explicitly verify they are different
        Assert.AreNotEqual(resultUnionThenMinus.SelectResultsCount,
                          resultUnionOfMinusRight.SelectResultsCount,
                          "Tree nesting must produce different results: (A UNION B) MINUS C != A UNION (B MINUS C)");
    }

    [TestMethod]
    public void ShouldEvaluateUnionWithSubquery()
    {
        // pgA matches pluto->topolino via a direct pattern;
        // subquery selects fido->paperino.
        // Union of both should yield 2 rows.
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery subQuery = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(subQuery));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateOptionalOperatorTree()
    {
        // When an operator tree is marked Optional, the outer join should preserve
        // left rows that have no match in the operator tree's result (UNBOUND right columns).
        //
        // pgMain: all dogs (?Y dogOf ?X) → 3 rows: pluto/topolino, fido/paperino, balto/whoever
        // Operator tree (Optional): union of two patterns that find names via ?X (shared variable)
        //   pgA: (?X hasName ?N) for topolino only; pgB: (?X hasName ?N) for paperino only
        //   → union yields topolino/Mickey and paperino/Donald with ?X and ?N
        //   → balto/whoever has no match on ?X in operator tree, but kept because Optional
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgMain = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        // Both leaf pattern groups share ?X with pgMain, ensuring a meaningful join
        RDFPatternGroup pgNamesA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")))
            .AddFilter(new RDFFilter(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFVariableExpression(new RDFVariable("?X")),
                    new RDFConstantExpression(new RDFResource("ex:topolino")))));
        RDFPatternGroup pgNamesB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")))
            .AddFilter(new RDFFilter(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                    new RDFVariableExpression(new RDFVariable("?X")),
                    new RDFConstantExpression(new RDFResource("ex:paperino")))));

        // Union of two name lookups filtered by ?X, marked as Optional
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(pgMain)
            .AddBinaryQueryMember(pgNamesA.Union(pgNamesB).Optional())
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // All 3 dogs should appear: 2 with names, 1 with UNBOUND ?N (because Optional)
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.SelectResultsCount);
        // paperino has a name
        Assert.AreEqual("Donald Duck@EN-US", result.SelectResults.Rows[0]["?N"].ToString());
        // topolino has a name
        Assert.AreEqual("Mickey Mouse@EN-US", result.SelectResults.Rows[1]["?N"].ToString());
        // whoever has no name (UNBOUND → empty string in DataTable)
        Assert.AreEqual(string.Empty, result.SelectResults.Rows[2]["?N"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluateEmptyPatternGroupInOperatorTree()
    {
        // pgA has patterns, pgB is empty (no evaluable members)
        // Union with empty should just return pgA's results
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgEmpty = new RDFPatternGroup();

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgEmpty));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // pgA yields 1 row (ex:topolino), pgEmpty yields 0 rows; union = 1 row
        Assert.AreEqual(1, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateMinusWithNoCommonVariables()
    {
        // When left and right have no common variables, MINUS keeps all left rows (SPARQL spec)
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Z"), new RDFResource("ex:hasName"), new RDFVariable("?N")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Minus(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // No common variables → all 3 left rows survive the MINUS
        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateDeepNestedOperatorTree()
    {
        // ((A UNION B) UNION C) — three levels of nesting
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgC = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgB).Union(pgC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // All three dogs should appear
        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateOperatorTreeWithFilter()
    {
        // Pattern group with a filter inside the operator tree leaf
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddFilter(new RDFFilter(
                new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.NotEqualTo,
                    new RDFVariableExpression(new RDFVariable("?Y")),
                    new RDFConstantExpression(new RDFResource("ex:balto")))));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // pgA with filter: pluto/topolino and fido/paperino (2 rows), pgB: balto/whoever (1 row) → union = 3 rows
        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateOperatorTreeAlongsidePlainPatternGroups()
    {
        // Mix of plain pattern group (joined normally) and operator tree
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgNames = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")));

        RDFPatternGroup pgPluto = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgFido = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        // The query has both a plain pattern group and an operator tree
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(pgNames)
            .AddBinaryQueryMember(pgPluto.Union(pgFido));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // pgNames returns 2 rows (topolino/Mickey, paperino/Donald)
        // Operator tree returns 2 rows (topolino, paperino)
        // They are joined on ?X → 2 rows with names
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateAskQueryWithOperatorTree()
    {
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFAskQuery askQuery = new RDFAskQuery()
            .AddBinaryQueryMember(pgA.Union(pgB));

        RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(askQuery, graph);

        Assert.IsTrue(result.AskResult);
    }

    [TestMethod]
    public void ShouldEvaluateConstructQueryWithOperatorTree()
    {
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFConstructQuery constructQuery = new RDFConstructQuery()
            .AddBinaryQueryMember(pgA.Union(pgB))
            .AddTemplate(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:isDog"), new RDFResource("ex:true")));

        RDFConstructQueryResult result = new RDFQueryEngine().EvaluateConstructQuery(constructQuery, graph);

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.ConstructResults.Rows.Count);
    }

    [TestMethod]
    public void ShouldEvaluateDescribeQueryWithOperatorTree()
    {
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFDescribeQuery describeQuery = new RDFDescribeQuery()
            .AddBinaryQueryMember(pgA.Union(pgB))
            .AddDescribeTerm(new RDFVariable("?X"));

        RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(describeQuery, graph);

        Assert.IsNotNull(result);
        // topolino and paperino are described; each has a hasName triple → at least 2 rows
        Assert.IsTrue(result.DescribeResults.Rows.Count >= 2);
    }

    #endregion

    #region Pattern-group-level operator tree (between patterns within a pattern group)

    [TestMethod]
    public void ShouldEvaluateUnionOfTwoPatternsInPatternGroup()
    {
        // Two patterns inside a pattern group combined via Union operator tree
        RDFGraph graph = BuildDogGraph();

        RDFPattern patternPluto = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternFido = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patternPluto.Union(patternFido)));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateMinusOfTwoPatternsInPatternGroup()
    {
        // Pattern A matches all dogs, Pattern B matches only fido->paperino
        // A MINUS B should remove the fido/paperino row
        RDFGraph graph = BuildDogGraph();

        RDFPattern patternAll = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternFido = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patternAll.Minus(patternFido)))
            .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // Only pluto/topolino and balto/whoever should remain
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual("ex:topolino", result.SelectResults.Rows[0]["?X"].ToString());
        Assert.AreEqual("ex:whoever", result.SelectResults.Rows[1]["?X"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluateNestedPatternOperatorTree()
    {
        // A UNION (B MINUS C) at pattern level
        RDFGraph graph = BuildDogGraph();

        RDFPattern patternPluto = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternAll = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternBalto = new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));

        // patternPluto UNION (patternAll MINUS patternBalto)
        // patternAll MINUS patternBalto = {pluto/topolino, fido/paperino} (2 rows)
        // Union with patternPluto = {topolino} + {topolino, paperino} = 3 rows
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patternPluto.Union(patternAll.Minus(patternBalto))));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluatePatternOperatorTreeWithPropertyPath()
    {
        // Union between a pattern and a property path
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:a"), new RDFResource("ex:p1"), new RDFResource("ex:b")),
            new RDFTriple(new RDFResource("ex:b"), new RDFResource("ex:p2"), new RDFResource("ex:c"))
        ]);

        RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:p1"), new RDFVariable("?O"));
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?S"), new RDFVariable("?O"))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:p1")))
            .AddStep(RDFPropertyPathStep.Link(new RDFResource("ex:p2")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(pattern.Union(propertyPath)));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // Pattern yields: (ex:a, ex:b), PropertyPath yields: (ex:a, ex:c) → Union = 2 rows
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluatePatternOperatorTreeMixedWithRegularPatterns()
    {
        // A pattern group that has both a regular pattern and an operator tree
        RDFGraph graph = BuildDogGraph();

        RDFPattern patternPluto = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
        RDFPattern patternFido = new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddBinaryPatternGroupMember(patternPluto.Union(patternFido))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // Operator tree yields {topolino, paperino}; regular pattern yields names.
        // CombineTables joins them on ?X → 2 rows with names
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    #endregion

    #region Edge cases

    [TestMethod]
    public void ShouldHandleMinusRemovingAllRows()
    {
        // When MINUS removes everything, the result should be empty
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Minus(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(0, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldHandleUnionWithIdenticalPatternGroups()
    {
        // A UNION A should yield double the rows (bag semantics, not set)
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgACopy = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgACopy));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        // Each side yields 1 row; Union appends both → 2 rows
        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateOperatorTreeOnStore()
    {
        // Verify that operator trees work on RDFMemoryStore (not just RDFGraph)
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFResource("ex:paperino"))
        ]);

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Union(pgB));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        Assert.AreEqual(2, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldEvaluateChainedMinusThenUnion()
    {
        // (A MINUS B) UNION C — chaining in the opposite order
        RDFGraph graph = BuildDogGraph();

        RDFPatternGroup pgA = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgB = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFPatternGroup pgC = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));

        // (A MINUS B): {pluto/topolino, fido/paperino} (balto removed), UNION C: {balto/whoever}
        // Total: 3 rows
        RDFSelectQuery query = new RDFSelectQuery()
            .AddBinaryQueryMember(pgA.Minus(pgB).Union(pgC));

        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        Assert.AreEqual(3, result.SelectResultsCount);
    }

    #endregion

    #region VALUES with UNDEF, FILTER on strings and ORDER BY by value

    /// <summary>
    /// Builds three wines with their grape and type, so that every VALUES row matches an unambiguous set of wines.
    /// </summary>
    private static RDFGraph BuildWineGraph()
    {
        //wine1 is a red sangiovese; wine2 is a white vermentino; wine3 is a white sangiovese that no VALUES row of the tests matches
        return new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:wine1"), new RDFResource("ex:grape"), new RDFResource("ex:sangiovese")),
            new RDFTriple(new RDFResource("ex:wine1"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:RedWine")),
            new RDFTriple(new RDFResource("ex:wine2"), new RDFResource("ex:grape"), new RDFResource("ex:vermentino")),
            new RDFTriple(new RDFResource("ex:wine2"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:WhiteWine")),
            new RDFTriple(new RDFResource("ex:wine3"), new RDFResource("ex:grape"), new RDFResource("ex:sangiovese")),
            new RDFTriple(new RDFResource("ex:wine3"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:WhiteWine"))
        ]);
    }

    [TestMethod]
    public void ShouldEvaluateValuesWithUndefInsidePatternGroupKeepingOnlyCompatibleRows()
    {
        //The second VALUES row leaves ?t UNDEF, so it accepts any type of vermentino wine
        RDFSelectQuery query = RDFSelectQuery.FromString("""
            SELECT ?w ?g ?t
            WHERE {
              ?w <ex:grape> ?g .
              ?w <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?t .
              VALUES (?g ?t) { (<ex:sangiovese> <ex:RedWine>) (<ex:vermentino> UNDEF) }
            }
            ORDER BY ?w
            """);
        //The query runs on the wine graph, where the VALUES block joins the two patterns
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildWineGraph());

        //wine3 is sangiovese but white: it matches neither VALUES row, which a left join would have kept
        Assert.AreEqual(2, result.SelectResultsCount);
        //The first row is the red sangiovese wine matched by the fully bound VALUES row
        Assert.AreEqual("ex:wine1", result.SelectResults.Rows[0]["?W"].ToString());
        Assert.AreEqual("ex:sangiovese", result.SelectResults.Rows[0]["?G"].ToString());
        Assert.AreEqual("ex:RedWine", result.SelectResults.Rows[0]["?T"].ToString());
        //The second row is the vermentino wine matched by the VALUES row with the UNDEF type
        Assert.AreEqual("ex:wine2", result.SelectResults.Rows[1]["?W"].ToString());
        Assert.AreEqual("ex:vermentino", result.SelectResults.Rows[1]["?G"].ToString());
        Assert.AreEqual("ex:WhiteWine", result.SelectResults.Rows[1]["?T"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluateTrailingValuesWithUndefKeepingOnlyCompatibleRows()
    {
        //The same VALUES block as the pattern group one, written after the WHERE clause as query-level inline data
        RDFSelectQuery query = RDFSelectQuery.FromString("""
            SELECT ?w ?g ?t
            WHERE {
              ?w <ex:grape> ?g .
              ?w <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?t .
            }
            VALUES (?g ?t) { (<ex:sangiovese> <ex:RedWine>) (<ex:vermentino> UNDEF) }
            """);
        //The query runs on the wine graph, where the trailing VALUES block filters the solutions of the WHERE clause
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildWineGraph());

        //wine3 is sangiovese but white: it matches neither VALUES row, which a left join would have kept
        Assert.AreEqual(2, result.SelectResultsCount);
        //Rows are looked up by wine, since the query has no ORDER BY that fixes their position
        DataRow redWineRow = result.SelectResults.Rows.Cast<DataRow>().Single(row => row["?W"].ToString() == "ex:wine1");
        //The red sangiovese wine is matched by the fully bound VALUES row
        Assert.AreEqual("ex:sangiovese", redWineRow["?G"].ToString());
        Assert.AreEqual("ex:RedWine", redWineRow["?T"].ToString());
        DataRow whiteWineRow = result.SelectResults.Rows.Cast<DataRow>().Single(row => row["?W"].ToString() == "ex:wine2");
        //The vermentino wine is matched by the VALUES row with the UNDEF type
        Assert.AreEqual("ex:vermentino", whiteWineRow["?G"].ToString());
        Assert.AreEqual("ex:WhiteWine", whiteWineRow["?T"].ToString());
    }

    [TestMethod]
    public void ShouldEvaluateValuesMadeOnlyOfUndefKeepingEveryRow()
    {
        //A VALUES block with a single UNDEF constrains nothing, so it must not drop any row
        RDFSelectQuery query = RDFSelectQuery.FromString("""
            SELECT ?w ?g
            WHERE {
              ?w <ex:grape> ?g .
              VALUES ?g { UNDEF }
            }
            ORDER BY ?w
            """);
        //The query runs on the wine graph, which holds three wines with a grape each
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildWineGraph());

        //All the three wines come back, since UNDEF is compatible with every grape
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.AreEqual("ex:wine1", result.SelectResults.Rows[0]["?W"].ToString());
        Assert.AreEqual("ex:sangiovese", result.SelectResults.Rows[0]["?G"].ToString());
        Assert.AreEqual("ex:wine2", result.SelectResults.Rows[1]["?W"].ToString());
        Assert.AreEqual("ex:vermentino", result.SelectResults.Rows[1]["?G"].ToString());
        Assert.AreEqual("ex:wine3", result.SelectResults.Rows[2]["?W"].ToString());
        Assert.AreEqual("ex:sangiovese", result.SelectResults.Rows[2]["?G"].ToString());
    }

    [TestMethod]
    [DataRow("<", 1)]
    [DataRow("!=", 1)]
    [DataRow(">", 0)]
    [DataRow("=", 0)]
    public void ShouldFilterPlainLiteralsAgainstNonAsciiLiteralByCodePointOrder(string comparisonOperator, int expectedRows)
    {
        //The graph holds a single plain literal that is far below the compared one in code points
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFPlainLiteral("a"))
        ]);
        //"a" (U+0061) precedes "\u00C4" (U+00C4) in code-point order, so only the operators that accept it keep the row
        RDFSelectQuery query = RDFSelectQuery.FromString(
            $"SELECT ?o WHERE {{ <ex:s> <ex:p> ?o . FILTER(?o {comparisonOperator} \"\u00C4\") }}");
        //The filter runs the string comparison that used to collide with the type error marker
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        //The row survives only when the operator holds between "a" and the compared literal
        Assert.AreEqual(expectedRows, result.SelectResultsCount);
    }

    [TestMethod]
    public void ShouldOrderByNumericValueAndLimit()
    {
        //120 12 9 sorted as text would be 12 120 9, so the two smallest rows prove the numeric order
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?n WHERE { VALUES ?n { 120 12 9 } } ORDER BY ?n LIMIT 2");
        //The query needs no data source content, since the values come from the VALUES block
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph());

        //LIMIT 2 keeps the two smallest values
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual("9^^http://www.w3.org/2001/XMLSchema#integer", result.SelectResults.Rows[0]["?N"].ToString());
        Assert.AreEqual("12^^http://www.w3.org/2001/XMLSchema#integer", result.SelectResults.Rows[1]["?N"].ToString());
    }

    /// <summary>
    /// Builds three wines with the vintage year as xsd:gYear, in non-chronological insertion order.
    /// </summary>
    private static RDFGraph BuildVintageGraph()
    {
        //The vintages are 2021 then 2015 then 2019, so that only a value-based order can sort them
        return new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:wine1"), new RDFResource("ex:vintage"), new RDFTypedLiteral("2021", RDFModelEnums.RDFDatatypes.XSD_GYEAR)),
            new RDFTriple(new RDFResource("ex:wine2"), new RDFResource("ex:vintage"), new RDFTypedLiteral("2015", RDFModelEnums.RDFDatatypes.XSD_GYEAR)),
            new RDFTriple(new RDFResource("ex:wine3"), new RDFResource("ex:vintage"), new RDFTypedLiteral("2019", RDFModelEnums.RDFDatatypes.XSD_GYEAR))
        ]);
    }

    [TestMethod]
    public void ShouldFilterGregorianYearsLowerThanGivenYear()
    {
        //Only the 2015 and 2019 vintages precede the xsd:gYear 2020
        RDFSelectQuery query = RDFSelectQuery.FromString("""
            SELECT ?w
            WHERE { ?w <ex:vintage> ?v . FILTER(?v < "2020"^^<http://www.w3.org/2001/XMLSchema#gYear>) }
            ORDER BY ?w
            """);
        //The filter compares the stored forms of the xsd:gYear literals, which a general-purpose date parser rejects
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildVintageGraph());

        //The 2015 vintage is wine2 and the 2019 vintage is wine3
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual("ex:wine2", result.SelectResults.Rows[0]["?W"].ToString());
        Assert.AreEqual("ex:wine3", result.SelectResults.Rows[1]["?W"].ToString());
    }

    [TestMethod]
    public void ShouldFilterGregorianYearsEqualToGivenYear()
    {
        //Only the 2021 vintage equals the xsd:gYear 2021
        RDFSelectQuery query = RDFSelectQuery.FromString("""
            SELECT ?w
            WHERE { ?w <ex:vintage> ?v . FILTER(?v = "2021"^^<http://www.w3.org/2001/XMLSchema#gYear>) }
            """);
        //The filter compares the stored forms of the xsd:gYear literals, which a general-purpose date parser rejects
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildVintageGraph());

        //The 2021 vintage is wine1
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.AreEqual("ex:wine1", result.SelectResults.Rows[0]["?W"].ToString());
    }

    [TestMethod]
    [DataRow("ASC", "2015Z", "2019Z", "2021Z")]
    [DataRow("DESC", "2021Z", "2019Z", "2015Z")]
    public void ShouldOrderByGregorianYearsChronologically(string direction, string firstYear, string secondYear, string thirdYear)
    {
        //The sort key is the xsd:gYear vintage, whose insertion order in the graph is not chronological
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT ?v WHERE {{ ?w <ex:vintage> ?v }} ORDER BY {direction}(?v)");
        //The sort orders the stored forms of the xsd:gYear literals by their value
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildVintageGraph());

        //The rows follow the chronological order (or its reverse for DESC)
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.AreEqual($"{firstYear}^^http://www.w3.org/2001/XMLSchema#gYear", result.SelectResults.Rows[0]["?V"].ToString());
        Assert.AreEqual($"{secondYear}^^http://www.w3.org/2001/XMLSchema#gYear", result.SelectResults.Rows[1]["?V"].ToString());
        Assert.AreEqual($"{thirdYear}^^http://www.w3.org/2001/XMLSchema#gYear", result.SelectResults.Rows[2]["?V"].ToString());
    }

    [TestMethod]
    public void ShouldFilterInfiniteDoubleGreaterThanLargestFiniteBound()
    {
        //The INF double and the 1 double are stored in their XSD forms
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:s1"), new RDFResource("ex:v"), new RDFTypedLiteral("INF", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            new RDFTriple(new RDFResource("ex:s2"), new RDFResource("ex:v"), new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))
        ]);
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?s WHERE { ?s <ex:v> ?x . FILTER(?x > 1e308) }");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        //Only the infinite value exceeds the largest finite bound
        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.AreEqual("ex:s1", result.SelectResults.Rows[0]["?S"].ToString());
    }

    [TestMethod]
    public void ShouldOrderByDoublesIncludingInfinities()
    {
        //The values are inserted out of order and -INF and INF are the extremes of the double value space
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:s1"), new RDFResource("ex:v"), new RDFTypedLiteral("INF", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            new RDFTriple(new RDFResource("ex:s2"), new RDFResource("ex:v"), new RDFTypedLiteral("-INF", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
            new RDFTriple(new RDFResource("ex:s3"), new RDFResource("ex:v"), new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_DOUBLE))
        ]);
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?x WHERE { ?s <ex:v> ?x } ORDER BY ?x");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

        //The rows follow the numeric order
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.AreEqual("-INF^^http://www.w3.org/2001/XMLSchema#double", result.SelectResults.Rows[0]["?X"].ToString());
        Assert.AreEqual("1^^http://www.w3.org/2001/XMLSchema#double", result.SelectResults.Rows[1]["?X"].ToString());
        Assert.AreEqual("INF^^http://www.w3.org/2001/XMLSchema#double", result.SelectResults.Rows[2]["?X"].ToString());
    }

    /// <summary>
    /// Builds five eras with the year as time:generalYear, in scrambled insertion order.
    /// </summary>
    private static RDFGraph BuildEraGraph()
    {
        //The years are inserted out of order and their lexical order differs from their numeric order
        return new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:era1"), new RDFResource("ex:year"), new RDFTypedLiteral("2021", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)),
            new RDFTriple(new RDFResource("ex:era2"), new RDFResource("ex:year"), new RDFTypedLiteral("-0100", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)),
            new RDFTriple(new RDFResource("ex:era3"), new RDFResource("ex:year"), new RDFTypedLiteral("10000", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)),
            new RDFTriple(new RDFResource("ex:era4"), new RDFResource("ex:year"), new RDFTypedLiteral("-0500", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)),
            new RDFTriple(new RDFResource("ex:era5"), new RDFResource("ex:year"), new RDFTypedLiteral("0950", RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR))
        ]);
    }

    [TestMethod]
    public void ShouldOrderByGeneralYearsBySignedPosition()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?y WHERE { ?s <ex:year> ?y } ORDER BY ?y");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildEraGraph());

        //The years are ordered by their signed position and not by their text
        Assert.AreEqual(5, result.SelectResultsCount);
        string[] expectedYears = ["-0500", "-0100", "0950", "2021", "10000"];
        for (int rowIndex = 0; rowIndex < expectedYears.Length; rowIndex++)
            Assert.AreEqual($"{expectedYears[rowIndex]}^^http://www.w3.org/2006/time#generalYear", result.SelectResults.Rows[rowIndex]["?Y"].ToString());
    }

    [TestMethod]
    public void ShouldFilterGeneralYearsLowerThanGivenYear()
    {
        RDFSelectQuery query = RDFSelectQuery.FromString("""
            SELECT ?y
            WHERE { ?s <ex:year> ?y . FILTER(?y < "1000"^^<http://www.w3.org/2006/time#generalYear>) }
            """);
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildEraGraph());

        //Only -0500 and -0100 and 0950 precede the year 1000
        Assert.AreEqual(3, result.SelectResultsCount);
    }

    #endregion

    #region Ground triple patterns

    /// <summary>
    /// Builds a graph where ex:a states two facts (ex:p ex:b and ex:q ex:c) and a literal fact; two more subjects share ex:p ex:b.
    /// </summary>
    private static RDFGraph BuildGroundPatternGraph()
    {
        //Three subjects share "ex:p ex:b" so that the group's variable pattern has a known cardinality of 3
        return new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:a"), new RDFResource("ex:p"), new RDFResource("ex:b")),
            new RDFTriple(new RDFResource("ex:a"), new RDFResource("ex:q"), new RDFResource("ex:c")),
            new RDFTriple(new RDFResource("ex:a"), new RDFResource("ex:r"), new RDFPlainLiteral("lit")),
            new RDFTriple(new RDFResource("ex:x1"), new RDFResource("ex:p"), new RDFResource("ex:b")),
            new RDFTriple(new RDFResource("ex:x2"), new RDFResource("ex:p"), new RDFResource("ex:b"))
        ]);
    }

    [TestMethod]
    [DataRow("<ex:a> <ex:p> <ex:b>", true)]
    [DataRow("<ex:a> <ex:p> <ex:zz>", false)]
    [DataRow("<ex:a> <ex:r> \"lit\"", true)]
    [DataRow("<ex:a> <ex:r> \"other\"", false)]
    public void ShouldEvaluateGroundPatternAskOnGraph(string groundPattern, bool expectedAnswer)
    {
        //The query holds a single ground pattern: the ASK answer is whether that triple is in the graph
        RDFAskQuery query = RDFAskQuery.FromString($"ASK {{ {groundPattern} }}");
        RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(query, BuildGroundPatternGraph());

        //The triple exists only for the first and third pattern (resource object and literal object)
        Assert.AreEqual(expectedAnswer, result.AskResult);
    }

    [TestMethod]
    [DataRow("<ex:a> <ex:q> <ex:c>", 3)]
    [DataRow("<ex:a> <ex:q> <ex:zz>", 0)]
    public void ShouldEvaluateGroundPatternAlongsideVariablePatternOnGraph(string groundPattern, int expectedRows)
    {
        //The ground pattern is a pure existence condition on the rows of the variable pattern
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT ?x WHERE {{ ?x <ex:p> <ex:b> . {groundPattern} }} ORDER BY ?x");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildGroundPatternGraph());

        //Every ?x row survives when the ground triple exists and none does otherwise
        Assert.AreEqual(expectedRows, result.SelectResultsCount);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        if (expectedRows > 0)
        {
            Assert.AreEqual("ex:a", result.SelectResults.Rows[0]["?X"].ToString());
            Assert.AreEqual("ex:x1", result.SelectResults.Rows[1]["?X"].ToString());
            Assert.AreEqual("ex:x2", result.SelectResults.Rows[2]["?X"].ToString());
        }
    }

    [TestMethod]
    [DataRow("<ex:a> <ex:q> <ex:c>")]
    [DataRow("<ex:a> <ex:q> <ex:zz>")]
    public void ShouldKeepLeftRowsOnGroundPatternInsideOptional(string groundPattern)
    {
        //The optional part only holds a ground pattern: it can neither drop the left rows nor add a column
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT ?x WHERE {{ ?x <ex:p> <ex:b> . OPTIONAL {{ {groundPattern} }} }} ORDER BY ?x");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildGroundPatternGraph());

        //The three left rows are kept whether the ground triple exists or not
        Assert.AreEqual(3, result.SelectResultsCount);
        Assert.AreEqual(1, result.SelectResults.Columns.Count);
        Assert.AreEqual("ex:a", result.SelectResults.Rows[0]["?X"].ToString());
        Assert.AreEqual("ex:x1", result.SelectResults.Rows[1]["?X"].ToString());
        Assert.AreEqual("ex:x2", result.SelectResults.Rows[2]["?X"].ToString());
    }

    [TestMethod]
    [DataRow("<ex:a> <ex:q> <ex:c>", 4)]
    [DataRow("<ex:a> <ex:q> <ex:zz>", 1)]
    public void ShouldDropOnlyTheBranchWithFailingGroundPatternInsideUnion(string groundPattern, int expectedRows)
    {
        //The left branch carries the ground pattern while the right branch matches the single ex:a ex:q ex:c triple
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT ?x WHERE {{ {{ ?x <ex:p> <ex:b> . {groundPattern} }} UNION {{ ?x <ex:q> <ex:c> }} }}");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildGroundPatternGraph());

        //The left branch contributes its 3 rows only when the ground triple exists while the right branch always gives 1
        Assert.AreEqual(expectedRows, result.SelectResultsCount);
    }

    [TestMethod]
    [DataRow("<ex:a> <ex:q> <ex:c>")]
    [DataRow("<ex:a> <ex:q> <ex:zz>")]
    public void ShouldRemoveNothingOnGroundPatternInsideMinus(string groundPattern)
    {
        //The minus operand shares no variable with the left side: SPARQL 1.1 (section 8.3) removes nothing
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT ?x WHERE {{ ?x <ex:p> <ex:b> MINUS {{ {groundPattern} }} }}");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildGroundPatternGraph());

        //The three left rows survive whether the ground triple exists or not
        Assert.AreEqual(3, result.SelectResultsCount);
    }

    [TestMethod]
    [DataRow("<ex:a> <ex:q> <ex:c>", 0)]
    [DataRow("<ex:a> <ex:q> <ex:zz>", 3)]
    public void ShouldRemoveEveryRowOnExistingGroundPatternInsideNotExists(string groundPattern, int expectedRows)
    {
        //Unlike MINUS the exists test does not require shared variables: an existing ground triple fails every row
        RDFSelectQuery query = RDFSelectQuery.FromString($"SELECT ?x WHERE {{ ?x <ex:p> <ex:b> . FILTER NOT EXISTS {{ {groundPattern} }} }}");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, BuildGroundPatternGraph());

        //All rows are removed when the ground triple exists and all are kept otherwise
        Assert.AreEqual(expectedRows, result.SelectResultsCount);
    }

    [TestMethod]
    [DataRow("ex:g1", true)]
    [DataRow("ex:g2", false)]
    public void ShouldEvaluateGroundPatternAskOnStoreWithContext(string context, bool expectedAnswer)
    {
        //The triple sits in ex:g1 only
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:g1"), new RDFResource("ex:a"), new RDFResource("ex:p"), new RDFResource("ex:b")),
            new RDFQuadruple(new RDFContext("ex:g2"), new RDFResource("ex:x1"), new RDFResource("ex:p"), new RDFResource("ex:b"))
        ]);
        RDFAskQuery query = RDFAskQuery.FromString($"ASK {{ GRAPH <{context}> {{ <ex:a> <ex:p> <ex:b> }} }}");
        RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(query, store);

        //The answer is true only for the graph that holds the triple
        Assert.AreEqual(expectedAnswer, result.AskResult);
    }

    [TestMethod]
    public void ShouldNotMultiplyRowsOnGroundPatternWithoutContextOnStoreHoldingTripleInTwoContexts()
    {
        //The ground triple sits in two contexts while each ?x matches in a single one
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:g1"), new RDFResource("ex:x1"), new RDFResource("ex:p"), new RDFResource("ex:b")),
            new RDFQuadruple(new RDFContext("ex:g1"), new RDFResource("ex:x2"), new RDFResource("ex:p"), new RDFResource("ex:b")),
            new RDFQuadruple(new RDFContext("ex:g1"), new RDFResource("ex:a"), new RDFResource("ex:q"), new RDFResource("ex:c")),
            new RDFQuadruple(new RDFContext("ex:g2"), new RDFResource("ex:a"), new RDFResource("ex:q"), new RDFResource("ex:c"))
        ]);
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?x WHERE { ?x <ex:p> <ex:b> . <ex:a> <ex:q> <ex:c> } ORDER BY ?x");
        RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

        //The ground pattern only states existence: each ?x is returned once instead of once per context
        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual("ex:x1", result.SelectResults.Rows[0]["?X"].ToString());
        Assert.AreEqual("ex:x2", result.SelectResults.Rows[1]["?X"].ToString());
    }

    [TestMethod]
    [DataRow(true, true)]
    [DataRow(false, false)]
    public void ShouldEvaluateGroundPatternAskOnFederationWithSPARQLEndpoint(bool endpointHoldsTriple, bool expectedAnswer)
    {
        string receivedQuery = "";

        //The endpoint answers the ground pattern with no variables and one empty result when the triple exists
        string mockedResponseXml = endpointHoldsTriple
            ? """
              <?xml version="1.0" encoding="utf-8"?>
              <sparql xmlns="http://www.w3.org/2005/sparql-results#">
                <head />
                <results>
                  <result />
                </results>
              </sparql>
              """
            : """
              <?xml version="1.0" encoding="utf-8"?>
              <sparql xmlns="http://www.w3.org/2005/sparql-results#">
                <head />
                <results />
              </sparql>
              """;
        string endpointPath = $"/RDFQueryEngineTest/ShouldEvaluateGroundPatternAskOnFederationWithSPARQLEndpoint_{endpointHoldsTriple}/sparql";
        server
            .Given(
                Request.Create()
                    .WithPath(endpointPath)
                    .UsingGet()
                    .WithParam(queryParams => queryParams.ContainsKey("query")))
            .RespondWith(
                Response.Create()
                    .WithHeader("Content-Type", "application/sparql-results+xml")
                    .WithCallback(req =>
                    {
                        receivedQuery = req.RawQuery;
                        return new WireMock.ResponseMessage
                        {
                            BodyData = new BodyData
                            {
                                BodyAsString = mockedResponseXml,
                                Encoding = Encoding.UTF8,
                                DetectedBodyType = BodyType.String
                            }
                        };
                    })
                    .WithStatusCode(HttpStatusCode.OK));

        RDFFederation federation = new RDFFederation().AddSPARQLEndpoint(new RDFSPARQLEndpoint(new Uri(server.Url + endpointPath)));
        RDFAskQuery query = RDFAskQuery.FromString("ASK { <ex:a> <ex:p> <ex:b> }");
        RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(query, federation);

        //The ground pattern travels to the endpoint as a query on that very triple
        Assert.IsTrue(HttpUtility.UrlDecode(receivedQuery).Contains("<ex:a> <ex:p> <ex:b>", StringComparison.Ordinal));
        //One empty result means the triple exists while no result means it does not
        Assert.AreEqual(expectedAnswer, result.AskResult);
    }

    #endregion

    #region Empty group solution

    /// <summary>
    /// Builds a graph with the single triple ex:s ex:p ex:o, matched by the ex:p patterns and missed by the ex:nomatch ones.
    /// </summary>
    private static RDFGraph BuildSingleTripleGraph()
        => new RDFGraph([ new RDFTriple(new RDFResource("ex:s"), new RDFResource("ex:p"), new RDFResource("ex:o")) ]);

    [TestMethod]
    public async Task ShouldKeepSolutionOfGroupWithOnlyFailingBindAsync()
    {
        //The division by zero is an error: the variable stays unbound but the solution of the empty group survives
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?r WHERE { BIND(1 / 0 AS ?r) }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(result.SelectResults.Rows[0].IsNull("?R"));
    }

    [TestMethod]
    public async Task ShouldKeepSolutionOfEmptyGroupWithFailingProjectionExpressionAsync()
    {
        //The projection expression fails on the single solution of the empty group and leaves its variable unbound
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (1 / 0 AS ?x) WHERE { }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.IsTrue(result.SelectResults.Rows[0].IsNull("?X"));
    }

    [TestMethod]
    public async Task ShouldAnswerTrueToAskWithOnlyFailingBindAsync()
    {
        //The failing BIND keeps the solution of the empty group so the ASK has something to confirm
        RDFAskQuery query = RDFAskQuery.FromString("ASK { BIND(1 / 0 AS ?r) }");
        RDFAskQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.IsTrue(result.AskResult);
    }

    [TestMethod]
    public async Task ShouldKeepFailingBindSolutionInUnionBranchAsync()
    {
        //Each branch is a group made of a sole BIND: the failing one still yields its solution with ?r unbound
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?r WHERE { { BIND(1 AS ?r) } UNION { BIND(1 / 0 AS ?r) } }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.AreEqual(2, result.SelectResultsCount);
        Assert.AreEqual(1, result.SelectResults.Rows.Cast<DataRow>().Count(row => row.IsNull("?R")));
        Assert.AreEqual(1, result.SelectResults.Rows.Cast<DataRow>().Count(row => !row.IsNull("?R") && row["?R"].ToString() == $"1^^{RDFVocabulary.XSD.INTEGER}"));
    }

    [TestMethod]
    public async Task ShouldKeepFailingBindSolutionInNestedGroupJoinedWithPatternAsync()
    {
        //The nested group has the single empty solution extended by the failing BIND: joined with the pattern it keeps the pattern row
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?s ?r WHERE { ?s <ex:p> ?o { BIND(1 / 0 AS ?r) } }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(BuildSingleTripleGraph());

        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.AreEqual("ex:s", result.SelectResults.Rows[0]["?S"].ToString());
        Assert.IsTrue(result.SelectResults.Rows[0].IsNull("?R"));
    }

    [TestMethod]
    public async Task ShouldNotInventSolutionForBindAfterPatternWithoutMatchesAsync()
    {
        //The pattern has no solution and the BIND extends no solution: nothing is created
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?r WHERE { ?s <ex:nomatch> ?o BIND(1 AS ?r) }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(BuildSingleTripleGraph());

        Assert.AreEqual(0, result.SelectResultsCount);
    }

    [TestMethod]
    public async Task ShouldNotInventSolutionForProjectionExpressionAfterPatternWithoutMatchesAsync()
    {
        //The projection expression is evaluated on the solutions of the pattern: there are none
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (1 AS ?x) WHERE { ?s <ex:nomatch> ?o }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(BuildSingleTripleGraph());

        Assert.AreEqual(0, result.SelectResultsCount);
    }

    [TestMethod]
    public async Task ShouldAnswerFalseToAskWithBindAfterPatternWithoutMatchesAsync()
    {
        //No solution of the pattern means no solution to extend: the BIND cannot make the ASK true
        RDFAskQuery query = RDFAskQuery.FromString("ASK { ?s <ex:nomatch> ?o BIND(1 AS ?r) }");
        RDFAskQueryResult result = await query.ApplyToGraphAsync(BuildSingleTripleGraph());

        Assert.IsFalse(result.AskResult);
    }

    [TestMethod]
    public async Task ShouldReturnSingleSolutionWithoutVariablesForEmptyGroupAsync()
    {
        //The empty group has exactly one solution and it binds no variable
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT * WHERE { }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.AreEqual(0, result.SelectResults.Columns.Count);
        Assert.AreEqual(1, result.SelectResultsCount);
    }

    [TestMethod]
    public async Task ShouldAnswerTrueToAskOnEmptyGroupAsync()
    {
        //The single solution of the empty group is enough to confirm the ASK
        RDFAskQuery query = RDFAskQuery.FromString("ASK { }");
        RDFAskQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.IsTrue(result.AskResult);
    }

    [TestMethod]
    public async Task ShouldBindValueOnSingleSolutionOfGroupWithOnlyBindAsync()
    {
        //The succeeding BIND extends the single solution of the empty group with its value
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT ?r WHERE { BIND(1 AS ?r) }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.AreEqual($"1^^{RDFVocabulary.XSD.INTEGER}", result.SelectResults.Rows[0]["?R"].ToString());
    }

    [TestMethod]
    public async Task ShouldProjectValueOnSingleSolutionOfEmptyGroupAsync()
    {
        //The projection expression extends the single solution of the empty group with its value
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (1 AS ?x) WHERE { }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.AreEqual($"1^^{RDFVocabulary.XSD.INTEGER}", result.SelectResults.Rows[0]["?X"].ToString());
    }

    [TestMethod]
    public async Task ShouldNotConstructGroundTemplateWithoutSolutionAsync()
    {
        //A template is instantiated once per solution: without any solution nothing is constructed
        RDFConstructQuery query = RDFConstructQuery.FromString("CONSTRUCT { <http://ex.org/x> <http://ex.org/y> <http://ex.org/z> } WHERE { ?s <http://ex.org/nomatch> ?o }");
        RDFConstructQueryResult result = await query.ApplyToGraphAsync(BuildSingleTripleGraph());

        Assert.AreEqual(0, result.ConstructResultsCount);
    }

    [TestMethod]
    public async Task ShouldConstructGroundTemplateOnEmptyGroupAsync()
    {
        //The single solution of the empty group instantiates the ground template once
        RDFConstructQuery query = RDFConstructQuery.FromString("CONSTRUCT { <http://ex.org/x> <http://ex.org/y> <http://ex.org/z> } WHERE { }");
        RDFConstructQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.AreEqual(1, result.ConstructResultsCount);
    }

    [TestMethod]
    public async Task ShouldReadDayOfCalendarDateWithPositiveOffsetAsWrittenAsync()
    {
        //The offset says in which time zone the date is reckoned: the day stays the written one
        RDFSelectQuery query = RDFSelectQuery.FromString("SELECT (DAY(\"2021-05-07+02:00\"^^<http://www.w3.org/2001/XMLSchema#date>) AS ?d) WHERE { }");
        RDFSelectQueryResult result = await query.ApplyToGraphAsync(new RDFGraph());

        Assert.AreEqual(1, result.SelectResultsCount);
        Assert.AreEqual($"7^^{RDFVocabulary.XSD.INTEGER}", result.SelectResults.Rows[0]["?D"].ToString());
    }

    #endregion

    #endregion
}