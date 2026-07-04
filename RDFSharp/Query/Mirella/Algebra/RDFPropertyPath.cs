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
        public RDFPatternMember Start { get; }

        /// <summary>
        /// End of the path
        /// </summary>
        public RDFPatternMember End { get; }

        /// <summary>
        /// Top-level steps composing the path (a path is a sequence of one or more steps; each step is
        /// an arbitrary <see cref="RDFPropertyPathStep"/> sub-tree). Their concatenation is exposed as <see cref="StepThree"/>.
        /// </summary>
        internal List<RDFPropertyPathStep> Steps { get; }

        /// <summary>
        /// Root of the path step tree: the single unit when there is exactly one, otherwise the sequence
        /// of all units. Null until at least one unit has been added.
        /// </summary>
        internal RDFPropertyPathStep StepThree
            => Steps.Count == 0 ? null : RDFPropertyPathStep.Sequence(Steps);

        /// <summary>
        /// Flag indicating the property path as Optional
        /// </summary>
        internal bool IsOptional { get; private set; }
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
            Steps = new List<RDFPropertyPathStep>();
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
        /// Adds the given sub-path to the path as its next unit. What the unit "is" (a plain link, an inverse,
        /// a sequence, an alternative, a negated property set, …) is already carried by the step itself:
        /// build it with the matching <see cref="RDFPropertyPathStep"/> factory before adding it here.
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPath AddStep(RDFPropertyPathStep step)
        {
            #region Guards
            if (step == null)
                throw new RDFQueryException("Cannot add step because it is null.");
            #endregion

            Steps.Add(step);

            UpdateEvaluabilityStatus();
            return this;
        }

        /// <summary>
        /// Returns the predicate IRI of this path when its step tree is a single plain predicate. Every
        /// decorated or composite path (inverse, recursive cardinality, sequence, alternative) returns null.
        /// </summary>
        internal RDFResource AsSinglePredicate()
            => StepThree is RDFPropertyPathStep step && step.IsPlainLink
                ? step.Property : null;

        /// <summary>
        /// Recomputes whether the path is independently evaluable by the engine. A path is evaluable unless it is
        /// a single, plain, exactly-once, non-inverse predicate between two concrete terms (which is just a ground
        /// triple, joined like a pattern rather than evaluated as a path).
        /// </summary>
        private void UpdateEvaluabilityStatus()
        {
            bool hasComplexStep = Steps.Count > 1 || (Steps.Count == 1 && !Steps[0].IsPlainLink);
            IsEvaluable = Start is RDFVariable || End is RDFVariable || hasComplexStep;
        }
        #endregion
    }

    /// <summary>
    /// <para>
    /// RDFPropertyPathStep is a node of the SPARQL 1.1 property path algebra tree. Every node denotes a
    /// BINARY RELATION over the datasource resources (the set of (start, end) pairs the sub-path connects), and
    /// carries an optional inverse decoration (<c>^</c>) and an optional cardinality modifier (<c>? * +</c>).
    /// </para>
    /// <para>
    /// Step kinds (see <see cref="RDFQueryEnums.RDFPropertyPathStepKinds"/>):
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
    public sealed class RDFPropertyPathStep
    {
        #region Properties
        /// <summary>
        /// Kind of the step
        /// </summary>
        public RDFQueryEnums.RDFPropertyPathStepKinds Kind { get; }

        /// <summary>
        /// Predicate IRI of a Link step
        /// </summary>
        public RDFResource Property { get; private set; }

        /// <summary>
        /// Children of a Sequence or Alternative step
        /// </summary>
        internal List<RDFPropertyPathStep> Children { get; private set; }

        /// <summary>
        /// Predicates excluded in the forward direction by a NegatedPropertySet step (the <c>a</c> in <c>!(a|^b)</c>)
        /// </summary>
        internal List<RDFResource> ForwardMembers { get; private set; }

        /// <summary>
        /// Predicates excluded in the inverse direction by a NegatedPropertySet step (the <c>b</c> in <c>!(a|^b)</c>)
        /// </summary>
        internal List<RDFResource> InverseMembers { get; private set; }

        /// <summary>
        /// Flag indicating that the node is matched in the inverse direction (<c>^</c>)
        /// </summary>
        public bool IsInverse { get; private set; }

        /// <summary>
        /// Cardinality modifier applied to the node (<c>? * +</c>, or ExactlyOne when none)
        /// </summary>
        public RDFQueryEnums.RDFPropertyPathStepCardinalities Cardinality { get; private set; }
            = RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne;

        /// <summary>
        /// Tells whether the node is a plain Link (single predicate, exactly-once, non-inverse) — the only shape
        /// equivalent to a ground triple predicate.
        /// </summary>
        internal bool IsPlainLink
            => Kind == RDFQueryEnums.RDFPropertyPathStepKinds.Link
                && !IsInverse
                && Cardinality == RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne;
        #endregion

        #region Ctors
        private RDFPropertyPathStep(RDFQueryEnums.RDFPropertyPathStepKinds kind)
            => Kind = kind;
        #endregion

        #region Factories
        /// <summary>
        /// Builds a Link node over the given predicate IRI
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathStep Link(RDFResource property)
        {
            #region Guards
            if (property == null)
                throw new RDFQueryException("Cannot create property path Link because given \"property\" parameter is null.");
            #endregion

            return new RDFPropertyPathStep(RDFQueryEnums.RDFPropertyPathStepKinds.Link) { Property = property };
        }

        /// <summary>
        /// Builds a Sequence node composing the given sub-paths (<c>P1/P2/…</c>)
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathStep Sequence(List<RDFPropertyPathStep> children)
            => CompositeStep(RDFQueryEnums.RDFPropertyPathStepKinds.Sequence, children);

        /// <summary>
        /// Builds an Alternative node unioning the given sub-paths (<c>P1|P2|…</c>)
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathStep Alternative(List<RDFPropertyPathStep> children)
            => CompositeStep(RDFQueryEnums.RDFPropertyPathStepKinds.Alternative, children);

        /// <summary>
        /// Builds a NegatedPropertySet node matching one hop over any predicate NOT among the given properties
        /// (<c>!(a|^b)</c>): <paramref name="forwardProperties"/> are excluded when traversed in the forward
        /// direction, <paramref name="inverseProperties"/> when traversed in the inverse (<c>^</c>) direction.
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathStep NegatedPropertySet(List<RDFResource> forwardProperties, List<RDFResource> inverseProperties = null)
        {
            if (forwardProperties == null)
                forwardProperties = new List<RDFResource>();
            if (inverseProperties == null)
                inverseProperties = new List<RDFResource>();

            #region Guards
            if (forwardProperties.Any(p => p == null) || inverseProperties.Any(p => p == null))
                throw new RDFQueryException("Cannot create property path NegatedPropertySet because given properties contain a null predicate.");
            if (forwardProperties.Count == 0 && inverseProperties.Count == 0)
                throw new RDFQueryException("Cannot create property path NegatedPropertySet because both \"forwardProperties\" and \"inverseProperties\" parameters are null or empty.");
            #endregion

            return new RDFPropertyPathStep(RDFQueryEnums.RDFPropertyPathStepKinds.NegatedPropertySet)
            {
                ForwardMembers = forwardProperties,
                InverseMembers = inverseProperties
            };
        }

        private static RDFPropertyPathStep CompositeStep(RDFQueryEnums.RDFPropertyPathStepKinds kind, List<RDFPropertyPathStep> children)
        {
            #region Guards
            if (children == null || children.Count == 0)
                throw new RDFQueryException($"Cannot create property path {kind} because given \"children\" parameter is null or empty.");
            if (children.Any(c => c == null))
                throw new RDFQueryException($"Cannot create property path {kind} because given \"children\" contains a null element.");
            #endregion

            //A single child needs no composition: it stands for itself (e.g. a 1-branch Alternative is just that branch)
            return children.Count == 1
                ? children[0]
                : new RDFPropertyPathStep(kind) { Children = new List<RDFPropertyPathStep>(children) };
        }
        #endregion

        #region Combinators
        /// <summary>
        /// Returns the inverse of this sub-path (<c>^P</c>). Wraps in a group when an inverse is already present,
        /// so a double inverse stays well-formed.
        /// </summary>
        public RDFPropertyPathStep Inverse()
        {
            RDFPropertyPathStep nodeToInvert = IsInverse ? WrapInImplicitGroup(this) : this;
            nodeToInvert.IsInverse = true;
            return nodeToInvert;
        }

        /// <summary>
        /// Returns this sub-path with a zero-or-one cardinality (<c>P?</c>)
        /// </summary>
        public RDFPropertyPathStep ZeroOrOne()
            => WithCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne);

        /// <summary>
        /// Returns this sub-path with a one-or-more transitive cardinality (<c>P+</c>)
        /// </summary>
        public RDFPropertyPathStep OneOrMore()
            => WithCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore);

        /// <summary>
        /// Returns this sub-path with a zero-or-more reflexive-transitive cardinality (<c>P*</c>)
        /// </summary>
        public RDFPropertyPathStep ZeroOrMore()
            => WithCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore);

        //Applies a cardinality. Because evaluation applies cardinality (inner) before inverse (outer), a
        //cardinality on a node that already carries an inverse or a cardinality must wrap in a group so that the
        //new cardinality stays the outermost operator.
        private RDFPropertyPathStep WithCardinality(RDFQueryEnums.RDFPropertyPathStepCardinalities cardinality)
        {
            RDFPropertyPathStep nodeToConstrain =
                (IsInverse || Cardinality != RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne)
                    ? WrapInImplicitGroup(this) : this;
            nodeToConstrain.Cardinality = cardinality;
            return nodeToConstrain;
        }

        //Builds the explicit single-child Sequence group wrapping the given inner sub-path
        private static RDFPropertyPathStep WrapInImplicitGroup(RDFPropertyPathStep innerSubPath)
            => new RDFPropertyPathStep(RDFQueryEnums.RDFPropertyPathStepKinds.Sequence) { Children = new List<RDFPropertyPathStep> { innerSubPath } };
        #endregion
    }
}