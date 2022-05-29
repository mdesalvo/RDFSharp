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
        #endregion
    }
}