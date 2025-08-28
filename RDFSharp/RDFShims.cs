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
        /*Prefix*/
        internal const string PrefixPattern = @"^[a-zA-Z0-9_\-]+$";
        /*Unicode*/
        internal const string EightBytesUnicodePattern = @"\\U([0-9A-Fa-f]{8})";
        internal const string FourBytesUnicodePattern = @"\\u([0-9A-Fa-f]{4})";
        /*Datatypes*/
        internal const string HexBinaryPattern = "^([0-9a-fA-F]{2})*$";
        internal const string OWLRationalPattern = "^(0|(-)?([1-9])+([0-9])*)(/([1-9])+([0-9])*)?$";
        internal const string TimeGeneralDayPattern = "---(0[1-9]|[1-9][0-9])(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?";
        internal const string TimeGeneralMonthPattern = "--(0[1-9]|1[0-9]|20)(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?";
        internal const string TimeGeneralYearPattern = "-?([1-9][0-9]{3,}|0[0-9]{3})(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?";

#if NET8_0_OR_GREATER
        /// <summary>
        /// Regex for validation of language tags (with support for direction)
        /// </summary>
        internal static readonly Lazy<Regex> LangTagRegex = new Lazy<Regex>(GeneratedLangTagRegex);
        [GeneratedRegex(LangTagPattern, RegexOptions.IgnoreCase)]
        private static partial Regex GeneratedLangTagRegex();

        /// <summary>
        /// Regex for validation of language tags (without support for direction)
        /// </summary>
        internal static readonly Lazy<Regex> LangTagNoDirRegex = new Lazy<Regex>(GeneratedLangTagNoDirRegex);
        [GeneratedRegex(LangTagNoDirPattern, RegexOptions.IgnoreCase)]
        private static partial Regex GeneratedLangTagNoDirRegex();

        /// <summary>
        /// Regex to catch 8-byte Unicode strings
        /// </summary>
        internal static readonly Lazy<Regex> EightBytesUnicodeRegex = new Lazy<Regex>(GeneratedEightBytesUnicodeRegex);
        [GeneratedRegex(EightBytesUnicodePattern)]
        private static partial Regex GeneratedEightBytesUnicodeRegex();

        /// <summary>
        /// Regex for validation of prefixes
        /// </summary>
        internal static readonly Lazy<Regex> PrefixRegex = new Lazy<Regex>(GeneratedPrefixRegex);
        [GeneratedRegex(PrefixPattern)]
        private static partial Regex GeneratedPrefixRegex();

        /// <summary>
        /// Regex to catch 4-byte Unicode strings
        /// </summary>
        internal static readonly Lazy<Regex> FourBytesUnicodeRegex = new Lazy<Regex>(GeneratedFourBytesUnicodeRegex);
        [GeneratedRegex(FourBytesUnicodePattern)]
        private static partial Regex GeneratedFourBytesUnicodeRegex();

        /// <summary>
        /// Regex to catch xsd:hexBinary typed literals
        /// </summary>
        internal static readonly Lazy<Regex> HexBinaryRegex = new Lazy<Regex>(GeneratedHexBinaryRegex);
        [GeneratedRegex(HexBinaryPattern)]
        private static partial Regex GeneratedHexBinaryRegex();

        /// <summary>
        /// Regex to catch owl:rational typed literals
        /// </summary>
        internal static readonly Lazy<Regex> OWLRationalRegex = new Lazy<Regex>(GeneratedOWLRationalRegex);
        [GeneratedRegex(OWLRationalPattern)]
        private static partial Regex GeneratedOWLRationalRegex();

        /// <summary>
        /// Regex to catch time:generalDay typed literals
        /// </summary>
        internal static readonly Lazy<Regex> TimeGeneralDayRegex = new Lazy<Regex>(GeneratedTimeGeneralDayRegex);
        [GeneratedRegex(TimeGeneralDayPattern)]
        private static partial Regex GeneratedTimeGeneralDayRegex();

        /// <summary>
        /// Regex to catch time:generalMonth typed literals
        /// </summary>
        internal static readonly Lazy<Regex> TimeGeneralMonthRegex = new Lazy<Regex>(GeneratedTimeGeneralMonthRegex);
        [GeneratedRegex(TimeGeneralMonthPattern)]
        private static partial Regex GeneratedTimeGeneralMonthRegex();

        /// <summary>
        /// Regex to catch time:generalYear typed literals
        /// </summary>
        internal static readonly Lazy<Regex> TimeGeneralYearRegex = new Lazy<Regex>(GeneratedTimeGeneralYearRegex);
        [GeneratedRegex(TimeGeneralYearPattern)]
        private static partial Regex GeneratedTimeGeneralYearRegex();
#else
        /// <summary>
        /// Regex for validation of language tags (with support for direction)
        /// </summary>
        internal static readonly Lazy<Regex> LangTagRegex = new Lazy<Regex>(() => new Regex(LangTagPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));

        /// <summary>
        /// Regex for validation of language tags (without support for direction)
        /// </summary>
        internal static readonly Lazy<Regex> LangTagNoDirRegex = new Lazy<Regex>(() => new Regex(LangTagNoDirPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));

        /// <summary>
        /// Regex for validation of prefixes
        /// </summary>
        internal static readonly Lazy<Regex> PrefixRegex = new Lazy<Regex>(() => new Regex(PrefixPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to catch 8-byte Unicode strings
        /// </summary>
        internal static readonly Lazy<Regex> EightBytesUnicodeRegex = new Lazy<Regex>(() => new Regex(EightBytesUnicodePattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to catch 4-byte Unicode strings
        /// </summary>
        internal static readonly Lazy<Regex> FourBytesUnicodeRegex = new Lazy<Regex>(() => new Regex(FourBytesUnicodePattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to catch xsd:hexBinary typed literals
        /// </summary>
        internal static readonly Lazy<Regex> HexBinaryRegex = new Lazy<Regex>(() => new Regex(HexBinaryPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to catch owl:rational typed literals
        /// </summary>
        internal static readonly Lazy<Regex> OWLRationalRegex = new Lazy<Regex>(() => new Regex(OWLRationalPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to catch time:generalDay typed literals
        /// </summary>
        internal static readonly Lazy<Regex> TimeGeneralDayRegex = new Lazy<Regex>(() => new Regex(TimeGeneralDayPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to catch time:generalMonth typed literals
        /// </summary>
        internal static readonly Lazy<Regex> TimeGeneralMonthRegex = new Lazy<Regex>(() => new Regex(TimeGeneralMonthPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to catch time:generalYear typed literals
        /// </summary>
        internal static readonly Lazy<Regex> TimeGeneralYearRegex = new Lazy<Regex>(() => new Regex(TimeGeneralYearPattern, RegexOptions.Compiled));
#endif
    }
}