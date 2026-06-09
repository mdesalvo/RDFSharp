/*
   Copyright 2012-2026 Marco De Salvo

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
            QueryMembers = new List<RDFQueryMember>();
            Prefixes = new List<RDFNamespace>();
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
        /// Adds the given operator tree to the body of the query
        /// </summary>
        internal T AddOperator<T>(RDFOperatorQueryMember operatorMember) where T : RDFQuery
        {
            if (operatorMember != null)
                QueryMembers.Add(operatorMember);
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
            List<RDFNamespace> result = new List<RDFNamespace>(Prefixes);

            //Collect prefixes from subqueries directly attached as query members
            foreach (RDFQuery subQuery in GetSubQueries())
                result.AddRange(subQuery.GetPrefixes());

            //Collect prefixes from subqueries nested inside operator tree members (Union/Minus),
            //otherwise the printed query would reference prefixes it never declares
            foreach (RDFOperatorQueryMember operatorMember in QueryMembers.OfType<RDFOperatorQueryMember>())
                CollectOperatorTreePrefixes(operatorMember, result);

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Collects the prefixes declared by the subqueries living inside an operator tree node,
        /// recursing through both operands so that deeply nested subqueries are reached as well
        /// </summary>
        private static void CollectOperatorTreePrefixes(RDFOperatorQueryMember operatorMember, List<RDFNamespace> result)
        {
            CollectOperatorTreeOperandPrefixes(operatorMember.LeftOperand, result);
            CollectOperatorTreeOperandPrefixes(operatorMember.RightOperand, result);
        }

        /// <summary>
        /// Collects the prefixes from a single operator tree operand: a subquery contributes its own
        /// prefixes, a nested operator node is traversed recursively, a pattern group has no prefixes
        /// </summary>
        private static void CollectOperatorTreeOperandPrefixes(RDFQueryMember operand, List<RDFNamespace> result)
        {
            switch (operand)
            {
                case RDFSelectQuery subQueryOperand:
                    result.AddRange(subQueryOperand.GetPrefixes());
                    break;
                case RDFOperatorQueryMember operatorOperand:
                    CollectOperatorTreePrefixes(operatorOperand, result);
                    break;
            }
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