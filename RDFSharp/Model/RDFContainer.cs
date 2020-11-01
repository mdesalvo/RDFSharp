/*
   Copyright 2012-2020 Marco De Salvo

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
using System;
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
        public Int32 ItemsCount
        {
            get { return this.Items.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the container's items for iteration
        /// </summary>
        public IEnumerator<RDFPatternMember> ItemsEnumerator
        {
            get { return this.Items.GetEnumerator(); }
        }

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
            this.ContainerType = containerType;
            this.ItemType = itemType;
            this.ReificationSubject = new RDFResource();
            this.Items = new List<RDFPatternMember>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes a typed enumerator on the container's items
        /// </summary>
        IEnumerator<RDFPatternMember> IEnumerable<RDFPatternMember>.GetEnumerator()
        {
            return this.ItemsEnumerator;
        }

        /// <summary>
        /// Exposes an untyped enumerator on the container's items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.ItemsEnumerator;
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the given item to the container
        /// </summary>
        public RDFContainer AddItem(RDFResource item)
        {
            if (item != null && this.ItemType == RDFModelEnums.RDFItemTypes.Resource)
            {
                switch (this.ContainerType)
                {
                    case RDFModelEnums.RDFContainerTypes.Alt:
                        //Avoid duplicates in case of "rdf:Alt" container
                        if (this.Items.Find(x => x.Equals(item)) == null)
                            this.Items.Add(item);
                        break;
                    case RDFModelEnums.RDFContainerTypes.Bag:
                        this.Items.Add(item);
                        break;
                    case RDFModelEnums.RDFContainerTypes.Seq:
                        this.Items.Add(item);
                        break;
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given item to the container
        /// </summary>
        public RDFContainer AddItem(RDFLiteral item)
        {
            if (item != null && this.ItemType == RDFModelEnums.RDFItemTypes.Literal)
            {
                switch (this.ContainerType)
                {
                    case RDFModelEnums.RDFContainerTypes.Alt:
                        //Avoid duplicates in case of "rdf:Alt" container
                        if (this.Items.Find(x => x.Equals(item)) == null)
                            this.Items.Add(item);
                        break;
                    case RDFModelEnums.RDFContainerTypes.Bag:
                        this.Items.Add(item);
                        break;
                    case RDFModelEnums.RDFContainerTypes.Seq:
                        this.Items.Add(item);
                        break;
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the given item from the container
        /// </summary>
        public RDFContainer RemoveItem(RDFResource item)
        {
            if (item != null && this.ItemType == RDFModelEnums.RDFItemTypes.Resource)
            {
                this.Items.RemoveAll(x => x.Equals(item));
            }
            return this;
        }

        /// <summary>
        /// Removes the given item from the container
        /// </summary>
        public RDFContainer RemoveItem(RDFLiteral item)
        {
            if (item != null && this.ItemType == RDFModelEnums.RDFItemTypes.Literal)
            {
                this.Items.RemoveAll(x => x.Equals(item));
            }
            return this;
        }

        /// <summary>
        /// Removes all the items from the container
        /// </summary>
        public void ClearItems()
        {
            this.Items.Clear();
        }
        #endregion

        #region Reify
        /// <summary>
        /// Builds the reification graph of the container:
        /// Subject -> rdf:type -> [rdf:Bag|rdf:Seq|rdf:Alt]
        /// Subject -> rdf:_N   -> RDFContainer.ITEM(N)
        /// </summary>
        public RDFGraph ReifyContainer()
        {
            RDFGraph reifCont = new RDFGraph();

            //  Subject -> rdf:type -> [rdf:Bag|rdf:Seq|rdf:Alt]
            switch (this.ContainerType)
            {
                case RDFModelEnums.RDFContainerTypes.Bag:
                    reifCont.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.BAG));
                    break;
                case RDFModelEnums.RDFContainerTypes.Seq:
                    reifCont.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.SEQ));
                    break;
                case RDFModelEnums.RDFContainerTypes.Alt:
                    reifCont.AddTriple(new RDFTriple(this.ReificationSubject, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT));
                    break;
            }

            //  Subject -> rdf:_N -> RDFContainer.ITEM(N)
            Int32 index = 0;
            foreach (Object item in this)
            {
                RDFResource ordPred = new RDFResource(RDFVocabulary.RDF.BASE_URI + "_" + (++index));
                if (this.ItemType == RDFModelEnums.RDFItemTypes.Resource)
                {
                    reifCont.AddTriple(new RDFTriple(this.ReificationSubject, ordPred, (RDFResource)item));
                }
                else
                {
                    reifCont.AddTriple(new RDFTriple(this.ReificationSubject, ordPred, (RDFLiteral)item));
                }
            }

            return reifCont;
        }
        #endregion

        #endregion

    }

}