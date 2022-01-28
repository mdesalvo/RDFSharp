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
using System;
using System.Linq;
using System.Collections.Generic;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFNamespaceRegisterTest
    {
        #region Tests
        [TestMethod]
        public void ShouldAccessInstance()
        {
            Assert.IsNotNull(RDFNamespaceRegister.Instance);
            Assert.IsTrue(RDFNamespaceRegister.Instance is IEnumerable<RDFNamespace>);
        }

        [TestMethod]
        public void ShouldAccessAndModifyDefaultNamespace()
        {
            Assert.IsNotNull(RDFNamespaceRegister.DefaultNamespace);
            Assert.IsTrue(RDFNamespaceRegister.DefaultNamespace.NamespacePrefix.Equals(RDFVocabulary.RDFSHARP.PREFIX));
            Assert.IsTrue(RDFNamespaceRegister.DefaultNamespace.NamespaceUri.Equals(new Uri(RDFVocabulary.RDFSHARP.BASE_URI)));

            RDFNamespace exNS = new RDFNamespace("ex", "http://example.org/");
            RDFNamespaceRegister.SetDefaultNamespace(exNS);
            Assert.IsNotNull(RDFNamespaceRegister.DefaultNamespace);
            Assert.IsTrue(RDFNamespaceRegister.DefaultNamespace.NamespacePrefix.Equals(exNS.NamespacePrefix));
            Assert.IsTrue(RDFNamespaceRegister.DefaultNamespace.NamespaceUri.Equals(exNS.NamespaceUri));
            Assert.IsNotNull(RDFNamespaceRegister.Instance.Register.Find(x => x.Equals(exNS)));

            RDFNamespaceRegister.ResetDefaultNamespace();
            Assert.IsNotNull(RDFNamespaceRegister.DefaultNamespace);
            Assert.IsTrue(RDFNamespaceRegister.DefaultNamespace.NamespacePrefix.Equals(RDFVocabulary.RDFSHARP.PREFIX));
            Assert.IsTrue(RDFNamespaceRegister.DefaultNamespace.NamespaceUri.Equals(new Uri(RDFVocabulary.RDFSHARP.BASE_URI)));
        }

        [TestMethod]
        public void ShouldEnlistNamespaces()
        {
            Assert.IsTrue(RDFNamespaceRegister.NamespacesCount > 0);
            Assert.IsNotNull(RDFNamespaceRegister.NamespacesEnumerator);

            var nsEnumerator = RDFNamespaceRegister.NamespacesEnumerator;
            while (nsEnumerator.MoveNext())
                Assert.IsNotNull(nsEnumerator.Current);

            foreach (RDFNamespace ns in RDFNamespaceRegister.Instance.Register)
                Assert.IsNotNull(ns);

            foreach (RDFNamespace ns in RDFNamespaceRegister.Instance)
                Assert.IsNotNull(ns);
        }

        [TestMethod]
        public void ShouldAddAndRemoveNamespaces()
        {
            RDFNamespace ex4NS = new RDFNamespace("ex4", "http://example.org4/");
            RDFNamespaceRegister.AddNamespace(ex4NS);
            Assert.IsNotNull(RDFNamespaceRegister.Instance.Register.Find(x => x.Equals(ex4NS)));

            RDFNamespaceRegister.RemoveByPrefix(ex4NS.NamespacePrefix);
            Assert.IsNull(RDFNamespaceRegister.Instance.Register.Find(x => x.Equals(ex4NS)));

            RDFNamespaceRegister.AddNamespace(ex4NS);
            RDFNamespaceRegister.RemoveByUri(ex4NS.NamespaceUri.ToString());
            Assert.IsNull(RDFNamespaceRegister.Instance.Register.Find(x => x.Equals(ex4NS)));
        }

        [TestMethod]
        public void ShouldNotAddAlreadyExistingNamespaces()
        {
            RDFNamespace ex5NS = new RDFNamespace("ex5", "http://example.org5/");
            RDFNamespaceRegister.AddNamespace(ex5NS);
            Assert.IsTrue(RDFNamespaceRegister.Instance.Register.Count(x => 
                x.NamespacePrefix.Equals(ex5NS.NamespacePrefix) || x.NamespaceUri.Equals(ex5NS.NamespaceUri)) == 1);

            RDFNamespace ex5PNS = new RDFNamespace("ex5P", "http://example.org5/");
            RDFNamespaceRegister.AddNamespace(ex5PNS);
            Assert.IsTrue(RDFNamespaceRegister.Instance.Register.Count(x =>
                x.NamespacePrefix.Equals(ex5PNS.NamespacePrefix) || x.NamespaceUri.Equals(ex5PNS.NamespaceUri)) == 1);

            RDFNamespace ex5UNS = new RDFNamespace("ex5", "http://example.org5U/");
            RDFNamespaceRegister.AddNamespace(ex5UNS);
            Assert.IsTrue(RDFNamespaceRegister.Instance.Register.Count(x =>
                x.NamespacePrefix.Equals(ex5UNS.NamespacePrefix) || x.NamespaceUri.Equals(ex5UNS.NamespaceUri)) == 1);

            int nsCountBefore = RDFNamespaceRegister.NamespacesCount;
            RDFNamespaceRegister.AddNamespace(null);
            int nsCountAfter = RDFNamespaceRegister.NamespacesCount;
            Assert.IsTrue(nsCountBefore == nsCountAfter);
        }

        [TestMethod]
        public void ShouldNotRemoveUnexistingNamespaces()
        {
            int nsCountBefore = RDFNamespaceRegister.NamespacesCount;
            RDFNamespace ex6NS = new RDFNamespace("ex6", "http://example.org6/");
            RDFNamespaceRegister.RemoveByPrefix(ex6NS.NamespacePrefix);
            int nsCountAfter = RDFNamespaceRegister.NamespacesCount;
            Assert.IsTrue(nsCountBefore == nsCountAfter);

            nsCountBefore = RDFNamespaceRegister.NamespacesCount;
            RDFNamespaceRegister.RemoveByPrefix(null);
            nsCountAfter = RDFNamespaceRegister.NamespacesCount;
            Assert.IsTrue(nsCountBefore == nsCountAfter);

            nsCountBefore = RDFNamespaceRegister.NamespacesCount;
            RDFNamespaceRegister.RemoveByUri(ex6NS.NamespaceUri.ToString());
            nsCountAfter = RDFNamespaceRegister.NamespacesCount;
            Assert.IsTrue(nsCountBefore == nsCountAfter);

            nsCountBefore = RDFNamespaceRegister.NamespacesCount;
            RDFNamespaceRegister.RemoveByUri(null);
            nsCountAfter = RDFNamespaceRegister.NamespacesCount;
            Assert.IsTrue(nsCountBefore == nsCountAfter);
        }

        [TestMethod]
        public void ShouldGetNamespaceByPrefix()
        {
            RDFNamespace ns = RDFNamespaceRegister.GetByPrefix(RDFVocabulary.RDF.PREFIX, false);

            Assert.IsNotNull(ns);
            Assert.IsTrue(ns.NamespacePrefix.Equals(RDFVocabulary.RDF.PREFIX));
            Assert.IsTrue(ns.NamespaceUri.Equals(new Uri(RDFVocabulary.RDF.BASE_URI)));
        }

        [TestMethod]
        [TestProperty("Warning", "This is a physical test! It tries to contact prefix.cc service, which may be unavailable! We are not going to mock tests like this.")]
        public void ShouldGetNamespaceByPrefixWithLookupService()
        {
            RDFNamespace ns = RDFNamespaceRegister.GetByPrefix("dbo", true);

            Assert.IsNotNull(ns);
            Assert.IsTrue(ns.NamespacePrefix.Equals("dbo"));
            Assert.IsTrue(ns.NamespaceUri.Equals(new Uri("http://dbpedia.org/ontology/")));
            Assert.IsTrue(RDFNamespaceRegister.Instance.Register.Any(x => x.Equals(ns)));
            RDFNamespaceRegister.RemoveByPrefix("dbo");
            Assert.IsFalse(RDFNamespaceRegister.Instance.Register.Any(x => x.Equals(ns)));
        }

        [TestMethod]
        public void ShouldNotGetNamespaceByPrefix()
        {
            Assert.IsNull(RDFNamespaceRegister.GetByPrefix("exxx", false));
            Assert.IsNull(RDFNamespaceRegister.GetByPrefix(null, false));
        }

        [TestMethod]
        [TestProperty("Warning", "This is a physical test! It tries to contact prefix.cc service, which may be unavailable! We are not going to mock tests like this.")]
        public void ShouldNotGetNamespaceByPrefixWithLookupService()
        {
            Assert.IsNull(RDFNamespaceRegister.GetByPrefix("exxx", true));
            Assert.IsNull(RDFNamespaceRegister.GetByPrefix(null, true));
        }

        [TestMethod]
        public void ShouldGetNamespaceByUri()
        {
            RDFNamespace ns = RDFNamespaceRegister.GetByUri(RDFVocabulary.RDF.BASE_URI, false);

            Assert.IsNotNull(ns);
            Assert.IsTrue(ns.NamespacePrefix.Equals(RDFVocabulary.RDF.PREFIX));
            Assert.IsTrue(ns.NamespaceUri.Equals(new Uri(RDFVocabulary.RDF.BASE_URI)));
        }

        [TestMethod]
        public void ShouldGetNamespaceByUriWithLookupService()
        {
            RDFNamespace ns = RDFNamespaceRegister.GetByUri("http://dbpedia.org/ontology/", true);

            Assert.IsNotNull(ns);
            Assert.IsTrue(ns.NamespacePrefix.Equals("dbo"));
            Assert.IsTrue(ns.NamespaceUri.Equals(new Uri("http://dbpedia.org/ontology/")));
            Assert.IsTrue(RDFNamespaceRegister.Instance.Register.Any(x => x.Equals(ns)));
            RDFNamespaceRegister.RemoveByPrefix("dbo");
            Assert.IsFalse(RDFNamespaceRegister.Instance.Register.Any(x => x.Equals(ns)));
        }

        [TestMethod]
        public void ShouldNotGetNamespaceByUri()
        {
            Assert.IsNull(RDFNamespaceRegister.GetByUri("http://exx.org/", false));
            Assert.IsNull(RDFNamespaceRegister.GetByUri(null, false));
        }

        [TestMethod]
        public void ShouldNotGetNamespaceByUriWithLookupService()
        {
            Assert.IsNull(RDFNamespaceRegister.GetByUri("http://exx.org/", true));
            Assert.IsNull(RDFNamespaceRegister.GetByUri(null, true));
        }
        #endregion
    }
}