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
    internal static class RDFNTriples {

        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given graph to the given filepath using N-Triples data format. 
        /// </summary>
        internal static void Serialize(RDFGraph graph, String filepath) {
            try {

                #region serialize
                using (StreamWriter sw         = new StreamWriter(filepath, false, Encoding.ASCII)) {
                    String tripleTemplate      = String.Empty;
                    foreach(RDFTriple t       in graph) {

                        #region template
                        if (t.TripleFlavor    == RDFModelEnums.RDFTripleFlavors.SPO) {
                            tripleTemplate     = "<{SUBJ}> <{PRED}> <{OBJ}> .";
                        }
                        else {
                            if (t.Object is RDFPlainLiteral) {
                                tripleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"@{LANG} .";
                            }
                            else {
                                tripleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"^^<{DTYPE}> .";
                            }
                        }
                        #endregion

                        #region subj
                        if (((RDFResource)t.Subject).IsBlank) {
                            tripleTemplate     = tripleTemplate.Replace("<{SUBJ}>", RDFModelUtilities.Unicode_To_ASCII(t.Subject.ToString()).Replace("bnode:", "_:"));
                        }
                        else {
                            tripleTemplate     = tripleTemplate.Replace("{SUBJ}", RDFModelUtilities.Unicode_To_ASCII(t.Subject.ToString()));
                        }
                        #endregion

                        #region pred
                        tripleTemplate         = tripleTemplate.Replace("{PRED}", RDFModelUtilities.Unicode_To_ASCII(t.Predicate.ToString()));
                        #endregion

                        #region object
                        if (t.TripleFlavor    == RDFModelEnums.RDFTripleFlavors.SPO) {
                            if (((RDFResource)t.Object).IsBlank) {
                                tripleTemplate = tripleTemplate.Replace("<{OBJ}>", RDFModelUtilities.Unicode_To_ASCII(t.Object.ToString())).Replace("bnode:", "_:");
                            }
                            else {
                                tripleTemplate = tripleTemplate.Replace("{OBJ}", RDFModelUtilities.Unicode_To_ASCII(t.Object.ToString()));
                            }
                        }
                        #endregion

                        #region literal
                        else {

                            tripleTemplate         = tripleTemplate.Replace("{VAL}", RDFModelUtilities.Unicode_To_ASCII(((RDFLiteral)t.Object).Value.Replace("\"", "\\\"")));
                            tripleTemplate         = tripleTemplate.Replace("\n", "\\n")
                                                                   .Replace("\t", "\\t")
                                                                   .Replace("\r", "\\r");

                            #region plain literal
                            if (t.Object is RDFPlainLiteral) {
                                if (((RDFPlainLiteral)t.Object).Language != String.Empty) {
                                    tripleTemplate = tripleTemplate.Replace("{LANG}", ((RDFPlainLiteral)t.Object).Language);
                                }
                                else {
                                    tripleTemplate = tripleTemplate.Replace("@{LANG}", String.Empty);
                                }
                            }
                            #endregion

                            #region typed literal
                            else {
                                tripleTemplate     = tripleTemplate.Replace("{DTYPE}", ((RDFTypedLiteral)t.Object).Datatype.ToString());
                            }
                            #endregion

                        }
                        #endregion

                        sw.WriteLine(tripleTemplate);
                    }
                }
                #endregion

            }
            catch (Exception ex) {
                throw new RDFModelException("Cannot serialize N-Triples because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given N-Triples filepath to a graph. 
        /// </summary>
        internal static RDFGraph Deserialize(String filepath) {
            try {

                #region deserialize
                using (StreamReader sr = new StreamReader(filepath)) {
                    RDFGraph result    = new RDFGraph();
                    String  ntriple    = String.Empty;
                    String[] tokens    = new String[3];
                    while ((ntriple    = sr.ReadLine()) != null) {

                        #region sanitize & parse
                        //Preliminary sanitizations: clean trailing space-like chars
                        ntriple        = ntriple.Trim(new Char[] { ' ', '\t', '\r', '\n', '.' });

                        //Skip empty or comment lines
                        if (ntriple   == String.Empty || ntriple.StartsWith("#")) {
                            continue;
                        }

                        //Parse the sanitized triple 
                        tokens         = RDFModelUtilities.ParseNTriple(ntriple);                       
                        #endregion

                        #region subj
                        String subj    = tokens[0].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new   Char[] { '>' })
                                                  .Replace("_:", "bnode:");
                        RDFResource S  = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(subj));
                        #endregion

                        #region pred
                        String pred    = tokens[1].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new   Char[] { '>' });
                        RDFResource P  = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(pred));
                        #endregion

                        #region object
                        if (tokens[2].StartsWith("<")      || 
                            tokens[2].StartsWith("bnode:") || 
                            tokens[2].StartsWith("_:")) {
                            String obj = tokens[2].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new Char[] { '>' })
                                                  .Replace("_:", "bnode:")
                                                  .Trim(new Char[] { ' ', '\n', '\t', '\r' });
                            var O      = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(obj));
                            result.AddTriple(new RDFTriple(S, P, O));
                        }
                        #endregion

                        #region literal
                        else {

                            #region sanitize
                            tokens[2]  = RDFModelUtilities.regexSqt.Replace(tokens[2], String.Empty);
                            tokens[2]  = RDFModelUtilities.regexEqt.Replace(tokens[2], String.Empty);
                            tokens[2]  = tokens[2].Replace("\\\"", "\"")
                                                  .Replace("\\n",  "\n")
                                                  .Replace("\\t",  "\t")
                                                  .Replace("\\r",  "\r");
                            tokens[2]  = RDFModelUtilities.ASCII_To_Unicode(tokens[2]);
                            #endregion

                            #region plain literal
                            if (!tokens[2].Contains("^^") || 
                                 tokens[2].EndsWith("^^") ||
                                 tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2, 1) != "<") {
                                 RDFPlainLiteral L    = null;
                                 if (RDFModelUtilities.regexLPL.Match(tokens[2]).Success) {
                                     tokens[2]        = tokens[2].Replace("\"@", "@");
                                     String pLitValue = tokens[2].Substring(0, tokens[2].LastIndexOf("@", StringComparison.Ordinal));
                                     String pLitLang  = tokens[2].Substring(tokens[2].LastIndexOf("@", StringComparison.Ordinal) + 1);
                                     L                = new RDFPlainLiteral(HttpUtility.HtmlDecode(pLitValue), pLitLang);
                                 }
                                 else {
                                     L                = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2]));
                                 }
                                 result.AddTriple(new RDFTriple(S, P, L));
                            }
                            #endregion

                            #region typed literal
                            else {
                                tokens[2]             = tokens[2].Replace("\"^^", "^^");
                                String tLitValue      = tokens[2].Substring(0, tokens[2].LastIndexOf("^^", StringComparison.Ordinal));
                                String tLitDatatype   = tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2)
                                                                 .TrimStart(new Char[] { '<' })
                                                                 .TrimEnd(new   Char[] { '>' });
                                RDFDatatype dt        = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                                RDFTypedLiteral L     = new RDFTypedLiteral(HttpUtility.HtmlDecode(tLitValue), dt);
                                result.AddTriple(new RDFTriple(S, P, L));
                            }
                            #endregion

                        }
                        #endregion

                    }
                    return result;
                }
                #endregion

            }
            catch (Exception ex) {
                throw new RDFModelException("Cannot deserialize N-Triples because: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Deserializes the given N-Triples stream to a graph. 
        /// </summary>
        internal static RDFGraph Deserialize(Stream inputStream) {
            try {

                #region deserialize
                using(StreamReader sr  = new StreamReader(inputStream)) {
                    RDFGraph result    = new RDFGraph();
                    String  ntriple    = String.Empty;
                    String[] tokens    = new String[3];
                    while((ntriple     = sr.ReadLine()) != null) {

                        #region sanitize & parse
                        //Preliminary sanitizations: clean trailing space-like chars
                        ntriple        = ntriple.Trim(new Char[] { ' ', '\t', '\r', '\n', '.' });

                        //Skip empty or comment lines
                        if (ntriple   == String.Empty || ntriple.StartsWith("#")) {
                            continue;
                        }

                        //Parse the sanitized triple 
                        tokens         = RDFModelUtilities.ParseNTriple(ntriple);
                        #endregion

                        #region subj
                        String subj    = tokens[0].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new   Char[] { '>' })
                                                  .Replace("_:", "bnode:");
                        RDFResource S  = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(subj));
                        #endregion

                        #region pred
                        String pred    = tokens[1].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new   Char[] { '>' });
                        RDFResource P  = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(pred));
                        #endregion

                        #region object
                        if (tokens[2].StartsWith("<")      ||
                            tokens[2].StartsWith("bnode:") ||
                            tokens[2].StartsWith("_:")) {
                            String obj = tokens[2].TrimStart(new Char[] { '<' })
                                                  .TrimEnd(new Char[] { '>' })
                                                  .Replace("_:", "bnode:")
                                                  .Trim(new Char[] { ' ', '\n', '\t', '\r' });
                            var O      = new RDFResource(RDFModelUtilities.ASCII_To_Unicode(obj));
                            result.AddTriple(new RDFTriple(S, P, O));
                        }
                        #endregion

                        #region literal
                        else {

                            #region sanitize
                            tokens[2] = RDFModelUtilities.regexSqt.Replace(tokens[2], String.Empty);
                            tokens[2] = RDFModelUtilities.regexEqt.Replace(tokens[2], String.Empty);
                            tokens[2] = tokens[2].Replace("\\\"", "\"")
                                                  .Replace("\\n", "\n")
                                                  .Replace("\\t", "\t")
                                                  .Replace("\\r", "\r");
                            tokens[2] = RDFModelUtilities.ASCII_To_Unicode(tokens[2]);
                            #endregion

                            #region plain literal
                            if (!tokens[2].Contains("^^") ||
                                 tokens[2].EndsWith("^^") ||
                                 tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2, 1) != "<") {
                                RDFPlainLiteral L    = null;
                                if (RDFModelUtilities.regexLPL.Match(tokens[2]).Success) {
                                    tokens[2]        = tokens[2].Replace("\"@", "@");
                                    String pLitValue = tokens[2].Substring(0, tokens[2].LastIndexOf("@", StringComparison.Ordinal));
                                    String pLitLang  = tokens[2].Substring(tokens[2].LastIndexOf("@", StringComparison.Ordinal) + 1);
                                    L                = new RDFPlainLiteral(HttpUtility.HtmlDecode(pLitValue), pLitLang);
                                }
                                else {
                                    L                = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2]));
                                }
                                result.AddTriple(new RDFTriple(S, P, L));
                            }
                            #endregion

                            #region typed literal
                            else {
                                tokens[2]             = tokens[2].Replace("\"^^", "^^");
                                String tLitValue      = tokens[2].Substring(0, tokens[2].LastIndexOf("^^", StringComparison.Ordinal));
                                String tLitDatatype   = tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2)
                                                                 .TrimStart(new Char[] { '<' })
                                                                 .TrimEnd(new   Char[] { '>' });
                                RDFDatatype dt        = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                                RDFTypedLiteral L     = new RDFTypedLiteral(HttpUtility.HtmlDecode(tLitValue), dt);
                                result.AddTriple(new RDFTriple(S, P, L));
                            }
                            #endregion

                        }
                        #endregion

                    }
                    return result;
                }
                #endregion

            }
            catch(Exception ex) {
                throw new RDFModelException("Cannot deserialize N-Triples because: " + ex.Message, ex);
            }
        }
        #endregion

        #endregion

    }

}