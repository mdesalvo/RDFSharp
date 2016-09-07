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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;
using RDFSharp.Model;
using RDFSharp.Store;

namespace RDFSharp.Query
{

    /// <summary>
    /// RDFQueryUtilities is a collector of reusable utility methods for RDF query management
    /// </summary>
    internal static class RDFQueryUtilities {

        #region ADO.NET
        /// <summary>
        /// Adds a new column to the given table, avoiding duplicates 
        /// </summary>
        internal static void AddColumn(DataTable table, String columnName) {
            if (!table.Columns.Contains(columnName.Trim().ToUpperInvariant())) {
                 table.Columns.Add(columnName.Trim().ToUpperInvariant(), Type.GetType("System.String"));
            }
        }

        /// <summary>
        /// Adds a new row to the given table 
        /// </summary>
        internal static void AddRow(DataTable table, Dictionary<String, String> bindings) {
            Boolean rowAdded     = false;
            DataRow resultRow    = table.NewRow();
            bindings.Keys.ToList<String>().ForEach(k => {
                if (table.Columns.Contains(k)) {
                    resultRow[k] = bindings[k];
                    rowAdded     = true;
                }
            });
            if (rowAdded) {
                table.Rows.Add(resultRow);
            }
        }
        #endregion

		#region MIRELLA ENGINE
        /// <summary>
        /// Parses the given string to return an instance of pattern member
        /// </summary>
        internal static RDFPatternMember ParseRDFPatternMember(String pMember) {

            if (pMember != null) { 

                #region Uri
                Uri testUri;
                if (Uri.TryCreate(pMember, UriKind.Absolute, out testUri)) {
                    return new RDFResource(pMember);
                }
                #endregion

                #region Plain Literal
                if (!pMember.Contains("^^") || 
                     pMember.EndsWith("^^") ||
                     RDFModelUtilities.GetUriFromString(pMember.Substring(pMember.LastIndexOf("^^", StringComparison.Ordinal) + 2)) == null) {
                     RDFPlainLiteral pLit = null;
                     if (RDFNTriples.regexLPL.Match(pMember).Success) {
                         String pLitVal   = pMember.Substring(0, pMember.LastIndexOf("@", StringComparison.Ordinal));
                         String pLitLng   = pMember.Substring(pMember.LastIndexOf("@", StringComparison.Ordinal) + 1);
                         pLit             = new RDFPlainLiteral(pLitVal, pLitLng);
                     }
                     else {
                        pLit              = new RDFPlainLiteral(pMember);
                     }
                     return pLit;
                }
                #endregion

                #region Typed Literal
                String tLitValue             = pMember.Substring(0, pMember.LastIndexOf("^^", StringComparison.Ordinal));
                String tLitDatatype          = pMember.Substring(pMember.LastIndexOf("^^", StringComparison.Ordinal) + 2);
                RDFModelEnums.RDFDatatypes dt = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                RDFTypedLiteral tLit         = new RDFTypedLiteral(tLitValue, dt);
                return tLit;
                #endregion

            }
            throw new RDFQueryException("Cannot parse pattern member because given \"pMember\" parameter is null.");

        }

        /// <summary>
        /// Compares the given pattern members, throwing a "Type Error" whenever the comparison operator detects sematically incompatible members;
        /// </summary>
        internal static Int32 CompareRDFPatternMembers(RDFPatternMember left, RDFPatternMember right) {

            #region CornerCase Comparisons
            if (left      == null) {
                if (right == null) {
                    return  0;
                }
                return -1;
            }
            if (right     == null) {
                return 1;
            }
            #endregion

            #region Effective  Comparisons

            #region RESOURCE/CONTEXT
            if (left is RDFResource      || left is RDFContext) {

                //RESOURCE/CONTEXT VS RESOURCE/CONTEXT
                if (right is RDFResource || right is RDFContext) {
                    return String.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }

                //RESOURCE/CONTEXT VS "XSD:ANYURI" TYPED LITERAL
                if (right is RDFTypedLiteral && ((RDFTypedLiteral)right).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ANYURI)) {
                    return String.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                }

                //RESOURCE/CONTEXT VS LITERAL
                return -1;

            }
            #endregion

            #region PLAINLITERAL
            if (left is RDFPlainLiteral) {

                //PLAIN LITERAL VS RESOURCE/CONTEXT
                if (right is RDFResource || right is RDFContext) {
                    return 1;
                }

                //PLAIN LITERAL VS PLAIN LITERAL
                if (right is RDFPlainLiteral) {
                    return String.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
                }

                //PLAIN LITERAL VS "RDFS:LITERAL" OR "XSD:STRING" TYPED LITERAL
                if (((RDFTypedLiteral)right).Datatype.Equals(RDFModelEnums.RDFDatatypes.RDFS_LITERAL) ||
                    ((RDFTypedLiteral)right).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))   {
                    return String.Compare(left.ToString(), ((RDFTypedLiteral)right).Value, StringComparison.Ordinal);
                }

                //PLAIN LITERAL VS TYPED LITERAL
                return -1;

            }
            #endregion

            #region TYPEDLITERAL
            //TYPED LITERAL VS RESOURCE/CONTEXT
            if (right is RDFResource || right is RDFContext) {

                //"XSD:ANYURI" TYPED LITERAL VS RESOURCE/CONTEXT
                if (left is RDFTypedLiteral && ((RDFTypedLiteral)left).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_ANYURI)) {
                    return String.Compare(((RDFTypedLiteral)left).Value, right.ToString(), StringComparison.Ordinal);
                }

                //TYPED LITERAL VS RESOURCE/CONTEXT
                return 1;

            }

            //TYPED LITERAL VS PLAIN LITERAL
            if (right is RDFPlainLiteral) {

                //"RDFS:LITERAL" OR "XSD:STRING" TYPED LITERAL VS PLAIN LITERAL
                if (((RDFTypedLiteral)left).Datatype.Equals(RDFModelEnums.RDFDatatypes.RDFS_LITERAL) ||
                    ((RDFTypedLiteral)left).Datatype.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING))   {
                    return String.Compare(((RDFTypedLiteral)left).Value, right.ToString(), StringComparison.Ordinal);
                }

                //TYPED LITERAL VS PLAIN LITERAL
                return 1;

            }

            //TYPED LITERAL VS TYPED LITERAL
            //Detect left typed literal's category
            RDFModelEnums.RDFDatatypes leftDType  = ((RDFTypedLiteral)left).Datatype;
            Boolean isLeftDTypeBoolean           = leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);
            Boolean isLeftDTypeNumeric           = leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_BYTE)               ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DECIMAL)            ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DOUBLE)             ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_FLOAT)              ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_INT)                ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER)            ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_LONG)               ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)    ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER) ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER) ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)    ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_SHORT)              ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)       ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)        ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)       ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT);
            Boolean isLeftDTypeDateTime          = leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DATE)               ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DATETIME)           ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GDAY)               ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTH)             ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)          ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEAR)              ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)         ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_TIME);
            Boolean isLeftDTypeTimeSpan          = leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DURATION);
            Boolean isLeftDTypeString            = leftDType.Equals(RDFModelEnums.RDFDatatypes.RDFS_LITERAL)           ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)         ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_ANYURI)             ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)       ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)          ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)           ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NAME)               ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NCNAME)             ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_ID)                 ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)            ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)   ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NOTATION)           ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_QNAME)              ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING)             ||
                                                   leftDType.Equals(RDFModelEnums.RDFDatatypes.XSD_TOKEN);

            //Detect right typed literal's category
            RDFModelEnums.RDFDatatypes rightDType = ((RDFTypedLiteral)right).Datatype;
            Boolean isRightDTypeBoolean          = rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_BOOLEAN);
            Boolean isRightDTypeNumeric          = rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_BYTE)               ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DECIMAL)            ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DOUBLE)             ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_FLOAT)              ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_INT)                ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_INTEGER)            ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_LONG)               ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NEGATIVEINTEGER)    ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NONNEGATIVEINTEGER) ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NONPOSITIVEINTEGER) ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_POSITIVEINTEGER)    ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_SHORT)              ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDBYTE)       ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDINT)        ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDLONG)       ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_UNSIGNEDSHORT);
            Boolean isRightDTypeDateTime         = rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DATE)               ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DATETIME)           ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GDAY)               ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTH)             ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GMONTHDAY)          ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEAR)              ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_GYEARMONTH)         ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_TIME);
            Boolean isRightDTypeTimeSpan         = rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_DURATION);
            Boolean isRightDTypeString           = rightDType.Equals(RDFModelEnums.RDFDatatypes.RDFS_LITERAL)           ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.RDF_XMLLITERAL)         ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_ANYURI)             ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_BASE64BINARY)       ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_HEXBINARY)          ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_LANGUAGE)           ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NAME)               ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NCNAME)             ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_ID)                 ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NMTOKEN)            ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NORMALIZEDSTRING)   ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_NOTATION)           ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_QNAME)              ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_STRING)             ||
                                                   rightDType.Equals(RDFModelEnums.RDFDatatypes.XSD_TOKEN);

            //Compare typed literals, only if categories are semantically compatible
            if (isLeftDTypeBoolean         && isRightDTypeBoolean)  {
                Boolean leftValueBoolean    = Boolean.Parse(((RDFTypedLiteral)left).Value);
                Boolean rightValueBoolean   = Boolean.Parse(((RDFTypedLiteral)right).Value);
                return leftValueBoolean.CompareTo(rightValueBoolean);
            }
            else if(isLeftDTypeDateTime    && isRightDTypeDateTime) {
                DateTime leftValueDateTime  = DateTime.Parse(((RDFTypedLiteral)left).Value,  CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                DateTime rightValueDateTime = DateTime.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                return leftValueDateTime.CompareTo(rightValueDateTime);
            }
            else if(isLeftDTypeNumeric     && isRightDTypeNumeric)  {
                Decimal leftValueDecimal    = Decimal.Parse(((RDFTypedLiteral)left).Value,  CultureInfo.InvariantCulture);
                Decimal rightValueDecimal   = Decimal.Parse(((RDFTypedLiteral)right).Value, CultureInfo.InvariantCulture);
                return leftValueDecimal.CompareTo(rightValueDecimal);
            }
            else if(isLeftDTypeString      && isRightDTypeString)   {
                String leftValueString      = ((RDFTypedLiteral)left).Value;
                String rightValueString     = ((RDFTypedLiteral)right).Value;
                return leftValueString.CompareTo(rightValueString);
            }
            else if(isLeftDTypeTimeSpan    && isRightDTypeTimeSpan) {
                TimeSpan leftValueDuration  = XmlConvert.ToTimeSpan(((RDFTypedLiteral)left).Value);
                TimeSpan rightValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)right).Value);
                return leftValueDuration.CompareTo(rightValueDuration);
            }
            else {
               throw new RDFQueryException("Type Error: Typed Literal of datatype (" + RDFModelUtilities.GetDatatypeFromEnum(leftDType) + ") cannot be compared to Typed Literal of datatype (" + RDFModelUtilities.GetDatatypeFromEnum(rightDType) + ")");
            }           
            #endregion

            #endregion

        }

        /// <summary>
        /// Gives a formatted string representation of the given pattern member
        /// </summary>
        internal static String PrintRDFPatternMember(RDFPatternMember patternMember) {

            if (patternMember != null) { 

                #region Variable
                if (patternMember is RDFVariable) {
                    return patternMember.ToString();
                }
                #endregion

                #region Non-Variable

                #region Resource/Context
                if (patternMember is RDFResource || patternMember is RDFContext) {
                    if (patternMember is RDFResource && ((RDFResource)patternMember).IsBlank) {
                        return patternMember.ToString();
                    }
                    return "<" + patternMember + ">";
                }
                #endregion

                #region Literal
                if (patternMember is RDFPlainLiteral) {
                    if (((RDFPlainLiteral)patternMember).Language != String.Empty) {
                        return "\"" + ((RDFPlainLiteral)patternMember).Value + "\"@" + ((RDFPlainLiteral)patternMember).Language;
                    }
                    return "\"" + ((RDFPlainLiteral)patternMember).Value + "\"";
                }
                return "\"" + ((RDFTypedLiteral)patternMember).Value + "\"^^<" + RDFModelUtilities.GetDatatypeFromEnum(((RDFTypedLiteral)patternMember).Datatype) + ">";
                #endregion

                #endregion

            }
            throw new RDFQueryException("Cannot print pattern member because given \"patternMember\" parameter is null.");

        }
        #endregion

    }

}