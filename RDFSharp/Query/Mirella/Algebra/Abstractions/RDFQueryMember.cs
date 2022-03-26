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
    /// RDFQueryMember defines an object which can be member of a query
    /// </summary>
    public class RDFQueryMember : IEquatable<RDFQueryMember>
    {
        #region Properties
        /// <summary>
        /// Unique identifier of the query member
        /// </summary>
        public long QueryMemberID
            => LazyQueryMemberID.Value;

        /// <summary>
        /// Lazy evaluation of the query member identifier
        /// </summary>
        protected Lazy<long> LazyQueryMemberID;

        /// <summary>
        /// Unique identifier of the query member (string)
        /// </summary>
        internal string QueryMemberStringID { get; set;}

        /// <summary>
        /// Flag indicating that the query member is evaluable by the engine
        /// </summary>
        internal bool IsEvaluable { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a query member
        /// </summary>
        internal RDFQueryMember()
        {
            this.QueryMemberStringID = Guid.NewGuid().ToString("N");
            this.LazyQueryMemberID = new Lazy<long>(() => RDFModelUtilities.CreateHash(this.QueryMemberStringID));
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the query member
        /// </summary>
        public override string ToString()
            => string.Empty;

        /// <summary>
        /// Performs the equality comparison between two query members
        /// </summary>
        public bool Equals(RDFQueryMember other)
            => other != null && this.QueryMemberID.Equals(other.QueryMemberID);
        #endregion
    }
}