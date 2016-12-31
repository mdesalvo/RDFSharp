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
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFSameTermFilter represents an equality filter between a variable and a RDF term.
    /// </summary>
    public class RDFSameTermFilter: RDFFilter {

        #region Properties
        /// <summary>
        /// Variable to be filtered
        /// </summary>
        public RDFVariable Variable { get; internal set; }

        /// <summary>
        /// RDF Term to be filtered (can be a RDFResource, a RDFLiteral or a RDFVariable)
        /// </summary>
        public RDFPatternMember RDFTerm { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build an equality filter between the given variable and the given RDF term 
        /// </summary>
        public RDFSameTermFilter(RDFVariable variable, RDFPatternMember rdfTerm) {
            if (variable         != null) {
                if (rdfTerm      != null) {
                    this.Variable = variable;
                    this.RDFTerm  = rdfTerm;
                    this.FilterID = RDFModelUtilities.CreateHash(this.ToString());
                }
                else {
                    throw new RDFQueryException("Cannot create RDFSameTermFilter because given \"rdfTerm\" parameter is null.");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFSameTermFilter because given \"variable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter 
        /// </summary>
        public override String ToString() {
            return "FILTER ( SAMETERM(" + this.Variable + ", " + RDFQueryUtilities.PrintRDFPatternMember(this.RDFTerm) + ") )";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override Boolean ApplyFilter(DataRow row, Boolean applyNegation) {
            Boolean keepRow = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(this.Variable.ToString())) {
                String varValue     = row[this.Variable.ToString()].ToString();

                //Equality comparison with a RDFTerm being RDFVariable
                if (this.RDFTerm is RDFVariable) {

                    //Check is performed only if the row contains a column named like the filter's RDFTerm
                    if (row.Table.Columns.Contains(this.RDFTerm.ToString())) {
                        var rdfTerm = row[this.RDFTerm.ToString()].ToString();
                        keepRow     = varValue.Equals(rdfTerm, StringComparison.Ordinal);
                    }

                }

                //Equality comparison with a RDFTerm being RDFResource/RDFLiteral
                else {
                    keepRow         = varValue.Equals(this.RDFTerm.ToString(), StringComparison.Ordinal);
                }

                //Apply the eventual negation
                if (applyNegation) {
                    keepRow = !keepRow;
                }
            }

            return keepRow;
        }
        #endregion

    }

}