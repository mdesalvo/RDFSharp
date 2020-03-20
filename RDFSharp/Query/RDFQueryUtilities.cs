/*
   Copyright 2012-2020 Marco De Salvo

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

using RDFSharp.Model;
using RDFSharp.Store;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFQueryUtilities is a collector of reusable utility methods for RDF query management
    /// </summary>
    internal static class RDFQueryUtilities
    {

        #region MIRELLA RDF
        /// <summary>
        /// Parses the given string to return an instance of pattern member
        /// </summary>
        internal static RDFPatternMember ParseRDFPatternMember(String pMember)
        {

            if (pMember != null)
            {

                #region Uri
                Uri testUri;
                if (Uri.TryCreate(pMember, UriKind.Absolute, out testUri))
                {
                    return new RDFResource(pMember);
                }
                #endregion

                #region Plain Literal
                if (!pMember.Contains("^^") ||
                     pMember.EndsWith("^^") ||
                     RDFModelUtilities.GetUriFromString(pMember.Substring(pMember.LastIndexOf("^^", StringComparison.Ordinal) + 2)) == null)
                {
                    RDFPlainLiteral pLit = null;
                    if (RDFNTriples.regexLPL.Match(pMember).Success)
                    {
                        String pLitVal = pMember.Substring(0, pMember.LastIndexOf("@", StringComparison.Ordinal));
                        String pLitLng = pMember.Substring(pMember.LastIndexOf("@", StringComparison.Ordinal) + 1);
                        pLit = new RDFPlainLiteral(pLitVal, pLitLng);
                    }
                    else
                    {
                        pLit = new RDFPlainLiteral(pMember);
                    }
                    return pLit;
                }
                #endregion

                #region Typed Literal
                String tLitValue = pMember.Substring(0, pMember.LastIndexOf("^^", StringComparison.Ordinal));
                String tLitDatatype = pMember.Substring(pMember.LastIndexOf("^^", StringComparison.Ordinal) + 2);
                RDFModelEnums.RDFDatatypes dt = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                RDFTypedLiteral tLit = new RDFTypedLiteral(tLitValue, dt);
                return tLit;
                #endregion

            }
            throw new RDFQueryException("Cannot parse pattern member because given \"pMember\" parameter is null.");

        }

        /// <summary>
        /// Compares the given pattern members, throwing a "Type Error" whenever the comparison operator detects sematically incompatible members;
        /// </summary>
        internal static Int32 CompareRDFPatternMembers(RDFPatternMember left, RDFPatternMember right)
        {

            #region NULLS
            if (left == null)
            {
                if (right == null)
                {
                    return 0;
                }
                return -1;
            }
            if (right == null)
            {
                return 1;
            }
            #endregion

            #region RESOURCE/CONTEXT
            if (left is RDFResource || left is RDFContext)
            {

                //RESOURCE/CONTEXT VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                {
                    return String.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }

                //RESOURCE/CONTEXT VS TYPEDLITERAL
                else
                {
                    if (((RDFTypedLiteral)right).HasStringDatatype())
                    {
                        return String.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

            }
            #endregion

            #region PLAINLITERAL
            else if (left is RDFPlainLiteral)
            {

                //PLAINLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                {
                    return String.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }

                //PLAINLITERAL VS TYPEDLITERAL
                else
                {
                    if (((RDFTypedLiteral)right).HasStringDatatype())
                    {
                        return String.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

            }
            #endregion

            #region TYPEDLITERAL
            else
            {

                //TYPEDLITERAL VS RESOURCE/CONTEXT/PLAINLITERAL
                if (right is RDFResource || right is RDFContext || right is RDFPlainLiteral)
                {
                    if (((RDFTypedLiteral)left).HasStringDatatype())
                    {
                        return String.Compare(((RDFTypedLiteral)left).Value, right.ToString(), StringComparison.Ordinal);
                    }
                    return -99; //Type Error
                }

                //TYPEDLITERAL VS TYPEDLITERAL
                else
                {
                    if (((RDFTypedLiteral)left).HasBooleanDatatype() && ((RDFTypedLiteral)right).HasBooleanDatatype())
                    {
                        Boolean leftValueBoolean = Boolean.Parse(((RDFTypedLiteral)left).Value);
                        Boolean rightValueBoolean = Boolean.Parse(((RDFTypedLiteral)right).Value);
                        return leftValueBoolean.CompareTo(rightValueBoolean);
                    }
                    else if (((RDFTypedLiteral)left).HasDatetimeDatatype() && ((RDFTypedLiteral)right).HasDatetimeDatatype())
                    {
                        DateTime leftValueDateTime = DateTime.Parse(((RDFTypedLiteral)left).Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        DateTime rightValueDateTime = DateTime.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                        return leftValueDateTime.CompareTo(rightValueDateTime);
                    }
                    else if (((RDFTypedLiteral)left).HasDecimalDatatype() && ((RDFTypedLiteral)right).HasDecimalDatatype())
                    {
                        Decimal leftValueDecimal = Decimal.Parse(((RDFTypedLiteral)left).Value, CultureInfo.InvariantCulture);
                        Decimal rightValueDecimal = Decimal.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture);
                        return leftValueDecimal.CompareTo(rightValueDecimal);
                    }
                    else if (((RDFTypedLiteral)left).HasStringDatatype() && ((RDFTypedLiteral)right).HasStringDatatype())
                    {
                        String leftValueString = ((RDFTypedLiteral)left).Value;
                        String rightValueString = ((RDFTypedLiteral)right).Value;
                        return leftValueString.CompareTo(rightValueString);
                    }
                    else if (((RDFTypedLiteral)left).HasTimespanDatatype() && ((RDFTypedLiteral)right).HasTimespanDatatype())
                    {
                        TimeSpan leftValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)left).Value);
                        TimeSpan rightValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)right).Value);
                        return leftValueDuration.CompareTo(rightValueDuration);
                    }
                    else
                    {
                        return -99; //Type Error
                    }
                }

            }
            #endregion

        }
        
        /// <summary>
        /// Tries to abbreviate the string representation of the given pattern member by searching for it in the given list of namespaces
        /// </summary>
        internal static (Boolean, String) AbbreviateRDFPatternMember(RDFPatternMember patternMember, List<RDFNamespace> prefixes)
        {
            #region Prefix Search
            //Check if the pattern member starts with a known prefix, if so just return it
            if (prefixes == null) prefixes = new List<RDFNamespace>();
            var prefixToSearch = patternMember.ToString().Split(':')[0];
            var searchedPrefix = prefixes.Find(pf => pf.NamespacePrefix.Equals(prefixToSearch, StringComparison.OrdinalIgnoreCase));
            if (searchedPrefix != null)
            {
                return (true, patternMember.ToString());
            }
            #endregion

            #region Namespace Search
            //Check if the pattern member starts with a known namespace, if so replace it with its prefix
            String pmString = patternMember.ToString();
            Boolean abbrev = false;
            prefixes.ForEach(ns =>
            {
                if (!abbrev)
                {
                    String nS = ns.ToString();
                    if (!pmString.Equals(nS, StringComparison.OrdinalIgnoreCase))
                    {
                        if (pmString.StartsWith(nS))
                        {
                            pmString = pmString.Replace(nS, ns.NamespacePrefix + ":").TrimEnd(new Char[] { '/' });

                            //Accept the abbreviation only if it has generated a valid XSD QName
                            try
                            {
                                var qn = new RDFTypedLiteral(pmString, RDFModelEnums.RDFDatatypes.XSD_QNAME);
                                abbrev = true;
                            }
                            catch
                            {
                                pmString = patternMember.ToString();
                                abbrev = false;
                            }
                        }
                    }
                }
            });
            return (abbrev, pmString);
            #endregion
        }
        
        /// <summary>
        /// Removes the duplicates from the given list of T elements
        /// </summary>
        internal static List<T> RemoveDuplicates<T>(List<T> elements) where T : RDFPatternMember {
            List<T> results = new List<T>();
            if (elements?.Count > 0) {
                HashSet<Int64> lookup = new HashSet<Int64>();
                elements.ForEach(element => {
                    if (!lookup.Contains(element.PatternMemberID)) {
                        lookup.Add(element.PatternMemberID);
                        results.Add(element);
                    }
                });
            }
            return results;
        }
        #endregion

    }

}