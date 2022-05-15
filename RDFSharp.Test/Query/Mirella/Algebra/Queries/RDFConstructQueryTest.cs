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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFConstructQueryTest
    {
        private WireMockServer server;

        [TestInitialize]
        public void Initialize() { server = WireMockServer.Start(); }

        [TestCleanup]
        public void Cleanup()  { server.Stop(); server.Dispose(); }
        
        #region Tests
        [TestMethod]
        public void ShouldCreateConstructQuery()
        {
            RDFConstructQuery query = new RDFConstructQuery();

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.QueryMembers);
            Assert.IsTrue(query.QueryMembers.Count == 0);
            Assert.IsNotNull(query.Templates);
            Assert.IsTrue(query.Templates.Count == 0);
            Assert.IsNotNull(query.Variables);
            Assert.IsTrue(query.Variables.Count == 0);
            Assert.IsNotNull(query.Prefixes);
            Assert.IsTrue(query.Prefixes.Count == 0);
            Assert.IsTrue(query.IsEvaluable);
            Assert.IsFalse(query.IsOptional);
            Assert.IsFalse(query.JoinAsUnion);
            Assert.IsFalse(query.IsSubQuery);
            Assert.IsTrue(query.ToString().Equals("CONSTRUCT" + Environment.NewLine + "{" + Environment.NewLine + "}" + Environment.NewLine + "WHERE {" + Environment.NewLine + "}"));
            Assert.IsTrue(query.QueryMemberID.Equals(RDFModelUtilities.CreateHash(query.QueryMemberStringID)));
            Assert.IsTrue(query.GetEvaluableQueryMembers().Count() == 0);
            Assert.IsTrue(query.GetPatternGroups().Count() == 0);
            Assert.IsTrue(query.GetSubQueries().Count() == 0);
            Assert.IsTrue(query.GetValues().Count() == 0);
            Assert.IsTrue(query.GetModifiers().Count() == 0);
            Assert.IsTrue(query.GetPrefixes().Count() == 0);
        }
        #endregion
    }
}