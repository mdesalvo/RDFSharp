/*
   Copyright 2012-2017 Marco De Salvo

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
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace RDFSharp.Model 
{

    /// <summary>
    /// RDFModelUtilities is a collector of reusable utility methods for RDF model management
    /// </summary>
    public static class RDFModelUtilities {

        #region Greta
        /// <summary>
        /// Performs MD5 hash calculation of the given string
        /// </summary>
        public static Int64 CreateHash(String input) {
            if (input != null) {
                var md5Encryptor   = new MD5CryptoServiceProvider();
                var inputBytes     = Encoding.UTF8.GetBytes(input);
                var hashBytes      = md5Encryptor.ComputeHash(inputBytes);
                return BitConverter.ToInt64(hashBytes, 0);
            }
            throw new RDFModelException("Cannot create hash because given \"input\" string parameter is null.");
        }
        #endregion

        #region Strings
        /// <summary>
        /// Regex to catch 8-byte unicodes
        /// </summary>
        internal static readonly Regex regexU8 = new Regex(@"\\U([0-9A-Fa-f]{8})", RegexOptions.Compiled);
        /// <summary>
        /// Regex to catch 4-byte unicodes
        /// </summary>
        internal static readonly Regex regexU4 = new Regex(@"\\u([0-9A-Fa-f]{4})", RegexOptions.Compiled);

        /// <summary>
        /// Gets the Uri corresponding to the given string
        /// </summary>
        internal static Uri GetUriFromString(String uriString) {
            Uri tempUri       = null;
            if (uriString    != null) {

                // blanks detection
                if (uriString.StartsWith("_:")) {
                    uriString = "bnode:" + uriString.Substring(2);
                }

				Uri.TryCreate(uriString, UriKind.Absolute, out tempUri);
            }
            return tempUri;
        }

        /// <summary>
        /// Generates a new Uri for a blank resource.
        /// It starts with "bnode:" and is a Guid.
        /// </summary>
        internal static Uri GenerateAnonUri() {
            return new Uri("bnode:" + Guid.NewGuid());
        }

        /// <summary>
        /// Turns back ASCII-encoded Unicodes into Unicodes. 
        /// </summary>
        internal static String ASCII_To_Unicode(String asciiString) {
            if (asciiString != null) {
                asciiString  = regexU8.Replace(asciiString, match => ((Char)Int64.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));
                asciiString  = regexU4.Replace(asciiString, match => ((Char)Int32.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));
            }
            return asciiString;
        }

        /// <summary>
        /// Turns Unicodes into ASCII-encoded Unicodes. 
        /// </summary>
        internal static String Unicode_To_ASCII(String unicodeString) {
            if (unicodeString   != null) {
                StringBuilder b  = new StringBuilder();
                foreach (Char c in unicodeString.ToCharArray()) {
                    if (c       <= 127) {
                        b.Append(c);
                    }
                    else {
                        if (c   <= 65535) {
                            b.Append("\\u" + ((Int32)c).ToString("X4"));
                        }
                        else {
                            b.Append("\\U" + ((Int32)c).ToString("X8"));
                        }
                    }
                }
                unicodeString    = b.ToString();
            }
            return unicodeString;
        }

        /// <summary>
        /// Replaces character controls for XML compatibility
        /// </summary>
        internal static String EscapeControlCharsForXML(String data) {
            if (data            != null) {
                StringBuilder b  = new StringBuilder();
                foreach (Char c in data.ToCharArray()) {
                    if  (Char.IsControl(c) && c != '\u0009' && c != '\u000A' && c != '\u000D') {
                         b.Append("\\u" + ((Int32)c).ToString("X4"));
                    }
                    else {
                         b.Append(c);
                    }
                }
                data             = b.ToString();
            }
            return data;
        }
        #endregion

        #region Graph
        /// <summary>
        /// Rebuild the metadata of the given graph
        /// </summary>
        internal static void RebuildGraph(RDFGraph graph) {
            var triples  = new Dictionary<Int64, RDFTriple>(graph.Triples);
            graph.ClearTriples();
            foreach (var t in triples) {
                graph.AddTriple(t.Value);
            }
        }

        /// <summary>
        /// Selects the triples corresponding to the given pattern from the given graph
        /// </summary>
        internal static List<RDFTriple> SelectTriples(RDFGraph graph,  RDFResource subj, 
                                                                       RDFResource pred, 
                                                                       RDFResource obj, 
                                                                       RDFLiteral  lit) {
            var matchSubj        = new List<RDFTriple>();
            var matchPred        = new List<RDFTriple>();
            var matchObj         = new List<RDFTriple>();
            var matchLit         = new List<RDFTriple>();
            var matchResult      = new List<RDFTriple>();
            if (graph           != null) {
                
                //Filter by Subject
                if (subj        != null) {
                    foreach (var t in graph.GraphIndex.SelectIndexBySubject(subj)) {
                        matchSubj.Add(graph.Triples[t]);
                    }
                }

                //Filter by Predicate
                if (pred        != null) {
                    foreach (var t in graph.GraphIndex.SelectIndexByPredicate(pred)) {
                        matchPred.Add(graph.Triples[t]);
                    }
                }

                //Filter by Object
                if (obj         != null) {
                    foreach (var t in graph.GraphIndex.SelectIndexByObject(obj)) {
                        matchObj.Add(graph.Triples[t]);
                    }
                }

                //Filter by Literal
                if (lit         != null) {
                    foreach (var t in graph.GraphIndex.SelectIndexByLiteral(lit)) {
                        matchLit.Add(graph.Triples[t]);
                    }
                }

                //Intersect the filters
                if (subj                   != null) {
                    if (pred               != null) {
                        if (obj            != null) {
                            //S->P->O
                            matchResult     = matchSubj.Intersect(matchPred)
                                                       .Intersect(matchObj)
                                                       .ToList();
                        }
                        else {
                            if (lit        != null) {
                                //S->P->L
                                matchResult = matchSubj.Intersect(matchPred)
                                                       .Intersect(matchLit)
                                                       .ToList();
                            }
                            else {
                                //S->P->
                                matchResult = matchSubj.Intersect(matchPred)
                                                       .ToList();
                            }
                        }
                    }
                    else {
                        if (obj            != null) {
                            //S->->O
                            matchResult     = matchSubj.Intersect(matchObj)
                                                       .ToList();
                        }
                        else {
                            if (lit        != null) {
                                //S->->L
                                matchResult = matchSubj.Intersect(matchLit)
                                                       .ToList();
                            }
                            else {
                                //S->->
                                matchResult = matchSubj;
                            }
                        }
                    }
                }
                else {
                    if (pred               != null) {
                        if (obj            != null) {
                            //->P->O
                            matchResult     = matchPred.Intersect(matchObj)
                                                       .ToList();
                        }
                        else {
                            if (lit        != null) {
                                //->P->L
                                matchResult = matchPred.Intersect(matchLit)
                                                       .ToList();
                            }
                            else {
                                //->P->
                                matchResult = matchPred;
                            }
                        }
                    }
                    else {
                        if (obj            != null) {
                            //->->O
                            matchResult     = matchObj;
                        }
                        else {
                            if (lit        != null) {
                                //->->L
                                matchResult = matchLit;
                            }
                            else {
                                //->->
                                matchResult = graph.Triples.Values.ToList();
                            }
                        }
                    }
                }

            }
            return matchResult;
        }
        #endregion

        #region Namespaces        
        /// <summary>
        /// Gets the list of namespaces used within the triples of the given graph
        /// </summary>
        internal static List<RDFNamespace> GetGraphNamespaces(RDFGraph graph) {
            var result     = new List<RDFNamespace>();
            foreach (var   t in graph) {
                var subj   = t.Subject.ToString();
                var pred   = t.Predicate.ToString();
                var obj    = t.Object is RDFResource ? t.Object.ToString() : 
                                (t.Object is RDFTypedLiteral ? GetDatatypeFromEnum(((RDFTypedLiteral)t.Object).Datatype) : String.Empty);

                //Resolve subject Uri
                var subjNS = RDFNamespaceRegister.Instance.Register.Where(x => subj.StartsWith(x.ToString()));

                //Resolve predicate Uri
                var predNS = RDFNamespaceRegister.Instance.Register.Where(x => pred.StartsWith(x.ToString()));

                //Resolve object Uri
                var objNS  = RDFNamespaceRegister.Instance.Register.Where(x => obj.StartsWith(x.ToString()));

                result.AddRange(subjNS);
                result.AddRange(predNS);
                result.AddRange(objNS);
            }
            return result.Distinct().ToList();
        }
        #endregion

        #region Datatypes
        /// <summary>
        /// Parse the given string in order to give the corresponding RDF/RDFS/XSD datatype
        /// </summary>
        internal static RDFModelEnums.RDFDatatypes GetDatatypeFromString(String datatypeString) {
            if (datatypeString != null) {

                //Preliminary check to verify if datatypeString is a valid Uri
                Uri dtypeStringUri;
                if (!Uri.TryCreate(datatypeString.Trim(), UriKind.Absolute, out dtypeStringUri)) {
                     throw new RDFModelException("Cannot recognize datatype representation of given \"datatypeString\" parameter because it is not a valid Uri.");
                }

                //Identification of specific RDF/RDFS/XSD datatype
                datatypeString  = dtypeStringUri.ToString();
                if (datatypeString.Equals(RDFVocabulary.RDF.XML_LITERAL.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL;
                }
                else if (datatypeString.Equals(RDFVocabulary.RDFS.LITERAL.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.RDFS_LITERAL;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.STRING.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_STRING;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.ANY_URI.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_ANYURI;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.BASE64_BINARY.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.BOOLEAN.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_BOOLEAN;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.BYTE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_BYTE;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.DATE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_DATE;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.DATETIME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_DATETIME;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.DECIMAL.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_DECIMAL;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.DOUBLE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_DOUBLE;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.DURATION.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_DURATION;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.FLOAT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_FLOAT;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.G_DAY.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_GDAY;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.G_MONTH.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_GMONTH;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.G_MONTH_DAY.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.G_YEAR.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_GYEAR;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.G_YEAR_MONTH.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.HEX_BINARY.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_HEXBINARY;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.ID.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_ID;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.INT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_INT;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_INTEGER;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.LANGUAGE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_LANGUAGE;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.LONG.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_LONG;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.NAME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_NAME;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.NCNAME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_NCNAME;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.NMTOKEN.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_NMTOKEN;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.NORMALIZED_STRING.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.NOTATION.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_NOTATION;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.POSITIVE_INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.QNAME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_QNAME;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.SHORT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_SHORT;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.TIME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_TIME;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.TOKEN.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_TOKEN;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.UNSIGNED_BYTE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.UNSIGNED_INT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.UNSIGNED_LONG.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG;
                }
                else if (datatypeString.Equals(RDFVocabulary.XSD.UNSIGNED_SHORT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT;
                }
                else {
                    //Unknown datatypes default to instances of "rdfs:Literal"
                    return RDFModelEnums.RDFDatatypes.RDFS_LITERAL;
                }

            }
            throw new RDFModelException("Cannot recognize datatype representation of given \"datatypeString\" parameter because it is null.");
        }

        /// <summary>
        /// Gives the string representation of the given RDF/RDFS/XSD datatype
        /// </summary>
        internal static String GetDatatypeFromEnum(RDFModelEnums.RDFDatatypes datatype) {
            if (datatype.Equals(RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)) {
                return RDFVocabulary.RDF.XML_LITERAL.ToString();
            }
			else if (datatype.Equals(RDFModelEnums.RDFDatatypes.RDFS_LITERAL)) {
                return RDFVocabulary.RDFS.LITERAL.ToString();
            }
			else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING)) {
                return RDFVocabulary.XSD.STRING.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ANYURI)) {
                return RDFVocabulary.XSD.ANY_URI.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)) {
                return RDFVocabulary.XSD.BASE64_BINARY.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)) {
                return RDFVocabulary.XSD.BOOLEAN.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BYTE)) {
                return RDFVocabulary.XSD.BYTE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DATE)) {
                return RDFVocabulary.XSD.DATE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DATETIME)) {
                return RDFVocabulary.XSD.DATETIME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DECIMAL)) {
                return RDFVocabulary.XSD.DECIMAL.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DOUBLE)) {
                return RDFVocabulary.XSD.DOUBLE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DURATION)) {
                return RDFVocabulary.XSD.DURATION.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_FLOAT)) {
                return RDFVocabulary.XSD.FLOAT.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GDAY)) {
                return RDFVocabulary.XSD.G_DAY.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTH)) {
                return RDFVocabulary.XSD.G_MONTH.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)) {
                return RDFVocabulary.XSD.G_MONTH_DAY.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEAR)) {
                return RDFVocabulary.XSD.G_YEAR.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)) {
                return RDFVocabulary.XSD.G_YEAR_MONTH.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)) {
                return RDFVocabulary.XSD.HEX_BINARY.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INT)) {
                return RDFVocabulary.XSD.INT.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER)) {
                return RDFVocabulary.XSD.INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)) {
                return RDFVocabulary.XSD.LANGUAGE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_LONG)) {
                return RDFVocabulary.XSD.LONG.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NAME)) {
                return RDFVocabulary.XSD.NAME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NCNAME)) {
                return RDFVocabulary.XSD.NCNAME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ID)) {
                return RDFVocabulary.XSD.ID.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)) {
                return RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)) {
                return RDFVocabulary.XSD.NMTOKEN.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)) {
                return RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)) {
                return RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)) {
                return RDFVocabulary.XSD.NORMALIZED_STRING.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NOTATION)) {
                return RDFVocabulary.XSD.NOTATION.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)) {
                return RDFVocabulary.XSD.POSITIVE_INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_QNAME)) {
                return RDFVocabulary.XSD.QNAME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_SHORT)) {
                return RDFVocabulary.XSD.SHORT.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_TIME)) {
                return RDFVocabulary.XSD.TIME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_TOKEN)) {
                return RDFVocabulary.XSD.TOKEN.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)) {
                return RDFVocabulary.XSD.UNSIGNED_BYTE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)) {
                return RDFVocabulary.XSD.UNSIGNED_INT.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)) {
                return RDFVocabulary.XSD.UNSIGNED_LONG.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)) {
                return RDFVocabulary.XSD.UNSIGNED_SHORT.ToString();
            }
            else {
                //Unknown datatypes default to instances of "rdfs:Literal"
                return RDFVocabulary.RDFS.LITERAL.ToString();
            }
        }

        /// <summary>
        /// Validates the value of the given typed literal against its datatype
        /// </summary>
        internal static Boolean ValidateTypedLiteral(RDFTypedLiteral typedLiteral) {
            if (typedLiteral != null) {

                #region STRING CATEGORY
                //LITERAL / STRING
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.RDFS_LITERAL) ||
                    typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))   {
                    return true;
                }

                //XML_LITERAL
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)) {
                    try {
                        XDocument.Parse(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //ANYURI
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ANYURI)) {
                    Uri outUri;
                    if (Uri.TryCreate(typedLiteral.Value, UriKind.Absolute, out outUri)) {
                        typedLiteral.Value = Convert.ToString(outUri);
                        return true;
                    }
                    return false;
                }

                //NAME
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NAME)) {
                    try {
                        XmlConvert.VerifyName(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //QNAME
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_QNAME)) {
                    var prefixedQName = typedLiteral.Value.Split(':');
                    if (prefixedQName.Length == 1) {
                        try {
                            XmlConvert.VerifyNCName(prefixedQName[0]);
                            return true;
                        }
                        catch {
                            return false;
                        }
                    }
                    else if(prefixedQName.Length == 2) {
                        try {
                            XmlConvert.VerifyNCName(prefixedQName[0]);
                            XmlConvert.VerifyNCName(prefixedQName[1]);
                            return true;
                        }
                        catch {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                }

                //NCNAME / ID
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NCNAME) ||
                    typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ID))     {
                    try {
                        XmlConvert.VerifyNCName(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //TOKEN
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_TOKEN)) {
                    try {
                        XmlConvert.VerifyTOKEN(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //NMTOKEN
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)) {
                    try {
                        XmlConvert.VerifyNMTOKEN(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //NORMALIZED_STRING
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)) {
                    if (typedLiteral.Value.IndexOfAny(new Char[] { '\n', '\r', '\t' }) == -1) {
                        return true;
                    }
                    else {
                         return false;
                    }
                }

                //LANGUAGE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)) {
                    if (Regex.IsMatch(typedLiteral.Value, "^[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$")) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //BASE64_BINARY
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)) {
                    try {
                        Convert.FromBase64String(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //HEX_BINARY
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)) {
                    if (Regex.IsMatch(typedLiteral.Value, @"^([0-9a-fA-F]{2})*$")) {
                        return true;
                    }
                    else {
                         return false;
                    }
                }
                #endregion

                #region BOOLEAN CATEGORY
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BOOLEAN)) {
                    Boolean outBool;
                    if (Boolean.TryParse(typedLiteral.Value, out outBool)) {
                        typedLiteral.Value = (outBool ? "true" : "false");
                    }
                    else {

                        //Even if lexical space of XSD:BOOLEAN allows 1/0,
                        //it must be converted to true/false value space
                        if (typedLiteral.Value.Equals("1")) {
                            typedLiteral.Value = "true";
                        }
                        else if(typedLiteral.Value.Equals("0")) {
                            typedLiteral.Value = "false";
                        }

                        else {
                            return false;
                        }
                    }
                    return true;
                }
                #endregion

                #region DATETIME CATEGORY
                //DATETIME
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DATETIME)) {
                    DateTime parsedDateTime;
                    if (DateTime.TryParseExact(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //DATE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DATE)) {
                    DateTime parsedDateTime;
                    if (DateTime.TryParseExact(typedLiteral.Value, "yyyy-MM-ddK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //TIME
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_TIME)) {
                    DateTime parsedDateTime;
                    if (DateTime.TryParseExact(typedLiteral.Value, "HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "HH:mm:ssK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //G_MONTH_DAY
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)) {
                    DateTime parsedDateTime;
                    if (DateTime.TryParseExact(typedLiteral.Value, "--MM-ddK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("--MM-dd", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "--MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("--MM-dd", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //G_YEAR_MONTH
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)) {
                    DateTime parsedDateTime;
                    if (DateTime.TryParseExact(typedLiteral.Value, "yyyy-MMK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy-MM", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy-MM", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

				//G_YEAR
				if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEAR)) {
                    DateTime parsedDateTime;
                    if (DateTime.TryParseExact(typedLiteral.Value, "yyyyK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("yyyy", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }
						
				//G_MONTH
				if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTH)) {
                    DateTime parsedDateTime;
                    if (DateTime.TryParseExact(typedLiteral.Value, "MMK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("MM", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "MM", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("MM", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }
						
				//G_DAY
				if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GDAY)) {
                    DateTime parsedDateTime;
                    if (DateTime.TryParseExact(typedLiteral.Value, "ddK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("dd", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else if (DateTime.TryParseExact(typedLiteral.Value, "dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDateTime)) {
                        typedLiteral.Value = parsedDateTime.ToString("dd", CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                #endregion

                #region TIMESPAN CATEGORY
                //DURATION
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DURATION)) {
                    try {
                        XmlConvert.ToTimeSpan(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }
                #endregion

                #region NUMERIC CATEGORY
                //DECIMAL
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DECIMAL)) {
                    Decimal outDecimal;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out outDecimal)) {
                        typedLiteral.Value = Convert.ToString(outDecimal, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //DOUBLE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DOUBLE)) {
                    Double outDouble;
                    if (Double.TryParse(typedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out outDouble)) {
                        typedLiteral.Value = Convert.ToString(outDouble, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //FLOAT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_FLOAT)) {
                    Single outFloat;
                    if (Single.TryParse(typedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out outFloat)) {
                        typedLiteral.Value = Convert.ToString(outFloat, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //INTEGER
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER)) {
                    Decimal outInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outInteger)) {
                        typedLiteral.Value = Convert.ToString(outInteger, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //LONG
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_LONG)) {
                    Int64 outLong;
                    if (Int64.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outLong)) {
                        typedLiteral.Value = Convert.ToString(outLong, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //INT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INT)) {
                    Int32 outInt;
                    if (Int32.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outInt)) {
                        typedLiteral.Value = Convert.ToString(outInt, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //SHORT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_SHORT)) {
                    Int16 outShort;
                    if (Int16.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outShort)) {
                        typedLiteral.Value = Convert.ToString(outShort, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //BYTE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BYTE)) {
                    SByte outSByte;
                    if (SByte.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outSByte)) {
                        typedLiteral.Value = Convert.ToString(outSByte, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //UNSIGNED LONG
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)) {
                    UInt64 outULong;
                    if (UInt64.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outULong)) {
                        typedLiteral.Value = Convert.ToString(outULong, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //UNSIGNED INT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)) {
                    UInt32 outUInt;
                    if (UInt32.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outUInt)) {
                        typedLiteral.Value = Convert.ToString(outUInt, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //UNSIGNED SHORT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT)) {
                    UInt16 outUShort;
                    if (UInt16.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outUShort)) {
                        typedLiteral.Value = Convert.ToString(outUShort, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //UNSIGNED BYTE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)) {
                    Byte outByte;
                    if (Byte.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outByte)) {
                        typedLiteral.Value = Convert.ToString(outByte, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //NON-POSITIVE INTEGER [Decimal.MinValue, 0]
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER)) {
                    Decimal outNPInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outNPInteger)) {
                        typedLiteral.Value = Convert.ToString(outNPInteger, CultureInfo.InvariantCulture);
                        if (outNPInteger > 0) {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                    return true;
                }

                //NEGATIVE INTEGER [Decimal.MinValue, -1]
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)) {
                    Decimal outNInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outNInteger)) {
                        typedLiteral.Value = Convert.ToString(outNInteger, CultureInfo.InvariantCulture);
                        if (outNInteger > -1) {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                    return true;
                }

                //NON-NEGATIVE INTEGER [0, Decimal.MaxValue]
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER)) {
                    Decimal outNNInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outNNInteger)) {
                        typedLiteral.Value = Convert.ToString(outNNInteger, CultureInfo.InvariantCulture);
                        if (outNNInteger < 0) {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                    return true;
                }

                //POSITIVE INTEGER [1, Decimal.MaxValue]
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)) {
                    Decimal outPInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outPInteger)) {
                        typedLiteral.Value = Convert.ToString(outPInteger, CultureInfo.InvariantCulture);
                        if (outPInteger < 1) {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                    return true;
                }
                #endregion

            }
            throw new RDFModelException("Cannot validate RDFTypedLiteral because given \"typedLiteral\" parameter is null.");
        }
        #endregion

    }

}