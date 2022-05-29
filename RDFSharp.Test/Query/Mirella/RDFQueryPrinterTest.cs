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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
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
@"SELECT *
WHERE {
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarPrefixed()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  {
    ?S rdfs:label ""label""@EN .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarUnprefixed()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString =
@"SELECT *
WHERE {
  {
    ?S <http://www.w3.org/2000/01/rdf-schema#label> ""label""@EN .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithOptionalPattern()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  {
    OPTIONAL { ?S rdfs:label ""label""@EN } .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithOptionalPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                    .Optional()
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  OPTIONAL {
    {
      ?S rdfs:label ""label""@EN .
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithOptionalPatternAndOptionalPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
                    .Optional()
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  OPTIONAL {
    {
      OPTIONAL { ?S rdfs:label ""label""@EN } .
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithSingleUnionPattern()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString =
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  {
    ?S rdfs:label ""label""@EN .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithSingleUnionPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                    .UnionWithNext()
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString =
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  {
    ?S rdfs:label ""label""@EN .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }


        [TestMethod]
        public void ShouldPrintSelectQueryStarWithSingleUnionPatternAndSingleUnionPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
                    .UnionWithNext()
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString =
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  {
    ?S rdfs:label ""label""@EN .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithEmptyPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString =
@"SELECT *
WHERE {
  {
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithEmptyOptionalPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1").Optional());
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString =
@"SELECT *
WHERE {
  OPTIONAL {
    {
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithEmptySingleUnionPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1").UnionWithNext());
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString =
@"SELECT *
WHERE {
  {
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixed()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                )
                .AddProjectionVariable(new RDFVariable("?S"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S
WHERE {
  {
    ?S rdfs:label ""label""@EN .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionUnprefixed()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                )
                .AddProjectionVariable(new RDFVariable("?S"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"SELECT ?S
WHERE {
  {
    ?S <http://www.w3.org/2000/01/rdf-schema#label> ""label""@EN .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndOptionalPattern()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).Optional())
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S ?T
WHERE {
  {
    ?S rdfs:label ""label""@EN .
    OPTIONAL { <ex:subj> <ex:pred> ?T } .
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndOptionalPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
                    .Optional()
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S ?T
WHERE {
  {
    ?S rdfs:label ""label""@EN .
  }
  OPTIONAL {
    {
      <ex:subj> <ex:pred> ?T .
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndOptionalPatternAndOptionalPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj1"), new RDFResource("ex:pred1"), new RDFVariable("?T")))
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj2"), new RDFResource("ex:pred2"), new RDFVariable("?T")).Optional())
                    .Optional()
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S ?T
WHERE {
  {
    ?S rdfs:label ""label""@EN .
  }
  OPTIONAL {
    {
      <ex:subj1> <ex:pred1> ?T .
      OPTIONAL { <ex:subj2> <ex:pred2> ?T } .
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndUnionPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S ?T
WHERE {
  {
    {
      ?S rdfs:label ""label""@EN .
    }
    UNION
    {
      <ex:subj> <ex:pred> ?T .
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG3")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
                    .UnionWithNext() //Will not be printed, since this is the last evaluable query member
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S ?T
WHERE {
  {
    {
      ?S rdfs:label ""label""@EN .
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
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupFollowedByPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG3")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
                )
                .AddPatternGroup(new RDFPatternGroup("PG4")
                    .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S ?T
WHERE {
  {
    {
      ?S rdfs:label ""label""@EN .
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
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupFollowedByOptionalPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG3")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
                )
                .AddPatternGroup(new RDFPatternGroup("PG4")
                    .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
                    .Optional()
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S ?T
WHERE {
  {
    {
      ?S rdfs:label ""label""@EN .
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
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupHavingOptionalPatternsAndFollowedByOptionalPatternGroup()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).Optional())
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")))
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("?P"), new RDFResource("bnode:12345")).Optional())
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), new RDFVariable("?P"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional())
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG3")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
                )
                .AddPatternGroup(new RDFPatternGroup("PG4")
                    .AddPattern(new RDFPattern(new RDFResource("bnode:12345"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
                    .Optional()
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX xsd: <http://www.w3.org/2001/XMLSchema#>

SELECT ?S ?T
WHERE {
  {
    {
      ?S rdfs:label ""label""@EN .
      OPTIONAL { ?S rdfs:comment ""comment"" } .
    }
    UNION
    {
      <ex:subj> <ex:pred> ?T .
      OPTIONAL { <ex:subj> ?P _:12345 } .
      OPTIONAL { ?S ?P ""25""^^xsd:integer } .
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
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionPrefixedAndMultipleUnionPatternGroupHavingMultipleUnionPatterns()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.COMMENT, new RDFPlainLiteral("comment")).UnionWithNext()) //Union will not be printed, since this is the last pattern group member
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG2")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("?T")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred2"), new RDFVariable("?T")).UnionWithNext())
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred3"), new RDFVariable("?T")).Optional())
                    .UnionWithNext()
                )
                .AddPatternGroup(new RDFPatternGroup("PG3")
                    .AddPattern(new RDFPattern(new RDFResource("ex:subj"), RDFVocabulary.RDFS.LABEL, new RDFVariable("?T")))
                    .UnionWithNext() //Union will not be printed, since this is the last evaluable query member
                )
                .AddProjectionVariable(new RDFVariable("?S"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S ?T
WHERE {
  {
    {
      { ?S rdfs:label ""label""@EN }
      UNION
      { ?S rdfs:comment ""comment"" }
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
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
        }
        #endregion
    }
}