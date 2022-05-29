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
using System.Collections.Generic;
using System.Linq;
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
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
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithStarSubQuery()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
                  )
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  {
    SELECT *
    WHERE {
      {
        OPTIONAL { ?S rdfs:label ""label""@EN } .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryStarWithProjectonSubQuery()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
                  )
                  .AddProjectionVariable(new RDFVariable("?S"))
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT *
WHERE {
  {
    SELECT ?S
    WHERE {
      {
        OPTIONAL { ?S rdfs:label ""label""@EN } .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionWithStarSubQuery()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
                  )
                )
                .AddProjectionVariable(new RDFVariable("?S"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?S
WHERE {
  {
    SELECT *
    WHERE {
      {
        OPTIONAL { ?S rdfs:label ""label""@EN } .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionWithProjectionSubQuery()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
                  )
                  .AddProjectionVariable(new RDFVariable("?T"))
                )
                .AddProjectionVariable(new RDFVariable("?S"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?S
WHERE {
  {
    SELECT ?T
    WHERE {
      {
        OPTIONAL { ?S rdfs:label ""label""@EN } .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionWithOptionalSubQuery()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")).Optional())
                  )
                  .Optional()
                  .AddProjectionVariable(new RDFVariable("?T"))
                )
                .AddProjectionVariable(new RDFVariable("?S"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?S
WHERE {
  OPTIONAL {
    SELECT ?T
    WHERE {
      {
        OPTIONAL { ?S rdfs:label ""label""@EN } .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionWithMultipleSubQueries()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
                  )
                  .AddProjectionVariable(new RDFVariable("?Z"))
                )
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                  )
                  .AddProjectionVariable(new RDFVariable("?T"))
                )
                .AddProjectionVariable(new RDFVariable("?Z"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
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
        ?S rdfs:label ""label""@EN .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionWithMultipleOptionalSubQueries()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
                  )
                  .AddProjectionVariable(new RDFVariable("?Z"))
                )
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                  )
                  .Optional()
                  .AddProjectionVariable(new RDFVariable("?T"))
                )
                .AddProjectionVariable(new RDFVariable("?Z"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
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
        ?S rdfs:label ""label""@EN .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionWithMultipleUnionSubQueries()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
                  )
                  .UnionWithNext()
                  .AddProjectionVariable(new RDFVariable("?Z"))
                )
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                  )
                  .AddProjectionVariable(new RDFVariable("?T"))
                )
                .AddProjectionVariable(new RDFVariable("?Z"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
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
          ?S rdfs:label ""label""@EN .
        }
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintSelectQueryProjectionWithMultipleOptionalAndUnionSubQueries()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
                  )
                  .UnionWithNext()
                  .AddProjectionVariable(new RDFVariable("?Z"))
                )
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                  )
                  .AddProjectionVariable(new RDFVariable("?T"))
                )
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                  )
                  .Optional()
                  .AddProjectionVariable(new RDFVariable("?T"))
                )
                .AddProjectionVariable(new RDFVariable("?Z"))
                .AddProjectionVariable(new RDFVariable("?T"));
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
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
          ?S rdfs:label ""label""@EN .
        }
      }
    }
  }
  OPTIONAL {
    SELECT ?T
    WHERE {
      {
        ?S rdfs:label ""label""@EN .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }

        [TestMethod]
        public void ShouldPrintComplexSelectQuery()
        {
            RDFSelectQuery query = new RDFSelectQuery()
                .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdfs"))
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("xsd"))
                  .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup("PG1")
                      .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
                      .AddFilter(new RDFBoundFilter(new RDFVariable("?S")))
                    )
                    .AddModifier(new RDFDistinctModifier())
                    .AddModifier(new RDFOrderByModifier(new RDFVariable("?S"), RDFQueryEnums.RDFOrderByFlavors.ASC))
                    .AddModifier(new RDFLimitModifier(5))
                    .AddModifier(new RDFOffsetModifier(1))
                  )
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFResource("bnode:12345")))
                  )
                  .UnionWithNext()
                  .AddProjectionVariable(new RDFVariable("?Z"))
                )
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                  )
                  .AddModifier(new RDFGroupByModifier(new List<RDFVariable>() { new RDFVariable("?S") })
                    .AddAggregator(new RDFAvgAggregator(new RDFVariable("?S"), new RDFVariable("?AVG_S"))
                      .SetHavingClause(RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan, new RDFTypedLiteral("11.44", RDFModelEnums.RDFDatatypes.XSD_FLOAT))
                    )
                  )
                  .UnionWithNext()
                )
                .AddSubQuery(new RDFSelectQuery()
                  .AddSubQuery(new RDFSelectQuery()
                    .AddPatternGroup(new RDFPatternGroup("PG1")
                      .AddValues(new RDFValues().AddColumn(new RDFVariable("?S"), new List<RDFPatternMember>() { new RDFResource("ex:org") }))
                      .AddPropertyPath(new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
                        .AddAlternativeSteps(new List<RDFPropertyPathStep>() {
                          new RDFPropertyPathStep(RDFVocabulary.RDFS.CLASS),
                          new RDFPropertyPathStep(RDFVocabulary.RDFS.LABEL),
                          new RDFPropertyPathStep(RDFVocabulary.OWL.CLASS).Inverse()
                        })
                      )
                    )
                    .AddProjectionVariable(new RDFVariable("?START"))
                  )
                )
                .AddSubQuery(new RDFSelectQuery()
                  .AddPrefix(RDFNamespaceRegister.GetByPrefix("rdf"))
                  .AddPatternGroup(new RDFPatternGroup("PG1")
                    .AddPattern(new RDFPattern(new RDFVariable("?S"), RDFVocabulary.RDFS.LABEL, new RDFPlainLiteral("label","en")))
                  )
                  .AddProjectionVariable(new RDFVariable("?T"))
                );
            string queryString = RDFQueryPrinter.PrintSelectQuery(query, 0, false);
            string expectedQueryString = 
@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
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
              FILTER ( BOUND(?S) ) 
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
          ?S rdfs:label ""label""@EN .
        }
      }
      GROUP BY ?S
      HAVING ((AVG(?S) >= ""11.44""^^xsd:float))
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
        ?S rdfs:label ""label""@EN .
      }
    }
  }
}
";
            Assert.IsTrue(string.Equals(queryString, expectedQueryString));
            Assert.IsTrue(queryString.Count(chr => chr == '{') == queryString.Count(chr => chr == '}'));
        }
        #endregion
    }
}