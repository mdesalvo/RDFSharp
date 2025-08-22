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
using RDFSharp.Store;
using System;
using System.IO;
using System.Linq;

namespace RDFSharp.Test.Store;

[TestClass]
public class RDFTriGTest
{
    #region Tests
    [TestMethod]
    public void ShouldSerializeEmptyStore()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeEmptyStore.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeEmptyStore.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeEmptyStore.trig"));
        Assert.IsTrue(fileContent.Equals(string.Empty, StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithDefaultGraphSPOQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruples.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruples.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruples.trig"));
        Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "  <http://subj1/> <http://pred1/> <http://obj1/>. " + Environment.NewLine + "  <http://subj2/> <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithDefaultGraphSPBQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("bnode:12345")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("bnode:54321")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPBQuadruples.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPBQuadruples.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPBQuadruples.trig"));
        Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "  <http://subj1/> <http://pred1/> _:12345. " + Environment.NewLine + "  <http://subj2/> <http://pred2/> _:54321. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithDefaultGraphBPOQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:54321"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPOQuadruples.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPOQuadruples.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPOQuadruples.trig"));
        Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "  _:12345 <http://pred1/> <http://obj1/>. " + Environment.NewLine + "  _:54321 <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithDefaultGraphBPBQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:12345"), new RDFResource("http://pred1/"), new RDFResource("bnode:54321")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("bnode:54321"), new RDFResource("http://pred2/"), new RDFResource("bnode:12345")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPBQuadruples.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPBQuadruples.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphBPBQuadruples.trig"));
        Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "  _:12345 <http://pred1/> _:54321. " + Environment.NewLine + "  _:54321 <http://pred2/> _:12345. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithDefaultGraphSPLQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFPlainLiteral("hello")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFPlainLiteral("hello","en-US--ltr")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruples.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruples.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruples.trig"));
        Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "  <http://subj1/> <http://pred1/> \"hello\". " + Environment.NewLine + "  <http://subj2/> <http://pred2/> \"hello\"@EN-US--LTR. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithDefaultGraphSPOQuadruplesHavingPrefixes()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), RDFVocabulary.RDFS.SEE_ALSO, new RDFResource("http://obj2/")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruplesHavingPrefixes.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruplesHavingPrefixes.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPOQuadruplesHavingPrefixes.trig"));
        Assert.IsTrue(fileContent.Equals("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>." + Environment.NewLine + "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>." + Environment.NewLine + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj1/> a <http://obj1/>. " + Environment.NewLine + "  <http://subj2/> rdfs:seeAlso <http://obj2/>. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithDefaultGraphSPLQuadruplesHavingPrefixes()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), RDFVocabulary.RDF.TYPE, new RDFPlainLiteral("hello")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), RDFVocabulary.RDFS.SEE_ALSO, new RDFPlainLiteral("hello","en-US")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruplesHavingPrefixes.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruplesHavingPrefixes.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithDefaultGraphSPLQuadruplesHavingPrefixes.trig"));
        Assert.IsTrue(fileContent.Equals("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>." + Environment.NewLine + "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>." + Environment.NewLine + "@prefix xsd: <http://www.w3.org/2001/XMLSchema#>." + Environment.NewLine + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj1/> a \"hello\". " + Environment.NewLine + "  <http://subj2/> <http://pred2/> \"25\"^^xsd:integer; " + Environment.NewLine + "                  rdfs:seeAlso \"hello\"@EN-US. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruples.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruples.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruples.trig"));
        Assert.IsTrue(fileContent.Equals("{" + Environment.NewLine + "  <http://subj1/> <http://pred1/> <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine + "GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj2/> <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithBothNotDefaultAndDefaultGraphsSPOQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothNotDefaultAndDefaultGraphsSPOQuadruples.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothNotDefaultAndDefaultGraphsSPOQuadruples.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothNotDefaultAndDefaultGraphsSPOQuadruples.trig"));
        Assert.IsTrue(fileContent.Equals("GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj1/> <http://pred1/> <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj2/> <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruplesHavingPrefixes()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj1/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj2/"), RDFVocabulary.RDFS.SEE_ALSO, new RDFResource("http://obj2/")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithBothDefaultAndNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig"));
        Assert.IsTrue(fileContent.Equals("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>." + Environment.NewLine + "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>." + Environment.NewLine + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj1/> a <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine + "GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj2/> rdfs:seeAlso <http://obj2/>. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruples()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), new RDFResource("http://pred1/"), new RDFResource("http://obj1/")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj2/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruples.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruples.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruples.trig"));
        Assert.IsTrue(fileContent.Equals("GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj1/> <http://pred1/> <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine + "GRAPH <http://ctx2/>" + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj2/> <http://pred2/> <http://obj2/>. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruplesHavingPrefixes()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx1/"), new RDFResource("http://subj1/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj1/")));
        store.AddQuadruple(new RDFQuadruple(new RDFContext("http://ctx2/"), new RDFResource("http://subj2/"), RDFVocabulary.RDFS.SEE_ALSO, new RDFResource("http://obj2/")));
        RDFTriG.Serialize(store, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig"));

        Assert.IsTrue(File.Exists(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig")));
        string fileContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldSerializeStoreWithNotDefaultGraphsSPOQuadruplesHavingPrefixes.trig"));
        Assert.IsTrue(fileContent.Equals("@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>." + Environment.NewLine + "@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>." + Environment.NewLine + Environment.NewLine + "GRAPH <http://ctx1/>" + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj1/> a <http://obj1/>. " + Environment.NewLine + "}" + Environment.NewLine + Environment.NewLine + "GRAPH <http://ctx2/>" + Environment.NewLine + "{" + Environment.NewLine + "  <http://subj2/> rdfs:seeAlso <http://obj2/>. " + Environment.NewLine + "}", StringComparison.Ordinal));
    }

    //DESERIALIZE

    [TestMethod]
    public void ShouldDeserializeEmptyStore()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(0, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldDeserializeEmptyStoreBecauseOnlyComments()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}#This is a comment!");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(0, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldDeserializeEmptyStoreBecauseOnlyCommentsEndingWithCarriageReturn()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.Write("#This is a comment! \r");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(0, store.QuadruplesCount);
    }

    [TestMethod]
    public void ShouldDeserializeStoreWithMultipleGraphsFromFile()
    {
        RDFMemoryStore store1 = new RDFMemoryStore();
        store1.AddQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
        store1.AddQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/")));
        RDFTriG.Serialize(store1, Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldDeserializeStoreWithMultipleGraphsFromFile.trig"));
        RDFMemoryStore store2 = RDFTriG.Deserialize(Path.Combine(Environment.CurrentDirectory, "RDFTriGTest_ShouldDeserializeStoreWithMultipleGraphsFromFile.trig"));

        Assert.IsNotNull(store2);
        Assert.AreEqual(2, store2.QuadruplesCount);
        Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:ctx"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store2.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphWithSPOTriple()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> <http://obj/>.");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphWithSPOTripleEvenOnMissingBaseDeclaration()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine("<http://subj/> <http://pred/> <http://obj/>.");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphWithSPOTripleEvenOnEmptyBaseDeclaration()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <>.{Environment.NewLine}<http://subj/> <http://pred/> <http://obj/>.");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeTrickyDefaultPrefixedGraphWithSPOTriple()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"PREFIX graph: <>{Environment.NewLine}graph:pippo <http://pred/> <http://obj/>.");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource($"{RDFNamespaceRegister.DefaultNamespace}pippo"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphByParenthesisWithSPOTriple()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}{{<http://subj/> <http://pred/> <http://obj/>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphByParenthesisWithSPOTripleHavingCustomBase()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <ex:org>.{Environment.NewLine}{{<http://subj/> <http://pred/> <http://obj/>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphByParenthesisWithMultipleSPOTriplesHavingCustomBase()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <ex:org>.{Environment.NewLine}{{<http://subj/> <http://pred/> <http://obj/>; <http://pred2/> <http://obj2/>, <http://obj3/>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(3, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj3/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphWithCustomDatatypeSPLTTriple()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}<http://subj/> <http://pred/> \"25\"^^<http://www.w3.org/2001/XMLSchema#testdt>.");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.RDFS_LITERAL))));
    }

    [TestMethod]
    public void ShouldDeserializeNamedGraphWithMultipleSPOTriples()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine("GRAPH <ex:org>{<http://subj/> <http://pred/> <http://obj/>; <http://pred2/> <http://obj2/>, <http://obj3/>.}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(3, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj3/"))));
    }

    [TestMethod]
    public void ShouldDeserializeNamedPrefixedGraphWithMultipleSPOTriples()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.{Environment.NewLine}GRAPH rdf:example{{<http://subj/> <http://pred/> <http://obj/>; <http://pred2/> <http://obj2/>, <http://obj3/>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(3, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://www.w3.org/1999/02/22-rdf-syntax-ns#example"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://www.w3.org/1999/02/22-rdf-syntax-ns#example"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://www.w3.org/1999/02/22-rdf-syntax-ns#example"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj3/"))));
    }

    [TestMethod]
    public void ShouldDeserializeNamedBaseGraphWithMultipleSPOTriples()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}GRAPH :example{{<http://subj/> <http://pred/> <http://obj/>; <http://pred2/> <http://obj2/>, <http://obj3/>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(3, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/example"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/example"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/example"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj3/"))));
    }

    [TestMethod]
    public void ShouldDeserializeUnnamedBaseGraphWithMultipleSPOTriples()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}:example{{<http://subj/> <http://pred/> <http://obj/>; <http://pred2/> <http://obj2/>, <http://obj3/>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(3, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/example"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/example"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/example"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj3/"))));
    }

    [TestMethod]
    public void ShouldDeserializeMultipleNamedGraphs()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}GRAPH <ex:graph1>{{<http://subj/> a <http://obj/>.}}{Environment.NewLine}GRAPH <ex:graph2>{{<http://subj> <http://pred> <http://obj>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(2, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:graph1"), new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:graph2"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeMultipleDefaultGraphs()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}{{<http://subj/> a <http://obj/>.}}{Environment.NewLine}{{<http://subj> <http://pred> <http://obj>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(2, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeMultipleBaseGraphs()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <ex:org>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}{{<http://subj/> a <http://obj/>.}}{Environment.NewLine}{{<http://subj> <http://pred> <http://obj>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(2, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeMultipleNamedDefaultGraphs()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}GRAPH :{{<http://subj/> a <http://obj/>.}}{Environment.NewLine}GRAPH :{{<http://subj> <http://pred> <http://obj>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(2, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext(), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeMultipleNamedBaseGraphs()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <ex:org>.{Environment.NewLine}@prefix rdf: <{RDFVocabulary.RDF.BASE_URI}>.{Environment.NewLine}GRAPH :{{<http://subj/> a <http://obj/>.}}{Environment.NewLine}GRAPH :{{<http://subj> <http://pred> <http://obj>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(2, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), RDFVocabulary.RDF.TYPE, new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("ex:org"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeMultipleGraphsTryingAllSyntaxCombinations1()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}:graph1{{<http://subj/> <http://pred/> <http://obj/>; <http://pred2/> <http://obj2/>, <http://obj3/>.}}{Environment.NewLine}GRAPH :graph2{{<http://subj> <http://pred> <http://obj>.}}{Environment.NewLine}GRAPH <http://ctx3/>{{_:12345 <http://pred/> \"hello\"@EN-US--LTR.}}{Environment.NewLine}GRAPH <http://ctx3/>{{<http://subj/> <http://pred/> <http://obj/>.}}{Environment.NewLine}<http://subjAlone/> <http://predAlone/> <http://objAlone/>.");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(7, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/graph1"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/graph1"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj2/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/graph1"), new RDFResource("http://subj/"), new RDFResource("http://pred2/"), new RDFResource("http://obj3/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/graph2"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx3/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello","EN-US--LTR"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx3/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/"), new RDFResource("http://subjAlone/"), new RDFResource("http://predAlone/"), new RDFResource("http://objAlone/"))));
    }

    [TestMethod]
    public void ShouldDeserializeMultipleGraphsTryingAllSyntaxCombinations2()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}<http://subj/> <http://pred/> <http://obj/>.{Environment.NewLine}GRAPH :graph2{{<http://subj> <http://pred> <http://obj>.}}{Environment.NewLine}GRAPH <http://ctx3/>{{_:12345 <http://pred/> \"hello\"@EN-US.}}{Environment.NewLine}GRAPH <http://ctx3/>{{<http://subj/> <http://pred/> <http://obj/>.}}{Environment.NewLine}GRAPH :{{<http://subjAlone/> <http://predAlone/> <http://objAlone/>.}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(5, store.QuadruplesCount);
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/graph2"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx3/"), new RDFResource("bnode:12345"), new RDFResource("http://pred/"), new RDFPlainLiteral("hello", "EN-US"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://ctx3/"), new RDFResource("http://subj/"), new RDFResource("http://pred/"), new RDFResource("http://obj/"))));
        Assert.IsTrue(store.ContainsQuadruple(new RDFQuadruple(new RDFContext("http://example.org/"), new RDFResource("http://subjAlone/"), new RDFResource("http://predAlone/"), new RDFResource("http://objAlone/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphByParenthesisWithBPOAnonymousInlineTriple()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}{{[<http://pred/> <http://obj/>].}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.Any(q => q.Context.Equals(new RDFContext())
                                     && q.Subject is RDFResource { IsBlank: true }
                                     && q.Predicate.Equals(new RDFResource("http://pred/"))
                                     && q.Object.Equals(new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphByParenthesisWithBPOAnonymousTriple()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <{RDFNamespaceRegister.DefaultNamespace}>.{Environment.NewLine}{{[] <http://pred/> <http://obj/> .}}");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.Any(q => q.Context.Equals(new RDFContext())
                                     && q.Subject is RDFResource { IsBlank: true }
                                     && q.Predicate.Equals(new RDFResource("http://pred/"))
                                     && q.Object.Equals(new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldDeserializeDefaultGraphWithBPOAnonymousTriple()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine("[] <http://pred/> <http://obj/> .");
        RDFMemoryStore store = RDFTriG.Deserialize(new MemoryStream(stream.ToArray()));

        Assert.IsNotNull(store);
        Assert.AreEqual(1, store.QuadruplesCount);
        Assert.IsTrue(store.Any(q => q.Context.Equals(new RDFContext())
                                     && q.Subject is RDFResource { IsBlank: true } && q.Predicate.Equals(new RDFResource("http://pred/"))
                                     && q.Object.Equals(new RDFResource("http://obj/"))));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingNamedGraphBecauseBadFormedSPARQLPrefix()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"PREFIX pf: <ex:pf>.{Environment.NewLine}GRAPH <ex:org>{{<http://subj/> <http://pred/> <http://obj/> .}}");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingBnodeNamedGraph()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine("GRAPH _:12345{<http://subj/> <http://pred/> <http://obj/> .}");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingNamedGraphBecauseMissingParenthesis()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}GRAPH <http://ctx3/> <http://subj/> <http://pred/> <http://obj/>.");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingNamedGraphBecauseUnclosedParenthesis()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}GRAPH <http://ctx3/> {{<http://subj/> <http://pred/> <http://obj/>.");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingNamedGraphBecauseUnopenedParenthesis()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}GRAPH <http://ctx3/> <http://subj/> <http://pred/> <http://obj/>.}}");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingNamedGraphBecauseWrongParenthesis()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}GRAPH <http://ctx3/> {{{{<http://subj/> <http://pred/> <http://obj/>.}}");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingBaseGraphBecauseMissingParenthesis()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}GRAPH : <http://subj/> <http://pred/> <http://obj/>.");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingBaseGraphBecauseIllegalGraphName()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine($"@base <http://example.org/>.{Environment.NewLine}GRAPH pippo {{<http://subj/> <http://pred/> <http://obj/>.}}");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnDeserializingVersionDirective()
    {
        MemoryStream stream = new MemoryStream();
        using (StreamWriter writer = new StreamWriter(stream))
            writer.WriteLine("@base <http://example.org/>. version 1.2");
        Assert.ThrowsExactly<RDFStoreException>(() => RDFTriG.Deserialize(new MemoryStream(stream.ToArray())));
    }

    [TestCleanup]
    public void Cleanup()
    {
        foreach (string file in Directory.EnumerateFiles(Environment.CurrentDirectory, "RDFTriGTest_Should*"))
            File.Delete(file);
    }
    #endregion
}