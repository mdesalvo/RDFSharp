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
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Query;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFPropertyPathTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreatePropertyPath()
        {
            RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);

            Assert.IsNotNull(propertyPath);
            Assert.IsNotNull(propertyPath.Start);
            Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
            Assert.IsNotNull(propertyPath.End);
            Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsNotNull(propertyPath.Steps);
            Assert.IsTrue(propertyPath.Steps.Count == 0);
            Assert.IsTrue(propertyPath.Depth == 0);
            Assert.IsFalse(propertyPath.IsEvaluable);
            Assert.IsTrue(propertyPath.ToString().Equals($"?START  <{RDFVocabulary.RDF.TYPE}>"));
            Assert.IsTrue(propertyPath.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals($"?START  rdf:type"));
            Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullStart()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(null, new RDFVariable("?END")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseUnsupportedStart()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(new RDFPlainLiteral("start"), new RDFVariable("?END")));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullEnd()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(new RDFVariable("?START"), null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseUnsupportedEnd()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(new RDFVariable("?START"), new RDFPlainLiteral("end")));
        
        [TestMethod]
        public void ShouldAddSingleSequenceStep()
        {
            RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
            propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT));

            Assert.IsNotNull(propertyPath);
            Assert.IsNotNull(propertyPath.Start);
            Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
            Assert.IsNotNull(propertyPath.End);
            Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsNotNull(propertyPath.Steps);
            Assert.IsTrue(propertyPath.Steps.Count == 1);
            Assert.IsTrue(propertyPath.Depth == 1);
            Assert.IsTrue(propertyPath.IsEvaluable);
            Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>"));
            Assert.IsTrue(propertyPath.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals($"?START rdf:Alt rdf:type"));
            Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullSequenceStep()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
                                                                    .AddSequenceStep(null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullPropertyPathStepInSequenceStep()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
                                                                    .AddSequenceStep(new RDFPropertyPathStep(null)));

        [TestMethod]
        public void ShouldAddMultipleSequenceStep()
        {
            RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
            propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.ALT));
            propertyPath.AddSequenceStep(new RDFPropertyPathStep(RDFVocabulary.RDF.BAG));

            Assert.IsNotNull(propertyPath);
            Assert.IsNotNull(propertyPath.Start);
            Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
            Assert.IsNotNull(propertyPath.End);
            Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsNotNull(propertyPath.Steps);
            Assert.IsTrue(propertyPath.Steps.Count == 2);
            Assert.IsTrue(propertyPath.Depth == 2);
            Assert.IsTrue(propertyPath.IsEvaluable);
            Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}>/<{RDFVocabulary.RDF.BAG}> <{RDFVocabulary.RDF.TYPE}>"));
            Assert.IsTrue(propertyPath.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals($"?START rdf:Alt/rdf:Bag rdf:type"));
            Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldAddSingleAlternativeStep()
        {
            RDFPropertyPath propertyPath = new RDFPropertyPath(new RDFVariable("?START"), RDFVocabulary.RDF.TYPE);
            propertyPath.AddAlternativeSteps(new List<RDFPropertyPathStep>() { new RDFPropertyPathStep(RDFVocabulary.RDF.ALT) });
            
            Assert.IsNotNull(propertyPath);
            Assert.IsNotNull(propertyPath.Start);
            Assert.IsTrue(propertyPath.Start.Equals(new RDFVariable("?START")));
            Assert.IsNotNull(propertyPath.End);
            Assert.IsTrue(propertyPath.End.Equals(RDFVocabulary.RDF.TYPE));
            Assert.IsNotNull(propertyPath.Steps);
            Assert.IsTrue(propertyPath.Steps.Count == 1);
            Assert.IsTrue(propertyPath.Depth == 1);
            Assert.IsTrue(propertyPath.IsEvaluable);
            Assert.IsTrue(propertyPath.ToString().Equals($"?START <{RDFVocabulary.RDF.ALT}> <{RDFVocabulary.RDF.TYPE}>"));
            Assert.IsTrue(propertyPath.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals($"?START rdf:Alt rdf:type"));
            Assert.IsTrue(propertyPath.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(propertyPath.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullAlternativeStep()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
                                                                    .AddAlternativeSteps(null));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseEmptyAlternativeStep()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
                                                                    .AddAlternativeSteps(new List<RDFPropertyPathStep>()));

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPropertyPathBecauseNullPropertyPathStepInAlternativeSteps()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPropertyPath(new RDFVariable("?START"), new RDFVariable("?END"))
                                                                    .AddAlternativeSteps(new List<RDFPropertyPathStep>() { null }));
        #endregion
    }
}