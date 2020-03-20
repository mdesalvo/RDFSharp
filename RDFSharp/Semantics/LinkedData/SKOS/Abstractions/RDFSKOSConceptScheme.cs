﻿/*
   Copyright 2015-2020 Marco De Salvo

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

using RDFSharp.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RDFSharp.Semantics
{

    /// <summary>
    /// RDFSKOSConceptScheme represents an instance of skos:ConceptScheme within an ontology data.
    /// </summary>
    public class RDFSKOSConceptScheme : RDFOntologyFact, IEnumerable<RDFSKOSConcept>  {

        #region Properties
        /// <summary>
        /// Count of the concepts composing the scheme
        /// </summary>
        public Int64 ConceptsCount {
            get { return this.Concepts.Count; }
        }

        /// <summary>
        /// Count of the collections composing the scheme
        /// </summary>
        public Int64 CollectionsCount {
            get { return this.Collections.Count; }
        }

        /// <summary>
        /// Count of the ordered collections composing the scheme
        /// </summary>
        public Int64 OrderedCollectionsCount {
            get { return this.OrderedCollections.Count; }
        }

        /// <summary>
        /// Count of the labels composing the scheme
        /// </summary>
        public Int64 LabelsCount {
            get { return this.Labels.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the concepts of the scheme for iteration
        /// </summary>
        public IEnumerator<RDFSKOSConcept> ConceptsEnumerator {
            get { return this.Concepts.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Gets the enumerator on the collections of the scheme for iteration
        /// </summary>
        public IEnumerator<RDFSKOSCollection> CollectionsEnumerator {
            get { return this.Collections.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Gets the enumerator on the ordered collections of the scheme for iteration
        /// </summary>
        public IEnumerator<RDFSKOSOrderedCollection> OrderedCollectionsEnumerator {
            get { return this.OrderedCollections.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Gets the enumerator on the labels of the scheme for iteration
        /// </summary>
        public IEnumerator<RDFSKOSLabel> LabelsEnumerator {
            get { return this.Labels.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Annotations describing concepts of the scheme
        /// </summary>
        public RDFSKOSAnnotations Annotations { get; internal set; }

        /// <summary>
        /// Relations describing concepts of the scheme
        /// </summary>
        public RDFSKOSRelations Relations { get; internal set; }

        /// <summary>
        /// Concepts contained in the scheme (encodes the 'skos:inScheme' relation)
        /// </summary>
        internal Dictionary<Int64, RDFSKOSConcept> Concepts { get; set; }

        /// <summary>
        /// Collections contained in the scheme
        /// </summary>
        internal Dictionary<Int64, RDFSKOSCollection> Collections { get; set; }

        /// <summary>
        /// OrderedCollections contained in the scheme
        /// </summary>
        internal Dictionary<Int64, RDFSKOSOrderedCollection> OrderedCollections { get; set; }

        /// <summary>
        /// Labels contained in the scheme
        /// </summary>
        internal Dictionary<Int64, RDFSKOSLabel> Labels { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a conceptScheme with the given name
        /// </summary>
        public RDFSKOSConceptScheme(RDFResource conceptName) : base(conceptName) {
            this.Concepts           = new Dictionary<Int64, RDFSKOSConcept>();
            this.Collections        = new Dictionary<Int64, RDFSKOSCollection>();
            this.OrderedCollections = new Dictionary<Int64, RDFSKOSOrderedCollection>();
            this.Labels             = new Dictionary<Int64, RDFSKOSLabel>();
            this.Annotations        = new RDFSKOSAnnotations();
            this.Relations          = new RDFSKOSRelations();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the scheme's concepts
        /// </summary>
        IEnumerator<RDFSKOSConcept> IEnumerable<RDFSKOSConcept>.GetEnumerator() {
            return this.ConceptsEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the scheme's concepts
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.ConceptsEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given concept to the scheme
        /// </summary>
        public RDFSKOSConceptScheme AddConcept(RDFSKOSConcept concept) {
            if (concept != null) {
                if (!this.Concepts.ContainsKey(concept.PatternMemberID)) {
                     this.Concepts.Add(concept.PatternMemberID, concept);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given collection to the scheme
        /// </summary>
        public RDFSKOSConceptScheme AddCollection(RDFSKOSCollection collection) {
            if (collection != null) {
                if (!this.Collections.ContainsKey(collection.PatternMemberID)) {
                     this.Collections.Add(collection.PatternMemberID, collection);
                     //Also add concepts of the collection
                     foreach (var cn in collection.Concepts.Values)
                         this.AddConcept(cn);
                     //Also adds collections of the collection
                     foreach (var cl in collection.Collections.Values)
                         this.AddCollection(cl);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given ordered collection to the scheme
        /// </summary>
        public RDFSKOSConceptScheme AddOrderedCollection(RDFSKOSOrderedCollection orderedCollection) {
            if (orderedCollection    != null) {
                if (!this.OrderedCollections.ContainsKey(orderedCollection.PatternMemberID)) {
                     this.OrderedCollections.Add(orderedCollection.PatternMemberID, orderedCollection);
                     //Also add concepts of the ordered collection
                     foreach (var cn in orderedCollection.Concepts.Values.OrderBy(x => x.Item1))
                         this.AddConcept(cn.Item2);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given label to the scheme
        /// </summary>
        public RDFSKOSConceptScheme AddLabel(RDFSKOSLabel label) {
            if (label != null) {
                if (!this.Labels.ContainsKey(label.PatternMemberID)) {
                     this.Labels.Add(label.PatternMemberID, label);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given concept from the scheme
        /// </summary>
        public RDFSKOSConceptScheme RemoveConcept(RDFSKOSConcept concept) {
            if (concept != null) {
                if (this.Concepts.ContainsKey(concept.PatternMemberID)) {
                    this.Concepts.Remove(concept.PatternMemberID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given collection from the scheme
        /// </summary>
        public RDFSKOSConceptScheme RemoveCollection(RDFSKOSCollection collection) {
            if (collection != null) {
                if (this.Collections.ContainsKey(collection.PatternMemberID)) {
                    this.Collections.Remove(collection.PatternMemberID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given ordered collection from the scheme
        /// </summary>
        public RDFSKOSConceptScheme RemoveOrderedCollection(RDFSKOSOrderedCollection orderedCollection) {
            if (orderedCollection != null) {
                if (this.OrderedCollections.ContainsKey(orderedCollection.PatternMemberID)) {
                    this.OrderedCollections.Remove(orderedCollection.PatternMemberID);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes the given label from the scheme
        /// </summary>
        public RDFSKOSConceptScheme RemoveLabel(RDFSKOSLabel label) {
            if (label != null) {
                if (this.Labels.ContainsKey(label.PatternMemberID)) {
                    this.Labels.Remove(label.PatternMemberID);
                }
            }
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the concept represented by the given string from the scheme
        /// </summary>
        public RDFSKOSConcept SelectConcept(String concept) {
            if (concept         != null) {
                Int64 conceptID  = RDFModelUtilities.CreateHash(concept);
                if (this.Concepts.ContainsKey(conceptID)) {
                    return this.Concepts[conceptID];
                }
            }
            return null;
        }

        /// <summary>
        /// Selects the collection represented by the given string from the scheme
        /// </summary>
        public RDFSKOSCollection SelectCollection(String collection) {
            if (collection         != null) {
                Int64 collectionID  = RDFModelUtilities.CreateHash(collection);
                if (this.Collections.ContainsKey(collectionID)) {
                    return this.Collections[collectionID];
                }
            }
            return null;
        }

        /// <summary>
        /// Selects the ordered collection represented by the given string from the scheme
        /// </summary>
        public RDFSKOSOrderedCollection SelectOrderedCollection(String orderedCollection) {
            if (orderedCollection         != null) {
                Int64 orderedCollectionID  = RDFModelUtilities.CreateHash(orderedCollection);
                if (this.OrderedCollections.ContainsKey(orderedCollectionID)) {
                    return this.OrderedCollections[orderedCollectionID];
                }
            }
            return null;
        }

        /// <summary>
        /// Selects the label represented by the given string from the scheme
        /// </summary>
        public RDFSKOSLabel SelectLabel(String label) {
            if (label         != null) {
                Int64 labelID  = RDFModelUtilities.CreateHash(label);
                if (this.Labels.ContainsKey(labelID)) {
                    return this.Labels[labelID];
                }
            }
            return null;
        }
        #endregion

        #region Set
        /// <summary>
        /// Builds a new intersection scheme from this scheme and a given one
        /// </summary>
        public RDFSKOSConceptScheme IntersectWith(RDFSKOSConceptScheme conceptScheme) {
            var result          = new RDFSKOSConceptScheme(new RDFResource());
            if (conceptScheme  != null) {

                //Add intersection concepts
                foreach (var c in this) {
                     if (conceptScheme.Concepts.ContainsKey(c.PatternMemberID)) {
                         result.AddConcept(c);
                    }
                }

                //Add intersection collections
                foreach (var c in this.Collections.Values) {
                    if  (conceptScheme.Collections.ContainsKey(c.PatternMemberID)) {
                         result.AddCollection(c);
                    }
                }

                //Add intersection ordered collections
                foreach (var o in this.OrderedCollections.Values) {
                    if  (conceptScheme.OrderedCollections.ContainsKey(o.PatternMemberID)) {
                         result.AddOrderedCollection(o);
                    }
                }

                //Add intersection labels
                foreach (var l in this.Labels.Values) {
                    if  (conceptScheme.Labels.ContainsKey(l.PatternMemberID)) {
                         result.AddLabel(l);
                    }
                }

                //Add intersection relations
                result.Relations.TopConcept         = this.Relations.TopConcept.IntersectWith(conceptScheme.Relations.TopConcept);
                result.Relations.Broader            = this.Relations.Broader.IntersectWith(conceptScheme.Relations.Broader);
                result.Relations.BroaderTransitive  = this.Relations.BroaderTransitive.IntersectWith(conceptScheme.Relations.BroaderTransitive);
                result.Relations.BroadMatch         = this.Relations.BroadMatch.IntersectWith(conceptScheme.Relations.BroadMatch);
                result.Relations.Narrower           = this.Relations.Narrower.IntersectWith(conceptScheme.Relations.Narrower);
                result.Relations.NarrowerTransitive = this.Relations.NarrowerTransitive.IntersectWith(conceptScheme.Relations.NarrowerTransitive);
                result.Relations.NarrowMatch        = this.Relations.NarrowMatch.IntersectWith(conceptScheme.Relations.NarrowMatch);
                result.Relations.Related            = this.Relations.Related.IntersectWith(conceptScheme.Relations.Related);
                result.Relations.RelatedMatch       = this.Relations.RelatedMatch.IntersectWith(conceptScheme.Relations.RelatedMatch);
                result.Relations.SemanticRelation   = this.Relations.SemanticRelation.IntersectWith(conceptScheme.Relations.SemanticRelation);
                result.Relations.MappingRelation    = this.Relations.MappingRelation.IntersectWith(conceptScheme.Relations.MappingRelation);
                result.Relations.CloseMatch         = this.Relations.CloseMatch.IntersectWith(conceptScheme.Relations.CloseMatch);
                result.Relations.ExactMatch         = this.Relations.ExactMatch.IntersectWith(conceptScheme.Relations.ExactMatch);
                result.Relations.Notation           = this.Relations.Notation.IntersectWith(conceptScheme.Relations.Notation);
                result.Relations.PrefLabel          = this.Relations.PrefLabel.IntersectWith(conceptScheme.Relations.PrefLabel);
                result.Relations.AltLabel           = this.Relations.AltLabel.IntersectWith(conceptScheme.Relations.AltLabel);
                result.Relations.HiddenLabel        = this.Relations.HiddenLabel.IntersectWith(conceptScheme.Relations.HiddenLabel);
                result.Relations.LiteralForm        = this.Relations.LiteralForm.IntersectWith(conceptScheme.Relations.LiteralForm);
                result.Relations.LabelRelation      = this.Relations.LabelRelation.IntersectWith(conceptScheme.Relations.LabelRelation);

                //Add intersection annotations
                result.Annotations.PrefLabel        = this.Annotations.PrefLabel.IntersectWith(conceptScheme.Annotations.PrefLabel);
                result.Annotations.AltLabel         = this.Annotations.AltLabel.IntersectWith(conceptScheme.Annotations.AltLabel);
                result.Annotations.HiddenLabel      = this.Annotations.HiddenLabel.IntersectWith(conceptScheme.Annotations.HiddenLabel);
                result.Annotations.Note             = this.Annotations.Note.IntersectWith(conceptScheme.Annotations.Note);
                result.Annotations.ChangeNote       = this.Annotations.ChangeNote.IntersectWith(conceptScheme.Annotations.ChangeNote);
                result.Annotations.EditorialNote    = this.Annotations.EditorialNote.IntersectWith(conceptScheme.Annotations.EditorialNote);
                result.Annotations.HistoryNote      = this.Annotations.HistoryNote.IntersectWith(conceptScheme.Annotations.HistoryNote);
                result.Annotations.ScopeNote        = this.Annotations.ScopeNote.IntersectWith(conceptScheme.Annotations.ScopeNote);
                result.Annotations.Definition       = this.Annotations.Definition.IntersectWith(conceptScheme.Annotations.Definition);
                result.Annotations.Example          = this.Annotations.Example.IntersectWith(conceptScheme.Annotations.Example);

            }
            return result;
        }

        /// <summary>
        /// Builds a new union scheme from this scheme and a given one
        /// </summary>
        public RDFSKOSConceptScheme UnionWith(RDFSKOSConceptScheme conceptScheme) {
            var result         = new RDFSKOSConceptScheme(new RDFResource());

            //Add concepts from this scheme
            foreach (var c    in this) {
                result.AddConcept(c);
            }

            //Add collections from this scheme
            foreach (var c    in this.Collections.Values) {
                result.AddCollection(c);
            }

            //Add ordered collections from this scheme
            foreach (var o    in this.OrderedCollections.Values) {
                result.AddOrderedCollection(o);
            }

            //Add labels from this scheme
            foreach (var l    in this.Labels.Values) {
                result.AddLabel(l);
            }

            //Add relations from this scheme
            result.Relations.TopConcept         = result.Relations.TopConcept.UnionWith(this.Relations.TopConcept);
            result.Relations.Broader            = result.Relations.Broader.UnionWith(this.Relations.Broader);
            result.Relations.BroaderTransitive  = result.Relations.BroaderTransitive.UnionWith(this.Relations.BroaderTransitive);
            result.Relations.BroadMatch         = result.Relations.BroadMatch.UnionWith(this.Relations.BroadMatch);
            result.Relations.Narrower           = result.Relations.Narrower.UnionWith(this.Relations.Narrower);
            result.Relations.NarrowerTransitive = result.Relations.NarrowerTransitive.UnionWith(this.Relations.NarrowerTransitive);
            result.Relations.NarrowMatch        = result.Relations.NarrowMatch.UnionWith(this.Relations.NarrowMatch);
            result.Relations.Related            = result.Relations.Related.UnionWith(this.Relations.Related);
            result.Relations.RelatedMatch       = result.Relations.RelatedMatch.UnionWith(this.Relations.RelatedMatch);
            result.Relations.SemanticRelation   = result.Relations.SemanticRelation.UnionWith(this.Relations.SemanticRelation);
            result.Relations.MappingRelation    = result.Relations.MappingRelation.UnionWith(this.Relations.MappingRelation);
            result.Relations.CloseMatch         = result.Relations.CloseMatch.UnionWith(this.Relations.CloseMatch);
            result.Relations.ExactMatch         = result.Relations.ExactMatch.UnionWith(this.Relations.ExactMatch);
            result.Relations.Notation           = result.Relations.Notation.UnionWith(this.Relations.Notation);
            result.Relations.PrefLabel          = result.Relations.PrefLabel.UnionWith(this.Relations.PrefLabel);
            result.Relations.AltLabel           = result.Relations.AltLabel.UnionWith(this.Relations.AltLabel);
            result.Relations.HiddenLabel        = result.Relations.HiddenLabel.UnionWith(this.Relations.HiddenLabel);
            result.Relations.LiteralForm        = result.Relations.LiteralForm.UnionWith(this.Relations.LiteralForm);
            result.Relations.LabelRelation      = result.Relations.LabelRelation.UnionWith(this.Relations.LabelRelation);

            //Add annotations from this scheme
            result.Annotations.PrefLabel        = result.Annotations.PrefLabel.UnionWith(this.Annotations.PrefLabel);
            result.Annotations.AltLabel         = result.Annotations.AltLabel.UnionWith(this.Annotations.AltLabel);
            result.Annotations.HiddenLabel      = result.Annotations.HiddenLabel.UnionWith(this.Annotations.HiddenLabel);
            result.Annotations.Note             = result.Annotations.Note.UnionWith(this.Annotations.Note);
            result.Annotations.ChangeNote       = result.Annotations.ChangeNote.UnionWith(this.Annotations.ChangeNote);
            result.Annotations.EditorialNote    = result.Annotations.EditorialNote.UnionWith(this.Annotations.EditorialNote);
            result.Annotations.HistoryNote      = result.Annotations.HistoryNote.UnionWith(this.Annotations.HistoryNote);
            result.Annotations.ScopeNote        = result.Annotations.ScopeNote.UnionWith(this.Annotations.ScopeNote);
            result.Annotations.Definition       = result.Annotations.Definition.UnionWith(this.Annotations.Definition);
            result.Annotations.Example          = result.Annotations.Example.UnionWith(this.Annotations.Example);

            //Manage the given scheme
            if (conceptScheme  != null) {

                //Add concepts from the given scheme
                foreach (var c in conceptScheme) {
                    result.AddConcept(c);
                }

                //Add collections from the given scheme
                foreach (var c in conceptScheme.Collections.Values) {
                    result.AddCollection(c);
                }

                //Add ordered collections from the given scheme
                foreach (var o in conceptScheme.OrderedCollections.Values) {
                    result.AddOrderedCollection(o);
                }

                //Add labels from the given scheme
                foreach (var l in conceptScheme.Labels.Values) {
                    result.AddLabel(l);
                }

                //Add relations from the given scheme
                result.Relations.TopConcept         = result.Relations.TopConcept.UnionWith(conceptScheme.Relations.TopConcept);
                result.Relations.Broader            = result.Relations.Broader.UnionWith(conceptScheme.Relations.Broader);
                result.Relations.BroaderTransitive  = result.Relations.BroaderTransitive.UnionWith(conceptScheme.Relations.BroaderTransitive);
                result.Relations.BroadMatch         = result.Relations.BroadMatch.UnionWith(conceptScheme.Relations.BroadMatch);
                result.Relations.Narrower           = result.Relations.Narrower.UnionWith(conceptScheme.Relations.Narrower);
                result.Relations.NarrowerTransitive = result.Relations.NarrowerTransitive.UnionWith(conceptScheme.Relations.NarrowerTransitive);
                result.Relations.NarrowMatch        = result.Relations.NarrowMatch.UnionWith(conceptScheme.Relations.NarrowMatch);
                result.Relations.Related            = result.Relations.Related.UnionWith(conceptScheme.Relations.Related);
                result.Relations.RelatedMatch       = result.Relations.RelatedMatch.UnionWith(conceptScheme.Relations.RelatedMatch);
                result.Relations.SemanticRelation   = result.Relations.SemanticRelation.UnionWith(conceptScheme.Relations.SemanticRelation);
                result.Relations.MappingRelation    = result.Relations.MappingRelation.UnionWith(conceptScheme.Relations.MappingRelation);
                result.Relations.CloseMatch         = result.Relations.CloseMatch.UnionWith(conceptScheme.Relations.CloseMatch);
                result.Relations.ExactMatch         = result.Relations.ExactMatch.UnionWith(conceptScheme.Relations.ExactMatch);
                result.Relations.Notation           = result.Relations.Notation.UnionWith(conceptScheme.Relations.Notation);
                result.Relations.PrefLabel          = result.Relations.PrefLabel.UnionWith(conceptScheme.Relations.PrefLabel);
                result.Relations.AltLabel           = result.Relations.AltLabel.UnionWith(conceptScheme.Relations.AltLabel);
                result.Relations.HiddenLabel        = result.Relations.HiddenLabel.UnionWith(conceptScheme.Relations.HiddenLabel);
                result.Relations.LiteralForm        = result.Relations.LiteralForm.UnionWith(conceptScheme.Relations.LiteralForm);
                result.Relations.LabelRelation      = result.Relations.LabelRelation.UnionWith(conceptScheme.Relations.LabelRelation);

                //Add annotations from the given scheme
                result.Annotations.PrefLabel        = result.Annotations.PrefLabel.UnionWith(conceptScheme.Annotations.PrefLabel);
                result.Annotations.AltLabel         = result.Annotations.AltLabel.UnionWith(conceptScheme.Annotations.AltLabel);
                result.Annotations.HiddenLabel      = result.Annotations.HiddenLabel.UnionWith(conceptScheme.Annotations.HiddenLabel);
                result.Annotations.Note             = result.Annotations.Note.UnionWith(conceptScheme.Annotations.Note);
                result.Annotations.ChangeNote       = result.Annotations.ChangeNote.UnionWith(conceptScheme.Annotations.ChangeNote);
                result.Annotations.EditorialNote    = result.Annotations.EditorialNote.UnionWith(conceptScheme.Annotations.EditorialNote);
                result.Annotations.HistoryNote      = result.Annotations.HistoryNote.UnionWith(conceptScheme.Annotations.HistoryNote);
                result.Annotations.ScopeNote        = result.Annotations.ScopeNote.UnionWith(conceptScheme.Annotations.ScopeNote);
                result.Annotations.Definition       = result.Annotations.Definition.UnionWith(conceptScheme.Annotations.Definition);
                result.Annotations.Example          = result.Annotations.Example.UnionWith(conceptScheme.Annotations.Example);

            }
            return result;
        }

        /// <summary>
        /// Builds a new difference scheme from this scheme and a given one
        /// </summary>
        public RDFSKOSConceptScheme DifferenceWith(RDFSKOSConceptScheme conceptScheme)
        {
            var result          = new RDFSKOSConceptScheme(new RDFResource());
            if (conceptScheme  != null) {

                //Add difference concepts
                foreach (var c in this) {
                    if (!conceptScheme.Concepts.ContainsKey(c.PatternMemberID)) {
                         result.AddConcept(c);
                    }
                }

                //Add difference collections
                foreach (var c in this.Collections.Values) {
                    if (!conceptScheme.Collections.ContainsKey(c.PatternMemberID)) {
                         result.AddCollection(c);
                    }
                }

                //Add difference ordered collections
                foreach (var o in this.OrderedCollections.Values) {
                    if (!conceptScheme.OrderedCollections.ContainsKey(o.PatternMemberID)) {
                         result.AddOrderedCollection(o);
                    }
                }

                //Add difference labels
                foreach (var l in this.Labels.Values) {
                    if (!conceptScheme.Labels.ContainsKey(l.PatternMemberID)) {
                         result.AddLabel(l);
                    }
                }

                //Add difference relations
                result.Relations.TopConcept         = this.Relations.TopConcept.DifferenceWith(conceptScheme.Relations.TopConcept);
                result.Relations.Broader            = this.Relations.Broader.DifferenceWith(conceptScheme.Relations.Broader);
                result.Relations.BroaderTransitive  = this.Relations.BroaderTransitive.DifferenceWith(conceptScheme.Relations.BroaderTransitive);
                result.Relations.BroadMatch         = this.Relations.BroadMatch.DifferenceWith(conceptScheme.Relations.BroadMatch);
                result.Relations.Narrower           = this.Relations.Narrower.DifferenceWith(conceptScheme.Relations.Narrower);
                result.Relations.NarrowerTransitive = this.Relations.NarrowerTransitive.DifferenceWith(conceptScheme.Relations.NarrowerTransitive);
                result.Relations.NarrowMatch        = this.Relations.NarrowMatch.DifferenceWith(conceptScheme.Relations.NarrowMatch);
                result.Relations.Related            = this.Relations.Related.DifferenceWith(conceptScheme.Relations.Related);
                result.Relations.RelatedMatch       = this.Relations.RelatedMatch.DifferenceWith(conceptScheme.Relations.RelatedMatch);
                result.Relations.SemanticRelation   = this.Relations.SemanticRelation.DifferenceWith(conceptScheme.Relations.SemanticRelation);
                result.Relations.MappingRelation    = this.Relations.MappingRelation.DifferenceWith(conceptScheme.Relations.MappingRelation);
                result.Relations.CloseMatch         = this.Relations.CloseMatch.DifferenceWith(conceptScheme.Relations.CloseMatch);
                result.Relations.ExactMatch         = this.Relations.ExactMatch.DifferenceWith(conceptScheme.Relations.ExactMatch);
                result.Relations.Notation           = this.Relations.Notation.DifferenceWith(conceptScheme.Relations.Notation);
                result.Relations.PrefLabel          = this.Relations.PrefLabel.DifferenceWith(conceptScheme.Relations.PrefLabel);
                result.Relations.AltLabel           = this.Relations.AltLabel.DifferenceWith(conceptScheme.Relations.AltLabel);
                result.Relations.HiddenLabel        = this.Relations.HiddenLabel.DifferenceWith(conceptScheme.Relations.HiddenLabel);
                result.Relations.LiteralForm        = this.Relations.LiteralForm.DifferenceWith(conceptScheme.Relations.LiteralForm);
                result.Relations.LabelRelation      = this.Relations.LabelRelation.DifferenceWith(conceptScheme.Relations.LabelRelation);

                //Add difference annotations
                result.Annotations.PrefLabel        = this.Annotations.PrefLabel.DifferenceWith(conceptScheme.Annotations.PrefLabel);
                result.Annotations.AltLabel         = this.Annotations.AltLabel.DifferenceWith(conceptScheme.Annotations.AltLabel);
                result.Annotations.HiddenLabel      = this.Annotations.HiddenLabel.DifferenceWith(conceptScheme.Annotations.HiddenLabel);
                result.Annotations.Note             = this.Annotations.Note.DifferenceWith(conceptScheme.Annotations.Note);
                result.Annotations.ChangeNote       = this.Annotations.ChangeNote.DifferenceWith(conceptScheme.Annotations.ChangeNote);
                result.Annotations.EditorialNote    = this.Annotations.EditorialNote.DifferenceWith(conceptScheme.Annotations.EditorialNote);
                result.Annotations.HistoryNote      = this.Annotations.HistoryNote.DifferenceWith(conceptScheme.Annotations.HistoryNote);
                result.Annotations.ScopeNote        = this.Annotations.ScopeNote.DifferenceWith(conceptScheme.Annotations.ScopeNote);
                result.Annotations.Definition       = this.Annotations.Definition.DifferenceWith(conceptScheme.Annotations.Definition);
                result.Annotations.Example          = this.Annotations.Example.DifferenceWith(conceptScheme.Annotations.Example);

            }
            else {

                //Add concepts from this scheme
                foreach (var c in this) {
                    result.AddConcept(c);
                }

                //Add collections from this scheme
                foreach (var c in this.Collections.Values) {
                    result.AddCollection(c);
                }

                //Add ordered collections from this scheme
                foreach (var o in this.OrderedCollections.Values) {
                    result.AddOrderedCollection(o);
                }

                //Add labels from this scheme
                foreach (var l in this.Labels.Values) {
                    result.AddLabel(l);
                }

                //Add relations from this scheme
                result.Relations.TopConcept         = result.Relations.TopConcept.UnionWith(this.Relations.TopConcept);
                result.Relations.Broader            = result.Relations.Broader.UnionWith(this.Relations.Broader);
                result.Relations.BroaderTransitive  = result.Relations.BroaderTransitive.UnionWith(this.Relations.BroaderTransitive);
                result.Relations.BroadMatch         = result.Relations.BroadMatch.UnionWith(this.Relations.BroadMatch);
                result.Relations.Narrower           = result.Relations.Narrower.UnionWith(this.Relations.Narrower);
                result.Relations.NarrowerTransitive = result.Relations.NarrowerTransitive.UnionWith(this.Relations.NarrowerTransitive);
                result.Relations.NarrowMatch        = result.Relations.NarrowMatch.UnionWith(this.Relations.NarrowMatch);
                result.Relations.Related            = result.Relations.Related.UnionWith(this.Relations.Related);
                result.Relations.RelatedMatch       = result.Relations.RelatedMatch.UnionWith(this.Relations.RelatedMatch);
                result.Relations.SemanticRelation   = result.Relations.SemanticRelation.UnionWith(this.Relations.SemanticRelation);
                result.Relations.MappingRelation    = result.Relations.MappingRelation.UnionWith(this.Relations.MappingRelation);
                result.Relations.CloseMatch         = result.Relations.CloseMatch.UnionWith(this.Relations.CloseMatch);
                result.Relations.ExactMatch         = result.Relations.ExactMatch.UnionWith(this.Relations.ExactMatch);
                result.Relations.Notation           = result.Relations.Notation.UnionWith(this.Relations.Notation);
                result.Relations.PrefLabel          = result.Relations.PrefLabel.UnionWith(this.Relations.PrefLabel);
                result.Relations.AltLabel           = result.Relations.AltLabel.UnionWith(this.Relations.AltLabel);
                result.Relations.HiddenLabel        = result.Relations.HiddenLabel.UnionWith(this.Relations.HiddenLabel);
                result.Relations.LiteralForm        = result.Relations.LiteralForm.UnionWith(this.Relations.LiteralForm);
                result.Relations.LabelRelation      = result.Relations.LabelRelation.UnionWith(this.Relations.LabelRelation);

                //Add annotations from this scheme
                result.Annotations.PrefLabel        = result.Annotations.PrefLabel.UnionWith(this.Annotations.PrefLabel);
                result.Annotations.AltLabel         = result.Annotations.AltLabel.UnionWith(this.Annotations.AltLabel);
                result.Annotations.HiddenLabel      = result.Annotations.HiddenLabel.UnionWith(this.Annotations.HiddenLabel);
                result.Annotations.Note             = result.Annotations.Note.UnionWith(this.Annotations.Note);
                result.Annotations.ChangeNote       = result.Annotations.ChangeNote.UnionWith(this.Annotations.ChangeNote);
                result.Annotations.EditorialNote    = result.Annotations.EditorialNote.UnionWith(this.Annotations.EditorialNote);
                result.Annotations.HistoryNote      = result.Annotations.HistoryNote.UnionWith(this.Annotations.HistoryNote);
                result.Annotations.ScopeNote        = result.Annotations.ScopeNote.UnionWith(this.Annotations.ScopeNote);
                result.Annotations.Definition       = result.Annotations.Definition.UnionWith(this.Annotations.Definition);
                result.Annotations.Example          = result.Annotations.Example.UnionWith(this.Annotations.Example);

            }
            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this scheme, exporting inferences according to the selected behavior
        /// </summary>
        public RDFGraph ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior) {
            return this.ToRDFOntologyData().ToRDFGraph(infexpBehavior);
        }
        
        /// <summary>
        /// Gets an ontology data representation of this scheme
        /// </summary>
        public RDFOntologyData ToRDFOntologyData() {
            var result      = new RDFOntologyData();

            //ConceptScheme
            result.AddFact(this);
            result.AddClassTypeRelation(this, RDFVocabulary.SKOS.CONCEPT_SCHEME.ToRDFOntologyClass());

            //Concepts
            foreach (var c in this) {
                result.AddFact(c);
                result.AddClassTypeRelation(c, RDFVocabulary.SKOS.CONCEPT.ToRDFOntologyClass());
                result.AddAssertionRelation(c, RDFVocabulary.SKOS.IN_SCHEME.ToRDFOntologyObjectProperty(), this);
            }

            //Collections
            foreach (var c in this.Collections.Values) {
                result.AddAssertionRelation(c, RDFVocabulary.SKOS.IN_SCHEME.ToRDFOntologyObjectProperty(), this);
                result      = result.UnionWith(c.ToRDFOntologyData());
            }

            //OrderedCollections
            foreach (var o in this.OrderedCollections.Values) {
                result.AddAssertionRelation(o, RDFVocabulary.SKOS.IN_SCHEME.ToRDFOntologyObjectProperty(), this);
                result      = result.UnionWith(o.ToRDFOntologyData());
            }

            //Labels
            foreach (var l in this.Labels.Values) {
                result.AddFact(l);
                result.AddClassTypeRelation(l, RDFVocabulary.SKOS.SKOSXL.LABEL.ToRDFOntologyClass());
                result.AddAssertionRelation(l, RDFVocabulary.SKOS.IN_SCHEME.ToRDFOntologyObjectProperty(), this);
            }

            //Assertions
            result.Relations.Assertions = result.Relations.Assertions.UnionWith(this.Relations.TopConcept)
                                                                     .UnionWith(this.Relations.Broader)
                                                                     .UnionWith(this.Relations.BroaderTransitive)
                                                                     .UnionWith(this.Relations.BroadMatch)
                                                                     .UnionWith(this.Relations.Narrower)
                                                                     .UnionWith(this.Relations.NarrowerTransitive)
                                                                     .UnionWith(this.Relations.NarrowMatch)
                                                                     .UnionWith(this.Relations.Related)
                                                                     .UnionWith(this.Relations.RelatedMatch)
                                                                     .UnionWith(this.Relations.SemanticRelation)
                                                                     .UnionWith(this.Relations.MappingRelation)
                                                                     .UnionWith(this.Relations.CloseMatch)
                                                                     .UnionWith(this.Relations.ExactMatch)
                                                                     .UnionWith(this.Relations.Notation)
                                                                     .UnionWith(this.Relations.PrefLabel)
                                                                     .UnionWith(this.Relations.AltLabel)
                                                                     .UnionWith(this.Relations.HiddenLabel)
                                                                     .UnionWith(this.Relations.LiteralForm)
                                                                     .UnionWith(this.Relations.LabelRelation);


            //Annotations
            result.Annotations.CustomAnnotations = result.Annotations.CustomAnnotations.UnionWith(this.Annotations.PrefLabel)
                                                                                       .UnionWith(this.Annotations.AltLabel)
                                                                                       .UnionWith(this.Annotations.HiddenLabel)
                                                                                       .UnionWith(this.Annotations.Note)
                                                                                       .UnionWith(this.Annotations.ChangeNote)
                                                                                       .UnionWith(this.Annotations.EditorialNote)
                                                                                       .UnionWith(this.Annotations.HistoryNote)
                                                                                       .UnionWith(this.Annotations.ScopeNote)
                                                                                       .UnionWith(this.Annotations.Definition)
                                                                                       .UnionWith(this.Annotations.Example);

            return result;
        }
        #endregion

        #endregion

    }

}