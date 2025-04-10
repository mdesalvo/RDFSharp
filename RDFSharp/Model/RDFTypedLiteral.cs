/*
   Copyright 2012-2025 Marco De Salvo

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
    public sealed class RDFTypedLiteral : RDFLiteral
    {
        #region Statics
        /// <summary>
        /// Represents an handy typed literal for boolean True
        /// </summary>
        public static readonly RDFTypedLiteral True = new RDFTypedLiteral("true", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);
        /// <summary>
        /// Represents an handy typed literal for boolean False
        /// </summary>
        public static readonly RDFTypedLiteral False = new RDFTypedLiteral("false", RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);
        /// <summary>
        /// Represents an handy typed literal for integer Zero
        /// </summary>
        public static readonly RDFTypedLiteral Zero = new RDFTypedLiteral("0", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
        /// <summary>
        /// Represents an handy typed literal for integer One
        /// </summary>
        public static readonly RDFTypedLiteral One = new RDFTypedLiteral("1", RDFModelEnums.RDFDatatypes.XSD_INTEGER);
        #endregion

        #region Properties
        /// <summary>
        /// Datatype of the literal's value
        /// </summary>
        public RDFDatatype Datatype { get; internal set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Default-ctor to build a typed literal with given value and given standard datatype
        /// </summary>
        public RDFTypedLiteral(string value, RDFModelEnums.RDFDatatypes datatype)
            : this(value, RDFDatatypeRegister.GetDatatype(datatype)) { }

        /// <summary>
        /// Default-ctor to build a typed literal with given value and given custom datatype (rdfs:Literal in case null)
        /// </summary>
        public RDFTypedLiteral(string value, RDFDatatype datatype)
        {
            Datatype = datatype ?? RDFDatatypeRegister.RDFSLiteral;

            //Validation against semantic of given datatype
            (bool,string) validationResult = Datatype.Validate(value ?? string.Empty);
            if (!validationResult.Item1)
                throw new RDFModelException("Cannot create RDFTypedLiteral because given \"value\" parameter (" + value + ") is not well-formed against given \"datatype\" parameter (" + Datatype + ") which is based on \"" + Datatype.TargetDatatype + "\" ");

            Value = validationResult.Item2;
        }
        #endregion

        #region Interfaces
        /// <summary>
        /// Gives the string representation of the typed literal
        /// </summary>
        public override string ToString()
            => string.Concat(base.ToString(), "^^", Datatype.ToString());
        #endregion

        #region Methods
        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with boolean
        /// </summary>
        public bool HasBooleanDatatype()
            => Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_BOOLEAN;

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with datetime
        /// </summary>
        public bool HasDatetimeDatatype()
        {
            switch (Datatype.TargetDatatype)
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
                case RDFModelEnums.RDFDatatypes.TIME_GENERALDAY:
                case RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH:
                case RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR:
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with timespan
        /// </summary>
        public bool HasTimespanDatatype()
            => Datatype.TargetDatatype == RDFModelEnums.RDFDatatypes.XSD_DURATION;

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with string
        /// </summary>
        public bool HasStringDatatype()
        {
            switch (Datatype.TargetDatatype)
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
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with geosparql
        /// </summary>
        public bool HasGeographicDatatype()
        {
            switch (Datatype.TargetDatatype)
            {
                case RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT:
                case RDFModelEnums.RDFDatatypes.GEOSPARQL_GML:
                    return true;
                default: return false;
            }
        }

        /// <summary>
        /// Checks if the datatype of this typed literal is compatible with decimal
        /// </summary>
        public bool HasDecimalDatatype()
        {
            switch (Datatype.TargetDatatype)
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
                case RDFModelEnums.RDFDatatypes.OWL_REAL:
                case RDFModelEnums.RDFDatatypes.OWL_RATIONAL:
                    return true;
                default: return false;
            }
        }
        #endregion
    }
}