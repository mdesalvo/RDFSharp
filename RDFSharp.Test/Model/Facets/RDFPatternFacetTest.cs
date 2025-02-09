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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFPatternFacetTest
    {
        #region Tests
        [TestMethod]
        
        public void ShouldCreatePatternFacet()
        {
            RDFPatternFacet facet = new RDFPatternFacet("^ex");

            Assert.IsNotNull(facet);
            Assert.IsTrue(facet.Pattern == "^ex");
            Assert.IsTrue(facet.URI.IsBlank);
        }

        [TestMethod]
        
        public void ShouldValidatePatternFacet()
        {
            RDFPatternFacet facet = new RDFPatternFacet("^ex");

            Assert.IsTrue(facet.Validate("example"));
            Assert.IsFalse(facet.Validate(null));
            Assert.IsFalse(facet.Validate(string.Empty));
            Assert.IsFalse(facet.Validate("a"));

            RDFPatternFacet facet0 = new RDFPatternFacet(null);
            Assert.IsTrue(facet0.Validate("abcdef"));
            Assert.IsTrue(facet0.Validate(null));
            Assert.IsTrue(facet0.Validate(string.Empty));
        }

        [TestMethod]
        
        public void ShouldConvertPatternFacetToGraph()
        {
            RDFPatternFacet facet = new RDFPatternFacet("^ex");
            RDFGraph graph = facet.ToRDFGraph();

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.XSD.PATTERN));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("^ex", RDFDatatypeRegister.GetDatatype(RDFVocabulary.XSD.STRING.ToString()))));
        }
        #endregion
    }
}