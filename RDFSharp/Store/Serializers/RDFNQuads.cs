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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using RDFSharp.Model;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFNQuads is responsible for managing serialization to and from N-Quads data format.
    /// </summary>
    internal static class RDFNQuads
    {
        #region Properties
        /// <summary>
        /// Regex to detect S->P->B->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPBC = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*_:[^<>\s]+\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->O->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPOC = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*<[^<>\s]+>\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(PLAIN)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPLC_PLAIN = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(PLAIN LANGUAGE)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPLC_PLANG = new Lazy<Regex>(() => new Regex($@"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""@{RDFShims.LangTagMask}\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->B->L(TYPED) form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPLC_TLIT = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->B->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPBC = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*_:[^<>\s]+\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->O->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPOC = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*<[^<>\s]+>\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(PLAIN)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPLC_PLAIN = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(PLAIN LANGUAGE)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPLC_PLANG = new Lazy<Regex>(() => new Regex($@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""@{RDFShims.LangTagMask}\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(TYPED)->C form of N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPLC_TLIT = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));
        #endregion

        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given store to the given filepath using N-Quads data format.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static void Serialize(RDFStore store, string filepath)
            => Serialize(store, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given store to the given filepath using N-Quads data format.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static void Serialize(RDFStore store, Stream outputStream)
        {
            try
            {
                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, Encoding.ASCII))
                {
                    foreach (RDFQuadruple q in store.SelectAllQuadruples())
                    {
                        #region template
                        string quadrupleTemplate;
                        if (q.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            quadrupleTemplate = "<{SUBJ}> <{PRED}> <{OBJ}> <{CTX}> .";
                        }
                        else
                        {
                            quadrupleTemplate = q.Object is RDFPlainLiteral ? "<{SUBJ}> <{PRED}> \"{VAL}\"@{LANG} <{CTX}> ."
                                                        : "<{SUBJ}> <{PRED}> \"{VAL}\"^^<{DTYPE}> <{CTX}> .";
                        }
                        #endregion

                        #region subj
                        quadrupleTemplate = ((RDFResource)q.Subject).IsBlank ? quadrupleTemplate.Replace("<{SUBJ}>", RDFModelUtilities.Unicode_To_ASCII(q.Subject.ToString()).Replace("bnode:", "_:"))
                                                                             : quadrupleTemplate.Replace("{SUBJ}", RDFModelUtilities.Unicode_To_ASCII(q.Subject.ToString()));
                        #endregion

                        #region pred
                        quadrupleTemplate = quadrupleTemplate.Replace("{PRED}", RDFModelUtilities.Unicode_To_ASCII(q.Predicate.ToString()));
                        #endregion

                        #region object
                        if (q.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            quadrupleTemplate = ((RDFResource)q.Object).IsBlank ? quadrupleTemplate.Replace("<{OBJ}>", RDFModelUtilities.Unicode_To_ASCII(q.Object.ToString())).Replace("bnode:", "_:")
                                                                                : quadrupleTemplate.Replace("{OBJ}", RDFModelUtilities.Unicode_To_ASCII(q.Object.ToString()));
                        }
                        #endregion

                        #region literal
                        else
                        {
                            quadrupleTemplate = quadrupleTemplate.Replace("{VAL}", RDFModelUtilities.EscapeControlCharsForXML(RDFModelUtilities.Unicode_To_ASCII(((RDFLiteral)q.Object).Value.Replace("\\", @"\\").Replace("\"", "\\\""))));
                            quadrupleTemplate = quadrupleTemplate.Replace("\n", "\\n")
                                                                 .Replace("\t", "\\t")
                                                                 .Replace("\r", "\\r");

                            #region plain literal
                            if (q.Object is RDFPlainLiteral plitObj)
                                quadrupleTemplate = plitObj.HasLanguage() ? quadrupleTemplate.Replace("{LANG}", plitObj.Language)
                                                                          : quadrupleTemplate.Replace("@{LANG}", string.Empty);
                            #endregion

                            #region typed literal
                            else
                                quadrupleTemplate = quadrupleTemplate.Replace("{DTYPE}", ((RDFTypedLiteral)q.Object).Datatype.URI.ToString());
                            #endregion
                        }
                        #endregion

                        #region context
                        quadrupleTemplate = quadrupleTemplate.Replace("{CTX}", RDFModelUtilities.Unicode_To_ASCII(q.Context.ToString()));
                        #endregion

                        sw.WriteLine(quadrupleTemplate);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot serialize N-Quads because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given N-Quads filepath to a memory store.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static RDFMemoryStore Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Deserializes the given N-Quads stream to a memory store.
        /// </summary>
        /// <exception cref="RDFStoreException"></exception>
        internal static RDFMemoryStore Deserialize(Stream inputStream)
        {
            RDFMemoryStore result = new RDFMemoryStore();
            long nquadIndex = 0;
            string nquad;
            string[] tokens = new string[4];
            Dictionary<string, long> hashContext = new Dictionary<string, long>();
            RDFContext defaultContext = new RDFContext();

            try
            {
                #region deserialize
                using (StreamReader sr = new StreamReader(inputStream, Encoding.ASCII))
                {
                    while ((nquad = sr.ReadLine()) != null)
                    {
                        nquadIndex++;

                        #region sanitize  & tokenize
                        //Cleanup data
                        RDFContext C = defaultContext;
                        RDFResource S = null;
                        RDFResource P = null;
                        RDFResource O = null;
                        RDFLiteral L = null;
                        tokens[0] = string.Empty;
                        tokens[1] = string.Empty;
                        tokens[2] = string.Empty;
                        tokens[3] = string.Empty;

                        //Preliminary sanitizations: clean trailing space-like chars
                        nquad = nquad.Trim(RDFNTriples.trimmableChars);

                        //Skip empty or comment lines
                        if (nquad.Length == 0 || nquad[0] == '#')
                            continue;

                        //Tokenizes the sanitized quad
                        TokenizeNQuad(nquad, ref tokens);
                        #endregion

                        #region subj
                        string subj = tokens[0].TrimStart('<')
                                               .TrimEnd('>')
                                               .Replace("_:", "bnode:");
                        S = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(subj), hashContext);
                        #endregion

                        #region pred
                        string pred = tokens[1].TrimStart('<')
                                               .TrimEnd('>');
                        P = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(pred), hashContext);
                        #endregion

                        #region object
                        if (tokens[2][0] == '<'
                             || tokens[2].StartsWith("bnode:", StringComparison.OrdinalIgnoreCase)
                             || tokens[2].StartsWith("_:", StringComparison.Ordinal))
                        {
                            string obj = tokens[2].TrimStart('<')
                                                  .TrimEnd('>')
                                                  .Replace("_:", "bnode:")
                                                  .Trim(' ', '\n', '\t', '\r');
                            O = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(obj), hashContext);
                        }
                        #endregion

                        #region literal
                        else
                        {
                            #region sanitize
                            tokens[2] = RDFNTriples.regexSqt.Value.Replace(tokens[2], string.Empty);
                            tokens[2] = RDFNTriples.regexEqt.Value.Replace(tokens[2], string.Empty);
                            tokens[2] = tokens[2].Replace(@"\\", "\\")
                                                 .Replace("\\\"", "\"")
                                                 .Replace("\\n", "\n")
                                                 .Replace("\\t", "\t")
                                                 .Replace("\\r", "\r")
                                                 .Replace("\"^^", "^^");
                            tokens[2] = RDFModelUtilities.ASCII_To_Unicode(tokens[2]);
                            #endregion

                            #region plain literal
                            //Detect presence of semantically valid datatype indicator ("^^")
                            int lastIndexOfDatatype = tokens[2].LastIndexOf("^^", StringComparison.Ordinal);
                            if (lastIndexOfDatatype == -1
                                 || lastIndexOfDatatype == tokens[2].Length - 2 //EndsWith "^^"
                                 || tokens[2][lastIndexOfDatatype + 2] != '<')
                            {
                                if (RDFNTriples.regexLPL.Value.Match(tokens[2]).Success)
                                {
                                    tokens[2] = tokens[2].Replace("\"@", "@");
                                    int lastIndexOfLanguage = tokens[2].LastIndexOf('@');
                                    string pLitValue = tokens[2].Substring(0, lastIndexOfLanguage);
                                    string pLitLang = tokens[2].Substring(lastIndexOfLanguage + 1);
                                    L = new RDFPlainLiteral(HttpUtility.HtmlDecode(pLitValue), pLitLang);
                                }
                                else
                                {
                                    L = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2]));
                                }
                            }
                            #endregion

                            #region typed literal
                            else
                            {
                                tokens[2] = tokens[2].Replace("\"^^", "^^");
                                string tLitValue = tokens[2].Substring(0, lastIndexOfDatatype);
                                string tLitDatatype = tokens[2].Substring(lastIndexOfDatatype + 2)
                                                               .TrimStart('<')
                                                               .TrimEnd('>');
                                L = new RDFTypedLiteral(HttpUtility.HtmlDecode(tLitValue), RDFDatatypeRegister.GetDatatype(tLitDatatype));
                            }
                            #endregion
                        }
                        #endregion

                        #region context
                        if (!string.IsNullOrEmpty(tokens[3]))
                        {
                            string ctx = tokens[3].TrimStart('<')
                                                  .TrimEnd('>');
                            C = new RDFContext(RDFModelUtilities.ASCII_To_Unicode(ctx));
                        }
                        #endregion

                        result.AddQuadruple(O != null ? new RDFQuadruple(C, S, P, O) : new RDFQuadruple(C, S, P, L));
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot deserialize N-Quads (line " + nquadIndex + ") because: " + ex.Message, ex);
            }

            return result;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Tries to tokenize the given N-Quad
        /// </summary>
        /// <exception cref="Exception"></exception>
        private static void TokenizeNQuad(string nquad, ref string[] tokens)
        {
            //S->->-> quadruple
            if (nquad[0] == '<')
            {
                //S->P->O->C
                if (SPOC.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->O->
                if (RDFNTriples.SPO.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->L(PLAIN)->C
                if (SPLC_PLAIN.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->L(PLAIN)->
                if (RDFNTriples.SPL_PLAIN.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->L(PLANG)->C
                if (SPLC_PLANG.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal with language
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->L(PLANG)->
                if (RDFNTriples.SPL_PLANG.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal with language
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->L(TLIT)->C
                if (SPLC_TLIT.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t', '>');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //typed literal
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->L(TLIT)->
                if (RDFNTriples.SPL_TLIT.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //typed literal
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->B->C
                if (SPBC.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //S->P->B->
                if (RDFNTriples.SPB.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                throw new Exception("found illegal N-Quad, unrecognized 'S->->->' structure");
            }

            //B->->-> quadruple
            if (nquad.Length > 1 && nquad[0] == '_' && nquad[1] == ':')
            {
                //B->P->O->C
                if (BPOC.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->O->
                if (RDFNTriples.BPO.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->L(PLAIN)->C
                if (BPLC_PLAIN.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->L(PLAIN)->
                if (RDFNTriples.BPL_PLAIN.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->L(PLANG)->C
                if (BPLC_PLANG.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal with language
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->L(PLANG)->
                if (RDFNTriples.BPL_PLANG.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal with language
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->L(TLIT)->C
                if (BPLC_TLIT.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t', '>');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //typed literal
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->L(TLIT)->
                if (RDFNTriples.BPL_TLIT.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //typed literal
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->B->C
                if (BPBC.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(' ', '\t');
                    tokens[2] = tokens[2].Trim(' ', '\t');

                    //context
                    tokens[3] = nquad.Trim(' ', '\t');
                    return;
                }

                //B->P->B->
                if (RDFNTriples.BPB.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = nquad.Trim(' ', '\t');
                    return;
                }

                throw new Exception("found illegal N-Quad, unrecognized 'B->->->' structure");
            }

            //A legal N-Quad starts with "_:" (blank) or "<" (uri)
            throw new Exception("found illegal N-Quad, must start with \"_:\" or with \"<\"");
        }
        #endregion

        #endregion
    }
}