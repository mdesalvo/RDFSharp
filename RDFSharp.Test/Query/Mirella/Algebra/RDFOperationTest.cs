/*
   Copyright 2012-2025 Marco De Salvo

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
using RDFSharp.Query;
using RDFSharp.Store;

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
            Assert.IsTrue(operation.QueryMembers.Count == 0);
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
            Assert.IsTrue(operation.QueryMembers.Count == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingDeleteGroundTemplateBecauseNullPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddDeleteGroundTemplate<RDFOperation>(null));

        [TestMethod]
        public void ShouldThrowExceptionOnAddingDeleteGroundTemplateBecauseNotGroundPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddDeleteGroundTemplate<RDFOperation>(new RDFPattern(new RDFVariable("?X"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));

        [TestMethod]
        public void ShouldAddDeleteNonGroundTemplate()
        {
            RDFPattern pattern1 = new RDFPattern(new RDFVariable("?C"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS);
            RDFPattern pattern2 = new RDFPattern(RDFVocabulary.RDF.LIST, new RDFVariable("?C"), RDFVocabulary.RDFS.CLASS);
            RDFPattern pattern3 = new RDFPattern(RDFVocabulary.RDF.LIST, RDFVocabulary.RDF.TYPE, new RDFVariable("?C"));
            RDFPattern pattern4 = new RDFPattern(new RDFVariable("?C"), RDFVocabulary.RDF.LIST, RDFVocabulary.RDF.TYPE, new RDFVariable("?C2"));
            RDFPattern pattern5 = new RDFPattern(new RDFVariable("?C1"), RDFVocabulary.RDF.LIST, RDFVocabulary.RDF.TYPE, new RDFVariable("?C2"));
            RDFPattern pattern6 = new RDFPattern(new RDFContext("ex:context"), RDFVocabulary.RDF.LIST, RDFVocabulary.RDF.TYPE, new RDFVariable("?C2"));
            RDFOperation operation = new RDFOperation();
            operation.AddDeleteNonGroundTemplate<RDFOperation>(pattern1);
            operation.AddDeleteNonGroundTemplate<RDFOperation>(pattern1); //Will be discarded, since duplicate patterns are not allowed
            operation.AddDeleteNonGroundTemplate<RDFOperation>(pattern2);
            operation.AddDeleteNonGroundTemplate<RDFOperation>(pattern3);
            operation.AddDeleteNonGroundTemplate<RDFOperation>(pattern4);
            operation.AddDeleteNonGroundTemplate<RDFOperation>(pattern5);
            operation.AddDeleteNonGroundTemplate<RDFOperation>(pattern6);

            Assert.IsTrue(operation.DeleteTemplates.Count == 6);
            Assert.IsTrue(operation.DeleteTemplates[0].Equals(pattern1));
            Assert.IsTrue(operation.DeleteTemplates[1].Equals(pattern2));
            Assert.IsTrue(operation.DeleteTemplates[2].Equals(pattern3));
            Assert.IsTrue(operation.DeleteTemplates[3].Equals(pattern4));
            Assert.IsTrue(operation.DeleteTemplates[4].Equals(pattern5));
            Assert.IsTrue(operation.DeleteTemplates[5].Equals(pattern6));
            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 3);
            Assert.IsTrue(operation.QueryMembers.Count == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingDeleteNonGroundTemplateBecauseNullPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddDeleteNonGroundTemplate<RDFOperation>(null));

        [TestMethod]
        public void ShouldAddInsertGroundTemplate()
        {
            RDFPattern pattern = new RDFPattern(RDFVocabulary.RDF.ALT, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS);
            RDFOperation operation = new RDFOperation();
            operation.AddInsertGroundTemplate<RDFOperation>(pattern);
            operation.AddInsertGroundTemplate<RDFOperation>(pattern); //Will be discarded, since duplicate patterns are not allowed

            Assert.IsTrue(operation.InsertTemplates.Count == 1);
            Assert.IsTrue(operation.InsertTemplates[0].Equals(pattern));
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingInsertGroundTemplateBecauseNullPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddInsertGroundTemplate<RDFOperation>(null));

        [TestMethod]
        public void ShouldThrowExceptionOnAddingInsertGroundTemplateBecauseNotGroundPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddInsertGroundTemplate<RDFOperation>(new RDFPattern(new RDFVariable("?X"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS)));

        [TestMethod]
        public void ShouldAddInsertNonGroundTemplate()
        {
            RDFPattern pattern1 = new RDFPattern(new RDFVariable("?C"), RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.CLASS);
            RDFPattern pattern2 = new RDFPattern(RDFVocabulary.RDF.LIST, new RDFVariable("?C"), RDFVocabulary.RDFS.CLASS);
            RDFPattern pattern3 = new RDFPattern(RDFVocabulary.RDF.LIST, new RDFVariable("?C1"), RDFVocabulary.RDFS.CLASS);
            RDFPattern pattern4 = new RDFPattern(RDFVocabulary.RDF.LIST, RDFVocabulary.RDF.TYPE, new RDFVariable("?C"));
            RDFPattern pattern5 = new RDFPattern(new RDFVariable("?C"), RDFVocabulary.RDF.LIST, RDFVocabulary.RDF.TYPE, new RDFVariable("?C2"));
            RDFPattern pattern6 = new RDFPattern(new RDFContext("ex:context"), RDFVocabulary.RDF.LIST, RDFVocabulary.RDF.TYPE, new RDFVariable("?C2"));
            RDFOperation operation = new RDFOperation();
            operation.AddInsertNonGroundTemplate<RDFOperation>(pattern1);
            operation.AddInsertNonGroundTemplate<RDFOperation>(pattern1); //Will be discarded, since duplicate patterns are not allowed
            operation.AddInsertNonGroundTemplate<RDFOperation>(pattern2);
            operation.AddInsertNonGroundTemplate<RDFOperation>(pattern3);
            operation.AddInsertNonGroundTemplate<RDFOperation>(pattern4);
            operation.AddInsertNonGroundTemplate<RDFOperation>(pattern5);
            operation.AddInsertNonGroundTemplate<RDFOperation>(pattern6);

            Assert.IsTrue(operation.InsertTemplates.Count == 6);
            Assert.IsTrue(operation.InsertTemplates[0].Equals(pattern1));
            Assert.IsTrue(operation.InsertTemplates[1].Equals(pattern2));
            Assert.IsTrue(operation.InsertTemplates[2].Equals(pattern3));
            Assert.IsTrue(operation.InsertTemplates[3].Equals(pattern4));
            Assert.IsTrue(operation.InsertTemplates[4].Equals(pattern5));
            Assert.IsTrue(operation.InsertTemplates[5].Equals(pattern6));
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 3);
            Assert.IsTrue(operation.QueryMembers.Count == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingInsertNonGroundTemplateBecauseNullPattern()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddInsertNonGroundTemplate<RDFOperation>(null));

        [TestMethod]
        public void ShouldAddPrefix()
        {
            RDFOperation operation = new RDFOperation();
            operation.AddPrefix<RDFOperation>(RDFNamespaceRegister.GetByPrefix("rdf"));
            operation.AddPrefix<RDFOperation>(RDFNamespaceRegister.GetByPrefix("rdf")); //Will be discarded, since duplicate prefixes are not allowed
            operation.AddPrefix<RDFOperation>(new RDFNamespace("rdf", $"{RDFVocabulary.RDF.BASE_URI}")); //Will be discarded, since duplicate prefixes are not allowed
            operation.AddPrefix<RDFOperation>(RDFNamespaceRegister.GetByPrefix("rdfs"));

            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 2);
            Assert.IsTrue(operation.QueryMembers.Count == 0);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingPrefixBecauseNullPrefix()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddPrefix<RDFOperation>(null));

        [TestMethod]
        public void ShouldAddPatternGroup()
        {
            RDFPatternGroup patternGroup = new RDFPatternGroup();
            RDFOperation operation = new RDFOperation();
            operation.AddPatternGroup<RDFOperation>(patternGroup);
            operation.AddPatternGroup<RDFOperation>(patternGroup); //Will be discarded, since duplicate pattern groups are not allowed

            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 1);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingPatternGroupBecauseNullPatternGroup()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddPatternGroup<RDFOperation>(null));

        [TestMethod]
        public void ShouldAddModifier()
        {
            RDFOperation operation = new RDFOperation();
            operation.AddModifier<RDFOperation>(new RDFDistinctModifier());
            operation.AddModifier<RDFOperation>(new RDFDistinctModifier()); //Will be discarded, since duplicate modifiers are not allowed

            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 1);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingModifierBecauseNullModifier()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddModifier<RDFOperation>(null));

        [TestMethod]
        public void ShouldAddSubQuery()
        {
            RDFSelectQuery subQuery = new RDFSelectQuery();
            RDFOperation operation = new RDFOperation();
            operation.AddSubQuery<RDFOperation>(subQuery);
            operation.AddSubQuery<RDFOperation>(subQuery); //Will be discarded, since duplicate sub queries are not allowed

            Assert.IsTrue(operation.InsertTemplates.Count == 0);
            Assert.IsTrue(operation.DeleteTemplates.Count == 0);
            Assert.IsTrue(operation.Variables.Count == 0);
            Assert.IsTrue(operation.Prefixes.Count == 0);
            Assert.IsTrue(operation.QueryMembers.Count == 1);
        }

        [TestMethod]
        public void ShouldThrowExceptionOnAddingSubQueryBecauseNullSubQuery()
            => Assert.ThrowsException<RDFQueryException>(() => new RDFOperation().AddSubQuery<RDFOperation>(null));
        #endregion
    }
}
