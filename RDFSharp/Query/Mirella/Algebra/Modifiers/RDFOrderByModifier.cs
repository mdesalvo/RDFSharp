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

using System.Linq;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFOrderByModifier is a modifier which applies a sort on the results of a SELECT query for the given ordering key.
    /// </summary>
    public sealed class RDFOrderByModifier : RDFModifier
    {
        #region Properties
        /// <summary>
        /// Expression to be ordered (the ordering key). A bare-variable ORDER BY is represented as an
        /// <see cref="RDFVariableExpression"/> over that variable.
        /// </summary>
        public RDFExpression Expression { get; internal set; }

        /// <summary>
        /// Flavor of ordering (ASC/DESC)
        /// </summary>
        public RDFQueryEnums.RDFOrderByFlavors OrderByFlavor { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds an OrderBy modifier of the given flavor on the given variable (ergonomic ctor: the variable is
        /// wrapped into an <see cref="RDFVariableExpression"/>, which becomes the single stored ordering key).
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFOrderByModifier(RDFVariable variable, RDFQueryEnums.RDFOrderByFlavors orderbyFlavor)
        {
            if (variable == null)
                throw new RDFQueryException("Cannot create RDFOrderByModifier because given \"variable\" parameter is null.");
            OrderByFlavor = orderbyFlavor;
            Expression = new RDFVariableExpression(variable);
        }

        /// <summary>
        /// Builds an OrderBy modifier of the given flavor on the given expression (the ordering key).
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFOrderByModifier(RDFExpression expression, RDFQueryEnums.RDFOrderByFlavors orderbyFlavor)
        {
            OrderByFlavor = orderbyFlavor;
            Expression = expression ?? throw new RDFQueryException("Cannot create RDFOrderByModifier because given \"expression\" parameter is null.");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier (e.g. "ASC(?x)", "DESC(STRLEN(?x))"). For the bare-variable
        /// case the variable expression renders as "?x", so the output stays identical to the historical form.
        /// </summary>
        public override string ToString()
            => $"{OrderByFlavor}({Expression})";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the modifier on the given table (stable Ordinal sort on the ordering key's column, UNBOUND
        /// sorts smallest; keys whose column is absent are ignored). In the live pipeline the ORDER BY sort
        /// is applied by the projection step, so this is exercised mainly by direct callers and tests.
        /// The ordering key column is resolved the same way as the live pipeline: a bare variable sorts on its
        /// existing column, any other expression is materialized into a (synthetic) column, sorted, then dropped.
        /// </summary>
        internal override RDFTable ApplyModifier(RDFTable table)
        {
            //Materialize the ordering key into a column (the existing variable column, or a synthetic one)
            string sortColumn = EnsureSortColumn(table);

            //Stable Ordinal sort on that column
            RDFTable sortedTable = RDFTableEngine.SortTable(table, new[] { (sortColumn, OrderByFlavor == RDFQueryEnums.RDFOrderByFlavors.DESC) });

            //Drop any synthetic ordering-key column so it never surfaces in the results (e.g. under SELECT *)
            foreach (RDFTableColumn syntheticColumn in sortedTable.Columns.Where(column => column.IsSynthetic).ToList())
                sortedTable.RemoveColumn(syntheticColumn.Name);
            return sortedTable;
        }

        /// <summary>
        /// Ensures the ordering key is available as a single column of the given table and returns its name. A bare
        /// variable reuses its existing column; any other expression is evaluated into a fresh (synthetic) column
        /// (same primitives the GROUP BY path uses to materialize computed columns). This is the single locus where
        /// the variable-vs-expression discrimination lives.
        /// </summary>
        internal string EnsureSortColumn(RDFTable table)
        {
            //Bare-variable ordering key: sort directly on the variable's existing column (nothing to materialize)
            if (Expression is RDFVariableExpression variableExpression && variableExpression.LeftArgument is RDFVariable orderingVariable)
                return orderingVariable.ToString();

            //Any other expression: materialize it into a synthetic column, reusing the same primitive the
            //projection/GROUP BY paths use (the column is flagged synthetic so it never surfaces in the results)
            RDFVariable syntheticVariable = new RDFVariable($"__ORDERBYEXPR{GetHashCode():X}");
            RDFTableEngine.EvaluateExpression(Expression, syntheticVariable, table, isSynthetic: true);
            return syntheticVariable.ToString();
        }
        #endregion
    }
}