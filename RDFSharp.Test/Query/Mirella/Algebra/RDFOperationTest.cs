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
using System;
using System.Collections.Generic;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFOperationTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateOperation()
        {
            RDFOperation operation = new RDFOperation();

            Assert.IsNotNull(operation);
            Assert.IsNotNull(operation.DeleteTemplates);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsNotNull(operation.InsertTemplates);
            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsNotNull(operation.Variables);
            Assert.IsTrue(operation.Variables.Count == 0);
        }

        [TestMethod]
        public void ShouldAddDeleteGroundTemplate()
        {
            RDFPattern pattern = new RDFPattern(RDFVocabulary.RDF.ALT, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS);
            RDFOperation operation = new RDFOperation();
            operation.AddDeleteGroundTemplate<RDFOperation>(pattern);
            operation.AddDeleteGroundTemplate<RDFOperation>(pattern); //Will be discarded, since duplicate patterns are not allowed

            Assert.IsTrue(operation.DeleteTemplates.Count == 1);
            Assert.IsTrue(operation.DeleteTemplates[0].Equals(pattern));
            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingDeleteGroundTemplateBecauseNullPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddDeleteGroundTemplate<RDFOperation>(null));

        [TestMethod]
        public void ShouldThrowExceptionOnAddingDeleteGroundTemplateBecauseNotGroundPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddDeleteGroundTemplate<RDFOperation>(new RDFPattern(new RDFVariable("?X"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));
        #endregion
    }
}