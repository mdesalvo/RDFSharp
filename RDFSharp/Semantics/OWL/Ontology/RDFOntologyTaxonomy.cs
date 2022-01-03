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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics.OWL
{
    /// <summary>
    /// RDFOntologyTaxonomy represents a register for storing a generic taxonomy relationship.
    /// </summary>
    public class RDFOntologyTaxonomy : IEnumerable<RDFOntologyTaxonomyEntry>
    {
        #region Properties
        /// <summary>
        /// Category of the ontology taxonomy
        /// </summary>
        public RDFSemanticsEnums.RDFOntologyTaxonomyCategory Category { get; internal set; }

        /// <summary>
        /// Count of the taxonomy entries
        /// </summary>
        public long EntriesCount => this.Entries.Count;

        /// <summary>
        /// Gets the enumerator on the taxonomy entries for iteration
        /// </summary>
        public IEnumerator<RDFOntologyTaxonomyEntry> EntriesEnumerator => this.Entries.GetEnumerator();

        /// <summary>
        /// Dictionary of ontology entries composing the taxonomy
        /// </summary>
        internal List<RDFOntologyTaxonomyEntry> Entries { get; set; }

        /// <summary>
        /// Lookup for ontology entries composing the taxonomy
        /// </summary>
        internal HashSet<long> EntriesLookup { get; set; }

        /// <summary>
        /// Flag indicating that this taxonomy exceptionally accepts duplicate entries
        /// </summary>
        internal bool AcceptDuplicates { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology taxonomy of the given category
        /// </summary>
        internal RDFOntologyTaxonomy(RDFSemanticsEnums.RDFOntologyTaxonomyCategory category, bool acceptDuplicates)
        {
            this.Category = category;
            this.Entries = new List<RDFOntologyTaxonomyEntry>();
            this.EntriesLookup = new HashSet<long>();
            this.AcceptDuplicates = acceptDuplicates;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the taxonomy entries
        /// </summary>
        IEnumerator<RDFOntologyTaxonomyEntry> IEnumerable<RDFOntologyTaxonomyEntry>.GetEnumerator() => this.EntriesEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the taxonomy entries
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => this.EntriesEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given taxonomy entry to the taxonomy.
        /// Returns true if the insertion has been made.
        /// </summary>
        internal bool AddEntry(RDFOntologyTaxonomyEntry taxonomyEntry)
        {
            if (taxonomyEntry != null)
            {
                if (this.AcceptDuplicates || !this.ContainsEntry(taxonomyEntry))
                {
                    this.Entries.Add(taxonomyEntry);
                    if (!this.EntriesLookup.Contains(taxonomyEntry.TaxonomyEntryID))
                        this.EntriesLookup.Add(taxonomyEntry.TaxonomyEntryID);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given taxonomy entry from the taxonomy.
        /// Returns true if the deletion has been made.
        /// </summary>
        internal bool RemoveEntry(RDFOntologyTaxonomyEntry taxonomyEntry)
        {
            if (taxonomyEntry != null)
            {
                if (this.ContainsEntry(taxonomyEntry))
                {
                    this.Entries.RemoveAll(te => te.Equals(taxonomyEntry));
                    this.EntriesLookup.Remove(taxonomyEntry.TaxonomyEntryID);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the taxonomy contains the given taxonomy entry
        /// </summary>
        public bool ContainsEntry(RDFOntologyTaxonomyEntry taxonomyEntry)
            => taxonomyEntry != null && this.EntriesLookup.Contains(taxonomyEntry.TaxonomyEntryID);

        /// <summary>
        /// Gets a taxonomy with the entries having the specified ontology resource as subject
        /// </summary>
        public RDFOntologyTaxonomy SelectEntriesBySubject(RDFOntologyResource subjectResource)
        {
            var resultTaxonomy = new RDFOntologyTaxonomy(this.Category, this.AcceptDuplicates);
            if (subjectResource != null)
            {
                foreach (var te in this.Where(tEntry => tEntry.TaxonomySubject.Equals(subjectResource)))
                    resultTaxonomy.AddEntry(te);
            }
            return resultTaxonomy;
        }

        /// <summary>
        /// Gets a taxonomy with the entries having the specified ontology resource as predicate
        /// </summary>
        public RDFOntologyTaxonomy SelectEntriesByPredicate(RDFOntologyResource predicateResource)
        {
            var resultTaxonomy = new RDFOntologyTaxonomy(this.Category, this.AcceptDuplicates);
            if (predicateResource != null)
            {
                foreach (var te in this.Where(tEntry => tEntry.TaxonomyPredicate.Equals(predicateResource)))
                    resultTaxonomy.AddEntry(te);
            }
            return resultTaxonomy;
        }

        /// <summary>
        /// Gets a taxonomy with the entries having the specified ontology resource as object
        /// </summary>
        public RDFOntologyTaxonomy SelectEntriesByObject(RDFOntologyResource objectResource)
        {
            var resultTaxonomy = new RDFOntologyTaxonomy(this.Category, this.AcceptDuplicates);
            if (objectResource != null)
            {
                foreach (var te in this.Where(tEntry => tEntry.TaxonomyObject.Equals(objectResource)))
                    resultTaxonomy.AddEntry(te);
            }
            return resultTaxonomy;
        }

        /// <summary>
        /// Gets the taxonomy entry having the specified identifier
        /// </summary>
        public RDFOntologyTaxonomyEntry SelectEntryByID(long taxonomyEntryID)
            => this.EntriesLookup.Contains(taxonomyEntryID) ? this.Entries.Find(tEntry => tEntry.TaxonomyEntryID == taxonomyEntryID) : null;
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection taxonomy from this taxonomy and a given one
        /// </summary>
        internal RDFOntologyTaxonomy IntersectWith(RDFOntologyTaxonomy taxonomy)
        {
            RDFOntologyTaxonomy result = new RDFOntologyTaxonomy(this.Category, this.AcceptDuplicates);
            if (taxonomy != null)
            {
                //Add intersection triples
                foreach (RDFOntologyTaxonomyEntry te in this)
                {
                    if (taxonomy.ContainsEntry(te))
                        result.AddEntry(te);
                }
            }
            return result;
        }

        /// <summary>
        /// Builds a new union taxonomy from this taxonomy and a given one
        /// </summary>
        internal RDFOntologyTaxonomy UnionWith(RDFOntologyTaxonomy taxonomy)
        {
            RDFOntologyTaxonomy result = new RDFOntologyTaxonomy(this.Category, this.AcceptDuplicates);

            //Add entries from this taxonomy
            foreach (RDFOntologyTaxonomyEntry te in this)
                result.AddEntry(te);

            //Manage the given taxonomy
            if (taxonomy != null)
            {
                //Add entries from the given taxonomy
                foreach (RDFOntologyTaxonomyEntry te in taxonomy)
                    result.AddEntry(te);
            }

            return result;
        }

        /// <summary>
        /// Builds a new difference taxonomy from this taxonomy and a given one
        /// </summary>
        internal RDFOntologyTaxonomy DifferenceWith(RDFOntologyTaxonomy taxonomy)
        {
            RDFOntologyTaxonomy result = new RDFOntologyTaxonomy(this.Category, this.AcceptDuplicates);
            if (taxonomy != null)
            {
                //Add difference entries
                foreach (RDFOntologyTaxonomyEntry te in this)
                {
                    if (!taxonomy.ContainsEntry(te))
                        result.AddEntry(te);
                }
            }
            else
            {
                //Add entries from this taxonomy
                foreach (RDFOntologyTaxonomyEntry te in this)
                    result.AddEntry(te);
            }
            return result;
        }
        #endregion

        #endregion
    }
}