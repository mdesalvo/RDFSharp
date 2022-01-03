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

using RDFSharp.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFResourceTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateUnnamedBlankResource()
        {
            RDFResource res = new RDFResource();

            Assert.IsNotNull(res);
            Assert.IsTrue(res.IsBlank);
            Assert.IsTrue(res.ToString().StartsWith("bnode:"));
        }

        [DataTestMethod]
        [DataRow("bnode:jwgdw")]
        [DataRow("bnoDe:jwgdw")]
        [DataRow("bnode:")]
        [DataRow("bnODe:")]
        [DataRow("_:jwgdw")]
        [DataRow("_:")]
        public void ShouldCreateNamedBlankResource(string input)
        {
            RDFResource res = new RDFResource(input);

            Assert.IsNotNull(res);
            Assert.IsTrue(res.IsBlank);
            Assert.IsTrue(res.ToString().StartsWith("bnode:"));
        }

        [DataTestMethod]
        [DataRow("http://hello/world#hi")]
        [DataRow("http://hello/world#")]
        [DataRow("http://hello/world/")]
        [DataRow("http://hello/world")]
        [DataRow("urn:hello:world")]
        public void ShouldCreateResource(string input)
        {
            RDFResource res = new RDFResource(input);

            Assert.IsNotNull(res);
            Assert.IsFalse(res.IsBlank);
            Assert.IsTrue(res.ToString().Equals(input));
        }

        [DataTestMethod]
        [DataRow(" ")]
        [DataRow("")]
        [DataRow("\t")]
        [DataRow(null)]
        public void ShouldNotCreateResourceDueToNullOrEmptyUri(string input)
            => Assert.ThrowsException<RDFModelException>(() => new RDFResource(input));

        [DataTestMethod]
        [DataRow("hello")]
        [DataRow("http:/hello#")]
        [DataRow("http:// ")]
        [DataRow("http:// hello/world")]
        [DataRow("http://hello\0")]
        [DataRow("http://hel\0lo/world")]
        public void ShouldNotCreateResourceDueToInvalidUri(string input)
            => Assert.ThrowsException<RDFModelException>(() => new RDFResource(input));
        #endregion
    }
}