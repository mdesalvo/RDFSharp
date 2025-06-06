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
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a path between the given terms
        /// </summary>
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
            => ToString(new List<RDFNamespace>());
        internal string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintPropertyPath(this, prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given alternative steps to the path. If only one is given, it is considered sequence.
        /// </summary>
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
                Depth++;

            //Collect the given steps into the property path
            if (alternativeSteps.Count == 1)
            {
                Steps.Add(alternativeSteps[0].SetOrdinal(Steps.Count)
                                                         .SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence));
            }
            else
            {
                foreach (RDFPropertyPathStep alternativeStep in alternativeSteps)
                    Steps.Add(alternativeStep.SetOrdinal(Steps.Count)
                                             .SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative));
            }

            //Update evaluability status of the property path
            if (Start is RDFVariable || End is RDFVariable || Depth > 1)
                IsEvaluable = true;

            return this;
        }

        /// <summary>
        /// Adds the given sequence step to the path
        /// </summary>
        public RDFPropertyPath AddSequenceStep(RDFPropertyPathStep sequenceStep)
        {
            #region Guards
            if (sequenceStep == null)
                throw new RDFQueryException("Cannot add sequence step because it is null.");
            #endregion

            //Update depth status of the property path
            Depth++;

            //Collect the given step into the property path
            Steps.Add(sequenceStep.SetOrdinal(Steps.Count)
                                  .SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence));

            //Update evaluability status of the property path
            if (Start is RDFVariable || End is RDFVariable || Depth > 1)
                IsEvaluable = true;

            return this;
        }

        /// <summary>
        /// Gets the list of patterns corresponding to the path
        /// </summary>
        internal List<RDFPattern> GetPatternList()
        {
            List<RDFPattern> patterns = new List<RDFPattern>();

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
                            if (!Steps.Any(p => p.StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence && p.StepOrdinal > i))
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
                                if (i == Steps.Count - 2 || !Steps.Any(p => p.StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence && p.StepOrdinal > i))
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
        /// Ordinal of the step
        /// </summary>
        internal int StepOrdinal { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a step of a property path
        /// </summary>
        public RDFPropertyPathStep(RDFResource stepProperty)
            => StepProperty = stepProperty ?? throw new RDFQueryException("Cannot create RDFPropertyPathStep because given \"stepProperty\" parameter is null.");
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
        /// Sets the ordinal of the step
        /// </summary>
        internal RDFPropertyPathStep SetOrdinal(int stepOrdinal)
        {
            StepOrdinal = stepOrdinal;
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
        #endregion
    }
}