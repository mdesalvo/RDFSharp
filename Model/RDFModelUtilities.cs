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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        /// Gets the string representation of the given term status
        /// </summary>
        internal static String GetTermStatus(RDFModelEnums.RDFTermStatus termStatus) {
            switch (termStatus) {
                case RDFModelEnums.RDFTermStatus.Stable:
                    return "stable";
                case RDFModelEnums.RDFTermStatus.Unstable:
                    return "unstable";
                case RDFModelEnums.RDFTermStatus.Testing:
                    return "testing";
                case RDFModelEnums.RDFTermStatus.Archaic:
                    return "archaic";

                default:
                    return "stable";
            }
        }

        /// <summary>
        /// Gets the Uri corresponding to the given string
        /// </summary>
        internal static Uri GetUriFromString(String uriString) {
            Uri tempUri       = null;
            if (uriString    != null && uriString.Trim() != String.Empty) {

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
        /// It starts by default with "bnode:", because it doesn't have to be dependant on namespaces.
        /// </summary>
        internal static Uri GenerateAnonUri() {
            return new Uri("bnode:" + Guid.NewGuid());
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
        /// Tries to parse the given string in order to build the corresponding datatype
        /// </summary>
        internal static RDFDatatype GetDatatypeFromString(String datatypeString) {
            if (datatypeString     != null && datatypeString.Trim() != String.Empty) {
                Uri datatypeUri     = GetUriFromString(datatypeString);
                if (datatypeUri    == null) {
                    throw new RDFModelException("Cannot create RDFDatatype because given \"datatypeString\" (" + datatypeString + ") parameter cannot be converted to a valid Uri");
                }
                String type         = null;
                String ns           = null;
                RDFDatatype dt      = null;

                // e.g.:  "http://www.w3.org/2001/XMLSchema#integer"
                if (datatypeUri.Fragment != String.Empty) {
                    type            = datatypeUri.Fragment.TrimStart(new Char[] { '#' });    //"integer"
                    ns              = datatypeUri.AbsoluteUri.TrimEnd(type.ToCharArray());   //"http://www.w3.org/2001/XMLSchema#"
                }
                // e.g.:  "http://example.org/integer" or "ex:integer"
                else {
                    type            = datatypeUri.Segments[datatypeUri.Segments.Length - 1]; //"integer"
                    ns              = datatypeUri.AbsoluteUri.TrimEnd(type.ToCharArray());   //"http://example.org/" or "ex:"
                }

                //First try to search the register for prefix and datatype
                if (ns.EndsWith(":")) {
                    ns              = ns.TrimEnd(':');
                    dt              = RDFDatatypeRegister.GetByPrefixAndDatatype(ns, type);
                }

                //If nothing found, try to search the register for namespace and datatype
                if(dt              == null) {
                    dt              = RDFDatatypeRegister.GetByNamespaceAndDatatype(ns, type);
                    
                    //If nothing found, we must create and register a new datatype
                    if (dt         == null) {

                        //First try to find a namespace to work with
                        var nSpace  = (RDFNamespaceRegister.GetByNamespace(ns) ?? GenerateNamespace(ns, true));

                        //If nothing found, we also have to create a new datatype
                        dt          = new RDFDatatype(nSpace.Prefix, nSpace.Namespace, type, RDFModelEnums.RDFDatatypeCategory.String);

                    }
                }

                return dt;
            }
            throw new RDFModelException("Cannot create RDFDatatype because given \"datatypeString\" parameter is null or empty");
        }

        /// <summary>
        /// Validates the value of the given typed literal against the category of its datatype
        /// </summary>
        internal static Boolean ValidateTypedLiteral(RDFTypedLiteral typedLiteral) {
            if (typedLiteral != null) {
                Boolean validateResponse             = true;
                switch (typedLiteral.Datatype.Category) { 


                    //STRING TYPES
                    case RDFModelEnums.RDFDatatypeCategory.String:

                        //ANYURI
                        if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "anyURI"))) {
                            Uri outUri;
                            if (!Uri.TryCreate(typedLiteral.Value, UriKind.Absolute, out outUri)) {
                                validateResponse     = false;
                            }
                        }

                        //XML_LITERAL
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.RDF.PREFIX, "XMLLiteral"))) {
                            try {
                                XDocument.Parse(typedLiteral.Value);
                            }
                            catch {
                                validateResponse     = false;
                            }
                        }

                        //NAME
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "Name"))) {
                            try {
                                XmlConvert.VerifyName(typedLiteral.Value);
                            }
                            catch {
                                validateResponse     = false;
                            }
                        }

                        //NCNAME
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "NCName"))) {
                            try {
                                XmlConvert.VerifyNCName(typedLiteral.Value);
                            }
                            catch {
                                validateResponse     = false;
                            }
                        }

                        //TOKEN
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "token"))) {
                            try {
                                XmlConvert.VerifyTOKEN(typedLiteral.Value);
                            }
                            catch {
                                validateResponse     = false;
                            }
                        }

                        //NMTOKEN
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "NMToken"))) {
                            try {
                                XmlConvert.VerifyNMTOKEN(typedLiteral.Value);
                            }
                            catch {
                                validateResponse     = false;
                            }
                        }

                        //NORMALIZED_STRING
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "normalizedString"))) {
                            if (typedLiteral.Value.Contains('\r') || typedLiteral.Value.Contains('\n') || typedLiteral.Value.Contains('\t')) {
                                validateResponse     = false;
                            }
                        }

                        //LANGUAGE
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "language"))) {
                            if (!Regex.IsMatch(typedLiteral.Value, "^[a-zA-Z]+([\\-][a-zA-Z0-9]+)*$")) {
                                validateResponse     = false;
                            }
                        }

                        //BASE64_BINARY
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "base64Binary"))) {
                            try {
                                Convert.FromBase64String(typedLiteral.Value);
                            }
                            catch {
                                validateResponse     = false;
                            }
                        }

                        //HEX_BINARY
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "hexBinary"))) {
                            if ((typedLiteral.Value.Length % 2 != 0) || (!Regex.IsMatch(typedLiteral.Value, @"^[a-fA-F0-9]+$"))) {
                                validateResponse     = false;
                            }
                        }

                        break;


                    //BOOLEAN TYPES
                    case RDFModelEnums.RDFDatatypeCategory.Boolean:
                        Boolean outBool;
                        if (!Boolean.TryParse(typedLiteral.Value, out outBool)) {
                            validateResponse         = false;
                        }
                        break;


                    //DATETIME TYPES
                    case RDFModelEnums.RDFDatatypeCategory.DateTime:

                        //DATETIME
                        if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "dateTime"))) {
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
                                            validateResponse = false;
                                        }
                                    }
                                }
                            }
                        }

                        //DATE
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "date"))) {
                            try {
                                DateTime.ParseExact(typedLiteral.Value, "yyyy-MM-ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                try {
                                    DateTime.ParseExact(typedLiteral.Value, "yyyy-MM-dd",  CultureInfo.InvariantCulture);
                                }
                                catch {
                                    validateResponse = false;
                                }
                            }
                        }

                        //TIME
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "time"))) {
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
                                            validateResponse = false;
                                        }
                                    }
                                }
                            }
                        }

                        //G_MONTH_DAY
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gMonthDay"))) {
                            try {
                                DateTime.ParseExact(typedLiteral.Value, "--MM-ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                try {
                                    DateTime.ParseExact(typedLiteral.Value, "--MM-dd",  CultureInfo.InvariantCulture);
                                }
                                catch {
                                    validateResponse = false;
                                }
                            }
                        }

                        //G_YEAR_MONTH
                        else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gYearMonth"))) {
                            try {
                                DateTime.ParseExact(typedLiteral.Value, "yyyy-MMK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                try {
                                    DateTime.ParseExact(typedLiteral.Value, "yyyy-MM",  CultureInfo.InvariantCulture);
                                }
                                catch {
                                    validateResponse = false;
                                }
                            }
                        }

						//G_YEAR
						else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gYear"))) {
							try {
                                DateTime.ParseExact(typedLiteral.Value, "yyyyK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                try {
                                    DateTime.ParseExact(typedLiteral.Value, "yyyy",  CultureInfo.InvariantCulture);
                                }
                                catch {
                                    validateResponse = false;
                                }
                            }
						}
						
						//G_MONTH
						else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gMonth"))) {
							try {
                                DateTime.ParseExact(typedLiteral.Value, "MMK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                try {
                                    DateTime.ParseExact(typedLiteral.Value, "MM",  CultureInfo.InvariantCulture);
                                }
                                catch {
                                    validateResponse = false;
                                }
                            }
						}
						
						//G_DAY
						else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gDay"))) {
							try {
                                DateTime.ParseExact(typedLiteral.Value, "ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                try {
                                    DateTime.ParseExact(typedLiteral.Value, "dd",  CultureInfo.InvariantCulture);
                                }
                                catch {
                                    validateResponse = false;
                                }
                            }
						}

                        break;


                    //TIMESPAN TYPES
                    case RDFModelEnums.RDFDatatypeCategory.TimeSpan:
                        try {
                            XmlConvert.ToTimeSpan(typedLiteral.Value);
                        }
                        catch {
                            validateResponse         = false;
                        }
                        break;


                    //NUMERIC TYPES
                    case RDFModelEnums.RDFDatatypeCategory.Numeric:
                        Decimal outDecimal;
                        if (Decimal.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outDecimal)) { 
                            
                            //INTEGER
                            if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "integer"))            ||
                                typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "int"))                ||
                                typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "positiveInteger"))    ||
                                typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "negativeInteger"))    ||
                                typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "nonPositiveInteger")) ||
                                typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "nonNegativeInteger"))) {
                                Int32 outInteger;
                                if (Int32.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outInteger)) { 
                                    
                                    //SUB-INTEGER
                                    if(typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "positiveInteger"))       ||
                                       typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "nonNegativeInteger")))    {
                                        if (outInteger < 0) {
                                            validateResponse = false;
                                        }
                                    }
                                    else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "negativeInteger")) ||
                                        typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX,  "nonPositiveInteger")))  {
                                        if (outInteger >= 0) {
                                            validateResponse = false;
                                        }
                                    }

                                }
                                else {
                                    validateResponse = false;
                                }
                            }

                            //LONG
                            else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "long"))) {
                                Int64 outlong;
                                if (!Int64.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outlong)) { 
                                    validateResponse = false;
                                }
                            }

                            //SHORT
                            else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "short"))) {
                                Int16 outShort;
                                if (!Int16.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outShort)) {
                                    validateResponse = false;
                                }
                            }

                            //FLOAT
                            else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "float"))) {
                                Single outFloat;
                                if (!Single.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outFloat)) {
                                    validateResponse = false;
                                }
                            }

                            //DOUBLE
                            else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "double"))) {
                                Double outDouble;
                                if (!Double.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outDouble)) {
                                    validateResponse = false;
                                }
                            }

                            //BYTE
                            else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "byte"))) {
                                SByte outSByte;
                                if (!SByte.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outSByte)) {
                                    validateResponse = false;
                                }
                            }

                            //UNSIGNED TYPES
                            else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "unsignedByte"))  ||
                                     typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "unsignedShort")) ||
                                     typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "unsignedInt"))   ||
                                     typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "unsignedLong")))  {
                                
                                //UNSIGNED BYTE
                                if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "unsignedByte"))) {
                                    Byte outByte;
                                    if (!Byte.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outByte)) {
                                        validateResponse = false;
                                    }    
                                }

                                //UNSIGNED SHORT
                                else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "unsignedShort"))) {
                                    UInt16 outUShort;
                                    if (!UInt16.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outUShort)) {
                                        validateResponse = false;
                                    }
                                }

                                //UNSIGNED INT
                                else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "unsignedInt"))) {
                                    UInt32 outUInt;
                                    if (!UInt32.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outUInt)) {
                                        validateResponse = false;
                                    }
                                }

                                //UNSIGNED LONG
                                else if (typedLiteral.Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "unsignedLong"))) {
                                    UInt64 outULong;
                                    if (!UInt64.TryParse(typedLiteral.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out outULong)) {
                                        validateResponse = false;
                                    }
                                }

                            }

                        }
                        else {
                            validateResponse = false;
                        }

                        break;


                }
                return validateResponse;
            }
            throw new RDFModelException("Cannot validate RDFTypedLiteral because given \"typedLiteral\" parameter is null.");
        }
        #endregion

        #region Serialization

        #region RDFNTriples
        /// <summary>
        /// Regex to catch 8-byte unicodes in N-Triples
        /// </summary>
        internal static readonly Regex regexU8  = new Regex(@"\\U([0-9A-Fa-f]{8})", RegexOptions.Compiled);
        /// <summary>
        /// Regex to catch 4-byte unicodes in N-Triples
        /// </summary>
        internal static readonly Regex regexU4  = new Regex(@"\\u([0-9A-Fa-f]{4})", RegexOptions.Compiled);
        /// <summary>
        /// Regex to parse N-Triples focusing on predicate position 
        /// </summary>
        internal static readonly Regex regexNT  = new Regex(@"(?<pred>\s+<[^>]+>\s+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        /// <summary>
        /// Regex to detect presence of a plain literal with language tag within a given N-Triple
        /// </summary>
        internal static readonly Regex regexLPL = new Regex(@"@[a-zA-Z]+(\-[a-zA-Z0-9]+)*$", RegexOptions.Compiled);
        /// <summary>
        /// Regex to detect presence of starting " in the value of a given N-Triple literal
        /// </summary>
        internal static readonly Regex regexSqt = new Regex(@"^""", RegexOptions.Compiled);
        /// <summary>
        /// Regex to detect presence of ending " in the value of a given N-Triple literal
        /// </summary>
        internal static readonly Regex regexEqt = new Regex(@"""$", RegexOptions.Compiled);


        /// <summary>
        /// Tries to parse the given N-Triple
        /// </summary>
        internal static String[] ParseNTriple(String ntriple) {
            String[] tokens        = new String[3];

            //A legal NTriple starts with "_:" of blanks or "<" of non-blanks
            if (ntriple.StartsWith("_:") || ntriple.StartsWith("<")) {

                //Parse NTriple by exploiting surrounding spaces and angle brackets of predicate
                tokens             = regexNT.Split(ntriple, 2);

                //An illegal NTriple cannot be splitted into 3 parts with this regex
                if (tokens.Length != 3) {
                    throw new Exception("found illegal N-Triple, predicate must be surrounded by \" <\" and \"> \"");
                }

                //Check subject for well-formedness
                tokens[0]          = tokens[0].Trim(new Char[] { ' ', '\n', '\r', '\t' });
                if (tokens[0].Contains(" ")) {
                    throw new Exception("found illegal N-Triple, subject Uri cannot contain spaces");
                }
                if ((tokens[0].StartsWith("<") && !tokens[0].EndsWith(">")) ||
                    (tokens[0].StartsWith("_:") && tokens[0].EndsWith(">")) ||
                    (tokens[0].Count(c => c.Equals('<')) > 1) ||
                    (tokens[0].Count(c => c.Equals('>')) > 1)) {
                    throw new Exception("found illegal N-Triple, subject Uri is not well-formed");
                }

                //Check predicate for well-formedness
                tokens[1]          = tokens[1].Trim(new Char[] { ' ', '\n', '\r', '\t' });
                if (tokens[1].Contains(" ")) {
                    throw new Exception("found illegal N-Triple, predicate Uri cannot contain spaces");
                }
                if ((tokens[1].Count(c => c.Equals('<')) > 1) ||
                    (tokens[1].Count(c => c.Equals('>')) > 1)) {
                    throw new Exception("found illegal N-Triple, predicate Uri is not well-formed");
                }

                //Check object for well-formedness
                tokens[2]          = tokens[2].Trim(new Char[] { ' ', '\n', '\r', '\t' });
                if (tokens[2].StartsWith("<")) {
                    if (tokens[2].Contains(" ")) {
                        throw new Exception("found illegal N-Triple, object Uri cannot contain spaces");
                    }
                    if ((!tokens[2].EndsWith(">") ||
                         (tokens[2].Count(c => c.Equals('<')) > 1) ||
                         (tokens[2].Count(c => c.Equals('>')) > 1))) {
                        throw new Exception("found illegal N-Triple, object Uri is not well-formed");
                    }
                }
                else if (tokens[2].StartsWith("_:")) {
                    if (tokens[2].EndsWith(">")) {
                        throw new Exception("found illegal N-Triple, object Uri is not well-formed");
                    }
                }

            }
            else {
                throw new Exception("found illegal N-Triple, must start with \"_:\" or with \"<\"");
            }

            return tokens;
        }

        /// <summary>
        /// Turns back ASCII-encoded Unicodes into Unicodes. 
        /// </summary>
        internal static String ASCII_To_Unicode(String asciiString) {
            if (asciiString != null) {
                asciiString = regexU8.Replace(asciiString, match => ((Char)Int64.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));
                asciiString = regexU4.Replace(asciiString, match => ((Char)Int32.Parse(match.Groups[1].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture));
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

        #region RDFXml
        /// <summary>
        /// Gives the "rdf:RDF" root node of the document
        /// </summary>
        internal static XmlNode GetRdfRootNode(XmlDocument xmlDoc, XmlNamespaceManager nsMgr) {
            XmlNode rdf = 
                (xmlDoc.SelectSingleNode(RDFVocabulary.RDF.PREFIX + ":RDF", nsMgr) ??
                    xmlDoc.SelectSingleNode("RDF", nsMgr));

            //Invalid RDF/XML file: root node is neither "rdf:RDF" or "RDF"
            if (rdf    == null) {
                throw new Exception("Given file has not a valid \"rdf:RDF\" or \"RDF\" root node");
            }

            return rdf;
        }

        /// <summary>
        /// Gives the collection of "xmlns" attributes of the "rdf:RDF" root node
        /// </summary>
        internal static XmlAttributeCollection GetXmlnsNamespaces(XmlNode rdfRDF, XmlNamespaceManager nsMgr) {
            XmlAttributeCollection xmlns = rdfRDF.Attributes;
            if (xmlns != null && xmlns.Count > 0) {

                IEnumerator iEnum        = xmlns.GetEnumerator();
                while (iEnum != null && iEnum.MoveNext()) {
                    XmlAttribute attr    = (XmlAttribute)iEnum.Current;
                    if (attr.LocalName.ToUpperInvariant() != "XMLNS") {

                        //Try to resolve the current namespace against the namespace register; 
                        //if not resolved, create new namespace with scope limited to actual node
                        RDFNamespace ns  =  
                        (RDFNamespaceRegister.GetByPrefix(attr.LocalName) ??
                                RDFNamespaceRegister.GetByNamespace(attr.Value) ??
                                    new RDFNamespace(attr.LocalName, attr.Value));

                        nsMgr.AddNamespace(ns.Prefix, ns.Namespace.ToString());

                    }
                }

            }
            return xmlns;
        }

        /// <summary>
        /// Gives the subj node extracted from the attribute list of the current element 
        /// </summary>
        internal static RDFResource GetSubjectNode(XmlNode subjNode, Uri xmlBase, RDFGraph result) {
            RDFResource subj             = null;

            //If there are attributes, search them for the one representing the subj
            if (subjNode.Attributes     != null && subjNode.Attributes.Count > 0) {

                //We are interested in finding the "rdf:about" node for the subj
                XmlAttribute rdfAbout    = GetRdfAboutAttribute(subjNode);
                if (rdfAbout != null) {
                    //Attribute found, but we must check if it is "rdf:ID", "rdf:nodeID" or a relative Uri: 
                    //in this case it must be resolved against the xmlBase namespace, or else it remains the same
                    String rdfAboutValue = RDFModelUtilities.ResolveRelativeNode(rdfAbout, xmlBase);
                    subj      = new RDFResource(rdfAboutValue);
                }

                //If "rdf:about" attribute has been found for the subj, we must
                //check if the node is not a standard "rdf:Description": this is
                //the case we can directly build a triple with "rdf:type" pred
                if (subj     != null && !CheckIfRdfDescriptionNode(subjNode)) {
                    RDFResource obj      = null;
                    if (subjNode.NamespaceURI == String.Empty) {
                        obj   = new RDFResource(xmlBase + subjNode.LocalName);
                    }
                    else {
                        obj   = new RDFResource(subjNode.NamespaceURI + subjNode.LocalName);
                    }
                    result.AddTriple(new RDFTriple(subj, RDFVocabulary.RDF.TYPE, obj));
                }

            }

            //There are no attributes, so there's only one way we can handle this element:
            //if it is a standard rdf:Description, it is a blank Subject
            else {
                if (CheckIfRdfDescriptionNode(subjNode)) {
                    subj      = new RDFResource();
                }
            }

            return subj;
        }

        /// <summary>
        /// Checks if the given attribute is absolute Uri, relative Uri, "rdf:ID" relative Uri, "rdf:nodeID" blank node Uri
        /// </summary>
        internal static String ResolveRelativeNode(XmlAttribute attr, Uri xmlBase) {
            if (attr != null && xmlBase != null) {
                String attrValue    = attr.Value;

                //"rdf:ID" relative Uri: must be resolved against the xmlBase namespace
                if (attr.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":ID", StringComparison.Ordinal) ||
                    attr.LocalName.Equals("ID", StringComparison.Ordinal)) {
                    attrValue       = RDFModelUtilities.GetUriFromString(xmlBase + attrValue).ToString();
                }

                //"rdf:nodeID" relative Uri: must be resolved against the "bnode:" prefix
                else if (attr.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":nodeID", StringComparison.Ordinal) ||
                         attr.LocalName.Equals("nodeID", StringComparison.Ordinal)) {
                     if (!attrValue.StartsWith("bnode:")) {
                          attrValue = "bnode:" + attrValue;
                     }
                }

                //"rdf:about" or "rdf:resource" relative Uri: must be resolved against the xmlBase namespace
                else if (RDFModelUtilities.GetUriFromString(attrValue) == null) {
                    attrValue       = RDFModelUtilities.GetUriFromString(xmlBase + attrValue).ToString();
                }

                return attrValue;
            }
            throw new RDFModelException("Cannot resolve relative node because given \"attr\" or \"xmlBase\" parameters are null");
        }

        /// <summary>
        /// Verify if we are on a standard rdf:Description element
        /// </summary>
        internal static Boolean CheckIfRdfDescriptionNode(XmlNode subjNode) {
            Boolean result = (subjNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Description", StringComparison.Ordinal) ||
                              subjNode.LocalName.Equals("Description", StringComparison.Ordinal));
            return result;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF subj
        /// </summary>
        internal static XmlAttribute GetRdfAboutAttribute(XmlNode subjNode) {
            //rdf:about
            XmlAttribute rdfAbout = 
                (subjNode.Attributes[RDFVocabulary.RDF.PREFIX + ":about"] ??
                    (subjNode.Attributes["about"] ??
                        (subjNode.Attributes[RDFVocabulary.RDF.PREFIX + ":nodeID"] ??
                            (subjNode.Attributes["nodeID"] ??
                                (subjNode.Attributes[RDFVocabulary.RDF.PREFIX + ":ID"] ??
                                    subjNode.Attributes["ID"])))));
            return rdfAbout;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF object
        /// </summary>
        internal static XmlAttribute GetRdfResourceAttribute(XmlNode predNode) {
            //rdf:Resource
            XmlAttribute rdfResource = 
                (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":resource"] ??
                    (predNode.Attributes["resource"] ??
                        (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":nodeID"] ??
                            predNode.Attributes["nodeID"])));
            return rdfResource;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF typed literal datatype
        /// </summary>
        internal static XmlAttribute GetRdfDatatypeAttribute(XmlNode predNode) {
            //rdf:datatype
            XmlAttribute rdfDatatype = 
                (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":datatype"] ??
                    predNode.Attributes["datatype"]);
            return rdfDatatype;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF plain literal language
        /// </summary>
        internal static XmlAttribute GetXmlLangAttribute(XmlNode predNode) {
            //xml:lang
            XmlAttribute xmlLang = 
                (predNode.Attributes[RDFVocabulary.XML.PREFIX + ":lang"] ??
                    predNode.Attributes["lang"]);
            return xmlLang;
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Collection"
        /// </summary>
        internal static XmlAttribute GetParseTypeCollectionAttribute(XmlNode predNode) {
            XmlAttribute rdfCollection = 
                (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":parseType"] ??
                    predNode.Attributes["parseType"]);

            return ((rdfCollection != null && rdfCollection.Value.Equals("Collection", StringComparison.Ordinal)) ? rdfCollection : null);
        }

        /// <summary>
        /// Given an element, return the attribute which can correspond to the RDF parseType "Literal"
        /// </summary>
        internal static XmlAttribute GetParseTypeLiteralAttribute(XmlNode predNode) {
            XmlAttribute rdfLiteral =
                (predNode.Attributes[RDFVocabulary.RDF.PREFIX + ":parseType"] ??
                    predNode.Attributes["parseType"]);

            return ((rdfLiteral != null && rdfLiteral.Value.Equals("Literal", StringComparison.Ordinal)) ? rdfLiteral : null);
        }

        /// <summary>
        /// Given an attribute representing a RDF collection, iterates on its constituent elements
        /// to build its standard reification triples. 
        /// </summary>
        internal static void ParseCollectionElements(Uri xmlBase, XmlNode predNode, RDFResource subj,
                                                     RDFResource pred, RDFGraph result) {

            //Attach the collection as the blank object of the current pred
            RDFResource  obj              = new RDFResource();
            result.AddTriple(new RDFTriple(subj, pred, obj));

            //Iterate on the collection items to reify it
            if (predNode.HasChildNodes) {
                IEnumerator elems         = predNode.ChildNodes.GetEnumerator();
                while (elems != null && elems.MoveNext()) {
                    XmlNode elem          = (XmlNode)elems.Current;

                    //Try to get items as "rdf:about" attributes, or as "rdf:resource"
                    XmlAttribute elemUri  = 
					    (GetRdfAboutAttribute(elem) ??
                             GetRdfResourceAttribute(elem));
                    if (elemUri          != null) {

                        //Sanitize eventual blank node or relative value, depending on attribute found
                        elemUri.Value     = RDFModelUtilities.ResolveRelativeNode(elemUri, xmlBase);

                        // obj -> rdf:type -> rdf:list
                        result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));

                        // obj -> rdf:first -> res
                        result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.FIRST, new RDFResource(elemUri.Value)));

                        //Last element of a collection must give a triple to a "rdf:nil" object
                        RDFResource newObj;
                        if (elem         != predNode.ChildNodes.Item(predNode.ChildNodes.Count - 1)) {
                            // obj -> rdf:rest -> newObj
                            newObj        = new RDFResource();
                        }
                        else {
                            // obj -> rdf:rest -> rdf:nil
                            newObj        = RDFVocabulary.RDF.NIL;
                        }
                        result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.REST, newObj));
                        obj               = newObj;

                    }
                }
            }
        }

        /// <summary>
        /// Given the metadata of a graph and a collection resource, it reconstructs the RDF collection and returns it as a list of nodes
        /// This is needed for building the " rdf:parseType=Collection>" RDF/XML abbreviation goody for collections of resources
        /// </summary>
        internal static List<XmlNode> ReconstructCollection(RDFGraphMetadata rdfGraphMetadata, RDFResource tripleObject, XmlDocument rdfDoc) {
            List<XmlNode> result          = new List<XmlNode>();
            Boolean nilFound              = false;
            XmlNode collElementToAppend   = null;
            XmlAttribute collElementAttr  = null;
            XmlText collElementAttrText   = null;

            //Iterate the elements of the collection until the last one (pointing to next="rdf:nil")
            while (!nilFound) {
                var collElement           = rdfGraphMetadata.Collections[tripleObject];
                collElementToAppend       = rdfDoc.CreateNode(XmlNodeType.Element, RDFVocabulary.RDF.PREFIX + ":Description", RDFVocabulary.RDF.BASE_URI);
                collElementAttr           = rdfDoc.CreateAttribute(RDFVocabulary.RDF.PREFIX + ":about", RDFVocabulary.RDF.BASE_URI);
                collElementAttrText       = rdfDoc.CreateTextNode(collElement.ItemValue.ToString());
                if (collElementAttrText.InnerText.StartsWith("bnode:")) {
                    collElementAttrText.InnerText = collElementAttrText.InnerText.Replace("bnode:", String.Empty);
                }
                collElementAttr.AppendChild(collElementAttrText);
                collElementToAppend.Attributes.Append(collElementAttr);
                result.Add(collElementToAppend);

                //Verify if this is the last element of the collection (pointing to next="rdf:nil")
                if (collElement.ItemNext.ToString().Equals(RDFVocabulary.RDF.NIL.ToString(), StringComparison.Ordinal)) {
                    nilFound              = true;
                }
                else {
                    tripleObject          = collElement.ItemNext as RDFResource;
                }

            }

            return result;
        }

        /// <summary>
        /// Verify if we are on a standard rdf:[Bag|Seq|Alt] element
        /// </summary>
        internal static Boolean CheckIfRdfContainerNode(XmlNode containerNode) {
            Boolean result = (containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Bag", StringComparison.Ordinal) || containerNode.LocalName.Equals("Bag", StringComparison.Ordinal) ||
                              containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Seq", StringComparison.Ordinal) || containerNode.LocalName.Equals("Seq", StringComparison.Ordinal) ||
                              containerNode.LocalName.Equals(RDFVocabulary.RDF.PREFIX + ":Alt", StringComparison.Ordinal) || containerNode.LocalName.Equals("Alt", StringComparison.Ordinal));
            return result;
        }

        /// <summary>
        /// Given an element, return the child element which can correspond to the RDF container
        /// </summary>
        internal static XmlNode GetContainerNode(XmlNode predNode) {
            //A container is the first child of the given node and it has no attributes.
            //Its localname must be the canonical "rdf:[Bag|Seq|Alt]", so we check for this.
            if (predNode.HasChildNodes) {
                XmlNode containerNode  = predNode.FirstChild;
                Boolean isRdfContainer = CheckIfRdfContainerNode(containerNode);
                if (isRdfContainer) {
                    return containerNode;
                }
            }
            return null;
        }

        /// <summary>
        /// Given an element representing a RDF container, iterates on its constituent elements
        /// to build its standard reification triples. 
        /// </summary>
        internal static void ParseContainerElements(RDFModelEnums.RDFContainerTypes contType, XmlNode container,
                                                    RDFResource subj, RDFResource pred, RDFGraph result) {

            //Attach the container as the blank object of the current pred
            RDFResource  obj                 = new RDFResource();
            result.AddTriple(new RDFTriple(subj, pred, obj));

            //obj -> rdf:type -> rdf:[Bag|Seq|Alt]
            switch (contType) {
                case RDFModelEnums.RDFContainerTypes.Bag:
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.BAG));
                    break;
                case RDFModelEnums.RDFContainerTypes.Seq:
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.SEQ));
                    break;
                default:
                    result.AddTriple(new RDFTriple(obj, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.ALT));
                    break;
            }

            //Iterate on the container items
            if (container.HasChildNodes) {
                IEnumerator elems              = container.ChildNodes.GetEnumerator();
                List<String> elemVals          = new List<String>();
                while (elems != null && elems.MoveNext()) {
                    XmlNode elem               = (XmlNode)elems.Current;
                    XmlAttribute elemUri       = GetRdfResourceAttribute(elem);

                    #region Container Resource Item
                    //This is a container of resources
                    if (elemUri               != null) {

                        //Sanitize eventual blank node value detected by presence of "nodeID" attribute
                        if (elemUri.LocalName.Equals("nodeID", StringComparison.Ordinal)) {
                            if (!elemUri.Value.StartsWith("bnode:")) {
                                 elemUri.Value = "bnode:" + elemUri.Value;
                            }
                        }

                        //obj -> rdf:_N -> VALUE 
                        if (contType          == RDFModelEnums.RDFContainerTypes.Alt) {
                            if (!elemVals.Contains(elemUri.Value)) {
                                elemVals.Add(elemUri.Value);
                                result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), new RDFResource(elemUri.Value)));
                            }
                        }
                        else {
                            result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), new RDFResource(elemUri.Value)));
                        }

                    }
                    #endregion

                    #region Container Literal Item
                    //This is a container of literals
                    else {

                        //Parse the literal contained in the item
                        RDFLiteral literal     = null;
                        XmlAttribute attr      = GetRdfDatatypeAttribute(elem);
                        if (attr              != null) {
                            literal            = new RDFTypedLiteral(elem.InnerText, RDFModelUtilities.GetDatatypeFromString(attr.InnerText));
                        }
                        else {
                            attr               = GetXmlLangAttribute(elem);
                            literal            = new RDFPlainLiteral(elem.InnerText, (attr != null ? attr.InnerText : String.Empty));
                        }

                        //obj -> rdf:_N -> VALUE 
                        if (contType          == RDFModelEnums.RDFContainerTypes.Alt) {
                            if (!elemVals.Contains(literal.ToString())) {
                                 elemVals.Add(literal.ToString());
                                 result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), literal));
                            }
                        }
                        else {
                            result.AddTriple(new RDFTriple(obj, new RDFResource(RDFVocabulary.RDF.BASE_URI + elem.LocalName), literal));
                        }

                    }
                    #endregion

                }
            }

        }
        #endregion

        #region Turtle
        /// <summary>
        /// Regex to catch literals which must be escaped as long literals in Turtle
        /// </summary>
        internal static readonly Regex regexTTL = new Regex("[\n\r\t\"]", RegexOptions.Compiled);
        #endregion

        #endregion

    }

}