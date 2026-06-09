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
public class RDFOperatorQueryMemberTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateUnionFromPatternGroups()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember op = pgA.Union(pgB);

        Assert.IsNotNull(op);
        Assert.IsTrue(op.IsEvaluable);
        Assert.IsFalse(op.IsOptional);
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
        Assert.AreSame(pgA, op.LeftOperand);
        Assert.AreSame(pgB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateMinusFromPatternGroups()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember op = pgA.Minus(pgB);

        Assert.IsNotNull(op);
        Assert.IsTrue(op.IsEvaluable);
        Assert.IsFalse(op.IsOptional);
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, op.OperatorType);
        Assert.AreSame(pgA, op.LeftOperand);
        Assert.AreSame(pgB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateUnionFromPatternGroupAndSubquery()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFSelectQuery sqB = new RDFSelectQuery();
        RDFOperatorQueryMember op = pgA.Union(sqB);

        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
        Assert.AreSame(pgA, op.LeftOperand);
        Assert.AreSame(sqB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateMinusFromSubqueryAndPatternGroup()
    {
        RDFSelectQuery sqA = new RDFSelectQuery();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember op = sqA.Minus(pgB);

        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, op.OperatorType);
        Assert.AreSame(sqA, op.LeftOperand);
        Assert.AreSame(pgB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateUnionBetweenSubqueries()
    {
        RDFSelectQuery sqA = new RDFSelectQuery();
        RDFSelectQuery sqB = new RDFSelectQuery();
        RDFOperatorQueryMember op = sqA.Union(sqB);

        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
        Assert.AreSame(sqA, op.LeftOperand);
        Assert.AreSame(sqB, op.RightOperand);
    }

    [TestMethod]
    public void ShouldChainUnionThenMinus()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFPatternGroup pgC = new RDFPatternGroup();

        // (A UNION B) MINUS C
        RDFOperatorQueryMember op = pgA.Union(pgB).Minus(pgC);

        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, op.OperatorType);
        Assert.AreSame(pgC, op.RightOperand);

        RDFOperatorQueryMember inner = op.LeftOperand as RDFOperatorQueryMember;
        Assert.IsNotNull(inner);
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, inner.OperatorType);
        Assert.AreSame(pgA, inner.LeftOperand);
        Assert.AreSame(pgB, inner.RightOperand);
    }

    [TestMethod]
    public void ShouldChainMinusThenUnion()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFPatternGroup pgC = new RDFPatternGroup();

        // (A MINUS B) UNION C
        RDFOperatorQueryMember op = pgA.Minus(pgB).Union(pgC);

        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
        Assert.AreSame(pgC, op.RightOperand);

        RDFOperatorQueryMember inner = op.LeftOperand as RDFOperatorQueryMember;
        Assert.IsNotNull(inner);
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, inner.OperatorType);
        Assert.AreSame(pgA, inner.LeftOperand);
        Assert.AreSame(pgB, inner.RightOperand);
    }

    [TestMethod]
    public void ShouldCreateNestedTree()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFPatternGroup pgC = new RDFPatternGroup();

        // A UNION (B MINUS C)
        RDFOperatorQueryMember op = pgA.Union(pgB.Minus(pgC));

        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
        Assert.AreSame(pgA, op.LeftOperand);

        RDFOperatorQueryMember inner = op.RightOperand as RDFOperatorQueryMember;
        Assert.IsNotNull(inner);
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Minus, inner.OperatorType);
        Assert.AreSame(pgB, inner.LeftOperand);
        Assert.AreSame(pgC, inner.RightOperand);
    }

    [TestMethod]
    public void ShouldSetOptionalOnOperator()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember op = pgA.Union(pgB).Optional();

        Assert.IsTrue(op.IsOptional);
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
    }

    [TestMethod]
    public void ShouldChainUnionWithOperatorNode()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFPatternGroup pgC = new RDFPatternGroup();
        RDFPatternGroup pgD = new RDFPatternGroup();

        // (A UNION B) UNION (C MINUS D)
        RDFOperatorQueryMember left = pgA.Union(pgB);
        RDFOperatorQueryMember right = pgC.Minus(pgD);
        RDFOperatorQueryMember op = left.Union(right);

        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
        Assert.AreSame(left, op.LeftOperand);
        Assert.AreSame(right, op.RightOperand);
    }

    [TestMethod]
    public void ShouldAddOperatorToSelectQuery()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember op = pgA.Union(pgB);

        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(op);

        Assert.AreEqual(1, query.GetEvaluableQueryMembers().Count());
        Assert.AreSame(op, query.GetEvaluableQueryMembers().First());
    }

    [TestMethod]
    public void ShouldAddOperatorToAskQuery()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember op = pgA.Union(pgB);

        RDFAskQuery query = new RDFAskQuery()
            .AddOperator(op);

        Assert.AreEqual(1, query.GetEvaluableQueryMembers().Count());
    }

    [TestMethod]
    public void ShouldAddOperatorToConstructQuery()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember op = pgA.Union(pgB);

        RDFConstructQuery query = new RDFConstructQuery()
            .AddOperator(op);

        Assert.AreEqual(1, query.GetEvaluableQueryMembers().Count());
    }

    [TestMethod]
    public void ShouldAddOperatorToDescribeQuery()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember op = pgA.Union(pgB);

        RDFDescribeQuery query = new RDFDescribeQuery()
            .AddOperator(op);

        Assert.AreEqual(1, query.GetEvaluableQueryMembers().Count());
    }

    [TestMethod]
    public void ShouldNotAddNullOperatorToQuery()
    {
        RDFSelectQuery query = new RDFSelectQuery()
            .AddOperator(null);

        Assert.AreEqual(0, query.GetEvaluableQueryMembers().Count());
    }

    #region Guards
    [TestMethod]
    public void ShouldThrowOnSelfReferenceUnion()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        Assert.ThrowsExactly<RDFQueryException>(() => pgA.Union(pgA));
    }

    [TestMethod]
    public void ShouldThrowOnSelfReferenceMinus()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        Assert.ThrowsExactly<RDFQueryException>(() => pgA.Minus(pgA));
    }

    [TestMethod]
    public void ShouldThrowOnSelfReferenceSubquery()
    {
        RDFSelectQuery sq = new RDFSelectQuery();
        Assert.ThrowsExactly<RDFQueryException>(() => sq.Union(sq));
    }

    [TestMethod]
    public void ShouldThrowOnSelfReferenceOperatorNode()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFOperatorQueryMember node = pgA.Union(pgB);

        Assert.ThrowsExactly<RDFQueryException>(() => node.Union(node));
    }

    [TestMethod]
    public void ShouldThrowOnNullLeftOperand()
    {
        Assert.ThrowsExactly<RDFQueryException>(
            () => new RDFOperatorQueryMember(RDFQueryEnums.RDFQueryOperatorType.Union, null, new RDFPatternGroup()));
    }

    [TestMethod]
    public void ShouldThrowOnNullRightOperand()
    {
        Assert.ThrowsExactly<RDFQueryException>(
            () => new RDFOperatorQueryMember(RDFQueryEnums.RDFQueryOperatorType.Union, new RDFPatternGroup(), null));
    }

    [TestMethod]
    public void ShouldAllowSameStructureDifferentInstances()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFPatternGroup pgC = new RDFPatternGroup();

        // B.Minus(C) creates a new node; A.Union(thatNode) is valid because A != node
        RDFOperatorQueryMember op = pgA.Union(pgB.Minus(pgC));

        Assert.IsNotNull(op);
        Assert.AreEqual(RDFQueryEnums.RDFQueryOperatorType.Union, op.OperatorType);
    }

    [TestMethod]
    public void ShouldAllowSharedLeafInDifferentBranches()
    {
        RDFPatternGroup pgA = new RDFPatternGroup();
        RDFPatternGroup pgB = new RDFPatternGroup();
        RDFPatternGroup pgC = new RDFPatternGroup();

        // pgA appears in both branches but via different tree nodes
        RDFOperatorQueryMember left = pgA.Union(pgB);
        RDFOperatorQueryMember right = pgC.Minus(pgA);
        RDFOperatorQueryMember op = left.Union(right);

        Assert.IsNotNull(op);
    }
    #endregion

    #endregion
}
