/*
   Copyright 2012-2024 Marco De Salvo

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

using RDFSharp.Query;
using System.Collections;
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFCollection represents a generic collection in the RDF model.
    /// </summary>
    public sealed class RDFCollection : IEnumerable<RDFPatternMember>
    {
        #region Properties
        /// <summary>
        /// Type of the items of the collection
        /// </summary>
        public RDFModelEnums.RDFItemTypes ItemType { get; internal set; }

        /// <summary>
        /// Subject of the collection's reification (rdf:nil when the collection is empty)
        /// </summary>
        public RDFResource ReificationSubject { get; internal set; }

        /// <summary>
        /// Internal subject of the collection's reification
        /// </summary>
        internal RDFResource InternalReificationSubject { get; set; }

        /// <summary>
        /// Count of the collection's items
        /// </summary>
        public int ItemsCount
            => Items.Count;

        /// <summary>
        /// Gets the enumerator on the collection's items for iteration
        /// </summary>
        public IEnumerator<RDFPatternMember> ItemsEnumerator
            => Items.GetEnumerator();

        /// <summary>
        /// Flag indicating that this collection exceptionally accepts duplicates
        /// </summary>
        internal bool AcceptDuplicates { get; set; }

        /// <summary>
        /// List of the items collected by the collection
        /// </summary>
        internal List<RDFPatternMember> Items { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty collection of the given type
        /// (initial configuration of the collection is "rdf:Nil")
        /// </summary>
        public RDFCollection(RDFModelEnums.RDFItemTypes itemType) : this(itemType, false) { }
        internal RDFCollection(RDFModelEnums.RDFItemTypes itemType, bool acceptDuplicates)
        {
            ItemType = itemType;
            ReificationSubject = RDFVocabulary.RDF.NIL;
            InternalReificationSubject = new RDFResource();
            AcceptDuplicates = acceptDuplicates;
            Items = new List<RDFPatternMember>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the collection's items
        /// </summary>
        IEnumerator<RDFPatternMember> IEnumerable<RDFPatternMember>.GetEnumerator()
            => ItemsEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the collection's items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => ItemsEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given item to the collection
        /// </summary>
        public RDFCollection AddItem(RDFResource item)
        {
            if (item != null && ItemType == RDFModelEnums.RDFItemTypes.Resource)
                AddItemInternal(item);
            return this;
        }

        /// <summary>
        /// Adds the given item to the collection
        /// </summary>
        public RDFCollection AddItem(RDFLiteral item)
        {
            if (item != null && ItemType == RDFModelEnums.RDFItemTypes.Literal)
                AddItemInternal(item);
            return this;
        }

        /// <summary>
        /// Adds the given item to the collection
        /// </summary>
        internal void AddItemInternal(RDFPatternMember item)
        {
            if (AcceptDuplicates || Items.Find(x => x.Equals(item)) == null)
            {
                //Add item to collection
                Items.Add(item);
                //Update ReificationSubject (if collection has left "rdf:nil" configuration)
                if (ItemsCount == 1)
                    ReificationSubject = InternalReificationSubject;
            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given item from the collection
        /// </summary>
        public RDFCollection RemoveItem(RDFResource item)
        {
            if (item != null && ItemType == RDFModelEnums.RDFItemTypes.Resource)
                RemoveItemInternal(item);
            return this;
        }

        /// <summary>
        /// Removes the given item from the collection
        /// </summary>
        public RDFCollection RemoveItem(RDFLiteral item)
        {
            if (item != null && ItemType == RDFModelEnums.RDFItemTypes.Literal)
                RemoveItemInternal(item);
            return this;
        }

        /// <summary>
        /// Removes the given item from the collection
        /// </summary>
        internal void RemoveItemInternal(RDFPatternMember item)
        {
            //Remove item from collection
            Items.RemoveAll(x => x.Equals(item));
            //Update ReificationSubject (if collection has turned back into "rdf:Nil" configuration)
            if (ItemsCount == 0)
                ReificationSubject = RDFVocabulary.RDF.NIL;
        }

        /// <summary>
        /// Removes all the items from the collection
        /// </summary>
        public void ClearItems()
        {
            //Clear items of collection
            Items.Clear();
            //Turn back the collection into "rdf:Nil" configuration
            ReificationSubject = RDFVocabulary.RDF.NIL;
        }
        #endregion

        #region Reify
        /// <summary>
        /// Builds the reification graph of the collection
        /// </summary>
        public RDFGraph ReifyCollection()
        {
            RDFGraph reifColl = new RDFGraph();
            RDFResource reifSubj = ReificationSubject;
            int itemCount = 0;

            //Collection can be reified only if it has at least one item
            foreach (RDFPatternMember collectionItem in this)
            {
                //Count the items to keep track of the last one, which will be connected to rdf:nil
                itemCount++;

                //  Subject -> rdf:type  -> rdf:List
                reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));

                //  Subject -> rdf:first -> RDFCollection.ITEM[i]
                if (ItemType == RDFModelEnums.RDFItemTypes.Resource)
                    reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.FIRST, (RDFResource)collectionItem));
                else
                    reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.FIRST, (RDFLiteral)collectionItem));

                //Not the last one: Subject -> rdf:rest  -> NEWBLANK
                if (itemCount < ItemsCount)
                {
                    RDFResource newSub = new RDFResource();
                    reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.REST, newSub));
                    reifSubj = newSub;
                }
                //The last one:     Subject -> rdf:rest  -> rdf:nil
                else
                {
                    reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.REST, RDFVocabulary.RDF.NIL));
                }
            }

            return reifColl;
        }
        #endregion

        #endregion
    }
}