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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
        /// <exception cref="RDFModelException"></exception>
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
        internal static readonly Lazy<Regex> hexBinary = new Lazy<Regex>(() => new Regex("^([0-9a-fA-F]{2})*$", RegexOptions.Compiled));
        /// <summary>
        /// Regex to catch owl:rational typed literals
        /// </summary>
        internal static readonly Lazy<Regex> owlRational = new Lazy<Regex>(() => new Regex("^(0|(-)?([1-9])+([0-9])*)(/([1-9])+([0-9])*)?$", RegexOptions.Compiled));

        /// <summary>
        /// Alternative representations of boolean True
        /// </summary>
        internal static readonly string[] AlternativesBoolTrue  = { "1", "one", "yes", "y", "t", "on", "ok", "up" };
        /// <summary>
        /// Alternative representations of boolean False
        /// </summary>
        internal static readonly string[] AlternativesBoolFalse = { "0", "zero", "no", "n", "f", "off", "ko", "down" };

        /// <summary>
        /// Characters whose presence is forbidden inside xsd:normalizedString literals
        /// </summary>
        internal static readonly char[] NormalizedStringForbiddenChars = { '\n', '\r', '\t' };

        /// <summary>
        /// Empty hashset to be returned in case of index miss
        /// </summary>
        internal static readonly HashSet<long> EmptyHashSet = new HashSet<long>();

        /// <summary>
        /// Gets the Uri corresponding to the given string
        /// </summary>
        internal static Uri GetUriFromString(string uriString)
        {
            // blank node detection and normalization
            if (uriString?.StartsWith("bnode:", StringComparison.OrdinalIgnoreCase) ?? false)
                uriString = $"bnode:{uriString.Substring(6)}";
            else if (uriString?.StartsWith("_:", StringComparison.Ordinal) ?? false)
                uriString = $"bnode:{uriString.Substring(2)}";

            _ = Uri.TryCreate(uriString, UriKind.Absolute, out Uri tempUri);
            return tempUri;
        }

        /// <summary>
        /// Searches the given Uri in the namespace register for getting its dereferenceable representation;<br/>
        /// if not found, just returns the given Uri
        /// </summary>
        internal static Uri RemapUriForDereference(Uri uri)
        {
            string uriString = uri?.ToString() ?? string.Empty;

            return RDFNamespaceRegister.GetByUri(uriString)?.DereferenceUri ?? uri;
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
        /// <exception cref="RDFModelException"></exception>
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
                {
                    b.Append(unicodeString[i]);
                }

                //UNICODE (UTF-8)
                else if (!char.IsSurrogate(unicodeString[i]))
                {
                    b.Append($"\\u{(int)unicodeString[i]:X4}");
                }

                //UNICODE (UTF-16)
                else if (i + 1 < unicodeString.Length && char.IsSurrogatePair(unicodeString[i], unicodeString[i + 1]))
                {
                    int codePoint = char.ConvertToUtf32(unicodeString[i], unicodeString[i + 1]);
                    b.Append($"\\U{codePoint:X8}");
                    i++;
                }

                //ERROR
                else
                {
                    throw new RDFModelException("Cannot convert string '" + unicodeString + "' to ASCII because it is not well-formed UTF-16");
                }
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
            foreach (char c in data)
            {
                if (char.IsControl(c) && c != '\u0009' && c != '\u000A' && c != '\u000D')
                    b.Append($"\\u{(int)c:X4}");
                else
                    b.Append(c);
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
                 || !source.EndsWith(value, StringComparison.Ordinal))
            {
                return source;
            }

            return source.Remove(source.LastIndexOf(value, StringComparison.Ordinal));
        }

        /// <summary>
        /// Gets the short representation of the given Uri
        /// </summary>
        internal static string GetShortUri(this Uri uri)
        {
            if (uri == null)
                return null;
            if (!string.IsNullOrEmpty(uri.Fragment))
                return uri.Fragment.TrimStart('#');

            return uri.Segments.Length > 1 ? uri.Segments.Last() : uri.ToString();
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
                StringBuilder queryFilters = new StringBuilder();
                List<RDFHashedTriple> S = new List<RDFHashedTriple>();
                List<RDFHashedTriple> P = new List<RDFHashedTriple>();
                List<RDFHashedTriple> O = new List<RDFHashedTriple>();
                List<RDFHashedTriple> L = new List<RDFHashedTriple>();
                List<RDFHashedTriple> matchResultHashedTriples;

                //Filter by Subject
                if (subj != null)
                {
                    queryFilters.Append('S');
                    S.AddRange(graph.Index.LookupIndexBySubject(subj).Select(t => graph.Index.Hashes[t]));
                }

                //Filter by Predicate
                if (pred != null)
                {
                    queryFilters.Append('P');
                    P.AddRange(graph.Index.LookupIndexByPredicate(pred).Select(t => graph.Index.Hashes[t]));
                }

                //Filter by Object
                if (obj != null)
                {
                    queryFilters.Append('O');
                    O.AddRange(graph.Index.LookupIndexByObject(obj).Select(t => graph.Index.Hashes[t]));
                }

                //Filter by Literal
                if (lit != null)
                {
                    queryFilters.Append('L');
                    L.AddRange(graph.Index.LookupIndexByLiteral(lit).Select(t => graph.Index.Hashes[t]));
                }

                //Intersect the filters
                switch (queryFilters.ToString())
                {
                    case "S":
                        matchResultHashedTriples = S;
                        break;
                    case "P":
                        matchResultHashedTriples = P;
                        break;
                    case "O":
                        matchResultHashedTriples = O;
                        break;
                    case "L":
                        matchResultHashedTriples = L;
                        break;
                    case "SP":
                        matchResultHashedTriples = S.Intersect(P).ToList();
                        break;
                    case "SO":
                        matchResultHashedTriples = S.Intersect(O).ToList();
                        break;
                    case "SL":
                        matchResultHashedTriples = S.Intersect(L).ToList();
                        break;
                    case "PO":
                        matchResultHashedTriples = P.Intersect(O).ToList();
                        break;
                    case "PL":
                        matchResultHashedTriples = P.Intersect(L).ToList();
                        break;
                    case "SPO":
                        matchResultHashedTriples = S.Intersect(P).Intersect(O).ToList();
                        break;
                    case "SPL":
                        matchResultHashedTriples = S.Intersect(P).Intersect(L).ToList();
                        break;
                    default:
                        matchResultHashedTriples = graph.Index.Hashes.Values.ToList();
                        break;
                }

                //Decompress hashed triples
                matchResultHashedTriples.ForEach(hashedTriple => matchResult.Add(new RDFTriple(hashedTriple, graph.Index)));
            }
            return matchResult;
        }

        /// <summary>
        /// Extracts the datatype definitions contained in the given graphs (both faceted and aliases)
        /// </summary>
        public static List<RDFDatatype> ExtractDatatypeDefinitions(this RDFGraph graph)
        {
            #region Guards
            if (graph == null)
                return new List<RDFDatatype>();
            #endregion

            List<RDFDatatype> datatypes = new List<RDFDatatype>();
            foreach (RDFTriple datatypeTriple in graph[null, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDFS.DATATYPE, null])
            {
                RDFResource datatypeIRI = (RDFResource)datatypeTriple.Subject;

                //Try detect a faceted datatype
                if (graph[datatypeIRI, RDFVocabulary.OWL.WITH_RESTRICTIONS, null, null].FirstOrDefault()?.Object is RDFResource facetsRepresentative
                     && graph[datatypeIRI, RDFVocabulary.OWL.ON_DATATYPE, null, null].FirstOrDefault()?.Object is RDFResource onDatatype)
                {
                    //Detect the target datatype (fallback to rdfs:Literal in case not found)
                    RDFDatatype targetDatatype = RDFDatatypeRegister.GetDatatype(onDatatype.ToString())
                                                  ?? RDFDatatypeRegister.RDFSLiteral;
                    RDFModelEnums.RDFDatatypes targetDatatypeEnum = targetDatatype.ToString().GetEnumFromDatatype();

                    //Detect the constraining facets
                    List<RDFFacet> targetFacets = new List<RDFFacet>();
                    RDFCollection facetsCollection = DeserializeCollectionFromGraph(graph, facetsRepresentative, RDFModelEnums.RDFTripleFlavors.SPO);
                    foreach (RDFResource facet in facetsCollection.Items.Cast<RDFResource>())
                    {
                        //xsd:length
                        if (graph[facet, RDFVocabulary.XSD.LENGTH, null, null].FirstOrDefault()?.Object is RDFTypedLiteral facetLength
                             && facetLength.HasDecimalDatatype() && uint.TryParse(facetLength.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint facetLengthValue))
                        {
                            targetFacets.Add(new RDFLengthFacet(facetLengthValue));
                            continue;
                        }
                        //xsd:maxExclusive
                        if (graph[facet, RDFVocabulary.XSD.MAX_EXCLUSIVE, null, null].FirstOrDefault()?.Object is RDFTypedLiteral facetMaxExclusive
                             && facetMaxExclusive.HasDecimalDatatype())
                        {
                            //owl:rational needs parsing and evaluation before being compared
                            if (facetMaxExclusive.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                            {
                                targetFacets.Add(new RDFMaxExclusiveFacet(Convert.ToDouble(ComputeOWLRationalValue(facetMaxExclusive), CultureInfo.InvariantCulture)));
                                continue;
                            }
                            if (double.TryParse(facetMaxExclusive.Value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double facetMaxExclusiveValue))
                            {
                                targetFacets.Add(new RDFMaxExclusiveFacet(facetMaxExclusiveValue));
                                continue;
                            }
                        }
                        //xsd:maxInclusive
                        if (graph[facet, RDFVocabulary.XSD.MAX_INCLUSIVE, null, null].FirstOrDefault()?.Object is RDFTypedLiteral facetMaxInclusive
                             && facetMaxInclusive.HasDecimalDatatype())
                        {
                            //owl:rational needs parsing and evaluation before being compared
                            if (facetMaxInclusive.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                            {
                                targetFacets.Add(new RDFMaxInclusiveFacet(Convert.ToDouble(ComputeOWLRationalValue(facetMaxInclusive), CultureInfo.InvariantCulture)));
                                continue;
                            }
                            if (double.TryParse(facetMaxInclusive.Value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double facetMaxInclusiveValue))
                            {
                                targetFacets.Add(new RDFMaxInclusiveFacet(facetMaxInclusiveValue));
                                continue;
                            }
                        }
                        //xsd:maxLength
                        if (graph[facet, RDFVocabulary.XSD.MAX_LENGTH, null, null].FirstOrDefault()?.Object is RDFTypedLiteral facetMaxLength
                             && facetMaxLength.HasDecimalDatatype() && uint.TryParse(facetMaxLength.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint facetMaxLengthValue))
                        {
                            targetFacets.Add(new RDFMaxLengthFacet(facetMaxLengthValue));
                            continue;
                        }
                        //xsd:minExclusive
                        if (graph[facet, RDFVocabulary.XSD.MIN_EXCLUSIVE, null, null].FirstOrDefault()?.Object is RDFTypedLiteral facetMinExclusive
                             && facetMinExclusive.HasDecimalDatatype())
                        {
                            //owl:rational needs parsing and evaluation before being compared
                            if (facetMinExclusive.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                            {
                                targetFacets.Add(new RDFMinExclusiveFacet(Convert.ToDouble(ComputeOWLRationalValue(facetMinExclusive), CultureInfo.InvariantCulture)));
                                continue;
                            }
                            if (double.TryParse(facetMinExclusive.Value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double facetMinExclusiveValue))
                            {
                                targetFacets.Add(new RDFMinExclusiveFacet(facetMinExclusiveValue));
                                continue;
                            }
                        }
                        //xsd:minInclusive
                        if (graph[facet, RDFVocabulary.XSD.MIN_INCLUSIVE, null, null].FirstOrDefault()?.Object is RDFTypedLiteral facetMinInclusive
                             && facetMinInclusive.HasDecimalDatatype())
                        {
                            //owl:rational needs parsing and evaluation before being compared
                            if (facetMinInclusive.Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.OWL_RATIONAL)
                            {
                                targetFacets.Add(new RDFMinInclusiveFacet(Convert.ToDouble(ComputeOWLRationalValue(facetMinInclusive), CultureInfo.InvariantCulture)));
                                continue;
                            }
                            if (double.TryParse(facetMinInclusive.Value, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double facetMinInclusiveValue))
                            {
                                targetFacets.Add(new RDFMinInclusiveFacet(facetMinInclusiveValue));
                                continue;
                            }
                        }
                        //xsd:minLength
                        if (graph[facet, RDFVocabulary.XSD.MIN_LENGTH, null, null].FirstOrDefault()?.Object is RDFTypedLiteral facetMinLength
                             && facetMinLength.HasDecimalDatatype() && uint.TryParse(facetMinLength.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint facetMinLengthValue))
                        {
                            targetFacets.Add(new RDFMinLengthFacet(facetMinLengthValue));
                            continue;
                        }
                        //xsd:pattern
                        if (graph[facet, RDFVocabulary.XSD.PATTERN, null, null].FirstOrDefault()?.Object is RDFTypedLiteral facetPattern
                             && facetPattern.HasStringDatatype())
                            targetFacets.Add(new RDFPatternFacet(facetPattern.Value));
                    }

                    //Finally collect the datatype
                    datatypes.Add(new RDFDatatype(datatypeIRI.URI, targetDatatypeEnum, targetFacets));
                }

                //Try detect an alias datatype
                else if (graph[datatypeIRI, RDFVocabulary.OWL.EQUIVALENT_CLASS, null, null].FirstOrDefault()?.Object is RDFResource equivalentDatatype)
                {
                    //Detect the target datatype (fallback to rdfs:Literal in case not found)
                    RDFDatatype targetDatatype = RDFDatatypeRegister.GetDatatype(equivalentDatatype.ToString())
                                                  ?? RDFDatatypeRegister.RDFSLiteral;
                    RDFModelEnums.RDFDatatypes targetDatatypeEnum = targetDatatype.ToString().GetEnumFromDatatype();

                    //Finally collect the datatype
                    datatypes.Add(new RDFDatatype(datatypeIRI.URI, targetDatatypeEnum, null));
                }
            }
            return datatypes;
        }
        #endregion

        #region Collections
        /// <summary>
        /// Rebuilds the collection represented by the given resource within the given graph
        /// </summary>
        internal static RDFCollection DeserializeCollectionFromGraph(RDFGraph graph, RDFResource collRepresentative, RDFModelEnums.RDFTripleFlavors expectedFlavor, bool acceptDuplicates=false)
        {
            RDFCollection collection = new RDFCollection(expectedFlavor == RDFModelEnums.RDFTripleFlavors.SPO ? RDFModelEnums.RDFItemTypes.Resource : RDFModelEnums.RDFItemTypes.Literal, acceptDuplicates);
            RDFGraph rdfFirst = graph[null, RDFVocabulary.RDF.FIRST, null, null];
            RDFGraph rdfRest = graph[null, RDFVocabulary.RDF.REST, null, null];

            #region Deserialization
            bool nilFound = false;
            RDFResource itemRest = collRepresentative;
            HashSet<long> itemRestVisitCache = new HashSet<long> { itemRest.PatternMemberID };
            while (!nilFound)
            {
                #region rdf:first
                RDFTriple first = rdfFirst[itemRest, null, null, null].FirstOrDefault();
                if (first != null)
                {
                    if (first.Object is RDFResource firstObjRes)
                    {
                        //Avoid rdf:nil to be collected...
                        if (!firstObjRes.Equals(RDFVocabulary.RDF.NIL))
                            collection.AddItemInternal(firstObjRes);
                    }
                    else
                    {
                        collection.AddItemInternal((RDFLiteral)first.Object);
                    }
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
                    RDFTriple rest = rdfRest[itemRest, null, null, null].FirstOrDefault();
                    if (rest != null)
                    {
                        if (rest.Object.Equals(RDFVocabulary.RDF.NIL))
                        {
                            nilFound = true;
                        }
                        else
                        {
                            itemRest = (RDFResource)rest.Object;
                            //Avoid bad-formed cyclic lists to generate infinite loops
                            if (!itemRestVisitCache.Add(itemRest.PatternMemberID))
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
                string obj = triple.Object is RDFResource
                    ? triple.Object.ToString()
                    : triple.Object is RDFTypedLiteral tlitObj ? tlitObj.Datatype.URI.ToString()
                                                               : string.Empty;

                //Resolve subject Uri
                result.AddRange(RDFNamespaceRegister.Instance.Register.Where(ns => subj.StartsWith(ns.ToString(), StringComparison.Ordinal)));

                //Resolve predicate Uri
                result.AddRange(RDFNamespaceRegister.Instance.Register.Where(ns => pred.StartsWith(ns.ToString(), StringComparison.Ordinal)));

                //Resolve object Uri
                result.AddRange(RDFNamespaceRegister.Instance.Register.Where(ns => obj.StartsWith(ns.ToString(), StringComparison.Ordinal)));
            }
            return result.Distinct().ToList();
        }
        #endregion

        #region Datatypes
        /// <summary>
        /// Gives the string representation of the given datatype
        /// </summary>
        public static string GetDatatypeFromEnum(this RDFModelEnums.RDFDatatypes datatype)
            => ((DescriptionAttribute)RDFModelEnums_RDFDatatypes_EnumType
                .GetField(datatype.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false)[0]).Description;
        private static readonly Type RDFModelEnums_RDFDatatypes_EnumType = typeof(RDFModelEnums.RDFDatatypes);
        private static readonly Array RDFModelEnums_RDFDatatypes_EnumValues = RDFModelEnums_RDFDatatypes_EnumType.GetEnumValues();

        /// <summary>
        /// Gives the Enum representation of the given datatype
        /// </summary>
        public static RDFModelEnums.RDFDatatypes GetEnumFromDatatype(this string datatype)
            => (from RDFModelEnums.RDFDatatypes enumValue
                in RDFModelEnums_RDFDatatypes_EnumValues
                let enumValueInfo = RDFModelEnums_RDFDatatypes_EnumType.GetMember(enumValue.ToString())[0]
                let enumValueDescriptionAttribute = enumValueInfo.GetCustomAttribute<DescriptionAttribute>()
                where string.Equals(datatype, enumValueDescriptionAttribute.Description)
                select enumValue).FirstOrDefault();

        /// <summary>
        /// Validates the value of the given typed literal against its datatype
        /// </summary>
        internal static (bool,string) ValidateTypedLiteral(string literalValue, RDFModelEnums.RDFDatatypes datatype)
        {
            //Tries to parse the given value into a DateTime having exactly the specified input/output formats.
            //RDFSharp datetime-based typed literals are automatically converted in UTC timezone (Z)
            (bool,string) TryParseDateTime(string value, string formatToParse, string formatToConvert)
            {
                return DateTime.TryParseExact(value, formatToParse, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime parsedDateTime)
                    ? (true, parsedDateTime.ToString(formatToConvert, CultureInfo.InvariantCulture))
                    : (false, literalValue);
            }

            switch (datatype)
            {
                #region STRING CATEGORY
                case RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL:
                    try
                    {
                        _ = XDocument.Parse(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }

                case RDFModelEnums.RDFDatatypes.RDF_JSON:
                    bool isValidJson = (literalValue.StartsWith("{", StringComparison.Ordinal) && literalValue.EndsWith("}", StringComparison.Ordinal))
                                        || (literalValue.StartsWith("[", StringComparison.Ordinal) && literalValue.EndsWith("]", StringComparison.Ordinal));
                    return (isValidJson, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_ANYURI:
                    return Uri.TryCreate(literalValue, UriKind.Absolute, out Uri outUri) ? (true, Convert.ToString(outUri))
                                                                                         : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_NAME:
                    try
                    {
                        _ = XmlConvert.VerifyName(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }

                case RDFModelEnums.RDFDatatypes.XSD_QNAME:
                    string[] prefixedQName = literalValue.Split(':');
                    switch (prefixedQName.Length)
                    {
                        case 1:
                            try
                            {
                                _ = XmlConvert.VerifyNCName(prefixedQName[0]);
                                return (true, literalValue);
                            }
                            catch { return (false, literalValue); }

                        case 2:
                            try
                            {
                                _ = XmlConvert.VerifyNCName(prefixedQName[0]);
                                _ = XmlConvert.VerifyNCName(prefixedQName[1]);
                                return (true, literalValue);
                            }
                            catch { return (false, literalValue); }

                        default:
                            return (false, literalValue);
                    }

                case RDFModelEnums.RDFDatatypes.XSD_NCNAME:
                case RDFModelEnums.RDFDatatypes.XSD_ID:
                    try
                    {
                        _ = XmlConvert.VerifyNCName(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }

                case RDFModelEnums.RDFDatatypes.XSD_TOKEN:
                    try
                    {
                        _ = XmlConvert.VerifyTOKEN(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }

                case RDFModelEnums.RDFDatatypes.XSD_NMTOKEN:
                    try
                    {
                        _ = XmlConvert.VerifyNMTOKEN(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }

                case RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING:
                    return (literalValue.IndexOfAny(NormalizedStringForbiddenChars) == -1, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_LANGUAGE:
                    return (RDFPlainLiteral.LangTagRegex.Value.Match(literalValue).Success, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY:
                    try
                    {
                        _ = Convert.FromBase64String(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }

                case RDFModelEnums.RDFDatatypes.XSD_HEXBINARY:
                    return (hexBinary.Value.Match(literalValue).Success, literalValue);
                #endregion

                #region GEOGRAPHIC CATEGORY
                case RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT:
                    try
                    {
                        _ = RDFGeoExpression.WKTReader.Read(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }

                case RDFModelEnums.RDFDatatypes.GEOSPARQL_GML:
                    try
                    {
                        _ = RDFGeoExpression.GMLReader.Read(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }
                #endregion

                #region BOOLEAN CATEGORY
                case RDFModelEnums.RDFDatatypes.XSD_BOOLEAN:
                    if (bool.TryParse(literalValue, out bool outBool))
                    {
                        literalValue = outBool ? "true" : "false";
                    }
                    else
                    {
                        //Support intelligent detection of alternative boolean representations
                        if (AlternativesBoolTrue.Any(tl => tl.Equals(literalValue, StringComparison.OrdinalIgnoreCase)))
                            literalValue = "true";
                        else if (AlternativesBoolFalse.Any(tl => tl.Equals(literalValue, StringComparison.OrdinalIgnoreCase)))
                            literalValue = "false";
                        else
                            return (false, literalValue);
                    }
                    return (true, literalValue);
                #endregion

                #region DATETIME CATEGORY
                case RDFModelEnums.RDFDatatypes.XSD_DATETIME:
                    (bool,string) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ssZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:sszzz", "yyyy-MM-ddTHH:mm:ssZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.f", "yyyy-MM-ddTHH:mm:ss.fZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ss.fZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fzzz", "yyyy-MM-ddTHH:mm:ss.fZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ff", "yyyy-MM-ddTHH:mm:ss.ffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.ffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffzzz", "yyyy-MM-ddTHH:mm:ss.ffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fff", "yyyy-MM-ddTHH:mm:ss.fffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.fffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffzzz", "yyyy-MM-ddTHH:mm:ss.fffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffff", "yyyy-MM-ddTHH:mm:ss.ffffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffff", "yyyy-MM-ddTHH:mm:ss.fffffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffffzzz", "yyyy-MM-ddTHH:mm:ss.fffffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffff", "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                    if (!isValidDateTime.Item1) isValidDateTime = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                    return isValidDateTime;

                case RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP:
                    (bool,string) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ssZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:sszzz", "yyyy-MM-ddTHH:mm:ssZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ss.fZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fzzz", "yyyy-MM-ddTHH:mm:ss.fZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.ffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffzzz", "yyyy-MM-ddTHH:mm:ss.ffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.fffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffzzz", "yyyy-MM-ddTHH:mm:ss.fffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.fffffzzz", "yyyy-MM-ddTHH:mm:ss.fffffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                    if (!isValidDateTimeStamp.Item1) isValidDateTimeStamp = TryParseDateTime(literalValue, "yyyy-MM-ddTHH:mm:ss.ffffffzzz", "yyyy-MM-ddTHH:mm:ss.ffffffZ");
                    return isValidDateTimeStamp;

                case RDFModelEnums.RDFDatatypes.XSD_DATE:
                    (bool,string) isValidDate = TryParseDateTime(literalValue, "yyyy-MM-dd", "yyyy-MM-ddZ");
                    if (!isValidDate.Item1) isValidDate = TryParseDateTime(literalValue, "yyyy-MM-ddZ", "yyyy-MM-ddZ");
                    if (!isValidDate.Item1) isValidDate = TryParseDateTime(literalValue, "yyyy-MM-ddzzz", "yyyy-MM-ddZ");
                    return isValidDate;

                case RDFModelEnums.RDFDatatypes.XSD_TIME:
                    (bool,string) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss", "HH:mm:ssZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ssZ", "HH:mm:ssZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:sszzz", "HH:mm:ssZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.f", "HH:mm:ss.fZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.fZ", "HH:mm:ss.fZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.fzzz", "HH:mm:ss.fZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ff", "HH:mm:ss.ffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ffZ", "HH:mm:ss.ffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ffzzz", "HH:mm:ss.ffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.fff", "HH:mm:ss.fffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.fffZ", "HH:mm:ss.fffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.fffzzz", "HH:mm:ss.fffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ffff", "HH:mm:ss.ffffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ffffZ", "HH:mm:ss.ffffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ffffzzz", "HH:mm:ss.ffffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.fffff", "HH:mm:ss.fffffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.fffffZ", "HH:mm:ss.fffffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.fffffzzz", "HH:mm:ss.fffffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ffffff", "HH:mm:ss.ffffffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ffffffZ", "HH:mm:ss.ffffffZ");
                    if (!isValidTime.Item1) isValidTime = TryParseDateTime(literalValue, "HH:mm:ss.ffffffzzz", "HH:mm:ss.ffffffZ");
                    return isValidTime;

                case RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY:
                    (bool,string) isValidGMonthDay = TryParseDateTime(literalValue, "--MM-dd", "--MM-ddZ");
                    if (!isValidGMonthDay.Item1) isValidGMonthDay = TryParseDateTime(literalValue, "--MM-ddZ", "--MM-ddZ");
                    if (!isValidGMonthDay.Item1) isValidGMonthDay = TryParseDateTime(literalValue, "--MM-ddzzz", "--MM-ddZ");
                    return isValidGMonthDay;

                case RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH:
                    (bool,string) isValidGYearMonth = TryParseDateTime(literalValue, "yyyy-MM", "yyyy-MMZ");
                    if (!isValidGYearMonth.Item1) isValidGYearMonth = TryParseDateTime(literalValue, "yyyy-MMZ", "yyyy-MMZ");
                    if (!isValidGYearMonth.Item1) isValidGYearMonth = TryParseDateTime(literalValue, "yyyy-MMzzz", "yyyy-MMZ");
                    return isValidGYearMonth;

                case RDFModelEnums.RDFDatatypes.XSD_GYEAR:
                    (bool,string) isValidGYear = TryParseDateTime(literalValue, "yyyy", "yyyyZ");
                    if (!isValidGYear.Item1) isValidGYear = TryParseDateTime(literalValue, "yyyyZ", "yyyyZ");
                    if (!isValidGYear.Item1) isValidGYear = TryParseDateTime(literalValue, "yyyyzzz", "yyyyZ");
                    return isValidGYear;

                case RDFModelEnums.RDFDatatypes.XSD_GMONTH:
                    (bool,string) isValidGMonth = TryParseDateTime(literalValue, "--MM", "--MMZ");
                    if (!isValidGMonth.Item1) isValidGMonth = TryParseDateTime(literalValue, "--MMZ", "--MMZ");
                    if (!isValidGMonth.Item1) isValidGMonth = TryParseDateTime(literalValue, "--MMzzz", "--MMZ");
                    return isValidGMonth;

                case RDFModelEnums.RDFDatatypes.XSD_GDAY:
                    (bool,string) isValidGDay = TryParseDateTime(literalValue, "---dd", "---ddZ");
                    if (!isValidGDay.Item1) isValidGDay = TryParseDateTime(literalValue, "---ddZ", "---ddZ");
                    if (!isValidGDay.Item1) isValidGDay = TryParseDateTime(literalValue, "---ddzzz", "---ddZ");
                    return isValidGDay;

                case RDFModelEnums.RDFDatatypes.TIME_GENERALDAY:
                    bool isValidGeneralDay = Regex.IsMatch(literalValue, "---(0[1-9]|[1-9][0-9])(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?");
                    return (isValidGeneralDay, literalValue);

                case RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH:
                    bool isValidGeneralMonth = Regex.IsMatch(literalValue, "--(0[1-9]|1[0-9]|20)(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?");
                    return (isValidGeneralMonth, literalValue);

                case RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR:
                    bool isValidGeneralYear = Regex.IsMatch(literalValue, "-?([1-9][0-9]{3,}|0[0-9]{3})(Z|(\\+|-)((0[0-9]|1[0-3]):[0-5][0-9]|14:00))?");
                    return (isValidGeneralYear, literalValue);
                #endregion

                #region TIMESPAN CATEGORY
                case RDFModelEnums.RDFDatatypes.XSD_DURATION:
                    try
                    {
                        _ = XmlConvert.ToTimeSpan(literalValue);
                        return (true, literalValue);
                    }
                    catch { return (false, literalValue); }
                #endregion

                #region NUMERIC CATEGORY
                case RDFModelEnums.RDFDatatypes.XSD_DECIMAL:
                case RDFModelEnums.RDFDatatypes.OWL_REAL:
                    return decimal.TryParse(literalValue, NumberStyles.Integer | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal outDecimal)
                        ? (true, Convert.ToString(outDecimal, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.OWL_RATIONAL:
                    bool isValidOwlRational = owlRational.Value.Match(literalValue).Success;
                    return (isValidOwlRational, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_DOUBLE:
                    return double.TryParse(literalValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double outDouble)
                        ? (true, Convert.ToString(outDouble, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_FLOAT:
                    return float.TryParse(literalValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float outFloat)
                        ? (true, Convert.ToString(outFloat, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_INTEGER:
                    return decimal.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outInteger)
                        ? (true, Convert.ToString(outInteger, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_LONG:
                    return long.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out long outLong)
                        ? (true, Convert.ToString(outLong, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_INT:
                    return int.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int outInt)
                        ? (true, Convert.ToString(outInt, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_SHORT:
                    return short.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out short outShort)
                        ? (true, Convert.ToString(outShort, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_BYTE:
                    return sbyte.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out sbyte outSByte)
                        ? (true, Convert.ToString(outSByte, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG:
                    return ulong.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong outULong)
                        ? (true, Convert.ToString(outULong, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT:
                    return uint.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out uint outUInt)
                        ? (true, Convert.ToString(outUInt, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT:
                    return ushort.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort outUShort)
                        ? (true, Convert.ToString(outUShort, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE:
                    return byte.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte outByte)
                        ? (true, Convert.ToString(outByte, CultureInfo.InvariantCulture))
                        : (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER:
                    if (decimal.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outNPInteger))
                        return outNPInteger > 0 ? (false, literalValue)
                                                : (true, Convert.ToString(outNPInteger, CultureInfo.InvariantCulture));

                    return (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER:
                    if (decimal.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outNInteger))
                        return outNInteger > -1 ? (false, literalValue)
                                                : (true, Convert.ToString(outNInteger, CultureInfo.InvariantCulture));

                    return (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER:
                    if (decimal.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outNNInteger))
                        return outNNInteger < 0 ? (false, literalValue)
                                                : (true, Convert.ToString(outNNInteger, CultureInfo.InvariantCulture));

                    return (false, literalValue);

                case RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER:
                    if (decimal.TryParse(literalValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out decimal outPInteger))
                        return outPInteger < 1 ? (false, literalValue)
                                               : (true, Convert.ToString(outPInteger, CultureInfo.InvariantCulture));

                    return (false, literalValue);
                #endregion

                default: return (true, literalValue);
            }
        }

        /// <summary>
        /// Gets the numeric value of the given owl:rational typed literal
        /// </summary>
        internal static decimal ComputeOWLRationalValue(RDFTypedLiteral typedLiteral)
        {
            string[] owlRationalParts = typedLiteral.Value.Split('/');
            return decimal.Parse(owlRationalParts[0]) / decimal.Parse(owlRationalParts[1]);
        }

        /// <summary>
        /// Extracts and registers the datatypes contained in the given graph
        /// </summary>
        internal static void ExtractAndRegisterDatatypes(RDFGraph graph)
        {
            foreach (RDFDatatype datatypeDefinition in graph.ExtractDatatypeDefinitions())
                RDFDatatypeRegister.AddDatatype(datatypeDefinition);
        }
        #endregion
    }
}