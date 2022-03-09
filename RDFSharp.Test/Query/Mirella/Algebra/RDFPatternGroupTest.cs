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
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFPatternGroupTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreatePatternGroup()
        {
            RDFPatternGroup pGroup = new RDFPatternGroup(" pGroup ");

            Assert.IsNotNull(pGroup);
            Assert.IsTrue(pGroup.PatternGroupName.Equals("PGROUP"));
            Assert.IsTrue(pGroup.IsEvaluable);
            Assert.IsFalse(pGroup.IsOptional);
            Assert.IsFalse(pGroup.JoinAsUnion);
            Assert.IsNotNull(pGroup.GroupMembers);
            Assert.IsTrue(pGroup.GroupMembers.Count == 0);
            Assert.IsNotNull(pGroup.Variables);
            Assert.IsTrue(pGroup.Variables.Count == 0);
            Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "  }", Environment.NewLine)));
            Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.PatternGroupName)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternGroupBecauseNullOrWhitespaceName()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPatternGroup(null));
        #endregion
    }
}