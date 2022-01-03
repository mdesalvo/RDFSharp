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

using System;
using System.Data;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOperationResult is a container for SPARQL UPDATE operation results
    /// </summary>
    public class RDFOperationResult
    {
        #region Properties
        /// <summary>
        /// Tabular response of the SPARQL DELETE operation
        /// </summary>
        public DataTable DeleteResults { get; internal set; }

        /// <summary>
        /// Tabular response of the SPARQL INSERT operation
        /// </summary>
        public DataTable InsertResults { get; internal set; }

        /// <summary>
        /// Gets the number of results produced by the SPARQL DELETE operation
        /// </summary>
        public long DeleteResultsCount
            => this.DeleteResults.Rows.Count;

        /// <summary>
        /// Gets the number of results produced by the SPARQL INSERT operation
        /// </summary>
        public long InsertResultsCount
            => this.InsertResults.Rows.Count;
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty operation result
        /// </summary>
        internal RDFOperationResult()
        {
            //Initialize DELETE templates
            this.DeleteResults = new DataTable("DELETE_RESULTS");

            //Initialize INSERT templates
            this.InsertResults = new DataTable("INSERT_RESULTS");
        }
        #endregion
    }
}