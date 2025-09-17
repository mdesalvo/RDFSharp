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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using RDFSharp.Query;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFTurtle is responsible for managing serialization to and from Turtle data format.
    /// </summary>
    internal static class RDFTurtle
    {
        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given graph to the given filepath using Turtle data format.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static void Serialize(RDFGraph graph, string filepath)
            => Serialize(graph, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given graph to the given stream using Turtle data format.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static void Serialize(RDFGraph graph, Stream outputStream)
        {
            try
            {
                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    //Collect namespaces
                    List<RDFNamespace> prefixes = RDFModelUtilities.GetGraphNamespaces(graph);

                    //Write namespaces
                    foreach (RDFNamespace ns in prefixes.OrderBy(n => n.NamespacePrefix))
                        sw.WriteLine($"@prefix {ns.NamespacePrefix}: <{ns.NamespaceUri}>.");
                    sw.WriteLine(string.Concat("@base <", graph.Context, $">.{Environment.NewLine}"));

                    //Write graph
                    WriteTurtleGraph(sw, graph, prefixes, false);
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
        /// <exception cref="RDFModelException"></exception>
        internal static RDFGraph Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open), null);

        /// <summary>
        /// Deserializes the given Turtle stream to a graph
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static RDFGraph Deserialize(Stream inputStream, Uri graphContext)
        {
            RDFGraph result = new RDFGraph().SetContext(graphContext);
            RDFTurtleContext turtleContext = new RDFTurtleContext();

            try
            {
                #region deserialize
                using (StreamReader streamReader = new StreamReader(inputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    using (RDFStreamReader rdfStreamReader = new RDFStreamReader(streamReader))
                    {
                        int bufferChar = SkipWhitespace(rdfStreamReader, turtleContext);
                        while (bufferChar != -1)
                        {
                            ParseStatement(rdfStreamReader, turtleContext, result);
                            //After parsing of a statement we discard spaces and comments
                            //and seek for the next eventual statement, until finally EOL
                            bufferChar = SkipWhitespace(rdfStreamReader, turtleContext);
                        }
                    }
                }
                RDFNamespaceRegister.RemoveTemporaryNamespaces();
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot deserialize Turtle because: " + ex.Message, ex);
            }

            return result;
        }
        #endregion

        #region Utilities

        #region Declarations
        /// <summary>
        /// Represents the context of the Turtle parser
        /// </summary>
        internal class RDFTurtleContext
        {
            #region Properties
            /// <summary>
            /// Indicates the current subject
            /// </summary>
            internal RDFResource Subject { get; set; }
            /// <summary>
            /// Indicates the current predicate
            /// </summary>
            internal RDFResource Predicate { get; set; }
            /// <summary>
            /// Indicates the current object/literal
            /// </summary>
            internal RDFPatternMember Object { get; set; }
            /// <summary>
            /// Context for Uri hashing
            /// </summary>
            internal Dictionary<string, long> HashContext { get; set; }
            #endregion

            #region Ctors
            internal RDFTurtleContext()
                => HashContext = new Dictionary<string, long>();
            #endregion
        }
        #endregion

        #region Parse.CodePoint
        /// <summary>
        /// Peeks at the next Unicode code point without advancing the reader
        /// </summary>
        internal static int PeekCodePoint(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext)
            => rdfStreamReader.Peek();

        /// <summary>
        /// Reads the next Unicode code point from the reader
        /// </summary>
        internal static int ReadCodePoint(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext)
            => rdfStreamReader.Read();

        /// <summary>
        /// Unreads the given Unicode code point from the reader
        /// </summary>
        internal static void UnreadCodePoint(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, int codePoint)
            => rdfStreamReader.Unread(codePoint);

        /// <summary>
        /// Unreads the given Unicode code point from the reader
        /// </summary>
        internal static void UnreadCodePoint(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, string codePoints)
        => rdfStreamReader.Unread(codePoints);
        #endregion

        #region Parse.Grammar
        /// <summary>
        /// Parses the Turtle data in order to detect a valid directive or statement
        /// </summary>
        internal static void ParseStatement(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            StringBuilder sb = new StringBuilder();
            do
            {
                int codePoint = ReadCodePoint(rdfStreamReader, turtleContext);
                if (codePoint == -1 || IsWhitespace(codePoint))
                {
                    UnreadCodePoint(rdfStreamReader, turtleContext, codePoint);
                    break;
                }
                sb.Append(char.ConvertFromUtf32(codePoint));
            } while (sb.Length < 8);

            string directive = sb.ToString();
            if (directive[0] == '@'
                 || directive.Equals("prefix", StringComparison.OrdinalIgnoreCase)
                 || directive.Equals("base", StringComparison.OrdinalIgnoreCase)
                 || directive.Equals("version", StringComparison.OrdinalIgnoreCase))
            {
                ParseDirective(rdfStreamReader, turtleContext, result, directive);
                SkipWhitespace(rdfStreamReader, turtleContext);
                // Turtle @base and @prefix directives MUST end with "."
                if (directive[0] == '@')
                {
                    VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), ".");
                }
                // SPARQL BASE and PREFIX directives MUST NOT end with "."
                else
                {
                    if (PeekCodePoint(rdfStreamReader, turtleContext) == '.')
                        throw new RDFModelException("SPARQL directive '" + directive + "' must not end with '.'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                }
            }
            else
            {
                UnreadCodePoint(rdfStreamReader, turtleContext, directive);
                ParseTriples(rdfStreamReader, turtleContext, result);
                SkipWhitespace(rdfStreamReader, turtleContext);
                VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), ".");
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid directive
        /// </summary>
        internal static void ParseDirective(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result, string directive)
        {
            switch (directive.ToLowerInvariant())
            {
                case "@prefix":
                case "prefix":
                    ParsePrefixID(rdfStreamReader, turtleContext, result);
                    break;

                case "@base":
                case "base":
                    ParseBase(rdfStreamReader, turtleContext, result);
                    break;

                case "@version":
                case "version":
                    throw new RDFModelException("Found version directive: this announces presence of unsupported RDF-Star/SPARQL-Star data!");

                //Any other directives are not allowed
                default:
                    throw new RDFModelException($"Found unknown directive: {directive} {GetTurtleContextCoordinates(rdfStreamReader, turtleContext)}");
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement
        /// </summary>
        internal static void ParseTriples(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            int bufChar = PeekCodePoint(rdfStreamReader, turtleContext);

            // If the first character is an open bracket we need to decide which of
            // the two parsing methods for blank nodes to use
            if (bufChar == '[')
            {
                ReadCodePoint(rdfStreamReader, turtleContext);
                SkipWhitespace(rdfStreamReader, turtleContext);
                bufChar = PeekCodePoint(rdfStreamReader, turtleContext);
                if (bufChar == ']')
                {
                    ReadCodePoint(rdfStreamReader, turtleContext);
                    turtleContext.Subject = new RDFResource();
                    SkipWhitespace(rdfStreamReader, turtleContext);
                    ParsePredicateObjectList(rdfStreamReader, turtleContext, result);
                }
                else
                {
                    //We have to parse an implicit blank, so we must rewind to the
                    //initial '[' character in order for the method to work
                    while (bufChar != '[')
                    {
                        UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
                        bufChar = PeekCodePoint(rdfStreamReader, turtleContext);
                    }
                    turtleContext.Subject = ParseImplicitBlank(rdfStreamReader, turtleContext, result);
                }
                SkipWhitespace(rdfStreamReader, turtleContext);
                bufChar = PeekCodePoint(rdfStreamReader, turtleContext);

                // if this is not the end of the statement, recurse into the list of
                // predicate and objects, using the subject parsed above as the subject
                // of the statement.
                if (bufChar != '.')
                    ParsePredicateObjectList(rdfStreamReader, turtleContext, result);
            }
            else
            {
                ParseSubject(rdfStreamReader, turtleContext, result);
                SkipWhitespace(rdfStreamReader, turtleContext);
                ParsePredicateObjectList(rdfStreamReader, turtleContext, result);
            }

            turtleContext.Subject = null;
            turtleContext.Predicate = null;
            turtleContext.Object = null;
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement subject
        /// </summary>
        internal static void ParseSubject(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            switch (PeekCodePoint(rdfStreamReader, turtleContext))
            {
                case '(':
                    turtleContext.Subject = ParseCollection(rdfStreamReader, turtleContext, result);
                    break;
                case '[':
                    turtleContext.Subject = ParseImplicitBlank(rdfStreamReader, turtleContext, result);
                    break;
                default:
                {
                    object value = ParseValue(rdfStreamReader, turtleContext, result);
                    switch (value)
                    {
                        case Uri _:
                            turtleContext.Subject = new RDFResource(value.ToString(), turtleContext.HashContext);
                            break;
                        case RDFResource valueResource:
                            turtleContext.Subject = valueResource;
                            break;
                        default:
                        {
                            if (value != null)
                                throw new RDFModelException("Illegal subject value: " + value + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                            break;
                        }
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement predicate
        /// </summary>
        internal static RDFResource ParsePredicate(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            // Check if the short-cut 'a' is used
            int bufChar1 = ReadCodePoint(rdfStreamReader, turtleContext);
            if (bufChar1 == 'a')
            {
                int bufChar2 = ReadCodePoint(rdfStreamReader, turtleContext);

                if (IsWhitespace(bufChar2))
                    // Short-cut is used, return the rdf:type URI
                    return RDFVocabulary.RDF.TYPE;

                // Short-cut is not used, unread all characters
                UnreadCodePoint(rdfStreamReader, turtleContext, bufChar2);
            }
            UnreadCodePoint(rdfStreamReader, turtleContext, bufChar1);

            // Predicate is a normal resource
            object predicate = ParseValue(rdfStreamReader, turtleContext, result);
            switch (predicate)
            {
                case Uri _:
                    return new RDFResource(predicate.ToString(), turtleContext.HashContext);
                case RDFResource predRes:
                    return predRes;
                default:
                    throw new RDFModelException("Illegal predicate value: " + predicate + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement predicate-object list
        /// </summary>
        internal static void ParsePredicateObjectList(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            turtleContext.Predicate = ParsePredicate(rdfStreamReader, turtleContext, result);

            SkipWhitespace(rdfStreamReader, turtleContext);

            ParseObjectList(rdfStreamReader, turtleContext, result);

            while (SkipWhitespace(rdfStreamReader, turtleContext) == ';')
            {
                ReadCodePoint(rdfStreamReader, turtleContext);

                int bufChar = SkipWhitespace(rdfStreamReader, turtleContext);
                if (bufChar == '.' || bufChar == ']' || bufChar == '}') break;

                if (bufChar == ';')
                    // empty predicateObjectList, skip to next
                    continue;

                turtleContext.Predicate = ParsePredicate(rdfStreamReader, turtleContext, result);

                SkipWhitespace(rdfStreamReader, turtleContext);

                ParseObjectList(rdfStreamReader, turtleContext, result);
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement object list
        /// </summary>
        internal static void ParseObjectList(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            ParseObject(rdfStreamReader, turtleContext, result);

            while (SkipWhitespace(rdfStreamReader, turtleContext) == ',')
            {
                ReadCodePoint(rdfStreamReader, turtleContext);
                SkipWhitespace(rdfStreamReader, turtleContext);
                ParseObject(rdfStreamReader, turtleContext, result);
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid statement object
        /// </summary>
        internal static void ParseObject(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            switch (PeekCodePoint(rdfStreamReader, turtleContext))
            {
                case '(':
                    turtleContext.Object = ParseCollection(rdfStreamReader, turtleContext, result);
                    break;
                case '[':
                    turtleContext.Object = ParseImplicitBlank(rdfStreamReader, turtleContext, result);
                    break;
                default:
                    object value = ParseValue(rdfStreamReader, turtleContext, result); //Uri or RDFPatternMember
                    switch (value)
                    {
                        case Uri _:
                            turtleContext.Object = new RDFResource(value.ToString(), turtleContext.HashContext);
                            break;
                        case RDFPatternMember pmemberValue:
                            turtleContext.Object = pmemberValue;
                            break;
                    }
                    break;
            }

            //report statement
            if (turtleContext.Object is RDFLiteral tcLit)
                result.AddTriple(new RDFTriple(turtleContext.Subject, turtleContext.Predicate, tcLit));
            else
                result.AddTriple(new RDFTriple(turtleContext.Subject, turtleContext.Predicate, (RDFResource)turtleContext.Object));
        }

        /// <summary>
        /// Parses a collection, e.g. ( item1 item2 item3 )
        /// </summary>
        internal static RDFResource ParseCollection(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), "(");

            int bufChar = SkipWhitespace(rdfStreamReader, turtleContext);
            if (bufChar == ')')
            {
                // Empty list (rdf:Nil)
                ReadCodePoint(rdfStreamReader, turtleContext);
                return RDFVocabulary.RDF.NIL;
            }

            //report statement
            RDFResource listRoot = new RDFResource();
            result.AddTriple(new RDFTriple(listRoot, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));

            // Remember current subject and predicate
            RDFResource oldSubject = turtleContext.Subject;
            RDFResource oldPredicate = turtleContext.Predicate;

            // generated bNode becomes subject, predicate becomes rdf:first
            turtleContext.Subject = listRoot;
            turtleContext.Predicate = RDFVocabulary.RDF.FIRST;

            ParseObject(rdfStreamReader, turtleContext, result);

            RDFResource bNode = listRoot;
            while (SkipWhitespace(rdfStreamReader, turtleContext) != ')')
            {
                // Create another list node and link it to the previous
                RDFResource newNode = new RDFResource();

                //report statement
                result.AddTriple(new RDFTriple(bNode, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));
                result.AddTriple(new RDFTriple(bNode, RDFVocabulary.RDF.REST, newNode));

                // New node becomes the current
                turtleContext.Subject = bNode = newNode;

                ParseObject(rdfStreamReader, turtleContext, result);
            }

            // Skip ')'
            ReadCodePoint(rdfStreamReader, turtleContext);

            // Close the list
            result.AddTriple(new RDFTriple(bNode, RDFVocabulary.RDF.TYPE, RDFVocabulary.RDF.LIST));
            result.AddTriple(new RDFTriple(bNode, RDFVocabulary.RDF.REST, RDFVocabulary.RDF.NIL));

            // Restore previous subject and predicate
            turtleContext.Subject = oldSubject;
            turtleContext.Predicate = oldPredicate;

            return listRoot;
        }

        /// <summary>
        /// Parses an implicit blank node. This method parses the token []
        /// and predicateObjectLists that are surrounded by square brackets.
        /// </summary>
        internal static RDFResource ParseImplicitBlank(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), "[");
            RDFResource bNode = new RDFResource(); // createBNode()

            SkipWhitespace(rdfStreamReader, turtleContext);
            int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            if (bufChar != ']')
            {
                UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);

                // Remember current subject and predicate
                RDFResource oldSubject = turtleContext.Subject;
                RDFResource oldPredicate = turtleContext.Predicate;

                // generated bNode becomes subject
                turtleContext.Subject = bNode;

                // Enter recursion with nested predicate-object list
                SkipWhitespace(rdfStreamReader, turtleContext);
                ParsePredicateObjectList(rdfStreamReader, turtleContext, result);
                SkipWhitespace(rdfStreamReader, turtleContext);

                // Read closing bracket
                VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), "]");

                // Restore previous subject and predicate
                turtleContext.Subject = oldSubject;
                turtleContext.Predicate = oldPredicate;
            }

            return bNode;
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid namespace prefix
        /// </summary>
        internal static void ParsePrefixID(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            SkipWhitespace(rdfStreamReader, turtleContext);

            // Read prefix ID (e.g. "rdf:" or ":")
            StringBuilder prefixID = new StringBuilder();
            while (true)
            {
                int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                if (bufChar == ':')
                {
                    UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
                    break;
                }

                if (IsWhitespace(bufChar))
                    break;

                if (bufChar == -1)
                    throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                prefixID.Append(char.ConvertFromUtf32(bufChar));
            }

            SkipWhitespace(rdfStreamReader, turtleContext);
            VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), ":");
            SkipWhitespace(rdfStreamReader, turtleContext);

            // Read the namespace URI
            Uri nspace = ParseURI(rdfStreamReader, turtleContext, result);

            // Store and report this namespace mapping
            string prefixStr = prefixID.ToString();
            string namespaceStr = nspace.ToString();
            // If prefix is empty it must be considered default context of the graph
            if (string.IsNullOrEmpty(prefixStr))
            {
                prefixStr = $"AutoNS{DateTime.UtcNow.Minute}{DateTime.UtcNow.Second}";
                result.SetContext(new Uri(namespaceStr));
            }

            //Support eventual redefinement of temporary namespaces
            RDFNamespace registerNSpace = RDFNamespaceRegister.GetByPrefix(prefixStr);
            if (registerNSpace == null)
                RDFNamespaceRegister.AddNamespace(new RDFNamespace(prefixStr, namespaceStr).SetTemporary(true));
            else if (registerNSpace.IsTemporary)
            {
                RDFNamespaceRegister.RemoveByPrefix(prefixStr);
                RDFNamespaceRegister.AddNamespace(new RDFNamespace(prefixStr, namespaceStr).SetTemporary(true));
            }
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid base directive
        /// </summary>
        internal static void ParseBase(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            SkipWhitespace(rdfStreamReader, turtleContext);
            Uri baseURI = ParseURI(rdfStreamReader, turtleContext, result);
            result.SetContext(baseURI);
        }

        /// <summary>
        /// Parses the Turtle data in order to detect a valid Uri
        /// </summary>
        internal static Uri ParseURI(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            StringBuilder uriBuf = new StringBuilder();

            // First character should be '<'
            int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            VerifyCharacterOrFail(rdfStreamReader, turtleContext, bufChar, "<");

            // Read up to the next '>' character
            while (true)
            {
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                if (bufChar == '>')
                    break;

                switch (bufChar)
                {
                    case -1:
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                    case ' ':
                        throw new RDFModelException("Uri included an unencoded space: '" + bufChar + "'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                }

                uriBuf.Append(char.ConvertFromUtf32(bufChar));

                if (bufChar == '\\')
                {
                    // This escapes the next character, which might be a '>'
                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    if (bufChar == -1)
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                    if (bufChar != 'u' && bufChar != 'U')
                        throw new RDFModelException("Uri includes string escapes: '\\" + bufChar + "'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                    uriBuf.Append(char.ConvertFromUtf32(bufChar));
                }
            }

            string uriString = DecodeString(rdfStreamReader, turtleContext, uriBuf.ToString());
            //Absolute: use as found
            if (Uri.IsWellFormedUriString(uriString, UriKind.Absolute))
                return new Uri(uriString);
            //Relative: append to graph context
            if (Uri.IsWellFormedUriString(uriString, UriKind.Relative))
                return new Uri(string.Concat(result.ToString(), uriString));
            //PureFragment: append to graph context
            return uriString.Equals("#")
                ? new Uri(string.Concat(result.ToString().TrimEnd('#'), uriString))
                //Error: not well-formed, so throw exception
                : throw new RDFModelException("Uri is not well-formed" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
        }

        /// <summary>
        /// Parses an RDF value. This method parses uriref, qname, node ID, quoted
        /// literal, integer, double and boolean.
        /// </summary>
        internal static object ParseValue(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            int bufChar = PeekCodePoint(rdfStreamReader, turtleContext);
            if (bufChar == '<')
                return ParseURI(rdfStreamReader, turtleContext, result); // uriref, e.g. <foo://bar>
            if (bufChar == ':' || IsPrefixStartChar(bufChar))
                return ParseQNameOrBoolean(rdfStreamReader, turtleContext, result); // qname or boolean

            switch (bufChar)
            {
                case '_':
                    return ParseNodeID(rdfStreamReader, turtleContext); // node ID, e.g. _:n1
                case '"':
                case '\'':
                    return ParseQuotedLiteral(rdfStreamReader, turtleContext, result); // quoted literal, e.g. "foo" or """foo""" or 'foo' or '''foo'''
            }
            if (IsNumber(bufChar) || bufChar == '.' || bufChar == '+' || bufChar == '-')
                return ParseNumber(rdfStreamReader, turtleContext); // integer or double, e.g. 123 or 1.2e3

            if (bufChar == -1)
                throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

            throw new RDFModelException("Expected an RDF value here, found '" + char.ConvertFromUtf32(bufChar) + "'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
        }

        /// <summary>
        /// Parses a blank node ID, e.g. _:node1
        /// </summary>
        internal static RDFResource ParseNodeID(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext)
        {
            // Node ID should start with "_:"
            VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), "_");
            VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), ":");

            // Read the node ID
            int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            if (bufChar == -1)
                throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
            if (!IsBLANK_NODE_LABEL_StartChar(bufChar))
                throw new RDFModelException("Expected a letter, found '" + (char)bufChar + "'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

            StringBuilder name = new StringBuilder();
            name.Append(char.ConvertFromUtf32(bufChar));

            // Read all following letter and numbers, they are part of the name
            bufChar = ReadCodePoint(rdfStreamReader, turtleContext);

            // If we would never go into the loop we must unread now
            if (!IsBLANK_NODE_LABEL_Char(bufChar))
                UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);

            while (IsBLANK_NODE_LABEL_Char(bufChar))
            {
                int previous = bufChar;
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                if (previous == '.' && (bufChar == -1 || IsWhitespace(bufChar) || bufChar == '<' || bufChar == '_'))
                {
                    UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
                    UnreadCodePoint(rdfStreamReader, turtleContext, previous);
                    break;
                }
                name.Append((char)previous);
                if (!IsBLANK_NODE_LABEL_Char(bufChar))
                    UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
            }

            return new RDFResource($"bnode:{name}");
        }

        /// <summary>
        /// Parses a number
        /// </summary>
        internal static RDFTypedLiteral ParseNumber(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext)
        {
            StringBuilder value = new StringBuilder();
            RDFModelEnums.RDFDatatypes dt = RDFModelEnums.RDFDatatypes.XSD_INTEGER;

            int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);

            // read optional sign character
            if (bufChar == '+' || bufChar == '-')
            {
                value.Append(char.ConvertFromUtf32(bufChar));
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            }

            while (IsNumber(bufChar))
            {
                value.Append(char.ConvertFromUtf32(bufChar));
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            }

            if (bufChar == '.' || bufChar == 'e' || bufChar == 'E')
            {
                // read optional fractional digits
                if (bufChar == '.')
                {
                    int next = PeekCodePoint(rdfStreamReader, turtleContext);
                    // Treat '.' as statement terminator at EOF only when we already parsed at least one digit
                    // (e.g., "30.") or when whitespace follows. Otherwise, attempt to parse as decimal
                    // which will surface a useful error for a stray '.' token.
                    if ((value.Length > 0 && next == -1) || IsWhitespace(PeekCodePoint(rdfStreamReader, turtleContext)))
                    {
                        // We're parsing an integer that did not have a space before the period to end the statement
                    }
                    else
                    {
                        value.Append(char.ConvertFromUtf32(bufChar));

                        bufChar = ReadCodePoint(rdfStreamReader, turtleContext);

                        while (IsNumber(bufChar))
                        {
                            value.Append(char.ConvertFromUtf32(bufChar));
                            bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                        }

                        if (value.Length == 1)
                            // We've only parsed a '.'
                            throw new RDFModelException("Object for statement missing" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                        // We're parsing a decimal or a double
                        dt = RDFModelEnums.RDFDatatypes.XSD_DECIMAL;
                    }
                }
                else
                {
                    if (value.Length == 0)
                        // We've only parsed a '.'
                        throw new RDFModelException("Object for statement missing" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                }

                // read optional exponent
                if (bufChar == 'e' || bufChar == 'E')
                {
                    dt = RDFModelEnums.RDFDatatypes.XSD_DOUBLE;
                    value.Append(char.ConvertFromUtf32(bufChar));

                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    if (bufChar == '+' || bufChar == '-')
                    {
                        value.Append(char.ConvertFromUtf32(bufChar));
                        bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    }

                    if (!IsNumber(bufChar))
                        throw new RDFModelException("Exponent value missing" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                    value.Append(char.ConvertFromUtf32(bufChar));

                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    while (IsNumber(bufChar))
                    {
                        value.Append(char.ConvertFromUtf32(bufChar));
                        bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    }
                }
            }

            // Unread last character, it isn't part of the number
            UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);

            // Return result as a typed literal
            return new RDFTypedLiteral(value.ToString(), dt);
        }

        /// <summary>
        /// Parses qnames and boolean values, which have equivalent starting characters
        /// </summary>
        internal static object ParseQNameOrBoolean(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            // First character should be a ':' or a letter
            int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            if (bufChar == -1)
                throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
            if (bufChar != ':' && !IsPrefixStartChar(bufChar))
                throw new RDFModelException("Expected a ':' or a letter, found '" + char.ConvertFromUtf32(bufChar) + "'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

            int previousChar;
            string nspace;
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
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                while (IsPrefixChar(bufChar))
                {
                    prefix.Append(char.ConvertFromUtf32(bufChar));
                    previousChar = bufChar;
                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                }

                while (previousChar == '.' && prefix.Length > 0)
                {
                    // '.' is a legal prefix name char, but can not appear at the end
                    UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
                    bufChar = previousChar;
                    prefix.Remove(prefix.Length - 1, 1);
                    previousChar = prefix.ToString().Last();
                }

                if (bufChar != ':')
                {
                    // prefix may actually be a boolean value
                    string value = prefix.ToString();
                    if (value.Equals("true", StringComparison.OrdinalIgnoreCase)
                         || value.Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
                        return new RDFTypedLiteral(value, RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);
                    }
                }
                else
                {
                    if (previousChar == '.')
                        // '.' is a legal prefix name char, but can not appear at the end
                        throw new RDFModelException("prefix can not end with with '.'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                }

                VerifyCharacterOrFail(rdfStreamReader, turtleContext, bufChar, ":");

                nspace = RDFNamespaceRegister.GetByPrefix(prefix.ToString())?.ToString();
            }

            // bufChar == ':', read optional local name
            StringBuilder localName = new StringBuilder();
            bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            if (IsNameStartChar(bufChar))
            {
                if (bufChar == '\\')
                    localName.Append(ReadLocalEscapedChar(rdfStreamReader, turtleContext));
                else
                    localName.Append(char.ConvertFromUtf32(bufChar));

                previousChar = bufChar;
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                while (IsNameChar(bufChar))
                {
                    if (bufChar == '\\')
                        localName.Append(ReadLocalEscapedChar(rdfStreamReader, turtleContext));
                    else
                        localName.Append(char.ConvertFromUtf32(bufChar));
                    previousChar = bufChar;
                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                }

                // Unread last character
                UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);

                if (previousChar == '.')
                {
                    // '.' is a legal name char, but can not appear at the end, so is not actually part of the name
                    UnreadCodePoint(rdfStreamReader, turtleContext, previousChar);
                    localName.Remove(localName.Length - 1, 1);
                }
            }
            else
            {
                // Unread last character
                UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
            }

            string localNameString = localName.ToString();
            return localNameString.Where((t, i) => t == '%' && (i > localNameString.Length - 3
                                                                || !Uri.IsHexDigit(localNameString[i + 1])
                                                                || !Uri.IsHexDigit(localNameString[i + 2]))).Any()
                ? throw new RDFModelException("Found incomplete percent-encoded sequence: " + localNameString + GetTurtleContextCoordinates(rdfStreamReader, turtleContext))
                : new Uri(string.Concat(nspace ?? result.Context.ToString(), localNameString));
        }

        /// <summary>
        /// Parses a quoted string, optionally followed by a language tag or datatype.
        /// </summary>
        internal static RDFLiteral ParseQuotedLiteral(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, RDFGraph result)
        {
            string label = ParseQuotedString(rdfStreamReader, turtleContext);

            // Check for presence of a language tag or datatype
            int bufChar = PeekCodePoint(rdfStreamReader, turtleContext);
            switch (bufChar)
            {
                case '@':
                {
                    ReadCodePoint(rdfStreamReader, turtleContext);

                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    if (bufChar == -1)
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                    if (!IsLanguageStartChar(bufChar))
                        throw new RDFModelException("Expected a letter, found '" + char.ConvertFromUtf32(bufChar) + "'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                    // Read language
                    StringBuilder lang = new StringBuilder();
                    lang.Append(char.ConvertFromUtf32(bufChar));

                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    while (!IsWhitespace(bufChar))
                    {
                        if (bufChar == '.' || bufChar == ';' || bufChar == ',' || bufChar == ')' || bufChar == ']' || bufChar == -1)
                            break;

                        if (!IsLanguageChar(bufChar))
                            throw new RDFModelException("Illegal language tag char: '" + char.ConvertFromUtf32(bufChar) + "'" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                        lang.Append(char.ConvertFromUtf32(bufChar));
                        bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    }

                    UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);

                    return new RDFPlainLiteral(label, lang.ToString());
                }
                case '^':
                {
                    ReadCodePoint(rdfStreamReader, turtleContext);

                    // next character should be another '^'
                    VerifyCharacterOrFail(rdfStreamReader, turtleContext, ReadCodePoint(rdfStreamReader, turtleContext), "^");

                    SkipWhitespace(rdfStreamReader, turtleContext);

                    // Read datatype
                    object datatype = ParseValue(rdfStreamReader, turtleContext, result);
                    if (datatype is Uri datatypeUri)
                        return new RDFTypedLiteral(label, RDFDatatypeRegister.GetDatatype(datatypeUri.ToString()));

                    throw new RDFModelException("Illegal datatype value: " + datatype + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                }
                default:
                    return new RDFPlainLiteral(label);
            }
        }

        /// <summary>
        /// Parses a quoted string, which is either a "normal string" or a """long string"""
        /// </summary>
        internal static string ParseQuotedString(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext)
        {
            string result;

            int c1 = ReadCodePoint(rdfStreamReader, turtleContext);

            // First character should be '"' or "'"
            VerifyCharacterOrFail(rdfStreamReader, turtleContext, c1, "\"\'");

            // Check for long-string, which starts and ends with three double quotes
            int c2 = ReadCodePoint(rdfStreamReader, turtleContext);
            int c3 = ReadCodePoint(rdfStreamReader, turtleContext);

            if ((c1 == '"' && c2 == '"' && c3 == '"') || (c1 == '\'' && c2 == '\'' && c3 == '\''))
            {
                // Long string
                result = ParseLongString(rdfStreamReader, turtleContext, c2);
            }
            else
            {
                // Normal string
                UnreadCodePoint(rdfStreamReader, turtleContext, c3);
                UnreadCodePoint(rdfStreamReader, turtleContext, c2);

                result = ParseString(rdfStreamReader, turtleContext, c1);
            }

            // Unescape any escape sequences
            return DecodeString(rdfStreamReader, turtleContext, result);
        }

        /// <summary>
        /// Parses a "normal string". This method requires that the opening character has already been parsed.
        /// </summary>
        internal static string ParseString(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, int closingCharacter)
        {
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                if (bufChar == closingCharacter)
                    break;

                switch (bufChar)
                {
                    case -1:
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                    //Unquoted literals cannot contain carriage return
                    case '\r':
                    case '\n':
                        throw new RDFModelException("Illegal carriage return or new line in literal");
                }
                sb.Append(char.ConvertFromUtf32(bufChar));

                if (bufChar == '\\')
                {
                    // This escapes the next character, which might be a '"'
                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    if (bufChar == -1)
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                    sb.Append(char.ConvertFromUtf32(bufChar));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parses a """long string""". This method requires that the first three characters have already been parsed.
        /// </summary>
        internal static string ParseLongString(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, int closingCharacter)
        {
            StringBuilder sb = new StringBuilder();

            int doubleQuoteCount = 0;
            while (doubleQuoteCount < 3)
            {
                int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                if (bufChar == -1)
                    throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                if (bufChar == closingCharacter)
                    doubleQuoteCount++;
                else
                    doubleQuoteCount = 0;

                sb.Append(char.ConvertFromUtf32(bufChar));

                if (bufChar == '\\')
                {
                    // This escapes the next character, which might be a '"'
                    bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                    if (bufChar == -1)
                        throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                    sb.Append(char.ConvertFromUtf32(bufChar));
                }
            }

            return sb.ToString().Substring(0, sb.Length - 3);
        }

        /// <summary>
        /// Decodes an encoded Turtle string. Any \-escape sequences are substituted with their decoded value.
        /// </summary>
        internal static string DecodeString(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, string s)
        {
            int backSlashIdx = s.IndexOf('\\');
            if (backSlashIdx == -1)
                return s; // No escaped characters found

            int startIdx = 0;
            int sLength = s.Length;
            StringBuilder sb = new StringBuilder();

            while (backSlashIdx != -1)
            {
                sb.Append(s, startIdx, backSlashIdx - startIdx);

                if (backSlashIdx + 1 >= sLength)
                    throw new RDFModelException("Unescaped backslash in: " + s + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

                switch (s[backSlashIdx + 1])
                {
                    case 't':
                        sb.Append('\t');
                        startIdx = backSlashIdx + 2;
                        break;
                    case 'r':
                        sb.Append('\r');
                        startIdx = backSlashIdx + 2;
                        break;
                    case 'n':
                        sb.Append('\n');
                        startIdx = backSlashIdx + 2;
                        break;
                    case 'b':
                        sb.Append('\b');
                        startIdx = backSlashIdx + 2;
                        break;
                    case 'f':
                        sb.Append('\f');
                        startIdx = backSlashIdx + 2;
                        break;
                    case '"':
                        sb.Append('"');
                        startIdx = backSlashIdx + 2;
                        break;
                    case '\'':
                        sb.Append('\'');
                        startIdx = backSlashIdx + 2;
                        break;
                    case '>':
                        sb.Append('>');
                        startIdx = backSlashIdx + 2;
                        break;
                    case '\\':
                        sb.Append('\\');
                        startIdx = backSlashIdx + 2;
                        break;
                    /*  \\uxxxx  */
                    case 'u' when backSlashIdx + 5 >= sLength:
                        throw new RDFModelException("Incomplete Unicode escape sequence in: " + s + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                    case 'u':
                    {
                        string uValue = s.Substring(backSlashIdx + 2, 4);
                        try
                        {
                            int cp = int.Parse(uValue, NumberStyles.AllowHexSpecifier);
                            sb.Append(char.ConvertFromUtf32(cp));
                            startIdx = backSlashIdx + 6;
                        }
                        catch
                        {
                            throw new RDFModelException("Illegal Unicode escape sequence '\\u" + uValue + "' in: " + s + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                        }

                        break;
                    }
                    /*  \\Uxxxxxxxx  */
                    case 'U' when backSlashIdx + 9 >= sLength:
                        throw new RDFModelException("Incomplete Unicode escape sequence in: " + s + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                    case 'U':
                    {
                        string UValue = s.Substring(backSlashIdx + 2, 8);
                        try
                        {
                            int cp = int.Parse(UValue, NumberStyles.AllowHexSpecifier);
                            sb.Append(char.ConvertFromUtf32(cp));
                            startIdx = backSlashIdx + 10;
                        }
                        catch
                        {
                            throw new RDFModelException("Illegal Unicode escape sequence '\\U" + UValue + "' in: " + s + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                        }

                        break;
                    }
                    default:
                        throw new RDFModelException("Unescaped backslash in: " + s + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
                }

                backSlashIdx = s.IndexOf('\\', startIdx);
            }

            sb.Append(s, startIdx, s.Length - startIdx);

            return sb.ToString();
        }

        /// <summary>
        /// Consumes any whitespace characters (space, tab, line feed, newline) and comments(#-style) from the Turtle data.
        /// After this method has been called, the first character that is returned is either a non-ignorable character or EOF.
        /// </summary>
        internal static int SkipWhitespace(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext)
        {
            int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            while (IsWhitespace(bufChar) || bufChar == '#')
            {
                if (bufChar == '#')
                    SkipComment(rdfStreamReader, turtleContext);
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            }
            UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
            return bufChar;
        }

        /// <summary>
        /// Consumes characters from reader until the first EOL has been read.
        /// </summary>
        internal static void SkipComment(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext)
        {
            int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            while (bufChar != -1 && bufChar != 0xD && bufChar != 0xA)
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);

            // bufChar is equal to -1, \r or \n.
            // In case bufChar is equal to \r, we should also read a following \n.
            if (bufChar == 0xD)
            {
                bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
                if (bufChar != 0xA)
                    UnreadCodePoint(rdfStreamReader, turtleContext, bufChar);
            }
        }

        /// <summary>
        /// Verifies that the supplied character code point is one of the expected chars.
        /// This method will throw an exception if this is not the case.
        /// </summary>
        internal static void VerifyCharacterOrFail(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext, int codePoint, string expected)
        {
            if (codePoint == -1)
                throw new RDFModelException("Unexpected end of Turtle file" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));

            string supplied = char.ConvertFromUtf32(codePoint);
            if (expected.IndexOf(supplied, StringComparison.Ordinal) == -1)
            {
                StringBuilder msg = new StringBuilder("Unexpected character found" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext) + ": expected ");
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
                msg.Append('\'');
                throw new RDFModelException(msg.ToString());
            }
        }

        internal static char ReadLocalEscapedChar(RDFStreamReader rdfStreamReader, RDFTurtleContext turtleContext)
        {
            int bufChar = ReadCodePoint(rdfStreamReader, turtleContext);
            if (IsLocalEscapedChar(bufChar))
                return (char)bufChar;

            throw new RDFModelException("Found '" + char.ConvertFromUtf32(bufChar) + "', expected one of: _~.-!$&\'()*+,;=/?#@%" + GetTurtleContextCoordinates(rdfStreamReader, turtleContext));
        }
        #endregion

        #region Parse.Check
        /// <summary>
        /// Check if the supplied code point represents a whitespace character
        /// </summary>
        internal static bool IsWhitespace(int codePoint)
            => codePoint == 0x20 || codePoint == 0x9 || codePoint == 0xA || codePoint == 0xD; // Whitespace character are space, tab, newline and carriage return

        /// <summary>
        /// Check if the supplied code point represents a numeric character
        /// </summary>
        internal static bool IsNumber(int codePoint)
            => char.IsNumber((char)codePoint);

        /// <summary>
        /// Check if the supplied code point represents a valid name start character
        /// </summary>
        internal static bool IsNameStartChar(int codePoint)
            => IsPN_CHARS_U(codePoint)
                || codePoint == ':'
                || IsNumber(codePoint)
                || codePoint == '\\'
                || codePoint == '%';

        /// <summary>
        /// Check if the supplied code point represents a valid name character
        /// </summary>
        internal static bool IsNameChar(int codePoint)
            => IsPN_CHARS(codePoint)
                || codePoint == '.'
                || codePoint == ':'
                || codePoint == '\\'
                || codePoint == '%';

        /// <summary>
        /// Check if the supplied code point represents a valid prefixed name start character
        /// </summary>
        internal static bool IsPrefixStartChar(int codePoint)
            => IsPN_CHARS_BASE(codePoint);

        /// <summary>
        /// Check if the supplied code point represents a valid prefix character
        /// </summary>
        internal static bool IsPrefixChar(int codePoint)
            => IsPN_CHARS_BASE(codePoint)
                || IsPN_CHARS(codePoint)
                || codePoint == '.';

        /// <summary>
        /// Check if the supplied code point represents a valid language tag start character
        /// </summary>
        internal static bool IsLanguageStartChar(int codePoint)
            => char.IsLetter((char)codePoint);

        /// <summary>
        /// Check if the supplied code point represents a valid language tag character
        /// </summary>
        internal static bool IsLanguageChar(int codePoint)
            => char.IsLetter((char)codePoint)
                || char.IsNumber((char)codePoint)
                || codePoint == '-';

        /// <summary>
        /// Check if the supplied code point represents a valid local escaped character.
        /// </summary>
        internal static bool IsLocalEscapedChar(int codePoint)
            => "_~.-!$&\'()*+,;=/?#@%".IndexOf((char)codePoint) > -1;

        /// <summary>
        /// Check if the supplied code point represents a valid prefixed name start character
        /// </summary>
        internal static bool IsBLANK_NODE_LABEL_StartChar(int codePoint)
            => IsPN_CHARS_U(codePoint) || char.IsNumber((char)codePoint);

        /// <summary>
        /// Check if the supplied code point represents a valid blank node label character
        /// </summary>
        internal static bool IsBLANK_NODE_LABEL_Char(int codePoint)
            => IsPN_CHARS(codePoint) || codePoint == '.';

        /// <summary>
        /// Check if the supplied code point represents a valid prefixed name character
        /// </summary>
        internal static bool IsPN_CHARS(int codePoint)
            => IsPN_CHARS_U(codePoint)
                || char.IsNumber((char)codePoint)
                || codePoint == '-'
                || codePoint == 0x00B7
                || (codePoint >= 0x0300 && codePoint <= 0x036F)
                || (codePoint >= 0x203F && codePoint <= 0x2040);

        /// <summary>
        /// Check if the supplied code point represents either a valid prefixed name base character or an underscore
        /// </summary>
        internal static bool IsPN_CHARS_U(int codePoint)
            => IsPN_CHARS_BASE(codePoint) || codePoint == '_';

        /// <summary>
        /// Check if the supplied code point represents a valid prefixed name base character
        /// </summary>
        internal static bool IsPN_CHARS_BASE(int codePoint)
            => char.IsLetter((char)codePoint)
                || (codePoint >= 0x00C0 && codePoint <= 0x00D6)
                || (codePoint >= 0x00D8 && codePoint <= 0x00F6)
                || (codePoint >= 0x00F8 && codePoint <= 0x02FF)
                || (codePoint >= 0x0370 && codePoint <= 0x037D)
                || (codePoint >= 0x037F && codePoint <= 0x1FFF)
                || (codePoint >= 0x200C && codePoint <= 0x200D)
                || (codePoint >= 0x2070 && codePoint <= 0x218F)
                || (codePoint >= 0x2C00 && codePoint <= 0x2FEF)
                || (codePoint >= 0x3001 && codePoint <= 0xD7FF)
                || (codePoint >= 0xF900 && codePoint <= 0xFDCF)
                || (codePoint >= 0xFDF0 && codePoint <= 0xFFFD)
                || (codePoint >= 0x10000 && codePoint <= 0xEFFFF);

        /// <summary>
        /// Gets the actual coordinates within Turtle context
        /// </summary>
        internal static string GetTurtleContextCoordinates(RDFStreamReader rdfSBuffer, RDFTurtleContext turtleContext)
            => $"[POSITION:{rdfSBuffer.Position}]";
        #endregion

        #region Write.Graph
        /// <summary>
        /// Writes the given graph into the given stream writer in Turtle format
        /// </summary>
        internal static void WriteTurtleGraph(StreamWriter sw, RDFGraph graph, List<RDFNamespace> prefixes, bool needsTrigIndentation)
        {
            #region linq
            //Group the graph's triples by subject and predicate
            var triplesGroupedBySubjectAndPredicate =
                (from triple in graph
                 orderby triple.Subject.ToString(), triple.Predicate.ToString()
                 group triple by new
                 {
                     subj = triple.Subject.ToString(),
                     pred = triple.Predicate.ToString()
                 }).ToList();
            var lastGroupOfTriples = triplesGroupedBySubjectAndPredicate.LastOrDefault();
            #endregion

            #region triples
            string actualSubject = string.Empty;
            string abbreviatedSubject = string.Empty;
            string actualPredicate = string.Empty;
            string abbreviatedPredicate = string.Empty;
            const string spaceConst = " ";
            StringBuilder result = new StringBuilder();

            //Iterate over the calculated groups
            foreach (var triplesGroup in triplesGroupedBySubjectAndPredicate)
            {
                RDFTriple lastTripleOfGroup = triplesGroup.Last();

                #region subj
                //Reset the flag of subj printing for the new iteration
                bool subjectHasBeenPrinted = false;

                //New subject found
                if (!string.Equals(actualSubject, triplesGroup.Key.subj, StringComparison.Ordinal))
                {
                    //Write the subject's Turtle token
                    if (result.Length > 0)
                    {
                        result.Replace(";", ".", result.Length - (2 + Environment.NewLine.Length), 1);
                        sw.Write(result.ToString());
                        result.Clear();
                    }

                    //Start collecting the new subject's one
                    actualSubject = triplesGroup.Key.subj;
                    actualPredicate = string.Empty;
                    abbreviatedSubject = RDFQueryPrinter.PrintPatternMember(
                        RDFQueryUtilities.ParseRDFPatternMember(actualSubject), prefixes);
                    result.Append(needsTrigIndentation ? string.Concat(spaceConst, spaceConst, abbreviatedSubject, spaceConst)
                                                       : string.Concat(abbreviatedSubject, spaceConst));
                    subjectHasBeenPrinted = true;
                }
                #endregion

                #region predObjList
                //Iterate over the triples of the current group
                foreach (RDFTriple triple in triplesGroup)
                {
                    #region pred
                    //New predicate found
                    if (!string.Equals(actualPredicate, triple.Predicate.ToString(), StringComparison.Ordinal))
                    {
                        //Write the predicate's Turtle token
                        if (!subjectHasBeenPrinted)
                            result.Append(needsTrigIndentation ? spaceConst.PadRight(abbreviatedSubject.Length + 3)
                                                               : spaceConst.PadRight(abbreviatedSubject.Length + 1));
                        actualPredicate = triple.Predicate.ToString();
                        abbreviatedPredicate = RDFQueryPrinter.PrintPatternMember(RDFQueryUtilities.ParseRDFPatternMember(actualPredicate), prefixes);

                        //Turtle goody for "rdf:type" shortcutting to "a"
                        if (string.Equals(abbreviatedPredicate, "rdf:type", StringComparison.Ordinal))
                            abbreviatedPredicate = "a";

                        result.Append(string.Concat(abbreviatedPredicate, spaceConst));
                    }
                    #endregion

                    #region object
                    //Collect the object to the Turtle token
                    if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                    {
                        //Write the object's Turtle token
                        result.Append(RDFQueryPrinter.PrintPatternMember(
                            RDFQueryUtilities.ParseRDFPatternMember(triple.Object.ToString()), prefixes));
                    }
                    #endregion

                    #region literal
                    //Collect the literal to the Turtle token
                    else
                    {
                        //Detect presence of long-literals in order to write proper delimiter
                        string litValDelim = "\"";
                        if (RDFShims.LongLiteralRegex.Value.IsMatch(triple.Object.ToString()))
                            litValDelim = "\"\"\"";

                        //Write the literal's Turtle token
                        switch (triple.Object)
                        {
                            case RDFTypedLiteral tLitObj:
                                string dtype = RDFQueryPrinter.PrintPatternMember(
                                    RDFQueryUtilities.ParseRDFPatternMember(tLitObj.Datatype.URI.ToString()), prefixes);
                                string tLit = $"{litValDelim}{tLitObj.Value.Replace("\\", @"\\")}{litValDelim}^^{dtype}";
                                result.Append(tLit);
                                break;

                            case RDFPlainLiteral pLitObj:
                                string pLit = string.Concat(litValDelim, pLitObj.Value.Replace("\\", @"\\"), litValDelim);
                                if (pLitObj.HasLanguage())
                                    pLit = $"{pLit}@{pLitObj.Language}";
                                result.Append(pLit);
                                break;
                        }
                    }
                    #endregion

                    #region continuation goody
                    //Then append appropriate Turtle continuation goody ("," or ";")
                    if (!triple.Equals(lastTripleOfGroup))
                        result.Append(", ");
                    else
                        result.AppendLine("; ");
                    #endregion
                }
                #endregion

                #region last group
                //This is only for the last group, which is not written into the cycle as the others
                if (triplesGroup.Key.Equals(lastGroupOfTriples.Key))
                {
                    result.Replace(";", ".", result.Length - (2 + Environment.NewLine.Length), 1);
                    sw.Write(result.ToString());
                }
                #endregion
            }
            #endregion
        }
        #endregion

        #endregion

        #endregion
    }
}