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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
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
        public static RDFPatternMember ParseRDFPatternMember(string pMember)
        {
            if (pMember == null)
                throw new RDFQueryException("Cannot parse pattern member because given \"pMember\" parameter is null.");

            #region Resource
            if (Uri.TryCreate(pMember, UriKind.Absolute, out _))
                return new RDFResource(pMember);
            #endregion

            #region Plain Literal
            int lastIndexOfDatatype = pMember.LastIndexOf("^^", StringComparison.OrdinalIgnoreCase);
            if (!pMember.Contains("^^")
                  || pMember.EndsWith("^^", StringComparison.Ordinal)
                  || RDFModelUtilities.GetUriFromString(pMember.Substring(lastIndexOfDatatype + 2)) == null)
            {
                RDFPlainLiteral pLit;
                if (RDFNTriples.regexLPL.Value.Match(pMember).Success)
                {
                    int lastIndexOfLanguage = pMember.LastIndexOf("@", StringComparison.OrdinalIgnoreCase);
                    string pLitVal = pMember.Substring(0, lastIndexOfLanguage);
                    string pLitLng = pMember.Substring(lastIndexOfLanguage + 1);
                    pLit = new RDFPlainLiteral(pLitVal, pLitLng);
                }
                else
                    pLit = new RDFPlainLiteral(pMember);
                return pLit;
            }
            #endregion

            #region Typed Literal
            string tLitValue = pMember.Substring(0, lastIndexOfDatatype);
            string tLitDatatype = pMember.Substring(lastIndexOfDatatype + 2);
            RDFTypedLiteral tLit = new RDFTypedLiteral(tLitValue, RDFDatatypeRegister.GetDatatype(tLitDatatype));
            return tLit;
            #endregion
        }

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
                    if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                        return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);

                    //RESOURCE/CONTEXT VS TYPEDLITERAL
                    if (((RDFTypedLiteral)right).HasStringDatatype())
                        return string.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);

                    return -99; //Type Error
                }
                //PLAINLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                case RDFPlainLiteral _ when right is RDFResource || right is RDFContext || right is RDFPlainLiteral:
                    return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                //PLAINLITERAL VS TYPEDLITERAL
                case RDFPlainLiteral _ when ((RDFTypedLiteral)right).HasStringDatatype():
                    return string.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                case RDFPlainLiteral _:
                    return -99; //Type Error
                default:
                {
                    //TYPEDLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                    if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                    {
                        if (((RDFTypedLiteral)left).HasStringDatatype())
                            return string.Compare(((RDFTypedLiteral)left).Value, right.ToString(), StringComparison.Ordinal);

                        return -99; //Type Error
                    }

                    //TYPEDLITERAL VS TYPEDLITERAL

                    //DATETIME
                    if (((RDFTypedLiteral)left).HasDatetimeDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasDatetimeDatatype())
                        {
                            DateTime leftValueDateTime = DateTime.Parse(((RDFTypedLiteral)left).Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                            DateTime rightValueDateTime = DateTime.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                            return leftValueDateTime.CompareTo(rightValueDateTime);
                        }
                        return -99; //Type Error
                    }

                    //DECIMAL
                    if (((RDFTypedLiteral)left).HasDecimalDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasDecimalDatatype())
                        {
                            //owl:rational needs parsing and evaluation before being compared (LEFT)
                            decimal leftValueDecimal = ((RDFTypedLiteral)left).Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                ? RDFModelUtilities.ComputeOWLRationalValue((RDFTypedLiteral)left)
                                : decimal.Parse(((RDFTypedLiteral)left).Value, CultureInfo.InvariantCulture);
                            //owl:rational needs parsing and evaluation before being compared (RIGHT)
                            decimal rightValueDecimal = ((RDFTypedLiteral)right).Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL
                                ? RDFModelUtilities.ComputeOWLRationalValue((RDFTypedLiteral)right)
                                : decimal.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture);
                            return leftValueDecimal.CompareTo(rightValueDecimal);
                        }
                        return -99; //Type Error
                    }

                    //STRING
                    if (((RDFTypedLiteral)left).HasStringDatatype())
                    {
                        if (((RDFTypedLiteral)right).HasStringDatatype())
                        {
                            string leftValueString = ((RDFTypedLiteral)left).Value;
                            string rightValueString = ((RDFTypedLiteral)right).Value;
                            return string.Compare(leftValueString, rightValueString, StringComparison.Ordinal);
                        }
                        return -99; //Type Error
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
                        return -99; //Type Error
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
                        return -99; //Type Error
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
                        return -99; //Type Error
                    }

                    //Fallback (not possible)
                    return -99; //Type Error
                }
            }
        }

        /// <summary>
        /// Tries to abbreviate the string representation of the given pattern member by searching for it in the given list of namespaces
        /// </summary>
        internal static (bool, string) AbbreviateRDFPatternMember(RDFPatternMember patternMember, List<RDFNamespace> prefixes)
        {
            if (prefixes == null)
                prefixes = new List<RDFNamespace>();
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
                if (!pmemberString.Equals(nspString, StringComparison.OrdinalIgnoreCase))
                    if (pmemberString.StartsWith(nspString, StringComparison.Ordinal))
                    {
                        pmemberString = pmemberString.Replace(nspString, string.Concat(nsp.NamespacePrefix, ":"))
                            .TrimEnd('/');

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
        /// Removes the duplicates from the given list of T elements
        /// </summary>
        public static List<T> RemoveDuplicates<T>(List<T> elements) where T : RDFPatternMember
        {
            List<T> results = new List<T>();
            if (elements?.Count > 0)
            {
                HashSet<long> lookup = new HashSet<long>();
                elements.ForEach(element =>
                {
                    if (lookup.Add(element.PatternMemberID)) results.Add(element);
                });
            }
            return results;
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