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

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFDatatype represents a generic datatype in the RDF model.
    /// </summary>
    public class RDFDatatype: RDFNamespace, IEquatable<RDFDatatype> {

        #region Properties
        /// <summary>
        /// Unique representation of the datatype
        /// </summary>
        internal Int64 DatatypeID { get; set; }

        /// <summary>
        /// String representing the type of data
        /// </summary>
        public String Datatype { get; internal set; }

        /// <summary>
        /// Category of the datatype
        /// </summary>
        public RDFModelEnums.RDFDatatypeCategory Category { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// String-based ctor to build a datatype of the given category
        /// </summary>
        public RDFDatatype(String prefix, String nSpace, String datatype, RDFModelEnums.RDFDatatypeCategory category): base(prefix, nSpace) {
            if (datatype       != null && datatype.Trim() != String.Empty) {
                this.Datatype   = datatype;
                this.Category   = category;
                this.DatatypeID = RDFModelUtilities.CreateHash(this.ToString());
            }
            else {
                throw new RDFModelException("Cannot create RDFDatatype because \"datatype\" parameter is null or empty");
            }            
        }

        /// <summary>
        /// Uri-based ctor to build a datatype of the given category
        /// </summary>
        public RDFDatatype(String prefix, Uri nSpace, String datatype, RDFModelEnums.RDFDatatypeCategory category): base(prefix, nSpace) {
            if (datatype       != null && datatype.Trim() != String.Empty) {
                this.Datatype   = datatype;
                this.Category   = category;
                this.DatatypeID = RDFModelUtilities.CreateHash(this.ToString());
            }
            else {
                throw new RDFModelException("Cannot create RDFDatatype because \"datatype\" parameter is null or empty");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the datatype
        /// </summary>
        public override String ToString() {
            return base.ToString() + this.Datatype;
        }

        /// <summary>
        /// Performs the equality comparison between two datatypes
        /// </summary>
        public Boolean Equals(RDFDatatype other) {
            return (other != null && this.DatatypeID.Equals(other.DatatypeID));
        }
        #endregion

    }

}