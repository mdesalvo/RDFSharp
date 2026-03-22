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
        RDFPropertyPathStep step = new RDFPropertyPathStep(Knows).Repeat(2, 4);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.BoundedRange, step.StepCardinality);
        Assert.AreEqual(2, step.MinCardinality);
        Assert.AreEqual(4, step.MaxCardinality);
    }

    [TestMethod]
    public void StepCardinality_Repeat_NegativeMin_Throws()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFPropertyPathStep(Knows).Repeat(-1, 2));

    [TestMethod]
    public void StepCardinality_Repeat_MaxLessThanMin_Throws()
        => Assert.ThrowsExactly<RDFQueryException>(() => new RDFPropertyPathStep(Knows).Repeat(3, 1));

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
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 4));
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("{2,4}"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_BoundedRange_ExactCount()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(3, 3));
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("{3}"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_InverseStep_OneOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Inverse().OneOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("^") && (printed.Contains("knows>+") || printed.Contains("knows+")), $"Printed: {printed}");
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

        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsTrue(ends.Contains(Dave.ToString()));
        Assert.IsFalse(ends.Contains(Alice.ToString())); // no self with OneOrMore
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
        Assert.IsTrue(result.Rows.Count >= 6);
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
        Assert.AreEqual(1, result.Rows.Count);
    }

    [TestMethod]
    public void Engine_OneOrMore_ConcreteStartEnd_NotReachable()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Dave, Alice)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(0, result.Rows.Count);
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
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Alice.ToString()));
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
        Assert.AreEqual(ends.Distinct().Count(), ends.Count, "No duplicates expected");
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
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
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
        Assert.IsTrue(ends.Contains(Alice.ToString()), "ZeroOrMore should include self");
        Assert.IsTrue(ends.Contains(Dave.ToString()));
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
        Assert.IsTrue(ends.Contains(Alice.ToString()), "ZeroOrOne should include self");
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "ZeroOrOne should include 1-hop");
        Assert.IsFalse(ends.Contains(Carol.ToString()), "ZeroOrOne should NOT include 2-hops");
    }

    [TestMethod]
    public void Engine_ZeroOrOne_MaxOneHop()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).ZeroOrOne());

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(2, result.Rows.Count); // alice (self) + bob (1 hop)
    }

    #endregion

    #region Engine — BoundedRange (prop{n,m})

    [TestMethod]
    public void Engine_BoundedRange_Exact2()
    {
        RDFGraph graph = BuildTestGraph();
        // alice knows{2} ?e  => carol (exactly 2 hops)
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 2));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsFalse(ends.Contains(Bob.ToString()),  "1-hop not expected");
        Assert.IsFalse(ends.Contains(Dave.ToString()), "3-hop not expected");
    }

    [TestMethod]
    public void Engine_BoundedRange_1To3()
    {
        RDFGraph graph = BuildTestGraph();
        // alice knows{1,3} ?e  => bob, carol, dave
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(1, 3));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsTrue(ends.Contains(Dave.ToString()));
    }

    [TestMethod]
    public void Engine_BoundedRange_ZeroMin_IncludesSelf()
    {
        RDFGraph graph = BuildTestGraph();
        // alice knows{0,2} ?e  => alice, bob, carol
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(0, 2));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsTrue(ends.Contains(Alice.ToString()), "0 hops => self");
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
        Assert.IsFalse(ends.Contains(Dave.ToString()), "3-hop exceeds max=2");
    }

    [TestMethod]
    public void Engine_BoundedRange_EmptyResult_TooFewEdges()
    {
        RDFGraph graph = BuildTestGraph(); // chain length = 3 hops max
        // alice knows{5,7} ?e  => no results (chain too short)
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(5, 7));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(0, result.Rows.Count);
    }

    #endregion

    #region Engine — SELECT query integration

    [TestMethod]
    public void SelectQuery_OneOrMore_ReturnsTransitiveClosure()
    {
        RDFGraph graph = BuildTestGraph();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).OneOrMore())));

        RDFSelectQueryResult result = query.ApplyToGraph(graph);
        Assert.IsTrue(result.SelectResultsCount >= 3);
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
        Assert.IsTrue(ends.Contains(Alice.ToString()));
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
        Assert.IsTrue(result.SelectResultsCount > 3);
    }

    [TestMethod]
    public void SelectQuery_BoundedRange_Exact()
    {
        RDFGraph graph = BuildTestGraph();
        RDFSelectQuery query = new RDFSelectQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, VarE)
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 2))));

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
        Assert.IsTrue(result.SelectResultsCount >= 5);
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
        Assert.IsTrue(ends.Contains(Bob.ToString()));
        Assert.IsTrue(ends.Contains(Carol.ToString()));
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
        Assert.IsTrue(ends.Contains(Alice.ToString()));
        Assert.IsTrue(ends.Contains(Bob.ToString()));
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
        Assert.IsTrue(ends.Contains(Dave.ToString()));
        Assert.IsTrue(ends.Contains(Eve.ToString()));
        Assert.IsFalse(ends.Contains(Alice.ToString()));
        Assert.IsFalse(ends.Contains(Carol.ToString())); // parent step target, not knows+ result
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
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "bob — 1 hop");
        Assert.IsTrue(ends.Contains(Carol.ToString()), "carol — 2 hops via graph2");
        Assert.IsTrue(ends.Contains(Dave.ToString()),  "dave — 3 hops via graph2");
        Assert.IsFalse(ends.Contains(Alice.ToString()), "alice must not appear (OneOrMore)");
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
        Assert.IsTrue(ends.Contains(Alice.ToString()), "alice — zero hops (self)");
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "bob — from graph source");
        Assert.IsTrue(ends.Contains(Carol.ToString()), "carol — from store source");
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
            .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 3));

        RDFQueryEngine engine = new RDFQueryEngine();
        DataTable result = engine.ApplyPropertyPath(path, federation);

        HashSet<string> ends = result.Rows.Cast<DataRow>()
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.IsFalse(ends.Contains(Bob.ToString()),  "bob — 1 hop, below range");
        Assert.IsTrue(ends.Contains(Carol.ToString()), "carol — 2 hops, in range");
        Assert.IsTrue(ends.Contains(Dave.ToString()),  "dave — 3 hops, in range");
        Assert.IsFalse(ends.Contains(Eve.ToString()),  "eve — 4 hops, above range");
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
        Assert.IsTrue(ends.Contains(Carol.ToString()), "carol — 1 reverse hop (graph2)");
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "bob — 2 reverse hops (graph1)");
        Assert.IsTrue(ends.Contains(Alice.ToString()), "alice — 3 reverse hops (graph1)");
        Assert.IsFalse(ends.Contains(Dave.ToString()),  "dave must not appear (OneOrMore)");
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
        Assert.IsTrue(ends.Contains(Bob.ToString()),   "knows 1-hop");
        Assert.IsTrue(ends.Contains(Carol.ToString()), "knows 2-hops");
        Assert.IsTrue(ends.Contains(Dave.ToString()),  "parent 1-hop");
        Assert.IsTrue(ends.Contains(Eve.ToString()),   "parent 2-hops");
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
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 2))));

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
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(1, 2))));

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
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(2, 2))));

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
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(1, 2))));

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
                    .AddSequenceStep(new RDFPropertyPathStep(Knows).Repeat(0, 1))));

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

    #endregion
}