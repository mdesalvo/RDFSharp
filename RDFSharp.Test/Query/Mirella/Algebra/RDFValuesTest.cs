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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RDFSharp.Query;
using RDFSharp.Model;

namespace RDFSharp.Test.Query
{
    [TestClass]
    public class RDFValuesTest
    {
        #region Tests
        [TestMethod]
        public void ShouldCreateValues()
        {
            RDFValues values = new RDFValues();

            Assert.IsNotNull(values);
            Assert.IsNotNull(values.Bindings);
            Assert.IsTrue(values.Bindings.Count == 0);
            Assert.IsTrue(values.MaxBindingsLength() == 0);
            Assert.IsFalse(values.IsEvaluable);
            Assert.IsFalse(values.IsInjected);
            Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES () {", Environment.NewLine, "    }")));
            Assert.IsTrue(values.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(values.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldAddColumns()
        {
            RDFValues values = new RDFValues();
            values.AddColumn(new RDFVariable("?V1"), new List<RDFPatternMember>() { RDFVocabulary.RDF.TYPE });
            values.AddColumn(new RDFVariable("?V2"), new List<RDFPatternMember>() { RDFVocabulary.FOAF.KNOWS });
            values.AddColumn(new RDFVariable("?V3"), null);

            Assert.IsNotNull(values);
            Assert.IsNotNull(values.Bindings);
            Assert.IsTrue(values.Bindings.Count == 3);
            Assert.IsTrue(values.MaxBindingsLength() == 1);
            Assert.IsTrue(values.IsEvaluable);
            Assert.IsFalse(values.IsInjected);
            Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "    }")));
            Assert.IsTrue(values.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }, string.Empty).Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( rdf:type <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "    }")));
            Assert.IsTrue(values.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(values.PatternGroupMemberStringID)));
        }

        [TestMethod]
        public void ShouldAddColumnsAtVariableBindingslength()
        {
            RDFValues values = new RDFValues();
            values.AddColumn(new RDFVariable("?V1"), new List<RDFPatternMember>() { RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT });
            values.AddColumn(new RDFVariable("?V2"), new List<RDFPatternMember>() { RDFVocabulary.FOAF.KNOWS });
            values.AddColumn(new RDFVariable("?V3"), null);

            Assert.IsNotNull(values);
            Assert.IsNotNull(values.Bindings);
            Assert.IsTrue(values.Bindings.Count == 3);
            Assert.IsTrue(values.MaxBindingsLength() == 2);
            Assert.IsTrue(values.IsEvaluable);
            Assert.IsFalse(values.IsInjected);
            Assert.IsTrue(values.ToString().Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "      ( <http://www.w3.org/1999/02/22-rdf-syntax-ns#Alt> UNDEF UNDEF )", Environment.NewLine, "    }")));
            Assert.IsTrue(values.ToString(new List<RDFNamespace>() { RDFNamespaceRegister.GetByPrefix("rdf") }, string.Empty).Equals(string.Concat("VALUES (?V1 ?V2 ?V3) {", Environment.NewLine, "      ( rdf:type <http://xmlns.com/foaf/0.1/knows> UNDEF )", Environment.NewLine, "      ( rdf:Alt UNDEF UNDEF )", Environment.NewLine, "    }")));
            Assert.IsTrue(values.PatternGroupMemberID.Equals(RDFModelUtilities.CreateHash(values.PatternGroupMemberStringID)));
        }
        #endregion
    }
}