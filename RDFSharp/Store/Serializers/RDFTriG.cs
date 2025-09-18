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
using System.IO;
using System.Linq;
using System.Text;
using RDFSharp.Model;
using static RDFSharp.Model.RDFTurtle;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFTriG is responsible for managing serialization to and from TriG data format.
    /// </summary>
    internal static class RDFTriG
    {
        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given store to the given filepath using TriG data format.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static void Serialize(RDFStore store, string filepath)
            => Serialize(store, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given store to the given stream using TriG data format.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static void Serialize(RDFStore store, Stream outputStream)
        {
            try
            {
                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    //Extract graphs
                    List<RDFGraph> graphs = store.ExtractGraphs();

                    //Collect namespaces
                    List<RDFNamespace> prefixes = new List<RDFNamespace>();
                    foreach (RDFGraph graph in graphs)
                        prefixes.AddRange(RDFModelUtilities.GetGraphNamespaces(graph));

                    //Write namespaces (avoid duplicates)
                    HashSet<string> printedNamespaces = new HashSet<string>();
                    foreach (RDFNamespace ns in prefixes.OrderBy(n => n.NamespacePrefix))
                    {
                        if (printedNamespaces.Add(ns.NamespacePrefix))
                            sw.WriteLine($"@prefix {ns.NamespacePrefix}: <{ns.NamespaceUri}>.");
                    }
                    if (printedNamespaces.Count > 0)
                        sw.WriteLine();

                    //Write graphs
                    for (int i = 0; i < graphs.Count; i++)
                    {
                        //Opening decorators
                        if (i > 0)
                            sw.WriteLine();
                        if (!graphs[i].Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri))
                            sw.WriteLine($"GRAPH <{graphs[i].Context}>");
                        sw.WriteLine("{");

                        WriteTurtleGraph(sw, graphs[i], prefixes, true);

                        //Closing decorators
                        if (i == graphs.Count - 1)
                            sw.Write("}");
                        else
                            sw.WriteLine("}");
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot serialize TriG because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given TriG filepath to a memory store.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static RDFMemoryStore Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Deserializes the given TriG stream to a memory store.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static RDFMemoryStore Deserialize(Stream inputStream)
        {
            RDFTriGContext trigContext = new RDFTriGContext();

            try
            {
                #region deserialize
                using (StreamReader sReader = new StreamReader(inputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    //Fetch TriG data
                    trigContext.Data = sReader.ReadToEnd();
                    
                    //Parse TriG data
                    int bufferChar = SkipWhitespace(trigContext);
                    while (bufferChar != -1)
                    {
                        ParseStatement(trigContext);
                        //After parsing of a statement we discard spaces and comments
                        //and seek for the next eventual statement, until finally EOL
                        bufferChar = SkipWhitespace(trigContext);
                    }
                }
                RDFNamespaceRegister.RemoveTemporaryNamespaces();
                #endregion deserialize
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot deserialize TriG because: " + ex.Message, ex);
            }

            return trigContext.Store;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Represents the context of the TriG parser
        /// </summary>
        internal sealed class RDFTriGContext : RDFTurtleContext
        {
            /// <summary>
            /// Indicates the current TriG context
            /// </summary>
            internal RDFResource Context { get; set; }

            /// <summary>
            /// Indicates the current TriG graph
            /// </summary>
            internal RDFGraph Graph { get; } = new RDFGraph();

            /// <summary>
            /// Indicates the result TriG store
            /// </summary>
            internal RDFMemoryStore Store { get; } = new RDFMemoryStore();
        }

        /// <summary>
        /// Parses the TriG data in order to detect a valid directive or statement
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static void ParseStatement(RDFTriGContext trigContext)
        {
            StringBuilder sb = new StringBuilder(8);
            do
            {
                int codePoint = ReadCodePoint(trigContext);
                if (codePoint == -1 || IsWhitespace(codePoint))
                {
                    UnreadCodePoint(trigContext, codePoint);
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
                ParseDirective(trigContext, trigContext.Graph, directive);
                SkipWhitespace(trigContext);

                // Turtle @base and @prefix directives MUST end with "."
                if (directive[0] == '@')
                    VerifyCharacterOrFail(trigContext, ReadCodePoint(trigContext), ".");

                // SPARQL BASE and PREFIX directives MUST NOT end with "."
                else if (PeekCodePoint(trigContext) == '.')
                    throw new RDFStoreException("SPARQL directive '" + directive + "' must not end with '.'" + GetTurtleContextCoordinates(trigContext));
            }
            else if (directive.StartsWith("graph:", StringComparison.OrdinalIgnoreCase))
            {
                // If there was a colon immediately after the graph keyword then
                // assume it was a pname and not the SPARQL GRAPH keyword
                UnreadCodePoint(trigContext, directive);
                ParseGraph(trigContext);
            }
            else if (directive.StartsWith("graph", StringComparison.OrdinalIgnoreCase))
            {
                // Do not unread the directive if it was SPARQL GRAPH
                // Just continue with TriG parsing at this point
                SkipWhitespace(trigContext);

                //Clear context in preparation of next graph's parsing
                trigContext.Context = null;

                ParseGraph(trigContext);
                if (trigContext.Context == null)
                    throw new RDFStoreException("Missing GRAPH label or subject");
            }
            else
            {
                UnreadCodePoint(trigContext, directive);
                ParseGraph(trigContext);
            }
        }

        /// <summary>
        /// Parses the TriG data in order to detect a valid graph
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static void ParseGraph(RDFTriGContext trigContext)
        {
            RDFResource contextOrSubject = null;
            bool foundContextOrSubject = false;
            int c = ReadCodePoint(trigContext);
            int c2 = PeekCodePoint(trigContext);

            if (c == '[')
            {
                SkipWhitespace(trigContext);
                c2 = ReadCodePoint(trigContext);
                if (c2 == ']')
                {
                    contextOrSubject = new RDFResource(); //createNode()
                    foundContextOrSubject = true;
                    SkipWhitespace(trigContext);
                }
                else
                {
                    UnreadCodePoint(trigContext, c2);
                    UnreadCodePoint(trigContext, c);
                }
                c = ReadCodePoint(trigContext);
            }
            else if (c == '<' || IsPrefixStartChar(c) || (c == ':' && c2 != '-') || (c == '_' && c2 == ':'))
            {
                UnreadCodePoint(trigContext, c);

                object value = ParseValue(trigContext, trigContext.Graph);

                if (value is Uri || (value is RDFResource resValue && !resValue.IsBlank)) //We don't accept blank contexts
                {
                    contextOrSubject = new RDFResource(value.ToString(), trigContext.HashContext);
                    foundContextOrSubject = true;
                }
                else
                {
                    // NOTE: If a user parses Turtle using TriG, then the following
                    // could actually be "Illegal subject name", but it should still hold
                    throw new RDFStoreException("Illegal (or blank-node) graph name: " + value);
                }

                SkipWhitespace(trigContext);
                c = ReadCodePoint(trigContext);
            }
            else
            {
                trigContext.Context = null;
            }

            if (c == '{')
            {
                trigContext.Context = contextOrSubject;

                c = SkipWhitespace(trigContext);
                if (c != '}')
                {
                    ParseTriples(trigContext);
                    c = SkipWhitespace(trigContext);

                    while (c == '.')
                    {
                        ReadCodePoint(trigContext);
                        c = SkipWhitespace(trigContext);
                        if (c == '}')
                            break;

                        ParseTriples(trigContext);
                        c = SkipWhitespace(trigContext);
                    }
                }

                VerifyCharacterOrFail(trigContext, c, "}");
            }
            else
            {
                trigContext.Context = null;

                // Did not turn out to be a graph, so assign it to subject instead
                // and parse from here to triples
                if (foundContextOrSubject)
                {
                    trigContext.Subject = contextOrSubject;
                    UnreadCodePoint(trigContext, c);
                    ParsePredicateObjectList(trigContext, trigContext.Graph);
                }
                // Or if we didn't recognise anything, just parse as Turtle
                else
                {
                    UnreadCodePoint(trigContext, c);
                    ParseTriples(trigContext);
                }
            }

            //Finalize collection of the current graph into the TriG store
            Uri backupGraphContext = trigContext.Graph.Context;
            trigContext.Graph.SetContext(trigContext.Context?.URI);
            trigContext.Store.MergeGraph(trigContext.Graph);
            trigContext.Graph.SetContext(backupGraphContext);
            trigContext.Graph.ClearTriples();

            ReadCodePoint(trigContext);
        }

        /// <summary>
        /// Parses the TriG data in order to detect a valid statement
        /// </summary>
        internal static void ParseTriples(RDFTriGContext trigContext)
        {
            int bufChar = PeekCodePoint(trigContext);

            // If the first character is an open bracket we need to decide which of
            // the two parsing methods for blank nodes to use
            if (bufChar == '[')
            {
                ReadCodePoint(trigContext);
                SkipWhitespace(trigContext);
                bufChar = PeekCodePoint(trigContext);
                if (bufChar == ']')
                {
                    ReadCodePoint(trigContext);
                    trigContext.Subject = new RDFResource();
                    SkipWhitespace(trigContext);
                    ParsePredicateObjectList(trigContext, trigContext.Graph);
                }
                else
                {
                    //We have to parse an implicit blank, so we must rewind to the
                    //initial '[' character in order for the method to work
                    while (bufChar != '[')
                    {
                        UnreadCodePoint(trigContext, bufChar);
                        bufChar = PeekCodePoint(trigContext);
                    }
                    trigContext.Subject = ParseImplicitBlank(trigContext, trigContext.Graph);
                }
                SkipWhitespace(trigContext);
                bufChar = PeekCodePoint(trigContext);

                // if this is not the end of the statement, recurse into the list of
                // predicate and objects, using the subject parsed above as the subject
                // of the statement.
                if (bufChar != '.' && bufChar != '}')
                    ParsePredicateObjectList(trigContext, trigContext.Graph);
            }
            else
            {
                ParseSubject(trigContext, trigContext.Graph);
                SkipWhitespace(trigContext);
                ParsePredicateObjectList(trigContext, trigContext.Graph);
            }

            trigContext.Subject = null;
            trigContext.Predicate = null;
            trigContext.Object = null;
        }
        #endregion

        #endregion
    }
}