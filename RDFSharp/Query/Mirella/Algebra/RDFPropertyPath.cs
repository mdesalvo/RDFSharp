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
    /// <para>
    /// RDFPropertyPath represents a SPARQL 1.1 property path connecting two terms (Start and End) in a RDF
    /// datasource. A property path is modelled as a recursive expression TREE (<see cref="RDFPropertyPathExpression"/>),
    /// mirroring the SPARQL 1.1 path algebra (W3C §18.4): the tree can represent every spec-legal path shape,
    /// including the genuinely recursive ones that a flat list of steps cannot — negated property sets
    /// (<c>!iri</c>, <c>!(a|b)</c>), a cardinality applied to a group (<c>(a/b)+</c>), the inverse of a group
    /// (<c>^(a|b)</c>) and a sequence used as a branch of an alternative (<c>(a/b)|c</c>).
    /// </para>
    /// <para>
    /// The top level of a path is always a SEQUENCE of "units" (a single unit is unwrapped). Each unit is an
    /// arbitrary <see cref="RDFPropertyPathExpression"/>: this lets the historical builder API
    /// (<see cref="AddSequenceStep(RDFPropertyPathStep)"/> / <see cref="AddAlternativeSteps(List{RDFPropertyPathStep})"/>)
    /// keep appending simple single-predicate units, while the expression-typed overloads append arbitrarily
    /// nested sub-paths for the recursive shapes.
    /// </para>
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
        /// Number of top-level sequence units of the path
        /// </summary>
        internal int Depth => SequenceUnits.Count;

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
        /// Adds the given alternative steps to the path as a single unit. If only one is given, it is added as a
        /// plain sequence step. This is the simple-predicate convenience over <see cref="AddAlternativeSteps(List{RDFPropertyPathExpression})"/>.
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPath AddAlternativeSteps(List<RDFPropertyPathStep> alternativeSteps)
        {
            #region Guards
            if (alternativeSteps == null || alternativeSteps.Count == 0)
                throw new RDFQueryException("Cannot add alternative steps because the given list is null or it does not contain elements.");
            if (alternativeSteps.Any(step => step == null))
                throw new RDFQueryException("Cannot add alternative steps because the given list contains a null element.");
            #endregion

            return AddAlternativeSteps(alternativeSteps.ConvertAll(step => step.ToExpression()));
        }

        /// <summary>
        /// Adds the given alternative sub-paths to the path as a single unit (OR semantics). A sequence used as a
        /// branch of the alternative (e.g. <c>(a/b)|c</c>) is now representable. A single branch is added as a
        /// plain sequence unit.
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
        /// Adds the given sequence step to the path. This is the simple-predicate convenience over
        /// <see cref="AddSequenceStep(RDFPropertyPathExpression)"/>.
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPath AddSequenceStep(RDFPropertyPathStep sequenceStep)
        {
            #region Guards
            if (sequenceStep == null)
                throw new RDFQueryException("Cannot add sequence step because it is null.");
            #endregion

            return AddSequenceStep(sequenceStep.ToExpression());
        }

        /// <summary>
        /// Adds the given sub-path to the path as a sequence unit (AND semantics). The unit can be an arbitrary
        /// nested expression, so groups carrying a cardinality (<c>(a/b)+</c>), the inverse of a group
        /// (<c>^(a|b)</c>) and negated property sets (<c>!(a|b)</c>) are now representable.
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
        /// Collects, in document order, the predicate IRIs of every Link node of the path. Used by the SHACL
        /// serializer, whose property shapes only ever hold flat sequence/alternative paths of plain predicates.
        /// </summary>
        internal List<RDFResource> GetFlatStepProperties()
        {
            List<RDFResource> properties = new List<RDFResource>();

            void Collect(RDFPropertyPathExpression expression)
            {
                if (expression.Kind == RDFQueryEnums.RDFPropertyPathExpressionKinds.Link)
                    properties.Add(expression.Property);
                else if (expression.Children != null)
                    expression.Children.ForEach(Collect);
            }

            if (Expression != null)
                Collect(Expression);
            return properties;
        }

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
            if (property == null)
                throw new RDFQueryException("Cannot create property path Link because given \"property\" parameter is null.");
            return new RDFPropertyPathExpression(RDFQueryEnums.RDFPropertyPathExpressionKinds.Link) { Property = property };
        }

        /// <summary>
        /// Builds a Sequence node composing the given sub-paths (<c>P1/P2/…</c>)
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathExpression Sequence(List<RDFPropertyPathExpression> children)
            => CompositeNode(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence, children, "Sequence");

        /// <summary>
        /// Builds an Alternative node unioning the given sub-paths (<c>P1|P2|…</c>)
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathExpression Alternative(List<RDFPropertyPathExpression> children)
            => CompositeNode(RDFQueryEnums.RDFPropertyPathExpressionKinds.Alternative, children, "Alternative");

        /// <summary>
        /// Builds a NegatedPropertySet node matching one hop over any predicate NOT among the given members.
        /// A member with IsInverse=true contributes to the inverse-direction exclusion set (<c>^a</c> in <c>!(a|^b)</c>).
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPropertyPathExpression NegatedPropertySet(List<(RDFResource Property, bool IsInverse)> members)
        {
            if (members == null)
                throw new RDFQueryException("Cannot create property path NegatedPropertySet because given \"members\" parameter is null.");
            if (members.Any(m => m.Property == null))
                throw new RDFQueryException("Cannot create property path NegatedPropertySet because given \"members\" contains a null predicate.");
            return new RDFPropertyPathExpression(RDFQueryEnums.RDFPropertyPathExpressionKinds.NegatedPropertySet) { NegatedMembers = members };
        }

        private static RDFPropertyPathExpression CompositeNode(RDFQueryEnums.RDFPropertyPathExpressionKinds kind, List<RDFPropertyPathExpression> children, string label)
        {
            if (children == null || children.Count == 0)
                throw new RDFQueryException($"Cannot create property path {label} because given \"children\" parameter is null or empty.");
            if (children.Any(c => c == null))
                throw new RDFQueryException($"Cannot create property path {label} because given \"children\" contains a null element.");
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
            => new RDFPropertyPathExpression(RDFQueryEnums.RDFPropertyPathExpressionKinds.Sequence)
               { Children = new List<RDFPropertyPathExpression> { innerSubPath } };
        #endregion
    }

    /// <summary>
    /// RDFPropertyPathStep is a simple-predicate convenience for building a property path: a single predicate IRI
    /// carrying an optional inverse flag and an optional cardinality (<c>? * +</c>). It materializes into a Link
    /// <see cref="RDFPropertyPathExpression"/> when added to a path.
    /// </summary>
    public sealed class RDFPropertyPathStep
    {
        #region Properties
        /// <summary>
        /// Property of the step
        /// </summary>
        public RDFResource StepProperty { get; internal set; }

        /// <summary>
        /// Flag indicating that the step has inverse evaluation
        /// </summary>
        public bool IsInverseStep { get; internal set; }

        /// <summary>
        /// Cardinality constraint of the step
        /// </summary>
        public RDFQueryEnums.RDFPropertyPathStepCardinalities StepCardinality { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Builds a step of a property path
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPathStep(RDFResource stepProperty)
        {
            StepProperty = stepProperty ?? throw new RDFQueryException("Cannot create RDFPropertyPathStep because given \"stepProperty\" parameter is null.");
            StepCardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the step as inverse
        /// </summary>
        public RDFPropertyPathStep Inverse()
        {
            IsInverseStep = true;
            return this;
        }

        /// <summary>
        /// Sets the step cardinality to zero-or-one (SPARQL ?)
        /// </summary>
        public RDFPropertyPathStep ZeroOrOne()
        {
            StepCardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrOne;
            return this;
        }

        /// <summary>
        /// Sets the step cardinality to one-or-more transitive closure (SPARQL +)
        /// </summary>
        public RDFPropertyPathStep OneOrMore()
        {
            StepCardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore;
            return this;
        }

        /// <summary>
        /// Sets the step cardinality to zero-or-more reflexive-transitive closure (SPARQL *)
        /// </summary>
        public RDFPropertyPathStep ZeroOrMore()
        {
            StepCardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore;
            return this;
        }

        /// <summary>
        /// Materializes the step into the equivalent Link path expression (predicate + inverse + cardinality).
        /// </summary>
        internal RDFPropertyPathExpression ToExpression()
        {
            RDFPropertyPathExpression link = RDFPropertyPathExpression.Link(StepProperty);
            link.IsInverse = IsInverseStep;
            link.Cardinality = StepCardinality;
            return link;
        }
        #endregion
    }
}
