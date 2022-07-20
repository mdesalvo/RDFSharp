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
        #endregion
    }
}