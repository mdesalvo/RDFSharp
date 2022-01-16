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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RDFSharp.Test.Model
{
    [TestClass]
    public class RDFPatternConstraintTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreatePatternConstraint()
        {
            RDFPatternConstraint patternConstraint = new RDFPatternConstraint(new Regex("^test$", RegexOptions.IgnoreCase));

            Assert.IsNotNull(patternConstraint);
            Assert.IsNotNull(patternConstraint.RegEx.Equals(new Regex("^test$", RegexOptions.IgnoreCase)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternConstraint()
            => Assert.ThrowsException<RDFModelException>(() => new RDFPatternConstraint(null));

        [TestMethod]
        public void ShouldExportPatternConstraint()
        {
            RDFPatternConstraint patternConstraint = new RDFPatternConstraint(new Regex("^test$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace));
            RDFGraph graph = patternConstraint.ToRDFGraph(new RDFNodeShape(new RDFResource("ex:NodeShape")));

            Assert.IsNotNull(graph);
            Assert.IsTrue(graph.TriplesCount == 2);
            Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                    && t.Value.Predicate.Equals(RDFVocabulary.SHACL.PATTERN)
                                                        && t.Value.Object.Equals(new RDFTypedLiteral("^test$", RDFModelEnums.RDFDatatypes.XSD_STRING))));
            Assert.IsTrue(graph.Triples.Any(t => t.Value.Subject.Equals(new RDFResource("ex:NodeShape"))
                                                    && t.Value.Predicate.Equals(RDFVocabulary.SHACL.FLAGS)
                                                        && t.Value.Object.Equals(new RDFTypedLiteral("ismx", RDFModelEnums.RDFDatatypes.XSD_STRING))));
        }

        //NS-CONFORMS: TRUE

        //PS-CONFORMS: TRUE

        //NS-CONFORMS: FALSE

        //PS-CONFORMS: FALSE
        
        #endregion
    }
}