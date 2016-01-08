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

namespace RDFSharp.Model
{
    
    /// <summary>
    /// RDFModelEnums represents a collector for all the enumerations used by the "RDFSharp.Model" namespace
    /// </summary>
    public static class RDFModelEnums {

        /// <summary>
        /// RDFFormats represents an enumeration for supported RDF graph serialization data formats.
        /// </summary>
        public enum RDFFormats { NTriples, Turtle, TriX, RdfXml };

        /// <summary>
        /// RDFTripleFlavors represents an enumeration for possible triple pattern flavors.
        /// </summary>
        public enum RDFTripleFlavors  { SPO = 1, SPL = 2, SPV = 3 };
        
        /// <summary>
        /// RDFContainerTypes represents an enumeration for supported container types.
        /// </summary>
        public enum RDFContainerTypes { Bag, Seq, Alt };

        /// <summary>
        /// RDFItemTypes represents an enumeration for acceptable RDFContainer and RDFCollection item types.
        /// </summary>
        public enum RDFItemTypes { Resource, Literal };

        /// <summary>
        /// RDFDatatypeCategory represents an enumeration for supported categories of datatype
        /// </summary>
        public enum RDFDatatypeCategory { Boolean, Numeric, DateTime, TimeSpan, String };

    }

}