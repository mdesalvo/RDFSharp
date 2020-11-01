/*
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
using RDFSharp.Semantics.OWL;
using System;
using System.Collections.Generic;

namespace RDFSharp.Semantics.SKOS
{

    /// <summary>
    /// RDFSKOSCollection represents an instance of skos:Collection within an instance of skos:ConceptScheme
    /// </summary>
    public class RDFSKOSCollection : RDFOntologyFact
    {

        #region Properties
        /// <summary>
        /// Count of the concepts of the collection
        /// </summary>
        public Int64 ConceptsCount
        {
            get { return this.Concepts.Count; }
        }

        /// <summary>
        /// Count of the collections of the collection
        /// </summary>
        public Int64 CollectionsCount
        {
            get { return this.Collections.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the concepts of the collection
        /// </summary>
        public IEnumerator<RDFSKOSConcept> ConceptsEnumerator
        {
            get { return this.Concepts.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Gets the enumerator on the collections of the collection
        /// </summary>
        public IEnumerator<RDFSKOSCollection> CollectionsEnumerator
        {
            get { return this.Collections.Values.GetEnumerator(); }
        }

        /// <summary>
        /// Dictionary of concepts contained in the collection
        /// </summary>
        internal Dictionary<Int64, RDFSKOSConcept> Concepts { get; set; }

        /// <summary>
        /// Dictionary of collections contained in the collection
        /// </summary>
        internal Dictionary<Int64, RDFSKOSCollection> Collections { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a skos:Collection with the given name
        /// </summary>
        public RDFSKOSCollection(RDFResource collectionName) : base(collectionName)
        {
            this.Concepts = new Dictionary<Int64, RDFSKOSConcept>();
            this.Collections = new Dictionary<Int64, RDFSKOSCollection>();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given concept to the collection
        /// </summary>
        public RDFSKOSCollection AddConcept(RDFSKOSConcept concept)
        {
            if (concept != null)
            {
                if (!this.Concepts.ContainsKey(concept.PatternMemberID))
                    this.Concepts.Add(concept.PatternMemberID, concept);
            }
            return this;
        }

        /// <summary>
        /// Adds the given collection to the collection
        /// </summary>
        public RDFSKOSCollection AddCollection(RDFSKOSCollection collection)
        {
            if (collection != null)
            {
                if (!this.Collections.ContainsKey(collection.PatternMemberID))
                    this.Collections.Add(collection.PatternMemberID, collection);
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given concept from the collection
        /// </summary>
        public RDFSKOSCollection RemoveConcept(RDFSKOSConcept concept)
        {
            if (concept != null)
            {
                if (this.Concepts.ContainsKey(concept.PatternMemberID))
                    this.Concepts.Remove(concept.PatternMemberID);
            }
            return this;
        }

        /// <summary>
        /// Removes the given collection from the collection
        /// </summary>
        public RDFSKOSCollection RemoveCollection(RDFSKOSCollection collection)
        {
            if (collection != null)
            {
                if (this.Collections.ContainsKey(collection.PatternMemberID))
                    this.Collections.Remove(collection.PatternMemberID);
            }
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the concept represented by the given string from the scheme
        /// </summary>
        public RDFSKOSConcept SelectConcept(String concept)
        {
            if (concept != null)
            {
                Int64 conceptID = RDFModelUtilities.CreateHash(concept);
                if (this.Concepts.ContainsKey(conceptID))
                {
                    return this.Concepts[conceptID];
                }
            }
            return null;
        }

        /// <summary>
        /// Selects the collection represented by the given string from the scheme
        /// </summary>
        public RDFSKOSCollection SelectCollection(String collection)
        {
            if (collection != null)
            {
                Int64 collectionID = RDFModelUtilities.CreateHash(collection);
                if (this.Collections.ContainsKey(collectionID))
                {
                    return this.Collections[collectionID];
                }
            }
            return null;
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets the complete list of concepts contained in the collection
        /// </summary>
        public List<RDFSKOSConcept> GetMembers()
        {
            var result = new List<RDFSKOSConcept>();

            //Concepts
            foreach (var concept in this.Concepts.Values)
            {
                result.Add(concept);
            }

            //Collections
            foreach (var collection in this.Collections.Values)
            {
                result.AddRange(collection.GetMembers());
            }

            return result;
        }
        #endregion

        #region Convert
        /// <summary>
        /// Gets a graph representation of this collection, exporting inferences according to the selected behavior
        /// </summary>
        public RDFGraph ToRDFGraph(RDFSemanticsEnums.RDFOntologyInferenceExportBehavior infexpBehavior)
        {
            return this.ToRDFOntologyData().ToRDFGraph(infexpBehavior);
        }

        /// <summary>
        /// Gets an ontology data representation of this collection
        /// </summary>
        public RDFOntologyData ToRDFOntologyData()
        {
            var result = new RDFOntologyData();

            //Collection
            result.AddFact(this);
            result.AddClassTypeRelation(this, RDFVocabulary.SKOS.COLLECTION.ToRDFOntologyClass());

            //Concepts
            foreach (var cn in this.Concepts.Values)
            {
                result.AddFact(cn);
                result.AddClassTypeRelation(cn, RDFVocabulary.SKOS.CONCEPT.ToRDFOntologyClass());
                result.AddAssertionRelation(this, RDFVocabulary.SKOS.MEMBER.ToRDFOntologyObjectProperty(), cn);
            }

            //Collections
            foreach (var cl in this.Collections.Values)
            {
                result.AddAssertionRelation(this, RDFVocabulary.SKOS.MEMBER.ToRDFOntologyObjectProperty(), cl);
                result = result.UnionWith(cl.ToRDFOntologyData());
            }

            return result;
        }
        #endregion

        #endregion

    }

}