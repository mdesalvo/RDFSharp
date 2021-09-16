/*
   Copyright 2012-2021 Marco De Salvo

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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFModelUtilities is a collector of reusable utility methods for RDF model management
    /// </summary>
    public static class RDFModelUtilities
    {
        #region Hashing
        /// <summary>
        /// Creates a unique long representation of the given string
        /// </summary>
        public static long CreateHash(string input)
        {
            if (input == null)
                throw new RDFModelException("Cannot create hash because given \"input\" string parameter is null.");

            using (MD5CryptoServiceProvider md5Encryptor = new MD5CryptoServiceProvider())
            {
                byte[] hashBytes = md5Encryptor.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToInt64(hashBytes, 0);
            }
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
        /// Regex to catch xsd:hexBinary typed literals
        /// </summary>
        internal static readonly Regex hexBinary = new Regex(@"^([0-9a-fA-F]{2})*$", RegexOptions.Compiled);

        /// <summary>
        /// Alternative representations of boolean True
        /// </summary>
        internal static string[] AlternativesBoolTrue = new string[] { "1", "one", "yes", "y", "on", "ok" };
        /// <summary>
        /// Alternative representations of boolean False
        /// </summary>
        internal static string[] AlternativesBoolFalse = new string[] { "0", "zero", "no", "n", "off", "ko" };

        /// <summary>
        /// Gets the Uri corresponding to the given string
        /// </summary>
        internal static Uri GetUriFromString(string uriString)
        {
            // blank node detection and normalization
            if (uriString?.StartsWith("bnode:", StringComparison.OrdinalIgnoreCase) ?? false)
                uriString = string.Concat("bnode:", uriString.Substring(6));
            else if (uriString?.StartsWith("_:") ?? false)
                uriString = string.Concat("bnode:", uriString.Substring(2));

            Uri.TryCreate(uriString, UriKind.Absolute, out Uri tempUri);
            return tempUri;
        }

        /// <summary>
        /// Searches the given Uri in the namespace register for getting its dereferenceable representation;<br/>
        /// if not found, just returns the given Uri
        /// </summary>
        internal static Uri RemapUriForDereference(Uri uri)
        {
            string uriString = uri?.ToString() ?? string.Empty;

            RDFNamespace rdfNamespace = RDFNamespaceRegister.GetByUri(uriString);
            if (rdfNamespace != null)
                return rdfNamespace.DereferenceUri;

            return uri;
        }

        /// <summary>
        /// Turns back ASCII-encoded Unicodes into Unicodes.
        /// </summary>
        public static string ASCII_To_Unicode(string asciiString)
        {
            if (string.IsNullOrEmpty(asciiString))
                return asciiString;

            //UNICODE (UTF-16)
            StringBuilder sbRegexU8 = new StringBuilder();
            sbRegexU8.Append(regexU8.Replace(asciiString, match => char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, NumberStyles.HexNumber))));

            //UNICODE (UTF-8)
            StringBuilder sbRegexU4 = new StringBuilder();
            sbRegexU4.Append(regexU4.Replace(sbRegexU8.ToString(), match => char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, NumberStyles.HexNumber))));

            return sbRegexU4.ToString();
        }

        /// <summary>
        /// Turns Unicodes into ASCII-encoded Unicodes.
        /// </summary>
        public static string Unicode_To_ASCII(string unicodeString)
        {
            if (string.IsNullOrEmpty(unicodeString))
                return unicodeString;

            //https://docs.microsoft.com/en-us/dotnet/api/system.text.rune?view=net-5.0&viewFallbackFrom=netstandard-2.0
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < unicodeString.Length; i++)
            {
                //ASCII
                if (unicodeString[i] <= 127)
                    b.Append(unicodeString[i]);

                //UNICODE (UTF-8)
                else if (!char.IsSurrogate(unicodeString[i]))
                    b.Append(string.Concat("\\u", ((int)unicodeString[i]).ToString("X4")));

                //UNICODE (UTF-16)
                else if (i + 1 < unicodeString.Length && char.IsSurrogatePair(unicodeString[i], unicodeString[i + 1]))
                {
                    int codePoint = char.ConvertToUtf32(unicodeString[i], unicodeString[i+1]);
                    b.Append(string.Concat("\\U", codePoint.ToString("X8")));
                    i++;
                }

                //ERROR
                else
                    throw new RDFModelException("Cannot convert string '" + unicodeString + "' to ASCII because it is not well-formed UTF-16");
            }
            return b.ToString();
        }

        /// <summary>
        /// Replaces character controls for XML compatibility
        /// </summary>
        internal static string EscapeControlCharsForXML(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            StringBuilder b = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (char.IsControl(data[i]) && data[i] != '\u0009' && data[i] != '\u000A' && data[i] != '\u000D')
                    b.Append(string.Concat("\\u", ((int)data[i]).ToString("X4")));
                else
                    b.Append(data[i]);
            }
            return b.ToString();
        }

        /// <summary>
        /// Trims the end of the given source string searching for the given value
        /// </summary>
        internal static string TrimEnd(this string source, string value)
        {
            if (string.IsNullOrEmpty(source) 
                    || string.IsNullOrEmpty(value) 
                        || !source.EndsWith(value))
                return source;

            return source.Remove(source.LastIndexOf(value));
        }
        
        /// <summary>
        /// Gets the short representation of the given Uri
        /// </summary>
        internal static string GetShortUri(this Uri uri)
        {
            if (uri == null)
                return null;

            string shortUri = uri.ToString();
            if (!string.IsNullOrEmpty(uri.Fragment))
                shortUri = uri.Fragment.TrimStart(new char[] { '#' });
            else if (uri.Segments.Length > 1)
                shortUri = uri.Segments.Last();
            return shortUri;
        }
        #endregion

        #region Graph
        /// <summary>
        /// Selects the triples corresponding to the given pattern from the given graph
        /// </summary>
        internal static List<RDFTriple> SelectTriples(RDFGraph graph, RDFResource subj, RDFResource pred, RDFResource obj, RDFLiteral lit)
        {
            List<RDFTriple> matchResult = new List<RDFTriple>();
            if (graph != null)
            {
                List<RDFTriple> S = new List<RDFTriple>();
                List<RDFTriple> P = new List<RDFTriple>();
                List<RDFTriple> O = new List<RDFTriple>();
                List<RDFTriple> L = new List<RDFTriple>();
                StringBuilder queryFilters = new StringBuilder();

                //Filter by Subject
                if (subj != null)
                {
                    queryFilters.Append("S");
                    foreach (long t in graph.GraphIndex.SelectIndexBySubject(subj))
                        S.Add(graph.Triples[t]);
                }

                //Filter by Predicate
                if (pred != null)
                {
                    queryFilters.Append("P");
                    foreach (long t in graph.GraphIndex.SelectIndexByPredicate(pred))
                        P.Add(graph.Triples[t]);
                }

                //Filter by Object
                if (obj != null)
                {
                    queryFilters.Append("O");
                    foreach (long t in graph.GraphIndex.SelectIndexByObject(obj))
                        O.Add(graph.Triples[t]);
                }

                //Filter by Literal
                if (lit != null)
                {
                    queryFilters.Append("L");
                    foreach (long t in graph.GraphIndex.SelectIndexByLiteral(lit))
                        L.Add(graph.Triples[t]);
                }

                //Intersect the filters
                string queryFilter = queryFilters.ToString();
                switch (queryFilter)
                {
                    case "S":
                        matchResult = S;
                        break;
                    case "P":
                        matchResult = P;
                        break;
                    case "O":
                        matchResult = O;
                        break;
                    case "L":
                        matchResult = L;
                        break;
                    case "SP":
                        matchResult = S.Intersect(P).ToList();
                        break;
                    case "SO":
                        matchResult = S.Intersect(O).ToList();
                        break;
                    case "SL":
                        matchResult = S.Intersect(L).ToList();
                        break;
                    case "PO":
                        matchResult = P.Intersect(O).ToList();
                        break;
                    case "PL":
                        matchResult = P.Intersect(L).ToList();
                        break;
                    case "SPO":
                        matchResult = S.Intersect(P).Intersect(O).ToList();
                        break;
                    case "SPL":
                        matchResult = S.Intersect(P).Intersect(L).ToList();
                        break;
                    default:
                        matchResult = graph.ToList();
                        break;
                }
            }
            return matchResult;
        }
        #endregion

        #region Collections
        /// <summary>
        /// Rebuilds the collection represented by the given resource within the given graph
        /// </summary>
        internal static RDFCollection DeserializeCollectionFromGraph(RDFGraph graph,
                                                                     RDFResource collRepresentative,
                                                                     RDFModelEnums.RDFTripleFlavors expectedFlavor)
        {
            RDFCollection collection = new RDFCollection(expectedFlavor == RDFModelEnums.RDFTripleFlavors.SPO ? RDFModelEnums.RDFItemTypes.Resource :
                                                                                                                RDFModelEnums.RDFItemTypes.Literal);
            RDFGraph rdfFirst = graph.SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST);
            RDFGraph rdfRest = graph.SelectTriplesByPredicate(RDFVocabulary.RDF.REST);

            #region Deserialization
            bool nilFound = false;
            RDFResource itemRest = collRepresentative;
            HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
            while (!nilFound)
            {
                #region rdf:first
                RDFTriple first = rdfFirst.SelectTriplesBySubject(itemRest).FirstOrDefault();
                if (first != null && first.TripleFlavor == expectedFlavor)
                {
                    if (expectedFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        collection.AddItem((RDFResource)first.Object);
                    else
                        collection.AddItem((RDFLiteral)first.Object);
                }
                else
                {
                    nilFound = true;
                }
                #endregion

                #region rdf:rest
                //Ensure considering exit signal from bad-formed rdf:first
                if (!nilFound)
                {
                    RDFTriple rest = rdfRest.SelectTriplesBySubject(itemRest).FirstOrDefault();
                    if (rest != null)
                    {
                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                            nilFound = true;
                        else
                        {
                            itemRest = (RDFResource)rest.Object;
                            //Avoid bad-formed cyclic lists to generate infinite loops
                            if (!itemRestVisitCache.Contains(itemRest.PatternMemberID))
                                itemRestVisitCache.Add(itemRest.PatternMemberID);
                            else
                                nilFound = true;
                        }
                    }
                    else
                    {
                        nilFound = true;
                    }
                }
                #endregion
            }
            #endregion

            return collection;
        }

        /// <summary>
        /// Detects the flavor (SPO/SPL) of the collection represented by the given resource within the given graph
        /// </summary>
        internal static RDFModelEnums.RDFTripleFlavors DetectCollectionFlavorFromGraph(RDFGraph graph,
                                                                                       RDFResource collRepresentative)
        {
            return graph.SelectTriplesBySubject(collRepresentative)
                        .SelectTriplesByPredicate(RDFVocabulary.RDF.FIRST)
                        .FirstOrDefault()
                       ?.TripleFlavor ?? RDFModelEnums.RDFTripleFlavors.SPO;
        }
        #endregion

        #region Namespaces
        /// <summary>
        /// Gets the list of namespaces used within the triples of the given graph
        /// </summary>
        internal static List<RDFNamespace> GetGraphNamespaces(RDFGraph graph)
        {
            List<RDFNamespace> result = new List<RDFNamespace>();
            foreach (RDFTriple t in graph)
            {
                string subj = t.Subject.ToString();
                string pred = t.Predicate.ToString();
                string obj = t.Object is RDFResource ? t.Object.ToString() :
                                (t.Object is RDFTypedLiteral ? GetDatatypeFromEnum(((RDFTypedLiteral)t.Object).Datatype) : string.Empty);

                //Resolve subject Uri
                IEnumerable<RDFNamespace> subjNS = RDFNamespaceRegister.Instance.Register.Where(ns => subj.StartsWith(ns.ToString()));
                result.AddRange(subjNS);

                //Resolve predicate Uri
                IEnumerable<RDFNamespace> predNS = RDFNamespaceRegister.Instance.Register.Where(ns => pred.StartsWith(ns.ToString()));
                result.AddRange(predNS);

                //Resolve object Uri
                IEnumerable<RDFNamespace> objNS = RDFNamespaceRegister.Instance.Register.Where(ns => obj.StartsWith(ns.ToString()));
                result.AddRange(objNS);
            }
            return result.Distinct().ToList();
        }
        #endregion

        #region Datatypes
        /// <summary>
        /// Parses the given string in order to give the corresponding RDF/RDFS/XSD datatype
        /// </summary>
        public static RDFModelEnums.RDFDatatypes GetDatatypeFromString(string datatypeString)
        {
            if (datatypeString == null)
                throw new RDFModelException("Cannot recognize datatype representation of given \"datatypeString\" parameter because it is null.");
            if (!Uri.TryCreate(datatypeString.Trim(), UriKind.Absolute, out Uri dtypeStringUri))
                throw new RDFModelException("Cannot recognize datatype representation of given \"datatypeString\" parameter because it is not a valid absolute Uri.");

            //Identification of specific RDF/RDFS/XSD datatypes
            datatypeString = dtypeStringUri.ToString();
            if (datatypeString.Equals(RDFVocabulary.RDFS.LITERAL.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.RDFS_LITERAL;
            else if (datatypeString.Equals(RDFVocabulary.XSD.STRING.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_STRING;
            else if (datatypeString.Equals(RDFVocabulary.XSD.BOOLEAN.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_BOOLEAN;
            else if (datatypeString.Equals(RDFVocabulary.XSD.DECIMAL.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_DECIMAL;
            else if (datatypeString.Equals(RDFVocabulary.XSD.DOUBLE.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_DOUBLE;
            else if (datatypeString.Equals(RDFVocabulary.XSD.FLOAT.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_FLOAT;
            else if (datatypeString.Equals(RDFVocabulary.XSD.INTEGER.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_INTEGER;
            else if (datatypeString.Equals(RDFVocabulary.XSD.LONG.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_LONG;
            else if (datatypeString.Equals(RDFVocabulary.XSD.INT.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_INT;
            else if (datatypeString.Equals(RDFVocabulary.XSD.SHORT.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_SHORT;
            else if (datatypeString.Equals(RDFVocabulary.XSD.BYTE.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_BYTE;
            else if (datatypeString.Equals(RDFVocabulary.XSD.UNSIGNED_LONG.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG;
            else if (datatypeString.Equals(RDFVocabulary.XSD.UNSIGNED_INT.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT;
            else if (datatypeString.Equals(RDFVocabulary.XSD.UNSIGNED_SHORT.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT;
            else if (datatypeString.Equals(RDFVocabulary.XSD.UNSIGNED_BYTE.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE;
            else if (datatypeString.Equals(RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER;
            else if (datatypeString.Equals(RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER;
            else if (datatypeString.Equals(RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER;
            else if (datatypeString.Equals(RDFVocabulary.XSD.POSITIVE_INTEGER.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER;
            else if (datatypeString.Equals(RDFVocabulary.XSD.DATE.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_DATE;
            else if (datatypeString.Equals(RDFVocabulary.XSD.DATETIME.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_DATETIME;
            else if (datatypeString.Equals(RDFVocabulary.XSD.DATETIMESTAMP.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP;
            else if (datatypeString.Equals(RDFVocabulary.XSD.TIME.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_TIME;
            else if (datatypeString.Equals(RDFVocabulary.XSD.G_DAY.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_GDAY;
            else if (datatypeString.Equals(RDFVocabulary.XSD.G_MONTH.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_GMONTH;
            else if (datatypeString.Equals(RDFVocabulary.XSD.G_MONTH_DAY.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY;
            else if (datatypeString.Equals(RDFVocabulary.XSD.G_YEAR.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_GYEAR;
            else if (datatypeString.Equals(RDFVocabulary.XSD.G_YEAR_MONTH.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH;
            else if (datatypeString.Equals(RDFVocabulary.XSD.DURATION.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_DURATION;

            //Less-likely to be used, so recognized with lower priority
            else if (datatypeString.Equals(RDFVocabulary.RDF.XML_LITERAL.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL;
            else if (datatypeString.Equals(RDFVocabulary.RDF.HTML.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.RDF_HTML;
            else if (datatypeString.Equals(RDFVocabulary.RDF.JSON.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.RDF_JSON;
            else if (datatypeString.Equals(RDFVocabulary.XSD.ANY_URI.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_ANYURI;
            else if (datatypeString.Equals(RDFVocabulary.XSD.BASE64_BINARY.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY;
            else if (datatypeString.Equals(RDFVocabulary.XSD.HEX_BINARY.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_HEXBINARY;
            else if (datatypeString.Equals(RDFVocabulary.XSD.ID.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_ID;
            else if (datatypeString.Equals(RDFVocabulary.XSD.LANGUAGE.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_LANGUAGE;
            else if (datatypeString.Equals(RDFVocabulary.XSD.NAME.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_NAME;
            else if (datatypeString.Equals(RDFVocabulary.XSD.NCNAME.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_NCNAME;
            else if (datatypeString.Equals(RDFVocabulary.XSD.QNAME.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_QNAME;
            else if (datatypeString.Equals(RDFVocabulary.XSD.TOKEN.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_TOKEN;
            else if (datatypeString.Equals(RDFVocabulary.XSD.NMTOKEN.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_NMTOKEN;
            else if (datatypeString.Equals(RDFVocabulary.XSD.NORMALIZED_STRING.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING;
            else if (datatypeString.Equals(RDFVocabulary.XSD.NOTATION.ToString(), StringComparison.Ordinal))
                return RDFModelEnums.RDFDatatypes.XSD_NOTATION;

            else
                //Unknown datatypes fallback to rdfs:Literal
                return RDFModelEnums.RDFDatatypes.RDFS_LITERAL;
        }

        /// <summary>
        /// Gives the string representation of the given RDF/RDFS/XSD datatype
        /// </summary>
        public static string GetDatatypeFromEnum(RDFModelEnums.RDFDatatypes datatype)
        {
            switch (datatype)
            {
                case RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL:
                    return RDFVocabulary.RDF.XML_LITERAL.ToString();
                case RDFModelEnums.RDFDatatypes.RDF_HTML:
                    return RDFVocabulary.RDF.HTML.ToString();
                case RDFModelEnums.RDFDatatypes.RDF_JSON:
                    return RDFVocabulary.RDF.JSON.ToString();
                case RDFModelEnums.RDFDatatypes.RDFS_LITERAL:
                    return RDFVocabulary.RDFS.LITERAL.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_STRING:
                    return RDFVocabulary.XSD.STRING.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_ANYURI:
                    return RDFVocabulary.XSD.ANY_URI.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY:
                    return RDFVocabulary.XSD.BASE64_BINARY.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_BOOLEAN:
                    return RDFVocabulary.XSD.BOOLEAN.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_BYTE:
                    return RDFVocabulary.XSD.BYTE.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_DATE:
                    return RDFVocabulary.XSD.DATE.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_DATETIME:
                    return RDFVocabulary.XSD.DATETIME.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP:
                    return RDFVocabulary.XSD.DATETIMESTAMP.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_DECIMAL:
                    return RDFVocabulary.XSD.DECIMAL.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_DOUBLE:
                    return RDFVocabulary.XSD.DOUBLE.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_DURATION:
                    return RDFVocabulary.XSD.DURATION.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_FLOAT:
                    return RDFVocabulary.XSD.FLOAT.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_GDAY:
                    return RDFVocabulary.XSD.G_DAY.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_GMONTH:
                    return RDFVocabulary.XSD.G_MONTH.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY:
                    return RDFVocabulary.XSD.G_MONTH_DAY.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_GYEAR:
                    return RDFVocabulary.XSD.G_YEAR.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH:
                    return RDFVocabulary.XSD.G_YEAR_MONTH.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_HEXBINARY:
                    return RDFVocabulary.XSD.HEX_BINARY.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_INT:
                    return RDFVocabulary.XSD.INT.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_INTEGER:
                    return RDFVocabulary.XSD.INTEGER.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_LANGUAGE:
                    return RDFVocabulary.XSD.LANGUAGE.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_LONG:
                    return RDFVocabulary.XSD.LONG.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_NAME:
                    return RDFVocabulary.XSD.NAME.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_NCNAME:
                    return RDFVocabulary.XSD.NCNAME.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_ID:
                    return RDFVocabulary.XSD.ID.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER:
                    return RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_NMTOKEN:
                    return RDFVocabulary.XSD.NMTOKEN.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER:
                    return RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER:
                    return RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING:
                    return RDFVocabulary.XSD.NORMALIZED_STRING.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_NOTATION:
                    return RDFVocabulary.XSD.NOTATION.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER:
                    return RDFVocabulary.XSD.POSITIVE_INTEGER.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_QNAME:
                    return RDFVocabulary.XSD.QNAME.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_SHORT:
                    return RDFVocabulary.XSD.SHORT.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_TIME:
                    return RDFVocabulary.XSD.TIME.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_TOKEN:
                    return RDFVocabulary.XSD.TOKEN.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG:
                    return RDFVocabulary.XSD.UNSIGNED_LONG.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT:
                    return RDFVocabulary.XSD.UNSIGNED_INT.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT:
                    return RDFVocabulary.XSD.UNSIGNED_SHORT.ToString();
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE:
                    return RDFVocabulary.XSD.UNSIGNED_BYTE.ToString();

                //Unrecognized datatypes are threated as rdfs:Literal
                default:
                    return RDFVocabulary.RDFS.LITERAL.ToString();
            }
        }

        /// <summary>
        /// Validates the value of the given typed literal against its datatype
        /// </summary>
        internal static bool ValidateTypedLiteral(RDFTypedLiteral typedLiteral)
        {
            //Tries to parse the given value into a DateTime having exactly the specified input/output formats.
            //RDFSharp datetime-based typed literals are automatically converted in UTC timezone (Z)
            bool TryParseDateTime(string value, string formatToParse, string formatToConvert)
            { 
                if (DateTime.TryParseExact(value, formatToParse, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime parsedDateTime))
                { 
                    typedLiteral.Value = parsedDateTime.ToString(formatToConvert, CultureInfo.InvariantCulture);
                    return true;
                }
                return false;
            }

            switch (typedLiteral.Datatype)
            {
                #region STRING CATEGORY
                case RDFModelEnums.RDFDatatypes.RDFS_LITERAL:
                case RDFModelEnums.RDFDatatypes.XSD_STRING:
                case RDFModelEnums.RDFDatatypes.RDF_HTML:
                default:
                    return true;

                case RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL:
                    try
                    {
                        XDocument.Parse(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }

                case RDFModelEnums.RDFDatatypes.RDF_JSON:
                    return (typedLiteral.Value.StartsWith("{") && typedLiteral.Value.EndsWith("}"))
                                || (typedLiteral.Value.StartsWith("[") && typedLiteral.Value.EndsWith("]"));

                case RDFModelEnums.RDFDatatypes.XSD_ANYURI:
                    if (Uri.TryCreate(typedLiteral.Value, UriKind.Absolute, out Uri outUri))
                    {
                        typedLiteral.Value = Convert.ToString(outUri);
                        return true;
                    }
                    return false;

                case RDFModelEnums.RDFDatatypes.XSD_NAME:
                    try
                    {
                        XmlConvert.VerifyName(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }

                case RDFModelEnums.RDFDatatypes.XSD_QNAME:
                    string[] prefixedQName = typedLiteral.Value.Split(':');
                    if (prefixedQName.Length == 1)
                    {
                        try
                        {
                            XmlConvert.VerifyNCName(prefixedQName[0]);
                            return true;
                        }
                        catch { return false; }
                    }
                    else if (prefixedQName.Length == 2)
                    {
                        try
                        {
                            XmlConvert.VerifyNCName(prefixedQName[0]);
                            XmlConvert.VerifyNCName(prefixedQName[1]);
                            return true;
                        }
                        catch { return false; }
                    }
                    else { return false; }

                case RDFModelEnums.RDFDatatypes.XSD_NCNAME:
                case RDFModelEnums.RDFDatatypes.XSD_ID:
                    try
                    {
                        XmlConvert.VerifyNCName(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }

                case RDFModelEnums.RDFDatatypes.XSD_TOKEN:
                    try
                    {
                        XmlConvert.VerifyTOKEN(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }

                case RDFModelEnums.RDFDatatypes.XSD_NMTOKEN:
                    try
                    {
                        XmlConvert.VerifyNMTOKEN(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }

                case RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING:
                    return typedLiteral.Value.IndexOfAny(new char[] { '\n', '\r', '\t' }) == -1;

                case RDFModelEnums.RDFDatatypes.XSD_LANGUAGE:
                    return RDFPlainLiteral.LangTag.Match(typedLiteral.Value).Success;

                case RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY:
                    try
                    {
                        Convert.FromBase64String(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }

                case RDFModelEnums.RDFDatatypes.XSD_HEXBINARY:
                    return hexBinary.Match(typedLiteral.Value).Success;
                #endregion

                #region BOOLEAN CATEGORY
                case RDFModelEnums.RDFDatatypes.XSD_BOOLEAN:
                    if (bool.TryParse(typedLiteral.Value, out bool outBool))
                        typedLiteral.Value = outBool ? "true" : "false";
                    else
                    {
                        //Support intelligent detection of alternative boolean representations
                        if (AlternativesBoolTrue.Any(tl => tl.Equals(typedLiteral.Value, StringComparison.OrdinalIgnoreCase)))
                            typedLiteral.Value = "true";
                        else if (AlternativesBoolFalse.Any(tl => tl.Equals(typedLiteral.Value, StringComparison.OrdinalIgnoreCase)))
                            typedLiteral.Value = "false";
                        else
                            return false;
                    }
                    return true;
                #endregion

                #region DATETIME CATEGORY
                case RDFModelEnums.RDFDatatypes.XSD_DATETIME:
                    return    TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ssZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:sszzz", "yyyy-MM-ddTHH:mm:ssZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.f", "yyyy-MM-ddTHH:mm:ss.fZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ss.fZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fzzz", "yyyy-MM-ddTHH:mm:ss.fZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ff", "yyyy-MM-ddTHH:mm:ss.ffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.ffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffzzz", "yyyy-MM-ddTHH:mm:ss.ffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fff", "yyyy-MM-ddTHH:mm:ss.fffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.fffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffzzz", "yyyy-MM-ddTHH:mm:ss.fffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffff", "yyyy-MM-ddTHH:mm:ss.ffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffff", "yyyy-MM-ddTHH:mm:ss.fffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffffzzz", "yyyy-MM-ddTHH:mm:ss.fffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffff", "yyyy-MM-ddTHH:mm:ss.ffffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffffZ");

                case RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP:
                    return    TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ssZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:sszzz", "yyyy-MM-ddTHH:mm:ssZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ss.fZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fzzz", "yyyy-MM-ddTHH:mm:ss.fZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.ffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffzzz", "yyyy-MM-ddTHH:mm:ss.ffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.fffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffzzz", "yyyy-MM-ddTHH:mm:ss.fffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.fffffzzz", "yyyy-MM-ddTHH:mm:ss.fffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddTHH:mm:ss.ffffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffffZ");

                case RDFModelEnums.RDFDatatypes.XSD_DATE:
                    return    TryParseDateTime(typedLiteral.Value, "yyyy-MM-dd", "yyyy-MM-ddZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddZ", "yyyy-MM-ddZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MM-ddzzz", "yyyy-MM-ddZ");

                case RDFModelEnums.RDFDatatypes.XSD_TIME:
                    return    TryParseDateTime(typedLiteral.Value, "HH:mm:ss", "HH:mm:ssZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ssZ", "HH:mm:ssZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:sszzz", "HH:mm:ssZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.f", "HH:mm:ss.fZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.fZ", "HH:mm:ss.fZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.fzzz", "HH:mm:ss.fZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ff", "HH:mm:ss.ffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ffZ", "HH:mm:ss.ffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ffzzz", "HH:mm:ss.ffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.fff", "HH:mm:ss.fffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.fffZ", "HH:mm:ss.fffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.fffzzz", "HH:mm:ss.fffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ffff", "HH:mm:ss.ffffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ffffZ", "HH:mm:ss.ffffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ffffzzz", "HH:mm:ss.ffffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.fffff", "HH:mm:ss.fffffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.fffffZ", "HH:mm:ss.fffffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.fffffzzz", "HH:mm:ss.fffffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ffffff", "HH:mm:ss.ffffffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ffffffZ", "HH:mm:ss.ffffffZ")
                           || TryParseDateTime(typedLiteral.Value, "HH:mm:ss.ffffffzzz", "HH:mm:ss.ffffffZ");

                case RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY:
                    return    TryParseDateTime(typedLiteral.Value, "--MM-dd", "--MM-ddZ")
                           || TryParseDateTime(typedLiteral.Value, "--MM-ddZ", "--MM-ddZ")
                           || TryParseDateTime(typedLiteral.Value, "--MM-ddzzz", "--MM-ddZ");

                case RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH:
                    return    TryParseDateTime(typedLiteral.Value, "yyyy-MM", "yyyy-MMZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MMZ", "yyyy-MMZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyy-MMzzz", "yyyy-MMZ");

                case RDFModelEnums.RDFDatatypes.XSD_GYEAR:
                    return    TryParseDateTime(typedLiteral.Value, "yyyy", "yyyyZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyyZ", "yyyyZ")
                           || TryParseDateTime(typedLiteral.Value, "yyyyzzz", "yyyyZ");

                case RDFModelEnums.RDFDatatypes.XSD_GMONTH:
                    return    TryParseDateTime(typedLiteral.Value, "MM", "MMZ")
                           || TryParseDateTime(typedLiteral.Value, "MMZ", "MMZ")
                           || TryParseDateTime(typedLiteral.Value, "MMzzz", "MMZ");

                case RDFModelEnums.RDFDatatypes.XSD_GDAY:
                    return    TryParseDateTime(typedLiteral.Value, "dd", "ddZ")
                           || TryParseDateTime(typedLiteral.Value, "ddZ", "ddZ")
                           || TryParseDateTime(typedLiteral.Value, "ddzzz", "ddZ");
                #endregion

                #region TIMESPAN CATEGORY
                case RDFModelEnums.RDFDatatypes.XSD_DURATION:
                    try
                    {
                        XmlConvert.ToTimeSpan(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }
                #endregion

                #region NUMERIC CATEGORY
                case RDFModelEnums.RDFDatatypes.XSD_DECIMAL:
                    if (decimal.TryParse(typedLiteral.Value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal outDecimal))
                    {
                        typedLiteral.Value = Convert.ToString(outDecimal, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_DOUBLE:
                    if (double.TryParse(typedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double outDouble))
                    {
                        typedLiteral.Value = Convert.ToString(outDouble, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_FLOAT:
                    if (float.TryParse(typedLiteral.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float outFloat))
                    {
                        typedLiteral.Value = Convert.ToString(outFloat, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_INTEGER:
                    if (decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outInteger))
                    {
                        typedLiteral.Value = Convert.ToString(outInteger, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_LONG:
                    if (long.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long outLong))
                    {
                        typedLiteral.Value = Convert.ToString(outLong, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_INT:
                    if (int.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int outInt))
                    {
                        typedLiteral.Value = Convert.ToString(outInt, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_SHORT:
                    if (short.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out short outShort))
                    {
                        typedLiteral.Value = Convert.ToString(outShort, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_BYTE:
                    if (sbyte.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out sbyte outSByte))
                    {
                        typedLiteral.Value = Convert.ToString(outSByte, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG:
                    if (ulong.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong outULong))
                    {
                        typedLiteral.Value = Convert.ToString(outULong, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT:
                    if (uint.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint outUInt))
                    {
                        typedLiteral.Value = Convert.ToString(outUInt, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT:
                    if (ushort.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort outUShort))
                    {
                        typedLiteral.Value = Convert.ToString(outUShort, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE:
                    if (byte.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte outByte))
                    {
                        typedLiteral.Value = Convert.ToString(outByte, CultureInfo.InvariantCulture);
                        return true;
                    }
                    else
                        return false;

                case RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER:
                    if (decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outNPInteger))
                    {
                        if (outNPInteger > 0)
                            return false;
                        else
                        {
                            typedLiteral.Value = Convert.ToString(outNPInteger, CultureInfo.InvariantCulture);
                            return true;
                        }
                    }
                    return false;

                case RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER:
                    if (decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outNInteger))
                    {
                        if (outNInteger > -1)
                            return false;
                        else
                        {
                            typedLiteral.Value = Convert.ToString(outNInteger, CultureInfo.InvariantCulture);
                            return true;
                        }
                    }
                    return false;

                case RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER:
                    if (decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outNNInteger))
                    {
                        if (outNNInteger < 0)
                            return false;
                        else
                        {
                            typedLiteral.Value = Convert.ToString(outNNInteger, CultureInfo.InvariantCulture);
                            return true;
                        }
                    }
                    return false;

                case RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER:
                    if (decimal.TryParse(typedLiteral.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outPInteger))
                    {
                        if (outPInteger < 1)
                            return false;
                        else
                        {
                            typedLiteral.Value = Convert.ToString(outPInteger, CultureInfo.InvariantCulture);
                            return true;
                        }
                    }
                    return false;
                #endregion
            }
        }
        #endregion
    }

}