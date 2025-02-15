/*
   Copyright 2012-2025 Marco De Salvo

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
using RDFSharp.Store;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace RDFSharp.Test.Store;

[TestClass]
public class RDFContextTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateDefaultContext()
    {
        RDFContext ctx = new RDFContext();

        Assert.IsNotNull(ctx);
        Assert.IsTrue(ctx.ToString().Equals(RDFNamespaceRegister.DefaultNamespace.ToString()));
    }

    [TestMethod]
    public void ShouldCreateContextFromString()
    {
        RDFContext ctx = new RDFContext("ex:context");

        Assert.IsNotNull(ctx);
        Assert.IsTrue(ctx.ToString().Equals("ex:context"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextFromStringBecauseBlankNode()
        => Assert.ThrowsExactly<RDFStoreException>(() => new RDFContext("bnode:12345"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextFromStringBecauseInvalidUri()
        => Assert.ThrowsExactly<RDFStoreException>(() => new RDFContext("test"));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextFromStringBecauseNull()
        => Assert.ThrowsExactly<RDFStoreException>(() => new RDFContext(null as string));

    [TestMethod]
    public void ShouldCreateContextFromUri()
    {
        RDFContext ctx = new RDFContext(new Uri("ex:context"));

        Assert.IsNotNull(ctx);
        Assert.IsTrue(ctx.ToString().Equals("ex:context"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextFromUriBecauseBlankNode()
        => Assert.ThrowsExactly<RDFStoreException>(() => new RDFContext(new Uri("bnode:12345")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextFromUriBecauseInvalidUri()
        => Assert.ThrowsExactly<RDFStoreException>(() => new RDFContext(new Uri("test", UriKind.Relative)));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingContextFromUriBecauseNull()
        => Assert.ThrowsExactly<RDFStoreException>(() => new RDFContext(null as Uri));
    #endregion
}