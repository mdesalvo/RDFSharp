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

using RDFSharp.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFTurtle is responsible for managing serialization to and from Turtle data format.
    /// </summary>
    internal static class RDFTurtle
    {

        #region Properties
        /// <summary>
        /// Regex to catch literals which must be escaped as long literals in Turtle
        /// </summary>
        internal static readonly Regex regexTTL = new Regex("[\n\r\t\"]", RegexOptions.Compiled);
        #endregion

        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given graph to the given filepath using Turtle data format.
        /// </summary>
        internal static void Serialize(RDFGraph graph, string filepath)
        {
            Serialize(graph, new FileStream(filepath, FileMode.Create));
        }

        /// <summary>
        /// Serializes the given graph to the given stream using Turtle data format.
        /// </summary>
        internal static void Serialize(RDFGraph graph, Stream outputStream)
        {
            try
            {

                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, Encoding.UTF8))
                {

                    #region prefixes
                    //Write the namespaces collected by the graph
                    var prefixes = RDFModelUtilities.GetGraphNamespaces(graph);
                    foreach (var ns in prefixes.OrderBy(n => n.NamespacePrefix))
                        sw.WriteLine(string.Concat("@prefix ", ns.NamespacePrefix, ": <", ns.NamespaceUri, ">."));
                    sw.WriteLine(string.Concat("@base <", graph.Context, ">.\n"));
                    #endregion

                    #region linq
                    //Group the graph's triples by subj and pred
                    var groupedList = (from triple in graph
                                       orderby triple.Subject.ToString(), triple.Predicate.ToString()
                                       group triple by new
                                       {
                                           subj = triple.Subject.ToString(),
                                           pred = triple.Predicate.ToString()
                                       });
                    var groupedListLast = groupedList.LastOrDefault();
                    #endregion

                    #region triples
                    string actualSubj = string.Empty;
                    string abbreviatedSubj = string.Empty;
                    string actualPred = string.Empty;
                    string abbreviatedPred = string.Empty;
                    const string spaceConst = " ";
                    StringBuilder result = new StringBuilder();

                    //Iterate over the calculated groups
                    foreach (var group in groupedList)
                    {
                        var groupLast = group.Last();

                        #region subj
                        //Reset the flag of subj printing for the new iteration
                        bool subjPrint = false;
                        //New subj found: write the finished Turtle token to the file, then start collecting the new one
                        if (!actualSubj.Equals(group.Key.subj, StringComparison.Ordinal))
                        {
                            if (result.Length > 0)
                            {
                                result.Replace(";", ".", result.Length - 4, 1);
                                sw.Write(result.ToString());
                                result.Remove(0, result.Length - 1);
                            }
                            actualSubj = group.Key.subj;
                            actualPred = string.Empty;
                            if (!actualSubj.StartsWith("_:"))
                            {
                                abbreviatedSubj = RDFQueryPrinter.PrintPatternMember(RDFQueryUtilities.ParseRDFPatternMember(actualSubj), prefixes);
                            }
                            else
                            {
                                abbreviatedSubj = actualSubj;
                            }
                            result.Append(string.Concat(abbreviatedSubj, " "));
                            subjPrint = true;
                        }
                        #endregion

                        #region predObjList
                        //Iterate over the triples of the current group
                        foreach (var triple in group)
                        {

                            #region pred
                            //New pred found: collect it to the actual Turtle token.
                            if (!actualPred.Equals(triple.Predicate.ToString(), StringComparison.Ordinal))
                            {
                                if (!subjPrint)
                                {
                                    result.Append(spaceConst.PadRight(abbreviatedSubj.Length + 1)); //pretty-printing spaces to align the predList
                                }
                                actualPred = triple.Predicate.ToString();
                                abbreviatedPred = RDFQueryPrinter.PrintPatternMember(RDFQueryUtilities.ParseRDFPatternMember(actualPred), prefixes);

                                //Turtle goody for "rdf:type" shortcutting to "a"
                                if (abbreviatedPred == string.Concat(RDFVocabulary.RDF.PREFIX, ":type"))
                                    abbreviatedPred = "a";

                                result.Append(string.Concat(abbreviatedPred, " "));
                            }
                            #endregion

                            #region object
                            //Collect the object or the literal to the Turtle token
                            if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            {
                                string obj = triple.Object.ToString();
                                if (!obj.StartsWith("_:"))
                                {
                                    result.Append(RDFQueryPrinter.PrintPatternMember(RDFQueryUtilities.ParseRDFPatternMember(obj), prefixes));
                                }
                                else
                                {
                                    result.Append(obj);
                                }
                            }
                            #endregion

                            #region literal
                            else
                            {

                                //Detect presence of long-literals
                                string litValDelim = "\"";
                                if (regexTTL.Match(triple.Object.ToString()).Success)
                                    litValDelim = "\"\"\"";

                                if (triple.Object is RDFTypedLiteral)
                                {
                                    string dtype = RDFQueryPrinter.PrintPatternMember(
                                                    RDFQueryUtilities.ParseRDFPatternMember(
                                                     RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)triple.Object).Datatype)), prefixes);
                                    string tLit = string.Concat(litValDelim, ((RDFTypedLiteral)triple.Object).Value.Replace("\\", "\\\\"), litValDelim, "^^", dtype);
                                    result.Append(tLit);
                                }
                                else
                                {
                                    string pLit = string.Concat(litValDelim, ((RDFPlainLiteral)triple.Object).Value.Replace("\\", "\\\\"), litValDelim);
                                    if (((RDFPlainLiteral)triple.Object).Language != string.Empty)
                                        pLit = string.Concat(pLit, "@", ((RDFPlainLiteral)triple.Object).Language);
                                    result.Append(pLit);
                                }

                            }
                            #endregion

                            #region continuation goody
                            //Then append the appropriated Turtle continuation goody ("," or ";")
                            if (!triple.Equals(groupLast))
                                result.Append(", ");
                            else
                                result.AppendLine("; ");
                            #endregion

                        }
                        #endregion

                        #region last group
                        //This is only for the last group, which is not written into the cycle as the others
                        if (group.Key.Equals(groupedListLast.Key))
                        {
                            result.Replace(";", ".", result.Length - 4, 1);
                            sw.Write(result.ToString());
                        }
                        #endregion

                    }
                    #endregion

                }
                #endregion

            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot serialize Turtle because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given Turtle filepath to a graph.
        /// </summary>
        internal static RDFGraph Deserialize(string filepath) => Deserialize(new FileStream(filepath, FileMode.Open), null);

        /// <summary>
        /// Deserializes the given Turtle stream to a graph.
        /// (This algorythm is based on Sesame Turtle parser)
        /// </summary>
        internal static RDFGraph Deserialize(Stream inputStream, Uri graphContext)
        {
            try
            {

                #region deserialize
                RDFGraph result = new RDFGraph().SetContext(graphContext);

                //Initialize Turtle context
                var turtleContext = new Dictionary<string, object>() {
                    { "SUBJECT",    null },
                    { "PREDICATE",  null },
                    { "OBJECT",     null },
                    { "POSITION",   0    }
                };

                //Fetch Turtle data
                var turtleData = string.Empty;
                using (var sReader = new StreamReader(inputStream, Encoding.UTF8))
                {
                    turtleData = sReader.ReadToEnd();
                }

                //Parse Turtle data
                int bufferChar = SkipWhitespace(turtleData, turtleContext, result);
                while (bufferChar != -1)
                {
                    ParseStatement(turtleData, turtleContext, result);
                    if ((int)turtleContext["POSITION"] < turtleData.Length)
                        bufferChar = SkipWhitespace(turtleData, turtleContext, result);
                    else
                        bufferChar = -1;
                }
                RDFNamespaceRegister.RemoveTemporaryNamespaces();

                return result;
                #endregion

            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot deserialize Turtle because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Utilities

        #region Parse.CodePoint
        /// <summary>
        /// Peeks at the next Unicode code point without advancing the reader
        /// </summary>
        private static int PeekCodePoint(string turtleData, Dictionary<string, object> turtleContext)
        {
            int codePoint = ReadCodePoint(turtleData, turtleContext);
            UnreadCodePoint(turtleData, turtleContext, codePoint);
            return codePoint;
        }

        /// <summary>
        /// Reads the next Unicode code point from the reader
        /// </summary>
        private static int ReadCodePoint(string turtleData, Dictionary<string, object> turtleContext)
        {
            if ((int)turtleContext["POSITION"] < turtleData.Length)
            {
                int highSurrogate = turtleData[(int)turtleContext["POSITION"]];
                UpdateTurtleContextPosition(turtleContext, 1);
                if (char.IsHighSurrogate((char)highSurrogate))
                {
                    if ((int)turtleContext["POSITION"] < turtleData.Length)
                    {
                        int lowSurrogate = turtleData[(int)turtleContext["POSITION"]];
                        UpdateTurtleContextPosition(turtleContext, 1);
                        if (char.IsLowSurrogate((char)lowSurrogate))
                        {
                            highSurrogate = char.ConvertToUtf32((char)highSurrogate, (char)lowSurrogate);
                        }
                    }
                }
                return highSurrogate;
            }
            else
            {
                return -1; //EOF
            }
        }

        /// <summary>
        /// Unreads the given Unicode code point from the reader
        /// </summary>
        private static void UnreadCodePoint(string turtleData, Dictionary<string, object> turtleContext, int codePoint)
        {
            if (codePoint != -1)
            {
                if (IsSupplementaryCodePoint(codePoint))
                {
                    string surrogatePair = char.ConvertFromUtf32(codePoint);
                    UpdateTurtleContextPosition(turtleContext, -surrogatePair.Length);
                }
                else
                {
                    UpdateTurtleContextPosition(turtleContext, -1);
                }
                SafetyCheckTurtleContextPosition(turtleContext);
            }
        }

        /// <summary>
        /// Unreads the given Unicode code point from the reader
        /// </summary>
        private static void UnreadCodePoint(string turtleData, Dictionary<string, object> turtleContext, string codePoints)
        {
            if (!string.IsNullOrEmpty(codePoints))
            {
                foreach (var cp in codePoints)
                {
                    UnreadCodePoint(turtleData, turtleContext, cp);
                }
            }
        }
        #endregion

        #region Parse.TurtleContext
        /// <summary>
        /// Gets the actual coordinates within Turtle context
        /// </summary>
        private static string GetTurtleContextCoordinates(Dictionary<string, object> turtleContext)
            => string.Concat("[POSITION:", turtleContext["POSITION"], "]");
        
        /// <summary>
        /// Updates the position of the cursor within Turtle context
        /// </summary>
        private static void UpdateTurtleContextPosition(Dictionary<string, object> turtleContext, int move)
        => turtleContext["POSITION"] = (int)turtleContext["POSITION"] + move;

        /// <summary>
        /// Safety checks the position of the cursor within Turtle context
        /// </summary>
        private static void SafetyCheckTurtleContextPosition(Dictionary<string, object> turtleContext)
        {
            if ((int)turtleContext["POSITION"] < 0)
                turtleContext["POSITION"] = 0;
        }
        #endregion

        #region Parse.Grammar
        /// <summary>
        /// Parses the Turtle data in order to detect a valid directive or statement
        /// </summary>
        private static void ParseStatement(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            StringBuilder sb = new StringBuilder(8);
            int codePoint;

            // longest valid directive @prefix
            do
            {
                codePoint = ReadCodePoint(turtleData, turtleContext);
                if (codePoint == -1 || IsWhitespace(codePoint))
                {
                    UnreadCodePoint(turtleData, turtleContext, codePoint);
                    break;
                }
                sb.Append(char.ConvertFromUtf32(codePoint));
            } while (sb.Length < 8);

            string directive = sb.ToString();
            if (directive.StartsWith("@")
                || directive.Equals("prefix", StringComparison.InvariantCultureIgnoreCase)
                || directive.Equals("base", StringComparison.InvariantCultureIgnoreCase))
            {
                ParseDirective(turtleData, turtleContext, result, directive);
                SkipWhitespace(turtleData, turtleContext, result);
                // Turtle @base and @prefix directives MUST end with "."
                if (directive.StartsWith("@"))
                {
                    VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), ".");
                }
                // SPARQL BASE and PREFIX directives MUST NOT end with "."
                else
                {
                    if (PeekCodePoint(turtleData, turtleContext) == '.')
                    {
                        throw new RDFModelException("SPARQL directive '" + directive + "' must not end with '.'" + GetTurtleContextCoordinates(turtleContext));
                    }
                }
            }
            else
            {
                UnreadCodePoint(turtleData, turtleContext, directive);
                ParseTriples(turtleData, turtleContext, result);
                SkipWhitespace(turtleData, turtleContext, result);
                VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), ".");
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid directive
        /// </summary>
        private static void ParseDirective(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result, string directive)
        {
            if (directive.Length >= 7 && directive.Substring(0, 7).Equals("@prefix", StringComparison.Ordinal))
            {
                if (directive.Length > 7)
                {
                    UnreadCodePoint(turtleData, turtleContext, directive.Substring(7));
                }
                ParsePrefixID(turtleData, turtleContext, result);
            }
            else if (directive.Length >= 5 && directive.Substring(0, 5).Equals("@base", StringComparison.Ordinal))
            {
                if (directive.Length > 5)
                {
                    UnreadCodePoint(turtleData, turtleContext, directive.Substring(5));
                }
                ParseBase(turtleData, turtleContext, result);
            }
            else if (directive.Length >= 6 && directive.Substring(0, 6).Equals("PREFIX", StringComparison.Ordinal))
            {
                if (directive.Length > 6)
                {
                    UnreadCodePoint(turtleData, turtleContext, directive.Substring(6));
                }
                ParsePrefixID(turtleData, turtleContext, result);
            }
            else if (directive.Length >= 4 && directive.Substring(0, 4).Equals("BASE", StringComparison.Ordinal))
            {
                if (directive.Length > 4)
                {
                    UnreadCodePoint(turtleData, turtleContext, directive.Substring(4));
                }
                ParseBase(turtleData, turtleContext, result);
            }
            else if (directive.Length == 0)
            {
                throw new RDFModelException("Directive name is missing, expected @prefix or @base" + GetTurtleContextCoordinates(turtleContext));
            }
            else
            {
                throw new RDFModelException("Found unknown directive \"" + directive + "\"" + GetTurtleContextCoordinates(turtleContext));
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement
        /// </summary>
        private static void ParseTriples(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            int bufChar = PeekCodePoint(turtleData, turtleContext);

            // If the first character is an open bracket we need to decide which of
            // the two parsing methods for blank nodes to use
            if (bufChar == '[')
            {
                bufChar = ReadCodePoint(turtleData, turtleContext);
                SkipWhitespace(turtleData, turtleContext, result);
                bufChar = PeekCodePoint(turtleData, turtleContext);
                if (bufChar == ']')
                {
                    bufChar = ReadCodePoint(turtleData, turtleContext);
                    turtleContext["SUBJECT"] = new RDFResource();
                    SkipWhitespace(turtleData, turtleContext, result);
                    ParsePredicateObjectList(turtleData, turtleContext, result);
                }
                else
                {
                    //We have to parse an implicit blank, so we must rewind to the
                    //initial '[' character in order for the method to work
                    while (bufChar != '[')
                    {
                        UnreadCodePoint(turtleData, turtleContext, bufChar);
                        bufChar = PeekCodePoint(turtleData, turtleContext);
                    }
                    turtleContext["SUBJECT"] = ParseImplicitBlank(turtleData, turtleContext, result);
                }
                SkipWhitespace(turtleData, turtleContext, result);
                bufChar = PeekCodePoint(turtleData, turtleContext);

                // if this is not the end of the statement, recurse into the list of
                // predicate and objects, using the subject parsed above as the subject
                // of the statement.
                if (bufChar != '.')
                {
                    ParsePredicateObjectList(turtleData, turtleContext, result);
                }
            }
            else
            {
                ParseSubject(turtleData, turtleContext, result);
                SkipWhitespace(turtleData, turtleContext, result);
                ParsePredicateObjectList(turtleData, turtleContext, result);
            }

            turtleContext["SUBJECT"] = null;
            turtleContext["PREDICATE"] = null;
            turtleContext["OBJECT"] = null;
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement subject
        /// </summary>
        private static void ParseSubject(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            int bufChar = PeekCodePoint(turtleData, turtleContext);
            if (bufChar == '(')
            {
                turtleContext["SUBJECT"] = ParseCollection(turtleData, turtleContext, result);
            }
            else if (bufChar == '[')
            {
                turtleContext["SUBJECT"] = ParseImplicitBlank(turtleData, turtleContext, result);
            }
            else
            {
                object value = ParseValue(turtleData, turtleContext, result);
                if (value is Uri)
                {
                    turtleContext["SUBJECT"] = new RDFResource(value.ToString());
                }
                else if (value is RDFResource)
                {
                    turtleContext["SUBJECT"] = value;
                }
                else if (value != null)
                {
                    throw new RDFModelException("Illegal subject value: " + value + GetTurtleContextCoordinates(turtleContext));
                }
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement predicate
        /// </summary>
        private static RDFResource ParsePredicate(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            // Check if the short-cut 'a' is used
            int bufChar1 = ReadCodePoint(turtleData, turtleContext);
            if (bufChar1 == 'a')
            {
                int bufChar2 = ReadCodePoint(turtleData, turtleContext);

                if (IsWhitespace(bufChar2))
                {
                    // Short-cut is used, return the rdf:type URI
                    return RDFVocabulary.RDF.TYPE;
                }

                // Short-cut is not used, unread all characters
                UnreadCodePoint(turtleData, turtleContext, bufChar2);
            }
            UnreadCodePoint(turtleData, turtleContext, bufChar1);

            // Predicate is a normal resource
            object predicate = ParseValue(turtleData, turtleContext, result);
            if (predicate is Uri)
            {
                return new RDFResource(predicate.ToString());
            }
            else if (predicate is RDFResource)
            {
                return (RDFResource)predicate;
            }
            else
            {
                throw new RDFModelException("Illegal predicate value: " + predicate + GetTurtleContextCoordinates(turtleContext));
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement predicate-object list
        /// </summary>
        private static void ParsePredicateObjectList(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            turtleContext["PREDICATE"] = ParsePredicate(turtleData, turtleContext, result);

            SkipWhitespace(turtleData, turtleContext, result);

            ParseObjectList(turtleData, turtleContext, result);

            while (SkipWhitespace(turtleData, turtleContext, result) == ';')
            {
                ReadCodePoint(turtleData, turtleContext);

                int bufChar = SkipWhitespace(turtleData, turtleContext, result);
                if (bufChar == '.' || bufChar == ']' || bufChar == '}')
                {
                    break;
                }
                else if (bufChar == ';')
                {
                    // empty predicateObjectList, skip to next
                    continue;
                }

                turtleContext["PREDICATE"] = ParsePredicate(turtleData, turtleContext, result);

                SkipWhitespace(turtleData, turtleContext, result);

                ParseObjectList(turtleData, turtleContext, result);
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement object list
        /// </summary>
        private static void ParseObjectList(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            ParseObject(turtleData, turtleContext, result);

            while (SkipWhitespace(turtleData, turtleContext, result) == ',')
            {
                ReadCodePoint(turtleData, turtleContext);
                SkipWhitespace(turtleData, turtleContext, result);
                ParseObject(turtleData, turtleContext, result);
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement object
        /// </summary>
        private static void ParseObject(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            int bufChar = PeekCodePoint(turtleData, turtleContext);
            switch (bufChar)
            {
                case '(':
                    turtleContext["OBJECT"] = ParseCollection(turtleData, turtleContext, result);
                    break;
                case '[':
                    turtleContext["OBJECT"] = ParseImplicitBlank(turtleData, turtleContext, result);
                    break;
                default:
                    turtleContext["OBJECT"] = ParseValue(turtleData, turtleContext, result);
                    break;
            }

            //If object in the context is a Uri, make it a resource for compatibility
            if (turtleContext["OBJECT"] is Uri)
                turtleContext["OBJECT"] = new RDFResource(turtleContext["OBJECT"].ToString());

            //report statement
            if (turtleContext["OBJECT"] is RDFLiteral)
                result.AddTriple(new RDFTriple((RDFResource)turtleContext["SUBJECT"],
                                               (RDFResource)turtleContext["PREDICATE"],
                                               (RDFLiteral)turtleContext["OBJECT"]));
            else
                result.AddTriple(new RDFTriple((RDFResource)turtleContext["SUBJECT"],
                                               (RDFResource)turtleContext["PREDICATE"],
                                               (RDFResource)turtleContext["OBJECT"]));
        }

        /// <summary>
        /// Parses a collection, e.g. ( item1 item2 item3 )
        /// </summary>
        private static RDFResource ParseCollection(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), "(");

            int bufChar = SkipWhitespace(turtleData, turtleContext, result);
            if (bufChar == ')')
            {
                // Empty list (rdf:Nil)
                ReadCodePoint(turtleData, turtleContext);
                return RDFVocabulary.RDF.NIL;
            }
            else
            {

                //report statement
                RDFResource listRoot = new RDFResource();
                result.AddTriple(new RDFTriple(listRoot, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));

                // Remember current subject and predicate
                RDFResource oldSubject = (RDFResource)turtleContext["SUBJECT"];
                RDFResource oldPredicate = (RDFResource)turtleContext["PREDICATE"];

                // generated bNode becomes subject, predicate becomes rdf:first
                turtleContext["SUBJECT"] = listRoot;
                turtleContext["PREDICATE"] = RDFVocabulary.RDF.FIRST;

                ParseObject(turtleData, turtleContext, result);

                RDFResource bNode = listRoot;
                while (SkipWhitespace(turtleData, turtleContext, result) != ')')
                {
                    // Create another list node and link it to the previous
                    RDFResource newNode = new RDFResource();

                    //report statement
                    result.AddTriple(new RDFTriple(bNode, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));
                    result.AddTriple(new RDFTriple(bNode, RDFVocabulary.RDF.REST, newNode));

                    // New node becomes the current
                    turtleContext["SUBJECT"] = bNode = newNode;

                    ParseObject(turtleData, turtleContext, result);
                }

                // Skip ')'
                ReadCodePoint(turtleData, turtleContext);

                // Close the list
                result.AddTriple(new RDFTriple(bNode, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));
                result.AddTriple(new RDFTriple(bNode, RDFVocabulary.RDF.REST, RDFVocabulary.RDF.NIL));

                // Restore previous subject and predicate
                turtleContext["SUBJECT"] = oldSubject;
                turtleContext["PREDICATE"] = oldPredicate;

                return listRoot;
            }
        }

        /// <summary>
        /// Parses an implicit blank node. This method parses the token []
        /// and predicateObjectLists that are surrounded by square brackets.
        /// </summary>
        private static RDFResource ParseImplicitBlank(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), "[");
            RDFResource bNode = new RDFResource(); // createBNode()

            SkipWhitespace(turtleData, turtleContext, result);
            int bufChar = ReadCodePoint(turtleData, turtleContext);
            if (bufChar != ']')
            {
                UnreadCodePoint(turtleData, turtleContext, bufChar);

                // Remember current subject and predicate
                RDFResource oldSubject = (RDFResource)turtleContext["SUBJECT"];
                RDFResource oldPredicate = (RDFResource)turtleContext["PREDICATE"];

                // generated bNode becomes subject
                turtleContext["SUBJECT"] = bNode;

                // Enter recursion with nested predicate-object list
                SkipWhitespace(turtleData, turtleContext, result);
                ParsePredicateObjectList(turtleData, turtleContext, result);
                SkipWhitespace(turtleData, turtleContext, result);

                // Read closing bracket
                VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), "]");

                // Restore previous subject and predicate
                turtleContext["SUBJECT"] = oldSubject;
                turtleContext["PREDICATE"] = oldPredicate;
            }

            return bNode;
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid namespace prefix
        /// </summary>
        private static void ParsePrefixID(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            SkipWhitespace(turtleData, turtleContext, result);

            // Read prefix ID (e.g. "rdf:" or ":")
            StringBuilder prefixID = new StringBuilder();
            while (true)
            {
                int bufChar = ReadCodePoint(turtleData, turtleContext);
                if (bufChar == ':')
                {
                    UnreadCodePoint(turtleData, turtleContext, bufChar);
                    break;
                }
                else if (IsWhitespace(bufChar))
                {
                    break;
                }
                else if (bufChar == -1)
                {
                    throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
                }
                prefixID.Append(char.ConvertFromUtf32(bufChar));
            }

            SkipWhitespace(turtleData, turtleContext, result);
            VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), ":");
            SkipWhitespace(turtleData, turtleContext, result);

            // Read the namespace URI
            Uri nspace = ParseURI(turtleData, turtleContext, result);

            // Store and report this namespace mapping
            string prefixStr = prefixID.ToString();
            string namespaceStr = nspace.ToString();
            // If prefix is empty it must be considered default context of the graph
            if (string.IsNullOrEmpty(prefixStr))
            {
                prefixStr = string.Format("AutoNS{0}{1}", DateTime.UtcNow.Minute, DateTime.UtcNow.Second);
                result.SetContext(new Uri(namespaceStr));
            }

            //Support eventual redefinement of temporary namespaces
            var registerNSpace = RDFNamespaceRegister.GetByPrefix(prefixStr);
            if (registerNSpace == null)
            {
                RDFNamespaceRegister.AddNamespace(new RDFNamespace(prefixStr, namespaceStr).SetTemporary(true));
            }
            else
            {
                if (registerNSpace.IsTemporary)
                {
                    RDFNamespaceRegister.RemoveByPrefix(prefixStr);
                    RDFNamespaceRegister.AddNamespace(new RDFNamespace(prefixStr, namespaceStr).SetTemporary(true));
                }
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid base directive
        /// </summary>
        private static void ParseBase(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            SkipWhitespace(turtleData, turtleContext, result);
            Uri baseURI = ParseURI(turtleData, turtleContext, result);
            result.SetContext(baseURI);
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid Uri
        /// </summary>
        private static Uri ParseURI(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            StringBuilder uriBuf = new StringBuilder();

            // First character should be '<'
            int bufChar = ReadCodePoint(turtleData, turtleContext);
            VerifyCharacterOrFail(turtleData, turtleContext, bufChar, "<");

            // Read up to the next '>' character
            while (true)
            {
                bufChar = ReadCodePoint(turtleData, turtleContext);
                if (bufChar == '>')
                {
                    break;
                }
                else if (bufChar == -1)
                {
                    throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
                }
                else if (bufChar == ' ')
                {
                    throw new RDFModelException("Uri included an unencoded space: '" + bufChar + "'" + GetTurtleContextCoordinates(turtleContext));
                }

                uriBuf.Append(char.ConvertFromUtf32(bufChar));

                if (bufChar == '\\')
                {
                    // This escapes the next character, which might be a '>'
                    bufChar = ReadCodePoint(turtleData, turtleContext);
                    if (bufChar == -1)
                    {
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
                    }
                    if (bufChar != 'u' && bufChar != 'U')
                    {
                        throw new RDFModelException("Uri includes string escapes: '\\" + bufChar + "'" + GetTurtleContextCoordinates(turtleContext));
                    }
                    uriBuf.Append(char.ConvertFromUtf32(bufChar));
                }
            }

            if (bufChar == '.')
            {
                throw new RDFModelException("Uri must not end with '.'" + GetTurtleContextCoordinates(turtleContext));
            }

            var uriString = DecodeString(turtleData, turtleContext, uriBuf.ToString());
            //Absolute: use as found
            if (Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
                return new Uri(uriString);
            //Relative: append to graph context
            else if (Uri.IsWellFormedUriString(uriString, UriKind.Relative))
                return new Uri(string.Concat(result.ToString(), uriString));
            //PureFragment: append to graph context
            else if (uriString.Equals("#"))
                return new Uri(string.Concat(result.ToString().TrimEnd(new char[] { '#' }), uriString));
            //Error: not well-formed, so throw exception
            else
                throw new RDFModelException("Uri is not well-formed" + GetTurtleContextCoordinates(turtleContext));
        }

        /// <summary>
        /// Parses an RDF value. This method parses uriref, qname, node ID, quoted
	    /// literal, integer, double and boolean.
        /// </summary>
        private static object ParseValue(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            int bufChar = PeekCodePoint(turtleData, turtleContext);
            if (bufChar == '<')
            {
                // uriref, e.g. <foo://bar>
                return ParseURI(turtleData, turtleContext, result);
            }
            else if (bufChar == ':' || IsPrefixStartChar(bufChar))
            {
                // qname or boolean
                return ParseQNameOrBoolean(turtleData, turtleContext, result);
            }
            else if (bufChar == '_')
            {
                // node ID, e.g. _:n1
                return ParseNodeID(turtleData, turtleContext, result);
            }
            else if (bufChar == '"' || bufChar == '\'')
            {
                // quoted literal, e.g. "foo" or """foo""" or 'foo' or '''foo'''
                return ParseQuotedLiteral(turtleData, turtleContext, result);
            }
            else if (IsNumber(bufChar) || bufChar == '.' || bufChar == '+' || bufChar == '-')
            {
                // integer or double, e.g. 123 or 1.2e3
                return ParseNumber(turtleData, turtleContext, result);
            }
            else if (bufChar == -1)
            {
                throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
            }
            else
            {
                throw new RDFModelException("Expected an RDF value here, found '" + char.ConvertFromUtf32(bufChar) + "'" + GetTurtleContextCoordinates(turtleContext));
            }
        }

        /// <summary>
        /// Parses a blank node ID, e.g. _:node1
        /// </summary>
        private static RDFResource ParseNodeID(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            // Node ID should start with "_:"
            VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), "_");
            VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), ":");

            // Read the node ID
            int bufChar = ReadCodePoint(turtleData, turtleContext);
            if (bufChar == -1)
            {
                throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
            }
            else if (!IsBLANK_NODE_LABEL_StartChar(bufChar))
            {
                throw new RDFModelException("Expected a letter, found '" + (char)bufChar + "'" + GetTurtleContextCoordinates(turtleContext));
            }

            StringBuilder name = new StringBuilder(32);
            name.Append(char.ConvertFromUtf32(bufChar));

            // Read all following letter and numbers, they are part of the name
            bufChar = ReadCodePoint(turtleData, turtleContext);

            // If we would never go into the loop we must unread now
            if (!IsBLANK_NODE_LABEL_Char(bufChar))
            {
                UnreadCodePoint(turtleData, turtleContext, bufChar);
            }

            while (IsBLANK_NODE_LABEL_Char(bufChar))
            {
                int previous = bufChar;
                bufChar = ReadCodePoint(turtleData, turtleContext);
                if (previous == '.' && (bufChar == -1 || IsWhitespace(bufChar) || bufChar == '<' || bufChar == '_'))
                {
                    UnreadCodePoint(turtleData, turtleContext, bufChar);
                    UnreadCodePoint(turtleData, turtleContext, previous);
                    break;
                }
                name.Append((char)previous);
                if (!IsBLANK_NODE_LABEL_Char(bufChar))
                {
                    UnreadCodePoint(turtleData, turtleContext, bufChar);
                }
            }

            return new RDFResource("bnode:" + name.ToString()); //return createBNode(name.toString());
        }

        /// <summary>
        /// Parses a number
        /// </summary>
        private static RDFTypedLiteral ParseNumber(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            StringBuilder value = new StringBuilder();
            RDFModelEnums.RDFDatatypes dt = RDFModelEnums.RDFDatatypes.XSD_INTEGER;

            int bufChar = ReadCodePoint(turtleData, turtleContext);

            // read optional sign character
            if (bufChar == '+' || bufChar == '-')
            {
                value.Append(char.ConvertFromUtf32(bufChar));
                bufChar = ReadCodePoint(turtleData, turtleContext);
            }

            while (IsNumber(bufChar))
            {
                value.Append(char.ConvertFromUtf32(bufChar));
                bufChar = ReadCodePoint(turtleData, turtleContext);
            }

            if (bufChar == '.' || bufChar == 'e' || bufChar == 'E')
            {

                // read optional fractional digits
                if (bufChar == '.')
                {
                    if (IsWhitespace(PeekCodePoint(turtleData, turtleContext)))
                    {
                        // We're parsing an integer that did not have a space before the
                        // period to end the statement
                    }
                    else
                    {
                        value.Append(char.ConvertFromUtf32(bufChar));

                        bufChar = ReadCodePoint(turtleData, turtleContext);

                        while (IsNumber(bufChar))
                        {
                            value.Append(char.ConvertFromUtf32(bufChar));
                            bufChar = ReadCodePoint(turtleData, turtleContext);
                        }

                        if (value.Length == 1)
                        {
                            // We've only parsed a '.'
                            throw new RDFModelException("Object for statement missing" + GetTurtleContextCoordinates(turtleContext));
                        }

                        // We're parsing a decimal or a double
                        dt = RDFModelEnums.RDFDatatypes.XSD_DECIMAL;
                    }
                }
                else
                {
                    if (value.Length == 0)
                    {
                        // We've only parsed a '.'
                        throw new RDFModelException("Object for statement missing" + GetTurtleContextCoordinates(turtleContext));
                    }
                }

                // read optional exponent
                if (bufChar == 'e' || bufChar == 'E')
                {
                    dt = RDFModelEnums.RDFDatatypes.XSD_DOUBLE;
                    value.Append(char.ConvertFromUtf32(bufChar));

                    bufChar = ReadCodePoint(turtleData, turtleContext);
                    if (bufChar == '+' || bufChar == '-')
                    {
                        value.Append(char.ConvertFromUtf32(bufChar));
                        bufChar = ReadCodePoint(turtleData, turtleContext);
                    }

                    if (!IsNumber(bufChar))
                    {
                        throw new RDFModelException("Exponent value missing" + GetTurtleContextCoordinates(turtleContext));
                    }

                    value.Append(char.ConvertFromUtf32(bufChar));

                    bufChar = ReadCodePoint(turtleData, turtleContext);
                    while (IsNumber(bufChar))
                    {
                        value.Append(char.ConvertFromUtf32(bufChar));
                        bufChar = ReadCodePoint(turtleData, turtleContext);
                    }
                }
            }

            // Unread last character, it isn't part of the number
            UnreadCodePoint(turtleData, turtleContext, bufChar);

            // Return result as a typed literal
            return new RDFTypedLiteral(value.ToString(), dt);
        }

        /// <summary>
        /// Parses qnames and boolean values, which have equivalent starting characters
        /// </summary>
        private static object ParseQNameOrBoolean(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            // First character should be a ':' or a letter
            int bufChar = ReadCodePoint(turtleData, turtleContext);
            if (bufChar == -1)
            {
                throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
            }
            if (bufChar != ':' && !IsPrefixStartChar(bufChar))
            {
                throw new RDFModelException("Expected a ':' or a letter, found '" + char.ConvertFromUtf32(bufChar) + "'" + GetTurtleContextCoordinates(turtleContext));
            }

            int previousChar;
            string nspace = null;
            if (bufChar == ':')
            {
                // qname using default namespace
                nspace = result.Context.ToString();
            }
            else
            {
                // bufChar is the first letter of the prefix
                StringBuilder prefix = new StringBuilder();
                prefix.Append(char.ConvertFromUtf32(bufChar));

                previousChar = bufChar;
                bufChar = ReadCodePoint(turtleData, turtleContext);
                while (IsPrefixChar(bufChar))
                {
                    prefix.Append(char.ConvertFromUtf32(bufChar));
                    previousChar = bufChar;
                    bufChar = ReadCodePoint(turtleData, turtleContext);
                }

                while (previousChar == '.' && prefix.Length > 0)
                {
                    // '.' is a legal prefix name char, but can not appear at the end
                    UnreadCodePoint(turtleData, turtleContext, bufChar);
                    bufChar = previousChar;
                    prefix.Remove(prefix.Length - 1, 1);
                    previousChar = prefix.ToString().Last();
                }

                if (bufChar != ':')
                {
                    // prefix may actually be a boolean value
                    string value = prefix.ToString();
                    if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase)
                            || value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                    {
                        UnreadCodePoint(turtleData, turtleContext, bufChar);
                        return new RDFTypedLiteral(value, RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);
                    }
                }
                else
                {
                    if (previousChar == '.')
                    {
                        // '.' is a legal prefix name char, but can not appear at the end
                        throw new RDFModelException("prefix can not end with with '.'" + GetTurtleContextCoordinates(turtleContext));
                    }
                }

                VerifyCharacterOrFail(turtleData, turtleContext, bufChar, ":");

                nspace = RDFNamespaceRegister.GetByPrefix(prefix.ToString())?.ToString();
            }

            // bufChar == ':', read optional local name
            StringBuilder localName = new StringBuilder();
            bufChar = ReadCodePoint(turtleData, turtleContext);
            if (IsNameStartChar(bufChar))
            {
                if (bufChar == '\\')
                {
                    localName.Append(ReadLocalEscapedChar(turtleData, turtleContext));
                }
                else
                {
                    localName.Append(char.ConvertFromUtf32(bufChar));
                }

                previousChar = bufChar;
                bufChar = ReadCodePoint(turtleData, turtleContext);
                while (IsNameChar(bufChar))
                {
                    if (bufChar == '\\')
                    {
                        localName.Append(ReadLocalEscapedChar(turtleData, turtleContext));
                    }
                    else
                    {
                        localName.Append(char.ConvertFromUtf32(bufChar));
                    }
                    previousChar = bufChar;
                    bufChar = ReadCodePoint(turtleData, turtleContext);
                }

                // Unread last character
                UnreadCodePoint(turtleData, turtleContext, bufChar);

                if (previousChar == '.')
                {
                    // '.' is a legal name char, but can not appear at the end, so is not actually part of the name
                    UnreadCodePoint(turtleData, turtleContext, previousChar);
                    localName.Remove(localName.Length - 1, 1);
                }
            }
            else
            {
                // Unread last character
                UnreadCodePoint(turtleData, turtleContext, bufChar);
            }

            string localNameString = localName.ToString();
            for (int i = 0; i < localNameString.Length; i++)
            {
                if (localNameString[i] == '%')
                {
                    if (i > localNameString.Length - 3
                            || !Uri.IsHexDigit(localNameString[i + 1])
                            || !Uri.IsHexDigit(localNameString[i + 2]))
                    {
                        throw new RDFModelException("Found incomplete percent-encoded sequence: " + localNameString + GetTurtleContextCoordinates(turtleContext));
                    }
                }
            }

            // Note: namespace has already been resolved
            return new Uri(string.Concat(nspace ?? result.Context.ToString(), localNameString));
        }

        /// <summary>
        /// Parses a quoted string, optionally followed by a language tag or datatype.
        /// </summary>
        private static RDFLiteral ParseQuotedLiteral(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            string label = ParseQuotedString(turtleData, turtleContext);

            // Check for presence of a language tag or datatype
            int bufChar = PeekCodePoint(turtleData, turtleContext);
            if (bufChar == '@')
            {
                ReadCodePoint(turtleData, turtleContext);

                // Read language
                StringBuilder lang = new StringBuilder();

                bufChar = ReadCodePoint(turtleData, turtleContext);
                if (bufChar == -1)
                {
                    throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
                }
                if (!IsLanguageStartChar(bufChar))
                {
                    throw new RDFModelException("Expected a letter, found '" + char.ConvertFromUtf32(bufChar) + "'" + GetTurtleContextCoordinates(turtleContext));
                }

                lang.Append(char.ConvertFromUtf32(bufChar));

                bufChar = ReadCodePoint(turtleData, turtleContext);
                while (!IsWhitespace(bufChar))
                {
                    if (bufChar == '.'
                        || bufChar == ';'
                        || bufChar == ','
                        || bufChar == ')'
                        || bufChar == ']'
                        || bufChar == -1)
                    {
                        break;
                    }
                    if (!IsLanguageChar(bufChar))
                    {
                        throw new RDFModelException("Illegal language tag char: '" + char.ConvertFromUtf32(bufChar) + "'" + GetTurtleContextCoordinates(turtleContext));
                    }
                    lang.Append(char.ConvertFromUtf32(bufChar));
                    bufChar = ReadCodePoint(turtleData, turtleContext);
                }

                UnreadCodePoint(turtleData, turtleContext, bufChar);

                return new RDFPlainLiteral(label, lang.ToString());
            }
            else if (bufChar == '^')
            {
                ReadCodePoint(turtleData, turtleContext);

                // next character should be another '^'
                VerifyCharacterOrFail(turtleData, turtleContext, ReadCodePoint(turtleData, turtleContext), "^");

                SkipWhitespace(turtleData, turtleContext, result);

                // Read datatype
                var datatype = ParseValue(turtleData, turtleContext, result);
                if (datatype is Uri)
                {
                    return new RDFTypedLiteral(label, RDFModelUtilities.GetDatatypeFromString(datatype.ToString()));
                }
                else
                {
                    throw new RDFModelException("Illegal datatype value: " + datatype + GetTurtleContextCoordinates(turtleContext));
                }
            }
            else
            {
                return new RDFPlainLiteral(label);
            }
        }

        /// <summary>
        /// Parses a quoted string, which is either a "normal string" or a """long string"""
        /// </summary>
        private static string ParseQuotedString(string turtleData, Dictionary<string, object> turtleContext)
        {
            string result = null;

            int c1 = ReadCodePoint(turtleData, turtleContext);

            // First character should be '"' or "'"
            VerifyCharacterOrFail(turtleData, turtleContext, c1, "\"\'");

            // Check for long-string, which starts and ends with three double quotes
            int c2 = ReadCodePoint(turtleData, turtleContext);
            int c3 = ReadCodePoint(turtleData, turtleContext);

            if ((c1 == '"' && c2 == '"' && c3 == '"') || (c1 == '\'' && c2 == '\'' && c3 == '\''))
            {
                // Long string
                result = ParseLongString(turtleData, turtleContext, c2);
            }
            else
            {
                // Normal string
                UnreadCodePoint(turtleData, turtleContext, c3);
                UnreadCodePoint(turtleData, turtleContext, c2);

                result = ParseString(turtleData, turtleContext, c1);
            }

            // Unescape any escape sequences
            result = DecodeString(turtleData, turtleContext, result);

            return result;
        }

        /// <summary>
        /// Parses a "normal string". This method requires that the opening character has already been parsed.
        /// </summary>
        private static string ParseString(string turtleData, Dictionary<string, object> turtleContext, int closingCharacter)
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                int bufChar = ReadCodePoint(turtleData, turtleContext);
                if (bufChar == closingCharacter)
                {
                    break;
                }
                else if (bufChar == -1)
                {
                    throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
                }

                //Unquoted literals cannot contain carriage return
                if (bufChar == '\r' || bufChar == '\n')
                {
                    throw new RDFModelException("Illegal carriage return or new line in literal");
                }

                sb.Append(char.ConvertFromUtf32(bufChar));

                if (bufChar == '\\')
                {
                    // This escapes the next character, which might be a '"'
                    bufChar = ReadCodePoint(turtleData, turtleContext);
                    if (bufChar == -1)
                    {
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
                    }
                    sb.Append(char.ConvertFromUtf32(bufChar));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parses a """long string""". This method requires that the first three characters have already been parsed.
        /// </summary>
        private static string ParseLongString(string turtleData, Dictionary<string, object> turtleContext, int closingCharacter)
        {
            StringBuilder sb = new StringBuilder();

            int doubleQuoteCount = 0;
            int bufChar;

            while (doubleQuoteCount < 3)
            {
                bufChar = ReadCodePoint(turtleData, turtleContext);
                if (bufChar == -1)
                {
                    throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
                }
                else if (bufChar == closingCharacter)
                {
                    doubleQuoteCount++;
                }
                else
                {
                    doubleQuoteCount = 0;
                }

                sb.Append(char.ConvertFromUtf32(bufChar));

                if (bufChar == '\\')
                {
                    // This escapes the next character, which might be a '"'
                    bufChar = ReadCodePoint(turtleData, turtleContext);
                    if (bufChar == -1)
                    {
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));
                    }
                    sb.Append(char.ConvertFromUtf32(bufChar));
                }
            }

            return sb.ToString().Substring(0, sb.Length - 3);
        }

        /// <summary>
        /// Decodes an encoded Turtle string. Any \-escape sequences are substituted with their decoded value.
        /// </summary>
        private static string DecodeString(string turtleData, Dictionary<string, object> turtleContext, string s)
        {
            int backSlashIdx = s.IndexOf('\\');

            if (backSlashIdx == -1)
            {
                // No escaped characters found
                return s;
            }

            int startIdx = 0;
            int sLength = s.Length;
            StringBuilder sb = new StringBuilder(sLength);

            while (backSlashIdx != -1)
            {
                sb.Append(s.Substring(startIdx, backSlashIdx - startIdx));

                if (backSlashIdx + 1 >= sLength)
                {
                    throw new RDFModelException("Unescaped backslash in: " + s + GetTurtleContextCoordinates(turtleContext));
                }

                char bufChar = s[backSlashIdx + 1];
                if (bufChar == 't')
                {
                    sb.Append('\t');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == 'r')
                {
                    sb.Append('\r');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == 'n')
                {
                    sb.Append('\n');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == 'b')
                {
                    sb.Append('\b');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == 'f')
                {
                    sb.Append('\f');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == '"')
                {
                    sb.Append('"');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == '\'')
                {
                    sb.Append('\'');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == '>')
                {
                    sb.Append('>');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == '\\')
                {
                    sb.Append('\\');
                    startIdx = backSlashIdx + 2;
                }
                else if (bufChar == 'u')
                {
                    // \\uxxxx
                    if (backSlashIdx + 5 >= sLength)
                    {
                        throw new RDFModelException("Incomplete Unicode escape sequence in: " + s + GetTurtleContextCoordinates(turtleContext));
                    }

                    string uValue = s.Substring(backSlashIdx + 2, 4/*backSlashIdx + 6*/);
                    try
                    {
                        int cp = int.Parse(uValue, NumberStyles.AllowHexSpecifier);
                        sb.Append(char.ConvertFromUtf32(cp));
                        startIdx = backSlashIdx + 6;
                    }
                    catch
                    {
                        throw new RDFModelException("Illegal Unicode escape sequence '\\u" + uValue + "' in: " + s + GetTurtleContextCoordinates(turtleContext));
                    }
                }
                else if (bufChar == 'U')
                {
                    // \\Uxxxxxxxx
                    if (backSlashIdx + 9 >= sLength)
                    {
                        throw new RDFModelException("Incomplete Unicode escape sequence in: " + s + GetTurtleContextCoordinates(turtleContext));
                    }

                    string UValue = s.Substring(backSlashIdx + 2, 8/*backSlashIdx + 10*/);
                    try
                    {
                        int cp = int.Parse(UValue, NumberStyles.AllowHexSpecifier);
                        sb.Append(char.ConvertFromUtf32(cp));
                        startIdx = backSlashIdx + 10;
                    }
                    catch
                    {
                        throw new RDFModelException("Illegal Unicode escape sequence '\\U" + UValue + "' in: " + s + GetTurtleContextCoordinates(turtleContext));
                    }
                }
                else
                {
                    throw new RDFModelException("Unescaped backslash in: " + s + GetTurtleContextCoordinates(turtleContext));
                }

                backSlashIdx = s.IndexOf('\\', startIdx);
            }

            sb.Append(s.Substring(startIdx));

            return sb.ToString();
        }

        /// <summary>
        /// Consumes any whitespace characters (space, tab, line feed, newline) and comments(#-style) from the Turtle data.
        /// After this method has been called, the first character that is returned is either a non-ignorable character or EOF.
        /// </summary>
        private static int SkipWhitespace(string turtleData, Dictionary<string, object> turtleContext, RDFGraph result)
        {
            int bufChar = ReadCodePoint(turtleData, turtleContext);
            while (IsWhitespace(bufChar) || bufChar == '#')
            {
                if (bufChar == '#')
                {
                    SkipComment(turtleData, turtleContext);
                }
                bufChar = ReadCodePoint(turtleData, turtleContext);
            }
            UnreadCodePoint(turtleData, turtleContext, bufChar);
            return bufChar;
        }

        /// <summary>
        /// Consumes characters from reader until the first EOL has been read.
        /// </summary>
        private static void SkipComment(string turtleData, Dictionary<string, object> turtleContext)
        {
            int bufChar = ReadCodePoint(turtleData, turtleContext);
            while (bufChar != -1 && bufChar != 0xD && bufChar != 0xA)
            {
                bufChar = ReadCodePoint(turtleData, turtleContext);
            }

            // bufChar is equal to -1, \r or \n.
            // In case bufChar is equal to \r, we should also read a following \n.
            if (bufChar == 0xD)
            {
                bufChar = ReadCodePoint(turtleData, turtleContext);
                if (bufChar != 0xA)
                {
                    UnreadCodePoint(turtleData, turtleContext, bufChar);
                }
            }
        }

        /// <summary>
        /// Verifies that the supplied character code point is one of the expected chars.
        /// This method will throw an exception if this is not the case.
        /// </summary>
        private static void VerifyCharacterOrFail(string turtleData, Dictionary<string, object> turtleContext, int codePoint, string expected)
        {
            if (codePoint == -1)
                throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(turtleContext));

            string supplied = char.ConvertFromUtf32(codePoint);
            if (expected.IndexOf(supplied) == -1)
            {
                StringBuilder msg = new StringBuilder();
                msg.Append("Unexpected character found" + GetTurtleContextCoordinates(turtleContext) + ": expected ");

                for (int i = 0; i < expected.Length; i++)
                {
                    if (i > 0)
                        msg.Append(" or ");
                    msg.Append('\'');
                    msg.Append(expected[i]);
                    msg.Append('\'');
                }
                msg.Append(", found '");
                msg.Append(supplied);
                msg.Append("'");

                throw new RDFModelException(msg.ToString());
            }
        }

        private static char ReadLocalEscapedChar(string turtleData, Dictionary<string, object> turtleContext)
        {
            int bufChar = ReadCodePoint(turtleData, turtleContext);
            if (IsLocalEscapedChar(bufChar))
            {
                return (char)bufChar;
            }
            else
            {
                throw new RDFModelException("Found '" + char.ConvertFromUtf32(bufChar) + "', expected one of: _~.-!$&\'()*+,;=/?#@%" + GetTurtleContextCoordinates(turtleContext));
            }
        }
        #endregion

        #region Parse.Check
        /// <summary>
        /// Check if the supplied code point represents a whitespace character
        /// </summary>
        private static bool IsWhitespace(int codePoint)
        {
            // Whitespace character are space, tab, newline and carriage return:
            return codePoint == 0x20 || codePoint == 0x9 || codePoint == 0xA || codePoint == 0xD;
        }

        /// <summary>
        /// Check if the supplied code point represents a numeric character
        /// </summary>
        private static bool IsNumber(int codePoint)
        {
            return char.IsNumber((char)codePoint);
        }

        /// <summary>
        /// Determines whether the given scalar value is in the supplementary plane and thus
        /// requires 2 characters to be represented in UTF-16 (as a surrogate pair).
        /// </summary>
        private static bool IsSupplementaryCodePoint(int codePoint)
        {
            return (codePoint & ~((int)char.MaxValue)) != 0;
        }

        /// <summary>
        /// Check if the supplied code point represents a valid name start character
        /// </summary>
        private static bool IsNameStartChar(int codePoint)
        {
            return IsPN_CHARS_U(codePoint)
                    || codePoint == ':'
                    || IsNumber(codePoint)
                    || codePoint == '\\'
                    || codePoint == '%';
        }

        /// <summary>
        /// Check if the supplied code point represents a valid name character
        /// </summary>
        private static bool IsNameChar(int codePoint)
        {
            return IsPN_CHARS(codePoint)
                    || codePoint == '.'
                    || codePoint == ':'
                    || codePoint == '\\'
                    || codePoint == '%';
        }

        /// <summary>
        /// Check if the supplied code point represents a valid prefixed name start character
        /// </summary>
        private static bool IsPrefixStartChar(int codePoint)
        {
            return IsPN_CHARS_BASE(codePoint);
        }

        /// <summary>
        /// Check if the supplied code point represents a valid prefix character
        /// </summary>
        private static bool IsPrefixChar(int codePoint)
        {
            return IsPN_CHARS_BASE(codePoint)
                    || IsPN_CHARS(codePoint)
                    || codePoint == '.';
        }

        /// <summary>
        /// Check if the supplied code point represents a valid language tag start character
        /// </summary>
        private static bool IsLanguageStartChar(int codePoint)
        {
            return char.IsLetter((char)codePoint);
        }

        /// <summary>
        /// Check if the supplied code point represents a valid language tag character
        /// </summary>
        private static bool IsLanguageChar(int codePoint)
        {
            return char.IsLetter((char)codePoint)
                   || char.IsNumber((char)codePoint)
                   || codePoint == '-';
        }

        /// <summary>
        /// Check if the supplied code point represents a valid local escaped character.
        /// </summary>
        private static bool IsLocalEscapedChar(int codePoint)
        {
            return "_~.-!$&\'()*+,;=/?#@%".IndexOf((char)codePoint) > -1;
        }

        /// <summary>
        /// Check if the supplied code point represents a valid prefixed name start character
        /// </summary>
        private static bool IsBLANK_NODE_LABEL_StartChar(int codePoint)
        {
            return IsPN_CHARS_U(codePoint) || char.IsNumber((char)codePoint);
        }

        /// <summary>
        /// Check if the supplied code point represents a valid blank node label character
        /// </summary>
        private static bool IsBLANK_NODE_LABEL_Char(int codePoint)
        {
            return IsPN_CHARS(codePoint) || codePoint == '.';
        }

        /// <summary>
        /// Check if the supplied code point represents a valid prefixed name character
        /// </summary>
        private static bool IsPN_CHARS(int codePoint)
        {
            return IsPN_CHARS_U(codePoint)
                    || char.IsNumber((char)codePoint)
                    || codePoint == '-'
                    || codePoint == 0x00B7
                    || codePoint >= 0x0300 && codePoint <= 0x036F
                    || codePoint >= 0x203F && codePoint <= 0x2040;
        }

        /// <summary>
        /// Check if the supplied code point represents either a valid prefixed name base character or an underscore
        /// </summary>
        private static bool IsPN_CHARS_U(int codePoint)
        {
            return IsPN_CHARS_BASE(codePoint) || codePoint == '_';
        }

        /// <summary>
        /// Check if the supplied code point represents a valid prefixed name base character
        /// </summary>
        private static bool IsPN_CHARS_BASE(int codePoint)
        {
            return char.IsLetter((char)codePoint)
                || codePoint >= 0x00C0 && codePoint <= 0x00D6
                || codePoint >= 0x00D8 && codePoint <= 0x00F6
                || codePoint >= 0x00F8 && codePoint <= 0x02FF
                || codePoint >= 0x0370 && codePoint <= 0x037D
                || codePoint >= 0x037F && codePoint <= 0x1FFF
                || codePoint >= 0x200C && codePoint <= 0x200D
                || codePoint >= 0x2070 && codePoint <= 0x218F
                || codePoint >= 0x2C00 && codePoint <= 0x2FEF
                || codePoint >= 0x3001 && codePoint <= 0xD7FF
                || codePoint >= 0xF900 && codePoint <= 0xFDCF
                || codePoint >= 0xFDF0 && codePoint <= 0xFFFD
                || codePoint >= 0x10000 && codePoint <= 0xEFFFF;
        }
        #endregion

        #endregion

        #endregion

    }

}
