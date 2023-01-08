/*
   Copyright 2012-2022 Marco De Salvo

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
    /// RDFTypedLiteral represents a literal decorated with an XML Schema datatype.
    /// </summary>
    public class RDFTypedLiteral : RDFLiteral
    {
        #region Properties
        /// <summary>
        /// Mandatory datatype of the typed literal
        /// </summary>
        public RDFModelEnums.RDFDatatypes Datatype { get; internal set; }

        /// <summary>
        /// Represents an handy typed literal for boolean True
        /// </summary>
        public static readonly RDFTypedLiteral True = new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);

        /// <summary>
        /// Represents an handy typed literal for boolean False
        /// </summary>
        public static readonly RDFTypedLiteral False = new RDFTypedLiteral("false", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a typed literal with given value and given datatype
        /// (semantic validation of given value against given datatype is performed).
        /// </summary>
        public RDFTypedLiteral(string value, RDFModelEnums.RDFDatatypes datatype)
        {
            this.Value = value ?? string.Empty;
            this.Datatype = datatype;

            //Validation against semantic of given datatype
            if (!RDFModelUtilities.ValidateTypedLiteral(this))
                throw new RDFModelException("Cannot create RDFTypedLiteral because given \"value\" parameter (" + value + ") is not well-formed against given \"datatype\" parameter (" + RDFModelUtilities.GetDatatypeFromEnum(datatype) + ")");
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the typed literal
        /// </summary>
        public override string ToString()
            => string.Concat(base.ToString(), "^^", RDFModelUtilities.GetDatatypeFromEnum(this.Datatype));
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with boolean
        /// </summary>
        internal bool HasBooleanDatatype()
            => this.Datatype == RDFModelEnums.RDFDatatypes.XSD_BOOLEAN;

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with datetime
        /// </summary>
        internal bool HasDatetimeDatatype()
        {
            bool hasDatetimeDatatype = false;
            switch (this.Datatype)
            {
                case RDFModelEnums.RDFDatatypes.XSD_DATE:
                case RDFModelEnums.RDFDatatypes.XSD_DATETIME:
                case RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP:
                case RDFModelEnums.RDFDatatypes.XSD_GDAY:
                case RDFModelEnums.RDFDatatypes.XSD_GMONTH:
                case RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY:
                case RDFModelEnums.RDFDatatypes.XSD_GYEAR:
                case RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH:
                case RDFModelEnums.RDFDatatypes.XSD_TIME:
                    hasDatetimeDatatype = true;
                    break;
            }
            return hasDatetimeDatatype;
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with timespan
        /// </summary>
        internal bool HasTimespanDatatype()
            => this.Datatype == RDFModelEnums.RDFDatatypes.XSD_DURATION;

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with string
        /// </summary>
        internal bool HasStringDatatype()
        {
            bool hasStringDatatype = false;
            switch (this.Datatype)
            {
                case RDFModelEnums.RDFDatatypes.RDFS_LITERAL:
                case RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL:
                case RDFModelEnums.RDFDatatypes.RDF_HTML:
                case RDFModelEnums.RDFDatatypes.RDF_JSON:
                case RDFModelEnums.RDFDatatypes.XSD_ANYURI:
                case RDFModelEnums.RDFDatatypes.XSD_ID:
                case RDFModelEnums.RDFDatatypes.XSD_LANGUAGE:
                case RDFModelEnums.RDFDatatypes.XSD_NAME:
                case RDFModelEnums.RDFDatatypes.XSD_NCNAME:
                case RDFModelEnums.RDFDatatypes.XSD_NMTOKEN:
                case RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING:
                case RDFModelEnums.RDFDatatypes.XSD_NOTATION:
                case RDFModelEnums.RDFDatatypes.XSD_QNAME:
                case RDFModelEnums.RDFDatatypes.XSD_STRING:
                case RDFModelEnums.RDFDatatypes.XSD_TOKEN:
                case RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY:
                case RDFModelEnums.RDFDatatypes.XSD_HEXBINARY:
                    hasStringDatatype = true;
                    break;
            }
            return hasStringDatatype;
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with decimal
        /// </summary>
        internal bool HasDecimalDatatype()
        {
            bool hasDecimalDatatype = false;
            switch (this.Datatype)
            {
                case RDFModelEnums.RDFDatatypes.XSD_DECIMAL:
                case RDFModelEnums.RDFDatatypes.XSD_DOUBLE:
                case RDFModelEnums.RDFDatatypes.XSD_FLOAT:
                case RDFModelEnums.RDFDatatypes.XSD_INTEGER:
                case RDFModelEnums.RDFDatatypes.XSD_LONG:
                case RDFModelEnums.RDFDatatypes.XSD_INT:
                case RDFModelEnums.RDFDatatypes.XSD_SHORT:
                case RDFModelEnums.RDFDatatypes.XSD_BYTE:
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG:
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT:
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT:
                case RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE:
                case RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER:
                case RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER:
                case RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER:
                case RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER:
                    hasDecimalDatatype = true;
                    break;
            }
            return hasDecimalDatatype;
        }
        #endregion
    }
}