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
        RDFGraph graph;

        #region Init
        [TestInitialize]
        public void Initialize()
        {
            graph = new RDFGraph(new List<RDFTriple>()
            {
                //gender
                new RDFTriple(new RDFResource("ex:mark"),RDFVocabulary.FOAF.GENDER,new RDFResource("ex:male")),
                new RDFTriple(new RDFResource("ex:bob"),RDFVocabulary.FOAF.GENDER,new RDFResource("ex:male")),
                new RDFTriple(new RDFResource("ex:john"),RDFVocabulary.FOAF.GENDER,new RDFResource("ex:male")),
                new RDFTriple(new RDFResource("ex:steve"),RDFVocabulary.FOAF.GENDER,new RDFResource("ex:male")),
                new RDFTriple(new RDFResource("ex:valentine"),RDFVocabulary.FOAF.GENDER,new RDFResource("ex:female")),
                new RDFTriple(new RDFResource("ex:elsa"),RDFVocabulary.FOAF.GENDER,new RDFResource("ex:female")),
                new RDFTriple(new RDFResource("ex:jenny"),RDFVocabulary.FOAF.GENDER,new RDFResource("ex:female")),
                //name
                new RDFTriple(new RDFResource("ex:mark"),RDFVocabulary.FOAF.NAME,new RDFPlainLiteral("mark","en")),
                //bob has no name (useful for optionality)
                new RDFTriple(new RDFResource("ex:john"),RDFVocabulary.FOAF.NAME,new RDFPlainLiteral("john")),
                new RDFTriple(new RDFResource("ex:steve"),RDFVocabulary.FOAF.NAME,new RDFPlainLiteral("steve","en")),
                new RDFTriple(new RDFResource("ex:valentine"),RDFVocabulary.FOAF.NAME,new RDFPlainLiteral("valentine","en")),
                new RDFTriple(new RDFResource("ex:elsa"),RDFVocabulary.FOAF.NAME,new RDFPlainLiteral("elsa","en")),
                new RDFTriple(new RDFResource("ex:elsa"),RDFVocabulary.FOAF.NAME,new RDFPlainLiteral("elsa","es")),
                new RDFTriple(new RDFResource("ex:jenny"),RDFVocabulary.FOAF.NAME,new RDFPlainLiteral("jenny")),
                //age
                new RDFTriple(new RDFResource("ex:mark"),RDFVocabulary.FOAF.AGE,new RDFTypedLiteral("39", RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)),
                new RDFTriple(new RDFResource("ex:bob"),RDFVocabulary.FOAF.AGE,new RDFTypedLiteral("43", RDFModelEnums.RDFDatatypes.XSD_INTEGER)),
                new RDFTriple(new RDFResource("ex:john"),RDFVocabulary.FOAF.AGE,new RDFTypedLiteral("37", RDFModelEnums.RDFDatatypes.XSD_DECIMAL)),
                new RDFTriple(new RDFResource("ex:steve"),RDFVocabulary.FOAF.AGE,new RDFTypedLiteral("31", RDFModelEnums.RDFDatatypes.XSD_FLOAT)),
                new RDFTriple(new RDFResource("ex:valentine"),RDFVocabulary.FOAF.AGE,new RDFTypedLiteral("36", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)),
                new RDFTriple(new RDFResource("ex:elsa"),RDFVocabulary.FOAF.AGE,new RDFTypedLiteral("29.0", RDFModelEnums.RDFDatatypes.XSD_DOUBLE)),
                new RDFTriple(new RDFResource("ex:jenny"),RDFVocabulary.FOAF.AGE,new RDFTypedLiteral("22", RDFModelEnums.RDFDatatypes.XSD_INT)),
                //knows
                new RDFTriple(new RDFResource("ex:mark"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:john")),
                new RDFTriple(new RDFResource("ex:john"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:mark")),
                new RDFTriple(new RDFResource("ex:mark"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:steve")),
                new RDFTriple(new RDFResource("ex:steve"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:mark")),
                new RDFTriple(new RDFResource("ex:mark"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:valentine")),
                new RDFTriple(new RDFResource("ex:valentine"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:mark")),
                new RDFTriple(new RDFResource("ex:mark"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:elsa")),
                new RDFTriple(new RDFResource("ex:elsa"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:mark")),
                new RDFTriple(new RDFResource("ex:john"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:steve")),
                new RDFTriple(new RDFResource("ex:steve"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:john")),
                new RDFTriple(new RDFResource("ex:valentine"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:elsa")),
                new RDFTriple(new RDFResource("ex:elsa"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:valentine")),
                new RDFTriple(new RDFResource("ex:valentine"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:jenny")),
                new RDFTriple(new RDFResource("ex:jenny"),RDFVocabulary.FOAF.KNOWS,new RDFResource("ex:valentine"))
            });
        }
        #endregion

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
        public void ShouldEvaluateSelectQuery_StarMalesWithName()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME"))));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NAME"].ToString(), "mark@EN"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:john"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NAME"].ToString(), "john"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?MALE"].ToString(), "ex:steve"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NAME"].ToString(), "steve@EN"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_StarMalesWithOptionalName()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 4);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?NAME"].ToString(), "mark@EN"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:bob"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?NAME"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?MALE"].ToString(), "ex:john"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?NAME"].ToString(), "john"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?MALE"].ToString(), "ex:steve"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?NAME"].ToString(), "steve@EN"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithName()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME"))))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 3);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:john"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?MALE"].ToString(), "ex:steve"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalName()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 4);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:bob"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?MALE"].ToString(), "ex:john"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?MALE"].ToString(), "ex:steve"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalNameAndUnboundVariable()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()))
                .AddProjectionVariable(new RDFVariable("?MALE"))
                .AddProjectionVariable(new RDFVariable("?UNBOUND"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 4);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?UNBOUND"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:bob"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?UNBOUND"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?MALE"].ToString(), "ex:john"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[2]["?UNBOUND"].ToString(), DBNull.Value.ToString()));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?MALE"].ToString(), "ex:steve"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[3]["?UNBOUND"].ToString(), DBNull.Value.ToString()));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalNameAndLimit()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()))
                .AddModifier(new RDFLimitModifier(2))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 2);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:bob"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalNameAndLimitZero()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()))
                .AddModifier(new RDFLimitModifier(0))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 0);
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalNameAndLimitOffset()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()))
                .AddModifier(new RDFLimitModifier(2))
                .AddModifier(new RDFOffsetModifier(1))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 2);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:bob"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:john"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalNameAndLimitOffsetExceeding()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()))
                .AddModifier(new RDFLimitModifier(2))
                .AddModifier(new RDFOffsetModifier(8))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 0);
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalNameAndLimitOffsetOrder()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()))
                .AddModifier(new RDFLimitModifier(2))
                .AddModifier(new RDFOffsetModifier(1))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?MALE"), RDFQueryEnums.RDFOrderByFlavors.ASC))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 2);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:john"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:mark"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalNameAndLimitOffsetOrderOnUnboundVariable()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional()))
                .AddModifier(new RDFLimitModifier(2))
                .AddModifier(new RDFOffsetModifier(1))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?UNBOUND"), RDFQueryEnums.RDFOrderByFlavors.ASC))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 2);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:bob"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?MALE"].ToString(), "ex:john"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjMalesWithOptionalNameAndFilter()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.GENDER, new RDFResource("ex:male")))
                    .AddPattern(new RDFPattern(new RDFVariable("?MALE"), RDFVocabulary.FOAF.NAME, new RDFVariable("?NAME")).Optional())
                    .AddFilter(new RDFRegexFilter(new RDFVariable("?MALE"), new Regex("^ex:m"))))
                .AddProjectionVariable(new RDFVariable("?MALE"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 1);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?MALE"].ToString(), "ex:mark"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_StarKnowersWithCountGreater2()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?KNOWER"), RDFVocabulary.FOAF.KNOWS, new RDFVariable("?KNOWN"))))
                .AddModifier(new RDFGroupByModifier(new List<RDFVariable>() { new RDFVariable("?KNOWER") })
                    .AddAggregator(new RDFCountAggregator(new RDFVariable("?KNOWN"), new RDFVariable("?COUNT_KNOWN"))
                        .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_BYTE))))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?COUNT_KNOWN"), RDFQueryEnums.RDFOrderByFlavors.DESC))
                .AddProjectionVariable(new RDFVariable("?KNOWN")); //Will not be projected, since GroupBy overrides projector
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 2);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?KNOWER"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?COUNT_KNOWN"].ToString(), $"4^^{RDFVocabulary.XSD.DECIMAL}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?KNOWER"].ToString(), "ex:valentine"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?COUNT_KNOWN"].ToString(), $"3^^{RDFVocabulary.XSD.DECIMAL}"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_ProjKnowersWithCountGreater2()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?KNOWER"), RDFVocabulary.FOAF.KNOWS, new RDFVariable("?KNOWN"))))
                .AddModifier(new RDFGroupByModifier(new List<RDFVariable>() { new RDFVariable("?KNOWER") })
                    .AddAggregator(new RDFCountAggregator(new RDFVariable("?KNOWN"), new RDFVariable("?COUNT_KNOWN"))
                        .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_BYTE))))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?COUNT_KNOWN"), RDFQueryEnums.RDFOrderByFlavors.DESC))
                .AddProjectionVariable(new RDFVariable("?KNOWER"))
                .AddProjectionVariable(new RDFVariable("?COUNT_KNOWN"));
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 2);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?KNOWER"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?COUNT_KNOWN"].ToString(), $"4^^{RDFVocabulary.XSD.DECIMAL}"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?KNOWER"].ToString(), "ex:valentine"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[1]["?COUNT_KNOWN"].ToString(), $"3^^{RDFVocabulary.XSD.DECIMAL}"));
        }

        [TestMethod]
        public void ShouldEvaluateSelectQuery_StarKnowersWithCountGreater2RestrictedByValuesFromSubQuery()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?KNOWER"), RDFVocabulary.FOAF.KNOWS, new RDFVariable("?KNOWN"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup("PG2")
                        .AddValues(new RDFValues()
                            .AddColumn(new RDFVariable("?KNOWER"), new List<RDFPatternMember>() { new RDFResource("ex:mark") }))))
                .AddModifier(new RDFGroupByModifier(new List<RDFVariable>() { new RDFVariable("?KNOWER") })
                    .AddAggregator(new RDFCountAggregator(new RDFVariable("?KNOWN"), new RDFVariable("?COUNT_KNOWN"))
                        .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterThan, new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_BYTE))))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?COUNT_KNOWN"), RDFQueryEnums.RDFOrderByFlavors.DESC))
                .AddProjectionVariable(new RDFVariable("?KNOWN")); //Will not be projected, since GroupBy overrides projector
            RDFSelectQueryResult result = new RDFQueryEngine().EvaluateSelectQuery(query, graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SelectResults);
            Assert.IsTrue(result.SelectResults.Columns.Count == 2);
            Assert.IsTrue(result.SelectResultsCount == 1);
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?KNOWER"].ToString(), "ex:mark"));
            Assert.IsTrue(string.Equals(result.SelectResults.Rows[0]["?COUNT_KNOWN"].ToString(), $"4^^{RDFVocabulary.XSD.DECIMAL}"));
        }
        #endregion
    }
}