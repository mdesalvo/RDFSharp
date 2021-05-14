/*
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
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFLangMatchesFilter represents a filter on the language of a variable.
    /// </summary>
    public class RDFLangMatchesFilter : RDFFilter
    {

        #region Properties
        /// <summary>
        /// Variable to be filtered
        /// </summary>
        public RDFVariable Variable { get; internal set; }

        /// <summary>
        /// Language to be filtered
        /// </summary>
        public string Language { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable for the given language
        /// </summary>
        public RDFLangMatchesFilter(RDFVariable variable, string language)
        {
            if (variable != null)
            {
                if (language != null)
                {
                    if (language == string.Empty || language == "*" || RDFPlainLiteral.LangTag.Match(language).Success)
                    {
                        this.Variable = variable;
                        this.Language = language.ToUpperInvariant();
                    }
                    else
                    {
                        throw new RDFQueryException("Cannot create RDFLangMatchesFilter because given \"language\" parameter (" + language + ") does not represent a valid language.");
                    }
                }
                else
                {
                    throw new RDFQueryException("Cannot create RDFLangMatchesFilter because given \"language\" parameter is null.");
                }
            }
            else
            {
                throw new RDFQueryException("Cannot create RDFLangMatchesFilter because given \"variable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => this.ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat("FILTER ( LANGMATCHES(LANG(", this.Variable, "), \"", this.Language, "\") )");
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
                string variableValue = row[this.Variable.ToString()].ToString();

                //NO language is found in the variable
                if (this.Language == string.Empty)
                    keepRow = !Regex.IsMatch(variableValue, "@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                //ANY language is found in the variable
                else if (this.Language == "*")
                    keepRow = Regex.IsMatch(variableValue, "@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                //GIVEN language is found in the variable
                else
                    keepRow = Regex.IsMatch(variableValue, string.Concat("@", this.Language, "(-[a-zA-Z0-9]{1,8})*$"), RegexOptions.IgnoreCase);

                //Apply the eventual negation
                if (applyNegation)
                    keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion

    }

}