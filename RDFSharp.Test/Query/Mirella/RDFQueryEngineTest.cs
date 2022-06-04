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
            Assert.IsNotNull(queryEngine.QueryMemberTemporaryResultTables);
            Assert.IsNotNull(queryEngine.QueryMemberFinalResultTables);
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
                .AddPatternGroup(new RDFPatternGroup("PG1")
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
        #endregion
    }
}