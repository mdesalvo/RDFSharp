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
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFPatternTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateSPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("<ex:subj> <ex:pred> <ex:obj>"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("<ex:subj> <ex:pred> \"lit\""));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("<ex:subj> <ex:pred> \"lit\"@EN-US")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("<ex:subj> <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer>")));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("<ex:subj> <ex:pred> \"25\"^^xsd:integer")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPatternBecauseNullSubject()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(null, new RDFResource("ex:pred"), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPatternBecauseUnsupportedSubject()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFPlainLiteral("lit"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPatternBecauseNullPredicate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFResource("ex:subj"), null, new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPatternBecauseBlankPredicate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFResource("ex:subj"), new RDFResource(), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPatternBecauseUnsupportedPredicate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFResource("ex:subj"), new RDFPlainLiteral("lit"), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPatternBecauseNullObject()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPatternBecauseUnsupportedObject()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFContext()));
        
    [TestMethod]
    public void ShouldCreateVPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> <ex:obj>"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> \"lit\""));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> \"lit\"@EN-US"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer>"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("?SUBJ <ex:pred> \"25\"^^xsd:integer")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED <ex:obj>"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED \"lit\""));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED \"lit\"@EN-US"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer>"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("<ex:subj> ?PRED \"25\"^^xsd:integer")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("<ex:subj> <ex:pred> ?OBJ"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED <ex:obj>"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED \"lit\""));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED \"lit\"@EN-US"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer>"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("?SUBJ ?PRED \"25\"^^xsd:integer")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateSVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("<ex:subj> ?PRED ?OBJ"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ <ex:pred> ?OBJ"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED ?OBJ"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> <ex:pred> <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> <ex:pred> \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("OPTIONAL { <ex:subj> <ex:pred> \"lit\"@EN-US }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("OPTIONAL { <ex:subj> <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }")));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { <ex:subj> <ex:pred> \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { ?SUBJ <ex:pred> \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { <ex:subj> ?PRED \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> <ex:pred> ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { ?SUBJ ?PRED \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalSVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { <ex:subj> ?PRED ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ <ex:pred> ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?PRED ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalPatternWithSharedVariable()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("subj"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { ?SUBJ ?SUBJ ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldSetUnionWithNextPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj")).UnionWithNext();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsTrue(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?PRED ?OBJ"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldSetMinusWithNextPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("subj"), new RDFVariable("subj"), new RDFVariable("obj")).MinusWithNext();

        Assert.IsNotNull(pattern);
        Assert.IsNull(pattern.Context);
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("subj"))); //test that variables will not be duplicated
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsTrue(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("?SUBJ ?SUBJ ?OBJ"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    //PATTERN WITH CONTEXT

    [TestMethod]
    public void ShouldCreateCSPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { <ex:subj> <ex:pred> <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { <ex:subj> <ex:pred> \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("GRAPH <ex:ctx> { <ex:subj> <ex:pred> \"lit\"@EN-US }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("GRAPH <ex:ctx> { <ex:subj> <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }")));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("GRAPH <ex:ctx> { <ex:subj> <ex:pred> \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseNullContext()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(null, new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseUnsupportedContext()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFPlainLiteral("lit"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseNullSubject()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFContext("ex:ctx"), null, new RDFResource("ex:pred"), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseUnsupportedSubject()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFContext("ex:ctx"), new RDFPlainLiteral("lit"), new RDFResource("ex:pred"), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseNullPredicate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), null, new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseBlankPredicate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource(), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseUnsupportedPredicate()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFPlainLiteral("lit"), new RDFResource("ex:obj")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseNullObject()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextPatternBecauseUnsupportedObject()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFContext()));

    [TestMethod]
    public void ShouldCreateCVPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ <ex:pred> <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ <ex:pred> \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ <ex:pred> \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("GRAPH <ex:ctx> { ?SUBJ <ex:pred> \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { <ex:subj> ?PRED <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { <ex:subj> ?PRED \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { <ex:subj> ?PRED \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { <ex:subj> ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("GRAPH <ex:ctx> { <ex:subj> ?PRED \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { <ex:subj> <ex:pred> ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ ?PRED <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ ?PRED \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ ?PRED \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("GRAPH <ex:ctx> { ?SUBJ ?PRED \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCSVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { <ex:subj> ?PRED ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ <ex:pred> ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateCVVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH <ex:ctx> { ?SUBJ ?PRED ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> <ex:pred> <ex:obj> } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> <ex:pred> \"lit\" } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> <ex:pred> \"lit\"@EN-US } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(0, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> } }")));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> <ex:pred> \"25\"^^xsd:integer } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ <ex:pred> <ex:obj> } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ <ex:pred> \"lit\" } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ <ex:pred> \"lit\"@EN-US } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> } }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ <ex:pred> \"25\"^^xsd:integer } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> ?PRED <ex:obj> } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> ?PRED \"lit\" } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> ?PRED \"lit\"@EN-US } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> } }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> ?PRED \"25\"^^xsd:integer } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> <ex:pred> ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ ?PRED <ex:obj> } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ ?PRED \"lit\" } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ ?PRED \"lit\"@EN-US } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> } }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ ?PRED \"25\"^^xsd:integer } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCSVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { <ex:subj> ?PRED ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ <ex:pred> ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalCVVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ ?PRED ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalContextPatternWithSharedVariable()
    {
        RDFPattern pattern = new RDFPattern(new RDFContext("ex:ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFContext("ex:ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH <ex:ctx> { ?SUBJ ?PRED ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    //PATTERN WITH VARIABLE CONTEXT

    [TestMethod]
    public void ShouldCreateVSPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { <ex:subj> <ex:pred> <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { <ex:subj> <ex:pred> \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("GRAPH ?CTX { <ex:subj> <ex:pred> \"lit\"@EN-US }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("GRAPH ?CTX { <ex:subj> <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }")));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("GRAPH ?CTX { <ex:subj> <ex:pred> \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ <ex:pred> <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ <ex:pred> \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ <ex:pred> \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("GRAPH ?CTX { ?SUBJ <ex:pred> \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { <ex:subj> ?PRED <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { <ex:subj> ?PRED \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { <ex:subj> ?PRED \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { <ex:subj> ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("GRAPH ?CTX { <ex:subj> ?PRED \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { <ex:subj> <ex:pred> ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFResource("ex:obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ ?PRED <ex:obj> }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ ?PRED \"lit\" }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ ?PRED \"lit\"@EN-US }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("GRAPH ?CTX { ?SUBJ ?PRED \"25\"^^xsd:integer }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVSVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { <ex:subj> ?PRED ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ <ex:pred> ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateVVVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj"));

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsFalse(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(4, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("GRAPH ?CTX { ?SUBJ ?PRED ?OBJ }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { <ex:subj> <ex:pred> <ex:obj> } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { <ex:subj> <ex:pred> \"lit\" } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("OPTIONAL { GRAPH ?CTX { <ex:subj> <ex:pred> \"lit\"@EN-US } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(1, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals(string.Concat("OPTIONAL { GRAPH ?CTX { <ex:subj> <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> } }")));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { GRAPH ?CTX { <ex:subj> <ex:pred> \"25\"^^xsd:integer } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVPOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ <ex:pred> <ex:obj> } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVPLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ <ex:pred> \"lit\" } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVPLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ <ex:pred> \"lit\"@EN-US } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVPLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ <ex:pred> \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> } }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { GRAPH ?CTX { ?SUBJ <ex:pred> \"25\"^^xsd:integer } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { <ex:subj> ?PRED <ex:obj> } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { <ex:subj> ?PRED \"lit\" } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { <ex:subj> ?PRED \"lit\"@EN-US } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { <ex:subj> ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> } }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { GRAPH ?CTX { <ex:subj> ?PRED \"25\"^^xsd:integer } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFResource("ex:pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(2, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { <ex:subj> <ex:pred> ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVVOPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFResource("ex:obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFResource("ex:obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ ?PRED <ex:obj> } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVVLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ ?PRED \"lit\" } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVVLLPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFPlainLiteral("lit", "en-US")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFPlainLiteral("lit", "en-US")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ ?PRED \"lit\"@EN-US } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVVLTPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ ?PRED \"25\"^^<http://www.w3.org/2001/XMLSchema#integer> } }"));
        Assert.IsTrue(pattern.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]).Equals(string.Concat("OPTIONAL { GRAPH ?CTX { ?SUBJ ?PRED \"25\"^^xsd:integer } }")));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVSVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFResource("ex:subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFResource("ex:subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { <ex:subj> ?PRED ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVPVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFResource("ex:pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFResource("ex:pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(3, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ <ex:pred> ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalVVVVPattern()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(4, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ ?PRED ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldCreateOptionalContextVariablePatternWithSharedVariable()
    {
        RDFPattern pattern = new RDFPattern(new RDFVariable("ctx"), new RDFVariable("subj"), new RDFVariable("pred"), new RDFVariable("obj")).Optional();

        Assert.IsNotNull(pattern);
        Assert.IsNotNull(pattern.Context);
        Assert.IsTrue(pattern.Context.Equals(new RDFVariable("ctx")));
        Assert.IsNotNull(pattern.Subject);
        Assert.IsTrue(pattern.Subject.Equals(new RDFVariable("subj")));
        Assert.IsNotNull(pattern.Predicate);
        Assert.IsTrue(pattern.Predicate.Equals(new RDFVariable("pred")));
        Assert.IsNotNull(pattern.Object);
        Assert.IsTrue(pattern.Object.Equals(new RDFVariable("obj")));
        Assert.IsTrue(pattern.IsEvaluable);
        Assert.IsTrue(pattern.IsOptional);
        Assert.IsFalse(pattern.JoinAsUnion);
        Assert.IsFalse(pattern.JoinAsMinus);
        Assert.IsNotNull(pattern.Variables);
        Assert.AreEqual(4, pattern.Variables.Count);
        Assert.IsTrue(pattern.ToString().Equals("OPTIONAL { GRAPH ?CTX { ?SUBJ ?PRED ?OBJ } }"));
        Assert.IsTrue(pattern.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(pattern.PatternGroupMemberStringID)));
    }
    #endregion
}