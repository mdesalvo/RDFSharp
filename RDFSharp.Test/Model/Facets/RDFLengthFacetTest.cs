﻿/*
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
public class RDFLengthFacetTest
{
    #region Tests
    [TestMethod]

    public void ShouldCreateLengthFacet()
    {
        RDFLengthFacet facet = new RDFLengthFacet(6);

        Assert.IsNotNull(facet);
        Assert.AreEqual(6u, facet.Length);
        Assert.IsTrue(facet.URI.IsBlank);
    }

    [TestMethod]

    public void ShouldValidateLengthFacet()
    {
        RDFLengthFacet facet = new RDFLengthFacet(6);

        Assert.IsTrue(facet.Validate("abcdef"));
        Assert.IsFalse(facet.Validate(null));
        Assert.IsFalse(facet.Validate(string.Empty));
        Assert.IsFalse(facet.Validate("a"));
        Assert.IsFalse(facet.Validate("abcdefgh"));

        RDFLengthFacet facet0 = new RDFLengthFacet(0);
        Assert.IsFalse(facet0.Validate("abcdef"));
        Assert.IsTrue(facet0.Validate(null));
        Assert.IsTrue(facet0.Validate(string.Empty));
        Assert.IsFalse(facet0.Validate("a"));
    }

    [TestMethod]

    public void ShouldConvertLengthFacetToGraph()
    {
        RDFLengthFacet facet = new RDFLengthFacet(6);
        RDFGraph graph = facet.ToRDFGraph();

        Assert.IsNotNull(graph);
        Assert.AreEqual(1, graph.TriplesCount);
        Assert.IsTrue(graph.Single().Predicate.Equals(RDFVocabulary.XSD.LENGTH));
        Assert.IsTrue(graph.Single().Object.Equals(new RDFTypedLiteral("6", RDFDatatypeRegister.GetDatatype(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString()))));
    }
    #endregion
}