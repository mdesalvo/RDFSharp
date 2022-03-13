/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
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
            Assert.IsTrue(query.QueryMembers.Count == 0);
            Assert.IsNotNull(query.Prefixes);
            Assert.IsTrue(query.Prefixes.Count == 0);
            Assert.IsTrue(query.IsEvaluable);
            Assert.IsFalse(query.IsOptional);
            Assert.IsFalse(query.JoinAsUnion);
            Assert.IsFalse(query.IsSubQuery);
            Assert.IsTrue(query.ToString().Equals(string.Empty));
            Assert.IsTrue(query.GetQueryMemberString().Equals(string.Empty));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.ToString())));
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 0);
            Assert.IsTrue(query.GetPatternGroups().Count() == 0);
            Assert.IsTrue(query.GetSubQueries().Count() == 0);
            Assert.IsTrue(query.GetValues().Count() == 0);
            Assert.IsTrue(query.GetModifiers().Count() == 0);
            Assert.IsTrue(query.GetPrefixes().Count() == 0);
        }

        [TestMethod]
        public void ShouldAddPatternGroup()
        {
            RDFQuery query = new RDFQuery()
                .AddPatternGroup<RDFQuery>(new RDFPatternGroup("PG1"))
                .AddPatternGroup<RDFQuery>(null) //Will not be accepted, since null pattern groups are not allowed
                .AddPatternGroup<RDFQuery>(new RDFPatternGroup("PG1")); //Will not be accepted, since duplicate pattern groups are not allowed

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.QueryMembers);
            Assert.IsTrue(query.QueryMembers.Count == 1);
            Assert.IsNotNull(query.Prefixes);
            Assert.IsTrue(query.Prefixes.Count == 0);
            Assert.IsTrue(query.IsEvaluable);
            Assert.IsFalse(query.IsOptional);
            Assert.IsFalse(query.JoinAsUnion);
            Assert.IsFalse(query.IsSubQuery);
            Assert.IsTrue(query.ToString().Equals(string.Empty));
            Assert.IsTrue(query.GetQueryMemberString().Equals(string.Empty));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.ToString())));
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 1);
            Assert.IsTrue(query.GetPatternGroups().Count() == 1);
            Assert.IsTrue(query.GetSubQueries().Count() == 0);
            Assert.IsTrue(query.GetValues().Count() == 0);
            Assert.IsTrue(query.GetModifiers().Count() == 0);
            Assert.IsTrue(query.GetPrefixes().Count() == 0);
        }
        #endregion
    }
}