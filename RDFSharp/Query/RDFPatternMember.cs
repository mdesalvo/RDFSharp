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

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFPatternMember defines an object which can be member of a pattern
    /// </summary>
    public abstract class RDFPatternMember: IEquatable<RDFPatternMember> {

        #region Properties
        /// <summary>
        /// Unique representation of the pattern member
        /// </summary>
        public Int64 PatternMemberID { get; internal set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the pattern member
        /// </summary>
        public override String ToString() {
            return base.ToString();
        }

        /// <summary>
        /// Performs the equality comparison between two pattern members
        /// </summary>
        public Boolean Equals(RDFPatternMember other) {
            return (other != null && this.PatternMemberID.Equals(other.PatternMemberID));
        }
        #endregion

    }

}