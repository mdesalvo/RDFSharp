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
using System.Linq;

namespace RDFSharp.Semantics.SKOS
{

    /// <summary>
    /// RDFSKOSOrderedCollection represents an instance of skos:OrderedCollection within an instance of skos:ConceptScheme
    /// </summary>
    public class RDFSKOSOrderedCollection : RDFOntologyFact
    {

        #region Properties
        /// <summary>
        /// Count of the concepts of the collection
        /// </summary>
        public long ConceptsCount
        {
            get { return this.Concepts.Count; }
        }

        /// <summary>
        /// Gets the ordered enumerator on the concepts of the collection
        /// </summary>
        public IEnumerator<Tuple<int, RDFSKOSConcept>> ConceptsEnumerator
        {
            get { return this.Concepts.Values.OrderBy(x => x.Item1).GetEnumerator(); }
        }

        /// <summary>
        /// Internal sequential counter of the concepts of the collection
        /// </summary>
        internal int ConceptsSequentialCounter { get; set; }

        /// <summary>
        /// Dictionary of concepts contained in the collection
        /// </summary>
        internal Dictionary<long, Tuple<int, RDFSKOSConcept>> Concepts { get; set; }

        /// <summary>
        /// Reification representative of the collection
        /// </summary>
        internal RDFOntologyFact Representative { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a skos:OrderedCollection with the given name
        /// </summary>
        public RDFSKOSOrderedCollection(RDFResource collectionName) : base(collectionName)
        {
            this.ConceptsSequentialCounter = 0;
            this.Concepts = new Dictionary<long, Tuple<int, RDFSKOSConcept>>();
            this.Representative = new RDFOntologyFact(new RDFResource("bnode:" + this.PatternMemberID));
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given concept to the collection
        /// </summary>
        public RDFSKOSOrderedCollection AddConcept(RDFSKOSConcept concept)
        {
            if (concept != null)
            {
                if (!this.Concepts.ContainsKey(concept.PatternMemberID))
                    this.Concepts.Add(concept.PatternMemberID, new Tuple<int, RDFSKOSConcept>(this.ConceptsSequentialCounter++, concept));
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given concept from the collection
        /// </summary>
        public RDFSKOSOrderedCollection RemoveConcept(RDFSKOSConcept concept)
        {
            if (concept != null)
            {
                if (this.Concepts.ContainsKey(concept.PatternMemberID))
                    this.Concepts.Remove(concept.PatternMemberID);
            }
            return this;
        }
        #endregion

        #region Select
        /// <summary>
        /// Selects the concept represented by the given string from the scheme
        /// </summary>
        public RDFSKOSConcept SelectConcept(string concept)
        {
            if (concept != null)
            {
                long conceptID = RDFModelUtilities.CreateHash(concept);
                if (this.Concepts.ContainsKey(conceptID))
                {
                    return this.Concepts[conceptID].Item2;
                }
            }
            return null;
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets the ordered list of concepts contained in the collection
        /// </summary>
        public List<RDFSKOSConcept> GetMembers()
        {
            var result = new List<RDFSKOSConcept>();

            //Concepts
            var conceptsEnum = this.ConceptsEnumerator;
            while (conceptsEnum.MoveNext())
            {
                result.Add(conceptsEnum.Current.Item2);
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

            //OrderedCollection
            result.AddFact(this);
            result.AddClassTypeRelation(this, RDFVocabulary.SKOS.ORDERED_COLLECTION.ToRDFOntologyClass());
            result.AddAssertionRelation(this, RDFVocabulary.SKOS.MEMBER_LIST.ToRDFOntologyObjectProperty(), this.Representative);

            //Representative
            result.AddFact(this.Representative);

            //Concepts
            var reifSubj = new RDFOntologyFact((RDFResource)this.Representative.Value);
            if (this.ConceptsCount == 0)
            {
                result.Relations.ClassType.AddEntry(new RDFOntologyTaxonomyEntry(reifSubj, RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), RDFVocabulary.RDF.LIST.ToRDFOntologyClass()));
                result.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(reifSubj, RDFVocabulary.RDF.FIRST.ToRDFOntologyObjectProperty(), RDFVocabulary.RDF.NIL.ToRDFOntologyFact()));
                result.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(reifSubj, RDFVocabulary.RDF.REST.ToRDFOntologyObjectProperty(), RDFVocabulary.RDF.NIL.ToRDFOntologyFact()));
            }
            else
            {
                var itemCounter = 0;
                var conceptsEnum = this.ConceptsEnumerator;
                while (conceptsEnum.MoveNext())
                {
                    result.AddFact(conceptsEnum.Current.Item2);
                    result.AddClassTypeRelation(conceptsEnum.Current.Item2, RDFVocabulary.SKOS.CONCEPT.ToRDFOntologyClass());

                    //first
                    result.Relations.ClassType.AddEntry(new RDFOntologyTaxonomyEntry(reifSubj, RDFVocabulary.RDF.TYPE.ToRDFOntologyObjectProperty(), RDFVocabulary.RDF.LIST.ToRDFOntologyClass()));
                    result.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(reifSubj, RDFVocabulary.RDF.FIRST.ToRDFOntologyObjectProperty(), conceptsEnum.Current.Item2));

                    //rest
                    itemCounter++;
                    if (itemCounter < this.ConceptsCount)
                    {
                        var newReifSubj = new RDFOntologyFact(new RDFResource());
                        result.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(reifSubj, RDFVocabulary.RDF.REST.ToRDFOntologyObjectProperty(), newReifSubj));
                        reifSubj = newReifSubj;
                    }
                    else
                    {
                        result.Relations.Assertions.AddEntry(new RDFOntologyTaxonomyEntry(reifSubj, RDFVocabulary.RDF.REST.ToRDFOntologyObjectProperty(), RDFVocabulary.RDF.NIL.ToRDFOntologyFact()));
                    }
                }
            }

            return result;
        }
        #endregion

        #endregion

    }

}