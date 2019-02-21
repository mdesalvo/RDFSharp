/*
   Copyright 2012-2019 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RDFSharp.Query {

    /// <summary>
    /// RDFQuery is the foundation class for modeling SPARQL queries
    /// </summary>
    public abstract class RDFQuery {

        #region Properties
        /// <summary>
        /// List of members carried by the query
        /// </summary>
        internal List<RDFQueryMember> QueryMembers { get; set; }

        /// <summary>
        /// Dictionary of pattern result tables
        /// </summary>
        internal Dictionary<Int64, List<DataTable>> PatternResultTables { get; set; }

        /// <summary>
        /// Dictionary of result tables produced by evaluation of query members
        /// </summary>
        internal Dictionary<Int64, DataTable> QueryMemberResultTables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty query
        /// </summary>
        internal RDFQuery() {
            this.QueryMembers            = new List<RDFQueryMember>();
            this.PatternResultTables     = new Dictionary<Int64, List<DataTable>>();
            this.QueryMemberResultTables = new Dictionary<Int64, DataTable>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the query members of type: pattern group
        /// </summary>
        internal IEnumerable<RDFPatternGroup> GetPatternGroups() {
            return this.QueryMembers.Where(q => q is RDFPatternGroup)
                                    .OfType<RDFPatternGroup>();
        }

        /// <summary>
        /// Gets the query members of type: modifier
        /// </summary>
        internal IEnumerable<RDFModifier> GetModifiers() {
            return this.QueryMembers.Where(q => q is RDFModifier)
                                    .OfType<RDFModifier>();
        }

        /// <summary>
        /// Gets the query members which can be evaluated
        /// </summary>
        internal IEnumerable<RDFQueryMember> GetEvaluableMembers() {
            return this.QueryMembers.Where(q => q.IsEvaluable);
        }
        #endregion

    }

}