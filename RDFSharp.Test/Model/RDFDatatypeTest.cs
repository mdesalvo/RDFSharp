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
using RDFSharp.Model;
using System;
using System.Linq;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFDatatypeTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateFacetedDatatype()
    {
        RDFDatatype length6 = new RDFDatatype(new Uri("ex:length6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFLengthFacet(6) ]);

        Assert.IsNotNull(length6);
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, length6.TargetDatatype);
        Assert.IsTrue(length6.URI.Equals(new Uri("ex:length6")));
        Assert.IsTrue(length6.Facets.Single() is RDFLengthFacet { Length: 6 });
        Assert.IsTrue(string.Equals(length6.ToString(), "ex:length6", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldConvertFacetedDatatypeToGraph()
    {
        RDFDatatype length6 = new RDFDatatype(new Uri("ex:length6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFLengthFacet(6) ]);
        RDFGraph length6Graph = length6.ToRDFGraph();

        Assert.IsNotNull(length6Graph);
        Assert.AreEqual(7, length6Graph.TriplesCount);
        Assert.AreEqual(1, length6Graph[new RDFResource("ex:length6"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.DATATYPE, null].TriplesCount);
        Assert.AreEqual(1, length6Graph[new RDFResource("ex:length6"), RDFVocabulary.OWL.WITH_RESTRICTIONS, null, null].TriplesCount);
        Assert.AreEqual(1, length6Graph[new RDFResource("ex:length6"), RDFVocabulary.OWL.ON_DATATYPE, RDFVocabulary.XSD.STRING, null].TriplesCount);
        Assert.AreEqual(1, length6Graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST, null].TriplesCount);
        Assert.AreEqual(1, length6Graph[null, RDFVocabulary.RDF.FIRST, null, null].TriplesCount);
        Assert.AreEqual(1, length6Graph[null, RDFVocabulary.XSD.LENGTH, null, new RDFTypedLiteral("6", RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)].TriplesCount);
        Assert.AreEqual(1, length6Graph[null, RDFVocabulary.RDF.REST, null, null].TriplesCount);
    }

    [TestMethod]
    public void ShouldValidateFacetedDatatype()
    {
        RDFDatatype length6 = new RDFDatatype(new Uri("ex:length6"), RDFModelEnums.RDFDatatypes.XSD_STRING, [
            new RDFLengthFacet(6) ]);

        Assert.IsTrue(length6.Validate("123456").Item1);
        Assert.IsFalse(length6.Validate("1234567").Item1);
    }

    [TestMethod]
    public void ShouldCreateAliasDatatype()
    {
        RDFDatatype exString = new RDFDatatype(new Uri("ex:string"), RDFModelEnums.RDFDatatypes.XSD_STRING, []);

        Assert.IsNotNull(exString);
        Assert.AreEqual(RDFModelEnums.RDFDatatypes.XSD_STRING, exString.TargetDatatype);
        Assert.IsTrue(exString.URI.Equals(new Uri("ex:string")));
        Assert.IsEmpty(exString.Facets);
    }

    [TestMethod]
    public void ShouldConvertAliasDatatypeToGraph()
    {
        RDFDatatype exString = new RDFDatatype(new Uri("ex:string"), RDFModelEnums.RDFDatatypes.XSD_STRING, []);
        RDFGraph exStringGraph = exString.ToRDFGraph();

        Assert.IsNotNull(exStringGraph);
        Assert.AreEqual(2, exStringGraph.TriplesCount);
        Assert.AreEqual(1, exStringGraph[new RDFResource("ex:string"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.DATATYPE, null].TriplesCount);
        Assert.AreEqual(1, exStringGraph[new RDFResource("ex:string"), RDFVocabulary.OWL.EQUIVALENT_CLASS, RDFVocabulary.XSD.STRING, null].TriplesCount);
    }

    [TestMethod]
    public void ShouldValidateAliasDatatype()
    {
        RDFDatatype exString = new RDFDatatype(new Uri("ex:string"), RDFModelEnums.RDFDatatypes.XSD_STRING, []);

        Assert.IsTrue(exString.Validate("hello").Item1);
    }
    #endregion
}