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
    /// RDFPatternMember defines an object which can be member of a pattern
    /// </summary>
    public class RDFPatternMember : IEquatable<RDFPatternMember>
    {
        #region Properties
        /// <summary>
        /// Unique identifier of the pattern member
        /// </summary>
        public long PatternMemberID
            => LazyPatternMemberID.Value;

        /// <summary>
        /// Lazy evaluation of the pattern member identifier
        /// </summary>
        protected Lazy<long> LazyPatternMemberID;
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the pattern member
        /// </summary>
        public override string ToString()
            => string.Empty;

        /// <summary>
        /// Performs the equality comparison between two pattern members
        /// </summary>
        public bool Equals(RDFPatternMember other)
            => other != null && this.PatternMemberID.Equals(other.PatternMemberID);
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a pattern member
        /// </summary>
        internal RDFPatternMember()
            => this.LazyPatternMemberID = new Lazy<long>(() => RDFModelUtilities.CreateHash(this.ToString()));
        #endregion
    }
}