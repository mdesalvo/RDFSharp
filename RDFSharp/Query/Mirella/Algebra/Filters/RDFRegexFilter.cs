﻿/*
   Copyright 2012-2020 Marco De Salvo

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
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFRegexFilter represents a filter applying a regular expression on the values of a variable.
    /// </summary>
    public class RDFRegexFilter : RDFFilter
    {

        #region Properties
        /// <summary>
        /// Variable to be filtered
        /// </summary>
        public RDFVariable Variable { get; internal set; }

        /// <summary>
        /// Regular Expression to be filtered
        /// </summary>
        public Regex RegEx { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable for the given regular expression
        /// </summary>
        public RDFRegexFilter(RDFVariable variable, Regex regex)
        {
            if (variable != null)
            {
                if (regex != null)
                {
                    this.Variable = variable;
                    this.RegEx = regex;
                }
                else
                {
                    throw new RDFQueryException("Cannot create RDFRegexFilter because given \"regex\" parameter is null.");
                }
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFRegexFilter because given \"variable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
        {
            return this.ToString(new List<RDFNamespace>());
        }
        internal override string ToString(List<RDFNamespace> prefixes)
        {
            StringBuilder regexFlags = new StringBuilder();
            if (this.RegEx.Options.HasFlag(RegexOptions.IgnoreCase))
            {
                regexFlags.Append("i");
            }
            if (this.RegEx.Options.HasFlag(RegexOptions.Singleline))
            {
                regexFlags.Append("s");
            }
            if (this.RegEx.Options.HasFlag(RegexOptions.Multiline))
            {
                regexFlags.Append("m");
            }
            if (this.RegEx.Options.HasFlag(RegexOptions.IgnorePatternWhitespace))
            {
                regexFlags.Append("x");
            }
            if (regexFlags.ToString() != string.Empty)
            {
                return "FILTER ( REGEX(STR(" + this.Variable + "), \"" + this.RegEx + "\", \"" + regexFlags + "\") )";
            }
            return "FILTER ( REGEX(STR(" + this.Variable + "), \"" + this.RegEx + "\") )";
        }
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(this.Variable.ToString()))
            {
                var varValue = row[this.Variable.ToString()].ToString();

                //Successfull match if the regular expression is satisfied by the variable
                keepRow = this.RegEx.IsMatch(varValue);

                //Apply the eventual negation
                if (applyNegation)
                {
                    keepRow = !keepRow;
                }
            }

            return keepRow;
        }
        #endregion

    }

}