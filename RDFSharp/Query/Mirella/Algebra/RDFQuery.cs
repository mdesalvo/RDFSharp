﻿/*
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

using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFQuery is the foundation class for modeling SPARQL queries
    /// </summary>
    public abstract class RDFQuery: RDFQueryMember
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

        /// <summary>
        /// Flag indicating the subquery to be joined as Optional
        /// </summary>
        internal Boolean IsOptional { get; set; }

        /// <summary>
        /// Flag indicating the subquery to be joined as Union
        /// </summary>
        internal Boolean JoinAsUnion { get; set; }

        /// <summary>
        /// Flag indicating that the query is a subquery
        /// </summary>
        internal Boolean IsSubQuery { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty query
        /// </summary>
        internal RDFQuery()
        {
            this.QueryMembers = new List<RDFQueryMember>();
            this.Prefixes = new List<RDFNamespace>();
            this.IsEvaluable = true;
            this.IsOptional = false;
            this.JoinAsUnion = false;
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
        /// Gets the query members of type: query
        /// </summary>
        internal IEnumerable<RDFQuery> GetSubQueries()
        {
            return this.QueryMembers.Where(q => q is RDFQuery)
                                    .OfType<RDFQuery>();
        }

        /// <summary>
        /// Gets the SPARQL values of the query, including those from patterngroups and subqueries
        /// </summary>
        internal List<RDFValues> GetValues()
        {
            List<RDFValues> result = new List<RDFValues>();

            //Add SPARQL values from pattern groups
            foreach (RDFPatternGroup patternGroup in this.GetPatternGroups())
                result.AddRange(patternGroup.GetValues());

            //Add SPARQL values from subqueries
            foreach (RDFQuery subQuery in this.GetSubQueries())
                result.AddRange(subQuery.GetValues());

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Injects the given list of SPARQL values to patterngroups and subqueries of the query
        /// </summary>
        internal RDFQuery InjectValues(List<RDFValues> values)
        {

            //Inject SPARQL values into pattern groups
            foreach (RDFPatternGroup patternGroup in this.GetPatternGroups())
                values.ForEach(v => patternGroup.AddInjectedValues(v));

            //Inject SPARQL values into subqueries
            foreach (RDFQuery subQuery in this.GetSubQueries())
                subQuery.InjectValues(values);

            return this;
        }

        /// <summary>
        /// Gets the prefixes of the query, including those from subqueries
        /// </summary>
        internal List<RDFNamespace> GetPrefixes()
        {
            List<RDFNamespace> result = new List<RDFNamespace>(this.Prefixes);
            foreach(RDFQuery subQuery in this.GetSubQueries())
                result.AddRange(subQuery.GetPrefixes());
            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets the query members which can be evaluated
        /// </summary>
        internal IEnumerable<RDFQueryMember> GetEvaluableQueryMembers()
        {
            return this.QueryMembers.Where(q => q.IsEvaluable);
        }

        /// <summary>
        /// Gets the strin representation of the query member
        /// </summary>
        internal override String GetQueryMemberString()
        {
            return this.ToString();
        }

        /// <summary>
        /// Sets the query as a subquery
        /// </summary>
        internal RDFQuery SubQuery()
        {
            this.IsSubQuery = true;
            return this;
        }
        #endregion

    }

}