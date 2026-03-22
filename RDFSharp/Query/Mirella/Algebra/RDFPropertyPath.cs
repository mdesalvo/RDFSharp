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
    /// RDFPropertyPath represents a chain of properties connecting two terms in a RDF datasource.
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
        /// Steps of the path
        /// </summary>
        internal List<RDFPropertyPathStep> Steps { get; set; }

        /// <summary>
        /// Depth of the path
        /// </summary>
        internal int Depth { get; set; }

        /// <summary>
        /// Indicates whether any step carries a transitive or bounded cardinality constraint
        /// </summary>
        internal bool HasTransitiveSteps
            => Steps.Any(s => s.StepCardinality != RDFQueryEnums.RDFPropertyPathStepCardinalities.ExactlyOne);
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
            Depth = 0;
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
        /// Adds the given alternative steps to the path. If only one is given, it is considered sequence.
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

            //Update depth status of the property path
            if (Steps.Count == 0
                 || alternativeSteps.Count == 1
                 || Steps.LastOrDefault()?.StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence)
            {
                Depth++;
            }

            //Collect the given steps into the property path
            if (alternativeSteps.Count == 1)
            {
                Steps.Add(alternativeSteps[0].SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence));
            }
            else
            {
                foreach (RDFPropertyPathStep alternativeStep in alternativeSteps)
                    Steps.Add(alternativeStep.SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative));
            }

            //Update evaluability status of the property path
            if (Start is RDFVariable || End is RDFVariable || Depth > 1 || HasTransitiveSteps)
                IsEvaluable = true;

            return this;
        }

        /// <summary>
        /// Adds the given sequence step to the path
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPath AddSequenceStep(RDFPropertyPathStep sequenceStep)
        {
            #region Guards
            if (sequenceStep == null)
                throw new RDFQueryException("Cannot add sequence step because it is null.");
            #endregion

            //Update depth status of the property path
            Depth++;

            //Collect the given step into the property path
            Steps.Add(sequenceStep
                                  .SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence));

            //Update evaluability status of the property path
            if (Start is RDFVariable || End is RDFVariable || Depth > 1 || HasTransitiveSteps)
                IsEvaluable = true;

            return this;
        }

        /// <summary>
        /// Gets the list of patterns corresponding to the path
        /// </summary>
        internal List<RDFPattern> GetPatternList()
        {
            List<RDFPattern> patterns = new List<RDFPattern>(Steps.Count);

            #region Single Property
            if (Steps.Count == 1)
            {
                patterns.Add(Steps[0].IsInverseStep
                    //InversePath (swap start/end)
                    ? new RDFPattern(End, Steps[0].StepProperty, Start)
                    //Path
                    : new RDFPattern(Start, Steps[0].StepProperty, End));
            }
            #endregion

            #region Multiple Properties
            else
            {
                RDFPatternMember currStart = Start;
                RDFPatternMember currEnd = new RDFVariable("__PP0");
                for (int i = 0; i < Steps.Count; i++)
                {
                    #region Alternative
                    if (Steps[i].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                    {
                        //Translate to union (item is not the last alternative)
                        if (i < Steps.Count - 1 && Steps[i + 1].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                        {
                            //Adjust start/end
                            if (!Steps.Skip(i + 1).Any(p => p.StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence))
                                currEnd = End;

                            patterns.Add(Steps[i].IsInverseStep
                                //InversePath (swap start/end)
                                ? new RDFPattern(currEnd, Steps[i].StepProperty, currStart).UnionWithNext()
                                //Path
                                : new RDFPattern(currStart, Steps[i].StepProperty, currEnd).UnionWithNext());
                        }
                        //Translate to pattern (item is the last alternative)
                        else
                        {
                            patterns.Add(Steps[i].IsInverseStep
                                //InversePath (swap start/end)
                                ? new RDFPattern(currEnd, Steps[i].StepProperty, currStart)
                                //Path
                                : new RDFPattern(currStart, Steps[i].StepProperty, currEnd));

                            //Adjust start/end
                            if (i < Steps.Count - 1)
                            {
                                currStart = currEnd;
                                if (i == Steps.Count - 2 || !Steps.Skip(i + 1).Any(p => p.StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence))
                                    currEnd = End;
                                else
                                    currEnd = new RDFVariable($"__PP{i+1}");
                            }
                        }
                    }
                    #endregion

                    #region Sequence
                    else
                    {
                        patterns.Add(Steps[i].IsInverseStep
                            //InversePath (swap start/end)
                            ? new RDFPattern(currEnd, Steps[i].StepProperty, currStart)
                            //Path
                            : new RDFPattern(currStart, Steps[i].StepProperty, currEnd));

                        //Adjust start/end
                        if (i < Steps.Count - 1)
                        {
                            currStart = currEnd;
                            currEnd = i == Steps.Count - 2 ? End : new RDFVariable($"__PP{i+1}");
                        }
                    }
                    #endregion
                }
            }
            #endregion

            return patterns;
        }
        #endregion
    }

    /// <summary>
    /// RDFPropertyPathStep represents a step of a property path
    /// </summary>
    public sealed class RDFPropertyPathStep
    {
        #region Properties
        /// <summary>
        /// Property of the step
        /// </summary>
        public RDFResource StepProperty { get; internal set; }

        /// <summary>
        /// Flavor of the step
        /// </summary>
        public RDFQueryEnums.RDFPropertyPathStepFlavors StepFlavor { get; internal set; }

        /// <summary>
        /// Flag indicating that the step has inverse evaluation
        /// </summary>
        public bool IsInverseStep { get; internal set; }

        /// <summary>
        /// Cardinality constraint of the step
        /// </summary>
        public RDFQueryEnums.RDFPropertyPathStepCardinalities StepCardinality { get; internal set; }

        /// <summary>
        /// Minimum repetitions for BoundedRange cardinality
        /// </summary>
        public int MinCardinality { get; internal set; }

        /// <summary>
        /// Maximum repetitions for BoundedRange cardinality
        /// </summary>
        public int MaxCardinality { get; internal set; }
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
            MinCardinality = 1;
            MaxCardinality = 1;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the flavor of the step
        /// </summary>
        internal RDFPropertyPathStep SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors stepFlavor)
        {
            StepFlavor = stepFlavor;
            return this;
        }

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
            MinCardinality = 0;
            MaxCardinality = 1;
            return this;
        }

        /// <summary>
        /// Sets the step cardinality to one-or-more transitive closure (SPARQL +)
        /// </summary>
        public RDFPropertyPathStep OneOrMore()
        {
            StepCardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.OneOrMore;
            MinCardinality = 1;
            MaxCardinality = -1;
            return this;
        }

        /// <summary>
        /// Sets the step cardinality to zero-or-more reflexive-transitive closure (SPARQL *)
        /// </summary>
        public RDFPropertyPathStep ZeroOrMore()
        {
            StepCardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.ZeroOrMore;
            MinCardinality = 0;
            MaxCardinality = -1;
            return this;
        }

        /// <summary>
        /// Sets the step cardinality to a bounded range (SPARQL {min,max})
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public RDFPropertyPathStep BoundedRange(int min, int max)
        {
            #region Guards
            if (min < 0)
                throw new RDFQueryException("Cannot set BoundedRange cardinality because \"min\" parameter must be >= 0.");
            if (max < min)
                throw new RDFQueryException("Cannot set BoundedRange cardinality because \"max\" parameter must be >= \"min\".");
            #endregion

            StepCardinality = RDFQueryEnums.RDFPropertyPathStepCardinalities.BoundedRange;
            MinCardinality = min;
            MaxCardinality = max;
            return this;
        }
        #endregion
    }
}