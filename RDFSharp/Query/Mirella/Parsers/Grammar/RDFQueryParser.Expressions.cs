/*
   Copyright 2012-2026 Marco De Salvo

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
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// Built-in-call dispatch and lexer helpers for the FILTER/expression half of the SPARQL parser (phase F6).
    /// <see cref="ParseBuiltInCall"/> maps each SPARQL 1.1 built-in keyword onto its RDFSharp expression class; the
    /// helpers below add the multi-character operator and keyword lexing the expression grammar needs on top of the
    /// single-character primitives already defined in the core parser.
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region BuiltInCall
        /// <summary>
        /// Parses a SPARQL <c>BuiltInCall</c> whose (upper-cased) name is <paramref name="builtInName"/> and whose
        /// argument list begins at the current reader position. Returns the corresponding <see cref="RDFExpression"/>.
        /// <para>
        /// A handful of standard built-ins have no expression class in the engine (IRI/URI, STRBEFORE/STRAFTER,
        /// TIMEZONE/TZ) and are rejected with a precise "not supported yet" message. EXISTS/NOT EXISTS are NOT handled
        /// here: they are FILTER-level filters parsed by the skeleton, never value-expressions.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When the built-in is unknown/unsupported or its arguments are malformed.</exception>
        private static RDFExpression ParseBuiltInCall(RDFQueryParserContext parserContext, string builtInName)
        {
            //Parse the full parenthesised argument list once, as expressions; the few built-ins that need literal
            //arguments (REGEX/REPLACE/SUBSTR/STRLANGDIR) extract them from the parsed constant expressions below.
            List<RDFExpression> arguments = ParseExpressionArgumentList(parserContext);

            switch (builtInName)
            {
                #region Nullary
                case "NOW":     RequireArgumentCount(parserContext, arguments, 0, builtInName); return new RDFNowExpression();
                case "RAND":    RequireArgumentCount(parserContext, arguments, 0, builtInName); return new RDFRandExpression();
                case "UUID":    RequireArgumentCount(parserContext, arguments, 0, builtInName); return new RDFUUIDExpression();
                case "STRUUID": RequireArgumentCount(parserContext, arguments, 0, builtInName); return new RDFStrUUIDExpression();
                case "BNODE":   RequireArgumentCount(parserContext, arguments, 0, builtInName); return new RDFBNodeExpression();
                #endregion

                #region Unary
                case "STR":            return new RDFStrExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "LANG":           return new RDFLangExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "DATATYPE":       return new RDFDatatypeExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "BOUND":          return new RDFBoundExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "ABS":            return new RDFAbsExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "CEIL":           return new RDFCeilExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "FLOOR":          return new RDFFloorExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "ROUND":          return new RDFRoundExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "STRLEN":         return new RDFStrLenExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "UCASE":          return new RDFUpperCaseExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "LCASE":          return new RDFLowerCaseExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "ENCODE_FOR_URI": return new RDFEncodeForURIExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "ISIRI":
                case "ISURI":          return new RDFIsUriExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "ISBLANK":        return new RDFIsBlankExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "ISLITERAL":      return new RDFIsLiteralExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "ISNUMERIC":      return new RDFIsNumericExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "MD5":            return new RDFMD5Expression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "SHA1":           return new RDFSHA1Expression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "SHA256":         return new RDFSHA256Expression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "SHA384":         return new RDFSHA384Expression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "SHA512":         return new RDFSHA512Expression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "YEAR":           return new RDFYearExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "MONTH":          return new RDFMonthExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "DAY":            return new RDFDayExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "HOURS":          return new RDFHoursExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "MINUTES":        return new RDFMinutesExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "SECONDS":        return new RDFSecondsExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "LANGDIR":        return new RDFLangDirExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "HASLANG":        return new RDFHasLangExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                case "HASLANGDIR":     return new RDFHasLangDirExpression(RequireSingleArgument(parserContext, arguments, builtInName));
                #endregion

                #region Binary
                case "CONTAINS":  RequireArgumentCount(parserContext, arguments, 2, builtInName); return new RDFContainsExpression(arguments[0], arguments[1]);
                case "STRSTARTS": RequireArgumentCount(parserContext, arguments, 2, builtInName); return new RDFStrStartsExpression(arguments[0], arguments[1]);
                case "STRENDS":   RequireArgumentCount(parserContext, arguments, 2, builtInName); return new RDFStrEndsExpression(arguments[0], arguments[1]);
                case "STRDT":     RequireArgumentCount(parserContext, arguments, 2, builtInName); return new RDFStrDtExpression(arguments[0], arguments[1]);
                case "STRLANG":   RequireArgumentCount(parserContext, arguments, 2, builtInName); return new RDFStrLangExpression(arguments[0], arguments[1]);
                case "SAMETERM":  RequireArgumentCount(parserContext, arguments, 2, builtInName); return new RDFSameTermExpression(arguments[0], arguments[1]);
                case "LANGMATCHES": return BuildLangMatchesExpression(parserContext, arguments);
                #endregion

                #region Variadic (folded into the engine's binary expression nodes)
                case "CONCAT":    return FoldConcatExpression(parserContext, arguments);
                case "COALESCE":  return FoldCoalesceExpression(parserContext, arguments);
                #endregion

                #region Ternary / argument-shaped
                case "IF":         RequireArgumentCount(parserContext, arguments, 3, builtInName); return new RDFIfExpression(arguments[0], arguments[1], arguments[2]);
                case "STRLANGDIR": return BuildStrLangDirExpression(parserContext, arguments);
                case "REGEX":      return BuildRegexExpression(parserContext, arguments);
                case "REPLACE":    return BuildReplaceExpression(parserContext, arguments);
                case "SUBSTR":     return BuildSubstringExpression(parserContext, arguments);
                #endregion

                #region Unsupported standard built-ins (no expression class in the engine)
                case "IRI":
                case "URI":
                case "STRBEFORE":
                case "STRAFTER":
                case "TIMEZONE":
                case "TZ":
                    throw new RDFQueryException("Cannot parse SPARQL built-in '" + builtInName + "': it has no corresponding expression in the engine and is not supported yet " + GetCoordinates(parserContext));
                #endregion

                default:
                    throw new RDFQueryException("Cannot parse SPARQL expression: unknown built-in function '" + builtInName + "' " + GetCoordinates(parserContext));
            }
        }

        /// <summary>
        /// Builds a <see cref="RDFLangMatchesExpression"/> from <c>LANGMATCHES(langTag, langRange)</c>. The engine's
        /// expression applies <c>LANG()</c> to its left argument internally, so when the author wrote the idiomatic
        /// <c>LANGMATCHES(LANG(?x), "…")</c> the parser unwraps the <c>LANG(?x)</c> and passes its inner operand.
        /// </summary>
        private static RDFExpression BuildLangMatchesExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            RequireArgumentCount(parserContext, arguments, 2, "LANGMATCHES");

            //Unwrap a leading LANG(...) so the model's internal LANG application is not applied twice
            RDFExpression languageTagArgument = arguments[0] is RDFLangExpression langExpression
                ? (RDFExpression)langExpression.LeftArgument
                : arguments[0];

            return new RDFLangMatchesExpression(languageTagArgument, arguments[1]);
        }

        /// <summary>
        /// Builds a <see cref="RDFStrLangDirExpression"/> from <c>STRLANGDIR(str, lang, dir)</c>, mapping the third
        /// argument (the language-direction string literal <c>"ltr"</c> / <c>"rtl"</c>) to its enum value.
        /// </summary>
        private static RDFExpression BuildStrLangDirExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            RequireArgumentCount(parserContext, arguments, 3, "STRLANGDIR");

            string directionText = RequireLiteralString(parserContext, arguments[2], "STRLANGDIR direction");
            RDFQueryEnums.RDFLanguageDirections direction;
            switch (directionText.ToLowerInvariant())
            {
                case "ltr": direction = RDFQueryEnums.RDFLanguageDirections.LTR; break;
                case "rtl": direction = RDFQueryEnums.RDFLanguageDirections.RTL; break;
                default:
                    throw new RDFQueryException("Cannot parse SPARQL 'STRLANGDIR': the direction must be \"ltr\" or \"rtl\" " + GetCoordinates(parserContext));
            }

            return new RDFStrLangDirExpression(arguments[0], arguments[1], direction);
        }

        /// <summary>
        /// Builds a <see cref="RDFRegexExpression"/> from <c>REGEX(text, pattern [, flags])</c>. The engine compiles
        /// the regular expression at build time, so <c>pattern</c> and <c>flags</c> must be constant string literals.
        /// </summary>
        private static RDFExpression BuildRegexExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            if (arguments.Count < 2 || arguments.Count > 3)
                throw new RDFQueryException("Cannot parse SPARQL 'REGEX': expected 2 or 3 arguments " + GetCoordinates(parserContext));

            string pattern = RequireLiteralString(parserContext, arguments[1], "REGEX pattern");
            string flags = arguments.Count == 3 ? RequireLiteralString(parserContext, arguments[2], "REGEX flags") : string.Empty;

            return new RDFRegexExpression(arguments[0], BuildRegex(parserContext, pattern, flags));
        }

        /// <summary>
        /// Builds a <see cref="RDFReplaceExpression"/> from <c>REPLACE(text, pattern, replacement [, flags])</c>.
        /// As with REGEX, <c>pattern</c> and <c>flags</c> must be constant string literals.
        /// </summary>
        private static RDFExpression BuildReplaceExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            if (arguments.Count < 3 || arguments.Count > 4)
                throw new RDFQueryException("Cannot parse SPARQL 'REPLACE': expected 3 or 4 arguments " + GetCoordinates(parserContext));

            string pattern = RequireLiteralString(parserContext, arguments[1], "REPLACE pattern");
            string flags = arguments.Count == 4 ? RequireLiteralString(parserContext, arguments[3], "REPLACE flags") : string.Empty;

            return new RDFReplaceExpression(arguments[0], arguments[2], BuildRegex(parserContext, pattern, flags));
        }

        /// <summary>
        /// Builds a <see cref="RDFSubstringExpression"/> from <c>SUBSTR(text, start [, length])</c>. The engine stores
        /// <c>start</c>/<c>length</c> as fixed integers, so both must be constant integer literals.
        /// </summary>
        private static RDFExpression BuildSubstringExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            if (arguments.Count < 2 || arguments.Count > 3)
                throw new RDFQueryException("Cannot parse SPARQL 'SUBSTR': expected 2 or 3 arguments " + GetCoordinates(parserContext));

            int start = RequireIntegerLiteral(parserContext, arguments[1], "SUBSTR start");
            int? length = arguments.Count == 3 ? RequireIntegerLiteral(parserContext, arguments[2], "SUBSTR length") : (int?)null;

            return new RDFSubstringExpression(arguments[0], start, length);
        }

        /// <summary>
        /// Folds the (≥2) arguments of a SPARQL variadic <c>CONCAT</c> into the engine's binary
        /// <see cref="RDFConcatExpression"/> nodes, left-associatively.
        /// </summary>
        private static RDFExpression FoldConcatExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            if (arguments.Count < 2)
                throw new RDFQueryException("Cannot parse SPARQL 'CONCAT': the engine requires at least 2 arguments " + GetCoordinates(parserContext));

            RDFExpression foldedExpression = arguments[0];
            for (int argumentIndex = 1; argumentIndex < arguments.Count; argumentIndex++)
                foldedExpression = new RDFConcatExpression(foldedExpression, arguments[argumentIndex]);
            return foldedExpression;
        }

        /// <summary>
        /// Folds the (≥2) arguments of a SPARQL variadic <c>COALESCE</c> into the engine's binary
        /// <see cref="RDFCoalesceExpression"/> nodes, left-associatively (first non-error/non-unbound value wins).
        /// </summary>
        private static RDFExpression FoldCoalesceExpression(RDFQueryParserContext parserContext, List<RDFExpression> arguments)
        {
            if (arguments.Count < 2)
                throw new RDFQueryException("Cannot parse SPARQL 'COALESCE': the engine requires at least 2 arguments " + GetCoordinates(parserContext));

            RDFExpression foldedExpression = arguments[0];
            for (int argumentIndex = 1; argumentIndex < arguments.Count; argumentIndex++)
                foldedExpression = new RDFCoalesceExpression(foldedExpression, arguments[argumentIndex]);
            return foldedExpression;
        }

        /// <summary>
        /// Compiles a SPARQL regular expression from its pattern and flag string, mapping the four SPARQL flag
        /// characters (<c>i</c>, <c>s</c>, <c>m</c>, <c>x</c>) onto the matching <see cref="RegexOptions"/>.
        /// </summary>
        private static Regex BuildRegex(RDFQueryParserContext parserContext, string pattern, string flags)
        {
            RegexOptions regexOptions = RegexOptions.None;
            foreach (char flagCharacter in flags)
            {
                switch (flagCharacter)
                {
                    case 'i': regexOptions |= RegexOptions.IgnoreCase;            break;
                    case 's': regexOptions |= RegexOptions.Singleline;            break;
                    case 'm': regexOptions |= RegexOptions.Multiline;             break;
                    case 'x': regexOptions |= RegexOptions.IgnorePatternWhitespace; break;
                    default:
                        throw new RDFQueryException("Cannot parse SPARQL regex flags: unknown flag '" + flagCharacter + "' (allowed: i, s, m, x) " + GetCoordinates(parserContext));
                }
            }

            try
            {
                return new Regex(pattern, regexOptions);
            }
            catch (ArgumentException regexCompilationException)
            {
                throw new RDFQueryException("Cannot parse SPARQL regex: invalid pattern '" + pattern + "' " + GetCoordinates(parserContext) + ": " + regexCompilationException.Message, regexCompilationException);
            }
        }
        #endregion

        #region BuiltInCall.Helpers
        /// <summary>
        /// Returns the single argument of a unary built-in, raising a precise error when the count is not exactly one.
        /// </summary>
        private static RDFExpression RequireSingleArgument(RDFQueryParserContext parserContext, List<RDFExpression> arguments, string builtInName)
        {
            RequireArgumentCount(parserContext, arguments, 1, builtInName);
            return arguments[0];
        }

        /// <summary>
        /// Validates that a built-in received exactly <paramref name="expectedCount"/> arguments.
        /// </summary>
        private static void RequireArgumentCount(RDFQueryParserContext parserContext, List<RDFExpression> arguments, int expectedCount, string builtInName)
        {
            if (arguments.Count != expectedCount)
                throw new RDFQueryException("Cannot parse SPARQL built-in '" + builtInName + "': expected " + expectedCount + " argument(s) but found " + arguments.Count + " " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Extracts the lexical value of a constant string-literal argument (used for REGEX/REPLACE patterns and
        /// flags, and the STRLANGDIR direction), rejecting any argument that is not a plain literal constant.
        /// </summary>
        private static string RequireLiteralString(RDFQueryParserContext parserContext, RDFExpression argument, string argumentLabel)
        {
            if (argument is RDFConstantExpression constantExpression && constantExpression.LeftArgument is RDFLiteral literal)
                return literal.Value;
            throw new RDFQueryException("Cannot parse SPARQL " + argumentLabel + ": expected a constant string literal " + GetCoordinates(parserContext));
        }

        /// <summary>
        /// Extracts the integer value of a constant numeric-literal argument (used for SUBSTR start/length), rejecting
        /// any argument that is not a constant integer literal.
        /// </summary>
        private static int RequireIntegerLiteral(RDFQueryParserContext parserContext, RDFExpression argument, string argumentLabel)
        {
            if (argument is RDFConstantExpression constantExpression
                 && constantExpression.LeftArgument is RDFTypedLiteral typedLiteral
                 && int.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int integerValue))
                return integerValue;
            throw new RDFQueryException("Cannot parse SPARQL " + argumentLabel + ": expected a constant integer literal " + GetCoordinates(parserContext));
        }
        #endregion

        #region Lexer.Expression
        /// <summary>
        /// Wraps a concrete RDF term (parsed by the shared term-reader) into the matching constant expression:
        /// an <see cref="RDFResource"/> or an <see cref="RDFLiteral"/> become an <see cref="RDFConstantExpression"/>.
        /// </summary>
        private static RDFExpression MakeConstantExpression(RDFQueryParserContext parserContext, RDFPatternMember term)
        {
            switch (term)
            {
                case RDFResource resourceTerm: return new RDFConstantExpression(resourceTerm);
                case RDFLiteral literalTerm:   return new RDFConstantExpression(literalTerm);
                default:
                    throw new RDFQueryException("Cannot parse SPARQL expression: unexpected term '" + term + "' " + GetCoordinates(parserContext));
            }
        }

        /// <summary>
        /// Builds the integer-zero constant expression used as the left operand of a desugared unary minus
        /// (<c>-x</c> is modelled as <c>0 - x</c>).
        /// </summary>
        private static RDFExpression MakeZeroConstantExpression()
            => new RDFConstantExpression(new RDFTypedLiteral("0", RDFModelEnums.RDFDatatypes.XSD_INTEGER));

        /// <summary>
        /// Skips whitespace/comments and, if the upcoming characters exactly spell <paramref name="operatorText"/>,
        /// consumes them and returns <c>true</c>; otherwise restores the reader and returns <c>false</c>. Used for the
        /// multi-character expression operators (<c>||</c>, <c>&amp;&amp;</c>, <c>&lt;=</c>, <c>&gt;=</c>, <c>!=</c>)
        /// and their single-character forms (<c>&lt;</c>, <c>&gt;</c>, <c>=</c>).
        /// </summary>
        private static bool TryConsumeOperator(RDFQueryParserContext parserContext, string operatorText)
        {
            SkipWhitespace(parserContext);

            //Read exactly operatorText.Length code points, remembering them so a partial match can be fully rewound
            List<int> consumedCodePoints = new List<int>(operatorText.Length);
            bool matches = true;
            foreach (char expectedCharacter in operatorText)
            {
                int actualCodePoint = ReadCodePoint(parserContext);
                consumedCodePoints.Add(actualCodePoint);
                if (actualCodePoint != expectedCharacter)
                {
                    matches = false;
                    break;
                }
            }

            //On a full match the operator stays consumed; on a mismatch push everything we read back, in reverse order
            if (!matches)
            {
                for (int rewindIndex = consumedCodePoints.Count - 1; rewindIndex >= 0; rewindIndex--)
                    UnreadCodePoint(parserContext, consumedCodePoints[rewindIndex]);
            }

            return matches;
        }

        /// <summary>
        /// Skips whitespace/comments and peeks at the upcoming ASCII-letter run, returning it upper-cased when it is a
        /// standalone keyword (EXISTS, NOT, IN, …) — i.e. NOT immediately followed by a QName continuer (digit, '_',
        /// '-') or a ':' (which would make it a prefix label). The reader is always restored to its prior position.
        /// </summary>
        private static string PeekFilterKeyword(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);

            string letterRun = ReadKeyword(parserContext);
            if (letterRun.Length == 0)
                return string.Empty;

            //A ':' or a QName-continuer right after the run means it is part of a prefixed name, not a keyword
            int terminatorCodePoint = PeekCodePoint(parserContext);
            bool isPartOfLongerToken = terminatorCodePoint == ':'
                                       || terminatorCodePoint == '_'
                                       || terminatorCodePoint == '-'
                                       || (terminatorCodePoint >= '0' && terminatorCodePoint <= '9');

            UnreadString(parserContext, letterRun);
            return isPartOfLongerToken ? string.Empty : letterRun.ToUpperInvariant();
        }

        /// <summary>
        /// Skips whitespace/comments and reports whether the next significant character is <paramref name="operatorChar"/>
        /// (one of the additive operators '+'/'-'). At the additive level the operand on the left is already parsed, so
        /// a sign here is unambiguously the binary operator.
        /// </summary>
        private static bool IsAdditiveOperatorAhead(RDFQueryParserContext parserContext, int operatorChar)
            => SkipWhitespace(parserContext) == operatorChar;

        /// <summary>
        /// At a unary-operator position (reader on a '+'/'-'), reports whether the sign directly introduces a numeric
        /// literal (a digit or decimal point follows). Such a sign belongs to the literal token and is left for the
        /// term-reader, rather than being treated as a unary operator on a sub-expression.
        /// </summary>
        private static bool IsSignedNumericLiteralAhead(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);

            //Consume the sign, peek the following character, then push the sign back so the reader is unchanged
            int signCodePoint = ReadCodePoint(parserContext);
            int afterSignCodePoint = PeekCodePoint(parserContext);
            UnreadCodePoint(parserContext, signCodePoint);

            return (afterSignCodePoint >= '0' && afterSignCodePoint <= '9') || afterSignCodePoint == '.';
        }

        /// <summary>
        /// Parses the argument list of an IRI-named <c>FunctionCall</c> (the IRI has already been read) and dispatches
        /// it to the matching expression. The only IRI functions the engine understands are the GeoSPARQL
        /// <c>geof:</c> (and a few <c>geosparql:</c>) functions; any other function IRI is rejected with a precise
        /// "not supported" message, per the F6 design decision.
        /// </summary>
        private static RDFExpression ParseIriFunctionCall(RDFQueryParserContext parserContext, RDFResource functionIri)
            => DispatchGeoFunctionCall(parserContext, functionIri, ParseExpressionArgumentList(parserContext));
        #endregion
    }
}