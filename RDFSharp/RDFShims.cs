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
        internal const string EndingLangTagPattern = "@" + LangTagMask + "$";
        /*LangTagNoDir*/
        internal const string LangTagSubMaskNoDir = "(-[a-zA-Z0-9]{1,8})*";
        internal const string LangTagMaskNoDir = "[a-zA-Z]{1,8}" + LangTagSubMaskNoDir;
        internal const string LangTagNoDirPattern = "^" + LangTagMaskNoDir + "$";
        /*Strings*/
        internal const string StartingQuotePattern = @"^""";
        internal const string EndingQuotePattern = @"""$";
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
        /*NTriples*/
        internal const string SPBPattern  = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*_:[^<>\s]+\s*\.$";
        internal const string SPOPattern  = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*<[^<>\s]+>\s*\.$";
        internal const string SPLPattern  = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""\s*\.$";
        internal const string SPLLPattern = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""@" + LangTagMask + @"\s*\.$";
        internal const string SPLTPattern = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*\.$";
        internal const string BPBPattern  = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*_:[^<>\s]+\s*\.$";
        internal const string BPOPattern  = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*<[^<>\s]+>\s*\.$";
        internal const string BPLPattern  = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\s*\.$";
        internal const string BPLLPattern = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""@" + LangTagMask + @"\s*\.$";
        internal const string BPLTPattern = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*\.$";
        /*NQuads*/
        internal const string CSPBPattern  = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*_:[^<>\s]+\s*<[^<>\s]+>\s*\.$";
        internal const string CSPOPattern  = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*<[^<>\s]+>\s*<[^<>\s]+>\s*\.$";
        internal const string CSPLPattern  = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""\s*<[^<>\s]+>\s*\.$";
        internal const string CSPLLPattern = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""@" + LangTagMask + @"\s*<[^<>\s]+>\s*\.$";
        internal const string CSPLTPattern = @"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*<[^<>\s]+>\s*\.$";
        internal const string CBPBPattern  = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*_:[^<>\s]+\s*<[^<>\s]+>\s*\.$";
        internal const string CBPOPattern  = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*<[^<>\s]+>\s*<[^<>\s]+>\s*\.$";
        internal const string CBPLPattern  = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\s*<[^<>\s]+>\s*\.$";
        internal const string CBPLLPattern = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""@" + LangTagMask + @"\s*<[^<>\s]+>\s*\.$";
        internal const string CBPLTPattern = @"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*<[^<>\s]+>\s*\.$";

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
        /// Regex to detect presence of a plain literal with language tag within a given N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> EndingLangTagRegex = new Lazy<Regex>(GeneratedEndingLangTagRegex);
        [GeneratedRegex(EndingLangTagPattern, RegexOptions.IgnoreCase)]
        private static partial Regex GeneratedEndingLangTagRegex();

        /// <summary>
        /// Regex to detect presence of starting " in the value of a given N-Triple/N-Quad literal
        /// </summary>
        internal static readonly Lazy<Regex> StartingQuoteRegex = new Lazy<Regex>(GeneratedStartingQuoteRegex);
        [GeneratedRegex(StartingQuotePattern)]
        private static partial Regex GeneratedStartingQuoteRegex();

        /// <summary>
        /// Regex to detect presence of ending " in the value of a given N-Triple/N-Quad literal
        /// </summary>
        internal static readonly Lazy<Regex> EndingQuoteRegex = new Lazy<Regex>(GeneratedEndingQuoteRegex);
        [GeneratedRegex(EndingQuotePattern)]
        private static partial Regex GeneratedEndingQuoteRegex();

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

        /// <summary>
        /// Regex to detect S->P->B form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPB = new Lazy<Regex>(GeneratedSPB);
        [GeneratedRegex(SPBPattern)]
        private static partial Regex GeneratedSPB();

        /// <summary>
        /// Regex to detect S->P->O form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPO = new Lazy<Regex>(GeneratedSPO);
        [GeneratedRegex(SPOPattern)]
        private static partial Regex GeneratedSPO();

        /// <summary>
        /// Regex to detect S->P->L(PLAIN) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPL = new Lazy<Regex>(GeneratedSPL);
        [GeneratedRegex(SPLPattern)]
        private static partial Regex GeneratedSPL();

        /// <summary>
        /// Regex to detect S->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPLL = new Lazy<Regex>(GeneratedSPLL);
        [GeneratedRegex(SPLLPattern)]
        private static partial Regex GeneratedSPLL();

        /// <summary>
        /// Regex to detect S->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPLT = new Lazy<Regex>(GeneratedSPLT);
        [GeneratedRegex(SPLTPattern)]
        private static partial Regex GeneratedSPLT();

        /// <summary>
        /// Regex to detect B->P->B form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPB = new Lazy<Regex>(GeneratedBPB);
        [GeneratedRegex(BPBPattern)]
        private static partial Regex GeneratedBPB();

        /// <summary>
        /// Regex to detect B->P->O form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPO = new Lazy<Regex>(GeneratedBPO);
        [GeneratedRegex(BPOPattern)]
        private static partial Regex GeneratedBPO();

        /// <summary>
        /// Regex to detect B->P->L(PLAIN) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPL = new Lazy<Regex>(GeneratedBPL);
        [GeneratedRegex(BPLPattern)]
        private static partial Regex GeneratedBPL();

        /// <summary>
        /// Regex to detect B->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPLL = new Lazy<Regex>(GeneratedBPLL);
        [GeneratedRegex(BPLLPattern)]
        private static partial Regex GeneratedBPLL();

        /// <summary>
        /// Regex to detect B->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPLT = new Lazy<Regex>(GeneratedBPLT);
        [GeneratedRegex(BPLTPattern)]
        private static partial Regex GeneratedBPLT();

        /// <summary>
        /// Regex to detect S->P->B->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPB = new Lazy<Regex>(GeneratedCSPB);
        [GeneratedRegex(CSPBPattern)]
        private static partial Regex GeneratedCSPB();

        /// <summary>
        /// Regex to detect S->P->O->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPO = new Lazy<Regex>(GeneratedCSPO);
        [GeneratedRegex(CSPOPattern)]
        private static partial Regex GeneratedCSPO();

        /// <summary>
        /// Regex to detect S->P->L(PLAIN)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPL = new Lazy<Regex>(GeneratedCSPL);
        [GeneratedRegex(CSPLPattern)]
        private static partial Regex GeneratedCSPL();

        /// <summary>
        /// Regex to detect S->P->L(PLAIN LANGUAGE)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPLL = new Lazy<Regex>(GeneratedCSPLL);
        [GeneratedRegex(CSPLLPattern)]
        private static partial Regex GeneratedCSPLL();

        /// <summary>
        /// Regex to detect S->P->B->L(TYPED) form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPLT = new Lazy<Regex>(GeneratedCSPLT);
        [GeneratedRegex(CSPLTPattern)]
        private static partial Regex GeneratedCSPLT();

        /// <summary>
        /// Regex to detect B->P->B->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPB = new Lazy<Regex>(GeneratedCBPB);
        [GeneratedRegex(CBPBPattern)]
        private static partial Regex GeneratedCBPB();

        /// <summary>
        /// Regex to detect B->P->O->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPO = new Lazy<Regex>(GeneratedCBPO);
        [GeneratedRegex(CBPOPattern)]
        private static partial Regex GeneratedCBPO();

        /// <summary>
        /// Regex to detect B->P->L(PLAIN)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPL = new Lazy<Regex>(GeneratedCBPL);
        [GeneratedRegex(CBPLPattern)]
        private static partial Regex GeneratedCBPL();

        /// <summary>
        /// Regex to detect B->P->L(PLAIN LANGUAGE)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPLL = new Lazy<Regex>(GeneratedCBPLL);
        [GeneratedRegex(CBPLLPattern)]
        private static partial Regex GeneratedCBPLL();

        /// <summary>
        /// Regex to detect B->P->L(TYPED)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPLT = new Lazy<Regex>(GeneratedCBPLT);
        [GeneratedRegex(CBPLTPattern)]
        private static partial Regex GeneratedCBPLT();
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
        /// Regex to detect presence of a plain literal with language tag within a given N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> EndingLangTagRegex = new Lazy<Regex>(() => new Regex(EndingLangTagPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));

        /// <summary>
        /// Regex to detect presence of starting " in the value of a given N-Triple/N-Quad literal
        /// </summary>
        internal static readonly Lazy<Regex> StartingQuoteRegex = new Lazy<Regex>(() => new Regex(StartingQuotePattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect presence of ending " in the value of a given N-Triple/N-Quad literal
        /// </summary>
        internal static readonly Lazy<Regex> EndingQuoteRegex = new Lazy<Regex>(() => new Regex(EndingQuotePattern, RegexOptions.Compiled));

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

        /// <summary>
        /// Regex to detect S->P->B form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPB = new Lazy<Regex>(() => new Regex(SPBPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->O form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPO = new Lazy<Regex>(() => new Regex(SPOPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(PLAIN) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPL = new Lazy<Regex>(() => new Regex(SPLPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPLL = new Lazy<Regex>(() => new Regex(SPLLPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPLT = new Lazy<Regex>(() => new Regex(SPLTPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->B form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPB = new Lazy<Regex>(() => new Regex(BPBPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->O form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPO = new Lazy<Regex>(() => new Regex(BPOPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(PLAIN) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPL = new Lazy<Regex>(() => new Regex(BPLPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPLL = new Lazy<Regex>(() => new Regex(BPLLPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPLT = new Lazy<Regex>(() => new Regex(BPLTPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->B->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPB = new Lazy<Regex>(() => new Regex(CSPBPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->O->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPO = new Lazy<Regex>(() => new Regex(CSPOPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(PLAIN)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPL = new Lazy<Regex>(() => new Regex(CSPLPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(PLAIN LANGUAGE)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPLL = new Lazy<Regex>(() => new Regex(CSPLLPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->B->L(TYPED) form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CSPLT = new Lazy<Regex>(() => new Regex(CSPLTPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->B->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPB = new Lazy<Regex>(() => new Regex(CBPBPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->O->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPO = new Lazy<Regex>(() => new Regex(CBPOPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(PLAIN)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPL = new Lazy<Regex>(() => new Regex(CBPLPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(PLAIN LANGUAGE)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPLL = new Lazy<Regex>(() => new Regex(CBPLLPattern, RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(TYPED)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> CBPLT = new Lazy<Regex>(() => new Regex(CBPLTPattern, RegexOptions.Compiled));
#endif
    }
}