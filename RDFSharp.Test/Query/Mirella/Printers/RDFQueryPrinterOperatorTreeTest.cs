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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFQueryPrinterOperatorTreeTest
{
  #region Tests

  #region Query-level operator trees (between pattern groups / subqueries)
  [TestMethod]
  public void ShouldPrintSelectQueryWithUnionOperatorTree()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddOperator(pgA.Union(pgB));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
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
            <ex:subj> <ex:pred> ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryWithMinusOperatorTree()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddOperator(pgA.Minus(pgB));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
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
            <ex:subj> <ex:pred> ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryWithNestedUnionMinusOperatorTree()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")));
    RDFPatternGroup pgC = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")));

    // (A UNION B) MINUS C
    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddOperator(pgA.Union(pgB).Minus(pgC));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            {
              ?S rdfs:label "label"@EN .
            }
            UNION
            {
              <ex:subj> <ex:pred> ?T .
            }
          }
          MINUS
          {
            <ex:subj> rdfs:label ?T .
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryWithNestedRightSubtreeOperatorTree()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")));
    RDFPatternGroup pgC = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")));

    // A UNION (B MINUS C)
    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddOperator(pgA.Union(pgB.Minus(pgC)));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
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
            {
              <ex:subj> <ex:pred> ?T .
            }
            MINUS
            {
              <ex:subj> rdfs:label ?T .
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryWithUnionOperatorTreeWithSubquery()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));

    RDFSelectQuery subQuery = new RDFSelectQuery()
        .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label2", "en"))))
        .AddProjectionVariable(new RDFVariable("?S"));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddOperator(pgA.Union(subQuery));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
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
            SELECT ?S
            WHERE {
              {
                ?S rdfs:label "label2"@EN .
              }
            }
          }
        }
      }

      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryWithOptionalOperatorTree()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddOperator(pgA.Union(pgB).Optional());

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        OPTIONAL {
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
      }

      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintSelectQueryWithUnionOperatorTreeWithServicePatternGroup()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org1")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, RDFVocabulary.RDFS.CLASS))
        .AsService(new RDFSPARQLEndpoint(new Uri("ex:org2")));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddOperator(pgA.Union(pgB));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
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
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }
  #endregion

  #region Pattern-group-level operator trees (between patterns / property paths)
  [TestMethod]
  public void ShouldPrintPatternGroupWithUnionOperatorTree()
  {
    RDFPattern pA = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"));
    RDFPattern pB = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?E"));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddPatternGroup(new RDFPatternGroup()
            .AddOperator(pA.Union(pB)));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
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
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintPatternGroupWithMinusOperatorTree()
  {
    RDFPattern pA = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"));
    RDFPattern pB = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?E"));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddPatternGroup(new RDFPatternGroup()
            .AddOperator(pA.Minus(pB)));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
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
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintPatternGroupWithNestedOperatorTree()
  {
    RDFPattern pA = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"));
    RDFPattern pB = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label2", "en"));
    RDFPattern pC = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment", "en"));

    // (A UNION B) MINUS C
    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddPatternGroup(new RDFPatternGroup()
            .AddOperator(pA.Union(pB).Minus(pC)));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          {
            { ?S rdfs:label "label"@EN }
            UNION
            { ?S rdfs:label "label2"@EN }
          }
          MINUS
          { ?S rdfs:comment "comment"@EN }
        }
      }

      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintPatternGroupWithUnionPatternAndPropertyPath()
  {
    RDFPattern pA = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"));
    RDFPropertyPath ppB = new RDFPropertyPath(new RDFVariable("?S"), new RDFVariable("?E"))
        .AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddPatternGroup(new RDFPatternGroup()
            .AddOperator(pA.Union(ppB)));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
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
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintPatternGroupWithOperatorTreeAlongsideNormalPattern()
  {
    RDFPattern normalPattern = new RDFPattern(new RDFVariable("?X"), RDFVocabulary.RDFS.LABEL, RDFVocabulary.RDFS.CLASS);
    RDFPattern pA = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en"));
    RDFPattern pB = new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?E"));

    RDFSelectQuery query = new RDFSelectQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddPatternGroup(new RDFPatternGroup()
            .AddPattern(normalPattern)
            .AddOperator(pA.Union(pB)));

    string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
    const string expectedQueryString =
      """
      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

      SELECT *
      WHERE {
        {
          ?X rdfs:label rdfs:Class .
          { ?S rdfs:label "label"@EN }
          UNION
          { ?S rdfs:label ?E }
        }
      }

      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }
  #endregion

  #region Non-SELECT query types with operator trees
  [TestMethod]
  public void ShouldPrintAskQueryWithOperatorTree()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")));

    RDFAskQuery query = new RDFAskQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddOperator(pgA.Union(pgB));

    string queryString = RDFQueryPrinter.PrintAskQuery(query);
    const string expectedQueryString =
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
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintConstructQueryWithOperatorTree()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")));

    RDFConstructQuery query = new RDFConstructQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddTemplate(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
        .AddOperator(pgA.Union(pgB));

    string queryString = RDFQueryPrinter.PrintConstructQuery(query);
    const string expectedQueryString =
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
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }

  [TestMethod]
  public void ShouldPrintDescribeQueryWithOperatorTree()
  {
    RDFPatternGroup pgA = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label", "en")));
    RDFPatternGroup pgB = new RDFPatternGroup()
        .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")));

    RDFDescribeQuery query = new RDFDescribeQuery()
        .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
        .AddDescribeTerm(new RDFVariable("?S"))
        .AddOperator(pgA.Union(pgB));

    string queryString = RDFQueryPrinter.PrintDescribeQuery(query);
    const string expectedQueryString =
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
        }
      }
      """;
    Assert.IsTrue(string.Equals(RDFTestUtilities.NormalizeEOL(queryString), RDFTestUtilities.NormalizeEOL(expectedQueryString), StringComparison.Ordinal));
    Assert.AreEqual(queryString.Count(chr => chr == '}'), queryString.Count(chr => chr == '{'));
  }
  #endregion

  #endregion
}
