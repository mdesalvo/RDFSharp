﻿/*
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
        /// List of variables carried by the patterns (and binds) of the pattern group
        /// </summary>
        internal List<RDFVariable> Variables { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an empty pattern group
        /// </summary>
        public RDFPatternGroup()
        {
            IsEvaluable = true;
            IsOptional = false;
            JoinAsUnion = false;
            GroupMembers = new List<RDFPatternGroupMember>();
            Variables = new List<RDFVariable>();
        }

        /// <summary>
        /// List-ctor to build a named pattern group with the given list of patterns
        /// </summary>
        public RDFPatternGroup(List<RDFPattern> patterns) : this()
        {
            if (patterns != null)
                patterns.ForEach(p => AddPattern(p));
        }

        /// <summary>
        /// List-ctor to build a named pattern group with the given list of patterns and filters
        /// </summary>
        public RDFPatternGroup(List<RDFPattern> patterns, List<RDFFilter> filters) : this(patterns)
        {
            if (filters != null)
                filters.ForEach(f => AddFilter(f));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the patternGroup
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintPatternGroup(this, 0, false, prefixes);
        #endregion

        #region Methods
        /// <summary>
        /// Adds the given pattern to the pattern group (only if it contains at least one variable)
        /// </summary>
        public RDFPatternGroup AddPattern(RDFPattern pattern)
        {
            if (pattern != null && pattern.Variables.Count > 0 && !GetPatterns().Any(p => p.Equals(pattern)))
            {
                GroupMembers.Add(pattern);

                //Context
                if (pattern.Context is RDFVariable patternContext)
                {
                    if (!Variables.Any(v => v.Equals(patternContext)))
                        Variables.Add(patternContext);
                }

                //Subject
                if (pattern.Subject is RDFVariable patternSubject)
                {
                    if (!Variables.Any(v => v.Equals(patternSubject)))
                        Variables.Add(patternSubject);
                }

                //Predicate
                if (pattern.Predicate is RDFVariable patternPredicate)
                {
                    if (!Variables.Any(v => v.Equals(patternPredicate)))
                        Variables.Add(patternPredicate);
                }

                //Object
                if (pattern.Object is RDFVariable patternObject)
                {
                    if (!Variables.Any(v => v.Equals(patternObject)))
                        Variables.Add(patternObject);
                }
            }
            return this;
        }

        /// <summary>
        /// Adds the given property path to the pattern group
        /// </summary>
        public RDFPatternGroup AddPropertyPath(RDFPropertyPath propertyPath)
        {
            if (propertyPath != null && !GetPropertyPaths().Any(p => p.Equals(propertyPath)))
            {
                GroupMembers.Add(propertyPath);

                //Collect variables carried by the given property path
                if (propertyPath.Start is RDFVariable ppathStartVariable && !Variables.Any(v => v.Equals(ppathStartVariable)))
                    Variables.Add(ppathStartVariable);
                if (propertyPath.End is RDFVariable ppathEndVariable && !Variables.Any(v => v.Equals(ppathEndVariable)))
                    Variables.Add(ppathEndVariable);
            }
            return this;
        }

        /// <summary>
        /// Adds the given SPARQL values to the pattern group
        /// </summary>
        public RDFPatternGroup AddValues(RDFValues values)
        {
            if (values != null && !GetValues().Any(v => v.Equals(values)))
            {
                GroupMembers.Add(values);

                //Collect variables carried by the given SPARQL values
                foreach (string bindingKey in values.Bindings.Keys)
                {
                    RDFVariable valuesVariable = new RDFVariable(bindingKey);
                    if (!Variables.Any(v => v.Equals(valuesVariable)))
                        Variables.Add(valuesVariable);
                }
            }   
            return this;
        }

        /// <summary>
        /// Adds the given bind operator to the pattern group
        /// </summary>
        public RDFPatternGroup AddBind(RDFBind bind)
        {
            if (bind != null && !GetBinds().Any(b => b.Equals(bind)))
            {
                if (Variables.Any(v => v.Equals(bind.Variable)))
                    throw new RDFQueryException($"Cannot add BIND to pattern group because its variable '{bind.Variable}' already exists at this moment!");

                GroupMembers.Add(bind);

                //Collect variables carried by the given bind operator
                Variables.Add(bind.Variable);
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
                if (!GetFilters().Any(f => f.Equals(filter)))
                    GroupMembers.Add(filter);
            }
            return this;
        }

        /// <summary>
        /// Sets the pattern group to be joined as optional with the previous query member
        /// </summary>
        public RDFPatternGroup Optional()
        {
            IsOptional = true;
            return this;
        }

        /// <summary>
        /// Sets the pattern group to be joined as union with the next query member
        /// </summary>
        public RDFPatternGroup UnionWithNext()
        {
            JoinAsUnion = true;
            return this;
        }

        #region Internals
        /// <summary>
        /// Gets the group members of type: pattern
        /// </summary>
        internal IEnumerable<RDFPattern> GetPatterns()
            => GroupMembers.OfType<RDFPattern>();

        /// <summary>
        /// Gets the group members of type: property path
        /// </summary>
        internal IEnumerable<RDFPropertyPath> GetPropertyPaths()
            => GroupMembers.OfType<RDFPropertyPath>();

        /// <summary>
        /// Gets the group members of type: values
        /// </summary>
        internal IEnumerable<RDFValues> GetValues()
            => GroupMembers.OfType<RDFValues>();

        /// <summary>
        /// Gets the group members of type: bind
        /// </summary>
        internal IEnumerable<RDFBind> GetBinds()
            => GroupMembers.OfType<RDFBind>();

        /// <summary>
        /// Gets the group members of type: filter
        /// </summary>
        internal IEnumerable<RDFFilter> GetFilters()
            => GroupMembers.OfType<RDFFilter>();

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

                AddValues(clonedValues);
            }
            return this;
        }

        /// <summary>
        /// Gets the group members which can be evaluated
        /// </summary>
        internal IEnumerable<RDFPatternGroupMember> GetEvaluablePatternGroupMembers()
            => GroupMembers.Where(g => g.IsEvaluable);
        #endregion

        #endregion
    }
}