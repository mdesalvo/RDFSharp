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
using RDFSharp.Semantics.OWL;
using System.Collections.Generic;

namespace RDFSharp.Test.Semantics
{
    [TestClass]
    public class RDFOntologyTaxonomyTest
    {
        #region Tests
        [DataTestMethod]
        [DataRow(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Annotation, false)]
        [DataRow(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false)]
        [DataRow(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Data, false)]
        public void ShouldCreateTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory category, bool acceptDuplicates)
        {
            RDFOntologyTaxonomy taxonomy = new RDFOntologyTaxonomy(category, acceptDuplicates);

            Assert.IsNotNull(taxonomy);
            Assert.IsTrue(taxonomy.Category == category);
            Assert.IsNotNull(taxonomy.Entries);
            Assert.IsTrue(taxonomy.EntriesCount == 0);
            Assert.IsNotNull(taxonomy.EntriesLookup);
            Assert.IsTrue(taxonomy.EntriesLookup.Count == 0);
            Assert.IsTrue(taxonomy.AcceptDuplicates == acceptDuplicates);

            int i = 0;
            foreach (RDFOntologyTaxonomyEntry te in taxonomy) i++;
            Assert.IsTrue(i == 0);

            int j = 0;
            IEnumerator<RDFOntologyTaxonomyEntry> taxonomyEntriesEnumerator = taxonomy.EntriesEnumerator;
            while (taxonomyEntriesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 0);
        }

        [TestMethod]
        public void ShouldAddEntryNotAcceptingDuplicates()
        {
            RDFOntologyTaxonomy taxonomy = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, false);
            RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(new RDFOntologyClass(new RDFResource("ex:class")),new RDFOntologyProperty(new RDFResource("ex:predicate")),new RDFOntologyLiteral(new RDFPlainLiteral("lit")));
            
            Assert.IsTrue(taxonomy.AddEntry(taxonomyEntry));
            Assert.IsFalse(taxonomy.AddEntry(taxonomyEntry)); //Will be discarded, since duplicate taxonomy entries are not allowed (in this test)
            Assert.IsFalse(taxonomy.AddEntry(null)); //Will be discarded, since null taxonomy entries are not allowed
            Assert.IsTrue(taxonomy.EntriesCount == 1);
            Assert.IsTrue(taxonomy.EntriesLookup.Count == 1);

            int i = 0;
            foreach (RDFOntologyTaxonomyEntry te in taxonomy) i++;
            Assert.IsTrue(i == 1);

            int j = 0;
            IEnumerator<RDFOntologyTaxonomyEntry> taxonomyEntriesEnumerator = taxonomy.EntriesEnumerator;
            while (taxonomyEntriesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 1);
        }

        [TestMethod]
        public void ShouldAddEntryAcceptingDuplicates()
        {
            RDFOntologyTaxonomy taxonomy = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, true);
            RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(new RDFOntologyClass(new RDFResource("ex:class")),new RDFOntologyProperty(new RDFResource("ex:predicate")),new RDFOntologyLiteral(new RDFPlainLiteral("lit")));
            
            Assert.IsTrue(taxonomy.AddEntry(taxonomyEntry));
            Assert.IsTrue(taxonomy.AddEntry(taxonomyEntry));
            Assert.IsFalse(taxonomy.AddEntry(null)); //Will be discarded, since null taxonomy entries are not allowed
            Assert.IsTrue(taxonomy.EntriesCount == 2);
            Assert.IsTrue(taxonomy.EntriesLookup.Count == 1);

            int i = 0;
            foreach (RDFOntologyTaxonomyEntry te in taxonomy) i++;
            Assert.IsTrue(i == 2);

            int j = 0;
            IEnumerator<RDFOntologyTaxonomyEntry> taxonomyEntriesEnumerator = taxonomy.EntriesEnumerator;
            while (taxonomyEntriesEnumerator.MoveNext()) j++;
            Assert.IsTrue(j == 2);
        }

        [TestMethod]
        public void ShouldRemoveEntry()
        {
            RDFOntologyTaxonomy taxonomy = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, true);
            RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(new RDFOntologyClass(new RDFResource("ex:class")),new RDFOntologyProperty(new RDFResource("ex:predicate")),new RDFOntologyLiteral(new RDFPlainLiteral("lit")));
            RDFOntologyTaxonomyEntry taxonomyEntry2 = new RDFOntologyTaxonomyEntry(new RDFOntologyClass(new RDFResource("ex:class")),new RDFOntologyProperty(new RDFResource("ex:predicate")),new RDFOntologyLiteral(new RDFPlainLiteral("lit2")));
            taxonomy.AddEntry(taxonomyEntry);
            taxonomy.AddEntry(taxonomyEntry);

            Assert.IsTrue(taxonomy.RemoveEntry(taxonomyEntry));
            Assert.IsFalse(taxonomy.RemoveEntry(taxonomyEntry));
            Assert.IsFalse(taxonomy.RemoveEntry(taxonomyEntry2)); //This entry was not added to the taxonomy
            Assert.IsFalse(taxonomy.RemoveEntry(null)); //Will be discarded, since null taxonomy entries are not allowed
            Assert.IsTrue(taxonomy.EntriesCount == 0);
            Assert.IsTrue(taxonomy.EntriesLookup.Count == 0);
        }

        [TestMethod]
        public void ShouldContainEntry()
        {
            RDFOntologyTaxonomy taxonomy = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, true);
            RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(new RDFOntologyClass(new RDFResource("ex:class")),new RDFOntologyProperty(new RDFResource("ex:predicate")),new RDFOntologyLiteral(new RDFPlainLiteral("lit")));
            taxonomy.AddEntry(taxonomyEntry);
            
            Assert.IsTrue(taxonomy.ContainsEntry(taxonomyEntry));
        }

        [TestMethod]
        public void ShouldNotContainEntry()
        {
            RDFOntologyTaxonomy taxonomy = new RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory.Model, true);
            RDFOntologyTaxonomyEntry taxonomyEntry = new RDFOntologyTaxonomyEntry(new RDFOntologyClass(new RDFResource("ex:class")),new RDFOntologyProperty(new RDFResource("ex:predicate")),new RDFOntologyLiteral(new RDFPlainLiteral("lit")));
            RDFOntologyTaxonomyEntry taxonomyEntry2 = new RDFOntologyTaxonomyEntry(new RDFOntologyClass(new RDFResource("ex:class")),new RDFOntologyProperty(new RDFResource("ex:predicate")),new RDFOntologyLiteral(new RDFPlainLiteral("lit2")));
            taxonomy.AddEntry(taxonomyEntry);
            
            Assert.IsFalse(taxonomy.ContainsEntry(taxonomyEntry2));
            Assert.IsFalse(taxonomy.ContainsEntry(null));
        }
        #endregion
    }
}