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

namespace RDFSharp.Store {

    /// <summary>
    /// RDFStoreSerializer exposes choices to read and write RDF store data in supported formats.
    /// </summary>
    public static class RDFStoreSerializer {

        #region Methods
        /// <summary>
        /// Writes the given store to the given file in the given RDF format. 
        /// </summary>
        public static void WriteRDF(RDFStoreEnums.RDFFormats rdfFormat, RDFStore store, String filepath) {
            if (store        != null) {
                if (filepath != null) {
                    switch (rdfFormat) {
                        case RDFStoreEnums.RDFFormats.NQuads:
                             RDFNQuads.Serialize(store, filepath);
                             break;
                    }
                }
                else {
                    throw new RDFStoreException("Cannot write RDF file because given \"filepath\" parameter is null.");
                }
            }
            else {
                throw new RDFStoreException("Cannot write RDF file because given \"store\" parameter is null.");
            }
        }
        #endregion

    }

}