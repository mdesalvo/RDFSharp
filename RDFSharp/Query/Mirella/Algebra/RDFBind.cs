/*
   Copyright 2012-2024 Marco De Salvo

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

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFBind represents an operator which binds a new variable to results of an expression.
    /// </summary>
    public class RDFBind : RDFPatternGroupMember
    {
        #region Properties
        /// <summary>
        /// Expression evaluated by the bind operator
        /// </summary>
        public RDFExpression Expression { get; internal set; }

        /// <summary>
        /// Variable emitted by the bind operator
        /// </summary>
        public RDFVariable Variable { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor for building a bind operator with given expression and variable
        /// </summary>
        public RDFBind(RDFExpression expression, RDFVariable variable)
        {
            Expression = expression ?? throw new RDFQueryException("Cannot create RDFBind because given \"expression\" parameter is null");
            Variable = variable ?? throw new RDFQueryException("Cannot create RDFBind because given \"variable\" parameter is null");
            IsEvaluable = true;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the bind operator
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal string ToString(List<RDFNamespace> prefixes)
            => RDFQueryPrinter.PrintBind(this, prefixes);
        #endregion
    }
}