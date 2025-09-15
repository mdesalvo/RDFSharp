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
using System.Web;

namespace RDFSharp.Model
{
    /// <summary>
    /// RDFNTriples is responsible for managing serialization to and from N-Triples data format.
    /// </summary>
    internal static class RDFNTriples
    {
        #region Properties
        private const string TemplateSPO = "<{SUBJ}> <{PRED}> <{OBJ}> .";
        private const string TemplateSPLL = "<{SUBJ}> <{PRED}> \"{VAL}\"@{LANG} .";
        private const string TemplateSPLT = "<{SUBJ}> <{PRED}> \"{VAL}\"^^<{DTYPE}> .";

        // Facilities for deserialization
        internal static readonly char[] openingBrackets = { '<' };
        internal static readonly char[] closingBrackets = { '>' };
        internal static readonly char[] trimmableChars = { ' ', '\t', '\r', '\n' };
        internal static readonly char[] SpaceAndTabChars = { ' ', '\t' };
        internal static readonly char[] DotSpaceAndTabChars = { '.', ' ', '\t' };
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
            RDFGraph result = new RDFGraph().SetContext(graphContext);
            long ntripleIndex = 0;
            string nTriple;
            string[] tokens = new string[3];
            Dictionary<string, long> hashContext = new Dictionary<string, long>();

            try
            {
                #region deserialize
                using (StreamReader sr = new StreamReader(inputStream, Encoding.ASCII))
                {
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
                        if (tokens[2][0] == '<'
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
                            tokens[2] = RDFShims.StartingQuoteRegex.Value.Replace(tokens[2], string.Empty);
                            tokens[2] = RDFShims.EndingQuoteRegex.Value.Replace(tokens[2], string.Empty);
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
                                if (RDFShims.EndingLangTagRegex.Value.IsMatch(tokens[2]))
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
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFModelException("Cannot deserialize N-Triples (line " + ntripleIndex + ") because: " + ex.Message, ex);
            }

            return result;
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
            if (ntriple[0] == '<')
            {
                //S->P->O
                if (RDFShims.SPO.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //object
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                //S->P->L(PLAIN)
                if (RDFShims.SPL.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //plain literal
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                //S->P->L(PLANG)
                if (RDFShims.SPLL.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //plain literal with language
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                //S->P->L(TLIT)
                if (RDFShims.SPLT.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //typed literal
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                //S->P->B
                if (RDFShims.SPB.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //object
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                throw new Exception("found illegal N-Triple, unrecognized 'S->->' structure");
            }

            //B->-> triple
            if (ntriple.Length > 1 && ntriple[0] == '_' && ntriple[1] == ':')
            {
                //B->P->O
                if (RDFShims.BPO.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //object
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                //B->P->L(PLAIN)
                if (RDFShims.BPL.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //plain literal
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                //B->P->L(PLANG)
                if (RDFShims.BPLL.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //plain literal with language
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                //B->P->L(TLIT)
                if (RDFShims.BPLT.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //typed literal
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                //B->P->B
                if (RDFShims.BPB.Value.IsMatch(ntriple))
                {
                    ntriple = ntriple.Trim(DotSpaceAndTabChars);

                    //subject
                    tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                    ntriple = ntriple.Substring(tokens[0].Length).Trim(SpaceAndTabChars);
                    tokens[0] = tokens[0].Trim(SpaceAndTabChars);

                    //predicate
                    tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                    ntriple = ntriple.Substring(tokens[1].Length).Trim(SpaceAndTabChars);
                    tokens[1] = tokens[1].Trim(SpaceAndTabChars);

                    //object
                    tokens[2] = ntriple.Trim(SpaceAndTabChars);
                    return;
                }

                throw new Exception("found illegal N-Triple, unrecognized 'B->->' structure");
            }

            //A legal N-Triple starts with "<" (uri) or "_:" (blank)
            throw new Exception("found illegal N-Triple, must start with \"<\" (for Uris) or \"_:\" (for blanks)");
        }
        #endregion

        #endregion
    }
}