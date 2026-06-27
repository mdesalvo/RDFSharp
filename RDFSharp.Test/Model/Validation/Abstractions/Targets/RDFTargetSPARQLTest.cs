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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Model;

[TestClass]
public class RDFTargetSPARQLTest
{
    #region Test
    [TestMethod]
    public void ShouldCreateTargetSPARQL()
    {
        RDFSelectQuery selectQuery = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT ?THIS WHERE { ?THIS a ex:Person }");
        RDFTargetSPARQL targetSPARQL = new RDFTargetSPARQL(selectQuery);

        Assert.IsNotNull(targetSPARQL);
        Assert.IsNotNull(targetSPARQL.SelectQuery);
        Assert.IsTrue(targetSPARQL.SelectQuery.Equals(selectQuery));
        Assert.IsNotNull(targetSPARQL.TargetValue);
        Assert.IsTrue(targetSPARQL.TargetValue.IsBlank);
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingTargetSPARQLBecauseNullValue()
        => Assert.ThrowsExactly<RDFModelException>(() => _ = new RDFTargetSPARQL(null));

    [TestMethod]
    public void ShouldExportTargetSPARQL()
    {
        RDFSelectQuery selectQuery = RDFSelectQuery.FromString(
            "PREFIX ex: <http://example.org/> SELECT ?THIS WHERE { ?THIS a ex:Person }");
        RDFTargetSPARQL targetSPARQL = new RDFTargetSPARQL(selectQuery);
        RDFGraph graph = targetSPARQL.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:nodeShape")));

        Assert.IsNotNull(graph);
        Assert.AreEqual(3, graph.TriplesCount);
        //shape sh:target _:t
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("ex:nodeShape"), RDFVocabulary.SHACL.TARGET, targetSPARQL.TargetValue)));
        //_:t rdf:type sh:SPARQLTarget
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(targetSPARQL.TargetValue, RDFVocabulary.RDF.TYPE, RDFVocabulary.SHACL.SPARQL_TARGET)));
        //_:t sh:select "<query>"^^xsd:string
        Assert.IsTrue(graph.SelectTriples(s: targetSPARQL.TargetValue, p: RDFVocabulary.SHACL.SELECT).Single()
                           .Object is RDFTypedLiteral selectLiteral
                      && selectLiteral.Datatype.ToString().Equals(RDFVocabulary.XSD.STRING.ToString())
                      && selectLiteral.Value.Equals(selectQuery.ToString()));
    }

    [TestMethod]
    public void ShouldExportTargetSPARQLWithNullShape()
    {
        RDFSelectQuery selectQuery = RDFSelectQuery.FromString("SELECT ?THIS WHERE { ?THIS ?P ?O }");
        RDFTargetSPARQL targetSPARQL = new RDFTargetSPARQL(selectQuery);
        RDFGraph graph = targetSPARQL.ToRDFGraph(null);

        Assert.IsNotNull(graph);
        Assert.AreEqual(0, graph.TriplesCount);
    }
    #endregion
}