/*
   Copyright 2012-2024 Marco De Salvo

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
    public class RDFFractionDigitsFacetTest
    {
        #region Tests
        [TestMethod]
        
        public void ShouldCreateFractionDigitsFacet()
        {
            RDFFractionDigitsFacet facet = new RDFFractionDigitsFacet(2);

            Assert.IsNotNull(facet);
            Assert.IsTrue(facet.AllowedDigits == 2);
            Assert.IsTrue(facet.URI.IsBlank);
        }

		[TestMethod]
        
        public void ShouldValidateFractionDigitsFacet()
        {
            RDFFractionDigitsFacet facet = new RDFFractionDigitsFacet(2);

            Assert.IsTrue(facet.Validate("-2.00"));
            Assert.IsTrue(facet.Validate("-2.77"));
            Assert.IsTrue(facet.Validate("-2"));
            Assert.IsTrue(facet.Validate("2"));
            Assert.IsTrue(facet.Validate("2.00"));
            Assert.IsTrue(facet.Validate("2.77"));
            Assert.IsFalse(facet.Validate(null));
            Assert.IsFalse(facet.Validate(string.Empty));
            Assert.IsFalse(facet.Validate("2.000"));
            Assert.IsFalse(facet.Validate("2.5773"));
            Assert.IsFalse(facet.Validate("-2.009"));
            Assert.IsFalse(facet.Validate("abcdefgh"));

            RDFFractionDigitsFacet facet0 = new RDFFractionDigitsFacet(0);
            Assert.IsFalse(facet0.Validate("-2.00"));
            Assert.IsFalse(facet0.Validate("-2.77"));
            Assert.IsTrue(facet0.Validate("-2"));
            Assert.IsTrue(facet0.Validate("2"));
            Assert.IsFalse(facet0.Validate("2.00"));
            Assert.IsFalse(facet0.Validate("2.77"));
            Assert.IsFalse(facet0.Validate(null));
            Assert.IsFalse(facet0.Validate(string.Empty));
            Assert.IsFalse(facet0.Validate("2.000"));
            Assert.IsFalse(facet0.Validate("2.5773"));
            Assert.IsFalse(facet0.Validate("-2.009"));
            Assert.IsFalse(facet0.Validate("abcdefgh"));
        }

		[TestMethod]
        
        public void ShouldConvertFractionDigitsFacetToGraph()
        {
            RDFFractionDigitsFacet facet = new RDFFractionDigitsFacet(2);
			RDFGraph graph = facet.ToRDFGraph();

			Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
			Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.XSD.FRACTION_DIGITS));
			Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("2", RDFDatatypeRegister.GetDatatype(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString()))));
        }
		#endregion
    }
}