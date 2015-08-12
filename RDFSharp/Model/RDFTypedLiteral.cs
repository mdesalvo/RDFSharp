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
    /// RDFTypedLiteral represents a RDFLiteral which is denoted by a datatype.
    /// </summary>
    public class RDFTypedLiteral: RDFLiteral {

        #region Properties
        /// <summary>
        /// Mandatory datatype of the typed literal
        /// </summary>
        public RDFDatatype Datatype { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default ctor to build a typed literal with value and default "xsd:string" datatype
        /// </summary>
        public RDFTypedLiteral(String value) {
            this.Value               = (value ?? String.Empty);
            this.Datatype            = RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "string");   
            this.PatternMemberID     = RDFModelUtilities.CreateHash(this.ToString());
            if (!RDFModelUtilities.ValidateTypedLiteral(this)) {
                throw new RDFModelException("Cannot create RDFTypedLiteral because given \"value\" parameter (" + value + ") is not well-formed or not compatible with the category (" + this.Datatype.Category + ") of the datatype.");
            }
        }

        /// <summary>
        /// Default ctor to build a typed literal with value and datatype
        /// </summary>
        public RDFTypedLiteral(String value, RDFDatatype datatype)  {
            if (datatype            != null) {
			    this.Value           = (value ?? String.Empty);
                this.Datatype        = datatype;
                this.PatternMemberID = RDFModelUtilities.CreateHash(this.ToString());
                if (!RDFModelUtilities.ValidateTypedLiteral(this)) {
                    throw new RDFModelException("Cannot create RDFTypedLiteral because given \"value\" parameter (" + value + ") is not well-formed or not compatible with the category (" + this.Datatype.Category + ") of the datatype.");
                }
            }
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the typed literal
        /// </summary>
        public override String ToString() {
            return base.ToString() + "^^" + this.Datatype;
        }
        #endregion

    }

}