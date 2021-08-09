/*
   Copyright 2012-2021 Marco De Salvo

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

using RDFSharp.Query;
using System;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFLiteral represents a generic literal in the RDF model.
    /// </summary>
    public abstract class RDFLiteral : RDFPatternMember
    {

        #region Properties
        /// <summary>
        /// Value of the literal
        /// </summary>
        public string Value { get; internal set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the literal
        /// </summary>
        public override string ToString() => this.Value;
        #endregion

    }

}