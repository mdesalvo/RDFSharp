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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFBinaryPatternGroupMemberTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateUnionFromPatterns()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, y);
        RDFBinaryPatternGroupMember op = pA.Union(pB);

        Assert.IsNotNull(op);
        Assert.IsTrue(op.IsEvaluable);
        Assert.IsFalse(op.IsOptional);
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, op.OperatorType);
        Assert.AreSame(pA, op.LeftOperand);
        Assert.AreSame(pB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateMinusFromPatterns()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, y);
        RDFBinaryPatternGroupMember op = pA.Minus(pB);

        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Minus, op.OperatorType);
        Assert.AreSame(pA, op.LeftOperand);
        Assert.AreSame(pB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateUnionBetweenPatternAndPropertyPath()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPropertyPath ppB = new RDFPropertyPath(x, y)
            .AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDFS.SUB_CLASS_OF));
        RDFBinaryPatternGroupMember op = pA.Union(ppB);

        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, op.OperatorType);
        Assert.AreSame(pA, op.LeftOperand);
        Assert.AreSame(ppB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateUnionFromPropertyPaths()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPropertyPath ppA = new RDFPropertyPath(x, y)
            .AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDFS.SUB_CLASS_OF));
        RDFPropertyPath ppB = new RDFPropertyPath(x, y)
            .AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDF.TYPE));
        RDFBinaryPatternGroupMember op = ppA.Union(ppB);

        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, op.OperatorType);
        Assert.AreSame(ppA, op.LeftOperand);
        Assert.AreSame(ppB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldChainUnionThenMinus()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, y);
        RDFPattern pC = new RDFPattern(x, RDFVocabulary.RDFS.COMMENT, y);

        // (A UNION B) MINUS C
        RDFBinaryPatternGroupMember op = pA.Union(pB).Minus(pC);

        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Minus, op.OperatorType);
        Assert.AreSame(pC, op.RightOperand);

        RDFBinaryPatternGroupMember inner = op.LeftOperand as RDFBinaryPatternGroupMember;
        Assert.IsNotNull(inner);
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, inner.OperatorType);
        Assert.AreSame(pA, inner.LeftOperand);
        Assert.AreSame(pB, inner.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateNestedTree()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, y);
        RDFPattern pC = new RDFPattern(x, RDFVocabulary.RDFS.COMMENT, y);

        // A UNION (B MINUS C)
        RDFBinaryPatternGroupMember op = pA.Union(pB.Minus(pC));

        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Union, op.OperatorType);
        Assert.AreSame(pA, op.LeftOperand);

        RDFBinaryPatternGroupMember inner = op.RightOperand as RDFBinaryPatternGroupMember;
        Assert.IsNotNull(inner);
        Assert.AreEqual(RDFQueryEnums.RDFBinaryOperatorType.Minus, inner.OperatorType);
        Assert.AreSame(pB, inner.LeftOperand);
        Assert.AreSame(pC, inner.RightOperand);
    }

    [TestMethod]
    public void ShouldSetOptionalOnOperator()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, y);
        RDFBinaryPatternGroupMember op = pA.Union(pB).Optional();

        Assert.IsTrue(op.IsOptional);
    }

    [TestMethod]
    public void ShouldCollectVariablesFromPatternUnion()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFVariable z = new RDFVariable("?Z");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, z);
        RDFBinaryPatternGroupMember op = pA.Union(pB);

        var variables = op.GetVariables().ToList();

        Assert.IsTrue(variables.Any(v => v.Equals(x)));
        Assert.IsTrue(variables.Any(v => v.Equals(y)));
        Assert.IsTrue(variables.Any(v => v.Equals(z)));
    }

    [TestMethod]
    public void ShouldCollectVariablesFromNestedTree()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFVariable z = new RDFVariable("?Z");
        RDFVariable w = new RDFVariable("?W");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, z);
        RDFPattern pC = new RDFPattern(z, RDFVocabulary.RDFS.COMMENT, w);

        // A UNION (B MINUS C) — should collect X, Y, Z, W
        RDFBinaryPatternGroupMember op = pA.Union(pB.Minus(pC));

        var variables = op.GetVariables().ToList();

        Assert.IsTrue(variables.Any(v => v.Equals(x)));
        Assert.IsTrue(variables.Any(v => v.Equals(y)));
        Assert.IsTrue(variables.Any(v => v.Equals(z)));
        Assert.IsTrue(variables.Any(v => v.Equals(w)));
    }

    [TestMethod]
    public void ShouldCollectVariablesFromPropertyPath()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFVariable z = new RDFVariable("?Z");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPropertyPath ppB = new RDFPropertyPath(x, z)
            .AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDFS.SUB_CLASS_OF));

        RDFBinaryPatternGroupMember op = pA.Union(ppB);

        var variables = op.GetVariables().ToList();

        Assert.IsTrue(variables.Any(v => v.Equals(x)));
        Assert.IsTrue(variables.Any(v => v.Equals(y)));
        Assert.IsTrue(variables.Any(v => v.Equals(z)));
    }

    [TestMethod]
    public void ShouldAddBinaryQueryMemberToPatternGroup()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFVariable z = new RDFVariable("?Z");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, z);

        RDFPatternGroup pg = new RDFPatternGroup()
            .AddBinaryPatternGroupMember(pA.Union(pB));

        Assert.AreEqual(1, pg.GetEvaluablePatternGroupMembers().Count());
        Assert.IsTrue(pg.Variables.Any(v => v.Equals(x)));
        Assert.IsTrue(pg.Variables.Any(v => v.Equals(y)));
        Assert.IsTrue(pg.Variables.Any(v => v.Equals(z)));
    }

    [TestMethod]
    public void ShouldNotAddNullOperatorToPatternGroup()
    {
        RDFPatternGroup pg = new RDFPatternGroup()
            .AddBinaryPatternGroupMember(null);

        Assert.AreEqual(0, pg.GetEvaluablePatternGroupMembers().Count());
    }

    #region Guards
    [TestMethod]
    public void ShouldThrowOnSelfReferencePattern()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, x);

        Assert.ThrowsExactly<RDFQueryException>(() => pA.Union(pA));
    }

    [TestMethod]
    public void ShouldThrowOnSelfReferencePropertyPath()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPropertyPath pp = new RDFPropertyPath(x, y)
            .AddSequenceStep(RDFPropertyPathExpression.Link(RDFVocabulary.RDFS.SUB_CLASS_OF));

        Assert.ThrowsExactly<RDFQueryException>(() => pp.Minus(pp));
    }

    [TestMethod]
    public void ShouldThrowOnSelfReferenceOperatorNode()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFVariable y = new RDFVariable("?Y");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, y);
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDFS.LABEL, y);
        RDFBinaryPatternGroupMember node = pA.Union(pB);

        Assert.ThrowsExactly<RDFQueryException>(() => node.Union(node));
    }

    [TestMethod]
    public void ShouldThrowOnNullLeftOperand()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFPattern pB = new RDFPattern(x, RDFVocabulary.RDF.TYPE, x);

        Assert.ThrowsExactly<RDFQueryException>(
            () => new RDFBinaryPatternGroupMember(RDFQueryEnums.RDFBinaryOperatorType.Union, null, pB));
    }

    [TestMethod]
    public void ShouldThrowOnNullRightOperand()
    {
        RDFVariable x = new RDFVariable("?X");
        RDFPattern pA = new RDFPattern(x, RDFVocabulary.RDF.TYPE, x);

        Assert.ThrowsExactly<RDFQueryException>(
            () => new RDFBinaryPatternGroupMember(RDFQueryEnums.RDFBinaryOperatorType.Union, pA, null));
    }
    #endregion

    #endregion
}