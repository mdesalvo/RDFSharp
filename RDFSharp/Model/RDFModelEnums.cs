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
        public enum RDFFormats {
            /// <summary>
            /// N-Triples serialization
            /// </summary>
            NTriples = 0,
            /// <summary>
            /// Turtle serialization
            /// </summary>
            Turtle = 1,
            /// <summary>
            /// TriX serialization
            /// </summary>
            TriX = 2,
            /// <summary>
            /// XML serialization
            /// </summary>
            RdfXml = 3
        };

        /// <summary>
        /// RDFTripleFlavors represents an enumeration for possible triple pattern flavors.
        /// </summary>
        public enum RDFTripleFlavors  {
            /// <summary>
            /// Indicates that the object of the triple is a resource
            /// </summary>
            SPO = 1,
            /// <summary>
            /// Indicates that the object of the triple is a literal
            /// </summary>
            SPL = 2,
            /// <summary>
            /// Indicates that the object of the triple is a variable
            /// </summary>
            SPV = 3
        };
        
        /// <summary>
        /// RDFContainerTypes represents an enumeration for supported container types.
        /// </summary>
        public enum RDFContainerTypes {
            /// <summary>
            /// Represents an unordered list which allows duplicates
            /// </summary>
            Bag = 0,
            /// <summary>
            /// Represents an ordered list which allows duplicates
            /// </summary>
            Seq = 1,
            /// <summary>
            /// Represents an unordered list which does not allow duplicates
            /// </summary>
            Alt = 2
        };

        /// <summary>
        /// RDFItemTypes represents an enumeration for acceptable RDFContainer and RDFCollection item types.
        /// </summary>
        public enum RDFItemTypes {
            /// <summary>
            /// Indicates that a container/collection accepts only resources
            /// </summary>
            Resource = 1,
            /// <summary>
            /// Indicates that a container/collection accepts only literals
            /// </summary>
            Literal = 2
        };

        /// <summary>
        /// RDFDatatypeCategory represents an enumeration for supported categories of datatype
        /// </summary>
        public enum RDFDatatypeCategory {
            /// <summary>
            /// Value of the typed literal is in the boolean domain
            /// </summary>
            Boolean = 1,
            /// <summary>
            /// Value of the typed literal is in the decimal domain
            /// </summary>
            Numeric = 2,
            /// <summary>
            /// Value of the typed literal is in the datetime domain
            /// </summary>
            DateTime = 3,
            /// <summary>
            /// Value of the typed literal is in the duration domain
            /// </summary>
            TimeSpan = 4,
            /// <summary>
            /// Value of the typed literal is in the string domain
            /// </summary>
            String = 5
        };

        /// <summary>
        /// RDFTermStatus represents an enumeration for supported values of "vs:term_status" triples
        /// </summary>
        public enum RDFTermStatus {
            /// <summary>
            /// stable
            /// </summary>
            Stable = 1,
            /// <summary>
            /// unstable
            /// </summary>
            Unstable = 2,
            /// <summary>
            /// testing
            /// </summary>
            Testing = 3,
            /// <summary>
            /// archaic
            /// </summary>
            Archaic = 4
        };

    }

}