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

using RDFSharp.Query;
using System.Data;

namespace RDFSharp.Semantics.SWRL
{
    /// <summary>
    /// RDFSWRLAtomResult is a container for SWRL atom entailments
    /// </summary>
    public class RDFSWRLAtomResult
    {
        #region Properties
        /// <summary>
        /// Tabular response of the atom
        /// </summary>
        public DataTable Results { get; internal set; }

        /// <summary>
        /// Gets the number of results produced by the atom
        /// </summary>
        public long ResultsCount
            => this.Results.Rows.Count;
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty SWRL atom result
        /// </summary>
        internal RDFSWRLAtomResult(RDFSWRLAtom atom)
            => this.Results = new DataTable(atom.ToString());
        #endregion
    }
}