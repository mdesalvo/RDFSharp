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
using System.Text.RegularExpressions;
using System.Web;
using RDFSharp.Model;

namespace RDFSharp.Store {

    /// <summary>
    /// RDFNQuads is responsible for managing serialization to and from N-Quads data format.
    /// </summary>
    internal static class RDFNQuads {

        #region Methods
        /// <summary>
        /// Serializes the given store to the given filepath using N-Quads data format. 
        /// </summary>
        internal static void Serialize(RDFStore store, String filepath) {
            try {

                #region serialize
                using (StreamWriter sw        = new StreamWriter(filepath, false, Encoding.ASCII)) {
                    String quadrupleTemplate  = String.Empty;
                    foreach (RDFQuadruple q  in store.SelectAllQuadruples()) {

                        #region template
                        if (q.TripleFlavor   == RDFModelEnums.RDFTripleFlavors.SPO) {
                            quadrupleTemplate = "<{SUBJ}> <{PRED}> <{OBJ}> <{CTX}>.";
                        }
                        else {
                            if (q.Object is RDFPlainLiteral) {
                                quadrupleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"@{LANG} <{CTX}>.";
                            }
                            else {
                                quadrupleTemplate = "<{SUBJ}> <{PRED}> \"{VAL}\"^^<{DTYPE}> <{CTX}>.";
                            }
                        }
                        #endregion

                        #region subj
                        if (((RDFResource)q.Subject).IsBlank) {
                            quadrupleTemplate     = quadrupleTemplate.Replace("<{SUBJ}>", RDFSerializerUtilities.Unicode_To_ASCII(q.Subject.ToString()).Replace("bnode:", "_:"));
                        }
                        else {
                            quadrupleTemplate     = quadrupleTemplate.Replace("{SUBJ}", RDFSerializerUtilities.Unicode_To_ASCII(q.Subject.ToString()));
                        }
                        #endregion

                        #region pred
                        quadrupleTemplate         = quadrupleTemplate.Replace("{PRED}", RDFSerializerUtilities.Unicode_To_ASCII(q.Predicate.ToString()));
                        #endregion

                        #region object
                        if (q.TripleFlavor       == RDFModelEnums.RDFTripleFlavors.SPO) {
                            if (((RDFResource)q.Object).IsBlank) {
                                quadrupleTemplate = quadrupleTemplate.Replace("<{OBJ}>", RDFSerializerUtilities.Unicode_To_ASCII(q.Object.ToString())).Replace("bnode:", "_:");
                            }
                            else {
                                quadrupleTemplate = quadrupleTemplate.Replace("{OBJ}", RDFSerializerUtilities.Unicode_To_ASCII(q.Object.ToString()));
                            }
                        }
                        #endregion

                        #region literal
                        else {

                            quadrupleTemplate     = quadrupleTemplate.Replace("{VAL}", RDFSerializerUtilities.Unicode_To_ASCII(((RDFLiteral)q.Object).Value).Replace("\"", "\\\""));
                            quadrupleTemplate     = quadrupleTemplate.Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r");

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
                        quadrupleTemplate         = quadrupleTemplate.Replace("<{CTX}>", RDFSerializerUtilities.Unicode_To_ASCII(q.Context.ToString()).Replace("bnode:", "_:"));
                        #endregion

                        sw.WriteLine(quadrupleTemplate);
                    }
                }
                #endregion

            }
            catch (Exception ex) {
                throw new RDFModelException("Cannot serialize N-Quads because: " + ex.Message, ex);
            }
        }
        #endregion

    }

}