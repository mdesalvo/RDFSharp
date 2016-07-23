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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFTurtle is responsible for managing serialization to Turtle data format.
    /// </summary>
    internal static class RDFTurtle {

        #region Properties
        /// <summary>
        /// Regex to catch literals which must be escaped as long literals in Turtle
        /// </summary>
        internal static readonly Regex regexTTL = new Regex("[\n\r\t\"]", RegexOptions.Compiled);
        #endregion

        #region Methods
        /// <summary>
        /// Serializes the given graph to the given filepath using Turtle data format. 
        /// </summary>
        internal static void Serialize(RDFGraph graph, String filepath) {
            Serialize(graph, new FileStream(filepath, FileMode.Create));
        }

        /// <summary>
        /// Serializes the given graph to the given stream using Turtle data format. 
        /// </summary>
        internal static void Serialize(RDFGraph graph, Stream outputStream) {
            try {

                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, Encoding.UTF8)) {

                    #region prefixes
                    //Write the namespaces collected by the graph
                    foreach(var ns in graph.GraphMetadata.Namespaces.OrderBy(n => n.Prefix)) {
                        sw.WriteLine("@prefix " + ns.Prefix + ": <" + ns.Namespace + ">.");
                    }
                    sw.WriteLine("@base <" + graph.Context + ">.\n");
                    #endregion

                    #region linq
                    //Group the graph's triples by subj and pred
                    var groupedList             = (from    triple in graph
                                                   orderby triple.Subject.ToString(), triple.Predicate.ToString()
                                                   group   triple by new {
                                                        subj = triple.Subject.ToString(),
                                                        pred = triple.Predicate.ToString()
                                                  });
                    var groupedListLast         = groupedList.Last();
                    #endregion
												   
					#region triples
                    String actualSubj           = String.Empty;
                    String abbreviatedSubj      = String.Empty;
                    String actualPred           = String.Empty;
                    String abbreviatedPred      = String.Empty;
                    const String spaceConst     = " ";
                    StringBuilder result        = new StringBuilder();

					//Iterate over the calculated groups
                    foreach (var group in groupedList) {
                        var groupLast           = group.Last();

                        //Reset the flag of subj printing for the new iteration
                        Boolean subjPrint       = false;

                        #region subj
                        //New subj found: write the finished Turtle token to the file, then start collecting the new one
                        if (!actualSubj.Equals(group.Key.subj, StringComparison.Ordinal)) {
                            if (result.Length > 0) {
                                result.Replace(";", ".", result.Length - 4, 1);
                                sw.Write(result.ToString());
                                result.Remove(0, result.Length - 1);
                            }
                            actualSubj          = group.Key.subj;
                            actualPred          = String.Empty;
                            if (!actualSubj.StartsWith("_:")) {
                                abbreviatedSubj = RDFModelUtilities.AbbreviateNamespace(actualSubj);
                            }
                            else {
                                abbreviatedSubj = actualSubj;
                            }
                            result.Append(abbreviatedSubj + " ");
                            subjPrint           = true;
                        }
                        #endregion

                        #region predObjList
                        //Iterate over the triples of the current group
                        foreach (var triple in group) {

                            #region pred
                            //New pred found: collect it to the actual Turtle token.
                            if (!actualPred.Equals(triple.Predicate.ToString(), StringComparison.Ordinal)) {
                                if (!subjPrint) {
                                    result.Append(spaceConst.PadRight(abbreviatedSubj.Length + 1)); //pretty-printing spaces to align the predList
                                }
                                actualPred          = triple.Predicate.ToString();
                                abbreviatedPred     = RDFModelUtilities.AbbreviateNamespace(actualPred);
                                //Turtle goody for "rdf:type" shortcutting to "a"
                                if (abbreviatedPred == RDFVocabulary.RDF.PREFIX + ":type") {
                                    abbreviatedPred = "a";
                                }
                                result.Append(abbreviatedPred + " ");
                            }
                            #endregion

                            #region object
                            //Collect the object or the literal to the Turtle token
                            if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO) {
                                String obj           = triple.Object.ToString();
                                if (!obj.StartsWith("_:")) {
                                     result.Append(RDFModelUtilities.AbbreviateNamespace(obj));
                                }
                                else {
                                     result.Append(obj);
                                }
                            }
                            #endregion

                            #region literal
                            else {

                                //Detect presence of long-literals
                                var litValDelim = "\"";
                                if (regexTTL.Match(triple.Object.ToString()).Success) {
                                    litValDelim = "\"\"\"";
                                }

                                if (triple.Object is RDFTypedLiteral) {
                                    String tLit = litValDelim + ((RDFTypedLiteral)triple.Object).Value.Replace("\"", "\\\"") + litValDelim + "^^" + ((RDFTypedLiteral)triple.Object).Datatype;
                                    result.Append(RDFModelUtilities.AbbreviateNamespace(tLit));
                                }
                                else {
                                    String pLit = litValDelim + ((RDFPlainLiteral)triple.Object).Value.Replace("\"", "\\\"") + litValDelim;
                                    if (((RDFPlainLiteral)triple.Object).Language != String.Empty) {
                                        pLit    = pLit + "@"  + ((RDFPlainLiteral)triple.Object).Language;
                                    }
                                    result.Append(pLit);
                                }

                            }
                            #endregion

                            #region continuation goody
                            //Then append the appropriated Turtle continuation goody ("," or ";")
                            if (!triple.Equals(groupLast)) {
                                result.Append(", ");
                            }
                            else {
                                result.AppendLine("; ");
                            }
                            #endregion

                        }
						#endregion

                        #region last group
                        //This is only for the last group, which is not written into the cycle as the others
                        if (group.Key.Equals(groupedListLast.Key)) {
                            result.Replace(";", ".", result.Length - 4, 1);
                            sw.Write(result.ToString());
                        }
                        #endregion

                    }
                    #endregion

                }
                #endregion

            }
            catch (Exception ex) {
                throw new RDFModelException("Cannot serialize Turtle because: " + ex.Message, ex);
            }
        }
        #endregion

    }

}