/*
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
using System;
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
        /// Name of the variable to be filtered
        /// </summary>
        public string VariableName { get; internal set; }

        /// <summary>
        /// Language to be filtered
        /// </summary>
        public string Language { get; internal set; }

        /// <summary>
        /// Regex to intercept values having any language tag
        /// </summary>
        internal static readonly Lazy<Regex> AnyLanguageRegex = new Lazy<Regex>(() => new Regex("@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

        /// <summary>
        /// Regex to intercept values having specific language tag
        /// </summary>
        internal Regex ExactLanguageRegex { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable for the given language
        /// </summary>
        public RDFLangMatchesFilter(RDFVariable variable, string language)
        {
            #region Guards
            if (variable == null)
                throw new RDFQueryException("Cannot create RDFLangMatchesFilter because given \"variable\" parameter is null.");
            #endregion

            bool acceptsNoneOrAnyLanguageTag = (string.IsNullOrEmpty(language) || language == "*");
            if (acceptsNoneOrAnyLanguageTag || RDFPlainLiteral.LangTag.Value.Match(language).Success)
            {
                VariableName = variable.ToString();
                Language = language?.ToUpperInvariant() ?? string.Empty;
                if (!acceptsNoneOrAnyLanguageTag)
                    ExactLanguageRegex = new Regex($"@{Language}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            else
                throw new RDFQueryException("Cannot create RDFLangMatchesFilter because given \"language\" parameter (" + language + ") does not represent an acceptable language.");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter
        /// </summary>
        public override string ToString()
            => ToString(new List<RDFNamespace>());
        internal override string ToString(List<RDFNamespace> prefixes)
            => string.Concat("FILTER ( LANGMATCHES(LANG(", VariableName, "), \"", Language, "\") )");
        #endregion

        #region Methods
        /// <summary>
        /// Applies the filter on the column corresponding to the variable in the given datarow
        /// </summary>
        internal override bool ApplyFilter(DataRow row, bool applyNegation)
        {
            bool keepRow = true;

            //Check is performed only if the row contains a column named like the filter's variable
            if (row.Table.Columns.Contains(VariableName))
            {
                string variableValue = row[VariableName].ToString();

                switch (Language)
                {
                    //NO language is acceptable in the variable
                    case "":
                        keepRow = !AnyLanguageRegex.Value.IsMatch(variableValue);
                        break;
                    
                    //ANY language is acceptable in the variable
                    case "*":
                        keepRow = AnyLanguageRegex.Value.IsMatch(variableValue);
                        break;

                    //GIVEN language is acceptable in the variable
                    default:
                        keepRow = ExactLanguageRegex.IsMatch(variableValue);
                        break;
                }

                //Apply the eventual negation
                if (applyNegation)
                    keepRow = !keepRow;
            }

            return keepRow;
        }
        #endregion
    }
}