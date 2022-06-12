/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Text.RegularExpressions;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFQueryEngineTest
    {
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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

        [TestMethod]
        public void ShouldEvaluateSelectQueryOnGraphWithResultsHavingCommonSPVariables()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:pluto")),
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:dogOf")),
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:pluto")),
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
        public void ShouldEvaluateSelectQueryOnGraphWithNoResults()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:ctx"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:ctx")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:dogOf"),new RDFResource("ex:ctx")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:ctx"),new RDFResource("ex:ctx")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:ctx"),new RDFResource("ex:ctx")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:ctx"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:pluto")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:dogOf")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:pluto"),new RDFResource("ex:pluto")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:pluto"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Pluto", "it")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Fido", "it")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
        public void ShouldEvaluateSelectQueryOnFederationWithResults()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            });
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {   
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
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
        public void ShouldEvaluateDescribeQueryWithResults()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
        public void ShouldEvaluateDescribeQueryWithResultsFromResourceTerm()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
        public void ShouldEvaluateDescribeQueryWithResultsFromResourceTermOnly()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
            });
            
            RDFDescribeQuery query = new RDFDescribeQuery()
                .AddDescribeTerm(new RDFVariable("?Y"));
            RDFDescribeQueryResult result = new RDFQueryEngine().EvaluateDescribeQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DescribeResults);
            Assert.IsTrue(result.DescribeResults.Columns.Count == 3);
            Assert.IsTrue(result.DescribeResultsCount == 0);
        }

        [TestMethod]
        public void ShouldEvaluateConstructQueryWithResults()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
            });
            
            RDFConstructQuery query = new RDFConstructQuery()
                .AddTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
            RDFConstructQueryResult result = new RDFQueryEngine().EvaluateConstructQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ConstructResults);
            Assert.IsTrue(result.ConstructResults.Columns.Count == 3);
            Assert.IsTrue(result.ConstructResultsCount == 0);
        }

        [TestMethod]
        public void ShouldEvaluateAskQueryTrue()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US"))
            });
            
            RDFAskQuery query = new RDFAskQuery();
            RDFAskQueryResult result = new RDFQueryEngine().EvaluateAskQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.AskResult);
        }

        [TestMethod]
        public void ShouldEvaluateQueryMembersWithResults()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .AddFilter(new RDFBoundFilter(new RDFVariable("?N"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), new List<RDFPatternMember>() { new RDFResource("ex:pluto") }))))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluateQueryMembers(query, evaluableQueryMembers, graph);

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .AddFilter(new RDFBoundFilter(new RDFVariable("?N")))
                    .Optional())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), new List<RDFPatternMember>() { new RDFResource("ex:pluto") })))
                    .UnionWithNext())
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluateQueryMembers(query, evaluableQueryMembers, graph);

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .AddFilter(new RDFBoundFilter(new RDFVariable("?N")))
                    .UnionWithNext())
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), new List<RDFPatternMember>() { new RDFResource("ex:pluto") })))
                    .Optional())
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluateQueryMembers(query, evaluableQueryMembers, graph);

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf2"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                    .AddFilter(new RDFBoundFilter(new RDFVariable("?N"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), new List<RDFPatternMember>() { new RDFResource("ex:pluto") }))))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?Y"), RDFQueryEnums.RDFOrderByFlavors.ASC));
            List<RDFQueryMember> evaluableQueryMembers = query.GetEvaluableQueryMembers().ToList();

            RDFQueryEngine queryEngine = new RDFQueryEngine();
            queryEngine.EvaluateQueryMembers(query, evaluableQueryMembers, graph);

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

            RDFPatternGroup patternGroup = new RDFPatternGroup()
                .AddValues(new RDFValues().AddColumn(new RDFVariable("?Y"), new List<RDFPatternMember>() { new RDFResource("ex:pluto") }));
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:baobao"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperoga")),
                new RDFTriple(new RDFResource("ex:paperoga"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Paperoga", "it-IT")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });

            RDFPatternGroup patternGroup = new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
                .AddFilter(new RDFRegexFilter(new RDFVariable("?Y"), new Regex("^ex:[a-zA-Z]+o$")))
                .AddFilter(new RDFInFilter(new RDFVariable("?Y"), new List<RDFPatternMember>() { new RDFResource("ex:pluto") }));
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()))
                .AddModifier(new RDFGroupByModifier(new List<RDFVariable>() { new RDFVariable("?Y") })
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
            List<RDFPattern> templates = new List<RDFPattern>()
            {
                new RDFPattern(new RDFResource("ex:bracco"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")),
                new RDFPattern(new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog"))
            };
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
            List<RDFPattern> templates = new List<RDFPattern>()
            {
                new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:bracco"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")),
                new RDFPattern(new RDFVariable("?Y"), new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog"))
            };
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            });
            
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
        public void ShouldDescribeUnexistingResourceTerms()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            });
            
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
        public void ShouldDescribeUnexistingVariableTerms()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:hasColor"),new RDFPlainLiteral("green", "en"))
            });
            
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
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
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
            DataTable result = queryEngine.DescribeTerms(query, graph, table);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Columns.Count == 3);
            Assert.IsTrue(result.Rows.Count == 1);
        }
        #endregion
    }
}