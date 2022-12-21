﻿/*
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
using System.Collections.Generic;

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

        [TestMethod]
        public void ShouldCreateResourceExploitingHashContext()
        {
            Dictionary<string, long> hashContext = new Dictionary<string, long>();
            RDFResource res1 = new RDFResource("ex:res1", hashContext);
            RDFResource res1CacheHitA = new RDFResource("ex:res1", hashContext);
            RDFResource res2 = new RDFResource("ex:res2", hashContext);
            RDFResource res2CacheHitA = new RDFResource("ex:res2", hashContext);
            RDFResource res2CacheHitB = new RDFResource("ex:res2", hashContext);
            RDFResource res3 = new RDFResource("ex:res3", hashContext);

            //At this stage we only have lazy promises for hashes
            Assert.IsTrue(hashContext.Count == 0);

            RDFTriple triple1 = new RDFTriple(res1, res2, res3);
            RDFTriple triple2 = new RDFTriple(res1CacheHitA, res2, res3);
            RDFTriple triple3 = new RDFTriple(res1, res2CacheHitA, res3);
            RDFTriple triple4 = new RDFTriple(res1, res2CacheHitB, res3);
            RDFTriple triple5 = new RDFTriple(res1CacheHitA, res2CacheHitA, res3);
            RDFTriple triple6 = new RDFTriple(res1CacheHitA, res2CacheHitB, res3);
            RDFGraph graph = new RDFGraph(new List<RDFTriple>(){ triple1, triple2, triple3, triple4, triple5, triple6 });

            //Now we have materialized the lazy promises and calculated the hashes, exploiting the cache for boosting performances
            Assert.IsTrue(hashContext.Count == 3);
            Assert.IsTrue(hashContext.ContainsKey("ex:res1"));
            Assert.IsTrue(hashContext.ContainsKey("ex:res2"));
            Assert.IsTrue(hashContext.ContainsKey("ex:res3"));
        }
        #endregion
    }
}