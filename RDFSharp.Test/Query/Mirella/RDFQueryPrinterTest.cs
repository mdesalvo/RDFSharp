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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFQueryPrinterTest
{
  #region Tests
  //SELECT
  [TestMethod]
  public void ShouldPrintSelectQueryNull()
    => Assert.IsTrue(string.IsNullOrEmpty(RDFQueryPrinter.PrintSelectQuery(null, 0, false)));

  [TestMethod]
  public void ShouldPrintSelectQueryEmpty()
  {
    RDFSelectQuery query = new RDFSelectQuery();
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarPrefixed()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarUnprefixed()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithOptionalPattern()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          OPTIONAL { ?S rdfs:label "label"@EN } .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithOptionalPatternAndOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          {
            OPTIONAL { ?S rdfs:label "label"@EN } .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleUnionPattern()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext()));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleUnionPatternFollowedByPropertyPath()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
        .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?S"), new RDFVariable("?E")).AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          { ?S rdfs:label "label"@EN }
          UNION
          { ?S rdfs:label ?E }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleUnionPatternFollowedByValues()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
        .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [RDFVocabulary.RDFS.LABEL])));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          { ?S rdfs:label "label"@EN }
          UNION
          { VALUES ?S { rdfs:label } }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleUnionPatternFollowedByBind()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).UnionWithNext())
        .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?EXP")), new RDFVariable("?V"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
          BIND(?EXP AS ?V) .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleUnionPatternFollowedByBind()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en-US")).UnionWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(RDFVocabulary.RDFS.CLASS), new RDFVariable("?V"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          { ?S1 rdfs:label "label"@EN }
          UNION
          { ?S2 rdfs:label "label"@EN-US }
          BIND(rdfs:Class AS ?V) .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleUnionPatternsHavingMiddleBind()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).UnionWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(RDFVocabulary.RDFS.CLASS), new RDFVariable("?V")))
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en-US")).UnionWithNext()));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S1 rdfs:label "label"@EN .
          BIND(rdfs:Class AS ?V) .
          ?S2 rdfs:label "label"@EN-US .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleOptionalPatternsHavingMiddleBind()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).Optional())
        .AddBind(new RDFBind(new RDFConstantExpression(RDFVocabulary.RDFS.CLASS), new RDFVariable("?V")))
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en-US")).Optional()));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          OPTIONAL { ?S1 rdfs:label "label"@EN } .
          BIND(rdfs:Class AS ?V) .
          OPTIONAL { ?S2 rdfs:label "label"@EN-US } .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleUnionPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleUnionPatternAndSingleUnionPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleMinusPattern()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext()));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleMinusPatternFollowedByPropertyPath()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?S"), new RDFVariable("?E")).AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          { ?S rdfs:label "label"@EN }
          MINUS
          { ?S rdfs:label ?E }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleMinusPatternFollowedByValues()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [RDFVocabulary.RDFS.LABEL])));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          { ?S rdfs:label "label"@EN }
          MINUS
          { VALUES ?S { rdfs:label } }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleMinusPatternFollowedByBind()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?EXP")), new RDFVariable("?V"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
          BIND(?EXP AS ?V) .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleMinusPatternFollowedByBind()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en-US")).MinusWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(RDFVocabulary.RDFS.CLASS), new RDFVariable("?V"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          { ?S1 rdfs:label "label"@EN }
          MINUS
          { ?S2 rdfs:label "label"@EN-US }
          BIND(rdfs:Class AS ?V) .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleMinusPatternFollowedByValues()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en-US")).MinusWithNext())
        .AddValues(new RDFValues().AddColumn(new RDFVariable("?V"), [ new RDFResource("ex:val") ])));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          { ?S1 rdfs:label "label"@EN }
          MINUS
          { ?S2 rdfs:label "label"@EN-US }
          MINUS
          { VALUES ?V { <ex:val> } }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleMinusPatternsHavingMiddleBind()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(RDFVocabulary.RDFS.CLASS), new RDFVariable("?V")))
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en-US")).MinusWithNext()));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S1 rdfs:label "label"@EN .
          BIND(rdfs:Class AS ?V) .
          ?S2 rdfs:label "label"@EN-US .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleMinusPatternsHavingMiddleValues()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .AddValues(new RDFValues().AddColumn(new RDFVariable("?V"), [ new RDFResource("ex:val") ]))
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en-US")).MinusWithNext()));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          { ?S1 rdfs:label "label"@EN }
          MINUS
          { VALUES ?V { <ex:val> } }
          ?S2 rdfs:label "label"@EN-US .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleMinusPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .MinusWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithSingleMinusPatternAndSingleMinusPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .MinusWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithServiceSilentPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org")), new RDFSPARQLEndpointQueryOptions { ErrorBehavior= RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult }));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE SILENT <ex:org> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithOptionalServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          SERVICE <ex:org> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithOptionalServiceSilentPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org")), new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult })
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          SERVICE SILENT <ex:org> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org")))
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionServiceSilentPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org")), new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult })
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE SILENT <ex:org> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleUnionServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          UNION
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionLeftServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS)));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          UNION
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionRightServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionMixedServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS)))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4")))
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          UNION
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
        {
          {
            SERVICE <ex:org3> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          UNION
          {
            SERVICE <ex:org4> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionMixed2ServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4")))
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
        OPTIONAL {
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
        {
          {
            SERVICE <ex:org3> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          UNION
          {
            SERVICE <ex:org4> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionMixed3ServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
        {
          {
            ?S rdfs:comment rdfs:Class .
          }
          UNION
          {
            ?S rdfs:label "label"@EN .
          }
        }
        OPTIONAL {
          SERVICE <ex:org4> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionMixed4ServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
        {
          {
            ?S rdfs:comment rdfs:Class .
          }
          UNION
          {
            ?S rdfs:label "label"@EN .
          }
        }
        SERVICE <ex:org4> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionMixed5ServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .Optional()
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("ex:org")))
        .Optional()) //Should be discarded since running under UnionWithNext semantic from the previous
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          SERVICE <ex:org1> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
        {
          {
            ?S rdfs:comment rdfs:Class .
          }
          UNION
          {
            ?S rdfs:label <ex:org> .
          }
        }
        OPTIONAL {
          SERVICE <ex:org4> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org")))
        .MinusWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusServiceSilentPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org")), new RDFSPARQLEndpointQueryOptions { ErrorBehavior = RDFQueryEnums.RDFSPARQLEndpointQueryErrorBehaviors.GiveEmptyResult })
        .MinusWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE SILENT <ex:org> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleMinusServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          MINUS
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusLeftServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS)));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          MINUS
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusRightServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          MINUS
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusMixedServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS)))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4")))
        .MinusWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          MINUS
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
        {
          {
            SERVICE <ex:org3> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          MINUS
          {
            SERVICE <ex:org4> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusMixed2ServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4")))
        .MinusWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
        OPTIONAL {
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
        {
          {
            SERVICE <ex:org3> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          MINUS
          {
            SERVICE <ex:org4> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusMixed3ServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
        {
          {
            ?S rdfs:comment rdfs:Class .
          }
          MINUS
          {
            ?S rdfs:label "label"@EN .
          }
        }
        OPTIONAL {
          SERVICE <ex:org4> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusMixed4ServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
        {
          {
            ?S rdfs:comment rdfs:Class .
          }
          MINUS
          {
            ?S rdfs:label "label"@EN .
          }
        }
        SERVICE <ex:org4> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMinusMixed5ServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .Optional()
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("ex:org")))
        .Optional()) //Should be discarded since running under MinusWithNext semantic from the previous
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org4")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          SERVICE <ex:org1> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
        {
          {
            ?S rdfs:comment rdfs:Class .
          }
          MINUS
          {
            ?S rdfs:label <ex:org> .
          }
        }
        OPTIONAL {
          SERVICE <ex:org4> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        SERVICE <ex:org2> {
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithPatternGroupFollowedByMultipleServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label <http://example.org/res> .
        }
        SERVICE <ex:org1> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
        SERVICE <ex:org2> {
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionServicePatternGroupFollowedByServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label <http://example.org/res> .
              }
            }
          }
          UNION
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
        SERVICE <ex:org3> {
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithServicePatternGroupFollowedByUnionServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label <http://example.org/res> .
          }
        }
        {
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          UNION
          {
            SERVICE <ex:org3> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithServicePatternGroupFollowedByMinusServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label <http://example.org/res> .
          }
        }
        {
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          MINUS
          {
            SERVICE <ex:org3> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithUnionServicePatternGroupFollowedByOptionalServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label <http://example.org/res> .
              }
            }
          }
          UNION
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
        OPTIONAL {
          SERVICE <ex:org3> {
            {
              ?S rdfs:comment rdfs:Class .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithServicePatternGroupFollowedByOptionalServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2")))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label <http://example.org/res> .
          }
        }
        OPTIONAL {
          SERVICE <ex:org2> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
        OPTIONAL {
          SERVICE <ex:org3> {
            {
              ?S rdfs:comment rdfs:Class .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithOptionalServicePatternGroupFollowedByOptionalServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2")))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          SERVICE <ex:org1> {
            {
              ?S rdfs:label <http://example.org/res> .
            }
          }
        }
        OPTIONAL {
          SERVICE <ex:org2> {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
        OPTIONAL {
          SERVICE <ex:org3> {
            {
              ?S rdfs:comment rdfs:Class .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithOptionalServicePatternGroupFollowedByUnionServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .Optional()); //this Optional will be discarded, since we are under UnionWithNext from previous
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          SERVICE <ex:org1> {
            {
              ?S rdfs:label <http://example.org/res> .
            }
          }
        }
        {
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          UNION
          {
            SERVICE <ex:org3> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithOptionalServicePatternGroupFollowedByMinusServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3")))
        .Optional()); //this Optional will be discarded, since we are under MinusWithNext from previous
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
          SERVICE <ex:org1> {
            {
              ?S rdfs:label <http://example.org/res> .
            }
          }
        }
        {
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          MINUS
          {
            SERVICE <ex:org3> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithMultipleServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org3"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org1> {
          {
            ?S rdfs:label <http://example.org/res> .
          }
        }
        SERVICE <ex:org2> {
          {
            ?S rdfs:label "label"@EN .
          }
        }
        SERVICE <ex:org3> {
          {
            ?S rdfs:comment rdfs:Class .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithPatternGroupFollowedByMultipleUnionServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label <http://example.org/res> .
        }
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          UNION
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithPatternGroupFollowedByMultipleMinusServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("http://example.org/res"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?S rdfs:label <http://example.org/res> .
        }
        {
          {
            SERVICE <ex:org1> {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
          MINUS
          {
            SERVICE <ex:org2> {
              {
                ?S rdfs:comment rdfs:Class .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithValuesInjectedIntoServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddValues(new RDFValues()
          .AddColumn(new RDFVariable("?V"), [new RDFResource("ex:val1")]))
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <ex:org> {
          {
            VALUES ?V { <ex:val1> } .
            ?S rdfs:label "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithEmptyPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithEmptyOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup().Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        OPTIONAL {
          {
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithEmptySingleUnionPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup().UnionWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithEmptySingleMinusPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup().MinusWithNext());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithEmptyServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AsService(new RDFSPARQLEndpoint(RDFVocabulary.RDFS.RESOURCE.URI)));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        SERVICE <http://www.w3.org/2000/01/rdf-schema#Resource> {
          {
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixed()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddProjectionVariable(new RDFVariable("?S"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionUnprefixed()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddProjectionVariable(new RDFVariable("?S"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT ?S
      WHERE {
        {
          ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndOptionalPattern()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).Optional()))
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          ?S rdfs:label "label"@EN .
          OPTIONAL { <ex:subj> <ex:pred> ?T } .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .Optional())
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        OPTIONAL {
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndOptionalPatternAndOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj1"), new RDFResource("ex:pred1"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj2"), new RDFResource("ex:pred2"), new RDFVariable("?T")).Optional())
        .Optional())
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        OPTIONAL {
          {
            <ex:subj1> <ex:pred1> ?T .
            OPTIONAL { <ex:subj2> <ex:pred2> ?T } .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndUnionPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T"))))
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext()) //Will not be printed, since this is the last evaluable query member
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupFollowedByPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        {
          _:12345 rdfs:label ?T .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupAllHavingUnion()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?T")), new RDFVariable("?TV")))
        .UnionWithNext())
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"))
      .AddProjectionVariable(new RDFVariable("?TVV"), new RDFVariableExpression(new RDFVariable("?TV")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T (?TV AS ?TVV)
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
          UNION
          {
            BIND(?T AS ?TV) .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupFollowedByOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional())
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupHavingOptionalPatternsAndFollowedByOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("?P"), new RDFResource("bnode:12345")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional())
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
            OPTIONAL { ?S rdfs:comment "comment" } .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
            OPTIONAL { <ex:subj> ?P _:12345 } .
            OPTIONAL { ?S ?P "25"^^xsd:integer } .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupHavingMultipleUnionPatterns()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).UnionWithNext()) //UnionWithNext will not be printed, since this is the last pattern group member
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFVariable("?T")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred3"), new RDFVariable("?T")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext()) //UnionWithNext will not be printed, since this is the last evaluable query member
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            { ?S rdfs:label "label"@EN }
            UNION
            { ?S rdfs:comment "comment" }
          }
          UNION
          {
            { <ex:subj> <ex:pred> ?T }
            UNION
            { <ex:subj> <ex:pred2> ?T }
            UNION
            { OPTIONAL { <ex:subj> <ex:pred3> ?T } }
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMinusPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T"))))
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          MINUS
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleMinusPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .MinusWithNext()) //Will not be printed, since this is the last evaluable query member
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          MINUS
          {
            <ex:subj> <ex:pred> ?T .
          }
          MINUS
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleMinusPatternGroupFollowedByPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          MINUS
          {
            <ex:subj> <ex:pred> ?T .
          }
          MINUS
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        {
          _:12345 rdfs:label ?T .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleMinusPatternGroupAllHavingMinus()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddBind(new RDFBind(new RDFVariableExpression(new RDFVariable("?T")), new RDFVariable("?TV")))
        .MinusWithNext())
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"))
      .AddProjectionVariable(new RDFVariable("?TVV"), new RDFVariableExpression(new RDFVariable("?TV")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T (?TV AS ?TVV)
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          MINUS
          {
            <ex:subj> <ex:pred> ?T .
          }
          MINUS
          {
            <ex:subj> rdfs:label ?T .
          }
          MINUS
          {
            BIND(?T AS ?TV) .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleMinusPatternGroupFollowedByOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional())
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          MINUS
          {
            <ex:subj> <ex:pred> ?T .
          }
          MINUS
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleMinusPatternGroupHavingOptionalPatternsAndFollowedByOptionalPatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).Optional())
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("?P"), new RDFResource("bnode:12345")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional())
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional())
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
            OPTIONAL { ?S rdfs:comment "comment" } .
          }
          MINUS
          {
            <ex:subj> <ex:pred> ?T .
            OPTIONAL { <ex:subj> ?P _:12345 } .
            OPTIONAL { ?S ?P "25"^^xsd:integer } .
          }
          MINUS
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleMinusPatternGroupHavingMultipleMinusPatterns()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).MinusWithNext()) //MinusWithNext will not be printed, since this is the last pattern group member
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFVariable("?T")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred3"), new RDFVariable("?T")).Optional())
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .MinusWithNext()) //MinusWithNext will not be printed, since this is the last evaluable query member
      .AddProjectionVariable(new RDFVariable("?S"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S ?T
      WHERE {
        {
          {
            { ?S rdfs:label "label"@EN }
            MINUS
            { ?S rdfs:comment "comment" }
          }
          MINUS
          {
            { <ex:subj> <ex:pred> ?T }
            MINUS
            { <ex:subj> <ex:pred2> ?T }
            MINUS
            { OPTIONAL { <ex:subj> <ex:pred3> ?T } }
          }
          MINUS
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithStarSubQuery()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          SELECT *
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithProjectionSubQuery()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .AddProjectionVariable(new RDFVariable("?S")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          SELECT ?S
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithProjectionSubQueryHavingServicePatternGroup()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).Optional())
          .AsService(new RDFSPARQLEndpoint(new Uri("ex:org"))))
        .AddProjectionVariable(new RDFVariable("?S")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          SELECT ?S
          WHERE {
            SERVICE <ex:org> {
              {
                OPTIONAL { ?S rdfs:label "label"@EN } .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryStarWithProjectionSubQueryHavingUnionServicePatternGroups()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).Optional())
          .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")))
          .UnionWithNext())
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, RDFVocabulary.RDFS.COMMENT).Optional())
          .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2"))))
        .AddProjectionVariable(new RDFVariable("?S")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          SELECT ?S
          WHERE {
            {
              {
                SERVICE <ex:org1> {
                  {
                    OPTIONAL { ?S rdfs:label "label"@EN } .
                  }
                }
              }
              UNION
              {
                SERVICE <ex:org2> {
                  {
                    OPTIONAL { ?S rdfs:label rdfs:comment } .
                  }
                }
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionWithStarSubQuery()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())))
      .AddProjectionVariable(new RDFVariable("?S"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?S
      WHERE {
        {
          SELECT *
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionWithProjectionSubQuery()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddProjectionVariable(new RDFVariable("?S"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?S
      WHERE {
        {
          SELECT ?T
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionWithOptionalSubQuery()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddProjectionVariable(new RDFVariable("?S"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?S
      WHERE {
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionWithMultipleSubQueries()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddProjectionVariable(new RDFVariable("?Z"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?Z ?T
      WHERE {
        {
          SELECT ?Z
          WHERE {
            {
              ?S rdfs:label _:12345 .
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionWithMultipleOptionalSubQueries()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddProjectionVariable(new RDFVariable("?Z"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?Z ?T
      WHERE {
        {
          SELECT ?Z
          WHERE {
            {
              ?S rdfs:label _:12345 .
            }
          }
        }
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionWithMultipleUnionSubQueries()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddProjectionVariable(new RDFVariable("?Z"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?Z ?T
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionWithMultipleUnionAllSubQueries()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .UnionWithNext() //This subquery will not be printed as union, since it is the last query member
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddProjectionVariable(new RDFVariable("?Z"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?Z ?T
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryProjectionWithMultipleOptionalAndUnionSubQueries()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddProjectionVariable(new RDFVariable("?Z"))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?Z ?T
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQuery()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
            .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
          .AddModifier(new RDFDistinctModifier())
          .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
          .AddModifier(new RDFLimitModifier(5))
          .AddModifier(new RDFOffsetModifier(1)))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [new RDFResource("ex:org")]))
            .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
              .AddAlternativeSteps([
                new RDFPropertyPathStep(RDFVocabulary.RDFS.CLASS),
                new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                new RDFPropertyPathStep(RDFVocabulary.OWL.CLASS).Inverse()
              ])))
          .AddProjectionVariable(new RDFVariable("?START"))))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT *
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                SELECT DISTINCT *
                WHERE {
                  {
                    ?S rdfs:label _:12345 .
                    FILTER ( (BOUND(?S)) ) 
                  }
                }
                ORDER BY ASC(?S)
                LIMIT 5
                OFFSET 1
              }
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^xsd:float))
          }
          UNION
          {
            SELECT *
            WHERE {
              {
                SELECT ?START
                WHERE {
                  {
                    VALUES ?S { <ex:org> } .
                    ?START (rdfs:Class|rdfs:label|^<http://www.w3.org/2002/07/owl#Class>) ?END .
                  }
                }
              }
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMinus()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
          .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
        .AddModifier(new RDFDistinctModifier())
        .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
        .AddModifier(new RDFLimitModifier(5))
        .AddModifier(new RDFOffsetModifier(1))
        .MinusWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .MinusWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta","it"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT *
      WHERE {
        {
          {
            SELECT DISTINCT *
            WHERE {
              {
                ?S rdfs:label _:12345 .
                FILTER ( (BOUND(?S)) ) 
              }
            }
            ORDER BY ASC(?S)
            LIMIT 5
            OFFSET 1
          }
          MINUS
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^xsd:float))
          }
          MINUS
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "eitchetta"@IT .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus1()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
          .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
        .AddModifier(new RDFDistinctModifier())
        .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
        .AddModifier(new RDFLimitModifier(5))
        .AddModifier(new RDFOffsetModifier(1))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .MinusWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta","it"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT *
      WHERE {
        {
          {
            SELECT DISTINCT *
            WHERE {
              {
                ?S rdfs:label _:12345 .
                FILTER ( (BOUND(?S)) ) 
              }
            }
            ORDER BY ASC(?S)
            LIMIT 5
            OFFSET 1
          }
          UNION
          {
            {
              SELECT ?S (AVG(?S) AS ?AVG_S)
              WHERE {
                {
                  ?S rdfs:label "label"@EN .
                }
              }
              GROUP BY ?S
              HAVING ((AVG(?S) >= "11.44"^^xsd:float))
            }
            MINUS
            {
              SELECT ?T
              WHERE {
                {
                  ?S rdfs:label "eitchetta"@IT .
                }
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus2()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .MinusWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
        .AddModifier(new RDFDistinctModifier())
        .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
        .AddModifier(new RDFLimitModifier(5))
        .AddModifier(new RDFOffsetModifier(1))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT *
      WHERE {
        {
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
          UNION
          {
            {
              SELECT ?S (AVG(?S) AS ?AVG_S)
              WHERE {
                {
                  ?S rdfs:label "label"@EN .
                }
              }
              GROUP BY ?S
              HAVING ((AVG(?S) >= "11.44"^^<http://www.w3.org/2001/XMLSchema#float>))
            }
            MINUS
            {
              SELECT DISTINCT ?T
              WHERE {
                {
                  ?S rdfs:label "eitchetta"@IT .
                }
              }
              ORDER BY ASC(?S)
              LIMIT 5
              OFFSET 1
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus3()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .MinusWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
        .AddModifier(new RDFDistinctModifier())
        .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
        .AddModifier(new RDFLimitModifier(5))
        .AddModifier(new RDFOffsetModifier(1))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT *
      WHERE {
        {
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
          MINUS
          {
            {
              SELECT ?S (AVG(?S) AS ?AVG_S)
              WHERE {
                {
                  ?S rdfs:label "label"@EN .
                }
              }
              GROUP BY ?S
              HAVING ((AVG(?S) >= "11.44"^^<http://www.w3.org/2001/XMLSchema#float>))
            }
            UNION
            {
              SELECT DISTINCT ?T
              WHERE {
                {
                  ?S rdfs:label "eitchetta"@IT .
                }
              }
              ORDER BY ASC(?S)
              LIMIT 5
              OFFSET 1
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus4()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .MinusWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
      .AddModifier(new RDFDistinctModifier())
      .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
      .AddModifier(new RDFLimitModifier(5))
      .AddModifier(new RDFOffsetModifier(1))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT DISTINCT ?T
      WHERE {
        {
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
          MINUS
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^<http://www.w3.org/2001/XMLSchema#float>))
          }
          MINUS
          {
            ?S rdfs:label "eitchetta"@IT .
          }
        }
      }
      ORDER BY ASC(?S)
      LIMIT 5
      OFFSET 1

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus5()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("etiquette", "fr")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .UnionWithNext())
      .AddModifier(new RDFDistinctModifier())
      .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
      .AddModifier(new RDFLimitModifier(5))
      .AddModifier(new RDFOffsetModifier(1))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT DISTINCT ?T
      WHERE {
        {
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
          MINUS
          {
            {
              ?S rdfs:label "label"@EN .
            }
            UNION
            {
              ?S rdfs:label "etiquette"@FR .
            }
            MINUS
            {
              ?S rdfs:label "eitchetta"@IT .
            }
          }
        }
      }
      ORDER BY ASC(?S)
      LIMIT 5
      OFFSET 1

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus6()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
          .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
          .MinusWithNext())
        .AddSubQuery(new RDFSelectQuery()
          .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
          .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
            .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
          .AddModifier(new RDFDistinctModifier())
          .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC)))
        .AddModifier(new RDFLimitModifier(5))
        .AddModifier(new RDFOffsetModifier(1))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?T
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                {
                  ?S rdfs:label _:12345 .
                  FILTER ( (BOUND(?S)) ) 
                }
                MINUS
                {
                  SELECT DISTINCT *
                  WHERE {
                    {
                      ?S rdfs:label _:12345 .
                      FILTER ( (BOUND(?S)) ) 
                    }
                  }
                  ORDER BY ASC(?S)
                }
              }
            }
            LIMIT 5
            OFFSET 1
          }
          UNION
          {
            {
              SELECT ?S (AVG(?S) AS ?AVG_S)
              WHERE {
                {
                  ?S rdfs:label "label"@EN .
                }
              }
              GROUP BY ?S
              HAVING ((AVG(?S) >= "11.44"^^xsd:float))
            }
            MINUS
            {
              ?S1 rdfs:label "eitchetta"@IT .
            }
          }
        }
        {
          ?S2 rdfs:label "eitchetta"@IT .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus7()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
          .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
        .AddSubQuery(new RDFSelectQuery()
          .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
          .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
            .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
          .AddModifier(new RDFDistinctModifier())
          .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC)))
        .AddModifier(new RDFLimitModifier(5))
        .AddModifier(new RDFOffsetModifier(1)))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .Optional())
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?T
      WHERE {
        {
          SELECT *
          WHERE {
            {
              ?S rdfs:label _:12345 .
              FILTER ( (BOUND(?S)) ) 
            }
            {
              SELECT DISTINCT *
              WHERE {
                {
                  ?S rdfs:label _:12345 .
                  FILTER ( (BOUND(?S)) ) 
                }
              }
              ORDER BY ASC(?S)
            }
          }
          LIMIT 5
          OFFSET 1
        }
        {
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^xsd:float))
          }
          MINUS
          {
            ?S1 rdfs:label "eitchetta"@IT .
          }
        }
        OPTIONAL {
          {
            ?S2 rdfs:label "eitchetta"@IT .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus8()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
          .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
        .AddSubQuery(new RDFSelectQuery()
          .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
          .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
            .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
          .AddModifier(new RDFDistinctModifier())
          .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC)))
        .AddModifier(new RDFLimitModifier(5))
        .AddModifier(new RDFOffsetModifier(1)))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .Optional())
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT ?T
      WHERE {
        {
          SELECT *
          WHERE {
            {
              ?S rdfs:label _:12345 .
              FILTER ( (BOUND(?S)) ) 
            }
            {
              SELECT DISTINCT *
              WHERE {
                {
                  ?S rdfs:label _:12345 .
                  FILTER ( (BOUND(?S)) ) 
                }
              }
              ORDER BY ASC(?S)
            }
          }
          LIMIT 5
          OFFSET 1
        }
        {
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^xsd:float))
          }
          MINUS
          {
            {
              ?S1 rdfs:label "eitchetta"@IT .
            }
            UNION
            {
              ?S2 rdfs:label "eitchetta"@IT .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus9A()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .Optional())
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT ?T
      WHERE {
        {
          ?S rdfs:label _:12345 .
          FILTER ( (BOUND(?S)) ) 
        }
        OPTIONAL {
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
        }
        {
          ?S1 rdfs:label "eitchetta"@IT .
        }
        OPTIONAL {
          {
            ?S2 rdfs:label "eitchetta"@IT .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus9B()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
          MINUS
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
        }
        OPTIONAL {
          {
            ?S1 rdfs:label "eitchetta"@IT .
          }
        }
        OPTIONAL {
          {
            ?S2 rdfs:label "eitchetta"@IT .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus9C()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
          MINUS
          {
            {
              ?S rdfs:label _:12345 .
              FILTER ( (BOUND(?S)) ) 
            }
            UNION
            {
              ?S1 rdfs:label "eitchetta"@IT .
            }
            UNION
            {
              ?S2 rdfs:label "eitchetta"@IT .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus9D()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            ?S rdfs:label _:12345 .
            FILTER ( (BOUND(?S)) ) 
          }
          MINUS
          {
            {
              ?S rdfs:label _:12345 .
              FILTER ( (BOUND(?S)) ) 
            }
            UNION
            {
              ?S1 rdfs:label "eitchetta"@IT .
            }
            MINUS
            {
              ?S2 rdfs:label "eitchetta"@IT .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinus10()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?S"), new RDFVariable("?E"))
          .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE))
          .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDFS.SUB_CLASS_OF)))
        .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?S"), new RDFVariable("?E"))
          .AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE),
            new RDFPropertyPathStep(RDFVocabulary.RDFS.SUB_CLASS_OF) ]))
        .AddValues(new RDFValues()
          .AddColumn(new RDFVariable("?S"), [ new RDFPlainLiteral("test") ]))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type>/rdfs:subClassOf ?E .
            FILTER ( (BOUND(?S)) ) 
          }
          MINUS
          {
            {
              ?S (<http://www.w3.org/1999/02/22-rdf-syntax-ns#type>|rdfs:subClassOf) ?E .
              VALUES ?S { "test" } .
            }
            UNION
            {
              ?S1 rdfs:label "eitchetta"@IT .
            }
            MINUS
            {
              ?S2 rdfs:label "eitchetta"@IT .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatterns1()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S1 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S2 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          }
          ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatterns2()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S1 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S2 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          }
          OPTIONAL { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatterns3()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S1 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S2 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { OPTIONAL { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } }
          }
          ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
          OPTIONAL { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatterns4()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S1 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S2 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { OPTIONAL { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } }
          }
          { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { OPTIONAL { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatterns5()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          UNION
          { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          UNION
          { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S7 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatterns6()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S10"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          UNION
          { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          UNION
          { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          { ?S7 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            { ?S10 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatterns7()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S10"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            {
              { ?S7 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              {
                { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                UNION
                { ?S10 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatterns8()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S10"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          UNION
          { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S7 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S10 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatternsHavingPropertyPath()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?S5A"), new RDFVariable("?S6A"))
          .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:step1")))
          .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:step2"))))
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?S5B"), new RDFVariable("?S6B"))
          .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:step1")))
          .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:step2"))))
        .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S10"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?S5C"), new RDFVariable("?S6C"))
          .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:step1")))
          .AddSequenceStep(new RDFPropertyPathStep(new RDFResource("ex:step2"))))
        .AddPattern(new RDFPattern(new RDFVariable("?S11"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S5A <ex:step1>/<ex:step2> ?S6A }
          }
          { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S5B <ex:step1>/<ex:step2> ?S6B }
          { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S10 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S5C <ex:step1>/<ex:step2> ?S6C }
          }
          ?S11 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatternsHavingValues()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddValues(new RDFValues().AddColumn(new RDFVariable("?V"), [ null ]))
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddValues(new RDFValues().AddColumn(new RDFVariable("?V"), [ null ]))
        .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S10"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddValues(new RDFValues().AddColumn(new RDFVariable("?V"), [null]))
        .AddPattern(new RDFPattern(new RDFVariable("?S11"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { VALUES ?V { UNDEF } }
          }
          { ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { VALUES ?V { UNDEF } }
          { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S10 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { VALUES ?V { UNDEF } }
          }
          ?S11 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatternsHavingBind()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFPlainLiteral("val")), new RDFVariable("?V")))
        .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFPlainLiteral("val")), new RDFVariable("?V2")))
        .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S10"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFPlainLiteral("val")), new RDFVariable("?V3")))
        .AddPattern(new RDFPattern(new RDFVariable("?S11"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional()));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          BIND("val" AS ?V) .
          ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
          BIND("val" AS ?V2) .
          { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          MINUS
          {
            { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S10 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          }
          BIND("val" AS ?V3) .
          OPTIONAL { ?S11 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatternsAndPatternGroups1()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S3C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S5C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S2D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S3D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          {
            { ?S1A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S2A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            { ?S3A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            {
              { ?S4A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S5A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            }
          }
          UNION
          {
            { ?S1B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            {
              { ?S2B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S3B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              { ?S4B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              { OPTIONAL { ?S5B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } }
            }
          }
          UNION
          {
            {
              { ?S1C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              { ?S2C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              { ?S3C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S4C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              OPTIONAL { ?S5C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
            }
            MINUS
            {
              ?S1D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
              OPTIONAL { ?S2D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
              { ?S3D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S4D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S5D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatternsAndPatternGroups2()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S3C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S5C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S2D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S3D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          {
            { ?S1A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S2A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            { ?S3A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            {
              { ?S4A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S5A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            }
          }
          MINUS
          {
            { ?S1B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            {
              { ?S2B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S3B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              { ?S4B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              { OPTIONAL { ?S5B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } }
            }
          }
          MINUS
          {
            {
              { ?S1C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              { ?S2C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              { ?S3C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S4C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              OPTIONAL { ?S5C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
            }
            UNION
            {
              ?S1D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
              OPTIONAL { ?S2D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
              { ?S3D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S4D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S5D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingMixedUnionMinusInPatternsAndPatternGroups3()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5A"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S3B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S2C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S3C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S5C"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S1D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .AddPattern(new RDFPattern(new RDFVariable("?S2D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S3D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S4D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S5D"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          {
            { ?S1A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S2A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            { ?S3A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            {
              { ?S4A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S5A <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            }
          }
          MINUS
          {
            { ?S1B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            {
              { ?S2B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              UNION
              { ?S3B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              { ?S4B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
              MINUS
              { OPTIONAL { ?S5B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } }
            }
          }
        }
        {
          {
            { ?S1C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            MINUS
            { ?S2C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            { ?S3C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S4C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            OPTIONAL { ?S5C <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
          }
          UNION
          {
            ?S1D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
            OPTIONAL { ?S2D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } .
            { ?S3D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S4D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
            UNION
            { ?S5D <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingSubQueryWithMixedUnionMinusInPatterns1()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
          .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
          .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
          .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
          .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
          .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
        .MinusWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddSubQuery(new RDFSelectQuery()
          .AddSubQuery(new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
              .AddPattern(new RDFPattern(new RDFVariable("?S7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S10"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S11"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S12"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
              .UnionWithNext())
            .AddPatternGroup(new RDFPatternGroup()
              .AddPattern(new RDFPattern(new RDFVariable("?S7B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S8B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S9B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S10B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S11B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional()))
            .MinusWithNext()
            .AddPatternGroup(new RDFPatternGroup()
              .AddPattern(new RDFPattern(new RDFVariable("?S7B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))))
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                { ?S1 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                MINUS
                { ?S2 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                MINUS
                {
                  { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                  UNION
                  { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                  UNION
                  { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                }
                ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
              }
            }
          }
          MINUS
          {
            SELECT *
            WHERE {
              {
                SELECT *
                WHERE {
                  {
                    SELECT *
                    WHERE {
                      {
                        {
                          { ?S7 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          MINUS
                          {
                            { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                            UNION
                            { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                            MINUS
                            { ?S10 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                            MINUS
                            {
                              { ?S11 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                              UNION
                              { ?S12 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                            }
                          }
                        }
                        UNION
                        {
                          { ?S7B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          UNION
                          { ?S8B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          UNION
                          { ?S9B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          UNION
                          { ?S10B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          MINUS
                          { OPTIONAL { ?S11B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } }
                        }
                      }
                      {
                        ?S7B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingSubQueryWithMixedUnionMinusInPatterns2()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
          .AddPattern(new RDFPattern(new RDFVariable("?S2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
          .AddPattern(new RDFPattern(new RDFVariable("?S3"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
          .AddPattern(new RDFPattern(new RDFVariable("?S4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
          .AddPattern(new RDFPattern(new RDFVariable("?S5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
          .AddPattern(new RDFPattern(new RDFVariable("?S6"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddSubQuery(new RDFSelectQuery()
          .AddSubQuery(new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
              .AddPattern(new RDFPattern(new RDFVariable("?S7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S10"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S11"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S12"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")))
              .MinusWithNext())
            .AddPatternGroup(new RDFPatternGroup()
              .AddPattern(new RDFPattern(new RDFVariable("?S7B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S8B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S9B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).UnionWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S10B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).MinusWithNext())
              .AddPattern(new RDFPattern(new RDFVariable("?S11B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it")).Optional()))
            .UnionWithNext()
            .AddPatternGroup(new RDFPatternGroup()
              .AddPattern(new RDFPattern(new RDFVariable("?S7B"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("eitchetta", "it"))))))
      );
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT *
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                { ?S1 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                MINUS
                { ?S2 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                MINUS
                {
                  { ?S3 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                  UNION
                  { ?S4 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                  UNION
                  { ?S5 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                }
                ?S6 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
              }
            }
          }
          UNION
          {
            SELECT *
            WHERE {
              {
                SELECT *
                WHERE {
                  {
                    SELECT *
                    WHERE {
                      {
                        {
                          { ?S7 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          MINUS
                          {
                            { ?S8 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                            UNION
                            { ?S9 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                            MINUS
                            { ?S10 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                            MINUS
                            {
                              { ?S11 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                              UNION
                              { ?S12 <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                            }
                          }
                        }
                        MINUS
                        {
                          { ?S7B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          UNION
                          { ?S8B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          UNION
                          { ?S9B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          UNION
                          { ?S10B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT }
                          MINUS
                          { OPTIONAL { ?S11B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT } }
                        }
                      }
                      {
                        ?S7B <http://www.w3.org/2000/01/rdf-schema#label> "eitchetta"@IT .
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingBinds1()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
            .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S"))))
            .AddBind(new RDFBind(new RDFAbsExpression(new RDFVariable("?T")), new RDFVariable("?ABST"))))
          .AddModifier(new RDFDistinctModifier())
          .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
          .AddModifier(new RDFLimitModifier(5))
          .AddModifier(new RDFOffsetModifier(1)))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
          .AddBind(new RDFBind(new RDFBooleanAndExpression(new RDFVariableExpression(new RDFVariable("?T")), new RDFVariableExpression(new RDFVariable("?Q"))), new RDFVariable("?ANDTQ"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [new RDFResource("ex:org")]))
            .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
              .AddAlternativeSteps([
                new RDFPropertyPathStep(RDFVocabulary.RDFS.CLASS),
                new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                new RDFPropertyPathStep(RDFVocabulary.OWL.CLASS).Inverse()
              ]))
            .AddBind(new RDFBind(new RDFAbsExpression(new RDFVariable("?T")), new RDFVariable("?ABST"))))
          .AddProjectionVariable(new RDFVariable("?START"))
          .AddProjectionVariable(new RDFVariable("?STARTLEN"), new RDFStrLenExpression(new RDFVariable("?START")))))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      SELECT *
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                SELECT DISTINCT *
                WHERE {
                  {
                    ?S rdfs:label _:12345 .
                    BIND((ABS(?T)) AS ?ABST) .
                    FILTER ( (BOUND(?S)) ) 
                  }
                }
                ORDER BY ASC(?S)
                LIMIT 5
                OFFSET 1
              }
              {
                ?S rdfs:label _:12345 .
                BIND((?T && ?Q) AS ?ANDTQ) .
              }
            }
          }
          UNION
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^xsd:float))
          }
          UNION
          {
            SELECT *
            WHERE {
              {
                SELECT ?START ((STRLEN(?START)) AS ?STARTLEN)
                WHERE {
                  {
                    VALUES ?S { <ex:org> } .
                    ?START (rdfs:Class|rdfs:label|^<http://www.w3.org/2002/07/owl#Class>) ?END .
                    BIND((ABS(?T)) AS ?ABST) .
                  }
                }
              }
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingBinds2()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t")), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFVariable("?T"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .UnionWithNext())
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT ?T
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
              }
            }
          }
          UNION
          {
            BIND(<ex:t> AS ?T) .
            ?T <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingBinds3()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t")), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFVariable("?T"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).UnionWithNext())
        .UnionWithNext())
      .AddProjectionVariable(new RDFVariable("?T"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT ?T
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
              }
            }
          }
          UNION
          {
            BIND(<ex:t> AS ?T) .
            ?T <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingBinds4()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?T1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label1", "en")).UnionWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t")), new RDFVariable("?T3")))
        .AddPattern(new RDFPattern(new RDFVariable("?T2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label2", "en")).UnionWithNext())
        .UnionWithNext())
      .AddProjectionVariable(new RDFVariable("?T3"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT ?T3
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
              }
            }
          }
          UNION
          {
            ?T1 <http://www.w3.org/2000/01/rdf-schema#label> "label1"@EN .
            BIND(<ex:t> AS ?T3) .
            ?T2 <http://www.w3.org/2000/01/rdf-schema#label> "label2"@EN .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingBinds5()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?T1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label1", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label2", "en")).UnionWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t3")), new RDFVariable("?T3")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?T4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label4", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label5", "en")).UnionWithNext())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t6")), new RDFVariable("?T6")))
        .AddPattern(new RDFPattern(new RDFVariable("?T7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label7", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label8", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label9", "en")).Optional())
        .UnionWithNext())
      .AddProjectionVariable(new RDFVariable("?T3"))
      .AddProjectionVariable(new RDFVariable("?T6"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT ?T3 ?T6
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
              }
            }
          }
          UNION
          {
            { ?T1 <http://www.w3.org/2000/01/rdf-schema#label> "label1"@EN }
            UNION
            { ?T2 <http://www.w3.org/2000/01/rdf-schema#label> "label2"@EN }
            BIND(<ex:t3> AS ?T3) .
          }
          UNION
          {
            { ?T4 <http://www.w3.org/2000/01/rdf-schema#label> "label4"@EN }
            UNION
            { ?T5 <http://www.w3.org/2000/01/rdf-schema#label> "label5"@EN }
            BIND(<ex:t6> AS ?T6) .
            { ?T7 <http://www.w3.org/2000/01/rdf-schema#label> "label7"@EN }
            UNION
            { ?T8 <http://www.w3.org/2000/01/rdf-schema#label> "label8"@EN }
            UNION
            { OPTIONAL { ?T9 <http://www.w3.org/2000/01/rdf-schema#label> "label9"@EN } }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingBinds6()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?T1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label1", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label2", "en")).Optional())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t3")), new RDFVariable("?T3")))
        .Optional())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?T4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label4", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label5", "en")).Optional())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t6")), new RDFVariable("?T6")))
        .AddPattern(new RDFPattern(new RDFVariable("?T7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label7", "en")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?T8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label8", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label9", "en")).Optional())
        .UnionWithNext())
      .AddProjectionVariable(new RDFVariable("?T3"))
      .AddProjectionVariable(new RDFVariable("?T6"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT ?T3 ?T6
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
              }
            }
          }
          UNION
          {
            { ?T1 <http://www.w3.org/2000/01/rdf-schema#label> "label1"@EN }
            UNION
            { OPTIONAL { ?T2 <http://www.w3.org/2000/01/rdf-schema#label> "label2"@EN } }
            BIND(<ex:t3> AS ?T3) .
          }
        }
        {
          { ?T4 <http://www.w3.org/2000/01/rdf-schema#label> "label4"@EN }
          UNION
          { OPTIONAL { ?T5 <http://www.w3.org/2000/01/rdf-schema#label> "label5"@EN } }
          BIND(<ex:t6> AS ?T6) .
          OPTIONAL { ?T7 <http://www.w3.org/2000/01/rdf-schema#label> "label7"@EN } .
          { ?T8 <http://www.w3.org/2000/01/rdf-schema#label> "label8"@EN }
          UNION
          { OPTIONAL { ?T9 <http://www.w3.org/2000/01/rdf-schema#label> "label9"@EN } }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexSelectQueryHavingBindsAndMixedUnionMinus()
  {
    RDFSelectQuery query = new RDFSelectQuery()
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"))))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?T1"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label1", "en")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T2"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label2", "en")).Optional())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t3")), new RDFVariable("?T3")))
        .MinusWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?T4"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label4", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T5"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label5", "en")).Optional())
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t6")), new RDFVariable("?T6")))
        .AddPattern(new RDFPattern(new RDFVariable("?T7"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label7", "en")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T8"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label8", "en")).MinusWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?T9"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label9", "en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddBind(new RDFBind(new RDFConstantExpression(new RDFResource("ex:t7")), new RDFVariable("?T7"))))
      .AddProjectionVariable(new RDFVariable("?T3"))
      .AddProjectionVariable(new RDFVariable("?T6"));
    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    string expectedQueryString =
      """
      SELECT ?T3 ?T6
      WHERE {
        {
          {
            SELECT *
            WHERE {
              {
                ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
              }
            }
          }
          UNION
          {
            {
              { ?T1 <http://www.w3.org/2000/01/rdf-schema#label> "label1"@EN }
              MINUS
              { OPTIONAL { ?T2 <http://www.w3.org/2000/01/rdf-schema#label> "label2"@EN } }
              BIND(<ex:t3> AS ?T3) .
            }
            MINUS
            {
              { ?T4 <http://www.w3.org/2000/01/rdf-schema#label> "label4"@EN }
              UNION
              { OPTIONAL { ?T5 <http://www.w3.org/2000/01/rdf-schema#label> "label5"@EN } }
              BIND(<ex:t6> AS ?T6) .
              { ?T7 <http://www.w3.org/2000/01/rdf-schema#label> "label7"@EN }
              MINUS
              { ?T8 <http://www.w3.org/2000/01/rdf-schema#label> "label8"@EN }
              MINUS
              { ?T9 <http://www.w3.org/2000/01/rdf-schema#label> "label9"@EN }
            }
            UNION
            {
              BIND(<ex:t7> AS ?T7) .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  //ASK
  [TestMethod]
  public void ShouldPrintAskQueryNull()
    => Assert.IsTrue(string.IsNullOrEmpty(RDFQueryPrinter.PrintAskQuery(null)));

  [TestMethod]
  public void ShouldPrintAskQueryEmpty()
  {
    RDFAskQuery query = new RDFAskQuery();
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      ASK
      WHERE {
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryPrefixed()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryUnprefixed()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      ASK
      WHERE {
        {
          ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithOptionalPattern()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          OPTIONAL { ?S rdfs:label "label"@EN } .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithOptionalPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        OPTIONAL {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithOptionalPatternAndOptionalPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
        .Optional());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        OPTIONAL {
          {
            OPTIONAL { ?S rdfs:label "label"@EN } .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithSingleUnionPattern()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext()));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithSingleUnionPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithSingleUnionPatternAndSingleUnionPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithEmptyPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPatternGroup(new RDFPatternGroup());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      ASK
      WHERE {
        {
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithEmptyOptionalPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPatternGroup(new RDFPatternGroup().Optional());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      ASK
      WHERE {
        OPTIONAL {
          {
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithEmptySingleUnionPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPatternGroup(new RDFPatternGroup().UnionWithNext());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      ASK
      WHERE {
        {
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultiplePatterns()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).Optional()));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          ?S rdfs:label "label"@EN .
          OPTIONAL { <ex:subj> <ex:pred> ?T } .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultiplePatternGroups()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        OPTIONAL {
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultiplePatternsAndMultiplePatternGroups()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj1"), new RDFResource("ex:pred1"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj2"), new RDFResource("ex:pred2"), new RDFVariable("?T")).Optional())
        .Optional());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        OPTIONAL {
          {
            <ex:subj1> <ex:pred1> ?T .
            OPTIONAL { <ex:subj2> <ex:pred2> ?T } .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithUnionPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T"))));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleUnionPatternGroups()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext()); //Will not be printed, since this is the last evaluable query member
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleUnionPatternGroupsFollowedByPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        {
          _:12345 rdfs:label ?T .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleUnionPatternGroupsFollowedByOptionalPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleUnionPatternGroupsHavingOptionalPatternsAndFollowedByOptionalPatternGroup()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("?P"), new RDFResource("bnode:12345")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

      ASK
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
            OPTIONAL { ?S rdfs:comment "comment" } .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
            OPTIONAL { <ex:subj> ?P _:12345 } .
            OPTIONAL { ?S ?P "25"^^xsd:integer } .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleUnionPatternGroupsHavingMultipleUnionPatterns()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).UnionWithNext()) //UnionWithNext will not be printed, since this is the last pattern group member
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFVariable("?T")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred3"), new RDFVariable("?T")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext()); //UnionWithNext will not be printed, since this is the last evaluable query member
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          {
            { ?S rdfs:label "label"@EN }
            UNION
            { ?S rdfs:comment "comment" }
          }
          UNION
          {
            { <ex:subj> <ex:pred> ?T }
            UNION
            { <ex:subj> <ex:pred2> ?T }
            UNION
            { OPTIONAL { <ex:subj> <ex:pred3> ?T } }
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithStarSubQuery()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          SELECT *
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithProjectionSubQuery()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .AddProjectionVariable(new RDFVariable("?S")));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          SELECT ?S
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithStarSubQueryHavingOptionalPattern()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      ASK
      WHERE {
        {
          SELECT *
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithProjectionSubQueryHavingOptionalPattern()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      ASK
      WHERE {
        {
          SELECT ?T
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithOptionalProjectionSubQuery()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      ASK
      WHERE {
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleProjectionSubQueries()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      ASK
      WHERE {
        {
          SELECT ?Z
          WHERE {
            {
              ?S rdfs:label _:12345 .
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleProjectionOptionalSubQueries()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      ASK
      WHERE {
        {
          SELECT ?Z
          WHERE {
            {
              ?S rdfs:label _:12345 .
            }
          }
        }
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleUnionProjectionSubQueries()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      ASK
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintAskQueryWithMultipleOptionalAndUnionProjectionSubQueries()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      ASK
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexAskQuery()
  {
    RDFAskQuery query = new RDFAskQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
            .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
          .AddModifier(new RDFDistinctModifier())
          .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
          .AddModifier(new RDFLimitModifier(5))
          .AddModifier(new RDFOffsetModifier(1)))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [new RDFResource("ex:org")]))
            .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
              .AddAlternativeSteps([
                new RDFPropertyPathStep(RDFVocabulary.RDFS.CLASS),
                new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                new RDFPropertyPathStep(RDFVocabulary.OWL.CLASS).Inverse()
              ])))
          .AddProjectionVariable(new RDFVariable("?START"))))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      ASK
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                SELECT DISTINCT *
                WHERE {
                  {
                    ?S rdfs:label _:12345 .
                    FILTER ( (BOUND(?S)) ) 
                  }
                }
                ORDER BY ASC(?S)
                LIMIT 5
                OFFSET 1
              }
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^xsd:float))
          }
          UNION
          {
            SELECT *
            WHERE {
              {
                SELECT ?START
                WHERE {
                  {
                    VALUES ?S { <ex:org> } .
                    ?START (rdfs:Class|rdfs:label|^<http://www.w3.org/2002/07/owl#Class>) ?END .
                  }
                }
              }
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  //CONSTRUCT
  [TestMethod]
  public void ShouldPrintConstructQueryNull()
    => Assert.IsTrue(string.IsNullOrEmpty(RDFQueryPrinter.PrintConstructQuery(null)));

  [TestMethod]
  public void ShouldPrintConstructQueryEmpty()
  {
    RDFConstructQuery query = new RDFConstructQuery();
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      CONSTRUCT {
      }
      WHERE {
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesPrefixed()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
      }
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesUnprefixed()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      CONSTRUCT {
      }
      WHERE {
        {
          ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithOptionalPattern()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
      }
      WHERE {
        {
          OPTIONAL { ?S rdfs:label "label"@EN } .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithOptionalPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
      }
      WHERE {
        OPTIONAL {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithOptionalPatternAndOptionalPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
        .Optional());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
      }
      WHERE {
        OPTIONAL {
          {
            OPTIONAL { ?S rdfs:label "label"@EN } .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithSingleUnionPattern()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext()));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
      }
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithSingleUnionPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
      }
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithSingleUnionPatternAndSingleUnionPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
      }
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithEmptyPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPatternGroup(new RDFPatternGroup());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      CONSTRUCT {
      }
      WHERE {
        {
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithEmptyOptionalPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPatternGroup(new RDFPatternGroup().Optional());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      CONSTRUCT {
      }
      WHERE {
        OPTIONAL {
          {
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryEmptyTemplatesWithEmptySingleUnionPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPatternGroup(new RDFPatternGroup().UnionWithNext());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      CONSTRUCT {
      }
      WHERE {
        {
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndOptionalPattern()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).Optional()));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          ?S rdfs:label "label"@EN .
          OPTIONAL { <ex:subj> <ex:pred> ?T } .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndOptionalPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        OPTIONAL {
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndOptionalPatternAndOptionalPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj1"), new RDFResource("ex:pred1"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj2"), new RDFResource("ex:pred2"), new RDFVariable("?T")).Optional())
        .Optional());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        OPTIONAL {
          {
            <ex:subj1> <ex:pred1> ?T .
            OPTIONAL { <ex:subj2> <ex:pred2> ?T } .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndUnionPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T"))));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleUnionPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")).Optional()) //Optional will not be printed, since it is not supported by CONSTRUCT
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext()); //Will not be printed, since this is the last evaluable query member
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleUnionPatternGroupFollowedByPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")).UnionWithNext()) //UnionWithNext will not be printed, since it is not supported by CONSTRUCT
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?Z")))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
        ?S rdfs:label ?Z .
      }
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        {
          _:12345 rdfs:label ?T .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleUnionPatternGroupFollowedByOptionalPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleUnionPatternGroupHavingOptionalPatternsAndFollowedByOptionalPatternGroup()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("?P"), new RDFResource("bnode:12345")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
            OPTIONAL { ?S rdfs:comment "comment" } .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
            OPTIONAL { <ex:subj> ?P _:12345 } .
            OPTIONAL { ?S ?P "25"^^xsd:integer } .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleUnionPatternGroupHavingMultipleUnionPatterns()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).UnionWithNext()) //UnionWithNext will not be printed, since this is the last pattern group member
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFVariable("?T")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred3"), new RDFVariable("?T")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext()); //UnionWithNext will not be printed, since this is the last evaluable query member
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          {
            { ?S rdfs:label "label"@EN }
            UNION
            { ?S rdfs:comment "comment" }
          }
          UNION
          {
            { <ex:subj> <ex:pred> ?T }
            UNION
            { <ex:subj> <ex:pred2> ?T }
            UNION
            { OPTIONAL { <ex:subj> <ex:pred3> ?T } }
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndStarSubQuery()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          SELECT *
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndProjectionSubQuery()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .AddProjectionVariable(new RDFVariable("?S")));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          SELECT ?S
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndOptionalProjectionSubQuery()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleSubQueries()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          SELECT ?Z
          WHERE {
            {
              ?S rdfs:label _:12345 .
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleOptionalSubQueries()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?C"), new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))) //Context will not be printed, since it is not supported by CONSTRUCT
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          SELECT ?Z
          WHERE {
            {
              ?S rdfs:label _:12345 .
            }
          }
        }
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleUnionSubQueries()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFContext("ex:org"), new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")).Optional()) //Context will not be printed, since it is not supported by CONSTRUCT
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithTemplatesAndMultipleOptionalAndUnionSubQueries()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      CONSTRUCT {
        ?S rdfs:label ?T .
      }
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexConstructQuery()
  {
    RDFConstructQuery query = new RDFConstructQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
      .AddTemplate(new RDFPattern(new RDFVariable("?S"), new RDFResource("ex:org"), new RDFPlainLiteral("hello", "en-US")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
            .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
          .AddModifier(new RDFDistinctModifier())
          .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
          .AddModifier(new RDFLimitModifier(5))
          .AddModifier(new RDFOffsetModifier(1)))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [new RDFResource("ex:org")]))
            .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
              .AddAlternativeSteps([
                new RDFPropertyPathStep(RDFVocabulary.RDFS.CLASS),
                new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                new RDFPropertyPathStep(RDFVocabulary.OWL.CLASS).Inverse()
              ])))
          .AddProjectionVariable(new RDFVariable("?START"))))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      CONSTRUCT {
        ?S rdfs:label ?T .
        ?S <ex:org> "hello"@EN-US .
      }
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                SELECT DISTINCT *
                WHERE {
                  {
                    ?S rdfs:label _:12345 .
                    FILTER ( (BOUND(?S)) ) 
                  }
                }
                ORDER BY ASC(?S)
                LIMIT 5
                OFFSET 1
              }
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^xsd:float))
          }
          UNION
          {
            SELECT *
            WHERE {
              {
                SELECT ?START
                WHERE {
                  {
                    VALUES ?S { <ex:org> } .
                    ?START (rdfs:Class|rdfs:label|^<http://www.w3.org/2002/07/owl#Class>) ?END .
                  }
                }
              }
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  //DESCRIBE
  [TestMethod]
  public void ShouldPrintDescribeQueryNull()
    => Assert.IsTrue(string.IsNullOrEmpty(RDFQueryPrinter.PrintDescribeQuery(null)));

  [TestMethod]
  public void ShouldPrintDescribeQueryEmpty()
  {
    RDFDescribeQuery query = new RDFDescribeQuery();
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      DESCRIBE *
      WHERE {
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsPrefixed()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsUnprefixed()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      DESCRIBE *
      WHERE {
        {
          ?S <http://www.w3.org/2000/01/rdf-schema#label> "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithOptionalPattern()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE *
      WHERE {
        {
          OPTIONAL { ?S rdfs:label "label"@EN } .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithOptionalPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE *
      WHERE {
        OPTIONAL {
          {
            ?S rdfs:label "label"@EN .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithOptionalPatternAndOptionalPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
        .Optional());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE *
      WHERE {
        OPTIONAL {
          {
            OPTIONAL { ?S rdfs:label "label"@EN } .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithSingleUnionPattern()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext()));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithSingleUnionPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithSingleUnionPatternAndSingleUnionPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
        .UnionWithNext());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE *
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithEmptyPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPatternGroup(new RDFPatternGroup());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      DESCRIBE *
      WHERE {
        {
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithEmptyOptionalPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPatternGroup(new RDFPatternGroup().Optional());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      DESCRIBE *
      WHERE {
        OPTIONAL {
          {
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryStarTermsWithEmptySingleUnionPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPatternGroup(new RDFPatternGroup().UnionWithNext());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      DESCRIBE *
      WHERE {
        {
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndOptionalPattern()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddDescribeTerm(RDFVocabulary.RDFS.LABEL)
      .AddDescribeTerm(new RDFResource("bnode:12345"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).Optional()));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE ?S rdfs:label _:12345
      WHERE {
        {
          ?S rdfs:label "label"@EN .
          OPTIONAL { <ex:subj> <ex:pred> ?T } .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndOptionalPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(RDFVocabulary.RDF.TYPE)
      .AddDescribeTerm(RDFVocabulary.RDF.ALT)
      .AddDescribeTerm(RDFVocabulary.RDFS.LABEL)
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt> rdfs:label
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        OPTIONAL {
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndOptionalPatternAndOptionalPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj1"), new RDFResource("ex:pred1"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj2"), new RDFResource("ex:pred2"), new RDFVariable("?T")).Optional())
        .Optional());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE ?S
      WHERE {
        {
          ?S rdfs:label "label"@EN .
        }
        OPTIONAL {
          {
            <ex:subj1> <ex:pred1> ?T .
            OPTIONAL { <ex:subj2> <ex:pred2> ?T } .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndUnionPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFResource("bnode:12345"))
      .AddDescribeTerm(new RDFResource("bnode:54321"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T"))));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE _:12345 _:54321 ?S
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleUnionPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext()); //Will not be printed, since this is the last evaluable query member
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE ?S
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleUnionPatternGroupFollowedByPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE ?S
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        {
          _:12345 rdfs:label ?T .
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleUnionPatternGroupFollowedByOptionalPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE ?S
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleUnionPatternGroupHavingOptionalPatternsAndFollowedByOptionalPatternGroup()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("?P"), new RDFResource("bnode:12345")).Optional())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T"))))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .Optional());
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

      DESCRIBE ?S
      WHERE {
        {
          {
            ?S rdfs:label "label"@EN .
            OPTIONAL { ?S rdfs:comment "comment" } .
          }
          UNION
          {
            <ex:subj> <ex:pred> ?T .
            OPTIONAL { <ex:subj> ?P _:12345 } .
            OPTIONAL { ?S ?P "25"^^xsd:integer } .
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
        OPTIONAL {
          {
            _:12345 rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleUnionPatternGroupHavingMultipleUnionPatterns()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(RDFVocabulary.RDFS.LABEL)
      .AddDescribeTerm(new RDFResource("ex:org"))
      .AddDescribeTerm(new RDFResource("bnode:12345"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).UnionWithNext()) //UnionWithNext will not be printed, since this is the last pattern group member
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFVariable("?T")).UnionWithNext())
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred3"), new RDFVariable("?T")).Optional())
        .UnionWithNext())
      .AddPatternGroup(new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .UnionWithNext()); //UnionWithNext will not be printed, since this is the last evaluable query member
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE rdfs:label <ex:org> _:12345 ?S
      WHERE {
        {
          {
            { ?S rdfs:label "label"@EN }
            UNION
            { ?S rdfs:comment "comment" }
          }
          UNION
          {
            { <ex:subj> <ex:pred> ?T }
            UNION
            { <ex:subj> <ex:pred2> ?T }
            UNION
            { OPTIONAL { <ex:subj> <ex:pred3> ?T } }
          }
          UNION
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndStarSubQuery()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE ?S
      WHERE {
        {
          SELECT *
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndProjectionSubQuery()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .AddProjectionVariable(new RDFVariable("?S")));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      DESCRIBE ?S
      WHERE {
        {
          SELECT ?S
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndOptionalProjectionSubQuery()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?T"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional()))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      DESCRIBE ?T
      WHERE {
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              OPTIONAL { ?S rdfs:label "label"@EN } .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleSubQueries()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      DESCRIBE ?S
      WHERE {
        {
          SELECT ?Z
          WHERE {
            {
              ?S rdfs:label _:12345 .
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleOptionalSubQueries()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?S"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      DESCRIBE ?S
      WHERE {
        {
          SELECT ?Z
          WHERE {
            {
              ?S rdfs:label _:12345 .
            }
          }
        }
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleUnionSubQueries()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFResource("ex:org"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      DESCRIBE <ex:org>
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithTermsAndMultipleOptionalAndUnionSubQueries()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?T"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .Optional()
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      DESCRIBE ?T
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?T
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
          }
        }
        OPTIONAL {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintComplexDescribeQuery()
  {
    RDFDescribeQuery query = new RDFDescribeQuery()
      .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
      .AddDescribeTerm(new RDFVariable("?T"))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
            .AddFilter(new RDFExpressionFilter(new RDFBoundExpression(new RDFVariable("?S")))))
          .AddModifier(new RDFDistinctModifier())
          .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
          .AddModifier(new RDFLimitModifier(5))
          .AddModifier(new RDFOffsetModifier(1)))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345"))))
        .UnionWithNext()
        .AddProjectionVariable(new RDFVariable("?Z")))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddModifier(new RDFGroupByModifier([new RDFVariable("?S")])
          .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
            .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))))
        .UnionWithNext())
      .AddSubQuery(new RDFSelectQuery()
        .AddSubQuery(new RDFSelectQuery()
          .AddPatternGroup(new RDFPatternGroup()
            .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), [new RDFResource("ex:org")]))
            .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
              .AddAlternativeSteps([
                new RDFPropertyPathStep(RDFVocabulary.RDFS.CLASS),
                new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                new RDFPropertyPathStep(RDFVocabulary.OWL.CLASS).Inverse()
              ])))
          .AddProjectionVariable(new RDFVariable("?START"))))
      .AddSubQuery(new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
        .AddPatternGroup(new RDFPatternGroup()
          .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en"))))
        .AddProjectionVariable(new RDFVariable("?T")));
    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
      PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>
      PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

      DESCRIBE ?T
      WHERE {
        {
          {
            SELECT ?Z
            WHERE {
              {
                SELECT DISTINCT *
                WHERE {
                  {
                    ?S rdfs:label _:12345 .
                    FILTER ( (BOUND(?S)) ) 
                  }
                }
                ORDER BY ASC(?S)
                LIMIT 5
                OFFSET 1
              }
              {
                ?S rdfs:label _:12345 .
              }
            }
          }
          UNION
          {
            SELECT ?S (AVG(?S) AS ?AVG_S)
            WHERE {
              {
                ?S rdfs:label "label"@EN .
              }
            }
            GROUP BY ?S
            HAVING ((AVG(?S) >= "11.44"^^xsd:float))
          }
          UNION
          {
            SELECT *
            WHERE {
              {
                SELECT ?START
                WHERE {
                  {
                    VALUES ?S { <ex:org> } .
                    ?START (rdfs:Class|rdfs:label|^<http://www.w3.org/2002/07/owl#Class>) ?END .
                  }
                }
              }
            }
          }
        }
        {
          SELECT ?T
          WHERE {
            {
              ?S rdfs:label "label"@EN .
            }
          }
        }
      }
      """;
    Assert.IsTrue(string.Equals(queryString, expectedQueryString, StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintPatternMemberNull()
    => Assert.IsNull(RDFQueryPrinter.PrintPatternMember(null, null));

  [TestMethod]
  public void ShouldPrintPatternMemberVariable()
    => Assert.IsTrue(string.Equals("?S", RDFQueryPrinter.PrintPatternMember(new RDFVariable("?S"), null), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberResourceBlank()
    => Assert.IsTrue(string.Equals("_:12345", RDFQueryPrinter.PrintPatternMember(new RDFResource("bnode:12345"), null), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberResourceNotBlankUnresolved()
    => Assert.IsTrue(string.Equals("<http://example.org/test>", RDFQueryPrinter.PrintPatternMember(new RDFResource("http://example.org/test"), null), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberResourceNotBlankResolved()
    => Assert.IsTrue(string.Equals("ex:test", RDFQueryPrinter.PrintPatternMember(new RDFResource("http://example.org/test"), [new RDFNamespace("ex", "http://example.org/")]), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberContextUnresolved()
    => Assert.IsTrue(string.Equals("<http://example.org/test>", RDFQueryPrinter.PrintPatternMember(new RDFContext(new Uri("http://example.org/test")), null), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberContextResolved()
    => Assert.IsTrue(string.Equals("ex:test", RDFQueryPrinter.PrintPatternMember(new RDFContext(new Uri("http://example.org/test")), [new RDFNamespace("ex", "http://example.org/")]), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberPlainLiteral()
    => Assert.IsTrue(string.Equals("\"hello\"", RDFQueryPrinter.PrintPatternMember(new RDFPlainLiteral("hello"), null), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberPlainLiteralWithLanguage()
    => Assert.IsTrue(string.Equals("\"hello\"@EN-US", RDFQueryPrinter.PrintPatternMember(new RDFPlainLiteral("hello", "en-US"), null), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberTypedLiteralUnresolved()
    => Assert.IsTrue(string.Equals("\"hello\"^^<http://www.w3.org/2001/XMLSchema#string>", RDFQueryPrinter.PrintPatternMember(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING), null), StringComparison.Ordinal));

  [TestMethod]
  public void ShouldPrintPatternMemberTypedLiteralResolved()
    => Assert.IsTrue(string.Equals("\"hello\"^^xsd:string", RDFQueryPrinter.PrintPatternMember(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING), [RDFNamespaceRegister.GetByPrefix("xsd")]), StringComparison.Ordinal));
  #endregion
}