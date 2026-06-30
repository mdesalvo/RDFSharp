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
    /// RDFPropertyPath represents a SPARQL 1.1 property path connecting two terms (Start and End) in a RDF datasource
    /// </summary>
    public sealed class RDFPropertyPath : RDFPatternGroupMember
    {
        #region Properties
        /// <summary>
        /// Start of the path
        /// </summary>
        public RDFPatternMember Start { get; internal set; }

        /// <summary>
        /// End of the path
        /// </summary>
        public RDFPatternMember End { get; internal set; }

        /// <summary>
        /// Top-level sequence units composing the path (a path is a sequence of one or more units; each unit is
        /// an arbitrary path expression). Their concatenation is exposed as <see cref="Expression"/>.
        /// </summary>
        internal List<RDFPropertyPathExpression> SequenceUnits { get; }

        /// <summary>
        /// Root of the path expression tree: the single unit when there is exactly one, otherwise the sequence
        /// of all units. Null until at least one unit has been added.
        /// </summary>
        internal RDFPropertyPathExpression Expression
            => SequenceUnits.Count == 0 ? null
             : SequenceUnits.Count == 1 ? SequenceUnits[0]
             : RDFPropertyPathExpression.Sequence(SequenceUnits);

        /// <summary>
        /// Flag indicating the property path as Optional
        /// </summary>
        internal bool IsOptional { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a path between the given terms
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPath(RDFPatternMember start, RDFPatternMember end)
        {
            #region Guards
            if (start == null)
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"start\" parameter is null.");
            if (!(start is RDFResource || start is RDFVariable))
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"start\" parameter is neither a resource or a variable.");
            if (end == null)
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"end\" parameter is null.");
            if (!(end is RDFResource || end is RDFVariable))
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"end\" parameter is neither a resource or a variable.");
            #endregion

            Start = start;
            End = end;
            SequenceUnits = new List<RDFPropertyPathExpression>();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the path
        /// </summary>
        public override string ToString()
            => ToString(RDFModelUtilities.EmptyNamespaceList);
        internal string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintPropertyPath(this, prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Sets the property path to be joined as Optional with the previous member
        /// </summary>
        public RDFPropertyPath Optional()
        {
            IsOptional = true;
            return this;
        }

        /// <summary>
        /// Creates a Union operator combining this property path with the given pattern
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryPatternGroupMember Union(RDFPattern other)
            => new RDFBinaryPatternGroupMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this property path with the given property path
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryPatternGroupMember Union(RDFPropertyPath other)
            => new RDFBinaryPatternGroupMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Union operator combining this property path with the given operator tree
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryPatternGroupMember Union(RDFBinaryPatternGroupMember other)
            => new RDFBinaryPatternGroupMember(RDFQueryEnums.RDFBinaryOperatorType.Union, this, other);

        /// <summary>
        /// Creates a Minus operator combining this property path with the given pattern
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryPatternGroupMember Minus(RDFPattern other)
            => new RDFBinaryPatternGroupMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this property path with the given property path
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryPatternGroupMember Minus(RDFPropertyPath other)
            => new RDFBinaryPatternGroupMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Creates a Minus operator combining this property path with the given operator tree
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFBinaryPatternGroupMember Minus(RDFBinaryPatternGroupMember other)
            => new RDFBinaryPatternGroupMember(RDFQueryEnums.RDFBinaryOperatorType.Minus, this, other);

        /// <summary>
        /// Adds the given alternative sub-paths to the path as a single unit (OR semantics).
        /// A single branch is added as a plain sequence unit.
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPath AddAlternativeSteps(List<RDFPropertyPathExpression> alternativeSteps)
        {
            #region Guards
            if (alternativeSteps == null || alternativeSteps.Count == 0)
                throw new RDFQueryException("Cannot add alternative steps because the given list is null or it does not contain elements.");
            if (alternativeSteps.Any(step => step == null))
                throw new RDFQueryException("Cannot add alternative steps because the given list contains a null element.");
            #endregion

            //A single branch is just a plain sequence unit; two or more become one alternative unit
            SequenceUnits.Add(alternativeSteps.Count == 1
                                ? alternativeSteps[0]
                                : RDFPropertyPathExpression.Alternative(alternativeSteps));

            UpdateEvaluabilityStatus();
            return this;
        }

        /// <summary>
        /// Adds the given sub-path to the path as a sequence unit (AND semantics)
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPath AddSequenceStep(RDFPropertyPathExpression sequenceStep)
        {
            #region Guards
            if (sequenceStep == null)
                throw new RDFQueryException("Cannot add sequence step because it is null.");
            #endregion

            SequenceUnits.Add(sequenceStep);

            UpdateEvaluabilityStatus();
            return this;
        }

        /// <summary>
        /// Returns the predicate IRI of this path when its expression is a single plain predicate. Every
        /// decorated or composite path (inverse, recursive cardinality, sequence, alternative) returns null.
        /// </summary>
        internal RDFResource AsSinglePredicate()
            => Expression is RDFPropertyPathExpression expression && expression.IsPlainLink
                ? expression.Property : null;

        /// <summary>
        /// Recomputes whether the path is independently evaluable by the engine. A path is evaluable unless it is
        /// a single, plain, exactly-once, non-inverse predicate between two concrete terms (which is just a ground
        /// triple, joined like a pattern rather than evaluated as a path).
        /// </summary>
        private void UpdateEvaluabilityStatus()
        {
            bool hasComplexExpression = SequenceUnits.Count > 1 || (SequenceUnits.Count == 1 && !SequenceUnits[0].IsPlainLink);
            IsEvaluable = Start is RDFVariable || End is RDFVariable || hasComplexExpression;
        }
        #endregion
    }

    /// <summary>
    /// <para>
    /// RDFPropertyPathExpression is a node of the SPARQL 1.1 property path algebra tree. Every node denotes a
    /// BINARY RELATION over the datasource resources (the set of (start, end) pairs the sub-path connects), and
    /// carries an optional inverse decoration (<c>^</c>) and an optional cardinality modifier (<c>? * +</c>).
    /// </para>
    /// <para>
    /// Node kinds (see <see cref="RDFQueryEnums.RDFPropertyPathExpressionKinds"/>):
    /// <list type="bullet">
    /// <item><c>Link</c> — a single predicate IRI (the atomic step);</item>
    /// <item><c>Sequence</c> — an ordered composition of children (<c>P1/P2/…</c>);</item>
    /// <item><c>Alternative</c> — a union of children (<c>P1|P2|…</c>);</item>
    /// <item><c>NegatedPropertySet</c> — one hop over any predicate NOT in the given set (<c>!(a|^b)</c>).</item>
    /// </list>
    /// A group is implicit in the tree structure: <c>(a/b)+</c> is a Sequence carrying the OneOrMore cardinality,
    /// <c>^(a|b)</c> is an Alternative carrying the inverse flag. When a decoration would conflict with the
    /// evaluation order (cardinality is applied before inverse), the combinator wraps the node in a single-child
    /// Sequence acting as the explicit group.
    /// </para>
    /// </summary>
    public sealed class RDFPropertyPathExpression
    {
        #region Properties
        /// <summary>
        /// Kind of the node
        /// </summary>
        public RDFQueryEnums.RDFPropertyPathExpressionKinds Kind { get; internal set; }

        /// <summary>
        /// Predicate IRI of a Link node
        /// </summary>
        public RDFResource Property { get; internal set; }

        /// <summary>
        /// Children of a Sequence or Alternative node
        /// </summary>
        internal List<RDFPropertyPathExpression> Children { get; set; }

        /// <summary>
        /// Members of a NegatedPropertySet node (each is a predicate IRI plus a flag telling whether it is matched
        /// in the inverse direction, i.e. the <c>^a</c> members of <c>!(a|^b)</c>)
        /// </summary>
        internal List<(RDFResource Property, bool IsInverse)> NegatedMembers { get; set; }

        /// <summary>
        /// Flag indicating that the node is matched in the inverse direction (<c>^</c>)
        /// </summary>
        public bool IsInverse { get; internal set; }

        /// <summary>
        /// Cardinality modifier applied to the node (<c>? * +</c>, or ExactlyOne when none)
        /// </summary>
        public RDFQueryEnums.RDFPropertyPathStepCardinalities Cardinality { get; internal set; }
            = RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne;

        /// <summary>
        /// Tells whether the node is a plain Link (single predicate, exactly-once, non-inverse) — the only shape
        /// equivalent to a ground triple predicate.
        /// </summary>
        internal bool IsPlainLink
            => Kind == RDFQueryEnums.RDFPropertyPathExpressionKinds.Link
                && !IsInverse
                && Cardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne;
        #endregion

        #region Ctors
        private RDFPropertyPathExpression(RDFQueryEnums.RDFPropertyPathExpressionKinds kind)
            => Kind = kind;
        #endregion

        #region Factories
        /// <summary>
        /// Builds a Link node over the given predicate IRI
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathExpression Link(RDFResource property)
        {
            #region Guards
            if (property == null)
                throw new RDFQueryException("Cannot create property path Link because given \"property\" parameter is null.");
            #endregion

            return new RDFPropertyPathExpression(RDFQueryEnums.RDFPropertyPathExpressionKinds.Link) { Property = property };
        }

        /// <summary>
        /// Builds a Sequence node composing the given sub-paths (<c>P1/P2/…</c>)
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathExpression Sequence(List<RDFPropertyPathExpression> children)
            => CompositeNode(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, children);

        /// <summary>
        /// Builds an Alternative node unioning the given sub-paths (<c>P1|P2|…</c>)
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathExpression Alternative(List<RDFPropertyPathExpression> children)
            => CompositeNode(RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative, children);

        /// <summary>
        /// Builds a NegatedPropertySet node matching one hop over any predicate NOT among the given members.
        /// A member with IsInverse=true contributes to the inverse-direction exclusion set (<c>^a</c> in <c>!(a|^b)</c>).
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathExpression NegatedPropertySet(List<(RDFResource Property, bool IsInverse)> members)
        {
            #region Guards
            if (members == null)
                throw new RDFQueryException("Cannot create property path NegatedPropertySet because given \"members\" parameter is null.");
            if (members.Any(m => m.Property == null))
                throw new RDFQueryException("Cannot create property path NegatedPropertySet because given \"members\" contains a null predicate.");
            #endregion

            return new RDFPropertyPathExpression(RDFQueryEnums.RDFPropertyPathExpressionKinds.NegatedPropertySet) { NegatedMembers = members };
        }

        private static RDFPropertyPathExpression CompositeNode(RDFQueryEnums.RDFPropertyPathExpressionKinds kind, List<RDFPropertyPathExpression> children)
        {
            #region Guards
            if (children == null || children.Count == 0)
                throw new RDFQueryException($"Cannot create property path {kind} because given \"children\" parameter is null or empty.");
            if (children.Any(c => c == null))
                throw new RDFQueryException($"Cannot create property path {kind} because given \"children\" contains a null element.");
            #endregion

            return new RDFPropertyPathExpression(kind) { Children = new List<RDFPropertyPathExpression>(children) };
        }
        #endregion

        #region Combinators
        /// <summary>
        /// Returns the inverse of this sub-path (<c>^P</c>). Wraps in a group when an inverse is already present,
        /// so a double inverse stays well-formed.
        /// </summary>
        public RDFPropertyPathExpression Inverse()
        {
            RDFPropertyPathExpression nodeToInvert = IsInverse ? WrapInImplicitGroup(this) : this;
            nodeToInvert.IsInverse = true;
            return nodeToInvert;
        }

        /// <summary>
        /// Returns this sub-path with a zero-or-one cardinality (<c>P?</c>)
        /// </summary>
        public RDFPropertyPathExpression ZeroOrOne()
            => WithCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne);

        /// <summary>
        /// Returns this sub-path with a one-or-more transitive cardinality (<c>P+</c>)
        /// </summary>
        public RDFPropertyPathExpression OneOrMore()
            => WithCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore);

        /// <summary>
        /// Returns this sub-path with a zero-or-more reflexive-transitive cardinality (<c>P*</c>)
        /// </summary>
        public RDFPropertyPathExpression ZeroOrMore()
            => WithCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore);

        //Applies a cardinality. Because evaluation applies cardinality (inner) before inverse (outer), a
        //cardinality on a node that already carries an inverse or a cardinality must wrap in a group so that the
        //new cardinality stays the outermost operator.
        private RDFPropertyPathExpression WithCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities cardinality)
        {
            RDFPropertyPathExpression nodeToConstrain =
                (IsInverse || Cardinality != RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne)
                    ? WrapInImplicitGroup(this) : this;
            nodeToConstrain.Cardinality = cardinality;
            return nodeToConstrain;
        }

        //Builds the explicit single-child Sequence group wrapping the given inner sub-path
        private static RDFPropertyPathExpression WrapInImplicitGroup(RDFPropertyPathExpression innerSubPath)
            => new RDFPropertyPathExpression(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence) { Children = new List<RDFPropertyPathExpression> { innerSubPath } };
        #endregion
    }
}
