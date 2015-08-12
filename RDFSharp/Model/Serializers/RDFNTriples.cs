/*
   Copyright 2012-2015 Marco De Salvo

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
    /// RDFNTriples is responsible for managing serialization to and from NTriples data format.
    /// </summary>
    internal static class RDFNTriples {

        #region Methods
        /// <summary>
        /// Serializes the given graph to the given filepath using NTriples data format. 
        /// </summary>
        internal static void Serialize(RDFGraph graph, String filepath) {
            try {

                #region serialize
                using (StreamWriter sw = new StreamWriter(filepath, false, Encoding.ASCII)) {
                    var tripleTemplate = String.Empty;
                    foreach(var t in graph) {

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
                            tripleTemplate     = tripleTemplate.Replace("<{SUBJ}>", RDFSerializerUtilities.Unicode_To_ASCII(t.Subject.ToString()).Replace("bnode:", "_:"));
                        }
                        else {
                            tripleTemplate     = tripleTemplate.Replace("{SUBJ}", RDFSerializerUtilities.Unicode_To_ASCII(t.Subject.ToString()));
                        }
                        #endregion

                        #region pred
                        tripleTemplate         = tripleTemplate.Replace("{PRED}", RDFSerializerUtilities.Unicode_To_ASCII(t.Predicate.ToString()));
                        #endregion

                        #region object
                        if (t.TripleFlavor    == RDFModelEnums.RDFTripleFlavors.SPO) {
                            if (((RDFResource)t.Object).IsBlank) {
                                tripleTemplate = tripleTemplate.Replace("<{OBJ}>", RDFSerializerUtilities.Unicode_To_ASCII(t.Object.ToString())).Replace("bnode:", "_:");
                            }
                            else {
                                tripleTemplate = tripleTemplate.Replace("{OBJ}", RDFSerializerUtilities.Unicode_To_ASCII(t.Object.ToString()));
                            }
                        }
                        #endregion

                        #region literal
                        else {

                            tripleTemplate         = tripleTemplate.Replace("{VAL}", RDFSerializerUtilities.Unicode_To_ASCII(((RDFLiteral)t.Object).Value).Replace("\"","\\\""));
                            tripleTemplate         = tripleTemplate.Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r");

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
                throw new RDFModelException("Cannot serialize NTriples because: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Deserializes the given NTriples filepath to a graph. 
        /// </summary>
        internal static RDFGraph Deserialize(String filepath) {
            try {

                #region deserialize
                using (StreamReader sr           = new StreamReader(filepath, Encoding.ASCII)) {
                    RDFGraph result              = new RDFGraph();
                    String  ntriple              = String.Empty;
                    String[] tokens              = new String[3];
                    while ((ntriple              = sr.ReadLine()) != null) {

                        #region sanitize & parse
                        //Clean most of unwanted space-like chars
                        ntriple                  = ntriple.Trim(new Char[] { ' ', '\t', '\r', '\n', '.' });

                        //Skip the NTriple if it is empty or comment
                        if (ntriple             == String.Empty || ntriple.StartsWith("#")) {
                            continue;
                        }
                        //Parse the sanitized triple 
                        tokens                   = RDFSerializerUtilities.ParseNTriple(ntriple);                       
                        #endregion

                        #region subj
                        String subj              = tokens[0].Replace("<", String.Empty).Replace(">", String.Empty).Replace("_:", "bnode:");
                        RDFResource S            = new RDFResource(RDFSerializerUtilities.ASCII_To_Unicode(subj));
                        #endregion

                        #region pred
                        String pred              = tokens[1].Replace("<", String.Empty).Replace(">", String.Empty);
                        RDFResource P            = new RDFResource(RDFSerializerUtilities.ASCII_To_Unicode(pred));
                        #endregion

                        #region object
                        if (tokens[2].StartsWith("<") || tokens[2].StartsWith("bnode:") || tokens[2].StartsWith("_:")) {
                            String obj           = tokens[2].Replace("<", String.Empty)
							                                .Replace(">", String.Empty)
                                                            .Replace("_:", "bnode:")
															.Trim(new char[] { ' ' });
                            RDFResource O        = new RDFResource(RDFSerializerUtilities.ASCII_To_Unicode(obj));
                            result.AddTriple(new RDFTriple(S, P, O));
                        }
                        #endregion

                        #region literal
                        else {

                            #region sanitize
                            tokens[2]                     = tokens[2].TrimStart(new Char[] { '\"' });
                            if (tokens[2].EndsWith("\"")) {
                                tokens[2]                 = tokens[2].Substring(0, tokens[2].Length - 1);
                            }
                            tokens[2]                     = tokens[2].Replace("\\\"", "\"");
                            tokens[2]                     = tokens[2].Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\r", "\r");
                            tokens[2]                     = RDFSerializerUtilities.ASCII_To_Unicode(tokens[2]);
                            #endregion

                            #region plain literal
                            if (!tokens[2].Contains("^^") || tokens[2].EndsWith("^^") ||
                                tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2, 1) != "<") {
                                RDFPlainLiteral L         = null;
                                if (tokens[2].Contains("@")) {
                                    tokens[2]             = tokens[2].Replace("\"@", "@");
                                    if (!tokens[2].EndsWith("@")) {
                                        Int32 lastAmp     = tokens[2].LastIndexOf('@');
                                        L                 = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2].Substring(0, lastAmp)), tokens[2].Substring(lastAmp + 1));
                                    }
                                    else {
                                        L                 = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2]));
                                    }
                                }
                                else {
                                    L                     = new RDFPlainLiteral(HttpUtility.HtmlDecode(tokens[2]));
                                }
                                result.AddTriple(new RDFTriple(S, P, L));
                            }
                            #endregion

                            #region typed literal
                            else {
                                tokens[2]                 = tokens[2].Replace("\"^^", "^^");
                                String tLitValue          = tokens[2].Substring(0, tokens[2].LastIndexOf("^^", StringComparison.Ordinal));                                                        //Value    comes before the last "^^" token
                                String tLitDatatype       = tokens[2].Substring(tokens[2].LastIndexOf("^^", StringComparison.Ordinal) + 2).Replace("<", String.Empty).Replace(">", String.Empty); //Datatype comes after  the last "^^" token and its angle brackets must be removed
                                RDFDatatype dt            = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                                RDFTypedLiteral L         = new RDFTypedLiteral(HttpUtility.HtmlDecode(tLitValue), dt);
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
                throw new RDFModelException("Cannot deserialize NTriples because: " + ex.Message, ex);
            }
        }
        #endregion

    }

}