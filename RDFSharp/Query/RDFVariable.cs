/*
   Copyright 2012-2017 Marco De Salvo

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
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFVariable represents a named "hole" in a pattern, to be filled with values during queries.
    /// </summary>
    public class RDFVariable: RDFPatternMember {

        #region Properties
        /// <summary>
        /// Name of the variable
        /// </summary>
        public String VariableName { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a named SPARQL variable
        /// </summary>
        public RDFVariable(String variableName) {
            if (variableName != null && variableName.Trim() != String.Empty) {
                this.VariableName     = "?" + variableName.Trim().ToUpperInvariant();
                this.PatternMemberID  = RDFModelUtilities.CreateHash(this.ToString());
            }
            else {
                throw new RDFQueryException("Cannot create RDFVariable because given \"variableName\" parameter is null or empty.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the variable
        /// </summary>
        public override String ToString() {
            return this.VariableName;
        }
        #endregion

    }

}