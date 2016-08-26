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

namespace RDFSharp.Model
{

    /// <summary>
    /// RDFTypedLiteral represents a RDFLiteral which is denoted by a Datatype.
    /// </summary>
    public class RDFTypedLiteral: RDFLiteral {

        #region Properties
        /// <summary>
        /// Mandatory datatype of the typed literal
        /// </summary>
        public RDFModelEnums.RDFDatatype Datatype { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a typed literal with given value and given datatype. 
        /// Semantic validation of given value against given datatype is performed.
        /// </summary>
        public RDFTypedLiteral(String value, RDFModelEnums.RDFDatatype datatype)  {
            this.Value    = (value ?? String.Empty);
            this.Datatype = datatype;
            if (RDFModelUtilities.ValidateTypedLiteral(this)) {
                this.PatternMemberID = RDFModelUtilities.CreateHash(this.ToString());
            }
            else {
                throw new RDFModelException("Cannot create RDFTypedLiteral because given \"value\" parameter (" + value + ") is not well-formed against given \"datatype\" parameter (" + RDFModelUtilities.GetDatatypeFromEnum(datatype) + ")");
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the typed literal
        /// </summary>
        public override String ToString() {
            return base.ToString() + "^^" + RDFModelUtilities.GetDatatypeFromEnum(this.Datatype);
        }
        #endregion

    }

}