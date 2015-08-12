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
using System.IO;

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFSerializer exposes choices to read and write RDF data in supported syntaxes.
    /// </summary>
    public static class RDFSerializer {

        #region Methods
        /// <summary>
        /// Writes the given graph to the given file in the given RDF format. 
        /// </summary>
        public static void WriteRDF(RDFModelEnums.RDFFormats rdfSyntax, RDFGraph graph, String filepath) {
            if (graph != null) {
                if (filepath != null && filepath.Trim() != String.Empty) {
                    switch(rdfSyntax) {
                        case RDFModelEnums.RDFFormats.NTriples:
                            RDFNTriples.Serialize(graph, filepath);
                            break;
                        case RDFModelEnums.RDFFormats.RdfXml:
                            RDFXml.Serialize(graph, filepath);
                            break;
                        case RDFModelEnums.RDFFormats.TriX:
                            RDFTrix.Serialize(graph, filepath);
                            break;
                        case RDFModelEnums.RDFFormats.Turtle:
                            RDFTurtle.Serialize(graph, filepath);
                            break;
                    }
                }
                else {
                    throw new RDFModelException("Cannot write RDF file because given \"filepath\" parameter is null or empty.");
                }
            }
            else {
                throw new RDFModelException("Cannot write RDF file because given \"graph\" parameter is null.");
            }
        }

        /// <summary>
        /// Reads the given file in the given RDF format to a graph. 
        /// </summary>
        public static RDFGraph ReadRDF(RDFModelEnums.RDFFormats rdfSyntax, String filepath) {
            if (filepath != null && filepath.Trim() != String.Empty) {
                if (File.Exists(filepath.Trim())) {
                    switch(rdfSyntax) {
                        case RDFModelEnums.RDFFormats.NTriples:
                            return RDFNTriples.Deserialize(filepath);
                        case RDFModelEnums.RDFFormats.RdfXml:
                            return RDFXml.Deserialize(filepath);
                        case RDFModelEnums.RDFFormats.TriX:
                            return RDFTrix.Deserialize(filepath);
                        case RDFModelEnums.RDFFormats.Turtle:
                            throw new RDFModelException("Cannot read RDF file because reading of Turtle format is not supported. What about joining the project to contribute it?");
                    }
                }
                throw new RDFModelException("Cannot read RDF file because given \"filepath\" parameter (" + filepath + ") does not indicate an existing file.");
            }
            throw new RDFModelException("Cannot read RDF file because given \"filepath\" parameter is null or empty.");
        }

        #endregion
        
    }

}