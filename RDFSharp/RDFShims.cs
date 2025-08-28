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
using System.Text.RegularExpressions;

namespace RDFSharp
{
    /// <summary>
    /// RDFShims maintains a backward-compatible interface for features having optimized implementation targeting .NET8+
    /// </summary>
#if NET8_0_OR_GREATER
    internal static partial class RDFShims
#else
    internal static class RDFShims
#endif
    {
        /*LangTag*/
        internal const string LangTagDirection = "(--ltr|--rtl)?";
        internal const string LangTagSubMask = "(-[a-zA-Z0-9]{1,8})*" + LangTagDirection;
        internal const string LangTagMask = "[a-zA-Z]{1,8}" + LangTagSubMask;
        internal const string LangTagPattern = "^" + LangTagMask + "$";
        /*LangTagNoDir*/
        internal const string LangTagSubMaskNoDir = "(-[a-zA-Z0-9]{1,8})*";
        internal const string LangTagMaskNoDir = "[a-zA-Z]{1,8}" + LangTagSubMaskNoDir;
        internal const string LangTagNoDirPattern = "^" + LangTagMaskNoDir + "$";

#if NET8_0_OR_GREATER
        /// <summary>
        /// Regex for validation of language tags (with support for direction)
        /// </summary>
        internal static readonly Lazy<Regex> LangTagRegex = new Lazy<Regex>(() => GeneratedLangTagRegex());
        [GeneratedRegex(LangTagPattern, RegexOptions.IgnoreCase)]
        private static partial Regex GeneratedLangTagRegex();

        /// <summary>
        /// Regex for validation of language tags (without support for direction)
        /// </summary>
        internal static readonly Lazy<Regex> LangTagNoDirRegex = new Lazy<Regex>(() => GeneratedLangTagNoDirRegex());
        [GeneratedRegex(LangTagNoDirPattern, RegexOptions.IgnoreCase)]
        private static partial Regex GeneratedLangTagNoDirRegex();
#else
        /// <summary>
        /// Regex for validation of language tags (with support for direction)
        /// </summary>
        internal static readonly Lazy<Regex> LangTagRegex = new Lazy<Regex>(() => new Regex(LangTagPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));

        /// <summary>
        /// Regex for validation of language tags (without support for direction)
        /// </summary>
        internal static readonly Lazy<Regex> LangTagNoDirRegex = new Lazy<Regex>(() => new Regex(LangTagNoDirPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));
#endif
    }
}