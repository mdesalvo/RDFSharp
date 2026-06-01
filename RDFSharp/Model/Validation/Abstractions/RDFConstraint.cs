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
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFConstraint represents a generic SHACL constraint definition within a shape.
    /// </summary>
    public abstract class RDFConstraint : RDFResource
    {
        #region Properties
        /// <summary>
        /// Opt-in fast membership lookup (by pattern member identity) used by constraints that
        /// repeatedly test "is this value among a set?". It stays null for constraints that never
        /// opt into it (i.e. never call BuildIdentityLookup).
        /// </summary>
        protected HashSet<long> IdentityLookup { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Opts this constraint into identity-based membership lookups by indexing the given members.
        /// </summary>
        protected void BuildIdentityLookup(IEnumerable<RDFPatternMember> members)
        {
            IdentityLookup = new HashSet<long>();
            if (members != null)
            {
                foreach (RDFPatternMember member in members)
                {
                    if (member != null)
                        IdentityLookup.Add(member.PatternMemberID);
                }
            }
        }

        /// <summary>
        /// Tells if the given pattern member belongs to this constraint's identity lookup.
        /// Returns false when the constraint has not opted into the lookup, or the member is null.
        /// </summary>
        protected bool IsInIdentityLookup(RDFPatternMember member)
            => IdentityLookup != null && member != null && IdentityLookup.Contains(member.PatternMemberID);

        /// <summary>
        /// Evaluates this constraint against the given data graph
        /// </summary>
        internal abstract RDFValidationReport ValidateConstraint(RDFShapesGraph shapesGraph,
                                                                 RDFGraph dataGraph,
                                                                 RDFShape shape,
                                                                 RDFPatternMember focusNode,
                                                                 List<RDFPatternMember> valueNodes);

        /// <summary>
        /// Gets a graph representation of this constraint
        /// </summary>
        internal abstract RDFGraph ToRDFGraph(RDFShape shape);
        #endregion
    }
}