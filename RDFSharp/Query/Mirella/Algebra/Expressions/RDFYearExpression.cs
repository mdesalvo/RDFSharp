﻿/*
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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFYearExpression represents a datetime year function to be applied on a query results table.
    /// </summary>
    public sealed class RDFYearExpression : RDFDateTimeExpression
    {
        #region Ctors
        /// <summary>
        /// Builds a datetime year function with given arguments
        /// </summary>
        public RDFYearExpression(RDFExpression leftArgument) : base(leftArgument) { }

        /// <summary>
        /// Builds a datetime year function with given arguments
        /// </summary>
        public RDFYearExpression(RDFVariable leftArgument) : base(leftArgument) { }
        #endregion
    }
}