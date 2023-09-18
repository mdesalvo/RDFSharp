﻿/*
   Copyright 2012-2023 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFComparisonFilter represents a filter applying a comparison between the given RDF terms.
    /// </summary>
    public class RDFComparisonFilter : RDFFilter
    {
        #region Properties
        /// <summary>
        /// Comparison to be applied between the given pattern members
        /// </summary>
        public RDFQueryEnums.RDFComparisonFlavors ComparisonFlavor { get; internal set; }

        /// <summary>
        /// Left Pattern Member
        /// </summary>
        public RDFPatternMember LeftMember { get; internal set; }
        internal string LeftMemberString { get; set; }

        /// <summary>
        /// Right Pattern Member
        /// </summary>
        public RDFPatternMember RightMember { get; internal set; }
        internal string RightMemberString { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a comparison filter of the given type on the given filters
        /// </summary>
        public RDFComparisonFilter(RDFQueryEnums.RDFComparisonFlavors comparisonFlavor, RDFPatternMember leftMember, RDFPatternMember rightMember)
        {
            #region Guards
            if (leftMember == null)
                throw new RDFQueryException("Cannot create RDFComparisonFilter because given \"leftMember\" parameter is null.");
            if (rightMember == null)
                throw new RDFQueryException("Cannot create RDFComparisonFilter because given \"rightMember\" parameter is null.");
            #endregion

            ComparisonFlavor = comparisonFlavor;
            LeftMember = leftMember;
            LeftMemberString = leftMember.ToString();
            RightMember = rightMember;
            RightMemberString = rightMember.ToString();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            string leftValue = RDFQueryPrinter.PrintPatternMember(LeftMember, prefixes);
            string rightValue = RDFQueryPrinter.PrintPatternMember(RightMember, prefixes);
            switch (ComparisonFlavor)
            {
                case RDFQueryEnums.RDFComparisonFlavors.LessThan:
                    return string.Concat("FILTER ( ", leftValue, " < ",  rightValue, " )");
                case RDFQueryEnums.RDFComparisonFlavors.LessOrEqualThan:
                    return string.Concat("FILTER ( ", leftValue, " <= ", rightValue, " )");
                case RDFQueryEnums.RDFComparisonFlavors.EqualTo:
                    return string.Concat("FILTER ( ", leftValue, " = ",  rightValue, " )");
                case RDFQueryEnums.RDFComparisonFlavors.NotEqualTo:
                    return string.Concat("FILTER ( ", leftValue, " != ", rightValue, " )");
                case RDFQueryEnums.RDFComparisonFlavors.GreaterOrEqualThan:
                    return string.Concat("FILTER ( ", leftValue, " >= ", rightValue, " )");
                case RDFQueryEnums.RDFComparisonFlavors.GreaterThan:
                    return string.Concat("FILTER ( ", leftValue, " > ",  rightValue, " )");
                default: throw new RDFQueryException("Cannot get string representation of unknown '" + ComparisonFlavor + "' RDFComparisonFilter.");
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = true;

            //In case LeftMember is a variable, try to get the value corresponding to its column; if column not found, the filter fails
            RDFPatternMember leftValue = LeftMember;
            if (LeftMember is RDFVariable)
            {
                if (row.Table.Columns.Contains(LeftMemberString))
                    leftValue = RDFQueryUtilities.ParseRDFPatternMember(row[LeftMemberString].ToString());
                else
                    keepRow = false;
            }

            //In case RightMember is a variable, try to get the value corresponding to its column; if column not found, the filter fails
            RDFPatternMember rightValue = RightMember;
            if (keepRow && RightMember is RDFVariable)
            {
                if (row.Table.Columns.Contains(RightMemberString))
                    rightValue = RDFQueryUtilities.ParseRDFPatternMember(row[RightMemberString].ToString());
                else
                    keepRow = false;
            }

            //Perform the comparison between leftValue and rightValue
            if (keepRow)
            {
                int comparison = RDFQueryUtilities.CompareRDFPatternMembers(leftValue, rightValue);

                //Type Error
                if (comparison == -99)
                    keepRow = false;

                //Type Correct
                else
                {
                    switch (ComparisonFlavor)
                    {
                        case RDFQueryEnums.RDFComparisonFlavors.LessThan:
                            keepRow = (comparison  < 0);
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
                            keepRow = (comparison  > 0);
                            break;
                    }
                }
            }

            //Apply the eventual negation
            if (applyNegation)
                keepRow = !keepRow;

            return keepRow;
        }
        #endregion
    }
}