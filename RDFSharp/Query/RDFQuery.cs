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
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Query {

    /// <summary>
    /// RDFQuery is the foundation class for modeling SPARQL queries
    /// </summary>
    public abstract class RDFQuery {

        #region Properties
        /// <summary>
        /// List of pattern groups carried by the query
        /// </summary>
        internal List<RDFPatternGroup> PatternGroups { get; set; }

        /// <summary>
        /// Dictionary of pattern result tables
        /// </summary>
        internal Dictionary<RDFPatternGroup, List<DataTable>> PatternResultTables { get; set; }

        /// <summary>
        /// Dictionary of pattern-group result tables
        /// </summary>
        internal Dictionary<RDFPatternGroup, DataTable> PatternGroupResultTables { get; set; }

        /// <summary>
        /// List of modifiers carried by the query
        /// </summary>
        internal List<RDFModifier> Modifiers { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty query
        /// </summary>
        internal RDFQuery() {
            this.PatternGroups            = new List<RDFPatternGroup>();
            this.PatternResultTables      = new Dictionary<RDFPatternGroup, List<DataTable>>();
            this.PatternGroupResultTables = new Dictionary<RDFPatternGroup, DataTable>();
            this.Modifiers                = new List<RDFModifier>();
        }
        #endregion

    }

}