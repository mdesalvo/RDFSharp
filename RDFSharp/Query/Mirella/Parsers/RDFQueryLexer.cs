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
using System.Text;
using RDFSharp.Model;
using RDFQueryParserContext = RDFSharp.Query.RDFQueryParser.RDFQueryParserContext;

namespace RDFSharp.Query
{
    /// <summary>
    /// <para>
    /// RDFQueryLexer is the lexing layer of the SPARQL parser: the low-level reader primitives (code-point
    /// read/peek/unread, keyword runs, variables, terms), the structural-punctuation helpers, and the
    /// character-class predicates shared by every <see cref="RDFQueryParser"/> file.
    /// </para>
    /// <para>
    /// It is a standalone static class rather than a partial of <see cref="RDFQueryParser"/> so the lexing
    /// concern is cleanly separable; the parser files import it via <c>using static RDFSharp.Query.RDFQueryLexer;</c>
    /// so their existing unqualified calls (<c>SkipWhitespace(…)</c>, <c>ReadCodePoint(…)</c>, …) keep working.
    /// It operates on the parser's <see cref="RDFQueryParser.RDFQueryParserContext"/> (aliased here for brevity),
    /// which carries the shared pushback reader and prologue resolver.
    /// </para>
    /// </summary>
    internal static class RDFQueryLexer
    {
        #region Lexer
        /// <summary>
        /// The set of SPARQL keywords that can appear at the <c>GraphPatternNotTriples</c> dispatch point
        /// (i.e. wherever a new non-triple element can begin inside a <c>GroupGraphPattern</c>).
        /// <para>
        /// This set serves two purposes:
        /// <list type="number">
        /// <item>It terminates an ongoing TriplesBlock scan — as soon as one of these keywords is spotted,
        ///   the triple reader stops so the enclosing group-pattern dispatcher can handle the keyword.</item>
        /// <item>It distinguishes a lone keyword (e.g. "UNION") from a prefixed-name local part that
        ///   happens to start with the same letters (e.g. "union:foo") — the check in
        ///   <see cref="PeekGraphPatternKeyword"/> uses this set to decide whether the letter run
        ///   should be treated as a keyword or as the beginning of a QName token.</item>
        /// </list>
        /// </para>
        /// <para>
        /// Comparison is case-insensitive (SPARQL keywords are case-insensitive per the spec).
        /// </para>
        /// <para>
        /// Phase note: OPTIONAL / UNION / MINUS (F2a) and GRAPH (F3) are handled; the remaining keywords
        /// (FILTER, SERVICE, BIND, VALUES) are recognized here only so the TriplesBlock scan stops at them and
        /// the dispatcher can raise a precise "not supported yet" error until their phase lands. SELECT is listed
        /// because a SubSelect (<c>'{' SubSelect '}'</c>) can begin a group; today it is rejected as
        /// unsupported, and when the SubSelect phase lands it must be REMOVED from the throw branch in
        /// <see cref="RDFQueryParser.ParseGroupGraphPatternSub"/> and routed to a dedicated SubSelect parser instead.
        /// </para>
        /// </summary>
        internal static readonly HashSet<string> GraphPatternKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "OPTIONAL", "UNION", "MINUS", "FILTER", "GRAPH", "SERVICE", "BIND", "VALUES", "SELECT" };

        /// <summary>
        /// Skips any run of whitespace and #-style comments, leaving the reader positioned on the next
        /// significant character. Returns the code point of that character (without consuming it), or -1 at EOF.
        /// Thin pass-through to RDFTurtle so SPARQL and Turtle share identical whitespace/comment handling.
        /// </summary>
        internal static int SkipWhitespace(RDFQueryParserContext parserContext)
            => RDFTurtle.SkipWhitespace(parserContext.TermParsingContext);

        /// <summary>
        /// Reads a maximal run of ASCII letters starting at the current position and returns it as a string
        /// (empty if the current character is not an ASCII letter). The first non-letter character is unread,
        /// so the reader is left exactly at the boundary of the run. SPARQL keywords (SELECT, BASE, PREFIX,
        /// ASK, ...) are pure ASCII letters, so this is the primitive used to peek/consume them; callers that
        /// only wanted to peek can push the run back with <see cref="UnreadString"/>.
        /// </summary>
        internal static string ReadKeyword(RDFQueryParserContext parserContext)
        {
            //Work directly on the reused Turtle parsing context: it owns the pushback reader that lets us
            //read the keyword run and then rewind the single character that marked its end.
            RDFTurtle.RDFTurtleContext termContext = parserContext.TermParsingContext;

            //Accumulate the consecutive ASCII letters that make up the keyword. We do NOT skip leading
            //whitespace here on purpose: callers are expected to have positioned the reader on the first
            //significant character already (typically via SkipWhitespace), so an empty result unambiguously
            //means "the cursor is not on a keyword" rather than "we skipped over some spaces".
            StringBuilder keywordRun = new StringBuilder();

            //Read the first code point; if it is not a letter the loop never runs and we return the empty string.
            int codePoint = RDFTurtle.ReadCodePoint(termContext);
            while (IsAsciiLetter(codePoint))
            {
                //This code point belongs to the keyword: append it (AppendCodePoint handles supplementary
                //planes too, though SPARQL keywords are always plain ASCII) and advance to the next one.
                RDFTurtle.AppendCodePoint(keywordRun, codePoint);
                codePoint = RDFTurtle.ReadCodePoint(termContext);
            }

            //The loop exits on the first non-letter code point (a space, punctuation, '<', '?', or even -1 at
            //EOF). That character does NOT belong to the keyword, so we push it back onto the reader: this keeps
            //ReadKeyword non-destructive past the keyword boundary and lets the caller resume exactly there
            //(e.g. ParsePrologue dispatching on the keyword, or the body parser reading the following token).
            RDFTurtle.UnreadCodePoint(termContext, codePoint);

            //Note: the keyword is returned VERBATIM, preserving its original casing. SPARQL keywords are
            //case-insensitive, so comparisons against this result must use a case-insensitive ordinal compare.
            return keywordRun.ToString();
        }

        /// <summary>
        /// Pushes the characters of the given string back onto the reader, preserving their original order,
        /// so a subsequent read returns them again. Used to "un-peek" a keyword run that turned out not to be
        /// the one we were looking for.
        /// </summary>
        internal static void UnreadString(RDFQueryParserContext parserContext, string alreadyReadText)
            => RDFTurtle.UnreadCodePoint(parserContext.TermParsingContext, alreadyReadText);

        /// <summary>
        /// Parses a SPARQL variable: the sigil <c>?</c> or <c>$</c> followed by a non-empty VARNAME.
        /// Both sigils denote the same variable, so they are interchangeable (RDFVariable normalizes the name).
        /// </summary>
        /// <exception cref="RDFQueryException">When no sigil is present or the variable name is empty.</exception>
        internal static RDFVariable ParseVariable(RDFQueryParserContext parserContext)
        {
            RDFTurtle.RDFTurtleContext termContext = parserContext.TermParsingContext;

            //A variable must open with the '?' or '$' sigil
            int sigilCodePoint = RDFTurtle.ReadCodePoint(termContext);
            if (sigilCodePoint != '?' && sigilCodePoint != '$')
                throw new RDFQueryException("Cannot parse SPARQL variable: expected '?' or '$' " + GetCoordinates(parserContext));

            //Read the VARNAME body
            StringBuilder variableName = new StringBuilder();
            int codePoint = RDFTurtle.ReadCodePoint(termContext);

            //The first VARNAME character has a slightly stricter rule than the following ones
            if (!IsVarNameStartChar(codePoint))
            {
                RDFTurtle.UnreadCodePoint(termContext, codePoint);
                throw new RDFQueryException("Cannot parse SPARQL variable: empty or invalid variable name " + GetCoordinates(parserContext));
            }
            RDFTurtle.AppendCodePoint(variableName, codePoint);

            //Consume the remaining VARNAME characters
            codePoint = RDFTurtle.ReadCodePoint(termContext);
            while (IsVarNameChar(codePoint))
            {
                RDFTurtle.AppendCodePoint(variableName, codePoint);
                codePoint = RDFTurtle.ReadCodePoint(termContext);
            }
            //Unread the first character that does not belong to the variable name
            RDFTurtle.UnreadCodePoint(termContext, codePoint);

            //RDFVariable normalizes the name (strips sigils, upper-cases): pass the bare VARNAME
            return new RDFVariable(variableName.ToString());
        }

        /// <summary>
        /// Parses a single RDF term (IRI reference, prefixed name, blank node, literal or numeric literal) by
        /// delegating to RDFTurtle's ParseValue and normalizing the result into an RDFPatternMember. Prefix and
        /// base resolution flow through the SPARQL resolver wired into the context.
        /// </summary>
        /// <exception cref="RDFQueryException">When the text at the cursor is not a valid RDF term.</exception>
        internal static RDFPatternMember ParseTerm(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);
            try
            {
                //RDFTurtle.ParseValue returns either a System.Uri (for IRIs/prefixed names) or an
                //RDFPatternMember (RDFResource for blank nodes, RDFLiteral for quoted/numeric/boolean literals)
                object parsedValue = RDFTurtle.ParseValue(parserContext.TermParsingContext);
                switch (parsedValue)
                {
                    case Uri parsedIri:
                        return new RDFResource(parsedIri.ToString());
                    case RDFPatternMember parsedPatternMember:
                        return parsedPatternMember;
                    default:
                        throw new RDFQueryException("Cannot parse SPARQL term: unexpected value '" + parsedValue + "' " + GetCoordinates(parserContext));
                }
            }
            catch (RDFModelException termParsingException)
            {
                //Surface term-level lexing failures as query-level parse errors (the parser speaks RDFQueryException)
                throw new RDFQueryException("Cannot parse SPARQL term " + GetCoordinates(parserContext) + ": " + termParsingException.Message, termParsingException);
            }
        }

        /// <summary>
        /// Returns a human-readable "[LINE:x,COL:y]" coordinate string for error messages, reusing the
        /// position tracking of the underlying reader.
        /// </summary>
        internal static string GetCoordinates(RDFQueryParserContext parserContext)
            => RDFTurtle.GetTurtleContextCoordinates(parserContext.TermParsingContext);

        /// <summary>
        /// <para>
        /// Peeks at the upcoming letter run to determine whether it is a recognized graph-pattern keyword
        /// (OPTIONAL, UNION, MINUS, FILTER, GRAPH, SERVICE, BIND, VALUES, SELECT) that stands alone as an
        /// operator, rather than the beginning of a QName-like token such as <c>union:SomeClass</c>.
        /// </para>
        /// <para>
        /// When a recognized keyword is found, the reader is restored to exactly the position it was at
        /// before the call (i.e. the keyword is left unconsumed) and the keyword is returned in uppercase.
        /// When the letter run is either not a recognized keyword, or IS a recognized keyword but is
        /// immediately followed by a QName-continuer character (digit, '_', or '-'), the reader is equally
        /// restored and the empty string is returned.
        /// </para>
        /// <para>
        /// IMPORTANT: This method does NOT skip leading whitespace. Callers are responsible for having
        /// positioned the reader on the first significant (non-whitespace) character BEFORE calling this
        /// method — typically by calling <see cref="SkipWhitespace"/> first. If the reader is sitting on a
        /// whitespace character, <see cref="ReadKeyword"/> will return the empty string immediately (since
        /// whitespace is not an ASCII letter), and this method will therefore return the empty string too,
        /// even if a keyword follows the whitespace.
        /// </para>
        /// </summary>
        /// <returns>
        /// The recognized keyword in uppercase (e.g. "UNION"), or the empty string if the next token is
        /// not a standalone graph-pattern keyword.
        /// </returns>
        internal static string PeekGraphPatternKeyword(RDFQueryParserContext parserContext)
        {
            //Read the maximal ASCII-letter run that starts at the current reader position.
            //ReadKeyword leaves the first non-letter character on the reader (it "unreads" it), so after
            //this call the reader is positioned on the character immediately after the letter run.
            string candidateKeywordRun = ReadKeyword(parserContext);

            if (GraphPatternKeywords.Contains(candidateKeywordRun))
            {
                //The letter run is one of the recognized keywords. Now check the very next character (the
                //one that ended the letter run, still sitting on the reader) to decide whether this run is
                //a standalone keyword or the beginning of a longer QName local part.
                //Characters that can CONTINUE a QName local name (PN_CHARS family) after a keyword-matching
                //prefix are: decimal digits (0-9), underscore ('_') and hyphen ('-').
                //If the terminator is any of these, "OPTIONAL123" or "union_foo" would be misidentified as
                //a keyword — we must treat the whole run as part of a longer token instead.
                int terminatorCodePoint = PeekCodePoint(parserContext);
                bool isQNameContinuation = (terminatorCodePoint >= '0' && terminatorCodePoint <= '9')
                                           || terminatorCodePoint == '_'
                                           || terminatorCodePoint == '-';
                if (!isQNameContinuation)
                {
                    //Standalone keyword: push the letter run back so the caller can consume it explicitly,
                    //and return the keyword in normalized uppercase so comparisons are case-insensitive.
                    UnreadString(parserContext, candidateKeywordRun);
                    return candidateKeywordRun.ToUpperInvariant();
                }
            }

            //Either the letter run is not a recognized keyword, or it is a keyword but is followed by a
            //QName-continuer: in both cases push it back and signal "no standalone keyword here".
            UnreadString(parserContext, candidateKeywordRun);
            return string.Empty;
        }

        /// <summary>
        /// Consumes the current graph-pattern keyword token from the reader.
        /// Callers MUST have confirmed via <see cref="PeekGraphPatternKeyword"/> that a keyword is present
        /// before calling this: the method simply advances past the letter run with no additional checks.
        /// </summary>
        internal static void ConsumeKeyword(RDFQueryParserContext parserContext)
            => ReadKeyword(parserContext);
        #endregion

        #region Lexer.Helpers
        /// <summary>
        /// Reads and returns the next Unicode code point from the underlying pushback reader, advancing the
        /// reader position by one code point. Returns <c>-1</c> at end of input. Thin delegation to
        /// <c>RDFTurtle.ReadCodePoint</c> so the SPARQL parser shares the identical pull-style I/O contract.
        /// </summary>
        internal static int ReadCodePoint(RDFQueryParserContext parserContext)
            => RDFTurtle.ReadCodePoint(parserContext.TermParsingContext);

        /// <summary>
        /// Returns the next Unicode code point from the reader WITHOUT advancing the reader position
        /// (i.e. the same code point will be returned by the next <see cref="ReadCodePoint"/> call).
        /// Returns <c>-1</c> at end of input. Used to look ahead by a single character without consuming it.
        /// </summary>
        internal static int PeekCodePoint(RDFQueryParserContext parserContext)
            => RDFTurtle.PeekCodePoint(parserContext.TermParsingContext);

        /// <summary>
        /// Pushes a single Unicode code point back onto the reader so that the next
        /// <see cref="ReadCodePoint"/> call returns it again. Used when a character has been consumed
        /// (e.g. to recognise a token boundary) but should be processed by a subsequent parsing step.
        /// </summary>
        internal static void UnreadCodePoint(RDFQueryParserContext parserContext, int codePoint)
            => RDFTurtle.UnreadCodePoint(parserContext.TermParsingContext, codePoint);

        /// <summary>
        /// <para>
        /// Skips any leading whitespace and <c>#</c>-style comments, then verifies that the next
        /// significant character is <paramref name="expectedCodePoint"/> and consumes it.
        /// </para>
        /// <para>
        /// Used for the structural punctuation of the SPARQL grammar: opening/closing braces
        /// (<c>{</c>, <c>}</c>), parentheses (<c>(</c>, <c>)</c>), and similar mandatory delimiters.
        /// When the expected character is not found, a descriptive <see cref="RDFQueryException"/> is
        /// thrown naming the grammar context (e.g. "WHERE clause") so the author knows which construct
        /// is malformed.
        /// </para>
        /// </summary>
        /// <param name="parserContext">The current parse state.</param>
        /// <param name="expectedCodePoint">The Unicode code point that must be present next.</param>
        /// <param name="grammarContext">Human-readable label for the grammar construct (used in error messages).</param>
        /// <exception cref="RDFQueryException">When the next significant character is not <paramref name="expectedCodePoint"/>.</exception>
        internal static void ExpectChar(RDFQueryParserContext parserContext, int expectedCodePoint, string grammarContext)
        {
            //Skip whitespace/comments and examine the first significant character
            int nextSignificantCodePoint = SkipWhitespace(parserContext);

            //If the significant character is not what we expect, throw a descriptive parse error
            if (nextSignificantCodePoint != expectedCodePoint)
                throw new RDFQueryException(
                    "Cannot parse SPARQL " + grammarContext + ": expected '"
                    + (char)expectedCodePoint + "' but found "
                    + DescribeCodePoint(nextSignificantCodePoint) + " "
                    + GetCoordinates(parserContext));

            //Consume the expected character
            ReadCodePoint(parserContext);
        }

        /// <summary>
        /// <para>
        /// Skips whitespace/comments and, if the next significant character equals
        /// <paramref name="candidateCodePoint"/>, consumes it and returns <c>true</c>. Otherwise leaves the
        /// reader positioned on that character (unchanged) and returns <c>false</c>.
        /// </para>
        /// <para>
        /// Used for optional single-character tokens such as the <c>*</c> wildcard in SELECT projections.
        /// </para>
        /// </summary>
        internal static bool TryConsumeChar(RDFQueryParserContext parserContext, int candidateCodePoint)
        {
            //Peek at the next significant character
            if (SkipWhitespace(parserContext) == candidateCodePoint)
            {
                //Match: consume and signal success
                ReadCodePoint(parserContext);
                return true;
            }

            //No match: the reader position is unchanged (SkipWhitespace leaves the char on the reader)
            return false;
        }

        /// <summary>
        /// <para>
        /// Skips whitespace/comments and, if the upcoming ASCII-letter run equals
        /// <paramref name="expectedKeyword"/> (case-insensitively), consumes it and returns <c>true</c>.
        /// Otherwise restores the reader to exactly the position it was at before the call and returns
        /// <c>false</c>.
        /// </para>
        /// <para>
        /// Used for keywords that are syntactically optional in the SPARQL grammar: <c>WHERE</c>,
        /// <c>DISTINCT</c>, <c>REDUCED</c>, and <c>BY</c> (in ORDER BY).
        /// </para>
        /// </summary>
        internal static bool TryConsumeKeyword(RDFQueryParserContext parserContext, string expectedKeyword)
        {
            //Position the reader on the first non-whitespace character before attempting to read a keyword
            SkipWhitespace(parserContext);

            //Read the maximal ASCII-letter run starting at the current position
            string candidateKeyword = ReadKeyword(parserContext);

            //Compare case-insensitively per SPARQL keyword rules
            if (candidateKeyword.Equals(expectedKeyword, StringComparison.OrdinalIgnoreCase))
                return true;

            //Not the expected keyword: push the run back so the reader is exactly where it started,
            //and signal that the optional keyword was not present
            UnreadString(parserContext, candidateKeyword);
            return false;
        }

        /// <summary>
        /// <para>
        /// Skips whitespace/comments and parses a non-negative decimal integer literal, returning its
        /// value as a 32-bit integer. Used for the LIMIT and OFFSET clause values.
        /// </para>
        /// </summary>
        /// <param name="parserContext">The current parse state.</param>
        /// <param name="grammarContext">Human-readable label for the containing grammar construct (used in error messages).</param>
        /// <exception cref="RDFQueryException">When no decimal digit is present at the current reader position.</exception>
        internal static int ParseInteger(RDFQueryParserContext parserContext, string grammarContext)
        {
            SkipWhitespace(parserContext);

            //Accumulate consecutive decimal-digit characters into a string builder
            StringBuilder digitRun = new StringBuilder();
            int codePoint = ReadCodePoint(parserContext);
            while (codePoint >= '0' && codePoint <= '9')
            {
                RDFTurtle.AppendCodePoint(digitRun, codePoint);
                codePoint = ReadCodePoint(parserContext);
            }

            //The first non-digit character terminates the number — push it back so the next reader call
            //sees it (it may be a keyword like LIMIT/OFFSET or the end of the query)
            UnreadCodePoint(parserContext, codePoint);

            //An empty digit run means no integer was present where one was required
            if (digitRun.Length == 0)
                throw new RDFQueryException(
                    "Cannot parse SPARQL " + grammarContext + ": expected a non-negative integer "
                    + GetCoordinates(parserContext));

            //Parse using the invariant culture so '.' is never misinterpreted as a decimal separator
            return int.Parse(digitRun.ToString(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Produces a human-readable description of a Unicode code point for use in parse-error messages:
        /// returns the character enclosed in single quotes (e.g. <c>'{'</c>) or the phrase
        /// <c>"end of input"</c> when the code point is <c>-1</c> (EOF sentinel).
        /// </summary>
        internal static string DescribeCodePoint(int codePoint)
            => codePoint == -1 ? "end of input" : "'" + char.ConvertFromUtf32(codePoint) + "'";

        #endregion

        #region Char.Check
        /// <summary>
        /// Returns <c>true</c> when <paramref name="codePoint"/> is an ASCII letter (A–Z or a–z).
        /// Used by <see cref="ReadKeyword"/> to accumulate the maximal letter run that may be a keyword.
        /// </summary>
        internal static bool IsAsciiLetter(int codePoint)
            => (codePoint >= 'A' && codePoint <= 'Z') || (codePoint >= 'a' && codePoint <= 'z');

        /// <summary>
        /// <para>
        /// Returns <c>true</c> when <paramref name="codePoint"/> is a valid FIRST character of a SPARQL
        /// variable name (VARNAME).
        /// </para>
        /// <para>
        /// SPARQL grammar: <c>VARNAME ::= ( PN_CHARS_U | [0-9] ) ( PN_CHARS_U | [0-9] | … )*</c>.
        /// The first character must be a PN_CHARS_U character (letter, underscore, or non-ASCII Unicode
        /// letter/digit in the PN_CHARS_U set) or an ASCII decimal digit.
        /// </para>
        /// </summary>
        internal static bool IsVarNameStartChar(int codePoint)
            => RDFTurtle.IsPN_CHARS_U(codePoint) || RDFTurtle.IsNumber(codePoint);

        /// <summary>
        /// <para>
        /// Returns <c>true</c> when <paramref name="codePoint"/> is a valid CONTINUATION character inside
        /// a SPARQL variable name (VARNAME) — i.e. any character valid at position 2 or later.
        /// </para>
        /// <para>
        /// SPARQL grammar: the tail of a VARNAME may contain PN_CHARS_U, decimal digits, and additional
        /// Unicode ranges: U+00B7 (middle dot), U+0300–U+036F (combining diacritics), and
        /// U+203F–U+2040 (undertie / character tie). The hyphen <c>-</c> is intentionally excluded:
        /// unlike Turtle's PN_CHARS, SPARQL VARNAME does not allow hyphens after the first character.
        /// </para>
        /// </summary>
        internal static bool IsVarNameChar(int codePoint)
            => RDFTurtle.IsPN_CHARS_U(codePoint)
               || RDFTurtle.IsNumber(codePoint)
               || codePoint == 0x00B7                              // MIDDLE DOT
               || (codePoint >= 0x0300 && codePoint <= 0x036F)    // Combining Diacritical Marks
               || (codePoint >= 0x203F && codePoint <= 0x2040);   // UNDERTIE, CHARACTER TIE
        #endregion
    }
}
