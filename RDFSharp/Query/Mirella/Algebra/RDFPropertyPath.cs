/*
   Copyright 2012-2022 Marco De Salvo

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
using System.Linq;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFPropertyPath represents a chain of properties connecting two terms in a RDF datasource.
    /// </summary>
    public class RDFPropertyPath : RDFPatternGroupMember
    {
        #region Properties
        /// <summary>
        /// Start of the path
        /// </summary>
        public RDFPatternMember Start { get; internal set; }

        /// <summary>
        /// Steps of the path
        /// </summary>
        internal List<RDFPropertyPathStep> Steps { get; set; }

        /// <summary>
        /// Depth of the path
        /// </summary>
        internal int Depth { get; set; }

        /// <summary>
        /// End of the path
        /// </summary>
        public RDFPatternMember End { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a path between the given terms
        /// </summary>
        public RDFPropertyPath(RDFPatternMember start, RDFPatternMember end)
        {
            if (start == null)
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"start\" parameter is null.");
            if (!(start is RDFResource || start is RDFVariable))
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"start\" parameter is neither a resource or a variable.");
            if (end == null)
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"end\" parameter is null.");
            if (!(end is RDFResource || end is RDFVariable))
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"end\" parameter is neither a resource or a variable.");

            this.Start = start;
            this.End = end;
            this.Steps = new List<RDFPropertyPathStep>();
            this.Depth = 0;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the path
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintPropertyPath(this, prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given alternative steps to the path. If only one is given, it is considered sequence.
        /// </summary>
        public RDFPropertyPath AddAlternativeSteps(List<RDFPropertyPathStep> alternativeSteps)
        {
            if (alternativeSteps == null || alternativeSteps.Count == 0)
                throw new RDFQueryException("Cannot add alternative steps because the given list is null or it does not contain elements.");
            if (alternativeSteps.Any(step => step == null))
                throw new RDFQueryException("Cannot add alternative steps because the given list contains a null element.");

            #region Depth Guard
            if (this.Steps.Count == 0
                    || alternativeSteps.Count == 1
                        || this.Steps.LastOrDefault()?.StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence)
            {
                this.Depth++;
            }
            #endregion

            #region Steps Update
            if (alternativeSteps.Count == 1)
            {
                this.Steps.Add(alternativeSteps[0].SetOrdinal(this.Steps.Count)
                                                  .SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence));
            }
            else
            {
                alternativeSteps.ForEach(alternativeStep =>
                {
                    this.Steps.Add(alternativeStep.SetOrdinal(this.Steps.Count)
                                                  .SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative));
                });
            }
            #endregion

            #region Evaluability Guard
            if (this.Start is RDFVariable || this.End is RDFVariable || this.Depth > 1)
                this.IsEvaluable = true;
            #endregion
            
            return this;
        }

        /// <summary>
        /// Adds the given sequence step to the path
        /// </summary>
        public RDFPropertyPath AddSequenceStep(RDFPropertyPathStep sequenceStep)
        {
            if (sequenceStep == null)
                throw new RDFQueryException("Cannot add sequence step because it is null.");

            #region Steps Update
            this.Depth++;
            this.Steps.Add(sequenceStep.SetOrdinal(this.Steps.Count)
                                       .SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence));
            #endregion

            #region Evaluability Guard
            if (this.Start is RDFVariable || this.End is RDFVariable || this.Depth > 1)
                this.IsEvaluable = true;
            #endregion

            return this;
        }

        /// <summary>
        /// Gets the list of patterns corresponding to the path
        /// </summary>
        internal List<RDFPattern> GetPatternList()
        {
            List<RDFPattern> patterns = new List<RDFPattern>();

            #region Single Property
            if (this.Steps.Count == 1)
            {
                //InversePath (swap start/end)
                if (this.Steps[0].IsInverseStep)
                    patterns.Add(new RDFPattern(this.End, this.Steps[0].StepProperty, this.Start));
                //Path
                else
                    patterns.Add(new RDFPattern(this.Start, this.Steps[0].StepProperty, this.End));
            }
            #endregion

            #region Multiple Properties
            else
            {
                RDFPatternMember currStart = this.Start;
                RDFPatternMember currEnd = new RDFVariable("__PP0");
                for (int i = 0; i < this.Steps.Count; i++)
                {
                    #region Alternative
                    if (this.Steps[i].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                    {
                        //Translate to union (item is not the last alternative)
                        if (i < this.Steps.Count - 1 && this.Steps[i + 1].StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Alternative)
                        {
                            //Adjust start/end
                            if (!this.Steps.Any(p => p.StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence && p.StepOrdinal > i))
                                currEnd = this.End;

                            //InversePath (swap start/end)
                            if (this.Steps[i].IsInverseStep)
                                patterns.Add(new RDFPattern(currEnd, this.Steps[i].StepProperty, currStart).UnionWithNext());
                            //Path
                            else
                                patterns.Add(new RDFPattern(currStart, this.Steps[i].StepProperty, currEnd).UnionWithNext());
                        }
                        //Translate to pattern (item is the last alternative)
                        else
                        {
                            //InversePath (swap start/end)
                            if (this.Steps[i].IsInverseStep)
                                patterns.Add(new RDFPattern(currEnd, this.Steps[i].StepProperty, currStart));
                            //Path
                            else
                                patterns.Add(new RDFPattern(currStart, this.Steps[i].StepProperty, currEnd));

                            //Adjust start/end
                            if (i < this.Steps.Count - 1)
                            {
                                currStart = currEnd;
                                if (i == this.Steps.Count - 2 || !this.Steps.Any(p => p.StepFlavor == RDFQueryEnums.RDFPropertyPathStepFlavors.Sequence && p.StepOrdinal > i))
                                    currEnd = this.End;
                                else
                                    currEnd = new RDFVariable($"__PP{i+1}");
                            }
                        }
                    }
                    #endregion

                    #region Sequence
                    else
                    {
                        //InversePath (swap start/end)
                        if (this.Steps[i].IsInverseStep)
                            patterns.Add(new RDFPattern(currEnd, this.Steps[i].StepProperty, currStart));
                        //Path
                        else
                            patterns.Add(new RDFPattern(currStart, this.Steps[i].StepProperty, currEnd));

                        //Adjust start/end
                        if (i < this.Steps.Count - 1)
                        {
                            currStart = currEnd;
                            if (i == this.Steps.Count - 2)
                                currEnd = this.End;
                            else
                                currEnd = new RDFVariable($"__PP{i+1}");
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
    public class RDFPropertyPathStep
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
        {
            if (stepProperty == null)
                throw new RDFQueryException("Cannot create RDFPropertyPathStep because given \"stepProperty\" parameter is null.");
            
            this.StepProperty = stepProperty;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the flavor of the step
        /// </summary>
        internal RDFPropertyPathStep SetFlavor(RDFQueryEnums.RDFPropertyPathStepFlavors stepFlavor)
        {
            this.StepFlavor = stepFlavor;
            return this;
        }

        /// <summary>
        /// Sets the ordinal of the step
        /// </summary>
        internal RDFPropertyPathStep SetOrdinal(int stepOrdinal)
        {
            this.StepOrdinal = stepOrdinal;
            return this;
        }

        /// <summary>
        /// Sets the step as inverse
        /// </summary>
        public RDFPropertyPathStep Inverse()
        {
            this.IsInverseStep = true;
            return this;
        }
        #endregion
    }
}