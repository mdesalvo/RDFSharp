/*
   Copyright 2012-2016 Marco De Salvo

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
    internal static class RDFModelUtilities {

        #region Greta
        /// <summary>
        /// Performs MD5 hash calculation of the given string
        /// </summary>
        internal static Int64 CreateHash(String input) {
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
        /// It starts by default with "bnode:".
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
                    foreach (var t in graph.GraphIndex.SelectIndexBySubject(subj).Keys) {
                        matchSubj.Add(graph.Triples[t]);
                    }
                }

                //Filter by Predicate
                if (pred        != null) {
                    foreach (var t in graph.GraphIndex.SelectIndexByPredicate(pred).Keys) {
                        matchPred.Add(graph.Triples[t]);
                    }
                }

                //Filter by Object
                if (obj         != null) {
                    foreach (var t in graph.GraphIndex.SelectIndexByObject(obj).Keys) {
                        matchObj.Add(graph.Triples[t]);
                    }
                }

                //Filter by Literal
                if (lit         != null) {
                    foreach (var t in graph.GraphIndex.SelectIndexByLiteral(lit).Keys) {
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
                                                       .ToList<RDFTriple>();
                        }
                        else {
                            if (lit        != null) {
                                //S->P->L
                                matchResult = matchSubj.Intersect(matchPred)
                                                       .Intersect(matchLit)
                                                       .ToList<RDFTriple>();
                            }
                            else {
                                //S->P->
                                matchResult = matchSubj.Intersect(matchPred)
                                                       .ToList<RDFTriple>();
                            }
                        }
                    }
                    else {
                        if (obj            != null) {
                            //S->->O
                            matchResult     = matchSubj.Intersect(matchObj)
                                                       .ToList<RDFTriple>();
                        }
                        else {
                            if (lit        != null) {
                                //S->->L
                                matchResult = matchSubj.Intersect(matchLit)
                                                       .ToList<RDFTriple>();
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
                                                       .ToList<RDFTriple>();
                        }
                        else {
                            if (lit        != null) {
                                //->P->L
                                matchResult = matchPred.Intersect(matchLit)
                                                       .ToList<RDFTriple>();
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
                                matchResult = graph.Triples.Values.ToList<RDFTriple>();
                            }
                        }
                    }
                }

            }
            return matchResult;
        }
        #endregion

        #region RDFNamespace
        /// <summary>
        /// Looksup the given prefix or namespace into the prefix.cc service
        /// </summary>
        internal static RDFNamespace LookupPrefixCC(String data, Int32 lookupMode) {
            var lookupString       = (lookupMode == 1 ? "http://prefix.cc/" + data + ".file.txt" :
                                                        "http://prefix.cc/reverse?uri=" + data + "&format=txt");

            using (var webclient   = new WebClient()) {
                try {
                    var response   = webclient.DownloadString(lookupString);
                    var new_prefix = response.Split('\t')[0];
                    var new_nspace = response.Split('\t')[1].TrimEnd(new Char[] { '\n' });
                    var result     = new RDFNamespace(new_prefix, new_nspace);

                    //Also add the namespace to the register, to avoid future lookups
                    RDFNamespaceRegister.AddNamespace(result);

                    //Return the found result
                    return result;
                }
                catch  (WebException wex) {
                    if (wex.Message.Contains("404")) {
                        return null;
                    }
                    else {
                        throw new RDFModelException("Cannot retrieve data from prefix.cc service because: " + wex.Message, wex);
                    }
                }
                catch(Exception ex) {
                    throw new RDFModelException("Cannot retrieve data from prefix.cc service because: " + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Finds if the given token contains a recognizable namespace and, if so, abbreviates it with its prefix.
        /// It also prepares the result in a format useful for serialization (it's used by Turtle writer).
        /// </summary>
        internal static String AbbreviateNamespace(String token) {

            //Null or Space token: it's a trick, give empty result
            if (token == null || token.Trim() == String.Empty) {
                return String.Empty;
            }
            //Blank token: abbreviate it with "_"
            if (token.StartsWith("bnode:")) {
                return token.Replace("bnode:", "_:");
            }
            //Prefixed token: check if it starts with a known prefix, if so just return it
            if (RDFNamespaceRegister.GetByPrefix(token.Split(':')[0]) != null) {
                return token;
            }

            //Uri token: search a known namespace, if found replace it with its prefix
            Boolean abbreviationDone     = false;
            RDFNamespaceRegister.Instance.Register.ForEach(ns => {
                if (!abbreviationDone) {
                    String nS            = ns.ToString();
                    if (token.Contains(nS)) {
                        token            = token.Replace(nS, ns.Prefix + ":").TrimEnd(new Char[] { '/' });
                        abbreviationDone = true;
                    }
                }
            });

            //Search done, let's analyze results:
            if (abbreviationDone) {
                return token; //token is a relative or a blank uri
            }
            if (token.Contains("^^")) { //token is a typedLiteral absolute uri
                return token.Replace("^^", "^^<") + ">";
            }
            return "<" + token + ">"; //token is an absolute uri

        }

        /// <summary>
        /// Generates an automatic prefix for a namespace
        /// </summary>
        internal static RDFNamespace GenerateNamespace(String namespaceString, Boolean isDatatypeNamespace) {
            if (namespaceString    != null && namespaceString.Trim() != String.Empty) {
                
                //Extract the prefixable part from the Uri
                Uri uriNS           = GetUriFromString(namespaceString);
                if (uriNS          == null) {
                    throw new RDFModelException("Cannot create RDFNamespace because given \"namespaceString\" (" + namespaceString + ") parameter cannot be converted to a valid Uri");
                }
                String type         = null;
                String ns           = uriNS.AbsoluteUri;

                // e.g.:  "http://www.w3.org/2001/XMLSchema#integer"
                if (uriNS.Fragment != String.Empty) {
                    type            = uriNS.Fragment.Replace("#", String.Empty);  //"integer"
                    if (type       != String.Empty) {
                        ns          = ns.TrimEnd(type.ToCharArray());             //"http://www.w3.org/2001/XMLSchema#"
                    }
                }
                else {
                    // e.g.:  "http://example.org/integer"
                    if (uriNS.LocalPath != "/") {
                        if (!isDatatypeNamespace) {
                            ns      = ns.TrimEnd(uriNS.Segments[uriNS.Segments.Length-1].ToCharArray());
                        }
                    }
                }

                //Check if a namespace with the extracted Uri is in the register, or generate an automatic one
                return (RDFNamespaceRegister.GetByNamespace(ns) ?? new RDFNamespace("autoNS", ns));

            }
            throw new RDFModelException("Cannot create RDFNamespace because given \"namespaceString\" parameter is null or empty");
        }
        #endregion

        #region RDFDatatype
        /// <summary>
        /// Parse the given string in order to give the corresponding RDF/RDFS/XSD datatype
        /// </summary>
        internal static RDFModelEnums.RDFDatatype GetDatatypeFromString(String datatypeString) {
            if (datatypeString != null) {

                //Preliminary check to verify if datatypeString is a valid Uri
                Uri dtypeStringUri;
                if (!Uri.TryCreate(datatypeString.Trim(), UriKind.Absolute, out dtypeStringUri)) {
                     throw new RDFModelException("Cannot recognize datatype representation of given \"datatypeString\" parameter because it is not a valid Uri.");
                }

                //Identification of specific RDF/RDFS/XSD datatype
                if (dtypeStringUri.ToString().Equals(RDFVocabulary.RDF.XML_LITERAL.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.RDF_XMLLITERAL;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.ANY_URI.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_ANYURI;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.BASE64_BINARY.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_BASE64BINARY;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.BOOLEAN.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_BOOLEAN;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.BYTE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_BYTE;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.DATE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_DATE;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.DATETIME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_DATETIME;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.DECIMAL.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_DECIMAL;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.DOUBLE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_DOUBLE;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.DURATION.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_DURATION;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.FLOAT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_FLOAT;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.G_DAY.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_GDAY;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.G_MONTH.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_GMONTH;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.G_MONTH_DAY.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_GMONTHDAY;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.G_YEAR.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_GYEAR;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.G_YEAR_MONTH.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_GYEARMONTH;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.HEX_BINARY.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_HEXBINARY;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.INT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_INT;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_INTEGER;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.LANGUAGE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_LANGUAGE;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.LONG.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_LONG;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.NAME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_NAME;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.NCNAME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_NCNAME;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_NEGATIVEINTEGER;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.NMTOKEN.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_NMTOKEN;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_NONNEGATIVEINTEGER;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_NONPOSITIVEINTEGER;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.NORMALIZED_STRING.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_NORMALIZEDSTRING;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.NOTATION.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_NOTATION;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.POSITIVE_INTEGER.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_POSITIVEINTEGER;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.QNAME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_QNAME;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.SHORT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_SHORT;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.STRING.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_STRING;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.TIME.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_TIME;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.TOKEN.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_TOKEN;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.UNSIGNED_BYTE.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_UNSIGNEDBYTE;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.UNSIGNED_INT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_UNSIGNEDINT;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.UNSIGNED_LONG.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_UNSIGNEDLONG;
                }
                else if (dtypeStringUri.ToString().Equals(RDFVocabulary.XSD.UNSIGNED_SHORT.ToString(), StringComparison.Ordinal)) {
                    return RDFModelEnums.RDFDatatype.XSD_UNSIGNEDSHORT;
                }
                else {
                    return RDFModelEnums.RDFDatatype.RDFS_LITERAL; //Default unsupported datatypes to rdfs:Literal
                }

            }
            throw new RDFModelException("Cannot recognize datatype representation of given \"datatypeString\" parameter because it is null.");
        }

        /// <summary>
        /// Gives the string representation of the given RDF/RDFS/XSD datatype
        /// </summary>
        internal static String GetDatatypeFromEnum(RDFModelEnums.RDFDatatype datatype) {
            if (datatype.Equals(RDFModelEnums.RDFDatatype.RDF_XMLLITERAL)) {
                return RDFVocabulary.RDF.XML_LITERAL.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_ANYURI)) {
                return RDFVocabulary.XSD.ANY_URI.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_BASE64BINARY)) {
                return RDFVocabulary.XSD.BASE64_BINARY.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_BOOLEAN)) {
                return RDFVocabulary.XSD.BOOLEAN.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_BYTE)) {
                return RDFVocabulary.XSD.BYTE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DATE)) {
                return RDFVocabulary.XSD.DATE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DATETIME)) {
                return RDFVocabulary.XSD.DATETIME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DECIMAL)) {
                return RDFVocabulary.XSD.DECIMAL.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DOUBLE)) {
                return RDFVocabulary.XSD.DOUBLE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DURATION)) {
                return RDFVocabulary.XSD.DURATION.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_FLOAT)) {
                return RDFVocabulary.XSD.FLOAT.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GDAY)) {
                return RDFVocabulary.XSD.G_DAY.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GMONTH)) {
                return RDFVocabulary.XSD.G_MONTH.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GMONTHDAY)) {
                return RDFVocabulary.XSD.G_MONTH_DAY.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GYEAR)) {
                return RDFVocabulary.XSD.G_YEAR.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GYEARMONTH)) {
                return RDFVocabulary.XSD.G_YEAR_MONTH.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_HEXBINARY)) {
                return RDFVocabulary.XSD.HEX_BINARY.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_INT)) {
                return RDFVocabulary.XSD.INT.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_INTEGER)) {
                return RDFVocabulary.XSD.INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_LANGUAGE)) {
                return RDFVocabulary.XSD.LANGUAGE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_LONG)) {
                return RDFVocabulary.XSD.LONG.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NAME)) {
                return RDFVocabulary.XSD.NAME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NCNAME)) {
                return RDFVocabulary.XSD.NCNAME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NEGATIVEINTEGER)) {
                return RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NMTOKEN)) {
                return RDFVocabulary.XSD.NMTOKEN.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NONNEGATIVEINTEGER)) {
                return RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NONPOSITIVEINTEGER)) {
                return RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NORMALIZEDSTRING)) {
                return RDFVocabulary.XSD.NORMALIZED_STRING.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NOTATION)) {
                return RDFVocabulary.XSD.NOTATION.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_POSITIVEINTEGER)) {
                return RDFVocabulary.XSD.POSITIVE_INTEGER.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_QNAME)) {
                return RDFVocabulary.XSD.QNAME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_SHORT)) {
                return RDFVocabulary.XSD.SHORT.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_STRING)) {
                return RDFVocabulary.XSD.STRING.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_TIME)) {
                return RDFVocabulary.XSD.TIME.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_TOKEN)) {
                return RDFVocabulary.XSD.TOKEN.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_UNSIGNEDBYTE)) {
                return RDFVocabulary.XSD.UNSIGNED_BYTE.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_UNSIGNEDINT)) {
                return RDFVocabulary.XSD.UNSIGNED_INT.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_UNSIGNEDLONG)) {
                return RDFVocabulary.XSD.UNSIGNED_LONG.ToString();
            }
            else if (datatype.Equals(RDFModelEnums.RDFDatatype.XSD_UNSIGNEDSHORT)) {
                return RDFVocabulary.XSD.UNSIGNED_SHORT.ToString();
            }
            else {
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
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.RDFS_LITERAL) ||
                    typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_STRING))   {
                    return true;
                }

                //XML_LITERAL
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.RDF_XMLLITERAL)) {
                    try {
                        XDocument.Parse(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //ANYURI
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_ANYURI)) {
                    Uri outUri;
                    if (Uri.TryCreate(typedLiteral.Value, UriKind.Absolute, out outUri)) {
                        return true;
                    }
                    return false;
                }

                //NAME
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NAME)) {
                    try {
                        XmlConvert.VerifyName(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //NCNAME
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NCNAME)) {
                    try {
                        XmlConvert.VerifyNCName(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //TOKEN
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_TOKEN)) {
                    try {
                        XmlConvert.VerifyTOKEN(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //NMTOKEN
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NMTOKEN)) {
                    try {
                        XmlConvert.VerifyNMTOKEN(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //NORMALIZED_STRING
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NORMALIZEDSTRING)) {
                    if (!typedLiteral.Value.Contains('\r') && typedLiteral.Value.Contains('\n') && typedLiteral.Value.Contains('\t')) {
                         return true;
                    }
                    else {
                         return false;
                    }
                }

                //LANGUAGE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_LANGUAGE)) {
                    if (Regex.IsMatch(typedLiteral.Value, "^[a-zA-Z]+([\\-][a-zA-Z0-9]+)*$")) {
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //BASE64_BINARY
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_BASE64BINARY)) {
                    try {
                        Convert.FromBase64String(typedLiteral.Value);
                        return true;
                    }
                    catch {
                        return false;
                    }
                }

                //HEX_BINARY
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_HEXBINARY)) {
                    if ((typedLiteral.Value.Length % 2 == 0) && Regex.IsMatch(typedLiteral.Value, @"^[a-fA-F0-9]+$")) {
                         return true;
                    }
                    else {
                         return false;
                    }
                }
                #endregion

                #region BOOLEAN CATEGORY
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_BOOLEAN)) {
                    Boolean outBool;
                    if (Boolean.TryParse(typedLiteral.Value, out outBool)) {
                        typedLiteral.Value = (outBool ? "true" : "false");
                    }
                    else {
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
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DATETIME)) {
                    try {
                        DateTime.ParseExact(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.FFFK", CultureInfo.InvariantCulture);
                    }
                    catch {
                        try {
                            DateTime.ParseExact(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.FFF", CultureInfo.InvariantCulture);
                        }
                        catch {
                            try {
                                DateTime.ParseExact(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                try {
                                    DateTime.ParseExact(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                                }
                                catch {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }

                //DATE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DATE)) {
                    try {
                        DateTime.ParseExact(typedLiteral.Value, "yyyy-MM-ddK", CultureInfo.InvariantCulture);
                    }
                    catch {
                        try {
                            DateTime.ParseExact(typedLiteral.Value, "yyyy-MM-dd",  CultureInfo.InvariantCulture);
                        }
                        catch {
                            return false;
                        }
                    }
                    return true;
                }

                //TIME
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_TIME)) {
                    try {
                        DateTime.ParseExact(typedLiteral.Value, "HH:mm:ss.FFFK", CultureInfo.InvariantCulture);
                    }
                    catch {
                        try {
                            DateTime.ParseExact(typedLiteral.Value, "HH:mm:ss.FFF", CultureInfo.InvariantCulture);
                        }
                        catch {
                            try {
                                DateTime.ParseExact(typedLiteral.Value, "HH:mm:ssK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                try {
                                    DateTime.ParseExact(typedLiteral.Value, "HH:mm:ss", CultureInfo.InvariantCulture);
                                }
                                catch {
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }

                //G_MONTH_DAY
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GMONTHDAY)) {
                    try {
                        DateTime.ParseExact(typedLiteral.Value, "--MM-ddK", CultureInfo.InvariantCulture);
                    }
                    catch {
                        try {
                            DateTime.ParseExact(typedLiteral.Value, "--MM-dd",  CultureInfo.InvariantCulture);
                        }
                        catch {
                            return false;
                        }
                    }
                    return true;
                }

                //G_YEAR_MONTH
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GYEARMONTH)) {
                    try {
                        DateTime.ParseExact(typedLiteral.Value, "yyyy-MMK", CultureInfo.InvariantCulture);
                    }
                    catch {
                        try {
                            DateTime.ParseExact(typedLiteral.Value, "yyyy-MM",  CultureInfo.InvariantCulture);
                        }
                        catch {
                            return false;
                        }
                    }
                    return true;
                }

				//G_YEAR
				if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GYEAR)) {
					try {
                        DateTime.ParseExact(typedLiteral.Value, "yyyyK", CultureInfo.InvariantCulture);
                    }
                    catch {
                        try {
                            DateTime.ParseExact(typedLiteral.Value, "yyyy",  CultureInfo.InvariantCulture);
                        }
                        catch {
                            return false;
                        }
                    }
                    return true;
				}
						
				//G_MONTH
				if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GMONTH)) {
					try {
                        DateTime.ParseExact(typedLiteral.Value, "MMK", CultureInfo.InvariantCulture);
                    }
                    catch {
                        try {
                            DateTime.ParseExact(typedLiteral.Value, "MM",  CultureInfo.InvariantCulture);
                        }
                        catch {
                            return false;
                        }
                    }
                    return true;
				}
						
				//G_DAY
				if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_GDAY)) {
					try {
                        DateTime.ParseExact(typedLiteral.Value, "ddK", CultureInfo.InvariantCulture);
                    }
                    catch {
                        try {
                            DateTime.ParseExact(typedLiteral.Value, "dd",  CultureInfo.InvariantCulture);
                        }
                        catch {
                            return false;
                        }
                    }
                    return true;
				}
                #endregion

                #region TIMESPAN CATEGORY
                //DURATION
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DURATION)) {
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
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DECIMAL)) {
                    Decimal outDecimal;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out outDecimal)) {
                        typedLiteral.Value = outDecimal.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //DOUBLE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_DOUBLE)) {
                    Double outDouble;
                    if (Double.TryParse(typedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out outDouble)) {
                        typedLiteral.Value = outDouble.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //FLOAT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_FLOAT)) {
                    Single outFloat;
                    if (Single.TryParse(typedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out outFloat)) {
                        typedLiteral.Value = outFloat.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //INTEGER
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_INTEGER)) {
                    Decimal outInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outInteger)) {
                        typedLiteral.Value = outInteger.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //LONG
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_LONG)) {
                    Int64 outLong;
                    if (Int64.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outLong)) {
                        typedLiteral.Value = outLong.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //INT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_INT)) {
                    Int32 outInt;
                    if (Int32.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outInt)) {
                        typedLiteral.Value = outInt.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //SHORT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_SHORT)) {
                    Int16 outShort;
                    if (Int16.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outShort)) {
                        typedLiteral.Value = outShort.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //BYTE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_BYTE)) {
                    SByte outSByte;
                    if (SByte.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outSByte)) {
                        typedLiteral.Value = outSByte.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //UNSIGNED LONG
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_UNSIGNEDLONG)) {
                    UInt64 outULong;
                    if (UInt64.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outULong)) {
                        typedLiteral.Value = outULong.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //UNSIGNED INT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_UNSIGNEDINT)) {
                    UInt32 outUInt;
                    if (UInt32.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outUInt)) {
                        typedLiteral.Value = outUInt.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //UNSIGNED SHORT
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_UNSIGNEDSHORT)) {
                    UInt16 outUShort;
                    if (UInt16.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outUShort)) {
                        typedLiteral.Value = outUShort.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //UNSIGNED BYTE
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_UNSIGNEDBYTE)) {
                    Byte outByte;
                    if (Byte.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outByte)) {
                        typedLiteral.Value = outByte.ToString(CultureInfo.InvariantCulture);
                        return true;
                    }
                    else {
                        return false;
                    }
                }

                //NON-POSITIVE INTEGER [Decimal.MinValue, 0]
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NONPOSITIVEINTEGER)) {
                    Decimal outNPInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outNPInteger)) {
                        typedLiteral.Value = outNPInteger.ToString(CultureInfo.InvariantCulture);
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
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NEGATIVEINTEGER)) {
                    Decimal outNInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outNInteger)) {
                        typedLiteral.Value = outNInteger.ToString(CultureInfo.InvariantCulture);
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
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_NONNEGATIVEINTEGER)) {
                    Decimal outNNInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outNNInteger)) {
                        typedLiteral.Value = outNNInteger.ToString(CultureInfo.InvariantCulture);
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
                if (typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatype.XSD_POSITIVEINTEGER)) {
                    Decimal outPInteger;
                    if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out outPInteger)) {
                        typedLiteral.Value = outPInteger.ToString(CultureInfo.InvariantCulture);
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