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
using System.Data;
using System.Linq;
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFLimitModifier is a modifier which applies an upper-bound counter to the number of query results to be considered. 
    /// </summary>
    public class RDFLimitModifier: RDFModifier {

        #region Properties
        /// <summary>
        /// Maximum number of results taken from the query
        /// </summary>
        public Int32 Limit { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a Limit modifier on a query 
        /// </summary>
        public RDFLimitModifier(Int32 limit) {
            if (limit          >= 0) {
                this.Limit      = limit;
                this.ModifierID = RDFModelUtilities.CreateHash(this.ToString());   
            }
            else {
                throw new RDFQueryException("Cannot create RDFLimitModifier because given \"limit\" parameter (" + limit + ") is negative.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the modifier 
        /// </summary>
        public override String ToString() {
            return "LIMIT " + this.Limit;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the modifier on the given datatable 
        /// </summary>
        internal override DataTable ApplyModifier(DataTable table) {
            String tableName       = table.TableName;
            String tableSort       = table.DefaultView.Sort;
            if (table.Rows.Count  == 0 || this.Limit == 0) {
                table              = table.Clone();
            }
            else{
                table              = table.AsEnumerable().Take(this.Limit).CopyToDataTable();
            }
            table.TableName        = tableName;
            table.DefaultView.Sort = tableSort;
            return table;
        }
        #endregion

    }

}