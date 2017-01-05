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
using RDFSharp.Store;

namespace RDFSharp.Model 
{

    /// <summary>
    /// RDFDataSource is the foundation class for modeling RDF data sources
    /// </summary>
    public abstract class RDFDataSource {

        #region Methods
        /// <summary>
        /// Checks if this data source is a graph
        /// </summary>
        internal Boolean IsGraph() {
            return this is RDFGraph;
        }

        /// <summary>
        /// Checks if this data source is a store
        /// </summary>
        internal Boolean IsStore() {
            return this is RDFStore;
        }

        /// <summary>
        /// Checks if this data source is a federation
        /// </summary>
        internal Boolean IsFederation() {
            return this is RDFFederation;
        }
        #endregion

    }

}