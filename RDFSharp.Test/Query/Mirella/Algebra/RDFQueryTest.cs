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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFQueryTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateQuery()
    {
        RDFQuery query = new RDFQuery();

        Assert.IsNotNull(query);
        Assert.IsNotNull(query.QueryMembers);
        Assert.IsEmpty(query.QueryMembers);
        Assert.IsNotNull(query.Prefixes);
        Assert.IsEmpty(query.Prefixes);
        Assert.IsTrue(query.IsEvaluable);
        Assert.IsTrue(query.ToString().Equals(string.Empty, System.StringComparison.Ordinal));
        Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
        Assert.AreEqual(0, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(0, query.GetPatternGroups().Count());
        Assert.AreEqual(0, query.GetSubQueries().Count());
        Assert.IsEmpty(query.GetValues());
        Assert.AreEqual(0, query.GetModifiers().Count());
        Assert.IsEmpty(query.GetPrefixes());
    }

    [TestMethod]
    public void ShouldAddPatternGroup()
    {
        RDFPatternGroup pg1 = new RDFPatternGroup();
        RDFQuery query = new RDFQuery()
            .AddPatternGroup<RDFQuery>(pg1)
            .AddPatternGroup<RDFQuery>(null) //Will not be accepted, since null pattern groups are not allowed
            .AddPatternGroup<RDFQuery>(pg1); //Will not be accepted, since duplicate pattern groups are not allowed

        Assert.IsNotNull(query);
        Assert.IsNotNull(query.QueryMembers);
        Assert.HasCount(1, query.QueryMembers);
        Assert.IsNotNull(query.Prefixes);
        Assert.IsEmpty(query.Prefixes);
        Assert.IsTrue(query.IsEvaluable);
        Assert.IsTrue(query.ToString().Equals(string.Empty, System.StringComparison.Ordinal));
        Assert.AreEqual(1, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(1, query.GetPatternGroups().Count());
        Assert.AreEqual(0, query.GetSubQueries().Count());
        Assert.IsEmpty(query.GetValues());
        Assert.AreEqual(0, query.GetModifiers().Count());
        Assert.IsEmpty(query.GetPrefixes());
    }

    [TestMethod]
    public void ShouldAddModifier()
    {
        RDFQuery query = new RDFQuery()
            .AddModifier<RDFQuery>(new RDFDistinctModifier())
            .AddModifier<RDFQuery>(null as RDFDistinctModifier) //Will not be accepted, since null modifiers are not allowed
            .AddModifier<RDFQuery>(new RDFDistinctModifier()) //Will not be accepted, since duplicate distinct modifiers are not allowed
            .AddModifier<RDFQuery>(new RDFLimitModifier(10))
            .AddModifier<RDFQuery>(null as RDFLimitModifier) //Will not be accepted, since null modifiers are not allowed
            .AddModifier<RDFQuery>(new RDFLimitModifier(8)) //Will not be accepted, since duplicate limit modifiers are not allowed
            .AddModifier<RDFQuery>(new RDFOffsetModifier(10))
            .AddModifier<RDFQuery>(null as RDFOffsetModifier) //Will not be accepted, since null modifiers are not allowed
            .AddModifier<RDFQuery>(new RDFOffsetModifier(8)); //Will not be accepted, since duplicate offset modifiers are not allowed

        Assert.IsNotNull(query);
        Assert.IsNotNull(query.QueryMembers);
        Assert.HasCount(3, query.QueryMembers);
        Assert.IsNotNull(query.Prefixes);
        Assert.IsEmpty(query.Prefixes);
        Assert.IsTrue(query.IsEvaluable);
        Assert.IsTrue(query.ToString().Equals(string.Empty, System.StringComparison.Ordinal));
        Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
        Assert.AreEqual(0, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(0, query.GetPatternGroups().Count());
        Assert.AreEqual(0, query.GetSubQueries().Count());
        Assert.IsEmpty(query.GetValues());
        Assert.AreEqual(3, query.GetModifiers().Count());
        Assert.IsEmpty(query.GetPrefixes());
    }

    [TestMethod]
    public void ShouldAddPrefix()
    {
        RDFQuery query = new RDFQuery()
            .AddPrefix<RDFQuery>(RDFNamespaceRegister.GetByPrefix("rdf"))
            .AddPrefix<RDFQuery>(null) //Will not be accepted, since null prefixes are not allowed
            .AddPrefix<RDFQuery>(RDFNamespaceRegister.GetByPrefix("rdf")); //Will not be accepted, since duplicate prefixes are not allowed

        Assert.IsNotNull(query);
        Assert.IsNotNull(query.QueryMembers);
        Assert.IsEmpty(query.QueryMembers);
        Assert.IsNotNull(query.Prefixes);
        Assert.HasCount(1, query.Prefixes);
        Assert.IsTrue(query.IsEvaluable);
        Assert.IsTrue(query.ToString().Equals(string.Empty, System.StringComparison.Ordinal));
        Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
        Assert.AreEqual(0, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(0, query.GetPatternGroups().Count());
        Assert.AreEqual(0, query.GetSubQueries().Count());
        Assert.IsEmpty(query.GetValues());
        Assert.AreEqual(0, query.GetModifiers().Count());
        Assert.HasCount(1, query.GetPrefixes());
    }

    [TestMethod]
    public void ShouldAddSubQuery()
    {
        RDFSelectQuery subQuery = new RDFSelectQuery();
        RDFQuery query = new RDFQuery()
            .AddSubQuery<RDFQuery>(subQuery)
            .AddSubQuery<RDFQuery>(null) //Will not be accepted, since null sub queries are not allowed
            .AddSubQuery<RDFQuery>(subQuery); //Will not be accepted, since duplicate sub queries are not allowed

        Assert.IsNotNull(query);
        Assert.IsNotNull(query.QueryMembers);
        Assert.HasCount(1, query.QueryMembers);
        Assert.IsNotNull(query.Prefixes);
        Assert.IsEmpty(query.Prefixes);
        Assert.IsTrue(query.IsEvaluable);
        Assert.IsTrue(subQuery.IsSubQuery);
        Assert.IsTrue(query.ToString().Equals(string.Empty, System.StringComparison.Ordinal));
        Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
        Assert.AreEqual(1, query.GetEvaluableQueryMembers().Count());
        Assert.AreEqual(0, query.GetPatternGroups().Count());
        Assert.AreEqual(1, query.GetSubQueries().Count());
        Assert.IsEmpty(query.GetValues());
        Assert.AreEqual(0, query.GetModifiers().Count());
        Assert.IsEmpty(query.GetPrefixes());
    }
    #endregion
}