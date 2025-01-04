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

using RDFSharp.Model;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFConstantExpression represents a single-argument constant expression to be applied on a query results table.
    /// </summary>
    public class RDFConstantExpression : RDFExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a constant expression with given argument
        /// </summary>
        public RDFConstantExpression(RDFResource leftArgument)
            : base(leftArgument, null as RDFExpression) { }

        /// <summary>
        /// Default-ctor to build a constant expression with given argument
        /// </summary>
        public RDFConstantExpression(RDFLiteral leftArgument)
            : base(leftArgument, null as RDFExpression) { }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the unary expression
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder sb = new StringBuilder();

            //L
            sb.Append(LeftArgument is RDFTypedLiteral tlLeftArgument && tlLeftArgument.HasDecimalDatatype() 
                        ? tlLeftArgument.Value.ToString(CultureInfo.InvariantCulture)
                        : RDFQueryPrinter.PrintPatternMember((RDFPatternMember)LeftArgument, prefixes));

            return sb.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the constant expression on the given datarow
        /// </summary>
        internal override RDFPatternMember ApplyExpression(DataRow row)
            => (RDFPatternMember)LeftArgument;
        #endregion
    }
}