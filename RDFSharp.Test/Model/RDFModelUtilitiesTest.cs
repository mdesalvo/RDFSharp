/*
   Copyright 2012-2021 Marco De Salvo

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
using RDFSharp.Model;
using System;
using System.Runtime.Serialization;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFModelUtilitiesTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateHash()
        {
            long hash = RDFModelUtilities.CreateHash("hello!");
            Assert.IsTrue(hash == 4443177098358787418);
        }

        [TestMethod]
        public void ShouldNotCreateHash()
            => Assert.ThrowsException<RDFModelException>(() => RDFModelUtilities.CreateHash(null));
        #endregion
    }
}