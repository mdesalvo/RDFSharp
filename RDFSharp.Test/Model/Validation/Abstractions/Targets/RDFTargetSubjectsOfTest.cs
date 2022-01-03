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

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFTargetSubjectsOfTest
    {
        #region Test
        [TestMethod]
        public void ShouldCreateTargetSubjectsOf()
        {
            RDFTargetSubjectsOf targetSubjectsOf = new RDFTargetSubjectsOf(new RDFResource("ex:targetSubjectsOf"));

            Assert.IsNotNull(targetSubjectsOf);
            Assert.IsTrue(targetSubjectsOf.TargetValue.Equals(new RDFResource("ex:targetSubjectsOf")));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingTargetSubjectsOfBecauseNullValue()
            => Assert.ThrowsException<RDFModelException>(() => new RDFTargetSubjectsOf(null));

        [TestMethod]
        public void ShouldExportTargetSubjectsOf()
        {
            RDFTargetSubjectsOf targetSubjectsOf = new RDFTargetSubjectsOf(new RDFResource("ex:targetSubjectsOf"));
            RDFGraph graph = targetSubjectsOf.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:nodeShape")));

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 1);
            Assert.IsTrue(graph.ContainsTriple(new RDFTriple(new RDFResource("ex:nodeShape"), RDFVocabulary.SHACL.TARGET_SUBJECTS_OF, targetSubjectsOf.TargetValue)));
        }
        #endregion
    }
}