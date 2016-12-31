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

namespace RDFSharp.Query {

    /// <summary>
    /// RDFComparisonFilter represents a filter applying a comparison between the given RDF terms.
    /// </summary>
    public class RDFComparisonFilter: RDFFilter {

        #region Properties
        /// <summary>
        /// Comparison to be applied between the given pattern members
        /// </summary>
        public RDFQueryEnums.RDFComparisonFlavors ComparisonFlavor { get; internal set; } 

        /// <summary>
        /// Left Pattern Member
        /// </summary>
        public RDFPatternMember LeftMember { get; internal set; }

        /// <summary>
        /// Right Pattern Member
        /// </summary>
        public RDFPatternMember RightMember { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a comparison filter of the given type on the given filters
        /// </summary>
        public RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor, RDFPatternMember leftMember, RDFPatternMember rightMember) {
            if (leftMember               != null) {
                if (rightMember          != null) {
                    this.ComparisonFlavor = comparisonFlavor;
                    this.LeftMember       = leftMember;
                    this.RightMember      = rightMember;
                    this.FilterID         = RDFModelUtilities.CreateHash(this.ToString());
                }
                else {
                    throw new RDFQueryException("Cannot create RDFComparisonFilter because given \"rightMember\" parameter is null.");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFComparisonFilter because given \"leftMember\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter 
        /// </summary>
        public override String ToString() {
            String leftValue  = RDFQueryUtilities.PrintRDFPatternMember(this.LeftMember);
            String rightValue = RDFQueryUtilities.PrintRDFPatternMember(this.RightMember);

            switch (this.ComparisonFlavor) {
                case RDFQueryEnums.RDFComparisonFlavors.LessThan:
                    return "FILTER ( " + leftValue + " < "  + rightValue + " )";
                case RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan:
                    return "FILTER ( " + leftValue + " <= " + rightValue + " )";
                case RDFQueryEnums.RDFComparisonFlavors.EqualTo:
                    return "FILTER ( " + leftValue + " = "  + rightValue + " )";
                case RDFQueryEnums.RDFComparisonFlavors.NotEqualTo:
                    return "FILTER ( " + leftValue + " != " + rightValue + " )";
                case RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan:
                    return "FILTER ( " + leftValue + " >= " + rightValue + " )";
                case RDFQueryEnums.RDFComparisonFlavors.GreaterThan:
                    return "FILTER ( " + leftValue + " > "  + rightValue + " )";
                default:
                    throw new RDFQueryException("Cannot get string representation of unknown '" + this.ComparisonFlavor  + "' RDFComparisonFilter.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow
        /// </summary>
        internal override Boolean ApplyFilter(DataRow row, Boolean applyNegation) {
            var keepRow              = true;
            var leftValue            = this.LeftMember;
            var rightValue           = this.RightMember;

            //In case LeftMember is a variable, try to get the value corresponding to its column; if column not found, the filter fails
            if (this.LeftMember     is RDFVariable) {
                if (row.Table.Columns.Contains(this.LeftMember.ToString())) {
                    leftValue        = RDFQueryUtilities.ParseRDFPatternMember(row[this.LeftMember.ToString()].ToString());
                }
                else {
                    keepRow          = false;
                }
            }

            //In case RightMember is a variable, try to get the value corresponding to its column; if column not found, the filter fails
            if (keepRow             && this.RightMember is RDFVariable) {
                if (row.Table.Columns.Contains(this.RightMember.ToString())) {
                    rightValue       = RDFQueryUtilities.ParseRDFPatternMember(row[this.RightMember.ToString()].ToString());
                }
                else {
                    keepRow          = false;
                }
            }

            //Perform the comparison between leftValue and rightValue
            if (keepRow) {
                try {
                    var comparison       = RDFQueryUtilities.CompareRDFPatternMembers(leftValue, rightValue);

                    //Type Error
                    if (comparison      == -99) {
                        keepRow          = false;
                    }

                    //Type Correct
                    else {
                        switch (this.ComparisonFlavor) {
                            case RDFQueryEnums.RDFComparisonFlavors.LessThan:
                                 keepRow = (comparison < 0);
                                 break;
                            case RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan:
                                 keepRow = (comparison <= 0);
                                 break;
                            case RDFQueryEnums.RDFComparisonFlavors.EqualTo:
                                 keepRow = (comparison == 0);
                                 break;
                            case RDFQueryEnums.RDFComparisonFlavors.NotEqualTo:
                                 keepRow = (comparison != 0);
                                 break;
                            case RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan:
                                 keepRow = (comparison >= 0);
                                 break;
                            case RDFQueryEnums.RDFComparisonFlavors.GreaterThan:
                                 keepRow = (comparison > 0);
                                 break;
                        }
                    }
                }
                catch {                    
                    keepRow = false; //Type Error
                }
            }

            //Apply the eventual negation
            if (applyNegation) {
                keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion

    }

}