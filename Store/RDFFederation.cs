/*
   Copyright 2012-2016 Marco De Salvo

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

namespace RDFSharp.Store
{

    /// <summary>
    /// RDFFederation represents a virtual store giving logically integrated access to multiple stores.
    /// </summary>
    public class RDFFederation: IEquatable<RDFFederation>, IEnumerable<RDFStore>   {

        #region Properties
        /// <summary>
        /// Name of the federation
        /// </summary>
        public String FederationName { get; internal set; }

        /// <summary>
        /// Count of the federation' stores
        /// </summary>
        public Int32 StoresCount {
            get { return this.Stores.Count; }
        }

        /// <summary>
        /// Gets the enumerator on the federation' stores for iteration
        /// </summary>
        public IEnumerator<RDFStore> StoresEnumerator {
            get { return this.Stores.Values.GetEnumerator(); }
        }

        /// <summary>
        /// List of stores embedded into the federation
        /// </summary>
        internal Dictionary<Int64, RDFStore> Stores { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build an empty federation
        /// </summary>
        public RDFFederation() {
            this.FederationName = "FEDERATION|ID=" + Guid.NewGuid();
            this.Stores         = new Dictionary<Int64, RDFStore>();
        }

        /// <summary>
        /// List-based ctor to build a federation with the given list of stores
        /// </summary>
        public RDFFederation(List<RDFStore> stores): this() {
            if (stores != null) {
                stores.ForEach(s => this.AddStore(s));
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the federation
        /// </summary>
        public override String ToString() {
            return this.FederationName;
        }

        /// <summary>
        /// Performs the equality comparison between two federations
        /// </summary>
        public Boolean Equals(RDFFederation other) {
            if (other == null || this.StoresCount != other.StoresCount) {
                return false;
            }
            foreach(RDFStore store in this) {
                if (!other.Stores.ContainsKey(store.StoreID)) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Exposes a typed enumerator on the federation'stores
        /// </summary>
        IEnumerator<RDFStore> IEnumerable<RDFStore>.GetEnumerator() {
            return this.Stores.Values.GetEnumerator();
        }

        /// <summary>
        /// Exposes an untyped enumerator on the federation'stores
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.Stores.Values.GetEnumerator();
        }
        #endregion

        #region Methods

        #region Add
        /// <summary>
        /// Adds the store to the federation, avoiding duplicate insertions
        /// </summary>
        public RDFFederation AddStore(RDFStore store) {
            if (store != null) {
                if (!this.Stores.ContainsKey(store.StoreID)) {
                    this.Stores.Add(store.StoreID, store);
                }
            }
            return this;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the store from the federation 
        /// </summary>
        public RDFFederation RemoveStore(RDFStore store) {
            if (store != null) {
                if (this.Stores.ContainsKey(store.StoreID)) {
                    this.Stores.Remove(store.StoreID);
                }
            }
            return this;
        }
        
        /// <summary>
        /// Clears the stores of the federation.
        /// </summary>
        public RDFFederation ClearStores() {
            this.Stores.Clear();
            return this;
        }
        #endregion

        #endregion

    }

}