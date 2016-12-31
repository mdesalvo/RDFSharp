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
using System.Text.RegularExpressions;
using RDFSharp.Model;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFLangMatchesFilter represents a filter on the language of a variable.
    /// </summary>
    public class RDFLangMatchesFilter: RDFFilter {

        #region Properties
        /// <summary>
        /// Variable to be filtered
        /// </summary>
        public RDFVariable Variable { get; internal set; }

        /// <summary>
        /// Language to be filtered
        /// </summary>
        public String Language   { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a filter on the given variable for the given language 
        /// </summary>
        public RDFLangMatchesFilter(RDFVariable variable, String language) {
            if (variable         != null) {
                if (language     != null) {
                    if (language == String.Empty || 
                        language == "*"          || 
                        Regex.IsMatch(language,  "^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$")) {
                            this.Variable = variable;
                            this.Language = language.ToUpperInvariant();
                            this.FilterID = RDFModelUtilities.CreateHash(this.ToString());
                    }
                    else {
                        throw new RDFQueryException("Cannot create RDFLangMatchesFilter because given \"language\" parameter (" + language + ") does not represent a valid language.");
                    }
                }
                else {
                    throw new RDFQueryException("Cannot create RDFLangMatchesFilter because given \"language\" parameter is null.");
                }
            }
            else {
                throw new RDFQueryException("Cannot create RDFLangMatchesFilter because given \"variable\" parameter is null.");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the filter 
        /// </summary>
        public override String ToString() {
            return "FILTER ( LANGMATCHES(LANG(" + this.Variable + "), \"" + this.Language + "\") )";
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
                String variableValue   = row[this.Variable.ToString()].ToString().ToUpperInvariant();
                
                //Successfull match if NO language is found in the variable
                if (this.Language     == String.Empty) {
                    keepRow            = !Regex.IsMatch(variableValue, "@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$");
                }
                else{
                    //Successfull match if ANY language is found in the variable
                    if (this.Language == "*") {
                        keepRow        = Regex.IsMatch(variableValue, "@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$");
                    }
					//Successfull match if GIVEN language is found in the variable
                    else{
                        keepRow        = Regex.IsMatch(variableValue, "@" + this.Language + "(-[a-zA-Z0-9]{1,8})*$");
                    }
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