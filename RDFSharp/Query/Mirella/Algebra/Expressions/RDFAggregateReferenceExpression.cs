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
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFAggregateReferenceExpression is an INTERNAL bridge that lets an aggregate (COUNT(?e), AVG(?g), ...) appear
    /// inside a composite expression while the SPARQL parser is building it — e.g. a free HAVING condition like
    /// '(COUNT(?e) > 1 || AVG(?g) >= 24)', or a projection like '(?x + COUNT(?y) AS ?v)'.
    /// <para>
    /// It is deliberately NOT public: it only makes sense bound to an aggregator of a GroupBy modifier, so exposing it
    /// to consumers would create a failure surface elsewhere in the model. The easy, supported way for application
    /// code to reference an aggregate inside a free HAVING expression is to reference the aggregator's projection
    /// variable (a plain <see cref="RDFVariableExpression"/>); the parser uses THIS class only to faithfully re-print
    /// the original aggregate call for aggregates it had to materialize behind the scenes (a hidden aggregator).
    /// </para>
    /// <para>
    /// It keeps EVALUATION and PRINTING separate, both delegated to the referenced aggregator on demand: evaluation
    /// reads the aggregate's already-materialized column (the aggregator's projection variable); printing re-emits the
    /// aggregator's original call text (so '(?x + COUNT(?y) AS ?v)' round-trips instead of leaking a synthetic column
    /// name).
    /// </para>
    /// <para>
    /// ⚠️ UNDER EVALUATION (IP3.3 check-in postil): this node currently exists ONLY to make the two HIDDEN-aggregate
    /// cases round-trip — a HAVING condition over an aggregate NOT in the SELECT projection, and an aggregate nested
    /// in a projection expression. For an aggregate that IS projected, a plain <see cref="RDFVariableExpression"/> over
    /// its alias suffices and this node is not used. It also feels like a foreign body in the expression library: its
    /// prefix-aware ToString overload ignores the prefixes argument (the aggregate call form is prefix-independent,
    /// mirroring <see cref="RDFAggregator.ToString"/>, which is itself prefix-less). To be reconsidered: whether the
    /// two hidden-aggregate cases justify a dedicated node, or should be
    /// dropped (reopening them as model limits) so this class can be removed entirely.
    /// </para>
    /// </summary>
    internal sealed class RDFAggregateReferenceExpression : RDFExpression
    {
        #region Properties
        /// <summary>
        /// The aggregator this expression stands in for: the single source of truth for both the column to read at
        /// evaluation time and the call text to re-print (computed on demand, nothing is cached).
        /// </summary>
        internal RDFAggregator ReferencedAggregator { get; }

        /// <summary>
        /// Internal variable expression used to actually read the aggregate column value at evaluation time, reusing
        /// the row/UNBOUND parsing already implemented by <see cref="RDFVariableExpression"/>.
        /// </summary>
        private readonly RDFVariableExpression columnReader;
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a reference to the given aggregator: it reads from the aggregator's projection column at evaluation
        /// time and re-prints the aggregator's original call form on demand, so neither the column name nor the call
        /// text has to be supplied by hand.
        /// </summary>
        /// <exception cref="RDFQueryException">When the given aggregator is null.</exception>
        internal RDFAggregateReferenceExpression(RDFAggregator aggregator)
            : base((aggregator ?? throw new RDFQueryException("Cannot create RDFAggregateReferenceExpression because given \"aggregator\" parameter is null.")).ProjectionVariable,
                   null as RDFExpression)
        {
            ReferencedAggregator = aggregator;
            columnReader = new RDFVariableExpression(aggregator.ProjectionVariable);
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the aggregate reference: the aggregator's original call (e.g.
        /// "COUNT(?e)"), computed on demand, so the printed query re-parses to an equivalent reference rather than
        /// leaking the synthetic column name.
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal override string ToString(List<RDFNamespace> prefixes)
            => ReferencedAggregator.GetAggregateCallString();
        #endregion

        #region Methods
        /// <summary>
        /// Applies the expression on the given table row by reading the already-materialized aggregate column value.
        /// </summary>
        internal override RDFPatternMember ApplyExpression(RDFTableRow row)
            => columnReader.ApplyExpression(row);
        #endregion
    }
}
