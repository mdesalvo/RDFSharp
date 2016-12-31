/*
   Copyright 2012-2017 Marco De Salvo

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

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFCollection represents a generic collection in the RDF model.
    /// It is made up of items, which must be all resources or all literals.
    /// </summary>
    public class RDFCollection: IEnumerable {

        #region Properties
        /// <summary>
        /// Type of the items of the collection
        /// </summary>
        public RDFModelEnums.RDFItemTypes ItemType { get; internal set; }

        /// <summary>
        /// Subject of the collection's reification
        /// </summary>
        public RDFResource ReificationSubject { get; internal set; }

        /// <summary>
        /// Count of the collection's items
        /// </summary>
        public Int32 ItemsCount {
            get { return this.Items.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the collection's items for iteration
        /// </summary>
        public IEnumerator ItemsEnumerator {
            get { return this.Items.GetEnumerator(); }
        }

        /// <summary>
        /// List of the items collected by the collection
        /// </summary>
        internal ArrayList Items { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty collection of the given type
        /// </summary>
        public RDFCollection(RDFModelEnums.RDFItemTypes itemType) {
           this.ItemType           = itemType;
           this.ReificationSubject = new RDFResource();
           this.Items              = new ArrayList();        
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Exposes an untyped enumerator on the collection's items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Items.GetEnumerator();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given item to the collection, avoiding duplicate insertions
        /// </summary>
        public RDFCollection AddItem(Object item) {
            if (item != null) {

                //Try to add a resource
                if (item is RDFResource && this.ItemType == RDFModelEnums.RDFItemTypes.Resource) {
                    Boolean itemFound    = false;
                    foreach(var itemEnum in this) {
                        if(((RDFResource)itemEnum).Equals((RDFResource)item)){
                            itemFound    = true;
                            break;
                        }    
                    }
                    if (!itemFound) {
                         this.Items.Add(item);
                    }
                }

                //Try to add a literal
                else if (item is RDFLiteral && this.ItemType == RDFModelEnums.RDFItemTypes.Literal) {
                    Boolean itemFound    = false;
                    foreach(var itemEnum in this) {
                        if (((RDFLiteral)itemEnum).Equals((RDFLiteral)item)) {
                            itemFound    = true;
                            break;
                        }
                    }
                    if (!itemFound) {
                         this.Items.Add(item);
                    }
                }

            }
            return this;
        }

        /// <summary>
        /// Removes the given item from the collection
        /// </summary>
        public RDFCollection RemoveItem(Object item) {
            if (item != null) {

                //Try to remove a resource
                if (item is RDFResource && this.ItemType == RDFModelEnums.RDFItemTypes.Resource) {
                    ArrayList resultList = new ArrayList();
                    foreach(var itemEnum in this) {
                        if (!((RDFResource)itemEnum).Equals((RDFResource)item)) {
                            resultList.Add(itemEnum);
                        }
                    }
                    this.Items = resultList;
                }

                //Try to remove a literal
                else if (item is RDFLiteral && this.ItemType == RDFModelEnums.RDFItemTypes.Literal) {
                    ArrayList resultList = new ArrayList();
                    foreach(var itemEnum in this) {
                        if (!((RDFLiteral)itemEnum).Equals((RDFLiteral)item)) {
                            resultList.Add(itemEnum);
                        }
                    }
                    this.Items = resultList;
                }

            }
            return this;
        }

        /// <summary>
        /// Removes all the items from the collection
        /// </summary>
        public void ClearItems() {
            this.Items.Clear();
        }

        /// <summary>
        /// Builds the reification graph of the collection
        /// </summary>
        public RDFGraph ReifyCollection() {
            RDFGraph reifColl          = new RDFGraph();
            RDFResource reifSubj       = this.ReificationSubject;
            Int32 itemCount            = 0;

            //Manage the empty collection
            if (this.ItemsCount       == 0) {

                //  Subject -> rdf:type  -> rdf:List
                reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.TYPE,  RDFVocabulary.RDF.LIST));

                // Subject  -> rdf:first -> rdf:nil
                reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.FIRST, RDFVocabulary.RDF.NIL));

                // Subject  -> rdf:rest  -> rdf:nil
                reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.REST,  RDFVocabulary.RDF.NIL));

            }

            //Manage the non-empty collection
            else {

                foreach (Object listEnum in this) {

                    //Count the items to keep track of the last one, which will be connected to rdf:nil
                    itemCount++;

                    //  Subject -> rdf:type  -> rdf:List
                    reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));

                    //  Subject -> rdf:first -> RDFCollection.ITEM[i]
                    if (this.ItemType     == RDFModelEnums.RDFItemTypes.Resource) {
                        reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.FIRST, (RDFResource)listEnum));
                    }
                    else {
                        reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.FIRST, (RDFLiteral)listEnum));
                    }

                    //Not the last one: Subject -> rdf:rest  -> NEWBLANK
                    if (itemCount          < this.ItemsCount) {
                        RDFResource newSub = new RDFResource();
                        reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.REST, newSub));
                        reifSubj           = newSub;
                    }
                    //The last one:     Subject -> rdf:rest  -> rdf:nil
                    else {
                        reifColl.AddTriple(new RDFTriple(reifSubj, RDFVocabulary.RDF.REST, RDFVocabulary.RDF.NIL));
                    }

                }

            }

            return reifColl;
        }
        #endregion

    }

    /// <summary>
    /// RDFCollectionItem represents an item of a collection
    /// </summary>
    internal class RDFCollectionItem {

        #region Properties
        /// <summary>
        /// Type of the collection item
        /// </summary>
        internal RDFModelEnums.RDFItemTypes ItemType { get; set; }

        /// <summary>
        /// Value of the collection item
        /// </summary>
        internal Object ItemValue { get; set; }

        /// <summary>
        /// Pointer to the next item of the collection
        /// </summary>
        internal Object ItemNext { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a RDFCollectionItem with the given parameters
        /// </summary>
        internal RDFCollectionItem(RDFModelEnums.RDFItemTypes itemType, Object itemValue, Object itemNext) {
            this.ItemType  = itemType;
            this.ItemValue = itemValue;
            this.ItemNext  = itemNext;
        }
        #endregion

    }

}