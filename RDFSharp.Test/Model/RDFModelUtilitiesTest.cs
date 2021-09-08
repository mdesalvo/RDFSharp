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
using System.Linq;
using System.Reflection;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFModelUtilitiesTest
    {
        #region Properties
        private Type ModelUtilities { get; set; }
        #endregion

        #region Ctors
        [TestInitialize]
        public void InitializeTestClass()
            => this.ModelUtilities = AppDomain.CurrentDomain.GetAssemblies()
                                                            .ToList()
                                                            .Find(asm => asm.GetName().Name.Equals("RDFSharp"))
                                                            .GetType("RDFSharp.Model.RDFModelUtilities");
        #endregion

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

        [DataTestMethod]
        [DataRow("http://example.org/test#")]
        public void ShouldGetUriFromString(string uriString)
        {
            object result = this.ModelUtilities.GetMethod("GetUriFromString", BindingFlags.NonPublic | BindingFlags.Static)
                                               .Invoke(null, new object[] { uriString });
            Assert.IsInstanceOfType(result, typeof(Uri));
            Assert.IsTrue(((Uri)result).Equals(new Uri(uriString)));
        }

        [DataTestMethod]
        [DataRow("bnode://example.org/test#")]
        [DataRow("bNoDE://example.org/test#")]
        [DataRow("_://example.org/test#")]
        public void ShouldGetBlankUriFromString(string uriString)
        {
            object result = this.ModelUtilities.GetMethod("GetUriFromString", BindingFlags.NonPublic | BindingFlags.Static)
                                               .Invoke(null, new object[] { uriString });
            Assert.IsInstanceOfType(result, typeof(Uri));
            Assert.IsTrue(((Uri)result).Equals(new Uri("bnode://example.org/test#")));
        }

        [DataTestMethod]
        [DataRow(null)]
        public void ShouldNotGetUriFromString(string uriString)
        {
            object result = this.ModelUtilities.GetMethod("GetUriFromString", BindingFlags.NonPublic | BindingFlags.Static)
                                               .Invoke(null, new object[] { uriString });
            Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow("/file/system")]
        public void ShouldNotGetRelativeUriFromString(string uriString)
        {
            object result = this.ModelUtilities.GetMethod("GetUriFromString", BindingFlags.NonPublic | BindingFlags.Static)
                                               .Invoke(null, new object[] { uriString });
            Assert.IsNull(result);
        }
        #endregion
    }
}