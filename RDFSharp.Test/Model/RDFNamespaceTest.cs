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

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFNamespaceTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow("ex", "http://example.org/")]
        [DataRow(" ex ", " http://example.org/")]
        public void ShouldCreateNamespace(string prefix, string uri)
        {
            RDFNamespace ns = new RDFNamespace(prefix, uri);
            Assert.IsNotNull(ns);
            Assert.IsTrue(ns.NamespacePrefix.Equals(prefix.Trim()));
            Assert.IsTrue(ns.NamespaceUri.Equals(new Uri(uri.Trim())));
            Assert.IsTrue(ns.DereferenceUri.Equals(ns.NamespaceUri));
            Assert.IsFalse(ns.IsTemporary);
            
            string nsString = ns.ToString();
            Assert.IsTrue(nsString.Equals(uri.Trim()));

            long nsID = RDFModelUtilities.CreateHash(nsString);
            Assert.IsTrue(ns.NamespaceID.Equals(nsID));

            RDFNamespace ns2 = new RDFNamespace(prefix, uri);
            Assert.IsTrue(ns.Equals(ns2));
        }

        [DataTestMethod]
        [DataRow(" ", "http://example.org/")]
        [DataRow("", "http://example.org/")]
        [DataRow(null, "http://example.org/")]
        public void ShouldNotCreateNamespaceBecauseOfBlankPrefix(string prefix, string uri)
            => Assert.ThrowsException<RDFModelException>(() => new RDFNamespace(prefix, uri));

        [DataTestMethod]
        [DataRow("ex%", "http://example.org/")]
        public void ShouldNotCreateNamespaceBecauseOfInvalidPrefix(string prefix, string uri)
            => Assert.ThrowsException<RDFModelException>(() => new RDFNamespace(prefix, uri));

        [DataTestMethod]
        [DataRow("bnode", "http://example.org/")]
        [DataRow("xmlns", "http://example.org/")]
        public void ShouldNotCreateNamespaceBecauseOfReservedPrefix(string prefix, string uri)
            => Assert.ThrowsException<RDFModelException>(() => new RDFNamespace(prefix, uri));

        [DataTestMethod]
        [DataRow("ex", " ")]
        [DataRow("ex", "")]
        [DataRow("ex", null)]
        public void ShouldNotCreateNamespaceBecauseOfBlankUri(string prefix, string uri)
            => Assert.ThrowsException<RDFModelException>(() => new RDFNamespace(prefix, uri));

        [DataTestMethod]
        [DataRow("ex", "http:/example.org/")]
        [DataRow("ex", "hello")]
        public void ShouldNotCreateNamespaceBecauseOfInvalidUri(string prefix, string uri)
            => Assert.ThrowsException<RDFModelException>(() => new RDFNamespace(prefix, uri));

        [DataTestMethod]
        [DataRow("ex", "bnode:/example.org/")]
        [DataRow("ex", "xmlns:/example.org/")]
        public void ShouldNotCreateNamespaceBecauseOfReservedUri(string prefix, string uri)
            => Assert.ThrowsException<RDFModelException>(() => new RDFNamespace(prefix, uri));

        [DataTestMethod]
        [DataRow("ex", "http://example.org/", "http://example.org/deref#")]
        public void ShouldSetDereferenceUri(string prefix, string uri, string derefUri)
        {
            RDFNamespace ns = new RDFNamespace(prefix, uri);
            ns.SetDereferenceUri(new Uri(derefUri));

            Assert.IsTrue(ns.DereferenceUri.Equals(new Uri(derefUri)));
            Assert.IsFalse(ns.DereferenceUri.Equals(ns.NamespaceUri));
        }

        [DataTestMethod]
        [DataRow("ex", "http://example.org/")]
        public void ShouldNotSetDereferenceUriBecauseofNullUri(string prefix, string uri)
        {
            RDFNamespace ns = new RDFNamespace(prefix, uri);
            ns.SetDereferenceUri(null);

            Assert.IsTrue(ns.DereferenceUri.Equals(ns.NamespaceUri));
        }

        [DataTestMethod]
        [DataRow("ex", "http://example.org/")]
        public void ShouldNotSetDereferenceUriBecauseofRelativeUri(string prefix, string uri)
        {
            RDFNamespace ns = new RDFNamespace(prefix, uri);
            ns.SetDereferenceUri(new Uri("/file/system", UriKind.Relative));

            Assert.IsTrue(ns.DereferenceUri.Equals(ns.NamespaceUri));
        }

        [DataTestMethod]
        [DataRow("ex", "http://example.org/")]
        public void ShouldNotSetDereferenceUriBecauseofReservedUri(string prefix, string uri)
        {
            RDFNamespace ns = new RDFNamespace(prefix, uri);
            ns.SetDereferenceUri(new Uri("bnode:/file/system/"));

            RDFNamespace ns2 = new RDFNamespace(prefix, uri);
            ns2.SetDereferenceUri(new Uri("xmlns:/file/system/"));

            Assert.IsTrue(ns.DereferenceUri.Equals(ns.NamespaceUri));
            Assert.IsTrue(ns2.DereferenceUri.Equals(ns2.NamespaceUri));
        }

        [DataTestMethod]
        [DataRow("ex", "http://example.org/")]
        public void ShouldSetTemporaryNamespace(string prefix, string uri)
        {
            RDFNamespace ns = new RDFNamespace(prefix, uri);
            ns.SetTemporary(true);
            Assert.IsTrue(ns.IsTemporary);
        }
        #endregion
    }
}