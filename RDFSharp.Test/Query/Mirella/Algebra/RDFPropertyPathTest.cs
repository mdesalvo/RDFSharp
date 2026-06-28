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
using System;
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
        Assert.AreEqual(0, propertyPath.SequenceUnits.Count);
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
            .AddSequenceStep((RDFPropertyPathExpression)null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullPropertyPathStepInSequenceStep()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddSequenceStep(RDFPropertyPathExpression.Link(null)));

    [TestMethod]
    public void ShouldAddSingleSequenceStep()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT));

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(1, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleSequenceInverseStep()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT).Inverse());

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(1, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleSequenceSteps()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT));
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG));

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(2, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleSequenceStepsWithInverseFirst()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT).Inverse());
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG));

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(2, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt/rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleSequenceStepsWithInverseLast()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT));
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG).Inverse());

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(2, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/^<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/^rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleSequenceStepsWithInverseBoth()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT).Inverse());
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG).Inverse());

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(2, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}>/^<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt/^rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeSteps()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG) ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(1, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt|rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeStepsWithInverseFirst()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT).Inverse(),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG) ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(1, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt|rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeStepsWithInverseLast()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG).Inverse() ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(1, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>|^<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt|^rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeStepsWithInverseBoth()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT).Inverse(),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG).Inverse() ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(1, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START ^<{RDFVocabulary.RDF.ALT}>|^<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START ^rdf:Alt|^rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddSingleAlternativeStepsBecomingSequenceStepBecauseSingle()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(1, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMultipleAlternativeStepsBecomingSequenceStepBecauseSingle()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT)]);
        propertyPath.AddAlternativeSteps([RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(2, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/rdf:Bag rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddTwoDistinctAlternativeUnitsInSequence()
    {
        //Two alternative units stay distinct sequence units (a|b)/(b|c) — the flat model used to wrongly collapse
        //them into a single 4-way alternative; the tree model keeps them separate.
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG) ]);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.SEQ) ]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(2, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.ALT}>|<{RDFVocabulary.RDF.BAG}>)/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Alt|rdf:Bag)/(rdf:Bag|rdf:Seq) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMixedStepsSequentialAlternatives()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT));
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.SEQ)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(2, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMixedStepsAlternativesSequential()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.SEQ)]);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT));

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(2, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/<{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/rdf:Alt rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMixedStepsAlternativesSequentialAlternatives()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.SEQ)]);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT));
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.SEQ)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(3, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/<{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldAddMixedStepsAlternativesInverseSequentialAlternatives()
    {
        RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.SEQ)]);
        propertyPath.AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.ALT).Inverse());
        propertyPath.AddAlternativeSteps([ RDFPropertyPathExpression.Link(RDFVocabulary.RDF.BAG),
            RDFPropertyPathExpression.Link(RDFVocabulary.RDF.SEQ)]);

        Assert.IsNotNull(propertyPath);
        Assert.IsNotNull(propertyPath.Start);
        Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
        Assert.IsNotNull(propertyPath.End);
        Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
        Assert.AreEqual(3, propertyPath.SequenceUnits.Count);
        Assert.IsTrue(propertyPath.IsEvaluable);
        Assert.IsTrue(propertyPath.ToString().Equals($"?START (<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>)/^<{RDFVocabulary.RDF.ALT}>/(<{RDFVocabulary.RDF.BAG}>|<{RDFVocabulary.RDF.SEQ}>) <{RDFVocabulary.RDF.TYPE}>", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.ToString([RDFNamespaceRegister.GetByPrefix("rdf")]).Equals("?START (rdf:Bag|rdf:Seq)/^rdf:Alt/(rdf:Bag|rdf:Seq) rdf:type", System.StringComparison.Ordinal));
        Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullAlternativeStep()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddAlternativeSteps((List<RDFPropertyPathExpression>)null));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseEmptyAlternativeStep()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddAlternativeSteps(new List<RDFPropertyPathExpression>()));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullPropertyPathStepInAlternativeSteps()
        => Assert.ThrowsExactly<RDFQueryException>(() => _ = new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
            .AddAlternativeSteps(new List<RDFPropertyPathExpression> { null }));

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
        RDFPropertyPathExpression step = RDFPropertyPathExpression.Link(Knows);
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne, step.Cardinality);
    }

    [TestMethod]
    public void StepCardinality_ZeroOrOne_Fluent()
    {
        RDFPropertyPathExpression step = RDFPropertyPathExpression.Link(Knows).ZeroOrOne();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne, step.Cardinality);
    }

    [TestMethod]
    public void StepCardinality_OneOrMore_Fluent()
    {
        RDFPropertyPathExpression step = RDFPropertyPathExpression.Link(Knows).OneOrMore();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, step.Cardinality);
    }

    [TestMethod]
    public void StepCardinality_ZeroOrMore_Fluent()
    {
        RDFPropertyPathExpression step = RDFPropertyPathExpression.Link(Knows).ZeroOrMore();
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore, step.Cardinality);
    }

    [TestMethod]
    public void StepCardinality_InverseCombines_WithCardinality()
    {
        RDFPropertyPathExpression step = RDFPropertyPathExpression.Link(Knows).Inverse().OneOrMore();
        //Cardinality is applied before inverse: the cardinality wraps the inverse link in an explicit group (^P)+,
        //so the outer node carries the cardinality and the inner (child) node carries the inverse
        Assert.AreEqual(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore, step.Cardinality);
        Assert.IsFalse(step.IsInverse);
        Assert.IsTrue(step.Children[0].IsInverse && step.Children[0].Property.Equals(Knows));
    }


    [TestMethod]
    public void PropertyPath_IsEvaluable_TrueForTransitiveSingleStepWithConcreteEnds()
    {
        // A transitive step with concrete start/end and Depth=1 should still be evaluable
        RDFPropertyPath path = new RDFPropertyPath(Alice, Bob)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());
        Assert.IsTrue(path.IsEvaluable);
    }

    #endregion

    #region Engine — OneOrMore (prop+)

    [TestMethod]
    public void Engine_OneOrMore_ChainForwardVariableEnd()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        // alice reachable from alice: bob, carol, dave
        // bob reachable from bob: carol, dave
        // carol reachable from carol: dave
        Assert.IsGreaterThanOrEqualTo(6, result.RowsCount);
        List<(string, string)> rows = result.Rows
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        Assert.IsTrue(rows.Any(p => p.Item1 == Alice.ToString() && p.Item2 == Dave.ToString()));
    }

    [TestMethod]
    public void Engine_OneOrMore_ConcreteStartEnd_Reachable()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Alice, Dave)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(1, result.RowsCount);
    }

    [TestMethod]
    public void Engine_OneOrMore_ConcreteStartEnd_NotReachable()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(Dave, Alice)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(0, result.RowsCount);
    }

    [TestMethod]
    public void Engine_OneOrMore_InverseStep()
    {
        RDFGraph graph = BuildTestGraph();
        // dave ^knows+ ?e  => alice, bob, carol reachable via reverse knows
        RDFPropertyPath path = new RDFPropertyPath(Dave, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).Inverse().OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        List<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends, "ZeroOrMore should include self");
        Assert.Contains(Dave.ToString(), ends);
    }

    [TestMethod]
    public void Engine_ZeroOrMore_BothVariables_IncludesIdentity()
    {
        RDFGraph graph = BuildTestGraph();
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        List<(string, string)> rows = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrOne());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrOne());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);
        Assert.AreEqual(2, result.RowsCount); // alice (self) + bob (1 hop)
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrOne());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        // Each node can reach all 3 nodes (including itself via ZeroOrMore),
        // so we expect exactly 9 pairs (3 sources × 3 targets), finite result.
        List<(string s, string e)> rows = result.Rows
            .Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToList();
        Assert.AreEqual(9, rows.Count, "3×3 pairs with no duplicates despite triangle cycle");
        Assert.IsTrue(rows.Any(p => p.s == Alice.ToString() && p.e == Alice.ToString()), "alice reaches itself");
        Assert.IsTrue(rows.Any(p => p.s == Bob.ToString()   && p.e == Alice.ToString()), "bob reaches alice via cycle");
        Assert.IsTrue(rows.Any(p => p.s == Carol.ToString() && p.e == Bob.ToString()),   "carol reaches bob via cycle");
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Parent))
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())
            .AddSequenceStep(RDFPropertyPathExpression.Link(Parent));

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        List<(string, string)> rows = result.Rows
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
            .AddAlternativeSteps([
                RDFPropertyPathExpression.Link(Knows).OneOrMore(),
                RDFPropertyPathExpression.Link(Parent).OneOrMore()
            ]);

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, graph);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, store);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, store);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, federation);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, federation);

        HashSet<string> ends = result.Rows
            .Select(r => r["?E"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), ends, "alice — zero hops (self)");
        Assert.Contains(Bob.ToString(), ends,   "bob — from graph source");
        Assert.Contains(Carol.ToString(), ends, "carol — from store source");
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).Inverse().OneOrMore());

        RDFQueryEngine engine = new RDFQueryEngine();
        RDFTable result = engine.ApplyPropertyPath(path, federation);

        HashSet<string> ends = result.Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrOne());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>?") || printed.Contains("knows?"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_OneOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>+") || printed.Contains("knows+"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_SingleStep_ZeroOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore());
        string printed = path.ToString();
        Assert.IsTrue(printed.Contains("knows>*") || printed.Contains("knows*"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_InverseStep_OneOrMore()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).Inverse().OneOrMore());
        string printed = path.ToString();
        //Cardinality-before-inverse renders as an explicit group: (^<...knows>)+
        Assert.IsTrue(printed.Contains('^') && printed.Contains(")+"), $"Printed: {printed}");
    }

    [TestMethod]
    public void Printer_Optional_SetsFlags()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows))
            .Optional();
        Assert.IsTrue(path.IsOptional);
    }

    [TestMethod]
    public void Printer_Optional_WrapsOutputWithOptional()
    {
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())
            .Optional();
        string printed = path.ToString();
        Assert.IsTrue(printed.StartsWith("OPTIONAL {", StringComparison.Ordinal), $"Expected OPTIONAL prefix, got: {printed}");
        Assert.IsTrue(printed.EndsWith("}"), $"Expected closing brace, got: {printed}");
        Assert.IsTrue(printed.Contains("knows"), $"Path predicate missing from: {printed}");
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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrOne())));

        RDFSelectQueryResult result = query.ApplyToGraph(graph);
        // Self-pairs included + direct knows pairs
        Assert.IsGreaterThan(3, result.SelectResultsCount);
    }

    [TestMethod]
    public void AskQuery_OneOrMore_True()
    {
        RDFGraph graph = BuildTestGraph();
        RDFAskQuery query = new RDFAskQuery()
            .AddPatternGroup(new RDFPatternGroup()
                .AddPropertyPath(new RDFPropertyPath(Alice, Dave)
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice))); // 0 hops
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));   // 1 hop
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));// no 2-hops
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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).Inverse().OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol))); // not matched (2 hops)
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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).Inverse().OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrOne())));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Alice)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Tag, Bob)));
        Assert.IsFalse(graph.ContainsTriple(new RDFTriple(Alice, Tag, Carol)));
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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore()))
                .AddFilter(new RDFFilter(
                    new RDFComparisonExpression(
                        RDFQueryEnums.RDFComparisonFlavors.EqualTo,
                        new RDFVariableExpression(VarS),
                        new RDFConstantExpression(Alice)))));

        op.ApplyToGraph(graph);

        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Bob)));
        Assert.IsTrue(graph.ContainsTriple(new RDFTriple(Alice, Reached, Dave)));
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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore())));

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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore())));

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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Type))
            .AddSequenceStep(RDFPropertyPathExpression.Link(SubClassOf).ZeroOrMore());

        HashSet<string> classes = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(SubClassOf).Inverse().ZeroOrMore());

        HashSet<string> subs = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Type))
            .AddSequenceStep(RDFPropertyPathExpression.Link(EquivClass))
            .AddSequenceStep(RDFPropertyPathExpression.Link(SubClassOf).ZeroOrMore());

        HashSet<string> classes = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows
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
            .AddSequenceStep(RDFPropertyPathExpression.Link(Type))
            .AddSequenceStep(RDFPropertyPathExpression.Link(SubClassOf).ZeroOrOne());

        HashSet<string> classes = new RDFQueryEngine()
            .ApplyPropertyPath(path, graph)
            .Rows
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
                    .AddSequenceStep(RDFPropertyPathExpression.Link(Type))
                    .AddSequenceStep(RDFPropertyPathExpression.Link(SubClassOf).ZeroOrMore())))
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


    #region Engine — Transitive acceleration (adjacency + SCC closure)

    // Helper: brute-force reference transitive closure (one-or-more) of `prop` over a graph,
    // computed with an independent per-node BFS — the exact semantics the fast path must preserve.
    private static Dictionary<string, HashSet<string>> ReferenceOneOrMore(RDFGraph graph, RDFResource prop)
    {
        // Build plain adjacency from the graph
        Dictionary<string, List<string>> adj = new Dictionary<string, List<string>>();
        HashSet<string> nodes = [];
        foreach (RDFTriple t in graph.SelectTriples(p: prop))
        {
            string s = t.Subject.ToString(), o = t.Object.ToString();
            nodes.Add(s); nodes.Add(o);
            if (!adj.TryGetValue(s, out List<string> succ)) adj[s] = succ = [];
            succ.Add(o);
        }

        Dictionary<string, HashSet<string>> closure = new Dictionary<string, HashSet<string>>();
        foreach (string start in nodes)
        {
            HashSet<string> reached = [];
            Queue<string> q = new Queue<string>();
            HashSet<string> enq = [start];
            q.Enqueue(start);
            while (q.Count > 0)
            {
                string cur = q.Dequeue();
                if (!adj.TryGetValue(cur, out List<string> succ)) continue;
                foreach (string n in succ)
                {
                    reached.Add(n); // one-or-more: every neighbor reached in >=1 hop counts (incl. cycle back to start)
                    if (enq.Add(n)) q.Enqueue(n);
                }
            }
            if (reached.Count > 0)
                closure[start] = reached;
        }
        return closure;
    }

    private static HashSet<(string, string)> EnginePairs(RDFGraph graph, RDFPropertyPath path)
    {
        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(path, graph);
        return result.Rows.Select(r => (r["?S"].ToString(), r["?E"].ToString())).ToHashSet();
    }

    [TestMethod]
    public void Transitive_Diamond_DAG_ClosureSharedAcrossPaths()
    {
        // a->b, a->c, b->d, c->d : d reachable from a via two distinct paths (no duplicate row)
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Alice, Knows, Carol));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Dave));
        graph.AddTriple(new RDFTriple(Carol, Knows, Dave));

        HashSet<(string, string)> pairs = EnginePairs(graph, new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore()));

        // alice -> b,c,d ; bob -> d ; carol -> d  => 5 distinct pairs, dave reaches nothing
        Assert.HasCount(5, pairs.ToList());
        Assert.Contains((Alice.ToString(), Dave.ToString()), pairs);
        Assert.Contains((Bob.ToString(),   Dave.ToString()), pairs);
        Assert.Contains((Carol.ToString(), Dave.ToString()), pairs);
        Assert.DoesNotContain((Dave.ToString(), Dave.ToString()), pairs);
    }

    [TestMethod]
    public void Transitive_MultiNodeCycle_ReachesSelfAndDownstream()
    {
        // a->b->c->a (3-cycle) and c->d (downstream sink)
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Bob));
        graph.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        graph.AddTriple(new RDFTriple(Carol, Knows, Alice));
        graph.AddTriple(new RDFTriple(Carol, Knows, Dave));

        HashSet<(string, string)> pairs = EnginePairs(graph, new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore()));

        // Every node of the cycle reaches all cycle members (incl. itself) plus dave
        foreach (RDFResource n in new[] { Alice, Bob, Carol })
            foreach (RDFResource m in new[] { Alice, Bob, Carol, Dave })
                Assert.Contains((n.ToString(), m.ToString()), pairs, $"{n} should reach {m}");
        // dave is a sink: reaches nothing
        Assert.IsFalse(pairs.Any(p => p.Item1 == Dave.ToString()));
        Assert.HasCount(12, pairs.ToList()); // 3 cycle nodes × 4 targets
    }

    [TestMethod]
    public void Transitive_SelfLoop_Singleton_ReachesItself()
    {
        // a->a self loop
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Knows, Alice));

        HashSet<(string, string)> plus = EnginePairs(graph, new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore()));
        Assert.Contains((Alice.ToString(), Alice.ToString()), plus);
        Assert.HasCount(1, plus.ToList());

        // ZeroOrMore must not duplicate the self pair
        HashSet<(string, string)> star = EnginePairs(graph, new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore()));
        Assert.Contains((Alice.ToString(), Alice.ToString()), star);
        Assert.HasCount(1, star.ToList());
    }

    [TestMethod]
    public void Transitive_OneOrMore_AcyclicChain_HasNoIdentityPairs()
    {
        // Seed pruning must not introduce identity (x,x) rows for an acyclic chain under "+"
        RDFGraph graph = BuildTestGraph(); // alice->bob->carol->dave
        HashSet<(string, string)> pairs = EnginePairs(graph, new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore()));

        Assert.IsFalse(pairs.Any(p => p.Item1 == p.Item2), "Acyclic chain must yield no identity pairs under +");
        Assert.HasCount(6, pairs.ToList()); // (a,b)(a,c)(a,d)(b,c)(b,d)(c,d)
    }

    [TestMethod]
    public void Transitive_ZeroOrMore_BothVars_IncludesIdentityForSinkOnlyNode()
    {
        // dave appears only as an object (no outgoing knows edge): "*" must still emit (dave,dave).
        // This guards against the seed pruning wrongly excluding sink nodes for the reflexive case.
        RDFGraph graph = BuildTestGraph();
        HashSet<(string, string)> pairs = EnginePairs(graph, new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).ZeroOrMore()));

        Assert.Contains((Dave.ToString(),  Dave.ToString()),  pairs, "sink-only node keeps its * identity");
        Assert.Contains((Alice.ToString(), Alice.ToString()), pairs);
        Assert.Contains((Alice.ToString(), Dave.ToString()),  pairs);
    }

    [TestMethod]
    public void Transitive_ConcreteEnd_VariableStart_PushDown()
    {
        // ?s knows+ dave  =>  alice, bob, carol (push-down via inverse closure from dave)
        RDFGraph graph = BuildTestGraph();
        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(
            new RDFPropertyPath(VarS, Dave)
                .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore()), graph);

        HashSet<string> starts = result.Rows.Select(r => r["?S"].ToString()).ToHashSet();
        Assert.Contains(Alice.ToString(), starts);
        Assert.Contains(Bob.ToString(),   starts);
        Assert.Contains(Carol.ToString(), starts);
        Assert.DoesNotContain(Dave.ToString(), starts); // dave does not reach itself (acyclic)
        Assert.HasCount(3, starts.ToList());
    }

    [TestMethod]
    public void Transitive_ConcreteEnd_InverseStep_PushDown()
    {
        // dave ^knows+ ?s evaluated with concrete end on the inverse step direction
        RDFGraph graph = BuildTestGraph();
        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(
            new RDFPropertyPath(VarS, Alice)
                .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).Inverse().OneOrMore()), graph);

        // ?s (^knows)+ alice  => nodes reachable from alice following knows forward: bob, carol, dave
        HashSet<string> starts = result.Rows.Select(r => r["?S"].ToString()).ToHashSet();
        Assert.Contains(Bob.ToString(),   starts);
        Assert.Contains(Carol.ToString(), starts);
        Assert.Contains(Dave.ToString(),  starts);
        Assert.HasCount(3, starts.ToList());
    }

    [TestMethod]
    public void Transitive_DifferentialAgainstBruteForce_CyclicBranchingGraph()
    {
        // A graph with branches, a shared sink (diamond), a multi-node cycle and a self loop:
        // the fast SCC-based closure must match an independent per-node BFS exactly.
        RDFGraph graph = new RDFGraph();
        void E(RDFResource s, RDFResource o) => graph.AddTriple(new RDFTriple(s, Knows, o));
        E(Alice, Bob);   E(Alice, Carol);   // branch
        E(Bob,   Dave);  E(Carol, Dave);    // diamond join
        E(Dave,  Eve);
        E(Eve,   Frank); E(Frank, Eve);     // 2-cycle eve<->frank
        E(Frank, Frank);                    // self loop

        Dictionary<string, HashSet<string>> reference = ReferenceOneOrMore(graph, Knows);
        HashSet<(string, string)> expected = reference
            .SelectMany(kv => kv.Value.Select(v => (kv.Key, v)))
            .ToHashSet();

        HashSet<(string, string)> actual = EnginePairs(graph, new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore()));

        Assert.IsTrue(expected.SetEquals(actual),
            $"SCC closure diverges from brute force.\nExpected\\Actual: {string.Join(",", expected.Except(actual))}\nActual\\Expected: {string.Join(",", actual.Except(expected))}");
    }

    [TestMethod]
    public void Transitive_LongChain_DoesNotStackOverflow()
    {
        // Deep linear chain n0->n1->...->n2000 : iterative Tarjan must not overflow the stack,
        // and the closure size must be n*(n-1)/2 (every node reaches all its successors).
        const int n = 2000;
        RDFGraph graph = new RDFGraph();
        RDFResource[] nodes = new RDFResource[n + 1];
        for (int i = 0; i <= n; i++)
            nodes[i] = new RDFResource($"ex:n{i}");
        for (int i = 0; i < n; i++)
            graph.AddTriple(new RDFTriple(nodes[i], Knows, nodes[i + 1]));

        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Link(Knows).OneOrMore()), graph);

        Assert.AreEqual((long)n * (n + 1) / 2, result.RowsCount);
    }

    #endregion

    #endregion

    #region Tests (IP5.3 — recursive forms: negated set, group cardinality, group inverse, composite alternative)

    #region Shared data
    private static readonly RDFResource Likes = new RDFResource("ex:likes");
    private static readonly RDFResource Step1 = new RDFResource("ex:step1");
    private static readonly RDFResource Step2 = new RDFResource("ex:step2");

    //alice -knows-> bob -knows-> carol -knows-> alice (cycle); alice -parent-> bob -parent-> carol; alice -likes-> dave
    private static RDFGraph BuildSocialGraph()
    {
        RDFGraph g = new RDFGraph();
        g.AddTriple(new RDFTriple(Alice, Knows, Bob));
        g.AddTriple(new RDFTriple(Bob,   Knows, Carol));
        g.AddTriple(new RDFTriple(Carol, Knows, Alice));
        g.AddTriple(new RDFTriple(Alice, Parent, Bob));
        g.AddTriple(new RDFTriple(Bob,   Parent, Carol));
        g.AddTriple(new RDFTriple(Alice, Likes, Dave));
        return g;
    }

    //A composite 3-cycle: the relation (step1/step2) is { a->b, b->c, c->a } through intermediate nodes
    private static RDFGraph BuildCompositeCycleGraph()
    {
        RDFGraph g = new RDFGraph();
        g.AddTriple(new RDFTriple(Alice, Step1, new RDFResource("ex:mAB"))); g.AddTriple(new RDFTriple(new RDFResource("ex:mAB"), Step2, Bob));
        g.AddTriple(new RDFTriple(Bob,   Step1, new RDFResource("ex:mBC"))); g.AddTriple(new RDFTriple(new RDFResource("ex:mBC"), Step2, Carol));
        g.AddTriple(new RDFTriple(Carol, Step1, new RDFResource("ex:mCA"))); g.AddTriple(new RDFTriple(new RDFResource("ex:mCA"), Step2, Alice));
        return g;
    }

    //The set of "?S->?E" pairs (both variable) of a result table
    private static HashSet<string> StartEndPairs(RDFTable table)
        => table.Rows.Select(r => $"{r["?S"]}->{r["?E"]}").ToHashSet();

    //The set of "?E" values (concrete start, variable end) of a result table
    private static HashSet<string> EndValues(RDFTable table)
        => table.Rows.Select(r => r["?E"].ToString()).ToHashSet();
    #endregion

    [TestMethod]
    public void Engine_NegatedPropertySet_ForwardExcludesOnlyTheMember()
    {
        //?s !knows ?o -> every forward edge whose predicate is NOT knows (parent and likes edges), no reverse
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.NegatedPropertySet([(Knows, false)]));

        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(path, BuildSocialGraph());

        CollectionAssert.AreEquivalent(
            new[] { "ex:alice->ex:bob", "ex:bob->ex:carol", "ex:alice->ex:dave" },
            StartEndPairs(result).ToList());
    }

    [TestMethod]
    public void Engine_NegatedPropertySet_InverseMemberTraversesOnlyBackwards()
    {
        //?s !^knows ?o -> reverse edges over any predicate but knows; no forward direction
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.NegatedPropertySet([(Knows, true)]));

        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(path, BuildSocialGraph());

        //Reverse of parent {a->b,b->c} and likes {a->d}: {b->a, c->b, d->a}
        CollectionAssert.AreEquivalent(
            new[] { "ex:bob->ex:alice", "ex:carol->ex:bob", "ex:dave->ex:alice" },
            StartEndPairs(result).ToList());
    }

    [TestMethod]
    public void Engine_GroupCardinality_TransitiveClosureOverCompositeRelation()
    {
        //(step1/step2)+ from alice over the composite 3-cycle reaches every node (cyclic): alice, bob, carol
        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Sequence([
                RDFPropertyPathExpression.Link(Step1), RDFPropertyPathExpression.Link(Step2)]).OneOrMore());

        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(path, BuildCompositeCycleGraph());

        CollectionAssert.AreEquivalent(
            new[] { "ex:alice", "ex:bob", "ex:carol" },
            EndValues(result).ToList());
    }

    [TestMethod]
    public void Engine_InverseGroup_ReversesTheWholeAlternative()
    {
        //^(knows|parent): reverse of the union knows ∪ parent
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Alternative([
                RDFPropertyPathExpression.Link(Knows), RDFPropertyPathExpression.Link(Parent)]).Inverse());

        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(path, BuildSocialGraph());

        //Forward knows∪parent = {a->b,b->c,c->a}; reversed and deduped = {b->a, c->b, a->c}
        CollectionAssert.AreEquivalent(
            new[] { "ex:bob->ex:alice", "ex:carol->ex:bob", "ex:alice->ex:carol" },
            StartEndPairs(result).ToList());
    }

    [TestMethod]
    public void Engine_CompositeAlternative_SequenceAsAlternativeBranch()
    {
        //(knows/parent)|likes : union of a composite sequence relation and a plain link
        RDFPropertyPath path = new RDFPropertyPath(VarS, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Alternative([
                RDFPropertyPathExpression.Sequence([RDFPropertyPathExpression.Link(Knows), RDFPropertyPathExpression.Link(Parent)]),
                RDFPropertyPathExpression.Link(Likes)]));

        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(path, BuildSocialGraph());

        //knows/parent = {a->c (a knows b, b parent c), c->b (c knows a, a parent b)}; likes = {a->d}
        CollectionAssert.AreEquivalent(
            new[] { "ex:alice->ex:carol", "ex:carol->ex:bob", "ex:alice->ex:dave" },
            StartEndPairs(result).ToList());
    }

    [TestMethod]
    public void Engine_GroupCardinality_SelfLoopReachesItself()
    {
        //Anti-loop guard: a node with a composite self-loop must terminate and reach itself under '+'
        RDFGraph graph = new RDFGraph();
        graph.AddTriple(new RDFTriple(Alice, Step1, new RDFResource("ex:mid")));
        graph.AddTriple(new RDFTriple(new RDFResource("ex:mid"), Step2, Alice)); // (step1/step2): alice->alice

        RDFPropertyPath path = new RDFPropertyPath(Alice, VarE)
            .AddSequenceStep(RDFPropertyPathExpression.Sequence([
                RDFPropertyPathExpression.Link(Step1), RDFPropertyPathExpression.Link(Step2)]).OneOrMore());

        RDFTable result = new RDFQueryEngine().ApplyPropertyPath(path, graph);

        CollectionAssert.AreEquivalent(new[] { "ex:alice" }, EndValues(result).ToList());
    }

    #region Iso gate (API ⇄ SPARQL) for the recursive forms
    private static RDFSelectQuery PathQuery(RDFPropertyPathExpression expression)
        => new RDFSelectQuery().AddPatternGroup(new RDFPatternGroup()
            .AddPropertyPath(new RDFPropertyPath(VarS, VarE).AddSequenceStep(expression)));

    [TestMethod]
    public void ShouldRoundTripAndEvaluateNegatedPropertySetIso()
        => RDFTestUtilities.AssertIso(
            PathQuery(RDFPropertyPathExpression.NegatedPropertySet([(Knows, false), (Parent, true)])),
            BuildSocialGraph());

    [TestMethod]
    public void ShouldRoundTripAndEvaluateGroupCardinalityIso()
        => RDFTestUtilities.AssertIso(
            PathQuery(RDFPropertyPathExpression.Sequence([
                RDFPropertyPathExpression.Link(Step1), RDFPropertyPathExpression.Link(Step2)]).OneOrMore()),
            BuildCompositeCycleGraph());

    [TestMethod]
    public void ShouldRoundTripAndEvaluateInverseGroupIso()
        => RDFTestUtilities.AssertIso(
            PathQuery(RDFPropertyPathExpression.Alternative([
                RDFPropertyPathExpression.Link(Knows), RDFPropertyPathExpression.Link(Parent)]).Inverse()),
            BuildSocialGraph());

    [TestMethod]
    public void ShouldRoundTripAndEvaluateCompositeAlternativeIso()
        => RDFTestUtilities.AssertIso(
            PathQuery(RDFPropertyPathExpression.Alternative([
                RDFPropertyPathExpression.Sequence([RDFPropertyPathExpression.Link(Knows), RDFPropertyPathExpression.Link(Parent)]),
                RDFPropertyPathExpression.Link(Likes)])),
            BuildSocialGraph());
    #endregion

    #endregion
}