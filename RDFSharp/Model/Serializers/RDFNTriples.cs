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
        internal static readonly Lazy<Regex> SPL_PLANG = new Lazy<Regex>(() => new Regex($@"^<[^<>\s]+>\s*<[^<>\s]+>\s*\""(.)*\""@{RDFPlainLiteral.LangTagMask}\s*\.$", RegexOptions.Compiled));

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
        internal static readonly Lazy<Regex> BPL_PLANG = new Lazy<Regex>(() => new Regex($@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""@{RDFPlainLiteral.LangTagMask}\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect B->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Lazy<Regex> BPL_TLIT = new Lazy<Regex>(() => new Regex(@"^_:[^<>\s]+\s*<[^<>\s]+>\s*\""(.)*\""\^\^<[^<>\s]+>\s*\.$", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect presence of a plain literal with language tag within a given N-Triple
        /// </summary>
        internal static readonly Lazy<Regex> regexLPL = new Lazy<Regex>(() => new Regex($"@{RDFPlainLiteral.LangTagMask}$", RegexOptions.Compiled | RegexOptions.IgnoreCase));

        /// <summary>
        /// Regex to detect presence of starting " in the value of a given N-Triple literal
        /// </summary>
        internal static readonly Lazy<Regex> regexSqt = new Lazy<Regex>(() => new Regex(@"^""", RegexOptions.Compiled));

        /// <summary>
        /// Regex to detect presence of ending " in the value of a given N-Triple literal
        /// </summary>
        internal static readonly Lazy<Regex> regexEqt = new Lazy<Regex>(() => new Regex(@"""$", RegexOptions.Compiled));

        // Facilities for deserialization
        internal static readonly char[] openingBrackets = { '<' };
        internal static readonly char[] closingBrackets = { '>' };
        internal static readonly char[] trimmableChars = { ' ', '\t', '\r', '\n' };
        #endregion

        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given graph to the given filepath using N-Triples data format.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static void Serialize(RDFGraph graph, string filepath)
            => Serialize(graph, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given graph to the given stream using N-Triples data format.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static void Serialize(RDFGraph graph, Stream outputStream)
        {
            try
            {
                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, Encoding.ASCII))
                {
                    foreach (RDFTriple t in graph)
                    {
                        #region template
                        string tripleTemplate = t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO
                            ? TemplateSPO
                            : t.Object is RDFPlainLiteral ? TemplateSPLL : TemplateSPLT;
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
                            tripleTemplate = ((RDFResource)t.Object).IsBlank
                                                ? tripleTemplate.Replace("<{OBJ}>", RDFModelUtilities.Unicode_To_ASCII(t.Object.ToString())).Replace("bnode:", "_:")
                                                : tripleTemplate.Replace("{OBJ}", RDFModelUtilities.Unicode_To_ASCII(t.Object.ToString()));
                        #endregion

                        #region literal
                        else
                        {
                            tripleTemplate = tripleTemplate.Replace("{VAL}", RDFModelUtilities.EscapeControlCharsForXML(RDFModelUtilities.Unicode_To_ASCII(((RDFLiteral)t.Object).Value.Replace("\\", @"\\").Replace("\"", "\\\""))))
                                                           .Replace("\n", "\\n")
                                                           .Replace("\t", "\\t")
                                                           .Replace("\r", "\\r");

                            #region plain literal
                            if (t.Object is RDFPlainLiteral plitObj)
                                tripleTemplate = plitObj.HasLanguage() ? tripleTemplate.Replace("{LANG}", plitObj.Language)
                                                                       : tripleTemplate.Replace("@{LANG}", string.Empty);
                            #endregion

                            #region typed literal
                            else
                                tripleTemplate = tripleTemplate.Replace("{DTYPE}", ((RDFTypedLiteral)t.Object).Datatype.URI.ToString());
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
        /// <exception cref="RDFModelException"></exception>
        internal static RDFGraph Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open), null);

        /// <summary>
        /// Deserializes the given N-Triples stream to a graph.
        /// </summary>
        /// <exception cref="RDFModelException"></exception>
        internal static RDFGraph Deserialize(Stream inputStream, Uri graphContext)
        {
            long ntripleIndex = 0;
            try
            {
                #region deserialize
                using (StreamReader sr = new StreamReader(inputStream, Encoding.ASCII))
                {
                    RDFGraph result = new RDFGraph().SetContext(graphContext);
                    string nTriple;
                    string[] tokens = new string[3];
                    Dictionary<string, long> hashContext = new Dictionary<string, long>(128);

                    while ((nTriple = sr.ReadLine()) != null)
                    {
                        ntripleIndex++;

                        #region sanitize  & tokenize
                        //Cleanup data
                        RDFResource S = null;
                        RDFResource P = null;
                        RDFResource O = null;
                        RDFLiteral L = null;
                        tokens[0] = string.Empty;
                        tokens[1] = string.Empty;
                        tokens[2] = string.Empty;

                        //Preliminary sanitizations: clean trailing space-like chars
                        nTriple = nTriple.Trim(trimmableChars);

                        //Skip empty or comment lines
                        if (nTriple.Length == 0 || nTriple[0] == '#')
                            continue;

                        //Tokenizes the sanitized triple
                        TokenizeNTriple(nTriple, ref tokens);
                        #endregion

                        #region subj
                        string subj = tokens[0].TrimStart(openingBrackets)
                                               .TrimEnd(closingBrackets)
                                               .Replace("_:", "bnode:");
                        S = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(subj), hashContext);
                        #endregion

                        #region pred
                        string pred = tokens[1].TrimStart(openingBrackets)
                                               .TrimEnd(closingBrackets);
                        P = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(pred), hashContext);
                        #endregion

                        #region object
                        if (tokens[2].StartsWith("<", StringComparison.Ordinal)
                             || tokens[2].StartsWith("bnode:", StringComparison.OrdinalIgnoreCase)
                             || tokens[2].StartsWith("_:", StringComparison.Ordinal))
                        {
                            string obj = tokens[2].TrimStart(openingBrackets)
                                                  .TrimEnd(closingBrackets)
                                                  .Replace("_:", "bnode:")
                                                  .Trim(trimmableChars);
                            O = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(obj), hashContext);
                        }
                        #endregion

                        #region literal
                        else
                        {
                            #region sanitize
                            tokens[2] = regexSqt.Value.Replace(tokens[2], string.Empty);
                            tokens[2] = regexEqt.Value.Replace(tokens[2], string.Empty);
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
                                if (regexLPL.Value.Match(tokens[2]).Success)
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

                        result.AddTriple(O != null ? new RDFTriple(S, P, O) : new RDFTriple(S, P, L));
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
        /// <exception cref="Exception"></exception>
        private static void TokenizeNTriple(string ntriple, ref string[] tokens)
        {
            //S->-> triple
            if (ntriple.StartsWith("<", StringComparison.Ordinal))
            {
                //S->P->O
                if (SPO.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                //S->P->L(PLAIN)
                if (SPL_PLAIN.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                //S->P->L(PLANG)
                if (SPL_PLANG.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal with language
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                //S->P->L(TLIT)
                if (SPL_TLIT.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //typed literal
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                //S->P->B
                if (SPB.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                throw new Exception("found illegal N-Triple, unrecognized 'S->->' structure");
            }

            //B->-> triple
            if (ntriple.StartsWith("_:", StringComparison.Ordinal))
            {
                //B->P->O
                if (BPO.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                //B->P->L(PLAIN)
                if (BPL_PLAIN.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                //B->P->L(PLANG)
                if (BPL_PLANG.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //plain literal with language
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                //B->P->L(TLIT)
                if (BPL_TLIT.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //typed literal
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                }

                //B->P->B
                if (BPB.Value.Match(ntriple).Success)
                {
                    ntriple = ntriple.Trim('.', ' ', '\t');

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(' ', '\t');
                    tokens[0] = tokens[0].Trim(' ', '\t');

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(' ', '\t');
                    tokens[1] = tokens[1].Trim(' ', '\t');

                    //object
                    tokens[2] = ntriple.Trim(' ', '\t');
                    return;
                    
                }

                throw new Exception("found illegal N-Triple, unrecognized 'B->->' structure");
            }

            //A legal N-Triple starts with "_:" (blank) or "<" (uri)
            throw new Exception("found illegal N-Triple, must start with \"_:\" or with \"<\"");
        }
        #endregion

        #endregion
    }
}