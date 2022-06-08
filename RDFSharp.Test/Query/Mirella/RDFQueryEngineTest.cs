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
        public void ShouldEvaluateSelectQueryWithResults()
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
        public void ShouldEvaluateSelectQueryWithNoResults()
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
        public void ShouldEvaluateSelectQueryWithNoResultsBecauseNoEvaluableQueryMembers()
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
        #endregion
    }
}