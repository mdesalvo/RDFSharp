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
    /// RDFMinutesExpression represents a datetime minutes function to be applied on a query results table.
    /// </summary>
    public sealed class RDFMinutesExpression : RDFDateTimeExpression
    {
        #region Ctors
        /// <summary>
        /// Default-ctor to build a datetime minutes function with given arguments
        /// </summary>
        public RDFMinutesExpression(RDFExpression leftArgument) : base(leftArgument) { }

        /// <summary>
        /// Default-ctor to build a datetime minutes function with given arguments
        /// </summary>
        public RDFMinutesExpression(RDFVariable leftArgument) : base(leftArgument) { }
        #endregion
    }
}