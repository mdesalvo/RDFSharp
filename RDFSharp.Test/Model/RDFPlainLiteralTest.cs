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
    public class RDFPlainLiteralTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow("donald duck")]
        [DataRow("donald duck@")]
        [DataRow("donald duck@en-US")] //Even if well-formed, this input will be threated as unlanguaged (in fact the ctor is for unlanguaged ones)
        [DataRow("@en")]
        [DataRow("")]
        [DataRow(null)]
        public void ShouldCreatePlainLiteral(string value)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value);

            Assert.IsNotNull(pl);
            Assert.IsFalse(pl.HasLanguage());
            Assert.IsTrue(pl.ToString().Equals(value ?? ""));
        }

        [DataTestMethod]
        [DataRow("donald duck", "")]
        [DataRow("donald duck@", null)]
        [DataRow("donald duck", "-")]
        [DataRow("donald duck", "25")]
        [DataRow("donald duck", "abcdefghi")]
        [DataRow("donald duck", "en-")]
        [DataRow("donald duck", "en-US-")]
        [DataRow("donald duck", "en-US-123456789")]
        [DataRow("donald duck", "@en-US")]
        [DataRow("", "@en-US")]
        [DataRow("", "")]
        [DataRow("", null)]
        [DataRow(null, "@en-US")]
        [DataRow(null, "")]
        [DataRow(null, null)]
        public void ShouldCreatePlainLiteralWithEmptyLanguage(string value, string language)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value, language);

            Assert.IsNotNull(pl);
            Assert.IsFalse(pl.HasLanguage());
            Assert.IsTrue(pl.ToString().Equals(value ?? ""));
        }

        [DataTestMethod]
        [DataRow("donal duck", "en")]
        [DataRow("donal duck", "en-US")]
        [DataRow("donal duck", "en-US-25")]
        [DataRow("donal duck@en-US", "en-US")]
        [DataRow("donal duck@", "en")]
        [DataRow("", "en")]
        [DataRow(null, "en")]
        public void ShouldCreatePlainLiteralWithLanguage(string value, string language)
        {
            RDFPlainLiteral pl = new RDFPlainLiteral(value, language);

            Assert.IsNotNull(pl);
            Assert.IsTrue(pl.HasLanguage());
            Assert.IsTrue(pl.ToString().Equals(string.Concat(value, "@", language.ToUpperInvariant())));
        }        
        #endregion
    }
}