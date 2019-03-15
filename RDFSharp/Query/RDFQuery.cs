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
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFQuery is the foundation class for modeling SPARQL queries
    /// </summary>
    public abstract class RDFQuery
    {

        #region Properties
        /// <summary>
        /// List of members carried by the query
        /// </summary>
        internal List<RDFQueryMember> QueryMembers { get; set; }

        /// <summary>
        /// List of prefixes registered for the query
        /// </summary>
        internal List<RDFNamespace> Prefixes { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty query
        /// </summary>
        internal RDFQuery()
        {
            this.QueryMembers = new List<RDFQueryMember>();
            this.Prefixes = new List<RDFNamespace>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the query members of type: pattern group
        /// </summary>
        internal IEnumerable<RDFPatternGroup> GetPatternGroups()
        {
            return this.QueryMembers.Where(q => q is RDFPatternGroup)
                                    .OfType<RDFPatternGroup>();
        }

        /// <summary>
        /// Gets the query members of type: modifier
        /// </summary>
        internal IEnumerable<RDFModifier> GetModifiers()
        {
            return this.QueryMembers.Where(q => q is RDFModifier)
                                    .OfType<RDFModifier>();
        }

        /// <summary>
        /// Gets the query members which can be evaluated
        /// </summary>
        internal IEnumerable<RDFQueryMember> GetEvaluableQueryMembers()
        {
            return this.QueryMembers.Where(q => q.IsEvaluable);
        }
        #endregion

    }

}