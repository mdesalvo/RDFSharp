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
                using (var sw = new StreamWriter(filepath, false, Encoding.ASCII)) {
                    foreach (var t in graph) { 
                        sw.WriteLine(t.ToNTriples()); 
                    }
                }
                #endregion

            }
            catch (Exception ex) {
                throw new RDFModelException("Cannot serialize N-Triples because: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Deserializes the given NTriples filepath to a graph. 
        /// </summary>
        internal static RDFGraph Deserialize(String filepath) {
            try {
                var result    = new RDFGraph();

                #region deserialize
                using (var sr = new StreamReader(filepath)) {
                    while (!sr.EndOfStream) {
                        var triple  = RDFTriple.FromNTriples(sr.ReadLine());
                        if (triple != null) {
                            result.AddTriple(triple);
                        }
                    }
                }
                #endregion

                return result;
            }
            catch (Exception ex) {
                throw new RDFModelException("Cannot deserialize N-Triples because: " + ex.Message, ex);
            }
        }
        #endregion

    }

}