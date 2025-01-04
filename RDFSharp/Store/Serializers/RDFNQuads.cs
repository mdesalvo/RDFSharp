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

using RDFSharp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

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
        internal static readonly Lazy<Regex> SPLC_PLANG = new Lazy<Regex>(() => new Regex(string.Concat(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""@", RDFPlainLiteral.LangTagMask, @"\s*<[^<>\s]+>\s*\.$"), RegexOptions.Compiled));

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
        internal static readonly Lazy<Regex> BPLC_PLANG = new Lazy<Regex>(() => new Regex(string.Concat(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""@", RDFPlainLiteral.LangTagMask, @"\s*<[^<>\s]+>\s*\.$"), RegexOptions.Compiled));

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
        internal static void Serialize(RDFStore store, string filepath)
            => Serialize(store, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given store to the given filepath using N-Quads data format.
        /// </summary>
        internal static void Serialize(RDFStore store, Stream outputStream)
        {
            try
            {
                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, Encoding.ASCII))
                {
                    string quadrupleTemplate = string.Empty;
                    foreach (RDFQuadruple q in store.SelectAllQuadruples())
                    {
                        #region template
                        if (q.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            quadrupleTemplate = "<{SUBJ}> <{PRED}> <{OBJ}> <{CTX}> .";
                        else
                        {
                            if (q.Object is RDFPlainLiteral)
                                quadrupleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"@{LANG} <{CTX}> .";
                            else
                                quadrupleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"^^<{DTYPE}> <{CTX}> .";
                        }
                        #endregion

                        #region subj
                        if (((RDFResource)q.Subject).IsBlank)
                            quadrupleTemplate = quadrupleTemplate.Replace("<{SUBJ}>", RDFModelUtilities.Unicode_To_ASCII(q.Subject.ToString()).Replace("bnode:", "_:"));
                        else
                            quadrupleTemplate = quadrupleTemplate.Replace("{SUBJ}", RDFModelUtilities.Unicode_To_ASCII(q.Subject.ToString()));
                        #endregion

                        #region pred
                        quadrupleTemplate = quadrupleTemplate.Replace("{PRED}", RDFModelUtilities.Unicode_To_ASCII(q.Predicate.ToString()));
                        #endregion

                        #region object
                        if (q.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                        {
                            if (((RDFResource)q.Object).IsBlank)
                                quadrupleTemplate = quadrupleTemplate.Replace("<{OBJ}>", RDFModelUtilities.Unicode_To_ASCII(q.Object.ToString())).Replace("bnode:", "_:");
                            else
                                quadrupleTemplate = quadrupleTemplate.Replace("{OBJ}", RDFModelUtilities.Unicode_To_ASCII(q.Object.ToString()));
                        }
                        #endregion

                        #region literal
                        else
                        {
                            quadrupleTemplate = quadrupleTemplate.Replace("{VAL}", RDFModelUtilities.EscapeControlCharsForXML(RDFModelUtilities.Unicode_To_ASCII(((RDFLiteral)q.Object).Value.Replace("\\", "\\\\").Replace("\"", "\\\""))));
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
        internal static RDFMemoryStore Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Deserializes the given N-Quads stream to a memory store.
        /// </summary>
        internal static RDFMemoryStore Deserialize(Stream inputStream)
        {
            long nquadIndex = 0;
            try
            {
                #region deserialize
                using (StreamReader sr = new StreamReader(inputStream, Encoding.ASCII))
                {
                    RDFMemoryStore result = new RDFMemoryStore();
                    string nquad = string.Empty;
                    string[] tokens = new string[4];
                    RDFResource S = null;
                    RDFResource P = null;
                    RDFResource O = null;
                    RDFLiteral L = null;
                    RDFContext C = new RDFContext();
                    Dictionary<string, long> hashContext = new Dictionary<string, long>();
                    while ((nquad = sr.ReadLine()) != null)
                    {
                        nquadIndex++;

                        #region sanitize  & tokenize
                        //Cleanup previous data
                        S = null;
                        tokens[0] = string.Empty;
                        P = null;
                        tokens[1] = string.Empty;
                        O = null;
                        L = null;
                        tokens[2] = string.Empty;
                        C = new RDFContext();
                        tokens[3] = string.Empty;

                        //Preliminary sanitizations: clean trailing space-like chars
                        nquad = nquad.Trim(new char[] { ' ', '\t', '\r', '\n' });

                        //Skip empty or comment lines
                        if (nquad == string.Empty || nquad.StartsWith("#"))
                            continue;

                        //Tokenizes the sanitized quad
                        tokens = TokenizeNQuad(nquad);
                        #endregion

                        #region subj
                        string subj = tokens[0].TrimStart(new char[] { '<' })
                                               .TrimEnd(new char[] { '>' })
                                               .Replace("_:", "bnode:");
                        S = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(subj), hashContext);
                        #endregion

                        #region pred
                        string pred = tokens[1].TrimStart(new char[] { '<' })
                                               .TrimEnd(new char[] { '>' });
                        P = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(pred), hashContext);
                        #endregion

                        #region object
                        if (tokens[2].StartsWith("<") ||
                            tokens[2].StartsWith("bnode:") ||
                            tokens[2].StartsWith("_:"))
                        {
                            string obj = tokens[2].TrimStart(new char[] { '<' })
                                                  .TrimEnd(new char[] { '>' })
                                                  .Replace("_:", "bnode:")
                                                  .Trim(new char[] { ' ', '\n', '\t', '\r' });
                            O = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(obj), hashContext);
                        }
                        #endregion

                        #region literal
                        else
                        {
                            #region sanitize
                            tokens[2] = RDFNTriples.regexSqt.Value.Replace(tokens[2], string.Empty);
                            tokens[2] = RDFNTriples.regexEqt.Value.Replace(tokens[2], string.Empty);
                            tokens[2] = tokens[2].Replace("\\\\", "\\")
                                                 .Replace("\\\"", "\"")
                                                 .Replace("\\n", "\n")
                                                 .Replace("\\t", "\t")
                                                 .Replace("\\r", "\r");
                            tokens[2] = RDFModelUtilities.ASCII_To_Unicode(tokens[2]);
                            #endregion

                            #region plain literal
                            if (!tokens[2].Contains("^^")
                                  || tokens[2].EndsWith("^^")
                                  || tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2, 1) != "<")
                            {
                                if (RDFNTriples.regexLPL.Value.Match(tokens[2]).Success)
                                {
                                    tokens[2] = tokens[2].Replace("\"@", "@");
                                    int lastIndexOfLanguage = tokens[2].LastIndexOf("@", StringComparison.OrdinalIgnoreCase);
                                    string pLitValue = tokens[2].Substring(0, lastIndexOfLanguage);
                                    string pLitLang = tokens[2].Substring(lastIndexOfLanguage + 1);
                                    L = new RDFPlainLiteral(HttpUtility.HtmlDecode(pLitValue), pLitLang);
                                }
                                else
                                    L = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2]));
                            }
                            #endregion

                            #region typed literal
                            else
                            {
                                tokens[2] = tokens[2].Replace("\"^^", "^^");
                                int lastIndexOfDatatype = tokens[2].LastIndexOf("^^", StringComparison.OrdinalIgnoreCase);
                                string tLitValue = tokens[2].Substring(0, lastIndexOfDatatype);
                                string tLitDatatype = tokens[2].Substring(lastIndexOfDatatype + 2)
                                                               .TrimStart(new char[] { '<' })
                                                               .TrimEnd(new char[] { '>' });
                                L = new RDFTypedLiteral(HttpUtility.HtmlDecode(tLitValue), RDFDatatypeRegister.GetDatatype(tLitDatatype));
                            }
                            #endregion
                        }
                        #endregion

                        #region context
                        if (!string.IsNullOrEmpty(tokens[3]))
                        {
                            string ctx = tokens[3].TrimStart(new char[] { '<' })
                                                  .TrimEnd(new char[] { '>' });
                            C = new RDFContext(RDFModelUtilities.ASCII_To_Unicode(ctx));
                        }
                        #endregion

                        #region addquadruple
                        if (O != null)
                            result.AddQuadruple(new RDFQuadruple(C, S, P, O));
                        else
                            result.AddQuadruple(new RDFQuadruple(C, S, P, L));
                        #endregion

                    }
                    return result;
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot deserialize N-Quads (line " + nquadIndex + ") because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Tries to tokenize the given N-Quad
        /// </summary>
        private static string[] TokenizeNQuad(string nquad)
        {
            //A legal N-Quad starts with "_:" of blanks or "<" of non-blanks
            if (!nquad.StartsWith("_:") && !nquad.StartsWith("<"))
                throw new Exception("found illegal N-Quad, must start with \"_:\" or with \"<\"");

            string[] tokens = new string[4];

            //S->->-> quadruple
            if (nquad.StartsWith("<"))
            {
                //S->P->O->C
                if (SPOC.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->O->
                if (RDFNTriples.SPO.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(PLAIN)->C
                if (SPLC_PLAIN.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(PLAIN)->
                if (RDFNTriples.SPL_PLAIN.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(PLANG)->C
                if (SPLC_PLANG.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal with language
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(PLANG)->
                if (RDFNTriples.SPL_PLANG.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal with language
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(TLIT)->C
                if (SPLC_TLIT.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t', '>' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //typed literal
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(TLIT)->
                if (RDFNTriples.SPL_TLIT.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //typed literal
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->B->C
                if (SPBC.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->B->
                if (RDFNTriples.SPB.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                throw new Exception("found illegal N-Quad, unrecognized 'S->->->' structure");
            }
            //B->->-> quadruple
            else
            {
                //B->P->O->C
                if (BPOC.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->O->
                if (RDFNTriples.BPO.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(PLAIN)->C
                if (BPLC_PLAIN.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(PLAIN)->
                if (RDFNTriples.BPL_PLAIN.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(PLANG)->C
                if (BPLC_PLANG.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal with language
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(PLANG)->
                if (RDFNTriples.BPL_PLANG.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal with language
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(TLIT)->C
                if (BPLC_TLIT.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t', '>' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //typed literal
                    tokens[2] = nquad.Substring(0, nquad.LastIndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(TLIT)->
                if (RDFNTriples.BPL_TLIT.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //typed literal
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->B->C
                if (BPBC.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[2].Length).Trim(new char[] { ' ', '\t' });
                    tokens[2] = tokens[2].Trim(new char[] { ' ', '\t' });

                    //context
                    tokens[3] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->B->
                if (RDFNTriples.BPB.Value.Match(nquad).Success)
                {
                    nquad = nquad.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = nquad.Substring(0, nquad.IndexOf('<'));
                    nquad = nquad.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = nquad.Substring(0, nquad.IndexOf('>') + 1);
                    nquad = nquad.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = nquad.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                throw new Exception("found illegal N-Quad, unrecognized 'B->->->' structure");
            }
        }
        #endregion

        #endregion
    }
}