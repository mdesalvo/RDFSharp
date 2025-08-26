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
using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFBNodeExpression represents a blank node generator function to be applied on a query results table.
    /// </summary>
    public sealed class RDFBNodeExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a blank node generator function
        /// </summary>
        public RDFBNodeExpression() { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the blank node generator function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>(0));
        internal override string ToString(List<RDFNamespace> prefixes)
            => "(BNODE())";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the blank node generator expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
            => new RDFResource();
        #endregion
    }
}