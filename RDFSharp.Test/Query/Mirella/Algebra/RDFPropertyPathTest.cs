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
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFPropertyPathTest
{
    #region Tests
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START  <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START  rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/rdf:Bag rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt/rdf:Bag rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/^<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/^rdf:Bag rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}>/^<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt/^rdf:Bag rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}>) <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Alt|rdf:Bag) rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (^<{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}>) <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (^rdf:Alt|rdf:Bag) rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.ALT}>|^<{RDFVocabulary.RDF.BAG}>) <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Alt|^rdf:Bag) rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (^<{RDFVocabulary.RDF.ALT}>|^<{RDFVocabulary.RDF.BAG}>) <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (^rdf:Alt|^rdf:Bag) rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/rdf:Bag rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Alt|rdf:Bag|rdf:Bag|rdf:Seq) rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/<{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/rdf:Alt rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/<{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type"));
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
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/^<{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>"));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/^rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type"));
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
}