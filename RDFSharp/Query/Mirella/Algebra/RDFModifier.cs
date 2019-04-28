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
using System.Data;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFModifier represents a modifier to be applied on a query results table.
    /// </summary>
    public abstract class RDFModifier : RDFQueryMember
    {

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier
        /// </summary>
        public override String ToString()
        {
            return base.ToString();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the modifier on the given datatable 
        /// </summary>
        internal abstract DataTable ApplyModifier(DataTable tableToFilter);

        /// <summary>
        /// Gets the string representation of the query member
        /// </summary>
        internal override String GetQueryMemberString()
        {
            return this.ToString();
        }
        #endregion

    }

}