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

using System;
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFPlainLiteral represents a literal eventually decorated with a language tag.
    /// </summary>
    public class RDFPlainLiteral : RDFLiteral
    {
        #region Properties
        /// <summary>
        /// Regex for validation of language tags
        /// </summary>
        internal static readonly Lazy<Regex> LangTag = new Lazy<Regex>(() => new Regex("^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled));

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
            Value = value ?? string.Empty;
            Language = string.Empty;
        }

        /// <summary>
        /// Default-ctor to build a plain literal with language (if not well-formed, the language will be discarded)
        /// </summary>
        public RDFPlainLiteral(string value, string language) : this(value)
        {
            if (language != null && LangTag.Value.Match(language).Success)
                Language = language.ToUpperInvariant();
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the plain literal
        /// </summary>
        public override string ToString()
            => HasLanguage() ? string.Concat(base.ToString(), "@", Language) : base.ToString();
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the plain literal has a language tag
        /// </summary>
        public bool HasLanguage()
            => !string.IsNullOrEmpty(Language);
        #endregion
    }
}