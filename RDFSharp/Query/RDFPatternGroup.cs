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
    /// RDFPatternGroup represents a named group of patterns having its own filters.
    /// </summary>
    public class RDFPatternGroup: IEquatable<RDFPatternGroup> {

        #region Properties
        /// <summary>
        /// Name of the pattern group, which must be unique in a query
        /// </summary>
        public String PatternGroupName { get; internal set; }

        /// <summary>
        /// Unique representation of the pattern group
        /// </summary>
        internal Int64 PatternGroupID { get; set; }

        /// <summary>
        /// Flag indicating the pattern group to be joined as Optional
        /// </summary>
        internal Boolean IsOptional { get; set; }

        /// <summary>
        /// Flag indicating the pattern group to be joined as Union
        /// </summary>
        internal Boolean JoinAsUnion { get; set; }

        /// <summary>
        /// List of members carried by the pattern group
        /// </summary>
        internal List<RDFPatternGroupMember> GroupMembers { get; set; }
    
        /// <summary>
        /// List of variables carried by the patterns of the pattern group
        /// </summary>
        internal List<RDFVariable> Variables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty named pattern group
        /// </summary>
        public RDFPatternGroup(String patternGroupName) {
            if (patternGroupName     != null && patternGroupName.Trim() != String.Empty) {
                this.PatternGroupName = patternGroupName.Trim().ToUpperInvariant();
                this.IsOptional       = false;
                this.JoinAsUnion      = false;
                this.GroupMembers     = new List<RDFPatternGroupMember>();
                this.Variables        = new List<RDFVariable>();
                this.PatternGroupID   = RDFModelUtilities.CreateHash(this.ToString());
            }
            else {
                throw new RDFQueryException("Cannot create RDFPatternGroup because given \"patternGroupName\" parameter is null or empty.");
            }
        }

        /// <summary>
        /// List-ctor to build a named pattern group with the given list of patterns 
        /// </summary>
        public RDFPatternGroup(String patternGroupName, List<RDFPattern> patterns): this(patternGroupName) {
            if (patterns != null) {
                patterns.ForEach(p => this.AddPattern(p));
            }
        }

        /// <summary>
        /// List-ctor to build a named pattern group with the given list of patterns and filters
        /// </summary>
        public RDFPatternGroup(String patternGroupName, List<RDFPattern> patterns, List<RDFFilter> filters): this(patternGroupName, patterns) {
            if (filters != null) {
                filters.ForEach(f => this.AddFilter(f));
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the patternGroup
        /// </summary>
        public override String ToString() {
            return this.ToString(0);
        }
        internal String ToString(Int32 spaceIndent) {
            String spaces = new StringBuilder().Append(' ', spaceIndent < 0 ? 0 : spaceIndent).ToString();
            
            //HEADER
            StringBuilder patternGroup  = new StringBuilder();
            if (this.IsOptional) {
                patternGroup.Append("\n  " + spaces + "OPTIONAL {");
                spaces    = spaces + "  ";
            }
            patternGroup.Append("\n  "  + spaces + "#" + this.PatternGroupName + "\n");
            patternGroup.Append(spaces  + "  {\n");

            //MEMBERS (PATTERNS, PROPERTY PATHS)
            Boolean printingUnion       = false;
            this.GroupMembers.ForEach(m => {

                //PATTERN
                if (m is RDFPattern) {

                    //Union pattern
                    if (((RDFPattern)m).JoinAsUnion) {
                        if (!m.Equals(this.GroupMembers.Last(gm => gm.Equals(m)))) {
                             //Begin a new Union block
                             printingUnion    = true;
                             patternGroup.Append(spaces + "    { " + m + " }\n" + spaces + "    UNION\n");
                        }
                        else {
                            //End the Union block
                            if (printingUnion) {
                                printingUnion = false;
                                patternGroup.Append(spaces + "    { " + m + " }\n");
                            }
                            else {
                                patternGroup.Append(spaces + "    " + m + " .\n");
                            }
                        }
                    }

                    //Intersect pattern
                    else {
                        //End the Union block
                        if (printingUnion) {
                            printingUnion     = false;
                            patternGroup.Append(spaces + "    { " + m + " }\n");
                        }
                        else {
                            patternGroup.Append(spaces + "    " + m + " .\n");
                        }
                    }
                }

                //PROPERTY PATH
                else if (m is RDFPropertyPath && !((RDFPropertyPath)m).IsEmpty()) {
                    //End the Union block
                    if (printingUnion) {
                        printingUnion         = false;
                        patternGroup.Append(spaces + "    { " + m + " }\n");
                    }
                    else {
                        patternGroup.Append(spaces + "    " + m + " .\n");
                    }
                }

            });

            //MEMBERS (FILTERS)
            this.GroupMembers.Where(m   => m is RDFFilter)
                             .ToList()
                             .ForEach(f => patternGroup.Append(spaces + "    " + f + " \n"));

            patternGroup.Append(spaces  + "  }\n");
            if (this.IsOptional) {
                patternGroup.Append(spaces + "}\n");
            }
            return patternGroup.ToString();
        }

        /// <summary>
        /// Performs the equality comparison between two pattern groups
        /// </summary>
        public Boolean Equals(RDFPatternGroup other) {
            return (other != null && this.PatternGroupID.Equals(other.PatternGroupID));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the pattern group
        /// </summary>
        public RDFPatternGroup AddPattern(RDFPattern pattern) {
            //Accept the pattern if it carries at least one variable
            if (pattern != null && pattern.Variables.Count > 0) {
                if (!this.GroupMembers.Exists(p => p is RDFPattern && p.Equals(pattern))) {
                     this.GroupMembers.Add(pattern);
                     this.PatternGroupID  = RDFModelUtilities.CreateHash(this.ToString());
                     
                     //Context
                     if (pattern.Context != null && pattern.Context is RDFVariable) {
                         if (!this.Variables.Exists(v => v.Equals(pattern.Context))) {
                              this.Variables.Add((RDFVariable)pattern.Context);
                         }
                     }
                     
                     //Subject
                     if (pattern.Subject is RDFVariable) {
                         if (!this.Variables.Exists(v => v.Equals(pattern.Subject))) {
                              this.Variables.Add((RDFVariable)pattern.Subject);
                         }
                     }
                     
                     //Predicate
                     if (pattern.Predicate is RDFVariable) {
                         if (!this.Variables.Exists(v => v.Equals(pattern.Predicate))) {
                              this.Variables.Add((RDFVariable)pattern.Predicate);
                         }
                     }
                     
                     //Object
                     if (pattern.Object is RDFVariable) {
                         if (!this.Variables.Exists(v => v.Equals(pattern.Object))) {
                              this.Variables.Add((RDFVariable)pattern.Object);
                         }
                     }

                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given property path to the pattern group
        /// </summary>
        public RDFPatternGroup AddPropertyPath(RDFPropertyPath propertyPath) {
            if (propertyPath != null) {
                if (!this.GroupMembers.Exists(p => p is RDFPropertyPath && p.Equals(propertyPath))) {
                     this.GroupMembers.Add(propertyPath);
                     this.PatternGroupID = RDFModelUtilities.CreateHash(this.ToString());
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given filter to the pattern group
        /// </summary>
        public RDFPatternGroup AddFilter(RDFFilter filter) {
            if (filter != null) {
                if (!this.GroupMembers.Exists(f => f is RDFFilter && f.Equals(filter))) {
                     this.GroupMembers.Add(filter);
                     this.PatternGroupID  = RDFModelUtilities.CreateHash(this.ToString());
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the pattern group as optional
        /// </summary>
        public RDFPatternGroup Optional() {
            this.IsOptional     = true;
            this.PatternGroupID = RDFModelUtilities.CreateHash(this.ToString());
            return this;
        }

        /// <summary>
        /// Sets the pattern group to be joined as union with the next pattern group encountered in the query
        /// </summary>
        public RDFPatternGroup UnionWithNext() {
            this.JoinAsUnion    = true;
            this.PatternGroupID = RDFModelUtilities.CreateHash(this.ToString());
            return this;
        }
        #endregion

    }

}