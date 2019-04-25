/*
   Copyright 2012-2019 Marco De Salvo

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
    /// RDFPatternGroupMember defines an object which can be member of a pattern group
    /// </summary>
    public abstract class RDFPatternGroupMember : IEquatable<RDFPatternGroupMember>
    {

        #region Properties
        /// <summary>
        /// Unique representation of the pattern group member
        /// </summary>
        public Int64 PatternGroupMemberID
        {
            get { return RDFModelUtilities.CreateHash(this.ToString()); }
        }

        /// <summary>
        /// Flag indicating that the pattern group member is evaluable by the engine
        /// </summary>
        public Boolean IsEvaluable { get; internal set; }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the pattern group member
        /// </summary>
        public override String ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// Performs the equality comparison between two pattern group members
        /// </summary>
        public Boolean Equals(RDFPatternGroupMember other)
        {
            return (other != null && this.PatternGroupMemberID.Equals(other.PatternGroupMemberID));
        }
        #endregion

    }

}