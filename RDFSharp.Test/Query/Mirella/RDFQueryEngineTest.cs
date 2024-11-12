/*
   Copyright 2012-2024 Marco De Salvo

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
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;
using WireMock.Util;
using WireMock.Types;
using System.Web;
using System.Threading.Tasks;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFQueryEngineTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        #region Tests
        [TestMethod]
        public void ShouldCreateQueryEngine()
        {
            RDFQueryEngine queryEngine = new RDFQueryEngine();

            Assert.IsNotNull(queryEngine);
            Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
            Assert.IsNotNull(queryEngine.QueryMemberResultTables);
            Assert.IsTrue(RDFQueryEngine.SystemString.Equals(typeof(string)));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), string.Empty));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_UnionsThenOptional()
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
                    .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_UnionsThenInner()
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
                    .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 2);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_BindAndProjectionExpressions()
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
                .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFLengthExpression(new RDFVariable("?XBIND")))
                .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 5);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_BindAndProjectionExpressionsSortedByBind()
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
                .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFLengthExpression(new RDFVariable("?XBIND")))
                .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 5);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:whoeverBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:paperinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_BindAndProjectionExpressionsSortedByProjectionExpressions()
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
                .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFLengthExpression(new RDFVariable("?XBIND")))
                .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 5);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperinoo"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinooBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"16^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_BindAndProjectionExpressionsSortedByProjectionExpressionsThenByBind()
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
                .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFLengthExpression(new RDFVariable("?XBIND")))
                .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 5);
            Assert.IsTrue(result.SelectResultsCount == 4);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:whoev"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:whoevBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"12^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:doggy"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:pippo"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:pippoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"12^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:topolinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?X"].ToString(), "ex:paperinoo"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XBIND"].ToString(), "ex:paperinooBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XBINDLENGTH"].ToString(), $"16^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?NEVERBOUND"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_BindAndProjectionExpressionsSortedByBindThenByProjectionExpressions()
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
                .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFLengthExpression(new RDFVariable("?XBIND")))
                .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 5);
            Assert.IsTrue(result.SelectResultsCount == 4);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:whoev"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:whoevBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"12^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:doggy"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:pippo"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:pippoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"12^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?X"].ToString(), "ex:paperinoo"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XBIND"].ToString(), "ex:paperinooBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XBINDLENGTH"].ToString(), $"16^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?NEVERBOUND"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_ProjectionExpressionsSortedByUnboundProjectionExpressions()
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 4);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XFLOOR"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XFLOOR"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XFLOOR"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?XFLOOR"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_StarWithBind()
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 5);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableConstantExpressionBoundValue()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:pluto")), new RDFVariable("?Y"))));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableConstantExpressionProjectedValue()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFConstantExpression(new RDFResource("ex:pluto")));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableNowExpressionProjectedValue()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFNowExpression());
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(!string.IsNullOrEmpty(result.SelectResults.Rows[0]["?Y"].ToString()));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableComplexVariableLessExpressionProjectedValue1()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFLengthExpression(
                    new RDFConcatExpression(new RDFConstantExpression(new RDFPlainLiteral("hello","en-US")),
                    new RDFLengthExpression(new RDFMD5Expression(new RDFConstantExpression(new RDFPlainLiteral("hello","en-US")))))));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals(new RDFTypedLiteral("7", RDFModelEnums.RDFDatatypes.XSD_INTEGER).ToString()));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableComplexVariableLessExpressionProjectedValue2()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFAddExpression(
                    new RDFLengthExpression(new RDFConstantExpression(new RDFPlainLiteral("hello", "en-US"))),
                    new RDFAddExpression(new RDFConstantExpression(new RDFTypedLiteral("3", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
                        new RDFMultiplyExpression(new RDFConstantExpression(new RDFTypedLiteral("3", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)), new RDFTypedLiteral("4.5", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals(new RDFTypedLiteral("21.5", RDFModelEnums.RDFDatatypes.XSD_DOUBLE).ToString()));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableComplexVariableLessExpressionProjectedValue3()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFConditionalExpression(
                    new RDFComparisonExpression(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFRandExpression(), new RDFConstantExpression(new RDFTypedLiteral("0.50", RDFModelEnums.RDFDatatypes.XSD_FLOAT))),
                    new RDFConstantExpression(new RDFPlainLiteral(">0.50")),
                    new RDFConstantExpression(new RDFPlainLiteral("<=0.50"))));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals(">0.50") || result.SelectResults.Rows[0]["?Y"].ToString().Equals("<=0.50"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableRandExpressionProjectedValue()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFRandExpression());
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(!string.IsNullOrEmpty(result.SelectResults.Rows[0]["?Y"].ToString()));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableFloorExpressionProjectedValue()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?Y"), new RDFFloorExpression(new RDFConstantExpression(new RDFTypedLiteral("3.35", RDFModelEnums.RDFDatatypes.XSD_FLOAT))));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals($"3^^{RDFVocabulary.XSD.DOUBLE}"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableFloorExpressionProjectedValueFromSubQuery1()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddSubQuery(new RDFSelectQuery()
                    .AddProjectionVariable(new RDFVariable("?Y"), new RDFFloorExpression(new RDFConstantExpression(new RDFTypedLiteral("3.35", RDFModelEnums.RDFDatatypes.XSD_FLOAT)))))
                .AddProjectionVariable(new RDFVariable("?Z"), new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(result.SelectResults.Rows[0]["?Z"].ToString().Equals($"4^^{RDFVocabulary.XSD.DOUBLE}"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraph_EmptyTableFloorExpressionProjectedValueFromSubQuery2()
        {
            RDFGraph graph = new RDFGraph();

            RDFSelectQuery query = new RDFSelectQuery()
                .AddSubQuery(new RDFSelectQuery()
                    .AddProjectionVariable(new RDFVariable("?Y"), new RDFFloorExpression(new RDFConstantExpression(new RDFTypedLiteral("3.35", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
                    .AddProjectionVariable(new RDFVariable("?Z"), new RDFAddExpression(new RDFVariable("?Y"), new RDFConstantExpression(new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)))));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(result.SelectResults.Rows[0]["?Y"].ToString().Equals($"3^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(result.SelectResults.Rows[0]["?Z"].ToString().Equals($"4^^{RDFVocabulary.XSD.DOUBLE}"));
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

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:baubau"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")).UnionWithNext())
                        .AddPattern(new RDFPattern(new RDFResource("ex:snoopie"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                        .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasColor"), new RDFVariable("?C"))))
                    .Optional())
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 4);
            Assert.IsTrue(result.SelectResultsCount == 2);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:paperoga"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "white"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:paperoga"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?C"].ToString(), "white"));
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

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:baubau"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")).UnionWithNext())
                        .AddPattern(new RDFPattern(new RDFResource("ex:snoopie"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                        .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasColor"), new RDFVariable("?C")).Optional()))
                    .Optional())
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 4);
            Assert.IsTrue(result.SelectResultsCount == 4);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:linus"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:paperoga"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?C"].ToString(), "white"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:linus"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?C"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?Y"].ToString(), "ex:paperoga"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?C"].ToString(), "white"));
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

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:fido"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:baubau"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")).UnionWithNext())
                        .AddPattern(new RDFPattern(new RDFResource("ex:snoopie"), new RDFResource("ex:dogOf"), new RDFVariable("?Y")))
                        .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:hasColor"), new RDFVariable("?C")).Optional()))
                    .Optional())
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 4);
            Assert.IsTrue(result.SelectResultsCount == 6);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:linus"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:paperoga"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?C"].ToString(), "white"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:linus"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?C"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?Y"].ToString(), "ex:paperoga"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?C"].ToString(), "white"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[4]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[4]["?N"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[4]["?Y"].ToString(), "ex:linus"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[4]["?C"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[5]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[5]["?N"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[5]["?Y"].ToString(), "ex:paperoga"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[5]["?C"].ToString(), "white"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 0);
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraphWithNoResultsBecauseNoEvaluableQueryMembers()
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 0);
            Assert.IsTrue(result.SelectResultsCount == 0);
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), string.Empty));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 4);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT"));
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Pluto@IT"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnStore_BindAndProjectionExpressions()
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
                .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFLengthExpression(new RDFVariable("?XBIND")))
                .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 5);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnFederationWithResults()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
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
            Assert.IsTrue(result.SelectResults.Columns.Count == 4);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnFederation_BindAndProjectionExpressions()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
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
                .AddProjectionVariable(new RDFVariable("?XBINDLENGTH"), new RDFLengthExpression(new RDFVariable("?XBIND")))
                .AddProjectionVariable(new RDFVariable("?NEVERBOUND"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, federation);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 5);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBIND"].ToString(), "ex:paperinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBIND"].ToString(), "ex:topolinoBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?XBINDLENGTH"].ToString(), $"15^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NEVERBOUND"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBIND"].ToString(), "ex:whoeverBIND"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?XBINDLENGTH"].ToString(), $"14^^{RDFVocabulary.XSD.INTEGER}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NEVERBOUND"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpoints()
        {
            string receivedQuery1 = "";
            string receivedQuery2 = "";

            string mockedResponseXml1 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:fido</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:paperino</uri>
      </binding>
    </result>
  </results>
</sparql>";
            string mockedResponseXml2 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:balto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:whoever</uri>
      </binding>
    </result>
  </results>
</sparql>";
            
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData() 
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData() 
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
                .AddSPARQLEndpoint(endpoint2, new RDFSPARQLEndpointQueryOptions() { QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post });
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));
            DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, federation).SelectResults;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));

            string qString = query.ToString();
            Assert.IsNotNull(receivedQuery1);
            Assert.IsTrue(string.Equals(receivedQuery1, $"?query={HttpUtility.UrlEncode(qString)}"));
            Assert.IsNotNull(receivedQuery2);
            Assert.IsTrue(string.Equals(receivedQuery2, $"query={HttpUtility.UrlEncode(qString)}"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneGivingEmptyResult()
        {
            string receivedQuery1 = "";
            string receivedQuery2 = "";

            string mockedResponseXml1 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:fido</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:paperino</uri>
      </binding>
    </result>
  </results>
</sparql>";
            string mockedResponseXml2 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:balto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:whoever</uri>
      </binding>
    </result>
  </results>
</sparql>";
            
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData() 
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData() 
                                    {
                                        BodyAsString = mockedResponseXml2,
                                        Encoding = Encoding.UTF8,
                                        DetectedBodyType = BodyType.String
                                    }
                                }; 
                            })
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(750));

            RDFSPARQLEndpoint endpoint1 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneGivingEmptyResult1/sparql"));
            RDFSPARQLEndpoint endpoint2 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneGivingEmptyResult2/sparql"));
            RDFFederation federation = new RDFFederation()
                .AddSPARQLEndpoint(endpoint1)
                .AddSPARQLEndpoint(endpoint2, new RDFSPARQLEndpointQueryOptions() { 
                    ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult, 
                    QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post,
                    TimeoutMilliseconds = 250 });
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));
            DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, federation).SelectResults;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));

            string qString = query.ToString();
            Assert.IsNotNull(receivedQuery1);
            Assert.IsTrue(string.Equals(receivedQuery1, $"?query={HttpUtility.UrlEncode(qString)}"));
            Assert.IsNotNull(receivedQuery2);
            Assert.IsTrue(string.Equals(receivedQuery2, string.Empty));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException()
        {
            string receivedQuery1 = "";
            string receivedQuery2 = "";

            string mockedResponseXml1 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:fido</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:paperino</uri>
      </binding>
    </result>
  </results>
</sparql>";
            string mockedResponseXml2 =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:balto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:whoever</uri>
      </binding>
    </result>
  </results>
</sparql>";
            
            server
                .Given(
                    Request.Create()
                           .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException1/sparql")
                           .UsingGet()
                           .WithParam(queryParams => queryParams.ContainsKey("query")))
                .RespondWith(
                    Response.Create()
                            .WithHeader("Content-Type", "application/sparql-results+xml")
                            .WithCallback(req => 
                            { 
                                receivedQuery1 = req.RawQuery;
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData() 
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
                           .WithPath("/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException2/sparql")
                           .UsingPost())
                .RespondWith(
                    Response.Create()
                            .WithHeader("Content-Type", "application/sparql-results+xml")
                            .WithCallback(req => 
                            { 
                                receivedQuery2 = req.Body;
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData() 
                                    {
                                        BodyAsString = mockedResponseXml2,
                                        Encoding = Encoding.UTF8,
                                        DetectedBodyType = BodyType.String
                                    }
                                }; 
                            })
                            .WithStatusCode(HttpStatusCode.OK)
                            .WithDelay(750));

            RDFSPARQLEndpoint endpoint1 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException1/sparql"));
            RDFSPARQLEndpoint endpoint2 = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFQueryEngineTest/ShouldEvaluateSelectQueryOnFederationWithResults_SPARQLEndpointsOneThrowingException2/sparql"));
            RDFFederation federation = new RDFFederation()
                .AddSPARQLEndpoint(endpoint1)
                .AddSPARQLEndpoint(endpoint2, new RDFSPARQLEndpointQueryOptions() { 
                    ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.ThrowException, 
                    QueryMethod = RDFQueryEnums.RDFSPARQLEndpointQueryMethods.Post,
                    TimeoutMilliseconds = 250 });
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));

            Assert.ThrowsException<RDFQueryException>(() => new RDFQueryEngine().EvaluateSelectQuery(query, federation));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraphWithServicePatternGroup()
        {
            string receivedQuery = "";
            string expectedQuery = string.Concat("?query=SELECT *", Environment.NewLine, "WHERE {", Environment.NewLine, "  {", Environment.NewLine, "    ?Y <ex:dogOf> ?X .", Environment.NewLine, "  }", Environment.NewLine, "}", Environment.NewLine);
            string mockedResponseXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:fido</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:paperino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:balto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:whoever</uri>
      </binding>
    </result>
  </results>
</sparql>";
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData()
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
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AsService(endpoint));
            DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));

            //Proves that the pattern group has been sent as an equivalent SELECT * to the given endpoint
            Assert.IsNotNull(receivedQuery);
            Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery), expectedQuery));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraphWithValuesInjectedIntoServicePatternGroup()
        {
            string receivedQuery = "";
            string expectedQuery =
@"?query=SELECT *
WHERE {
  {
    VALUES (?Y ?X) {
      ( <ex:pluto> <ex:topolino> )
    } .
    ?Y <ex:dogOf> ?X .
  }
}
";
            string mockedResponseXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:fido</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:paperino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:balto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:whoever</uri>
      </binding>
    </result>
  </results>
</sparql>";
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData()
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
                .AddPatternGroup(new RDFPatternGroup()
                    .AddValues(new RDFValues()
                        .AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")])
                        .AddColumn(new RDFVariable("?X"), [new RDFResource("ex:topolino")]))
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AsService(endpoint));
            DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto")); //Here it is mocked in XML, but real SPARQL servers will have it thanks to injected VALUES
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));

            //Proves that the pattern group has been sent as an equivalent SELECT * to the given endpoint
            Assert.IsNotNull(receivedQuery);
            Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery), expectedQuery));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraphWithServicePatternGroupAndCombineResults()
        {
            string receivedQuery = "";
            string expectedQuery = string.Concat("?query=SELECT *", Environment.NewLine, "WHERE {", Environment.NewLine, "  {", Environment.NewLine, "    ?Y <ex:dogOf> ?X .", Environment.NewLine, "  }", Environment.NewLine, "}", Environment.NewLine);
            string mockedResponseXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:fido</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:paperino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:balto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:whoever</uri>
      </binding>
    </result>
  </results>
</sparql>";
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData()
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
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AsService(endpoint));
            DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));

            //Proves that the pattern group has been sent as an equivalent SELECT * to the given endpoint
            Assert.IsNotNull(receivedQuery);
            Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery), expectedQuery));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraphWithDifferentServicePatternGroupsAndCombineResults()
        {
            string receivedQuery1 = "";
            string expectedQuery1 = string.Concat("?query=SELECT *", Environment.NewLine, "WHERE {", Environment.NewLine, "  {", Environment.NewLine, "    ?Y <ex:dogOf> ?X .", Environment.NewLine, "  }", Environment.NewLine, "}", Environment.NewLine);
            string mockedResponse1Xml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
  </results>
</sparql>";
            string receivedQuery2 = "";
            string expectedQuery2 = string.Concat("?query=SELECT *", Environment.NewLine, "WHERE {", Environment.NewLine, "  {", Environment.NewLine, "    ?Y <ex:dogOf> ?X .", Environment.NewLine, "  }", Environment.NewLine, "}", Environment.NewLine);
            string mockedResponse2Xml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:fido</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:paperino</uri>
      </binding>
    </result>
    <result>
      <binding name=""?Y"">
        <uri>ex:balto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:whoever</uri>
      </binding>
    </result>
  </results>
</sparql>";
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData()
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
                                return new WireMock.ResponseMessage()
                                {
                                    BodyData = new BodyData()
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
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AsService(endpoint1))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AsService(endpoint2));
            DataTable result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph()).SelectResults;

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));

            //Proves that the pattern groups have been sent as equivalent SELECT * to the given endpoints
            Assert.IsNotNull(receivedQuery1);
            Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery1), expectedQuery1));
            Assert.IsNotNull(receivedQuery2);
            Assert.IsTrue(string.Equals(HttpUtility.UrlDecode(receivedQuery2), expectedQuery2));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx2"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "bnode:12345"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx2"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResultsCount == 2);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx1"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?SUBJECT"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?PREDICATE"].ToString(), "ex:hasName"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[1]["?OBJECT"].ToString(), "Mickey Mouse@EN-US"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 4);
            Assert.IsTrue(result.DescribeResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx1"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "bnode:12345"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "bnode:12345"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResultsCount == 1);
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.DescribeResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResultsCount == 0);
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
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResultsCount == 0);
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
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 2);
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[1]["?OBJECT"].ToString(), "ex:dog"));
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
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
                .AddModifier(new RDFLimitModifier(2));
            RDFConstructQueryResult result = new RDFQueryEngine().EvaluateConstructQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 1);
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.ConstructResults.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
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
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 0);
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
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 0);
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
        public void ShouldEvaluateAskQueryFalseBecauseNoEvaluableQueryMembers()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
            ]);

            RDFAskQuery query = new RDFAskQuery();
            RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
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
                    .AddFilter(new RDFBoundFilter(new RDFVariable("?N"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")]))))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluateQueryMembers(evaluableQueryMembers, graph);

            Assert.IsNotNull(queryEngine.QueryMemberResultTables);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.Count == 2);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count == 3);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count == 2);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(),"ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(),"ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(),"Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(),"ex:fido"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(),"ex:paperino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(),"Donald Duck@EN-US"));
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Columns.Count == 1);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows.Count == 1);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows[0]["?Y"].ToString(),"ex:pluto"));
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

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .AddFilter(new RDFBoundFilter(new RDFVariable("?N")))
                    .Optional())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")])))
                    .UnionWithNext())
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluateQueryMembers(evaluableQueryMembers, graph);

            Assert.IsNotNull(queryEngine.QueryMemberResultTables);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.Count == 2);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsTrue((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count == 3);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count == 2);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(),"ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(),"ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(),"Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(),"ex:fido"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(),"ex:paperino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(),"Donald Duck@EN-US"));
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsTrue((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Columns.Count == 1);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows.Count == 1);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows[0]["?Y"].ToString(),"ex:pluto"));
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

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .AddFilter(new RDFBoundFilter(new RDFVariable("?N")))
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")])))
                    .Optional())
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluateQueryMembers(evaluableQueryMembers, graph);

            Assert.IsNotNull(queryEngine.QueryMemberResultTables);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.Count == 2);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsTrue((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count == 3);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count == 2);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(),"ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(),"ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(),"Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(),"ex:fido"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(),"ex:paperino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(),"Donald Duck@EN-US"));
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsTrue((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Columns.Count == 1);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows.Count == 1);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows[0]["?Y"].ToString(),"ex:pluto"));
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
                    .AddFilter(new RDFBoundFilter(new RDFVariable("?N"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), [new RDFResource("ex:pluto")]))))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluateQueryMembers(evaluableQueryMembers, graph);

            Assert.IsNotNull(queryEngine.QueryMemberResultTables);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.Count == 2);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count == 3);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count == 0);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(1).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Columns.Count == 1);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows.Count == 1);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(1).Value.Rows[0]["?Y"].ToString(),"ex:pluto"));
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
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value.Count == 2);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count == 2);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count == 3);
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?Y"].ToString(),"ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?X"].ToString(),"ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[1]["?Y"].ToString(),"ex:fido"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[1]["?X"].ToString(),"ex:paperino"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[2]["?Y"].ToString(),"ex:balto"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[2]["?X"].ToString(),"ex:whoever"));
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Columns.Count == 2);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows.Count == 2);
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[0]["?X"].ToString(),"ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[0]["?N"].ToString(),"Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[1]["?X"].ToString(),"ex:paperino"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows[1]["?N"].ToString(),"Donald Duck@EN-US"));
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

            RDFPatternGroup patternGroup = new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")).UnionWithNext())
                .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional());
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluatePatternGroup(patternGroup, graph);

            Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value.Count == 2);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsTrue((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count == 2);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count == 0);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsTrue((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Columns.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[1].Rows.Count == 0);
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
                    .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:dogOf")))
                    .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:hasName"))));
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluatePatternGroup(patternGroup, graph);

            Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value.Count == 1);
            Assert.IsFalse(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count == 2);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count == 2);
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?Y"].ToString(),"ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?N"].ToString(),"Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[1]["?Y"].ToString(),"ex:fido"));
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[1]["?N"].ToString(),"Donald Duck@EN-US"));
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
                    .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:dogOf")))
                    .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:hasName2"))));
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluatePatternGroup(patternGroup, graph);

            Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value.Count == 1);
            Assert.IsFalse(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count == 2);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count == 0);
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
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count == 1);
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?Y"].ToString(),"ex:pluto"));
            Assert.IsTrue(patternGroup.GetFilters().Single() is RDFValuesFilter);
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
                .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), null)); //UNDEF
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluatePatternGroup(patternGroup, graph);

            Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsTrue((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties[RDFQueryEngine.IsOptional]); //UNDEF => optional
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Columns.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows.Count == 1);
            Assert.IsTrue(string.Equals(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value[0].Rows[0]["?Y"].ToString(), string.Empty));
            Assert.IsTrue(patternGroup.GetFilters().Single() is RDFValuesFilter);
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
                .AddFilter(new RDFExistsFilter(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))));
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluatePatternGroup(patternGroup, graph);

            Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value.Count == 0);
            Assert.IsTrue(patternGroup.GetFilters().Single() is RDFExistsFilter);

            RDFExistsFilter existsFilter = (RDFExistsFilter)patternGroup.GetFilters().Single();
            Assert.IsNotNull(existsFilter.PatternResults);
            Assert.IsTrue(existsFilter.PatternResults.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)existsFilter.PatternResults.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(existsFilter.PatternResults.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)existsFilter.PatternResults.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(existsFilter.PatternResults.Columns.Count == 2);
            Assert.IsTrue(existsFilter.PatternResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(existsFilter.PatternResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(existsFilter.PatternResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(existsFilter.PatternResults.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(existsFilter.PatternResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(existsFilter.PatternResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(existsFilter.PatternResults.Rows[2]["?X"].ToString(), "ex:whoever"));
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
                .AddFilter(new RDFExistsFilter(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X"))));
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluatePatternGroup(patternGroup, graph);

            Assert.IsNotNull(queryEngine.PatternGroupMemberResultTables);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.PatternGroupMemberResultTables.ElementAt(0).Value.Count == 0);
            Assert.IsTrue(patternGroup.GetFilters().Single() is RDFExistsFilter);

            RDFExistsFilter existsFilter = (RDFExistsFilter)patternGroup.GetFilters().Single();
            Assert.IsNotNull(existsFilter.PatternResults);
            Assert.IsTrue(existsFilter.PatternResults.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)existsFilter.PatternResults.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(existsFilter.PatternResults.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)existsFilter.PatternResults.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(existsFilter.PatternResults.Columns.Count == 2);
            Assert.IsTrue(existsFilter.PatternResults.Rows.Count == 0);
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
            Assert.IsTrue(queryEngine.QueryMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ContainsKey(patternGroup.QueryMemberID));
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count == 3);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count == 3);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?N"].ToString(), string.Empty));
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
            Assert.IsTrue(queryEngine.QueryMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ContainsKey(patternGroup.QueryMemberID));
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsTrue((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count == 3);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count == 3);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?N"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldFinalizePatternGroupWithUnionWithNext()
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
                .UnionWithNext();
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluatePatternGroup(patternGroup, graph); //Just to obtain real pattern tables (instead of mocking them)
            queryEngine.FinalizePatternGroup(patternGroup);

            Assert.IsNotNull(queryEngine.QueryMemberResultTables);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ContainsKey(patternGroup.QueryMemberID));
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsTrue((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count == 3);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count == 3);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[1]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[2]["?N"].ToString(), string.Empty));
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
                .AddFilter(new RDFRegexFilter(new RDFVariable("?Y"), new Regex("^ex:[a-zA-Z]+o$")))
                .AddFilter(new RDFInFilter(new RDFVariable("?Y"), [new RDFResource("ex:pluto")]));
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluatePatternGroup(patternGroup, graph); //Just to obtain real pattern tables (instead of mocking them)
            queryEngine.FinalizePatternGroup(patternGroup); //Just to obtain real pattern group table  (instead of mocking it)
            queryEngine.ApplyFilters(patternGroup);

            Assert.IsNotNull(queryEngine.QueryMemberResultTables);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.Count == 1);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ContainsKey(patternGroup.QueryMemberID));
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.IsOptional));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.IsOptional]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties.ContainsKey(RDFQueryEngine.JoinAsUnion));
            Assert.IsFalse((bool)queryEngine.QueryMemberResultTables.ElementAt(0).Value.ExtendedProperties[RDFQueryEngine.JoinAsUnion]);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Columns.Count == 3);
            Assert.IsTrue(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows.Count == 1);
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(queryEngine.QueryMemberResultTables.ElementAt(0).Value.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            queryEngine.EvaluatePatternGroup(query.GetPatternGroups().Single(), graph); //Just to obtain real pattern tables (instead of mocking them)
            queryEngine.FinalizePatternGroup(query.GetPatternGroups().Single()); //Just to obtain real pattern group table  (instead of mocking it)
            queryEngine.ApplyFilters(query.GetPatternGroups().Single()); //Just to obtain real filtered table (instead of mocking it)
            DataTable resultTable = queryEngine.ApplyModifiers(query, queryEngine.QueryMemberResultTables.ElementAt(0).Value);

            Assert.IsTrue(resultTable.Columns.Count == 2);
            Assert.IsTrue(resultTable.Rows.Count == 2);
            Assert.IsTrue(string.Equals(resultTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(resultTable.Rows[0]["?SAMPLE_X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(resultTable.Rows[1]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(resultTable.Rows[1]["?SAMPLE_X"].ToString(), "ex:whoever"));
        }

        [TestMethod]
        public void ShouldFillTemplateTriples()
        {
            List<RDFPattern> templates =
            [
                new RDFPattern(new RDFResource("ex:bracco"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")),
                new RDFPattern(new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog"))
            ];
            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = DBNull.Value.ToString(); //Will not be considered, since null values are not allowed
            row2["?X"] = "ex:paperino";
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = "hello"; //Will not be considered, since literal values are not allowed in subject
            row3["?X"] = "ex:paperino";
            table.Rows.Add(row3);
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable filledTable = queryEngine.FillTemplates(templates, table, false);

            Assert.IsNotNull(filledTable);
            Assert.IsTrue(filledTable.Columns.Count == 3);
            Assert.IsTrue(filledTable.Rows.Count == 3);
            Assert.IsTrue(string.Equals(filledTable.Rows[0]["?SUBJECT"].ToString(), "ex:bracco"));
            Assert.IsTrue(string.Equals(filledTable.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(filledTable.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(filledTable.Rows[1]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(filledTable.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(filledTable.Rows[1]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(filledTable.Rows[2]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(filledTable.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(filledTable.Rows[2]["?OBJECT"].ToString(), "ex:dog"));
        }

        [TestMethod]
        public void ShouldFillTemplateQuadruples()
        {
            List<RDFPattern> templates =
            [
                new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:bracco"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")),
                new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog"))
            ];
            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = DBNull.Value.ToString(); //Will not be considered, since null values are not allowed
            row2["?X"] = "ex:paperino";
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = "hello"; //Will not be considered, since literal values are not allowed in context
            row3["?X"] = "ex:paperino";
            table.Rows.Add(row3);
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable filledTable = queryEngine.FillTemplates(templates, table, true);

            Assert.IsNotNull(filledTable);
            Assert.IsTrue(filledTable.Columns.Count == 4);
            Assert.IsTrue(filledTable.Rows.Count == 3);
            Assert.IsTrue(string.Equals(filledTable.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(filledTable.Rows[0]["?SUBJECT"].ToString(), "ex:bracco"));
            Assert.IsTrue(string.Equals(filledTable.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(filledTable.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(filledTable.Rows[1]["?CONTEXT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(filledTable.Rows[1]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(filledTable.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(filledTable.Rows[1]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(filledTable.Rows[2]["?CONTEXT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(filledTable.Rows[2]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(filledTable.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(filledTable.Rows[2]["?OBJECT"].ToString(), "ex:dog"));
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

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            row0["?N"] = "Mickey Mouse@EN-US";
            row0["?C"] = null;
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            row1["?N"] = "Donald Duck@EN-US";
            row1["?C"] = null;
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = "ex:balto";
            row2["?X"] = "ex:whoever";
            row2["?N"] = null;
            row2["?C"] = null;
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = null;
            row3["?X"] = null;
            row3["?N"] = null;
            row3["?C"] = "green@EN";
            table.Rows.Add(row3);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, graph, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 12);
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

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFResource("ex:balto"))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            row0["?N"] = "Mickey Mouse@EN-US";
            row0["?C"] = null;
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            row1["?N"] = "Donald Duck@EN-US";
            row1["?C"] = null;
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = "ex:balto";
            row2["?X"] = "ex:whoever";
            row2["?N"] = null;
            row2["?C"] = null;
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = null;
            row3["?X"] = null;
            row3["?N"] = null;
            row3["?C"] = "green@EN";
            table.Rows.Add(row3);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, graph, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 2);
        }

        [TestMethod]
        public void ShouldDescribeResourceTermsOnFederation()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino"))
            ]);
            RDFAsyncGraph asyncGraph = new RDFAsyncGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US"))
            });
            RDFMemoryStore store = new RDFMemoryStore(
            [
                new RDFQuadruple(new RDFContext(), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFQuadruple(new RDFContext(), new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            ]);
            RDFAsyncStore asyncStore = new RDFAsyncStore(
                new RDFMemoryStore(
                [
                    new RDFQuadruple(new RDFContext(), new RDFResource("ex:snoopy"),new RDFResource("ex:dogOf"),new RDFResource("ex:linus")),
                    new RDFQuadruple(new RDFContext(), new RDFResource("ex:linus"),new RDFResource("ex:hasName"),new RDFTypedLiteral("Linus", RDFModelEnums.RDFDatatypes.XSD_STRING))
                ]));
            RDFFederation federation = new RDFFederation().AddGraph(graph)
                                                          .AddAsyncGraph(asyncGraph)
                                                          .AddStore(store)
                                                          .AddAsyncStore(asyncStore);

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFResource("ex:balto"))
                .AddDescribeTerm(new RDFResource("ex:snoopy"))
                .AddDescribeTerm(new RDFResource())
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            row0["?N"] = "Mickey Mouse@EN-US";
            row0["?C"] = null;
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            row1["?N"] = "Donald Duck@EN-US";
            row1["?C"] = null;
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = "ex:balto";
            row2["?X"] = "ex:whoever";
            row2["?N"] = null;
            row2["?C"] = null;
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = null;
            row3["?X"] = null;
            row3["?N"] = null;
            row3["?C"] = "green@EN";
            table.Rows.Add(row3);
            DataRow row4 = table.NewRow();
            row4["?Y"] = "ex:snoopy";
            row4["?X"] = "ex:linus";
            row4["?N"] = $"Linus^^{RDFVocabulary.XSD.STRING}";
            row4["?C"] = null;
            table.Rows.Add(row4);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, federation, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 3);
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

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFResource("ex:balto2"))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            row0["?N"] = "Mickey Mouse@EN-US";
            row0["?C"] = null;
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            row1["?N"] = "Donald Duck@EN-US";
            row1["?C"] = null;
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = "ex:balto";
            row2["?X"] = "ex:whoever";
            row2["?N"] = null;
            row2["?C"] = null;
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = null;
            row3["?X"] = null;
            row3["?N"] = null;
            row3["?C"] = "green@EN";
            table.Rows.Add(row3);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, graph, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 0);
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

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFVariable("?Y"))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            row0["?N"] = "Mickey Mouse@EN-US";
            row0["?C"] = null;
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            row1["?N"] = "Donald Duck@EN-US";
            row1["?C"] = null;
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = "ex:balto";
            row2["?X"] = "ex:whoever";
            row2["?N"] = null;
            row2["?C"] = null;
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = null;
            row3["?X"] = null;
            row3["?N"] = null;
            row3["?C"] = "green@EN";
            table.Rows.Add(row3);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, graph, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 4);
        }

        [TestMethod]
        public void ShouldDescribeVariableTermsOnFederation()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino"))
            ]);
            RDFAsyncGraph agraph = new RDFAsyncGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            });
            RDFFederation federation = new RDFFederation().AddGraph(graph).AddAsyncGraph(agraph);

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFVariable("?Y"))
                .AddDescribeTerm(new RDFVariable("?N"))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            row0["?N"] = "Mickey Mouse@EN-US";
            row0["?C"] = null;
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            row1["?N"] = "Donald Duck@EN-US";
            row1["?C"] = null;
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = "ex:balto";
            row2["?X"] = "ex:whoever";
            row2["?N"] = null;
            row2["?C"] = null;
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = null;
            row3["?X"] = null;
            row3["?N"] = null;
            row3["?C"] = "green@EN";
            table.Rows.Add(row3);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, federation, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 6);
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

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFVariable("?Z"))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = "ex:pluto";
            row0["?X"] = "ex:topolino";
            row0["?N"] = "Mickey Mouse@EN-US";
            row0["?C"] = null;
            table.Rows.Add(row0);
            DataRow row1 = table.NewRow();
            row1["?Y"] = "ex:fido";
            row1["?X"] = "ex:paperino";
            row1["?N"] = "Donald Duck@EN-US";
            row1["?C"] = null;
            table.Rows.Add(row1);
            DataRow row2 = table.NewRow();
            row2["?Y"] = "ex:balto";
            row2["?X"] = "ex:whoever";
            row2["?N"] = null;
            row2["?C"] = null;
            table.Rows.Add(row2);
            DataRow row3 = table.NewRow();
            row3["?Y"] = null;
            row3["?X"] = null;
            row3["?N"] = null;
            row3["?C"] = "green@EN";
            table.Rows.Add(row3);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, graph, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldDescribeLiteralBoundVariableTerms()
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

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFVariable("?C"))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = null;
            row0["?X"] = null;
            row0["?N"] = null;
            row0["?C"] = "green@EN";
            table.Rows.Add(row0);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, graph, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 1);
        }

        [TestMethod]
        public void ShouldDescribeLiteralBoundVariableTermsOnAsyncGraph()
        {
            RDFAsyncGraph agraph = new RDFAsyncGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            });

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFVariable("?C"))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = null;
            row0["?X"] = null;
            row0["?N"] = null;
            row0["?C"] = "green@EN";
            table.Rows.Add(row0);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, agraph, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 1);
        }

        [TestMethod]
        public void ShouldDescribeLiteralBoundVariableTermsOnAsyncStore()
        {
            RDFAsyncStore astore = new RDFAsyncStore(new RDFMemoryStore(
            [
                new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            ]));

            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFVariable("?C"))
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFResource("ex:balto"), new RDFResource("ex:hasColor"), new RDFVariable("?C")))));

            DataTable table = new DataTable();
            table.Columns.Add("?Y", typeof(string));
            table.Columns.Add("?X", typeof(string));
            table.Columns.Add("?N", typeof(string));
            table.Columns.Add("?C", typeof(string));
            table.AcceptChanges();
            DataRow row0 = table.NewRow();
            row0["?Y"] = null;
            row0["?X"] = null;
            row0["?N"] = null;
            row0["?C"] = "green@EN";
            table.Rows.Add(row0);

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.DescribeTerms(query, astore, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 4);
            Assert.IsTrue(result.Rows.Count == 1);
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
            DataTable result = queryEngine.ApplyPattern(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
        }

        [TestMethod]
        public void ShouldApplyPatternToDataSourceAsyncGraph()
        {
            RDFAsyncGraph agraph = new RDFAsyncGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.ApplyPattern(pattern, agraph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
            DataTable result = queryEngine.ApplyPattern(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
        }

        [TestMethod]
        public async Task ShouldApplyPatternToDataSourceAsyncStore()
        {
            RDFAsyncStore astore = new RDFAsyncStore();
            await astore.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")));
            await astore.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")));
            await astore.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")));
            await astore.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")));
            await astore.AddQuadrupleAsync(new RDFQuadruple(new RDFContext(), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")));
            RDFPattern pattern = new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"));
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.ApplyPattern(pattern, astore);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
            DataTable result = queryEngine.ApplyPattern(pattern, federation);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?P"].ToString(), "ex:dogOf"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "Mickey Mouse@EN-US"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?V"].ToString(), "ex:dogOf"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?V"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?V"].ToString(), "ex:dogOf"));
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 5); //All the triples...
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
            DataTable result = queryEngine.ApplyPatternToGraph(pattern, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?P"].ToString(), "ex:dogOf"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "Mickey Mouse@EN-US"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?P"].ToString(), "ex:dogOf"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?P"].ToString(), "ex:dogOf"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?P"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 4);
            Assert.IsTrue(result.Rows.Count == 5); //All the quadruples...
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?C"].ToString(), "ex:ctx"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?V"].ToString(), "ex:dogOf"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?V"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?V"].ToString(), "ex:dogOf"));
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 5); //All the quadruples...
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
            DataTable result = queryEngine.ApplyPatternToStore(pattern, store);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 1);
            Assert.IsTrue(result.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
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
            DataTable result = queryEngine.ApplyPatternToFederation(pattern, federation);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
            DataTable result = queryEngine.ApplyPatternToFederation(pattern, federation);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
            DataTable result = queryEngine.ApplyPatternToFederation(pattern, federation);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:whoever"));
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
@"<?xml version=""1.0"" encoding=""utf-8""?>
<sparql xmlns=""http://www.w3.org/2005/sparql-results#"">
  <head>
    <variable name=""?Y"" />
    <variable name=""?X"" />
  </head>
  <results>
    <result>
      <binding name=""?Y"">
        <uri>ex:pluto</uri>
      </binding>
      <binding name=""?X"">
        <uri>ex:topolino</uri>
      </binding>
    </result>
  </results>
</sparql>", encoding: Encoding.UTF8)
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
            DataTable result = queryEngine.ApplyPatternToFederation(pattern, federation);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[2]["?X"].ToString(), "ex:topolino"));
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
                .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:dogOf")))
                .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:hasName")));
            RDFQueryEngine queryEngine = new RDFQueryEngine();
            DataTable result = queryEngine.ApplyPropertyPath(propertyPath, graph);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 2);
            Assert.IsTrue(result.Rows.Count == 2);
            Assert.IsTrue(string.Equals(result.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.Rows[1]["?N"].ToString(), "Donald Duck@EN-US"));
        }

        //MIRELLA TABLES

        [TestMethod]
        public void ShouldCompareDataColumns()
        {
            DataColumn colA = new DataColumn("?A", typeof(string));
            DataColumn colB = new DataColumn("?B", typeof(string));
            Assert.IsTrue(RDFQueryEngine.dtComparer.Equals(colA, colA));
            Assert.IsFalse(RDFQueryEngine.dtComparer.Equals(colA, colB));
            Assert.IsFalse(RDFQueryEngine.dtComparer.Equals(null, colB));
            Assert.IsFalse(RDFQueryEngine.dtComparer.Equals(colA, null));
            Assert.IsTrue(RDFQueryEngine.dtComparer.Equals(null, null));
        }

        [TestMethod]
        public void ShouldAddDataColumn()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, " ?Col ");
            RDFQueryEngine.AddColumn(table, "?COL");

            Assert.IsTrue(table.Columns.Count == 1);
            Assert.IsTrue(string.Equals(table.Columns[0].ColumnName, "?COL"));
            Assert.IsTrue(table.Columns[0].DataType.Equals(RDFQueryEngine.SystemString));
        }

        [TestMethod]
        public void ShouldAddDataRow()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?Y");
            RDFQueryEngine.AddColumn(table, "?X");
            Dictionary<string, string> bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(table, bindings);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?X"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldNotAddDataRowBecauseUnknownBinding()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?Y");
            RDFQueryEngine.AddColumn(table, "?X");
            Dictionary<string, string> bindings = new Dictionary<string, string>()
            {
                { "?Z", "ex:pluto" } //Will not be added to the table, because it is not a column
            };
            RDFQueryEngine.AddRow(table, bindings);

            Assert.IsTrue(table.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldPopulateTableFromGraphWithPatternHoleS()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?S");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
            List<RDFTriple> matchingTriples =
            [
                new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.S, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromGraphWithPatternHoleP()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?P");
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
            List<RDFTriple> matchingTriples =
            [
                new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.P, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromGraphWithPatternHoleO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
            List<RDFTriple> matchingTriples =
            [
                new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.O, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromGraphWithPatternHoleSP()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?P");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
            List<RDFTriple> matchingTriples =
            [
                new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SP, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromGraphWithPatternHoleSO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
            List<RDFTriple> matchingTriples =
            [
                new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromGraphWithPatternHolePO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?P");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFVariable("?O"));
            List<RDFTriple> matchingTriples =
            [
                new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.PO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromGraphWithPatternHoleSPO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?P");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"));
            List<RDFTriple> matchingTriples =
            [
                new RDFTriple(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFQueryEngine.PopulateTable(pattern, matchingTriples, RDFQueryEnums.RDFPatternHoles.SPO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleC()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?C");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.C, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?C"].ToString(), "ex:ctx"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleS()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?S");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.S, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleP()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?P");
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.P, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.O, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleCS()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?C");
            RDFQueryEngine.AddColumn(table, "?S");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.CS, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleCP()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?C");
            RDFQueryEngine.AddColumn(table, "?P");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.CP, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleCO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?C");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.CO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleSP()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?P");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.SP, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleSO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.SO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHolePO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?P");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFVariable("?O"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.PO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleCSP()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?C");
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?P");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), new RDFResource("ex:topolino"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.CSP, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleCSO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?C");
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFResource("ex:dogOf"), new RDFVariable("?O"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.CSO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleCPO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?C");
            RDFQueryEngine.AddColumn(table, "?P");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFResource("ex:pluto"), new RDFVariable("?P"), new RDFVariable("?O"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.CPO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleSPO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?P");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.SPO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldPopulateTableFromStoreWithPatternHoleCSPO()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?C");
            RDFQueryEngine.AddColumn(table, "?S");
            RDFQueryEngine.AddColumn(table, "?P");
            RDFQueryEngine.AddColumn(table, "?O");
            RDFPattern pattern = new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), new RDFVariable("?P"), new RDFVariable("?O"));
            List<RDFQuadruple> matchingQuadruples =
            [
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"), new RDFResource("ex:dogOf"), new RDFResource("ex:topolino"))
            ];
            RDFMemoryStore store = new RDFMemoryStore(matchingQuadruples);
            RDFQueryEngine.PopulateTable(pattern, store, RDFQueryEnums.RDFPatternHoles.CSPO, table);

            Assert.IsTrue(table.Rows.Count == 1);
            Assert.IsTrue(string.Equals(table.Rows[0]["?C"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?S"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?P"].ToString(), "ex:dogOf"));
            Assert.IsTrue(string.Equals(table.Rows[0]["?O"].ToString(), "ex:topolino"));
        }

        [TestMethod]
        public void ShouldProductJoinTables()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?Z");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?Z", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 4);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?Z"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?Z"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
        }

        [TestMethod]
        public void ShouldProductJoinTablesWithEmptyRight()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?Z");
            RDFQueryEngine.AddColumn(dt2, "?N");
            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 4);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?Z"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldProductJoinTablesWithEmptyLeft()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?Z");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?Z", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 4);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?Z"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldProductJoinTablesWithEmptyBoth()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?Z");
            RDFQueryEngine.AddColumn(dt2, "?N");

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 4);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?Z"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldInnerJoinTables()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
        }

        [TestMethod]
        public void ShouldInnerJoinTablesWithUnmatchingKeys()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:minnie" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldInnerJoinTablesWithUnmatchingKeysBecauseNotOptional()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", DBNull.Value.ToString() },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldInnerJoinTablesWithEmptyRight()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldInnerJoinTablesWithEmptyLeft()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:minnie" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldInnerJoinTablesWithEmptyBoth()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");

            DataTable joinTable = RDFQueryEngine.InnerJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldOuterJoinTables()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
        }

        [TestMethod]
        public void ShouldOuterJoinTablesWithLeftOptional()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", null }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
        }

        [TestMethod]
        public void ShouldOuterJoinTablesWithRightOptional()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", null },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
        }

        [TestMethod]
        public void ShouldOuterJoinTablesWithBothOptional()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", null }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", null },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?X"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
        }

        [TestMethod]
        public void ShouldOuterJoinTablesWithUnmatchingKeysButOptionalRight()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            dt2.ExtendedProperties.Add(RDFQueryEngine.IsOptional, true); //Need to simulate Mirella here, since right-optionality will keep at least left row
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?X"].ToString(), "ex:minnie"));
            Assert.IsTrue(string.Equals(joinTable.Rows[0]["?N"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldOuterJoinTablesWithUnmatchingKeys()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldOuterJoinTablesWithEmptyLeft()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldOuterJoinTablesWithEmptyRight()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldOuterJoinTablesWithEmptyBoth()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");

            DataTable joinTable = RDFQueryEngine.OuterJoinTables(dt1, dt2);

            Assert.IsNotNull(joinTable);
            Assert.IsTrue(joinTable.Columns.Count == 3);
            Assert.IsTrue(joinTable.Columns.Contains("?Y"));
            Assert.IsTrue(joinTable.Columns.Contains("?X"));
            Assert.IsTrue(joinTable.Columns.Contains("?N"));
            Assert.IsTrue(joinTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldCombineTablesUnion()
        {
            DataTable dt1 = new DataTable();
            dt1.ExtendedProperties.Add(RDFQueryEngine.JoinAsUnion, true); //Need to simulate Mirella here, since left-union will merge tables
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            dt2.ExtendedProperties.Add(RDFQueryEngine.JoinAsUnion, true); //Need to simulate Mirella here, since left-union will merge tables
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable dt3 = new DataTable();
            RDFQueryEngine.AddColumn(dt3, "?Q");
            Dictionary<string, string> dt3Bindings = new Dictionary<string, string>()
            {
                { "?Q", null }
            };
            RDFQueryEngine.AddRow(dt3, dt3Bindings);

            List<DataTable> tables = [dt1, dt2, dt3];
            DataTable combineTable = RDFQueryEngine.CombineTables(tables, false);

            Assert.IsNotNull(combineTable);
            Assert.IsTrue(combineTable.Columns.Count == 4);
            Assert.IsTrue(combineTable.Columns.Contains("?Y"));
            Assert.IsTrue(combineTable.Columns.Contains("?X"));
            Assert.IsTrue(combineTable.Columns.Contains("?N"));
            Assert.IsTrue(combineTable.Columns.Contains("?Q"));
            Assert.IsTrue(combineTable.Rows.Count == 3);
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Y"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?X"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?N"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?Y"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[2]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(combineTable.Rows[2]["?X"].ToString(), "ex:minnie"));
            Assert.IsTrue(string.Equals(combineTable.Rows[2]["?N"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[2]["?Q"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldCombineTablesUnionWithEmptyTable()
        {
            DataTable dt1 = new DataTable();
            dt1.ExtendedProperties.Add(RDFQueryEngine.JoinAsUnion, true); //Need to simulate Mirella here, since left-union will merge tables
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            dt2.ExtendedProperties.Add(RDFQueryEngine.JoinAsUnion, true); //Need to simulate Mirella here, since left-union will merge tables
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable dt3 = new DataTable();
            RDFQueryEngine.AddColumn(dt3, "?Q");

            List<DataTable> tables = [dt1, dt2, dt3];
            DataTable combineTable = RDFQueryEngine.CombineTables(tables, false);

            Assert.IsNotNull(combineTable);
            Assert.IsTrue(combineTable.Columns.Count == 4);
            Assert.IsTrue(combineTable.Columns.Contains("?Y"));
            Assert.IsTrue(combineTable.Columns.Contains("?X"));
            Assert.IsTrue(combineTable.Columns.Contains("?N"));
            Assert.IsTrue(combineTable.Columns.Contains("?Q"));
            Assert.IsTrue(combineTable.Rows.Count == 2);
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Y"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?X"].ToString(), "ex:minnie"));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?N"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?Q"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldCombineTablesMerge()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable dt3 = new DataTable();
            RDFQueryEngine.AddColumn(dt3, "?Q");
            Dictionary<string, string> dt3Bindings = new Dictionary<string, string>()
            {
                { "?Q", null }
            };
            RDFQueryEngine.AddRow(dt3, dt3Bindings);

            List<DataTable> tables = [dt1, dt2, dt3];
            DataTable combineTable = RDFQueryEngine.CombineTables(tables, true);

            Assert.IsNotNull(combineTable);
            Assert.IsTrue(combineTable.Columns.Count == 4);
            Assert.IsTrue(combineTable.Columns.Contains("?Y"));
            Assert.IsTrue(combineTable.Columns.Contains("?X"));
            Assert.IsTrue(combineTable.Columns.Contains("?N"));
            Assert.IsTrue(combineTable.Columns.Contains("?Q"));
            Assert.IsTrue(combineTable.Rows.Count == 3);
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Y"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?X"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?N"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?Y"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[2]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(combineTable.Rows[2]["?X"].ToString(), "ex:minnie"));
            Assert.IsTrue(string.Equals(combineTable.Rows[2]["?N"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[2]["?Q"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldCombineTablesMergeWithEmptyTable()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable dt3 = new DataTable();
            RDFQueryEngine.AddColumn(dt3, "?Q");

            List<DataTable> tables = [dt1, dt2, dt3];
            DataTable combineTable = RDFQueryEngine.CombineTables(tables, true);

            Assert.IsNotNull(combineTable);
            Assert.IsTrue(combineTable.Columns.Count == 4);
            Assert.IsTrue(combineTable.Columns.Contains("?Y"));
            Assert.IsTrue(combineTable.Columns.Contains("?X"));
            Assert.IsTrue(combineTable.Columns.Contains("?N"));
            Assert.IsTrue(combineTable.Columns.Contains("?Q"));
            Assert.IsTrue(combineTable.Rows.Count == 2);
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Y"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?X"].ToString(), "ex:minnie"));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?N"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(combineTable.Rows[1]["?Q"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldCombineTablesOuterJoin()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            dt2.ExtendedProperties.Add(RDFQueryEngine.IsOptional, true); //Need to simulate Mirella here, since right-optionality  will keep at least left rows
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", null },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable dt3 = new DataTable();
            RDFQueryEngine.AddColumn(dt3, "?Q");
            Dictionary<string, string> dt3Bindings = new Dictionary<string, string>()
            {
                { "?Q", null }
            };
            RDFQueryEngine.AddRow(dt3, dt3Bindings);

            List<DataTable> tables = [dt1, dt2, dt3];
            DataTable combineTable = RDFQueryEngine.CombineTables(tables, false);

            Assert.IsNotNull(combineTable);
            Assert.IsTrue(combineTable.Columns.Count == 4);
            Assert.IsTrue(combineTable.Columns.Contains("?Y"));
            Assert.IsTrue(combineTable.Columns.Contains("?X"));
            Assert.IsTrue(combineTable.Columns.Contains("?N"));
            Assert.IsTrue(combineTable.Columns.Contains("?Q"));
            Assert.IsTrue(combineTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?X"].ToString(), "ex:minnie"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Q"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldCombineTablesInnerJoin()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:minnie" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            List<DataTable> tables = [dt1, dt2];
            DataTable combineTable = RDFQueryEngine.CombineTables(tables, false);

            Assert.IsNotNull(combineTable);
            Assert.IsTrue(combineTable.Columns.Count == 3);
            Assert.IsTrue(combineTable.Columns.Contains("?Y"));
            Assert.IsTrue(combineTable.Columns.Contains("?X"));
            Assert.IsTrue(combineTable.Columns.Contains("?N"));
            Assert.IsTrue(combineTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?X"].ToString(), "ex:minnie"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
        }

        [TestMethod]
        public void ShouldCombineTablesInnerJoinWithNoResults()
        {
            DataTable dt1 = new DataTable();
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:minnie" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            List<DataTable> tables = [dt1, dt2];
            DataTable combineTable = RDFQueryEngine.CombineTables(tables, false);

            Assert.IsNotNull(combineTable);
            Assert.IsTrue(combineTable.Columns.Count == 3);
            Assert.IsTrue(combineTable.Columns.Contains("?Y"));
            Assert.IsTrue(combineTable.Columns.Contains("?X"));
            Assert.IsTrue(combineTable.Columns.Contains("?N"));
            Assert.IsTrue(combineTable.Rows.Count == 0);
        }

        [TestMethod]
        public void ShouldCombineTablesUnionOuterUnion()
        {
            DataTable dt1 = new DataTable();
            dt1.ExtendedProperties.Add(RDFQueryEngine.JoinAsUnion, true); //Need to simulate Mirella here, since left-union will merge tables
            RDFQueryEngine.AddColumn(dt1, "?Y");
            RDFQueryEngine.AddColumn(dt1, "?X");
            Dictionary<string, string> dt1Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:minnie" }
            };
            RDFQueryEngine.AddRow(dt1, dt1Bindings);

            DataTable dt2 = new DataTable();
            RDFQueryEngine.AddColumn(dt2, "?X");
            RDFQueryEngine.AddColumn(dt2, "?N");
            Dictionary<string, string> dt2Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" }
            };
            RDFQueryEngine.AddRow(dt2, dt2Bindings);

            DataTable dt3 = new DataTable();
            dt3.ExtendedProperties.Add(RDFQueryEngine.IsOptional, true); //Need to simulate Mirella here, since right-optionality  will keep at least left rows
            RDFQueryEngine.AddColumn(dt3, "?Y");
            RDFQueryEngine.AddColumn(dt3, "?X");
            RDFQueryEngine.AddColumn(dt3, "?Q");
            Dictionary<string, string> dt3Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", null },
                { "?Q", "Pluto@IT" }
            };
            RDFQueryEngine.AddRow(dt3, dt3Bindings);

            DataTable dt4 = new DataTable();
            dt4.ExtendedProperties.Add(RDFQueryEngine.JoinAsUnion, true); //Need to simulate Mirella here, since left-union will merge tables
            RDFQueryEngine.AddColumn(dt4, "?Y");
            Dictionary<string, string> dt4Bindings = new Dictionary<string, string>()
            {
                { "?Y", "ex:paperino" }
            };
            RDFQueryEngine.AddRow(dt4, dt4Bindings);

            DataTable dt5 = new DataTable();
            RDFQueryEngine.AddColumn(dt5, "?X");
            Dictionary<string, string> dt5Bindings = new Dictionary<string, string>()
            {
                { "?X", "ex:topolino" }
            };
            RDFQueryEngine.AddRow(dt5, dt5Bindings);

            List<DataTable> tables = [dt1, dt2, dt3, dt4, dt5];
            DataTable combineTable = RDFQueryEngine.CombineTables(tables, false);

            Assert.IsNotNull(combineTable);
            Assert.IsTrue(combineTable.Columns.Count == 4);
            Assert.IsTrue(combineTable.Columns.Contains("?Y"));
            Assert.IsTrue(combineTable.Columns.Contains("?X"));
            Assert.IsTrue(combineTable.Columns.Contains("?N"));
            Assert.IsTrue(combineTable.Columns.Contains("?Q"));
            Assert.IsTrue(combineTable.Rows.Count == 1);
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(combineTable.Rows[0]["?Q"].ToString(), "Pluto@IT"));
        }

        [TestMethod]
        public void ShouldProjectStarTable()
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
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
            Assert.IsTrue(result.SelectResults.Columns["?Y"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?N"));
            Assert.IsTrue(result.SelectResults.Columns["?N"].Ordinal == 2);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldProjectNonStarTable()
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
                .AddProjectionVariable(new RDFVariable("?X"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
        }

        [TestMethod]
        public void ShouldProjectNonStarTableWithAllUnexistingVariable()
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
                .AddProjectionVariable(new RDFVariable("?Q"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?Q"));
            Assert.IsTrue(result.SelectResults.Columns["?Q"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Q"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldProjectNonStarTableWithAlsoUnexistingVariable()
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
                .AddProjectionVariable(new RDFVariable("?Y"))
                .AddProjectionVariable(new RDFVariable("?Q"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
            Assert.IsTrue(result.SelectResults.Columns["?Y"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?Q"));
            Assert.IsTrue(result.SelectResults.Columns["?Q"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Q"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Q"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldProjectNonStarTableWithCommonProjectionFromSubQuery()
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
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                    .Optional()
                    .AddProjectionVariable(new RDFVariable("?X")))
                .AddProjectionVariable(new RDFVariable("?Y"))
                .AddProjectionVariable(new RDFVariable("?X"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
            Assert.IsTrue(result.SelectResults.Columns["?Y"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
        }

        [TestMethod]
        public void ShouldProjectNonStarTableWithUncommonProjectionFromSubQuery()
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
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                    .Optional()
                    .AddProjectionVariable(new RDFVariable("?N")))
                .AddModifier(new RDFDistinctModifier())
                .AddProjectionVariable(new RDFVariable("?Y"))
                .AddProjectionVariable(new RDFVariable("?X"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
            Assert.IsTrue(result.SelectResults.Columns["?Y"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
        }

        [TestMethod]
        public void ShouldProjectStarTableWithProjectionFromSubQuery()
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
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N"))))
                    .Optional())
                .AddProjectionVariable(new RDFVariable("?N"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?N"));
            Assert.IsTrue(result.SelectResults.Columns["?N"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?N"].ToString(), "Mickey Mouse@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?N"].ToString(), "Donald Duck@EN-US"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?N"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldProjectExpressions()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?Y");
            RDFQueryEngine.AddColumn(table, "?X");
            RDFQueryEngine.AddColumn(table, "?N");
            RDFQueryEngine.AddColumn(table, "?A");
            Dictionary<string, string> tableBindings1 = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" },
                { "?A", $"85^^{RDFVocabulary.XSD.INTEGER}" },
            };
            RDFQueryEngine.AddRow(table, tableBindings1);
            Dictionary<string, string> tableBindings2 = new Dictionary<string, string>()
            {
                { "?Y", "ex:fido" },
                { "?X", "ex:paperino" },
                { "?N", "Donald Duck@EN-US" },
                { "?A", $"83^^{RDFVocabulary.XSD.INTEGER}" },
            };
            RDFQueryEngine.AddRow(table, tableBindings2);
            Dictionary<string, string> tableBindings3 = new Dictionary<string, string>()
            {
                { "?Y", "ex:balto" },
                { "?X", "ex:whoever" },
                { "?N", null },
                { "?A", null },
            };
            RDFQueryEngine.AddRow(table, tableBindings3);

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?A"))
                .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)));

            RDFQueryEngine.ProjectExpressions(query, table);

            Assert.IsNotNull(table);
            Assert.IsTrue(table.Columns.Count == 5);
            Assert.IsTrue(table.Columns.Contains("?AGEX2"));
            Assert.IsTrue(table.Rows.Count == 3);
            Assert.IsTrue(string.Equals(table.Rows[0]["?AGEX2"].ToString(), $"170^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(string.Equals(table.Rows[1]["?AGEX2"].ToString(), $"166^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(string.Equals(table.Rows[2]["?AGEX2"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldProjectExpressionsWithUnexistingVariable()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?Y");
            RDFQueryEngine.AddColumn(table, "?X");
            RDFQueryEngine.AddColumn(table, "?N");
            RDFQueryEngine.AddColumn(table, "?A");
            Dictionary<string, string> tableBindings1 = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" },
                { "?A", $"85^^{RDFVocabulary.XSD.INTEGER}" },
            };
            RDFQueryEngine.AddRow(table, tableBindings1);
            Dictionary<string, string> tableBindings2 = new Dictionary<string, string>()
            {
                { "?Y", "ex:fido" },
                { "?X", "ex:paperino" },
                { "?N", "Donald Duck@EN-US" },
                { "?A", $"83^^{RDFVocabulary.XSD.INTEGER}" },
            };
            RDFQueryEngine.AddRow(table, tableBindings2);
            Dictionary<string, string> tableBindings3 = new Dictionary<string, string>()
            {
                { "?Y", "ex:balto" },
                { "?X", "ex:whoever" },
                { "?N", null },
                { "?A", null },
            };
            RDFQueryEngine.AddRow(table, tableBindings3);

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?Q"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)));

            RDFQueryEngine.ProjectExpressions(query, table);

            Assert.IsNotNull(table);
            Assert.IsTrue(table.Columns.Count == 5);
            Assert.IsTrue(table.Columns.Contains("?AGEX2"));
            Assert.IsTrue(table.Rows.Count == 3);
            Assert.IsTrue(string.Equals(table.Rows[0]["?AGEX2"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(table.Rows[1]["?AGEX2"].ToString(), string.Empty));
            Assert.IsTrue(string.Equals(table.Rows[2]["?AGEX2"].ToString(), string.Empty));
        }

        [TestMethod]
        public void ShouldProjectExpressionsWithoutHavingExpressions()
        {
            DataTable table = new DataTable();
            RDFQueryEngine.AddColumn(table, "?Y");
            RDFQueryEngine.AddColumn(table, "?X");
            RDFQueryEngine.AddColumn(table, "?N");
            RDFQueryEngine.AddColumn(table, "?A");
            Dictionary<string, string> tableBindings1 = new Dictionary<string, string>()
            {
                { "?Y", "ex:pluto" },
                { "?X", "ex:topolino" },
                { "?N", "Mickey Mouse@EN-US" },
                { "?A", $"85^^{RDFVocabulary.XSD.INTEGER}" },
            };
            RDFQueryEngine.AddRow(table, tableBindings1);
            Dictionary<string, string> tableBindings2 = new Dictionary<string, string>()
            {
                { "?Y", "ex:fido" },
                { "?X", "ex:paperino" },
                { "?N", "Donald Duck@EN-US" },
                { "?A", $"83^^{RDFVocabulary.XSD.INTEGER}" },
            };
            RDFQueryEngine.AddRow(table, tableBindings2);
            Dictionary<string, string> tableBindings3 = new Dictionary<string, string>()
            {
                { "?Y", "ex:balto" },
                { "?X", "ex:whoever" },
                { "?N", null },
                { "?A", null },
            };
            RDFQueryEngine.AddRow(table, tableBindings3);

            RDFSelectQuery query = new RDFSelectQuery()
                .AddProjectionVariable(new RDFVariable("?A"));

            RDFQueryEngine.ProjectExpressions(query, table);

            Assert.IsNotNull(table);
            Assert.IsTrue(table.Columns.Count == 4);
            Assert.IsTrue(table.Rows.Count == 3);
        }

        [TestMethod]
        public void ShouldProjectTableAlsoWithExpressions()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            ]);

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A")).Optional()))
                .AddProjectionVariable(new RDFVariable("?X"))
                .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX2"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "170^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX2"].ToString(), "166^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX2"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldProjectTableAlsoWithSubsequentExpressions()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            ]);

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A")).Optional()))
                .AddProjectionVariable(new RDFVariable("?X"))
                .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)))
                .AddProjectionVariable(new RDFVariable("?AGEX4"), new RDFMultiplyExpression(new RDFVariable("?AGEX2"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)))
                .AddProjectionVariable(new RDFVariable("?AGEX4PLUS1"), new RDFVariableExpression(new RDFAddExpression(new RDFVariable("?AGEX4"), RDFTypedLiteral.One)));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 4);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX2"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX4"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX4"].Ordinal == 2);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX4PLUS1"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX4PLUS1"].Ordinal == 3);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "170^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX4"].ToString(), "340^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX4PLUS1"].ToString(), "341^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX2"].ToString(), "166^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX4"].ToString(), "332^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX4PLUS1"].ToString(), "333^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX2"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX4"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX4PLUS1"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldProjectTableAlsoWithSubsequentExpressionsWorkingOnValues()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddValues(new RDFValues().AddColumn(new RDFVariable("?X"), [new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INTEGER)])))
                .AddProjectionVariable(new RDFVariable("?X"))
                .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?X"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)))
                .AddProjectionVariable(new RDFVariable("?AGEX4"), new RDFMultiplyExpression(new RDFVariable("?AGEX2"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, new RDFGraph());

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX2"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX4"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX4"].Ordinal == 2);
            Assert.IsTrue(result.SelectResults.Rows.Count == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "2^^http://www.w3.org/2001/XMLSchema#integer"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "4^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX4"].ToString(), "8^^http://www.w3.org/2001/XMLSchema#double"));
        }

        [TestMethod]
        public void ShouldProjectTableAlsoWithExpressionsFromSubQuery()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            ]);

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A"))))
                    .Optional()
                    .AddProjectionVariable(new RDFVariable("?X"))
                    .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT))))
                .AddProjectionVariable(new RDFVariable("?Y"))
                .AddProjectionVariable(new RDFVariable("?AGEX2"))
                .AddProjectionVariable(new RDFVariable("?X"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 3);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
            Assert.IsTrue(result.SelectResults.Columns["?Y"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX2"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 2);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "170^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX2"].ToString(), "166^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX2"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
        }

        [TestMethod]
        public void ShouldProjectTableAlsoWithExpressionsAndExpressionsFromSubQuery()
        {
            RDFGraph graph = new RDFGraph(
            [
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            ]);

            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A"))))
                    .Optional()
                    .AddProjectionVariable(new RDFVariable("?X"))
                    .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT))))
                .AddProjectionVariable(new RDFVariable("?Y"))
                .AddProjectionVariable(new RDFVariable("?AGEX2"))
                .AddProjectionVariable(new RDFVariable("?AGEX4"), new RDFMultiplyExpression(new RDFVariable("?AGEX2"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_INT)))
                .AddProjectionVariable(new RDFVariable("?X"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 4);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?Y"));
            Assert.IsTrue(result.SelectResults.Columns["?Y"].Ordinal == 0);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX2"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX2"].Ordinal == 1);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?AGEX4"));
            Assert.IsTrue(result.SelectResults.Columns["?AGEX4"].Ordinal == 2);
            Assert.IsTrue(result.SelectResults.Columns.Contains("?X"));
            Assert.IsTrue(result.SelectResults.Columns["?X"].Ordinal == 3);
            Assert.IsTrue(result.SelectResults.Rows.Count == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?Y"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX2"].ToString(), "170^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?AGEX4"].ToString(), "340^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?X"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?Y"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX2"].ToString(), "166^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?AGEX4"].ToString(), "332^^http://www.w3.org/2001/XMLSchema#double"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?X"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?Y"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX2"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?AGEX4"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?X"].ToString(), "ex:whoever"));
        }
        #endregion
    }
}