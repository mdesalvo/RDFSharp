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

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFMaxExclusiveFacetTest
{
    #region Tests
    [TestMethod]
        
    public void ShouldCreateMaxExclusiveFacet()
    {
        RDFMaxExclusiveFacet facet = new RDFMaxExclusiveFacet(6.145);

        Assert.IsNotNull(facet);
        Assert.AreEqual(6.145d, facet.ExclusiveUpperBound);
        Assert.IsTrue(facet.URI.IsBlank);
    }

    [TestMethod]
        
    public void ShouldValidateMaxExclusiveFacet()
    {
        RDFMaxExclusiveFacet facet = new RDFMaxExclusiveFacet(6);

        Assert.IsTrue(facet.Validate("-2.0089"));
        Assert.IsTrue(facet.Validate("2.047"));
        Assert.IsFalse(facet.Validate(null));
        Assert.IsFalse(facet.Validate(string.Empty));
        Assert.IsFalse(facet.Validate("14.5773"));
        Assert.IsFalse(facet.Validate("6"));
        Assert.IsFalse(facet.Validate("abcdefgh"));

        RDFMaxExclusiveFacet facet0 = new RDFMaxExclusiveFacet(-12.45);
        Assert.IsTrue(facet0.Validate("-16.2442"));
        Assert.IsFalse(facet0.Validate("-12.00"));
    }

    [TestMethod]
        
    public void ShouldConvertMaxExclusiveFacetToGraph()
    {
        RDFMaxExclusiveFacet facet = new RDFMaxExclusiveFacet(6);
        RDFGraph graph = facet.ToRDFGraph();

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.XSD.MAX_EXCLUSIVE));
        Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("6", RDFDatatypeRegister.GetDatatype(RDFVocabulary.XSD.DOUBLE.ToString()))));
    }
    #endregion
}