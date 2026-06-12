/*
   Copyright 2012-2026 Marco De Salvo

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
using System.Text;
using RDFSharp.Model;
using static RDFSharp.Query.RDFQueryLexer;

namespace RDFSharp.Query
{
    /// <summary>
    /// Prologue half of the SPARQL parser: the BASE/PREFIX declarations and the re-attachment of the declared prefixes onto the parsed query.
    /// </summary>
    internal static partial class RDFQueryParser
    {
        #region Prologue
        /// <summary>
        /// <para>
        /// Parses the SPARQL prologue — a (possibly empty) sequence of <c>BASE &lt;iri&gt;</c> and
        /// <c>PREFIX label: &lt;iri&gt;</c> declarations — populating the context's resolver as it goes.
        /// </para>
        /// <para>
        /// Parsing stops at the first token that is neither BASE nor PREFIX (typically the query form
        /// keyword SELECT/ASK/CONSTRUCT/DESCRIBE, or an opening brace). That token is left unconsumed in
        /// the reader so the caller can dispatch on it.
        /// </para>
        /// </summary>
        /// <exception cref="RDFQueryException">When a BASE/PREFIX declaration is malformed.</exception>
        internal static void ParsePrologue(RDFQueryParserContext parserContext)
        {
            while (true)
            {
                //Position the reader on the next significant character
                SkipWhitespace(parserContext);

                //Read the upcoming keyword run; this consumes it, so non-prologue keywords are pushed back below
                string keyword = ReadKeyword(parserContext);

                //SPARQL keywords are case-insensitive: compare ignoring case
                if (keyword.Equals("BASE", StringComparison.OrdinalIgnoreCase))
                {
                    ParseBaseDeclaration(parserContext);
                }
                else if (keyword.Equals("PREFIX", StringComparison.OrdinalIgnoreCase))
                {
                    ParsePrefixDeclaration(parserContext);
                }
                else
                {
                    //Not a prologue keyword: push it back untouched and hand control to the caller
                    UnreadString(parserContext, keyword);
                    return;
                }
            }
        }

        /// <summary>
        /// Parses the body of a <c>BASE &lt;absoluteIri&gt;</c> declaration (the BASE keyword has already been
        /// consumed) and records the base IRI into the resolver. A later BASE overrides an earlier one.
        /// </summary>
        private static void ParseBaseDeclaration(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);
            try
            {
                //Reuse the Turtle IRI reader; resolution of relative IRIs uses whatever base is currently set
                Uri baseIri = RDFTurtle.ParseURI(parserContext.TermParsingContext);
                parserContext.Resolver.SetBaseIri(baseIri.ToString());
            }
            catch (RDFModelException iriParsingException)
            {
                throw new RDFQueryException("Cannot parse SPARQL BASE declaration " + GetCoordinates(parserContext) + ": " + iriParsingException.Message, iriParsingException);
            }
        }

        /// <summary>
        /// Parses the body of a <c>PREFIX label: &lt;namespaceIri&gt;</c> declaration (the PREFIX keyword has
        /// already been consumed) and registers the prefix-to-namespace binding into the resolver. The label
        /// may be empty (the default namespace, declared as <c>PREFIX : &lt;...&gt;</c>).
        /// </summary>
        private static void ParsePrefixDeclaration(RDFQueryParserContext parserContext)
        {
            SkipWhitespace(parserContext);

            //Read the prefix label up to (and consuming) the mandatory ':' terminator. Per the SPARQL grammar
            //(PNAME_NS ::= PN_PREFIX? ':') there is no whitespace between the label and the colon.
            string prefixLabel = ReadPrefixLabel(parserContext);

            SkipWhitespace(parserContext);
            try
            {
                //The namespace is a plain IRI reference
                Uri namespaceIri = RDFTurtle.ParseURI(parserContext.TermParsingContext);
                parserContext.Resolver.RegisterPrefix(prefixLabel, namespaceIri.ToString());
            }
            catch (RDFModelException iriParsingException)
            {
                throw new RDFQueryException("Cannot parse SPARQL PREFIX declaration " + GetCoordinates(parserContext) + ": " + iriParsingException.Message, iriParsingException);
            }
        }

        /// <summary>
        /// Reads a prefix label (the part before the ':' of a PNAME_NS) and consumes the trailing ':'.
        /// Returns the label, which is the empty string for the default namespace declaration <c>PREFIX :</c>.
        /// </summary>
        /// <exception cref="RDFQueryException">When the ':' terminator is missing.</exception>
        private static string ReadPrefixLabel(RDFQueryParserContext parserContext)
        {
            RDFTurtle.RDFTurtleContext termContext = parserContext.TermParsingContext;

            StringBuilder prefixLabel = new StringBuilder();
            int codePoint = RDFTurtle.ReadCodePoint(termContext);
            while (codePoint != ':')
            {
                //A PREFIX label must be terminated by ':' before any whitespace or end of input
                if (codePoint == -1 || RDFTurtle.IsWhitespace(codePoint))
                    throw new RDFQueryException("Cannot parse SPARQL PREFIX declaration: expected ':' to terminate the prefix label " + GetCoordinates(parserContext));

                RDFTurtle.AppendCodePoint(prefixLabel, codePoint);
                codePoint = RDFTurtle.ReadCodePoint(termContext);
            }
            //codePoint is the ':' here, already consumed
            return prefixLabel.ToString();
        }

        /// <summary>
        /// <para>
        /// Re-attaches the PREFIX declarations accumulated by this query's prologue to
        /// <paramref name="selectQuery"/> as <see cref="RDFNamespace"/> objects, so that the parsed query
        /// can re-serialise its prologue section identically to the original input text.
        /// </para>
        /// <para>
        /// Two categories of declared prefixes are silently skipped rather than added:
        /// <list type="bullet">
        /// <item>The <b>default namespace</b> (empty label, declared as <c>PREFIX : &lt;...&gt;</c>):
        ///   <see cref="RDFNamespace"/> forbids an empty prefix label, and the SPARQL printer has no
        ///   mechanism to re-emit it, so it cannot survive the serialisation round-trip and is omitted.</item>
        /// <item>Any prefix whose label or URI is rejected by <see cref="RDFNamespace"/> (e.g. a reserved
        ///   or otherwise invalid label): the prefix still participates in term resolution during parsing
        ///   (it lives in the resolver's internal dictionary) but it is not forwarded to the query object
        ///   model, so it will not appear in the re-serialised prologue.</item>
        /// </list>
        /// </para>
        /// </summary>
        private static void ApplyDeclaredPrefixes(RDFQueryParserContext parserContext, RDFSelectQuery selectQuery)
        {
            foreach (KeyValuePair<string, string> prologuePrefixBinding in parserContext.Resolver.DeclaredPrefixes)
            {
                //Skip the default namespace: RDFNamespace forbids an empty prefix label
                if (prologuePrefixBinding.Key.Length == 0)
                    continue;

                try
                {
                    //Attempt to construct an RDFNamespace and add it as a prefix declaration to the query
                    selectQuery.AddPrefix(new RDFNamespace(prologuePrefixBinding.Key, prologuePrefixBinding.Value));
                }
                catch (RDFModelException)
                {
                    //RDFNamespace rejected this label or URI (e.g. a reserved prefix name): the binding is
                    //already in the resolver's dictionary and will still be used for term resolution, but it
                    //cannot be modelled as an RDFNamespace so we leave it out of the query's prologue list
                }
            }
        }
        #endregion
    }
}
