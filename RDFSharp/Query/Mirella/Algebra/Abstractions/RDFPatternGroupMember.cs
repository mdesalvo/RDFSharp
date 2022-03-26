/*
   Copyright 2012-2022 Marco De Salvo

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
using System;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFPatternGroupMember defines an object which can be member of a pattern group
    /// </summary>
    public class RDFPatternGroupMember : IEquatable<RDFPatternGroupMember>
    {
        #region Properties
        /// <summary>
        /// Unique identifier of the pattern group member
        /// </summary>
        public long PatternGroupMemberID
            => LazyPatternGroupMemberID.Value;

        /// <summary>
        /// Lazy evaluation of the pattern group member identifier
        /// </summary>
        protected Lazy<long> LazyPatternGroupMemberID;

        /// <summary>
        /// Unique identifier of the pattern group member (string)
        /// </summary>
        internal string PatternGroupMemberStringID { get; set;}

        /// <summary>
        /// Flag indicating that the pattern group member is evaluable by the engine
        /// </summary>
        internal bool IsEvaluable { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a pattern group member
        /// </summary>
        internal RDFPatternGroupMember()
        {
            this.PatternGroupMemberStringID = Guid.NewGuid().ToString("N");
            this.LazyPatternGroupMemberID = new Lazy<long>(() => RDFModelUtilities.CreateHash(this.PatternGroupMemberStringID));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the pattern group member
        /// </summary>
        public override string ToString()
            => string.Empty;

        /// <summary>
        /// Performs the equality comparison between two pattern group members
        /// </summary>
        public bool Equals(RDFPatternGroupMember other)
            => other != null && this.PatternGroupMemberID.Equals(other.PatternGroupMemberID);
        #endregion
    }
}