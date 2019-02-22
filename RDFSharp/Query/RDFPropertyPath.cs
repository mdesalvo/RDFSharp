/*
   Copyright 2012-2019 Marco De Salvo

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFPropertyPath represents a chain of properties connecting two terms in a RDF datasource.
    /// </summary>
    public class RDFPropertyPath: RDFPatternGroupMember {

        #region Properties
        /// <summary>
        /// Start of the path
        /// </summary>
        public RDFPatternMember Start { get; internal set; }

        /// <summary>
        /// Properties of the path
        /// </summary>
        internal List<Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32, Boolean>> Properties { get; set; }

        /// <summary>
        /// End of the path
        /// </summary>
        public RDFPatternMember End { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a path between the given terms
        /// </summary>
        public RDFPropertyPath(RDFPatternMember start, RDFPatternMember end) {

            //Start
            if (start != null) {
                if (start is RDFResource || start is RDFVariable) {
                    this.Start = start;
                }
                else {
                    throw new RDFQueryException("Cannot create RDFPropertyPath because given \"start\" parameter is neither a resource or a variable.");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"start\" parameter is null.");
            }

            //Properties
            this.Properties  = new List<Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32, Boolean>>();

            //End
            if (end != null) {
                if (end is RDFResource || end is RDFVariable) {
                    this.End = end;
                }
                else {
                    throw new RDFQueryException("Cannot create RDFPropertyPath because given \"end\" parameter is neither a resource or a variable.");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFPropertyPath because given \"end\" parameter is null.");
            }

            //IsEvaluable
            this.IsEvaluable          = false;

            //PatternGroupMemberID
            this.PatternGroupMemberID = RDFModelUtilities.CreateHash(this.ToString());

        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the path
        /// </summary>
        public override String ToString() {
            return this.Start + " " + this.GetPathString() + " " + this.End;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given alternatives to the path. If only one is given, it is considered sequence.
        /// </summary>
        public RDFPropertyPath AddAlternatives(List<Tuple<RDFResource, Boolean>> props) {
            if (props != null && props.Any()) {
                if (props.Count == 1) {
                    this.Properties.Add(new Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32, Boolean>(props[0].Item1, RDFQueryEnums.RDFPropertyPathFlavors.Sequence, this.Properties.Count, props[0].Item2));
                }
                else {
                    props.ForEach(prop => {
                        this.Properties.Add(new Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32, Boolean>(prop.Item1, RDFQueryEnums.RDFPropertyPathFlavors.Alternative, this.Properties.Count, prop.Item2));
                    });
                }
                this.IsEvaluable          = true;
                this.PatternGroupMemberID = RDFModelUtilities.CreateHash(this.ToString());
            }
            return this;
        }

        /// <summary>
        /// Adds the given sequence to the path
        /// </summary>
        public RDFPropertyPath AddSequence(Tuple<RDFResource, Boolean> prop) {
            if (prop != null) {
                this.Properties.Add(new Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32, Boolean>(prop.Item1, RDFQueryEnums.RDFPropertyPathFlavors.Sequence, this.Properties.Count, prop.Item2));
                this.IsEvaluable          = true;
                this.PatternGroupMemberID = RDFModelUtilities.CreateHash(this.ToString());
            }
            return this;
        }

        /// <summary>
        /// Gets the string representation of the path
        /// </summary>
        internal String GetPathString() {
            var result = new StringBuilder();

            #region Single Property
            if (this.Properties.Count == 1) {

                //InversePath (will swap start/end)
                if (this.Properties[0].Item4) {
                    result.Append("^");
                }

                result.Append(this.Properties[0].Item1.ToString());

            }
            #endregion

            #region Multiple Properties
            else {

                //Initialize printing
                Boolean openedParenthesis = false;

                //Iterate properties
                for (int i = 0; i < this.Properties.Count; i++) {

                    //Alternative: generate union pattern
                    if (this.Properties[i].Item2 == RDFQueryEnums.RDFPropertyPathFlavors.Alternative) {
                        if (!openedParenthesis) { 
                             openedParenthesis = true;
                             result.Append("(");
                        }

                        //InversePath (will swap start/end)
                        if (this.Properties[i].Item4) {
                            result.Append("^");
                        }

                        if (i < this.Properties.Count - 1) {
                            result.Append(this.Properties[i].Item1.ToString() + (Char)this.Properties[i].Item2);
                        }
                        else {
                            result.Append(this.Properties[i].Item1.ToString());
                            result.Append(")");
                        }
                    }

                    //Sequence: generate pattern
                    else {
                        if (openedParenthesis) {
                            result.Remove(result.Length - 1, 1);
                            openedParenthesis = false;
                            result.Append(")/");
                        }

                        //InversePath (will swap start/end)
                        if (this.Properties[i].Item4) {
                            result.Append("^");
                        }

                        if (i < this.Properties.Count - 1) {
                            result.Append(this.Properties[i].Item1.ToString() + (Char)this.Properties[i].Item2);
                        }
                        else {
                            result.Append(this.Properties[i].Item1.ToString());
                        }
                    }

                }

            }
            #endregion

            return result.ToString();
        }

        /// <summary>
        /// Gets the list of patterns corresponding to the path
        /// </summary>
        internal List<RDFPattern> GetPatternList() {
            var patterns = new List<RDFPattern>();

            #region Single Property
            if (this.Properties.Count == 1) {

                //InversePath (swap start/end)
                if (this.Properties[0].Item4) {
                    patterns.Add(new RDFPattern(this.End, this.Properties[0].Item1, this.Start));
                }

                //Path
                else {
                    patterns.Add(new RDFPattern(this.Start, this.Properties[0].Item1, this.End));
                }

            }
            #endregion

            #region Multiple Properties
            else {
                RDFPatternMember currStart  = this.Start;
                RDFPatternMember currEnd    = new RDFVariable("__PP0");
                for (int i = 0; i < this.Properties.Count; i++) {

                    #region Alternative
                    if (this.Properties[i].Item2 == RDFQueryEnums.RDFPropertyPathFlavors.Alternative) {

                        //Translate to union (item is not the last alternative)
                        if (i < this.Properties.Count - 1 && this.Properties[i + 1].Item2 == RDFQueryEnums.RDFPropertyPathFlavors.Alternative) {

                            //Adjust start/end
                            if (!this.Properties.Any(p => p.Item2 == RDFQueryEnums.RDFPropertyPathFlavors.Sequence && p.Item3 > i)) {
                                 currEnd    = this.End;
                            }

                            //InversePath (swap start/end)
                            if (this.Properties[i].Item4) {
                                patterns.Add(new RDFPattern(currEnd, this.Properties[i].Item1, currStart).UnionWithNext());
                            }

                            //Path
                            else {
                                patterns.Add(new RDFPattern(currStart, this.Properties[i].Item1, currEnd).UnionWithNext());
                            }

                        }

                        //Translate to pattern (item is the last alternative)
                        else {

                            //InversePath (swap start/end)
                            if (this.Properties[i].Item4) {
                                patterns.Add(new RDFPattern(currEnd, this.Properties[i].Item1, currStart));
                            }

                            //Path
                            else {
                                patterns.Add(new RDFPattern(currStart, this.Properties[i].Item1, currEnd));
                            }

                            //Adjust start/end
                            if (i           < this.Properties.Count - 1) {
                                currStart   = currEnd;
                                if (i      == this.Properties.Count - 2 || !this.Properties.Any(p => p.Item2 == RDFQueryEnums.RDFPropertyPathFlavors.Sequence && p.Item3 > i)) {
                                    currEnd = this.End;
                                }
                                else {
                                    currEnd = new RDFVariable("__PP" + (i + 1));
                                }
                            }

                        }

                    }
                    #endregion

                    #region Sequence
                    else {

                        //InversePath (swap start/end)
                        if (this.Properties[i].Item4) {
                            patterns.Add(new RDFPattern(currEnd, this.Properties[i].Item1, currStart));
                        }

                        //Path
                        else {
                            patterns.Add(new RDFPattern(currStart, this.Properties[i].Item1, currEnd));
                        }

                        //Adjust start/end
                        if (i               < this.Properties.Count - 1) {
                            currStart       = currEnd;
                            if (i          == this.Properties.Count - 2) {
                                currEnd     = this.End;
                            }
                            else {
                                currEnd     = new RDFVariable("__PP" + (i + 1));
                            }
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

}