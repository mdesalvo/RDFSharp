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

using RDFSharp.Query;
using System.Collections;
using System.Collections.Generic;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFContainer represents a generic container in the RDF model.
    /// </summary>
    public sealed class RDFContainer : IEnumerable<RDFPatternMember>
    {
        #region Properties
        /// <summary>
        /// Type of the container
        /// </summary>
        public RDFModelEnums.RDFContainerTypes ContainerType { get; internal set; }

        /// <summary>
        /// Type of the items of the container
        /// </summary>
        public RDFModelEnums.RDFItemTypes ItemType { get; internal set; }

        /// <summary>
        /// Subject of the container's reification
        /// </summary>
        public RDFResource ReificationSubject { get; internal set; }

        /// <summary>
        /// Count of the container's items
        /// </summary>
        public int ItemsCount
            => Items.Count;

        /// <summary>
        /// Gets the enumerator on the container's items for iteration
        /// </summary>
        public IEnumerator<RDFPatternMember> ItemsEnumerator
            => Items.GetEnumerator();

        /// <summary>
        /// List of the items contained in the container
        /// </summary>
        internal List<RDFPatternMember> Items { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty container of the given flavor and given type
        /// </summary>
        public RDFContainer(RDFModelEnums.RDFContainerTypes containerType, RDFModelEnums.RDFItemTypes itemType)
        {
            ContainerType = containerType;
            ItemType = itemType;
            ReificationSubject = new RDFResource();
            Items = new List<RDFPatternMember>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the container's items
        /// </summary>
        IEnumerator<RDFPatternMember> IEnumerable<RDFPatternMember>.GetEnumerator()
            => ItemsEnumerator;

        /// <summary>
        /// Exposes an untyped enumerator on the container's items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
            => ItemsEnumerator;
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given item to the container
        /// </summary>
        public RDFContainer AddItem(RDFResource item)
        {
            if (item != null && ItemType == RDFModelEnums.RDFItemTypes.Resource)
                AddItemInternal(item);
            return this;
        }

        /// <summary>
        /// Adds the given item to the container
        /// </summary>
        public RDFContainer AddItem(RDFLiteral item)
        {
            if (item != null && ItemType == RDFModelEnums.RDFItemTypes.Literal)
                AddItemInternal(item);
            return this;
        }

        /// <summary>
        /// Adds the given item to the container
        /// </summary>
        internal void AddItemInternal(RDFPatternMember item)
        {
            switch (ContainerType)
            {
                case RDFModelEnums.RDFContainerTypes.Alt:
                    //Avoid duplicates in case of "rdf:Alt" container
                    if (Items.Find(x => x.Equals(item)) == null)
                        Items.Add(item);
                    break;
                case RDFModelEnums.RDFContainerTypes.Bag:
                case RDFModelEnums.RDFContainerTypes.Seq:
                    Items.Add(item);
                    break;
            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given item from the container
        /// </summary>
        public RDFContainer RemoveItem(RDFResource item)
        {
            if (item != null && ItemType == RDFModelEnums.RDFItemTypes.Resource)
                Items.RemoveAll(x => x.Equals(item));
            return this;
        }

        /// <summary>
        /// Removes the given item from the container
        /// </summary>
        public RDFContainer RemoveItem(RDFLiteral item)
        {
            if (item != null && ItemType == RDFModelEnums.RDFItemTypes.Literal)
                Items.RemoveAll(x => x.Equals(item));
            return this;
        }

        /// <summary>
        /// Removes all the items from the container
        /// </summary>
        public void ClearItems()
            => Items.Clear();
        #endregion

        #region Reify
        /// <summary>
        /// Builds the reification graph of the container:<br/>
        /// Subject -> rdf:type -> [rdf:Bag|rdf:Seq|rdf:Alt]<br/>
        /// Subject -> rdf:_N   -> item(N)
        /// </summary>
        public RDFGraph ReifyContainer()
        {
            RDFGraph reifCont = new RDFGraph();

            //  Subject -> rdf:type -> [rdf:Bag|rdf:Seq|rdf:Alt]
            switch (ContainerType)
            {
                case RDFModelEnums.RDFContainerTypes.Bag:
                    reifCont.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.BAG));
                    break;
                case RDFModelEnums.RDFContainerTypes.Seq:
                    reifCont.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.SEQ));
                    break;
                case RDFModelEnums.RDFContainerTypes.Alt:
                    reifCont.AddTriple(new RDFTriple(ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT));
                    break;
            }

            //  Subject -> rdf:_N -> RDFContainer.ITEM(N)
            int index = 0;
            foreach (RDFPatternMember item in this)
            {
                RDFResource ordPred = new RDFResource(string.Concat(RDFVocabulary.RDF.BASE_URI, "_", (++index).ToString()));
                if (ItemType == RDFModelEnums.RDFItemTypes.Resource)
                    reifCont.AddTriple(new RDFTriple(ReificationSubject, ordPred, (RDFResource)item));
                else
                    reifCont.AddTriple(new RDFTriple(ReificationSubject, ordPred, (RDFLiteral)item));
            }

            return reifCont;
        }
        #endregion

        #endregion
    }
}