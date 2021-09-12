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

namespace RDFSharp.Test
{
    [TestClass]
    public class RDFModelUtilitiesTest
    {
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
            Uri result = RDFModelUtilities.GetUriFromString(uriString);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Equals(new Uri(uriString)));
        }

        [DataTestMethod]
        [DataRow("bnode://example.org/test#")]
        [DataRow("bNoDE://example.org/test#")]
        [DataRow("_://example.org/test#")]
        public void ShouldGetBlankUriFromString(string uriString)
        {
            Uri result = RDFModelUtilities.GetUriFromString(uriString);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Equals(new Uri("bnode://example.org/test#")));
        }

        [DataTestMethod]
        [DataRow(null)]
        public void ShouldNotGetUriFromString(string uriString)
        {
            Uri result = RDFModelUtilities.GetUriFromString(uriString);
            Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow("/file/system")]
        public void ShouldNotGetRelativeUriFromString(string uriString)
        {
            Uri result = RDFModelUtilities.GetUriFromString(uriString);
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
            Uri result = RDFModelUtilities.RemapUriForDereference(new Uri(uriString));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Equals(new Uri(RDFVocabulary.FOAF.BASE_URI)));
        }

        [DataTestMethod]
        [DataRow("http://purl.org/dc/elements/1.1/")]
        public void ShouldRemapDifferentUriForDereference(string uriString)
        {
            Uri result = RDFModelUtilities.RemapUriForDereference(new Uri(uriString));
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Equals(new Uri(RDFVocabulary.DC.BASE_URI)));
        }

        [TestMethod]
        public void ShouldNotRemapNullUriForDereference()
        {
            Uri result = RDFModelUtilities.RemapUriForDereference(null);
            Assert.IsNull(result);
        }

        [DataTestMethod]
        [DataRow("http://example.org/test#")]
        public void ShouldNotRemapUnknownUriForDereference(string uriString)
        {
            Uri result = RDFModelUtilities.RemapUriForDereference(new Uri(uriString));
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Equals(new Uri("http://example.org/test#")));
        }

        [DataTestMethod]
        [DataRow("This is smiling face: \\U0001F603\\U0001F603; This is tilde: \\U0000007E\\U0001F603")]
        [DataRow("This is smiling face: \\U0001F603; This is tilde: \\U0000007E")]
        [DataRow("This is smiling face: \\U0001F603; This is tilde: \\u007E")]
        [DataRow("This is smiling face: \\U0001F603; This is tilde: \\u007E\\U0001F603")]
        [DataRow("This is smiling face: \\U0001F603\\u007E; This is tilde: \\u007E\\u007E")]
        public void ShouldTransformASCII_To_UnicodeWithSurrogatesAndUnicode(string input)
        {
            string output = RDFModelUtilities.ASCII_To_Unicode(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.IndexOf("\\U") == -1);
            Assert.IsTrue(output.IndexOf("\\u") == -1);
            Assert.IsTrue(output.IndexOf("😃") > -1);
            Assert.IsTrue(output.IndexOf("~") > -1);
        }

        [DataTestMethod]
        [DataRow("This is delta: \\U00000394; This is tilde: \\U0000007E")]
        [DataRow("This is delta: \\U00000394; This is tilde: \\u007E")]
        [DataRow("This is delta: \\u0394; This is tilde: \\u007E")]
        [DataRow("This is delta: \\u0394\\u0394; This is tilde: \\u007E\\u007E\\u007E")]
        [DataRow("This is delta: \\u0394; This is tilde: \\U0000007E")]
        public void ShouldTransformASCII_To_UnicodeWithUnicode(string input)
        {
            string output = RDFModelUtilities.ASCII_To_Unicode(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.IndexOf("\\U") == -1);
            Assert.IsTrue(output.IndexOf("\\u") == -1);
            Assert.IsTrue(output.IndexOf("~") > -1);
            Assert.IsTrue(output.IndexOf("Δ") > -1);
        }

        [DataTestMethod]
        [DataRow("This is smiling face: \\U0001F603; These are smiling faces: \\U0001F603\\U0001F603")]
        public void ShouldTransformASCII_To_UnicodeWithSurrogates(string input)
        {
            string output = RDFModelUtilities.ASCII_To_Unicode(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.IndexOf("\\U") == -1);
            Assert.IsTrue(output.IndexOf("\\u") == -1);
            Assert.IsTrue(output.IndexOf("😃") > -1);
        }

        [DataTestMethod]
        [DataRow("This is nothing!")]
        public void ShouldTransformASCII_To_UnicodeWithNoSurrogatesAndNoUnicode(string input)
        {
            string output = RDFModelUtilities.ASCII_To_Unicode(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.IndexOf("\\U") == -1);
            Assert.IsTrue(output.IndexOf("\\u") == -1);
        }

        [DataTestMethod]
        [DataRow(null)]
        public void ShouldNotTransformASCII_To_Unicode(string input)
        {
            string output = RDFModelUtilities.ASCII_To_Unicode(input);

            Assert.IsNull(output);
        }

        [DataTestMethod]
        [DataRow("\\U9\\u8")]
        public void ShouldNotTransformBadFormedASCII_To_Unicode(string input)
        {
            string output = RDFModelUtilities.ASCII_To_Unicode(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.Equals(input, StringComparison.Ordinal));
        }

        [DataTestMethod]
        [DataRow("This is a smiling face: 😃; This is Euro: €")]
        [DataRow("This is a smiling face: 😃😃; This is Euro: €😃")]
        [DataRow("This is a smiling face: 😃€; This is Euro: €😃€")]
        public void ShouldTransformUnicode_To_ASCIIWithSurrogatesAndUnicode(string input)
        {
            string output = RDFModelUtilities.Unicode_To_ASCII(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.IndexOf("\\U0001F603") > -1);
            Assert.IsTrue(output.IndexOf("\\u20AC") > -1);
        }

        [DataTestMethod]
        [DataRow("This is a smiling face: 😃;")]
        public void ShouldTransformUnicode_To_ASCIIWithSurrogates(string input)
        {
            string output = RDFModelUtilities.Unicode_To_ASCII(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.IndexOf("\\U0001F603") > -1);
            Assert.IsTrue(output.IndexOf("\\u") == -1);
        }

        [DataTestMethod]
        [DataRow("This is Euro: €;")]
        [DataRow("This is Euro: €€€;")]
        public void ShouldTransformUnicode_To_ASCIIWithUnicode(string input)
        {
            string output = RDFModelUtilities.Unicode_To_ASCII(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.IndexOf("\\U") == -1);
            Assert.IsTrue(output.IndexOf("\\u20AC") > -1);
        }

        [DataTestMethod]
        [DataRow("This is nothing!")]
        public void ShouldTransformUnicode_To_ASCIIWithNoSurrogatesAndNoUnicode(string input)
        {
            string output = RDFModelUtilities.Unicode_To_ASCII(input);

            Assert.IsNotNull(output);
            Assert.IsTrue(output.IndexOf("\\U") == -1);
            Assert.IsTrue(output.IndexOf("\\u") == -1);
        }

        [DataTestMethod]
        [DataRow(null)]
        public void ShouldNotTransformUnicode_To_ASCII(string input)
        {
            string output = RDFModelUtilities.Unicode_To_ASCII(input);

            Assert.IsNull(output);
        }
        #endregion
    }
}