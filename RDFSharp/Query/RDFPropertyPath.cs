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
    public class RDFPropertyPath: IEquatable<RDFPropertyPath> {

        #region Properties
        /// <summary>
        /// Unique representation of the path
        /// </summary>
        public Int64 PropertyPathID { get; internal set; }

        /// <summary>
        /// Start of the path
        /// </summary>
        public RDFPatternMember Start { get; internal set; }

        /// <summary>
        /// Properties of the path
        /// </summary>
        internal List<Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32>> Properties { get; set; }

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
            this.Properties  = new List<Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32>>();

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

            //PropertyPathID
            this.PropertyPathID = RDFModelUtilities.CreateHash(this.ToString());

        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the path
        /// </summary>
        public override String ToString() {
            return this.Start + " " + this.GetPathString() + " " + this.End;
        }

        /// <summary>
        /// Performs the equality comparison between two paths
        /// </summary>
        public Boolean Equals(RDFPropertyPath other) {
            return (other != null && this.PropertyPathID.Equals(other.PropertyPathID));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given alternative properties to the path 
        /// (if only one is given, it is considered sequence)
        /// </summary>
        public RDFPropertyPath AddAlternatives(List<RDFResource> props) {
            if (props != null && props.Any()) {
                if (props.Count == 1) {
                    this.Properties.Add(new Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32>(props[0], RDFQueryEnums.RDFPropertyPathFlavors.Sequence, this.Properties.Count));
                }
                else {
                    props.ForEach(prop => {
                        this.Properties.Add(new Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32>(prop, RDFQueryEnums.RDFPropertyPathFlavors.Alternative, this.Properties.Count));
                    });
                }
                this.PropertyPathID = RDFModelUtilities.CreateHash(this.ToString());
            }
            return this;
        }

        /// <summary>
        /// Adds the given sequence property to the path
        /// </summary>
        public RDFPropertyPath AddSequence(RDFResource prop) {
            if (prop != null) {
                this.Properties.Add(new Tuple<RDFResource, RDFQueryEnums.RDFPropertyPathFlavors, Int32>(prop, RDFQueryEnums.RDFPropertyPathFlavors.Sequence, this.Properties.Count));
                this.PropertyPathID = RDFModelUtilities.CreateHash(this.ToString());
            }
            return this;
        }

        /// <summary>
        /// Gets the string representation of the path
        /// </summary>
        internal String GetPathString() {
            var result = new StringBuilder();

            //Single-property path
            if (this.Properties.Count == 1) {
                result.Append(this.Properties[0].Item1.ToString());
            }

            //Multi-property path
            else {

                //Initialize printing
                Boolean openedParenthesis = false;

                //Iterate properties
                for (int i = 0; i < this.Properties.Count; i++) {

                    //Alternative: generate a union pattern
                    if (this.Properties[i].Item2 == RDFQueryEnums.RDFPropertyPathFlavors.Alternative) {
                        if (!openedParenthesis) { 
                             openedParenthesis = true;
                             result.Append("(");
                        }
                        if (i < this.Properties.Count - 1) {
                            result.Append(this.Properties[i].Item1.ToString() + (Char)this.Properties[i].Item2);
                        }
                        else {
                            result.Append(this.Properties[i].Item1.ToString());
                            result.Append(")");
                        }
                    }

                    //Sequence: generate a pattern
                    else {
                        if (openedParenthesis) {
                            result.Remove(result.Length - 1, 1);
                            openedParenthesis = false;
                            result.Append(")/");
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

            return result.ToString();
        }

        /// <summary>
        /// Gets the list of patterns corresponding to the path
        /// </summary>
        internal List<RDFPattern> GetPatternList() {
            var patterns = new List<RDFPattern>();

            #region Single Property
            if (this.Properties.Count == 1) {
                patterns.Add(new RDFPattern(this.Start, this.Properties[0].Item1, this.End).PropertyPath());
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
                            if (!this.Properties.Any(p => p.Item2 == RDFQueryEnums.RDFPropertyPathFlavors.Sequence && p.Item3 > i)) {
                                 currEnd    = this.End;
                            }
                            patterns.Add(new RDFPattern(currStart, this.Properties[i].Item1, currEnd).UnionWithNext().PropertyPath());
                        }

                        //Translate to pattern (item is the last alternative)
                        else {
                            patterns.Add(new RDFPattern(currStart, this.Properties[i].Item1, currEnd).PropertyPath());
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
                        patterns.Add(new RDFPattern(currStart, this.Properties[i].Item1, currEnd).PropertyPath());
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