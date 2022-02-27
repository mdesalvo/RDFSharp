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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFQueryUtilitiesTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow("http://res.org/")]
        public void ShouldParsePatternMemberAsUri(string uri)
        {
            RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember(uri);

            Assert.IsNotNull(pMember);
            Assert.IsTrue(pMember is RDFResource pMemberResource && pMemberResource.Equals(new RDFResource(uri)));
        }

        [DataTestMethod]
        [DataRow("hello")]
        [DataRow("hello^^")]
        [DataRow("hello^^test")]
        [DataRow("hello@")]
        [DataRow("file/system")]
        public void ShouldParsePatternMemberAsPlainLiteral(string litVal)
        {
            RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember(litVal);

            Assert.IsNotNull(pMember);
            Assert.IsTrue(pMember is RDFPlainLiteral pMemberLiteral && pMemberLiteral.Equals(new RDFPlainLiteral(litVal)));
        }

        [DataTestMethod]
        [DataRow("hello","en")]
        [DataRow("hello","en-US")]
        [DataRow("hello","en-US1-US2")]
        [DataRow("hello^^","en-US")]
        public void ShouldParsePatternMemberAsPlainLiteralWithLanguage(string litVal, string litLang)
        {
            RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember($"{litVal}@{litLang}");

            Assert.IsNotNull(pMember);
            Assert.IsTrue(pMember is RDFPlainLiteral pMemberLiteral && pMemberLiteral.Equals(new RDFPlainLiteral(litVal, litLang)));
        }

        [TestMethod]
        public void ShouldParsePatternMemberAsTypedLiteral()
        {
            RDFPatternMember pMember = RDFQueryUtilities.ParseRDFPatternMember("25^^http://www.w3.org/2001/XMLSchema#integer");

            Assert.IsNotNull(pMember);
            Assert.IsTrue(pMember is RDFTypedLiteral pMemberLiteral && pMemberLiteral.Equals(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_INTEGER)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnParsingNullPatternMember()
            => Assert.ThrowsException<RDFQueryException>(() => RDFQueryUtilities.ParseRDFPatternMember(null));
        #endregion
    }
}