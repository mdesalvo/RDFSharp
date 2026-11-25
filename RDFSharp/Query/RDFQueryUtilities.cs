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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Xml;
using NetTopologySuite.Geometries;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{
    /// <summary>
    /// RDFQueryUtilities is a collector of reusable utility methods for RDF query management
    /// </summary>
    public static class RDFQueryUtilities
    {
        #region MIRELLA RDF
        /// <summary>
        /// Parses the given string to return an instance of pattern member
        /// </summary>
        /// <exception cref="RDFQueryException"></exception>
        public static RDFPatternMember ParseRDFPatternMember(string pMember)
        {
            if (pMember == null)
                throw new RDFQueryException("Cannot parse pattern member because given \"pMember\" parameter is null.");

            #region Typed Literal
            //A trailing "^^" followed by an absolute datatype IRI marks a typed literal even when its value looks like an IRI:
            //the check comes before the resource one because '^' cannot appear in a valid IRI (RFC 3986)
            int lastIndexOfDatatype = pMember.LastIndexOf("^^", StringComparison.Ordinal);
            if (lastIndexOfDatatype != -1
                 && lastIndexOfDatatype != pMember.Length - 2 //EndsWith "^^"
                 && RDFModelUtilities.GetUriFromString(pMember.Substring(lastIndexOfDatatype + 2)) != null)
            {
                string tLitValue = pMember.Substring(0, lastIndexOfDatatype);
                string tLitDatatype = pMember.Substring(lastIndexOfDatatype + 2);
                return new RDFTypedLiteral(tLitValue, RDFDatatypeRegister.GetDatatype(tLitDatatype));
            }
            #endregion

            #region Resource
            //An absolute IRI is a resource: a language-tagged literal whose value looks like an IRI cannot be told apart
            //from an IRI having '@' in its path, so the resource reading wins
            if (Uri.TryCreate(pMember, UriKind.Absolute, out _))
                return new RDFResource(pMember);
            #endregion

            #region Plain Literal
            //A trailing language tag splits the text into value and language
            if (RDFShims.EndingLangTagRegex.Value.IsMatch(pMember))
            {
                int lastIndexOfLanguage = pMember.LastIndexOf('@');
                string pLitVal = pMember.Substring(0, lastIndexOfLanguage);
                string pLitLng = pMember.Substring(lastIndexOfLanguage + 1);
                return new RDFPlainLiteral(pLitVal, pLitLng);
            }
            //Any other text is a plain literal without language
            return new RDFPlainLiteral(pMember);
            #endregion
        }

        /// <summary>
        /// Result of CompareRDFPatternMembers when the given pattern members cannot be compared (SPARQL type error)
        /// </summary>
        internal const int ComparisonTypeError = -99;

        /// <summary>
        /// Compares the given pattern members, throwing a "Type Error" whenever the comparison operator detects sematically incompatible members;
        /// </summary>
        internal static int CompareRDFPatternMembers(RDFPatternMember left, RDFPatternMember right)
        {
            #region NULLS
            if (left == null)
            {
                if (right == null)
                    return 0;

                return -1;
            }
            if (right == null)
                return 1;
            #endregion

            switch (left)
            {
                case RDFResource _:
                case RDFContext _:
                {
                    //RESOURCE/CONTEXT VS RESOURCE/CONTEXT/PLAINLITERAL
                    //Only the sign is returned, since a raw code-point difference could collide with ComparisonTypeError
                    if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                        //Resources and contexts are ordered against strings by the code points of their lexical form
                        return Math.Sign(string.CompareOrdinal(left.ToString(), right.ToString()));

                    //RESOURCE/CONTEXT VS TYPEDLITERAL
                    if (((RDFTypedLiteral)right).HasStringDatatype())
                        //A string-typed literal takes part in the order with its lexical value
                        return Math.Sign(string.CompareOrdinal(left.ToString(), ((RDFTypedLiteral)right).Value));

                    //Resources and contexts are comparable only with string-typed literals
                    return ComparisonTypeError;
                }
                //PLAINLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                case RDFPlainLiteral _ when right is RDFResource || right is RDFContext || right is RDFPlainLiteral:
                    //Plain literals are ordered by the code points of their string form (the language tag included)
                    return Math.Sign(string.CompareOrdinal(left.ToString(), right.ToString()));
                //PLAINLITERAL VS TYPEDLITERAL
                case RDFPlainLiteral _ when ((RDFTypedLiteral)right).HasStringDatatype():
                    //A string-typed literal takes part in the order with its lexical value
                    return Math.Sign(string.CompareOrdinal(left.ToString(), ((RDFTypedLiteral)right).Value));
                case RDFPlainLiteral _:
                    //Plain literals are comparable only with string-typed literals
                    return ComparisonTypeError;
                default:
                {
                    //TYPEDLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                    if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                    {
                        if (((RDFTypedLiteral)left).HasStringDatatype())
                            //A string-typed literal takes part in the order with its lexical value
                            return Math.Sign(string.CompareOrdinal(((RDFTypedLiteral)left).Value, right.ToString()));

                        //A non-string typed literal cannot be compared with resources or plain literals
                        return ComparisonTypeError;
                    }

                    //TYPEDLITERAL VS TYPEDLITERAL

                    //DATETIME
                    if (((RDFTypedLiteral)left).HasDatetimeDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasDatetimeDatatype())
                        {
                            //A generic calendar has no absolute instant: its values are ordered by signed position and only within the same datatype
                            RDFModelEnums.RDFDatatypes leftDatatype = ((RDFTypedLiteral)left).Datatype.TargetDatatype;
                            if (leftDatatype == RDFModelEnums.RDFDatatypes.TIME_GENERALDAY
                                 || leftDatatype == RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH
                                 || leftDatatype == RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR)
                            {
                                return leftDatatype == ((RDFTypedLiteral)right).Datatype.TargetDatatype
                                    ? Math.Sign(GetGeneralTimeValue((RDFTypedLiteral)left).CompareTo(GetGeneralTimeValue((RDFTypedLiteral)right)))
                                    : ComparisonTypeError;
                            }

                            //Both instants come from the lexical formats of their datatypes
                            if (RDFModelUtilities.TryGetDateTimeValue((RDFTypedLiteral)left, out DateTime leftValueDateTime)
                                 && RDFModelUtilities.TryGetDateTimeValue((RDFTypedLiteral)right, out DateTime rightValueDateTime))
                            {
                                return leftValueDateTime.CompareTo(rightValueDateTime);
                            }

                            //A time:general* value on the right has no instant to be ordered against a date or time
                            return ComparisonTypeError;
                        }
                        //A date or time cannot be compared with a value of another datatype family
                        return ComparisonTypeError;
                    }

                    //DECIMAL
                    if (((RDFTypedLiteral)left).HasDecimalDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasDecimalDatatype())
                        {
                            //Each side is read in the value space of its datatype (double for xsd:double/xsd:float and decimal otherwise)
                            (bool leftIsFloatingPoint, double leftValueDouble, decimal leftValueDecimal) = GetNumericValue((RDFTypedLiteral)left);
                            (bool rightIsFloatingPoint, double rightValueDouble, decimal rightValueDecimal) = GetNumericValue((RDFTypedLiteral)right);

                            //Two exact numbers are compared without losing precision
                            if (!leftIsFloatingPoint && !rightIsFloatingPoint)
                                return leftValueDecimal.CompareTo(rightValueDecimal);

                            //A floating point operand brings the comparison into the double value space (numeric type promotion)
                            if (!leftIsFloatingPoint)
                                leftValueDouble = (double)leftValueDecimal;
                            if (!rightIsFloatingPoint)
                                rightValueDouble = (double)rightValueDecimal;

                            //NaN is not ordered with any number (SPARQL: every comparison with NaN is false)
                            if (double.IsNaN(leftValueDouble) || double.IsNaN(rightValueDouble))
                                return ComparisonTypeError;
                            //Infinities take part in the order as the extremes of the double value space
                            return leftValueDouble.CompareTo(rightValueDouble);
                        }
                        //A number cannot be compared with a value of another datatype family
                        return ComparisonTypeError;
                    }

                    //STRING
                    if (((RDFTypedLiteral)left).HasStringDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasStringDatatype())
                        {
                            string leftValueString = ((RDFTypedLiteral)left).Value;
                            string rightValueString = ((RDFTypedLiteral)right).Value;
                            //Two string-typed literals are ordered by the code points of their lexical values
                            return Math.Sign(string.CompareOrdinal(leftValueString, rightValueString));
                        }
                        //A string-typed literal cannot be compared with a value of another datatype family
                        return ComparisonTypeError;
                    }

                    //GEOGRAPHIC
                    if (((RDFTypedLiteral)left).HasGeographicDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasGeographicDatatype())
                        {
                            Geometry leftGeometry = ((RDFTypedLiteral)left).Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.WKT_LITERAL.ToString())
                                ? RDFGeoExpression.WKTReader.Read(((RDFTypedLiteral)left).Value)
                                : RDFGeoExpression.GMLReader.Read(((RDFTypedLiteral)left).Value);
                            leftGeometry.SRID = 4326;
                            Geometry rightGeometry = ((RDFTypedLiteral)right).Datatype.ToString().Equals(RDFVocabulary.GEOSPARQL.WKT_LITERAL.ToString())
                                ? RDFGeoExpression.WKTReader.Read(((RDFTypedLiteral)right).Value)
                                : RDFGeoExpression.GMLReader.Read(((RDFTypedLiteral)right).Value);
                            rightGeometry.SRID = 4326;
                            return leftGeometry.CompareTo(rightGeometry);
                        }
                        //A geographic value cannot be compared with a value of another datatype family
                        return ComparisonTypeError;
                    }

                    //TIMESPAN
                    if (((RDFTypedLiteral)left).HasTimespanDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasTimespanDatatype())
                        {
                            TimeSpan leftValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)left).Value);
                            TimeSpan rightValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)right).Value);
                            return leftValueDuration.CompareTo(rightValueDuration);
                        }
                        //A duration cannot be compared with a value of another datatype family
                        return ComparisonTypeError;
                    }

                    //BOOLEAN
                    if (((RDFTypedLiteral)left).HasBooleanDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasBooleanDatatype())
                        {
                            bool leftValueBoolean = bool.Parse(((RDFTypedLiteral)left).Value);
                            bool rightValueBoolean = bool.Parse(((RDFTypedLiteral)right).Value);
                            return leftValueBoolean.CompareTo(rightValueBoolean);
                        }
                        //A boolean cannot be compared with a value of another datatype family
                        return ComparisonTypeError;
                    }

                    //Fallback (not possible)
                    //A typed literal of an unsupported datatype family is incomparable
                    return ComparisonTypeError;
                }
            }

            //Gives the signed position of a time:generalYear/Month/Day literal: the time zone is dropped since a generic calendar has no absolute instant
            BigInteger GetGeneralTimeValue(RDFTypedLiteral typedLiteral)
            {
                //The literal was validated at creation: its digits sit at the fixed positions of the datatype grammar
                string lexicalValue = typedLiteral.Value;
                switch (typedLiteral.Datatype.TargetDatatype)
                {
                    case RDFModelEnums.RDFDatatypes.TIME_GENERALDAY:
                        //Day lexical form is ---DD with an optional time zone
                        return BigInteger.Parse(lexicalValue.Substring(3, 2), NumberStyles.None, CultureInfo.InvariantCulture);
                    case RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH:
                        //Month lexical form is --MM with an optional time zone
                        return BigInteger.Parse(lexicalValue.Substring(2, 2), NumberStyles.None, CultureInfo.InvariantCulture);
                    default:
                    {
                        //Year lexical form is an optional minus sign and an unbounded digit run: the time zone that follows is not part of the run
                        bool isBeforeCommonEra = lexicalValue[0] == '-';
                        int yearDigitsStart = isBeforeCommonEra ? 1 : 0;
                        //The digit run ends where the time zone (Z, + or -) starts or at the end of the value
                        int yearDigitsEnd = yearDigitsStart;
                        while (yearDigitsEnd < lexicalValue.Length && lexicalValue[yearDigitsEnd] >= '0' && lexicalValue[yearDigitsEnd] <= '9')
                            yearDigitsEnd++;
                        //A year before the common era is a negative position of the calendar
                        BigInteger year = BigInteger.Parse(lexicalValue.Substring(yearDigitsStart, yearDigitsEnd - yearDigitsStart), NumberStyles.None, CultureInfo.InvariantCulture);
                        return isBeforeCommonEra ? -year : year;
                    }
                }
            }

            //Gives the value of a numeric typed literal in its own value space: double for xsd:double and xsd:float, decimal for every other numeric datatype
            (bool isFloatingPoint, double doubleValue, decimal decimalValue) GetNumericValue(RDFTypedLiteral typedLiteral)
            {
                //xsd:double and xsd:float keep NaN, infinities and exponent forms that decimal cannot represent
                if (typedLiteral.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DOUBLE
                     || typedLiteral.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_FLOAT)
                {
                    //The literal was validated at creation: its lexical form always reads back as a double
                    RDFModelUtilities.TryParseFloatingPoint(typedLiteral.Value, out double floatingPointValue);
                    return (true, floatingPointValue, default);
                }

                //owl:rational ("p/q") needs evaluation before being a number
                if (typedLiteral.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                    return (false, default, RDFArithmeticEngine.ComputeOWLRationalValue(typedLiteral));

                //Every other numeric datatype is exact and lives in the decimal value space
                return (false, default, decimal.Parse(typedLiteral.Value, CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Tries to abbreviate the string representation of the given pattern member by searching for it in the given list of namespaces
        /// </summary>
        internal static (bool, string) AbbreviateRDFPatternMember(RDFPatternMember patternMember, List<RDFNamespace> prefixes)
        {
            if (prefixes == null)
                prefixes = Enumerable.Empty<RDFNamespace>().ToList();
            string pmemberString = patternMember.ToString();
            string pmemberStringOriginal = patternMember.ToString();

            #region Prefix Search
            //Check if the pattern member starts with a known prefix, if so just return it
            string prefixToSearch = pmemberString.Split(':')[0];
            RDFNamespace searchedPrefix = prefixes.Find(pf => pf.NamespacePrefix.Equals(prefixToSearch, StringComparison.OrdinalIgnoreCase));
            if (searchedPrefix != null)
                return (true, pmemberString);
            #endregion

            #region Namespace Search
            //Check if the pattern member starts with a known namespace, if so replace it with its prefix
            bool hasAbbreviation = false;
            foreach (RDFNamespace nsp in prefixes)
            {
                string nspString = nsp.ToString();
                if (!string.Equals(pmemberString, nspString, StringComparison.OrdinalIgnoreCase)
                     && pmemberString.StartsWith(nspString, StringComparison.Ordinal))
                {
                    pmemberString = pmemberString.Replace(nspString, $"{nsp.NamespacePrefix}:").TrimEnd('/');

                    //Accept the abbreviation only if it has generated a valid XSD QName
                    try
                    {
                        _ = new RDFTypedLiteral(pmemberString, RDFModelEnums.RDFDatatypes.XSD_QNAME);
                        hasAbbreviation = true;
                        break;
                    }
                    catch
                    {
                        pmemberString = pmemberStringOriginal;
                    }
                }
            }
            return (hasAbbreviation, pmemberString);
            #endregion
        }

        /// <summary>
        /// Compiles a SPARQL regular expression from its pattern and flag string, returning null when the pattern is
        /// null/invalid or a flag is unknown. Used by the dynamic (per-row) REGEX/REPLACE expressions, where a binding
        /// error must degrade to an unbound result rather than throwing (the parser-side BuildRegex throws instead).
        /// </summary>
        internal static Regex CompileRegexOrNull(string pattern, string flags)
        {
            if (pattern == null)
                return null;

            RegexOptions regexOptions = RegexOptions.None;
            foreach (char flagCharacter in flags ?? string.Empty)
            {
                switch (flagCharacter)
                {
                    case 'i': regexOptions |= RegexOptions.IgnoreCase;              break;
                    case 's': regexOptions |= RegexOptions.Singleline;              break;
                    case 'm': regexOptions |= RegexOptions.Multiline;               break;
                    case 'x': regexOptions |= RegexOptions.IgnorePatternWhitespace; break;
                    default:  return null; //unknown flag => binding error
                }
            }

            try
            {
                return new Regex(pattern, regexOptions);
            }
            catch (ArgumentException)
            {
                return null; //invalid pattern => binding error
            }
        }

        /// <summary>
        /// Removes the duplicates from the given list of T elements
        /// </summary>
        public static List<T> RemoveDuplicates<T>(List<T> elements) where T : RDFPatternMember
        {
            if (elements?.Count > 0)
            {
                List<T> results = new List<T>(elements.Count);
                HashSet<long> lookup = new HashSet<long>();
                elements.ForEach(element =>
                {
                    if (lookup.Add(element.PatternMemberID))
                        results.Add(element);
                });
                return results;
            }
            return Enumerable.Empty<T>().ToList();
        }

        /// <summary>
        /// RDFWebClient extends WebClient with support for customization of timeout
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal sealed class RDFWebClient : WebClient
        {
            #region Properties
            private int TimeOut { get; }
            #endregion

            #region Ctors
            internal RDFWebClient(int timeoutMilliseconds)
                => TimeOut = timeoutMilliseconds < -1 ? -1 : timeoutMilliseconds;
            #endregion

            #region Methods
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest webRequest = base.GetWebRequest(address);
                webRequest.Timeout = TimeOut;
                return webRequest;
            }
            #endregion
        }
        #endregion
    }
}