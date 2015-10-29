/*
   Copyright 2012-2015 Marco De Salvo

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;
using RDFSharp.Store;
using RDFSharp.Query;

namespace RDFSharp.Semantics {

    /// <summary>
    /// RDFOntologyTaxonomy represents a register for storing a generic taxonomy relationship.
    /// </summary>
    public class RDFOntologyTaxonomy: IEnumerable<RDFOntologyTaxonomyEntry> {

        #region Properties
        /// <summary>
        /// Count of the taxonomy entries
        /// </summary>
        public Int64 EntriesCount {
            get { return this.Entries.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the taxonomy entries for iteration
        /// </summary>
        public IEnumerator<RDFOntologyTaxonomyEntry> EntriesEnumerator {
            get { return this.Entries.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Dictionary of ontology entries composing the taxonomy
        /// </summary>
        internal Dictionary<Int64, RDFOntologyTaxonomyEntry> Entries { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty ontology taxonomy
        /// </summary>
        internal RDFOntologyTaxonomy() {
            this.Entries = new Dictionary<Int64, RDFOntologyTaxonomyEntry>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the taxonomy entries
        /// </summary>
        IEnumerator<RDFOntologyTaxonomyEntry> IEnumerable<RDFOntologyTaxonomyEntry>.GetEnumerator() {
            return this.Entries.Values.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the taxonomy entries
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Entries.Values.GetEnumerator();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given taxonomy entry to the taxonomy
        /// </summary>
        internal RDFOntologyTaxonomy AddEntry(RDFOntologyTaxonomyEntry taxonomyEntry) {
            if (taxonomyEntry != null) {
                if (!this.Entries.ContainsKey(taxonomyEntry.TaxonomyEntryID)) {
                     this.Entries.Add(taxonomyEntry.TaxonomyEntryID, taxonomyEntry);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given taxonomy entry from the taxonomy
        /// </summary>
        internal RDFOntologyTaxonomy RemoveEntry(RDFOntologyTaxonomyEntry taxonomyEntry) {
            if (taxonomyEntry != null) {
                if (this.Entries.ContainsKey(taxonomyEntry.TaxonomyEntryID)) {
                    this.Entries.Remove(taxonomyEntry.TaxonomyEntryID);
                }
            }
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Checks if the taxonomy contains the given taxonomy entry
        /// </summary>
        internal Boolean ContainsEntry(RDFOntologyTaxonomyEntry taxonomyEntry) {
            return (taxonomyEntry != null && this.Entries.ContainsKey(taxonomyEntry.TaxonomyEntryID));
        }

        /// <summary>
        /// Gets a taxonomy with the entries having the specified ontology resource as subject 
        /// </summary>
        public RDFOntologyTaxonomy SelectEntriesBySubject(RDFOntologyResource subjectResource) {
            var resultTaxonomy   = new RDFOntologyTaxonomy();
            if (subjectResource != null) {                
                foreach (var te in this.Where(tEntry => tEntry.TaxonomySubject.Equals(subjectResource))) {
                    resultTaxonomy.AddEntry(te);
                }
            }
            return resultTaxonomy;
        }

        /// <summary>
        /// Gets a taxonomy with the entries having the specified ontology resource as predicate 
        /// </summary>
        public RDFOntologyTaxonomy SelectEntriesByPredicate(RDFOntologyResource predicateResource) {
            var resultTaxonomy     = new RDFOntologyTaxonomy();
            if (predicateResource != null) {
                foreach (var te   in this.Where(tEntry => tEntry.TaxonomyPredicate.Equals(predicateResource))) {
                    resultTaxonomy.AddEntry(te);
                }
            }
            return resultTaxonomy;
        }

        /// <summary>
        /// Gets a taxonomy with the entries having the specified ontology resource as object 
        /// </summary>
        public RDFOntologyTaxonomy SelectEntriesByObject(RDFOntologyResource objectResource) {
            var resultTaxonomy  = new RDFOntologyTaxonomy();
            if (objectResource != null) {
                foreach (var te in this.Where(tEntry => tEntry.TaxonomyObject.Equals(objectResource))) {
                    resultTaxonomy.AddEntry(te);
                }
            }
            return resultTaxonomy;
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection taxonomy from this taxonomy and a given one
        /// </summary>
        internal RDFOntologyTaxonomy IntersectWith(RDFOntologyTaxonomy taxonomy) {
            var result    = new RDFOntologyTaxonomy();
            if (taxonomy != null) {

                //Add intersection triples
                foreach (var te in this) {
                    if  (taxonomy.ContainsEntry(te)) {
                         result.AddEntry(te);
                    }
                }

            }
            return result;
        }

        /// <summary>
        /// Builds a new union taxonomy from this taxonomy and a given one
        /// </summary>
        internal RDFOntologyTaxonomy UnionWith(RDFOntologyTaxonomy taxonomy) {
            var result    = new RDFOntologyTaxonomy();

            //Add entries from this taxonomy
            foreach (var te in this) {
                result.AddEntry(te);
            }
            
            //Manage the given taxonomy
            if (taxonomy != null) {

                //Add entries from the given taxonomy
                foreach (var te in taxonomy) {
                    result.AddEntry(te);
                }

            }
            return result;
        }

        /// <summary>
        /// Builds a new difference taxonomy from this taxonomy and a given one
        /// </summary>
        internal RDFOntologyTaxonomy DifferenceWith(RDFOntologyTaxonomy taxonomy) {
            var result    = new RDFOntologyTaxonomy();
            if (taxonomy != null) {

                //Add difference entries
                foreach (var te in this) {
                    if  (!taxonomy.ContainsEntry(te)) {
                          result.AddEntry(te);
                    }
                }

            }
            else {

                //Add entries from this taxonomy
                foreach (var te in this) {
                    result.AddEntry(te);
                }

            }
            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this taxonomy, eventually including semantic inferences
        /// </summary>
        internal RDFGraph ToRDFGraph(Boolean includeInferences) {
            var result    = new RDFGraph();
            foreach (var te in this) {
                if (includeInferences) {
                    result.AddTriple(te.ToRDFTriple());
                }
                else {
                    if (!te.IsInference) {
                         result.AddTriple(te.ToRDFTriple());
                    }
                }
            }
            return result;
        }
        #endregion

        #endregion

    }

    #region RDFOntologyTaxonomyEntry
    /// <summary>
    /// RDFOntologyTaxonomy represents an entry of a RDFOntologyTaxonomy object.
    /// </summary>
    public class RDFOntologyTaxonomyEntry: IEquatable<RDFOntologyTaxonomyEntry> {

        #region Properties
        /// <summary>
        /// Unique representation of the taxonomy entry
        /// </summary>
        public Int64 TaxonomyEntryID { get; internal set; }

        /// <summary>
        /// Ontology resource acting as subject of the taxonomy relationship
        /// </summary>
        public RDFOntologyResource TaxonomySubject { get; internal set; }

        /// <summary>
        /// Ontology resource acting as predicate of the taxonomy relationship
        /// </summary>
        public RDFOntologyResource TaxonomyPredicate { get; internal set; }

        /// <summary>
        /// Ontology resource acting as object of the taxonomy relationship
        /// </summary>
        public RDFOntologyResource TaxonomyObject { get; internal set; }

        /// <summary>
        /// Flag indicating that this taxonomy entry represents a semantic inference
        /// </summary>
        public Boolean IsInference { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a taxonomy entry with the given subject, predicate and object resources
        /// </summary>
        internal RDFOntologyTaxonomyEntry(RDFOntologyResource taxonomySubject,
                                          RDFOntologyResource taxonomyPredicate,
                                          RDFOntologyResource taxonomyObject) {
            if (taxonomySubject        != null) {
                if (taxonomyPredicate  != null) {
                    if (taxonomyObject != null) {
                        this.TaxonomySubject   = taxonomySubject;
                        this.TaxonomyPredicate = taxonomyPredicate;
                        this.TaxonomyObject    = taxonomyObject;
                        this.TaxonomyEntryID   = RDFModelUtilities.CreateHash(this.ToString());
                    }
                    else {
                        throw new RDFSemanticsException("Cannot create RDFOntologyTaxonomyEntry because given \"taxonomyObject\" parameter is null.");
                    }    
                }
                else {
                    throw new RDFSemanticsException("Cannot create RDFOntologyTaxonomyEntry because given \"taxonomyPredicate\" parameter is null.");
                } 
            }
            else {
                throw new RDFSemanticsException("Cannot create RDFOntologyTaxonomyEntry because given \"taxonomySubject\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gets the string representation of the taxonomy entry
        /// </summary>
        public override String ToString() {
            return this.TaxonomySubject + " " + this.TaxonomyPredicate + " " + this.TaxonomyObject;
        }

        /// <summary>
        /// Performs the equality comparison between two taxonomy entries
        /// </summary>
        public Boolean Equals(RDFOntologyTaxonomyEntry other) {
            return (other != null && this.TaxonomyEntryID.Equals(other.TaxonomyEntryID));
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Marks this taxonomy entry as a semantic inference, depending on the given parameter
        /// </summary>
        internal RDFOntologyTaxonomyEntry SetInference(Boolean isInference) {
            this.IsInference = isInference;
            return this;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Get a triple representation of this taxonomy entry
        /// </summary>
        internal RDFTriple ToRDFTriple() {
            if (this.TaxonomyObject.IsLiteral()) {
                return new RDFTriple((RDFResource)this.TaxonomySubject.Value, (RDFResource)this.TaxonomyPredicate.Value, (RDFLiteral)this.TaxonomyObject.Value);
            }
            else {
                return new RDFTriple((RDFResource)this.TaxonomySubject.Value, (RDFResource)this.TaxonomyPredicate.Value, (RDFResource)this.TaxonomyObject.Value);
            }
        }
        #endregion

        #endregion

    }
    #endregion

}