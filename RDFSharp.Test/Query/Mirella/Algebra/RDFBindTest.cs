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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query;

[TestClass]
public class RDFBindTest
{
    #region Tests
    [TestMethod]
    public void ShouldCreateBind()
    {
        RDFBind bind = new RDFBind(new RDFVariableExpression(new RDFVariable("?EXP")), new RDFVariable("?BIND"));

        Assert.IsNotNull(bind);
        Assert.IsNotNull(bind.Expression);
        Assert.IsNotNull(bind.Variable);
        Assert.IsTrue(bind.IsEvaluable);
        Assert.IsTrue(bind.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(bind.PatternGroupMemberStringID)));
        Assert.IsTrue(string.Equals(bind.ToString(), "BIND(?EXP AS ?BIND)"));
        Assert.IsTrue(string.Equals(bind.ToString([]), "BIND(?EXP AS ?BIND)"));
    }

    [TestMethod]
    public void ShouldCreateBindWithPrefixableUris()
    {
        RDFBind bind = new RDFBind(new RDFConstantExpression(new RDFTypedLiteral("hello", RDFModelEnums.RDFDatatypes.XSD_STRING)), new RDFVariable("?BIND"));

        Assert.IsNotNull(bind);
        Assert.IsNotNull(bind.Expression);
        Assert.IsNotNull(bind.Variable);
        Assert.IsTrue(bind.IsEvaluable);
        Assert.IsTrue(bind.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(bind.PatternGroupMemberStringID)));
        Assert.IsTrue(string.Equals(bind.ToString(), $"BIND(\"hello\"^^<{RDFVocabulary.XSD.STRING}> AS ?BIND)"));
        Assert.IsTrue(string.Equals(bind.ToString([]), $"BIND(\"hello\"^^<{RDFVocabulary.XSD.STRING}> AS ?BIND)"));
        Assert.IsTrue(string.Equals(bind.ToString([RDFNamespaceRegister.GetByPrefix("xsd")]), "BIND(\"hello\"^^xsd:string AS ?BIND)"));
    }

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBindBecauseNullExpression()
        => Assert.ThrowsException<RDFQueryException>(() => new RDFBind(null, new RDFVariable("?BIND")));

    [TestMethod]
    public void ShouldThrowExceptionOnCreatingBindBecauseNullVariable()
        => Assert.ThrowsException<RDFQueryException>(() => new RDFBind(new RDFVariableExpression(new RDFVariable("?EXP")), null));
    #endregion
}