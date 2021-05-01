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

using System;
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFPlainLiteral represents a RDFLiteral which can be denoted by a Language.
    /// </summary>
    public class RDFPlainLiteral : RDFLiteral
    {

        #region Properties
        /// <summary>
        /// Optional language of the plain literal
        /// </summary>
        public string Language { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a plain literal without language
        /// </summary>
        public RDFPlainLiteral(string value)
        {
            this.Value = value ?? string.Empty;
            this.Language = string.Empty;
            this.SetLazyPatternMemberID();
        }

        /// <summary>
        /// Default-ctor to build a plain literal with language
        /// </summary>
        public RDFPlainLiteral(string value, string language) : this(value)
        {
            if (language != null && Regex.IsMatch(language, "^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$"))
            {
                this.Language = language.ToUpperInvariant();
                this.SetLazyPatternMemberID();
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the plain literal
        /// </summary>
        public override string ToString()
            => string.IsNullOrEmpty(this.Language) ? base.ToString() : string.Concat(base.ToString(), "@", this.Language);
        #endregion

    }

}