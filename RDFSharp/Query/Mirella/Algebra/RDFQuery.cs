/*
   Copyright 2012-2025 Marco De Salvo

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

using System.Collections.Generic;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQuery is the foundation class for modeling SPARQL queries
    /// </summary>
    public class RDFQuery : RDFQueryMember
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
        /// Builds an empty query
        /// </summary>
        internal RDFQuery()
        {
            QueryMembers = [];
            Prefixes = [];
            IsEvaluable = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern group to the body of the query
        /// </summary>
        internal T AddPatternGroup<T>(RDFPatternGroup patternGroup) where T: RDFQuery
        {
            if (patternGroup != null && !GetPatternGroups().Any(q => q.Equals(patternGroup)))
                QueryMembers.Add(patternGroup);
            return (T)this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        internal T AddModifier<T>(RDFDistinctModifier modifier) where T : RDFQuery
        {
            if (modifier != null && !GetModifiers().Any(m => m is RDFDistinctModifier))
                QueryMembers.Add(modifier);
            return (T)this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        internal T AddModifier<T>(RDFLimitModifier modifier) where T : RDFQuery
        {
            if (modifier != null && !GetModifiers().Any(m => m is RDFLimitModifier))
                QueryMembers.Add(modifier);
            return (T)this;
        }

        /// <summary>
        /// Adds the given modifier to the query
        /// </summary>
        internal T AddModifier<T>(RDFOffsetModifier modifier) where T : RDFQuery
        {
            if (modifier != null && !GetModifiers().Any(m => m is RDFOffsetModifier))
                QueryMembers.Add(modifier);
            return (T)this;
        }

        /// <summary>
        /// Adds the given prefix declaration to the query
        /// </summary>
        internal T AddPrefix<T>(RDFNamespace prefix) where T : RDFQuery
        {
            if (prefix != null && !Prefixes.Any(p => p.Equals(prefix)))
                Prefixes.Add(prefix);
            return (T)this;
        }

        /// <summary>
        /// Adds the given subquery to the query
        /// </summary>
        internal T AddSubQuery<T>(RDFSelectQuery subQuery) where T : RDFQuery
        {
            if (subQuery != null && !GetSubQueries().Any(q => q.Equals(subQuery)))
                QueryMembers.Add(subQuery.SubQuery());
            return (T)this;
        }

        /// <summary>
        /// Gets the query members of type: pattern group
        /// </summary>
        internal IEnumerable<RDFPatternGroup> GetPatternGroups()
            => QueryMembers.OfType<RDFPatternGroup>();

        /// <summary>
        /// Gets the query members of type: modifier
        /// </summary>
        internal IEnumerable<RDFModifier> GetModifiers()
            => QueryMembers.OfType<RDFModifier>();

        /// <summary>
        /// Gets the query members of type: query
        /// </summary>
        internal IEnumerable<RDFQuery> GetSubQueries()
            => QueryMembers.OfType<RDFQuery>();

        /// <summary>
        /// Gets the prefixes of the query, including those from subqueries
        /// </summary>
        internal List<RDFNamespace> GetPrefixes()
        {
            List<RDFNamespace> result = [.. Prefixes];
            foreach (RDFQuery subQuery in GetSubQueries())
                result.AddRange(subQuery.GetPrefixes());
            return [.. result.Distinct()];
        }

        /// <summary>
        /// Gets the query members which can be evaluated
        /// </summary>
        internal IEnumerable<RDFQueryMember> GetEvaluableQueryMembers()
            => QueryMembers.Where(q => q.IsEvaluable);
        #endregion
    }

    /// <summary>
    /// RDFQueryResult is the foundation class for modeling SPARQL query results
    /// </summary>
    public class RDFQueryResult {}
}