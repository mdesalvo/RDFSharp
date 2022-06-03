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
using RDFSharp.Store;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFQueryEngineTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateQueryEngine()
        {
            RDFQueryEngine queryEngine = new RDFQueryEngine();

            Assert.IsNotNull(queryEngine);
            Assert.IsNotNull(queryEngine.QueryMemberTemporaryResultTables);
            Assert.IsNotNull(queryEngine.QueryMemberFinalResultTables);
            Assert.IsTrue(RDFQueryEngine.SystemString.Equals(typeof(string)));
        }
        #endregion
    }
}