/*
   Copyright 2012-2025 Marco De Salvo

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
using System.Net;
using System.Threading.Tasks;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFDeleteWhereOperationTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    #region Tests
    [TestMethod]
    public void ShouldCreateDeleteWhereOperation()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();

        Assert.IsNotNull(operation);
        Assert.IsNotNull(operation.DeleteTemplates);
        Assert.IsEmpty(operation.DeleteTemplates);
        Assert.IsNotNull(operation.InsertTemplates);
        Assert.IsEmpty(operation.InsertTemplates);
        Assert.IsNotNull(operation.Variables);
        Assert.IsEmpty(operation.Variables);
        Assert.IsEmpty(operation.Prefixes);
        Assert.IsEmpty(operation.QueryMembers);

        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            DELETE {
            }
            WHERE {
            }
            """));
    }

    [TestMethod]
    public void ShouldAddDeleteTemplate()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

        Assert.IsNotNull(operation);
        Assert.IsNotNull(operation.DeleteTemplates);
        Assert.HasCount(1, operation.DeleteTemplates);
        Assert.IsNotNull(operation.InsertTemplates);
        Assert.IsEmpty(operation.InsertTemplates);
        Assert.IsNotNull(operation.Variables);
        Assert.IsEmpty(operation.Variables);
        Assert.IsEmpty(operation.Prefixes);
        Assert.IsEmpty(operation.QueryMembers);

        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            DELETE {
              <ex:subj> <ex:pred> <ex:obj> .
            }
            WHERE {
            }
            """));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingDeleteTemplateBecauseNullTemplate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFDeleteWhereOperation().AddDeleteTemplate(null));

    [TestMethod]
    public void ShouldAddPrefix()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
        operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf")); //Will be discarded, since duplicate prefixes are not allowed
        operation.AddPrefix(new RDFNamespace("rdf", $"{RDFVocabulary.RDF.BASE_URI}")); //Will be discarded, since duplicate prefixes are not allowed
        operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"));

        Assert.IsEmpty(operation.InsertTemplates);
        Assert.IsEmpty(operation.DeleteTemplates);
        Assert.IsEmpty(operation.Variables);
        Assert.HasCount(2, operation.Prefixes);
        Assert.IsEmpty(operation.QueryMembers);

        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

            DELETE {
            }
            WHERE {
            }
            """));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingPrefixBecauseNullPrefix()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFDeleteWhereOperation().AddPrefix(null));

    [TestMethod]
    public void ShouldAddModifier()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddModifier<RDFOperation>(new RDFDistinctModifier());
        operation.AddModifier<RDFOperation>(new RDFDistinctModifier()); //Will be discarded, since duplicate modifiers are not allowed

        Assert.IsEmpty(operation.InsertTemplates);
        Assert.IsEmpty(operation.DeleteTemplates);
        Assert.IsEmpty(operation.Variables);
        Assert.IsEmpty(operation.Prefixes);
        Assert.HasCount(1, operation.QueryMembers);

        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            DELETE {
            }
            WHERE {
            }
            """));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingModifierBecauseNullModifier()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFDeleteWhereOperation().AddModifier(null));

    [TestMethod]
    public void ShouldAddPatternGroup()
    {
        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddPatternGroup(patternGroup);
        operation.AddPatternGroup(patternGroup); //Will be discarded, since duplicate patternGroups are not allowed

        Assert.IsEmpty(operation.InsertTemplates);
        Assert.IsEmpty(operation.DeleteTemplates);
        Assert.IsEmpty(operation.Variables);
        Assert.IsEmpty(operation.Prefixes);
        Assert.HasCount(1, operation.QueryMembers);

        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            DELETE {
            }
            WHERE {
              {
                ?Y <ex:dogOf> ?X .
              }
            }
            """));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingPatternGroupBecauseNullPatternGroup()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFDeleteWhereOperation().AddPatternGroup(null));

    [TestMethod]
    public void ShouldAddSubQuery()
    {
        RDFSelectQuery subQuery = new RDFSelectQuery();
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddSubQuery<RDFOperation>(subQuery);
        operation.AddSubQuery<RDFOperation>(subQuery); //Will be discarded, since duplicate sub queries are not allowed

        Assert.IsEmpty(operation.InsertTemplates);
        Assert.IsEmpty(operation.DeleteTemplates);
        Assert.IsEmpty(operation.Variables);
        Assert.IsEmpty(operation.Prefixes);
        Assert.HasCount(1, operation.QueryMembers);

        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            DELETE {
            }
            WHERE {
              {
                SELECT *
                WHERE {
                }
              }
            }
            """));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingSubQueryBecauseNullSubQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFDeleteWhereOperation().AddSubQuery(null));

    [TestMethod]
    public void ShouldApplyToNullGraph()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToGraph(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
    }

    [TestMethod]
    public void ShouldApplyToGraph()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(3, result.DeleteResults.Columns.Count);
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(3, result.DeleteResultsCount);
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?OBJECT"].ToString(), "ex:whoever"));
        Assert.AreEqual(2, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToGraphWithGroundTemplate()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(3, result.DeleteResults.Columns.Count);
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.DeleteResultsCount);
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
        Assert.AreEqual(4, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToGraphWithVariableFromBind()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?XBIND")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?X")), new RDFVariable("?XBIND"))));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(3, result.DeleteResults.Columns.Count);
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(3, result.DeleteResultsCount);
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?OBJECT"].ToString(), "ex:whoever"));
        Assert.AreEqual(2, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToGraphWithVariableFromBindHavingErrors()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?XBIND")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional())
            .AddBind(new RDFBind(new RDFFloorExpression(new RDFVariable("?X")), new RDFVariable("?XBIND"))));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(3, result.DeleteResults.Columns.Count);
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(5, graph.TriplesCount);
    }

    [TestMethod]
    public async Task ShouldApplyToNullGraphAsync()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = await operation.ApplyToGraphAsync(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
    }

    [TestMethod]
    public async Task ShouldApplyToGraphAsync()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = await operation.ApplyToGraphAsync(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(3, result.DeleteResults.Columns.Count);
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(3, result.DeleteResultsCount);
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?OBJECT"].ToString(), "ex:whoever"));
        Assert.AreEqual(2, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToNullStore()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToStore(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
    }

    [TestMethod]
    public void ShouldApplyToStore()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(4, result.DeleteResults.Columns.Count);
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(3, result.DeleteResultsCount);
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?OBJECT"].ToString(), "ex:whoever"));
        Assert.AreEqual(2, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldApplyToStoreWithGroundTemplate()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(4, result.DeleteResults.Columns.Count);
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.DeleteResultsCount);
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
        Assert.AreEqual(4, store.QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldApplyToNullStoreAsync()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = await operation.ApplyToStoreAsync(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
    }

    [TestMethod]
    public async Task ShouldApplyToStoreAsync()
    {
        RDFMemoryStore store = new RDFMemoryStore(
        [
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = await operation.ApplyToStoreAsync(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.AreEqual(0, result.InsertResultsCount);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(4, result.DeleteResults.Columns.Count);
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.DeleteResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(3, result.DeleteResultsCount);
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[0]["?OBJECT"].ToString(), "ex:topolino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[1]["?OBJECT"].ToString(), "ex:paperino"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?CONTEXT"].ToString(), "ex:ctx"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?PREDICATE"].ToString(), "ex:dogOf"));
        Assert.IsTrue(string.Equals(result.DeleteResults.Rows[2]["?OBJECT"].ToString(), "ex:whoever"));
        Assert.AreEqual(2, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldApplyToNullSPARQLUpdateEndpoint()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpoint()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpoint"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpoint"));

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithParams()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams")
                    .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                    .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams"));
        endpoint.AddDefaultGraphUri("ex:ctx1");
        endpoint.AddNamedGraphUri("ex:ctx2");

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(250));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"));

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentType()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType")
                    .WithBody(new RegexMatcher("update=.*")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(250));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType"));

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams")
                    .WithBody(new RegexMatcher("using-named-graph-uri=ex%3actx2&using-graph-uri=ex%3actx1&update=.*")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(250));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams"));
        endpoint.AddDefaultGraphUri("ex:ctx1");
        endpoint.AddNamedGraphUri("ex:ctx2");

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(750));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"));

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

        Assert.ThrowsExactly<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(250)));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.InternalServerError));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"));

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

        Assert.ThrowsExactly<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint));
    }

    [TestMethod]
    public async Task ShouldApplyToNullSPARQLUpdateEndpointAsync()
    {
        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ShouldApplyToSPARQLUpdateEndpointAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"));

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ShouldApplyToSPARQLUpdateEndpointWithParamsAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParamsAsync")
                    .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                    .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFDeleteWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParamsAsync"));
        endpoint.AddDefaultGraphUri("ex:ctx1");
        endpoint.AddNamedGraphUri("ex:ctx2");

        RDFDeleteWhereOperation operation = new RDFDeleteWhereOperation();
        operation.AddDeleteTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

        Assert.IsTrue(result);
    }
    #endregion
}