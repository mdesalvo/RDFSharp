/*
   Copyright 2012-2025 Marco De Salvo

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

namespace RDFSharp.Model;

/// <summary>
/// RDFPlainLiteral represents a literal which can be eventually decorated with a language tag and a base direction
/// </summary>
public sealed class RDFPlainLiteral : RDFLiteral
{
    #region Statics
    /// <summary>
    /// Represents an handy plain literal for empty strings
    /// </summary>
    public static readonly RDFPlainLiteral Empty = new RDFPlainLiteral(string.Empty);
    /// <summary>
    /// Represents an handy plain literal for querying any language tags (*)
    /// </summary>
    public static readonly RDFPlainLiteral Star = new RDFPlainLiteral("*");
    #endregion

    #region Properties
    /// <summary>
    /// Optional language of the literal
    /// </summary>
    public string Language { get; }
    #endregion

    #region Ctors
    /// <summary>
    /// Builds a plain literal without language
    /// </summary>
    public RDFPlainLiteral(string value)
    {
        Value = value ?? string.Empty;
        Language = string.Empty;
    }

    /// <summary>
    /// Builds a plain literal with language (if not well-formed, the language will be discarded)
    /// </summary>
    public RDFPlainLiteral(string value, string language) : this(value)
    {
        if (language != null && RDFShims.LangTagRegex().IsMatch(language))
            Language = language.ToUpperInvariant();
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the plain literal
    /// </summary>
    public override string ToString()
        => HasLanguage() ? $"{base.ToString()}@{Language}" : base.ToString();
    #endregion

    #region Methods
    /// <summary>
    /// Checks if the plain literal has a language tag
    /// </summary>
    public bool HasLanguage()
        => !string.IsNullOrEmpty(Language);

    /// <summary>
    /// Checks if the plain literal has a language tag with base direction
    /// </summary>
    public bool HasDirection()
        => HasLanguage()
           && (Language.EndsWith("--ltr", StringComparison.OrdinalIgnoreCase)
               || Language.EndsWith("--rtl", StringComparison.OrdinalIgnoreCase));
    #endregion
}