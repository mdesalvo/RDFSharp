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
    public class RDFPatternGroupTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreatePatternGroup()
        {
            RDFPatternGroup pGroup = new RDFPatternGroup(" pGroup ");

            Assert.IsNotNull(pGroup);
            Assert.IsTrue(pGroup.PatternGroupName.Equals("PGROUP"));
            Assert.IsTrue(pGroup.IsEvaluable);
            Assert.IsFalse(pGroup.IsOptional);
            Assert.IsFalse(pGroup.JoinAsUnion);
            Assert.IsNotNull(pGroup.GroupMembers);
            Assert.IsTrue(pGroup.GroupMembers.Count == 0);
            Assert.IsNotNull(pGroup.Variables);
            Assert.IsTrue(pGroup.Variables.Count == 0);
            Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "  }", Environment.NewLine)));
            Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.PatternGroupName)));
        }

        [TestMethod]
        public void ShouldThrowExceptionOnCreatingPatternGroupBecauseNullOrWhitespaceName()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFPatternGroup(null));

        [TestMethod]
        public void ShouldCreatePatternGroupWithPatterns()
        {
            RDFPatternGroup pGroup = new RDFPatternGroup(" pGroup ", 
                new List<RDFPattern>() {
                    new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS),
                    new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")),
                    new RDFPattern(new RDFVariable("c"), new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS),
                    new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS), //Will not be added, since duplicate patterns are not allowed
                    null, //Will not be added, since null is not allowed
                    new RDFPattern(RDFVocabulary.OWL.CLASS, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS), //Will not be added, since ground patterns are not allowed
                });

            Assert.IsNotNull(pGroup);
            Assert.IsTrue(pGroup.PatternGroupName.Equals("PGROUP"));
            Assert.IsTrue(pGroup.IsEvaluable);
            Assert.IsFalse(pGroup.IsOptional);
            Assert.IsFalse(pGroup.JoinAsUnion);
            Assert.IsNotNull(pGroup.GroupMembers);
            Assert.IsTrue(pGroup.GroupMembers.Count == 3);
            Assert.IsNotNull(pGroup.Variables);
            Assert.IsTrue(pGroup.Variables.Count == 4);
            Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine, "    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "  }", Environment.NewLine)));
            Assert.IsTrue(pGroup.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals(string.Concat("  {", Environment.NewLine, "    ?S rdf:type <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine,"    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S rdf:type <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "  }", Environment.NewLine)));
            Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.PatternGroupName)));
        }

        [TestMethod]
        public void ShouldCreatePatternGroupWithPatternsAndFilters()
        {
            RDFPatternGroup pGroup = new RDFPatternGroup(" pGroup ", 
                new List<RDFPattern>() {
                    new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS),
                    new RDFPattern(new RDFVariable("s"), new RDFVariable("p"), new RDFVariable("o")),
                    new RDFPattern(new RDFVariable("c"), new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS),
                    new RDFPattern(new RDFVariable("s"), RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS), //Will not be added, since duplicate patterns are not allowed
                    null, //Will not be added, since null is not allowed
                    new RDFPattern(RDFVocabulary.OWL.CLASS, RDFVocabulary.RDF.TYPE, RDFVocabulary.OWL.CLASS), //Will not be added, since ground patterns are not allowed
                },
                new List<RDFFilter>() {
                    new RDFIsUriFilter(new RDFVariable("s")),
                    null, //Will not be added, since null is not allowed
                    new RDFIsUriFilter(new RDFVariable("s")) //Will not be added, since duplicate filters are not allowed
                });

            Assert.IsNotNull(pGroup);
            Assert.IsTrue(pGroup.PatternGroupName.Equals("PGROUP"));
            Assert.IsTrue(pGroup.IsEvaluable);
            Assert.IsFalse(pGroup.IsOptional);
            Assert.IsFalse(pGroup.JoinAsUnion);
            Assert.IsNotNull(pGroup.GroupMembers);
            Assert.IsTrue(pGroup.GroupMembers.Count == 4);
            Assert.IsNotNull(pGroup.Variables);
            Assert.IsTrue(pGroup.Variables.Count == 4);
            Assert.IsTrue(pGroup.ToString().Equals(string.Concat("  {", Environment.NewLine, "    ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine, "    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "    FILTER ( ISURI(?S) ) ", Environment.NewLine, "  }", Environment.NewLine)));
            Assert.IsTrue(pGroup.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }).Equals(string.Concat("  {", Environment.NewLine, "    ?S rdf:type <http://www.w3.org/2002/07/owl#Class> .", Environment.NewLine,"    ?S ?P ?O .", Environment.NewLine, "    GRAPH ?C { ?S rdf:type <http://www.w3.org/2002/07/owl#Class> } .", Environment.NewLine, "    FILTER ( ISURI(?S) ) ", Environment.NewLine, "  }", Environment.NewLine)));
            Assert.IsTrue(pGroup.QueryMemberID.Equals(RDFModelUtilities.CreateHash(pGroup.PatternGroupName)));
        }
        #endregion
    }
}