/*
   Copyright 2012-2017 Marco De Salvo

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


namespace RDFSharp.Model
{
    
    /// <summary>
    /// RDFNTriples is responsible for managing serialization to and from N-Triples data format.
    /// </summary>
    internal static class RDFNTriples {

        #region Properties
        /// <summary>
        /// Regex to detect S->P->B form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex SPB        = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*_:[^<>]+\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect S->P->O form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex SPO        = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*<[^<>]+>\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect S->P->L(PLAIN) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex SPL_PLAIN  = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect S->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex SPL_PLANG  = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect S->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex SPL_TLIT   = new Regex(@"^<[^<>]+>\s*<[^<>]+>\s*\""(.)*\""\^\^<[^<>]+>\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect B->P->B form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex BPB        = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*_:[^<>]+\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect B->P->O form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex BPO        = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*<[^<>]+>\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect B->P->L(PLAIN) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex BPL_PLAIN  = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect B->P->L(PLAIN LANGUAGE) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex BPL_PLANG  = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*\s*.$", RegexOptions.Compiled);

        /// <summary>
        /// Regex to detect B->P->L(TYPED) form of N-Triple/N-Quad
        /// </summary>
        internal static readonly Regex BPL_TLIT   = new Regex(@"^_:[^<>]+\s*<[^<>]+>\s*\""(.)*\""\^\^<[^<>]+>\s*.$", RegexOptions.Compiled);
        
        /// <summary>
        /// Regex to detect presence of a plain literal with language tag within a given N-Triple
        /// </summary>
        internal static readonly Regex regexLPL = new Regex(@"@[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*$", RegexOptions.Compiled);
        
        /// <summary>
        /// Regex to detect presence of starting " in the value of a given N-Triple literal
        /// </summary>
        internal static readonly Regex regexSqt = new Regex(@"^""", RegexOptions.Compiled);
        
        /// <summary>
        /// Regex to detect presence of ending " in the value of a given N-Triple literal
        /// </summary>
        internal static readonly Regex regexEqt = new Regex(@"""$", RegexOptions.Compiled);
        #endregion

        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given graph to the given filepath using N-Triples data format. 
        /// </summary>
        internal static void Serialize(RDFGraph graph, String filepath) {
            Serialize(graph, new FileStream(filepath, FileMode.Create));
        }

        /// <summary>
        /// Serializes the given graph to the given stream using N-Triples data format. 
        /// </summary>
        internal static void Serialize(RDFGraph graph, Stream outputStream) {
            try {

                #region serialize
                using (StreamWriter sw         = new StreamWriter(outputStream, Encoding.ASCII)) {
                    String tripleTemplate      = String.Empty;
                    foreach(var t             in graph) {

                        #region template
                        if (t.TripleFlavor    == RDFModelEnums.RDFTripleFlavors.SPO) {
                            tripleTemplate     = "<{SUBJ}> <{PRED}> <{OBJ}> .";
                        }
                        else {
                            if (t.Object      is RDFPlainLiteral) {
                                tripleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"@{LANG} .";
                            }
                            else {
                                tripleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"^^<{DTYPE}> .";
                            }
                        }
                        #endregion

                        #region subj
                        if(((RDFResource)t.Subject).IsBlank) {
                             tripleTemplate    = tripleTemplate.Replace("<{SUBJ}>", RDFModelUtilities.Unicode_To_ASCII(t.Subject.ToString()).Replace("bnode:", "_:"));
                        }
                        else {
                             tripleTemplate    = tripleTemplate.Replace("{SUBJ}", RDFModelUtilities.Unicode_To_ASCII(t.Subject.ToString()));
                        }
                        #endregion

                        #region pred
                        tripleTemplate         = tripleTemplate.Replace("{PRED}", RDFModelUtilities.Unicode_To_ASCII(t.Predicate.ToString()));
                        #endregion

                        #region object
                        if (t.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                            if(((RDFResource)t.Object).IsBlank) {
                                tripleTemplate = tripleTemplate.Replace("<{OBJ}>", RDFModelUtilities.Unicode_To_ASCII(t.Object.ToString())).Replace("bnode:", "_:");
                            }
                            else {
                                tripleTemplate = tripleTemplate.Replace("{OBJ}", RDFModelUtilities.Unicode_To_ASCII(t.Object.ToString()));
                            }
                        }
                        #endregion

                        #region literal
                        else {

                            tripleTemplate     = tripleTemplate.Replace("{VAL}", RDFModelUtilities.EscapeControlCharsForXML(RDFModelUtilities.Unicode_To_ASCII(((RDFLiteral)t.Object).Value.Replace("\\", "\\\\").Replace("\"", "\\\""))));
                            tripleTemplate     = tripleTemplate.Replace("\n", "\\n")
                                                               .Replace("\t", "\\t")
                                                               .Replace("\r", "\\r");

                            #region plain literal
                            if (t.Object is RDFPlainLiteral) {
                                if(((RDFPlainLiteral)t.Object).Language != String.Empty) {
                                     tripleTemplate = tripleTemplate.Replace("{LANG}", ((RDFPlainLiteral)t.Object).Language);
                                }
                                else {
                                     tripleTemplate = tripleTemplate.Replace("@{LANG}", String.Empty);
                                }
                            }
                            #endregion

                            #region typed literal
                            else {
                                tripleTemplate = tripleTemplate.Replace("{DTYPE}", RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)t.Object).Datatype));
                            }
                            #endregion

                        }
                        #endregion

                        sw.WriteLine(tripleTemplate);
                    }
                }
                #endregion

            }
            catch(Exception ex) {
                throw new RDFModelException("Cannot serialize N-Triples because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given N-Triples filepath to a graph. 
        /// </summary>
        internal static RDFGraph Deserialize(String filepath) {
            return Deserialize(new FileStream(filepath, FileMode.Open));
        }

        /// <summary>
        /// Deserializes the given N-Triples stream to a graph. 
        /// </summary>
        internal static RDFGraph Deserialize(Stream inputStream) {
            Int64 ntripleIndex = 0;
            try {

                #region deserialize
                using (StreamReader sr = new StreamReader(inputStream, Encoding.ASCII)) {
                    RDFGraph result    = new RDFGraph();
                    String  ntriple    = String.Empty;
                    String[] tokens    = new String[3];
                    RDFResource S      = null;
                    RDFResource P      = null;
                    RDFResource O      = null;
                    RDFLiteral  L      = null;
                    while((ntriple     = sr.ReadLine()) != null) {
                        ntripleIndex++;

                        #region sanitize  & tokenize
                        //Cleanup previous data
                        S              = null;
                        tokens[0]      = String.Empty;
                        P              = null;
                        tokens[1]      = String.Empty;
                        O              = null;
                        L              = null;
                        tokens[2]      = String.Empty;

                        //Preliminary sanitizations: clean trailing space-like chars
                        ntriple        = ntriple.Trim(new Char[] { ' ', '\t', '\r', '\n' });

                        //Skip empty or comment lines
                        if (ntriple   == String.Empty || ntriple.StartsWith("#")) {
                            continue;
                        }

                        //Tokenizes the sanitized triple 
                        tokens         = TokenizeNTriple(ntriple);
                        #endregion

                        #region subj
                        String subj    = tokens[0].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new   Char[] { '>' })
                                                  .Replace("_:", "bnode:");
                        S              = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(subj));
                        #endregion

                        #region pred
                        String pred    = tokens[1].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new   Char[] { '>' });
                        P              = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(pred));
                        #endregion

                        #region object
                        if (tokens[2].StartsWith("<")      ||
                            tokens[2].StartsWith("bnode:") ||
                            tokens[2].StartsWith("_:")) {
                            String obj = tokens[2].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new Char[] { '>' })
                                                  .Replace("_:", "bnode:")
                                                  .Trim(new Char[] { ' ', '\n', '\t', '\r' });
                            O          = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(obj));
                        }
                        #endregion

                        #region literal
                        else {

                            #region sanitize
                            tokens[2]  = regexSqt.Replace(tokens[2], String.Empty);
                            tokens[2]  = regexEqt.Replace(tokens[2], String.Empty);
                            tokens[2]  = tokens[2].Replace("\\\\", "\\")
                                                  .Replace("\\\"", "\"")
                                                  .Replace("\\n", "\n")
                                                  .Replace("\\t", "\t")
                                                  .Replace("\\r", "\r");
                            tokens[2]  = RDFModelUtilities.ASCII_To_Unicode(tokens[2]);
                            #endregion

                            #region plain literal
                            if (!tokens[2].Contains("^^") ||
                                 tokens[2].EndsWith("^^") ||
                                 tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2, 1) != "<") {
                                 if (regexLPL.Match(tokens[2]).Success) {
                                     tokens[2]        = tokens[2].Replace("\"@", "@");
                                     String pLitValue = tokens[2].Substring(0, tokens[2].LastIndexOf("@", StringComparison.Ordinal));
                                     String pLitLang  = tokens[2].Substring(tokens[2].LastIndexOf("@", StringComparison.Ordinal) + 1);
                                     L                = new RDFPlainLiteral(HttpUtility.HtmlDecode(pLitValue), pLitLang);
                                 }
                                 else {
                                     L                = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2]));
                                 }
                            }
                            #endregion

                            #region typed literal
                            else {
                                tokens[2]                    = tokens[2].Replace("\"^^", "^^");
                                String tLitValue             = tokens[2].Substring(0, tokens[2].LastIndexOf("^^", StringComparison.Ordinal));
                                String tLitDatatype          = tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2)
                                                                        .TrimStart(new Char[] { '<' })
                                                                        .TrimEnd(new   Char[] { '>' });
                                RDFModelEnums.RDFDatatypes dt = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                                L                            = new RDFTypedLiteral(HttpUtility.HtmlDecode(tLitValue), dt);
                            }
                            #endregion

                        }
                        #endregion

                        #region addtriple
                        if (O != null) {
                            result.AddTriple(new RDFTriple(S, P, O));
                        }
                        else {
                            result.AddTriple(new RDFTriple(S, P, L));
                        }
                        #endregion

                    }
                    return result;
                }
                #endregion

            }
            catch(Exception ex) {
                throw new RDFModelException("Cannot deserialize N-Triples (line " + ntripleIndex + ") because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Tries to tokenize the given N-Triple
        /// </summary>
        internal static String[] TokenizeNTriple(String ntriple) {
            String[] tokens        = new String[3];

            //A legal N-Triple starts with "_:" of blanks or "<" of non-blanks
            if (ntriple.StartsWith("_:") || ntriple.StartsWith("<")) {

                //S->-> triple
                if (ntriple.StartsWith("<")) {

                    //S->P->B
                    if (SPB.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->O
                    if (SPO.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(PLAIN)
                    if (SPL_PLAIN.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(PLANG)
                    if (SPL_PLANG.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal with language
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //S->P->L(TLIT)
                    if (SPL_TLIT.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //typed literal
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    throw new Exception("found illegal N-Triple, unrecognized 'S->->' structure");
                }

                //B->-> triple
                else {

                    //B->P->B
                    if (BPB.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->O
                    if (BPO.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //object
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(PLAIN)
                    if (BPL_PLAIN.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(PLANG)
                    if (BPL_PLANG.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //plain literal with language
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    //B->P->L(TLIT)
                    if (BPL_TLIT.Match(ntriple).Success) {
                        ntriple   = ntriple.Trim(new Char[] { '.', ' ', '\t' });

                        //subject
                        tokens[0] = ntriple.Substring(0, ntriple.IndexOf('<'));
                        ntriple   = ntriple.Substring(tokens[0].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[0] = tokens[0].Trim(new Char[] { ' ', '\t' });

                        //predicate
                        tokens[1] = ntriple.Substring(0, ntriple.IndexOf('>') + 1);
                        ntriple   = ntriple.Substring(tokens[1].Length).Trim(new Char[] { ' ', '\t' });
                        tokens[1] = tokens[1].Trim(new Char[] { ' ', '\t' });

                        //typed literal
                        tokens[2] = ntriple.Trim(new Char[] { ' ', '\t' });

                        return tokens;
                    }

                    throw new Exception("found illegal N-Triple, unrecognized 'B->->' structure");
                }

            }
            else {
                throw new Exception("found illegal N-Triple, must start with \"_:\" or with \"<\"");
            }

        }
        #endregion

        #endregion

    }

}