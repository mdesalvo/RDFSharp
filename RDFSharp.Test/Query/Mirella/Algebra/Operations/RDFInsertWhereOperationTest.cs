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
public class RDFInsertWhereOperationTest
{
    private WireMockServer server;

    [TestInitialize]
    public void Initialize() { server = WireMockServer.Start(); }

    #region Tests
    [TestMethod]
    public void ShouldCreateInsertWhereOperation()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();

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
            INSERT {
            }
            WHERE {
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldAddInsertTemplate()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

        Assert.IsNotNull(operation);
        Assert.IsNotNull(operation.DeleteTemplates);
        Assert.IsEmpty(operation.DeleteTemplates);
        Assert.IsNotNull(operation.InsertTemplates);
        Assert.HasCount(1, operation.InsertTemplates);
        Assert.IsNotNull(operation.Variables);
        Assert.IsEmpty(operation.Variables);
        Assert.IsEmpty(operation.Prefixes);
        Assert.IsEmpty(operation.QueryMembers);

        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            INSERT {
              <ex:subj> <ex:pred> <ex:obj> .
            }
            WHERE {
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingInsertTemplateBecauseNullTemplate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFInsertWhereOperation().AddInsertTemplate(null));

    [TestMethod]
    public void ShouldAddPrefix()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
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

            INSERT {
            }
            WHERE {
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingPrefixBecauseNullPrefix()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFInsertWhereOperation().AddPrefix(null));

    [TestMethod]
    public void ShouldAddModifier()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
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
            INSERT {
            }
            WHERE {
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingModifierBecauseNullModifier()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFInsertWhereOperation().AddModifier(null));

    [TestMethod]
    public void ShouldAddPatternGroup()
    {
        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
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
            INSERT {
            }
            WHERE {
              {
                ?Y <ex:dogOf> ?X .
              }
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldAddPatternGroupWithBind()
    {
        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND")));
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
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
            INSERT {
            }
            WHERE {
              {
                ?Y <ex:dogOf> ?X .
                BIND(?Y AS ?YBIND) .
              }
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldAddPatternGroupWithBindAfterUnion()
    {
        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).Union())
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND")));
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
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
            INSERT {
            }
            WHERE {
              {
                ?Y <ex:dogOf> ?X .
                BIND(?Y AS ?YBIND) .
              }
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldAddPatternGroupWithBindAfterUnionAndThenPattern()
    {
        RDFPatternGroup patternGroup = new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")).Union())
            .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND")))
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X")).Union());
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
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
            INSERT {
            }
            WHERE {
              {
                ?Y <ex:dogOf> ?X .
                BIND(?Y AS ?YBIND) .
                ?Y <ex:isDogOf> ?X .
              }
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingPatternGroupBecauseNullPatternGroup()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFInsertWhereOperation().AddPatternGroup(null));

    [TestMethod]
    public void ShouldAddSubQuery()
    {
        RDFSelectQuery subQuery = new RDFSelectQuery();
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
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
            INSERT {
            }
            WHERE {
              {
                SELECT *
                WHERE {
                }
              }
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnAddingSubQueryBecauseNullSubQuery()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFInsertWhereOperation().AddSubQuery(null));

    [TestMethod]
    public void ShouldPrintComplexOperation()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
            .AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")).Optional())
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))
                    .Union())
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X")))
                .Union())
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?X"))
                        .AddAlternativeSteps([
                            new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                            new RDFPropertyPathStep(RDFVocabulary.RDFS.COMMENT)])))
                .AddProjectionVariable(new RDFVariable("?Y"))
                .Union())
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasDog"), new RDFVariable("?Y"))))
                .AddProjectionVariable(new RDFVariable("?Y"))
                .AddProjectionVariable(new RDFVariable("?X"))
                .AddProjectionVariable(new RDFVariable("?XADD1"), new RDFAddExpression(new RDFVariable("?X"),new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INT)))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.DESC)))
            .AddModifier(new RDFDistinctModifier());
        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

            INSERT {
              ?Y rdf:type <ex:dog> .
            }
            WHERE {
              {
                {
                  { ?Y <ex:dogOf> ?X }
                  UNION
                  { ?Y <ex:isDogOf> ?X }
                }
                UNION
                {
                  ?Y <ex:isDogOf> ?X .
                }
              }
              {
                {
                  SELECT ?Y
                  WHERE {
                    {
                      ?Y (<http://www.w3.org/2000/01/rdf-schema#label>|<http://www.w3.org/2000/01/rdf-schema#comment>) ?X .
                    }
                  }
                }
                UNION
                {
                  SELECT ?Y ?X ((?X + 1) AS ?XADD1)
                  WHERE {
                    {
                      ?X <ex:hasDog> ?Y .
                    }
                  }
                  ORDER BY DESC(?X)
                }
              }
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPrintComplexOperationWithBindAndExpressions()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
            .AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")).Optional())
            .AddPatternGroup(new RDFPatternGroup()
                .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND3")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))
                    .Union())
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X")))
                .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDoggyOf"), new RDFVariable("?X")))
                .Union())
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X")))
                .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND2")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))
                    .Union())
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDoggyOf"), new RDFVariable("?X"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?X"))
                        .AddAlternativeSteps([
                            new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                            new RDFPropertyPathStep(RDFVocabulary.RDFS.COMMENT)])))
                .AddProjectionVariable(new RDFVariable("?Y"))
                .Union())
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasDog"), new RDFVariable("?Y"))))
                .AddProjectionVariable(new RDFVariable("?Y"))
                .AddProjectionVariable(new RDFVariable("?X"))
                .AddProjectionVariable(new RDFVariable("?XADD1"), new RDFAddExpression(new RDFVariable("?X"), new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INT)))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.DESC)))
            .AddModifier(new RDFDistinctModifier());
        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

            INSERT {
              ?Y rdf:type <ex:dog> .
            }
            WHERE {
              {
                {
                  BIND(?Y AS ?YBIND3) .
                  { ?Y <ex:dogOf> ?X }
                  UNION
                  { ?Y <ex:isDogOf> ?X }
                  BIND(?Y AS ?YBIND) .
                  ?Y <ex:isDoggyOf> ?X .
                }
                UNION
                {
                  ?Y <ex:isDogOf> ?X .
                  BIND(?Y AS ?YBIND2) .
                  { ?Y <ex:dogOf> ?X }
                  UNION
                  { ?Y <ex:isDoggyOf> ?X }
                }
              }
              {
                {
                  SELECT ?Y
                  WHERE {
                    {
                      ?Y (<http://www.w3.org/2000/01/rdf-schema#label>|<http://www.w3.org/2000/01/rdf-schema#comment>) ?X .
                    }
                  }
                }
                UNION
                {
                  SELECT ?Y ?X ((?X + 1) AS ?XADD1)
                  WHERE {
                    {
                      ?X <ex:hasDog> ?Y .
                    }
                  }
                  ORDER BY DESC(?X)
                }
              }
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldPrintComplexOperationWithBindAndExpressionsAndLastQueryMemberUnions()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation()
            .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
            .AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")).Optional())
            .AddPatternGroup(new RDFPatternGroup()
                .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND3")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))
                    .Union())
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X")))
                .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDoggyOf"), new RDFVariable("?X")))
                .Union())
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X")))
                .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?Y")), new RDFVariable("?YBIND2")))
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))
                    .Union())
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDoggyOf"), new RDFVariable("?X"))))
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?X"))
                        .AddAlternativeSteps([
                            new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                            new RDFPropertyPathStep(RDFVocabulary.RDFS.COMMENT)]))
                    .Union())
                .AddProjectionVariable(new RDFVariable("?Y"))
                .Union())
            .AddSubQuery(new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasDog"), new RDFVariable("?Y"))))
                .Union()
                .AddProjectionVariable(new RDFVariable("?Y"))
                .AddProjectionVariable(new RDFVariable("?X"))
                .AddProjectionVariable(new RDFVariable("?XADD1"), new RDFAddExpression(new RDFVariable("?X"), new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INT)))
                .AddModifier(new RDFOrderByModifier(new RDFVariable("?X"), RDFQueryEnums.RDFOrderByFlavors.DESC)))
            .AddModifier(new RDFDistinctModifier());
        string operationString = operation.ToString();

        Assert.IsTrue(string.Equals(operationString,
            """
            PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

            INSERT {
              ?Y rdf:type <ex:dog> .
            }
            WHERE {
              {
                {
                  BIND(?Y AS ?YBIND3) .
                  { ?Y <ex:dogOf> ?X }
                  UNION
                  { ?Y <ex:isDogOf> ?X }
                  BIND(?Y AS ?YBIND) .
                  ?Y <ex:isDoggyOf> ?X .
                }
                UNION
                {
                  ?Y <ex:isDogOf> ?X .
                  BIND(?Y AS ?YBIND2) .
                  { ?Y <ex:dogOf> ?X }
                  UNION
                  { ?Y <ex:isDoggyOf> ?X }
                }
              }
              {
                {
                  SELECT ?Y
                  WHERE {
                    {
                      ?Y (<http://www.w3.org/2000/01/rdf-schema#label>|<http://www.w3.org/2000/01/rdf-schema#comment>) ?X .
                    }
                  }
                }
                UNION
                {
                  SELECT ?Y ?X ((?X + 1) AS ?XADD1)
                  WHERE {
                    {
                      ?X <ex:hasDog> ?Y .
                    }
                  }
                  ORDER BY DESC(?X)
                }
              }
            }
            """, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldApplyToNullGraph()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToGraph(null);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(0, result.InsertResults.Columns.Count);
        Assert.IsNotNull(result.InsertResultsCount == 0);
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
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(3, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.IsNotNull(result.InsertResultsCount == 3);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(8, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToGraphWithExpressionsFromSubQuery()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85.00", RDFModelEnums.RDFDatatypes.XSD_FLOAT)),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83", RDFModelEnums.RDFDatatypes.XSD_BYTE)),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?X"), RDFVocabulary.FOAF.AGE, new RDFVariable("?AGEX2")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        operation.AddSubQuery(new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A"))))
            .Optional()
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(3, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.IsNotNull(result.InsertResultsCount == 2);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.FOAF.AGE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), $"170^^{RDFVocabulary.XSD.DOUBLE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.FOAF.AGE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), $"166^^{RDFVocabulary.XSD.DOUBLE}", StringComparison.Ordinal));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(9, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToGraphWithExpressionsAndBindFromSubQuery()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85.00", RDFModelEnums.RDFDatatypes.XSD_FLOAT)),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83", RDFModelEnums.RDFDatatypes.XSD_BYTE)),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?X"), RDFVocabulary.FOAF.AGE, new RDFVariable("?AGEX2")));
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?XAA"), new RDFResource("ex:derivedFrom"), new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        operation.AddSubQuery(new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A")))
                .AddBind(new RDFBind(new RDFConcatExpression(new RDFVariableExpression(new RDFVariable("?X")), new RDFConstantExpression(new RDFPlainLiteral("AA"))), new RDFVariable("?XAA"))))
            .Optional()
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XAA"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?A"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(3, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.IsNotNull(result.InsertResultsCount == 4);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.FOAF.AGE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), $"170^^{RDFVocabulary.XSD.DOUBLE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.FOAF.AGE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), $"166^^{RDFVocabulary.XSD.DOUBLE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:topolinoAA", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), "ex:derivedFrom", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:topolino", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[3]["?SUBJECT"].ToString(), "ex:paperinoAA", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[3]["?PREDICATE"].ToString(), "ex:derivedFrom", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[3]["?OBJECT"].ToString(), "ex:paperino", StringComparison.Ordinal));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(11, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToGraphWithExpressionsAndBindFromSubQueryHavingErrors()
    {
        RDFGraph graph = new RDFGraph(
        [
            new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
            new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85.00", RDFModelEnums.RDFDatatypes.XSD_FLOAT)),
            new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
            new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83", RDFModelEnums.RDFDatatypes.XSD_BYTE)),
            new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
        ]);
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?X"), RDFVocabulary.FOAF.AGE, new RDFVariable("?AGEX2")));
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?XFLOOR"), new RDFResource("ex:derivedFrom"), new RDFVariable("?X")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasName"), new RDFVariable("?N")).Optional()));
        operation.AddSubQuery(new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?X"), new RDFResource("ex:hasAge"), new RDFVariable("?A")))
                .AddBind(new RDFBind(new RDFFloorExpression(new RDFVariable("?X")), new RDFVariable("?XFLOOR"))))
            .Optional()
            .AddProjectionVariable(new RDFVariable("?X"))
            .AddProjectionVariable(new RDFVariable("?XFLOOR"))
            .AddProjectionVariable(new RDFVariable("?AGEX2"), new RDFMultiplyExpression(new RDFVariable("?X"), new RDFTypedLiteral("2", RDFModelEnums.RDFDatatypes.XSD_DECIMAL))));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(3, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.IsNotNull(result.InsertResultsCount == 0);
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(7, graph.TriplesCount);
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
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:doggy"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToGraph(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(3, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:doggy", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(6, graph.TriplesCount);
    }

    [TestMethod]
    public async Task ShouldApplyToNullGraphAsync()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
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
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = await operation.ApplyToGraphAsync(graph);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(3, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(3, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(8, graph.TriplesCount);
    }

    [TestMethod]
    public void ShouldApplyToNullStore()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
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
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(4, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(3, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?CONTEXT"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(8, store.QuadruplesCount);
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
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:doggy"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = operation.ApplyToStore(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(4, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(1, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:doggy", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(6, store.QuadruplesCount);
    }

    [TestMethod]
    public async Task ShouldApplyToNullStoreAsync()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
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
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
        operation.AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
            .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
        RDFOperationResult result = await operation.ApplyToStoreAsync(store);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.InsertResults);
        Assert.AreEqual(4, result.InsertResults.Columns.Count);
        Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
        Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
        Assert.AreEqual(3, result.InsertResultsCount);
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?CONTEXT"].ToString(), "ex:ctx", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}", StringComparison.Ordinal));
        Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:dog", StringComparison.Ordinal));
        Assert.IsNotNull(result.DeleteResults);
        Assert.AreEqual(0, result.DeleteResults.Columns.Count);
        Assert.AreEqual(0, result.DeleteResultsCount);
        Assert.AreEqual(8, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldApplyToNullSPARQLUpdateEndpoint()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpoint()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpoint"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpoint"));

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithParams()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams")
                    .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                    .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParams"));
        endpoint.AddDefaultGraphUri("ex:ctx1");
        endpoint.AddNamedGraphUri("ex:ctx2");

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(100));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithTimeoutMilliseconds"));

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentType()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType")
                    .WithBody(new RegexMatcher("update=.*")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(100));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentType"));

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams")
                    .WithBody(new RegexMatcher("using-named-graph-uri=ex%3actx2&using-graph-uri=ex%3actx1&update=.*")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(100));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithRequestContentTypeAndParams"));
        endpoint.AddDefaultGraphUri("ex:ctx1");
        endpoint.AddNamedGraphUri("ex:ctx2");

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(1000, RDFQueryEnums.RDFSPARQLEndpointOperationContentTypes.X_WWW_FormUrlencoded));

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithDelay(300));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"));

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

        Assert.ThrowsExactly<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(250)));
    }

    [TestMethod]
    public void ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.InternalServerError));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpoint"));

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

        Assert.ThrowsExactly<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint));
    }

    [TestMethod]
    public async Task ShouldApplyToNullSPARQLUpdateEndpointAsync()
    {
        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(null);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ShouldApplyToSPARQLUpdateEndpointAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointAsync"));

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ShouldApplyToSPARQLUpdateEndpointWithParamsAsync()
    {
        server
            .Given(
                Request.Create()
                    .WithPath("/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParamsAsync")
                    .WithParam("using-graph-uri", new ExactMatcher("ex:ctx1"))
                    .WithParam("using-named-graph-uri", new ExactMatcher("ex:ctx2")))
            .RespondWith(
                Response.Create()
                    .WithStatusCode(HttpStatusCode.OK));

        RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldApplyToSPARQLUpdateEndpointWithParamsAsync"));
        endpoint.AddDefaultGraphUri("ex:ctx1");
        endpoint.AddNamedGraphUri("ex:ctx2");

        RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
        operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));
        bool result = await operation.ApplyToSPARQLUpdateEndpointAsync(endpoint);

        Assert.IsTrue(result);
    }
    #endregion
}