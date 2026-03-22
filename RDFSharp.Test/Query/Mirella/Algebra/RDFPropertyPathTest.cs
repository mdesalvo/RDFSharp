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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFPropertyPathTest
{
    #region Tests (Legacy)
    [TestMethod]
    public void ShouldCreatePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.IsEmpty(propertyPath.Steps);
        Assert.AreEqual(0, propertyPath.Depth);
        Assert.IsFalse(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START  <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START  rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullStart()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(null, new RDFVariable("?END")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseUnsupportedStart()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFPlainLiteral("start"), new RDFVariable("?END")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullEnd()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseUnsupportedEnd()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFPlainLiteral("end")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullSequenceStep()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddSequenceStep(null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullPropertyPathStepInSequenceStep()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddSequenceStep(new RDFPropertyPathStep(null)));

    [TestMethod]
    public void ShouldAddSingleSequenceStep()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT));

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(1, propertyPath.Steps);
        Assert.AreEqual(1, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleSequenceInverseStep()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT).Inverse());

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(1, propertyPath.Steps);
        Assert.AreEqual(1, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleSequenceSteps()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.BAG));

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(2, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleSequenceStepsWithInverseFirst()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT).Inverse());
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.BAG));

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(2, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt/rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleSequenceStepsWithInverseLast()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.BAG).Inverse());

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(2, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/^<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/^rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleSequenceStepsWithInverseBoth()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT).Inverse());
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.BAG).Inverse());

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(2, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}>/^<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt/^rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeSteps()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.ALT),
            new RDFPropertyPathStep(RDFVocabulary.RDF.BAG) ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(1, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Alt|rdf:Bag) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeStepsWithInverseFirst()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.ALT).Inverse(),
            new RDFPropertyPathStep(RDFVocabulary.RDF.BAG) ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(1, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (^<{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (^rdf:Alt|rdf:Bag) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeStepsWithInverseLast()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.ALT),
            new RDFPropertyPathStep(RDFVocabulary.RDF.BAG).Inverse() ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(1, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.ALT}>|^<{RDFVocabulary.RDF.BAG}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Alt|^rdf:Bag) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeStepsWithInverseBoth()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.ALT).Inverse(),
            new RDFPropertyPathStep(RDFVocabulary.RDF.BAG).Inverse() ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(1, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (^<{RDFVocabulary.RDF.ALT}>|^<{RDFVocabulary.RDF.BAG}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (^rdf:Alt|^rdf:Bag) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeStepsBecomingSequenceStepBecauseSingle()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([new RDFPropertyPathStep(RDFVocabulary.RDF.ALT)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(1, propertyPath.Steps);
        Assert.AreEqual(1, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleAlternativeStepsBecomingSequenceStepBecauseSingle()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([new RDFPropertyPathStep(RDFVocabulary.RDF.ALT)]);
        propertyPath.AddAlternativeSteps([new RDFPropertyPathStep(RDFVocabulary.RDF.BAG)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(2, propertyPath.Steps);
        Assert.AreEqual(2, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleAlternativeStepsAndMerge()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.ALT),
            new RDFPropertyPathStep(RDFVocabulary.RDF.BAG) ]);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ) ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(4, propertyPath.Steps);
        Assert.AreEqual(1, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Alt|rdf:Bag|rdf:Bag|rdf:Seq) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMixedStepsSequentialAlternatives()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(3, propertyPath.Steps);
        Assert.AreEqual(2, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMixedStepsAlternativesSequential()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ)]);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT));

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(3, propertyPath.Steps);
        Assert.AreEqual(2, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/<{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/rdf:Alt rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMixedStepsAlternativesSequentialAlternatives()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ)]);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(5, propertyPath.Steps);
        Assert.AreEqual(3, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/<{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMixedStepsAlternativesInverseSequentialAlternatives()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ)]);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT).Inverse());
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.IsNotNull(propertyPath.Steps);
        Assert.HasCount(5, propertyPath.Steps);
        Assert.AreEqual(3, propertyPath.Depth);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/^<{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/^rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullAlternativeStep()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddAlternativeSteps(null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseEmptyAlternativeStep()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddAlternativeSteps([]));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullPropertyPathStepInAlternativeSteps()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddAlternativeSteps([null]));

    [TestMethod]
    public void ShouldGetPatternListFromEmptyPropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.IsEmpty(patterns);
    }

    [TestMethod]
    public void ShouldGetPatternListFromUniqueSequencePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE));
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(1, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?START")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?END")) && !patterns[0].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromUniqueSequenceInversePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE).Inverse());
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(1, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?END")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?START"))&& !patterns[0].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromUniqueAlternativePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddAlternativeSteps([new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE)]);
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(1, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?START")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?END")) && !patterns[0].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromUniqueAlternativeInversePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddAlternativeSteps([new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE).Inverse()]);
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(1, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?END")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?START")) && !patterns[0].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromMultipleSequencePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.LI));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.REST));
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(3, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?START")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?__PP0")) && !patterns[0].JoinAsUnion);
        Assert.IsTrue(patterns[1].Subject.Equals(new RDFVariable("?__PP0")) && patterns[1].Predicate.Equals(RDFVocabulary.RDF.LI) && patterns[1].Object.Equals(new RDFVariable("?__PP1")) && !patterns[1].JoinAsUnion);
        Assert.IsTrue(patterns[2].Subject.Equals(new RDFVariable("?__PP1")) && patterns[2].Predicate.Equals(RDFVocabulary.RDF.REST) && patterns[2].Object.Equals(new RDFVariable("?END")) && !patterns[2].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromMultipleSequenceInversePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE).Inverse());
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.LI));
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.REST));
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(3, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?__PP0")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?START")) && !patterns[0].JoinAsUnion);
        Assert.IsTrue(patterns[1].Subject.Equals(new RDFVariable("?__PP0")) && patterns[1].Predicate.Equals(RDFVocabulary.RDF.LI) && patterns[1].Object.Equals(new RDFVariable("?__PP1")) && !patterns[1].JoinAsUnion);
        Assert.IsTrue(patterns[2].Subject.Equals(new RDFVariable("?__PP1")) && patterns[2].Predicate.Equals(RDFVocabulary.RDF.REST) && patterns[2].Object.Equals(new RDFVariable("?END")) && !patterns[2].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromSingleAlternativePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE),
            new RDFPropertyPathStep(RDFVocabulary.RDF.LI),
            new RDFPropertyPathStep(RDFVocabulary.RDF.REST) ]);
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(3, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?START")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?END")) && patterns[0].JoinAsUnion);
        Assert.IsTrue(patterns[1].Subject.Equals(new RDFVariable("?START")) && patterns[1].Predicate.Equals(RDFVocabulary.RDF.LI) && patterns[1].Object.Equals(new RDFVariable("?END")) && patterns[1].JoinAsUnion);
        Assert.IsTrue(patterns[2].Subject.Equals(new RDFVariable("?START")) && patterns[2].Predicate.Equals(RDFVocabulary.RDF.REST) && patterns[2].Object.Equals(new RDFVariable("?END")) && !patterns[2].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromMultipleAlternativePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE),
            new RDFPropertyPathStep(RDFVocabulary.RDF.LI),
            new RDFPropertyPathStep(RDFVocabulary.RDF.REST) ]);
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ) ]);
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(5, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?START")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?END")) && patterns[0].JoinAsUnion);
        Assert.IsTrue(patterns[1].Subject.Equals(new RDFVariable("?START")) && patterns[1].Predicate.Equals(RDFVocabulary.RDF.LI) && patterns[1].Object.Equals(new RDFVariable("?END")) && patterns[1].JoinAsUnion);
        Assert.IsTrue(patterns[2].Subject.Equals(new RDFVariable("?START")) && patterns[2].Predicate.Equals(RDFVocabulary.RDF.REST) && patterns[2].Object.Equals(new RDFVariable("?END")) && patterns[2].JoinAsUnion);
        Assert.IsTrue(patterns[3].Subject.Equals(new RDFVariable("?START")) && patterns[3].Predicate.Equals(RDFVocabulary.RDF.BAG) && patterns[3].Object.Equals(new RDFVariable("?END")) && patterns[3].JoinAsUnion);
        Assert.IsTrue(patterns[4].Subject.Equals(new RDFVariable("?START")) && patterns[4].Predicate.Equals(RDFVocabulary.RDF.SEQ) && patterns[4].Object.Equals(new RDFVariable("?END")) && !patterns[4].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromMultipleAlternativeAndSequenceMiddlePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE),
            new RDFPropertyPathStep(RDFVocabulary.RDF.LI),
            new RDFPropertyPathStep(RDFVocabulary.RDF.REST) ]);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.HTML));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ) ]);
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(6, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?START")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?__PP0")) && patterns[0].JoinAsUnion);
        Assert.IsTrue(patterns[1].Subject.Equals(new RDFVariable("?START")) && patterns[1].Predicate.Equals(RDFVocabulary.RDF.LI) && patterns[1].Object.Equals(new RDFVariable("?__PP0")) && patterns[1].JoinAsUnion);
        Assert.IsTrue(patterns[2].Subject.Equals(new RDFVariable("?START")) && patterns[2].Predicate.Equals(RDFVocabulary.RDF.REST) && patterns[2].Object.Equals(new RDFVariable("?__PP0")) && !patterns[2].JoinAsUnion);
        Assert.IsTrue(patterns[3].Subject.Equals(new RDFVariable("?__PP0")) && patterns[3].Predicate.Equals(RDFVocabulary.RDF.HTML) && patterns[3].Object.Equals(new RDFVariable("?__PP3")) && !patterns[3].JoinAsUnion);
        Assert.IsTrue(patterns[4].Subject.Equals(new RDFVariable("?__PP3")) && patterns[4].Predicate.Equals(RDFVocabulary.RDF.BAG) && patterns[4].Object.Equals(new RDFVariable("?END")) && patterns[4].JoinAsUnion);
        Assert.IsTrue(patterns[5].Subject.Equals(new RDFVariable("?__PP3")) && patterns[5].Predicate.Equals(RDFVocabulary.RDF.SEQ) && patterns[5].Object.Equals(new RDFVariable("?END")) && !patterns[5].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromMultipleAlternativeAndSequenceMiddleAndInversePropertyPath()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE),
            new RDFPropertyPathStep(RDFVocabulary.RDF.LI).Inverse(),
            new RDFPropertyPathStep(RDFVocabulary.RDF.REST).Inverse() ]);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.HTML));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.BAG),
            new RDFPropertyPathStep(RDFVocabulary.RDF.SEQ) ]);
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(6, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?START")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?__PP0")) && patterns[0].JoinAsUnion);
        Assert.IsTrue(patterns[1].Subject.Equals(new RDFVariable("?__PP0")) && patterns[1].Predicate.Equals(RDFVocabulary.RDF.LI) && patterns[1].Object.Equals(new RDFVariable("?START")) && patterns[1].JoinAsUnion);
        Assert.IsTrue(patterns[2].Subject.Equals(new RDFVariable("?__PP0")) && patterns[2].Predicate.Equals(RDFVocabulary.RDF.REST) && patterns[2].Object.Equals(new RDFVariable("?START")) && !patterns[2].JoinAsUnion);
        Assert.IsTrue(patterns[3].Subject.Equals(new RDFVariable("?__PP0")) && patterns[3].Predicate.Equals(RDFVocabulary.RDF.HTML) && patterns[3].Object.Equals(new RDFVariable("?__PP3")) && !patterns[3].JoinAsUnion);
        Assert.IsTrue(patterns[4].Subject.Equals(new RDFVariable("?__PP3")) && patterns[4].Predicate.Equals(RDFVocabulary.RDF.BAG) && patterns[4].Object.Equals(new RDFVariable("?END")) && patterns[4].JoinAsUnion);
        Assert.IsTrue(patterns[5].Subject.Equals(new RDFVariable("?__PP3")) && patterns[5].Predicate.Equals(RDFVocabulary.RDF.SEQ) && patterns[5].Object.Equals(new RDFVariable("?END")) && !patterns[5].JoinAsUnion);
    }

    [TestMethod]
    public void ShouldGetPatternListFromMultipleAlternativePropertyPathEndingWithSequence()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"));
        propertyPath.AddAlternativeSteps([ new RDFPropertyPathStep(RDFVocabulary.RDF.TYPE),
            new RDFPropertyPathStep(RDFVocabulary.RDF.LI),
            new RDFPropertyPathStep(RDFVocabulary.RDF.REST) ]);
        propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.HTML));
        List<RDFPattern> patterns = propertyPath.GetPatternList();

        Assert.IsNotNull(patterns);
        Assert.HasCount(4, patterns);
        Assert.IsTrue(patterns[0].Subject.Equals(new RDFVariable("?START")) && patterns[0].Predicate.Equals(RDFVocabulary.RDF.TYPE) && patterns[0].Object.Equals(new RDFVariable("?__PP0")) && patterns[0].JoinAsUnion);
        Assert.IsTrue(patterns[1].Subject.Equals(new RDFVariable("?START")) && patterns[1].Predicate.Equals(RDFVocabulary.RDF.LI) && patterns[1].Object.Equals(new RDFVariable("?__PP0")) && patterns[1].JoinAsUnion);
        Assert.IsTrue(patterns[2].Subject.Equals(new RDFVariable("?START")) && patterns[2].Predicate.Equals(RDFVocabulary.RDF.REST) && patterns[2].Object.Equals(new RDFVariable("?__PP0")) && !patterns[2].JoinAsUnion);
        Assert.IsTrue(patterns[3].Subject.Equals(new RDFVariable("?__PP0")) && patterns[3].Predicate.Equals(RDFVocabulary.RDF.HTML) && patterns[3].Object.Equals(new RDFVariable("?END")) && !patterns[3].JoinAsUnion);
    }
    #endregion

    #region Tests (Algebric)

    #region Shared vocabulary
    private static readonly RDFResource Alice  = new RDFResource("ex:alice");
    private static readonly RDFResource Bob    = new RDFResource("ex:bob");
    private static readonly RDFResource Carol  = new RDFResource("ex:carol");
    private static readonly RDFResource Dave   = new RDFResource("ex:dave");
    private static readonly RDFResource Eve    = new RDFResource("ex:eve");
    private static readonly RDFResource Frank  = new RDFResource("ex:frank");
    private static readonly RDFResource Knows  = new RDFResource("ex:knows");
    private static readonly RDFResource Parent = new RDFResource("ex:parent");
    private static readonly RDFResource Type   = new RDFResource("rdf:type");
    private static readonly RDFResource Person = new RDFResource("ex:Person");
    private static readonly RDFResource Tag     = new RDFResource("ex:tag");
    private static readonly RDFResource Tagged  = new RDFResource("ex:tagged");
    private static readonly RDFResource Reached = new RDFResource("ex:reached");
    private static readonly RDFResource Ancestor= new RDFResource("ex:ancestor");
    private static readonly RDFResource Related = new RDFResource("ex:related");

    private static readonly RDFVariable VarS = new RDFVariable("s");
    private static readonly RDFVariable VarE = new RDFVariable("e");
    private static readonly RDFVariable VarX = new RDFVariable("x");

    private static RDFGraph BuildTestGraph()
    {
        // alice -> bob -> carol -> dave  (via ex:knows)
        RDFGraph g = new RDFGraph();
        g.AddTriple(new RDFTriple(Alice, Knows, Bob));
        g.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        g.AddTriple(new RDFTriple(Carol, Knows, Dave));
        return g;
    }
    #endregion

    #region Algebra (step cardinality fluent API)

    [TestMethod]
    public void StepCardinality_Default_IsExactlyOne()
    {
        RDFPropertyPathStep step = new RDFPropertyPathStep(Knows);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne, step.StepCardinality);
        Assert.AreEqual(1, step.MinCardinality);
        Assert.AreEqual(1, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_ZeroOrOne_Fluent()
    {
        RDFPropertyPathStep step = new RDFPropertyPathStep(Knows).ZeroOrOne();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne, step.StepCardinality);
        Assert.AreEqual(0, step.MinCardinality);
        Assert.AreEqual(1, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_OneOrMore_Fluent()
    {
        RDFPropertyPathStep step = new RDFPropertyPathStep(Knows).OneOrMore();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, step.StepCardinality);
        Assert.AreEqual(1, step.MinCardinality);
        Assert.AreEqual(-1, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_ZeroOrMore_Fluent()
    {
        RDFPropertyPathStep step = new RDFPropertyPathStep(Knows).ZeroOrMore();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore, step.StepCardinality);
        Assert.AreEqual(0, step.MinCardinality);
        Assert.AreEqual(-1, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_BoundedRange_Fluent()
    {
        RDFPropertyPathStep step = new RDFPropertyPathStep(Knows).BoundedRange(2, 4);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.BoundedRange, step.StepCardinality);
        Assert.AreEqual(2, step.MinCardinality);
        Assert.AreEqual(4, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_Repeat_NegativeMin_Throws()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFPropertyPathStep(Knows).BoundedRange(-1, 2));

    [TestMethod]
    public void StepCardinality_Repeat_MaxLessThanMin_Throws()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFPropertyPathStep(Knows).BoundedRange(3, 1));

    [TestMethod]
    public void StepCardinality_InverseCombines_WithCardinality()
    {
        RDFPropertyPathStep step = new RDFPropertyPathStep(Knows).Inverse().OneOrMore();
        Assert.IsTrue(step.IsInverseStep);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, step.StepCardinality);
    }

    [TestMethod]
    public void PropertyPath_HasTransitiveSteps_FalseByDefault()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows));
        Assert.IsFalse(path.HasTransitiveSteps);
    }

    [TestMethod]
    public void PropertyPath_HasTransitiveSteps_TrueForOneOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());
        Assert.IsTrue(path.HasTransitiveSteps);
    }

    [TestMethod]
    public void PropertyPath_HasTransitiveSteps_TrueForZeroOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());
        Assert.IsTrue(path.HasTransitiveSteps);
    }

    [TestMethod]
    public void PropertyPath_IsEvaluable_TrueForTransitiveSingleStepWithConcreteEnds()
    {
        // A transitive step with concrete start/end and Depth=1 should still be evaluable
        RDFPropertyPath path = new RDFPropertyPath(Alice, Bob)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());
        Assert.IsTrue(path.IsEvaluable);
    }

    #endregion

    #region Engine — OneOrMore (prop+)

    [TestMethod]
    public void Engine_OneOrMore_ChainForwardVariableEnd()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();

        Assert.Contains(Bob.ToString(), ends);
        Assert.Contains(Carol.ToString(), ends);
        Assert.Contains(Dave.ToString(), ends);
        Assert.DoesNotContain(Alice.ToString(), ends); // no self with OneOrMore
    }

    [TestMethod]
    public void Engine_OneOrMore_ChainBothVariable()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        // alice reachable from alice: bob, carol, dave
        // bob reachable from bob: carol, dave
        // carol reachable from carol: dave
        Assert.IsGreaterThanOrEqualTo(6, result.Rows.Count);
        List<(string, string)> rows = result.Rows.Cast<DataRow>()
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        Assert.IsTrue(rows.Any(p => p.Item1 == Alice.ToString() && p.Item2 == Dave.ToString()));
    }

    [TestMethod]
    public void Engine_OneOrMore_ConcreteStartEnd_Reachable()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, Dave)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);
        Assert.HasCount(1, result.Rows);
    }

    [TestMethod]
    public void Engine_OneOrMore_ConcreteStartEnd_NotReachable()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Dave, Alice)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);
        Assert.IsEmpty(result.Rows);
    }

    [TestMethod]
    public void Engine_OneOrMore_InverseStep()
    {
        RDFGraph graph = BuildTestGraph();
        // dave ^knows+ ?e  => alice, bob, carol reachable via reverse knows
        RDFPropertyPath path = new RDFPropertyPath(Dave, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Carol.ToString(), ends);
        Assert.Contains(Bob.ToString(), ends);
        Assert.Contains(Alice.ToString(), ends);
    }

    [TestMethod]
    public void Engine_OneOrMore_NoCycle_NoDuplicates()
    {
        // linear chain: no cycles
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        List<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToList();
        Assert.HasCount(ends.Distinct().Count(), ends, "No duplicates expected");
    }

    [TestMethod]
    public void Engine_OneOrMore_Cycle_DoesNotLoop()
    {
        // alice <-> bob  (cycle)
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Alice));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        // bob and carol reachable; alice is a back-edge but NOT included (OneOrMore, visited)
        Assert.Contains(Bob.ToString(), ends);
        Assert.Contains(Carol.ToString(), ends);
    }

    #endregion

    #region Engine — ZeroOrMore (prop*)

    [TestMethod]
    public void Engine_ZeroOrMore_IncludesSelf()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends, "ZeroOrMore should include self");
        Assert.Contains(Dave.ToString(), ends);
    }

    [TestMethod]
    public void Engine_ZeroOrMore_BothVariables_IncludesIdentity()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        List<(string, string)> rows = result.Rows.Cast<DataRow>()
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        // Self-pairs for alice, bob, carol, dave
        Assert.IsTrue(rows.Any(p => p.Item1 == Alice.ToString() && p.Item2 == Alice.ToString()));
        Assert.IsTrue(rows.Any(p => p.Item1 == Dave.ToString()  && p.Item2 == Dave.ToString()));
    }

    #endregion

    #region Engine — ZeroOrOne (prop?)

    [TestMethod]
    public void Engine_ZeroOrOne_IncludesSelf()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends, "ZeroOrOne should include self");
        Assert.Contains(Bob.ToString(), ends,   "ZeroOrOne should include 1-hop");
        Assert.DoesNotContain(Carol.ToString(), ends, "ZeroOrOne should NOT include 2-hops");
    }

    [TestMethod]
    public void Engine_ZeroOrOne_MaxOneHop()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);
        Assert.HasCount(2, result.Rows); // alice (self) + bob (1 hop)
    }

    #endregion

    #region Engine — Cycles in cardinal paths

    [TestMethod]
    public void Engine_ZeroOrMore_Cycle_DoesNotLoop()
    {
        // alice <-> bob (bidirectional cycle), bob -> carol
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Alice));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends, "ZeroOrMore includes self");
        Assert.Contains(Bob.ToString(),   ends);
        Assert.Contains(Carol.ToString(), ends);
        Assert.AreEqual(3, ends.Count, "No duplicates despite cycle");
    }

    [TestMethod]
    public void Engine_ZeroOrOne_Cycle_DoesNotLoop()
    {
        // alice <-> bob (bidirectional cycle)
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Alice));

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        // ZeroOrOne: self (0 hops) + bob (1 hop); alice via back-edge is NOT a new node
        Assert.Contains(Alice.ToString(), ends, "ZeroOrOne includes self");
        Assert.Contains(Bob.ToString(),   ends, "ZeroOrOne includes 1-hop");
        Assert.AreEqual(2, ends.Count, "No extra nodes via back-edge");
    }

    [TestMethod]
    public void Engine_OneOrMore_TriangleCycle_AllNodesReachable()
    {
        // triangle: alice -> bob -> carol -> alice
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows, Alice));

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Bob.ToString(),   ends);
        Assert.Contains(Carol.ToString(), ends);
        // alice is reachable via carol (back-edge closes the triangle)
        Assert.Contains(Alice.ToString(), ends, "OneOrMore reaches alice via carol->alice");
        Assert.AreEqual(3, ends.Count, "Exactly 3 distinct nodes, no infinite loop");
    }

    [TestMethod]
    public void Engine_ZeroOrMore_BothVars_TriangleCycle_FiniteResult()
    {
        // triangle: alice -> bob -> carol -> alice
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows, Alice));

        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        // Each node can reach all 3 nodes (including itself via ZeroOrMore),
        // so we expect exactly 9 pairs (3 sources × 3 targets), finite result.
        List<(string s, string e)> rows = result.Rows.Cast<DataRow>()
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        Assert.AreEqual(9, rows.Count, "3×3 pairs with no duplicates despite triangle cycle");
        Assert.IsTrue(rows.Any(p => p.s == Alice.ToString() && p.e == Alice.ToString()), "alice reaches itself");
        Assert.IsTrue(rows.Any(p => p.s == Bob.ToString()   && p.e == Alice.ToString()), "bob reaches alice via cycle");
        Assert.IsTrue(rows.Any(p => p.s == Carol.ToString() && p.e == Bob.ToString()),   "carol reaches bob via cycle");
    }

    #endregion

    #region Engine — BoundedRange (prop{n,m})

    [TestMethod]
    public void Engine_BoundedRange_Exact2()
    {
        RDFGraph graph = BuildTestGraph();
        // alice knows{2} ?e  => carol (exactly 2 hops)
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(2, 2));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Carol.ToString(), ends);
        Assert.DoesNotContain(Bob.ToString(), ends,  "1-hop not expected");
        Assert.DoesNotContain(Dave.ToString(), ends, "3-hop not expected");
    }

    [TestMethod]
    public void Engine_BoundedRange_1To3()
    {
        RDFGraph graph = BuildTestGraph();
        // alice knows{1,3} ?e  => bob, carol, dave
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(1, 3));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Bob.ToString(), ends);
        Assert.Contains(Carol.ToString(), ends);
        Assert.Contains(Dave.ToString(), ends);
    }

    [TestMethod]
    public void Engine_BoundedRange_ZeroMin_IncludesSelf()
    {
        RDFGraph graph = BuildTestGraph();
        // alice knows{0,2} ?e  => alice, bob, carol
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(0, 2));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends, "0 hops => self");
        Assert.Contains(Bob.ToString(), ends);
        Assert.Contains(Carol.ToString(), ends);
        Assert.DoesNotContain(Dave.ToString(), ends, "3-hop exceeds max=2");
    }

    [TestMethod]
    public void Engine_BoundedRange_EmptyResult_TooFewEdges()
    {
        RDFGraph graph = BuildTestGraph(); // chain length = 3 hops max
        // alice knows{5,7} ?e  => no results (chain too short)
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(5, 7));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);
        Assert.IsEmpty(result.Rows);
    }

    #endregion

    #region Engine — Sequence path with mixed cardinality

    [TestMethod]
    public void Engine_Sequence_StaticThenTransitive()
    {
        // alice parent carol, carol knows dave knows eve
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Parent, Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows,  Dave));
        graph.AddTriple(new RDFTriple(Dave,  Knows,  Eve));

        // alice parent/knows+ ?e  => all nodes reachable from carol via knows+
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Parent))
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Dave.ToString(), ends);
        Assert.Contains(Eve.ToString(), ends);
        Assert.DoesNotContain(Alice.ToString(), ends);
        Assert.DoesNotContain(Carol.ToString(), ends); // parent step target, not knows+ result
    }

    [TestMethod]
    public void Engine_Sequence_TransitiveThenStatic()
    {
        // alice knows+ dave, dave parent eve
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows,  Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Dave));
        graph.AddTriple(new RDFTriple(Dave,  Parent, Eve));

        // ?s knows+/parent ?e
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())
            .AddSequenceStep(new RDFPropertyPathStep(Parent));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        List<(string, string)> rows = result.Rows.Cast<DataRow>()
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        // alice knows+ bob/dave; dave parent eve → alice→eve, bob→eve
        Assert.IsTrue(rows.Any(p => p.Item1 == Alice.ToString() && p.Item2 == Eve.ToString()));
        Assert.IsTrue(rows.Any(p => p.Item1 == Bob.ToString()   && p.Item2 == Eve.ToString()));
    }

    #endregion

    #region Engine — Alternative path with cardinality

    [TestMethod]
    public void Engine_Alternative_OneOrMore()
    {
        // alice has both knows and parent edges directly
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows,  Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Carol));   // knows chain: alice→bob→carol
        graph.AddTriple(new RDFTriple(Alice, Parent, Dave));
        graph.AddTriple(new RDFTriple(Dave,  Parent, Eve));     // parent chain: alice→dave→eve

        // alice (knows+|parent+) ?e  =  knows+ from alice UNION parent+ from alice
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddAlternativeSteps(new List<RDFPropertyPathStep>
            {
                new RDFPropertyPathStep(Knows).OneOrMore(),
                new RDFPropertyPathStep(Parent).OneOrMore()
            });

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r[0].ToString()).ToHashSet();
        Assert.Contains(Bob.ToString(), ends,   "knows 1-hop");
        Assert.Contains(Carol.ToString(), ends, "knows 2-hops");
        Assert.Contains(Dave.ToString(), ends,  "parent 1-hop");
        Assert.Contains(Eve.ToString(), ends,   "parent 2-hops");
    }

    #endregion

    #region Engine — Store datasource

    [TestMethod]
    public void Engine_OneOrMore_OnMemoryStore()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFContext ctx   = new RDFContext("ex:ctx");
        store.AddQuadruple(new RDFQuadruple(ctx, Alice, Knows, Bob));
        store.AddQuadruple(new RDFQuadruple(ctx, Bob,   Knows, Carol));

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, store);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Bob.ToString(), ends);
        Assert.Contains(Carol.ToString(), ends);
    }

    [TestMethod]
    public void Engine_ZeroOrMore_OnMemoryStore_IncludesSelf()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFContext ctx   = new RDFContext("ex:ctx");
        store.AddQuadruple(new RDFQuadruple(ctx, Alice, Knows, Bob));

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, store);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends);
        Assert.Contains(Bob.ToString(), ends);
    }

    #endregion

    #region Engine — Federation datasource

    [TestMethod]
    public void Engine_OneOrMore_Federation_ChainSplitAcrossTwoGraphs()
    {
        // Chain split: alice→bob in graph1, bob→carol→dave in graph2.
        // The transitive BFS must merge both sources to reach the full chain.
        RDFGraph graph1 = new RDFGraph();
        graph1.AddTriple(new RDFTriple(Alice, Knows, Bob));

        RDFGraph graph2 = new RDFGraph();
        graph2.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        graph2.AddTriple(new RDFTriple(Carol, Knows, Dave));

        RDFFederation federation = new RDFFederation()
            .AddGraph(graph1)
            .AddGraph(graph2);

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, federation);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Bob.ToString(), ends,   "bob — 1 hop");
        Assert.Contains(Carol.ToString(), ends, "carol — 2 hops via graph2");
        Assert.Contains(Dave.ToString(), ends,  "dave — 3 hops via graph2");
        Assert.DoesNotContain(Alice.ToString(), ends, "alice must not appear (OneOrMore)");
    }

    [TestMethod]
    public void Engine_ZeroOrMore_Federation_HeterogeneousSources_IncludesSelf()
    {
        // Heterogeneous federation: one RDFGraph + one RDFMemoryStore.
        // ZeroOrMore must include the start node (zero-step case) even when data is spread.
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));

        RDFMemoryStore store = new RDFMemoryStore();
        RDFContext ctx   = new RDFContext("ex:ctx");
        store.AddQuadruple(new RDFQuadruple(ctx, Bob, Knows, Carol));

        RDFFederation federation = new RDFFederation()
            .AddGraph(graph)
            .AddStore(store);

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, federation);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends, "alice — zero hops (self)");
        Assert.Contains(Bob.ToString(), ends,   "bob — from graph source");
        Assert.Contains(Carol.ToString(), ends, "carol — from store source");
    }

    [TestMethod]
    public void Engine_BoundedRange_Federation_NestedFederation_ExactHops()
    {
        // Bounded range {2,3}: only nodes reachable in 2 or 3 hops must appear.
        // Data is nested inside a sub-federation to exercise the nested-federation code path.
        // Chain: alice→bob(1)→carol(2)→dave(3)→eve(4)
        RDFGraph innerGraph = new RDFGraph();
        innerGraph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        innerGraph.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        RDFGraph outerGraph = new RDFGraph();
        outerGraph.AddTriple(new RDFTriple(Carol, Knows, Dave));
        outerGraph.AddTriple(new RDFTriple(Dave,  Knows, Eve));

        RDFFederation federation = new RDFFederation()
            .AddFederation(new RDFFederation().AddGraph(innerGraph))
            .AddGraph(outerGraph);

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(2, 3));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, federation);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.DoesNotContain(Bob.ToString(), ends,  "bob — 1 hop, below range");
        Assert.Contains(Carol.ToString(), ends, "carol — 2 hops, in range");
        Assert.Contains(Dave.ToString(), ends,  "dave — 3 hops, in range");
        Assert.DoesNotContain(Eve.ToString(), ends,  "eve — 4 hops, above range");
    }

    [TestMethod]
    public void Engine_OneOrMore_InverseStep_Federation_MultiSource()
    {
        // Inverse path (^knows+) navigates edges in reverse across two graph sources.
        // dave ^knows+ ?e in a federation must reach carol, bob, alice.
        RDFGraph graph1 = new RDFGraph();
        graph1.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph1.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        RDFGraph graph2 = new RDFGraph();
        graph2.AddTriple(new RDFTriple(Carol, Knows, Dave));

        RDFFederation federation = new RDFFederation()
            .AddGraph(graph1)
            .AddGraph(graph2);

        RDFPropertyPath path = new RDFPropertyPath(Dave, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, federation);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Carol.ToString(), ends, "carol — 1 reverse hop (graph2)");
        Assert.Contains(Bob.ToString(), ends,   "bob — 2 reverse hops (graph1)");
        Assert.Contains(Alice.ToString(), ends, "alice — 3 reverse hops (graph1)");
        Assert.DoesNotContain(Dave.ToString(), ends,  "dave must not appear (OneOrMore)");
    }

    #endregion

    #region SPARQL printer

    [TestMethod]
    public void Printer_SingleStep_ZeroOrOne()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>?") || printed.Contains("knows?"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_OneOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>+") || printed.Contains("knows+"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_ZeroOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>*") || printed.Contains("knows*"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_BoundedRange()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(2, 4));
        string printed = path.ToString();
        Assert.Contains("{2,4}", printed, $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_BoundedRange_ExactCount()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(3, 3));
        string printed = path.ToString();
        Assert.Contains("{3}", printed, $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_InverseStep_OneOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains('^') && (printed.Contains("knows>+") || printed.Contains("knows+")), $"Printed: {printed}");
    }

    #endregion

    #region SPARQL

    [TestMethod]
    public void SelectQuery_OneOrMore_ReturnsTransitiveClosure()
    {
        RDFGraph graph = BuildTestGraph();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        RDFSelectQueryResult result = query.ApplyToGraph(graph);
        Assert.IsGreaterThanOrEqualTo(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void SelectQuery_ZeroOrMore_IncludesSelf()
    {
        RDFGraph graph = BuildTestGraph();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        RDFSelectQueryResult result = query.ApplyToGraph(graph);
        HashSet<string> ends = result.SelectResults.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends);
    }

    [TestMethod]
    public void SelectQuery_ZeroOrOne_BothVars()
    {
        RDFGraph graph = BuildTestGraph();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne())));

        RDFSelectQueryResult result = query.ApplyToGraph(graph);
        // Self-pairs included + direct knows pairs
        Assert.IsGreaterThan(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void SelectQuery_BoundedRange_Exact()
    {
        RDFGraph graph = BuildTestGraph();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(2, 2))));

        RDFSelectQueryResult result = query.ApplyToGraph(graph);
        Assert.AreEqual(1, result.SelectResultsCount); // only carol
    }

    [TestMethod]
    public void AskQuery_OneOrMore_True()
    {
        RDFGraph graph = BuildTestGraph();
        RDFAskQuery query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, Dave)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        RDFAskQueryResult result = query.ApplyToGraph(graph);
        Assert.IsTrue(result.AskResult);
    }

    [TestMethod]
    public void AskQuery_OneOrMore_False()
    {
        RDFGraph graph = BuildTestGraph();
        RDFAskQuery query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Dave, Alice)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        RDFAskQueryResult result = query.ApplyToGraph(graph);
        Assert.IsFalse(result.AskResult);
    }

    [TestMethod]
    public void AskQuery_ZeroOrMore_SelfTrue()
    {
        RDFGraph graph = BuildTestGraph();
        RDFAskQuery query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, Alice)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        RDFAskQueryResult result = query.ApplyToGraph(graph);
        Assert.IsTrue(result.AskResult); // zero hops: alice -> alice
    }

    [TestMethod]
    public void SelectQuery_OneOrMore_WithAdditionalPattern()
    {
        // Graph: alice type Person, alice knows bob knows carol
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Alice, Type, Person));
        graph.AddTriple(new RDFTriple(Bob,   Type, Person));

        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(VarS, Type, Person))
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        RDFSelectQueryResult result = query.ApplyToGraph(graph);
        // alice's reachable: bob, carol, dave; bob's reachable: carol, dave
        Assert.IsGreaterThanOrEqualTo(5, result.SelectResultsCount);
    }

    #endregion

    #region SPARQL UPDATE

    // ─────────────────────────────────────────────────────────────────
    // INSERT WHERE — prop+
    // ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void InsertWhere_OneOrMore_ForwardChain()
    {
        // WHERE { alice ex:knows+ ?e } INSERT { alice ex:reached ?e }
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        // bob, carol, dave should be tagged as reached
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Alice))); // no self
    }

    [TestMethod]
    public void InsertWhere_OneOrMore_BothVariables()
    {
        // WHERE { ?s ex:knows+ ?e } INSERT { ?s ex:ancestor ?e }
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(VarS, Ancestor, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Ancestor, Dave)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Ancestor, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Bob,   Ancestor, Dave)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Bob,   Ancestor, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Carol, Ancestor, Dave)));
    }

    [TestMethod]
    public void InsertWhere_ZeroOrMore_IncludesSelf()
    {
        // WHERE { alice ex:knows* ?e } INSERT { alice ex:related ?e }
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Related, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Dave)));
    }

    [TestMethod]
    public void InsertWhere_ZeroOrOne_OnlyOneHop()
    {
        // WHERE { alice ex:knows? ?e } INSERT { alice ex:tag ?e }
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice))); // 0 hops
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));   // 1 hop
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));// no 2-hops
    }

    [TestMethod]
    public void InsertWhere_BoundedRange_Exact2()
    {
        // WHERE { alice ex:knows{2,2} ?e } INSERT { alice ex:tag ?e }
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(2, 2))));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Dave)));
    }

    [TestMethod]
    public void InsertWhere_BoundedRange_1To2()
    {
        // WHERE { alice ex:knows{1,2} ?e } INSERT { alice ex:tag ?e }
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(1, 2))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Dave)));
    }

    [TestMethod]
    public void InsertWhere_OneOrMore_Inverse()
    {
        // WHERE { dave ^ex:knows+ ?e } INSERT { dave ex:ancestor ?e }
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Dave, Ancestor, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Dave, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Alice)));
    }

    [TestMethod]
    public void InsertWhere_NoMatchingPath_NoInsert()
    {
        // WHERE { dave ex:knows+ ?e } — dave has no outgoing knows
        RDFGraph graph = BuildTestGraph();
        long before = graph.TriplesCount;
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Dave, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Dave, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);
        Assert.AreEqual(before, graph.TriplesCount);
    }

    // ─────────────────────────────────────────────────────────────────
    // DELETE WHERE — prop+
    // ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void DeleteWhere_OneOrMore_RemovesReachableEdges()
    {
        // Anchor graph: alice reached bob, carol, dave  (already inserted)
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Alice, Reached, Bob));
        graph.AddTriple(new RDFTriple(Alice, Reached, Carol));
        graph.AddTriple(new RDFTriple(Alice, Reached, Dave));

        // WHERE { alice ex:knows+ ?e } DELETE { alice ex:reached ?e }
        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
        // Original knows chain should remain
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Knows, Bob)));
    }

    [TestMethod]
    public void DeleteWhere_ZeroOrMore_RemovesSelfAndChain()
    {
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Alice, Related, Alice));
        graph.AddTriple(new RDFTriple(Alice, Related, Bob));
        graph.AddTriple(new RDFTriple(Alice, Related, Carol));

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Related, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Related, Alice)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Related, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Related, Carol)));
    }

    [TestMethod]
    public void DeleteWhere_ZeroOrOne_OnlyRemovesUpToOneHop()
    {
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Alice, Tag, Alice));
        graph.AddTriple(new RDFTriple(Alice, Tag, Bob));
        graph.AddTriple(new RDFTriple(Alice, Tag, Carol));

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol))); // not matched (2 hops)
    }

    [TestMethod]
    public void DeleteWhere_BoundedRange_Exact2()
    {
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Alice, Tag, Bob));
        graph.AddTriple(new RDFTriple(Alice, Tag, Carol));
        graph.AddTriple(new RDFTriple(Alice, Tag, Dave));

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(2, 2))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));   // 1-hop, not deleted
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));// 2-hop, deleted
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Dave)));  // 3-hop, not deleted
    }

    [TestMethod]
    public void DeleteWhere_OneOrMore_Inverse_RemovesAncestors()
    {
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Dave, Ancestor, Carol));
        graph.AddTriple(new RDFTriple(Dave, Ancestor, Bob));
        graph.AddTriple(new RDFTriple(Dave, Ancestor, Alice));

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Dave, Ancestor, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Dave, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Dave, Ancestor, Alice)));
    }

    [TestMethod]
    public void DeleteWhere_NoMatch_GraphUnchanged()
    {
        RDFGraph graph = BuildTestGraph();
        long before = graph.TriplesCount;

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Eve, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Eve, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);
        Assert.AreEqual(before, graph.TriplesCount);
    }

    [TestMethod]
    public void DeleteWhere_OneOrMore_BothVariables()
    {
        RDFGraph graph = BuildTestGraph();
        // Pre-populate ancestor triples
        graph.AddTriple(new RDFTriple(Alice, Ancestor, Bob));
        graph.AddTriple(new RDFTriple(Alice, Ancestor, Carol));
        graph.AddTriple(new RDFTriple(Alice, Ancestor, Dave));
        graph.AddTriple(new RDFTriple(Bob,   Ancestor, Carol));
        graph.AddTriple(new RDFTriple(Bob,   Ancestor, Dave));
        graph.AddTriple(new RDFTriple(Carol, Ancestor, Dave));

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(VarS, Ancestor, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Ancestor, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Carol, Ancestor, Dave)));
    }

    // ─────────────────────────────────────────────────────────────────
    // DELETE/INSERT WHERE
    // ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void DeleteInsertWhere_OneOrMore_MovesReachable()
    {
        // Reached triples exist; after op delete Reached, insert Tagged for all reachable via knows+
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Alice, Reached, Bob));
        graph.AddTriple(new RDFTriple(Alice, Reached, Carol));

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddInsertTemplate(new RDFPattern(Alice, Tagged, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Dave)));
    }

    [TestMethod]
    public void DeleteInsertWhere_ZeroOrMore_IncludesSelfInInsert()
    {
        RDFGraph graph = BuildTestGraph();

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Related, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Related, Dave)));
    }

    [TestMethod]
    public void DeleteInsertWhere_ZeroOrOne_LimitedScope()
    {
        RDFGraph graph = BuildTestGraph();

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
    }

    [TestMethod]
    public void DeleteInsertWhere_BoundedRange_DeleteOnlyInRange()
    {
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Alice, Reached, Bob));
        graph.AddTriple(new RDFTriple(Alice, Reached, Carol));
        graph.AddTriple(new RDFTriple(Alice, Reached, Dave));

        // DELETE { alice reached ?e } INSERT { alice tagged ?e } WHERE { alice knows{1,2} ?e }
        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation()
            .AddDeleteTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddInsertTemplate(new RDFPattern(Alice, Tagged, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(1, 2))));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));   // in {1,2} → deleted
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol))); // in {1,2} → deleted
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));   // out of {1,2} → kept
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tagged, Dave)));
    }

    [TestMethod]
    public void DeleteInsertWhere_OneOrMore_OnStore()
    {
        RDFMemoryStore store = new RDFMemoryStore();
        RDFContext ctx   = new RDFContext("ex:ctx");
        store.AddQuadruple(new RDFQuadruple(ctx, Alice, Knows, Bob));
        store.AddQuadruple(new RDFQuadruple(ctx, Bob,   Knows, Carol));

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToStore(store);

        bool reachedBob   = store.ContainsQuadruple(new RDFQuadruple(ctx, Alice, Reached, Bob));
        bool reachedCarol = store.ContainsQuadruple(new RDFQuadruple(ctx, Alice, Reached, Carol));
        Assert.IsTrue(reachedBob   || store.SelectQuadruples(s: Alice, p: Reached, o: Bob).Any());
        Assert.IsTrue(reachedCarol || store.SelectQuadruples(s: Alice, p: Reached, o: Carol).Any());
    }

    // ─────────────────────────────────────────────────────────────────
    // Complex WHERE clauses (patterns + path)
    // ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void InsertWhere_PatternPlusTransitivePath_JoinedCorrectly()
    {
        // Graph: alice parent bob, bob knows carol knows dave
        // WHERE { ?s ex:parent ?x . ?x ex:knows+ ?e } INSERT { ?s ex:reached ?e }
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Parent, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows,  Dave));

        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(VarS, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(VarS, Parent, VarX))
                .AddPropertyPath(new RDFPropertyPath(VarX, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob))); // bob is ?x, not ?e
    }

    [TestMethod]
    public void DeleteWhere_PatternPlusTransitivePath_DeletesCorrectly()
    {
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Parent, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows,  Dave));
        graph.AddTriple(new RDFTriple(Alice, Reached, Carol));
        graph.AddTriple(new RDFTriple(Alice, Reached, Dave));
        graph.AddTriple(new RDFTriple(Bob,   Reached, Alice)); // unrelated

        RDFDeleteWhereOperation op = new RDFDeleteWhereOperation()
            .AddDeleteTemplate(new RDFPattern(VarS, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(VarS, Parent, VarX))
                .AddPropertyPath(new RDFPropertyPath(VarX, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Bob, Reached, Alice))); // untouched
    }

    [TestMethod]
    public void InsertWhere_TransitivePath_WithFilter()
    {
        // WHERE { ?s ex:knows+ ?e FILTER(sameTerm(?s, alice)) } INSERT { alice ex:reached ?e }
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore()))
                .AddFilter(new RDFExpressionFilter(
                    new RDFComparisonExpression(
                        RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                        new RDFVariableExpression(VarS),
                        new RDFConstantExpression(Alice)))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
    }

    [TestMethod]
    public void InsertWhere_BoundedRange_ZeroMin_IncludesSelf()
    {
        RDFGraph graph = BuildTestGraph();
        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Tag, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).BoundedRange(0, 1))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
    }

    [TestMethod]
    public void InsertWhere_OneOrMore_DoesNotInsertDuplicates()
    {
        // Pre-insert alice reached bob; re-run op; triple count should be unchanged on bob
        RDFGraph graph = BuildTestGraph();
        graph.AddTriple(new RDFTriple(Alice, Reached, Bob));
        long before = graph.TriplesCount;

        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        // Graph semantics prevent duplicate triples; count should reflect unique triples
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
    }

    [TestMethod]
    public void DeleteInsertWhere_TransitiveSequence_Correct()
    {
        // alice parent bob, bob knows+ carol/dave
        // DELETE { ?s ex:tag ?e } INSERT { ?s ex:reached ?e }
        // WHERE { ?s ex:parent ?x . ?x ex:knows+ ?e }
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Parent, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows,  Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows,  Dave));
        graph.AddTriple(new RDFTriple(Alice, Tag, Carol));
        graph.AddTriple(new RDFTriple(Alice, Tag, Dave));

        RDFDeleteInsertWhereOperation op = new RDFDeleteInsertWhereOperation()
            .AddDeleteTemplate(new RDFPattern(VarS, Tag, VarE))
            .AddInsertTemplate(new RDFPattern(VarS, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPattern(new RDFPattern(VarS, Parent, VarX))
                .AddPropertyPath(new RDFPropertyPath(VarX, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Dave)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
    }

    [TestMethod]
    public void InsertWhere_ZeroOrMore_Cycle_FinitesResult()
    {
        // alice <-> bob cycle; insert reached for all reachable via ZeroOrMore
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Alice));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Carol));

        RDFInsertWhereOperation op = new RDFInsertWhereOperation()
            .AddInsertTemplate(new RDFPattern(Alice, Reached, VarE))
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrMore())));

        op.ApplyToGraph(graph); // should terminate, not loop

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Carol)));
    }

    #endregion

    #region Ontology — RDFS subsumption (rdf:type / rdfs:subClassOf*)
    // =========================================================================
    //
    // Ontology shared by all tests in this region
    // ─────────────────────────────────────────────────────────────────────────
    // 16-level class hierarchy (bottom → top via rdfs:subClassOf):
    //
    //   L0  Person
    //   L1  CognitiveAgent
    //   L2  ModernHuman
    //   L3  HomoSapiens
    //   L4  Hominid
    //   L5  Primate
    //   L6  Mammal
    //   L7  Vertebrate
    //   L8  Chordate
    //   L9  Animal
    //   L10 Organism
    //   L11 LivingThing
    //   L12 PhysicalObject
    //   L13 Object
    //   L14 Entity
    //   L15 Thing (root)
    //
    // owl:equivalentClass sideways bridges:
    //   HumanBeing  ≡ Person          (used by dave)
    //   SocialAnimal ≡ CognitiveAgent (used by inverse tests)
    //
    // Named individuals:
    //   alice  rdf:type Person        (L0)
    //   bob    rdf:type Mammal        (L6 — enters mid-hierarchy)
    //   carol  rdf:type HomoSapiens   (L3)
    //   dave   rdf:type HumanBeing    (reaches Person via equivalentClass)
    //   eve    rdf:type Animal        (L9 — superclass of Mammal, not below)
    // ─────────────────────────────────────────────────────────────────────────

    private static readonly RDFResource SubClassOf = RDFVocabulary.RDFS.SUB_CLASS_OF;
    private static readonly RDFResource EquivClass  = RDFVocabulary.OWL.EQUIVALENT_CLASS;

    private static RDFGraph BuildSubsumptionGraph()
    {
        // Class nodes
        RDFResource cPerson         = new RDFResource("ex:Person");
        RDFResource cCognitiveAgent = new RDFResource("ex:CognitiveAgent");
        RDFResource cModernHuman    = new RDFResource("ex:ModernHuman");
        RDFResource cHomoSapiens    = new RDFResource("ex:HomoSapiens");
        RDFResource cHominid        = new RDFResource("ex:Hominid");
        RDFResource cPrimate        = new RDFResource("ex:Primate");
        RDFResource cMammal         = new RDFResource("ex:Mammal");
        RDFResource cVertebrate     = new RDFResource("ex:Vertebrate");
        RDFResource cChordate       = new RDFResource("ex:Chordate");
        RDFResource cAnimal         = new RDFResource("ex:Animal");
        RDFResource cOrganism       = new RDFResource("ex:Organism");
        RDFResource cLivingThing    = new RDFResource("ex:LivingThing");
        RDFResource cPhysicalObject = new RDFResource("ex:PhysicalObject");
        RDFResource cObject         = new RDFResource("ex:Object");
        RDFResource cEntity         = new RDFResource("ex:Entity");
        RDFResource cThing          = new RDFResource("ex:Thing");
        RDFResource cHumanBeing     = new RDFResource("ex:HumanBeing");
        RDFResource cSocialAnimal   = new RDFResource("ex:SocialAnimal");

        RDFGraph g = new RDFGraph();

        // 15 rdfs:subClassOf edges (L0 → L15)
        g.AddTriple(new RDFTriple(cPerson,         SubClassOf, cCognitiveAgent));   // L0→L1
        g.AddTriple(new RDFTriple(cCognitiveAgent, SubClassOf, cModernHuman));      // L1→L2
        g.AddTriple(new RDFTriple(cModernHuman,    SubClassOf, cHomoSapiens));      // L2→L3
        g.AddTriple(new RDFTriple(cHomoSapiens,    SubClassOf, cHominid));          // L3→L4
        g.AddTriple(new RDFTriple(cHominid,        SubClassOf, cPrimate));          // L4→L5
        g.AddTriple(new RDFTriple(cPrimate,        SubClassOf, cMammal));           // L5→L6
        g.AddTriple(new RDFTriple(cMammal,         SubClassOf, cVertebrate));       // L6→L7
        g.AddTriple(new RDFTriple(cVertebrate,     SubClassOf, cChordate));         // L7→L8
        g.AddTriple(new RDFTriple(cChordate,       SubClassOf, cAnimal));           // L8→L9
        g.AddTriple(new RDFTriple(cAnimal,         SubClassOf, cOrganism));         // L9→L10
        g.AddTriple(new RDFTriple(cOrganism,       SubClassOf, cLivingThing));      // L10→L11
        g.AddTriple(new RDFTriple(cLivingThing,    SubClassOf, cPhysicalObject));   // L11→L12
        g.AddTriple(new RDFTriple(cPhysicalObject, SubClassOf, cObject));           // L12→L13
        g.AddTriple(new RDFTriple(cObject,         SubClassOf, cEntity));           // L13→L14
        g.AddTriple(new RDFTriple(cEntity,         SubClassOf, cThing));            // L14→L15

        // owl:equivalentClass bridges
        g.AddTriple(new RDFTriple(cHumanBeing,   EquivClass, cPerson));         // HumanBeing ≡ Person
        g.AddTriple(new RDFTriple(cSocialAnimal, EquivClass, cCognitiveAgent)); // SocialAnimal ≡ CognitiveAgent

        // Named individuals
        g.AddTriple(new RDFTriple(Alice, Type, cPerson));       // alice  rdf:type Person       (L0)
        g.AddTriple(new RDFTriple(Bob,   Type, cMammal));       // bob    rdf:type Mammal        (L6)
        g.AddTriple(new RDFTriple(Carol, Type, cHomoSapiens));  // carol  rdf:type HomoSapiens   (L3)
        g.AddTriple(new RDFTriple(Dave,  Type, cHumanBeing));   // dave   rdf:type HumanBeing    (→Person via equiv)
        g.AddTriple(new RDFTriple(Eve,   Type, cAnimal));       // eve    rdf:type Animal         (L9, above Mammal)

        return g;
    }

    // ── Test 1 ───────────────────────────────────────────────────────────────
    // Canonical RDFS subsumption: type/subClassOf* must surface all 16 levels.
    [TestMethod]
    public void Subsumption_Canonical_TypeSubClassOfStar_Covers16Levels()
    {
        // alice rdf:type/rdfs:subClassOf* ?class
        // ZeroOrMore includes Person itself at 0 hops, then climbs to Thing at hop 15.
        RDFGraph graph = BuildSubsumptionGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Type))
            .AddSequenceStep(new RDFPropertyPathStep(SubClassOf).ZeroOrMore());

        HashSet<string> classes = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString())
            .ToHashSet();

        // All 16 levels present
        Assert.Contains("ex:Person", classes,         "L0  Person (zero hops of subClassOf)");
        Assert.Contains("ex:CognitiveAgent", classes, "L1  CognitiveAgent");
        Assert.Contains("ex:ModernHuman", classes,    "L2  ModernHuman");
        Assert.Contains("ex:HomoSapiens", classes,    "L3  HomoSapiens");
        Assert.Contains("ex:Hominid", classes,        "L4  Hominid");
        Assert.Contains("ex:Primate", classes,        "L5  Primate");
        Assert.Contains("ex:Mammal", classes,         "L6  Mammal");
        Assert.Contains("ex:Vertebrate", classes,     "L7  Vertebrate");
        Assert.Contains("ex:Chordate", classes,       "L8  Chordate");
        Assert.Contains("ex:Animal", classes,         "L9  Animal");
        Assert.Contains("ex:Organism", classes,       "L10 Organism");
        Assert.Contains("ex:LivingThing", classes,    "L11 LivingThing");
        Assert.Contains("ex:PhysicalObject", classes, "L12 PhysicalObject");
        Assert.Contains("ex:Object", classes,         "L13 Object");
        Assert.Contains("ex:Entity", classes,         "L14 Entity");
        Assert.Contains("ex:Thing", classes,          "L15 Thing (root)");

        // Exactly 16 — no spurious classes (individuals or unrelated resources)
        Assert.HasCount(16, classes, "Exactly 16 distinct classes, no spurious results");
        Assert.DoesNotContain(Alice.ToString(), classes, "alice is an individual, not a class");
    }

    // ── Test 2 ───────────────────────────────────────────────────────────────
    // Bounded range: only the 6 classes sitting at hops 5–10 from Person.
    [TestMethod]
    public void Subsumption_BoundedRange_TypeSubClassOf_5to10_MidHierarchy()
    {
        // alice rdf:type/rdfs:subClassOf{5,10} ?class
        // type lands on Person (L0); subClassOf{5,10} then selects exactly
        // hops 5 (Primate) … 10 (Organism), skipping both the shallow top
        // of the human clade and the deep metazoan root.
        RDFGraph graph = BuildSubsumptionGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Type))
            .AddSequenceStep(new RDFPropertyPathStep(SubClassOf).BoundedRange(5, 10));

        HashSet<string> classes = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString())
            .ToHashSet();

        // Inside window [5,10]
        Assert.Contains("ex:Primate", classes,     "L5  Primate  — hop 5 (lower bound)");
        Assert.Contains("ex:Mammal", classes,      "L6  Mammal   — hop 6");
        Assert.Contains("ex:Vertebrate", classes,  "L7  Vertebrate — hop 7");
        Assert.Contains("ex:Chordate", classes,    "L8  Chordate — hop 8");
        Assert.Contains("ex:Animal", classes,      "L9  Animal   — hop 9");
        Assert.Contains("ex:Organism", classes,    "L10 Organism — hop 10 (upper bound)");

        // Too shallow (hops 0–4)
        Assert.DoesNotContain("ex:Person", classes,         "L0 hop 0 — below range");
        Assert.DoesNotContain("ex:CognitiveAgent", classes, "L1 hop 1 — below range");
        Assert.DoesNotContain("ex:ModernHuman", classes,    "L2 hop 2 — below range");
        Assert.DoesNotContain("ex:HomoSapiens", classes,    "L3 hop 3 — below range");
        Assert.DoesNotContain("ex:Hominid", classes,        "L4 hop 4 — below range");

        // Too deep (hops 11–15)
        Assert.DoesNotContain("ex:LivingThing", classes,    "L11 hop 11 — above range");
        Assert.DoesNotContain("ex:PhysicalObject", classes, "L12 hop 12 — above range");
        Assert.DoesNotContain("ex:Object", classes,         "L13 hop 13 — above range");
        Assert.DoesNotContain("ex:Entity", classes,         "L14 hop 14 — above range");
        Assert.DoesNotContain("ex:Thing", classes,          "L15 hop 15 — above range");

        Assert.HasCount(6, classes, "Exactly 6 classes in window [5,10]");
    }

    // ── Test 3 ───────────────────────────────────────────────────────────────
    // Inverse step: ^subClassOf* navigates DOWN the hierarchy from Mammal.
    [TestMethod]
    public void Subsumption_Inverse_AllSubClassesOf_Mammal()
    {
        // ex:Mammal ^rdfs:subClassOf* ?sub
        // Inverse ZeroOrMore descends from Mammal (L6) towards the leaf.
        // Expected: Mammal itself + 6 transitive subclasses = 7 nodes total.
        // Classes above Mammal (Vertebrate … Thing) must not appear.
        RDFGraph graph = BuildSubsumptionGraph();
        RDFPropertyPath path = new RDFPropertyPath(new RDFResource("ex:Mammal"), VarE)
            .AddSequenceStep(new RDFPropertyPathStep(SubClassOf).Inverse().ZeroOrMore());

        HashSet<string> subs = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString())
            .ToHashSet();

        // Mammal itself (zero hops)
        Assert.Contains("ex:Mammal", subs,         "Mammal itself (0 hops)");
        // Transitive subclasses descending from L6 towards L0
        Assert.Contains("ex:Primate", subs,        "Primate — 1 hop down");
        Assert.Contains("ex:Hominid", subs,        "Hominid — 2 hops down");
        Assert.Contains("ex:HomoSapiens", subs,    "HomoSapiens — 3 hops down");
        Assert.Contains("ex:ModernHuman", subs,    "ModernHuman — 4 hops down");
        Assert.Contains("ex:CognitiveAgent", subs, "CognitiveAgent — 5 hops down");
        Assert.Contains("ex:Person", subs,         "Person — 6 hops down (leaf)");

        // Superclasses of Mammal must not appear
        Assert.DoesNotContain("ex:Vertebrate", subs,    "Vertebrate is a superclass of Mammal");
        Assert.DoesNotContain("ex:Chordate", subs,      "Chordate is a superclass of Mammal");
        Assert.DoesNotContain("ex:Animal", subs,        "Animal is a superclass of Mammal");
        Assert.DoesNotContain("ex:Thing", subs,         "Thing is the taxonomy root");
        // bob has rdf:type Mammal but is an individual, not reachable via ^subClassOf
        Assert.DoesNotContain(Bob.ToString(), subs,     "bob is an individual, not a class node");

        Assert.HasCount(7, subs, "Exactly 7 nodes: Mammal + 6 transitive subclasses");
    }

    // ── Test 4 ───────────────────────────────────────────────────────────────
    // Multi-step sequence bridging an equivalentClass link before the deep hierarchy.
    [TestMethod]
    public void Subsumption_Sequence_TypeThroughEquivalence_ReachesFullHierarchy()
    {
        // dave rdf:type/owl:equivalentClass/rdfs:subClassOf* ?class
        // 3-step sequence mixing ExactlyOne × ExactlyOne × ZeroOrMore:
        //   dave ─type─► HumanBeing ─equivalentClass─► Person ─subClassOf*─► …Thing
        // Validates that the engine correctly threads a non-transitive bridge
        // step before entering the 16-level transitive closure.
        RDFGraph graph = BuildSubsumptionGraph();
        RDFPropertyPath path = new RDFPropertyPath(Dave, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Type))
            .AddSequenceStep(new RDFPropertyPathStep(EquivClass))
            .AddSequenceStep(new RDFPropertyPathStep(SubClassOf).ZeroOrMore());

        HashSet<string> classes = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString())
            .ToHashSet();

        // ZeroOrMore starts at Person (0 hops) and climbs to Thing (15 hops)
        Assert.Contains("ex:Person", classes,         "Person — entry of subClassOf* (0 hops)");
        Assert.Contains("ex:CognitiveAgent", classes, "CognitiveAgent — L1");
        Assert.Contains("ex:HomoSapiens", classes,    "HomoSapiens — L3");
        Assert.Contains("ex:Hominid", classes,        "Hominid — L4");
        Assert.Contains("ex:Mammal", classes,         "Mammal — L6");
        Assert.Contains("ex:Animal", classes,         "Animal — L9");
        Assert.Contains("ex:LivingThing", classes,    "LivingThing — L11");
        Assert.Contains("ex:Thing", classes,          "Thing — L15 (root)");

        Assert.HasCount(16, classes, "All 16 superclasses reachable via the equivalence bridge");

        // The equivalence-bridge intermediate must NOT bleed into the result
        Assert.DoesNotContain("ex:HumanBeing", classes, "HumanBeing is an intermediate, not an endpoint");
        Assert.DoesNotContain(Dave.ToString(), classes,  "dave is the subject individual");
    }

    // ── Test 5 ───────────────────────────────────────────────────────────────
    // Optional step: subClassOf? limits the climb to 0 or 1 hop only.
    [TestMethod]
    public void Subsumption_Optional_TypeSubClassOfZeroOrOne_OnlyDirectAndImmediate()
    {
        // carol rdf:type/rdfs:subClassOf? ?class
        // carol has asserted type HomoSapiens (L3).
        // subClassOf? = 0 hops → HomoSapiens itself
        //              1 hop  → Hominid (L4, immediate superclass)
        // Everything at L5 and above must be absent.
        RDFGraph graph = BuildSubsumptionGraph();
        RDFPropertyPath path = new RDFPropertyPath(Carol, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Type))
            .AddSequenceStep(new RDFPropertyPathStep(SubClassOf).ZeroOrOne());

        HashSet<string> classes = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString())
            .ToHashSet();

        Assert.Contains("ex:HomoSapiens", classes, "HomoSapiens — 0 hops (direct type)");
        Assert.Contains("ex:Hominid", classes,     "Hominid — 1 hop (immediate superclass)");

        // Excluded because they are ≥ 2 hops away
        Assert.DoesNotContain("ex:Primate", classes,        "Primate — 2 hops, cut off by ?");
        Assert.DoesNotContain("ex:Mammal", classes,         "Mammal — 3 hops, cut off by ?");
        Assert.DoesNotContain("ex:Animal", classes,         "Animal — 6 hops, cut off by ?");
        Assert.DoesNotContain("ex:LivingThing", classes,    "LivingThing — 8 hops, cut off by ?");
        Assert.DoesNotContain("ex:Thing", classes,          "Thing — 12 hops, cut off by ?");
        Assert.DoesNotContain(Carol.ToString(), classes,    "carol is an individual, not a class");

        Assert.HasCount(2, classes, "Exactly 2 classes with subClassOf?");
    }

    // ── Test 6 ───────────────────────────────────────────────────────────────
    // Full SELECT query: all instances of Mammal or any of its subclasses.
    [TestMethod]
    public void Subsumption_SelectQuery_AllInstancesOfMammalOrBelow()
    {
        // SELECT ?inst WHERE { ?inst rdf:type/rdfs:subClassOf* ex:Mammal }
        //
        // The path has a concrete end-node (ex:Mammal), so only individuals
        // whose asserted type is Mammal or a subclass thereof are returned.
        //
        // alice  rdf:type Person (L0)      → Person subClassOf* Mammal (6 hops up) ✓
        // bob    rdf:type Mammal (L6)      → Mammal subClassOf* Mammal (0 hops)    ✓
        // carol  rdf:type HomoSapiens (L3) → HomoSapiens subClassOf* Mammal (3 hops) ✓
        // dave   rdf:type HumanBeing       → HumanBeing has no subClassOf edge      ✗
        // eve    rdf:type Animal (L9)      → Animal is a SUPERCLASS of Mammal       ✗
        RDFGraph graph = BuildSubsumptionGraph();
        RDFResource cMammal = new RDFResource("ex:Mammal");

        RDFSelectQueryResult result = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(VarS, cMammal)
                    .AddSequenceStep(new RDFPropertyPathStep(Type))
                    .AddSequenceStep(new RDFPropertyPathStep(SubClassOf).ZeroOrMore())))
            .ApplyToGraph(graph);

        HashSet<string> instances = result.SelectResults.Rows.Cast<DataRow>()
            .Select(r => r["?S"].ToString())
            .ToHashSet();

        // Must find the three Mammal-or-below individuals
        Assert.Contains(Alice.ToString(), instances, "alice — type Person, subClassOf* reaches Mammal");
        Assert.Contains(Bob.ToString(), instances,   "bob — type Mammal, ZeroOrMore includes self");
        Assert.Contains(Carol.ToString(), instances, "carol — type HomoSapiens, subClassOf* reaches Mammal");

        // Must not find individuals whose type is above Mammal or unrelated
        Assert.DoesNotContain(Dave.ToString(), instances, "dave — type HumanBeing, no subClassOf chain to Mammal");
        Assert.DoesNotContain(Eve.ToString(), instances,  "eve — type Animal, superclass of Mammal");

        Assert.AreEqual(3, result.SelectResultsCount, "Exactly 3 instances of Mammal or its subclasses");
    }

    #endregion

    #endregion
}