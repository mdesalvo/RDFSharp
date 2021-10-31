using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;
using RDFSharp.Store;
using System;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFDataSourceTest
    {
        [TestMethod]
        public void ShouldCreateDataSourceOfTypeGraph()
            => Assert.IsTrue(new RDFGraph().IsGraph());

        [TestMethod]
        public void ShouldCreateDataSourceOfTypeStore()
            => Assert.IsTrue(new RDFMemoryStore().IsStore());

        [TestMethod]
        public void ShouldCreateDataSourceOfTypeFederation()
            => Assert.IsTrue(new RDFFederation().IsFederation());

        [TestMethod]
        public void ShouldCreateDataSourceOfTypeSPARQLEndpoint()
            => Assert.IsTrue(new RDFSPARQLEndpoint(new Uri("http://example.org/")).IsSPARQLEndpoint());
    }
}
