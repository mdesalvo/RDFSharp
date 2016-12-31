/*
   Copyright 2012-2017 Marco De Salvo

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
        public RDFModelEnums.RDFDatatypes Datatype { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a typed literal with given value and given datatype. 
        /// Semantic validation of given value against given datatype is performed.
        /// </summary>
        public RDFTypedLiteral(String value, RDFModelEnums.RDFDatatypes datatype)  {
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

        #region Methods
        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with boolean
        /// </summary>
        internal Boolean HasBooleanDatatype() {
            return (this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BOOLEAN));
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with datetime
        /// </summary>
        internal Boolean HasDatetimeDatatype() {
            return (this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DATE)       ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DATETIME)   ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GDAY)       ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTH)     ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)  ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEAR)      ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH) ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_TIME));
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with timespan
        /// </summary>
        internal Boolean HasTimespanDatatype() {
            return (this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DURATION));
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with string
        /// </summary>
        internal Boolean HasStringDatatype() {
            return (this.Datatype.Equals(RDFModelEnums.RDFDatatypes.RDFS_LITERAL)         ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)       ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ANYURI)           ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ID)               ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)         ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NAME)             ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NCNAME)           ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)          ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING) ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NOTATION)         ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_QNAME)            ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING)           ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_TOKEN)            ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)     ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_HEXBINARY));
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with decimal
        /// </summary>
        internal Boolean HasDecimalDatatype() {
            return (this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_BYTE)               ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DECIMAL)            ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_DOUBLE)             ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_FLOAT)              ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INT)                ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER)            ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_LONG)               ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)    ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER) ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER) ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)    ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_SHORT)              ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)       ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)        ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)       ||
                    this.Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT));
        }
        #endregion

    }

}