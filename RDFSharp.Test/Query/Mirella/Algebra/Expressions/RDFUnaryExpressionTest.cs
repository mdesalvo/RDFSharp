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
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;
using RDFSharp.Query;
using System.Runtime.Serialization;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFUnaryExpressionTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateUnaryExpressionWithExpression()
        {
            RDFUnaryExpression expression = new RDFUnaryExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2")));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("((?V1 + ?V2))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("((?V1 + ?V2))"));
        }

        [TestMethod]
        public void ShouldCreateUnaryExpressionWithUnaryExpression()
        {
            RDFUnaryExpression expression = new RDFUnaryExpression(new RDFUnaryExpression(
                new RDFAddExpression(new RDFVariable("?V1"), new RDFVariable("?V2"))));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(((?V1 + ?V2)))"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(((?V1 + ?V2)))"));
        }

        [TestMethod]
        public void ShouldCreateUnaryExpressionWithVariable()
        {
            RDFUnaryExpression expression = new RDFUnaryExpression(new RDFVariable("?V"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(?V)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(?V)"));
        }

        [TestMethod]
        public void ShouldCreateUnaryExpressionWithResource()
        {
            RDFUnaryExpression expression = new RDFUnaryExpression(RDFVocabulary.FOAF.AGE);

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(<{RDFVocabulary.FOAF.AGE}>)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals($"(<{RDFVocabulary.FOAF.AGE}>)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("foaf") }).Equals("(foaf:age)"));
        }

        [TestMethod]
        public void ShouldCreateUnaryExpressionWithPlainLiteral()
        {
            RDFUnaryExpression expression = new RDFUnaryExpression(new RDFPlainLiteral("lit","en-US"));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(\"lit\"@EN-US)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(\"lit\"@EN-US)"));
        }

        [TestMethod]
        public void ShouldCreateUnaryExpressionWithNumericTypedLiteral()
        {
            RDFUnaryExpression expression = new RDFUnaryExpression(new RDFTypedLiteral("25.04", RDFModelEnums.RDFDatatypes.XSD_FLOAT));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals("(25.04)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals("(25.04)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals("(25.04)"));
        }

        [TestMethod]
        public void ShouldCreateUnaryExpressionWithNotNumericTypedLiteral()
        {
            RDFUnaryExpression expression = new RDFUnaryExpression(new RDFTypedLiteral("25", RDFModelEnums.RDFDatatypes.XSD_GDAY));

            Assert.IsNotNull(expression);
            Assert.IsNotNull(expression.LeftArgument);
            Assert.IsNull(expression.RightArgument);
            Assert.IsTrue(expression.ToString().Equals($"(\"25Z\"^^<{RDFVocabulary.XSD.G_DAY}>)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>()).Equals($"(\"25Z\"^^<{RDFVocabulary.XSD.G_DAY}>)"));
            Assert.IsTrue(expression.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("xsd") }).Equals($"(\"25Z\"^^xsd:gDay)"));
        }
        #endregion
    }
}