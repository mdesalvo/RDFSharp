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

using System.Text.RegularExpressions;

namespace RDFSharp;

/// <summary>
/// RDFRegex compiles and exposes all the regular expressions used across the library
/// </summary>
internal static partial class RDFRegex
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
    internal const string LongLiteralPattern = "[\n\r\t\"]";
    internal const string QueryStringStartPattern = @"^\?"; 
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
    internal const string GeoRelatesPattern = "^[012TF\\*]{9}$";
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

    /// <summary>
    /// Regex for validation of language tags (with support for direction)
    /// </summary>
    [GeneratedRegex(LangTagPattern, RegexOptions.IgnoreCase)]
    internal static partial Regex LangTagRegex();

    /// <summary>
    /// Regex for validation of language tags (without support for direction)
    /// </summary>
    [GeneratedRegex(LangTagNoDirPattern, RegexOptions.IgnoreCase)]
    internal static partial Regex LangTagNoDirRegex();

    /// <summary>
    /// Regex to detect presence of a plain literal with language tag within a given N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(EndingLangTagPattern, RegexOptions.IgnoreCase)]
    internal static partial Regex EndingLangTagRegex();

    /// <summary>
    /// Regex to detect presence of starting " in the value of a given N-Triple/N-Quad literal
    /// </summary>
    [GeneratedRegex(StartingQuotePattern)]
    internal static partial Regex StartingQuoteRegex();

    /// <summary>
    /// Regex to detect presence of ending " in the value of a given N-Triple/N-Quad literal
    /// </summary>
    [GeneratedRegex(EndingQuotePattern)]
    internal static partial Regex EndingQuoteRegex();

    /// <summary>
    /// Regex to catch literals which must be escaped as long literals in Turtle
    /// </summary>
    [GeneratedRegex(LongLiteralPattern)]
    internal static partial Regex LongLiteralRegex();

    /// <summary>
    /// Regex to catch beginning characetr of a querystring
    /// </summary>
    [GeneratedRegex(QueryStringStartPattern)]
    internal static partial Regex QueryStringStartRegex();

    /// <summary>
    /// Regex to catch 8-byte Unicode strings
    /// </summary>
    [GeneratedRegex(EightBytesUnicodePattern)]
    internal static partial Regex EightBytesUnicodeRegex();

    /// <summary>
    /// Regex for validation of prefixes
    /// </summary>
    [GeneratedRegex(PrefixPattern)]
    internal static partial Regex PrefixRegex();

    /// <summary>
    /// Regex to catch 4-byte Unicode strings
    /// </summary>
    [GeneratedRegex(FourBytesUnicodePattern)]
    internal static partial Regex FourBytesUnicodeRegex();

    /// <summary>
    /// Regex to catch xsd:hexBinary typed literals
    /// </summary>
    [GeneratedRegex(HexBinaryPattern)]
    internal static partial Regex HexBinaryRegex();

    /// <summary>
    /// Regex to catch owl:rational typed literals
    /// </summary>
    [GeneratedRegex(OWLRationalPattern)]
    internal static partial Regex OWLRationalRegex();

    /// <summary>
    /// Regex to catch time:generalDay typed literals
    /// </summary>
    [GeneratedRegex(TimeGeneralDayPattern)]
    internal static partial Regex TimeGeneralDayRegex();

    /// <summary>
    /// Regex to catch time:generalMonth typed literals
    /// </summary>
    [GeneratedRegex(TimeGeneralMonthPattern)]
    internal static partial Regex TimeGeneralMonthRegex();

    /// <summary>
    /// Regex to catch time:generalYear typed literals
    /// </summary>
    [GeneratedRegex(TimeGeneralYearPattern)]
    internal static partial Regex TimeGeneralYearRegex();

    /// <summary>
    /// Regex to validates geof:sfRelate arguments
    /// </summary>
    [GeneratedRegex(GeoRelatesPattern, RegexOptions.IgnoreCase)]
    internal static partial Regex GeoRelatesRegex();

    /// <summary>
    /// Regex to detect S->P->B form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(SPBPattern)]
    internal static partial Regex SPB();

    /// <summary>
    /// Regex to detect S->P->O form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(SPOPattern)]
    internal static partial Regex SPO();

    /// <summary>
    /// Regex to detect S->P->L(PLAIN) form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(SPLPattern)]
    internal static partial Regex SPL();

    /// <summary>
    /// Regex to detect S->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(SPLLPattern)]
    internal static partial Regex SPLL();

    /// <summary>
    /// Regex to detect S->P->L(TYPED) form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(SPLTPattern)]
    internal static partial Regex SPLT();

    /// <summary>
    /// Regex to detect B->P->B form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(BPBPattern)]
    internal static partial Regex BPB();

    /// <summary>
    /// Regex to detect B->P->O form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(BPOPattern)]
    internal static partial Regex BPO();

    /// <summary>
    /// Regex to detect B->P->L(PLAIN) form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(BPLPattern)]
    internal static partial Regex BPL();

    /// <summary>
    /// Regex to detect B->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(BPLLPattern)]
    internal static partial Regex BPLL();

    /// <summary>
    /// Regex to detect B->P->L(TYPED) form of N-Triple/N-Quad
    /// </summary>
    [GeneratedRegex(BPLTPattern)]
    internal static partial Regex BPLT();

    /// <summary>
    /// Regex to detect S->P->B->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CSPBPattern)]
    internal static partial Regex CSPB();

    /// <summary>
    /// Regex to detect S->P->O->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CSPOPattern)]
    internal static partial Regex CSPO();

    /// <summary>
    /// Regex to detect S->P->L(PLAIN)->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CSPLPattern)]
    internal static partial Regex CSPL();

    /// <summary>
    /// Regex to detect S->P->L(PLAIN LANGUAGE)->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CSPLLPattern)]
    internal static partial Regex CSPLL();

    /// <summary>
    /// Regex to detect S->P->B->L(TYPED) form of N-Quad
    /// </summary>
    [GeneratedRegex(CSPLTPattern)]
    internal static partial Regex CSPLT();

    /// <summary>
    /// Regex to detect B->P->B->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CBPBPattern)]
    internal static partial Regex CBPB();

    /// <summary>
    /// Regex to detect B->P->O->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CBPOPattern)]
    internal static partial Regex CBPO();

    /// <summary>
    /// Regex to detect B->P->L(PLAIN)->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CBPLPattern)]
    internal static partial Regex CBPL();

    /// <summary>
    /// Regex to detect B->P->L(PLAIN LANGUAGE)->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CBPLLPattern)]
    internal static partial Regex CBPLL();

    /// <summary>
    /// Regex to detect B->P->L(TYPED)->C form of N-Quad
    /// </summary>
    [GeneratedRegex(CBPLTPattern)]
    internal static partial Regex CBPLT();
}