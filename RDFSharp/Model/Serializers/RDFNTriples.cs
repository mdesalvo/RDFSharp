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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFNTriples is responsible for managing serialization to and from N-Triples data format.
    /// </summary>
    internal static class RDFNTriples
    {
        #region Properties
        private const string TemplateSPO  = "<{SUBJ}> <{PRED}> <{OBJ}> .";
        private const string TemplateSPLL = "<{SUBJ}> <{PRED}> \"{VAL}\"@{LANG} .";
        private const string TemplateSPLT = "<{SUBJ}> <{PRED}> \"{VAL}\"^^<{DTYPE}> .";

        /// <summary>
        /// Regex to detect S->P->B form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPB = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*_:[^<>\s]+\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->O form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPO = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(PLAIN) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPL_PLAIN = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPL_PLANG = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect S->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> SPL_TLIT = new Lazy<Regex>(() => new Regex(@"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->B form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPB = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*_:[^<>\s]+\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->O form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPO = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(PLAIN) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPL_PLAIN = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPL_PLANG = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPL_TLIT = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect presence of a plain literal with language tag within a given N-Triple
        /// </summary>
        internal static readonly Lazy<Regex> regexLPL = new Lazy<Regex>(() => new Regex(@"@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect presence of starting " in the value of a given N-Triple literal
        /// </summary>
        internal static readonly Lazy<Regex> regexSqt = new Lazy<Regex>(() => new Regex(@"^""", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect presence of ending " in the value of a given N-Triple literal
        /// </summary>
        internal static readonly Lazy<Regex> regexEqt = new Lazy<Regex>(() => new Regex(@"""$", RegexOptions.Compiled));
        #endregion

        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given graph to the given filepath using N-Triples data format.
        /// </summary>
        internal static void Serialize(RDFGraph graph, string filepath)
            => Serialize(graph, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given graph to the given stream using N-Triples data format.
        /// </summary>
        internal static void Serialize(RDFGraph graph, Stream outputStream)
        {
            try
            {
                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, Encoding.ASCII))
                {
                    string tripleTemplate = string.Empty;
                    foreach (RDFTriple t in graph)
                    {
                        #region template
                        tripleTemplate = t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO ? TemplateSPO
                                            : t.Object is RDFPlainLiteral ? TemplateSPLL
                                                : TemplateSPLT;
                        #endregion

                        #region subj
                        tripleTemplate = ((RDFResource)t.Subject).IsBlank ? tripleTemplate.Replace("<{SUBJ}>", RDFModelUtilities.Unicode_To_ASCII(t.Subject.ToString()).Replace("bnode:", "_:"))
                                            : tripleTemplate.Replace("{SUBJ}", RDFModelUtilities.Unicode_To_ASCII(t.Subject.ToString()));
                        #endregion

                        #region pred
                        tripleTemplate = tripleTemplate.Replace("{PRED}", RDFModelUtilities.Unicode_To_ASCII(t.Predicate.ToString()));
                        #endregion

                        #region object
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                            tripleTemplate = ((RDFResource)t.Object).IsBlank ? tripleTemplate.Replace("<{OBJ}>", RDFModelUtilities.Unicode_To_ASCII(t.Object.ToString())).Replace("bnode:", "_:")
                                                : tripleTemplate.Replace("{OBJ}", RDFModelUtilities.Unicode_To_ASCII(t.Object.ToString()));
                        #endregion

                        #region literal
                        else
                        {
                            tripleTemplate = tripleTemplate.Replace("{VAL}", RDFModelUtilities.EscapeControlCharsForXML(RDFModelUtilities.Unicode_To_ASCII(((RDFLiteral)t.Object).Value.Replace("\\", "\\\\").Replace("\"", "\\\""))))
                                                           .Replace("\n", "\\n")
                                                           .Replace("\t", "\\t")
                                                           .Replace("\r", "\\r");

                            #region plain literal
                            if (t.Object is RDFPlainLiteral)
                                tripleTemplate = ((RDFPlainLiteral)t.Object).HasLanguage() ? tripleTemplate.Replace("{LANG}", ((RDFPlainLiteral)t.Object).Language)
                                                    : tripleTemplate.Replace("@{LANG}", string.Empty);
                            #endregion

                            #region typed literal
                            else
                                tripleTemplate = tripleTemplate.Replace("{DTYPE}", RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)t.Object).Datatype));
                            #endregion
                        }
                        #endregion

                        sw.WriteLine(tripleTemplate);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot serialize N-Triples because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given N-Triples filepath to a graph.
        /// </summary>
        internal static RDFGraph Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open), null);

        /// <summary>
        /// Deserializes the given N-Triples stream to a graph.
        /// </summary>
        internal static RDFGraph Deserialize(Stream inputStream, Uri graphContext)
        {
            long ntripleIndex = 0;

            try
            {
                #region deserialize
                using (StreamReader sr = new StreamReader(inputStream, Encoding.ASCII))
                {
                    RDFGraph result = new RDFGraph().SetContext(graphContext);
                    string ntriple = string.Empty;
                    string[] tokens = new string[3];
                    RDFResource S = null;
                    RDFResource P = null;
                    RDFResource O = null;
                    RDFLiteral L = null;
                    Dictionary<string, long> hashContext = new Dictionary<string, long>();
                    char[] openingBrackets = new char[] { '<' };
                    char[] closingBrackets = new char[] { '>' };
                    char[] trimmableChars  = new char[] { ' ', '\t', '\r', '\n' };

                    while ((ntriple = sr.ReadLine()) != null)
                    {
                        ntripleIndex++;

                        #region sanitize  & tokenize
                        //Cleanup previous data
                        S = null; tokens[0] = string.Empty;
                        P = null; tokens[1] = string.Empty;
                        O = null; L = null; tokens[2] = string.Empty;

                        //Preliminary sanitizations: clean trailing space-like chars
                        ntriple = ntriple.Trim(trimmableChars);

                        //Skip empty or comment lines
                        if (ntriple == string.Empty || ntriple.StartsWith("#"))
                            continue;

                        //Tokenizes the sanitized triple
                        tokens = TokenizeNTriple(ntriple);
                        #endregion

                        #region subj
                        string subj = tokens[0].TrimStart(openingBrackets)
                                               .TrimEnd(closingBrackets)
                                               .Replace("_:", "bnode:");
                        string finalSubj = RDFModelUtilities.ASCII_To_Unicode(subj);
                        S = new RDFResource(finalSubj, hashContext);
                        #endregion

                        #region pred
                        string pred = tokens[1].TrimStart(openingBrackets)
                                               .TrimEnd(closingBrackets);
                        string finalPred = RDFModelUtilities.ASCII_To_Unicode(pred);
                        P = new RDFResource(finalPred, hashContext);
                        #endregion

                        #region object
                        if (tokens[2].StartsWith("<")
                                || tokens[2].StartsWith("bnode:")
                                    || tokens[2].StartsWith("_:"))
                        {
                            string obj = tokens[2].TrimStart(openingBrackets)
                                                  .TrimEnd(closingBrackets)
                                                  .Replace("_:", "bnode:")
                                                  .Trim(trimmableChars);
                            string finalObj = RDFModelUtilities.ASCII_To_Unicode(obj);
                            O = new RDFResource(finalObj, hashContext);
                        }
                        #endregion

                        #region literal
                        else
                        {

                            #region sanitize
                            tokens[2] = regexSqt.Value.Replace(tokens[2], string.Empty);
                            tokens[2] = regexEqt.Value.Replace(tokens[2], string.Empty);
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
                                if (regexLPL.Value.Match(tokens[2]).Success)
                                {
                                    tokens[2] = tokens[2].Replace("\"@", "@");
                                    int lastIndexOfLanguage = tokens[2].LastIndexOf("@", StringComparison.OrdinalIgnoreCase);
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
                                int lastIndexOfDatatype = tokens[2].LastIndexOf("^^", StringComparison.OrdinalIgnoreCase);
                                string tLitValue = tokens[2].Substring(0, lastIndexOfDatatype);
                                string tLitDatatype = tokens[2].Substring(lastIndexOfDatatype + 2)
                                                               .TrimStart(new char[] { '<' })
                                                               .TrimEnd(new char[] { '>' });
                                RDFModelEnums.RDFDatatypes dt = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                                L = new RDFTypedLiteral(HttpUtility.HtmlDecode(tLitValue), dt);
                            }
                            #endregion

                        }
                        #endregion

                        #region addtriple
                        if (O != null)
                            result.AddTriple(new RDFTriple(S, P, O));
                        else
                            result.AddTriple(new RDFTriple(S, P, L));
                        #endregion

                    }
                    return result;
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot deserialize N-Triples (line " + ntripleIndex + ") because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Tries to tokenize the given N-Triple
        /// </summary>
        private static string[] TokenizeNTriple(string ntriple)
        {
            //A legal N-Triple starts with "_:" (blank) or "<" (uri)
            if (!ntriple.StartsWith("_:") && !ntriple.StartsWith("<"))
                throw new Exception("found illegal N-Triple, must start with \"_:\" or with \"<\"");

            string[] tokens = new string[3];

            //S->-> triple
            if (ntriple.StartsWith("<"))
            {
                //S->P->O
                if (SPO.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(PLAIN)
                if (SPL_PLAIN.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(PLANG)
                if (SPL_PLANG.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal with language
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->L(TLIT)
                if (SPL_TLIT.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //typed literal
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //S->P->B
                if (SPB.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                throw new Exception("found illegal N-Triple, unrecognized 'S->->' structure");
            }
            //B->-> triple
            else
            {
                //B->P->O
                if (BPO.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(PLAIN)
                if (BPL_PLAIN.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(PLANG)
                if (BPL_PLANG.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //plain literal with language
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->L(TLIT)
                if (BPL_TLIT.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //typed literal
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                //B->P->B
                if (BPB.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim(new char[] { '.', ' ', '\t' });

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(new char[] { ' ', '\t' });
                    tokens[0] = tokens[0].Trim(new char[] { ' ', '\t' });

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(new char[] { ' ', '\t' });
                    tokens[1] = tokens[1].Trim(new char[] { ' ', '\t' });

                    //object
                    tokens[2] = ntriple.Trim(new char[] { ' ', '\t' });

                    return tokens;
                }

                throw new Exception("found illegal N-Triple, unrecognized 'B->->' structure");
            }
        }
        #endregion

        #endregion
    }
}