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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using RDFSharp.Model;

namespace RDFSharp.Store {

    /// <summary>
    /// RDFNQuads is responsible for managing serialization to and from N-Quads data format.
    /// </summary>
    internal static class RDFNQuads {

        #region Properties
        internal static readonly Regex SPB        = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*_:[^<>]+\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPO        = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPL_PLAIN  = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPL_PLANG  = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""@[a-zA-Z]+(-[a-zA-Z0-9]+)?\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPL_TLIT   = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""\^\^<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPBC       = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*_:[^<>]+\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPOC       = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*<[^<>]+>\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPLC_PLAIN = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPLC_PLANG = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""@[a-zA-Z]+(-[a-zA-Z0-9]+)?\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex SPLC_TLIT  = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""\^\^<[^<>]+>\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPB        = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*_:[^<>]+\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPO        = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPL_PLAIN  = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPL_PLANG  = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""@[a-zA-Z]+(-[a-zA-Z0-9]+)?\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPL_TLIT   = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""\^\^<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPBC       = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*_:[^<>]+\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPOC       = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*<[^<>]+>\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPLC_PLAIN = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPLC_PLANG = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""@[a-zA-Z]+(-[a-zA-Z0-9]+)?\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        internal static readonly Regex BPLC_TLIT  = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""\^\^<[^<>]+>\s*<[^<>]+>\s*.$", RegexOptions.Compiled);
        #endregion

        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given store to the given filepath using N-Quads data format. 
        /// </summary>
        internal static void Serialize(RDFStore store, String filepath) {
            Serialize(store, new FileStream(filepath, FileMode.Create));
        }

        /// <summary>
        /// Serializes the given store to the given filepath using N-Quads data format. 
        /// </summary>
        internal static void Serialize(RDFStore store, Stream outputStream) {
            try {

                #region serialize
                using (StreamWriter sw            = new StreamWriter(outputStream, Encoding.ASCII)) {
                    String quadrupleTemplate      = String.Empty;
                    foreach (var q               in store.SelectAllQuadruples()) {

                        #region template
                        if (q.TripleFlavor       == RDFModelEnums.RDFTripleFlavors.SPO) {
                            quadrupleTemplate     = "<{SUBJ}> <{PRED}> <{OBJ}> <{CTX}> .";
                        }
                        else {
                            if (q.Object is RDFPlainLiteral) {
                                quadrupleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"@{LANG} <{CTX}> .";
                            }
                            else {
                                quadrupleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"^^<{DTYPE}> <{CTX}> .";
                            }
                        }
                        #endregion

                        #region subj
                        if (((RDFResource)q.Subject).IsBlank) {
                            quadrupleTemplate     = quadrupleTemplate.Replace("<{SUBJ}>", RDFModelUtilities.Unicode_To_ASCII(q.Subject.ToString()).Replace("bnode:", "_:"));
                        }
                        else {
                            quadrupleTemplate     = quadrupleTemplate.Replace("{SUBJ}", RDFModelUtilities.Unicode_To_ASCII(q.Subject.ToString()));
                        }
                        #endregion

                        #region pred
                        quadrupleTemplate         = quadrupleTemplate.Replace("{PRED}", RDFModelUtilities.Unicode_To_ASCII(q.Predicate.ToString()));
                        #endregion

                        #region object
                        if (q.TripleFlavor       == RDFModelEnums.RDFTripleFlavors.SPO) {
                            if (((RDFResource)q.Object).IsBlank) {
                                quadrupleTemplate = quadrupleTemplate.Replace("<{OBJ}>", RDFModelUtilities.Unicode_To_ASCII(q.Object.ToString())).Replace("bnode:", "_:");
                            }
                            else {
                                quadrupleTemplate = quadrupleTemplate.Replace("{OBJ}", RDFModelUtilities.Unicode_To_ASCII(q.Object.ToString()));
                            }
                        }
                        #endregion

                        #region literal
                        else {

                            quadrupleTemplate     = quadrupleTemplate.Replace("{VAL}", RDFModelUtilities.Unicode_To_ASCII(((RDFLiteral)q.Object).Value.Replace("\"", "\\\"")));
                            quadrupleTemplate     = quadrupleTemplate.Replace("\n", "\\n")
                                                                     .Replace("\t", "\\t")
                                                                     .Replace("\r", "\\r");

                            #region plain literal
                            if (q.Object is RDFPlainLiteral) {
                                if (((RDFPlainLiteral)q.Object).Language != String.Empty) {
                                    quadrupleTemplate = quadrupleTemplate.Replace("{LANG}", ((RDFPlainLiteral)q.Object).Language);
                                }
                                else {
                                    quadrupleTemplate = quadrupleTemplate.Replace("@{LANG}", String.Empty);
                                }
                            }
                            #endregion

                            #region typed literal
                            else {
                                quadrupleTemplate = quadrupleTemplate.Replace("{DTYPE}", ((RDFTypedLiteral)q.Object).Datatype.ToString());
                            }
                            #endregion

                        }
                        #endregion

                        #region context
                        quadrupleTemplate         = quadrupleTemplate.Replace("{CTX}", RDFModelUtilities.Unicode_To_ASCII(q.Context.ToString()));
                        #endregion

                        sw.WriteLine(quadrupleTemplate);
                    }
                }
                #endregion

            }
            catch (Exception ex) {
                throw new RDFStoreException("Cannot serialize N-Quads because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given N-Quads filepath to a memory store. 
        /// </summary>
        internal static RDFMemoryStore Deserialize(String filepath) {
            return Deserialize(new FileStream(filepath, FileMode.Open));
        }

        /// <summary>
        /// Deserializes the given N-Quads stream to a memory store. 
        /// </summary>
        internal static RDFMemoryStore Deserialize(Stream inputStream) {
            try {

                #region deserialize
                using (StreamReader sr    = new StreamReader(inputStream)) {
                    RDFMemoryStore result = new RDFMemoryStore();
                    String  nquad         = String.Empty;
                    String[] tokens       = new String[4];
                    RDFResource S         = null;
                    RDFResource P         = null;
                    RDFResource O         = null;
                    RDFLiteral  L         = null;
                    RDFContext  C         = new RDFContext();
                    while((nquad          = sr.ReadLine()) != null) {

                        #region sanitize  & tokenize
                        //Cleanup previous data
                        S                 = null;
                        tokens[0]         = String.Empty;
                        P                 = null;
                        tokens[1]         = String.Empty;
                        O                 = null;
                        L                 = null;
                        tokens[2]         = String.Empty;
                        C                 = new RDFContext();
                        tokens[3]         = String.Empty;

                        //Preliminary sanitizations: clean trailing space-like chars
                        nquad             = nquad.Trim(new Char[] { ' ', '\t', '\r', '\n' });

                        //Skip empty or comment lines
                        if (nquad        == String.Empty || nquad.StartsWith("#")) {
                            continue;
                        }

                        //Tokenizes the sanitized quad 
                        tokens            = TokenizeNQuad(nquad);
                        #endregion

                        #region subj
                        String subj       = tokens[0].TrimStart(new Char[] { '<' })
                                                     .TrimEnd(new   Char[] { '>' })
                                                     .Replace("_:", "bnode:");
                        S                 = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(subj));
                        #endregion

                        #region pred
                        String pred       = tokens[1].TrimStart(new Char[] { '<' })
                                                     .TrimEnd(new   Char[] { '>' });
                        P                 = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(pred));
                        #endregion

                        #region object
                        if (tokens[2].StartsWith("<")      ||
                            tokens[2].StartsWith("bnode:") ||
                            tokens[2].StartsWith("_:")) {
                            String obj    = tokens[2].TrimStart(new Char[] { '<' })
                                                     .TrimEnd(new Char[] { '>' })
                                                     .Replace("_:", "bnode:")
                                                     .Trim(new Char[] { ' ', '\n', '\t', '\r' });
                            O             = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(obj));
                        }
                        #endregion

                        #region literal
                        else {

                            #region sanitize
                            tokens[2]     = RDFNTriples.regexSqt.Replace(tokens[2], String.Empty);
                            tokens[2]     = RDFNTriples.regexEqt.Replace(tokens[2], String.Empty);
                            tokens[2]     = tokens[2].Replace("\\\"", "\"")
                                                  .Replace("\\n", "\n")
                                                  .Replace("\\t", "\t")
                                                  .Replace("\\r", "\r");
                            tokens[2]     = RDFModelUtilities.ASCII_To_Unicode(tokens[2]);
                            #endregion

                            #region plain literal
                            if (!tokens[2].Contains("^^") ||
                                 tokens[2].EndsWith("^^") ||
                                 tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2, 1) != "<") {
                                if (RDFNTriples.regexLPL.Match(tokens[2]).Success) {
                                    tokens[2]        = tokens[2].Replace("\"@", "@");
                                    String pLitValue = tokens[2].Substring(0, tokens[2].LastIndexOf("@", StringComparison.Ordinal));
                                    String pLitLang  = tokens[2].Substring(tokens[2].LastIndexOf("@", StringComparison.Ordinal) + 1);
                                    L     = new RDFPlainLiteral(HttpUtility.HtmlDecode(pLitValue), pLitLang);
                                }
                                else {
                                    L     = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2]));
                                }
                            }
                            #endregion

                            #region typed literal
                            else {
                                tokens[2] = tokens[2].Replace("\"^^", "^^");
                                String tLitValue    = tokens[2].Substring(0, tokens[2].LastIndexOf("^^", StringComparison.Ordinal));
                                String tLitDatatype = tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2)
                                                               .TrimStart(new Char[] { '<' })
                                                               .TrimEnd(new   Char[] { '>' });
                                RDFDatatype dt      = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                                L                   = new RDFTypedLiteral(HttpUtility.HtmlDecode(tLitValue), dt);
                            }
                            #endregion

                        }
                        #endregion

                        #region context
                        if (!String.IsNullOrEmpty(tokens[3])) {
                             String ctx     = tokens[3].TrimStart(new Char[] { '<' })
                                                       .TrimEnd(new   Char[] { '>' });
                            if (Uri.IsWellFormedUriString(ctx, UriKind.Absolute)) {
                                C           = new RDFContext(RDFModelUtilities.ASCII_To_Unicode(ctx));
                            }
                            else {
                                throw new RDFModelException("found context '" + ctx +"' which is not a well-formed absolute Uri");
                            }
                        }
                        #endregion

                        #region addquadruple
                        if (O != null) {
                            result.AddQuadruple(new RDFQuadruple(C, S, P, O));
                        }
                        else {
                            result.AddQuadruple(new RDFQuadruple(C, S, P, L));
                        }
                        #endregion

                    }
                    return result;
                }
                #endregion

            }
            catch(Exception ex) {
                throw new RDFModelException("Cannot deserialize N-Quads because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Tries to tokenize the given N-Quad
        /// </summary>
        internal static String[] TokenizeNQuad(String nquad) {
            String[] tokens        = new String[4];

            //A legal N-Quad starts with "_:" of blanks or "<" of non-blanks
            if (nquad.StartsWith("_:") || nquad.StartsWith("<")) {

                //S->->-> quadruple
                if (nquad.StartsWith("<")) {

                    //S->P->B->C
                    if (SPBC.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->B->
                    if (SPB.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->O->C
                    if (SPOC.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->O->
                    if (SPO.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(PLAIN)->C
                    if (SPLC_PLAIN.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal
                        tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(PLAIN)->
                    if (SPL_PLAIN.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(PLANG)->C
                    if (SPLC_PLANG.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal with language
                        tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(PLANG)->
                    if (SPL_PLANG.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal with language
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(TLIT)->C
                    if (SPLC_TLIT.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t', '>' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //typed literal
                        tokens[2] = nquad.Substring(0, nquad.LastIndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(TLIT)->
                    if (SPL_TLIT.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //typed literal
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    throw new Exception("found illegal N-Quad, unrecognized 'S->->->' structure");
                }

                //B->->-> quadruple
                else {

                    //B->P->B->C
                    if (BPBC.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->B->
                    if (BPB.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->O->C
                    if (BPOC.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->O->
                    if (BPO.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(PLAIN)->C
                    if (BPLC_PLAIN.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal
                        tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(PLAIN)->
                    if (BPL_PLAIN.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(PLANG)->C
                    if (BPLC_PLANG.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal with language
                        tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(PLANG)->
                    if (BPL_PLANG.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal with language
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(TLIT)->C
                    if (BPLC_TLIT.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t', '>' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //typed literal
                        tokens[2] = nquad.Substring(0, nquad.LastIndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[2].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[2] = tokens[2].Trim(new Char[] { ' ', '\t' });

                        //context
                        tokens[3] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(TLIT)->
                    if (BPL_TLIT.Match(nquad).Success) {
                        nquad     = nquad.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                        nquad     = nquad.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                        nquad     = nquad.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //typed literal
                        tokens[2] = nquad.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    throw new Exception("found illegal N-Quad, unrecognized 'B->->->' structure");
                }

            }
            else {
                throw new Exception("found illegal N-Quad, must start with \"_:\" or with \"<\"");
            }

        }
        #endregion

        #endregion

    }

}