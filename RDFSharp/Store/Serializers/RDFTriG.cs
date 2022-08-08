/*
   Copyright 2012-2022 Marco De Salvo

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
using RDFSharp.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RDFSharp.Store
{
    /// <summary>
    /// RDFTriG is responsible for managing serialization to and from TriG data format.
    /// </summary>
    internal static class RDFTriG
    {
        #region Methods

        #region Write
        /// <summary>
        /// Serializes the given store to the given filepath using TriG data format.
        /// </summary>
        internal static void Serialize(RDFStore store, string filepath)
            => Serialize(store, new FileStream(filepath, FileMode.Create));

        /// <summary>
        /// Serializes the given store to the given stream using TriG data format.
        /// </summary>
        internal static void Serialize(RDFStore store, Stream outputStream)
        {
            try
            {
                #region serialize
                using (StreamWriter sw = new StreamWriter(outputStream, RDFModelUtilities.UTF8_NoBOM))
                {
                    #region prefixes
                    //Extract graphs from the store
                    List<RDFGraph> graphs = store.ExtractGraphs();

                    //Collect the namespaces from the extracted graphs
                    List<RDFNamespace> namespaces = new List<RDFNamespace>();
                    foreach (RDFGraph graph in graphs)
                        namespaces.AddRange(RDFModelUtilities.GetGraphNamespaces(graph));

                    //Print the collected namespaces
                    HashSet<string> printedNamespaces = new HashSet<string>();
                    foreach (RDFNamespace nameSpace in namespaces.OrderBy(n => n.NamespacePrefix))
                    {
                        if (!printedNamespaces.Contains(nameSpace.NamespacePrefix))
                        {
                            printedNamespaces.Add(nameSpace.NamespacePrefix);
                            sw.WriteLine(string.Concat("@prefix ", nameSpace.NamespacePrefix, ": <", nameSpace.NamespaceUri, ">."));
                        }
                    }
                    if (printedNamespaces.Count > 0)
                        sw.WriteLine();
                    #endregion

                    #region graphs
                    //Print extracted graphs
                    foreach (RDFGraph graph in graphs)
                    {
                        #region linq
                        //Group the graph's triples by subject and predicate
                        var triplesGroupedBySubjectAndPredicate =
                            (from triple in graph
                             orderby triple.Subject.ToString(), triple.Predicate.ToString()
                             group triple by new
                             {
                                 subj = triple.Subject.ToString(),
                                 pred = triple.Predicate.ToString()
                             });
                        var lastGroupOfTriples = triplesGroupedBySubjectAndPredicate.LastOrDefault();
                        #endregion

                        #region graph
                        //Custom contexts must be wrapped with GRAPH form
                        if (!graph.Context.Equals(RDFNamespaceRegister.DefaultNamespace.NamespaceUri))
                            sw.WriteLine($"GRAPH <{graph.Context}>");
                        sw.WriteLine("{");

                        #region triples
                        string actualSubject = string.Empty;
                        string abbreviatedSubject = string.Empty;
                        string actualPredicate = string.Empty;
                        string abbreviatedPredicate = string.Empty;
                        const string spaceConst = " ";
                        StringBuilder result = new StringBuilder();

                        //Iterate over the calculated groups
                        foreach (var triplesGroup in triplesGroupedBySubjectAndPredicate)
                        {
                            RDFTriple lastTripleOfGroup = triplesGroup.Last();

                            #region subj
                            //Reset the flag of subj printing for the new iteration
                            bool subjectHasBeenPrinted = false;

                            //New subject found
                            if (!actualSubject.Equals(triplesGroup.Key.subj, StringComparison.Ordinal))
                            {
                                //Write the subject's Turtle token
                                if (result.Length > 0)
                                {
                                    result.Replace(";", ".", result.Length - (2 + Environment.NewLine.Length), 1);
                                    sw.Write(result.ToString());
                                    result.Clear();
                                }

                                //Start collecting the new subject's one
                                actualSubject = triplesGroup.Key.subj;
                                actualPredicate = string.Empty;
                                abbreviatedSubject = RDFQueryPrinter.PrintPatternMember(RDFQueryUtilities.ParseRDFPatternMember(actualSubject), namespaces);
                                result.Append(string.Concat(abbreviatedSubject, spaceConst));
                                subjectHasBeenPrinted = true;
                            }
                            #endregion

                            #region predObjList
                            //Iterate over the triples of the current group
                            foreach (RDFTriple triple in triplesGroup)
                            {
                                #region pred
                                //New predicate found
                                if (!actualPredicate.Equals(triple.Predicate.ToString(), StringComparison.Ordinal))
                                {
                                    //Write the predicate's Turtle token
                                    if (!subjectHasBeenPrinted)
                                        result.Append(spaceConst.PadRight(abbreviatedSubject.Length + 1)); //pretty-printing spaces to align the predList
                                    actualPredicate = triple.Predicate.ToString();
                                    abbreviatedPredicate = RDFQueryPrinter.PrintPatternMember(RDFQueryUtilities.ParseRDFPatternMember(actualPredicate), namespaces);

                                    //Turtle goody for "rdf:type" shortcutting to "a"
                                    if (abbreviatedPredicate.Equals("rdf:type", StringComparison.Ordinal))
                                        abbreviatedPredicate = "a";

                                    result.Append(string.Concat(abbreviatedPredicate, spaceConst));
                                }
                                #endregion

                                #region object
                                //Collect the object to the Turtle token
                                if (triple.TripleFlavor == RDFModelEnums.RDFTripleFlavors.SPO)
                                {
                                    //Write the object's Turtle token
                                    string currentObject = triple.Object.ToString();
                                    result.Append(RDFQueryPrinter.PrintPatternMember(RDFQueryUtilities.ParseRDFPatternMember(currentObject), namespaces));
                                }
                                #endregion

                                #region literal
                                //Collect the literal to the Turtle token
                                else
                                {
                                    //Detect presence of long-literals in order to write proper delimiter
                                    string litValDelim = "\"";
                                    if (RDFTurtle.regexTTL.Value.Match(triple.Object.ToString()).Success)
                                        litValDelim = "\"\"\"";

                                    //Write the literal's Turtle token
                                    if (triple.Object is RDFTypedLiteral)
                                    {
                                        string dtype = RDFQueryPrinter.PrintPatternMember(
                                                        RDFQueryUtilities.ParseRDFPatternMember(
                                                         RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)triple.Object).Datatype)), namespaces);
                                        string tLit = string.Concat(litValDelim, ((RDFTypedLiteral)triple.Object).Value.Replace("\\", "\\\\"), litValDelim, "^^", dtype);
                                        result.Append(tLit);
                                    }
                                    else
                                    {
                                        string pLit = string.Concat(litValDelim, ((RDFPlainLiteral)triple.Object).Value.Replace("\\", "\\\\"), litValDelim);
                                        if (((RDFPlainLiteral)triple.Object).HasLanguage())
                                            pLit = string.Concat(pLit, "@", ((RDFPlainLiteral)triple.Object).Language);
                                        result.Append(pLit);
                                    }
                                }
                                #endregion

                                #region continuation goody
                                //Then append appropriate Turtle continuation goody ("," or ";")
                                if (!triple.Equals(lastTripleOfGroup))
                                    result.Append(", ");
                                else
                                    result.AppendLine("; ");
                                #endregion
                            }
                            #endregion

                            #region last group
                            //This is only for the last group, which is not written into the cycle as the others
                            if (triplesGroup.Key.Equals(lastGroupOfTriples.Key))
                            {
                                result.Replace(";", ".", result.Length - (2 + Environment.NewLine.Length), 1);
                                sw.Write(result.ToString());
                            }
                            #endregion
                        }
                        #endregion

                        sw.WriteLine("}");
                        #endregion
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot serialize TriG because: " + ex.Message, ex);
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Deserializes the given TriG filepath to a memory store.
        /// </summary>
        internal static RDFMemoryStore Deserialize(string filepath)
            => Deserialize(new FileStream(filepath, FileMode.Open));

        /// <summary>
        /// Deserializes the given TriG stream to a memory store.
        /// </summary>
        internal static RDFMemoryStore Deserialize(Stream inputStream)
        {
            try
            {
                #region deserialize

                RDFMemoryStore result = new RDFMemoryStore();
                
                //TODO


                return result;

                #endregion deserialize
            }
            catch (Exception ex)
            {
                throw new RDFStoreException("Cannot deserialize TriG because: " + ex.Message, ex);
            }
        }
        #endregion

        #endregion
    }
}