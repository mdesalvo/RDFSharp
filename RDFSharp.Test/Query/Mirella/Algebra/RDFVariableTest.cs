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
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFVariableTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateVariable()
        {
            RDFVariable variable1 = new RDFVariable("var");

            Assert.IsNotNull(variable1);
            Assert.IsTrue(variable1.VariableName.Equals("?VAR"));
            Assert.IsTrue(variable1.ToString().Equals("?VAR"));

            RDFVariable variable2 = new RDFVariable("?var");

            Assert.IsNotNull(variable2);
            Assert.IsTrue(variable2.VariableName.Equals("?VAR"));
            Assert.IsTrue(variable2.ToString().Equals("?VAR"));

            Assert.IsTrue(variable1.Equals(variable2));
        }

        [TestMethod]
        public void ShouldCreateVariableTrimmingUndesiredSpaces()
        {
            RDFVariable variable = new RDFVariable(" ?var ");

            Assert.IsNotNull(variable);
            Assert.IsTrue(variable.VariableName.Equals("?VAR"));
            Assert.IsTrue(variable.ToString().Equals("?VAR"));
        }

        [TestMethod]
        public void ShouldCreateVariableTrimmingUndesiredQuestionMarks()
        {
            RDFVariable variable = new RDFVariable("?var?");

            Assert.IsNotNull(variable);
            Assert.IsTrue(variable.VariableName.Equals("?VAR"));
            Assert.IsTrue(variable.ToString().Equals("?VAR"));
        }

        [TestMethod]
        public void ShouldCreateVariableTrimmingUndesiredDollars()
        {
            RDFVariable variable = new RDFVariable("$var$");

            Assert.IsNotNull(variable);
            Assert.IsTrue(variable.VariableName.Equals("?VAR"));
            Assert.IsTrue(variable.ToString().Equals("?VAR"));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVariableBecauseNullName()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFVariable(null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVariableBecauseEmptyName()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFVariable(string.Empty));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVariableBecauseTrimmedEmptyNameQuestionMarks()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFVariable("??"));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVariableBecauseTrimmedEmptyNameDollars()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFVariable("$$"));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVariableBecauseTrimmedEmptyNameSpaces()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFVariable("  "));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingVariableBecauseTrimmedEmptyMixed()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFVariable("?  $"));
        #endregion
    }
}