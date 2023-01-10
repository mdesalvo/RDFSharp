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
using System.Net;
using System.Threading.Tasks;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFInsertWhereOperationTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        [TestCleanup]
        public void Cleanup()  { server.Stop(); server.Dispose(); }

        #region Tests
        [TestMethod]
        public void ShouldCreateInsertWhereOperation()
        {
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.DeleteTemplates);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsNotNull(operation.InsertTemplates);
            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsNotNull(operation.Variables);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 0);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"INSERT {
}
WHERE {
}"));
        }

        [TestMethod]
        public void ShouldAddInsertTemplate()
        {
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.DeleteTemplates);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsNotNull(operation.InsertTemplates);
            Assert.IsTrue(operation.InsertTemplates.Count == 1);
            Assert.IsNotNull(operation.Variables);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 0);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"INSERT {
  <ex:subj> <ex:pred> <ex:obj> .
}
WHERE {
}"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingInsertTemplateBecauseNullTemplate()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFInsertWhereOperation().AddInsertTemplate(null));

        [TestMethod]
        public void ShouldAddPrefix()
        {
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"));
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf")); //Will be discarded, since duplicate prefixes are not allowed
            operation.AddPrefix(new RDFNamespace("rdf", $"{RDFVocabulary.RDF.BASE_URI}")); //Will be discarded, since duplicate prefixes are not allowed
            operation.AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"));

            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 2);
            Assert.IsTrue(operation.QueryMembers.Count == 0);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

INSERT {
}
WHERE {
}"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingPrefixBecauseNullPrefix()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFInsertWhereOperation().AddPrefix(null));

        [TestMethod]
        public void ShouldAddModifier()
        {
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddModifier<RDFOperation>(new RDFDistinctModifier());
            operation.AddModifier<RDFOperation>(new RDFDistinctModifier()); //Will be discarded, since duplicate modifiers are not allowed

            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 1);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"INSERT {
}
WHERE {
}"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingModifierBecauseNullModifier()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFInsertWhereOperation().AddModifier(null));

        [TestMethod]
        public void ShouldAddPatternGroup()
        {
            RDFPatternGroup patternGroup = new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X")));
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddPatternGroup(patternGroup);
            operation.AddPatternGroup(patternGroup); //Will be discarded, since duplicate patternGroups are not allowed

            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 1);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"INSERT {
}
WHERE {
  {
    ?Y <ex:dogOf> ?X .
  }
}"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingPatternGroupBecauseNullPatternGroup()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFInsertWhereOperation().AddPatternGroup(null));

        [TestMethod]
        public void ShouldAddSubQuery()
        {
            RDFSelectQuery subQuery = new RDFSelectQuery();
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddSubQuery<RDFOperation>(subQuery);
            operation.AddSubQuery<RDFOperation>(subQuery); //Will be discarded, since duplicate sub queries are not allowed

            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 1);

            string operationString = operation.ToString();

            Assert.IsTrue(string.Equals(operationString,
@"INSERT {
}
WHERE {
  {
    SELECT *
    WHERE {
    }
  }
}"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingSubQueryBecauseNullSubQuery()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFInsertWhereOperation().AddSubQuery(null));

        [TestMethod]
        public void ShouldPrintComplexOperation()
        {
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                .AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"), RDFVocabulary.RDF.TYPE, new RDFResource("ex:dog")).Optional())
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:dogOf"), new RDFVariable("?X"))
                        .UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X")))
                    .UnionWithNext())
                .AddPatternGroup(new RDFPatternGroup()
                    .AddPattern(new RDFPattern(new RDFVariable("?Y"), new RDFResource("ex:isDogOf"), new RDFVariable("?X"))))
                .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup()
                        .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?Y"), new RDFVariable("?X"))
                            .AddAlternativeSteps(new List<RDFPropertyPathStep>() { 
                                new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                                new RDFPropertyPathStep(RDFVocabulary.RDFS.COMMENT)})))
                    .AddProjectionVariable(new RDFVariable("?Y"))
                    .UnionWithNext())
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
@"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

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
}"));
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
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsNotNull(result.InsertResultsCount == 0);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToGraph()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
            operation.AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
            RDFOperationResult result = operation.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 3);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsNotNull(result.InsertResultsCount == 3);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsTrue(graph.TriplesCount == 8);
        }

        [TestMethod]
        public void ShouldApplyToGraphWithExpressionsFromSubQuery()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("85.00", RDFModelEnums.RDFDatatypes.XSD_FLOAT)),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasAge"),new RDFTypedLiteral("83", RDFModelEnums.RDFDatatypes.XSD_BYTE)),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
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
            Assert.IsTrue(result.InsertResults.Columns.Count == 3);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsNotNull(result.InsertResultsCount == 2);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:topolino"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.FOAF.AGE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), $"170^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:paperino"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.FOAF.AGE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), $"166^^{RDFVocabulary.XSD.DOUBLE}"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsTrue(graph.TriplesCount == 9);
        }

        [TestMethod]
        public void ShouldApplyToGraphWithGroundTemplate()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFResource("ex:doggy"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
            operation.AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
            RDFOperationResult result = operation.ApplyToGraph(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 3);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.InsertResultsCount == 1);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:doggy"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsTrue(graph.TriplesCount == 6);
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
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToGraphAsync()
        {
            RDFGraph graph = new RDFGraph(new List<RDFTriple>()
            {
                new RDFTriple(new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFTriple(new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFTriple(new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFTriple(new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFTriple(new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
            operation.AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
            RDFOperationResult result = await operation.ApplyToGraphAsync(graph);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 3);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.InsertResultsCount == 3);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsTrue(graph.TriplesCount == 8);
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
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public void ShouldApplyToStore()
        {
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
            operation.AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
            RDFOperationResult result = operation.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 4);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.InsertResultsCount == 3);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsTrue(store.QuadruplesCount == 8);
        }

        [TestMethod]
        public void ShouldApplyToStoreWithGroundTemplate()
        {
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:doggy"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
            operation.AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
            RDFOperationResult result = operation.ApplyToStore(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 4);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.InsertResultsCount == 1);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:doggy"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsTrue(store.QuadruplesCount == 6);
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
            Assert.IsTrue(result.InsertResults.Columns.Count == 0);
            Assert.IsTrue(result.InsertResultsCount == 0);
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
        }

        [TestMethod]
        public async Task ShouldApplyToStoreAsync()
        {
            RDFMemoryStore store = new RDFMemoryStore(new List<RDFQuadruple>()
            {
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:pluto"),new RDFResource("ex:dogOf"),new RDFResource("ex:topolino")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:topolino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Mickey Mouse", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:fido"),new RDFResource("ex:dogOf"),new RDFResource("ex:paperino")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:paperino"),new RDFResource("ex:hasName"),new RDFPlainLiteral("Donald Duck", "en-US")),
                new RDFQuadruple(new RDFContext("ex:ctx"),new RDFResource("ex:balto"),new RDFResource("ex:dogOf"),new RDFResource("ex:whoever"))
            });
            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),RDFVocabulary.RDF.TYPE,new RDFResource("ex:dog")));
            operation.AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?Y"),new RDFResource("ex:dogOf"),new RDFVariable("?X")))
                .AddPattern(new RDFPattern(new RDFContext("ex:ctx"),new RDFVariable("?X"),new RDFResource("ex:hasName"),new RDFVariable("?N")).Optional()));
            RDFOperationResult result = await operation.ApplyToStoreAsync(store);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.InsertResults);
            Assert.IsTrue(result.InsertResults.Columns.Count == 4);
            Assert.IsTrue(result.InsertResults.Columns.Contains("?CONTEXT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?SUBJECT"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?PREDICATE"));
            Assert.IsTrue(result.InsertResults.Columns.Contains("?OBJECT"));
            Assert.IsTrue(result.InsertResultsCount == 3);
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?SUBJECT"].ToString(), "ex:pluto"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[0]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?SUBJECT"].ToString(), "ex:fido"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[1]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?CONTEXT"].ToString(), "ex:ctx"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?SUBJECT"].ToString(), "ex:balto"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?PREDICATE"].ToString(), $"{RDFVocabulary.RDF.TYPE}"));
            Assert.IsTrue(string.Equals(result.InsertResults.Rows[2]["?OBJECT"].ToString(), "ex:dog"));
            Assert.IsNotNull(result.DeleteResults);
            Assert.IsTrue(result.DeleteResults.Columns.Count == 0);
            Assert.IsTrue(result.DeleteResultsCount == 0);
            Assert.IsTrue(store.QuadruplesCount == 8);
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
                            .WithDelay(250));

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
                            .WithDelay(250));

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
                            .WithDelay(250));

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
                            .WithDelay(750));

            RDFSPARQLEndpoint endpoint = new RDFSPARQLEndpoint(new Uri(server.Url + "/RDFInsertWhereOperationTest/ShouldThrowExceptionWhenApplyingToSPARQLUpdateEndpointAccordingToTimeoutBehavior"));

            RDFInsertWhereOperation operation = new RDFInsertWhereOperation();
            operation.AddInsertTemplate(new RDFPattern(new RDFContext("ex:ctx"),new RDFResource("ex:subj"),new RDFResource("ex:pred"),new RDFResource("ex:obj")));

            Assert.ThrowsException<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint, new RDFSPARQLEndpointOperationOptions(250)));
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

            Assert.ThrowsException<RDFQueryException>(() => operation.ApplyToSPARQLUpdateEndpoint(endpoint));
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
}