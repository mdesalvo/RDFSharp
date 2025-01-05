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

using RDFSharp.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFMaxLengthFacetTest
    {
        #region Tests
        [TestMethod]
        
        public void ShouldCreateMaxLengthFacet()
        {
            RDFMaxLengthFacet facet = new RDFMaxLengthFacet(6);

            Assert.IsNotNull(facet);
            Assert.IsTrue(facet.Length == 6);
            Assert.IsTrue(facet.URI.IsBlank);
        }

        [TestMethod]
        
        public void ShouldValidateMaxLengthFacet()
        {
            RDFMaxLengthFacet facet = new RDFMaxLengthFacet(6);

            Assert.IsTrue(facet.Validate("abcdef"));
            Assert.IsTrue(facet.Validate(null));
            Assert.IsTrue(facet.Validate(string.Empty));
            Assert.IsTrue(facet.Validate("a"));
            Assert.IsFalse(facet.Validate("abcdefgh"));

            RDFMaxLengthFacet facet0 = new RDFMaxLengthFacet(0);
            Assert.IsFalse(facet0.Validate("abcdef"));
            Assert.IsTrue(facet0.Validate(null));
            Assert.IsTrue(facet0.Validate(string.Empty));
            Assert.IsFalse(facet0.Validate("a"));
            Assert.IsFalse(facet0.Validate("abcdefgh"));
        }

        [TestMethod]
        
        public void ShouldConvertMaxLengthFacetToGraph()
        {
            RDFMaxLengthFacet facet = new RDFMaxLengthFacet(6);
            RDFGraph graph = facet.ToRDFGraph();

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.XSD.MAX_LENGTH));
            Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("6", RDFDatatypeRegister.GetDatatype(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString()))));
        }
        #endregion
    }
}