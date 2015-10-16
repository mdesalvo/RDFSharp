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
    public static class RDFQueryUtilities {

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
                     if (RDFModelUtilities.regexLPL.Value.Match(pMember).Success) {
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
                String tLitValue          = pMember.Substring(0, pMember.LastIndexOf("^^", StringComparison.Ordinal));
                String tLitDatatype       = pMember.Substring(pMember.LastIndexOf("^^", StringComparison.Ordinal) + 2);
                RDFDatatype dt            = RDFModelUtilities.GetDatatypeFromString(tLitDatatype);
                RDFTypedLiteral tLit      = new RDFTypedLiteral(tLitValue, dt);
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
                if (right is RDFTypedLiteral && ((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "anyURI"))) {
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

                //PLAIN LITERAL VS "XSD:STRING" TYPED LITERAL
                if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "string"))) {
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
                if (left is RDFTypedLiteral && ((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "anyURI"))) {
                    return String.Compare(((RDFTypedLiteral)left).Value, right.ToString(), StringComparison.Ordinal);
                }

                //TYPED LITERAL VS RESOURCE/CONTEXT
                return 1;

            }

            //TYPED LITERAL VS PLAIN LITERAL
            if (right is RDFPlainLiteral) {

                //"XSD:STRING" TYPED LITERAL VS PLAIN LITERAL
                if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "string"))) {
                    return String.Compare(((RDFTypedLiteral)left).Value, right.ToString(), StringComparison.Ordinal);
                }

                //TYPED LITERAL VS PLAIN LITERAL
                return 1;

            }

            //TYPED LITERAL VS TYPED LITERAL
            //SEMANTICALLY COMPATIBLE CATEGORY
            if (((RDFTypedLiteral)left).Datatype.Category.Equals(((RDFTypedLiteral)right).Datatype.Category)) {
                Int32 comparison = 0;
                switch (((RDFTypedLiteral)left).Datatype.Category) {

                    case RDFModelEnums.RDFDatatypeCategory.Numeric:
                        Decimal leftValueDecimal    = Decimal.Parse(((RDFTypedLiteral)left).Value,  NumberStyles.Number, CultureInfo.InvariantCulture);
                        Decimal rightValueDecimal   = Decimal.Parse(((RDFTypedLiteral)right).Value, NumberStyles.Number, CultureInfo.InvariantCulture);
                        comparison                  = leftValueDecimal.CompareTo(rightValueDecimal);
                        break;

                    case RDFModelEnums.RDFDatatypeCategory.Boolean:
                        Boolean leftValueBoolean    = Boolean.Parse(((RDFTypedLiteral)left).Value);
                        Boolean rightValueBoolean   = Boolean.Parse(((RDFTypedLiteral)right).Value);
                        comparison                  = leftValueBoolean.CompareTo(rightValueBoolean);
                        break;

                    case RDFModelEnums.RDFDatatypeCategory.DateTime:
                        DateTime leftValueDateTime;
                        DateTime rightValueDateTime;

                        //Detect exact type of left typed literal (dateTime, date, time, gYearMonth, gMonthDay, gYear, gMonth, gDay)
                        if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "dateTime"))) {
                            try {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "date"))) {
                            try {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "yyyy-MM-ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "time"))) {
                            try {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "HH:mm:ssK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gYearMonth"))) {
                            try {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "yyyy-MMK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "yyyy-MM", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gMonthDay"))) {
                            try {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "--MM-ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "--MM-dd", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gYear"))) {
                            try {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "yyyyK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "yyyy", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gMonth"))) {
                            try {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "MMK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "MM", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)left).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gDay"))) {
                            try {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                leftValueDateTime  = DateTime.ParseExact(((RDFTypedLiteral)left).Value, "dd", CultureInfo.InvariantCulture);
                            }
                        }
                        else {
                            throw new RDFQueryException("Cannot parse typed literal (" + ((RDFTypedLiteral)left).Value + ") of DateTime category because unknown format detected. Please, switch to one of XSD types: 'dateTime', 'date', 'time', 'gYearMonth', 'gMonthDay', 'gYear', 'gMonth', 'gDay'.");
                        }

                        //Detect exact type of right typed literal (dateTime, date, time, gYearMonth, gMonthDay, gYear, gMonth, gDay)
                        if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "dateTime"))) {
                            try {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "date"))) {
                            try {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "yyyy-MM-ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "time"))) {
                            try {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "HH:mm:ssK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gYearMonth"))) {
                            try {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "yyyy-MMK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "yyyy-MM", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gMonthDay"))) {
                            try {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "--MM-ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "--MM-dd", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gYear"))) {
                            try {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "yyyyK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "yyyy", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gMonth"))) {
                            try {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "MMK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "MM", CultureInfo.InvariantCulture);
                            }
                        }
                        else if (((RDFTypedLiteral)right).Datatype.Equals(RDFDatatypeRegister.GetByPrefixAndDatatype(RDFVocabulary.XSD.PREFIX, "gDay"))) {
                            try {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "ddK", CultureInfo.InvariantCulture);
                            }
                            catch {
                                rightValueDateTime = DateTime.ParseExact(((RDFTypedLiteral)right).Value, "dd", CultureInfo.InvariantCulture);
                            }
                        }
                        else {
                            throw new RDFQueryException("Cannot parse typed literal (" + ((RDFTypedLiteral)right).Value + ") of DateTime category because unknown format detected. Please, switch to one of XSD types: 'dateTime', 'date', 'time', 'gYearMonth', 'gMonthDay', 'gYear', 'gMonth', 'gDay'.");
                        }

                        comparison                  = leftValueDateTime.CompareTo(rightValueDateTime);
                        break;

                    case RDFModelEnums.RDFDatatypeCategory.TimeSpan:
                        TimeSpan leftValueDuration  = XmlConvert.ToTimeSpan(((RDFTypedLiteral)left).Value);
                        TimeSpan rightValueDuration = XmlConvert.ToTimeSpan(((RDFTypedLiteral)right).Value);
                        comparison                  = leftValueDuration.CompareTo(rightValueDuration);
                        break;

                    case RDFModelEnums.RDFDatatypeCategory.String:
                        String leftValueString      = ((RDFTypedLiteral)left).Value;
                        String rightValueString     = ((RDFTypedLiteral)right).Value;
                        comparison                  = String.Compare(leftValueString, rightValueString, StringComparison.Ordinal);
                        break;

                }
                return comparison;
            }

            //SEMANTICALLY NOT COMPATIBLE CATEGORY
            throw new RDFQueryException("Type Error: Typed Literal of category (" + ((RDFTypedLiteral)left).Datatype.Category + ") cannot be compared to Typed Literal of category (" + ((RDFTypedLiteral)right).Datatype.Category + ")");

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
                return "\"" + ((RDFTypedLiteral)patternMember).Value + "\"^^<" + ((RDFTypedLiteral)patternMember).Datatype + ">";
                #endregion

                #endregion

            }
            throw new RDFQueryException("Cannot print pattern member because given \"patternMember\" parameter is null.");

        }
        #endregion

        #region EVENTS
        /// <summary>
        /// Event raised during RDF query management to signal a warning
        /// </summary>
        public static event RDFQueryEventHandler OnQueryWarning;

        /// <summary>
        /// Delegate to handle warning events generated during RDF query management
        /// </summary>
        public delegate void RDFQueryEventHandler(String queryEventMessage);
        #endregion

    }

}