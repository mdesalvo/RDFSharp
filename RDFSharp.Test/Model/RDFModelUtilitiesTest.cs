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

        [DataTestMethod]
        [DataRow("\\U09AFaf90")]
        public void ShouldMatchRegexU8(string input)
            => Assert.IsTrue(RDFModelUtilities.regexU8.IsMatch(input));

        [DataTestMethod]
        [DataRow("\\u09AFaf90")]
        [DataRow("\\U09AFaf9")]
        [DataRow("\\U09AFaf9P")]
        public void ShouldNotMatchRegexU8(string input)
            => Assert.IsFalse(RDFModelUtilities.regexU8.IsMatch(input));

        [DataTestMethod]
        [DataRow("\\u09Af")]
        public void ShouldMatchRegexU4(string input)
           => Assert.IsTrue(RDFModelUtilities.regexU4.IsMatch(input));

        [DataTestMethod]
        [DataRow("\\U09Af")]
        [DataRow("\\u09A")]
        [DataRow("\\u09AP")]
        public void ShouldNotMatchRegexU4(string input)
            => Assert.IsFalse(RDFModelUtilities.regexU4.IsMatch(input));

        [DataTestMethod]
        [DataRow("09AFaf09")]
        public void ShouldMatchHexBinary(string input)
           => Assert.IsTrue(RDFModelUtilities.hexBinary.IsMatch(input));

        [DataTestMethod]
        [DataRow("0")]
        [DataRow("09A")]
        [DataRow("000P")]
        public void ShouldNotMatchHexBinary(string input)
            => Assert.IsFalse(RDFModelUtilities.hexBinary.IsMatch(input));

        [DataTestMethod]
        [DataRow("http://xmlns.com/foaf/0.1/")]
        public void ShouldRemapSameUriForDereference(string uriString)
        {
            Uri uri = new Uri(uriString);
            object result = this.ModelUtilities.GetMethod("RemapUriForDereference", BindingFlags.NonPublic | BindingFlags.Static)
                                               .Invoke(null, new object[] { uri });

            Assert.IsNotNull(result);
            Assert.IsTrue(((Uri)result).Equals(new Uri(RDFVocabulary.FOAF.BASE_URI)));
        }

        [DataTestMethod]
        [DataRow("http://purl.org/dc/elements/1.1/")]
        public void ShouldRemapDifferentUriForDereference(string uriString)
        {
            Uri uri = new Uri(uriString);
            object result = this.ModelUtilities.GetMethod("RemapUriForDereference", BindingFlags.NonPublic | BindingFlags.Static)
                                               .Invoke(null, new object[] { uri });

            Assert.IsNotNull(result);
            Assert.IsFalse(((Uri)result).Equals(new Uri(RDFVocabulary.DC.BASE_URI)));
        }

        [TestMethod]
        public void ShouldNotRemapNullUriForDereference()
        {
            object result = this.ModelUtilities.GetMethod("RemapUriForDereference", BindingFlags.NonPublic | BindingFlags.Static)
                                               .Invoke(null, new object[] { null });

            Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow("http://example.org/test#")]
        public void ShouldNotRemapUnknownUriForDereference(string uriString)
        {
            Uri uri = new Uri(uriString);
            object result = this.ModelUtilities.GetMethod("RemapUriForDereference", BindingFlags.NonPublic | BindingFlags.Static)
                                               .Invoke(null, new object[] { uri });

            Assert.IsNotNull(result);
            Assert.IsTrue(((Uri)result).Equals(new Uri("http://example.org/test#")));
        }
        #endregion
    }
}