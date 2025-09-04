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

namespace RDFSharp.Model;

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
    /// Builds a typed literal with given value and given standard datatype
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public RDFTypedLiteral(string value, RDFModelEnums.RDFDatatypes datatype)
        : this(value, RDFDatatypeRegister.GetDatatype(datatype)) { }

    /// <summary>
    /// Builds a typed literal with given value and given custom datatype (rdfs:Literal in case null)
    /// </summary>
    /// <exception cref="RDFModelException"></exception>
    public RDFTypedLiteral(string value, RDFDatatype datatype)
    {
        Datatype = datatype ?? RDFDatatypeRegister.RDFSLiteral;

        //Validation against semantic of given datatype
        (bool,string) validationResult = Datatype.Validate(value ?? string.Empty);
        if (!validationResult.Item1)
            throw new RDFModelException($"Cannot create RDFTypedLiteral because given \"value\" parameter ({value}) is not well-formed against given \"datatype\" parameter ({Datatype}) which is based on \"{Datatype.TargetDatatype}\" standard datatype");

        Value = validationResult.Item2;
    }
    #endregion

    #region Interfaces
    /// <summary>
    /// Gives the string representation of the typed literal
    /// </summary>
    public override string ToString()
        => $"{base.ToString()}^^{Datatype}";
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
        return Datatype.TargetDatatype switch
        {
            RDFModelEnums.RDFDatatypes.XSD_DATE or RDFModelEnums.RDFDatatypes.XSD_DATETIME or RDFModelEnums.RDFDatatypes.XSD_DATETIMESTAMP or RDFModelEnums.RDFDatatypes.XSD_GDAY or RDFModelEnums.RDFDatatypes.XSD_GMONTH or RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY or RDFModelEnums.RDFDatatypes.XSD_GYEAR or RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH or RDFModelEnums.RDFDatatypes.XSD_TIME or RDFModelEnums.RDFDatatypes.TIME_GENERALDAY or RDFModelEnums.RDFDatatypes.TIME_GENERALMONTH or RDFModelEnums.RDFDatatypes.TIME_GENERALYEAR => true,
            _ => false
        };
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
        return Datatype.TargetDatatype switch
        {
            RDFModelEnums.RDFDatatypes.RDFS_LITERAL or RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL or RDFModelEnums.RDFDatatypes.RDF_HTML or RDFModelEnums.RDFDatatypes.RDF_JSON or RDFModelEnums.RDFDatatypes.XSD_ANYURI or RDFModelEnums.RDFDatatypes.XSD_ID or RDFModelEnums.RDFDatatypes.XSD_LANGUAGE or RDFModelEnums.RDFDatatypes.XSD_NAME or RDFModelEnums.RDFDatatypes.XSD_NCNAME or RDFModelEnums.RDFDatatypes.XSD_NMTOKEN or RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING or RDFModelEnums.RDFDatatypes.XSD_NOTATION or RDFModelEnums.RDFDatatypes.XSD_QNAME or RDFModelEnums.RDFDatatypes.XSD_STRING or RDFModelEnums.RDFDatatypes.XSD_TOKEN or RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY or RDFModelEnums.RDFDatatypes.XSD_HEXBINARY => true,
            _ => false
        };
    }

    /// <summary>
    /// Checks if the datatype of this typed literal is compatible with geosparql
    /// </summary>
    public bool HasGeographicDatatype()
    {
        return Datatype.TargetDatatype switch
        {
            RDFModelEnums.RDFDatatypes.GEOSPARQL_WKT or RDFModelEnums.RDFDatatypes.GEOSPARQL_GML => true,
            _ => false
        };
    }

    /// <summary>
    /// Checks if the datatype of this typed literal is compatible with decimal
    /// </summary>
    public bool HasDecimalDatatype()
    {
        return Datatype.TargetDatatype switch
        {
            RDFModelEnums.RDFDatatypes.XSD_DECIMAL or RDFModelEnums.RDFDatatypes.XSD_DOUBLE or RDFModelEnums.RDFDatatypes.XSD_FLOAT or RDFModelEnums.RDFDatatypes.XSD_INTEGER or RDFModelEnums.RDFDatatypes.XSD_LONG or RDFModelEnums.RDFDatatypes.XSD_INT or RDFModelEnums.RDFDatatypes.XSD_SHORT or RDFModelEnums.RDFDatatypes.XSD_BYTE or RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG or RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT or RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT or RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE or RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER or RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER or RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER or RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER or RDFModelEnums.RDFDatatypes.OWL_REAL or RDFModelEnums.RDFDatatypes.OWL_RATIONAL => true,
            _ => false
        };
    }
    #endregion
}