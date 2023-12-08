/*
   Copyright 2012-2024 Marco De Salvo

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
using RDFSharp.Query;

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
            #region Guards
            if (input == null)
                throw new RDFModelException("Cannot create hash because given \"input\" string parameter is null.");
            #endregion

            using (MD5CryptoServiceProvider md5Encryptor = new MD5CryptoServiceProvider())
                return BitConverter.ToInt64(md5Encryptor.ComputeHash(UTF8_NoBOM.GetBytes(input)), 0);
        }
        #endregion

        #region Strings
        /// <summary>
        /// UTF8 encoding which does not emit BOM (for better OS interoperability)
        /// </summary>
        internal static readonly UTF8Encoding UTF8_NoBOM = new UTF8Encoding(false);

        /// <summary>
        /// Regex to catch 8-byte unicodes
        /// </summary>
        internal static readonly Lazy<Regex> regexU8 = new Lazy<Regex>(() => new Regex(@"\\U([0-9A-Fa-f]{8})", RegexOptions.Compiled));
        /// <summary>
        /// Regex to catch 4-byte unicodes
        /// </summary>
        internal static readonly Lazy<Regex> regexU4 = new Lazy<Regex>(() => new Regex(@"\\u([0-9A-Fa-f]{4})", RegexOptions.Compiled));
        /// <summary>
        /// Regex to catch xsd:hexBinary typed literals
        /// </summary>
        internal static readonly Lazy<Regex> hexBinary = new Lazy<Regex>(() => new Regex(@"^([0-9a-fA-F]{2})*$", RegexOptions.Compiled));

        /// <summary>
        /// Alternative representations of boolean True
        /// </summary>
        internal static readonly string[] AlternativesBoolTrue  = new string[] { "1", "one", "yes", "y", "t", "on", "ok", "up" };
        /// <summary>
        /// Alternative representations of boolean False
        /// </summary>
        internal static readonly string[] AlternativesBoolFalse = new string[] { "0", "zero", "no", "n", "f", "off", "ko", "down" };

        /// <summary>
        /// Dictionay of known datatypes for supporting direct lookup by string
        /// </summary>
        internal static Dictionary<string, RDFModelEnums.RDFDatatypes> KnownDatatypes = new Dictionary<string, RDFModelEnums.RDFDatatypes>()
        {
            { RDFVocabulary.RDF.HTML.ToString(), RDFModelEnums.RDFDatatypes.RDF_HTML },
            { RDFVocabulary.RDF.JSON.ToString(), RDFModelEnums.RDFDatatypes.RDF_JSON },
            { RDFVocabulary.RDF.XML_LITERAL.ToString(), RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL },
            { RDFVocabulary.RDFS.LITERAL.ToString(), RDFModelEnums.RDFDatatypes.RDFS_LITERAL },
            { RDFVocabulary.XSD.ANY_URI.ToString(), RDFModelEnums.RDFDatatypes.XSD_ANYURI },
            { RDFVocabulary.XSD.BASE64_BINARY.ToString(), RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY },
            { RDFVocabulary.XSD.BOOLEAN.ToString(), RDFModelEnums.RDFDatatypes.XSD_BOOLEAN },
            { RDFVocabulary.XSD.BYTE.ToString(), RDFModelEnums.RDFDatatypes.XSD_BYTE },
            { RDFVocabulary.XSD.DATE.ToString(), RDFModelEnums.RDFDatatypes.XSD_DATE },
            { RDFVocabulary.XSD.DATETIME.ToString(), RDFModelEnums.RDFDatatypes.XSD_DATETIME },
            { RDFVocabulary.XSD.DATETIMESTAMP.ToString(), RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP },
            { RDFVocabulary.XSD.DECIMAL.ToString(), RDFModelEnums.RDFDatatypes.XSD_DECIMAL },
            { RDFVocabulary.XSD.DOUBLE.ToString(), RDFModelEnums.RDFDatatypes.XSD_DOUBLE },
            { RDFVocabulary.XSD.DURATION.ToString(), RDFModelEnums.RDFDatatypes.XSD_DURATION },
            { RDFVocabulary.XSD.FLOAT.ToString(), RDFModelEnums.RDFDatatypes.XSD_FLOAT },
            { RDFVocabulary.XSD.G_DAY.ToString(), RDFModelEnums.RDFDatatypes.XSD_GDAY },
            { RDFVocabulary.XSD.G_MONTH.ToString(), RDFModelEnums.RDFDatatypes.XSD_GMONTH },
            { RDFVocabulary.XSD.G_MONTH_DAY.ToString(), RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY },
            { RDFVocabulary.XSD.G_YEAR.ToString(), RDFModelEnums.RDFDatatypes.XSD_GYEAR },
            { RDFVocabulary.XSD.G_YEAR_MONTH.ToString(), RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH },
            { RDFVocabulary.XSD.HEX_BINARY.ToString(), RDFModelEnums.RDFDatatypes.XSD_HEXBINARY },
            { RDFVocabulary.XSD.ID.ToString(), RDFModelEnums.RDFDatatypes.XSD_ID },
            { RDFVocabulary.XSD.INT.ToString(), RDFModelEnums.RDFDatatypes.XSD_INT },
            { RDFVocabulary.XSD.INTEGER.ToString(), RDFModelEnums.RDFDatatypes.XSD_INTEGER },
            { RDFVocabulary.XSD.LANGUAGE.ToString(), RDFModelEnums.RDFDatatypes.XSD_LANGUAGE },
            { RDFVocabulary.XSD.LONG.ToString(), RDFModelEnums.RDFDatatypes.XSD_LONG },
            { RDFVocabulary.XSD.NAME.ToString(), RDFModelEnums.RDFDatatypes.XSD_NAME },
            { RDFVocabulary.XSD.NCNAME.ToString(), RDFModelEnums.RDFDatatypes.XSD_NCNAME },
            { RDFVocabulary.XSD.NEGATIVE_INTEGER.ToString(), RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER },
            { RDFVocabulary.XSD.NMTOKEN.ToString(), RDFModelEnums.RDFDatatypes.XSD_NMTOKEN },
            { RDFVocabulary.XSD.NON_NEGATIVE_INTEGER.ToString(), RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER },
            { RDFVocabulary.XSD.NON_POSITIVE_INTEGER.ToString(), RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER },
            { RDFVocabulary.XSD.NORMALIZED_STRING.ToString(), RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING },
            { RDFVocabulary.XSD.NOTATION.ToString(), RDFModelEnums.RDFDatatypes.XSD_NOTATION },
            { RDFVocabulary.XSD.POSITIVE_INTEGER.ToString(), RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER },
            { RDFVocabulary.XSD.QNAME.ToString(), RDFModelEnums.RDFDatatypes.XSD_QNAME },
            { RDFVocabulary.XSD.SHORT.ToString(), RDFModelEnums.RDFDatatypes.XSD_SHORT },
            { RDFVocabulary.XSD.STRING.ToString(), RDFModelEnums.RDFDatatypes.XSD_STRING },
            { RDFVocabulary.XSD.TIME.ToString(), RDFModelEnums.RDFDatatypes.XSD_TIME },
            { RDFVocabulary.XSD.TOKEN.ToString(), RDFModelEnums.RDFDatatypes.XSD_TOKEN },
            { RDFVocabulary.XSD.UNSIGNED_BYTE.ToString(), RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE },
            { RDFVocabulary.XSD.UNSIGNED_INT.ToString(), RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT },
            { RDFVocabulary.XSD.UNSIGNED_LONG.ToString(), RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG },
            { RDFVocabulary.XSD.UNSIGNED_SHORT.ToString(), RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT },
            { RDFVocabulary.GEOSPARQL.WKT_LITERAL.ToString(), RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT },
            { RDFVocabulary.GEOSPARQL.GML_LITERAL.ToString(), RDFModelEnums.RDFDatatypes.GEOSPARQL_GML },
            { RDFVocabulary.TIME.GENERAL_DAY.ToString(), RDFModelEnums.RDFDatatypes.TIME_GENERALDAY },
            { RDFVocabulary.TIME.GENERAL_MONTH.ToString(), RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH },
            { RDFVocabulary.TIME.GENERAL_YEAR.ToString(), RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR },
            { RDFVocabulary.OWL.REAL.ToString(), RDFModelEnums.RDFDatatypes.OWL_REAL }
        };

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
            sbRegexU8.Append(regexU8.Value.Replace(asciiString, match => char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, NumberStyles.HexNumber))));

            //UNICODE (UTF-8)
            StringBuilder sbRegexU4 = new StringBuilder();
            sbRegexU4.Append(regexU4.Value.Replace(sbRegexU8.ToString(), match => char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, NumberStyles.HexNumber))));

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
                List<RDFIndexedTriple> S = new List<RDFIndexedTriple>();
                List<RDFIndexedTriple> P = new List<RDFIndexedTriple>();
                List<RDFIndexedTriple> O = new List<RDFIndexedTriple>();
                List<RDFIndexedTriple> L = new List<RDFIndexedTriple>();
                List<RDFIndexedTriple> matchResultIndexedTriples = new List<RDFIndexedTriple>();
                StringBuilder queryFilters = new StringBuilder();

                //Filter by Subject
                if (subj != null)
                {
                    queryFilters.Append('S');
                    foreach (long t in graph.GraphIndex.SelectIndexBySubject(subj))
                        S.Add(graph.IndexedTriples[t]);
                }

                //Filter by Predicate
                if (pred != null)
                {
                    queryFilters.Append('P');
                    foreach (long t in graph.GraphIndex.SelectIndexByPredicate(pred))
                        P.Add(graph.IndexedTriples[t]);
                }

                //Filter by Object
                if (obj != null)
                {
                    queryFilters.Append('O');
                    foreach (long t in graph.GraphIndex.SelectIndexByObject(obj))
                        O.Add(graph.IndexedTriples[t]);
                }

                //Filter by Literal
                if (lit != null)
                {
                    queryFilters.Append('L');
                    foreach (long t in graph.GraphIndex.SelectIndexByLiteral(lit))
                        L.Add(graph.IndexedTriples[t]);
                }

                //Intersect the filters
                switch (queryFilters.ToString())
                {
                    case "S":
                        matchResultIndexedTriples = S;
                        break;
                    case "P":
                        matchResultIndexedTriples = P;
                        break;
                    case "O":
                        matchResultIndexedTriples = O;
                        break;
                    case "L":
                        matchResultIndexedTriples = L;
                        break;
                    case "SP":
                        matchResultIndexedTriples = S.Intersect(P).ToList();
                        break;
                    case "SO":
                        matchResultIndexedTriples = S.Intersect(O).ToList();
                        break;
                    case "SL":
                        matchResultIndexedTriples = S.Intersect(L).ToList();
                        break;
                    case "PO":
                        matchResultIndexedTriples = P.Intersect(O).ToList();
                        break;
                    case "PL":
                        matchResultIndexedTriples = P.Intersect(L).ToList();
                        break;
                    case "SPO":
                        matchResultIndexedTriples = S.Intersect(P).Intersect(O).ToList();
                        break;
                    case "SPL":
                        matchResultIndexedTriples = S.Intersect(P).Intersect(L).ToList();
                        break;
                    default:
                        matchResultIndexedTriples = graph.IndexedTriples.Values.ToList();
                        break;
                }

                //Decompress indexed triples
                matchResultIndexedTriples.ForEach(indexedTriple => matchResult.Add(new RDFTriple(indexedTriple, graph.GraphIndex)));
            }
            return matchResult;
        }
        #endregion

        #region Collections
        /// <summary>
        /// Rebuilds the collection represented by the given resource within the given graph
        /// </summary>
        internal static RDFCollection DeserializeCollectionFromGraph(RDFGraph graph, RDFResource collRepresentative, RDFModelEnums.RDFTripleFlavors expectedFlavor)
        {
            RDFCollection collection = new RDFCollection(expectedFlavor == RDFModelEnums.RDFTripleFlavors.SPO ? RDFModelEnums.RDFItemTypes.Resource : RDFModelEnums.RDFItemTypes.Literal);
            RDFGraph rdfFirst = graph[null, RDFVocabulary.RDF.FIRST, null, null];
            RDFGraph rdfRest = graph[null, RDFVocabulary.RDF.REST, null, null];

            #region Deserialization
            bool nilFound = false;
            RDFResource itemRest = collRepresentative;
            HashSet<long> itemRestVisitCache = new HashSet<long>() { itemRest.PatternMemberID };
            while (!nilFound)
            {
                #region rdf:first
                RDFTriple first = rdfFirst[itemRest, null, null, null].FirstOrDefault();
                if (first != null && first.TripleFlavor == expectedFlavor)
                {
                    if (expectedFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        collection.AddItem((RDFResource)first.Object);
                    else
                        collection.AddItem((RDFLiteral)first.Object);
                }
                else
                    nilFound = true;
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
                        nilFound = true;
                }
                #endregion
            }
            #endregion

            return collection;
        }

        /// <summary>
        /// Detects the flavor (SPO/SPL) of the collection represented by the given resource within the given graph
        /// </summary>
        internal static RDFModelEnums.RDFTripleFlavors DetectCollectionFlavorFromGraph(RDFGraph graph, RDFResource collRepresentative)
            => graph[collRepresentative, RDFVocabulary.RDF.FIRST, null, null].FirstOrDefault()?.TripleFlavor ?? RDFModelEnums.RDFTripleFlavors.SPO;
        #endregion

        #region Namespaces
        /// <summary>
        /// Gets the list of namespaces used within the triples of the given graph
        /// </summary>
        internal static List<RDFNamespace> GetGraphNamespaces(RDFGraph graph)
        {
            List<RDFNamespace> result = new List<RDFNamespace>();
            foreach (RDFTriple triple in graph)
            {
                string subj = triple.Subject.ToString();
                string pred = triple.Predicate.ToString();
                string obj = triple.Object is RDFResource ? triple.Object.ToString() :
                                (triple.Object is RDFTypedLiteral ? GetDatatypeFromEnum(((RDFTypedLiteral)triple.Object).Datatype) : string.Empty);

                //Resolve subject Uri
                result.AddRange(RDFNamespaceRegister.Instance.Register.Where(ns => subj.StartsWith(ns.ToString())));

                //Resolve predicate Uri
                result.AddRange(RDFNamespaceRegister.Instance.Register.Where(ns => pred.StartsWith(ns.ToString())));

                //Resolve object Uri
                result.AddRange(RDFNamespaceRegister.Instance.Register.Where(ns => obj.StartsWith(ns.ToString())));
            }
            return result.Distinct().ToList();
        }
        #endregion

        #region Datatypes
        /// <summary>
        /// Parses the given string in order to give the corresponding datatype
        /// </summary>
        public static RDFModelEnums.RDFDatatypes GetDatatypeFromString(string datatypeString)
        {
            #region Guards
            if (string.IsNullOrEmpty(datatypeString))
                throw new RDFModelException("Cannot recognize datatype representation of given \"datatypeString\" parameter because it is null or empty");
            if (!Uri.TryCreate(datatypeString, UriKind.Absolute, out Uri datatypeUri))
                throw new RDFModelException("Cannot recognize datatype representation of given \"datatypeString\" parameter because it is not a valid absolute Uri");
            #endregion

            //Lookup known datatypes (fallback to rdfs:Literal if unsupported)
            return KnownDatatypes.TryGetValue(datatypeUri.ToString(), out RDFModelEnums.RDFDatatypes datatype) 
                ? datatype : RDFModelEnums.RDFDatatypes.RDFS_LITERAL;
        }

        /// <summary>
        /// Gives the string representation of the given datatype
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
                case RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT:
                    return RDFVocabulary.GEOSPARQL.WKT_LITERAL.ToString();
                case RDFModelEnums.RDFDatatypes.GEOSPARQL_GML:
                    return RDFVocabulary.GEOSPARQL.GML_LITERAL.ToString();
                case RDFModelEnums.RDFDatatypes.TIME_GENERALDAY:
                    return RDFVocabulary.TIME.GENERAL_DAY.ToString();
                case RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH:
                    return RDFVocabulary.TIME.GENERAL_MONTH.ToString();
                case RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR:
                    return RDFVocabulary.TIME.GENERAL_YEAR.ToString();
                case RDFModelEnums.RDFDatatypes.OWL_REAL:
                    return RDFVocabulary.OWL.REAL.ToString();

                //Fallback to rdfs:Literal
                case RDFModelEnums.RDFDatatypes.RDFS_LITERAL:
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

                case RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT:
                case RDFModelEnums.RDFDatatypes.GEOSPARQL_GML:
                    try
                    {
                        _ = typedLiteral.Datatype.Equals(RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT) ?
                                RDFGeoExpression.WKTReader.Read(typedLiteral.Value) : RDFGeoExpression.GMLReader.Read(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }

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
                    return RDFPlainLiteral.LangTag.Value.Match(typedLiteral.Value).Success;

                case RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY:
                    try
                    {
                        Convert.FromBase64String(typedLiteral.Value);
                        return true;
                    }
                    catch { return false; }

                case RDFModelEnums.RDFDatatypes.XSD_HEXBINARY:
                    return hexBinary.Value.Match(typedLiteral.Value).Success;
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
                    return    TryParseDateTime(typedLiteral.Value, "--MM", "--MMZ")
                           || TryParseDateTime(typedLiteral.Value, "--MMZ", "--MMZ")
                           || TryParseDateTime(typedLiteral.Value, "--MMzzz", "--MMZ");

                case RDFModelEnums.RDFDatatypes.XSD_GDAY:
                    return    TryParseDateTime(typedLiteral.Value, "---dd", "---ddZ")
                           || TryParseDateTime(typedLiteral.Value, "---ddZ", "---ddZ")
                           || TryParseDateTime(typedLiteral.Value, "---ddzzz", "---ddZ");

                case RDFModelEnums.RDFDatatypes.TIME_GENERALDAY:
                    return Regex.IsMatch(typedLiteral.Value, "---(0[1-9]|[1-9][0-9])(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?");

                case RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH:
                    return Regex.IsMatch(typedLiteral.Value, "--(0[1-9]|1[0-9]|20)(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?");

                case RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR:
                    return Regex.IsMatch(typedLiteral.Value, "-?([1-9][0-9]{3,}|0[0-9]{3})(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?");
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
                case RDFModelEnums.RDFDatatypes.OWL_REAL:
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