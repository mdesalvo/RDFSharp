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

using System;
using System.Collections.Generic;
using System.Data;
using RDFSharp.Model;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFNowExpression represents a datetime now function (represented in UTC) to be applied on a query results table.
    /// </summary>
    public sealed class RDFNowExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a datetime now function
        /// </summary>
        public RDFNowExpression() { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the datetime now function
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => "(NOW())";
        #endregion

        #region Methods
        /// <summary>
        /// Applies the now expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
            => new RDFTypedLiteral(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"), RDFModelEnums.RDFDatatypes.XSD_DATETIME);
        #endregion
    }
}