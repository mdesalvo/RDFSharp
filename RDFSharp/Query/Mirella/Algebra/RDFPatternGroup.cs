/*
   Copyright 2012-2020 Marco De Salvo

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
    /// RDFPatternGroup represents a named group of patterns having its own filters.
    /// </summary>
    public class RDFPatternGroup : RDFQueryMember
    {

        #region Properties
        /// <summary>
        /// Name of the pattern group, which must be unique in a query
        /// </summary>
        public string PatternGroupName { get; internal set; }

        /// <summary>
        /// Flag indicating the pattern group to be joined as Optional
        /// </summary>
        internal bool IsOptional { get; set; }

        /// <summary>
        /// Flag indicating the pattern group to be joined as Union
        /// </summary>
        internal bool JoinAsUnion { get; set; }

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
        public RDFPatternGroup(string patternGroupName)
        {
            if (patternGroupName != null && patternGroupName.Trim() != string.Empty)
            {
                this.PatternGroupName = patternGroupName.Trim().ToUpperInvariant();
                this.IsEvaluable = true;
                this.IsOptional = false;
                this.JoinAsUnion = false;
                this.GroupMembers = new List<RDFPatternGroupMember>();
                this.Variables = new List<RDFVariable>();
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFPatternGroup because given \"patternGroupName\" parameter is null or empty.");
            }
        }

        /// <summary>
        /// List-ctor to build a named pattern group with the given list of patterns
        /// </summary>
        public RDFPatternGroup(string patternGroupName, List<RDFPattern> patterns) : this(patternGroupName)
        {
            if (patterns != null)
            {
                patterns.ForEach(p => this.AddPattern(p));
            }
        }

        /// <summary>
        /// List-ctor to build a named pattern group with the given list of patterns and filters
        /// </summary>
        public RDFPatternGroup(string patternGroupName, List<RDFPattern> patterns, List<RDFFilter> filters) : this(patternGroupName, patterns)
        {
            if (filters != null)
            {
                filters.ForEach(f => this.AddFilter(f));
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the patternGroup
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintPatternGroup(this, 0, false, prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the pattern group
        /// </summary>
        public RDFPatternGroup AddPattern(RDFPattern pattern)
        {
            //Accept the pattern if it carries at least one variable
            if (pattern != null && pattern.Variables.Count > 0)
            {
                if (!this.GetPatterns().Any(p => p.Equals(pattern)))
                {
                    this.GroupMembers.Add(pattern);

                    //Context
                    if (pattern.Context != null && pattern.Context is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(pattern.Context)))
                        {
                            this.Variables.Add((RDFVariable)pattern.Context);
                        }
                    }

                    //Subject
                    if (pattern.Subject is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(pattern.Subject)))
                        {
                            this.Variables.Add((RDFVariable)pattern.Subject);
                        }
                    }

                    //Predicate
                    if (pattern.Predicate is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(pattern.Predicate)))
                        {
                            this.Variables.Add((RDFVariable)pattern.Predicate);
                        }
                    }

                    //Object
                    if (pattern.Object is RDFVariable)
                    {
                        if (!this.Variables.Any(v => v.Equals(pattern.Object)))
                        {
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
        public RDFPatternGroup AddPropertyPath(RDFPropertyPath propertyPath)
        {
            if (propertyPath != null)
            {
                if (!this.GetPropertyPaths().Any(p => p.Equals(propertyPath)))
                {
                    this.GroupMembers.Add(propertyPath);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given SPARQL values to the pattern group
        /// </summary>
        public RDFPatternGroup AddValues(RDFValues values)
        {
            if (values != null)
            {
                if (!this.GetValues().Any(v => v.Equals(values)))
                {
                    this.GroupMembers.Add(values);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given filter to the pattern group
        /// </summary>
        public RDFPatternGroup AddFilter(RDFFilter filter)
        {
            if (filter != null)
            {
                if (!this.GetFilters().Any(f => f.Equals(filter)))
                {
                    this.GroupMembers.Add(filter);
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the pattern group to be joined as optional with the previous query member
        /// </summary>
        public RDFPatternGroup Optional()
        {
            this.IsOptional = true;
            return this;
        }

        /// <summary>
        /// Sets the pattern group to be joined as union with the next query member
        /// </summary>
        public RDFPatternGroup UnionWithNext()
        {
            this.JoinAsUnion = true;
            return this;
        }

        #region Internals
        /// <summary>
        /// Gets the group members of type: pattern
        /// </summary>
        internal IEnumerable<RDFPattern> GetPatterns()
            => this.GroupMembers.OfType<RDFPattern>();

        /// <summary>
        /// Gets the group members of type: property path
        /// </summary>
        internal IEnumerable<RDFPropertyPath> GetPropertyPaths()
            => this.GroupMembers.OfType<RDFPropertyPath>();

        /// <summary>
        /// Gets the group members of type: values
        /// </summary>
        internal IEnumerable<RDFValues> GetValues()
            => this.GroupMembers.OfType<RDFValues>();

        /// <summary>
        /// Gets the group members of type: filter
        /// </summary>
        internal IEnumerable<RDFFilter> GetFilters()
            => this.GroupMembers.OfType<RDFFilter>();

        /// <summary>
        /// Adds the given injected SPARQL values to the pattern group
        /// </summary>
        internal RDFPatternGroup AddInjectedValues(RDFValues values)
        {
            if (values != null)
            {
                //Clone the SPARQL values and set as injected
                RDFValues clonedValues = new RDFValues()
                {
                    Bindings = values.Bindings,
                    IsEvaluable = values.IsEvaluable,
                    IsInjected = true
                };

                this.AddValues(clonedValues);
            }
            return this;
        }

        /// <summary>
        /// Gets the group members which can be evaluated
        /// </summary>
        internal IEnumerable<RDFPatternGroupMember> GetEvaluablePatternGroupMembers()
            => this.GroupMembers.Where(g => g.IsEvaluable);

        /// <summary>
        /// Gets the string representation of the query member
        /// </summary>
        internal override string GetQueryMemberString()
            => this.PatternGroupName;
        #endregion

        #endregion

    }

}